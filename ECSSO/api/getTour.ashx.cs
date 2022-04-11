using System;
using System.Collections.Generic;
using System.Web;
using ECSSO.Library;
using ECSSO;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ECSSO.api
{
    /// <summary>
    /// getTour 的摘要描述
    /// </summary>
    public class getTour : IHttpHandler
    {
        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            GetStr GS = new GetStr();

            int statement = 0;
            string returnMsg = "Something has wrong!!";
            try
            {
                if (context.Request.Params["type"] == null || context.Request.Params["type"].ToString() == "") statement = 1;
                if (context.Request.Params["SiteID"] == null || context.Request.Params["SiteID"].ToString() == "") statement = 2;

                switch (statement)
                {
                    case 0:
                        {
                            String Type = context.Request.Params["type"].ToString();
                            String SiteID = context.Request.Params["SiteID"].ToString();
                            String Id = (context.Request.Params["id"] == null) ? "" : context.Request.Params["id"].ToString();
                            String download = (context.Request.Params["download"] == null) ? "" : context.Request.Params["download"].ToString();
                            String platform = (context.Request.Params["platform"] == null) ? "" : context.Request.Params["platform"].ToString();

                            if (GS.GetSettingForChecked(SiteID) == "")
                            {
                                returnMsg = ErrorMsg("error", "請檢查SiteID是否正確", "");
                            }
                            else
                            {
                                XmlHeader xmlHeader = new XmlHeader();
                                List<object> dataList = new List<object>();

                                System.Xml.Linq.XDocument xdoc = new System.Xml.Linq.XDocument();

                                xmlHeader = getXmlHeader(SiteID, Type);
                                dataList = formatDataStringId(getDataString(SiteID, Type), Type, xmlHeader, Id);

                                xdoc = XMLHelper.CreateXDocument(xmlHeader, dataList, platform);
                                if (download.ToLower() == "true")
                                {
                                    string strContentDisposition = String.Format("{0}; filename=\"{1}\"", "attachment", HttpUtility.UrlEncode(getFileName(Type)));
                                    context.Response.AddHeader("Content-Disposition", strContentDisposition);
                                }
                                context.Response.ContentType = "text/xml";
                                returnMsg = ((xdoc == null) ? "null" : xdoc.Declaration.ToString() + Environment.NewLine + xdoc);
                            }
                            break;
                        }
                    case 1:
                        {
                            returnMsg = ErrorMsg("error", "Type必填", "");
                            break;
                        }
                    case 2:
                        {
                            returnMsg = ErrorMsg("error", "SiteID必填", "");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                returnMsg = ErrorMsg("error", ex.ToString(), "");
            }
            context.Response.Write(returnMsg);
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "getTour error", "", RspnMsg);
            }

            ContextErrorMessager root = new ContextErrorMessager();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion
        #region insert log
        private void InsertLog(String Setting, String JobName, String JobTitle, String Detail)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_userlogAdd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", "guest"));
                cmd.Parameters.Add(new SqlParameter("@prog_name", "Tour"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " getTour.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion
        #region Get IP
        private string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string sIPAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(sIPAddress))
            {
                return context.Request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                string[] ipArray = sIPAddress.Split(new Char[] { ',' });
                return ipArray[0];
            }
        }
        #endregion

        private XmlHeader getXmlHeader(string siteID, string type)
        {
            GetStr GS = new GetStr();
            String strSqlConnection = GS.GetSettingForChecked(siteID);

            XmlHeader xmlHeader = new XmlHeader();
            switch (type)
            {
                case "node":
                    {
                        xmlHeader.Listname = "1";
                        break;
                    }
                case "event":
                    {
                        xmlHeader.Listname = "2";
                        break;
                    }
                case "shop":
                    {
                        xmlHeader.Listname = "3";
                        break;
                    }
                case "hotel":
                    {
                        xmlHeader.Listname = "4";
                        break;
                    }
                case "all":
                    {
                        xmlHeader.Listname = "0";
                        break;
                    }
            }
            xmlHeader.Language = ((GS.GetLanString(GS.GetFullLanString(siteID)).ToUpper() == "EN") ? "E" : "C");
            using (SqlConnection conn = new SqlConnection(strSqlConnection))
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(getCmdStringForUpdatetime(type), conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    xmlHeader.Updatetime = Convert.ToDateTime(reader[0]).ToString("yyyy/MM/dd HH:mm:ss");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    xmlHeader.Updatetime = (type == "all") ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : ex.ToString();
                }
                try
                {
                    using (SqlCommand cmd = new SqlCommand(getCmdStringForOrgName(type), conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    xmlHeader.Orgname = (reader[0] is DBNull) ? "" : reader[0].ToString();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    xmlHeader.Orgname = (type == "all") ? "0000000000" : ex.ToString();
                }
            }
            return xmlHeader;
        }
        private string getCmdString(string type, string lng)
        {
            string cmdString = "";
            switch (type)
            {
                case "node":
                    {
                        cmdString = "SELECT * FROM attractions tName inner join menu on tName.menuID = menu.id ";
                        cmdString += "Where tName.Id is not null and tName.Id not like '' ";
                        cmdString += "and tName.Name is not null and tName.Name not like '' ";
                        cmdString += "and tName.Toldescribe is not null and tName.Toldescribe not like '' ";
                        cmdString += "and tName.Tel is not null and tName.Tel not like '' ";
                        cmdString += "and tName.[Add] is not null and tName.[Add] not like '' ";
                        cmdString += "and tName.Opentime is not null and tName.Opentime not like '' ";
                        cmdString += "and tName.Gov is not null and tName.Gov not like '' ";
                        cmdString += "and tName.Px is not null and tName.Px not like '' ";
                        cmdString += "and tName.Py is not null and tName.Py not like '' ";
                        cmdString += "and tName.Class1 is not null and tName.Class1 not like '' ";
                        cmdString += "and tName.Changetime is not null and tName.Changetime not like '' ";
                        cmdString += (lng == "en") ? "and tName.Name_ch is not null and tName.Name_ch not like '' " : "";
                        cmdString += "and menu.disp_opt='Y' ";
                        cmdString += "and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        cmdString += ";";
                        break;
                    }
                case "event":
                    {
                        cmdString = "SELECT * FROM active tName inner join menu on tName.menuID = menu.id ";
                        cmdString += "Where tName.Id is not null and tName.Id not like '' ";
                        cmdString += "and tName.Name is not null and tName.Name not like '' ";
                        cmdString += "and tName.Toldescribe is not null and tName.Toldescribe not like '' ";
                        cmdString += "and tName.Description is not null and tName.Description not like '' ";
                        cmdString += "and tName.Tel is not null and tName.Tel not like '' ";
                        cmdString += "and tName.[Add] is not null and tName.[Add] not like '' ";
                        cmdString += "and tName.[Start] is not null and tName.[Start] not like '' ";
                        cmdString += "and tName.[End] is not null and tName.[End] not like '' ";
                        cmdString += "and tName.Org is not null and tName.Org not like '' ";
                        cmdString += "and tName.Px is not null and tName.Px not like '' ";
                        cmdString += "and tName.Py is not null and tName.Py not like '' ";
                        cmdString += "and tName.Class1 is not null and tName.Class1 not like '' ";
                        cmdString += "and tName.Changetime is not null and tName.Changetime not like '' ";
                        cmdString += (lng == "en") ? "and tName.Name_ch is not null and tName.Name_ch not like '' " : "";
                        cmdString += "and menu.disp_opt='Y' ";
                        cmdString += "and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        cmdString += ";";
                        break;
                    }
                case "shop":
                    {
                        cmdString = "SELECT * FROM shop tName inner join menu on tName.menuID = menu.id ";
                        cmdString += "Where tName.Id is not null and tName.Id not like '' ";
                        cmdString += "and tName.Name is not null and tName.Name not like '' ";
                        cmdString += "and tName.Description is not null and tName.Description not like '' ";
                        cmdString += "and tName.Tel is not null and tName.Tel not like '' ";
                        cmdString += "and tName.[Add] is not null and tName.[Add] not like '' ";
                        cmdString += "and tName.Opentime is not null and tName.Opentime not like '' ";
                        cmdString += "and tName.Px is not null and tName.Px not like '' ";
                        cmdString += "and tName.Py is not null and tName.Py not like '' ";
                        cmdString += "and tName.Class1 is not null and tName.Class1 not like '' ";
                        cmdString += "and tName.Changetime is not null and tName.Changetime not like '' ";
                        cmdString += (lng == "en") ? "and tName.Name_ch is not null and tName.Name_ch not like '' " : "";
                        cmdString += "and menu.disp_opt='Y' ";
                        cmdString += "and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        cmdString += ";";
                        break;
                    }
                case "hotel":
                    {
                        cmdString = "SELECT * FROM hotel tName inner join menu on tName.menuID = menu.id ";
                        cmdString += "Where tName.Id is not null and tName.Id not like '' ";
                        cmdString += "and tName.Name is not null and tName.Name not like '' ";
                        cmdString += "and tName.Fax is not null and tName.Fax not like '' ";
                        cmdString += "and tName.Description is not null and tName.Description not like '' ";
                        cmdString += "and tName.Tel is not null and tName.Tel not like '' ";
                        cmdString += "and tName.[Add] is not null and tName.[Add] not like '' ";
                        cmdString += "and tName.Gov is not null and tName.Gov not like '' ";
                        cmdString += "and tName.Px is not null and tName.Px not like '' ";
                        cmdString += "and tName.Py is not null and tName.Py not like '' ";
                        cmdString += "and tName.Class1 is not null and tName.Class1 not like '' ";
                        cmdString += "and tName.Changetime is not null and tName.Changetime not like '' ";
                        cmdString += (lng == "en") ? "and tName.Name_ch is not null and tName.Name_ch not like '' " : "";
                        cmdString += "and menu.disp_opt='Y' ";
                        cmdString += "and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        cmdString += ";";
                        break;
                    }
            }
            return cmdString;
        }
        private string getCmdStringForOrgName(string type)
        {
            string cmdString = "";
            switch (type)
            {
                case "node":
                    {
                        cmdString = "SELECT top 1 OrgName FROM attractions tName inner join menu on tName.menuID = menu.id where menu.disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        break;
                    }
                case "event":
                    {
                        cmdString = "SELECT top 1 OrgName FROM active tName inner join menu on tName.menuID = menu.id where menu.disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        break;
                    }
                case "shop":
                    {
                        cmdString = "SELECT top 1 OrgName FROM shop tName inner join menu on tName.menuID = menu.id where menu.disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        break;
                    }
                case "hotel":
                    {
                        cmdString = "SELECT top 1 OrgName FROM hotel tName inner join menu on tName.menuID = menu.id where menu.disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        break;
                    }
            }
            return cmdString;
        }
        private string getCmdStringForUpdatetime(string type)
        {
            string cmdString = "";
            switch (type)
            {
                case "node":
                    {
                        cmdString = "SELECT top 1 edate FROM attractions tName inner join menu on tName.menuID = menu.id Where menu.disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        break;
                    }
                case "event":
                    {
                        cmdString = "SELECT top 1 edate FROM active tName inner join menu on tName.menuID = menu.id Where menu.disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        break;
                    }
                case "shop":
                    {
                        cmdString = "SELECT top 1 edate FROM shop tName inner join menu on tName.menuID = menu.id Where menu.disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        break;
                    }
                case "hotel":
                    {
                        cmdString = "SELECT top 1 edate FROM hotel tName inner join menu on tName.menuID = menu.id Where menu.disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                        break;
                    }
            }
            return cmdString;
        }
        private string getFileName(string type)
        {
            string FileName = "";
            switch (type)
            {
                case "node":
                    {
                        FileName = "node.xml";
                        break;
                    }
                case "event":
                    {
                        FileName = "event.xml";
                        break;
                    }
                case "shop":
                    {
                        FileName = "shop.xml";
                        break;
                    }
                case "hotel":
                    {
                        FileName = "hotel.xml";
                        break;
                    }
            }
            return FileName;
        }
        public List<object> getDataString(string SiteID, string Type)
        {
            GetStr GS = new GetStr();
            String Lng = GS.GetFullLanString(SiteID);
            String strSqlConnection = GS.GetSettingForChecked(SiteID);
            String orgName = GS.GetOrgName(strSqlConnection);

            List<object> dataList = new List<object>();

            using (SqlConnection conn = new SqlConnection(strSqlConnection))
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(getCmdString(Type, GS.GetLanString(Lng)), conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    object item;
                                    string pic1 = (reader["Picture1"] is DBNull) ? "" : (GS.GetAllLinkString(orgName, reader["Picture1"].ToString(), Lng, "Image"));
                                    string pic2 = (reader["Picture2"] is DBNull) ? "" : (GS.GetAllLinkString(orgName, reader["Picture2"].ToString(), Lng, "Image"));
                                    string pic3 = (reader["Picture3"] is DBNull) ? "" : (GS.GetAllLinkString(orgName, reader["Picture3"].ToString(), Lng, "Image"));
                                    string telNumber = (reader["Tel"] is DBNull) ? "" : (GS.InternationalPrefix(reader["Tel"].ToString()));
                                    double Px = (reader["Px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Px"].ToString());
                                    double Py = (reader["Py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Py"].ToString());
                                    double Pix = (reader["Parkinginfo_px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Parkinginfo_px"].ToString());
                                    double Piy = (reader["Parkinginfo_py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Parkinginfo_py"].ToString());
                                    
                                    if (Type == "node")
                                    {
                                        item = (GS.GetLanString(Lng) == "en") ?
                                            new XMLNodeEn()
                                            {
                                                Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                                Name = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                Name_c = (reader["Name_ch"] is DBNull) ? "" : reader["Name_ch"].ToString(),
                                                Zone = (reader["Zone"] is DBNull) ? "" : reader["Zone"].ToString(),
                                                Toldescribe = (reader["Toldescribe"] is DBNull) ? "" : reader["Toldescribe"].ToString(),
                                                Description = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                Tel = telNumber,
                                                Add = (reader["Add"] is DBNull) ? "" : reader["Add"].ToString(),
                                                Zipcode = (reader["Zipcode"] is DBNull) ? "" : reader["Zipcode"].ToString(),
                                                Travellinginfo = (reader["Travellinginfo"] is DBNull) ? "" : reader["Travellinginfo"].ToString(),
                                                Opentime = (reader["Opentime"] is DBNull) ? "" : reader["Opentime"].ToString(),
                                                Picture1 = pic1,
                                                Picdescribe1 = (reader["Picdescribe1"] is DBNull) ? "" : reader["Picdescribe1"].ToString(),
                                                Picture2 = pic2,
                                                Picdescribe2 = (reader["Picdescribe2"] is DBNull) ? "" : reader["Picdescribe2"].ToString(),
                                                Picture3 = pic3,
                                                Picdescribe3 = (reader["Picdescribe3"] is DBNull) ? "" : reader["Picdescribe3"].ToString(),
                                                Map = (reader["Map"] is DBNull) ? "" : reader["Map"].ToString(),
                                                Gov = (reader["Gov"] is DBNull) ? "" : reader["Gov"].ToString(),
                                                Px = (Px > Py ? Px : Py),
                                                Py = (Px > Py ? Py : Px),
                                                Orgclass = (reader["Orgclass"] is DBNull) ? "" : reader["Orgclass"].ToString(),
                                                Class1 = (reader["Class1"] is DBNull) ? "" : reader["Class1"].ToString(),
                                                Class2 = (reader["Class2"] is DBNull) ? "" : reader["Class2"].ToString(),
                                                Class3 = (reader["Class3"] is DBNull) ? "" : reader["Class3"].ToString(),
                                                Level = (reader["Level"] is DBNull) ? "" : reader["Level"].ToString(),
                                                Website = (reader["Website"] is DBNull) ? "" : (reader["Website"].ToString()),
                                                Parkinginfo = (reader["Parkinginfo"] is DBNull) ? "" : reader["Parkinginfo"].ToString(),
                                                Parkinginfo_px = (Pix > Piy ? Pix : Piy),
                                                Parkinginfo_py = (Pix > Piy ? Piy : Pix),
                                                Ticketinfo = (reader["Ticketinfo"] is DBNull) ? "" : reader["Ticketinfo"].ToString(),
                                                Remarks = (reader["Remarks"] is DBNull) ? "" : reader["Remarks"].ToString(),
                                                Keyword = (reader["Keyword"] is DBNull) ? "" : reader["Keyword"].ToString(),
                                                Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy/MM/dd HH:mm:ss")
                                            }
                                          : new XMLNode()
                                          {
                                              Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                              Name = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                              Zone = (reader["Zone"] is DBNull) ? "" : reader["Zone"].ToString(),
                                              Toldescribe = (reader["Toldescribe"] is DBNull) ? "" : reader["Toldescribe"].ToString(),
                                              Description = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                              Tel = telNumber,
                                              Add = (reader["Add"] is DBNull) ? "" : reader["Add"].ToString(),
                                              Zipcode = (reader["Zipcode"] is DBNull) ? "" : reader["Zipcode"].ToString(),
                                              Travellinginfo = (reader["Travellinginfo"] is DBNull) ? "" : reader["Travellinginfo"].ToString(),
                                              Opentime = (reader["Opentime"] is DBNull) ? "" : reader["Opentime"].ToString(),
                                              Picture1 = pic1,
                                              Picdescribe1 = (reader["Picdescribe1"] is DBNull) ? "" : reader["Picdescribe1"].ToString(),
                                              Picture2 = pic2,
                                              Picdescribe2 = (reader["Picdescribe2"] is DBNull) ? "" : reader["Picdescribe2"].ToString(),
                                              Picture3 = pic3,
                                              Picdescribe3 = (reader["Picdescribe3"] is DBNull) ? "" : reader["Picdescribe3"].ToString(),
                                              Map = (reader["Map"] is DBNull) ? "" : reader["Map"].ToString(),
                                              Gov = (reader["Gov"] is DBNull) ? "" : reader["Gov"].ToString(),
                                              Px = (Px > Py ? Px : Py),
                                              Py = (Px > Py ? Py : Px),
                                              Orgclass = (reader["Orgclass"] is DBNull) ? "" : reader["Orgclass"].ToString(),
                                              Class1 = (reader["Class1"] is DBNull) ? "" : reader["Class1"].ToString(),
                                              Class2 = (reader["Class2"] is DBNull) ? "" : reader["Class2"].ToString(),
                                              Class3 = (reader["Class3"] is DBNull) ? "" : reader["Class3"].ToString(),
                                              Level = (reader["Level"] is DBNull) ? "" : reader["Level"].ToString(),
                                              Website = (reader["Website"] is DBNull) ? "" : (reader["Website"].ToString()),
                                              Parkinginfo = (reader["Parkinginfo"] is DBNull) ? "" : reader["Parkinginfo"].ToString(),
                                              Parkinginfo_px = (Pix > Piy ? Pix : Piy),
                                              Parkinginfo_py = (Pix > Piy ? Piy : Pix),
                                              Ticketinfo = (reader["Ticketinfo"] is DBNull) ? "" : reader["Ticketinfo"].ToString(),
                                              Remarks = (reader["Remarks"] is DBNull) ? "" : reader["Remarks"].ToString(),
                                              Keyword = (reader["Keyword"] is DBNull) ? "" : reader["Keyword"].ToString(),
                                              Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy/MM/dd HH:mm:ss")
                                          };
                                    }
                                    else if (Type == "event")
                                    {
                                        item = (GS.GetLanString(Lng) == "en") ?
                                            new XMLEventEn()
                                            {
                                                Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                                Org = (reader["Org"] is DBNull) ? "" : reader["Org"].ToString(),
                                                //Co_Organiser = (reader["Co_Organiser"] is DBNull) ? "" : reader["Co_Organiser"].ToString(),
                                                //GovID = (reader["GovID"] is DBNull) ? "" : reader["GovID"].ToString(),
                                                Name = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                Name_c = (reader["Name_ch"] is DBNull) ? "" : reader["Name_ch"].ToString(),
                                                Description = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                Tel = telNumber,
                                                Location = (reader["Location"] is DBNull) ? "" : reader["Location"].ToString(),
                                                Add = (reader["Add"] is DBNull) ? "" : reader["Add"].ToString(),
                                                Particpation = (reader["Particpation"] is DBNull) ? "" : reader["Particpation"].ToString(),
                                                Cycle = (reader["Cycle"] is DBNull) ? "" : reader["Cycle"].ToString(),
                                                Noncycle = (reader["NonCycle"] is DBNull) ? "" : reader["NonCycle"].ToString(),
                                                Start = (reader["Start"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["Start"]).ToString("yyyy/MM/dd HH:mm:ss"),
                                                End = (reader["End"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["End"]).ToString("yyyy/MM/dd HH:mm:ss"),
                                                Travellinginfo = (reader["Travellinginfo"] is DBNull) ? "" : reader["Travellinginfo"].ToString(),
                                                Charge = (reader["Charge"] is DBNull) ? "" : reader["Charge"].ToString(),
                                                Picture1 = pic1,
                                                Picdescribe1 = (reader["Picdescribe1"] is DBNull) ? "" : reader["Picdescribe1"].ToString(),
                                                Picture2 = pic2,
                                                Picdescribe2 = (reader["Picdescribe2"] is DBNull) ? "" : reader["Picdescribe2"].ToString(),
                                                Picture3 = pic3,
                                                Picdescribe3 = (reader["Picdescribe3"] is DBNull) ? "" : reader["Picdescribe3"].ToString(),
                                                Map = (reader["Map"] is DBNull) ? "" : reader["Map"].ToString(),
                                                Px = (Px > Py ? Px : Py),
                                                Py = (Px > Py ? Py : Px),
                                                Website = (reader["Website"] is DBNull) ? "" : (reader["Website"].ToString()),
                                                Parkinginfo = (reader["Parkinginfo"] is DBNull) ? "" : reader["Parkinginfo"].ToString(),
                                                Parkinginfo_px = (Pix > Piy ? Pix : Piy),
                                                Parkinginfo_py = (Pix > Piy ? Piy : Pix),
                                                Class1 = (reader["Class1"] is DBNull) ? "" : reader["Class1"].ToString(),
                                                Class2 = (reader["Class2"] is DBNull) ? "" : reader["Class2"].ToString(),
                                                Remarks = (reader["Remarks"] is DBNull) ? "" : reader["Remarks"].ToString(),
                                                Keyword = (reader["Keyword"] is DBNull) ? "" : reader["Keyword"].ToString(),
                                                Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy/MM/dd HH:mm:ss")
                                            }
                                          : new XMLEvent()
                                          {
                                              Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                              Org = (reader["Org"] is DBNull) ? "" : reader["Org"].ToString(),
                                              //Co_Organiser = (reader["Co_Organiser"] is DBNull) ? "" : reader["Co_Organiser"].ToString(),
                                              //GovID = (reader["GovID"] is DBNull) ? "" : reader["GovID"].ToString(),
                                              Name = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                              Description = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                              Tel = telNumber,
                                              Location = (reader["Location"] is DBNull) ? "" : reader["Location"].ToString(),
                                              Add = (reader["Add"] is DBNull) ? "" : reader["Add"].ToString(),
                                              Particpation = (reader["Particpation"] is DBNull) ? "" : reader["Particpation"].ToString(),
                                              Cycle = (reader["Cycle"] is DBNull) ? "" : reader["Cycle"].ToString(),
                                              Noncycle = (reader["NonCycle"] is DBNull) ? "" : reader["NonCycle"].ToString(),
                                              Start = (reader["Start"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["Start"]).ToString("yyyy/MM/dd HH:mm:ss"),
                                              End = (reader["End"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["End"]).ToString("yyyy/MM/dd HH:mm:ss"),
                                              Travellinginfo = (reader["Travellinginfo"] is DBNull) ? "" : reader["Travellinginfo"].ToString(),
                                              Charge = (reader["Charge"] is DBNull) ? "" : reader["Charge"].ToString(),
                                              Picture1 = pic1,
                                              Picdescribe1 = (reader["Picdescribe1"] is DBNull) ? "" : reader["Picdescribe1"].ToString(),
                                              Picture2 = pic2,
                                              Picdescribe2 = (reader["Picdescribe2"] is DBNull) ? "" : reader["Picdescribe2"].ToString(),
                                              Picture3 = pic3,
                                              Picdescribe3 = (reader["Picdescribe3"] is DBNull) ? "" : reader["Picdescribe3"].ToString(),
                                              Map = (reader["Map"] is DBNull) ? "" : reader["Map"].ToString(),
                                              Px = (Px > Py ? Px : Py),
                                              Py = (Px > Py ? Py : Px),
                                              Website = (reader["Website"] is DBNull) ? "" : (reader["Website"].ToString()),
                                              Parkinginfo = (reader["Parkinginfo"] is DBNull) ? "" : reader["Parkinginfo"].ToString(),
                                              Parkinginfo_px = (Pix > Piy ? Pix : Piy),
                                              Parkinginfo_py = (Pix > Piy ? Piy : Pix),
                                              Class1 = (reader["Class1"] is DBNull) ? "" : reader["Class1"].ToString(),
                                              Class2 = (reader["Class2"] is DBNull) ? "" : reader["Class2"].ToString(),
                                              Remarks = (reader["Remarks"] is DBNull) ? "" : reader["Remarks"].ToString(),
                                              Keyword = (reader["Keyword"] is DBNull) ? "" : reader["Keyword"].ToString(),
                                              Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy/MM/dd HH:mm:ss")
                                          };
                                    }
                                    else if (Type == "shop")
                                    {
                                        item = (GS.GetLanString(Lng) == "en") ?
                                            new XMLShopEn()
                                            {
                                                Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                                Name = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                Name_c = (reader["Name_ch"] is DBNull) ? "" : reader["Name_ch"].ToString(),
                                                Description = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                Tel = telNumber,
                                                Add = (reader["Add"] is DBNull) ? "" : reader["Add"].ToString(),
                                                Zipcode = (reader["Zipcode"] is DBNull) ? "" : reader["Zipcode"].ToString(),
                                                Opentime = (reader["Opentime"] is DBNull) ? "" : reader["Opentime"].ToString(),
                                                Picture1 = pic1,
                                                Picdescribe1 = (reader["Picdescribe1"] is DBNull) ? "" : reader["Picdescribe1"].ToString(),
                                                Picture2 = pic2,
                                                Picdescribe2 = (reader["Picdescribe2"] is DBNull) ? "" : reader["Picdescribe2"].ToString(),
                                                Picture3 = pic3,
                                                Picdescribe3 = (reader["Picdescribe3"] is DBNull) ? "" : reader["Picdescribe3"].ToString(),
                                                Map = (reader["Map"] is DBNull) ? "" : reader["Map"].ToString(),
                                                Px = (Px > Py ? Px : Py),
                                                Py = (Px > Py ? Py : Px),
                                                Website = (reader["Website"] is DBNull) ? "" : (reader["Website"].ToString()),
                                                Parkinginfo = (reader["Parkinginfo"] is DBNull) ? "" : reader["Parkinginfo"].ToString(),
                                                Parkinginfo_px = (Pix > Piy ? Pix : Piy),
                                                Parkinginfo_py = (Pix > Piy ? Piy : Pix),
                                                Class = (reader["Class1"] is DBNull) ? "" : reader["Class1"].ToString(),
                                                //Remarks = (reader["Remarks"] is DBNull) ? "" : reader["Remarks"].ToString(),
                                                Keyword = (reader["Keyword"] is DBNull) ? "" : reader["Keyword"].ToString(),
                                                Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy/MM/dd HH:mm:ss")
                                            }
                                          : new XMLShop()
                                          {
                                              Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                              Name = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                              Description = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                              Tel = telNumber,
                                              Add = (reader["Add"] is DBNull) ? "" : reader["Add"].ToString(),
                                              Zipcode = (reader["Zipcode"] is DBNull) ? "" : reader["Zipcode"].ToString(),
                                              Opentime = (reader["Opentime"] is DBNull) ? "" : reader["Opentime"].ToString(),
                                              Picture1 = pic1,
                                              Picdescribe1 = (reader["Picdescribe1"] is DBNull) ? "" : reader["Picdescribe1"].ToString(),
                                              Picture2 = pic2,
                                              Picdescribe2 = (reader["Picdescribe2"] is DBNull) ? "" : reader["Picdescribe2"].ToString(),
                                              Picture3 = pic3,
                                              Picdescribe3 = (reader["Picdescribe3"] is DBNull) ? "" : reader["Picdescribe3"].ToString(),
                                              Map = (reader["Map"] is DBNull) ? "" : reader["Map"].ToString(),
                                              Px = (Px > Py ? Px : Py),
                                              Py = (Px > Py ? Py : Px),
                                              Website = (reader["Website"] is DBNull) ? "" : (reader["Website"].ToString()),
                                              Parkinginfo = (reader["Parkinginfo"] is DBNull) ? "" : reader["Parkinginfo"].ToString(),
                                              Parkinginfo_px = (Pix > Piy ? Pix : Piy),
                                              Parkinginfo_py = (Pix > Piy ? Piy : Pix),
                                              Class = (reader["Class1"] is DBNull) ? "" : reader["Class1"].ToString(),
                                              //Remarks = (reader["Remarks"] is DBNull) ? "" : reader["Remarks"].ToString(),
                                              Keyword = (reader["Keyword"] is DBNull) ? "" : reader["Keyword"].ToString(),
                                              Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy/MM/dd HH:mm:ss")
                                          };
                                    }
                                    else if (Type == "hotel")
                                    {
                                        string faxNumber = (reader["Fax"] is DBNull) ? "" : (GS.InternationalPrefix(reader["Fax"].ToString()));
                                        item = (GS.GetLanString(Lng) == "en") ?
                                            new XMLHotelEn()
                                            {
                                                Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                                Name = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                Name_c = (reader["Name_ch"] is DBNull) ? "" : reader["Name_ch"].ToString(),
                                                Grade = (reader["Grade"] is DBNull) ? "" : reader["Grade"].ToString(),
                                                Fax = faxNumber,
                                                Description = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                Tel = telNumber,
                                                Add = (reader["Add"] is DBNull) ? "" : reader["Add"].ToString(),
                                                Zipcode = (reader["Zipcode"] is DBNull) ? "" : reader["Zipcode"].ToString(),
                                                Spec = (reader["Spec"] is DBNull) ? "" : reader["Spec"].ToString(),
                                                Picture1 = pic1,
                                                Picdescribe1 = (reader["Picdescribe1"] is DBNull) ? "" : reader["Picdescribe1"].ToString(),
                                                Picture2 = pic2,
                                                Picdescribe2 = (reader["Picdescribe2"] is DBNull) ? "" : reader["Picdescribe2"].ToString(),
                                                Picture3 = pic3,
                                                Picdescribe3 = (reader["Picdescribe3"] is DBNull) ? "" : reader["Picdescribe3"].ToString(),
                                                Map = (reader["Map"] is DBNull) ? "" : reader["Map"].ToString(),
                                                Gov = (reader["Gov"] is DBNull) ? "" : reader["Gov"].ToString(),
                                                Px = (Px > Py ? Px : Py),
                                                Py = (Px > Py ? Py : Px),
                                                Website = (reader["Website"] is DBNull) ? "" : (reader["Website"].ToString()),
                                                Parkinginfo = (reader["Parkinginfo"] is DBNull) ? "" : reader["Parkinginfo"].ToString(),
                                                Parkinginfo_px = (Pix > Piy ? Pix : Piy),
                                                Parkinginfo_py = (Pix > Piy ? Piy : Pix),
                                                Serviceinfo = (reader["Serviceinfo"] is DBNull) ? "" : reader["Serviceinfo"].ToString(),
                                                Class = (reader["Class1"] is DBNull) ? "" : reader["Class1"].ToString(),
                                                //Remarks = (reader["Remarks"] is DBNull) ? "" : reader["Remarks"].ToString(),
                                                Keyword = (reader["Keyword"] is DBNull) ? "" : reader["Keyword"].ToString(),
                                                Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy/MM/dd HH:mm:ss")
                                            }
                                          : new XMLHotel()
                                          {
                                              Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                              Name = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                              Grade = (reader["Grade"] is DBNull) ? "" : reader["Grade"].ToString(),
                                              Fax = faxNumber,
                                              Description = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                              Tel = telNumber,
                                              Add = (reader["Add"] is DBNull) ? "" : reader["Add"].ToString(),
                                              Zipcode = (reader["Zipcode"] is DBNull) ? "" : reader["Zipcode"].ToString(),
                                              Spec = (reader["Spec"] is DBNull) ? "" : reader["Spec"].ToString(),
                                              Picture1 = pic1,
                                              Picdescribe1 = (reader["Picdescribe1"] is DBNull) ? "" : reader["Picdescribe1"].ToString(),
                                              Picture2 = pic2,
                                              Picdescribe2 = (reader["Picdescribe2"] is DBNull) ? "" : reader["Picdescribe2"].ToString(),
                                              Picture3 = pic3,
                                              Picdescribe3 = (reader["Picdescribe3"] is DBNull) ? "" : reader["Picdescribe3"].ToString(),
                                              Map = (reader["Map"] is DBNull) ? "" : reader["Map"].ToString(),
                                              Gov = (reader["Gov"] is DBNull) ? "" : reader["Gov"].ToString(),
                                              Px = (Px > Py ? Px : Py),
                                              Py = (Px > Py ? Py : Px),
                                              Website = (reader["Website"] is DBNull) ? "" : (reader["Website"].ToString()),
                                              Parkinginfo = (reader["Parkinginfo"] is DBNull) ? "" : reader["Parkinginfo"].ToString(),
                                              Parkinginfo_px = (Pix > Piy ? Pix : Piy),
                                              Parkinginfo_py = (Pix > Piy ? Piy : Pix),
                                              Serviceinfo = (reader["Serviceinfo"] is DBNull) ? "" : reader["Serviceinfo"].ToString(),
                                              Class = (reader["Class1"] is DBNull) ? "" : reader["Class1"].ToString(),
                                              //Remarks = (reader["Remarks"] is DBNull) ? "" : reader["Remarks"].ToString(),
                                              Keyword = (reader["Keyword"] is DBNull) ? "" : reader["Keyword"].ToString(),
                                              Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy/MM/dd HH:mm:ss")
                                          };
                                    }
                                    else
                                    {
                                        item = new object();
                                    }

                                    dataList.Add(item);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ContextErrorMessager CEM = new ContextErrorMessager()
                    {
                        RspnMsg = ex.ToString()
                    };
                    dataList.Add(CEM);
                }
            }
            return dataList;
        }
        public List<object> formatDataStringId(List<object> list, string type, XmlHeader xmlHeader, string Id)
        {
            int StartValue = 100000;
            foreach (object item in list)
            {
                if (item.GetType().Name == "ContextErrorMessager")
                {
                    return list;
                }
            }
            Id = (Id == "") ? "" : Convert.ToInt32(Id.Split(new char[] { '_' }).Last()).ToString();
            Id = (Id == "") ? "" : xmlHeader.Language.ToString() + xmlHeader.Listname.ToString() + "_" + xmlHeader.Orgname.ToString() + "_" + Id.ToString();
            switch (type)
            {
                case "node":
                    {
                        if (xmlHeader.Language.ToString() == "E")
                        {
                            foreach (XMLNodeEn item in list)
                            {
                                item.Id = (Int32.TryParse(item.Id, out int num)) ? string.Format("{0}{1}_{2}_{3}",
                                    xmlHeader.Language.ToString(),
                                    xmlHeader.Listname.ToString(),
                                    xmlHeader.Orgname.ToString(),
                                    (num + StartValue).ToString()
                                    ) : "";
                            }
                            list.RemoveAll((o => Id != "" && ((XMLNodeEn)o).Id != Id));
                        }
                        else
                        {
                            foreach (XMLNode item in list)
                            {
                                item.Id = (Int32.TryParse(item.Id, out int num)) ? string.Format("{0}{1}_{2}_{3}",
                                    xmlHeader.Language.ToString(),
                                    xmlHeader.Listname.ToString(),
                                    xmlHeader.Orgname.ToString(),
                                    (num + StartValue).ToString()
                                    ) : "";
                            }
                            list.RemoveAll((o => Id != "" && ((XMLNode)o).Id != Id));
                        }
                        break;
                    }
                case "event":
                    {
                        if (xmlHeader.Language.ToString() == "E")
                        {
                            foreach (XMLEventEn item in list)
                            {
                                item.Id = (Int32.TryParse(item.Id, out int num)) ? string.Format("{0}{1}_{2}_{3}",
                                    xmlHeader.Language.ToString(),
                                    xmlHeader.Listname.ToString(),
                                    xmlHeader.Orgname.ToString(),
                                    (num + StartValue).ToString()
                                    ) : "";
                            }
                            list.RemoveAll((o => Id != "" && ((XMLEventEn)o).Id != Id));
                        }
                        else
                        {
                            foreach (XMLEvent item in list)
                            {
                                item.Id = (Int32.TryParse(item.Id, out int num)) ? string.Format("{0}{1}_{2}_{3}",
                                    xmlHeader.Language.ToString(),
                                    xmlHeader.Listname.ToString(),
                                    xmlHeader.Orgname.ToString(),
                                    (num + StartValue).ToString()
                                    ) : "";
                            }
                            list.RemoveAll((o => Id != "" && ((XMLEvent)o).Id != Id));
                        }
                        break;
                    }
                case "shop":
                    {
                        if (xmlHeader.Language.ToString() == "E")
                        {
                            foreach (XMLShopEn item in list)
                            {
                                item.Id = (Int32.TryParse(item.Id, out int num)) ? string.Format("{0}{1}_{2}_{3}",
                                    xmlHeader.Language.ToString(),
                                    xmlHeader.Listname.ToString(),
                                    xmlHeader.Orgname.ToString(),
                                    (num + StartValue).ToString()
                                    ) : "";
                            }
                            list.RemoveAll((o => Id != "" && ((XMLShopEn)o).Id != Id));
                        }
                        else
                        {
                            foreach (XMLShop item in list)
                            {
                                item.Id = (Int32.TryParse(item.Id, out int num)) ? string.Format("{0}{1}_{2}_{3}",
                                    xmlHeader.Language.ToString(),
                                    xmlHeader.Listname.ToString(),
                                    xmlHeader.Orgname.ToString(),
                                    (num + StartValue).ToString()
                                    ) : "";
                            }
                            list.RemoveAll((o => Id != "" && ((XMLShop)o).Id != Id));
                        }
                        break;
                    }
                case "hotel":
                    {
                        if (xmlHeader.Language.ToString() == "E")
                        {
                            foreach (XMLHotelEn item in list)
                            {
                                item.Id = (Int32.TryParse(item.Id, out int num)) ? string.Format("{0}{1}_{2}_{3}",
                                    xmlHeader.Language.ToString(),
                                    xmlHeader.Listname.ToString(),
                                    xmlHeader.Orgname.ToString(),
                                    (num + StartValue).ToString()
                                    ) : "";
                            }
                            list.RemoveAll((o => Id != "" && ((XMLHotelEn)o).Id != Id));
                        }
                        else
                        {
                            foreach (XMLHotel item in list)
                            {
                                item.Id = (Int32.TryParse(item.Id, out int num)) ? string.Format("{0}{1}_{2}_{3}",
                                    xmlHeader.Language.ToString(),
                                    xmlHeader.Listname.ToString(),
                                    xmlHeader.Orgname.ToString(),
                                    (num + StartValue).ToString()
                                    ) : "";
                            }
                            list.RemoveAll((o => Id != "" && ((XMLHotel)o).Id != Id));
                        }
                        break;
                    }
            }
            return list;
        }
    }

    #region Class For Cn
    public class XMLNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Zone { get; set; }
        public string Toldescribe { get; set; }
        public string Description { get; set; }
        public string Tel { get; set; }
        public string Add { get; set; }
        public string Zipcode { get; set; }
        public string Travellinginfo { get; set; }
        public string Opentime { get; set; }
        public string Picture1 { get; set; }
        public string Picdescribe1 { get; set; }
        public string Picture2 { get; set; }
        public string Picdescribe2 { get; set; }
        public string Picture3 { get; set; }
        public string Picdescribe3 { get; set; }
        public string Map { get; set; }
        public string Gov { get; set; }
        public double Px { get; set; }
        public double Py { get; set; }
        public string Orgclass { get; set; }
        public string Class1 { get; set; }
        public string Class2 { get; set; }
        public string Class3 { get; set; }
        public string Level { get; set; }
        public string Website { get; set; }
        public string Parkinginfo { get; set; }
        public double Parkinginfo_px { get; set; }
        public double Parkinginfo_py { get; set; }
        public string Ticketinfo { get; set; }
        public string Remarks { get; set; }
        public string Keyword { get; set; }
        public string Changetime { get; set; }
    }
    public class XMLEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Description { get; set; }
        public string Tel { get; set; }
        public string Add { get; set; }
        public string Location { get; set; }
        public string Travellinginfo { get; set; }
        public string Picture1 { get; set; }
        public string Picdescribe1 { get; set; }
        public string Picture2 { get; set; }
        public string Picdescribe2 { get; set; }
        public string Picture3 { get; set; }
        public string Picdescribe3 { get; set; }
        public string Map { get; set; }
        public string Org { get; set; }
        public double Px { get; set; }
        public double Py { get; set; }
        public string Particpation { get; set; }
        public string Class1 { get; set; }
        public string Class2 { get; set; }
        public string Cycle { get; set; }
        public string Noncycle { get; set; }
        public string Website { get; set; }
        public string Parkinginfo { get; set; }
        public double Parkinginfo_px { get; set; }
        public double Parkinginfo_py { get; set; }
        public string Charge { get; set; }
        public string Remarks { get; set; }
        public string Keyword { get; set; }
        public string Changetime { get; set; }
    }
    public class XMLShop
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tel { get; set; }
        public string Add { get; set; }
        public string Zipcode { get; set; }
        public string Opentime { get; set; }
        public string Picture1 { get; set; }
        public string Picdescribe1 { get; set; }
        public string Picture2 { get; set; }
        public string Picdescribe2 { get; set; }
        public string Picture3 { get; set; }
        public string Picdescribe3 { get; set; }
        public string Map { get; set; }
        public double Px { get; set; }
        public double Py { get; set; }
        public string Class { get; set; }
        public string Website { get; set; }
        public string Parkinginfo { get; set; }
        public double Parkinginfo_px { get; set; }
        public double Parkinginfo_py { get; set; }
        //public string Remarks { get; set; }
        public string Keyword { get; set; }
        public string Changetime { get; set; }
    }
    public class XMLHotel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Grade { get; set; }
        public string Fax { get; set; }
        public string Description { get; set; }
        public string Tel { get; set; }
        public string Add { get; set; }
        public string Zipcode { get; set; }
        public string Spec { get; set; }
        public string Picture1 { get; set; }
        public string Picdescribe1 { get; set; }
        public string Picture2 { get; set; }
        public string Picdescribe2 { get; set; }
        public string Picture3 { get; set; }
        public string Picdescribe3 { get; set; }
        public string Map { get; set; }
        public string Gov { get; set; }
        public double Px { get; set; }
        public double Py { get; set; }
        public string Class { get; set; }
        public string Website { get; set; }
        public string Parkinginfo { get; set; }
        public double Parkinginfo_px { get; set; }
        public double Parkinginfo_py { get; set; }
        public string Serviceinfo { get; set; }
        //public string Remarks { get; set; }
        public string Keyword { get; set; }
        public string Changetime { get; set; }
    }
    #endregion
    #region Class For En
    public class XMLNodeEn : XMLNode
    {
        public string Name_c { get; set; }
    }
    public class XMLEventEn : XMLEvent
    {
        public string Name_c { get; set; }
    }
    public class XMLShopEn : XMLShop
    {
        public string Name_c { get; set; }
    }
    public class XMLHotelEn : XMLHotel
    {
        public string Name_c { get; set; }
    }
    #endregion
}