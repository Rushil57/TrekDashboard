using System.Collections.Generic;

namespace SalesAnalytics.Web.Dashboard.Models
{
    public class AllAcc_TopAccount
    {
        public string AccountName { get; set; }
        public decimal TotalSalesOpp { get; set; }
        public string LegacyBU { get; set; }
        public string AccountUrl { get; set; }
        public decimal SalesOpp { get; set; }
        public AllAcc_TopAccount_Details Details { get; set; }
    }
    public class AllAcc_TopAccount_Details
    {
        public decimal CallerID { get; set; }
        public decimal UserID { get; set; }
        public decimal NoProposals { get; set; }
        public decimal TotalSalesOpp { get; set; }
        public string NewUrl { get; set; }
        public string AccountID { get; set; }
        public string AccountUrl { get; set; }
    }
    public class GroupData_AllAccount
    {
        public string AccountName { get; set; }
        public List<AllAcc_TopAccount> AccountList { get; set; }
    }
}