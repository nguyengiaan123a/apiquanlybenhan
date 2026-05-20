using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using yhctapp.Attributes;

namespace yhctapp.Interceptors
{
    public class CacheInvalidationInterceptor : SaveChangesInterceptor
    {
        private readonly IMemoryCache _cache;

        public CacheInvalidationInterceptor(IMemoryCache cache)
        {
            _cache = cache;
        }

        // Đổi từ SavedChangesAsync thành SavingChangesAsync
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, 
            InterceptionResult<int> result, 
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context != null)
            {
                // 1. Lấy trạng thái trước khi lưu (Lúc này State vẫn là Added/Modified/Deleted)
                var changedEntries = context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added ||
                                e.State == EntityState.Modified ||
                                e.State == EntityState.Deleted)
                    .ToList();

                // 2. Xóa cache
                foreach (var entry in changedEntries)
                {
                    var cacheAttribute = entry.Entity.GetType().GetCustomAttribute<InvalidateCacheAttribute>();

                    if (cacheAttribute != null)
                    {
                        _cache.Remove(cacheAttribute.CacheKey);
                    }
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}