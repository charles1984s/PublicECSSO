using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class booking
    {
        public class root3
        {
            public string Pno { get; set; }        //現場點餐叫號號碼
            public List<WaitData> WaitData { get; set; }            
        }

        public class WaitData
        {
            public string SeatTitle { get; set; }
            public string SeatID { get; set; }
            public string BookingTime { get; set; }
            public string Pno { get; set; }
            public string TotalNum { get; set; }            
        }

        public class root
        {            
            public List<BookingData> BookingDatas { get; set; }
        }

        public class BookingData 
        {
            public string ID { get; set; }
            public string Time { get; set; }
            public string Pno { get; set; }
            public string Tel { get; set; }
            public string Num { get; set; }
            public string Cdate { get; set; }
            public string StoreTitle { get; set; }
            public string SeatTitle { get; set; }
            public string RegisterType { get; set; }
        }

        public class root2
        {
            public List<BookingStore> BookingStore { get; set; }
        }

        public class BookingData2 
        {
            public string Title { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }            
            public Int32 Stocks { get; set; }
        }

        public class BookingSeat 
        {
            public string SeatID { get; set; }
            public string SeatTitle { get; set; }
            public List<BookingData2> BookingData2 { get; set; }
        }

        public class BookingStore
        {
            public string StoreID { get; set; }
            public string StoreTitle { get; set; }
            public List<BookingSeat> BookingSeat { get; set; }
        }

        public class RootObject 
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
            public string Pno { get; set; }
        }
    }
}