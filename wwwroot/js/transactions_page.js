function TransactionsFilter(year, month) {
    var isExpense = sessionStorage.isExpense;
    var isIncurred = sessionStorage.isIncurred;
    var category = sessionStorage.category;
    var subcategory = sessionStorage.subcategory;
    var type = sessionStorage.type;
    var paymentAccount = sessionStorage.paymentAccount;
    var dateStart = sessionStorage.dateStart;
    var dateEnd = sessionStorage.dateEnd;
    var searchString = sessionStorage.searchString;
    var currentFilter = sessionStorage.currentFilter;
    var sortOrder = sessionStorage.sortOrder;

    if (dateStart == "") {
        dateStart = "2016-01-01";
    }
    if (dateEnd == "") {
        dateEnd = "2199-12-31";
    }
    if (year != null && month != null) {
        var url = "FilteredDetails?year=" + year +
            "&month=" + month +
            "&isExpenseStr=" + isExpense +
            "&isIncurredStr=" + isIncurred +
            "&category=" + category +
            "&subcategory=" + subcategory +
            "&type=" + type +
            "&paymentAccount=" + paymentAccount +
            "&dateStart=" + dateStart +
            "&dateEnd=" + dateEnd +
            "&searchString=" + searchString +
            "&sortOrder=" + sortOrder +
            "&currentFilter=" + currentFilter
            ;
    } else {
        var url = "Transactions/FilteredIndex?isExpenseStr=" + isExpense +
            "&isIncurredStr=" + isIncurred +
            "&category=" + category +
            "&subcategory=" + subcategory +
            "&type=" + type +
            "&paymentAccount=" + paymentAccount +
            "&dateStart=" + dateStart +
            "&dateEnd=" + dateEnd +
            "&searchString=" + searchString +
            "&sortOrder=" + sortOrder +
            "&currentFilter=" + currentFilter
            ;
    }

    $.ajax({
        type: "Get",
        url: url,
        success: function (data) {
            $("#transactions-container").html("");
            $("#transactions-container").html(data);
        },
        error: function (response) {
            console.log('error: ', response.responseText);
        }
    });
};
function GetCategoriesList(url, elementId, isExpense) {
    var categoryElementId = '#Category-' + GetElementIndex(elementId);
    url += 'GetCategoriesList?isExpense=' + isExpense;
    var elementValue = $(categoryElementId).val();
    $.getJSON(url, { isExpense: $(elementId).val() }, function (data) {
        var items = "<option value=0>Todos</option>";
        $(categoryElementId).empty();
        $.each(data, function (i, category) {
            if (category.value === elementValue) {
                items += "<option selected=\"selected\" value='" + category.value + "'>" + category.text + "</option>";
            } else {
                items += "<option value='" + category.value + "'>" + category.text + "</option>";
            }
        });
        $(categoryElementId).html(items);
    });
};
function GetSubcategoriesList(url, categoryId, subcategoryElementId) {
    url += 'GetSubcategoriesList?categoryId=' + categoryId;
    subcategoryElementId = '#' + subcategoryElementId;
    var elementValue = $(subcategoryElementId).val();

    $.getJSON(url, { categoryId: categoryId }, function (data) {
        var items = "<option value=0>Todos</option>";
        $(subcategoryElementId).empty();
        $.each(data, function (i, subcategory) {
            if (subcategory.value === elementValue) {
                items += "<option selected=\"selected\" value='" + subcategory.value + "'>" + subcategory.text + "</option>";

            } else {
                items += "<option value='" + subcategory.value + "'>" + subcategory.text + "</option>";
            }
        });
        $(subcategoryElementId).html(items);
    });
};

function GetTypesList(url, subcategoryId, typeElementId) {
    url += 'GetTypesList?subcategoryId=' + subcategoryId;
    var elementValue = $(typeElementId).val();

    $.getJSON(url, { subcategoryId: subcategoryId }, function (data) {
        var items = "<option value=0>Todos</option>";
        $(typeElementId).empty();
        $.each(data, function (i, type) {

            if (type.value === elementValue) {
                items += "<option selected=\"selected\" value='" + type.value + "'>" + type.text + "</option>";
            } else {
                items += "<option value='" + type.value + "'>" + type.text + "</option>";
            }
        });
        $(typeElementId).html(items);
    });
};

function ClearTypesList(typeElementId) {
    var items = "<option value=0>Todos</option>";
    $(typeElementId).empty();
    $(typeElementId).html(items);
};

function GetAvailableMonths(url, year) {
    $.getJSON(url, { year: year }, function (data) {
        var items = '';
        $('#monthsAvailable').empty();
        $.each(data, function (i, month) {
            items += "<option value='" + month.text + "'>" + month.text + "</option>";
        });
        $('#monthsAvailable').html(items);
    });
};

function GetElementIndex(elementId) {
    var parts = elementId.split("-");
    return parts[1];
}
function SaveTransactionsFilterParams(isExpense, isIncurred, category, subcategory, type, paymentAccount, dateStart, dateEnd, searchString, currentFilter, sortOrder, pageNumber, pageSize) {
    sessionStorage.setItem('isExpense', isExpense);
    sessionStorage.setItem('isIncurred', isIncurred);
    sessionStorage.setItem('category', category);
    sessionStorage.setItem('subcategory', subcategory);
    sessionStorage.setItem('type', type);
    sessionStorage.setItem('paymentAccount', paymentAccount);
    sessionStorage.setItem('dateStart', dateStart);
    sessionStorage.setItem('dateEnd', dateEnd);
    sessionStorage.setItem('searchString', searchString);
    sessionStorage.setItem('currentFilter', currentFilter);
    sessionStorage.setItem('sortOrder', sortOrder);
    sessionStorage.setItem('pageNumber', pageNumber);
    sessionStorage.setItem('pageSize', pageSize);
};
function RestoreTransactionsFilterFromStorage() {
    $('#isExpense').val(sessionStorage.isExpense = null || 'undefined' ? 0 : sessionStorage.isExpense);
    $('#isIncurred').val(sessionStorage.isIncurred = null || 'undefined' ? true : sessionStorage.isIncurred);
    $('#category').val(sessionStorage.category = null || 'undefined' ? 0 : sessionStorage.category);
    $('#subcategory').val(sessionStorage.subcategory = null || 'undefined' ? 0 : sessionStorage.subcategory);
    $('#type').val(sessionStorage.type = null || 'undefined' ? 0 : sessionStorage.type);
    $('#paymentAccount').val(sessionStorage.paymentAccount = null || 'undefined' ? 0 : sessionStorage.paymentAccount);
    $('#dateStart').val(sessionStorage.dateStart);
    $('#dateEnd').val(sessionStorage.dateEnd);
    $('#searchString').val(sessionStorage.searchString);
    $('#currentFilter').val(sessionStorage.currentFilter);
    $('#pageNumber').val(sessionStorage.pageNumber = null || 'undefined' ? 1 : sessionStorage.pageNumber);
    $('#pageSize').val(sessionStorage.pageSize = null || 'undefined' ? 50 : sessionStorage.pageSize);
};