
function save_subtask(key, sub_id) {
   // var sub_name = $("input[name='sub_name_" + sub_id + "']").val();
    //var sub_complete = $('#chk_' + sub_id).is(":checked");
    //if (sub_name == "" || sub_name == null) {
    //    var optionsErr = $.parseJSON('{"text":"Sub task name is required","layout":"topRight","type":"error"}');
    //    noty(optionsErr);
    //}
    //else {

        //        //data: object[] {[true/false], [key('update'/'addnew')/message error], [subtask_complete], [percent_complete], [list_member]}
        //        debugger;
        //        if (data[0] == true) {

        if (key == "addnew") { //Add subtask
            //$("input[name='sub_name_" + sub_id + "']").attr('onchange', 'save_subtask(\'update\', ' + sub_id + ')');
            //$('#chk_' + sub_id).attr('onchange', 'save_subtask(\'update\', ' + sub_id + ')');

            //$("#btn_add_" + sub_id).hide("500");
            //$("#btn_del_" + sub_id).show("500");

            $.ajax({
                method: "POST",
                url: "/tasksman/NewSubtask",
                dataType: "html"

            })
                .done(function (data) {
                    $('#subtask_body').append(data);
                    $('input[name^=sub_name_]')[$('input[name^=sub_name_]').length - 1].focus();
                    refresh_progress_bar();
                })

            //$('.subtask-' + sub_number).focus();

        }
        //else { //Update subtask
        //    //$('#_noty').html('<i class="fa fa-check-square" style="color:#00a65a"></i> ' + data[2]);
        //    refresh_progress_bar();
        //}
   // }
}

function detete_sub(sub_id) {
    $('#div_subtask_' + sub_id).remove();
    refresh_progress_bar();    
}

function refresh_progress_bar() {
    debugger;
    let count_all = $('.check_completed').length;
    let count_done = $('.check_completed:checked').length;
    let percen = (count_all == 0) ? $("#complete_task:not(:checked)").length?0: 100 : parseInt(count_done / count_all * 100);
    $('#_percent').html('<b>' + percen + '%</b>');
    $('#progress_bar').css('width', percen + '%');

    if (percen == "100") {
        $("#complete_task:not(:checked)").iCheck('check');
    }
    else {
        $("#complete_task:checked").iCheck('uncheck');
    }
    $('#_noty .count_all').text(count_all == 0 ? 1 : count_all);
    $('#_noty .count_done').text(count_done == 0 ? 1 : count_done);
}

