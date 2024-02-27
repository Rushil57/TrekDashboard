var Default_Page_Size = 10;
var Current_Page_TopAccount = 1;

var LoadingFirstTime;
var _genericElements = {
    //fetch: SalesAnalytics.data,

    UserInfo: $("#UserInfoHeader").val(),
    //PreviousName: params.STName === null ? '' : params.STName,
    AccountID: '',
    WhereConditions: '',
    TargetToExclude: '',
    FirstLoad: true,
    flag: false,
    listInstance: null,

    //BUArray: ko.observable([])
};
// NOTE object below must be a valid JSON
window.SalesAnalytics = $.extend(true, window.SalesAnalytics, {
    "config": {
        "layoutSet": "simple",
        "navigation": [
          {
              "title": "SALES ANALYTICS OVERVIEW",
              "onExecute": "#Accounts"
          },
          {
              "title": "SalesTeam",
              "onExecute": "#SalesTeam",
              "icon": "salesteam"
          },
          {
              "title": "SingleAccount",
              "onExecute": "#SingleAccount",
              "icon": "singleaccount"
          }
        ],
        "endpoints": {
            "db": {
                "local": "http://localhost:5158/AnalyticsAPI/api/",
                "production": "http://localhost:5158/AnalyticsAPI/api/"
            }
        }
    },
    "tooltip": {
        noProposals: 'Proposals'
    }
});

$(document).ready(function () {

    _genericElements.UserInfo = $("#UserInfoHeader").val();
    $(".spnDefaultPageSize").html(Default_Page_Size);
    setPageDetails();
    LoadingFirstTime = $("#UserInfoHeader").val();
    SetSiteCount();
    if (parseInt($("#Top_Account_TotalCount").val()) > Default_Page_Size) {
        $("#btnTopAccNext").show();
    }
});
function SetSiteCount() {
    var modelSiteRawData = $("#hdnModelSiteData").html();
    var modelSiteJsonData = JSON.parse(modelSiteRawData);
    $("#cntNoSite").html(modelSiteJsonData[0].NoSite);
    $("#cntNoAccounts").html(modelSiteJsonData[0].NoAccounts);
    $("#cntTotalSalesReps").html(modelSiteJsonData[0].TotalSalesReps);
    
    $("#cntNoProposals").html(modelSiteJsonData[0].NoProposals);
    $("#cntTotalLegacyQty").html(modelSiteJsonData[0].TotalLegacyQty);
    $("#cntTotalSalesOpps").html(Globalize('en-US').currencyFormatter('USD', {
        maximumFractionDigits: 0
    })(modelSiteJsonData[0].TotalSalesOpps));
    $("#cntHRSalesOpps").html(Globalize('en-US').currencyFormatter('USD', {
        maximumFractionDigits: 0
    })(parseFloat(modelSiteJsonData[0].HighRankingSalesOpps)));
    $("#cntHRProposals").html(modelSiteJsonData[0].HighRankingPropsals);
    $("#cntSiteCities").html(modelSiteJsonData[0].SiteCities);
}
function topAccountsArgClicked(e) {
    //
    debugger;
    var pointValueArray = [];
    var SalesRepUserID, SalesRepLevel, CallerID;
    CallerID = 0;
    var instance = e.component;
    instance.clearSelection();
    var series = e.component.getAllSeries();
    var accountUrl = '';
    var urlSalesTeam = $("#hdnSalesTeamDashboard").val();
    var urlAccount = $("#hdnAccountsDashboard").val();
    for (var i = 0; i < series.length; i++) {
        SalesRepUserID = series[i].getPointsByArg(e.argument)[0].tag.SalesRepID;
        SalesRepLevel = series[i].getPointsByArg(e.argument)[0].tag.SalesRepLevel;
        CallerID = series[i].getPointsByArg(e.argument)[0].tag.CallerID;
        series[i].getPointsByArg(e.argument)[0].select();
        if (series[i].getPointsByArg(e.argument)[0].originalValue > 0 && series[i].getPointsByArg(e.argument)[0].series.name.indexOf("Total") < 0)
            pointValueArray.push("'" + series[i].getPointsByArg(e.argument)[0].series.name + "'");
    }
    var pointValuestring = "AND ProposalRank in (" + pointValueArray.join(',') + " )";
    var colNameValue = "AND SalesRepFullName  = '" + e.argument + "' ";
    _genericElements.TargetToExclude = "salesTeamOppChartSettings";
    _genericElements.WhereConditions = colNameValue + pointValuestring;
    var encUrl = $("#hdnEncryptionTestUrl").val();
    targetDrill = '';
    if (SalesRepLevel <= 1) {
        var user = JSON.stringify({ UserID: SalesRepUserID, UserLevel: SalesRepLevel, CallerID: CallerID, FirstUser: LoadingFirstTime });
        var xhrSettings = JSON.stringify({ headers: [{ name: "UserInfo", value: user }] });
        //
        var modelData = {}
        modelData.XhrSettings = user;
        $.ajax({
            url: encUrl,
            data: modelData,
            type: 'POST',
            //dataType: 'json',
            success: function (dt) {
                var parentArg = e.argument;
                //targetDrill = 'Accounts/' + dt + "/" + $("#CurrentUser").val() + " < " + e.argument;
                targetDrill = dt;
                var Ds = [
                 {
                     text: 'Drill Down',
                     value: "drilldown",
                     targetDrill: null
                 }, {
                     text: 'Go to ' + e.argument + ' Dashboard',
                     value: "nav",
                     targetDrill: targetDrill
                 }
                ];
                
                $("#context-menu").dxContextMenu({
                    dataSource: Ds,
                    width: 200,
                    position:
                        {
                            of: e.jQueryEvent
                        },
                    onItemClick: function (e) {
                        if (e.itemData.value === "drilldown") {
                            _genericElements.FirstLoad = false;
                            DataLoadForAllaccounts();
                        }
                        else {
                            $('#dataLoadPanel').dxLoadPanel('instance').show();
                            window.location.href = urlAccount + "?userInfo=" + e.itemData.targetDrill + "&name=" + $("#CurrentUser").val() + " < " + parentArg;
                        }
                    }
                });
                $('#context-menu').dxContextMenu('instance').show();
            },
            error: function (err, er1, er2) {

            }
        });
    }
    else {
        var user1 = JSON.stringify({ UserID: SalesRepUserID, UserLevel: SalesRepLevel, FirstUser: LoadingFirstTime });
        var xhrSettings = JSON.stringify({ headers: [{ name: "UserInfo", value: user1 }] });
        //
        var modelData = {}
        modelData.XhrSettings = user1;
        $.ajax({
            url: encUrl,
            data: modelData,
            type: 'POST',
            //dataType: 'json',
            success: function (dt) {
                //targetDrill = 'SalesTeam/' + dt + "/" + $("#CurrentUser").val();
                targetDrill = dt ;
                var Ds = [
                      {
                          text: 'Drill Down ', // to ' + e.argument,
                          value: "drilldown",
                          targetDrill: null
                      }, {
                          text: 'Go to ' + e.argument + ' Dashboard',
                          value: "nav",
                          targetDrill: targetDrill
                      }
                ];

                $("#context-menu").dxContextMenu({
                    dataSource: Ds,
                    width: 200,
                    position:
                        {
                            of: e.jQueryEvent
                        },
                    onItemClick: function (e) {
                        if (e.itemData.value === "drilldown") {
                            _genericElements.FirstLoad = false;
                            DataLoadForAllaccounts();
                        }
                        else {
                            $('#dataLoadPanel').dxLoadPanel('instance').show();
                            window.location.href = urlSalesTeam + "?userInfo=" + e.itemData.targetDrill + "&saleRepName=" + $("#STsaleRepName").val();
                            //SalesAnalytics.app.navigate(e.itemData.targetDrill, { root: false });
                        }
                    }
                });
                $('#context-menu').dxContextMenu('instance').show();
            },
            error: function (err, er1, er2) {

            }
        });
        
    }
}

