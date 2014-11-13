using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;

namespace DemoCrm
{
    /// <summary>
    /// This class holds the interface business logic we want to test for the second sample
    /// </summary>
    public class Sample02
    {
        /// <summary>
        /// Creates a new account with a given name and then creates a follow-up task linked to the account
        /// </summary>
        /// <param name="accountName">account name</param>
        /// <param name="service">organization service</param>
        public static void CreateCrmAccount2(string accountName, IOrganizationService service)
        {
            //create the account
            Entity account = new Entity("account");
            account["name"] = accountName;
            Guid newId = service.Create(account);

            //get the account number
            account = service.Retrieve("account", newId, new Microsoft.Xrm.Sdk.Query.ColumnSet(new string[] { "name", "accountid", "accountnumber" }));
            string accountNumber = account["accountnumber"].ToString();

            //create the task
            Entity task = new Entity("task");
            task["subject"] = "Finish account set up for " + accountName + " - " + accountNumber;
            task["regardingobjectid"] = new Microsoft.Xrm.Sdk.EntityReference("account", newId);
            service.Create(task);
        }

    }
}
