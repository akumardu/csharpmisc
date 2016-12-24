using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.Dynamic
{
    public static class CommonMathDynamicWithGenericsExplicitCast
    {
        public static T Add<T>(T a, T b)
        {
            dynamic result = (dynamic)a + b;
            return (T)result;
        }
    }
}
