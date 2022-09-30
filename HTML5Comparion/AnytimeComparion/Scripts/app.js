// Foundation JavaScript
// Documentation can be found at: http://foundation.zurb.com/docs
//test commit for mike
$(document).foundation({}); // do not remove this Foundation default

function getProjWidth(t){
    var projTitle = $(".proj-title").width();
    var projTitleDuplicate = $(".proj-title-duplicate");
    
    var projOl = $(".proj-ol").width();
    var projOlDuplicate = $(".proj-ol-duplicate");

    var projStatus = $(".proj-status").width();
    var projStatusDuplicate = $(".proj-status-duplicate");

    var lastAccess = $(".tt-last_access-duplicate");
    lastAccessOrig = $(".tt-last_access-orig").width();
    projStatusDuplicate.width(projStatus);
    projTitleDuplicate.width(projTitle);
    projOlDuplicate.width(projOl);
    lastAccess.width(lastAccessOrig);
    
    
//    console.log("projTitle:" + projTitle);
//    console.log("projTitleDuplicate:" + projTitleDuplicate.width());
}
$(document).ready(function () {
    var anytimeScope = angular.element($("#anytime-page")).scope();
    if (angular.isUndefined(anytimeScope)) {
        $("#divHeading").removeClass("hide");
    }

    getProjWidth(80); 
    checkHomepageHeight();
    checkProjLoopHeight();
    //    checkObjectivesHeight();
});

$(document).on("focus", "#copy-clipboard", function(){
    $(this).select(); 
});
$(document).on("focus", "#ClipBoardData", function () {
    $(this).select();
});

$(window).on("resize", Foundation.utils.throttle(function (e) {
    getProjWidth(40);
    checkHomepageHeight();
//    checkObjectivesHeight();
 }, 800));


//resize MyProjects page
function checkProjLoopHeight() {
    var projectsLoopWrap = $(".projects-loop-wrap");

    if (projectsLoopWrap.length > 0) {
        var windowHeight = $(window).height();
        var mobileHeader = $(".mobile-header");
        var mobileHeaderHeight = mobileHeader.length == 0 || mobileHeader.hasClass("hide") ? 0 : mobileHeader.height();
        var footerHeight = $(".tt-footer-wrap").height();
        var zoomLevel = parseFloat((window.outerWidth / window.innerWidth).toFixed(2));

        if (zoomLevel < 1.05) {
            var newMultiLoopWrapHeight = windowHeight - (mobileHeaderHeight + footerHeight);

            $(".projects-loop-wrap").css({
                "overflow": "auto",
                "max-height": newMultiLoopWrapHeight + "px",
                "height": newMultiLoopWrapHeight - 125 + "px"
            });
        }
    }
}

function checkHomepageHeight() {

    var windowHeight = $(window).height();
    var mobileHeaderHeight = $(".mobile-header").height();
    var footerHeight = $(".tt-footer-wrap").height();
    var zoomLevel = parseFloat((window.outerWidth / window.innerWidth).toFixed(2));

    if (zoomLevel == 1) {
        var newMultiLoopWrapHeight = windowHeight - (mobileHeaderHeight + footerHeight);

        $(".main-content").css({
            //"overflow": "auto",
            //        "min-height": homePagecontentHeight + "px",
            "height": newMultiLoopWrapHeight - 20 + "px"
        });
    }
}

$(window).on("resize", Foundation.utils.throttle(function (e) {
    checkProjLoopHeight();
//    checkObjectivesHeight();
}, 800));

/*function checkObjectivesHeight() {

    var windowHeight = $(window).height();
    var mobileHeaderHeight = $(".mobile-header").height();
    var footerHeight = $(".tt-footer-wrap").height();

    var newMultiLoopWrapHeight = windowHeight - (mobileHeaderHeight + footerHeight);
    
    $(".overflow-fix").css({
        "max-height" : newMultiLoopWrapHeight - 200 + "px",
    });
}
checkObjectivesHeight();*/

var tooltip_count = 0;

