using System;
using System.Collections.Generic;
using System.Web;
using Meal.Library;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;


namespace Meal.api
{
    /// <summary>
    /// Lookfor 的摘要描述
    /// </summary>
    public class Lookfor : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            #region 檢查post值
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["OrgName"] == null) ResponseWriteEnd(context, ErrorMsg("error", "OrgName必填"));
            if (context.Request.Form["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填"));
            if (context.Request.Form["Items"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Items必填"));
            
            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["OrgName"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "OrgName必填"));
            if (context.Request.Form["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填"));
            if (context.Request.Form["Items"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Items必填"));
            #endregion
            
            String CheckM = context.Request.Form["CheckM"].ToString();
            String OrgName = context.Request.Form["OrgName"].ToString();
            String type = context.Request.Form["Type"].ToString();

            GetMealStr GS = new GetMealStr();
            if (GS.MD5Check(type + OrgName, CheckM))
            {
                String Setting = GS.GetSetting(OrgName);
                Create.SearchItem postf = JsonConvert.DeserializeObject<Create.SearchItem>(context.Request.Form["Items"]);

                switch (type)
                {
                    case "Store":       //分店
                        ResponseWriteEnd(context, FindStore(Setting, postf, OrgName));
                        break;
                    case "StoreTable":  //桌況
                        ResponseWriteEnd(context, FindStoreTable(Setting, postf));
                        break;
                    case "ProdAuthors":       //產品大類
                        ResponseWriteEnd(context, FindProdAuthors(Setting, postf));
                        break;
                    case "ProdSub":       //產品分類..促銷商品
                        ResponseWriteEnd(context, FindProdSub(Setting, postf));
                        break;
                    case "Prod":          //產品
                        ResponseWriteEnd(context, FindProd(Setting, postf));
                        break;
                    case "MealOptions":  //加料加價區
                        ResponseWriteEnd(context, FindMealOptions(Setting, postf));
                        break;
                    case "Meal":        //套餐
                        ResponseWriteEnd(context, GetMeal(Setting));
                        break;
                    case "CancelMeal":  //退餐資料
                        ResponseWriteEnd(context, GetCancelMeal(Setting, postf));
                        break;
                }
            }
            else 
            {
                ResponseWriteEnd(context, "CheckM錯誤");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg)
        {

            Library.Meal.VoidReturn root = new Library.Meal.VoidReturn();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }

        #region 分店管理
        private String FindStore(String Setting, Create.SearchItem SearchItem, String OrgName) 
        {
            List<Create.Store> root = new List<Create.Store>();
            String VerCode = "";
            
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();                
                
                SqlCommand cmd = new SqlCommand(GetSqlStr("bookingStore", SearchItem), conn);
                if (SearchItem.Filter != null && SearchItem.Filter.Count > 0)
                {
                    foreach (Create.Filter FL in SearchItem.Filter) 
                    {
                        if (FL.Logic == "in" || FL.Logic == "not in")
                        {
                            for (int i = 0; i < FL.Value3.Length; i++) 
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + i, FL.Value3[i]));
                            }
                        }
                        else 
                        {
                            cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName, FL.Value));
                            if (FL.Value2 != "" && FL.Value2 != null)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + "2", FL.Value2));
                            }
                        }                        
                    }                    
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            using (SqlConnection conn2 = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB2"].ToString()))
                            {
                                conn2.Open();
                                SqlCommand cmd2 = new SqlCommand("select VerCode from device where Orgname=@orgname and storeid=@storeid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@orgname", OrgName));
                                cmd2.Parameters.Add(new SqlParameter("@storeid", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            VerCode = reader2[0].ToString();
                                        }
                                    }
                                }
                                finally {
                                    reader2.Close();
                                }
                            }


                            Create.Store SList = new Create.Store
                            {
                                ID = reader["id"].ToString(),
                                Display = reader["disp_opt"].ToString(),
                                VerCode = VerCode,
                                Title = reader["title"].ToString(),
                                Cdate = reader["cdate"].ToString(),
                                Edate = reader["edate"].ToString()
                            };
                            root.Add(SList);
                        }
                    }
                    else 
                    {
                        root = null;
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 桌況管理
        private String FindStoreTable(String Setting, Create.SearchItem SearchItem)
        {
            List<Create.StoreTable> root = new List<Create.StoreTable>();
            String CallTime = "0";
            String CancelMeal = "N";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(GetSqlStr("Meal_table", SearchItem), conn);
                if (SearchItem.Filter != null && SearchItem.Filter.Count > 0)
                {
                    foreach (Create.Filter FL in SearchItem.Filter)
                    {
                        if (FL.Logic == "in" || FL.Logic == "not in")
                        {
                            for (int i = 0; i < FL.Value3.Length; i++)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + i, FL.Value3[i]));
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName, FL.Value));
                            if (FL.Value2 != "" && FL.Value2 != null)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + "2", FL.Value2));
                            }
                        }
                    }
                }
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
                                cmd2 = new SqlCommand("select count(*) from [CallService] where calltime>=@calltime and feedback='' and tableid=@id", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@calltime", reader["SeatingTime"].ToString()));
                                cmd2.Parameters.Add(new SqlParameter("@id", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            CallTime = reader2[0].ToString();
                                        }
                                    }
                                }
                                finally { reader2.Close(); }
                            }

                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("select * from [CancelOrder] where tableid=@tableid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@tableid", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        CancelMeal = "Y";
                                    }
                                    else 
                                    {
                                        CancelMeal = "N";
                                    }
                                }
                                finally { reader2.Close(); }
                            }

                            Create.StoreTable SList = new Create.StoreTable
                            {
                                ID = reader["id"].ToString(),
                                StoreID = reader["vercode"].ToString(),
                                Title = reader["title"].ToString(),
                                Num = reader["NumberPeople"].ToString(),
                                Stat = reader["stat"].ToString(),
                                SeatingTime = reader["SeatingTime"].ToString(),
                                Cdate = reader["cdate"].ToString(),
                                Edate = reader["edate"].ToString(),
                                CancelMeal = CancelMeal,
                                Calltime = CallTime
                            };
                            root.Add(SList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 產品大類管理
        private String FindProdAuthors(String Setting, Create.SearchItem SearchItem)
        {
            List<Create.ProdAuthors> root = new List<Create.ProdAuthors>();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(GetSqlStr("prod_authors", SearchItem), conn);
                if (SearchItem.Filter != null && SearchItem.Filter.Count > 0)
                {
                    foreach (Create.Filter FL in SearchItem.Filter)
                    {
                        if (FL.Logic == "in" || FL.Logic == "not in")
                        {
                            for (int i = 0; i < FL.Value3.Length; i++)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + i, FL.Value3[i]));
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName, FL.Value));
                            if (FL.Value2 != "" && FL.Value2 != null)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + "2", FL.Value2));
                            }
                        }
                    }
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Create.ProdAuthors PList = new Create.ProdAuthors
                            {
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                SerNo = reader["ser_no"].ToString(),
                                Display = reader["disp_opt"].ToString()
                            };
                            root.Add(PList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 產品分類管理
        private String FindProdSub(String Setting, Create.SearchItem SearchItem)
        {
            List<Create.ProdSub> root = new List<Create.ProdSub>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(GetSqlStr("prod_list", SearchItem), conn);
                if (SearchItem.Filter != null && SearchItem.Filter.Count > 0)
                {
                    foreach (Create.Filter FL in SearchItem.Filter)
                    {
                        if (FL.Logic == "in" || FL.Logic == "not in")
                        {
                            for (int i = 0; i < FL.Value3.Length; i++)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + i, FL.Value3[i]));
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName, FL.Value));
                            if (FL.Value2 != "" && FL.Value2 != null)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + "2", FL.Value2));
                            }
                        }
                    }
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Create.ProdSub PList = new Create.ProdSub
                            {
                                ID = reader["id"].ToString(),
                                AuID = reader["au_id"].ToString(),
                                Title = reader["title"].ToString(),
                                SerNo = reader["ser_no"].ToString(),
                                Display = reader["disp_opt"].ToString(),
                                BtnImg = reader["banner_img"].ToString(),
                                Cdate = reader["cdate"].ToString(),
                                Edate = reader["edate"].ToString()
                            };
                            root.Add(PList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 產品管理
        private String FindProd(String Setting, Create.SearchItem SearchItem)
        {
            List<Create.Prod> root = new List<Create.Prod>();
            List<Create.Meal_Options_Sub> MOS = new List<Create.Meal_Options_Sub>();
            List<Create.Meal_Options> MO = new List<Create.Meal_Options>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(GetSqlStr("prod", SearchItem), conn);
                if (SearchItem.Filter != null && SearchItem.Filter.Count > 0)
                {
                    foreach (Create.Filter FL in SearchItem.Filter)
                    {
                        if (FL.Logic == "in" || FL.Logic == "not in")
                        {
                            for (int i = 0; i < FL.Value3.Length; i++)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + i, FL.Value3[i]));
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName, FL.Value));
                            if (FL.Value2 != "" && FL.Value2 != null)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + "2", FL.Value2));
                            }
                        }
                    }
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {                        
                        while (reader.Read())
                        {
                            List<Create.Meal_Options_Sub> MealOptionsSub;
                            
                            if (FindMealOptions(Setting, reader["id"].ToString()) == "") 
                            {
                                MealOptionsSub = null;
                            }
                            else 
                            {
                                MealOptionsSub = JsonConvert.DeserializeObject<List<Create.Meal_Options_Sub>>(FindMealOptions(Setting, reader["id"].ToString()));
                            }

                            Create.Prod PList = new Create.Prod
                            {
                                ID = reader["id"].ToString(),
                                SubID = reader["sub_id"].ToString(),
                                Title = reader["title"].ToString(),
                                SerNo = reader["ser_no"].ToString(),
                                Display = reader["disp_opt"].ToString(),
                                StartDate = reader["start_date"].ToString(),
                                EndDate = reader["end_date"].ToString(),
                                Img1 = reader["img1"].ToString(),
                                Item1 = reader["item1"].ToString(),
                                Item2 = reader["item2"].ToString(),
                                Item3 = reader["item3"].ToString(),
                                Item4 = reader["item4"].ToString(),
                                Value1 = reader["value1"].ToString(),
                                Value2 = reader["value2"].ToString(),
                                Value3 = reader["value3"].ToString(),
                                Cdate = reader["cdate"].ToString(),
                                Edate = reader["edate"].ToString(),
                                GS1Code = reader["GS1Code"].ToString(),
                                Options = MealOptionsSub
                            };
                            
                            root.Add(PList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 加料加價區/免費客製區(for產品)
        private String FindMealOptions(String Setting, String ProdID)
        {
            List<Create.Meal_Options_Sub> root = new List<Create.Meal_Options_Sub>();
            List<Create.Meal_Options> MO = new List<Create.Meal_Options>();
            
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select id,title,type from  [Meal_Options_Sub]", conn);
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
                                cmd2 = new SqlCommand("select id,title from  [Meal_Options] where fid=@fid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@fid", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            if (ProdID != "")
                                            {
                                                using (SqlConnection conn3 = new SqlConnection(Setting))
                                                {
                                                    conn3.Open();
                                                    SqlCommand cmd3;
                                                    cmd3 = new SqlCommand("select price from Meal_Detail_Memo where Optionid=@Optionid and pid=@pid", conn3);
                                                    cmd3.Parameters.Add(new SqlParameter("@Optionid", reader2["id"].ToString()));
                                                    cmd3.Parameters.Add(new SqlParameter("@pid", ProdID));
                                                    SqlDataReader reader3 = cmd3.ExecuteReader();
                                                    try
                                                    {
                                                        if (reader3.HasRows)
                                                        {
                                                            while (reader3.Read())
                                                            {
                                                                Create.Meal_Options MOList = new Create.Meal_Options
                                                                {
                                                                    ID = reader2["id"].ToString(),
                                                                    Title = reader2["title"].ToString(),
                                                                    Stat = "Y",
                                                                    Price = reader3[0].ToString()
                                                                };
                                                                MO.Add(MOList);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Create.Meal_Options MOList = new Create.Meal_Options
                                                            {
                                                                ID = reader2["id"].ToString(),
                                                                Title = reader2["title"].ToString(),
                                                                Stat = "N",
                                                                Price = "0"
                                                            };
                                                            MO.Add(MOList);
                                                        }
                                                    }
                                                    finally
                                                    {
                                                        reader3.Close();
                                                    }
                                                }
                                            }
                                            else 
                                            {
                                                Create.Meal_Options MOList = new Create.Meal_Options
                                                {
                                                    ID = reader2["id"].ToString(),
                                                    Title = reader2["title"].ToString(),
                                                    Stat = "N",
                                                    Price = "0"
                                                };
                                                MO.Add(MOList);
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }
                            Create.Meal_Options_Sub MList = new Create.Meal_Options_Sub
                            {
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Type = reader["type"].ToString(),
                                Meal_Options = MO
                            };
                            root.Add(MList);
                            MO = new List<Create.Meal_Options>();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 加料加價區/免費客製區(含搜尋)
        private String FindMealOptions(String Setting, Create.SearchItem SearchItem)
        {
            List<Create.Meal_Options_Sub> root = new List<Create.Meal_Options_Sub>();
            List<Create.Meal_Options> MO = new List<Create.Meal_Options>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(GetSqlStr("Meal_Options_Sub", SearchItem), conn);
                if (SearchItem.Filter != null && SearchItem.Filter.Count > 0)
                {
                    foreach (Create.Filter FL in SearchItem.Filter)
                    {
                        if (FL.Logic == "in" || FL.Logic == "not in")
                        {
                            for (int i = 0; i < FL.Value3.Length; i++)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + i, FL.Value3[i]));
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName, FL.Value));
                            if (FL.Value2 != "" && FL.Value2 != null)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + "2", FL.Value2));
                            }
                        }
                    }
                }
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
                                cmd2 = new SqlCommand("select id,title from  [Meal_Options] where fid=@fid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@fid", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {                                            
                                            Create.Meal_Options MOList = new Create.Meal_Options
                                            {
                                                ID = reader2["id"].ToString(),
                                                Title = reader2["title"].ToString(),
                                                Stat = "N",
                                                Price = "0"
                                            };
                                            MO.Add(MOList);                                            
                                        }
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }
                            Create.Meal_Options_Sub MList = new Create.Meal_Options_Sub
                            {
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                Type = reader["type"].ToString(),
                                Meal_Options = MO
                            };
                            root.Add(MList);
                            MO = new List<Create.Meal_Options>();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 套餐
        private String GetMeal(String Setting) 
        {
            List<Create.Meal> root = new List<Create.Meal>();
            List<Library.Meal.Menu> Menu = new List<Library.Meal.Menu>();
            List<Library.Meal.Item2> Item = new List<Library.Meal.Item2>();
            List<Library.Meal.Other> OtherList = new List<Library.Meal.Other>();
            List<Library.Meal.Memo> MemoList = new List<Library.Meal.Memo>();
            Library.Meal.Memo Memo;
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select * from meal", conn);
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
                                cmd2 = new SqlCommand("select * from meal_Sub where fid=@fid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@fid", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {

                                            using (SqlConnection conn3 = new SqlConnection(Setting))
                                            {
                                                conn3.Open();
                                                SqlCommand cmd3;
                                                cmd3 = new SqlCommand("select prod.title,Meal_Detail.*,prod.GS1Code from Meal_Detail left join prod on Meal_Detail.pid=prod.id where fid=@fid", conn3);
                                                cmd3.Parameters.Add(new SqlParameter("@fid", reader2["id"].ToString()));
                                                SqlDataReader reader3 = cmd3.ExecuteReader();
                                                try
                                                {
                                                    if (reader3.HasRows)
                                                    {
                                                        while (reader3.Read())
                                                        {

                                                            #region 取得可加價項目
                                                            using (SqlConnection conn4 = new SqlConnection(Setting))
                                                            {
                                                                conn4.Open();
                                                                SqlCommand cmd4;

                                                                cmd4 = new SqlCommand("select c.title as title,b.price,b.Optionid,a.GS1Code from prod as a left join Meal_Detail_Memo as b on a.id=b.pid left join Meal_Options as c on b.Optionid=c.id left join Meal_Options_Sub as d on c.fid=d.id where a.id=@pid and d.type='1' order by c.title", conn4);
                                                                cmd4.Parameters.Add(new SqlParameter("@pid", reader3["pid"].ToString()));
                                                                SqlDataReader reader4 = cmd4.ExecuteReader();
                                                                try
                                                                {
                                                                    if (reader4.HasRows)
                                                                    {
                                                                        while (reader4.Read())
                                                                        {
                                                                            Library.Meal.Other Otheritem = new Library.Meal.Other
                                                                            {
                                                                                id = reader4["Optionid"].ToString(),
                                                                                title = reader4["title"].ToString(),
                                                                                price = reader4["price"].ToString(),
                                                                                GS1Code = reader4["GS1Code"].ToString()
                                                                            };
                                                                            OtherList.Add(Otheritem);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        OtherList = null;
                                                                    }
                                                                }
                                                                finally { reader4.Close(); }
                                                            }
                                                            #endregion

                                                            #region 取得非加價項目

                                                            using (SqlConnection conn4 = new SqlConnection(Setting))
                                                            {
                                                                conn4.Open();
                                                                SqlCommand cmd4;

                                                                cmd4 = new SqlCommand("select d.title as title1,c.title as title2,a.GS1Code from prod as a left join Meal_Detail_Memo as b on a.id=b.pid left join Meal_Options as c on b.Optionid=c.id left join Meal_Options_Sub as d on c.fid=d.id where a.id=@pid and d.type='2' order by d.title", conn4);
                                                                cmd4.Parameters.Add(new SqlParameter("@pid", reader3["pid"].ToString()));
                                                                SqlDataReader reader4 = cmd4.ExecuteReader();
                                                                try
                                                                {
                                                                    if (reader4.HasRows)
                                                                    {
                                                                        String MemoTitle = "";
                                                                        List<String> MemoItemList = new List<String>();
                                                                        while (reader4.Read())
                                                                        {
                                                                            if (MemoTitle != reader4["title1"].ToString())
                                                                            {
                                                                                if (MemoTitle != "")
                                                                                {
                                                                                    Memo = new Library.Meal.Memo
                                                                                    {
                                                                                        title = MemoTitle,
                                                                                        item = MemoItemList
                                                                                    };
                                                                                    MemoList.Add(Memo);
                                                                                }

                                                                                MemoTitle = reader4["title1"].ToString();
                                                                                MemoItemList = new List<string>();

                                                                            }
                                                                            MemoItemList.Add(reader4["title2"].ToString());
                                                                        }
                                                                        Memo = new Library.Meal.Memo
                                                                        {
                                                                            title = MemoTitle,
                                                                            item = MemoItemList
                                                                        };
                                                                        MemoList.Add(Memo);
                                                                    }
                                                                    else
                                                                    {
                                                                        MemoList = null;
                                                                    }
                                                                }
                                                                finally { reader4.Close(); }
                                                            }
                                                            #endregion

                                                            Library.Meal.Item2 ItemList = new Library.Meal.Item2
                                                            {
                                                                id = reader3["id"].ToString(),
                                                                title = reader3["title"].ToString(),
                                                                price = reader3["price"].ToString(),
                                                                GS1Code = reader3["GS1Code"].ToString(),
                                                                Memo = MemoList,
                                                                other = OtherList
                                                            };
                                                            Item.Add(ItemList);
                                                            OtherList = new List<Library.Meal.Other>();
                                                            MemoList = new List<Library.Meal.Memo>();
                                                        }
                                                    }
                                                }
                                                finally { reader3.Close(); }
                                            }

                                            Library.Meal.Menu MenuList = new Library.Meal.Menu
                                            {
                                                id = reader2["id"].ToString(),
                                                Title = reader2["title"].ToString(),
                                                Discount = reader2["discount"].ToString(),
                                                SerNo = reader2["ser_no"].ToString(),
                                                ChoiceNum = reader2["ChoiceNum"].ToString(),
                                                GS1Code = reader2["GS1Code"].ToString(),
                                                Item = Item
                                            };
                                            Menu.Add(MenuList);
                                            Item = new List<Library.Meal.Item2>();
                                        }
                                    }
                                }
                                finally { reader2.Close(); }
                            }

                            Create.Meal MList = new Create.Meal
                            {
                                ID = reader["id"].ToString(),
                                Title = reader["title"].ToString(),
                                SerNo = reader["ser_no"].ToString(),
                                Cdate = reader["cdate"].ToString(),
                                Edate = reader["edate"].ToString(),
                                Cont = reader["cont"].ToString(),
                                DispOpt = reader["disp_opt"].ToString(),
                                EndDate = reader["end_Date"].ToString(),
                                Img = reader["img"].ToString(),
                                StartDate = reader["start_date"].ToString(),
                                VerCode = reader["vercode"].ToString(),
                                Value1 = reader["value1"].ToString(),
                                Value2 = reader["value2"].ToString(),
                                Value3 = reader["value3"].ToString(),
                                GS1Code = reader["GS1Code"].ToString(),
                                Menus = Menu
                            };
                            root.Add(MList);
                            Menu = new List<Library.Meal.Menu>();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 退餐資料
        private String GetCancelMeal(String Setting, Create.SearchItem SearchItem)
        {
            List<Create.CancelMeal> root = new List<Create.CancelMeal>();
            String AlreadyCancel = "N";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(GetSqlStr("CancelOrder", SearchItem), conn);
                if (SearchItem.Filter != null && SearchItem.Filter.Count > 0)
                {
                    foreach (Create.Filter FL in SearchItem.Filter)
                    {
                        if (FL.Logic == "in" || FL.Logic == "not in")
                        {
                            for (int i = 0; i < FL.Value3.Length; i++)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + i, FL.Value3[i]));
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName, FL.Value));
                            if (FL.Value2 != "" && FL.Value2 != null)
                            {
                                cmd.Parameters.Add(new SqlParameter("@" + FL.ColumnName + "2", FL.Value2));
                            }
                        }
                    }
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["cancel_date"].ToString() != "")
                            {
                                AlreadyCancel = "Y";
                            }
                            else {
                                AlreadyCancel = "N";
                            }

                            Create.CancelMeal MList = new Create.CancelMeal
                            {
                                OrderNo = reader["order_no"].ToString(),
                                SerNo = reader["ser_no"].ToString(),
                                ProdName = reader["prod_name"].ToString(),
                                Discription = reader["discription"].ToString(),
                                Amt = reader["amt"].ToString(),
                                AlreadyCancel = AlreadyCancel,
                                MealReady = reader["meal_ready"].ToString()
                            };
                            root.Add(MList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 產生SQL字串
        private String GetSqlStr(String TableName,Create.SearchItem SearchItem) 
        {
            String Str_Sql = "";

            Str_Sql += "WITH CTEResults AS ";
            Str_Sql += "(";
            Str_Sql += "     select *,ROW_NUMBER() OVER ";

            #region 排序
            if (SearchItem.OrderBy != null && SearchItem.OrderBy.Count > 0)
            {
                Str_Sql += "(ORDER BY ";
                int i = 1;
                foreach (Create.OrderBy OB in SearchItem.OrderBy)
                {
                    Str_Sql += OB.ColumnName + " " + OB.Mode;
                    if (i < SearchItem.OrderBy.Count) Str_Sql += ",";
                    i = i + 1;
                }
                Str_Sql += "    )";
            }
            #endregion

            Str_Sql += "    AS RowNum";
            Str_Sql += "    from " + TableName;
            

            #region 條件設定
            if (SearchItem.Filter != null && SearchItem.Filter.Count > 0)
            {
                Str_Sql += " where 1=1 ";
                foreach (Create.Filter FL in SearchItem.Filter)
                {
                    if (FL.Logic == "in" || FL.Logic == "not in")
                    {
                        Str_Sql += " and " + FL.ColumnName + " " + FL.Logic + " (";
                        for (int i = 0; i < FL.Value3.Length; i++) 
                        {
                            if (i < FL.Value3.Length-1)
                            {
                                Str_Sql += " @" + FL.ColumnName + i + ",";
                            }
                            else {
                                Str_Sql += " @" + FL.ColumnName + i;
                            }                            
                        }
                        Str_Sql += " ) ";
                    }
                    else 
                    {
                        if (FL.Value2 != null && FL.Value2 != "")
                        {
                            Str_Sql += " and " + FL.ColumnName + " " + FL.Logic + " @" + FL.ColumnName + " and @" + FL.ColumnName + "2 ";
                        }
                        else
                        {
                            Str_Sql += " and " + FL.ColumnName + " " + FL.Logic + " @" + FL.ColumnName;
                        }
                    }
                }
            }
            
            #endregion

            Str_Sql += ") ";
            Str_Sql += " SELECT *,";
            Str_Sql += " (SELECT MAX(RowNum) FROM CTEResults)  as TotalRows";//  ---### 這裡會回傳總筆數 
            Str_Sql += " FROM CTEResults ";

            #region 分頁處理
            if (SearchItem.Range.GetCount != "max")
            {
                Str_Sql += " WHERE RowNum BETWEEN " + SearchItem.Range.From + " AND " + (Convert.ToInt32(SearchItem.Range.From) + Convert.ToInt32(SearchItem.Range.GetCount) - 1);
            }
            #endregion
            return Str_Sql;
        }
        #endregion
    }
}