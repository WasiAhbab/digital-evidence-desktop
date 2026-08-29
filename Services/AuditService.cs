using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;

namespace TraceLock.Desktop.Services;

public static class AuditService
{
    public static void Record(string action, string details, string entity = "System", string entityId = "")
    {
        using var db = Database.Create();
        db.AuditLogs.Add(new AuditLog { Actor=AppSession.User?.FullName ?? "System", Action=action, Entity=entity, EntityId=entityId, Details=details, IpAddress="Local desktop" });
        db.SaveChanges();
    }
}


//pathing to this directory