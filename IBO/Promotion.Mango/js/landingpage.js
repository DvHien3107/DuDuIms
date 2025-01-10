// HEADER ANIMATION
$(window).scroll(function () {
  if ($(this).scrollTop() > 70) {
    $("#header .logoheader").attr(
      "src",
      "/images/Main_visual/enrich_logo-black.svg"
    );
  } else {
    $("#header .logoheader").attr("src", "/images/Main_visual/enrich_logo.svg");
  }
}); // HEADER ANIMATION
window.onscroll = function () {
  scrollFunction();
};
var element = document.getElementById("body");
function scrollFunction() {
  if (
    document.body.scrollTop > 400 ||
    document.documentElement.scrollTop > 400
  ) {
    $(".navbar").addClass("fixed-top");
    element.classList.add("header-small");
    $("body").addClass("body-top-padding");
  } else {
    $(".navbar").removeClass("fixed-top");
    element.classList.remove("header-small");
    $("body").removeClass("body-top-padding");
  }
}

// OWL-CAROUSAL
$(".owl-carousel").owlCarousel({
  items: 3,
  loop: true,
  nav: false,
  dot: true,
  autoplay: true,
  slideTransition: "linear",
  autoplayHoverPause: true,
  responsive: {
    0: {
      items: 1,
    },
    600: {
      items: 2,
    },
    1000: {
      items: 3,
    },
  },
});

// SCROLLSPY
$(document).ready(function () {
  $(".nav-link").click(function () {
    var t = $(this).attr("href");
    var height;
    if (t == "#header-section") {
      height = 0;
    } else {
      if ($("#navbar").hasClass("fixed-top")) {
        height = $(t).offset().top - 83;
      } else {
        height = $(t).offset().top + 83;
      }
    }

    $("html, body").animate(
      {
        scrollTop: height,
      },
      {
        duration: 1000,
      }
    );
    $("body").scrollspy({ target: ".navbar", offset: $(t).offset().top });
    return false;
  });
});

// AOS
AOS.init({
  offset: 450,
  delay: 150,
  duration: 1200,
  easing: "ease",
  once: true,
  mirror: false,
  anchorPlacement: "top-bottom",
  disable: "mobile",
});

//SIDEBAR-OPEN
$("#navbarSupportedContent").on("hidden.bs.collapse", function () {
  $("body").removeClass("sidebar-open");
});

$("#navbarSupportedContent").on("shown.bs.collapse", function () {
  $("body").addClass("sidebar-open");
});

$(document).ready(function () {
    $(document).click(function (e) {
        var container = $("#navbarSupportedContent");

        if (!container.is(e.target) && container.has(e.target).length === 0) {
            $('.navbar-collapse').collapse('hide');
        }
    //var clickover = $(event.target);
    //var _opened = $(".navbar-collapse").hasClass("show");
    //if (_opened === true && !clickover.hasClass("navbar-toggler")) {
    //  $(".navbar-toggler").click();
    //}
  });
});

$('.navbar-nav>li>a').on('click', function(){
  $('.navbar-collapse').collapse('hide');
});
window.onresize = function () {
  var w = window.innerWidth;
  if (w >= 992) {
    $("body").removeClass("sidebar-open");
    $("#navbarSupportedContent").removeClass("show");
  }
};

$(".ipt-valid").on("change", function () {
  if ($(this).val().length > 0) {
    $(this).next(".placeholder").hide();
  } else {
    $(this).next(".placeholder").show();
  }
});
