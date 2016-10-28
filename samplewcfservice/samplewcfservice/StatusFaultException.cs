using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace samplewcfservice
{
    class StatusFaultException<T> : FaultException<T>
    {
        public StatusFaultException(T detail, System.Net.HttpStatusCode statusCode, string faultReason, FaultCode faultCode)
        : base(detail, faultReason, faultCode, "CustomFaultStatus" + ((int)statusCode).ToString())
        {
            //StatusCode is placed in the response Action. Action would be "CustomFaultStatus503" to return a 503 error code
        }


    }
}
