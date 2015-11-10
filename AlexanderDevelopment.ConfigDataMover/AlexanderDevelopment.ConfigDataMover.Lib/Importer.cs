using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using log4net;
using System.Xml;
using System.IO;
using System.Text;
using System.Runtime.Serialization;

namespace AlexanderDevelopment.ConfigDataMover.Lib
{
    public class Importer
    {
        public string SourceString { get; set; }
        public string TargetString { get; set; }
        public bool MapBaseBu { get; set; }
        public bool MapBaseCurrency { get; set; }
        public List<GuidMapping> GuidMappings { get; set; }
        public List<JobStep> JobSteps { get; set; }

        public int ErrorCount { get { return _errorCount; } }
        private int _errorCount;

        private static CrmConnection _sourceConn;
        private static CrmConnection _targetConn;
        List<GuidMapping> _mappings = new List<GuidMapping>();

        /// <summary>
        /// log4net logger
        /// </summary>
        private ILog logger;

        public delegate void ProgressUpdate(string value);
        public event ProgressUpdate OnProgressUpdate;

        public Importer()
        {
            _mappings = new List<GuidMapping>();
            _errorCount = 0;
            log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        /// used to report progress and log status via a single method
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        private void LogMessage(string level, string message)
        {
            if (OnProgressUpdate != null)
            {
                OnProgressUpdate(message.Trim());
            }
            switch (level.ToUpper())
            {
                case "INFO":
                    logger.Info(message);
                    break;
                case "ERROR":
                    logger.Error(message);
                    break;
                case "WARN":
                    logger.Warn(message);
                    break;
                case "DEBUG":
                    logger.Debug(message);
                    break;
                case "FATAL":
                    logger.Fatal(message);
                    break;
                default:
                    logger.Info(message); //default to info
                    break;
            }
        }

        /// <summary>
        /// parse supplied crm connection strings to get crmconnection objects
        /// </summary>
        private void ParseConnections()
        {
            LogMessage("INFO","parsing source connection");
            _sourceConn = CrmConnection.Parse(SourceString);

            LogMessage("INFO","parsing target connection");
            _targetConn = CrmConnection.Parse(TargetString);

        }

        /// <summary>
        /// handle base business unit, base currency and other GUID mappings
        /// </summary>
        private void SetupGuidMappings()
        {
            LogMessage("INFO","setting up GUID mappings");
            _mappings.Clear();

            Guid sourceBaseBu = Guid.Empty;
            Guid targetBaseBu = Guid.Empty;
            Guid sourceBaseCurrency = Guid.Empty;
            Guid targetBaseCurrency = Guid.Empty;

            using (OrganizationService service = new OrganizationService(_sourceConn))
            {
                if (MapBaseBu)
                {
                    LogMessage("INFO","querying source base business unit");
                    try
                    {
                        string baseBuFetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='businessunit'>
                            <attribute name='name' />
                            <attribute name='businessunitid' />
                            <filter type='and'>
                              <condition attribute='parentbusinessunitid' operator='null' />
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection buEntities = service.RetrieveMultiple(new FetchExpression(baseBuFetchXml));
                        sourceBaseBu = (Guid)(buEntities[0]["businessunitid"]);
                    }
                    catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                    {
                        string errormsg = string.Format(string.Format("could not retrieve source base business unit: {0}", ex.Message));
                        LogMessage("ERROR",errormsg);
                        throw new Exception(errormsg);
                    }
                }

                if (MapBaseCurrency)
                {
                    LogMessage("INFO","querying source base currency");
                    try
                    {
                        string baseCurrencyFetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='organization'>
                            <attribute name='basecurrencyid' />
                          </entity>
                        </fetch>";
                        EntityCollection currencyEntities = service.RetrieveMultiple(new FetchExpression(baseCurrencyFetchXml));
                        sourceBaseCurrency = ((EntityReference)currencyEntities[0]["basecurrencyid"]).Id;
                    }
                    catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                    {
                        string errormsg = string.Format(string.Format("could not source target base currency: {0}", ex.Message));
                        LogMessage("ERROR",errormsg);
                        throw new Exception(errormsg);
                    }
                }
            }

            using (OrganizationService service = new OrganizationService(_targetConn))
            {
                if (MapBaseBu)
                {
                    LogMessage("INFO","querying target base business unit");
                    try
                    {
                        string baseBuFetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='businessunit'>
                            <attribute name='name' />
                            <attribute name='businessunitid' />
                            <filter type='and'>
                              <condition attribute='parentbusinessunitid' operator='null' />
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection buEntities = service.RetrieveMultiple(new FetchExpression(baseBuFetchXml));
                        targetBaseBu = (Guid)(buEntities[0]["businessunitid"]);
                    }
                    catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                    {
                        string errormsg = string.Format("could not retrieve target base business unit: {0}", ex.Message);
                        LogMessage("ERROR",errormsg);
                        throw new Exception(errormsg);
                    }
                }

                if (MapBaseCurrency)
                {
                    LogMessage("INFO","querying target base currency");
                    try
                    {
                        string baseCurrencyFetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='organization'>
                            <attribute name='basecurrencyid' />
                          </entity>
                        </fetch>";
                        EntityCollection currencyEntities = service.RetrieveMultiple(new FetchExpression(baseCurrencyFetchXml));
                        targetBaseCurrency = ((EntityReference)currencyEntities[0]["basecurrencyid"]).Id;
                    }
                    catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                    {
                        string errormsg = string.Format(string.Format("could not retrieve target base currency: {0}", ex.Message));
                        LogMessage("ERROR",errormsg);
                        throw new Exception(errormsg);
                    }
                }
            }

            if (MapBaseBu)
            {
                LogMessage("INFO","setting base business unit GUID mapping");
                if (sourceBaseBu != Guid.Empty && targetBaseBu != Guid.Empty)
                {
                    _mappings.Add(new GuidMapping { sourceId = sourceBaseBu, targetId = targetBaseBu });
                }
            }

            if (MapBaseBu)
            {
                LogMessage("INFO","setting base currency GUID mapping");
                if (sourceBaseCurrency != Guid.Empty && targetBaseCurrency != Guid.Empty)
                {
                    _mappings.Add(new GuidMapping { sourceId = sourceBaseCurrency, targetId = targetBaseCurrency });
                }
            }

            foreach (var item in GuidMappings)
            {
                _mappings.Add(item);
            }
        }

        /// <summary>
        /// used to enable paging in the fetchxml queries - https://msdn.microsoft.com/en-us/library/gg328046.aspx
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="cookie"></param>
        /// <param name="page"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public string CreateXml(string xml, string cookie, int page, int count)
        {
            StringReader stringReader = new StringReader(xml);
            XmlTextReader reader = new XmlTextReader(stringReader);

            // Load document
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            return CreateXml(doc, cookie, page, count);
        }

        /// <summary>
        /// used to enable paging in the fetchxml queries - https://msdn.microsoft.com/en-us/library/gg328046.aspx
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="cookie"></param>
        /// <param name="page"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public string CreateXml(XmlDocument doc, string cookie, int page, int count)
        {
            XmlAttributeCollection attrs = doc.DocumentElement.Attributes;

            if (cookie != null)
            {
                XmlAttribute pagingAttr = doc.CreateAttribute("paging-cookie");
                pagingAttr.Value = cookie;
                attrs.Append(pagingAttr);
            }

            XmlAttribute pageAttr = doc.CreateAttribute("page");
            pageAttr.Value = System.Convert.ToString(page);
            attrs.Append(pageAttr);

            XmlAttribute countAttr = doc.CreateAttribute("count");
            countAttr.Value = System.Convert.ToString(count);
            attrs.Append(countAttr);

            StringBuilder sb = new StringBuilder(1024);
            StringWriter stringWriter = new StringWriter(sb);

            XmlTextWriter writer = new XmlTextWriter(stringWriter);
            doc.WriteTo(writer);
            writer.Close();

            return sb.ToString();
        }

        /// <summary>
        /// runs the import process
        /// </summary>
        public void Process()
        {
            logger = LogManager.GetLogger(typeof(Importer));
            LogMessage("INFO", "starting job");
            ParseConnections();

            SetupGuidMappings();

            LogMessage("INFO","processing records");
            OrganizationService sourceService = new OrganizationService(_sourceConn);
            OrganizationService targetService = new OrganizationService(_targetConn);

            foreach (var item in JobSteps)
            {
                JobStep step = (JobStep)item;
                LogMessage("INFO",string.Format("starting step {0}", step.StepName));
                string fetchQuery = step.StepFetch;

                LogMessage("INFO","  retrieving records");

                // Set the number of records per page to retrieve.
                int fetchCount = 5000;
                
                // Initialize the page number.
                int pageNumber = 1;

                // Specify the current paging cookie. For retrieving the first page, 
                // pagingCookie should be null.
                string pagingCookie = null;

                //create a list of entities to hold retrieved entities so we can page through results
                List<Entity> ec = new List<Entity>();

                while (true)
                {
                    // Build fetchXml string with the placeholders.
                    string fetchXml = CreateXml(fetchQuery, pagingCookie, pageNumber, fetchCount);

                    EntityCollection retrieved = sourceService.RetrieveMultiple(new FetchExpression(fetchXml));
                    ec.AddRange(retrieved.Entities);

                    if (retrieved.MoreRecords)
                    {
                        // Increment the page number to retrieve the next page.
                        pageNumber++;

                        // Set the paging cookie to the paging cookie returned from current results.                            
                        pagingCookie = retrieved.PagingCookie;
                    }
                    else
                    {
                        // If no more records in the result nodes, exit the loop.
                        break;
                    }
                }

                LogMessage("INFO",string.Format("  {0} records retrieved", ec.Count));
                if (ec.Count > 0)
                {
                    foreach (Entity entity in ec)
                    {
                        //create a list to hold the replacement guids. a second pass is required because c# disallows modifying a collection while enumerating
                        List<KeyValuePair<string, object>> guidsToUpdate = new List<KeyValuePair<string, object>>();
                        LogMessage("INFO",string.Format("  processing record {0}, {1}", entity.Id, entity.LogicalName));
                        try
                        {
                            LogMessage("INFO","    processing GUID replacements");
                            foreach (KeyValuePair<string, object> attribute in entity.Attributes)
                            {
                                //LogMessage("INFO",string.Format("Attribute - {0} {1}", attribute.Key, attribute.Value.GetType().ToString()));
                                if (attribute.Value is Microsoft.Xrm.Sdk.EntityReference)
                                {
                                    //LogMessage("INFO","getting source");

                                    EntityReference source = ((EntityReference)attribute.Value);
                                    try
                                    {
                                        //LogMessage("INFO","looking for GUID replacement");
                                        Guid sourceId = source.Id;
                                        Guid targetId = _mappings.Find(t => t.sourceId == source.Id).targetId;
                                        source.Id = targetId;
                                        guidsToUpdate.Add(new KeyValuePair<string, object>(attribute.Key, source));
                                        //LogMessage("INFO",string.Format("replacement found - {0} -> {1}", sourceId, targetId));
                                    }
                                    catch (System.NullReferenceException ex)
                                    {
                                        //LogMessage("INFO", "NullReferenceException happened");
                                        //do nothing because nullreferenceexception means there's no guid mapping to use
                                    }
                                }
                            }

                            //now actually update the GUIDs with the mapped values
                            foreach (KeyValuePair<string, object> attribute in guidsToUpdate)
                            {
                                //LogMessage("INFO",string.Format("    replacing attribute GUID {0} {1}", attribute.Key, attribute.Value));
                                entity[attribute.Key] = attribute.Value;
                            }

                            //try to update first
                            try
                            {
                                LogMessage("INFO","    trying target update");
                                targetService.Update(entity);
                                LogMessage("INFO","    update ok");
                            }
                            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                            {
                                if (!step.UpdateOnly)
                                {
                                    LogMessage("INFO","    trying target create");
                                    //if update fails and step is not update-only then try to create
                                    targetService.Create(entity);
                                    LogMessage("INFO","    create ok");
                                }
                                else
                                {
                                    throw new FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>(ex.Detail);
                                }
                            }
                        }
                        catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                        {
                            //if everything fails, log error
                            //to main log
                            LogMessage("ERROR", string.Format("    record transfer failed"));

                            //to record error log
                            LogMessage("ERROR", string.Format("RECORD ERROR: {0}, {1}", entity.Id, entity.LogicalName));

                            //increment the error count
                            _errorCount++;
                        }
                    }
                }
            }
            LogMessage("INFO","job complete");
            logger.Logger.Repository.Shutdown();
        }
    }
}