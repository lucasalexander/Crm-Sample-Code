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
using System.Collections.Generic;

namespace CrmAzureMlDemo
{
    public sealed class ExecuteModel : CodeActivity
    {
        [Input("MaritalStatus")]
        public InArgument<String> MaritalStatus { get; set; }

        [Input("StateOrProvince")]
        public InArgument<String> StateOrProvince { get; set; }

        [Input("CommuteDistance")]
        public InArgument<String> CommuteDistance { get; set; }

        [Input("Gender")]
        public InArgument<String> Gender { get; set; }

        [Input("Homeowner")]
        public InArgument<String> Homeowner { get; set; }

        [Input("NumChildren")]
        public InArgument<Int32> NumChildren { get; set; }

        [Input("NumCars")]
        public InArgument<Int32> NumCars { get; set; }

        [Input("NumChildrenAtHome")]
        public InArgument<Int32> NumChildrenAtHome { get; set; }

        [Input("Occupation")]
        public InArgument<String> Occupation { get; set; }

        [Input("Age")]
        public InArgument<Int32> Age { get; set; }

        [Input("AnnualIncome")]
        public InArgument<Decimal> AnnualIncome { get; set; }

        [Input("Education")]
        public InArgument<String> Education { get; set; }

        [Input("ApiKey")]
        public InArgument<String> ApiKey { get; set; }

        [Input("Endpoint")]
        public InArgument<String> Endpoint { get; set; }

        [Output("BikeBuyer")]
        public OutArgument<String> BikeBuyer { get; set; }

        //name of your custom workflow activity for tracing/error logging
        private string _activityName = "ExecuteModel";

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
                //create a new myjsonrequest object from which data will be serialized
                JsonRequest myRequest = new JsonRequest();
                myRequest.InputObj = new CrmAzureMlDemo.Input1();
                Input input = new Input();

                string[] columns = {"address1_stateorprovince", "annualincome", "lpa_age",
                    "numberofchildren", "educationcodename", "familystatuscodename",
                    "gendercodename", "lpa_commutedistancename", "lpa_homeownername",
                    "lpa_occupationname", "lpa_numberofcarsowned", "lpa_numberofchildrenathome" };

                object[] values = {StateOrProvince.Get(executionContext), AnnualIncome.Get(executionContext), Age.Get(executionContext),
                    NumChildren.Get(executionContext), Education.Get(executionContext), MaritalStatus.Get(executionContext),
                    Gender.Get(executionContext), CommuteDistance.Get(executionContext), Homeowner.Get(executionContext),
                    Occupation.Get(executionContext), NumCars.Get(executionContext), NumChildrenAtHome.Get(executionContext) };

                input.Columns = columns;
                input.Values = new object[][] { values };

                myRequest.InputObj.Inputs = new Input();
                myRequest.InputObj.Inputs = input;

                //serialize the myjsonrequest to json
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(myRequest.GetType());
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, myRequest);
                string jsonMsg = Encoding.Default.GetString(ms.ToArray());

                //create the webrequest object and execute it (and post jsonmsg to it)
                System.Net.WebRequest req = System.Net.WebRequest.Create(Endpoint.Get(executionContext));

                //must set the content type for json
                req.ContentType = "application/json";

                //must set method to post
                req.Method = "POST";

                //add authorization header
                req.Headers.Add(string.Format("Authorization:Bearer {0}", ApiKey.Get(executionContext)));

                tracingService.Trace("json request: {0}", jsonMsg);

                //create a stream
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(jsonMsg.ToString());
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream();
                os.Write(bytes, 0, bytes.Length);
                os.Close();

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

                //set output values from the fields of the deserialzed myjsonresponse object
                BikeBuyer.Set(executionContext, myResponse.Results.Output1.Value.Values[0][0]);
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