using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order
{
    public class ResponseOrderHead: OrderReportData
    {
        public responseJson result { get; set; }
        private string getStatusStr(int status)
        {
            string statusStr;
            switch (status)
            {
                case -1:
                    statusStr = "詢價中";
                    break;
                case 2:
                    statusStr = "已收款";
                    break;
                case 3:
                    statusStr = "已出貨";
                    break;
                case 4:
                    statusStr = "已取消";
                    break;
                case 5:
                    statusStr = "付款失敗";
                    break;
                case 6:
                    statusStr = "出貨中";
                    break;
                case 7:
                    statusStr = "已完成(已成立)";
                    break;
                case 8:
                    statusStr = "修改";
                    break;
                default:
                    statusStr = "審核中";
                    break;
            }
            return statusStr;
        }
        public void getStore2Order(string setting, string id) {
            GetStr GS = new GetStr();
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from orders_hd where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        orderID = reader["id"].ToString();
                        status = getStatusStr(
                            (reader["type"].ToString() == "S" && GS.StringToInt(reader["amt"].ToString(), 0) == 0) ?
                            -1 : GS.StringToInt(reader["state"].ToString(), 0)
                        );
                        servicePrice = double.Parse(reader["servicePrice"].ToString());
                        store2File1 = reader["store2File1"].ToString();
                        store2File2 = reader["store2File2"].ToString();
                        store2img1 = reader["store2img1"].ToString();
                        store2Code = reader["store2Code"].ToString();
                    }
                }
                catch (Exception e) {
                    throw e;
                }
            }
        }
    }
}