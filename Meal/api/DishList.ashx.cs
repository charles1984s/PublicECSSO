using System;
using System.Collections.Generic;
using System.Web;
using Meal.Library;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.IO;

namespace Meal.api
{
    /// <summary>
    /// DishList 的摘要描述
    /// </summary>
    public class DishList : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填",""));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填",""));
            if (context.Request.Params["OrderID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填",""));


            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填",""));
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填",""));
            if (context.Request.Params["OrderID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填",""));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String OrderID = context.Request.Params["OrderID"].ToString();

            GetMealStr GS = new GetMealStr();
            String Setting = GS.GetSetting2(SiteID);
            String OrgName = GS.GetOrgName2(Setting);

            if (GS.MD5Check(SiteID + OrgName + OrderID, ChkM))
            {

                //String Path = @"c:\printer\test.txt";
                String FileText = "";
                String ProdName = "";
                Int32 TotalAmt = 0;
                String einvoice_title = "";
                String einvoice_addr = "";

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select einvoice_title,einvoice_addr from head", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                einvoice_title = reader[0].ToString();
                                einvoice_addr = reader[1].ToString();
                            }
                        }
                    }
                    finally {
                        reader.Close();
                    }
                }

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select cdate,convert(int,amt)+convert(int,freightamount)-convert(int,bonus_discount)-convert(int,discount_amt),freightamount,bonus_discount,discount_amt,name,tableid,takemealtype from orders_hd where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", OrderID));

                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                FileText += String.Format("{0}", einvoice_title) + "\r\n";
                                //FileText += String.Format("賣方統編:{0}", "25074611") + "\r\n";
                                //FileText += String.Format("{0}", einvoice_addr) + "\r\n";
                                //FileText += String.Format("買方:{0}", "易碩網際科技股份有限公司") + "\r\n";
                                //FileText += String.Format("買方統編:{0}", "27693379") + "\r\n";
                                //FileText += String.Format("買方地址:{0}", "807高雄市三民區鼎昌街258號") + "\r\n";
                                FileText += String.Format("{0}", GS.GetMealType(reader[7].ToString())).PadRight(5);
                                if (reader[7].ToString() == "1") {
                                    FileText += String.Format("桌號:{0}", reader[6].ToString()).PadRight(5);
                                }
                                FileText += String.Format("單號:{0}", OrderID) + "\r\n";
                                FileText += String.Format("{0}", Convert.ToDateTime(reader[0].ToString()).ToString("yyyy-MM-dd HH:mm:ss")) + "\r\n";
                                FileText += "------------------------------\r\n";
                                //FileText += String.Format("載具號碼:{0}", "") + "\r\n";
                                //FileText += String.Format("愛心碼:{0}", "") + "\r\n";
                                //FileText += String.Format("自然人憑證:{0}", "") + "\r\n\r\n";
                                //ListTitle += ChineseHelper.insertSpace("購物明細", 40) + "\r\n";
                                //ListTitle += ChineseHelper.insertSpace("商品名稱", 40);
                                //ListTitle += ChineseHelper.insertSpace("料號", 10);
                                //ListTitle += ChineseHelper.insertSpace("尺寸", 6);
                                //ListTitle += ChineseHelper.insertSpace("顏色", 6);
                                //ListTitle += ChineseHelper.insertSpace("  數量", 6);
                                //ListTitle += ChineseHelper.insertSpace("  單價", 6);
                                //ListTitle += ChineseHelper.insertSpace("  折扣", 6);
                                //ListTitle += ChineseHelper.insertSpace("  小計", 6);
                                //FileText += ListTitle + "\r\n";
                                using (SqlConnection conn2 = new SqlConnection(Setting))
                                {
                                    conn2.Open();
                                    SqlCommand cmd2 = new SqlCommand("select prod_name,amt,isnull(b.title,'') as sizetitle,isnull(c.title,'') as colortitle ,qty,price,p_no,discount,discription,memo from orders as a left join prod_size as b on a.sizeid=b.id left join prod_color as c on a.colorid=c.id where order_no=@id", conn2);
                                    cmd2.Parameters.Add(new SqlParameter("@id", OrderID));

                                    SqlDataReader reader2 = cmd2.ExecuteReader();
                                    try
                                    {
                                        if (reader2.HasRows)
                                        {
                                            while (reader2.Read())
                                            {
                                                if (reader2[9].ToString() != "") { ProdName = "(" + reader2[9].ToString() + ")" + reader2[0].ToString() + " "; }
                                                else { ProdName = reader2[0].ToString() + " "; }
                                                if (reader2[2].ToString() != "") { ProdName += "/" + reader2[2].ToString() + " "; }
                                                if (reader2[3].ToString() != "") { ProdName += "/" + reader2[3].ToString() + " "; }
                                                FileText += ProdName + "\r\n";

                                                FileText += String.Format("{0}", reader2[4].ToString() + "*" + reader2[5].ToString() + (Convert.ToInt32(reader2[4].ToString()) * Convert.ToInt32(reader2[5].ToString())).ToString().PadLeft(8, ' ') + "TX").PadLeft(30, ' ') + "\r\n";

                                                if (Convert.ToInt32(reader2[7].ToString()) > 0)
                                                {
                                                    FileText += "折扣\r\n";
                                                    FileText += String.Format("{0}", String.Format("{0}", "-" + reader2[7].ToString()).PadLeft(8, ' ') + "TX").PadLeft(30, ' ') + "\r\n";
                                                }
                                                else if (Convert.ToInt32(reader2[7].ToString()) < 0)
                                                {
                                                    FileText += reader2[8].ToString() + "\r\n";
                                                    FileText += String.Format("{0}", reader2[7].ToString().Replace("-", "").PadLeft(8, ' ') + "TX").PadLeft(30, ' ') + "\r\n";
                                                }
                                                //FileText += reader2[1].ToString().PadLeft(8, ' ') + "TX\r\n";
                                                TotalAmt = TotalAmt + Convert.ToInt32(reader2[1].ToString());
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        reader2.Close();
                                    }
                                }
                                FileText += "------------------------------\r\n";
                                //FileText += String.Format("商品金額合計:{0}", TotalAmt) + "\r\n";
                                if (Convert.ToInt32(reader[4].ToString()) > 0)
                                {
                                    FileText += "訂單總折扣\r\n";
                                    FileText += String.Format("{0}", "-" + reader[4].ToString() + "TX").PadLeft(30, ' ') + "\r\n";
                                }
                                if (Convert.ToInt32(reader[2].ToString()) > 0)
                                {
                                    FileText += "運費\r\n";
                                    FileText += String.Format("{0}", reader[2].ToString() + "TX").PadLeft(30, ' ') + "\r\n";
                                }
                                if (Convert.ToInt32(reader[3].ToString()) > 0)
                                {
                                    FileText += "紅利折抵\r\n";
                                    FileText += String.Format("{0}", "-" + reader[3].ToString() + "TX").PadLeft(30, ' ') + "\r\n";
                                }
                                Double TAX = Math.Round(Convert.ToDouble(reader[1].ToString()) / 1.05, 0, MidpointRounding.AwayFromZero);

                                FileText += "應稅銷售額(不含稅)\r\n";
                                FileText += String.Format("{0}", TAX.ToString().PadLeft(30, ' ')) + "\r\n";
                                //FileText += "免稅銷售額(不含稅)\r\n";
                                //FileText += String.Format("{0}", "0").PadLeft(30, ' ') + "\r\n";
                                FileText += "稅額\r\n";
                                FileText += String.Format("{0}", (Convert.ToDouble(reader[1].ToString()) - TAX).ToString().PadLeft(30, ' ')) + "\r\n";
                                FileText += "總計\r\n";
                                FileText += String.Format("{0}", reader[1].ToString()).PadLeft(30, ' ') + "\r\n";
                                //FileText += "備註:\r\n";
                                //FileText += String.Format("{0}", "1.備註一備註一備註一備註一註一") + "\r\n";
                                //FileText += String.Format("{0}", "2.備註二55備註二55備註二55") + "\r\n";
                                FileText += "------------------------------";
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                //WriteTxt(Path, FileText);
                context.Response.Write(FileText);

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

        private void WriteTxt(String path, String FileText)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //開始寫入
            sw.Write(FileText);
            //清空緩衝區
            sw.Flush();
            //關閉流
            sw.Close();
            fs.Close();
        }

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
    }
}