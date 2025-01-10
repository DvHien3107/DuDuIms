
$("#li_information").click(function () {
    CheckRequired("information", "information");
});
$("#li_specifics").click(function () {
    CheckRequired("boarding", "specifics");
});
$("#li_boarding").click(function () {
    CheckRequired("information", "boarding");
});
$("#li_checkout").click(function () {
    var form_info_complete = CheckRequired("information", "specifics");

    if (form_info_complete == true) {
        CheckRequired("specifics", "checkout");
    }
});
$("#btn_information").click(function () {
    CheckRequired("information", "boarding");
});
$("#btn_boarding").click(function () {
    CheckRequired("boarding", "specifics");
});
$("#btn_specifics").click(function () {
    CheckRequired("specifics", "checkout");
});
$("#btn_checkout").click(function () {
    CheckRequired("specifics", "question");
});
$("#li_question").click(function () {
    var form_info_complete = CheckRequired("information", "specifics");
    if (form_info_complete == true) {
        var form_spec_complete = CheckRequired("specifics", "checkout");
        if (form_spec_complete == true) {
            CheckRequired("specifics", "question");
        }
    }
});

$("#btn_submit").click(function () {
    CheckRequired("question", "");
});


$("#btn_back_information").click(function () {
    goback("information");
});

$("#btn_back_boarding").click(function () {
    goback("boarding");
});

$("#btn_back_specifics").click(function () {
    goback("information");
});
$("#btn_back_checkout").click(function () {
    goback("specifics");
});
$("#btn_back_question").click(function () {
    goback("checkout");
})


