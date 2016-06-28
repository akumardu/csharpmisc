using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.Misc
{
    public partial class partialclass
    {
        public string FirstFunc()
        {
            return "FirstFunc";
        }

        partial void PartialMethod(ref int number)
        {
            Console.WriteLine("PartialMethod");
            number = 6;
        }
    }
}
