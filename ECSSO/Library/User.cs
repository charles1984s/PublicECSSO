using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    #region input
    public class CheckUser
    {
        public string ID { get; set; }   //會員帳號
        public string Pwd { get; set; }      //密碼
        public string UUID { get; set; }
    }
    #endregion
    #region input
    public class CheckUser2
    {
        public string Login { get; set; }      //會員帳號密碼
        public string UUID { get; set; }
    }
    #endregion
    #region output
    public class UserLogin
    {
        public string MemID { get; set; }   //會員編號
        public string LoginState { get; set; }   //登入狀態
    }
    #endregion
}