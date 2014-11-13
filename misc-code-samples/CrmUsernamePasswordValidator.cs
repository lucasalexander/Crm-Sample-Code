using System;
using System.Configuration;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Principal;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using System.ServiceModel;
using System.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CustomValidators
{
    /// <summary>
    /// class used for username/password authentication against CRM
    /// </summary>
    public class CrmUsernamePasswordValidator : UserNamePasswordValidator
    {
        /// <summary>
        /// Validate method to attempt to connect to CRM with supplied username/password and then execute a whoami request
        /// </summary>
        /// <param name="username">crm username</param>
        /// <param name="password">crm password</param>
        public override void Validate(string username, string password)
        {
            //get the httpcontext so we can store the user guid for impersonation later
            HttpContext context = HttpContext.Current;

            //if username or password are null, obvs we can't continue
            if (null == username || null == password)
            {
                throw new ArgumentNullException();
            }

            //get the crm connection using the simplified connection string method

            // the following assumes the server url is stored in the web.config appsettings collection like so:
            // <add key="crmconnectionstring" value="Url=https://crm.example.com; "/>
            string connectionString = ConfigurationManager.AppSettings["crmconnectionstring"];
            connectionString += " Username=" + username + "; Password=" + password;

            Microsoft.Xrm.Client.CrmConnection connection = CrmConnection.Parse(connectionString);

            //try the whoami request
            //if it fails (user can't be authenticated, is disabled, etc.), the client will get a soap fault message
            using (OrganizationService service = new OrganizationService(connection))
            {
                try
                {
                    WhoAmIRequest req = new WhoAmIRequest();
                    WhoAmIResponse resp = (WhoAmIResponse)service.Execute(req);
                    List<string> roles = GetUserRoles(resp.UserId, service);
                    context.User = new GenericPrincipal(new GenericIdentity(resp.UserId.ToString()), roles.ToArray());
                }
                catch (System.ServiceModel.Security.MessageSecurityException ex)
                {
                    throw new FaultException(ex.Message); 
                }
                catch (Exception ex)
                {
                    throw new FaultException(ex.Message);
                }
            }
        }

        /// <summary>
        /// retrieves a list of CRM roles assigned to a specific user
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        private List<string> GetUserRoles(Guid userid, OrganizationService service)
        {
            List<string> roles = new List<string>();

            string fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
              <entity name='role'>
                <attribute name='name' />
                <attribute name='businessunitid' />
                <attribute name='roleid' />
                <order attribute='name' descending='false' />
                <link-entity name='systemuserroles' from='roleid' to='roleid' visible='false' intersect='true'>
                  <link-entity name='systemuser' from='systemuserid' to='systemuserid' alias='af'>
                    <filter type='and'>
                      <condition attribute='systemuserid' operator='eq' uitype='systemuser' value='{$USERID}' />
                    </filter>
                  </link-entity>
                </link-entity>
              </entity>
            </fetch>";
            fetchXml = fetchXml.Replace("$USERID", userid.ToString());
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (Entity entity in results.Entities)
            {
                roles.Add((string)entity["name"]);
            }
            return roles;
        }
    }
}