//===Check required===
function CheckRequired(idFormCheck, idFormNext) {
    var complete = true;
    //alert(idFormCheck + "- next: " + idFormNext);

    if (idFormCheck == "information") {
        var salon_name = $("#SalonName").val();
        var contact_name = $("#ContactName").val();
        var contact_number = $("#ContactNumber").val();
        var salon_email = $("#SalonEmail").val();
        var owneremail = $("#OwnerEmail").val();
        var salon_add1 = $("#SalonAddress1").val();
      //  var salon_add2 = $("#SalonAddress2").val();
        var salon_city = $("#SalonCity").val();
        var salon_state = $("#SalonState").val();
        var salon_zip = $("#SalonZipcode").val();
        var Salon_timezone = $("#SalonTimeZone").val();
        var listEmail = [];
        //alert(salon_add1);
        if (salon_email == "") {
            $("#SalonEmail").addClass("input-required");
            $("#error_SalonEmail").show();
            complete = false;
        }
        else {
            $("#SalonEmail").removeClass("input-required");
            $("#error_SalonEmail").hide();
        
        }
        if (owneremail == "") {
            $("#OwnerEmail").addClass("input-required");
            $("#error_OwnerEmail").show();
            complete = false;
        }
        else {
            $("#OwnerEmail").removeClass("input-required");
            $("#error_OwnerEmail").hide();
        }
        if ($("#SendToOwnersEmail").val() == '') {
            var listEmail = [];
            if (salon_email != "") {
                console.log(salon_email);
                $('#SendToOwnersEmail').tagEditor('addTag', salon_email);
            }
            if (owneremail != "") { 
                console.log(owneremail);
                $('#SendToOwnersEmail').tagEditor('addTag', owneremail);
            }
            $('.tag-editor>li').each(function (index, value) {
                if ($(this).children('.active').length > 0) {
                    $(this).remove();
                }
            })
        }
       
        if (salon_add1 == "") {
            $("#SalonAddress1").addClass("input-required");
            $("#error_SalonAddress1").show();
            complete = false;
        }
        else {
            $("#SalonAddress1").removeClass("input-required");
            $("#error_SalonAddress1").hide();
        }

        if (salon_city == "") {
            $("#SalonCity").addClass("input-required");
            $("#error_SalonCity").show();
            complete = false;
        }
        else {
            $("#SalonCity").removeClass("input-required");
            $("#error_SalonCity").hide();
        }

        if (salon_state == "") {
            $("#SalonState").addClass("input-required");
            $("#error_SalonState").show();
            complete = false;
        }
        else {
            $("#SalonState").removeClass("input-required");
            $("#error_SalonState").hide();
        }


        if (salon_zip == "") {
            $("#SalonZipcode").addClass("input-required");
            $("#error_SalonZipcode").show();
            complete = false;
        }
        else {
            $("#SalonZipcode").removeClass("input-required");
            $("#error_SalonZipcode").hide();
        }
        if (Salon_timezone == "") {
            $("#SalonTimeZone").addClass("input-required");
            $("#error_SalonTimeZone").show();
            complete = false;
        }
        else {
            $("#SalonTimeZone").removeClass("input-required");
            $("#error_SalonTimeZone").hide();
        }


        if (salon_name == "") {
            $("#SalonName").addClass("input-required");
            $("#error_SalonName").show();
            complete = false;
        }
        else {
            $("#SalonName").removeClass("input-required");
            $("#error_SalonName").hide();
        }

        if (contact_name == "") {
            $("#ContactName").addClass("input-required");
            $("#error_ContactName").show();
            complete = false;
        }
        else {
            $("#ContactName").removeClass("input-required");
            $("#error_ContactName").hide();
        }

        if (contact_number == "") {
            $("#ContactNumber").addClass("input-required");
            $("#error_ContactNumber").show();
            complete = false;
        }
        else {
            $("#ContactNumber").removeClass("input-required");
            $("#error_ContactNumber").hide();
        }
    }
  
    if (idFormCheck == "boarding") {
        if ($("#Drivers_License_Front_Image-dl").html().trim()== "")
        {
            $(".Drivers_License_Front_Image-groups").addClass("input-required");
            complete = false;
        }
        else {
            $(".Drivers_License_Front_Image-groups").removeClass("input-required");
        }
        if ($("#Drivers_License_Front_Image-dl").html().trim()== "") {
            $(".Drivers_License_Back_Image-groups").addClass("input-required");
            complete = false;
        }
        else {
            $(".Drivers_License_Back_Image-groups").removeClass("input-required");
        }
        if ($("#SS4_Name").val().trim() == "") {
            $(".SS4-groups").addClass("input-required");
            complete = false;
        }
        else {
            $(".SS4-groups").removeClass("input-required");
        }
        if ($("#VoidedBusinessCheck-dl").html().trim() == "") {
            $(".VoidedBusinessCheck-groups").addClass("input-required");
            complete = false;
        }
        else {
            $(".VoidedBusinessCheck-groups").removeClass("input-required");
        }

        
    }


    if (idFormCheck == "specifics") {

        if ($("input[name='PromotionDiscount']:checked").val() == 1 && $("#PromotionType").val() == "") {
            $("#PromotionType").addClass("input-required");
            $("#error_PromotionType").show();
            complete = false;
        }
        else {
            $("#PromotionType").removeClass("input-required");
            $("#error_PromotionType").hide();
        }

        if ($("input[name='ServicesCharge_SupplyFee']:checked").val() == 1 && $("#ServiceFee").val() == "") {
            $("#ServiceFee").addClass("input-required");
            $("#error_ServiceFee").show();
            complete = false;
        }
        else {
            $("#ServiceFee").removeClass("input-required");
            $("#error_ServiceFee").hide();
        }

        if ($("input[name='GeneralDiscount']:checked").val() == 1 && $("#DiscountType").val() == "") {
            $("#DiscountType").addClass("input-required");
            $("#error_DiscountType").show();
            complete = false;
        }
        else {
            $("#DiscountType").removeClass("input-required");
            $("#error_DiscountType").hide();
        }
     
    }

    if (idFormCheck == "question") {
        if ($("input[name='TechSellGiftCard']:checked").val() == 1 && $("#AUTHORIZED_Seller").val() == "") {
            $("#AUTHORIZED_Seller").addClass("input-required");
            $("#error_AUTHORIZED_Seller").show();
            complete = false;
        }
        else {
            $("#AUTHORIZED_Seller").removeClass("input-required");
            $("#error_AUTHORIZED_Seller").hide();
        }

        if ($("input[name='TechMakeAppointment']:checked").val() == 1 && $("#AUTHORIZED_Tech").val() == "") {
            $("#AUTHORIZED_Tech").addClass("input-required");
            $("#error_AUTHORIZED_Tech").show();
            complete = false;
        }
        else {
            $("#AUTHORIZED_Tech").removeClass("input-required");
            $("#error_AUTHORIZED_Tech").hide();
        }
    }

    if (complete == true) {

        if (idFormNext == "") {
            //submit
            $("#submit_loading").show();
            $('#questionare_form').submit();
        }
        else {
            $(".li_tabs").removeClass("active");
            $(".tab-pane").removeClass("active in");

            $("#li_" + idFormNext).addClass("active");
            $("#" + idFormNext).addClass("active in");

            $('body,html').animate({
                scrollTop: 0
            }, 500);
        }
    }
    else {
        $("#" + idFormCheck + " .input-required:first").focus();
    }

    return complete;
}