function expiringContractsChartToolTipHtml(arg) {
    
    if (arg.seriesName.indexOf('Legacy') >= 0) {
        var value = Globalize('en_US').numberFormatter()(arg.value);
        return {
            html:
             "<div class='state-tooltip' style='display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Account Industry</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Legacy Quantity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + value + "</label></div></td> \
                                    </tr> \
                                    </table>\
                               </div>"
        }
    }
    else {
        // var salevalue = Globalize('en_US').currencyFormatter('USD')(arg.point.tag.SaleOpps);
        return {
            html:

               "<div class='state-tooltip' style='display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Account Industry</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>BU</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.seriesName + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Accounts</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en_US').numberFormatter()(arg.value) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Sales Reps</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en_US').numberFormatter()(arg.point.tag.SalesRepCount) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en_US').currencyFormatter('USD', { maximumFractionDigits: 0, })(arg.point.tag.SalesOpp) + "</label></div></td> \
                                    </tr> \
                                    </table>\
                               </div>"
        }
    }

}

function proposalsToolTipHtml(arg) {

    return {
        html:
            //"<div class='state-tooltip' style='width:250px;'>"

            //+ "<div style='width:100%;float:left'><label style='float:left'>Proposal SFDC Status </label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div>"
            // + "<div style='width:100%;float:left'><label style='float:left'>" + window.SalesAnalytics.tooltip.noProposals + "</label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.value) + "</label></div>"
            // + "<div style='width:100%;float:left'><label style='float:left'>No. Accounts</label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.point.tag) + "</label></div>"
            //+ "</div>"
             "<div class='state-tooltip' style='display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Proposal SFDC Status </label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Accounts</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.point.tag.NoAccounts) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').currencyFormatter('USD', { maximumFractionDigits: 0, })(arg.point.tag.TotalSalesOpp) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>" + window.SalesAnalytics.tooltip.noProposals + "</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.value) + "</label></div></td> \
                                    </tr> \
                                    </table>\
                               </div>"
    }
}
function proposalsChartToolTipHtml(arg) {

    return {
        html:
             "<div class='state-tooltip' style='height: 100%;display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Proposal Rank </label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Accounts</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter({ maximumFractionDigits: 0 })(arg.point.tag.Accounts) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').currencyFormatter('USD', { maximumFractionDigits: 0 })(arg.point.tag.TotalSalesOpp) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>" + window.SalesAnalytics.tooltip.noProposals + "</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.value) + "</label></div></td> \
                                    </tr> \
                                    </table>\
                               </div>"
    }
}
function salesRespTopAccountsToolTipHtml(arg)
{

    return {
        html:
            //"<div class='state-tooltip' style='width:250px;'>"

            //+ "<div style='width:100%;float:left'><label style='float:left'>Account Name </label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div>"
            //+ "<div style='width:100%;float:left'><label style='float:left'>Proposal Ranking </label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + arg.seriesName + "</label></div>"
            //+ "<div style='width:100%;float:left'><label style='float:left'>Total Sales Opp </label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + Globalize('en_US').currencyFormatter('USD', { maximumFractionDigits: 0, })(arg.value) + "</label></div>"
            //      + "<div style='width:100%;float:left'><label style='float:left'>" + window.SalesAnalytics.tooltip.noProposals + "</label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.point.tag.NoProposals) + "</label></div>"
            //+ "<div style='width:100%;float:left'><label style='float:left'>No. LegacyProd </label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.point.tag.NoLegacyProd) + "</label></div>"
            //+ "</div>"
             "<div class='state-tooltip' style='height: 100%;display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Account Name </label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Proposal Rank</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.seriesName + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en_US').currencyFormatter('USD', { maximumFractionDigits: 0, })(arg.value) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>" + window.SalesAnalytics.tooltip.noProposals + "</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.point.tag.NoProposals) + "</label></div></td> \
                                    </tr> \
                                     <tr> \
                                    <td><div class='labelHolder'><label>Legacy Quantity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.point.tag.NoLegacyProd) + "</label></div></td> \
                                    </tr> \
                                    </table>\
                               </div>"
    }
}
function topproductsToolTipHtml(arg) {
    
    return {
        html:
            //"<div class='state-tooltip' style='width:250px;'>"

            //+ "<div style='width:100%;float:left'><label style='float:left'>Legacy system </label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div>"
            //+ "<div style='width:100%;float:left'><label style='float:left'>Age (In Months) </label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + arg.seriesName + "</label></div>"
            //+ "<div style='width:100%;float:left'><label style='float:left'>Total Sale Opps </label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + Globalize('en_US').currencyFormatter('USD', { maximumFractionDigits: 0, })(arg.value) + "</label></div></div>"
            //+ "<div style='width:100%;float:left'><label style='float:left'>No. Accounts </label>"
            //+ "<label style='float:right;color:" + arg.point.getColor() + "'>" + Globalize('en_US').numberFormatter()(arg.point.tag) + "</label></div></div>"
             "<div class='state-tooltip' style='height: 100%;display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Age (In Months)</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.seriesName + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Legacy Product</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Legacy Quantity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en_US').numberFormatter()(arg.point.tag.LegacyQty) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Accounts</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en_US').numberFormatter()(arg.point.tag.Qty) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en_US').currencyFormatter('USD', { maximumFractionDigits: 0, })(arg.point.tag.SalesOpp) + "</label></div></td> \
                                    </tr> \
                                    </table>\
                               </div>"
    }

}

