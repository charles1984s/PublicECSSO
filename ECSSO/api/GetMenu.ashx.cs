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
    /// GetMenu 的摘要描述
    /// </summary>
    public class GetMenu : IHttpHandler
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
            InsertLog(Setting, "內容API", "", LogStr);

            String AuID = "";
            String SubID = "";
            String From = "";
            String GetCount = "";
            String ReturnStr = "";
            String WebURL = GS.GetDefaultURL(SiteID);

            switch (Type)
            {
                case "1":   //取得主選單
                    try
                    {
                        if (postf.Range.From != "" && postf.Range.GetCount != "")
                        {
                            From = postf.Range.From;
                            GetCount = postf.Range.GetCount;

                            ReturnStr = GetMenuAu(Setting, From, GetCount);
                            InsertLog(Setting, "取得主選單", "", ReturnStr);

                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得主選單");

                        }
                    }
                    catch
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得主選單");
                    }

                    ResponseWriteEnd(context, ReturnStr);
                    break;

                case "2":   //取得子選單
                    try
                    {
                        if (postf.AuID != "" && postf.Range.From != "" && postf.Range.GetCount != "")
                        {
                            AuID = postf.AuID;
                            From = postf.Range.From;
                            GetCount = postf.Range.GetCount;

                            ReturnStr = GetMenuSub(Setting, AuID, From, GetCount, WebURL);
                            InsertLog(Setting, "取得子選單", "", ReturnStr);
                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得子選單");
                        }
                    }
                    catch
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得子選單");
                    }
                    ResponseWriteEnd(context, ReturnStr);
                    break;

                case "3":   //取得內容列表
                    try
                    {
                        if (postf.SubID != "" && postf.Range.From != "" && postf.Range.GetCount != "")
                        {
                            SubID = postf.SubID;
                            From = postf.Range.From;
                            GetCount = postf.Range.GetCount;

                            ReturnStr = GetMenuList(Setting, SubID, From, GetCount, WebURL);
                            InsertLog(Setting, "取得內容列表", "", ReturnStr);

                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得內容列表");
                        }
                    }
                    catch
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得內容列表");
                    }
                    ResponseWriteEnd(context, ReturnStr);

                    break;

                case "4":   //取得內容資料
                    try
                    {
                        if (postf.ID != "")
                        {
                            String ID = postf.ID;

                            ReturnStr = GetMenuDetail(Setting, ID, WebURL);
                            InsertLog(Setting, "取得內容資料", "", ReturnStr);

                        }
                        else
                        {
                            ReturnStr = ErrorMsg("error", "error:4", Setting, "取得內容資料");
                        }
                    }
                    catch
                    {
                        ReturnStr = ErrorMsg("error", "error:5", Setting, "取得內容資料");
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "呼叫網站內容API"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " /tat/api/getmenu.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting, String Txt)
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

        #region 取得主選單
        private String GetMenuAu(String Setting, String From, String GetCount)
        {
            #region 產生SQL
            String Str_Sql = "";
            Str_Sql += "WITH CTEResults AS ";
            Str_Sql += "(";
            Str_Sql += "     select id,title,ROW_NUMBER() OVER ";
            Str_Sql += "(ORDER BY ser_no)";
            Str_Sql += "    AS RowNum";
            Str_Sql += "    from menu_sub";
            Str_Sql += "    where disp_opt='Y' and authors_id='0'";
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
            InsertLog(Setting, "取得主選單", "", Str_Sql);
            List<Menu.MenuAu> MenuAu = null;
            bool HasMenuSub = false;

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
                        MenuAu = new List<Menu.MenuAu>();

                        while (reader.Read())
                        {
                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("select * from menu_sub where authors_id=@au_id and disp_opt='Y'", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@au_id", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        HasMenuSub = true;
                                    }
                                    else
                                    {
                                        HasMenuSub = false;
                                    }
                                }
                                catch
                                {
                                    return ErrorMsg("error", "error:1", Setting, "取得主選單");
                                }
                                finally { reader2.Close(); }
                            }

                            Menu.MenuAu AuList = new Menu.MenuAu
                            {
                                AuID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                HasMenuSub = HasMenuSub
                            };
                            MenuAu.Add(AuList);

                        }

                    }
                    else
                    {
                        return ErrorMsg("error", "error:2", Setting, "取得主選單");
                    }
                }
                catch
                {
                    return ErrorMsg("error", "error:1", Setting, "取得主選單");
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(MenuAu);
        }
        #endregion


        #region 取得子選單
        private String GetMenuSub(String Setting, String AuID, String From, String GetCount, String WebURL)
        {
            #region 產生SQL
            String Str_Sql = "";
            Str_Sql += "WITH CTEResults AS ";
            Str_Sql += "(";
            Str_Sql += "     select id,title,banner_img,cont,ROW_NUMBER() OVER ";
            Str_Sql += "(ORDER BY ser_no)";
            Str_Sql += "    AS RowNum";
            Str_Sql += "    from menu_sub";
            Str_Sql += " where authors_id=@au_id and disp_opt='Y'";
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
            InsertLog(Setting, "取得子選單", "", Str_Sql);
            List<Menu.MenuSub> MenuSub = null;
            String img1 = "";
            bool HasMenuSub = false;
            bool HasMenu = false;
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
                        MenuSub = new List<Menu.MenuSub>();

                        while (reader.Read())
                        {

                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("select * from menu_sub where authors_id=@au_id and disp_opt='Y'", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@au_id", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        HasMenuSub = true;
                                    }
                                    else
                                    {
                                        HasMenuSub = false;
                                    }
                                }
                                catch
                                {
                                    return ErrorMsg("error", "error:1", Setting, "取得主選單");
                                }
                                finally { reader2.Close(); }
                            }

                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("select * from menu where sub_id=@sub_id and disp_opt='Y'", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@sub_id", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        HasMenu = true;
                                    }
                                    else
                                    {
                                        HasMenu = false;
                                    }
                                }
                                catch
                                {
                                    return ErrorMsg("error", "error:1", Setting, "取得主選單");
                                }
                                finally { reader2.Close(); }
                            }


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

                            Menu.MenuSub SubList = new Menu.MenuSub
                            {
                                SubID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Cont = HttpUtility.HtmlEncode(reader["cont"].ToString()),
                                BannerImg = img1,
                                HasMenu = HasMenu,
                                HasMenuSub = HasMenuSub
                            };
                            MenuSub.Add(SubList);
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
            return JsonConvert.SerializeObject(MenuSub);
        }
        #endregion

        #region 取得內容列表
        private String GetMenuList(String Setting, String SubID, String From, String GetCount, String WebURL)
        {
            String img1 = "";
            #region 產生SQL
            String Str_Sql = "";
            Str_Sql += "WITH CTEResults AS ";
            Str_Sql += "(";
            Str_Sql += "     select id,title,img1,note_date,ROW_NUMBER() OVER ";
            Str_Sql += "(ORDER BY ser_no)";
            Str_Sql += "    AS RowNum";
            Str_Sql += "    from menu";
            Str_Sql += " where sub_id=@sub_id and disp_opt='Y'";
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
            InsertLog(Setting, "取得內容列表", "", Str_Sql);
            List<Menu.MenuList> MenuList = null;

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@sub_id", SubID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        MenuList = new List<Menu.MenuList>();

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

                            Menu.MenuList MList = new Menu.MenuList
                            {
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Img1 = img1,
                                NoteData = reader["note_date"].ToString()
                            };
                            MenuList.Add(MList);
                        }
                    }
                    else
                    {
                        return ErrorMsg("error", "error:2", Setting, "取得內容列表");
                    }
                }
                catch
                {
                    return ErrorMsg("error", "error:1", Setting, "取得內容列表");
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(MenuList);
        }
        #endregion

        #region 取得內容資料
        private String GetMenuDetail(String Setting, String ID, String WebURL)
        {
            Menu.MenuDetail MList = null;
            List<Menu.MenuCont> MCont = new List<Menu.MenuCont>();
            Menu.MenuCont MContList;

            String Str_Sql = "select id,title,note_date,media_link,img1,img2,img3,cont from menu where disp_opt='Y' and id=@id";
            InsertLog(Setting, "取得內容資料", "", Str_Sql);
            String img = "";
            String img1 = "";
            String img2 = "";
            String img3 = "";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", ID));
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
                                cmd2 = new SqlCommand("select img,cont from menu_cont where menu_id=@id and disp_opt='Y' and type='1' order by ser_no", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@id", ID));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            if (reader2["img"].ToString() == "")
                                            {
                                                img = "";
                                            }
                                            else
                                            {
                                                if (reader2["img"].ToString().Contains("http"))
                                                {
                                                    img = reader2["img"].ToString();
                                                }
                                                else
                                                {
                                                    img = "http://" + WebURL + reader2["img"].ToString();
                                                }
                                            }

                                            MContList = new Menu.MenuCont
                                            {
                                                Img = img,
                                                Cont = HttpUtility.HtmlEncode(reader2["cont"].ToString())
                                            };
                                            MCont.Add(MContList);
                                        }
                                    }
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

                            MList = new Menu.MenuDetail
                            {
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Img1 = img1,
                                Img2 = img2,
                                Img3 = img3,
                                NoteDate = reader["note_date"].ToString(),
                                MediaLink = reader["media_link"].ToString(),
                                Cont = HttpUtility.HtmlEncode(reader["cont"].ToString()),
                                MenuCont = MCont
                            };

                            MCont = new List<Menu.MenuCont>();
                        }
                    }
                    else
                    {
                        return ErrorMsg("error", "error:2", Setting, "取得內容資料");
                    }
                }
                //catch
                //{
                //    return ErrorMsg("error", "error:1", Setting, "取得內容資料");
                //}
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(MList);
        }
        #endregion

    }
}