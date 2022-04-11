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
using Microsoft.Security.Application;

namespace ECSSO
{
    public partial class MemberEdit : System.Web.UI.Page
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
                if (Request.Form["language"] != null) {
                    str_language = Request.Form["language"].ToString();
                }                
            }
            if (str_language == "") {
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/memberedit.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_PreRender(object sender, EventArgs e)
        {
            Response.Write(this.test.Value);
        }
        protected void Page_Load(object sender, EventArgs e)
        {            
            this.language.Value = str_language;
            if (!IsPostBack)
            {
                String setting = "";                

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
                    this.returnurl.Value = Encoder.HtmlFormUrlEncode(Request.Form["ReturnUrl"].ToString());
                }
                else if (Request.QueryString["ReturnUrl"] != null){
                        this.returnurl.Value = Encoder.HtmlFormUrlEncode(Request.QueryString["ReturnUrl"].ToString());
                }
                else
                {
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
                                    if (reader["web_url"].ToString().IndexOf("http") >= 0) this.returnurl.Value = reader["web_url"].ToString() + "?" + this.returnurl.Value.Split('?')[1];
                                    else this.returnurl.Value = "http://" + reader["web_url"].ToString() + "?" + this.returnurl.Value.Split('?')[1];
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
                    MemID = Encoder.HtmlEncode(Request.Form["MemID"].ToString());
                }
                else
                {
                    if (Request.QueryString["MemID"] != null)
                    {
                        MemID = Encoder.HtmlEncode(Request.QueryString["MemID"].ToString());
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
                
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword,web_url from cocker_cust where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", this.siteid.Value));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (this.returnurl.Value == "")
                                {
                                    if (reader["web_url"].ToString().IndexOf("http") >= 0) this.returnurl.Value = reader["web_url"].ToString();
                                    else this.returnurl.Value = "http://" + reader["web_url"].ToString();
                                }
                                setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=" + reader["dbusername"].ToString() + "; password=" + reader["dbpassword"].ToString() + "; database=" + reader["dbname"].ToString();
                            }
                        }
                        else
                        {
                            Response.Write("<script type='text/javascript'>history.go(-1);</script>");
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                GetStr GS = new GetStr();

                if (GS.MD5Check(this.siteid.Value + MemID, CheckM))
                {
                    String ProdQA = "N";
                    String WebURL = Server.UrlDecode(this.returnurl.Value).Replace("http://", "").Replace("https://", "").Split('/')[0];
                    String CreditDisplay = "N";
                    bool storeType2 = false;
                    if (this.returnurl.Value.IndexOf("https") > -1) WebURL = "https://" + WebURL;
                    else WebURL = "http://" + WebURL;
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select b.title,b.prod_QA,b.back_top_logo,HashKey,HashIv,mer_id,paypal,storeType from CurrentUseFrame as a left join head as b on a.id=b.hid", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            while (reader.Read())
                            {
                                this.WebTitle.Text = Encoder.HtmlEncode(reader[0].ToString());
                                Page.Title = Encoder.HtmlEncode(reader[0].ToString());
                                ProdQA = Encoder.HtmlEncode(reader[1].ToString());
                                HyperLink1.NavigateUrl = WebURL;
                                if (reader["storeType"].ToString() == "2") storeType2 = true;
                                if (reader[3].ToString() != "" && reader[4].ToString() != "" && reader[5].ToString() != "" && reader[6].ToString() == "Y")
                                {
                                    CreditDisplay = "Y";
                                }

                                if (reader[2].ToString() != "")
                                {
                                    if (reader[2].ToString().IndexOf("http") > -1)
                                    {
                                        Image1.ImageUrl = Encoder.HtmlEncode(reader[2].ToString());
                                    }
                                    else {
                                        Image1.ImageUrl = WebURL + Encoder.HtmlEncode(reader[2].ToString());
                                    }
                                    
                                    WebTitle.Visible = false;
                                }
                                else {
                                    Image1.Visible = false;                                    
                                }
                                
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }

                    #region 是否要開放Q&A分頁
                    if (ProdQA == "Y")
                    {
                        li6.Visible = true;
                    }
                    else
                    {
                        li6.Visible = false;
                    }
                    #endregion
                    #region 是否要開放虛擬商品分頁
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select * from prod where virtual='Y' and disp_opt='Y' and '" + DateTime.Now.ToString("yyyy-MM-dd") + "' between start_date and end_date", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                li3.Visible = true;
                                #region 是否要開放智財局檔案上傳查詢
                                li3.Visible = !storeType2;
                                li11.Visible = storeType2;
                                #endregion
                            }
                            else
                            {
                                li3.Visible = false;
                                li11.Visible = false;
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    #endregion
                    #region 是否要開放經銷商購買專區分頁
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select vip from cust where mem_id=@mem_id", conn);
                        cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    switch (reader[0].ToString()) { 
                                        case "1":

                                            li4.Visible = false;
                                            li2.Visible = true;
                                            li5.Visible = false;
                                            li7.Visible = false;
                                            break;

                                        case "2":
                                            
                                            li4.Visible = false;
                                            li2.Visible = true;
                                            li5.Visible = false;
                                            //li7.Visible = true;
                                            li7.Visible = false;    //2015.11.03會員卡掃Qrcode改為虛擬商品製作,暫時隱藏此頁
                                            break;

                                        case "3":

                                            li4.Visible = true;
                                            li2.Visible = false;
                                            li5.Visible = true;
                                            li7.Visible = false;
                                            break;
                                    }                                    
                                }
                            }
                            else
                            {
                                li4.Visible = false;
                                li2.Visible = true;
                                li5.Visible = false;
                                li7.Visible = false;
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    #endregion
                    #region 是否要開放紅利紀錄查詢
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select * from authors where canexe='Y' and job_id='F002'", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    li1.Visible = true;
                                }
                            }
                            else
                            {
                                li1.Visible = false;
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    #endregion
                    #region 是否要開放優惠券
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select * from authors where canexe='Y' and job_id='E036'", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    li8.Visible = true;
                                }
                            }
                            else
                            {
                                li8.Visible = false;
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    #endregion
                    #region 是否要開放信用卡設定
                    if (CreditDisplay == "Y")
                    {
                        li9.Visible = true;
                    }
                    else {
                        li9.Visible = false;
                    }

                    #endregion
                    #region 是否要開放服務紀錄
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select * from serviceRecord where [type]=0 and [status]!=2 and bindID=@bindID", conn);
                        cmd.Parameters.Add(new SqlParameter("@bindID", MemID));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                li10.Visible = true;
                            }
                            else
                            {
                                li10.Visible = false;
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    #endregion
                    #region 是否要開放[對帳單]
                    li12.Visible = false;
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        List<string> okMem = new List<string> { "000001", "000004", "000005" };
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select canexe from webjobs where job_id = 'E007'", conn);
                        try {
                            SqlDataReader reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                if (reader["canexe"].ToString() == "Y" && okMem.Contains(MemID))
                                {
                                    li12.Visible = true;
                                }
                            }
                        }
                        catch (Exception ex) { }
                    }
                    
                    #endregion

                    String MemberEditURL = Encoder.HtmlEncode("Member.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String QrCodeURL = Encoder.HtmlEncode("QrCode.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String OrderListURL = Encoder.HtmlEncode("OrderList.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String OrderLisr2URL = Encoder.HtmlEncode("OrderList2.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String VirtualOrderListURL = Encoder.HtmlEncode("VirtualOrderList.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String DealerURL = Encoder.HtmlEncode("Dealer.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String QAURL = Encoder.HtmlEncode("QA.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String BonusURL = Encoder.HtmlEncode("MemberBonus.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String CouponURL = Encoder.HtmlEncode("Coupon.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String CreditURL = Encoder.HtmlEncode("Credit.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String ServiceRecordListURL = Encoder.HtmlEncode("ServiceRecordList.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String StoreType2URL = Encoder.HtmlEncode("StoreType2.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);
                    String BankStatementURL = Encoder.HtmlEncode("Customer/BankStatement.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MemID + "&language=" + this.language.Value + "&CheckM=" + CheckM);

                    memberedit.InnerHtml = "<iframe src='" + MemberEditURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' id='memberedit' name='memberedit'></iframe>";
                    if (li1.Visible) bonuslist.InnerHtml = "<iframe src='" + BonusURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' id='bonuslist' name='bonuslist'></iframe>";
                    if (li2.Visible) orderlist.InnerHtml = "<iframe src='" + OrderListURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' id='orderlist' name='orderlist'></iframe>";
                    if (li5.Visible) orderlist2.InnerHtml = "<iframe src='" + OrderLisr2URL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' id='orderlist2' name='orderlist2'></iframe>";
                    if (li3.Visible) VirtualOrderList.InnerHtml = "<iframe src='" + VirtualOrderListURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' id='VirtualOrderList' name='VirtualOrderList'></iframe>";
                    if (li4.Visible) Dealer.InnerHtml = "<iframe src='" + DealerURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' id='Dealer' name='Dealer'></iframe>";
                    if (li6.Visible) QA.InnerHtml = "<iframe src='" + QAURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' id='QA' name='QA'></iframe>";
                    if (li7.Visible) QrCode.InnerHtml = "<iframe src='" + QrCodeURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' id='QrCode' name='QrCode'></iframe>";
                    if (li8.Visible) Coupon.InnerHtml = "<iframe src='" + CouponURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' id='Coupon' name='Coupon'></iframe>";
                    if (li9.Visible) Credit.InnerHtml = "<iframe src='" + CreditURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' id='Credit' name='Credit'></iframe>";
                    if (li10.Visible) ServiceRecordList.InnerHtml = "<iframe src='" + ServiceRecordListURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' name='ServiceRecord'></iframe>";
                    if (li11.Visible) StoreType2.InnerHtml = "<iframe src='" + StoreType2URL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' name='StoreType2'></iframe>";
                    if (li12.Visible) BankStatement.InnerHtml = "<iframe src='" + BankStatementURL + "' class='autoHeight' scrolling='no'  frameborder='0' style='width:100%;' name='StoreType2'></iframe>";
                }
                else 
                {
                    Response.Redirect(this.returnurl.Value);
                }
            }
            else
            {
                Page.Title = this.WebTitle.Text;
            }
        }
        
        public Boolean isright(string s, String right) //定義正則表達式函數
        {
            Regex Regex1 = new Regex(right, RegexOptions.IgnoreCase);
            return Regex1.IsMatch(s);
        }
        
        protected void LinkButton2_Click(object sender, EventArgs e)
        {
            Response.Redirect(this.returnurl.Value);
        }

        protected void LinkButton3_Click(object sender, EventArgs e)
        {
            Session.Clear();

            String StrUrl = this.returnurl.Value; 
            string[] strs = StrUrl.Split(new string[] { "/tw/" }, StringSplitOptions.RemoveEmptyEntries);            
            Response.Redirect(strs[0] + "/tw/logout.asp");            
        }

        protected void LinkButton4_Click(object sender, EventArgs e)
        {
            //Response.Redirect("OrderList.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + this.returnurl.Value + "&MemID=" + this.MemberID.Text + "&language=" + str_language);
        }

    }
}