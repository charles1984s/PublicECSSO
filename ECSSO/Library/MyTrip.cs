using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace ECSSO.Library
{
    public class MyTrip
    {
        public string AreaID { get; set; }
        public string MemID { get; set; }
        public string TripID { get; set; }
    }
    public class MyTripItem
    {
        public string ID { get; set; }
        public string AreaID { get; set; }
        public string MemID { get; set; }
        public string Title { get; set; }
        public string Remark { get; set; }
        public string PicURL { get; set; }
        public int NodeNum { get; set; }
        public int TotalTime { get; set; }
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public int Transportation { get; set; }
        //public int Recommendation { get; set; }
    }
    public class detailMyTripItem
    {
        public string TripID { get; set; }
        public int TripDay { get; set; }
        public string StartTime { get; set; }
        /*public string Remark { get; set; }
        public string Title { get; set; }
        public string Img { get; set; }*/
        public List<detailTripNode> TripNodes { get; set; }
    }
    public class detailTripNode
    {
        public int NodeID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int Nth_day { get; set; }
        public string tripDateTime { get; set; }
        public int sn { get; set; }
        public int Duration { get; set; }
        public int RouteTime { get; set; }
        public string RouteDetail { get; set; }
        public int Transportation { get; set; }
        public float Px { get; set; }
        public float Py { get; set; }
        public string Img { get; set; }
    }


    #region CRUD
    public class MyTripInstance
    {
        private GetStr gs = new GetStr();
        private string _tripID = "";
        public string getCurrentTripId
        {
            get
            {
                return _tripID;
            }
            private set { }
        }
        public bool IsCheckedTripByTripID(SqlConnection conn, string TripID)
        {
            bool state = false;
            if (gs.CheckStringIsNotNull(TripID) == "") return state;
            string selectString = "SELECT Trip.* "
                                + "from trip "
                                + "where trip.Id=@TripID";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@TripID", TripID);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            state = true;
                        }
                        else
                        {
                            state = false;
                        }
                    }
                }
                catch
                {
                    state = false;
                }
            }

            return state;
        }
        public string getLastTripId(SqlConnection conn, bool isDesc)
        {
            string itemId = "";
            string selectString = "SELECT top 1 Id FROM Trip order by Id";
                   selectString += (isDesc) ? " desc;" : ";";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _tripID = itemId = reader[0].ToString();
                        }
                    }
                }
                catch
                {
                    _tripID = itemId = "";
                }
            }
            return itemId;
        }
        public string getMemberIDbyTrip(SqlConnection conn, string TripID)
        {
            string itemId = null;
            string selectString = "SELECT mem_id FROM Trip "
                                + "Where Id = @Id;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@Id", TripID);
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
                    itemId = null;
                }
            }
            return itemId;
        }
        /*public int copybackTripData(SqlConnection conn, List<detailMyTripItem> Source, string TripId)
        {
            DeleteTripDataByTripId(conn, TripId);
            if (Source != null || Source.Count > 0)
            {
                foreach (detailMyTripItem oldItem in Source)
                {
                    CreateTripData(conn, oldItem);
                }
            }
            return 1;
        }*/
        /*public int copybackTripData(SqlConnection conn, detailMyTripItem Source, string TripId, int sn)
        {
            DeleteTripDataByTripId(conn, TripId, sn);
            if (Source != null)
            {
                CreateTripData(conn, Source);
            }
            return 1;
        }*/
        /// <summary>
        /// checkMemberID， Return Statement
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="MemID"></param>
        /// <returns>0: Has Null MemID、1: Has MemID、2: No MemID、3: Something Error.</returns>
        public int checkMemberID(SqlConnection conn, string MemID)
        {
            int state = 0;
            if (gs.CheckStringIsNotNull(MemID) == "")
            {
                using (SqlCommand cmd = new SqlCommand("SELECT trip.id from trip where mem_id=@mem_id", conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                state = 0;
                            }
                            else
                            {
                                state = 2;
                            }
                        }
                    }
                    catch
                    {
                        state = 3;
                    }
                }
            }
            else
            {
                using (SqlCommand cmd = new SqlCommand("select id from Cust where mem_id=@mem_id", conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                state = 1;
                            }
                            else
                            {
                                state = 2;
                            }
                        }
                    }
                    catch
                    {
                        state = 3;
                    }
                }
            }
            return state;
        }

        #region MyTrip CRUD
        public int CreateTrip(SqlConnection conn, MyTripItem obj)
        {
            int state = 0;

            //string dtNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string selectString = "INSERT INTO Trip (mem_id, AreaID, title, remark, amount, days, disp_opt, img, StartDay, EndDay, Transportation) " //, Recommendation) "
                                + "Values(@mem_id, @AreaID, @title, @remark, @amount, @days, 'Y', @img, @StartDay, @EndDay, @Transportation);" //, @Recommendation);"
                                + "Select scope_identity();";
            
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@mem_id", gs.CheckStringIsNotNull(obj.MemID));
                cmd.Parameters.AddWithValue("@AreaID", gs.CheckStringIsNotNull(obj.AreaID));
                cmd.Parameters.AddWithValue("@title", gs.CheckStringIsNotNull(obj.Title));
                cmd.Parameters.AddWithValue("@remark", gs.CheckStringIsNotNull(obj.Remark));
                cmd.Parameters.AddWithValue("@amount", obj.NodeNum);
                cmd.Parameters.AddWithValue("@days", obj.TotalTime);
                cmd.Parameters.AddWithValue("@img", gs.CheckStringIsNotNull(obj.PicURL));
                cmd.Parameters.AddWithValue("@StartDay", gs.CheckStringIsNotNull(obj.StartDay));
                cmd.Parameters.AddWithValue("@EndDay", gs.CheckStringIsNotNull(obj.EndDay));
                cmd.Parameters.AddWithValue("@Transportation", obj.Transportation);
                //cmd.Parameters.AddWithValue("@Recommendation", obj.Recommendation);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                _tripID = reader[0].ToString();
                            }
                        }
                    }
                }
                catch
                {
                    state = 1;
                }
            }
            return state;
        }
        public List<MyTripItem> SelectTrip(SqlConnection conn, string MemID)
        {
            MemID = (gs.CheckStringIsNotNull(MemID) == "") ? "" : MemID;
            List<MyTripItem> myTripList = new List<MyTripItem>();
            string selectString = "SELECT trip.* "
                                + "from trip "
                                + "where trip.mem_id=@MemID";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@MemID", MemID);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MyTripItem myTrip = new MyTripItem()
                            {
                                ID = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                AreaID = (reader["AreaID"] is DBNull) ? "" : reader["AreaID"].ToString(),
                                MemID = (reader["mem_id"] is DBNull) ? "" : reader["mem_id"].ToString(),
                                Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                Remark = (reader["Remark"] is DBNull) ? "" : reader["Remark"].ToString(),
                                NodeNum = (reader["Amount"] is DBNull) ? 0 : Convert.ToInt32(reader["Amount"].ToString()),
                                TotalTime = (reader["Days"] is DBNull) ? 0 : Convert.ToInt32(reader["Days"].ToString()),
                                Transportation = (reader["Transportation"] is DBNull) ? 0 : Convert.ToInt32(reader["Transportation"].ToString()),
                                PicURL = (reader["img"] is DBNull) ? "" : reader["img"].ToString(),
                                //Recommendation = (reader["Recommendation"] is DBNull) ? 0 : Convert.ToInt32(reader["Recommendation"].ToString()),
                                StartDay = (reader["startDay"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["startDay"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                EndDay = (reader["endDay"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["endDay"]).ToString("yyyy-MM-dd HH:mm:ss")
                            };
                            myTripList.Add(myTrip);
                        }
                    }
                }
                catch
                {
                    myTripList.Clear();
                }
            }

            return myTripList;
        }
        public MyTripItem SelectTripByOne(SqlConnection conn, string TripID)
        {
            MyTripItem Trip = new MyTripItem();
            string selectString = "SELECT trip.* "
                                + "from trip "
                                + "where trip.id=@TripID";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@TripID", TripID);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MyTripItem myTrip = new MyTripItem()
                            {
                                ID = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                AreaID = (reader["AreaID"] is DBNull) ? "" : reader["AreaID"].ToString(),
                                MemID = (reader["mem_id"] is DBNull) ? "" : reader["mem_id"].ToString(),
                                Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                Remark = (reader["Remark"] is DBNull) ? "" : reader["Remark"].ToString(),
                                NodeNum = (reader["Amount"] is DBNull) ? 0 : Convert.ToInt32(reader["Amount"].ToString()),
                                TotalTime = (reader["Days"] is DBNull) ? 0 : Convert.ToInt32(reader["Days"].ToString()),
                                Transportation = (reader["Transportation"] is DBNull) ? 0 : Convert.ToInt32(reader["Transportation"].ToString()),
                                PicURL = (reader["img"] is DBNull) ? "" : reader["img"].ToString(),
                                //Recommendation = (reader["Recommendation"] is DBNull) ? 0 : Convert.ToInt32(reader["Recommendation"].ToString()),
                                StartDay = (reader["startDay"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["startDay"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                EndDay = (reader["endDay"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["endDay"]).ToString("yyyy-MM-dd HH:mm:ss")
                            };
                            Trip = myTrip;
                        }
                    }
                }
                catch
                {
                    Trip = null;
                }
            }

            return Trip;
        }
        public int UpdateTrip(SqlConnection conn, MyTripItem obj)
        {
            int state = 0;
            if (gs.CheckStringIsNotNull(obj.MemID) == "")
            {
                return 1;
            }

            string selectString = "UPDATE Trip "
                                + " SET AreaID = @AreaID, Title = @Title, Remark = @Remark";
            selectString += ", Amount = @Amount, Days = @Days, img = @img, startDay = @startDay, endDay = @endDay, Transportation = @Transportation"//, Recommendation = @Recommendation"
                         + " FROM Trip"
                         + " Where mem_id = @MemID and Id = @TripID;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@AreaID", gs.CheckStringIsNotNull(obj.AreaID));
                cmd.Parameters.AddWithValue("@Title", gs.CheckStringIsNotNull(obj.Title));
                cmd.Parameters.AddWithValue("@Remark", gs.CheckStringIsNotNull(obj.Remark));
                cmd.Parameters.AddWithValue("@Amount", obj.NodeNum);
                cmd.Parameters.AddWithValue("@Days", obj.TotalTime);
                cmd.Parameters.AddWithValue("@img", gs.CheckStringIsNotNull(obj.PicURL));
                cmd.Parameters.AddWithValue("@startDay", obj.StartDay);
                cmd.Parameters.AddWithValue("@endDay", obj.EndDay);
                cmd.Parameters.AddWithValue("@Transportation", obj.Transportation);
                //cmd.Parameters.AddWithValue("@Recommendation", obj.Recommendation);

                cmd.Parameters.AddWithValue("@MemID", obj.MemID);
                cmd.Parameters.AddWithValue("@TripID", obj.ID);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = 1;
                }
            }
            return state;
        }
        public int DeleteTripById(SqlConnection conn, string TripId)
        {
            int state = 0;
            string selectString = "Delete From Trip "
                                + "Where Id = @Id;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@Id", TripId);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = 1;
                }
            }
            return state;
        }
        #endregion
        #region MyTripData CRUD
        public int CreateTripNodes(SqlConnection conn, detailTripNode obj, string TripID)
        {
            int state = 0;
            string selectString = "INSERT INTO TripData (ser_no, nodeID, tripID, Type, duration, routeDetail, TripDateTime, routeTime, nodeTitle, Img, Nth_day, Transportation, x, y) "
                                + "Values(@sn, @nodeID, @tripID, @Type, @duration, @routeDetail, @TripDateTime, @routeTime, @Name, @Img, @Nth_day, @Transportation, @Px, @Py);";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@sn", obj.sn);
                cmd.Parameters.AddWithValue("@nodeID", obj.NodeID);
                cmd.Parameters.AddWithValue("@tripID", gs.CheckStringIsNotNull(TripID));
                cmd.Parameters.AddWithValue("@Type", gs.CheckStringIsNotNull(obj.Type));
                cmd.Parameters.AddWithValue("@duration", obj.Duration);
                cmd.Parameters.AddWithValue("@routeDetail", gs.CheckStringIsNotNull(obj.RouteDetail));
                cmd.Parameters.AddWithValue("@TripDateTime", gs.CheckStringIsNotNull(obj.tripDateTime));
                cmd.Parameters.AddWithValue("@routeTime", obj.RouteTime);
                cmd.Parameters.AddWithValue("@Name", gs.CheckStringIsNotNull(obj.Name));
                cmd.Parameters.AddWithValue("@Img", gs.CheckStringIsNotNull(obj.Img));
                cmd.Parameters.AddWithValue("@Nth_day", obj.Nth_day);
                cmd.Parameters.AddWithValue("@Transportation", obj.Transportation);
                cmd.Parameters.AddWithValue("@Px", obj.Px);
                cmd.Parameters.AddWithValue("@Py", obj.Py);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = 1;
                }
            }
            return state;
        }
        public int CreateTripDay(SqlConnection conn, detailMyTripItem obj)
        {
            int state = 0;
            if (gs.CheckStringIsNotNull(obj.TripID) == "")
            {
                return 1;
            }
            string selectString = "INSERT INTO TripDay (tripID, tripDay, startTime) "
                                + "Values(@tripID, @tripDay, @startTime);";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@tripID", obj.TripID);
                cmd.Parameters.AddWithValue("@tripDay", obj.TripDay);
                cmd.Parameters.AddWithValue("@startTime", obj.StartTime);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = 1;
                }
            }
            
            return state;
        }
        public int CreateTripData(SqlConnection conn, detailMyTripItem obj)
        {
            int state = 0;
            if (gs.CheckStringIsNotNull(obj.TripID) == "")
            {
                return 1;
            }
            if (CreateTripDay(conn, obj) == 0)
            {
                for (int i = 0; i < obj.TripNodes.Count || state == 1; i++)
                {
                    state = CreateTripNodes(conn, obj.TripNodes[i], obj.TripID);
                }
            }
            else
            {
                state = 1;
            }
            return state;
        }
        public List<detailMyTripItem> SelectTripData(SqlConnection conn, string MemID, string TripID)
        {
            MemID = (gs.CheckStringIsNotNull(MemID) == "") ? "" : MemID;
            List<detailMyTripItem> myTripItemList = new List<detailMyTripItem>();
            if (gs.CheckStringIsNotNull(TripID) == "") return myTripItemList;
            string selectString = "Select MyTrip.*, TripDay, StartTime, Trip.Title, Trip.Remark, Trip.Img, '/index.asp?au_id='+CONVERT(nvarchar,menu_sub.authors_id)+'&sub_id='+CONVERT(nvarchar,menu.sub_id)+'&id='+CONVERT(nvarchar,menu.id) link From ("
                                + "select TripData.*, active.menuID, active.Name, active.Description from TripData inner join active on TripData.nodeID = active.id where[TYPE] = 1 "
                                + " union all "
                                + "select TripData.*, shop.menuID, shop.Name, shop.Description from TripData inner join shop on TripData.nodeID = shop.id where[TYPE] = 2 or [TYPE] = 5 "
                                + " union all "
                                + "select TripData.*, hotel.menuID, hotel.Name, hotel.Description from TripData inner join hotel on TripData.nodeID = hotel.id where[TYPE] = 3 "
                                + " union all "
                                + "select TripData.*, attractions.menuID, attractions.Name, attractions.Description from TripData inner join attractions on TripData.nodeID = attractions.id where[TYPE] = 4 "
                                + ") as myTrip inner join TripDay on TripDay.TripID = myTrip.TripID and TripDay = Nth_Day left join Trip on TripDay.TripID = Trip.id left join (menu left join menu_sub on menu.sub_id = menu_sub.id) on myTrip.menuID = menu.id "
                                + "Where TripDay.TripID = @TripID and Menu.disp_opt = 'Y' and Trip.Mem_Id = @MemID "
                                + "Order By TripID, TripDay, Ser_No,id";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@TripID", TripID);
                cmd.Parameters.AddWithValue("@MemID", MemID);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            detailMyTripItem MyTripitem = new detailMyTripItem();
                            MyTripitem.TripNodes = new List<detailTripNode>();
                            int iCurrent = 1;
                            while (reader.Read())
                            {
                                int iCount = Convert.ToInt32(reader["TripDay"].ToString());
                                if (iCurrent != iCount)
                                {
                                    myTripItemList.Add(MyTripitem);
                                    MyTripitem = new detailMyTripItem();
                                    MyTripitem.TripNodes = new List<detailTripNode>();
                                    iCurrent++;
                                }
                                MyTripitem.TripID = (reader["TripID"] is DBNull) ? "" : reader["TripID"].ToString();
                                MyTripitem.TripDay = iCount;
                                MyTripitem.StartTime = (reader["StartTime"] is DBNull) ? DateTime.Now.ToString("HH:mm:ss") : reader["StartTime"].ToString();
                                /*item.Remark = reader["Remark"].ToString();
                                item.Title = reader["Title"].ToString();
                                item.Img = reader[23].ToString();*/
                                detailTripNode item = new detailTripNode()
                                {
                                    sn = (reader["ser_no"] is DBNull) ? 0 : Convert.ToInt32(reader["ser_no"].ToString()),
                                    tripDateTime = (reader["TripDateTime"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["TripDateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                    NodeID = (reader["nodeID"] is DBNull) ? 0 : Convert.ToInt32(reader["nodeID"].ToString()),
                                    Duration = (reader["duration"] is DBNull) ? 0 : Convert.ToInt32(reader["duration"].ToString()),
                                    Nth_day = (reader["Nth_day"] is DBNull) ? 0 : Convert.ToInt32(reader["Nth_day"].ToString()),
                                    RouteDetail = (reader["routeDetail"] is DBNull) ? "" : reader["routeDetail"].ToString(),
                                    RouteTime = (reader["routeTime"] is DBNull) ? 0 : Convert.ToInt32(reader["routeTime"].ToString()),
                                    Name = (reader["nodeTitle"] is DBNull) ? "" : reader["nodeTitle"].ToString(),
                                    Img = (reader[14] is DBNull) ? "" : HttpContext.Current.Server.UrlEncode(gs.GetAllLinkString("", reader[14].ToString(), "", "Image")),
                                    Type = (reader["Type"] is DBNull) ? "" : reader["Type"].ToString(),
                                    Transportation = (reader["Transportation"] is DBNull) ? 0 : Convert.ToInt32(reader["Transportation"].ToString()),
                                    Px = (reader["x"] is DBNull) ? 0F : Convert.ToSingle(reader["x"].ToString()),
                                    Py = (reader["y"] is DBNull) ? 0F : Convert.ToSingle(reader["y"].ToString())
                                };
                                MyTripitem.TripNodes.Add(item);
                            }
                            myTripItemList.Add(MyTripitem);
                        }
                    }
                }
                catch
                {
                    myTripItemList.Clear();
                }
            }
            return myTripItemList;
        }
        public detailMyTripItem SelectTripData(SqlConnection conn, string MemID, string TripID, string TripDay)
        {
            MemID = (gs.CheckStringIsNotNull(MemID) == "") ? "" : MemID;
            detailMyTripItem MyTripitem = null;
            if (gs.CheckStringIsNotNull(TripID) == "") return MyTripitem;
            string selectString = "Select MyTrip.*, TripDay, StartTime, Trip.Title, Trip.Remark, Trip.Img From ("
                                + "select TripData.*, active.menuID, active.Name, active.Description from TripData inner join active on TripData.nodeID = active.id "
                                + " union all "
                                + "select TripData.*, shop.menuID, shop.Name, shop.Description from TripData inner join shop on TripData.nodeID = shop.id "
                                + " union all "
                                + "select TripData.*, hotel.menuID, hotel.Name, hotel.Description from TripData inner join hotel on TripData.nodeID = hotel.id "
                                + " union all "
                                + "select TripData.*, attractions.menuID, attractions.Name, attractions.Description from TripData inner join attractions on TripData.nodeID = attractions.id "
                                + ") as myTrip inner join TripDay on TripDay.TripID = myTrip.TripID and TripDay = Nth_Day left join Trip on TripDay.TripID = Trip.id left join (menu left join menu_sub on menu.sub_id = menu_sub.id) on myTrip.menuID = menu.id "
                                + "Where TripDay.TripID = @TripID and Menu.disp_opt = 'Y' and Trip.Mem_Id = @MemID and TripDay = @TripDay "
                                + "Order By TripID, TripDay, Ser_No";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@TripID", TripID);
                cmd.Parameters.AddWithValue("@MemID", MemID);
                cmd.Parameters.AddWithValue("@TripDay", TripDay);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            MyTripitem = new detailMyTripItem();
                            MyTripitem.TripNodes = new List<detailTripNode>();
                            while (reader.Read())
                            {
                                MyTripitem.TripID = (reader["TripID"] is DBNull) ? "" : reader["TripID"].ToString();
                                MyTripitem.TripDay = (reader["TripDay"] is DBNull) ? 0 : Convert.ToInt32(reader["TripDay"].ToString());
                                MyTripitem.StartTime = (reader["StartTime"] is DBNull) ? DateTime.Now.ToString("HH:mm:ss") : reader["StartTime"].ToString();
                                /*item.Remark = reader["Remark"].ToString();
                                item.Title = reader["Title"].ToString();
                                item.Img = reader[23].ToString();*/
                                detailTripNode item = new detailTripNode()
                                {
                                    sn = (reader["ser_no"] is DBNull) ? 0 : Convert.ToInt32(reader["ser_no"].ToString()),
                                    tripDateTime = (reader["TripDateTime"] is DBNull) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : Convert.ToDateTime(reader["TripDateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                    NodeID = (reader["nodeID"] is DBNull) ? 0 : Convert.ToInt32(reader["nodeID"].ToString()),
                                    Duration = (reader["duration"] is DBNull) ? 0 : Convert.ToInt32(reader["duration"].ToString()),
                                    Nth_day = (reader["Nth_day"] is DBNull) ? 0 : Convert.ToInt32(reader["Nth_day"].ToString()),
                                    RouteDetail = (reader["routeDetail"] is DBNull) ? "" : reader["routeDetail"].ToString(),
                                    RouteTime = (reader["routeTime"] is DBNull) ? 0 : Convert.ToInt32(reader["routeTime"].ToString()),
                                    Name = (reader["nodeTitle"] is DBNull) ? "" : reader["nodeTitle"].ToString(),
                                    Img = (reader[14] is DBNull) ? "" : HttpContext.Current.Server.UrlEncode(gs.GetAllLinkString("", reader[14].ToString(), "", "Image")),
                                    Type = (reader["Type"] is DBNull) ? "" : reader["Type"].ToString(),
                                    Transportation = (reader["Transportation"] is DBNull) ? 0 : Convert.ToInt32(reader["Transportation"].ToString()),
                                    Px = (reader["x"] is DBNull) ? 0F : Convert.ToSingle(reader["x"].ToString()),
                                    Py = (reader["y"] is DBNull) ? 0F : Convert.ToSingle(reader["y"].ToString())
                                };
                                MyTripitem.TripNodes.Add(item);
                            }
                        }
                    }
                }
                catch
                {
                    MyTripitem = null;
                }
            }
            return MyTripitem;
        }
        public int DeleteTripDataByTripId(SqlConnection conn, string TripId)
        {
            int state = 0;
            string selectString = "Delete From TripData "
                                + "Where tripId = @TripId;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@TripId", TripId);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = 1;
                }
            }
            return state;
        }
        public int DeleteTripDayByTripId(SqlConnection conn, string TripId)
        {
            int state = 0;
            string selectString = "Delete From TripDay "
                                + "Where tripId = @TripId;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@TripId", TripId);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = 1;
                }
            }
            return state;
        }
        public int DeleteTripDayByTripId(SqlConnection conn, string TripId, string TripDay)
        {
            int state = 0;
            string selectString = "Delete From TripDay "
                                + "Where tripId = @TripId and TripDay = @TripDay;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@TripId", TripId);
                cmd.Parameters.AddWithValue("@TripDay", TripDay);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = 1;
                }
            }
            return state;
        }
        public int DeleteTripDataByTripId(SqlConnection conn, string TripId, string TripDay)
        {
            int state = 0;
            string selectString = "Delete TripData From TripData inner join TripDay on TripData.TripID = TripDay.TripID and TripData.Nth_day = TripDay "
                                + "Where TripData.tripId = @TripId and TripData.Nth_day = @TripDay;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@TripId", TripId);
                cmd.Parameters.AddWithValue("@TripDay", TripDay);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = 1;
                }
            }
            return state;
        }
        #endregion
    }
    #endregion
}