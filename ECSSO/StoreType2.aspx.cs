using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.Security.Application;

namespace ECSSO
{
    public partial class StoreType2 : System.Web.UI.Page
    {
        private string str_language = string.Empty, web_url;
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/StoreType2.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            this.language.Value = str_language;
            if (!IsPostBack) {
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
                        if (reader.Read())
                        {
                            if (reader["web_url"].ToString().IndexOf("http") >= 0)
                            {
                                web_url = reader["web_url"].ToString();
                                this.returnurl.Value = reader["web_url"].ToString() + "?" + this.returnurl.Value.Split('?')[1];
                            }
                            else
                            {
                                web_url = "http://" + reader["web_url"].ToString();
                                this.returnurl.Value = "http://" + reader["web_url"].ToString() + "?" + this.returnurl.Value.Split('?')[1];
                            }
                        }
                    }
                    catch
                    {

                    }
                    finally { reader.Close(); }
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
                                    if(reader["web_url"].ToString().IndexOf("http")>=0)
                                        this.returnurl.Value = reader["web_url"].ToString();
                                    else
                                        this.returnurl.Value = "http://" + reader["web_url"].ToString();
                                }
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
                setting = GS.GetSetting2(this.siteid.Value);
                if (GS.MD5Check(this.siteid.Value + MemID, CheckM)) 
                {
                    List<String> MySearchID = new List<string>();
                    /*MySearchID.Add("97");
                    MySearchID.Add("299");
                    */
                    String Str_Left = "";
                    String FirstNo = "";
                    string token = getToken(setting, MemID);
                    setDonloadFileLine(setting);
                    Products Prods = new Products();
                    Str_Left += "<div id='tips'>" + GetLocalResourceObject("StringResource1") + "</div>";
                    Str_Left += "<div id='arrow'></div>";
                    Str_Left += "<ul class='nav nav-pills nav-stacked' role='tablist'>";

                    int i = 0;
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(@"
                            select * from orders_hd
                            where id in (select order_no from orders where orders.virtual='Y') and
                                 mem_id = @MemID and amt<>0 and[state]in (2,3,6,7)
                        ", conn);
                        cmd.Parameters.Add(new SqlParameter("@MemID", MemID));
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read()) {
                            if (i == 0)
                            {
                                FirstNo = reader["id"].ToString();
                            }
                            Str_Left += $"<li id='navOrder{reader["id"].ToString()}' class='nav-menu' role='presentation'><a href='javascript:void(0);' OnClick='ChangeOrder({this.siteid.Value},\"{reader["id"].ToString()}\",\"{MemID}\",\"{token}\");' >{reader["id"].ToString()}</a></li>";
                            i = i + 1;
                        }
                    }
                    Str_Left += "</ul>";

                    leftframe.InnerHtml = HttpUtility.HtmlDecode(Server.HtmlDecode(Encoder.HtmlEncode(Str_Left)));

                    if (FirstNo != "")
                    {
                        body.Attributes.Add("onload", $"ChangeOrder({this.siteid.Value},\"{FirstNo}\",\"{MemID}\",\"{token}\");");
                    }
                    else
                    {
                        rightframe.InnerHtml = "<span class='notfoundprod'>" + GetLocalResourceObject("StringResource2") + "</span>";
                    }
                }
            }
        }
        private void setDonloadFileLine(string setting) {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select store2File1,store2File2 from CurrentUseHead
                ", conn);
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (!string.IsNullOrEmpty(reader["store2File1"].ToString()))
                        {
                            string[] ex1 = reader["store2File1"].ToString().Split('.');
                            this.donloadStore2File1.Attributes["href"] = web_url + reader["store2File1"].ToString();
                            this.donloadStore2File1.Attributes["download"] = $"商標委任書.{ex1[ex1.Length - 1]}";
                            this.donloadStore2File1.Attributes["title"] = $"商標委任書";
                            if (web_url.IndexOf("https") < 0) this.donloadStore2File1.Attributes["target"]= "_blank";
                        }
                        else this.donloadStore2File1.Visible = false;
                        if (!string.IsNullOrEmpty(reader["store2File2"].ToString()))
                        {
                            string[] ex2 = reader["store2File2"].ToString().Split('.');
                            this.donloadStore2File2.Attributes["href"] = web_url + reader["store2File2"].ToString();
                            this.donloadStore2File2.Attributes["download"] = $"委任契約書.{ex2[ex2.Length - 1]}";
                            this.donloadStore2File2.Attributes["title"] = $"委任契約書";
                            if (web_url.IndexOf("https") < 0) this.donloadStore2File2.Attributes["target"] = "_blank";
                        }
                        else this.donloadStore2File2.Visible = false;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        private string getToken(string setting,string memID) {
            string token = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select token.* from token 
                    left join Cust on cust.id=token.ManagerID
                    where Cust.mem_id=@MemID
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@MemID", memID));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        token = reader["id"].ToString();
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return token;
        }
        protected void LinkButton2_Click(object sender, EventArgs e)
        {
            //Response.Redirect(this.returnurl.Value);
            Response.Write("<script language='javascript'>window.parent.window.location.href='" + this.returnurl.Value + "';</script>");
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