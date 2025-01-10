function LoadData(key) {
    if (key == "milestone") {
        $("#milestone_loading").show();
    }

    let _data = JSON.parse(sessionStorage.getItem("_ProjectMilestone"));
    if (_data == null) {
        GetList_ProjectMilestone(key);
    }
    else {
        if (key == "project") {
            let _optPro = '<option value="">None yet</option>';
            for (var i = 0; i < _data.length; i++) {
                if (_data[i].Type == "project") {
                    _optPro += '<option value="' + _data[i].Id + '">' + _data[i].Name + '</option>';
                }
            }
            $("#project").html(_optPro);
            $("#project_loading").hide();
        }
        else {
            let projectId = $("#project").val();

            let _optMile = '<option value="">No milestone</option>';
            for (var i = 0; i < _data.length; i++) {
                if (_data[i].Type == "milestone" && _data[i].ParentId == projectId) {
                    _optMile += '<option value="' + _data[i].Id + '">' + _data[i].Name + '</option>';
                }
            }
            $("#milestone").html(_optMile);
            $("#milestone_loading").hide();
        }
    }
};

function GetList_ProjectMilestone() {
    $.post("/development/GetProjectMilestone", function (data) {
        //data: [{true/false}, {list_ProjectMilestone/error_msg}]
        if (data[0] == true) {
            //luu du lieu vao sesstion
            sessionStorage.setItem("_ProjectMilestone", JSON.stringify(data[1]));


            let _optPro = '<option value="">None yet</option>';
            for (var i = 0; i < data[1].length; i++) {
                if (data[1][i].Type == "project") {
                    _optPro += '<option value="' + data[1][i].Id + '">' + data[1][i].Name + '</option>';
                }
            }
            $("#project").html(_optPro);
            $("#project").val($("#ticket_projectId").val());

            $("#project_loading").hide();

            let projectId = $("#project").val();

            let _optMile = '<option value="">No milestone</option>';
            for (var i = 0; i < data[1].length; i++) {
                if (data[1][i].Type == "milestone" && data[1][i].ParentId == projectId) {
                    _optMile += '<option value="' + data[1][i].Id + '">' + data[1][i].Name + '</option>';
                }
            }

            $("#milestone").html(_optMile);
            $("#milestone").val($("#ticket_milestoneId").val());

            $("#milestone_loading").hide();

        }
        else {
            alert("Oops! " + data[1]);
            $("#project_loading").hide();
            $("#milestone_loading").hide();
        }
    }).fail(function () {
        alert("Oops! Something went wrong");
    });
};

/**save product */
function SaveProject(key) {
    //key: add|edit
    let pro_id = $("#project").val();
    let pro_name = "";

    if (key == "edit" && (pro_id == "" || pro_id == null)) {
        let optError = $.parseJSON('{"text":"Please select Project","layout":"topRight","type":"error"}');
        noty(optError);
    }
    else {
        if (pro_id != "" && pro_id != null && key == "edit") {
            pro_name = prompt("Project name:*", $("#project option:selected").text());
        }
        else {
            pro_name = prompt("Project name:*", "");
        }

        if (pro_name != "" && pro_name != null) {
            $("#project_loading").show();

            $.post("/development/SaveProject", { key: key, id: pro_id, name: pro_name }, function (data) {
                //data: [{true/false}, {list_ProjectMilestone/error_msg}]
                $("#project_loading").hide();

                if (data[0] == true) {
                    sessionStorage.setItem("_ProjectMilestone", JSON.stringify(data[1]));

                    let _optPro = '<option value="">None yet</option>';
                    for (var i = 0; i < data[1].length; i++) {
                        if (data[1][i].Type == "project") {
                            _optPro += '<option value="' + data[1][i].Id + '">' + data[1][i].Name + '</option>';
                        }
                    }

                    $("#project").html(_optPro);
                    $("#project").val(pro_name.replace(/ /g, "_"));
                    if (key == "add") {
                        $("#milestone").html('<option value="">No milestone</option>');
                    }

                    let optSuccess = $.parseJSON('{"text":"Save success","layout":"topRight","type":"success"}');
                    noty(optSuccess);
                }
                else {
                    let optError = $.parseJSON('{"text":"' + data[1] + '","layout":"topRight","type":"error"}');
                    noty(optError);
                }
            });
        }
        else {
            if (pro_name != null) {
                let optError = $.parseJSON('{"text":"Project name is required.","layout":"topRight","type":"error"}');
                noty(optError);
            }
        }
    }
}

/**save milestone */
function SaveMilestone(key) {
    //key: add|edit
    let pro_id = $("#project").val();
    let mile_id = $("#milestone").val();
    let mile_name = "";

    if (key == "edit" && (mile_id == "" || mile_id == null)) {
        let optError = $.parseJSON('{"text":"Please select Milestone","layout":"topRight","type":"error"}');
        noty(optError);
    }
    else {
        if (pro_id == "" || pro_id == null) {
            let optError = $.parseJSON('{"text":"Please select Project","layout":"topRight","type":"error"}');
            noty(optError);
        }
        else {
            if (mile_id != "" && mile_id != null && key == "edit") {
                mile_name = prompt("Milestone name:*", $("#milestone option:selected").text());
            }
            else {
                mile_name = prompt("Milestone name:*", "");
            }
            
            if (mile_name != "" && mile_name != null) {
                if (pro_id == "" || pro_id == null) {
                    let optError = $.parseJSON('{"text":"Please select Project","layout":"topRight","type":"error"}');
                    noty(optError);
                }
                else {
                    $("#milestone_loading").show();

                    $.post("/development/SaveMilestone", { key: key, projectId: pro_id, id: mile_id, name: mile_name }, function (data) {
                        //data: [{true/false}, {list_ProjectMilestone/error_msg}]
                        $("#milestone_loading").hide();
                        if (data[0] == true) {
                            sessionStorage.setItem("_ProjectMilestone", JSON.stringify(data[1]));

                            let _optMile = '<option value="">No milestone</option>';
                            for (var i = 0; i < data[1].length; i++) {
                                if (data[1][i].Type == "milestone" && data[1][i].ParentId == pro_id) {
                                    _optMile += '<option value="' + data[1][i].Id + '">' + data[1][i].Name + '</option>';
                                }
                            }

                            $("#milestone").html(_optMile);
                            $("#milestone").val(mile_name.replace(/ /g, "_"));

                            let optSuccess = $.parseJSON('{"text":"Save success","layout":"topRight","type":"success"}');
                            noty(optSuccess);
                        }
                        else {
                            let optError = $.parseJSON('{"text":"' + data[1] + '","layout":"topRight","type":"error"}');
                            noty(optError);
                        }
                    });
                }
            }
            else {
                if (mile_name != null) {
                    let optError = $.parseJSON('{"text":"Milestone name is required.","layout":"topRight","type":"error"}');
                    noty(optError);
                }
            }
        }
    }
}