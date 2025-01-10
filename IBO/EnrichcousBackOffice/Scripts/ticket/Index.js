let PageDefine = {
    PageDeployment: "DeploymentTicket",
    PageDevelopmentsTicket: "DevelopmentsTicket",
    PageSupportTicket: "SupportTicket",
}
let TicketSessionStorageKey = {
    SaveDataFilterDeployment: "SaveDataFilterDeployment",
    SaveDataFilterDevelopAndSupport: "SaveDataFilterDevelopAndSupport",
    Type_Deployment: "TypeDeployment",
}
let DateRangeValue = {
    ThisMonth: 1,
    LastMonth: 2,
    NearestThreeMonth: 3,
    ThisYear: 4,
    LastYear: 5,
    Custom: 6,
}

let DevelopmentLocalStorageKey = {
    ProjectId: "ProjectId",
    StageId: "StageId",
    VersionId: "VersionId",
    LengthDatatable: "LengthDatatable",
}
let TicketParamName = {
    Page: "Page",
    //FromDate: "fdate",
    //ToDate: "tdate",
    Type_Tab: "typeTicket",
    Type_Deployment: "typeDeployment",
    typeFilterDevelopment: "typeFilter",
    SearchText: "s_content",
    statusFilter: "statusFilter",
    departmentFilter: "departmentFilter",
    priorityFilter: "priorityFilter",
    severityFilter: "severityFilter",
    tagFilter: "tagFilter",
    memberFilter: "MemberFilter",
    tagMemberFilter: "TagMemberFilter",
    openByFilter: "OpenByFilter",
    salonFilter: "SalonFilter",
    licenseFilter: "LicenseFilter",
    filterAssign: "s_filter",
    closedateFilter: "closedate",
    markas_type: "markas_type",
    markas_version: "markas_version",
    markas_assign: "markas_assign",
    markas_priority: "markas_priority",
    markas_severity: "markas_severity",
    markas_label: "markas_label",
    markas_status: "markas_status",
    projectId: "projectId",
    stageId: "stageId",
    versionId: "versionId",
    changeLengthDataTable: "dataTableLength",
}

let TicketIdIndex = {
    ListTypeSelect: "#select_list_type",
    ReSelectType: "#select-again-type",
    Count_All: "#count-all",
    Count_Open: "#count-open",
    Count_Closed: "#count-closed",
    Count_Invisible: "#count-invisible",
    Count_Unassigned: "#count-unassigned",
    Wrapper_TicketFilter: "#ticket_type_select",
    WrapperCloseatFilter: "#wrapper-closeat-filter",
    Table: ".table-list-ticket",

}

