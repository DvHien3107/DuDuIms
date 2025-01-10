// search click
$('#search_submit').click(function () {
    $('#search_loading').show()
    $('#dataTable').DataTable().ajax.reload();
    $('#search_loading').hide()
})

//view detail
function open_account_popup(Id, element) {
    $(".loading").show();
    var Type = $(element).attr('id');
    $("#img_load_" + Type + (Id || "newlead")).show();
    $.ajax({
        method: "POST",
        url: "/SaleLead/NewRegister_ViewDetail",
        data: { "id": Id }
    })
        .done(function (data) {
            $("#partial_detail").html(data);
            $('#form-sales_lead').modal('show');
            var buttonname = $(element).attr('data-button');
            $('#form-btn-' + buttonname).show();
            ProcessRequired(buttonname);
        })
        .fail(function () {
            alert("fail");
        })
        .always(function () {
            $("#img_load_" + Type + (Id || "newlead")).hide();
            $(".loading").hide();
        });
}
function ProcessRequired(command) {
    if (command == "verify" || command == "resend-email" || command == "create-trial") {
        $(".div-country label").append('<span style="color: red">*</span>');
        $(".div-country input").attr("required", true);
        $(".div-street label").append('<span style="color: red">*</span>');
        $(".div-street input").attr("required", true);
        $(".div-city label").append('<span style="color: red">*</span>');
        $(".div-city input").attr("required", true);
        $(".div-state label").append('<span style="color: red">*</span>');
        $(".div-state select").attr("required", true);
        $(".div-zipcode label").append('<span style="color: red">*</span>');
        $(".div-zipcode input").attr("required", true);
        //$(".div-contactname label").append('<span style="color: red">*</span>');
        //$(".div-contactname input").attr("required", true);
        $(".div-contactphone label").append('<span style="color: red">*</span>');
        $(".div-contactphone input").attr("required", true);
        //$(".div-salonname label").append('<span style="color: red">*</span>');
        //$(".div-salonname input").attr("required", true);
        $(".div-emailsalon label").append('<span style="color: red">*</span>');
        $(".div-emailsalon input").attr("required", true);
        $(".div-phonesalon label").append('<span style="color: red">*</span>');
        $(".div-phonesalon input").attr("required", true);
    }
}
//delete function
function delete_account(Id, row_index) {
    $.confirm({
        title: "<span class='text-secondary'>Confirm</span>",
        icon: 'fa fa-warning',
        content: 'Are you sure ?',
        type: 'blue',
        closeIcon: true,
        buttons: {
            confirm: {
                text: 'Confirm',
                btnClass: 'btn-confirm text-capitalize',
                action: function () {
                    $("#img_load_delete_" + (Id || "newlead")).show();
                    $.ajax({
                        method: "POST",
                        url: "/SaleLead/NewRegister_Delete",
                        data: { "id": Id },
                        dataType: "html"
                    }).done(function (data) {
                        if (data.status = true) {
                            //var i = $(row_index).closest("tr")[0].rowIndex;
                            //document.getElementById('dataTable').deleteRow(i);
                            $('#dataTable').DataTable().ajax.reload();
                            noty({ "text": "Delete success !", "layout": "topRight", "type": "success" });
                        }
                        else {
                            error(data.message)
                        }
                    })
                        .fail(function () {
                            alert("fail");
                        })
                        .always(function () {
                            $("#img_load_delete_" + (Id || "newlead")).hide();
                        });
                }
            },
            cancel: {
                text: 'Cancel',
                btnClass: 'btn-cancel text-capitalize',
                action: function () {
                }
            }
        }
    });
}

//form submit
$("#form-sales_lead").submit(function (e) {
    e.preventDefault();
    command = $('#command').val();
    var confirmation = true;
    if (command == 'verify') {
        confirmation = confirm("Are you sure you want to verify this account !");
        if (!confirmation) {
            return;
        }
    }
    if (confirmation) {
        $("#submit_img_" + command).show()
        $.ajax({
            type: "POST",
            url: "/SaleLead/NewRegister_Update",
            data: $("#form-sales_lead").serialize() + "&command=" + command, // serializes the form's elements.
            success: function (data) {
                if (data.status) {
                    if (command == 'verify') {
                        verifyOpenWindow(data.message);
                    }
                    else {
                        noty({ "text": data.message, "layout": "topRight", "type": "success" });
                        $('#form-sales_lead').modal('hide');
                        $('#dataTable').DataTable().ajax.reload();
                    }
                }
                else {
                    noty({ "text": data.message, "layout": "topRight", "type": "warning" });
                }
            },
            complete: function () {
                $("#submit_img_" + command).hide()
            },
        });
    }

})
$(document).ready(function () {
    $(".select2").select2();
})

