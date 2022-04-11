using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Microsoft.Security.Application;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Threading;
using System.Globalization;

namespace ECSSO
{
    public partial class ResetPassword : System.Web.UI.Page
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/ResetPassword.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        String connSetting;
        private Dictionary<string, string> Member = new Dictionary<string, string>();
        protected void Page_Load(object sender, EventArgs e)
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

            //String SiteID = Request.Params["SiteID"];
            //ConnSetting DBSetting = new ConnSetting();
            //connSetting = DBSetting.GetByID(SiteID);  //取得網站連線設定
            //Response.Write(GetValid("000034"));
            GetStr GS = new GetStr();
            String ValidCode = Request.Params["valid"];
            if (GS.MD5Check(this.siteid.Value, CheckM))
            {
                if (string.IsNullOrEmpty(ValidCode))
                {
                    SendRestMailProcess();
                    Response.End();
                }
            }
            String Site = Microsoft.Security.Application.Encoder.HtmlEncode(Request.Params["site"]);
            String mem_id = Microsoft.Security.Application.Encoder.HtmlEncode(Request.Params["mem"]);

            ConnSetting DBSetting = new ConnSetting();
            connSetting = DBSetting.GetByOrg(Site);  //取得網站連線設定
            if (string.IsNullOrEmpty(connSetting)) { Response.End(); return; }      //網站連線設定空白


