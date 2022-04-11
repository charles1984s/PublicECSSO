using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.SearchClass
{
    public class SearchClassDetailInputOfInsertDto
    {
        public int searchId { get; set; }
        public List<int> list { get; set; }
    }
}