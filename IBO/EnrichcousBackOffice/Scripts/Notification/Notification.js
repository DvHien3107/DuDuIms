let start = 0;
let length = 15;
let loadAll = false;
//let totalItemRead = 0;
let totalItemUnread = 0;
let totalItem = 0;
let loading = false;
let saveDataNoti = true;
let memberNumber = $("#memberNo").val();
var tabReadActive = sessionStorage.getItem("ReadTabNotificationActive");
if (tabReadActive)
{
    $(`#notification-content .tab-nav-item-notification[data-type='${tabReadActive}']`).addClass("active");
}
else
{
    $("#notification-content .tab-nav-item-notification[data-type='all']").addClass("active");
}
function getCountNotification() {
    return $.ajax({
        method: "get",
        url: "/Home/CountNotificationNotRead",
        dataType: "json"
    })
        .done(function (data) {
            //totalItemRead = data.totalItemRead;
            totalItemUnread = data.totalItemUnread;
            $("#count-unread").html(totalItemUnread);
            //$("#count-read").html(totalItemRead);
            $(".count-notification").html(data.count);
            //let read = $("#notification-content .tab-nav-item-notification.active").attr("data-type") == 'read';
            //totalItem = read ? totalItemRead : totalItemUnread;
            //if ((read ? totalItemRead : totalItemUnread) == 0) {
            //    $(".notification-nodata").show();
            //    //if (!read) {
            //    //    $("#markAllRead").hide();
            //   //}
            //}
        }).fail(function () {
            alert('cannot load notification !')
        })
}
//getCountNotification();

function showNotification() {
    loading = true;
    $(".notification-nodata").hide();
    let tab = $("#notification-content .tab-nav-item-notification.active").attr("data-type");

    $.ajax({
        method: "get",
        url: "/Home/GetNotification",
        data:
        {
            start,
            length,
            tab
        },
        dataType: "json"
    })
        .done(function (data) {

            start = length;
            length = length + 15;
            $(".loader-content-noti").remove();
            $(".notification-wrapper").append(data.data);
            $(".time-noti-read").html(function (index, value) {
                $(".time-noti-read").removeClass("time-noti-read");
                return 'Read at: ' + moment(value).format('lll');
            });
            $(".time-noti-raw").html(function (index, value) {
                $(".time-noti-raw").removeClass("time-noti-raw");
                return moment(value).fromNow();
            });

            //if (read) {
            //    $(".time-noti-read").html(function (index, value) {
            //        $(".time-noti-read").removeClass("time-noti-read");
            //        return 'Read at: ' + moment(value).format('lll');
            //    });
            //}
            //else {
            //    $(".time-noti-raw").html(function (index, value) {
            //        $(".time-noti-raw").removeClass("time-noti-raw");
            //        return moment(value).fromNow();
            //    });
            //}
            if (tab == 'read') {
                $("#markAllRead").hide();
            }
            else {
                $("#markAllRead").show();
            }
            var showed = localStorage.getItem("showNoti") == "true";
            //if ($('.pin-tab-notification-content').length) {
            //    $('#_body').addClass('c-notification-fixed');
            //}
            if (showed || $('.pin-tab-notification-content').length) {
                // $(".notifications-menu").addClass("open");
                let lastPositionScroll = localStorage.getItem("notiPos");
                if (Math.round($(".notification-wrapper").prop('scrollHeight') - $(".notification-wrapper").height()) < parseInt(lastPositionScroll) - 10) {
                    showNotification();
                    return;
                }
                else {
                    $(".notification-wrapper").scrollTop(localStorage.getItem("notiPos"));
                    //   localStorage.removeItem("showNoti");
                    localStorage.removeItem("notiPos");

                }
            }
            if (data.count == 0) {
                $(".notification-nodata").show();
            }
            else {
                if (!(length > data.count)) {
                    addLoader();
                }
                else {
                    loadAll = true;
                }
            }
            loading = false;
            $('.notification-wrapper').css('height', $('.notification-wrapper li:nth-of-type(n + 8)').height);
        })
        .fail(function () {
            alert('cannot load notification !')
        })
}

function scrollNotification() {
    //$('.notification-wrapper').scroll(function () {
    //    if ($('.notification-wrapper').scrollTop() == $('.notification-wrapper').height() - $('.notification-wrapper').height()) {
    //        // ajax call get data from server and append to the div
    //    }
    //});
}