let DeploymentHandle = {
    init: function () {
      
        let stageId = $('[name=' + TicketParamName.stageId + ']').val();
        if (stageId !== '') {
            let item = $(`.filter-type-item[data-key='${stageId}']`);
            $("#select-again-type").html(item.html());
            $("#select-again-type").css({ "background-color": `${item.parent("label").css("background-color")} !important` });
            $("#select-again-type").show();
        }
        let lengthDataTalbeSave = localStorage.getItem(DevelopmentLocalStorageKey.LengthDatatable);
        if (lengthDataTalbeSave !== null) {
            $("input[name=" + TicketParamName.changeLengthDataTable + "]").val(lengthDataTalbeSave);
        }
        else {
            $("input[name=" + TicketParamName.changeLengthDataTable + "]").val(10);
        }
        // filter again status

        (DeploymentHandle.LoadAllFilter()).then(function () {
          
            $('.filter-select2').select2();
            $('.filter-multiple').attr('multiple',true);
            $('.filter-multiple').selectpicker('val', []);
            let Page = $("input[name=" + TicketParamName.Page + "]").val();
            let mangoDeliveryOptionValue = "21100048";
            if (Page == PageDefine.PageDeployment) {
                $("select[name=" + TicketParamName.departmentFilter + "]").selectpicker('val', [mangoDeliveryOptionValue]);
            }
            $("select[name=" + TicketParamName.memberFilter + "]").next(".select2-container").addClass("select2-container-multi-custom").hide();
            $("select[name=" + TicketParamName.tagMemberFilter + "]").next(".select2-container").addClass("select2-container-multi-custom").hide();
            $("select[name=" + TicketParamName.salonFilter + "]").next(".select2-container").addClass("select2-container-multi-custom").hide();
            $("select[name=" + TicketParamName.openByFilter + "]").next(".select2-container").addClass("select2-container-multi-custom").hide();
            $("select[name=" + TicketParamName.licenseFilter + "]").next(".select2-container").addClass("select2-container-multi-custom").hide();
       
            DeploymentHandle.LoadTable();
            $("#date_search").on('change', function () {
                DeploymentHandle.ReloadTable();
            })

        })
        DeploymentHandle.WatchChangeParam();
    },
    // watch change param filter
    WatchChangeParam: function () {
        //change length dttable
        $("input[name=" + TicketParamName.changeLengthDataTable + "]").on('change', function () {
            let length = Number($(this).val());
            localStorage.setItem(DevelopmentLocalStorageKey.LengthDatatable, length);
            $(TicketIdIndex.Table).DataTable().page.len(length).draw();
        })

        // change type
        $("input[name=" + TicketParamName.Type_Deployment + "]").on('change', function () {
            sessionStorage.setItem(TicketSessionStorageKey.Type_Deployment, $(this).val());
            DeploymentHandle.LoadStatus();
        })

        // change type development 
        $("select[name=" + TicketParamName.typeFilterDevelopment + "]").on('change', function () {
            DeploymentHandle.ReloadTable();
        })
        // change tab
        $(".ticket-tab").on('click', function () {
            $(".ticket-tab").removeClass("btn-info active");
            $(this).addClass("btn-info active");
            var valueTab = $(this).attr("data-tab");

            if (valueTab === "closed") {
                $(TicketIdIndex.WrapperCloseatFilter).fadeIn();
                $("select[name=" + TicketParamName.statusFilter + "]").find("[group='open']").hide();
                $("select[name=" + TicketParamName.statusFilter + "]").find("[group='closed']").show();

                // update status then change tab (close,open)
                let statusVal = $("select[name=" + TicketParamName.statusFilter + "]").val();
                if (statusVal !== "") {
                    let group = $("select[name=" + TicketParamName.statusFilter + "]").find("[value ='" + statusVal + "']").attr("group");
                    if (group == "open") {
                        $("select[name=" + TicketParamName.statusFilter + "]").val("");
                    }
                }
            }
            else if (valueTab === "open") {
                $("select[name=" + TicketParamName.statusFilter + "]").find("[group='closed']").hide();
                $("select[name=" + TicketParamName.statusFilter + "]").find("[group='open']").show();
                $(TicketIdIndex.WrapperCloseatFilter).hide();

                // update status then change tab (close,open)
                let statusVal = $("select[name=" + TicketParamName.statusFilter + "]").val();
                if (statusVal !== "") {
                    let group = $("select[name=" + TicketParamName.statusFilter + "]").find("[value ='" + statusVal + "']").attr("group");
                    if (group == "closed" || group == "close") {
                        $("select[name=" + TicketParamName.statusFilter + "]").val("");
                    }
                }
            }
            else {
                $("select[name=" + TicketParamName.statusFilter + "]").find("[group='closed']").show();
                $("select[name=" + TicketParamName.statusFilter + "]").find("[group='open']").show();
                $(TicketIdIndex.WrapperCloseatFilter).hide();
            }


            $("input[name=" + TicketParamName.Type_Tab + "]").val(valueTab);
            DeploymentHandle.ReloadTable();
        })
        // change search text
        $("input[name=" + TicketParamName.SearchText + "]").on('change', function () {
            DeploymentHandle.ReloadTable();
        })
        // change from date

        //// change to date
        //$("input[name=" + TicketParamName.ToDate + "]").on('change', function () {
        //    DeploymentHandle.ReloadTable();
        //})
        // change type tab
        //$("input[name=" + TicketParamName.Type_Tab + "]").on('change', function () {
        //    sessionStorage.setItem(TicketSessionStorageKey.Type_Tab, $(this).val());
        //    DeploymentHandle.ReloadTable();
        //})
        // change filter assign
        $("select[name=" + TicketParamName.closedateFilter + "]").on('change', function () {
            DeploymentHandle.ReloadTable();
        })
        // change filter assign

        $("select[name=" + TicketParamName.filterAssign + "]").on('change', function () {
            if ($(this).val() === "assigned") {
                $("select[name=" + TicketParamName.memberFilter + "]").next(".select2-container").fadeIn();
                $("select[name=" + TicketParamName.memberFilter + "]").val($("select[name=" + TicketParamName.memberFilter + "] option:first").val()).trigger('change');

                $("select[name=" + TicketParamName.tagMemberFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.salonFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.openByFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.licenseFilter + "]").next(".select2-container").hide();
            }
            else if ($(this).val() === "membertag") {
                $("select[name=" + TicketParamName.tagMemberFilter + "]").next(".select2-container").fadeIn();
                $("select[name=" + TicketParamName.tagMemberFilter + "]").val($("select[name=" + TicketParamName.tagMemberFilter + "] option:first").val()).trigger('change');

                $("select[name=" + TicketParamName.memberFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.salonFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.openByFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.licenseFilter + "]").next(".select2-container").hide();
            }
            else if ($(this).val() === "salon") {

                $("select[name=" + TicketParamName.salonFilter + "]").next(".select2-container").fadeIn();
                $("select[name=" + TicketParamName.salonFilter + "]").val("").trigger('change');


                $("select[name=" + TicketParamName.tagMemberFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.memberFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.openByFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.licenseFilter + "]").next(".select2-container").hide();
            }
            else if ($(this).val() === "openby") {
                $("select[name=" + TicketParamName.openByFilter + "]").next(".select2-container").fadeIn();
                $("select[name=" + TicketParamName.openByFilter + "]").val($("select[name=" + TicketParamName.openByFilter + "] option:first").val()).trigger('change');

                $("select[name=" + TicketParamName.tagMemberFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.salonFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.memberFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.licenseFilter + "]").next(".select2-container").hide();
            }
            else if ($(this).val() === "license") {
                $("select[name=" + TicketParamName.licenseFilter + "]").next(".select2-container").fadeIn();
                $("select[name=" + TicketParamName.licenseFilter + "]").val("").trigger('change');

                $("select[name=" + TicketParamName.tagMemberFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.salonFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.memberFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.openByFilter + "]").next(".select2-container").hide();
            }
            else {
                $("select[name=" + TicketParamName.tagMemberFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.memberFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.salonFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.openByFilter + "]").next(".select2-container").hide();
                $("select[name=" + TicketParamName.licenseFilter + "]").next(".select2-container").hide();
                DeploymentHandle.ReloadTable();
            }

        })
        // change member number
        $("select[name=" + TicketParamName.memberFilter + "]").on('change', function () {

            DeploymentHandle.ReloadTable();
        })
        // change tag member
        $("select[name=" + TicketParamName.tagMemberFilter + "]").on('change', function () {

            DeploymentHandle.ReloadTable();
        })
        //change salon filter
        $("select[name=" + TicketParamName.salonFilter + "]").on('change', function () {

            DeploymentHandle.ReloadTable();
        })

        //change salon filter
        $("select[name=" + TicketParamName.licenseFilter + "]").on('change', function () {

            DeploymentHandle.ReloadTable();
        })
        //change open by filter
        $("select[name=" + TicketParamName.openByFilter + "]").on('change', function () {

            DeploymentHandle.ReloadTable();
        })
        //change status
        $("select[name=" + TicketParamName.statusFilter + "]").on('change', function () {

            DeploymentHandle.ReloadTable();
        })
        //change department
        $("select[name=" + TicketParamName.departmentFilter + "]").on('change', function () {

            DeploymentHandle.ReloadTable();
        })
        //change priority
        $("select[name=" + TicketParamName.priorityFilter + "]").on('change', function () {

            DeploymentHandle.ReloadTable();
        })
        //change severity
        $("select[name=" + TicketParamName.severityFilter + "]").on('change', function () {

            DeploymentHandle.ReloadTable();
        })
        //change tag
        $("select[name=" + TicketParamName.tagFilter + "]").on('change', function () {

            DeploymentHandle.ReloadTable();
        })
    },

    LoadTypeDeployment: function () {
        $.ajax({
            url: '/DeploymentTicket/LoadTicketType',
            type: 'Get',
            dataType: 'Json',
            data: {},
            success: function (types) {
                let colors = [
                    "#007bff", "#ff851b", "#17a2b8",
                    "#6610f2", "var(--main-color-1)", "#39cccc", "#28a745"
                ];
                //$(TicketIdIndex.ListTypeSelect).append($(`
                //    <label onclick="filterTypeDeployment(this)" class="btn btn-info btn-lg btn-flat filter-group-value" data-type="" data-color="${colors[0]}" style="border-color: unset; background-color: ${colors[0]} !important;">
                //       <span class="filter-type-item" data-key="">All Type</span>
                //    </label>`)
                //);
                //for (let i = 0; i < types.length; i++) {
                //    $(TicketIdIndex.ListTypeSelect).append($(`
                //    <label onclick="filterTypeDeployment(this)" class="btn btn-info btn-lg btn-flat filter-group-value" data-type="${types[i].Id}"  data-color="${colors[i + 1]}" style="border-color: unset; background-color: ${colors[i + 1]} !important;">
                //       <span class="filter-type-item" data-key="${types[i].Id}">${types[i].TypeName}</span>
                //    </label>`)
                //    );
                //}
                let type = $('[name=' + TicketParamName.Type_Deployment + ']').val();
                if (type !== '') {
                    let item = $(`.filter-type-item[data-key='${type}']`);
                    $("#select-again-type").html(item.html());
                    $("#select-again-type").css({ "background-color": `${item.parent("label").css("background-color")} !important` });
                }
            },
            error: function (res) {
                console.log(res);
            }
        });
    },
    LoadAllFilter: async function () {
        overlayOn();
        let Page = $("input[name=" + TicketParamName.Page + "]").val();

        return await $.ajax({
            url: '/Ticket_New/LoadAllTicketFilter',
            type: 'Get',
            dataType: 'Json',
            data: {
                "TypeId": $('[name=' + TicketParamName.Type_Deployment + ']').val(),
                "Page": Page
            },
            success: function (data) {
                // load status
                //$('[name=' + TicketParamName.statusFilter + ']').html(
                //    $('<option>', {
                //        value: "",
                //        text: "All Status"
                //    }));

                $('[name=' + TicketParamName.statusFilter + ']').html('');
              
                $.each(data.status, function (i, item) {
                    $('[name=' + TicketParamName.statusFilter + ']').append($('<option>', {
                        value: item.Value,
                        text: item.Text,
                        group: item.Group.Name
                    }));
                });

                if (Page != PageDefine.PageDevelopmentsTicket) {

                    $('[name=' + TicketParamName.departmentFilter + ']').html('');
                    $.each(data.departments, function (i, item) {
                        var option = $('<option>', {
                            value: item.Value,
                            text: item.Text,
                        });
                        //// add a "selected" attribute to the first option element in the dropdown list
                       
                        //if (item.Selected == true) {
                        //    console.log(item);
                        //    option.attr('selected', true);
                        //}
                        $('[name=' + TicketParamName.departmentFilter + ']').append(option);
                    });
                   
                }

                // load severity
                $('[name=' + TicketParamName.priorityFilter + ']').html('');
                $('[name=' + TicketParamName.priorityFilter + ']').append($('<option>', {
                    value: "0",
                    text: "No Priority",
                }));

                $.each(data.priorities, function (i, item) {
                    $('[name=' + TicketParamName.priorityFilter + ']').append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                        style: 'font-weight:bold;color:' + item.Color + '',
                    }));

                    // mark as
                    //$('[name=' + TicketParamName.markas_priority + ']').append($('<option>', {
                    //    value: item.Id,
                    //    text: item.Name,
                    //    style: 'font-weight:bold;color:' + item.Color + '',
                    //}));
                });


                //type
                // type for deployment (* label)
                if (Page == PageDefine.PageDeployment) {

                    if (sessionStorage.getItem(TicketSessionStorageKey.Type_Deployment) !== null) {
                        $("input[name=" + TicketParamName.Type_Deployment + "]").val(sessionStorage.getItem(TicketSessionStorageKey.Type_Deployment))
                    };
                    if (sessionStorage.getItem(TicketSessionStorageKey.SaveDataFilterDeployment) === null) {
                        $("input[name=" + TicketParamName.Type_Tab + "]").val('open');
                        $("[data-tab='open']").addClass("btn-info active");
                        $("select[name=" + TicketParamName.statusFilter + "]").find("[group='closed']").hide();
                        $("select[name=" + TicketParamName.statusFilter + "]").find("[group='open']").show();
                    }

                    $('[name=' + TicketParamName.typeFilterDevelopment + ']').html('');

                    $.each(data.types, function (i, item) {
                        $('[name=' + TicketParamName.typeFilterDevelopment + ']').append($('<option>', {
                            value: item.Text,
                            text: item.Text
                        }));
                    });
                    //let colors = [
                    //    "#007bff", "#ff851b", "#17a2b8",
                    //    "#6610f2", "var(--main-color-1)", "#39cccc", "#28a745"
                    //];
                    //$(TicketIdIndex.ListTypeSelect).append($(`
                    //<label onclick="filterTypeDeployment(this)" class="btn btn-info btn-lg btn-flat filter-group-value" data-type="" data-color="${colors[0]}" style="border-color: unset; background-color: ${colors[0]} !important;">
                    //   <span class="filter-type-item" data-key="">All Type</span>
                    //</label>`)
                    //);
                    //for (let i = 0; i < data.types.length; i++) {
                    //    $(TicketIdIndex.ListTypeSelect).append($(`
                    //<label onclick="filterTypeDeployment(this)" class="btn btn-info btn-lg btn-flat filter-group-value" data-type="${data.types[i].Value}"  data-color="${colors[i + 1]}" style="border-color: unset; background-color: ${colors[i + 1]} !important;">
                    //   <span class="filter-type-item" data-key="${data.types[i].Value}">${data.types[i].Text}</span>
                    //</label>`)
                    //    );
                    //}

                    $.each(data.status, function (i, item) {
                        $('[name=' + TicketParamName.markas_status + ']').append($('<option>', {
                            value: item.Value,
                            text: item.Text
                        }));
                    });

                    $('[name=' + TicketParamName.markas_status + ']').selectpicker("refresh");
                }

                //type develop and support
                else {
                    if (sessionStorage.getItem(TicketSessionStorageKey.SaveDataFilterDevelopAndSupport) === null) {
                        $("input[name=" + TicketParamName.Type_Tab + "]").val('open');
                        $("[data-tab='open']").addClass("btn-info active");
                        $("select[name=" + TicketParamName.statusFilter + "]").find("[group='closed']").hide();
                        $("select[name=" + TicketParamName.statusFilter + "]").find("[group='open']").show();
                    }
                    // load type filter and mark as type
                    //type filter
                    //$('[name=' + TicketParamName.typeFilterDevelopment + ']').html(
                    //    $('<option>', {
                    //        value: "",
                    //        text: "All Type"
                    //    }));
                    $('[name=' + TicketParamName.typeFilterDevelopment + ']').html('');
                  
                    $.each(data.types, function (i, item) {
                        $('[name=' + TicketParamName.typeFilterDevelopment + ']').append($('<option>', {
                            value: item.Value,
                            text: item.Text
                        }));
                    });

                    // mask as type
                    if ($('[name=' + TicketParamName.projectId + ']').val() === 'all')
                    {
                        $('[name=' + TicketParamName.markas_type + ']').append("<option value='BUG'> BUG </option><option value='REQUESTFEATURE'> REQUEST FEATURE </option>");
                    }
                    else
                    {
                        $.each(data.types, function (i, item) {
                            $('[name=' + TicketParamName.markas_type + ']').append($('<option>', {
                                value: item.Value,
                                text: item.Text
                            }));
                        });
                    }

                    $('[name=' + TicketParamName.markas_type + ']').selectpicker("refresh");
                    // mark as proj version
                    $.each(data.version, function (i, item) {
                        $('select[name=' + TicketParamName.markas_version + ']').append($('<option>', {
                            value: item.Value,
                            text: item.Text,
                        }));
                    });
                    // mask as status
                    if ($('[name=' + TicketParamName.projectId + ']').val() === 'all') {
                        $('[name=' + TicketParamName.markas_status + ']').append("<option value='OPEN'> OPEN </option><option value='CLOSED'> CLOSED </option>");
                    }
                    else {
                        $.each(data.status, function (i, item) {
                            $('[name=' + TicketParamName.markas_status + ']').append($('<option>', {
                                value: item.Value,
                                text: item.Text
                            }));
                        });
                    }
                    $('[name=' + TicketParamName.markas_status + ']').selectpicker("refresh");

                }
                //mask as assign
                $.each(data.member.filter(item => item.Text !== 'Me'), function (i, item) {
                    // mask as
                    $('[name=' + TicketParamName.markas_assign + ']').append($('<option>', {
                        value: item.Value,
                        text: "@" + item.Text,
                    }));
                });
                $('[name=' + TicketParamName.markas_assign + ']').selectpicker("refresh");


                // load priorities
                //let priorities = ["Low", "Medium", "High", "Urgent"];
                //let colorsPriorities = ["#cfcfcf", "#00acd6", "#ff8616", "#ff1e16"];
                //$.each(priorities, function (i, item) {
                //    $('[name=' + TicketParamName.priorityFilter + ']').append($('<option>', {
                //        value: item,
                //        text: item,
                //        style: 'font-weight:bold;color:' + colorsPriorities[i] + '',
                //    }));
                //});
                // load severity
                let colorsSeverity = ["#cfcfcf", "#ff8616", "#ff1e16"];
                $('[name=' + TicketParamName.severityFilter + ']').html('');
                $('[name=' + TicketParamName.severityFilter + ']').append($('<option>', {
                    value: "0",
                    text: "No Severity",
                }));

                $.each(data.severities, function (i, item) {
                    $('[name=' + TicketParamName.severityFilter + ']').append($('<option>', {
                        value: item.Id,
                        text: item.SeverityName,
                        style: 'font-weight:bold;color:' + colorsSeverity[i] + '',
                    }));

                    // mark as
                    $('[name=' + TicketParamName.markas_severity + ']').append($('<option>', {
                        value: item.Id,
                        text: item.SeverityName,
                        style: 'font-weight:bold;color:' + colorsSeverity[i] + '',
                    }));
                });
                // load tag
                $('[name=' + TicketParamName.tagFilter + ']').html('');
                $('[name=' + TicketParamName.tagFilter + ']').append($('<option>', {
                    value: "0",
                    text: "No Tags",
                }));
                $.each(data.tags, function (i, item) {
                    $('[name=' + TicketParamName.tagFilter + ']').append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                        style: 'font-weight:bold;color:' + item.Color + '',
                    }));
                    $('[name=' + TicketParamName.markas_label + ']').append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                        style: 'font-weight:bold;color:' + item.Color + '',
                    }));
                    $('[name=' + TicketParamName.markas_label + ']').selectpicker("refresh");
                });
                //load member
                $.each(data.member, function (i, item) {
                    $('select[name=' + TicketParamName.memberFilter + ']').append($('<option>', {
                        value: item.Value,
                        text: "@" + item.Text,
                    }));
                });

                $.each(data.member, function (i, item) {
                    $('select[name=' + TicketParamName.tagMemberFilter + ']').append($('<option>', {
                        value: item.Value,
                        text: "@" + item.Text,
                    }));
                });

                $.each(data.openBy, function (i, item) {
                    $('select[name=' + TicketParamName.openByFilter + ']').append($('<option>', {
                        value: item.Value,
                        text: "@" + item.Text,
                    }));
                })

                // load salon
                $.each(data.merchant, function (i, item) {
                    $('select[name=' + TicketParamName.salonFilter + ']').append($('<option>', {
                        value: item.CustomerCode,
                        text: item.BusinessName,
                    }));
                });
                // load license
                $.each(data.licenses, function (i, item) {
                    $('select[name=' + TicketParamName.licenseFilter + ']').append($('<option>', {
                        value: item.Value,
                        text: item.Text,
                    }));
                });
               // $('.filter-multiple').selectpicker('refresh');
            },
            error: function (res) {
                console.log(res);
            },
            complete: function () {
                overlayOff();
            }
        });
    },
    LoadStatus: function () {
        return $.ajax({
            url: '/DeploymentTicket/LoadTicketStatus',
            type: 'Get',
            dataType: 'Json',
            data: { "TypeId": $('[name=' + TicketParamName.Type_Deployment + ']').val() },
            success: function (status) {
                $('[name=' + TicketParamName.statusFilter + ']').html(
                    $('<option>', {
                        value: "",
                        text: "All Status"
                    }));
                $.each(status, function (i, item) {
                    $('[name=' + TicketParamName.statusFilter + ']').append($('<option>', {
                        value: item.Name,
                        text: item.Name
                    }));
                });
            },
            error: function (res) {
                console.log(res);
            }
        });
    },
    LoadPriority: function () {
        let priorities = ["Low", "Medium", "High", "Urgent"];
        let colors = ["#cfcfcf", "#00acd6", "#ff8616", "#ff1e16"];
        $.each(priorities, function (i, item) {
            $('[name=' + TicketParamName.priorityFilter + ']').append($('<option>', {
                value: item,
                text: item,
                style: 'font-weight:bold;color:' + colors[i] + '',
            }));
        });
    },
    LoadSeverity: function () {
        return $.ajax({
            url: '/DeploymentTicket/LoadTicketSeverity',
            type: 'Get',
            dataType: 'Json',
            data: {},
            success: function (severities) {
                let colors = ["#cfcfcf", "#ff8616", "#ff1e16"];
                $.each(severities, function (i, item) {
                    $('[name=' + TicketParamName.severityFilter + ']').append($('<option>', {
                        value: item.Id,
                        text: item.SeverityName,
                        style: 'font-weight:bold;color:' + colors[i] + '',
                    }));
                });
            },
            error: function (res) {
                console.log(res);
            }
        });
    },
    LoadTag: function () {
        return $.ajax({
            url: '/DeploymentTicket/LoadTicketTag',
            type: 'Get',
            dataType: 'Json',
            data: {},
            success: function (tags) {
                $.each(tags, function (i, item) {
                    $('[name=' + TicketParamName.tagFilter + ']').append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                        style: 'font-weight:bold;color:' + item.Color + '',
                    }));
                });
            },
            error: function (res) {
                console.log(res);
            }
        });
    },
    LoadListSalon: function () {
        return $.ajax({
            url: '/DeploymentTicket/LoadTicketTag',
            type: 'Get',
            dataType: 'Json',
            data: {},
            success: function (tags) {
                $.each(tags, function (i, item) {
                    $('[name=' + TicketParamName.tagFilter + ']').append($('<option>', {
                        value: item.Id,
                        text: item.Name,
                        style: 'font-weight:bold;color:' + item.Color + '',
                    }));
                });
            },
            error: function (res) {
                console.log(res);
            }
        });
    },
    LoadMemberNumber: function () {
        return $.ajax({
            url: '/DeploymentTicket/LoadTicketMember',
            type: 'Get',
            dataType: 'Json',
            data: {},
            success: function (member) {
                $.each(member, function (i, item) {
                    $('select[name=' + TicketParamName.memberFilter + ']').append($('<option>', {
                        value: item.Id,
                        text: "@" + item.Name,
                    }));
                });
            },
            error: function (res) {
                console.log(res);
            }
        });
    },
    LoadSalon: function () {
        $.ajax({
            url: '/DeploymentTicket/LoadTicketMerchant',
            type: 'Get',
            dataType: 'Json',
            data: {},
            success: function (member) {
                $.each(member, function (i, item) {
                    $('select[name=' + TicketParamName.salonFilter + ']').append($('<option>', {
                        value: item.CustomerCode,
                        text: item.BusinessName,
                    }));
                });
            },
            error: function (res) {
                console.log(res);
            }
        });
    },
    LoadTable: function () {
        table = $(TicketIdIndex.Table).DataTable({
            "language": {
                "infoFiltered": "",
                "paginate": {
                    "first": '<i class="fa fa-angle-double-left" aria-hidden="true"></i>',
                    "last": '<i class="fa fa-angle-double-right" aria-hidden="true"></i>',
                    "next": '<i class="fa fa-angle-right" aria-hidden="true"></i>',
                    "previous": '<i class="fa fa-angle-left" aria-hidden="true"></i>'
                },
                "processing": '  <svg class="circular" viewBox="25 25 50 50"><circle class="path" cx="50" cy="50" r="20" fill="none" stroke-width="2" stroke-miterlimit="10"></circle></svg>'
            },
            "serverSide": true,
            'paging': true,
            'searching': false,
            'ordering': true,
            'order': [],
            "initComplete": function (settings, json) {
                $(".time-datatable-log").html(function (index, value) {
                    let result = moment(value + ' +00').format('lll');
                    return result;
                });
            },
            //'scrollX': true,
            //'scrollCollapse': true,
            //'fixedColumns': {
            //    'left': 3,
            //},
            'info': false,
            'autoWidth': false,
            "lengthChange": false,
            "pageLength": Number($("input[name=" + TicketParamName.changeLengthDataTable + "]").val()),
            "columnDefs": [
                {
                    targets: [0],
                    width: '10px',
                    orderable: false,
                },
                {
                    targets: [1],
                    width: '10px',
                    orderable: false,
                },
                {
                    targets: [2],
                    width: '80px',
                    orderable: true,

                },
                {
                    targets: [3],
                    width: '160px',
                },
                {
                    targets: [4],
                    width: '140px',
                    className: "relative",
                },
                {
                    targets: [5],
                    width: '100px',
                },
                {
                    targets: [6],
                    width: '100px',
                },
                {
                    targets: [7],
                    className: "updated-td",
                    width: '250px',
                    orderable: false,
                },
                {
                    targets: [8],
                    width: '100px',
                },

            ],

            "processing": true,
            'fnCreatedRow': function (nRow, aData, iDataIndex) {
                $(nRow).attr('class', "tr-ticket-item");

            },

            "ajax": {
                "type": "POST",
                "url": "/Ticket_New/LoadTicketTable",
                "data": function (d) {
                    d.StageId = $("input[name=" + TicketParamName.stageId + "]").val();
                    d.VersionId = $("input[name=" + TicketParamName.versionId + "]").val();
                    d.Tab = $("input[name=" + TicketParamName.Type_Tab + "]").val();
                    d.Status = $("select[name=" + TicketParamName.statusFilter + "]").val();
                    d.Departments = $("select[name=" + TicketParamName.departmentFilter + "]").val();
                    d.Priority = $("select[name=" + TicketParamName.priorityFilter + "]").val();
                    d.Severity = $("select[name=" + TicketParamName.severityFilter + "]").val();
                    d.Tags = $("select[name=" + TicketParamName.tagFilter + "]").val();
                    //d.From = $("input[name=" + TicketParamName.FromDate + "]").val();
                    //d.To = $("input[name=" + TicketParamName.ToDate + "]").val();
                    d.From = $("#date_search").data('daterangepicker').startDate.format('YYYY-MM-DD HH:mm');
                    d.To = $("#date_search").data('daterangepicker').endDate.format('YYYY-MM-DD HH:mm"');
                    d.GMT = moment().format("Z");
                    d.FilterBy = $("select[name=" + TicketParamName.filterAssign + "]").val();
                    d.CustomerCode = $("select[name=" + TicketParamName.salonFilter + "]").val();
                    d.LicenseCode = $("select[name=" + TicketParamName.licenseFilter + "]").val();
                    d.MemberNumber = $("select[name=" + TicketParamName.memberFilter + "]").val();
                    d.MemberTag = $("select[name=" + TicketParamName.tagMemberFilter + "]").val();
                    d.OpenBy = $("select[name=" + TicketParamName.openByFilter + "]").val();
                    d.CloseAt = $("select[name=" + TicketParamName.closedateFilter + "]").val();
                    d.SearchText = $("input[name=" + TicketParamName.SearchText + "]").val();
                    d.Page = $("input[name=" + TicketParamName.Page + "]").val();
                    d.TypeDevelop = $("select[name=" + TicketParamName.typeFilterDevelopment + "]").val();
                    return d;
                },
                "dataSrc": function (json) {
                    $(TicketIdIndex.Wrapper_TicketFilter + " a.active .fill_count").html(json.recordsFiltered + "/");
                    $(TicketIdIndex.Count_All).html(json.ticketAllCount);
                    $(TicketIdIndex.Count_Open).html(json.ticketOpenCount);
                    $(TicketIdIndex.Count_Unassigned).html(json.ticketUnassignedCount);
                    $(TicketIdIndex.Count_Closed).html(json.ticketClosedCount);
                    $(TicketIdIndex.Count_Invisible).html(json.ticketinvisibleCount);

                    if (json) {
                        var rows = [];
                        let Page = $("#Page").val();
                        for (var i in json.data) {
                            //Make your callback here.
                            var Ticket = json.data[i];

                            let TicketCheckbox = `<input id="input_checkbox_${Ticket.Id}" class="type_${Ticket.StatusType}" name="checkticket" data-project="${Ticket.ProjectId}" onclick="Open_markas()" type="checkbox" value="${Ticket.Id}" />`;
                            let Date = "";
                            Date += `- Opened: ${Ticket.OpenDate}`;
                            if (Ticket.CloseDate != null) {
                                Date += `<br/>- Closed: ${Ticket.CloseDate}`;
                            }
                            Date += `<br/><i style="color:grey;font-size:0.85em">
                                ${Ticket.CloseDate != null ? "closed" : "opened"} <span>${Ticket.CloseDate != null ? Ticket.CloseDateAgo : Ticket.OpenDateAgo}</span> - by ${Ticket.CloseDate != null ? Ticket.CloseByName : Ticket.OpenByName}
                                </i><br />`;
                            let TicketId = `#...${Ticket.Id.toString().slice(6)}`;

                            let Tags = "";
                            (Ticket.Tags || '').split(`|`).forEach(function (item) {
                                try {
                                    let tag_name = item.split(`::`)[0];
                                    let tag_color = item.split(`::`)[1];
                                    let tag_id = item.split(`::`)[2];
                                    Tags += `<a data-toggle="tooltip" data-original-title="Click to filter" onclick="label_select('tags','${tag_id}',$(this).html().trim())"><span class="label" style="background-color:${tag_color}">${tag_name.replace(/\[/g, '').replace(/\]/g, '')}</span></a> `;
                                } catch { }
                            });
                            let TicketName;
                           // let stage = `?stage=${Ticket.StageId}`;
                            if (!($("[name='stageId']").val() === 'all')) {
                                stage = ''
                            }
                            TicketName = `<a href="/ticket/detail/${Ticket.Id}" onclick="overlayOn()" title="Click here to open ticket" data-toggle="tooltip">${Ticket.Name}</a><br />`
                       
                             
                            if (Ticket.CustomerId) {
                                TicketName += `<i><b>Merchant: </b> ${Ticket.CustomerName}</i><br />
                                <span style="font-size:13px">Salon Phone: ${Ticket.SalonPhone} </span><br/>
                                <span style="font-size:13px">Owner Phone: ${Ticket.OwnerPhone}</span>
                                 <br /><span style="display:inline-block;padding: 5px 8px;" class="account-manager label label-default" data-toggle="tooltip" title="update account manager"  onclick="update_merchant('${Ticket.CustomerId}', 'true')"><i class="fa fa-user" aria-hidden="true"></i>&nbsp;${Ticket.AccountManager || "N/A"}</span><br>`;
                            }

                            if (Ticket.Priority.Name != null)
                            {
                                TicketName += `<a onclick="label_select('priority','${Ticket.Priority.Id}',$(this).html().trim())" data-toggle="tooltip" data-original-title="Click to filter">
                                                <span class="label" style="background-color:${Ticket.Priority.Color}">${Ticket.Priority.Name}</span>
                                                </a>`
                            }
                            TicketName +=`<a onclick="label_select('severity','${Ticket.SeverityId}',$(this).html().trim())" data-toggle="tooltip" data-original-title="Click to filter">
                                        <span class="label label_${(Ticket.SeverityName || "").split(':')[0].toLowerCase()}">${(Ticket.SeverityName || "").split(':')[0]}</span>
                                </a>
                                ${Tags}`;

                            var deadline = "";
                            if (Ticket.Deadline != null) {
                  
                                //var _dl_of_hours = moment(Ticket.Deadline).diff(moment(), "hours", true);
                                var _dl = Ticket.DeadlineLevel;
                                var _dlname = Ticket.DeadlineText;
                                if (Ticket.StatusType == "closed" && moment(Ticket.CloseDate) < moment(Ticket.Deadline)) {
                                    deadline = `<span class='label mt-1 wrap label-default'>Deadline: ${Ticket.Deadline} - Good job</span>`;
                                } else if (_dl == -1) {
                                    deadline = `<span class='label mt-1  wrap label-danger'>Deadline: ${Ticket.Deadline} - Expired</span>`;

                                } else if (_dl == 1) {
                                    deadline = `<span class='label mt-1  wrap animate-flicker'  style="background-color: red">Deadline: ${_dlname} </span>`
                                }
                                else
                                {
                                    deadline = `<span class='label mt-1 wrap label-info'>Deadline: ${Ticket.Deadline}</span>`;
                                }
                            }
                            let TicketStatus = '';
                            Ticket.Status.split(',').forEach(function (status, index) {
                                if (status.toLowerCase() == "open" || status.toLowerCase() == "opened") {
                                    TicketStatus += `<span class='label label-status label-danger label-outlined'><span>${status || "Open"}</span></span>`;
                                }
                                else if (status.toLowerCase() == "close" || status.toLowerCase() == "closed" || status.toLowerCase() == "cancel" || status.toLowerCase() == "cancelled") {
                                    TicketStatus += `<spanclass=' label label-status label-secondary label-outlined'><span>${status}</span></span>`;
                                }
                                else {
                                    TicketStatus += `<span  class='label label-status label-primary label-outlined'><span>${status || "Open"}</span></span>`;
                                }
                                TicketStatus += `</br>`;
                            });
                            TicketStatus += `${deadline}`;
                            TicketStatus += `<span class="note-ticket" id="note-content-${Ticket.Id}">${Ticket.Note ? 'Note: ' + Ticket.Note : ""}</span>`;
                            TicketStatus += `<a onclick="UpdateNote(${Ticket.Id})" title="add note" class="btn-note-ticket"><i class="fa fa-pencil" aria-hidden="true"></i></a>`;
                            let TicketType = '';
                            if (Ticket.TypeName) {
                                Ticket.TypeName.split(',').forEach(function (type, index) {
                                    TicketType += `<span class='label-type label label-default label-outlined'>${type}</span>`;
                                });
                            }
                            let TicketAssigned = '';
                            if (Ticket.AssignMemberNumbers) {
                                let assignNames = (Ticket.AssignMemberNames || '').split(',');
                                let assignNumber = (Ticket.AssignMemberNumbers || '').split(',');
                                let assignAvatar = (Ticket.AssignMemberAvatars || '').split('|');
                                assignNames.forEach(function (memberName, index) {
                                    if (memberName != "") {
                                        TicketAssigned += `<span class="label label-default" style="border-radius: 15px; cursor: pointer; font-size: 13px; padding: 3px 5px 3px 0;" onclick="label_select_member('${assignNumber[index]}')"  data-toggle="tooltip" title="${assignNames[index]}">`;
                                        if (assignAvatar[index] != "")
                                            TicketAssigned += `<img src="${assignAvatar[index]}" onError="this.src='/Upload/Img/Male.png';" class="img-circle" alt="${assignNames[index]}" width="20" height="20" style="margin: 3px 5px 5px 0;">`;
                                        TicketAssigned += `<i>${assignNames[index]}</i>`;
                                        TicketAssigned += `</span><br/>`
                                    }
                                });
                            }
                            let LicenseName = '';

                            if (Ticket.CustomerCode != "" && Ticket.CustomerCode != null) {
                                if (Ticket.LicenseName != "" && Ticket.LicenseName != null) {
                                    LicenseName = Ticket.LicenseName;
                                    LicenseName += '<br/>';
                                    LicenseName += `<span class="license-expried">${Ticket.RemainingDate > 0 ? (Ticket.RemainingDate > 3650 ? `<span class="label label-success">Lifetime<span>`:`<span class="label label-success">${Ticket.RemainingDate} days</span>`) :'<span class="label label-danger">Expires</span>'}</span>`;
                                }
                                else
                                {
                                    LicenseName += `<span><label class="label label-default">N/A</label></span>`;
                                }
                            }
                            let TicketShareds = '';
                            if (Page == PageDefine.PageDevelopmentsTicket && Ticket.StageName) {
                                Ticket.StageName.split(',').forEach(function (item,index) {
                                    TicketShareds += `<label class="label label-warning">${item}</label> `;
                                });
                                TicketShareds += `<br/> <i><b style='font-size:.7em'>${Ticket.ProjectName} > ${Ticket.VersionName||"N/A"}</b></i>`;
                            }
                            //let TicketClosedDate = Ticket.CloseDate != null ? moment(Ticket.CloseDate).format('MMM DD, YYYY') : "";
                            //last update
                            var last = (Ticket.Updated || '').split('|').filter(i => i).slice(-1)[0];
                            let TicketUpdated = "";
                            TicketUpdated += `<section id="update-content-render-${Ticket.Id}">`;
                            if (last) {
                                TicketUpdated += `<span>${moment(last.split("-")[0] + " UTC").format('MMM DD, YYYY hh:mm A')} - ${last.split("-")[1]}</span><br/>`;
                                if (Ticket.DetailUpdate) {
                                    let htmlInPanel = '';
                                    htmlInPanel += `<span id="wrapper-update-${Ticket.Id}" class="wrapper-update">`;
                                    let updateDetail = Ticket.DetailUpdate.split("|");
                                    $.each(updateDetail, function (i, item) {
                                        console.log(item);
                                        if (i == updateDetail.length) {
                                            htmlInPanel += `<span>- ${item} </span>`;
                                        }
                                        else {
                                            htmlInPanel += `<span>- ${item} </span><br/>`;
                                        }

                                    });

                                    htmlInPanel += '</span>';
                                    htmlInPanel += `<div  class='read-more-update read-more-update_${Ticket.Id}' style='width:100%' data-id='${Ticket.Id}'> <i class='pull-right'>more...</i> <span class="content_tooltip content_tooltip_${Ticket.Id}"  style="-webkit-line-clamp: 8;z-index:9"></span></div>`;

                                    TicketUpdated += htmlInPanel;
                                }

                            }
                            TicketUpdated += "</section>";
                            var row;
                            if (Page == PageDefine.PageDevelopmentsTicket) {
                                row = [
                                    TicketCheckbox,
                                    TicketId,
                                    Date,
                                    TicketName,
                                    TicketStatus,
                                    TicketType,
                                    TicketAssigned,
                                    TicketUpdated,
                                    TicketShareds,
                                    LicenseName,
                                ];
                            }
                            else {
                                row = [
                                    TicketCheckbox,
                                    TicketId,
                                    Date,
                                    TicketName,
                                    TicketStatus,
                                    TicketType,
                                    TicketAssigned,
                                    TicketUpdated,
                                    LicenseName,
                                ];
                            }

                            rows.push(row);

                        }
                        return rows;
                    } else {
                        error(data[1]);
                        return '';
                    }
                },
                "complete": function (json) {
                    $("#list_tickets tr").has(".type_closed").addClass("strikeout");
                    show_hide_clear_btn();
                    CallMoment();
                    initToolTip();
                }
            },
            "stateSave": true,
            "stateSaveParams": function (settings, d) {
                d.Type = $("input[name=" + TicketParamName.Type_Deployment + "]").val();
                d.Tab = $("input[name=" + TicketParamName.Type_Tab + "]").val();
                d.Status = $("select[name=" + TicketParamName.statusFilter + "]").val();
                d.Departments = $("select[name=" + TicketParamName.departmentFilter + "]").val();
                d.Priority = $("select[name=" + TicketParamName.priorityFilter + "]").val();
                d.Severity = $("select[name=" + TicketParamName.severityFilter + "]").val();
                d.Tags = $("select[name=" + TicketParamName.tagFilter + "]").val();

                d.FilterBy = $("select[name=" + TicketParamName.filterAssign + "]").val();
                d.CustomerCode = $("select[name=" + TicketParamName.salonFilter + "]").val();
                d.LicenseCode = $("select[name=" + TicketParamName.licenseFilter + "]").val();
                d.MemberNumber = $("select[name=" + TicketParamName.memberFilter + "]").val();
                d.MemberTag = $("select[name=" + TicketParamName.tagMemberFilter + "]").val();
                d.OpenBy = $("select[name=" + TicketParamName.openByFilter + "]").val();
                d.SearchText = $("input[name=" + TicketParamName.SearchText + "]").val();
                d.CloseAt = $("select[name=" + TicketParamName.closedateFilter + "]").val();
                d.TypeDevelop = $("select[name=" + TicketParamName.typeFilterDevelopment + "]").val();
                let Page = $("input[name=" + TicketParamName.Page + "]").val();

                d.From = $("#date_search").data('daterangepicker').startDate.format('MMM D, YYYY');
                d.To = $("#date_search").data('daterangepicker').endDate.format('MMM D, YYYY');

                let DateRangeNumber;

                if (d.From == moment().startOf('month').format('MMM D, YYYY') && d.To == moment().endOf('month').format('MMM D, YYYY')) {
                    DateRangeNumber = DateRangeValue.ThisMonth;
                }
                else if (d.From == moment().subtract(1, 'month').startOf('month').format('MMM D, YYYY') && d.To == moment().subtract(1, 'month').endOf('month').format('MMM D, YYYY')) {
                    DateRangeNumber = DateRangeValue.LastMonth;
                }
                else if (d.From == moment().subtract(3, 'month').startOf('month').format('MMM D, YYYY') && d.To == moment().format('MMM D, YYYY')) {
                    DateRangeNumber = DateRangeValue.NearestThreeMonth;
                }
                else if (d.From == moment().startOf('year').format('MMM D, YYYY') && d.To == moment().endOf('year').format('MMM D, YYYY')) {
                    DateRangeNumber = DateRangeValue.ThisYear;
                }
                else if (d.From == moment().subtract(1, 'year').startOf('year').format('MMM D, YYYY') && d.To == moment().subtract(1, 'year').endOf('year').format('MMM D, YYYY')) {
                    DateRangeNumber = DateRangeValue.LastYear;
                }
                else {
                    DateRangeNumber = DateRangeValue.Custom;
                }
                d.DateRangeNumber = DateRangeNumber;

                if (Page == PageDefine.PageDeployment) {
                    sessionStorage.removeItem(TicketSessionStorageKey.SaveDataFilterDeployment);
                    sessionStorage.setItem(TicketSessionStorageKey.SaveDataFilterDeployment, JSON.stringify(d));
                }
                else {
                    sessionStorage.removeItem(TicketSessionStorageKey.SaveDataFilterDevelopAndSupport);
                    sessionStorage.setItem(TicketSessionStorageKey.SaveDataFilterDevelopAndSupport, JSON.stringify(d));
                }

            },
            "stateLoadParams": function (settings, data) {
                let Page = $("input[name=" + TicketParamName.Page + "]").val();
                let stateSaveData;

                if (Page == PageDefine.PageDeployment) {
                    stateSaveData = sessionStorage.getItem(TicketSessionStorageKey.SaveDataFilterDeployment);
                }
                else {
                    stateSaveData = sessionStorage.getItem(TicketSessionStorageKey.SaveDataFilterDevelopAndSupport);
                }

                if (stateSaveData) {
                    let data = JSON.parse(stateSaveData);

                    //load from date
                    //$("input[name=" + TicketParamName.FromDate + "]").val(data.From);
                    ////load to date
                    //$("input[name=" + TicketParamName.ToDate + "]").val(data.To);
                    //load type

                    if (data.DateRangeNumber == DateRangeValue.ThisMonth) {
                        $('#date_search').data('daterangepicker').setStartDate(moment().startOf('month'));
                        $('#date_search').data('daterangepicker').setEndDate(moment().endOf('month'));
                    }
                    else if (data.DateRangeNumber == DateRangeValue.LastMonth) {
                        $('#date_search').data('daterangepicker').setStartDate(moment().subtract(1, 'month').startOf('month'));
                        $('#date_search').data('daterangepicker').setEndDate(moment().subtract(1, 'month').endOf('month'));
                    }
                    else if (data.DateRangeNumber == DateRangeValue.NearestThreeMonth) {
                        $('#date_search').data('daterangepicker').setStartDate(moment().subtract(3, 'month').startOf('month'));
                        $('#date_search').data('daterangepicker').setEndDate(moment());
                    }
                    else if (data.DateRangeNumber == DateRangeValue.ThisYear) {
                        $('#date_search').data('daterangepicker').setStartDate(moment().startOf('year'));
                        $('#date_search').data('daterangepicker').setEndDate(moment().endOf('year'));
                    }
                    else if (data.DateRangeNumber == DateRangeValue.LastYear) {
                        $('#date_search').data('daterangepicker').setStartDate(moment().subtract(1, 'year').startOf('year'));
                        $('#date_search').data('daterangepicker').setEndDate(moment().subtract(1, 'year').endOf('year'));
                    }
                    else {
                        $('#date_search').data('daterangepicker').setStartDate(data.From);
                        $('#date_search').data('daterangepicker').setEndDate(data.To);
                    }


                    $(".ticket-tab").removeClass("btn-info active");
                    if (data.Tab !== "" && data.Tab !== null) {
                        $("input[name=" + TicketParamName.Type_Tab + "]").val(data.Tab);
                        $("[data-tab=" + data.Tab + "]").addClass("btn-info active");
                        if (data.Tab === "closed") {
                            $(TicketIdIndex.WrapperCloseatFilter).show();
                            $("select[name=" + TicketParamName.statusFilter + "]").find("[group='open']").hide();
                            $("select[name=" + TicketParamName.statusFilter + "]").find("[group='closed']").show();
                        }
                        else if (data.Tab === "open") {
                            $("select[name=" + TicketParamName.statusFilter + "]").find("[group='closed']").hide();
                            $("select[name=" + TicketParamName.statusFilter + "]").find("[group='open']").show();
                        }
                    }
                    else {
                        $("input[name=" + TicketParamName.Type_Tab + "]").val('open');
                        $("[data-tab='open']").addClass("btn-info active");
                        $("select[name=" + TicketParamName.statusFilter + "]").find("[group='closed']").hide();
                        $("select[name=" + TicketParamName.statusFilter + "]").find("[group='open']").show();
                    }
                    //load search text
                    $("input[name=" + TicketParamName.SearchText + "]").val(data.SearchText)

                    //load assign
                    let value = data.FilterBy;
                    $("select[name=" + TicketParamName.filterAssign + "]").val(value);
                    if (value === "assigned") {
                        $("select[name=" + TicketParamName.memberFilter + "]").next(".select2-container").fadeIn();

                    }
                    else if (value === "membertag") {
                        $("select[name=" + TicketParamName.tagMemberFilter + "]").next(".select2-container").fadeIn();

                    }
                    else if (value === "salon") {
                        $("select[name=" + TicketParamName.salonFilter + "]").next(".select2-container").fadeIn();

                    }
                    else if (value === "license") {
                        $("select[name=" + TicketParamName.licenseFilter + "]").next(".select2-container").fadeIn();

                    }
                    else if (value === "openby") {
                        $("select[name=" + TicketParamName.openByFilter + "]").next(".select2-container").fadeIn();

                    }
                    // load close at
                    $("select[name=" + TicketParamName.closedateFilter + "]").val(data.CloseAt);
                    //load member number
                    $("select[name=" + TicketParamName.memberFilter + "]").val(data.MemberNumber);
                    $("select[name=" + TicketParamName.memberFilter + "]").trigger("change.select2");

                    //load member tag
                    $("select[name=" + TicketParamName.tagMemberFilter + "]").val(data.MemberTag);
                    $("select[name=" + TicketParamName.tagMemberFilter + "]").trigger("change.select2");
                    //open by
                    $("select[name=" + TicketParamName.openByFilter + "]").val(data.OpenBy);
                    $("select[name=" + TicketParamName.openByFilter + "]").trigger("change.select2");
                    //load salon
                    $("select[name=" + TicketParamName.salonFilter + "]").val(data.CustomerCode);
                    $("select[name=" + TicketParamName.salonFilter + "]").trigger("change.select2");

                    //load license
                    $("select[name=" + TicketParamName.licenseFilter + "]").val(data.LicenseCode);
                    $("select[name=" + TicketParamName.licenseFilter + "]").trigger("change.select2");
                    //load status
                    $("select[name=" + TicketParamName.statusFilter + "]").val(data.Status);
                    //load department
                    $("select[name=" + TicketParamName.departmentFilter + "]").val(data.Departments);
                    //load priority
                    $("select[name=" + TicketParamName.priorityFilter + "]").val(data.Priority);
                    //load severity
                    $("select[name=" + TicketParamName.severityFilter + "]").val(data.Severity);
                    //load tag
                    $("select[name=" + TicketParamName.tagFilter + "]").val(data.Tags);
                    //load type for develop
                    $("select[name=" + TicketParamName.typeFilterDevelopment + "]").val(data.TypeDevelop);
                    $('.filter-multiple').selectpicker('refresh');
                }
                else {
                    $("input[name=" + TicketParamName.Type_Tab + "]").val('open');
                    $("[data-tab='open']").addClass("btn-info active");
                    $("select[name=" + TicketParamName.statusFilter + "]").find("[group='closed']").hide();
                    $("select[name=" + TicketParamName.statusFilter + "]").find("[group='open']").show();

                }
                return data;
            },


        });
    },
    ReloadTable: function () {
        $(TicketIdIndex.Table).DataTable().ajax.reload();
    },


}
DeploymentHandle.init();
function filterTypeDeployment(el) {
    let key = $(el).attr("data-type");
    let color = $(el).attr("data-color");
    let name = $(el).find("span").text();
    $("input[name=" + TicketParamName.stageId + "]").val(key).trigger("change");
    $(TicketIdIndex.ReSelectType).css("background-color", color);
    $(TicketIdIndex.ReSelectType).text(name);
    $(TicketIdIndex.ListTypeSelect).removeClass("flex");
    $(".fill-over").hide();
    DeploymentHandle.ReloadTable();
}