function goback(formId) {
    $(".li_tabs").removeClass("active");
    $(".tab-pane").removeClass("active in");

    $("#li_" + formId).addClass("active");
    $("#" + formId).addClass("active in");

    $('body,html').animate({
        scrollTop: 0
    }, 500);
}


//Add Manager Item / Tech Iem
function addManagerItem() {
    var id = parseInt($("#hd_managerItem").val());
    var curId = id - 1;

    if (curId > 0 && $("#manager_name" + curId).val() == "") {
        $("#error_M_name" + curId).show();
        $("#manager_name" + curId).addClass("input-required");
        $("#manager_name" + curId).focus();
    }
    else {
        $("#error_M_name" + curId).hide();
        $("#manager_name" + curId).removeClass("input-required");

        var _content = item_manager_draw(id);
        //console.log(_content);
        if (id == 1) {
            //btn_addmanager_item
            $(_content).insertBefore("#btn_addmanager_item");
        }
        else {
            $(_content).insertAfter("#manager" + (id - 1));
        }
        $("#hd_managerItem").val(id + 1);
    }
}

function addTechItem() {
    var id = parseInt($("#hd_techItem").val());
    var curId = id - 1;

    if (curId > 0 && $("#tech_name" + curId).val() == "") {
        $("#error_T_name" + curId).show();
        $("#tech_name" + curId).addClass("input-required");
        $("#tech_name" + curId).focus();
    }
    else {
        $("#error_T_name" + curId).hide();
        $("#tech_name" + curId).removeClass("input-required");

        var _content = item_tech_draw(id);
        //console.log(_content);
        if (id == 1) {
            //btn_addmanager_item
            $(_content).insertBefore("#btn_addtech_item");
        }
        else {
            $(_content).insertAfter("#tech" + (id - 1));
        }
        $("#hd_techItem").val(id + 1);
    }
}


//Remove Manager Item / Tech Item
function removeManagerItem(idRemove) {
    var id = parseInt($("#hd_managerItem").val());
    var currentId = id - 1;

    for (var i = parseInt(idRemove); i < currentId; i++) {
        $("#manager_id" + i).val($("#manager_id" + (i + 1)).val());
        $("#manager_name" + i).val($("#manager_name" + (i + 1)).val());
        $("#manager_job" + i).val($("#manager_job" + (i + 1)).val());
        $("#manager_pay" + i).val($("#manager_pay" + (i + 1)).val());

        //alert($("input[name='manager_payroll" + (i + 1) + "']:checked").val());
        if ($("input[name='manager_payroll" + (i + 1) + "']:checked").val() == 0) {
            $("#manager_payroll_yes" + i).prop("checked", true);
        }
        else {
            $("#manager_payroll_no" + i).prop("checked", true);
        }
    }

    $("#manager" + currentId).remove();
    $("#hd_managerItem").val(currentId);
}

function removeTechItem(idRemove) {
    var id = parseInt($("#hd_techItem").val());
    var currentId = id - 1;

    for (var i = parseInt(idRemove); i < currentId; i++) {
        $("#tech_id" + i).val($("#tech_id" + (i + 1)).val());
        $("#tech_name" + i).val($("#tech_name" + (i + 1)).val());
        $("#tech_nickname" + i).val($("#tech_nickname" + (i + 1)).val());
        $("#tech_comm" + i).val($("#tech_comm" + (i + 1)).val());
        $("#tech_pay" + i).val($("#tech_pay" + (i + 1)).val());

        //alert($("input[name='tech_discout" + (i + 1) + "']:checked").val());
        if ($("input[name='tech_discout" + (i + 1) + "']:checked").val() == 0) {
            $("#tech_dis_yes" + i).prop("checked", true);
        }
        else {
            $("#tech_dis_no" + i).prop("checked", true);
        }

        if ($("input[name='tech_price" + (i + 1) + "']:checked").val() == 0) {
            $("#tech_price_yes" + i).prop("checked", true);
        }
        else {
            $("#tech_price_no" + i).prop("checked", true);
        }
    }

    $("#tech" + currentId).remove();
    $("#hd_techItem").val(currentId);
}


