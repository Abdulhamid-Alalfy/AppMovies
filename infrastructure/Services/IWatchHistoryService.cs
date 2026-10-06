using Domin.Model;

namespace infrastructure.Services
{
    public interface IWatchHistoryService
    {
        Task AddToWatchHistoryAsync(int movieId, AppUser user, int progress = 0);
        Task<int> AddToWatchHistoryWithIdAsync(int movieId, AppUser user, int progress = 0);
        Task<List<WatchHistory>> GetWatchHistoryAsync(AppUser user);
        Task RemoveFromWatchHistoryAsync(int watchHistoryId);
        Task<bool> RemoveFromWatchHistoryForUserAsync(int watchHistoryId, string userId);
        Task ClearWatchHistoryAsync(AppUser user);
        Task UpdateWatchProgressAsync(int watchHistoryId, int progress, TimeSpan? watchedDuration);
        Task<bool> UpdateProgressByMovieAsync(int movieId, string userId, int progress);
        Task<bool> UpdateWatchProgressForUserAsync(int watchHistoryId, string userId, int progress, TimeSpan? watchedDuration = null);
    }
}
