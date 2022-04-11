using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;
namespace ECSSO.api
{
    /// <summary>
    /// Einvoice 的摘要描述
    /// </summary>
    public class Einvoice : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Params["OrderID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填"));


            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Params["OrderID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填"));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String OrderID = context.Request.Params["OrderID"].ToString();

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);

            if (GS.MD5Check(SiteID + OrgName + OrderID, ChkM))
            {
                //String Path = HttpContext.Current.Server.MapPath(@"~/upload/" + OrgName + "/" + OrderID + ".txt");
                String Path = @"c:\inetpub\printer\test.txt";
                String FileText = "";
                String ProdName = "";
                Int32 TotalAmt = 0;

                String einvoice_title = "";
                String einvoice_no = "";
                String einvoice_addr = "";

                String[] OrderIDList = OrderID.Split('/');

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select einvoice_title,einvoice_no,einvoice_addr from head", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", OrderID));

                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                einvoice_title = reader[0].ToString();
                                einvoice_no = reader[1].ToString();
                                einvoice_addr = reader[2].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                String Custname = "";
                String Custeinvoice_no = "";
                String Custeinvoice_addr = "";
                String Custcarrierid1 = "";
                String CustNPOBAN = "";
                String OrderDate = "";

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select name,einvoice_no,einvoice_addr,carrierid1,NPOBAN,cdate from orders_hd where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", OrderIDList[0]));
                   
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Custname = reader[0].ToString();
                                Custeinvoice_no = reader[1].ToString();
                                Custeinvoice_addr = reader[2].ToString();
                                Custcarrierid1 = reader[3].ToString();
                                CustNPOBAN = reader[4].ToString();
                                OrderDate = Convert.ToDateTime(reader[5].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                

                FileText += String.Format("賣方:{0}", einvoice_title) + "\r\n";
                FileText += String.Format("賣方統編:{0}", einvoice_no) + "\r\n";
                FileText += String.Format("賣方地址:{0}", einvoice_addr) + "\r\n";
                FileText += String.Format("買方:{0}", Custname) + "\r\n";
                FileText += String.Format("買方統編:{0}", Custeinvoice_no) + "\r\n";
                FileText += String.Format("買方地址:{0}", Custeinvoice_addr) + "\r\n";
                FileText += String.Format("訂單編號:{0}", OrderIDList[0]) + "\r\n";
                FileText += String.Format("訂購日期:{0}", OrderDate) + "\r\n";
                FileText += String.Format("載具號碼:{0}", Custcarrierid1) + "\r\n";
                FileText += String.Format("愛心碼:{0}", CustNPOBAN) + "\r\n";
                FileText += String.Format("自然人憑證:{0}", "") + "\r\n\r\n";

                String Sql1 = "(";
                for (int i = 0; i < OrderIDList.Length; i++)
                {
                    if (i == 0)
                    {
                        Sql1 += "@id" + i;
                    }
                    else 
                    {
                        Sql1 += ",@id" + i;
                    }
                }
                Sql1 += ")";

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select sum(convert(int,amt))+sum(convert(int,freightamount))-sum(convert(int,bonus_discount))-sum(convert(int,discount_amt)),sum(convert(int,freightamount)),sum(convert(int,bonus_discount)),sum(convert(int,discount_amt)) from orders_hd where id in " + Sql1, conn);

                    for (int i = 0; i < OrderIDList.Length; i++)
                    {
                        cmd.Parameters.Add(new SqlParameter("@id" + i, OrderIDList[i]));
                    }
                    

                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read()) 
                            {
                                
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
                                    SqlCommand cmd2 = new SqlCommand("select prod_name,amt,isnull(b.title,'') as sizetitle,isnull(c.title,'') as colortitle ,qty,price,p_no,discount,discription,memo from orders as a left join prod_size as b on a.sizeid=b.id left join prod_color as c on a.colorid=c.id where order_no in " + Sql1 + " order by a.order_no,a.ser_no", conn2);
                                    for (int i = 0; i < OrderIDList.Length; i++)
                                    {
                                        cmd2.Parameters.Add(new SqlParameter("@id" + i, OrderIDList[i]));
                                    }
                    

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
                                //FileText += String.Format("商品金額合計:{0}", TotalAmt) + "\r\n";
                                if (Convert.ToInt32(reader[3].ToString()) > 0) 
                                {
                                    FileText += "訂單總折扣\r\n";
                                    FileText += String.Format("{0}", "-" + reader[3].ToString() + "TX").PadLeft(30, ' ') + "\r\n";
                                }
                                if (Convert.ToInt32(reader[1].ToString()) > 0) 
                                {
                                    FileText += "運費\r\n";
                                    FileText += String.Format("{0}", reader[1].ToString() + "TX").PadLeft(30, ' ') + "\r\n";
                                }
                                if (Convert.ToInt32(reader[2].ToString()) > 0)
                                {
                                    FileText += "紅利折抵\r\n";
                                    FileText += String.Format("{0}", "-" + reader[2].ToString() + "TX").PadLeft(30, ' ') + "\r\n";
                                }
                                Double TAX = Math.Round(Convert.ToDouble(reader[0].ToString())/1.05,0, MidpointRounding.AwayFromZero);

                                FileText += "應稅銷售額(不含稅)\r\n";
                                FileText += String.Format("{0}", TAX.ToString().PadLeft(30, ' ')) + "\r\n";
                                FileText += "免稅銷售額(不含稅)\r\n";
                                FileText += String.Format("{0}", "0").PadLeft(30, ' ') + "\r\n";
                                FileText += "稅額\r\n";
                                FileText += String.Format("{0}",(Convert.ToDouble(reader[0].ToString()) - TAX).ToString().PadLeft(30, ' ')) + "\r\n";
                                FileText += "總計\r\n";
                                FileText += String.Format("{0}", reader[0].ToString()).PadLeft(30, ' ') + "\r\n";
                                FileText += "備註:\r\n";
                                if (OrderIDList.Length > 1) 
                                {
                                    FileText += String.Format("{0}", "合併單號:" + OrderID) + "\r\n";
                                }
                                
                                FileText += String.Format("{0}", "") + "\r\n";
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
        private String ErrorMsg(String RspnCode, String RspnMsg)
        {

            Library.Products.RootObject root = new Library.Products.RootObject();
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

        private void WriteTxt(String path,String FileText) 
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


        public static class ChineseHelper
        {
            static ChineseHelper()
            {
                Encoding = Encoding.GetEncoding(950); //950是Big5的CodePage
            }

            public static Encoding Encoding { get; set; }

            /// <summary>
            /// 判斷文字的Bytes數有沒有超過上限，有的話截斷，沒有的話補空白
            /// </summary>        
            public static string insertSpace(string value, int maxLength)
            {
                if (string.IsNullOrWhiteSpace(value) || maxLength <= 0)
                {
                    return string.Empty;
                }

                var result = GetStringBase(value, maxLength);

                if (result.Item2 == 0)
                {
                    return result.Item1;
                }
                else
                {
                    return result.Item1 + "".PadRight(result.Item2);
                }
            }

            /// <summary>
            /// 判斷文字的Bytes數有沒有超過上限，有的話截斷
            /// </summary>
            public static string GetString(string value, int maxLength)
            {
                if (string.IsNullOrWhiteSpace(value) || maxLength <= 0)
                {
                    return string.Empty;
                }

                return GetStringBase(value, maxLength).Item1;
            }

            /// <summary>
            /// 給截字補空與截字使用
            /// </summary>
            private static Tuple<string, int> GetStringBase(string value, int maxLength)
            {
                int padding = 0;
                var buffer = Encoding.GetBytes(value);
                if (buffer.Length > maxLength)
                {
                    int charStartIndex = maxLength - 1;
                    int charEndIndex = 0;
                    //跑回圈去算出結尾。
                    for (int i = 0; i < maxLength; )
                    {
                        if (buffer[i] <= 128)
                        {
                            charEndIndex = i; //英數1Byte
                            i += 1;
                        }
                        else
                        {
                            charEndIndex = i + 1; //中文2Byte
                            i += 2;
                        }
                    }

                    //如果開始不同與結尾，表示截到2Byte的中文字了，要捨棄1Byte
                    if (charStartIndex != charEndIndex)
                    {
                        value = Encoding.GetString(buffer, 0, charStartIndex);
                        padding = 1;
                    }
                    else
                    {
                        value = Encoding.GetString(buffer, 0, maxLength);
                    }
                }
                else
                {
                    value = Encoding.GetString(buffer);

                    padding = maxLength - buffer.Length;
                }

                return Tuple.Create(value, padding);
            }
        }
    }
}