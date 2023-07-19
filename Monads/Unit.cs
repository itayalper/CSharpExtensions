using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpExtensions.Monads
{
    public struct Unit
    {
        public static readonly Unit Default = new Unit();
    }
}
