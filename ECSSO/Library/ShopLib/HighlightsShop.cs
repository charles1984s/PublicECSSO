using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.ShopLib
{
    public class HighlightsShop : responseJson
    {
        public int id { get; set; }
        public int menuID { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public string Add { get; set; }
        public string Add_en { get; set; }
        public string Opentime { get; set; }
        public string Opentime_en { get; set; }
        public string Toldescribe { get; set; }
        public string Toldescribe_en { get; set; }
        public string Tel { get; set; }
        public string Tel2 { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }
    }
}