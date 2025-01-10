function CreatePDFComplete(data) {
    //data: object[] {[true/false], [msg_success/msg_error], [file_id], [file_name], [file_path], [terminal_id]}
    $("#create_dejavoo_loading").hide();
    $("#update_dejavoo_loading").hide();
    if (data[0] == true) {
        $('#modal-dejavoo').modal('hide');
        var MsgSuccess = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"success"}');
        noty(MsgSuccess);
        Load();
        LoadNewHistory();
        //view_template(data[2], data[3], data[4], data[5]);
        console.log(data);
        ViewForSend('view', data[2], true)

    }
    else {
        var Error = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
        noty(Error);
    }
}

function CreatePDFError() {
    alert("Oops! Something went wrong");
    $("#create_dejavoo_loading").hide();
    $("#update_dejavoo_loading").hide();
    $("#modal-dejavoo").modal('hide');
}

function LoadNewHistory() {
    $.ajax({
        method: "POST",
        url: "/MerchantFormManage/LoadNewHistory",
        dataType: "json"
    })
        .done(function (data) {
            if (data[0] == true) {
                $('#List_PDF_history').empty();
                var _option = "";

                for (var i = 0; i < data[1].length; i++) {
                    var _tempalte_name = data[1][i]["TemplateName"];
                    if (data[1][i]["TemplateName"].length > 30) {
                        _tempalte_name = data[1][i]["TemplateName"].substring(0, 30) + '...';
                    }

                    var _status = "";
                    if (data[1][i]["Status"] == "completed") {
                        _status = '<span class="label label-primary">Signed</span><br />' +
                            '<b><i class="fa fa-clock-o"></i></b><span> ' + data[1][i]["UpdateAt"] + '</span><br />' +
                            '<b>Sent by: </b><span>' + data[1][i]["SendByAgent"] + ' - At: ' + data[1][i]["SendAt"] + '</span>';
                    }
                    else if (data[1][i]["Status"] == "sent") {
                        _status = '<span class="label label-success">Sent</span><br />' +
                            '<b><i class="fa fa-clock-o"></i></b><span> ' + data[1][i]["SendAt"] + ' - By: ' + data[1][i]["SendByAgent"] + '</span>';
                    }
                    else if (data[1][i]["Status"] == "delivered") {
                        _status = '<span class="label" style="background-color:deepskyblue">Delivered</span><br />' +
                            '<b><i class="fa fa-clock-o"></i></b><span> ' + data[1][i]["UpdateAt"] + '</span><br />' +
                            '<b>Sent by: </b><span>' + data[1][i]["SendByAgent"] + ' - At: ' + data[1][i]["SendAt"] + '</span>';
                    }
                    else {
                        _status = '<span class="label label-warning">Uploaded</span><br />' +
                            '<b><i class="fa fa-clock-o"></i></b><span> ' + data[1][i]["CreateAt"] + ' - By: ' + data[1][i]["CreateBy"] + '</span>';
                    }

                    var data_view = data[1][i]["TemplateName"] + '|' + data[1][i]["MerchantCode"] + '|' + data[1][i]["OrderId"];
                    _option += '<div class="col-md-12 PDF_history" title="' + data[1][i]["TemplateName"] + '" style="border-bottom: 1px solid lightgray; padding-left:0px; margin-bottom:10px; padding-bottom:10px; cursor:pointer" ' +
                        'onclick="ViewForSend(\'view\',\'' + data_view + '\')">' +
                        '<div class="col-md-2" style="padding-left:0px"><img src="/Upload/Img/pdf.png" width="40" /></div>' +
                        '<div class="col-md-10">' +
                        '<b style="color:#00A65A">' + _tempalte_name + '</b><br />' +
                        '<b>DBA Name: </b>' + data[1][i]["MerchantName"] + '<br />' +
                        '<b>Status: </b>' + _status +
                        '</div></div>';
                }
                $('#List_PDF_history').append(_option);

                if (data[2] == true) {
                    $('#List_PDF_history').append('<button type="button" class="btn btn-block btn-flat btn-success" id="add_pdf_history" onclick="load_more_history()">View more...</button>');
                }
            }
            else {
                var Error = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(Error);
            }
        })
        .fail(function () {
            alert("Oops! Something went wrong");
        })
        .always(function () {
        });
}