function ClearStateDatatable() {
    var activeRequestsTable = $('.dataTable').DataTable();
    activeRequestsTable.page('first').state.save();
}

function CallMoment() {
    $(".entry-time-ticket-feedback").html(function (index, value) {
        let date = moment(value + ' +00').format('lll');
        return date;
    });
    $(".time-ticket").html(function (index, value) {
        let now = moment().toDate();
        let ticketDate = moment(value).toDate();
        let result = timeDifference(now, ticketDate)
        return result;
    });
}

function timeDifference(current, previous,showAgo=true) {
    var msPerMinute = 60 * 1000;
    var msPerHour = msPerMinute * 60;
    var msPerDay = msPerHour * 24;
    var msPerMonth = msPerDay * 30;
    var msPerYear = msPerDay * 365;

    var elapsed = current - previous;
    let showSecond = true;
    let showMinute = true;

    let html = "";
    if (elapsed >= msPerYear) {
        let year = Math.floor(elapsed / msPerYear);
        if (year > 0) {
            html += year + (year == 1 ? " year " : " years ");
            elapsed = elapsed - (year * msPerYear);
            showMinute = false;
            showSecond = false;
        }
    }
    if (elapsed < msPerYear) {
        let month = Math.floor(elapsed / msPerMonth);
        if (month > 0) {
            html += month + (month == 1 ? " month " : " months ");
            elapsed = elapsed - (month * msPerMonth);
            showMinute = false;
            showSecond = false;
        }

    }
    if (elapsed < msPerMonth) {
        let day = Math.floor(elapsed / msPerDay);
        if (day > 0) {
            html += day + (day == 1 ? " day " : " days ");
            elapsed = elapsed - (day * msPerDay);
            showSecond = false;
        }

    }
    if (elapsed < msPerDay) {
        let hour = Math.floor(elapsed / msPerHour);
        if (hour > 0) {
            html += hour + (hour == 1 ? " hour " : " hours ");
            elapsed = elapsed - (hour * msPerHour);
            showSecond = false;
        }
    }
    if (elapsed < msPerHour && showMinute) {
        let minute = Math.floor(elapsed / msPerMinute);
        if (minute > 0) {
            html += minute + (minute == 1 ? " minute " : " minutes ");
            elapsed = elapsed - (minute * msPerMinute);
        }
    }
    if (elapsed < msPerMinute && showSecond) {
        let second = Math.floor(elapsed / 1000);
        html += second + (second == 1 ? " second " : " seconds ");
        elapsed = elapsed - (second * 1000);
    }
    if (showAgo) {
        html += " ago"
    }
    return html;
}




