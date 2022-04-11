using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Globalization;
using Microsoft.Security.Application;

namespace ECSSO
{
    public partial class MemberAdd : System.Web.UI.Page
    {
        string str_language = string.Empty;
        //語系變換
        protected override void InitializeCulture()
        {
            //此currentculture來自default.aspx頁面上兩個超連結的連結位址,見html部分            
            if (Request.QueryString["language"] != null )
            {
                if (Request.QueryString["language"].ToString() != "")
                {
                    str_language = Request.QueryString["language"].ToString();
                }
                else {
                    str_language = "zh-tw";
                }                
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/memberadd.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
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
                                this.weburl.Value = reader["web_url"].ToString();
                                this.returnurl.Value = "http://" + this.weburl.Value + "?" + this.returnurl.Value.Split('?')[1];
                            }
                        }
                    }
                    catch
                    {

                    }
                    finally { reader.Close(); }
                }
                
                String CheckM = "";
                if (Request.Form["CheckM"] != null)
                {
                    CheckM = Encoder.HtmlEncode(Request.Form["CheckM"].ToString());
                    this.Checkm.Value = Encoder.HtmlEncode(Request.Form["CheckM"].ToString());
                }
                else
                {
                    if (Request.QueryString["CheckM"] != null)
                    {
                        CheckM = Encoder.HtmlEncode(Request.QueryString["CheckM"].ToString());
                        this.Checkm.Value = Encoder.HtmlEncode(Request.QueryString["CheckM"].ToString());
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
                        SqlCommand cmd = new SqlCommand("select b.title,b.member_agree from CurrentUseFrame as a left join head as b on a.id=b.hid", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            while (reader.Read())
                            {
                                this.WebTitle.Text = reader[0].ToString();
                                str_agree.InnerHtml = HttpUtility.HtmlDecode(Server.HtmlDecode(reader[1].ToString()));

                                Page.Title = Encoder.HtmlEncode(reader[0].ToString());
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
            else {
                Page.Title = this.WebTitle.Text;
            }
        }
       
        public void send_email(string msg, string mysubject, string sender, string mail)
        {
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            message.From = new MailAddress(sender, GetLocalResourceObject("StringResource1").ToString());
            //message.Bcc.Add(sender);
            message.To.Add(mail);

            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;//E-mail編碼
            message.Subject = mysubject;//E-mail主旨
            message.Body = msg;//E-mail內容

            //SmtpClient smtpClient = new SmtpClient("msa.hinet.net");//設定E-mail Server和port
            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings.Get("smtpServer"), Int32.Parse(ConfigurationManager.AppSettings.Get("smtpPort")));//設定E-mail Server和port
            if (ConfigurationManager.AppSettings.Get("CredentialUser") != "")
            {
                smtpClient.Credentials = new System.Net.NetworkCredential(
                        ConfigurationManager.AppSettings.Get("CredentialUser"),
                        ConfigurationManager.AppSettings.Get("CredentialPW")
                );
            }
            try
            {
                smtpClient.Send(message);
            }
            catch
            {
                smtpClient.Send(message);
            }
        }

        #region 定義正則表達式函數

        
        public Boolean isright(string s, String right) 
       {
            Regex Regex1 = new Regex(right, RegexOptions.IgnoreCase);
            return Regex1.IsMatch(s);
        }
        #endregion

        protected void LinkButton2_Click(object sender, EventArgs e)
        {
            Response.Redirect("login.aspx?language=" + this.language.Value + "&SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value) + "&CheckM=" + HttpContext.Current.Server.UrlEncode(this.Checkm.Value));
        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            #region 檢查填寫資料是否正確
            
            
            if (!this.CheckBox1.Checked)
            {
                this.CheckService.Text = GetLocalResourceObject("StringResource2").ToString();
                this.CheckService.Visible = true;
            }
            else {
                this.CheckService.Text = "";
                this.CheckService.Visible = false;
            }
            String RegStr = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
            if (!isright(this.Email.Text, RegStr))
            {
                this.CheckEmail.Text = GetLocalResourceObject("StringResource3").ToString();
                this.CheckEmail.Visible = true;
            }
            else
            {
                this.CheckEmail.Text = "";
                this.CheckEmail.Visible = false;
            }
            RegStr = @"^[0-9a-zA-Z_]{4,10}$";
            if (!isright(this.Pwd.Text, RegStr))
            {
                this.CheckPwd.Text = GetLocalResourceObject("StringResource4").ToString();
                this.CheckPwd.Visible = true;
            }
            else
            {
                this.CheckPwd.Text = "";
                this.CheckPwd.Visible = false;
            }
            if (this.ChkPwd.Text != this.Pwd.Text)
            {
                this.CheckPwd2.Text = GetLocalResourceObject("StringResource5").ToString();
                this.CheckPwd2.Visible = true;
            }
            else
            {
                this.CheckPwd2.Text = "";
                this.CheckPwd2.Visible = false;
            }
            if (this.Name.Text == "")
            {
                this.CheckName.Text = GetLocalResourceObject("StringResource6").ToString();
                this.CheckName.Visible = true;
            }
            else
            {
                this.CheckName.Text = "";
                this.CheckName.Visible = false;
            }
            DateTime dt = Convert.ToDateTime("1911-01-01");
            RegStr = @"\d{4}\-\d{2}\-\d{2}";
            if (!DateTime.TryParse(this.BirthDay.Text, out dt) || !isright(this.BirthDay.Text, RegStr))
            {
                this.CheckBirthDay.Text = GetLocalResourceObject("StringResource7").ToString();
                this.CheckBirthDay.Visible = true;
            }
            else
            {
                this.CheckBirthDay.Text = "";
                this.CheckBirthDay.Visible = false;
            }
            RegStr = @"\d{2,4}\-\d{6,8}";
            if (!isright(this.Tel.Text, RegStr))
            {
                this.CheckTel.Text = GetLocalResourceObject("StringResource8").ToString();
                this.CheckTel.Visible = true;
            }
            else
            {
                this.CheckTel.Text = "";
                this.CheckTel.Visible = false;
            }
            RegStr = @"09\d{2}\-\d{6}";
            if (!isright(this.CellPhone.Text, RegStr))
            {
                this.CheckCellPhone.Text = GetLocalResourceObject("StringResource9").ToString();
                this.CheckCellPhone.Visible = true;
            }
            else
            {
                this.CheckCellPhone.Text = "";
                this.CheckCellPhone.Visible = false;
            }
            if (this.address.Text == "")
            {
                this.Checkaddress.Text = GetLocalResourceObject("StringResource10").ToString();
                this.Checkaddress.Visible = true;
            }
            else
            {
                this.Checkaddress.Text = "";
                this.Checkaddress.Visible = false;
            }

            #endregion
            if (!this.CheckEmail.Visible && !this.CheckPwd.Visible && !this.CheckPwd2.Visible && !this.CheckName.Visible && !this.CheckBirthDay.Visible && !this.CheckTel.Visible && !this.CheckCellPhone.Visible && !this.Checkaddress.Visible && !this.CheckService.Visible)
            {
                if (Session["CheckCode"] != null && String.Compare(Session["CheckCode"].ToString(), this.TextBox1.Text, true) == 0)
                {
                    
                    String Str_Error = "";
                    String MemID = "";

                    GetStr getstr = new GetStr();
                    String setting = getstr.GetSetting(this.siteid.Value);                    
                    //Check ID repeat
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select mem_id from cust where id=@id", conn);
                        cmd.Parameters.Add(new SqlParameter("@id", this.Email.Text));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                Str_Error = GetLocalResourceObject("StringResource11").ToString();
                            }
                            else
                            {
                                //setting Mem_id
                                using (SqlConnection conn2 = new SqlConnection(setting))
                                {
                                    conn2.Open();
                                    SqlCommand cmd2 = new SqlCommand("select isnull(max(mem_id),'') from cust", conn2);
                                    SqlDataReader reader2 = cmd2.ExecuteReader();
                                    try
                                    {                                        
                                        while (reader2.Read())
                                        {
                                            if (reader2[0].ToString() != "")
                                            {
                                                MemID = (Convert.ToInt16(reader2[0].ToString()) + 1).ToString().PadLeft(6, '0');
                                            }
                                            else {
                                                MemID = "000001";
                                            }
                                                
                                        }                                     
                                    }
                                    finally
                                    {
                                        reader2.Close();
                                    }
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    if (MemID != "")
                    {
                        //Insert Cust
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = "sp_NewMember2";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = conn;
                            
                            cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                            cmd.Parameters.Add(new SqlParameter("@id", this.Email.Text));
                            cmd.Parameters.Add(new SqlParameter("@pwd", this.Pwd.Text));
                            cmd.Parameters.Add(new SqlParameter("@ch_name", this.Name.Text));
                            cmd.Parameters.Add(new SqlParameter("@sex", this.Sex.SelectedItem.Value));
                            cmd.Parameters.Add(new SqlParameter("@email", this.Email.Text));
                            cmd.Parameters.Add(new SqlParameter("@birth", Convert.ToDateTime(this.BirthDay.Text).ToString("yyyy-MM-dd")));
                            cmd.Parameters.Add(new SqlParameter("@tel", this.Tel.Text));
                            cmd.Parameters.Add(new SqlParameter("@cell_phone", this.CellPhone.Text));
                            cmd.Parameters.Add(new SqlParameter("@addr", this.ddlCity.SelectedItem.Text + this.ddlCountry.SelectedItem.Text + this.address.Text));
                            cmd.Parameters.Add(new SqlParameter("@ident", ""));
                            cmd.Parameters.Add(new SqlParameter("@id2", ""));
                            cmd.Parameters.Add(new SqlParameter("@C_ZIP", this.ddlzip.SelectedItem.Text));
                            cmd.Parameters.Add(new SqlParameter("@SnAndId", ""));
                            cmd.Parameters.Add(new SqlParameter("@chk", "O"));
                            cmd.ExecuteNonQuery();
                        }

                        String Service_mail = "";
                        String Mail_title = "";
                        String CrmVersion = "";
                        String CertificationURL = "";
                        String SendMemberMail = "";
                        //Search Service Data
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            String Str_sql = "select b.service_mail,b.title,b.crm_version,b.CertificationURL,b.send_member_mail from CurrentUseFrame as a left join head as b on a.id=b.hid";
                            SqlCommand cmd = new SqlCommand(Str_sql, conn);
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                if (reader.FieldCount > 0)
                                {
                                    while (reader.Read())
                                    {
                                        Service_mail = reader[0].ToString();
                                        Mail_title = reader[1].ToString();
                                        CrmVersion = reader[2].ToString();
                                        CertificationURL = reader[3].ToString();
                                        SendMemberMail = reader[4].ToString();
                                    }
                                }
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }

                        //Send Mail
                        GetStr GS = new GetStr();

                        /*新版會員信*/
                        String Mail_Cont = "";
                        Mail_Cont += "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />";
                        Mail_Cont += "<div style='font-family:微軟正黑體, Arial, 新細明體, Helvetica, sans-serif'>";
                        Mail_Cont += "    <font size='4' color='#ff0000'><b>" + GetLocalResourceObject("StringResource12") + Mail_title + GetLocalResourceObject("StringResource13") + "</b></font><br>";
                        Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px; margin-top:8px; margin-bottom:20px;'>";
                        Mail_Cont += GetLocalResourceObject("StringResource14").ToString() + "<br>";
                        Mail_Cont += "    <br>";
                        Mail_Cont += "    <strong>" + GetLocalResourceObject("StringResource15").ToString() + "：</strong>" + this.Email.Text + "<br>";
                        Mail_Cont += "    <br>";
                        Mail_Cont += "    <strong>" + GetLocalResourceObject("StringResource16").ToString() + "：</strong>" + this.Pwd.Text + "<br>";
                        Mail_Cont += "    <br>";
                        if (CertificationURL == "Y")
                        {
                            Mail_Cont += "    <strong>" + GetLocalResourceObject("StringResource17").ToString() + "</strong><br>";
                            Mail_Cont += "    <a href='" + this.weburl.Value + "/" + GS.GetLanString(this.language.Value) + "/checkm.asp?mem_id=" + MemID + "'>" + this.weburl.Value + "/" + GS.GetLanString(this.language.Value) + "/checkm.asp?mem_id=" + MemID + "</a><br>";
                            Mail_Cont += "    <span style='color: #666;'>(" + GetLocalResourceObject("StringResource18") + ")<br>";
                        }

                        Mail_Cont += GetLocalResourceObject("StringResource19") + "~</span><br>";
                        Mail_Cont += "    <br>";
                        Mail_Cont += "    <span style='color:#f00;'>" + GetLocalResourceObject("StringResource20").ToString() + "</span>";
                        Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px'>";
                        Mail_Cont += GetLocalResourceObject("StringResource21").ToString() + "    <br>";
                        Mail_Cont += GetLocalResourceObject("StringResource22").ToString();
                        Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";
                        Mail_Cont += "</div>";



                        String Mail_Cont2 = "";
                        Mail_Cont2 += "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />";
                        Mail_Cont2 += "<div style='font-family:微軟正黑體, Arial, 新細明體, Helvetica, sans-serif'>";
                        Mail_Cont2 += GetLocalResourceObject("StringResource23").ToString() + MemID + GetLocalResourceObject("StringResource24").ToString() + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + GetLocalResourceObject("StringResource25").ToString() + "<br>";
                        Mail_Cont2 += "</div>";


                        /*
                        String Mail_cont = "<center><span style='color:red;'>" + GetLocalResourceObject("提醒您：此封『會員通知』為系統發出，請勿直接回覆。").ToString() + "</span></center><br>";
                        Mail_cont += "<table width='576' cellpadding='0' cellspacing='0' align='center'>";
                        Mail_cont += "<tr><td><font color='#333333' size='3'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                        Mail_cont += Mail_title + GetLocalResourceObject("會員通知").ToString() + "</font></td></tr><tr><td><img src='http://www.cocker.com.tw/images/cockermail_bg.jpg'></td></tr>";
                        Mail_cont += "<tr><td><table width='450' border='0' align='center' cellpadding='0' cellspacing='0'><tr><td height='15'></td></tr><tr><td ><font color='#333333' size='3'>" + GetLocalResourceObject("當您收到這封信的同時，表示您已經正式成為會員！現在，您可盡情享用各種優質功能，掌握最即時的資訊及優惠活動喔。").ToString() + "<br><br>" + GetLocalResourceObject("請熟記以下重要訊息").ToString() + "<br>" + GetLocalResourceObject("您的帳號").ToString() + " / ";
                        Mail_cont += this.Email.Text + "<br>" + GetLocalResourceObject("您的密碼").ToString() + " / " + this.Pwd.Text + "<br>";
                        if (CertificationURL == "Y") {
                            Mail_cont += GetLocalResourceObject("開通帳號網址：").ToString() + "<br><a href='" + this.weburl.Value + "/" + GS.GetLanString(this.language.Value) + "/checkm.asp?mem_id=" + MemID + "'>" + this.weburl.Value + "/" + GS.GetLanString(this.language.Value) + "/checkm.asp?mem_id=" + MemID + "</a>";
                        }                                                
                        Mail_cont += "</font></td></tr></table></td></tr><tr><td align='center' height='15'></td></tr>";
                        Mail_cont += "<tr><td align='center'><font color='#d62929' size='2'>" + GetLocalResourceObject("提醒您，客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款").ToString() + "<br>" + GetLocalResourceObject("若有上述情形，請立即撥打165防詐騙專線查詢").ToString() + "</font></td></tr></table>";
                        */
                        send_email(Mail_Cont, GetLocalResourceObject("StringResource26").ToString() + " 【" + Mail_title + "】", Service_mail, this.Email.Text);//呼叫send_email函式測試，寄給會員

                        if (SendMemberMail == "Y") {
                            send_email(Mail_Cont2, GetLocalResourceObject("StringResource26").ToString() + " 【" + Mail_title + "】", Service_mail, Service_mail);//呼叫send_email函式測試，寄給管理者
                        }

                        Response.Write("<script language='javascript'>alert('" + GetLocalResourceObject("StringResource27").ToString() + "'); window.location.href='login.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value) + "&CheckM=" + HttpContext.Current.Server.UrlEncode(this.Checkm.Value) + "';</script>");
                    }
                    else
                    {
                        Response.Write("<script language='javascript'>alert('" + Str_Error + "');</script>");
                    }
                    //this.Label6.Text = "";
                    this.Label6.Visible = false;
                }
                else {
                    //this.Label6.Text = "請輸入正確驗證碼";
                    this.Label6.Visible = true;
                }                
            }
        }       
    }
}