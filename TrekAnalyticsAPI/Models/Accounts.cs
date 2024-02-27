using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace AnalyticsAPI.Models
{
    public class Accounts
    {
        public string Status { get; set; }
        //public int Count { get; set; }
        public int NoAccounts { get; set; }
        public int NoProposals { get; set; }

        public int TotalSalesOpp { get; set; }
        // public List<object> Details { get; set; }

    }
 

    public class ExpiringContracts
    {
        public DateTime ExpiryDate { get; set; }

        public int NoProposals { get; set; }
        public int TotalSalesOpp { get; set; }

    }

    public class Proposals
    {
        public int NoProposals { get; set; }
        public string ProposaRank { get; set; }
        public int TotalSalesOpp { get; set; }
    }

    public class TopProposals
    {
        public int ProposalID { get; set; }
        public string Rank { get; set; }
        public decimal SalesOpportunity { get; set; }
        public string LegacyProduct { get; set; }
        public int? LegacyQty { get; set; }
        public string ProposedProduct { get; set; }
        public int? ProposedQty { get; set; }
        public double? ProposalScore { get; set; }
        public string ProposalURL { get; set; }
        public int CallerID { get; set; }
    }

    public class TopLegacyProduct
    {
        public string LegacyName { get; set; }
        public int? LegacyAge { get; set; }
        public int? LegacyQty { get; set; }
    }
}