//$(window).bind('beforeunload', function () {
//    if (loading == false && saveDataNoti == true) {
//        saveNotification();
//    }
//    else {
//        sessionStorage.removeItem("notificationData_" + memberNumber);
//    }

//});

//function saveNotification() {
//    var notfication = $('.tab-notification-content').html();
//    var oldData = { notfication, start, length, loadAll, totalItemUnread, totalItem, loading };
//    sessionStorage.setItem("notificationData_" + memberNumber, JSON.stringify(oldData));
//    localStorage.setItem("notiPos", $(".notification-wrapper").scrollTop());
//    if ($(".notifications-menu").hasClass('open')) {
//        localStorage.setItem("showNoti", true);
//    }
//}




function markAsRead(el, Id, isread = true) {

    let notificationItem = $(el).parents('.notification-item');
    $(notificationItem).find(".dropdown-notification-action-content").hide();

    let tab = $("#notification-content .tab-nav-item-notification.active").attr("data-type");
    if (!$(el).hasClass('notification-read')) {
        totalItemUnread = totalItemUnread > 0 ? (totalItemUnread - 1) : totalItemUnread;
    }
    if (tab == 'unread' && !$(el).hasClass('notification-read') || tab == 'unread' && isread || (tab == 'read' && !isread)) {
        $(notificationItem).remove();
    }

    if (isread) {
        $(notificationItem).find(".notification-container").addClass("notification-read");
        $(notificationItem).find(".icon-view").addClass("fa-eye");
        $(notificationItem).find(".icon-view").removeClass("fa-bell-o bell");
        $(notificationItem).find(".btn-notification-read").hide();
        $(notificationItem).find(".btn-notification-unread").show();
        if (!$(notificationItem).find(".time-noti-read-define").is(":visible")) {
            $(notificationItem).find(".time-noti-read-define").show();
            $(notificationItem).find(".time-noti-read-define").html('Read at: ' + moment().format('lll'));
        }
        $(notificationItem).find(".time-noti-createat").hide();
    }
    else {
        $(notificationItem).find(".notification-container").removeClass("notification-read");
        $(notificationItem).find(".icon-view").removeClass("fa-eye");
        $(notificationItem).find(".icon-view").addClass("fa-bell-o bell");
        $(notificationItem).find(".btn-notification-read").show();
        $(notificationItem).find(".btn-notification-unread").hide();
        $(notificationItem).find(".time-noti-read-define").hide();
        $(notificationItem).find(".time-noti-createat").show();
        //totalItemUnread = parseInt(totalItemUnread) + 1;
    }
    //$("#count-unread").html(totalItemUnread);
    //$(".count-notification").html(totalItemUnread);
    //var notfication = $('.tab-notification-content').html();
    //var oldData = { notfication, start, length, loadAll, totalItemUnread, totalItem, loading };
    //sessionStorage.setItem("notificationData", JSON.stringify(oldData));
    localStorage.setItem("notiPos", $(".notification-wrapper").scrollTop());
    localStorage.setItem("showNoti", true);
    //if (!$(el).hasClass('notification-read')) {
  
    $.ajax({
        method: "get",
        url: "/Home/MarkReadNoti",
        data: { Id, isread },
        dataType: "json"
    }).done(function () {
        getCountNotification();
    })
}

