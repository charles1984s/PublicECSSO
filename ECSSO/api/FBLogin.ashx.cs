using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.SessionState;
namespace ECSSO.api
{
    /// <summary>
    /// FBLogin 的摘要描述
    /// </summary>
    public class FBLogin : IHttpHandler, IReadOnlySessionState
    {
        HttpContext context;
        #region FB架構
        private class FBReturn
        {
            public String access_token { get; set; }
            public String token_type { get; set; }
            public String expires_in { get; set; }
        }

        public class APPTokenData
        {
            public string app_id { get; set; }
            public string application { get; set; }
            public int expires_at { get; set; }
            public bool is_valid { get; set; }
            public int issued_at { get; set; }
            public List<string> scopes { get; set; }
            public string user_id { get; set; }
        }

        public class APPToken
        {
            public APPTokenData data { get; set; }
        }

        public class PermissionData
        {
            public string permission { get; set; }
            public string status { get; set; }
        }

        public class Permission
        {
            public List<PermissionData> data { get; set; }
        }

        public class FBUserData
        {
            public string id { get; set; }
            public string name { get; set; }
            public string email { get; set; }
        }
        #endregion

        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            String ClientID = ConfigurationManager.AppSettings.Get("FB_Client_ID");
            String RedirectUri = ConfigurationManager.AppSettings.Get("FB_RedirectURIs");

            if (HttpContext.Current.Request.QueryString["code"] != null)
            {
                String ClientSecret = ConfigurationManager.AppSettings.Get("FB_Client_secret");
                String code = HttpContext.Current.Request.QueryString["code"].ToString();

                #region 把FB傳來的字串解碼!取得萬能的Token
                GetStr GS = new GetStr();
                FBReturn rlib = JsonConvert.DeserializeObject<FBReturn>(GetWebRequest(String.Format("https://graph.facebook.com/v2.7/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}"
                    , ClientID
                    , RedirectUri
                    , ClientSecret
                    , code)));
                String Token = rlib.access_token;
                #endregion

                #region 測試access_token的正確性
                APPToken APT = JsonConvert.DeserializeObject<APPToken>(GetWebRequest(String.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}"
                    , Token
                    , ClientID
                    , ClientSecret)));
                String UserID = APT.data.user_id;
                #endregion

                #region 檢查使用者授權的權限正確性 : granted = 已授權
                Permission PS = JsonConvert.DeserializeObject<Permission>(GetWebRequest(String.Format("https://graph.facebook.com/v2.7/me/permissions?access_token={0}"
                    , Token)));
                Boolean EmailPs = false;
                Boolean PublicProfile = false;
                foreach (PermissionData ps in PS.data)
                {
                    switch (ps.permission)
                    {
                        case "email":
                            if (ps.status == "granted") EmailPs = true;
                            break;
                        case "public_profile":
                            if (ps.status == "granted") PublicProfile = true;
                            break;
                        default:

                            break;
                    }
                }

