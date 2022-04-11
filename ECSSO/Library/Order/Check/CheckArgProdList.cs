using ECSSO.Library.Prod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.Check
{
    public class CheckArgProdList: responseJson
    {
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public List<Product.ProductList> products { get; set; }
        public void getProds() {
            RspnCode = "500.2";
            d1 = DateTime.Parse(checkToken.context.Request.Form["d1"]);
            d2 = DateTime.Parse(checkToken.context.Request.Form["d2"]).AddSeconds(1);
            products = new List<Product.ProductList>();
            using (SqlConnection conn = new SqlConnection(checkToken.setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select p.*,c.id colorID,c.title colorTitle,s.id sizeID,s.title sizeTitle from prod as p
                    left join prod_Stock as st on p.id=st.prod_id
                    left join prod_size as s on st.sizeID=s.id
                    left join prod_color as c on st.colorID=c.id
                    where p.id in(
	                    select productid from orders
	                    left join orders_hd as hd on orders.order_no=hd.id
	                    where hd.FK_CheckId is null and hd.cdate between convert(datetime,@d1) and convert(datetime,@d2)
                    ) and p.timePrice='A'
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@d1", d1.ToString("yyyy-MM-dd HH:mm:ss")));
                cmd.Parameters.Add(new SqlParameter("@d2", d2.ToString("yyyy-MM-dd HH:mm:ss")));
                try {
                    SqlDataReader reader = cmd.ExecuteReader();
                    RspnCode = "500.3";
                    while (reader.Read()) {
                        products.Add(new Product.ProductList
                        {
                            ID = reader["id"].ToString(),
                            ColorID = string.IsNullOrEmpty(reader["colorID"].ToString()) ? 0 : int.Parse(reader["colorID"].ToString()),
                            SizeID = string.IsNullOrEmpty(reader["sizeID"].ToString()) ? 0 : int.Parse(reader["sizeID"].ToString()),
                            SizeTitle = reader["sizeTitle"].ToString(),
                            ColorTitle = reader["colorTitle"].ToString(),
                            Title = reader["title"].ToString(),
                            Value1 = reader["value1"].ToString(),
                            Value2 = "0"
                        });
                    }
                    checkToken.GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "對帳單", "取得帳單農產品資訊", "",
                        $"{d1.ToString("yyyy-MM-dd HH:mm:ss")}~{d2.ToString("yyyy-MM-dd HH:mm:ss")}",
                        "api/Order/CheckHandler.ashx"
                    );
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                    success = false;
                    throw new Exception(e.Message);
                }
            }
        }
    }
}