using ECSSO.Library.Enumeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.StoreSet
{
    public class StoreSetItem
    {
        public string key { get; set; }
        public string name { get; set; }
        public string memo { get; set; }
        public int type { get; set; }
        public int maxlength { get; set; }
        public bool enable { get; set; }
        public string value { get; set; }
        public string text { get; set; }
        public List<StoreSetDetail> details { get; set; }
        public void setDetail()
        {
            int[] hasDetailList = { 3, 4, 5, 8 };
            if (Array.IndexOf(hasDetailList, type) >= 0) setDetailForKey();
        }
        private void convertEunm<T>(string column) where T : Enum
        {
            details = Enum.GetValues(typeof(T))
                        .Cast<T>()
                        .Select(t =>
                        {
                            StoreSetDetail d = new StoreSetDetail
                            {
                                id = Convert.ToInt32(t),
                                name = t.ToString()
                            };
                            switch (column) {
                                case "enable":
                                    d.isChecked = (enable ? d.id == 1 : d.id == 0);
                                    break;
                                case "value":
                                    d.isChecked = (d.name == value);
                                    break;
                            }
                            return d;
                        }).ToList();
        }
        private void setDetailForKey()
        {
            details = new List<StoreSetDetail>();
            switch (key)
            {
                case "sendShipment":
                    convertEunm<EnableEnum>("enable");
                    break;
                case "companyHardwareClass":
                    convertEunm<HardWareClassEunm>("value");
                    break;
            }
        }
    }
}