//function open new window verify
function verifyOpenWindow(url) {
    debugger;
    var w = 900;
    var h = 500;
    var dualScreenLeft = window.screenLeft != undefined ? window.screenLeft : screen.left;
    var dualScreenTop = window.screenTop != undefined ? window.screenTop : screen.top;

    width = window.innerWidth ? window.innerWidth : document.documentElement.clientWidth ? document.documentElement.clientWidth : screen.width;
    height = window.innerHeight ? window.innerHeight : document.documentElement.clientHeight ? document.documentElement.clientHeight : screen.height;

    var left = ((width / 2) - (w / 2)) + dualScreenLeft;
    var top = ((height / 2) - (h / 2)) + dualScreenTop;
    var newWindow = window.open(url, 'Verify', 'scrollbars=yes, width=' + w + ', height=' + h + ', top=' + top + ', left=' + left);

    // Puts focus on the newWindow
    if (window.focus) {
        newWindow.focus();
    }
    var timer = setInterval(function () {
        if (newWindow.closed) {
            clearInterval(timer);
            $('#form-sales_lead').modal('hide');
            $('#dataTable').DataTable().ajax.reload();
        }
    }, 500);
}
// click row select input in row
$('#dataTable_state tbody').on('click', 'tr', function () {
    $(this).toggleClass('selected');
    var checkBoxes = $(this).find("input[type=checkbox]");
    checkBoxes.prop("checked", !checkBoxes.prop("checked"));
});
//disale event click input
function on_state_selected(el) {
    event.stopPropagation();
    if (el.checked) {
        $(el).closest("tr").addClass("selected");
    }
    else {
        $(el).closest("tr").removeClass("selected");
    }
}
// select all
$('#mastercheckbox-state').click(function () {
    $('.selectState').prop('checked', $(this).is(':checked')).change();
    if ($(this).is(':checked')) {
        $('#dataTable_state tbody tr').addClass('selected');
    }
    else {
        $('#dataTable_state tbody tr').removeClass('selected');
    }
});

function showModalState() {
    $('#state-search').css("visibility", "visible");
    $('#state-search').modal("show");

    $('#dataTable_state tbody tr').removeClass("selected");
    $('.selectState[type=checkbox]').prop('checked', false);
    $(".state-label").each(function () {
        if ($(this).attr("data-name") != 'All') {
            var codeState = $(this).attr("data-value");
            var elementselected = $('.' + codeState);
            elementselected.prop('checked', true);
            elementselected.parent().parent().addClass("selected");
        }
    });
}
function removestate(el) {
    $(el).parent().remove();
    if (!$('.state-label').length) {
        $('#State-div').append('<span class="label state-label" data-value="" data-name="All"  style="background-color:#21ab4a;margin-right:5px;"><span onclick="showModalState()">All </span></span>');
    }
    $(".sale-lead").DataTable().ajax.reload();
}
// submit search after select state
function submitSearchState() {
    $('#State-div').html('');
    if ($(".selectState[type=checkbox]:checked").length) {
        $(".selectState[type=checkbox]:checked").each(function () {
            var nameState = $(this).attr("data-name");
            var codeState = $(this).val();
            $('#State-div').append('<span class="label state-label" data-value="' + codeState + '" data-name="' + nameState + '"  style="background-color:#21ab4a;margin-right:5px;"><span onclick="showModalState()">' + nameState + ' </span><i onclick="removestate(this)" class="fa fa-times"></i></span>');
        });
    }
    else {
        $('#State-div').append('<span class="label state-label" data-value="" data-name="All"  style="background-color:#21ab4a;margin-right:5px;"><span onclick="showModalState()">All </span></span>');
    }
    $('#state-search').modal('hide');
    $(".sale-lead").DataTable().ajax.reload();
}

$('#mastercheckbox').click(function () {
    $('.select_membernumber').prop('checked', $(this).is(':checked')).change();
});
var trigger_event = function (type, el) {
    var e = document.createEvent("HTMLEvents")
    e.initEvent(type, false, true)
    el.dispatchEvent(e)
}

