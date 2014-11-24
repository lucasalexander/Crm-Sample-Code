using System;
using System.Globalization;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Net;
using Microsoft.Xrm.Sdk;
using System.Xml;

namespace Node_Notifier
{
    public class Notifier : IPlugin
    {
        private string webAddress;
        private string fetchXml;

        /// <summary>
        /// The plug-in constructor.
        /// </summary>
        /// <param name="config"></param>
        public Notifier(string config)
        {
            if (String.IsNullOrEmpty(config))
            {
                throw new Exception("must supply configuration data");
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(config);

                XmlNodeList endpointnodes = doc.DocumentElement.SelectNodes("/config/endpoint");
                if (endpointnodes.Count == 1)
                {
                    webAddress = endpointnodes[0].InnerText;
                }
                else
                {
                    throw new Exception("config data must contain exactly one 'endpoint' element");
                }

                XmlNodeList querynodes = doc.DocumentElement.SelectNodes("/config/query");
                if (querynodes.Count == 1)
                {
                    fetchXml = querynodes[0].InnerText;
                }
                else
                {
                    throw new Exception("config data must contain exactly one 'query' element");
                }

            }
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in plug-in debugging.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            Microsoft.Xrm.Sdk.IPluginExecutionContext context = (Microsoft.Xrm.Sdk.IPluginExecutionContext)
                serviceProvider.GetService(typeof(Microsoft.Xrm.Sdk.IPluginExecutionContext));

            //make sure we have a target that is an entity
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                //set up the org service reference for our retrieve
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                //retrieve some results using the fetchxml supplied in the configuration
                EntityCollection results = service.RetrieveMultiple(new Microsoft.Xrm.Sdk.Query.FetchExpression(string.Format(fetchXml, entity.Id.ToString())));

                //we should have one and only one result
                if(results.Entities.Count!=1)
                {
                    throw new Exception("query did not return a single result");
                }

                Entity retrieved = results.Entities[0];

                //set up our json writer
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                JsonWriter jsonWriter = new JsonTextWriter(sw);

                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;

                jsonWriter.WriteStartObject();

                //loop through the retrieved attributes
                foreach (string attribute in retrieved.Attributes.Keys)
                {
                    //generate different output for different attribute types
                    switch (retrieved[attribute].GetType().ToString())
                    {
                        //if we have a lookup, return the id and the name
                        case "Microsoft.Xrm.Sdk.EntityReference":
                            jsonWriter.WritePropertyName(attribute);
                            jsonWriter.WriteValue(((EntityReference)retrieved[attribute]).Id);
                            jsonWriter.WritePropertyName(attribute + "_name");
                            jsonWriter.WriteValue(((EntityReference)retrieved[attribute]).Name);
                            break;
                        //if we have an optionset value, return the value and the formatted value
                        case "Microsoft.Xrm.Sdk.OptionSetValue":
                            jsonWriter.WritePropertyName(attribute);
                            jsonWriter.WriteValue(((OptionSetValue)retrieved[attribute]).Value);
                            if (retrieved.FormattedValues.Contains(attribute))
                            {
                                jsonWriter.WritePropertyName(attribute + "_formatted");
                                jsonWriter.WriteValue(retrieved.FormattedValues[attribute]);
                            }
                            break;
                        //if we have money, return the value
                        case "Microsoft.Xrm.Sdk.Money":
                            jsonWriter.WritePropertyName(attribute);
                            jsonWriter.WriteValue(((Money)retrieved[attribute]).Value);
                            break;
                        //if we have a datetime, return the value
                        case "System.DateTime":
                            jsonWriter.WritePropertyName(attribute);
                            jsonWriter.WriteValue(retrieved[attribute]);
                            break;
                        //for everything else, return the value and a formatted value if it exists
                        default:
                            jsonWriter.WritePropertyName(attribute);
                            jsonWriter.WriteValue(retrieved[attribute]);
                            if (retrieved.FormattedValues.Contains(attribute))
                            {
                                jsonWriter.WritePropertyName(attribute + "_formatted");
                                jsonWriter.WriteValue(retrieved.FormattedValues[attribute]);
                            }
                            break;
                    }

                }
                //always write out the message name (update, create, etc.), entity name and record id
                jsonWriter.WritePropertyName("operation");
                jsonWriter.WriteValue(context.MessageName);
                jsonWriter.WritePropertyName("entity");
                jsonWriter.WriteValue(retrieved.LogicalName);
                jsonWriter.WritePropertyName("id");
                jsonWriter.WriteValue(retrieved.Id);
                jsonWriter.WriteEndObject();

                //generate the json string
                string jsonMsg = sw.ToString();

                jsonWriter.Close();
                sw.Close();

                try
                {
                    //create the webrequest object and execute it (and post jsonmsg to it)
                    //see http://www.alexanderdevelopment.net/post/2013/04/22/Postingprocessing-JSON-in-a-CRM-2011-custom-workflow-activity 
                    //for additional information on working with json from dynamics CRM
                    System.Net.WebRequest req = System.Net.WebRequest.Create(webAddress);

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
                catch (Exception e)
                {
                    tracingService.Trace("Exception: {0}", e.ToString());
                    throw;
                }
            }
        }
    }
}