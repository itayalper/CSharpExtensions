using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSharpExtensions.Monads.DataStructures;

namespace Memoization
{
    public interface ICacheRepository
    {
        internal WeakDict<string, TResult> GetCache<TResult>();
        internal WeakDict<T1, TResult> GetCacheSingle<T1, TResult>();


        internal TimedWeakDict<string, TResult> GetTimedCache<TResult>(int ttl);
        internal TimedWeakDict<T1, TResult> GetTimedCacheSingle<T1, TResult>(int ttl);
    }

    public class CacheRepository: ICacheRepository
    {
        public static CacheRepository Default = new CacheRepository();

        WeakDict<string, TResult> ICacheRepository.GetCache<TResult>()
        {
            return new WeakDict<string, TResult>();
        }

        WeakDict<T1, TResult> ICacheRepository.GetCacheSingle<T1, TResult>()
        {
            return new WeakDict<T1, TResult>();
        }

        TimedWeakDict<string, TResult> ICacheRepository.GetTimedCache<TResult>(int ttl)
        {
            return new TimedWeakDict<string, TResult>(ttl);
        }

        TimedWeakDict<T1, TResult> ICacheRepository.GetTimedCacheSingle<T1, TResult>(int ttl)
        {
            return new TimedWeakDict<T1, TResult>(ttl);
        }
    }

    internal class TestCacheRepository: ICacheRepository
    {

        public dynamic Cache;
        public dynamic CacheSingle;


        public dynamic TimedCache;
        public dynamic TimedCacheSingle;


        WeakDict<string, TResult> ICacheRepository.GetCache<TResult>() 
        {
            return Cache != null ? (WeakDict<string, TResult>)Cache : new WeakDict<string, TResult>();
        }

        WeakDict<T1, TResult> ICacheRepository.GetCacheSingle<T1, TResult>()
        {
            return CacheSingle != null ? CacheSingle : new WeakDict<T1, TResult>();
        }


        TimedWeakDict<string, TResult> ICacheRepository.GetTimedCache<TResult>(int ttl)
        {
            return TimedCache != null ? TimedCache : new TimedWeakDict<string, TResult>(ttl);
        }

        TimedWeakDict<T1, TResult> ICacheRepository.GetTimedCacheSingle<T1, TResult>(int ttl)
        {
            return TimedCacheSingle != null ? TimedCacheSingle : new TimedWeakDict<T1, TResult>(ttl);
        }
    }
}
