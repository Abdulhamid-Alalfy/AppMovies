using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domin.Model;
using infrastructure.Data;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace infrastructure.Services
{
    public interface INotificationService
    {
        Task CreateNotificationForAllUsersAsync(string title, string message, string link, NotificationType type);
        Task CreateNotificationForUserAsync(string userId, string title, string message, string link, NotificationType type);
        Task<List<Notification>> GetUserNotificationsAsync(string userId);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly DB _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<Hubs.NotificationHub> _hubContext;

        public NotificationService(DB context, ILogger<NotificationService> logger, IHubContext<Hubs.NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task CreateNotificationForAllUsersAsync(string title, string message, string link, NotificationType type)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Link = link,
                Type = type,
                IsForAllUsers = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification to all connected clients
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    link = notification.Link,
                    type = (int)notification.Type,
                    createdAt = notification.CreatedAt,
                    isRead = notification.IsRead
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send real-time notification via SignalR");
            }
        }

        public async Task CreateNotificationForUserAsync(string userId, string title, string message, string link, NotificationType type)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Link = link,
                Type = type,
                UserId = userId,
                IsForAllUsers = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification to specific user
            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    link = notification.Link,
                    type = (int)notification.Type,
                    createdAt = notification.CreatedAt,
                    isRead = notification.IsRead
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send real-time notification to user {UserId} via SignalR", userId);
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            var readGlobalIds = (await _context.UserNotificationReads
                .Where(r => r.UserId == userId)
                .Select(r => r.NotificationId)
                .ToListAsync())
                .ToHashSet();

            var notifications = await _context.Notifications
                .Where(n => n.IsForAllUsers || n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20) // آخر 20 إشعار
                .ToListAsync();

            foreach (var n in notifications)
            {
                if (n.IsForAllUsers)
                {
                    n.IsRead = readGlobalIds.Contains(n.Id);
                }
            }

            return notifications;
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            var readGlobalIds = await _context.UserNotificationReads
                .Where(r => r.UserId == userId)
                .Select(r => r.NotificationId)
                .ToListAsync();

            var unreadDirectCount = await _context.Notifications
                .Where(n => !n.IsForAllUsers && n.UserId == userId && !n.IsRead)
                .CountAsync();

            var unreadGlobalCount = await _context.Notifications
                .Where(n => n.IsForAllUsers && !readGlobalIds.Contains(n.Id))
                .CountAsync();

            return unreadDirectCount + unreadGlobalCount;
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId &&
                         (n.IsForAllUsers || n.UserId == userId));

            if (notification != null)
            {
                if (notification.IsForAllUsers)
                {
                    var alreadyRead = await _context.UserNotificationReads
                        .AnyAsync(r => r.NotificationId == notificationId && r.UserId == userId);
                    if (!alreadyRead)
                    {
                        _context.UserNotificationReads.Add(new UserNotificationRead
                        {
                            NotificationId = notificationId,
                            UserId = userId,
                            ReadAt = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            // Direct notifications for this user
            var unreadDirect = await _context.Notifications
                .Where(n => !n.IsForAllUsers && n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadDirect)
            {
                notification.IsRead = true;
            }

            // Global notifications
            var readGlobalIds = await _context.UserNotificationReads
                .Where(r => r.UserId == userId)
                .Select(r => r.NotificationId)
                .ToListAsync();

            var unreadGlobal = await _context.Notifications
                .Where(n => n.IsForAllUsers && !readGlobalIds.Contains(n.Id))
                .ToListAsync();

            foreach (var gn in unreadGlobal)
            {
                _context.UserNotificationReads.Add(new UserNotificationRead
                {
                    NotificationId = gn.Id,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}