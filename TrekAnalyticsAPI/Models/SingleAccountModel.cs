//using AnalyticsAPI.EF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace AnalyticsAPI.Models
{
    public class SingleAccountModel : IDisposable
    {
        //private SAnalyticsEntities _ef = new SAnalyticsEntities();

        public void Dispose()
        {
            this.Dispose();
            //_ef.Dispose();
        }

        internal SingleAccountDashData SaleRep(int? userID, string accountID, string whereCondition, int CallerID, int InitialUser, string userGUID, int AccountSearchType)
        {
            userID = userID == null ? userID = 0 : userID;
            accountID = accountID == null ? "" : accountID;
            whereCondition = whereCondition == null ? "" : whereCondition;
            var dt = new SingleAccountDashData();
            string connectionString = ConfigurationManager.AppSettings["DbConnection"].ToString();
            //var allData = _ef.SaleRepSP(userID, accountID, whereCondition).ToList();

            QueryBuilder.DashboardTracking_DelegateMethod(userID, whereCondition, connectionString, "sa", userGUID);
            var allData = QueryBuilder.SaleRepSP(userID, accountID, whereCondition, connectionString, "sa", InitialUser, userGUID, AccountSearchType);

            AllAccoutsModel aam = new AllAccoutsModel();
            dt.BUList = aam.BUListFilter(allData.Tables[0]);
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
            
            dt.Site_Card = AccountsSummary(allData.Tables[2]);
            dt.TopSites_StackedBar = TopAccountsSummary(allData.Tables[3]);
            //dt.ExpiringContracts_MixedChart = aam.ExpiringContracts(allData.Tables[4]);
            dt.ExpiringContracts_MixedChart = ExpiringContractsSA(allData.Tables[4]);
            dt.Proposals_Pie = aam.Proposals(allData.Tables[5]);
            dt.TopProposals_Grid = aam.TopProposal(allData.Tables[6], CallerID);
            dt.TopLegacyProduct = aam.TopProductsSummary(allData.Tables[7]);
            dt.Account_Pie = AccountsSummaryPipeLine(allData.Tables[8]);

            dt.UserGUID = userGUID;
            dt.DBUserId = Convert.ToString(userID);
            dt.UserLevel = Convert.ToInt32(allData.Tables[10].Rows[0][0]);
            dt.AccountSearchList = AccountSearchListData(allData.Tables[9]);
            return dt;
        }
        public void SendEmail(string MailMessageBody, string InitialUser)
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
            var user = (from i in data select string.Concat(i.AccountName, ". (", i.AccountID, ")")).FirstOrDefault();
            return user;
        }
        internal IEnumerable<object> AccountsSummaryPipeLine(DataTable data)
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
        private IEnumerable<object> AccountsSummary(DataTable data)
        {
            var results = (from i in data.AsEnumerable()
                           group i by i.Field<string>("AccountID") into grp
                           select new
                           {

                               NoSite = grp.Select(x => x.Field<Int64>("SiteID").ToString("#,##0")),
                               NoSiteStates = grp.Select(x => x.Field<Int64>("SiteState").ToString("#,##0")),
                               NoSiteCities = grp.Select(x => x.Field<Int64>("SiteCity").ToString("#,##0")),
                               NoProposals = grp.Select(x => x.Field<Int64>("ProposalId").ToString("#,##0")),
                               TotalSalesOpps = grp.Sum(x => x.Field<double>("ProposalSalesOpportunity")),
                               TotalLegacyQty = grp.Sum(x => x.Field<decimal>("LegacyQuantity")).ToString("#,##0"),

                               HighRankingTotalSalesOpps = grp.Select(x => x.Field<Int64>("HighRankingTotalSalesOpps")),
                               HighRankingProposals = grp.Select(x => x.Field<Int64>("HighRankingProposals").ToString("#,##0")),
                               Accounts = grp.Select(x => x.Field<Int64>("Accounts").ToString("#,##0"))
                           }).AsEnumerable();

            return results;
        }
        private IEnumerable<object> TopAccountsSummary(DataTable data)
        {

            var query = (from i in data.AsEnumerable()
                         group i by new { SiteCity = i.Field<string>("SiteCity") }
            into grp
                         select new
                         {
                             SiteCity = grp.Key.SiteCity,
                             TotalSales = grp.Sum(x => x.Field<decimal>("ProposalSalesOpportunity")),
                             Details = grp.ToList()
                         }
            ).OrderByDescending(x => x.TotalSales).Take(10).AsEnumerable();

            List<dynamic> AggregatedData = new List<dynamic>();

            foreach (var item in query)
            {
                var t = item.Details.GroupBy(x => new { SiteID = x.Field<string>("SiteID"), SiteState = x.Field<string>("SiteState"), SiteName = x.Field<string>("SiteName") }).Select(h =>
                       new
                       {
                           SiteID = h.Key.SiteID,
                           SiteName = h.Key.SiteName,
                           TotalSalesOpp = h.Sum(x => x.Field<decimal>("ProposalSalesOpportunity")),
                           SiteState = h.Key.SiteState,
                           NoProposals = h.Sum(x => x.Field<Int64>("ProposalId")),
                           SiteUrl = h.Select(x=>x.Field<string>("SiteUrl")).FirstOrDefault()
                       }).ToArray();

                foreach (var lbu in t)
                {
                    IDictionary<string, object> newItem = new ExpandoObject();
                    newItem["SiteName"] = lbu.SiteName;
                    //newItem["SiteCity"] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.SiteCity.ToLower());
                    newItem["SiteCity"] = item.SiteCity;
                    newItem["SiteID"] = lbu.SiteID;
                    newItem["SiteState"] = lbu.SiteState;
                    newItem["TotalSalesOpp"] = lbu.TotalSalesOpp;
                    newItem["Sales"] = item.TotalSales;
                    newItem["NoProposals"] = lbu.NoProposals;
                    newItem["SiteUrl"] = lbu.SiteUrl;

                    AggregatedData.Add(newItem);
                }

            }
            var lastQuery = (from i in AggregatedData
                             group i by new { i.SiteCity, i.SiteID } into x
                             select new
                             {
                                 SiteCity = x.Key.SiteCity,
                                 SiteID = x.Key.SiteID,
                                 SiteUrl = x.Select(h=>h.SiteUrl).FirstOrDefault(),
                                 Sales = x.Sum(h => (decimal)h.Sales),
                                 TotalSalesOpp = x.Sum(h => (decimal)h.TotalSalesOpp),
                                 Details = new
                                 {
                                     SiteState = x.Select(h => h.SiteState).FirstOrDefault(),
                                     SiteName = x.Select(h => h.SiteName).FirstOrDefault(),
                                     NoProposals = x.Sum(h => (decimal)h.NoProposals),
                                     SiteUrl = x.Select(h => h.SiteUrl).FirstOrDefault(),
                                 }
                             }).ToList();
            return lastQuery.OrderBy(x => x.Sales);

        }
        private IEnumerable<object> ExpiringContracts(List<tbldashboardsalesrep> data)
        {
            var results = (from i in data
                           where i.LegacyContractEndDate >= DateTime.Today.AddMonths(-3) && i.LegacyContractEndDate <= DateTime.Today.AddMonths(8)
                           group i by i.LegacyContractEndDate.Value.ToString("MMM yyyy") into grp
                           select new
                           {
                               ExpiryDate = grp.Key,
                               SortDate = DateTime.Parse(grp.Key),
                               LegacyQty = grp.Sum(c => c.LegacyQuantity),
                               NoProposals = grp.Select(x => x.ProposalId).Distinct().Count(),
                               TotalSalesOpp = (int)grp.Sum(x => x.ProposalSalesOpportunity),
                               Tag = new
                               {
                                   LegacyQty = grp.Sum(c => c.LegacyQuantity),
                                   NoProposals = grp.Select(x => x.ProposalId).Distinct().Count(),
                                   TotalSalesOpp = (int)grp.Sum(x => x.ProposalSalesOpportunity),
                               }
                           }

            ).OrderBy(c => c.SortDate).ToList();

            return results;
        }
        private IEnumerable<object> Proposals(List<tbldashboardsalesrep> data)
        {
            var results = (from i in data
                           group i by i.ProposalRank into grp
                           select new
                           {
                               ProposaRank = grp.Key,
                               NoProposals = grp.Select(x => x.ProposalId).Distinct().Count(),
                               TotalSalesOpp = (int)grp.Sum(x => x.ProposalSalesOpportunity)
                           }).OrderBy(x => x.ProposaRank).ToList();

            return results;
        }
        private IEnumerable<object> TopProposal(List<tbldashboardsalesrep> data)
        {
            var query = (from i in data
                         group i by new { i.ProposalId, i.ProposalRank, i.LegacyShortName, i.ProposedShortName } into grp
                         select new
                         {
                             ProposalID = grp.Key.ProposalId,
                             Rank = grp.Key.ProposalRank,
                             SalesOpportunity = grp.Sum(x => x.ProposalSalesOpportunity),
                             LegacyProduct = grp.Key.LegacyShortName,
                             LegacyQty = grp.Sum(x => x.LegacyQuantity),
                             ProposedProduct = grp.Key.ProposedShortName,
                             ProposedQty = grp.Sum(x => x.ProposedQuantity),
                             ProposalScore = grp.Sum(x => x.ProposalScore),
                             ProposalURL = grp.Select(h => h.SingleProposalUrl).FirstOrDefault()

                         }).OrderByDescending(x => x.ProposalScore).ThenByDescending(x => x.SalesOpportunity).ThenBy(x => x.ProposalID).Take(10).ToList();

            return query;
        }
        private IEnumerable<object> TopProductsSummary(List<tbldashboardsalesrep> data)
        {

            var query = (from i in data
                         group i by new { i.LegacyShortName } into grp
                         select new
                         {
                             LegacyName = grp.Key.LegacyShortName,
                             Details = grp.ToList(),
                             TotalQty = grp.Sum(x => x.LegacyQuantity),

                         }).OrderByDescending(x => x.TotalQty).Take(10).ToList();

            List<dynamic> AggregatedData = new List<dynamic>();
            List<AgeRanges> ranges = new List<AgeRanges>();

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
                         join i in item.Details on range equals ranges.Where(x => i.LegacyAge >= x.StartMonth && i.LegacyAge <= x.EndMonth).LastOrDefault() into groups
                         select new { Age = range.MonthLabel, AgeKey = range.EndMonth, Qty = groups.Sum(x => x.LegacyQuantity), SalesOpp = groups.Sum(x => x.ProposalSalesOpportunity), Proposals = groups.Select(x => x.ProposalId).Distinct().Count() }).ToArray();
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
            return AggregatedData.OrderBy(x => x.TotalQty); //.ThenByDescending(x => x.Qty);

        }
        internal IEnumerable<object> AccountSearchListData(DataTable data)
        {

            var results = (from i in data.AsEnumerable()
                           group i by i.Field<string>("AccountID") into grp
                           select new
                           {
                               AccountID = grp.ToList().FirstOrDefault().Field<string>("AccountID"),
                               AccountName = grp.ToList().FirstOrDefault().Field<string>("AccountName"),
                               TotalProposalSalesOpportunity = grp.Sum(x => x.Field<float>("ProposalSalesOpportunity")),
                               Country = grp.ToList().FirstOrDefault().Field<string>("Country"),
                               TotalTopProposals = grp.Sum(x => x.Field<Int64>("TopProposals")),
                               TotalTopLegacyProducts = grp.Sum(x => x.Field<Int64>("TopLegacyProducts")),
                               //Details = grp.ToList(),
                           }).ToList();

            return results;
        }
        internal IEnumerable<object> ExpiringContractsSA(DataTable data)
        {
            var results = (from i in data.AsEnumerable()
                           group i by i.Field<DateTime>("LegacyContractEndDate").ToString("MMM yyyy") into grp
                           select new
                           {
                               ExpiryDate = grp.Key,
                               SortDate = DateTime.Parse(grp.Key),
                               NoProposals = (int)grp.Sum(x => x.Field<Int64>("ProposalId")),
                               TotalSalesOpp = (double)grp.Sum(x => x.Field<double>("ProposalSalesOpportunity")),
                               LegacyQty = grp.Sum(x => x.Field<int>("LegacyQuantity")),
                           }

            ).OrderBy(x => x.SortDate).ToList();

            return results;
        }
        private IEnumerable<object> BUList(List<tbldashboardsalesrep> data)
        {
            var _bu = (from i in data select new { text = i.ProposedBU, value = string.Concat("'", i.ProposedBU, "'") }).Distinct();
            return _bu;
        }
        private IEnumerable<object> BUListFilter(DataTable data)
        {
            var _bu = (from i in data.AsEnumerable() select new { text = i["Text"], value = i["Value"] }).ToList();
            return _bu;
        }
        public static bool IsOdd(int value)
        {
            return value % 12 != 0;
        }

        #endregion
    }
}