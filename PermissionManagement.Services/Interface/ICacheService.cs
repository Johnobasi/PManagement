using System;
using System.Runtime.Caching;

namespace PermissionManagement.Services
{
    public interface ICacheService
    {
        bool AddAndTieToSession(string key, object item);
        bool Add(string key, object item);
        bool Add(string key, object item, CacheItemPolicy policy);
        object Get(string key);
        bool Remove(string key);
        bool RemoveStartWith(string key);
        bool RemoveEndWith(string key);
        object GetOrAdd(string key, object item);
        object GetOrAdd(string key, Func<object> func);
        object GetOrAdd(string key, Func<object> func, CacheItemPolicy policy);
        object GetOrAdd(string key, object item, CacheItemPolicy policy);
    }
}
