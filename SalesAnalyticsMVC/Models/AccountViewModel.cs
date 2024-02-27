namespace SalesAnalytics.Web.Dashboard.Models
{
    public class AccountViewModel
    {
        public string AccountName { get; set; }
        public float TotalProposalSalesOpportunity { get; set; }
        public string LegacyBU { get; set; }
        public string AccountID { get; set; }
        public string Country { get; set; }
        public int TotalTopProposals { get; set; }
        public int TotalTopLegacyProducts { get; set; }
        public string IsSelected { get; set; }

    }
}