using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using Moq;

namespace DemoCrmWorkflowActivities.Test
{
    /// <summary>
    /// Set of happy-path tests for the GetLocation
    /// Tests are labeled [name_of_method_under_test]+_Test_+additional_details
    /// </summary>
    [TestClass]
    public class TestGetLocation
    {
        /// <summary>
        /// This tests a happy-path request/response
        /// </summary>
        [TestMethod]
        public void GetLocation_Test_Success()
        {

            //ARRANGE
            string bingKey = "GOOD-KEY";
            string country = "US";
            string stateProvince = "WA";
            string city = "Redmond";
            string postalCode = "98052";
            string address = "1 Microsoft Way";


            //get new GetLocation object
            GetLocation getLocation = new GetLocation();

            //instantiate the workflowinvoker
            var invoker = new WorkflowInvoker(getLocation);

            //create our mocks
            var serviceMock = new Mock<IOrganizationService>();
            var factoryMock = new Mock<IOrganizationServiceFactory>();
            var tracingServiceMock = new Mock<ITracingService>();
            var workflowContextMock = new Mock<IWorkflowContext>();

            //set up a mock service to act like the CRM organization service
            IOrganizationService service = serviceMock.Object;

            //set up a mock workflowcontext
            var workflowUserId = Guid.NewGuid();
            var workflowCorrelationId = Guid.NewGuid();
            var workflowInitiatingUserId = Guid.NewGuid();

            workflowContextMock.Setup(t => t.InitiatingUserId).Returns(workflowInitiatingUserId);
            workflowContextMock.Setup(t => t.CorrelationId).Returns(workflowCorrelationId);
            workflowContextMock.Setup(t => t.UserId).Returns(workflowUserId);
            var workflowContext = workflowContextMock.Object;

            //set up a mock tracingservice - will write output to console for now. maybe should store somewhere and read for asserts later?
            tracingServiceMock.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((t1, t2) => Console.WriteLine(t1, t2));
            var tracingService = tracingServiceMock.Object;

            //set up a mock servicefactory
            factoryMock.Setup(t => t.CreateOrganizationService(It.IsAny<Guid>())).Returns(service);
            var factory = factoryMock.Object;

            invoker.Extensions.Add<ITracingService>(() => tracingService);
            invoker.Extensions.Add<IWorkflowContext>(() => workflowContext);
            invoker.Extensions.Add<IOrganizationServiceFactory>(() => factory);

            var inputs = new Dictionary<string, object> 
            {
                { "BingKey", bingKey},
                { "Country", country},
                { "StateProvince", stateProvince},
                { "City", city},
                { "PostalCode", postalCode},
                { "Address", address}
            };
            string mockResponse = "";
            using (System.IO.StreamReader sr = new System.IO.StreamReader("map-response-good.xml"))
            {
                String line = sr.ReadToEnd();
                mockResponse += line;
            }

            string expectedUrl = "http://dev.virtualearth.net/REST/v1/Locations/US/WA/98052/Redmond/1%20Microsoft%20Way?o=xml&key=GOOD-KEY";
            WebRequest.RegisterPrefix(expectedUrl, new TestWebRequestCreate());
            TestWebRequest request = TestWebRequestCreate.CreateTestRequest(mockResponse);
            
            //ACT
            var outputs = invoker.Invoke(inputs);
            string lat = Convert.ToString(outputs["Latitude"]);
            string lon = Convert.ToString(outputs["Longitude"]);

            //ASSERT
            Assert.AreEqual("47.640120461583138", lat);
            Assert.AreEqual("-122.12971039116383", lon);
            
        }
    }
}
