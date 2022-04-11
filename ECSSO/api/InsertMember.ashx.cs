using System;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using System.Net.Mail;
namespace ECSSO.api
{
    /// <summary>
    /// InsertMember 的摘要描述
    /// </summary>
    public class InsertMember : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Form["Items"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Items必填"));

            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Form["Items"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Items必填"));

            String ChkM = context.Request.Form["CheckM"].ToString();
            String SiteID = context.Request.Form["SiteID"].ToString();

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);

            if (GS.MD5Check(SiteID + OrgName, ChkM))
            {
                String MemID = "";
                Library.Member.Data member = JsonConvert.DeserializeObject<Library.Member.Data>(System.Web.HttpUtility.UrlDecode(context.Request.Form["Items"]));
                //Insert Cust
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_NewMember3";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    cmd.Parameters.Add(new SqlParameter("@id", member.ID));
                    cmd.Parameters.Add(new SqlParameter("@pwd", member.Pwd));
                    cmd.Parameters.Add(new SqlParameter("@ch_name", member.ChName));
                    cmd.Parameters.Add(new SqlParameter("@sex", member.Sex));
                    cmd.Parameters.Add(new SqlParameter("@email", member.Email));
                    cmd.Parameters.Add(new SqlParameter("@birth", Convert.ToDateTime(member.Birth).ToString("yyyy-MM-dd")));
                    cmd.Parameters.Add(new SqlParameter("@tel", member.Tel));
                    cmd.Parameters.Add(new SqlParameter("@cell_phone", member.CellPhone));
                    cmd.Parameters.Add(new SqlParameter("@addr", member.Addr));
                    cmd.Parameters.Add(new SqlParameter("@ident", ""));
                    cmd.Parameters.Add(new SqlParameter("@id2", ""));
                    cmd.Parameters.Add(new SqlParameter("@C_ZIP", member.C_ZIP));
                    cmd.Parameters.Add(new SqlParameter("@SnAndId", ""));
                    cmd.Parameters.Add(new SqlParameter("@chk", "O"));
                    SqlParameter SPOutput = cmd.Parameters.Add("@mem_id", SqlDbType.NVarChar, 7);
                    SPOutput.Direction = ParameterDirection.Output;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        MemID = SPOutput.Value.ToString();
                    }
                    catch
                    {
                        MemID = "";
                    }
                }


                if (MemID != "" && MemID != "error:2")
                {
                    String Service_mail = "";
                    String Mail_title = "";
                    String CrmVersion = "";
                    String CertificationURL = "";
                    String SendMemberMail = "";
                    String WebUrl = "";
                    //Search Service Data
                    using (SqlConnection conn = new SqlConnection(Setting))
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
                                    WebUrl = "http://" + reader["web_url"].ToString();
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }                    

                    /*新版會員信*/
                    String Mail_Cont = "";
                    Mail_Cont += "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />";
                    Mail_Cont += "<div style='font-family:微軟正黑體, Arial, 新細明體, Helvetica, sans-serif'>";
                    Mail_Cont += "    <font size='4' color='#ff0000'><b>親愛的會員，您好！歡迎加入" + Mail_title + "會員</b></font><br>";
                    Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px; margin-top:8px; margin-bottom:20px;'>";
                    Mail_Cont += "以下是您的帳號資料，請熟記以下重要訊息<br>";
                    Mail_Cont += "    <br>";
                    Mail_Cont += "    <strong>您的帳號：</strong>" + member.Email + "<br>";
                    Mail_Cont += "    <br>";
                    Mail_Cont += "    <strong>您的密碼：</strong>" + member.Pwd + "<br>";
                    Mail_Cont += "    <br>";
                    if (CertificationURL == "Y")
                    {
                        Mail_Cont += "    <strong>開通帳號網址：</strong><br>";
                        Mail_Cont += "    <a href='" + WebUrl + "/" + GS.GetLanString(member.Language) + "/checkm.asp?mem_id=" + MemID + "'>" + WebUrl + "/" + GS.GetLanString(member.Language) + "/checkm.asp?mem_id=" + MemID + "</a><br>";
                        Mail_Cont += "    <span style='color: #666;'>(為了啟動你的帳號，請點選連結或是複製連結在瀏覽器上貼上)<br>";
                    }

                    Mail_Cont += "感謝您的加入！~</span><br>";
                    Mail_Cont += "    <br>";
                    Mail_Cont += "    <span style='color:#f00;'>提醒您：此封『會員通知』為系統發出，請勿直接回覆。</span>";
                    Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px'>";
                    Mail_Cont += "提醒您，客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款<br>";
                    Mail_Cont += "若有上述情形，請立即撥打165防詐騙專線查詢";
                    Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";
                    Mail_Cont += "</div>";

                    String Mail_Cont2 = "";
                    Mail_Cont2 += "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />";
                    Mail_Cont2 += "<div style='font-family:微軟正黑體, Arial, 新細明體, Helvetica, sans-serif'>";
                    Mail_Cont2 += "會員編號" + MemID + "已於" + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "加入會員<br>";
                    Mail_Cont2 += "</div>";


                    send_email(Mail_Cont, "加入會員通知" + " 【" + Mail_title + "】", Service_mail, member.Email);//呼叫send_email函式測試，寄給會員

                    if (SendMemberMail == "Y")
                    {
                        send_email(Mail_Cont2, "加入會員通知" + " 【" + Mail_title + "】", Service_mail, Service_mail);//呼叫send_email函式測試，寄給管理者
                    }

                    ResponseWriteEnd(context, MemID);
                }
                else
                {
                    ResponseWriteEnd(context, MemID);
                }

            }
            else {
                ResponseWriteEnd(context, "chkm error");
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg)
        {

            Library.Products.RootObject root = new Library.Products.RootObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion

    }
}