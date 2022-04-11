using ECSSO.Library;
using ECSSO.Library.ShopLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ECSSO.api.ShopItem
{
    /// <summary>
    /// highlightsShop 的摘要描述
    /// </summary>
    public class HighlightsShopHandler : IHttpHandler
    {
        private HighlightsShop shop;
        private CheckToken checkToken;
        public void ProcessRequest(HttpContext context)
        {
            shop = new HighlightsShop();
            checkToken = new CheckToken(shop);
            try {
                checkToken.check(context);
                if (shop.RspnCode == "200") {
                    try
                    {
                        shop.menuID = int.Parse(context.Request.Form["id"]);
                        switch (context.Request.Form["type"])
                        {
                            case "Get":
                                getShop();
                                if (shop.id == 0) initShop();
                                break;
                            case "Update":
                                shop.RspnCode = "500.1";
                                HighlightsShop request = JsonConvert.DeserializeObject<HighlightsShop>(context.Request.Form["data"]);
                                shop.RspnCode = "500.2";
                                shop = request;
                                shop.RspnCode = "500.3";
                                checkToken.response = shop;
                                shop.RspnCode = "500.4";
                                updateShop();
                                break;
                        }
                        shop.RspnCode = "200";
                    }
                    catch(Exception e) {
                        throw new Exception(e.Message);
                    }
                }else throw new Exception("Token不存在");
            }
            catch (Exception ex)
            {
                shop.RspnMsg = ex.Message;
            }
            finally
            {
                context.Response.Write(checkToken.printMsg());
            }
        }
        private void updateMenu() {
            shop.RspnCode = "500.31";
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update menu set cont = @Toldescribe where id=@menuID;
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@menuID", shop.menuID));
                cmd.Parameters.Add(new SqlParameter("@Toldescribe", shop.Toldescribe));
                try
                {
                    cmd.ExecuteReader();
                    shop.RspnMsg = "儲存成功";
                }
                catch
                {
                    throw new Exception("儲存失敗");
                }
            }
        }
        private void updateShop() {
            shop.RspnCode = "500.3";
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update shopInfo set 
                        name=@name,name_en=@name_en,
                        [Add]=@Add,[Add_en]=@Add_en,
                        Toldescribe=@Toldescribe,Toldescribe_en=@Toldescribe_en,
                        Opentime=@Opentime,Opentime_en=@Opentime_en,
                        Tel=@Tel,Tel2=@Tel2,Website=@Website,Fax=@Fax
                    where id=@id;
                    update menu set cont = @Toldescribe where id=@menuID;
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", shop.id));
                cmd.Parameters.Add(new SqlParameter("@menuID", shop.menuID));
                cmd.Parameters.Add(new SqlParameter("@name", shop.name));
                cmd.Parameters.Add(new SqlParameter("@name_en", shop.name_en));
                cmd.Parameters.Add(new SqlParameter("@Add", shop.Add));
                cmd.Parameters.Add(new SqlParameter("@Add_en", shop.Add_en));
                cmd.Parameters.Add(new SqlParameter("@Toldescribe", shop.Toldescribe));
                cmd.Parameters.Add(new SqlParameter("@Toldescribe_en", shop.Toldescribe_en));
                cmd.Parameters.Add(new SqlParameter("@Opentime", shop.Opentime));
                cmd.Parameters.Add(new SqlParameter("@Opentime_en", shop.Opentime_en));
                cmd.Parameters.Add(new SqlParameter("@Tel", shop.Tel));
                cmd.Parameters.Add(new SqlParameter("@Tel2", shop.Tel2));
                cmd.Parameters.Add(new SqlParameter("@Fax", shop.Fax));
                cmd.Parameters.Add(new SqlParameter("@Website", shop.Website));
                try
                {
                    cmd.ExecuteReader();
                    updateMenu();
                    shop.RspnMsg = "儲存成功";
                }
                catch
                {
                    throw new Exception("儲存失敗");
                }
            }
        }
        private void initShop() {
            shop.RspnCode = "500.2";
            using (SqlConnection conn = new SqlConnection(checkToken.setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    insert into shopInfo(menuID)
                    output inserted.id
                    values(@menuID)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@menuID", shop.menuID));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        shop.id = int.Parse(reader["id"].ToString());
                        shop.name = "";
                        shop.name_en = "";
                        shop.Add = "";
                        shop.Add_en = "";
                        shop.Opentime = "";
                        shop.Opentime_en = "";
                        shop.Toldescribe = "";
                        shop.Toldescribe_en = "";
                        shop.Tel = "";
                        shop.Tel2 = "";
                        shop.Website = "";
                        shop.Fax = "";
                    }
                }
                catch
                {
                    throw new Exception("新增失敗");
                }
                finally {
                    if (reader != null) reader.Close();
                }
            }
        }
        private void getShop() {
            shop.RspnCode = "500.1";
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select
                        id,menuID,name,name_en,[Add],[Add_en],Toldescribe,Toldescribe_en,
                        Opentime,Opentime_en ,Tel,Tel2,Website,Fax
                    from shopInfo
                    where menuID=@menuID
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@menuID", shop.menuID));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        shop.id = int.Parse(reader["id"].ToString());
                        shop.name = reader["name"].ToString();
                        shop.name_en = reader["name_en"].ToString();
                        shop.Add = reader["Add"].ToString();
                        shop.Add_en = reader["Add_en"].ToString();
                        shop.Opentime = reader["Opentime"].ToString();
                        shop.Opentime_en = reader["Opentime_en"].ToString();
                        shop.Toldescribe = reader["Toldescribe"].ToString();
                        shop.Toldescribe_en = reader["Toldescribe_en"].ToString();
                        shop.Tel = reader["Tel"].ToString();
                        shop.Tel2 = reader["Tel2"].ToString();
                        shop.Website = reader["Website"].ToString();
                        shop.Fax = reader["Fax"].ToString();
                    }
                }
                catch
                {
                    throw new Exception("查詢失敗");
                }
                finally {
                    if (reader != null) reader.Close();
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
    }
}