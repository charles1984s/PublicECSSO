using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.Net.Mail;
using Microsoft.Security.Application;
using System.Timers;

namespace ECSSO
{
    public partial class Coupon : System.Web.UI.Page
    {
        private string str_language = string.Empty;
        //語系變換
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
        protected void Page_Init(object sender, EventArgs e)
        {
            HtmlGenericControl objLink = new HtmlGenericControl("link");
            objLink.Attributes.Add("rel", "stylesheet");
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/Coupon.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            TimeOfPrice.Text = DateTime.Now.ToLongTimeString();
            this.language.Value = str_language;
            if (Request.Form["SiteID"] != null)
            {
                this.siteid.Value = Encoder.HtmlEncode(Request.Form["SiteID"].ToString());
            }
            else
            {
                if (Request.QueryString["SiteID"] != null)
                {
                    this.siteid.Value = Encoder.HtmlEncode(Request.QueryString["SiteID"].ToString());
                }
            }

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
                            this.returnurl.Value = "http://" + reader["web_url"].ToString() + "?" + this.returnurl.Value.Split('?')[1];
                        }
                    }
                }
                catch
                {

                }
                finally { reader.Close(); }
            }

            if (Request.Form["MemID"] != null)
            {
                this.MemberID.Value = Encoder.HtmlEncode(Request.Form["MemID"].ToString());
            }
            else
            {
                if (Request.QueryString["MemID"] != null)
                {
                    this.MemberID.Value = Encoder.HtmlEncode(Request.QueryString["MemID"].ToString());
                }
            }

            if (Request.Form["CheckM"] != null)
            {
                this.CheckM.Value = Encoder.HtmlEncode(Request.Form["CheckM"].ToString());
            }
            else
            {
                if (Request.QueryString["CheckM"] != null)
                {
                    this.CheckM.Value = Encoder.HtmlEncode(Request.QueryString["CheckM"].ToString());
                }
            }

            GetStr GS = new GetStr();
            if (GS.MD5Check(this.siteid.Value + this.MemberID.Value, this.CheckM.Value))
            {
                if (!IsPostBack)
                {
                    String Setting = GS.GetSetting(this.siteid.Value);
                    String MD5Str = "";
                    String DivStr = "";
                    String URL = this.returnurl.Value.Replace("http://","").Split('/')[0].ToString();
                    String usedClassName = "", usedClassName2 = "";
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(@"
                            select b.title,a.VCode,a.ExpireDate,a.Stat,a.ExchangeDay,a.GetDay,b.photo1,b.discription,b.type useType,a.GCode,a.canUseDate,c.id ordersID
                            from Cust_Coupon as a
                            left join Coupon as b on a.VCode = b.VCode
                            left join orders_hd as c on c.getCouponID = a.id
                            where a.memid = @memid order by a.stat, a.ExpireDate
                        ", conn);
                        cmd.Parameters.Add(new SqlParameter("@memid", this.MemberID.Value));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.FieldCount > 0)
                            {
                                while (reader.Read())
                                {
                                    MD5Str = GS.MD5Endode("2" + this.siteid.Value + GS.GetOrgName(Setting) + this.MemberID.Value + reader[1].ToString());
                                    usedClassName2 = "";
                                    if (reader["Stat"].ToString() == "1" && reader["ExchangeDay"].ToString() == "")
                                    {
                                        usedClassName = "";
                                        if (string.IsNullOrEmpty(reader["canUseDate"].ToString()))
                                        {
                                            usedClassName2 = " Usent";
                                        }
                                    }
                                    else 
                                    {
                                        usedClassName = "used";
                                    }
                                    DivStr += "<div class='row " + usedClassName + "'>";
                                    DivStr += "<div class='col-md-12 col-sm-12 col-xs-12" + usedClassName2 + "'>";
                                    DivStr += "<div class='title'>" + reader["title"].ToString() + "<div class='code'>" + reader["GCode"].ToString() + "</div></div>";
                                    DivStr += "<div class='UseCoupon'>";
                                    if (reader["Stat"].ToString() == "1" && reader["ExchangeDay"].ToString() == "")
                                    {
                                        if (reader["useType"].ToString() == "4")
                                        {
                                            DivStr += "<div><div>有效期限</div><div>" + reader["ExpireDate"].ToString() + "</div></div>";
                                            if (string.IsNullOrEmpty(reader["canUseDate"].ToString()))
                                            {
                                                DivStr += "<div>";
                                                DivStr += "<div>等待訂單完成</div>";
                                                DivStr += "<div>訂單編號:"+ reader["ordersID"].ToString() + "</div>";
                                                DivStr += "</div>";
                                            }
                                            else
                                            {
                                                DivStr += "<div><input type='button' class='btn btn-success' onclick='javascript:Exchange(\"" + this.siteid.Value + "\",\"" + reader[1].ToString() + "\",\"" + reader["GCode"].ToString() + "\",\"" + this.MemberID.Value + "\",\"" + MD5Str + "\");return false;' value='兌換' id='exchangebtn'></div>";
                                            }
                                        } else {
                                            DivStr += "<div><div>有效期限</div><div>" + reader["ExpireDate"].ToString() + "</div></div>";
                                            if (string.IsNullOrEmpty(reader["canUseDate"].ToString())) {
                                                DivStr += "<div>";
                                                DivStr += "<div>等待訂單完成</div>";
                                                DivStr += "<div>訂單編號:" + reader["ordersID"].ToString() + "</div>";
                                                DivStr += "</div>";
                                            }
                                            else DivStr += "<div>購物使用</div>";
                                        }
                                    }
                                    else {
                                        DivStr += "<div>" + reader["ExchangeDay"].ToString() + "已兌換</div>";
                                    }
                                    DivStr += "</div>";
                                    DivStr += "</div>";
                                    DivStr += "</div>";
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }

                    CouponList.InnerHtml = DivStr;
                }
            }
            else {
                
            }
        }
        protected void Timer1_Tick(object sender, EventArgs e)
        {            
            TimeOfPrice.Text = DateTime.Now.ToLongTimeString();
        }

    }
}