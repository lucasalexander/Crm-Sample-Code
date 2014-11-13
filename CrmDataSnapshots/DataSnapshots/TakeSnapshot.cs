using System;
using System.Activities;
using System.ServiceModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using System.Data;

namespace DataSnapshots
{
    public sealed class TakeSnapshot : CodeActivity
    {
        [Input("FetchXML query")]
        public InArgument<String> FetchXMLQuery { get; set; }

        [Input("Snapshot mappings")]
        public InArgument<String> SnapshotMappings { get; set; }
        
        [Input("Target entity")]
        public InArgument<String> TargetEntity { get; set; }
        
        //name of your custom workflow activity for tracing/error logging
        private string _activityName = "TakeSnapshot";

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
                //get the source->target field mappings from the input parameters
                string mappingsText = SnapshotMappings.Get(executionContext);

                //split up the lines into an array
                string[] mappingLines  = mappingsText.Split(Environment.NewLine.ToCharArray());

                //create a dictionary object to hold the mappings
                Dictionary<string, string> mappingDictionary = new Dictionary<string,string>();

                //for each line in the array, split it on the ">" character and store the source->target as a key-value pair
                foreach(string s in mappingLines)
                {
                    try 
                    {
                        string targetfield = s.Split(">".ToCharArray())[1];
                        string sourcefield = s.Split(">".ToCharArray())[0];
                        tracingService.Trace("line: " + s + " source: " + sourcefield + " target: " + targetfield);
                        mappingDictionary.Add(sourcefield, targetfield);
                    }
                    catch(Exception ex)
                    {
                        throw new InvalidPluginExecutionException("Error parsing source->target mappings.");
                    }
                }

                //get the target entity name from the input parameters
                string targetEntityType = TargetEntity.Get(executionContext);

                //execute the fetchxml query and loop through the results
                EntityCollection recordsToProcess = service.RetrieveMultiple(new FetchExpression(FetchXMLQuery.Get(executionContext)));
                recordsToProcess.Entities.ToList().ForEach(a =>
                {
                    tracingService.Trace("instantiating new entity of type: " + targetEntityType);
                    Entity targetRecord = new Entity(targetEntityType);
                    
                    //loop through the mapping key-value pairs
                    foreach (string key in mappingDictionary.Keys)
                    {
                        //check to see if attribute exists in this particular result
                        //fetchxml results don't return fields with null values
                        if (a.Contains(key))
                        {
                            //if the result field is an aliasedvalue, we need to handle it differently than non-aliasedvalue types
                            if (a[key].GetType().FullName.ToUpperInvariant().EndsWith("ALIASEDVALUE"))
                            {
                                //cast it to aliasedvalue
                                var resultfield = (AliasedValue)a[key];

                                //set the target field to the casted resultfield value
                                targetRecord[mappingDictionary[key]] = resultfield.Value;

                                //trace just in case
                                tracingService.Trace("new source column [" + key + "]: destination [" + mappingDictionary[key] + "]: " + (resultfield.Value).ToString());
                            }
                            else
                            {
                                //set the target field to the resultfield
                                //target field needs to be of EXACT same type as result field
                                //for example, this code doesn't work if you try to store an integer in a decimal field
                                targetRecord[mappingDictionary[key]] = a[key];

                                //trace just in case
                                tracingService.Trace("new source column [" + key + "]: destination [" + mappingDictionary[key] + "]: " + (a[key]).ToString());
                            }
                        }
                    }
                    try
                    {
                        //create the record
                        service.Create(targetRecord);
                    }
                    catch (FaultException<OrganizationServiceFault> e)
                    {
                        //catch orgservice faults
                        tracingService.Trace("Exception: {0}", e.ToString());

                        // Handle the exception.
                        throw new InvalidPluginExecutionException("Could not create snapshot record.");
                    }
                    catch (Exception ex)
                    {
                        //catch anything else
                        throw new InvalidPluginExecutionException("Could not create snapshot record.");
                    }

                });
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                tracingService.Trace("Exception: {0}", e.ToString());
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