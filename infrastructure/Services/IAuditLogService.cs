using System.Threading.Tasks;

namespace infrastructure.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string userId, string action, string entity, int? entityId, string details);
    }
}
