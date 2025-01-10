$(function () {
    $('#Save_bunlde_form').on('submit', function () {
        save_bundle();
        return false;
    })
    $('#Search_device_select').on('submit', function () {
        search_product($('#search_text_product').val());
        return false;
    })
    //CKEDITOR.replace('bundle_info', {
    //    toolbar: [
    //        // Defines toolbar group with name (used to create voice label) and items in 3 subgroups.
    //        { name: 'basicstyles', items: ['Bold', 'Italic'] }, ['NumberedList', 'BulletedList']		// Defines toolbar group without name.																				// Line break - next group will be placed in new line.

    //    ],
    //});
});
$("#tab_btn a").click(function () {
    $('#tab_btn .btn-primary').removeClass('btn-primary');
    $(this).addClass('btn-primary');
});

function pickup_a_bundle_modal() {
    $('#order_code').html('-----');
    $('order_date').html('-----');
    $('order_merchant').html('-----');
    $('order_total').html('-----');
    $('#list_order_bundles').empty();
    $('#list_pick_bundles').empty();
    $('#product_table_body').empty();
    $('#pickupbundle').modal('show');
}

function load_bundle() {
    $('#loading_tab').show();
    $.ajax({
        method: "POST",
        url: "/device/LoadBundle",
        data: {},
        dataType: "html"
    })
        .done(function (data) {
            $("#list_bundle_management").html(data);
            $('#loading_tab').hide();
        })
        .fail(function () {
        })
        .always(function () {
        });
}
//
function Load_pick_bundles(id, search = null) {
    if (id == 0) return false;
    $('#Load_pick_bundles_image').show();
    $.ajax({
        method: "POST",
        url: "/Device/LoadPickBundles",
        data: { id, search },
        dataType: "json"
    })
        .done(function (data) {
            if (data[0] == true) {
                $('#order_id_selected').val(id);
                $('#order_code').html(data[1]['OrdersCode']);
                $('order_date').html(data[2]);
                $('order_merchant').html(data[1]['CustomerName']);
                $('order_total').html(toMoney(data[1]['GrandTotal']));

                $('#product_table_body').empty();
                if (data[3].length == 0) {
                    $('#product_table_body').append("<tr><td>( No product )</td></tr>");
                } else
                    for (var i = 0; i < data[3].length; i++) {
                        $('#product_table_body').append("<tr><td>" + data[3][i]['Quantity'] + " " + data[3][i]['ProductCode'] + "</td></tr>");
                    }
                $('#list_order_bundles').html(data[4]);
                $('#list_pick_bundles').html(data[5]);
                check_match_bundle();
            }
        })
        .fail(function () {
        })
        .always(function () {
            $('#Load_pick_bundles_image').hide();
        });
}
function pick_bundle(bundle_id) {
    $('#bundle_' + bundle_id).appendTo('#list_order_bundles');
    $('#bundle_' + bundle_id + " a.pick_bundle").hide();
    $('#bundle_' + bundle_id + " a.unpick_bundle").show();
    check_match_bundle();
}

function remove_order_bundle(bundle_id) {
    $('#bundle_' + bundle_id).appendTo('#list_pick_bundles');
    $('#bundle_' + bundle_id + " a.pick_bundle").show();
    $('#bundle_' + bundle_id + " a.unpick_bundle").hide();
    check_match_bundle();
}
function remove_bundle_device(bundle_id, device_id) {
    var d_num = $('#device_list_' + bundle_id + ' tr').length - $('#device_list_' + bundle_id + ' tr.removed').length;
    if (d_num <= 1) {
        var Error = $.parseJSON('{"text":"Can not remove all device of a bundle!", "layout":"topRight", "type":"error"}');
        noty(Error);
    }
    else {
        $('#b_de_' + device_id).addClass('removed');
        $('#bundle_' + bundle_id + ' .btn_refresh').show();
        check_match_bundle();
    }
}

