using static CSharpExtensions.Monads.DataStructures;
using NUnit.Framework;
using Monads.Memoization;

namespace Memoization;

[TestFixture]
public class TimedMemoizeTests
{
    [Test]
    public void Memoize_ThreadSafety_NoMemoryLeaks_NoWastedMemory()
    {
        // Arrange
        int funcCallCount = 0;
        var syncRoot = new object();
        Func<int, int, int, string> func = (arg1, arg2, arg3) =>
        {
            Interlocked.Increment(ref funcCallCount);
            Thread.Sleep(100); // Simulate some work
            return $"{arg1} + {arg2} + {arg3}";
        };

        // create a monitorable cache
        var cache = new TimedWeakDict<string, string>();
        var cacheRepo = new TestCacheRepository()
        {
            TimedCache = cache,
        };

        // memoize + inject the monitorable cache
        var memoizedFunc = func.TimedMemoize(1000, cacheRepo);

        // Act
        var results = new List<string>();
        var tasks = new List<Task>();
        for (int i = 0; i < 1000; i++)
        {
            var task = Task.Run(() =>
            {
                //var result = memoizedFunc(i, i, i);
                var result = memoizedFunc(1, 2, 3);
                lock (syncRoot)
                {
                    results.Add(result);
                }
            });
            tasks.Add(task);
        }
        Task.WaitAll(tasks.ToArray());

        // Assert

        // Verify thread safety
        Assert.AreEqual(1, funcCallCount); // Only one actual function call should have been made

        // Verify no memory leaks and no wasted memory
        var distinctResults = results.Distinct().ToList();
        Assert.AreEqual(1, distinctResults.Count, "Only one distinct result should exist in the list");
        Assert.AreEqual(1000, results.Count, "All 1000 tasks should have produced the same result");

        Assert.IsFalse(cache.IsEmpty);

        results = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsTrue(cache.IsEmpty);
    }

    [Test]
    public void Memoize_Async_ThreadSafety_NoMemoryLeaks_NoWastedMemory()
    {
        // Arrange
        int funcCallCount = 0;
        var syncRoot = new object();
        Func<int, int, int, Task<string>> func = async (arg1, arg2, arg3) =>
        {
            Interlocked.Increment(ref funcCallCount);
            await Task.Delay(100); // Simulate some asynchronous work
            return $"{arg1} + {arg2}+ {arg3}";
        };

        var cache = new TimedWeakDict<string, string>();
        // Assert
        var cachRepo = new TestCacheRepository()
        {
            TimedCache = cache,
        };
        // inject a mock of the cache repository to enable monitoring the cache
        var memoizedFunc = func.TimedMemoizeAsync(10000, cachRepo);

        // Act
        var tasks = new List<Task<string>>();
        for (int i = 0; i < 1000; i++)
        {
            //var task = Task.Run(() => memoizedFunc(i, i, i));
            var task = Task.Run(() => memoizedFunc(1, 2, 3));
            tasks.Add(task);
        }
        Task.WaitAll(tasks.ToArray());

        Assert.IsFalse(cache.IsEmpty);


        // Verify thread safety
        Assert.AreEqual(1, funcCallCount); // Only one actual function call should have been made

        // Ensure all tasks have been garbage collected
        tasks = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsTrue(cache.IsEmpty);
    }

