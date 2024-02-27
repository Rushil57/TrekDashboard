using System.Collections.Generic;

namespace SalesAnalytics.Web.Dashboard.Models
{
    public class SingleAccountDashDataViewModel
    {
        public string CurrentUser { get; set; }
        public string Version { get; set; }
        public string DateRefreshed { get; set; }
        public IEnumerable<object> Site_Card { get; set; }
        public IEnumerable<object> TopSites_StackedBar { get; set; }
        public IEnumerable<object> ExpiringContracts_MixedChart { get; set; }
        public IEnumerable<object> Proposals_Pie { get; set; }
        public IEnumerable<object> TopProposals_Grid { get; set; }
        public IEnumerable<object> TopLegacyProduct { get; set; }
        public IEnumerable<object> Account_Pie { get; set; }

        public string UserGUID { get; set; }
        public IEnumerable<object> BUList { get; set; }
        public string UserInfoHeader { get; set; }
        public List<AccountViewModel> AccountListData { get; set; }
        public string Name { get; set; }
        public string CurrentUserFromAccountsDash { get; set; }
        public string DBUserId { get; set; }
        public int UserLevel { get; set; }
        public string ApiURLlink { get; set; }
        public string AccountSearchSelectedIds { get; set; }
        
    }
}