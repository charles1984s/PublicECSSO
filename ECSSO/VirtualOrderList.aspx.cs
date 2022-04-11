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
    public partial class VirtualOrderList : System.Web.UI.Page
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/VirtualOrderList.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            this.language.Value = str_language;
            if (!IsPostBack)
            {
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
                                    this.returnurl.Value = "http://" + reader["web_url"].ToString();
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
                    List<String> MySearchID = new List<string>();
                    /*MySearchID.Add("97");
                    MySearchID.Add("299");
                    */
                    String Str_Left = "";
                    String FirstNo = "";
                    Products Prods = new Products();
                    Str_Left += "<div id='tips'>" + GetLocalResourceObject("StringResource1") + "</div>";
                    Str_Left += "<div id='arrow'></div>";
                    Library.Products.RootObject rlib = JsonConvert.DeserializeObject<Library.Products.RootObject>(Prods.GetProdJson(MySearchID, MemID, this.siteid.Value));
                    Str_Left += "<ul class='nav nav-pills nav-stacked' role='tablist'>";

                    int i = 0;
                    foreach (Library.Products.ProductData Product in rlib.ProductDatas)
                    {
                        if (i == 0)
                        {
                            FirstNo = Product.ID;
                        }
                        Str_Left += "<li role='presentation'><a href='javascript:void(0);' OnClick='ChangeProd(" + this.siteid.Value + "," + Product.ID + ",\"" + MemID + "\");' >" + Product.Title + "</a></li>";
                        i = i + 1;
                    }
                    Str_Left += "</ul>";

                    leftframe.InnerHtml = HttpUtility.HtmlDecode(Server.HtmlDecode(Encoder.HtmlEncode(Str_Left)));

                    if (FirstNo != "")
                    {
                        body.Attributes.Add("onload", "ChangeProd(" + this.siteid.Value + "," + FirstNo + ",\"" + MemID + "\");");
                    }
                    else
                    {
                        rightframe.InnerHtml = "<span class='notfoundprod'>" + GetLocalResourceObject("StringResource2") + "</span>";
                    }
                }
            }            
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