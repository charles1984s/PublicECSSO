using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Web.Configuration;
using System.Text.RegularExpressions;

namespace ECSSO.api
{
    /// <summary>
    /// LogisticsSetting 的摘要描述
    /// </summary>
    public class LogisticsSetting : IHttpHandler
    {
        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));
            
            if (context.Request.Params["CheckM"] == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["SiteID"] == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));
            
            String CheckM = context.Request.Params["CheckM"];
            String SiteID = context.Request.Params["SiteID"];
            String prodList = context.Request.Params["prodList"];
            
            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);
            if ((new Regex("^/d[,/d]*/d$")).IsMatch(prodList))
            {
                prodList = "0";
            }
            if (GS.MD5Check(SiteID + OrgName, CheckM))
            {
                List<Library.Logistics.Logistic> Logistic = new List<Library.Logistics.Logistic>();
                
                Library.Logistics.RootLogistic root;
                Library.Logistics.Logistic LogisticList;
                List<String> Canpay = CanPayment(Setting);

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("select DefaultValue,LogisticsType,FreigntType,FreigntAmt,FreightFree,title,id,FreigntAmt2 from LogisticsSetting order by FreigntAmt", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                LogisticList = new Library.Logistics.Logistic
                                {
                                    ID = reader[6].ToString(),
                                    Title = reader[5].ToString(),
                                    Default = reader[0].ToString(),
                                    LogisticstypeID = reader[1].ToString(),
                                    FreightType = reader[2].ToString(),
                                    FreightAmt = reader[3].ToString(),
                                    FreightAmt2 = reader[7].ToString(),
                                    FreightFree = reader[4].ToString(),
                                    PaymentType = GetPaymentType(Setting, reader[1].ToString(), Canpay),
                                    packageList = (reader[2].ToString() == "4" ? GetFreightList(Setting, reader[6].ToString(), prodList) : null)
                                };
                                Logistic.Add(LogisticList);
                            }
                        }
                    }
                    catch (Exception e) {
                        ResponseWriteEnd(context, ErrorMsg("error", e.Message, ""));
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                root = new Library.Logistics.RootLogistic
                {
                    Logisticsapi = GetLogisticsapi(Setting),
                    Logistics = Logistic
                };

                ResponseWriteEnd(context, JsonConvert.SerializeObject(root));
            }else ResponseWriteEnd(context, ErrorMsg("error", "驗證錯誤", ""));
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public class RootObject
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "Logistics error", "", RspnMsg);
            }

            RootObject root = new RootObject();
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "物流"));
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

        #region 取得網站是介接哪家物流
        private String GetLogisticsapi(String Setting)
        {
            String ReturnStr = "";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("select Logistics_FAMI,Logistics_UNIMART,Logistics_FAMIC2C,Logistics_UNIMARTC2C,Logistics_TCAT,Logistics_ECAN from head", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader[0].ToString() == "Y" || reader[1].ToString() == "Y" || reader[2].ToString() == "Y" || reader[3].ToString() == "Y" || reader[4].ToString() == "Y" || reader[5].ToString() == "Y")
                            {
                                ReturnStr= "ecpay";
                            }
                        }
                    }
                }

                finally { reader.Close(); }
            }
            return ReturnStr;
        }
        #endregion

        #region 取得開放金流
        private List<String> CanPayment(String Setting)
        {
            List<String> CanPay = new List<string>();

            #region 取得開放的金流
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("select id,head_column from paymenttype where isnull(ThirdID,'')=''", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            using (SqlConnection conn1 = new SqlConnection(Setting))
                            {
                                conn1.Open();

                                SqlCommand cmd1 = new SqlCommand("select b." + reader[1].ToString() + " from CurrentUseFrame as a left join head as b on a.id=b.hid", conn1);
                                SqlDataReader reader1 = cmd1.ExecuteReader();
                                try
                                {
                                    if (reader1.HasRows)
                                    {
                                        while (reader1.Read())
                                        {
                                            if (reader1[0].ToString() == "Y") 
                                            {
                                                CanPay.Add(reader[0].ToString());
                                            } 
                                        }
                                    }
                                }
                                finally
                                {
                                    reader1.Close();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    CanPayment(Setting,CanPay);
                    reader.Close();
                }
            }
            #endregion

            return CanPay;
        }
        private void CanPayment(String Setting, List<String> list) {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select id,code from paymenttype where isnull(ThirdID,'')!='' and used='Y'", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try {
                    while (reader.Read()) {
                        list.Add(reader["id"].ToString());
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private List<Library.Logistics.PaymentType> GetPaymentType(String Setting, String LogisticstypeID, List<String> CanPay)
        {
            String Sqlstr = "select isnull(c.title,'') as title,isnull(c.code,'') as code,isnull(a.amountlimit,'') as amountlimit from Logisticstype_paymenttype as a left join Logisticstype as b on a.Logisticstype_id=b.id left join paymenttype as c on a.paymennttype_id=c.id where a.Logisticstype_id=@L and a.paymennttype_id in (";
            for (int i = 0; i < CanPay.Count; i++)
            {
                if (i == 0)
                {
                    Sqlstr += "@P" + i;
                }
                else {
                    Sqlstr += ",@P" + i;
                }
                
            }
            Sqlstr += ")";

            List<Library.Logistics.PaymentType> PaymentType = new List<Library.Logistics.PaymentType>();
            Library.Logistics.PaymentType PaymentTypeList;

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(Sqlstr, conn);
                cmd.Parameters.Add(new SqlParameter("@L", LogisticstypeID));
                for (int i = 0; i < CanPay.Count; i++)
                {
                    cmd.Parameters.Add(new SqlParameter("@P" + i, CanPay[i]));
                }
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader[0].ToString() != "") {
                                PaymentTypeList = new Library.Logistics.PaymentType
                                {
                                    title = reader[0].ToString(),
                                    value = reader[1].ToString(),
                                    AmountLimit = reader[2].ToString()
                                };
                                if (PaymentType.Exists(e => e.value == "getandpay") && PaymentTypeList.value == "PCHomeIPL7") {
                                    PaymentType.Remove(PaymentType.Find(e => e.value == "getandpay"));
                                }
                                if(!PaymentType.Exists(e => e.value == "PCHomeIPL7") || PaymentTypeList.value != "getandpay")
                                    PaymentType.Add(PaymentTypeList);
                            }

                        }
                    }
                }
                catch (Exception e) {
                    ResponseWriteEnd(context, Sqlstr);
                }
                finally
                {
                    reader.Close();
                }
            }
            
            return PaymentType;
        }
        #endregion

        #region 取得包裝
        private List<Library.Logistics.packageList> GetFreightList(String Setting, String LogisticstypeSettingID, String prodList)
        {
            List<Library.Logistics.packageList> packageList = new List<Library.Logistics.packageList>();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("select * from package where id in(select packageID from packageLink where type='P' and settingID in(" + prodList + ") group by packageID) order by qty2 desc", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Library.Logistics.packageList myPackage = new Library.Logistics.packageList();
                            myPackage.min = (int)reader["qty"];
                            myPackage.max = (int)reader["qty2"];
                            myPackage.price = (int)reader["price"];
                            packageList.Add(myPackage);
                        }
                    }
                    else {
                        using (SqlConnection conn2 = new SqlConnection(Setting))
                        {
                            conn2.Open();
                            SqlCommand cmd2 = new SqlCommand("select * from package where id in(select packageID from packageLink where type='L' and settingID=" + LogisticstypeSettingID + ") order by qty2 desc", conn2);
                            SqlDataReader reader2 = cmd2.ExecuteReader();
                            try
                            {
                                if (reader2.HasRows)
                                {
                                    while (reader2.Read())
                                    {
                                        Library.Logistics.packageList myPackage = new Library.Logistics.packageList();
                                        myPackage.min = (int)reader2["qty"];
                                        myPackage.max = (int)reader2["qty2"];
                                        myPackage.price = (int)reader2["price"];
                                        packageList.Add(myPackage);
                                    }
                                }
                            }
                            finally {
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
            return packageList;
        }
        #endregion
    }
}