using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ECSSO.api
{
    public partial class TForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write(Server.UrlEncode(@"http://develop.ezsale.tw/tw/index.asp?au_id=40&sub_id=93&prod_sub_id=100&prod_id=207&prodSalesType=prod&RID="));
        }
    }
}