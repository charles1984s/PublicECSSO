using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace ECSSO.Library.Order
{
    public enum AffiliatesAtatusList : int
    {
        Confirm = 0,
        Return = 1
    }
    public class Affiliates
    {
        public string api { get; set; }
        public AffiliatesAtatusList status { get; set; }
        public string order { get; set; }
        public string server_subid { get; set; }
        public bool enable { get; set; }
        private CheckToken checkToken { get; set; }
        public Affiliates(string orderid, CheckToken checkToken)
        {
            this.checkToken = checkToken;
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select top 1 isnull(Affi_Site_ID,'') SiteID,Affi_API_Key from head
                ", conn);
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        enable = reader["SiteID"].ToString() != "";
                        api = reader["Affi_API_Key"].ToString();
                        order = orderid;
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        public void setAffiliates()
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select state,affi_id,Click_ID
                    from orders_hd
                    where isnull(affi_id,'')<>'' and id=@OrderId
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@OrderId", order));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (int.Parse(reader["state"].ToString()) != 4 &&
                            int.Parse(reader["state"].ToString()) != 7)
                            throw new Exception("尚未完成訂單");
                        server_subid = reader["Click_ID"].ToString();
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        public string Finish()
        {
            string result = "";
            if (enable)
            {
                status = AffiliatesAtatusList.Confirm;
                result = submitAffiliates();
            }
            return result;
        }
        public void Cancel()
        {
            if (enable)
            {
                status = AffiliatesAtatusList.Return;
                setAffiliates();
                submitAffiliates();
            }
        }

        public string submitAffiliates()
        {
            if (!enable || string.IsNullOrEmpty(server_subid)) return null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://vbtrax.com/api/advertisers/orders/modify.json");
                request.Method = "POST";
                string postData = "api=" + api + "&server_subid=" + server_subid;
                postData += "&order=" + order + "&status=" + status.ToString().ToLower();

                //throw new Exception(postData);
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();

                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();

                dataStream = response.GetResponseStream();
                StreamReader reader2 = new StreamReader(dataStream);
                string responseFromServer = reader2.ReadToEnd();
                checkToken.GS.InsertLog(
                    checkToken.setting,
                    checkToken.token.id, "訂單管理", "將訊息傳送到聯盟網", order,
                    postData,
                    "Library/Order/Affiliates.cs"
                );

                reader2.Close();
                dataStream.Close();
                response.Close();
                return responseFromServer;
            }
            catch (WebException even)
            {
                throw new Exception(even.Message + ":" + ServicePointManager.SecurityProtocol);
            }
        }
    }
}