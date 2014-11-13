using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DemoCrmWorkflowActivities.Test
{
    /// <summary>
    /// Set of tests for the exception throwing in ValidateRegex class
    /// Tests are labeled [name_of_method_under_test]+_Test_+additional_details
    /// </summary>
    [TestClass]
    public class TestValidateRegex_Sample02
    {
        /// <summary>
        /// This tests a null workflow context using the ExpectedException approach
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ValidateRegex_Test_NullContext_ExpectedException()
        {

            //ARRANGE

            //set matchpattern to nanp format of xxx-xxx-xxxx
            string matchPattern = @"^[2-9]\d{2}-\d{3}-\d{4}$";

            //set string to validate to a valid phone number
            string stringToValidate = "334-867-5309";

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

            //get new validateregex object
            ValidateRegex valRegex = new ValidateRegex();

            var invoker = new WorkflowInvoker(valRegex);
            invoker.Extensions.Add<ITracingService>(() => tracingService);
            //below line commented out to generate exception
            //invoker.Extensions.Add<IWorkflowContext>(() => workflowContext);
            invoker.Extensions.Add<IOrganizationServiceFactory>(() => factory);

            var inputs = new Dictionary<string, object> 
            {
            { "MatchPattern", matchPattern},
            { "StringToValidate", stringToValidate }
            };

            //ACT (assertion is implied)
            invoker.Invoke(inputs);
        }

        /// <summary>
        /// This tests a null workflow context using the try-catch approach
        /// </summary>
        [TestMethod]
        public void ValidateRegex_Test_NullContext_TryCatch()
        {

            //ARRANGE

            //set matchpattern to nanp format of xxx-xxx-xxxx
            string matchPattern = @"^[2-9]\d{2}-\d{3}-\d{4}$";

            //set string to validate to a valid phone number
            string stringToValidate = "334-867-5309";

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

            //get new validateregex object
            ValidateRegex valRegex = new ValidateRegex();

            var invoker = new WorkflowInvoker(valRegex);
            invoker.Extensions.Add<ITracingService>(() => tracingService);
            //below line commented out to generate exception
            //invoker.Extensions.Add<IWorkflowContext>(() => workflowContext);
            invoker.Extensions.Add<IOrganizationServiceFactory>(() => factory);

            var inputs = new Dictionary<string, object> 
            {
            { "MatchPattern", matchPattern},
            { "StringToValidate", stringToValidate }
            };

            //instantiate an expection object we can use to see if we threw the right thing
            Exception expectedException = null;
            
            //ACT
            try
            {
                invoker.Invoke(inputs);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }
            
            //ASSERT
            Assert.IsInstanceOfType(expectedException, typeof(InvalidPluginExecutionException));
            Assert.AreEqual("Failed to retrieve workflow context.", expectedException.Message);
        }
    }
}
