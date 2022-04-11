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
using Newtonsoft.Json;

namespace ECSSO
{
    public partial class MemberBonus : System.Web.UI.Page
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/bonuslist.css");
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
                String TableDiv = "";

                GetStr GS = new GetStr();
                if (GS.MD5Check(this.siteid.Value + this.MemberID.Value, this.CheckM.Value))
                {
                    Library.Member MB = new Library.Member();
                    Library.Member.RootObject rlib = JsonConvert.DeserializeObject<Library.Member.RootObject>(MB.GetBonusListJson(this.MemberID.Value, this.siteid.Value));
                    int count = 0;
                    foreach (Library.Member.Bonus bonus in rlib.Bonus)
                    {
                        count += 1;
                        TableDiv += "<div class='panel panel-default'>";
                        TableDiv += "   <div class='panel-heading'>";
                        TableDiv += "       <a data-toggle='collapse' data-parent='#OrdersList' aria-expanded='true' aria-controls='collapse" + count + "'>";
                        TableDiv += "           <div class='row'>";
                        TableDiv += "               <div class='col-md-1'>" + count + "</div>";
                        TableDiv += "               <div class='col-md-2'>" + bonus.Date + "</div>";
                        TableDiv += "               <div class='col-md-2'>" + bonus.Add + "</div>";
                        TableDiv += "               <div class='col-md-2'>" + bonus.Spend + "</div>";
                        TableDiv += "               <div class='col-md-5'>" + bonus.Memo + "</div>";
                        TableDiv += "           </div>";
                        TableDiv += "       </a>";
                        TableDiv += "   </div>";
                        TableDiv += "</div>";
                    }

                    BonusList.InnerHtml = HttpUtility.HtmlDecode(Server.HtmlDecode(Encoder.HtmlEncode(TableDiv)));
                }
            }
            else
            {
                //Page.Title = this.WebTitle.Text;
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