using System;
using System.Activities;
using System.ServiceModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using System.Net;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using System.Text.RegularExpressions;
using System.Linq;

namespace CrmTeamConnection
{
    public class AssignUserToAccessTeamByName : CodeActivity
    {
        [Input("Connection")]
        [ReferenceTarget("connection")]
        public InArgument<EntityReference> Connection { get; set; }

        [Input("Team Template name")]
        public InArgument<string> TeamTemplateName { get; set; }

        private string _processName = "AssignUserToAccessTeamByName";
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

            try
            {
                Guid connectionId = Connection.Get(executionContext).Id;
                Entity connection = service.Retrieve("connection", connectionId, new ColumnSet("record1id", "record2id"));

                EntityReference connectedFromId = (EntityReference)connection["record1id"];
                EntityReference connectedToId = (EntityReference)connection["record2id"];

                //only run this if the connection is to a user record
                if (connectedToId.LogicalName.ToUpper() == "SYSTEMUSER")
                {
                    Guid teamTemplateId;
                    string teamTemplateName = TeamTemplateName.Get(executionContext);

                    //look up team template by name
                    QueryByAttribute querybyexpression = new QueryByAttribute("teamtemplate");
                    querybyexpression.ColumnSet = new ColumnSet("teamtemplatename", "teamtemplateid");
                    querybyexpression.Attributes.AddRange("teamtemplatename");
                    querybyexpression.Values.AddRange(teamTemplateName);
                    EntityCollection retrieved = service.RetrieveMultiple(querybyexpression);

                    //if we find something, we're set
                    if (retrieved.Entities.Count > 0)
                    {
                        teamTemplateId = retrieved.Entities[0].Id;
                    }
                    else
                    {
                        //throw exception if unable to find a matching template
                        throw new Exception("could not find team template named: " + teamTemplateName);
                    }

                    AddUserToRecordTeamRequest teamAddRequest = new AddUserToRecordTeamRequest();
                    teamAddRequest.Record = connectedFromId;
                    teamAddRequest.SystemUserId = connectedToId.Id;
                    teamAddRequest.TeamTemplateId = teamTemplateId;

                    AddUserToRecordTeamResponse response = (AddUserToRecordTeamResponse)service.Execute(teamAddRequest);
                }
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