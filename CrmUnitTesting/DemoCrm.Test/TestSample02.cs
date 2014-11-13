using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DemoCrm.Test
{
    /// <summary>
    /// This class holds our VS unit test methods for the second sample
    /// </summary>
    [TestClass]
    public class TestSample02
    {
        /// <summary>
        /// Tests the CreateCrmAccount2 method
        /// </summary>
        [TestMethod]
        public void CreateCrmAccount2_Test()
        {
            //ARRANGE - set up everything our test needs

            //first - set up a mock service to act like the CRM organization service
            var serviceMock = new Mock<IOrganizationService>();
            IOrganizationService service = serviceMock.Object;

            //next - set a name and account number for our fake account record to create
            string accountName = "Lucas Demo Company";
            string accountNumber = "LPA1234";

            //next - create a guid that we want our mock service Create method to return when called
            Guid idToReturn = Guid.NewGuid();

            //next - create an object that will allow us to capture the account object that is passed to the Create method
            Entity createdAccount = new Entity();

            //next - create an entity object that will allow us to capture the task object that is passed to the Create method
            Entity createdTask = new Entity();

            //next - create an mock account record to pass back to the Retrieve method
            Entity mockReturnedAccount = new Entity("account");
            mockReturnedAccount["name"] = accountName;
            mockReturnedAccount["accountnumber"] = accountNumber;
            mockReturnedAccount["accountid"] = idToReturn;
            mockReturnedAccount.Id = idToReturn;

            //finally - tell our mock service what to do when the CRM service methods are called

            //handle the account creation
            serviceMock.Setup(t =>
                t.Create(It.Is<Entity>(e => e.LogicalName.ToUpper() == "account".ToUpper()))) //only match an entity with a logical name of "account"
                .Returns(idToReturn) //return the idToReturn guid
                .Callback<Entity>(s => createdAccount = s); //store the Create method invocation parameter for inspection later

            //handle the task creation
            serviceMock.Setup(t =>
                t.Create(It.Is<Entity>(e => e.LogicalName.ToUpper() == "task".ToUpper()))) //only match an entity with a logical name of "task"
                .Returns(Guid.NewGuid()) //can return any guid here
                .Callback<Entity>(s => createdTask = s); //store the Create method invocation parameter for inspection later

            //handle the retrieve account operation
            serviceMock.Setup(t =>
                t.Retrieve(
                        It.Is<string>(e => e.ToUpper() == "account".ToUpper()),
                        It.Is<Guid>(e => e == idToReturn),
                        It.IsAny<Microsoft.Xrm.Sdk.Query.ColumnSet>())
                    ) //here we match on logical name of account and the correct id
                .Returns(mockReturnedAccount);

            //ACT - do the thing(s) we want to test

            //call the CreateCrmAccount2 method like usual, but supply the mock service as an invocation parameter
            Sample02.CreateCrmAccount2(accountName, service);

            //ASSERT - verify the results are correct

            //verify the entity created inside the CreateCrmAccount method has the name we supplied 
            Assert.AreEqual(accountName, createdAccount["name"]);

            //verify task regardingobjectid is the same as the id we returned upon account creation
            Assert.AreEqual(idToReturn, ((Microsoft.Xrm.Sdk.EntityReference)createdTask["regardingobjectid"]).Id);
            Assert.AreEqual("Finish account set up for " + accountName + " - " + accountNumber, (string)createdTask["subject"]);
        }
    }
}
