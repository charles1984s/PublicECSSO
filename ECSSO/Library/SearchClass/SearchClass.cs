using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.SearchClass
{
    public class SearchClass
    {
        public int id { get; set; }
        public string title { get; set; }
        public string placeholder { get; set; }
        public bool searchAllmenu { get; set; }
        public bool searchAlltag { get; set; }
        public List<CustSearchBind> menuSet { get; set; }
        public List<CustSearchBind> tagSet { get; set; }
        private CheckToken token;
        public void CreateOrEdit(CheckToken token)
        {
            this.token = token;
            if (id == 0) Create();
            else Edit();
            token.response.RspnCode = "200";
            token.response.RspnMsg = "success";
        }
        public void Delete(CheckToken token)
        {
            if (id == 0) throw new Exception("資料不存在");
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    delete custSearch where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                try
                {
                    cmd.ExecuteReader();
                    token.response.RspnCode = "200";
                    token.response.RspnMsg = "success";
                }
                catch (Exception e)
                {
                    menuSet = null;
                    throw e;
                }
            }
        }
        private void Create()
        {
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    insert into custSearch(title,placeholder,ser_no,searchAllmenu,searchAlltag)
                    OUTPUT INSERTED.id
                    values(@title,500,@searchAllmenu,@searchAlltag)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@title", title));
                cmd.Parameters.Add(new SqlParameter("@placeholder", placeholder));
                cmd.Parameters.Add(new SqlParameter("@searchAllmenu", searchAllmenu ? 1 : 0));
                cmd.Parameters.Add(new SqlParameter("@searchAlltag", searchAlltag ? 1 : 0));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        id = int.Parse(reader["id"].ToString());
                    }
                }
                catch (Exception e)
                {
                    menuSet = null;
                    throw e;
                }
            }
        }
        private void Edit()
        {
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update custSearch set title=@title,placeholder=@placeholder,searchAllmenu=@searchAllmenu,searchAlltag=@searchAlltag where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@title", title));
                cmd.Parameters.Add(new SqlParameter("@placeholder", placeholder));
                cmd.Parameters.Add(new SqlParameter("@searchAllmenu", searchAllmenu));
                cmd.Parameters.Add(new SqlParameter("@searchAlltag", searchAlltag));
                cmd.Parameters.Add(new SqlParameter("@id", id));
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    menuSet = null;
                    throw e;
                }
            }
        }

        public void setSearchBind(CheckToken token)
        {
            setSearchBindMenu(token);
            setSearchBindTag(token);
        }
        private void setSearchBindMenu(CheckToken token)
        {
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select b.id,b.bind,b.searchId,menu_sub.title from menu_sub
                    left join custSearchBind as b on menu_sub.id =b.bind
                    where sType=1 and searchId=@searchId
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@searchId", id));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    menuSet = new List<CustSearchBind>();
                    while (reader.Read())
                    {
                        CustSearchBind menuBind = new CustSearchBind
                        {
                            id = int.Parse(reader["id"].ToString()),
                            searchId = int.Parse(reader["searchId"].ToString()),
                            bind = int.Parse(reader["bind"].ToString()),
                            title = reader["title"].ToString()
                        };
                        menuSet.Add(menuBind);
                    }
                }
                catch (Exception e)
                {
                    menuSet = null;
                    throw e;
                }
            }
        }
        private void setSearchBindTag(CheckToken token)
        {
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select b.id,b.bind,b.searchId,tag.title from tag
                    left join custSearchBind as b on tag.id =b.bind
                    where sType=2 and searchId=@searchId
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@searchId", id));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    tagSet = new List<CustSearchBind>();
                    while (reader.Read())
                    {
                        CustSearchBind menuBind = new CustSearchBind
                        {
                            id = int.Parse(reader["id"].ToString()),
                            searchId = int.Parse(reader["searchId"].ToString()),
                            bind = int.Parse(reader["bind"].ToString()),
                            title = reader["title"].ToString()
                        };
                        tagSet.Add(menuBind);
                    }
                }
                catch (Exception e)
                {
                    tagSet = null;
                    throw e;
                }
            }
        }
    }
}