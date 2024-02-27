using Dapper;
using Newtonsoft.Json;
using SCE.Web.DataUtil.Dto;
using SCE.Web.DataUtil.Service;
using SCE.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SCE.Web.Controllers
{
    public class HomeController : Controller
    {

        public async Task<ActionResult> Index()
        {
            SCEViewModel SCEObj = await GetSCEDataFromDB();
            return View(SCEObj);
        }

        public async Task<SCEViewModel> GetSCEDataFromDB()
        {
            CloseToBook ctbObj = new CloseToBook();
            var ObjRes = await ctbObj.GetSCEModelData(0);
            var sceProposalsData = await ctbObj.GetSCEProposalsCount();


            SCEViewModel SCEObj = new SCEViewModel();
            SCEObj.SaleSlapseDays = new List<SaleSlapseDaysDto>();
            var saleSlapseDays = (List<SaleSlapseDaysDto>)ObjRes[0];


            
            foreach (var item in saleSlapseDays)
            {
                SaleSlapseDaysDto objSaleSlapseDays = new SaleSlapseDaysDto();
                objSaleSlapseDays.Id = item.Id;
                objSaleSlapseDays.DealSizeBandId = item.DealSizeBandId;
                objSaleSlapseDays.SalesCallsSavedValue = item.SalesCallsSavedValue;
                objSaleSlapseDays.CallGapDaysSavedValue = item.CallGapDaysSavedValue;
                objSaleSlapseDays.SalesLapseDays = objSaleSlapseDays.SalesCallsSavedValue + (objSaleSlapseDays.SalesCallsSavedValue - 1) * objSaleSlapseDays.CallGapDaysSavedValue;

                SCEObj.SaleSlapseDays.Add(objSaleSlapseDays);
            }

            var closeToBook = (List<CloseToBookDto>)ObjRes[1];
            closeToBook.Add(new CloseToBookDto
            {
                Id = 0,
                Name = "Total",
                DefaultValue = closeToBook.Sum(x => x.DefaultValue)
            });

            SCEObj.CloseToBook = closeToBook;
            SCEObj.SalesCycleExtension = (List<SalesCycleExtensionDto>)ObjRes[2];
            SCEObj.StarsViewModel = StarsViewModel.getData();
            var proposalCount_One = (List<SCEResult_OneDto>)sceProposalsData[0];
            SCEObj.ProposalCount_OneViewModel = proposalCount_One;

            var proposalCount_Two = (List<SCEResult_TwoDto>)sceProposalsData[1];
            SCEObj.ProposalCount_TwoViewModel = proposalCount_Two;

            return SCEObj;
        }

        public async Task SaveSalesLapseDays(List<SaleSlapseDaysDto> salesLapseDaysList)
        {
            try
            {
                CloseToBook ctbObj = new CloseToBook();
                foreach (var item in salesLapseDaysList)
                {
                    await ctbObj.InsertUpdateSCE(1, item.Id, 1, item.SalesCallsSavedValue, item.CallGapDaysSavedValue);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            
            
        }

        public async Task SaveCloseToBook(List<CloseToBookDto> closeToBookList)
        {   
            try
            {
                CloseToBook ctbObj = new CloseToBook();
                foreach (var item in closeToBookList)
                {
                    await ctbObj.InsertUpdateSCE(2, item.Id, 1, item.SavedValue, 0);
                }
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public async Task SaveSCEExt(List<SalesCycleExtensionDto> sCEExtList)
        {
            try
            {
                CloseToBook ctbObj = new CloseToBook();
                foreach (var item in sCEExtList)
                {
                    await ctbObj.InsertUpdateSCE(3, item.Id, 1, Convert.ToInt32(item.SavedValue), 0);
                }
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public async Task<ActionResult> ResetData(int sectionId)
        {
            try
            {
                CloseToBook ctbObj = new CloseToBook();
                await ctbObj.InsertUpdateSCE(sectionId, 0, 2, 0, 0);
                SCEViewModel SCEObj = await GetSCEDataFromDB();
                return Json(SCEObj);

            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public async Task<ActionResult> ApplySCE()
        {
            try
            {
                CloseToBook ctbObj = new CloseToBook();
                var status = await ctbObj.ApplySCE();
                return Json(status);

            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult SCEViewDetails()
        {
            SCEViewModel model = new SCEViewModel();
            List<SCECompareTo> obj = new List<SCECompareTo>();
            obj.Add(new SCECompareTo(1, "Value1"));
            obj.Add(new SCECompareTo(2, "Value2"));
            obj.Add(new SCECompareTo(3, "Value3"));
            obj.Add(new SCECompareTo(4, "Value4"));
            obj.Add(new SCECompareTo(5, "Value5"));
            obj.Add(new SCECompareTo(6, "Value6"));
            obj.Add(new SCECompareTo(7, "Value7"));
            model.SCECompareToList = obj;

            string dataSource = JsonConvert.SerializeObject(model.SCECompareToList);
            model.Datasource = dataSource;

            return View(model);
        }
    }
}