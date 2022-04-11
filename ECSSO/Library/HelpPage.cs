using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class HelpPage
    {
        public List<HelpPageItem> Data { get; set; }
    }
    public class HelpPageItem{
        public string PageID { get; set; }
        public string StepID { get; set; }
        public string PicURL { get; set; }
        public string Description { get; set; }
    }
}