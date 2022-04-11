using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ECSSO.Library;
using ECSSO.Library.ShoppingCar;
using Newtonsoft.Json;

namespace ECSSO.api.Shopping
{
    /// <summary>
    /// ShoppingCar 的摘要描述
    /// </summary>
    public class ShoppingCar : IHttpHandler
    {
        private responseJson response { get; set; }
        private Token token { get; set; }
        private GetStr GS { get; set; }
        private ShoppingCarInput input { get; set; }
        private ShoppingCarItem item { get; set; }

        private Library.Member.Data memeber { get; set; }
        private string setting { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            response = new responseJson { RspnCode = "404" };
            try
            {
                if (string.IsNullOrEmpty(context.Request.Form["token"])) throw new Exception("驗證錯誤");
                else if (string.IsNullOrEmpty(context.Request.Form["items"])) throw new Exception("商品不存在");
                else if (string.IsNullOrEmpty(context.Request.Form["SiteID"])) throw new Exception("網站不存在");
                else if (string.IsNullOrEmpty(context.Request.Form["CheckM"])) throw new Exception("缺少驗證訊息");
                else
                {
                    response.RspnCode = "401";
                    Init(context.Request.Form["SiteID"], context.Request.Form["CheckM"]);
                    response.RspnCode = "404";
                    memeber = token.checkTokenAndGetMember(setting, context.Request.Form["token"]);
                    response.RspnCode = "404.1";
                    input = JsonConvert.DeserializeObject<ShoppingCarInput>(context.Request.Form["items"]);
                    item = JsonConvert.DeserializeObject<ShoppingCarItem>(context.Request.Form["items"]);
                    switch (context.Request.Form["Type"])
                    {
                        case "1":
                            response.RspnCode = "500";
                            addShoppingCar();
                            break;
                        default:
                            throw new Exception("操作不存在");
                    }
                }
            }
            catch (Exception e)
            {
                response.RspnMsg = e.Message;
            }
            finally
            {
                context.Response.Write(response.printMsg());
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #region 搜尋產品
        private void setProdItem()
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from prod where id=@prod_id 
                        and disp_opt='Y' and del='N' 
                        and GETDATE() between convert(datetime,[start_date]) and convert(datetime,end_date)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@prod_id", input.prod_id));
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    item.title = reader["title"].ToString();
                    //setProdPrice();
                }
                else throw new Exception("商品不存在");
            }
        }
        #endregion

        #region 搜尋產品價格
        private void setProdPrice() {
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from prodPrice 
                    where 
	                    prod_id=@prodID and stock>@qty and
                        isnull(colorID,0)=@colorID and 
		                isnull(sizeID,0)=@sizeID and
	                    (
		                    case @priceType 
			                    WHEN id then 1
			                    else
				                    case isnull(id,0)
					                    when 0 then
						                    case @vip
							                    WHEN vip then 1
							                    else 0
						                    end
					                    else 0
				                    end
		                    end = 1
	                    )
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@priceType", input.priceType));
                cmd.Parameters.Add(new SqlParameter("@colorID", input.prod_color));
                cmd.Parameters.Add(new SqlParameter("@sizeID", input.prod_size));
                cmd.Parameters.Add(new SqlParameter("@vip", memeber.Vip+1));
                cmd.Parameters.Add(new SqlParameter("@qty", input.qty));
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    if (reader.IsDBNull(reader.GetOrdinal("bonus"))) item.bonus = 0;
                    else item.bonus = int.Parse(reader["bonus"].ToString());

                    if (reader.IsDBNull(reader.GetOrdinal("price"))) item.price = 0;
                    else item.price = int.Parse(reader["price"].ToString());

                    if (item.bonus != 0 && memeber.Vip!=0) throw new Exception("請登入會員");
                }
                else throw new Exception(item.title + "庫存不足");
            }
        }
        #endregion

        #region 加入購物車
        private void addShoppingCar()
        {
            setProdItem();
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"sp_addShoppingCar");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@token", memeber.token));
                cmd.Parameters.Add(new SqlParameter("@auID", input.au_id));
                cmd.Parameters.Add(new SqlParameter("@subID", input.sub_id));
                cmd.Parameters.Add(new SqlParameter("@prodSubID", input.prod_sub_id));
                cmd.Parameters.Add(new SqlParameter("@prodID", input.prod_id));
                cmd.Parameters.Add(new SqlParameter("@qty", input.qty));
                cmd.Parameters.Add(new SqlParameter("@sizeID", input.prod_size));
                cmd.Parameters.Add(new SqlParameter("@colorID", input.prod_color));
                cmd.Parameters.Add(new SqlParameter("@priceType", input.priceType));
                cmd.Parameters.Add(new SqlParameter("@isAdditional", input.isAdditional?"Y":"N"));
                cmd.Parameters.Add(new SqlParameter("@bid", input.bid));
                SqlParameter SPOutput = cmd.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 50);
                SPOutput.Direction = ParameterDirection.Output;
                SqlDataReader reader = cmd.ExecuteReader();
                string ReturnCode = SPOutput.Value.ToString();
                switch (ReturnCode) {
                    case "error:1":
                        response.RspnCode = "500.1";
                        throw new Exception("庫存不足");
                    case "error:2":
                    case "error:3":
                        response.RspnCode = "500.3";
                        throw new Exception("請登入會員");
                    case "error:4":
                        response.RspnCode = "500.4";
                        throw new Exception("商品已下架");
                    case "error:5":
                        response.RspnCode = "500.5";
                        throw new Exception("商品加價購活動已結束");
                    case "success":
                        response.RspnCode = "200";
                        response.RspnMsg = "新增成功";
                        break;
                    default:
                        throw new Exception("操作不存在");
                }
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