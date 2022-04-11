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

namespace ECSSO
{
    public partial class Member : System.Web.UI.Page
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/member.css");
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
                else if (Request.QueryString["ReturnUrl"] != null)
                {
                    this.returnurl.Value = Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString());
                }
                else
                {
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
                }



                if (Request.Form["MemID"] != null)
                {
                    this.MemberID.Text = Encoder.HtmlEncode(Request.Form["MemID"].ToString());
                }
                else
                {
                    if (Request.QueryString["MemID"] != null)
                    {
                        this.MemberID.Text = Encoder.HtmlEncode(Request.QueryString["MemID"].ToString());
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

                GetStr GS = new GetStr();
                if (GS.MD5Check(this.siteid.Value + this.MemberID.Text, this.CheckM.Value))
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
                        SqlCommand cmd = new SqlCommand("select b.title from CurrentUseFrame as a left join head as b on a.id=b.hid", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            while (reader.Read())
                            {
                                //this.WebTitle.Text = reader[0].ToString();
                                Page.Title = reader[0].ToString();
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
                        SqlCommand cmd = new SqlCommand("select * from cust where mem_id=@mem_id", conn);
                        cmd.Parameters.Add(new SqlParameter("@mem_id", this.MemberID.Text));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.FieldCount > 0)
                            {
                                while (reader.Read())
                                {
                                    this.UserID.Value = Encoder.HtmlEncode(reader["id"].ToString());
                                    this.CHName.Text = HttpUtility.HtmlDecode(Server.HtmlDecode(Encoder.HtmlEncode(reader["ch_name"].ToString())));
                                    this.Sex.SelectedIndex = Convert.ToInt16(reader["sex"].ToString()) - 1;
                                    //this.Tel.Text = Encoder.HtmlEncode(reader["tel"].ToString());
                                    this.Email.Text = Encoder.HtmlEncode(reader["email"].ToString());
                                    this.CellPhone.Text = Encoder.HtmlEncode(reader["cell_phone"].ToString());
                                    this.BirthDay.Text = Encoder.HtmlEncode(reader["birth"].ToString());
                                    this.Address.Text = HttpUtility.HtmlDecode(Server.HtmlDecode(Encoder.HtmlEncode(reader["addr"].ToString())));
                                    this.bonusTotal.Text = Encoder.HtmlEncode(reader["bonus_total"].ToString());
                                    switch (Encoder.HtmlEncode(reader["vip"].ToString()))
                                    {
                                        case "1":
                                            this.VIP.Text = GetLocalResourceObject("StringResource1").ToString();
                                            this.EffectiveDate.Text = "";
                                            break;
                                        case "2":
                                            this.VIP.Text = GetLocalResourceObject("StringResource2").ToString();
                                            //2015.11.03 VIP期限功能暫時關閉
                                            //this.EffectiveDate.Text = "(" + Encoder.HtmlEncode(reader["starttime"].ToString()) + "~" + Encoder.HtmlEncode(reader["endtime"].ToString()) + ")";
                                            break;
                                    }
                                }
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
                    Response.Write("<center>" + GetLocalResourceObject("StringResource3").ToString() + "</center>");
                    LinkButton1.Visible = false;
                }
            }
            else
            {
                //Page.Title = this.WebTitle.Text;
            }
        }

        public Boolean isright(string s, String right) //定義正則表達式函數
        {
            Regex Regex1 = new Regex(right, RegexOptions.IgnoreCase);
            return Regex1.IsMatch(s);
        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            GetStr getstr = new GetStr();
            if (this.CHName.Text == "")
            {
                this.CheckName.Text = GetLocalResourceObject("StringResource4").ToString();
            }
            else
            {
                this.CheckName.Text = "";
            }
            String RegStr = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
            if (!isright(this.Email.Text, RegStr))
            {
                this.CheckEmail.Text = GetLocalResourceObject("StringResource5").ToString();
            }
            else
            {
                this.CheckEmail.Text = "";
            }
            if (this.BirthDay.Text != "")
            {
                DateTime dt = Convert.ToDateTime("1911-01-01");
                RegStr = @"\d{4}\-\d{2}\-\d{2}";
                if (!DateTime.TryParse(this.BirthDay.Text, out dt) || !isright(this.BirthDay.Text, RegStr))
                {
                    this.CheckBirthDay.Text = GetLocalResourceObject("StringResource6").ToString();
                }
                else
                {
                    this.CheckBirthDay.Text = "";
                }
            }

            /*RegStr = @"\d{2,3}\-\d{6,8}";
            if (!isright(this.Tel.Text, RegStr))
            {
                this.CheckTel.Text = GetLocalResourceObject("StringResource7").ToString();
            }
            else
            {
                this.CheckTel.Text = "";
            }*/
            RegStr = @"09\d{2}\-\d{6}";
            if (!isright(this.CellPhone.Text, RegStr))
            {
                this.CheckCellPhone.Text = GetLocalResourceObject("StringResource8").ToString();
            }
            else
            {
                this.CheckCellPhone.Text = "";
            }
            if (this.Address.Text == "")
            {
                this.CheckAddress.Text = GetLocalResourceObject("StringResource9").ToString();
            }
            else
            {
                this.CheckAddress.Text = "";
            }

            if (CheckBox1.Checked)
            {
                RegStr = @"^[0-9a-zA-Z_]{4,20}$";
                if (!isright(this.NewPwd.Text, RegStr))
                {
                    Label1.Text = GetLocalResourceObject("StringResource10").ToString();
                }
                else
                {
                    if (this.NewPwd.Text != this.ChkNewPwd.Text)
                    {
                        Label1.Text = GetLocalResourceObject("StringResource11").ToString();
                    }
                    else
                    {
                        Label1.Text = "";
                    }
                }
            }

            if (this.CheckName.Text == "" && this.CheckBirthDay.Text == "" && this.CheckEmail.Text == "" && this.CheckCellPhone.Text == "" && this.CheckAddress.Text == "" && this.Label1.Text == "")
            {
                String str_ChName = getstr.ReplaceStr(this.CHName.Text);
                String str_SEX = getstr.ReplaceStr(this.Sex.SelectedItem.Value);
                String str_Birth = getstr.ReplaceStr(this.BirthDay.Text);
                String str_Email = getstr.ReplaceStr(this.Email.Text);
                //String str_Tel = getstr.ReplaceStr(this.Tel.Text);
                String str_Tel = getstr.ReplaceStr("");
                String str_CellPhone = getstr.ReplaceStr(this.CellPhone.Text);
                String str_Addr = getstr.ReplaceStr(this.Address.Text);

                String setting = getstr.GetSetting(this.siteid.Value);

                if (CheckBox1.Checked)
                {
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = "sp_ResetPassword";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;
                        cmd.Parameters.Add(new SqlParameter("@id", this.UserID.Value));
                        cmd.Parameters.Add(new SqlParameter("@pwd", this.NewPwd.Text));
                        cmd.ExecuteNonQuery();
                        Label1.Text = "";
                    }
                }
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_EditMember2";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@ch_name", str_ChName));
                    cmd.Parameters.Add(new SqlParameter("@sex", str_SEX));
                    cmd.Parameters.Add(new SqlParameter("@ident", ""));
                    if (str_Birth != "")
                    {
                        cmd.Parameters.Add(new SqlParameter("@birth", Convert.ToDateTime(str_Birth).ToString("yyyy-MM-dd")));
                    }
                    else
                    {
                        cmd.Parameters.Add(new SqlParameter("@birth", ""));
                    }
                    cmd.Parameters.Add(new SqlParameter("@tel", str_Tel));
                    cmd.Parameters.Add(new SqlParameter("@cell_phone", str_CellPhone));
                    cmd.Parameters.Add(new SqlParameter("@email", str_Email));
                    cmd.Parameters.Add(new SqlParameter("@addr", str_Addr));
                    cmd.Parameters.Add(new SqlParameter("@id", this.MemberID.Text));
                    cmd.ExecuteNonQuery();
                }

                String Service_mail = "";
                String Mail_title = "";
                //Search Service Data
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    String Str_sql = "select b.service_mail,b.title from CurrentUseFrame as a left join head as b on a.id=b.hid";
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
                            }
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
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_userlogAdd";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@id", this.MemberID.Text));
                    cmd.Parameters.Add(new SqlParameter("@prog_name", "會員專區"));
                    cmd.Parameters.Add(new SqlParameter("@job_name", "修改"));
                    cmd.Parameters.Add(new SqlParameter("@title", ""));
                    cmd.Parameters.Add(new SqlParameter("@table_id", GetClientIP()));
                    cmd.Parameters.Add(new SqlParameter("@detail", ""));
                    cmd.Parameters.Add(new SqlParameter("@ip", GetClientIP()));
                    cmd.Parameters.Add(new SqlParameter("@filename", "member.aspx"));
                    cmd.ExecuteNonQuery();
                }

                String Mail_Cont = "";

                Mail_Cont += "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />";
                Mail_Cont += "<div style='font-family:微軟正黑體, Arial, 新細明體, Helvetica, sans-serif'>";
                Mail_Cont += "    <font size='4' color='#ff0000'><b>" + GetLocalResourceObject("StringResource12") + str_ChName + "(" + GetLocalResourceObject("StringResource13") + "：" + this.MemberID.Text + ")" + GetLocalResourceObject("StringResource14") + "</b></font><br>";
                Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px; margin-top:8px; margin-bottom:20px;'>";
                Mail_Cont += GetLocalResourceObject("StringResource15") + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + GetLocalResourceObject("StringResource16") + "<br>";
                Mail_Cont += GetLocalResourceObject("StringResource17") + "<br>";
                Mail_Cont += GetLocalResourceObject("StringResource18") + "<br>";
                Mail_Cont += "    <br>";
                Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px'>";
                Mail_Cont += "    <span style='color:#f00;'>" + GetLocalResourceObject("StringResource19").ToString() + "</span>";
                Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";
                Mail_Cont += "</div>";

                send_email(Mail_Cont, " 【" + Mail_title + "】" + GetLocalResourceObject("StringResource20").ToString(), Service_mail, str_Email);//呼叫send_email函式測試


                Response.Write("<script type='text/javascript'>alert('" + GetLocalResourceObject("StringResource21").ToString() + "');</script>");
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

        public void send_email(string msg, string mysubject, string sender, string mail)
        {
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            message.From = new MailAddress(sender, GetLocalResourceObject("StringResource22").ToString());
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

        #region 取得IP
        public String GetClientIP()
        {
            //判所client端是否有設定代理伺服器
            if (Request.ServerVariables["HTTP_VIA"] == null)
                return Request.ServerVariables["REMOTE_ADDR"].ToString();
            else
                return Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
        }
        #endregion
    }
}