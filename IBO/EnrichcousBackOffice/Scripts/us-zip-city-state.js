
$(function () {
    loadUSAState();
    //loadUSACityByState();
    //loadUSAZipcodeByCity();

    $("#Country").change(function () {
        //if ($("#txtIssuingState").length)
        //    $("#txtIssuingState").val("");

        $("#txtState").val("");
        //$("#txtCity").val("");
        //$("#txtZipcode").val("");
        loadUSAState();
        //loadUSACityByState();
        //loadUSAZipcodeByCity();
    });

    //$("#selectState").change(function () {
    //    $("#txtCity").val("");
    //    $("#txtZipcode").val("");
    //    loadUSACityByState();
    //    loadUSAZipcodeByCity();
    //});

    //$("#selectCity").change(function () {
    //    $("#txtZipcode").val("");
    //    loadUSAZipcodeByCity();
    //});


});

function loadUSAState() {
    //alert($("#Country").val());


    if ($("#Country").val() === "United States") {

        $("#stateLoading").css("display", "block");
        $("#selectState").css("display", "block");
        $("#txtState").css("display", "none");
        $("#txtState").prop("disabled",true);

        //if ($("#txtIssuingState").length) {
        //    $("#issuingStateLoading").css("display", "inline");
        //    $("#selectIssuingState").css("display", "inline");
        //    $("#txtIssuingState").css("display", "none");
        //    $("#txtIssuingState").prop("disabled", true);
        //}

        $.ajax({
            method: "POST",
            url: "/Account/GetUSAStates",
            data: {},
            dataType: "json"
        })
            .done(function (result) {
                $("#selectState").append("<option value=''> --- </option>");
                for (var i = 0; i < result.length; i++) {

                    if (result[i].ID === $("#txtState").val()) {
                        $("#selectState").append("<option selected=\"selected\"  value=\"" + result[i].ID + "\" >" + result[i].Name + "</option>");
                    }
                    else {
                        $("#selectState").append("<option value=\"" + result[i].ID + "\" >" + result[i].Name + "</option>");
                    }
                }
            })
            .fail(function () {
                alert("Oops! Something went wrong");
                $("#selectState").empty();
                $("#selectState").css("display", "none");
                $("#txtState").css("display", "block");
                $("#txtState").prop("disabled", false);
            })
            .always(function () { $("#stateLoading").css("display", "none");});


        //var obj = {};
        //obj.url = "/Account/GetUSAStates";
        //obj.type = "POST";
        //obj.contentType = "application/json";
        //obj.dataType = "json";
        //obj.success = function (result) {
        //    $("#selectState").empty();

        //    $("#selectState").append("<option value=''> --- </option>");
        //    for (var i = 0; i < result.length; i++) {
        //        //if ($("#selectIssuingState").length) {
        //        //    if (result[i].ID == $("#txtIssuingState").val()) {
        //        //        $("#selectIssuingState").append("<option selected=\"selected\"  value=\"" + result[i].ID + "\" >" + result[i].Name + "</option>");
        //        //    }
        //        //    else {
        //        //        $("#selectIssuingState").append("<option value=\"" + result[i].ID + "\" >" + result[i].Name + "</option>");
        //        //    }
        //        //}

        //        if (result[i].ID === $("#txtState").val()) {
        //            $("#selectState").append("<option selected=\"selected\"  value=\"" + result[i].ID + "\" >" + result[i].Name + "</option>");
        //        }
        //        else {
        //            $("#selectState").append("<option value=\"" + result[i].ID + "\" >" + result[i].Name + "</option>");
        //        }
        //    }

        //};
        //obj.complete = function () {
        //    $("#stateLoading").css("display", "none");
        //    //if ($("#issuingStateLoading").length) {
        //    //    $("#issuingStateLoading").css("display", "none");
        //    //}
        //};
        //obj.error = function () {
        //    alert("Oops! Something went wrong");
        //    $("#selectState").empty();
        //    $("#selectState").css("display", "none");
        //    $("#txtState").css("display", "block");
        //    $("#txtState").prop("disabled", false);

        //    //if ($("#selectIssuingState").length) {
        //    //    $("#selectIssuingState").empty();
        //    //    $("#selectIssuingState").css("display", "none");
        //    //    $("#txtIssuingState").css("display", "inline");
        //    //    $("#txtIssuingState").prop("disabled",false);
        //    //}
        //};

        //$.ajax(obj);


    }
    else {
        $("#selectState").empty();
        $("#selectState").css("display", "none");
        $("#txtState").css("display", "block");
        $("#txtState").prop("disabled", false);

        //if ($("#selectIssuingState").length) {
        //    $("#selectIssuingState").empty();
        //    $("#selectIssuingState").css("display", "none");
        //    $("#txtIssuingState").css("display", "inline");
        //    $("#txtIssuingState").prop("disabled", false);
        //}

    }

}

