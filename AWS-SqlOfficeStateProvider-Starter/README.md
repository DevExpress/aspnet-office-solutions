# Overview

This example demonstrates how to use the DevExpress ASP.NET Spreadsheet and Rich Text Editor controls with the SQL Document State Provider in Amazon Web Services.

# Structure 

This solution contains a single project - a simple ASP.NET web application that can be deployed to the [AWS Elastic Beanstalk](https://aws.amazon.com/elasticbeanstalk/) environment.

The project has the following pages:
* **RichEdit.aspx** - Contains a single ASPxRichEdit control. No additional markup or code is needed.
* **Spreadsheet.aspx** - Contains a single ASPxSpreadsheet control. No additional markup or code is needed.

This project additionally contains:
* **Web.config** - A standard Web.config file with DevExpress control registrations and a State Storage connection string. 
* **Global.asax** - A standard Global.asax file with the SqlOfficeStateProvider registration in the *Application_Start* method.

## References 

This solution references the DevExpress ASP.NET controls and the *SqlOfficeStateProvider* project.

# Requirements

No additional software has to be installed to open the project in the Microsoft Visual Studio environment. 

# How to use it

>Note: Amazon Web Services is a paid commercial product which may include usage charges. Refer to the official [AWS website](https://aws.amazon.com/) for further details.

1. Install the DevExpress ASP.NET Controls. You can download the installation from the [DevExpress site page](https://www.devexpress.com/Home/try.xml).
2. Download the current solution's source files.
3. Download the SqlOfficeStateProvider project's source files.
4. Ensure that the reference to the SqlOfficeStateProvider project is working.
5. Create a Microsoft SQL Server Database in the [Amazon Relational Database Service (RDS)](https://aws.amazon.com/rds/), and copy the database connection string (it will be used in the following steps).
6. Connect your local *Microsoft SQL Server Management Studio* to the Microsoft SQL Server on Amazon RDS and run the initialization script (SqlOfficeStateProvider/sql/init.sql) against the new database.
7. Insert the copied connection string into the Web.config file.
8. Publish the solution to AWS Elastic Beanstalk using your AWS credentials (refer to the official [AWS website](https://aws.amazon.com/) for details). 

To test all advantages of the stateless mode, the web site should be deployed as a multi-instance environment.

After the publishing process is completed, you can open the RichEdit.aspx or Spreadsheet.aspx pages to access the corresponding controls. Different requests may be processed on different web server instances. The document states are stored within the Microsoft SQL Server database. You can modify the web servers, reboot some of them, increase or decrease their number, and continue working with the same documents.

# Contacts

If you have any feedback, contact us at support@devexpress.com.
