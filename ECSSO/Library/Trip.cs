using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    #region input
    public class Trip
    {
        public string Id { get; set; }
        public string AreaID { get; set; }
        public string ClassID { get; set; }
        public string keyword { get; set; }
        public Range Range { get; set; }
    }
    #endregion
    #region output
    public class TripItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Brief { get; set; }
        public string PicURL { get; set; }
        public string DetailURL { get; set; }
        public int NodeNum { get; set; }
        public string TotalTime { get; set; }
        public int Popular { get; set; }
        //public int Recommendation { get; set; }
        public List<Tags> Tag { get; set; }
    }
    public class detailTrip
    {
        public int TripDay { get; set; }
        public string StartTime { get; set; }
        /*public string Remark { get; set; }
        public string Title { get; set; }
        public string Img { get; set; }*/
        public List<Tags> Tag { get; set; }
        public List<TripNode> TripNodes { get; set; }
    }
    public class TripNode
    {
        public int sn { get; set; }
        public int NodeID { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public string tripDateTime { get; set; }
        public int Duration { get; set; }
        public int RouteTime { get; set; }
        public int Transportation { get; set; }
        public double Px { get; set; }
        public double Py { get; set; }
        public string Link { get; set; }
    }
    #endregion
}