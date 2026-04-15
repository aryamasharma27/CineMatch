// ============================================================
//  DataService.cs  —  Every SQL query lives here
//
//  BACKEND EXPLANATION:
//  This is the "Data Access Layer" (DAL).  The UI forms never
//  write SQL themselves — they call methods here.
//  That separation is called "Separation of Concerns".
//
//  KEY ADO.NET CONCEPTS USED:
//
//  1. MySqlCommand + ExecuteReader  — "Connected architecture"
//     The connection stays open while we read row by row.
//     Good for streaming large result sets.
//
//  2. MySqlDataAdapter + DataTable  — "Disconnected architecture"
//     The adapter fetches ALL rows into a DataTable in memory,
//     then the connection closes.  We can bind the DataTable
//     directly to a DataGridView.
//
//  3. Parameters (@name)  — prevent SQL Injection
//     We NEVER concatenate user input into SQL strings.
//     cmd.Parameters.AddWithValue("@username", value) is safe.
//
//  4. using() blocks  — automatic resource cleanup
//     When the using block ends C# calls .Dispose() on the
//     connection/command even if an exception is thrown.
// ============================================================
using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace CineMatch
{
    public static class DataService
    {
        // ════════════════════════════════════════════════════
        //  AUTH
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Login: SELECT user WHERE username=? AND password=?
        /// Returns the User object or null if credentials are wrong.
        /// </summary>
        public static User Login(string username, string password)
        {
            // "using" ensures the connection is closed when the block exits
            using (var conn = DB.GetConnection())
            // MySqlCommand wraps a SQL string + the connection
            using (var cmd = new MySqlCommand(
                "SELECT * FROM Users WHERE username = @u AND password = @p", conn))
            {
                // Parameters replace @u and @p safely — no SQL injection possible
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                // ExecuteReader runs SELECT and returns a cursor
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())           // .Read() advances to the next row; returns false if no rows
                        return MapUser(rdr);  // convert the current row into a User object
                    return null;              // wrong credentials
                }
            }
        }

        /// <summary>
        /// Register: INSERT a new user row.
        /// Returns the new user's auto-generated ID.
        /// </summary>
        public static int Register(string fullName, string email,
                                   string username, string password,
                                   DateTime dob, string favGenre)
        {
            using (var conn = DB.GetConnection())
            using (var cmd = new MySqlCommand(@"
                INSERT INTO Users (full_name, email, username, password, dob, fav_genre)
                VALUES (@fn, @em, @un, @pw, @dob, @fg)", conn))
            {
                cmd.Parameters.AddWithValue("@fn", fullName);
                cmd.Parameters.AddWithValue("@em", email);
                cmd.Parameters.AddWithValue("@un", username);
                cmd.Parameters.AddWithValue("@pw", password);
                cmd.Parameters.AddWithValue("@dob", dob.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@fg", string.IsNullOrEmpty(favGenre) ? DBNull.Value : (object)favGenre);

                // ExecuteNonQuery runs INSERT/UPDATE/DELETE; returns rows affected
                cmd.ExecuteNonQuery();

                // LastInsertedId is the AUTO_INCREMENT value MySQL assigned
                return (int)cmd.LastInsertedId;
            }
        }

        // ════════════════════════════════════════════════════
        //  MOVIES
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Returns all movies, optionally filtered and sorted.
        /// This demonstrates building a dynamic SQL query safely.
        /// </summary>
        public static List<Movie> GetMovies(string search = "", string genre = "",
                                            string mood = "", string sort = "rating_avg",
                                            int userId = 0)
        {
            // Build WHERE clause dynamically
            var where = new System.Text.StringBuilder("WHERE 1=1");
            if (!string.IsNullOrWhiteSpace(search))
                where.Append(" AND (m.title LIKE @search OR m.director LIKE @search)");
            if (!string.IsNullOrWhiteSpace(genre))
                where.Append(" AND FIND_IN_SET(@genre, REPLACE(m.genres,', ',',')) > 0");
            if (!string.IsNullOrWhiteSpace(mood))
                where.Append(" AND FIND_IN_SET(@mood, REPLACE(m.moods,', ',',')) > 0");

            // Whitelist sort columns to prevent injection
            string orderCol = sort == "year" ? "m.release_year DESC"
                            : sort == "title" ? "m.title ASC"
                            : sort == "trending" ? "m.trending DESC"
                            : "m.rating_avg DESC";

            // LEFT JOIN brings in the current user's rating (NULL if not rated)
            string sql = $@"
                SELECT m.*,
                       COALESCE(r.stars, 0) AS user_rating
                FROM Movies m
                LEFT JOIN Ratings r ON r.movie_id = m.movie_id AND r.user_id = @uid
                {where}
                ORDER BY {orderCol}";

            var movies = new List<Movie>();

            using (var conn = DB.GetConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                if (!string.IsNullOrWhiteSpace(search))
                    cmd.Parameters.AddWithValue("@search", $"%{search}%");
                if (!string.IsNullOrWhiteSpace(genre))
                    cmd.Parameters.AddWithValue("@genre", genre);
                if (!string.IsNullOrWhiteSpace(mood))
                    cmd.Parameters.AddWithValue("@mood", mood);

                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())           // loop: each call to Read() = one row
                        movies.Add(MapMovie(rdr));
            }
            return movies;
        }

        // ════════════════════════════════════════════════════
        //  RATINGS  — CRUD: CREATE / UPDATE
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Upsert rating: if the user already rated this movie UPDATE it,
        /// otherwise INSERT a new row.
        /// MySQL's "ON DUPLICATE KEY UPDATE" handles this in one statement
        /// because (user_id, movie_id) has a UNIQUE constraint.
        /// </summary>
        public static void SaveRating(int userId, int movieId, int stars)
        {
            using (var conn = DB.GetConnection())
            {
                // Step 1 — insert or update the rating row
                using (var cmd = new MySqlCommand(@"
                    INSERT INTO Ratings (user_id, movie_id, stars, rated_at)
                    VALUES (@uid, @mid, @stars, NOW())
                    ON DUPLICATE KEY UPDATE stars = @stars, rated_at = NOW()", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@mid", movieId);
                    cmd.Parameters.AddWithValue("@stars", stars);
                    cmd.ExecuteNonQuery();
                }

                // Step 2 — recalculate the movie's average rating
                // AVG() and COUNT() are SQL aggregate functions
                using (var cmd = new MySqlCommand(@"
                    UPDATE Movies m
                    SET m.rating_avg   = (SELECT ROUND(AVG(r.stars),1) FROM Ratings r WHERE r.movie_id = @mid),
                        m.rating_count = (SELECT COUNT(*)              FROM Ratings r WHERE r.movie_id = @mid)
                    WHERE m.movie_id = @mid", conn))
                {
                    cmd.Parameters.AddWithValue("@mid", movieId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ════════════════════════════════════════════════════
        //  PROFILE  — calls the stored procedure
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Calls sp_GetUserProfile stored procedure, then runs
        /// separate queries for rating history and genre preferences.
        /// </summary>
        public static UserProfile GetProfile(int userId)
        {
            var profile = new UserProfile();

            using (var conn = DB.GetConnection())
            {
                // ── Stored Procedure call ─────────────────────
                // CommandType.StoredProcedure tells ADO.NET to use CALL syntax
                using (var cmd = new MySqlCommand("sp_GetUserProfile", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_user_id", userId);

                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            // The SP returns one row with user info + aggregated stats
                            profile.Info = new User
                            {
                                UserId = rdr.GetInt32("user_id"),
                                FullName = rdr.GetString("full_name"),
                                Email = rdr.GetString("email"),
                                Username = rdr.GetString("username"),
                                Dob = rdr.GetDateTime("dob"),
                                FavGenre = rdr.IsDBNull(rdr.GetOrdinal("fav_genre"))
                                           ? "" : rdr.GetString("fav_genre"),
                                JoinedAt = rdr.GetDateTime("joined_at"),
                            };
                            profile.TotalRatings = rdr.GetInt32("total_ratings");
                            profile.AvgStars = rdr.IsDBNull(rdr.GetOrdinal("avg_stars"))
                                               ? 0 : (double)rdr.GetDecimal("avg_stars");
                        }
                    }
                }

                if (profile.Info == null) return profile;

                // ── Rating history (JOIN Ratings × Movies) ────
                // This shows the "JOIN" concept from Unit 3
                using (var cmd = new MySqlCommand(@"
                    SELECT r.rating_id, r.movie_id, m.title, m.genres,
                           r.stars, r.rated_at
                    FROM Ratings r
                    JOIN Movies m ON m.movie_id = r.movie_id
                    WHERE r.user_id = @uid
                    ORDER BY r.rated_at DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                            profile.Ratings.Add(new RatingHistory
                            {
                                RatingId = rdr.GetInt32("rating_id"),
                                MovieId = rdr.GetInt32("movie_id"),
                                MovieTitle = rdr.GetString("title"),
                                Genre = rdr.GetString("genres").Split(',')[0].Trim(),
                                Stars = rdr.GetInt32("stars"),
                                RatedAt = rdr.GetDateTime("rated_at"),
                            });
                    }
                }

                // ── Genre taste map ───────────────────────────
                // GROUP BY genre, COUNT how many movies user rated per genre
                using (var cmd = new MySqlCommand(@"
                    SELECT TRIM(SUBSTRING_INDEX(m.genres,',',1)) AS genre,
                           COUNT(*) AS cnt
                    FROM Ratings r
                    JOIN Movies m ON m.movie_id = r.movie_id
                    WHERE r.user_id = @uid
                    GROUP BY genre
                    ORDER BY cnt DESC
                    LIMIT 7", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    int total = profile.TotalRatings == 0 ? 1 : profile.TotalRatings;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                            profile.GenrePrefs.Add(new GenrePref
                            {
                                Genre = rdr.GetString("genre"),
                                Count = (int)rdr.GetInt64("cnt"),
                                Weight = (double)rdr.GetInt64("cnt") / total,
                            });
                    }
                }
            }
            return profile;
        }

        // ════════════════════════════════════════════════════
        //  RECOMMENDATIONS  — calls the stored procedure
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Calls sp_GetRecommendations which returns THREE result sets
        /// (genre-based, top-rated, trending).
        /// NextResult() advances to the next result set.
        /// </summary>
        public static List<RecSection> GetRecommendations(int userId)
        {
            var result = new List<RecSection>();
            var types = new[] { "genre", "rating", "trending" };

            using (var conn = DB.GetConnection())
            using (var cmd = new MySqlCommand("sp_GetRecommendations", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_user_id", userId);

                using (var rdr = cmd.ExecuteReader())
                {
                    int setIndex = 0;
                    do   // loop over each result set
                    {
                        var movies = new List<Movie>();
                        while (rdr.Read())      // loop over rows in this result set
                        {
                            var m = MapMovie(rdr);
                            m.RecType = setIndex < types.Length ? types[setIndex] : "other";
                            movies.Add(m);
                        }
                        if (movies.Count > 0)
                            result.Add(new RecSection { Type = types[setIndex], Movies = movies });
                        setIndex++;
                    }
                    while (rdr.NextResult());   // NextResult() = advance to next SELECT result
                }
            }
            return result;
        }

        // ════════════════════════════════════════════════════
        //  STATS  — aggregate queries for dashboard
        // ════════════════════════════════════════════════════

        public static (int Movies, int Ratings, int Users) GetStats()
        {
            using (var conn = DB.GetConnection())
            using (var cmd = new MySqlCommand(@"
                SELECT
                  (SELECT COUNT(*) FROM Movies)  AS movie_count,
                  (SELECT COUNT(*) FROM Ratings) AS rating_count,
                  (SELECT COUNT(*) FROM Users)   AS user_count", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                    return (rdr.GetInt32(0), rdr.GetInt32(1), rdr.GetInt32(2));
                return (0, 0, 0);
            }
        }

        // ════════════════════════════════════════════════════
        //  HELPERS  — convert a DataReader row into an object
        // ════════════════════════════════════════════════════

        // Reads columns from the current reader row → User
        private static User MapUser(MySqlDataReader rdr) => new User
        {
            UserId = rdr.GetInt32("user_id"),
            FullName = rdr.GetString("full_name"),
            Email = rdr.GetString("email"),
            Username = rdr.GetString("username"),
            Dob = rdr.GetDateTime("dob"),
            FavGenre = rdr.IsDBNull(rdr.GetOrdinal("fav_genre")) ? "" : rdr.GetString("fav_genre"),
            JoinedAt = rdr.GetDateTime("joined_at"),
        };

        // Reads columns from the current reader row → Movie
        private static Movie MapMovie(MySqlDataReader rdr) => new Movie
        {
            MovieId = rdr.GetInt32("movie_id"),
            Title = rdr.GetString("title"),
            ReleaseYear = rdr.IsDBNull(rdr.GetOrdinal("release_year")) ? 0 : rdr.GetInt32("release_year"),
            Director = rdr.IsDBNull(rdr.GetOrdinal("director")) ? "" : rdr.GetString("director"),
            Genres = rdr.IsDBNull(rdr.GetOrdinal("genres")) ? "" : rdr.GetString("genres"),
            Moods = rdr.IsDBNull(rdr.GetOrdinal("moods")) ? "" : rdr.GetString("moods"),
            Description = rdr.IsDBNull(rdr.GetOrdinal("description")) ? "" : rdr.GetString("description"),
            RatingAvg = rdr.IsDBNull(rdr.GetOrdinal("rating_avg")) ? 0 : rdr.GetDecimal("rating_avg"),
            RatingCount = rdr.IsDBNull(rdr.GetOrdinal("rating_count")) ? 0 : rdr.GetInt32("rating_count"),
            Trending = rdr.IsDBNull(rdr.GetOrdinal("trending")) ? 0 : rdr.GetInt32("trending"),
            UserRating = HasColumn(rdr, "user_rating") && !rdr.IsDBNull(rdr.GetOrdinal("user_rating"))
                          ? rdr.GetInt32("user_rating") : 0,
        };

        // Safely checks whether a column name exists in the current reader
        private static bool HasColumn(MySqlDataReader rdr, string col)
        {
            for (int i = 0; i < rdr.FieldCount; i++)
                if (rdr.GetName(i) == col) return true;
            return false;
        }
    }
}