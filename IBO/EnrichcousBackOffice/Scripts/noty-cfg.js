function success(msg, layout = "top") {
    $.noty.closeAll();
    noty({"text": msg, "layout": layout, "type": "success"});
}

function warning(msg, layout = "top") {
    $.noty.closeAll();
    noty({ text: msg, layout: layout, type: "warning"});
}

function error(msg, layout = "top") {
    $.noty.closeAll();
    noty({"text": msg, "layout": layout, "type": "error"});
}