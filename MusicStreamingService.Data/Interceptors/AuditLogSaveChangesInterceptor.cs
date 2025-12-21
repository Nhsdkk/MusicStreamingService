using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Data.Interceptors;

public interface IAuditable
{
    
}

public class AuditLogSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditLogs(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, 
        InterceptionResult<int> result)
    {
        AddAuditLogs(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    private static void AddAuditLogs(
        DbContext context)
    {
        var changedEntities = context.ChangeTracker
            .Entries<IAuditable>()
            .Where(x => x.State is not EntityState.Detached && x.State is not EntityState.Unchanged)
            .ToList();

        foreach (var changedEntity in changedEntities)
        {
            Dictionary<string, object?> oldValues = new();
            Dictionary<string, object?> newValues = new();
            
            switch (changedEntity.State)
            {
                case EntityState.Added:
                    newValues = changedEntity.Properties
                        .ToDictionary(x => x.Metadata.Name, x => x.CurrentValue);
                    break;
                case EntityState.Deleted:
                    oldValues = changedEntity.Properties
                        .ToDictionary(x => x.Metadata.Name, x => x.OriginalValue);
                    break;
                case EntityState.Modified:
                    newValues = changedEntity.Properties
                        .Where(x => x.IsModified)
                        .ToDictionary(x => x.Metadata.Name, x => x.CurrentValue);
                    oldValues = changedEntity.Properties
                        .Where(x => x.IsModified)
                        .ToDictionary(x => x.Metadata.Name, x => x.OriginalValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(EntityState), "Unexpected entity state", changedEntity.State.ToString());
            }
            
            var auditLog = new AuditLogEntity
            {
                Action = changedEntity.State switch
                {
                    EntityState.Added => EntityAction.Create,
                    EntityState.Deleted => EntityAction.Delete,
                    EntityState.Modified => EntityAction.Update,
                    _ => throw new ArgumentOutOfRangeException(nameof(EntityState), "Unexpected entity state", changedEntity.State.ToString())
                },
                TableName = changedEntity.Metadata.GetTableName() ?? string.Empty,
                NewValues = System.Text.Json.JsonSerializer.Serialize(newValues),
                OldValues = System.Text.Json.JsonSerializer.Serialize(oldValues)
            };

            context.Set<AuditLogEntity>().Add(auditLog);
        }
    }
}
