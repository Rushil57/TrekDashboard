using System.Collections.Generic;

namespace SalesAnalytics.Web.Dashboard.Models
{
    public class SalesTeam_TopAccount
    {
        public string AccountName { get; set; }
        public decimal Star_1 { get; set; }
        public decimal Stars_2 { get; set; }
        public decimal Stars_3 { get; set; }
        public decimal Stars_4 { get; set; }
        public decimal Stars_5 { get; set; }
        public decimal SalesOpp { get; set; }
        public decimal NoLegacyProd { get; set; }
        public decimal NoProposals { get; set; }
        public decimal TotalSalesOpp { get; set; }
        public SalesTeam_TopAccount_TagDetails Tag { get; set; }
    }

    public class SalesTeam_TopAccount_TagDetails
    {
        public decimal NoLegacyProd { get; set; }
        public decimal NoProposals { get; set; }
    }

    public class GroupData_AllAccount_SalesTeam
    {
        public string AccountName { get; set; }
        public List<SalesTeam_TopAccount> AccountList { get; set; }
    }
}