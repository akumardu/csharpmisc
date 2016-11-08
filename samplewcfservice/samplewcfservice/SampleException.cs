using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace samplewcfservice
{
    [DataContract]
    public class SampleException
    {
        private string message;
        public SampleException(string m)
        {
            message = m;
        }

        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