                if (!EmailPs || !PublicProfile)
                {
                    ResponseWriteEnd(context, "<script language='javascript'> alert('Facebook取得資料失敗,請選擇其他方式登入');history.go(-1); </script>"); //資料取得失敗
                }
                else
                {
                    #region 取得使用者資料
                    FBUserData User = JsonConvert.DeserializeObject<FBUserData>(GetWebRequest(String.Format("https://graph.facebook.com/v2.7/{0}?fields=id,name,email&access_token={1}"
                        , UserID
                        , Token)));
                    String FBID = User.id;
                    String Name = User.name;
                    String Email = User.email;
                    String setting = GS.GetSetting2(HttpContext.Current.Session["siteid"].ToString());
                    String RedirectURL = "";
                    String Returnstr = "";

                    //switch (HttpContext.Current.Session["Action"].ToString())
                    //{
                    //    case "Binding":

                    //Returnstr = fbBinding(setting, FBID, Email);
                    //switch (Returnstr)
                    //{
                    //    case "success": //成功

                    //        RedirectURL = String.Format("/Member.aspx?language={0}&SiteID={1}&ReturnUrl={2}&MemID={3}&CheckM={4}"
                    //            , HttpContext.Current.Session["language"].ToString()
                    //            , HttpContext.Current.Session["siteid"].ToString()
                    //            , HttpContext.Current.Session["ReturnUrl"].ToString()
                    //            , HttpContext.Current.Session["Memid"].ToString()
                    //            , GS.MD5Endode(HttpContext.Current.Session["siteid"].ToString() + HttpContext.Current.Session["Memid"].ToString()));
                    //        HttpContext.Current.Session.RemoveAll();
                    //        ResponseWriteEnd(context, "<script language='javascript'> alert('綁定成功');window.location.href='" + RedirectURL + "'; </script>");
                    //        break;
                    //    case "error:1": //FB回傳參數有誤
                    //        HttpContext.Current.Session.RemoveAll();
                    //        ResponseWriteEnd(context, "<script language='javascript'> alert('綁定失敗:" + Returnstr + "');history.go(-1); </script>");
                    //        break;
                    //    case "error:2": //無此會員編號
                    //        HttpContext.Current.Session.RemoveAll();
                    //        ResponseWriteEnd(context, "<script language='javascript'> alert('綁定失敗:" + Returnstr + "');history.go(-1); </script>");
                    //        break;
                    //    case "error:3": //SQL執行失敗
                    //        HttpContext.Current.Session.RemoveAll();
                    //        ResponseWriteEnd(context, "<script language='javascript'> alert('綁定失敗:" + Returnstr + "');history.go(-1); </script>");
                    //        break;
                    //    case "error:4": //FB已被綁定
                    //        HttpContext.Current.Session.RemoveAll();
                    //        ResponseWriteEnd(context, "<script language='javascript'> alert('Facebook帳號重複綁定:" + Returnstr + "');history.go(-1); </script>");
                    //        break;
                    //    default:
                    //        HttpContext.Current.Session.RemoveAll();
                    //        ResponseWriteEnd(context, "<script language='javascript'> alert('綁定失敗');history.go(-1); </script>");
                    //        break;
                    //}

                    //break;
                    //case "Login":
                    Returnstr = ChkMember(setting, FBID, Name, Email);
                    if (Returnstr != "")
                    {
                        Token token = new Token();
                        token.updateToken(setting, Email, GS.GetIPAddress());
                        ////////////////臨時給大仁用的///////////////////
                        int totalPrice = 0;
                        string scaned = "0";
                        string memid = "";
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            string sql = "select mem_id from cust where id=@id";
                            SqlCommand cmd = new SqlCommand(sql, conn);
                            cmd.Parameters.Add(new SqlParameter("@id", Email));
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {

                                        memid = reader["mem_id"].ToString();
                                    }
                                }
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }


