using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.Reflection
{
    public class SpeedTestForReflection
    {
        public long CreateNormalList(int iterations)
        {
            var timer = Stopwatch.StartNew();
            for(int i = 0; i < iterations; i++)
            {
                var list = new List<int>();
            }

            return timer.ElapsedMilliseconds;
        }

        public long CreateReflectedList(int iterations)
        {
            Type listType = typeof(List<int>);
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var list = Activator.CreateInstance(listType);
            }

            return timer.ElapsedMilliseconds;
        }

        public long AddToNormalList(int iterations)
        {
            var list = new List<int>();
            var timer = Stopwatch.StartNew();
            for(int i = 0; i < iterations; i++)
            {
                list.Add(i);
            }

            return timer.ElapsedMilliseconds;
        }

        public long AddToReflectedList(int iterations)
        {
            var list = new List<int>();
            Type listType = typeof(List<int>);
            Type[] parameterTypes = { typeof(int) };
            MethodInfo mi = listType.GetMethod("Add", parameterTypes);
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                mi.Invoke(list, new object[] { i });
            }

            return timer.ElapsedMilliseconds;
        }
    }
}
