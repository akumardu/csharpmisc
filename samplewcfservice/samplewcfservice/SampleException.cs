using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace samplewcfservice
{
    public class SampleException
    {
        public string message;
        public SampleException(string m)
        {
            message = m;
        }
    }
}
