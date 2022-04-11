using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using ECSSO.Library;

namespace ECSSO.api
{
    /// <summary>
    /// GetProd 的摘要描述
    /// </summary>
    public class GetProd : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "error:4", "", ""));
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "error:4", "", ""));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "error:4", "", ""));
            if (context.Request.Params["Items"] == null) ResponseWriteEnd(context, ErrorMsg("error", "error:4", "", ""));

            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "error:4", "", ""));
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "error:4", "", ""));
            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "error:4", "", ""));
            if (context.Request.Params["Items"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "error:4", "", ""));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String Type = context.Request.Params["Type"].ToString();

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            if (!GS.MD5Check(Type + SiteID + GS.GetOrgName(Setting), ChkM)) 
            {
                ResponseWriteEnd(context, ErrorMsg("error", "error:3", Setting, "檢查參數"));     //驗證碼錯誤
            }

            Product.InputData postf = null;
            try
            {
                postf = JsonConvert.DeserializeObject<Product.InputData>(context.Request.Params["Items"]);
            }
            catch
            {
                ResponseWriteEnd(context, ErrorMsg("error", "error:5", Setting, "檢查參數"));
            }

            String LogStr = "Type = " + Type + ",SiteID = " + SiteID + ",ChkM = " + ChkM + ",Items = " + context.Request.Params["Items"];
            InsertLog(Setting, "商品API", "", LogStr);

            String AuID = "";
            String SubID = "";
            String From = "";
            String GetCount = "";
            String ReturnStr = "";
            String WebURL = GS.GetDefaultURL(SiteID);

            switch (Type)
            {
                case "1":   //取得商品大類
                    try 
                    {
                        if (postf.Range.From != "" && postf.Range.GetCount != "")
                        {
                            From = postf.Range.From;
                            GetCount = postf.Range.GetCount;

                            ReturnStr = GetProdAu(Setting, From, GetCount);
                            InsertLog(Setting, "取得商品大類", "", ReturnStr);
                            
                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得商品大類");
                            
                        }
                    }
                    catch 
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得商品大類");
                    }

                    ResponseWriteEnd(context, ReturnStr);
                    break;

                case "2":   //取得商品分類
                    try 
                    {
                        if (postf.AuID != "" && postf.Range.From != "" && postf.Range.GetCount != "")
                        {
                            AuID = postf.AuID;
                            From = postf.Range.From;
                            GetCount = postf.Range.GetCount;

                            ReturnStr = GetProdSub(Setting, AuID, From, GetCount, WebURL);
                            InsertLog(Setting, "取得商品分類", "", ReturnStr);
                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得商品分類");
                        }
                    }
                    catch
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得商品分類");
                    }
                    ResponseWriteEnd(context, ReturnStr);
                    break;

                case "3":   //取得商品列表
                    try
                    {
                        if (postf.SubID != "" && postf.Range.From != "" && postf.Range.GetCount != "")
                        {
                            SubID = postf.SubID;
                            From = postf.Range.From;
                            GetCount = postf.Range.GetCount;

                            ReturnStr = GetProdList(Setting, SubID, From, GetCount, WebURL);
                            InsertLog(Setting, "取得商品列表", "", ReturnStr);
                            
                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得商品列表");
                        }
                    }
                    catch
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得商品列表");
                    }
                    ResponseWriteEnd(context, ReturnStr);
                    
                    break;
                case "4":   //取得商品資料
                    try
                    {
                        if (postf.ID != "")
                        {
                            String ID = postf.ID;

                            ReturnStr = GetProdDetail(Setting, ID, WebURL);
                            InsertLog(Setting, "取得商品資料", "", ReturnStr);
                            
                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得商品資料");
                        }
                    }
                    catch
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得商品資料");
                    }
                    ResponseWriteEnd(context, ReturnStr);
                    break;
                case "5":   //取得最新商品列表
                    try
                    {
                        if (postf.Range.From != "" && postf.Range.GetCount != "")
                        {
                            From = postf.Range.From;
                            GetCount = postf.Range.GetCount;

                            ReturnStr = GetNewProdList(Setting, From, GetCount, WebURL);
                            InsertLog(Setting, "取得最新商品列表", "", ReturnStr);
                            
                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得最新商品列表");
                        }
                    }
                    catch
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得最新商品列表");
                    }
                    ResponseWriteEnd(context, ReturnStr);
                    
                    break;
                case "6":   //取得熱銷商品
                    try
                    {
                        if (postf.Range.GetCount != "" && postf.StartDate != "" && postf.EndDate != "")
                        {
                            String StartDate = "";
                            String EndDate = "";

                            try
                            {
                                StartDate = DateTime.Parse(postf.StartDate).ToString("yyyy/MM/dd");
                            }
                            catch 
                            { 
                                ReturnStr = ErrorMsg("error", "error:4", Setting, "取得熱銷商品列表"); 
                            }

                            try
                            {
                                EndDate = DateTime.Parse(postf.EndDate).ToString("yyyy/MM/dd");
                            }
                            catch 
                            { 
                                ReturnStr = ErrorMsg("error", "error:4", Setting, "取得熱銷商品列表"); 
                            }

                            GetCount = postf.Range.GetCount;

                            ReturnStr = GetHotProdList(Setting, GetCount, StartDate, EndDate, WebURL);
                            InsertLog(Setting, "取得熱銷商品列表", "", ReturnStr);

                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得熱銷商品列表");
                        }
                    }
                    catch
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得熱銷商品列表");
                    }
                    ResponseWriteEnd(context, ReturnStr);
                    break;
                case "7":   //取得商品+規格列表
                    try
                    {
                        if (postf.SubID != "" && postf.Range.From != "" && postf.Range.GetCount != "")
                        {
                            SubID = postf.SubID;
                            From = postf.Range.From;
                            GetCount = postf.Range.GetCount;

                            ReturnStr = GetProdAndSpecList(Setting, SubID, From, GetCount, WebURL);
                            InsertLog(Setting, "取得商品+規格列表", "", ReturnStr);

                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得商品列表");
                        }
                    }
                    catch
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得商品列表");
                    }
                    ResponseWriteEnd(context, ReturnStr);
                    break;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #region Get IP
        private string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string sIPAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(sIPAddress))
            {
                return context.Request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                string[] ipArray = sIPAddress.Split(new Char[] { ',' });
                return ipArray[0];
            }
        }
        #endregion

        #region insert log
        private void InsertLog(String Setting, String JobName, String JobTitle, String Detail)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_userlogAdd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", "guest"));
                cmd.Parameters.Add(new SqlParameter("@prog_name", "呼叫商品API"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " /tat/api/getprod.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting,String Txt)
        {
            String ReturnStr = "";
            
            Library.Product.ErrorObject root = new Library.Product.ErrorObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            ReturnStr = JsonConvert.SerializeObject(root);

            if (Setting != "") 
            {
                InsertLog(Setting, Txt, "", ReturnStr);
            }

            return ReturnStr;


        }
        #endregion

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }

        #region 取得商品大類
        private String GetProdAu(String Setting, String From, String GetCount)
        {
            #region 產生SQL
            String Str_Sql = "";
            Str_Sql += "WITH CTEResults AS ";
            Str_Sql += "(";
            Str_Sql += "     select id,title,ROW_NUMBER() OVER ";
            Str_Sql += "(ORDER BY ser_no)";
            Str_Sql += "    AS RowNum";
            Str_Sql += "    from prod_authors";
            Str_Sql += "    where disp_opt='Y' and type='1'";
            Str_Sql += ") ";
            Str_Sql += " SELECT *,";
            Str_Sql += " (SELECT MAX(RowNum) FROM CTEResults)  as TotalRows";//  ---### 這裡會回傳總筆數 
            Str_Sql += " FROM CTEResults ";

            #region 分頁處理
            if (GetCount != "max")
            {
                Str_Sql += " WHERE RowNum BETWEEN " + From + " AND " + (Convert.ToInt32(From) + Convert.ToInt32(GetCount) - 1);
            }
            #endregion
            #endregion
            InsertLog(Setting, "取得商品大類", "", Str_Sql);
            List<Product.ProductAu> ProductAu = null;

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(Str_Sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        ProductAu = new List<Product.ProductAu>();

                        while (reader.Read())
                        {
                            Product.ProductAu AuList = new Product.ProductAu
                            {
                                AuID = reader["id"].ToString(),
                                Title = reader["title"].ToString()
                            };
                            ProductAu.Add(AuList);
                        }
                        
                    }
                    else 
                    {
                        return ErrorMsg("error", "error:2", Setting, "取得商品大類");
                    }
                }
                catch 
                {
                    return ErrorMsg("error", "error:1", Setting, "取得商品大類");
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(ProductAu);
        }
        #endregion

        #region 取得商品分類
        private String GetProdSub(String Setting, String AuID, String From, String GetCount, String WebURL)
        {
            #region 產生SQL
            String Str_Sql = "";
            Str_Sql += "WITH CTEResults AS ";
            Str_Sql += "(";
            Str_Sql += "     select id,title,banner_img,ROW_NUMBER() OVER ";
            Str_Sql += "(ORDER BY ser_no)";
            Str_Sql += "    AS RowNum";
            Str_Sql += "    from prod_list";
            Str_Sql += " where au_id=@au_id and disp_opt='Y' and type=''";
            Str_Sql += ") ";
            Str_Sql += " SELECT *,";
            Str_Sql += " (SELECT MAX(RowNum) FROM CTEResults)  as TotalRows";//  ---### 這裡會回傳總筆數 
            Str_Sql += " FROM CTEResults ";

            #region 分頁處理
            if (GetCount != "max")
            {
                Str_Sql += " WHERE RowNum BETWEEN " + From + " AND " + (Convert.ToInt32(From) + Convert.ToInt32(GetCount) - 1);
            }
            #endregion
            #endregion
            InsertLog(Setting, "取得商品分類", "", Str_Sql);
            List<Product.ProductSub> ProductSub = null;
            String img1 = "";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@au_id", AuID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        ProductSub = new List<Product.ProductSub>();

                        while (reader.Read())
                        {

                            if (reader["banner_img"].ToString() == "")
                            {
                                img1 = "";
                            }
                            else
                            {
                                if (reader["banner_img"].ToString().Contains("http"))
                                {
                                    img1 = reader["banner_img"].ToString();
                                }
                                else
                                {
                                    img1 = "http://" + WebURL + reader["banner_img"].ToString();
                                }
                            }

                            Product.ProductSub SubList = new Product.ProductSub
                            {
                                SubID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                BannerImg = img1
                            };
                            ProductSub.Add(SubList);
                        }
                    }
                    else
                    {
                        return ErrorMsg("error", "error:2", Setting, "取得商品分類");
                    }
                }
                catch
                {
                    return ErrorMsg("error", "error:1", Setting, "取得商品分類");
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(ProductSub);
        }
        #endregion

        #region 取得商品列表
        private String GetProdList(String Setting, String SubID, String From, String GetCount, String WebURL)
        {
            String img1 = "";
            #region 產生SQL
            String Str_Sql = "";
            Str_Sql += "WITH CTEResults AS ";
            Str_Sql += "(";
            Str_Sql += "     select id,title,img1,value1,value2,value3,ROW_NUMBER() OVER ";
            Str_Sql += "(ORDER BY ser_no)";
            Str_Sql += "    AS RowNum";
            Str_Sql += "    from prod";
            Str_Sql += " where sub_id=@sub_id and disp_opt='Y' and @date between start_date and end_date";
            Str_Sql += ") ";
            Str_Sql += " SELECT *,";
            Str_Sql += " (SELECT MAX(RowNum) FROM CTEResults)  as TotalRows";//  ---### 這裡會回傳總筆數 
            Str_Sql += " FROM CTEResults ";

            #region 分頁處理
            if (GetCount != "max")
            {
                Str_Sql += " WHERE RowNum BETWEEN " + From + " AND " + (Convert.ToInt32(From) + Convert.ToInt32(GetCount) - 1);
            }
            #endregion
            #endregion
            InsertLog(Setting, "取得商品列表", "", Str_Sql);
            List<Product.ProductList> ProductList = null;

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@sub_id", SubID));
                cmd.Parameters.Add(new SqlParameter("@date", DateTime.Now.ToString("yyyy-MM-dd")));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        ProductList = new List<Product.ProductList>();

                        while (reader.Read())
                        {

                            if (reader["img1"].ToString() == "")
                            {
                                img1 = "";
                            }
                            else
                            {
                                if (reader["img1"].ToString().Contains("http"))
                                {
                                    img1 = reader["img1"].ToString();
                                }
                                else
                                {
                                    img1 = "http://" + WebURL + reader["img1"].ToString();
                                }
                            }

                            Product.ProductList PList = new Product.ProductList
                            {
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Img1 = img1,
                                Value1 = reader["value1"].ToString(),
                                Value2 = reader["value2"].ToString(),
                                Value3 = reader["value3"].ToString()
                            };
                            ProductList.Add(PList);
                        }
                    }
                    else
                    {
                        return ErrorMsg("error", "error:2", Setting, "取得商品列表");
                    }
                }
                catch
                {
                    return ErrorMsg("error", "error:1", Setting, "取得商品列表");
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(ProductList);
        }
        #endregion

        #region 取得商品資料
        private String GetProdDetail(String Setting, String ID, String WebURL)
        {
            Product.ProductDetail PList = null;
            List<Product.ProductStock> PStock = new List<Product.ProductStock>();
            Product.ProductStock PSList = null;

            String Str_Sql = "select sub_id,id,title,img1,img2,img3,value1,value2,value3,item1,item2,item3,item4 from prod where disp_opt='Y' and id=@id and @date between start_date and end_date";
            InsertLog(Setting, "取得商品資料", "", Str_Sql);
            String img1 = "";
            String img2 = "";
            String img3 = "";
            String SalesQty = "0";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", ID));
                cmd.Parameters.Add(new SqlParameter("@date", DateTime.Now.ToString("yyyy-MM-dd")));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("select a.sizeID,isnull(b.title,'') as sizetitle,a.colorID,isnull(c.title,'') as colortitle,a.stock from prod_Stock as a left join prod_size as b on a.sizeID=b.id left join prod_color as c on a.colorID=c.id where a.prod_id=@id", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@id", ID));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            PSList = new Product.ProductStock
                                            {
                                                ColorID = reader2["colorid"].ToString(),
                                                ColorName = reader2["colortitle"].ToString(),
                                                SizeID = reader2["sizeID"].ToString(),
                                                SizeName = reader2["sizetitle"].ToString(),
                                                Num = reader2["stock"].ToString()
                                            };
                                            PStock.Add(PSList);
                                        }
                                    }
                                }
                                catch
                                {
                                    return ErrorMsg("error", "error:1", Setting, "取得商品資料");
                                }
                                finally { reader2.Close(); }
                            }

                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("select isnull(SUM(qty),'0') from orders where productid=@id", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@id", ID));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            SalesQty = reader2[0].ToString();
                                        }
                                    }
                                }
                                catch
                                {
                                    return ErrorMsg("error", "error:1", Setting, "取得商品資料");
                                }
                                finally { reader2.Close(); }
                            }

                            if (reader["img1"].ToString() == "")
                            {
                                img1 = "";
                            }
                            else 
                            {
                                if (reader["img1"].ToString().Contains("http"))
                                {
                                    img1 = reader["img1"].ToString();
                                }
                                else 
                                {
                                    img1 = "http://" + WebURL + reader["img1"].ToString();
                                }
                            }

                            if (reader["img2"].ToString() == "")
                            {
                                img2 = "";
                            }
                            else
                            {
                                if (reader["img2"].ToString().Contains("http"))
                                {
                                    img2 = reader["img2"].ToString();
                                }
                                else
                                {
                                    img2 = "http://" + WebURL + reader["img2"].ToString();
                                }
                            }

                            if (reader["img3"].ToString() == "")
                            {
                                img3 = "";
                            }
                            else
                            {
                                if (reader["img3"].ToString().Contains("http"))
                                {
                                    img3 = reader["img3"].ToString();
                                }
                                else
                                {
                                    img3 = "http://" + WebURL + reader["img3"].ToString();
                                }
                            }
                            
                            PList = new Product.ProductDetail
                            {
                                SubID = reader["sub_id"].ToString(),
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Img1 = img1,
                                Img2 = img2,
                                Img3 = img3,
                                Value1 = reader["value1"].ToString(),
                                Value2 = reader["value2"].ToString(),
                                Value3 = reader["value3"].ToString(),
                                item1 = HttpUtility.HtmlEncode(reader["item1"].ToString()),
                                item2 = HttpUtility.HtmlEncode(reader["item2"].ToString()),
                                item3 = HttpUtility.HtmlEncode(reader["item3"].ToString()),
                                item4 = HttpUtility.HtmlEncode(reader["item4"].ToString()),
                                SalesQty = SalesQty,
                                Stock = PStock
                            };

                            PStock = new List<Product.ProductStock>();
                        }
                    }
                    else
                    {
                        return ErrorMsg("error", "error:2", Setting, "取得商品資料");
                    }
                }
                catch
                {
                    return ErrorMsg("error", "error:1", Setting, "取得商品資料");
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(PList);
        }
        #endregion

        #region 取得最新商品列表
        private String GetNewProdList(String Setting, String From, String GetCount, String WebURL)
        {
            String img1 = "";
            #region 產生SQL
            String Str_Sql = "";
            Str_Sql += "WITH CTEResults AS ";
            Str_Sql += "(";
            Str_Sql += "     select id,title,img1,value1,value2,value3,ROW_NUMBER() OVER ";
            Str_Sql += "(ORDER BY id desc)";
            Str_Sql += "    AS RowNum";
            Str_Sql += "    from prod";
            Str_Sql += " where disp_opt='Y' and @date between start_date and end_date";
            Str_Sql += ") ";
            Str_Sql += " SELECT *,";
            Str_Sql += " (SELECT MAX(RowNum) FROM CTEResults)  as TotalRows";//  ---### 這裡會回傳總筆數 
            Str_Sql += " FROM CTEResults ";

            #region 分頁處理
            if (GetCount != "max")
            {
                Str_Sql += " WHERE RowNum BETWEEN " + From + " AND " + (Convert.ToInt32(From) + Convert.ToInt32(GetCount) - 1);
            }
            #endregion
            #endregion
            InsertLog(Setting, "取得最新商品列表", "", Str_Sql);
            List<Product.ProductList> ProductList = null;

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@date", DateTime.Now.ToString("yyyy-MM-dd")));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        ProductList = new List<Product.ProductList>();

                        while (reader.Read())
                        {

                            if (reader["img1"].ToString() == "")
                            {
                                img1 = "";
                            }
                            else
                            {
                                if (reader["img1"].ToString().Contains("http"))
                                {
                                    img1 = reader["img1"].ToString();
                                }
                                else
                                {
                                    img1 = "http://" + WebURL + reader["img1"].ToString();
                                }
                            }

                            Product.ProductList PList = new Product.ProductList
                            {
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Img1 = img1,
                                Value1 = reader["value1"].ToString(),
                                Value2 = reader["value2"].ToString(),
                                Value3 = reader["value3"].ToString()
                            };
                            ProductList.Add(PList);
                        }
                    }
                    else
                    {
                        return ErrorMsg("error", "error:2", Setting, "取得最新商品列表");
                    }
                }
                catch
                {
                    return ErrorMsg("error", "error:1", Setting, "取得最新商品列表");
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(ProductList);
        }
        #endregion

        #region 取得熱銷商品列表
        private String GetHotProdList(String Setting, String GetCount, String StartDate, String EndDate, String WebURL)
        {
            String img1 = "";
            String Value1 = "";
            String Value2 = "";
            String Value3 = "";

            List<Product.ProductList> ProductList = null;
            
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_hotitem";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@startdate", StartDate));
                cmd.Parameters.Add(new SqlParameter("@enddate", EndDate));
                cmd.Parameters.Add(new SqlParameter("@rownum", GetCount));
                
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        ProductList = new List<Product.ProductList>();

                        while (reader.Read())
                        {

                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("select value1,value2,value3 from prod where id=@id", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@id", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            Value1 = reader2["value1"].ToString();
                                            Value2 = reader2["value2"].ToString();
                                            Value3 = reader2["value3"].ToString();
                                        }
                                    }
                                }
                                catch
                                {
                                    return ErrorMsg("error", "error:12", Setting, "取得熱銷商品列表");
                                }
                                finally { reader2.Close(); }
                            }

                            if (reader["img1"].ToString() == "")
                            {
                                img1 = "";
                            }
                            else
                            {
                                if (reader["img1"].ToString().Contains("http"))
                                {
                                    img1 = reader["img1"].ToString();
                                }
                                else
                                {
                                    img1 = "http://" + WebURL + reader["img1"].ToString();
                                }
                            }

                            Product.ProductList PList = new Product.ProductList
                            {
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Img1 = img1,
                                Value1 = Value1,
                                Value2 = Value2,
                                Value3 = Value3
                            };
                            ProductList.Add(PList);
                        }
                    }
                    else
                    {
                        return ErrorMsg("error", "error:2", Setting, "取得熱銷商品列表");
                    }
                }
                catch
                {
                    return ErrorMsg("error", "error:13", Setting, "取得熱銷商品列表");
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(ProductList);
        }
        #endregion

        #region 取得商品+規格列表
        private String GetProdAndSpecList(String Setting, String SubID, String From, String GetCount, String WebURL)
        {
            String img1 = "";
            #region 產生SQL
            String Str_Sql = @";
                WITH CTEResults AS(
                    select a.id,a.title,a.img1,b.ser_no,c.title as colorTitle,d.title as sizeTitle,ROW_NUMBER() OVER 
                        (ORDER BY a.ser_no)
                        AS RowNum
                    from prod as a
                    left join prod_Stock as b on a.id=b.prod_id
                    left join prod_color as c on b.colorID=c.id
                    left join prod_size as d on b.sizeID=d.id
                    where a.sub_id=@sub_id and a.disp_opt='Y' and @date between start_date and a.end_date
                ) 
                SELECT *,
                    (SELECT MAX(RowNum) FROM CTEResults)  as TotalRows
                FROM CTEResults ";

            #region 分頁處理
            if (GetCount != "max")
            {
                Str_Sql += " WHERE RowNum BETWEEN " + From + " AND " + (Convert.ToInt32(From) + Convert.ToInt32(GetCount) - 1);
            }
            #endregion
            #endregion
            InsertLog(Setting, "取得商品+規格列表", "", Str_Sql);
            List<Product.ProductList> ProductList = null;

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@sub_id", SubID));
                cmd.Parameters.Add(new SqlParameter("@date", DateTime.Now.ToString("yyyy-MM-dd")));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        ProductList = new List<Product.ProductList>();

                        while (reader.Read())
                        {

                            if (reader["img1"].ToString() == "")
                            {
                                img1 = "";
                            }
                            else
                            {
                                if (reader["img1"].ToString().Contains("http"))
                                {
                                    img1 = reader["img1"].ToString();
                                }
                                else
                                {
                                    img1 = "http://" + WebURL + reader["img1"].ToString();
                                }
                            }

                            Product.ProductList PList = new Product.ProductList
                            {
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Img1 = img1,
                                StockNo = reader["ser_no"].ToString(),
                                ColorTitle = reader["colorTitle"].ToString(),
                                SizeTitle = reader["sizeTitle"].ToString()
                            };
                            ProductList.Add(PList);
                        }
                    }
                    else
                    {
                        return ErrorMsg("error", "error:2", Setting, "取得商品+規格列表");
                    }
                }
                catch
                {
                    return ErrorMsg("error", "error:1", Setting, "取得商品+規格列表");
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(ProductList);
        }
        #endregion
    }
}