<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Spreadsheet.aspx.cs" Inherits="DocumentSite.Spreadsheet" %>

<%@ Register Assembly="DevExpress.Web.v19.1" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.ASPxSpreadsheet.v19.1" Namespace="DevExpress.Web.ASPxSpreadsheet" TagPrefix="dx" %>

<%@ Register Src="~/Diagnostic/Diagnostic.ascx" TagPrefix="uc1" TagName="Diagnostic" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <uc1:Diagnostic runat="server" id="Diagnostic" />
        <dx:ASPxSpreadsheet ID="ASPxSpreadsheet1" runat="server" ShowConfirmOnLosingChanges="false">
        </dx:ASPxSpreadsheet>
    </form>
</body>
</html>
