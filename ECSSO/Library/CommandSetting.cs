using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class CommandSetting
    {
        GetStr gs = new GetStr();
        public string _strSqlConnection, _orgName;
        private string _type, _tableName, _item, _keyword, _Lng;
        private int _minValue, _maxValue;
        public int isVerityState(string ipAddress, string Token, string strSqlConnection)
        {
            int state = 0;
            using (SqlConnection conn = new SqlConnection(strSqlConnection))
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from talken where talken=@talken and ip=@ip and DateDiff(MINUTE,GETDATE(),CONVERT(datetime,end_time))>=0", conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@talken", Token));
                    cmd.Parameters.Add(new SqlParameter("@ip", ipAddress));
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _strSqlConnection = new GetStr().GetSetting3(reader["orgName"].ToString());
                                    _orgName = reader["orgName"].ToString();
                                }
                            }
                            else
                            {
                                state = 1;
                            }
                        }
                    }
                    catch
                    {
                        state = 2;
                    }
                }
                conn.Close();
            }
            return state;
        }

        public String getDataResult(string ipAddress, string Token, string strSqlConnection, string type, string Lng, string tableName, string itemString, string keyword, int minValue, int maxValue)
        {
            _type = type;
            _tableName = tableName;
            _item = (string.IsNullOrEmpty(itemString)) ? "" : itemString;
            _keyword = (string.IsNullOrEmpty(keyword)) ? "" : keyword;
            //_Lng = gs.CheckStringIsNotNull(Lng);
            _minValue = minValue;
            _maxValue = maxValue;
            string returnMsg = ErrorMsg("error", "有地方出錯了，請聯絡客服人員", "");

            switch (isVerityState(ipAddress, Token, strSqlConnection))
            {
                case 0:
                    DataListSetting dataList = new DataListSetting();
                    dataList.Data = new List<object>();
                    _Lng = gs.GetFullLanString2(_orgName);
                    string selectString = "";

                    using (SqlConnection conn = new SqlConnection(_strSqlConnection))
                    {
                        if (conn.State == ConnectionState.Closed) conn.Open();
                        switch (type)
                        {
                            #region APP-API
                            #region Data Counts
                            case "TotalCounts":
                                {
                                    DataCountsInput items = JsonConvert.DeserializeObject<DataCountsInput>(_item);
                                    bool IsMixed = false;
                                    /*if (_tableName == "attractions") items.ClassID = 101;
                                    else if (_tableName == "Hotel") items.ClassID = 99;
                                    else if (_tableName == "Shop" && items.ClassID == 0) { items.ClassID = 72;  IsMixed = true; };*/
                                    if (_tableName == "attractions") items.ClassID = Convert.ToInt32(getClassID(conn, 4));
                                    else if (_tableName == "Hotel") items.ClassID = Convert.ToInt32(getClassID(conn, 3));
                                    else if (_tableName == "Shop" && items.ClassID == 0) { items.ClassID = Convert.ToInt32(getClassID(conn, 2)); IsMixed = true; }
                                    else if (_tableName == "News") { items.ClassID = Convert.ToInt32(getClassID(conn, 6)); }
                                    if (gs.GetLanString(_Lng) == "en" || gs.GetLanString(_Lng) == "jp")
                                    {
                                        if (items.ClassID == 46) items.ClassID = Convert.ToInt32(getClassID(conn, 7));
                                        if (items.ClassID == 106) items.ClassID = Convert.ToInt32(getClassID(conn, 8));
                                    }
                                    selectString = "SELECT count(distinct menu.id) FROM menu left join menu_sub on menu.sub_id = menu_sub.id Where menu.disp_opt='Y' and ( menu.sub_id = @ClassID ) and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                                    if (_tableName == "Shop" && IsMixed) selectString += " or ( menu.sub_id = " + Convert.ToInt32(getClassID(conn, 5)) + " ) and menu.disp_opt='Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date ";
                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@ClassID", items.ClassID);
                                        try
                                        {
                                            DataCounts TotalCounts = new DataCounts()
                                            {
                                                TotalCounts = Convert.ToInt32(cmd.ExecuteScalar())
                                            };
                                            dataList.Data.Add(TotalCounts);
                                            returnMsg = JsonConvert.SerializeObject(dataList);
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                        }
                                    }
                                    break;
                                }
                            case "TotalCountsByTag":
                                {
                                    DataCountsInput items = JsonConvert.DeserializeObject<DataCountsInput>(_item);
                                    selectString = "SELECT count(distinct menu.id) FROM menu left join menu_sub on menu.sub_id = menu_sub.id left join prod_tag on prod_tag.prod_id = menu.id and prod_tag.[type]='cont' left join tag on tag.id = prod_tag.tag_id Where menu.disp_opt='Y' and ( tag.id = @ClassID ) and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date;";
                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@ClassID", items.ClassID);
                                        try
                                        {
                                            DataCounts TotalCounts = new DataCounts()
                                            {
                                                TotalCounts = Convert.ToInt32(cmd.ExecuteScalar())
                                            };
                                            dataList.Data.Add(TotalCounts);
                                            returnMsg = JsonConvert.SerializeObject(dataList);
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                        }
                                    }
                                    break;
                                }
                            #endregion
                            #region Brief Introduction By Keywords
                            case "Nodes":
                            case "Shops":
                            case "Foods":
                            case "Hotels":
                                {
                                    string tagType = "";
                                    //SqlServer Select語法: 範圍 minValue ~ maxValue
                                    selectString = "SELECT TOP " + maxValue + " * FROM ( "
                                                 + "SELECT  TOP " + (minValue + maxValue) + " tName.menuID, tName.ID, tName.Name, tName.Name_ch, tName.duration, tName.Description, Px, Py, tName.Picture1, '/index.asp?au_id=' + convert(nvarchar, menu_sub.authors_id) + '&sub_id=' + convert(nvarchar, menu_sub.id) + '&id=' + convert(nvarchar, menu.id) DetailURL, menu.popular "
                                                 + "  from " + _tableName + " tName inner join (menu left join menu_sub on menu.sub_id = menu_sub.id inner join searchRelation on menu_sub.id = searchRelation.bindID) on tName.menuID = menu.id "
                                                 + "  where menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date and (tName.Name like @keyword or tName.Keyword like @Keyword) ";
                                    if (type == "Nodes")
                                    {
                                        tagType = "attra";
                                        selectString += "and searchRelation.ClassID = 4 ";
                                    }
                                    else if (type == "Foods")
                                    {
                                        tagType = "shop";
                                        selectString += "and searchRelation.ClassID = 5 ";
                                    }
                                    else if (type == "Shops")
                                    {
                                        tagType = "shop";
                                        selectString += "and searchRelation.ClassID = 2 ";
                                    }
                                    else if (type == "Hotels")
                                    {
                                        tagType = "hotel";
                                        selectString += "and searchRelation.ClassID = 3 ";
                                    }
                                    else
                                    {
                                        tagType = "";
                                        selectString += "";
                                    }
                                    selectString += "ORDER BY ID DESC)a ORDER BY ID; ";
                                    try
                                    {
                                        using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@Keyword", "%" + _keyword + "%");
                                            
                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string picurl = (reader["Picture1"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Picture1"].ToString(), _Lng, "Image"));
                                                        string detailurl = (reader["DetailURL"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["DetailURL"].ToString(), _Lng, ""));

                                                        simpleDataDescription item = new simpleDataDescription
                                                        {
                                                            ID = (reader["ID"] is DBNull) ? "" : reader["ID"].ToString(),
                                                            Title = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                            Brief = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                            Name_ch = (reader["Name_ch"] is DBNull) ? "" : reader["Name_ch"].ToString(),
                                                            PicURL = picurl,
                                                            DetailURL = detailurl,
                                                            Px = (reader["Px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Px"].ToString()),
                                                            Py = (reader["Py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Py"].ToString()),
                                                            Duration = (reader["Duration"] is DBNull) ? 0 : Convert.ToInt32(reader["Duration"].ToString()),
                                                            Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString())
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                        }
                                        dataList.Data = dataList.Data.OrderByDescending(o => ((simpleDataDescription)o).ID).ToList();
                                        if (tagType != "")
                                        {
                                            foreach (simpleDataDescription item in dataList.Data)
                                            {
                                                item.Tag = new List<Tags>();
                                                item.Tag = getTags(conn, item.ID, tagType);
                                            }
                                        }
                                        returnMsg = JsonConvert.SerializeObject(dataList);
                                    }
                                    catch (Exception ex)
                                    {
                                        returnMsg = ErrorMsg("error", ex.ToString(), "");
                                        //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                    }
                                    break;
                                }
                            #endregion
                            #region Detail Introduction
                            case "detailsShop":
                            case "detailsHotel":
                            case "detailsNode":
                            case "detailsEvents":
                            case "detailsShopW":
                            case "detailsHotelW":
                            case "detailsNodeW":
                            case "detailsEventsW":
                                {
                                    if (!int.TryParse(_item, out int i))
                                    {
                                        returnMsg = ErrorMsg("error", "Id不是非負整數", "");
                                        break;
                                    }
                                    selectString = "SELECT tName.*,popular FROM " + _tableName + " tName left join menu on tName.menuID = menu.id WHERE tName.ID = @Id and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date;";
                                    try
                                    {
                                        using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@Id", _item);

                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        object item;
                                                        string pic1 = (reader["Picture1"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Picture1"].ToString(), _Lng, "Image"));
                                                        string pic2 = (reader["Picture2"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Picture2"].ToString(), _Lng, "Image"));
                                                        string pic3 = (reader["Picture3"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Picture3"].ToString(), _Lng, "Image"));
                                                        if (type == "detailsNode" || type == "detailsNodeW")
                                                        {
                                                            item = new NodeItem()
                                                            {
                                                                Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString()),
                                                                Id = reader["Id"].ToString(),
                                                                Name = reader["Name"].ToString(),
                                                                Name_ch = reader["Name_ch"].ToString(),
                                                                Zone = reader["Zone"].ToString(),
                                                                Toldescribe = reader["Toldescribe"].ToString(),
                                                                Description = reader["Description"].ToString(),
                                                                Tel = reader["Tel"].ToString(),
                                                                Location = reader["Add"].ToString(),
                                                                Zipcode = reader["Zipcode"].ToString(),
                                                                Travellinginfo = reader["Travellinginfo"].ToString(),
                                                                Opentime = reader["Opentime"].ToString(),
                                                                Picture1 = pic1,
                                                                Picdescribe1 = reader["Picdescribe1"].ToString(),
                                                                Picture2 = pic2,
                                                                Picdescribe2 = reader["Picdescribe2"].ToString(),
                                                                Picture3 = pic3,
                                                                Picdescribe3 = reader["Picdescribe3"].ToString(),
                                                                Map = reader["Map"].ToString(),
                                                                Gov = reader["Gov"].ToString(),
                                                                Px = (reader["Px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Px"].ToString()),
                                                                Py = (reader["Py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Py"].ToString()),
                                                                Orgclass = reader["Orgclass"].ToString(),
                                                                Class1 = reader["Class1"].ToString(),
                                                                Class2 = reader["Class2"].ToString(),
                                                                Class3 = reader["Class3"].ToString(),
                                                                Level = reader["Level"].ToString(),
                                                                Website = Uri.EscapeDataString(reader["Website"].ToString()),
                                                                Parkinginfo = reader["Parkinginfo"].ToString(),
                                                                Parkinginfo_px = (reader["Parkinginfo_px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Parkinginfo_px"].ToString()),
                                                                Parkinginfo_py = (reader["Parkinginfo_py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Parkinginfo_py"].ToString()),
                                                                Ticketinfo = reader["Ticketinfo"].ToString(),
                                                                Remarks = reader["Remarks"].ToString(),
                                                                Keyword = reader["Keyword"].ToString(),
                                                                Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                                //BeaconUUID = reader["BeaconUUID"].ToString(),
                                                                TripAdvisorUrl = reader["TripAdvisorUrl"].ToString(),
                                                                TripAdvisorComment = (reader["TripAdvisorComment"] is DBNull) ? 0.0 : Convert.ToDouble(reader["TripAdvisorComment"].ToString()),
                                                                GoogleUrl = reader["GoogleUrl"].ToString(),
                                                                GoogleComment = (reader["GoogleComment"] is DBNull) ? 0.0 : Convert.ToDouble(reader["GoogleComment"].ToString()),
                                                                //Comment = Convert.ToInt32(reader["Comment"].ToString()),
                                                                BusInfo = reader["BusInfo"].ToString(),
                                                                Duration = Convert.ToInt32(reader["Duration"].ToString())
                                                            };
                                                        }
                                                        else if (type == "detailsShop" || type == "detailsShopW")
                                                        {
                                                            item = new ShopItem()
                                                            {
                                                                Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString()),
                                                                Id = reader["Id"].ToString(),
                                                                Name = reader["Name"].ToString(),
                                                                Name_ch = reader["Name_ch"].ToString(),
                                                                Toldescribe = reader["Toldescribe"].ToString(),
                                                                Description = reader["Description"].ToString(),
                                                                Tel = reader["Tel"].ToString(),
                                                                Fax = reader["Fax"].ToString(),
                                                                Location = reader["Add"].ToString(),
                                                                Zipcode = reader["Zipcode"].ToString(),
                                                                Travellinginfo = reader["Travellinginfo"].ToString(),
                                                                Opentime = reader["Opentime"].ToString(),
                                                                Picture1 = pic1,
                                                                Picdescribe1 = reader["Picdescribe1"].ToString(),
                                                                Picture2 = pic2,
                                                                Picdescribe2 = reader["Picdescribe2"].ToString(),
                                                                Picture3 = pic3,
                                                                Picdescribe3 = reader["Picdescribe3"].ToString(),
                                                                Map = reader["Map"].ToString(),
                                                                Px = (reader["Px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Px"].ToString()),
                                                                Py = (reader["Py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Py"].ToString()),
                                                                Website = Uri.EscapeDataString(reader["Website"].ToString()),
                                                                Parkinginfo = reader["Parkinginfo"].ToString(),
                                                                Parkinginfo_px = (reader["Parkinginfo_px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Parkinginfo_px"].ToString()),
                                                                Parkinginfo_py = (reader["Parkinginfo_py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Parkinginfo_py"].ToString()),
                                                                Zone = reader["Zone"].ToString(),
                                                                Remarks = reader["Remarks"].ToString(),
                                                                Keyword = reader["Keyword"].ToString(),
                                                                //BeaconUUID = "資料庫無此欄位",
                                                                QRCode = reader["QRCode"].ToString(),
                                                                Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                                Class1 = reader["Class1"].ToString(),
                                                                Source = reader["Source"].ToString(),
                                                                TripAdvisorUrl = reader["TripAdvisorUrl"].ToString(),
                                                                TripAdvisorComment = (reader["TripAdvisorComment"] is DBNull) ? 0.0 : Convert.ToDouble(reader["TripAdvisorComment"].ToString()),
                                                                GoogleUrl = reader["GoogleUrl"].ToString(),
                                                                GoogleComment = (reader["GoogleComment"] is DBNull) ? 0.0 : Convert.ToDouble(reader["GoogleComment"].ToString()),
                                                                //Comment = Convert.ToInt32(reader["Comment"].ToString()),
                                                                BusInfo = reader["BusInfo"].ToString(),
                                                                Duration = Convert.ToInt32(reader["Duration"].ToString())
                                                            };
                                                        }
                                                        else if (type == "detailsHotel" || type == "detailsHotelW")
                                                        {
                                                            item = new HotelItem()
                                                            {
                                                                Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString()),
                                                                Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                                                Name = (reader["Name_ch"] is DBNull) ? "" : reader["Name"].ToString(),
                                                                Name_ch = (reader["Name_ch"] is DBNull) ? "" : reader["Name_ch"].ToString(),
                                                                Grade = (reader["Grade"] is DBNull) ? "" : reader["Grade"].ToString(),
                                                                Fax = (reader["Fax"] is DBNull) ? "" : reader["Fax"].ToString(),
                                                                Toldescribe = (reader["Toldescribe"] is DBNull) ? "" : reader["Toldescribe"].ToString(),
                                                                Description = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                                Tel = (reader["Tel"] is DBNull) ? "" : reader["Tel"].ToString(),
                                                                Location = (reader["Add"] is DBNull) ? "" : reader["Add"].ToString(),
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
                                                                Px = (reader["Px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Px"].ToString()),
                                                                Py = (reader["Py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Py"].ToString()),
                                                                Website = (reader["Website"] is DBNull) ? "" : (reader["Website"].ToString()),
                                                                Parkinginfo = (reader["Parkinginfo"] is DBNull) ? "" : reader["Parkinginfo"].ToString(),
                                                                Parkinginfo_px = (reader["Parkinginfo_px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Parkinginfo_px"].ToString()),
                                                                Parkinginfo_py = (reader["Parkinginfo_py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Parkinginfo_py"].ToString()),
                                                                Serviceinfo = (reader["Serviceinfo"] is DBNull) ? "" : reader["Serviceinfo"].ToString(),
                                                                Class1 = (reader["Class1"] is DBNull) ? "" : reader["Class1"].ToString(),
                                                                Keyword = (reader["Keyword"] is DBNull) ? "" : reader["Keyword"].ToString(),
                                                                Source = (reader["Source"] is DBNull) ? "" : reader["Source"].ToString(),
                                                                QRCode = (reader["QRCode"] is DBNull) ? "" : reader["QRCode"].ToString(),
                                                                Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                                //BeaconUUID = reader["BeaconUUID"].ToString(),
                                                                TripAdvisorUrl = (reader["TripAdvisorUrl"] is DBNull) ? "" : reader["TripAdvisorUrl"].ToString(),
                                                                TripAdvisorComment = (reader["TripAdvisorComment"] is DBNull) ? 0.0 : Convert.ToDouble(reader["TripAdvisorComment"].ToString()),
                                                                GoogleUrl = (reader["GoogleUrl"] is DBNull) ? "" : reader["GoogleUrl"].ToString(),
                                                                GoogleComment = (reader["GoogleComment"] is DBNull) ? 0.0 : Convert.ToDouble(reader["GoogleComment"].ToString()),
                                                                //Comment = Convert.ToInt32(reader["Comment"].ToString()),
                                                                BusInfo = (reader["BusInfo"] is DBNull) ? "" : reader["BusInfo"].ToString(),
                                                                Duration = (reader["Duration"] is DBNull) ? 0 : Convert.ToInt32(reader["Duration"].ToString())
                                                            };
                                                        }
                                                        else if (type == "detailsEvents" || type == "detailsEventsW")
                                                        {
                                                            item = new detailEventsItem()
                                                            {
                                                                Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString()),
                                                                Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                                                Orgname = (reader["Orgname"] is DBNull) ? "" : reader["Orgname"].ToString(),
                                                                Org = (reader["Org"] is DBNull) ? "" : reader["Org"].ToString(),
                                                                Co_Organiser = (reader["Co_Organiser"] is DBNull) ? "" : reader["Co_Organiser"].ToString(),
                                                                GovID = (reader["GovID"] is DBNull) ? "" : reader["GovID"].ToString(),
                                                                Name = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                                Name_ch = (reader["Name_ch"] is DBNull) ? "" : reader["Name_ch"].ToString(),
                                                                Toldescribe = (reader["Toldescribe"] is DBNull) ? "" : reader["Toldescribe"].ToString(),
                                                                Description = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                                Tel = (reader["Tel"] is DBNull) ? "" : reader["Tel"].ToString(),
                                                                Location = (reader["Location"] is DBNull) ? "" : reader["Location"].ToString(),
                                                                Add = (reader["Add"] is DBNull) ? "" : reader["Add"].ToString(),
                                                                Particpation = (reader["Particpation"] is DBNull) ? "" : reader["Particpation"].ToString(),
                                                                Cycle = (reader["Cycle"] is DBNull) ? "" : reader["Cycle"].ToString(),
                                                                NonCycle = (reader["NonCycle"] is DBNull) ? "" : reader["NonCycle"].ToString(),
                                                                StartDay = (reader["Start"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["Start"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                                EndDay = (reader["End"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["End"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                                Travellinginfo = (reader["Travellinginfo"] is DBNull) ? "" : reader["Travellinginfo"].ToString(),
                                                                Charge = (reader["Charge"] is DBNull) ? "" : reader["Charge"].ToString(),
                                                                Picture1 = pic1,
                                                                Picdescribe1 = (reader["Picdescribe1"] is DBNull) ? "" : reader["Picdescribe1"].ToString(),
                                                                Picture2 = pic2,
                                                                Picdescribe2 = (reader["Picdescribe2"] is DBNull) ? "" : reader["Picdescribe2"].ToString(),
                                                                Picture3 = pic3,
                                                                Picdescribe3 = (reader["Picdescribe3"] is DBNull) ? "" : reader["Picdescribe3"].ToString(),
                                                                Map = (reader["Map"] is DBNull) ? "" : reader["Map"].ToString(),
                                                                Px = (reader["Px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Px"].ToString()),
                                                                Py = (reader["Py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Py"].ToString()),
                                                                Website = (reader["Website"] is DBNull) ? "" : Uri.EscapeDataString(reader["Website"].ToString()),
                                                                Parkinginfo = (reader["Parkinginfo"] is DBNull) ? "" : reader["Parkinginfo"].ToString(),
                                                                Parkinginfo_px = (reader["Parkinginfo_px"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Parkinginfo_px"].ToString()),
                                                                Parkinginfo_py = (reader["Parkinginfo_py"] is DBNull) ? 0.0 : Convert.ToDouble(reader["Parkinginfo_py"].ToString()),
                                                                Remarks = (reader["Remarks"] is DBNull) ? "" : reader["Remarks"].ToString(),
                                                                Keyword = (reader["Keyword"] is DBNull) ? "" : reader["Keyword"].ToString(),
                                                                QRCode = (reader["QRCode"] is DBNull) ? "" : reader["QRCode"].ToString(),
                                                                Changetime = (reader["Changetime"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["Changetime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                                Updatetime = (reader["Updatetime"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["Updatetime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                                Class1 = (reader["Class1"] is DBNull) ? "" : reader["Class1"].ToString(),
                                                                Class2 = (reader["Class2"] is DBNull) ? "" : reader["Class2"].ToString(),
                                                                //BeaconUUID = reader["BeaconUUID"].ToString(),
                                                                //TripAdvisorUrl = reader["TripAdvisorUrl"].ToString(),
                                                                //TripAdvisorComment = (reader["TripAdvisorComment"] is DBNull) ? 0F : Convert.ToDouble(reader["TripAdvisorComment"].ToString()),
                                                                //GoogleUrl = reader["GoogleUrl"].ToString(),
                                                                //GoogleComment = (reader["GoogleComment"] is DBNull) ? 0F : Convert.ToDouble(reader["GoogleComment"].ToString()),
                                                                //Comment = Convert.ToInt32(reader["Comment"].ToString()),
                                                                BusInfo = (reader["BusInfo"] is DBNull) ? "" : reader["BusInfo"].ToString(),
                                                                Duration = (reader["Duration"] is DBNull) ? 0 : Convert.ToInt32(reader["Duration"].ToString())
                                                            };
                                                        }
                                                        else
                                                        {
                                                            item = new object();
                                                        }
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                        }
                                        if (type == "detailsNode" || type == "detailsNodeW")
                                        {
                                            dataList.Data = dataList.Data.OrderByDescending(o => ((NodeItem)o).Id).ToList();
                                            foreach (NodeItem item in dataList.Data)
                                            {
                                                item.Tag = new List<Tags>();
                                                item.Tag = getTags(conn, item.Id, "attra");
                                            }
                                            if (type == "detailsNodeW")
                                            {
                                                foreach (NodeItem item in dataList.Data)
                                                {
                                                    item.Fram = new List<Frams>();
                                                    item.Fram = getFences(conn, getMenuID(conn, item.Id, "attra"));
                                                }
                                            }
                                        }
                                        else if (type == "detailsShop" || type == "detailsShopW")
                                        {
                                            dataList.Data = dataList.Data.OrderByDescending(o => ((ShopItem)o).Id).ToList();
                                            foreach (ShopItem item in dataList.Data)
                                            {
                                                item.Tag = new List<Tags>();
                                                item.Tag = getTags(conn, item.Id, "shop");
                                            }
                                            if (type == "detailsShopW")
                                            {
                                                foreach (ShopItem item in dataList.Data)
                                                {
                                                    item.Fram = new List<Frams>();
                                                    item.Fram = getFences(conn, getMenuID(conn, item.Id, "shop"));
                                                }
                                            }
                                        }
                                        else if (type == "detailsHotel" || type == "detailsHotelW")
                                        {
                                            dataList.Data = dataList.Data.OrderByDescending(o => ((HotelItem)o).Id).ToList();
                                            foreach (HotelItem item in dataList.Data)
                                            {
                                                item.Tag = new List<Tags>();
                                                item.Tag = getTags(conn, item.Id, "hotel");
                                            }
                                            if (type == "detailsHotelW")
                                            {
                                                foreach (HotelItem item in dataList.Data)
                                                {
                                                    item.Fram = new List<Frams>();
                                                    item.Fram = getFences(conn, getMenuID(conn, item.Id, "hotel"));
                                                }
                                            }
                                        }
                                        else if (type == "detailsEvents" || type == "detailsEventsW")
                                        {
                                            dataList.Data = dataList.Data.OrderByDescending(o => ((detailEventsItem)o).Id).ToList();
                                            foreach (detailEventsItem item in dataList.Data)
                                            {
                                                item.Tag = new List<Tags>();
                                                item.Tag = getTags(conn, item.Id, "active");
                                            }
                                            if (type == "detailsEventsW")
                                            {
                                                foreach (detailEventsItem item in dataList.Data)
                                                {
                                                    item.Fram = new List<Frams>();
                                                    item.Fram = getFences(conn, getMenuID(conn, item.Id, "active"));
                                                }
                                            }
                                        }

                                        returnMsg = JsonConvert.SerializeObject(dataList);
                                    }
                                    catch (Exception ex)
                                    {
                                        returnMsg = ErrorMsg("error", ex.ToString(), "");
                                        //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                    }
                                    break;
                                }
                            #endregion
                            #region Voices
                            case "nodeGuide":
                            case "shopGuide":
                            case "hotelGuide":
                            case "greeting":
                                {
                                    Voices items = JsonConvert.DeserializeObject<Voices>(_item);

                                    selectString = "SELECT url FROM " + _tableName + " WHERE Type = @type and linkID = @linkID;";
                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        if (!String.IsNullOrEmpty(items.nodeID) || items.nodeID == "")
                                        {
                                            cmd.Parameters.AddWithValue("@type", 1);
                                            cmd.Parameters.AddWithValue("@linkID", items.nodeID);
                                        }
                                        else if (!String.IsNullOrEmpty(items.shopID) || items.shopID == "")
                                        {
                                            cmd.Parameters.AddWithValue("@type", 3);
                                            cmd.Parameters.AddWithValue("@linkID", items.shopID);
                                        }
                                        else if (!String.IsNullOrEmpty(items.hotelID) || items.hotelID == "")
                                        {
                                            cmd.Parameters.AddWithValue("@type", 4);
                                            cmd.Parameters.AddWithValue("@linkID", items.hotelID);
                                        }
                                        else
                                        {
                                            cmd.Parameters.AddWithValue("@type", 0);
                                            cmd.Parameters.AddWithValue("@linkID", 0);
                                        }
                                        try
                                        {
                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        VoicesItem item = new VoicesItem
                                                        {
                                                            URL = (reader["url"] is DBNull) ? "" : Uri.EscapeDataString(reader["url"].ToString())
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                            returnMsg = JsonConvert.SerializeObject(dataList);
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                        }
                                    }
                                    break;
                                }
                            #endregion
                            #region MyTripSet
                            case "getMyTrip":
                                {
                                    MyTrip items = JsonConvert.DeserializeObject<MyTrip>(_item);
                                    MyTripInstance instance = new MyTripInstance();

                                    List<MyTripItem> MyTripList = instance.SelectTrip(conn, items.MemID);

                                    foreach (MyTripItem item in MyTripList)
                                    {
                                        dataList.Data.Add(item);
                                    }

                                    returnMsg = JsonConvert.SerializeObject(dataList);
                                    break;
                                }
                            case "getMyTripDetail":
                                {
                                    MyTrip items = JsonConvert.DeserializeObject<MyTrip>(_item);
                                    MyTripInstance instance = new MyTripInstance();

                                    List<detailMyTripItem> item = instance.SelectTripData(conn, items.MemID, items.TripID);
                                    if (item != null || item.Count > 0)
                                    {
                                        foreach (detailMyTripItem obj in item)
                                        {
                                            dataList.Data.Add(obj);
                                        }
                                    }

                                    returnMsg = JsonConvert.SerializeObject(dataList);
                                    break;
                                }
                            case "UploadMyTrip":
                                {
                                    MyTripItem items = JsonConvert.DeserializeObject<MyTripItem>(_item);
                                    MyTripInstance instance = new MyTripInstance();
                                    int state = 0, CUstate = -1;
                                    string curTripID = "";

                                    if (instance.checkMemberID(conn, items.MemID) < 2) //This MemID is correct
                                    {
                                        if (gs.CheckStringIsNotNull(items.ID) == "")
                                        {
                                            // Create
                                            CUstate = 1;
                                            state |= instance.CreateTrip(conn, items);
                                            curTripID = instance.getCurrentTripId;
                                        }
                                        else if (instance.IsCheckedTripByTripID(conn, items.ID))
                                        {
                                            // Update
                                            CUstate = 0;
                                            state |= instance.UpdateTrip(conn, items);
                                            curTripID = items.ID;
                                        }
                                    }
                                    else
                                    {
                                        state = 1;
                                        CUstate = 2;
                                    }

                                    returnMsg = ContextMessager(state, CUstate.ToString(), curTripID);
                                    break;
                                }
                            case "UploadMyTripDetail":
                                {
                                    detailMyTripItem items = JsonConvert.DeserializeObject<detailMyTripItem>(_item);
                                    MyTripInstance instance = new MyTripInstance();
                                    int state = 0, CUstate = -1;
                                    string curTripID = gs.CheckStringIsNotNull(items.TripID);

                                    string memID = instance.getMemberIDbyTrip(conn, curTripID);
                                    detailMyTripItem MyDetailTrip = instance.SelectTripData(conn, memID, curTripID, items.TripDay.ToString());
                                    
                                    if (memID != null) //This MemID is correct
                                    {
                                        // Has value
                                        if (MyDetailTrip != null)
                                        {
                                            state |= instance.DeleteTripDataByTripId(conn, curTripID, items.TripDay.ToString());
                                            state |= instance.DeleteTripDayByTripId(conn, curTripID, items.TripDay.ToString());
                                        }
                                        if (state == 0)
                                        {
                                            if (instance.CreateTripData(conn, items) == 1)
                                            {
                                                state = 1;
                                                CUstate = 1;
                                            }
                                        }
                                        else
                                        {
                                            CUstate = 0;
                                        }
                                    }
                                    else
                                    {
                                        state = 1;
                                        CUstate = 2;
                                    }
                                    
                                    returnMsg = ContextMessager(state, CUstate.ToString(), curTripID);
                                    break;
                                }
                            case "DeleteMyTrip":
                                {
                                    MyTrip items = JsonConvert.DeserializeObject<MyTrip>(_item);
                                    MyTripInstance instance = new MyTripInstance();
                                    int state = 0, CUstate = 4;
                                    
                                    if (instance.DeleteTripById(conn, items.TripID) == 0)
                                    {
                                        state |= instance.DeleteTripDataByTripId(conn, items.TripID);
                                        state |= instance.DeleteTripDayByTripId(conn, items.TripID);
                                    }

                                    returnMsg = ContextMessager(state, CUstate.ToString(), items.TripID);
                                    break;
                                }
                            case "DeleteMyTripDetail":
                                {
                                    MyTrip items = JsonConvert.DeserializeObject<MyTrip>(_item);
                                    MyTripInstance instance = new MyTripInstance();
                                    int state = 0, CUstate = 4;

                                    state |= instance.DeleteTripDataByTripId(conn, items.TripID);
                                    state |= instance.DeleteTripDayByTripId(conn, items.TripID);

                                    returnMsg = ContextMessager(state, CUstate.ToString(), items.TripID);
                                    break;
                                }
                            #endregion
                            #region Traffic
                            case "Traffic":
                                {
                                    string url = "/index.asp?au_id=69&sub_id=73";
                                    TrafficItem item = new TrafficItem()
                                    {
                                        URL = Uri.EscapeDataString(gs.GetAllLinkString(_orgName, url, _Lng, ""))
                                    };
                                    dataList.Data.Add(item);
                                    returnMsg = JsonConvert.SerializeObject(dataList);
                                    break;
                                }
                            #endregion
                            #region Beacon
                            case "Beacon":
                                {
                                    Beacon items = JsonConvert.DeserializeObject<Beacon>(_item);
                                    items.BeaconUUID = (gs.CheckStringIsNotNull(items.BeaconUUID) == "") ? "%%" : items.BeaconUUID;
                                    selectString = "sp_getBeaconData";
                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@beaconID", items.BeaconUUID);
                                        cmd.Parameters.AddWithValue("@major", items.BeaconMajor);
                                        try
                                        {
                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string detailURL = (reader["href"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["href"].ToString(), _Lng, ""));
                                                        
                                                        BeaconItem item = new BeaconItem()
                                                        {
                                                            Title = (reader["title"] is DBNull) ? "" : reader["title"].ToString(),
                                                            URL = detailURL
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                            returnMsg = JsonConvert.SerializeObject(dataList);
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                        }
                                    }
                                    break;
                                }
                            case "BeaconPower":
                                {
                                    Beacon items = JsonConvert.DeserializeObject<Beacon>(_item);
                                    BeaconInstance instance = new BeaconInstance();
                                    int state = 0, CUstate = -1;

                                    string curBeaconID = instance.SelectIdByBeacon(conn, items);
                                    if (gs.CheckStringIsNotNull(curBeaconID) != "")
                                    {
                                        if (instance.UpdateBeaconPower(conn, curBeaconID, items.Power) == 1)
                                        {
                                            state = 1;
                                            CUstate = 1; //更新失敗
                                        }
                                    }
                                    else
                                    {
                                        state = 1;
                                        CUstate = 0; //查無Beacon
                                    }

                                    returnMsg = ContextMessager2(state, CUstate.ToString(), curBeaconID);
                                    break;
                                }
                            #endregion
                            #region Trips
                            case "Trips":
                                {
                                    Trip items = JsonConvert.DeserializeObject<Trip>(_item);
                                    selectString = "sp_getTrip";
                                    try
                                    {
                                        using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.Parameters.AddWithValue("@id", items.ClassID);

                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string imgUrl = (reader["Img1"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Img1"].ToString(), _Lng, "Image"));
                                                        string detailURL = (reader["href"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["href"].ToString(), _Lng, ""));

                                                        TripItem item = new TripItem
                                                        {
                                                            ID = (reader["ID"] is DBNull) ? 0 : Convert.ToInt32(reader["ID"].ToString()),
                                                            Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                                            Brief = (reader["Remark"] is DBNull) ? "" : reader["Remark"].ToString(),
                                                            PicURL = imgUrl,
                                                            DetailURL = detailURL,
                                                            NodeNum = (reader["Amount"] is DBNull) ? 0 : Convert.ToInt32(reader["Amount"].ToString()),
                                                            TotalTime = (reader["Days"] is DBNull) ? "" : reader["Days"].ToString(),
                                                            Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString())
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                        }
                                        foreach (TripItem item in dataList.Data)
                                        {
                                            item.Tag = new List<Tags>();
                                            item.Tag = getTags(conn, item.ID.ToString(), "trip");
                                        }
                                        returnMsg = JsonConvert.SerializeObject(dataList);
                                    }
                                    catch (Exception ex)
                                    {
                                        returnMsg = ErrorMsg("error", ex.ToString(), "");
                                        //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                    }
                                    break;
                                }
                            case "detailsTrip":
                                {
                                    Trip items = JsonConvert.DeserializeObject<Trip>(_item);
                                    selectString = "Select MyTrip.*, TripDay, StartTime, Trip.Title, Trip.Remark, Trip.Img, '/index.asp?au_id='+CONVERT(nvarchar,menu_sub.authors_id)+'&sub_id='+CONVERT(nvarchar,menu.sub_id)+'&id='+CONVERT(nvarchar,menu.id) link From ("
                                                 + "select TripData.*, active.menuID, active.Name, active.Description from TripData left join active on TripData.nodeID = active.id where[TYPE] = 1 "
                                                 + " union all "
                                                 + "select TripData.*, shop.menuID, shop.Name, shop.Description from TripData inner join shop on TripData.nodeID = shop.id where[TYPE] = 2 "
                                                 + " union all "
                                                 + "select TripData.*, hotel.menuID, hotel.Name, hotel.Description from TripData inner join hotel on TripData.nodeID = hotel.id where[TYPE] = 3 "
                                                 + " union all "
                                                 + "select TripData.*, attractions.menuID, attractions.Name, attractions.Description from TripData inner join attractions on TripData.nodeID = attractions.id where[TYPE] = 4 "
                                                 + ") as myTrip inner join TripDay on TripDay.TripID = myTrip.TripID and TripDay = Nth_Day left join Trip on TripDay.TripID = Trip.id left join (menu left join menu_sub on menu.sub_id = menu_sub.id) on myTrip.menuID = menu.id "
                                                 + "Where TripDay.TripID = @TripID and Menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date "
                                                 + "Order By TripID, TripDay, Ser_No";
                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@TripID", items.Id);
                                        try
                                        {
                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    detailTrip item = new detailTrip();
                                                    item.Tag = new List<Tags>();
                                                    item.TripNodes = new List<TripNode>();
                                                    int iCurrent = 1;
                                                    while (reader.Read())
                                                    {
                                                        string imgUrl = (reader[14] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader[14].ToString(), _Lng, "Image"));
                                                        string detailURL = (reader["link"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["link"].ToString(), _Lng, ""));

                                                        int iCount = Convert.ToInt32(reader["TripDay"].ToString());
                                                        if (iCurrent != iCount)
                                                        {
                                                            dataList.Data.Add(item);
                                                            item = new detailTrip();
                                                            item.Tag = new List<Tags>();
                                                            item.TripNodes = new List<TripNode>();
                                                            iCurrent++;
                                                        }
                                                        item.TripDay = iCount;
                                                        item.StartTime = (reader["StartTime"] is DBNull) ? DateTime.Now.ToString("HH:mm:ss") : reader["StartTime"].ToString();
                                                        /*item.Remark = reader["Remark"].ToString();
                                                        item.Title = reader["Title"].ToString();
                                                        item.Img = reader[23].ToString();*/
                                                        TripNode obj = new TripNode()
                                                        {
                                                            sn = (reader["ser_no"] is DBNull) ? 0 : Convert.ToInt32(reader["ser_no"].ToString()),
                                                            NodeID = (reader["NodeID"] is DBNull) ? 0 : Convert.ToInt32(reader["NodeID"].ToString()),
                                                            Name = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                            Img = imgUrl,
                                                            Text = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                            Type = (reader["Type"] is DBNull) ? "" : reader["Type"].ToString(),
                                                            tripDateTime = (reader["tripDateTime"] is DBNull) ? "" : reader["tripDateTime"].ToString(),
                                                            Duration = (reader["Duration"] is DBNull) ? 0 : Convert.ToInt32(reader["Duration"].ToString()),
                                                            RouteTime = (reader["RouteTime"] is DBNull) ? 0 : Convert.ToInt32(reader["RouteTime"].ToString()),
                                                            Transportation = (reader["Transportation"] is DBNull) ? 0 : Convert.ToInt32(reader["Transportation"].ToString()),
                                                            Px = (reader["x"] is DBNull) ? 0.0 : Convert.ToDouble(reader["x"].ToString()),
                                                            Py = (reader["y"] is DBNull) ? 0.0 : Convert.ToDouble(reader["y"].ToString()),
                                                            Link = detailURL
                                                        };
                                                        item.TripNodes.Add(obj);
                                                    }
                                                    dataList.Data.Add(item);
                                                }
                                            }
                                            returnMsg = JsonConvert.SerializeObject(dataList);
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                        }
                                    }
                                    break;
                                }
                            case "searchTrips":
                                {
                                    Trip items = JsonConvert.DeserializeObject<Trip>(_item);
                                    selectString = "select trip.id,trip.title,trip.remark,menu.img1,trip.amount,trip.[days],menu.popular,'/index.asp?au_id='+CONVERT(nvarchar,menu_sub.authors_id)+'&sub_id='+CONVERT(nvarchar,menu_sub.id)+'&id='+CONVERT(nvarchar,menu.id) href from trip "
                                                 + "inner join menu on trip.menuID=menu.id "
                                                 + "inner join menu_sub on menu.sub_id=menu_sub.id "/*
                                                 + "inner join prod_tag on prod_tag.prod_id = menu.id and prod_tag.[type]='cont' "
                                                 + "inner join tag on tag.id = prod_tag.tag_id "
                                                 + "where tag.id = @id and menu.disp_opt = 'Y' and trip.disp_opt='Y' and (trip.title like @Keyword or (trip.remark like @Keyword or trip.remark like @DecodeKeyword)) "*/
                                                 + "where Menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date and (trip.title like @Keyword or trip.remark like @DecodeKeyword) "
                                                 + "order by menu.ser_no";
                                    try
                                    {
                                        using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                        {
                                            //cmd.Parameters.AddWithValue("@id", items.ClassID);
                                            cmd.Parameters.AddWithValue("@Keyword", "%" + items.keyword + "%");
                                            cmd.Parameters.AddWithValue("@DecodeKeyword", "%" + gs.HtmlEncode(items.keyword) + "%");

                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string imgUrl = (reader["Img1"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Img1"].ToString(), _Lng, "Image"));
                                                        string detailURL = (reader["href"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["href"].ToString(), _Lng, ""));

                                                        TripItem item = new TripItem
                                                        {
                                                            ID = (reader["ID"] is DBNull) ? 0 : Convert.ToInt32(reader["ID"].ToString()),
                                                            Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                                            Brief = (reader["Remark"] is DBNull) ? "" : reader["Remark"].ToString(),
                                                            PicURL = imgUrl,
                                                            DetailURL = detailURL,
                                                            NodeNum = (reader["Amount"] is DBNull) ? 0 : Convert.ToInt32(reader["Amount"].ToString()),
                                                            TotalTime = (reader["Days"] is DBNull) ? "" : reader["Days"].ToString(),
                                                            Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString())
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                        }
                                        foreach (TripItem item in dataList.Data)
                                        {
                                            item.Tag = new List<Tags>();
                                            item.Tag = getTags(conn, item.ID.ToString(), "trip");
                                        }
                                        returnMsg = JsonConvert.SerializeObject(dataList);
                                    }
                                    catch (Exception ex)
                                    {
                                        returnMsg = ErrorMsg("error", ex.ToString(), "");
                                        //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                    }
                                    break;
                                }
                            #endregion
                            #region Events
                            case "TotalEvents":
                                {
                                    DataCountsInput items = JsonConvert.DeserializeObject<DataCountsInput>(_item);
                                    selectString = "SELECT COUNT(menu.id) FROM Menu left join menu_sub on menu.sub_id = menu_sub.id Where menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date and (menu.sub_id = @EventArtistic or menu.sub_id = @EventInformation);";

                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@EventArtistic", getClassID(conn, 7));
                                        cmd.Parameters.AddWithValue("@EventInformation", getClassID(conn, 8));
                                        try
                                        {
                                            DataCounts TotalCounts = new DataCounts()
                                            {
                                                TotalCounts = Convert.ToInt32(cmd.ExecuteScalar())
                                            };
                                            dataList.Data.Add(TotalCounts);
                                            returnMsg = JsonConvert.SerializeObject(dataList);
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                        }
                                    }
                                    break;
                                }
                            case "NewsEvents":
                                {
                                    Events items = JsonConvert.DeserializeObject<Events>(_item);
                                    //SqlServer Select語法: 範圍 minValue ~ maxValue
                                    selectString = @"SELECT tName.Id, tName.Name, tName.Description, tName.Picture1,'/index.asp?au_id=' + convert(nvarchar, menu_sub.authors_id) + '&sub_id=' + CONVERT(nvarchar, menu.sub_id) + '&id=' + convert(nvarchar, menu.id) DetailURL, tName.[Start], tName.[End] 
                                                     from active tName left join menu on tName.menuID = menu.id left join menu_sub on menu.sub_id = menu_sub.id 
                                                     Where menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date and menu.sub_id = @subMenuID ";

                                    try
                                    {
                                        using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@subMenuID", getClassID(conn, 6));

                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string imgUrl = (reader["Picture1"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Picture1"].ToString(), _Lng, "Image"));
                                                        string detailURL = (reader["DetailURL"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["DetailURL"].ToString(), _Lng, ""));
                                                        EventsItem item = new EventsItem
                                                        {
                                                            ID = (reader["Id"] is DBNull) ? 0 : Convert.ToInt32(reader["Id"].ToString()),
                                                            Title = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                            Brief = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                            PicURL = imgUrl,
                                                            DetailURL = detailURL,
                                                            StartDay = (reader["Start"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["Start"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                            EndDay = (reader["End"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["End"]).ToString("yyyy-MM-dd HH:mm:ss")
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                        }
                                        
                                        dataList.Data = dataList.Data.OrderByDescending(o => ((EventsItem)o).ID).Take(minValue + maxValue).Skip(minValue).ToList();

                                        foreach (EventsItem item in dataList.Data)
                                        {
                                            item.Tag = new List<Tags>();
                                            item.Tag = getTags(conn, item.ID.ToString(), "active");
                                        }
                                        returnMsg = JsonConvert.SerializeObject(dataList);
                                    }
                                    catch (Exception ex)
                                    {
                                        returnMsg = ErrorMsg("error", ex.ToString(), "");
                                        //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                    }
                                    break;
                                }
                            case "Events":
                            case "otherEvents":
                                {
                                    Events items = JsonConvert.DeserializeObject<Events>(_item);
                                    //SqlServer Select語法: 範圍 minValue ~ maxValue
                                    selectString = "SELECT TOP " + _maxValue + " * FROM (SELECT TOP " + (_minValue + _maxValue) + " tName.[Start], tName.[End], tName.Id, tName.Name, tName.Description, tName.Picture1,'/index.asp?au_id=' + convert(nvarchar, menu_sub.authors_id) + '&sub_id=' + CONVERT(nvarchar, menu.sub_id) + '&id=' + convert(nvarchar, menu.id) DetailURL from " + _tableName + " tName left join menu on tName.menuID = menu.id left join menu_sub on menu.sub_id = menu_sub.id Where menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date and menu.sub_id = @subMenuID ORDER BY Id DESC) a ORDER BY Id;";
                                    try
                                    {
                                        using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                        {
                                            if (type == "Events") cmd.Parameters.AddWithValue("@subMenuID", getClassID(conn, 7));
                                            else if (type == "otherEvents") cmd.Parameters.AddWithValue("@subMenuID", getClassID(conn, 8));
                                            else cmd.Parameters.AddWithValue("@subMenuID", items.subMenuID);

                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string imgUrl = (reader["Picture1"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Picture1"].ToString(), _Lng, "Image"));
                                                        string detailURL = (reader["DetailURL"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["DetailURL"].ToString(), _Lng, ""));
                                                        EventsItem item = new EventsItem
                                                        {
                                                            ID = (reader["Id"] is DBNull) ? 0 : Convert.ToInt32(reader["Id"].ToString()),
                                                            Title = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                            Brief = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                            PicURL = imgUrl,
                                                            DetailURL = detailURL,
                                                            StartDay = (reader["Start"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["Start"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                            EndDay = (reader["End"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["End"]).ToString("yyyy-MM-dd HH:mm:ss")
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                        }
                                        dataList.Data = dataList.Data.OrderByDescending(o => ((EventsItem)o).ID).ToList();
                                        foreach (EventsItem item in dataList.Data)
                                        {
                                            item.Tag = new List<Tags>();
                                            item.Tag = getTags(conn, item.ID.ToString(), "active");
                                        }
                                        returnMsg = JsonConvert.SerializeObject(dataList);
                                    }
                                    catch (Exception ex)
                                    {
                                        returnMsg = ErrorMsg("error", ex.ToString(), "");
                                        //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                    }
                                    break;
                                }
                            case "AllEvents":
                            case "searchEvents":
                                {
                                    Events items = JsonConvert.DeserializeObject<Events>(_item);
                                    //SqlServer Select語法: 範圍 minValue ~ maxValue
                                    selectString = "SELECT TOP " + _maxValue + " * FROM (SELECT TOP " + (_minValue + _maxValue) + " tName.[Start], tName.[End], tName.Id, tName.Name, tName.Description, tName.Picture1,'/index.asp?au_id=' + convert(nvarchar, menu_sub.authors_id) + '&sub_id=' + CONVERT(nvarchar, menu.sub_id) + '&id=' + convert(nvarchar, menu.id) DetailURL from " + _tableName + " tName left join menu on tName.menuID = menu.id left join menu_sub on menu.sub_id = menu_sub.id Where menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date and (menu.sub_id = @EventInformation or menu.sub_id = @EventArtistic) and (tName.Name like @Keyword or tName.Keyword like @Keyword) ORDER BY Id DESC) a ORDER BY Id;";
                                    try
                                    {
                                        using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@EventArtistic", getClassID(conn, 7));
                                            cmd.Parameters.AddWithValue("@EventInformation", getClassID(conn, 8));
                                            cmd.Parameters.AddWithValue("@Keyword", "%" + _keyword + "%");

                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string imgUrl = (reader["Picture1"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Picture1"].ToString(), _Lng, "Image"));
                                                        string detailURL = (reader["DetailURL"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["DetailURL"].ToString(), _Lng, ""));
                                                        EventsItem item = new EventsItem
                                                        {
                                                            ID = (reader["Id"] is DBNull) ? 0 : Convert.ToInt32(reader["Id"].ToString()),
                                                            Title = (reader["Name"] is DBNull) ? "" : reader["Name"].ToString(),
                                                            Brief = (reader["Description"] is DBNull) ? "" : reader["Description"].ToString(),
                                                            PicURL = imgUrl,
                                                            DetailURL = detailURL,
                                                            StartDay = (reader["Start"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["Start"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                            EndDay = (reader["End"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["End"]).ToString("yyyy-MM-dd HH:mm:ss")
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                        }
                                        dataList.Data = dataList.Data.OrderByDescending(o => ((EventsItem)o).ID).ToList();
                                        foreach (EventsItem item in dataList.Data)
                                        {
                                            item.Tag = new List<Tags>();
                                            item.Tag = getTags(conn, item.ID.ToString(), "active");
                                        }
                                        returnMsg = JsonConvert.SerializeObject(dataList);
                                    }
                                    catch (Exception ex)
                                    {
                                        returnMsg = ErrorMsg("error", ex.ToString(), "");
                                        //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                    }
                                    break;
                                }
                            #endregion
                            #region News
                            case "News":
                            case "NewsW":
                                {
                                    News items = JsonConvert.DeserializeObject<News>(_item);
                                    //SqlServer Select語法: 範圍 minValue ~ maxValue 
                                    selectString = @"SELECT menu.Start_Date [Start], menu.End_Date [End], menu.Ser_No,menu.popular,menu.id,menu.note_date,menu.title,menu.cont,menu.img1,'/index.asp?au_id=' + convert(nvarchar, menu_sub.authors_id) + '&sub_id=' + CONVERT(nvarchar, menu.sub_id) + '&id=' + convert(nvarchar, menu.id) DetailURL 
                                                     FROM Menu left join menu_sub on menu.sub_id = menu_sub.id 
                                                     Where menu.disp_opt = 'Y' and CONVERT([varchar](10),getdate(),(120)) between menu.start_date and menu.end_date and menu.sub_id = @NewsTypeID ";
                                    try
                                    {
                                        using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@NewsTypeID", getClassID(conn, 6));
                                            //cmd.Parameters.AddWithValue("@NewsTypeID", items.NewsTypeID);

                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string imgUrl = (reader["Img1"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Img1"].ToString(), _Lng, "Image"));
                                                        string detailURL = (reader["DetailURL"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["DetailURL"].ToString(), _Lng, ""));
                                                        NewsItem item = new NewsItem
                                                        {
                                                            Popular = (reader["Popular"] is DBNull) ? 0 : Convert.ToInt32(reader["Popular"].ToString()),
                                                            NewsID = (reader["ID"] is DBNull) ? 0 : Convert.ToInt32(reader["ID"].ToString()),
                                                            Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                                            Brief = (reader["Cont"] is DBNull) ? "" : Uri.EscapeDataString(reader["Cont"].ToString()),
                                                            PicURL = imgUrl,
                                                            DetailURL = detailURL,
                                                            Date = string.IsNullOrEmpty(reader["Note_Date"].ToString()) ? DateTime.Now.ToString("yyyy-MM-dd") : Convert.ToDateTime(reader["Note_Date"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                            Ser_No = (reader["Ser_No"] is DBNull) ? "-1" : reader["Ser_No"].ToString(),
                                                            StartDay = (reader["Start"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["Start"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                                            EndDay = (reader["End"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["End"]).ToString("yyyy-MM-dd HH:mm:ss")
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                        }
                                        dataList.Data = dataList.Data.OrderBy(o => ((NewsItem)o).Ser_No).ThenByDescending(o => ((NewsItem)o).NewsID).Take(_minValue + _maxValue).Skip(_minValue).ToList();
                                        if (type == "NewsW")
                                        {
                                            foreach (NewsItem item in dataList.Data)
                                            {
                                                item.Fram = new List<Frams>();
                                                item.Fram = getFences(conn, item.NewsID.ToString());
                                            }
                                        }
                                        returnMsg = JsonConvert.SerializeObject(dataList);
                                    }
                                    catch (Exception ex)
                                    {
                                        returnMsg = ErrorMsg("error", ex.ToString(), "");
                                        //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                    }
                                    break;
                                }
                            #endregion
                            #region Ads
                            case "Banners":
                                {
                                    Ads items = JsonConvert.DeserializeObject<Ads>(_item);
                                    selectString = "Select logo_img,weblink1,logo_img2,weblink2,logo_img3,weblink3,logo_img4,weblink4,logo_img5,weblink5 from menu_sub where id = @MenuID";

                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        if (items.ClassID != 0)
                                        {
                                            cmd.Parameters.AddWithValue("@MenuID", items.ClassID);
                                        }
                                        try
                                        {
                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    if (reader.Read())
                                                    {
                                                        string URL1 = (reader["logo_img"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["logo_img"].ToString(), _Lng, "Image"));
                                                        string URL2 = (reader["logo_img2"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["logo_img2"].ToString(), _Lng, "Image"));
                                                        string URL3 = (reader["logo_img3"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["logo_img3"].ToString(), _Lng, "Image"));
                                                        string URL4 = (reader["logo_img4"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["logo_img4"].ToString(), _Lng, "Image"));
                                                        string URL5 = (reader["logo_img5"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["logo_img5"].ToString(), _Lng, "Image"));

                                                        string Link1 = (reader["weblink1"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["weblink1"].ToString(), _Lng, "Link"));
                                                        string Link2 = (reader["weblink2"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["weblink2"].ToString(), _Lng, "Link"));
                                                        string Link3 = (reader["weblink3"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["weblink3"].ToString(), _Lng, "Link"));
                                                        string Link4 = (reader["weblink4"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["weblink4"].ToString(), _Lng, "Link"));
                                                        string Link5 = (reader["weblink5"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["weblink5"].ToString(), _Lng, "Link"));
                                                        
                                                        AdsItem item = new AdsItem()
                                                        {
                                                            URL1 = URL1,
                                                            URL2 = URL2,
                                                            URL3 = URL3,
                                                            URL4 = URL4,
                                                            URL5 = URL5,

                                                            Link1 = Link1,
                                                            Link2 = Link2,
                                                            Link3 = Link3,
                                                            Link4 = Link4,
                                                            Link5 = Link5
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                            returnMsg = JsonConvert.SerializeObject(dataList);
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                        }
                                    }
                                    break;
                                }
                            case "others":
                                {
                                    Ads items = JsonConvert.DeserializeObject<Ads>(_item);
                                    selectString = "SELECT TOP " + _maxValue + " *  FROM (SELECT TOP " + (_minValue + _maxValue) + " id, title, img, col9 [target], col1 link from menu_cont where type=3 and menu_id=@menuID ORDER BY Id DESC) a ORDER BY ID;";
                                    List<BaseLinkId> baseLinks = new List<BaseLinkId>();
                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@menuID", items.ClassID);
                                        try
                                        {
                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string imgUrl = (reader["img"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["img"].ToString(), _Lng, "Image"));
                                                        string link = (reader["link"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["link"].ToString(), _Lng, "Link"));
                                                        BaseLinkId item = new BaseLinkId()
                                                        {
                                                            Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                                            Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                                            Image = imgUrl,
                                                            Link = link,
                                                            Target = (reader["Target"] is DBNull) ? "" : reader["Target"].ToString()
                                                        };
                                                        baseLinks.Add(item);
                                                    }
                                                }
                                            }
                                            baseLinks = baseLinks.OrderByDescending(o => o.Id).ToList();
                                            foreach (BaseLink item in baseLinks.Select(o => new BaseLink() { Title = o.Title, Image = o.Image, Link = o.Link }).ToList())
                                            {
                                                dataList.Data.Add(item);
                                            }
                                            returnMsg = JsonConvert.SerializeObject(dataList);
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                        }
                                    }
                                    break;
                                }
                            case "FriendlyLink":
                                {
                                    Ads items = JsonConvert.DeserializeObject<Ads>(_item);
                                    selectString = "SELECT id, title, img, col9 [target], col1 as link from menu_cont where menu_id = @ClassID and type=1 ORDER BY Id;";
                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@ClassID", items.ClassID);
                                        try
                                        {
                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string imgUrl = (reader["img"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["img"].ToString(), _Lng, "Image"));
                                                        string link = (reader["link"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["link"].ToString(), _Lng, "Link"));
                                                        BaseLink item = new BaseLink()
                                                        {
                                                            Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                                            Image = imgUrl,
                                                            Link = link,
                                                            Target = (reader["Target"] is DBNull) ? "" : reader["Target"].ToString()
                                                        };
                                                        dataList.Data.Add(item);
                                                    }
                                                }
                                            }
                                            returnMsg = JsonConvert.SerializeObject(dataList);
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                        }
                                    }
                                    break;
                                }
                            case "HomeBanners":
                                {
                                    Ads items = JsonConvert.DeserializeObject<Ads>(_item);
                                    List<HomeBannersItem> HBList = new List<HomeBannersItem>();
                                    selectString = "Select defaultBenner1,defaultBenner2,defaultBenner3,defaultBenner4,defaultBenner5 from head;";
                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        try
                                        {
                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string imageUrl = "";
                                                        for (int i = 0; i < 5; i++)
                                                        {
                                                            HomeBannersItem HBItem = new HomeBannersItem();
                                                            HBItem.MenuCont = new List<MenuContent>();
                                                            if (i == 0)
                                                            {
                                                                imageUrl = (reader["defaultBenner1"] is DBNull) ? "" : reader["defaultBenner1"].ToString();
                                                            }
                                                            else if (i == 1)
                                                            {
                                                                imageUrl = (reader["defaultBenner2"] is DBNull) ? "" : reader["defaultBenner2"].ToString();
                                                            }
                                                            else if (i == 2)
                                                            {
                                                                imageUrl = (reader["defaultBenner3"] is DBNull) ? "" : reader["defaultBenner3"].ToString();
                                                            }
                                                            else if (i == 3)
                                                            {
                                                                imageUrl = (reader["defaultBenner4"] is DBNull) ? "" : reader["defaultBenner4"].ToString();
                                                            }
                                                            else if (i == 4)
                                                            {
                                                                imageUrl = (reader["defaultBenner5"] is DBNull) ? "" : reader["defaultBenner5"].ToString();
                                                            }
                                                            imageUrl = Uri.EscapeDataString(gs.GetAllLinkString(_orgName, imageUrl, _Lng, "Image"));
                                                            HBItem.Image = imageUrl;
                                                            HBList.Add(HBItem);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                            break;
                                        }
                                    }
                                    selectString = "Select menu_id,col3 [level],img,menu_cont.title,cont,col1 link,col9 [target],img_align algin,col7 leftOrRight,col8 [top],col5 [delay],col11 Duration,col4 [In-Animation] from menu_cont where [type]=9 and (menu_id = 1 or menu_id = 2 or menu_id = 3 or menu_id = 4 or menu_id = 5);";
                                    using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                    {
                                        try
                                        {
                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        string imgUrl = (reader["img"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["img"].ToString(), _Lng, "Image"));
                                                        string link = (reader["link"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["link"].ToString(), _Lng, "Link"));
                                                        MenuContent item = new MenuContent()
                                                        {
                                                            Level = (reader["img"] is DBNull) ? 0 : Convert.ToInt32(reader["Level"].ToString()),
                                                            Image = Uri.EscapeDataString(imgUrl),
                                                            Title = (reader["img"] is DBNull) ? "" : reader["Title"].ToString(),
                                                            Cont = (reader["img"] is DBNull) ? "" : Uri.EscapeDataString(reader["Cont"].ToString()),
                                                            Link = Uri.EscapeDataString(link),
                                                            Target = (reader["img"] is DBNull) ? "" : reader["Target"].ToString(),
                                                            Width = "",
                                                            Height = "",
                                                            Align = (reader["Algin"] is DBNull) ? "" : reader["Algin"].ToString(),
                                                            LeftOrRight = (reader["LeftOrRight"] is DBNull) ? 0 : Convert.ToInt32(reader["LeftOrRight"].ToString()),
                                                            Top = (reader["Top"] is DBNull) ? 0 : Convert.ToInt32(reader["Top"].ToString()),
                                                            Delay = (reader["Delay"] is DBNull) ? 0 : Convert.ToInt32(reader["Delay"].ToString()),
                                                            Duration = (reader["Duration"] is DBNull) ? 0 : Convert.ToInt32(reader["Duration"].ToString()),
                                                            InAnimation = (reader["In-Animation"] is DBNull) ? 0 : Convert.ToInt32(reader["In-Animation"].ToString())
                                                        };
                                                        if (reader["menu_id"].ToString() == "1")
                                                        {
                                                            HBList[0].Width = "";
                                                            HBList[0].Height = "";
                                                            HBList[0].MenuCont.Add(item);
                                                        }
                                                        else if (reader["menu_id"].ToString() == "2")
                                                        {
                                                            HBList[1].Width = "";
                                                            HBList[1].Height = "";
                                                            HBList[1].MenuCont.Add(item);
                                                        }
                                                        else if (reader["menu_id"].ToString() == "3")
                                                        {
                                                            HBList[2].Width = "";
                                                            HBList[2].Height = "";
                                                            HBList[2].MenuCont.Add(item);
                                                        }
                                                        else if (reader["menu_id"].ToString() == "4")
                                                        {
                                                            HBList[3].Width = "";
                                                            HBList[3].Height = "";
                                                            HBList[3].MenuCont.Add(item);
                                                        }
                                                        else if (reader["menu_id"].ToString() == "5")
                                                        {
                                                            HBList[4].Width = "";
                                                            HBList[4].Height = "";
                                                            HBList[4].MenuCont.Add(item);
                                                        }
                                                        
                                                    }
                                                }
                                            }
                                            foreach(HomeBannersItem item in HBList)
                                            {
                                                dataList.Data.Add(item);
                                            }
                                            returnMsg = JsonConvert.SerializeObject(dataList);
                                        }
                                        catch (Exception ex)
                                        {
                                            returnMsg = ErrorMsg("error", ex.ToString(), "");
                                            //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                        }
                                    }
                                    break;
                                }
                            #endregion
                            #endregion
                            default:
                                {
                                    returnMsg = ErrorMsg("error", "Type不存在", "");
                                    break;
                                }
                        }
                        conn.Close();
                    }
                    break;
                case 1:
                    returnMsg = ErrorMsg("error", "Token不存在", "");
                    break;
                case 2:
                    returnMsg = ErrorMsg("error", "Token權限出問題", "");
                    break;
            }
            return returnMsg;
        }


        public List<Tags> getTags(SqlConnection conn, string SourceId, string Type)
        {
            List<Tags> tags = new List<Tags>();
            string selectString = "sp_getTag";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", SourceId);
                cmd.Parameters.AddWithValue("@type", Type);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Tags item = new Tags()
                                {
                                    Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                    Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                    Tg_Id = (reader["Tg_Id"] is DBNull) ? "" : reader["Tg_Id"].ToString()
                                };
                                tags.Add(item);
                            }
                        }
                    }
                }
                catch
                {
                    tags.Clear();
                }
            }
            return tags;
        }
        /// <summary>
        /// Get Frams by Menu ID
        /// </summary>
        /// <param name="conn">Connecting String</param>
        /// <param name="SourceId">Menu Id</param>
        /// <returns></returns>
        public List<Frams> getFences(SqlConnection conn, string SourceId)
        {
            List<Frams> Fram = new List<Frams>();
            /*string selectString = @"SELECT Fence.Id,Fence.colNum cNo, menu_cont.Img, menu_cont.img_align Align, menu_cont.Title, menu_cont.col1 Link, menu_cont.col2 Range, menu_cont.col9 Target, menu_cont.cont 
                                    FROM Menu_cont inner join Fence on Menu_cont.menu_id = Fence.id 
                                    Where menu_cont.[Type] = 13 and Fence.id in (
                                        SELECT Fence.Id
                                        FROM (Menu left join ShopInfo on Menu.id = ShopInfo.menuID) left join Fence on ShopInfo.Id = Fence.cid 
                                        Where Menu.disp_opt = 'Y' and Fence.disp_opt = 'Y' and Fence.[Type] = 11 and Menu.id = @SourceId
                                    ) Order By Id";*/
            string selectString = @"Select Menu_Cont.disp_opt MCD, Fence.Id,Fence.colNum cNo, menu_cont.Img, menu_cont.img_align Align, menu_cont.Title, menu_cont.col1 Link, menu_cont.col2 Range, menu_cont.col9 Target, menu_cont.cont 
                                    From menu left join ShopInfo on menu.id = ShopInfo.menuID
		                                      left join Fence on ShopInfo.id = Fence.cid and Fence.[type] = 11
		                                      left join Menu_Cont on Fence.id = Menu_cont.menu_id and Menu_cont.[type] = 13
                                    Where Menu.disp_opt = 'Y' and Fence.disp_opt = 'Y' and Menu_Cont.disp_opt != 'P' and Menu.id = @SourceId
                                    Order By Id";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@SourceId", SourceId);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            Frams item = new Frams();
                            item.Cont = new List<Content>();
                            int iCount = -1;
                            while (reader.Read())
                            {
                                int iCurrent = Convert.ToInt32(reader["Id"].ToString());
                                iCount = (iCount < 0) ? iCurrent : iCount;
                                if (iCount != iCurrent)
                                {
                                    Fram.Add(item);
                                    item = new Frams();
                                    item.Cont = new List<Content>();
                                    iCount = -1;
                                }
                                item.cNo = (reader["cNo"] is DBNull) ? "0" : reader["cNo"].ToString();
                                if (((reader["MCD"] is DBNull) ? "N" : reader["MCD"].ToString()) == "Y")
                                {
                                    string imgUrl = (reader["Img"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Img"].ToString(), _Lng, "Image"));
                                    string Link = (reader["Link"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Link"].ToString(), _Lng, "Link"));
                                    Content obj = new Content()
                                    {
                                        Image = imgUrl,
                                        Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                        Range = (reader["Range"] is DBNull) ? "" : reader["Range"].ToString(),
                                        Link = Link,
                                        Target = (reader["Target"] is DBNull) ? "" : reader["Target"].ToString(),
                                        Align = (reader["Align"] is DBNull) ? "" : reader["Align"].ToString(),
                                        Cont = (reader["Cont"] is DBNull) ? "" : Uri.EscapeDataString(reader["Cont"].ToString())
                                    };
                                    item.Cont.Add(obj);
                                }
                            }
                            Fram.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Fram.Clear();
                    Frams item = new Frams()
                    {
                        cNo = ex.ToString()
                    };

                    Fram.Add(item);
                }
            }

            return Fram;
        }
        public List<Content> getContents(SqlConnection conn, string SourceId)
        {
            List<Content> Cont = new List<Content>();
            string selectString = @"SELECT menu_cont.Img, menu_cont.img_align Align, menu_cont.Title, menu_cont.col1 Link, menu_cont.col2 Range, menu_cont.col9 Target, menu_cont.cont 
                                    FROM Menu_cont inner join Fence on Menu_cont.menu_id = Fence.id 
                                    Where menu_cont.[Type] = 13 and Fence.id = @SourceId";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@SourceId", SourceId);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string imgUrl = (reader["Img"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Img"].ToString(), _Lng, "Image"));
                                string Link = (reader["Link"] is DBNull) ? "" : Uri.EscapeDataString(gs.GetAllLinkString(_orgName, reader["Link"].ToString(), _Lng, "Link"));
                                Content item = new Content()
                                {
                                    Image = imgUrl,
                                    Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                    Range = (reader["Range"] is DBNull) ? "" : reader["Range"].ToString(),
                                    Link = Link,
                                    Target = (reader["Target"] is DBNull) ? "" : reader["Target"].ToString(),
                                    Align = (reader["Align"] is DBNull) ? "" : reader["Align"].ToString(),
                                    Cont = (reader["Cont"] is DBNull) ? "" : Uri.EscapeDataString(reader["Cont"].ToString())
                                };
                                Cont.Add(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Cont.Clear();
                    Content item = new Content()
                    {
                        Title = ex.ToString()
                    };
                    Cont.Add(item);
                }
            }

            return Cont;
        }

        public string getMenuID(SqlConnection conn, string SourceId, string type)
        {
            string itemId = "", tableName = "";
            if (type == "attra")
            {
                tableName = "attractions";
            }
            else if (type == "shop")
            {
                tableName = "shop";
            }
            else if (type == "hotel")
            {
                tableName = "hotel";
            }
            else if (type == "active")
            {
                tableName = "active";
            }
            else
            {
                return "";
            }
            string selectString = "SELECT MenuID FROM " + tableName + " Where Id = @SourceId;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@SourceId", SourceId);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            itemId = reader[0].ToString();
                        }
                    }
                }
                catch
                {
                    itemId = "";
                }
            }
            return itemId;
        }
        public string getClassID(SqlConnection conn, int type)
        {
            string itemId = "";
            string selectString = "SELECT tName.bindID FROM searchRelation tName inner join searchClass tName2 on tName.ClassID = tName2.Id Where ClassID = @ClassID;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@ClassID", type);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            itemId = reader[0].ToString();
                        }
                    }
                }
                catch
                {
                    itemId = "";
                }
            }
            return itemId;
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "CommandSetting error", "", RspnMsg);
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "認證與取得資料"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", "CommandSetting.ashx"));

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
        #region ContextMessager
        private string ContextMessager(int state, string cuState, string TripId)
        {
            if (state == 0)
            {
                ContextSucessMessager context = new ContextSucessMessager();
                context.Data = new List<object>();
                ContextTripID contextTripID = new ContextTripID()
                {
                    TripID = TripId
                };
                context.Data.Add(contextTripID);
                return JsonConvert.SerializeObject(context);
            }
            else
            {
                ContextErrorMessager context = new ContextErrorMessager()
                {
                    RspnMsg = cuState
                    //RspnMsg = ((CUstate == 2) ? "資料有誤" : ((CUstate == 1) ? "儲存失敗" : "更新失敗"))
                    //RspnMsg = "刪除失敗"
                };
                return JsonConvert.SerializeObject(context);
            }
        }
        private string ContextMessager2(int state, string cuState, string Id)
        {
            if (state == 0)
            {
                ContextSucessMessager context = new ContextSucessMessager();
                context.Data = new List<object>();
                ContextID contextID = new ContextID()
                {
                    ID = Id
                };
                context.Data.Add(contextID);
                return JsonConvert.SerializeObject(context);
            }
            else
            {
                ContextErrorMessager context = new ContextErrorMessager()
                {
                    RspnMsg = cuState
                };
                return JsonConvert.SerializeObject(context);
            }
        }
        #endregion
    }

    public class DataListSetting
    {
        public List<object> Data { get; set; }
    }
    public class DataCounts
    {
        public int TotalCounts { get; set; }
    }
    public class DataCountsInput
    {
        public string AreaID { get; set; }
        public int ClassID { get; set; }
    }
    public class simpleDataDescription
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Brief { get; set; }
        public string Name_ch { get; set; }
        public string PicURL { get; set; }
        public double Px { get; set; }
        public double Py { get; set; }
        public string DetailURL { get; set; }
        public int Duration { get; set; }
        public int Popular { get; set; }
        public List<Tags> Tag { get; set; }
    }
    public class ContextErrorMessager
    {
        public String RspnCode = "error";
        public String RspnMsg { get; set; }
        //public String Token { get; set; }
    }
    public class ContextSucessMessager
    {
        public String RspnCode = "sucess";
        public List<object> Data { get; set; }
        //public String Token { get; set; }
    }
    public class ContextTripID
    {
        public String TripID { get; set; }
    }
    public class ContextID
    {
        public String ID { get; set; }
    }
    public class Tags
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Tg_Id { get; set; }
    }
    public class Frams
    {
        public string cNo { get; set; }
        public List<Content> Cont { get; set; }
    }
    public class Content
    { 
        public string Image { get; set; }
        public string Title { get; set; }
        public string Range { get; set; }
        public string Link { get; set; }
        public string Target { get; set; }
        public string Align { get; set; }
        public string Cont { get; set; }
    }
    public class BaseLink
    {
        public string Title { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }
        public string Target { get; set; }
    }
    public class BaseLinkWithContent
    {
        public string Title { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }
        public string Brief { get; set; }
    }
    public class BaseLinkId : BaseLink
    {
        public string Id { get; set; }
    }
    public class BaseLinkIdWithContent : BaseLinkWithContent
    {
        public string Id { get; set; }
    }
}