<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="formpost.aspx.cs" Inherits="ECSSO.formpost" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server" action="Shopping.aspx">
    <div>
        <!--MemID:<asp:TextBox ID="MemID" runat="server" Text="000001"></asp:TextBox><br />
        Orgname:<asp:TextBox ID="Orgname" runat="server" Text="derek"></asp:TextBox><br />
        ReturnUrl:<asp:TextBox ID="ReturnUrl" runat="server" Text="http://derek.ezsale.tw"></asp:TextBox>-->
        <asp:HiddenField ID="orderData" runat="server" />
        <asp:Button ID="Button1" runat="server" Text="Button" />
    </div>
    </form>
</body>
</html>
