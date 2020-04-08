using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PermissionManagement.Model;
using System.Runtime.Caching;
using PermissionManagement.Utility;

namespace PermissionManagement.Services
{
        public class CacheService : ICacheService
        {
            private static MemoryCache _memoryCache = new MemoryCache("ApplicationCache", new System.Collections.Specialized.NameValueCollection());
            private CacheItemPolicy _policy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 25, 0) };

            public bool AddAndTieToSession(string key, object item)
            {
                if (string.IsNullOrEmpty(key) || item == null) return false;
                _memoryCache.Set(key.ToLower(), item, new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, Utility.SecurityConfig.GetCurrent().Cookie.Timeout, 0) });
                return true;
            }
            public bool Add(string key, object item)
            {
                if (string.IsNullOrEmpty(key) || item == null) return false;
                _memoryCache.Set(key.ToLower(), item, _policy);
                return true;
            }

            public bool Add(string key, object item, CacheItemPolicy policy)
            {
                if (string.IsNullOrEmpty(key) || item == null) return false;
                _memoryCache.Set(key.ToLower(), item, policy);
                return true;
            }

            public Object Get(string key)
            {
                if (string.IsNullOrEmpty(key)) { return null; }
                return _memoryCache.Get(key.ToLower());
            }

            public object GetOrAdd(string key, object item)
            {
                if (string.IsNullOrEmpty(key))
                {
                    return null;
                }
                CacheItemPolicy _policy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 25, 0) };
                return _memoryCache.AddOrGetExisting(key.ToLower(), item, _policy);
            }

            public object GetOrAdd(string key, Func<object> func)
            {
                CacheItemPolicy _policy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 25, 0) };
                return GetOrAdd(key, func, _policy);
            }

            public object GetOrAdd(string key, Func<object> func, CacheItemPolicy policy)
            {
                if (string.IsNullOrEmpty(key))
                {
                    return null;
                }
                var v1 = _memoryCache.Get(key.ToLower());
                if (v1 == null)
                {
                    var item = func();
                    var v = _memoryCache.AddOrGetExisting(key.ToLower(), item, policy);
                    v1 = v ?? item;
                }
                return v1;
            }

            public object GetOrAdd(string key, object item, CacheItemPolicy policy)
            {
                if (string.IsNullOrEmpty(key))
                {
                    return null;
                }
                return _memoryCache.AddOrGetExisting(key.ToLower(), item, policy);
            }

            public bool Remove(string key)
            {
                if (string.IsNullOrEmpty(key)) { return false; }
                _memoryCache.Remove(key.ToLower());
                return true;
            }

            public bool RemoveStartWith(string key)
            {
                if (string.IsNullOrEmpty(key)) { return false; }
                var l = (from m in _memoryCache where m.Key.StartsWith(key.ToLower()) select m.Key).ToList();
                foreach (var s in l)
                {
                    _memoryCache.Remove(s);
                }
                return true;
            }

            public bool RemoveEndWith(string key)
            {
                if (string.IsNullOrEmpty(key)) { return false; }
                var l = (from m in _memoryCache where m.Key.EndsWith(key.ToLower()) select m.Key).ToList();
                foreach (var s in l)
                {
                    _memoryCache.Remove(s);
                }
                return true;
            }
    }
}