function check_match_bundle() {
    var order_code = $('#order_code').html();
    var list_device = [];
    $("#list_order_bundles input.device_id").each(function () {
        list_device.push($(this).val());
    });
    var list_device_removed = [];
    $("#list_order_bundles tr.removed input.device_id").each(function () {
        list_device_removed.push($(this).val());
    });
    $.ajax({
        method: "POST",
        url: "/device/_checkMatchBundle",
        data: { order_code, "devices_id": list_device.join(), "devices_removed_id": list_device_removed.join() },
        dataType: "json"
    })
        .done(function (data) {
            if (data == true) {
                $("#not_match").hide();
                $("#save_order").show();
            }
            else {
                $("#not_match").show(200);
                $("#save_order").hide();
            }
        })
        .fail(function () {
        })
        .always(function () {
        });
}
function save_order_bundle() {
    var order_code = $('#order_code').html();
    var list_device = [];
    $("#list_order_bundles input.device_id").each(function () {
        list_device.push($(this).val());
    });
    var list_device_removed = [];
    $("#list_order_bundles tr.removed input.device_id").each(function () {
        list_device_removed.push($(this).val());
    });


    $('#save_order_bundle_image').show();

    $.ajax({
        method: "POST",
        url: "/device/SaveOrderBundle",
        data: { order_code, "devices_id": list_device.join(), "devices_removed_id": list_device_removed.join() },
        dataType: "json"
    })
        .done(function (data) {
            if (data[0] == true) {
                var Success = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"success"}');
                noty(Success);
                //
                for (var i = 0; i < data[2].length; i++) {
                    var bundle = data[2][i];

                    $("#dashboard_bundle_" + bundle["Id"]).remove();

                    var Update = "";
                    if (bundle["UpdateAt"]) {
                        var UpdateAt = new Date(parseInt(bundle["UpdateAt"].substr(6))).format("mmm dd,yyyy hh:MMTT");
                        Update = 'By <b>' + bundle["UpdateBy"] + "</b><br/>At <i>" + UpdateAt || "------" + '</i>';
                    }

                    $('#bundle_dashboard_list tbody').prepend(
                        '<tr id="dashboard_bundle_' + bundle["Id"] + '"><td>#' + bundle["OrderCode"] + '</td>' +
                        '<td>' + bundle["MerchantName"] + '</td>' +
                        '<td>' + bundle["BundleCode"] + '</td>' +
                        '<td>' + bundle["Name"] + '</td>' +
                        '<td><div class="label label-info">' + bundle["Status"] + '</div></td>' +
                        '<td>' + Update + '</td>' +
                        '<td style="padding:5px"><a class="btn btn-warning" onclick="update_package(' + bundle["Id"] + ',\'' + bundle["Status"] + '\')">Progress update</a></td>' +
                        '</tr>'
                    );
                    $('#select_order option[value="' + $('#order_id_selected').val() + '"]').remove();
                    $('#select_order').trigger('change');
                }

                $('#pickupbundle').modal('hide');
                if (list_device_removed.length > 0) {
                    if (confirm("You you want create task with removed device from bunlde?")) {
                        update_task('true', '0', 'true', data[3], data[4]);
                    }
                }
            }
            else {
                var Error = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(Error);
            }
        })
        .fail(function () {
        })
        .always(function () {
            $('#save_order_bundle_image').hide();
        });
}


