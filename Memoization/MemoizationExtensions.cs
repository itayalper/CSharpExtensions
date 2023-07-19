using System.Collections.Concurrent;
using System.Security.Cryptography;
using Memoization;
using static CSharpExtensions.Monads.DataStructures;
using System.Text;

namespace Monads.Memoization;

public static partial class MemoizationExtensions
{
    private static string Hash(string input)
    {
        return Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(input)));
    }

    #region Memo no args
    public static Func<TResult> Memoize<TResult>(this Func<TResult> func)
        where TResult : class
    {
        WeakReference<TResult>? cachedResult = null;
        var sync = new object();

        return () =>
        {
            if (cachedResult != null && cachedResult.TryGetTarget(out var result))
            {
                return result;
            }
            lock (sync)
            {
                result = func();
                cachedResult = new WeakReference<TResult>(result);
                return result;
            }
        };
    }

    public static Func<Task<TResult>> Memoize<TResult>(this Func<Task<TResult>> func)
        where TResult : class
    {
        WeakReference<TResult>? cachedResult = null;
        var sync = new SemaphoreSlim(1, 1);

        return async () =>
        {
            if (cachedResult != null && cachedResult.TryGetTarget(out var result))
            {
                return result;
            }
            await sync.WaitAsync();
            try
            {
                result = await func();
                cachedResult = new WeakReference<TResult>(result);
                return result;
            }
            finally
            {
                sync.Release();
            }
        };
    }
    #endregion

    #region Memo 1 arg
    public static Func<T1, TResult> Memoize<T1, TResult>(this Func<T1, TResult> func)
        where T1 : notnull
    {
        var cache = new WeakDict<T1, TResult>();
        var syncMap = new ConcurrentDictionary<T1, object>();
        return (arg1) =>
        {
            if (cache.TryGetValue(arg1, out var result))
                return result;
            var sync = syncMap.GetOrAdd(arg1, new object());

            lock (sync)
            {
                // in case t1 and t2 both try to access the sync
                // 1. only 1 will succeed and it will update the cache first
                // 2. the 2nd thread will then retrieve the lock and it will read the assigned response
                if (cache.TryGetValue(arg1, out result))
                    return result;
                result = func(arg1);
                cache.Set(arg1, result);
            }
            // clean the sync map
            syncMap.TryRemove(arg1, out var _);
            return result;
        };
    }

    public static Func<T1, Task<TResult>> Memoize<T1, TResult>(this Func<T1, Task<TResult>> func)
    {
        var cache = new WeakDict<T1, TResult>();
        var syncMap = new ConcurrentDictionary<T1, SemaphoreSlim>();

        return async (arg1) =>
        {
            if (cache.TryGetValue(arg1, out var result))
                return result;

            var sync = syncMap.GetOrAdd(arg1, new SemaphoreSlim(1, 1));

            await sync.WaitAsync();
            try
            {
                // in case t1 and t2 both try to access the sync
                // 1. only 1 will succeed and it will update the cache first
                // 2. the 2nd thread will then retrieve the lock and it will read the assigned response
                if (cache.TryGetValue(arg1, out result))
                    return result;
                result = await func(arg1);
                cache.Set(arg1, result);
            }
            finally
            {
                sync.Release();
            }
            // clean the sync map
            // even if t3 gets a different sync to t1/t2 it will check if there is data in the cache first and only then attempt to overwrite it
            // the overwrite scenario is fine since it could happen that the weakreference had been collected
            syncMap.TryRemove(arg1, out var _);
            return result;
        };
    }
    #endregion

    #region Memo 2 args

    public static Func<T1, T2, TResult> Memoize<T1, T2, TResult>(this Func<T1, T2, TResult> func)
    {
        var cache = new WeakDict<string, TResult>();
        var syncMap = new ConcurrentDictionary<string, object>();
        return (arg1, arg2) =>
        {
            var hash = Hash(string.Concat(arg1?.ToString(), arg2?.ToString()));
            if (cache.TryGetValue(hash, out var result))
                return result;
            var sync = syncMap.GetOrAdd(hash, new object());

            lock (sync)
            {
                // in case t1 and t2 both try to access the sync
                // 1. only 1 will succeed and it will update the cache first
                // 2. the 2nd thread will then retrieve the lock and it will read the assigned response
                if (cache.TryGetValue(hash, out result))
                    return result;
                result = func(arg1, arg2);
                cache.Set(hash, result);
            }
            // clean the sync map
            syncMap.TryRemove(hash, out var _);
            return result;
        };
    }

    public static Func<T1, T2, Task<TResult>> Memoize<T1, T2, TResult>(
        this Func<T1, T2, Task<TResult>> func
    )
    {
        var cache = new WeakDict<string, TResult>();
        var syncMap = new ConcurrentDictionary<string, SemaphoreSlim>();
        return async (arg1, arg2) =>
        {
            var hash = Hash(string.Concat(arg1?.ToString(), arg2?.ToString()));
            if (cache.TryGetValue(hash, out var result))
                return result;

            var sync = syncMap.GetOrAdd(hash, new SemaphoreSlim(1, 1));

            await sync.WaitAsync();
            try
            {
                // in case t1 and t2 both try to access the sync
                // 1. only 1 will succeed and it will update the cache first
                // 2. the 2nd thread will then retrieve the lock and it will read the assigned response
                if (cache.TryGetValue(hash, out result))
                    return result;
                result = await func(arg1, arg2);
                cache.Set(hash, result);
            }
            finally
            {
                sync.Release();
            }
            // clean the sync map
            // even if t3 gets a different sync to t1/t2 it will check if there is data in the cache first and only then attempt to overwrite it
            // the overwrite scenario is fine since it could happen that the weakreference had been collected
            syncMap.TryRemove(hash, out var _);
            return result;
        };
    }
    #endregion

    #region Memo 3 args
    public static Func<T1, T2, T3, TResult> Memoize<T1, T2, T3, TResult>(
        this Func<T1, T2, T3, TResult> func, ICacheRepository repository = null
    )
    {
        repository = repository ?? CacheRepository.Default;
        var cache = repository.GetCache<TResult>();
        var syncMap = new ConcurrentDictionary<string, object>();
        return (arg1, arg2, arg3) =>
        {
            var hash = Hash(string.Concat(arg1?.ToString(), arg2?.ToString(), arg3?.ToString()));
            if (cache.TryGetValue(hash, out var result))
                return result;
            var sync = syncMap.GetOrAdd(hash, new object());

            lock (sync)
            {
                // in case t1 and t2 both try to access the sync
                // 1. only 1 will succeed and it will update the cache first
                // 2. the 2nd thread will then retrieve the lock and it will read the assigned response
                if (cache.TryGetValue(hash, out result))
                    return result;
                result = func(arg1, arg2, arg3);
                cache.Set(hash, result);
            }
            // clean the sync map
            syncMap.TryRemove(hash, out var _);
            return result;
        };
    }

    public static Func<T1, T2, T3, Task<TResult>> MemoizeAsync<T1, T2, T3, TResult>(
        this Func<T1, T2, T3, Task<TResult>> func, ICacheRepository repository = null
    )
    {
        // patch because c# doesn't enable default assignment of ref types
        repository = repository ?? CacheRepository.Default;
        var cache = repository.GetCache<TResult>();
        var syncMap = new ConcurrentDictionary<string, SemaphoreSlim>();
        return async (arg1, arg2, arg3) =>
        {
            var hash = Hash(string.Concat(arg1?.ToString(), arg2?.ToString(), arg3?.ToString()));
            if (cache.TryGetValue(hash, out var result))
            {
                return result;
            }

            var sync = syncMap.GetOrAdd(hash, new SemaphoreSlim(1, 1));

            await sync.WaitAsync();
            try
            {
                // in case t1 and t2 both try to access the sync
                // 1. only 1 will succeed and it will update the cache first
                // 2. the 2nd thread will then retrieve the lock and it will read the assigned response
                if (cache.TryGetValue(hash, out result))
                    return result;
                result = await func(arg1, arg2, arg3);
                cache.Set(hash, result);
            }
            finally
            {
                sync.Release();
            }
            // clean the sync map
            // even if t3 gets a different sync to t1/t2 it will check if there is data in the cache first and only then attempt to overwrite it
            // the overwrite scenario is fine since it could happen that the weakreference had been collected
            syncMap.TryRemove(hash, out var _);
            return result;
        };
    }

    public static Func<T1, T2, T3, TResult> TimedMemoize<T1, T2, T3, TResult>(
        this Func<T1, T2, T3, TResult> func, int ttlInMS, ICacheRepository repository = null
    )
    {
        repository = repository ?? CacheRepository.Default;
        var cache = repository.GetTimedCache<TResult>(ttlInMS);
        var syncMap = new ConcurrentDictionary<string, object>();
        return (arg1, arg2, arg3) =>
        {
            var hash = Hash(string.Concat(arg1?.ToString(), arg2?.ToString(), arg3?.ToString()));
            if (cache.TryGetValue(hash, out var result))
                return result;
            var sync = syncMap.GetOrAdd(hash, new object());

            lock (sync)
            {
                // in case t1 and t2 both try to access the sync
                // 1. only 1 will succeed and it will update the cache first
                // 2. the 2nd thread will then retrieve the lock and it will read the assigned response
                if (cache.TryGetValue(hash, out result))
                    return result;
                result = func(arg1, arg2, arg3);
                cache.Set(hash, result);
            }
            // clean the sync map
            syncMap.TryRemove(hash, out var _);
            return result;
        };
    }

    public static Func<T1, T2, T3, Task<TResult>> TimedMemoizeAsync<T1, T2, T3, TResult>(
        this Func<T1, T2, T3, Task<TResult>> func, int ttl, ICacheRepository repository = null
    )
    {
        // patch because c# doesn't enable default assignment of ref types
        repository = repository ?? CacheRepository.Default;
        var cache = repository.GetTimedCache<TResult>(ttl);
        var syncMap = new ConcurrentDictionary<string, SemaphoreSlim>();
        return async (arg1, arg2, arg3) =>
        {
            var hash = Hash(string.Concat(arg1?.ToString(), arg2?.ToString(), arg3?.ToString()));
            if (cache.TryGetValue(hash, out var result))
            {
                return result;
            }

            var sync = syncMap.GetOrAdd(hash, new SemaphoreSlim(1, 1));

            await sync.WaitAsync();
            try
            {
                // in case t1 and t2 both try to access the sync
                // 1. only 1 will succeed and it will update the cache first
                // 2. the 2nd thread will then retrieve the lock and it will read the assigned response
                if (cache.TryGetValue(hash, out result))
                    return result;
                result = await func(arg1, arg2, arg3);
                cache.Set(hash, result);
            }
            finally
            {
                sync.Release();
            }
            // clean the sync map
            // even if t3 gets a different sync to t1/t2 it will check if there is data in the cache first and only then attempt to overwrite it
            // the overwrite scenario is fine since it could happen that the weakreference had been collected
            syncMap.TryRemove(hash, out var _);
            return result;
        };
    }
    #endregion
}
