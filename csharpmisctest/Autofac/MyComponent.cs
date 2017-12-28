using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisctest.Autofac
{
    public class MultiConstrSameParamComponent : IComponent
    {
        public MultiConstrSameParamComponent() : this("param1", "param2")
        {

        }

        public MultiConstrSameParamComponent(string param1) : this(param1, "param2")
        {

        }

        public MultiConstrSameParamComponent(string param1, string param2)
        {
            Param1 = param1;
            Param2 = param2;
        }

        public string Param1 { get; }
        public string Param2 { get; }

        public string SomeRandomMethod1(string param1, string param2)
        {
            Console.WriteLine($"{this.GetType().Name} + {param1} + {param2}");
            return param1 + param2;
        }
    }
}
