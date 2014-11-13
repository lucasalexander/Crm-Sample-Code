using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;

namespace DemoCrm
{
    /// <summary>
    /// This class holds the interface business logic we want to test for the first sample
    /// </summary>
    public class Sample01
    {
        /// <summary>
        /// Creates a new account with a given name using the supplied organization service
        /// </summary>
        /// <param name="accountName">account name</param>
        /// <param name="service">organization service</param>
        /// <returns>id of the new account</returns>
        public static Guid CreateCrmAccount(string accountName, IOrganizationService service)
        {
            Entity account = new Entity("account");
            account["name"] = accountName;
            Guid newId = service.Create(account);
            return newId;
        }
   }
}