//***** Resizable divs *****//
/*var prevX = -1;
var $leftDiv = $(".tt-sr-left");
var $rightDiv = $(".tt-sr-right");

var $leftDivWidth = $leftDiv.width();
var $rightDivWidth = $rightDiv.width();

var $window = $(document).width();
var lowerLimit = 200;
var upperLimit = 900;

$( ".tt-resize-div-handle" ).draggable({ 
    axis: "x",
    containment: ".tt-resize-div-wrap", 
    scroll: false ,
    drag: function(e, ui) {
//        console.log(e.pageX);
        console.log("ui:" + ui.position.left);
        if(ui.position.left >= lowerLimit && ui.position.left <= upperLimit) {
//            return false;
        
        var lefttDivWidth = ui.position.left;
        var rightDivWidth = $window - lefttDivWidth;
        console.log("tt", lefttDivWidth, rightDivWidth);
        $leftDiv.css({
            "width": lefttDivWidth
            })
        $rightDiv.css({
            "width": rightDivWidth
            })
        } else {
            if(ui.position.left < lowerLimit) {
                ui.position.left = lowerLimit;
            } else {
                ui.position.left = upperLimit;
            }
            
        }
        
        // Detect dragged left or right
        if(prevX == -1) {
            prevX = e.pageX;    
            return false;
        }
        
        // dragged left
        if(prevX > e.pageX) {
            
            console.log($leftDivWidth  + $rightDivWidth);
           
        }
        // dragged right
        else if(prevX < e.pageX) { 
//            console.log("dragged right");
            
        }
        prevX = e.pageX;
    }
});*/

//***** date pickers *****//
//$( "#setSchedDatePicker" ).datepicker();


$(document).on("click", ".force-close-drowdown", function () {
    $(".f-dropdown").removeClass("open");
});

//***** Close dropdown via close button *****//
Foundation.libs.dropdown.close($(".force-close-drowdown"));

var previousScrollTop = 0;
var isProcessing1 = false, isProcessing2 = false;

$(window).bind("scroll", Foundation.utils.throttle(function (e) {
    var scrollTop = $(window).scrollTop();

    if (previousScrollTop < scrollTop || scrollTop == 0) {
        previousScrollTop = scrollTop;
    } else {
        scrollTop += previousScrollTop;
    }

//    var elementOffset = $(".tt-sticky-element").offset().top;
//    var currentElementOffset = (elementOffset - scrollTop);
    var scope = angular.element($("#main-body")).scope();
    var anytimeScope = angular.element($("#anytime-page")).scope();

    if (scrollTop >= $(".tt-header").outerHeight()) {

        if ($(".tt-sticky-element").length > 0 && !$(".tt-sticky-element").hasClass("fixed")) {
            //console.log("fixed");
            $(".tt-sticky-element").addClass("fixed");

            if ($(".original-info-doc").outerHeight() > 90) {
                //reducing extra scrolling after applying "fixed" class on ".tt-sticky-element" element for multi-pairwise
                //$("html, body").animate({ scrollTop: "-=" + ($(".tt-sticky-element").outerHeight() + "px" }, 0);
                $("html, body").animate({ scrollTop: "+=" + ($(".tt-sticky-element").outerHeight() / 3) + "px" }, 0);
            } //else {
            //    $("html, body").animate({ scrollTop: "-=" + ($(".original-info-doc").outerHeight() * 0.75) + "px" }, 0);
            //}
        }

        //multi ratings
        if ($("#anytime-page").length > 0) {
            $(".space-before-judgment-for-mobile").show();

            //if (!$(".tt-sticky-element-ratings").hasClass("fixed-multi-ratings")) {
            //    //console.log("fixed");
            //    $(".tt-sticky-element-ratings").addClass("fixed-multi-ratings");
            //    $(".fixed-multi-ratings").draggable();

            //    //            if(!$(this).hasClass("ui-draggable-disabled")){
            //    $(".tt-sticky-element-ratings").removeClass("multi-ratings-draggable-disabled");
            //    //            }
            //}

            try {
                if (scope.isMobile()) {
                    $(".other-info-doc").removeClass("hide");
                    $(".legend").removeClass("hide");
                    $(".other-info-doc-height").removeClass("hide");

                    $(".original-info-doc").addClass("hide");
                    $(".original-legend").addClass("hide");

                    if (scope.framed_info_docs) {
                        $(".editable-content-height").addClass("hide");
                    }
                }

            } catch (e) {
            }

            if (angular.isDefined(anytimeScope) && isProcessing1 == false) {
                isProcessing1 = true;
                isProcessing2 = false;
                anytimeScope.callResizeFrameImagesProportionally(50, "app.js 1");
            }
        }


    } else {
        $(".space-before-judgment-for-mobile").hide();

        if ($(".tt-sticky-element").hasClass("fixed")) {
            //console.log("not fixed");
            $(".tt-sticky-element").removeClass("fixed");
        }

        $(".other-info-doc").addClass("hide");
        $(".legend").addClass("hide");
        $(".other-info-doc-height").addClass("hide");

        if ($(".parent-node-info-text").html() != "" || scope.is_AT_owner) {
            $(".original-info-doc").removeClass("hide");
            $(".original-legend").removeClass("hide");

        }

        if (scope.framed_info_docs) {
            $(".editable-content-height").removeClass("hide");
        }

        if (angular.isDefined(anytimeScope) && isProcessing2 == false) {
            isProcessing2 = true;
            isProcessing1 = false;
            anytimeScope.callResizeFrameImagesProportionally(50, "app.js 2");
        }


        //multi ratings
        if ($(".tt-sticky-element-ratings").hasClass("fixed-multi-ratings")) {
            //console.log("not fixed");
            //$(".fixed-multi-ratings").draggable("disable");
            $(".tt-sticky-element-ratings").removeClass("fixed-multi-ratings");

            $(".tt-sticky-element-ratings").addClass("multi-ratings-draggable-disabled");
        }
    }

}, 200));


