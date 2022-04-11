using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TAT.Library
{
    public class Account
    {
        public class ErrorObject
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
        }

        public class InputData
        {
            public string ID { get; set; }
            public string Pwd { get; set; }
            public string Name { get; set; }
            public string Birth { get; set; }
            public string Gender { get; set; }
            public string PhoneCode { get; set; }
        }

        public class SMSData
        {
            public string stats { get; set; }
            public string error_code { get; set; }
            public string error_msg { get; set; }
        }
    }
}