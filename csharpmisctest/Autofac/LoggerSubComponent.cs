using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisctest.Autofac
{
    public class LoggerSubComponent : ISubComponent
    {
        public LoggerSubComponent() : this(0)
        {

        }

        public LoggerSubComponent(int member1)
        {
            Member1 = member1;
        }

        public int Member1 { get; }

        public string RandomSubComponentMethod(string param1, string param2)
        {
            Console.WriteLine($"{this.GetType().Name} + {param1} + {param2} + {Member1}");
            return this.GetType().Name;
        }
    }
}
