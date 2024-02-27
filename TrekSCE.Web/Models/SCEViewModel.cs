using SCE.Web.DataUtil.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCE.Web.Models
{
    public class SCEViewModel
    {
        public List<StarsViewModel> StarsViewModel { get; set; }
        public List<CloseToBookDto> CloseToBook { get; set; }
        public List<SalesCycleExtensionDto> SalesCycleExtension { get; set; }
        public List<SaleSlapseDaysDto> SaleSlapseDays { get; set; }
        public List<SCEResult_OneDto> ProposalCount_OneViewModel { get; set; }
        public List<SCEResult_TwoDto> ProposalCount_TwoViewModel { get; set; }
        public List<SCECompareTo> SCECompareToList { get; set; }
        public string Datasource { get; set; }
    }

    public class SCECompareTo
    {
        public SCECompareTo(int id, string name) {
            Id = id;
            Name = name;
        }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}