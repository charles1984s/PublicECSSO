<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TForm.aspx.cs" Inherits="ECSSO.api.TForm" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server" action="GameData.ashx">
    <div>
        <asp:TextBox ID="code" runat="server"></asp:TextBox>
        <asp:TextBox ID="ItemData" runat="server" TextMode="MultiLine" Height="222px" Width="553px"></asp:TextBox>
        <asp:Button ID="Button1" runat="server" Text="send" />
    </div>
    </form>
</body>
</html>
