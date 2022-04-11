<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MemberEdit.aspx.cs" Inherits="ECSSO.MemberEdit" culture="auto" meta:resourcekey="PageResource2" uiculture="auto" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <meta http-equiv="x-ua-compatible" content="IE=edge, chrome=1"/>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <style type="text/css"> 
        div { border: 0px; } 
        body{font-family:Arial,微軟正黑體,新細明體, Helvetica, sans-serif; color:#333333;}
        .col-md-2{text-align:right; padding-right:0px;}
        .row{ padding:4px 0px;}
        .col-md-10{padding-left:0px;}
        .btn {width:135px; height:37px; margin-right:10px;}
        .bs-footer{padding:0; margin:0; bottom:0; left:0; width:100%; position:fixed;}        
        #foot { background-image:url("img/infooter.jpg"); background-repeat:repeat-x; color:#898989; text-align:center; margin-top:20px;}
        @media (max-width: 979px) 
        {
            .col-md-2{text-align:left;}
            .col-md-10{padding-left:30px;}
        }
        @media (min-width: 979px) {
            .container{width:820px;}            
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <asp:HiddenField ID="siteid" runat="server" />
    <asp:HiddenField ID="language" runat="server" />
    <asp:HiddenField ID="returnurl" runat="server" />
    <div class="container">
        <div class="row">
            <div class="col-md-12" style=" padding:15px 0px;">
                <asp:Label ID="WebTitle" runat="server" 
                    style="font-size:xx-large; font-weight:bold; color:#555555;" 
                    meta:resourcekey="WebTitleResource2"></asp:Label>
            </div>
        </div>
        <div class="row">
            <div class="col-md-12" style="background-color:#f5f5f5; padding:10px 30px; border-top:1px solid #d4d4d4; border-bottom:1px solid #d4d4d4;">
                <span class="glyphicon glyphicon-user" style="font-size:large; margin-right:7px;"></span>
                <asp:Label ID="Label14" runat="server" Text="會員資料" 
                    meta:resourcekey="Label14Resource2"></asp:Label>
            </div>
        </div>
        <div class="row">
            <div class="col-md-2">
                <asp:Label ID="Label2" runat="server" Text="＊會員編號：" 
                    meta:resourcekey="Label2Resource2"></asp:Label></div>
            <div class="col-md-10"><asp:Label ID="MemberID" runat="server" 
                    meta:resourcekey="MemberIDResource2"></asp:Label></div>            
        </div>
        <div class="row">
            <div class="col-md-2">
                <asp:Label ID="Label3" runat="server" Text="＊姓名／名稱：" 
                    meta:resourcekey="Label3Resource2"></asp:Label></div>
            <div class="col-md-10">
                <asp:TextBox ID="CHName" runat="server" Width="300px" 
                    meta:resourcekey="CHNameResource2"></asp:TextBox>
                <asp:DropDownList ID="Sex" runat="server" meta:resourcekey="SexResource2">
                    <asp:ListItem Value="1" meta:resourcekey="ListItemResource3">先生</asp:ListItem>
                    <asp:ListItem Value="2" meta:resourcekey="ListItemResource4">小姐</asp:ListItem>
                </asp:DropDownList>
                <asp:Label ID="CheckName" runat="server" Text="" ForeColor="Red"></asp:Label>
            </div>
        </div>
        <div class="row">
            <div class="col-md-2"><asp:Label ID="Label4" runat="server" Text="剩餘紅利：" 
                    meta:resourcekey="Label4Resource2"></asp:Label></div>
            <div class="col-md-10"><asp:Label ID="bonusTotal" runat="server" 
                    meta:resourcekey="bonusTotalResource2"></asp:Label></div>
        </div>
        <div class="row">
            <div class="col-md-2">
                <asp:Label ID="Label5" runat="server" Text="生日：" 
                    meta:resourcekey="Label5Resource2"></asp:Label></div>
            <div class="col-md-10">
                <asp:TextBox ID="BirthDay" runat="server" Width="300px" 
                    meta:resourcekey="BirthDayResource2"></asp:TextBox>
                <asp:Label ID="CheckBirthDay" runat="server" Text="" 
                    ForeColor="Red"></asp:Label>
            </div>
        </div>
        <div class="row">
            <div class="col-md-2"><asp:Label ID="Label6" runat="server" Text="＊電子信箱：" 
                    meta:resourcekey="Label6Resource2"></asp:Label></div>
            <div class="col-md-10">
                <asp:TextBox ID="Email" runat="server" Width="300px" 
                    meta:resourcekey="EmailResource2"></asp:TextBox>
                <asp:Label ID="CheckEmail" runat="server" Text="" ForeColor="Red" 
                    ></asp:Label>
            </div>
        </div>
        <div class="row">
            <div class="col-md-2">
                <asp:Label ID="Label7" runat="server" Text="＊聯絡電話：" 
                    meta:resourcekey="Label7Resource2"></asp:Label></div>
            <div class="col-md-10">
                <asp:TextBox ID="Tel" runat="server" Width="300px" 
                    meta:resourcekey="TelResource2"></asp:TextBox>
                <asp:Label ID="CheckTel" runat="server" Text="" 
                    ForeColor="Red"></asp:Label>
            </div>
        </div>
        <div class="row">
            <div class="col-md-2">
                <asp:Label ID="Label8" runat="server" Text="＊行動電話：" 
                    meta:resourcekey="Label8Resource2"></asp:Label></div>
            <div class="col-md-10">
                <asp:TextBox ID="CellPhone" runat="server" Width="300px" 
                    meta:resourcekey="CellPhoneResource2"></asp:TextBox>
                <asp:Label ID="CheckCellPhone" runat="server" Text="" 
                    ForeColor="Red"></asp:Label>
            </div>
        </div>
        <div class="row">
            <div class="col-md-2">
                <asp:Label ID="Label9" runat="server" Text="＊地址：" 
                    meta:resourcekey="Label9Resource2"></asp:Label></div>
            <div class="col-md-10">
                <asp:TextBox ID="Address" runat="server" Width="300px" 
                    meta:resourcekey="AddressResource2"></asp:TextBox>
                <asp:Label ID="CheckAddress" runat="server" Text="" ForeColor="Red" 
                 ></asp:Label>
            </div>
        </div>
        <div class="row" style="margin-bottom:20px;">
            <div class="col-md-2">
                <asp:Label ID="Label10" runat="server" Text="權限：" 
                    meta:resourcekey="Label10Resource2"></asp:Label></div>
            <div class="col-md-10">
                <asp:Label ID="VIP" runat="server" meta:resourcekey="VIPResource2"></asp:Label>
            </div>
        </div>
        <div class="row" style="border-top:1px dotted #d4d4d4;border-bottom:1px dotted #d4d4d4; background-color:#fef4ec; padding:10px 0px;">
            <div class="col-md-12">
                <div class="row">
                    <div class="col-md-2">
                        <asp:CheckBox ID="CheckBox1" runat="server" Text="修改密碼" 
                            meta:resourcekey="CheckBox1Resource2" />
                    </div>
                    <div class="col-md-10">
                        <asp:Label ID="Label1" runat="server" Text="" ForeColor="Red" ></asp:Label>
                    </div>
                </div>       
                <div class="row">
                    <div class="col-md-2">
                        <asp:Label ID="Label11" runat="server" Text="新密碼：" 
                            meta:resourcekey="Label11Resource2"></asp:Label></div>
                    <div class="col-md-10">
                        <asp:TextBox ID="NewPwd" runat="server" TextMode="Password" Width="300px" 
                            meta:resourcekey="NewPwdResource2"></asp:TextBox>
                        <asp:HiddenField ID="UserID" runat="server" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-2">
                        <asp:Label ID="Label12" runat="server" Text="確認新密碼：" 
                            meta:resourcekey="Label12Resource2"></asp:Label></div>
                    <div class="col-md-10">
                        <asp:TextBox ID="ChkNewPwd" runat="server" 
                            TextMode="Password" Width="300px" meta:resourcekey="ChkNewPwdResource2"></asp:TextBox></div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-md-12" style="text-align:center; margin-top:20px;">
                <asp:LinkButton ID="LinkButton1" runat="server" 
                    CssClass="btn btn-danger dropdown-toggle" onclick="LinkButton1_Click" 
                    meta:resourcekey="LinkButton1Resource2"><span class="glyphicon glyphicon-ok-sign" style="font-size:large; margin-right:7px;"></span>確認儲存</asp:LinkButton>
                <asp:LinkButton ID="LinkButton2" runat="server" 
                    CssClass="btn btn-warning dropdown-toggle" onclick="LinkButton2_Click" 
                    meta:resourcekey="LinkButton2Resource2"><span class="glyphicon glyphicon-home" style="font-size:large; margin-right:7px;"></span>回網站首頁</asp:LinkButton>
                <asp:LinkButton ID="LinkButton3" runat="server" 
                    CssClass="btn btn-primary dropdown-toggle" onclick="LinkButton3_Click" 
                    meta:resourcekey="LinkButton3Resource2"><span class="glyphicon glyphicon-remove" style="font-size:large; margin-right:7px;"></span>登出</asp:LinkButton>                
            </div>
        </div>
        <div style="margin-top:40px;"></div>
        <footer class="bs-footer">
            <div class="row" id="foot">
                <div class="col-md-12 fontcenter" style="padding-top:15px;">                
                    <asp:Label ID="Label13" runat="server" 
                        Text="建議您使用IE9.0 以上的瀏覽器| 螢幕解析度請設為 1280*1024 以上" 
                        meta:resourcekey="Label13Resource2"></asp:Label>
                </div>
            </div>
        </footer>
    </div>
    </form>
</body>
</html>