function GetListTemplate() {
    $.ajax({
        method: "POST",
        url: "/MerchantFormManage/GetListTemplate",
        dataType: "json"
    })
        .done(function (data) {
            //data: object[] {[true/false], [FileName_Array/msg_error]}
            if (data[0] == true) {
                $("#list_template").empty();

                var _option = '<option value="">--N/A--</option>';
                for (var i = 0; i < data[1].length; i++) {
                    var template_name = data[1][i].replace(/_/g, ' ');
                    _option += '<option value="' + data[1][i] + '">' + template_name + '</option>';
                }

                $("#list_template").append(_option);
            }
            else {
                var MsgError = $.parseJSON('{"text":"Error! ' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(MsgError);
            }
        })
        .fail(function () {
            alert("Oops! Something went wrong");
        });
}

function AddMoreFile(merCode, merName) {
    $("#addmorefile_loading").show();
    $("#list_terminal").val("");
    $("#div_list_terminal").hide();
    $("#list_order").val("");
    $("#div_list_order").hide();

    $("#list_template").val("").trigger("change");
    $("#merchant_code").val(merCode);
    $("#merchant_name").val(merName);
    $("#modal_choose_template").modal('show');
    $("#addmorefile_loading").hide();

    $("#choose_period").empty();
    $("#div_period").hide();
}

function LoadOrder() {
    var template_name = $("#list_template").val();
    var merCode = $("#merchant_code").val();

    $("#submit_btn").html("OK");
    if (template_name == "Nuvei") {
        $("#submit_btn").html("Update");
    }

    if (template_name == "DEJAVOO_Z11_DATA_SHEET") {
        $("#addfile_btn").show();
        $("#submit_btn").hide();
    } else {
        $("#addfile_btn").hide();
        $("#submit_btn").show();
    }

    if (template_name != "" && template_name != "BANK CHANGE FORM" && template_name != "CHANGE REQUEST FORM" && template_name != "RequestForm" && template_name != "Nuvei" && template_name.indexOf("Priority") == -1) {
        $("#template_loading").show();

        $.ajax({
            method: "POST",
            url: "/MerchantFormManage/GetListOrder",
            data: { merchantCode: merCode, templateName: template_name },
            dataType: "json"
        })
            .done(function (data) {
                //data: object[] {[true/false], [list_order/msg_error]}
                $("#template_loading").hide();
                if (data[0] == true) {
                    $("#div_list_order").show();
                    $("#div_attach").hide(300);
                    $("#list_order").empty();

                    var _option = '<option value="">--Select--</option>';
                    for (var i = 0; i < data[1].length; i++) {
                        _option += '<option value="' + data[1][i].OrdersCode + '">Order code: #' + data[1][i].OrdersCode + '</option>';
                    }

                    if (template_name != "Recurring Payment ACH") {
                        $("#div_period").hide();
                    }

                    $("#list_order").append(_option);
                }
                else {
                    var MsgError = $.parseJSON('{"text":"Error! ' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(MsgError);
                }
            })
            .fail(function () {
                $("#addmorefile_loading").hide();
                $("#template_loading").hide();
                alert("Oops! Something went wrong");
            });
    }
    else {
        $("#list_order").val("");
        $("#div_list_order").hide();
        $("#list_terminal").val("");
        $("#div_list_terminal").hide();
        if (template_name != "Recurring Payment ACH") {
            $("#div_period").hide();
        }
    }
}

function change_order() {
    var template_name = $("#list_template").val();
    var order_code = $("#list_order").val();
    if (template_name == "Recurring Payment ACH") {

        if (order_code != "") {
            $("#order_loading").show();

            $.post("/MerchantFormManage/GetPeriod", { OrderCode: order_code }, function (data) {
                //data: [{true/false}, {list_period/error_msg}]
                $("#order_loading").hide();
                if (data[0] == true) {
                    var _opt = '';
                    for (var i = 0; i < data[1].length; i++) {
                        _opt += '<option value="' + data[1][i] + '">' + data[1][i] + '</option>';
                    }

                    $("#choose_period").html(_opt);
                    $("#div_period").show();
                }
                else {
                    var optionsErr = $.parseJSON('{"text":"' + data[1] + '","layout":"topRight","type":"error"}');
                    noty(optionsErr);
                }
            }).fail(function () {
                $("#order_loading").hide();
                alert("Oops! Something went wrong");
            });
        }
        else {
            $("#choose_period").empty();
            $("#div_period").hide();
        }
    }

    if (template_name == "DEJAVOO_Z11_DATA_SHEET" && order_code != "") {
        $.ajax({
            method: "POST",
            url: "/MerchantFormManage/GetListTerminal",
            data: { orderCode: order_code },
            dataType: "json"
        })
            .done(function (data) {
                //data: object[] {[true/false], [list_terminal/msg_error]}
                if (data[0] == true) {
                    $("#div_list_terminal").show();
                    $("#list_terminal").empty();

                    var _option = '<option value="">--N/A--</option>';
                    for (var i = 0; i < data[1].length; i++) {
                        _option += '<option value="' + data[1][i].DeviceId + '">Inventory Number: #' + data[1][i].InvNumber + '</option>';
                    }

                    $("#list_terminal").append(_option);
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
    else {
        $("#choose_period").empty();
        $("#div_period").hide();
    }
}

function LoadTerminal() {
    var template_name = $("#list_template").val();
    var order_code = $("#list_order").val();

    if (template_name == "DEJAVOO_Z11_DATA_SHEET" && order_code != "") {

        $.ajax({
            method: "POST",
            url: "/MerchantFormManage/GetListTerminal",
            data: { orderCode: order_code },
            dataType: "json"
        })
            .done(function (data) {
                //data: object[] {[true/false], [list_terminal/msg_error]}
                if (data[0] == true) {
                    $("#div_list_terminal").show();
                    $("#list_terminal").empty();

                    var _option = '<option value="">--N/A--</option>';
                    for (var i = 0; i < data[1].length; i++) {
                        _option += '<option value="' + data[1][i].DeviceId + '">Inventory Number: #' + data[1][i].InvNumber + '</option>';
                    }

                    $("#list_terminal").append(_option);
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
    else {
        $("#list_terminal").val("");
        $("#div_list_terminal").hide();
    }
}

function UpdateFile() {
    var merchant_form_id = $("#merchant_form_id").val();
    $("#addfile_loading").show();
    $.ajax({
        method: "POST",
        url: "/MerchantFormManage/UpdateFile",
        data: {
            merchant_form_id
        },
        dataType: "html"
    })
        .done(function (data) {
            $("#PartialDejavo").empty();
            $("#PartialDejavo").append(data);
            $("#modal-dejavoo").modal("show");
            $("#modal_choose_template").modal("hide");
            $("#addfile_loading").hide();
            $("#modal_PDFfile").modal("hide");
        })
        .fail(function () {
            alert("Oops! Something went wrong");
            $("#addfile_loading").hide();
        })
}

function AddFile() {
    $("#addfile_loading").show();
    var template_name = $("#list_template").val();
    var customer_code = $("#merchant_code").val()
    var order_code = $("#list_order").val();
    var terminal_id = $("#list_terminal").val();

    if (template_name == "" || template_name == null) {
        var MsgError = $.parseJSON('{"text":"Please choose template.", "layout":"topRight", "type":"error"}');
        noty(MsgError);
        $("#addfile_loading").hide();
    }
    else if (template_name != "Bank_Change_Form" && (order_code == "" || order_code == null)) {
        var MsgError = $.parseJSON('{"text":"Please choose order.", "layout":"topRight", "type":"error"}');
        noty(MsgError);
        $("#addfile_loading").hide();
    }
    else if (template_name == "DEJAVOO_Z11_DATA_SHEET" && order_code != "" && (terminal_id == "" || terminal_id == null)) {
        var MsgError = $.parseJSON('{"text":"Please choose terminal.", "layout":"topRight", "type":"error"}');
        noty(MsgError);
        $("#addfile_loading").hide();
    }
    else if (template_name == "Bank_Change_Form") {
        $.ajax({
            method: "POST",
            url: "/MerchantFormManage/CreateBankChangeForm",
            data: { customerCode: customer_code },
            dataType: "json"
        })
            .done(function (data) {
                //data: object[] {[true/false], [msg_success/msg_error], [file_id], [file_name], [file_path]}
                if (data[0] == true) {
                    var MsgSuccess = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);
                    Load();
                    LoadNewHistory();
                    view_template(data[2], data[3], data[4], "null");
                }
                else {
                    var MsgError = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(MsgError);
                }

                $("#modal_choose_template").modal("hide");
                $("#addfile_loading").hide();
            })
            .fail(function () {
                alert("Oops! Something went wrong");
                $("#addfile_loading").hide();
            })
    }
    else if (template_name == "Enrich_Finance_OneTime_Payment") {
        $.ajax({
            method: "POST",
            url: "/MerchantFormManage/CreateOneTimePayment",
            data: {
                customerCode: customer_code,
                orderCode: order_code
            },
            dataType: "json"
        })
            .done(function (data) {
                //data: object[] {[true/false], [msg_success/msg_error], [file_id], [file_name], [file_path]}
                if (data[0] == true) {
                    var MsgSuccess = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);
                    Load();
                    LoadNewHistory();
                    view_template(data[2], data[3], data[4], "null")
                }
                else {
                    var MsgError = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(MsgError);
                }

                $("#modal_choose_template").modal("hide");
                $("#addfile_loading").hide();
            })
            .fail(function () {
                alert("Oops! Something went wrong");
                $("#addfile_loading").hide();
            })
    }
    else {
        $.ajax({
            method: "POST",
            url: "/MerchantFormManage/AddFile",
            data: {
                templateName: template_name,
                customerCode: customer_code,
                orderCode: order_code,
                terminalId: terminal_id
            },
            dataType: "html"
        })
            .done(function (data) {
                $("#PartialDejavo").empty();
                $("#PartialDejavo").append(data);
                $("#modal-dejavoo").modal("show");
                $("#modal_choose_template").modal("hide");
                $("#addfile_loading").hide();
            })
            .fail(function () {
                alert("Oops! Something went wrong");
                $("#addfile_loading").hide();
            })
    }
}

function check_file_open() {
    var my_url = window.location.href;
    var url_query = my_url.split("?")[1];

    if (url_query != "undefined" && sessionStorage.getItem("file_id") != null) {
        var file_id = url_query.replace("op=", "");

        $.ajax({
            method: "POST",
            url: "/MerchantFormManage/OpenFileAfterCheck",
            data: { id: file_id },
            dataType: "json"
        })
            .done(function (data) {
                //data: object[] {[true/false], [file_id/msg_error], [file_name], [file_url], [sign_url]}
                if (data[0] == true) {
                    //view_template(data[1], data[2], data[3], data[4]);

                    $('#Id_PDFfile').val(data[1]);
                    $("#FileSending_Id").val(data[1]);
                    $("#FileSending_TemplateName").html(data[2]);
                    $("#FileSending_Name").html(data[2]);
                    $('#modal_FileSending').modal({ backdrop: 'static', keyboard: false });
                    //$('#myModal').modal({backdrop: 'static', keyboard: false}) 
                    //sessionStorage.clear();
                    sendFile();
                }
            })
            .fail(function () {
                alert("Oops! Something went wrong");
            })
            .always(function () {
            });
    }
}

function sendFile() {
    if (sessionStorage.getItem("file_id") != null) {
        send();
    }
    else {
        if (confirm("Do you want to send to merchant?")) {
            send();
        }
    }
}

function send() {
    sessionStorage.clear();
    $('#sendfile_loading').show();
    var id = $('#Id_PDFfile').val();

    $.ajax({
        method: "POST",
        url: "/MerchantFormManage/SendPDF",
        data: { fileId: id },
        dataType: "json"
    })
        .done(function (data) {
            //data: object[] {[true/false], [msg_success/msg_error]}

            if (data[0] == true) {
                $('#sendfile_loading').hide();
                $('#modal_PDFfile').modal('hide');
                $('#modal_FileSending').modal('hide');

                var MsgSuccess = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"success"}');
                noty(MsgSuccess);
                Load();
                LoadNewHistory();
            }
            else {
                if (data[1] == "Login for get access token") {
                    sessionStorage.setItem("file_id", id);
                    window.location.href = '/DocuSignLoad?goback=/MerchantFormManage?op=' + id;
                }
                else {
                    $('#sendfile_loading').hide();
                    var Error = $.parseJSON('{"text":"Send failure! "' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(Error);
                }
            }
        })
        .fail(function () {
            $('#sendfile_loading').hide();
            alert("Oops! Something went wrong");
        })
        .always(function () {
        });
}


function CreateNewPDF() {
    
    var TemplateName = $("#list_template").val();
    var MerchantCode = $("#merchant_code").val();
    if (TemplateName.indexOf("Priority")==-1) {
        ViewForSend();
    } else {
        $("#createfile_loading").show();
        $.ajax({
            method: "POST",
            url: "/MerchantFormHandle/CreateNewPDF",
            data: {
                TemplateName,
                MerchantCode
            },
            dataType: "json"
        })
            .done(function (data) {
                if (data[0]) {
                    $("#modal_choose_template").modal("hide");
                    Load();
                    ViewForSend('view', data[1], `${TemplateName}|${MerchantCode}|`);
                } else {
                    error(data[1]);
                }
            })
            .fail(function () {
                alert("Oops! Something went wrong");
            })
            .always(function () {
                $("#createfile_loading").hide();
            });
    }
}
//Loc update 20200227

function ViewForSend(key, merchant_form_id, data_view, show_update_dejavoo = false) {
    //data_view: "TemplateName|MerchantCode|OrderCode"
    sessionStorage.clear();
    $("#pdf_key").val(key);

    var template_Name = $("#list_template").val();
    var order_Code = $("select[name='orderCode']").val();

    if (key == "view") {
        var _data = data_view.split("|");

        if (_data[0].indexOf("Nuvei") != -1) {
            $("#list_template").val("Nuvei");
            $("[name=pdf_nuvei_type]").val(_data[0]);
        }
        else {
            $("#list_template").val(_data[0]);
            $("[name=pdf_nuvei_type]").val("");
        }


        $("#merchant_form_id").val(merchant_form_id);
        $("#merchant_code").val(_data[1]);
        //$("#list_order").val(_data[2]);
        $("#list_order").html('<option id="opt-temp" value="' + _data[2] + '" selected>Order code: #' + _data[2] + '</option>');

        template_Name = _data[0];
        order_Code = _data[2];
    }
    else {
        $("#merchant_form_id").val("");
    }
    
    if (key != "view" && (template_Name == "" || template_Name == null)) {
        let MsgError = $.parseJSON('{"text":"Please choose template.", "layout":"topRight", "type":"error"}');
        noty(MsgError);
    }
    else if (key != "view" && template_Name != "Nuvei" && template_Name != "BANK CHANGE FORM" && template_Name != "CHANGE REQUEST FORM" && (template_Name != "RequestForm" && (order_Code == "" || order_Code == null))) {
        let MsgError = $.parseJSON('{"text":"Please choose order.", "layout":"topRight", "type":"error"}');
        noty(MsgError);
    }
    else {
        if (key == "view" || template_Name != "Nuvei" || confirm("\"" + $("[name=pdf_nuvei_type]").val() + "\" attachment will upload for \"" + $("#merchant_name").val() + "\". The uploaded attachment will replace the current file on the system.")) {

            if (template_Name == "Nuvei") {
                if ($("select[name='pdf_nuvei_type']").val() == "") {
                    noty({ "text": "Please select Nuvei PDF", "layout": "top", "type": "warning" });
                }
                else if ($("input[name='file_attachment']").val() == "") {
                    noty({ "text": "Please select a file to upload", "layout": "top", "type": "warning" });
                }
                else {
                    $.ajax({
                        method: "POST",
                        url: "/MerchantFormHandle/CheckSigned",
                        data: {
                            templateName: $("select[name='pdf_nuvei_type']").val(),
                            merchantCode: $("#merchant_code").val()
                        },
                        dataType: "json"
                    })
                        .done(function (data) {
                            if (data == "signed" && confirm("This file has been signed. It will be replaced if you RECREATE a new file. Do you agree?") == true) {
                                overlayOn();
                                $("#add_more_file_form").submit();
                            }
                            else if (data == "unsigned") {
                                overlayOn();
                                $("#add_more_file_form").submit();
                            }
                            else {
                                var ErrorMsg = $.parseJSON('{"text":"Check file failure! ' + message + '", "layout":"topRight", "type":"error"}');
                                noty(ErrorMsg);
                            }
                        })
                        .fail(function () {
                            alert("Oops! Something went wrong");
                        });
                }
            }
            else {
                overlayOn();
                $("#add_more_file_form").submit();
            }
        }
    }

    if (show_update_dejavoo) {
        $("#update_dejavo").show();
    } else {
        $("#update_dejavo").hide();
    }
}

$("#add_more_file_form").ajaxForm(function (data) {
    if (data[0] == true) {
        overlayOff();
        if (data[2] == "") {
            $('#li_pdf_signed').hide();
            $('#li_pdf_unsigned a').trigger("click");
        }
        else {
            $('#PDFfile_sign_url').prop("src", data[2]);
            $('#li_pdf_signed').show();
            $('#li_pdf_signed a').trigger("click");
        }

        $("#opt-temp").remove();
        if (data[4].search("Nuvei") != -1) {
            $("#PDFfile_name").html(data[4]);
        }
        else {
            $("#PDFfile_name").html(data[4] + " (Order #" + data[6] + ")");
        }
        
        $("#modal_choose_template").modal('hide');
        $("#_body").css("padding-right", "0px");
        document.getElementById('PDFfile_url').contentWindow.location.reload(true);
        $('#PDFfile_url').attr("src", data[1]);


        $("#merchant_form_id").val(data[3]);
        if (data[7] == "view") {
            $('#modal_PDFfile').modal('show');
            $("#btn-send-title").html("Send to merchant ");
        }
        else if (data[4].indexOf("Nuvei") == -1) {
            $('#modal_PDFfile').modal('show');
            $("#btn-send-title").html("Create & Send to merchant ");
        } else {
            noty({ "text": "Upload completed", "layout": "topRight", "type": "success" });
            $("#tr_" + data[3]).trigger("click");
            Load();
        }
        //if (data[4].indexOf("Nuvei") != -1 && data[7] != "view") {
        //    $(".pdf_btn").hide();
        //    $("#btn_save_nuvei_pdf").show();
        //} else {
        //    $("#btn_sendmerchant").show();
        //    $("#btn_save_nuvei_pdf").hide();
        //}
        var send_param_data = data[7] + "|" + data[4] + "|" + data[5] + "|" + data[6];
        sessionStorage.setItem("send_param", send_param_data);
    }
    else {
        if (data[1] == "Login for get access token") {
            var _data = data[7] + "|" + data[4] + "|" + data[5] + "|" + data[6];
            sessionStorage.setItem("viewdata", _data);
            window.location.href = '/DocuSignLoad?goback=/MerchantFormManage';
        }
        else {
            overlayOff();
            noty({ "text": data[1], "layout": "topRight", "type": "error" });

        }
    }
});

function DownloadtoUpdate(newfile = false) {
    var merchantCode = $("#merchant_code").val();
    var pdf_type = $("select[name=pdf_nuvei_type]").val();

    if (pdf_type == "" || pdf_type == null) {
        noty({ "text": "Please select Nuvei PDF", "layout": "top", "type": "warning" });
    }
    else {
        $.ajax({
            method: "POST",
            url: "/Page/MerchantFormHandle/DownloadtoUpdate",
            data: {
                pdf_type, merchantCode, newfile
            },
            dataType: "json"
        })
            .done(function (data) {
                //data: object[] {[true/false], [msg_success/msg_error]}
                if (data[0]) {
                    window.location = "/upload/downloadfile_?file=" + data[1];
                } else {
                    noty({ "text": "Check file failure! " + data[1] + "", "layout": "topRight", "type": "error" });
                }
            })
            .fail(function () {
                overlayOff();
                $("#modal_FileSending").modal("hide");
                alert("Oops! Something went wrong");
            });
    }
}
function download_pdf() {
    var data_param = sessionStorage.getItem("send_param");
    var data = data_param.split("|");
    var key = data[0];
    var template_Name = data[1];
    var merchant_Code = data[2];
    var order_Code = data[3];
    overlayOn();
    var period = $("#choose_period").val();
    $.ajax({
        method: "POST",
        url: "/Page/MerchantFormHandle/DownloadSendPDF",
        data: {
            Key: key,
            templateName: template_Name,
            merchantCode: merchant_Code,
            orderCode: order_Code,
            period: period,
        },
        dataType: "json"
    })
        .done(function (data) {
            if (data[0]) {
                if (key != "view") {
                    success(data[1]);
                }
               
                $("#search_form").trigger("submit");
                $("#modal_PDFfile").modal("hide");
                window.location = "/upload/downloadfile_?file=" + data[2];
            }
            else {
                error(data[1]);
            }
        })
        .always(function () {
            overlayOff();
        })
}
function SendToMerchant(flag) {
    var data_param;
    if (flag == "reload") {
        data_param = sessionStorage.getItem("send");
    }
    else {
        data_param = sessionStorage.getItem("send_param");
    }
    var data = data_param.split("|");
    var key = data[0];
    var template_Name = data[1];
    var merchant_Code = data[2];
    var order_Code = data[3];
    if (flag == "reload") {
        $("#FileSending_TemplateName").html(template_Name);
        //$("#modal_FileSending").modal("show");
        $('#modal_FileSending').modal({ backdrop: 'static', keyboard: false });

        sendAction(key, template_Name, merchant_Code, order_Code);
    }
    else {
        if (key == "view") {
            if (confirm("Do you want to send to merchant?") == true) {
                sendAction(key, template_Name, merchant_Code, order_Code);
            }
        }
        else {
            $.ajax({
                method: "POST",
                url: "/MerchantFormHandle/CheckSigned",
                data: {
                    templateName: template_Name,
                    merchantCode: merchant_Code,
                    orderCode: order_Code
                },
                dataType: "json"
            })
                .done(function (data) {
                    if (data == "signed" && confirm("This file has been signed. It will be replaced if you RECREATE a new file. Do you agree?") == true) {
                        sendAction(key, template_Name, merchant_Code, order_Code);
                    }
                    else if (data == "unsigned" && confirm("Do you want to Create & Send to merchant?") == true) {
                        sendAction(key, template_Name, merchant_Code, order_Code);
                    }
                    else {
                        var ErrorMsg = $.parseJSON('{"text":"Check file failure! ' + message + '", "layout":"topRight", "type":"error"}');
                        noty(ErrorMsg);
                    }
                })
                .fail(function () {
                    alert("Oops! Something went wrong");
                });
        }
    }
}
function SaveNuveiPDF() {
    var data_param = sessionStorage.getItem("send_param");
    var Type = $("[name=pdf_nuvei_type]").val();
    var merchantCode = data_param.split("|")[2];
    overlayOn();
    $.ajax({
        method: "POST",
        url: "/MerchantFormHandle/SaveNuveiPDF",
        data: {
            merchantCode, Type
        },
        dataType: "json"
    })
        .done(function (data) {
            if (data[0]) {
                overlayOff();
                $('#sendfile_loading').hide();
                $('#modal_PDFfile').modal('hide');
                $('#modal_FileSending').modal('hide');

                var MsgSuccess = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"success"}');
                noty(MsgSuccess);
                $("#modal_FileSending").modal("hide");
                Load();
                LoadNewHistory();
                sessionStorage.clear();
            } else {
                var MsgSuccess = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(MsgSuccess);
            }
        })
        .fail(function () {
            alert("Oops! Something went wrong");
        });

}
function sendAction(key, tempName, merCode, orderCode) {
    overlayOn();
    var period = $("#choose_period").val();

    $.ajax({
        method: "POST",
        url: "/Page/MerchantFormHandle/SendPDFtoMerchant",
        data: {
            Key: key,
            templateName: tempName,
            merchantCode: merCode,
            orderCode: orderCode,
            period: period,
        },
        dataType: "json"
    })
        .done(function (data) {
            //data: object[] {[true/false], [msg_success/msg_error]}

            if (data[0] == true) {
                overlayOff();
                $('#sendfile_loading').hide();
                $('#modal_PDFfile').modal('hide');
                $('#modal_FileSending').modal('hide');

                var MsgSuccess = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"success"}');
                noty(MsgSuccess);
                $("#modal_FileSending").modal("hide");
                Load();
                LoadNewHistory();
            }
            else {
                if (data[1] == "Login for get access token") {
                    var send_param_data = key + "|" + tempName + "|" + merCode + "|" + orderCode;
                    sessionStorage.setItem("send", send_param_data);
                    window.location.href = '/DocuSignLoad?goback=/MerchantFormManage';
                }
                else if (data[1] == "-1") {
                    overlayOff();
                    var WarnMsg = $.parseJSON('{"text":"Cannot send signed file. Please recreate file to send.", "layout":"top", "type":"warning"}');
                    noty(WarnMsg);
                    $("#modal_FileSending").modal("hide");
                }
                else {
                    overlayOff();
                    var Error = $.parseJSON('{"text":"Send failure! ' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(Error);
                    $("#modal_FileSending").modal("hide");
                }
            }
        })
        .fail(function () {
            overlayOff();
            $("#modal_FileSending").modal("hide");
            alert("Oops! Something went wrong");
        });
}

