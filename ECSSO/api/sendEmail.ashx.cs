using System;
using System.Collections.Generic;
using System.Web;
using ECSSO.Library;
using ECSSO;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Threading;

namespace ECSSO.api
{
    /// <summary>
    /// sendEmail 的摘要描述
    /// </summary>
    public class sendEmail : IHttpHandler
    {
        private HttpContext context;
        private String setting, SiteID, Type, scheduleID, email, pwd, server;
        private int daySendMailCount, sendMemberCount, port;
        private static String[] typeList = { "active", "news" };
        private class Sender
        {
            public string msg { get; set; }
            public string mysubject { get; set; }
            public string sender { get; set; }
            public string senderName { get; set; }
            public List<Subscriber> mail { get; set; }
        }
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            if (context.Request.Params["Type"] == null) ResponseWriteEnd("error", "401", "Type", "is null");
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd("error", "401", "CheckM", "is null");
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd("error", "401", "SiteID", "is null");
            if (context.Request.Params["scheduleID"] == null) ResponseWriteEnd("error", "401", "scheduleID", "is null");

            String ChkM = context.Request.Params["CheckM"].ToString();
            GetStr GS = new GetStr();
            Type = context.Request.Params["Type"].ToString();
            scheduleID = context.Request.Params["scheduleID"].ToString();
            SiteID = context.Request.Params["SiteID"].ToString();
            setting = GS.GetSetting(SiteID);
            context.Response.Write("in");
            if (!GS.MD5Check(Type + SiteID + GS.GetOrgName(setting), ChkM)) ResponseWriteEnd("error", "402", "CheckM", "isn't match");
            else
            {
                switch (Type)
                {
                    case "1":
                        send(getSubscriber(), getActiveContent());
                        break;
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #region 取得活動寄件內容
        private Mail getActiveContent()
        {
            Mail mail = new Mail();
            string web_url = "", title = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select web_url from cocker_cust where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", SiteID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            web_url = reader["web_url"].ToString().IndexOf("http") >= 0 ? reader["web_url"].ToString() : "http://" + reader["web_url"].ToString();
                        }
                    }
                }
                catch (Exception even)
                {
                    ResponseWriteEnd("error", "500", "", even.Message);
                }
            }

            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select top 1 title,service_mail from head", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            title = reader["title"].ToString();
                            mail.sender = reader["service_mail"].ToString();
                        }
                    }
                }
                catch (Exception even)
                {
                    ResponseWriteEnd("error", "500", "", even.Message);
                }
            }
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select top 1 * from smtpServer", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            email = reader["email"].ToString();
                            pwd = reader["pwd"].ToString();
                            port = Int32.Parse(reader["port"].ToString());
                            daySendMailCount = Int32.Parse(reader["daySendMailCount"].ToString());
                            sendMemberCount = Int32.Parse(reader["sendMemberCount"].ToString());
                            server = reader["server"].ToString();
                        }
                    }
                    else
                    {
                        ResponseWriteEnd("error", "401", "Type", "smtp no seting");
                    }
                }
                catch (Exception even)
                {
                    ResponseWriteEnd("error", "500", "", even.Message);
                }
            }
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select shopInfo.* from shopInfo left join systemSchedule on shopInfo.id=systemSchedule.menuID where systemSchedule.id=@scheduleID", conn);
                cmd.Parameters.Add(new SqlParameter("@scheduleID", scheduleID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            mail.title = "【活動通知】" + reader["name"].ToString();
                            mail.content = mail.content + "<h3>" + mail.title + "</h3>";
                            mail.content = mail.content + "<img src='" +
                                (reader["Picture1"].ToString().IndexOf("http") >= 0 ? reader["Picture1"].ToString() : web_url + reader["Picture1"].ToString())
                                + "' /><br />";
                            mail.content = mail.content + reader["Toldescribe"].ToString().Replace(System.Environment.NewLine, "<br />").Replace("\r\n", "<br />").Replace("\r", "<br />").Replace("\n", "<br />").Replace("" + (char)10, "<br />") + "<br />";
                            mail.content = mail.content + title + "敬上<br/>此為系統自動通知信，請勿直接回信！<br />";
                            mail.content = mail.content + "若您要取消電子報寄送，請 <a title='取消訂閱' href='" + web_url + "/tw/cancelSubscription.asp?AppID={{AppID}}'>按此</a>";
                        }
                    }
                }
                catch (Exception even)
                {
                    ResponseWriteEnd("error", "500", "", even.Message);
                }
                finally
                {
                    reader.Close();
                }
            }
            return mail;
        }
        #endregion
        #region 取得訂閱者
        private List<Subscriber> getSubscriber()
        {
            List<Subscriber> targets = new List<Subscriber>();
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_beginSendSubscriber";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@scheduleID", scheduleID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Subscriber sc = new Subscriber();
                            sc.AppID = reader["APPid"].ToString();
                            sc.email = reader["email"].ToString();
                            targets.Add(sc);
                        }
                    }
                }
                catch (Exception even)
                {
                    ResponseWriteEnd("error", "500", "", even.Message);
                }
                finally
                {
                    reader.Close();
                }
            }
            return targets;
        }
        #endregion

        private void send(List<Subscriber> targets, Mail mail)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                Sender sender = new Sender
                {
                    msg = mail.content,
                    sender = mail.sender,
                    mysubject = mail.title,
                    senderName = "客服中心",
                    mail = new List<Subscriber>()
                };
                if (sendMemberCount == 1)
                {
                    sender.msg = sender.msg.Replace("{{AppID}}", targets[i].AppID);
                    sender.mail.Add(targets[i]);
                }
                else
                {
                    for (int j = 1; j < sendMemberCount && i < targets.Count; j++, i++)
                    {
                        sender.mail.Add(targets[i]);
                    }
                }
                //Thread t1 = new Thread(new ParameterizedThreadStart(send_email));
                //t1.Start(sender);
                send_email(sender);
            }
        }

        public void send_email(object mysender)
        {
            Sender sender = (Sender)mysender;
            List<Subscriber> mail = sender.mail;
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            DataTable tvp = new DataTable();
            message.From = new MailAddress(sender.sender, sender.senderName);
            try
            {
                DataRow row = tvp.NewRow();
                tvp.Columns.Add("mail", typeof(String));
                tvp.Columns["mail"].MaxLength = 100;//長度
                if (mail.Count == 1)
                {
                    row["mail"] = mail[0].email;
                    message.To.Add(mail[0].email);
                }
                else
                {
                    message.To.Add(sender.sender);
                    for (int i = 0; i < mail.Count; i++)
                    {
                        row["mail"] = mail[i].email;
                        message.Bcc.Add(mail[i].email);
                    }
                }
                tvp.Rows.Add(row);
            }
            catch (Exception even)
            {
                ResponseWriteEnd("error", "500-1", "", even.Message);
            }
            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;//E-mail編碼
            message.Subject = sender.mysubject;//E-mail主旨
            message.Body = sender.msg;//E-mail內容

            SmtpClient smtpClient = new SmtpClient(server, port);//設定E-mail Server和port
            smtpClient.Credentials = new NetworkCredential(email, pwd);
            smtpClient.EnableSsl = true;

            //context.Response.Write("{sever:'"+ server + "',port:"+ port + ",account:'"+ email + "',pwd:'"+ pwd + "'}");
            smtpClient.Send(message);
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_updateSystemSchedule";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@scheduleID", scheduleID));
                cmd.Parameters.Add(new SqlParameter("@sender", tvp));
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception even)
                {
                    ResponseWriteEnd("error", "500-2", "", even.Message);
                }
                ResponseWriteEnd("success", "200", "", "success");
            }
        }

        #region 將訊息以json格式字串輸出
        private void ResponseWriteEnd(string status, string code, string id, string msg)
        {
            context.Response.Write("{\"status\":\"" + status + "\",\"code\":\"" + code + "\",\"id\":\"" + id + "\",\"msg\":\"" + msg + "\"}");
            context.Response.End();
        }
        #endregion
    }
}