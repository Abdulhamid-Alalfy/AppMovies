using Domin.Model;
using infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Services
{
    public class WatchHistoryService : IWatchHistoryService
    {
        private readonly DB _context;

        public WatchHistoryService(DB context)
        {
            _context = context;
        }

        public async Task AddToWatchHistoryAsync(int movieId, AppUser user, int progress = 0)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
                throw new Exception("Movie not found");

            var existingEntry = await _context.WatchHistory
                .FirstOrDefaultAsync(wh => wh.MovieId == movieId && wh.UserId == user.Id);

            if (existingEntry == null)
            {
                var watchHistory = new WatchHistory
                {
                    MovieId = movieId,
                    UserId = user.Id,
                    WatchedAt = DateTime.UtcNow,
                    Progress = progress
                };

                _context.WatchHistory.Add(watchHistory);
            }
            else
            {
                existingEntry.WatchedAt = DateTime.UtcNow;
                if (progress > existingEntry.Progress)
                {
                    existingEntry.Progress = progress;
                }
                _context.WatchHistory.Update(existingEntry);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<WatchHistory>> GetWatchHistoryAsync(AppUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var watchHistory = await _context.WatchHistory
                .Where(wh => wh.UserId == user.Id)
                .Include(wh => wh.Movie)
                .OrderByDescending(wh => wh.WatchedAt)
                .ToListAsync();

            return watchHistory;
        }

        public async Task RemoveFromWatchHistoryAsync(int watchHistoryId)
        {
            var watchHistory = await _context.WatchHistory.FindAsync(watchHistoryId);
            if (watchHistory == null)
                throw new Exception("Watch history entry not found");

            _context.WatchHistory.Remove(watchHistory);
            await _context.SaveChangesAsync();
        }

        public async Task ClearWatchHistoryAsync(AppUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var userWatchHistory = await _context.WatchHistory
                .Where(wh => wh.UserId == user.Id)
                .ToListAsync();

            _context.WatchHistory.RemoveRange(userWatchHistory);
            await _context.SaveChangesAsync();
        }

        public async Task<int> AddToWatchHistoryWithIdAsync(int movieId, AppUser user, int progress = 0)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
                throw new Exception("Movie not found");

            var existingEntry = await _context.WatchHistory
                .FirstOrDefaultAsync(wh => wh.MovieId == movieId && wh.UserId == user.Id);

            if (existingEntry == null)
            {
                var watchHistory = new WatchHistory
                {
                    MovieId = movieId,
                    UserId = user.Id,
                    WatchedAt = DateTime.UtcNow,
                    Progress = progress
                };

                _context.WatchHistory.Add(watchHistory);
                await _context.SaveChangesAsync();
                return watchHistory.Id;
            }
            else
            {
                existingEntry.WatchedAt = DateTime.UtcNow;
                if (progress > existingEntry.Progress)
                {
                    existingEntry.Progress = progress;
                }
                _context.WatchHistory.Update(existingEntry);
                await _context.SaveChangesAsync();
                return existingEntry.Id;
            }
        }

        public async Task<bool> UpdateProgressByMovieAsync(int movieId, string userId, int progress)
        {
            var entry = await _context.WatchHistory
                .FirstOrDefaultAsync(wh => wh.MovieId == movieId && wh.UserId == userId);
            if (entry == null) return false;

            entry.Progress = progress;
            entry.WatchedAt = DateTime.UtcNow;
            _context.WatchHistory.Update(entry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromWatchHistoryForUserAsync(int watchHistoryId, string userId)
        {
            var entry = await _context.WatchHistory
                .FirstOrDefaultAsync(wh => wh.Id == watchHistoryId && wh.UserId == userId);
            if (entry == null) return false;

            _context.WatchHistory.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateWatchProgressForUserAsync(int watchHistoryId, string userId, int progress, TimeSpan? watchedDuration = null)
        {
            var entry = await _context.WatchHistory
                .FirstOrDefaultAsync(wh => wh.Id == watchHistoryId && wh.UserId == userId);
            if (entry == null) return false;

            entry.Progress = progress;
            entry.WatchedDuration = watchedDuration;
            entry.WatchedAt = DateTime.UtcNow;
            _context.WatchHistory.Update(entry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateWatchProgressAsync(int watchHistoryId, int progress, TimeSpan? watchedDuration)
        {
            var watchHistory = await _context.WatchHistory.FindAsync(watchHistoryId);
            if (watchHistory == null)
                throw new Exception("Watch history entry not found");

            watchHistory.Progress = progress;
            watchHistory.WatchedDuration = watchedDuration;
            watchHistory.WatchedAt = DateTime.UtcNow;

            _context.WatchHistory.Update(watchHistory);
            await _context.SaveChangesAsync();
        }
    }
}
