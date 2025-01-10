var toastConsole = (function () {


    function error(text) {
        Toastify({
            text: text,
            duration: 3000,
            newWindow: true,
            close: true,
            gravity: "top", // `top` or `bottom`
            position: "right", // `left`, `center` or `right`
            stopOnFocus: true, // Prevents dismissing of toast on hover
            style: {
                background: "#c3000a",
                color: "#fff",
            },
            onClick: function () { } // Callback after click
        }).showToast();
    }

    function success(text) {
        Toastify({
            text: text,
            duration: 3000,
            newWindow: true,
            close: true,
            gravity: "top", // `top` or `bottom`
            position: "right", // `left`, `center` or `right`
            stopOnFocus: true, // Prevents dismissing of toast on hover
            style: {
                background: "#00ff3a",
                color: "#fff",
            },
            onClick: function () { } // Callback after click
        }).showToast();
    }
    //toastConsole.error()
    return {
        error: error,
        success:success,
    }
})();

function searchToObject() {
    var pairs = window.location.search.substring(1).split("&"),
        obj = {},
        pair,
        i;

    for (i in pairs) {
        if (pairs[i] === "") continue;

        pair = pairs[i].split("=");
        obj[decodeURIComponent(pair[0])] = decodeURIComponent(pair[1]);
    }

    return obj;
}