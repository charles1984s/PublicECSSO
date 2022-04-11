using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ECSSO
{
    public partial class TestForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            GetStr GS = new GetStr();
            form1.Action = "/api/sendEmail.ashx";
            Response.Write(GS.MD5Endode("1730ksp"));
        }
    }
}