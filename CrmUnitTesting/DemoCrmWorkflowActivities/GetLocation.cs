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
using System.Xml;
using System.Xml.XPath;

namespace DemoCrmWorkflowActivities
{
    public class GetLocation : CodeActivity
    {
        [Input("Bing key")]
        public InArgument<String> BingKey { get; set; }

        [Input("Country")]
        public InArgument<String> Country { get; set; }

        [Input("State province")]
        public InArgument<String> StateProvince { get; set; }

        [Input("City")]
        public InArgument<String> City { get; set; }

        [Input("Postal code")]
        public InArgument<String> PostalCode { get; set; }

        [Input("Address")]
        public InArgument<String> Address { get; set; }

        [Output("Latitude")]
        public OutArgument<String> Latitude { get; set; }

        [Output("Longitude")]
        public OutArgument<String> Longitude { get; set; }

        private string _processName = "GetBingLocation";
        private string _webAddress = "http://dev.virtualearth.net/REST/v1/Locations/";

        /// <summary>
        /// Executes the workflow activity.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        protected override void Execute(CodeActivityContext executionContext)
        {
            string country = Country.Get(executionContext);
            string stateProvince = StateProvince.Get(executionContext);
            string address = Address.Get(executionContext);
            string postalCode = PostalCode.Get(executionContext);
            string city = City.Get(executionContext);
            string bingKey = BingKey.Get(executionContext);

            _webAddress += country + "/" + stateProvince + "/" + postalCode + "/" + city + "/" + address + "?o=xml&key=" + bingKey;

            // Create the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            if (tracingService == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve tracing service.");
            }

            tracingService.Trace("Entered " + _processName + ".Execute(), Activity Instance Id: {0}, Workflow Instance Id: {1}",
                executionContext.ActivityInstanceId,
                executionContext.WorkflowInstanceId);

            // Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();

            if (context == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve workflow context.");
            }

            tracingService.Trace(_processName + ".Execute(), Correlation Id: {0}, Initiating User: {1}",
                context.CorrelationId,
                context.InitiatingUserId);


            XmlDocument bingDoc = CallBing(_webAddress);

            //retrieve lat/long
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(bingDoc.NameTable);
            nsmgr.AddNamespace("rest", "http://schemas.microsoft.com/search/local/ws/rest/v1");

            //set lat/long
            XmlNodeList latElements = bingDoc.SelectNodes("//rest:Latitude", nsmgr);
            XmlNodeList lonElements = bingDoc.SelectNodes("//rest:Longitude", nsmgr);

            Latitude.Set(executionContext, latElements[0].InnerText);
            Longitude.Set(executionContext, lonElements[0].InnerText);

            tracingService.Trace("Exiting " + _processName + ".Execute(), Correlation Id: {0}", context.CorrelationId);
        }

        /// <summary>
        /// method to call a RESTful Bing service using HTTP GET and return the XML response to the calling method
        /// </summary>
        /// <param name="svcUri">full URL</param>
        /// <returns>XML response</returns>
        XmlDocument CallBing(string svcUri)
        {
            try
            {
                WebRequest req = WebRequest.Create(svcUri);
                req.GetResponse().GetResponseStream();

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(req.GetResponse().GetResponseStream());

                
                return xmlDoc;
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
                    throw new InvalidPluginExecutionException("The timeout elapsed while attempting to issue the request.",
                        exception);
                }
                throw new InvalidPluginExecutionException(String.Format(CultureInfo.InvariantCulture,
                    "A Web exception ocurred while attempting to issue the request. {0}: {1}",
                    exception.Message, str), exception);
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(String.Format(CultureInfo.InvariantCulture,
                    "Exception: {0}", e.ToString()));
            }
        }
    }
}
