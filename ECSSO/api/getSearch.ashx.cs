using System;
using System.Collections.Generic;
using System.Web;
using ECSSO.Library;
using ECSSO;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ECSSO.api
{
    /// <summary>
    /// getSearch 的摘要描述
    /// </summary>
    public class getSearch : IHttpHandler
    {
        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            GetStr GS = new GetStr();

            int statement = 0;
            string returnMsg = "Something has wrong!!", tableName = "Menu";
            try
            {
                if (context.Request.Params["Type"] == null || context.Request.Params["Type"].ToString() == "") statement = 1;
                if (context.Request.Params["Items"] == null || context.Request.Params["Items"].ToString() == "") statement = 2;
                if (context.Request.Params["CheckSum"] == null || context.Request.Params["CheckSum"].ToString() == "") statement = 3;
                if (context.Request.Params["Token"] == null || context.Request.Params["Token"].ToString() == "") statement = 4;
                if (context.Request.Params["Lng"] == null || context.Request.Params["Lng"].ToString() == "") statement = 5;

                switch (statement)
                {
                    case 0:
                        {
                            String ChkM = context.Request.Params["CheckSum"].ToString();
                            String Type = context.Request.Params["Type"].ToString();
                            String Token = context.Request.Params["Token"].ToString();
                            String Lng = GS.CheckStringIsNotNull(context.Request.Params["Lng"].ToString());
                            Search items = JsonConvert.DeserializeObject<Search>(context.Request.Params["Items"]);
                            string Items = context.Request.Params["Items"].ToString();
                            string strSqlConnection = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();

                            if (GS.MD5Check(Type + Items, ChkM))
                            {
                                CommandSetting cmdSetting = new CommandSetting();
                                DataListSetting dataList = new DataListSetting();
                                dataList.Data = new List<object>();

                                int minValue = 0, maxValue = 0;
                                if (items.Range != null)
                                {
                                    if (items.Range.From != null || items.Range.From != "")
                                    {
                                        minValue = (Int32.TryParse(items.Range.From, out minValue) ? Int32.Parse(items.Range.From) : 1) - 1;
                                    }
                                    if (items.Range.GetCount != null || items.Range.GetCount != "")
                                    {
                                        maxValue = (items.Range.GetCount.ToLower() == "max") ? 10 : (Int32.TryParse(items.Range.GetCount, out maxValue) ? Int32.Parse(items.Range.GetCount) : 0);
                                    }
                                    minValue = (minValue == -1) ? 0 : minValue;
                                    if (maxValue < 0 || minValue < 0)
                                    {
                                        returnMsg = ErrorMsg("error", "From或是GetCount不得為負數", "");
                                        break;
                                    }
                                }

                                switch (cmdSetting.isVerityState(GS.GetIPAddress(), Token, strSqlConnection))
                                {
                                    case 0:
                                        if (Type == "Search")
                                        {
                                            string selectString = "", tagType = "";
                                            selectString = @"Select SearchData.*, searchRelation.ClassID Type, '/index.asp?au_id=' + convert(nvarchar, menu_sub.authors_id) + '&sub_id=' + convert(nvarchar, menu_sub.id) + '&id=' + convert(nvarchar, menu.id) DetailURL, menu.Popular From ( 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from shop tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                  union all 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from hotel tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                  union all 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from attractions tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                  union all 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from active tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                ) as SearchData left join (menu inner join menu_sub on menu.sub_id = menu_sub.id inner join searchRelation on menu_sub.id = searchRelation.bindID) on SearchData.menuID = menu.id
                                                                Where menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                                            if (items.ClassID != "0")
                                            {
                                                selectString += "and searchRelation.ClassID = @ClassID ";
                                            }
                                            using (SqlConnection conn = new SqlConnection(cmdSetting._strSqlConnection))
                                            {
                                                try
                                                {
                                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                                    {
                                                        if (conn.State == ConnectionState.Closed) conn.Open();
                                                        cmd.Parameters.AddWithValue("@Keyword", "%" + items.Keyword + "%");
                                                        if (items.ClassID != "0")
                                                        {
                                                            cmd.Parameters.AddWithValue("@ClassID", items.ClassID);
                                                        }

                                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                                        {
                                                            if (reader.HasRows)
                                                            {
                                                                while (reader.Read())
                                                                {
                                                                    string picurl = (reader["Picture1"] is DBNull) ? "" : HttpContext.Current.Server.UrlEncode(GS.GetAllLinkString(cmdSetting._orgName, reader["Picture1"].ToString(), Lng, "Image"));
                                                                    string detailurl = (reader["DetailURL"] is DBNull) ? "" : HttpContext.Current.Server.UrlEncode(GS.GetAllLinkString(cmdSetting._orgName, reader["DetailURL"].ToString(), Lng, ""));
                                                                    SearchItem item = new SearchItem
                                                                    {
                                                                        ID = (reader["ID"] is DBNull) ? "" : reader["ID"].ToString(),
                                                                        Title = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                                        Brief = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                                        Name_ch = (reader["Name_ch"] is DBNull) ? "" : reader["Name_ch"].ToString(),
                                                                        PicURL = picurl,
                                                                        DetailURL = detailurl,
                                                                        Px = (reader["Px"] is DBNull) ? 0F : Convert.ToSingle(reader["Px"].ToString()),
                                                                        Py = (reader["Py"] is DBNull) ? 0F : Convert.ToSingle(reader["Py"].ToString()),
                                                                        Type = (reader["Type"] is DBNull) ? "" : reader["Type"].ToString(),
                                                                        Duration = (reader["Duration"] is DBNull) ? 0 : Convert.ToInt32(reader["Duration"].ToString()),
                                                                        Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString())
                                                                    };
                                                                    dataList.Data.Add(item);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    dataList.Data = dataList.Data.OrderByDescending(o => ((SearchItem)o).ID).Take(minValue + maxValue).Skip(minValue).ToList();
                                                    foreach (SearchItem item in dataList.Data)
                                                    {
                                                        if (item.Type == "1")
                                                        {
                                                            tagType = "active";
                                                        }
                                                        else if (item.Type == "2" || item.Type == "5")
                                                        {
                                                            tagType = "shop";
                                                        }
                                                        else if (item.Type == "3")
                                                        {
                                                            tagType = "hotel";
                                                        }
                                                        else if (item.Type == "4")
                                                        {
                                                            tagType = "attra";
                                                        }
                                                        else
                                                        {
                                                            tagType = "";
                                                        }
                                                        item.Tag = new List<Tags>();
                                                        item.Tag = new CommandSetting().getTags(conn, item.ID, tagType);
                                                    }
                                                    returnMsg = JsonConvert.SerializeObject(dataList);
                                                }
                                                catch (Exception ex)
                                                {
                                                    returnMsg = ErrorMsg("error", ex.ToString(), "");
                                                    //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                                }
                                            }
                                        }
                                        else if (Type == "Search2")
                                        {
                                            newDataListSetting newDataList = new newDataListSetting();
                                            newDataList.Data = new List<object>();

                                            string selectString = "", tagType = "";
                                            selectString = @"Select SearchData.*, searchRelation.ClassID Type, '/index.asp?au_id=' + convert(nvarchar, menu_sub.authors_id) + '&sub_id=' + convert(nvarchar, menu_sub.id) + '&id=' + convert(nvarchar, menu.id) DetailURL, menu.Popular From ( 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from shop tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                  union all 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from hotel tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                  union all 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from attractions tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                  union all 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from active tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                ) as SearchData left join (menu inner join menu_sub on menu.sub_id = menu_sub.id inner join searchRelation on menu_sub.id = searchRelation.bindID) on SearchData.menuID = menu.id
                                                                Where menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                                            if (items.ClassID != "0")
                                            {
                                                selectString += "and searchRelation.ClassID = @ClassID ";
                                            }
                                            using (SqlConnection conn = new SqlConnection(cmdSetting._strSqlConnection))
                                            {
                                                try
                                                {
                                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                                    {
                                                        if (conn.State == ConnectionState.Closed) conn.Open();
                                                        cmd.Parameters.AddWithValue("@Keyword", "%" + items.Keyword + "%");
                                                        if (items.ClassID != "0")
                                                        {
                                                            cmd.Parameters.AddWithValue("@ClassID", items.ClassID);
                                                        }
                                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                                        {
                                                            if (reader.HasRows)
                                                            {
                                                                while (reader.Read())
                                                                {
                                                                    string picurl = (reader["Picture1"] is DBNull) ? "" : HttpContext.Current.Server.UrlEncode(GS.GetAllLinkString(cmdSetting._orgName, reader["Picture1"].ToString(), Lng, "Image"));
                                                                    string detailurl = (reader["DetailURL"] is DBNull) ? "" : HttpContext.Current.Server.UrlEncode(GS.GetAllLinkString(cmdSetting._orgName, reader["DetailURL"].ToString(), Lng, ""));
                                                                    SearchItem item = new SearchItem
                                                                    {
                                                                        ID = (reader["ID"] is DBNull) ? "" : reader["ID"].ToString(),
                                                                        Title = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                                        Brief = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                                        Name_ch = (reader["Name_ch"] is DBNull) ? "" : reader["Name_ch"].ToString(),
                                                                        PicURL = picurl,
                                                                        DetailURL = detailurl,
                                                                        Px = (reader["Px"] is DBNull) ? 0F : Convert.ToSingle(reader["Px"].ToString()),
                                                                        Py = (reader["Py"] is DBNull) ? 0F : Convert.ToSingle(reader["Py"].ToString()),
                                                                        Type = (reader["Type"] is DBNull) ? "" : reader["Type"].ToString(),
                                                                        Duration = (reader["Duration"] is DBNull) ? 0 : Convert.ToInt32(reader["Duration"].ToString()),
                                                                        Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString())
                                                                    };
                                                                    newDataList.Data.Add(item);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    newDataList.Total = newDataList.Data.Count;
                                                    newDataList.Data = newDataList.Data.OrderByDescending(o => ((SearchItem)o).ID).Take(minValue + maxValue).Skip(minValue).ToList();
                                                    foreach (SearchItem item in newDataList.Data)
                                                    {
                                                        if (item.Type == "1")
                                                        {
                                                            tagType = "active";
                                                        }
                                                        else if (item.Type == "2" || item.Type == "5")
                                                        {
                                                            tagType = "shop";
                                                        }
                                                        else if (item.Type == "3")
                                                        {
                                                            tagType = "hotel";
                                                        }
                                                        else if (item.Type == "4")
                                                        {
                                                            tagType = "attra";
                                                        }
                                                        else
                                                        {
                                                            tagType = "";
                                                        }
                                                        item.Tag = new List<Tags>();
                                                        item.Tag = new CommandSetting().getTags(conn, item.ID, tagType);
                                                    }
                                                    returnMsg = JsonConvert.SerializeObject(newDataList);
                                                }
                                                catch (Exception ex)
                                                {
                                                    returnMsg = ErrorMsg("error", ex.ToString(), "");
                                                    //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                                }
                                            }
                                        }
                                        else if (Type == "getNodesByTagID")
                                        {
                                            newDataListSetting newDataList = new newDataListSetting();
                                            newDataList.Data = new List<object>();
                                            if (items.TagID == null || items.TagID == "")
                                            {
                                                returnMsg = ErrorMsg("error", "TagID不得為空", "");
                                                break;
                                            }
                                            string selectString = "", tagType = "";
                                            selectString = @"Select SearchData.*, Menu.Ser_No, searchRelation.ClassID Type, '/index.asp?au_id=' + convert(nvarchar, menu_sub.authors_id) + '&sub_id=' + convert(nvarchar, menu_sub.id) + '&id=' + convert(nvarchar, menu.id) DetailURL, menu.Popular From ( 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from shop tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                  union all 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from hotel tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                  union all 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from attractions tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                  union all 
                                                                 SELECT tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1 
                                                                 from active tName where(tName.Name like @Keyword or tName.Keyword like @Keyword) 
                                                                ) as SearchData left join (menu inner join menu_sub on menu.sub_id = menu_sub.id inner join searchRelation on menu_sub.id = searchRelation.bindID left join prod_tag on prod_tag.prod_id = menu.id and prod_tag.[type]='cont' left join tag on tag.id = prod_tag.tag_id) on SearchData.menuID = menu.id 
                                                                Where menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date and tag.id = @TagID ";
                                            selectString += " and searchRelation.ClassID = 4 ";

                                            using (SqlConnection conn = new SqlConnection(cmdSetting._strSqlConnection))
                                            {
                                                try
                                                {
                                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                                    {
                                                        if (conn.State == ConnectionState.Closed) conn.Open();
                                                        cmd.Parameters.AddWithValue("@Keyword", "%" + items.Keyword + "%");
                                                        cmd.Parameters.AddWithValue("@TagID", items.TagID);
                                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                                        {
                                                            if (reader.HasRows)
                                                            {
                                                                while (reader.Read())
                                                                {
                                                                    string picurl = (reader["Picture1"] is DBNull) ? "" : HttpContext.Current.Server.UrlEncode(GS.GetAllLinkString(cmdSetting._orgName, reader["Picture1"].ToString(), Lng, "Image"));
                                                                    string detailurl = (reader["DetailURL"] is DBNull) ? "" : HttpContext.Current.Server.UrlEncode(GS.GetAllLinkString(cmdSetting._orgName, reader["DetailURL"].ToString(), Lng, ""));
                                                                    SearchItemByTagId item = new SearchItemByTagId
                                                                    {
                                                                        ID = (reader["ID"] is DBNull) ? "-1" : reader["ID"].ToString(),
                                                                        Title = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                                        Brief = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                                        Name_ch = (reader["Name_ch"] is DBNull) ? "" : reader["Name_ch"].ToString(),
                                                                        PicURL = picurl,
                                                                        DetailURL = detailurl,
                                                                        Px = (reader["Px"] is DBNull) ? 0F : Convert.ToSingle(reader["Px"].ToString()),
                                                                        Py = (reader["Py"] is DBNull) ? 0F : Convert.ToSingle(reader["Py"].ToString()),
                                                                        Type = (reader["Type"] is DBNull) ? "" : reader["Type"].ToString(),
                                                                        Duration = (reader["Duration"] is DBNull) ? 0 : Convert.ToInt32(reader["Duration"].ToString()),
                                                                        Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString()),
                                                                        Ser_No = (reader["ser_no"] is DBNull) ? "-1" : reader["ser_no"].ToString()
                                                                    };
                                                                    newDataList.Data.Add(item);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    newDataList.Total = newDataList.Data.Count;
                                                    newDataList.Data = newDataList.Data.OrderBy(o => ((SearchItemByTagId)o).Ser_No).ThenByDescending(o => ((SearchItemByTagId)o).ID).Take(minValue + maxValue).Skip(minValue).ToList();
                                                    foreach (SearchItemByTagId item in newDataList.Data)
                                                    {
                                                        if (item.Type == "1")
                                                        {
                                                            tagType = "active";
                                                        }
                                                        else if (item.Type == "2" || item.Type == "5")
                                                        {
                                                            tagType = "shop";
                                                        }
                                                        else if (item.Type == "3")
                                                        {
                                                            tagType = "hotel";
                                                        }
                                                        else if (item.Type == "4")
                                                        {
                                                            tagType = "attra";
                                                        }
                                                        else
                                                        {
                                                            tagType = "";
                                                        }
                                                        item.Tag = new List<Tags>();
                                                        item.Tag = new CommandSetting().getTags(conn, item.ID, tagType);
                                                    }
                                                    returnMsg = JsonConvert.SerializeObject(newDataList);
                                                }
                                                catch (Exception ex)
                                                {
                                                    returnMsg = ErrorMsg("error", ex.ToString(), "");
                                                    //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                                }
                                            }
                                        }
                                        else if (Type == "SearchAll")
                                        {

                                        }
                                        else
                                        {
                                            returnMsg = ErrorMsg("error", "Type不存在", "");
                                        }
                                        break;
                                    case 1:
                                        returnMsg = ErrorMsg("error", "Token不存在", "");
                                        break;
                                    case 2:
                                        returnMsg = ErrorMsg("error", "Token權限出問題", "");
                                        break;
                                }
                            }
                            else
                            {
                                returnMsg = ErrorMsg("error", "CheckSum驗證失敗", "");
                            }
                            break;
                        }
                    case 1:
                        {
                            returnMsg = ErrorMsg("error", "Type必填", "");
                            break;
                        }
                    case 2:
                        {
                            returnMsg = ErrorMsg("error", "Items必填", "");
                            break;
                        }
                    case 3:
                        {
                            returnMsg = ErrorMsg("error", "CheckSum必填", "");
                            break;
                        }
                    case 4:
                        {
                            returnMsg = ErrorMsg("error", "Token必填", "");
                            break;
                        }
                    case 5:
                        {
                            returnMsg = ErrorMsg("error", "Lng必填", "");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                returnMsg = ErrorMsg("error", ex.ToString(), "");
            }

            //context.Response.ContentType = "text/html";
            context.Response.Write(returnMsg);
            context.Response.End();
        }
        public class newDataListSetting : DataListSetting
        {
            public int Total { get; set; }
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "getSearch error", "", RspnMsg);
            }

            ContextErrorMessager root = new ContextErrorMessager();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "搜尋"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " getSearch.ashx"));

                cmd.ExecuteNonQuery();
            }
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
    }
}