//***** START detect scrolling on div*****//
$(window).scroll(function () {
    var ttHeader = $(".tt-header").height();
    var scroll = $(window).scrollTop();
//    var editPencilIcon = $(".mobile-edit-pencil-icon");
    var StickyHeader = $(".tt-sticky-header");

    if (scroll > ttHeader) {
        
        StickyHeader.removeClass("mobile-sticky").addClass("scrolling");
        StickyHeader.css({
            "top": "0px",
        });
        
     

    } else {

        StickyHeader.addClass("mobile-sticky").removeClass("scrolling");
        StickyHeader.css({
            "top": "inherit",
        });
        
      
    }

});
//***** END detect scrolling on div*****//

//***** START detect overlapping div*****//
//checkInnerScrollBars();
$(window).on("resize", Foundation.utils.throttle(function (e) {
    checkInnerScrollBars();
 }, 800));

$(window).scroll(function (e) {
    //checkInnerScrollBars();
 });

$(document).ready(function () {
    $(document).mousemove(function(event){
        checkInnerScrollBars();
        //console.log(event.pageX + ", " + event.pageY);
    });
 });

$(document).on("click",Foundation.utils.debounce(function (e) {
    checkInnerScrollBars();
 }, 300, true));


function checkInnerScrollBars() {

    try{
//        var ttQuestion = $(".tt-question-title").scrollTop();
        var stickyHeader = $(".tt-sticky-header");
        var div1 = $(".ds-div-trigger");
        var div2 = $(".ds-div-bottom");
        var div_scroll_wrap = $(".detect-scroll-wrap");
        var div1_top = div1.offset().top;
        var div2_top = div2.offset().top;
        var div1_bottom = (div1_top + div1.height());
        //var screen_height = window.outerHeight;
        //var screen_width = window.outerWidth;
        //if (screen_height < 900 && screen_height > 700 && screen_width > 1000) {
        //    div1_bottom = (div1_top + div1.height()) * .75;
        //}
        var div2_bottom = div2_top + div2.height();
        if (div1_bottom >= div2_top && div1_top < div2_bottom) {
            div_scroll_wrap.fadeIn();
        } else {
            div_scroll_wrap.fadeOut();

        }
        
        if( $(window).width() < 1025) {
           stickyHeader.removeClass("desktop-scrolling");
        }else{
           
            if (div1_top < 62) {
    //            console.log(div1_top);
                stickyHeader.addClass("desktop-scrolling");
            }else{
                stickyHeader.removeClass("desktop-scrolling");
            }
        }
    }
    catch(e){

    }
}
//***** END detect overlapping div*****//

