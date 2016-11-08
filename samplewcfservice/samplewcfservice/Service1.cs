using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace samplewcfservice
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                throw new FaultException<SampleException>(new SampleException("BoolValue is good"),"BoolValue is good");
            }
            else
            {
                throw new StatusFaultException<SampleException>(new SampleException("BoolValue is bad"), System.Net.HttpStatusCode.BadRequest, "BoolValue is bad", new FaultCode("Sender"));
            }
            return composite;
        }
    }
}
