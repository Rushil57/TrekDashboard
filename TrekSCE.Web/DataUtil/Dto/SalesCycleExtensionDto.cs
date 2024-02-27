using System;
using System.Collections.Generic;
using System.Text;

namespace SCE.Web.DataUtil.Dto
{
    public class SalesCycleExtensionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MinTotalStars { get; set; }
        public int MaxTotalStars { get; set; }
        public decimal DefaultValue { get; set; }
        public decimal SavedValue { get; set; }
        public DateTime SavedTimeStamp { get; set; }
    }
}
