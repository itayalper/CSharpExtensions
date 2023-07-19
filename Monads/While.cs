using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpExtensions.Monads
{
    public class While
    {
        public While(Func<bool> predicate, Action iterate)
        {
            while (predicate())
            {
                iterate();
            }
        }
    }

    public class While<T>
    {
        public readonly T Result;

        public While(Func<bool> predicate, Func<T> iterate)
        {
            while (predicate())
            {
                Result = iterate();
            }
        }
    }
}
