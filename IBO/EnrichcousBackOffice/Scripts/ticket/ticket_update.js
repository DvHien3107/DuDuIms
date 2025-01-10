//import { fail } from "assert";

///**
//       * load ticket type
//       * */
//function loadTicketType() {
//    $("#loading_type").show();


//    $.ajax({
//        method: "POST",
//        url: "/ticket/LoadTicketType",
//        data: {},
//        dataType: "json"
//    })
//        .done(function (data) {
//            $("select[name=type]").empty();
//            var optcontent = $("#template_promo").html();
//            for (var i = 0; i < data.length; i++) {
//                addcontent = addcontent.replace(/\{VALUE\}/gim, data[i].Id);
//                addcontent = addcontent.replace(/\{NAME\}/gim, data[i].TypeName);
//                $("select[name=type]").append(optcontent);
//            }
           

//        })
//        .fail(function () {
//            console.log("Oops! Something went wrong. Merchant update failure");
//        })
//        .always(function () {
//            $("#loading_type").hide();
//        });

//}

///**
//* load ds group
//* */
//function load_group_status_by_type() {
//    var typeid = $("select[name=type]").val();
//    if (typeid !== "") {
//        $("#loading_group").show();
//        $("#loading_status").show();

//        var groupSl = $("input[name=hdgroup]").val();

//        $.ajax({
//            method: "POST",
//            url: "/ticket/LoadGroupStatusByType",
//            data: { tid: typeid },
//            dataType: "json"
//        })
//            .done(
//                function (data) {
//                    //groups
//                    $("select[name=group]").empty();
//                    var optcontent = $("#template_promo").html();
//                    for (var i = 0; i < data[0].length; i++) {
//                        addcontent = addcontent.replace(/\{VALUE\}/gim, data[0][i].Id);
//                        addcontent = addcontent.replace(/\{NAME\}/gim, data[0][i].TypeName);
//                        $("select[name=group]").append(optcontent);
//                    }

//                    //status
//                    $("select[name=status]").empty();
//                    var optcontent_status = $("#template_promo").html();
//                    for (var j = 0; j < data[1].length; j++) {
//                        addcontent = addcontent.replace(/\{VALUE\}/gim, data[1][i].Id);
//                        addcontent = addcontent.replace(/\{NAME\}/gim, data[1][i].TypeName);
//                        $("select[name=status]").append(optcontent_status);
//                    }

//                }).fail( function (response) {
//                    console.log("Oops! Something went wrong. Groups, status loading failure");

//                }).always(function () {
//                    $("#loading_group").hide();
//                    $("#loading_status").hide();

//                    //$("select[name=group]").val(groupSl).trigger('change');

//                    //if ($scope.groups.length > 0)
//                    //{
//                    //    $scope.group = $scope.groups[0];

//                    //}
                   
//                    loadMemberInGroup();

//                });

//    }
//    else {
//        $("input[name=hdgroup]").val('');
//        $("input[name=hdstatus]").val('');
//        //$scope.groups = null;
//        //$("select[name=group]").val('').trigger("chosen:updated");
//    }


//}

///**
// * load ds nhan vien by group
// * */
//function loadEmployee() {
//    $("#loading_member").show();

//    var subcribeSL = $("input[name=hdsubcribe]").val();
//    $.ajax({
//        method: "POST",
//        url: "/memberman/LoadMemberList",
//        data: { type: 'emp' },
//        dataType: "json"
//    }).done(
//            function (data) {
//                //alert(response.data[0].FullName);

//            $("select[name=subcribe]").empty();
//            var optcontent = $("#template_promo").html();
//            for (var i = 0; i < data.length; i++) {
//                addcontent = addcontent.replace(/\{VALUE\}/gim, data[i].MemberNumber);
//                addcontent = addcontent.replace(/\{NAME\}/gim, data[i].FullName);
//                $("select[name=subcribe]").append(optcontent);
//            }

              

//        }).fail(function () {
//            console.log("Oops! Something went wrong. Members loading failure");
//        })
//        .always(function () {

//            //reselect assigned/subcriber
//            var subcribeSL = $("input[name=hdsubcribe]").val();

