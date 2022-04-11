using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Web.Services;

namespace ECSSO.HinetAPI
{
    public partial class checkEmail : System.Web.UI.Page
    {
        //要用的加密方式
        SHA256 sha256 = new SHA256CryptoServiceProvider();
        string Hash;    //存放Hash的變數

        //HashDataXX = 要查詢的application值
        string HashData00 = "HashData00";
        string HashData11 = "HashData11";
        int EncryptionTime = 10;    //加密時間差
        int DecryptionTime = 12;    //解密時間差

        protected void Page_Load(object sender, EventArgs e)
        {
            //先檢查如果有這些Session代表目前應該是要發認證信的時候
            if (Session["siteid"] != null && Session["returnurl"] != null && Session["weburl"] != null && Session["SendEmail"] != null)
            {
                //如果有就發認證信囉
                sendEmailCheck(Session["siteid"].ToString(), Session["SendEmail"].ToString());
                return;
            }
            //接收時存放的
            string ReceiveEmail = "";
            string ReceiveHash = "";

            //現在做到要接收確認信的URL內容
            if ((Request.QueryString["email"] != null) && (Request.QueryString["Hash"] != null) && (Request.QueryString["SiteID"] != null) && (Request.QueryString["ReturnUrl"] != null) && (Request.QueryString["Url"] != null))
            {
                if ((Request["email"].ToString() != null) && (Request["Hash"].ToString() != null) && (Request["SiteID"].ToString() != null) && (Request["ReturnUrl"].ToString() != null) && (Request["Url"].ToString() != null))
                {
                    Session["siteid"] = Request.QueryString["SiteID"].ToString();
                    Session["returnurl"] = HttpContext.Current.Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString());
                    Session["weburl"] = HttpContext.Current.Server.UrlDecode(Request.QueryString["Url"].ToString());
                    ReceiveEmail = HttpContext.Current.Server.UrlDecode(Request.QueryString["email"].ToString());
                    ReceiveHash = Request.QueryString["Hash"].ToString();   //QueryString會自動解碼，所以不用再額外解碼
                    
                    //先看看HashData是否為null
                    if (Application[HashData00] != null)
                    {
                        string[] HashData = (string[])Application[HashData00];

                        //如果時間差小於解密時間才開始解密
                        if ((int.Parse(DateTime.Now.ToString("yyMMddHHmm").ToString()) - int.Parse(HashData[0])) < DecryptionTime)
                        {
                            //email -> hash
                            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(ReceiveEmail)));

                            //hash + time -> hash
                            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Hash + HashData[0])));

