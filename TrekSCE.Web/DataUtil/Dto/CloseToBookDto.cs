using System;
using System.Collections.Generic;
using System.Text;

namespace SCE.Web.DataUtil.Dto
{
    public class CloseToBookDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DefaultValue { get; set; }
        public int SavedValue { get; set; }
        public DateTime SavedTimeStamp { get; set; }
    }
}
