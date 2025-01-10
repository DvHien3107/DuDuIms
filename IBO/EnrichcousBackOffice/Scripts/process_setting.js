//Add process
function add_process() {
    var _type = $("select[name='process_type']").val();
    var _name = $("input[name='process_name']").val();
    var _ischeck = $("select[name='process_check']").val()=="1";
    var _requirement = $("input[name='process_requirement']").is(":checked");

    if (_name == "" || _name == null) {
        var MsgError = $.parseJSON('{"text":"Field Name may not be empty.", "layout":"topRight", "type":"error"}');
        noty(MsgError);
    }
    else {
        $.ajax({
            method: "POST",
            url: "/device/AddProcess",
            data: {
                _Type: _type,
                _Name: _name,
                _Requirement: _requirement,
                _ischeck: _ischeck
            },
            dataType: "json"
        })
            .done(function (data) {
                //data: object[] {[true/false], [new_process/msg_error]}
                if (data[0] == true) {
                    var _content = '<tr id="tr_' + data[1].Id + '"><td style="display:none"><select class="form-control" name="process_type_' + data[1].Id + '" disabled>';
                    _content += '<option selected value="Packaging">Preparation</option></select></td>';
                    //'<option value="Testing">Testing</option>'+
                    //'<option value="Shipping">Shipping</option></td>';
                    _content += '<td><input type="text" class="form-control" name="process_name_' + data[1].Id + '" value="' + data[1].FieldName + '" disabled/></td>';
                    _content += '<td><input type="checkbox"' + (data[1].Requirement ? 'checked' : '') + ' name="process_requirement_' + data[1].Id + '" style="height:20px; width:20px" disabled/></td>';
                    _content += '<td><select name="process_check_' + data[1].Id + '" class="form-control" disabled><option value="1" ' + (data[1].IsCheck ? 'selected' : '') + '>Checkbox</option><option value="0" ' + (!data[1].IsCheck ? 'selected' : '') + '>Text field</option></select></td>';
                    _content += '<td><button type="button" id="btn_u_' + data[1].Id + '" class="btn btn-default btn-sm" onclick="update_process(\'' + data[1].Id + '\')"><i class="fa fa-edit"></i> Update</button>&nbsp;&nbsp;' +
                        '<button type="button" id="btn_s_' + data[1].Id + '" class="btn btn-default btn-sm" onclick="save_process(\'' + data[1].Id + '\')" style="display:none"><i class="fa fa-save"></i> Save</button>&nbsp;&nbsp;' +
                        '<button type="button" class="btn btn-default btn-sm" onclick="delete_process(\'' + data[1].Id + '\')"><i class="fa fa-close"></i> Remove</button></td></tr>';

                    $("#tbody").append(_content);
                    var MsgSuccess = $.parseJSON('{"text":"Add new process success!", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);

                    $('select[name="process_type_' + data[1].Id + '"]').val(data[1].FieldType);
                    $("select[name='process_type']").val("Packaging");
                    $("input[name='process_name']").val("");
                    $("input[name='process_check']").prop("checked", false);
                    $("input[name='process_requirement']").prop("checked", false);
                }
                else {
                    var MsgError = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(MsgError);
                }
            })
            .fail(function () {
                alert("Oops! Something went wrong");
            })
    }
}

//click button update process
function update_process(_process_id) {
    $("select[name='process_type_" + _process_id + "']").prop('disabled', false);
    $("input[name='process_name_" + _process_id + "']").prop('disabled', false);
    $("select[name='process_check_" + _process_id + "']").prop('disabled', false);
    $("input[name='process_requirement_" + _process_id + "']").prop('disabled', false);
    $("#btn_u_" + _process_id).hide('alow');
    $("#btn_s_" + _process_id).show('alow');
}

//update process
function save_process(_process_id) {
    var _type = $("select[name='process_type_" + _process_id + "']").val();
    var _name = $("input[name='process_name_" + _process_id + "']").val();
    var _requirement = $("input[name='process_requirement_" + _process_id + "']").is(":checked");
    var _ischeck = $("select[name='process_check_" + _process_id + "']").val()=="1";

    if (_name == "" || _name == null) {
        var MsgError = $.parseJSON('{"text":"Field Name may not be empty.", "layout":"topRight", "type":"error"}');
        noty(MsgError);
    }
    else {
        $.ajax({
            method: "POST",
            url: "/device/SaveProcess",
            data: {
                _Id: _process_id,
                _Type: _type,
                _Name: _name,
                _Requirement: _requirement,
                _ischeck: _ischeck
            },
            dataType: "json"
        })
            .done(function (data) {
                //data: object[] {[true/false], [process/msg_error]}
                if (data[0] == true) {
                    $("select[name='process_type_" + _process_id + "']").val(data[1].FieldType);
                    $("input[name='process_name_" + _process_id + "']").val(data[1].FieldName);
                    $("select[name='process_check_" + _process_id + "']").val(data[1].IsCheck?"1":"0");
                    $("input[name='process_requirement_" + _process_id + "']").prop("checked", data[1].Requirement);

                    $("select[name='process_type_" + _process_id + "']").prop('disabled', true);
                    $("input[name='process_name_" + _process_id + "']").prop('disabled', true);
                    $("select[name='process_check_" + _process_id + "']").prop('disabled', true);
                    $("input[name='process_requirement_" + _process_id + "']").prop('disabled', true);
                    $("#btn_s_" + _process_id).hide('alow');
                    $("#btn_u_" + _process_id).show('alow');

                    var MsgSuccess = $.parseJSON('{"text":"Update process success!", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);
                }
                else {
                    var MsgError = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(MsgError);
                }
            })
            .fail(function () {
                alert("Oops! Something went wrong");
            })
    }
}

//delete process
function delete_process(_process_id) {
    if (confirm("You want to delete this process?")) {
        $.ajax({
            method: "POST",
            url: "/device/DeleteProcess",
            data: { _Id: _process_id },
            dataType: "json"
        })
            .done(function (data) {
                if (data[0] == true) {
                    $("#tr_" + _process_id).remove();

                    var MsgSuccess = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);
                }
                else {
                    var MsgError = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                    noty(MsgError);
                }
            })
            .fail(function () {
                alert("Oops! Something went wrong");
            })
    }
}

//search process
function search_process() {
    var _type = $("#s_process_type").val();
    var _name_search = $("input[name='search_process']").val();

    $.ajax({
        method: "POST",
        url: "/device/SearchProcess",
        data: {
            Type: _type,
            SearchName: _name_search
        },
        dataType: "json"
    })
        .done(function (data) {
            //data: object[] {[true/false], [list_prcess/msg_error]}
            if (data[0] == true) {
                var _content = "";
                $("#tbody").empty();

                if (data[1] != "" && data[1].length > 0) {


                    for (var i = 0; i < data[1].length; i++) {
                        _content += '<tr id="tr_' + data[1][i].Id + '"><td style="display:none"><select class="form-control" name="process_type_' + data[1][i].Id + '" disabled>';
                        _content += '<option selected value="Packaging">Preparation</option></select></td>';
                        //'<option value="Testing">Testing</option>'+
                        //'<option value="Shipping">Shipping</option></select></td>';
                        _content += '<td><input type="text" class="form-control" name="process_name_' + data[1][i].Id + '" value="' + data[1][i].FieldName + '" disabled/></td>';

                        _content += '<td><input type="checkbox"' + (data[1][i].Requirement ? 'checked' : '') + ' name="process_requirement_' + data[1][i].Id + '" style="height:20px; width:20px" disabled/></td>';
                        _content += '<td><input type="checkbox" ' + (data[1][i].IsCheck ? 'checked' : '') + ' name="process_check_' + data[1][i].Id + '" style="height:20px; width:20px" disabled /></td>';
                        _content += '<td><button type="button" id="btn_u_' + data[1][i].Id + '" class="btn btn-default btn-sm" onclick="update_process(\'' + data[1][i].Id + '\')"><i class="fa fa-edit"></i> Update</button>&nbsp;&nbsp;' +
                            '<button type="button" id="btn_s_' + data[1][i].Id + '" class="btn btn-default btn-sm" onclick="save_process(\'' + data[1][i].Id + '\')" style="display:none"><i class="fa fa-save"></i> Save</button>&nbsp;&nbsp;' +
                            '<button type="button" class="btn btn-default btn-sm" onclick="delete_process(\'' + data[1][i].Id + '\')"><i class="fa fa-close"></i> Remove</button></td></tr>';
                    }

                    $("#tbody").append(_content);
                    for (var i = 0; i < data[1].length; i++) {
                        $('select[name="process_type_' + data[1][i].Id + '"]').val(data[1][i].FieldType);
                    }
                    var MsgSuccess = $.parseJSON('{"text":"Found ' + data[1].length + ' field(s)", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);
                }
                else {
                    var MsgSuccess = $.parseJSON('{"text":"Found 0 field", "layout":"topRight", "type":"success"}');
                    noty(MsgSuccess);
                }
            }
            else {
                var MsgError = $.parseJSON('{"text":"' + data[1] + '", "layout":"topRight", "type":"error"}');
                noty(MsgError);
            }
        })
        .fail(function () {
            alert("Oops! Something went wrong");
        })
};