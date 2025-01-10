////(function($) {

////  $('#reset').on('click', function(){
////      $('#register-form').reset();
////  });

////})(jQuery);
/**
       * select member type
       * */
function SelectMembertype() {
    var type = $("select[name='MemberType'] option:selected").val();

    if (type == "distributor") {
        $('#ReferNumber').attr('required', true);
        $('#ReferedForm').show(200);
    }
    else {
        $("#ReferNumber").attr("Required", false);
        $('#ReferedForm').hide(200);
        $('#submit').removeAttr("disabled");
    }

};

/**
 * Get name refered by
 * */
function getNameRefered() {
    var ReferNumber = $('#ReferNumber').val();

    $.ajax({
        method: "POST",
        url: "/Account/GetNameRefered",
        data: { 'ReferNumber': ReferNumber },
        dataType: "json"
    })
        .done(function (data) {
            //alert(data);
            if (data == "") {
                $('#submit').attr("disabled", true);
                $("#ReferName").val("Not found !!!");
                $("#ReferedByNumber").val('');
            }
            else {
                $("#ReferName").val(data);
                $('#submit').removeAttr("disabled");
            }
        })
        .fail(function () {
            alert("Oops! Something went wrong");
        });

}
