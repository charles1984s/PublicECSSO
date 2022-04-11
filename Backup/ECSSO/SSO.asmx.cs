using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using System.Net.Mail;

namespace ECSSO
{
    /// <summary>
    /// SSO 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下一行。
    // [System.Web.Script.Services.ScriptService]
    public class SSO : System.Web.Services.WebService
    {        
        [WebMethod]
        public string Login(String OrgName,String UserName,String UserPwd)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement MemberData = doc.CreateElement("MemberData");
            doc.AppendChild(MemberData);

            String setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
            using (SqlConnection conn = new SqlConnection(setting))
            {                
                conn.Open();
                String Str_sql = "select ch_name,email,sex,vip,mem_id from cust where id=@id and pwd=@pwd and chk='Y'";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", UserName));
                cmd.Parameters.Add(new SqlParameter("@pwd", UserPwd));
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        XmlElement sub1 = doc.CreateElement("success");
                        sub1.InnerText = "true";
                        MemberData.AppendChild(sub1);

                        sub1 = doc.CreateElement("MemID");
                        sub1.InnerText = reader["mem_id"].ToString();
                        MemberData.AppendChild(sub1);

                        sub1 = doc.CreateElement("Name");
                        sub1.InnerText = reader["ch_name"].ToString();
                        MemberData.AppendChild(sub1);

                        sub1 = doc.CreateElement("Vip");
                        sub1.InnerText = reader["vip"].ToString();
                        MemberData.AppendChild(sub1);

                        sub1 = doc.CreateElement("Sex");
                        sub1.InnerText = reader["sex"].ToString();
                        MemberData.AppendChild(sub1);

                        sub1 = doc.CreateElement("ErrorMsg");
                        sub1.InnerText = "";
                        MemberData.AppendChild(sub1);                        
                    }
                }
                else {

                    XmlElement sub1 = doc.CreateElement("success");
                    sub1.InnerText = "false";
                    MemberData.AppendChild(sub1);

                    sub1 = doc.CreateElement("MemID");
                    sub1.InnerText = "";
                    MemberData.AppendChild(sub1);

                    sub1 = doc.CreateElement("Name");
                    sub1.InnerText = "";
                    MemberData.AppendChild(sub1);

                    sub1 = doc.CreateElement("Vip");
                    sub1.InnerText = "";
                    MemberData.AppendChild(sub1);

                    sub1 = doc.CreateElement("Sex");
                    sub1.InnerText = "";
                    MemberData.AppendChild(sub1);

                    sub1 = doc.CreateElement("ErrorMsg");
                    sub1.InnerText = "查無資料";
                    MemberData.AppendChild(sub1);

                }                
            }
            return MemberData.OuterXml;
        }
        
        [WebMethod]
        public void Forget(String OrgName, String UserName, String Tel) {

            String ServiceMail = "";
            String ServiceTitle = "";
            String CustID = "";
            String CustMail = "";
            String CustPwd = "";
            
            String setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                String Str_sql = "select email,id,pwd from cust where id=@id and chk='Y' and (tel=@tel or cell_phone=@cell_phone)";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", UserName));
                cmd.Parameters.Add(new SqlParameter("@tel", Tel));
                cmd.Parameters.Add(new SqlParameter("@cell_phone", Tel));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        using (SqlConnection conn2 = new SqlConnection(setting))
                        {
                            conn2.Open();
                            String Str_sql2 = "select service_mail,title from head";
                            SqlCommand cmd2 = new SqlCommand(Str_sql2,conn2);
                            SqlDataReader reader2 = cmd2.ExecuteReader();
                            try
                            {
                                while (reader2.Read()) {
                                    ServiceMail = reader2[0].ToString();                                    
                                    ServiceTitle = reader2[1].ToString();
                                }
                            }
                            finally {
                                reader2.Close();
                            }
                        }                        

                        while (reader.Read()) 
                        {
                            CustMail = reader[0].ToString();
                            CustID = reader[1].ToString();
                            CustPwd = reader[2].ToString();
                        }

                        if (ServiceMail == "") {
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
                        Mail_cont = Mail_cont + "</font></td></tr></table></td>";
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
                            
                        }
                    }
                }
                finally {
                    reader.Close();
                }
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
    }
}
