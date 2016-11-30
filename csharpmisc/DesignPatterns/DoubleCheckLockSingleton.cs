using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.DesignPatterns
{
    public class DoubleCheckLockSingleton
    {
        private static readonly object mutex = new object();

        // Memory issues when DoubleCheckLockSingleton is volatile 
        private static DoubleCheckLockSingleton instance;

        private DoubleCheckLockSingleton()
        {
            // initialize stuff
        }

        public static DoubleCheckLockSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (mutex)
                    {
                        if (instance == null)
                        {
                            instance = new DoubleCheckLockSingleton();
                        }
                    }
                }

                return instance;
            }
        }
    }
}