function accountsToolTipHtml(arg) {
    return {
        html:
         "<div class='state-tooltip' style='height: 100%;display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Proposal SFDC Status</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argument + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Accounts</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.point.tag.NoAccounts) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').currencyFormatter('USD', { maximumFractionDigits: 0 })(arg.point.tag.TotalSalesOpp) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>" + window.SalesAnalytics.tooltip.noProposals + "</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.point.tag.NoProposals) + "</label></div></td> \
                                    </tr> \
                                    </table>\
                               </div>"
    }
}

function topaccountsToolTipHtml(arg) {
    if (arg.seriesName.indexOf('Sales ') >= 0) {
        var salevalue = Globalize('en_US').currencyFormatter('USD', { maximumFractionDigits: 0 })(arg.value);
        return {

            html: "<div class='state-tooltip' style='height: 100%;display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Sales Rep Name </label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + salevalue + "</label></div></td> \
                                    </tr> \
                                    </table>\
                                    </div>"

        }
    }
    else {
        var salevalue1 = Globalize('en_US').currencyFormatter('USD', { maximumFractionDigits: 0 })(arg.point.tag.SaleOpps);
        return {
            html:

               "<div class='state-tooltip' style='height: 100%;display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Sales Rep Name </label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Accounts</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.point.tag.NoAccounts + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Proposal Rank</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.seriesName + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + salevalue1 + "</label></div></td> \
                                    </tr> \
                                     <tr> \
                                    <td><div class='labelHolder'><label>" + window.SalesAnalytics.tooltip.noProposals + "</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.point.tag.NoProposals + "</label></div></td> \
                                    </tr> \
                                    </table>\
                               </div>"
        }
    }
}

