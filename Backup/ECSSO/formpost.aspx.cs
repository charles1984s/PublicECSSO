using System;
using System.Collections.Generic;
//using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace ECSSO
{
    public partial class formpost : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //CockerAdmin.CockerAdmin CA = new CockerAdmin.CockerAdmin();

            orderData.Value = @"{OrderData:{'WebTitle':'1','OrgName':'derek','ReturnUrl':'http:\/\/derek.ezsale.tw\/','ErrorUrl':'http:\/\/derek.ezsale.tw\/buy_save.asp','PayType':'','FreightAmount':'50','BonusDiscount':'0','BonusAmt':'40','MemID':'','OrderLists':[{'ID':'13','Type':'1','Title':'買三件699','OrderItems':[{'ID':'2','Size':'1','Color':'1','Name':'測試產品','Qty':'1','Price':'350','FinalPrice':'133','PosNo':'A1'},{'ID':'7','Size':'1','Color':'1','Name':'POS Prod','Qty':'2','Price':'2500','FinalPrice':'133','PosNo':'A2'}]},{'ID':'13','Type':'1','Title':'買三件699','OrderItems':[{'ID':'2','Size':'1','Color':'1','Name':'測試產品','Qty':'1','Price':'350','FinalPrice':'133','PosNo':'A1'},{'ID':'7','Size':'1','Color':'1','Name':'POS Prod','Qty':'2','Price':'2500','FinalPrice':'133','PosNo':'A2'}]}]}}";
        }
    }
}