using System;
using System.Activities;
using System.ServiceModel;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Text.RegularExpressions;

namespace DemoCrmWorkflowActivities
{
    /// <summary>
    /// This custom workflow activity is used to validate a string against a regular expression
    /// </summary>
    public sealed class ValidateRegex : CodeActivity
    {
        /// <summary>
        /// This is the string that will be validated
        /// </summary>
        [Input("String to validate")]
        public InArgument<String> StringToValidate { get; set; }

        /// <summary>
        /// This is the regular expression used for validation - see C# regex quick reference here - http://msdn.microsoft.com/en-us/library/az24scfc.aspx
        /// </summary>
        [Input("Match pattern")]
        public InArgument<String> MatchPattern { get; set; }

        /// <summary>
        /// This returns a 1 if a match is found or 0 if a match is not found
        /// </summary>
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

            string valString = StringToValidate.Get(executionContext);
            string matchPattern = MatchPattern.Get(executionContext);

            try
            {
                //did we match anything?
                if (ValidateString(valString, matchPattern))
                {
                    Valid.Set(executionContext, 1);
                }
                else
                {
                    Valid.Set(executionContext, 0);
                }

            }
            catch (Exception e)
            {
                tracingService.Trace("Exception: {0}", e.ToString());
                throw;
            }

            tracingService.Trace("Exiting " + _processName + ".Execute(), Correlation Id: {0}", context.CorrelationId);
        }

        /// <summary>
        /// method to do the regex match
        /// </summary>
        /// <param name="valString">string to validate</param>
        /// <param name="matchPattern">regex pattern to use for validation</param>
        /// <returns>true if a match is found, else false</returns>
        private bool ValidateString(string valString, string matchPattern)
        {
            //do the regex match
            Match match = Regex.Match(valString, matchPattern, RegexOptions.IgnoreCase);
            return match.Success;
        }

        private bool ValPhoneNumer(string numberToValidate)
        {
            //north american phone numbers should be entered in this format 334-867-5309

            //create an array of the three sets of digits
            string[] phoneParts = numberToValidate.Split("-".ToCharArray());

            //if we have less than three sets, return false
            if (phoneParts.Length != 3)
            {
                return false;
            }
            else
            {
                //if the sets aren't the correct length, return false
                if ((phoneParts[0].Length != 3) || (phoneParts[1].Length != 3) || (phoneParts[2].Length != 4))
                {
                    return false;
                }
                else
                {
                    //if any of the sets can't be parsed as an integer, return false
                    for (int i = 0; i < 3; i++)
                    {
                        int parseResult;
                        if (!Int32.TryParse(phoneParts[i], out parseResult))
                        {
                            return false;
                        }
                    }
                }

            }

            //we got here after all our previous checks, return true
            return true;
        }

    }
}