using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ECSSO.Library;
using ECSSO.Library.Prod;
using Newtonsoft.Json;

namespace ECSSO.api.Prod
{
    /// <summary>
    /// ProdHandelar 的摘要描述
    /// </summary>
    public class ProdHandelar : IHttpHandler
    {
        private GetStr GS;
        private responseJson prod;
        private TokenItem token;
        private CheckToken checkToken;
        private string setting;

        public void ProcessRequest(HttpContext context)
        {
            string code, message;
            code = "404";
            message = "not fount";
            prod = new responseJson();
            checkToken = new CheckToken();
            try
            {
                checkToken.check(context);
                prod = checkToken.response;
                token = checkToken.token;
                GS = checkToken.GS;
                code = prod.RspnCode;
                if (prod.RspnCode == "200")
                {
                    switch (context.Request.Form["type"])
                    {
                        case "ProdAuthors":
                            prod = new ProdAuList();
                            getProdAuthors((ProdAuList)prod);
                            break;
                        case "ProdSub":
                            int auid = 0;
                            prod = new ProdSubList();
                            if (int.TryParse(context.Request.Form["auID"], out auid))
                            {
                                getProdSub((ProdSubList)prod, auid);
                            }
                            else throw new Exception("沒有搜尋對象");
                            break;
                        case "ProdList":
                            int subID = 0;
                            prod = new ProdList();
                            if (int.TryParse(context.Request.Form["subID"], out subID))
                            {
                                string text = context.Request.Form["text"];
                                getProdList((ProdList)prod, subID, text);
                            }
                            else throw new Exception("沒有搜尋對象");
                            break;
                        default:
                            code = "404";
                            message = "no type";
                            break;
                    }
                }
                else throw new Exception("Token不存在");
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                context.Response.Write(printMsg(code, message));
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private void getProdAuthors(ProdAuList list)
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select id,title,disp_opt from prod_authors where [type]=1 order by disp_opt desc,ser_no,id desc
                ", conn);
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    list.list = new List<Product.ProductAu>();
                    while (reader.Read())
                    {
                        Product.ProductAu productAu = new Product.ProductAu { 
                            AuID = reader["id"].ToString(),
                            Title = reader["title"].ToString() + (reader["disp_opt"].ToString()=="Y"?"":"(不顯示)")
                        };
                        list.list.Add(productAu);
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private void getProdSub(ProdSubList list,int auID)
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select id,title,disp_opt 
                    from prod_list
                    where au_id=@au_id
                    order by disp_opt desc,ser_no,id desc
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@au_id", auID));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    list.list = new List<Product.ProductSub>();
                    while (reader.Read())
                    {
                        Product.ProductSub productSub = new Product.ProductSub
                        {
                            SubID = reader["id"].ToString(),
                            Title = reader["title"].ToString() + (reader["disp_opt"].ToString() == "Y" ? "" : "(不顯示)")
                        };
                        list.list.Add(productSub);
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private void getProdList(ProdList list, int subID, string text)
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select top 100 p.id,p.title,p.disp_opt 
                    from prod as p
                    where 
	                    case 
		                    when @subID=0 and @searchstr!='' then 1
		                    when @subID=p.sub_id then 1
		                    else 0
	                    end =1
	                    and
	                    case 
		                    when @searchstr='' and @subID=0 then 0
                            when p.id like '%' + @searchstr + '%' then 1
                            when p.itemno like '%' + @searchstr + '%' then 1
                            when p.title like '%' + @searchstr + '%' then 1
                            when exists(
			                    select prod_Stock.prod_id from prod_Stock
			                    left join prod_size on prod_Stock.sizeID=prod_size.id
			                    left join prod_color on prod_Stock.colorID=prod_color.id
			                    where prod_Stock.prod_id=p.id and
				                    (prod_size.title like '%' + @searchstr + '%' or 
				                    prod_color.title like '%' + @searchstr + '%')
		                    ) then 1
		                    when exists(
			                    select prod_tag.id from prod_tag
			                    left join tag on prod_tag.tag_id=tag.id
			                    where prod_tag.prod_id=p.id and prod_tag.[type]='prod' and
				                    tag.title like '%' + @searchstr + '%'
		                    ) then 1
                            when @searchstr='' and @subID!=0 then 1
                            else 0
                        end = 1
                    order by disp_opt desc,ser_no,id desc
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@subID", subID));
                cmd.Parameters.Add(new SqlParameter("@searchstr", text));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    list.list = new List<Product.ProductList>();
                    while (reader.Read())
                    {
                        Product.ProductList productList = new Product.ProductList
                        {
                            ID = reader["id"].ToString(),
                            Title = reader["title"].ToString() + (reader["disp_opt"].ToString() == "Y" ? "" : "(不顯示)")
                        };
                        list.list.Add(productList);
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private String printMsg(String RspnCode, String RspnMsg)
        {
            prod.RspnCode = RspnCode;
            prod.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(prod);
        }
    }
}