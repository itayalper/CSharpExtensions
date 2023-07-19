using static CSharpExtensions.Monads.DataStructures;
using NUnit.Framework;
using Monads.Memoization;

namespace Memoization;

[TestFixture]
public class MemoizeTests
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
        var cache = new WeakDict<string, string>();
        var cacheRepo = new TestCacheRepository()
        {
            Cache = cache,
        };

        // memoize + inject the monitorable cache
        var memoizedFunc = func.Memoize(cacheRepo);

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

        var cache = new WeakDict<string, string>();
        // Assert
        var cachRepo = new TestCacheRepository()
        {
            Cache = cache,
        };
        // inject a mock of the cache repository to enable monitoring the cache
        var memoizedFunc = func.MemoizeAsync(cachRepo);

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
}

