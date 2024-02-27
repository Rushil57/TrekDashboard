using AnalyticsAPI.Models;
using Newtonsoft.Json;
using SalesAnalytics.Web.Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SalesAnalytics.Web.Dashboard.Controllers
{
    public class DashboardController : Controller
    {
        public const int Default_Page_Size = 10;
        #region All Accounts
        public async Task<ActionResult> Accounts(string userInfo = "", string isAccSearch = "", string name = "")
        {

            Dictionary<string, string> headers = new Dictionary<string, string>();
            userInfo = WebUtility.UrlEncode(userInfo);
            headers.Add("UserInfo", userInfo);
            if (isAccSearch.ToLower().Equals("true"))
            {
                headers.Add("AccountSearchType", "1");
            }
            else
            {
                System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] = null;
                System.Web.HttpContext.Current.Session["seesion_SA_AccountSearchData"] = null;
            }
            System.Web.HttpContext.Current.Session["seesion_CurrentUser"] = userInfo;
            var model = await GetAllAccountDataFromAPI(true, headers, "", name);
            return View(model);
        }
        public async Task<AllAccountDashDataViewModel> GetAllAccountDataFromAPI(bool isFirstTime, Dictionary<string, string> headers, string targetToExclude = "", string name = "")
        {
            try
            {

                //
                AllAccountDashDataViewModel model = new AllAccountDashDataViewModel();
                model.UserInfoHeader = headers["UserInfo"];
                var url = ConfigurationManager.AppSettings["AnalyticsAPIUrl"].ToString();
                string apiUrl = url + "api/AllAccounts/GetAllAccountsData";
                model.ApiURLlink = url;
                var client = new HttpClient(new HttpClientHandler { UseProxy = false });

                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                foreach (var item in headers)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
                if (System.Web.HttpContext.Current.Session["seesion_UserGUID"] != null)
                {
                    client.DefaultRequestHeaders.Add("ReqUserGuid", Convert.ToString(System.Web.HttpContext.Current.Session["seesion_UserGUID"]));
                }
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    if (data != null && data != "null")
                    {
                        var listOfDashData = JsonConvert.DeserializeObject<AllAccountDashData>(data);

                        model.Account_Pie = listOfDashData.Account_Pie;
                        model.CallerID = listOfDashData.CallerID;
                        model.CurrentUser = string.IsNullOrEmpty(name) ? listOfDashData.CurrentUser : name;
                        model.DateRefreshed = listOfDashData.DateRefreshed;
                        model.ExpiringContracts_MixedChart = listOfDashData.ExpiringContracts_MixedChart;
                        model.ListBU = listOfDashData.ListBU;
                        model.Proposals_Pie = listOfDashData.Proposals_Pie;
                        model.Site_Card = listOfDashData.Site_Card;
                        model.TopLegacyProduct = listOfDashData.TopLegacyProduct;
                        model.TopProposals_Grid = listOfDashData.TopProposals_Grid;
                        model.Version = listOfDashData.Version;
                        model.UserGUID = listOfDashData.UserGUID;
                        model.DBUserId = listOfDashData.DBUserId;

                        System.Web.HttpContext.Current.Session["seesion_UserGUID"] = model.UserGUID;
                        //model.TopAccounts_StackedBar = listOfDashData.TopAccounts_StackedBar;
                        var serializeObject = JsonConvert.SerializeObject(listOfDashData.TopAccounts_StackedBar);
                        var topAccList = JsonConvert.DeserializeObject<List<AllAcc_TopAccount>>(serializeObject);
                        var groupData = topAccList.GroupBy(
                                                p => p.AccountName,
                                                (key, g) => new GroupData_AllAccount { AccountName = key, AccountList = g.ToList() }).ToList();
                        if (targetToExclude.ToLower() != "topaccountschartsettings")
                        {

                            model.Top_Account_TotalCount = groupData.Count();
                            var listGroupData = groupData.Take(Default_Page_Size)
                                                                        .Select(x => x.AccountList).ToList();
                            var finalListData = new List<AllAcc_TopAccount>();
                            foreach (var item in listGroupData)
                            {
                                finalListData.AddRange(item);
                            }
                            model.TopAccounts_StackedBar = finalListData.OrderBy(z => z.TotalSalesOpp);
                            System.Web.HttpContext.Current.Session["seesion_TopAccounts"] = groupData;
                        }
                        model.AccountListData = new List<AccountViewModel>();
                        //if (isFirstTime) // && !headers.ContainsKey("AccountSearchType")
                        if (System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] == null || ((List<AccountViewModel>)System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"]).ToList().Count == 0)
                        {
                            var serializeObjectAccountList = JsonConvert.SerializeObject(listOfDashData.AccountSearchList);
                            var accountViewModelList = JsonConvert.DeserializeObject<List<AccountViewModel>>(serializeObjectAccountList);
                            model.AccountListData = accountViewModelList;
                            System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] = model.AccountListData;
                        }
                        else
                        {
                            model.AccountListData = (List<AccountViewModel>)System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"];
                        }

                        if (headers.ContainsKey("AccountSearchType"))
                        {
                            model.IsAccountSearch = "1";
                            //model.AccountListData = System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] != null ? (List<AccountViewModel>)System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] : new List<AccountViewModel>();
                        }
                        if (System.Web.HttpContext.Current.Session["seesion_AccSearch_SelectedIds"] != null)
                        {
                            model.AccountSearchSelectedIds = (string)System.Web.HttpContext.Current.Session["seesion_AccSearch_SelectedIds"];
                        }


                        if (System.Web.HttpContext.Current.Session["seesion_UserLevel"] == null)
                        {
                            model.UserLevel = listOfDashData.UserLevel;
                            System.Web.HttpContext.Current.Session["seesion_UserLevel"] = model.UserLevel;
                        }
                        else
                        {
                            model.UserLevel = Convert.ToInt32(System.Web.HttpContext.Current.Session["seesion_UserLevel"]);
                        }
                    }
                }

                return model;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpPost]
        public async Task<string> LoadAllAccountData(Dictionary<string, string> headers, string targetToExclude)
        {
            var model = await GetAllAccountDataFromAPI(false, headers, targetToExclude, "");
            return JsonConvert.SerializeObject(model);
        }
        [HttpPost]
        public async Task<JsonResult> LoadTopAccountData(int currentPage)
        {

            var finalListData = new List<AllAcc_TopAccount>();
            var groupData = System.Web.HttpContext.Current.Session["seesion_TopAccounts"] != null ? (List<GroupData_AllAccount>)System.Web.HttpContext.Current.Session["seesion_TopAccounts"] : new List<GroupData_AllAccount>();

            var total = groupData.Count();
            var skip = Default_Page_Size * (currentPage - 1);

            var canPage = skip < total;

            if (!canPage)
                return Json(finalListData);

            var listGroupData = groupData
                         .Skip(skip)
                         .Take(Default_Page_Size)
                         .Select(x => x.AccountList)
                         .ToList();

            foreach (var item in listGroupData)
            {
                finalListData.AddRange(item);
            }
            return Json(finalListData.OrderBy(z => z.TotalSalesOpp));
        }

        [HttpPost]
        public async Task<string> SearchAccountByKeyWord(Dictionary<string, string> searchObj)
        {
            var keyWord = searchObj["keyWord"];
            var accountList = System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] != null ? (List<AccountViewModel>)System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] : new List<AccountViewModel>();
            var data = accountList.Where(x => x.AccountID.Contains(keyWord) || x.AccountName.ToLower().Contains(keyWord.ToLower())).ToList();
            return JsonConvert.SerializeObject(data);
        }

        [HttpPost]
        public async Task<string> FilterDataBasedOnSelectedAccount(Dictionary<string, string> headers, List<string> selectedAccountIds)
        {
            var url = ConfigurationManager.AppSettings["AnalyticsAPIUrl"].ToString();
            string apiUrl = url + "api/AllAccounts/InsertSelectedAccountIds";
            var client = new HttpClient(new HttpClientHandler { UseProxy = false });
            System.Web.HttpContext.Current.Session["seesion_AccSearch_SelectedIds"] = string.Join("|", selectedAccountIds);
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("accountIds", string.Join("|", selectedAccountIds));
            if (System.Web.HttpContext.Current.Session["seesion_UserGUID"] != null)
            {
                client.DefaultRequestHeaders.Add("ReqUserGuid", Convert.ToString(System.Web.HttpContext.Current.Session["seesion_UserGUID"]));
            }
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                if (data != null && data != "null")
                {
                    var listOfDashData = JsonConvert.DeserializeObject<AllAccountDashData>(data);

                }
            }

            headers.Add("AccountSearchType", "1");
            var model = await GetAllAccountDataFromAPI(false, headers, "", "");
            return JsonConvert.SerializeObject(model);
        }

        [HttpPost]
        public async Task UpdateAccountSearchData(Dictionary<string, string> modelData)
        {
            // ###### Need to confirm - BU filter apply in AccountSearch Grid data.
            //var selectedBu = modelData["AllList"];
            //List<AccountViewModel> accountListData = System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] != null ? (List<AccountViewModel>)System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] : new List<AccountViewModel>();
            //accountListData = accountListData.Where(x => selectedBu.Split(',').ToArray().Contains(x.LegacyBU)).ToList();
            //System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] = accountListData;

        }
        [HttpPost]
        public async Task<string> GetAccountSearchList()
        {
            // ###### Need to confirm - BU filter apply in AccountSearch Grid data.
            //var accountList = System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] != null ? (List<AccountViewModel>)System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] : new List<AccountViewModel>();
            //return JsonConvert.SerializeObject(accountList);
            return "";
        }
        [HttpPost]
        public async Task<string> BindAccountSearchListOnResetClick()
        {
            var accountList = System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] != null ? (List<AccountViewModel>)System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] : new List<AccountViewModel>();
            return JsonConvert.SerializeObject(accountList);
        }
        #endregion

        #region Single Account
        public async Task<ActionResult> SingleAccount(string userInfo = "", string name = "")
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            userInfo = WebUtility.UrlEncode(userInfo);
            headers.Add("UserInfo", userInfo);
            var model = await GetSingleAccountDataFromAPI(true, headers, name);
            return View(model);
        }
        public async Task<SingleAccountDashDataViewModel> GetSingleAccountDataFromAPI(bool isFirstTime, Dictionary<string, string> headers, string name = "")
        {
            try
            {
                //
                SingleAccountDashDataViewModel model = new SingleAccountDashDataViewModel();
                model.UserInfoHeader = headers["UserInfo"];
                var url = ConfigurationManager.AppSettings["AnalyticsAPIUrl"].ToString();
                string apiUrl = url + "api/SingleAccount/GetSingleAccountData";
                model.ApiURLlink = url;
                var client = new HttpClient(new HttpClientHandler { UseProxy = false });

                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Add("UserInfo", "sitdW1bx0LZm0HuPctdcNuKCZtws0MLjLdwmxkEbDxqkXdMws0Z9kJcQ4KFUQWV3");
                foreach (var item in headers)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }

                if (System.Web.HttpContext.Current.Session["seesion_UserGUID"] != null)
                {
                    client.DefaultRequestHeaders.Add("ReqUserGuid", Convert.ToString(System.Web.HttpContext.Current.Session["seesion_UserGUID"]));
                }
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    if (data != null && data != "null")
                    {
                        var listOfDashData = JsonConvert.DeserializeObject<SingleAccountDashData>(data);

                        model.BUList = listOfDashData.BUList;
                        model.CurrentUser = name + " > " + listOfDashData.CurrentUser;

                        model.DateRefreshed = listOfDashData.DateRefreshed;
                        model.Account_Pie = listOfDashData.Account_Pie;
                        model.ExpiringContracts_MixedChart = listOfDashData.ExpiringContracts_MixedChart;
                        model.Proposals_Pie = listOfDashData.Proposals_Pie;
                        model.Site_Card = listOfDashData.Site_Card;
                        model.TopLegacyProduct = listOfDashData.TopLegacyProduct;
                        model.TopProposals_Grid = listOfDashData.TopProposals_Grid;
                        model.TopSites_StackedBar = listOfDashData.TopSites_StackedBar;
                        model.Version = listOfDashData.Version;
                        model.UserGUID = listOfDashData.UserGUID;
                        model.AccountListData = new List<AccountViewModel>();
                        if (isFirstTime)
                        {

                            if (System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] == null)
                            {
                                var serializeObjectAccountList = JsonConvert.SerializeObject(listOfDashData.AccountSearchList);
                                var accountViewModelList = JsonConvert.DeserializeObject<List<AccountViewModel>>(serializeObjectAccountList);
                                model.AccountListData = accountViewModelList;
                                System.Web.HttpContext.Current.Session["seesion_SA_AccountSearchData"] = model.AccountListData;
                            }
                            else
                            {
                                model.AccountListData = (List<AccountViewModel>)System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"];
                                System.Web.HttpContext.Current.Session["seesion_SA_AccountSearchData"] = model.AccountListData;
                            }
                        }

                        if (System.Web.HttpContext.Current.Session["seesion_AccSearch_SelectedIds"] != null)
                        {
                            model.AccountSearchSelectedIds = (string)System.Web.HttpContext.Current.Session["seesion_AccSearch_SelectedIds"];
                        }

                        model.Name = name.Replace(">", "<");
                        model.DBUserId = listOfDashData.DBUserId;

                        if (System.Web.HttpContext.Current.Session["seesion_UserLevel"] == null)
                        {
                            model.UserLevel = listOfDashData.UserLevel;
                            System.Web.HttpContext.Current.Session["seesion_UserLevel"] = model.UserLevel;
                        }
                        else
                        {
                            model.UserLevel = Convert.ToInt32(System.Web.HttpContext.Current.Session["seesion_UserLevel"]);
                        }
                        model.CurrentUserFromAccountsDash = System.Web.HttpContext.Current.Session["seesion_CurrentUser"] != null ? Convert.ToString(System.Web.HttpContext.Current.Session["seesion_CurrentUser"]) : string.Empty;

                    }
                }

                return model;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost]
        public async Task<string> SearchAccountByKeyWordForSA(Dictionary<string, string> searchObj)
        {
            // Note: This function will use for Search Acount functionality in Single Account Dashboard.
            var keyWord = searchObj["keyWord"];
            var accountList = System.Web.HttpContext.Current.Session["seesion_SA_AccountSearchData"] != null ? (List<AccountViewModel>)System.Web.HttpContext.Current.Session["seesion_SA_AccountSearchData"] : new List<AccountViewModel>();
            var data = accountList.Where(x => x.AccountID.Contains(keyWord) || x.AccountName.ToLower().Contains(keyWord.ToLower())).ToList();
            return JsonConvert.SerializeObject(data);
        }
        [HttpPost]
        public async Task FilterDataBasedOnSelectedAccountForSA(Dictionary<string, string> headers, List<string> selectedAccountIds, string name)
        {
            // Note: This function will use for Search Acount functionality in Single Account Dashboard.
            var url = ConfigurationManager.AppSettings["AnalyticsAPIUrl"].ToString();
            string apiUrl = url + "api/AllAccounts/InsertSelectedAccountIds";
            System.Web.HttpContext.Current.Session["seesion_AccSearch_SelectedIds"] = string.Join("|", selectedAccountIds);
            var client = new HttpClient(new HttpClientHandler { UseProxy = false });
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("accountIds", string.Join("|", selectedAccountIds));
            if (System.Web.HttpContext.Current.Session["seesion_UserGUID"] != null)
            {
                client.DefaultRequestHeaders.Add("ReqUserGuid", Convert.ToString(System.Web.HttpContext.Current.Session["seesion_UserGUID"]));
            }
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                if (data != null && data != "null")
                {
                    var listOfDashData = JsonConvert.DeserializeObject<AllAccountDashData>(data);

                }
            }

            //headers.Add("AccountSearchType", "1");
            //var model = GetSingleAccountDataFromAPI(false, headers, name);
            //return JsonConvert.SerializeObject(model);
        }
        #endregion

        #region Sales Team
        public async Task<ActionResult> SalesTeam(string userInfo = "", string saleRepName = "")
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            userInfo = WebUtility.UrlEncode(userInfo);
            headers.Add("UserInfo", userInfo);
            var model = await GetSalesTeamAllAccountDataFromAPI(headers, saleRepName);
            System.Web.HttpContext.Current.Session["seesion_AA_AccountSearchData"] = null;
            System.Web.HttpContext.Current.Session["seesion_SA_AccountSearchData"] = null;
            System.Web.HttpContext.Current.Session["seesion_AccSearch_SelectedIds"] = null;
            System.Web.HttpContext.Current.Session["seesion_UserLevel"] = null;
            return View(model);
        }
        public async Task<SalesRepsDashDataViewModel> GetSalesTeamAllAccountDataFromAPI(Dictionary<string, string> headers, string saleRepName, string targetToExclude = "")
        {

            try
            {
                //
                SalesRepsDashDataViewModel model = new SalesRepsDashDataViewModel();
                model.UserInfoHeader = headers["UserInfo"];
                var url = ConfigurationManager.AppSettings["AnalyticsAPIUrl"].ToString();
                string apiUrl = url + "api/SalesTeam/GetSalesTeamData";
                model.ApiURLlink = url;
                var client = new HttpClient(new HttpClientHandler { UseProxy = false });
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                foreach (var item in headers)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
                if (System.Web.HttpContext.Current.Session["seesion_UserGUID"] != null)
                {
                    client.DefaultRequestHeaders.Add("ReqUserGuid", Convert.ToString(System.Web.HttpContext.Current.Session["seesion_UserGUID"]));
                }
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    if (data != null && data != "null")
                    {
                        var listOfDashData = JsonConvert.DeserializeObject<SalesRepsDashData>(data);

                        model.CallerID = listOfDashData.CallerID;
                        model.CurrentUser = listOfDashData.CurrentUser;
                        model.DateRefreshed = listOfDashData.DateRefreshed;
                        model.Pipeline_Pie = listOfDashData.Pipeline_Pie;
                        model.Proposals_Pie = listOfDashData.Proposals_Pie;
                        model.SalesTeamOpp_StackedBar = listOfDashData.SalesTeamOpp_StackedBar;
                        model.Site_Card = listOfDashData.Site_Card;
                        model.UserGUID = listOfDashData.UserGUID;
                        System.Web.HttpContext.Current.Session["seesion_UserGUID"] = model.UserGUID;
                        //model.TopAccounts_StackedBar = listOfDashData.TopAccounts_StackedBar;
                        if (targetToExclude.ToLower() != "salesresptopaccountssettings")
                        {
                            var serializeObject = JsonConvert.SerializeObject(listOfDashData.TopAccounts_StackedBar);
                            var topAccList = JsonConvert.DeserializeObject<List<SalesTeam_TopAccount>>(serializeObject);
                            var groupData = topAccList.GroupBy(
                                                    p => p.AccountName,
                                                    (key, g) => new GroupData_AllAccount_SalesTeam { AccountName = key, AccountList = g.ToList() }).ToList();
                            model.Top_Account_TotalCount = groupData.Count();
                            var listGroupData = groupData.Take(Default_Page_Size)
                                                                        .Select(x => x.AccountList).ToList();
                            var finalListData = new List<SalesTeam_TopAccount>();
                            foreach (var item in listGroupData)
                            {
                                finalListData.AddRange(item);
                            }
                            model.TopAccounts_StackedBar = finalListData.OrderBy(z => z.TotalSalesOpp);
                            System.Web.HttpContext.Current.Session["seesion_TopAccounts_salesTeam"] = groupData;
                        }
                        model.TopLegacyLegacyPlateform_StackedBar = listOfDashData.TopLegacyLegacyPlateform_StackedBar;
                        model.TotalAccounts_MixedChart = listOfDashData.TotalAccounts_MixedChart;
                        model.Version = listOfDashData.Version;
                        //viewModel.CurrentUser(dt[0].CurrentUser === null ? "" : params.STName === undefined ? dt[0].CurrentUser.toString() : params.STName + " < " + dt[0].CurrentUser.toString());
                        model.STsaleRepName = string.IsNullOrEmpty(listOfDashData.CurrentUser) ? string.Empty : (string.IsNullOrEmpty(saleRepName) ? listOfDashData.CurrentUser : saleRepName + " < " + listOfDashData.CurrentUser);//saleRepName;
                        model.CurrentUser = model.STsaleRepName;
                    }
                }

                return model;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpPost]
        public async Task<string> LoadAllDataForSalesTeam(Dictionary<string, string> headers, string targetToExclude, string saleRepName)
        {
            var model = await GetSalesTeamAllAccountDataFromAPI(headers, saleRepName, targetToExclude);
            return JsonConvert.SerializeObject(model);
        }

        [HttpPost]
        public async Task<JsonResult> LoadTopAccountDataForSales(int currentPage)
        {

            var finalListData = new List<SalesTeam_TopAccount>();
            var groupData = System.Web.HttpContext.Current.Session["seesion_TopAccounts_salesTeam"] != null ? (List<GroupData_AllAccount_SalesTeam>)System.Web.HttpContext.Current.Session["seesion_TopAccounts_salesTeam"] : new List<GroupData_AllAccount_SalesTeam>();

            var total = groupData.Count();
            var skip = Default_Page_Size * (currentPage - 1);

            var canPage = skip < total;

            if (!canPage)
                return Json(finalListData);

            var listGroupData = groupData
                         .Skip(skip)
                         .Take(Default_Page_Size)
                         .Select(x => x.AccountList)
                         .ToList();

            foreach (var item in listGroupData)
            {
                finalListData.AddRange(item);
            }
            return Json(finalListData.OrderBy(z => z.TotalSalesOpp));
        }
        #endregion

        #region Common
        [HttpPost]
        public async Task<string> EncryptionTest(EncryptionParamData modelData)
        {
            string retObj = string.Empty;
            try
            {
                var url = ConfigurationManager.AppSettings["AnalyticsAPIUrl"].ToString();
                string apiUrl = url + "api/AllAccounts/EncryptionTest";

                var client = new HttpClient(new HttpClientHandler { UseProxy = false });
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("UserInfo", modelData.XhrSettings);
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    retObj = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(data);
                }

            }
            catch (Exception ex)
            {

                throw;
            }

            return retObj;
        }
        #endregion
    }
}
