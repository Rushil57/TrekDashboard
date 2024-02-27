using AnalyticsAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using static AnalyticsAPI.Models.EncryDecry;

namespace AnalyticsAPI.Controllers
{
    public class SingleAccountController : ApiController
    {
        private SingleAccountModel _rep = new SingleAccountModel();

        [HttpGet]
        public SingleAccountDashData GetSingleAccountData()
        {
            string userGUID = "";
            int AccountSearchType = 0; // in sp 1 is for selected accounts

            if (HttpContext.Current.Request.Headers["AccountSearchType"] != null)
            {
                AccountSearchType = Convert.ToInt32(HttpContext.Current.Request.Headers["AccountSearchType"]);
            }
            if (HttpContext.Current.Session["db_UserGUID"] != null)
            {
                userGUID = HttpContext.Current.Session["db_UserGUID"].ToString();
            }
            else
            {
                userGUID = HttpContext.Current.Request.Headers["ReqUserGuid"] != null ? HttpContext.Current.Request.Headers["ReqUserGuid"] : Guid.NewGuid().ToString();
                //userGUID = Guid.NewGuid().ToString();
                HttpContext.Current.Session["db_UserGUID"] = userGUID;
            }

            var userInfo = HttpContext.Current.Request.Headers["UserInfo"];
            var cEcnry = new EncryDecry();
            var DecryptedUserInfo = cEcnry.Decrypt(WebUtility.UrlDecode(userInfo));
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            var DuserInfo = json_serializer.Deserialize<AccountInfo>(DecryptedUserInfo);
            string _accountID = HttpContext.Current.Request.Headers["AccountID"] != null ? HttpContext.Current.Request.Headers["AccountID"] : "";
            string _whereCondition = HttpContext.Current.Request.Headers["WhereConditions"] != null ? HttpContext.Current.Request.Headers["WhereConditions"] : "";

            int InitialUser = 0;
            try
            {
                try
                {
                    try
                    {
                        var DecryptedUserInfoOld = cEcnry.Decrypt(WebUtility.UrlDecode(DuserInfo.FirstUser));
                        var iUserInfo = json_serializer.Deserialize<UserInfo>(DecryptedUserInfoOld);
                        InitialUser = iUserInfo.UserID;

                        while (!string.IsNullOrEmpty(iUserInfo.FirstUser))
                        {
                            if (iUserInfo.FirstUser == "undefined")
                            {
                                iUserInfo.FirstUser = "";
                            }
                            else
                            {
                                var fUser = WebUtility.UrlDecode(iUserInfo.FirstUser);
                                if (string.IsNullOrEmpty(fUser))
                                {
                                    fUser = iUserInfo.FirstUser;
                                }

                                DecryptedUserInfoOld = cEcnry.Decrypt(fUser);
                                iUserInfo = json_serializer.Deserialize<UserInfo>(DecryptedUserInfoOld);
                            }
                        }
                        /*
                        DecryptedUserInfoOld = cEcnry.Decrypt(iUserInfo.FirstUser);
                        iUserInfo = json_serializer.Deserialize<UserInfo>(DecryptedUserInfoOld);

                        DecryptedUserInfoOld = cEcnry.Decrypt(iUserInfo.FirstUser);
                        iUserInfo = json_serializer.Deserialize<UserInfo>(DecryptedUserInfoOld);
                        */
                        InitialUser = iUserInfo.UserID;
                    }
                    catch (Exception ex)
                    {

                    }

                    if (InitialUser == 0)
                    {
                        InitialUser = DuserInfo.UserID;
                    }
                }
                catch (Exception ex)
                {

                }

                if (InitialUser == 0)
                {
                    InitialUser = DuserInfo.UserID;
                }

                var singleAccountDashData =  _rep.SaleRep(DuserInfo.UserID, DuserInfo.AccountID, _whereCondition, DuserInfo.CallerID, InitialUser, userGUID, AccountSearchType);
                //singleAccountDashData.UserLevel = DuserInfo.UserLevel;
                return singleAccountDashData;
            }
            catch (Exception ex)
            {
                _rep.SendEmail(ex.Message, userGUID);
                return null;
            }
        }
    }
}
