using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ECSSO.Library;
using System.Net;
using System.IO;
using System.Text;
using System.Web;

namespace Beacon.api
{
    /// <summary>
    /// getCockerAPI 的摘要描述
    /// </summary>
    public class getCockerAPI : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            String VerCode = "";
            if (context.Request.Params["items"] != null) 
            {
                Library.BeaconInput.Items postf = JsonConvert.DeserializeObject<Library.BeaconInput.Items>(context.Request.Params["items"]);
                
                VerCode = postf.verCode;
                String Key = postf.key;
                String MAC = postf.MAC;
                String TxPower = postf.TxPower;
                
                ECSSO.GetStr GS = new ECSSO.GetStr();
                String Orgname = GetOrgName("{" + VerCode + "}");

                if (Orgname == "") ResponseWriteEnd(context, ErrorMsg("error", "查無Orgname", ""));

                String Setting = GetSetting(Orgname);

                //更新電量
                //UpdateBeaconPower(Key, MAC, TxPower, @"http://beacon.ezsale.tw/powerUpdate.ashx");

                //提供資料
                ResponseWriteEnd(context, GetBeaconData(postf.Data, Setting));
            }            
            else if (context.Request.Params["verCode"] != null && context.Request.Params["method"] != null)
            {
                VerCode = context.Request.Params["verCode"].ToString();

                ECSSO.GetStr GS = new ECSSO.GetStr();
                String Orgname = GetOrgName("{" + VerCode + "}");

                if (Orgname == "") ResponseWriteEnd(context, ErrorMsg("error", "查無Orgname", ""));

                String Setting = GetSetting(Orgname);
                
                String Method = context.Request.Params["method"].ToString();
                String ID = "";
                String Type = "";

                if (context.Request.Params["id"] != null) ID = context.Request.Params["id"].ToString();
                if (context.Request.Params["type"] != null) Type = context.Request.Params["type"].ToString();

                ResponseWriteEnd(context, GetData(Setting, Method, ID, Type));
            }
            else 
            {
                ResponseWriteEnd(context, ErrorMsg("error", "無參數", ""));
            }
            

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "Beacon error", "", RspnMsg);
            }

            Library.BeaconInput.Error root = new Library.BeaconInput.Error();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }

        #region Get IP
        private string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string sIPAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(sIPAddress))
            {
                return context.Request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                string[] ipArray = sIPAddress.Split(new Char[] { ',' });
                return ipArray[0];
            }
        }
        #endregion

        #region insert log
        private void InsertLog(String Setting, String JobName, String JobTitle, String Detail)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_userlogAdd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", "guest"));
                cmd.Parameters.Add(new SqlParameter("@prog_name", "候位前台"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " booking.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region 由Vercode取得Orgname
        private String GetOrgName(String VerCode)
        {
            String OrgName = "";
            String Str_Sql = "select orgname from Device where stat='Y' and getdate() between start_date and end_date and VerCode=@VerCode";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@VerCode", VerCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            OrgName = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            return OrgName;
        }
        #endregion

        #region 取得Orgname連結字串
        private String GetSetting(String OrgName)
        {
            return "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
        }
        #endregion   

        #region 更新Beacon電量
        private void UpdateBeaconPower(String Key, String MAC, String TxPower,String BeaconURL) 
        {
            string Url = BeaconURL;
            HttpWebRequest request = HttpWebRequest.Create(Url) as HttpWebRequest;
            string result = null;
            request.Method = "POST";    // 方法
            request.KeepAlive = true; //是否保持連線
            request.ContentType = "application/x-www-form-urlencoded";
            Library.BeaconInput.BeaconPower BP = new Library.BeaconInput.BeaconPower
            {
                key = Key,
                MAC = MAC,
                TxPower = TxPower
            };
            string param = "data=" + JsonConvert.SerializeObject(BP);
            byte[] bs = Encoding.ASCII.GetBytes(param);
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }
            using (WebResponse response = request.GetResponse())
            {
                StreamReader sr = new StreamReader(response.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
            }
        }
        #endregion

        #region 取得Beacon播放資料
        private String GetBeaconData(List<Library.BeaconInput.Datum> Data, String Setting) 
        {
            Library.BeaconInput.ReturnObject root = new Library.BeaconInput.ReturnObject();
            List<Library.BeaconInput.Detail> Details = new List<Library.BeaconInput.Detail>();
            
            String stat = "";
            String ID = "";
            String StrSql = "";

            foreach (Library.BeaconInput.Datum Datas in Data)
            {
                ID = Datas.ID;

                switch(Datas.type)
                {
                    case "prod":
                        StrSql = "select img1,'',item1 from prod where id=@ID and disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between start_date and end_date";
                    break;
                    case "video":
                        StrSql = "select '',media_link,'' from menu where id=@ID and disp_opt='Y'";
                    break;
                    case "coupon":
                    StrSql = "select photo1,'',discription from Coupon where id=@ID and disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between start_date and end_date ";
                    break;
                }

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;
                    cmd = new SqlCommand(StrSql, conn);
                    cmd.Parameters.Add(new SqlParameter("@ID", ID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Library.BeaconInput.Detail DT = new Library.BeaconInput.Detail
                                {
                                    img = reader[0].ToString(),
                                    video = reader[1].ToString(),
                                    Description = HttpUtility.UrlEncode(reader[2].ToString())
                                };
                                Details.Add(DT);
                            }
                        }
                        else
                        {
                            Details = null;
                        }
                        stat = "200";
                    }
                    catch
                    {
                        stat = "401";
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            
            root.stat = stat;
            root.Detail = Details;
            
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 取得網站資料列表
        private String GetData(String Setting, String Method, String ID, String Type)
        {
            String StrSql = "";

            switch (Method) { 
                case "DataList":    //選單列表
                    if (Type != "") 
                    {
                        switch (Type)
                        {
                            case "prod":
                                StrSql = "select id,title from prod_authors where disp_opt='Y' and type='1'";                                
                                break;
                            case "video":
                                StrSql = "select b.id,b.title from menu_sub as a left join menu as b on a.id=b.sub_id where a.use_module='7' and a.disp_opt='Y' and b.disp_opt='Y'";
                                break;
                            case "coupon":
                                StrSql = "select id,title from coupon where disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between start_date and end_date";
                                break;
                        }
                    }
                    break;
                case "prodSubList": //產品分類列表
                    if (ID != "")
                    {
                        StrSql = "select id,title from prod_list where au_id=@ID and disp_opt='Y'";
                    }
                    break;
                case "prodList":    //產品列表
                    if (ID != "")
                    {
                        StrSql = "select id,title from prod where sub_id=@ID and disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between start_date and end_date";
                    }
                    break;
            }

            if (StrSql != "")
            {
                return GetData(Setting, StrSql, ID);
            }
            else 
            {
                Library.BeaconInput.ReturnObject2 RO2 = new Library.BeaconInput.ReturnObject2
                {
                    stat = "401",
                    List = null
                };
                return JsonConvert.SerializeObject(RO2);
            }
        }

        private String GetData(String Setting,String StrSql,String ID) 
        {
            Library.BeaconInput.ReturnObject2 RO2 = new Library.BeaconInput.ReturnObject2();
            List<Library.BeaconInput.List> BList = new List<Library.BeaconInput.List>();
            String stat = "";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(StrSql, conn);
                if (ID != "") cmd.Parameters.Add(new SqlParameter("@ID", ID));                
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Library.BeaconInput.List Data = new Library.BeaconInput.List
                            {
                                id = reader[0].ToString(),
                                title = reader[1].ToString()
                            };
                            BList.Add(Data);
                        }
                    }
                    else
                    {
                        BList = null;
                    }
                    stat = "200";
                }
                catch
                {
                    stat = "401";
                }
                finally
                {
                    reader.Close();
                }
            }

            RO2.List = BList;
            RO2.stat = stat;

            return JsonConvert.SerializeObject(RO2);
        }

        #endregion


    }
}