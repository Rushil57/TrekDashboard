using System;
using System.Collections.Generic;
using System.Text;

namespace SCE.Web.DataUtil.Dto
{
    public class SaleSlapseDaysDto
    {
        public int Id { get; set; }
        public string DealSizeBandId { get; set; }
        public int SalesCallsDefaultValue { get; set; }
        public int SalesCallsSavedValue { get; set; }
        public int CallGapDaysDefaultValue { get; set; }
        public int CallGapDaysSavedValue { get; set; }
        public DateTime SavedTimeStamp { get; set; }
        public int SalesLapseDays { get; set; }
    }
}
