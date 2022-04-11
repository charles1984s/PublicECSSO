using ECSSO.Library.ShoppingCar;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.api.Shopping
{
    /// <summary>
    /// getOrder 的摘要描述
    /// </summary>
    public class getOrder : IHttpHandler
    {
        private ShoppingCarOutput output { get; set; }
        private Token token { get; set; }
        private GetStr GS { get; set; }
        private string setting { get; set; }
        public void ProcessRequest(HttpContext context)
        {
            output = new ShoppingCarOutput { RspnCode = "401" };
            try
            {
                if (string.IsNullOrEmpty(context.Request.Form["token"])) throw new Exception("驗證錯誤");
                else if (string.IsNullOrEmpty(context.Request.Form["SiteID"])) throw new Exception("網站不存在");
                else if (string.IsNullOrEmpty(context.Request.Form["CheckM"])) throw new Exception("缺少驗證訊息");
                else
                {
                    output.RspnCode = "401";
                    Init(context.Request.Form["SiteID"], context.Request.Form["CheckM"]);
                    output.RspnCode = "404";
                    if (!token.checkToken(setting, context.Request.Form["token"]))
                    {
                        throw new Exception("驗證失敗");
                    }
                    else
                    {
                        getOrderData();
                    }
                }

            }
            catch (Exception e)
            {
                output.RspnMsg = e.Message;
            }
            finally
            {
                context.Response.Write(output.printMsg());
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #region 取得購物車資料
        private void getOrderData()
        {
            output.RspnCode = "500";
            output.Order = new Shoppingcar.OrderData();
            getOrderDetail();
        }
        #endregion

        #region 取得購物車資料
        private void setOrderTitle()
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from CurrentUseHead
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@token", token));
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    output.Order = new Shoppingcar.OrderData
                    {
                        WebTitle = reader["title"].ToString()
                    };
                }
            }
        }
        #endregion

        #region 取得購物車明細
        private void getOrderDetail()
        {
            List<Shoppingcar.OrderList> list = new List<Shoppingcar.OrderList>();
            using (SqlConnection conn = new SqlConnection(setting))
            {
                output.RspnCode = "500.1";
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select 
                        list.title listTitle,list.sales_type,list.sales_value,list.sales_qty,
                        prod.title prodTitle,prod.img1,prod.PosNo,prod.virtual,isnull(v.usetime,0) usetime,
                        case
		                    when exc.id is null then
			                    case 
				                    when Cust.vip=1 then prod.value2
				                    when Cust.vip=2 then prod.value3
				                    else prod.value2
			                    end
		                    else exc.price	
	                    end prodPrice,
	                    case
		                    when exc.id is null then 0
		                    else exc.bonus	
	                    end prodBonus,
                        isnull(color.title,'none') colorTitle,isnull(size.title,'none') sizeTitle,
                        addit.price AddPrice,
                        case 
                            when prod.Del='Y' then 'D'
                            when GETDATE() not between prod.[start_date] and prod.end_date then 'L'
                            when prod.disp_opt='N' then 'L'
                            when stock.stock<cart.qty then 'S'
                            else 'N'
                        end isDel,
                        cart.*
                    from shoppingCart as cart
                    left join token as t on t.id = cart.token
                    left join Cust on Cust.id = t.ManagerID
                    left join prod_list as list on list.id = cart.prod_sub_id
                    left join prod on prod.id = cart.prod_id
                    left join prod_Stock as stock on stock.prod_id=prod.id and stock.colorID=cart.color_id and stock.sizeID = cart.size_id
                    left join prod_Exchange as exc on exc.id = cart.priceType
                    left join prod_color as color on color.id = stock.colorID
                    left join prod_size as size on size.id = stock.sizeID
                    left join virtual_prod as v on prod.id=v.prod_id and prod.virtual='Y'
                    left join Additional_purchase as addit on cart.isAdditional='Y' and addit.prod_id=prod.id and addit.color_id=cart.color_id and addit.size_id = cart.size_id
                    where token=@token
                    order by cart.ser_no
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@token", token.token));
                SqlDataReader reader = cmd.ExecuteReader();
                output.RspnCode = "500.2";
                while (reader.Read())
                {
                    output.RspnCode = "500.3";
                    Shoppingcar.OrderList item = list.Find(e => e.ID == reader["prod_sub_id"].ToString());
                    if (item == null)
                    {
                        output.RspnCode = "500.5";
                        item = new Shoppingcar.OrderList
                        {
                            ID = reader["prod_sub_id"].ToString(),
                            Title = reader["listTitle"].ToString(),
                            Type = int.Parse(reader["sales_type"].ToString()),
                            saleQty = int.Parse(reader["sales_qty"].ToString()),
                            salePrice = double.Parse(reader["sales_value"].ToString()),
                            OrderItems = new List<Shoppingcar.OrderItem>()
                        };
                        list.Add(item);
                    }
                    output.RspnCode = "500.4";
                    Shoppingcar.OrderItem orderItem = null;
                    if (reader["isAdditional"].ToString() == "Y") {
                        output.RspnCode = "500.6";
                        orderItem = item.OrderItems.Find(e => e.ID == reader["bid"].ToString());
                        Shoppingcar.AdditionalItem add = orderItem.AdditionalItems.Find(e => e.ID == int.Parse(reader["prod_id"].ToString()));
                        if (add == null) {
                            add = new Shoppingcar.AdditionalItem
                            {
                                ID = int.Parse(reader["prod_id"].ToString()),
                                Name = reader["prodTitle"].ToString(),
                                Color = int.Parse(reader["color_id"].ToString()),
                                Size = int.Parse(reader["size_id"].ToString()),
                                ColorTitle = reader["colorTitle"].ToString(),
                                SizeTitle = reader["sizeTitle"].ToString(),
                                PosNo = reader["PosNo"].ToString(),
                                FinalPrice = int.Parse(reader["AddPrice"].ToString()),
                                Price = int.Parse(reader["prodPrice"].ToString()),
                                Qty = 1
                            };
                        }
                    }
                    else {
                        output.RspnCode = "500.7";
                        orderItem = item.OrderItems.Find(e => e.ID == reader["prod_id"].ToString());
                        if (orderItem == null)
                        {
                            orderItem = new Shoppingcar.OrderItem
                            {
                                ID = reader["prod_id"].ToString(),
                                Name = reader["prodTitle"].ToString(),
                                PosNo = reader["PosNo"].ToString(),
                                Virtual = reader["Virtual"].ToString(),
                                UseTime = reader["usetime"].ToString(),
                                isDel = reader["isDel"].ToString(),
                                URL = "index.asp?au_id="+ reader["au_id"].ToString() + 
                                    "&sub_id="+ reader["sub_id"].ToString() +
                                    "&prod_sub_id=" + reader["prod_sub_id"].ToString() +
                                    "&prod_id=" + reader["prod_id"].ToString(),
                                Img = reader["img1"].ToString()==""? "/upload/default/defaultImg.png" : reader["img1"].ToString(),
                                OrderSpecs = new List<Shoppingcar.OrderSpec>(),
                                AdditionalItems = new List<Shoppingcar.AdditionalItem>()
                            };
                            item.OrderItems.Add(orderItem);
                        }
                        Shoppingcar.OrderSpec specs = orderItem.OrderSpecs.Find(e => {
                            return
                                e.Color == int.Parse(reader["color_id"].ToString()) &&
                                e.Size == int.Parse(reader["size_id"].ToString()) &&
                                e.PriceType == int.Parse(reader["priceType"].ToString());
                        });
                        if (specs == null)
                        {
                            specs = new Shoppingcar.OrderSpec
                            {
                                Color = int.Parse(reader["color_id"].ToString()),
                                Size = int.Parse(reader["size_id"].ToString()),
                                ColorTitle = reader["colorTitle"].ToString(),
                                SizeTitle = reader["sizeTitle"].ToString(),
                                Price = double.Parse(reader["prodPrice"].ToString()),
                                Bonus = int.Parse(reader["prodBonus"].ToString()),
                                Qty = int.Parse(reader["qty"].ToString()),
                                PriceType = int.Parse(reader["priceType"].ToString())
                            };
                            orderItem.OrderSpecs.Add(specs);
                        }
                        specs.Qty = specs.Qty + int.Parse(reader["qty"].ToString());
                        setPrice(specs, item.Type);
                        specs.FinalPrice = int.Parse(reader["sales_value"].ToString());
                    }
                }
            }
            output.Order.OrderLists = list;
        }
        #endregion

        #region 設定購物車
        private void setPrice(Shoppingcar.OrderSpec spec,int type) {
            switch (type) {
                case 1:
                case 3:
                case 6:
                case 7:
                case 8:
                    break;
                case 2:
                case 4:
                case 9:
                    spec.FinalPrice = Math.Round(spec.Price * spec.FinalPrice / 100,0);
                    break;
                default:
                    spec.FinalPrice = spec.Price;
                    spec.Discount = 0;
                    break;
            }
        }
        #endregion

        #region 初始化及資料驗證
        private void Init(string SiteID, string CheckM)
        {
            token = new Token();
            GS = new GetStr();
            setting = GS.GetSetting(SiteID);
            string OrgName = GS.GetOrgName(setting);
            if (!GS.MD5Check(SiteID + OrgName, CheckM)) throw new Exception("驗證錯誤");
        }
        #endregion
    }
}