function load_product(id = null) {
    $.ajax({
        method: "POST",
        url: "/device/LoadSelectProduct",
        data: { id },
        dataType: "html"
    })
        .done(function (data) {
            $('#device_select_list').html(data);
        })
        .fail(function () {
        })
}
function search_product(search_text) {
    var id = $('#bundle_id').val();
    var line = $('#select_line').val();
    $('#search_product_img').show();
    $.ajax({
        method: "POST",
        url: "/device/SearchSelectProduct",
        data: { search_text, line, id },
        dataType: "html"
    })
        .done(function (data) {
            $('#device_select_list').html(data);
        })
        .fail(function () {
        })
        .always(function () {
            $('#search_product_img').hide()
        })
}
function select_bundle_device(id) {
    $.ajax({
        method: "POST",
        url: "/device/SelectBundleDevice",
        data: { id },
        dataType: "json"
    })
        .done(function (data) {
            //data { true/false , List_device, Product_code}
            if (data[0] == true) {
                Refresh_selected_device(data[1]);
                $('option[value="' + id + '"]').remove();
                if (data[2] && $('#product_' + data[2] + ' .product_device option').length <= 0) {
                    $('#product_' + data[2]).hide();
                }
                var quantity = parseInt($('#product_' + data[2] + ' quantity').html());
                $('#product_' + data[2] + ' quantity').html(quantity - 1);
            }
            else {
                var Error = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(Error);
            }
        })
        .fail(function () {
        })
        .always(function () {
        });


}
function unselect_bundle_device(product_code, id, serial) {
    $.ajax({
        method: "POST",
        url: "/Device/UnselectBundleDevice",
        data: { id },
        dataType: "json"
    })
        .done(function (data) {
            if (data[0] == true) {
                $('#selected_device_' + id).remove();
                var opt = new Option('#' + serial, id, false, false);
                $('#product_' + product_code + ' .product_device').append(opt).trigger('change');
                if ($('#product_' + product_code + ' .product_device option').length > 0) {
                    $('#product_' + product_code).show();
                }
                var quantity = parseInt($('#product_' + product_code + ' quantity').html());
                $('#product_' + product_code + ' quantity').html(quantity + 1);
            }
            else {
                var Error = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(Error);
            }
        })
        .fail(function () {
        })
        .always(function () {
        });

}
function Refresh_selected_device(devices) {
    $('#bd_list_device tbody').empty();
    for (var i = 0; i < devices.length; i++) {
        $('#bd_list_device tbody').append('<tr id="selected_device_' + devices[i]['DeviceId'] + '"><td style="padding:0px;width: 55px"><img onerror="this.onerror=null; this.src=\'/Upload/Img/no_image.jpg\'" width="50" height="50" src="' + devices[i]['Picture'] + '"></td>' +
            '<td><input type="hidden" class="bundle_device_id" value="' + devices[i]['DeviceId'] + '">' +
            '<b><a>#' + devices[i]['SerialNumber'] + '</a></b><br> ' + devices[i]['ProductName'] +
            '<a class="btn btn-sm pull-right text-danger" onclick="unselect_bundle_device(\'' + devices[i]['ProductCode'] + '\',' + devices[i]['DeviceId'] + ',\'' + devices[i]['SerialNumber'] +
            '\')"><i class="fa fa-close"></i> remove</a></td></tr>');
    }
}
function delete_bundle(id) {
    if (confirm("Do you want to delete this bundle?")) {
        $.ajax({
            method: "POST",
            url: "/device/DeleteBundle",
            data: { id },
            dataType: "json"
        })
            .done(function (data) {

                if (data[0] == true) {
                    var Success = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"success"}');
                    noty(Success);
                    $('#tr_' + id).remove();
                }
                else {
                    var Error = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(Error);
                }
            })
            .fail(function () {
            })
            .always(function () {
            });
    }
}
function save_bundle() {
    var code = $('#bundle_code').val();
    var name = $('#bundle_name').val();
    var info = CKEDITOR.instances['bundle_info'].getData();
    var id = $('#bundle_id').val();
    $('#save_bundle_img').show();
    $.ajax({
        method: "POST",
        url: "/Device/SaveBundle",
        data: { code, name, info, id },
        dataType: "json"
    })
        .done(function (data) {
            if (data[0] == true) {
                var Success = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"success"}');
                noty(Success);
                if (id) {
                    $('#tr_' + id).remove();
                }
                var CreateAt = new Date(parseInt(data[2]["CreateAt"].substr(6)));
                var Update = "";
                if (data[2]["UpdateAt"]) {
                    var UpdateAt = new Date(parseInt(data[2]["UpdateAt"].substr(6))).format("mmm dd,yyyy hh:MMTT");
                    Update = 'By <b>' + data[2]["UpdateBy"] + "</b><br/>At <i>" + UpdateAt || "------" + '</i>';
                }
                $("#list_bundle_management tbody").append(
                    '<tr id="tr_' + data[2]["Id"] + '">'
                    + '<td>' + data[2]["BundleCode"] + '</td>'
                    + '<td>' + data[2]["Name"] + '</td>'
                    + '<td>' + data[2]["Info"] + '</td>'
                    + '<td>By <b>' + data[2]["CreateBy"] + "</b><br/>At <i>" + CreateAt.format("mmm dd,yyyy hh:MMTT") + '</i></td>'
                    + '<td>' + Update + '</td>'
                    + '<td><a class="btn btn-warning" onclick="show_modal_bundle(' + data[2]["Id"] + ')"><i class="fa fa-pencil"></i></a>'
                    + ' <a class="btn btn-danger" onclick="delete_bundle(' + data[2]["Id"] + ')"><i class="fa fa-trash"></i></a>'
                    + '</td>'
                    + '</tr>'
                );
                $('btn_tag_bundlehardware').trigger("click");
                $('#Edit_bundle').modal('hide');
            }
            else {
                var Error = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(Error);
            }
        })
        .fail(function () {
        })
        .always(function () {
            $('#save_bundle_img').hide();
        });
}
function update_package(id, status) {
    sessionStorage.setItem("id", id);

    if (status == "READY TO PACK" || status == "PACKAGING") {
        window.location.href = "/device/preparation";
    }
    else {
        window.location.href = "/device/delivery";
    }
}

