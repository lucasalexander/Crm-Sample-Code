using System;
using System.Activities;
using System.ServiceModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using System.Net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Crm.Sdk.Messages;
using System.Collections.Generic;

namespace LicensePlateDemo
{
    public sealed class CheckPlate : CodeActivity
    {
        [Input("Endpoint")]
        public InArgument<String> Endpoint { get; set; }

        [Output("PlateFound")]
        public OutArgument<bool> PlateFound { get; set; }

        [Output("PlateNum")]
        public OutArgument<String> PlateNum { get; set; }

        [Output("ImgUrl")]
        public OutArgument<String> ImgUrl { get; set; }

        [Output("ContactId")]
        public OutArgument<String> ContactId { get; set; }

        [Output("ContactName")]
        public OutArgument<String> ContactName { get; set; }

        [Output("ContactFound")]
        public OutArgument<bool> ContactFound { get; set; }

        //name of your custom workflow activity for tracing/error logging
        private string _activityName = "CheckPlate";

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

            tracingService.Trace("Entered " + _activityName + ".Execute(), Activity Instance Id: {0}, Workflow Instance Id: {1}",
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
                //set default outputs to false, blank, whatever
                PlateFound.Set(executionContext, false);
                ContactFound.Set(executionContext, false);
                ContactId.Set(executionContext, Guid.Empty.ToString());
                ContactName.Set(executionContext, string.Empty);
                PlateNum.Set(executionContext, string.Empty);
                ImgUrl.Set(executionContext, string.Empty);

                //create the webrequest object and execute it
                System.Net.WebRequest req = System.Net.WebRequest.Create(Endpoint.Get(executionContext));

                //must set the content type for json
                req.ContentType = "application/json";

                //must set method to post
                req.Method = "GET";

                //get the response
                System.Net.WebResponse resp = req.GetResponse();

                Stream responseStream = CopyAndClose(resp.GetResponseStream());
                // Do something with the stream
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                String responseString = reader.ReadToEnd();
                tracingService.Trace("json response: {0}", responseString);

                responseStream.Position = 0;
                //deserialize the response to a myjsonresponse object
                JsonResponse myResponse = new JsonResponse();
                System.Runtime.Serialization.Json.DataContractJsonSerializer deserializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(myResponse.GetType());
                myResponse = deserializer.ReadObject(responseStream) as JsonResponse;

                if (myResponse.Results.Length > 0)
                {
                    //plate detected
                    string platenum = myResponse.Results[0].Plate;
                    PlateFound.Set(executionContext, true);
                    PlateNum.Set(executionContext, platenum);

                    //set image path
                    var uri = new Uri(Endpoint.Get(executionContext));
                    string image = string.Format("{0}://{1}:{2}{3}", uri.Scheme, uri.Host, uri.Port, myResponse.Image);

                    ImgUrl.Set(executionContext, image);

                    //search for the contact
                    //this should probably search for other matches if it can't find a result for the "best" match
                    tracingService.Trace("entering 'contact search'", "");
                    tracingService.Trace("building fetch", "");
                    string fetchXml = @"<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>
                        <entity name='contact'>
                            <attribute name='fullname'/>
                            <filter type='and'>
                                <condition attribute='lpa_platenumber' value='{0}' operator='eq'/>
                            </filter>
                        </entity>
                    </fetch> ";

                    fetchXml = string.Format(fetchXml, platenum);

                    tracingService.Trace("prepared fetchxml: {0}", fetchXml);

                    tracingService.Trace("retrieving contacts", "");
                    EntityCollection contacts = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (contacts.Entities.Count == 1)
                    {
                        ContactFound.Set(executionContext, true);
                        ContactId.Set(executionContext, contacts.Entities[0].Id.ToString());
                        ContactName.Set(executionContext, contacts.Entities[0]["fullname"].ToString());
                    }
                }
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

        private static Stream CopyAndClose(Stream inputStream)
        {
            const int readSize = 256;
            byte[] buffer = new byte[readSize];
            MemoryStream ms = new MemoryStream();

            int count = inputStream.Read(buffer, 0, readSize);
            while (count > 0)
            {
                ms.Write(buffer, 0, count);
                count = inputStream.Read(buffer, 0, readSize);
            }
            ms.Position = 0;
            inputStream.Close();
            return ms;
        }
    }

}