$(document).ready(function () { 
    //***** START detect if window is too small for desktop *****//
    $(document).ready(function(event){
        checkBrowserHeightWidth();
        checkWindowHeight();
    });
    
    $(document).mousemove(function(event){
        checkBrowserHeightWidth();
        checkWindowHeight();
    });

    $(window).on("resize", Foundation.utils.throttle(function (e) {
        checkBrowserHeightWidth();
        checkWindowHeight();
     }, 800));

    function checkBrowserHeightWidth(){
        var $browserHeight = $(window).height();
        var $browserWidth = $(window).width();
        var $body = $("body");
        var $tbodyWrap = $(".tbody-wrap");
        var $tableBody = $(".tt-table-body");

        var $newTbodyHeight =  $browserHeight - 250;
    //    console.log("$newTbodyHeight:" + $newTbodyHeight);
        $tableBody.css({
            "max-height": $newTbodyHeight + "px",
        });

        if($browserHeight < 550 && $browserWidth > 1024 ){
            $body.css({
                "overflow": "inherit", 
            });


        }
        else if($browserWidth < 1024 ){
            $tbodyWrap.removeClass("tt-table-body");
        }
        else if($browserWidth > 1024 ){
            $tbodyWrap.addClass("tt-table-body");
        }
        else{
            /*$body.css({
                "overflow": "hidden", 
            });*/

        }

    }

    //***** END detect if window is too small for desktop *****//

    
    //***** START height for tt-main-body-wrap in SECTION *****//
    
    
    function checkWindowHeight(){
       /* var $windowHeight = $(window).height();

        var $mainBodyWrap = $(".tt-main-body-wrap");
        var $currentPage = $mainBodyWrap.attr("data-url");
        var $autoResizeContent = $(".tt-auto-resize-content");
        var $footerWrap = $(".tt-footer-wrap").height();
        var $footerNav = $(".tt-judgements-footer-nav").height();

        var $newHeight = $windowHeight - ($footerNav + $footerWrap) - 60;
        var $ar_newHeight = $newHeight - 120;

        if($currentPage != "my-projects"){
            if($autoResizeContent.hasClass("tt-homepage")){
                $autoResizeContent.css({
                    "height":$ar_newHeight + 50,
                });
    //            console.log("has class");
            }
            else{
                $autoResizeContent.css({
                    "height":$ar_newHeight - 20,
                });
    //            console.log("no class");
            }

             $mainBodyWrap.css({
                    "height":$newHeight - 5
                });
        }

        return;*/
    }
    
    
    //***** END height for tt-main-body-wrap in SECTION *****//
    
    
    //***** START height for tt-comments-archives in comments area *****//
    var $windowHeight = $(window).height();
    var $commentWrap = $(".tt-comments-archives") ;
    var $theCommentHeight = 350;
    
    $commentWrap.css({ height: $windowHeight - $theCommentHeight });
    
    $(window).on("resize", Foundation.utils.throttle(function (e) {
        $commentWrap.css({ height: $windowHeight - $theCommentHeight });
        
//        $theStickyWrap.css({ height: $windowHeight - $elementHeight });
//        $theStickyContent.css({ height: $windowHeight - $elementHeight });
        
        
        
    }, 800));
    
    //***** END height for tt-comments-archives in comments area
    


    //***** START GLOBAL slick sliders*****//
    //$(".tt-judgements-slider").slick({
    //    initialSlide: 0,
    //    adaptiveHeight: true,
    //    infinite: true,
    //    swipe: false,
    //    prevArrow: '<span class="icon-tt-chevron-left tt-slider-arrow prev"></span>',
    //    nextArrow: '<span class="icon-tt-chevron-right tt-slider-arrow next"></span>'
    //});
   
    $(document).on("click", ".cancel-sf", Foundation.utils.debounce(function (e) {
        $(".cancel-tg-sf").click();
        
    }, 300, true));
    
    
            //Start of Mike A's Section for design
            //comment forms
            $("#tt-comment-form").on("valid.fndtn.abide", Foundation.utils.debounce(function (e) {

                var $comList = $(".tt-comment-list");
                var theComment = $('textarea[name="tt-comm-textarea"]');
                var theCommentLayout = '<div class="columns tt-com-single"><h4>New Comments Name</h4><div class="tt-com-date">                <span class="action">Group 1</span>                 <span class="action">Group 2</span>                 <span class="action">May 1, 2015</span>              </div>            <p>' + theComment.val() + '</p></div>';

                $(theCommentLayout).hide().prependTo($comList).fadeIn(800);
                theComment.val("");


            }, 800, false));
            //end of comment forms

            //toggle comments
            $(document).on("click", ".toggleComments", Foundation.utils.debounce(function (e) {
                var leftCol = $(".tt-left-col"); //the comments wrap
                var rightCol = $(".tt-right-col"); //the right content wrap
                var leftBtn = $(".tc-btn-left");
                var rightBtn = $(".tc-btn-right");

                leftBtn.toggle();
                rightBtn.toggle();
                rightCol.toggle();
                leftCol.toggleClass("large-12 large-9");
                //$(".tt-judgements-slider").slick("setPosition");//reload the slick slider to make it responsive

            }, 100, true));
    
    //***** START togglers *****//

    $(document).on("click", ".tt-toggler", Foundation.utils.debounce(function (e) {
        var toggleWhat = $(this).attr("data-toggler");
        switch (toggleWhat) {
                
            case "tg-class-checkbox": //toggles class and title attribute
                var thisClass = $(this).attr("data-class");
                var thisSpan = $(this).find("span.icon ");
                var thisCheckbox = $(this).find('input[type="checkbox"]');
                
                thisSpan.toggleClass(thisClass);
                
                if(thisCheckbox.is(":checked")){
                    thisCheckbox.prop("checked", false);
                }else{
                    thisCheckbox.prop("checked", true);
                }
                
                break;
                
            case "tg-class-checkbox-inverted": //toggles class and title attribute
                var thisClass = $(this).attr("data-class");
                var thisSpan = $(this).find("span.icon ");
                var thisCheckbox = $(this).find('input[type="checkbox"]');
                
                thisSpan.toggleClass(thisClass);
                
                if(thisCheckbox.is(":checked")){
                    thisCheckbox.prop("checked", false);
                }else{
                    thisCheckbox.prop("checked", true);
                }
                
                break;
                
            case "tg-mic": //mute unmute microphone
                var thisWrap = $(this);
                var thisSpan = $(this).find("span.icon ");
                thisWrap.toggleClass("disabled");
                thisSpan.toggleClass("icon-tt-microphone icon-tt-microphone-mute");
                break;
            
            case "tg-pause-play": //pause play pipe
                var thisId = $(this).attr("id");
                var thisWrap = $(this);
                var thisSpan = $(this).find("span.icon ");
                thisSpan.toggleClass("icon-tt-pause icon-tt-play");
                
                if(thisId == "TTpause"){
                    thisWrap.attr("id", "TTstart");
                    thisSpan.attr("data-event", "resume");
                }
                if(thisId == "TTstart"){
                    thisWrap.attr("id", "TTpause");
                    thisSpan.attr("data-event", "pause");
                }
                    
                break;
                
            case "tg-s":
                $(".tg-s").slideToggle();
                break;
            
            case "tg-sf":
                $(".tg-sf").slideToggle();
                $(this).toggleClass("close open");
                if($(this).hasClass("open")){
                    $(this).text("Cancel");
                }else{
                    $(this).text("Edit Data");
                }
                    
                break;

            case "fmobile":
                $(this).toggleClass("down up"); 
                
                var thisWrap = $(".footer-nav-mobile");
                var thisSpan = $(this).find("span.icon");
                thisSpan.toggleClass("icon-tt-sort-down icon-tt-sort-up");
                thisWrap.toggleClass("up down");

              
                break;

            case "tg-legend":
                $(".tg-legend").fadeToggle();
                var thisSpan = $(this).find("span.text");
                if ($(thisSpan).text() == "View Legend")
                    $(thisSpan).text("Hide Legend");
                else
                    $(thisSpan).text("View Legend");
                break;

            case "tg-site-map":
                var id = $(this).attr("data-toggler-id");
                $(".tg-sm-" + id).slideToggle();
                var thisSpan = $(this).find("span.tt-sm-icon");
                thisSpan.toggleClass("icon-tt-minus-circle icon-tt-plus-circle");
                break;

            case "tg-mobileNav":
                $(".tg-mobileNav").slideToggle();
                break;

            case "tg-mNav-sub":
                $(".tg-mNav-sub").slideToggle();
                var thisSpan = $(this).find("span.rightArrow");
                thisSpan.toggleClass("icon-tt-chevron-down icon-tt-chevron-up");
                break;

            case "tg-accordion":
                var id = $(this).attr("id");
                $(".tg-accordion-" + id).slideToggle();
                $(".ep-" + id).toggleClass("hide");
                var thisSpan = $(this).find("span.icon");
                thisSpan.toggleClass("icon-tt-plus-square icon-tt-minus-square");
                break;

            case "tg-accordion-sub":
                var id = $(this).attr("id");
                $(".tg-accordion-sub-" + id).slideToggle();
                $(".ep-sub-" + id).toggleClass("hide");
                var thisSpan = $(this).find("span.icon");
                thisSpan.toggleClass("icon-tt-plus-square icon-tt-minus-square");
                break;
            
            case "tg-tree-nav":
                var id = $(this).attr("id");
                $(".tg-tree-nav-" + id).toggle();
                var thisSpan = $(this).find("span.icon");
                thisSpan.toggleClass("icon-tt-plus-square icon-tt-minus-square");
                
                console.log(id);
                break;
            
            case "tg-tree-nav-sub-first":
                var id = $(this).attr("id");
                $(".tg-tree-nav-sub-first-" + id).toggle();
                var thisSpan = $(this).find("span.icon");
                thisSpan.toggleClass("icon-tt-plus-square icon-tt-minus-square");
                
                console.log(id);
                break;
            
            case "tg-tree-nav-sub-second":
                var id = $(this).attr("id");
                $(".tg-tree-nav-sub-second-" + id).toggle();
                var thisSpan = $(this).find("span.icon");
                thisSpan.toggleClass("icon-tt-plus-square icon-tt-minus-square");
                
                console.log(id);
                break;
            
            case "tg-obj-content":
//                var id = $(this).attr("id");
                $(".tg-obj-content").toggle("slide");
//                var thisSpan = $(this).find("span.icon");
                
                break;

            default:
                var id = $(this).attr("id");
                $(".tg-" + id).slideToggle();
                var thisSpan = $(this).find("span.icon");
                var thisText = $(this).find("span.text");
                if (thisText.text() == "Show"){
                    thisText.text("Hide")}
                else{
                    thisText.text("Show");
                }
                thisSpan.toggleClass("icon-tt-plus-square icon-tt-minus-square");
                break;
        }
    }, 300, true));
    //***** END togglers *****//

    //***** START custom modals *****//
    $(document).on("click", ".openDetailsWrap", Foundation.utils.debounce(function (e) {
        $(".details-wrap").fadeIn();
    }, 300, true));

    $(document).on("click", ".close-details", Foundation.utils.debounce(function (e) {
//        $(this).parent().hide();
        $(".details-wrap").fadeOut();
    }, 300, true));

    //**  modal bindings **//
    /**-- Use this bindings if you want to OPEN or CLOSE a modal in other button or in a flow --**/
    
    /**-- PIPE --**/
    $("a.open-first").on("click", function () {
        $("#tt-s-modal").foundation("reveal", "open");
    });
    
    /**-- MY PROJECTS PAGE --**/
    $("a.back-to-parent.create-project").on("click", function () {
        $("#selectProjMode").foundation("reveal", "open");
    });
    
    $("a.back-to-parent.create-new").on("click", function () {
        $("#createNewProj").foundation("reveal", "open");
    });

    $("a.back-to-parent.upload-file").on("click", function () {
        $("#uploadFile").foundation("reveal", "open");
    });

    //***** END custom modals
    
    
    //***** START togglePart-columns *****//bar-a
    //**-- show overall result --**//
    $(document).on("click", "input[class='togglePart-columns']", function () {

        var theColumn = $(this).attr("data-name");
        var isOn = $(this).is(":checked");

        if (isOn == true) {
            $(".tt-" + theColumn).fadeIn();
        }
        else {
            $(".tt-" + theColumn).fadeOut();
        }
        getProjWidth(0);
    });
    //***** END togglePart-columns *****//
    
    //***** START hide elements if screen is lower 1200px *****//
   /* $(window).on("resize", Foundation.utils.throttle(function (e) {

        $windowHeight = $(window).height();
        $windowWidth = $(window).width();
        $hideElement = $(".hide-element");
        $hideElementCheckbox = $(".hide-element-checkbox");

        if ($windowWidth < 1200 && $windowHeight < 900){
            $hideElement.hide();
//            $hideElementCheckbox.prop("checked", false);
            $hideElementCheckbox.removeProp("checked");
        }
        
        else {
            $hideElement.show();
//            $hideElementCheckbox.prop("checked", true);
//            console.log("higher than 1200 and 1024");
        }

    }, 800));*/
    //***** END hide elements if screen is lower 1200px *****//
    
    

    //*****START form validations *****//
    /** ----- send comments ------**/
        //copied to TeamTime.aspx

    /** ----- send feedback ------**/
    $("#tt-sf-form")
    .on("valid.fndtn.abide", function () {
        //var invalid_fields = $(this).find('[data-invalid]');
        //console.log(invalid_fields);
        $(".tt-sf-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box success radius tt-alert msg-stat">           <h3>Message Sent with a smile :)          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");

        //do ajax here

    })
    .on("invalid.fndtn.abide", function () {
        //console.log("valid!");
        $(".tt-sf-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <h3>Oooops! we have error sending your message          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");
    });

    /** ----- LOGIN ------**/
    $("#tt-login-form")
    .on("valid.fndtn.abide", function () {
        //var invalid_fields = $(this).find("[data-invalid]");
        //console.log(invalid_fields);
        $(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box success radius tt-alert msg-stat">           <h3>Logged in successfully! Redirecting...          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");
    })
    .on("invalid.fndtn.abide", function () {
        //console.log("valid!");
        $(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <h3>Your details are incorrect. Kindly try again.         <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");
    });

    /**-- JOING MEETING --**/

    /** ----- LOGIN ------**/
    $("#tt-login-form")
    .on("valid.fndtn.abide", function () {
        //var invalid_fields = $(this).find("[data-invalid]");
        //console.log(invalid_fields);
        $(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box success radius tt-alert msg-stat">           <h3>Logged in successfully! Redirecting...          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");
    })
    .on("invalid.fndtn.abide", function () {
        //console.log("valid!");
        $(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <h3>Your details are incorrect. Kindly try again.         <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");
    });

    $(document).on("click", ".tt-joinMeeting-btn", Foundation.utils.debounce(function (e) {

        if ($('input[name="meetingId"]').val() == "") {

            $(".jm-error").html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <span>Your details are incorrect. Kindly try again.        <a href="#" class="close">&times;</a>          </span>        </div>      </div>');
        }
        if (!$.isNumeric($('input[name="meetingId"]').val())) {

            $(".jm-error").html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <span>Invalid Meeting ID. Kindly try again.       <a href="#" class="close">&times;</a>          </span>        </div>      </div>');
        }
        else {
            $(".jm-error").html("");
            $("#joinMeetingEval").foundation("reveal", "open");
        }
        $(document).foundation("reflow");
    }, 500, true));

    /** ----- ACCOUNT SETTINGS ------**/
    /**-- account information --**/
    $("#tt-ai-form")
    .on("valid.fndtn.abide", function () {
        //var invalid_fields = $(this).find("[data-invalid]");
        //console.log(invalid_fields);
        $(".tt-ai-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box success radius tt-alert msg-stat">           <h3>Account Information Updated!          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");


    })
    .on("invalid.fndtn.abide", function () {
        //console.log("valid!");
        $(".tt-ai-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <h3>Oooops! we have error updating your info.          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");
    });

    /**-- change password --**/
    $("#tt-cp-form")
    .on("valid.fndtn.abide", function () {
        //var invalid_fields = $(this).find("[data-invalid]");
        //console.log(invalid_fields);
        $(".tt-cp-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box success radius tt-alert msg-stat">           <h3>Account Information Updated!          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");

        //do ajax here
    })
    .on("invalid.fndtn.abide", function () {
        //console.log("valid!");
        $(".tt-cp-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <h3>Oooops! we have error updating your info.          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");
    });

    /**-- my preference --**/
    $("#tt-mp-form")
    .on("valid.fndtn.abide", function () {
        //var invalid_fields = $(this).find("[data-invalid]");
        //console.log(invalid_fields);
        $(".tt-mp-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box success radius tt-alert msg-stat">           <h3>Preferences Updated!          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");

        //do ajax here

    })
    .on("invalid.fndtn.abide", function () {
        //console.log("valid!");
        $(".tt-mp-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <h3>Oooops! we have error updating your info.          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(document).foundation("reflow");
    });
    //***** END form validations *****//



    
    //*****START Responsive tables *****//
    //var switched = false;
    //var updateTables = function () {
    //    if (($(window).width() < 767) && !switched) {
    //        switched = true;
    //        $("table.responsive").each(function (i, element) {
    //            splitTable($(element));
    //        });
    //        return true;
    //    }
    //    else if (switched && ($(window).width() > 767)) {
    //        switched = false;
    //        $("table.responsive").each(function (i, element) {
    //            unsplitTable($(element));
    //        });
    //    }
    //};

    //$(window).load(updateTables);
    //$(window).on("redraw", function () { switched = false; updateTables(); }); // An event to listen for
    //$(window).on("resize", updateTables);


    //function splitTable(original) {
    //    original.wrap("<div class='table-wrapper' />");

    //    var copy = original.clone();
    //    copy.find("td:not(:first-child), th:not(:first-child)").css("display", "none");
    //    copy.removeClass("responsive");

    //    original.closest(".table-wrapper").append(copy);
    //    copy.wrap("<div class='pinned' />");
    //    original.wrap("<div class='scrollable' />");

    //    setCellHeights(original, copy);
    //}

    //function unsplitTable(original) {
    //    original.closest(".table-wrapper").find(".pinned").remove();
    //    original.unwrap();
    //    original.unwrap();
    //}

    //function setCellHeights(original, copy) {
    //    var tr = original.find("tr"),
    //        tr_copy = copy.find("tr"),
    //        heights = [];

    //    tr.each(function (index) {
    //        var self = $(this),
    //            tx = self.find("th, td");

    //        tx.each(function () {
    //            var height = $(this).outerHeight(true);
    //            heights[index] = heights[index] || 0;
    //            if (height > heights[index]) heights[index] = height;
    //        });

    //    });

    //    tr_copy.each(function (index) {
    //        $(this).height(heights[index]);
    //    });
    //}
    //*****END Responsive tables *****//
    
    //*****START show overall result  *****//
    $("input[name='cb-showOverallResult']").click(function () {
        var isOn = $(this).is(":checked");
        var togglePmPartView = $(".tt-j-show-overall-result");

        if (isOn == true) {
            togglePmPartView.slideDown();
        }
        else {
            togglePmPartView.slideUp();
        }
    });
    //*****END show overall result  *****//

    $(window).on("resize", Foundation.utils.throttle(function (e) {
        var anytimeScope = angular.element($("#anytime-page")).scope();
        if (angular.isDefined(anytimeScope)) {
            ////console.log("app.js: Calling change_info_doc_image_size...");
            setTimeout(function() {
                anytimeScope.change_info_doc_image_size();
            }, 200);
            setTimeout(function () {
                anytimeScope.change_info_doc_image_size();
            }, 400);
        }
    }, 700));

    $(window).on("resize", function() {
        var mainScope = angular.element($("#main-body")).scope();
        if (angular.isDefined(mainScope)) {
            mainScope.isMobileDevice = null;
        }
    });

    //*****START Responsive MODALS on resize *****//
    $(window).on("resize", Foundation.utils.throttle(function (e) {
        $windowHeight = $(window).height();
        $modalWrap = $(".tt-modal-wrap");

        if ($windowHeight < 768) {
            $modalWrap.removeClass("fixed-modal");
        }
        else {
            $modalWrap.addClass("fixed-modal");
        }
    }, 300));
    //*****END Responsive MODALS *****//

    /**-- toggle open close folder --**/
    $(document).on("click", ".toggleOpenCloseFoler", Foundation.utils.debounce(function (e) {
        var $this = $(this).find("span");
        $this.toggleClass("icon-tt-folder-close-solid icon-tt-folder-open-solid");
    }, 500, true));

    
    //*****START smooth scroll on elements with link *****//
    //**-- add hash on data-link attribute --**//

    $(".smoothScrollLink").on("click", function (e) {
      var id = $(this).attr("data-link"), // ex: #yourTarget   then add id on your targeted element  id="yourTarget"
        $elem = $(id);
     
      if ($elem.length > 0) {
        e.preventDefault();
          
          setTimeout(function(){
            TweenMax.to(window, 3, { // this is scroll power. Increasing the number will have fast scrolling
              scrollTo: {
                y: $elem.offset().top
              },
              ease: Circ.easeInOut
            });
            if (window.history && window.history.pushState) {
              //update the URL.
              history.pushState("", document.title, id);
            }
          }, 800);
      }
    });
    //*****END smooth scroll on elements with link *****//
    
    
    //***** START smooth wheelscroll *****//
    //**-- add smooth scrolling using mouse wheel --**//
   /* $(function () {
      var $window = $(window);
      var isTweening = false;
      document.onmousewheel = function () {
        customScroll();
      }
      if (document.addEventListener) {
        document.addEventListener("DOMMouseScroll", customScroll, false);
      }

      function customScroll(event) {
        var delta = 0;
        if (!event) {
          event = window.event;
        }
        if (event.wheelDelta) {
          delta = event.wheelDelta / 120;
        } else if (event.detail) {
          delta = -event.detail / 3;
        }
        if (delta) {
          var scrollTop = $window.scrollTop();
          var finScroll = scrollTop - parseInt(delta * 100) * 3;
          TweenMax.to($window, 2, { // this is scroll power. Increasing the number will have fast scrolling
            scrollTo: {
              y: finScroll,
              autoKill: true
            },
            ease: Power4.easeOut,
            autoKill: true,
            overwrite: 5,
            onComplete: function () {
            }
          });
        }
        if (event.preventDefault) {
          event.preventDefault();
        }
        event.returnValue = false;
      }
    });*/
    //***** END smooth wheelscroll *****//
    
    

});//end doc ready
  
  
// Prevent jQuery UI dialog from blocking focusin
function stopFocus(){
    
    $(document).on("focusin", function(e) {
        if ($(e.target).closest(".mce-window, .moxman-window").length) {
            e.stopImmediatePropagation();
        }
    });

}
stopFocus();

//loading 
function show_loading_modal() {
    $(".fullwidth-loading-wrap").removeClass("hide");
    $(".fullwidth-loading-wrap").css("display", "inline");
}
function hide_loading_modal() {
    //    setTimeout(function () {
    // $(".wrt_path").addClass("yellow-back-pulse");
    //    }, 1000);


    setTimeout(function () {
    //   $(".wrt_path").removeClass("yellow-back-pulse");
    $(".wrt_path").fadeOut();
    }, 5000);

    setTimeout(function () {
        $(".fullwidth-loading-wrap").fadeOut();
        $(".load-canvas-gif").hide();
    }, 500);
    $(".fullwidth-loading-wrap").fadeOut();


    //Commented this out, causes conflict in initializinf tinymce in multi screens
    //setTimeout(function(){
    //$(document).foundation("reflow");
    //}, 1500);
}
//End of Loading




$(document).on("opened.fndtn.reveal", "#InviteParticipant[data-reveal]", function () {
    
    $("html, body").animate({ scrollTop: 0 }, "slow");
});