function loadUSACityByState() {
    if ($("#Country").val() == "United States") {
        $("#cityLoading").css("display", "inline");
        $("#selectCity").css("display", "inline");
        $("#txtCity").css("display", "none");
        $("#txtCity").prop("disabled", true);
        var state = "AK";
        if ($("#txtState").val() != "") {
            state = $("#txtState").val();
        }
        if ($("#selectState").val() != null) {
            state = $("#selectState").val();
        }
        var obj = {};
        obj.url = "/Home/GetUSACityByState";
        obj.dataType = "json";
        obj.data = JSON.stringify({ idState: state });
        obj.type = "POST";
        obj.contentType = "application/json";
        obj.success = function (result) {
            $("#selectCity").empty();
            $("#selectCity").append("<option value=''> --- </option>");
            for (var i = 0; i < result.length; i++) {
                if (result[i].City == $("#txtCity").val()) {
                    $("#selectCity").append("<option selected=\"selected\" value=\"" + result[i].City + "\" >" + result[i].City + "</option>");
                }
                else {
                    $("#selectCity").append("<option value=\"" + result[i].City + "\" >" + result[i].City + "</option>");
                }

            }
        };
        obj.complete = function () { $("#cityLoading").css("display", "none"); };
        obj.error = function () {
            $("#selectCity").empty();
            $("#selectCity").css("display", "none");
            $("#txtCity").css("display", "inline");
            $("#txtCity").prop("disabled", false);
        };

        $.ajax(obj);
    }
    else {
        $("#selectCity").empty();
        $("#selectCity").css("display", "none");
        $("#txtCity").css("display", "inline");
        $("#txtCity").prop("disabled", false);
    }

}

function loadUSAZipcodeByCity() {
    if ($("#Country").val() == "United States") {
        $("#zipcodeLoading").css("display", "inline");
        $("#selectZipcode").css("display", "inline");
        $("#txtZipcode").css("display", "none");
        $("#txtZipcode").prop("disabled", true);
        var cityName = "Adak";
        if ($("#txtCity").val() != "") {
            cityName = $("#txtCity").val();
        }
        if ($("#selectCity").val() != null) {
            cityName = $("#selectCity").val();
        }

        var obj = {};
        obj.url = "/Home/GetUSAZipCode";
        obj.dataType = "json";
        obj.data = JSON.stringify({ city: cityName });
        obj.type = "POST";
        obj.contentType = "application/json";
        obj.success = function (result) {
            $("#selectZipcode").empty();
            $("#selectZipcode").append("<option value=''> --- </option>");
            for (var i = 0; i < result.length; i++) {

                if (result[i].Zip == $("#txtZipcode").val()) {

                    $("#selectZipcode").append("<option selected=\"selected\" value=\"" + result[i].Zip + "\" >" + result[i].Zip + "</option>");
                }
                else {
                    $("#selectZipcode").append("<option value=\"" + result[i].Zip + "\" >" + result[i].Zip + "</option>");
                }

            }
        };
        obj.complete = function () { $("#zipcodeLoading").css("display", "none"); };
        obj.error = function () {
            $("#selectZipcode").empty();
            $("#selectZipcode").css("display", "none");
            $("#txtZipcode").css("display", "inline");
            $("#txtZipcode").prop("disabled", false);
        };

        $.ajax(obj);
    }
    else {
        $("#selectZipcode").empty();
        $("#selectZipcode").css("display", "none");
        $("#txtZipcode").css("display", "inline");
        $("#txtZipcode").prop("disabled", false);
    }

}

//Reload

//reload lai thong tin da co set trong input
function loadUSAStateReload() {
    if ($("#Country").val() == "United States") {
        $("#stateLoading").css("display", "inline");
        $("#selectState").css("display", "inline");
        $("#txtState").css("display", "none");
        $("#txtState").prop("disabled", true);

        if ($("#txtIssuingState").length) {
            $("#issuingStateLoading").css("display", "inline");
            $("#selectIssuingState").css("display", "inline");
            $("#txtIssuingState").css("display", "none");
            $("#txtIssuingState").prop("disabled", true);
        }


        var obj = {};
        obj.url = "/Home/GetUSAStates";
        obj.dataType = "json";
        obj.type = "POST";
        obj.contentType = "application/json";
        obj.success = function (result) {
            $("#selectState").empty();
            if ($("#selectIssuingState").length) {
                $("#selectIssuingState").append("<option value=''  > ---  </option>");
            }
            $("#selectState").append("<option value=''> --- </option>");
            for (var i = 0; i < result.length; i++) {
                if ($("#selectIssuingState").length) {
                    if (result[i].ID == $("#txtIssuingState").val()) {
                        $("#selectIssuingState").append("<option selected=\"selected\"  value=\"" + result[i].ID + "\" >" + result[i].Name + "</option>");
                    }
                    else {
                        $("#selectIssuingState").append("<option value=\"" + result[i].ID + "\" >" + result[i].Name + "</option>");
                    }
                }

                if (result[i].ID == $("#txtState").val()) {
                    $("#selectState").append("<option selected=\"selected\"  value=\"" + result[i].ID + "\" >" + result[i].Name + "</option>");
                }
                else {
                    $("#selectState").append("<option value=\"" + result[i].ID + "\" >" + result[i].Name + "</option>");
                }

            }

            loadUSACityByStateReload();

        };
        obj.complete = function () {
            $("#stateLoading").css("display", "none");
            if ($("#issuingStateLoading").length) {
                $("#issuingStateLoading").css("display", "none");
            }
        };
        obj.error = function () {
            $("#selectState").empty();
            $("#selectState").css("display", "none");
            $("#txtState").css("display", "inline");
            $("#txtState").prop("disabled", false);

            if ($("#selectIssuingState").length) {
                $("#selectIssuingState").empty();
                $("#selectIssuingState").css("display", "none");
                $("#txtIssuingState").css("display", "inline");
                $("#txtIssuingState").prop("disabled", false);
            }
        };

        $.ajax(obj);
    }
    else {
        $("#selectState").empty();
        $("#selectState").css("display", "none");
        $("#txtState").css("display", "inline");
        $("#txtState").prop("disabled", false);

        if ($("#selectIssuingState").length) {
            $("#selectIssuingState").empty();
            $("#selectIssuingState").css("display", "none");
            $("#txtIssuingState").css("display", "inline");
            $("#txtIssuingState").prop("disabled", false);
        }

    }

}

