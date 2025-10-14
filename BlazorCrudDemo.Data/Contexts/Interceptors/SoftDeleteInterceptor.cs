using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Data.Contexts.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            HandleSoftDelete(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            HandleSoftDelete(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void HandleSoftDelete(DbContext? context)
        {
            if (context == null) return;

            // Handle Deleted entities (soft delete)
            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Deleted))
            {
                // Instead of actually deleting, mark as inactive
                entry.State = EntityState.Modified;
                entry.Entity.IsActive = false;
                entry.Entity.ModifiedDate = DateTime.UtcNow;
            }
        }
    }
}
