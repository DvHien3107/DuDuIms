﻿function redirectPost(location, args) {
    overlayOn();
    var form = '';
    $.each(args, function (key, value) {
        //value = value.split('"').join('\"')
        form += '<input type="hidden" name="' + key + '" value="' + value + '">';
    });
    $('<form action="' + location + '" method="POST">' + form + '</form>').appendTo($(document.body)).submit();
}
