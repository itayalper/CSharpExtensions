namespace Monads
{
    public interface Maybe<T>
    {
    }

    public sealed class Nothing<T> : Maybe<T> { }
    public sealed class Just<T> : Maybe<T>
    {
        public readonly T Value;
        public Just(T value) => Value = value;
    }
}
