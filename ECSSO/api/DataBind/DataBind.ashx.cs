using System;
using System.Web;
using System.Data.SqlClient;
using System.Collections.Generic;
using Newtonsoft.Json;
using ECSSO.Library.DataBind;

namespace ECSSO.api.DataBind
{
    /// <summary>
    /// DataBind 的摘要描述
    /// </summary>
    public class DataBind : IHttpHandler
    {
        private DataBindInterface reponse;
        private string setting;
        public void ProcessRequest(HttpContext context)
        {
            GetStr gs = new GetStr();
            reponse = new DataBindInterface();
            try
            {
                setting = gs.checkToken(context.Request.Form["token"]);
                if (setting.IndexOf("error") < 0)
                {
                    if (context.Request.Form["type"] != null)
                    {
                        if (context.Request.Form["id"] != null)
                        {
                            getData(int.Parse(context.Request.Form["type"]), context.Request.Form["id"]);
                        }
                        else
                        {
                            reponse.code = "401.2";
                            reponse.message = "資料錯誤";
                        }
                    }
                    else
                    {
                        reponse.code = "401.1";
                        reponse.message = "資料錯誤";
                    }
                }
                else
                {
                    reponse.code = "401";
                    reponse.message = "Token已過期";
                }
            }
            catch (Exception ex)
            {
                reponse.code = "500";
                reponse.message = ex.StackTrace;
            }
            finally
            {
                gs.ResponseWriteEnd(context, JsonConvert.SerializeObject(reponse));
            }
        }
        private void getData(int type, string id)
        {
            switch (type)
            {
                case 1:
                    getProdAuthorsData(id);
                    break;
                case 2:
                    getProdListData(id);
                    break;
                case 3:
                    getProdData(id);
                    break;
                case 4:
                    getSellAuthorsData(id);
                    break;
                case 5:
                    getSellListData(id);
                    break;
                default:
                    reponse.code = "401.3";
                    reponse.message = "資料錯誤";
                    break;
            }
        }
        private void getProdData(string id)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from prod where disp_opt='Y' and id in(select id from bindData where [type]=3 and bid=@bid)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@bid", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    reponse.data = new List<DataBindDetailInterface>();
                    while (reader.Read())
                    {
                        DataBindDetailInterface detail = new DataBindDetailInterface
                        {
                            id = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString()
                        };
                        reponse.data.Add(detail);
                    }
                    reponse.code = "200";
                    reponse.message = "success";
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private void getProdListData(string id)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from prod_list where disp_opt='Y' and id in(select id from bindData where [type]=2 and bid=@bid) order by ser_no,id desc
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@bid", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    reponse.data = new List<DataBindDetailInterface>();
                    while (reader.Read())
                    {
                        DataBindDetailInterface detail = new DataBindDetailInterface
                        {
                            id = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString()
                        };
                        reponse.data.Add(detail);
                    }
                    reponse.code = "200";
                    reponse.message = "success";
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private void getProdAuthorsData(string id)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from prod_authors where disp_opt='Y' and id in(select id from bindData where [type]=1 and bid=@bid) order by ser_no,id desc
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@bid", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    reponse.data = new List<DataBindDetailInterface>();
                    while (reader.Read())
                    {
                        DataBindDetailInterface detail = new DataBindDetailInterface
                        {
                            id = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString()
                        };
                        reponse.data.Add(detail);
                    }
                    reponse.code = "200";
                    reponse.message = "success";
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private void getSellAuthorsData(string id)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from prod_authors where disp_opt='Y' and id in(select id from bindData where [type]=4 and bid=@bid) order by ser_no,id desc
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@bid", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    reponse.data = new List<DataBindDetailInterface>();
                    while (reader.Read())
                    {
                        DataBindDetailInterface detail = new DataBindDetailInterface
                        {
                            id = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString()
                        };
                        reponse.data.Add(detail);
                    }
                    reponse.code = "200";
                    reponse.message = "success";
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private void getSellListData(string id)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from prod_list where disp_opt='Y' and id in(select id from bindData where [type]=5 and bid=@bid) order by ser_no,id desc
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@bid", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    reponse.data = new List<DataBindDetailInterface>();
                    while (reader.Read())
                    {
                        DataBindDetailInterface detail = new DataBindDetailInterface
                        {
                            id = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString()
                        };
                        reponse.data.Add(detail);
                    }
                    reponse.code = "200";
                    reponse.message = "success";
                }
                finally
                {
                    reader.Close();
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
    }
}