//Draw HTML Manager Item / Tech Item
function item_manager_draw(id) {
    var bg_color = 'whitesmoke';
    if (id % 2 == 0) {
        bg_color = 'white';
    }

    var content = '<div id="manager' + id + '" style="background-color:' + bg_color + '; padding:5px">' +
        '<div><i class="fa fa-times-circle text-primary" style="cursor:pointer" onclick="removeManagerItem(' + id + ')"></i><b> Mục ' + id + '</b>' +
        '<input type="hidden" id="manager_id' + id + '" name="manager_id' + id + '" value="' + id + '"/></div>' +
        '<div class="col-md-3"><div class="form-group"><label>Tên <span style="color:red">*</span></label>' +
        '<input type="text" class="form-control" id="manager_name' + id + '" name="manager_name' + id + '" placeholder="Tên">' +
        '<span id="error_M_name' + id + '" class="fa fa-exclamation-circle errspan" style="display:none"></span></div></div>' +
        '<div class="col-md-3"><div class="form-group"><label>Công việc</label>' +
        '<input type="text" class="form-control" id="manager_job' + id + '" name="manager_job' + id + '" placeholder="Công việc"></div></div>' +
        '<div class="col-md-3"><div class="form-group"><label>Lương</label>' +
        '<input type="text" class="form-control" id="manager_pay' + id + '" name="manager_pay' + id + '" placeholder="$15/giờ $200/tháng"></div></div>' +
        '<div class="col-md-3"><div class="form-group"><label style="white-space: nowrap;">Điều chỉnh/Xem bảng lương</label>' +
        '<div class="switch"><input type="radio" id="manager_payroll_yes' + id + '" class="switch-input" name="manager_payroll' + id + '" value="0" checked>' +
        '<label for="manager_payroll_yes' + id + '" class="switch-label switch-label-off"> Có</label>' +
        '<input type="radio" id="manager_payroll_no' + id + '" class="switch-input" name="manager_payroll' + id + '" value="1">' +
        '<label for="manager_payroll_no' + id + '" class="switch-label switch-label-on"> Không</label><span class="switch-selection"></span>' +
        '</div></div></div>' +
        '<div class="clearfix"></div></div>';

    return content;
}

function item_tech_draw(id) {
    var bg_color = 'whitesmoke';
    if (id % 2 == 0) {
        bg_color = 'white';
    }

    var content = '<div id="tech' + id + '" style="background-color:' + bg_color + '; padding:5px">' +
        '<div><i class="fa fa-times-circle text-primary" style="cursor:pointer" onclick="removeTechItem(' + id + ')"></i><b> Mục ' + id + '</b>' +
        '<input type="hidden" id="tech_id' + id + '" name="tech_id' + id + '" value="' + id + '"/></div>' +
        '<div class="col-md-3"><div class="form-group"><label>Tên kỹ thuật viên <span style="color:red">*</span></label>' +
        '<input type="text" class="form-control" id="tech_name' + id + '" name="tech_name' + id + '">' +
        '<span id="error_T_name' + id + '" class="fa fa-exclamation-circle errspan" style="display:none"></span></div></div>' +
        '<div class="col-md-2"><div class="form-group"><label>Biệt danh</label>' +
        '<input type="text" class="form-control" id="tech_nickname' + id + '" name="tech_nickname' + id + '"></div></div>' +
        '<div class="col-md-2"><div class="form-group"><label>Tiền hoa hồng</label>' +
        '<input type="text" class="form-control" id="tech_comm' + id + '" name="tech_comm' + id + '" placeholder="Tiền hoa hồng"></div></div>' +
        '<div class="col-md-5"><div class="form-group"><label>Tỉ lệ phân lương/Tiền mặt cho bảng lương</label>' +
        '<input type="text" class="form-control" id="tech_pay' + id + '" name="tech_pay' + id + '" placeholder="60/40 70/30"></div></div>' +
        '<div class="col-md-2"><div class="form-group"><label>Thêm giảm giá</label>' +
        '<div class="switch"><input type="radio" id="tech_dis_yes' + id + '" class="switch-input" name="tech_discout' + id + '" value="0" checked>' +
        '<label for="tech_dis_yes' + id + '" class="switch-label switch-label-off"> Có</label>' +
        '<input type="radio" id="tech_dis_no' + id + '" class="switch-input" name="tech_discout' + id + '" value="1">' +
        '<label for="tech_dis_no' + id + '" class="switch-label switch-label-on"> Không</label><span class="switch-selection"></span>' +
        '</div></div></div>' +
        '<div class="col-md-2"><div class="form-group"><label>Điều chỉnh giá</label>' +
        '<div class="switch"><input type="radio" id="tech_price_yes' + id + '" class="switch-input" name="tech_price' + id + '" value="0" checked>' +
        '<label for="tech_price_yes' + id + '" class="switch-label switch-label-off"> Có</label>' +
        '<input type="radio" id="tech_price_no' + id + '" class="switch-input" name="tech_price' + id + '" value="1">' +
        '<label for="tech_price_no' + id + '" class="switch-label switch-label-on"> Không</label><span class="switch-selection"></span>' +
        '</div></div></div>' +
        '<div class="clearfix"></div></div>';

    return content;
}