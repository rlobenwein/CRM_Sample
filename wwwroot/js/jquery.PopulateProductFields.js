function EnableFieldsByCategory() {
    var isSoftware = $("#CategoryId option:selected").text() == "Software";
    $('#LicenseTimeId').prop("disabled", !isSoftware);
    $('#LicenseTimeQuantity').prop("disabled", !isSoftware);
    $('#CommercialLicense').prop("disabled", !isSoftware);
    $('#LicenseNetworkTypeId').prop("disabled", !isSoftware);
    $('#NetworkSeats').prop("disabled", !isSoftware);
    $('#Cores').prop("disabled", !isSoftware);
    $('#Tasks').prop("disabled", !isSoftware);
    $('#Workbench').prop("disabled", !isSoftware);
}
function EnableFieldsByProduct() {
    var softwareName = $("#ProductId option:selected").text();
    switch (softwareName) {
        case "QForm":
            $('#Cores').prop("disabled", false).val(6);
            $('#Tasks').prop("disabled", false).val(1);
            $('#Workbench').prop("disabled", false).val(0);
            $('#LicenseTimeId').val(1);
            $('#LicenseTimeQuantity').val(1);
            $('#LicenseNetworkTypeId').val(1);
            $('#NetworkSeats').val(1);
            //$('#SubProductId').val(1);
            break;
        case "JMatPro":
            $('#Cores').prop("disabled", true).val(0);
            $('#Tasks').prop("disabled", true).val(0);
            $('#Workbench').prop("disabled", true).val(0);
            $('#LicenseTimeId').val(1);
            $('#LicenseTimeQuantity').val(1);
            $('#LicenseNetworkTypeId').val(2);
            $('#NetworkSeats').val(1);
            //$('#SubProductId').val(6);
            break;
        case "OmniCAD":
            $('#Cores').prop("disabled", true).val(0);
            $('#Tasks').prop("disabled", true).val(0);
            $('#Workbench').prop("disabled", true).val(0);
            $('#LicenseTimeId').val(1);
            $('#LicenseTimeQuantity').val(1);
            $('#LicenseNetworkTypeId').val(1);
            $('#NetworkSeats').val(1);
            //$('#SubProductId').val(7);
            break;
        case "PamStamp":
            $('#Cores').prop("disabled", true).val(0);
            $('#Tasks').prop("disabled", true).val(0);
            $('#Workbench').prop("disabled", true).val(0);
            $('#LicenseTimeId').val(1);
            $('#LicenseTimeQuantity').val(1);
            $('#LicenseNetworkTypeId').val(1);
            $('#NetworkSeats').val(1);
            //$('#SubProductId').val(8);
            break;
        case "Dante":
            $('#Cores').prop("disabled", true).val(0);
            $('#Tasks').prop("disabled", true).val(0);
            $('#Workbench').prop("disabled", true).val(0);
            $('#LicenseTimeId').val(1);
            $('#LicenseTimeQuantity').val(1);
            $('#LicenseNetworkTypeId').val(1);
            $('#NetworkSeats').val(1);
            //$('#SubProductId').val(9);
            break;
        default:
    }
}

function GetProductList(categoryId, url) {
    url += 'GetProductList';
    $.getJSON(url, { CategoryId: $(categoryId).val() }, function (data) {
        var items = '';
        $('#ProductId').empty();
        $.each(data, function (i, product) {
            items += "<option value='" + product.value + "'>" + product.text + "</option>";
        });
        $('#ProductId').html(items);
        EnableFieldsByCategory();
    });
};
function GetSubProductList(productId, url) {
    url += 'GetSubProductList';

    $.getJSON(url, { ProductId: $(productId).val() }, function (data) {
        var items = '';
        $('#SubProductId').empty();
        $.each(data, function (i, subproduct) {
            items += "<option value='" + subproduct.value + "'>" + subproduct.text + "</option>";
        });
        $('#SubProductId').html(items);
        EnableFieldsByProduct();
    });
};
function GetNetworkTypeList(url) {
    url += 'GetNetworkTypeList';
    var selectionText = $("#ProductId option:selected").text();

    $.getJSON(url, { ProductName: selectionText }, function (data) {
        var items = '';
        $('#LicenseNetworkTypeId').empty();
        $.each(data, function (i, NetworkType) {
            items += "<option value='" + NetworkType.value + "'>" + NetworkType.text + "</option>";
        });
        $('#LicenseNetworkTypeId').html(items);
    });
};
function GetCommercialLicenseList(url) {
    url += 'GetCommercialLicenseList';
    var selectionText = $("#ProductId option:selected").text();

    $.getJSON(url, { ProductName: selectionText }, function (data) {
        var items = '';
        $('#CommercialLicense').empty();
        $.each(data, function (i, CommercialType) {
            items += "<option value='" + CommercialType.value + "'>" + CommercialType.text + "</option>";
        });
        $('#CommercialLicense').html(items);
    });
};
function GetLicenseTimeList(url) {
    url += 'GetLicenseTimeList';
    var selectionText = $("#ProductId option:selected").text();

    $.getJSON(url, { ProductName: selectionText }, function (data) {
        var items = '';
        $('#LicenseTimeId').empty();
        $.each(data, function (i, LicenseTime) {
            items += "<option value='" + LicenseTime.value + "'>" + LicenseTime.text + "</option>";
        });
        $('#LicenseTimeId').html(items);
    });
};
function GetOpportunityProductPrice(url) {
    url += 'GetOpportunityProductPrice';
    var opportunityProductId = $("#OpportunityProductsId option:selected").val();

    $.getJSON(url, { opportunityProductId: opportunityProductId }, function (data) {
        $('#BasePrice').val(data);
    });

};
function GetSubproductPrice(url) {
    url += 'GetSubproductPrice';
    var subproductId = $("#SubproductId option:selected").val();
    $.getJSON(url, { subproductId: subproductId }, function (data) {
        $('#BasePrice').val(data);
    });

} function GetOptionalPrice(url) {
    url += 'GetOptionalPrice';
    var optionalId = $(".OptionalId option:selected").val();
    $.getJSON(url, { optionalId: optionalId }, function (data) {
        $('.BasePrice').val(data);
    });
}