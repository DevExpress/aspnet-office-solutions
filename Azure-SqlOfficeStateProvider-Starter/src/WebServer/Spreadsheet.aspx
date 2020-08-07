<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Spreadsheet.aspx.cs" Inherits="WebServer.Spreadsheet" %>

<%@ Register Assembly="DevExpress.Web.ASPxSpreadsheet.v20.1, Version=20.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxSpreadsheet" TagPrefix="dx" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <dx:ASPxSpreadsheet ID="ASPxSpreadsheet1" runat="server" WorkDirectory="~/App_Data/WorkDirectory" FullscreenMode="true">
        </dx:ASPxSpreadsheet>
    </form>
</body>
</html>