function topAccountsOnPointClick(arg) {
    var instance = arg.component;
    instance.clearSelection();
    arg.target.select();

    if (arg.target !== null) {
        if (arg.target.series.name.indexOf("Total") >= 0) return;
        var _account = arg.target.argument;
        var _BU = arg.target.series.name;
        _genericElements.TargetToExclude = "salesTeamOppChartSettings";
        _genericElements.WhereConditions = " And SalesRepFullName = '" + _account + "' And ProposalRank = '" + _BU + "'";
        _genericElements.FirstLoad = false;
        DataLoadForAllaccounts();
    }
}
function expiringContractsChartOnPointClick(arg) {
    var instance = arg.component;
    instance.clearSelection();
    arg.target.select();

    if (arg.target !== null) {
        if (arg.target.series.name.indexOf("Legacy") >= 0) return;
        var _account = arg.target.argument;
        var _BU = arg.target.series.name;
        _genericElements.TargetToExclude = "totalAccountsChartSettings";
        _genericElements.WhereConditions = " And AccountIndustry = '" + _account + "' And LegacyBU = '" + _BU + "'";
        _genericElements.FirstLoad = false;
        DataLoadForAllaccounts();
    }
}
function expiringContractsChartOnArgumentAxisClick(e) {
    var colNameValue = "AND AccountIndustry  = '" + e.argument + "' ";
    var pointValueArray = [];
    var instance = e.component;
    instance.clearSelection();
    var series = e.component.getAllSeries();
    
    for (var i = 0; i < series.length; i++) {
        if (series[i].getPointsByArg(e.argument)[0] != undefined && series[i].getPointsByArg(e.argument)[0].series.name.indexOf("Legacy ") < 0) {
            for (var j = 0 ; j < series[i].getPointsByArg(e.argument).length ; j++) {
                series[i].getPointsByArg(e.argument)[j].select();
                if (series[i].getPointsByArg(e.argument)[j].originalValue > 0)
                    pointValueArray.push("'" + series[i].getPointsByArg(e.argument)[j].series.name + "'");
            }
        }
    }
    var pointValuestring = "AND LegacyBU in (" + pointValueArray.join(',') + " )";
    _genericElements.TargetToExclude = "totalAccountsChartSettings";
    _genericElements.WhereConditions = colNameValue + pointValuestring;
    _genericElements.FirstLoad = false;
    DataLoadForAllaccounts();
}

function proposalsOnPointClick(arg) {
    arg.target.select();

    if (arg.target !== null) {
        var _proposaRank = arg.target.argument;
        _genericElements.TargetToExclude = "pipelinePieSettings";
        _genericElements.WhereConditions = " And ProposalSFDCStatus = '" + _proposaRank + "'";
        _genericElements.FirstLoad = false;
        DataLoadForAllaccounts();
    }
}
function proposalsChartOnPointClick(arg) {
    arg.target.select();

    if (arg.target !== null) {
        var _proposaRank = arg.target.argument;
        _genericElements.TargetToExclude = "proposalsPieSettings";
        _genericElements.WhereConditions = " And ProposalRank = '" + _proposaRank + "'";
        DataLoadForAllaccounts();
    }
}

function salesRespTopAccountsOnPointClick(arg) {
    var instance = arg.component;
    instance.clearSelection();
    arg.target.select();



    if (arg.target !== null) {
        var _account = arg.target.argument;
        var _BU = arg.target.series.name;
        _genericElements.TargetToExclude = "salesRespTopAccountsSettings";
        _genericElements.WhereConditions = " And AccountName = '" + _account + "' And ProposalRank = '" + _BU + "'";
        _genericElements.FirstLoad = false;
        DataLoadForAllaccounts();
    }
}

