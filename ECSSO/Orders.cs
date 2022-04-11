using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using ECSSO.Library.CustFormLibary;

namespace ECSSO
{
    public class Orders
    {
        #region 歷史訂單架構
        public class RootObject
        {
            public List<OrdersHead> OrderHeads { get; set; }
        }
        public class OrdersHead		//同舊版說明，沒更新
        {
            public String OrderID { get; set; }      //訂單編號
            public String OrderTime { get; set; }    //訂購時間
            public String PaymentType { get; set; }  //付款方式
            public String OrderAmt { get; set; }    //訂購總金額
            public String Freightamount { get; set; }    //運費
            public String BonusDiscount { get; set; }    //紅利扣抵
            public String DiscountAmt { get; set; }    //活動折扣金額
            public String CouponDiscount { get; set; }  //優惠券扣抵
            public String GetCouponTitle { get; set; } //訂單贈送優惠券
            public String CouponTitle { get; set; }  //使用優惠券名稱
            public String OrderState { get; set; }   //處理狀態
            public Boolean NewQA { get; set; }          //賣方是否有回應QA
            public Boolean Logistics { get; set; }        //物流資訊
            public Boolean CanResetPay { get; set; }    //是否可以進行補刷補付款 (用於刷卡或付款失敗後)
            public List<OrdersDetail> OrdersDetail { get; set; }

            public int DateDiff { get; set; }   //金流已成立多久時間 (用來限制多久可以申請重新付款)
            public int ServicePrice { get; set; }   //服務費
            public string NoteMemo { get; set; }    //備註
            public string orderType { get; set; }    //備註
            public List<FormColumnItem> CustMemo { get; set; }
        }
        
        public class OrdersDetail
        {
            public String SubTitle { get; set; }
            public String ProductName { get; set; }
            public String ProductID { get; set; }
            public String Price { get; set; }
            public String Qty { get; set; }
            public String Amt { get; set; }
            public String Size { get; set; }
            public String Color { get; set; }
            public String Memo { get; set; }
            public String StartTime { get; set; }
            public String EndTime { get; set; }
            public String UseTime { get; set; }
            public String Virtual { get; set; }
            public String Vcode { get; set; }
            public String Discount { get; set; }
            public String Discription { get; set; }
            public String Bonus { get; set; }
            public List<OrdersDetailData> OrdersDetailData { get; set; }
        }
        public class OrdersDetailData
        {
            public String Name { get; set; }
            public String Tel { get; set; }
            public String CellPhone { get; set; }
            public String Addr { get; set; }
            public String Birth { get; set; }
            public String LBirth { get; set; }
            public String Hour { get; set; }
            public String Animal { get; set; }
            public String LightNo { get; set; }
        }
        #endregion

