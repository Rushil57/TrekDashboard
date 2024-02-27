var isFromApply = true;
var glblSortOrder = "asc";
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
function CreateBUDropdown() {
    var treeView, dataGrid;

    var syncTreeViewSelection = function (treeView, value) {
        if (!value) {
            treeView.unselectAll();
            return;
        }

        value.forEach(function (key) {
            treeView.selectItem(key);
        });
    };
    var makeAsyncDataSourceTest = $("#hdnlistBUDataCnt").html();
    var makeAsyncDataSource = JSON.parse(makeAsyncDataSourceTest);
    var placeholderValueInit = "BU (" + makeAsyncDataSource.length + " of " + makeAsyncDataSource.length + " selected)";
    if (placeholderValueInit == undefined || placeholderValueInit == null || placeholderValueInit == "") {
        placeholderValueInit = "BU";
    }
    try {
        $("#treeBox").dxDropDownBox("dispose");
        $("#treeBox").remove();
        $("#treeBox_prnt").html("<div id='treeBox'></div>");
    } catch (e) { }
    $("#treeBox").dxDropDownBox({
        //value: ["1_1"],
        valueExpr: "value",
        displayExpr: "text",
        placeholder: placeholderValueInit,
        showClearButton: true,
        dataSource: makeAsyncDataSource,
        accessKey: "LegacyBU",
        contentTemplate: function (e) {
            var value = e.component.option("value"),
                $treeView = $("<div>").dxTreeView({
                    dataSource: e.component.option("dataSource"),
                    dataStructure: "plain",
                    //keyExpr: "ID",
                    //parentIdExpr: "categoryId",
                    selectionMode: "multiple",
                    displayExpr: "text",
                    selectByClick: true,
                    onContentReady: function (args) {
                        //syncTreeViewSelection(args.component, value);
                        args.component.selectAll();
                    },
                    selectNodesRecursive: false,
                    showCheckBoxesMode: "normal",
                    onItemSelectionChanged: function (args) {
                        var value = args.component.getSelectedNodesKeys();
                        e.component.option("value", value);
                    }
                });

            treeView = $treeView.dxTreeView("instance");

            e.component.on("valueChanged", function (args) {
                var value = args.value;
                var text = args.text;
                syncTreeViewSelection(treeView, value);
                updatePlaceholderForBU();
                btnFiltersClick(args);
                //UpdateAccountSearchData(value);
            });

            return $treeView;
        }
    });

}
$(function () {
    CreateBUDropdown();
});
$(document).ready(function () {

    _genericElements.UserInfo = $("#UserInfoHeader").val();
    SetSiteCount();

    // Note: This function will use for Search Acount functionality in Single Account Dashboard.
    $("#selectAccounts").dxSelectBox({
        dataSource: [{ text: 'All' }, { text: 'Selected from search', disabled: true }],
        displayExpr: 'text',
        valueExpr: 'text',
        placeholder: "Accounts (All)",
        onValueChanged: function (arg) {
            if (isFromApply) {
                selectAccountsChanged(arg)
            }
            isFromApply = true;
        },
        onFocusOut: function (arg) {
            setPlaceholderForAccounts();
        },
    });

    //UpdateAccountSearchData(null);

});
function SetSiteCount() {
    var modelSiteRawData = $("#hdnModelSiteData").html();
    var modelSiteJsonData = JSON.parse(modelSiteRawData);
    if (modelSiteJsonData.length > 0) {
        $("#cntNoSite").html(modelSiteJsonData[0].NoSite);
        $("#cntNoSiteStates").html(modelSiteJsonData[0].NoSiteStates);
        $("#cntNoSiteCities").html(modelSiteJsonData[0].NoSiteCities);
        $("#cntNoProposals").html(modelSiteJsonData[0].NoProposals);
        $("#cntTotalLegacyQty").html(modelSiteJsonData[0].TotalLegacyQty);
        $("#cntTotalSalesOpps").html(Globalize('en-US').currencyFormatter('USD', {
            maximumFractionDigits: 0
        })(modelSiteJsonData[0].TotalSalesOpps));

        $("#cntHighRankingTotalSalesOpps").html(Globalize('en-US').currencyFormatter('USD', {
            maximumFractionDigits: 0
        })(parseFloat(modelSiteJsonData[0].HighRankingTotalSalesOpps)));
        $("#cntHighRankingProposals").html(modelSiteJsonData[0].HighRankingProposals);
        $("#cntAccounts").html(modelSiteJsonData[0].Accounts);
    }

}
function topAccountsArgClicked(e) {

    var colNameValue = "AND SiteCity  = '" + e.argument + "' ";
    var pointValueArray = [];
    var instance = e.component;
    instance.clearSelection();
    var series = e.component.getAllSeries();
    var SiteUrl = '';

    for (var i = 0; i < series.length; i++) {
        if (series[i].getPointsByArg(e.argument)[0] !== undefined) {
            SiteUrl = series[i].getPointsByArg(e.argument)[0].tag.SiteUrl;
            series[i].getPointsByArg(e.argument)[0].select();
            if (series[i].getPointsByArg(e.argument)[0].originalValue > 0)
                pointValueArray.push("'" + series[i].getPointsByArg(e.argument)[0].series.name + "'");
        }
    }

    var pointValuestring = "AND SiteID in (" + pointValueArray.join(',') + " )";
    _genericElements.TargetToExclude = "topAccountsChartSettings";
    _genericElements.WhereConditions = colNameValue + pointValuestring;

    var callerId = 0;
    try {
        callerId = window.parent.GetCallerID();
    }
    catch (e) {

    }
    if (callerId < 1) {
        var AccountID, UserID;
        AccountID = '', UserID = '';
        var user = JSON.stringify({ UserID: UserID, AccountID: AccountID, UserLevel: 1 });
        //var user = JSON.stringify({ UserID: UserID, AccountID: AccountID, UserLevel: 1, FirstUser: _genericElements.UserInfo });
        var xhrSettings = JSON.stringify({ headers: [{ name: "UserInfo", value: user }] });
        var encUrl = $("#hdnEncryptionTestUrl").val();
        //ajax call
        var modelData = {}
        modelData.XhrSettings = user;
        $.ajax({
            url: encUrl,
            data: modelData,
            type: 'POST',
            //dataType: 'json',
            success: function (dt) {

                var Ds = [{ text: 'Drill Down', value: "drilldown" }, { text: 'Go to ' + e.argument + ' Details', value: SiteUrl }];

                //viewModel._topAccountsMenu.alternativeInvocationMode.invokingElement = e.JQueryEvent; // ##
                $("#context-menu").dxContextMenu({
                    dataSource: Ds,
                    width: 200,
                    position:
                        {
                            of: e.jQueryEvent
                        },
                    onItemClick: function (e) {

                        if (e.itemData.value === "drilldown") {
                            $('#topaccounts').dxChart('instance').clearSelection();
                            _genericElements.FirstLoad = false;
                            DataLoadForAllaccounts();
                        }
                        else {
                            var win = window.open(e.itemData.value, 'HomeTab');
                            win.focus();
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
        _genericElements.FirstLoad = false;
        DataLoadForAllaccounts();
    }

}

function expiringContractsChartToolTipHtml(arg) {

    var value = arg.seriesName.indexOf(' Sales') > 0
        ?
        Globalize('en_US').currencyFormatter('USD', { maximumFractionDigits: 0 })(arg.value)
        :
        Globalize('en_US').numberFormatter()(arg.value);
    var ht = '';
    if (arg.seriesName.indexOf(' Sales') < 0)
        ht = "<div class='state-tooltip' style='height: 100%;display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Month </label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + (arg.argument) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>" + arg.seriesName + "</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + value + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    </table>\
                               </div>";
    else {

        ht = "<div class='state-tooltip' style='height: 100%;display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Month </label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + (arg.argument) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>" + arg.seriesName + "</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + value + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Legacy Quantity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en_US').numberFormatter()(arg.point.data.LegacyQty) + "</label></div></td> \
                                    </tr> \
                                    </table>\
                               </div>";
        //<td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en_US').numberFormatter()(arg.point.tag.LegacyQty) + "</label></div></td> \
    }
    return {
        html: ht

    }

}

function proposalsToolTipHtml(arg) {

    return {
        html:
            "<div class='state-tooltip' style='height: 100%;display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Proposal Ranking</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').currencyFormatter('USD', { maximumFractionDigits: 0 })(arg.point.tag) + "</label></div></td> \
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

function topproductsToolTipHtml(arg) {
    return {
        html:

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
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter()(arg.value) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').currencyFormatter('USD', { maximumFractionDigits: 0 })(arg.point.tag.SalesOpp) + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>" + window.SalesAnalytics.tooltip.noProposals + "</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').numberFormatter({ maximumFractionDigits: 0 })(arg.point.tag.Proposals) + "</label></div></td> \
                                    </tr> \
                                    </table>\
                               </div>"
    }

}

function topproposalsGridRowClick(e) {
    var callerId = 0;
    try {
        callerId = window.parent.GetCallerID();
    }
    catch (e) {

    }
    var ds;
    if (callerId < 1) {
        ds = [{ text: 'Drill Down', value: "drilldown" }, { text: 'Go to ' + e.data.ProposalID + ' Details', value: e.data.ProposalURL }];
    }
    else {
        ds = [{ text: 'Drill Down', value: "drilldown" }, { text: 'Go to ' + e.data.ProposalID + ' Details', value: "GetProposal(" + e.data.ProposalID + ")" }];
    }

    if (parseInt($("#UserLevel").val()) == 1) {
        var pqObj = {};
        pqObj.text = 'Add to Proposal Queue';
        pqObj.value = 'send';
        pqObj.proposalID = e.data.ProposalID;
        ds.push(pqObj);
    }
    var _proposalID = e.data.ProposalID;
    _genericElements.TargetToExclude = "TopProposalsGrid";
    _genericElements.WhereConditions = " And tbldb_Proposals.ProposalID = '" + _proposalID + "'";
    _genericElements.FirstLoad = false;
    $("#topProposalsMenu").dxContextMenu({
        dataSource: ds,
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
            else if (e.itemData.value === "send") {
                SendProposalToQueue(e.itemData.proposalID, $("#UserGUID").val(), $("#DBUserId").val());
            }
            else {
                if (e.itemData.value.indexOf('GetProposal') > -1) {
                    //viewModel.CreateSessionValues(e.itemData.value, 2);
                }
                else {
                    var win = window.open(e.itemData.value, 'HomeTab');
                    win.focus();
                }
            }
        }
    });
    $('#topProposalsMenu').dxContextMenu('instance').show();
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
    return {
        html:

            "<div class='state-tooltip' style='height: 100%;display: inline-block;'>\
                                    <table style='width: 100%'> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Site City </label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.argumentText + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Site ID</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.seriesName + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Site Name</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + arg.point.tag.SiteName + "</label></div></td> \
                                    </tr> \
                                    <tr> \
                                    <td><div class='labelHolder'><label>Total Sales Opportunity</label></div> </td>\
                                    <td style='padding:5px;'></td> \
                                    <td><div><label style='color:" + arg.point.getColor() + "'>" + Globalize('en-US').currencyFormatter('USD', { maximumFractionDigits: 0 })(arg.value) + "</label></div></td> \
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

function topAccountsOnPointClick(arg) {
    var instance = arg.component;
    instance.clearSelection();
    arg.target.select();

    if (arg.target !== null) {
        var _account = arg.target.argument;
        var _BU = arg.target.series.name;
        _genericElements.TargetToExclude = "topAccountsChartSettings";
        _genericElements.WhereConditions = " And SiteCity = '" + _account + "' And SiteID = '" + _BU + "'";
        _genericElements.FirstLoad = false;
        DataLoadForAllaccounts();
    }
}
function expiringContractsChartOnPointClick(arg) {
    arg.target.select();
    if (arg.target !== null) {
        var _expiryDate = arg.target.argument;

        var dt = Globalize('en-US').dateFormatter()(new Date(_expiryDate));
        if (dt.indexOf('NaN') > -1) {
            dt = Globalize('en-US').dateFormatter()(new Date('01 ' + _expiryDate));
        }

        _genericElements.TargetToExclude = "expiringContractsChartSettings";
        _genericElements.WhereConditions = ' And date_format(IF(LegacyContractEndDate = "" , NULL ,LegacyContractEndDate ),"%b %Y") = date_format(STR_TO_DATE("' + Globalize('en-US').dateFormatter()(new Date(_expiryDate)) + '", "%m/%d/%Y %H:%i:%s"),"%b %Y")';
        _genericElements.FirstLoad = false;
        DataLoadForAllaccounts();
    }
}

function proposalsOnPointClick(arg) {
    arg.target.select();

    if (arg.target !== null) {
        var _proposaRank = arg.target.argument;
        _genericElements.TargetToExclude = "proposalsPieSettings";
        _genericElements.WhereConditions = " And ProposalRank = '" + _proposaRank + "'";
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
        _genericElements.TargetToExclude = "TopLegacyProducts";
        _genericElements.WhereConditions = " And LegacyShortName = '" + _legacyName + "'" + _legacyAge;
        _genericElements.FirstLoad = false;
        DataLoadForAllaccounts();
    }
}

function topproductsOnArgumentAxisClick(e) {
    var colNameValue = "AND LegacyShortName  = '" + e.argument + "' ";
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
    var pointValuestring = whereCol;
    _genericElements.TargetToExclude = "TopLegacyProducts";
    _genericElements.WhereConditions = colNameValue + pointValuestring;
    _genericElements.FirstLoad = false;
    DataLoadForAllaccounts();
}
function DataLoadForAllaccounts(isFilterbuttonClicked, isSingleAccountSearch) {
    $('#dataLoadPanel').dxLoadPanel('instance').show();
    if (_genericElements.FirstLoad == false) {
        var filterString = getFilterString();
        _genericElements.WhereConditions = _genericElements.WhereConditions + filterString;
    }

    // Note: This function will use for Search Acount functionality in Single Account Dashboard.
    //var accSearchType = "0";
    //var serarchTypeValue = $('#selectAccounts').dxSelectBox('instance').option('value');
    //if (serarchTypeValue == "Selected from search") {
    //    accSearchType = "1";
    //}

    var apiUrlLink = $("#ApiURLlink").val();
    $.ajax({
        url: apiUrlLink + "api/SingleAccount/GetSingleAccountData",
        data: {},
        type: 'GET',
        //dataType: 'json',
        headers: { "UserInfo": _genericElements.UserInfo, "AccountID": _genericElements.AccountID, "WhereConditions": _genericElements.WhereConditions, "CallerID": "" },
        success: function (dt) {
            setChartData(dt, isSingleAccountSearch);
        },
        error: function (err, er1, er2) {
            $('#dataLoadPanel').dxLoadPanel('instance').hide();
            $("#nodataPopup").dxPopup("show");

        }
    });
}

function setChartData(dt, isSingleAccountSearch) {

    if (dt != undefined) {
        if (dt.DateRefreshed != undefined) {
            $("#lblDateRefreshed").html(dt.DateRefreshed);
        }
        if (dt.Version != undefined) {
            $("#lblVersion").html(dt.Version);
        }
        if (dt.Site_Card.length === 0
            && dt.TopSites_StackedBar.length === 0
            && dt.ExpiringContracts_MixedChart.length === 0
            && dt.Proposals_Pie.length === 0
            && dt.TopProposals_Grid.length === 0
            && dt.TopLegacyProduct.length === 0) {
            $("#nodataPopup").dxPopup("show");
            //disableFilters(true); //##
            $("#cntNoSite").html("0");
            $("#cntNoSiteStates").html("0");
            $("#cntNoSiteCities").html("0");
            $("#cntNoProposals").html("0");
            $("#cntTotalLegacyQty").html("0");
            $("#cntTotalSalesOpps").html(Globalize('en-US').currencyFormatter('USD', {
                maximumFractionDigits: 0
            })(0));

            $("#cntHighRankingTotalSalesOpps").html(Globalize('en-US').currencyFormatter('USD', {
                maximumFractionDigits: 0
            })(0));
            $("#cntHighRankingProposals").html("0");
            $("#cntAccounts").html("0");


            $('#topaccounts').dxChart('instance').option('dataSource', []);
            $('#topaccounts').dxChart('instance')._render();

            $('#expiringcontracts').dxChart('instance').option('dataSource', []);
            $('#expiringcontracts').dxChart('instance')._render();

            $('#proposals').dxPieChart('instance').option('dataSource', []);
            $('#proposals').dxPieChart('instance')._render();

            $('#topproposals').dxDataGrid('instance').option('dataSource', []);
            $('#topproposals').dxDataGrid('instance')._render();
            $('#topproposals').dxDataGrid('instance').pageIndex(0);

            $('#topproducts').dxChart('instance').option('dataSource', []);
            $('#topproducts').dxChart('instance')._render();
            //##
        }
        else {

            if (isSingleAccountSearch != undefined && isSingleAccountSearch == true && dt.CurrentUser != undefined) {
                try {
                    var crntUsr = dt.CurrentUser.replace(/>/g, "<");
                    var pos = crntUsr.lastIndexOf('.');
                    crntUsr = crntUsr.substring(0, pos) + ' ' + crntUsr.substring(pos + 1)

                    $("#lblCurrentUser").html($("#Name").val().replace(/>/g, "<") + " < " + crntUsr);
                } catch (e) {

                }
            }
            $("#cntNoSite").html(dt.Site_Card[0].NoSite);
            $("#cntNoSiteStates").html(dt.Site_Card[0].NoSiteStates);
            $("#cntNoSiteCities").html(dt.Site_Card[0].NoSiteCities);
            $("#cntNoProposals").html(dt.Site_Card[0].NoProposals);
            $("#cntTotalLegacyQty").html(dt.Site_Card[0].TotalLegacyQty);
            $("#cntTotalSalesOpps").html(Globalize('en-US').currencyFormatter('USD', {
                maximumFractionDigits: 0
            })(dt.Site_Card[0].TotalSalesOpps));

            $("#cntHighRankingTotalSalesOpps").html(Globalize('en-US').currencyFormatter('USD', {
                maximumFractionDigits: 0
            })(parseFloat(dt.Site_Card[0].HighRankingTotalSalesOpps)));
            $("#cntHighRankingProposals").html(dt.Site_Card[0].HighRankingProposals);
            $("#cntAccounts").html(dt.Site_Card[0].Accounts);
            if (_genericElements.FirstLoad === true) {

                $('#accounts').dxPieChart('instance').option('dataSource', dt.Account_Pie);
                $('#accounts').dxPieChart('instance')._render();

                $('#topaccounts').dxChart('instance').option('dataSource', dt.TopSites_StackedBar);
                $('#topaccounts').dxChart('instance')._render();

                $('#expiringcontracts').dxChart('instance').option('dataSource', dt.ExpiringContracts_MixedChart);
                $('#expiringcontracts').dxChart('instance')._render();

                $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                $('#proposals').dxPieChart('instance')._render();

                $('#topproposals').dxDataGrid('instance').option('dataSource', dt.TopProposals_Grid);
                $('#topproposals').dxDataGrid('instance')._render();
                $('#topproposals').dxDataGrid('instance').pageIndex(0);

                $('#topproducts').dxChart('instance').option('dataSource', dt.TopLegacyProduct);
                $('#topproducts').dxChart('instance')._render();

                //_genericElements.BUArray(_genericElements.BUArray().length === 0 ? dt[0].BUList : _genericElements.BUArray());
                //if (viewModel.BUList.list.selectedItems().length === 0) {
                //    viewModel.BUList.value("BU (" + viewModel.BUList.list.dataSource().length + " of " + viewModel.BUList.list.dataSource().length + " selected)");
                //    viewModel.BUList.list.selectedItems(viewModel.BUList.list.dataSource());
                //}
                _genericElements.FirstLoad = false;

            }
            else {
                switch (_genericElements.TargetToExclude) {
                    case "accountPieSettings":

                        $('#expiringcontracts').dxChart('instance').option('dataSource', dt.ExpiringContracts_MixedChart);
                        $('#expiringcontracts').dxChart('instance')._render();

                        $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                        $('#proposals').dxPieChart('instance')._render();

                        $('#topproposals').dxDataGrid('instance').option('dataSource', dt.TopProposals_Grid);
                        $('#topproposals').dxDataGrid('instance')._render();
                        $('#topproposals').dxDataGrid('instance').pageIndex(0);

                        $('#topproducts').dxChart('instance').option('dataSource', dt.TopLegacyProduct);
                        $('#topproducts').dxChart('instance')._render();
                        break;
                    case "topAccountsChartSettings":

                        $('#accounts').dxPieChart('instance').option('dataSource', dt.Account_Pie);
                        $('#accounts').dxPieChart('instance')._render();

                        $('#expiringcontracts').dxChart('instance').option('dataSource', dt.ExpiringContracts_MixedChart);
                        $('#expiringcontracts').dxChart('instance')._render();

                        $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                        $('#proposals').dxPieChart('instance')._render();

                        $('#topproposals').dxDataGrid('instance').option('dataSource', dt.TopProposals_Grid);
                        $('#topproposals').dxDataGrid('instance')._render();
                        $('#topproposals').dxDataGrid('instance').pageIndex(0);

                        $('#topproducts').dxChart('instance').option('dataSource', dt.TopLegacyProduct);
                        $('#topproducts').dxChart('instance')._render();
                        break;
                    case "expiringContractsChartSettings":

                        $('#accounts').dxPieChart('instance').option('dataSource', dt.Account_Pie);
                        $('#accounts').dxPieChart('instance')._render();

                        $('#topaccounts').dxChart('instance').option('dataSource', dt.TopSites_StackedBar);
                        $('#topaccounts').dxChart('instance')._render();

                        $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                        $('#proposals').dxPieChart('instance')._render();

                        $('#topproposals').dxDataGrid('instance').option('dataSource', dt.TopProposals_Grid);
                        $('#topproposals').dxDataGrid('instance')._render();
                        $('#topproposals').dxDataGrid('instance').pageIndex(0);

                        $('#topproducts').dxChart('instance').option('dataSource', dt.TopLegacyProduct);
                        $('#topproducts').dxChart('instance')._render();
                        break;
                    case "proposalsPieSettings":

                        $('#accounts').dxPieChart('instance').option('dataSource', dt.Account_Pie);
                        $('#accounts').dxPieChart('instance')._render();

                        $('#topaccounts').dxChart('instance').option('dataSource', dt.TopSites_StackedBar);
                        $('#topaccounts').dxChart('instance')._render();

                        $('#expiringcontracts').dxChart('instance').option('dataSource', dt.ExpiringContracts_MixedChart);
                        $('#expiringcontracts').dxChart('instance')._render();

                        $('#topproposals').dxDataGrid('instance').option('dataSource', dt.TopProposals_Grid);
                        $('#topproposals').dxDataGrid('instance')._render();
                        $('#topproposals').dxDataGrid('instance').pageIndex(0);

                        $('#topproducts').dxChart('instance').option('dataSource', dt.TopLegacyProduct);
                        $('#topproducts').dxChart('instance')._render();

                        break;
                    case "TopProposalsGrid":

                        $('#accounts').dxPieChart('instance').option('dataSource', dt.Account_Pie);
                        $('#accounts').dxPieChart('instance')._render();

                        $('#topaccounts').dxChart('instance').option('dataSource', dt.TopSites_StackedBar);
                        $('#topaccounts').dxChart('instance')._render();

                        $('#expiringcontracts').dxChart('instance').option('dataSource', dt.ExpiringContracts_MixedChart);
                        $('#expiringcontracts').dxChart('instance')._render();

                        $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                        $('#proposals').dxPieChart('instance')._render();

                        $('#topproducts').dxChart('instance').option('dataSource', dt.TopLegacyProduct);
                        $('#topproducts').dxChart('instance')._render();

                        break;
                    case "TopLegacyProducts":

                        $('#accounts').dxPieChart('instance').option('dataSource', dt.Account_Pie);
                        $('#accounts').dxPieChart('instance')._render();

                        $('#topaccounts').dxChart('instance').option('dataSource', dt.TopSites_StackedBar);
                        $('#topaccounts').dxChart('instance')._render();

                        $('#expiringcontracts').dxChart('instance').option('dataSource', dt.ExpiringContracts_MixedChart);
                        $('#expiringcontracts').dxChart('instance')._render();

                        $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                        $('#proposals').dxPieChart('instance')._render();

                        $('#topproposals').dxDataGrid('instance').option('dataSource', dt.TopProposals_Grid);
                        $('#topproposals').dxDataGrid('instance')._render();
                        $('#topproposals').dxDataGrid('instance').pageIndex(0);

                        break;
                    default:

                        $('#accounts').dxPieChart('instance').option('dataSource', dt.Account_Pie);
                        $('#accounts').dxPieChart('instance')._render();

                        $('#topaccounts').dxChart('instance').option('dataSource', dt.TopSites_StackedBar);
                        $('#topaccounts').dxChart('instance')._render();

                        $('#expiringcontracts').dxChart('instance').option('dataSource', dt.ExpiringContracts_MixedChart);
                        $('#expiringcontracts').dxChart('instance')._render();

                        $('#proposals').dxPieChart('instance').option('dataSource', dt.Proposals_Pie);
                        $('#proposals').dxPieChart('instance')._render();

                        $('#topproposals').dxDataGrid('instance').option('dataSource', dt.TopProposals_Grid);
                        $('#topproposals').dxDataGrid('instance')._render();
                        $('#topproposals').dxDataGrid('instance').pageIndex(0);

                        $('#topproducts').dxChart('instance').option('dataSource', dt.TopLegacyProduct);
                        $('#topproducts').dxChart('instance')._render();
                        break;

                }
            }
        }
        $('#dataLoadPanel').dxLoadPanel('instance').hide();
    }
}
var sortInts = function (data) {
    var dataSorted = data.sort(function (a, b) { return a - b; });
    return dataSorted;
}
function btnFiltersClick(e) {

    var filter = '';
    var colName = e.component._options.accessKey;
    var isFilterbutton = false;
    switch (colName) {
        case "ProposalIsHighRank":
            isFilterbutton = true;
            $("#btnHRP").toggleClass("filter-selected").focusout();
            //filter = ' AND ' + colName + " = 1"
            //_genericElements.WhereConditions = filter;
            //_genericElements.FirstLoad = true;
            //DataLoadForAllaccounts();
            break;
        case "LegacyContractEndDate":
            isFilterbutton = true;
            $("#btnCE").toggleClass("filter-selected").focusout();
            break;

        case "LegacyEOSLDate":
            isFilterbutton = true;
            $("#btnEOSL").toggleClass("filter-selected").focusout();
            break;
        case "LegacyAge":
            isFilterbutton = true;
            $("#btnLegacy").toggleClass("filter-selected").focusout();
            break;
        case "LegacyBU":

            var values = e.value;
            var makeAsyncDataSourceTest = $("#hdnlistBUDataCnt").html();
            var makeAsyncDataSource = JSON.parse(makeAsyncDataSourceTest);
            var allList = "";
            for (var i = 0; i < values.length; i++) {
                //var filterednames = makeAsyncDataSource.filter(function (obj) {
                //    return (obj.value === values[i]);
                //});
                if (makeAsyncDataSource[values[i] - 1] != undefined) {
                    if (allList != "") {
                        allList += ",";
                    }
                    var nm = "'" + makeAsyncDataSource[values[i] - 1].text + "'";
                    allList += nm;
                }

            }
            filter = ' AND ' + colName + " in (" + allList + ")";
            _genericElements.WhereConditions = filter;
            //_genericElements.FirstLoad = true;
            DataLoadForAllaccounts();

            break;
        case "Close":
            if ($(".filter-selected").length > 0) {
                $(".filter-selected").removeClass("filter-selected");
            }
            _genericElements.WhereConditions = '';
            _genericElements.FirstLoad = true;
            RefreshAccountSearchPopup();
            CreateBUDropdown();
            DataLoadForAllaccounts();
            //disableFilters(false);

            break;
    }

    if (isFilterbutton) {

        var filter = "";
        var colName = "";
        if ($("#btnHRP").hasClass("filter-selected")) {
            colName = "ProposalIsHighRank";
            filter += ' AND ' + colName + " = 1";
        }
        if ($("#btnCE").hasClass("filter-selected")) {
            colName = "LegacyContractEndDate";
            var startDate = new Date();
            startDate.setMonth(startDate.getMonth() + 7);
            var endDate = new Date();
            endDate.setMonth(endDate.getMonth() + 12);

            filter += ' AND  IF(' + colName + ' = "",NULL,' + colName + ') >= STR_TO_DATE("' + Globalize('en-US').dateFormatter()(startDate) + '", "%m/%d/%Y %H:%i:%s") AND  IF(' + colName + ' = "",NULL,' + colName + ') < STR_TO_DATE("' + Globalize('en-US').dateFormatter()(endDate) + '", "%m/%d/%Y %H:%i:%s")';
        }
        if ($("#btnEOSL").hasClass("filter-selected")) {
            colName = "LegacyEOSLDate";
            var startDate1 = new Date();
            var endDate1 = new Date();
            endDate1.setMonth(endDate1.getMonth() + 18);
            filter += ' AND  IF(' + colName + ' = "",NULL,' + colName + ') >= STR_TO_DATE("' + Globalize('en-US').dateFormatter()(startDate1) + '", "%m/%d/%Y %H:%i:%s") AND  IF(' + colName + ' = "",NULL,' + colName + ') < STR_TO_DATE("' + Globalize('en-US').dateFormatter()(endDate1) + '", "%m/%d/%Y %H:%i:%s")';
        }
        if ($("#btnLegacy").hasClass("filter-selected")) {
            colName = "LegacyAge";
            filter += ' AND ' + colName + " BETWEEN 36 AND 72"
        }

        //_genericElements.WhereConditions = filter;
        _genericElements.FirstLoad = false;
        DataLoadForAllaccounts(true);
    }
}
function accountsOnPointClick(arg) {
    arg.target.select();

    if (arg.target !== null) {
        var _status = arg.target.argument;
        _genericElements.TargetToExclude = "accountPieSettings";
        _genericElements.WhereConditions = " And ProposalSFDCStatus = '" + _status + "'";
        DataLoadForAllaccounts();
    }
}
function hidePopup() {
    $("#nodataPopup").dxPopup("hide");
}
// Note: Below functions will use for Search Acount functionality in Single Account Dashboard.
function ShowAccountSearch() {
    $("#accountSearchPopup").dxPopup("show");
    var popup = $("#accountSearchPopup").dxPopup("instance");
    popup.show();
    //GetAccountSearchList();
}

function SearchAccountByKeyword(isRefresh) {
    var searchUrl = $("#hdnSearchAccountUrl").val();
    var searchObj = {};
    searchObj.keyWord = $("#txtAccountSearch").val().trim();
    $.ajax({
        url: searchUrl,
        data: { searchObj: searchObj },
        type: 'POST',
        //dataType: 'json',
        success: function (dt) {
            if (JSON.parse(dt).length > 0) {
                $("#searchResultText").show();
                //$("#searchResultTextForNoData").hide();
                //$("#btnAccountSearchViewSelecte").show();
                $("#btnAccountSearchClose").show();
                $("#btnAccountSearchClose").val("Cancel");
                //$("#search-acc-grid").show();
            }
            else {
                $("#searchResultText").hide();
                //$("#searchResultTextForNoData").show();
                //$("#btnAccountSearchViewSelecte").hide();
                $("#btnAccountSearchClose").show();
                $("#btnAccountSearchClose").val("Close");
                //$("#search-acc-grid").hide();
            }
            $("#searchCount").html(JSON.parse(dt).length);
            clearGridFilter();
            $('#accountsearchgrid').dxDataGrid('instance').option('dataSource', JSON.parse(dt));
            $('#accountsearchgrid').dxDataGrid('instance')._render();
            $('#accountsearchgrid').dxDataGrid('instance').pageIndex(0);
            $('#accountsearchgrid').dxDataGrid('instance').deselectAll();
            //if (isRefresh) {
            //    var serarchTypeValue = $('#selectAccounts').dxSelectBox('instance').option('value');
            //    if (serarchTypeValue == "Selected from search") {
            //        SetAccountSearchSelectedIds("1");
            //    }
            //}
        },
        error: function (err, er1, er2) {

        }
    });
}
function SetAccountSearchSelectedIds(isAccSearch) {
    var ids = $("#AccountSearchSelectedIds").val();
    if (ids != null && ids.trim() != "" && isAccSearch == "1") {
        var gridInstance = $('#accountsearchgrid').dxDataGrid('instance');
        var gridData = $('#accountsearchgrid').dxDataGrid('getDataSource');
        var dt = gridData.store()._array;
        var list = ids.split('|');
        var keyArray = [];
        for (var i = 0; i < list.length; i++) {
            var filterednames = dt.filter(function (obj) {
                return (obj.AccountID === list[i]);
            });
            if (filterednames.length == 1) {
                keyArray.push(filterednames[0]);
            }
        }

        gridInstance.selectRows(keyArray);
        //isFromAS = false;
        //var dropDownBox = $("#selectAccounts").dxSelectBox("instance");
        //dropDownBox.option("value", "Selected from search");
        //$("#selectAccounts").find(".dx-texteditor-input").val("Accounts (Selected from search)");

    }
}
function accountSearchApply() {

    var selectedAccount = $('#accountsearchgrid').dxDataGrid('instance').getSelectedRowKeys();
    var accList = [];
    for (var i = 0; i < selectedAccount.length; i++) {
        accList.push(selectedAccount[i].AccountID);
    }
    if (selectedAccount.length > 0) {

        $('#dataLoadPanel').dxLoadPanel('instance').show();
        var applySearchUrl = $("#hdnAppltAccountSearchUrl").val();
        var postHeaders = {};
        postHeaders.UserInfo = _genericElements.UserInfo;
        isFromApply = false;
        //
        $.ajax({
            url: applySearchUrl,
            data: { headers: postHeaders, selectedAccountIds: accList, name: $("#Name").val() },
            type: 'POST',
            //dataType: 'json',
            success: function (resData) {
                $('#dataLoadPanel').dxLoadPanel('instance').hide();

                if (accList.length == 1) {

                    var encUrl = $("#hdnEncryptionTestUrl").val();
                    var user = JSON.stringify({ UserID: $("#DBUserId").val(), AccountID: accList[0], FirstUser: _genericElements.UserInfo });
                    var modelData = {}
                    modelData.XhrSettings = user;
                    $.ajax({
                        url: encUrl,
                        data: modelData,
                        type: 'POST',
                        //dataType: 'json',
                        success: function (dt) {
                            //$('#dataLoadPanel').dxLoadPanel('instance').show();
                            //
                            if ($(".filter-selected").length > 0) {
                                $(".filter-selected").removeClass("filter-selected");
                            }
                            _genericElements.UserInfo = dt;
                            _genericElements.AccountID = accList[0];
                            _genericElements.WhereConditions = "";
                            DataLoadForAllaccounts(undefined, true);
                            //$('#dataLoadPanel').dxLoadPanel('instance').hide();
                            $("#accountSearchPopup").dxPopup("hide");
                            //
                            var data = [{ text: 'All' }, { text: 'Selected from search' }];
                            $('#selectAccounts').dxSelectBox('instance').option('dataSource', data);
                            var dropDownBox = $("#selectAccounts").dxSelectBox("instance");
                            dropDownBox.option("value", "Selected from search");
                            $("#selectAccounts").find(".dx-texteditor-input").val("Accounts (Selected from search)");
                            //
                            //return;
                            //
                            //var singleAccountDashUrl = $("#hdnSingleAccountDashUrl").val();
                            //window.location.href = singleAccountDashUrl + "?userInfo=" + dt + "&name=" + $("#CurrentUser").val();
                        },
                        error: function () {
                        }
                    });


                }
                else {
                    $('#dataLoadPanel').dxLoadPanel('instance').show();
                    var accountDashUrl = $("#hdnAccountDashUrl").val();
                    window.location.href = accountDashUrl + "?userInfo=" + $("#CurrentUserFromAccountsDash").val() + "&isAccSearch=true&name=" + $("#Name").val();
                }
            },
            error: function (err, er1, er2) {

            }
        });
    }
    else {
        DevExpress.ui.dialog.alert("Please select at least one account.", "");
    }

}
function accountSearchCancelClick() {
    $("#accountSearchPopup").dxPopup("hide");
}
function selectAccountsChanged(e) {
    var searchType = $('#selectAccounts').dxSelectBox('instance').option('value');
    if (searchType == "Selected from search") {
        accountSearchApply();
        $("#selectAccounts").find(".dx-texteditor-input").val("Accounts (Selected from search)");
    }
    else {
        // All
        $('#dataLoadPanel').dxLoadPanel('instance').show();
        var accountDashUrl = $("#hdnAccountDashUrl").val();
        window.location.href = accountDashUrl + "?userInfo=" + $("#CurrentUserFromAccountsDash").val() + "&name=" + $("#Name").val();
    }
}

function SendProposalToQueue(proposalId, userGUID, userId) {
    var apiUrlLink = $("#ApiURLlink").val();
    $('#dataLoadPanel').dxLoadPanel('instance').show();
    $.ajax({
        url: apiUrlLink + "api/AllAccounts/SendProposalToQueue",
        data: {},
        type: 'GET',
        //dataType: 'json',
        headers: { "proposalId": proposalId, "userGUID": userGUID, "userId": userId },
        success: function (dt) {
            $('#dataLoadPanel').dxLoadPanel('instance').hide();
            DevExpress.ui.dialog.alert("Proposal is being sent to the queue. It may take a few minutes before it's listed in the queue; refresh the page to view.", "");
            LoadProposalGrid();
        },
        error: function (dt) {
            $('#dataLoadPanel').dxLoadPanel('instance').hide();
        }
    });
}

function LoadProposalGrid() {
    var apiUrlLink = $("#ApiURLlink").val();
    $.ajax({
        url: apiUrlLink + "api/SingleAccount/GetSingleAccountData",
        data: {},
        type: 'GET',
        //dataType: 'json',
        headers: { "UserInfo": _genericElements.UserInfo, "AccountID": _genericElements.AccountID, "WhereConditions": "", "CallerID": "" },
        success: function (dt) {

            $('#dataLoadPanel').dxLoadPanel('instance').hide();
            if (dt != undefined && dt.TopProposals_Grid != undefined) {
                $('#topproposals').dxDataGrid('instance').option('dataSource', dt.TopProposals_Grid);
                $('#topproposals').dxDataGrid('instance')._render();
                $('#topproposals').dxDataGrid('instance').pageIndex(0);
            }
        },
        error: function (err) {

        }
    });
}
function OnSelectionChangedAccSearch(row) {
    var gridInstance = $('#accountsearchgrid').dxDataGrid('instance');
    if (row.selectedRowKeys.length <= 9999) {
        //this.selectedCount++;
    }
    else if (row.selectedRowKeys.length == 10000) {

        gridInstance.deselectRows(row.currentSelectedRowKeys);
        DevExpress.ui.dialog.alert("You have selected the maximum 9999 accounts. Unselect first if you want to select a different one.", "");
        //this.selectedCount--;
    }
    else {
        DevExpress.ui.dialog.alert("You have selected the maximum 9999 accounts. Unselect first if you want to select a different one.", "");
        gridInstance.selectRowsByIndexes([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
    }
}

function UpdateAccountSearchData(values) {

    var makeAsyncDataSourceTest = $("#hdnlistBUDataCnt").html();
    var makeAsyncDataSource = JSON.parse(makeAsyncDataSourceTest);
    var allList = "";
    if (values != null) {
        for (var i = 0; i < values.length; i++) {
            if (makeAsyncDataSource[values[i] - 1] != undefined) {
                if (allList != "") {
                    allList += ",";
                }
                var nm = makeAsyncDataSource[values[i] - 1].text;
                allList += nm;
            }
        }
    }
    else {
        for (var i = 0; i < makeAsyncDataSource.length; i++) {
            if (makeAsyncDataSource[i] != undefined) {
                if (allList != "") {
                    allList += ",";
                }
                var nm = makeAsyncDataSource[i].text;
                allList += nm;
            }
        }
    }

    var url = $("#hdnUpdateAccountSearchData").val();
    var modelData = {}
    modelData.AllList = allList;

    $.ajax({
        url: url,
        data: { modelData: modelData },
        type: 'POST',
        //dataType: 'json',
        success: function (dt) {
        },
        error: function (err) {

        }
    });


}

function GetAccountSearchList() {
    return; // ###### Need to confirm - BU filter apply in AccountSearch Grid data.
    var url = $("#hdnGetAccountSearchList").val();
    $.ajax({
        url: url,
        data: {},
        type: 'POST',
        //dataType: 'json',
        success: function (dt) {

            $('#accountsearchgrid').dxDataGrid('instance').option('dataSource', JSON.parse(dt));
            $('#accountsearchgrid').dxDataGrid('instance')._render();
            $('#accountsearchgrid').dxDataGrid('instance').pageIndex(0);
        },
        error: function (err) {
        }
    });
}
function updatePlaceholderForBU() {
    try {

        var buInstance = $('#treeBox').dxDropDownBox('instance');
        var len = 0;
        if (buInstance.option('value') == undefined || buInstance.option('value') == null) {
            var makeAsyncDataSourceTest = $("#hdnlistBUDataCnt").html();
            var makeAsyncDataSource = JSON.parse(makeAsyncDataSourceTest);
            len = makeAsyncDataSource.length;
        }
        else {
            len = buInstance.option('value').length;
        }

        var placeholderValue = "BU (" + len + " of " + buInstance.option('dataSource').length + " selected)";
        buInstance.option('placeholder', placeholderValue);
    } catch (e) {

    }
}
function RefreshAccountSearchPopup() {
    try {
        //var serarchTypeValue = $('#selectAccounts').dxSelectBox('instance').option('value');
        //serarchTypeValue == null ? "" : serarchTypeValue;
        //if (serarchTypeValue == "All") {

        //}
        $("#txtAccountSearch").val('');
        SearchAccountByKeyword(true);
        //var selectedAccount = $('#accountsearchgrid').dxDataGrid('instance').getSelectedRowKeys();
        //if (selectedAccount == null || selectedAccount.length == 0) {
        //    $("#txtAccountSearch").val('');
        //    SearchAccountByKeyword(true);
        //    var clGrid = $('#accountsearchgrid').dxDataGrid('instance');
        //    clGrid.deselectAll();
        //}
    } catch (e) {

    }
}

function setPlaceholderForAccounts() {

    try {
        setTimeout(function () {
            var searchType = $('#selectAccounts').dxSelectBox('instance').option('value');
            if (searchType == "Selected from search") {
                $("#selectAccounts").find(".dx-texteditor-input").val("Accounts (Selected from search)");
            }
            else if (searchType == "All") {
                $("#selectAccounts").find(".dx-texteditor-input").val("Accounts (All)");
            }
        }, 10);

    } catch (e) {

    }
}

function getFilterString() {
    var filter = "";
    var colName = "";
    if ($("#btnHRP").hasClass("filter-selected")) {
        colName = "ProposalIsHighRank";
        filter += ' AND ' + colName + " = 1";
    }
    else {
        colName = "ProposalIsHighRank";
        var cl_hrp = ' AND ' + colName + " = 1";
        _genericElements.WhereConditions = replaceAll(_genericElements.WhereConditions, cl_hrp);
    }
    if ($("#btnCE").hasClass("filter-selected")) {
        colName = "LegacyContractEndDate";
        var startDate = new Date();
        startDate.setMonth(startDate.getMonth() + 7);
        var endDate = new Date();
        endDate.setMonth(endDate.getMonth() + 12);

        filter += ' AND  IF(' + colName + ' = "",NULL,' + colName + ') >= STR_TO_DATE("' + Globalize('en-US').dateFormatter()(startDate) + '", "%m/%d/%Y %H:%i:%s") AND  IF(' + colName + ' = "",NULL,' + colName + ') < STR_TO_DATE("' + Globalize('en-US').dateFormatter()(endDate) + '", "%m/%d/%Y %H:%i:%s")';
    }
    else {
        colName = "LegacyContractEndDate";
        var startDate = new Date();
        startDate.setMonth(startDate.getMonth() + 7);
        var endDate = new Date();
        endDate.setMonth(endDate.getMonth() + 12);

        var cl_ce = ' AND  IF(' + colName + ' = "",NULL,' + colName + ') >= STR_TO_DATE("' + Globalize('en-US').dateFormatter()(startDate) + '", "%m/%d/%Y %H:%i:%s") AND  IF(' + colName + ' = "",NULL,' + colName + ') < STR_TO_DATE("' + Globalize('en-US').dateFormatter()(endDate) + '", "%m/%d/%Y %H:%i:%s")';
        _genericElements.WhereConditions = replaceAll(_genericElements.WhereConditions, cl_ce);
    }
    if ($("#btnEOSL").hasClass("filter-selected")) {
        colName = "LegacyEOSLDate";
        var startDate1 = new Date();
        var endDate1 = new Date();
        endDate1.setMonth(endDate1.getMonth() + 18);
        filter += ' AND  IF(' + colName + ' = "",NULL,' + colName + ') >= STR_TO_DATE("' + Globalize('en-US').dateFormatter()(startDate1) + '", "%m/%d/%Y %H:%i:%s") AND  IF(' + colName + ' = "",NULL,' + colName + ') < STR_TO_DATE("' + Globalize('en-US').dateFormatter()(endDate1) + '", "%m/%d/%Y %H:%i:%s")';
    }
    else {
        colName = "LegacyEOSLDate";
        var startDate1 = new Date();
        var endDate1 = new Date();
        endDate1.setMonth(endDate1.getMonth() + 18);
        var cl_eosl = ' AND  IF(' + colName + ' = "",NULL,' + colName + ') >= STR_TO_DATE("' + Globalize('en-US').dateFormatter()(startDate1) + '", "%m/%d/%Y %H:%i:%s") AND  IF(' + colName + ' = "",NULL,' + colName + ') < STR_TO_DATE("' + Globalize('en-US').dateFormatter()(endDate1) + '", "%m/%d/%Y %H:%i:%s")';
        _genericElements.WhereConditions = replaceAll(_genericElements.WhereConditions, cl_eosl);
    }
    if ($("#btnLegacy").hasClass("filter-selected")) {
        colName = "LegacyAge";
        filter += ' AND ' + colName + " BETWEEN 36 AND 72"
    }
    else {
        colName = "LegacyAge";
        var cl_legacy = ' AND ' + colName + " BETWEEN 36 AND 72";
        _genericElements.WhereConditions = replaceAll(_genericElements.WhereConditions, cl_legacy);
    }

    var buInstance = $('#treeBox').dxDropDownBox('instance');
    var len = 0;
    if (buInstance.option('value') != undefined || buInstance.option('value') != null) {
        var values = buInstance.option('value');
        var makeAsyncDataSourceTest = $("#hdnlistBUDataCnt").html();
        var makeAsyncDataSource = JSON.parse(makeAsyncDataSourceTest);
        var allList = "";
        for (var i = 0; i < values.length; i++) {
            if (makeAsyncDataSource[values[i] - 1] != undefined) {
                if (allList != "") {
                    allList += ",";
                }
                var nm = "'" + makeAsyncDataSource[values[i] - 1].text + "'";
                allList += nm;
            }
        }
        colName = "LegacyBU";
        filter += ' AND ' + colName + " in (" + allList + ")";
    }

    return filter;
}

function replaceAll(source, search) {
    var target = source;
    var replacement = "";
    return target.split(search).join(replacement);
}
function btnExportClick() {
    var accGrid = $('#accountsearchgrid').dxDataGrid('instance');
    var selectedAccount = accGrid.getSelectedRowKeys();
    if (selectedAccount.length > 0) {
        accGrid.exportToExcel(true);
    }
    else {
        DevExpress.ui.dialog.alert("Please select at least one account.", "");
    }
}
function clearGridFilter() {
    try {
        $('#accountsearchgrid').dxDataGrid('instance').filter(null);
        $('#accountsearchgrid').dxDataGrid('instance').columnOption("AccountID", "filterValues", []);
        $('#accountsearchgrid').dxDataGrid('instance').columnOption("AccountName", "filterValues", []);
        $('#accountsearchgrid').dxDataGrid('instance').columnOption("Country", "filterValues", []);
        $('#accountsearchgrid').dxDataGrid('instance').columnOption("TotalProposalSalesOpportunity", "filterValues", []);
        $('#accountsearchgrid').dxDataGrid('instance').columnOption("TotalTopProposals", "filterValues", []);
        $('#accountsearchgrid').dxDataGrid('instance').columnOption("TotalTopLegacyProducts", "filterValues", []);
    } catch (e) { }

}

function btnResetClick() {
    clearGridFilter();
    $("#txtAccountSearch").val("");
    $('#accountsearchgrid').dxDataGrid('instance').deselectAll();
    $('#accountsearchgrid').dxDataGrid('instance').clearSorting();
    var url = $("#hdnBindAccountSearchListOnResetClick").val();
    $.ajax({
        url: url,
        data: {},
        type: 'POST',
        //dataType: 'json',
        success: function (dt) {
            $('#accountsearchgrid').dxDataGrid('instance').option('dataSource', JSON.parse(dt));
            $('#accountsearchgrid').dxDataGrid('instance')._render();
            $('#accountsearchgrid').dxDataGrid('instance').pageIndex(0);


            //var isAccSearch = $("#IsAccountSearch").val();
            var ids = $("#AccountSearchSelectedIds").val();
            if (ids != null && ids.trim() != "") {
                var gridInstance = $('#accountsearchgrid').dxDataGrid('instance');
                var gridData = $('#accountsearchgrid').dxDataGrid('getDataSource');
                var dt = gridData.store()._array;
                var list = ids.split('|');
                var keyArray = [];
                for (var i = 0; i < list.length; i++) {
                    var filterednames = dt.filter(function (obj) {
                        return (obj.AccountID === list[i]);
                    });
                    if (filterednames.length == 1) {
                        keyArray.push(filterednames[0]);
                    }
                }

                gridInstance.selectRows(keyArray);
            }
        },
        error: function (err) {
        }
    });

}

function btnSortSelectedClick() {
    try {
        var dt = $('#accountsearchgrid').dxDataGrid('getDataSource');
        var gridData = dt.store()._array;
        var selectedAccount = $('#accountsearchgrid').dxDataGrid('instance').getSelectedRowKeys();
        var accList = [];
        for (var k = 0; k < gridData.length; k++) {

            var dataObj = selectedAccount.filter(function (obj) {
                return (obj.AccountID === gridData[k].AccountID);
            });

            if (dataObj.length > 0) {
                gridData[k].IsSelected = 'true';
            }
            else {
                gridData[k].IsSelected = 'false';
            }
        }

        $('#accountsearchgrid').dxDataGrid('instance').option('dataSource', gridData);
        $('#accountsearchgrid').dxDataGrid('instance')._render();
        $('#accountsearchgrid').dxDataGrid('instance').pageIndex(0);
        $('#accountsearchgrid').dxDataGrid('instance').selectRows(selectedAccount);
        var dataGridInstance = $('#accountsearchgrid').dxDataGrid('instance');
        dataGridInstance.beginUpdate();
        glblSortOrder = glblSortOrder == "asc" ? "desc" : "asc";
        dataGridInstance.columnOption('IsSelected', 'sortOrder', glblSortOrder);
        dataGridInstance.endUpdate();
    } catch (err) {

    }
}