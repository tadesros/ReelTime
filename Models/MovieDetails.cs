namespace ReelTime.Models
{
    public class MovieDetails
    {
        public bool Adult { get; set; }
        public string? BackdropPath { get; set; }
        public int Budget { get; set; }
        public List<Genre> Genres { get; set; } = new();
        public string? Homepage { get; set; }
        public int Id { get; set; }
        public string? ImdbId { get; set; }
        public string[] OriginCountry { get; set; } = [];
        public string? OriginalLanguage { get; set; }
        public string? OriginalTitle { get; set; }
        public string? Overview { get; set; }
        public float Popularity { get; set; }
        public string? PosterPath { get; set; }
        public List<ProductionCompanies> ProductionCompanies { get; set; } = new();
        public List<ProductionCountries> ProductionCountries { get; set; } = new();
        public string? ReleaseDate { get; set; }
        public int Revenue { get; set; }
        public int Runtime { get; set; }
        public List<SpokenLanguages> SpokenLanguages { get; set; } = new();
        public string? Status { get; set; }
        public string? Tagline { get; set; }
        public string? Title { get; set; }
        public bool Video { get; set; }
        public float VoteAverage { get; set; }
        public int VoteCount { get; set; }
    }

    public class Genre
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class ProductionCompanies
    {
        public int Id { get; set; }
        public string? LogoPath { get; set; }
        public string? Name { get; set; }
        public string? OriginCountry { get; set; }
    }

    public class ProductionCountries
    {
        public string? Name { get; set; }
    }

    public class SpokenLanguages
    {
        public string? EnglishName { get; set; }
        public string? Name { get; set; }
    }
}