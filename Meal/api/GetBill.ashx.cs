using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Meal.Library;
using System.Net;
using System.IO;
using System.Text;

namespace Meal.api
{
    /// <summary>
    /// GetBill 的摘要描述
    /// </summary>
    public class GetBill : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Form["VerCode"] == null) ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填", ""));

            if (context.Request.Form["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Form["VerCode"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填", ""));

            String ChkM = context.Request.Form["CheckM"].ToString();
            String VerCode = context.Request.Form["VerCode"].ToString();
            String Type = context.Request.Form["Type"].ToString();

            GetMealStr GS = new GetMealStr();
            if (GS.MD5Check(Type + VerCode, ChkM))
            {
                String Orgname = GS.GetOrgName("{" + VerCode + "}");

                if (Orgname == "") ResponseWriteEnd(context, ErrorMsg("error", "查無Orgname", ""));

                String Setting = GS.GetSetting(Orgname);

                if (context.Request.Form["Items"] == null || context.Request.Form["Items"] == "") ResponseWriteEnd(context, ErrorMsg("error", "Items必填", Setting));
                Bill.Items bill = null;
                try
                {
                    bill = JsonConvert.DeserializeObject<Bill.Items>(context.Request.Form["Items"]);
                }
                catch 
                {
                    ResponseWriteEnd(context, ErrorMsg("error", "Json格式不正確", Setting));
                }
                

                switch (Type)
                {
                    case "1":   //取得訂單資料(桌號未結帳)
                        if (bill.ShopID == null || bill.ShopID == "") ResponseWriteEnd(context, ErrorMsg("error", "ShopID必填", Setting));
                        if (bill.TableID == null || bill.TableID == "") ResponseWriteEnd(context, ErrorMsg("error", "TableID必填", Setting));

                        ResponseWriteEnd(context, GetBillData(bill.ShopID, bill.TableID, Setting, "", "", "", "", "", ""));
                        break;
                    case "2":   //修改訂單狀態
                        if (bill.Stat == null || bill.Stat == "") ResponseWriteEnd(context, ErrorMsg("error", "Stat必填", Setting));

                        if (bill.OrderID == null || bill.OrderID == "") 
                        {
                            if (bill.ShopID == null || bill.ShopID == "") ResponseWriteEnd(context, ErrorMsg("error", "ShopID必填", Setting));
                            if (bill.TableID == null || bill.TableID == "") ResponseWriteEnd(context, ErrorMsg("error", "TableID必填", Setting));
                            
                            //ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", Setting));
                        }

                        UpdateOrderStat(bill.ShopID, bill.TableID, Setting, bill.Stat, bill.OrderID);

                        #region 紀錄現場收款金額
                        if (bill.Stat == "7" && bill.Cash != null)
                        {
                            foreach (Bill.Cash cash in bill.Cash) 
                            {
                                CashIcome(bill.ShopID, bill.TableID, Setting, bill.OrderID, cash.Income, cash.Change, cash.Redeem, cash.type);
                            }
                        }
                        #endregion
                        
                        ResponseWriteEnd(context, "success");
                        break;
                    case "3":   //取消訂單項目

                        if (bill.OrderID == null || bill.OrderID == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", Setting));
                        if (bill.SerNo == null || bill.SerNo.Length <= 0) ResponseWriteEnd(context, ErrorMsg("error", "SerNo必填", Setting));

                        CancelOrder(Setting, bill.OrderID, bill.SerNo, bill.CancelChk);
                        ResponseWriteEnd(context, "success");

                        break;
                    case "4":   //備餐確認

                        if (bill.OrderID == null || bill.OrderID == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", Setting));
                        if (bill.SerNo == null) ResponseWriteEnd(context, ErrorMsg("error", "SerNo必填", Setting));
                        if (bill.Track == null) ResponseWriteEnd(context, ErrorMsg("error", "Track必填", Setting));

                        CheckMeal(Setting, bill.OrderID, bill.SerNo, "Y", bill.Track);
                        ResponseWriteEnd(context, "success");

                        break;
                    case "5":   //取得訂單資料(使用訂單號碼查詢)
                        if (bill.OrderID == null || bill.OrderID == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", Setting));
                        ResponseWriteEnd(context, GetBillData("", "", Setting, bill.OrderID, "", "", "", "", ""));
                        break;

                    case "6":   //儲存訂單
                        if (bill.OrderData == null) ResponseWriteEnd(context, ErrorMsg("error", "OrderData必填", Setting));
                        
                        Bill.Items ItemsData = JsonConvert.DeserializeObject<Bill.Items>(SaveOrder(Setting, bill.OrderData));
                        String OrderNo = ItemsData.OrderID;
                        
                        if (OrderNo == "0")
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "訂單新增失敗", Setting));
                        }
                        else {
                            ResponseWriteEnd(context, JsonConvert.SerializeObject(ItemsData));
                        }
                        
                        break;
                    case "7":   //列印訂單註記
                        if (bill.OrderID == null || bill.OrderID == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", Setting));
                        PringTag(Setting, bill.OrderID);
                        ResponseWriteEnd(context, "success");
                        break;   
                    case "8":   //抓退訂資料
                        if (bill.ShopID == null || bill.ShopID == "") ResponseWriteEnd(context, ErrorMsg("error", "ShopID必填", Setting));
                        if (bill.TableID == null || bill.TableID == "") ResponseWriteEnd(context, ErrorMsg("error", "TableID必填", Setting));

                        ResponseWriteEnd(context, GetCancelOrder(bill.TableID, bill.ShopID, Setting));
                        break;
                    case "9":   //取得訂單資料(使用訂位號碼查詢)
                        if (bill.BookingID == null || bill.BookingID == "") ResponseWriteEnd(context, ErrorMsg("error", "BookingID必填", Setting));
                        ResponseWriteEnd(context, GetBillData("", "", Setting, "", bill.BookingID, "", "", "", ""));
                        break;
                    case "10":   //取得訂單資料(使用日期區間查詢)
                        if (bill.StartDate == null || bill.StartDate == "") ResponseWriteEnd(context, ErrorMsg("error", "StartDate必填", Setting));
                        if (bill.EndDate == null || bill.EndDate == "") ResponseWriteEnd(context, ErrorMsg("error", "EndDate必填", Setting));
                        ResponseWriteEnd(context, GetBillData("", "", Setting, "", "", bill.StartDate, bill.EndDate, "", ""));
                        break;
                    case "11":   //取得N筆有訂單資料的日期
                        if (bill.num == null || bill.num == "") ResponseWriteEnd(context, ErrorMsg("error", "num必填", Setting));
                        ResponseWriteEnd(context, GetOrderDate(Setting, bill.num));
                        break;
                    case "12":   //取得訂單資料(使用取餐方式查詢)
                        if (bill.TakeMealType == null || bill.TakeMealType == "") ResponseWriteEnd(context, ErrorMsg("error", "TakeMealType必填", Setting));
                        ResponseWriteEnd(context, GetBillData("", "", Setting, "", "", "", "", bill.TakeMealType, ""));
                        break;
                    case "13":   //出餐確認

                        if (bill.OrderID == null || bill.OrderID == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", Setting));
                        if (bill.SerNo == null) ResponseWriteEnd(context, ErrorMsg("error", "SerNo必填", Setting));
                        if (bill.Track == null) ResponseWriteEnd(context, ErrorMsg("error", "Track必填", Setting));

                        CheckMeal(Setting, bill.OrderID, bill.SerNo, "S", bill.Track);
                        ResponseWriteEnd(context, "success");

                        break;
                    case "14":  //出餐列表(依桌號查詢)
                        if (bill.ShopID == null || bill.ShopID == "") ResponseWriteEnd(context, ErrorMsg("error", "ShopID必填", Setting));
                        if (bill.TableID == null || bill.TableID == "") ResponseWriteEnd(context, ErrorMsg("error", "TableID必填", Setting));

                        ResponseWriteEnd(context, MealReadyList(Setting, bill.ShopID, bill.TableID));
                        break;
                    case "15":   //取得訂單資料(使用會員編號查詢)
                        if (bill.TakeMealType == null || bill.TakeMealType == "") ResponseWriteEnd(context, ErrorMsg("error", "TakeMealType必填", Setting));
                        ResponseWriteEnd(context, GetBillData("", "", Setting, "", "", "", "", "", bill.MemberID));
                        break;

                    case "16":  //儲存悠遊卡付款資料
                        
                        if (bill.OrderID == null || bill.OrderID == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", Setting));
                        if (bill.EasyCard == null) ResponseWriteEnd(context, ErrorMsg("error", "EasyCard必填", Setting));

                        AddEasyCardData(Setting, bill.OrderID, bill.EasyCard);
                        ResponseWriteEnd(context, "success");
                        
                        break;
                }
            }
            else
            {
                ResponseWriteEnd(context, ErrorMsg("error", "檢查碼錯誤", ""));
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
                InsertLog(Setting, "bill error", "", RspnMsg);
            }

            Bill.ErrorObject root = new Bill.ErrorObject();
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

        #region 取得N筆有訂單的日期
        private String GetOrderDate(String Setting,String Num) 
        {

            Bill.DateObject root = new Bill.DateObject();
            Bill.OrderDate Orderdate = new Bill.OrderDate();
            List<Bill.OrderDate> OrderdateList = new List<Bill.OrderDate>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("SELECT  * from ( select ROW_NUMBER() OVER(ORDER BY CONVERT(varchar(100), cdate, 111) desc) AS RowNum, CONVERT(varchar(100), cdate, 111) as date,COUNT(*) as num from orders_hd group by CONVERT(varchar(100), cdate, 111) ) as newtable WHERE RowNum >= 1 AND RowNum <= @RowNum", conn);
                cmd.Parameters.Add(new SqlParameter("@RowNum", Num));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Orderdate = new Bill.OrderDate
                            {
                                Date = reader["date"].ToString(),
                                num = reader["num"].ToString()
                            };

                            OrderdateList.Add(Orderdate);
                        }
                    }
                }
                finally { reader.Close(); }
            }

            root.OrderDate = OrderdateList;
            return JsonConvert.SerializeObject(root);
        }
        #endregion


        #region 取得出餐列表
        private String MealReadyList(String Setting, String ShopID, String TableID)
        {
            List<Bill.MealReady> MR = new List<Bill.MealReady>();
            Bill.MealReady MRList;

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                String Str_sql = "select a.order_no,a.ser_no,a.prod_name,a.track from orders as a left join orders_hd as b on a.order_no=b.id where meal_ready='Y' and b.TableID=@tableid and b.ShopID=@shopid and b.state not in ('3','7','8') order by a.order_no";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@tableid", TableID));
                cmd.Parameters.Add(new SqlParameter("@shopid", ShopID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            MRList = new Bill.MealReady
                            {
                                OrderID = reader["order_no"].ToString(),
                                SerNo = reader["ser_no"].ToString(),
                                Title = reader["prod_name"].ToString(),
                                Track = reader["track"].ToString()
                            };
                            MR.Add(MRList);
                        }
                    }
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(MR);
        }
        #endregion

        #region 退訂單
        private void CancelOrder(String Setting, String OrderID, String[] SerNo, String CancelChk)
        {
            if (CancelChk == "Y")
            {
                int statcount = 0;
                for (int i = 0; i < SerNo.Length; i++)
                {
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd;
                        cmd = new SqlCommand("update orders set stat='Y',cancel_date=@cancel_date where order_no=@order_no and ser_no=@ser_no", conn);
                        cmd.Parameters.Add(new SqlParameter("@cancel_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                        cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                        cmd.Parameters.Add(new SqlParameter("@ser_no", SerNo[i].ToString()));
                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("select (amt-discount) as amt from orders where order_no=@order_no and ser_no=@ser_no", conn);
                        cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                        cmd.Parameters.Add(new SqlParameter("@ser_no", SerNo[i].ToString()));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    statcount = statcount + Convert.ToInt32(reader[0].ToString());
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }

                #region 修改訂單總金額
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;
                    cmd = new SqlCommand("update orders_hd set amt=amt-@amt where id=@order_no", conn);
                    cmd.Parameters.Add(new SqlParameter("@amt", statcount));
                    cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                    cmd.ExecuteNonQuery();
                }
                #endregion
            }
            else
            {
                #region 更新訂單表身
                for (int i = 0; i < SerNo.Length; i++)
                {
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd;
                        cmd = new SqlCommand("update orders set stat='Y' where order_no=@order_no and ser_no=@ser_no", conn);
                        cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                        cmd.Parameters.Add(new SqlParameter("@ser_no", SerNo[i].ToString()));
                        cmd.ExecuteNonQuery();
                    }
                }
                #endregion
            }
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

        #region 列印註記
        private void PringTag(String Setting, String OrderID) 
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("update orders_hd set print_tag=@print_tag where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@print_tag", DateTime.Now.ToString("yyyyMMddHHmm")));
                cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region 取得訂單資料
        private String GetBillData(String ShopID, String TableID, String Setting, String OrderID, String BookingID, String StartDate, String EndDate, String TakeMealType,String MemID)
        {
            Bill.RootObject root = new Bill.RootObject();
            Bill.Bills bill = new Bill.Bills();
            Bill.Item item = new Bill.Item();
            Bill.Detail detail = new Bill.Detail();
            Bill.Cash cash = new Bill.Cash();
            Bill.EasyCard Easycard = new Bill.EasyCard();

            List<Bill.Bills> BillList = new List<Bill.Bills>();
            List<Bill.Item> ItemList = new List<Bill.Item>();
            List<Bill.Detail> DetailList = new List<Bill.Detail>();
            List<Bill.Cash> CashList = new List<Bill.Cash>();
            List<Bill.EasyCard> EasycardList = new List<Bill.EasyCard>();

            GetMealStr GS = new GetMealStr();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                if (OrderID == "")
                {
                    if (BookingID == "")
                    {
                        if (TableID == "0")
                        {
                            cmd = new SqlCommand("select id,TakeMealType,amt,print_tag,state,tableid from orders_hd where shopid=@shopid and convert(nvarchar(10), cdate, 111) = convert(nvarchar(10), getdate(), 111)", conn);
                            cmd.Parameters.Add(new SqlParameter("@shopid", ShopID));
                        }
                        else
                        {
                            cmd = new SqlCommand("select id,TakeMealType,amt,print_tag,state,tableid from orders_hd where shopid=@shopid and tableid=@tableid and convert(nvarchar(10), cdate, 111) = convert(nvarchar(10), getdate(), 111)", conn);
                            cmd.Parameters.Add(new SqlParameter("@shopid", ShopID));
                            cmd.Parameters.Add(new SqlParameter("@tableid", TableID));
                        }
                    }
                    else 
                    {
                        cmd = new SqlCommand("select id,TakeMealType,amt,print_tag,state,tableid from orders_hd where bookingid=@bookingid", conn);
                        cmd.Parameters.Add(new SqlParameter("@bookingid", BookingID));
                    }

                    if (TakeMealType != "") 
                    {
                        cmd = new SqlCommand("select id,TakeMealType,amt,print_tag,state,tableid from orders_hd where TakeMealType=@TakeMealType", conn);
                        cmd.Parameters.Add(new SqlParameter("@TakeMealType", TakeMealType));
                    }

                    if (StartDate != "" && EndDate != "")
                    {
                        cmd = new SqlCommand("select id,TakeMealType,amt,print_tag,state,tableid from orders_hd where CONVERT(varchar(10), cdate, 111) between @StartDate and @EndDate", conn);
                        cmd.Parameters.Add(new SqlParameter("@StartDate", StartDate));
                        cmd.Parameters.Add(new SqlParameter("@EndDate", EndDate));
                    }

                    if (MemID != "")
                    {
                        cmd = new SqlCommand("select id,TakeMealType,amt,print_tag,state,tableid from orders_hd where mem_id=@mem_id", conn);
                        cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                        
                    }
                }
                else
                {
                    cmd = new SqlCommand("select id,TakeMealType,amt,print_tag,state,tableid from orders_hd where id=@orderid", conn);
                    cmd.Parameters.Add(new SqlParameter("@orderid", OrderID));
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            CashList = new List<Bill.Cash>();

                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("select * from orders_cash where order_no=@orderid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@orderid", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            cash = new Bill.Cash
                                            {
                                                Income = reader2["income"].ToString(),
                                                Change = reader2["change"].ToString(),
                                                Redeem = reader2["redeem"].ToString(),
                                                type = reader2["type"].ToString()
                                            };
                                            CashList.Add(cash);
                                        }
                                    }
                                }
                                finally { reader2.Close(); }

                            }

                            EasycardList = new List<Bill.EasyCard>();

                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("select * from orders_easycard where order_no=@orderid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@orderid", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            Easycard = new Bill.EasyCard
                                            {
                                                ShopName = reader2["income"].ToString(),
                                                ShopName2 = reader2["change"].ToString(),
                                                ShopID = reader2["redeem"].ToString(),
                                                MachineID = reader2["redeem"].ToString(),
                                                Type = reader2["redeem"].ToString(),
                                                CardID = reader2["redeem"].ToString(),
                                                BeforeAmt = reader2["redeem"].ToString(),
                                                AddAmt = reader2["redeem"].ToString(),
                                                PayAmt = reader2["redeem"].ToString(),
                                                AfterAmt = reader2["redeem"].ToString(),
                                                TransTime = reader2["redeem"].ToString()
                                            };
                                            EasycardList.Add(Easycard);
                                        }
                                    }
                                }
                                finally { reader2.Close(); }

                            }

                            bill = new Bill.Bills
                            {
                                Amt = reader["amt"].ToString(),
                                TableID = reader["tableid"].ToString(),
                                Items = GetBillItem(Setting, reader["id"].ToString(), ""),
                                CancelItems = GetBillItem(Setting, reader["id"].ToString(), "Y"),
                                OrderID = reader["id"].ToString(),
                                TakeMealType = reader["TakeMealType"].ToString(),
                                PringTime = reader["print_tag"].ToString(),
                                State = reader["state"].ToString(),
                                Cash = CashList,
                                EasyCard = EasycardList
                            };
                            BillList.Add(bill);
                            ItemList = new List<Bill.Item>();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            root.Bills = BillList;
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 取得訂單內容
        private List<Bill.Item> GetBillItem(String Setting, String OrderID, String Stat)
        {

            Bill.Item item = new Bill.Item();
            Bill.Detail detail = new Bill.Detail();

            List<Bill.Item> ItemList = new List<Bill.Item>();
            List<Bill.Detail> DetailList = new List<Bill.Detail>();

            String SerNo = "";
            String StrSql = "";
            #region 取得此訂單點菜數量

            using (SqlConnection conn2 = new SqlConnection(Setting))
            {
                conn2.Open();
                SqlCommand cmd2;
                
                if (Stat == "Y")
                {
                    StrSql = "select max(ser_no) from orders where order_no=@order_no and stat='Y' and cancel_date<>''";
                }
                else 
                {
                    StrSql = "select max(ser_no) from orders where order_no=@order_no and cancel_date=''";
                }
                cmd2 = new SqlCommand(StrSql, conn2);
                cmd2.Parameters.Add(new SqlParameter("@order_no", OrderID));

                SqlDataReader reader2 = cmd2.ExecuteReader();
                try
                {
                    if (reader2.HasRows)
                    {
                        while (reader2.Read())
                        {
                            SerNo = reader2[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader2.Close();
                }
            }
            #endregion

            #region 點餐細項
            using (SqlConnection conn2 = new SqlConnection(Setting))
            {
                conn2.Open();
                SqlCommand cmd2;

                if (Stat == "Y")
                {
                    StrSql = "select b.sub_id,a.prod_name,a.qty,(a.qty*a.price)-a.discount as amt,a.discription,a.memo,a.ser_no,a.meal_ready,b.img1,c.PrinterName,b.GS1Code from orders as a left join prod as b on a.productid=b.id left join Printer as c on b.PrinterID=c.id where order_no=@order_no and a.stat='Y' and a.cancel_date<>'' order by a.ser_no";
                }
                else
                {
                    StrSql = "select b.sub_id,a.prod_name,a.qty,(a.qty*a.price)-a.discount as amt,a.discription,a.memo,a.ser_no,a.meal_ready,b.img1,c.PrinterName,b.GS1Code from orders as a left join prod as b on a.productid=b.id left join Printer as c on b.PrinterID=c.id where order_no=@order_no and a.cancel_date='' order by a.ser_no";
                }

                cmd2 = new SqlCommand(StrSql, conn2);
                cmd2.Parameters.Add(new SqlParameter("@order_no", OrderID));
                SqlDataReader reader2 = cmd2.ExecuteReader();
                try
                {
                    if (reader2.HasRows)
                    {

                        String MealSetName = "";
                        String MealQty = "";
                        String MealAmt = "";
                        String MealSerNo = "";
                        String MealReady = "";
                        String MealImg = "";
                        String MealDiscription = "";
                        String PrinterName = "";
                        String GS1Code = "";

                        while (reader2.Read())
                        {
                            if (reader2["sub_id"].ToString() == "999999999")
                            {
                                MealSetName = reader2["prod_name"].ToString();
                                MealQty = reader2["qty"].ToString();
                                MealAmt = reader2["amt"].ToString();
                                MealSerNo = reader2["ser_no"].ToString();
                                MealImg = reader2["img1"].ToString();
                                MealDiscription = reader2["discription"].ToString();
                                PrinterName = reader2["PrinterName"].ToString();
                                GS1Code = reader2["GS1Code"].ToString();
                            }
                            else
                            {


                                if (reader2["memo"].ToString() != MealSetName && MealSetName != "")
                                {
                                    if (MealQty != "")
                                    {
                                        item = new Bill.Item
                                        {
                                            Detail = DetailList,
                                            Img = MealImg,
                                            Amt = MealAmt,
                                            title = MealSetName,
                                            Qty = MealQty,
                                            SerNo = MealSerNo,
                                            Ready = MealReady,
                                            Discription = MealDiscription,
                                            Printer = PrinterName,
                                            GS1Code = GS1Code
                                        };
                                        ItemList.Add(item);
                                        DetailList = new List<Bill.Detail>();

                                        MealSetName = "";
                                        MealImg = "";
                                        MealAmt = "";
                                        MealQty = "";
                                        MealSerNo = "";
                                        MealDiscription = "";
                                        PrinterName = "";
                                        GS1Code = "";
                                    }
                                    else
                                    {
                                        item = new Bill.Item
                                        {
                                            Detail = null,
                                            Amt = reader2["amt"].ToString(),
                                            Img = reader2["img1"].ToString(),
                                            title = reader2["prod_name"].ToString(),
                                            Qty = reader2["qty"].ToString(),
                                            SerNo = reader2["ser_no"].ToString(),
                                            Ready = reader2["meal_ready"].ToString(),
                                            Discription = reader2["discription"].ToString(),
                                            Printer = reader2["PrinterName"].ToString(),
                                            GS1Code = reader2["GS1Code"].ToString()
                                        };
                                        ItemList.Add(item);
                                    }
                                }

                                if (reader2["memo"].ToString() == MealSetName && MealSetName != "")
                                {
                                    detail = new Bill.Detail
                                    {
                                        Amt = reader2["amt"].ToString(),
                                        Discription = reader2["discription"].ToString().Replace(", ",""),
                                        Qty = reader2["qty"].ToString(),
                                        Title = reader2["prod_name"].ToString(),
                                        SerNo = reader2["ser_no"].ToString(),
                                        Ready = reader2["meal_ready"].ToString(),
                                        Printer = reader2["PrinterName"].ToString(),
                                        GS1Code = reader2["GS1Code"].ToString()
                                    };
                                    DetailList.Add(detail);

                                    if (reader2["ser_no"].ToString() == SerNo)
                                    {
                                        item = new Bill.Item
                                        {
                                            Detail = DetailList,
                                            Amt = MealAmt,
                                            title = MealSetName,
                                            Img = MealImg,
                                            Qty = MealQty,
                                            SerNo = MealSerNo,
                                            Ready = MealReady,
                                            Discription = MealDiscription,
                                            Printer = PrinterName,
                                            GS1Code = GS1Code
                                        };
                                        ItemList.Add(item);
                                        DetailList = new List<Bill.Detail>();
                                    }
                                }

                                if (reader2["memo"].ToString() == MealSetName && MealSetName == "")
                                {
                                    item = new Bill.Item
                                    {
                                        Detail = DetailList,
                                        Amt = reader2["amt"].ToString(),
                                        title = reader2["prod_name"].ToString(),
                                        Img = reader2["img1"].ToString(),
                                        Qty = reader2["qty"].ToString(),
                                        SerNo = reader2["ser_no"].ToString(),
                                        Ready = reader2["meal_ready"].ToString(),
                                        Discription = reader2["discription"].ToString(),
                                        Printer = reader2["PrinterName"].ToString(),
                                        GS1Code = reader2["GS1Code"].ToString()
                                    };
                                    ItemList.Add(item);
                                    DetailList = new List<Bill.Detail>();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    reader2.Close();
                }
            }
            #endregion

            return ItemList;

        }
        #endregion

        #region 修改訂單狀態
        private void UpdateOrderStat(String ShopID, String TableID, String Setting,String Stat,String OrderID)
        {
            
            if (OrderID != "")
            {
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("update orders_hd set state=@state,edate=getdate() where id=@OrderID", conn);
                    cmd.Parameters.Add(new SqlParameter("@state", Stat));
                    cmd.Parameters.Add(new SqlParameter("@OrderID", OrderID));
                    cmd.ExecuteNonQuery();
                }
            }
            else 
            {
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("update orders_hd set state=@state,edate=getdate() where shopid=@shopid and tableid=@tableid and state='1'", conn);
                    cmd.Parameters.Add(new SqlParameter("@state", Stat));
                    cmd.Parameters.Add(new SqlParameter("@shopid", ShopID));
                    cmd.Parameters.Add(new SqlParameter("@tableid", TableID));
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region 紀錄現金收支
        private void CashIcome(String ShopID, String TableID, String Setting, String OrderID, String Income, String Change, String Redeem, String Type)
        {

            if (OrderID == "")
            {
                if (TableID != null && TableID != "" && TableID != "0")
                {
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd;

                        cmd = new SqlCommand("select id from orders_hd where shopid=@shopid and tableid=@tableid", conn);
                        cmd.Parameters.Add(new SqlParameter("@shopid", ShopID));
                        cmd.Parameters.Add(new SqlParameter("@tableid", TableID));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    OrderID = reader[0].ToString();
                                }
                            }
                        }
                        catch { OrderID = ""; }
                        finally { reader.Close(); }
                    }
                }
            }

            if (OrderID != "")
            {
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("INSERT INTO [orders_cash] ([order_no],[income],[change],[redeem],[type]) VALUES (@OrderID,@Income,@Change,@Redeem,@type)", conn);
                    
                    cmd.Parameters.Add(new SqlParameter("@OrderID", OrderID));
                    cmd.Parameters.Add(new SqlParameter("@Income", Income));
                    cmd.Parameters.Add(new SqlParameter("@Change", Change));
                    cmd.Parameters.Add(new SqlParameter("@Redeem", Redeem));
                    cmd.Parameters.Add(new SqlParameter("@type", Type));
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region 出餐確認(checktag=S)/備餐確認(checktag=Y)
        private void CheckMeal(String Setting, String OrderID, String[] SerNo, String checktag, String[] Track)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                for (int i = 0; i < SerNo.Length; i++)
                {
                    cmd = new SqlCommand("update orders set meal_ready=@meal_ready,edate=getdate() where order_no=@order_no and ser_no=@ser_no and stat<>'Y'", conn);
                    cmd.Parameters.Add(new SqlParameter("@meal_ready", checktag));
                    cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                    cmd.Parameters.Add(new SqlParameter("@ser_no", SerNo[i].ToString()));
                    cmd.ExecuteNonQuery();

                    if (Track[i].ToString() != "") 
                    {
                        cmd = new SqlCommand("update orders set track=@track,edate=getdate() where order_no=@order_no and ser_no=@ser_no and stat<>'Y'", conn);
                        cmd.Parameters.Add(new SqlParameter("@track", Track[i].ToString()));
                        cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                        cmd.Parameters.Add(new SqlParameter("@ser_no", SerNo[i].ToString()));
                        cmd.ExecuteNonQuery();
                    }
                }

            }

        }
        #endregion

        #region 儲存訂單
        private String SaveOrder(String setting, Bill.OrderData rlib)
        {
            String OrderID = "";
            String BookingID = "";
            GetMealStr GS = new GetMealStr();
            #region 訂單變數
            String BonusAmt = rlib.BonusAmt;
            String BonusDiscount = rlib.BonusDiscount;
            String FreightAmount = rlib.FreightAmount;
            String MemID = rlib.MemID;
            String PayType = rlib.PayType;
            String ReturnUrl = rlib.ReturnUrl;
            String RID = rlib.RID;

            Int32 DiscountAmt = 0;  //Convert.ToInt32(GS.ReplaceStr(this.discount_amt.Value));
            String Name = GS.ReplaceStr(rlib.Name);
            String Tel = GS.ReplaceStr(rlib.Tel);
            String Sex = GS.ReplaceStr(rlib.Sex);
            String Email = GS.ReplaceStr(rlib.Mail);
            String City = GS.ReplaceStr(rlib.City);
            String Country = GS.ReplaceStr(rlib.Country);
            String Zip = GS.ReplaceStr(rlib.Zip);
            String Address = City + Country + GS.ReplaceStr(rlib.Address);
            String Notememo = GS.ReplaceStr(rlib.Memo);
            #endregion

            SqlCommand cmd;

            #region save訂單表頭
            using (SqlConnection conn = new SqlConnection(setting))
            {
                //20140331有更新此預存程序!!!!(新增郵遞區號,縣市,鄉鎮區)
                conn.Open();

                cmd = new SqlCommand();
                cmd.CommandText = "sp_Mealorderhd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@name", Name));
                cmd.Parameters.Add(new SqlParameter("@sex", Convert.ToInt32(Sex)));
                cmd.Parameters.Add(new SqlParameter("@tel", Tel));
                cmd.Parameters.Add(new SqlParameter("@cell", Tel));
                cmd.Parameters.Add(new SqlParameter("@addr", Address));
                cmd.Parameters.Add(new SqlParameter("@mail", Email));
                cmd.Parameters.Add(new SqlParameter("@notememo", Notememo));
                cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                cmd.Parameters.Add(new SqlParameter("@item2", ""));
                cmd.Parameters.Add(new SqlParameter("@item3", ""));
                cmd.Parameters.Add(new SqlParameter("@item4", ""));
                cmd.Parameters.Add(new SqlParameter("@payment_type", PayType));
                cmd.Parameters.Add(new SqlParameter("@o_name", Name));
                cmd.Parameters.Add(new SqlParameter("@o_tel", Tel));
                cmd.Parameters.Add(new SqlParameter("@o_cell", Tel));
                cmd.Parameters.Add(new SqlParameter("@o_addr", ""));
                cmd.Parameters.Add(new SqlParameter("@bonus_amt", BonusAmt));
                cmd.Parameters.Add(new SqlParameter("@bonus_discount", BonusDiscount));
                cmd.Parameters.Add(new SqlParameter("@freightamount", FreightAmount));
                cmd.Parameters.Add(new SqlParameter("@c_no", ""));
                cmd.Parameters.Add(new SqlParameter("@ship_city", City));
                cmd.Parameters.Add(new SqlParameter("@ship_zip", Zip));
                cmd.Parameters.Add(new SqlParameter("@ship_countryname", Country));
                cmd.Parameters.Add(new SqlParameter("@discount_amt", DiscountAmt));
                cmd.Parameters.Add(new SqlParameter("@RID", RID));
                cmd.Parameters.Add(new SqlParameter("@prod_bonus", ""));                
                SqlParameter SPOutput = cmd.Parameters.Add("@OrderID", SqlDbType.NVarChar, 9);
                SPOutput.Direction = ParameterDirection.Output;
                try
                {
                    cmd.ExecuteNonQuery();
                    OrderID = SPOutput.Value.ToString();
                }
                catch
                {
                    OrderID = "0";
                }
            }
            
            #endregion
            #region 儲存訂單內容
            
            if (OrderID != "0")
            {
                int i = 1;
                int order_totalamt = 0;
                String Memo = "";

                #region 點餐
                String VerCode = rlib.MenuLists.Vercode;
                String TableID = rlib.MenuLists.TableID;
                String ShopID = rlib.MenuLists.ShopID;
                String ShopName = rlib.MenuLists.ShopName;
                String TakeMealType = "";
                if (rlib.MenuLists.TakeMealType == null)
                {
                    TakeMealType = "";
                }
                else
                {
                    TakeMealType = rlib.MenuLists.TakeMealType;
                }
                Int32 SpecAmt = 0;
                String Discription = "";
                String MenuItemName = "";

                Int32 MenuPrice = 0;
                Int32 MenuAddPrice = 0;
                Int32 MenuDiscount = 0;

                #region 改桌子狀態 20161005取消更新桌子狀態
                //using (SqlConnection conn = new SqlConnection(setting))
                //{
                //    conn.Open();
                //    cmd = new SqlCommand("update [Meal_table] set [stat]='2',[SeatingTime]=CONVERT([nvarchar](50),getdate(),(120)) where id=@TableID", conn);                    
                //    cmd.Parameters.Add(new SqlParameter("@TableID", TableID));
                //    cmd.ExecuteNonQuery();
                    
                //}
                #endregion

                #region 註記點餐桌號
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();

                    cmd = new SqlCommand();
                    cmd.CommandText = "sp_order_table";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@orderID", OrderID));
                    cmd.Parameters.Add(new SqlParameter("@tableID", TableID));
                    cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                    cmd.Parameters.Add(new SqlParameter("@TakeMealType", TakeMealType));
                    cmd.Parameters.Add(new SqlParameter("@shopID", ShopID));
                    cmd.Parameters.Add(new SqlParameter("@shopName", ShopName));
                    SqlParameter SPOutput = cmd.Parameters.Add("@Bid", SqlDbType.NVarChar, 12);
                    SPOutput.Direction = ParameterDirection.Output;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        BookingID = SPOutput.Value.ToString();
                    }
                    catch
                    {
                        BookingID = "0";
                    }
                }
                #endregion

                foreach (Bill.Menu Menu in rlib.MenuLists.Menu)
                {
                    if (Menu.ID == "Single")
                    {
                        Memo = "";
                    }
                    else
                    {
                        #region 儲存套餐名稱
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            #region 新增表身
                            cmd = new SqlCommand();
                            cmd.CommandText = "sp_order";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = conn;
                            cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                            cmd.Parameters.Add(new SqlParameter("@ser_no", i.ToString().PadLeft(3, '0')));
                            cmd.Parameters.Add(new SqlParameter("@prod_name", Menu.Name));
                            cmd.Parameters.Add(new SqlParameter("@price", GetProdPrice(Menu.ID, "", setting, "", "")));
                            cmd.Parameters.Add(new SqlParameter("@qty", Menu.Qty));
                            cmd.Parameters.Add(new SqlParameter("@amt", Convert.ToInt32(Menu.Qty) * Convert.ToInt32(GetProdPrice(Menu.ID, "", setting, "", ""))));
                            cmd.Parameters.Add(new SqlParameter("@productid", Menu.ID));
                            cmd.Parameters.Add(new SqlParameter("@colorid", ""));
                            cmd.Parameters.Add(new SqlParameter("@sizeid", ""));
                            cmd.Parameters.Add(new SqlParameter("@posno", ""));
                            cmd.Parameters.Add(new SqlParameter("@memo", ""));
                            cmd.Parameters.Add(new SqlParameter("@virtual", "N"));
                            cmd.Parameters.Add(new SqlParameter("@usetime", "0"));
                            cmd.Parameters.Add(new SqlParameter("@usedate", ""));
                            cmd.Parameters.Add(new SqlParameter("@discount", Menu.Discount));
                            cmd.Parameters.Add(new SqlParameter("@discription", ""));
                            cmd.Parameters.Add(new SqlParameter("@bonus", ""));
                            cmd.ExecuteNonQuery();
                            #endregion

                            order_totalamt += Convert.ToInt32(Menu.Qty) * Convert.ToInt32(GetProdPrice(Menu.ID, "", setting, "", "")) - Convert.ToInt32(Menu.Discount);
                            i = i + 1;
                        }

                        #endregion
                        Memo = Menu.Name;
                    }

                    foreach (Bill.MenuItem MenuItem in Menu.MenuItems)
                    {
                        MenuPrice = Convert.ToInt32(GetProdPrice(MenuItem.ID, "", setting, "", ""));

                        if (Menu.ID == "Single")
                        {
                            MenuAddPrice = 0;
                        }
                        else
                        {
                            MenuAddPrice = Convert.ToInt32(GetProdPrice(MenuItem.ID, "", setting, "", Menu.ID));
                        }

                        foreach (Bill.MenuSpec MenuSpec in MenuItem.MenuSpec)
                        {
                            Discription = "";
                            SpecAmt = 0;
                            MenuDiscount = 0;
                            if (MenuSpec.OtherID != null)
                            {
                                for (int j = 0; j < MenuSpec.OtherID.Count; j++)
                                {
                                    Discription += GetMemoName(setting, MenuSpec.OtherID[j]) + "$" + GetProdPrice(MenuItem.ID, "", setting, MenuSpec.OtherID[j], "") + " ";
                                    SpecAmt += Convert.ToInt32(GetProdPrice(MenuItem.ID, "", setting, MenuSpec.OtherID[j], ""));
                                }
                            }

                            if (MenuSpec.Memo != null)
                            {
                                for (int j = 0; j < MenuSpec.Memo.Count; j++)
                                {
                                    Discription += MenuSpec.Memo[j].ToString() + " ";
                                }
                            }

                            if (Menu.ID == "Single")
                            {
                                MenuDiscount = 0 - SpecAmt;
                            }
                            else
                            {
                                MenuDiscount = MenuPrice - SpecAmt - MenuAddPrice;
                            }

                            if (MenuAddPrice > 0)
                            {
                                MenuItemName = MenuItem.Name + "(需加價" + MenuAddPrice + ")";
                            }
                            else
                            {
                                MenuItemName = MenuItem.Name;
                            }

                            #region 儲存餐點項目
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                conn.Open();
                                #region 新增表身
                                cmd = new SqlCommand();
                                cmd.CommandText = "sp_order";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                                cmd.Parameters.Add(new SqlParameter("@ser_no", i.ToString().PadLeft(3, '0')));
                                cmd.Parameters.Add(new SqlParameter("@prod_name", MenuItemName));
                                cmd.Parameters.Add(new SqlParameter("@price", MenuPrice));
                                cmd.Parameters.Add(new SqlParameter("@qty", MenuSpec.Qty));
                                cmd.Parameters.Add(new SqlParameter("@amt", Convert.ToInt32(MenuPrice) * Convert.ToInt32(MenuSpec.Qty)));
                                cmd.Parameters.Add(new SqlParameter("@productid", MenuItem.ID));
                                cmd.Parameters.Add(new SqlParameter("@colorid", "0"));
                                cmd.Parameters.Add(new SqlParameter("@sizeid", "0"));
                                cmd.Parameters.Add(new SqlParameter("@posno", ""));
                                cmd.Parameters.Add(new SqlParameter("@memo", Memo));
                                cmd.Parameters.Add(new SqlParameter("@virtual", "N"));
                                cmd.Parameters.Add(new SqlParameter("@usetime", "0"));
                                cmd.Parameters.Add(new SqlParameter("@usedate", ""));
                                cmd.Parameters.Add(new SqlParameter("@discount", MenuDiscount * Convert.ToInt32(MenuSpec.Qty)));
                                cmd.Parameters.Add(new SqlParameter("@discription", Discription));
                                cmd.Parameters.Add(new SqlParameter("@bonus", "0"));
                                cmd.ExecuteNonQuery();
                                #endregion

                                order_totalamt += Convert.ToInt32(MenuSpec.Qty) * (MenuPrice - MenuDiscount);

                                i = i + 1;
                            }
                            #endregion
                        }
                    }
                }

                #endregion

                #region 儲存訂單總金額
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    cmd = new SqlCommand();
                    cmd.CommandText = "sp_order_freight";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                    cmd.Parameters.Add(new SqlParameter("@amt", order_totalamt));
                    cmd.ExecuteNonQuery();
                }
                #endregion
            }
             
            #endregion

            Bill.Items ReturnStr = new Bill.Items
            {
                BookingID = BookingID,
                OrderID = OrderID
            };
            return JsonConvert.SerializeObject(ReturnStr);
        }
        #endregion

        #region 取得產品價格
        private String GetProdPrice(String ProdID, String MemberLabel, String setting, String OptionID, String ComboID)
        {
            String Value = "0";
            if (OptionID == "" && ComboID == "")
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    String Str_sql = "select value2,value3 from prod where id=@id";
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@id", ProdID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (MemberLabel == "VIP")
                                {
                                    Value = reader[1].ToString();
                                }
                                else
                                {
                                    Value = reader[0].ToString();
                                }
                            }
                        }
                    }
                    finally { reader.Close(); }
                }
            }
            else if (OptionID == "" && ComboID != "")
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    String Str_sql = "select price from Meal_Detail where fid in (select id from Meal_Sub where fid = @fid) and pid=@pid";
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@fid", ComboID));
                    cmd.Parameters.Add(new SqlParameter("@pid", ProdID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Value = reader[0].ToString();
                            }
                        }
                    }
                    finally { reader.Close(); }
                }
            }
            else if (OptionID != "" && ComboID == "")
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    String Str_sql = "select price from dbo.Meal_Detail_Memo where pid=@pid and Optionid=@Optionid";
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@pid", ProdID));
                    cmd.Parameters.Add(new SqlParameter("@Optionid", OptionID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Value = reader[0].ToString();
                            }
                        }
                    }
                    finally { reader.Close(); }
                }
            }
            return Value;
        }
        #endregion

        #region 取得加價客製名稱
        private String GetMemoName(String setting, String OptionID)
        {
            String ReturnStr = "";
            //select * from dbo.Meal_Options
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                String Str_sql = "select title from dbo.Meal_Options where id=@id";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", OptionID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ReturnStr = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            return ReturnStr;
        }
        #endregion

        #region 搜尋退訂訂單資料
        private String GetCancelOrder(String TableID, String ShopID, String setting)
        {
            List<Bill.CancelItem> CI = new List<Bill.CancelItem>();
            Bill.CancelItem CIList;

            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                String Str_sql = "select * from cancelorder where tableid=@tableid and shopid=@shopid";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@tableid", TableID));
                cmd.Parameters.Add(new SqlParameter("@shopid", ShopID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            CIList = new Bill.CancelItem
                            {
                                OrderID = reader["order_no"].ToString(),
                                SerNo = reader["ser_no"].ToString(),
                                Title=reader["prod_name"].ToString(),
                                Ready=reader["meal_ready"].ToString(),
                                Amt = reader["amt"].ToString()
                            };
                            CI.Add(CIList);
                        }
                    }
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(CI);
        }
        #endregion

        #region 儲存悠遊卡付款交易資料
        private void AddEasyCardData(String Setting, String OrderID, List<Bill.EasyCard> EasyCard)
        {
            if (EasyCard != null) 
            {
                String SerNo = "";

                for (int i = 0; i < EasyCard.Count; i++) 
                {
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        String Str_sql = "select Right('000' + Cast(isnull(MAX(ser_no),0)+1 as nvarchar),3) as ser_no from orders_easycard where order_no=@id";
                        SqlCommand cmd = new SqlCommand(Str_sql, conn);
                        cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    SerNo = reader["ser_no"].ToString();
                                }
                            }
                        }
                        finally { reader.Close(); }
                    }

                    if (OrderID != "" && SerNo != "")
                    {
                        using (SqlConnection conn = new SqlConnection(Setting))
                        {
                            conn.Open();
                            SqlCommand cmd;

                            cmd = new SqlCommand("INSERT INTO [orders_easycard] ([order_no],[ser_no],[shopName],[shopName2],[shopID],[machineID],[type],[cardID],[BeforeAmt],[AddAmt],[PayAmt],[AfterAmt],[transtime]) VALUES (@OrderID,@ser_no,@shopName,@shopName2,@shopID,@machineID,@type,@cardID,@BeforeAmt,@AddAmt,@PayAmt,@AfterAmt,@transtime)", conn);
                            cmd.Parameters.Add(new SqlParameter("@OrderID", OrderID));
                            cmd.Parameters.Add(new SqlParameter("@ser_no", SerNo));
                            cmd.Parameters.Add(new SqlParameter("@shopName", EasyCard[i].ShopName));
                            cmd.Parameters.Add(new SqlParameter("@shopName2", EasyCard[i].ShopName2));
                            cmd.Parameters.Add(new SqlParameter("@shopID", EasyCard[i].ShopID));
                            cmd.Parameters.Add(new SqlParameter("@machineID", EasyCard[i].MachineID));
                            cmd.Parameters.Add(new SqlParameter("@type", EasyCard[i].Type));
                            cmd.Parameters.Add(new SqlParameter("@cardID", EasyCard[i].CardID));
                            cmd.Parameters.Add(new SqlParameter("@BeforeAmt", EasyCard[i].BeforeAmt));
                            cmd.Parameters.Add(new SqlParameter("@AddAmt", EasyCard[i].AddAmt));
                            cmd.Parameters.Add(new SqlParameter("@PayAmt", EasyCard[i].PayAmt));
                            cmd.Parameters.Add(new SqlParameter("@AfterAmt", EasyCard[i].AfterAmt));
                            cmd.Parameters.Add(new SqlParameter("@transtime", EasyCard[i].TransTime));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                
            }
        }
        #endregion
    }
}