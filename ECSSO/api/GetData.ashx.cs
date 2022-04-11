using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ECSSO.Library;

namespace ECSSO.api
{
    /// <summary>
    /// GetData 的摘要描述
    /// </summary>
    public class GetData : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form["ApiCode"] == null || context.Request.Form["ApiCode"] == "") ResponseWriteEnd(context, ErrorMsg("error", "ApiCode必填"));
            if (context.Request.Form["Email"] == null || context.Request.Form["Email"] == "") ResponseWriteEnd(context, ErrorMsg("error", "Email必填"));
            if (context.Request.Form["CheckM"] == null || context.Request.Form["CheckM"] == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));

            String ApiCode = context.Request.Form["ApiCode"];
            String Email = context.Request.Form["Email"];
            String ChkM = context.Request.Form["CheckM"]; ;
            //取得來源網址
            String ChkURL = context.Request.UrlReferrer.ToString();

            if (context.Request.Form["ItemData"] != null)
            {
                PostForm.Form postf = JsonConvert.DeserializeObject<PostForm.Form>(context.Request.Form["ItemData"]);

                #region 檢查post值                
                if (postf.OrgName == null || postf.OrgName == "") ResponseWriteEnd(context, ErrorMsg("error","Orgname必填"));
                if (postf.Type == null || postf.Type == "") ResponseWriteEnd(context, ErrorMsg("error","Type必填"));
                if (postf.Condition.ID == null && postf.Condition.Parent == null) ResponseWriteEnd(context, ErrorMsg("error","Condition.ID及Condition.Parent擇一必填"));
                if (postf.Condition.ID == "" && postf.Condition.Parent == "") ResponseWriteEnd(context, ErrorMsg("error","Condition.ID及Condition.Parent擇一必填"));   //ID,parent empty
                if (postf.Range.from == null) ResponseWriteEnd(context, ErrorMsg("error","Range.from必填"));
                if (postf.Range.GetCount == null || postf.Range.GetCount == "") ResponseWriteEnd(context, ErrorMsg("error","Range.GetCount必填"));                
                #endregion
                
                
                String Orgname = postf.OrgName;
                String Type = postf.Type;
                String ConditionID = postf.Condition.ID;
                String ConditionParent = postf.Condition.Parent;
                String ConditionVisible = postf.Condition.Visible;
                String ConditionStocks = postf.Condition.Stocks;
                String RangeFrom = postf.Range.from;
                String RangeGetCount = postf.Range.GetCount;
                List<String> Orderby = postf.OrderBy;
                

                if (!ChkIP(ChkURL, Orgname, ApiCode, Email)) ResponseWriteEnd(context, ErrorMsg("error", "無API權限"));
                

                if (postf.Condition.Visible == "" || postf.Condition.Visible == null) ConditionVisible = "F";
                if (postf.Condition.Stocks == "" || postf.Condition.Stocks == null) ConditionStocks = "F";
                if (postf.OrderBy == null || Orderby.Count == 0) Orderby.Add("ID-Inv");
                
                GetStr GS = new GetStr();

                if (GS.MD5Check(Orgname + Type + ApiCode + Email, ChkM))
                {
                    String Setting = GetSetting(Orgname);
                    String Str_Sql = "";                    

                    Library.Products.RootObject root = new Library.Products.RootObject();
                    List<Library.Products.ProductData> ProdData = new List<Library.Products.ProductData>();
                    List<Library.Products.Stocks> ProdStock = new List<Library.Products.Stocks>();
                    List<Library.Products.MenuCont> ProdMenuCont = new List<Library.Products.MenuCont>();

                    Str_Sql += "WITH CTEResults AS ";
                    Str_Sql += "(";
                    Str_Sql += "     select a.id,a.title,a.value1,a.value2,a.value3,a.item1,a.item2,a.item3,a.item4,a.img1,a.img2,a.img3,a.virtual,b.id as sub_id,b.title as sub_title,c.id as au_id,c.title as au_title,ROW_NUMBER() OVER ";
                    
                    #region 排序
                    Str_Sql += "(ORDER BY ";

                    for (int i = 0; i < Orderby.Count; i++) 
                    {
                        switch (Orderby[i].ToString()) 
                        {
                            case "Serial":
                                Str_Sql += " a.ser_no";
                                break;
                            case "Serial-Inv":
                                Str_Sql += " a.ser_no desc";
                                break;
                            case "Random":
                                Str_Sql += " newid()";
                                break;
                            case "ID":
                                Str_Sql += " a.id";
                                break;
                            case "ID-Inv":
                                Str_Sql += " a.id desc";
                                break;
                            case "Edit":
                                Str_Sql += " a.edate";
                                break;
                            case "Edit-Ine":
                                Str_Sql += " a.edate desc";
                                break;                            
                            default:
                                Str_Sql += " a.id desc";
                                break;
                        }
                        if ((i + 1) < Orderby.Count) {
                            Str_Sql += ",";
                        }
                    }
                    Str_Sql += "    )";
                    #endregion
                    
                    Str_Sql += "    AS RowNum";
	                Str_Sql += "    from prod as a ";
                    Str_Sql += "    left join prod_list as b on a.sub_id=b.id ";
                    Str_Sql += "    left join prod_authors as c on c.id=b.au_id ";
	                Str_Sql += "    where ";

                    #region 條件設定
                    if (ConditionID != "") Str_Sql += " a.id = @id";
                    else
                    {
                        if (ConditionParent != "") Str_Sql += " a.sub_id = @sub_id";
                    }

                    switch (ConditionStocks) { 
                        case "Y":
                            Str_Sql += "    and a.id in (";
	                        Str_Sql += "    select prod_id from prod_Stock  group by prod_id having COUNT(stock)>0";
	                        Str_Sql += "    )";
                            break;
                        case "N":
                            Str_Sql += "    and a.id in (";
	                        Str_Sql += "    select prod_id from prod_Stock  group by prod_id having COUNT(stock)<=0";
	                        Str_Sql += "    )";
                            break;
                        default:
                            break;
                    }

                    switch (ConditionVisible) { 
                        case "Y":
                            Str_Sql += "    and a.disp_opt='Y' and GETDATE() between a.start_date and a.end_date";
                            break;
                        case "N":
                            Str_Sql += "    and a.disp_opt='N' and GETDATE() not between a.start_date and a.end_date";
                            break;
                        default:
                            break;
                    }
                    #endregion
                    
                    Str_Sql += ") ";
                    Str_Sql += " SELECT *,";
                    Str_Sql += " (SELECT MAX(RowNum) FROM CTEResults)  as TotalRows";//  ---### 這裡會回傳總筆數 
                    Str_Sql += " FROM CTEResults ";

                    #region 分頁處理
                    if (RangeGetCount != "max") 
                    {
                        Str_Sql += " WHERE RowNum BETWEEN " + RangeFrom + " AND " + (Convert.ToInt32(RangeFrom) + Convert.ToInt32(RangeGetCount) -1);
                    }
                    #endregion
                    

                    using (SqlConnection conn = new SqlConnection(GetSetting(Orgname)))
                    {
                        conn.Open();
                        SqlCommand cmd;

                        cmd = new SqlCommand(Str_Sql, conn);
                        if (ConditionID != "") cmd.Parameters.Add(new SqlParameter("@id", ConditionID));
                        else
                        {
                            if (ConditionParent != "") cmd.Parameters.Add(new SqlParameter("@sub_id", ConditionParent));
                        }
                        
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    
                                    #region 撈庫存
                                    ProdStock = new List<Library.Products.Stocks>();    //清空庫存List
                                    using (SqlConnection conn2 = new SqlConnection(GetSetting(Orgname)))
                                    {
                                        conn2.Open();
                                        SqlCommand cmd2;
                                        cmd2 = new SqlCommand("select b.title as colorTitle,c.title as sizeTitle,a.stock from prod_Stock as a left join prod_color as b on a.colorID=b.id left join prod_size as c on a.sizeID=c.id where prod_id=@prodid", conn2);
                                        cmd2.Parameters.Add(new SqlParameter("@prodid", reader["id"].ToString()));
                                        SqlDataReader reader2 = cmd2.ExecuteReader();
                                        try
                                        {
                                            if (reader2.HasRows)
                                            {
                                                while (reader2.Read())
                                                {
                                                    Library.Products.Stocks stockitem = new Library.Products.Stocks
                                                    {
                                                        SpecTitle1 = reader2[0].ToString(),
                                                        SpecTitle2 = reader2[1].ToString(),
                                                        Num = reader2[2].ToString()
                                                    };
                                                    ProdStock.Add(stockitem);
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            reader2.Close();
                                        }
                                    }
                                    
                                    #endregion
                                    #region 撈MenuCont
                                    ProdMenuCont = new List<Library.Products.MenuCont>();
                                    using (SqlConnection conn2 = new SqlConnection(GetSetting(Orgname)))
                                    {
                                        conn2.Open();
                                        SqlCommand cmd2;
                                        cmd2 = new SqlCommand("select img,cont,title from menu_cont where type=2 and disp_opt='Y' and menu_id=@prodid order by ser_no", conn2);
                                        cmd2.Parameters.Add(new SqlParameter("@prodid", reader["id"].ToString()));
                                        SqlDataReader reader2 = cmd2.ExecuteReader();
                                        try
                                        {
                                            if (reader2.HasRows)
                                            {
                                                while (reader2.Read())
                                                {
                                                    Library.Products.MenuCont MenuContitem = new Library.Products.MenuCont
                                                    {
                                                        Img = reader2[0].ToString(),                                                        
                                                        Cont = reader2[1].ToString(),
                                                        Title = reader2[2].ToString()
                                                    };
                                                    ProdMenuCont.Add(MenuContitem);
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            reader2.Close();
                                        }
                                    }
                                    #endregion

                                    Library.Products.ProductData prodItem = new Library.Products.ProductData
                                    {
                                        AuID = reader["au_id"].ToString(),
                                        AuTitle = GS.ReplaceStr(reader["au_title"].ToString()),
                                        SubID = reader["sub_id"].ToString(),
                                        SubTitle = GS.ReplaceStr(reader["sub_title"].ToString()),
                                        ID = reader["id"].ToString(),
                                        Title = GS.ReplaceStr(reader["title"].ToString()),
                                        Value1 = reader["value1"].ToString(),
                                        Value2 = reader["value2"].ToString(),
                                        Value3 = reader["value3"].ToString(),
                                        Item1 = GS.ReplaceStr(reader["item1"].ToString()),
                                        Item2 = GS.ReplaceStr(reader["item2"].ToString()),
                                        Item3 = GS.ReplaceStr(reader["item3"].ToString()),
                                        Item4 = GS.ReplaceStr(reader["item4"].ToString()),
                                        Img1 = reader["img1"].ToString(),
                                        Img2 = reader["img2"].ToString(),
                                        Img3 = reader["img3"].ToString(),
                                        Virtual = reader["virtual"].ToString(),
                                        URL = GetProdURL(GetSetting(Orgname), reader["id"].ToString(), reader["sub_id"].ToString()),
                                        Stock = ProdStock,
                                        MenuConts = ProdMenuCont
                                    };
                                    ProdData.Add(prodItem);
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    root.ProductDatas = ProdData;
                    root.RspnCode = "success";
                    root.RspnMsg = "";

                    String returnStr = JsonConvert.SerializeObject(root);
                    ResponseWriteEnd(context, returnStr);
                }
                else {                    
                    ResponseWriteEnd(context, ErrorMsg("error", "檢查碼錯誤"));
                }
            }
            else {
                ResponseWriteEnd(context, ErrorMsg("error", "無參數"));                
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }

        #region 取得Orgname連結字串
        private String GetSetting(String OrgName)
        {
            return "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
        }
        #endregion    
    
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

        #region 取得商品URL
        private String GetProdURL(String setting,String ProdID,String ProdSubID){
            
            String MauID = "";
            String MsubID = "";

            #region 搜尋商品大類ID
            String ProdAuID = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd;

                String Str_Sql = "select au_id from prod_list where id=@id";
                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", ProdSubID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ProdAuID = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion

            if (ProdAuID != "")
            {
                #region 搜尋架構SubID
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("select menu_id from menu_prod where prod_au_id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", ProdAuID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                MsubID = reader[0].ToString();
                            }
                        }
                        else
                        {
                            #region 商品大類沒被架構綁定，搜尋產品模組sub_ID
                            using (SqlConnection conn2 = new SqlConnection(setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("select top 1 id from menu_Sub where use_module='10' and disp_opt='Y' order by ser_no", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@id", ProdAuID));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            MsubID = reader2[0].ToString();
                                        }
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }
                            
                            #endregion
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                #endregion   
                

                #region 搜尋架構AUID
                if (MsubID != "") {
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd;
                        cmd = new SqlCommand();
                        cmd.CommandText = "sp_MenuFindFatherNode";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;                        
                        cmd.Parameters.Add(new SqlParameter("@id", MsubID));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (reader["authors_id"].ToString() == "0")
                                    {
                                        MauID = reader["id"].ToString();
                                    }
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }                
                #endregion

            }
            

            return "au_id=" + MauID + "&sub_id=" + MsubID + "&prod_sub_id=" + ProdSubID + "&prod_id=" + ProdID + "&prodSalesType=prod";
        }
        #endregion
        
        #region 取得來源網址IP
        /// <summary>
        /// 根据主机名（域名）获得主机的IP地址
        /// </summary>
        /// <param name="hostName">主机名或域名</param>
        /// <example> GetIPByDomain("www.google.com");</example>
        /// <returns>主机的IP地址</returns>
        public string GetIpByHostName(string hostName)
        {
            hostName = hostName.Trim();
            if (hostName == string.Empty)
                return string.Empty;
            try
            {
                System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(hostName);
                return host.AddressList.GetValue(0).ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        #region 檢查來源網址IP與OrgName權限
        private bool ChkIP(String URL, String Orgname, String ApiCode, String Email)
        {
            if (URL == "" || Orgname == "" || ApiCode == "" || Email == "")
            {
                return false;
            }
            else 
            {
                URL = URL.Replace("http://","").Replace("https://","").Split(new string[] { "/", ":" }, StringSplitOptions.RemoveEmptyEntries)[0].ToString();

                using (SqlConnection conn = new SqlConnection(GetSetting(Orgname)))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select * from api where Disp_opt='Y' and ApiCode=@ApiCode and Email=@Email and Url=@url", conn);
                    cmd.Parameters.Add(new SqlParameter("@ApiCode", ApiCode));
                    cmd.Parameters.Add(new SqlParameter("@Email", Email));
                    cmd.Parameters.Add(new SqlParameter("@url", URL));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    finally 
                    {
                        reader.Close();
                    }
                }                
            }            
        }
        #endregion

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg)
        {

            Library.Products.RootObject root = new Library.Products.RootObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion        
    }
}