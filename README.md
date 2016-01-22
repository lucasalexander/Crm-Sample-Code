This is a collection of sample code for working with Microsoft Dynamics CRM I have used in a variety of blog posts. Many of the samples were originally written for Dynamics CRM 2011, but each solution has been updated to use the Microsoft Dynamics CRM 2013 Service Pack 1 Update Rollup 1 SDK (version 6.1.1 installed from [NuGet](http://www.nuget.org/packages/Microsoft.CrmSdk.Workflow/6.1.1)). I have verified that each solution builds, but I have not necessarily deployed or tested. USE AT YOUR OWN RISK!

###CrmDataSnapshots
This solution shows an approach to taking data "snapshots" in Dynamics CRM, similar to Salesforce's analytic snapshots feature. The solution is described in detail in my ["A Data Snapshot Framework for Dynamics CRM"](http://www.alexanderdevelopment.net/post/2013/07/23/data-snapshot-framework-for-dynamics-crm) blog post. 

###CrmJsonProcessing
This solution shows an approach to working with JSON-formatted messages in Dynamics CRM custom workflow activities. This code sample was originally described in the MSDN code samples gallery [here](https://code.msdn.microsoft.com/Postingprocessing-JSON-in-396ead03). 

###CrmKeyValueManager
This solution shows an approach to working with key-value pair (KVP) data in Dynamics CRM custom workflow activities. This approach was originally discussed in these two blog posts:

- ["Working with key-value pair data inside Microsoft Dynamics CRM workflows"](http://alexanderdevelopment.net/post/2014/01/14/working-with-key-value-pair-data-inside-microsoft-dynamics-crm-workflows/)
- ["Working with key-value pair data inside Microsoft Dynamics CRM workflows – part 2"](http://alexanderdevelopment.net/post/2014/01/16/working-with-key-value-pair-data-inside-microsoft-dynamics-crm-workflows-part-2/).

###CrmMessageQueuing
This is a collection of code for using [RabbitMQ](http://www.rabbitmq.com/) as a message broker with Dynamics CRM data interfaces.

- LucasCrmMessageQueueTools/CliConsumer - This is a sample console application that reads messages from a RabbitMQ queue and writes them to the CLI.
- LucasCrmMessageQueueTools/CliProvider - This is a sample console application that reads messages from the CLI and publishes them to a RabbitMQ exchange.
- LucasCrmMessageQueueTools/LeadWriterSample - This a sample console application that reads messages from a RabbitMQ queue and creates corresponding lead records in Dynamics CRM.
- LucasCrmMessageQueueTools/MessageQueuePlugin - This is a CRM plug-in that publishes notification messages to a RabbitMQ exchange using the RabbitMQ .Net client. This plug-in cannot be executed in the Dynamics CRM sandbox.
- LucasCrmMessageQueueTools/MessageQueueSandboxPlugin - This is a CRM plug-in that posts notification messages a Node.js application (see the "node-app" item below), which then publishes the messages to a RabbitMQ exchange using the [node-amqp library](https://github.com/postwait/node-amqp/).
- node-app - This contains the queuewriter.js Node.js application that is used by the MessageQueueSandboxPlugin plug-in. Additionally the leadform.htm web form can be used to submit lead data that will be processed by the LeadWriterSample application.

Here are the relevant blog posts:

- [Using RabbitMQ as a message broker in Dynamics CRM data interfaces - part 1](http://alexanderdevelopment.net/post/2015/01/12/using-rabbitmq-as-a-message-broker-in-dynamics-crm-data-interfaces-part-1/)
- [Using RabbitMQ as a message broker in Dynamics CRM data interfaces - part 2](http://alexanderdevelopment.net/post/2015/01/14/using-rabbitmq-as-a-message-broker-in-dynamics-crm-data-interfaces-part-2/)
- [Using RabbitMQ as a message broker in Dynamics CRM data interfaces - part 3](http://alexanderdevelopment.net/post/2015/01/20/using-rabbitmq-as-a-message-broker-in-dynamics-crm-data-interfaces-part-3/)
- [Using RabbitMQ as a message broker in Dynamics CRM data interfaces - part 4](http://alexanderdevelopment.net/post/2015/01/22/using-rabbitmq-as-a-message-broker-in-dynamics-crm-data-interfaces-part-4/)
- [Using RabbitMQ as a message broker in Dynamics CRM data interfaces - part 5](http://alexanderdevelopment.net/post/2015/01/27/using-rabbitmq-as-a-message-broker-in-dynamics-crm-data-interfaces-part-5/)

###CrmQueueGetNext
This solution implements get next case functionality in Dynamics CRM and Unified Service Desk. It is discussed in these two blog posts: 

- ["Get next case functionality for Dynamics CRM"](http://alexanderdevelopment.net/post/2015/10/02/get-next-case-functionality-for-dynamics-crm/)
- ["Get next case functionality for CRM Unified Service Desk"](http://alexanderdevelopment.net/post/2015/10/08/get-next-case-functionality-for-crm-unified-service-desk/) 

###CrmRegexTools
This solution shows how to validate and extract text inside Dynamics CRM custom workflow activities using regular expressions. This approach was originally discussed in these two blog posts:

- ["Using regular expressions in Dynamics CRM 2011 processes"](http://alexanderdevelopment.net/post/2013/09/03/using-regular-expressions-in-dynamics-crm-2011-processes-2/)
- ["Extracting data with regular expressions in Microsoft Dynamics CRM 2011 processes"](http://alexanderdevelopment.net/post/2013/09/09/extracting-data-with-regular-expressions-in-microsoft-dynamics-crm-2011-processes-2/)

###CrmScheduledWorkflows
This solution shows an method for scheduling recurring workflows in Dynamics CRM. The approach was originally described in my ["Scheduling recurring Dynamics CRM workflows with FetchXML"](http://www.alexanderdevelopment.net/post/2013/05/18/scheduling-recurring-dynamics-crm-workflows-with-fetchxml/) blog post.

###CrmStreamingNotifications
This is a proof-of-concept solution for implementing a near real-time streaming API for Dynamics CRM with Node.js and Socket.IO. A video demonstration of the solution in action can be seen here: http://youtu.be/j7rG9qD3ycg.

I also wrote a four-part blog series about this topic.

- ["Creating a near real-time streaming interface for Dynamics CRM with Node.js – part 1"](http://alexanderdevelopment.net/post/2014/12/03/creating-a-near-real-time-streaming-interface-for-dynamics-crm-with-node-js-part-1/)
- ["Creating a near real-time streaming interface for Dynamics CRM with Node.js – part 2"](http://alexanderdevelopment.net/post/2014/12/05/creating-a-near-real-time-streaming-interface-for-dynamics-crm-with-node-js-part-2/)
- ["Creating a near real-time streaming interface for Dynamics CRM with Node.js – part 3"](http://alexanderdevelopment.net/post/2014/12/09/creating-a-near-real-time-streaming-interface-for-dynamics-crm-with-node-js-part-3/)
- ["Creating a near real-time streaming interface for Dynamics CRM with Node.js – part 4"](http://alexanderdevelopment.net/post/2014/12/11/creating-a-near-real-time-streaming-interface-for-dynamics-crm-with-node-js-part-4/)

###CrmTeamConnection
This solution shows an approach to managing Dynamics CRM access team membership with connections and custom workflow activities. This approach was originally discussed in my ["Managing Microsoft Dynamics CRM 2013 access team membership using connections"](http://alexanderdevelopment.net/post/2014/01/09/managing-microsoft-dynamics-crm-2013-access-team-membership-using-connections-2/) blog post.

###CrmUnitTesting
This solution contains samples from my ["Unit testing custom Microsoft Dynamics CRM code" series](http://alexanderdevelopment.net/post/2013/10/01/unit-testing-custom-microsoft-dynamics-crm-code-part-1/) series that shows how to test custom CRM code with Moq and Visual Studio's unit testing tools. There are three demo projects and three corresponding Visual Studio unit testing projects:

- DemoCrm shows the testing approach described in parts [2](http://alexanderdevelopment.net/post/2013/10/08/unit-testing-custom-microsoft-dynamics-crm-code-part-2/), [3](http://alexanderdevelopment.net/post/2013/10/09/unit-testing-custom-microsoft-dynamics-crm-code-part-3/) and [4](http://alexanderdevelopment.net/post/2013/10/16/unit-testing-custom-microsoft-dynamics-crm-code-part-4/) of the series for basic SDK usage.
- DemoCrmPlugin shows the testing approach for plug-ins described in [part 6](http://alexanderdevelopment.net/post/2013/10/20/unit-testing-custom-microsoft-dynamics-crm-code-part-6-plug-ins/) of the series.
- DemoCrmWorkflowActivities shows the testing approach for custom workflow activities described in parts [5](http://alexanderdevelopment.net/post/2013/10/16/unit-testing-custom-microsoft-dynamics-crm-code-part-5/), [7](http://alexanderdevelopment.net/post/2013/10/23/unit-testing-custom-microsoft-dynamics-crm-code-part-7-web-requests/) and [8](http://alexanderdevelopment.net/post/2013/10/23/unit-testing-custom-microsoft-dynamics-crm-code-part-8-exception-raising/) of the series.

###NodeClientDemo
This sample shows how to connect from Node.js to Dynamics CRM using AD FS and OAuth2 for authentication. This [blog post](http://www.alexanderdevelopment.net/post/2015/01/24/authenticating-from-a-node.js-client-to-dynamics-crm-via-ad-fs-and-oauth2) has additional information explaining the structure of the solution and the application flow.

###misc-code-samples
This directory contains code samples that don't fit anywhere else.
- CrmUsernamePasswordValidator.cs and crmidentity.cs are both used to support validation of WCF services with Dynamics CRM user credentials as discussed in my ["Custom WCF service authentication using Microsoft Dynamics CRM credentials"](http://alexanderdevelopment.net/post/2013/08/01/custom-wcf-service-authentication-using-microsoft-dynamics-crm-credentials-2/) and ["Custom identity class to represent Dynamics CRM users in WCF services"](http://alexanderdevelopment.net/post/2013/08/26/custom-identity-class-to-represent-dynamics-crm-users-in-wcf-services/) blog posts.

