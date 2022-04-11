using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;

namespace ECSSO.Library
{
    public class Logistics
    {

        public class RootLogistic
        {
            public string Logisticsapi { get; set; }
            public List<Logistic> Logistics { get; set; }
            public RootLogistic() { }
            public RootLogistic(string Setting) {
                Logistics = new List<Logistic>();
                Logistic Logistic = new Logistic();
                List<string> Canpay = Logistic.CanPayment(Setting);
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("select DefaultValue,LogisticsType,FreigntType,FreigntAmt,FreightFree,title,id,FreigntAmt2 from LogisticsSetting", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Logistic logistic = new Logistic
                                {
                                    ID = reader[6].ToString(),
                                    Title = reader[5].ToString(),
                                    Default = reader[0].ToString(),
                                    LogisticstypeID = reader[1].ToString(),
                                    FreightType = reader[2].ToString(),
                                    FreightAmt = reader[3].ToString(),
                                    FreightAmt2 = reader[7].ToString(),
                                    FreightFree = reader[4].ToString()
                                };
                                logistic.PaymentType = logistic.GetPaymentType(Setting, reader[1].ToString(), Canpay);
                                Logistics.Add(logistic);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
        }

        public class Logistic
        {
            public string ID { get; set; }
            public string Title { get; set; }
            public string LogisticstypeID { get; set; }
            public string FreightType { get; set; }
            public string FreightAmt { get; set; }
            public string FreightAmt2 { get; set; }
            public string FreightFree { get; set; }
            public string Default { get; set; }
            public List<PaymentType> PaymentType { get; set; }
            public List<packageList> packageList { get; set; }
            #region 取得開放金流
            public List<string> CanPayment(string Setting)
            {
                List<string> CanPay = new List<string>();

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
                        CanPayment(Setting, CanPay);
                        reader.Close();
                    }
                }
                #endregion

                return CanPay;
            }
            public void CanPayment(String Setting, List<String> list)
            {
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select id,code from paymenttype where isnull(ThirdID,'')!='' and used='Y'", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            list.Add(reader["id"].ToString());
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            public List<PaymentType> GetPaymentType(String Setting, String LogisticstypeID, List<String> CanPay)
            {
                String Sqlstr = "select isnull(c.title,'') as title,isnull(c.code,'') as code,isnull(a.amountlimit,'') as amountlimit,c.id from Logisticstype_paymenttype as a left join Logisticstype as b on a.Logisticstype_id=b.id left join paymenttype as c on a.paymennttype_id=c.id where a.Logisticstype_id=@L and a.paymennttype_id in (";
                for (int i = 0; i < CanPay.Count; i++)
                {
                    if (i == 0)
                    {
                        Sqlstr += "@P" + i;
                    }
                    else
                    {
                        Sqlstr += ",@P" + i;
                    }

                }
                Sqlstr += ")";

                List<PaymentType> PaymentType = new List<PaymentType>();
                PaymentType PaymentTypeList;

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
                                if (reader[0].ToString() != "")
                                {
                                    PaymentTypeList = new PaymentType
                                    {
                                        id = reader["id"].ToString(),
                                        title = reader[0].ToString(),
                                        value = reader[1].ToString(),
                                        AmountLimit = reader[2].ToString()
                                    };

                                    PaymentType.Add(PaymentTypeList);
                                }

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
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
            public List<packageList> GetFreightList(String Setting, String LogisticstypeSettingID, String prodList)
            {
                List<packageList> packageList = new List<Library.Logistics.packageList>();
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
                        else
                        {
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
                return packageList;
            }
            #endregion
        }

        public class PaymentType
        {
            public string id { get; set; }
            public string title { get; set; }
            public string value { get; set; }
            public string AmountLimit { get; set; }
        }

        public class packageList
        {
            public string title { get; set; }
            public int min { get; set; }
            public int max { get; set; }
            public int price { get; set; }
        }
    }
}