            string sqlcmd = "select * from cust where mem_id = @mem_id";
            SqlConnection conn = new SqlConnection(connSetting);
            SqlCommand cmd = new SqlCommand(sqlcmd, conn);
            SqlDataReader reader = null;
            //cmd = new SqlCommand(sqlcmd, conn);
            if (mem_id == Microsoft.Security.Application.Encoder.HtmlEncode(Request.Params["mem"]))
            {
                cmd.Parameters.Add(new SqlParameter("@mem_id", mem_id));

                Member.Clear();
                try
                {
                    conn.Open();
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        string[] ColList = { "mem_id", "id", "ch_name", "email", "validborn" };
                        foreach (string Column in ColList) { Member.Add(Column, reader[Column].ToString()); }
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    reader.Close();
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
            }
            else
            {
                Member.Clear();
            }

            if (Member.Count < 1) { Response.End(); return; }     //會員資料取得錯誤
            String id = Member["id"];

            DateTime ValidBorn = DateTime.Parse(Member["validborn"]);   //是否超過一天(24hr)
            if (new TimeSpan(DateTime.Now.Ticks - ValidBorn.Ticks).Days != 0) { Response.End(); return; }

            if (ValidCode != GetValid(Member["mem_id"])) { Response.End(); return; }    //檢查驗證碼是否一致

            //頁面資訊
            cmd.CommandText = "select b.* from CurrentUseFrame as a left join head as b on a.id=b.hid";
            reader = null;
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    this.WebTitle.Text = Microsoft.Security.Application.Encoder.HtmlEncode(reader["title"].ToString());
                    Page.Title = Microsoft.Security.Application.Encoder.HtmlEncode(reader["title"].ToString());
                }
            }
            catch (Exception ex) { throw (ex); }
            finally { if (conn.State == ConnectionState.Open) conn.Close(); }


        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            if (this.Email.Text == "")
            { this.CheckEmail.Visible = true; return; }
            { this.CheckEmail.Visible = false; }

            if (this.Email.Text != Member["email"])
            { this.CheckEmail.Text = GetLocalResourceObject("StringResource5").ToString(); this.CheckEmail.Visible = true; return; }
            { this.CheckEmail.Visible = false; }

            if (this.Password.Text == "")
            { this.CheckPassword.Visible = true; return; }
            { this.CheckPassword.Visible = false; }

            if (this.PasswordAgain.Text == "")
            { this.CheckPasswordAgain.Visible = true; return; }
            { this.CheckPasswordAgain.Visible = false; }

            if (Session["CheckCode"] != null && String.Compare(Session["CheckCode"].ToString(), this.TextBox1.Text, true) != 0)
            { this.Label8.Visible = true; return; }
            { this.Label8.Visible = false; }

            PasswordSetting();
        }

        //密碼設定
        private void PasswordSetting()
        {
            using (SqlConnection conn = new SqlConnection(connSetting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_ResetPassword", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (this.Password.Text != "")
                {
                    cmd.Parameters.Add(new SqlParameter("@id", Member["id"]));
                    cmd.Parameters.Add(new SqlParameter("@pwd", this.Password.Text));
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }

            Response.Write(@"<div class=""alert alert-success"">
                <a href=""#"" class=""close"" data-dismiss=""alert"">&times;</a>
                " + GetLocalResourceObject("StringResource19") + "</div>");

            this.language.Value = Request.Params["language"];
            this.returnurl.Value = "http://" + this.returnurl.Value;
            this.weburl.Value = this.returnurl.Value;

            CreateValid(Member["mem_id"]);  //建立新的驗證碼, 使原網址失效

            String SiteID = "";

            using (SqlConnection conn = new SqlConnection(connSetting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select b.id from CurrentUseFrame as a left join head as b on a.id=b.hid", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        SiteID = reader["id"].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            String web_url = "Login.aspx?language=" + this.language.Value.Replace(",", "") + "&SiteID=" + SiteID + "&ReturnUrl=" + this.returnurl.Value + "&Url=" + this.returnurl.Value + "&CheckM=" + Request.Params["CheckM"];

            Response.Write("<script language='javascript' type='text/javascript'>alert('" + GetLocalResourceObject("StringResource18") + "');window.location.href='" + web_url + "';</script>");
        }

        private void SendRestMailProcess()
        {
            //Parameters from ForgetPassword
            //language:zh-tw
            //SiteID:504
            //ReturnUrl:http://derek.ezsale.tw/tw/index.asp?au_id=71&sub_id=23
            //Url:http://derek.ezsale.tw
            //mail: Email.Text
            String status = GetLocalResourceObject("StringResource3").ToString();
            String SiteID = Request.Params["SiteID"];
            String eMail = Request.Params["mail"];
            String CheckM = Request.Params["CheckM"];
            String returnurl = Request.Params["ReturnUrl"].Replace("http://", "").Replace("https://", "").Split('/')[0];
            String language = Request.Params["language"];

            if (string.IsNullOrEmpty(SiteID))    //網站ID或email空白
            { Response.Write(GetLocalResourceObject("StringResource4")); return; }

            if (string.IsNullOrEmpty(eMail))    //網站ID或email空白
            { Response.Write(GetLocalResourceObject("StringResource5")); return; }


            ConnSetting DBSetting = new ConnSetting();
            connSetting = DBSetting.GetByID(SiteID);  //取得網站連線設定
            if (string.IsNullOrEmpty(connSetting)) { Response.Write(GetLocalResourceObject("StringResource6")); return; }      //網站連線設定空白

            MemberCheck(eMail);
            //Member list:  "mem_id", "id", "ch_name", "ident", "sex", "birth", "addr", "email"
            if (Member.Count < 1) { Response.Write(GetLocalResourceObject("StringResource7")); return; }     //會員資料取得錯誤

            string mem_id = Member["mem_id"];
            string site = GetOrgName(SiteID);
            string valid = CreateValid(mem_id);
            string valid_url = @Request.Url.GetLeftPart(UriPartial.Path) +
                                "?language=" + language +
                                "&site=" + site +
                                "&mem=" + mem_id +
                                "&valid=" + valid +
                                "&CheckM=" + CheckM +
                                "&ReturnUrl=" + returnurl;

            SendResetMail(Member["id"].ToString(), Member["email"].ToString(), valid_url);
            return;
        }

        //回傳驗證碼
        private string GetValid(string mem_id)
        {
            string sqlcmd = "select sys.fn_VarBinToHexStr(hashbytes('MD5',(id+','+convert(nvarchar,validborn)+','+convert(nvarchar,datediff(HH,validborn,GETDATE())/24)))) as valid from cust where mem_id=@mem_id";
            SqlDataReader reader = null;
            string valid = "";

            SqlConnection conn = new SqlConnection(connSetting);
            SqlCommand cmd = new SqlCommand(sqlcmd, conn);
            int u = 0;
            if (int.TryParse(mem_id, out u))
            {
                cmd.Parameters.Add(new SqlParameter("@mem_id", mem_id));
                try
                {
                    conn.Open();
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        valid = reader["valid"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    reader.Close();
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
            }

            return valid;
        }

        //產生驗證碼並回傳
        private string CreateValid(string mem_id)
        {
            String sqlcmd = "sp_CreateValid";
            SqlConnection conn = new SqlConnection(connSetting);
            SqlCommand cmd = new SqlCommand(sqlcmd, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            int u = 0;
            if (int.TryParse(mem_id, out u))
            {
                cmd.Parameters.Add(new SqlParameter("@mem_id", mem_id));
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
            }

            return GetValid(mem_id);
        }

        //確認是否有此mail會員, 回傳該會員資料
        private void MemberCheck(string email)
        {
            string mssqlstr = "sp_CheckMember";

            SqlConnection conn = new SqlConnection(connSetting);
            SqlCommand cmd = new SqlCommand(mssqlstr, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            if (email != "")
            {
                cmd.Parameters.Add(new SqlParameter("@mem_id", email));
                SqlDataReader reader = null;
                Member.Clear();
                try
                {
                    conn.Open();
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        string[] ColList = { "mem_id", "id", "ch_name", "email" };
                        foreach (string Column in ColList) { Member.Add(Column, reader[Column].ToString()); }
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    reader.Close();
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
            }
            else
            {
                Member.Clear();
            }
        }

        //建構email內容
        private void SendResetMail(string id, string mail, string url)
        {
            String ServiceMail = "";
            String ServiceTitle = "";
            using (SqlConnection conn = new SqlConnection(connSetting))
            {
                conn.Open();
                String Str_sql = "select b.service_mail,b.title from CurrentUseFrame as a left join head as b on a.id=b.hid";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        ServiceMail = reader[0].ToString();
                        ServiceTitle = reader[1].ToString();
                    }
                }
                finally { reader.Close(); }
            }

            String CustID = id;
            String CustMail = mail;

            if (ServiceMail == "") { ServiceMail = "service@ether.com.tw"; }

            StringBuilder Mail_cont = new StringBuilder();
            Mail_cont.Append("<table width='576' cellpadding='0' cellspacing='0' align='center'>");
            Mail_cont.Append("  <tr>");
            Mail_cont.Append("    <td><font color='#333333' size='3'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;【");
            Mail_cont.Append(ServiceTitle);
            Mail_cont.Append("】 " + GetLocalResourceObject("StringResource14") + "</font></td>");
            Mail_cont.Append("  </tr>");
            Mail_cont.Append("  <tr>");
            Mail_cont.Append("    <td><table width='450' border='0' align='center' cellpadding='0' cellspacing='0'><tr><td height='15'></td></tr><tr><td ><font color='#333333' size='3'>" + GetLocalResourceObject("StringResource8") + "<br>" + GetLocalResourceObject("StringResource9") + "：");
            Mail_cont.Append(CustID);
            Mail_cont.Append("<br>" + GetLocalResourceObject("StringResource10") + "：");
            Mail_cont.Append(url);
            Mail_cont.Append("</font><br>" + GetLocalResourceObject("StringResource11") + "</td></tr></table></td>");
            Mail_cont.Append("  </tr>");
            Mail_cont.Append("  <tr>");
            Mail_cont.Append("    <td align='center' height='15'></td>");
            Mail_cont.Append("  </tr>");
            Mail_cont.Append("  <tr>");
            Mail_cont.Append("    <td align='center'><font color='#d62929' size='2'>" + GetLocalResourceObject("StringResource12") + "<br>" + GetLocalResourceObject("StringResource13") + "</font></td>");
            Mail_cont.Append("  </tr>");
            Mail_cont.Append("</table>");

            //send_email(Mail_cont, "忘記密碼通知 【" + ServiceTitle + "】", "service@ether.com.tw", "finn@ether.com.tw");
            //ECSSO.ForgetPassword obj = new ECSSO.ForgetPassword();
            //obj.send_email("test", "title", "service@ether.com.tw", "finn@ether.com.tw");
            send_email(Mail_cont, GetLocalResourceObject("StringResource14") + " 【" + ServiceTitle + "】", ServiceMail, CustMail);
        }

        //發送email
        private Boolean send_email(StringBuilder msg, string mysubject, string sender, string mail)
        {
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            message.From = new MailAddress(
                (ConfigurationManager.AppSettings.Get("CredentialUser") == ""? sender: ConfigurationManager.AppSettings.Get("CredentialUser")),  
                GetLocalResourceObject("StringResource15").ToString());
            message.Bcc.Add(sender);
            message.To.Add(mail);

            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;//E-mail編碼
            message.Subject = mysubject;//E-mail主旨
            message.Body = msg.ToString();//E-mail內容

            //SmtpClient smtpClient = new SmtpClient("msa.hinet.net");//設定E-mail Server和port
            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings.Get("smtpServer"), Int32.Parse(ConfigurationManager.AppSettings.Get("smtpPort")));//設定E-mail Server和port
            if (ConfigurationManager.AppSettings.Get("CredentialUser") != "")
            {
                smtpClient.Credentials = new System.Net.NetworkCredential(
                        ConfigurationManager.AppSettings.Get("CredentialUser"),
                        ConfigurationManager.AppSettings.Get("CredentialPW")
                );
                smtpClient.EnableSsl = true;
            }
            Boolean Success = false;
            try { smtpClient.Send(message); Response.Write(GetLocalResourceObject("StringResource16").ToString() + Microsoft.Security.Application.Encoder.HtmlEncode(mail)); Success = true; }
            catch { Response.Write(GetLocalResourceObject("StringResource17").ToString()); Success = false; }   //密碼發送失敗！
            return Success;
        }

        //輸入SiteID, return orgName
        private string GetOrgName(string SiteID)
        {
            string OrgName = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select crm_org from cocker_cust where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", SiteID));

                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        OrgName = reader["crm_org"].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            return OrgName;
        }
    }
}