using ECSSO.Extension;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.SearchClass
{
    public class SearchClassDetailDto : responseJson
    {
        private CheckToken token { get; set; }
        public int id { get; set; }
        public int type { get; set; }
        public List<CustSearchBind> list { get; set; }
        public SearchClassDetailDto(CheckToken token, int id, int type)
        {
            this.token = token;
            this.id = id;
            this.type = type;
        }
        public void getAllTag()
        {
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select id,title
                    from tag
                    where id not in(select bind from custSearchBind where sType=@sType and searchId=22)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@sType", type));
                cmd.Parameters.Add(new SqlParameter("@searchId", id));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    list = new List<CustSearchBind>();
                    while (reader.Read())
                    {
                        CustSearchBind myClass = new CustSearchBind
                        {
                            bind = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString()
                        };
                        list.Add(myClass);
                    }
                    RspnCode = "200";
                    RspnMsg = "success";
                }
                catch (Exception e)
                {
                    list = null;
                    throw e;
                }
            }
        }
        public void insertTag(SearchClassDetailInputOfInsertDto dto)
        {
            if (dto.list == null || dto.list.Count == 0) throw new Exception("資料不存在。");
            id = dto.searchId;
            type = 2;
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    insert into custSearchBind(bind,sType,searchId)
                    select id,@sType,@searchId from @list
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@sType", type));
                cmd.Parameters.Add(new SqlParameter("@searchId", dto.searchId));
                cmd.AddParameter("@list", dto.list);
                try
                {
                    cmd.ExecuteReader();
                    setSearchBindTag();
                    RspnCode = "200";
                    RspnMsg = "success";
                }
                catch (Exception e)
                {
                    list = null;
                    throw e;
                }
            }
        }
        public void insertMenu(CustSearchBindDto dto)
        {
            if (dto.list == null || dto.list.Count == 0) throw new Exception("資料不存在。");
            type = 1;
            List<int> setList = new List<int>();
            List<int> nonsetList = new List<int>();
            token.response.RspnCode = "500.1";
            dto.list.ForEach(e =>
            {
                token.response.RspnCode = "500.1.1";
                if (e.check) setSumMenuList(setList, e);
                else setSumMenuList(nonsetList, e);
                token.response.RspnCode = "500.1.2";
            });
            token.response.RspnCode = "500.2";
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    insert into custSearchBind(bind,sType,searchId)
                    select id,@sType,@searchId 
                    from @list 
                    where id not in(select bind from custSearchBind where searchId=@searchId and sType=@sType)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@sType", type));
                cmd.Parameters.Add(new SqlParameter("@searchId", id));
                cmd.AddParameter("@list", setList);
                try
                {
                    token.response.RspnCode = "500.3";
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    list = null;
                    throw e;
                }
            }
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    delete custSearchBind
                    where 
                        bind in(select id from @list) and 
                        sType=@sType and 
                        searchId=@searchId
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@sType", type));
                cmd.Parameters.Add(new SqlParameter("@searchId", id));
                cmd.AddParameter("@list", nonsetList);
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    list = null;
                    throw e;
                }
            }

            setSearchBindMenu();
            RspnCode = "200";
            RspnMsg = "success";
        }
        public void setSumMenuList(List<int> list, CustSearchBindCheckItem item)
        {
            if (!list.Contains(item.bind)) list.Add(item.bind);
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from menu_sub where authors_id=@auid
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@auid", item.bind));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        setSumMenuList(list, new CustSearchBindCheckItem { bind = int.Parse(reader["id"].ToString()) });
                    }
                }
                catch (Exception e)
                {
                    list = null;
                    throw e;
                }
            }
        }
        public void Delete(CustSearchBind detail)
        {
            if (detail == null) throw new Exception("資料不存在");
            using (SqlConnection conn = new SqlConnection(token.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    delete custSearchBind where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", detail.id));
                try
                {
                    cmd.ExecuteReader();
                    token.response.RspnCode = "200";
                    token.response.RspnMsg = "success";
                }
                catch (Exception e)
                {
                    list = null;
                    throw e;
                }
            }
        }
        private void setSearchBindMenu()
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
                    list = new List<CustSearchBind>();
                    while (reader.Read())
                    {
                        CustSearchBind menuBind = new CustSearchBind
                        {
                            id = int.Parse(reader["id"].ToString()),
                            searchId = int.Parse(reader["searchId"].ToString()),
                            bind = int.Parse(reader["bind"].ToString()),
                            title = reader["title"].ToString()
                        };
                        list.Add(menuBind);
                    }
                }
                catch (Exception e)
                {
                    list = null;
                    throw e;
                }
            }
        }
        private void setSearchBindTag()
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
                    list = new List<CustSearchBind>();
                    while (reader.Read())
                    {
                        CustSearchBind menuBind = new CustSearchBind
                        {
                            id = int.Parse(reader["id"].ToString()),
                            searchId = int.Parse(reader["searchId"].ToString()),
                            bind = int.Parse(reader["bind"].ToString()),
                            title = reader["title"].ToString()
                        };
                        list.Add(menuBind);
                    }
                }
                catch (Exception e)
                {
                    list = null;
                    throw e;
                }
            }
        }
    }
}