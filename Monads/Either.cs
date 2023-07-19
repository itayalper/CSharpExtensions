namespace CSharpExtensions.Monads;

public class Either<Left, Right>
{
    internal readonly Left? _left;
    internal readonly Right? _right;
    private readonly EitherType _eitherType;

    public Either(Left left)
    {
        _left = left;
        _eitherType = EitherType.Left;
    }

    public Either(Right right)
    {
        _right = right;
        _eitherType = EitherType.Right;
    }

    public bool IsRight => _eitherType == EitherType.Right;
    public bool IsLeft => _eitherType == EitherType.Left;

    private enum EitherType
    {
        Neither,
        Left,
        Right
    }

    public T Match<T>(Func<Left, T> leftMatch, Func<Right, T> rightMatch) =>
        _eitherType == EitherType.Left ? leftMatch(_left!) : rightMatch(_right!);

    public static implicit operator Either<Left, Right>(Left left) => new(left);

    public static implicit operator Either<Left, Right>(Right right) => new(right);
}