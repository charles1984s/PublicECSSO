using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    #region input
    public class News
    {
        public int NewsTypeID { get; set; }
        public Range Range { get; set; }
    }
    public class Ads
    {
        public string AreaID { get; set; }
        public int ClassID { get; set; }
        public Range Range { get; set; }
    }
    public class Events
    {
        public string ID { get; set; }
        public string AreaID { get; set; }
        public string keyword { get; set; }
        public int subMenuID { get; set; }
        public Range Range { get; set; }

    }
    #endregion
    #region output
    public class NewsItem
    {
        public int Popular { get; set; }
        public int NewsID { get; set; }
        public string Date { get; set; }
        public string Title { get; set; }
        public string Brief { get; set; }
        public string PicURL { get; set; }
        public string DetailURL { get; set; }
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public string Ser_No { get; set; }
        public List<Frams> Fram { get; set; }
    }
    public class AdsItem
    {
        public string URL1 { get; set; }
        public string URL2 { get; set; }
        public string URL3 { get; set; }
        public string URL4 { get; set; }
        public string URL5 { get; set; }
        public string Link1 { get; set; }
        public string Link2 { get; set; }
        public string Link3 { get; set; }
        public string Link4 { get; set; }
        public string Link5 { get; set; }
    }
    public class EventsItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Brief { get; set; }
        public string PicURL { get; set; }
        public string DetailURL { get; set; }
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public List<Tags> Tag { get; set; }
    }
    public class HomeBannersItem
    {
        public string Image { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public List<MenuContent> MenuCont { get; set; }
    }
    public class MenuContent
    {
        public int Level { get; set; }
        public string Image { get; set; }
        public string Title { get; set; }
        public string Cont { get; set; }
        public string Link { get; set; }
        public string Target { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Align { get; set; }
        public int LeftOrRight { get; set; }
        public int Top { get; set; }
        public int Delay { get; set; }
        public int Duration { get; set; }
        public int InAnimation { get; set; }
    }
    public class detailEventsItem
    {
        public int Popular { get; set; }
        public string Id { get; set; }
        public string Orgname { get; set; }
        public string Org { get; set; }
        public string Co_Organiser { get; set; }
        public string GovID { get; set; }
        public string Name { get; set; }
        public string Name_ch { get; set; }
        public string Toldescribe { get; set; }
        public string Description { get; set; }
        public string Tel { get; set; }
        public string Location { get; set; }
        public string Add { get; set; }
        public string Particpation { get; set; }
        public string Cycle { get; set; }
        public string NonCycle { get; set; }
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public string Travellinginfo { get; set; }
        public string Charge { get; set; }
        public string Picture1 { get; set; }
        public string Picdescribe1 { get; set; }
        public string Picture2 { get; set; }
        public string Picdescribe2 { get; set; }
        public string Picture3 { get; set; }
        public string Picdescribe3 { get; set; }
        public string Map { get; set; }
        public double Px { get; set; }
        public double Py { get; set; }
        public string Website { get; set; }
        public string Parkinginfo { get; set; }
        public double Parkinginfo_px { get; set; }
        public double Parkinginfo_py { get; set; }
        public string Remarks { get; set; }
        public string Keyword { get; set; }
        public string QRCode { get; set; }
        public string Changetime { get; set; }
        public string Updatetime { get; set; }
        //public string BeaconUUID { get; set; }
        public string BusInfo { get; set; }
        //public string TripAdvisorUrl { get; set; }
        //public float TripAdvisorComment { get; set; }
        //public string GoogleUrl { get; set; }
        //public float GoogleComment { get; set; }
        //public int Comment { get; set; }
        public string Class1 { get; set; }
        public string Class2 { get; set; }
        public int Duration { get; set; }
        public List<Frams> Fram { get; set; }
        public List<Tags> Tag { get; set; }

    }
    #endregion
}
