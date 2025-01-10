/**
 * CardType
 * @param cardNumber
 * @returns {*}
 * @constructor
 */
function CardType(cardNumber)
{
    if (/^4[0-9]{6,}$/g.test(cardNumber)) return "VISA";
    if (/^3[47][0-9]{5,}$/g.test(cardNumber)) return "AMEX";
    if (/^5[1-5][0-9]{5,}|222[1-9][0-9]{3,}|22[3-9][0-9]{4,}|2[3-6][0-9]{5,}|27[01][0-9]{4,}|2720[0-9]{3,}$/g.test(cardNumber)) return "MASTERCARD";
    if (/^(4026|417500|4405|4508|4844|4913|4917)\d+$/g.test(cardNumber)) return "ELECTRON";
    if (/^(5018|5020|5038|6304|6759|6761|6763)[0-9]{8,15}$/g.test(cardNumber)) return "MAESTRO";
    if (/^(6304|6706|6709|6771)[0-9]{12,15}$/g.test(cardNumber)) return "LASER";
    if (/^3(?:0[0-5]|[68][0-9])[0-9]{11}$/g.test(cardNumber)) return "DINERS";
    if (/^65[4-9][0-9]{13}|64[4-9][0-9]{13}|6011[0-9]{12}|(622(?:12[6-9]|1[3-9][0-9]|[2-8][0-9][0-9]|9[01][0-9]|92[0-5])[0-9]{10})$/g.test(cardNumber)) return "DISCOVER";
    if (/^(?:2131|1800|35\d{3})\d{11}$/g.test(cardNumber)) return "JCB";
    return "Unknown";
}
function overlay(on) {
    if (on != false) {
        $("#overlay").show().animate({ opacity: 1 },200);
    } else {
        $("#overlay").animate({ opacity: 0 },200, function () { $("#overlay").hide()});
    }
}