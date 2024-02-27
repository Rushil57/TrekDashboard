using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AnalyticsAPI.Models
{
    public class tbldashboardsalesrep
    {
        //public int Id { get; set; }
        public int SalesRepUserId { get; set; }
        public string SalesRepFullName { get; set; }
        public string SalesRepEmail { get; set; }
        public string SalesRepSalesRole { get; set; }
        public Nullable<int> SalesRepLevel { get; set; }
        public string SalesRepTreatmentcode { get; set; }
        public Nullable<bool> SalesRepSFDCPrimary { get; set; }
        public Nullable<int> SalesRepManagerUserId { get; set; }
        public string SalesRepManagerFullName { get; set; }
        public string SalesRepManagerEmail { get; set; }
        public string AccountID { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public string AccountIndustry { get; set; }
        public string AccountSuperRegion { get; set; }
        public string AccountSubRegion { get; set; }
        public string AccountRegion { get; set; }
        public string AccountCountry { get; set; }
        public string AccountState { get; set; }
        public string AccountCity { get; set; }
        public int ProposalId { get; set; }
        public string BundledLeadID { get; set; }
        public int ProposalIsHighRank { get; set; }
        public string ProposalRank { get; set; }
        public Nullable<double> ProposalScore { get; set; }
        public Nullable<float> ProposalSalesOpportunity { get; set; }
        public Nullable<double> ProposalTCOSavings { get; set; }
        public string ProposedShortName { get; set; }
        public string ProposedPlatformName { get; set; }
        public Nullable<int> ProposedQuantity { get; set; }
        public string ProposedBU { get; set; }
        public int LegacyProdutId { get; set; }
        public string ProposalSFDCStatus { get; set; }
        public string SingleProposalUrl { get; set; }
        public string LegacyShortName { get; set; }
        public string LegacyPlatformName { get; set; }
        public Nullable<int> LegacyQuantity { get; set; }
        public string LegacyBU { get; set; }
        public Nullable<System.DateTime> LegacyContractEndDate { get; set; }
        public Nullable<int> LegacyAge { get; set; }
        public Nullable<System.DateTime> LegacyEOSLDate { get; set; }
        public string GUID { get; set; }
        public string SingleAccountUrl { get; set; }
        public Nullable<int> InputID { get; set; }
        public string SiteID { get; set; }
        public string SiteCountry { get; set; }
        public string SiteState { get; set; }
        public string SiteCity { get; set; }
        public string SiteName { get; set; }
    }
    public static class QueryBuilder
    {
        public static string SalesRepIDS(int userID, string connectionString)
        {
            List<string> retVal = new List<string>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandText = @"CALL db_SalesteamDashboard(@user);";
                    command.Parameters.Add("user", MySqlDbType.Int32).Value = userID;

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            retVal.Add(reader["SalesRepUserId"].ToString());
                        }
                    }
                }
            }

            return string.Join(",", retVal);
        }
        public static List<tbldashboardsalesrep> SaleRepSPAggregated(int? userID, string accountID, string whereCondition, string connectionString)
        {
            List<tbldashboardsalesrep> retVal = new List<tbldashboardsalesrep>();

            string sqlString = @"CALL db_SingleSalesRepDashBoard_Aug3(@user, @account, @condition);";

            MySqlDataAdapter adapter = new MySqlDataAdapter(sqlString, connectionString);
            DataSet resultsGrid = new DataSet();
            adapter.SelectCommand.CommandTimeout = 0;
            adapter.SelectCommand.Parameters.AddWithValue("@user", userID.HasValue ? userID.Value : 0);
            adapter.SelectCommand.Parameters.AddWithValue("@account", accountID);
            adapter.SelectCommand.Parameters.AddWithValue("@condition", whereCondition);
            adapter.Fill(resultsGrid, "dbreps");

            foreach (DataRow row in resultsGrid.Tables[0].Rows)
            {
                retVal.Add(new tbldashboardsalesrep
                {
                    //Id = Convert.ToInt32(row["Id"].ToString()),
                    SalesRepUserId = Convert.ToInt32(row["SalesRepUserId"].ToString()),
                    SalesRepFullName = row["SalesRepFullName"].ToString(),
                    SalesRepEmail = row["SalesRepEmail"].ToString(),
                    SalesRepSalesRole = row["SalesRepSalesRole"].ToString(),
                    SalesRepLevel = row["SalesRepLevel"].ToString() == "" ? 1 : Convert.ToInt32(row["SalesRepLevel"].ToString()),
                    SalesRepTreatmentcode = row["SalesRepTreatmentcode"].ToString(),
                    SalesRepSFDCPrimary = row["SalesRepSFDCPrimary"].ToString() == "1",
                    SalesRepManagerUserId = row["SalesRepManagerUserId"].ToString() == "" ? 0 : Convert.ToInt32(row["SalesRepManagerUserId"].ToString()),
                    SalesRepManagerFullName = row["SalesRepManagerFullName"].ToString(),
                    SalesRepManagerEmail = row["SalesRepManagerEmail"].ToString(),
                    AccountID = row["AccountID"].ToString(),
                    AccountName = row["AccountName"].ToString(),
                    AccountType = row["AccountType"].ToString(),
                    AccountIndustry = row["AccountIndustry"].ToString(),
                    AccountSuperRegion = row["AccountSuperRegion"].ToString(),
                    AccountSubRegion = row["AccountSubRegion"].ToString(),
                    AccountRegion = row["AccountRegion"].ToString(),
                    AccountCountry = row["AccountCountry"].ToString(),
                    AccountState = row["AccountState"].ToString(),
                    AccountCity = row["AccountCity"].ToString(),
                    ProposalId = row["ProposalId"].ToString() == "" ? 0 : Convert.ToInt32(row["ProposalId"].ToString()),
                    BundledLeadID = row["BundledLeadID"].ToString(),
                    ProposalIsHighRank = row["ProposalIsHighRank"].ToString() == "" ? 0 : Convert.ToInt32(row["ProposalIsHighRank"].ToString()),
                    ProposalRank = row["ProposalRank"].ToString(),
                    ProposalScore = row["ProposalScore"].ToString() == "" ? 0 : Convert.ToDouble(row["ProposalScore"].ToString()),
                    ProposalSalesOpportunity = (row["ProposalSalesOpportunity"] as float? ?? 0),
                    ProposalTCOSavings = row["ProposalTCOSavings"].ToString() == "" ? 0 : Convert.ToDouble(row["ProposalTCOSavings"].ToString()),
                    ProposedShortName = row["ProposedShortName"].ToString(),
                    ProposedPlatformName = row["ProposedPlatformName"].ToString(),
                    ProposedQuantity = row["ProposedQuantity"].ToString() == "" ? 0 : Convert.ToInt32(row["ProposedQuantity"].ToString()),
                    ProposedBU = row["ProposedBU"].ToString(),
                    LegacyProdutId = Convert.ToInt32(row["LegacyProdutId"].ToString()),
                    ProposalSFDCStatus = row["ProposalSFDCStatus"].ToString(),
                    SingleProposalUrl = row["SingleProposalUrl"].ToString(),
                    LegacyShortName = row["LegacyShortName"].ToString(),
                    LegacyPlatformName = row["LegacyPlatformName"].ToString(),
                    LegacyQuantity = row["LegacyQuantity"].ToString() == "" ? 0 : Convert.ToInt32(row["LegacyQuantity"].ToString()),
                    LegacyBU = row["LegacyBU"].ToString(),
                    LegacyContractEndDate = row["LegacyContractEndDate"] as DateTime?,
                    LegacyAge = row["LegacyAge"].ToString() == "" ? 0 : Convert.ToInt32(row["LegacyAge"].ToString()),
                    LegacyEOSLDate = row["LegacyEOSLDate"] as DateTime?,
                    //GUID = row["GUID"].ToString(),
                    SingleAccountUrl = row["SingleAccountUrl"].ToString(),
                    InputID = row["InputID"].ToString() == "" ? 1 : Convert.ToInt32(row["InputID"].ToString()),
                    SiteID = row["SiteID"].ToString(),
                    SiteCountry = row["SiteCountry"].ToString(),
                    SiteState = row["SiteState"].ToString(),
                    SiteCity = row["SiteCity"].ToString(),
                    SiteName = row["SiteName"].ToString()
                });
            }

            return retVal;
        }

        public static void DashboardTracking_DelegateMethod(int? userID, string whereCondition, string connectionString, string PageName, string userGUID)
        {
            DashboardTracking_Delegate del = new DashboardTracking_Delegate(DashboardTracking);
            del.BeginInvoke(userID, whereCondition, connectionString, PageName, userGUID, null, null);
        }
        public delegate void DashboardTracking_Delegate(int? userID, string whereCondition, string connectionString, string PageName, string userGUID);
        public static void DashboardTracking(int? userID, string whereCondition, string connectionString, string PageName, string userGUID)
        {
            string sqlString = @"CALL sp_DashboardTracking(@user, @userGUID, @condition, @page);";

            MySqlDataAdapter adapter = new MySqlDataAdapter(sqlString, connectionString);
            DataSet resultsGrid = new DataSet();
            adapter.SelectCommand.CommandTimeout = 0;
            adapter.SelectCommand.Parameters.AddWithValue("@user", userID.HasValue ? userID.Value : 0);
            adapter.SelectCommand.Parameters.AddWithValue("@userGUID", userGUID);
            adapter.SelectCommand.Parameters.AddWithValue("@condition", whereCondition);
            adapter.SelectCommand.Parameters.AddWithValue("@page", PageName);

            adapter.Fill(resultsGrid, "dbreps");
        }
        public static DataSet SaleRepSP(int? userID, string accountID, string whereCondition, string connectionString, string DashboardType, int InitialUserId, string userGUID = "", int accountSearchType = 0)
        {
            //List<tbldashboardsalesrep> retVal = new List<tbldashboardsalesrep>();

            string sqlString = @"CALL db_SingleSalesRepDashBoard_V6(@user, @account, @condition, @dsbtype, @initialuser, @userGUID, @AccSearchType);";
            

            MySqlDataAdapter adapter = new MySqlDataAdapter(sqlString, connectionString);
            DataSet resultsGrid = new DataSet();
            adapter.SelectCommand.CommandTimeout = 0;
            adapter.SelectCommand.Parameters.AddWithValue("@user", userID.HasValue ? userID.Value : 0);
            adapter.SelectCommand.Parameters.AddWithValue("@account", accountID);
            adapter.SelectCommand.Parameters.AddWithValue("@condition", whereCondition);
            adapter.SelectCommand.Parameters.AddWithValue("@dsbtype", DashboardType);
            adapter.SelectCommand.Parameters.AddWithValue("@initialuser", InitialUserId);
            adapter.SelectCommand.Parameters.AddWithValue("@userGUID", userGUID);
            adapter.SelectCommand.Parameters.AddWithValue("@AccSearchType", accountSearchType);

            adapter.Fill(resultsGrid, "dbreps");
            /*
            foreach (DataRow row in resultsGrid.Tables[0].Rows)
            {
                retVal.Add(new tbldashboardsalesrep
                {
                    //Id = Convert.ToInt32(row["Id"].ToString()),
                    SalesRepUserId = Convert.ToInt32(row["SalesRepUserId"].ToString()),
                    SalesRepFullName = row["SalesRepFullName"].ToString(),
                    SalesRepEmail = row["SalesRepEmail"].ToString(),
                    SalesRepSalesRole = row["SalesRepSalesRole"].ToString(),
                    SalesRepLevel = row["SalesRepLevel"].ToString() == "" ? 1 : Convert.ToInt32(row["SalesRepLevel"].ToString()),
                    SalesRepTreatmentcode = row["SalesRepTreatmentcode"].ToString(),
                    SalesRepSFDCPrimary = row["SalesRepSFDCPrimary"].ToString() == "1",
                    SalesRepManagerUserId = row["SalesRepManagerUserId"].ToString() == "" ? 0 : Convert.ToInt32(row["SalesRepManagerUserId"].ToString()),
                    SalesRepManagerFullName = row["SalesRepManagerFullName"].ToString(),
                    SalesRepManagerEmail = row["SalesRepManagerEmail"].ToString(),
                    AccountID = row["AccountID"].ToString(),
                    AccountName = row["AccountName"].ToString(),
                    AccountType = row["AccountType"].ToString(),
                    AccountIndustry = row["AccountIndustry"].ToString(),
                    AccountSuperRegion = row["AccountSuperRegion"].ToString(),
                    AccountSubRegion = row["AccountSubRegion"].ToString(),
                    AccountRegion = row["AccountRegion"].ToString(),
                    AccountCountry = row["AccountCountry"].ToString(),
                    AccountState = row["AccountState"].ToString(),
                    AccountCity = row["AccountCity"].ToString(),
                    ProposalId = row["ProposalId"].ToString() == "" ? 0 : Convert.ToInt32(row["ProposalId"].ToString()),
                    BundledLeadID = row["BundledLeadID"].ToString(),
                    ProposalIsHighRank = row["ProposalIsHighRank"].ToString() == "" ? 0 : Convert.ToInt32(row["ProposalIsHighRank"].ToString()),
                    ProposalRank = row["ProposalRank"].ToString(),
                    ProposalScore = row["ProposalScore"].ToString() == "" ? 0 : Convert.ToDouble(row["ProposalScore"].ToString()),
                    ProposalSalesOpportunity = (row["ProposalSalesOpportunity"] as float? ?? 0),
                    ProposalTCOSavings = row["ProposalTCOSavings"].ToString() == "" ? 0 : Convert.ToDouble(row["ProposalTCOSavings"].ToString()),
                    ProposedShortName = row["ProposedShortName"].ToString(),
                    ProposedPlatformName = row["ProposedPlatformName"].ToString(),
                    ProposedQuantity = row["ProposedQuantity"].ToString() == "" ? 0 : Convert.ToInt32(row["ProposedQuantity"].ToString()),
                    ProposedBU = row["ProposedBU"].ToString(),
                    LegacyProdutId = Convert.ToInt32(row["LegacyProdutId"].ToString()),
                    ProposalSFDCStatus = row["ProposalSFDCStatus"].ToString(),
                    SingleProposalUrl = row["SingleProposalUrl"].ToString(),
                    LegacyShortName = row["LegacyShortName"].ToString(),
                    LegacyPlatformName = row["LegacyPlatformName"].ToString(),
                    LegacyQuantity = row["LegacyQuantity"].ToString() == "" ? 0 : Convert.ToInt32(row["LegacyQuantity"].ToString()),
                    LegacyBU = row["LegacyBU"].ToString(),
                    LegacyContractEndDate = row["LegacyContractEndDate"] as DateTime?,
                    LegacyAge = row["LegacyAge"].ToString() == "" ? 0 : Convert.ToInt32(row["LegacyAge"].ToString()),
                    LegacyEOSLDate = row["LegacyEOSLDate"] as DateTime?,
                    //GUID = row["GUID"].ToString(),
                    SingleAccountUrl = row["SingleAccountUrl"].ToString(),
                    InputID = row["InputID"].ToString() == "" ? 1 : Convert.ToInt32(row["InputID"].ToString()),
                    SiteID = row["SiteID"].ToString(),
                    SiteCountry = row["SiteCountry"].ToString(),
                    SiteState = row["SiteState"].ToString(),
                    SiteCity = row["SiteCity"].ToString(),
                    SiteName = row["SiteName"].ToString()
                });
            }
            */
            return resultsGrid;
        }
        public static void InsertSelectedAccountIdsSP(string accountId, string userGUID, string connectionString)
        {
            string sqlString = @"CALL db_InsertAccountIds(@userGUID, @accountId,@createdOn);";

            MySqlDataAdapter adapter = new MySqlDataAdapter(sqlString, connectionString);
            DataSet resultsGrid = new DataSet();
            adapter.SelectCommand.CommandTimeout = 0;
            adapter.SelectCommand.Parameters.AddWithValue("@userGUID",userGUID);
            adapter.SelectCommand.Parameters.AddWithValue("@accountId", accountId);
            adapter.SelectCommand.Parameters.AddWithValue("@createdOn", DateTime.Now);

            adapter.Fill(resultsGrid, "dbreps");
        }
        public static void RemoveAccountIdsByUserGuidSP(string userGUID, string connectionString)
        {
            string sqlString = @"CALL db_RemoveAccountIds(@userGUID);";

            MySqlDataAdapter adapter = new MySqlDataAdapter(sqlString, connectionString);
            DataSet resultsGrid = new DataSet();
            adapter.SelectCommand.CommandTimeout = 0;
            adapter.SelectCommand.Parameters.AddWithValue("@userGUID", userGUID);
            adapter.Fill(resultsGrid, "dbreps");
        }
        public async static Task SendProposalToQueueSP(string proposalId, string userGUID, string userId, string connectionString)
        {
            string sqlString = @"CALL db_SendProposalToQueue(@proposalId,@userGUID,@userId);";
            // Need to create StoredProcedure : db_SendProposalToQueue
            MySqlDataAdapter adapter = new MySqlDataAdapter(sqlString, connectionString);
            DataSet resultsGrid = new DataSet();
            adapter.SelectCommand.CommandTimeout = 0;
            adapter.SelectCommand.Parameters.AddWithValue("@proposalId", proposalId);
            adapter.SelectCommand.Parameters.AddWithValue("@userGUID", userGUID);
            adapter.SelectCommand.Parameters.AddWithValue("@userId", userId);
            await adapter.FillAsync(resultsGrid, "dbreps");
        }
    }
}