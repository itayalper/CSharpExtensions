
namespace Monads
{
    public static class MaybeExtensions
    {
        private static Just<T> Just<T>(T value) => new Just<T>(value);
        private static Nothing<T> Nothing<T>() => new Nothing<T>();

        public static Maybe<T> ToMaybe<T>(this T? self)
            where T : struct
            => self.HasValue ? Just(self.Value) : Nothing<T>();

        public static Maybe<U> Transform<T, U>(this Maybe<T> maybe, Func<T, Just<U>> justCb, Func<Nothing<U>> nothingCb)
            where T : struct
            =>
            maybe == null ? nothingCb()
            : maybe is Nothing<T> ? nothingCb()
            : maybe is Just<T> just ? justCb(just.Value)
            : throw new ArgumentException();

        public static T? IfJust<T>(this T? self, Action<T> cb)
            where T : struct
        {
            if (self.HasValue)
            {
                cb?.Invoke(self.Value);
            }
            return self;
        }

        public static T? IfJust<T, U>(this T? self, Func<T, U> cb)
            where T : struct
        {
            if (self.HasValue)
            {
                cb?.Invoke(self.Value);
            }
            return self;
        }

        public static T? IfNothing<T>(this T? self, Action cb)
            where T : struct
        {
            if (!self.HasValue)
            {
                cb?.Invoke();
            }
            return self;
        }

        public static T? IfNothing<T>(this T? self, Func<T> cb)
            where T : struct
        {
            if (!self.HasValue)
            {
                cb?.Invoke();
            }
            return self;
        }
    }
}
