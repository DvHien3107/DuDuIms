
function select_product(id, qty) {
    alert("a");
    $.ajax({
        method: "POST",
        url: "/order/GetProduct",
        data: { id, qty },
        dataType: "json"
    })
        .done(function (data) {
            if ($("#subs_" + data.SubscriptionId).length == 0) {
                var price = numberToMoney_USD(data.Price);
                if (data.SubscriptionDuration) {
                    price = numberToMoney_USD(data.Price) + " /" + data.SubscriptionDuration;
                }
                var row = `<tr id="subs_${data.SubscriptionId}" class="${data.Type == "license" ? "selected_subscription" : "selected_addon"}"><td></td>
                   <td><div class="row" bis_skin_checked="1"><div class="col-md-3" bis_skin_checked="1"> <img src="/Content/Img/noimage.png" class="img-thumbnail" style="height:75px">
                   </div><div class="col-md-9" bis_skin_checked="1"><button type="button" class="btn btn-xs btn-default pull-right" style="cursor:pointer" onclick="if(confirm(\'Are you sure to unselect ${data.SubscriptionName}?\')){$(\'#pr_${data.SubscriptionId}\').trigger(\'click\')}">
                   <i class="glyphicon glyphicon-trash"></i>
                   </button><b style="color:dodgerblue">${data.SubscriptionName}</b><br>
                   </td>
                   <td>${data.SubscriptionDuration == "MONTHLY" ? '<select id="qty_' + data.SubscriptionId + '" onchange=select_product("' + data.SubscriptionId + '",this.value) class="form-control"><option value="1">monthly</option><option value="3">per 3 months</option><option value="6">per 6 months</option><option value="12">Annually</option></select>' : '1'}</td>
                   <td><b style="color:#258e4f">${price}</b></td>
                   <td><b style="color:#258e4f" class="subs_amount">${numberToMoney_USD(data.Amount)}</b></td></tr>`;
                $("#list_product_order").append(row);
                $("#qty_" + data.SubscriptionId).val(data.Quantity);
            }
            else {
                $("#subs_" + data.SubscriptionId).find(".subs_amount").html(numberToMoney_USD(data.Amount));
            }
            if (data.Type == "license") {
                $(".selected_subscription").hide();
            }
            if (qty == 0) {
                $("#subs_" + data.SubscriptionId).hide();
            } else {
                $("#subs_" + data.SubscriptionId).show();
            }
            refresh_index_count();
            change_money();
        })
        .fail(function () {
        })
        .always(function () {
        });

}
function get_prd_license_item(prd_id, is_product = true, qty = 1) {
    if (!$("#pr_" + prd_id).find('.check').is(":checked")) {
        var order_code = $("input[name=order_code]").val();
        $.ajax({
            method: "POST",
            url: "/Order/getPrdLicenseItemList",
            data: { prd_id, order_code },
            dataType: "html"
        })
            .done(function (data) { 
                $("#prdata_" + prd_id).html(data);
                $("#pr_" + prd_id).find(".icon_span").toggle();

                if ($("#prdata_" + prd_id).html().trim().length) {
                    $("#pr_" + prd_id).next("tr").show(100);
                }
                $("#pr_" + prd_id).find('input').prop("checked", true);
                if (is_product) {
                    $(".prd_collapse.actived").find(".icon_span").toggle();
                    $(".prd_collapse.actived").removeClass("actived").next("tr").hide();
                }
                $("#pr_" + prd_id).toggleClass("actived");
               
                // change_money();
                select_product(prd_id, qty);
                if (is_product) {
                    $("#renewal").removeAttr("disabled").closest(".form-control").css("background-color", "");
                }
            })
            .fail(function () {
                alert("get_prd_license_item failure");
            })
            .always(function () {
            });
        //var period = $("#prd_period_" + prd_id).html().trim();
        //if (!first_load && period != "Onetime" && period != null) {
        //    if (period == "DAILY")
        //        $("#period_show").html("Days");
        //    if (period == "MONTHLY")
        //        $("#period_show").html("Months");
        //    if (period == "QUARTERLY")
        //        $("#period_show").html("Quanters");

        //        $('#start_date').datepicker("setDate", $("[name=InvoiceDate]").datepicker("getDate") || new Date());

        //    $('#start_date').trigger("change");
        //    $("#prd_date").val(prd_id);
        //    $("#select_date").modal("show");

        //}
    } else {
        $("#pr_" + prd_id).find(".icon_span").toggle();
        if ($("#prdata_" + prd_id).html().trim().length) {
            $("#pr_" + prd_id).next("tr").toggle(100);
        }
        $("#pr_" + prd_id).toggleClass("actived");
        $("#pr_" + prd_id).find('input').prop("checked", false);
        select_product(prd_id, 0);
        change_money();
        if (is_product) {
            $("#renewal").prop("checked", false).attr("disabled", "disabled").closest(".form-control").css("background-color", "#ddd");
        }
    }
    
}
function refresh_index_count() {
    var i = 1;
    $("#list_product_order tr:visible").each(function () {
        $(this).find("td").first().html(i++);
    });
}
function change_subs_qty(a, b) {
    console.log(a + " " + b);
}