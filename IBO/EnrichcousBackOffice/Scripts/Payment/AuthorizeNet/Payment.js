

var payment = (function () {
    const stopOverlay = () => {
        try {
            overlay(false);
        } catch { }
    }

    const doCollect = (info, callBack) => {
        
        axiosNextGen.post(`/Pay/Collect?${$.param(info)}`, info).then(responseData => {
            let response = responseData?.data?.responseData
            if (!response) {
                response = responseData?.data?.result || responseData?.data
            }
            console.log(response)
            if (response.status != 200) {
                stopOverlay()
                toastConsole.error(response.message)
            } else {
                callBack(response)
            }
        }).catch(error => {
            console.error(error)
            toastConsole.error(error.message)
            stopOverlay()
        });
    }
    const doCollectExistCard = (OrdersCode,  PaymentProfile_Id, callBack) => {
        const info = {
            PaymentProfile_Id: PaymentProfile_Id,
            OrdersCode: OrdersCode
        }
        doCollect(info, callBack)
    }
    const doCollectNewCard = (OrdersCode, CardNumber, CardExpiry, CardCSC, CardHolderName, callBack) => {
        const info = {
            CardNumber: CardNumber,
            CardExpiry: CardExpiry,
            CardCSC: CardCSC,
            OrdersCode: OrdersCode,
            CardHolderName: CardHolderName
        }
        doCollect(info, callBack)
    }

    const loadLstCard = (CustomerCode, callBack, onEmpty) => {
        const info = {
            CustomerCode: CustomerCode,
        }
        axiosNextGen.get(`/Pay/GetListCardByCustomerId?${$.param(info)}`, info).then(response => {
            const Records = response.data.result
            try {
                if (response.data.status == 401 || Records.data.profile.paymentProfiles.length == 0) {
                    onEmpty()
                }
            } catch (err) { console.log(err) }
            
            if (Records.status == 200) {
                callBack(Records.data.profile);
            } else {
                toastConsole.error(Records.message);
            }
        }).catch(error => {
            console.error(error)
            toastConsole.error(error.response.data.Message)
        });
    }

    //payment.loadLstCard(CustomerCode, callBack, onEmpty)
    return {
        doCollectExistCard: doCollectExistCard,
        doCollectNewCard: doCollectNewCard,
        loadLstCard: loadLstCard,
    }
})();