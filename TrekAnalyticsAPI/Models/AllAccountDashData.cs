using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnalyticsAPI.Models
{
    public class AllAccountDashData
    {
        public string CurrentUser { get; set; }
        public string Version { get; set; }
        public string DateRefreshed { get; set; }
        public IEnumerable<object> Account_Pie { get; set; }
        public IEnumerable<object> TopAccounts_StackedBar { get; set; }
        public IEnumerable<object> ExpiringContracts_MixedChart { get; set; }
        public IEnumerable<object> Proposals_Pie { get; set; }
        public List<TopProposals> TopProposals_Grid { get; set; }
        public IEnumerable<object> TopLegacyProduct { get; set; }
        public IEnumerable<object> Site_Card { get; set; }

        public IEnumerable<object> ListBU { get; set; }
        public int CallerID { get; set; }
        public string UserGUID { get; set; }
        public string DBUserId { get; set; }
        public int UserLevel { get; set; }

        public IEnumerable<object> AccountSearchList { get; set; }
    }
}