function setIframeTicket(el, category) {
    overlayOn();
    let url = $(el).attr("data-link");
    let iframe = `<div style="height: calc(100vh);">
            <iframe src="${url}" name="ticket_iframe" style="top:0;left:0;height:100%;width:100%;" id="ticket_iframe" onload="overlayOff();window.history.pushState('', '', (document.getElementById('ticket_iframe').contentWindow.location.href.replace('?mode=iframe','').replace('&mode=iframe','')));SetActiveLeftMenu()" scrolling="auto" frameborder="0"></iframe>
        </div>`;
    $("#_body").addClass('mode-iframe-body');
    $(".content-wrapper").html(iframe);

    let originLocation = window.location.origin;
    if (category == "Order") {
        SetActiveLeftMenu(originLocation + "/order");
    }
    else if (category == "SalesLead")
    {
        SetActiveLeftMenu(originLocation + "/SaleLead");
    }
    else
    {
        SetActiveLeftMenu(originLocation+"/ticket");
    }
}
function SetActiveLeftMenu(url) {
  
    // for sidebar menu but not for treeview submenu
    $('ul.sidebar-menu a').filter(function () {
        return this.href == url;
    }).parent().siblings().removeClass('active menu-open').end().addClass('active');
    // for treeview which is like a submenu
    $('ul.treeview-menu a').filter(function () {
        return this.href == url;
    }).parentsUntil(".sidebar-menu > .treeview-menu").siblings().removeClass('active menu-open').end().addClass('active menu-open');
}
function markAllRead() {
    $.ajax({
        method: "post",
        url: "/Home/MarkReadAll",
        dataType: "json"
    }).done(function (data) {
        getCountNotification();
        $(".notification-container").addClass("notification-read");
        $(".icon-view").addClass("fa-eye");
        $(".icon-view").removeClass("fa-bell-o bell");
        $("a[id^='btn-mark-read-']").hide();
        $("a[id^='btn-mark-unread-']").show();
    })
        .fail(function () {
            alert('cannot mark as all read !')
        })
}
//var notificationDataRaw = sessionStorage.getItem("notificationData_" + memberNumber);
//if (notificationDataRaw) {
//    let notificationData = JSON.parse(notificationDataRaw);
//    $('.tab-notification-content').html(notificationData.notfication);
//    start = notificationData.start;
//    length = notificationData.length;
//    loadAll = notificationData.loadAll;
//    //totalItemUnread = notificationData.totalItemUnread;
//    totalItem = notificationData.totalItem;
//    loading = notificationData.loading;
//    var showed = localStorage.getItem("showNoti") == "true";
//    if (showed) {
//        $(".notifications-menu").addClass("open");
//    }
//    $(".notification-wrapper").scrollTop(localStorage.getItem("notiPos"));
//    localStorage.removeItem("showNoti");
//    localStorage.removeItem("notiPos");
//    scrollNotification();
//    getCountNotification();
//}
//else {
$(window).on('load', function () {
    (getCountNotification()).then(function () {
        showNotification();
    })
});
//}

//$(function () {
//    // Reference the hub.
//    var hubNotif = $.connection.broadcastHub;
//    $.connection.hub.start();
//    // Start the connection.
//    $.connection.hub.start().done(function () {
//        //setTimeout(function () {
//        //    (getCountNotification()).then(function () {
//        //        showNotification();
//        //    })
//        //}, 500);
//        //function htmlLoaded() {
//        // //setTimeout(function () {
//        //    //(getCountNotification()).then(function () {
//        //    //    showNotification();
//        //    //})
//        ////}, 500);
//        //}
//    });
//    // Notify while anyChanges.
//    hubNotif.client.pushNotification = function (Id, EntityId, TemplateId) {
//        getCountNotification();
//        noty({ "text": "You have a new notification !", "layout": "topRight", "type": "success" });
//        pushNewNotification(Id, EntityId, TemplateId);
//    };
//});


$('.notification-ul li').on('click', function (event) {
    event.stopPropagation();
});


function scrollNotification() {
    $('.notification-wrapper').scroll(function () {
        let div = $(this).get(0);
        localStorage.setItem("notiPos", $(".notification-wrapper").scrollTop());
        if ((div.scrollTop + div.clientHeight) + 120 >= div.scrollHeight) {
            if (!loading && loadAll == false) {
                loading = true;
                showNotification();
            }
        }
    });
}

scrollNotification();

function addLoader() {
    var html = '';
    html += '<li class="loader-content-noti">';
    html += '<div class="animated-background" >';
    html += '<div class="background-masker header-top"></div>';
    html += '  <div class="background-masker header-left"></div>';
    html += '  <div class="background-masker header-right"></div>';
    html += '  <div class="background-masker header-bottom"></div>';
    html += '  <div class="background-masker subheader-left"></div>';
    html += '  <div class="background-masker subheader-right"></div>';
    html += '  <div class="background-masker subheader-bottom"></div>';
    html += '  <div class="background-masker content-top"></div>';
    html += '  <div class="background-masker content-first-end"></div>';
    html += '  <div class="background-masker content-second-line"></div>';
    html += '  <div class="background-masker content-second-end"></div>';
    html += '  <div class="background-masker content-third-line"></div>';
    html += '  <div class="background-masker content-third-end"></div>';
    html += '</div>';
    html += '</li>';
    $(".notification-wrapper").append(html);
    $(".notification-wrapper").append(html);
}