    [Test]
    public void Memoize_ThreadSafety_NoMemoryLeaks_NoWastedMemory_Value_Expires_And_Reassigned()
    {
        // Arrange
        int funcCallCount = 0;
        var syncRoot = new object();
        Func<int, int, int, string> func = (arg1, arg2, arg3) =>
        {
            Interlocked.Increment(ref funcCallCount);
            Thread.Sleep(100); // Simulate some work
            return $"{arg1} + {arg2} + {arg3}";
        };

        // create a monitorable cache
        int ttl = 100;
        var cache = new TimedWeakDict<string, string>(ttl);

        var cacheRepo = new TestCacheRepository()
        {
            TimedCache = cache,
        };

        // memoize + inject the monitorable cache
        var memoizedFunc = func.TimedMemoize(ttl, cacheRepo);

        // Act
        var results = new List<string>();
        Run(syncRoot, memoizedFunc, results);

        // Assert

        // Verify thread safety
        Assert.AreEqual(1, funcCallCount); // Only one actual function call should have been made

        // Verify no memory leaks and no wasted memory
        var distinctResults = results.Distinct().ToList();
        Assert.AreEqual(1, distinctResults.Count, "Only one distinct result should exist in the list");
        Assert.AreEqual(1000, results.Count, "All 1000 tasks should have produced the same result");

        Assert.IsFalse(cache.IsEmpty);

        // wait for the cached values to be counted as expired
        Thread.Sleep(300);

        Assert.IsFalse(cache.IsEmpty);
        Run(syncRoot, memoizedFunc, results);
        Assert.AreEqual(1, distinctResults.Count, "Only one distinct result should exist in the list"); // Only one actual function call should have been made
        Assert.AreEqual(2, funcCallCount, "Cache value should have expired, thus the function should have been invoked a 2nd time");
        Assert.AreEqual(2000, results.Count, "All 1000 tasks should have produced the same result");

        // will mark the weak references values for gc collection, thus triggering the cache cleanup
        results = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsTrue(cache.IsEmpty);
    }

    private static void Run(object syncRoot, Func<int, int, int, string> memoizedFunc, List<string>? results)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < 1000; i++)
        {
            var task = Task.Run(() =>
            {
                //var result = memoizedFunc(i, i, i);
                var result = memoizedFunc(1, 2, 3);
                lock (syncRoot)
                {
                    results.Add(result);
                }
            });
            tasks.Add(task);
        }
        Task.WaitAll(tasks.ToArray());
    }

    [Test]
    public void Memoize_Async_ThreadSafety_NoMemoryLeaks_NoWastedMemory_Value_Expires_And_Reassigned()
    {
        // Arrange
        int funcCallCount = 0;
        var syncRoot = new object();
        Func<int, int, int, Task<string>> func = async (arg1, arg2, arg3) =>
        {
            Interlocked.Increment(ref funcCallCount);
            await Task.Delay(100); // Simulate some asynchronous work
            return $"{arg1} + {arg2}+ {arg3}";
        };

        int ttl = 100;
        var cache = new TimedWeakDict<string, string>(ttl);

        var cacheRepo = new TestCacheRepository()
        {
            TimedCache = cache,
        };

        // memoize + inject the monitorable cache
        var memoizedFunc = func.TimedMemoizeAsync(ttl, cacheRepo);

        // Act
        var tasks = new List<Task<string>>();
        for (int i = 0; i < 1000; i++)
        {
            //var task = Task.Run(() => memoizedFunc(i, i, i));
            var task = Task.Run(() => memoizedFunc(1, 2, 3));
            tasks.Add(task);
        }
        Task.WaitAll(tasks.ToArray());

        Assert.IsFalse(cache.IsEmpty);


        // Verify thread safety
        Assert.AreEqual(1, funcCallCount); // Only one actual function call should have been made

        // wait for the cached values to be counted as expired
        Thread.Sleep(300);

        Assert.IsFalse(cache.IsEmpty);


        for (int i = 0; i < 1000; i++)
        {
            //var task = Task.Run(() => memoizedFunc(i, i, i));
            var task = Task.Run(() => memoizedFunc(1, 2, 3));
            tasks.Add(task);
        }
        Assert.AreEqual(1, tasks.Select(t => t.Result).Distinct().Count(), "Only one distinct result should exist in the list"); // Only one actual function call should have been made
        Assert.AreEqual(2, funcCallCount, "Cache value should have expired, thus the function should have been invoked a 2nd time");
        Assert.AreEqual(2000, tasks.Count, "All 1000 tasks should have produced the same result");

        // will mark the weak references values for gc collection, thus triggering the cache cleanup
        tasks = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsTrue(cache.IsEmpty);
    }
}

