using ECSSO.Library;
using ECSSO.Library.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ECSSO.api.ThirdParty
{
    /// <summary>
    /// ThirdPartyHandler 的摘要描述
    /// </summary>
    public class ThirdPartyHandler : IHttpHandler
    {
        private ThirdPartyList third;
        private CheckToken checkToken;
        public void ProcessRequest(HttpContext context)
        {
            third = new ThirdPartyList();
            checkToken = new CheckToken(third);
            try {
                third.RspnCode = "500";
                checkToken.check(context);
                if (third.RspnCode=="200") {
                    third.RspnCode = "500.1";
                    Method(context);
                    third.RspnCode = "200";
                }
                else throw new Exception("Token不存在");
            }
            catch (Exception ex)
            {
                third.RspnMsg = ex.Message;
            }
            finally
            {
                context.Response.Write(checkToken.printMsg());
            }
        }
        private void Method(HttpContext context) {
            switch (context.Request.Form["type"].ToUpper())
            {
                case "GET":
                    Get();
                    break;
                case "POST":
                    third.List = JsonConvert.DeserializeObject<List<ThirdPartyItem>>(context.Request.Form["data"]);
                    third.List.ForEach(e => {
                        Post(e);
                    });
                    break;
            }
        }
        private void Post(ThirdPartyItem e) {
            using (SqlConnection conn = new SqlConnection(checkToken.setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update ThirdParty 
                    set shopID=@shopID,account=@account,code1=@code1,code2=@code2,[password]=@password,
	                    TaxID=@TaxID,expire_day=@expire_day,auto_deposit=@auto_deposit
                    where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", e.ID));
                cmd.Parameters.Add(new SqlParameter("@shopID", 
                    e.Keypair.Exists(x => x.code == "shopID") ? 
                        e.Keypair.Find(x => x.code == "shopID").value : ""
                ));
                cmd.Parameters.Add(new SqlParameter("@account",
                    e.Keypair.Exists(x => x.code == "account") ?
                        e.Keypair.Find(x => x.code == "account").value : ""
                ));
                cmd.Parameters.Add(new SqlParameter("@code1",
                    e.Keypair.Exists(x => x.code == "code1") ?
                        e.Keypair.Find(x => x.code == "code1").value : ""
                ));
                cmd.Parameters.Add(new SqlParameter("@code2",
                    e.Keypair.Exists(x => x.code == "code2") ?
                        e.Keypair.Find(x => x.code == "code2").value : ""
                ));
                cmd.Parameters.Add(new SqlParameter("@password",
                    e.Keypair.Exists(x => x.code == "password") ?
                        e.Keypair.Find(x => x.code == "password").value : ""
                ));
                cmd.Parameters.Add(new SqlParameter("@TaxID",
                    e.Keypair.Exists(x => x.code == "TaxID") ?
                        e.Keypair.Find(x => x.code == "TaxID").value : ""
                ));
                cmd.Parameters.Add(new SqlParameter("@expire_day",
                    e.Keypair.Exists(x => x.code == "expire_day") ?
                        e.Keypair.Find(x => x.code == "expire_day").value : ""
                ));
                cmd.Parameters.Add(new SqlParameter("@auto_deposit",
                    e.Keypair.Exists(x => x.code == "auto_deposit") ?
                        e.Keypair.Find(x => x.code == "auto_deposit").value : ""
                ));
                SqlDataReader reader = null;
                try {
                    reader = cmd.ExecuteReader();
                    e.Payments.ForEach(p => {
                        savePaymenttype(p);
                    });
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private void savePaymenttype(PaymentTypeItem item) {
            using (SqlConnection conn = new SqlConnection(checkToken.setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update paymenttype set used=@used where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@used", item.used?"Y":"N"));
                cmd.Parameters.Add(new SqlParameter("@id", item.ID));
                SqlDataReader reader = null;
                try {
                    reader = cmd.ExecuteReader();
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private void Get() {
            third.RspnCode = "500.11";
            third.List = new List<ThirdPartyItem>();
            using (SqlConnection conn = new SqlConnection(checkToken.setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from ThirdParty order by ser_no
                ", conn);
                SqlDataReader reader = null;
                try
                {
                    third.RspnCode = "500.12";
                    reader = cmd.ExecuteReader();
                    third.RspnCode = "500.13";
                    while (reader.Read())
                    {
                        ThirdPartyItem thirdPartyItem = new ThirdPartyItem
                        {
                            ID = reader["id"].ToString(),
                            Title = reader["title"].ToString(),
                            Keypair = getKeypair(reader["id"].ToString()),
                            Payments = getPayments(reader["id"].ToString())
                        };
                        thirdPartyItem.Keypair.ForEach(e =>
                        {
                            e.value = reader[e.code].ToString();
                        });
                        third.List.Add(thirdPartyItem);
                    }
                }
                catch(Exception e) {
                    throw new Exception("無可串接之第三方支付");
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private List<PaymentTypeItem> getPayments(string id)
        {
            List<PaymentTypeItem> list = new List<PaymentTypeItem>();
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from paymenttype where ThirdID=@id and disp_opt='Y' order by ser_no
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        PaymentTypeItem payment = new PaymentTypeItem
                        {
                            ID = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString(),
                            used = reader["used"].ToString() == "Y"
                        };
                        list.Add(payment);
                    }
                }
                catch {
                    third.RspnCode = "500.2";
                    throw new Exception("無可設定的付款方式");
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return list;
        }
        private List<ThirdPartyKeypair> getKeypair(string id) {
            List < ThirdPartyKeypair > list = new List<ThirdPartyKeypair>();
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from ThirdPartyKeypair where ThirdPartyID=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ThirdPartyKeypair thirdPartyKeypair = new ThirdPartyKeypair
                        {
                            title = reader["title"].ToString(),
                            code = reader["code"].ToString()
                        };
                        list.Add(thirdPartyKeypair);
                    }
                }
                catch {
                    third.RspnCode = "500.3";
                    throw new Exception("無可設定的金流");
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return list;
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}