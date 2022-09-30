/// <reference path="../angular.js" />
/// <reference path="~/Scripts/angular-route.js" />
/// <reference path="~/Scripts/angular.min.js" />
/// <reference path="~/Scripts/angular-resource.js" />
/// <reference path="../fastclick.js" />
/// <reference path="../angular.intellisense.js" />
/// <reference path="../judgment.js" />

var stringResources = {
    loadingModalMessage: {
        //default: ["Loading. Please wait...", "Almost ready", "Done"],
        moveStep: ["Moving to step ", "Moving to step ", "Done"],
        error: ["", "", "Error loading"],
        anytime: ["Starting anytime", "Loading project", "Redirecting..."]
    },
    yellowLoadingIcon: {
        save: ["saving...", "&#10004; changes saved"],
        saveAA: ["saving...", "&#10004; changes saved. Moving to the next step"],
        multiSave: ["...", "&#10004 Click Next to proceed and save changes"]
    }
}

var app = angular.module("comparion", ["ngSanitize", "ngTouch", "ngclipboard", "infinite-scroll"]);

app.controller("MainController", ["$scope", "$http", "$window", "$sce", "$timeout", "$location", "$interval", "$interpolate", function ($scope, $http, $window, $sce, $timeout, $location, $interval, $interpolate) {

    $scope.ecProjectStatus = {
        'psActive': 0,
        'psArchived': 1,
        'psTemplate': 2,
        'psMasterProject': 3,
        'psTrash' : 4
    };

    $scope.is_chrome = /Chrome/.test(navigator.userAgent) && /Google Inc/.test(navigator.vendor);
    $scope.is_explorer = navigator.appName == "Microsoft Internet Explorer" || !!(navigator.userAgent.match(/Trident/) || navigator.userAgent.match(/rv:11/)) || (typeof $.browser !== "undefined" && $.browser.msie == 1);
    $scope.is_firefox = navigator.userAgent.indexOf("Firefox") > -1;
    $scope.is_safari = navigator.userAgent.indexOf("Safari") > -1;
    $scope.is_opera = navigator.userAgent.toLowerCase().indexOf("op") > -1;
    if (($scope.is_chrome) && ($scope.is_safari)) { $scope.is_safari = false; }
    if (($scope.is_chrome) && ($scope.is_opera)) { $scope.is_chrome = false; }

    $scope.showFinish = false;
    $scope.showSave = false;
    $scope.projectLockedInfo = { "status": false, "message": "" };

    $scope.browser_recommended = true;
    if ($scope.is_explorer || $scope.is_firefox) {
        //$scope.browser_recommended = false;
    }
    $scope.browser_recommended = true; //force to be true
    if (!$scope.browser_recommended) {
        $(".comparion-content").addClass("hide");
    }

    $scope.showBrowserWarning = function () {
        // console.log("showBrowserWarning: " + (($scope.is_explorer || $scope.is_firefox) && !$scope.output.hideBrowserWarning));
        return ($scope.is_explorer || $scope.is_firefox) && !$scope.output.hideBrowserWarning;
    }

    $scope.showWarningMessageIcon = function () {
        //console.log("showWarningMessageIcon: " + (($scope.is_explorer || $scope.is_firefox) ? $scope.output.hideBrowserWarning : false));
        return ($scope.is_explorer || $scope.is_firefox) ? $scope.output.hideBrowserWarning : false;
    }

    $scope.show_warning_message = function () {
        $scope.output.hideBrowserWarning = false;
        $("#browser-warning-message").fadeIn();
        $scope.$broadcast("check-multi-height", "multi-loop-wrap", "80");
    }

    $scope.close_warning_message = function () {
        if (confirm("Are you sure you want to close this message?") == true) {
            $scope.closeWarningMessage();
        }
    }

    $scope.closeWarningMessage = function () {
        if (!$scope.output.hideBrowserWarning) {
            var pageUrl = baseUrl + ($scope.is_anytime ? "pages/Anytime/Anytime.aspx" : "pages/my-projects.aspx") + "/HideBrowserWarningMessage";
            $("#browser-warning-message").fadeOut();

            $http({
                method: "POST",
                url: pageUrl,
                data: {}
            }).then(function success(response) {
                $scope.output.hideBrowserWarning = true;
                $scope.$broadcast("check-multi-height", "multi-loop-wrap", "80");
            }, function error(response) {
                //console.log(response);
            });
        }
    }

    var loadingTypes = {
        'loadingModal': 1,
        'yellowLoadingIcon': 2,
        'smallLoadingIcon' : 3
    }

    $scope.showLoadingModal = false;
    $scope.checkSignUpLink = function (link) {
        if (link != "" && link != null && link != undefined) {
            $scope.showLoadingModal = true;
        }
    }

    $scope.loadingProgress = 0;
    $scope.openModal = null;    //11646

    $scope.runProgressBar = function (loadingType, start, end, loadingMessage, fFinish) {

        if ($scope.openModal == null || ($scope.openModal !== false && $scope.openModal.length === 0)) {
            //$scope.openModal = $(".reveal-modal.open:not(#SelectStepModal)"); //11646
            //$scope.openModal = $(".reveal-modal.open#tt-c-modal"); //11646 + 11532
        }

        switch(loadingType)
        {
            case loadingTypes.loadingModal:
                $scope.showLoadingModal = true;
                $interval.cancel($scope.progressTimer);
                if (start == 70)
                    $("#LoadingModalMessage").html(loadingMessage[1]);
                if (!fFinish)
                    $scope.loadingProgress = start;
                var intervalStep = 100;
                if (fFinish) {
                    
                    intervalStep = 0.5;
                }
                    

                $scope.progressTimer = $interval(function () {
                    if ($scope.loadingProgress < 30 && $("#LoadingModalMessage").text() == "") {
                        $("#LoadingModalMessage").html(loadingMessage[0]);
                    }
                    if (!fFinish) {
                        var step = (end - $scope.loadingProgress) / end;
                        $scope.loadingProgress += step;
                    }
                    else {
                        $scope.loadingProgress += 1;
                    }
                    if ($scope.loadingProgress >= end) {
                        $("#LoadingModalMessage").html(loadingMessage[2]);
                        $scope.loadingProgress = end;
                        $interval.cancel($scope.progressTimer);
                        $timeout(function () { $scope.showLoadingModal = false; }, 500);
                    }
                }, intervalStep);
                break;
            case loadingTypes.yellowLoadingIcon:
                
                if (fFinish == false) {
                    $(".tt-loading-icon-content").css("width", "auto");
                    $(".icon-tt-loading").fadeIn();
                    $(".tt-loading-icon-wrap").fadeIn();
                    $("#pipe-loading-message").html(loadingMessage[0]);
                    if (end == 100) {
                        $timeout(function () {
                            $(".icon-tt-loading").fadeOut();
                            $("#pipe-loading-message").fadeOut(function () {
                                $("#pipe-loading-message").html(loadingMessage[1]);
                                $(".tt-loading-icon-content").animate({
                                    width: parseInt($("#pipe-loading-message").width() + 50)
                                });
                                
                            }).fadeIn(500);
                        }, 500);
                    }

                }
                    
                if (fFinish)
                    $timeout(function () {
                        $(".tt-loading-icon-wrap").fadeOut();

                        if ($scope.openModal != null && $scope.openModal !== false && $scope.openModal.length > 0) {
                            $scope.openModal.foundation("reveal", "open"); //11646
                        }

                        $scope.openModal = null;    //11646
                    }, 800);

                    
                break;
            case loadingTypes.smallLoadingIcon:
                break;
        }
    }

    $scope.mobile_help_data = ["",""];
    $scope.help_descriptions = {
        'auto_advance': ["Auto Advance", "Move to next step after entering judgment"],
        'expected_value': ["Show Expected Value", "Show Expected Value"],
        'single_line': ["Single Line Mode", "Show graphical slider in single line"],
        'tooltip_mode': ["Tooltip Mode", "Display the information docs in tooltips"],
        'quick_help': ["Show Quick Help Automatically", "Show Quick Help dialog automatically"],
        'show_index': ["Show Index", "Show index of the intermediate results"],
        'anonymous_mode': ["Anonymous Mode", "Hide names/emails of the participants"],
        'hide_offline_users': ["Hide Offline Users", "Hide offline participants in this meeting"],
        'hide_judgments': ["Hide Judgemnts", "Hide judgements of participants"],
        'hide_pie_chart': ["Hide Pie Chart", "Hide Pie Chart in the graphical screen"],
        'hide_for_all': ["Hide For All", "Hide jugments of the pariticpants, only showing the judgment of the logged in participant"],
        'show_to_me': ["Show to Me", "As a project manager, all the judgments of the participants are shown to you only"],
        'resize_info_docs': ["Resize Info Docs", "When project manager resizes the info docs it will be reflected to the participants"],
        'toggle_info_docs': ["Toggle Info Docs", "When project manager toggles the info docs it will be reflected to the participants"],
        'vertical_bars': ["Collapse Bars", "Show selected row in bars with equal height"],
        'archive': ["Archived Projects", "List of projects archived"],
        'delete': ["Deleted Projects", "List of projects deleted"]

    };

    $scope.copyToClipboard = function () {
        alert("Successfully copied to clipboard!");
    }

    $scope.open_help_modal = function(data) {
        $scope.mobile_help_data = data;
        $("#MobileHelpModal").foundation("reveal", "open");
    }

    $scope.isMobileDevice = null;
    $scope.isMobile = function () {
        var isMobile = $scope.isMobileDevice == null ? false : $scope.isMobileDevice;

        if ($scope.isMobileDevice == null) {
            try {
                ///<summary>Detecting whether the browser is a mobile browser or desktop browser</summary>
                ///<returns>A boolean value indicating whether the browser is a mobile browser or not</returns>

                if (sessionStorage.desktop) // desktop storage 
                    isMobile = false;
                else if (localStorage.mobile) // mobile storage
                    isMobile = true;

                // alternative
                var mobile = ["iphone", "ipad", "android", "blackberry", "nokia", "opera mini", "windows mobile", "windows phone", "iemobile"];
                for (var i in mobile) if (navigator.userAgent.toLowerCase().indexOf(mobile[i].toLowerCase()) > 0) isMobile = true;

            }
            catch (e) {

            }

            try {
                var deviceWidth = $("body").width();
                deviceWidth += (deviceWidth < 1025 && document.body.scrollHeight > document.body.clientHeight) ? $scope.getScrollbarWidth() : 0;

                if (!isMobile) {
                    if (deviceWidth < 1025)
                        isMobile = true;
                    else {
                        isMobile = false;
                    }
                }

                //for project list to display projects on tablets
                if ($scope.projects.length > 0) {
                    if (deviceWidth > 640)
                        isMobile = false;
                }

                if ($scope.is_anytime) {
                    if ($scope.output.page_type == "atShowGlobalResults" || $scope.output.pairwise_type == "ptGraphical") {
                        if (deviceWidth < 1025)
                            isMobile = true;
                        else {
                            isMobile = false;
                        }
                    }
                }
            }
            catch (e) {

            }
            
            $scope.isMobileDevice = isMobile;
        }
        
        return isMobile;
    }

    $scope.scrollbarWidth = 0;
    $scope.getScrollbarWidth = function () {
        if ($scope.scrollbarWidth == 0) {
            var outerDiv = $("<div>").css({ visibility: "hidden", width: 100, overflow: "scroll" }).appendTo("body");
            $scope.scrollbarWidth = outerDiv.width() - $("<div>").css({ width: "100%" }).appendTo(outerDiv).outerWidth();
            outerDiv.remove();
            ////console.log("scrollbar width: " + $scope.scrollbarWidth);
        }

        return $scope.scrollbarWidth;
    }
   
    //==================THese are from ANYTIMECONTROLLER FOOTER=============================

    $scope.columns = [];
    $scope.output = [];
    $scope.is_judgment_screen = true;
    $scope.isPM = false;
    $scope.is_multi = false;
    $scope.orig_infodoc_setting = false;
    $scope.is_AT_owner = 0;
    //this is to detect resize in angular
    $scope.check_offset_for_footer = function () {
        if (angular.element(".mobile-paginator").offsetHeight > 30) {
            $scope.execute_onresize();
        }

    }
    $scope.projects = [];
    $scope.busy = false;
    $scope.incrementProjects = function (TotalProjects) {

            if ($scope.listofallProjects != null) {
                if ($scope.projects.length < $scope.listofallProjects.length) {
                    $scope.busy = true;
                    var ProjectCount = $scope.projects.length;
                    var MaxProjectCount = ProjectCount + 20;
                    //$scope.pageSize = $scope.listofallProjects.length;
                    if (TotalProjects != null) {
                        MaxProjectCount = TotalProjects + 20;
                    }
                    for (i = ProjectCount; i < MaxProjectCount; i++) {
                        if (i < $scope.listofallProjects.length)
                            $scope.projects.push($scope.listofallProjects[i]);
                    }
                    $scope.busy = false;
                }
            }
    }

    $scope.screen_sizes = [{ "option": 0 }];

    $scope.render_infodoc_sizes = function () {
        $scope.$broadcast("render_infodoc_sizes");
    }

    //text limit for turncated names in multi pw

    $scope.text_limit = 15;

    $scope.stepButtons = [];
    $scope.execute_onresize = function () {
        $scope.isMobile();

        var original_tooltip_value = $scope.output.is_infodoc_tooltip;
        var screen_height = window.outerHeight;
        var screen_width = document.documentElement.clientWidth;

        if (screen_width < 330) {
            $scope.text_limit = 11;
            $scope.screen_sizes.option = 0.6;
            // $scope.output.is_infodoc_tooltip = true;
        }
        else if (screen_width < 380) {
            $scope.text_limit = 13;
            $scope.screen_sizes.option = 0.8;
            // $scope.output.is_infodoc_tooltip = true;
        }
        else if (screen_width < 419) {
            $scope.text_limit = 15;
            $scope.screen_sizes.option = 1;
           // $scope.output.is_infodoc_tooltip = true;
        }
        else if (screen_width < 537) {
            $scope.text_limit = 25;
            $scope.screen_sizes.option = 2;
           // $scope.output.is_infodoc_tooltip = true;
        }//below is for desktop
        else if (screen_width < 600 || (screen_width > 1024 && screen_width < 1350)) {
            $scope.screen_sizes.option = 3;
            $scope.text_limit = 30;
           // $scope.output.is_infodoc_tooltip = $scope.orig_infodoc_setting;
            $scope.render_infodoc_sizes();
        }
        else if (screen_width < 641 ) {
            $scope.screen_sizes.option = 3;
           // $scope.output.is_infodoc_tooltip = $scope.orig_infodoc_setting;
            $scope.render_infodoc_sizes();
        }
        else if (screen_width < 716 || (screen_width > 1024 && screen_width < 1600)) {

            $scope.screen_sizes.option = 4;
            //$scope.output.is_infodoc_tooltip = $scope.orig_infodoc_setting;
            $scope.render_infodoc_sizes();
        }
        else if (screen_width < 810 || (screen_width > 1024 && screen_width < 1900)) {
            $scope.screen_sizes.option = 5;
        }
        else if (screen_width < 896 ) {
            $scope.screen_sizes.option = 5.5;
        }
        else {
            $scope.screen_sizes.option = 6;
        }
       
        var actual_window_size = window.innerWidth;

        try {
            //if ($scope.output.page_type == "atNonPWOneAtATime" && $scope.output.non_pw_type == "mtRatings") {
            //    if (actual_window_size <= 1024) {
            //        $("#ratings-dropdown .hidden-toggle-btn").click();
            //    }
            //    else{
            //        $("#ratings-dropdown .close-drop-down").click();
            //    }
            //}

            //if (($scope.output.page_type == "atNonPWAllCovObjs" || $scope.output.page_type == "atNonPWAllChildren") && $scope.output.non_pw_type == "mtRatings") {
            //    if (actual_window_size <= 1024) {
            //        $("#hidden-toggle-btn-0").click();
            //    }
            //    else {
            //        $(".cdd_0").click();
            //    }
            //}

            //if (output.mtype == "mtRatings") {
            //    if (actual_window_size <= 1024) {
            //        $("#hidden-toggle-btn-0").click();
            //    }
            //    else {
            //        $(".cdd_0").click();
            //    }
            //}

            //if ($scope.output.page_type != "atPairwise") {
            //    if (actual_window_size <= 1024) {
            //        $scope.output.is_infodoc_tooltip = true;
            //    }
            //    else {
            //        $scope.output.is_infodoc_tooltip = $scope.orig_infodoc_setting;
            //    }
            //}
            //else {
            //    if (actual_window_size <= 1024) {
            //        $scope.output.is_infodoc_tooltip = $scope.orig_infodoc_setting;;
            //    }
            //    else {
            //        $scope.output.is_infodoc_tooltip = $scope.orig_infodoc_setting;
            //    }
            //}
        }
        catch (e) {

        }

        

        


        //console.log($scope.screen_sizes.option);
        //console.log($scope.text_limit);
       // //console.log(temp_iframe_width + "-" + screen_width);

        //if (screen_width > 1024) {
        //    //console.log(temp_iframe_width);
        //    $("iframe").each(function () {
        //        //$(this)[0].width = temp_iframe_width;
        //       // $(this)[0].align = "center;";
        //    });
        //}
        //else {
        //    $("iframe").each(function () {
        //        $(this)[0].width = "100%";
        //    });
        //}


        //if (screen_width > 1300) {
        //    $("#tt-view-qh-modal").removeClass("medium");
        //    $("#tt-view-qh-modal").addClass("small");
        //}
        //else {
        //    $("#tt-view-qh-modal").removeClass("small");
        //    $("#tt-view-qh-modal").addClass("medium");
        //}

        if (screen_height < 900 && screen_height > 700 && screen_width > 1024) {
            $scope.is_screen_reduced = true;
        }
        else {
          
            $scope.is_screen_reduced = false;
        }

        //for smaller screens
        if (screen_width < 410) {
            $scope.mobile.is_too_small = true;
        }
        else {
            $scope.mobile.is_too_small = false;
        }

    }

    $scope.enabledLastStep = -1;
    $scope.checkStepStatus = function (index, step, isUndefined) {
        if ($scope.output.pipeOptions.dontAllowMissingJudgment) {
            ////console.log("index: " + index + ", step: " + step + ", isUndefined: " + isUndefined + ", enabledLastStep: " + $scope.enabledLastStep + ", current_step: " + $scope.output.current_step + ", output.IsUndefined: " + $scope.output.IsUndefined);

            if ($scope.enabledLastStep > 0) {
                if ($scope.output.IsUndefined) {
                    $scope.enabledLastStep = $scope.output.current_step;
                    ////console.log("enabledLastStep: " + $scope.enabledLastStep);
                }

                if ($scope.enabledLastStep == $scope.output.current_step && !$scope.output.IsUndefined && $scope.output.current_step != $scope.output.total_pipe_steps) {
                    $scope.enabledLastStep = -1;
                }
            }

            if (isUndefined == 1) {
                ////console.log("isUndefined -> enabledLastStep: " + $scope.enabledLastStep);

                if ($scope.enabledLastStep < 0) {
                    $scope.enabledLastStep = parseInt(step);
                    ////console.log("isUndefined -> Changed enabledLastStep: " + $scope.enabledLastStep);
                }

                if (parseInt(step) > $scope.enabledLastStep) {
                    ////console.log("true isUndefined -> step: " + step + ", enabledLastStep: " + $scope.enabledLastStep);
                    return true;
                } else {
                    ////console.log("false isUndefined -> step: " + step + ", enabledLastStep: " + $scope.enabledLastStep);
                    return false;
                }
            } else {
                ////console.log("enabledLastStep: " + $scope.enabledLastStep);

                if ($scope.enabledLastStep < 0) {
                    $scope.enabledLastStep = $scope.output.total_pipe_steps;
                    for (var i = 0; i < $scope.stepButtons.length; ++i) {
                        if ($scope.stepButtons[i][3] == "1" && ($scope.output.current_step != parseInt($scope.stepButtons[i][0]))) {
                            $scope.enabledLastStep = parseInt($scope.stepButtons[i][0]);
                            break;
                        }
                    }
                    ////console.log("Changed enabledLastStep: " + $scope.enabledLastStep);
                }

                if (parseInt(step) > $scope.enabledLastStep) {
                    ////console.log("true -> step: " + step + ", enabledLastStep: " + $scope.enabledLastStep);
                    return true;
                } else {
                    ////console.log("false -> step: " + step + ", enabledLastStep: " + $scope.enabledLastStep);
                    return false;
                }
            }
        } else {
            if ($scope.enabledLastStep < 0) {
                $scope.enabledLastStep = $scope.output.total_pipe_steps;
            }
            
            return false;
        }
    }

    $scope.get_step_display = function (step, current_step, total_step, request, is_mobile) {
        var val1 = 7;
        var val2 = 5;
        var val3 = 6;
        var close_range = 7;
        if ($scope.screen_sizes.option == 6) {
            val1 = 9; val2 = 7; val3 = 8; close_range = 9;
        }
        if ($scope.screen_sizes.option == 5.5) {
            val1 = 8; val2 = 6; val3 = 7; close_range = 8;
        }
        if ($scope.screen_sizes.option == 5) {
            val1 = 7; val2 = 5; val3 = 6; close_range = 7;
        }
        if ($scope.screen_sizes.option == 4) {
            val1 = 6; val2 = 4; val3 = 5; close_range = 6;
        }
        if ($scope.screen_sizes.option == 3) {
            val1 = 4; val2 = 2; val3 = 3; close_range = 4;
        }
        if ($scope.screen_sizes.option == 2) {
            val1 = 3; val2 = 1; val3 = 2; close_range = 3;
        }
        if ($scope.screen_sizes.option <= 1) {
            val1 = 2; val2 = 0; val3 = 1; close_range = 2;
            //alert($scope.mobile.is_too_small);
        }

        if (window.innerWidth > 1024 && window.innerWidth < 1041) {
            //AR: fix for Case 15491
            val1 = 4; val2 = 2; val3 = 3; close_range = 4;
        }
        var last = $scope.stepButtons.length - 2;
        var LeftStep = val1 - (current_step - 1);
        var RightStep = val1 - (total_step - current_step);
        
        
        if (step > (current_step + val2) + Math.max(0, LeftStep) && step != total_step && (current_step + close_range) != total_step) {
            if ($scope.is_anytime && (($scope.stepButtons.length > 0 && step == $scope.stepButtons[last][0]) || step == total_step) && request == "unavailable") {
                return true;
            }
            else if ($scope.is_teamtime && ((current_step + val2 != total_step && step == total_step - 2) || step == total_step) && request == "unavailable")
                return true;
            //return false;
        }
        else if (step <= (current_step - val2) - Math.max(0, RightStep) && step != 1 && (current_step - close_range) != 1) {
            if ((step == (current_step - val3) - Math.max(0, RightStep)) && request == "unavailable") {
                return true;
            }
            if (step == (current_step - val2) - Math.max(0, RightStep) && request == "available" && !$scope.mobile.is_too_small) {
                return true;
            }
            //return false;
        }
        else {
            if (request == "available" && current_step != step) {
                return true;
            }
        }
        return false;
    }

    $scope.signUpLinkFields = [];
    $scope.show_signup_fields = function () {
        if ($scope.signup_mode.indexOf("e") > -1) 
            $scope.signUpLinkFields[0] = true;
        else
            $scope.signUpLinkFields[0] = false;

        if ($scope.signup_mode.indexOf("n") > -1) 
            $scope.signUpLinkFields[1] = true;
        else
            $scope.signUpLinkFields[1] = false;

        if ($scope.signup_mode.indexOf("p") > -1) 
            $scope.signUpLinkFields[2] = true;
        else
            $scope.signUpLinkFields[2] = false;

        if ($scope.signup_mode.indexOf("t") > -1)
            $scope.signUpLinkFields[3] = true;
        else
            $scope.signUpLinkFields[3] = false;
    }
    $scope.footer_up = function () {
        $(".next_step").addClass("back-pulse");
        $(".footer-nav-mobile").removeClass("down");
        $(".fnv-toggler .icon").removeClass("icon-tt-sort-up");
        $(".fnv-toggler").removeClass("down");
        $(".footer-nav-mobile").addClass("up");
        $(".fnv-toggler").addClass("up");
        $(".fnv-toggler .icon").addClass("icon-tt-sort-down");
        if ($scope.output.page_type == "atSensitivityAnalysis") {
            $("body").addClass("remove-scroll");
        }
        $timeout(function () {
            $(".next_step").removeClass("back-pulse");
        }, 2000);
    }

    $scope.footer_down = function () {
        $(".next_step").removeClass("back-pulse");
        $(".mobile .fnv-toggler").addClass("down");
        $(".mobile .fnv-toggler").removeClass("up");
        $(".fnv-toggler").removeClass("up");
        $(".fnv-toggler").addClass("down");
        var thisWrap = $(".footer-nav-mobile");
        var thisSpan = $(".mobile .fnv-toggler").find("span.icon");
        thisWrap.removeClass("up");
        thisWrap.addClass("down");
        thisSpan.removeClass("icon-tt-sort-down");
        thisSpan.addClass("icon-tt-sort-up");
        if ($scope.output.page_type == "atSensitivityAnalysis") {
            $("body").removeClass("remove-scroll");
        }
        $timeout(function () {
            $(".next_step").addClass("back-pulse");
        }, 2000);
    }

    $scope.showExpectedValue = { 'Value': true };
    $scope.showHiddenValue = function () {
        $http.post($scope.baseurl + "pages/Anytime/Anytime.aspx/getExpectedValue", {})
            .then(function success(response) {
                $scope.ExpectedValue = response.data.d;
            });
    }
    $scope.toggleExpectedValue = function (value) {

        $http.post($scope.baseurl + "pages/Anytime/Anytime.aspx/toggleExpectedValue", 
                JSON.stringify({
                    showvalue: value,
                    screentype: $scope.output.page_type
                })
            )
            .then(function success(response) {

                $scope.showExpectedValue.Value = response.data.d;
            });
    }

    $scope.getShowExpectedValue = function () {
        $http.post($scope.baseurl + "pages/Anytime/Anytime.aspx/GetShowExpectedValue",
            JSON.stringify({ screenType: $scope.output.page_type })
        ).then(function success(response) {
            $scope.showExpectedValue.Value = response.data.d;
        });
    }

    $scope.stepRange = { first: 1, last: 0 };
    $scope.fStepList2Long = false;
    $scope.checkStepList = function () {
        $(".nothing").removeClass("hide");
        $(".nothing .icon-tt-loading").show();
        $(".steps_list_content ul").addClass("hide");
        $scope.steps_list = null;
        LoadingScreen.init(LoadingScreen.type.smallLoadingIcon, $(".percentage-value"), $(".nothing"), $(".steps_list_content ul"));
        LoadingScreen.start(100);
        //runLoadingScreen(0, 100);
        var first = 1; var last = -1;
        var left = 250; var right = 250;
        if ($scope.output.total_pipe_steps > 2000) {
            if ($scope.stepRange.last == 0) {
                var tempfirst = $scope.output.current_step - left;
                if (tempfirst <= 0) {
                    right += Math.abs(tempfirst);
                    tempfirst = 1;
                }
                var templast = right + $scope.output.current_step;
                if (templast > $scope.output.total_pipe_steps) {
                    left += templast - $scope.output.total_pipe_steps;
                    templast = $scope.output.total_pipe_steps;
                    tempfirst = $scope.output.current_step - left;
                }
                first = tempfirst;
                last = templast;

                $scope.stepRange.first = first;
                $scope.stepRange.last = last;

                
            }
            $scope.fStepList2Long = true;
        }
        $scope.load_step_list($scope.stepRange.first, $scope.stepRange.last);
    }
    $scope.load_step_list = function (first, last) {
        if ($scope.steps_list == null) {

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/loadStepList",
                data: {
                    first: first,
                    last: last
                }
            }).then(function success(response) {
                //$scope.$apply(function () {
                LoadingScreen.end(100);
                $scope.steps_list = eval("[" + response.data.d + "]");
                $scope.is_pipescreen = $scope.is_anytime || $scope.is_teamtime ? true : false;
                //$scope.showLoadingModal = false;
                
                $scope.scrolltoelement($scope.output.current_step, 0);
                //$scope.get_pipe_data(step, is_AT_owner);
                //console.log(2);
                //});
                // $scope.get_pipe_data(step, is_AT_owner);
                
                $scope.fStepList2Long = false;
            }, function error(response) {
                //////////console.log(response);
                // $scope.get_pipe_data(1, is_AT_owner);
            });
        }
    }

    $scope.load_hierarchies = function (reloadhierarchy) {
        if ($scope.hierarchies == null || reloadhierarchy) {
            $("#tt-h-modal .nothing").removeClass("hide");
            $("#tt-h-modal .hierarchies").addClass("hide");
            LoadingScreen.init(LoadingScreen.type.smallLoadingIcon, $(".percentage-value"), $("#tt-h-modal .nothing"), $("#tt-h-modal .hierarchies"));
            LoadingScreen.start(100);
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/loadHierarchy",
                data: {}
            }).then(function success(response) {
                if (response.data.d.success) {
                    LoadingScreen.end(100);
                    $scope.hierarchies = response.data.d.data;
                    
                   
                    
                }
            }, function error(response) {
                //console.log(response);
            });
        }
    }


    //resizing
    $scope.mobile = [{ 'is_too_small': false }];
    //this is to detect resize in angular
    angular.element($window).bind("resize", "load", function () {
        $timeout(function () {
            $scope.execute_onresize();
        }, 300);
    });
    $scope.execute_onresize();
    $timeout(function () {
        $scope.execute_onresize();
    }, 1000);
    $scope.is_mobile = false;

    $scope.focus_input = function () {
        $("input").focus();
    }
    $scope.MoveToPipe = function (pipe, event) {
        if ($scope.is_anytime) {

            if (event != null) {
                var code = event.keyCode;
                if (code == 13) {
                    if (pipe > 0 && pipe <= $scope.output.total_pipe_steps) {
                        $scope.save_multis();
                        $scope.get_pipe_data(pipe, null, false);
                    }

                    $("#drop").removeClass("open f-open-dropdown");
                    return event.preventDefault();
                }
            }
            else {
                if (pipe > 0 && pipe <= $scope.output.total_pipe_steps) {
                    $scope.save_multis();
                    $scope.get_pipe_data(pipe, null, false);
                }

                $("#drop").removeClass("open f-open-dropdown");
            }
            
        } 
        if ($scope.is_teamtime) {
            if (event != null) {
                var code = event.keyCode;
                if (code == 13) {
                    if (pipe > 0 && pipe <= $scope.output.totalPipe)
                        $scope.$broadcast("MovePipeStep", pipe);
                    //$window.location.href = '/pages/teamtimetest/teamtime.aspx?pipe=' + pipe + '&steps=' + false;
                    $("#drop").removeClass("open f-open-dropdown");
                    return event.preventDefault();
                }
                
            }
            else {
                if (pipe > 0 && pipe <= $scope.output.totalPipe)
                    $scope.$broadcast("MovePipeStep", pipe);

                //$window.location.href = '/pages/teamtimetest/teamtime.aspx?pipe=' + pipe + '&steps=' + false;
                $("#drop").removeClass("open f-open-dropdown");
            }
            
        }

        




    }

    $scope.getCorrectStep = function (index) {
        if ($scope.enabledLastStep > 0)
            return $scope.enabledLastStep < index ? $scope.enabledLastStep : index;

        return index;
    }
    $scope.getNodeColor = function (hierarchy) {
        return hierarchy == $scope.output.parentnodeID ? "#b45f06" : "";
    }
    $scope.get_pipe_data = function (step, is_AT_owner, is_prev) {
        angular.element(".next_step").removeClass("back-pulse");

        $scope.is_AT_owner_temp = is_AT_owner ? true : false;
        $scope.pipe_data = [step, is_AT_owner, is_prev];
        $scope.$broadcast("get_pipe_data");
    }

    //$scope.$on("ispipescreen", function (e, data) {
    //    $scope.is_pipescreen = data;
    //});
    $scope.is_anytime = false;
    $scope.is_teamtime = false;

    $scope.qh_text = "";
    $timeout(function () {
        $scope.$apply(function() {
            $scope.is_pipescreen = $scope.is_anytime || $scope.is_teamtime ? true : false;
        });
    }, 100);
    
    $scope.xWidth = [];



    $scope.SASensitivity = null;
    $scope.setSANotmalization = function (normalization) {
        $scope.normalization_SA = normalization;
        initPage();
    }
    $scope.normalization_SA_list = [{ "value": "normAll", "text": "Normalized" }, { "value": "unnormalized", "text": "Unnormalized" }];
    $scope.normalization_SA = $scope.normalization_SA_list[0];

    $scope.autoFitInfoDocImagesOptionText = "Auto-fit information document images";
    $scope.autoFitInfoDocImages = false;
    $scope.setAutoFitInfoDocImages = function () {
        $scope.$broadcast("setAutoFitInfoDocImages");
    }

    $scope.framed_info_docs = false;
    $scope.show_framed_info_docs = function () {
        $scope.$broadcast("show_framed_info_docs");
    }

    $scope.is_infodoc_tooltip = false;
    $scope.set_infodoc_mode = function () {
        $scope.$broadcast("set_infodoc_mode");
    }

    $scope.unassessed_data = [0, 1];
    $scope.next_unassessed = function () {
        $scope.save_multis();
        $scope.$broadcast("next_unassessed");
    }

    $scope.save_multis = function () {
        $scope.closeWarningMessage();
        $scope.$broadcast("save_multis");
    }

    $scope.set_auto_advance = function () {
        $scope.$broadcast("set_auto_advance");
    }

    $scope.set_view_status = function () {
        $scope.$broadcast("set_view_status");
    }

    $scope.orig_is_AT_owner = false;
    $scope.is_AT_owner_temp = false;
    $scope.pm_switch = false;
    $scope.switch_pm = function (pm_switch) {
        $scope.pm_switch = pm_switch;
        $scope.is_AT_owner = !pm_switch ? 1 : 0;
        if ($scope.is_AT_owner == 0) {
            $(".edit-info-doc-btn").addClass("hide");
        }
        else {
            $(".edit-info-doc-btn").removeClass("hide");
        }

        $scope.set_view_status();
    }


    $scope.set_vertical_bars = function () {
        $scope.$broadcast("set_vertical_bars");
    }
    $scope.ellipsis = false;
    $scope.toggleEllips = function () {
        if (!$scope.ellipsis) {
            $timeout(function () {
                $(".toggle-ellipsis").removeClass("hide");
                $scope.ellipsis = true;
            }, 100);
        }
        else {
            $timeout(function () {
                $(".toggle-ellipsis").addClass("hide");
                $scope.ellipsis = false;
            }, 100);
        }

    }

    //$scope.comment_updated = function () {
    //    $scope.$broadcast("comment_updated");
    //}

    //$scope.update_users_list = function () {
    //    $scope.$broadcast("update_users_list");
    //}

    $scope.show_qh_automatically = function () {
        $scope.$broadcast("show_qh_automatically");
    }

    $scope.switch_graphical = function (isSingle) {
        $scope.$broadcast("switch_graphical", isSingle);
    }

    $scope.getBaseUrl = function() {
        return window.location.origin ? window.location.origin + "/" : window.location.protocol + "/" + window.location.host + "/";
    }

    $scope.finish_anytime = function (LoginMethod) {
        //case 12155 - login method to track if user signs in to login page or via hash link
        $scope.save_multis();
        show_loading_modal();
        // case 15140 - to continuously save_survey
        if ($scope.output.page_type == "atSpyronSurvey" || $scope.output.page_type == "") {
            //$scope.$broadcast("save_survey");
            $scope.saveSurvey(true);
        }
        window.onbeforeunload = null;
        if (typeof (LoginMethod) == "undefined") {
            if ($scope.output.doneOptions.redirect && $scope.output.doneOptions.url != "") {
                $scope.redirect_to_page($scope.output.doneOptions.url);
            }
            else if ($scope.output.doneOptions.openProject && $scope.output.nextProject != -1) {
                $window.open("/pages/Anytime/Anytime.aspx", "_self", "");
            }
            else {
                $scope.save_multis();
                if (confirm("You are done. Click OK to close this tab/window.")) {
                    $window.top.close();
                } else {
                    hide_loading_modal();
                    return false;
                }
            }
        }
        else {
            if ($scope.output.doneOptions.logout) {
                if (confirm("Thank you! Click OK to logout.")) {

                } else {
                    hide_loading_modal();
                    return false;
                }
                
                $http.post(
                    $scope.baseurl + "Default.aspx/logout",
                    {}
                    )
                .then(function success(response) {

                    if ($scope.output.doneOptions.redirect) {
                        if ($scope.output.doneOptions.url) {
                            $scope.redirect_to_page($scope.output.doneOptions.url);
                        }
                        else {
                            $scope.redirect_to_page($scope.getBaseUrl());
                        }
                    }
                    else if ($scope.output.doneOptions.closeTab) {
                        if ($scope.is_explorer)
                            $window.open(location.href, "_self").close();
                        else {
                            $window.open($scope.getBaseUrl() + "Pages/you-are-done.aspx", "_self");
                        }
                    }
                    else {
                        if (LoginMethod == 1 && !$scope.output.doneOptions.redirect) {
                            $scope.redirect_to_page($scope.getBaseUrl() + "evaluationDone");
                            return true;
                        } else {
                            $scope.redirect_to_page($scope.getBaseUrl());
                        }
                    }
                    //hide_loading_modal();
                });
            }
            else {

                if ($scope.output.doneOptions.redirect) {
                    if ($scope.output.doneOptions.url != "")
                        $scope.redirect_to_page($scope.output.doneOptions.url);
                    else
                        $window.open("/pages/Anytime/Anytime.aspx", "_self", "");
                }
                else if ($scope.output.doneOptions.closeTab) {
                    $window.open($scope.getBaseUrl() + "Pages/you-are-done.aspx", "_self");
                }
                else if ($scope.output.doneOptions.openProject) {
                    $window.open("/pages/Anytime/Anytime.aspx", "_self", "");
                }
                else if ($scope.output.doneOptions.stayAtEval) {
                    $scope.save_multis();
                    $window.open("/pages/Anytime/Anytime.aspx", "_self", "");
                }
                else {
                    $scope.redirect_to_page($scope.getBaseUrl());
                }
            }
        }
    }

    $scope.saveSurvey = function(isFinishStep) {
        $scope.$broadcast("save_survey", isFinishStep);
    }

    $scope.redirect_to_page = function (url){
        $scope.block_unload_prompt = true;
        if (url.indexOf("http") == -1 && url != $scope.getBaseUrl()) {
            url = "http://" + url;
        }
        window.location.href = url;
    }


    //case 13198: not needed since we will show warning messages on pages that are slow to use chrome and safari

    //if (!$scope.isMobile()) {
       
    //    var path = $location.absUrl();
    //    //console.log("original path:" + path);
    //    path = path.replace("//r.", "//");
    //    //console.log("Redirect path:" + path);
    //    //console.log($location.absUrl() != path);
    //    //console.log(!/[?&]redirect=no/.test(path) && /[?&]hash=/.test(path));
    //    if ($location.absUrl() != path) {
    //        if (! /[?&]redirect=no/.test(path) && /[?&]hash=/.test(path)) {
    //            $scope.redirect_to_page(path);
    //        }
    //    }
       
        
    //}


    $scope.show_check_box = false;
    $scope.is_under_review = false;
    $scope.move_to_cluster = function () {
        $scope.show_check_box = false;
        $scope.save_multis();
        $scope.$broadcast("move_to_cluster");
    }

    $scope.scrolltoelement = function (step, animate) {
        var multiplier = $scope.detectIE() ? 6 : 2;
        var timeOutMs = 500 + ($scope.steps_list.length * multiplier);
        timeOutMs = $scope.is_teamtime ? 700 : timeOutMs;

        $timeout(function () {
            $scope.callStepSelectionCounter = 0;
            $scope.callStepSelectionRepeater(step);
        }, timeOutMs);
    }

    $scope.selectCurrentStepFromStepList = function (step) {
        var stepElement = $("#step_" + step + "_modal");

        if (stepElement.length == 0 || stepElement.height() == 0) {
            $scope.callStepSelectionRepeater(step);
        } else {
            var decreaseScroll = $scope.isMobile() ? stepElement.height() : (stepElement.height() * 3);
            $(".tt-modal-content").animate({
                scrollTop: stepElement[0].offsetTop - decreaseScroll
            }, 250);
        }
    }

    $scope.callStepSelectionCounter = 0;
    $scope.callStepSelectionRepeater = function (step) {
        if ($scope.callStepSelectionCounter < 15) {
            $scope.callStepSelectionCounter++;
            $timeout(function () {
                $scope.selectCurrentStepFromStepList(step);
            }, 500, false);
        }
    }

    $scope.scrolltoTTelement = function (element, animate) {
        $timeout(function () {
            try{
                $("body").animate({
                    scrollTop: $("." + element).offset().top
                }, animate);
            }
            catch (e) {

            }
        }, 300);

    }

    $scope.is_AT_owner = 0;

    $scope.graphical_switch = true;
    //$scope.switch_graphical = function (isSingle) {
    //    $scope.graphical_switch = isSingle;
    //}

    $scope.MoveStepat = function (e, step, isDisabled, isATowner, is_prev, is_saving_infodoc) {
        if (step == null || step < 1) {
            return false;
        }
        if (isDisabled)
            e.preventDefault();
        else {
            $timeout(function () {
                $(".tg-mobileNav").slideUp();
                //if (typeof (is_saving_infodoc) == "undefined") {
                    $scope.save_multis();
                //}
               
            }, 0);
            $timeout(function () {
                var anytimeScope = angular.element($("#anytime-page")).scope();
                if (angular.isUndefined(anytimeScope) || !anytimeScope.isAutoSaving) {
                    //$interpolate(stringResources.loadingModalMessage.moveStep)(output);
                    var message = [];
                    if (typeof (is_saving_infodoc) == "undefined") {
                        var message = [];
                        $scope.get_pipe_data(step, isATowner, message);
                    }
                    else {
                        //if (is_saving_infodoc == "question") { // question in pipe is being edited
                        $scope.get_pipe_data(step, isATowner, false);
                        //}
                    }
                }
                
                // or
              
                //$(document).foundation("reflow");
                //console.log("reflow from timeout");
            }, 300);
        }
        //$(document).foundation("reflow");
        $scope.$broadcast("MoveStepat");
        //console.log("reflow onload");
    }

    $scope.showOptions = function () {
        var optionDiv = $(".tg-aa-wrap.tg-aa");
        var childCount = 0;

        if (optionDiv.length > 0) {
            optionDiv.each(function() {
                childCount += this.childElementCount;
            });
        }

        return (childCount > 0);
    }

    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ ABOVE are from ANYTIMECONTROLLER FOOTER^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    //************************************* Below is for TEAMTIMECONTROLLER FOOTER **************************************
    $scope.MovePipeStep = function (step) {
        $scope.$broadcast("MovePipeStep", step);
    }
    $scope.view_evaluation = function () {
        $scope.$broadcast("view_evaluation");
    }
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ ABOVE are from TEAMTIMECONTROLLER FOOTER^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    $scope.baseurl = baseUrl;
    //************************************ MY-PROJECTS ******************************************//
    
    $scope.pageOptions = [5, 10, 15, 20, 30, "ALL"];
    
    
    
    $scope.allow_my_projects = function () {
        $scope.main_workgroups($scope.ecProjectStatus.psActive);
    }

    var myObj = $.parseJSON('{"date_created":"1273185387"}'),
    myDate = new Date(1000 * myObj.date_created);

 
    $scope.return_Date = function (date) {
        var thisdate = new Date(1000 * date);
        var localdate = thisdate.getTimezoneOffset();
        thisdate.setHours(thisdate.getHours() + (localdate / 60));
        return '<div class="medium-12 small-5 columns">' + thisdate.toLocaleDateString() + '</div> <div class="medium-12 small-7 columns">' + thisdate.toLocaleTimeString() + '</div>';
    } 

    $scope.go_ToTeamtime = function (is_redirect, projectID) {
        show_loading_modal();
        if (is_redirect) {
            $http.post(
                $scope.baseurl + "Pages/my-projects.aspx/setProject",
                JSON.stringify({
                    projid: projectID
                })
                )
            .then(function success(response) {
                //$window.location.href = $scope.baseurl + 'pages/teamtimetest/teamtime.aspx';
            });
        }
        else {
            $http.post(
                $scope.baseurl + "Pages/my-projects.aspx/stopTeamTime",
                JSON.stringify({
                    projID: projectID
                })
                )
            .then(function success(response) {
                $scope.main_workgroups();
                hide_loading_modal();
            });
        }
    }


    $scope.selectedProject = { 'project_id' : 0 };
    $scope.ProjectStatus = false;
    $scope.current_project = { 'has_project': false, 'is_teamtime': false, 'project_id': 0, 'project_name': '', 'is_online': false, 'workgroup' : '' };
    $scope.get_current_project_info = function () {
        $http.post(
            $scope.baseurl + "default.aspx/getCurrentProjectInfo", ""
            )
        .then(function success(response) {
            $scope.current_project = JSON.parse(response.data.d);
            try{
                $scope.project_id = $scope.current_project.project_id;
            }
            catch (e) {

            }
           
        });
    }
    $scope.showProjectStatus = function (value, turn_online) {
        $scope.ProjectStatus = value;
        if (turn_online) {
            $timeout(function () {
                $scope.main_DisplayStatus($scope.current_project.project_id);
            }, 500);
            
        }
    }

    $scope.downloadFile = function (id) {
        var wnd = $window.open($scope.baseurl + "Pages/download.aspx?projectid=" + id + "&ext=ahps");
        show_loading_modal();
        $timeout(function () {
            wnd.close();
            hide_loading_modal();
        }, 5000);
        //$http.post(
        //    $scope.baseurl + "Pages/download.aspx/DownloadDecision",
        //    JSON.stringify({
        //        ProjectID: id,
        //        Extension: '.ahps'
        //    })
        //)
        //.then(function success(response) {
        //    $scope.main_workgroups();
        //    hide_loading_modal();
        //})
    }

    //check active project always on load
    $scope.get_current_project_info();

    $scope.setProjectbyStatus = function (status, projectID) {
        show_loading_modal();
        $http.post(
            $scope.baseurl + "Pages/my-projects.aspx/setProjectbyStatus",
            JSON.stringify({
                status: status,
                ProjectID: projectID
            })
        )
        .then(function success(response) {
            $scope.main_workgroups();
            hide_loading_modal();
        });
    }

    $scope.mobile_sort_selected = "";

    $scope.main_workgroups = function (project_status) {
        LoadingScreen.init(LoadingScreen.type.loadingModal, $("#LoadingModalPercentage"), $(".fullwidth-loading-wrap"));
        LoadingScreen.start(90);
        if (project_status == null) {
            project_status = -1;
        }
        else {
            
        }
        $http.post($scope.baseurl + 'default.aspx/getWorkgroupsandProjects',
            JSON.stringify({
                ecProjectStatus: project_status
            })
            )
            .then(function success(response) {
                var output = JSON.parse(response.data.d);
               // //console.log(JSON.stringify(output));
                $scope.workgroups = output.workgroups;
                $scope.workgroup = output.active_workgroup_id;
                $scope.wkgRoleGroupId = output.role_workgroup_id;
                $scope.combinedGroupID = output.combined_group_id;
                $scope.listofallProjects = output.projects;
                $scope.projects = [];
                $scope.current_project.project_id = output.active_project_id;
                $scope.project_users = output.users;
                $scope.current_user = output.currentUserID;
                $scope.isPM = output.isPM;
                $scope.pageSize = output.pageSize;
                $scope.columnname = "this[" + output.sort[0] + "]";
                $scope.ProjectsStatus = output.ProjectStatus;

                $scope.incrementProjects();
                $scope.get_sort_name($scope.columnname);
                $scope.reverse = output.sort[1].toLowerCase() == "true";

                $scope.output.project_status = output.project_status;
                $scope.output.project_access = output.project_access;
                $scope.output.last_access = output.last_access;
                $scope.output.last_modified = output.last_modified;
                $scope.output.date_created = output.date_created;
                $scope.output.overal_judgment_process = output.overal_judgment_process;
                
                var size = $scope.pageSize;
                if (output.pageSize == "ALL") {
                    size = $scope.projects.length;
                    $scope.incrementProjects($scope.listofallProjects.length);
                }
                if ($scope.pageSize != "ALL") {
                    $scope.pageSize = parseInt(output.pageSize);
                    $scope.incrementProjects();
                };
                $scope.numberOfPages = (output.pageSize != "ALL") ? Math.ceil($scope.projects.length / size) : 1;
                
                var screen_height = window.outerHeight;
                var screen_width = window.outerWidth;
                if (screen_height < 900 && screen_height > 700 && screen_width > 1040) {
                    $scope.main_is_screen_reduced = true;
                }
                else {
                    $scope.main_is_screen_reduced = false;
                }

                //console.log(output.totalProjects);
                //console.log("PAssed Projects: " + output.debugprojects)
                //console.log($scope.workgroup);
                //console.log($scope.workgroups);
                //console.log(JSON.stringify($scope.projects));
                //console.log($scope.current_project);
                
                LoadingScreen.end(100);
                
                
            },
            function error() {
                hide_loading_modal();
            });

    }
    $scope.normalColumns = [["name", "Project Name"], ["online", "Online"], ["status", "Status"], ["last", "Last Access"], ["modify", "Last Modified"], ["create", "Date Created"]];
    $scope.evaluatorColumns = [["name", "Project Name"], ["online", "Online"], ["status", "Status"], ["last", "Last Access"]];
    $scope.columns = $scope.normalColumns;

    $scope.changecolumns = function () {
        $timeout(function () {
            if ($scope.isPM) {
                $scope.columns = $scope.normalColumns;
            }
            else {
                $scope.columns = $scope.evaluatorColumns;
            }
        }, 1500);
    }

    $scope.setProjWidth = function (last) {
        $timeout(function () {
            getProjWidth(0);
        }, 500);
    }

    $scope.get_sort_name = function (request) {
        switch (request) {
            case "this[8]":
                $scope.mobile_sort_selected = ["create", "Date Created"];
                break;
            case "this[7]":
                $scope.mobile_sort_selected = ["modify", "Last Modified"];
                break;
            case "this[3]":
                $scope.mobile_sort_selected = ["status", "Status"];
                break;
            case "this[4]":
                $scope.mobile_sort_selected = ["online", "Online"];
                break;
            case "this[1]":
                $scope.mobile_sort_selected = ["name", "Project Name"];
                break;
            case "this[10]":
                $scope.mobile_sort_selected = ["last", "Last Access"];
                break;
        }
    }

    $scope.sort_Project = function (request) {
        switch (request) {
            case "create":
                $scope.reverse = !$scope.reverse; $scope.columnname = 8;
                break;
            case "modify":
                $scope.reverse = !$scope.reverse; $scope.columnname = 7;
                break;
            case "status":
                $scope.reverse = !$scope.reverse; $scope.columnname = 3;
                break;
            case "online":
                $scope.reverse = !$scope.reverse; $scope.columnname = 4;
                break;
            case "name":
                $scope.reverse = !$scope.reverse; $scope.columnname = 1;
                break;
            case "last":
                $scope.reverse = !$scope.reverse; $scope.columnname = 10;
                break;
        }
        $http.post(
            $scope.baseurl + "Pages/my-projects.aspx/save_sort",
            JSON.stringify({
                column: $scope.columnname,
                reverse: $scope.reverse
            })
            )
        .then(function success(response) {
            $scope.main_workgroups();
        });

    }


    $scope.currentPage = 0;
    $scope.set_Page = function (is_left) {
        if (is_left)
            $scope.currentPage -= 1;
        else
            $scope.currentPage += 1;
        if ($scope.currentPage > $scope.numberOfPages - 1)
            $scope.currentPage = $scope.numberOfPages - 1;
        if ($scope.currentPage < 0)
            $scope.currentPage = 0;

    }
    $scope.change_pageSize = function (pageSize) {
        $scope.currentPage = 0;
        var size = pageSize;
        if (pageSize == $scope.pageOptions[5]) {
            size = $scope.projects.length;
        }
        else {
            $scope.pageSize = pageSize;
        }
        $scope.numberOfPages = Math.ceil($scope.projects.length / size);
        $http.post(
        $scope.baseurl + "Pages/my-projects.aspx/savePageSize",
        JSON.stringify({
            pageSize: pageSize
        })
        )
        .then(function success(response) {
            if (response.data.d) {
                window.location.href = baseUrl;
            }
            else {
                $scope.current_project = null;
            }
            hide_loading_modal();
        });

    }
    $scope.search_ProjectName = function (project_name_search) {
        $timeout.cancel($scope.search_project_timer);
        $scope.search_project_timer = $timeout(function () {
            if (project_name_search != "") {
                if ($scope.projects.length != $scope.listofallProjects.length) {
                    $scope.incrementProjects($scope.listofallProjects.length - 20);
                }
            }
            else {
                $scope.projects = [];
                $scope.incrementProjects();
            }

            $scope.project_name_search = project_name_search;
        }, 500);
    }

    $scope.getUsersByProject = function (projectID) {
        $scope.listofusers = null;
        $http.post(
            $scope.baseurl + "default.aspx/getUsersByProject",
            JSON.stringify({
                projectID: projectID
            })
            )
        .then(function success(response) {
            $scope.listofusers = response.data.d;
        }, function error() {

        });
    }
    $scope.output.project_status = true;
    $scope.output.project_access = true;
    $scope.rememberFilter = function (model, filtername) {
        
        $http.post(
            $scope.baseurl + "Default.aspx/RememberFilter",
            JSON.stringify({
                filter: model,
                filtername: filtername
            })
            )
        .then(function success(response) {
        }, function error(response) {
            
        });
    }
    $scope.ProcessProject = function (ProjectID, status) {
        show_loading_modal();
        $http.post(
            $scope.baseurl + "Pages/my-projects.aspx/CheckProject",
            JSON.stringify({
                ProjectID: ProjectID,
                Status: status
            })
            )
            .then(function success(response) {
                $scope.current_project.has_project = response.data.d;
                if (response.data.d) {
                    $scope.current_project.project_id = ProjectID;
                    //window.location.href = $scope.baseurl + "Pages/my-projects.aspx";
                }
                else {
                    $scope.current_project = { 'has_project': false, 'is_teamtime': false, 'project_id': 0, 'project_name': '', 'is_online': false, 'workgroup': '' };
                }
                hide_loading_modal();
            },
            function error() {
                hide_loading_modal();
            });
    }

    $scope.main_SetWorkgroup = function (ID) {
        show_loading_modal();
        $http.post(
            $scope.baseurl + "Pages/my-projects.aspx/setWorkgroup",
            JSON.stringify({
                ID: ID
            })
        )
        .then(function success(response) {
            $scope.projects = [];
            $scope.main_workgroups();
            //$scope.incrementProjects($scope.listofallProjects.length);
            hide_loading_modal();
        },
        function error() {
            hide_loading_modal();
        });
    }
    $scope.main_DisplayStatus = function (ID) {
        show_loading_modal();
        $http.post(
            $scope.baseurl + "Pages/my-projects.aspx/displayStatus",
            JSON.stringify({
                projid: ID
            })
            )
        .then(function success(response) {
            $scope.main_workgroups();
            hide_loading_modal();
        }, function error() {
            hide_loading_modal();
        });
    }

    $scope.reveal_TT_modal = function (project_id) {
        show_loading_modal();
        $(".tt-meeting-modal").hide();
        $http.post(
            $scope.baseurl + "Pages/my-projects.aspx/setProject",
            JSON.stringify({
                projid: project_id
            })
            )
        .then(function success(response) {
            hide_loading_modal();
            $scope.fill_start_modal();
        });
       
    }
    $scope.active_project = {};

    $scope.main_StartAnytime = function (project_id, is_valid, is_online, can_modify) {
        is_valid = is_valid == "True" ? true : false;
        if (is_online == "False" && can_modify == "False") return false;
        if (!is_valid) {
            //alert("This project has different version from the current database version.");
        }
        LoadingScreen.init(LoadingScreen.type.loadingModal, $("#LoadingModalPercentage"), $(".fullwidth-loading-wrap"));
        LoadingScreen.start(70);
        $http({
            method: "POST",
            url: $scope.baseurl + "default.aspx/StartAnytime",
            async: false,
            data: {
                projID: project_id
            }
        }).then(function success(response) {
            //over riding message
            var output = JSON.parse(response.data.d);
            $scope.active_project = output;

            var project_details = "Loading model details..." + "<br/>";
            try {
                project_details += "Name:" + $scope.active_project.name.substring(0, 50) + "...<br/>";
            }
            catch (e) {

            }
            //project_details += "Owned by:" + $scope.active_project.owner + "<br/>";
            //project_details += "Passcode:" + $scope.active_project.passcode + "<br/>";
            LoadingScreen.end(90, false);



            if (output.start_anytime) {
                $timeout(function () {
                    LoadingScreen.end(100, true);
                    $window.location.href = '/pages/Anytime/Anytime.aspx', 200
                }, 200);

            }
            else
                $(document).ready(function () { $("#ErrorMessage").foundation("reveal", "open"); });
        }, function error(response) {
            console.log(response);
            return false;
        });
    }

    $scope.fill_start_modal = function () {
        show_loading_modal();
        $http.post(
            $scope.baseurl + "default.aspx/FillStartMeetingModal", ''
            )
        .then(function success(response) {
            var output = JSON.parse(response.data.d);
            if (!output.isTeamTime) {
                $(".start-tt-as-pm").show();
                $(".start-tt-as-participant").hide();
            }
            else {
                if (output.isTeamTimeMeetingOwner) {
                    $(".start-tt-as-pm").show();
                    $(".start-tt-as-participant").hide();
                }
                else {
                    $(".start-tt-as-participant").show();
                    $(".start-tt-as-pm").hide();
                }
            }
            $(".start-tt-as-pm .workgroup").html('Workgroup: <b>' + output.CurrentWorkgroup + '</b>');
            $(".start-tt-as-pm .project").html('Project: <b>' + output.CurrentProject + '</b>');
            $(".tt-meeting-modal").show();
            $(document).ready(function () { $("#startMeeting").foundation("reveal", "open"); });
            hide_loading_modal();
        });
    }

    if (meeting_signal) {
        $scope.fill_start_modal();
    }

    $scope.format_project_name = function (project) {
        try {
            return project.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/([A-Z])([a-z])/g, ' $1$2').replace(/\ +/g, ' ').replace('_', ' ');
        }
        catch (e) {
            return project;
        }
    }

    $scope.copied_clipboard = function (e, element_id) {
        if ($(element_id).text().trim() == "Copy to Clipboard") {
            $(element_id).html("Copied");
        }
        else {
            $(element_id).html("Copy to Clipboard");
        }
    }

    $scope.isProjectTeamtime = false;
    $scope.general_link_switch = { 'radio': '2', 'mradio': '2' };
    $scope.generateLink = function (is_teamtime, projectID, projectName, is_online) {
        //general link if project is offline
        //$scope.general_link_switch.radio = '0';
        //$scope.general_link_switch.mradio = '0';
        $scope.general_link = "";
        $scope.ProjectStatus = is_online == "False";
        $scope.current_project.project_id = projectID;
        //console.log($scope.ProjectStatus);
        $timeout(function () {
            $scope.current_project.project_name = projectName;
        },600);
        $scope.isProjectTeamtime = is_teamtime;
        
        $timeout(function () {
            $scope.$apply(function() {
                $scope.current_project.is_teamtime = is_teamtime;

                $scope.is_general_link = true;

            });
        }, 1000);
        
        $http.post(
            $scope.baseurl + "Default.aspx/GenerateLink",
            JSON.stringify({
                is_teamtime: is_teamtime,
                projID: projectID
            })
            )
        .then(function success(response) {
            $scope.project_id = projectID;
            $scope.is_Teamtime = is_teamtime;
            $scope.hash_links = response.data.d;
            $scope.invite_message = "Please provide the following information to sign up or login and start evaluation «<b>" + projectName + "</b>»";
            if ($scope.isMobile)
                $scope.generateGeneralLink($scope.general_link_switch.mradio);
            else
                $scope.generateGeneralLink($scope.general_link_switch.radio);
        });        
    }

    $scope.show_offset = function () {
        //alert($scope.inputcode);

        //alert(" innerwidth:" + window.innerWidth + " outerwidth:" + window.outerWidth + "I am news here");
    }
    //************************************ MY-PROJECTS ******************************************//

    //************************************ GENERAL LINK ******************************************//
    $scope.general_link_title = "Sign up or Log in";
    $scope.is_signup_form = true;
    $scope.switch_form = function (is_signup) {
        if (is_signup) {
            $scope.general_link_title = "Sign up";
            $scope.is_signup_form = true;

        }
        else {
            $scope.error_message.show = "";
            $(".tt-login-form-status").hide();
            $scope.general_link_title = "Log in";
            $scope.is_signup_form = false;
            //$(".error").hide();
        }
    }
    $scope.signup = [{ 'email': 'e', 'name': 'n', 'password': 'p', 'message' : '' }];
    $scope.req_signup = { 'email': 'e', 'name': 'n', 'password': 'p' };
    $scope.signup_mode = "enp";
    $scope.req_signup_mode = "enp";
    $scope.permission_level = "1";

    $scope.set_message = function (bool) {
        $scope.signup.message = bool;
    }
    $scope.save_invite_message = function (message) {
        $scope.invite_message = message;
    }

    $scope.generateGeneralLink = function (tmode) {
        $scope.general_link_switch.mradio = tmode;
        $scope.general_link_switch.radio = tmode;
        angular.element("#general_link_copy_btn").html("Copy to Clipboard");
        if ($scope.general_link_switch.radio == tmode) {
            $scope.permission_level = angular.element("#SelectPermissionLevel").val();
        } else {
            $scope.permission_level = angular.element("#SelectMobilePermissionLevel").val();
        }
        

        $scope.signup_mode = "";
        $scope.req_signup_mode = "";
        //signup
        if ($scope.signup.email)
            $scope.signup_mode = "e";
        else
            $scope.signup_mode = $scope.signup_mode.replace("e", "");

        if ($scope.signup.name)
            $scope.signup_mode += "n";
        else
            $scope.signup_mode = $scope.signup_mode.replace("n", "");

        if ($scope.signup.password)
            $scope.signup_mode += "p";
        else
            $scope.signup_mode = $scope.signup_mode.replace("p", "");
        //req
        if ($scope.req_signup.email == true)
            $scope.req_signup_mode = "e";
        else
            $scope.req_signup_mode = $scope.req_signup_mode.replace("e", "");

        if ($scope.req_signup.name == true)
            $scope.req_signup_mode += "n";
        else
            $scope.req_signup_mode = $scope.req_signup_mode.replace("n", "");

        if ($scope.req_signup.password == true)
            $scope.req_signup_mode += "p";
        else
            $scope.req_signup_mode = $scope.req_signup_mode.replace("p", "");
        $scope.req_signup_mode = $scope.req_signup_mode;

        var groupID = -1; // for combined groups: unfinished
        var wkgid = $scope.wkgRoleGroupId;
        $http.post(
            $scope.baseurl + "Default.aspx/getGeneralLink",
            JSON.stringify({
                tmode: tmode,
                projectID: $scope.project_id,
                signupmode: $scope.signup_mode,
                otherparams: $scope.req_signup_mode,
                combinedGroupID: groupID,
                wkgRoleGroupId: parseInt($scope.permission_level)
            })
            )
        .then(function success(response) {
            $timeout(function () {
                $scope.$apply(function () {
                    $scope.general_link = response.data.d;
                });
            }, 100);
        }, function error(response) {
            //console.log(response);
        });
    }

    $scope.GeneralLinkLogin = function (email, password) {
        $scope.is_signup_form = false;
        $(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box radius tt-alert msg-stat" style="background: #0058a3">           <h3>Checking...          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        email = email == null ? "" : email;
        password = password == null ? "" : password;

        $http.post(
            $scope.baseurl + "Default.aspx/GeneralLinkLogin",
            JSON.stringify({
                email: email,
                password: password
            })
            )
        .then(function success(response) {
            var result = JSON.parse(response.data.d);
            if (result.pass) {
                //$('.tt-login-form-status').html('<div class="large-12 columns">          <div data-alert class="alert-box success radius tt-alert msg-stat">           <h3>Logged in successfully! Redirecting...          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
                $window.location.href = $scope.baseurl + "pages/Anytime/Anytime.aspx";
                $scope.mesage_layout("success", "Logged in successfully! Redirecting...");

                LoadingScreen.init(LoadingScreen.type.loadingModal, $("#LoadingModalPercentage"), $(".fullwidth-loading-wrap"));
                LoadingScreen.start(70);
                return true;
            }
            else {
                $scope.mesage_layout("alert", result.message);
                return false;
            }
        });
    }
    $scope.is_general_link = true;

    $scope.is_general_link;
    $scope.toggle_general_link = function () {
        $timeout(function () {
            $scope.$apply(function () {
                $scope.is_general_link = !$scope.is_general_link;
            });
        }, 100);
        
    }
    $scope.user_entries = 1;

    $scope.signUpUser = {
        SEmail: "",
        SName: "",
        SPhone: "",
        SPass: "",
        SPass2: "",
        IsNewUser: true
    }

    //$scope.SEmail = "";
    //$scope.IsNewUser = true;
    $scope.GeneralLinkSignUp = function (email, name, password, password2, phone, e, allow) {
        if (e == null) $scope.is_signup_form = true;
        //if enter or go key was pressed
        if (e != null)
        {
            var code = e.which;
            if (code != 13)
                return false;
        }

        $(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box radius tt-alert msg-stat" style="background: #0058a3">           <h3>Proceeding...          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
        $(".error").hide();

        var bypass = true;
        if ($scope.req_signup_mode.indexOf("e") > -1) {
            var testEmail = /^[A-Z0-9._%+-]+@([A-Z0-9-]+\.)+[A-Z]{2,4}$/i;
            if (email != null || email != "") {
                if (!testEmail.test(email)) {
                    bypass = false;
                    $("#SEmailError").show();
                }
            }
            else {
                bypass = false;
                $("#SEmailreq").show();
            }
        }
        else {
            if (email == null || email == "") {
                email = "";
            }
            else {
                var testEmail = /^[A-Z0-9._%+-]+@([A-Z0-9-]+\.)+[A-Z]{2,4}$/i;
                if (email != null) {
                    if (!testEmail.test(email)) {
                        bypass = false;
                        $("#SEmailError").show();
                    }
                }
            }

        }
        
        if ($scope.req_signup_mode.indexOf("n") > -1) {
            if (name == null || name == "") {
                bypass = false;
                $("#SNameError").show();
            }
        }
        else {
            if (name == null) {
                name = "";
            }
        }

        if ($scope.req_signup_mode.indexOf("p") > -1) {
            if (password == null || password == undefined || password == "") {
                bypass = false;
                $("#SPasswordreq").show();
            }
            
            if (password != password2) {
                bypass = false;
                $("#SPasswordError").show();
            }
        }
        else {
            //if ($scope.signUpLinkFields[2] || !$scope.signUpUser.IsNewUser) {
                if (password == null || password == undefined) {
                    password = "";
                }
                if (password2 == null || password2 == undefined) {
                    password2 = "";
                }

                if ($scope.req_signup_mode.indexOf("p") == -1) {
                    password = "";
                    password2 = "";
                }

                if (password != password2) {
                    bypass = false;
                    $("#SPasswordError").show();
                }
            //} else {
            //    password = "";
            //}
        }

        if ($scope.req_signup_mode.indexOf("t") > -1) {
            if (phone == null) {
                bypass = false;
                $("#SPhonereq").show();
            }
        }
        else {
            if (phone == null) {
                phone = "";
            }
        }

        //$(".tt-login-form-status").html("");

        if (allow) {
            bypass = true;
            email = "";
            name = "";
            password = "";
            //show_loading_modal();
        }

        if (bypass) {
            $http.post(
                $scope.baseurl + "Default.aspx/GeneralLinkSignUp",
                JSON.stringify({
                    email: email,
                    name: name,
                    password: password,
                    sPhone: phone,
                    signup_mode: $scope.signup_mode.indexOf("p") > -1
                })
                )
            .then(function success(response) {
                var result = JSON.parse(response.data.d);
                //hide_loading_modal();
                if (result.pass) {
                    //$('.tt-login-form-status').html('<div class="large-12 columns">          <div data-alert class="alert-box success radius tt-alert msg-stat">           <h3>Proceeding...          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
                    $window.location.href = $scope.baseurl + "pages/Anytime/Anytime.aspx";
                    $scope.mesage_layout("success", "Proceeding...");

                    LoadingScreen.init(LoadingScreen.type.loadingModal, $("#LoadingModalPercentage"), $(".fullwidth-loading-wrap"));
                    LoadingScreen.start(70);
                    return true;
                }
                else {
                    $scope.mesage_layout("alert", result.message);

                    var errorElement = $("#SignUpModal .tt-login-form-status");
                    if (errorElement.length > 0) {
                        errorElement[0].scrollIntoView();
                    }
                    
                    return false;
                }

            }, function error(response) {
                //hide_loading_modal();
                //$scope.mesage_layout("alert", "Something Went Wrong");
            });
        }
        else {
            //$scope.mesage_layout("alert", "Error: Please check the data you entered");
            $scope.clearPreviousLoginMessage();
        }
    }

    $scope.processLogin = function (email, name, password, password2, phone) {
        if (angular.isUndefined($scope.existingUeserTimer)) {
            if ($scope.signUpUser.IsNewUser) {
                $scope.GeneralLinkSignUp(email, name, password, password2, phone, null);
            } else {
                $scope.GeneralLinkLogin(email, password);
            }
        }
    }

    $scope.loginFormInputChanged = function (inputType) {
        $scope.clearPreviousLoginMessage();

        if (inputType == "name") {
            $("#SNameError").hide();
        } else if (inputType == "pass") {
            $("#SPasswordreq").hide();
            $("#SPasswordError").hide();
        } else if (inputType == "repass") {
            $("#SPasswordError2").hide();
        }
    }

    $scope.shouldCursorMoveToNextField = false;

    $scope.checkEnteredKeyForEmail = function (event) {
        var keyCode = event ? event.keyCode : 0;
        if (keyCode == 9 || keyCode == 13) {
            $scope.shouldCursorMoveToNextField = true;
        }
    }

    $scope.checkExistingUser = function () {
        $scope.cancelExistingUserTimer();

        var timeOutMs = $scope.isValidEmail($scope.signUpUser.SEmail) ? 500 : 2000;

        $scope.existingUeserTimer = $timeout(function () {
            if (angular.isDefined($scope.signUpUser.SEmail) && $scope.signUpUser.SEmail != null && $scope.signUpUser.SEmail.trim().length > 0) {
                $("#SEmailChecking").removeClass("hide");
                $("#SEmail").val($scope.signUpUser.SEmail.trim());
                $scope.cancelExistingUserTimer();
                //$scope.clearPreviousLoginMessage();

                $("#SEmailError").hide();
                $("#SEmailreq").hide();

                $timeout(function () {
                    $http.post(
                        $scope.baseurl + "Default.aspx/IsExistingUser",
                        JSON.stringify({
                            email: $scope.signUpUser.SEmail.trim()
                        })
                    ).then(function success(response) {
                        $scope.signUpUser.IsNewUser = (response.data.d == "false");
                        $("#SEmailChecking").addClass("hide");
                        $scope.clearPreviousLoginMessage();
                        $scope.moveToNextFieldOnSignUp();
                    }, function error(response) {
                        $("#SEmailChecking").addClass("hide");
                        var errorMessage = response.status + (response.data || "Request failed");
                        $scope.mesage_layout("alert", errorMessage);
                    });
                }, 100);
            }
        }, timeOutMs);
    }

    $scope.cancelExistingUserTimer = function() {
        if (angular.isDefined($scope.existingUeserTimer)) {
            $timeout.cancel($scope.existingUeserTimer);
            $scope.existingUeserTimer = undefined;
        }
    }

    $scope.clearPreviousLoginMessage = function() {
        $(".tt-login-form-status").html("");
    }

    $scope.moveToNextFieldOnSignUp = function () {
        if ($scope.shouldCursorMoveToNextField) {
            $timeout(function () {
                if ($scope.signUpUser.IsNewUser && $scope.signUpLinkFields[1]) {
                    if (angular.isUndefined($scope.existingUeserTimer)) {
                        $("#SName").focus();
                    }
                } else {
                    $("#SPass").focus();
                }
            }, 50);

            $scope.shouldCursorMoveToNextField = false;
        }
    }

    $scope.getLastHash = function() {
        $http.post(
            $scope.baseurl + "Default.aspx/GetLastHash",
            JSON.stringify({
            })
        ).then(function success(response) {
            var lastHash = response.data.d;
            if (lastHash.length > 0) {
                $("#hashLinkMessage").html("<span>Click <a href='/?hash=" + lastHash + "'>here</a> to return to your evaluation.</span>");
            } else {
                $("#hashLinkMessage").html("<span>If you have a Comparion evaluation link, please use it to return to your evaluation.</span>");
            }
        }, function error(response) {
        });
    }

    $scope.isPasswordResetFromSignUp = false;
    $scope.showPasswordResetModal = function(formName) {
        if (formName == "login" || formName == "hashLogin") {
            $("#PasswordResetModal").foundation("reveal", "open");
            $scope.isPasswordResetFromSignUp = formName == "hashLogin";
        }

        return false;
    }

    $(document).on("close.fndtn.reveal", "#PasswordResetModal[data-reveal]", function () {
        $scope.showSignInSignUpModal();
    });

    $(document).on("closed.fndtn.reveal", "#PasswordResetModal[data-reveal]", function () {
        $scope.showSignInSignUpModalBackground();
    });

    $(document).on("open.fndtn.reveal", "#SignUpModal[data-reveal]", function () {
        $scope.showSignInSignUpModalBackground();
    });

    $scope.showSignInSignUpModalBackground = function() {
        if ($scope.isPasswordResetFromSignUp) {
            $(".reveal-modal-bg").css("display", "block");
            //$(".reveal-modal-bg").css("background", "#ecf0f1");
            $scope.isPasswordResetEmailSent = false;
        }
    }

    $scope.showSignInSignUpModal = function () {
        if ($scope.isPasswordResetFromSignUp) {
            $("#SignUpModal").foundation("reveal", "open");
        }
    }

    $scope.ResetEmail = "";
    $scope.isPasswordResetEmailSent = false;
    $scope.resetPassword = function (email) {
        $("#ResetEmailError").hide();
        $("#ResetEmailInfo").hide();

        if ($scope.isPasswordResetEmailSent) {
            $scope.isPasswordResetEmailSent = false;
            $("#PasswordResetModal").foundation("reveal", "close");

            $("#ResetSendSuccess").hide();
            $("#ResetSendSuccess").html("");
            $("#ResetEmailDiv").show();
        } else {
            if (angular.isDefined(email) && email != null && email.trim().length > 0) {
                if ($scope.isValidEmail(email)) {
                    $("#ResetEmailInfo").html("Processing...");
                    $("#ResetEmailInfo").show();

                    $http.post(
                        $scope.baseurl + "Default.aspx/ResetPassword",
                        JSON.stringify({
                            email: email
                        })
                    ).then(function success(response) {
                            $("#ResetEmailInfo").hide();
                            var result = JSON.parse(response.data.d);
                            $scope.isPasswordResetEmailSent = result.isSuccess;

                            if (result.isSuccess) {
                                $("#ResetEmailDiv").hide();
                                $("#ResetSendSuccess").html(result.message);
                                $("#ResetSendSuccess").show();
                            } else {
                                $("#ResetEmailDiv").show();
                                $("#ResetEmailError").html(result.message);
                                $("#ResetEmailError").show();
                            }
                        },
                        function error(response) {

                        });
                } else {
                    $("#ResetEmailError").html("Invalid email address!");
                    $("#ResetEmailError").show();
                }
            } else {
                $("#ResetEmailError").html("Email is required");
                $("#ResetEmailError").show();
            }
        }
    }

    $scope.isValidEmail = function (email) {
        if (angular.isUndefined(email) || email == null || email.trim().length == 0) return false;

        var testEmail = /^[A-Z0-9._%+-]+@([A-Z0-9-]+\.)+[A-Z]{2,4}$/i;
        return testEmail.test(email);
    }

    $scope.isValidPhoneNumber = function (phone) {
        if (angular.isUndefined(phone) || phone == null || phone.trim().length < 10) return false;

        var phoneRegEx = /^(\+\d{1,3}\s?)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$/;
        return phoneRegEx.test(phone);
    }

    $scope.PasswordFieldText = "";
    $scope.PasswordAgainFieldText = "";
    $scope.PasswordEqualMessage = "";
    $scope.checkPasswordResetUrl = function () {
        $http.post(
            $scope.baseurl + "Password.aspx/CheckPasswordResetUrl",
            JSON.stringify({})
        ).then(function success(response) {
            var result = JSON.parse(response.data.d);
            if (result.redirectUrl.length > 0) {
                window.location = baseUrl + result.redirectUrl;
            } else {
                if (result.hasError) {
                    $("#divPasswordInput").addClass("hide");
                } else {
                    $("#divPasswordInput").removeClass("hide");
                }

                $scope.PasswordFieldText = result.passwordFieldText;
                $scope.PasswordAgainFieldText = result.passwordAgainFieldText;
                $scope.PasswordEqualMessage = result.passwordEqualMessage;

                $("#divResetSuccessMessage").addClass("hide");
                $("#divMessage").html("");
                if (result.message.length > 0) {
                    $("#divMessage").html(result.message);
                    $("#divResetSuccessMessage").removeClass("hide");
                }
            }
        }, function error(response) {

        });
    }

    $scope.PasswordNew = "";
    $scope.PasswordAgain = "";
    $scope.checkPasswordStrength = function (passNew, passAgain) {
        var passNew = angular.isUndefined(passNew) ? "" : passNew;
        var passAgain = angular.isUndefined(passAgain) ? "" : passAgain;
        //var passNew = $scope.PasswordNew;
        //var passAgain = $scope.PasswordAgain;

        if (passNew.length === 0) {
            $("#divPasswordCheckMessage").html("");
            return;
        }

        if (passAgain.length > 0 && passNew !== passAgain) {
            $("#divPasswordCheckMessage").html($scope.PasswordEqualMessage);
            return;
        }

        var matchedCase = new Array();
        matchedCase.push("[^A-Za-z0-9]"); // Special characters
        matchedCase.push("[A-Z]");      // Uppercase
        matchedCase.push("[0-9]");      // Numbers
        matchedCase.push("[a-z]");     // Lowercase

        var strengthCounter = passNew.length > 7 ? 1 : 0;
        for (var i = 0; i < matchedCase.length; i++) {
            if (new RegExp(matchedCase[i]).test(passNew)) {
                strengthCounter++;
            }
        }
        
        var strength = "";
        switch (strengthCounter) {
            case 0:
            case 1:
                strength = "Very Weak";
                break;
            case 2:
                strength = "Weak";
                break;
            case 3:
                strength = "Average";
                break;
            case 4:
                strength = "Strong";
                break;
            case 5:
                strength = "Very Strong";
                break;
        }

        $("#divPasswordCheckMessage").html("Password strength: " + strength);
    }

    $scope.saveNewPassword = function (password, passwordAgain) {
        password = angular.isUndefined(password) ? "" : password;
        passwordAgain = angular.isUndefined(passwordAgain) ? "" : passwordAgain;

        var isOk = (password === passwordAgain);
        $("#divPasswordCheckMessage").html("");

        if (isOk) {
            $http.post(
                $scope.baseurl + "Password.aspx/SaveNewPassword",
                JSON.stringify({
                    password: password,
                    reTypePassword: passwordAgain
                })
            ).then(function success(response) {
                var result = JSON.parse(response.data.d);

                if (result.redirectUrl.length > 0) {
                    window.location = baseUrl + result.redirectUrl;
                } else if(result.message.length > 0) {
                    if (result.isSuccess) {
                        $("#divPasswordInput").addClass("hide");
                        $("#divResetSuccessMessage").removeClass("hide");
                        $("#divMessage").html(result.message);
                    } else {
                        $("#divPasswordErrorMessage").html(result.message);
                    }
                }
            }, function error(response) {

            });
        } else {
            $("#divPasswordCheckMessage").html($scope.PasswordEqualMessage);
        }
    }

    //VOLT - no need to use this function as we have a function for this already. Look for $scope.GeneralLinkSignUp. also this code is wrong as it redirects to anytime immediately without signing up.
    //$scope.CheckGeneralLink = function () {

    //    $(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box radius tt-alert msg-stat">           <h3>Checking...          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
    //    $(".error").hide();
        
    //    $(".tt-login-form-status").html("");

    //    $http.post(
    //        $scope.baseurl + "Default.aspx/GeneralLinkSignUp",
    //        JSON.stringify({
    //            email: "",
    //            name: "",
    //            password: "",
    //            signup_mode: false
    //        })
    //        ).then(function success(response) {
    //        var result = JSON.parse(response.data.d);
            
    //        if (!result.pass) {
    //            $scope.mesage_layout("alert", result.message);
    //        }
    //    }, function error(response) {
    //        //$scope.mesage_layout("alert", "Something Went Wrong");
    //    });
    //}

    $scope.DeviceChecker = {
        Android: function() {
            return navigator.userAgent.match(/Android/i);
        },
        BlackBerry: function() {
            return navigator.userAgent.match(/BlackBerry/i);
        },
        iOS: function() {
            return navigator.userAgent.match(/iPhone|iPad|iPod/i);
        },
        Opera: function() {
            return navigator.userAgent.match(/Opera Mini/i);
        },
        Windows: function() {
            return navigator.userAgent.match(/IEMobile/i) || navigator.userAgent.match(/WPDesktop/i);
        },
        isMajorBrowser: function () {
            
            return (navigator.userAgent.match(/IEMobile/i) || navigator.userAgent.match(/Opera Mini/i) || navigator.userAgent.match(/Chrome/i) || navigator.userAgent.match(/Safari/i) || navigator.userAgent.match(/Firefox/i) || $scope.detectIE());
        },
        any: function() {
            return (DeviceChecker.Android() || DeviceChecker.BlackBerry() || DeviceChecker.iOS() || DeviceChecker.Opera() || DeviceChecker.Windows());
        }
    };

    $scope.detectIE = function() {
        var ua = window.navigator.userAgent;

        var msie = ua.indexOf("MSIE ");
        if (msie > 0) {
            // IE 10 or older => return version number
            return (parseInt(ua.substring(msie + 5, ua.indexOf(".", msie)), 10) <= 10);
        }

        var trident = ua.indexOf("Trident/");
        if (trident > 0) {
            // IE 11 => return version number
            var rv = ua.indexOf("rv:");
            return (parseInt(ua.substring(rv + 3, ua.indexOf(".", rv)), 10) >= 11);
        }

        var edge = ua.indexOf("Edge/");
        if (edge > 0) {
            // Edge (IE 12+) => return version number
            return (parseInt(ua.substring(edge + 5, ua.indexOf(".", edge)), 10) >= 12);
        }

        // other browser
        return false;
    }

    $scope.getIEVersion = function () {
        var ua = window.navigator.userAgent;

        var msie = ua.indexOf("MSIE ");
        if (msie > 0) {
            // IE 10 or older => return version number
            return parseInt(ua.substring(msie + 5, ua.indexOf(".", msie)), 10);
        }

        var trident = ua.indexOf("Trident/");
        if (trident > 0) {
            // IE 11 => return version number
            var rv = ua.indexOf("rv:");
            return parseInt(ua.substring(rv + 3, ua.indexOf(".", rv)), 10);
        }

        var edge = ua.indexOf("Edge/");
        if (edge > 0) {
            // Edge (IE 12+) => return version number
            return parseInt(ua.substring(edge + 5, ua.indexOf(".", edge)), 10);
        }

        // other browser
        return 12;
    }

    //alert(navigator.userAgent);
   // alert($scope.DeviceChecker.isMajorBrowser());
   //console.log(navigator.userAgent);

    //$timeout(function () {
    //    alert($(".prev_step").height());
    //}, 3000);
    //************************************ END OF GENERAL LINK ******************************************//

    //Errors -> please add here the type of errors for gecko if not yet included
    //$scope.pageError = [{
    //    'help': ' The access code for specified project is disabled. Please contact the project manager to get the access.',
    //    '': ''
    //}];
    $scope.error_message = {
        "inviteUserExist": 'Sign up failed. User already exist.',
        "inviteNoAccess": 'The access code for specified project is disabled. Please contact the project manager to get the access.',
        "invalidLink": 'Sorry but we don\'t recognized your evaluation link. Please check if you have the correct link or contact your Project Manager',
        'show' : ''
    };
    $scope.mesage_layout = function (type, message) {
        return $(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box '+ type +' radius tt-alert msg-stat">           <h3> ' + message + '<a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
    }
    //alert($scope.error_message.inviteUserExist);
    //var ss= '101';
    //alert(JSON.stringify($scope.pageError['help']));

    //Scopes for AnytimeController
    $scope.setFocusonElement = function (element) {
        $timeout(function () {
            $(element).focus();
        }, 50);
        $(element).val("");
    }

    $scope.resultsreverse = true;
    $scope.is_result_screen = false;
    $scope.msg = "";
    $scope.anytimecolumns = [["nodeID", "Index"], ["nodeName", "Name"], ["yourResults", "Your Results"], ["combine", "Combined"]];
    $scope.normalization_change = function (normalization) {
        $scope.normalization = normalization;
        $scope.$broadcast("someEvent");
    }
    $scope.sort_Results = function (request) {
        $scope.columnsort = request;
        $scope.$broadcast("sortcolumns");
    }
    $scope.set_reverse = function () {
        $scope.resultsreverse = !$scope.resultsreverse;
        $scope.$broadcast("reverse");
    }
    $scope.normalization_list = [{ "value": 1, "text": "Normalized" }, { "value": 2, "text": "% of Maximum" }, { "value": 3, "text": "Unnormalized" }];
    $scope.normalization = $scope.normalization_list[0];
    $scope.wrt_id = 1;

    $scope.is_html_empty = function (html) {
        if (angular.isDefined(html) && angular.isString(html)) {
            if (/<[a-z\][\s\S]*>/i.test(html)) {
                if (html.indexOf("img") >= 0) {
                    return false;
                }
                if ($(html).text().trim() == "") {
                    return true;
                } else {
                    return false;
                }
            } else {
                if (html.trim() != "") {
                    return false;
                } else {
                    return true;
                }
            }
        } else {
            return true;
        }
    }

    //$scope.showMagnifyIcon = function (html) {
    //    var minLength = 800;

    //    if (html.length > minLength) {
    //        var charCount = $scope.getHtmlInnerTextCount(html);
    //        return (charCount > minLength);
    //    } else {
    //        return false;
    //    }
    //}

    //$scope.getHtmlInnerTextCount = function (html) {
    //    var charCount = 0;

    //    $(html).children().each(function () {
    //        if ($(this).children().lenght > 0) {
    //            charCount += $scope.getHtmlInnerTextCount(this);
    //        } else {
    //            charCount += this.innerText.length;
    //        }
    //    });

    //    return charCount;
    //}
    //Scopes for AnytimeController


    //Scopes for TeamTimeController
    $scope.main_user_list_check = false;
    $scope.remove_user_list = function () {
        show_loading_icon();
        $scope.main_user_list_check = true;
    }
    $scope.step_lower_bound = [];
    $scope.main_teamtime_screen = "";

    $scope.TTpause = "pause";
    $scope.setTTEvent = function (event) {
        if (event == "stop") {
            var r = confirm("Are you sure you want to end the meeting?");
            if (r == true) {
               
            } else {
                return false;
            }
        }
        show_loading_modal();
        $http({
            method: "POST",
            url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/TeamTimeExecuteEvent",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                TeamTimeEvent: event
            }),
        }).then(function success(data) {
            window.onbeforeunload = null;
            if (event == "stop") {
                window.location = baseUrl + "Pages/my-projects.aspx";
            }
            if (event == "pause") {
                $timeout($(".disable-when-pause").addClass("disabledbutton"), 100);

                $scope.TTpause = "resume";
                   
            }
            else {
                $(".disable-when-pause").removeClass("disabledbutton");
                $scope.TTpause = "pause";
                //window.location = baseUrl + "pages/teamtimetest/teamtime.aspx";
                //$scope.update_users_list();
            }
            hide_loading_modal();
        }, function error(response) {

        });
    }

    

    //Scopes for TeamTimeController

    //Account preferences

    $scope.get_gradient_checker = function () {
        $http({
            method: "POST",
            url: $scope.baseurl + "Default.aspx/get_preferences",
            headers: {
                'Content-Type': 'application/json; charset=utf-8'
            },
            data: {
                option: "gradient_color"
            },
        }).then(function success(data) {
            $scope.main_gradient_checkbox = data.data.d;
        }, function error(response) {

        });
    }

    $scope.show_content = false;
    $scope.show_content_now = function () {
        $scope.show_content = true;
    }

    $scope.gradient_checker = function () {
        $(".save-acct-settings-btn").html("Saving...");
        if ($scope.main_gradient_checkbox) {
            $scope.main_gradient_checkbox = true;
            $http({
                method: "POST",
                url: $scope.baseurl + "Default.aspx/preferences",
                data: {
                    option: "gradient_color",
                    value: "1"
                },
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                }
            }).then(function success(data) {
                // alert(data);
                $(".save-acct-settings-btn").html("Save Preference");
            }, function error(response) {
                //console.log(response);
            });
            $(".tt-equalizer-result-mobile li").removeClass("no-gradient");
            $(".tt-equalizer-mobile li").removeClass("no-gradient");
        }
        else {
            $scope.main_gradient_checkbox = false;
            $http({
                method: "POST",
                url: $scope.baseurl + "Default.aspx/preferences",
                data: {
                    option: "gradient_color",
                    value: "0"
                },
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                }
            }).then(function success(data) {
                // alert(data);
                $(".save-acct-settings-btn").html("Save Preference");
            }, function error(response) {
                //console.log(response);
            });
        }
    }
    // End of Account preferences


    //html
    ////$scope.getHtmlCounter = 0;
    $scope.getHtml = function (html) {
        ////console.log($scope.getHtmlCounter++);
        return $sce.trustAsHtml(html);
    };

    //$scope.getQuestionHtml = function (html) {
    //     if ( $scope.output.page_type != "atSensitivityAnalysis") {
    //        if ($scope.is_AT_owner == 1) {
    //            html += '<a data-node-type="0" data-location="0"  data-node="question-node" class="edit-info-doc-btn edit-pencil" title="" href="#"><span class="icon-tt-pencil">Edit Question</span></a>';
    //        }

    //    }
    //    return $sce.trustAsHtml(html);
    //};

    $scope.updateUser = function (email, name, password, passwordtochange, passwordconfirm) {
        if (passwordtochange != null) {
            if (passwordconfirm != passwordtochange) {
                $(".ASInfo").html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <h3>Password do not match          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
                $scope.onLoad();
                $timeout(function () {
                    $(".ASInfo").html("");
                }, 3000);
                return null;
            }
        }
        if (passwordtochange == null) {
            passwordtochange = "";
        }
        if (email == "" && name == "") {
            email = $scope.App.ActiveUser.UserEMail;
            name = $scope.App.ActiveUser.UserName;
        }
        $http.post(
            $scope.baseurl + "Default.aspx/updateUser",
            JSON.stringify({
                email: email,
                name: name,
                password: password,
                passwordtochange: passwordtochange
            })
            )
        .then(function success(response) {
            if (response.data.d) {
                $(".ASInfo").html('<div class="large-12 columns">          <div data-alert class="alert-box success radius tt-alert msg-stat">           <h3>Successfully saved changes          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
            }
            else {
                $(".ASInfo").html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <h3>Incorrect password          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
            }
            $scope.onLoad();
            $timeout(function () {
                $(".ASInfo").html("");
            }, 3000);
        });
    }
    $scope.App = [];
    $scope.onLoad = function () {
        $http.post(
            $scope.baseurl + "Default.aspx/getDataonLoad",
            {}
            )
        .then(function success(response) {

            $scope.App = JSON.parse(response.data.d);
        });
    }

    $scope.expandForPSA = function () {
        if ($scope.output != null) {
            if ($scope.output.page_type == "atSensitivityAnalysis") {
                $timeout(function () {
                    initPage();
                }, 400);

            }
        }
    }

    //For Modals
    $scope.modal = { 'errorMessage': '#ErrorMessage' }
    $scope.closeModal = function (modalName) {
        $(document).ready(function () { $(modalName).foundation("reveal", "close"); });
    }

    //Pipe Options
    $scope.pipeOptions = [];
    


    $scope.showResultsIndex = function (showIndex, global) {
        $http({
            method: "POST",
            url: baseUrl + "pages/Anytime/Anytime.aspx/showResultsIndex",
            data: {
                showIndex: showIndex,
                global: global
            }
        }).then(function success(response) {
            $scope.output.PipeParameters.showIndex = response.data.d;
        }, function error(response) {
            //console.log(response);
        });
    }

    //Teamtime Options
    $scope.TTOptions = [];
    $scope.showToMe = false;
    $scope.tt_is_single_line = function (is_single_line) {
        show_loading_icon();
        update(is_single_line, $scope.getStepGuid(), "single_line", "", $scope.baseurl);
    }
    $scope.hideOfflineUsers = function (hideOffline) {
        update(hideOffline, $scope.getStepGuid(), "offline", "", $scope.baseurl);
        $scope.remove_user_list();
    }
    $scope.setAnonymousMode = function (anonymousMode) {
        show_loading_icon();
        
        if (anonymousMode) {
            update("true_" + -1, $scope.getStepGuid(), "anonymous", "", $scope.baseurl);
        }
        else {
            update("false_" + -1, $scope.getStepGuid(), "anonymous", "", $scope.baseurl);
        }
    }
//    $scope.hideJudgment = function (hiddenJudgment) {
//        if (hiddenJudgment) {
//            $(".tt-j-show-overall-result").removeClass("hide");
//        } else {
//            $(".tt-j-show-overall-result").addClass("hide");
//        }
//    }
    $scope.setHiddenJUdgmentForPM = function () {
        show_loading_icon();

        if ($scope.TTOptions.hideJudgments) {
            update("true", $scope.getStepGuid(), "hide", "", $scope.baseurl);
        } else {
            update("false", $scope.getStepGuid(), "hide", "", $scope.baseurl);
        }
    }
    $scope.showJudgment = function (showJudgment) {
        $scope.showToMe = showJudgment;
    }

    $scope.hidePieChart = function (hidePieChart) {
        show_loading_icon();

        if (hidePieChart) {
            update("1", $scope.getStepGuid(), "pie-chart", "", $scope.baseurl);
        } else {
//            try {
//                var scope = angular.element($("#TeamTimeDiv")).scope();
//                calculate_pie(scope.user_judgment);
//            }
//            catch (error) {
//                //do nothing
//            }
            update("0", $scope.getStepGuid(), "pie-chart", "", $scope.baseurl);
        }
    }

    //If you do any change in this function then you have to change accordingly on the same function in TeamTimeController.js
    $scope.getStepGuid = function () {
        var stepGuId = "";

        if ($scope.output && $scope.output.sdata) {
            var stepData = eval("[" + $scope.output.sdata + "]");
            stepData = stepData[0];

            if (stepData && stepData.data) {
                stepGuId = stepData.data.stepGuid;
            }
        }

        return stepGuId;
    }

    $scope.debug = false;
    $scope.testfunction = function (text) {
        show_loading_modal();
        $http({
            method: "POST",
            url: baseUrl + "pages/Anytime/Anytime.aspx/testfunction",
            data: {
                text: text
            }
        }).then(function success(response) {
            //$scope.$apply(function () {
            alert(response.data.d);
            //});
            hide_loading_modal();

        }, function error(response) {
            //////////console.log(response);
        });
    }

    $scope.getResource = function (step) {
        show_loading_modal();
        $http({
            method: "POST",
            url: baseUrl + "pages/Anytime/Anytime.aspx/getResource",
            data: {
                step: step
            }
        }).then(function success(response) {
            //$scope.$apply(function () {
            alert(response.data.d);
            //});
            hide_loading_modal();
        }, function error(response) {
            //////////console.log(response);
        });
    }

    $scope.checkObject = function (objectName) {
        show_loading_modal();
        $http({
            method: "POST",
            url: baseUrl + "pages/Anytime/Anytime.aspx/checkObject",
            data: {
                objectName: objectName
            }
        }).then(function success(response) {
            //$scope.$apply(function () {
            alert(response.data.d);
            //console.log(response.data.d);
            //});
            hide_loading_modal();
        }, function error(response) {
            //////////console.log(response);
        });
    }

    $scope.enableDebug = function () {
        $timeout(function () {
            $("#DebugModal").foundation("reveal", "open");
        }, 500);
    }

    $scope.isRestricted = false;


    $scope.has_long_content = function (content) {
        if (typeof (content) != "undefined") {
            if (content.length > 50) {
                if (/<[a-z\][\s\S]*>/i.test(content)) {
                    if (content.match(/<img/)) {
                        return true;
                    }
                    if (!$(content)[0].hasChildNodes()) {
                        return false;
                    }
                }
                return true;
            }
        }
        
    }

    //Help URL

    $scope.showPipeHelpPopup = function () {
        if ($scope.output.help_pipe_root.length > 0 && $scope.output.help_pipe_url.length > 0) {
            var tParam = $scope.output.help_pipe_url.substring(0, $scope.output.help_pipe_url.lastIndexOf("/")) + "/Welcome.htm";
            var filterParam = ($scope.isMobile() ? "mobile" : "desktop");
            var pipeHelpUrl = $scope.output.help_pipe_root + "?filter=" + filterParam + "&t=" + tParam + "#t=" + $scope.output.help_pipe_url;
            var options = "menubar=no,maximize=no,titlebar=no,status=yes,location=yes,toolbar=yes,channelmode=no,scrollbars=yes,resizable=yes,width=950,height=600,left=50,top=30";
            window.open(pipeHelpUrl, "", options, true); 
        }
            
        return false;
    }

    function show_loading_modal() {
        $(".fullwidth-loading-wrap").removeClass("hide");
        $(".fullwidth-loading-wrap").css("display", "inline");
    }

    //End of Help URL

    //Knowledge Owl Help Starts

    //$scope.hideKoHelpPage = function () {
    //    $scope.$broadcast("hideKoHelpPage");
    //}

    $scope.showKoHelpPage = function () {
        $scope.$broadcast("showKoHelpPage");
    }

    //Knowledge Owl Help Ends

    $scope.getEvaluationProgressPercentage = function (decimalPercent, isTooltip) {
        var valueReturn = 0;

        if (decimalPercent && $scope.output) {
            valueReturn = $scope.output.total_evaluation.toString().length - 3; //getting how many digits more than thousand
            var decimalPoints = isTooltip ? ((valueReturn > 0 ? valueReturn : 0) + 3)
                : decimalPercent < 100 ? 1 : 0;

            valueReturn = parseFloat(decimalPercent.toFixed(decimalPoints));
        }
        
        return valueReturn;
    }

}]);

