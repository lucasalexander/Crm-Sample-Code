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
using System.Text.RegularExpressions;

namespace CrmQueueGetNext
{
    /// <summary>
    /// This custom workflow activity is used to retrieve the next case in a queue and assign it to a user
    /// </summary>
    public sealed class GetNextItem : CodeActivity
    {
        /// <summary>
        /// Queue to be searched
        /// </summary>
        [Input("Queue")]
        [ReferenceTarget("queue")]
        public InArgument<EntityReference> Queue { get; set; }

        /// <summary>
        /// System user
        /// </summary>
        [Input("User")]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> User { get; set; }

        /// <summary>
        /// True if case should be assigned to user or false to just return the details
        /// </summary>
        [Input("Assign")]
        public InArgument<Boolean> Assign { get; set; }

        /// <summary>
        /// True if case should be removed from queue or false to leave existing queue item
        /// </summary>
        [Input("Remove from queue")]
        public InArgument<Boolean> RemoveFromQueue { get; set; }

        /// <summary>
        /// This returns the case id as a string
        /// </summary>
        [Output("Case id")]
        public OutArgument<String> CaseId { get; set; }

        /// <summary>
        /// This returns the case entityreference
        /// </summary>
        [Output("Case number")]
        public OutArgument<String> CaseNumber { get; set; }

        /// <summary>
        /// Flag if case is found
        /// </summary>
        [Output("Case found")]
        public OutArgument<Boolean> CaseFound { get; set; }

        /// <summary>
        /// This returns the case entityreference
        /// </summary>
        [Output("Case title")]
        public OutArgument<String> CaseTitle { get; set; }

        private string _processName = "GetNextItem";

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

            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            tracingService.Trace("entering 'try'", "");
            try
            {
                tracingService.Trace("building fetch", "");
                string fetchXml = @"<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>
                        <entity name='queueitem'>
                            <attribute name='title'/>
                            <attribute name='enteredon'/>
                            <attribute name='objecttypecode'/>
                            <attribute name='objectid'/>
                            <attribute name='queueid'/>
                            <attribute name='workerid'/>
                            <order descending='false' attribute='enteredon'/>
                            <filter type='and'>
                                <condition attribute='statecode' value='0' operator='eq'/>
                                <condition attribute='objecttypecode' value='112' operator='eq'/>
                                <condition attribute='queueid' value='{0}' operator='eq'/>
                                <condition attribute='workerid' operator='null'/>
                            </filter>
                            <link-entity name='incident' alias='casealias' link-type='inner' to='objectid' from='incidentid'>
                                <attribute name='prioritycode'/>
                                <attribute name='ticketnumber'/>
                            </link-entity>
                        </entity>
                    </fetch> ";

                Guid queueId = Queue.Get(executionContext).Id;

                fetchXml = string.Format(fetchXml, queueId);

                tracingService.Trace("prepared fetchxml: {0}", fetchXml);

                tracingService.Trace("retrieving queue items", "");
                EntityCollection queueItems = service.RetrieveMultiple(new FetchExpression(fetchXml));

                tracingService.Trace("instantiating empty queueitem", "");
                Entity item = new Entity("queueitem");
                string caseNumber = "";
                string caseTitle = "";
                string caseId = Guid.Empty.ToString();
                bool caseFound = false;

                if (queueItems.Entities.Count > 0)
                {
                    caseFound = true;
                    tracingService.Trace("processing queueitems", "");
                    item = queueItems.Entities[0];
                    tracingService.Trace("objectid: {0}", ((EntityReference)item["objectid"]).Id);
                    caseId = ((EntityReference)item["objectid"]).Id.ToString();
                    caseTitle = item["title"].ToString();
                    caseNumber = ((AliasedValue)item["casealias.ticketnumber"]).Value.ToString();

                    if (Assign.Get(executionContext))
                    {
                        tracingService.Trace("processing assignment", "");
                        PickFromQueueRequest request = new PickFromQueueRequest();
                        request.RemoveQueueItem = RemoveFromQueue.Get(executionContext);
                        request.QueueItemId = item.Id;
                        request.WorkerId = User.Get(executionContext).Id;
                        service.Execute(request);
                    }
                }

                tracingService.Trace("setting outputs", "");
                CaseId.Set(executionContext, caseId);
                CaseNumber.Set(executionContext, caseNumber);
                CaseTitle.Set(executionContext, caseTitle);
                CaseFound.Set(executionContext, caseFound);
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

            tracingService.Trace("Exiting " + _processName + ".Execute(), Correlation Id: {0}", context.CorrelationId);
        }
    }
}