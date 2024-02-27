//using AnalyticsAPI.EF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Objects;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace AnalyticsAPI.Models
{
    public class SalesRepsModel : IDisposable
    {
        //private SAnalyticsEntities _ef = new SAnalyticsEntities();

        public void Dispose()
        {
            this.Dispose();
            //_ef.Dispose();
        }

        internal SalesRepsDashData SaleRep(int? userID, string accountID, string whereCondition, string userGUID, int CallerID = 0, int InitialUserId = 0)
        {
            userID = userID == null ? userID = 0 : userID;
            accountID = accountID == null ? "" : accountID;
            whereCondition = whereCondition == null ? "" : whereCondition;
            var dt = new SalesRepsDashData();
            string connectionString = ConfigurationManager.AppSettings["DbConnection"].ToString();
            var userIDS = QueryBuilder.SalesRepIDS(userID.HasValue ? userID.Value : 0, connectionString);

            //var userIDS = "876, 889, 894, 912, 915";
            if (string.IsNullOrEmpty(userIDS))
                return null;
            whereCondition = whereCondition == null ? " AND SalesRepUserId IN (" + userIDS + " )" : whereCondition + " AND SalesRepUserId IN (" + userIDS + " )";

            //var salesData = _ef.SaleRepSP(0, accountID, whereCondition).ToList();
            QueryBuilder.DashboardTracking_DelegateMethod(userID, whereCondition, connectionString, "st", userGUID);

            var salesData = QueryBuilder.SaleRepSP(0, accountID, whereCondition, connectionString, "st", InitialUserId,userGUID);

            /*
            var salRep = SalesRepsCurrentUser(salesData);
            dt.CurrentUser = salRep;
            dt.SalesRepsByAccountType_Pie = SalesRepsByAccountType(salesData);
            dt.SalesTeamOpp_StackedBar = SalesRepsByOpp(salesData, CallerID);
            dt.TotalAccounts_MixedChart = SalesRepsAccount(salesData);
            dt.Pipeline_Pie = SalesRepsPipeline(salesData);
            dt.TopAccounts_StackedBar = SalesRepsTopAccounts(salesData);
            dt.TopLegacyLegacyPlateform_StackedBar = SalesRepsLegacyProd(salesData);
            dt.TopLegacyLegacyPlateform_StackedBar = aam.TopProductsSummary(salesData.Tables[6]);
            */

            AllAccoutsModel aam = new AllAccoutsModel();

            dt.CurrentUser = salesData.Tables[0].Rows[0][0].ToString();
            dt.Version = salesData.Tables[0].Rows[0][1].ToString();
            dt.DateRefreshed = salesData.Tables[0].Rows[0][2].ToString();
            //dt.SalesRepsByAccountType_Pie = SalesRepsByAccountType(salesData.Tables[1]);
            dt.Site_Card = RepsSummary(salesData.Tables[1]);
            dt.SalesTeamOpp_StackedBar = SalesRepsByOpp(salesData.Tables[2], CallerID);
            dt.TotalAccounts_MixedChart = SalesRepsAccount(salesData.Tables[3]);
            dt.Pipeline_Pie = SalesRepsPipeline(salesData.Tables[4]);
            dt.TopAccounts_StackedBar = SalesRepsTopAccounts(salesData.Tables[5]);
            dt.TopLegacyLegacyPlateform_StackedBar = SalesRepsLegacyProd(salesData.Tables[6]);
            dt.Proposals_Pie = Proposals(salesData.Tables[7]);
            dt.CallerID = CallerID;
            dt.UserGUID = userGUID;
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
        private string SalesRepsCurrentUser(List<tbldashboardsalesrep> data)
        {
            var user = (from i in data select i.SalesRepManagerFullName).FirstOrDefault();
            return user;
        }

        private IEnumerable<object> RepsSummary(DataTable data)
        {
            var results = (from i in data.AsEnumerable()
                           select new
                           {
                               TotalSalesOpps = i.Field<double>("TotalSalesOpps"),
                               NoProposals = i.Field<Int64>("NoProposals").ToString("#,##0"),
                               NoAccounts = i.Field<Int64>("NoAccounts").ToString("#,##0"),
                               NoSite = i.Field<Int64>("NoSite").ToString("#,##0"),
                               TotalLegacyQty = i.Field<decimal>("LegacyQuantity").ToString("#,##0"),
                               TotalSalesReps = i.Field<Int64>("TotalSalesReps").ToString("#,##0"),
                               HighRankingSalesOpps = i.Field<Int64>("HighRankingSalesOpps"),
                               HighRankingPropsals = i.Field<Int64>("HighRankingPropsals").ToString("#,##0"),
                               SiteCities = i.Field<Int64>("SiteCities").ToString("#,##0"),
                           }).AsEnumerable();

            return results;
        }
        private IEnumerable<object> SalesRepsByAccountType(DataTable data)
        {
            var results = (from i in data.AsEnumerable()
                           group i by i.Field<string>("AccountType") into grp
                           select new
                           {
                               AccountType = grp.Key,
                               NoSalesRep = grp.Select(x => x.Field<Int64>("SalesRepUserId")),
                               NoAccounts = grp.Select(x => x.Field<Int64>("AccountID")),
                           }).AsEnumerable();

            return results;
        }

        private IEnumerable<object> SalesRepsByOpp(DataTable data, int CallerID)
        {
            var query = (from i in data.AsEnumerable()
                         group i by new { SalesRepFullName = i.Field<string>("SalesRepFullName"), SalesRepUserId = i.Field<int>("SalesRepUserId"), SalesRepLevel = i.Field<int>("SalesRepLevel") } into grp
                         select new
                         {
                             SalesRepName = grp.Key.SalesRepFullName,
                             SalesRepID = grp.Key.SalesRepUserId,
                             SalesRepLevel = grp.Key.SalesRepLevel,
                             AccountCount = grp.Select(x => x.Field<string>("AccountID")).Distinct().Count(),
                             NoProposals = grp.Select(x => x.Field<int>("ProposalId")).Distinct().Count(),
                             Details = grp.ToList(),
                             CallerID = CallerID
                         }
                      ).OrderByDescending(x => x.NoProposals).ToList();

            List<dynamic> AggregatedData = new List<dynamic>();
            var ranges = new[] { "TotalOpps", "1 Star", "2 Stars", "3 Stars", "4 Stars", "5 Stars" };

            foreach (var item in query)
            {
                var t = (from range in ranges
                         join i in item.Details on range equals ranges.Where(x => i.Field<string>("ProposalRank").ToLower() == x.ToLower()).Last() into groups
                         select
                         new
                         {
                             Rank = range,
                             NoAccount = groups.Select(x => x.Field<string>("AccountID")).Distinct().Count(),
                             SalesOpp = groups.Sum(h => h.Field<decimal>("ProposalSalesOpportunity")),
                             NoProposals = groups.Select(x => x.Field<int>("ProposalId")).Distinct().Count()
                         }).ToArray();

                foreach (var age in t)
                {
                    IDictionary<string, object> newItem = new ExpandoObject();
                    newItem["SalesRepName"] = item.SalesRepName;
                    newItem["Rank"] = age.Rank != "TotalOpps" ? age.Rank : "Total Sales Opp";
                    newItem["Proposals"] = age.Rank != "TotalOpps" ? age.NoProposals : 0;
                    newItem["TotalSalesOpp"] = item.Details.Sum(x => x.Field<decimal>("ProposalSalesOpportunity"));
                    newItem["Tag"] = new { NoProposals = age.NoProposals, NoAccounts = age.NoAccount, SaleOpps = age.SalesOpp, SalesRepID = item.SalesRepID, SalesRepLevel = item.SalesRepLevel, CallerID = item.CallerID };
                    AggregatedData.Add(newItem);
                }
            }

            return AggregatedData.OrderBy(x => x.Rank);
        }

        private IEnumerable<object> SalesRepsAccount(DataTable data)
        {

            var query = (from i in data.AsEnumerable()
                         group i by new { AccountIndustry = i.Field<string>("AccountIndustry") } into grp
                         orderby grp.Select(x => new { AccountID = x.Field<string>("AccountID") }).Distinct().Count() descending
                         select
                         new
                         {
                             AccountIndustry = grp.Key.AccountIndustry,
                             AccountCount = grp.Select(x => new { AccountID = x.Field<string>("AccountID"), LegacyBU = x.Field<string>("LegacyBU") }).Distinct().Count(),
                             TotalLegacyQty = grp.Sum(x => x.Field<int>("LegacyQuantity")),
                             Details = grp.GroupBy(x => x.Field<string>("LegacyBU")).Select(h => new { BU = h.Key, AccountsCount = h.Select(t => t.Field<string>("AccountID")).Distinct().Count(), SalesOpp = h.Sum(x => x.Field<decimal>("ProposalSalesOpportunity")), SalesRepCount = h.Select(x => x.Field<int>("SalesRepUserId")).Distinct().Count() }),
                         }).Take(10).ToList();

            var buKeys = BUList(data);

            List<dynamic> AggregatedData = new List<dynamic>();
            foreach (var item in query)
            {
                var t = (from keys in buKeys

                         join i in item.Details on keys.text equals i.BU into g
                         select new
                         {

                             LegacyBU = keys.text,
                             NoAccounts = g.Sum(x => x.AccountsCount),
                             SalesOpp = (int)g.Sum(x => x.SalesOpp),
                             SalesRepCount = g.Sum(x => x.SalesRepCount)
                         }).Distinct().ToArray();

                foreach (var subItem in t)
                {
                    IDictionary<string, object> newItem = new ExpandoObject();
                    newItem["AccountIndustry"] = item.AccountIndustry;
                    newItem["TotalAccounts"] = item.AccountCount;
                    newItem["TotalLegacyQty"] = item.TotalLegacyQty;
                    newItem["LegacyBU"] = subItem.LegacyBU;
                    newItem["NoAccounts"] = subItem.NoAccounts;
                    newItem["Tag"] = new { SalesRepCount = subItem.SalesRepCount, SalesOpp = subItem.SalesOpp };
                    AggregatedData.Add(newItem);
                }
            }

            return AggregatedData.OrderByDescending(x => x.TotalAccounts);
        }

        private IEnumerable<object> SalesRepsPipeline(DataTable data)
        {
            var query = (from i in data.AsEnumerable()
                         select
                  new
                  {
                      ProposalSFDCStatus = i.Field<string>("ProposalSFDCStatus"),
                      NoProposals = i.Field<Int64>("ProposalId"),
                      Tag = new
                      {
                          NoAccounts = i.Field<Int64>("AccountID"),
                          TotalSalesOpp = i.Field<double>("ProposalSalesOpportunity")
                      }
                  }).OrderBy(x => x.NoProposals).ToList();

            return query;
        }

        private IEnumerable<object> SalesRepsTopAccounts(DataTable data)
        {
            var query = (from i in data.AsEnumerable()
                         group i by new { AccountName = i.Field<string>("AccountName") } into grp
                         select new
                         {
                             AccountName = grp.Key.AccountName,
                             SalesOpp = grp.Sum(x => x.Field<decimal>("ProposalSalesOpportunity")),
                             Details = grp.ToList()
                         }).OrderByDescending(x => x.SalesOpp).ToList();


            List<dynamic> AggregatedData = new List<dynamic>();
            foreach (var item in query)
            {
                var t = item.Details.GroupBy(x => x.Field<string>("ProposalRank")).Select(h =>
                  new
                  {
                      ProposalRank = h.Key,

                      NoProposals = h.Select(x => x.Field<int>("ProposalId")).Distinct().Count(),
                      NoLegacyProd = h.Select(x => x.Field<int>("LegacyProdutId")).Distinct().Count(),
                      SalesOpp = h.Sum(x => x.Field<decimal>("ProposalSalesOpportunity"))

                  }).Distinct().ToArray();

                foreach (var rank in t)
                {
                    IDictionary<string, object> newItem = new ExpandoObject();
                    newItem["AccountName"] = item.AccountName;
                    newItem["Star_1"] = rank.ProposalRank.ToLower() == "1 star" ? rank.SalesOpp : 0;
                    newItem["Stars_2"] = rank.ProposalRank.ToLower() == "2 stars" ? rank.SalesOpp : 0;
                    newItem["Stars_3"] = rank.ProposalRank.ToLower() == "3 stars" ? rank.SalesOpp : 0;
                    newItem["Stars_4"] = rank.ProposalRank.ToLower() == "4 stars" ? rank.SalesOpp : 0;
                    newItem["Stars_5"] = rank.ProposalRank.ToLower() == "5 stars" ? rank.SalesOpp : 0;
                    newItem["SalesOpp"] = rank.SalesOpp;
                    newItem["NoLegacyProd"] = rank.NoLegacyProd;
                    newItem["NoProposals"] = rank.NoProposals;
                    newItem["TotalSalesOpp"] = item.SalesOpp;
                    newItem["Tag"] = new { NoProposals = rank.NoProposals, NoLegacyProd = rank.NoLegacyProd };
                    AggregatedData.Add(newItem);
                }
            }


            //return AggregatedData.Distinct().OrderBy(x => x.TotalSalesOpp).ThenByDescending(x => x.SalesOpp);
            return AggregatedData.Distinct().OrderByDescending(x => x.TotalSalesOpp);
        }


        private IEnumerable<object> SalesRepsLegacyProd(DataTable data)
        {

            var query = (from i in data.AsEnumerable()
                         group i by i.Field<string>("LegacyPlatformName") into grp
                         orderby grp.Sum(x => x.Field<int>("LegacyQuantity")) descending
                         select new
                         {
                             LegacyPlatformName = grp.Key,
                             TotalQty = grp.Sum(x => x.Field<int>("LegacyQuantity")),
                             Details = grp.GroupBy(x => x.Field<int>("LegacyAge")).Select(x => new { Age = x.Key, LegacyQty = x.Sum(h => h.Field<int>("LegacyQuantity")), NoAccounts = x.Select(h => h.Field<string>("AccountID")).Distinct().Count(), SalesOpp = x.Sum(h => h.Field<decimal>("ProposalSalesOpportunity")) })
                         }).Take(10);

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
                         join i in item.Details on range equals ranges.Where(x => i.Age >= x.StartMonth && i.Age <= x.EndMonth).LastOrDefault() into g
                         select new
                         {
                             Age = range.MonthLabel,
                             AgeKey = range.EndMonth,
                             NoAccount = g.Sum(x => x.NoAccounts),
                             LegacyQty = g.Sum(x => x.LegacyQty),
                             SalesOpp = g.Sum(x => x.SalesOpp)
                         }).Distinct().ToArray();

                foreach (var subItem in t)
                {
                    IDictionary<string, object> newItem = new ExpandoObject();
                    newItem["LegacyPlatformName"] = item.LegacyPlatformName;
                    newItem["LegacyAge"] = subItem.Age;
                    newItem["Qty"] = subItem.NoAccount;
                    newItem["SalesOpp"] = subItem.SalesOpp;
                    newItem["LegacyQty"] = subItem.LegacyQty;
                    newItem["TotalQty"] = item.TotalQty;
                    newItem["Tag"] = new { LegacyQty = subItem.LegacyQty, Qty = subItem.NoAccount, SalesOpp = subItem.SalesOpp, AgeKey = subItem.AgeKey };
                    AggregatedData.Add(newItem);
                }
            }
            return AggregatedData.OrderBy(x => x.TotalQty);//.ThenBy(x => x.LegacyQty);
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

        private IEnumerable<BUItem> BUList(DataTable data)
        {
            var _bu = (from i in data.AsEnumerable() select new BUItem { text = i.Field<string>("LegacyBU") }).Distinct().ToList();
            var totalQy = new BUItem { text = "LegacyQty" };
            _bu.Add(totalQy);
            return _bu;
        }

        class BUItem
        {
            internal string text { get; set; }

        }
    }
}