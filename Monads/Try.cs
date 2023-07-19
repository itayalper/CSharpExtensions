using System;

namespace CSharpExtensions.Monads
{
    public sealed class Try<T>
    {
        public readonly Either<Exception, T> Result;

        public Try(Func<Either<Exception, T>> tryFunc)
        {
            try
            {
                Result = tryFunc();
            }
            catch (Exception ex)
            {
                Result = ex;
            }
        }

        public Try<T1> Select<T1>(Func<T, T1> func)
        {
            return Result.Match(
                (ex) => new Try<T1>(() => ex),
                (result) => new Try<T1>(() => func(result))
            );
        }

        public static implicit operator Try<T>(Func<Either<Exception, T>> tryFunc) => new(tryFunc);
    }

    public sealed class Try
    {
        public readonly Either<Exception, Unit> Result;

        public Try(Func<Either<Exception, Unit>> tryFunc)
        {
            try
            {
                Result = tryFunc();
            }
            catch (Exception ex)
            {
                Result = ex;
            }
        }

        public Try<T1> Select<T1>(Func<Unit, T1> func)
        {
            return Result.Match(
                (ex) => new Try<T1>(() => ex),
                (result) => new Try<T1>(() => func(result))
            );
        }

        public static implicit operator Try(Func<Either<Exception, Unit>> tryFunc) => new(tryFunc);
    }
}

