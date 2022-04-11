using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ECSSO.HinetAPI
{
    public partial class index : System.Web.UI.Page
    {
        //該OAuth app 的基本資料       
        string RedirectURIs = ConfigurationManager.AppSettings.Get("Hinet_RedirectURIs");
        string Client_ID = ConfigurationManager.AppSettings.Get("Hinet_Client_ID");
        string Client_secret = ConfigurationManager.AppSettings.Get("Hinet_Client_secret");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //以下是抓取?後面的參數的程式碼,抓完後要把參數存到session裡面
                if ((Request.QueryString["SiteID"] != null) && (Request.QueryString["ReturnUrl"] != null) && (Request.QueryString["Url"] != null) && (Request.QueryString["language"] != null))
                {
                    if ((Request.QueryString["SiteID"].ToString() != null) && (Request.QueryString["ReturnUrl"].ToString() != null) && (Request.QueryString["Url"].ToString() != null) && (Request.QueryString["language"] != null))
                    {
                        Session["siteid"] = Request.QueryString["SiteID"].ToString();
                        Session["returnurl"] = HttpContext.Current.Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString());
                        Session["weburl"] = HttpContext.Current.Server.UrlDecode(Request.QueryString["Url"].ToString());
                        Session["language"] = Request.QueryString["language"].ToString();
                        Session["VerCode"] = Request.QueryString["VerCode"].ToString();

                        //抓完參數後就直接redirect
                        redirectToHinet();
                        return;
                    }
                    else
                    {
                        removeSession();
                        Response.Write("<script type='text/javascript'>history.go(-2);</script>");
                        Response.End();
                    }
                }
                //以上是抓取?後面的參數的程式碼,抓完後要把參數存到session裡面

                if ((Session["siteid"] != null) && (Session["returnurl"] != null) && (Session["weburl"] != null) && (Session["language"] != null))
                {
                    if ((Session["siteid"].ToString() != null) && (Session["returnurl"].ToString() != null) && (Session["weburl"].ToString() != null) && (Session["language"].ToString() != null))
                    {
                        if (Request.QueryString["Code"] != null)
                        {
                            var Json = new System.Web.Script.Serialization.JavaScriptSerializer();
                            var JsonObj = Json.Deserialize<Dictionary<string, string>>(GetToken());
                            var JsonAccess_token = JsonObj["access_token"];
                            var JsonUserEmail = "";
                            String OAuthSnAndId = "";

                            JsonObj = Json.Deserialize<Dictionary<string, string>>(GetUserInfor(JsonAccess_token));
                            JsonUserEmail = JsonObj["email"].ToString();

                            //因為此OpenID可能會有抓不到email之情形，所以要加上sn+id來做為辨別
                            OAuthSnAndId += JsonObj["sn"].ToString() + JsonObj["userid"].ToString();

                            common.APIandDB Comm = new common.APIandDB();
                            if (Comm.LoginOrReg(Session["siteid"].ToString(), JsonUserEmail.ToString(), OAuthSnAndId))
                            {
                                //確定有這個人或者已經成功OAuth取得資料註冊後要做的事

                                //因為此OpenID有可能會有email為空的情況，所以登入完要再把他正確的email拉出來(因為使用者有可能是在註冊後才新增OpenID的email,所以主要還是以SnAndId來做辨識email)
                                JsonUserEmail = Comm.getEmail(Session["siteid"].ToString(), OAuthSnAndId);
                                Token token = new Token();
                                GetStr getstr = new GetStr();
                                String TokenID = token.LoginToken(JsonUserEmail.ToString(), getstr.GetSetting(Session["siteid"].ToString()));
                                
                                //string RedirectTemp = Session["weburl"].ToString() + "/tw/log.asp?id=" + JsonUserEmail.ToString() + "'&ReturnUrl=" + Session["returnurl"].ToString().Replace("&", "////");

                                string[] RedirectTemp = new string[] { "" };
                                try
                                {
                                    RedirectTemp = new string[] { Session["weburl"].ToString() + "/" + getstr.GetLanString(Session["language"].ToString()) + "/log.asp?id=" + JsonUserEmail.ToString() + "&tokenid=" + TokenID + "&VerCode=" + Session["VerCode"].ToString() + "&ReturnUrl=" + Session["returnurl"].ToString().Replace("&", "////").Split('?')[1] };
                                }
                                catch
                                {
                                    RedirectTemp = new string[] { Session["weburl"].ToString() + "/" + getstr.GetLanString(Session["language"].ToString()) + "/log.asp?id=" + JsonUserEmail.ToString() + "&tokenid=" + TokenID + "&VerCode=" + Session["VerCode"].ToString() + "&ReturnUrl=au_id=a////sub_id=b" };
                                }
                                finally {
                                    removeSession();
                                    Response.Redirect(RedirectTemp[0]);                                
                                }
                                return;
                            }
                            else
                            {
                                /*
                                 * email認證之後還要重新跑OAuth的流程好麻煩~"~
                                 * 但是我不想額外找地方儲存SnAndId了
                                 * 就乾脆讓使用者重跑一次吧 :P
                                 */

                                //登入or自動註冊失敗代表此OpenID並沒有給予正確的email，所以要傳入使用者已認證成功的email
                                //當發現此OpenID並沒有給予正確的email就會先跑一次認證流程
                                if (Session["CheckedEmail"] != null)    //如果有已認證過的email就註冊吧
                                {
                                    RegMember(OAuthSnAndId);    //RegMember會自動抓取Session裡面的CheckedEmail所以不用額外傳入

                                    //不管註冊失敗或成功RegMember都會自動跳轉到該跳的頁面
                                }

                                //如果Session["CheckedEmail"] != null檢查沒通過代表該使用者根本沒成功跑過email認證，所以要先顯示畫面讓使用者輸入email並且認證
                                return; //要先return離開Page_Load才能顯示畫面
                                //(一定要return,不然會下面的程式碼會把Session清掉)
                                //(這一個else裡面絕不能清空session,不然會需要重跑整個流程)
                            }
                        }
                    }
                }
                removeSession();
                Response.Write("<script type='text/javascript'>history.go(-2);</script>");
                Response.End();
            }
        }

        //OAuth 一開始讓使用者先redirect
        private void redirectToHinet()
        {
            string _oauthUrl = string.Format("http://member.cht.com.tw/CHTAccount/Auth?" +
                        "scope={0}&state={1}&redirect_uri={2}&response_type=code&client_id={3}",
                        "basic_profile",
                        "",
                        HttpUtility.UrlEncode(RedirectURIs),
                        HttpUtility.UrlEncode(Client_ID)
                        );
            _oauthUrl = string.Format("http://member.cht.com.tw/CHTAccount/Auth?" +
            "client_id={0}&redirect_uri={1}&response_type=code&scope=basic_profile&state={2}",
            Client_ID,
            RedirectURIs,
            ""
            );
            Response.Redirect(_oauthUrl);
        }

        //取到code後post to google,會回傳JSON檔
        private string GetToken()
        {
            string result = "";
            string queryStringFormat = @"code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code";
            string postcontents = string.Format(queryStringFormat
                                               , HttpUtility.UrlEncode(Request.QueryString["Code"])
                                               , HttpUtility.UrlEncode(Client_ID)
                                               , HttpUtility.UrlEncode(Client_secret)
                                               , HttpUtility.UrlEncode(RedirectURIs));
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://member.cht.com.tw/CHTAccount/Token");
            request.Method = "POST";
            byte[] postcontentsArray = Encoding.UTF8.GetBytes(postcontents);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postcontentsArray.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(postcontentsArray, 0, postcontentsArray.Length);
                requestStream.Close();
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    responseStream.Close();
                    response.Close();
                    result = responseFromServer;
                }
            }
            return result;
        }
        private string GetUserInfor(string access_token)
        {
            string result = "";
            WebRequest response;
            string GetUserInforUrl = string.Format("http://member.cht.com.tw/CHTAccount/UserInfo?" + @"access_token={0}&client_id={1}"
            , HttpUtility.UrlEncode(access_token)
            , HttpUtility.UrlEncode(Client_ID));
            response = WebRequest.Create(GetUserInforUrl);
            using (Stream responseStream = response.GetResponse().GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    responseStream.Close();
                    result = responseFromServer;
                }
            }
            return result;
        }

        protected void 確定_Click(object sender, EventArgs e)
        {
            //為避免hinet會員的email為null時會發生錯誤，取得使用者手動輸入的資料當做會員id

            //先檢查手動輸入的email是不是正確的email，如果不是就退回去，輸入正確的email才能跑認證流程(跑完認證流程才給註冊唷!!)
            if (Regex.IsMatch(TextBox1.Text, @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$"))
            {   
                //先把不必要的Session清掉並且修改target，然後就Transfer到checkEmail.aspx去吧
                Session.Remove("snAndid");
                Session.Remove("CheckedEmail");
                Session["target"] = "SendCheckMail";
                Session["SendEmail"] = TextBox1.Text;
                Server.Transfer("checkEmail.aspx");
            }
        }
        private void RegMember(string OAuthSnAndId)
        {
            common.APIandDB Comm = new common.APIandDB();
            string[] RedirectTemp = new string[] { "" };
            if (Comm.RegMember(Session["siteid"].ToString(), Session["CheckedEmail"].ToString(), OAuthSnAndId))
            {
                //成功"新增"新的會員才會跑到這邊
                //成功註冊後要做的事
                Token token = new Token();
                GetStr getstr = new GetStr();
                String TokenID = token.LoginToken(Session["CheckedEmail"].ToString(), getstr.GetSetting(Session["siteid"].ToString()));

                RedirectTemp = new string[] { Session["weburl"].ToString() + "/" + getstr.GetLanString(Session["language"].ToString()) + "/log.asp?id=" + Session["CheckedEmail"].ToString() + "&tokenid=" + TokenID + "&ReturnUrl=" + Session["returnurl"].ToString().Replace("&", "////").Split('?')[1] };
                //RedirectTemp = Session["weburl"].ToString() + "/tw/log.asp?id=" + Session["CheckedEmail"].ToString() + "'&ReturnUrl=" + Session["returnurl"].ToString().Replace("&", "////");
            }
            else
            {
                //有可能是email or snAndId重複，所以要改用update的方式
                //updateSnAndId會檢查SnAndId是否重複，重複的話就會回傳false(這樣很像多做一次,如果到時候覺得太慢再把他拿掉吧)
                if (Comm.updateSnAndId(Session["siteid"].ToString(), Session["CheckedEmail"].ToString(), OAuthSnAndId)) //update的code還沒寫唷
                {
                    //update成功了就讓他登入吧
                    Token token = new Token();
                    GetStr getstr = new GetStr();
                    String TokenID = token.LoginToken(Session["CheckedEmail"].ToString(), getstr.GetSetting(Session["siteid"].ToString()));

                    RedirectTemp = new string[] { Session["weburl"].ToString() + "/" + getstr.GetLanString(Session["language"].ToString()) + "/log.asp?id=" + Session["CheckedEmail"].ToString() + "&tokenid=" + TokenID + "&ReturnUrl=" + Session["returnurl"].ToString().Replace("&", "////").Split('?')[1] };
                
                    //RedirectTemp = Session["weburl"].ToString() + "/tw/log.asp?id=" + Session["CheckedEmail"].ToString() + "'&ReturnUrl=" + Session["returnurl"].ToString().Replace("&", "////");
                }
                else
                {
                    //搞那麼多驗證還有問題就掰掰踢出去!
                    RedirectTemp = new string[] { Session["returnurl"].ToString() };
                }
            }
            removeSession();
            Response.Redirect(RedirectTemp[0]);
        }
        private void removeSession()
        {
            Session.Remove("siteid");
            Session.Remove("returnurl");
            Session.Remove("weburl");
            Session.Remove("language");
            Session.Remove("target");
            Session.Remove("snAndid");
            Session.Remove("CheckedEmail");
            Session.Remove("VerCode");
        }
    }
}