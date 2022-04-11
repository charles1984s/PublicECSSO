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
    /// UpdateData 的摘要描述
    /// </summary>
    public class UpdateData : IHttpHandler
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
                String Setting = GS.GetSetting(OrgName);

                switch (type)
                {
                    case "Store":       //分店
                        Create.Store Store = JsonConvert.DeserializeObject<Create.Store>(context.Request.Params["Items"]);
                        UpdateStore(Setting, Store);

                        ResponseWriteEnd(context, "success");
                        break;
                    case "StoreTable":  //桌況
                        Create.StoreTable StoreTable = JsonConvert.DeserializeObject<Create.StoreTable>(context.Request.Params["Items"]);
                        UpdateStoreTable(Setting, StoreTable);

                        ResponseWriteEnd(context, "success");
                        break;
                    case "ProdAuthors":       //產品大類
                        Create.ProdAuthors ProdAuthors = JsonConvert.DeserializeObject<Create.ProdAuthors>(context.Request.Params["Items"]);
                        UpdateProdAuthors(Setting, ProdAuthors);

                        ResponseWriteEnd(context, "success");
                        break;
                    case "ProdSub":       //產品分類..促銷商品
                        Create.ProdSub ProdSub = JsonConvert.DeserializeObject<Create.ProdSub>(context.Request.Params["Items"]);
                        UpdateProdSub(Setting, ProdSub);

                        ResponseWriteEnd(context, "success");
                        break;
                    case "Prod":          //產品
                        Create.Prod Prod = JsonConvert.DeserializeObject<Create.Prod>(context.Request.Params["Items"]);
                        UpdateProd(Setting, Prod);

                        ResponseWriteEnd(context, "success");
                        break;
                    case "MealOptions":  //加料加價區
                        List<Create.Meal_Options_Sub> MealOptionsSub = JsonConvert.DeserializeObject<List<Create.Meal_Options_Sub>>(context.Request.Params["Items"]);
                        UpdateMealOptions(Setting, MealOptionsSub);
                        ResponseWriteEnd(context, "success");
                        break;
                    case "Meal":        //套餐

                        break;
                    case "CallService": //呼叫服務生
                        Create.StoreTable CallService = JsonConvert.DeserializeObject<Create.StoreTable>(context.Request.Form["Items"]);
                        UpdateCall(Setting, CallService.ID);
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
        #region 分店管理
        private void UpdateStore(String Setting,Create.Store Store)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("update bookingStore set title=@title,disp_opt=@disp_opt,edate=getdate() where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@title", Store.Title));
                cmd.Parameters.Add(new SqlParameter("@disp_opt", Store.Display));
                cmd.Parameters.Add(new SqlParameter("@id", Store.ID));
                cmd.ExecuteNonQuery();
            }

            String LogStr = "update bookingStore set title='" + Store.Title + "',disp_opt='" + Store.Display + "',cdate=getdate() where id='" + Store.ID + "')";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "分店管理", "修改", Store.Title, Store.ID, LogStr, "/Meal/api/UpdateData.ashx");
        }
        #endregion

        #region 桌況管理
        private void UpdateStoreTable(String Setting,Create.StoreTable StoreTable)
        {
            String TableLogID = "";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                if (StoreTable.SeatingTime != "")
                {
                    cmd = new SqlCommand("update [Meal_table] set title=@title,vercode=@vercode,stat=@stat,SeatingTime=@SeatingTime,NumberPeople=@NumberPeople,edate=getdate() where id=@id", conn);
                }
                else 
                {
                    cmd = new SqlCommand("update [Meal_table] set title=@title,vercode=@vercode,stat=@stat,NumberPeople=@NumberPeople,edate=getdate() where id=@id", conn);
                }
                
                cmd.Parameters.Add(new SqlParameter("@title", StoreTable.Title));
                cmd.Parameters.Add(new SqlParameter("@vercode", StoreTable.StoreID));
                cmd.Parameters.Add(new SqlParameter("@stat", StoreTable.Stat));
                if (StoreTable.SeatingTime != "")
                {
                    cmd.Parameters.Add(new SqlParameter("@SeatingTime", StoreTable.SeatingTime));
                }
                
                cmd.Parameters.Add(new SqlParameter("@NumberPeople", StoreTable.Num));
                cmd.Parameters.Add(new SqlParameter("@id", StoreTable.ID));
                cmd.ExecuteNonQuery();
            }

            String LogStr = "update [Meal_table] set title='" + StoreTable.Title + "',vercode='" + StoreTable.StoreID + "',stat='" + StoreTable.Stat + "',SeatingTime='" + StoreTable.SeatingTime + "',NumberPeople='" + StoreTable.Num + "',edate=getdate() where id='" + StoreTable.ID + "'";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "桌況管理", "修改", StoreTable.Title, StoreTable.ID, LogStr, "/Meal/api/UpdateData.ashx");

            #region 取得目前桌子的TableLog ID
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("select TableLogID from Meal_table where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", StoreTable.ID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            TableLogID = reader[0].ToString();
                        }
                    }
                    else
                    {
                        TableLogID = "";
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion
            
            #region 修改Tablelog狀態
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("update [TableLog] set stat=@stat,edate=getdate() where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@stat", StoreTable.Stat));
                cmd.Parameters.Add(new SqlParameter("@id", TableLogID));
                cmd.ExecuteNonQuery();
            }

            LogStr = "update [TableLog] set stat='" + StoreTable.Stat + "',edate=getdate() where id='" + TableLogID + "'";
            GS.SaveLog(Setting, "admin", "桌況管理", "修改", StoreTable.Title, StoreTable.ID, LogStr, "/Meal/api/UpdateData.ashx");
            #endregion

            #region 修改TableLog價格
            if (StoreTable.amt != "" && TableLogID != "")
            {
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;
                    cmd = new SqlCommand("update [TableLog] set amt=@amt,edate=getdate() where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@amt", StoreTable.amt));
                    cmd.Parameters.Add(new SqlParameter("@id", TableLogID));
                    cmd.ExecuteNonQuery();
                }

                LogStr = "update [TableLog] set amt='" + StoreTable.amt + "',edate=getdate() where id='" + TableLogID + "'";
                GS.SaveLog(Setting, "admin", "桌況管理", "修改", StoreTable.Title, StoreTable.ID, LogStr, "/Meal/api/UpdateData.ashx");
            }
            #endregion

            #region 紀錄離桌時間
            if (StoreTable.SeatingTime == "" && TableLogID != "")
            {
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;
                    cmd = new SqlCommand("update [TableLog] set endTime=@endTime,edate=getdate() where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@endTime", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));
                    cmd.Parameters.Add(new SqlParameter("@id", TableLogID));
                    cmd.ExecuteNonQuery();
                }

                LogStr = "update [TableLog] set endTime='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',edate=getdate() where id='" + TableLogID + "'";
                GS.SaveLog(Setting, "admin", "桌況管理", "修改", StoreTable.Title, StoreTable.ID, LogStr, "/Meal/api/UpdateData.ashx");
            }
            #endregion
        }
        #endregion

        #region 產品大類管理
        private void UpdateProdAuthors(String Setting, Create.ProdAuthors ProdAuthors)
        {            
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("update [prod_authors] set title=@title,disp_opt=@disp_opt,ser_no=@ser_no,edate=getdate() where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@title", ProdAuthors.Title));
                cmd.Parameters.Add(new SqlParameter("@disp_opt", ProdAuthors.Display));
                cmd.Parameters.Add(new SqlParameter("@ser_no", ProdAuthors.SerNo));
                cmd.Parameters.Add(new SqlParameter("@id", ProdAuthors.ID));
                cmd.ExecuteNonQuery();
            }

            String LogStr = "update [prod_authors] set title='" + ProdAuthors.Title + "',disp_opt='" + ProdAuthors.Display + "',ser_no='" + ProdAuthors.SerNo + "',edate=getdate() where id='" + ProdAuthors.ID + "'";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "產品大類管理", "修改", ProdAuthors.Title, ProdAuthors.ID, LogStr, "/Meal/api/UpdateData.ashx");
        }
        #endregion

        #region 產品分類管理
        private void UpdateProdSub(String Setting, Create.ProdSub ProdSub)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("update [prod_list] set au_id=@au_id,title=@title,disp_opt=@disp_opt,ser_no=@ser_no,banner_img=@banner_img,edate=getdate() where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@au_id", ProdSub.AuID));
                cmd.Parameters.Add(new SqlParameter("@title", ProdSub.Title));
                cmd.Parameters.Add(new SqlParameter("@disp_opt", ProdSub.Display));
                cmd.Parameters.Add(new SqlParameter("@ser_no", ProdSub.SerNo));
                cmd.Parameters.Add(new SqlParameter("@banner_img", ProdSub.BtnImg));
                cmd.Parameters.Add(new SqlParameter("@id", ProdSub.ID));
                cmd.ExecuteNonQuery();
            }

            String LogStr = "update [prod_list] set au_id='" + ProdSub.AuID + "',title='" + ProdSub.Title + "',disp_opt='" + ProdSub.Display + "',ser_no='" + ProdSub.SerNo + "',banner_img='" + ProdSub.BtnImg + "',edate=getdate() where id='" + ProdSub.ID + "'";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "產品分類管理", "修改", ProdSub.Title, ProdSub.ID, LogStr, "/Meal/api/UpdateData.ashx");
        }
        #endregion

        #region 產品管理
        private void UpdateProd(String Setting, Create.Prod Prod)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("update [prod] set sub_id=@sub_id,title=@title,disp_opt=@disp_opt,ser_no=@ser_no,start_Date=@start_Date,end_Date=@end_Date,img1=@img1,item1=@item1,value1=@value1,value2=@value2,value3=@value3,edate=getdate(),PrinterID=@PrinterID where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@sub_id", Prod.SubID));
                cmd.Parameters.Add(new SqlParameter("@title", Prod.Title));
                cmd.Parameters.Add(new SqlParameter("@disp_opt", Prod.Display));
                cmd.Parameters.Add(new SqlParameter("@ser_no", Prod.SerNo));
                cmd.Parameters.Add(new SqlParameter("@start_Date", Prod.StartDate));
                cmd.Parameters.Add(new SqlParameter("@end_Date", Prod.EndDate));
                cmd.Parameters.Add(new SqlParameter("@img1", Prod.Img1));
                cmd.Parameters.Add(new SqlParameter("@item1", Prod.Item1));
                cmd.Parameters.Add(new SqlParameter("@value1", Prod.Value1));
                cmd.Parameters.Add(new SqlParameter("@value2", Prod.Value2));
                cmd.Parameters.Add(new SqlParameter("@value3", Prod.Value3));
                cmd.Parameters.Add(new SqlParameter("@PrinterID", Prod.PrinterID));
                cmd.Parameters.Add(new SqlParameter("@id", Prod.ID));
                cmd.ExecuteNonQuery();
            }

            String LogStr = "update [prod] set sub_id='" + Prod.SubID + "',title='" + Prod.Title + "',disp_opt='" + Prod.Display + "',ser_no='" + Prod.SerNo + "',start_Date='" + Prod.StartDate + "',end_Date='" + Prod.EndDate + "',img1='" + Prod.Img1 + "',value1='" + Prod.Value1 + "',value2='" + Prod.Value2 + "',value3='" + Prod.Value3 + "',item1='" + Prod.Item1 + "',edate=getdate(),PrinterID='" + Prod.PrinterID + "' where id='" + Prod.ID + "'";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "產品管理", "修改", Prod.Title, Prod.ID, LogStr, "/Meal/api/UpdateData.ashx");
        }
        #endregion

        #region 加料區管理
        private void UpdateMealOptions(String Setting, List<Create.Meal_Options_Sub> MealOptionsSub)
        {
            foreach (Create.Meal_Options_Sub MOS in MealOptionsSub)
            {
                #region 加料區表頭
                //檢查是否有資料
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;
                    cmd = new SqlCommand("update [Meal_Options_Sub] set title=@title,type=@type where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@title", MOS.Title));
                    cmd.Parameters.Add(new SqlParameter("@type", MOS.Type));
                    cmd.Parameters.Add(new SqlParameter("@id", MOS.ID));
                    cmd.ExecuteNonQuery();
                }
                #endregion

                #region 加料區表身
                foreach (Create.Meal_Options MO in MOS.Meal_Options)
                {
                    //檢查是否有資料
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd;
                        cmd = new SqlCommand("update [Meal_Options] set title=@title where id=@id", conn);
                        cmd.Parameters.Add(new SqlParameter("@title", MO.Title));
                        cmd.Parameters.Add(new SqlParameter("@id", MO.ID));
                        cmd.ExecuteNonQuery();
                    }
                }
                #endregion
            }
        }
        #endregion        

        #region 套餐管理
        private void UpdateMeal(String Setting, Create.Meal Meal)
        {
            #region 修改表頭
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("update [Meal] set title=@title,disp_opt=@disp_opt,ser_no=@ser_no,value1=@value1,value2=@value2,value3=@value3,img1=@img1,item1=@item1,start_date=@start_date,end_date=@end_date,vercode=@vercode where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@title", Meal.Title));
                cmd.Parameters.Add(new SqlParameter("@disp_opt", Meal.DispOpt));
                cmd.Parameters.Add(new SqlParameter("@ser_no", Meal.SerNo));
                cmd.Parameters.Add(new SqlParameter("@value1", Meal.Value1));
                cmd.Parameters.Add(new SqlParameter("@value2", Meal.Value2));
                cmd.Parameters.Add(new SqlParameter("@value3", Meal.Value3));
                cmd.Parameters.Add(new SqlParameter("@img1", Meal.Img));
                cmd.Parameters.Add(new SqlParameter("@item1", Meal.Cont));
                cmd.Parameters.Add(new SqlParameter("@start_date", Meal.StartDate));
                cmd.Parameters.Add(new SqlParameter("@end_date", Meal.EndDate));
                cmd.Parameters.Add(new SqlParameter("@vercode", Meal.VerCode));
                cmd.Parameters.Add(new SqlParameter("@id", Meal.ID));
                cmd.ExecuteNonQuery();
            }
            #endregion
            #region 修改表身
            String MenuID = "";
            foreach (Library.Meal.Menu Menu in Meal.Menus)
            {                
                if (Menu.id != null && Menu.id != "")
                {
                    MenuID = Menu.id;
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd;
                        cmd = new SqlCommand("update [Meal_Sub] set title=@title,ChoiceNum=@ChoiceNum,ser_no=@ser_no,Discount=@Discount where id=@id", conn);
                        cmd.Parameters.Add(new SqlParameter("@title", Menu.Title));
                        cmd.Parameters.Add(new SqlParameter("@ChoiceNum", Menu.ChoiceNum));
                        cmd.Parameters.Add(new SqlParameter("@ser_no", Menu.SerNo));
                        cmd.Parameters.Add(new SqlParameter("@Discount", Menu.Discount));
                        cmd.Parameters.Add(new SqlParameter("@id", MenuID));
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd;
                        cmd = new SqlCommand("insert into [Meal_Sub] (fid,title,ChoiceNum,ser_no,Discount) value (@fid,@title,@ChoiceNum,@ser_no,@Discount);select IDENT_CURRENT('prod_list')", conn);
                        cmd.Parameters.Add(new SqlParameter("@fid", Meal.ID));
                        cmd.Parameters.Add(new SqlParameter("@title", Menu.Title));
                        cmd.Parameters.Add(new SqlParameter("@ChoiceNum", Menu.ChoiceNum));
                        cmd.Parameters.Add(new SqlParameter("@ser_no", Menu.SerNo));
                        cmd.Parameters.Add(new SqlParameter("@Discount", Menu.Discount));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    MenuID = reader[0].ToString();
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }

                #region 關聯產品
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;
                    cmd = new SqlCommand("delete from [Meal_Detail] where fid=@fid", conn);
                    cmd.Parameters.Add(new SqlParameter("@fid", MenuID));
                    cmd.ExecuteNonQuery();
                }
                foreach (Library.Meal.Item2 item in Menu.Item)
                {
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd;
                        cmd = new SqlCommand("insert into [Meal_Detail] (fid,pid,ser_no,price) value (@fid,@pid,@ser_no,@price);", conn);
                        cmd.Parameters.Add(new SqlParameter("@fid", MenuID));
                        cmd.Parameters.Add(new SqlParameter("@pid", item.id));
                        cmd.Parameters.Add(new SqlParameter("@price", item.price));
                        cmd.ExecuteNonQuery();
                    }
                }
                #endregion
            }
            #endregion
        }
        #endregion   

        #region 呼叫服務生
        private void UpdateCall(String Setting, String tableid)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("update [CallService] set feedback=@feedback where [tableid]=@id and feedback=''", conn);
                cmd.Parameters.Add(new SqlParameter("@feedback", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                cmd.Parameters.Add(new SqlParameter("@id", tableid));
                cmd.ExecuteNonQuery();
            }
            String LogStr = "update [CallService] set DateTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where id='" + tableid + "' and feedback=''";
            GetMealStr GS = new GetMealStr();
            GS.SaveLog(Setting, "admin", "呼叫服務生", "修改", "", tableid, LogStr, "/Meal/api/UpdateData.ashx");
        }
        #endregion
    }
}