function loadUSACityByStateReload() {
    if ($("#Country").val() == "United States") {
        $("#cityLoading").css("display", "inline");
        $("#selectCity").css("display", "inline");
        $("#txtCity").css("display", "none");
        $("#txtCity").prop("disabled", true);
        var state = "AK";
        if ($("#txtState").val() != "") {
            state = $("#txtState").val();
        }
        if ($("#selectState").val() != null) {
            state = $("#selectState").val();
        }
        var obj = {};
        obj.url = "/Home/GetUSACityByState";
        obj.dataType = "json";
        obj.data = JSON.stringify({ idState: state });
        obj.type = "POST";
        obj.contentType = "application/json";
        obj.success = function (result) {
            $("#selectCity").empty();
            $("#selectCity").append("<option value=''> --- </option>");
            for (var i = 0; i < result.length; i++) {
                if (result[i].City == $("#txtCity").val()) {
                    $("#selectCity").append("<option selected=\"selected\" value=\"" + result[i].City + "\" >" + result[i].City + "</option>");
                }
                else {
                    $("#selectCity").append("<option value=\"" + result[i].City + "\" >" + result[i].City + "</option>");
                }

            }

            loadUSAZipcodeByCityReload();

        };
        obj.complete = function () { $("#cityLoading").css("display", "none"); };
        obj.error = function () {
            $("#selectCity").empty();
            $("#selectCity").css("display", "none");
            $("#txtCity").css("display", "inline");
            $("#txtCity").prop("disabled", false);
        };

        $.ajax(obj);
    }
    else {
        $("#selectCity").empty();
        $("#selectCity").css("display", "none");
        $("#txtCity").css("display", "inline");
        $("#txtCity").prop("disabled", false);
    }

}

function loadUSAZipcodeByCityReload() {
    if ($("#Country").val() == "United States") {
        $("#zipcodeLoading").css("display", "inline");
        $("#selectZipcode").css("display", "inline");
        $("#txtZipcode").css("display", "none");
        $("#txtZipcode").prop("disabled", true);
        var cityName = "Adak";
        if ($("#txtCity").val() != "") {
            cityName = $("#txtCity").val();
        }
        if ($("#selectCity").val() != null) {
            cityName = $("#selectCity").val();
        }

        var obj = {};
        obj.url = "/Home/GetUSAZipCode";
        obj.dataType = "json";
        obj.data = JSON.stringify({ city: cityName });
        obj.type = "POST";
        obj.contentType = "application/json";
        obj.success = function (result) {
            $("#selectZipcode").empty();
            $("#selectZipcode").append("<option value=''> --- </option>");
            for (var i = 0; i < result.length; i++) {

                if (result[i].Zip == $("#txtZipcode").val()) {

                    $("#selectZipcode").append("<option selected=\"selected\" value=\"" + result[i].Zip + "\" >" + result[i].Zip + "</option>");
                }
                else {
                    $("#selectZipcode").append("<option value=\"" + result[i].Zip + "\" >" + result[i].Zip + "</option>");
                }

            }
        };
        obj.complete = function () { $("#zipcodeLoading").css("display", "none"); };
        obj.error = function () {
            $("#selectZipcode").empty();
            $("#selectZipcode").css("display", "none");
            $("#txtZipcode").css("display", "inline");
            $("#txtZipcode").prop("disabled", false);
        };

        $.ajax(obj);
    }
    else {
        $("#selectZipcode").empty();
        $("#selectZipcode").css("display", "none");
        $("#txtZipcode").css("display", "inline");
        $("#txtZipcode").prop("disabled", false);
    }

}
