using ECSSO.Extension;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.Check
{
    public class CheckAddDto
    {
        public int id { get; set; }
        public List<OrderHdItem> Orders { get; set; }
        public List<Product.ProductList> Products { get; set; }
        private string setting { get; set; }
        public void save(string setting) {
            this.setting = setting;
            saveProd();
            saveCheckOrder();
            saveOrder();
        }
        private void saveOrder() {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update orders_hd set FK_CheckId=@id from(
	                    select * from @orders
                    ) as a
                    where a.order_id=orders_hd.id and orders_hd.FK_CheckId is null
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                cmd.AddTableParameter("@orders", ConvertCheckOrderType());
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw new Exception("訂單紀錄儲存失敗");
                }
            }
        }
        private void saveCheckOrder()
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    insert into check_orders(check_id,order_id)
                    select check_id,order_id from @orders
                ", conn);
                cmd.AddTableParameter("@orders", ConvertCheckOrderType());
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw new Exception("訂單資料儲存失敗");
                }
            }
        }
        private void saveProd() {
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    insert into check_prod(check_id,prod_id,size_id,color_id,price)
                    select check_id,prod_id,size_id,color_id,price from @prods
                ", conn);
                cmd.AddTableParameter("@prods", ConvertCheckProdType());
                try {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw new Exception("產品金額儲存失敗");
                }
            }
        }
        private List<checkProdType> ConvertCheckProdType() {
            List<checkProdType> list = new List<checkProdType>();
            Products.ForEach(e => {
                list.Add(new checkProdType { 
                    check_id = id,
                    size_id = e.SizeID,
                    color_id = e.ColorID,
                    prod_id = int.Parse(e.ID),
                    price = double.Parse(e.Value1)
                });
            });
            return list;
        }
        private List<checkOrderType> ConvertCheckOrderType()
        {
            List<checkOrderType> list = new List<checkOrderType>();
            Orders.ForEach(e => {
                list.Add(new checkOrderType
                {
                    check_id = id,
                    order_id = e.id
                });
            });
            return list;
        }
    }
}