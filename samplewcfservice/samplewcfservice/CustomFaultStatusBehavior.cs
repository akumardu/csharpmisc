using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace samplewcfservice
{
    /// <summary>
    /// Endpoint Behavior that allows returning custom HTTP response codes for SOAP Faults 
    /// </summary>
    public class CustomFaultStatusBehavior : BehaviorExtensionElement, IEndpointBehavior
    {
        //based on https://msdn.microsoft.com/es-es/library/ee844556(v=vs.95).aspx
        public override Type BehaviorType
        {
            get
            {
                return typeof(CustomFaultStatusBehavior);
            }
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            CustomFaultStatusMessageInspector inspector = new CustomFaultStatusMessageInspector();
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        protected override object CreateBehavior()
        {
            return new CustomFaultStatusBehavior();
        }
    }

    /// <summary>
    /// Message Inspector that updates the HTTP response code for faulted messages with a CustomFaultStatus action
    /// </summary>
    public class CustomFaultStatusMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (!reply.IsFault) return;
            if (!reply.Headers.Action.StartsWith("CustomFaultStatus", StringComparison.Ordinal)) return;
            //get the string value for desired response code
            string statusCodeString = reply.Headers.Action.Substring(17);
            //convert to int
            int statusCodeInt;
            if (!int.TryParse(statusCodeString, out statusCodeInt)) return;

            //cast to HttpStatusCode
            System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.InternalServerError;
            try
            {
                statusCode = (System.Net.HttpStatusCode)statusCodeInt;
            }
            catch (Exception ex)
            {
                return;
            }

            // Here the response code is changed
            reply.Properties[HttpResponseMessageProperty.Name] = new HttpResponseMessageProperty() { StatusCode = statusCode };
        }
    }

}
