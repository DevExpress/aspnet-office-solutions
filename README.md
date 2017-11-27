# Overview

This repository contains a set of ready-to-use **state providers** and Azure **starter solutions** to choose from when building scalable web applications with the use of DevExpress ASP.NET Office document processing components ([Spreadsheet](https://documentation.devexpress.com/AspNet/16157/ASP-NET-WebForms-Controls/Spreadsheet) and [Rich Text Editor](https://documentation.devexpress.com/AspNet/17721/ASP-NET-WebForms-Controls/Rich-Text-Editor)).

## Office document state providers

An Office document state provider allows several web server instances to work together when processing a set of documents by persisting all opened documents' states in a separate storage. Web servers check-out the requested documents from storage, process them and return the latest document states. This means that web servers do not maintain any server-specific state between requests using a document state provider, which enables the application to be dynamically scaled.

The following Office document state providers are available: 

* **RedisOfficeStateProvider**  
A state provider implementation that uses a Redis cache as the document state storage.

* **SqlOfficeStateProvider**  
A state provider implementation that uses an SQL Server database as the document state storage.


A solution that uses a state provider is highly scalable and provides support for Web Gardens, Web Farms, and clouds with IIS/ASP.NET-based web servers.

## Azure starter solutions

We have prepared the following Microsoft Azure starter templates to help you get started with application scalability when using our ASP.NET Spreadsheet and Rich Text Editor controls:

* **Azure-RedisOfficeStateProvider-Starter**  
An example of an [Azure Cloud Service](https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-choose-me) that uses the [Azure Redis Cache](https://docs.microsoft.com/en-us/azure/redis-cache/) to store document states between user requests to multiple web servers.

* **Azure-SqlOfficeStateProvider-Starter**  
An example of an [Azure Cloud Service](https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-choose-me) that uses the [Azure SQL Database](https://docs.microsoft.com/en-us/azure/sql-database/) to store document states between user requests to multiple web servers.

* **Azure-SessionAffinity-Starter**  
An example of an [Azure Cloud Service](https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-choose-me) that uses the session affinity paradigm and custom-implemented smart request routing that routes requests from a user to the web server instance that has the requested document loaded in RAM.


# Choosing What to Use

Use the tables below to determine the appropriate approach.

## Which starter solution to choose

The following table compares the available starter templates:

| Azure Solution| Ease of Adaptation | Scalability  | Performance |
| --- |:---:|:---:|:---:|
| Session Affinity-based | Complex solution. Requires building specific application architecture | High, but may require additional coding | Maximum |
| Redis/SQL State Provider-based | Simple to use | Easy to scale | Slower than Session Affinity |

## Which provider to choose

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
We would appreciate it if you [participate in this short survey]
(https://docs.google.com/forms/d/e/1FAIpQLSeRlfTWhrRfHei8LgNPgC-Ol9dT9sX773_Pmo8-X-XRfQpSww/viewform) (4 questions, ~2 min).
