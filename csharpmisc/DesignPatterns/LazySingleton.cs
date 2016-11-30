using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.DesignPatterns
{
    public class LazySingleton
    {
        private class SingletonHolder
        {
            internal static readonly LazySingleton instance = new LazySingleton();

            // Empty static constructor - Forces laziness
            static SingletonHolder() { }
        }

        private LazySingleton()
        {
            Console.WriteLine("Singleton Constructor");
        }

        public static LazySingleton Instance => SingletonHolder.instance;

        // Calling this method won't force instantiation
        public static void SayHi()
        {
            Console.WriteLine("Hi There");
        }
    }
}
