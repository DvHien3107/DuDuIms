
let TicketTranferParamName = {
    Project: "projectId",
    MainVersion: "versionId",
    Stages: "stage",
    Department: "department",
    AssignMember: "assign",
}

let TicketTranferHandle = {
    init: function () {
     
        // filter again status
        TicketTranferHandle.WatchChangeParam();
    },
    // watch change param filter
    WatchChangeParam: function () {
        $(document).ready(function () {
        //change project
            $("#TransferForm select[name=" + TicketTranferParamName.Project + "]").on('change', function () {
             
                TicketTranferHandle.UpdateInterface(this);
                TicketTranferHandle.LoadVersion();
                TicketTranferHandle.LoadStages();
                TicketTranferHandle.LoadAssignMember();
              
        })
          $("#TransferForm select[name=" + TicketTranferParamName.Department + "]").on('change', function () {
              TicketTranferHandle.LoadAssignMember();
        })
           
            
        });
    },
    UpdateInterface: function (el) {
             let BuildInCode = $(el).find(":selected").attr("buildincode");
            if (BuildInCode == "Support_Ticket" || BuildInCode == "Deployment_Ticket") {
                if (BuildInCode == "Support_Ticket") {
                    $("#transfer-modal [name='department'] option[data-type='SUPPORT']").show();
                    $("#transfer-modal [name='department'] option[data-type='DEPLOYMENT']").hide();
                }
                else {
                    $("#transfer-modal [name='department'] option[data-type='DEPLOYMENT']").show();
                    $("#transfer-modal [name='department'] option[data-type='SUPPORT']").hide();
                }
                $('#transfer-modal .department-wrapper').show();
                $(".stage-transfer-wrapper").hide();

            }
            else {
                $('#transfer-modal .department-wrapper').hide();
                $(".stage-transfer-wrapper").show();

            }
    },
    LoadVersion: function () {
       
       $.ajax({
            url: '/Ticket_New/LoadVersion',
            type: 'Post',
            dataType: 'Json',
            data: {
                projectId: $("#TransferForm select[name=" + TicketTranferParamName.Project + "]").val(),
            },
           success: function (data) {
               debugger;
                $("#TransferForm select[name=" + TicketTranferParamName.MainVersion + "]").empty();
                $.each(data, function (i, item) {
                    $("#TransferForm select[name=" + TicketTranferParamName.MainVersion + "]").append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                        // selected: typeSelectedIds != "" ? typeSelectedIds.include(item.value):false
                    }));
                });
                $("#TransferForm select[name=" + TicketTranferParamName.MainVersion + "]").trigger("change");
            },
            error: function (res) {
                console.log(res);
               
            }
        });
    },
    LoadStages: function () {
         $.ajax({
            url: '/Ticket_New/GetStages',
            type: 'Post',
            dataType: 'Json',
            data: {
                projectId: $("#TransferForm select[name=" + TicketTranferParamName.Project + "]").val(),
            },
            success: function (data) {
                $("#TransferForm select[name=" + TicketTranferParamName.Stages + "]").empty();
                $.each(data, function (i, item) {
                    $("#TransferForm select[name=" + TicketTranferParamName.Stages + "]").append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                        selected: i == 0 ? true : false
                    }));
                });
                $("#TransferForm select[name=" + TicketTranferParamName.Stages + "]").trigger("change");
            },
            error: function (res) {
                console.log(res);
            }
        });
    },
   

   
    LoadAssignMember: function () {
   
        var projectId = $("#TransferForm select[name=" + TicketTranferParamName.Project + "]").val();
        if (projectId == '') {
         
            return;
        }
        overlayOn();
       
        var Page = $("#Page").val();
        var departmentId = $("#TransferForm select[name=" + TicketTranferParamName.Department + "]").val();
        $.ajax({
            method: "POST",
            url: "/Ticket_New/getStageMember",
            data: { projectId, "Page": Page, departmentId },
            dataType: "json"
        })
            .done(function (data) {
                if (data.status) {
                    // reset member
                    $("#TransferForm select[name=" + TicketTranferParamName.AssignMember + "]").html('');
                    $("#TransferForm select[name=" + TicketTranferParamName.AssignMember + "]").append("<option value='auto'>Assign to Leader Automation</option>");
                    $.each(data.member, function (i, item) {
                        $("#TransferForm select[name=" + TicketTranferParamName.AssignMember + "]").append($('<option>', {
                            value: item.MemberNumber,
                            text: item.MemberName,
                            'data-avatar': item.Avatar == "" || item.Avatar == null ? "/Upload/Img/" + item.Gender + ".png" : item.Avatar,
                          
                        }))
                    })
                }
                else {
                    error(data.message);
                }
            })
            .fail(function () {
                alert("Oops! Something went wrong. getStageMember failure");
            })
            .always(function () {
                overlayOff();
               
            });



    }

}
TicketTranferHandle.init();


function showTranferTicket(TicketId) {

    CKEDITOR.instances['transfer_note'].setData('')
    $.ajax({
        method: "POST",
        url: "/Ticket_New/GetProjectTranfer",
        data: { TicketId },
        dataType: "json"
    })
        .done(function (data) {
            let projectEl = $("#transfer-modal").find('[name="projectId"]');
            $(projectEl).html('');
            $(projectEl).append('<option value="">N/A</option>');
            $.each(data.filter(p => p.Id !== $("#Project_select").val()), function (i, item) {
                $(projectEl).append($('<option>', {
                    value: item.Id,
                    text: item.Name,
                    buildincode: item.BuildInCode,
                }));
            });
           // $("#transfer-modal").find('[name="projectId"]').trigger("change");
            $("#transfer-modal").modal('show');
        })
        .fail(function () {
            alert("Oops! Something went wrong");
        })
        .always(function () {
        });
}