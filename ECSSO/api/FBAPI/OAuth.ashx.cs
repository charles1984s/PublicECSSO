using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Facebook;
using System.Dynamic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using ECSSO.Library;

namespace ECSSO.api.FBAPI
{
    /// <summary>
    /// OAuth 的摘要描述
    /// </summary>
    public class OAuth : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            String code = "";
            
            if (context.Request.QueryString["code"] != null)
            {
                code = context.Request.QueryString["code"].ToString();
                string client_id = "";
                string redirect_uri = "";
                string client_secret = "";
                string URL = "";

                using (SqlConnection conn = new SqlConnection(GetSetting(context.Session["OrgName"].ToString())))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("select Game_AppID,Game_AppURL,Game_AppSecret from head", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                client_id = reader[0].ToString();
                                redirect_uri = reader[1].ToString();
                                client_secret = reader[2].ToString();
                            }
                        }
                    }
                    finally { reader.Close(); }
                }

                URL = "https://graph.facebook.com/oauth/access_token?client_id=" + client_id + "&redirect_uri=" + redirect_uri + "&client_secret=" + client_secret + "&code=" + code;

                //取得頁面返回值
                //此時頁面上返回 access_token=demo&expires=demo 字串
                string result = GetWebRequest(URL);
                //稍微剖一下就取得 access_token
                string token = result.Replace("access_token=", "").Split('&')[0];
                //取得 access_token後，透過https://graph.facebook.com/me 抓個人資料
                //頁面返回 JSON
                string JSON = GetWebRequest("https://graph.facebook.com/me?access_token=" + token);

                FB.me postf = JsonConvert.DeserializeObject<FB.me>(JSON);                
                
                
                //使用 JSON.Net 動態剖 JSON
                //dynamic json = JValue.Parse(JSON);
                //取得 Facebook UID
                //Session["UID"] = json.id;
                //地點 = json.location.name
                //照片 = json.link
                //名稱 = json.name
                //生日 = json.birthday
                //EMAIL = json.email
                //性別 = json.gender
                String UID = postf.id;
                String Name = postf.name;
                String Photo = postf.link;
                String Email = postf.email;
                String Gender = postf.gender;

                SaveCust(GetSetting(context.Session["OrgName"].ToString()), UID, Name, Photo, Email, Gender);
                SaveCust(GetSetting("ezsaleo2o"), UID, Name, Photo, Email, Gender);
                GameSave(GetSetting(context.Session["OrgName"].ToString()), UID, Name, Photo, context.Session["VerCode"].ToString());

                Game.FBData fbdata = new Game.FBData()
                {
                    UID = UID,
                    photo = Photo,
                    name = Name
                };

                String RspnCode = "0";
                String RspnMsg = "";

                Game.Login root = new Game.Login()
                {
                    RspnCode = RspnCode,
                    RspnMsg = RspnMsg,
                    Item = fbdata
                };

                context.Response.Write(JsonConvert.SerializeObject(root));
            }
            
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public string GetWebRequest(string URL)
        {
            WebRequest MyRequest = WebRequest.Create(URL);
            MyRequest.Method = "GET";
            WebResponse MyResponse = MyRequest.GetResponse();
            StreamReader sr = new StreamReader(MyResponse.GetResponseStream());
            string result = sr.ReadToEnd();
            sr.Close();
            MyResponse.Close();
            return result;
        }

        #region 儲存會員資料
        private void SaveCust(String Setting, String UID, String Name, String Photo,String Email,String Gender)
        {

            String MemID = "";
            String Pwd = DateTime.Now.ToString("yyyyMMddHHmmss");            
            String Birth = "";
            String Bonus = "0";

            switch (Gender) { 
                case "female":
                    Gender = "2";
                    break;
                case "male":
                    Gender = "1";
                    break;
                default:
                    Gender = "1";
                    break;
            }
            String Str_Sql = "";

            #region 判斷是否已是會員
            bool IsMem = false;
            Str_Sql = "select mem_id from cust where id=@id";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", Email));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {                        
                            IsMem = true;                     
                    }
                }
                finally { reader.Close(); }
            }
            #endregion

            if (!IsMem)
            {
                #region 取得MemID
                Str_Sql = "select REPLICATE('0',6-LEN(isnull(MAX(mem_id),'0')+1)) + RTRIM(CAST(isnull(MAX(mem_id),'0')+1 AS CHAR)) from cust";
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand(Str_Sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                MemID = reader[0].ToString();
                            }
                        }
                    }
                    finally { reader.Close(); }
                }
                #endregion

                #region insert into cust
                Str_Sql = "insert into Cust (mem_id,id,pwd,ch_name,sex,birth,email,vip,bonus_total,chk,UID,photo,crm_date,logintime)";
                Str_Sql += " values (@mem_id,@id,sys.fn_VarBinToHexStr(hashbytes('MD5', convert(nvarchar,@pwd))),@ch_name,@sex,@birth,@email,'1',@bonus_total,'Y',@UID,@photo,replace(replace(CONVERT([varchar](256),getdate(),(120)),'-',''),':',''),getdate())";
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand(Str_Sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                    cmd.Parameters.Add(new SqlParameter("@id", Email));
                    cmd.Parameters.Add(new SqlParameter("@pwd", Pwd));
                    cmd.Parameters.Add(new SqlParameter("@ch_name", Name));
                    cmd.Parameters.Add(new SqlParameter("@sex", Gender));
                    cmd.Parameters.Add(new SqlParameter("@birth", Birth));
                    cmd.Parameters.Add(new SqlParameter("@email", Email));
                    cmd.Parameters.Add(new SqlParameter("@bonus_total", "0"));
                    cmd.Parameters.Add(new SqlParameter("@UID", UID));
                    cmd.Parameters.Add(new SqlParameter("@photo", Photo));
                    cmd.ExecuteNonQuery();
                }
                #endregion

                #region 取得加入會員紅利
                Str_Sql = "select bonus_first from head";
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand(Str_Sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Bonus = reader[0].ToString();
                            }
                        }
                    }
                    finally { reader.Close(); }
                }
                #endregion

                #region 紀錄紅利LOG
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_CheckMail";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                    cmd.Parameters.Add(new SqlParameter("@bonus", Bonus));
                    cmd.ExecuteNonQuery();
                }
                #endregion
            }
        }
        #endregion

        #region 儲存資料到玩家暫存區
        private void GameSave(String Setting,String UID,String Name,String Photo,String VerCode) { 
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("insert into gamer(UID,name,photo,vercode) values (@UID,@name,@photo,@vercode)", conn);
                cmd.Parameters.Add(new SqlParameter("@UID", UID));
                cmd.Parameters.Add(new SqlParameter("@name", Name));
                cmd.Parameters.Add(new SqlParameter("@photo", Photo));
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                cmd.ExecuteNonQuery();
            }            
        }
        #endregion

        #region 取得Orgname連結字串
        private String GetSetting(String OrgName)
        {
            return "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
        }
        #endregion    

        
    }
}