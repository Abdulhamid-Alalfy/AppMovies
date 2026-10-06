// IMoviesService.cs
using Domin.Model;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using static infrastructure.ViewModel.QualitMoviesVM;

namespace infrastructure.Services
{
    public interface IMoviesService
    {
        Task<IEnumerable<MovieViewModel>> GetAllMoviesAsync();
        Task<IEnumerable<MovieViewModel>> GetLatestMoviesAsync(int count);
        Task<MovieDetailsViewModel> GetMovieDetailsByIdAsync(int id); // إضافة جديدة
        Task<UpdateMovieModelView> GetMovieForEditAsync(int id); // إضافة جديدة
        Task<bool> AddMovieAsync(CreateMovieViewModel model, string? userId = null);
        Task<bool> UpdateMovieAsync(UpdateMovieModelView model, string? userId = null);
        Task<bool> DeleteMovieAsync(int id, string? userId = null);
        Task<List<ProducerListVM>> GetAllProducersAsync();
        Task<List<ActorListVm>> GetAllActorsAsync();
        Task<IEnumerable<MovieViewModel>> SearchMoviesAsync(string searchTerm);
        Task<List<GenreListViewModel>> GetAllGenresAsync();
        Task<MoviesByGenreViewModel> GetMoviesByGenreWithPaginationAsync(int genreId, int page = 1, int pageSize = 12);
        Task<List<GenreListViewModel>> GetAllGenresWithCountAsync();

        //Add Quality
        Task<List<MovieQualityViewModel>> GetMovieQualitiesAsync(int movieId);

    }
}
