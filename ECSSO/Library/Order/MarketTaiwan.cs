using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace ECSSO.Library.Order
{
    public class MarketTaiwan
    {
        public string Offer_ID { get; set; }
        public string Advertiser_ID { get; set; }
        public int Refund_Amount { get; set; }
        public double Commission_Amount { get; set; }
        public string ORDER_ID { get; set; }
        public string RID { get; set; }
        public string Click_ID { get; set; }
        public DateTime Date { get; set; }
        public bool enable { get; set; }
        public bool state { get; set; }
        public int stateCode { get; set; }
        private CheckToken checkToken { get; set; }

        public MarketTaiwan(string orderid, CheckToken checkToken)
        {
            this.checkToken = checkToken;
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select commission, OFFER_ID, advertiser_id, market_taiwan from head
                ", conn);
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        enable = reader["market_taiwan"].ToString() == "Y";
                        Commission_Amount = reader["commission"].ToString() == "" ? 0 :
                            double.Parse(reader["commission"].ToString()) / 100;
                        Offer_ID = reader["OFFER_ID"].ToString();
                        ORDER_ID = orderid;
                        Advertiser_ID = reader["advertiser_id"].ToString();
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        public void setMarketTaiwanPrice()
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select convert(varchar,year(b.cdate))+'-'+right('00'+convert(varchar,MONTH(b.cdate)),2) +'-'+right('00'+convert(varchar,DAY(b.cdate)),2) as orderdate,b.RID,convert(int,qty*price)-CONVERT(int,discount) as amt,b.Click_ID,b.[state]
                    from orders as a 
                    left join orders_hd as b on a.order_no=b.id 
                    where isnull(b.RID,'')<>'' and a.order_no=@OrderId
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@OrderId", ORDER_ID));
                SqlDataReader reader = null;
                try
                {
                    bool isFirst = false;
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (int.Parse(reader["state"].ToString()) != 4 &&
                            int.Parse(reader["state"].ToString()) != 7)
                            throw new Exception("尚未完成訂單");
                        Refund_Amount += int.Parse(reader["amt"].ToString());
                        if (!isFirst)
                        {
                            RID = reader["RID"].ToString();
                            Click_ID = reader["Click_ID"].ToString();
                            Date = DateTime.Parse(reader["orderdate"].ToString());
                            stateCode = int.Parse(reader["state"].ToString());
                            isFirst = true;
                        }
                    }
                    Commission_Amount = Math.Round(Refund_Amount * Commission_Amount, 2);
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }

        public string Finish()
        {
            string result = "";
            if (enable)
            {
                state = true;
                setMarketTaiwanPrice();
                result = submitMarketTaiwan();
            }
            return result;
        }

        public void Cancel() {
            if (enable)
            {
                state = false;
                setMarketTaiwanPrice();
                submitMarketTaiwan();
            }
        }
        public string submitMarketTaiwan()
        {
            if (!enable || string.IsNullOrEmpty(RID)) return null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.hasoffers.com/Api");
                request.Method = "POST";
                string postData = "Format=json&Target=Conversion&Method=create&Service=HasOffers&Version=2&NetworkId=marktamerica&NetworkToken=NETPYKNAYOswzsboApxaL6GPQRiY2s&";
                postData += "data[offer_id]=" + Offer_ID + "&data[advertiser_id]=" + Advertiser_ID + "&";
                if (state)
                {
                    postData += "data[sale_amount]=" + Refund_Amount + "&data[affiliate_id]=12&data[payout]=" + Commission_Amount + "&";
                    postData += "data[revenue]=" + Commission_Amount;
                }
                else
                {
                    postData += "data[sale_amount]=-" + Refund_Amount + "&data[affiliate_id]=12&data[payout]=-" + Commission_Amount + "&";
                    postData += "data[revenue]=-" + Commission_Amount;
                }
                postData += "&data[advertiser_info]=" + ORDER_ID + "&data[affiliate_info1]=" + RID + "&data[ad_id]=" + Click_ID + "&";
                postData += "data[session_datetime]=" + Date.ToString("yyyy-MM-dd");
                //throw new Exception(postData);
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();

                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();

                dataStream = response.GetResponseStream();
                StreamReader reader2 = new StreamReader(dataStream);
                string responseFromServer = reader2.ReadToEnd();

                reader2.Close();
                dataStream.Close();
                response.Close();
                return responseFromServer;
            }
            catch (WebException even)
            {
                throw new Exception(even.Message);
            }
        }
    }
}