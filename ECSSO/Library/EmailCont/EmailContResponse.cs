using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;

namespace ECSSO.Library.EmailCont
{
    public class EmailContResponse : responseJson
    {
        public List<EmailCont> list { get; set; }
        private string setting { get; set; }
        public EmailContResponse() { }
        public EmailContResponse(string setting)
        {
            setSetting(setting);
        }
        public void setSetting(string s) {
            this.setting = s;
        }
        private void insetRFQ()
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    if not exists(select * from EmailCont where id in (1,2,3)) begin
	                    insert into EmailCont(id)values(1),(2),(3)
                    end
                ", conn);
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        private void insetStore2()
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    if not exists(select * from EmailCont where id in (4,5,6)) begin
	                    insert into EmailCont(id)values(4),(5),(6)
                    end
                ", conn);
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        private void init()
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select RFQType,storeType from CurrentUseHead
                ", conn);
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (reader["RFQType"].ToString() == "S") insetRFQ();
                        if (reader["storeType"].ToString() == "2") insetStore2();
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public void sendEmail(string seting, EmailCont cont, string title, string sender, string mail)
        {
            SmtpServer smtp = new SmtpServer(seting);
            smtp.send(cont, title, sender, mail);
        }
        public EmailCont getItem(int id)
        {
            EmailCont item = null;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from EmailCont where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        item = new EmailCont
                        {
                            id = int.Parse(reader["id"].ToString()),
                            signature = WebUtility.HtmlDecode(reader["signature"].ToString()),
                            introduction = WebUtility.HtmlDecode(reader["introduction"].ToString())
                        };
                    }
                    else {
                        item = new EmailCont
                        {
                            id = id,
                            signature = "",
                            introduction = ""
                        };
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return item;
        }
        public void setList()
        {
            init();
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from EmailCont
                ", conn);
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    list = new List<EmailCont>();
                    while (reader.Read())
                    {
                        list.Add(new EmailCont
                        {
                            id = int.Parse(reader["id"].ToString()),
                            signature = reader["signature"].ToString(),
                            introduction = reader["introduction"].ToString()
                        });
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public void GetSender(out string title, out string sender)
        {
            string s = "", t = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select service_mail,title from CurrentUseHead
                ", conn);
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        t = $"【{reader["title"].ToString()}】";
                        s = reader["service_mail"].ToString();
                        if (string.IsNullOrEmpty(s)) s = "service@etehr.com.tw";
                    }
                }
                catch { }
                finally
                {
                    sender = s;
                    title = t;
                }
            }
        }
        public void GetOrderMail(string orderId, out int price, out string date, out string name, out string mail)
        {
            mail = "";
            name = "";
            price = 0;
            date = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select mail,amt,name,convert(nvarchar,edate,111) edate from orders_hd where id = @OrderId
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@OrderId", orderId));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        mail = reader["mail"].ToString();
                        name = reader["name"].ToString();
                        price = int.Parse(reader["amt"].ToString());
                        date = reader["edate"].ToString();
                    }
                }
                catch { }
            }
        }
        public void save(string seting, EmailCont item)
        {
            using (SqlConnection conn = new SqlConnection(seting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update EmailCont 
                    set [signature]=@signature,introduction=@introduction
                    where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", item.id));
                cmd.Parameters.Add(new SqlParameter("@signature", item.signature));
                cmd.Parameters.Add(new SqlParameter("@introduction", item.introduction));
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}