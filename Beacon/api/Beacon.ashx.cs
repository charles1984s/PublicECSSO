using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ECSSO.Library;
using ECSSO;
using Beacon.Library;

namespace Beacon.api
{
    /// <summary>
    /// Beacon 的摘要描述
    /// </summary>
    public class Beacon : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填"));
            if (context.Request.Params["minorID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "minorID必填"));
            if (context.Request.Params["map"] == null) ResponseWriteEnd(context, ErrorMsg("error", "map必填"));

            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填"));
            if (context.Request.Params["minorID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "minorID必填"));
            if (context.Request.Params["location"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "location必填"));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String Type = context.Request.Params["Type"].ToString();
            String minorID = context.Request.Params["minorID"].ToString();
            String location = context.Request.Params["location"].ToString();

            String uuidID = context.Request.Params["uuidID"] == null ? "" : context.Request.Params["uuidID"].ToString();
            String majorID = context.Request.Params["majorID"] == null ? "" : context.Request.Params["majorID"].ToString();

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);

            if (GS.MD5Check(Type + SiteID + OrgName, ChkM))
            {
                switch (Type)
                {
                    case "4":       //兌換紅利的網頁
                        ResponseWriteEnd(context, ouputBeaconWebpage(Setting, minorID,uuidID,majorID));
                        break;
                    case "5":       //查詢對應Beacon地點資料
                        ResponseWriteEnd(context, beaconLocationData(Setting, location));
                        break;
                }           
            }
        }

private void ResponseWriteEnd(HttpContext context,bool p)
{
 	throw new NotImplementedException();
}


        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg)
        {

            Library.RootObject root = new Library.RootObject();
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

       
        #region 兌換紅利的網頁
        private string ouputBeaconWebpage(String Setting, String minorID, String uuidID,String majorID)
        {
            List<webpageurl> webpageUrl = new List<webpageurl>();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(string.Format("select webpage from beaconSiteData where minorID = @minorID {0} {1}", uuidID=="" ? "" : "and uuid=@uuid", majorID=="" ? "" : "and majorID=@majorID"), conn);
                cmd.Parameters.Add(new SqlParameter("@minorID", minorID));
                if(uuidID !="") cmd.Parameters.Add(new SqlParameter("@uuid", uuidID));
                if (uuidID != "") cmd.Parameters.Add(new SqlParameter("@majorID", majorID));


                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            webpageUrl.Add(new webpageurl
                            {
                                webpage = reader["webpage"].ToString()
                            });
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return JsonConvert.SerializeObject(webpageUrl);
        }
        #endregion


        #region 查詢對應Beacon地點資料
        private string beaconLocationData(String Setting, String location)
        {
            List<beacon> beaconDataList = new List<beacon>();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from beaconSiteData where location = @location", conn);
                cmd.Parameters.Add(new SqlParameter("@location", location));

                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            beaconDataList.Add(new beacon
                            {
                                minorID = reader["minorID"].ToString(),
                                uuidID = reader["uuidID"].ToString(),
                                majorID = reader["majorID"].ToString(),
                                locationName = reader["locationName"].ToString(),
                                map_x = reader["map_x"].ToString(),
                                map_y = reader["map_y"].ToString(),
                                mapPicSource = reader["mapPicSource"].ToString(),
                                locationNumber = reader["locationNumber"].ToString(),
                                webpage = reader["webpage"].ToString()
                            });
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return JsonConvert.SerializeObject(beaconDataList);
        }
        #endregion


    
public  object location { get; set; }}
}