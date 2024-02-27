//using AnalyticsAPI.EF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Objects;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace AnalyticsAPI.Models
{
    public class AllAccoutsModel : IDisposable
    {
        //private SAnalyticsEntities _ef = new SAnalyticsEntities();

        public void Dispose()
        {
            this.Dispose();
            //_ef.Dispose();
        }

        internal AllAccountDashData SaleRep(int? userID, string accountID, string whereCondition, int CallerID, int InitialUserId, string userGUID, int AccountSearchType)
        {
            //string datetimevalues = DateTime.Now.ToString();
            userID = userID == null ? userID = 0 : userID;
            accountID = accountID == null ? "" : accountID;
            whereCondition = whereCondition == null ? "" : whereCondition;
            var dt = new AllAccountDashData();
            //var allData = _ef.SaleRepSP(userID, accountID, whereCondition).ToList();
            string connectionString = ConfigurationManager.AppSettings["DbConnection"].ToString();

            QueryBuilder.DashboardTracking_DelegateMethod(userID, whereCondition, connectionString, "aa", userGUID);

            var allData = QueryBuilder.SaleRepSP(userID, accountID, whereCondition, connectionString, "aa", InitialUserId, userGUID, AccountSearchType);

            //datetimevalues = datetimevalues + "___" + DateTime.Now.ToString();

            dt.ListBU = BUListFilter(allData.Tables[0]);
            if (allData.Tables[1].Rows.Count > 0)
            {
                dt.CurrentUser = allData.Tables[1].Rows[0][0].ToString();
                dt.Version = allData.Tables[1].Rows[0][1].ToString();
                dt.DateRefreshed = allData.Tables[1].Rows[0][2].ToString();
            }
            else {
                dt.CurrentUser = string.Empty;
                dt.Version = string.Empty;
                dt.DateRefreshed = string.Empty;
            }
            dt.Account_Pie = AccountsSummary(allData.Tables[2]);
            dt.TopAccounts_StackedBar = TopAccountsSummary(allData.Tables[3], CallerID);
            dt.ExpiringContracts_MixedChart = ExpiringContracts(allData.Tables[4]);
            dt.Proposals_Pie = Proposals(allData.Tables[5]);
            dt.TopProposals_Grid = TopProposal(allData.Tables[6], CallerID);
            dt.TopLegacyProduct = TopProductsSummary(allData.Tables[7]);
            dt.Site_Card = RepsSummary(allData.Tables[8]);
            dt.CallerID = CallerID;
            dt.UserGUID = userGUID;
            dt.DBUserId = Convert.ToString(userID);
            dt.UserLevel = Convert.ToInt32(allData.Tables[10].Rows[0][0]);
            dt.AccountSearchList = AccountSearchListData(allData.Tables[9]);
            //datetimevalues = datetimevalues + "___" + DateTime.Now.ToString();
            //SendEmail("Time: " + datetimevalues);
            return dt;
        }
        public void SendEmail(string MailMessageBody = "", string InitialUser = "")
        {
            string userName = "SafariTrak@sales-analytics.com";
            string password = "seacamel";
            string host = "sai-mx.mwdata.net";
            string AppVersion = ConfigurationManager.AppSettings["DbConnection"].ToString();

            try
            {
                SmtpClient client;
                MailAddress to = new MailAddress("rama.satyanarayana@salesanalytics.com");
                MailAddress from = new MailAddress("noreply@safaritrak.com");
                MailMessage message = new MailMessage(from, to);

                message.Body = MailMessageBody;
                message.Subject = "Dashboard Alert: " + AppVersion + " GUID: " + InitialUser;
                message.IsBodyHtml = true;

                client = new SmtpClient(host, 25);
                client.Credentials = new NetworkCredential(userName, password);
                client.EnableSsl = false;
                client.Timeout = 600000;

                client.Send(message);
            }
            catch (Exception ex)
            {
            }
        }
        #region CustomServerSideData

        private string SalesRepsCurrentUser(List<tbldashboardsalesrep> data)
        {
            var user = (from i in data select i.SalesRepFullName).FirstOrDefault();
            return user;
        }

        private IEnumerable<object> RepsSummary(DataTable data)
        {
            var results = (from i in data.AsEnumerable()
                           select new
                           {
                               TotalSalesOpps = getValidValue(i, "TotalSalesOpps"),
                               NoProposals = i.Field<Int64>("NoProposals").ToString("#,##0"),
                               NoAccounts = i.Field<Int64>("NoAccounts").ToString("#,##0"),
                               NoSite = i.Field<Int64>("NoSite").ToString("#,##0"),
                               TotalLegacyQty = getValidValue(i, "LegacyQuantity"),
                               TotalHighRanking = i.Field<Int64>("TotalHighRanking").ToString("#,##0"),
                               TotalHighRankingSalesOpps = i.Field<Int64>("TotalHighRankingSalesOpps"),
                           }).AsEnumerable();

            return results;
        }

        private object getValidValue(DataRow i, string colName)
        {
            if (colName == "TotalSalesOpps") {
                double dblOut = 0.0;
                var val = Convert.ToString(i.Field<object>("TotalSalesOpps"));
                if (double.TryParse(val, out dblOut))
                {
                    dblOut = double.Parse(val);
                }
                return dblOut;
            }
            if (colName == "LegacyQuantity")
            {
                decimal dblOut = 0;
                var val = Convert.ToString(i.Field<object>("LegacyQuantity"));
                if (decimal.TryParse(val, out dblOut))
                {
                    dblOut = decimal.Parse(val);
                }
                return dblOut.ToString("#,##0");
            }
            return 0;
        }

        internal IEnumerable<object> AccountsSummary(DataTable data)
        {
            var results = (from i in data.AsEnumerable()
                           select new
                           {
                               Status = i["ProposalSFDCStatus"],
                               NoAccounts = i["NoAccounts"],
                               NoProposals = i["NoProposals"],
                               Details = new
                               {
                                   NoAccounts = i["NoAccounts"],
                                   NoProposals = i["NoProposals"],
                                   TotalSalesOpp = i["TotalSalesOpp"],
                               }
                           }).AsEnumerable();

            return results;
        }
        internal IEnumerable<object> TopAccountsSummary(DataTable data, int CallerID)
        {

            var query = (from i in data.AsEnumerable()
                         group i by new { AccountName = i.Field<string>("AccountName") } into grp
                         orderby grp.Sum(x => x.Field<float>("ProposalSalesOpportunity")) descending
                         select
                         new
                         {
                             AccountName = grp.Key.AccountName,
                             AccountID = grp.Select(x => x.Field<string>("AccountID")).FirstOrDefault(),
                             UserID = grp.Select(x => x.Field<int>("SalesRepUserId")).FirstOrDefault(),
                             TotalSalesOpp = grp.Sum(x => x.Field<float>("ProposalSalesOpportunity")),
                             Details = grp.GroupBy(x => x.Field<string>("LegacyBU")).Select(h => new { BU = h.Key, NoProposals = h.Select(t => t.Field<int>("ProposalId")).Distinct().Count(), AccountUrl = h.Select(x => x.Field<string>("SingleAccountUrl")).FirstOrDefault(), SalesOpp = h.Sum(x => x.Field<float>("ProposalSalesOpportunity")) }),
                         }).ToList();

            List<dynamic> AggregatedData = new List<dynamic>();

            string acUrl = "";
            var bus = BUListForAccounts(data);
            foreach (var item in query)
            {
                var t = (from bu in bus
                         join i in item.Details
                          on bu equals bus.Where(x => i.BU.ToLower() == x.text.ToLower()).FirstOrDefault() into g

                         select new
                         {
                             BU = bu.text,
                             NoProposals = g.Sum(x => x.NoProposals),
                             SalesOpp = (float)g.Sum(x => x.SalesOpp),
                             AccountUrl = g.Where(x => x.AccountUrl.Length > 2).Select(x => x.AccountUrl).FirstOrDefault(),
                         }).ToArray();
                foreach (var subitem in t)
                {
                    if (subitem.AccountUrl != null)
                    {
                        acUrl = subitem.AccountUrl;
                    }
                    IDictionary<string, object> newItem = new ExpandoObject();
                    newItem["AccountName"] = item.AccountName;
                    newItem["TotalSalesOpp"] = item.TotalSalesOpp;
                    newItem["LegacyBU"] = subitem.BU;
                    newItem["AccountUrl"] = acUrl;
                    newItem["SalesOpp"] = subitem.SalesOpp;
                    newItem["Details"] = new { TotalSalesOpp = subitem.SalesOpp, NoProposals = subitem.NoProposals, AccountUrl = acUrl, AccountID = item.AccountID, UserID = item.UserID, CallerID = CallerID, NewUrl = "GetAccountURL('" + item.AccountID + "')" };
                    if (acUrl != null)
                    {
                        AggregatedData.Add(newItem);
                    }
                }
            }

            //return AggregatedData.OrderBy(x => x.SalesOpp).ThenByDescending(x => x.TotalSalesOpp);
            return AggregatedData.OrderByDescending(x => x.TotalSalesOpp);
        }
        internal IEnumerable<object> ExpiringContracts(DataTable data)
        {
            var results = (from i in data.AsEnumerable()
                           group i by i.Field<DateTime>("LegacyContractEndDate").ToString("MMM yyyy") into grp
                           select new
                           {
                               ExpiryDate = grp.Key,
                               SortDate = DateTime.Parse(grp.Key),
                               NoProposals = (int)grp.Sum(x => x.Field<Int64>("ProposalId")),
                               TotalSalesOpp = (double)grp.Sum(x => x.Field<double>("ProposalSalesOpportunity"))
                           }

            ).OrderBy(x => x.SortDate).ToList();

            return results;
        }
        internal IEnumerable<object> Proposals(DataTable data)
        {
            var results = (from i in data.AsEnumerable()
                               //group i by i.Field<string>("ProposalRank") into grp
                           select new
                           {
                               ProposaRank = i["ProposalRank"].ToString(),
                               NoProposals = i.Field<Int64>("ProposalId"),
                               TotalSalesOpp = i.Field<double>("ProposalSalesOpportunity"),
                               Tag = new
                               {
                                   NoProposals = i.Field<Int64>("ProposalId"),
                                   TotalSalesOpp = i.Field<double>("ProposalSalesOpportunity"),
                                   Accounts = i.Field<Int64>("AccountID")
                               }
                           }).ToList();

            return results;
        }
        internal List<TopProposals> TopProposal(DataTable data, int CallerID)
        {

            var query = (from i in data.AsEnumerable()
                             //group i by new { i.ProposalId, i.ProposalRank, i.LegacyShortName, i.ProposedShortName } into grp
                         select new TopProposals
                         {
                             ProposalID = i.Field<int>("ProposalId"),
                             Rank = i.Field<string>("ProposalRank"),
                             SalesOpportunity = i.Field<decimal>("ProposalSalesOpportunity"),
                             LegacyProduct = i.Field<string>("LegacyShortName"),
                             LegacyQty = i.Field<int>("LegacyQuantity"),
                             ProposedProduct = i.Field<string>("ProposedShortName"),
                             ProposedQty = i.Field<int>("ProposedQuantity"),
                             ProposalScore = i.Field<double>("ProposalScore"),
                             ProposalURL = i.Field<string>("SingleProposalUrl"),
                             CallerID = CallerID
                         }).ToList();

            return query;
        }
        internal IEnumerable<object> TopProductsSummary(DataTable data)
        {

            var query = (from i in data.AsEnumerable()
                         group i by i.Field<string>("LegacyShortName") into grp
                         select new
                         {
                             LegacyName = grp.Key,
                             Details = grp.ToList(),
                             TotalQty = grp.Sum(x => x.Field<int>("LegacyQuantity")),
                         }).OrderByDescending(x => x.TotalQty).Take(10).ToList();

            List<dynamic> AggregatedData = new List<dynamic>();
            List<AgeRanges> ranges = new List<AgeRanges>();

            //Dictionary<int, string> ranges = new Dictionary<int, string>();
            ranges.Add(new AgeRanges() { StartMonth = 0, EndMonth = 12, MonthLabel = "0 - 12" });
            ranges.Add(new AgeRanges() { StartMonth = 13, EndMonth = 24, MonthLabel = "13 - 24" });
            ranges.Add(new AgeRanges() { StartMonth = 25, EndMonth = 36, MonthLabel = "25 - 36" });
            ranges.Add(new AgeRanges() { StartMonth = 37, EndMonth = 48, MonthLabel = "37 - 48" });
            ranges.Add(new AgeRanges() { StartMonth = 49, EndMonth = 60, MonthLabel = "49 - 60" });
            ranges.Add(new AgeRanges() { StartMonth = 61, EndMonth = 72, MonthLabel = "61 - 72" });
            ranges.Add(new AgeRanges() { StartMonth = 73, EndMonth = 84, MonthLabel = "73 - 84" });
            ranges.Add(new AgeRanges() { StartMonth = 85, EndMonth = 96, MonthLabel = "85 - 96" });
            ranges.Add(new AgeRanges() { StartMonth = 97, EndMonth = 108, MonthLabel = "97 - 108" });
            ranges.Add(new AgeRanges() { StartMonth = 109, EndMonth = 120, MonthLabel = "109 - 120" });
            ranges.Add(new AgeRanges() { StartMonth = 120, EndMonth = 999, MonthLabel = "120+ Months" });

            foreach (var item in query)
            {
                var t = (from range in ranges
                         join i in item.Details on range equals ranges.Where(x => i.Field<int>("LegacyAge") >= x.StartMonth && i.Field<int>("LegacyAge") <= x.EndMonth).LastOrDefault() into groups
                         select new { Age = range.MonthLabel, AgeKey = range.EndMonth, Qty = groups.Sum(x => x.Field<int>("LegacyQuantity")), SalesOpp = groups.Sum(x => x.Field<decimal>("ProposalSalesOpportunity")), Proposals = groups.Select(x => x.Field<int>("ProposalId")).Distinct().Count() }).ToArray();

                foreach (var age in t)
                {
                    IDictionary<string, object> newItem = new ExpandoObject();
                    newItem["LegacyName"] = item.LegacyName;
                    newItem["LegacyAge"] = age.Age;
                    newItem["LegacyQty"] = age.Qty;
                    newItem["TotalQty"] = item.TotalQty;
                    newItem["Qty"] = age.Qty;
                    newItem["Tag"] = new
                    {
                        SalesOpp = age.SalesOpp,
                        Proposals = age.Proposals,
                        LegacyQty = age.Qty,
                        AgeKey = age.AgeKey,
                    };
                    AggregatedData.Add(newItem);
                }
            }

            return AggregatedData.OrderBy(x => x.TotalQty);
        }

        internal IEnumerable<object> BUListFilter(DataTable data)
        {
            var _bu = (from i in data.AsEnumerable() select new { text = i["Text"], value = i["Value"] }).ToList();
            return _bu;
        }

        private IEnumerable<buITem> BUList(DataTable data)
        {
            var _bu = (from i in data.AsEnumerable() select new buITem { text = i["Text"].ToString(), value = i["Value"].ToString() }).ToList();
            return _bu;
        }
        private IEnumerable<buITem> BUListForAccounts(DataTable data)
        {
            var _bu = (from i in data.AsEnumerable()
                       group i by i.Field<string>("LegacyBU") into grp
                       select new buITem
                       {
                           text = grp.Key,
                           value = grp.Key
                       }).ToList();

            /*var disBU = (from i in _bu
                         group i by i.text into grp
                         select new buITem
                         {
                             text = grp,
                             value = grp.Select(x => x.value)
                         }).ToList();
                         */
            return _bu;
        }
        public static bool IsOdd(int value)
        {
            return value % 12 != 0;
        }

        internal IEnumerable<object> AccountSearchListData(DataTable data)
        {

            var results = (from i in data.AsEnumerable()
                           group i by i.Field<string>("AccountID") into grp
                           select new
                           {
                               AccountID = grp.ToList().FirstOrDefault().Field<string>("AccountID"),
                               AccountName = grp.ToList().FirstOrDefault().Field<string>("AccountName"),
                               TotalProposalSalesOpportunity = grp.Sum(x => x.Field<double>("ProposalSalesOpportunity")),
                               Country = grp.ToList().FirstOrDefault().Field<string>("Country"),
                               TotalTopProposals = grp.Sum(x => x.Field<Int64>("TopProposals")),
                               TotalTopLegacyProducts = grp.Sum(x => x.Field<decimal>("TopLegacyProducts")),
                               //Details = grp.ToList(),
                           }).ToList();

            return results;
        }
        #endregion

        internal void InsertSelectedAccountIds(string accountIds, string userGUID)
        {
            string connectionString = ConfigurationManager.AppSettings["DbConnection"].ToString();
            QueryBuilder.RemoveAccountIdsByUserGuidSP(userGUID, connectionString);
            foreach (var accountId in accountIds.Split('|'))
            {
                QueryBuilder.InsertSelectedAccountIdsSP(accountId, userGUID, connectionString);
            }
            
        }
        internal async Task SendProposalToQueue(string proposalId, string userGUID, string userId)
        {
            string connectionString = ConfigurationManager.AppSettings["DbConnection"].ToString();
            await QueryBuilder.SendProposalToQueueSP(proposalId, userGUID, userId, connectionString);
        }
        class buITem
        {
            public string text { get; set; }
            public string value { get; set; }
        }

    }
}