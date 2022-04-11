using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Web.Security;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Globalization;

namespace ECSSO
{
    public partial class ForgetPassword : System.Web.UI.Page
    {
        private string str_language = string.Empty;
        //語系變換
        protected override void InitializeCulture()
        {
            //此currentculture來自default.aspx頁面上兩個超連結的連結位址,見html部分            
            if (Request.QueryString["language"] != null || Request.QueryString["language"].ToString() != "")
            {
                str_language = Request.QueryString["language"].ToString();
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
        protected void Page_Load(object sender, EventArgs e)
        {
            this.language.Value = str_language;
            if (!IsPostBack)
            {
                String setting = "";
                if (Request.QueryString["SiteID"] != null)
                {
                    if (Request.QueryString["SiteID"].ToString() != "")
                    {
                        this.siteid.Value = Request.QueryString["SiteID"].ToString();
                    }
                    else
                    {

                        Response.Write("<script type='text/javascript'>history.go(-1);</script>");
                        Response.End();
                    }
                }
                if (Request.QueryString["Url"] != null)
                {
                    if (Request.QueryString["Url"].ToString() != "")
                    {
                        this.weburl.Value = HttpContext.Current.Server.UrlDecode(Request.QueryString["Url"].ToString());
                    }
                }
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
                    SqlCommand cmd = new SqlCommand("select dbname,dbusername,dbpassword,web_url from cocker_cust where id=@id", conn);
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
                            this.WebTitle.Text = reader[0].ToString();
                            Page.Title = reader[0].ToString();
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
                Page.Title = this.WebTitle.Text;
            }
        }

        public void send_email(string msg, string mysubject, string sender, string mail)
        {
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            message.From = new MailAddress(sender, "客服中心");
            message.Bcc.Add(sender);
            message.To.Add(mail);

            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;//E-mail編碼
            message.Subject = mysubject;//E-mail主旨
            message.Body = msg;//E-mail內容

            SmtpClient smtpClient = new SmtpClient("msa.hinet.net");//設定E-mail Server和port
            try
            {
                smtpClient.Send(message);
            }
            catch
            {
                smtpClient.Send(message);
            }

        }
        
        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            if (this.Email.Text == "")
            {
                this.CheckEmail.Text = "請輸入Email";
                this.CheckEmail.Visible = true;
            }
            else
            {
                this.CheckEmail.Text = "";
                this.CheckEmail.Visible = false;
            }
            if (this.Tel.Text == "")
            {
                this.CheckTel.Text = "請輸入電話";
                this.CheckTel.Visible = true;
            }
            else
            {
                this.CheckTel.Text = "";
                this.CheckTel.Visible = false;
            }

            if (!this.CheckTel.Visible && !this.CheckEmail.Visible)
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
                        String Str_sql = "select email,id,pwd from cust where id=@id and chk='Y' and (tel=@tel or cell_phone=@cell_phone)";
                        SqlCommand cmd = new SqlCommand(Str_sql, conn);
                        cmd.Parameters.Add(new SqlParameter("@id", this.Email.Text));
                        cmd.Parameters.Add(new SqlParameter("@tel", this.Tel.Text));
                        cmd.Parameters.Add(new SqlParameter("@cell_phone", this.Tel.Text));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                using (SqlConnection conn2 = new SqlConnection(setting))
                                {
                                    conn2.Open();
                                    String Str_sql2 = "select service_mail,title from head";
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
                                        cmd2.Parameters.Add(new SqlParameter("@id", CustID));
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
                                Mail_cont = Mail_cont + "】 忘記密碼通知信</font></td>";
                                Mail_cont = Mail_cont + "  </tr>";
                                Mail_cont = Mail_cont + "  <tr>";
                                Mail_cont = Mail_cont + "    <td><img src='http://www.cocker.com.tw/images/cockermail_bg.jpg'></td>";
                                Mail_cont = Mail_cont + "  </tr>";
                                Mail_cont = Mail_cont + "  <tr>";
                                Mail_cont = Mail_cont + "    <td><table width='450' border='0' align='center' cellpadding='0' cellspacing='0'><tr><td height='15'></td></tr><tr><td ><font color='#333333' size='3'>請熟記以下重要訊息<br>您的帳號 / ";
                                Mail_cont = Mail_cont + CustID;
                                Mail_cont = Mail_cont + "<br>您的密碼 / ";
                                Mail_cont = Mail_cont + CustPwd;
                                Mail_cont = Mail_cont + "</font><br>您的密碼已重新設定，請使用新密碼登入，謝謝</td></tr></table></td>";
                                Mail_cont = Mail_cont + "  </tr>";
                                Mail_cont = Mail_cont + "  <tr>";
                                Mail_cont = Mail_cont + "    <td align='center' height='15'></td>";
                                Mail_cont = Mail_cont + "  </tr>";
                                Mail_cont = Mail_cont + "  <tr>";
                                Mail_cont = Mail_cont + "    <td align='center'><font color='#d62929' size='2'>提醒您，客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款<br>若有上述情形，請立即撥打165防詐騙專線查詢</font></td>";
                                Mail_cont = Mail_cont + "  </tr>";
                                Mail_cont = Mail_cont + "</table>";
                                try
                                {
                                    send_email(Mail_cont, "忘記密碼通知 【" + ServiceTitle + "】", ServiceMail, CustMail);//呼叫send_email函式測試                            
                                }
                                finally
                                {
                                    Label1.Text = "您的帳密已發送到您的電子信箱。";
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    Label1.Text = "您的帳密已發送到您的電子信箱。";
                    this.Label6.Text = "";
                }
                else {
                    this.Label6.Text = "請輸入正確驗證碼";
                }                
            }            
        }

        protected void LinkButton2_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx?language=" + this.language.Value + "&SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value));
        }
    }
}