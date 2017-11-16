<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DocumentSite._Default" %>

<%@ Register Src="~/Diagnostic/Diagnostic.ascx" TagPrefix="uc1" TagName="Diagnostic" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <uc1:Diagnostic runat="server" id="Diagnostic" />
        <div>
            <ul>
                <asp:Repeater ID="Repeater1" runat="server">
                    <ItemTemplate>
                        <li>
                            <%# DocumentViewModel.Id %>
                            <span runat="server" visible="<%# DocumentViewModel.IsFolder %>">[</span>
                            <a href="<%# DocumentViewModel.OnClick %>" runat="server">
                                <%# DocumentViewModel.Name %>
                            </a>
                            <span runat="server" visible="<%# DocumentViewModel.IsFolder %>">]</span>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </div>
    </form>
</body>
</html>