var select = document.getElementById('assign-select');
localStorage.setItem("non_selected_header", "Member");
localStorage.setItem("selected_header", "Assigned To Sales Person");
multi(select, {
    "ajax": {
        endpoint: '/SaleLead/GetListMemberAssigned',
        transform: function (res) {
            return res.data.map(function (res) {
                return {
                    value: res.Value,
                    label: res.Text,
                    //disabled: disabled,
                    //selected: selected && !disabled
                };

            });
        },
    },
    "limit": 1,
    "non_selected_header": 'Members',
    "selected_header": 'Assigned To Sales Person',
});

function LoadAssignByTeam() {
    localStorage.setItem("non_selected_header", "Team");
    localStorage.setItem("selected_header", "Assigned To Team");
    $.ajax({
        type: "Post",
        url: "/SaleLead/GetListTeamAssigned",
        success: function (res) {
            $('#assign-select').html('');
            $.each(res.data, function (id, option) {
                $('#assign-select').append($('<option></option>').val(option.Value).html(option.Text));
            });
            $('#assign-select').attr("data-type", "Team")
            trigger_event("change", select)
            $('.btn-select-assigned').removeClass('active');
            $('.btn-team').addClass('active');
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('fail');
        },
    });
}
function LoadAssignBySalesPerson() {
    localStorage.setItem("non_selected_header", "Member");
    localStorage.setItem("selected_header", "Assigned To Sales Person");
    $.ajax({
        type: "Post",
        url: "/SaleLead/GetListMemberAssigned",
        success: function (res) {
            $('#assign-select').html('');
            $.each(res.data, function (id, option) {
                $('#assign-select').append($('<option></option>').val(option.Value).html(option.Text));
            });
            $('#assign-select').attr("data-type", "SalesPerson")
            trigger_event("change", select)
            $('.btn-select-assigned').removeClass('active');
            $('.btn-salesperson').addClass('active');
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('fail');
        },
    });
}
$('.btn-assigned-outside').click(function () {
    if ($(".select_membernumber[type=checkbox]:checked").length == 0) {
        return alert('There are no row selected !');
    }
    else {
        $(".modal-select-member-outside").modal('show');
    }

})
//funtion assign
$('#btn-submit-assign-outsite').click(function () {

    var SelectedIds = [];
    $(".select_membernumber[type=checkbox]:checked").each(function () {
        SelectedIds.push($(this).val());
    });
    var SalesPerson = $("#assign-select").val();
    var Type = $("#assign-select").attr("data-type");
    $.ajax({
        method: "Post",
        url: "/SaleLead/Assigned",
        data: { "SalesLeadIds": SelectedIds, "Type": Type, "SalesPerson": SalesPerson },
        dataType: "json"
    }).done(function (data) {
        if (data) {
            noty({ "text": "assigned success", "layout": "topRight", "type": "success" });
            $(".modal-select-member-outside").modal('hide');
            $('#mastercheckbox').prop('checked', false).change();
            $('#dataTable').DataTable().ajax.reload();
        }
        else {
            alert("fail");
        }
    })
        .fail(function () {
            alert("fail");
        })
})

//function delete 
$('.btn-delete-outside').click(function () {
    if ($(".select_membernumber[type=checkbox]:checked").length == 0) {
        return alert('There are no row selected !');
    }
    var SelectedIds = [];
    $(".select_membernumber[type=checkbox]:checked").each(function () {
        SelectedIds.push($(this).val());
    });
    $.confirm({
        title: "<span class='text-secondary'>Confirm</span>",
        icon: 'fa fa-warning',
        content: 'Are you sure ?',
        type: 'blue',
        closeIcon: true,
        buttons: {
            confirm: {
                text: 'Confirm',
                btnClass: 'btn-confirm text-capitalize',
                action: function () {
                    $.ajax({
                        method: "Post",
                        url: "/SaleLead/Delete",
                        data: { "SalesLeadIds": SelectedIds },
                        dataType: "json"
                    }).done(function (data) {
                        if (data) {
                            noty({ "text": "delete success", "layout": "topRight", "type": "success" });
                            $(".modal-select-member-outside").modal('hide');
                            $('#mastercheckbox').prop('checked', false).change();
                            $('#dataTable').DataTable().ajax.reload();
                        }
                        else {
                            alert("fail");
                        }
                    })
                        .fail(function () {
                            alert("fail");
                        })
                }
            },
            cancel: {
                text: 'Cancel',
                btnClass: 'btn-cancel text-capitalize',
                action: function () {
                }
            }
        }
    });
})
//unassign
$('#btn-submit-unassign-outsite').click(function () {
    var SelectedIds = [];
    $(".select_membernumber[type=checkbox]:checked").each(function () {
        SelectedIds.push($(this).val());
    });
    $.ajax({
        method: "Post",
        url: "/SaleLead/UnAssigned",
        data: { "SalesLeadIds": SelectedIds },
        dataType: "json"
    }).done(function (data) {
        if (data) {
            noty({ "text": "UnAssigned success", "layout": "topRight", "type": "success" });
            $(".modal-select-member-outside").modal('hide');
            $('#mastercheckbox').prop('checked', false).change();
            $('#dataTable').DataTable().ajax.reload();
        }
        else {
            alert("fail");
        }
    })
        .fail(function () {
            alert("fail");
        })
})

