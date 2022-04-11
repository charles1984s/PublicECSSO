using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ECSSO.Library;

namespace ECSSO.api
{
    /// <summary>
    /// getServiceRecord 的摘要描述
    /// </summary>
    public class getServiceRecord : IHttpHandler
    {
        private CheckToken checkToken;
        private String memberID,message;
        public void ProcessRequest(HttpContext context)
        {
            ServiceRecord serviceRecord = new ServiceRecord();
            checkToken = new CheckToken();
            try
            {
                if (string.IsNullOrEmpty(context.Request.Params["MemberID"])) throw new Exception("MemberID必填");
                checkToken.check(context);
                if (checkToken.response.RspnCode == "200")
                {
                    memberID = context.Request.Params["MemberID"];
                                                       
                    try
                    {
                        serviceRecord.getMemRecode(checkToken.setting, checkToken.token.orgName, "0", memberID);
                        checkToken.GS.InsertLog(checkToken.setting, checkToken.token.id, "取得客戶服務紀錄", "getServiceRecord", memberID, "成功取得服務紀錄", "getServiceRecord.ashx");
                        message = JsonConvert.SerializeObject(serviceRecord);
                    }
                    catch(Exception e)
                    {
                        throw new Exception(e.Message);
                    }

                }
                else throw new Exception("Token不存在");
            }
            catch (Exception e)
            {
                checkToken.response.RspnMsg = e.Message;
                message = JsonConvert.SerializeObject(checkToken.response);
            }
            finally
            {
                checkToken.GS.ResponseWriteEnd(context, message);
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