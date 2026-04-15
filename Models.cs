// ============================================================
//  Models.cs  —  Plain C# classes that mirror database tables
//
//  BACKEND EXPLANATION:
//  These are called "POCOs" (Plain Old C# Objects).
//  Each property maps to one column in the database table.
//  When we read a row from MySQL we fill one of these objects.
//  This is the "Model" layer — it holds data, no logic.
// ============================================================
using System;
using System.Collections.Generic;

namespace CineMatch
{
    // ── Mirrors the Users table ───────────────────────────────
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public DateTime Dob { get; set; }
        public string FavGenre { get; set; }
        public DateTime JoinedAt { get; set; }

        // Calculated — not a column, we compute it from Dob
        public int Age => DateTime.Today.Year - Dob.Year -
                                  (DateTime.Today < Dob.AddYears(DateTime.Today.Year - Dob.Year) ? 1 : 0);
        public string AgeGroup
        {
            get
            {
                if (Age < 18) return "Teen";
                if (Age < 25) return "Young Adult";
                if (Age < 40) return "Adult";
                return "Senior";
            }
        }

        // Returns first two letters of full name — used for avatar circle
        public string AvatarInit => FullName?.Length >= 2
            ? FullName.Substring(0, 2).ToUpper()
            : (FullName ?? "?").ToUpper();
    }

    // ── Mirrors the Movies table ──────────────────────────────
    public class Movie
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public int ReleaseYear { get; set; }
        public string Director { get; set; }
        public string Genres { get; set; }   // "Action, Sci-Fi"
        public string Moods { get; set; }   // "Dark, Tense"
        public string Description { get; set; }
        public decimal RatingAvg { get; set; }
        public int RatingCount { get; set; }
        public int Trending { get; set; }

        // The user's own rating for this movie (0 = not rated)
        public int UserRating { get; set; }

        // Which recommendation category put this movie here
        public string RecType { get; set; }

        // Primary genre (first one before the comma)
        public string PrimaryGenre => Genres?.Split(',')[0].Trim() ?? "—";
    }

    // ── Mirrors one row from the Ratings JOIN Movies query ────
    public class RatingHistory
    {
        public int RatingId { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string Genre { get; set; }
        public int Stars { get; set; }
        public DateTime RatedAt { get; set; }
    }

    // ── Genre preference — computed, not a table ──────────────
    public class GenrePref
    {
        public string Genre { get; set; }
        public int Count { get; set; }   // how many movies of this genre user rated
        public double Weight { get; set; }   // 0.0–1.0 normalised fraction
    }

    // ── Full profile returned by sp_GetUserProfile ────────────
    public class UserProfile
    {
        public User Info { get; set; }
        public int TotalRatings { get; set; }
        public double AvgStars { get; set; }
        public List<RatingHistory> Ratings { get; set; } = new List<RatingHistory>();
        public List<GenrePref> GenrePrefs { get; set; } = new List<GenrePref>();
    }
    public class RecSection
    {
        public string Type { get; set; }
        public List<Movie> Movies { get; set; } = new List<Movie>();
    }
}