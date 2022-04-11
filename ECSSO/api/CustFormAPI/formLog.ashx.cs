using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Web;

namespace ECSSO.api.CustFormAPI
{
    /// <summary>
    /// formLog 的摘要描述
    /// </summary>
    public class formLog : IHttpHandler
    {
        HttpContext context = null;
        int formLogID;
        string setting, code, message,talken,email, ManagerID,reply,siteName;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            this.talken = context.Request.Form["token"];
            GetStr GS = new GetStr();

            try
            {
                if (!string.IsNullOrEmpty(context.Request.Form["id"]))
                {
                    if (context.Request.Form["token"] != null)
                    {
                        this.setting = GS.checkToken(talken);
                        if (this.setting.IndexOf("error") < 0)
                        {
                            this.context = context;
                            if (context.Request.Form["id"] != null)
                            {
                                formLogID = int.Parse(context.Request.Form["id"].ToString());
                                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                                {
                                    conn.Open();
                                    SqlCommand cmd = new SqlCommand(@"select talken.ManagerID,IDManagement.email 
                                                    from talken
                                                    left join IDManagement on talken.ManagerID = IDManagement.Manager_ID and 
                                                              talken.orgName = IDManagement.orgName
                                                    where talken.talken = @talken", conn);
                                    cmd.Parameters.Add(new SqlParameter("@talken", talken));
                                    SqlDataReader reader = cmd.ExecuteReader();
                                    if (reader.Read())
                                    {
                                        email = reader["email"].ToString();
                                        ManagerID = reader["ManagerID"].ToString();
                                        if (email != "")
                                        {
                                            setSiteName();
                                            sendReady();
                                        }
                                        else {
                                            code = "404";
                                            message = "您的帳號未輸入信箱資訊，請輸入後再重新操作";
                                        }
                                    }
                                    else {
                                        code = "404";
                                        message = "無此權限";
                                    }
                                }
                            }
                        }
                        else
                        {
                            code = "401";
                            message = "Token已過期";
                        }
                    }
                    else
                    {
                        code = "401";
                        message = "系統已登出，請重新登入!";
                    }
                }
                else
                {
                    code = "404";
                    message = "操作不存在";
                }
            }
            catch (Exception ex)
            {
                code = "500";
                message = ex.Message;
            }
            finally {
                ResponseWriteEnd("{\"code\":\""+ code + "\",\"message\":\""+ message + "\"}");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #region 設定網站名稱
        private void setSiteName() {
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select top 1 title from head", conn);
                cmd.Parameters.Add(new SqlParameter("@id", formLogID));
                SqlDataReader reader = null;
                try {
                    reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        siteName = reader[0].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        #endregion
        #region 信件發送
        private void sendReady() {
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select * from Contact where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", formLogID));
                SqlDataReader reader=null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        send_email(HttpUtility.HtmlDecode(reader["reply"].ToString()), "【" + siteName + "】客服信回覆", email,reader["email"].ToString());
                    }
                    else {
                        code = "404";
                        message = "紀錄不存在";
                    }
                }
                catch {
                    code = "404";
                    message = "紀錄不存在";
                }
                finally {
                    reader.Close();
                }
            }
        }
        private void send_email(string msg, string mysubject, string sender, string mail)
        {
            string[] senderList = sender.Split(',');
            string CredentialUser = ConfigurationManager.AppSettings.Get("CredentialUser");
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            if (CredentialUser != "") message.From = new MailAddress(CredentialUser, "客服中心");
            else message.From = new MailAddress(sender, "客服中心");
            message.Bcc.Add(sender);
            message.To.Add(mail);

            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;//E-mail編碼
            message.Subject = mysubject;//E-mail主旨
            message.Body = msg;//E-mail內容
            //設定E-mail Server和port
            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings.Get("smtpServer"), Int32.Parse(ConfigurationManager.AppSettings.Get("smtpPort")));
            if (CredentialUser != "")
            {
                smtpClient.Credentials = new System.Net.NetworkCredential(
                        ConfigurationManager.AppSettings.Get("CredentialUser"),
                        ConfigurationManager.AppSettings.Get("CredentialPW")
                );
                smtpClient.EnableSsl = true;
            }
            try
            {
                smtpClient.Send(message);
                code = "200";
                this.message = "信件發送成功";
            }
            catch (SmtpException e)
            {
                switch (e.StatusCode.ToString()) {
                    case "MailboxNameNotAllowed":
                        this.code = "401";
                        this.message = "信箱錯誤:" + e.Message;
                        break;
                    default:
                        this.code = "401";
                        this.message = "發送錯誤:" + e.Message;
                        break;
                }
            }

        }
        #endregion
        private void ResponseWriteEnd(string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }
    }
}