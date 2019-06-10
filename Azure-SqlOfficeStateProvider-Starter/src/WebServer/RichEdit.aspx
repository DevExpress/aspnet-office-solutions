<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RichEdit.aspx.cs" Inherits="WebServer.RichEdit" %>

<%@ Register Assembly="DevExpress.Web.ASPxRichEdit.v19.1, Version=19.1.3.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxRichEdit" TagPrefix="dx" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <dx:ASPxRichEdit ID="ASPxRichEdit1" runat="server" WorkDirectory="~\App_Data\WorkDirectory">
        </dx:ASPxRichEdit>
    </form>
</body>
</html>