function topproductsOnPointClick(arg) {
    var instance = arg.component;
    instance.clearSelection();

    arg.target.select();
    if (arg.target !== null) {

        var _legacyAge;
        if (arg.target.series.name.indexOf('120+') >= 0)
            _legacyAge = " and LegacyAge >= 109";
        else {
            var arrRange = [];
            arrRange.push(parseInt(arg.target.series.name) + 11);
            arrRange.push(parseInt(arg.target.series.name));

            _legacyAge = " and LegacyAge <= " + arrRange[0] + " And LegacyAge >= " + arrRange[1];
        }
        var _legacyName = arg.target.argument;
        _genericElements.TargetToExclude = "salesRespTopLegProdSettings";
        _genericElements.WhereConditions = " And LegacyPlatformName = '" + _legacyName + "'" + _legacyAge;
        _genericElements.FirstLoad = false;
        DataLoadForAllaccounts();
    }

}
function salesRespTopAccountsOnArgumentAxisClick(e) {
    var colNameValue = "AND AccountName  = '" + e.argument + "' ";
    var pointValueArray = [];
    var instance = e.component;
    instance.clearSelection();
    var series = e.component.getAllSeries();
    //var accountUrl = '';
    for (var i = 0; i < series.length; i++) {
        for (var j = 0 ; j < series[i].getPointsByArg(e.argument).length ; j++) {
            series[i].getPointsByArg(e.argument)[j].select();
            if (series[i].getPointsByArg(e.argument)[j].originalValue > 0)
                pointValueArray.push("'" + series[i].getPointsByArg(e.argument)[j].series.name + "'");
        }

    }
    var pointValuestring = "AND ProposalRank in (" + pointValueArray.join(',') + " )";
    _genericElements.TargetToExclude = "salesRespTopAccountsSettings";
    _genericElements.WhereConditions = colNameValue + pointValuestring;
    _genericElements.FirstLoad = false;
    DataLoadForAllaccounts();
}
function topproductsOnArgumentAxisClick(e) {
    var colNameValue = "AND LegacyPlatformName  = '" + e.argument + "' ";
    var pointValueArray = [];
    var instance = e.component;
    instance.clearSelection();
    var series = e.component.getAllSeries();

    for (var i = 0; i < series.length; i++) {
        series[i].getPointsByArg(e.argument)[0].select();
        if (series[i].getPointsByArg(e.argument)[0].originalValue > 0)
            pointValueArray.push(series[i].getPointsByArg(e.argument)[0].tag.AgeKey);
    }
    pointValueArray = sortInts(pointValueArray);
    var whereCol = '';
    if (pointValueArray.indexOf('120+') >= 0) {
        var arr = pointValueArray.slice(0, -1);
        whereCol = "AND LegacyAge >= " + arr[0];// + arr[arr.length - 1];
    }
    else {
        if (pointValueArray.length > 1) {
            var arrRange = [];
            var lastVal = parseInt(pointValueArray[pointValueArray.length - 1]) - 11;
            arrRange.push(pointValueArray[0] - 11);
            arrRange.push(pointValueArray[pointValueArray.length - 1]);

            whereCol = "AND LegacyAge  >= " + arrRange[0] + " and LegacyAge <= " + arrRange[1];

        }
        else {
            var _legacyAge;

            var arrRange1 = [];
            arrRange1.push(parseInt(pointValueArray[0]) - 11);
            arrRange1.push(parseInt(pointValueArray[1]));

            _legacyAge = " and LegacyAge >= " + arrRange1[0] + " And LegacyAge <= " + arrRange1[1];
        }
    }
    var pointValuestring = whereCol;//pointValueArray.indexOf("120+ Months") >= 0 ? "AND LegacyAge in " + pointValueArray[0] : "AND LegacyAge between " + pointValueArray[0] + " and " + pointValueArray[pointValueArray.length - 1];
    _genericElements.TargetToExclude = "salesRespTopLegProdSettings";
    _genericElements.WhereConditions = colNameValue + pointValuestring;
    _genericElements.FirstLoad = false;
    DataLoadForAllaccounts();

}
function DataLoadForAllaccounts() {
    if (LoadingFirstTime === undefined) {
        LoadingFirstTime = "0";
    }

    if (LoadingFirstTime === "0") {
        LoadingFirstTime = _genericElements.UserInfo;
    }
    $('#dataLoadPanel').dxLoadPanel('instance').show();
    var dataLoadUrl = $("#hdnLoadSalesTeamAllDataUrl").val();
    var postHeaders = {};
    postHeaders.UserInfo = _genericElements.UserInfo;
    postHeaders.AccountID = _genericElements.AccountID;
    postHeaders.WhereConditions = _genericElements.WhereConditions;
    postHeaders.SaleRepName = "";
    postHeaders.CallerID = "";
    postHeaders.LoadedPreviously = LoadingFirstTime;
    
    $.ajax({
        //url: window.SalesAnalytics.config.endpoints.db.local + "SalesTeam/GetSalesTeamData",
        url: dataLoadUrl,
        data: { headers: postHeaders, targetToExclude: _genericElements.TargetToExclude },
        type: 'POST',
        //dataType: 'json',
        headers: { "UserInfo": _genericElements.UserInfo, "AccountID": _genericElements.AccountID, "WhereConditions": _genericElements.WhereConditions, "SaleRepName": "", "CallerID": "", "LoadedPreviously": LoadingFirstTime },
        success: function (resData) {
            
            var dt = JSON.parse(resData);
            if (dt != undefined) {
                if (dt.DateRefreshed != undefined) {
                    $("#lblDateRefreshed").html(dt.DateRefreshed);
                }
                if (dt.Version != undefined) {
                    $("#lblVersion").html(dt.Version);
                }
                if (dt.Site_Card.length === 0 &&
                        dt.SalesTeamOpp_StackedBar.length === 0 &&
                        dt.TotalAccounts_MixedChart.length === 0 &&
                        dt.Pipeline_Pie.length === 0 &&
                        dt.TopAccounts_StackedBar.length === 0 &&
                        dt.TopAccounts_StackedBar.length === 0) {
                    $('#dataLoadPanel').dxLoadPanel('instance').hide();
                    $("#nodataPopup").dxPopup("show");
                    //disableFilters(true); //##
                    Current_Page_TopAccount = 1;
                    ShowHideBtnNextPrev();

                }
                else {
                    //alert(_genericElements.TargetToExclude);
                    $("#cntNoSite").html(dt.Site_Card[0].NoSite);
                    $("#cntNoAccounts").html(dt.Site_Card[0].NoAccounts);
                    $("#cntTotalSalesReps").html(dt.Site_Card[0].TotalSalesReps);
                    $("#cntNoProposals").html(dt.Site_Card[0].NoProposals);
                    $("#cntTotalLegacyQty").html(dt.Site_Card[0].TotalLegacyQty);
                    $("#cntTotalSalesOpps").html(Globalize('en-US').currencyFormatter('USD', {
                        maximumFractionDigits: 0
                    })(dt.Site_Card[0].TotalSalesOpps));
                    $("#cntHRSalesOpps").html(Globalize('en-US').currencyFormatter('USD', {
                        maximumFractionDigits: 0
                    })(parseFloat(dt.Site_Card[0].HighRankingSalesOpps)));
                    $("#cntHRProposals").html(dt.Site_Card[0].TotalHighRankingSalesOpps);
                    $("#cntSiteCities").html(dt.Site_Card[0].SiteCities);

                    if (_genericElements.FirstLoad === true) {

                        Current_Page_TopAccount = 1;
                        $("#Top_Account_TotalCount").val(dt.Top_Account_TotalCount);
                        ShowHideBtnNextPrev();

                        $('#topaccounts').dxChart('instance').option('dataSource', dt.SalesTeamOpp_StackedBar);
                        $('#topaccounts').dxChart('instance')._render();

                        $('#totalAccounts').dxChart('instance').option('dataSource', dt.TotalAccounts_MixedChart);
                        $('#totalAccounts').dxChart('instance')._render();

                        $('#proposalspipeline').dxPieChart('instance').option('dataSource', dt.Pipeline_Pie);
                        $('#proposalspipeline').dxPieChart('instance')._render();

                        $('#salesRespTopAccounts').dxChart('instance').option('dataSource', dt.TopAccounts_StackedBar);
                        $('#salesRespTopAccounts').dxChart('instance')._render();

                        $('#salesRespTopLegProd').dxChart('instance').option('dataSource', dt.TopLegacyLegacyPlateform_StackedBar);
                        $('#salesRespTopLegProd').dxChart('instance')._render();

                        //_genericElements.BUArray(_genericElements.BUArray().length === 0 ? dt[0].BUList : _genericElements.BUArray());
                        //if (viewModel.BUList.list.selectedItems().length === 0) {
                        //    viewModel.BUList.value("BU (" + viewModel.BUList.list.dataSource().length + " of " + viewModel.BUList.list.dataSource().length + " selected)");
                        //    viewModel.BUList.list.selectedItems(viewModel.BUList.list.dataSource());
                        //}
                        _genericElements.FirstLoad = false;

                    }
                    else {
                        switch (_genericElements.TargetToExclude) {
                            case "proposalsPieSettings":
                                $('#topaccounts').dxChart('instance').option('dataSource', dt.SalesTeamOpp_StackedBar);
                                $('#topaccounts').dxChart('instance')._render();
                                
                                $('#totalAccounts').dxChart('instance').option('dataSource', dt.TotalAccounts_MixedChart);
                                $('#totalAccounts').dxChart('instance')._render();

                                $('#proposalspipeline').dxPieChart('instance').option('dataSource', dt.Pipeline_Pie);
                                $('#proposalspipeline').dxPieChart('instance')._render();

                                $('#salesRespTopAccounts').dxChart('instance').option('dataSource', dt.TopAccounts_StackedBar);
                                $('#salesRespTopAccounts').dxChart('instance')._render();

                                $('#salesRespTopLegProd').dxChart('instance').option('dataSource', dt.TopLegacyLegacyPlateform_StackedBar);
                                $('#salesRespTopLegProd').dxChart('instance')._render();

                                Current_Page_TopAccount = 1;
                                $("#Top_Account_TotalCount").val(dt.Top_Account_TotalCount);
                                ShowHideBtnNextPrev();

                                break;
                            case "salesRespSettings":
                                $('#topaccounts').dxChart('instance').option('dataSource', dt.SalesTeamOpp_StackedBar);
                                $('#topaccounts').dxChart('instance')._render();
                                
                                $('#totalAccounts').dxChart('instance').option('dataSource', dt.TotalAccounts_MixedChart);
                                $('#totalAccounts').dxChart('instance')._render();

                                $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                                $('#proposals').dxPieChart('instance')._render();

                                $('#proposalspipeline').dxPieChart('instance').option('dataSource', dt.Pipeline_Pie);
                                $('#proposalspipeline').dxPieChart('instance')._render();

                                $('#salesRespTopAccounts').dxChart('instance').option('dataSource', dt.TopAccounts_StackedBar);
                                $('#salesRespTopAccounts').dxChart('instance')._render();

                                $('#salesRespTopLegProd').dxChart('instance').option('dataSource', dt.TopLegacyLegacyPlateform_StackedBar);
                                $('#salesRespTopLegProd').dxChart('instance')._render();

                                Current_Page_TopAccount = 1;
                                $("#Top_Account_TotalCount").val(dt.Top_Account_TotalCount);
                                ShowHideBtnNextPrev();

                                break;
                            case "salesTeamOppChartSettings":
                                
                                $('#totalAccounts').dxChart('instance').option('dataSource', dt.TotalAccounts_MixedChart);
                                $('#totalAccounts').dxChart('instance')._render();

                                $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                                $('#proposals').dxPieChart('instance')._render();

                                $('#proposalspipeline').dxPieChart('instance').option('dataSource', dt.Pipeline_Pie);
                                $('#proposalspipeline').dxPieChart('instance')._render();

                                $('#salesRespTopAccounts').dxChart('instance').option('dataSource', dt.TopAccounts_StackedBar);
                                $('#salesRespTopAccounts').dxChart('instance')._render();

                                $('#salesRespTopLegProd').dxChart('instance').option('dataSource', dt.TopLegacyLegacyPlateform_StackedBar);
                                $('#salesRespTopLegProd').dxChart('instance')._render();
                                
                                Current_Page_TopAccount = 1;
                                $("#Top_Account_TotalCount").val(dt.Top_Account_TotalCount);
                                ShowHideBtnNextPrev();

                                break;
                            case "totalAccountsChartSettings":

                                $('#topaccounts').dxChart('instance').option('dataSource', dt.SalesTeamOpp_StackedBar);
                                $('#topaccounts').dxChart('instance')._render();

                                $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                                $('#proposals').dxPieChart('instance')._render();

                                $('#proposalspipeline').dxPieChart('instance').option('dataSource', dt.Pipeline_Pie);
                                $('#proposalspipeline').dxPieChart('instance')._render();

                                $('#salesRespTopAccounts').dxChart('instance').option('dataSource', dt.TopAccounts_StackedBar);
                                $('#salesRespTopAccounts').dxChart('instance')._render();

                                $('#salesRespTopLegProd').dxChart('instance').option('dataSource', dt.TopLegacyLegacyPlateform_StackedBar);
                                $('#salesRespTopLegProd').dxChart('instance')._render();
                                
                                Current_Page_TopAccount = 1;
                                $("#Top_Account_TotalCount").val(dt.Top_Account_TotalCount);
                                ShowHideBtnNextPrev();

                                break;
                            case "pipelinePieSettings":
                                
                                
                                $('#topaccounts').dxChart('instance').option('dataSource', dt.SalesTeamOpp_StackedBar);
                                $('#topaccounts').dxChart('instance')._render();

                                $('#totalAccounts').dxChart('instance').option('dataSource', dt.TotalAccounts_MixedChart);
                                $('#totalAccounts').dxChart('instance')._render();

                                $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                                $('#proposals').dxPieChart('instance')._render();

                                $('#salesRespTopAccounts').dxChart('instance').option('dataSource', dt.TopAccounts_StackedBar);
                                $('#salesRespTopAccounts').dxChart('instance')._render();

                                $('#salesRespTopLegProd').dxChart('instance').option('dataSource', dt.TopLegacyLegacyPlateform_StackedBar);
                                $('#salesRespTopLegProd').dxChart('instance')._render();
                                
                                Current_Page_TopAccount = 1;
                                $("#Top_Account_TotalCount").val(dt.Top_Account_TotalCount);
                                ShowHideBtnNextPrev();

                                break;
                            case "salesRespTopAccountsSettings":
                                $('#topaccounts').dxChart('instance').option('dataSource', dt.SalesTeamOpp_StackedBar);
                                $('#topaccounts').dxChart('instance')._render();

                                $('#totalAccounts').dxChart('instance').option('dataSource', dt.TotalAccounts_MixedChart);
                                $('#totalAccounts').dxChart('instance')._render();

                                $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                                $('#proposals').dxPieChart('instance')._render();

                                $('#proposalspipeline').dxPieChart('instance').option('dataSource', dt.Pipeline_Pie);
                                $('#proposalspipeline').dxPieChart('instance')._render();

                                $('#salesRespTopLegProd').dxChart('instance').option('dataSource', dt.TopLegacyLegacyPlateform_StackedBar);
                                $('#salesRespTopLegProd').dxChart('instance')._render();

                                break;
                            case "salesRespTopLegProdSettings":
                                $('#topaccounts').dxChart('instance').option('dataSource', dt.SalesTeamOpp_StackedBar);
                                $('#topaccounts').dxChart('instance')._render();

                                $('#totalAccounts').dxChart('instance').option('dataSource', dt.TotalAccounts_MixedChart);
                                $('#totalAccounts').dxChart('instance')._render();

                                $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                                $('#proposals').dxPieChart('instance')._render();

                                $('#proposalspipeline').dxPieChart('instance').option('dataSource', dt.Pipeline_Pie);
                                $('#proposalspipeline').dxPieChart('instance')._render();

                                $('#salesRespTopAccounts').dxChart('instance').option('dataSource', dt.TopAccounts_StackedBar);
                                $('#salesRespTopAccounts').dxChart('instance')._render();
                                
                                Current_Page_TopAccount = 1;
                                $("#Top_Account_TotalCount").val(dt.Top_Account_TotalCount);
                                ShowHideBtnNextPrev();

                                break;

                        }
                    }
                }
                $('#dataLoadPanel').dxLoadPanel('instance').hide();
            }

        },
        error: function (err, er1, er2) {
            $('#dataLoadPanel').dxLoadPanel('instance').hide();
            $("#nodataPopup").dxPopup("show");

        }
    });
}
function seriesTemplateData(serie) {
    var retObj = {};
    if (serie === "LegacyQty") {
        retObj.type = "Spline";
        retObj.axis = "total";
        retObj.valueField = "TotalLegacyQty";
        retObj.name = "Legacy Qty";
        retObj.argumentField = "AccountIndustry";
        
    }
    else {
        
        retObj.Type = "stackedbar";
        //retObj.axis = "total";
        retObj.ValueField = "NoAccounts";
        //retObj.name = serie;
        retObj.ArgumentField = "AccountIndustry";
    }
    return serie === "LegacyQty" ? { type: 'line', axis: 'total', label: { visible: true }, valueField: 'TotalLegacyQty', name: "Legacy Qty" } : {}
    //return retObj;
}

