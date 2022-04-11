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
    /// GetTable 的摘要描述
    /// </summary>
    public class GetTable : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            #region 檢查post值
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["VerCode"] == null) ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填"));
            if (context.Request.Form["type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "type必填"));

            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["VerCode"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填"));
            if (context.Request.Form["type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "type必填"));
            #endregion

            String type = context.Request.Form["type"].ToString();
            String ChkM = context.Request.Form["CheckM"].ToString();
            String VerCode = context.Request.Form["VerCode"].ToString();

            GetStr GS = new GetStr();
            if (GS.MD5Check(VerCode + type, ChkM))
            {
                VerCode = "{" + VerCode + "}";
                String Orgname = GetOrgName(VerCode);

                if (Orgname == "") ResponseWriteEnd(context, ErrorMsg("error", "查無Orgname,VerCode=" + VerCode));

                String Setting = GetSetting(Orgname);

                switch (type)
                {
                    case "1":   //取得桌子資訊
                        String returnStr = JsonConvert.SerializeObject(GetAllTable(Setting, VerCode));
                        ResponseWriteEnd(context, returnStr);
                        break;
                    case "2":   //修改桌況
                        if (context.Request.Form["TableID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "TableID必填"));
                        if (context.Request.Form["TableState"] == null) ResponseWriteEnd(context, ErrorMsg("error", "TableState必填"));
                        if (context.Request.Form["TableID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "TableID必填"));
                        if (context.Request.Form["TableState"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "TableState必填"));

                        String TableID = context.Request.Form["TableID"].ToString();
                        String TableStat = context.Request.Form["TableState"].ToString();

                        UpdateTableStat(Setting, TableID, TableStat);
                        ResponseWriteEnd(context, "OK");
                        break;
                }
            }
            else
            {
                ResponseWriteEnd(context, ErrorMsg("error", "檢查碼錯誤"));
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

            Meal.VoidReturn root = new Meal.VoidReturn();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion

        #region 由Vercode取得Orgname
        private String GetOrgName(String VerCode)
        {
            String OrgName = "";
            String Str_Sql = "select orgname from Device where stat='Y' and getdate() between start_date and end_date and VerCode=@VerCode";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@VerCode", VerCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            OrgName = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            return OrgName;
        }
        #endregion

        #region 取得Orgname連結字串
        private String GetSetting(String OrgName)
        {
            return "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
        }
        #endregion

        #region 取得桌況
        private Meal.Tables GetAllTable(String Setting, String VerCode)
        {
            Meal.Tables root = new Meal.Tables();
            Meal.Table TableItem = new Meal.Table();

            List<Meal.Table> MealTable = new List<Meal.Table>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select id,title,stat,SeatingTime,NumberPeople from meal_table where vercode=@vercode and stat<>'9'", conn);
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            TableItem = new Meal.Table
                            {
                                id = reader[0].ToString(),
                                title = reader[1].ToString(),
                                state = reader[2].ToString(),
                                seatingtime = reader[3].ToString(),
                                number = reader[4].ToString()
                            };
                            MealTable.Add(TableItem);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            root.Table = MealTable;
            return root;
        }
        #endregion

        #region 修改桌況
        private bool UpdateTableStat(String setting,String TableID,String State) 
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();                
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_UpdateMealTableStat";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@stat", State));
                cmd.Parameters.Add(new SqlParameter("@id", TableID));
                cmd.ExecuteNonQuery();
            }
            return true;
        }
        #endregion
        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }
    }
}