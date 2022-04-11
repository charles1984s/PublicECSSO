using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI.HtmlControls;
using Microsoft.Security.Application;
using System.Data.SqlClient;
using System.Configuration;
using ECSSO.Library;

namespace ECSSO
{
    public partial class ServiceRecordList : System.Web.UI.Page
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
            objLink.Attributes.Add("href", "SSOcss/serviceRecord.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            String setting = "";
            String TableDiv = "";
            this.language.Value = str_language;
            if (!IsPostBack)
            {
                #region 檢查必要參數
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
                                this.returnurl.Value = "https://" + reader["web_url"].ToString() + "?" + HttpContext.Current.Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString()).Split('?')[1];
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
                    ServiceRecord serviceRecord = new ServiceRecord();
                    setting = GS.GetSetting(this.siteid.Value);
                    serviceRecord.getMemRecode(setting, GS.GetOrgName(GS.GetSetting(this.siteid.Value)), "0", MemID);
                    foreach (ServiceRecordItem item in serviceRecord.items)
                    {
                        if (item.status == "2") continue;
                        TableDiv += "<div class='panel panel-default'>";
                        TableDiv += "     <div class='panel-heading' role='tab' id='serviceRecordHead" + item.ID + "'>";
                        TableDiv += "          <h4 class='panel-title'>";
                        TableDiv += "               <a role='button' data-toggle='collapse' data-parent='#myServiceRecords' href='#serviceRecord" + item.ID + "' aria-expanded='true' aria-controls='serviceRecord" + item.ID + "'>";
                        TableDiv += item.title;
                        TableDiv += "               </a>";
                        TableDiv += "          </h4>";
                        TableDiv += "     </div>";
                        TableDiv += "     <div id='serviceRecord" + item.ID + "' class='panel-collapse collapse' role='tabpanel' aria-labelledby='serviceRecordHead" + item.ID + "'>";
                        TableDiv += "          <div class='panel-body'>";
                        TableDiv += "               <div class='row'>";
                        TableDiv += "                    <h4>問題/需求說明</h4>";
                        TableDiv += "                    <div>";
                        TableDiv += item.question;
                        TableDiv += "                    </div>";
                        TableDiv += "               </div>";
                        TableDiv += "               <div class='row'>";
                        TableDiv += "                    <h4>處理情況</h4>";
                        TableDiv += "                    <div>";
                        TableDiv += item.handle;
                        TableDiv += "                    </div>";
                        TableDiv += "               </div>";
                        if (item.file != "")
                        {
                            TableDiv += "               <div class='row'>";
                            TableDiv += "               <a type='button' class='btn btn-success' href='" + item.file + "' target='_blank' download='" + item.title + item.notedate + "服務紀錄'><span class='glyphicon glyphicon-file'></span>檔案下載</a>";
                            TableDiv += "               </div>";
                        }
                        TableDiv += "          </div>";
                        TableDiv += "     </div>";
                        TableDiv += "</div>";
                    }
                    myServiceRecords.InnerHtml = HttpUtility.HtmlDecode(Server.HtmlDecode(Encoder.HtmlEncode(TableDiv)));
                }
            }
        }
    }
}