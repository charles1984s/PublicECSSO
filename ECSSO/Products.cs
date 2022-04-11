using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;

namespace ECSSO
{
    public class Products
    {        
        #region 取得產品json
        public String GetProdJson(List<String> ID, String MemberID, String SiteID)
        {
            String ReturnStr = "";
            String Str_Search1 = "";
            String Str_Sql = "";
            GetStr GS = new GetStr();
            String setting = GS.GetSetting(SiteID);
            
            if (ID.Count > 0) {                         
                Str_Search1 = " and a.id in (";
                for (int i = 0; i < ID.Count; i++)
                {
                    if (i < ID.Count - 1)
                    {
                        Str_Search1 += "@id" + i.ToString() + ",";
                    }
                    else {
                        Str_Search1 += "@id" + i.ToString();
                    }
                }
                Str_Search1 += ")";                
            }

            if (MemberID != "")
            {
                if (ID.Count > 0)
                {
                    Str_Sql = "select a.*,b.id as sub_id,b.title as sub_title,c.id as au_id,c.title as au_title from prod as a left join prod_list as b on a.sub_id=b.id left join prod_authors as c on c.id=b.au_id where a.disp_opt='Y' and a.id in (select distinct(b.productid) from orders_hd as a left join orders as b on a.id=b.order_no where mem_id=@mem_id and b.virtual='Y' and b.end_date<>'' and @datetime between b.start_date and b.end_date) " + Str_Search1;
                }
                else
                {
                    Str_Sql = "select a.*,b.id as sub_id,b.title as sub_title,c.id as au_id,c.title as au_title from prod as a left join prod_list as b on a.sub_id=b.id left join prod_authors as c on c.id=b.au_id where a.disp_opt='Y' and a.id in (select distinct(b.productid) from orders_hd as a left join orders as b on a.id=b.order_no where mem_id=@mem_id and b.virtual='Y' and b.end_date<>'' and @datetime between b.start_date and b.end_date)";
                }
            }
            else {
                Str_Sql = "select a.*,b.id as sub_id,b.title as sub_title,c.id as au_id,c.title as au_title from prod as a left join prod_list as b on a.sub_id=b.id left join prod_authors as c on c.id=b.au_id where a.disp_opt='Y' " + Str_Search1;
            }
            
            Library.Products.RootObject root = new Library.Products.RootObject();
            List<Library.Products.ProductData> Product = new List<Library.Products.ProductData>();
            List<Library.Products.MenuCont> ProdMenuCont = new List<Library.Products.MenuCont>();

            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Str_Sql,conn);
                if (MemberID != "")
                {
                    cmd.Parameters.Add(new SqlParameter("@mem_id", MemberID));
                    cmd.Parameters.Add(new SqlParameter("@datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    if (ID.Count > 0)
                    {
                        for (int i = 0; i < ID.Count; i++)
                        {
                            cmd.Parameters.Add(new SqlParameter("@id" + i.ToString(), ID[i].ToString()));
                        }                        
                    }
                }
                else {
                    if (ID.Count > 0)
                    {
                        for (int i = 0; i < ID.Count; i++)
                        {
                            cmd.Parameters.Add(new SqlParameter("@id" + i.ToString(), ID[i].ToString()));
                        }
                    }
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        Product = new List<Library.Products.ProductData>();
                        
                        while (reader.Read())
                        {
                            using (SqlConnection conn2 = new SqlConnection(setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2 = new SqlCommand("select * from menu_cont where type='2' and menu_id=@prodid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@prodid", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        ProdMenuCont = new List<Library.Products.MenuCont>();
                                        while (reader2.Read())
                                        {
                                            Library.Products.MenuCont List2 = new Library.Products.MenuCont
                                            {
                                                Cont = reader2["cont"].ToString(),
                                                Img = reader2["img"].ToString(),
                                                ImgAlign = reader2["img_align"].ToString(),
                                                Title = reader2["title"].ToString()
                                            };
                                            ProdMenuCont.Add(List2);
                                        }
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }

                            Library.Products.ProductData List = new Library.Products.ProductData
                            {
                                AuID = reader["au_id"].ToString(),
                                AuTitle = reader["au_title"].ToString(),
                                SubID = reader["sub_id"].ToString(),
                                SubTitle = reader["sub_title"].ToString(),
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Value1 = reader["value1"].ToString(),
                                Value2 = reader["value2"].ToString(),
                                Value3 = reader["value3"].ToString(),
                                Item1 = reader["item1"].ToString(),
                                Item2 = reader["item2"].ToString(),
                                Item3 = reader["item3"].ToString(),
                                Item4 = reader["item4"].ToString(),
                                Img1 = reader["img1"].ToString(),
                                Img2 = reader["img2"].ToString(),
                                Img3 = reader["img3"].ToString(),
                                Virtual = reader["virtual"].ToString(),
                                MenuConts = ProdMenuCont
                            };
                            Product.Add(List);                           
                        }
                    }
                }
                finally 
                {
                    reader.Close();
                }
            }
            root.ProductDatas = Product;
            ReturnStr = JsonConvert.SerializeObject(root);
            return ReturnStr;
        }
        #endregion

    }
}