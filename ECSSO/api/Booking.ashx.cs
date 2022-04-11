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

namespace ECSSO.api
{
    /// <summary>
    /// Booking 的摘要描述
    /// </summary>
    public class Booking : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填", "", ""));
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", "", ""));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", "", ""));

            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填", "", ""));
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", "", ""));
            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", "", ""));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String Type = context.Request.Params["Type"].ToString();

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);


            String DetailStr = "Type = " + Type + ",SiteID = " + SiteID + ",ChkM = " + ChkM;
            InsertLog(Setting, "呼叫booking", "", DetailStr);

            PostForm.BookingPost postf;
            String SeatID = "";
            String StoreID = "";

            switch (Type) 
            {
                case "1":
                    #region 訂位登錄
                    if (context.Request.Params["ItemData"] != null && context.Request.Params["ItemData"] != "")
                    {
                        postf = JsonConvert.DeserializeObject<PostForm.BookingPost>(context.Request.Params["ItemData"]);

                        if (postf.Tel == null || postf.Tel == "") ResponseWriteEnd(context, ErrorMsg("error", "Tel必填", Setting, ""));
                        if (postf.Time == null || postf.Time == "") ResponseWriteEnd(context, ErrorMsg("error", "Time必填", Setting, ""));
                        if (postf.StoreID == null || postf.StoreID == "") ResponseWriteEnd(context, ErrorMsg("error", "StoreID必填", Setting, ""));
                        if (postf.RegisterType == null || postf.RegisterType == "") ResponseWriteEnd(context, ErrorMsg("error", "RegisterType必填", Setting, ""));
                        if (postf.ArrivalTime == null || postf.ArrivalTime == "") ResponseWriteEnd(context, ErrorMsg("error", "ArrivalTime必填", Setting, ""));
                        //if (postf.SeatID == null || postf.SeatID == "") ResponseWriteEnd(context, ErrorMsg("error", "SeatID必填", Setting));                        

                        String Tel = postf.Tel;
                        String Time = postf.Time;                        
                        String RegisterType = postf.RegisterType;
                        String ArrivalTime = postf.ArrivalTime;
                        StoreID = postf.StoreID;
                        SeatID = postf.SeatID;

                        Int32 Pno = 0;
                        String ErrorCode = "";
                        DateTime dtDate;
                        if (!DateTime.TryParse(Time, out dtDate))
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "error:3", Setting, ""));        //日期格式錯誤
                        }

                        if (GS.MD5Check(SiteID + OrgName + Tel + Time, ChkM))
                        {
                            if (!ChkBooking(Setting, Tel, Time, StoreID,SeatID))
                            {
                                Pno = InsertBooking(Setting, Tel, Time, StoreID, RegisterType, SeatID, ArrivalTime, "");        //不要動，Pno會跑掉！
                                if (Pno > 0)
                                {
                                    ErrorCode = SendSMS(Tel, Time, Pno.ToString(), Setting, StoreID, SiteID, RegisterType, SeatID, ArrivalTime);
                                    if (ErrorCode == "000")
                                    {
                                        ResponseWriteEnd(context, ErrorMsg("error", "success", Setting, Pno.ToString()));        //成功
                                    }
                                    else
                                    {
                                        ResponseWriteEnd(context, ErrorMsg("error", GS.GetSMSErrorMsg(ErrorCode), Setting, Pno.ToString()));        //SMS ErrorCode
                                    }
                                }
                                else
                                {
                                    if (RegisterType == "3")
                                    {
                                        ResponseWriteEnd(context, ErrorMsg("error", "error:4", Setting, ""));        //沒庫存
                                    }
                                    else 
                                    {
                                        ResponseWriteEnd(context, ErrorMsg("error", "error:2", Setting, ""));        //SQL錯誤
                                    }
                                }
                            }
                            else
                            {
                                ResponseWriteEnd(context, ErrorMsg("error", "error:1", Setting, ""));        //重複訂位
                            }
                        }
                        else
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting, ""));        //驗證碼錯誤
                        }
                    }
                    else 
                    {
                        ResponseWriteEnd(context, ErrorMsg("error", "ItemData必填", Setting, ""));    
                    }
                    
                    #endregion                    
                    break;
                case "2":
                    #region 查詢目前等位人數及排隊序號
                    if (GS.MD5Check(SiteID + OrgName, ChkM))
                    {                        
                        if (context.Request.Params["ItemData"] != null && context.Request.Params["ItemData"] != "")
                        {
                            postf = JsonConvert.DeserializeObject<PostForm.BookingPost>(context.Request.Params["ItemData"]);
                            if (postf.StoreID != null && postf.StoreID != "") StoreID = postf.StoreID;                            
                            
                            ResponseWriteEnd(context, GetWaitData(Setting, StoreID));
                        }
                        else
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "ItemData必填", Setting, ""));
                        }
                        
                    }
                    else
                    {
                        ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting, ""));        //驗證碼錯誤
                    }
                    #endregion                    
                    break;
                case "3":
                    #region 查詢當日所有未入場/未跳號的預約資料
                    if (GS.MD5Check(SiteID + OrgName, ChkM))
                    {
                        if (context.Request.Params["ItemData"] != null && context.Request.Params["ItemData"] != "")
                        {
                            postf = JsonConvert.DeserializeObject<PostForm.BookingPost>(context.Request.Params["ItemData"]);
                            if (postf.StoreID != null && postf.StoreID != "") StoreID = postf.StoreID;

                            booking.root root = new booking.root();
                            List<booking.BookingData> BDatas = new List<booking.BookingData>();
                            String RegisterType = "";
                            using (SqlConnection conn = new SqlConnection(Setting))
                            {
                                conn.Open();

                                SqlCommand cmd = new SqlCommand("select bookingdate,Pno,tel,num,a.id,a.cdate,b.title as storetitle,c.title as seattitle,c.id as seatid,a.RegisterType from booking as a left join bookingStore as b on a.storeID=b.id left join bookingSeat as c on a.SeatID=c.id where (stat='1' or stat='2') and bookingdate between replace(CONVERT(VARCHAR(10), GETDATE(), 120),'-','/') + ' 00:00' and replace(CONVERT(VARCHAR(10), GETDATE(), 120),'-','/') + ' 23:59' and a.storeid=@storeid order by bookingdate,Pno,RegisterType desc", conn);
                                cmd.Parameters.Add(new SqlParameter("@storeid", StoreID));
                                SqlDataReader reader = cmd.ExecuteReader();
                                try
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            switch (reader[9].ToString())
                                            {
                                                case "1":
                                                    RegisterType = "現場預約候位";
                                                    break;
                                                case"2":
                                                    RegisterType = "網路預約候位";
                                                    break;
                                                case"3":
                                                    RegisterType = "網路訂位";
                                                    break;
                                                default:

                                                    break;
                                            }

                                            booking.BookingData BList = new booking.BookingData()
                                            {
                                                ID = reader[4].ToString(),
                                                Num = reader[3].ToString(),
                                                Tel = reader[2].ToString(),
                                                Pno = reader[1].ToString(),
                                                Time = reader[0].ToString().Substring(11),
                                                Cdate = reader[5].ToString(),
                                                StoreTitle = reader[6].ToString(),
                                                SeatTitle = reader[7].ToString(),
                                                RegisterType = RegisterType
                                            };
                                            BDatas.Add(BList);
                                        }
                                    }
                                }
                                finally
                                {
                                    reader.Close();
                                }
                            }
                            root.BookingDatas = BDatas;
                            ResponseWriteEnd(context, JsonConvert.SerializeObject(root));
                        }
                        else
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "ItemData必填", Setting, ""));
                        }
                    }
                    else
                    {
                        ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting, ""));        //驗證碼錯誤
                    }
                    #endregion
                    break;
                case "4":
                    #region 訂位:查詢每個時間桌位剩餘數量
                    if (context.Request.Params["ItemData"] != null && context.Request.Params["ItemData"] != "")
                    {
                        postf = JsonConvert.DeserializeObject<PostForm.BookingPost>(context.Request.Params["ItemData"]);

                        String Date = "";
                        String Hour = "";

                        if (postf.Date == null || postf.Date == "") ResponseWriteEnd(context, ErrorMsg("error", "Date必填", Setting, ""));
                        else Date = postf.Date;
                        if (postf.Hour != null && postf.Hour != "") Hour = postf.Hour;
                        if (postf.SeatID != null && postf.SeatID != "") SeatID = postf.SeatID;
                        if (postf.StoreID != null && postf.StoreID != "") StoreID = postf.StoreID;

                        if (GS.MD5Check(SiteID + OrgName + Date, ChkM))
                        {
                            ResponseWriteEnd(context, SearchStocks(Setting, StoreID, SeatID, Date, Hour));
                        }
                        else
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting, ""));        //驗證碼錯誤
                        }
                    }
                    else {
                        ResponseWriteEnd(context, ErrorMsg("error", "ItemData必填", Setting, ""));
                    }
                    
                    #endregion
                    break;
                case "5":
                    #region 查詢所有門市
                    if (GS.MD5Check(SiteID + OrgName, ChkM))
                    {
                        if (context.Request.Params["ItemData"] != null && context.Request.Params["ItemData"] != "")
                        {
                            postf = JsonConvert.DeserializeObject<PostForm.BookingPost>(context.Request.Params["ItemData"]);
                            if (postf.StoreID != null && postf.StoreID != "") StoreID = postf.StoreID;
                        }
                        ResponseWriteEnd(context, SearchStore(Setting, StoreID));
                    }
                    else
                    {
                        ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting, ""));        //驗證碼錯誤
                    }
                    #endregion
                    break;
                case "6":
                    #region 查詢所有桌次
                    if (GS.MD5Check(SiteID + OrgName, ChkM))
                    {
                        if (context.Request.Params["ItemData"] != null && context.Request.Params["ItemData"] != "")
                        {
                            postf = JsonConvert.DeserializeObject<PostForm.BookingPost>(context.Request.Params["ItemData"]);
                            if (postf.SeatID != null && postf.SeatID != "") SeatID = postf.SeatID;                            
                        }
                        ResponseWriteEnd(context, SearchSeat(Setting, SeatID));
                    }
                    else
                    {
                        ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting, ""));        //驗證碼錯誤
                    }
                    #endregion
                    break;
                case "7":
                    #region 查詢所有時段
                    if (GS.MD5Check(SiteID + OrgName, ChkM))
                    {
                        ResponseWriteEnd(context, SearchTime(Setting));
                    }
                    else
                    {
                        ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting, ""));        //驗證碼錯誤
                    }
                    #endregion
                    break;
                case "8":
                    #region 查詢歷史訂位
                    //select COUNT(*) as num,bookingdate,b.title from booking as a left join bookingStore as b on a.storeID=b.id where a.storeid='" & storeid & "' group by bookingdate,b.title
                    //select a.*,b.title as storetitle,c.title as seattitle,c.id as seatid from booking as a left join bookingStore as b on a.storeID=b.id left join bookingSeat as c on a.seatid=c.id where bookingdate = '2015/11/20 14:00' order by bookingdate desc,Pno,stat
                    if (GS.MD5Check(SiteID + OrgName, ChkM))
                    {
                        if (context.Request.Params["ItemData"] != null && context.Request.Params["ItemData"] != "")
                        {
                            postf = JsonConvert.DeserializeObject<PostForm.BookingPost>(context.Request.Params["ItemData"]);
                            if (postf.SeatID != null && postf.SeatID != "") SeatID = postf.SeatID;
                        }
                        ResponseWriteEnd(context, SearchSeat(Setting, SeatID));
                    }
                    else
                    {
                        ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting, ""));        //驗證碼錯誤
                    }

                    #endregion
                    break;
                case "9":
                    #region 現場點餐叫號登錄
                    if (context.Request.Params["ItemData"] != null && context.Request.Params["ItemData"] != "")
                    {
                        postf = JsonConvert.DeserializeObject<PostForm.BookingPost>(context.Request.Params["ItemData"]);

                        if (postf.Pno == null || postf.Pno == "") ResponseWriteEnd(context, ErrorMsg("error", "Pno必填", Setting, ""));
                        if (postf.StoreID == null || postf.StoreID == "") ResponseWriteEnd(context, ErrorMsg("error", "StoreID必填", Setting, ""));

                        String Pno = postf.Pno;
                        StoreID = postf.StoreID;

                        if (GS.MD5Check(SiteID + OrgName, ChkM))
                        {
                            InsertBooking(Setting, "", DateTime.Now.ToString("yyyy/MM/dd hh:mm"), StoreID, "4", "", "", Pno);        //不要動，Pno會跑掉！
                        }
                        else
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting, ""));        //驗證碼錯誤
                        }
                    }
                    else
                    {
                        ResponseWriteEnd(context, ErrorMsg("error", "ItemData必填", Setting, ""));
                    }
                    #endregion                    
                    break;
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
        private String ErrorMsg(String RspnCode, String RspnMsg,String Setting,String Pno)
        {
            if (Setting != "") {
                InsertLog(Setting, "booking error", "", RspnMsg);
            }

            Library.booking.RootObject root = new Library.booking.RootObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            root.Pno = Pno;
            return JsonConvert.SerializeObject(root);
            
        }
        #endregion

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }

        #region 確認是否已訂位過
        private bool ChkBooking(String Setting, String Tel, String Time, String StoreID, String SeatID)
        {
            String DetailStr = "select * from booking where tel = " + Tel + " and bookingdate = " + Time + " and storeid=" + StoreID + " and seatid=" + SeatID + " and (stat='1' or stat='2')";
            InsertLog(Setting, "確認是否已訂位過", "", DetailStr);

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from booking where tel = @tel and bookingdate = @bookingdate and storeid=@storeid and seatid=@seatid and (stat='1' or stat='2')", conn);
                cmd.Parameters.Add(new SqlParameter("@tel", Tel));
                cmd.Parameters.Add(new SqlParameter("@bookingdate", Time));
                cmd.Parameters.Add(new SqlParameter("@storeid", StoreID));
                cmd.Parameters.Add(new SqlParameter("@seatid", SeatID));

                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        #endregion

        #region 發送簡訊
        private String SendSMS(String Tel, String Time, String Pno, String Setting, String StoreID, String SiteID, String RegisterType, String SeatID, String ArrivalTime) 
        {
            String SMSID = "";
            String SMSPwd = "";
            String StoreName = "";
            GetStr GS = new GetStr();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select SMSID,SMSPwd from head", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read()) 
                        {
                            SMSID = reader[0].ToString();
                            SMSPwd = reader[1].ToString();
                        }
                    }                    
                }
                finally
                {
                    reader.Close();
                }
            }
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select title from bookingStore where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", StoreID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            StoreName = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            if (SMSID != "" && SMSPwd != "")
            {                
                GoogleUrlShortener GoogleUrlShortener = new GoogleUrlShortener("AIzaSyBtSg_nn1z7ppBFPVfaS6ySH2M5y49IA7Q");
                String ShortURL = GoogleUrlShortener.Shorten("http://" + GS.GetDefaultURL(SiteID) + "/tw/" + GS.GetUseModuleURL(Setting, "28") + "&id=" + StoreID + "&seatid=" + SeatID);

                String SMSStr = "";

                switch (RegisterType) { 
                    case "1":   //線場預約簡訊
                        //SMSStr = "您於" + DateTime.Now.ToString("MM/dd HH:mm") + StoreName + "預約" + GetSiteName(SeatID, Setting) + "．" + SeatID + Pno.PadLeft(2, '0') + "號．過號不保留名額！現場候位狀態" + ShortURL;
                        SMSStr = "您於" + DateTime.Now.ToString("MM/dd HH:mm") + StoreName + "預約" + GetSiteName(SeatID, Setting) + "．" + Pno + "號．過號不保留名額！現場候位狀態" + ShortURL;
                        break;
                    case"2":    //網路預約簡訊                        
                        //SMSStr = "您線上預約" + StoreName + Time.Substring(5) + GetSiteName(SeatID, Setting) + "．編號" + SeatID + Pno.PadLeft(2, '0') + "．過號不保留！候位狀況" + ShortURL;
                        SMSStr = "您線上預約" + StoreName + Time.Substring(5) + GetSiteName(SeatID, Setting) + "．編號" + Pno + "．過號不保留！候位狀況" + ShortURL;
                        break;
                    case "3":    //網路訂位簡訊
                        //SMSStr = "您線上訂位" + StoreName + Time.Substring(5, 5) + " " + ArrivalTime + GetSiteName(SeatID, Setting) + "．編號" + SeatID + Pno.PadLeft(2, '0') + "．座位保留10分鐘，逾時視同放棄";
                        SMSStr = "您線上訂位" + StoreName + Time.Substring(5, 5) + " " + ArrivalTime + GetSiteName(SeatID, Setting) + "．編號" + Pno + "．座位保留10分鐘，逾時視同放棄";
                        break;
                }
                
                String URL = @"http://sms-get.com/api_send.php?username=" + SMSID + "&password=" + SMSPwd + "&method=1&sms_msg=" + GS.StringToUTF8(SMSStr) + "&phone=" + Tel + "&send_date=&hour=&min=";
                Uri urlCheck = new Uri(URL);
                WebRequest request = WebRequest.Create(urlCheck);
                request.Timeout = 10000;
                using (WebResponse wr = request.GetResponse())
                {
                    using (StreamReader myStreamReader = new StreamReader(wr.GetResponseStream()))
                    {
                        SMS.SMSData postf = JsonConvert.DeserializeObject<SMS.SMSData>(myStreamReader.ReadToEnd());
                        if (postf.error_code == "000")
                        {
                            using (SqlConnection conn = new SqlConnection(Setting))
                            {
                                conn.Open();
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "sp_EditBooking";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@stat", "2"));
                                cmd.Parameters.Add(new SqlParameter("@smsstat", postf.stats));
                                cmd.Parameters.Add(new SqlParameter("@errorcode", postf.error_code));
                                cmd.Parameters.Add(new SqlParameter("@msg", GS.GetSMSErrorMsg(postf.error_code)));
                                cmd.Parameters.Add(new SqlParameter("@seatid", SeatID));
                                cmd.Parameters.Add(new SqlParameter("@tel", Tel));
                                cmd.Parameters.Add(new SqlParameter("@bookingdate", Time));
                                cmd.Parameters.Add(new SqlParameter("@Pno", Pno));
                                cmd.Parameters.Add(new SqlParameter("@SMSID", postf.error_msg.Split('|')[0].ToString()));
                                cmd.Parameters.Add(new SqlParameter("@SMSTime", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));
                                cmd.Parameters.Add(new SqlParameter("@storeID", StoreID));
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else 
                        {
                            using (SqlConnection conn = new SqlConnection(Setting))
                            {
                                conn.Open();
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "sp_EditBooking";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@stat", "1"));
                                cmd.Parameters.Add(new SqlParameter("@smsstat", postf.stats));
                                cmd.Parameters.Add(new SqlParameter("@errorcode", postf.error_code));
                                cmd.Parameters.Add(new SqlParameter("@msg", GS.GetSMSErrorMsg(postf.error_code)));
                                cmd.Parameters.Add(new SqlParameter("@seatid", SeatID));
                                cmd.Parameters.Add(new SqlParameter("@tel", Tel));
                                cmd.Parameters.Add(new SqlParameter("@bookingdate", Time));
                                cmd.Parameters.Add(new SqlParameter("@Pno", Pno));
                                cmd.Parameters.Add(new SqlParameter("@SMSID", postf.error_msg.Split('|')[0].ToString()));
                                cmd.Parameters.Add(new SqlParameter("@SMSTime", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));
                                cmd.Parameters.Add(new SqlParameter("@storeID", StoreID));
                                cmd.ExecuteNonQuery();
                            }
                        }
                        
                        return postf.error_code;
                    }
                }
            }
            else 
            {
                return "";
            }
        }
        #endregion

        #region 查詢目前等位人數及排隊序號
        private String GetWaitData(String Setting, String StoreID)
        {
            #region 查詢目前時段
            String Hour1 = "";
            String Min1 = "";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("select hour1,min1 from bookingTime where SUBSTRING(CONVERT(VARCHAR(20), GETDATE(), 120), 12, 2) + ':' + SUBSTRING(CONVERT(VARCHAR(20), GETDATE(), 120), 15, 2) between hour1+':'+min1 and hour2+':'+min2", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Hour1 = reader[0].ToString();
                            Min1 = reader[1].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion
            booking.root3 root = new booking.root3();
            

            if (Hour1 != "" && Min1 != "")
            {                
                List<booking.WaitData> WD = new List<booking.WaitData>();

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("select id,title from bookingSeat where disp_opt='Y'", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                booking.WaitData WDList = new booking.WaitData
                                {
                                    BookingTime = Hour1 + ":" + Min1,
                                    Pno = GetMaxPno(Setting, DateTime.Now.ToString("yyyy/MM/dd") + " " + Hour1 + ":" + Min1, StoreID, reader[0].ToString()),
                                    SeatID = reader[0].ToString(),
                                    SeatTitle = reader[1].ToString(),
                                    TotalNum = GetWaitNum(Setting, DateTime.Now.ToString("yyyy/MM/dd") + " " + Hour1 + ":" + Min1, StoreID, reader[0].ToString())                                    
                                };
                                WD.Add(WDList);
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                root.Pno = GetMaxPno(Setting, "", StoreID, "");
                root.WaitData = WD;
                
            }
            return JsonConvert.SerializeObject(root);
            
        }

        #endregion

        #region 某場次,某分店最後入場序號
        private String GetMaxPno(String Setting, String Time, String StoreID, String SeatID)
        {
            String Pno = "";

            if (SeatID == "" && Time == "")         //現場點餐叫號
            {
                String DetailStr = "select isnull(Max(Pno),'0') from booking where RegisterType='4' and bookingdate between '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00' and '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59' and storeid='" + StoreID + "'";
                InsertLog(Setting, "現場點餐最後入場序號", "", DetailStr);

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("select isnull(Max(Pno),'0') from booking where RegisterType='4' and bookingdate between @bookingdate1 and @bookingdate2 and storeid=@storeid", conn);
                    cmd.Parameters.Add(new SqlParameter("@bookingdate1", DateTime.Now.ToString("yyyy/MM/dd") + " 00:00"));
                    cmd.Parameters.Add(new SqlParameter("@bookingdate2", DateTime.Now.ToString("yyyy/MM/dd") + " 23:59"));
                    cmd.Parameters.Add(new SqlParameter("@storeid", StoreID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                //Pno = SeatID + reader[0].ToString().PadLeft(2, '0');
                                Pno = reader[0].ToString();
                            }
                        }
                        if (Pno == "0")
                        {
                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();

                                SqlCommand cmd2 = new SqlCommand("select BookingStartNo from bookingStore where id=@storeid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@storeid", StoreID));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            Pno = reader2[0].ToString();
                                        }
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            else 
            {
                String DetailStr = "select isnull(Max(Pno),'0') from booking where bookingdate=" + Time + " and (Stat='3' or Stat='4') and storeid=" + StoreID + " and seatid=" + SeatID;
                InsertLog(Setting, "最後入場序號", "", DetailStr);

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("select isnull(Max(Pno),'0') from booking where bookingdate=@bookingdate and (Stat='3' or Stat='4') and storeid=@storeid and seatid=@seatid", conn);
                    cmd.Parameters.Add(new SqlParameter("@bookingdate", Time));
                    cmd.Parameters.Add(new SqlParameter("@storeid", StoreID));
                    cmd.Parameters.Add(new SqlParameter("@seatid", SeatID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                //Pno = SeatID + reader[0].ToString().PadLeft(2, '0');
                                Pno = reader[0].ToString();
                            }
                        }
                        if (Pno == "0")
                        {

                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();

                                SqlCommand cmd2 = new SqlCommand("select startnum from bookingSeat where id=@seatid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@seatid", SeatID));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            Pno = reader2[0].ToString();
                                        }
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }

            

            return Pno;
        }

        #endregion

        #region 某場次等待總人數
        private String GetWaitNum(String Setting, String Time, String StoreID, String SeatID) 
        {
            String DetailStr = "select COUNT(*) from booking where bookingdate=" + Time + " and (Stat='2' or Stat='1') and storeid=" + StoreID + " and seatid=" + SeatID;
            InsertLog(Setting, "某場次等待總人數", "", DetailStr);

            String TotalNum = "";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("select COUNT(*) from booking where bookingdate=@bookingdate and (Stat='2' or Stat='1') and storeid=@storeid and seatid=@seatid", conn);
                cmd.Parameters.Add(new SqlParameter("@bookingdate", Time));
                cmd.Parameters.Add(new SqlParameter("@storeid", StoreID));
                cmd.Parameters.Add(new SqlParameter("@seatid", SeatID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            TotalNum = reader[0].ToString();
                        }
                    }
                    else 
                    {
                        TotalNum = "0";
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return TotalNum;

        }
        #endregion

        #region 取得座位名稱
        private String GetSiteName(String SeatID,String Setting) {

            String SiteName = "";
            
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select title from bookingSeat where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", SeatID));
                
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            SiteName = reader[0].ToString();
                        }
                    }
                }
                finally 
                { 
                    reader.Close();
                }
            }
            return SiteName;
        }
        #endregion

        #region insert booking
        private Int32 InsertBooking(String Setting, String Tel, String Time, String StoreID, String RegisterType, String SeatID, String ArrivalTime, String Pno1)
        {
            Int32 Pno = 0;
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_AddBooking";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@tel", Tel));                
                cmd.Parameters.Add(new SqlParameter("@bookingdate", Time));
                cmd.Parameters.Add(new SqlParameter("@stordid", StoreID));
                cmd.Parameters.Add(new SqlParameter("@seatid", SeatID));
                cmd.Parameters.Add(new SqlParameter("@RegisterType", RegisterType));     //1=現場預約候位;2=網路預約候位;3=網路訂位;4=現場點餐叫號
                cmd.Parameters.Add(new SqlParameter("@ArrivalTime", ArrivalTime));
                cmd.Parameters.Add(new SqlParameter("@Pno1", Pno1));
                SqlParameter SPOutput = cmd.Parameters.Add("@Pno", SqlDbType.Int);
                SPOutput.Direction = ParameterDirection.Output;
                try
                {
                    cmd.ExecuteNonQuery();
                    Pno = Convert.ToInt32(SPOutput.Value);
                }
                catch
                {
                    Pno = 0;
                }
            }
            String DetailStr = "sp_AddBooking " + Tel + "," + Time + "," + StoreID + "," + SeatID + "," + RegisterType + "," + ArrivalTime + "," + Pno1;
            InsertLog(Setting, "新增booking", Pno.ToString(), DetailStr);
            return Pno;
        }
        #endregion

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

        #region 訂位:查詢每個時間桌位剩餘數量
        private String SearchStocks(String Setting, String StoreID, String SeatID, String Date, String Hour)
        {
            booking.root2 root = new booking.root2();
            List<booking.BookingStore> BStores = new List<booking.BookingStore>();
            List<booking.BookingSeat> BSeats = new List<booking.BookingSeat>();
            List<booking.BookingData2> BData2 = new List<booking.BookingData2>();
            int stocks = 0;

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                if (StoreID != "")
                {
                    cmd = new SqlCommand("select id,title from bookingStore where disp_opt='Y' and id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", StoreID));
                }
                else 
                {
                    cmd = new SqlCommand("select id,title from bookingStore where disp_opt='Y'", conn);
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            #region 座位
                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;

                                if (SeatID != "")
                                {
                                    cmd2 = new SqlCommand("select ID,title,stocks from bookingseat where disp_opt='Y' and id=@id", conn2);
                                    cmd2.Parameters.Add(new SqlParameter("@id", SeatID));
                                }
                                else {
                                    cmd2 = new SqlCommand("select ID,title,stocks from bookingseat where disp_opt='Y'", conn2);                                
                                }
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            #region 時段
                                            using (SqlConnection conn3 = new SqlConnection(Setting))
                                            {
                                                conn3.Open();
                                                SqlCommand cmd3;
                                                if (Hour != "")
                                                {
                                                    cmd3 = new SqlCommand("select title, hour1+':'+min1 as time1, hour2+':'+min2 as time2 from dbo.bookingTime where hour1+':'+min1=@hour", conn3);
                                                    cmd3.Parameters.Add(new SqlParameter("@hour", Hour));
                                                }
                                                else 
                                                {
                                                    cmd3 = new SqlCommand("select title, hour1+':'+min1 as time1, hour2+':'+min2 as time2 from dbo.bookingTime", conn3);                                                
                                                }
                                                SqlDataReader reader3 = cmd3.ExecuteReader();
                                                try
                                                {
                                                    if (reader3.HasRows)
                                                    {
                                                        while (reader3.Read())
                                                        {
                                                            stocks = Convert.ToInt32(reader2[2].ToString());
                                                            using (SqlConnection conn4 = new SqlConnection(Setting))
                                                            {
                                                                conn4.Open();

                                                                SqlCommand cmd4 = new SqlCommand("select bookingnum from dbo.BookingCount where registerType='3' and bookingdate=@bookingdate and storeid=@storeid and seatid=@seatid", conn4);
                                                                cmd4.Parameters.Add(new SqlParameter("@bookingdate", Date + " " + reader3[1].ToString()));
                                                                cmd4.Parameters.Add(new SqlParameter("@storeid", reader[0].ToString()));
                                                                cmd4.Parameters.Add(new SqlParameter("@seatid", reader2[0].ToString()));
                                                                SqlDataReader reader4 = cmd4.ExecuteReader();
                                                                try
                                                                {
                                                                    if (reader4.HasRows)
                                                                    {
                                                                        while (reader4.Read())
                                                                        {
                                                                            stocks = stocks - Convert.ToInt32(reader4[0].ToString());
                                                                        }
                                                                    }
                                                                }
                                                                finally 
                                                                {
                                                                    reader4.Close();
                                                                }
                                                            } 
                                                           
                                                            booking.BookingData2 BData2List = new booking.BookingData2()
                                                            {
                                                                Title = reader3[0].ToString(),
                                                                StartTime = reader3[1].ToString(),
                                                                EndTime = reader3[2].ToString(),                                                                
                                                                Stocks = stocks
                                                            };
                                                            BData2.Add(BData2List);
                                                        }
                                                    }
                                                }
                                                finally
                                                {
                                                    reader3.Close();
                                                }
                                            }
                                            #endregion

                                            booking.BookingSeat BSeatList = new booking.BookingSeat()
                                            {
                                                SeatID = reader2[0].ToString(),
                                                SeatTitle = reader2[1].ToString(),
                                                BookingData2 = BData2
                                            };
                                            BSeats.Add(BSeatList);
                                            BData2 = new List<booking.BookingData2>();
                                        }
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }
                            #endregion

                            booking.BookingStore BStoreList = new booking.BookingStore()
                            {
                                StoreID = reader[0].ToString(),
                                StoreTitle = reader[1].ToString(),
                                BookingSeat = BSeats
                            };
                            BStores.Add(BStoreList);
                            BSeats = new List<booking.BookingSeat>();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            root.BookingStore = BStores;
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 查詢所有門市
        private String SearchStore(String Setting,String StoreID) 
        {
            booking.root2 root = new booking.root2();
            List<booking.BookingStore> BStores = new List<booking.BookingStore>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                if (StoreID != "")
                {
                    cmd = new SqlCommand("select id,title from bookingStore where disp_opt='Y' and id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", StoreID));
                }
                else
                {
                    cmd = new SqlCommand("select id,title from bookingStore where disp_opt='Y'", conn);
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            booking.BookingStore BStoreList = new booking.BookingStore()
                            {
                                StoreID = reader[0].ToString(),
                                StoreTitle = reader[1].ToString(),
                                BookingSeat = null
                            };
                            BStores.Add(BStoreList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            root.BookingStore = BStores;
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 查詢所有桌號
        private String SearchSeat(String Setting, String SeatID)
        {
            booking.BookingStore root = new booking.BookingStore();
            List<booking.BookingSeat> BSeats = new List<booking.BookingSeat>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                if (SeatID != "")
                {
                    cmd = new SqlCommand("select id,title from bookingSeat where disp_opt='Y' and id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", SeatID));
                }
                else
                {
                    cmd = new SqlCommand("select id,title from bookingSeat where disp_opt='Y'", conn);
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            booking.BookingSeat BSeatList = new booking.BookingSeat()
                            {
                                SeatID = reader[0].ToString(),
                                SeatTitle = reader[1].ToString(),
                                BookingData2 = null
                            };
                            BSeats.Add(BSeatList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            root.BookingSeat = BSeats;
            root.StoreID = null;
            root.StoreTitle = null;
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 查詢所有時段
        private String SearchTime(String Setting)
        {
            booking.BookingSeat root = new booking.BookingSeat();
            List<booking.BookingData2> BDatas = new List<booking.BookingData2>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select title, hour1+':'+min1 as time1, hour2+':'+min2 as time2 from dbo.bookingTime", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            booking.BookingData2 BList = new booking.BookingData2()
                            {
                                Title = reader[0].ToString(),
                                StartTime = reader[1].ToString(),
                                EndTime = reader[2].ToString(),
                                Stocks = 0
                            };
                            BDatas.Add(BList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            root.BookingData2 = BDatas;
            root.SeatID = null;
            root.SeatTitle = null;
            return JsonConvert.SerializeObject(root);
        }
        #endregion
    }
}