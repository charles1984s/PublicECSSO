using ECSSO.Library;
using ECSSO.Library.Component;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.api.Component
{
    /// <summary>
    /// ObjectItemHandler 的摘要描述
    /// </summary>
    public class ObjectItemHandler : IHttpHandler
    {
        private CheckToken checkToken;
        private ObjectItems objectItems;
        public void ProcessRequest(HttpContext context)
        {
            checkToken = new CheckToken();
            objectItems = new ObjectItems();
            try {
                checkToken.check(context);
                objectItems = new ObjectItems { 
                    RspnCode = checkToken.response.RspnCode,
                    RspnMsg = checkToken.response.RspnMsg
                };
                if (checkToken.response.RspnCode == "200") {
                    switch (context.Request.Form["type"]) {
                        case "Items":
                            objectItems.RspnCode = "500.1";
                            setObectItems();
                            break;
                        default:
                            objectItems.RspnCode = "404";
                            throw new Exception("查無操作");
                    }
                }else throw new Exception("Token已過期");
            }
            catch (Exception ex)
            {
                objectItems.RspnMsg = ex.Message;
            }
            finally
            {
                context.Response.Write(objectItems.printMsg());
            }
        }
        private void setObectItems() {
            objectItems.RspnCode = "500.2";
            using (SqlConnection conn = new SqlConnection(checkToken.setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select w.job_id,w.job_name from webjobs as w
                    inner join authors as a on a.job_id = w.job_id
                    where w.job_id like 'O%' and w.canexe='Y' and a.canexe='Y' and a.empl_id=@empl_id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@empl_id", checkToken.token.id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    objectItems.RspnCode = "500.3";
                    objectItems.List = new List<ObjectItem>();
                    while (reader.Read())
                    {
                        objectItems.RspnCode = "500.4";
                        ObjectItem objectItem = new ObjectItem
                        {
                            ID = int.Parse(reader["job_id"].ToString().Replace("O", "")),
                            Title = reader["job_name"].ToString()
                        };
                        switch (objectItem.ID) {
                            case 1:
                                objectItem.ico = "glyphicon glyphicon-font";
                                break;
                            case 2:
                                objectItem.ico = "glyphicon glyphicon-file";
                                break;
                            case 3:
                                objectItem.ico = "glyphicon glyphicon-picture";
                                break;
                            case 4:
                                objectItem.ico = "glyphicon glyphicon-map-marker";
                                break;
                            case 5:
                                objectItem.ico = "glyphicon glyphicon-resize-full";
                                break;
                            case 6:
                                objectItem.ico = "glyphicon fa fa-youtube";
                                break;
                            case 7:
                                objectItem.ico = "glyphicon fa fa-ticket";
                                break;
                            case 10:
                                objectItem.ico = "glyphicon fa fa-gift";
                                break;
                            case 11:
                                objectItem.ico = "fa fa-recycle";
                                break;
                        }
                        objectItems.List.Add(objectItem);
                    }
                    objectItems.RspnCode = "200";
                    objectItems.RspnMsg = "查詢完成";
                }
                catch {
                    throw new Exception("系統錯誤");
                }
                finally
                {
                    if (reader != null) reader.Close();
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