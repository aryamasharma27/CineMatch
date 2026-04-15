
using System;
using System.Collections.Generic;

namespace CineMatch
{
    //Mirrors the Users table
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public DateTime Dob { get; set; }
        public string FavGenre { get; set; }
        public DateTime JoinedAt { get; set; }

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

        public string AvatarInit => FullName?.Length >= 2
            ? FullName.Substring(0, 2).ToUpper()
            : (FullName ?? "?").ToUpper();
    }

    //Mirrors the Movies table
    public class Movie
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public int ReleaseYear { get; set; }
        public string Director { get; set; }
        public string Genres { get; set; }  
        public string Moods { get; set; }  
        public string Description { get; set; }
        public decimal RatingAvg { get; set; }
        public int RatingCount { get; set; }
        public int Trending { get; set; }

       
        public int UserRating { get; set; }

     
        public string RecType { get; set; }

      
        public string PrimaryGenre => Genres?.Split(',')[0].Trim() ?? "—";
    }

    public class RatingHistory
    {
        public int RatingId { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string Genre { get; set; }
        public int Stars { get; set; }
        public DateTime RatedAt { get; set; }
    }

    //Genre preference — computed, not a table 
    public class GenrePref
    {
        public string Genre { get; set; }
        public int Count { get; set; } 
        public double Weight { get; set; }   
    }

    //Full profile 
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