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
using System.Text.RegularExpressions;

namespace RegexTools
{
    public sealed class ValidateRegex : CodeActivity
    {
        [Input("String to validate")]
        public InArgument<String> StringToValidate { get; set; }

        [Input("Match pattern")]
        public InArgument<String> MatchPattern { get; set; }

        [Output("Valid")]
        public OutArgument<int> Valid { get; set; }

        private string _processName = "ValidateRegex";

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
                //do the regex match
                Match match = Regex.Match(StringToValidate.Get(executionContext), MatchPattern.Get(executionContext),
                    RegexOptions.IgnoreCase);

                //did we match anything?
                if (match.Success)
                {
                    Valid.Set(executionContext, 1);
                }
                else
                {
                    Valid.Set(executionContext, 0);
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