using System;

namespace yhctapp.Attributes
{
    // Đánh dấu Attribute này chỉ được dùng cho Class (Bảng)
    [AttributeUsage(AttributeTargets.Class)]
    public class InvalidateCacheAttribute : Attribute
    {
        public string CacheKey { get; }

        public InvalidateCacheAttribute(string cacheKey)
        {
            CacheKey = cacheKey;
        }
    }
}