<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ECSSO._Default" %>

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
        .shoppingdetail{border:1px #D4D4D4 dotted;border-bottom:none;}
        .shoppinggraybg{background-color:#f5f5f5;}
        .shoppingwhitebg{background-color:#ffffff;padding-left:3px;}
        .shoppingred{color:#d60000; font-weight:bold;}
        .shoppingline{border-top:solid 1px #d4d4d4;}
        .fontright{text-align:right;}
        .fontcenter{text-align:center;}
        #foot { background-image:url("img/infooter.jpg"); background-repeat:no-repeat; color:#898989;}
        .orderitem {text-align:right;}
        .orderitem2 {background-color:#f5f5f5; margin-left:-5px; margin-right:-5px;}
        .orderitem3 {border-left:1px dotted #d4d4d4;}
        .orderitem3 row {}
        .orderitem4 {border-bottom:1px dotted #d4d4d4;}
        .table-bordered>tbody>tr>td {border:1px dotted #d4d4d4;}
        .table-bordered>thead>tr>th {border:1px dotted #d4d4d4; background-color:#f5f5f5; text-align:center;}
        .table{margin-bottom:0px;}
        @media (max-width: 979px) {
            .orderitem{text-align:left; padding-left:0px;}
            .orderitem2 {background-color:#ffffff; margin-left:0px; margin-right:0px;}
            .orderitem3 {border-left:none;}            
            .orderitem4 {border-bottom:none;}
        }
    </style>
    <script type="text/javascript" src="address.js"></script>    
</head>
<body>
    <form id="form1" runat="server">
        <asp:HiddenField ID="jsonStr" runat="server" />           
        <div class="container">          
            <div class="row-fluid">
                <div class="col-md-12" id="shoppingcar" runat="server" style="margin-bottom:3px;">                
                   
                </div>
            </div>           
            <div class="row orderitem4" style="margin-left:15px;">
                <div class="col-md-6">     
                    <div class="row" style="margin-left:-10px;">
                        <div class="col-md-12" style=" margin-left:5px; padding:5px 0px;">
                        <span class="glyphicon glyphicon-pencil" style="background-color:#000000; color:#ffffff; padding:2px 2px;"></span>
                        訂購資訊</div>
                    </div>
                    <div class="row orderitem2">
                        <div class="col-md-2 orderitem" style="padding-right: 0px;">
                        姓名：
                        </div>
                        <div class="col-md-10 shoppingwhitebg">
                        <asp:TextBox ID="o_name" runat="server" Width="150px" style="margin-top:3px;"></asp:TextBox>
                            <asp:DropDownList ID="o_sex" runat="server">
                                <asp:ListItem Value="1">先生</asp:ListItem>
                                <asp:ListItem Value="2">小姐</asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
                                ControlToValidate="o_name" ErrorMessage="請輸入訂購者人名"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="row orderitem2">
                        <div class="col-md-2 orderitem" style="padding-right: 0px;">
                        電話：
                        </div>
                        <div class="col-md-10 shoppingwhitebg">
                        <asp:TextBox ID="o_tel" runat="server" Width="207px" style="margin-top:3px;"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" 
                            ControlToValidate="o_tel" ErrorMessage="請輸入訂購人電話"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="row orderitem2">
                        <div class="col-md-2 orderitem" style="padding-right: 0px;">
                        手機：
                        </div>
                        <div class="col-md-10 shoppingwhitebg">
                        <asp:TextBox ID="o_cell" runat="server" Width="207px" style="margin-top:3px;"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" 
                            ControlToValidate="o_cell" ErrorMessage="請輸入訂購人手機"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="row orderitem2">
                        <div class="col-md-2 orderitem" style="padding-right: 0px;">
                        電子信箱：
                        </div>
                        <div class="col-md-10 shoppingwhitebg">
                        <asp:TextBox ID="mail" runat="server" Width="207px" style="margin-top:3px;"></asp:TextBox>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" 
                            ErrorMessage="請輸入正確email" ControlToValidate="mail" 
                            ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                        </div>
                    </div>
                </div>
                <div class="col-md-6 orderitem3">     
                    <div class="row" style="margin-left:-5px;">
                        <div class="col-md-12" style="padding:5px 0px;">
                        <span class="glyphicon glyphicon-pencil" style="background-color:#000000; color:#ffffff; padding:2px 2px;"></span>
                        收件人資訊             
                        <asp:CheckBox ID="CheckBox1" Text="同訂購人資訊" runat="server" 
                                oncheckedchanged="CheckBox1_CheckedChanged" AutoPostBack="true" style="padding-left:5px;" /></div>
                    </div>
                    <div class="row orderitem2">
                        <div class="col-md-2 orderitem" style="padding-right: 0px;">
                        姓名：
                        </div>
                        <div class="col-md-10 shoppingwhitebg">
                        <asp:TextBox ID="name" runat="server" style="margin-top:3px;" Width="155px"></asp:TextBox>
                        <asp:DropDownList ID="sex" runat="server">
                            <asp:ListItem Value="1">先生</asp:ListItem>
                            <asp:ListItem Value="2">小姐</asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" 
                            ControlToValidate="name" ErrorMessage="請輸入收件人名"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="row orderitem2">
                        <div class="col-md-2 orderitem" style="padding-right: 0px;">
                        電話：
                        </div>
                        <div class="col-md-10 shoppingwhitebg">
                        <asp:TextBox ID="tel" runat="server" style="margin-top:3px;" Width="212px"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" 
                            ControlToValidate="tel" ErrorMessage="請輸入收件人電話"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="row orderitem2">
                        <div class="col-md-2 orderitem" style="padding-right: 0px;">
                        手機：
                        </div>
                        <div class="col-md-10 shoppingwhitebg">
                        <asp:TextBox ID="cell" runat="server" style="margin-top:3px;" Width="212px"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" 
                            ControlToValidate="cell" ErrorMessage="請輸入收件人手機"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="row orderitem2">
                        <div class="col-md-2 orderitem" style="border:0px; padding-right: 0px;">
                        地址：
                        </div>
                        <div class="col-md-10 shoppingwhitebg">
                        <asp:DropDownList ID="ddlCity" runat="server" AutoPostBack="True" AppendDataBoundItems="true" 
                            DataTextField="name" DataValueField="id" DataSourceID="SqlDataSource1" style="margin-top:3px;">
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="SqlDataSource1" runat="server" 
                            ConnectionString="<%$ ConnectionStrings:sqlDB %>" 
                            ProviderName="System.Data.SqlClient" 
                            SelectCommand="SELECT [id], [name] FROM [city]"></asp:SqlDataSource>
                        &nbsp;<asp:DropDownList ID="ddlCountry" runat="server" 
                            AppendDataBoundItems="false" AutoPostBack="true" 
                            DataTextField="name" DataValueField="name" DataSourceID="SqlDataSource2">
                        </asp:DropDownList>  
                        <asp:SqlDataSource ID="SqlDataSource2" runat="server" 
                            ConnectionString="<%$ ConnectionStrings:sqlDB %>" 
                            ProviderName="System.Data.SqlClient" 
                            SelectCommand="SELECT [name] FROM [area] WHERE ([cityid] = @cityid)">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="ddlCity" Name="cityid" 
                                    PropertyName="SelectedValue" Type="Int32" />
                            </SelectParameters>
                        </asp:SqlDataSource>

                        <asp:DropDownList ID="ddlzip" runat="server"
                        AppendDataBoundItems="false" AutoPostBack="true" 
                            DataTextField="zip" DataValueField="zip" DataSourceID="SqlDataSource3">
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="SqlDataSource3" runat="server" 
                            ConnectionString="<%$ ConnectionStrings:sqlDB %>" 
                    
                            SelectCommand="SELECT [zip] FROM [area] WHERE ([cityid] = @cityid) and name=@name">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="ddlCity" Name="cityid" 
                                    PropertyName="SelectedValue" Type="Int32" />
                                <asp:ControlParameter ControlID="ddlCountry" Name="name" 
                                    PropertyName="SelectedValue" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <br />
                        <asp:TextBox ID="address" runat="server" Width="212px" style="margin-top:3px;"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" 
                            ControlToValidate="address" ErrorMessage="請輸入收件地址"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="row orderitem2" style="margin-bottom:10px;">
                        <div class="col-md-2 orderitem" style="padding-right: 0px;">
                        備註：
                        </div>
                        <div class="col-md-10 shoppingwhitebg">
                        <asp:TextBox ID="notememo" runat="server" Width="212px" style="margin-top:3px;"></asp:TextBox>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row" style="margin-top:30px; margin-bottom:30px;">
                <div class="col-md-12 fontcenter">     
                    <a href="javascript:history.go(-2);" style="color:#898989; font-size:small; padding-right:30px;">回上一頁</a>
                    <asp:LinkButton ID="LinkButton1" runat="server" onclick="LinkButton1_Click" CssClass="btn btn-danger dropdown-toggle"><span class="glyphicon glyphicon-shopping-cart"></span>結帳</asp:LinkButton>                
                </div>
            </div>
            <div class="row" id="foot">
                <div class="col-md-12 fontcenter" style="padding-top:15px;">                
                    建議您使用IE9.0 以上的瀏覽器| 螢幕解析度請設為 1280*1024 以上
                </div>
            </div>
        </div>
        
        
    <script type="text/javascript" src="Scripts/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="Scripts/bootstrap.min.js"></script>
    </form>
</body>
</html>