function remainingLicenseDate(current) {
    let now = moment().toDate();
    var msPerMinute = 60 * 1000;
    var msPerHour = msPerMinute * 60;
    var msPerDay = msPerHour * 24;
    var msPerMonth = msPerDay * 30;
    var msPerYear = msPerDay * 365;
    var elapsed = current - now;


    let html = "";
    if (elapsed > 0) {
        let day = Math.floor(elapsed / msPerDay);
        if (day > 0 && day < 3650) {
            html += '<label class="label label-success">' + day + (day == 1 ? " day " : " days ") + '</label>';

        }
        else if (day >= 3650) {
            html += '<label class="label label-success">Lifetime</label>';
        }
        else {
            html = '<label class="label label-danger">Expires</label>';
        }
    }
    else {
        html = '<label class="label label-danger">Expires</label>';
    }
    return html;

}
function monthDiff(d1, d2) {
    var months;
    months = (d2.getFullYear() - d1.getFullYear()) * 12;
    months -= d1.getMonth();
    months += d2.getMonth();
    return months <= 0 ? 0 : months;
}
function clear_table_filter() {
    $("#TicketFilter select").val('0');
}

function label_select_member(id) {
    //clear_table_filter();
    if ($('select[name=MemberFilter]').find("option[value=" + id + "]").length == 0) {
        $('select[name=MemberFilter]').append("<option value='" + id + "'></option>")
    }
    $('select[name=s_filter]').val("assigned");
    $('select[name=MemberFilter]').val(id).trigger('change');
    $('select[name=MemberFilter]').next(".select2-container").fadeIn();
}


