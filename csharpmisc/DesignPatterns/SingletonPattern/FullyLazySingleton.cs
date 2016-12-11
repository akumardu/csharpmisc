using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.DesignPatterns.SingletonPattern
{
    public class FullyLazySingleton
    {
        private static readonly Lazy<FullyLazySingleton> lazyInstance = new Lazy<FullyLazySingleton>(() => new FullyLazySingleton(), true);

        private FullyLazySingleton()
        {
            
        } 

        public static FullyLazySingleton Instance => lazyInstance.Value;
    }
}
