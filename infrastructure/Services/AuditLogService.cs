using System.Text.Json;
using System.Threading.Tasks;
using infrastructure.Data;
using Domin.Model;

namespace infrastructure.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly DB _db;

        public AuditLogService(DB db)
        {
            _db = db;
        }

        public async Task LogAsync(string userId, string action, string entity, int? entityId, string details)
        {
            var entry = new AuditLog
            {
                UserId = userId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Details = details
            };

            _db.AuditLogs.Add(entry);
            await _db.SaveChangesAsync();
        }
    }
}
