﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MemberBonus.aspx.cs" Inherits="ECSSO.MemberBonus" culture="auto" meta:resourcekey="PageResource1" uiculture="auto" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <meta http-equiv="x-ua-compatible" content="IE=edge, chrome=1"/>
    <link href="Content/bootstrap.css" rel="stylesheet" />
    <!--[if lt IE 9]>
    <script src="/Scripts/html5shiv.min.js"></script>
    <script src="/Scripts/respond.min.js"></script>
    <![endif]-->
</head>
<body>
    <form id="form1" runat="server">
        <asp:HiddenField ID="siteid" runat="server" />
        <asp:HiddenField ID="language" runat="server" />
        <asp:HiddenField ID="returnurl" runat="server" />
        <asp:HiddenField ID="MemberID" runat="server" />
        <asp:HiddenField ID="CheckM" runat="server" />
        <div class="container">       
            
            
            <div class="row-fluid">
                <div class="col-md-12 listitem">
                    <div class='col-md-1'></div>
                    <div class='col-md-2'>
                        <asp:Label ID="Label1" runat="server" Text="日期" meta:resourcekey="Label1Resource1"></asp:Label></div>
                    <div class='col-md-2'>
                        <asp:Label ID="Label2" runat="server" Text="增加紅利" meta:resourcekey="Label2Resource1"></asp:Label></div>
                    <div class='col-md-2'>
                        <asp:Label ID="Label3" runat="server" Text="減少紅利" meta:resourcekey="Label3Resource1"></asp:Label></div>
                    <div class='col-md-5'>
                        <asp:Label ID="Label4" runat="server" Text="備註" meta:resourcekey="Label4Resource1"></asp:Label></div>
                </div>
                <div class="panel-group" id="BonusList" runat="server" style="margin-bottom:3px;" role="tablist" aria-multiselectable="true">                
                   
                </div>
            </div>   
            <div class="row-fluid">
                <div class="col-md-12">
                    <asp:Label ID="Label5" runat="server" Text="僅提供六個月內歷史資料" meta:resourcekey="Label5Resource1"></asp:Label>
                </div>
            </div>
            <div class="row">
                <div class="col-md-12" style="text-align:center; margin-top:20px;">
                    <asp:LinkButton ID="LinkButton2" runat="server" 
                        CssClass="btn btn-warning dropdown-toggle" onclick="LinkButton2_Click" Text="
                        &lt;span class=&quot;glyphicon glyphicon-home&quot; style=&quot;font-size:large; margin-right:7px;&quot;&gt;&lt;/span&gt;回網站首頁" meta:resourcekey="LinkButton2Resource1"></asp:LinkButton>
                    <asp:LinkButton ID="LinkButton3" runat="server" 
                        CssClass="btn btn-primary dropdown-toggle" onclick="LinkButton3_Click" Text="
                        &lt;span class=&quot;glyphicon glyphicon-remove&quot; style=&quot;font-size:large; margin-right:7px;&quot;&gt;&lt;/span&gt;登出" meta:resourcekey="LinkButton3Resource1"></asp:LinkButton>                
                </div>
            </div>  
        </div>
        <script type="text/javascript" src="/Scripts/jquery/jquery-3.5.1.min.js"></script>
        <script type="text/javascript" src="Scripts/bootstrap.min.js"></script>
        <script type='text/javascript' src="js/jquery.s2t.js"></script>
        <script type="text/javascript">
            $("#OrdersList").on('shown.bs.collapse', function () {
                if (!(parent.window == window)) {
                    var $height = parent.window.$(".autoHeight").height();
                    var i = 0;
                    var $timer = setInterval(function () {
                        parent.doIframe && parent.doIframe();
                        if (!!i) {
                            if (parent.window.$(".autoHeight").height() != $height) {
                                $height = parent.window.$(".autoHeight").height();
                            } else {
                                clearInterval($timer);
                            }
                        } else {
                            i++;
                        }
                    }, 150);
                }
            });
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
            function refreshPage() {         
                document.location.reload();
            }
        </script>
    </form>
</body>
</html>