//function update_task(_action, _id, _update, taskname, assigneds) {
//    $.ajax({
//        method: "POST",
//        url: "/tasksman/GetInfoTask",
//        data: {
//            action: _action,
//            id: _id,
//            update: _update,
//        },
//        dataType: "html"
//    })
//        .done(function (data) {
//            $("#task_popup").html(data);
//            $("#modal-task").modal('show');
//            $("select[name=ticket_id]").parent().hide();
//            $("textarea[name=Name]").html(taskname);
//            $('#Assigned_select').val(assigneds);
//            $('#Assigned_select').trigger('change');
//        })
//        .fail(function () {
//            alert("Oops! Something went wrong");
//            $("#modal-task").modal('hide');
//        })
//        .always(function () {
//            //$("#loading").hide();
//        });
//}



//////////////////
function show_order_bundle() {
    $('#select_order_group').show();
    $("#Edit_order_bundle").modal("show");
}
function show_order_bundle_edit(id) {
    Load_order(id);
    $('#select_order_group').hide();
    $("#Edit_order_bundle").modal("show");
}
function Load_order(id) {
    if (id == 0) return false;
    $('#Load_pick_bundles_image').show();
    $.ajax({
        method: "POST",
        url: "/Device/LoadOrderModal",
        data: { id },
        dataType: "json"
    })
        .done(function (data) {
            if (data[0] == true) {
                $('#order_id_selected').val(id);
                $('#order_code').html(data[1]['OrdersCode']);
                $('order_date').html(data[2]);
                $('order_merchant').html(data[1]['CustomerName']);
                $('order_total').html(toMoney(data[1]['GrandTotal']));

                $('#product_table_body').empty();
                if (data[3].length == 0) {
                    $('#product_table_body').append("<tr><td>( No product )</td></tr>");
                } else
                    for (var i = 0; i < data[3].length; i++) {
                        $('#product_table_body').append("<tr><td>" + data[3][i]['Quantity'] + " " + data[3][i]['ProductName'] + "</td>" +
                            "<input type='hidden' id='o_qty_" + data[3][i]['ProductCode'] + "' value='" + data[3][i]['Quantity'] + "'/></tr>");
                    }
                $('#product_selected').html(data[4]);
                $('#product_selected .Select_product_feature').hide();
                $('#product_selected .Remove_product_feature').show();
                load_productfeature(id, true);
                checkmatch_order();
                //$('#list_order_bundles').html(data[4]);
                //$('#list_pick_bundles').html(data[5]);
            }
            else {
                noty($.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}'));
                
            }
        })
        .fail(function () {
        })
        .always(function () {
            $('#Load_pick_bundles_image').hide();
        });
}
function load_productfeature(id, q0 = false) {

    $.ajax({
        method: "POST",
        url: "/device/LoadProductFeature",
        data: { id },
        dataType: "html"
    })
        .done(function (data) {
            $('#list_product_feature').html(data);
            //$('#product_selected').empty();
            //if (q0) {
            //$('#product_selected').html(data);
            //$('#product_selected .product_item').hide();
            //$('#product_selected .feature_item').hide();
            //$('#product_selected .p_quantity').html('0');
            //$('#product_selected .f_quantity').html('0');
            //$('#product_selected .Select_product_feature').hide();
            //$('#product_selected .Remove_product_feature').show();
            //}
        })
        .fail(function () {
        })
}
function Select_product_feature(e, is_remove) {
    var p_code = $(e).closest(".product_item").find(".product_code").val();
    var f_text = $(e).closest(".feature_item").find(".feature_text").val();

    if (!is_remove) {
        var src = $('#list_product_feature');
        var des = $('#product_selected');
        if (!des.find(".p_" + p_code).length) {
            src.find(".p_" + p_code).clone().find('.feature_item').remove().end().appendTo(des);
            des.find(".p_" + p_code + " .p_quantity").html(0);
        }
        if (!des.find(".p_" + p_code + " .f_" + f_text).length) {
            src.find(".p_" + p_code + " .f_" + f_text).clone().appendTo(des.find(".p_" + p_code));
            des.find(".p_" + p_code + " .f_" + f_text + " .f_quantity").html(0);
            des.find('.Select_product_feature').hide();
            des.find('.Remove_product_feature').show();
        }
    }
    else {
        var des = $('#list_product_feature');
        var src = $('#product_selected');
    }


    var s_p_qty = src.find(".p_" + p_code + " .p_quantity");
    var s_f_qty = src.find(".p_" + p_code + " .f_" + f_text + " .f_quantity");
    var d_p_qty = des.find(".p_" + p_code + " .p_quantity");
    var d_f_qty = des.find(".p_" + p_code + " .f_" + f_text + " .f_quantity");

    $.ajax({
        method: "POST",
        url: "/device/AddRemoveSelected",
        data: { p_code, f_text, is_remove },
        dataType: "json"
    })
        .done(function (data) {
            s_p_qty.html(s_p_qty.html() - 1);
            s_f_qty.html(s_f_qty.html() - 1);
            d_p_qty.html(d_p_qty.html() - -1);
            d_f_qty.html(d_f_qty.html() - -1);
            if (s_p_qty.html() <= 0) { s_p_qty.closest(".product_item").hide(); }
            if (s_f_qty.html() <= 0) { s_f_qty.closest(".feature_item").hide(); }
            if (d_p_qty.html() > 0) { d_p_qty.closest(".product_item").show(); }
            if (d_f_qty.html() > 0) { d_f_qty.closest(".feature_item").show(); }
            checkmatch_order();
        })
        .fail(function () {
        })





}
function checkmatch_order() {
    var match = $('#product_selected .product_item').length > 0;
    $('#product_selected .product_item').each(function () {
        var p_code = $(this).find(".product_code").val();
        if ($(this).find(".p_quantity").html() != $('#o_qty_' + p_code).val()) {
            match = false; return false;
        }
    });
    if (match) {
        $("#save_order").show();
        $("#warning_notmatch").hide();
    } else {
        $("#save_order").hide();
        $("#warning_notmatch").show();
    }
}
function save_order_hardwares() {
    var order_id = $("#order_id_selected").val();
    $.ajax({
        method: "POST",
        url: "/device/SaveOrderHardwares",
        data: { order_id },
        dataType: "json"
    })
        .done(function (data) {
            if (data[0]) {
                var Success = $.parseJSON('{"text":"Save completed", "layout":"topRight", "type":"success"}');
                noty(Success);
                var bundle = data[1];
                $("#dashboard_bundle_" + bundle["Id"]).remove();

                var Update = "";
                if (bundle["UpdateAt"]) {
                    var UpdateAt = new Date(parseInt(bundle["UpdateAt"].substr(6))).format("mmm dd,yyyy hh:MMTT");
                    Update = 'By <b>' + bundle["UpdateBy"] + "</b><br/>At <i>" + UpdateAt || "------" + '</i>';
                }

                $('#bundle_dashboard_list tbody').prepend(
                    '<tr id="dashboard_bundle_' + bundle["Id"] + '"><td>#<b>' + bundle["OrderCode"] + '</b></td>' +
                    '<td>' + bundle["MerchantName"] + '</td>' +
                    '<td>#<b>' + bundle["BundleCode"] + '</b></td>' +
                    '<td><div class="label label-info">' + bundle["Status"] + '</div></td>' +
                    '<td>' + Update + '</td>' +
                    '<td style="padding:5px"><a class="btn btn-warning" onclick="update_package(' + bundle["Id"] + ',\'' + bundle["Status"] + '\')">Progress update</a></td>' +
                    '</tr>'
                );
                $('#select_order option[value="' + $('#order_id_selected').val() + '"]').remove();
                $('#select_order').trigger('change');
                $("#Edit_order_bundle").modal("hide");


            }
        })
        .fail(function () {
        })
        .always(function () {
        });

}