# Sql Office State Provider

Allows ASPxSpreadsheet/ASPxRichEdit to persist the document state in the MS SQL database.

## Overview

This project implements a document state provider that uses a Microsoft SQL Server database as a storage to keep states of documents opened by the DevExpress ASP.NET Spreadsheet and Rich Text Editor controls.

By using a document state provider, you can build high scalable web applications that provide support for Web Gardens, Web Farms, and clouds.

## Details

The SQL State Provider used in a scaled web application frees the app’s web server instances from keeping per-session document states in their server RAM between requests. Servers that have no state are equal and can process any request. This equality supports scalability in applications:
- load balancers can spread the workload among such “stateless” web server instances
- any server instance can be removed without data loss
- newly added web server instances immediately start participating in the application activity.

## Caution

The SQL State Provider's storage keeps internal representations of document states. The internal document state representation is designed to be written/read at a fast speed to/from an object graph representing a document in the storage's RAM. The internal states should not be available for end-users (through download/upload), and the storage for internal document states is not the same as (and should not be confused with) a database containing user documents. End-users should use the [open](https://documentation.devexpress.com/AspNet/117677/Common-Concepts/Office-Document-Management/Document-Loading) and [save](https://documentation.devexpress.com/AspNet/117676/Common-Concepts/Office-Document-Management/Document-Saving) functionality (available through UI or API) to work with their documents (read/write).

A typical end-user workflow is described below.

1. A user opens a document: the application loads the document from the user document database and passes the document to a control (Spreadsheet/Rich Text Editor) to open.
2. The user works with the document: the control interacts with the internal state storage (defined by the SQL State Provider).
3. The user saves the document: the control saves the document into a certain  Office file format, the application saves the resulting document's content to the user document database.
4. (Optional) The user closes the document: the application forces the control to close the document (the control disposes of the document's internal state in the internal state storage).

## Requirements

* Microsoft SQL Server

## Installation

1. Download the T-SQL script and correct it so it refers to your database.
2. Run this T-SQL script against your database (this script will set up the required set of Tables and Stored Procedures).
3. Download this project to your local computer.
4. Add a reference to this project into your solution (or build the project and add a reference to the resulting binary file).
5. Make sure that your application has the Global.asax file (add it to the application if required).
6. n the Global.asax file, put the following code inside the *Application_Start* event handler:

    ```c#
    var connectionString = "{Put your SQL Server connection string here}";
    var provider = new DevExpress.Web.SqlOfficeStateProvider.SqlOfficeStateProvider(connectionString);
    DevExpress.Web.Office.DocumentManager.StateProvider = provider;
    ```
After assigning the *DocumentManager.StateProvider* property, the Spreadsheet and Rich Text Editor controls will check-in/check-out the requested documents' states to/from the specified SQL Server dtabase instead of keeping states in web servers' RAM.

## Development

Feel free to clone this project and modify its sources based on your application requirements.

## Hibernation

Due to the persistence nature of SQL Server databases, there is no need to use the built-in [Hibernation](https://documentation.devexpress.com/AspNet/116408/Common-Concepts/Office-Document-Management/Document-Hibernation) feature of the Spreadsheet and Rich Text Editor controls to specifically keep document states. The only possible hibernation-related task that may occur is to clean up the database by removing the states of old unused documents from it. You can accomplish this task by writing custom code to track document states' last access time and remove the states of those documents that can be treated as "not used" (inactive for a long period of time).

## Autosave

The built-in [Auto-Saving](https://documentation.devexpress.com/AspNet/116407/Common-Concepts/Office-Document-Management/Document-Saving/Auto-Saving) feature of the Spreadsheet and Rich Text Editor controls does not work for external document state storages (such as an SQL database) out of the box. If your application requires this functionality, it is enough to manipulate documents using the following recommendations: 

* **Open documents** by calling the *[Open(documentId, contentAccessor)](https://documentation.devexpress.com/AspNet/117678/Common-Concepts/Office-Document-Management/Document-Loading/Opening-Documents)* method with the correct *DocumentId* and with no content provided. This will force the control to check-out the specified document's state from the storage.

* **Save documents** by calling the [Save/SaveCopy](https://documentation.devexpress.com/AspNet/117675/Common-Concepts/Office-Document-Management/Document-Saving/Standard-and-Custom-Saving) method to save the document to any required destination.

## Contacts

Sould you have any feedback, we would greatly appreciate in you cantact us: support@devexpress.com.
