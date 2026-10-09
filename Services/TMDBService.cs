using ReelTime.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ReelTime.Services
{
    // 1. Declare your class
    public class TMDBService
    {    // 2. Private readonly field for the injected HttpClient
        private readonly HttpClient _http;

        // 3. Private readonly JsonSerializerOptions for snake case
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        // 4. Constructor injecting both HttpClient and IConfiguration
        public TMDBService(HttpClient http, IConfiguration config)
        {
            _http = http; // Assign the injected HttpClient to our private field

            // 5. Retrieve TMDB access key from configuration
            // The only time it will find key is in development
            string? tmdbKey = config["TmdbAccessKey"];

            // 6. Check if key is present; if so, set BaseAddress and Authorization headers
            if (!string.IsNullOrWhiteSpace(tmdbKey))
            {
                _http.BaseAddress = new Uri("https://api.themoviedb.org/3/");
                _http.DefaultRequestHeaders.Authorization = new("Bearer", tmdbKey);
            }
            else
            {
                //For not development - Deployed to netlify
                // For example, if you host on Netlify with an edge function or proxy
                _http.BaseAddress = new Uri(_http.BaseAddress + "tmdb/");
            }
        }

        // 7. Public method to fetch "now playing" movies from TMDB
        public async Task<MovieListResponse> GetNowPlayingMovies()
        {
            // 7a. The relative URL for the now playing endpoint
            string url = "movie/now_playing?region=US&language=en-us";

            // 7b. Make the HTTP GET call, deserializing into a MovieListResponse
            MovieListResponse response = await _http.GetFromJsonAsync<MovieListResponse>(url, _jsonOptions)
                ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "Now Playing Movies could not be loaded");

            // 7c. Loop through each movie and fix up PosterPath
            foreach (Movie movie in response.Results)
            {
                if (string.IsNullOrWhiteSpace(movie.PosterPath))
                {
                    // Fallback image
                    movie.PosterPath = "/images/poster.png";
                }
                else
                {
                    // Construct a valid URL for the poster
                    movie.PosterPath = $"http://image.tmdb.org/t/p/w500{movie.PosterPath}";
                }
            }

            // 7d. Return the fully populated list
            return response;
        }

        public async Task<MovieListResponse> GetPopularMovies()
        {
            string url = "movie/popular?region=US&language=en-US";

            MovieListResponse response = await _http.GetFromJsonAsync<MovieListResponse>(url, _jsonOptions)
                ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "Popular Movies could not be loaded");

            foreach (Movie movie in response.Results)
            {
                if (string.IsNullOrWhiteSpace(movie.PosterPath))
                {
                    movie.PosterPath = "/images/poster.png";
                }
                else
                {
                    movie.PosterPath = $"http://image.tmdb.org/t/p/w500{movie.PosterPath}";
                }
            }

            return response;
        }

        public async Task<CreditsResponse> GetMovieCredits(int movieId)
        {
            string url = $"movie/{movieId}/credits?language=en-US";

            var credits = await _http.GetFromJsonAsync<CreditsResponse>(url, _jsonOptions)
                          ?? throw new HttpIOException(HttpRequestError.InvalidResponse,
                              $"Could not retrieve movie credits");

            foreach (var castMember in credits.Cast)
            {
                castMember.ProfilePath = string.IsNullOrEmpty(castMember.ProfilePath)
                    ? "/images/profile.jpg"
                    : $"https://image.tmdb.org/t/p/w500{castMember.ProfilePath}";
            }

            foreach (var crewMember in credits.Crew)
            {
                crewMember.ProfilePath = string.IsNullOrEmpty(crewMember.ProfilePath)
                    ? "/images/profile.jpg"
                    : $"https://image.tmdb.org/t/p/w500{crewMember.ProfilePath}";
            }

            return credits;
        }
        // Search for movies      
        // <param name="query">User supplied query</param>
        // <returns>A MovieListResponse with matching results</returns>
        // <exception cref="HttpIOException"></exception>
        public async Task<MovieListResponse> SearchMovies(string query)
        {
            var url = $"search/movie?query={query}&include_adult=false&language=en-US";

            MovieListResponse response = await _http.GetFromJsonAsync<MovieListResponse>(url, _jsonOptions)
                ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "Search results could not be loaded");

            foreach (Movie movie in response.Results)
            {
                if (string.IsNullOrWhiteSpace(movie.PosterPath))
                {
                    movie.PosterPath = "/images/poster.png";
                }
                else
                {
                    movie.PosterPath = $"http://image.tmdb.org/t/p/w500{movie.PosterPath}";
                }
            }

            return response;
        }


        /// <summary>
        /// Retrieves detailed information for a specific movie from The Movie Database API.
        /// </summary>
        /// <param name="movieId">The unique identifier of the movie to retrieve.</param>
        /// <returns>
        /// A <see cref="MovieDetails"/> object containing the movie's information, including poster and backdrop image URLs.
        /// </returns>
        /// <exception cref="HttpIOException">
        /// Thrown when the API response is null or cannot be deserialized into a valid <see cref="MovieDetails"/> object.
        /// The <see cref="HttpRequestError.InvalidResponse"/> error indicates the TMDB API returned an unexpected response format.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method constructs a request to the TMDB API endpoint for a specific movie by ID.
        /// The response is deserialized using the configured JSON options in <see cref="_jsonOptions"/>.
        /// </para>
        /// <para>
        /// Image paths are processed as follows:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// If <see cref="MovieDetails.PosterPath"/> is null or empty, it defaults to "/images/poster.png" 
        /// (a local fallback image resource).
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// If <see cref="MovieDetails.BackdropPath"/> is null or empty, it defaults to "/images/backdrop.jpg" 
        /// (a local fallback image resource).
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// If image paths are provided by the API, they are prefixed with the TMDB image CDN base URL 
        /// (http://image.tmdb.org/t/p/w500) to form complete, publicly accessible URLs using the 500px width format.
        /// </description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        public async Task<MovieDetails> GetMovieById(int movieId)
        {
            string url = $"movie/{movieId}";

            MovieDetails movie = await _http.GetFromJsonAsync<MovieDetails>(url, _jsonOptions)
                                    ?? throw new HttpIOException(HttpRequestError.InvalidResponse,
                                    "Could not retrieve movie details");

            movie.PosterPath = string.IsNullOrEmpty(movie.PosterPath)
                                ? "/images/poster.png"
                                : $"http://image.tmdb.org/t/p/w500{movie.PosterPath}";

            movie.BackdropPath = string.IsNullOrEmpty(movie.BackdropPath)
                               ? "/images/backdrop.jpg"
                               : $"http://image.tmdb.org/t/p/w500{movie.BackdropPath}";

            return movie;
        }

        //public async Task<Video?> GetMovieTrailer(int movieId)
        //{
        //    string url = $"https://api.themoviedb.org/3/movie/{movieId}/videos?language=en-US";

        //    var videos = await _http.GetFromJsonAsync<MovieVideosResponse>(url, _jsonOptions)
        //        ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "Could not retreive movie videos");

        //    return videos.Results.FirstOrDefault(v =>
        //        v.Site!.Contains("YouTube", StringComparison.OrdinalIgnoreCase)
        //        && v.Type!.Contains("Trailer", StringComparison.OrdinalIgnoreCase));
        //}

        public async Task<Video?> GetMovieTrailer(int movieId)
        {
            string url = $"movie/{movieId}/videos?language=en-US";

            // TEMP: see the raw JSON TMDB actually sends back
            string raw = await _http.GetStringAsync(url);

            var videos = await _http.GetFromJsonAsync<MovieVideosResponse>(url, _jsonOptions);
            Console.WriteLine($"Results: {videos.Results?.Count}, first site: {videos.Results?.FirstOrDefault()?.Site}, type: {videos.Results?.FirstOrDefault()?.Type}");
         
            var count = videos?.Results?.Count ?? -1;


            var trailer = videos?.Results?.FirstOrDefault(v =>
                (v.Site ?? "").Contains("YouTube", StringComparison.OrdinalIgnoreCase)
                && (v.Type ?? "").Contains("Trailer", StringComparison.OrdinalIgnoreCase));

            Console.WriteLine($"Trailer key: {trailer?.Key ?? "NULL"}");

            return trailer;   // <-- put breakpoint here
        }
    }

}
