using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Enumeration
{
    public enum ORderStatusEnum : int
    {
        詢價中 = -1,
        審核中 = 1,
        已收款 = 2,
        已出貨 = 3,
        已取消 = 4,
        付款失敗 = 5,
        出貨中 = 6,
        已完成 = 7,
        註記中 = 8
    }
}