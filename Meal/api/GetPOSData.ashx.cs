using System;
using System.Collections.Generic;
using System.Web;
using Meal.Library;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace Meal.api
{
    /// <summary>
    /// GetPOSData 的摘要描述
    /// </summary>
    public class GetPOSData : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Form["VerCode"] == null) ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填", ""));

            if (context.Request.Form["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Form["VerCode"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填", ""));

            String ChkM = context.Request.Form["CheckM"].ToString();
            String VerCode = context.Request.Form["VerCode"].ToString();
            String Type = context.Request.Form["Type"].ToString();

            GetMealStr GS = new GetMealStr();
            if (GS.MD5Check(Type + VerCode, ChkM))
            {
                String Orgname = GS.GetOrgName("{" + VerCode + "}");

                if (Orgname == "") ResponseWriteEnd(context, ErrorMsg("error", "查無Orgname", ""));

                String Setting = GS.GetSetting(Orgname);

                //if (context.Request.Form["Items"] == null || context.Request.Form["Items"] == "") ResponseWriteEnd(context, ErrorMsg("error", "Items必填", Setting));
                //Bill.Items bill = null;
                //try
                //{
                //    bill = JsonConvert.DeserializeObject<Bill.Items>(context.Request.Form["Items"]);
                //}
                //catch
                //{
                //    ResponseWriteEnd(context, ErrorMsg("error", "Json格式不正確", Setting));
                //}


                switch (Type)
                {
                    case "Printer":     //印表機列表
                        ResponseWriteEnd(context, GetPrinter(Setting));
                        break;
                    case "LANIP":     //區域網路主機IP
                        ResponseWriteEnd(context, GetLANIP(Setting));
                        break;
                    case "EasycardPort":     //悠遊卡COM Port
                        ResponseWriteEnd(context, GetEasycardPort(Setting));
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
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "bill error", "", RspnMsg);
            }

            Bill.ErrorObject root = new Bill.ErrorObject();
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "候位前台"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " booking.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion


        #region 印表機列表
        private String GetPrinter(String setting)
        {
            List<POSData.Printer> Printer = new List<POSData.Printer>();
            POSData.Printer PList;

            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                String Str_sql = "select id,title,PrinterName from printer order by id";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            PList = new POSData.Printer
                            {
                                id = reader["id"].ToString(),
                                title = reader["title"].ToString(),
                                PrinterName = reader["PrinterName"].ToString()
                            };
                            Printer.Add(PList);
                        }
                    }
                }
                finally { reader.Close(); }
            }
            return JsonConvert.SerializeObject(Printer);
        }
        #endregion

        #region 區域網路主機IP
        private String GetLANIP(String setting)
        {
            POSData.LANIP Lanip = new POSData.LANIP();
            String IP = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                String Str_sql = "select LANIP from POSSetting";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            IP = reader["LANIP"].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }

            Lanip.IP = IP;
            return JsonConvert.SerializeObject(Lanip);
        }
        #endregion

        #region 悠遊卡COM Port
        private String GetEasycardPort(String setting)
        {
            POSData.EasycardPort Lanip = new POSData.EasycardPort();
            String EasycardPort = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                String Str_sql = "select EasycardPort from POSSetting";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            EasycardPort = reader["EasycardPort"].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }

            Lanip.Port = EasycardPort;
            return JsonConvert.SerializeObject(Lanip);
        }
        #endregion

    }
}