$(".treeview-menu a[href='#']").removeAttr("onclick");
$("li.active").parents(".treeview").addClass("active");
$(".treeview-menu").each(function () {
    if ($(this).find("li").length == 0) {
        $(this).parent().remove();
    }
});
$(".treeview-menu").each(function () {
    if ($(this).find("li").length == 0) {
        $(this).parent().remove();
    }
});
function redirectPost(location, args) {
    overlayOn();
    var form = '';
    $.each(args, function (key, value) {
        //value = value.split('"').join('\"')
        form += '<input type="hidden" name="' + key + '" value="' + value + '">';
    });
    $('<form action="' + location + '" method="POST">' + form + '</form>').appendTo($(document.body)).submit();
}
function goback() {
    overlayOn();
    window.history.back();
}


const loadTicketTreeView = (MemberNumber, callBack) => {
    //ticket-treeview
    axiosNextGen.get(`Member/loadDepartment?MemberNumber=${MemberNumber}`).then(response => {
        console.log(response);
        const Records = response.data.data
        callBack(Records)
    }).catch(error => {
        console.error(error)
        toastConsole.error(error.data.data.Message)
    });
}

const loadDataTicketTree = () => {
    const MemNum = getMember().MemberNumber;
    
    loadTicketTreeView(MemNum, function (RecordsData) {
        var form = ``;
        $.each(RecordsData, function (key, item) {
            //if (item.id == 19120001 || item.id == 19120002 || item.id == 19120003) {
                const ticketType = item.ticketTypes || []
                if (ticketType.length == 0) {
                    form += `
                <li>
                    <a href="/TicketManage?departmentid=${item.id}">
                        <i class="fa fa-circle-o"></i> <span>${item.name} </span>
                    </a>
                </li>`;
                } else {
                    form += `
                <li>
                    <a href="/TicketManage?departmentid=${item.id}">
                      <i class="fa fa-circle-o"></i> <span>${item.name} </span>
                       <ul class="level_2">`
                    $.each(ticketType, function (key, type) {
                        form += ` <li><a href="/TicketManage/Type?departmentid=${item.id}&typeid=${type.id}">-></i><span> ${type.typeName}</span></a></li>`
                    })
                    form += `</ul>
                    </a>
                </li>`;
                }
            //}
        });
        $('#ticket-treeview').html(form)

    })
}

loadDataTicketTree();