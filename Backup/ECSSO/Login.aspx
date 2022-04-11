<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ECSSO.Login" culture="auto" meta:resourcekey="PageResource1" uiculture="auto" %>

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
        .col-md-3{text-align:right;}
        .col-md-9{padding:0px 0px;}
        .textLabel{margin-left:15px;}
        .btn1{margin-left:60px;}
        .bs-footer{padding:0; margin:0; bottom:0; left:0; width:100%; position:fixed;}
        #foot { background-image:url("img/infooter.jpg"); background-repeat:repeat-x; color:#898989; text-align:center; margin-top:20px;}
        @media (min-width: 979px) {
            .container{width:820px;}                  
        }
        @media (max-width: 979px) {            
            .col-md-3{text-align:left; padding:0px 0px;}      
            .textLabel{margin-left:0px;}
            .btn1{margin-left:0px;}
        }
    </style>    
    <script language="javascript" type="text/javascript">        
        count=0;
        function reload() {
            if (count % 2 == 0) {
                document.getElementById("Image1").src = "admin/ValidateCode.ashx";            
            }
            else {
                document.getElementById("Image1").src = "ValidateCode.ashx";
            }
            count++;
        }
    </script>
</head>
<body class="para">
    <form id="form1" runat="server" defaultbutton="LinkButton1">    
    <asp:HiddenField ID="siteid" runat="server" />
    <asp:HiddenField ID="language" runat="server" />
    <asp:HiddenField ID="returnurl" runat="server" />
    <asp:HiddenField ID="weburl" runat="server" />    
    <div class="container">
        <div class="row">
            <div class="col-md-12" style=" padding:15px 0px;">
                <asp:Label ID="WebTitle" class="para" runat="server" 
                    style="font-size:xx-large; font-weight:bold; color:#555555;" 
                    meta:resourcekey="WebTitleResource1"></asp:Label>
            </div>
        </div>
        <div class="row">
            <div class="col-md-12" style="background-color:#f5f5f5; padding:10px 30px;">
                <span class="glyphicon glyphicon-user" style="font-size:large; margin-right:7px;"></span>
                <asp:Label ID="Label5" runat="server" Text="會員登入" 
                    meta:resourcekey="Label5Resource1"></asp:Label>
            </div>
        </div>
        <div class="row" style="border:1px solid #d4d4d4;">
            <div class="col-md-7">     
                <div class="row" style="padding:25px 60px;">
                    <div  class="col-md-12"> 
                        <div class="row">
                            <div class="col-md-3">
                                <asp:Label ID="Label2" runat="server" Text="帳號：" Width="45px" 
                                    CssClass="textLabel" meta:resourcekey="Label2Resource1"></asp:Label>
                            </div>
                            <div class="col-md-9">
                                <asp:TextBox ID="UserID" runat="server" Width="270px" 
                                    style="margin-bottom:5px;" placeholder="請輸入Email" 
                                    meta:resourcekey="UserIDResource1"></asp:TextBox>
                                <asp:Label ID="CheckUserID" runat="server" Text="請輸入帳號" ForeColor="Red" 
                                    Visible="False" meta:resourcekey="CheckUserIDResource1"></asp:Label>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-3">
                                <asp:Label ID="Label3" runat="server" Text="密碼：" Width="45px" 
                                    CssClass="textLabel" meta:resourcekey="Label3Resource1"></asp:Label>
                            </div>
                            <div class="col-md-9">
                                <asp:TextBox ID="UserPwd" runat="server" Width="270px" TextMode="Password" 
                                    style="margin-bottom:5px;" meta:resourcekey="UserPwdResource1"></asp:TextBox>
                                <asp:Label ID="CheckUserPwd" runat="server" Text="請輸入密碼" ForeColor="Red" 
                                    Visible="False" meta:resourcekey="CheckUserPwdResource1"></asp:Label>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-3">
                                <asp:Label ID="Label4" runat="server" Text="驗證碼：" Width="60px" 
                                    meta:resourcekey="Label4Resource1"></asp:Label>
                            </div>
                            <div class="col-md-9">
                                <asp:TextBox ID="TextBox1" runat="server" Width="155px" 
                                    style="margin-bottom:20px;" meta:resourcekey="TextBox1Resource1"></asp:TextBox>
                                <asp:Image ID="Image1" name="Image1" runat="server" ImageUrl="ValidateCode.ashx" Width="57px" 
                                    align="absmiddle" style="margin-left:5px; margin-bottom:6px;" />                                
                                <a href="javascript: reload();" class="btn"><span class="glyphicon glyphicon-repeat"></span></a>
                                <asp:Label ID="Label1" runat="server" ForeColor="Red" Visible="False" 
                                    meta:resourcekey="Label1Resource1"></asp:Label>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <asp:LinkButton ID="LinkButton1" runat="server" 
                                    CssClass="btn btn-danger dropdown-toggle btn1" onclick="LinkButton1_Click" 
                                    style="margin-right:30px; width:107px;" meta:resourcekey="LinkButton1Resource1">
                                    <span class='glyphicon glyphicon-ok-circle' style='font-size:large; margin-right:7px;'></span>
                                    登　入</asp:LinkButton>
                                <asp:LinkButton ID="LinkButton2" runat="server" 
                                    CssClass="btn btn-primary dropdown-toggle" onclick="LinkButton2_Click" 
                                    meta:resourcekey="LinkButton2Resource1">
                                    <span class='glyphicon glyphicon-log-in' style='font-size:large; margin-right:7px;'></span>
                                    忘記密碼</asp:LinkButton>
                            </div>                            
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-md-5" style="border-left:1px dotted #d4d4d4;">
                <div class="row" style="padding:25px 40px; height:200px;">
                    <asp:LinkButton ID="LinkButton3" runat="server" 
                        CssClass="btn btn-danger dropdown-toggle" onclick="LinkButton3_Click" 
                        style="margin-bottom:10px;" meta:resourcekey="LinkButton3Resource1"><span class="glyphicon glyphicon-user" style="font-size:large; margin-right:13px; margin-left:8px;"></span>
                        加入本站會員
                    </asp:LinkButton>   
                    
                    <!-------------安慶start---------------->       
                    <!--
                    <br />
                    <span style="font-size:large; color:#000000; font-weight:bold;">不需註冊即時登入</span>
                    <br />
                    使用您已有的FB或HINET帳號馬上登入
                    <br />
                    <asp:LinkButton ID="LinkButton6" runat="server" OnClick="LinkButton6_Click"><img src="img/hinet_login.png" style="margin-top:10px;margin-bottom:10px;" /></asp:LinkButton>
                    <br />-->
                    <asp:LinkButton ID="LinkButton4" runat="server" OnClick="LinkButton4_Click"><img src="img/facebook_login.png" style="margin-top:10px;margin-bottom:10px;" /></asp:LinkButton>
                    <br /> 
                    <asp:LinkButton ID="LinkButton5" runat="server" OnClick="LinkButton5_Click"><img src="img/Google_login.png" style="margin-top:10px;margin-bottom:10px;" /></asp:LinkButton>
                   
                    <!-------------安慶end-------------->
                          
                    <!--<br />
                    <span style="font-size:large; color:#000000; font-weight:bold;">不需註冊即時登入</span>
                    <br />
                    使用您已有的FB或HINET帳號馬上登入
                    <br />
                    <img src="img/hinet_login.png" style="margin-top:10px;margin-bottom:10px;" />
                    <br />
                    <img src="img/facebook_login.png" />-->
                </div>
            </div>
        </div>
        <div style="margin-top:40px;"></div>
        <footer class="bs-footer">
            <div class="row" id="foot">
                <div class="col-md-12 fontcenter" style="padding-top:15px;">                
                    <asp:Label ID="Label6" runat="server" 
                        Text="建議您使用IE9.0 以上的瀏覽器| 螢幕解析度請設為 1280*1024 以上" 
                        meta:resourcekey="Label6Resource1"></asp:Label>
                </div>
            </div>
        </footer>
        
    </div>
    <script type="text/javascript" src="Scripts/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="Scripts/bootstrap.min.js"></script>
    <script type='text/javascript' src="js/jquery.s2t.js"></script>
    <script type="text/javascript">
        
        $(document).ready(function () {
            if ($("input[name=language]").val() == "zh-cn") {
                myInit();
                
            }
            
        });
        function myInit() {
            
                var text = $('#text-result').text();
                $('#text-result').text($.t2s(text));
                $('#content-wap').t2s();
                $('.para').t2s();
            
        }
    </script>
    </form>
</body>
</html>
