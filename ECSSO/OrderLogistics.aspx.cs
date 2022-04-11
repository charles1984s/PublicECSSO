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
using Microsoft.Security.Application;

namespace ECSSO
{
    public partial class OrderLogistics : System.Web.UI.Page
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/OrderLogistics.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            this.language.Value = str_language;
            if (!IsPostBack)
            {
                #region 檢查必要參數
                if (Request.Params["OrderNo"] != null)
                {
                    this.OrderNo.Value = Request.Params["OrderNo"].ToString();
                }

                if (Request.Params["SiteID"] != null)
                {
                    this.siteid.Value = Request.Params["SiteID"].ToString();
                }
                String CheckM = "";
                if (Request.Params["CheckM"] != null)
                {
                    CheckM = Encoder.HtmlEncode(Request.Params["CheckM"].ToString());
                }
                #endregion
                GetStr GS = new GetStr();
                if (GS.MD5Check(this.siteid.Value + GS.GetOrgName(GS.GetSetting(this.siteid.Value)) + this.OrderNo.Value, CheckM))
                {
                    String Setting = GS.GetSetting(this.siteid.Value);

                    
                    OrderQA.InnerHtml = GetFile(Setting);
                }
            }   
        }
        private String GetFile(String Setting)
        {
            String ReturnStr = "";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select a.*,b.title  as LogisticstypeName from orders_Logistics as a left join Logisticstype as b on a.LogisticstypeID=b.id where a.order_no=@order_no order by a.ser_no desc", conn);
                cmd.Parameters.Add(new SqlParameter("@order_no", this.OrderNo.Value));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ReturnStr += "<div class='row'>";
                            ReturnStr += "  <div class='col-md-3'>" + reader["UpdateStatusDate"].ToString() + "</div>";
                            ReturnStr += "  <div class='col-md-3'>取貨方式：" + reader["LogisticstypeName"].ToString();
                            switch (reader["LogisticsSubType"].ToString()) 
                            {
                                case "TCAT":
                                    ReturnStr += "(黑貓)";
                                    break;
                                case "ECAN":
                                    ReturnStr += "(宅配通)";
                                    break;
                            }

                            if (reader["ReceiverStoreName"].ToString() != "") 
                            {
                                ReturnStr += "(" + reader["ReceiverStoreName"].ToString() + " - " + reader["ReceiverStoreAddr"].ToString() + ")";
                            }
                            ReturnStr += "</div>";
                            if (reader["RtnMsg"].ToString() != "") 
                            {
                                ReturnStr += "<div class='col-md-3'>" + reader["RtnMsg"].ToString() + "</div>";
                            }
                            ReturnStr += "  <div class='col-md-3'>";
                            if (reader["CVSPaymentNo"].ToString() != "")
                            {
                                ReturnStr += "寄貨編號:" + reader["CVSPaymentNo"].ToString();
                            }
                            if (reader["CVSValidationNo"].ToString() != "")
                            {
                                ReturnStr += "驗證碼:" + reader["CVSValidationNo"].ToString();
                            }
                            if (reader["BookingNote"].ToString() != "")
                            {
                                ReturnStr += "托運單號:" + reader["BookingNote"].ToString();
                            }
                            ReturnStr += "  </div>";
                            ReturnStr += "</div>";

                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2 = new SqlCommand("select a.UpdateStatusDate,a.RtnMsg,a.RtnCode from dbo.orders_Logistics_Log as a left join orders_Logistics as b on a.AllPayLogisticsID=b.AllPayLogisticsID where a.ser_no=@ser_no and a.AllPayLogisticsID=@AllPayLogisticsID order by UpdateStatusDate desc", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@ser_no", reader["ser_no"].ToString()));
                                cmd2.Parameters.Add(new SqlParameter("@AllPayLogisticsID", reader["AllPayLogisticsID"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            ReturnStr += "<div class='row'>";
                                            ReturnStr += "  <div class='col-md-12'>" + reader["UpdateStatusDate"].ToString() + " (" + reader["RtnCode"].ToString() + ") " + reader["RtnMsg"].ToString() + "</div>";
                                            ReturnStr += "</div>";
                                        }
                                    }
                                    else 
                                    {
                                        if (reader["CVSPaymentNo"].ToString() == "" && reader["CVSValidationNo"].ToString() == "" && reader["BookingNote"].ToString() == "") 
                                        {
                                            using (SqlConnection conn3 = new SqlConnection(Setting))
                                            {
                                                conn3.Open();
                                                SqlCommand cmd3 = new SqlCommand("select * from orders_hd where id=@id", conn3);
                                                cmd3.Parameters.Add(new SqlParameter("@id", this.OrderNo.Value));
                                                SqlDataReader reader3 = cmd3.ExecuteReader();
                                                if (reader3.HasRows)
                                                {
                                                    while (reader3.Read())
                                                    {
                                                        if (reader3["state"].ToString() == "3")
                                                        {
                                                            ReturnStr += "<div class='row'>";
                                                            ReturnStr += "  <div class='col-md-12'>" + reader3["edate"] + "  已出貨</div>";
                                                            ReturnStr += "</div>";
                                                        }
                                                        else if (reader3["state"].ToString() == "7") {
                                                            ReturnStr += "<div class='row'>";
                                                            ReturnStr += "  <div class='col-md-12'>" + reader3["edate"] + "  已完成</div>";
                                                            ReturnStr += "</div>";
                                                        }
                                                        else
                                                        {
                                                            ReturnStr += "<div class='row'>";
                                                            ReturnStr += "  <div class='col-md-12'>" + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + " 尚未出貨</div>";
                                                            ReturnStr += "</div>";
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                finally { reader2.Close(); }
                            }
                        }
                    }
                    else 
                    {
                        ReturnStr = "not found";
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return ReturnStr;
        }
    }
}