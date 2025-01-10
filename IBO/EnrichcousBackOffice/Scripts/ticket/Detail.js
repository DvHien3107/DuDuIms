let TicketDetailParamName = {
    Project: "Project_select",
    MainVersion: "ProjectVersion",
    Stages: "Stages",
    FixedVersion: "FixedVersion",
    AffectedVersion: "AffectedVersion",
    Types: "type",
    StatusId: "StatusId",
    Department: "department",
    OtherDepartment: "Other_Department",
    AssignMember: "Assigned",

}
let PageDefine = {
    PageDeployment: "DeploymentTicket",
    PageDevelopmentsTicket: "DevelopmentsTicket",
    PageSupportTicket: "SupportTicket",
}

let TicketDetailHandle = {
    init: function () {
        // filter again status
        TicketDetailHandle.WatchChangeParam();
    },
    // watch change param filter
    WatchChangeParam: function () {
        $(document).ready(function () {
        //change project
         $("#MainFormTicket select[name=" + TicketDetailParamName.Project + "]").on('change', function () {
            TicketDetailHandle.LoadVersion();
            TicketDetailHandle.LoadStages();
            TicketDetailHandle.LoadAssignMember();
            TicketDetailHandle.LoadTypeByProject();
            TicketDetailHandle.LoadStatusByProject();
        })
            $("#MainFormTicket select[name=" + TicketDetailParamName.Department + "]").on('change', function () {
            TicketDetailHandle.LoadAssignMember();
        })
            $("#MainFormTicket select[name=" + TicketDetailParamName.OtherDepartment + "]").on('change', function () {
            TicketDetailHandle.LoadAssignMember();
        })
            $("#MainFormTicket select[name=" + TicketDetailParamName.MainVersion + "]").on('change', function () {
            TicketDetailHandle.LoadSubVersion();
        })
        });
    },
    LoadVersion: function () {
       $.ajax({
            url: '/Ticket_New/LoadVersion',
            type: 'Post',
            dataType: 'Json',
            data: {
                projectId: $("#MainFormTicket select[name=" + TicketDetailParamName.Project + "]").val(),
            },
            success: function (data) {
                $("#MainFormTicket select[name=" + TicketDetailParamName.MainVersion + "]").empty();
                $.each(data, function (i, item) {
                    $("#MainFormTicket select[name=" + TicketDetailParamName.MainVersion + "]").append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                        // selected: typeSelectedIds != "" ? typeSelectedIds.include(item.value):false
                    }));
                });
                $("#MainFormTicket select[name=" + TicketDetailParamName.MainVersion + "]").trigger("change");
            },
            error: function (res) {
                console.log(res);
            }
        });
    },
    LoadSubVersion: function () {
        var VersionId = $("#MainFormTicket select[name=" + TicketDetailParamName.MainVersion + "]").val();
        $.ajax({
            method: "POST",
            url: "/Ticket_New/LoadSubVersion",
            data: { 'VersionId': VersionId },
            dataType: "json"
        })
            .done(function (data) {
                $("#MainFormTicket #available_versions,#fixed_versions").html('');
                $.each(data, function (i, item) {
                    $("#available_versions,#fixed_versions").append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                    }));
                });
                $("#MainFormTicket #available_versions,#fixed_versions").trigger('change.select2');
            })
            .fail(function () {

            })
            .always(function () {

            });
    },

    LoadStages: function () {
         $.ajax({
            url: '/Ticket_New/GetStages',
            type: 'Post',
            dataType: 'Json',
            data: {
                projectId: $("#MainFormTicket select[name=" + TicketDetailParamName.Project + "]").val(),
            },
            success: function (data) {
                $("#MainFormTicket select[name=" + TicketDetailParamName.Stages + "]").empty();
                $.each(data, function (i, item) {
                    $("#MainFormTicket select[name=" + TicketDetailParamName.Stages + "]").append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                        // selected: typeSelectedIds != "" ? typeSelectedIds.include(item.value):false
                    }));
                });
                $("#MainFormTicket select[name=" + TicketDetailParamName.Stages + "]").trigger("change");
            },
            error: function (res) {
                console.log(res);
            }
        });
    },
    LoadTypeByProject: function () {
        $('#loading_type').show();
       // var typeSelectedIds = $("select[name=" + TicketDetailParamName.Types + "]").val();
        $.ajax({
            method: "POST",
            url: "/Ticket_New/LoadTicketType",
            data: {
                projectId: $("#MainFormTicket select[name=" + TicketDetailParamName.Project + "]").val(),
            },
            dataType: "json"
        })
            .done(function (data) {
                $("#MainFormTicket select[name=" + TicketDetailParamName.Types + "]").empty();
                $.each(data, function (i, item) {
                    $("#MainFormTicket select[name=" + TicketDetailParamName.Types + "]").append($('<option>', {
                        value: item.Value,
                        text: item.Text,
                       //selected: typeSelectedIds != "" ? typeSelectedIds.include(item.value):false
                    }));
                });
                $("#MainFormTicket select[name=" + TicketDetailParamName.Types + "]").trigger("change");
            })
            .fail(function () {
                alert("Oops! load type failure");
            })
            .always(function () {
                $("#loading_type").hide();
            });
    },

    LoadStatusByProject: function () {
        $("#loading_status").show();
      //  var StatusSelectedId = $("select[name=" + TicketDetailParamName.StatusId + "]").val();
        $.ajax({
            method: "POST",
            url: "/Ticket_New/LoadTicketStatus",
            data: {
                projectId: $("#MainFormTicket select[name=" + TicketDetailParamName.Project + "]").val(),
            },
            dataType: "json"
        })
            .done(function (data) {
                $("#MainFormTicket select[name=" + TicketDetailParamName.StatusId + "]").empty();
                $.each(data, function (i, item) {
                    $("#MainFormTicket select[name=" + TicketDetailParamName.StatusId + "]").append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                       // selected: StatusSelectedId == item.value 
                    }));
                });
                $("#MainFormTicket select[name=" + TicketDetailParamName.StatusId + "]").trigger("change");
            })
            .fail(function () {
                alert("Oops! load type failure");
            })
            .always(function () {
                $("#loading_status").hide();
            });
    },
    LoadAssignMember: function () {
   
        var projectId = $("#MainFormTicket select[name=" + TicketDetailParamName.Project + "]").val();
        if (projectId == '') {
         
            return;
        }
        overlayOn();
        var AssignMemberSelected = $("#MainFormTicket select[name=" + TicketDetailParamName.AssignMember + "]").val();
        var Page = $("#Page").val();
        var departmentId = $("#MainFormTicket select[name=" + TicketDetailParamName.Department + "]").val();
        var otherDepartmentId = $("#MainFormTicket select[name=" + TicketDetailParamName.OtherDepartment + "]").val();
        $.ajax({
            method: "POST",
            url: "/Ticket_New/getStageMember",
            data: { projectId, "Page": Page, departmentId, otherDepartmentId },
            dataType: "json"
        })
            .done(function (data) {
                if (data.status) {
                    // reset member
                    $("#MainFormTicket select[name=" + TicketDetailParamName.AssignMember + "]").html('');
                    $("#MainFormTicket select[name=" + TicketDetailParamName.AssignMember + "]").append("<option value='auto'>Assign to Leader Automation</option>");
                    $.each(data.member, function (i, item) {
                        $("#MainFormTicket select[name=" + TicketDetailParamName.AssignMember + "]").append($('<option>', {
                            value: item.MemberNumber,
                            text: item.MemberName,
                            'data-avatar': item.Avatar == "" || item.Avatar == null ? "/Upload/Img/" + item.Gender + ".png" : item.Avatar,
                            selected: ((Page == PageDefine.PageDeployment || Page == PageDefine.PageSupportTicket) && AssignMemberSelected != null) ? AssignMemberSelected.includes(item.MemberNumber):false
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
TicketDetailHandle.init();


