using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.DesignPatterns.SingletonPattern
{
    public class SimpleSingleton
    {
        // .NET runtime ensures that this is thread safe
        // This gets initialized on first use (even on first static method use)
        private static readonly SimpleSingleton instance = new SimpleSingleton();

        static SimpleSingleton() { }

        private SimpleSingleton()
        {

        }

        // Calling this method forces instantiation
        public static void SayHi()
        {
            Console.WriteLine("Hi There");
        }

        public static SimpleSingleton Instance
        {
            get
            {
                return instance;
            }
        }

    }
}
