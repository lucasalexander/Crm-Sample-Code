This is a collection of sample code for working with Microsoft Dynamics CRM I have used in a variety of blog posts. Many of the samples were originally written for Dynamics CRM 2011, but each solution has been updated to use the Microsoft Dynamics CRM 2013 Service Pack 1 Update Rollup 1 SDK (version 6.1.1 installed from [NuGet](http://www.nuget.org/packages/Microsoft.CrmSdk.Workflow/6.1.1)). I have verified that each solution builds, but I have not necessarily deployed or tested. USE AT YOUR OWN RISK!

###CrmAccessTeamMover
This is the solution for a console application to copy/update access team template records from one Dynamics CRM organization to another. It's described in detail in my ["Console application for moving Dynamics CRM access team templates"](http://www.alexanderdevelopment.net/post/2014/12/13/console-application-for-moving-dynamics-crm-access-team-templates) blog post. 

###CrmDataSnapshots
This solution shows an approach to taking data "snapshots" in Dynamics CRM, similar to Salesforce's analytic snapshots feature. The solution is described in detail in my ["A Data Snapshot Framework for Dynamics CRM"](http://www.alexanderdevelopment.net/post/2013/07/24/data-snapshot-framework-for-dynamics-crm) blog post. 

###CrmJsonProcessing
This solution shows an approach to working with JSON-formatted messages in Dynamics CRM custom workflow activities. This code sample was originally described in the MSDN code samples gallery [here](https://code.msdn.microsoft.com/Postingprocessing-JSON-in-396ead03). 

###CrmKeyValueManager
This solution shows an approach to working with key-value pair (KVP) data in Dynamics CRM custom workflow activities. This approach was originally discussed in these two blog posts:

- ["Working with key-value pair data inside Microsoft Dynamics CRM workflows"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Working-with-key-value-pair-data-inside-Microsoft-Dynamics-CRM/ba-p/152911)
- ["Working with key-value pair data inside Microsoft Dynamics CRM workflows – part 2"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Working-with-key-value-pair-data-inside-Microsoft-Dynamics-CRM/ba-p/153037).

###CrmMessageQueuing
This is a collection of code for using [RabbitMQ](http://www.rabbitmq.com/) as a message broker with Dynamics CRM data interfaces. I haven't yet published the explanatory blog posts, but I expect them to be online by the end of January. For now I've attempted so summarize each component in the collection.

- LucasCrmMessageQueueTools/CliConsumer - This is a sample console application that reads messages from a RabbitMQ queue and writes them to the CLI.
- LucasCrmMessageQueueTools/CliProvider - This is a sample console application that reads messages from the CLI and publishes them to a RabbitMQ exchange.
- LucasCrmMessageQueueTools/LeadWriterSample - This a sample console application that reads messages from a RabbitMQ queue and creates corresponding lead records in Dynamics CRM.
- LucasCrmMessageQueueTools/MessageQueuePlugin - This is a CRM plug-in that publishes notification messages to a RabbitMQ exchange using the RabbitMQ .Net client. This plug-in cannot be executed in the Dynamics CRM sandbox.
- LucasCrmMessageQueueTools/MessageQueueSandboxPlugin - This is a CRM plug-in that posts notification messages a Node.js application (see the "node-app" item below), which then publishes the messages to a RabbitMQ exchange using the [node-amqp library](https://github.com/postwait/node-amqp/).
- node-app - This contains the queuewriter.js Node.js application that is used by the MessageQueueSandboxPlugin plug-in. Additionally the leadform.htm web form can be used to submit lead data that will be processed by the LeadWriterSample application.

###CrmRegexTools
This solution shows how to validate and extract text inside Dynamics CRM custom workflow activities using regular expressions. This approach was originally discussed in these two blog posts:

- ["Using regular expressions in Dynamics CRM 2011 processes"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Using-regular-expressions-in-Dynamics-CRM-2011-processes/ba-p/145437)
- ["Extracting data with regular expressions in Microsoft Dynamics CRM 2011 processes"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Extracting-data-with-regular-expressions-in-Microsoft-Dynamics/ba-p/145701).

###CrmScheduledWorkflows
This solution shows an method for scheduling recurring workflows in Dynamics CRM. The approach was originally described in my ["Scheduling recurring Dynamics CRM workflows with FetchXML"](http://www.alexanderdevelopment.net/post/2013/05/19/Scheduling-recurring-Dynamics-CRM-workflows-with-FetchXML) blog post.

###CrmStreamingNotifications
This is a proof-of-concept solution for implementing a near real-time streaming API for Dynamics CRM with Node.js and Socket.IO. A video demonstration of the solution in action can be seen here: http://youtu.be/j7rG9qD3ycg.

I also wrote a four-part blog series about this topic.

- ["Creating a near real-time streaming interface for Dynamics CRM with Node.js – part 1"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Creating-a-near-real-time-streaming-interface-for-Dynamics-CRM/ba-p/178149)
- ["Creating a near real-time streaming interface for Dynamics CRM with Node.js – part 2"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Creating-a-near-real-time-streaming-interface-for-Dynamics-CRM/ba-p/178153)
- ["Creating a near real-time streaming interface for Dynamics CRM with Node.js – part 3"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Creating-a-near-real-time-streaming-interface-for-Dynamics-CRM/ba-p/178160)
- ["Creating a near real-time streaming interface for Dynamics CRM with Node.js – part 4"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Creating-a-near-real-time-streaming-interface-for-Dynamics-CRM/ba-p/178495)

###CrmTeamConnection
This solution shows an approach to managing Dynamics CRM access team membership with connections and custom workflow activities. This approach was originally discussed in my ["Managing Microsoft Dynamics CRM 2013 access team membership using connections"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Managing-Microsoft-Dynamics-CRM-2013-access-team-membership/ba-p/152491) blog post.

###CrmUnitTesting
This solution contains samples from my ["Unit testing custom Microsoft Dynamics CRM code" series](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Unit-testing-custom-Microsoft-Dynamics-CRM-code-Part-1/ba-p/147009) that shows how to test custom CRM code with Moq and Visual Studio's unit testing tools. There are three demo projects and three corresponding Visual Studio unit testing projects:

- DemoCrm shows the testing approach described in parts [2](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Unit-testing-custom-Microsoft-Dynamics-CRM-code-Part-2/ba-p/147081), [3](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Unit-testing-custom-Microsoft-Dynamics-CRM-code-Part-3/ba-p/147387) and [4](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Unit-testing-custom-Microsoft-Dynamics-CRM-code-Part-4/ba-p/147611) of the series for basic SDK usage.
- DemoCrmPlugin shows the testing approach for plug-ins described in [part 6](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Unit-testing-custom-Microsoft-Dynamics-CRM-Code-Part-6/ba-p/148219) of the series.
- DemoCrmWorkflowActivities shows the testing approach for custom workflow activities described in parts [5](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Unit-testing-custom-Microsoft-Dynamics-CRM-Code-Part-5/ba-p/147873), [7](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Unit-testing-custom-Microsoft-Dynamics-CRM-Code-Part-7/ba-p/148385) and [8](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Unit-testing-custom-Microsoft-Dynamics-CRM-code-Part-8/ba-p/148395) of the series.

###misc-code-samples
This directory contains code samples that don't fit anywhere else.
- CrmUsernamePasswordValidator.cs and crmidentity.cs are both used to support validation of WCF services with Dynamics CRM user credentials as discussed in my ["Custom WCF service authentication using Microsoft Dynamics CRM credentials"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Custom-WCF-service-authentication-using-Microsoft-Dynamics-CRM/ba-p/143465) and ["Custom identity class to represent Dynamics CRM users in WCF services"](http://h30507.www3.hp.com/t5/Applications-Services-Blog/Custom-identity-class-to-represent-Dynamics-CRM-users-in-WCF/ba-p/144925) blog posts.

