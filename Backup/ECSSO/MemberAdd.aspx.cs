using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
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

namespace ECSSO
{
    public partial class MemberAdd : System.Web.UI.Page
    {
        string str_language = string.Empty;
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
                    SqlCommand cmd = new SqlCommand("select title,member_agree from head", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {                            
                            this.WebTitle.Text = reader[0].ToString();
                            str_agree.InnerHtml = HttpUtility.HtmlDecode(Server.HtmlDecode(reader[1].ToString()));
                            Page.Title = reader[0].ToString();
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            else {
                Page.Title = this.WebTitle.Text;
            }
        }
       
        public void send_email(string msg, string mysubject, string sender, string mail)
        {
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            message.From = new MailAddress(sender, "客服中心");
            //message.Bcc.Add(sender);
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

        public Boolean isright(string s, String right) //定義正則表達式函數
       {
            Regex Regex1 = new Regex(right, RegexOptions.IgnoreCase);
            return Regex1.IsMatch(s);
        }

        protected void LinkButton2_Click(object sender, EventArgs e)
        {
            Response.Redirect("login.aspx?language=" + this.language.Value + "&SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value));
        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {            
            if (!this.CheckBox1.Checked)
            {
                this.CheckService.Text = "您尚未同意服務條款";
                this.CheckService.Visible = true;
            }
            else {
                this.CheckService.Text = "";
                this.CheckService.Visible = false;
            }
            String RegStr = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
            if (!isright(this.Email.Text, RegStr))
            {
                this.CheckEmail.Text = "Email格式錯誤";
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
                this.CheckPwd.Text = "密碼長度請輸入4至10個英文或數字";
                this.CheckPwd.Visible = true;
            }
            else
            {
                this.CheckPwd.Text = "";
                this.CheckPwd.Visible = false;
            }
            if (this.ChkPwd.Text != this.Pwd.Text)
            {
                this.CheckPwd2.Text = "密碼與確認密碼不同";
                this.CheckPwd2.Visible = true;
            }
            else
            {
                this.CheckPwd2.Text = "";
                this.CheckPwd2.Visible = false;
            }
            if (this.Name.Text == "")
            {
                this.CheckName.Text = "請輸入您的名字";
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
                this.CheckBirthDay.Text = "請輸入正確日期";
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
                this.CheckTel.Text = "聯絡電話格式錯誤";
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
                this.CheckCellPhone.Text = "行動電話格式錯誤";
                this.CheckCellPhone.Visible = true;
            }
            else
            {
                this.CheckCellPhone.Text = "";
                this.CheckCellPhone.Visible = false;
            }
            if (this.address.Text == "")
            {
                this.Checkaddress.Text = "請輸入地址";
                this.Checkaddress.Visible = true;
            }
            else
            {
                this.Checkaddress.Text = "";
                this.Checkaddress.Visible = false;
            }
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
                                Str_Error = "帳號重複";
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
                            cmd.ExecuteNonQuery();
                        }

                        String Service_mail = "";
                        String Mail_title = "";
                        String CrmVersion = "";

                        //Search Service Data
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            String Str_sql = "select service_mail,title,crm_version from head";
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
                                    }
                                }
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }

                        //CRM
                        if (CrmVersion == "Y")
                        {
                            //String CrmID = "sysuser@" + OrgName + ".hisales.hinet.net";
                            //String CrmPwd = "!SysUser";

                            //EbgCrmWebServiceApi CrmService = Session["MyService"] as EbgCrmWebServiceApi;
                            //if (CrmService == null)
                            //{
                            //    CrmService = new EbgCrmWebServiceApi();            // create the proxy
                            //    CrmService.CookieContainer = new CookieContainer();// create a container for the SessionID cookie
                            //    Session["MyService"] = CrmService;            // store it in Session for next usage
                            //}

                            ////身分驗證
                            //Result LoginResult = CrmService.CrmAuthentication(CrmID, CrmPwd, OrgName);

                            //string xmlString = string.Empty;
                            //xmlString += "<record>";
                            //xmlString += "<firstname>" + ch_name + "</firstname>";
                            //xmlString += "<mobilephone>" + cell_phone + "</mobilephone>";
                            //xmlString += "<telephone1>" + tel + "</telephone1>";
                            //xmlString += "<address1_name>" + addr + "</address1_name>";
                            //xmlString += "<emailaddress1>" + email + "</emailaddress1>";
                            //xmlString += "<gendercode>" + sex + "</gendercode>";
                            //xmlString += "<new_ecaccount>" + Cust_id + "</new_ecaccount>";
                            //xmlString += "<new_vipcode>" + mem_id + "</new_vipcode>";
                            //xmlString += "<birthdate>" + birth + "</birthdate>";
                            //xmlString += "</record>";

                            //XmlDocument XmlDoc = new XmlDocument();
                            //XmlDoc.LoadXml(xmlString);

                            ////新增單筆
                            //Result ExecResult = CrmService.CreateRecord("contact", XmlDoc.FirstChild);

                            //if (ExecResult.success)
                            //{
                            //    str_sql = "update cust set contact_id='" + ExecResult.descriptions[0].Replace("{", "").Replace("}", "") + "' where mem_id='" + mem_id + "'";
                            //    conn.UpdateCommand = str_sql;
                            //    conn.Update();
                            //    //Response.Write("success");
                            //}
                            //else
                            //{
                            //    Response.Write("CRM同步失敗，原因：" + ExecResult.descriptions[0] + ExecResult.descriptions[1]);
                            //    for (int i = 0; i < ExecResult.descriptions.Length; i++)
                            //    {
                            //        Response.Write(ExecResult.descriptions[i]);
                            //    }
                            //}
                        }

                        //Send Mail
                        String Mail_cont = "<center><span style='color:red;'>提醒您：此封『會員通知』為系統發出，請勿直接回覆。</span></center><br>";
                        Mail_cont += "<table width='576' cellpadding='0' cellspacing='0' align='center'>";
                        Mail_cont += "<tr><td><font color='#333333' size='3'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                        Mail_cont += Mail_title + "會員通知</font></td></tr><tr><td><img src='http://www.cocker.com.tw/images/cockermail_bg.jpg'></td></tr>";
                        Mail_cont += "<tr><td><table width='450' border='0' align='center' cellpadding='0' cellspacing='0'><tr><td height='15'></td></tr><tr><td ><font color='#333333' size='3'>當您收到這封信的同時，表示您已經正式成為會員！現在，您可盡情享用各種優質功能，掌握最即時的資訊及優惠活動喔。<br><br>請熟記以下重要訊息<br>您的帳號 / ";
                        Mail_cont += this.Email.Text + "<br>您的密碼 / " + this.Pwd.Text + "<br>";
                        Mail_cont += "開通帳號網址：<br><a href='" + this.weburl.Value + "/tw/checkm.asp?mem_id=" + MemID + "'>" + this.weburl.Value + "/tw/checkm.asp?mem_id=" + MemID + "</a>";
                        Mail_cont += "</font></td></tr></table></td></tr><tr><td align='center' height='15'></td></tr>";
                        Mail_cont += "<tr><td align='center'><font color='#d62929' size='2'>提醒您，客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款<br>若有上述情形，請立即撥打165防詐騙專線查詢</font></td></tr></table>";

                        send_email(Mail_cont, "加入會員通知 【" + Mail_title + "】", Service_mail, this.Email.Text);//呼叫send_email函式測試
                        Response.Write("<script language='javascript'>alert('系統將立即寄發『加入會員通知』信函至您所登錄之E-Mail中，您必須完成帳號開通後，才能登入網站與使用會員功能，此信函中包含您所設定之登錄帳號(即E-mail)、密碼以及開通會員帳號之連結。請按下信函中的『開通帳號請按此』連結，會自動帶到開通會員帳號的頁面，完成會員開通。'); window.location.href='login.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(this.returnurl.Value) + "&Url=" + HttpContext.Current.Server.UrlEncode(this.weburl.Value) + "';</script>");
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