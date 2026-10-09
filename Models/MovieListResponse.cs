namespace ReelTime.Models
{
    public class MovieListResponse
    {
        public int Page { get; set; }
        public List<Movie> Results { get; set; } = new List<Movie>();
        public int TotalPages { get; set; }
        public int TotalResults { get; set; }
    }
}
