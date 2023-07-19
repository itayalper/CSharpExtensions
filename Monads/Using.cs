namespace CSharpExtensions.Monads
{
    public class Using<T>
        where T : IDisposable
    {
        private readonly Try _result;
        public Either<Exception, Unit> Result => _result.Result;

        public Using(T resource, Action<T> usingCb)
        {
            using (resource)
            {
                var @try = new Try(() => Inner(resource, usingCb));
                _result = @try;
            }
        }

        private static Either<Exception, Unit> Inner(T resource, Action<T> usingCb)
        {
            usingCb(resource);
            return Unit.Default;
        }
    }

    public class Using<T, U>
        where T : IDisposable
    {
        private readonly Try<U> _result;
        public Either<Exception, U> Result => _result.Result;

        public Using(T resource, Func<T, U> usingCb)
        {
            using (resource)
            {
                var @try = new Try<U>(() => Inner(resource, usingCb));
                _result = @try;
            }
        }

        private static Either<Exception, U> Inner(T resource, Func<T, U> usingCb)
        {
            return usingCb(resource);
        }
    }
}

