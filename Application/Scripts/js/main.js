$(".switch_toggle").click(function() {
    $(".renderer_sec").toggle();
 });
$(".btn-toggle").click(function () {
    $(this).toggleClass("ev_view");
});

$(".filter_icon").click(function () {
    $(this).addClass("toggle_filter");
});

$(".filter_closer").click(function () {
    $(".filter_icon").removeClass("toggle_filter");
});

 $("#legend_dropdown").click(function () {
            $(".legend_dropdown_list").toggle();
});
jQuery(document).ready(function() { 
   /* var vars = [], hash,activeClass;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for(var i = 0; i < hashes.length; i++)
    {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    activeClass = vars.activeClass;
    $("."+activeClass).addClass('active');
    console.log(vars.activeClass);
    $(".activeClass").click(function() {
        $('.getActiveClass li.active').removeClass('active');
        $(this).addClass('active');
        console.log(location.search);
    });*/
});
$(document).ready(function(){
    
    
    $('.gradient_slider').owlCarousel({
      loop:true,
      margin:0,
      nav:true,
      dots:true,
      responsive:{
          0:{
              items:1
          },
          600:{
              items:1
          },
          1000:{
              items:1
          }
      }
    })
    initLandingPageContent();
  });



  (function () {

    'use strict';

    $('.input-file').each(function () {
        var $input = $(this),
            $label = $input.next('.js-labelFile'),
            labelVal = $label.html();

        $input.on('change', function (element) {
            var fileName = '';
            if (element.target.value) fileName = element.target.value.split('\\').pop();
            fileName ? $label.addClass('has-file').find('.js-fileName').html(fileName) : $label.removeClass('has-file').html(labelVal);
        });
    });

})();


//const range = document.getElementById('range'),
//tooltip = document.getElementById('tooltip'),
//setValue = ()=>{
//    const
//        newValue = Number( (range.value - range.min) * 100 / (range.max - range.min) ),
//        newPosition = 16 - (newValue * 0.32);
//    tooltip.innerHTML = `<span>${range.value}</span>`;
//    tooltip.style.left = `calc(${newValue}% + (${newPosition}px))`;
//    document.documentElement.style.setProperty("--range-progress", `calc(${newValue}% + (${newPosition}px))`);
//};
//document.addEventListener("DOMContentLoaded", setValue);
//range.addEventListener('input', setValue);

/* full screen js */
function toggleFullScreen(elem) {
  if ((document.fullScreenElement !== undefined && document.fullScreenElement === null) || (document.msFullscreenElement !== undefined && document.msFullscreenElement === null) || (document.mozFullScreen !== undefined && !document.mozFullScreen) || (document.webkitIsFullScreen !== undefined && !document.webkitIsFullScreen)) {
    if (elem.requestFullScreen) {
      elem.requestFullScreen();
    } else if (elem.mozRequestFullScreen) {
      elem.mozRequestFullScreen();
    } else if (elem.webkitRequestFullScreen) {
      elem.webkitRequestFullScreen(Element.ALLOW_KEYBOARD_INPUT);
    } else if (elem.msRequestFullscreen) {
      elem.msRequestFullscreen();
    }
  } else {
    if (document.cancelFullScreen) {
      document.cancelFullScreen();
    } else if (document.mozCancelFullScreen) {
      document.mozCancelFullScreen();
    } else if (document.webkitCancelFullScreen) {
      document.webkitCancelFullScreen();
    } else if (document.msExitFullscreen) {
      document.msExitFullscreen();
    }
  }
}

