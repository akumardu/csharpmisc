using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.Misc
{
    public partial class partialclass
    {
        public string SecondFunc()
        {
            return "SecondFunc";
        }

        public void CallPartialMethod(ref int number)
        {
            PartialMethod(ref number);
        }

        partial void PartialMethod(ref int number);
    }
}
