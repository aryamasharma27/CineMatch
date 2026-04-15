
using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace CineMatch
{
    public static class DataService
    {
        // AUTH
        public static User Login(string username, string password)
        {
            
            using (var conn = DB.GetConnection())
            
            using (var cmd = new MySqlCommand(
                "SELECT * FROM Users WHERE username = @u AND password = @p", conn))
            {
                
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())           
                        return MapUser(rdr);  
                    return null;              
                }
            }
        }

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

                
                cmd.ExecuteNonQuery();

                return (int)cmd.LastInsertedId;
            }
        }

        
        //  MOVIES
        
        public static List<Movie> GetMovies(string search = "", string genre = "",
                                            string mood = "", string sort = "rating_avg",
                                            int userId = 0)
        {
          
            var where = new System.Text.StringBuilder("WHERE 1=1");
            if (!string.IsNullOrWhiteSpace(search))
                where.Append(" AND (m.title LIKE @search OR m.director LIKE @search)");
            if (!string.IsNullOrWhiteSpace(genre))
                where.Append(" AND FIND_IN_SET(@genre, REPLACE(m.genres,', ',',')) > 0");
            if (!string.IsNullOrWhiteSpace(mood))
                where.Append(" AND FIND_IN_SET(@mood, REPLACE(m.moods,', ',',')) > 0");

            string orderCol = sort == "year" ? "m.release_year DESC"
                            : sort == "title" ? "m.title ASC"
                            : sort == "trending" ? "m.trending DESC"
                            : "m.rating_avg DESC";

          
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

       
        //  RATINGS  — CRUD: CREATE / UPDATE
    
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

       
        //  PROFILE  — calls the stored procedure
       
        public static UserProfile GetProfile(int userId)
        {
            var profile = new UserProfile();

            using (var conn = DB.GetConnection())
            {
                // Stored Procedure call
               
                using (var cmd = new MySqlCommand("sp_GetUserProfile", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_user_id", userId);

                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                        
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

                //Rating history (JOIN Ratings × Movies)
               
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

                // Genre taste map 
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

        //  RECOMMENDATIONS  — calls the stored procedure
      
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
                    do 
                    {
                        var movies = new List<Movie>();
                        while (rdr.Read())      
                        {
                            var m = MapMovie(rdr);
                            m.RecType = setIndex < types.Length ? types[setIndex] : "other";
                            movies.Add(m);
                        }
                        if (movies.Count > 0)
                            result.Add(new RecSection { Type = types[setIndex], Movies = movies });
                        setIndex++;
                    }
                    while (rdr.NextResult());   
                }
            }
            return result;
        }
        //  STATS  — aggregate queries for dashboard
    
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
        //  HELPERS  — convert a DataReader row into an object
      
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

        private static bool HasColumn(MySqlDataReader rdr, string col)
        {
            for (int i = 0; i < rdr.FieldCount; i++)
                if (rdr.GetName(i) == col) return true;
            return false;
        }
    }
}