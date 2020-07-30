# Overview

This repository contains two ready-to-use **state providers** (for Redis and SQL data stores) and a set of **starter solutions** (for Azure and Amazon Web Services) to choose from when building scalable web applications with the use of DevExpress ASP.NET Office document processing components ([Spreadsheet](https://documentation.devexpress.com/AspNet/16157/ASP-NET-WebForms-Controls/Spreadsheet) and [Rich Text Editor](https://documentation.devexpress.com/AspNet/17721/ASP-NET-WebForms-Controls/Rich-Text-Editor)).

> **Starting with v20.1, we recommend that you use the client [RichEdit]( https://docs.devexpress.com/AspNetCore/400373/rich-edit) control instead of the server control with a state provider.** This allows you to avoid the following issues:
> 
> * RichEdit imports and exports a document model for every request to a state provider (for instance, changing a document content or opening a dialog). This consumes additional CPU resources.
> * A server's memory stores a document model because the control synchronizes the model between the server and client every time the model changes (for instance, text input or format change).
> * The server load increases non-linearly with the number of users.
> * Complex document and session management causes the [session expired error]( https://docs.devexpress.com/AspNet/401519/common-concepts/office-document-management/your-session-has-expired-error).
> * Complicated session error detection.
> * The server spell checker uses server memory and sends multiple checking requests to the server.
> * The server RichEdit cannot open documents over a certain size. However, the client RichEdit can open documents containing thousands of pages.
> * Closing the document can result in the loss of recent changes due to [client-server synchronization]( https://docs.devexpress.com/AspNet/401911/aspnet-webforms-controls/rich-text-editor/concepts/client-server-synchronization).
> 
> Note that the following features are not supported in the client RichEdit control.
> 
> * The DOC format; use third-party server libraries (for instance, [Office File API]( https://docs.devexpress.com/OfficeFileAPI/17488/word-processing-document-api)) for conversion
> * Document protection (coming in 20.2)
> * Spell check ([coming in 20.2](https://supportcenter.devexpress.com/internal/ticket/details/T913302))
> * Embedded file manager. You can use the [DevExtreme File Manager](https://js.devexpress.com/Documentation/Guide/Widgets/FileManager/Getting_Started_with_File_Manager/) to implement advanced file management functionality
> * [ASP themes]( https://docs.devexpress.com/AspNet/6655/common-concepts/appearance-customization-theming/available-themes); the control uses [DevExtreme themes]( https://js.devexpress.com/DevExtreme/Guide/Themes_and_Styles/Predefined_Themes/) only.

## State providers

An Office document state provider allows several web server instances to work together when processing a set of documents by persisting all opened documents' states in a separate storage. Web servers check-out the requested documents from storage, process them and return the latest document states. This means that web servers do not maintain any server-specific state between requests using a document state provider, which enables the application to be dynamically scaled.

The following Office document state providers are available: 

* **RedisOfficeStateProvider**  
A state provider implementation that uses a Redis cache as the document state storage.

* **SqlOfficeStateProvider**  
A state provider implementation that uses an SQL Server database as the document state storage.


A solution that uses a state provider is highly scalable and provides support for Web Gardens, Web Farms, and clouds with IIS/ASP.NET-based web servers.

## Starter solutions

We have prepared the following starter templates to help you get started with application scalability when using our ASP.NET Spreadsheet and Rich Text Editor controls.

### For Microsoft Azure:

* **Azure-RedisOfficeStateProvider-Starter**  
An example of an [Azure Cloud Service](https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-choose-me) that uses the [Azure Redis Cache](https://docs.microsoft.com/en-us/azure/redis-cache/) to store document states between user requests to multiple web servers.

* **Azure-SqlOfficeStateProvider-Starter**  
An example of an [Azure Cloud Service](https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-choose-me) that uses the [Azure SQL Database](https://docs.microsoft.com/en-us/azure/sql-database/) to store document states between user requests to multiple web servers.

* **Azure-SessionAffinity-Starter**  
An example of an [Azure Cloud Service](https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-choose-me) that uses the session affinity paradigm and custom-implemented smart request routing that routes requests from a user to the web server instance that has the requested document loaded in RAM.

### For Amazon Web Services:

* **AWS-RedisOfficeStateProvider-Starter**  
An example of a web application that uses [Amazon ElastiCache for Redis](https://aws.amazon.com/redis/) to store document states between user requests to multiple web servers. The application can be deployed to the [AWS Elastic Beanstalk](https://aws.amazon.com/elasticbeanstalk/) environment.

* **AWS-SqlOfficeStateProvider-Starter**  
An example of a web application that uses [Amazon Relational Database Services](https://aws.amazon.com/rds/) to store document states between user requests to multiple web servers. The application can be deployed to the [AWS Elastic Beanstalk](https://aws.amazon.com/elasticbeanstalk/) environment.

# Choosing What to Use

Use the tables below to determine the appropriate approach.

## Which starter solution to choose

### For Microsoft Azure

The following table compares the available starter templates:

| Azure Solution| Ease of Adaptation | Scalability  | Performance |
| --- |:---:|:---:|:---:|
| Session Affinity-based | Complex solution. Requires building specific application architecture | High, but may require additional coding | Maximum |
| Redis/SQL State Provider-based | Simple to use | Easy to scale | Slower than Session Affinity |


### For Amazon Web Services

**Note:** For the Amazon Web Services environment, we provide only Redis/SQL State Provider-based solutions since they are easy to use and can be embedded into your existing applications without any modification.

## Which provider to choose

Both state provider solutions are used in the available Microsoft Azure and Amazon Web Services starter solutions without modifications. The provider solutions can also be reused in custom projects for dynamically scaled web applications.

The following table compares the available document state providers:

| Provider | Performance | Autosave Support | Hibernation Support |
| --- |:---:|:---:|:---:|
| Redis Storage-based | High | Required. Needs custom coding | Required. Needs custom coding | 
| SQL Server Database-based | Slower than Redis | Not required, but possible with custom coding | Not required, but possible with custom coding |

Unlike Redis, an SQL Server database is a persistent storage. This means that an SQL server does not drop the document state after rebooting.

Redis uses RAM to store document states and requires that the Redis server has sufficient RAM to store all open documents' states.

The SQL Server stores document states in a database on a persistent storage (HDD/SDD) and requires that the SQL Server database has sufficient size to store all open documents' states.

**Choose Redis** if a developer can guarantee that the Redis server RAM has sufficient storage for all opened documents’ states. We recommend implementing custom document autosaving and hibernation - this reduces the Redis RAM consumption and improves Redis' persistence.

**Choose SQL** as a long-term solution when working with a large number of documents or if implementing custom document autosaving and hibernation is not possible.

# Contact Us
We would appreciate it if you [participate in this short survey](https://docs.google.com/forms/d/e/1FAIpQLSeRlfTWhrRfHei8LgNPgC-Ol9dT9sX773_Pmo8-X-XRfQpSww/viewform) (4 questions, ~2 min).
