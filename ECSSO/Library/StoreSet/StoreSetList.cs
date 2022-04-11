using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ECSSO.Library.StoreSet
{
    public class StoreSetList : responseJson
    {
        public string orgName { get; set;}
        public string jobID { get; set; }
        public List<StoreSetItem> List { get; set; }
        public StoreSetList()
        {
            jobID = "";
            List = new List<StoreSetItem>();
        }
        public StoreSetList(string job) : this()
        {
            jobID = job;
        }
        public void load()
        {
            RspnCode = "500";
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select 
                        store.* from storeSet as store
                    left join webjobs on webjobs.job_id=store.job_id
                    left join authors on webjobs.job_id=authors.job_id
                    where authors.empl_id=@mem_id and (@jobID='' or store.job_id=@jobID)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@mem_id", checkToken.token.id));
                cmd.Parameters.Add(new SqlParameter("@jobID", jobID));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        StoreSetItem item = new StoreSetItem
                        {
                            key = reader["key"].ToString(),
                            name = reader["name"].ToString(),
                            memo = reader["memo"].ToString(),
                            type = int.Parse(reader["type"].ToString()),
                            enable = bool.Parse(reader["enable"].ToString()),
                            value = reader["value"].ToString(),
                            text = reader["text"].ToString(),
                            maxlength = reader.IsDBNull(reader.GetOrdinal("maxlength"))?0:int.Parse(reader["maxlength"].ToString())
                        };
                        if (item.type == 7) item.value = item.value.Replace("/upload/",$"/upload/{orgName}/");
                        item.setDetail();
                        List.Add(item);
                    }
                    RspnCode = "200";
                    RspnMsg = "success";
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public void update()
        {
            RspnCode = "500";
            List.ForEach(e =>
            {
                using (SqlConnection conn = new SqlConnection(checkToken.setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        update 
	                        storeSet set [enable]=@enable,[value]=@value,[text]=@text
                        where
	                        [key]=@key and (
		                        job_id in(
			                        select webjobs.job_id from webjobs
			                        left join authors on webjobs.job_id = authors.job_id
			                        where authors.canexe='Y' and webjobs.canexe='Y' and authors.empl_id=@mem_id
		                        )
	                        )
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@key", e.key));
                    cmd.Parameters.Add(new SqlParameter("@value", e.value));
                    cmd.Parameters.Add(new SqlParameter("@text", e.text));
                    cmd.Parameters.Add(new SqlParameter("@enable", e.enable));
                    cmd.Parameters.Add(new SqlParameter("@mem_id", checkToken.token.id));
                    try
                    {
                        cmd.ExecuteReader();
                        RspnCode = "200";
                        RspnMsg = "success";
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            });
        }
    }
}