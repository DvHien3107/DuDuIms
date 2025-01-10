//slidebar class = "stick"
//Add and edit follow css :
//.sticky {
//      position: fixed;
//      top: 0;
//      right: 0;
//      width: 22%;
//   }
window.onscroll = function () { stickfixedscroll() };
var stick = $(".stick");
var sticky = stick.position().top;
function stickfixedscroll() {
    if (window.pageYOffset > sticky) {
        stick.addClass("sticky");
    } else {
        stick.removeClass("sticky");
    }
}