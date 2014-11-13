using System;
using System.Activities;
using System.ServiceModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using System.Net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace LucasJsonPlugins
{
    public sealed class DoSomething : CodeActivity
    {
        //generic inputs - yours will obviously be different
        [Input("Input #1")]
        public InArgument<String> Input1 { get; set; }

        [Input("Input #2")]
        public InArgument<String> Input2 { get; set; }

        //generic outputs - yours will obviously be different
        [Output("Output #1")]
        public OutArgument<String> Output1 { get; set; }

        [Output("Output #2")]
        public OutArgument<String> Output2 { get; set; }

        //address of the service to which you will post your json messages
        private string _webAddress = "https://PATH_TO_SERVICE";
        
        //name of your custom workflow activity for tracing/error logging
        private string _activityName = "DoSomething";

        /// <summary>
        /// Executes the workflow activity.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        protected override void Execute(CodeActivityContext executionContext)
        {
            // Create the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            if (tracingService == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve tracing service.");
            }

            tracingService.Trace("Entered "+_activityName+".Execute(), Activity Instance Id: {0}, Workflow Instance Id: {1}",
                executionContext.ActivityInstanceId,
                executionContext.WorkflowInstanceId);

            // Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();

            if (context == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve workflow context.");
            }

            tracingService.Trace(_activityName + ".Execute(), Correlation Id: {0}, Initiating User: {1}",
                context.CorrelationId,
                context.InitiatingUserId);

            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                //create a new myjsonrequest object from which data will be serialized
                MyJsonRequest myRequest = new MyJsonRequest
                {
                    Input1 = Input1.Get(executionContext),
                    Input2 = Input2.Get(executionContext)                    
                };

                //serialize the myjsonrequest to json
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(myRequest.GetType());
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, myRequest);
                string jsonMsg = Encoding.Default.GetString(ms.ToArray());

                //create the webrequest object and execute it (and post jsonmsg to it)
                System.Net.WebRequest req = System.Net.WebRequest.Create(_webAddress);

                //must set the content type for json
                req.ContentType = "application/json";

                //must set method to post
                req.Method = "POST";

                //create a stream
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(jsonMsg.ToString());
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream();
                os.Write(bytes, 0, bytes.Length);
                os.Close();

                //get the response
                System.Net.WebResponse resp = req.GetResponse();

                //deserialize the response to a myjsonresponse object
                MyJsonResponse myResponse = new MyJsonResponse();
                System.Runtime.Serialization.Json.DataContractJsonSerializer deserializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(myResponse.GetType());
                myResponse = deserializer.ReadObject(resp.GetResponseStream()) as MyJsonResponse;

                //set output values from the fields of the deserialzed myjsonresponse object
                Output1.Set(executionContext, myResponse.Output1);
                Output2.Set(executionContext, myResponse.Output2);

            }

            catch (WebException exception)
            {
                string str = string.Empty;
                if (exception.Response != null)
                {
                    using (StreamReader reader =
                        new StreamReader(exception.Response.GetResponseStream()))
                    {
                        str = reader.ReadToEnd();
                    }
                    exception.Response.Close();
                }
                if (exception.Status == WebExceptionStatus.Timeout)
                {
                    throw new InvalidPluginExecutionException(
                        "The timeout elapsed while attempting to issue the request.", exception);
                }
                throw new InvalidPluginExecutionException(String.Format(CultureInfo.InvariantCulture,
                    "A Web exception ocurred while attempting to issue the request. {0}: {1}",
                    exception.Message, str), exception);
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                tracingService.Trace("Exception: {0}", e.ToString());

                // Handle the exception.
                throw;
            }
            catch (Exception e)
            {
                tracingService.Trace("Exception: {0}", e.ToString());
                throw;
            }

            tracingService.Trace("Exiting " + _activityName + ".Execute(), Correlation Id: {0}", context.CorrelationId);
        }
    }

}