        #region 取得歷史訂單json字串
        public string GetOrderListJson(String MemID, String SiteID)
        {
            GetStr GS = new GetStr();
            String setting = GS.GetSetting(SiteID);
            RootObject root = new RootObject();
            List<OrdersHead> orderhd = new List<OrdersHead>();
            List<OrdersDetail> ordersDetail = new List<OrdersDetail>();
            List<OrdersDetailData> orderDetailData = new List<OrdersDetailData>();

            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select o.id,convert(datetime, o.cdate, 100) as cdate,
                        o.payment_type,o.[state],o.amt,o.freightamount,o.ServicePrice,o.[type],
                        o.bonus_discount,o.discount_amt,o.notememo,
                        DateDiff(MINUTE,o.cdate,GETDATE()) as cDateDiff,
                        DateDiff(MINUTE,o.repaydate,GETDATE()) as rDateDiff,
                        isnull(o.couponDiscount,0) couponDiscount,Coupon.title CouponTitle,
                        c.title getCouponTitle
                    from orders_hd as o
                    left join Coupon on o.couponID=Coupon.id
                    left join Cust_Coupon on o.getCouponID=Cust_Coupon.id
                    left join Coupon as c on c.VCode = Cust_Coupon.VCode
                    where o.mem_id=@MemID and o.cdate>=DATEADD(MONTH,-6,GETDATE()) order by o.id desc
                ", conn);
                int u = 0;
                if (int.TryParse(MemID, out u))
                {
                    cmd.Parameters.Add(new SqlParameter("@MemID", MemID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                using (SqlConnection conn2 = new SqlConnection(setting))
                                {
                                    conn2.Open();
                                    SqlCommand cmd2 = new SqlCommand("select prod_name,productid,price,qty,amt,sizeid,colorid,memo,vcode,start_date,end_date,usetime,virtual,discount,ser_no,discription,bonus from orders where order_no=@id", conn2);
                                    cmd2.Parameters.Add(new SqlParameter("@id", reader["id"].ToString()));
                                    SqlDataReader reader2 = cmd2.ExecuteReader();
                                    try
                                    {
                                        if (reader2.HasRows)
                                        {
                                            ordersDetail = new List<OrdersDetail>();
                                            orderDetailData = new List<OrdersDetailData>();
                                            while (reader2.Read())
                                            {
                                                orderDetailData = new List<OrdersDetailData>();
                                                #region 取得點燈資料
                                                using (SqlConnection conn3 = new SqlConnection(setting))
                                                {
                                                    conn3.Open();
                                                    SqlCommand cmd3 = new SqlCommand("select * from orders_detail where order_no=@order_no and orders_ser_no=@orders_ser_no", conn3);
                                                    cmd3.Parameters.Add(new SqlParameter("@order_no", reader["id"].ToString()));
                                                    cmd3.Parameters.Add(new SqlParameter("@orders_ser_no", reader2["ser_no"].ToString()));
                                                    SqlDataReader reader3 = cmd3.ExecuteReader();
                                                    try
                                                    {
                                                        if (reader3.HasRows)
                                                        {
                                                            
                                                            while (reader3.Read())
                                                            {
                                                                OrdersDetailData DList = new OrdersDetailData
                                                                {
                                                                    Name = reader3["name"].ToString(),
                                                                    Birth = reader3["birth"].ToString(),
                                                                    Addr = reader3["addr"].ToString(),
                                                                    Animal = reader3["animal"].ToString(),
                                                                    CellPhone = reader3["cellphone"].ToString(),
                                                                    Tel = reader3["tel"].ToString(),
                                                                    Hour = reader3["hour"].ToString(),
                                                                    LBirth = reader3["Lunar_birth"].ToString(),
                                                                    LightNo=reader3["LightNo"].ToString()
                                                                };
                                                                orderDetailData.Add(DList);
                                                            }
                                                        }
                                                    }
                                                    finally {
                                                        reader3.Close();
                                                    }
                                                }
                                                #endregion

                                                OrdersDetail List = new OrdersDetail
                                                {
                                                    ProductName = reader2["prod_name"].ToString(),
                                                    ProductID = reader2["productid"].ToString(),
                                                    Price = reader2["price"].ToString(),
                                                    Qty = reader2["qty"].ToString(),
                                                    Amt = reader2["amt"].ToString(),
                                                    Size = GS.GetSpec(setting, "prod_size", int.Parse(reader2["sizeid"].ToString())),
                                                    Color = GS.GetSpec(setting, "prod_color", int.Parse(reader2["colorid"].ToString())),
                                                    Memo = reader2["memo"].ToString(),
                                                    StartTime = reader2["start_date"].ToString(),
                                                    EndTime = reader2["end_date"].ToString(),
                                                    UseTime = reader2["usetime"].ToString(),
                                                    Virtual = reader2["virtual"].ToString(),
                                                    Vcode = reader2["vcode"].ToString(),
                                                    Discount = reader2["discount"].ToString(),
                                                    Discription = reader2["discription"].ToString(),
                                                    Bonus = reader2["bonus"].ToString(),
                                                    OrdersDetailData = orderDetailData
                                                };
                                                ordersDetail.Add(List);
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        reader2.Close();
                                    }
                                }
                                Boolean NewQA = false;
                                #region 檢查是否有新的回應
                                using (SqlConnection conn2 = new SqlConnection(setting))
                                {
                                    conn2.Open();
                                    SqlCommand cmd2 = new SqlCommand("select id from dbo.orders_QA where Qchk='N' and order_no=@id", conn2);
                                    cmd2.Parameters.Add(new SqlParameter("@id", reader["id"].ToString()));
                                    SqlDataReader reader2 = cmd2.ExecuteReader();
                                    try
                                    {
                                        if (reader2.HasRows)
                                        {
                                            NewQA = true;
                                        }
                                    }
                                    finally { reader2.Close(); }
                                }
                                #endregion

                                #region 物流資訊
                                Boolean Logistics = false;
                                using (SqlConnection conn2 = new SqlConnection(setting))
                                {
                                    conn2.Open();
                                    SqlCommand cmd2 = new SqlCommand("select * from orders_Logistics where order_no=@id", conn2);
                                    cmd2.Parameters.Add(new SqlParameter("@id", reader["id"].ToString()));
                                    SqlDataReader reader2 = cmd2.ExecuteReader();
                                    try
                                    {
                                        if (reader2.HasRows)
                                        {
                                            Logistics = true;
                                        }
                                    }
                                    finally { reader2.Close(); }
                                }
                                #endregion

                                FormColumns form = new FormColumns(setting, reader["id"].ToString());
                                OrdersHead ListHead = new OrdersHead
                                {
                                    OrderID = reader["id"].ToString(),
                                    OrderTime = reader["cdate"].ToString(),
                                    DateDiff = reader["rDateDiff"] == DBNull.Value ? (int)reader["cDateDiff"] : (int)reader["rDateDiff"],
                                    PaymentType = GS.GetPayType(setting,reader["payment_type"].ToString()),
                                    OrderAmt = reader["amt"].ToString(),
                                    Freightamount = reader["freightamount"].ToString(),
                                    BonusDiscount = reader["bonus_discount"].ToString(),
                                    CouponDiscount = reader["couponDiscount"].ToString(),
                                    CouponTitle = reader["CouponTitle"].ToString(),
                                    GetCouponTitle = reader["GetCouponTitle"].ToString(),
                                    DiscountAmt = reader["discount_amt"].ToString(),
                                    OrderState = GS.GetOrderState(reader["state"].ToString()),
                                    OrdersDetail = ordersDetail,
                                    Logistics = Logistics,
                                    NewQA = NewQA,
                                    CanResetPay = GS.CanResetPay(reader["payment_type"].ToString()),
                                    ServicePrice = int.Parse(reader["ServicePrice"].ToString()),
                                    orderType = reader["type"].ToString(),
                                    NoteMemo = reader["notememo"].ToString(),
                                    CustMemo = form.columnItems
                                };
                                orderhd.Add(ListHead);
                            }

                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            root.OrderHeads = orderhd;
            String returnStr = JsonConvert.SerializeObject(root);
            return returnStr;
        }
        #endregion
    }
    
}