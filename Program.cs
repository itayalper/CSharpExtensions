using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using CSharpExtensions.Monads;
using Monads.Memoization;

public class Program
{
    public static decimal Calculate(decimal a, long b, string c)
    {
        var either = ValidateInput(a);
        var result = either.Match(floatA => a / b, _ => a * b);
        return result;
    }
    public static decimal? Calculate1(decimal a, long b, string c)
    {
        var either = ValidateInput(a);
        var result = either.Match(floatA => a / b, _ => a * b);
        return result;
    }

    private static Either<float, int> ValidateInput(decimal dec)
    {
        return dec > 0 ? (int)dec : (float)dec;
    }

    private static Either<Exception, decimal> CalculateNumDivivedByZero(int n) => n / 0;

    public static void Main(string[] args)
    {
        //var a = Calculate1(1, 1, "1");
        //var bla = new Try<decimal>(() => CalculateNumDivivedByZero(100)).Result.Match((err) => default, (result) => result.Scale);

        //var resource = new Resource();
        //var blaUsing = new Using<Resource>(resource, (res) =>
        //{
        //    //Console.WriteLine(res);
        //});

        //var blaUsing1 = new Using<Resource, int>(resource, (res) =>
        //{
        //    return 1;
        //}).Result;

        //int iterator = 10;
        var config = DefaultConfig.Instance.WithOptions(ConfigOptions.DisableOptimizationsValidator);
        BenchmarkRunner.Run<MemoizeBenchmarks>(config);




        //var aa = new While<int>(() => iterator > 0, (a) => { iterator-- = 1; });

        //bla.Match(() 

        // var memoCalculate = a.Memoize();
    }

    [RPlotExporter]
    public class MemoizeBenchmarks
    {
        HashSet<string> s1 = new HashSet<string>();
        HashSet<string> s2 = new HashSet<string>();

        static Func<int, string, bool, string> func;

        static Func<int, string, bool, string> memoFunc;

        [GlobalSetup]
        public void IterationSetup()
        {
            func = (int arg1, string arg2, bool arg3) =>
            {
                Thread.Sleep(100);
                return $"{arg1.GetHashCode()} + {arg2.Length}+ {(arg3 ? 1 : 0)}";
            };
            memoFunc = func.Memoize();
        }

        public MemoizeBenchmarks()
        {
            //memoFunc = func.Memoize();
        }

        [Benchmark]
        public void MemoizeBenchmark()
        {
            
            // Your code to execute the Memoize function goes here
            // Make sure to provide appropriate arguments for the function
            // For example:
            s1.Add(memoFunc(1, "example", true));
        }
        [Benchmark]
        public void BenchmarkReg()
        {
           
            // Your code to execute the Memoize function goes here
            // Make sure to provide appropriate arguments for the function
            // For example:
            s2.Add(func(1, "example", true));
        }
    }


    private class Resource : IDisposable
    {
        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}