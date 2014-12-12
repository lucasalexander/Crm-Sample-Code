using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Client;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client.Services;
using System.Collections.Generic;

namespace CrmAccessTeamMover
{
    class Program
    {
        private static string sourceOrg;
        private static string targetOrg;
        private static CrmConnection sourceConn;
        private static CrmConnection targetConn;
        private static EntityCollection exported;

        static void Main(string[] args)
        {
            Console.WriteLine("Enter the source connection string: ");
            sourceOrg = Console.ReadLine();
            try
            {
                sourceConn = CrmConnection.Parse(sourceOrg);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Could not parse source connection string: {0}", ex.Message);
                return;
            }

            Console.WriteLine("Enter the destination connection string: ");
            targetOrg = Console.ReadLine();
            try
            {
                targetConn = CrmConnection.Parse(targetOrg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not parse destination connection string: {0}", ex.Message);
                return;
            }

            //export teamtemplates
            using (OrganizationService service = new OrganizationService(sourceConn))
            {
                try
                {
                    //attributes to exclude from the query
                    List<string> IgnoredAttributes = new List<string> { "issystem" };

                    Console.WriteLine("Retrieving entity metadata . . .");
                    RetrieveEntityRequest entityreq = new RetrieveEntityRequest
                    {
                        LogicalName = "teamtemplate",
                        EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Attributes
                    };
                    RetrieveEntityResponse entityres = (RetrieveEntityResponse)service.Execute(entityreq);
                    string fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                    fetchXml += "<entity name='teamtemplate'>";

                    foreach (AttributeMetadata amd in entityres.EntityMetadata.Attributes)
                    {
                        if (!IgnoredAttributes.Contains(amd.LogicalName))
                        {
                            fetchXml += "<attribute name='" + amd.LogicalName + "' />";
                            //Console.WriteLine(amd.LogicalName);
                        }
                    }
                    fetchXml += "</entity></fetch>";

                    Console.WriteLine("");
                    Console.WriteLine("Exporting data . . .");
                    exported = service.RetrieveMultiple(new FetchExpression(fetchXml));
                }
                catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                {
                    Console.WriteLine("Could not export data: {0}", ex.Message);
                    return;
                }
            }

            //import teamtemplates
            Console.WriteLine("Importing data . . .");
            using (OrganizationService service = new OrganizationService(targetConn))
            {
                if (exported.Entities.Count > 0)
                {
                    foreach (Entity entity in exported.Entities)
                    {
                        try
                        {
                            //Console.WriteLine("Id - {0}", entity.Id.ToString());

                            //try to update first
                            try
                            {
                                service.Update(entity);
                            }
                            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
                            {
                                //if update fails, then create
                                service.Create(entity);
                            }
                        }
                        catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                        {
                            //if everything fails, return error
                            Console.WriteLine("Error: {0} - {1}", entity.Id, entity["teamtemplatename"]);
                        }
                    }
                }
            }
            Console.WriteLine("Import complete");
            Console.WriteLine("");
            Console.WriteLine("Press the enter key to exit");
            Console.ReadLine();
        }
    }
}
