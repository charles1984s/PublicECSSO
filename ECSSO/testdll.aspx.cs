using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using templatecss;
using System.Configuration;

namespace ECSSO
{
    public partial class testdll : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            CSSRelease CR = new CSSRelease();
            Response.Write(CR.Release("develop"));            
            
            DeviceProd DP = new DeviceProd();

            String JsonStr = Request.QueryString["items"].ToString();
            DP.Delete(JsonStr);
           

        }
        
    }
}