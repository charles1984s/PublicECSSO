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
    /// DeleteData 的摘要描述
    /// </summary>
    public class DeleteData : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            #region 檢查post值
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["VerCode"] == null) ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填"));
            if (context.Request.Form["type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "type必填"));
            if (context.Request.Form["Items"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Items必填"));

            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["VerCode"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填"));
            if (context.Request.Form["type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "type必填"));
            if (context.Request.Form["Items"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Items必填"));
            #endregion

            String CheckM = context.Request.Form["CheckM"].ToString();
            String VerCode = context.Request.Form["VerCode"].ToString();
            String type = context.Request.Form["type"].ToString();

            GetMealStr GS = new GetMealStr();
            if (GS.MD5Check(type + VerCode, CheckM))
            {
                String Orgname = GS.GetOrgName("{" + VerCode + "}");
                if (Orgname == "") ResponseWriteEnd(context, ErrorMsg("error", "查無Orgname"));
                String Setting = GS.GetSetting(Orgname);

                switch (type)
                {
                    case "Store":       //分店
                        Create.Store Store = JsonConvert.DeserializeObject<Create.Store>(context.Request.Params["Items"]);
                        DelStore(Setting, Store.DelID);

                        ResponseWriteEnd(context, "success");
                        break;
                    case "StoreTable":  //桌況
                        Create.StoreTable StoreTable = JsonConvert.DeserializeObject<Create.StoreTable>(context.Request.Params["Items"]);
                        DelStoreTable(Setting, StoreTable.DelID);

                        ResponseWriteEnd(context, "success");
                        break;
                    case "ProdAuthors":       //產品大類
                        Create.ProdAuthors ProdAuthors = JsonConvert.DeserializeObject<Create.ProdAuthors>(context.Request.Params["Items"]);
                        DelProdAuthors(Setting, ProdAuthors.DelID);

                        ResponseWriteEnd(context, "success");
                        break;
                    case "ProdSub":       //產品分類..促銷商品
                        Create.ProdSub ProdSub = JsonConvert.DeserializeObject<Create.ProdSub>(context.Request.Params["Items"]);
                        DelProdSub(Setting, ProdSub.DelID);

                        ResponseWriteEnd(context, "success");
                        break;
                    case "Prod":          //產品
                        Create.Prod Prod = JsonConvert.DeserializeObject<Create.Prod>(context.Request.Params["Items"]);
                        DelProd(Setting, Prod.DelID);

                        ResponseWriteEnd(context, "success");
                        break;
                    case "ProdOptionsSub":  //加料加價區

                        break;
                    case "Meal":        //套餐

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
        private void DelStore(String Setting,String[] ID)
        {
            String LogStr = "";
            GetMealStr GS = new GetMealStr();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                for (int i = 0; i < ID.Length; i++)
                {
                    cmd = new SqlCommand("delete from bookingStore where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", ID[i].ToString()));
                    cmd.ExecuteNonQuery();

                    LogStr = "delete from bookingStore where id='" + ID[i].ToString() + "')";
                    GS.SaveLog(Setting, "admin", "分店管理", "刪除", "", ID[i].ToString(), LogStr, "/Meal/api/DeleteData.ashx");
                }
            }
        }
        #endregion

        #region 桌況管理
        private void DelStoreTable(String Setting, String[] ID)
        {
            String LogStr = "";
            GetMealStr GS = new GetMealStr();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                for (int i = 0; i < ID.Length; i++)
                {
                    cmd = new SqlCommand("delete from [Meal_table] where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", ID[i].ToString()));
                    cmd.ExecuteNonQuery();

                    LogStr = "delete from bookingStore where id='" + ID[i].ToString() + "')";
                    GS.SaveLog(Setting, "admin", "桌況管理", "刪除", "", ID[i].ToString(), LogStr, "/Meal/api/DeleteData.ashx");
                }
            }
        }
        #endregion

        #region 產品大類管理
        private void DelProdAuthors(String Setting, String[] ID)
        {
            String LogStr = "";
            GetMealStr GS = new GetMealStr();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                for (int i = 0; i < ID.Length; i++)
                {
                    cmd = new SqlCommand("delete from [prod_authors] where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", ID[i].ToString()));
                    cmd.ExecuteNonQuery();

                    LogStr = "delete from ProdAuthors where id='" + ID[i].ToString() + "')";
                    GS.SaveLog(Setting, "admin", "產品大類管理", "刪除", "", ID[i].ToString(), LogStr, "/Meal/api/DeleteData.ashx");
                }
            }
        }
        #endregion

        #region 產品分類管理
        private void DelProdSub(String Setting, String[] ID)
        {
            String LogStr = "";
            GetMealStr GS = new GetMealStr();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                for (int i = 0; i < ID.Length; i++)
                {
                    cmd = new SqlCommand("delete from [Prod_list] where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", ID[i].ToString()));
                    cmd.ExecuteNonQuery();

                    LogStr = "delete from Prod_list where id='" + ID[i].ToString() + "')";
                    GS.SaveLog(Setting, "admin", "產品分類管理", "刪除", "", ID[i].ToString(), LogStr, "/Meal/api/DeleteData.ashx");
                }
            }
        }
        #endregion

        #region 產品管理
        private void DelProd(String Setting, String[] ID)
        {
            String LogStr = "";
            GetMealStr GS = new GetMealStr();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                for (int i = 0; i < ID.Length; i++)
                {
                    cmd = new SqlCommand("delete from [Prod] where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", ID[i].ToString()));
                    cmd.ExecuteNonQuery();

                    LogStr = "delete from Prod where id='" + ID[i].ToString() + "')";
                    GS.SaveLog(Setting, "admin", "產品管理", "刪除", "", ID[i].ToString(), LogStr, "/Meal/api/DeleteData.ashx");
                }
            }
        }
        #endregion
    }
}