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
    /// This class holds our VS unit test methods for the first sample
    /// </summary>
    [TestClass]
    public class TestSample01
    {
        /// <summary>
        /// Tests the CreateCrmAccount method
        /// </summary>
        [TestMethod]
        public void CreateCrmAccount_Test()
        {
            //ARRANGE - set up everything our test needs

            //first - set up a mock service to act like the CRM organization service
            var serviceMock = new Mock<IOrganizationService>();
            IOrganizationService service = serviceMock.Object;

            //next - set a name for our fake account record to create
            string accountName = "Lucas Demo Company";

            //next - create a guid that we want our mock service Create method to return when called
            Guid idToReturn = Guid.NewGuid();

            //next - create an entity object that will allow us to capture the entity record that is passed to the Create method
            Entity actualEntity = new Entity();

            //finally - tell our mock service what to do when the Create method is called
            serviceMock.Setup(t =>
                t.Create(It.IsAny<Entity>())) //when Create is called with any entity as an invocation parameter
                .Returns(idToReturn) //return the idToReturn guid
                .Callback<Entity>(s => actualEntity = s); //store the Create method invocation parameter for inspection later

            //ACT - do the thing(s) we want to test

            //call the CreateCrmAccount method like usual, but supply the mock service as an invocation parameter
            Guid actualGuid = Sample01.CreateCrmAccount(accountName, service);

            //ASSERT - verify the results are correct

            //verify the entity created inside the CreateCrmAccount method has the name we supplied 
            Assert.AreEqual(accountName, actualEntity["name"]);

            //verify the guid returned by the CreateCrmAccount is the same guid the Create method returns
            Assert.AreEqual(idToReturn, actualGuid);
        }
    }
}
