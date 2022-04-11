using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.DataBind
{
    public class DataBindInterface
    {
        public string code { get; set; }
        public string message { get; set; }
        public List<DataBindDetailInterface> data { get; set; }
    }
    public class DataBindDetailInterface
    {
        public int id { get; set; }
        public string title { get; set; }
    }
}