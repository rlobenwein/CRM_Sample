

function OpenModal(url, modalId) {
    $(modalId).load(url, function () {
        $(modalId).modal();
    })
};
function UpdateFinalPrice(basePrice, discount) {
    if (discount > 1) { discount /= 100 };
    return basePrice * (1 - discount);
};
function PlannedActionsFilter() {
    var erpUserId = sessionStorage.erpUserIdPlanned;
    var actionStatus = sessionStorage.actionStatusPlanned;
    var DateStart = sessionStorage.DateStartPlanned;
    var DateEnd = sessionStorage.DateEndPlanned;
    var searchString = sessionStorage.searchString;
    var currentFilter = sessionStorage.currentFilter;

    if (DateStart == "") {
        DateStart = "2016-11-01";
    }
    if (DateEnd == "") {
        DateEnd = "2199-12-31";
    }

    var url = "PlannedActions/FilteredIndex?erpUserId=" + erpUserId +
        "&status=" + actionStatus +
        "&DateStart=" + DateStart +
        "&DateEnd=" + DateEnd +
        "&searchString=" + searchString +
        "&currentFilter=" + currentFilter;

    console.log('planned actions filter url', url);

    $.ajax({
        type: "Get",
        url: url,
        success: function (data) {
            $("#actions-container").html("");
            $("#actions-container").html(data);
        },
        error: function (response) {
            console.log('error: ', response.responseText);
        }
    });
};
function CompaniesFilter() {
    var searchString = sessionStorage.companiesSearchString;
    var currentFilter = sessionStorage.companiesCurrentFilter;

    var url = "Companies/Index?&searchString=" + searchString + "&currentFilter=" + currentFilter;

    $.ajax({
        type: "Get",
        url: url,
        success: function (data) {
            $("#actions-container").html("");
            $("#actions-container").html(data);
            console.log('success: ', searchString);
        },
        error: function (response) {
            console.log('error: ', response.responseText);
        }
    });
};
function DoneActionsFilter() {
    var erpUserId = sessionStorage.erpUserIdDone;
    var quantity = sessionStorage.quantityDone;
    var actionStatus = sessionStorage.actionStatusDone;
    var DateStart = sessionStorage.DateStartDone;
    var DateEnd = sessionStorage.DateEndDone;
    var searchString = sessionStorage.searchString;
    var currentFilter = sessionStorage.currentFilter;

    if (DateStart == "" || DateStart == undefined) {
        DateStart = "2016-11-01";
    }
    if (DateEnd == "" || DateEnd == undefined) {
        DateEnd = "2199-12-31";
    }
    if (quantity == '' || quantity == null) {
        quantity = 50;
    }

    var url = "DoneActions/FilteredIndex?erpUserId=" + erpUserId +
        "&status=" + actionStatus +
        "&DateStart=" + DateStart +
        "&DateEnd=" + DateEnd +
        "&strQuantity=" + quantity;

    $.ajax({
        type: "Get",
        url: url,
        success: function (data) {
            $("#actions-container").html("");
            $("#actions-container").html(data);
        },
        error: function (response) {
            console.log('error done: ', response.responseText);
        }
    });
};
function GetCountriesList(url) {
    url += 'GetCountriesList';

    $.getJSON(url, function (data) {
        var items = '';
        $('#Country').empty();
        $.each(data, function (i, country) {
            items += "<option value='" + country.value + "'>" + country.text + "</option>";
        });
        $('#Country').html(items);
    });
};
function GetStatesList(url, countryId) {
    url += 'GetStatesList?countryId=' + countryId;

    $.getJSON(url, { CountryId: $('#Country').val() }, function (data) {
        var items = '';
        $('#State').empty();
        $('#City').empty();
        var cityOption = "<option value=''>Selecione um estado</option>";
        $.each(data, function (i, state) {
            items += "<option value='" + state.value + "'>" + state.text + "</option>";
        });
        $('#State').html(items);
        $('#City').html(cityOption);
    });
};
function GetCitiesList(url, stateId) {
    url += 'GetCitiesList?stateId=' + stateId;

    $.getJSON(url, { StateId: $('#State').val() }, function (data) {
        var items = '';
        $('#City').empty();
        $.each(data, function (i, city) {
            items += "<option value='" + city.value + "'>" + city.text + "</option>";
        });
        $('#City').html(items);
    });
};
function SavePlannedFilterParams(erpUserId, actionStatus, quantity, DateStart, DateEnd, searchString, currentFilter) {
    sessionStorage.setItem('erpUserIdPlanned', erpUserId);
    sessionStorage.setItem('actionStatusPlanned', actionStatus);
    sessionStorage.setItem('quantityPlanned', quantity);
    sessionStorage.setItem('DateStartPlanned', DateStart);
    sessionStorage.setItem('DateEndPlanned', DateEnd);
    sessionStorage.setItem('searchString', searchString);
    sessionStorage.setItem('currentFilter', currentFilter);
};
function SaveCompaniesFilterParams(searchString, currentFilter) {
    sessionStorage.setItem('companiesSearchString', searchString);
    sessionStorage.setItem('companiesCurrentFilter', currentFilter);
};
function SaveDoneFilterParams(erpUserId, actionStatus, quantity, DateStart, DateEnd, searchString, currentFilter) {
    sessionStorage.setItem('erpUserIdDone', erpUserId);
    sessionStorage.setItem('actionStatusDone', actionStatus);
    sessionStorage.setItem('quantityDone', quantity);
    sessionStorage.setItem('DateStartDone', DateStart);
    sessionStorage.setItem('DateEndDone', DateEnd);
    sessionStorage.setItem('searchString', searchString);
    sessionStorage.setItem('currentFilter', currentFilter);
};
function RestorePlannedFilterFromStorage() {

    $('#DateStart').val(sessionStorage.DateStartPlanned ?? "");
    $('#DateEnd').val(sessionStorage.DateEndPlanned ?? "");
    $("#ErpUserSelect").val(sessionStorage.erpUserIdPlanned ?? 0);
    $('#actionStatus').val(sessionStorage.actionStatusPlanned ?? "Todas");
    $('#searchString').val(sessionStorage.searchString ?? "");
};
function RestoreDoneFilterFromStorage() {
    $('#DateStart').val(sessionStorage.DateStartDone ?? "");
    $('#DateEnd').val(sessionStorage.DateEndDone ?? "");
    $("#ErpUserSelect").val(sessionStorage.erpUserIdDone ?? 0);
    $('#actionStatus').val(sessionStorage.actionStatusDone ?? 'Todas');
    $('#quantity').val(sessionStorage.quantityDone = "null" ? 50 : sessionStorage.quantityDone);

};