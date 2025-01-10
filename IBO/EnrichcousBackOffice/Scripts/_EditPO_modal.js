$.ajax({
    method: "POST",
    url: "/po/LoadColorlist",
    dataType: "json"
})
    .done(function (data) {

        $('#request_color').autocomplete({
            source: data,
            select: function () {
                $('#request_color').trigger("input");
                return false;
            }
        });
    })
    .fail(function () {
    });
function open_request(code = null, i_fa = null) {
    $('#request_modal').resetForm();
    $("#request_code").val('');
    if (i_fa) { i_fa.addClass("fa-spinner fa-spin").removeClass("fa-pencil");};
    if (code) {
        $.ajax({
            method: "POST",
            url: "/po/GetRequestInfo",
            data: { code },
            dataType: "json"
        })
            .done(function (data) {
                if (data[0] == true) {
                    $("#request_code").val(data[1].Code);
                    $("#select_product").val(data[1].ProductCode).select2();
                    load_model_select(data[1].ProductCode, data[1].ModelCode)
                    $("#request_color").html(data[1].Color);
                    if (data[1].Color) {
                        $("#model_color").show();
                    } else {
                        $("#model_color").hide();
                    }
                    $("#request_qty").val(data[1].RequestQty);
                    $("#request_note").val(data[1].Note);
                    $("#model_image").attr("src", data[1].ModelPicture);
                    $("#model_image").on("error", function () { this.onerror = null; this.src = '/Upload/Img/no_image.jpg' });
                    $("#request_modal .modal-title").html("Edit request #" + data[1].Code);
                    $('#request_modal').modal('show');
                }
                else {
                    noty({ "text": data[1], "layout": "topRight", "type": "error" });
                }
            })
            .fail(function () {
            })
            .always(function () {
                if (i_fa) { i_fa.removeClass("fa-spinner fa-spin").addClass("fa-pencil"); };
            });
    } else {
        $(".select2").select2();
        $("#model_image").attr("src", "/Upload/Img/no_image.jpg");
        $("#request_modal .modal-title").html("New request");
        $('#request_modal').modal('show');
    }

}
function delete_request(code, i_fa = null) {
    if (confirm("Do you wan't to delete request #" + code + "?")) {
        if (i_fa) { i_fa.addClass("fa-spinner fa-spin").removeClass("fa-trash"); };
        $.ajax({
            method: "POST",
            url: "/po/deleterequest",
            data: { code },
            dataType: "json"
        })
            .done(function (data) {
                if (data[0] == true) {
                    search_load();
                    noty({ "text": "Request delete complete", "layout": "topRight", "type": "success" });
                }
                else {
                    noty({ "text": data[1], "layout": "topRight", "type": "error" });
                }
            })
            .fail(function () {
            })
            .always(function () {
                search_load();
                if (i_fa) { i_fa.removeClass("fa-spinner fa-spin").addClass("fa-trash"); };
            });
    }
}
function load_model(code) {
    if (code) {
        $.ajax({
            method: "POST",
            url: "/PO/GetModelInfo",
            data: { code },
            dataType: "json"
        })
            .done(function (data) {
                if (data[0] == true) {
                    
                    $("#request_color").html(data[1].Color);
                    if (data[1].Color) {
                        $("#model_color").show();
                    } else {
                        $("#model_color").hide();
                    }
                    
                    $("#model_image").attr("src", data[1].Picture);
                    $("#model_image").on("error", function () { this.onerror = null; this.src = '/Upload/Img/no_image.jpg' });
                    
                }
                else {
                    noty({ "text": data[1], "layout": "topRight", "type": "error" });
                }
            })
            .fail(function () {
               
            })
            .always(function () {
            });
    }
}
function load_model_select(product_code,selectmodel = null) {
        $.ajax({
            method: "POST",
            url: "/PO/LoadModelSelect",
            data: { product_code },
            dataType: "json"
        })
            .done(function (data) {
                if (data[0] == true) {
                    console.log(data[1]);
                    $("#select_model").find("option").not("[value='']").remove();
                    for (var i in data[1]) {
                        var opt = new Option("#" + data[1][i].ModelName, data[1][i].ModelCode, false, false);
                        $("#select_model").append(opt);
                    }
                    $("#model_image").attr("src", "/Upload/Img/no_image.jpg");
                    $("#model_color").hide();
                    $("#select_model").removeAttr("disabled");
                    if (selectmodel) {
                        $("#select_model").val(selectmodel).select2().trigger("change");
                    }
                }
                else {
                    noty({ "text": data[1], "layout": "topRight", "type": "error" });
                    $("#select_model").attr("disabled", true); return false;
                }
            })
            .fail(function () {
                return false;
            })
            .always(function () {
            });
    
}
//function load_model_image() {
//    $("#select_model").val("").select2();
//    var product_code = $("#select_product").val();
//    var color = $("#request_color").val();
//    $.ajax({
//        method: "POST",
//        url: "/po/get_model_img",
//        data: { product_code, color },
//        dataType: "json"
//    })
//        .done(function (data) {
//            $("#model_image").attr("src", data);
//        })
//        .fail(function () {
//        })
//        .always(function () {
//        });
//}