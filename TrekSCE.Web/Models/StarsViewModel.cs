using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCE.Web.Models
{
    public class StarsViewModel
    {

        //public StarsViewModel(string name, string praposalRank, string customerRank, string resellerRank, string totalRank, string totalStarRank)
        //{
        //    Name = name;
        //    PraposalRank = praposalRank;
        //    CustomerRank = customerRank;
        //    ResellerRank = resellerRank;
        //    TotalRank = totalRank;
        //    TotalStarRank = totalStarRank;
        //}
        //public string Name { get; set; }
        //public string PraposalRank { get; set; }
        //public string CustomerRank { get; set; }
        //public string ResellerRank { get; set; }
        //public string TotalRank { get; set; }
        //public string TotalStarRank { get; set; }

        //public static List<StarsViewModel> getData()
        //{

        //    var starsListByCat = new List<StarsViewModel>();
        //    starsListByCat.Add(new StarsViewModel("", "5", "4", "3", "2", "1"));
        //    starsListByCat.Add(new StarsViewModel("", "5", "4", "3", "2", "1"));
        //    starsListByCat.Add(new StarsViewModel("", "5", "4", "3", "2", "1"));
        //    starsListByCat.Add(new StarsViewModel("", "15", "12", "9", "6", "3"));
        //    starsListByCat.Add(new StarsViewModel("", "15", "12 - 14", "9 - 11", "6 - 8", "3 - 5"));
        //    return starsListByCat;
        //}

        public StarsViewModel(string name, string col1, string col2, string col3, string col4, string col5)
        {
            Name = name;
            Col1 = col1;
            Col2 = col2;
            Col3 = col3;
            Col4 = col4;
            Col5 = col5;
        }

        public string Name { get; set; }
        public string Col1 { get; set; }
        public string Col2 { get; set; }
        public string Col3 { get; set; }
        public string Col4 { get; set; }
        public string Col5 { get; set; }

        public static List<StarsViewModel> getData()
        {

            var starsListByCat = new List<StarsViewModel>();
            starsListByCat.Add(new StarsViewModel("Proposal Rank", "5", "4", "3", "2", "1"));
            starsListByCat.Add(new StarsViewModel("Customer Rank", "5", "4", "3", "2", "1"));
            starsListByCat.Add(new StarsViewModel("Reseller Rank", "5", "4", "3", "2", "1"));
            starsListByCat.Add(new StarsViewModel("Total Stars", "15", "12", "9", "6", "3"));
            starsListByCat.Add(new StarsViewModel("Total Stars Range", "15", "12 - 14", "9 - 11", "6 - 8", "3 - 5"));
            return starsListByCat;
        }
        public static List<StarsViewModel> getDataForProposalsCount()
        {

            var starsListByCat = new List<StarsViewModel>();
            starsListByCat.Add(new StarsViewModel("1", "5", "4", "3", "2", "1"));
            starsListByCat.Add(new StarsViewModel("2", "5", "4", "3", "2", "1"));
            starsListByCat.Add(new StarsViewModel("3", "5", "4", "3", "2", "1"));
            starsListByCat.Add(new StarsViewModel("4", "15", "12", "9", "6", "3"));
            starsListByCat.Add(new StarsViewModel("5", "5", "4", "3", "2", "1"));
            starsListByCat.Add(new StarsViewModel("25", "5", "4", "3", "2", "1"));
            starsListByCat.Add(new StarsViewModel("30", "5", "4", "3", "2", "1"));
            starsListByCat.Add(new StarsViewModel("80", "15", "12", "9", "6", "3"));
            return starsListByCat;
        }
    }
}