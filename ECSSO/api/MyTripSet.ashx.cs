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

namespace ECSSO.api
{
    /// <summary>
    /// getMyTripSet 的摘要描述
    /// </summary>
    public class MyTripSet : IHttpHandler
    {
        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            GetStr GS = new GetStr();

            int statement = 0;
            string returnMsg = "Something has wrong!!", tableName = "Trip";
            try
            {
                if (context.Request.Params["Type"] == null || context.Request.Params["Type"].ToString() == "") statement = 1;
                if (context.Request.Params["Items"] == null || context.Request.Params["Items"].ToString() == "") statement = 2;
                if (context.Request.Params["CheckSum"] == null || context.Request.Params["CheckSum"].ToString() == "") statement = 3;
                if (context.Request.Params["Token"] == null || context.Request.Params["Token"].ToString() == "") statement = 4;
                if (context.Request.Params["Lng"] == null || context.Request.Params["Lng"].ToString() == "") statement = 5;


                switch (statement)
                {
                    case 0:
                        {
                            String ChkM = context.Request.Params["CheckSum"].ToString();
                            String Type = context.Request.Params["Type"].ToString();
                            String Token = context.Request.Params["Token"].ToString();
                            String Lng = GS.CheckStringIsNotNull(context.Request.Params["Lng"].ToString());
                            string Items = context.Request.Params["Items"].ToString();
                            string strSqlConnection = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();


                            if (GS.MD5Check(Type + Items, ChkM))
                            {
                                returnMsg = new CommandSetting().getDataResult(GS.GetIPAddress(), Token, strSqlConnection, Type, Lng, tableName, Items, "", 0, 0);
                            }
                            else
                            {
                                returnMsg = ErrorMsg("error", "CheckSum驗證失敗", "");
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
                    case 5:
                        {
                            returnMsg = ErrorMsg("error", "Lng必填", "");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                returnMsg = ErrorMsg("error", ex.ToString(), "");
            }

            context.Response.Write(returnMsg);
            context.Response.End();
        }
        /*
        private string downloadMyTrip(PostForm.MyTrip items)
        {
            MyTrip root = new MyTrip();
            root.Data = new List<MyTripItem>();
            MyTripItem item1 = new MyTripItem();
            item1.sn="01";
            item1.TripDate="2018-01-01";
            item1.TripTime="13:00";
            item1.nodeID="xxxx(高鐵左營站)";
            item1.duration=0;
            MyTripItem item2 = new MyTripItem();
            item2.routeID="01";
            item2.routeDetail="高鐵333至高鐵台中站轉台鐵555至斗六車站步行至公車總站轉台西客運6666至雲科大站步行15分鐘";
            item2.duration=150;
            MyTripItem item3 = new MyTripItem();
            item3.sn = "01";
            item3.TripDate = "2018-01-01";
            item3.TripTime = "15:30";
            item3.nodeID = "xxxx(雲林文化中心)";
            item3.duration = 120;
            MyTripItem item4 = new MyTripItem();
            item4.routeID = "01";
            item4.routeDetail = "步行15分鐘至雲科大";
            item4.duration = 15;
            MyTripItem item5 = new MyTripItem();
            item5.sn = "03";
            item5.TripDate = "2018-01-01";
            item5.TripTime = "17:00";
            item5.nodeID = "xxxx(雲科大)";
            item5.duration = 60;
            MyTripItem item6 = new MyTripItem();
            item6.routeID = "01";
            item6.routeDetail = "開車20分鐘至OOMOTEL";
            item6.duration = 20;
            MyTripItem item7 = new MyTripItem();
            item7.sn = "04";
            item7.TripDate = "2018-01-01";
            item7.TripTime = "17:20";
            item7.nodeID = "xxxx(OOMOTEL)";
            item7.duration = 0;
            root.Data.Add(item1);
            root.Data.Add(item2);
            root.Data.Add(item3);
            root.Data.Add(item4);
            root.Data.Add(item5);
            root.Data.Add(item6);
            root.Data.Add(item7);
            return JsonConvert.SerializeObject(root);
        }
        private string uploadMyTrip(MyTrip items)
        {
            MyTrip.RootObject root = new MyTrip.RootObject();
            MyTrip.TripItem item = new MyTrip.TripItem();
            root.Data = new List<MyTrip.TripItem>();
            root.RspnCode = "success";
            item.TripID = items.TripTitle;
            root.Data.Add(item);
            return JsonConvert.SerializeObject(root);
        }
        private string getMyTrip(PostForm.MyTrip items)
        {
            MyTripList root = new MyTripList();
            root.Data = new List<MyTrip>();
            MyTrip item = new MyTrip();
            item.AreaID = "000001";
            item.UUID = "uuidxxx";
            item.UDID = "udidxxx";
            item.TripTitle = "TripToYunLin";
            item.uploadDateTime = "2018-01-01 13:59:59";
            item.Remark = "智慧旅運計畫遊程踩線";
            root.Data.Add(item);
            return JsonConvert.SerializeObject(root);
        }
        */

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
                InsertLog(Setting, "getMyTripSet error", "", RspnMsg);
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "取得旅程"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " getMyTripSet.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }
}