<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RichEdit.aspx.cs" Inherits="DocumentSite.RichEdit" %>

<%@ Register Assembly="DevExpress.Web.v18.2" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.ASPxRichEdit.v18.2" Namespace="DevExpress.Web.ASPxRichEdit" TagPrefix="dx" %>

<%@ Register Src="~/Diagnostic/Diagnostic.ascx" TagPrefix="uc1" TagName="Diagnostic" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <uc1:Diagnostic runat="server" id="Diagnostic" />

        <dx:ASPxRichEdit ID="ASPxRichEdit1" runat="server" ShowConfirmOnLosingChanges="false">
        </dx:ASPxRichEdit>
    </form>
</body>
</html>

