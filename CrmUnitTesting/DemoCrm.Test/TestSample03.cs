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
    /// This class holds our VS unit test methods for the third sample
    /// </summary>
    [TestClass]
    public class TestSample03
    {
        [TestMethod]
        public void GetPicklistOptionCountTest_Test()
        {
            //ARRANGE - set up everything our test needs

            //first - set up a mock service to act like the CRM organization service
            var serviceMock = new Mock<IOrganizationService>();
            IOrganizationService service = serviceMock.Object;

            PicklistAttributeMetadata retrievedPicklistAttributeMetadata = new PicklistAttributeMetadata();
            OptionMetadata femaleOption = new OptionMetadata(new Label("Female", 1033), 43);
            femaleOption.Label.UserLocalizedLabel = new LocalizedLabel("Female", 1033);
            femaleOption.Label.UserLocalizedLabel.Label = "Female";
            OptionMetadata maleOption = new OptionMetadata(new Label("Male", 1033), 400);
            maleOption.Label.UserLocalizedLabel = new LocalizedLabel("Male", 400);
            maleOption.Label.UserLocalizedLabel.Label = "Male";
            OptionSetMetadata genderOptionSet = new OptionSetMetadata
            {
                Name = "gendercode",
                DisplayName = new Label("Gender", 1033),
                IsGlobal = true,
                OptionSetType = OptionSetType.Picklist,
                Options = { femaleOption, maleOption }
            };

            retrievedPicklistAttributeMetadata.OptionSet = genderOptionSet;
            RetrieveAttributeResponseWrapper picklistWrapper = new RetrieveAttributeResponseWrapper(new RetrieveAttributeResponse());
            picklistWrapper.AttributeMetadata = retrievedPicklistAttributeMetadata;

            serviceMock.Setup(t => t.Execute(It.Is<RetrieveAttributeRequest>(r => r.LogicalName == "gendercode"))).Returns(picklistWrapper);

            //ACT
            int returnedCount = Sample03.GetPicklistOptionCount("ANYENTITYMATCHES", "gendercode", service);

            //ASSERT
            Assert.AreEqual(2, returnedCount);
        }
    }
}
