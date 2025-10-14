using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Data.Contexts.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditFields(DbContext? context)
        {
            if (context == null) return;

            var now = DateTime.UtcNow;

            // Handle Added entities
            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added))
            {
                entry.Entity.CreatedDate = now;
                entry.Entity.ModifiedDate = now;
            }

            // Handle Modified entities
            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Modified))
            {
                entry.Entity.ModifiedDate = now;

                // Prevent CreatedDate from being modified
                entry.Property(e => e.CreatedDate).IsModified = false;
            }
        }
    }
}
