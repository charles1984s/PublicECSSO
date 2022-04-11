using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.SearchClass
{
    public class CustSearchDto : responseJson
    {
        public List<SearchClass> classes { get; set; }
        public SearchClass theClass { get; set; }
        public void GetAll(CheckToken token)
        {
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from custSearch
                ", conn);
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    classes = new List<SearchClass>();
                    while (reader.Read())
                    {
                        SearchClass myClass = new SearchClass
                        {
                            id = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString(),
                            placeholder = reader["placeholder"].ToString(),
                            searchAllmenu = reader["searchAllmenu"].ToString() == "1",
                            searchAlltag = reader["searchAlltag"].ToString() == "1",
                        };
                        myClass.setSearchBind(token);
                        classes.Add(myClass);
                    }
                    RspnCode = "200";
                    RspnMsg = "success";
                }
                catch (Exception e)
                {
                    classes = null;
                    throw e;
                }
            }
        }
    }
}