using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.Security.Application;
using System.IO;
using static ECSSO.Library.Logistics;

namespace ECSSO
{
    public partial class OrderList : System.Web.UI.Page
    {
        private string str_language = string.Empty;
        private string disablePrice;
        private int storeType;
        #region  語系變換
        protected override void InitializeCulture()
        {
            //此currentculture來自default.aspx頁面上兩個超連結的連結位址,見html部分            
            if (Request.QueryString["language"] != null)
            {
                str_language = Request.QueryString["language"].ToString();
            }
            else
            {
                if (Request.Form["language"] != null)
                {
                    str_language = Request.Form["language"].ToString();
                }
            }
            if (str_language == "")
            {
                str_language = "zh-tw";
            }

            if (!String.IsNullOrEmpty(str_language))
            {
                //Nation - 決定了採用哪一種當地語系化資源，也就是使用哪種語言
                //Culture - 決定各種資料類型是如何組織，如數位與日期
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(str_language);
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(str_language);
            }
        }
        #endregion
        protected void Page_Init(object sender, EventArgs e)
        {
            HtmlGenericControl objLink = new HtmlGenericControl("link");
            objLink.Attributes.Add("rel", "stylesheet");
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/orderlist.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        #region 讀取客製化CSS
        private void getCustCss()
        {
            GetStr GS = new GetStr();
            string setting = GS.GetSetting(this.siteid.Value);
            HtmlGenericControl objStyle = new HtmlGenericControl("style");
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select end_css,disablePrice,storeType from CurrentUseHead", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        StringWriter myWriter = new StringWriter();
                        HttpUtility.HtmlDecode(reader["end_css"].ToString(), myWriter);
                        objStyle.InnerHtml = HttpUtility.HtmlDecode(myWriter.ToString());
                        disablePrice = reader["disablePrice"].ToString();
                        storeType = int.Parse(reader["storeType"].ToString());
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            this.Page.Header.Controls.Add(objStyle);
        }
        #endregion
        protected void Page_Load(object sender, EventArgs e)
        {
            this.language.Value = str_language;
            if (!IsPostBack)
            {
                #region 檢查必要參數
                String setting = "";

                if (Request.Form["SiteID"] != null)
                {
                    this.siteid.Value = Request.Form["SiteID"].ToString();
                }
                else
                {
                    if (Request.QueryString["SiteID"] != null)
                    {
                        this.siteid.Value = Request.QueryString["SiteID"].ToString();
                    }
                }

                if (Request.Form["ReturnUrl"] != null)
                {
                    this.returnurl.Value = Server.UrlDecode(Request.Form["ReturnUrl"].ToString());
                }
                else if (Request.QueryString["ReturnUrl"] != null)
                {
                    this.returnurl.Value = Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString());
                }
                else {
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                    {
                        conn.Open();
                        SqlCommand cmd;
                        cmd = new SqlCommand("select web_url from cocker_cust where id=@id", conn);
                        cmd.Parameters.Add(new SqlParameter("@id", this.siteid.Value));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    this.returnurl.Value = "https://" + reader["web_url"].ToString() + "?" + HttpContext.Current.Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString()).Split('?')[1];
                                }
                            }
                        }
                        catch
                        {

                        }
                        finally { reader.Close(); }
                    }
                }

                String MemID = "";
                if (Request.Form["MemID"] != null)
                {
                    MemID = Request.Form["MemID"].ToString();
                }
                else
                {
                    if (Request.QueryString["MemID"] != null)
                    {
                        MemID = Request.QueryString["MemID"].ToString();
                    }
                }
                String CheckM = "";
                if (Request.Form["CheckM"] != null)
                {
                    CheckM = Encoder.HtmlEncode(Request.Form["CheckM"].ToString());
                }
                else
                {
                    if (Request.QueryString["CheckM"] != null)
                    {
                        CheckM = Encoder.HtmlEncode(Request.QueryString["CheckM"].ToString());
                    }
                }
                #endregion

                getCustCss();
                GetStr GS = new GetStr();
                if (GS.MD5Check(this.siteid.Value + MemID, CheckM))
                {
                    Orders ODs = new Orders();
                    setting = GS.GetSetting(this.siteid.Value);

                    Orders.RootObject rlib = JsonConvert.DeserializeObject<Orders.RootObject>(ODs.GetOrderListJson(MemID, this.siteid.Value));
                    String TableDiv = "";

                    int count = 0;
                    int colspace = (disablePrice == "S"? 5 : 6);
                    foreach (Orders.OrdersHead ordershd in rlib.OrderHeads)
                    {
                        if (ordershd.orderType == "S") ordershd.ServicePrice = 0;
                        Int64 OrderTotalAmt = Convert.ToInt64(ordershd.OrderAmt) - Convert.ToInt64(ordershd.BonusDiscount) - Convert.ToInt64(ordershd.DiscountAmt) - Convert.ToInt64(ordershd.CouponDiscount) + ordershd.ServicePrice;
                        if (OrderTotalAmt < 0) OrderTotalAmt = Convert.ToInt64(ordershd.Freightamount);
                        else OrderTotalAmt += Convert.ToInt64(ordershd.Freightamount);
                        count += 1;
                        TableDiv += "<div class='panel panel-default'>";
                        TableDiv += "   <div class='panel-heading'>";
                        TableDiv += "       <a data-toggle='collapse' data-parent='#OrdersList' href='#collapse" + count + "' aria-expanded='true' aria-controls='collapse" + count + "'>";
                        TableDiv += "           <div class='row'>";
                        TableDiv += "               <div class='col-md-1'>" + count + "</div>";
                        TableDiv += "               <div class='col-md-2'>" + ordershd.OrderID + "</div>";                                                
                        TableDiv += "               <div class='col-md-2'>" + ordershd.OrderTime + "</div>";
                        TableDiv += "               <div class='col-md-2'>" + ordershd.PaymentType + "</div>";
                        TableDiv += "               <div class='col-md-2' style='color:#f00; font-weight: bold;'>" + 
                                    (
                                        ordershd.orderType=="S" && OrderTotalAmt==0?
                                        "詢價中": (
                                            ordershd.orderType == "S"?
                                            OrderTotalAmt.ToString()+ "<br />詢價回覆" : OrderTotalAmt.ToString()
                                        )
                                    ) +  "</div>";
                        TableDiv += "               <div class='col-md-1' style='padding:0px;'>" + GetLocalResourceObject(ordershd.OrderState) + "</div>";
                        TableDiv += "               <div class='col-md-1'><span class='glyphicon glyphicon-chevron-down'></span></div>";

                        
                        
                        
                        if (ordershd.NewQA)
                        {
                            TableDiv += "   <div class='col-md-1 QA'>";
                        }
                        else 
                        {
                            TableDiv += "   <div class='col-md-1 QA2'>";
                        }

                        #region 重新付款按鈕
                        if (ordershd.CanResetPay)
                        if(ordershd.OrderState == "付款失敗" && ordershd.DateDiff < 4320)
                        {
                            TableDiv += "   <input type='button' value='重新付款' onclick='javascript:window.open(\"OrderResetPay.ashx?OrderNo=" + ordershd.OrderID + "&SiteID=" + this.siteid.Value + "&CheckM=" + GS.MD5Endode(this.siteid.Value + GS.GetOrgName(GS.GetSetting(this.siteid.Value)) + ordershd.OrderID) + "\",\"_parent\");return false;' id='repaybtn'><br>";
                        }
                        #endregion

                        #region 出貨查詢按鈕
                        if (ordershd.Logistics)
                        {
                            TableDiv += "   <input type='button' value='出貨查詢' onclick='javascript:window.open(\"OrderLogistics.aspx?OrderNo=" + ordershd.OrderID + "&SiteID=" + this.siteid.Value + "&CheckM=" + GS.MD5Endode(this.siteid.Value + GS.GetOrgName(GS.GetSetting(this.siteid.Value)) + ordershd.OrderID) + "\",\"Logistics\",\"height=500,width=500\");return false;' id='Logisticsbtn'><br>";
                        }
                        #endregion
                        
                        //if (ordershd.OrderState != "已取消" && ordershd.OrderState != "付款失敗")
                        //{
                            TableDiv += "   <input type='button' value='問與答' onclick='javascript:window.open(\"orderqa.aspx?OrderNo=" + ordershd.OrderID + "&SiteID=" + this.siteid.Value + "&CheckM=" + GS.MD5Endode(this.siteid.Value + GS.GetOrgName(GS.GetSetting(this.siteid.Value)) + ordershd.OrderID) + "\",\"orderQA\",\"height=500,width=500\");return false;' id='qabtn'>";
                        //}

                        TableDiv += "   </div>";
                        

                        TableDiv += "           </div>";
                        TableDiv += "       </a>";
                        TableDiv += "   </div>";
                        #region 表身資料
                        TableDiv += "<div id='collapse" + count + "' class='panel-collapse collapse' role='tabpanel'>";
                        TableDiv += "   <div class='panel-body'>";
                        TableDiv += "       <table class='table table-striped table-bordered orderDetailHistory'>";
                        TableDiv += "          <thead>";
                        TableDiv += "              <tr>";
                        TableDiv += "                 <th width='5%' align='center'>編號</th>";
                        TableDiv += "                 <th width='45%' align='center'>" + GetLocalResourceObject("StringResource1") + "(" + GS.GetSpecTitle(setting, "1") + "/" + GS.GetSpecTitle(setting, "2") + ")</th>";
                        if(ordershd.orderType != "S")
                            TableDiv += "                 <th width='10%' align='center'>" + GetLocalResourceObject("StringResource2") + "</th>";
                        TableDiv += "                 <th width='10%' align='center'>" + GetLocalResourceObject("StringResource3") + "</th>";
                        if (ordershd.orderType != "S")
                            TableDiv += "                 <th width='10%' align='center'>" + GetLocalResourceObject("StringResource4") + "</th>";
                        if (disablePrice != "S")
                            TableDiv += "                 <th width='20%' align='center'>" + GetLocalResourceObject("StringResource5") + "</th>";
                        TableDiv += "            </tr>";
                        TableDiv += "         </thead>";
                        TableDiv += "     <tbody>";
                        int count1 = 0;
                        long listPrice = 0;
                        String Spec = "";
                        string subTitle = "";
                        foreach (Orders.OrdersDetail ordersdetail in ordershd.OrdersDetail)
                        {
                            count1 += 1;
                            if (ordersdetail.Color != "" || ordersdetail.Size != "")
                            {
                                Spec = "(" + ordersdetail.Color + "/" + ordersdetail.Size + ")";
                            }
                            String Memo = "";
                            //if (ordersdetail.Memo != "") Memo += ordersdetail.Memo + "<br>";

                            if (ordersdetail.Virtual == "Y")
                            {
                                if (ordersdetail.EndTime != "" && storeType !=2) Memo += "<br>" + GetLocalResourceObject("StringResource6") + ordersdetail.StartTime + " ~ " + ordersdetail.EndTime + "<br>";
                                //驗證碼暫時不開放～
                                //if (ordersdetail.Vcode != "") Memo += GetLocalResourceObject("驗證碼：") + ordersdetail.Vcode;                        
                            }
                            if (disablePrice == "S" && ordershd.orderType != "S") {
                                if (ordersdetail.Memo != subTitle)
                                {
                                    if (listPrice != 0)
                                    {
                                        TableDiv += "       <tr>";
                                        TableDiv += "           <td align='right' colspan='" + (colspace - 1) + "'>" + subTitle + "【合計】</td>";
                                        TableDiv += "           <td align='center red'>" + listPrice + "</td>";
                                        TableDiv += "       </tr>";
                                        listPrice = 0;
                                    }
                                    subTitle = ordersdetail.Memo;
                                    TableDiv += "       <tr>";
                                    TableDiv += "<td colspan='" + colspace + "'>" + subTitle + "</td>";
                                    TableDiv += "       </tr>";
                                }
                            }
                            TableDiv += "       <tr>";
                            TableDiv += "           <td align='center'>" + count1 + "</td>";
                            TableDiv += "           <td align='center'>" + ordersdetail.ProductName + Spec + Memo;
                            if (ordersdetail.Discription != "") 
                            {
                                TableDiv += "<br />" + ordersdetail.Discription;
                            }
                            TableDiv += "</td>";
                            if (ordershd.orderType != "S")
                                TableDiv += "           <td align='center'>" + ordersdetail.Price + "</td>";
                            TableDiv += "           <td align='center'>" + ordersdetail.Qty + "</td>";
                            long reg = (Convert.ToInt64(ordersdetail.Amt) - Convert.ToInt64(ordersdetail.Discount));
                            listPrice += reg;
                            if (ordershd.orderType != "S")
                            {
                                TableDiv += "           <td align='center'>" + reg;
                                TableDiv += "           <br>(" + GetLocalResourceObject("StringResource10") + ordersdetail.Bonus + ")";
                                TableDiv += "           </td>";
                            }
                            if(disablePrice != "S")
                                TableDiv += "           <td align='center'>" + ordersdetail.Memo + "</td>";
                            TableDiv += "       </tr>";

                            #region 點燈資料
                            foreach (Orders.OrdersDetailData ODD in ordersdetail.OrdersDetailData)
                            {
                                TableDiv += "       <tr class='pray_tr1'>";
                                TableDiv += "           <td></td>";
                                TableDiv += "           <td align='center'>姓名:" + ODD.Name + "</td>";
                                TableDiv += "           <td align='center' colspan='" + (colspace - 2) + "'>燈座號碼:" + ODD.LightNo + "</td>";
                                TableDiv += "       </tr>";
                                TableDiv += "       <tr class='pray_tr3'>";
                                TableDiv += "           <td></td>";
                                TableDiv += "           <td align='center'>生日:" + ODD.Birth + " / " + ODD.Hour + "時</td>";
                                TableDiv += "           <td align='center' colspan='2'>農曆:" + ODD.LBirth + "</td>";
                                TableDiv += "           <td align='center' colspan='" + (colspace - 4) + "'>生肖:" + ODD.Animal + "</td>";
                                TableDiv += "       </tr>";
                                TableDiv += "       <tr class='pray_tr2'>";
                                TableDiv += "           <td></td>";
                                TableDiv += "           <td align='center'>地址:" + ODD.Addr + "</td>";
                                TableDiv += "           <td align='center' colspan='2'>電話:" + ODD.Tel + "</td>";
                                TableDiv += "           <td align='center' colspan='" + (colspace - 4) + "'>手機:" + ODD.CellPhone + "</td>";
                                TableDiv += "       </tr>";
                            }
                            #endregion                            
                        }
                        if (disablePrice == "S" && ordershd.orderType != "S")
                        {
                            if (listPrice != 0)
                            {
                                TableDiv += "       <tr>";
                                TableDiv += "           <td align='right' colspan='" + (colspace - 1) + "'>" + subTitle + "【合計】</td>";
                                TableDiv += "           <td align='center red'>" + listPrice + "</td>";
                                TableDiv += "       </tr>";
                                listPrice = 0;
                            }
                        }
                        if (ordershd.ServicePrice > 0 && ordershd.orderType != "S") {
                            TableDiv += "       <tr class='finaltr'>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'>" + GetLocalResourceObject("ServicePrice") + "</td>";
                            TableDiv += "           <td align='center'>" + ordershd.ServicePrice + "</td>";
                            if (disablePrice != "S")
                                TableDiv += "           <td align='center'></td>";
                            TableDiv += "       </tr>";
                        }
                        if (Convert.ToInt64(ordershd.DiscountAmt) > 0 && ordershd.orderType != "S")
                        {
                            TableDiv += "       <tr class='finaltr'>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'>" + GetLocalResourceObject("StringResource9") + "</td>";
                            TableDiv += "           <td align='center'>-" + ordershd.DiscountAmt + "</td>";
                            if (disablePrice != "S")
                                TableDiv += "           <td align='center'></td>";
                            TableDiv += "       </tr>";
                        }
                        if (Convert.ToInt64(ordershd.CouponDiscount) > 0 && ordershd.orderType != "S") {
                            TableDiv += "       <tr class='finaltr'>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='right' colspan='2'>" + ordershd.CouponTitle + "</td>";
                            TableDiv += "           <td align='center' style='text-align:center'>-" + ordershd.CouponDiscount + "</td>";
                            if (disablePrice != "S")
                                TableDiv += "           <td align='center'></td>";
                            TableDiv += "       </tr>";
                        }
                        if (!string.IsNullOrEmpty(ordershd.GetCouponTitle) && ordershd.orderType != "S")
                        {
                            TableDiv += "       <tr class='finaltr'>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'>贈送優惠券:</td>";
                            TableDiv += "           <td align='center' align='" + (colspace - 4) + "'>" + ordershd.GetCouponTitle + "</td>";
                            TableDiv += "       </tr>";
                        }
                        if (Convert.ToInt64(ordershd.BonusDiscount) > 0 && ordershd.orderType != "S")
                        {
                            TableDiv += "       <tr class='finaltr'>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'>" + GetLocalResourceObject("StringResource8") + "</td>";
                            TableDiv += "           <td align='center'>-" + ordershd.BonusDiscount + "</td>";
                            if (disablePrice != "S")
                                TableDiv += "           <td align='center'></td>";
                            TableDiv += "       </tr>";
                        }
                        if (Convert.ToInt64(ordershd.Freightamount) > 0 && ordershd.orderType != "S")
                        {
                            TableDiv += "       <tr class='finaltr'>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'>" + GetLocalResourceObject("StringResource7") + "</td>";
                            TableDiv += "           <td align='center'>" + ordershd.Freightamount + "</td>";
                            if (disablePrice != "S")
                                TableDiv += "           <td align='center'></td>";
                            TableDiv += "       </tr>";
                        }
                        TableDiv += "           </tbody>";
                        TableDiv += "       </table>";
                        if (!string.IsNullOrEmpty(ordershd.NoteMemo)|| ordershd.CustMemo.Count>0)
                        {
                            TableDiv += $@"<table class='table table-striped table-bordered orderNoteMemo'>
                                <thead>
                                    <tr>
                                        <td colspan='2'>{GetLocalResourceObject("StringResource5")}</td>
                                    </tr>
                                </thead>
                                <tbody>";
                            ordershd.CustMemo.ForEach(item => {
                                TableDiv += $@"
                                    <tr>
                                        <td style='width:30%'>{item.title}</td>
                                        <td>{item.value}</td>
                                    </tr>";
                            });
                            if (!string.IsNullOrEmpty(ordershd.NoteMemo)) {
                                TableDiv += $@"
                                    <tr>
                                        <td colspan='2'>{ordershd.NoteMemo}</td>
                                    </tr>";
                            }
                            TableDiv += $@"    
                                </tbody>
                            </table>";
                        }
                        TableDiv += "   </div>";
                        if (ordershd.orderType == "S" && OrderTotalAmt!=0) {
                            RootLogistic logistic = new RootLogistic(setting);
                            TableDiv += @"<div class='panel-body'>
                                <div class='row'><h3>選擇付款方式</h3></div>
                            ";
                            logistic.Logistics.ForEach(l => {
                                TableDiv += $@"
                                        <div class='row'>
                                            <div class='col-xs-12'>
                                                <input id='LogisticsType{l.ID}_{l.LogisticstypeID}' type='radio' name='LogisticsType' value='{l.LogisticstypeID}' {(logistic.Logistics.Count > 1 ? "" : "class='hidden'")} />
                                                <label for='LogisticsType{l.ID}_{l.LogisticstypeID}' {(logistic.Logistics.Count > 1?"": "class='hidden'")}>{l.Title}</label>
                                                <ul>";
                                l.PaymentType.ForEach(p => {
                                    if (Convert.ToInt64(ordershd.OrderAmt) < int.Parse(p.AmountLimit))
                                    {
                                        TableDiv += $@"
                                                <li>
                                                    <input id='PaymentType{l.ID}_{p.id}' type='radio' name='PaymentType' value='{p.value}' />
                                                    <label for='PaymentType{l.ID}_{p.id}'>{p.title}</label>
                                                </li>
                                        ";
                                    }
                                });
                                TableDiv += $@"</ul>
                                            </div>
                                        </div>";
                            });
                            TableDiv += $@"
                                <div class='row'>
                                    <div class='col-md-12 pull-center'>
                                        <button type='button' class='btn btn-danger checkPay'
                                            data-OrderNo='{ordershd.OrderID}'
                                            data-SiteID='{this.siteid.Value}'
                                            data-CheckM='{GS.MD5Endode(this.siteid.Value + GS.GetOrgName(GS.GetSetting(this.siteid.Value)) + ordershd.OrderID)}'>
                                            <span class='glyphicon glyphicon-saved'></span>確認付款方式
                                        </button>
                                    </div>
                                </div>
                            </div>";
                        }

                        TableDiv += "</div>";
                        #endregion

                        TableDiv += "</div>";
                    }
                    OrdersList.InnerHtml = HttpUtility.HtmlDecode(Server.HtmlDecode(Encoder.HtmlEncode(TableDiv)));
                }
            }
        }

        protected void LinkButton2_Click(object sender, EventArgs e)
        {
            Response.Write("<script language='javascript'>top.location.href='" + this.returnurl.Value + "';</script>");
            //Response.Redirect(this.returnurl.Value);
        }

        protected void LinkButton3_Click(object sender, EventArgs e)
        {
            Session.Clear();
            GetStr GS = new GetStr();

            String StrUrl = this.returnurl.Value;
            string[] strs = StrUrl.Split(new string[] { "/" + GS.GetLanString(str_language) + "/" }, StringSplitOptions.RemoveEmptyEntries);
            Response.Write("<script language='javascript'>top.location.href='" + strs[0] + "/" + GS.GetLanString(str_language) + "/logout.asp';</script>");
            //Response.Redirect(strs[0] + "/tw/logout.asp");
        }
    }
}