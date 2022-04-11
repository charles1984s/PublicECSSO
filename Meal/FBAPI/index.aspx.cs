using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ECSSO.FBAPI
{
    public partial class index : System.Web.UI.Page
    {
        //該OAuth app 的基本資料
        string RedirectURIs = ConfigurationManager.AppSettings.Get("FB_RedirectURIs");
        string Client_ID = ConfigurationManager.AppSettings.Get("FB_Client_ID");
        string Client_secret = ConfigurationManager.AppSettings.Get("FB_Client_secret");


        protected void Page_Load(object sender, EventArgs e)
        {   
            if (!IsPostBack)
            {
                //以下是抓取?後面的參數的程式碼,抓完後要把參數存到session裡面
                if ((Request.QueryString["SiteID"] != null) && (Request.QueryString["ReturnUrl"] != null) && (Request.QueryString["Url"] != null) && (Request.QueryString["language"] != null))
                {
                    if ((Request.QueryString["SiteID"].ToString() != null) && (Request.QueryString["ReturnUrl"].ToString() != null) && (Request.QueryString["Url"].ToString() != null) && (Request.QueryString["language"].ToString() != null))
                    {
                        Session["siteid"] = Request.QueryString["SiteID"].ToString();
                        Session["language"] = Request.QueryString["language"].ToString();
                        Session["returnurl"] = HttpContext.Current.Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString());
                        Session["weburl"] = HttpContext.Current.Server.UrlDecode(Request.QueryString["Url"].ToString());
                        Session["VerCode"] = Request.QueryString["VerCode"].ToString();

                        //抓完參數後就直接redirect
                        redirectToFacebook();
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
                            string JsonAccess_token = GetToken().Replace("access_token=", "").Split('&')[0];
                            
                            var JsonObj = Json.Deserialize<Dictionary<string, string>>(GetUserInfor(JsonAccess_token));                            
                            var JsonUserEmail = "";
                            JsonUserEmail = JsonObj["email"].ToString().Replace("\u0040", "@");
                            common.APIandDB Conn = new common.APIandDB();
                            if (Conn.LoginOrReg(Session["siteid"].ToString(), JsonUserEmail.ToString()))
                            {
                                Token token = new Token();
                                GetStr getstr = new GetStr();
                                String TokenID = token.LoginToken(JsonUserEmail.ToString(), getstr.GetSetting(Session["siteid"].ToString()));
                                
                                //確定有這個人或者已經成功OAuth取得資料註冊後要做的事
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
                                string[] RedirectTemp = new string[] {Session["returnurl"].ToString()};
                                removeSession();
                                
                                Response.Redirect(RedirectTemp[0]);
                                return;
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
        public void redirectToFacebook()
        {
            string _oauthUrl = string.Format("https://www.facebook.com/dialog/oauth/?" +
                        "scope={0}&state={1}&redirect_uri={2}&response_type=code&client_id={3}",
                        HttpUtility.UrlEncode("email,user_birthday"),
                        "",
                        HttpUtility.UrlEncode(RedirectURIs),
                        HttpUtility.UrlEncode(Client_ID)
                        );
            Response.Redirect(_oauthUrl);
        }

        //取到code後post to google,會回傳JSON檔
        public string GetToken()
        {
            string result = "";
            string GetTokenUrl = string.Format("https://graph.facebook.com/oauth/access_token?" + @"code={0}&client_id={1}&client_secret={2}&redirect_uri={3}"
                        , HttpUtility.UrlEncode(Request.QueryString["Code"])
                        , HttpUtility.UrlEncode(Client_ID)
                        , HttpUtility.UrlEncode(Client_secret)
                        , HttpUtility.UrlEncode(RedirectURIs));
            WebRequest MyRequest = WebRequest.Create(GetTokenUrl);
            MyRequest.Method = "GET";
            WebResponse MyResponse = MyRequest.GetResponse();
            StreamReader sr = new StreamReader(MyResponse.GetResponseStream());
            result = sr.ReadToEnd();
            sr.Close();
            MyResponse.Close();
            return result;
        }
        public string GetUserInfor(string access_token)
        {
            string result = "";
            WebRequest response;
            response = WebRequest.Create("https://graph.facebook.com/me?access_token=" + HttpUtility.UrlEncode(access_token));
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
        private void removeSession()
        {
            Session.Remove("siteid");
            Session.Remove("language");
            Session.Remove("returnurl");
            Session.Remove("weburl");
            Session.Remove("target");
            Session.Remove("snAndid");
            Session.Remove("VerCode");
        }
    }
}