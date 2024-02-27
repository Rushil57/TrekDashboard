using AnalyticsAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using static AnalyticsAPI.Models.EncryDecry;

namespace AnalyticsAPI.Controllers
{
    public class SalesTeamController : ApiController
    {
        private SalesRepsModel _rep = new SalesRepsModel();

        [HttpGet]
        [HttpPost]
        public SalesRepsDashData GetSalesTeamData()
        {
            string userGUID = "";

            if (HttpContext.Current.Session["db_UserGUID"] != null)
            {
                userGUID = HttpContext.Current.Session["db_UserGUID"].ToString();
            }
            else
            {
                //userGUID = Guid.NewGuid().ToString();
                userGUID = HttpContext.Current.Request.Headers["ReqUserGuid"] != null ? HttpContext.Current.Request.Headers["ReqUserGuid"] : Guid.NewGuid().ToString();
                HttpContext.Current.Session["db_UserGUID"] = userGUID;
            }

            var userInfo = HttpContext.Current.Request.Headers["UserInfo"];
            var cEcnry = new EncryDecry();
            var DecryptedUserInfo = cEcnry.Decrypt(WebUtility.UrlDecode(userInfo));
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            var DuserInfo = json_serializer.Deserialize<UserInfo>(DecryptedUserInfo);
            string _accountID = HttpContext.Current.Request.Headers["AccountID"] != null ? HttpContext.Current.Request.Headers["AccountID"] : "";
            string _whereCondition = HttpContext.Current.Request.Headers["WhereConditions"] != null ? HttpContext.Current.Request.Headers["WhereConditions"] : "";
            string _saleRepName = HttpContext.Current.Request.Headers["SaleRepName"] != null ? HttpContext.Current.Request.Headers["SaleRepName"] : "";
            //var _targetExclude = HttpContext.Current.Request.Headers["Target"] != null ? HttpContext.Current.Request.Headers["Target"].Split(',') : null;

            try
            {
                if (DuserInfo != null)
                {
                    //if(DuserInfo.LoadedPreviously == 0 && string.IsNullOrEmpty(_whereCondition) && string.IsNullOrEmpty(_saleRepName))
                    //{
                    //    HttpContext.Current.Session.Add("db_LoggedinUser", DuserInfo.UserID);

                    //    HttpResponseMessage respMessage = new HttpResponseMessage();
                    //    respMessage.Content = new ObjectContent<string[]> (new string[] { "value1", "value2" }, new System.Net.Http.Formatting.JsonMediaTypeFormatter());

                    //    var nvc = new System.Collections.Specialized.NameValueCollection();
                    //    nvc["db_LoggedinUser"] = DuserInfo.UserID.ToString();

                    //    CookieHeaderValue cookie = new CookieHeaderValue("session", nvc);
                    //    cookie.Expires = DateTimeOffset.Now.AddDays(1);
                    //    cookie.Domain = Request.RequestUri.Host;
                    //    cookie.Path = "/";
                    //    respMessage.Headers.AddCookies(new CookieHeaderValue[] { cookie });
                    //}

                    //int InitialUser = 0;

                    //CookieHeaderValue cookie1 = Request.Headers.GetCookies("session").FirstOrDefault();
                    //if (cookie1 != null)
                    //{
                    //    CookieState cookieState = cookie1["session"];
                    //    var user = cookieState["db_LoggedinUser"];
                    //}

                    return _rep.SaleRep(DuserInfo.UserID, _accountID, _whereCondition, userGUID, DuserInfo.CallerID);
                }

                return _rep.SaleRep(0, _accountID, _whereCondition, userGUID, 0);
            }
            catch (Exception ex)
            {
                _rep.SendEmail(ex.Message, userGUID);
                return null;
            }
        }
    }
}
