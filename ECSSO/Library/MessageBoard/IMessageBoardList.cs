using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ECSSO.Library.MessageBoard
{
    public class IMessageBoardList
    {
        List<IMessageBoard> list { get; set; }
        public IMessageBoardList(string setting) {
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select title,ch_name,cont,re_cont,cdate,edate from [message] where [type]=''", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try {
                    list = new List<IMessageBoard>();
                    while (reader.Read())
                    {
                        list.Add(new IMessageBoard {
                            title = reader["title"].ToString(),
                            name = reader["ch_name"].ToString(),
                            question = reader["cont"].ToString(),
                            Reply = reader["re_cont"].ToString(),
                        });
                    }
                }
                catch (Exception e) {
                    throw new Exception("資料抓取失敗");
                }
            }
        }
        public StringBuilder getHtmlString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>無標題文件</title></head><body>");
            list.ForEach(e => {
                sb.Append($@"
                    <h3>{e.title}</h3>
                    <h4>{e.name}</h4>
                    <p>{e.question}</p>
                    <p>{e.Reply}</p><br /><br />
                ");
            });
            sb.Append("</body></html>");
            return sb;
        }
    }
}