function pushNewNotification(Id, EntityId, TemplateId) {
    let read = $("#notification-content .tab-nav-item-notification.active").attr("data-type") == 'read';
    if (!read) {
        $.ajax({
            method: "get",
            url: "/Home/GetNewNotification",
            data: {
                Id
            },
            dataType: "html"
        })
            .done(function (data) {
                $(".notification_" + TemplateId + "_" + EntityId).remove();
                if ($(".notification-wrapper").children().not(".notification-nodata").first().hasClass("notifications-groups")) {
                    if ($(".notification-wrapper").children().not(".notification-nodata").first().hasClass("Today")) {
                        $(".notification-wrapper").children().not(".notification-nodata").first().after(data);
                    }
                    else {
                        $(".notification-wrapper").prepend('<h3 class="notifications-groups Today">Today</h3>' + data);
                    }
                }
                else {
                    $(".notification-wrapper").prepend(data);
                }
                loading = false;
                start = length + 1;
                length = start + 10;
                $(".notification-nodata").hide();
                $(".time-noti-raw").html(function (index, value) {
                    $(".time-noti-raw").removeClass("time-noti-raw");
                    return moment(value).fromNow();
                });
            })
            .fail(function () {
                alert('cannot push notification !')
            })
    }

}

window.onclick = function (event) {
    if (!$(event.target).hasClass('.notification-more-button')) {
        $('.dropdown-notification-action-content').removeClass('show');

    }
}

//function hideDropdown(_id) {
//    event.stopPropagation();
//    var dropdowns = document.getElementsByClassName("dropdown-notification-action-content");
//    var i;
//    for (i = 0; i < dropdowns.length; i++) {
//        var openDropdown = dropdowns[i];
//        if (openDropdown.classList.contains('show') && openDropdown.getAttribute("data-content") != _id) {
//            openDropdown.classList.remove('show');
//        }
//    }
//}
function showMarkAsRead(_id, element) {
    $(".dropdown-notification-action-content").not("#myDropdown-" + _id).hide();
    $(element).next().toggle();
    //var div = document.getElementById("myDropdown-" + _id);
    //if (div.style.display !== 'none') {
    //    div.style.display = 'none';
    //}
    //else {
    //    div.style.display = 'block';
    //}
    //event.stopPropagation();
    //var dropdowns = document.getElementsByClassName("dropdown-notification-action-content");
    //var i;
    //for (i = 0; i < dropdowns.length; i++) {
    //    var openDropdown = dropdowns[i];
    //    if (openDropdown.classList.contains('show') && openDropdown.getAttribute("data-content") != _id) {
    //        openDropdown.classList.remove('show');
    //    }
    //}
}

$("#notification-content .tab-nav-item-notification").on('click', function () {
    $("#notification-content .tab-nav-item-notification").removeClass('active');
    $(this).addClass('active');
    let tab = $("#notification-content .tab-nav-item-notification.active").attr("data-type");
    //totalItem = read ? totalItemRead : totalItemUnread;
    start = 0;
    length = 15;
    loadAll = false;
    sessionStorage.setItem("ReadTabNotificationActive", tab);
    $(".notification-wrapper .notification-item").remove();
    $(".notification-wrapper .notifications-groups").remove();
    addLoader();
    showNotification();
    getCountNotification();
});

$('#pin-notification-button').click(function () {
    saveDataNoti = false;
    overlayOn();
    $.ajax({
        method: "get",
        url: "/Home/SetPinNotification",
        dataType: "json"
    })
        .done(function (data) {
            if ($('.pin-tab-notification-content').length) {
                localStorage.setItem("showNoti", true);
            }
            else {
                localStorage.removeItem("showNoti");
            }
            window.location.reload();
        })
        .fail(function () {
            alert('cannot pin notification !')
        })
})


$("#notification-button-top").click(function () {
    if ($('.pin-tab-notification-content').length) {
        $('#_body').toggleClass('c-notification-fixed');
        if ($('#_body').hasClass('c-notification-fixed')) {
            localStorage.setItem("show-content-notification", true);
        }
        else {
            localStorage.setItem("show-content-notification", false);
        }
    }
    else {
        $('.notifications-menu').toggleClass('open');
        if ($('.notifications-menu').hasClass('open')) {
            localStorage.setItem("showNoti", true);
        }
        else {
            localStorage.setItem("showNoti", false);
        }
    }
})

function checkHidePinNotification() {
    if ($('.pin-tab-notification-content').length) {
        if (localStorage.getItem("show-content-notification") == "false") {
            $('#_body').removeClass('c-notification-fixed');
        }
        else {
            $('#_body').addClass('c-notification-fixed');
        }
    }
    else {
        if (localStorage.getItem("showNoti") == "true") {
            $(".notifications-menu").addClass("open");
        }
        else {
            $(".notifications-menu").removeClass("open");
        }
    }
}
checkHidePinNotification()