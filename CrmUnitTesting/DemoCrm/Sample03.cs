using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;

namespace DemoCrm
{
    /// <summary>
    /// This class holds the interface business logic we want to test for the third sample
    /// </summary>
    public class Sample03
    {
        /// <summary>
        /// Returns the number of options for a picklist
        /// </summary>
        /// <param name="entityName">name of the entity</param>
        /// <param name="picklistName">name of the picklist</param>
        /// <param name="service">CRM service</param>
        /// <returns>integer count</returns>
        public static int GetPicklistOptionCount(string entityName, string picklistName, IOrganizationService service)
        {
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = picklistName,
                RetrieveAsIfPublished = true
            };

            // Execute the request.
            RetrieveAttributeResponseWrapper retrieveAttributeResponse = (new RetrieveAttributeResponseWrapper(service.Execute(retrieveAttributeRequest))); //this is the only change from before
            // Access the retrieved attribute.

            PicklistAttributeMetadata retrievedPicklistAttributeMetadata = (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
            OptionMetadata[] optionList = retrievedPicklistAttributeMetadata.OptionSet.Options.ToArray();
            return optionList.Length;
        }

    }
}
