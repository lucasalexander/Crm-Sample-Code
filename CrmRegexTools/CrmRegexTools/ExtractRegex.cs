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
    /// <summary>
    /// This custom workflow activity is used to validate a string against a regular expression
    /// </summary>
    public sealed class ExtractRegex : CodeActivity
    {
        /// <summary>
        /// This is the string that will be parsed
        /// </summary>
        [Input("String to parse")]
        public InArgument<String> StringToParse { get; set; }

        /// <summary>
        /// This is the regular expression used for extraction - see C# regex quick reference here - http://msdn.microsoft.com/en-us/library/az24scfc.aspx
        /// </summary>
        [Input("Match pattern")]
        public InArgument<String> MatchPattern { get; set; }

        /// <summary>
        /// This specifies whether to return the first match, the last match or all matches (in a concatenated string). Acceptable values are First|Last|All.
        /// </summary>
        [Input("Return type")]
        public InArgument<String> ReturnType { get; set; }

        /// <summary>
        /// If "All" is specified as extraction type, this used as a string separator in the concatenated extract string
        /// </summary>
        [Input("String separator")]
        public InArgument<String> StringSeparator { get; set; }

        /// <summary>
        /// This returns the matching string if one is found or an empty string if a match is not found
        /// </summary>
        [Output("Extracted string")]
        public OutArgument<string> ExtractedString { get; set; }

        private string _processName = "ExtractRegex";

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


            //get all our inputs ready to work with

            //string to parse
            string parseString = StringToParse.Get(executionContext);

            //pattern to match
            string matchPattern = MatchPattern.Get(executionContext);
            
            //type of match to be returned - first match, last match or all matches
            ExtractionType extractType;
            switch (ReturnType.Get(executionContext).ToUpperInvariant())
            {
                case "FIRST":
                    extractType = ExtractionType.First;
                    break;
                case "LAST":
                    extractType = ExtractionType.Last;
                    break;
                case "ALL":
                    extractType = ExtractionType.All;
                    break;
                default:
                    //default will return first match only
                    extractType = ExtractionType.First;
                    break;
            }
            
            //separator to be used for an "all" match
            string stringSeparator = StringSeparator.Get(executionContext);
            
            //evaluate the regex and return the match(es)
            try
            {
                string extractedString = ExtractMatchingString(parseString, matchPattern, extractType, stringSeparator);
                ExtractedString.Set(executionContext, extractedString);
            }
            catch (Exception e)
            {
                tracingService.Trace("Exception: {0}", e.ToString());
                throw;
            }

            tracingService.Trace("Exiting " + _processName + ".Execute(), Correlation Id: {0}", context.CorrelationId);
        }

        /// <summary>
        /// method to evaluate a regular expression and return the first match, last match or all matches in a concatenated string
        /// </summary>
        /// <param name="parseString">string to parse</param>
        /// <param name="matchPattern">regular expression to evaluate</param>
        /// <param name="extractType">match(es) to return - first|last|all</param>
        /// <param name="separator">string to use as a separator for an "all" match return type</param>
        /// <returns></returns>
        private string ExtractMatchingString(string parseString, string matchPattern, ExtractionType extractType, string separator)
        {
            //set the default output to the empty string. if we match something, we'll change it.
            string output = string.Empty;

            //do the regex match
            MatchCollection matches = Regex.Matches(parseString, matchPattern);
            if (matches.Count > 0)
            {
                //which match(es) should we return?
                switch (extractType)
                {
                    case ExtractionType.First:
                        output = matches[0].Value;
                        break;
                    case ExtractionType.Last:
                        output = matches[matches.Count - 1].Value;
                        break;
                    case ExtractionType.All:
                        StringBuilder matchingSb = new StringBuilder();
                        for (int i = 0; i < matches.Count; i++)
                        {
                            matchingSb.Append(matches[i].Value);
                            if (i != matches.Count - 1)
                            {
                                matchingSb.Append(separator);
                            }
                        }
                        output = matchingSb.ToString();
                        break;
                }
            }
            return output;
        }
    }

    enum ExtractionType
    {
        First,
        Last,
        All
    }
}