//            //alert(assignedSL); alert(subcribeSL);
//            var arrSubcribe = new Array();
//            var _arrSubcribe = subcribeSL.split(",");
//            for (var ii = 0; ii < _arrSubcribe.length; ii++) {
//                arrSubcribe.push(_arrSubcribe[ii]);
//            }
//            $("select[name=subcribe]").val(arrSubcribe).trigger('change');

//        });


//}

///**
// * load member trong group/department
// * */
//function loadMemberInGroup() {
   
//    var groupSL = $("select[name=group]").val();

//    if (groupSL !== "" && groupSL !== null) {

//        $("#loading_memberInGroup").show();

//        $.ajax({
//            method: "POST",
//            url: "/ticket/LoadMemberInGroup",
//            data: { deptid: groupSL },
//            dataType: "json"
//        })
//            .done(
//                function (data) {
//                    $("select[name=assigned]").empty();
//                    var optcontent = $("#template_promo").html();
//                    for (var i = 0; i < data.length; i++) {
//                        addcontent = addcontent.replace(/\{VALUE\}/gim, data[i].MemberNumber);
//                        addcontent = addcontent.replace(/\{NAME\}/gim, data[i].FullName);
//                        $("select[name=assigned]").append(optcontent);
//                    }

//                }).fail( function () {
//                    console.log("Oops! Something went wrong. Members loading failure");
//                })
//            .finally(function () {
//                //console.log("finally get member in group");
//                ////reselect assigned/subcriber
//                var assignedSL = $("input[name=hdassigned]").val();

//                //alert(assignedSL); alert(subcribeSL);
//                var arrAssigned = new Array();
//                var _arrAssigned = assignedSL.split(",");
//                for (var i = 0; i < _arrAssigned.length; i++) {
//                    arrAssigned.push(_arrAssigned[i]);
//                }
//                $("select[name=assigned]").val(arrAssigned).trigger('change');


//                $("#loading_memberInGroup").hide();

//            });
//    }
//    else {
//        $("#loading_memberInGroup").hide();

//    }


//}



///**
// * load danh sach merchant
// * */
//function load_merchants() {
//    $("#loading_merchant").show();
//    var merchantSl = $("input[name=hdmerchant]").val();
//    $.ajax({
//        method: "POST",
//        url: "/ticket/GetMerchant",
//        data: {},
//        dataType: "json"
//    })
//        .done(
//            function (data) {
//                //alert(response.data[0][0].Name);

//                $("select[name=merchant]").empty();
//                var optcontent = $("#template_promo").html();
//                for (var i = 0; i < data.length; i++) {
//                    addcontent = addcontent.replace(/\{VALUE\}/gim, data[i].CustomerCode);
//                    addcontent = addcontent.replace(/\{NAME\}/gim, data[i].BusinessName);
//                    $("select[name=merchant]").append(optcontent);
//                }


//                load_product();

//            }).fail(function () {
//                alert("Oops! Something went wrong. Merchants loading failure");
//                $("#loading_merchant").hide();
//            });
//}

///**
// * load product thuoc merchant
// * */
//function load_product() {
//    $("#loading_product").show();
//    var productSl = $("input[name=hdproduct]").val();

//    $.ajax({
//        method: "POST",
//        url: "/ticket/GetProducts",
//        data: { code: $("select[name=merchant] option:selected").val()},
//        dataType: "json"
//    })
//        .done(
//            function (data) {
//                //alert(response.data[0][0].Name);
//                $scope.products = response.data;
//                $scope.productSelected = productSl;

//                $("select[name=product]").empty();
//                var optcontent = $("#template_promo").html();
//                for (var i = 0; i < data.length; i++) {
//                    addcontent = addcontent.replace(/\{VALUE\}/gim, data[i].Id);
//                    //SerialNo duoc gan boi InvNumber
//                    addcontent = addcontent.replace(/\{NAME\}/gim, data[i].ProductName + '#' + data[i].SerialNo);
//                    $("select[name=product]").append(optcontent);
//                }


//            }).fail(function () {
//                console.log("Oops! Something went wrong. Products loading failure");
//            }).always(function () { $("#loading_product").hide(); });
//}
