using System.Collections.Concurrent;

namespace CSharpExtensions.Monads;

public static partial class DataStructures
{

    private class OnFinalise<T>
    {
        public readonly T Value;
        readonly Action OnFinaliseCallback;

        public OnFinalise(T value, Action onFinaliseCallback)
        {
            Value = value;
            OnFinaliseCallback = onFinaliseCallback;
        }

        ~OnFinalise() => OnFinaliseCallback?.Invoke();
    }

    /// <summary>
    /// <para>A thread-safe dictionary which also has no memory leaks.</para>
    /// <para>Ideally used for caching.</para>
    /// </summary>
    /// <typeparam name="T">Key Type</typeparam>
    /// <typeparam name="U">Value Type</typeparam>
    public class WeakDict<T, U>
        where T : notnull
    {
        private readonly ConcurrentDictionary<T, WeakReference<OnFinalise<U>>> _cache;

        public WeakDict()
        {
            _cache = new();
        }

        public void Set(T key, U value)
        {
            // creates a new value which will trigger a cache clear once the value finalises
            var data = new WeakReference<OnFinalise<U>>(
                new OnFinalise<U>(value, () =>
                {
                    _cache.TryRemove(key, out var _);
                })
            );
            // insert or update the cached value
            _cache.AddOrUpdate(key, data, (key, value) => data);
        }

        public bool TryGetValue(T key, out U value)
        {
            // verify if we have a value, and if that value is still alive
            if (_cache.TryGetValue(key, out var weakRef) && weakRef.TryGetTarget(out var v))
            {
                value = v.Value;
                return true;
            }
            // value doesn't exist
            value = default;
            return false;
        }

        public bool IsEmpty => _cache.IsEmpty;
    }
    public class TimedWeakDict<TKey, TValue>
    {

        private record TimedValue<T>
        {
            internal readonly T Value;
            internal readonly long CreatedAt;

            public TimedValue(T value, long createdAt)
            {
                Value = value;
                CreatedAt = createdAt;
            }
        }

        private ConcurrentDictionary<TKey, WeakReference<OnFinalise<TimedValue<TValue>>>> _cache;

        // in MS
        public readonly int TTL;

        public TimedWeakDict(int ttl = 2000)
        {
            _cache = new();
            TTL = ttl;
        }

        public void Set(TKey key, TValue value)
        {
            var createdAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var finalizer = new OnFinalise<TimedValue<TValue>>(new TimedValue<TValue>(value, createdAt), () =>
            {
                _cache.TryRemove(key, out var ignore);
            });

            var newWeakRef = new WeakReference<OnFinalise<TimedValue<TValue>>>(finalizer);

            _cache.AddOrUpdate(key, newWeakRef, (k, oldWeakRef) =>
            {
                if (oldWeakRef.TryGetTarget(out var existingValue)) return existingValue.Value.CreatedAt + TTL > createdAt ? oldWeakRef : newWeakRef;
                return newWeakRef;
            });
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var weakRef) && weakRef.TryGetTarget(out var result))
            {
                var current = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var old = result.Value.CreatedAt;
                var delta = current - old;
                if (delta < TTL)
                {
                    value = result.Value.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        public bool IsEmpty => _cache.IsEmpty;
    }
}