                            //hash + random -> hash
                            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Hash + HashData[1])));

                            //hash + SiteId -> hash
                            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Hash + Session["siteid"].ToString())));

                            if (Hash.Equals(ReceiveHash))
                            {
                                //認證成功要把Session存進去後重跑一次認證流程
                                Session["CheckedEmail"] = ReceiveEmail;
                                Session["target"] = "EmailChecked";
                                Response.Write("<script language='javascript'>alert('認證成功，現在將重新登入'); window.location.href='../login.aspx?SiteID=" + Session["siteid"].ToString() + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(Session["returnurl"].ToString()) + "&Url=" + HttpContext.Current.Server.UrlEncode(Session["weburl"].ToString()) + "';</script>");
                                return;
                            }
                        }
                    }

                    //上面沒過的話就檢查第二個
                    //先看看HashData是否為null
                    if (Application[HashData11] != null)
                    {
                        string[] HashData = (string[])Application[HashData11];

                        //如果時間差小於解密時間才開始解密
                        if ((int.Parse(DateTime.Now.ToString("yyMMddHHmm").ToString()) - int.Parse(HashData[0])) < DecryptionTime)
                        {
                            //email -> hash
                            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(ReceiveEmail)));

                            //hash + time -> hash
                            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Hash + HashData[0])));

                            //hash + random -> hash
                            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Hash + HashData[1])));

                            //hash + SiteId -> hash
                            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Hash + Session["siteid"].ToString())));

                            if (Hash.Equals(ReceiveHash))
                            {
                                //認證成功要把Session存進去後重跑一次認證流程
                                Session["CheckedEmail"] = ReceiveEmail;
                                Session["target"] = "EmailChecked";
                                Response.Write("<script language='javascript'>alert('認證成功，現在將重新登入'); window.location.href='../login.aspx?SiteID=" + Session["siteid"].ToString() + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(Session["returnurl"].ToString()) + "&Url=" + HttpContext.Current.Server.UrlEncode(Session["weburl"].ToString()) + "';</script>");
                                return;
                            }
                        }
                    }
                    //上面都沒有認證成功的話就代表是認證失敗了!!
                    Response.Write("<script language='javascript'>alert('認證失敗，可能是超過十分鐘了，點選確定並重新登入'); window.location.href='../login.aspx?SiteID=" + Session["siteid"].ToString() + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(Session["returnurl"].ToString()) + "&Url=" + HttpContext.Current.Server.UrlEncode(Session["weburl"].ToString()) + "';</script>");
                }
            }
        }

        private void sendEmailCheck(string SiteId,string email)
        {   
            //基本上Session["SendEmail"]只是用來傳遞資料而已，傳遞完畢就把它清除吧
            Session.Remove("SendEmail");


            //寄認證信
            //以下是認證信的Hash解說//
            /*
             * time = 系統時間
             * random = 亂數
             * SiteId = 對應的SiteID
             * Time = 放在Application["HashData00"] and Application["HashData11"]裡面的時間
             * Random = 放在Application["HashData00"] and Application["HashData11"]裡面的數字
             * 
             * 加密 email -> hash + time -> hash + random -> hash + SiteId -> hash1
             * 解密 email -> hash + Time -> hash + Random -> hash + SiteId -> hash2
             * (hash1 == hash2) ? CheckedMail : mailUncheck
             * 
             * 加密時 10 分鐘time過期
             * 解密時 12 分鐘time過期
             * 
             * chech email之後寫入session裡面，並且整個OAuth流程自動重跑
             */
            //以上是認證信的Hash解說//

            //實做yo~

            //加密時先取得可以用的time和random
            string[] HashData = CheckApplication();

            //email -> hash
            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(email)));

            //hash + time -> hash
            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Hash + HashData[0])));

            //hash + random -> hash
            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Hash + HashData[1])));

            //hash + SiteId -> hash
            Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Hash + SiteId)));

            //認證網址
            string EmailCheckUrl = "http://sson.ezsale.tw/HinetAPI/checkEmail.aspx?email=" + HttpContext.Current.Server.UrlEncode(email) + "&Hash=" + HttpContext.Current.Server.UrlEncode(Hash) + "&SiteID=" + Session["siteid"].ToString() + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(Session["returnurl"].ToString()) + "&Url=" + HttpContext.Current.Server.UrlEncode(Session["weburl"].ToString());

            string Mail_title = "";

            //Send Mail
            String Mail_cont = "";
            Mail_cont += "<table width='576' cellpadding='0' cellspacing='0' align='center'>";
            Mail_cont += "<tr><td><font color='#333333' size='3'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            Mail_cont += Mail_title + "認證信通知</font></td></tr><tr><td><img src='http://www.cocker.com.tw/images/cockermail_bg.jpg'></td></tr>";
            Mail_cont += "<tr><td><table width='450' border='0' align='center' cellpadding='0' cellspacing='0'><tr><td height='15'></td></tr><tr><td ><font color='#333333' size='3'>您好，請在十分鐘內點選以下網址進行Email認證。</ br>";
            Mail_cont += "Email認證<a href='" + EmailCheckUrl + "'>請按此</a>";
            Mail_cont += "</font></td></tr></table></td></tr><tr><td align='center' height='15'></td></tr>";
            Mail_cont += "<tr><td align='center'><font color='#d62929' size='2'>提醒您，客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款<br>若有上述情形，請立即撥打165防詐騙專線查詢</font></td></tr></table>";

            send_email(Mail_cont, "認證信通知通知", "a8092947@yahoo.com.tw", email);//呼叫send_email函式測試
            Response.Write("<script language='javascript'>alert('已寄出認證信！請到您的信箱(" + email + ")進行認證。'); window.location.href='../login.aspx?SiteID=" + Session["siteid"].ToString() + "&ReturnUrl=" + HttpContext.Current.Server.UrlEncode(Session["returnurl"].ToString()) + "&Url=" + HttpContext.Current.Server.UrlEncode(Session["weburl"].ToString()) + "';</script>");
        }

        //加密時才用得到這個，解密時直接調用application["HashDataXX"]就可以了(XX是00和11，都有)
        private string[] CheckApplication()
        {
            string[] TimeAndRandom = new string[2]; //一份HashData包含的陣列資訊(0是time,1是random)
            string time = DateTime.Now.ToString("yyMMddHHmm");  //目前時間
            string random = DateTime.Now.ToString("fffffff");   //random的亂數值
            

            //目前時間和HashDataXX的時間相差(直接用字串轉int然後相減的方式)
            int timeDiff;
            

            //-----以下是一組HashData-----
            
            //用try的原因是因為可以順便檢查看看HashData是否為null，如果是null就直接給值
            try
            {
                //檢查時間差
                timeDiff = (int.Parse(time) - int.Parse(((string[])Application[HashData00])[0]));
            }
            catch (NullReferenceException)
            {
                //如果出現NullReferenceException代表Application[HashData00])是空的(最最最開始使用的樣子)，那就直接給定一個值然後return
                TimeAndRandom[0] = time;
                TimeAndRandom[1] = random;
                Application[HashData00] = TimeAndRandom;
                return ((string[])Application[HashData00]);
            }
            
            //看有沒有超過加密有效分鐘數
            if (timeDiff >= EncryptionTime)
            {
                //如果相差有超過加密有效分鐘數就檢查看看有沒有超過解密有效時間
                if (timeDiff >= DecryptionTime)
                {
                    //如果有超過，代表這個HashData已經過期了，就更新它吧
                    TimeAndRandom[0] = time;
                    TimeAndRandom[1] = random;                    
                    Application[HashData00] = TimeAndRandom;
                    return ((string[])Application[HashData00]);
                }
            }
            else
            {
                return ((string[])Application[HashData00]);
            }
            //如果上面比對超過加密有效時間可是未超過解密有效時間就使用另一個HashDataXX，當然也要先比對一下
            
            //-----以上是一組HashData-----



            //-----以下是一組HashData-----

            //第二個HashData
            try
            {
                //一樣要檢查時間差
                timeDiff = (int.Parse(time) - int.Parse(((string[])Application[HashData11])[0]));
            }
            catch (NullReferenceException)
            {
                //如果出現NullReferenceException代表Application[HashData00])是空的(最最最開始使用的樣子)，那就直接給定一個值然後return
                TimeAndRandom[0] = time;
                TimeAndRandom[1] = random;
                Application[HashData11] = TimeAndRandom;
                return ((string[])Application[HashData11]);
            }            

            //看目有沒有超過加密有效分鐘數
            if (timeDiff >= EncryptionTime)
            {
                //如果相差有超過加密有效分鐘數就檢查看看有沒有超過解密有效時間
                if (timeDiff >= DecryptionTime)
                {
                    //如果有超過，代表這個HashData已經過期了，就更新它吧
                    TimeAndRandom[0] = time;
                    TimeAndRandom[1] = random;
                    Application[HashData11] = TimeAndRandom;
                    return ((string[])Application[HashData11]);
                }
            }
            else
            {
                return ((string[])Application[HashData11]);
            }

            //-----以上是一組HashData-----

            //如果能跑到這裡，代表系統有設計上瑕疵
            TimeAndRandom[0] = time;
            TimeAndRandom[1] = random;
            return TimeAndRandom;
        }

        private void send_email(string msg, string mysubject, string sender, string mail)
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