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

namespace ECSSO
{
    public partial class OrderList2 : System.Web.UI.Page
    {
        private string str_language = string.Empty;
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
                                this.returnurl.Value = reader["web_url"].ToString();
                            }
                        }
                    }
                    catch
                    {

                    }
                    finally { reader.Close(); }
                }
                /*
                if (Request.Form["ReturnUrl"] != null)
                {
                    this.returnurl.Value = Server.UrlDecode(Request.Form["ReturnUrl"].ToString());
                }
                else
                {
                    if (Request.QueryString["ReturnUrl"] != null)
                    {
                        this.returnurl.Value = Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString());
                    }
                }*/

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


                GetStr GS = new GetStr();

                if (GS.MD5Check(this.siteid.Value + MemID, CheckM))
                {
                    Orders ODs = new Orders();
                    setting = GS.GetSetting(this.siteid.Value);

                    Orders.RootObject rlib = JsonConvert.DeserializeObject<Orders.RootObject>(ODs.GetOrderListJson(MemID, this.siteid.Value));
                    String TableDiv = "";

                    int count = 0;
                    foreach (Orders.OrdersHead ordershd in rlib.OrderHeads)
                    {
                        Int64 OrderTotalAmt = Convert.ToInt64(ordershd.OrderAmt) + Convert.ToInt64(ordershd.Freightamount) - Convert.ToInt64(ordershd.BonusDiscount) - Convert.ToInt64(ordershd.DiscountAmt);
                        count += 1;
                        TableDiv += "<div class='panel panel-default'>";
                        TableDiv += "   <div class='panel-heading'>";
                        TableDiv += "       <a data-toggle='collapse' data-parent='#OrdersList' href='#collapse" + count + "' aria-expanded='true' aria-controls='collapse" + count + "'>";
                        TableDiv += "           <div class='row'>";
                        TableDiv += "               <div class='col-md-1'>" + count + "</div>";
                        TableDiv += "               <div class='col-md-2'>" + ordershd.OrderID + "</div>";
                        TableDiv += "               <div class='col-md-3'>" + ordershd.OrderTime + "</div>";
                        TableDiv += "               <div class='col-md-3'>" + ordershd.OrderState + "</div>";
                        TableDiv += "               <div class='col-md-1'><span class='glyphicon glyphicon-chevron-down'></span></div>";
                        TableDiv += "           </div>";
                        TableDiv += "       </a>";
                        TableDiv += "   </div>";
                        #region 表身資料
                        TableDiv += "<div id='collapse" + count + "' class='panel-collapse collapse' role='tabpanel'>";
                        TableDiv += "   <div class='panel-body'>";
                        TableDiv += "       <table class='table table-striped table-bordered'>";
                        TableDiv += "          <thead>";
                        TableDiv += "              <tr>";
                        TableDiv += "                 <th width='5%' align='center'>&nbsp;</th>";
                        TableDiv += "                 <th width='45%' align='center'>" + GetLocalResourceObject("StringResource1") + "(" + GS.GetSpecTitle(setting, "1") + "/" + GS.GetSpecTitle(setting, "2") + ")</th>";
                        TableDiv += "                 <th width='10%' align='center'>" + GetLocalResourceObject("StringResource2") + "</th>";
                        TableDiv += "            </tr>";
                        TableDiv += "         </thead>";
                        TableDiv += "     <tbody>";
                        int count1 = 0;
                        String Spec = "";
                        foreach (Orders.OrdersDetail ordersdetail in ordershd.OrdersDetail)
                        {
                            count1 += 1;
                            if (ordersdetail.Color != "" || ordersdetail.Size != "")
                            {
                                Spec = "(" + ordersdetail.Color + "/" + ordersdetail.Size + ")";
                            }
                            String Memo = "";

                            TableDiv += "       <tr>";
                            TableDiv += "           <td align='center'>" + count1 + "</td>";
                            TableDiv += "           <td align='center'>" + ordersdetail.ProductName + Spec + Memo + "</td>";
                            TableDiv += "           <td align='center'>" + ordersdetail.Qty + "</td>";
                            TableDiv += "       </tr>";
                        }

                        TableDiv += "           </tbody>";
                        TableDiv += "       </table>";
                        TableDiv += "   </div>";
                        TableDiv += "</div>";
                        #endregion

                        TableDiv += "</div>";
                    }
                    OrdersList.InnerHtml = TableDiv;
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