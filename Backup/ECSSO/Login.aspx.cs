using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Globalization;

namespace ECSSO
{
    public partial class Login : System.Web.UI.Page
    {
        private string str_language = string.Empty;
        //語系變換
        protected override void InitializeCulture()
        {
            //此currentculture來自default.aspx頁面上兩個超連結的連結位址,見html部分
            
            if (Request.Form["language"] != null)
            {
                str_language = Request.Form["language"].ToString();
            }
            else {
                if (Request.QueryString["language"] != null)
                {
                    str_language = Request.QueryString["language"].ToString();
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
        protected void Page_Load(object sender, EventArgs e)
        {           
            this.language.Value = str_language;
            /*---------安慶 start---------*/

            //當Session["target"] == "EmailChecked"時，代表使用者才剛認證完email，那就自動幫他跳轉一下吧
            if (Session["target"] != null && Session["target"].ToString() == "EmailChecked")
            {
                //目前只有Hinet會需要額外認證
                Session["target"] = "HinetAPI";
                Server.Transfer("HinetAPI/index.aspx");
                return;
            }

            //如果有target這個session就代表這個使用者目前是要用OAuth的方式登入,所以直接server內跳轉就可以了
            //記得登入完後要把target清掉阿
            if (Session["target"] != null && Session["target"].ToString() == "HinetAPI")
            {
                Server.Transfer("HinetAPI/index.aspx");
                return;
            }
            if (Session["target"] != null && Session["target"].ToString() == "GoogleAPI")
            {
                Server.Transfer("GAPI/index.aspx");
                return;
            }
            if (Session["target"] != null && Session["target"].ToString() == "FaceBookAPI")
            {
                Server.Transfer("FBAPI/index.aspx");
                return;
            }
            //如果上面都沒攔截到那就把這個session刪掉吧
            Session.Remove("target");


            /*-----------安慶 end--------------*/


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
                    else {

                        Response.Write("<script type='text/javascript'>history.go(-1);</script>");
                        Response.End();
                    }
                }

                if (Request.Form["ReturnUrl"] != null)
                {
                    this.returnurl.Value = Request.Form["ReturnUrl"].ToString();
                }
                else
                {
                    if (Request.QueryString["ReturnUrl"].ToString() != null)
                    {
                        this.returnurl.Value = Request.QueryString["ReturnUrl"].ToString();
                    }
                }

                if (Request.Form["Url"] != null)
                {
                    this.weburl.Value = Request.Form["Url"].ToString();
                }
                else
                {
                    if (Request.QueryString["Url"].ToString() != null)
                    {
                        this.weburl.Value = Request.QueryString["Url"].ToString();
                    }
                }

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select dbname,dbusername,dbpassword,web_url from cocker_cust where id=@id", conn);
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
                                if (this.weburl.Value == "")
                                {
                                    this.weburl.Value = "http://" + reader["web_url"].ToString();
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

                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select title from head", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            WebTitle.Text = reader[0].ToString();
                            Page.Title = reader[0].ToString();
                        }
                    }
                }
            }
            else {
                Page.Title = WebTitle.Text;
            }
        }
                
        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            
            if (this.UserID.Text == "")
            {
                this.CheckUserID.Text = "請輸入帳號";                
                this.CheckUserID.Visible = true;
            }
            else
            {
                this.CheckUserID.Text = "";
                this.CheckUserID.Visible = false;
            }

            if (this.UserPwd.Text == "")
            {
                this.CheckUserPwd.Text = "請輸入密碼";
                this.CheckUserPwd.Visible = true;
            }
            else
            {
                this.CheckUserPwd.Text = "";
                this.CheckUserPwd.Visible = false;
            }

            if (!this.CheckUserID.Visible && !this.CheckUserPwd.Visible)
            {
                if (Session["CheckCode"] != null && String.Compare(Session["CheckCode"].ToString(), this.TextBox1.Text, true) == 0)
                {

                    GetStr getstr = new GetStr();
                    String setting = getstr.GetSetting(this.siteid.Value);

                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        String Str_sql = "sp_CheckPassWord @pwd,@id";
                        SqlCommand cmd = new SqlCommand(Str_sql, conn);
                        cmd.Parameters.Add(new SqlParameter("@pwd", this.UserPwd.Text));
                        cmd.Parameters.Add(new SqlParameter("@id", this.UserID.Text));                        
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            Token token = new Token();                            
                            String TokenID = token.LoginToken(this.UserID.Text, setting);
                            if (TokenID == "")
                            {
                                Label1.Text = "登入失敗";
                                this.Label1.Visible = true;
                            }
                            else 
                            {
                                GetStr gstr = new GetStr();
                                Response.Redirect(this.weburl.Value + "/" + gstr.GetLanString(this.language.Value) + "/log.asp?id=" + this.UserID.Text + "&tokenid=" + TokenID + "&ReturnUrl=" + this.returnurl.Value.Replace("&", "////").Split('?')[1]);
                            }                            
                        }
                        else
                        {
                            Label1.Text = "登入失敗";
                            this.Label1.Visible = true;
                        }
                    }
                    this.Label1.Text = "";
                    this.Label1.Visible = false;
                }
                else
                {
                    Label1.Text = "請輸入正確驗證碼";
                    this.Label1.Visible = true;
                }
            }
        }

        protected void LinkButton2_Click(object sender, EventArgs e)
        {
            Response.Redirect("ForgetPassword.aspx?language=" + this.language.Value + "&SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value));
        }

        protected void LinkButton3_Click(object sender, EventArgs e)
        {
            Response.Redirect("MemberAdd.aspx?language=" + this.language.Value + "&SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value));
        }
        /*---------------安慶start-------------------*/
        protected void LinkButton4_Click(object sender, EventArgs e)
        {
            Session["target"] = "FaceBookAPI";
            //Response.Redirect("http://sson.ezsale.tw/Login.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value));
            Response.Redirect("FBAPI/index.aspx?SiteID=" + this.siteid.Value + "&language=" + this.language.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value));
        }

        protected void LinkButton5_Click(object sender, EventArgs e)
        {
            Session["target"] = "GoogleAPI";
            //Response.Redirect("http://sson.ezsale.tw/Login.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value));
            Response.Redirect("GAPI/index.aspx?SiteID=" + this.siteid.Value + "&language=" + this.language.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value));
        }

        protected void LinkButton6_Click(object sender, EventArgs e)
        {
            Session["target"] = "HinetAPI";
            Response.Redirect("http://sson.ezsale.tw/Login.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value));
        }
        /*---------------安慶end-------------------*/
        /*
        private string LoginToken(string custid,string setting) {
            String Str_Token = "";


            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                String Str_sql = "select sys.fn_VarBinToHexStr(hashbytes('MD5', convert(nvarchar,id+pwd+logintime))) from cust where id=@id";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", this.UserID.Text));
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read()) {
                        Str_Token = reader[0].ToString();
                    }                    
                }
            }

            return Str_Token;
        }*/
    }
}