# Overview

This example demonstrates how to use the DevExpress ASP.NET Spreadsheet and Rich Text Editor controls with the Redis Document State Provider in Microsoft Azure.

# Structure 

This solution consists of the following components:

* **Azure-RedisOfficeStateProvider-Starter** - The *Microsoft Azure Cloud Service* project.
* **WebServer** - The *ASP.NET Web Server Role*.

## Azure-RedisOfficeStateProvider-Starter

The *Microsoft Azure Cloud Service* is a special project designed to configure your application in both cloud and local environments.

Refer to the official Microsoft documentation for more information on how to [create](https://docs.microsoft.com/en-us/azure/vs-azure-tools-azure-project-create) and [configure](https://docs.microsoft.com/en-us/azure/vs-azure-tools-configure-roles-for-cloud-service) Azure cloud services.

## WebServer

This is a simple ASP.NET web project. It has the following pages:
* **RichEdit.aspx** - Contains a single ASPxRichEdit control. No additional markup or code is needed.
* **Spreadsheet.aspx** - Contains a single ASPxSpreadsheet control. No additional markup or code is needed.

This project also contains:
* **Web.config** - A standard Web.config file with DevExpress control registrations and a State Storage connection string. 
* **Global.asax** - A standard Global.asax file with the RedisOfficeStateProvider registration in the *Application_Start* method.

## References 

This solution references the DevExpress ASP.NET controls and the *RedisOfficeStateProvider* project.

# Requirements

* Microsoft Azure SDK for .NET - 2.9

# How to use it

>Note: Microsoft Azure is a paid commercial product which may include usage charges. Refer to the official [Microsoft Azure site](https://azure.microsoft.com) for further details.

1. Install the DevExpress ASP.NET Controls. You can download the installation from the [DevExpress site page](https://www.devexpress.com/Home/try.xml).
2. Download the current solution's source files.
3. Download the RedisOfficeStateProvider project's source files.
4. Ensure that the reference to the RedisOfficeStateProvider project is working.
5. Create a Redis Cache in Microsoft Azure.
6. Add a connection string to the Redis Cache (deployed to Microsoft Azure) to the Web.config file.
7. Publish the solution to Microsoft Azure using your Microsoft Azure credentials (refer to the official [MS Azure site](https://azure.microsoft.com) for details).

Three web server instances are created and deployed based on the *WebServer* project by default.

After the publishing process is completed, you can open the RichEdit.aspx or Spreadsheet.aspx pages to access the corresponding controls. Different requests may be processed on different web server instances (each represented by the *WebServer* project). The document states are stored within the Redis Cache in Microsoft Azure. You can modify the web servers, reboot some of them, increase or decrease their number, and continue working with the same documents.

# Contacts

If you have any feedback, contact us at support@devexpress.com.