//show detail
function showDetailLog(salesLeadId, show = true) {
    //$(".loading").show();
    //if ($(".tr-detail-" + salesLeadId).length) {
    //    if (show == false) {
    //        $(".div-detail-" + salesLeadId).slideUp(function () {
    //            $(".tr-detail-" + salesLeadId).remove();
    //        });
    //        $(".loading").hide();
    //        return;
    //    }
    //}

    //$.ajax({
    //    type: "POST",
    //    url: "/SaleLead/GetLogSalesLead",
    //    data: { "SalesLeadId": salesLeadId },
    //    success: function (data) {
    //        $(".tr-detail").not("#tr_" + salesLeadId).remove();
    //        $("#tr_" + salesLeadId).after(data);
    //        $(".div-detail-" + salesLeadId).slideDown();
    //    },
    //    error: function (xhr, ajaxOptions, thrownError) {
    //        alert('fail');
    //    },
    //    complete: function () {
    //        $(".loading").hide();
    //    },
    //});
    $(".loading").show();
    if ($("#appoiments_" + salesLeadId).is(":visible")) {
        $("#tr_" + salesLeadId).find('.icon').removeClass('dd-menu-show');
        $("tr.appoiments").remove();
        $('.loading').hide();
    } else {
        $('#List_SaleLead').find('.appoiments').remove();
        $("#img_load_" + salesLeadId).show();
        $(".appoiments").not("#appoiments_" + salesLeadId).hide().find(".list_appoi").slideUp(200);
        $.ajax({
            type: "POST",
            url: "/SaleLead/GetLogSalesLead",
            data: { "SalesLeadId": salesLeadId },
            success: function (data) {
                $("#tr_" + salesLeadId).after(data);
                $("#appoiments_" + salesLeadId).show().find(".list_appoi").slideDown(200);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('fail');
            },
            complete: function () {
                $(".loading").hide();
            },
        });
    }
}

//update row
function UpdateRow(salesLeadId, showdetail = false) {
    $.ajax({
        method: "Post",
        url: "/SaleLead/SalesLead_UpdateRow_RI",
        data: { salesLeadId },
        dataType: "json"
    }).done(function (data) {
        var numberrow = t.row('#tr_' + salesLeadId).index()
        t.row(numberrow).data(data.data);
        if (showdetail) {
            showDetailLog(salesLeadId, true);
        }
    })
        .fail(function () {
            alert("update row fail");
        });
}

$("#Team").change(function () {
    var IdTeam = $('#Team').val();
    $.ajax({
        type: "Post",
        url: "/SaleLead/GetMemberSalesPersonByTeam",
        data: { "IdTeam": IdTeam },
        success: function (data) {
            $('#Member').html('');
            $('#Member').append($('<option value="">All</option>'));
            $('#Member').append($('<option value="Unassigned">Unassigned</option>'));
            $.each(data, function (id, option) {
                $('#Member').append($('<option></option>').val(option.Id).html(option.Name));
            });
            $(".dataTable").DataTable().ajax.reload();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('fail');
        }
    });
})
$('.change-search').change(function () {
    $(".dataTable").DataTable().ajax.reload();
})

$('#assign-select').change(function () {
    var _data = $('#assign-select').val();
    if (_data.length > 0)
        $('#btn-submit-assign-outsite').removeAttr("disabled");
    else
        $('#btn-submit-assign-outsite').attr("disabled", "disabled");
})