using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace ECSSO.Library.EmailCont
{
    public class SmtpServer
    {
        public string email { get; set; }
        public string pwd { get; set; }
        public int port { get; set; }
        public int daySendMailCount { get; set; }
        public int sendMemberCount { get; set; }
        public string server { get; set; }
        public SmtpServer() { }
        public SmtpServer(string seting) {
            using (SqlConnection conn = new SqlConnection(seting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select top 1 * from smtpServer
                ", conn);
                try {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        this.email = reader["email"].ToString();
                        this.pwd = reader["pwd"].ToString();
                        this.port = int.Parse(reader["port"].ToString());
                        this.daySendMailCount = int.Parse(reader["daySendMailCount"].ToString());
                        this.sendMemberCount = int.Parse(reader["sendMemberCount"].ToString());
                        this.server = reader["server"].ToString();
                    }
                    else {
                        this.server = ConfigurationManager.AppSettings.Get("smtpServer");
                        this.port = Int32.Parse(ConfigurationManager.AppSettings.Get("smtpPort"));
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public void send(EmailCont cont, string mysubject, string sender, string mail)
        {
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            if (string.IsNullOrEmpty(this.email)) message.From = new MailAddress(sender, "客服中心");
            else message.From = new MailAddress(this.email, "客服中心"); 
            message.Bcc.Add(sender);
            message.To.Add(mail);

            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;//E-mail編碼
            message.Subject = mysubject;//E-mail主旨
            message.Body = $@"
                {cont.introduction}<br /><br />
                {cont.signature}
            ";//E-mail內容
            //設定E-mail Server和port
            SmtpClient smtpClient = new SmtpClient(this.server, this.port);
            if (!string.IsNullOrEmpty(this.email))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new System.Net.NetworkCredential(this.email, this.pwd);
            }
            try
            {
                smtpClient.Send(message);
            }
            catch (SmtpException e)
            {
                switch (e.StatusCode.ToString())
                {
                    case "MailboxNameNotAllowed":
                        throw new Exception("信箱錯誤:" + e.Message);
                    default:
                        throw new Exception("發送錯誤:" + e.Message);
                }
            }
        }
    }
}