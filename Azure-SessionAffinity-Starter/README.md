# Overview

This example demonstrates the concepts of how to build a stateful ASP.NET application that is able to work on multiple web servers while using the ASPxSpreadsheet or ASPxRichEdit control. For this solution, we have implemented a custom request routing functionality (called Custom Request Routing further in this topic).
 
# Common concepts

The Client Affinity, on which paradigm this solution is based, is a feature of the IIS' [Application Request Routing](https://docs.microsoft.com/en-us/iis/extensions/planning-for-arr/application-request-routing-version-2-overview) (ARR). This feature affinitizes (maps) requests from a user client to a web application server instance for the duration of a client session.

In this solution, we use a modified Client Affinity approach - the so called *Document Affinity*. The *Document Affinity* ensures that a user client "sticks" to a web server instance that hosts the currently requested document. If the client switches documents (by opening another one) it may be "re-stuck" to another web server instance.

Our *Custom Request Routing* that realizes the *Document Affinity* is implemented by a custom HTTP module - the Routing Module. The Routing Module routes all subsequent requests for the same document to the same web server instance that has once loaded the document into RAM. In this case, the application can always access the document state and there are no delays for document loading/migration.
 
# Functionality
 
The solution's proof-of-concept implementation has the following functionality:
 
* Storing a number of sample documents in a database.
 
* Providing the document hibernation functionality to unload documents from server RAMs to a hibernation database after a predefined period of document inactivity or on a web application shutdown.
 
* Providing several instances of the DocumentSite WebRole Server that implements the UI to work with documents.
 
* Providing a Routing WebRole Server instance to route document requests using the *Document Affinity* approach.
 
* Providing the document state migration for cases when the server that currently hosts the requested document shuts down.
 
# Getting started

## Solution structure

The solution consists of the following 8 projects:

* **Azure.CloudService.Documents** - The MS Azure Cloud Service project: a special project designed to configure your application in both cloud and local environments. Refer to the official Microsoft documentation for more information on how to [create](https://docs.microsoft.com/en-us/azure/vs-azure-tools-azure-project-create) and [configure](https://docs.microsoft.com/en-us/azure/vs-azure-tools-configure-roles-for-cloud-service) Azure cloud services.
 
* **Azure.WebRole.Routing** - The Web Role project that implements the IIS Web Farm configuration and provides Custom Request Routing.
 
* **Azure.WebRole.DocumentSite** - A sample web application that use the ASPxSpreadsheet/ASPxRichEdit control to work with documents stored in a database.
 
* **DevExpress.Azure** directory - A set of three projects providing custom implementation for Custom Request Routing between document server instances. The class libraries from this directory can be reused in your applications without modification.
 
* **DB.Documents.DAL** - The data access layer for a database containing sample documents.
 
* **DevExpress.Web.DatabaseHibernationProvider** - A custom hibernation provider for the ASPxSpreadsheet/ASPxRichEdit control.

This solution also includes sample database files. They are located in the Database directory.

## References 
 
This solution references the DevExpress ASP.NET Controls.

# Requirements

* Microsoft Azure SDK for .NET - 2.9

# How to use it

>Note: Microsoft Azure is a paid commercial product which may include usage charges. Refer to the official [Microsoft Azure site](https://azure.microsoft.com) for further details.

1. Install the DevExpress ASP.NET control. You can download the installation from the [DevExpress site page](https://www.devexpress.com/Home/try.xml).
 
2. Download the current solution's source files.
 
3. Deploy the Documents and HibernationStorage databases to Microsoft Azure. The databases are located in the Database directory.
 
4. Insert connection strings to the databases into the ServiceConfiguration.Cloud.cscfg file.
 
5. Create a Microsoft Azure Service Bus.
 
6. Specify values for the following parameters in the ServiceConfiguration.Cloud.cscfg file using the Service Bus information:

   * ServiceBusNamespace
   * ServiceBusSharedAccessKeyName
   * ServiceBusSharedAccessKey

7. Specify values for the following parameters in the Web.config file (within the DocumentSite and Routing web applications) using the Service Bus information:
 
   * ServiceBusNamespace
   * ServiceBusSharedAccessKeyName
   * ServiceBusSharedAccessKey

8. Publish the CloudService to Microsoft Azure using your Microsoft Azure credentials (refer to the [Microsoft Azure site](https://azure.microsoft.com) for details).

Two instances of the DocumentSite Web Role and one instance of the Routing Web Role are created by default.

After the publishing process is completed, you can open the document list using the CloudService's URL. Navigate to document and make some changes to its content. Use different devices or browsers to open the modified document. The changes will be shared across all devices/browsers.
 
# Contacts
 
If you have any feedback, contact us at support@devexpress.com.
