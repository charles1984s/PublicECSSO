using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.MessageBoard
{
    public class IMessageBoard
    {
        public string title { get; set; }
        public string name { get; set; }
        public string question { get; set; }
        public string Reply { get; set; }
        public string questionTime { get; set; }
        public string ReplyTime { get; set; }
    }
}