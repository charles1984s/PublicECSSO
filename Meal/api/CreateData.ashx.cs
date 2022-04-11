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
    /// CreateData 的摘要描述
    /// </summary>
    public class CreateData : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            #region 檢查post值
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["OrgName"] == null) ResponseWriteEnd(context, ErrorMsg("error", "OrgName必填"));
            if (context.Request.Form["type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "type必填"));
            if (context.Request.Form["Items"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Items必填"));

            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["OrgName"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "OrgName必填"));
            if (context.Request.Form["type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "type必填"));
            if (context.Request.Form["Items"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Items必填"));
            #endregion

            String CheckM = context.Request.Form["CheckM"].ToString();
            String OrgName = context.Request.Form["OrgName"].ToString();
            String type = context.Request.Form["type"].ToString();

            GetMealStr GS = new GetMealStr();
            
            
            if (GS.MD5Check(type + OrgName, CheckM))
            {
                String ID = "0";
                String Setting = GS.GetSetting(OrgName);

                switch (type)
                {
                    case "Store":       //分店
                        Create.Store Store = JsonConvert.DeserializeObject<Create.Store>(context.Request.Params["Items"]);
                        ID = InsertStore(Setting, Store);
                        if (ID != "0")
                        {
                            ResponseWriteEnd(context, ID);
                        }
                        else 
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "新增失敗"));
                        }                        
                        break;
                    case "StoreTable":  //桌況
                        Create.StoreTable StoreTable = JsonConvert.DeserializeObject<Create.StoreTable>(context.Request.Params["Items"]);
                        ID = InsertStoreTable(Setting, StoreTable);
                        if (ID != "0") 
                        {
                            ResponseWriteEnd(context, ID);
                        }
                        else
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "新增失敗"));
                        }                        
                        break;
                    case "ProdAuthors":       //產品大類
                        Create.ProdAuthors ProdAuthors = JsonConvert.DeserializeObject<Create.ProdAuthors>(context.Request.Params["Items"]);
                        ID = InsertProdAuthors(Setting, ProdAuthors);
                        if (ID != "0")
                        {
                            ResponseWriteEnd(context, ID);
                        }
                        else
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "新增失敗"));
                        }    
                        break;
                    case "ProdSub":       //產品分類..促銷商品
                        Create.ProdSub ProdSub = JsonConvert.DeserializeObject<Create.ProdSub>(context.Request.Params["Items"]);
                        ID = InsertProdSub(Setting, ProdSub, "");
                        if (ID != "0")
                        {
                            ResponseWriteEnd(context, ID);
                        }
                        else
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "新增失敗"));
                        }                            
                        break;
                    case "Prod":          //產品
                        Create.Prod Prod = JsonConvert.DeserializeObject<Create.Prod>(context.Request.Params["Items"]);
                        try 
                        {
                            ID = InsertProd(Setting, Prod);
                        }
                        catch (Exception ex)
                        {
                            ResponseWriteEnd(context, ex.Message.ToString()); //上傳失敗
                        }
                        
                        if (ID != "0")
                        {
                            ResponseWriteEnd(context, ID);
                        }
                        else
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "新增失敗"));
                        }       
                        break;
                    case "MealOptions":  //加料加價區
                        List<Create.Meal_Options_Sub> Meal_Options_Sub = JsonConvert.DeserializeObject<List<Create.Meal_Options_Sub>>(context.Request.Params["Items"]);
                        InserMealOptions(Setting, Meal_Options_Sub);
                        ResponseWriteEnd(context, "success");
                        break;
                    case "Meal":        //套餐
                        Create.ProdSub Meal = JsonConvert.DeserializeObject<Create.ProdSub>(context.Request.Params["Items"]);
                        ID = InsertProdSub(Setting, Meal, "meal");
                        if (ID != "0")
                        {
                            ResponseWriteEnd(context, ID);
                        }
                        else
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "新增失敗"));
                        }         
                        break;
                    case "CallService": //呼叫服務生
                        Create.StoreTable CallService = JsonConvert.DeserializeObject<Create.StoreTable>(context.Request.Form["Items"]);
                        InsertCall(Setting, CallService.ID);
                        ResponseWriteEnd(context, "success");
                        break;
                    case "Printer":     //印表機
                        
                        //GS.SaveLog(Setting, "admin", "印表機管理", "新增", "", "", context.Request.Form["Items"], "/Meal/api/CreateData.ashx");
                        //ResponseWriteEnd(context, "");
                        List<Create.Printer> Printer = JsonConvert.DeserializeObject<List<Create.Printer>>(context.Request.Form["Items"]);
                        InsertPrinter(Setting, Printer);
                        ResponseWriteEnd(context, "success");
                        break;
                    case "TableLog":    //開桌
                        Create.TableLog TableLog = JsonConvert.DeserializeObject<Create.TableLog>(context.Request.Params["Items"]);
                        ID = InsertTableLog(Setting, TableLog);
                        ResponseWriteEnd(context, "success");                   
                        break;
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

        #region 印表機管理
        private void InsertPrinter(String Setting, List<Create.Printer> Printer)
        {
            
            String LogStr = "";
            GetMealStr GS = new GetMealStr();
            foreach (Create.Printer Print in Printer) 
            {
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;
                    cmd = new SqlCommand("select * from [printer] where [PrinterName]=@PrinterName", conn);                    
                    cmd.Parameters.Add(new SqlParameter("@PrinterName", Print.PrinterName));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (!reader.HasRows)
                        {
                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();                                
                                SqlCommand cmd2 = new SqlCommand("INSERT INTO [printer] ([title],[PrinterName]) VALUES (@title,@PrinterName);select IDENT_CURRENT('printer')", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@title", Print.PrinterName));
                                cmd2.Parameters.Add(new SqlParameter("@PrinterName", Print.PrinterName));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            LogStr = "INSERT INTO [printer] ([title],[PrinterName]) VALUES ('" + Print.PrinterName + "','" + Print.PrinterName + "')";
                                            GS.SaveLog(Setting, "admin", "印表機管理", "新增", Print.PrinterName, reader2[0].ToString(), LogStr, "/Meal/api/CreateData.ashx");
                                        }
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
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
        }
        #endregion

        #region 分店管理
        private String InsertStore(String Setting, Create.Store Store)
        {
            String ID = "0";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("INSERT INTO [bookingStore] ([title],[disp_opt]) VALUES (@title,@dispopt);select IDENT_CURRENT('bookingStore')", conn);
                cmd.Parameters.Add(new SqlParameter("@title", Store.Title));
                cmd.Parameters.Add(new SqlParameter("@dispopt", Store.Display));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ID = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            String LogStr = "INSERT INTO [bookingStore] ([title],[disp_opt]) VALUES ('" + Store.Title + "','" + Store.Display + "')";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "分店管理", "新增", Store.Title, ID, LogStr, "/Meal/api/CreateData.ashx");

            return ID;
        }
        #endregion

        #region 桌況管理
        private String InsertStoreTable(String Setting, Create.StoreTable StoreTable)
        {
            String ID = "0";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("INSERT INTO [Meal_table] ([title],[vercode],[stat],[NumberPeople]) VALUES (@title,@vercode,@stat,@NumberPeople);select IDENT_CURRENT('Meal_table')", conn);
                cmd.Parameters.Add(new SqlParameter("@title", StoreTable.Title));
                cmd.Parameters.Add(new SqlParameter("@vercode", StoreTable.StoreID));
                cmd.Parameters.Add(new SqlParameter("@stat", StoreTable.Stat));
                cmd.Parameters.Add(new SqlParameter("@NumberPeople", Convert.ToInt32(StoreTable.Num))); 
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ID = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            String LogStr = "INSERT INTO [Meal_table] ([title],[vercode],[stat],[NumberPeople]) VALUES ('" + StoreTable.Title + "','" + StoreTable.StoreID + "','" + StoreTable.Stat + "','" + StoreTable.Num + "')";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "桌況管理", "新增", StoreTable.Title, ID, LogStr, "/Meal/api/CreateData.ashx");

            return ID;
        }
        #endregion

        #region 開桌
        private String InsertTableLog(String Setting, Create.TableLog TableLog)
        {
            String ID = "0";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("INSERT INTO [TableLog] ([Datetime],[VerCode],[TableID],[Stat],[f_num],[m_num],[amt],[country],[startTime],[endTime]) VALUES (getdate(),@VerCode,@TableID,@Stat,@f_num,@m_num,0,@country,@startTime,'');select IDENT_CURRENT('TableLog')", conn);
                cmd.Parameters.Add(new SqlParameter("@VerCode", TableLog.VerCode));
                cmd.Parameters.Add(new SqlParameter("@TableID", TableLog.TableID));
                cmd.Parameters.Add(new SqlParameter("@Stat", TableLog.Stat));
                cmd.Parameters.Add(new SqlParameter("@f_num", Convert.ToInt32(TableLog.Fnum)));
                cmd.Parameters.Add(new SqlParameter("@m_num", Convert.ToInt32(TableLog.Mnum)));
                cmd.Parameters.Add(new SqlParameter("@country", TableLog.country));
                cmd.Parameters.Add(new SqlParameter("@startTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ID = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            String LogStr = "INSERT INTO [TableLog] ([Datetime],[VerCode],[TableID],[Stat],[f_num],[m_num],[amt],[country],[startTime],[endTime]) VALUES (getdate(),'" + TableLog.VerCode + "','" + TableLog.TableID + "','" + TableLog.Stat + "','" + TableLog.Fnum + "','" + TableLog.Mnum + "','" + TableLog.country + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "開桌", "新增", TableLog.VerCode + "/" + TableLog.TableID, ID, LogStr, "/Meal/api/CreateData.ashx");

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("update [Meal_table]  set stat=@stat,SeatingTime=@SeatingTime,TableLogID=@TableLogID where id=@id and vercode=@vercode", conn);
                cmd.Parameters.Add(new SqlParameter("@stat", TableLog.Stat));
                cmd.Parameters.Add(new SqlParameter("@SeatingTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                cmd.Parameters.Add(new SqlParameter("@TableLogID", ID));
                cmd.Parameters.Add(new SqlParameter("@id", TableLog.TableID));
                cmd.Parameters.Add(new SqlParameter("@vercode", TableLog.VerCode));
                cmd.ExecuteNonQuery();
            }

            LogStr = "update [Meal_table]  set stat='" + TableLog.Stat + "',SeatingTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',TableLogID='" + ID + "' where id='" + TableLog.TableID + "' and vercode='" + TableLog.VerCode + "'";
            GS.SaveLog(Setting, "admin", "開桌", "新增", TableLog.VerCode + "/" + TableLog.TableID, ID, LogStr, "/Meal/api/CreateData.ashx");

            return ID;
        }
        #endregion

        #region 產品大類管理
        private String InsertProdAuthors(String Setting, Create.ProdAuthors ProdAuthors)
        {
            String ID = "0";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("INSERT INTO [prod_authors] ([title],[disp_opt],[ser_no],[type]) VALUES (@title,@disp_opt,@ser_no,'1');select IDENT_CURRENT('prod_authors')", conn);
                cmd.Parameters.Add(new SqlParameter("@title", ProdAuthors.Title));
                cmd.Parameters.Add(new SqlParameter("@disp_opt", ProdAuthors.Display));
                cmd.Parameters.Add(new SqlParameter("@ser_no", ProdAuthors.SerNo));
                
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ID = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            String LogStr = "INSERT INTO [prod_authors] [title],[disp_opt],[ser_no],[type]) VALUES ('" + ProdAuthors.Title + "','" + ProdAuthors.Display + "','" + ProdAuthors.SerNo + "','1')";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "產品大類管理", "新增", ProdAuthors.Title, ID, LogStr, "/Meal/api/CreateData.ashx");

            return ID;
        }
        #endregion

        #region 產品分類管理
        private String InsertProdSub(String Setting, Create.ProdSub ProdSub,String type)
        {
            String AuID = "";
            String ID = "0";
            
            if (type == "meal") 
            {
                AuID = "9999999";
            }
            else
            {
                AuID = ProdSub.AuID;
            }
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("INSERT INTO [prod_list] ([au_id],[title],[disp_opt],[ser_no],[banner_img],[type]) VALUES (@au_id,@title,@disp_opt,@ser_no,@banner_img,@type);select IDENT_CURRENT('prod_list')", conn);
                cmd.Parameters.Add(new SqlParameter("@au_id", AuID));
                cmd.Parameters.Add(new SqlParameter("@title", ProdSub.Title));
                cmd.Parameters.Add(new SqlParameter("@disp_opt", ProdSub.Display));
                cmd.Parameters.Add(new SqlParameter("@ser_no", ProdSub.SerNo));
                cmd.Parameters.Add(new SqlParameter("@banner_img", ProdSub.BtnImg));
                cmd.Parameters.Add(new SqlParameter("@type", type));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ID = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            String LogStr = "INSERT INTO [prod_list] ([au_id],[title],[disp_opt],[ser_no],[banner_img],[type]) VALUES ('" + AuID + "','" + ProdSub.Title + "','" + ProdSub.Display + "','" + ProdSub.SerNo + "','" + ProdSub.BtnImg + "','" + type + "')";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "產品分類管理", "新增", ProdSub.Title, ID, LogStr, "/Meal/api/CreateData.ashx");

            return ID;
        }
        #endregion

        #region 產品管理
        private String InsertProd(String Setting, Create.Prod Prod)
        {
            String ID = "0";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("INSERT INTO [prod] ([sub_id],[title],[disp_opt],[ser_no],[value1],[value2],[value3],[item1],[item2],[item3],[item4],[img1],[start_date],[end_date]) VALUES (@sub_id,@title,@disp_opt,@ser_no,@value1,@value2,@value3,@item1,@item2,@item3,@item4,@img1,@start_date,@end_date);select IDENT_CURRENT('prod')", conn);
                cmd.Parameters.Add(new SqlParameter("@sub_id", Prod.SubID));
                cmd.Parameters.Add(new SqlParameter("@title", Prod.Title));
                cmd.Parameters.Add(new SqlParameter("@disp_opt", Prod.Display));
                cmd.Parameters.Add(new SqlParameter("@ser_no", Prod.SerNo));
                cmd.Parameters.Add(new SqlParameter("@value1", Prod.Value1));
                cmd.Parameters.Add(new SqlParameter("@value2", Prod.Value2));
                cmd.Parameters.Add(new SqlParameter("@value3", Prod.Value3));
                cmd.Parameters.Add(new SqlParameter("@item1", Prod.Item1));
                cmd.Parameters.Add(new SqlParameter("@item2", Prod.Item2));
                cmd.Parameters.Add(new SqlParameter("@item3", Prod.Item3));
                cmd.Parameters.Add(new SqlParameter("@item4", Prod.Item4));
                cmd.Parameters.Add(new SqlParameter("@img1", Prod.Img1));
                cmd.Parameters.Add(new SqlParameter("@start_date", Prod.StartDate));
                cmd.Parameters.Add(new SqlParameter("@end_date", Prod.EndDate));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ID = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            String LogStr = "INSERT INTO [prod] ([sub_id],[title],[disp_opt],[ser_no],[value1],[value2],[value3],[item1],[item2],[item3],[item4],[img1],[start_date],[end_date]) VALUES ('" + Prod.SubID + "','" + Prod.Title + "','" + Prod.Display + "','" + Prod.SerNo + "','" + Prod.Value1 + "','" + Prod.Value2 + "','" + Prod.Value3 + "','" + Prod.Item1 + "','" + Prod.Item2 + "','" + Prod.Item3 + "','" + Prod.Item4 + "','" + Prod.Img1 + "','" + Prod.StartDate + "','" + Prod.EndDate + "')";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "產品管理", "新增", Prod.Title, ID, LogStr, "/Meal/api/CreateData.ashx");

            return ID;
        }
        #endregion

        #region 加料區管理
        private void InserMealOptions(String Setting, List<Create.Meal_Options_Sub> MealOptionsSub) 
        {
            String Fid = "";
            
            foreach (Create.Meal_Options_Sub MOS in MealOptionsSub)
            {
                #region 加料區表頭
                //檢查是否有資料
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;
                    cmd = new SqlCommand("select id from [Meal_Options_Sub] where title=@title", conn);
                    cmd.Parameters.Add(new SqlParameter("@title", MOS.Title));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Fid = reader[0].ToString();
                            }
                        }
                        else
                        {
                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;
                                cmd2 = new SqlCommand("INSERT INTO [Meal_Options_Sub] ([title],[type]) VALUES (@title,@type);select IDENT_CURRENT('Meal_Options_Sub')", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@title", MOS.Title));
                                cmd2.Parameters.Add(new SqlParameter("@type", MOS.Type));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            Fid = reader2[0].ToString();
                                        }
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                #endregion

                if (Fid != "") 
                {
                    #region 加料區表身
                    foreach (Create.Meal_Options MO in MOS.Meal_Options)
                    {
                        //檢查是否有資料
                        using (SqlConnection conn = new SqlConnection(Setting))
                        {
                            conn.Open();
                            SqlCommand cmd;
                            cmd = new SqlCommand("select id from [Meal_Options] where title=@title and fid=@fid", conn);
                            cmd.Parameters.Add(new SqlParameter("@title", MO.Title));
                            cmd.Parameters.Add(new SqlParameter("@fid", Fid));
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                if (!reader.HasRows)
                                {
                                    using (SqlConnection conn2 = new SqlConnection(Setting))
                                    {
                                        conn2.Open();
                                        SqlCommand cmd2;
                                        cmd2 = new SqlCommand("INSERT INTO [Meal_Options] ([fid],[title]) VALUES (@fid,@title)", conn2);
                                        cmd2.Parameters.Add(new SqlParameter("@fid", Fid));
                                        cmd2.Parameters.Add(new SqlParameter("@title", MO.Title));
                                        cmd2.ExecuteNonQuery();
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
                
            }
        }
        #endregion        

        #region 呼叫服務生
        private void InsertCall(String Setting, String TableID)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("INSERT INTO [CallService] ([tableid],[calltime]) VALUES (@table_id,@calltime)", conn);
                cmd.Parameters.Add(new SqlParameter("@table_id", TableID));
                cmd.Parameters.Add(new SqlParameter("@calltime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                cmd.ExecuteNonQuery();
            }

            String LogStr = "INSERT INTO [CallService] ([tableid],[calltime]) VALUES ('" + TableID + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')'";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "呼叫服務生", "新增", "", TableID, LogStr, "/Meal/api/CreateData.ashx");
        }

        #endregion
    }
}