                        using (SqlConnection conn1 = new SqlConnection(setting))
                        {
                            conn1.Open();
                            SqlCommand cmd1 = new SqlCommand(@"select cp.id as cid,memid,Cust_Coupon.VCode,Price,title,Cust_Coupon.ExpireDate,Stat,ExchangeDay from Cust_Coupon 
                                                    join (select convert(nvarchar(36), VCode) as vVcode,* from Coupon where disp_opt = 'Y') as cp  on '{'+cp.vVcode+'}' = Cust_Coupon.VCode
                                                where  memid= @memid", conn1);
                            cmd1.Parameters.Add(new SqlParameter("@memid", memid));

                            SqlDataReader reader1 = cmd1.ExecuteReader();
                            try
                            {
                                if (reader1.HasRows)
                                {
                                    while (reader1.Read())
                                    {
                                        switch (reader1["cid"].ToString())
                                        {
                                            case "2":
                                                totalPrice += int.Parse(reader1["Price"].ToString());
                                                scaned += "," + "63124";
                                                break;
                                            case "3":
                                                totalPrice += int.Parse(reader1["Price"].ToString());
                                                scaned += "," + "2350";
                                                break;
                                            case "4":
                                                totalPrice += int.Parse(reader1["Price"].ToString());
                                                scaned += "," + "4544";
                                                break;
                                            case "5":
                                                totalPrice += int.Parse(reader1["Price"].ToString());
                                                scaned += "," + "7872";
                                                break;
                                            case "6":
                                                totalPrice += int.Parse(reader1["Price"].ToString());
                                                scaned += "," + "60813";
                                                break;
                                            case "7":
                                                totalPrice += int.Parse(reader1["Price"].ToString());
                                                scaned += "," + "2348";
                                                break;
                                            case "8":
                                                totalPrice += int.Parse(reader1["Price"].ToString());
                                                scaned += "," + "13964";
                                                break;
                                            case "9":
                                                totalPrice += int.Parse(reader1["Price"].ToString());
                                                scaned += "," + "1728";
                                                break;
                                        }

                                    }
                                }
                            }
                            finally
                            {
                                reader1.Close();
                            }
                        }
                        //////////////////////
                        try
                        {
                            //RedirectURL = String.Format("{0}/{1}/log.asp?id={2}&tokenid={3}&VerCode={4}&ReturnUrl={5}"
                            //    , HttpContext.Current.Session["weburl"].ToString()
                            //    , GS.GetLanString(HttpContext.Current.Session["language"].ToString())
                            //    , Returnstr
                            //    , token.LoginToken(Returnstr, setting)
                            //    , HttpContext.Current.Session["VerCode"].ToString()
                            //    , HttpContext.Current.Session["returnurl"].ToString().Replace("&", "////").Split('?')[1]);

                            ////////////////臨時給大仁用的///////////////////
                            RedirectURL = String.Format("{0}/{1}/log.asp?id={2}&tokenid={3}&VerCode={4}&scaned={6}&totalPrice={7}&ReturnUrl={5}"
                                    , HttpContext.Current.Session["weburl"].ToString()
                                    , GS.GetLanString(HttpContext.Current.Session["language"].ToString())
                                    , Returnstr
                                    , token.LoginToken(Returnstr, setting)
                                    , HttpContext.Current.Session["VerCode"].ToString()
                                    , HttpContext.Current.Session["returnurl"].ToString().Replace("&", "////").Split('?')[1]//);
                                    , scaned
                                        , totalPrice);
                            ///////////////////////////////////////////////////
                        }
                        catch
                        {
                            //RedirectURL = String.Format("{0}/{1}/log.asp?id={2}&tokenid={3}&VerCode={4}&ReturnUrl=au_id=a////sub_id=b"
                            //    , HttpContext.Current.Session["weburl"].ToString()
                            //    , GS.GetLanString(HttpContext.Current.Session["language"].ToString())
                            //    , Returnstr
                            //    , token.LoginToken(Returnstr, setting)
                            //    , HttpContext.Current.Session["VerCode"].ToString());

                            ////////////////臨時給大仁用的///////////////////
                            RedirectURL = String.Format("{0}/{1}/log.asp?id={2}&tokenid={3}&VerCode={4}&scaned={5}&totalPrice={6}&ReturnUrl=au_id=a////sub_id=b"
                                , HttpContext.Current.Session["weburl"].ToString()
                                , GS.GetLanString(HttpContext.Current.Session["language"].ToString())
                                , Returnstr
                                , token.LoginToken(Returnstr, setting)
                                , HttpContext.Current.Session["VerCode"].ToString()//);
                                , scaned
                                    , totalPrice);
                            ///////////////////////////////////////////////////
                        }
                        finally
                        {
                            HttpContext.Current.Response.Redirect(RedirectURL);

                        }
                    }
                    else
                    {
                        ResponseWriteEnd(context, "<script language='javascript'> alert('Facebook登入失敗,請選擇其他方式登入或重新註冊');history.go(-1); </script>"); //新增會員失敗
                    }
                    //break;
                    //    default:
                    //        ResponseWriteEnd(context, "<script language='javascript'>history.go(-1); </script>"); //新增會員失敗
                    //        break;
                    //}
                    #endregion
                }
                #endregion
            }
            else
            {
                //HttpContext.Current.Session["siteid"] = HttpContext.Current.Request.QueryString["siteid"].ToString();
                //HttpContext.Current.Session["weburl"] = HttpContext.Current.Request.QueryString["weburl"].ToString();
                //HttpContext.Current.Session["language"] = HttpContext.Current.Request.QueryString["language"].ToString();
                //HttpContext.Current.Session["VerCode"] = HttpContext.Current.Request.QueryString["VerCode"].ToString();
                //HttpContext.Current.Session["returnurl"] = HttpContext.Current.Request.QueryString["returnurl"].ToString();

                HttpContext.Current.Response.Redirect("https://www.facebook.com/dialog/oauth?client_id=" + ClientID + "&scope=email,public_profile&redirect_uri=" + RedirectUri);
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

        public string GetWebRequest(string URL)
        {
            string result = "";
            try
            {
                WebRequest MyRequest = WebRequest.Create(URL);
                MyRequest.Method = "GET";
                WebResponse MyResponse = MyRequest.GetResponse();
                StreamReader sr = new StreamReader(MyResponse.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
                MyResponse.Close();
            }
            catch(Exception e) {
                context.Response.Write(e);
                context.Response.End();
            }
            return result;
        }

        #region 會員新增開通及紅利點數發放
        private String ChkMember(String setting, String UID, String Name, String Email)
        {
            String ID = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select mem_id,chk,id from cust where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", Email));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        ID = reader[2].ToString();
                        if (reader[1].ToString() != "Y")
                        {
                            using (SqlConnection conn2 = new SqlConnection(setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2 = new SqlCommand();
                                cmd2.CommandText = "sp_CheckMail";
                                cmd2.CommandType = CommandType.StoredProcedure;
                                cmd2.Connection = conn2;
                                cmd2.Parameters.Add(new SqlParameter("@mem_id", reader[0].ToString()));
                                cmd2.Parameters.Add(new SqlParameter("@bonus", ""));
                                cmd2.ExecuteNonQuery();
                            }
                        }
                        return ID;
                    }
                    else
                    {

                        //新增會員資料
                        String MemID = "";
                        using (SqlConnection conn2 = new SqlConnection(setting))
                        {
                            conn2.Open();
                            SqlCommand cmd2 = new SqlCommand();
                            cmd2.CommandText = "sp_NewMember3";
                            cmd2.CommandType = CommandType.StoredProcedure;
                            cmd2.Connection = conn2;
                            cmd2.Parameters.Add(new SqlParameter("@id", Email));
                            cmd2.Parameters.Add(new SqlParameter("@pwd", DateTime.Now.ToString("yyyyMMddhhmmss")));
                            cmd2.Parameters.Add(new SqlParameter("@ch_name", Name));
                            cmd2.Parameters.Add(new SqlParameter("@sex", "1"));
                            cmd2.Parameters.Add(new SqlParameter("@email", Email));
                            cmd2.Parameters.Add(new SqlParameter("@birth", ""));
                            cmd2.Parameters.Add(new SqlParameter("@tel", ""));
                            cmd2.Parameters.Add(new SqlParameter("@cell_phone", ""));
                            cmd2.Parameters.Add(new SqlParameter("@addr", ""));
                            cmd2.Parameters.Add(new SqlParameter("@ident", ""));
                            cmd2.Parameters.Add(new SqlParameter("@id2", ""));
                            cmd2.Parameters.Add(new SqlParameter("@C_ZIP", ""));
                            cmd2.Parameters.Add(new SqlParameter("@SnAndId", UID));
                            cmd2.Parameters.Add(new SqlParameter("@chk", "N"));
                            SqlParameter SPOutput = cmd2.Parameters.Add("@mem_id", SqlDbType.NVarChar, 7);
                            SPOutput.Direction = ParameterDirection.Output;
                            try
                            {
                                cmd2.ExecuteNonQuery();
                                MemID = SPOutput.Value.ToString();
                            }
                            catch
                            {
                                MemID = "error:2";
                            }
                        }

                        //新增完成
                        if (MemID.IndexOf("error") < 0)
                        {
                            using (SqlConnection conn2 = new SqlConnection(setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2 = new SqlCommand();
                                cmd2.CommandText = "sp_CheckMail";
                                cmd2.CommandType = CommandType.StoredProcedure;
                                cmd2.Connection = conn2;
                                cmd2.Parameters.Add(new SqlParameter("@mem_id", MemID));
                                cmd2.Parameters.Add(new SqlParameter("@bonus", ""));
                                cmd2.ExecuteNonQuery();
                            }
                            return Email;
                        }
                        else
                        {
                            //新增失敗
                            return "";
                        }
                    }
                }
                finally { reader.Close(); }
            }

        }
        #endregion
        /*
        private String fbBinding(String setting, String UID, String Email)
        {
            String ReturnStr = "";
            String MemID = HttpContext.Current.Session["Memid"].ToString();
            using (SqlConnection conn2 = new SqlConnection(setting))
            {
                conn2.Open();
                SqlCommand cmd2 = new SqlCommand();
                cmd2.CommandText = "CustBinding";
                cmd2.CommandType = CommandType.StoredProcedure;
                cmd2.Connection = conn2;
                cmd2.Parameters.Add(new SqlParameter("@memID", MemID));
                cmd2.Parameters.Add(new SqlParameter("@email", Email));
                cmd2.Parameters.Add(new SqlParameter("@UID", UID));
                SqlParameter SPOutput = cmd2.Parameters.Add("@returnStr", SqlDbType.NVarChar, 10);
                SPOutput.Direction = ParameterDirection.Output;
                try
                {
                    cmd2.ExecuteNonQuery();
                    ReturnStr = SPOutput.Value.ToString();
                }
                catch
                {
                    ReturnStr = "error:3";
                }
            }

            return ReturnStr;
        }
         * */
    }
}