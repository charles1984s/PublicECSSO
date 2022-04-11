using ECSSO.Library.EmailCont;
using ECSSO.Library.Enumeration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace ECSSO.Library.Order
{
    public class FinishOrder
    {
        private CheckToken checkToken { get; set; }
        private GetStr GS { get; set; }
        public string orderid { get; set; }
        public string bonusmemo { get; set; }
        public string memid { get; set; }
        public int memberVip { get; set; }
        public int bonusadd { get; set; }
        public int state { get; set; }
        public int xxtype { get; set; }
        public int bonusspend { get; set; }
        public int bonustotaladd { get; set; }
        public int bonusDiscount { get; set; }
        public int CouponID { get; set; }
        public int freightamount { get; set; }
        public int Amt { get; set; }
        public string paidDate { get; set; }
        public string sex { get; set; }
        public string tel { get; set; }
        public string cell { get; set; }
        public string zip { get; set; }
        public string addr { get; set; }
        public string memoStr { get; set; }
        public List<OrderMemo> memo { get; set; }
        public List<OrderDetail> details { get; set; }
        public MarketTaiwan marketTaiwan { get; set; }
        public Affiliates affiliates { get; set; }

        #region 建構子
        public FinishOrder() { }
        public FinishOrder(CheckToken checkToken, string orderId)
        {
            GS = new GetStr();
            this.checkToken = checkToken;
            this.orderid = orderId;
            GetOrder();
        }
        #endregion

        #region 變更訂單狀態
        private void UpdateOrderStatus(int status)
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update orders_hd set state=@status,crm_date=replace(replace(CONVERT([varchar](256),getdate(),(120)),'-',''),':',''),edate=getdate() where id=@OrderId
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@status", status));
                cmd.Parameters.Add(new SqlParameter("@OrderId", orderid));
                try
                {
                    cmd.ExecuteReader();
                    GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "訂單管理", "修改訂單狀態為" + Enum.GetName(typeof(ORderStatusEnum), status), orderid,
                        "update orders_hd set state=@status,crm_date=replace(replace(CONVERT([varchar](256),getdate(),(120)),'-',''),':',''),edate=getdate() where id=@OrderId",
                        "api/Order/Report/OrderUpdate.ashx"
                    );
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion

        #region 取得訂單資料
        private void GetOrder()
        {
            TokenItem token = checkToken.token;
            if (GS.hasPwoer(checkToken.setting, "E004", "canedit", token.id))
            {
                using (SqlConnection conn = new SqlConnection(checkToken.setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                    select 
                        isnull(Cust.vip,0) vip,
                        case when (convert(int,amt)-convert(int,bonus_discount)-convert(int,discount_amt)-convert(int,isnull(couponDiscount,0)))<0 then 
                            convert(int,freightamount)
                        else
                            (convert(int,amt)-convert(int,bonus_discount)-convert(int,discount_amt)-convert(int,isnull(couponDiscount,0)))+convert(int,freightamount)
                        end as payAmt,
                        orders_hd.* 
                    from orders_hd 
                    left join Cust on orders_hd.mem_id=Cust.mem_id
                    where orders_hd.id = @OrderId
                ", conn);
                    cmd.Parameters.Add(new SqlParameter("@OrderId", orderid));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            int amt = reader["bonus_amt"].ToString() == "" ? 0 : int.Parse(reader["bonus_amt"].ToString());
                            orderid = reader["id"].ToString();
                            state = (reader["type"].ToString() == "S" && GS.StringToInt(reader["amt"].ToString(), 0) == 0) ?
                                -1 : int.Parse(reader["state"].ToString());
                            xxtype = 1;
                            memid = reader["mem_id"].ToString();
                            bonusmemo = reader["id"].ToString();
                            bonusadd = amt;
                            bonusspend = 0;
                            bonustotaladd = amt;
                            memberVip = int.Parse(reader["vip"].ToString());
                            CouponID = reader["getCouponID"].ToString() == "" ? 0 : int.Parse(reader["getCouponID"].ToString());
                            bonusDiscount = reader["bonus_discount"].ToString() == "" ? 0 : int.Parse(reader["bonus_discount"].ToString());
                            marketTaiwan = new MarketTaiwan(orderid, checkToken);
                            affiliates = new Affiliates(orderid, checkToken);
                            paidDate = reader["paid_date"].ToString();
                            Amt = int.Parse(reader["payAmt"].ToString());
                            sex = reader["sex"].ToString() == "1" ? "先生" : "女士";
                            tel = reader["tel"].ToString();
                            cell = reader["cell"].ToString();
                            zip = reader["ship_zip"].ToString();
                            addr = reader["addr"].ToString();
                            freightamount = int.Parse(reader["freightamount"].ToString());
                            memoStr = reader["notememo"].ToString();
                            memo = new List<OrderMemo>();
                            details = new List<OrderDetail>();
                            if (affiliates.enable)
                            {
                                affiliates.server_subid = reader["Click_ID"].ToString();
                            }
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else throw new Exception("訂單不存在!");
        }
        #endregion

        #region 更新訂單資料
        private void updateOrder()
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_UpdateOrder";
                cmd.Connection = conn;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add(new SqlParameter("@orderId", orderid));
                cmd.Parameters.Add(new SqlParameter("@bonus_memo", bonusmemo));
                cmd.Parameters.Add(new SqlParameter("@mem_id", memid));
                cmd.Parameters.Add(new SqlParameter("@bonus_add", bonusadd));
                cmd.Parameters.Add(new SqlParameter("@user_id", checkToken.token.id));
                cmd.Parameters.Add(new SqlParameter("@ip", GS.GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", "/api/OrderUpdate.ashx"));
                cmd.Parameters.Add(new SqlParameter("@type", xxtype));
                cmd.Parameters.Add(new SqlParameter("@bonus_spend", bonusspend));
                cmd.Parameters.Add(new SqlParameter("@bonus_total_add", bonustotaladd));
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion

        #region 更新優惠券可用日期
        private int getCouponStat()
        {
            int use = 0;
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select stat from cust_coupon where id=@CouponID and CONVERT(int,memid)=CONVERT(int,@MemID)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@CouponID", CouponID));
                cmd.Parameters.Add(new SqlParameter("@MemID", memid));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        use = int.Parse(reader["stat"].ToString());
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return use;
        }
        private void notUseCoupon()
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                string sql = @"
                    update cust_coupon set canUseDate=null,stat=1
                    where id=@CouponID and memid=CONVERT(int,@MemID) and isnull(canUseDate,'')=''
                ";
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@CouponID", CouponID));
                cmd.Parameters.Add(new SqlParameter("@MemID", memid));
                try
                {
                    cmd.ExecuteReader();
                    GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "訂單管理", "取消客戶優惠券", CouponID.ToString(),
                        sql,
                        "api/Order/Report/OrderUpdate.ashx"
                    );
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        private void cancelCoupon()
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                string sql = @"
                    update cust_coupon set canUseDate=null,stat=3
                    where id=@CouponID and memid=CONVERT(int,@MemID) and isnull(canUseDate,'')=''
                ";
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@CouponID", CouponID));
                cmd.Parameters.Add(new SqlParameter("@MemID", memid));
                try
                {
                    cmd.ExecuteReader();
                    GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "訂單管理", "取消客戶優惠券", CouponID.ToString(),
                        sql,
                        "api/Order/Report/OrderUpdate.ashx"
                    );
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        private void updateCoupon()
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                string sql = @"
                    update cust_coupon set canUseDate=getdate(),stat=1
                    where id=@CouponID and memid=CONVERT(int,@MemID) and isnull(canUseDate,'')=''
                ";
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@CouponID", CouponID));
                cmd.Parameters.Add(new SqlParameter("@MemID", memid));
                try
                {
                    cmd.ExecuteReader();
                    GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "訂單管理", "更新優惠券可用日期", CouponID.ToString(),
                        sql,
                        "api/Order/Report/OrderUpdate.ashx"
                    );
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion

        #region 更新庫存
        private void updateStock(int multiplier)
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                string sql = @"
                    update prod_stock set stock = stock + (a.qty * @multiplier)
                    from (
	                    select * from orders where order_no=@orderID
                    ) as a
                    where 
	                    prod_stock.prod_id=a.productid and 
	                    prod_stock.colorID=a.colorid and 
	                    prod_stock.sizeID=a.sizeid
                ";
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@orderID", orderid));
                cmd.Parameters.Add(new SqlParameter("@multiplier", multiplier));
                try
                {
                    cmd.ExecuteReader();
                    GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "訂單管理", "完成訂單更新庫存", orderid,
                        sql,
                        "api/Order/Report/OrderUpdate.ashx"
                    );
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        private void addStock()
        {
            updateStock(1);
        }
        private void subStock()
        {
            updateStock(-1);
        }
        #endregion

        #region 報價
        private void NoticeCustPrice(string orderId)
        {
            EmailContResponse response = new EmailContResponse(checkToken.setting);
            string title, sender, date, name, mail;
            int price;
            response.GetSender(out title, out sender);
            response.GetOrderMail(orderId, out price, out date, out name, out mail);
            EmailCont.EmailCont emailCont = response.getItem(2);
            emailCont.introduction = $@"
                訂單編號 : {orderId}  <br />
                報價日期 : {date}<br />
                詢價人 : {name}<br />
                詢價金額 : {price}<br /><br />
                {emailCont.introduction}
            ";
            if (emailCont != null)
                response.sendEmail(checkToken.setting, emailCont, title+ " 詢價回覆通知", sender, mail);
        }
        public void setOrderPrice(int price)
        {
            if (price == 0) throw new Exception("詢價金額不可為0");
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update orders_hd set amt=@amt,servicePrice=0,discount_amt=0,edate=getdate() where id=@OrderId
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@OrderId", orderid));
                cmd.Parameters.Add(new SqlParameter("@amt", price));
                try
                {
                    cmd.ExecuteReader();
                    NoticeCustPrice(orderid);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion

        #region 申請案號
        private void NoticeOrderStore2Code(string code)
        {
            EmailContResponse response = new EmailContResponse(checkToken.setting);
            string title, sender, date, name, mail;
            int price;
            response.GetSender(out title, out sender);
            response.GetOrderMail(orderid, out price, out date, out name, out mail);
            Library.EmailCont.EmailCont emailCont = response.getItem(6);
            emailCont.introduction = $@"
                {emailCont.introduction}<br /><br />
                <p>{HttpUtility.HtmlDecode(code)}</p>
            ";
            if (emailCont != null)
                response.sendEmail(checkToken.setting, emailCont, title+ " 申請案號回覆通知", sender, mail);
        }
        public void setOrderStore2Code(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new Exception("案號不可為空");
            }
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update orders_hd set store2Code=@code,edate=getdate() where id=@OrderId
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@OrderId", orderid));
                cmd.Parameters.Add(new SqlParameter("@code", code));
                try
                {
                    cmd.ExecuteReader();
                    NoticeOrderStore2Code(code);
                    GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "訂單管理", "輸入案號", orderid,
                        "update orders_hd set store2Code=@code,edate=getdate() where id=@OrderId",
                        "api/Order/Report/OrderUpdate.ashx"
                    );
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion

        #region 完成變一般
        private void setCompleteToCommon()
        {
            if (state == (int)ORderStatusEnum.已完成)
            {
                bonusspend = bonusadd;
                bonusadd = 0;
                bonustotaladd = bonusspend * -1;
                updateOrder();
                if (getCouponStat() == 1) notUseCoupon();
                marketTaiwan.Cancel();
                affiliates.Cancel();
            }
        }
        #endregion

        #region 取得訂單備註
        public void setOrderMemo()
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select col.title,value.value 
                    from contactValue as value 
                    left join Contact as head on head.id = value.contactID 
                    left join contactUsColumn as col on col.id=value.columnID 
                    where head.[type]=@orderID
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@orderID", orderid));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        memo.Add(new OrderMemo
                        {
                            title = reader["title"].ToString(),
                            value = reader["value"].ToString()
                        });
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public string getOrderMemoSstring()
        {
            string memoStr = "";
            if (memo.Count == 0) setOrderMemo();
            memo.ForEach(e =>
            {
                memoStr += $@"{e.title}：{e.value}<br />";
            });
            if (!string.IsNullOrEmpty(this.memoStr)) memoStr += $@"備註：{this.memoStr}<br />";
            return memoStr;
        }
        #endregion

        #region 取得訂單明細
        public void setOrderDetail()
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select productid,prod_name,qty,isnull(b.title,'') as colortitle,isnull(c.title,'') as sizetitle 
                    from orders as a 
                    left join prod_color as b on a.colorid=b.id 
                    left join prod_size as c on a.sizeid=c.id 
                    where a.order_no=@orderID
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@orderID", orderid));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        details.Add(new OrderDetail
                        {
                            id = int.Parse(reader["productid"].ToString()),
                            name = reader["prod_name"].ToString(),
                            qty = int.Parse(reader["qty"].ToString()),
                            sizeTitle = reader["sizetitle"].ToString(),
                            colorTitle = reader["colortitle"].ToString()
                        });
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public string getOrderDetailStr() {
            string detailStr = "";
            if (details.Count == 0) setOrderDetail();
            details.ForEach(e =>
            {
                string specStr = "";
                if (!string.IsNullOrEmpty(e.sizeTitle)) specStr += $"{e.sizeTitle}/";
                if (!string.IsNullOrEmpty(e.colorTitle)) specStr += $"{e.colorTitle}/";
                if (specStr.Length > 0) specStr = specStr.Substring(0, specStr.Length-1);
                detailStr += 
                $@"<tr>
                    <td>{e.name} {specStr}</td>
                    <td>{e.qty}</td>
                </tr>";
            });
            return $@"
                <table>
                    <thead>
                        <tr>
                            <td>訂購商品</td>
                            <td>數量</td>
                        </tr>
                    </thead>
                    <tbody>
                        {detailStr}
                    </tbody>
                </table>
            ";
        }
        #endregion

        #region 發送出貨通知信
        public void sendShipment()
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from storeSet where [key]='sendShipment' and [enable]=1
                ", conn);
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        EmailContResponse email = new EmailContResponse(checkToken.setting);
                        Library.EmailCont.EmailCont emailCont = new EmailCont.EmailCont();
                        string title, sender, date, name, mail;
                        int price;
                        email.GetSender(out title, out sender);
                        email.GetOrderMail(orderid, out price, out date, out name, out mail);
                        emailCont.introduction = $@"
                            訂單資訊<br />
                            編號：{orderid}<br />
                            {(paidDate != "" ? $"付款日期：{paidDate}<br />" : "")}
                            運費：{freightamount}<br />
                            金額（含運費）：{Amt}<br /><br />
                            收件人資訊<br />
                            姓名：{name} {sex}<br />
                            {(tel != "" ? $"電話：{tel}<br />" : "")}
                            {(cell != "" ? $"電話：{cell}<br />" : "")}
                            地址：{zip}{addr}<br>
                            備註：<br />
                            {getOrderMemoSstring()}<br />
                            {getOrderDetailStr()}
                        ";

                        emailCont.signature = $@"
                            如有任何問題，請來函{sender}，我們會盡速為您處理。 <br><br>
                            ＊此為系統自動發出訊息，請勿直接回覆此信，以避免您的寶貴意見無法確實傳達！ <br>
                            祝　您事業順利、生意興隆 <br>
                            {title}　敬上<br>
                        ";
                        email.sendEmail(checkToken.setting, emailCont, title + " 出貨通知", sender, mail);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion

        #region 變更訂單狀態
        public void underReviewOrder()
        {
            if (state == 1) return;
            UpdateOrderStatus(1);
            setCompleteToCommon();
        }
        public void paidForOrder()
        {
            if (state == 2) return;
            UpdateOrderStatus(2);
            setCompleteToCommon();
        }
        public void ShippedOrder()
        {
            if (state == 3) return;
            UpdateOrderStatus(3);
            setCompleteToCommon();
            sendShipment();
        }
        public void cancelOrder()
        {
            if (state == 4) return;
            UpdateOrderStatus(4);
            addStock();
            if (state == (int)ORderStatusEnum.已完成) bonusspend = bonusadd;
            else bonusspend = 0;
            bonusadd = bonusDiscount;
            bonustotaladd = bonusadd - bonusspend;
            updateOrder();
            if (getCouponStat() == 1) cancelCoupon();
            marketTaiwan.Cancel();
            affiliates.Cancel();
        }
        public void ShippingOrder()
        {
            if (state == 6) return;
            UpdateOrderStatus(6);
            setCompleteToCommon();
        }
        public string finishOrder()
        {
            if (state == 7) return "";
            string RspnMsg = "";
            int[] canSave = { 1, 2, 3, 6, 8 };
            bonusmemo += "完成訂單";
            if (Array.Exists<int>(canSave, e =>
            {
                if (e == state) return true;
                else return false;
            }))
            {
                UpdateOrderStatus(7);
                updateOrder();
                if (CouponID != 0) updateCoupon();
                if (memberVip == 3)
                {
                    subStock();
                }
                if (marketTaiwan.enable) RspnMsg = marketTaiwan.Finish();
                if (affiliates.enable) RspnMsg = affiliates.Finish();

                return RspnMsg;
            }
            else
            {
                throw new Exception("訂單不可變更");
            }
        }
        public void MemoOrder()
        {
            if (state == 8) return;
            UpdateOrderStatus(8);
            setCompleteToCommon();
        }
        #endregion
    }
}