//reference:http://jsfiddle.net/sonicblis/by2jax5v/
app.filter("html", function ($sce) {
    return function (val) {
        return $sce.trustAsHtml(val);
    };
});

app.filter("startFrom", function () {
    return function (input, start) {
        if (!input || !input.length) { return; }
        start = +start; //parse to int
        return input.slice(start);
    }
});
app.filter("startTo", function () {
    return function (input, noOfUsers, currentPage) {
        if (!input || !input.length) { return; }
        if (currentPage == 0) { return input; }
        var start = (currentPage * noOfUsers) - (noOfUsers);
        if (start == 0)
            start = 1;
        var end = (currentPage * noOfUsers);
        var currentUser = input.slice(0, 1);
        var currentParticipants = input.slice(start, end);
        var CurrentArray = currentUser.concat(currentParticipants);
        return CurrentArray;
    }
});

var clicks = [false, false];

app.directive("ngDbltap", function () {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            
            element.bind("click", function () {
                if (!clicks[0]) {
                    clicks[0] = true;
                }
                else {
                    if (!clicks[1]) {
                        clicks[1] = true;
                        scope.$parent.showHiddenValue();
                        clicks = [false, false];
                    }
                }


                setTimeout(function () {
                    clicks = [false, false];
                }, 1000);
            });
        }
    };
});



if (typeof AnytimeController !== "undefined") {
    initAnytimeCtrl();
}

function currentDateTimeString() {
    var now = new Date();
    var hours = now.getHours();
    hours = hours < 10 ? "0" + hours : hours;
    var minutes = now.getMinutes();
    minutes = minutes < 10 ? "0" + minutes : minutes;

    var seconds = now.getSeconds();
    seconds = seconds < 10 ? "0" + seconds : seconds;

    var milliSeconds = now.getMilliseconds();
    milliSeconds = milliSeconds < 10 ? "00" + milliSeconds : (milliSeconds < 100 ? "0" + milliSeconds : milliSeconds);

    return ("" + hours + ":" + minutes + ":" + seconds + "." + milliSeconds);
}

function consoleLog(logMessage) {
    if (logMessage) {
        console.log(currentDateTimeString() + ": " + logMessage);
    }
}