var sortInts = function (data) {
    var dataSorted = data.sort(function (a, b) { return a - b; });
    return dataSorted;
}
function btnFiltersClick(e) {
    
    _genericElements.WhereConditions = "";
    _genericElements.FirstLoad = true;
    _genericElements.TargetToExclude = "";
    DataLoadForAllaccounts();
   
}
function btnPrevNextTopAccount(mode) {
    //var mode = e.component._options.accessKey;
    if (mode == "Prev") {
        Current_Page_TopAccount = Current_Page_TopAccount - 1;
    }
    else {
        Current_Page_TopAccount = Current_Page_TopAccount + 1;
    }
    ShowHideBtnNextPrev();

    var topAccountUrl = $("#hdnTopAccountUrl").val();
    $('#dataLoadPanel').dxLoadPanel('instance').show();
    $.ajax({
        url: topAccountUrl,
        data: { currentPage: Current_Page_TopAccount },
        type: 'POST',
        //dataType: 'json',
        success: function (dt) {
            $('#dataLoadPanel').dxLoadPanel('instance').hide();
            $('#salesRespTopAccounts').dxChart('instance').option('dataSource', dt);
            $('#salesRespTopAccounts').dxChart('instance')._render();

        },
        error: function (err, er1, er2) {

        }
    });
}
function hidePopup() {
    $("#nodataPopup").dxPopup("hide");
}

function ShowHideBtnNextPrev() {
    setPageDetails();
    if ((Current_Page_TopAccount * Default_Page_Size) < parseInt($("#Top_Account_TotalCount").val())) {
        $("#btnTopAccNext").show();
    }
    else {
        $("#btnTopAccNext").hide();
    }
    if (Current_Page_TopAccount == 1) {
        $("#btnTopAccPrev").hide();
    }
    else {
        $("#btnTopAccPrev").show();
    }
}
function setPageDetails() {
    $("#spnCurrentPage").html(Current_Page_TopAccount);
    var totalPages = parseInt($("#Top_Account_TotalCount").val()) / Default_Page_Size;
    $("#spnTotalPage").html(Math.ceil(totalPages));
}