using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    #region input
    public class Node
    {
        public string id { get; set; }
        public string AreaID { get; set; }
        public string keyword { get; set; }
        public Range Range { get; set; }
    }
    #endregion
    #region output
    public class NodeBrief
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Brief { get; set; }
        public string PicURL { get; set; }
        public float Px { get; set; }
        public float Py { get; set; }
        public string DetailURL { get; set; }
    }
    public class NodeItem
    {
        public int Popular { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Name_ch { get; set; }
        public string Zone { get; set; }
        public string Toldescribe { get; set; }
        public string Description { get; set; }
        public string Tel { get; set; }
        public string Location { get; set; }
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
        //public string BeaconUUID { get; set; }
        public string BusInfo { get; set; }
        public string TripAdvisorUrl { get; set; }
        public double TripAdvisorComment { get; set; }
        public string GoogleUrl { get; set; }
        public double GoogleComment { get; set; }
        //public int Comment { get; set; }
        public int Duration { get; set; }
        public List<Frams> Fram { get; set; }
        public List<Tags> Tag { get; set; }
    }
    #endregion
}