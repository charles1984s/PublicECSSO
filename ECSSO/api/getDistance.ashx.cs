using ECSSO.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace ECSSO.api
{
    /// <summary>
    /// getDistance 的摘要描述
    /// </summary>
    public class getDistance : IHttpHandler
    {
        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            GetStr GS = new GetStr();

            int statement = 0;
            string returnMsg = "Something has wrong!!";

            if (context.Request.Params["Type"] == null || context.Request.Params["Type"].ToString() == "") statement = 1;
            if (context.Request.Params["Items"] == null || context.Request.Params["Items"].ToString() == "") statement = 2;
            if (context.Request.Params["CheckSum"] == null || context.Request.Params["CheckSum"].ToString() == "") statement = 3;
            if (context.Request.Params["Token"] == null || context.Request.Params["Token"].ToString() == "") statement = 4;

            switch (statement)
            {
                case 0:
                    {
                        String ChkM = context.Request.Params["CheckSum"].ToString();
                        String Type = context.Request.Params["Type"].ToString();
                        String Token = context.Request.Params["Token"].ToString();
                        Location items = JsonConvert.DeserializeObject<Location>(context.Request.Params["Items"]);
                        string Items = context.Request.Params["Items"].ToString();
                        string strSqlConnection = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
                        
                        CommandSetting cmdSetting = new CommandSetting();
                        DataListSetting dataList = new DataListSetting();
                        dataList.Data = new List<object>();
                        switch (cmdSetting.isVerityState(GS.GetIPAddress(), Token, strSqlConnection))
                        {
                            case 0:
                                if (GS.MD5Check(Type + Items, ChkM))
                                {
                                    if (Type == "Distance")
                                    {
                                        //計算距離
                                        MapDistanceServices _distanceServices = new MapDistanceServices();
                                        double dbdistince = _distanceServices.GetDistance(items.lat1, items.lng1, items.lat2, items.lng2);
                                        

                                        dataList.Data.Add(dbdistince);
                                        returnMsg = JsonConvert.SerializeObject(dataList);
                                    }
                                    else
                                    {
                                        returnMsg = ErrorMsg("error", "Type不存在", "");
                                    }
                                }
                                else
                                {
                                    returnMsg = ErrorMsg("error", "CheckSum驗證失敗", "");
                                }
                                break;
                            case 1:
                                returnMsg = ErrorMsg("error", "Token不存在", "");
                                break;
                            case 2:
                                returnMsg = ErrorMsg("error", "Token權限出問題", "");
                                break;
                        }
                        break;
                    }
                case 1:
                    {
                        returnMsg = ErrorMsg("error", "Type必填", "");
                        break;
                    }
                case 2:
                    {
                        returnMsg = ErrorMsg("error", "Items必填", "");
                        break;
                    }
                case 3:
                    {
                        returnMsg = ErrorMsg("error", "CheckSum必填", "");
                        break;
                    }
                case 4:
                    {
                        returnMsg = ErrorMsg("error", "Token必填", "");
                        break;
                    }
            }

            //context.Response.ContentType = "text/plain";
            context.Response.Write(returnMsg);
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
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
        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "getVoices error", "", RspnMsg);
            }

            ECSSO.Library.Products.RootObject root = new ECSSO.Library.Products.RootObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "取得影片"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " getTrip.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        public class MapDistanceServices
        {
            public MapDistanceServices()
            {
            }

            private const double EARTH_RADIUS = 6378.137;
            private double rad(double d)
            {
                return d * Math.PI / 180.0;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="lat1">緯度1</param>
            /// <param name="lng1">經度1</param>
            /// <param name="lat2">緯度2</param>
            /// <param name="lng2">經度2</param>
            /// <returns></returns>
            public double GetDistance(double lat1, double lng1, double lat2, double lng2)
            {
                double dblResult = 0;
                double radLat1 = rad(lat1);
                double radLat2 = rad(lat2);
                double distLat = radLat1 - radLat2;
                double distLng = rad(lng1) - rad(lng2);
                dblResult = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(distLat / 2), 2) +
                                Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(distLng / 2), 2)));
                dblResult = dblResult * EARTH_RADIUS;
                //dblResult = Math.Round(dblResult * 10000) /10000;  //這回傳變成公里,少3個0變公尺
                dblResult = Math.Round(dblResult * 10000) / 10;

                return dblResult;
            }
        }
        public class Location
        {
            public double lat1 { get; set; }
            public double lng1 { get; set; }
            public double lat2 { get; set; }
            public double lng2 { get; set; }
        }
    }
}