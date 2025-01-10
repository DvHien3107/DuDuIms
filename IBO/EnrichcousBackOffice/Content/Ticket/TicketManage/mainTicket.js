var mainTicket = (function () {

    var startDate = moment().subtract(6, 'days')
    var endDate = moment()
    const typeid = urlParams.get('typeid')||''
    const departmentid = urlParams.get('departmentid') || ''


    const getDataTicket = () => {
        axiosTicket(departmentid, typeid, function (dataRecord) {
            const reponsData = dataRecord.reponsData
            let recordHtml = '';
            if(departmentid != 19120002){
                $('#target-project').text('Customer')
            }
           
            $.each(reponsData, function (key, item) {
                let deadlineTxt = '';
                if (item.deadline) {
                    deadlineTxt = moment(item.deadline).format('DD/MM/YYYY')
                }else{
                    deadlineTxt = ''
                }
                recordHtml += `<tr class="tr-ticket-item odd" onclick="location.href='/ticket/detail/${item.ticketId}'" role="row" style="
    cursor: pointer;
">
                            <td>#${item.ticketId}</td>
                            <td><div>${item.ticketName}</div> <div style="
    display: flex;
    flex-wrap: wrap;
    gap: 2px;
">`
                $.each(item.lstTags ||[], function (key, tags) {
                    recordHtml += ` <div class="tags" style="
                                        background-color: ${tags.color};
                                    "> ${tags.name }</div>`
                })
                if(departmentid != 19120002){
                    recordHtml += `</div></td>
                    <td>${item.customerName}</td>
                    <td>`
                } else {
                    recordHtml += `</div></td>
                    <td>${item.projectName}</td>
                    <td>`
                }
               
                $.each(item.lstAssigned || [], function (key, member) {
                    let profileDefinedColor = '';
                    if (member.profileDefinedColor != '#ffffff') {
                        profileDefinedColor = member.profileDefinedColor
                    }
                    recordHtml += ` <div class="memberItem" style="margin-bottom:2px; background:${profileDefinedColor}"><img src="${member.picture || '/Upload/Img/Male.png'}"/> ${member.fullName}</div>`
                })
                recordHtml += `</td><td style="
    display: flex;
    flex-wrap: wrap;
    gap: 2px;
">`

                $.each(item.lstMember || [], function (key, member) {
                    let profileDefinedColor = '';
                    console.log(member.profileDefinedColor)
                    if (member.profileDefinedColor != '#ffffff') {
                        profileDefinedColor = member.profileDefinedColor
                    }
                    recordHtml += ` <div class="memberItem" style="background:${profileDefinedColor}"><img src="${member.picture || '/Upload/Img/Male.png'}"/> ${member.fullName}</div>`
                })
                recordHtml += `</td>`            

                recordHtml += ` 
                   <td class=" relative">${moment(item.createAt).format('DD/MM/YYYY')}</td>
                <td class="">
                                ${deadlineTxt}
                            </td>
                            <td>
                                ${item.statusName}
                            </td>
                        </tr>`
            })
            $('#list_tickets').html(recordHtml)
           
        })
    }
    const axiosTicket = (DepartmentID, TypeId, callBack) => {
        const start = mainTicket.startDate.format('YYYY-MM-DD');
        const end = mainTicket.endDate.format('YYYY-MM-DD');
        axiosNextGen.get(`Ticket/LoadTicket?DepartmentID=${DepartmentID || ''}&TypeId=${TypeId||''}&start=${start}&end=${end}`).then(response => {
            //console.log(response);
            const Records = response.data
            callBack(Records)
        }).catch(error => {
            console.error(error)
            toastConsole.error(error.data.data.Message)
        });
    }
    //mainTicket.getDataTicket()
    return {
        startDate: startDate,
        endDate: endDate,
        getDataTicket: getDataTicket,
    }
})();