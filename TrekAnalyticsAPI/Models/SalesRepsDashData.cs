using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnalyticsAPI.Models
{
    public class SalesRepsDashData
    {

        public string CurrentUser { get; set; }
        public string Version { get; set; }
        public string DateRefreshed { get; set; }
        public IEnumerable<object> Site_Card { get; set; }
        public IEnumerable<object> SalesTeamOpp_StackedBar { get; set; }
        public IEnumerable<object> TotalAccounts_MixedChart { get; set; }
        public IEnumerable<object> Pipeline_Pie { get; set; }
        public IEnumerable<object> TopAccounts_StackedBar { get; set; }
        public IEnumerable<object> TopLegacyLegacyPlateform_StackedBar { get; set; }
        public int CallerID { get; set; }
        public IEnumerable<object> Proposals_Pie { get; set; }
        public string UserGUID { get; set; }

    }
}