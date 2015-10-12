using System;
using System.Activities;
using System.ServiceModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Runtime.Serialization;

namespace CrmAzureTextAnalysis
{
    /// <summary>
    /// This is a custom workflow activity for performing sentiment analysis using Azure text analytics API:
    ///  - https://datamarket.azure.com/dataset/amla/text-analytics
    ///  - http://blogs.technet.com/b/machinelearning/archive/2015/04/08/introducing-text-analytics-in-the-azure-ml-marketplace.aspx
    ///  - https://azure.microsoft.com/en-us/documentation/articles/machine-learning-apps-text-analytics/
    /// 
    /// This is based on code I originally wrote for performing sentiment analysis using HP IDOL OnDemand (now called HP Haven OnDemand)
    ///  - https://github.com/lucasalexander/CRM-IdolOnDemand-Tools
    ///  - http://h30507.www3.hp.com/t5/Applications-Services-Blog/Using-IDOL-OnDemand-for-text-analysis-in-Dynamics-CRM-Part-1/ba-p/171242
    ///  - http://h30507.www3.hp.com/t5/Applications-Services-Blog/Using-IDOL-OnDemand-for-text-analysis-in-Dynamics-CRM-Part-2/ba-p/171424
    /// </summary>
    public sealed class AnalyzeSentiment : CodeActivity
    {
        /// <summary>
        /// text to analyze
        /// </summary>
        [RequiredArgument]
        [Input("Text Input")]
        public InArgument<String> TextInput { get; set; }

        /// <summary>
        /// azure text analytics access key 
        /// </summary>
        [RequiredArgument]
        [Input("API Key")]
        public InArgument<String> ApiKey { get; set; }

        /// <summary>
        /// sentiment score - numbers closer to 1 are more positive, numbers closer to 0 are more negative
        /// </summary>
        [Output("Score")]
        public OutArgument<Decimal> Score { get; set; }

        //web service address
        private string _webAddress = "https://api.datamarket.azure.com/data.ashx/amla/text-analytics/v1/GetSentimentBatch";

        //name of your custom workflow activity for tracing/error logging
        private string _activityName = "AnalyzeSentiment";

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
                string inputText = TextInput.Get(executionContext);
                if (inputText != string.Empty)
                {
                    //strip any html characters from the text to analyze
                    inputText = HtmlTools.StripHTML(inputText);
                    tracingService.Trace(string.Format("stripped text: {0}", inputText));

                    //escape any double quote characters (") so they don't cause problems when posting the json later
                    string escapedText = inputText.Replace("\"", "\\\"");
                    tracingService.Trace(string.Format("escaped text: {0}", escapedText));

                    //create the webrequest object
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(_webAddress);

                    //set request content type so it is treated as a regular form post
                    req.ContentType = "application/x-www-form-urlencoded";

                    //set method to post
                    req.Method = "POST";

                    //buld the request
                    StringBuilder postData = new StringBuilder();

                    //note the Id value set to 1 - we can hardcode this because we're only sending a batch of one
                    postData.Append("{\"Inputs\":[{\"Id\":\"1\",\"Text\":\""+escapedText+"\"}]}");

                    //set the authentication using the azure service access key
                    string authInfo = "AccountKey:" + ApiKey.Get(executionContext);
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    req.Headers["Authorization"] = "Basic " + authInfo;

                    //create a stream
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(postData.ToString());
                    req.ContentLength = bytes.Length;
                    System.IO.Stream os = req.GetRequestStream();
                    os.Write(bytes, 0, bytes.Length);
                    os.Close();

                    //post the request and get the response
                    System.Net.WebResponse resp = req.GetResponse();

                    //deserialize the response to a SentimentBatchResult object
                    SentimentBatchResult sentimentResult = new SentimentBatchResult();
                    System.Runtime.Serialization.Json.DataContractJsonSerializer deserializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(sentimentResult.GetType());
                    sentimentResult = deserializer.ReadObject(resp.GetResponseStream()) as SentimentBatchResult;

                    //if no errors, return the sentiment
                    if (sentimentResult.Errors.Count == 0)
                    {
                        //set output values from the fields of the deserialzed myjsonresponse object
                        Score.Set(executionContext, sentimentResult.SentimentBatch[0].Score);
                    }
                    else
                    {
                        //otherwise raise an exception with the returned error message
                        throw new InvalidPluginExecutionException(String.Format("Sentiment analyis error: {0)", sentimentResult.Errors[0].Message));
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

    }

    /// <summary>
    /// class to represent invidual sentiment result item
    /// </summary>
    [DataContract]
    public class SentimentBatchResultItem
    {
        [DataMember(Name = "Score")]
        public Decimal Score { get; set; }

        [DataMember(Name = "Id")]
        public String Id { get; set; }
    }

    /// <summary>
    /// class to represent invidual error item
    /// </summary>
    [DataContract]
    public class ErrorRecord
    {
        [DataMember(Name = "Id")]
        public String Id { get; set; }

        [DataMember(Name = "Message")]
        public String Message { get; set; }
    }

    /// <summary>
    /// class to represent sentiment result batch response
    /// </summary>
    [DataContract]
    public class SentimentBatchResult
    {
        [DataMember(Name = "SentimentBatch")]
        public List<SentimentBatchResultItem> SentimentBatch { get; set; }

        [DataMember(Name = "Errors")]
        public List<ErrorRecord> Errors { get; set; }
    }
}
