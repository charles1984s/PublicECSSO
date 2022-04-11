using ECSSO.Library;
using ECSSO.Library.StoreSet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ECSSO.api.StoreSet
{
    /// <summary>
    /// StoreSetHandler 的摘要描述
    /// </summary>
    public class StoreSetHandler : IHttpHandler
    {
        private StoreSetList store;
        private CheckToken checkToken;
        public void ProcessRequest(HttpContext context)
        {
            store = new StoreSetList();
            checkToken = new CheckToken(store);
            try {
                checkToken.check(context);
                if (store.RspnCode == "200")
                {
                    switch (context.Request.Headers["option"]) {
                        case "GET":
                            if (new Regex("^[A-Z][0-9]{3}$").IsMatch(context.Request["jobID"]))
                            {
                                store.jobID = context.Request.Form["jobID"];
                                store.orgName = checkToken.token.orgName;
                                store.load();
                            }
                            else throw new Exception("權限不足");
                            break;
                        case "UPDATE":
                            store.List = JsonConvert.DeserializeObject<List<StoreSetItem>>(context.Request.Form["data"]);
                            if (store.List.Count > 0) store.update();
                            break;
                    }
                    
                }
                else throw new Exception("Token不存在");
            }
            catch (Exception ex)
            {
                store.RspnMsg = ex.Message;
            }
            finally
            {
                context.Response.Write(checkToken.printMsg());
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}