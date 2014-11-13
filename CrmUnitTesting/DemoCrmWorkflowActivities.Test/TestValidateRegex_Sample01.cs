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
    /// Set of happy-path tests for the ValidateRegex class
    /// Tests are labeled [name_of_method_under_test]+_Test_+additional_details
    /// </summary>
    [TestClass]
    public class TestValidateRegex_Sample01
    {
        /// <summary>
        /// This tests a valid input string with a single NANP phone number pattern (xxx-xxx-xxxx)
        /// </summary>
        [TestMethod]
        public void ValidateRegex_Test_ValidPhone()
        {

            //ARRANGE

            //set matchpattern to nanp format of xxx-xxx-xxxx
            string matchPattern = @"^[2-9]\d{2}-\d{3}-\d{4}$";

            //set string to validate to a valid phone number
            string stringToValidate = "334-867-5309";

            //get new validateregex object
            ValidateRegex valRegex = new ValidateRegex();

            //instantiate the workflowinvoker
            var invoker = new WorkflowInvoker(valRegex);
            
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
                { "MatchPattern", matchPattern},
                { "StringToValidate", stringToValidate }
            };

            //ACT

            var outputs = invoker.Invoke(inputs);
            int valid = Convert.ToInt16(outputs["Valid"]);

            //ASSERT

            //0 = invalid, 1 = valid
            Assert.AreEqual(1, valid);
        }

        /// <summary>
        /// This tests an invalid input string with a single NANP phone number pattern (xxx-xxx-xxxx)
        /// </summary>
        [TestMethod]
        public void ValidateRegex_Test_InvalidPhone()
        {

            //ARRANGE

            //set matchpattern to nanp format of xxx-xxx-xxxx
            string matchPattern = @"^[2-9]\d{2}-\d{3}-\d{4}$";

            //set string to validate to a valid phone number
            string stringToValidate = "334-867-53099";

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
            invoker.Extensions.Add<IWorkflowContext>(() => workflowContext);
            invoker.Extensions.Add<IOrganizationServiceFactory>(() => factory);

            var inputs = new Dictionary<string, object> 
            {
                { "MatchPattern", matchPattern},
                { "StringToValidate", stringToValidate }
            };

            //ACT

            var outputs = invoker.Invoke(inputs);
            int valid = Convert.ToInt16(outputs["Valid"]);

            //ASSERT

            //0 = invalid, 1 = valid
            Assert.AreEqual(0, valid);
        }
    }
}
