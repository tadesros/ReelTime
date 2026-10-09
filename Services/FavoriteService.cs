using Microsoft.JSInterop;
using ReelTime.Models;
using System.Text.Json;

namespace ReelTime.Services
{
    public class FavoritesService(IJSRuntime jsRuntime)
    {
        private readonly string _localStorageKey = "favorites";

        /// <summary>
        /// Returns a list of movies stored in local storage
        /// </summary>
        /// <returns></returns>
        public async Task<List<Movie>> GetFavorites()
        {
            List<Movie> movies = new();
            try
            {
                var json = await jsRuntime.InvokeAsync<string>("localStorage.getItem", _localStorageKey);
                movies = JsonSerializer.Deserialize<List<Movie>>(json) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return movies;
        }

        /// <summary>
        /// Saves a list of movies to local storage
        /// </summary>
        /// <param name="movies">a list of movies</param>
        /// <returns></returns>
        public async Task SaveFavorites(List<Movie> movies)
        {
            try
            {
                var json = JsonSerializer.Serialize(movies);
                await jsRuntime.InvokeVoidAsync("localStorage.setItem", _localStorageKey, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Adds a movie to local storage
        /// </summary>
        /// <param name="movie">An object of type Movie</param>
        /// <returns></returns>
        public async Task AddFavorite(Movie movie)
        {
            var currentMovies = await GetFavorites();

            if (currentMovies.All(m => m.Id != movie.Id))
            {
                currentMovies.Add(movie);
                await SaveFavorites(currentMovies);
            }
        }

        /// <summary>
        /// removes a movie from local storage
        /// </summary>
        /// <param name="movie">an object of type Movie</param>
        /// <returns></returns>
        public async Task RemoveFavorite(Movie movie)
        {
            var currentMovies = await GetFavorites();
            currentMovies = currentMovies.Where(f => f.Id != movie.Id).ToList();
            await SaveFavorites(currentMovies);
        }

        /// <summary>
        /// checks if a movie is in the list of favorites
        /// </summary>
        /// <param name="id">id of a movie</param>
        /// <returns></returns>
        public async Task<bool> IsFavorite(int id)
        {
            var currentMovies = await GetFavorites();
            bool IsFavorite = currentMovies.Any(f => f.Id == id);
            return IsFavorite;
        }
    }
}

