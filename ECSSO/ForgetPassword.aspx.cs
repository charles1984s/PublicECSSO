using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Web.Security;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Globalization;
using Microsoft.Security.Application;
using System.IO;
using System.Net;
using System.Text;


namespace ECSSO
{
    public partial class ForgetPassword : System.Web.UI.Page
    {
        private string str_language = string.Empty;
        //語系變換
        protected override void InitializeCulture()
        {
            //此currentculture來自default.aspx頁面上兩個超連結的連結位址,見html部分            
            if (Request.QueryString["language"] != null)
            {
                str_language = Microsoft.Security.Application.Encoder.HtmlEncode(Request.QueryString["language"].ToString());
            }
            else
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/forgetpassword.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            this.language.Value = str_language;
            String CheckM = "";
            if (!IsPostBack)
            {
                String setting = "";                

                if (Request.QueryString["SiteID"] != null)
                {
                    if (Request.QueryString["SiteID"].ToString() != "")
                    {
                        this.siteid.Value = Microsoft.Security.Application.Encoder.HtmlEncode(Request.QueryString["SiteID"].ToString());
                    }
                    else
                    {

                        Response.Write("<script type='text/javascript'>history.go(-1);</script>");
                        Response.End();
                    }
                }
                /*if (Request.QueryString["Url"] != null)
                {
                    if (Request.QueryString["Url"].ToString() != "")
                    {
                        this.weburl.Value = HttpContext.Current.Server.UrlDecode(Request.QueryString["Url"].ToString());
                    }
                }*/
                if (Request.QueryString["ReturnUrl"] != null)
                {
                    if (Request.QueryString["ReturnUrl"].ToString() != "")
                    {
                        this.returnurl.Value = HttpContext.Current.Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString());
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
                                this.weburl.Value = "http://" + reader["web_url"].ToString();
                                this.returnurl.Value = this.weburl.Value + "?" + HttpContext.Current.Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString()).Split('?')[1];
                            }
                        }
                    }
                    catch
                    {

                    }
                    finally { reader.Close(); }
                }

                if (Request.Form["CheckM"] != null)
                {
                    CheckM = Microsoft.Security.Application.Encoder.HtmlEncode(Request.Form["CheckM"].ToString());
                    this.Checkm.Value = Microsoft.Security.Application.Encoder.HtmlEncode(Request.Form["CheckM"].ToString());
                }
                else
                {
                    if (Request.QueryString["CheckM"] != null)
                    {
                        CheckM = Microsoft.Security.Application.Encoder.HtmlEncode(Request.QueryString["CheckM"].ToString());
                        this.Checkm.Value = Microsoft.Security.Application.Encoder.HtmlEncode(Request.QueryString["CheckM"].ToString());
                    }
                }

                GetStr GS = new GetStr();

                if (GS.MD5Check(this.siteid.Value, CheckM))
                {
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
                                    if (this.weburl.Value == "")
                                    {
                                        this.weburl.Value = "http://" + reader["web_url"].ToString();
                                    }
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

                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select title from head", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            while (reader.Read())
                            {
                                this.WebTitle.Text = Microsoft.Security.Application.Encoder.HtmlEncode(reader[0].ToString());
                                Page.Title = Microsoft.Security.Application.Encoder.HtmlEncode(reader[0].ToString());
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
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

        public void send_email(string msg, string mysubject, string sender, string mail)
        {
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            message.From = new MailAddress(
                (ConfigurationManager.AppSettings.Get("CredentialUser") == ""? sender:ConfigurationManager.AppSettings.Get("CredentialUser")), 
                GetLocalResourceObject("StringResource1").ToString());
            message.Bcc.Add(sender);
            message.To.Add(mail);

            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;//E-mail編碼
            message.Subject = mysubject;//E-mail主旨
            message.Body = msg;//E-mail內容

            //SmtpClient smtpClient = new SmtpClient("msa.hinet.net");//設定E-mail Server和port
            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings.Get("smtpServer"), Int32.Parse(ConfigurationManager.AppSettings.Get("smtpPort")));//設定E-mail Server和port
            if (ConfigurationManager.AppSettings.Get("CredentialUser") != "")
            {
                smtpClient.Credentials = new NetworkCredential(
                        ConfigurationManager.AppSettings.Get("CredentialUser"),
                        ConfigurationManager.AppSettings.Get("CredentialPW")
                );
                smtpClient.EnableSsl = true;
            }
            try
            {
                smtpClient.Send(message);
            }
            catch(Exception e)
            {
                Response.Write(@"<div class=""alert alert-danger"">
                <a href=""#"" class=""close"" data-dismiss=""alert"">&times;</a>Email驗證錯誤</div>");
            }

        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            if (Session["CheckCode"] != null && String.Compare(Session["CheckCode"].ToString(), this.TextBox1.Text, true) == 0)
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2;
                WebRequest request = WebRequest.Create(Request.Url.Scheme.ToString() + "://" + Request.Url.Host.ToString() + "/ResetPassword.aspx");
                request.Method = "POST";
                string postData = new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Query.TrimStart(new Char[] { '?', '&' }) + "&mail=" + Microsoft.Security.Application.Encoder.HtmlEncode(Email.Text);

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                try
                {
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    WebResponse response = request.GetResponse();
                    //Response.Write(((HttpWebResponse)response).StatusDescription);
                    dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd(); //ResetPassword.aspx回傳訊息

                    if (responseFromServer.Contains("@"))
                    {
                        Response.Write($@"<div class=""alert alert-success""><a href=""#"" class=""close"" data-dismiss=""alert"">&times;</a>{responseFromServer}</div>");     ///<strong>成功!</strong>
                    }
                    //else if (responseFromServer.Contains("失敗"))
                    else
                    {
                        throw new Exception(responseFromServer);
                    }
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                }
                catch (Exception ex) {
                    Response.Write($@"<div class=""alert alert-danger""><a href=""#"" class=""close"" data-dismiss=""alert"">&times;</a>{ex.Message}</div>");     ///<strong>錯誤!</strong>
                }
            }
            else
            {
                this.Label6.Text = GetLocalResourceObject("StringResource11").ToString();
            }  
            
        }
        protected void LinkButton1_Click_old(object sender, EventArgs e)
        {
            if (this.Email.Text == "")
            {
                this.CheckEmail.Text = GetLocalResourceObject("StringResource2").ToString();
                this.CheckEmail.Visible = true;
            }
            else
            {
                this.CheckEmail.Text = "";
                this.CheckEmail.Visible = false;
            }

            if (!this.CheckEmail.Visible)
            {
                if (Session["CheckCode"] != null && String.Compare(Session["CheckCode"].ToString(), this.TextBox1.Text, true) == 0)
                {
                    String ServiceMail = "";
                    String ServiceTitle = "";
                    String CustID = "";
                    String CustMail = "";
                    String CustPwd = "";
                    GetStr getstr = new GetStr();
                    String setting = getstr.GetSetting(this.siteid.Value);
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        String Str_sql = "select email,id,pwd from cust where id=@id and chk='Y'";
                        SqlCommand cmd = new SqlCommand(Str_sql, conn);
                        cmd.Parameters.Add(new SqlParameter("@id", this.Email.Text));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                using (SqlConnection conn2 = new SqlConnection(setting))
                                {
                                    conn2.Open();
                                    String Str_sql2 = "select b.service_mail,b.title from CurrentUseFrame as a left join head as b on a.id=b.hid";
                                    SqlCommand cmd2 = new SqlCommand(Str_sql2, conn2);
                                    SqlDataReader reader2 = cmd2.ExecuteReader();
                                    try
                                    {
                                        while (reader2.Read())
                                        {
                                            ServiceMail = reader2[0].ToString();
                                            ServiceTitle = reader2[1].ToString();
                                        }
                                    }
                                    finally
                                    {
                                        reader2.Close();
                                    }
                                }

                                while (reader.Read())
                                {
                                    CustMail = reader[0].ToString();
                                    CustID = reader[1].ToString();
                                    CustPwd = Membership.GeneratePassword(8, 1);

                                    using (SqlConnection conn2 = new SqlConnection(setting))
                                    {
                                        conn2.Open();
                                        SqlCommand cmd2 = new SqlCommand();
                                        cmd2.CommandText = "sp_ResetPassword";
                                        cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                                        cmd2.Connection = conn2;
                                        cmd2.Parameters.Add(new SqlParameter("@id", reader[1].ToString()));
                                        cmd2.Parameters.Add(new SqlParameter("@pwd", CustPwd));
                                        cmd2.ExecuteNonQuery();                                        
                                    }
                                }

                                if (ServiceMail == "")
                                {
                                    ServiceMail = "service@ether.com.tw";
                                }
                                String Mail_cont = "<table width='576' cellpadding='0' cellspacing='0' align='center'>";
                                Mail_cont = Mail_cont + "  <tr>";
                                Mail_cont = Mail_cont + "    <td><font color='#333333' size='3'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;【";
                                Mail_cont = Mail_cont + ServiceTitle;
                                Mail_cont = Mail_cont + "】 " + GetLocalResourceObject("StringResource9").ToString() + "</font></td>";
                                Mail_cont = Mail_cont + "  </tr>";
                                Mail_cont = Mail_cont + "  <tr>";
                                Mail_cont = Mail_cont + "    <td><img src='http://www.cocker.com.tw/images/cockermail_bg.jpg'></td>";
                                Mail_cont = Mail_cont + "  </tr>";
                                Mail_cont = Mail_cont + "  <tr>";
                                Mail_cont = Mail_cont + "    <td><table width='450' border='0' align='center' cellpadding='0' cellspacing='0'><tr><td height='15'></td></tr><tr><td ><font color='#333333' size='3'>" + GetLocalResourceObject("StringResource3").ToString() + "<br>" + GetLocalResourceObject("StringResource4").ToString() + " / ";
                                Mail_cont = Mail_cont + CustID;
                                Mail_cont = Mail_cont + "<br>" + GetLocalResourceObject("StringResource5").ToString() + " / ";
                                Mail_cont = Mail_cont + CustPwd;
                                Mail_cont = Mail_cont + "</font><br>" + GetLocalResourceObject("StringResource6").ToString() + "</td></tr></table></td>";
                                Mail_cont = Mail_cont + "  </tr>";
                                Mail_cont = Mail_cont + "  <tr>";
                                Mail_cont = Mail_cont + "    <td align='center' height='15'></td>";
                                Mail_cont = Mail_cont + "  </tr>";
                                Mail_cont = Mail_cont + "  <tr>";
                                Mail_cont = Mail_cont + "    <td align='center'><font color='#d62929' size='2'>" + GetLocalResourceObject("StringResource7").ToString() + "<br>" + GetLocalResourceObject("StringResource8").ToString() + "</font></td>";
                                Mail_cont = Mail_cont + "  </tr>";
                                Mail_cont = Mail_cont + "</table>";
                                try
                                {
                                    send_email(Mail_cont, GetLocalResourceObject("StringResource9").ToString() + " 【" + ServiceTitle + "】", ServiceMail, CustMail);//呼叫send_email函式測試                            
                                }
                                finally
                                {
                                    Label1.Text = GetLocalResourceObject("StringResource10").ToString();
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    Label1.Text = GetLocalResourceObject("StringResource10").ToString();
                    this.Label6.Text = "";
                }
                else {
                    this.Label6.Text = GetLocalResourceObject("StringResource11").ToString();
                }                
            }
        }

        protected void LinkButton2_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx?language=" + this.language.Value + "&SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value) + "&CheckM=" + HttpContext.Current.Server.UrlEncode(this.Checkm.Value));
        }
    }
}