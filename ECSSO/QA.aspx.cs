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

namespace ECSSO
{
    public partial class QA : System.Web.UI.Page
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/QA.css");
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
                    CheckM = Microsoft.Security.Application.Encoder.HtmlEncode(Request.Form["CheckM"].ToString());
                }
                else
                {
                    if (Request.QueryString["CheckM"] != null)
                    {
                        CheckM = Microsoft.Security.Application.Encoder.HtmlEncode(Request.QueryString["CheckM"].ToString());
                    }
                }
                #endregion

                GetStr GS = new GetStr();

                if (GS.MD5Check(this.siteid.Value + MemID, CheckM))
                {
                    setting = GS.GetSetting(this.siteid.Value);
                    
                    String TableDiv = "";
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select a.cdate,a.title,a.cont,a.re_cont,b.sub_id,a.itemid,b.del from message as a left join prod as b on a.itemid=b.id where a.mem_id=@memid order by a.id desc", conn);
                        cmd.Parameters.Add(new SqlParameter("@memid", MemID));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    TableDiv += "<div class='panel panel-default'>";
                                    TableDiv += "   <div class='panel-heading'>";
                                    TableDiv += "       <div class='row'>";
                                    TableDiv += "           <div class='col-md-2'>" + reader[0].ToString() + "</div>";
                                    if (reader[6].ToString() == "N")
                                    {
                                        TableDiv += "           <div class='col-md-2'><a href='" + this.returnurl.Value.Split(':')[0].ToString() + "://" + this.returnurl.Value.Split(':')[1].Split('/')[2].ToString() + "/" + GS.GetLanString(str_language) + "/" + GS.GetUseModuleURL(setting, "10") + "&prod_sub_id=" + reader[4].ToString() + "&prod_id=" + reader[5].ToString() + "&prodSalesType=prod' target='_blank'>" + reader[1].ToString() + "</a></div>";
                                    }
                                    else
                                    {
                                        TableDiv += "           <div class='col-md-2'>" + reader[1].ToString() + "</div>";
                                    }
                                    TableDiv += "           <div class='col-md-4'>" + reader[2].ToString() + "</div>";
                                    TableDiv += "           <div class='col-md-4'>" + reader[3].ToString() + "</div>";
                                    TableDiv += "       </div>";
                                    TableDiv += "   </div>";
                                    TableDiv += "</div>";
                                }
                            }
                            else {
                                TableDiv += "<center>" + GetLocalResourceObject("StringResource1").ToString() + "</center>";
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    QAList.InnerHtml = TableDiv;
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