function clear_table_filter() {
    $("#TicketFilter select").val('0');
    $('.filter-multiple').selectpicker('refresh');
}
function clear_filter() {
    $("select[name=typeFilter]").val("");
    $("select[name=statusFilter]").val("");
    $("select[name=priorityFilter]").val("");
    $("select[name=severityFilter]").val("");
    $("select[name=tagFilter]").val("");
    $("input[name=s_content]").val("");
    $("select[name=s_filter]").val("").trigger("change");
    $('.filter-multiple').selectpicker('refresh');

}
//function diff_hours(dt2, dt1) {

//    var diff = (dt2.getTime() - dt1.getTime()) / 1000;
//    diff /= (60 * 60);
//    return Math.abs(Math.round(diff));

//}
function exportExcel() {
    $("#exportExcelBtn").attr("disabled", true);
    $("#exportExcelBtn").find(".loading").show();
    $.ajax({
        url: '/Ticket_New/ExportExcel',
        type: 'POST',
        dataType: 'Json',
        data: {
            StageId : $("input[name=" + TicketParamName.stageId + "]").val(),
            VersionId : $("input[name=" + TicketParamName.versionId + "]").val(),
            Tab: $("input[name=" + TicketParamName.Type_Tab + "]").val(),
            Status: $("select[name=" + TicketParamName.statusFilter + "]").val(),
            Departments: $("select[name=" + TicketParamName.departmentFilter + "]").val(),
            Priority: $("select[name=" + TicketParamName.priorityFilter + "]").val(),
            Severity: $("select[name=" + TicketParamName.severityFilter + "]").val(),
            Tags: $("select[name=" + TicketParamName.tagFilter + "]").val(),
            From: $("#date_search").data('daterangepicker').startDate.format('YYYY-MM-DD'),
            To: $("#date_search").data('daterangepicker').endDate.format('YYYY-MM-DD'),
            FilterBy: $("select[name=" + TicketParamName.filterAssign + "]").val(),
            CustomerCode: $("select[name=" + TicketParamName.salonFilter + "]").val(),
            LicenseCode: $("select[name=" + TicketParamName.licenseFilter + "]").val(),
            MemberNumber: $("select[name=" + TicketParamName.memberFilter + "]").val(),
            MemberTag: $("select[name=" + TicketParamName.tagMemberFilter + "]").val(),
            OpenBy: $("select[name=" + TicketParamName.openByFilter + "]").val(),
            CloseAt: $("select[name=" + TicketParamName.closedateFilter + "]").val(),
            SearchText: $("input[name=" + TicketParamName.SearchText + "]").val(),
            Page: $("input[name=" + TicketParamName.Page + "]").val(),
            TypeDevelop: $("select[name=" + TicketParamName.typeFilterDevelopment + "]").val(),

        },
        success: function (data) {
            if (data.status == true) {
                window.location.href = "/Ticket_New/DownloadExcelFile?path=" + data.path;

            }
        },
        error: function (res) {
            console.log(res);
        },
        complete: function (data) {
            $("#exportExcelBtn").attr("disabled", false);
            $("#exportExcelBtn").find(".loading").hide();
        }
    });
}
function UpdateNote(TicketId) {

    $.ajax({
        url: '/Ticket_New/GetNoteByTicketId',
        type: 'Get',
        dataType: 'Json',
        data: { TicketId },
        success: function (data) {
            if (data.status) {
                $.confirm({
                    title: `<span style="font-size:16px;"><i class="fa fa-pencil-square-o"></i> Ticket #${TicketId}</span>`,
                    content: '' +

                        '<form action="" class="formName">' +
                        '<div class="form-group">' +
                        '<label>Note: </label>' +
                        '<textarea rows="4" placeholder="Note" class="note form-control">' + (data.Note || "") + '</textarea>' +
                        '</div>' +
                        '</form>',
                    buttons: {
                        formSubmit: {
                            text: 'Save',
                            btnClass: 'btn-blue',
                            action: function () {
                                var note = this.$content.find('.note').val();
                                $.ajax({
                                    url: '/Ticket_New/NoteSave',
                                    type: 'Post',
                                    dataType: 'Json',
                                    data: { 'TicketId': TicketId, 'Note': note },
                                    success: function (data) {
                                        if (data.status) {
                                            noty({ "text": "update note success", "layout": "topRight", "type": "success" });
                                            $(`#note-content-${TicketId}`).html(note ? 'Note: ' + note : "");
                                            UpdateLog(TicketId);
                                        }
                                        else {
                                            $.alert(data.message);
                                        }
                                    },
                                    error: function (res) {
                                        alert('oops! something went wrong.');
                                        console.log(res);
                                    }
                                });
                            }
                        },
                        cancel: function () {
                            //close
                        },
                    },
                    onContentReady: function () {
                        // bind to events
                        var jc = this;
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });

            }
            else {
                error(data.mesage);
            }
        },
        error: function (res) {
            console.log(res);
        }
    });
}

//update row
function UpdateLog(TicketId) {
    $.ajax({
        method: "Post",
        url: "/Ticket_New/GetLastUpdate",
        data: { TicketId },
        dataType: "json"
    }).done(function (data) {
        if (data) {
            var last = (data.Updated || '').split('|').filter(i => i).slice(-1)[0];
            let TicketUpdated = '';
            TicketUpdated = `<span>${moment(last.split("-")[0] + " UTC").format('MMM DD, YYYY hh:mm A')} - ${last.split("-")[1]}</span><br/>`
            let htmlInPanel = '';
            htmlInPanel += `<span id="wrapper-update-${TicketId}" class="wrapper-update">`;
            let updateDetail = data.DetailUpdate.split("|");
            $.each(updateDetail, function (i, item) {

                if (i == updateDetail.length) {
                    htmlInPanel += '<span class="">- ' + item + '</span>'
                }
                else {
                    htmlInPanel += '<span class="">- ' + item + '</span><br/>'
                }
            });
            htmlInPanel += '</span>';
            htmlInPanel += `<div  class='read-more-update read-more-update_${TicketId}' style='width:100%' data-id='${TicketId}'> <i class='pull-right'>more...</i> <span class="content_tooltip content_tooltip_${TicketId}"  style="-webkit-line-clamp: 8;z-index:9"></span></div>`;
            TicketUpdated += htmlInPanel;
            $(`#update-content-render-${TicketId}`).html(TicketUpdated);
            //  convertTimeLastUpdate();
        }
    })
        .fail(function () {
            alert("update log fail");
        });

}

function update_merchant(_id, _update) {
    overlayOn();
    $.ajax({
        method: "POST",
        url: "/merchantman/GetMerchantInfo",
        data: { id: _id, update: _update },
        dataType: "html"
    })
        .done(function (data) {
            $("#merchant_popup").html(data);
            $("#modal-merchant").modal('show');
            //$('a[href="#account_manager"]').trigger('click');
        })
        .fail(function () {
            alert("Oops! Something went wrong");
            $("#modal-merchant").modal('hide');
        })
        .always(function () {
            //$("#loading").hide();
            overlayOff();
        });
}

function SaveComplete(data) {
    //event.preventDefault();
    if (data[0] == true) {
        $('#modal-merchant').modal('toggle');
        DeploymentHandle.ReloadTable();
        //var url = $("input[name='hd_url']").val();
        //window.location.href = url;
    }
    else {
        var Error = { "text": data[1], "layout": "top", "type": "error" };
        noty(Error);
    }

}

//function convertTimeLastUpdate() {
//    $(".time-line-moment-label").html(function (index, value) {
//        let result = moment(value).format('ll');
//        return result;
//    });
//    $(".detail-timeline-update-time").html(function (index, value) {
//        let result = moment.utc(value).local().format('lll');
//        return result;
//    });
//    $(".detail-timeline-update-estimated").html(function (index, value) {
//        let result = moment(value + ' +00').local().format('lll');
//        return result;
//    });

//}

