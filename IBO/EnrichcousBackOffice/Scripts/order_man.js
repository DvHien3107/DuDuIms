/// <reference path="noty-cfg.js" />

var partnerSelected = "";
//search merchant
function search_merchant(type) {
    var _name = $("input[name='merchant_search']").val();

    if (type == "all") {
        $("#searchall_loading").show();
        _name = "";
        $("input[name='merchant_search']").val('');
    }
    else {
        $("#search_loading").show();
        if (_name == "" || _name == null) {
            var MsgError = $.parseJSON('{"text":"Please enter a keyword to search", "layout":"topRight", "type":"warning"}');
            noty(MsgError);
            $("#search_loading").hide();
            $("#list_merchant").empty();
            $("#div_list_merchant").hide('500');
            return;
        }
        if (_name.length < 3) {
            var MsgError = $.parseJSON('{"text":"Please enter a keyword more than 3 character to search", "layout":"topRight", "type":"warning"}');
            noty(MsgError);
            $("#search_loading").hide();
            $("#list_merchant").empty();
            $("#div_list_merchant").hide('500');
            return;
        }
    }

    $.ajax({
        method: "POST",
        url: "/order/SearchMerchant",
        data: { NameSearch: _name },
        dataType: "json"
    })
        .done(function (data) {
            //data: object[] {[true/false], [list_merchant.Count()/msg_error], [list_merchant]}

            if (data[0] == true) {
                if (data[1] > 0) {
                    let MsgSuccess = $.parseJSON('{"text":"Found ' + data[1] + ' merchants", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);

                    $("#list_merchant").empty();
                    var _option = "";
                    for (var i = 0; i < data[2].length; i++) {
                        var contact_name = data[2][i].OwnerName;
                        if (contact_name == null || contact_name == "") {
                            contact_name = data[2][i].ContactName;
                        }

                        var contact_phone = data[2][i].OwnerMobile;
                        if (contact_phone == null || contact_phone == "") {
                            contact_phone = data[2][i].BusinessPhone;
                        }

                        var zipCode = data[2][i].BusinessZipCode;
                        if (zipCode == null || zipCode == 'null') { zipCode = ""; }

                        _option = _option + "<tr><td><b>" + data[2][i].StoreCode + "</b></td>" +
                            "<td><span><b style='color:var(--main-color-1)'>" + (data[2][i].BusinessName || "") + "</b></span><br/>" +
                            "<span>" + (data[2][i].SalonAddress1 || "") + ", " + (data[2][i].SalonCity || "") + ", " + (data[2][i].SalonState || "") + " " + (zipCode || "") + ", " + (data[2][i].BusinessCountry || "") + "</span></td>" +
                            "<td><i>Name: </i>" + (contact_name || "") + "<br/><i>Phone: </i>" + (contact_phone || "") + "<br/><i>Email: </i>" + (data[2][i].Email || "") + "</td>" +
                            "<td><button type='button' class='btn btn-sm btn-success' onclick='select_merchant(\"" + (data[2][i].CustomerCode || "") + "\")'>Select <i class='fa fa-thumbs-o-up'></i></button></td></tr>";
                    }

                    $("#list_merchant").append(_option);
                    $("#div_list_merchant").show('500');
                }
                else {
                    let MsgSuccess = $.parseJSON('{"text":"Found ' + data[1] + ' merchant", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);
                    $("#list_merchant").empty();
                    $("#div_list_merchant").hide('500');
                }
            }
            else {
                var MsgError = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(MsgError);
            }
        })
        .fail(function () {
            alert("Oops! Something went wrong. Merchant can't load");
        }).always(function () {
            $("#search_loading").hide();
            $("#searchall_loading").hide();
        });

}

//select merchant
function select_merchant(_code) {
    overlayOn();
    $.ajax({
        method: "POST",
        url: "/order/SelectMerchant",
        data: { cus_code: _code },
        dataType: "json"
    })
        .done(function (data) {
            //data: object[] {[true/false], [merchant/msg_error]}
            overlayOff();
            if (data[0] == true) {
                $("#merchant_id_hd").val(_code);
                $("#list_merchant").empty();
                $("#div_list_merchant").hide('500');

                var zipCode = data[2].BusinessZipCode;
                if (zipCode == null || zipCode == "null") { zipCode = ""; };
                var _merchant_address = (data[2].SalonAddress1 || "---") + ", " + (data[2].SalonCity || "---") + ", " + (data[2].SalonState || "---") + ", " + (zipCode || "---") + ", " + (data[2].BusinessCountry || "---");

                $("#m_name").html(data[2].BusinessName);
                $("#m_address").html(_merchant_address);
                $("#c_email").html(data[2].SalonEmail);

                if (data[2].OwnerName == null || data[2].OwnerName == "") {
                    $("#c_name").html(data[2].ContactName);
                }
                else {
                    $("#c_name").html(data[2].OwnerName);
                }

                if (data[2].OwnerMobile == null || data[2].OwnerMobile == "") {
                    $("#c_phone").html(data[2].BusinessPhone);
                }
                else {
                    $("#c_phone").html(data[2].OwnerMobile);
                }

                //auto complete shipping address
                $("input[name='sh_street']").val(data[2].BusinessAddressStreet);
                $("input[name='sh_city']").val(data[2].BusinessCity);
                $("input[name='sh_state']").val(data[2].BusinessState);
                $("input[name='sh_zip']").val(data[2].BusinessZipCode);
                $("input[name='sh_country']").val(data[2].BusinessCountry);
                $("#SalesMemberNumber").val(data[5]);
                $("#SalesMemberNumber").change();
                partnerSelected = data[6] ?? "";
                $("#div_merchant_info").show('500');
                $("#same_address").trigger("click");
                var MsgSuccess = $.parseJSON('{"text":"Select merchant success!", "layout":"topRight", "type":"success"}');
                noty(MsgSuccess);
                if (!data[1]) {
                    $("#exist_estimate").val(data[3]);
                    $('#dialog').dialog("open");
                    //document.location.href = "/order/save/" + data[2] + "?url_back=/order/estimates";
                    //window.location.href = window.location.hostname + "/order/save/"+data[2]+"?url_back=/order/estimates";
                }
                $("#render-license-is-active").html(data[4]);
                $('input[name="selected_prd"][type="radio"]:checked').change();

                //update_selected(null, 'all', null, null, null, null, null, true);
                //$(".check").prop("checked", false);
            }
            else {
                if (data[2] == true) {
                    var MsgError = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"warning"}');
                    noty(MsgError);
                    openPopupNewOrEditMerchant(data[3]);
                }
                else
                {
                    $("#div_merchant_info").hide('500');
                    var MsgError = $.parseJSON('{"text":"Select fail! ' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(MsgError);
                }
              
            }
        })
        .fail(function () {
            overlayOff();
            alert("Oops! Something went wrong");
        });
}

////select device
//function select_device(_flag, _key, _pro_code) {
//    $("#select_pos_dv_loading").show();

//    //[flag] = ["add"/"update"]
//    if (_flag == "add") {
//        var product_code = _pro_code;
//        if (product_code == "" || product_code == null) {
//            $("#select_pos_dv_loading").hide();
//            $("#div_POS_Device_Hardware").hide("alow");
//        }
//        else {
//            $.ajax({
//                method: "POST",
//                url: "/order/GetDeviceInfo",
//                data: {
//                    flag: _flag,
//                    ProductCode: product_code,
//                    Key: 0
//                },
//                dataType: "html"
//            })
//                .done(function (data) {
//                    //data: _POSDeviceHardwarePartial
//                    $("#select_pos_dv_loading").hide();
//                    $("#div_POS_Device_Hardware").html(data);

//                    ///$("input[name='hd_product_picture']").val(picture_url);
//                    $("input[name='hd_add_update']").val("add");
//                    $("#btn_name").html(" Add");
//                    $("#p_model").trigger("change");
//                    $("#div_POS_Device_Hardware").show("alow");
//                })
//                .fail(function () {
//                    $("#select_pos_dv_loading").hide();
//                    $("#pos_device").val("");
//                    alert("Oops! Something went wrong");
//                })
//        }
//    }
//    else {

//        $.ajax({
//            method: "POST",
//            url: "/order/GetDeviceInfo",
//            data: {
//                flag: _flag,
//                ProductCode: _pro_code,
//                Key: _key
//            },
//            dataType: "html"
//        })
//            .done(function (data) {
//                //data: _POSDeviceHardwarePartial
//                $("#select_pos_dv_loading").hide();
//                $("#div_POS_Device_Hardware").html(data);

//                $("input[name='hd_add_update']").val("update");
//                $("#btn_name").html(" Update");
//                $("#pos_device").val(_pro_code);
//                $("#pos_device").prop("disabled", true);

//                $("#_option_select").val("Item");
//                select_option();
//                $("#_option_select").prop("disabled", true);

//                $("select[name='vendor']").prop("disabled", true);
//                $("#p_feature").select2({ disabled: true });
//                $("#_btn_close").html(" Cancel");
//                $("#div_POS_Device_Hardware").show("alow");
//            })
//            .fail(function () {
//                $("#select_pos_dv_loading").hide();
//                $("#pos_device").val("");
//                alert("Oops! Something went wrong");
//            })
//    }
//}

////select vendor
//function select_vendor() {
//    var _product_code = $("input[name='hd_product_code']").val();
//    var _vendor_id = $("select[name='vendor']").val();

//    $.ajax({
//        method: "POST",
//        url: "/order/SelectVendor",
//        data: {
//            _ProductCode: _product_code,
//            _VendorId: _vendor_id
//        },
//        dataType: "json"
//    })
//        .done(function (data) {
//            //data: object[] {[true/false], [ferture_default/msg_error], [list_feature_available], [list_feature_not_available], [_inventory], [price], [_ModelCode], [original_number]}
//            if (data[0] == true) {
//                $("#p_feature").empty();
//                var _option = "";
//                var _qty_str = "item";

//                if (data[1].FeatureName != null) {
//                    if (data[1].Quantity_Total > 1) {
//                        _qty_str = "items";
//                    }
//                    _option += '<option value="">--Default-- ' + data[1].Quantity_Total + ' ' + _qty_str + '</option>';
//                }


//                _option += '<optgroup label="Available">';
//                if (data[2] != "" && data[2] != null) {
//                    for (var i = 0; i < data[2].length; i++) {
//                        if (data[2][i].Quantity_Total > 1) {
//                            _qty_str = "items";
//                        }
//                        _option += '<option value="' + data[2][i].FeatureName + '">- ' + data[2][i].FeatureName + ' - ' + data[2][i].Quantity_Total + ' ' + _qty_str + '</option>';
//                    }
//                }
//                else {
//                    _option += '<option disabled>- No feature found</option>';
//                }
//                _option += '</optgroup>';

//                _option += '<optgroup label="Not Available">';
//                if (data[3] != "" && data[3] != null) {
//                    for (var i = 0; i < data[3].length; i++) {
//                        if (data[3][i].Quantity_Total > 1) {
//                            _qty_str = "items";
//                        }
//                        _option += '<option value="' + data[3][i].FeatureName + '">- ' + data[3][i].FeatureName + ' - ' + data[3][i].Quantity_Total + ' ' + _qty_str + '</option>';
//                    }
//                }
//                else {
//                    _option += '<option disabled>- No feature found</option>';
//                }
//                _option += '</optgroup>';


//                $("#p_feature").append(_option);
//                $("input[name='p_inventory']").val(data[4]);
//                $("input[name='p_price']").val(data[5]);
//                $("input[name='hd_model_code']").val(data[6]);
//                $("input[name='hd_original_number']").val(data[7]);
//            }
//            else {
//                var MsgError = $.parseJSON('{"text":"Fail! ' + data[1] + '", "layout":"topRight", "type":"error"}');
//                noty(MsgError);
//            }
//        })
//        .fail(function () {
//            alert("Oops! Something went wrong");
//        })
//}

//select feature
//function select_model(Code) {
//    //alert(_product_code + "|" + _feature);
//    $.ajax({
//        method: "POST",
//        url: "/order/SelectModel",
//        data: {
//            Code
//        },
//        dataType: "json"
//    })
//        .done(function (data) {
//            //data: object[] {[true/false], [_inventory/msg_error], [price], [_ModelCode], [original_number],[picture]}
//            if (data[0] == true) {
//                $("#p_picture").attr("src", data[5])
//                $("input[name='p_inventory']").val(data[1]);
//                $("input[name='p_price']").val(data[2]);
//                $("input[name='hd_model_code']").val(data[3]);
//                $("input[name='hd_original_number']").val(data[4]);
//                if (data[4]) {
//                    $("#color_show").show();
//                    $("#color_show").find(".color").html(data[4]);
//                } else {
//                    $("#color_show").hide();
//                }
//            }
//            else {
//                var MsgError = $.parseJSON('{"text":"Fail! ' + data[1] + '", "layout":"topRight", "type":"error"}');
//                noty(MsgError);
//            }
//        })
//        .fail(function () {
//            alert("Oops! Something went wrong");
//        })
//}

//add device
function add_device(ModelCode, Quantity, name, remain) {
    if (remain < Quantity) {
        if (!confirm("Remaining quantity is not enough. Are you sure to add " + Quantity + " " + name + "?")) {
            undo_select(ModelCode);
            return false;
        }
    }
    if (Quantity > 0) {
        update_selected(ModelCode, "device", Quantity)
    }
    else {
        $("#add_dv_loading").hide();
        var MsgError = $.parseJSON('{"text":"Quantity must be > 0", "layout":"topRight", "type":"error"}');
        noty(MsgError);
    }
}

//delete device
function delete_device(model_code) {

    update_selected(model_code, "device", 0)

}


//call when add or delete device fail
function undo_select(code) {
    var cb = $("#select_" + code);
    if (!cb.next(".device_check").is(":checked")) {
        cb.addClass("selected")
        cb.next(".device_check").prop("checked", true);
    } else {
        cb.removeClass("selected");
        cb.next(".device_check").prop("checked", false);
    }
}

/**
 * Change bundle qty on order 
 * @param {any} id BundleId
 * @param {any} qty Bundle qty
 */
function change_qty_package(id, add) {
    var new_qty = parseInt($("#qty_bundle_" + id).val()) + add;
    if (new_qty < 1) {
        new_qty = 1;
    }
    if (new_qty > 999999) {
        new_qty = 999999;
    }
    add_bundle(id, new_qty);
}
function change_qty_device(code, add) {
    var new_qty = parseInt($("#qty_model_" + code).val()) + add;
    if (new_qty < 1) {
        new_qty = 1;
    }
    if (new_qty > 999999) {
        new_qty = 999999;
    }
    add_device(code, new_qty);
}

//select service
function select_service(_flag, _service_code) {
    $("#service_loading").show();

    if (_flag == "add") {
        _service_code = $("#service_code").val();
    }

    if (_service_code == "" || _service_code == null) {
        $("#service_loading").hide();
        $("#div_mango_service").hide("alow");
    }
    else {
        $.ajax({
            method: "POST",
            url: "/order/GetServiceInfo",
            data: {
                flag: _flag,
                ServiceCode: _service_code
            },
            dataType: "json"
        })
            .done(function (data) {
                //data: object[] {[true/false], [_service/msg_error]}
                $("#service_loading").hide();
                if (data[0] == true) {

                    if (_flag == "add") {
                        $("input[name='s_setup_fee']").val(Number(data[1].SetupFee));
                        $("input[name='s_price']").val(data[1].SalesPrice);
                        $("select[name='service_plan']").val("Unlimit");

                        //var today = new Date();
                        //var dd = String(today.getDate()).padStart(2, '0');
                        //var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
                        //var yyyy = today.getFullYear();
                        //today = mm + '/' + dd + '/' + yyyy;
                        //$("#s_startdate").val(today);

                        $("#_btn_add_service").html(" Add");
                        $("#_btn_close_add_service").html(" Close");
                    }
                    else {
                        $("input[name='s_setup_fee']").val(toMoney(Number(data[1].SetupFee)));
                        $("input[name='s_price']").val(toMoney(Number(data[1].MonthlyFee)));
                        $("select[name='service_plan']").val(data[1].ServicePlan);
                        $("#s_startdate").val(data[1].StartDate);

                        $("#service_code option[value='" + data[1].ServiceCode + "']").attr("disabled", false);//enable option
                        $("#service_code").val(data[1].ServiceCode);
                        $("#service_code").prop("disabled", true);

                        $("#_btn_add_service").html(" Update");
                        $("#_btn_close_add_service").html(" Cancel");
                    }

                    $("#div_mango_service").show("alow");
                }
                else {
                    var MsgError = $.parseJSON('{"text":"Error! ' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(MsgError);
                }
            })
            .fail(function () {
                $("#service_loading").hide();
                alert("Oops! Something went wrong");
            })
    }
}

//add service
function add_service() {
    $("#add_service_loading").show();
    var service_code = $("#service_code").val();
    var setup_fee = $("input[name='s_setup_fee']").val().replace(/,/g, '');
    var monthly_fee = $("input[name='s_price']").val().replace(/,/g, '');
    var service_plan = $("select[name='service_plan']").val();
    var start_date = $("#s_startdate").val();

    $.ajax({
        method: "POST",
        url: "/order/AddService",
        data: {
            _ServiceCode: service_code,
            _SetupFee: setup_fee,
            _MonthlyFee: monthly_fee,
            _ServicePlan: service_plan,
            _StartDate: start_date
        },
        dataType: "json"
    })
        .done(function (data) {
            //data: object[] {[true/false], [_service/msg_error], total_money_order, list_product, replace}
            $("#add_service_loading").hide();
            if (data[0] == true) {
                if (data[4] == true) {
                    $("#" + data[1].ServiceCode).remove();
                    var MsgSuccess = $.parseJSON('{"text":"Update service success", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);
                }
                else {
                    var MsgSuccess = $.parseJSON('{"text":"Add service success", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);
                }

                var new_div_service = '<div id="' + data[1].ServiceCode + '" style="background-color:whitesmoke; padding:5px; margin-bottom:5px">' +
                    '<div class="col-md-4" style="padding:0px 5px 0px 8px">' +
                    '<div style="margin-bottom:5px">' +
                    '<button type="button" class="btn btn-xs btn-warning" style="cursor:pointer" onclick="select_service(\'update\',\'' + data[1].ServiceCode + '\')"> <i class="glyphicon glyphicon-edit"></i></button>&nbsp&nbsp' +
                    '<button type="button" class="btn btn-xs btn-default" style="cursor:pointer" onclick="delete_service(\'' + data[1].ServiceCode + '\')"><i class="glyphicon glyphicon-trash"></i></button>' +
                    '</div>' +
                    '<img src="' + data[1].Picture + '" onerror="this.onerror=null;this.src=\'/Content/Img/noimage.png\'" class="img-thumbnail" style="max-height:120px" />' +
                    '</div>' +
                    '<div class="col-md-8">' +
                    '<span style="color:#258e4f"><b>' + data[1].ServiceName.toUpperCase() + '</b></span><br />' +
                    '<span><b>Type: </b>' + data[1].Type + '</span><br />' +
                    '<span><b>Quantity: </b>' + data[1].Quantity + '</span><br />' +
                    '<b>Setup fee: </b>' + numberToMoney_USD(data[1].SetupFee) + '</span><br />' +
                    '<b>Monthly fee: </b>' + numberToMoney_USD(data[1].MonthlyFee) + '/month</span><br />' +
                    '<span><b>Service plan: </b>' + data[1].ServicePlan + '</span><br />' +
                    '<span><b>Start date: </b>' + data[1].StartDate + '</span><br />' +
                    '<span><b>Total amount: </b>' + numberToMoney_USD(data[1].SetupFee + data[1].MonthlyFee) + '</span>' +
                    '</div>' +
                    '<div class="clearfix"></div></div>';

                $("#div_product").prepend(new_div_service);
                $("#service_code").val("");
                $("#service_code").prop("disabled", false);
                $("#service_code option[value='" + data[1].ServiceCode + "']").attr("disabled", true);//disable option
                $('#service_code option:contains("' + data[1].ServiceName + '")').text(data[1].ServiceName + " - Added");//change text option

                $("#div_mango_service").hide("alow");

                $("#sub_total").html(numberToMoney_USD(data[2].SubTotal));
                $("#grand_tottal").html(numberToMoney_USD(data[2].GrandTotal));
                $("#s_startdate").attr("disabled", true);
                draw_table(data[3]);
            }
            else {
                var MsgError = $.parseJSON('{"text":"Fail! ' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(MsgError);
            }
        })
        .fail(function () {
            $("#add_service_loading").hide();
            $("#service_code").val("");
            alert("Oops! Something went wrong");
        })
}

//delete device
function delete_service(_service_code) {
    $.ajax({
        method: "POST",
        url: "/order/DeleteService",
        data: { _ServiceCode: _service_code },
        dataType: "json"
    })
        .done(function (data) {
            //data: object[] {[true/false], [list_product/msg_error], total_money_order, service}
            if (data[0] == true) {
                $("#" + _service_code).remove();
                $("#div_mango_service").hide('alow');
                $("#service_code").val("");
                $("#service_code").prop("disabled", false);

                $("#service_code option[value='" + data[3].ServiceCode + "']").attr("disabled", false);
                $('#service_code option:contains(' + data[3].ServiceName + ' - Added)').text(data[3].ServiceName);

                $("#sub_total").html(numberToMoney_USD(data[2].SubTotal));
                $("#grand_tottal").html(numberToMoney_USD(data[2].GrandTotal));
                draw_table(data[1]);

                if (data[2].ServiceTotalAmount == 0) {
                    $("#s_startdate").attr("disabled", false);
                    var today = new Date();
                    var dd = String(today.getDate()).padStart(2, '0');
                    var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
                    var yyyy = today.getFullYear();
                    today = mm + '/' + dd + '/' + yyyy;
                    $("#s_startdate").val(today);
                }

                var MsgSuccess = $.parseJSON('{"text":"Delete service success", "layout":"topRight", "type":"success"}');
                noty(MsgSuccess);
            }
            else {
                var MsgError = $.parseJSON('{"text":"Delete fail! ' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(MsgError);
            }
        })
        .fail(function () {
            alert("Oops! Something went wrong");
        })
}

//Draw Table List_Product_Order
function draw_table(data) {
    $("#list_product_order tr").not(".grand_product").remove();
    var _content = "";
    var index = $("#list_product_order tr").length;
    for (var i = 0; i < data.length; i++) {
        _content += '<tr><td>' + (i + index) + '</td>';

        if (!data[i].BundleId) {
            var _properties = "";

            if (data[i].BundleName == null) {
                data[i].BundleName = "";
            }

            if (data[i].Picture == "" || data[i].Picture == null) {
                data[i].Picture = "/Upload/Img/no_image.jpg";
            }

            _content += '<td><div class="row"><div class="col-md-3"><img src="' + (data[i].Picture ?? "") + '" onerror="this.onerror=null;this.src=\'/Content/Img/noimage.png\'" class="img-thumbnail" style="height:75px" /></div>' +
                '<div class="col-md-9"><button type="button" class="btn btn-xs btn-default pull-right" style="cursor:pointer" onclick="if(confirm(\'Are you sure to unselect Model ' + data[i].ModelName.toUpperCase() + '?\')){$(\'#select_' + data[i].ModelCode + '\').trigger(\'click\')}">' +
                '<i class="glyphicon glyphicon-trash"></i>' +
                '</button>' +
                '<b style="color:dodgerblue">' + data[i].ProductName + '</b><i> (Model: ' + data[i].ModelName + ')</i><br />' +
                '<div style="width:50%">Type: <b><i>Device</i></b></div><div style="width:50%">Color: <i><b>' + (data[i].Feature || "N/A") + '</b></i></div><br />' +
                '</div></div></td>' +
                '<td><input type="number" id="qty_model_' + data[i].ModelCode.replace(/ /g, '_') + '" readonly value="' + data[i].Quantity + '" style="border:1px solid gray; padding:2px 5px; width:50px; background-color: transparent;float:left" />' +
                '<button type="button" style="float:left; margin-left:-1px" onclick="change_qty_device(\'' + data[i].ModelCode + '\', -1)"><i class="fa fa-minus"></i></button><button onclick="change_qty_device(\'' + data[i].ModelCode + '\', 1)" type="button" style="float:left; margin-left:-1px"><i class="fa fa-plus"></i></button></td>' +
                //'<td><b style="color:#258e4f">$0</b></td>' +
                '<td><b style="color:#258e4f" >' + numberToMoney_USD(data[i].Price) + '</b></td>' +
                '<td><b style="color:#258e4f" id="amount_model_' + data[i].ModelCode.replace(/ /g, "_") + '">' + numberToMoney_USD(data[i].Price * data[i].Quantity) + '</b></td>';
        }
        if (data[i].BundleId) {
            var list_model = "";
            for (var j in data[i].list_Bundle_Device) {
                var model = data[i].list_Bundle_Device[j];
                list_model += '<div class="container-fluid" style="border: 2px outset lightgray; border-radius:5px; background-color: #fff; margin-bottom: 5px">' +
                    '<b style="color:dodgerblue">' + model.ProductName + '</b><br /><span> Model: </span > <b>' + model.ModelName + '</b> <br /><div style="float:left; width:60%"><span>Color:</span> <b>' + (model.Color || "N/A") + '</b></div><div style="float:left; width:40%"><span>QTY:</span> <b class="model_qty_' + data[i].BundleId + '" data-qty="' + model.Quantity + '">' + (model.Quantity * data[i].Quantity) + '</b></div></div>';
            }

            if (data[i].BundleName == null) {
                data[i].BundleName = "";
            }

            _content += '<td><div id="bundle_' + data[i].BundleId + '" class="row"><div class="col-md-3"><img src="' + (data[i].Picture ?? "") + '"  onerror="this.onerror=null;this.src=\'/Content/Img/noimage.png\'" class="img-thumbnail" style="height:75px" /></div>' +
                '<div class="col-md-9"><button type="button" class="btn btn-xs btn-default pull-right" style="cursor:pointer" onclick="if(confirm(\'Are you sure to unselect Package ' + data[i].BundleName.toUpperCase() + '?\')){$(\'#select_' + data[i].BundleId + '\').trigger(\'click\')}">' +
                '<i class="glyphicon glyphicon-trash"></i>' +
                '</button>' +
                '<p><b>Pakage: <span class="text-green">' + data[i].BundleName + '</span></b></p>' +
                list_model +
                '</div></div></td>' +
                '<td><input type="number" id="qty_bundle_' + data[i].BundleId + '" readonly value="' + data[i].Quantity + '" style="border:1px solid gray; padding:2px 5px; width:50px; background-color: transparent;float:left" />' +
                '<button type="button" style="float:left; margin-left:-1px" onclick="change_qty_package(\'' + data[i].BundleId + '\', -1)"><i class="fa fa-minus"></i></button><button onclick="change_qty_package(\'' + data[i].BundleId + '\', 1)" type="button" style="float:left; margin-left:-1px"><i class="fa fa-plus"></i></button></td>' +
                //'<td><b style="color:#258e4f">$0</b></td>' +
                '<td><b style="color:#258e4f">' + numberToMoney_USD(data[i].Price) + '</b></td>' +
                '<td><b style="color:#258e4f" id="amount_package_' + data[i].BundleId + '">' + numberToMoney_USD(data[i].Price * data[i].Quantity) + '</b></td>';
        }
        _content += '</tr>';
    }
    $("#list_product_order").append(_content);
    refresh_index_count();
}
//change_qty_device

//change money
function change_money() {
    let key = $('input[name="discount"]:checked').val();
    var discount = parseFloat($("input[name='discount-value']").val());
    var shipping_fee = $("input[name='shipping_fee']").val();
    var tax_rate = $("input[name='tax_rate']").val();
    //var other_fee = $("input[name='other']").val();

    if (discount < 0 || isNaN(discount)) {
        discount = 0;
    }
    if (discount == 0) {
        $("input[name='discount-value']").val(discount);
    }

    if (shipping_fee < 0 || isNaN(shipping_fee)) {
        shipping_fee = 0;
        $("input[name='shipping_fee']").val(0);
    }

    //if (other_fee < 0 || other_fee == "" || other_fee == ".") {
    //    other_fee = 0;
    //    $("input[name='other']").val(0);
    //}

    if (tax_rate < 0 || tax_rate == "" || tax_rate == ".") {
        tax_rate = 0;
        $("input[name='tax_rate']").val(0);
    }

    $.ajax({
        method: "POST",
        url: "/order/ChangeMoney",
        data: {
            DisAmount: key == "discount-amount" ? discount : 0,
            DisPercent: key == "discount-rate" ? discount : 0,
            ShippingFee: shipping_fee,
            TaxRate: tax_rate
            //OtherFee: other_fee
        },
        dataType: "json"
    })
        .done(function (data) {
            //data: object[] {[true/false], [total_money_order/msg_error]}
            if (data[0] == true) {
                $("#sub_total").html(numberToMoney_USD(data[1].SubTotal));
                $("#grand_tottal").html(numberToMoney_USD(data[1].GrandTotal));
                if (discount != 0) {
                    if ($('input[name="discount"]:checked').val() == "discount-amount") {
                        $("input[name='discount-value']").val(data[1].DiscountAmount);
                    } else {
                        $("input[name='discount-value']").val(data[1].DiscountPercent);
                    }
                }
            }
            else {
                var MsgError = $.parseJSON('{"text":"Error! ' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(MsgError);
            }
        })
        .fail(function () {
            alert("Oops! Something went wrong");
        })
}

//save order
function save_order() {
    $("#save_order_loading").show();
    var cus = $("#merchant_id_hd").val();

    if (cus == "" || cus == null) {
        var MsgError = $.parseJSON('{"text":"Please select merchant.", "layout":"topRight", "type":"error"}');
        noty(MsgError);
        $("#save_order_loading").hide();

        return false;
    }
    var email = ($("#c_email").html()||'').trim();

    if (email == "" || email == null) {
        var MsgError = $.parseJSON('{"text":"Please update email.", "layout":"topRight", "type":"error"}');
        noty(MsgError);
        $("#save_order_loading").hide();

        return false;
    }
    overlayOn();
    return true;
}

function change_price_format(key) {
    var _price = "0";
    var _input_name = "";
    if (key == "device") {
        _price = $("input[name='p_price']").val();
        _input_name = "input[name='p_price']";
    }
    else if (key == "setup_fee") {
        _price = $("input[name='s_setup_fee']").val();
        _input_name = "input[name='s_setup_fee']";
    }
    else if (key == "monthly_fee") {
        _price = $("input[name='s_price']").val();
        _input_name = "input[name='s_price']";
    }

    if (_input_name != "") {
        var _regexp = new RegExp(/[^0-9]/igm);
        _price = toMoney2(Number(_price.replace(_regexp, "")));
        $(_input_name).val(_price);
    }
}

////select option
//function select_option() {
//    var _option = $("#_option_select").val();

//    if (_option == "Item") {
//        $('#bundle_id').hide('alow');
//        $('#bundle_id').val("");
//        $('#pos_device').show('alow');
//        $("#div_bundle_select").hide("alow");
//        //$("#div_bundle_select").empty();
//    }
//    else {
//        $('#pos_device').hide('alow');
//        $('#pos_device').val("");
//        $('#bundle_id').show('alow');
//        $("#div_POS_Device_Hardware").hide("alow");
//        //$("#div_POS_Device_Hardware").empty();
//    }
//}

////select bundle
//function select_bundle() {
//    $("#select_pos_dv_loading").show();
//    var _bundle_id = $("#bundle_id").val();

//    if (_bundle_id != "") {
//        $.ajax({
//            method: "POST",
//            url: "/order/SelectBundle",
//            data: { BundleId: _bundle_id },
//            dataType: "html"
//        })
//            .done(function (data) {
//                $("#select_pos_dv_loading").hide();
//                $("#div_bundle_select").html(data);
//                $("#div_bundle_select").show("alow");
//            })
//            .fail(function () {
//                $("#select_pos_dv_loading").hide();
//                alert("Oops! Something went wrong");
//            })
//    }
//    else {
//        $("#select_pos_dv_loading").hide();
//        $("#div_bundle_select").hide("alow");
//    }
//}

//function ConfirmBeforeAddBundle() {
//    $("#add_bundle_loading").show();
//    var _bundle_id = $("#bundle_id").val();

//    var id_exist = $("#bundle_" + _bundle_id);
//    //alert(id_exist.length);
//    //[id_exist.length = 0 : id khong ton tai], [id_exist.length = 1 : id co ton tai]
//    if (id_exist.length > 0) {
//        if (confirm("Package has been selected, you want to add continue?")) {
//            change_qty_package(_bundle_id, 1);
//            $("#bundle_id").val("");
//            $("#div_bundle_select").hide("slow");
//        }
//        else {
//            $("#add_bundle_loading").hide();
//        }
//    }
//    else {
//        add_bundle(_bundle_id);
//    }
//}

//add bundle

//preview estimates
$(".btn_preview_es").click(function () {
    $("#preview_loading").show();
    PreviewEstimatesOrCreateInvoice(true, false);
});

//create invoice
$("#btn_create_inv").click(function () {
    $("#create_inv_loading").show();
    PreviewEstimatesOrCreateInvoice(false, true);
});

function PreviewEstimatesOrCreateInvoice(preview, create) {
    var cus_code = $("#merchant_id_hd").val();
    var sales_memNumber = $("#SalesMemberNumber").val();
    var desc = $("textarea[name='desc']").val();
    var ship_address = $('input[name="sh_street"]').val() +
        "|" + $('input[name="sh_city"]').val() +
        "|" + $('input[name="sh_state"]').val() +
        "|" + $('input[name="sh_zip"]').val() +
        "|" + $('input[name="sh_country"]').val();

    if (cus_code == "" || cus_code == null) {
        $("#preview_loading").hide();
        $("#create_inv_loading").hide();
        var MsgError = $.parseJSON('{"text":"Please select merchant.", "layout":"topRight", "type":"error"}');
        noty(MsgError);
    }
    else {
        //alert(preview + "|" + create);
        //preview
        if (preview == true && create == false) {
            $.ajax({
                method: "POST",
                url: "/order/PreviewEstimatesOrCreateInvoice",
                data: {
                    preview: preview,
                    create: create,
                    cus_code: cus_code,
                    sales_memNumber: sales_memNumber,
                    desc: desc,
                    ship_address: ship_address
                },
                dataType: "html"
            })
                .done(function (data) {
                    //data: _InvoiceDetailPartial
                    $("#preview_loading").hide();
                    $("#invoice_partial_div").html(data);
                    $("#view_invoice").modal("show");
                })
                .fail(function () {
                    $("#preview_loading").hide();
                    alert("Oops! Something went wrong");
                })
        }

        //create invooice
        if (preview == false && create == true) {
            $.ajax({
                method: "POST",
                url: "/order/PreviewEstimatesOrCreateInvoice",
                data: {
                    preview: preview,
                    create: create,
                    cus_code: cus_code,
                    sales_memNumber: sales_memNumber,
                    desc: desc,
                    ship_address: ship_address
                },
                dataType: "json"
            })
                .done(function (data) {
                    //data: {[true/false], [url, error_msg]}
                    if (data[0] == true) {
                        //alert(data[1]);
                        window.location.href = data[1];
                    }
                    else {
                        $("#create_inv_loading").hide();
                        var MsgError = $.parseJSON('{"text":"Create failure! ' + data[1] + '", "layout":"topRight", "type":"error"}');
                        noty(MsgError);
                    }
                })
                .fail(function () {
                    $("#create_inv_loading").hide();
                    alert("Oops! Something went wrong");
                })
        }
    }
}

