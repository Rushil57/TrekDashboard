using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AnalyticsAPI.Models;
//using AnalyticsAPI.EF;
using System.Web;
using System.Data.Objects;
using Newtonsoft.Json;
using static AnalyticsAPI.Models.EncryDecry;
using System.Web.Script.Serialization;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AnalyticsAPI.Controllers
{
    public class AllAccountsController : ApiController
    {
        private AllAccoutsModel _rep = new AllAccoutsModel();
        [HttpPost]
        [HttpGet]
        public string CreateSessionValues()
        {
            var ID = HttpContext.Current.Request.Headers["val"];
            HttpContext.Current.Session["db_CustomerKey"] = ID;
            return "";
        }
        [HttpGet]
        public AllAccountDashData GetAllAccountsData()
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
            var DuserInfo = json_serializer.Deserialize<UserInfo>(DecryptedUserInfo);
            string _accountID = HttpContext.Current.Request.Headers["AccountID"] != null ? HttpContext.Current.Request.Headers["AccountID"] : "";
            string _whereCondition = HttpContext.Current.Request.Headers["WhereConditions"] != null ? HttpContext.Current.Request.Headers["WhereConditions"] : "";
            //var _targetExclude = HttpContext.Current.Request.Headers["Target"] != null ? HttpContext.Current.Request.Headers["Target"].Split(',') : null;

            int InitialUser = 0;

            try
            {
                /*
                var content = Request.Content.ReadAsStringAsync().Result;
                string contents = Request.Headers.Where(x=>x.Key=="UserInfo").Select(x=>x.Value).FirstOrDefault().ToString();
                var contents1 = Request.Headers.Where(x => x.Key == "UserInfo").ToList();
                var DecryptedUserInfo1 = cEcnry.Decrypt(WebUtility.UrlDecode(((string[])(contents1.Where(x => x.Key == "UserInfo").FirstOrDefault().Value))[0]));
                */

                try
                {
                    //var DecryptedUserInfoOld = cEcnry.Decrypt(WebUtility.UrlDecode(DuserInfo.FirstUser));
                    //var iUserInfo = json_serializer.Deserialize<UserInfo>(DecryptedUserInfoOld);
                    //InitialUser = iUserInfo.UserID;

                    if (DuserInfo.FirstUser != null)
                    {

                        var DecryptedUserInfoOld = cEcnry.Decrypt(WebUtility.UrlDecode(DuserInfo.FirstUser));

                        if (string.IsNullOrEmpty(DecryptedUserInfoOld))
                        {
                            DecryptedUserInfoOld = cEcnry.Decrypt(DuserInfo.FirstUser);
                        }

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
                    }
                }
                catch (Exception ex)
                {
                }

                if (InitialUser == 0)
                {
                    InitialUser = DuserInfo.UserID;
                }
                
                var accountDashData = _rep.SaleRep(DuserInfo.UserID, _accountID, _whereCondition, DuserInfo.CallerID, InitialUser, userGUID, AccountSearchType);
                //accountDashData.UserLevel = DuserInfo.UserLevel;
                return accountDashData;
            }
            catch (Exception ex)
            {
                _rep.SendEmail(ex.Message, userGUID);
                return null;
            }
        }

        [HttpGet]
        public string EncryptionTest()
        {
            var userInfo = HttpContext.Current.Request.Headers["UserInfo"];
            var cEcnry = new EncryDecry();
            // var serilized = JsonConvert.DeserializeObject(userInfo);
            return WebUtility.UrlEncode(cEcnry.Encrypt(userInfo));

        }

        [HttpGet]
        public string DecryptionTest()
        {
            var userInfo = HttpContext.Current.Request.Headers["UserInfo"];
            var cEcnry = new EncryDecry();
            return cEcnry.Decrypt(WebUtility.UrlDecode(userInfo));

        }

        [HttpGet]
        public void InsertSelectedAccountIds()
        {
            string userGUID = "";
            if (HttpContext.Current.Session["db_UserGUID"] != null)
            {
                userGUID = HttpContext.Current.Session["db_UserGUID"].ToString();
            }
            else
            {
                userGUID = HttpContext.Current.Request.Headers["ReqUserGuid"] != null ? HttpContext.Current.Request.Headers["ReqUserGuid"] : Guid.NewGuid().ToString();
            }
            var accountIds = HttpContext.Current.Request.Headers["accountIds"];

            try
            {
                _rep.InsertSelectedAccountIds(accountIds, userGUID);
            }
            catch (Exception ex)
            {
                _rep.SendEmail(ex.Message, userGUID);
            }
        }
        [HttpGet]
        public async Task SendProposalToQueue()
        {
            var proposalId = HttpContext.Current.Request.Headers["proposalId"];
            var userGUID = HttpContext.Current.Request.Headers["userGUID"];
            var userId = HttpContext.Current.Request.Headers["userId"];
            await _rep.SendProposalToQueue(proposalId, userGUID, userId);
        }
    }
}