using System;
using System.Globalization;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Net;
using Microsoft.Xrm.Sdk;
using System.Xml;
using RabbitMQ.Client;

namespace MessageQueuePlugin
{
    public class QueueWriter : IPlugin
    {
        private string _brokerEndpoint;
        private string _brokerUser;
        private string _brokerPassword;
        private string _fetchXml;
        private string _exchange;
        private string _routingKey;

        /// <summary>
        /// The plug-in constructor.
        /// </summary>
        /// <param name="config"></param>
        public QueueWriter(string config)
        {
            if (String.IsNullOrEmpty(config))
            {
                throw new Exception("must supply configuration data");
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(config);

                XmlNodeList routingkeynodes = doc.DocumentElement.SelectNodes("/config/routingkey");
                if (routingkeynodes.Count == 1)
                {
                    _routingKey = routingkeynodes[0].InnerText;
                }
                else
                {
                    throw new Exception("config data must contain exactly one 'routingkey' element");
                }

                XmlNodeList exchangenodes = doc.DocumentElement.SelectNodes("/config/exchange");
                if (exchangenodes.Count == 1)
                {
                    _exchange = exchangenodes[0].InnerText;
                }
                else
                {
                    throw new Exception("config data must contain exactly one 'exchange' element");
                }

                XmlNodeList usernodes = doc.DocumentElement.SelectNodes("/config/user");
                if (usernodes.Count == 1)
                {
                    _brokerUser = usernodes[0].InnerText;
                }
                else
                {
                    throw new Exception("config data must contain exactly one 'user' element");
                }

                XmlNodeList passwordnodes = doc.DocumentElement.SelectNodes("/config/password");
                if (passwordnodes.Count == 1)
                {
                    _brokerPassword = passwordnodes[0].InnerText;
                }
                else
                {
                    throw new Exception("config data must contain exactly one 'password' element");
                }

                XmlNodeList endpointnodes = doc.DocumentElement.SelectNodes("/config/endpoint");
                if (endpointnodes.Count == 1)
                {
                    _brokerEndpoint = endpointnodes[0].InnerText;
                }
                else
                {
                    throw new Exception("config data must contain exactly one 'endpoint' element");
                }

                XmlNodeList querynodes = doc.DocumentElement.SelectNodes("/config/query");
                if (querynodes.Count == 1)
                {
                    _fetchXml = querynodes[0].InnerText;
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
                EntityCollection results = service.RetrieveMultiple(new Microsoft.Xrm.Sdk.Query.FetchExpression(string.Format(_fetchXml, entity.Id.ToString())));

                //we should have one and only one result
                if (results.Entities.Count != 1)
                {
                    throw new Exception("query did not return a single result");
                }

                Entity retrieved = results.Entities[0];

                //prepare the json message that will be sent to rabbitmq
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
                    //connect to rabbitmq
                    var factory = new ConnectionFactory();
                    factory.UserName = _brokerUser;
                    factory.Password = _brokerPassword;
                    factory.VirtualHost = "/";
                    factory.Protocol = Protocols.DefaultProtocol;
                    factory.HostName = _brokerEndpoint;
                    factory.Port = AmqpTcpEndpoint.UseDefaultPort;
                    IConnection conn = factory.CreateConnection();
                    using (var connection = factory.CreateConnection())
                    {
                        using (var channel = connection.CreateModel())
                        {
                            //tell rabbitmq to send confirmation when messages are successfully published
                            channel.ConfirmSelect();
                            channel.WaitForConfirmsOrDie();
                            
                            //prepare message to write to queue
                            var body = Encoding.UTF8.GetBytes(jsonMsg);

                            var properties = channel.CreateBasicProperties();
                            properties.SetPersistent(true);
                            
                            //publish the message to the exchange with the supplied routing key
                            channel.BasicPublish(_exchange, _routingKey, properties, body);
                        }
                    }
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