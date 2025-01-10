$('.select2').select2();
$(".datepicker").datepicker({
    changeMonth: true,
    changeYear: true,
    yearRange: '1950:2050'
});
$("#form-delete-demo-scheduler").off("submit").on("submit", function (e) {
    e.preventDefault(); // avoid to execute the actual submit of the form.
    var form = $(this);
    var url = form.attr('action');
    var loading = form.find('.loading');
    $(loading).show();
    $.ajax({
        type: "POST",
        url: url,
        data: { "Id": form.find("[name='Id']").val() }, // serializes the form's elements.
        success: function (data) {
            if (data.status) {
                $("#modal-delete-demo-scheduler").modal("hide");
                let MsgSuccess = $.parseJSON('{"text":"delete success !", "layout":"topRight", "type":"success"}');
                noty(MsgSuccess);
                $('.demo-scheduler').DataTable().ajax.reload();
            }
            else {
                let MsgError = $.parseJSON('{"text":"' + data.message + '", "layout":"topRight", "type":"error"}');
                noty(MsgError);
            }

        },
        complete: function () {
            $(loading).hide();
        },
    });


});


var t = $('.demo-scheduler').DataTable({
    "language": {
        "processing": '<img src="/Content/ajax-loaders/loading2.gif" width="60px" />',
    },
    "pageLength": 10,
    "serverSide": true, // for process server side
    "processing": true, // for show progress bar
    'paging': true,
    'lengthChange': true,
    'searching': false,
    'ordering': true,
    'info': false,
    'autoWidth': true,
    'stateSave': false,
    "ajax": {
        "url": "/DemoScheduler/GetListDemoScheduler",
        "type": "POST",
        "datatype": "json",
        data: function (data) {
            data.From = $("#FromSearch").val(),
                data.To = $("#ToSearch").val(),
                data.Member = $("#MemberSearch").val(),
                data.SearchText = $("#SalonText").val(),
                data.Status = $("#Status_Tab").val()
            return data;
        },
    },
    'fnCreatedRow': function (nRow, aData, iDataIndex) {
        $(nRow).attr('id', 'tr_' + aData.Id.toString());
        $(nRow).attr('data-Id', aData.Id.toString());
    },
    "columns": [
        {
            name: 'UpdateAt',
            visible: true,
            searchable: false,
            render: function (data, type, row, meta) {
                return '<i>' + moment(data).format("MMM DD YYYY <br/> h:mm:ss A") + '</i>';
            },
            data: 'UpdateAt',
            "className": 'open-detail',
        },
        {
            name: 'BusinessName',
            visible: true,
            searchable: false,
            render: function (data, type, row, meta) {
                var html = '';
                html += '<b class="text text-success">' + data + '</b>';
                html += '<br/>'
                html += '<span>' + row["Address"] + '</span>'
                return html;
            },
            "render": function (data, type, row) {
                var html = '<span style="color:grey"><b  class="text-success">' + row["BusinessName"] + '</b></span>';
                html += '<a class="on_rows" style="cursor:pointer;margin-left:5px;transform: translateY(2px);display: inline-block;" onclick="update_merchant(this,0,\'' + row["CustomerCode"] + '\')" title="" data-toggle="tooltip" data-original-title="View detail"><i class="glyphicon glyphicon-eye-open"></i><img class="loading-view-merchant" src="/Content/ajax-loaders/ajax-loader-1.gif" style="display:none"></a><br/>';
                html += '<span style="color:grey">Contact: <b class="text-primary"> ' + (row["OwnerName"] ?? 'N/A') + '</b> </span><br/>';
                html += '<span style="color:grey">Phone: <b class="text-info"> ' + (row["CellPhone"] ?? 'N/A') + '</b> </span><br/>';

                html += '</div>';
                return html;
            },
            data: 'BusinessName',
            "className": 'open-detail',
        },

        {
            "name": "Note",
            "data": "Note",
            "className": 'open-detail',
        },
        {
            "name": "Assign",
            "data": "Assign",
            "orderable": false,
            "className": 'open-detail',
            "render": function (data, type, row, meta) {
                var html = '';
                if (row["Assign"] !== '' && row["Assign"] !== null) {
                    var arr = row["Assign"].split(',');
                    $.each(arr, function (index, value) {
                        html += "<label style='margin-right:3px;' class='label label-primary'>" + value + "</label>";
                    });

                }

                return html;/*'<span class="btn btn-info btn-sm dropdown-toggle btn-action" style="padding: 3px 15px; outline: none;">Detail <i class="icon fa fa-angle-right" aria-hidden="true" ></i ></span >';*/
            },
        },
        {
            "name": "NextEvent",
            "className": 'open-detail',
            "orderable": false,
            "render": function (data, type, row, meta) {
                var html = '';
                if (row["NextStartTime"] !== null && row["NextStartTime"] !== "") {
                    html += '<div>';
                    html += '<span>' + moment(row["NextStartTime"]).format("MMM DD YYYY, h:mm:ss A") + '</span><br/>';

                    html += '</div>';
                }

                return html;/*'<span class="btn btn-info btn-sm dropdown-toggle btn-action" style="padding: 3px 15px; outline: none;">Detail <i class="icon fa fa-angle-right" aria-hidden="true" ></i ></span >';*/
            },
        },
        //{
        //    "name": "AttendeesName",
        //    "data": "AttendeesName",
        //    "className": 'open-detail',
        //},
        {
            "name": "Status",
            "className": 'open-detail',
            "orderable": false,
            "render": function (data, type, row, meta) {
                var html = '';
                if (row["Status"] == 1) {
                    html += '<label class="label label-success">Completed</label>';
                }
                else if (row["Status"] == 0) {
                    html += '<label class="label label-danger">Cancel</label>';
                }
                else {
                    html += '<label class="label label-default">Demo Scheduler</label>';
                }

                return html;
            },
        },
        {
            "name": "Action",
            "orderable": false,
            "render": function (data, type, row, meta) {
                var html = '<div class="btn-group">';
                html += '<button type="button" style="padding: 5px" onclick="showDetail(\'' + row['Id'] + '\')" class="btn btn-info btn-sm dropdown-toggle btn-action"><i class="icon icon-detail fa fa-plus" aria-hidden="true" ></i >  <img class="loading" src="/Content/ajax-loaders/ajax-loader-1.gif" style="display:none"></button>';
                html += '</div>';
                return html;/*'<span class="btn btn-info btn-sm dropdown-toggle btn-action" style="padding: 3px 15px; outline: none;">Detail <i class="icon fa fa-angle-right" aria-hidden="true" ></i ></span >';*/
            },
            "className": 'align-middle text-center',
            "width":"20px"
        },
    ]
});
$('.select2').select2();
$('.change-search').off("change").on("change", function () {
    $(".demo-scheduler").DataTable().ajax.reload();
})
$("#create-demo-scheduler-btn").off("click").on("click", function () {
    var loading = $(this).find('.loading');
    $(loading).show();
    $.ajax({
        method: "POST",
        url: "/DemoScheduler/ShowPopUpCreateOrUpdate",
        dataType: "html"
    })
        .done(function (data) {
            $("#render-popup-cru").html(data);
            $("#modal-cru-demo-scheduler").modal("show");
        })
        .fail(function () {
            alert("Oops! Something went wrong.");
        }).always(function () {
            $(loading).hide();
        });
})