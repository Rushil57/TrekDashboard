using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SCE.Web.DataUtil.Dto
{
    public class SCEResult_TwoDto
    {
        [Column("SCE (Days)")]
        public int SCE_Days { get; set; }

        [Column("5 Stars")]
        public int Stars_5_First { get; set; }
        [Column("4 Stars")]
        public int Stars_4_First { get; set; }
        [Column("3 Stars")]
        public int Stars_3_First { get; set; }
        [Column("2 Stars")]
        public int Stars_2_First { get; set; }
        [Column("1 Stars")]
        public int Stars_1_First { get; set; }
        [Column("Total")]
        public int Total { get; set; }

        [Column("5 Stars5")]
        public int Stars_5_Second { get; set; }
        [Column("5 Stars4")]
        public int Stars_4_Second { get; set; }
        [Column("5 Stars3")]
        public int Stars_3_Second { get; set; }
        [Column("5 Stars2")]
        public int Stars_2_Second { get; set; }
        [Column("5 Stars1")]
        public int Stars_1_Second { get; set; }
        [Column("Total1")]
        public int Total_1 { get; set; }
    }
}