using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ECSSO.api
{
    public partial class tabletest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            GetStr GS = new GetStr();
            //Response.Write(GS.MD5Endode("834de6f72-0502-4c4e-9069-52b0da5a9395"));
            //Response.Write(GS.MD5Endode("1800ezsaleo2o") + "<BR>");
            //Response.Write(GS.MD5Endode("2800ezsaleo2o") + "<BR>");
            //Response.Write(GS.MD5Endode("3800ezsaleo2o") + "<BR>");
            //Response.Write(GS.MD5Endode("4800ezsaleo2o") + "<BR>");
            //Response.Write(GS.MD5Endode("5800ezsaleo2o") + "<BR>");
            //Response.Write(GS.MD5Endode("6800ezsaleo2o") + "<BR>");
            Response.Write(GS.MD5Endode("6504develop") + "<BR>");
        }
    }
}