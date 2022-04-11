using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ECSSO.Library.Coupon;
using Newtonsoft.Json;

namespace ECSSO.api.Coupon
{
    /// <summary>
    /// Coupons 的摘要描述
    /// </summary>
    public class Coupons : IHttpHandler
    {
        private CouponList couponList;
        private CouponDetail couponDetail;
        private CouponUseList couponUseList;
        private string setting;
        public void ProcessRequest(HttpContext context)
        {
            GetStr GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            couponList = new CouponList();
            try
            {
                if (context.Request.Form["token"] != null)
                {
                    this.setting = GS.checkToken(context.Request.Form["token"]);
                    if (this.setting.IndexOf("error") < 0)
                    {
                        Regex NumberPattern = new Regex("^[0-9]*[1-9][0-9]*$");
                        switch (context.Request.Form["type"])
                        {
                            case "detail":
                                if (!NumberPattern.IsMatch(context.Request.Form["id"]))
                                {
                                    code = "404";
                                    message = "not fount";
                                }
                                else
                                {
                                    getCouponDetail(int.Parse(context.Request.Form["id"]));
                                }
                                break;
                            case "UseList":
                                if (!NumberPattern.IsMatch(context.Request.Form["id"]))
                                {
                                    code = "404";
                                    message = "not fount";
                                }
                                else
                                {
                                    if (!NumberPattern.IsMatch(context.Request.Form["page"])) getUseList(int.Parse(context.Request.Form["id"]), 1);
                                    else getUseList(int.Parse(context.Request.Form["id"]), int.Parse(context.Request.Form["page"]));
                                }
                                break;
                            default:
                                if (!NumberPattern.IsMatch(context.Request.Form["page"])) getCouponList(1);
                                else getCouponList(int.Parse(context.Request.Form["page"]));
                                break;
                        }

                        code = "200";
                        message = "success";
                    }
                    else
                    {
                        code = "401";
                        message = "Token已過期";
                    }
                }
            }
            catch (Exception ex)
            {
                code = "500";
                message = ex.StackTrace;
            }
            finally
            {
                context.Response.Write(printMsg(code, message));
            }
        }
        private void getCouponDetail(int id)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select a.*,prod_Stock.ser_no StockNo,
                        case a.activeType when 3 then prod.title else '' end activeTitle,
                        case a.activeType when 3 then prod_color.title else '' end colorTitle,
                        case a.activeType when 3 then prod_size.title else '' end sizeTitle,
                        case isnull(a.LogisticsID,0) when 0 then '' else LogisticsSetting.title end LogisticsTitle
                    from Coupon as a 
                    left join prod on a.gift=prod.id
                    left join prod_Stock on prod_Stock.prod_id=prod.id and prod_Stock.ser_no=a.prodStoreNo
                    left join prod_color on prod_color.id=prod_Stock.colorID
                    left join prod_size on prod_size.id=prod_Stock.sizeID
                    left join LogisticsSetting on a.LogisticsID=LogisticsSetting.id
                    where a.id=@id and ((a.activeType=3 and not a.prodStoreNo is null) or a.activeType!=3)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int useType = string.IsNullOrEmpty(reader["type"].ToString()) ? 0 : int.Parse(reader["type"].ToString()),
                            activeType = string.IsNullOrEmpty(reader["activeType"].ToString()) ? 0 : int.Parse(reader["activeType"].ToString()),
                            getType = string.IsNullOrEmpty(reader["getType"].ToString()) ? 0 : int.Parse(reader["getType"].ToString()),
                            discount = string.IsNullOrEmpty(reader["discount"].ToString()) ? 0 : int.Parse(reader["discount"].ToString()),
                            gift = string.IsNullOrEmpty(reader["gift"].ToString()) ? 0 : int.Parse(reader["gift"].ToString()),
                            Price = string.IsNullOrEmpty(reader["Price"].ToString()) ? 0 : int.Parse(reader["Price"].ToString()),
                            fullGetPrice = string.IsNullOrEmpty(reader["fullGetPrice"].ToString()) ? 0 : int.Parse(reader["fullGetPrice"].ToString()),
                            noType = string.IsNullOrEmpty(reader["noType"].ToString()) ? 1 : int.Parse(reader["noType"].ToString()),
                            LogisticsID = string.IsNullOrEmpty(reader["LogisticsID"].ToString())?0:int.Parse(reader["LogisticsID"].ToString());
                        string r = "";
                        if (reader["colorTitle"].ToString() != "" && reader["sizeTitle"].ToString() != "")
                        {
                            r = "(" + reader["colorTitle"].ToString() + "/" + reader["sizeTitle"].ToString() + ")";
                        }
                        else if (reader["colorTitle"].ToString() != "")
                        {
                            r = "(" + reader["colorTitle"].ToString() + ")";
                        }
                        else if (reader["sizeTitle"].ToString() != "")
                        {
                            r = "(" + reader["sizeTitle"].ToString() + ")";
                        }
                        couponDetail = new CouponDetail
                        {
                            id = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString(),
                            fullGetPrice = fullGetPrice,
                            vcode = reader["VCode"].ToString(),
                            GCode = noType == 3 ? reader["GCode"].ToString() : "",
                            FCode = noType == 2 ? reader["GCode"].ToString() : "",
                            noType = noType,
                            stocks = int.Parse(reader["stocks"].ToString()),
                            getQty = int.Parse(reader["getQty"].ToString()),
                            useQty = int.Parse(reader["useQty"].ToString()),
                            stat = reader["disp_opt"].ToString() == "Y" && DateTime.Compare(Convert.ToDateTime(reader["end_date"].ToString()), DateTime.Now) > 0,
                            locked = reader["locked"].ToString() == "Y",
                            ActivationDate = reader["start_date"].ToString(),
                            ExpireDate = reader["end_date"].ToString(),
                            useType = useType,
                            activeType = activeType,
                            getType = getType,
                            fullPrice = (useType == 2 ? Price : 0),
                            discount = activeType == 2 ? discount : 0,
                            percent = activeType == 1 ? discount == 0 ? 100 : discount : 100,
                            giftID = activeType == 3 ? gift : 0,
                            giftTitle = activeType == 3 ? reader["activeTitle"].ToString() + r : "",
                            prodStockNo = activeType == 3 ? reader["StockNo"].ToString() : "",
                            LogisticsID = LogisticsID,
                            LogisticsTitle = LogisticsID==0?"不指定":reader["LogisticsTitle"].ToString() 
                        };
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private void getCouponList(int page)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT * FROM(
	                    SELECT ROW_NUMBER() OVER(ORDER BY edate desc) rowNumber,
		                    (select CEILING((COUNT(*)-1)/@PAGE_COUNT)+1 from Coupon) t,* 
	                    FROM Coupon
                    ) myTable WHERE rowNumber>(@PAGE-1)*@PAGE_COUNT AND rowNumber<=@PAGE*@PAGE_COUNT
                    order by edate desc
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@PAGE_COUNT", 20));
                cmd.Parameters.Add(new SqlParameter("@PAGE", page));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    couponList.list = new List<CouponItem>();
                    couponList.CurrentPage = page;
                    couponList.TotalPage = 0;
                    while (reader.Read())
                    {
                        if (couponList.TotalPage == 0) couponList.TotalPage = int.Parse(reader["t"].ToString());
                        couponList.list.Add(new CouponItem
                        {
                            options = new CouponOptions { classes = "couponItem" + reader["id"].ToString(), expanded = false },
                            value = new CouponTableValue
                            {
                                id = int.Parse(reader["id"].ToString()),
                                title = reader["title"].ToString(),
                                sendCount = reader["Stocks"].ToString(),
                                startTimt = reader["start_date"].ToString(),
                                endTimt = reader["end_date"].ToString(),
                                getType = reader["getType"].ToString() == "1" ? "廣發型" : "滿額贈",
                                disp = reader["disp_opt"].ToString() == "Y" && DateTime.Compare(Convert.ToDateTime(reader["end_date"].ToString()), DateTime.Now) > 0 ?
                                    "啟用" : "停用"
                            }
                        });
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private void getUseList(int id,int page) {
            couponUseList = new CouponUseList();
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    with CustTable as(
	                    select ROW_NUMBER() OVER(ORDER BY Cust_Coupon.Stat,Cust_Coupon.ExchangeDay,Cust_Coupon.[ExpireDate]) rowNumber,
		                    Cust.ch_name,Coupon.title CouponTitle,Cust_Coupon.* 
	                    from Cust_Coupon
	                    left join Coupon on Cust_Coupon.VCode=Coupon.VCode
	                    left join Cust on Cust_Coupon.memid=Cust.mem_id
	                    where Coupon.id = @id
                    )
                    SELECT(
	                    select CEILING((COUNT(*)-1)/@PAGE_COUNT)+1 from CustTable
                    ) t,*
                    FROM CustTable
                    WHERE
	                    rowNumber>(@PAGE-1)*@PAGE_COUNT AND rowNumber<=@PAGE*@PAGE_COUNT;
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                cmd.Parameters.Add(new SqlParameter("@PAGE_COUNT", 20));
                cmd.Parameters.Add(new SqlParameter("@PAGE", page));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    couponUseList.list = new List<CouponUseItem>();
                    while (reader.Read()) {
                        if (string.IsNullOrEmpty(couponUseList.VCode)) couponUseList.VCode = reader["CouponTitle"].ToString();
                        CouponUseItem couponUseItem = new CouponUseItem
                        {
                            id= int.Parse(reader["id"].ToString()),
                            memID= reader["memid"].ToString(),
                            memName= reader["ch_name"].ToString(),
                            GCode = reader["GCode"].ToString(),
                            stat = reader["stat"].ToString(),
                            getDate = reader["getDate"].ToString(),
                            canUseDate = reader["canUseDate"].ToString(),
                            ExchangeDay = reader["ExchangeDay"].ToString(),
                            ExpireDate = reader["ExpireDate"].ToString()
    };
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private String printMsg(String RspnCode, String RspnMsg)
        {
            string rsp = "";
            if (couponDetail != null)
            {
                couponDetail.RspnCode = RspnCode;
                couponDetail.RspnMsg = RspnMsg;
                rsp = JsonConvert.SerializeObject(couponDetail);
            }
            else if (couponUseList != null) {
                couponUseList.RspnCode = RspnCode;
                couponUseList.RspnMsg = RspnMsg;
                rsp = JsonConvert.SerializeObject(couponUseList);
            } else
            {
                couponList.RspnCode = RspnCode;
                couponList.RspnMsg = RspnMsg;
                rsp = JsonConvert.SerializeObject(couponList);
            }
            return rsp;
        }
    }
}