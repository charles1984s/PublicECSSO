<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="ECSSO.HinetAPI.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label ID="Label1" runat="server" Text="因為您OpenID的email顯示為null，煩請您額外輸入Email當做會員id"></asp:Label>
        <br />
        <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox><asp:Button ID="確定" runat="server" Text="Button" OnClick="確定_Click" /><asp:Label ID="Label2" runat="server" Visible="False"></asp:Label>
        <br />        
    </div>
    </form>
</body>
</html>
