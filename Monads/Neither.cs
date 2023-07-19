namespace CSharpExtensions.Monads;

public class Neither<L, R>
{
    private readonly L? _left;
    private readonly R? _right;
    private readonly object? _neither;
    private readonly EitherType _type;
    public Neither(object neither)
    {
        _neither = neither;
        _type = EitherType.Neither;
    }
    public Neither(L left)
    {
        _left = left;
        _type = EitherType.Left;
    }
    public Neither(R right)
    {
        _right = right;
        _type = EitherType.Right;
    }

    private enum EitherType
    {
        Neither,
        Left,
        Right
    }

    public T Match<T>(Func<L, T> leftMatcher, Func<R, T> rightMatcher, Func<object, T> neitherMatcher)
    {
        return _type switch
        {
            EitherType.Neither => neitherMatcher(_neither!),
            EitherType.Left => leftMatcher(_left!),
            EitherType.Right => rightMatcher(_right!),
            _ => throw new InvalidOperationException(),
        };
    }
}