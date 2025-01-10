


var axiosConsole = (function () {
    

    function loadDashboardDetail(month, year, type, callBack) {
        const params = {
            year: year,
            month: month,
            type: type//NEWCUS,CANCEL
        }
        axiosUniversal.post('/customer/report-for-chart/ReportCustomerByMonth', params).then(response => {
            const Records = response.data.response.Records
            callBack(Records)
        }).catch(error => {
            console.error(error)
            toastConsole.error(error.response.data.Message)
        });
    }

    const loadDepartment = (callBack) => {

    }

    //axiosConsole.loadDashboardDetail()
    return {
        loadDashboardDetail: loadDashboardDetail,
    }
})();