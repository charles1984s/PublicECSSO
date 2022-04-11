using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Booking.Library
{
    public class Account
    {
        public class Store 
        {
            public String ID { get; set; }
            public String Title { get; set; }            
        }
        public class Character 
        {
            public String ID { get; set; }
            public String Title { get; set; }
        }
        public class WebjobsGroup {
            public String ID { get; set; }
            public String Title { get; set; }
            public List<Webjobs> Webjobs { get; set; }
        }
        public class SetWebjobs
        {
            public String ID { get; set; }
            public String StoreID { get; set; }
            public String CID { get; set; }
            public List<Webjobs> Webjobs { get; set; }
        }
        public class Webjobs
        {
            public String JobID { get; set; }
            public String Title { get; set; }
            public String JobUrl { get; set; }
            public String CanAdd { get; set; }
            public String CanEdit { get; set; }
            public String CanDel { get; set; }
            public String CanExe { get; set; }
            public String CanQry { get; set; }
        }
        
    }
}