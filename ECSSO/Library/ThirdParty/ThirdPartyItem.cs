using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.ThirdParty
{
    public class ThirdPartyItem
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public List<ThirdPartyKeypair> Keypair { get; set; }
        public List<PaymentTypeItem> Payments { get; set; }
    }
}