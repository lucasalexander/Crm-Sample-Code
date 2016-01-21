// --------------------------------------------------------------------------------------------------------------------
// Importer.cs
//
// Copyright 2015 Lucas Alexander
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
using Newtonsoft.Json;

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
        private static string _sourceFile;
        private static string _targetFile;
        private static bool _isFileSource;
        private static bool _isFileTarget;

        private static ExportedData _savedSourceData;

        List<GuidMapping> _mappings = new List<GuidMapping>();

        /// <summary>
        /// log4net logger
        /// </summary>
        private ILog logger;

        public delegate void ProgressUpdate(string value);
        public event ProgressUpdate OnProgressUpdate;

        public Importer()
        {
            _sourceFile = string.Empty;
            _targetFile = string.Empty;
            _isFileSource = false;
            _isFileTarget = false;
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
            LogMessage("INFO", "parsing source connection");
            if (SourceString.ToUpper().StartsWith("FILE="))
            {
                _sourceFile = Regex.Replace(SourceString, "FILE=", "", RegexOptions.IgnoreCase);
                _isFileSource = true;
                LogMessage("INFO", "source is file - " + _sourceFile);

                //deserialze source data
                using (StreamReader sr = new StreamReader(_sourceFile))
                {
                    LogMessage("INFO", "  deserializing source data from file");
                    // Read the stream to a string, and write the string to the console.
                    String lines = sr.ReadToEnd();
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.TypeNameHandling = TypeNameHandling.None;
                    _savedSourceData = (ExportedData)JsonConvert.DeserializeObject<ExportedData>(lines,settings);
                    LogMessage("INFO", "  source data deserialization complete");

                }
            }
            else
            {
                _sourceConn = CrmConnection.Parse(SourceString);
                _isFileSource = false;
            }

            LogMessage("INFO", "parsing target connection");
            if (TargetString.ToUpper().StartsWith("FILE="))
            {
                _targetFile = Regex.Replace(TargetString, "FILE=", "", RegexOptions.IgnoreCase);
                _savedSourceData = new ExportedData();
                _isFileTarget = true;
                LogMessage("INFO", "target is file - " + _targetFile);
            }
            else
            {
                _targetConn = CrmConnection.Parse(TargetString);
                _isFileTarget = false;
            }
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

            if (_isFileSource)
            {
                if (MapBaseBu)
                {
                    sourceBaseBu = _savedSourceData.BaseBu;
                }
                if (MapBaseCurrency)
                {
                    sourceBaseCurrency = _savedSourceData.BaseCurrency;
                }
            }
            else
            {
                using (OrganizationService service = new OrganizationService(_sourceConn))
                {
                    if (MapBaseBu)
                    {
                        LogMessage("INFO", "querying source base business unit");
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
                            LogMessage("ERROR", errormsg);
                            throw new Exception(errormsg);
                        }
                    }

                    if (MapBaseCurrency)
                    {
                        LogMessage("INFO", "querying source base currency");
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
                            LogMessage("ERROR", errormsg);
                            throw new Exception(errormsg);
                        }
                    }
                }
            }

            if (!_isFileTarget)
            {
                using (OrganizationService service = new OrganizationService(_targetConn))
                {
                    if (MapBaseBu)
                    {
                        LogMessage("INFO", "querying target base business unit");
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
                            LogMessage("ERROR", errormsg);
                            throw new Exception(errormsg);
                        }
                    }

                    if (MapBaseCurrency)
                    {
                        LogMessage("INFO", "querying target base currency");
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
                            LogMessage("ERROR", errormsg);
                            throw new Exception(errormsg);
                        }
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

                if (_isFileTarget)
                {
                    _savedSourceData.BaseBu = sourceBaseBu;
                }
            }

            if (MapBaseCurrency)
            {
                LogMessage("INFO","setting base currency GUID mapping");
                if (sourceBaseCurrency != Guid.Empty && targetBaseCurrency != Guid.Empty)
                {
                    _mappings.Add(new GuidMapping { sourceId = sourceBaseCurrency, targetId = targetBaseCurrency });
                }

                if (_isFileTarget)
                {
                    _savedSourceData.BaseCurrency = sourceBaseCurrency;
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

            LogMessage("INFO", "processing records");

            for (int i = 0; i < JobSteps.Count; i++)
            {
                var item = JobSteps[i];
                JobStep step = (JobStep)item;
                LogMessage("INFO", string.Format("starting step {0}", step.StepName));

                //create a list of entities to hold retrieved entities so we can page through results
                List<Entity> ec = new List<Entity>();

                if (_isFileSource)
                {
                    LogMessage("INFO", "  preparing data from source file for update/import");
                    foreach (var e in _savedSourceData.RecordSets[i])
                    {
                        Entity entity = new Entity(e.LogicalName);
                        entity.Id = e.Id;
                        entity.LogicalName = e.LogicalName;
                        foreach (ExportAttribute exportAttribute in e.Attributes)
                        {
                            Newtonsoft.Json.Linq.JObject jObject;
                            object attributeValue = null;
                            string attributeName = exportAttribute.AttributeName;
                            switch (exportAttribute.AttributeType)
                            {
                                case "Microsoft.Xrm.Sdk.EntityReference":
                                    jObject = (Newtonsoft.Json.Linq.JObject)exportAttribute.AttributeValue;
                                    EntityReference lookup = new EntityReference((string)jObject["LogicalName"], (Guid)jObject["Id"]);
                                    attributeValue = lookup;
                                    break;
                                case "Microsoft.Xrm.Sdk.OptionSetValue":
                                    jObject = (Newtonsoft.Json.Linq.JObject)exportAttribute.AttributeValue;
                                    attributeValue = new OptionSetValue { Value = (int)jObject["Value"] };
                                    break;
                                case "Microsoft.Xrm.Sdk.Money":
                                    jObject = (Newtonsoft.Json.Linq.JObject)exportAttribute.AttributeValue;
                                    attributeValue = new Microsoft.Xrm.Sdk.Money { Value = (decimal)jObject["Value"] };
                                    break;
                                default:
                                    attributeValue = exportAttribute.AttributeValue;
                                    break;
                            }
                            entity.Attributes.Add(attributeName, attributeValue);
                        }
                        ec.Add(entity);
                    }
                }
                else
                {
                    OrganizationService sourceService = new OrganizationService(_sourceConn);

                    string fetchQuery = step.StepFetch;

                    LogMessage("INFO", "  retrieving records");

                    // Set the number of records per page to retrieve.
                    int fetchCount = 5000;

                    // Initialize the page number.
                    int pageNumber = 1;

                    // Specify the current paging cookie. For retrieving the first page, 
                    // pagingCookie should be null.
                    string pagingCookie = null;

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
                    LogMessage("INFO", string.Format("  {0} records retrieved", ec.Count));
                }

                if (ec.Count > 0)
                {
                    //_savedSourceData.RecordSets = new List<EntityCollection>();
                    if (!_isFileTarget)
                    {
                        foreach (Entity entity in ec)
                        {

                            OrganizationService targetService = new OrganizationService(_targetConn);

                            //create a list to hold the replacement guids. a second pass is required because c# disallows modifying a collection while enumerating
                            List<KeyValuePair<string, object>> guidsToUpdate = new List<KeyValuePair<string, object>>();
                            LogMessage("INFO", string.Format("  processing record {0}, {1}", entity.Id, entity.LogicalName));
                            try
                            {
                                LogMessage("INFO", "    processing GUID replacements");
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
                                    LogMessage("INFO", "    trying target update");
                                    targetService.Update(entity);
                                    LogMessage("INFO", "    update ok");
                                }
                                catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                                {
                                    if (!step.UpdateOnly)
                                    {
                                        LogMessage("INFO", "    trying target create");
                                        //if update fails and step is not update-only then try to create
                                        targetService.Create(entity);
                                        LogMessage("INFO", "    create ok");
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
                if (_isFileTarget)
                {
                    LogMessage("INFO", "  preparing records for serialization");
                    List<ExportEntity> entitiesToExport = new List<ExportEntity>();
                    foreach(Entity e in ec)
                    {
                        ExportEntity exportEntity = new ExportEntity();
                        exportEntity.Id = e.Id;
                        exportEntity.LogicalName = e.LogicalName;
                        foreach(var attribute in e.Attributes)
                        {
                            if ((attribute.Key.ToUpper() != e.LogicalName.ToUpper() + "ID") 
                                && (attribute.Key.ToUpper() != "LOGICALNAME"))
                            {
                                ExportAttribute exportAttribute = new ExportAttribute();
                                exportAttribute.AttributeName = attribute.Key;
                                exportAttribute.AttributeValue = attribute.Value;
                                exportAttribute.AttributeType = attribute.Value.GetType().ToString();
                                exportEntity.Attributes.Add(exportAttribute);
                            }
                        }
                        entitiesToExport.Add(exportEntity);
                    }
                    _savedSourceData.RecordSets.Add(entitiesToExport);
               }
            }
            if (_isFileTarget)
            {
                LogMessage("INFO", "  serializing data to target file");
                JsonSerializer serializer = new JsonSerializer();
                using (StreamWriter sw = new StreamWriter(_targetFile))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.NullValueHandling = NullValueHandling.Ignore;
                        serializer.TypeNameHandling = TypeNameHandling.None;
                        serializer.Formatting = Newtonsoft.Json.Formatting.None;
                        serializer.Serialize(writer, _savedSourceData);
                    }
                }
                LogMessage("INFO", "data serialization complete");
            }
            LogMessage("INFO", "job complete");
            logger.Logger.Repository.Shutdown();
        }
    }

    [Serializable()]
    class ExportedData
    {
        public Guid BaseBu { get; set; }
        public Guid BaseCurrency { get; set; }
        public List<List<ExportEntity>> RecordSets { get; set; }

        public ExportedData()
        {
            RecordSets = new List<List<ExportEntity>>();
            BaseBu = Guid.Empty;
            BaseCurrency = Guid.Empty;
            Entity e = new Entity();
        }
    }

    [Serializable()]
    class ExportEntity
    {
        public string LogicalName { get; set; }
        public Guid Id { get; set; }
        public List<ExportAttribute> Attributes { get; set; }

        public ExportEntity()
        {
            Attributes = new List<ExportAttribute>(); ;
        }
    }

    [Serializable()]
    class ExportAttribute
    {
        public string AttributeName { get; set; }
        public object AttributeValue { get; set; }
        public string AttributeType { get; set; }
    }
}