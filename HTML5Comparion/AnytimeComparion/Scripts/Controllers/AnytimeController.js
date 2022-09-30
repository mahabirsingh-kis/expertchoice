/// <reference path="~/Scripts/angular.js" />
/// <reference path="~/Scripts/angular-route.js" />
/// <reference path="~/Scripts/angular-resource.js" />
/// <reference path="~/Scripts/angular-sanitize.js" />
/// <reference path="~/Scripts/jquery-1.8.2.js" />
/// <reference path="MainController.js" />
/// <reference path="../responsive-tables.js" />
/// <reference path="../app.js" />

//************ Angular JS Codes ****************





//app.run(function () {
//    FastClick.attach(document.body);
//});

var AnytimeController = true;

function initAnytimeCtrl() {
    app.controller("AnytimeController",["$scope", "$http", "$timeout", "$window", "$interval", "$anchorScroll", "$location", "$compile", function ($scope, $http, $timeout, $window, $interval, $anchorScroll, $location, $compile) {
        $(window).bind("beforeunload", function (e) {
            //$scope.showLoadingMessage = false; //for hiding loading/saving message
            if ($scope.output.page_type == "atSpyronSurvey") {
                $scope.get_pipe_data($scope.current_step, $scope.is_AT_owner, null);
            } else {
                $scope.save_multis();
            }

            console.log("block_unload_prompt: " + $scope.$parent.block_unload_prompt + ", current_step: " + $scope.current_step);

            if (!$scope.$parent.block_unload_prompt && $scope.current_step > 1) {
                //return "Are you sure you want to leave this page?";
                var messageString = "";
                if (!e) {
                    e = window.event;
                }

                // For IE and Firefox 
                if (e) {
                    e.returnValue = messageString;
                }
                // For Safari
                return messageString;
            }

            return null;
        });

        $(window).bind("unload", function (e) {
            $.ajax({
                type: "POST",
                async: false,
                url: baseUrl + "Default.aspx/unsetAllowIESession",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    
                },
                error: function (response) {
                   
                }
            });
        });

        $scope.output.page_type = ""; //temp initialization
        $scope.temp_multi_data = [];
        $scope.$parent.block_unload_prompt = false;
        $scope.is_anytime = true;
        $scope.is_teamtime = false;
        $scope.$parent.is_anytime = true;
        $scope.$parent.is_teamtime = false;
        $scope.qh_appy_all = true;
        //hide teamtime-related buttons
        $(".tt-related-elements").hide();
        $scope.active_multi_index = 0;
        $scope.graphical_slider = [];
        $scope.main_bar = [];
        $scope.main_bar_txt = [];
        var baseUrl = $("#base_url").attr("href");
        $scope.output = null;
        $scope.welcome_information_message = "<div class='tt-homepage medium-8 columns  large-centered medium-centered tt-center-height-wrap'> <div class='text-center tt-center-content-wrap'><h1>Welcome to Expert Choice Comparion</h1> <p> Comparion is a collaborative decision tool on the web where a team can come together to evaluate factors influencing a decision and alternative decision choices. After completing the task on a page, you simply need to click 'Next' to continue. You may be alerted along the way of specific things to keep in mind. </p></div></div>";
        $scope.ty_information_message = '<div class="tt-homepage medium-8 columns  large-centered medium-centered tt-center-height-wrap"> <div class="text-center tt-center-content-wrap"><h1>You are done!</h1><p>Thank you for providing your input.</p></div></div>';
        $scope.numericalSlider = [];
        $scope.refreshed = false;
        $scope.gptoVerbal = false;
        //verbal bars
        $scope.bars_left =
                        new Array(
                            ["9", "Extreme", "nine", "EX"],
                            ["8", "Very Strong to Extreme", "eight", " "],
                            ["7", "Very Strong", "seven", "VS"],
                            ["6", "Strong to Very Strong", "six", " "],
                            ["5", "Strong", "five", "S"],
                            ["4", "Moderate to Strong", "four", " "],
                            ["3", "Moderate", "three", "M"],
                            ["2", "Equal to Moderate", "two", " "]
                        );
        $scope.bars_right = new Array(
                            ["2", "Equal to Moderate", "two", " "],
                            ["3", "Moderate", "three", "M"],
                            ["4", "Moderate to Strong", "four", " "],
                            ["5", "Strong", "five", "S"],
                            ["6", "Strong to Very Strong", "six", " "],
                            ["7", "Very Strong", "seven", "VS"],
                            ["8", "Very Strong to Extreme", "eight", " "],
                            ["9", "Extreme", "nine", "EX"]
                        );

        $scope.sessionTimeoutTime;
        $scope.isAutoSaving = false;
        $scope.is_multi = false;
        $scope.current_step = 1;
        //$scope.pw_advantage = 1;
        $scope.is_AT_owner = 0;
        $scope.node_type = null;
        $scope.lowest_value = -999999999999;
        $scope.multi_comments = [];
        $scope.oldSliderValue = [];
        var outerIndex = -1;
        var snapValues = [];
        var sliderLeft = [];
        var sliderRight = [];


        $scope.is_saving_judgment = false;
        $scope.swipe_enabled = true;

        $scope.check_swipe = function (step) {
            if ($scope.swipe_enabled)
                $scope.get_pipe_data(step);
        }

        var pipeContents = localStorage.getItem("pipeContents") === null ? (  {
            pairwise : null,
            multiPairwise : null,
            direct : null,
            multiDirect : null,
            ratings : null,
            multiRatings : null,
            utility : null,
            stepfunction : null,
            localresults : null,
            globalresults : null,
            survey : null,
            sensitivty: null
        }) : (JSON.parse(localStorage.getItem("pipeContents")));

        $scope.display_selected_bar = function (value, advantage) {
            value = Math.abs(Math.floor(value));
            if (advantage == 1 && value >=2) {

                return '<div class=" text-centered small-7 left columns eq-label" > <span class="leftLabel">' + $scope.bars_right[value - 2][1] + '</span></div>';
            }
            if (advantage == -1) {
                var index = ($scope.bars_left.length + 1) - value;
                if (index < 0) {
                    index = -1 * index;
                }
                return '<div class=" text-centered  small-7 right columns eq-label" > <span class="rightLabel">' + $scope.bars_left[index][1] + '</span></div>';
            }
            else {
                if (value == 1) {
                    return '<div class=" text-center small-6 small-centered columns eq-label" > <span class="equal">Equal</span> </div>';
                }
                return "";
            }

        }

        $scope.get_label_position = function (advantage, value) {
            if (advantage == 1) {
                return { "right": ((value > 5) ? value - (value % 2 == 1 ? 1.3 : 3) : -4) + "em" };
            }
            else if (advantage == -1) {
                return { "left": ((value > 5) ? value - (value % 2 == 1 ? 1.3 : 3) : -4) + "em" };
            }
            return false;
        }
        
        $scope.$on("save_multis", function (e) {
            $scope.save_multis();
        });

        $scope.$on("set_vertical_bars", function (e) {
            $scope.set_vertical_bars();

        });

        $scope.set_vertical_bars = function () {
            $scope.set_multi_index($scope.active_multi_index);
            $scope.output.collapse_bars = $scope.output.orig_collapse_bars;
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/setMultiBarsMode",
                data: {
                    value: $scope.output.collapse_bars
                }
            }).then(function success(response) {
                //console.log(response);
            }, function error(response) {
                //console.log(response);
            });
        }

        $scope.saveMultisAndGetPipeData = function (newStep) {
            $scope.$parent.closeWarningMessage();
            $scope.save_multis();

            $timeout(function() {
                $scope.get_pipe_data(newStep);
            }, 200, false);
        }

        $scope.save_multis = function () {
            $scope.gptoVerbal = false;
            if ($scope.output != null) {
                var comment_str = "";
                if ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atAllPairwiseOutcomes") {
                    //$scope.$parent.runProgressBar(1, 0, 50, stringResources.yellowLoadingIcon.save);
                    $scope.save_multi_pairwise_on_next();
                }
                if (($scope.output.page_type == "atNonPWAllChildren" || $scope.output.page_type == "atNonPWAllCovObjs") && $scope.output.non_pw_type == "mtRatings") {
                    //$scope.$parent.runProgressBar(1, 0, 50, stringResources.yellowLoadingIcon.save);
                    $scope.save_multi_ratings_on_next();
                }
                if (($scope.output.page_type == "atNonPWAllChildren" || $scope.output.page_type == "atNonPWAllCovObjs") && $scope.output.non_pw_type == "mtDirect") {
                    //$scope.$parent.runProgressBar(1, 0, 50, stringResources.yellowLoadingIcon.save);
                    $scope.save_multi_direct_on_next();
                }
                if ($scope.output.page_type == "atPairwise") {
                    $scope.save_pairwise_on_next(false);
                }

                if ($scope.output.page_type == "atNonPWOneAtATime" && $scope.output.non_pw_type == "mtStep") {
                    $scope.save_step_function_on_next();
                }
                if ($scope.output.page_type == "atNonPWOneAtATime" && ($scope.output.non_pw_type == "mtAdvancedUtilityCurve" || $scope.output.non_pw_type == "mtRegularUtilityCurve")) {
                    $scope.save_utility_curve_on_next();
                }

                if ($scope.output.page_type == "atNonPWOneAtATime" && $scope.output.non_pw_type == "mtRatings") {
                    $scope.save_ratings_on_next();
                }

                if ($scope.output.page_type == "atNonPWOneAtATime" && $scope.output.non_pw_type == "mtDirect") {
                    $scope.save_direct_on_next();
                }

                
            }
        }


        $scope.set_collapse_cookies = function (node_type) {
            var is_hidden = $("." + node_type + "-info-div").is(":visible") ? 1 : 0;
            var bool_hidden = $("." + node_type + "-info-div").is(":visible");
            var currentStepType = ($scope.is_multi ? "All_" : "One_") + $scope.output.pairwise_type + $scope.output.non_pw_type;

            ////console.log("node_type: " + node_type);
            if (node_type == "left-node" || node_type == "right-node" || node_type == "parent-node") {
                if (node_type == "parent-node" && !$scope.is_multi && $scope.output.page_type == "atPairwise") {
                    node_type = "parent-node";
                } else {
                    node_type = "pw-nodes";
                }
            }
            else if (node_type == "wrt-left-node" || node_type == "wrt-right-node") {
                if ($scope.output.non_pw_type == "mtDirect" || $scope.output.non_pw_type == "mtRatings" || currentStepType == "One_mtStep" || currentStepType == "One_mtRegularUtilityCurve") {
                    node_type = "pw-nodes";
                } else {
                    node_type = "pw-wrt-nodes";
                }
            }

            ////console.log("changed node_type: " + node_type);
            if (angular.isUndefined($scope.project_id)) {
                $scope.project_id = $scope.current_project.project_id;
            }

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/setCollapseCookies",
                data: {
                    projectId: $scope.project_id,
                    stepType: currentStepType,
                    node_type: node_type,
                    status: is_hidden
                }
            }).then(function success(response) {
                angular.element(".tt-accordion-content").attr("style", "");

                if (node_type == "pw-nodes" || node_type == "parent-node") {
                    if ($scope.is_multi) {
                        $scope.setCollapsedInfoDocsValue(0, bool_hidden);
                        $scope.setCollapsedInfoDocsValue(1, bool_hidden);
                        $scope.setCollapsedInfoDocsValue(2, bool_hidden);
                    } else {
                        if (node_type == "pw-nodes") {
                            if ($scope.output.non_pw_type == "mtDirect" || $scope.output.non_pw_type == "mtRatings" || $scope.output.non_pw_type == "mtStep" || $scope.output.non_pw_type == "mtRegularUtilityCurve") {
                                $scope.setCollapsedInfoDocsValue(0, bool_hidden);
                                $scope.setCollapsedInfoDocsValue(1, bool_hidden);
                                $scope.setCollapsedInfoDocsValue(3, bool_hidden);
                            } else {
                                $scope.setCollapsedInfoDocsValue(1, bool_hidden);
                                $scope.setCollapsedInfoDocsValue(2, bool_hidden);
                            }
                        }
                        else {
                            $scope.setCollapsedInfoDocsValue(0, bool_hidden);
                        }
                    }
                } else if (node_type == "pw-wrt-nodes") {
                    $scope.setCollapsedInfoDocsValue(3, bool_hidden);
                    $scope.setCollapsedInfoDocsValue(4, bool_hidden);
                }

                if (!is_hidden) {
                    if ($scope.is_judgment_screen) {
                        $timeout(function () {
                            $scope.change_info_doc_image_size();
                        }, 50);
                    }
                }
            }, function error() {
                //console.log(response);
            });
        }

        $scope.setCollapsedInfoDocsValue = function (index, newValue) {
            if ($scope.collapsed_info_docs[index] != newValue) {
                $scope.collapsed_info_docs[index] = newValue;
                //console.log("collapsed_info_docs[" + index + "] got new value");
            }
        }

        $scope.get_collapse_cookies = function (node_type) {
            var is_hidden = $("." + node_type + "-info-div").is(":visible") ? 1 : 0;
            var currentStepType = ($scope.is_multi ? "All_" : "One_") + $scope.output.pairwise_type + $scope.output.non_pw_type;
            
            if (angular.isUndefined($scope.project_id)) {
                $scope.project_id = $scope.current_project.project_id;
            }

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/getCollapseCookies",
                data: {
                    projectId: $scope.project_id,
                    stepType: currentStepType,
                    node_type: node_type,
                    is_multi: $scope.is_multi
                },
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                }
            }).then(function success(response) {
                angular.element(".tt-accordion-content").attr("style", "");
                var is_hidden = response.data.d;
                var bool_hidden = is_hidden == "1" ? true : false;
                var value1, value2, value3;

                ////console.log("node_type: " + node_type);

                //Setting collapsed_info_docs values and set collapsed_info_docs to true when info doc is empty
                if (node_type == "pw-nodes" || node_type == "parent-node") {
                    if ($scope.is_multi) {
                        value1 = $scope.is_html_empty($scope.output.parent_node_info) ? true : bool_hidden;
                        $scope.setCollapsedInfoDocsValue(0, value1);

                        if ($scope.output.page_type == "atAllPairwise") {
                            value2 = $scope.is_html_empty($scope.multi_data[$scope.active_multi_index].InfodocLeft) ? true : bool_hidden;
                            value3 = $scope.is_html_empty($scope.multi_data[$scope.active_multi_index].InfodocRight) ? true : bool_hidden;
                            $scope.setCollapsedInfoDocsValue(1, value2);
                            $scope.setCollapsedInfoDocsValue(2, value3);
                        } else if ($scope.output.page_type != "atShowLocalResults") {
                            value2 = $scope.is_html_empty($scope.output.multi_non_pw_data[$scope.active_multi_index].Infodoc) ? true : bool_hidden;
                            value3 = $scope.is_html_empty($scope.output.multi_non_pw_data[$scope.active_multi_index].InfodocWRT) ? true : bool_hidden;
                            $scope.setCollapsedInfoDocsValue(1, value2);
                            $scope.setCollapsedInfoDocsValue(2, value3);
                        }
                    } else {
                        if (node_type == "pw-nodes") {
                            if ($scope.output.non_pw_type == "mtDirect" || $scope.output.non_pw_type == "mtRatings" || $scope.output.non_pw_type == "mtStep" || $scope.output.non_pw_type == "mtRegularUtilityCurve") {
                                value1 = $scope.is_html_empty($scope.output.parent_node_info) ? true : bool_hidden;
                                value2 = $scope.is_html_empty($scope.output.first_node_info) ? true : bool_hidden;
                                value3 = $scope.is_html_empty($scope.output.wrt_first_node_info) ? true : bool_hidden;
                                $scope.setCollapsedInfoDocsValue(0, value1);
                                $scope.setCollapsedInfoDocsValue(1, value2);
                                $scope.setCollapsedInfoDocsValue(3, value3);
                            } else {
                                value1 = $scope.is_html_empty($scope.output.first_node_info) && $scope.is_html_empty($scope.output.second_node_info) ? true : bool_hidden;
                                value2 = bool_hidden;    //Doesn't have any info doc for this
                                $scope.setCollapsedInfoDocsValue(1, value1);
                                $scope.setCollapsedInfoDocsValue(2, value2);
                            }
                        }
                        else {
                            value1 = $scope.is_html_empty($scope.output.parent_node_info) ? true : bool_hidden;
                            $scope.setCollapsedInfoDocsValue(0, value1);
                        }
                    }
                } else if (node_type == "pw-wrt-nodes") {
                    if ($scope.is_multi) {
                        value1 = $scope.is_html_empty($scope.multi_data[$scope.active_multi_index].InfodocLeftWRT) && $scope.is_html_empty($scope.output.wrt_second_node_info) ? true : bool_hidden;
                        value2 = $scope.is_html_empty($scope.multi_data[$scope.active_multi_index].InfodocRightWRT) ? true : bool_hidden;
                        $scope.setCollapsedInfoDocsValue(3, value1);
                        $scope.setCollapsedInfoDocsValue(4, value2);
                    } else {
                        value1 = $scope.is_html_empty($scope.output.wrt_first_node_info) && $scope.is_html_empty($scope.output.wrt_second_node_info) ? true : bool_hidden;
                        value2 = bool_hidden;    //Doesn't have any info doc for this
                        $scope.setCollapsedInfoDocsValue(3, value1);
                        $scope.setCollapsedInfoDocsValue(4, value2);
                    }
                }
            }, function error(response) {
                //console.log(response);
            });
        }


        //QH

        $scope.set_qh_cookies = function (value, qh_text) {
            if (angular.isUndefined($scope.project_id)) {
                $scope.project_id = $scope.current_project.project_id;
            }

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/setQHCookies",
                data: {
                    ProjectID: $scope.project_id,
                    step: $scope.current_step,
                    status: value,
                    qh_text: qh_text

                },
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                }
            }).then(function success(response) {
                //alert(JSON.stringify(response));
            }, function error(response) {
                //console.log(response);
            });
        }
        $scope.dontShowMessageCookie = function (value) {
            $scope.output.dont_show = value;
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/dontShowMessageCookie",
                data: {
                    status: value
                }
            }).then(function () {
            });
        }

        $scope.showMessage = function (sessvar) {
            $scope.output.dont_show = sessvar;
            if ($scope.output.dont_show)
                $scope.showResult();
        }

        $scope.get_qh_cookies = function () {
            if (angular.isUndefined($scope.project_id)) {
                $scope.project_id = $scope.current_project.project_id;
            }

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/getQHCookies",
                data: {
                    ProjectID: $scope.project_id,
                    step: $scope.current_step
                },
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                }
            }).then(function success(response) {
                if (response.data.d == 1) {
                    return true;
                }
                else {
                    return false;
                }
            }, function error(response) {
                return false;
            });
        }

        $scope.get_quick_help = function () {
            var step = $scope.current_step;
            if ($scope.output.QH_cluster) {
                step = 0;
            }

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/getQuickHelpInfo",
                data: {
                    tNodeID: $scope.output.qh_tnode_id,
                    tEvalStep: $scope.output.qh_help_id,
                    step: step,
                    show_qh_automatically: $scope.output.show_qh_automatically
                }
            }).then(function success(response) {
                $timeout(function() {
                    $scope.$apply(function () {
                        $scope.output.qh_info = response.data.d;
                        $scope.qh_text = $scope.output.qh_info;

                        if ($scope.$parent.getHtml($scope.output.qh_info) != "") {
                            var tmp_qh_str = $scope.output.qh_info;
                            if (tmp_qh_str.indexOf("img") >= 0) {
                                //if has image
                                //$scope.qh_text = $scope.$parent.getHtml($scope.output.qh_info);
                            } else {
                                $scope.qh_text = $("'" + $scope.$parent.getHtml($scope.output.qh_info) + "'").text()
                                    .trim();
                            }
                        }

                        $scope.output.qh_yt_info = $scope.output.qh_info;
                        quick_help = $scope.output.qh_yt_info;
                        $scope.$parent.qh_text = $scope.qh_text;
                    });
                }, 1000);
                $(document).foundation();
                angular.element("#tt-qh-modal").foundation("reveal", "close");
            }, function error(response) {
                alert(JSON.stringify(response));

            });
        }

        $scope.flash = function (text) {
            //alert($scope.qh_text);
            if (text != "") {
                if (!$scope.output.show_qh_automatically) {
                    $(".qh-icons").addClass("change-color-pulse");
                }
                $timeout(function () {
                    $(".qh-icons").removeClass("change-color-pulse");
                }, 5000);
            }
        }

        $scope.format_survey_question = function (question) {
            var elem = null;
            if (question.indexOf("<P>") > -1) {
                elem = $(question);
                if (elem[2] != "undefined" || elem[2] != null)
                    return $scope.$parent.getHtml(elem[2].innerHTML);
            }
            return $scope.$parent.getHtml(question);
        }

        $scope.set_qe_content = function () {
            var content = tinymce.activeEditor.getBody().innerText.trim();

            if (content.length > 0) {
                var confirmMessage = "Your current content will be replaced with default Quick Help example.\nDo you want to continue?";

                if (confirm(confirmMessage)) {
                    try {
                        tinymce.activeEditor.setContent($scope.output.defaultQhInfo);
                    } catch (e) {

                    }
                }
            } else {
                try {
                    tinymce.activeEditor.setContent($scope.output.defaultQhInfo);
                } catch (e) {

                }
            }
        }

        $scope.$on("show_qh_automatically", function (e) {
            $scope.show_qh_automatically();
        });

        $scope.show_qh_automatically = function (status) {
            //alert(status);
            // alert($scope.output.show_qh_automatically);
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/setQHSettingCookies",
                data: {
                    status: status
                }
            }).then(function success(response) {
                //alert($scope.output.QH_cluster);
            }, function error(response) {
                //console.log(response);
            });
        }

        $scope.temp_node_ids = [];
        $scope.cluster_nodes = [];
        $scope.add_to_cluster_nodes = function (is_checked, node) {
            if (is_checked) {
                $scope.cluster_nodes.push(node);
            }
        }

       
        $scope.save_by_cluster = function (nodes) {
           // //console.log($scope.cluster_nodes);
            
            var node_ids = [];
            $.each($scope.cluster_nodes, function (i, node) {
                node_ids.push(node[2]);
            });

            node_ids.push($scope.output.parentnodeID);
            //console.log($scope.output);
            var is_multi = false;
            var is_evaluation = true;

            if ($scope.output.page_type == "atNonPWAllChildren" || $scope.output.page_type == "atNonPWAllCovObjs" || $scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atAllPairwiseOutcomes" || $scope.output.page_type == "atShowLocalResults") {
                is_multi = true;
            }
            
            if ($scope.output.page_type == "atShowLocalResults" || $scope.output.page_type == "atShowGlobalResults") {
                is_evaluation = false;
            }

           

            //node type was in global.js
            if (node_type == "quick-help") {
                $.ajax({
                    type: "POST",
                    url: baseUrl + "Pages/Anytime/Anytime.aspx/setQuickHelpInfoByCluster",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        nodes: $scope.cluster_nodes,
                        qh_info: draft,
                        show_qh_automatically: $scope.output.show_qh_automatically
                    }),
                    beforeSend: function () {
                        $(".tt-loading-icon-wrap").show();
                        // $(".apply-nodes-btn").html("Saving...");
                    },
                    success: function (data) {
                        $(".tt-loading-icon-wrap").hide();
                        //console.log(node_ids);
                        //if (is_anytime && node_type == 0) {
                        // $(".step-" + current_step).click();
                        // }
                        // $(".tt-loading-icon-wrap").hide();
                        // $(".apply-nodes-btn").html('<a href="#" class="button tiny tt-button-icond-left apply-nodes-btn button tiny tt-button-primary success"><span class="icon-tt-save icon"></span><span class="text">Save</span></a>');
                        // $("#GlobalNodesModal .success").show();
                        $scope.get_pipe_data($scope.current_step);
                        setTimeout(function () {
                            $("#cluster-modal").foundation("reveal", "close");
                        }, 2000);

                    },
                    error: function (response) {
                        //console.log(response);
                        $(".tt-loading-icon-wrap").hide();
                        $(".success").hide();
                        $("#GlobalNodesModal .alert-box .alert").show();
                    }
                });
            }
            else {
                $.ajax({
                    type: "POST",
                    url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/ApplyCustomWordingToNodes",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        node_ids: node_ids,
                        custom_word: draft,
                        is_multi: is_multi,
                        is_evaluation: is_evaluation
                    }),
                    beforeSend: function () {
                        $(".tt-loading-icon-wrap").show();
                        // $(".apply-nodes-btn").html("Saving...");
                    },
                    success: function (data) {
                        $(".tt-loading-icon-wrap").hide();
                        //console.log(node_ids);
                        //if (is_anytime && node_type == 0) {
                        // $(".step-" + current_step).click();
                        // }
                        // $(".tt-loading-icon-wrap").hide();
                        // $(".apply-nodes-btn").html('<a href="#" class="button tiny tt-button-icond-left apply-nodes-btn button tiny tt-button-primary success"><span class="icon-tt-save icon"></span><span class="text">Save</span></a>');
                        // $("#GlobalNodesModal .success").show();
                        $scope.get_pipe_data($scope.current_step);
                        setTimeout(function () {
                            $("#cluster-modal").foundation("reveal", "close");
                        }, 2000);
                   
                    },
                    error: function (response) {
                        //console.log(response);
                        $(".tt-loading-icon-wrap").hide();
                        $(".success").hide();
                        $("#GlobalNodesModal .alert-box .alert").show();
                    }
                });
            }
           
      

            //$.each($scope.cluster_nodes, function (i, node) {
            //    $http({
            //        method: "POST",
            //        url: baseUrl + "pages/Anytime/Anytime.aspx/setQuickHelpInfo",
            //        data: {
            //            tNodeID: node[2],
            //            tEvalStep: $scope.output.qh_help_id,
            //            step: step,
            //            actual_step: $scope.current_step,
            //            message: draft.trim(),
            //            show_qh_automatically: $scope.output.show_qh_automatically
            //        }
            //    }).then(function success(response) {
            //        $scope.output.qh_info = draft.trim();
            //        $scope.get_quick_help();
            //        $(document).foundation();
            //        angular.element("#tt-qh-modal").foundation("reveal", "close");
            //    }, function error(response) {
            //        alert(JSON.stringify(response));

            //    });
            //});

            
        }

        $scope.save_quick_help = function () {
            //alert($scope.qh_appy_all);
            var step = $scope.current_step;
            if ($scope.output.QH_cluster) {
                step = 0;
            }

            try {
                var draft = tinymce.activeEditor.getContent();
            }
            catch (e) {
                var draft = "";
            }


            test = "";
            if (draft != "") {
                try {
                    $(draft).find("title").remove();
                }
                catch (e) {

                } sRes = $scope.output.sRes;

                if (draft.indexOf("img") >= 0) {
                    //if has image
                    test = draft;
                }
                else {
                    test = $(draft).text();
                }
            }


            if (test.trim() == "") {
                draft = "";
            }

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/setQuickHelpInfo",
                data: {
                    tNodeID: $scope.output.qh_tnode_id,
                    tEvalStep: $scope.output.qh_help_id,
                    step: step,
                    actual_step: $scope.current_step,
                    message: draft.trim(),
                    show_qh_automatically: $scope.output.show_qh_automatically
                }
            }).then(function success(response) {
                $scope.output.qh_info = draft.trim();
                $scope.get_quick_help();
                $(document).foundation();
                angular.element("#tt-qh-modal").foundation("reveal", "close");
            }, function error(response) {
                //alert(JSON.stringify(response));

            });
        }

        //QH

        //update infodoc headings
        $scope.update_infodoc_headings = function (node_type, node_guid, wrt_guid) {
            if ($scope.is_multi) {
                return false;
            }
            // $scope.output.infodoc_params[i];
            params = "t=test",
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/setInfodocParams",
                data: {
                    NodeID: node_guid,
                    WrtNodeID: wrt_guid,
                    value: params,
                    message: $scope.output.qh_info,
                    is_multi: $scope.is_multi
                }
            }).then(function success(response) {

            }, function error(response) {
                //console.log(response);
            });

        }

        $scope.update_infodoc_params = function (node_type, node_guid, wrt_guid, heading_id) {
          
            if ($scope.is_multi) {
                $timeout(function () {
                    $scope.CheckMultiHeight("tt-homepage-main-wrap", "40");
                    $scope.CheckMultiHeight("multi-loop-wrap", "80");
                    
                    $scope.CheckMultiHeight("multi-loop-wrap-local-results", "500");
                    $scope.CheckMultiHeight("tt-pipe-canvas", "152");
                    //$scope.CheckMultiHeight("tt-rating-scale", "100");
                }, 800);
                //return false;
            }

            //        alert(node_guid);
            var is_hidden = $("." + node_type + "-info-div").is(":visible") ? 1 : 0;

            //`c=1&w=300&h=200`
            var width = $("." + node_type + "-info-div > .tt-panel").width();
            var height = $("." + node_type + "-info-div > .tt-panel").height();
            var params = "c=" + is_hidden + "&w=" + width + "&h=" + height;
            var pair_params = params; //pass this w/o 't'
            if (typeof (heading_id) != "undefined") {
                is_hidden = is_hidden == 1 ? 0 : 1;
                params = "c=" + is_hidden + "&w=" + width + "&h=" + height;
                if (angular.element("#" + heading_id).val() != "") {
                    params = params + "&t=" + angular.element("#" + heading_id).val();
                }
                else {
                    params = params + "&t=-2";
                }
            }
            else {
                heading_id = "";
            }
            
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/setInfodocParams",
                data: {
                    NodeID: node_guid,
                    WrtNodeID: wrt_guid,
                    value: params,
                    is_multi: $scope.is_multi
                }
            }).then(function success(response) {

                var index = 0;
                var pair_index = -1;
                var pair_indeces = [];
                var node_guids = [];
                var wrt_guids = [];
                if (node_type == "parent-node") {
                    index = 0;

                    node_guids[0] = node_guid;
                    wrt_guids[0] = wrt_guid;
                    pair_indeces.push(0);
                    if ($scope.output.page_type != "atPairwise" && !$scope.is_multi) {
                        node_guids[1] = $scope.output.LeftNodeGUID;
                        wrt_guids[1] = "";

                        node_guids[2] = $scope.output.LeftNodeGUID;
                        wrt_guids[2] = $scope.output.ParentNodeGUID;

                        pair_indeces.push(1);
                        pair_indeces.push(3);
                    }

                    //if ($scope.is_multi) {
                    //    node_guids[0] =$scope.output.multi_GUIDs[$scope.active_multi_index][1];
                    //    wrt_guids[0] = $scope.output.multi_GUIDs[$scope.active_multi_index][2];

                    //    node_guids[1] = $scope.output.multi_GUIDs[$scope.active_multi_index][2];
                    //    wrt_guids[1] = $scope.output.multi_GUIDs[$scope.active_multi_index][1];

                    //    pair_indeces.push(1);
                    //    pair_indeces.push(2);
                    //}
                }
                else if (node_type == "left-node") {
                    index = 1;
                    node_guid = $scope.output.RightNodeGUID;
                    wrt_guid = $scope.output.LeftNodeGUID;

                    node_guids[0] = $scope.output.RightNodeGUID;
                    wrt_guids[0] = $scope.output.LeftNodeGUID;
                    pair_index = 2;
                    if ($scope.is_multi) {
                        //node_guids[0] = $scope.output.multi_GUIDs[$scope.active_multi_index][0];
                        //wrt_guids[0] = "";

                        //node_guids[1] = $scope.output.multi_GUIDs[$scope.active_multi_index][2];
                        //wrt_guids[1] = $scope.output.multi_GUIDs[$scope.active_multi_index][1];


                        //pair_indeces.push(0);
                        //pair_indeces.push(2);
                    }
                    else {
                        pair_indeces.push(pair_index);

                        if ($scope.output.page_type != "atPairwise") {
                            node_guids[1] = $scope.output.ParentNodeGUID;
                            wrt_guids[1] = "";

                            node_guids[2] = $scope.output.LeftNodeGUID;
                            wrt_guids[2] = $scope.output.ParentNodeGUID;

                            pair_indeces.push(0);
                            pair_indeces.push(3);
                        }
                    }
                }
                else if (node_type == "right-node") {
                    index = 2;
                    node_guid = $scope.output.LeftNodeGUID;
                    wrt_guid = $scope.output.RightNodeGUID;

                    node_guids[0] = $scope.output.LeftNodeGUID;
                    wrt_guids[0] = $scope.output.RightNodeGUID;
                    pair_index = 1;
                    if ($scope.is_multi) {
                        //node_guids[0] = $scope.output.multi_GUIDs[$scope.active_multi_index][0];
                        //wrt_guids[0] = "";

                        //node_guids[1] = $scope.output.multi_GUIDs[$scope.active_multi_index][1];
                        //wrt_guids[1] = $scope.output.multi_GUIDs[$scope.active_multi_index][2];

                        //pair_indeces.push(0);
                        //pair_indeces.push(1);
                    }
                    else {
                        pair_indeces.push(pair_index);
                    }
                    //angular.element(".left-node").click();
                }
                else if (node_type == "wrt-left-node") {
                    index = 3;
                    node_guid = $scope.output.RightNodeGUID;
                    wrt_guid = $scope.output.ParentNodeGUID;

                    node_guids[0] = $scope.output.RightNodeGUID;
                    wrt_guids[0] = $scope.output.ParentNodeGUID;

                    pair_index = 4;
                    if ($scope.is_multi) {
                        //node_guids[0] = $scope.output.multi_GUIDs[$scope.active_multi_index][2];
                        //wrt_guids[0] = $scope.output.multi_GUIDs[$scope.active_multi_index][0];
                        //pair_indeces.push(4);
                    }
                    else {
                        pair_indeces.push(pair_index);
                        if ($scope.output.page_type != "atPairwise") {
                            node_guids[1] = $scope.output.ParentNodeGUID;
                            wrt_guids[1] = "";
                            node_guids[2] = $scope.output.LeftNodeGUID;
                            wrt_guids[2] = "";
                            pair_indeces.push(0);
                            pair_indeces.push(1);
                        }
                    }
                }
                else if (node_type == "wrt-right-node") {
                    index = 4;
                    node_guid = $scope.output.LeftNodeGUID;
                    wrt_guid = $scope.output.ParentNodeGUID;

                    node_guids[0] = $scope.output.LeftNodeGUID;
                    wrt_guids[0] = $scope.output.ParentNodeGUID;

                    pair_index = 3;
                    if ($scope.is_multi) {
                        //node_guids[0] = $scope.output.multi_GUIDs[$scope.active_multi_index][1];
                        //wrt_guids[0] = $scope.output.multi_GUIDs[$scope.active_multi_index][0];
                        //pair_indeces.push(3);
                    }
                    else {
                        pair_indeces.push(pair_index);
                    }

                }
                // alert(index + "-" + params);
                angular.element(".tt-accordion-content").attr("style", "");


                if ($scope.is_multi) {
                    //$scope.output.multi_infodoc_params[$scope.active_multi_index][index] = params;
                    //$scope.multi_collapsed_info_docs[$scope.active_multi_index][index] = is_hidden == 1 ? true : false;
                    // alert(pair_params + "-" + index);
                }
                else {
                    $scope.output.infodoc_params[index] = params;
                    //$scope.collapsed_info_docs[index] = is_hidden == 1 ? true : false;
                }




                //console.log(pair_indeces);
                if (pair_indeces.length > 0 && heading_id == "") {
                    $.each(pair_indeces, function (index, pair_index) {
                        //console.log(node_guids[index]);
                        //console.log(wrt_guids[index]);
                        //console.log(pair_params);
                        //console.log("==============================");
                        $http({
                            method: "POST",
                            url: baseUrl + "pages/Anytime/Anytime.aspx/setInfodocParams",
                            data: {
                                NodeID: node_guids[index],
                                WrtNodeID: wrt_guids[index],
                                value: pair_params,
                                is_multi: $scope.is_multi
                            }
                        }).then(function success(response) {
                            // alert(pair_index + "-" + pair_params);
                            if ($scope.is_multi) {
                                // alert(pair_index);
                                //$scope.output.multi_infodoc_params[$scope.active_multi_index][pair_index] = pair_params;
                                //alert(pair_params + "-" + pair_index);
                                //alert(is_hidden == 1 ? true : false);
                                //$scope.multi_collapsed_info_docs[$scope.active_multi_index][pair_index] = is_hidden == 1 ? true : false;
                            }
                            else {
                                $scope.output.infodoc_params[pair_index] = pair_params;
                                //$scope.collapsed_info_docs[pair_index] = is_hidden ? true : false;
                            }
                        });
                    });

                }

                //console.log($scope.output.infodoc_params);
            }, function error(response) {
                //console.log(response);
                //console.log(response);
            });
        }

        $scope.$on("next_unassessed", function (e) {
            $scope.next_unassessed();
        });
        $scope.next_unassessed = function () {
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/GetNextUnassessed",
                data: {
                    StartingStep: $scope.current_step
                }
            }).then(function success(response) {
                //console.log(response.data.d);
                $scope.$parent.unassessed_data = response.data.d;
                $scope.get_pipe_data(response.data.d[0]);
            }, function error(response) {
                //console.log(response);
            });
        }

        $scope.first_unassessed = function () {
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/GetFirstUnassessed",
                data: {}
            }).then(function success(response) {
                //console.log(response.data.d);
                $scope.get_pipe_data(response.data.d);
            }, function error(response) {
                //console.log(response);
            });
        }

        $scope.qh_text = "";
        $scope.show_apply_qh_cluster = false;
        //get_pipe_data

        var temp_step = 0;

        //Utility funcs//

        $scope.is_float = function (n) {
            return Number(n) === n && n % 1 !== 0;
        }

        $scope.UCFunction = function (x) {
            var XMin = $scope.output.UCData.XMinValue;
            var XMax = $scope.output.UCData.XMaxValue;
            var Decreasing = $scope.output.UCData.Decreasing;
            var Curvature = $scope.output.UCData.Curvature;
            if (XMin >= XMax) { return 0 };
            var n = new Number(0.0);
            if (Decreasing == "false") {
                if (x < XMin) { return 0 };
                if (x > XMax) { return 1 };
                n = (XMax - XMin) * Curvature;
                if (n == 0) { return (x - XMin) / (XMax - XMin) }
                else { return (1 - Math.exp(-(x - XMin) / n)) / (1 - Math.exp(-(XMax - XMin) / n)) }
            }
            else {
                if (x < XMin) { return 1 };
                if (x > XMax) { return 0 };
                n = (XMax - XMin) * Curvature;
                if (n == 0) { return (XMax - x) / (XMax - XMin) }
                else { return (1 - Math.exp(-(XMax - x) / n)) / (1 - Math.exp(-(XMax - XMin) / n)) }
            }
        }

        // Save Utility Curve 
        $scope.save_utility_curve_on_next = function () {
            var value = $scope.isValueChanged ? $scope.utilityCurveInput : $scope.output.UCData.XValue;

            if (value == null || value.toString().length == 0)
                value = -2147483648000;

            $scope.output.UCData.XValue = value;

            var loadingMessage = "";
            $(".next_step").removeClass("back-pulse");

            $http.post(
                $scope.baseurl + "pages/Anytime/Anytime.aspx/SaveUtilityCurve",
                JSON.stringify({
                    step: $scope.current_step,
                    value: value,
                    sComment: $scope.output.comment
                })
            ).then(function success(response) {
                $scope.isValueChanged = false;

                // $scope.$parent.runProgressBar(2, 0, 100, loadingMessage, false);

                // $timeout(function () {
                    // $(".next_step").removeClass("back-pulse");
                //}, 2000);

                // if ($scope.is_comment_updated) {
                    //$scope.is_saving_judgment = true;
                    //$scope.get_pipe_data($scope.current_step);
                // }render

                //$timeout(function () {
                    //$scope.$parent.runProgressBar(2, 100, 100, loadingMessage, true);
                    //$scope.is_comment_updated = false;
                // }, 2000);

                if ($scope.isAutoSaving) {
                    $scope.is_saving_judgment = true;

                    var timeOutValue = $scope.isAutoSaving ? 300 : 0;
                    $timeout(function () {
                        $scope.get_pipe_data($scope.current_step);
                    }, timeOutValue, false);
                }

                $scope.$parent.showLoadingModal = false;
                //console.log(JSON.stringify(response));
            });
        }

        $scope.is_undefined_float = function (value) {
            if (isNaN(parseFloat(value))) {
                return true;
            }
            return false;
        }

        $scope.save_utility_curve = function (value, is_erase) {
            //alert(value);
            if (value != "-") {
                value = (value == null ? -2147483648000 : parseFloat(value));
                is_erase = (value == -2147483648000);

               // alert(value);
                $scope.utilityCurveInput = value;
                $scope.isValueChanged = $scope.output.UCData.XValue != value;

                if (value < $scope.output.UCData.XMinValue) value = $scope.output.UCData.XMinValue;
                if (value > $scope.output.UCData.XMaxValue) value = $scope.output.UCData.XMaxValue;
                //$(".load-canvas-gif").show();
                var loadingMessage = stringResources.yellowLoadingIcon.save;

                $timeout(function () {
                    $scope.$apply(function () {
                        $scope.$parent.disableNext = $scope.output.pipeOptions.dontAllowMissingJudgment && is_erase;
                        $scope.output.IsUndefined = is_erase;
                        $scope.$parent.output.IsUndefined = is_erase;
                    });
                }, 50);

                if (is_erase) {
                    $scope.chart.options.ucpriorityLabel[0].text = "";
                    $scope.utilityCurveInput = "";
                    $scope.chart.update();
                }
                else {
                    var steplabel = getXandYofPriority(value, $scope.chart.data.datasets[0].data);
                    steplabel[0] = ($scope.UCFunction(value) * 100);
                    $scope.chart.options.ucpriorityLabel[0].x = value;
                    $scope.chart.options.ucpriorityLabel[0].y = value >= $scope.output.UCData.XMaxValue ? 0.1 : steplabel[0];
                    $scope.chart.options.ucpriorityLabel[0].reverse = steplabel[1];
                    $scope.chart.options.ucpriorityLabel[0].bottom = steplabel[2];
                    $scope.chart.options.ucpriorityLabel[0].text = (steplabel[0]).toFixed(2) + "%";
                    $scope.chart.update();
                }

                $(".dsBarClr").slider("value", value);
                var $handlePos = $(".tt-utility-curve-wrap .ui-slider-handle").position();
                var $dragHandle = $(".tt-utility-curve-wrap .ui-slider-handle");
                // resizeCurveHandle($dragHandle, $handlePos);

                if (is_erase) {
                    $("#uCurveInput").val("");
                    $scope.set_slider_gray("#sliderCurve");
                    // $("#sliderCurve .ui-slider-handle").fadeIn();
                    //$("#sliderCurve .ui-slider-handle").hide();
                }
                else {
                    $scope.set_slider_blue("#sliderCurve");
                }

                $("#sliderCurve .ui-slider-handle").fadeIn();
                
                if (is_erase) {
                    $(".next_step").removeClass("back-pulse");
                } else {
                    $(".next_step").addClass("back-pulse");
                }

                angular.element(".load-canvas-gif").hide();
            }
        }
        // END OF  Save Utility Curve

        $scope.chart = null;
        var fUCPriorityloaded = false;
        function UCInitPriorityLabel() {
            if (!fUCPriorityloaded) {
                fUCPriorityloaded = true;
                var ucPriorityLabelPlugin = {
                    afterDraw: function (chartInstance) {
                        var yScale = chartInstance.scales["y-axis-0"];
                        var xScale = chartInstance.scales["x-axis-0"];
                        var canvas = chartInstance.chart;
                        var ctx = canvas.ctx;
                        var index;
                        var line;
                        var style;
                        //ctx.shadowColor = "rgba(240, 36, 36, 0.5)"; // string
                        //Color of the shadow;  RGB, RGBA, HSL, HEX, and other inputs are valid.
                        //ctx.shadowOffsetX = 3; // integer
                        //Horizontal distance of the shadow, in relation to the text.
                        //ctx.shadowOffsetY = 3; // integer
                        //Vertical distance of the shadow, in relation to the text.
                        //ctx.shadowBlur = 20; // integer
                        ctx.font = "bold 13pt Arial";
                        if (chartInstance.options.ucpriorityLabel) {
                            for (index = 0; index < chartInstance.options.ucpriorityLabel.length; index++) {
                                line = chartInstance.options.ucpriorityLabel[index];

                                if (!line.style) {
                                    style = "rgb(231, 76, 60)";
                                } else {
                                    style = line.style;
                                }

                                if (line.y) {
                                    yValue = yScale.getPixelForValue(line.y);
                                } else {
                                    yValue = 0;
                                }

                                if (line.x) {
                                    xValue = xScale.getPixelForValue(line.x);
                                } else {
                                    xValue = 0;
                                }

                                ctx.lineWidth = 3;

                                if (yValue) {
                                    //ctx.beginPath();
                                    //ctx.moveTo(0, yValue);
                                    //ctx.lineTo(canvas.width, yValue);
                                    //ctx.strokeStyle = style;
                                    //ctx.stroke();
                                }

                                if (line.text) {
                                    //console.log([xValue,yValue]);

                                    ctx.fillStyle = style;
                                    if (line.reverse) {
                                        if (line.bottom) {
                                            ctx.fillStyle = "black";
                                            ctx.globalAlpha = 0.4;
                                            ctx.fillRect(xValue + 5, yValue + ctx.lineWidth, 70, 30);
                                            ctx.globalAlpha = 1.0;
                                            ctx.fillStyle = style;
                                            ctx.fillText(line.text, xValue + 5, yValue + ctx.lineWidth + 20);
                                        }
                                        else {
                                            ctx.fillStyle = "black";
                                            ctx.globalAlpha = 0.4;
                                            ctx.fillRect(xValue + 5, yValue + ctx.lineWidth - 40, 70, 30);
                                            ctx.globalAlpha = 1.0;
                                            ctx.fillStyle = style;
                                            ctx.fillText(line.text, xValue + 5, yValue + ctx.lineWidth - 20);

                                        }

                                    }
                                    if (!line.reverse) {
                                        if (line.bottom) {
                                            ctx.fillStyle = "black";
                                            ctx.globalAlpha = 0.4;
                                            ctx.fillRect(xValue - 75, yValue + ctx.lineWidth, 70, 30);
                                            ctx.globalAlpha = 1.0;
                                            ctx.fillStyle = style;
                                            ctx.fillText(line.text, xValue - 70, yValue + ctx.lineWidth + 20);

                                        }
                                        else {
                                            ctx.fillStyle = "black";
                                            ctx.globalAlpha = 0.4;
                                            ctx.fillRect(xValue - 75, yValue + ctx.lineWidth - 40, 70, 30);
                                            ctx.globalAlpha = 1.0;
                                            ctx.fillStyle = style;
                                            ctx.fillText(line.text, xValue - 70, yValue + ctx.lineWidth - 20);

                                        }
                                    }


                                    ctx.shadowColor = "white"; // string
                                    //Color of the shadow;  RGB, RGBA, HSL, HEX, and other inputs are valid.
                                    ctx.shadowOffsetX = 0; // integer
                                    //Horizontal distance of the shadow, in relation to the text.
                                    ctx.shadowOffsetY = 0; // integer
                                    //Vertical distance of the shadow, in relation to the text.
                                    ctx.shadowBlur = 0; // integer

                                }


                            }
                            return;
                        };
                    }
                };
                Chart.pluginService.register(ucPriorityLabelPlugin);
            }

        }


       // alert(456);

       $scope.utilityCurveInput = null;
        $scope.render_utility_curve = function () {
           // alert(123);

            $scope.chart != null ? $scope.chart.destroy() : 1 + 1;
            fUCPriorityloaded = false;
          
            //$(".tt-utility-curve-wrap").hide();
            $(".load-canvas-gif").show();
            $scope.$parent.showLoadingModal = false;
            $scope.refreshed = false;
            var XMin = $scope.output.UCData.XMinValue;
            var XMax = $scope.output.UCData.XMaxValue;
            var Decreasing = $scope.output.UCData.Decreasing;

            $scope.isValueChanged = false;
            $scope.utilityCurveInput = (parseFloat($scope.output.UCData.XValue.toFixed(2)));
            var XValue = $scope.utilityCurveInput;

            if ($scope.utilityCurveInput < XMin) XValue = XMin;
            if ($scope.utilityCurveInput > XMax) XValue = XMax;
            //finc x values
            // var x = [];
            //var y = []
            var uc_points = [];

            var Shift = (XMax - XMin) / 10;

            for (var i = 0; i < 11; i++) {
                var x_point = (XMin + i * Shift);
                var y_point = $scope.UCFunction(XMin + i * Shift) * 100;
                //x.push( (XMin + i * Shift).toFixed(2) );
                //y.push( $scope.UCFunction(XMin + i * Shift) * 100 );
                uc_points.push({ x: x_point, y: y_point });
            };
           // alert(123);
            //console.log(uc_points);
            
            var label = $scope.output.child_node + " for " + $scope.output.parent_node;
           

            var canvasData = getXandYofPriority(XValue, uc_points);
            //console.log("============");
            if (canvasData[0] != "") {
                canvasData[0] = ($scope.UCFunction(XValue) * 100);
                canvasData[0] = (canvasData[0]).toFixed(2);
            }

            $timeout(function() {
                if ($scope.utilityCurveInput == -2147483648000) {
                    $("#uCurveInput").val("");
                }
            }, 500);
          

            $timeout(function () {
                var priorityLabel = canvasData[0] + "%";

                if ($scope.utilityCurveInput == -2147483648000) {
                    priorityLabel = "";
                }

                UCInitPriorityLabel();
                 var data = {
                datasets: [{
                    label: label,
                    borderColor: "#0058a3",
                    fill: false,
                    data: uc_points,
                    pointBackgroundColor: "#ecfaff"
                }]
                 };

                var options = {
                    responsive: true,
                    //maintainAspectRatio: false,
                    scales: {
                        xAxes: [{
                            type: "linear",
                            position: "bottom",
                            scaleLabel: {
                                display: true,
                                labelString: "Data"
                            },
                            ticks: {
                                min: XMin,
                                max: XMax,
                                callback: function (label, index, labels) {

                                    if ($scope.is_float(label)) {
                                        label = parseFloat(label).toFixed(2);
                                    }
                                    return label;
                                }
                            }
                        }],
                        yAxes: [{
                            scaleLabel: {
                                display: true,
                                labelString: "Priority"
                            },
                            ticks: {
                                callback: function (label, index, labels) {
                                    if ($scope.is_float(label)) {
                                        label = parseFloat(label).toFixed(2);
                                    }
                                    return label + ".00%";
                                }
                            }
                        }]
                    },
                    ucpriorityLabel: [{
                        "x": XValue,
                        "y": XValue >= $scope.output.UCData.XMaxValue ? 0.1 : canvasData[0],
                        "style": "white",
                        "text": priorityLabel,
                        "reverse": canvasData[1],
                        "bottom": canvasData[2],
                        "glowonce": true
                    }]

                };

                var canvas = document.getElementById("uCurve");
                var ctx = canvas.getContext("2d");
                Chart.defaults.global.legend.display = false; //remove label at the top of graph
                Chart.defaults.global.tooltips.enabled = false;
                var chart = Chart.Line(ctx, {
                    data: data,
                    options: options                    
                });

                

                $scope.chart = chart;
                $scope.chart.update();
                //console.log($scope.chart);
                $timeout(function () {
                    $scope.resize_UC_canvas();
                    $(".tt-utility-curve-wrap .ui-slider-handle").hide();
                }, 100);


                $(".tt-curve-slider").slider("value", XValue);
                //     $("#sliderCurve .ui-slider-handle").           resizeCurveHandle($dragHandle, $handlePos);


                $timeout(function () {
                    if ($scope.utilityCurveInput == -2147483648000) {
                        $("#uCurveInput").val("");
                        priorityLabel = "";
                    }

                    if ($scope.utilityCurveInput < XMin) {
                        $scope.set_slider_gray("#sliderCurve");
                        $("#sliderCurve .ui-slider-handle").addClass("back-pulse");

                        $timeout(function () {
                            $("#sliderCurve .ui-slider-handle").removeClass("back-pulse");
                        }, 1000);
                    }
                    else {
                        $scope.set_slider_blue("#sliderCurve .ui-slider-handle");
                    }

                    $("#sliderCurve .ui-slider-handle").fadeIn();
                    $scope.chart.options.ucpriorityLabel[0].text = priorityLabel;
                    //console.log(canvasData[0] == "" ? + "" : canvasData[0] + " %");
                    $scope.chart.update();

                }, 500);

                $(".load-canvas-gif").hide();
                $(".tt-utility-curve-wrap").show();

            }, 1500);

        }

        $scope.set_slider_gray = function (element) {
            //$(element).css("background", "#b4b6b7");
            //$(element).css("border", "1px solid #b4b6b7");
            $(element).addClass("disabled");
            //$(".tt-content-wrap .tt-body .tt-judgements-item .tt-step-function-wrap .tt-steps-slider.disabled .ui-slider-handle").css("cursor", "pointer");
        }

        $scope.set_slider_blue = function (element) {
            // $(element).css("background", "#0058a3");
            //$(element).css("border", "1px solid #0058a3");
            $(element).removeClass("disabled");
        }

        $scope.addToptosteplist = function () {
            $timeout(function () {
                var top = $(".steps-range-option").height() + 4;
                $(".steps_list_content").css("margin-top", top);
            }, 500);
        }

        $scope.load_step_list = function (first, last) {
            if (!first)
                first = 1;
            if (!last)
                last = $scope.output.total_pipe_steps;
            $scope.$parent.steps_list = null;
            if ($scope.$parent.steps_list == null) {
                $http({
                    method: "POST",
                    url: baseUrl + "pages/Anytime/Anytime.aspx/loadStepList",
                    data: {
                        first: first,
                        last: last
                    },
                }).then(function success(response) {
                    //$scope.$apply(function () {
                    $scope.$parent.steps_list = eval("[" + response.data.d + "]");
                    $scope.$parent.is_pipescreen = $scope.is_anytime || $scope.is_teamtime ? true : false;
                    //$scope.showLoadingModal = false;
                    $scope.$parent.loadSteps = false;
                    $scope.scrolltoelement($scope.output.current_step, 0);
                    //$scope.get_pipe_data(step, is_AT_owner);
                    //console.log(2);
                    //});
                    // $scope.get_pipe_data(step, is_AT_owner);

                }, function error(response) {
                    //console.log(response);
                    // $scope.get_pipe_data(1, is_AT_owner);
                });
            }
        }



        $scope.check_key_for_steplist = function (event, first, last) {
            var code = event ? event.which : null;
            if (code == 13 || event == null) {

                if (first < 1)
                    first = 1;
                if (first > last)
                    last = first + 500;
                if (last - first > 3000)
                    last = first + 3000;
                if (last > $scope.output.total_pipe_steps)
                    last = $scope.output.total_pipe_steps;

                $scope.$parent.stepRange.first = first;

                $scope.$parent.stepRange.last = last;


                $scope.load_step_list(first, last);
                if (event!= null)
                    return event.preventDefault();
            }
            else {
                return event.preventDefault();
            }
        }
        //End of Utility funcs//

        $scope.refresh_output = function (step, index) {
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/GetDataOfPipeStep",
                data: {
                    step: step
                },
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                }
            }).then(function success(response) {
                $timeout(function () {
                    $scope.$apply(function () {


                        try {
                            $scope.setoutput = JSON.parse(response.data.d);
                            $scope.$parent.output = $scope.output;
                        }
                        catch (e) {
                            $scope.output = JSON.parse(response.data.d);
                            $scope.$parent.output = $scope.output;

                        }
                        var temp_step_task = $scope.output.step_task;
                        $scope.output.step_task = temp_step_task;
                        $scope.multi_data = $scope.output.multi_pw_data;
                        $scope.temp_multi_data = $scope.multi_data;
                        //if ($scope.output.page_type != "atShowLocalResults" && $scope.output.page_type != "atShowGlobalResults") {
                            $scope.output.step_task = $scope.output.step_task.replace(/<[\/]{0,1}(p)[^><]*>/ig, "");
                            $scope.output.step_task += '<a data-node-type="0" data-location="0"  data-node="question-node" class="edit-info-doc-btn edit-pencil" title="" href="#"><span class="icon-tt-pencil"></span></a>';
                       // }
                        var st = "<div>" + $scope.output.step_task + "</div>";

                        // Here is the correct patter
                        $scope.output.step_task = $scope.format_step_task(st);

                        //if ($scope.is_multi) {
                        //    $scope.set_multi_index(index); //if next step is clicked, set row to 0
                        //}
                    });
                }, 1000);

            });
        }

        $scope.format_step_task = function (s) {
            var $s = $(s);
            var $elements = $s.find("*").not("span,a,em,strong");
            for (var i = $elements.length - 1; i >= 0; i--) {
                var e = $elements[i];
                $(e).replaceWith(e.innerHTML);
            }
            return $s.html();
        }


        $scope.formatted_comment = "";
        $scope.multi_comments = [];
        var newScope;
        var ratingsDropdowOpen = true;
        $scope.loader_counter = 0;
        $scope.infodocSizes = null;

        $scope.initializeAnytime = function(step, isAtOwner) {
            $scope.checkProjectLockStatus();
            $scope.getAllSinglePairwiseInfoDocs();

            $timeout(function() {
                $scope.get_pipe_data(step, isAtOwner);
                $scope.save_multis();
            }, 800, false);
        }

        $scope.get_pipe_data = function (step, is_AT_owner, loading_message, multi_index) {
            $scope.is_judgment_screen = true;
            $scope.temp_comments = [];
            $scope.intermediate_screen = 0;
            $scope.infodocSizes = null;
            var loading_message = stringResources.loadingModalMessage.default;
            var loadingType = 1;
            //if (!$scope.is_saving_judgment) {
            //    //loading Modal
            //    loadingType = 1;
            //}
            //else {
            //    //loading Icon
            //    loadingType = 2;
            //}
            //alert(loading_message);
            //70% max finish when ajax is not yet called
            
            //$scope.$parent.runProgressBar(loadingType, 0, 70, loading_message);

            $scope.hideKoHelpPage();
            $scope.checkProjectLockStatus();

            angular.element(".tg-aa-wrap").hide();
            $scope.fparentNotHidden = false;
            $scope.$parent.is_anytime = true;
            $scope.$parent.is_teamtime = false;
            is_anytime = true;
            current_step = step;
            //var is_previous = is_prev;

            $scope.QuestionStep = 0;


            var stip = $scope.current_step;
            var PrevParentNodeID = "";
            if ($scope.output != null) {
                PrevParentNodeID = $scope.output.ParentNodeID;
                if ($scope.output.page_type == "atSpyronSurvey" && $scope.SurveyAnswers.length > 0) {
                    return $scope.save_respondentAnswer(stip, step);
                }
            }

            //consoleLog("get_pipe_data. current_step: " + $scope.current_step + ", step: " + step);
            $scope.current_step = step;
            current_step = step;


            if (step <= 0) {
                step = 1;
            }
            /////console.log(step);


            $scope.is_saving_judgment = false;
            if ($scope.is_multi && !$scope.isAutoSaving) {
                $scope.output = null; //case 11534
                outerIndex = -1;
            }

            
            var move_message = $scope.current_step == step && $scope.isAutoSaving ? "Saving ..." : "Loading ...";
            
            if ($scope.isAutoSaving) {
                $timeout(function() {
                    $scope.isAutoSaving = false;
                }, 500, false);
            }

            var showLoadingMessage = (typeof $scope.showLoadingMessage == "undefined" ? true : $scope.$parent.showLoadingMessage);

            if ($scope.loader_counter == 0 && showLoadingMessage) { // first load or refreshed
                LoadingScreen.init(LoadingScreen.type.loadingModal, $(".LoadingModalPercentage"), $(".fullwidth-loading-wrap"));
                LoadingScreen.start(80, loading_message);
            }
            else if (showLoadingMessage) {
                LoadingScreen.init(LoadingScreen.type.loadingModal, $(".LoadingModalPercentage"), $(".small-fullwidth-loading-wrap"));
                LoadingScreen.start(80, move_message);
            }

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/GetDataOfPipeStep",
                data: {
                    step: step,
                },
                async: false,
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                }
            }).then(function success(response) {

               
                //$scope.$parent.runProgressBar(loadingType, 70, 100, loading_message);

                if (!response.data.d) {
                    $scope.$parent.block_unload_prompt = true;
                    console.log("block_unload_prompt changed to true #1702");

                    //CR case 11322
                    $scope.output.page_type = "atTeamTime";
                  
                    //$scope.$parent.runProgressBar(loadingType, 100, 100, '', true);


                    //$window.location.href = '/pages/TeamTimeTest/teamtime.aspx';   //redirect to teamtime if running
                    return false;
                }

                $scope.$parent.block_unload_prompt = false;
                $scope.output = JSON.parse(response.data.d);
                if ($scope.loader_counter == 0) { // first load or refreshed
                   
                    var project_details = "Loading model details..." + "<br/>";
                    try{
                        project_details += "Name:" + $scope.output.name.substring(0, 50) + "...<br/>";
                    }
                    catch(e){

                    }
                    //project_details += "Owned by:" + $scope.output.owner + "<br/>";
                    //project_details += "Passcode:" + $scope.output.passcode + "<br/>";

                    if (showLoadingMessage) {
                        LoadingScreen.start(80);
                        LoadingScreen.end(100, false);
                        $scope.loader_counter++;
                    }
                }
                else {
                    //70% to 100% when ajax call is done

                    if (showLoadingMessage) {
                        LoadingScreen.start(80, move_message);
                        LoadingScreen.end(100, false, move_message);
                        $scope.loader_counter++;
                    }
                }

                $scope.refreshed = false;
                $(document).foundation();
                $("#tt-s-modal").foundation("reveal", "close");

                        if (typeof (is_AT_owner) == "undefined" || is_AT_owner == null) {
                            $scope.is_AT_owner = $scope.is_AT_owner;
                        }
                        else {
                            $scope.is_AT_owner = is_AT_owner;
                        }
                        $scope.$parent.is_AT_owner = $scope.is_AT_owner;
                        $scope.$parent.is_AT_owner_temp = $scope.is_AT_owner == 1 ? true : false;
                       // $scope.$parent.orig_is_AT_owner = $scope.is_AT_owner;
                        try {
                            $scope.output = JSON.parse(response.data.d);


                            if (typeof ($scope.output.no_footer_options) == "undefined") {
                                $scope.output.no_footer_options = 0;
                            }
                            else {
                                $scope.output.no_footer_options = 0;
                            }

                            $scope.output.orig_collapse_bars = $scope.output.collapse_bars;
                            if ($scope.isMobile()) {
                                $scope.output.collapse_bars = $scope.output.orig_collapse_bars;
                            }
                            else {
                                $scope.output.collapse_bars = false;
                            }
                            // alert($scope.output.collapse_bars);
                            $scope.$parent.output = $scope.output;
                        }
                        catch (e) {
                            $scope.output = JSON.parse(response.data.d);
                            $scope.$parent.output = $scope.output;

                        }

                        $scope.$parent.autoFitInfoDocImages = $scope.output.autoFitInfoDocImages;
                        $scope.$parent.autoFitInfoDocImagesOptionText = $scope.output.autoFitInfoDocImagesOptionText;

                        $scope.$parent.framed_info_docs = $scope.output.framed_info_docs;

                        if ($scope.output.status == "timeout") {
                            $timeout(function () {
                                if (!angular.isDefined($scope.isLogOutClicked)) {
                                    //$scope.$parent.block_unload_prompt = true;
                                    //console.log("block_unload_prompt changed to true #1788");
                                    window.onbeforeunload = null;
                                    alert($scope.output.message);
                                    window.location.href = baseUrl;
                                }
                            }, 100);
                        }

                        //code to set the pipe content
                        var content = checkLoadedScreens($scope.output.page_type, $scope.output.non_pw_type, $scope.output.userControlContent);

                        if (newScope != null) {
                            newScope.$destroy();
                        }
                        newScope = $scope.$new();
                        angular.element("#pipeContent").html($compile(content)(newScope));
                        

                       // //console.log($scope.output.is_infodoc_tooltip);
                        //load step buttons
                        $scope.$parent.stepButtons = eval("[" + $scope.output.stepButtons + "]");
                        //console.log(JSON.stringify($scope.$parent.stepButtons));
                        $scope.$parent.orig_infodoc_setting = $scope.output.is_infodoc_tooltip;
                        cluster_phrase = $scope.output.cluster_phrase;
                        sRes = $scope.output.sRes;
                        $scope.qh_text = $scope.output.qh_info;
                        $scope.$parent.qh_text = $scope.qh_text;
                        nodes_data = $scope.output.nodes_data;
                        //if ($scope.output.steps != "") {
                        //    $scope.steps_list[$scope.output.previous_step - 1] = eval($scope.output.steps);
                        //}
                        var temp_step_task = $scope.output.step_task;
                        //$scope.output.page_type != "atShowLocalResults" && $scope.output.page_type != "atShowGlobalResults" &&
                        if ($scope.output.page_type != "atSensitivityAnalysis") {
                            if ($scope.output.page_type != "atShowLocalResults" && $scope.output.page_type != "atShowGlobalResults") {
                                $scope.output.step_task = temp_step_task.replace(/<[\/]{0,1}(p)[^><]*>/ig, "");
                            }
                            if ($scope.is_AT_owner == 1) {
                                $scope.output.step_task += '<a data-node-type="0" data-location="0"  data-node="question-node" class="edit-info-doc-btn edit-pencil" title="" href="#"><span class="icon-tt-pencil"></span></a>';
                            }

                        }

                        var step_task = "<div>" + $scope.output.step_task + "</div>";

                        // Here is the correct pattern
                        if ($scope.output.page_type != "atShowLocalResults" && $scope.output.page_type != "atShowGlobalResults") {
                            $scope.output.step_task = $scope.format_step_task(step_task);
                        }
                        else {
                            $scope.output.step_task = step_task;
                        }

                        try {
                            if ($scope.$parent.getHtml($scope.output.qh_info) != "") {
                                var tmp_qh_str = $scope.output.qh_info;
                                if (tmp_qh_str.indexOf("img") >= 0) {
                                    //if has image
                                    //$scope.qh_text = $scope.$parent.getHtml($scope.output.qh_info);
                                }
                                else {
                                    $scope.qh_text = $("'" + $scope.$parent.getHtml($scope.output.qh_info) + "'").text().trim();
                                }
                            }
                            else {
                                $scope.qh_text = $scope.output.qh_info;
                            }
                            $scope.$parent.qh_text = $scope.qh_text;
                        }
                        catch (e) {

                        }
                        quick_help = $scope.output.qh_yt_info;
                        var quick_help_temp = $scope.qh_text;
                        if (quick_help_temp != "") {
                            if (!$scope.output.show_qh_automatically) {
                                $(".qh-icons").addClass("change-color-pulse");
                            }

                        }

                        if ($scope.output.page_type == "atInformationPage") {
                            $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);
                            $scope.is_judgment_screen = false;

                            //inner div resize
                            $timeout(function () {
                                $scope.CheckMultiHeight("tt-homepage-main-wrap", "40");//resize homepage
                                //                            $scope.CheckMultiHeight("multi-loop-wrap", "150");//resize multipairwise
                                //$scope.CheckMultiHeight("tt-rating-scale", "100");
                            }, 800);

                        }


                        if ($scope.output.page_type == "atNonPWOneAtATime" && $scope.output.non_pw_type == "mtRatings") {
                            $scope.get_ratings_data();
                            $scope.is_multi = false;
                            $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;

                            if ($scope.output.is_direct && $scope.output.non_pw_value >= 0) {
                                $scope.output.non_pw_value = parseFloat($scope.output.non_pw_value);
                                $scope.singleRatingTempDirectValue = parseFloat($scope.output.non_pw_value);
                            }
                        }

                        if ($scope.output.page_type == "atNonPWOneAtATime" && $scope.output.non_pw_type == "mtDirect") {
                            $scope.get_direct_data();
                            $scope.is_multi = false;
                            $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;
                        }

                        if ($scope.output == null) {
                            if (!(step == 1)) {
                                alert("Something went wrong. You will be moved back to the previous step");
                                $scope.get_pipe_data(stip);
                                return null;
                            }
                            else {
                                alert("Something went wrong. Please try again");
                            }
                            window.location.href = baseUrl;
                        }
                        else {
                            $scope.information_message = $scope.output.information_message;
                        }

                        try {
                            if ($scope.output.step_task.indexOf("(!)") >= 0 || $scope.output.step_task == "") {
                                $scope.output.step_task = "which of the two <span class='question'><b>" + $scope.output.question + "</b></span> <span class='wording task_bold'><b>" + $scope.output.wording + "</b></span>?";
                            }
                        }
                        catch (e) {

                        }

                        $scope.$parent.is_result_screen = false;
                        //insert what will happen to each screens below
                        if ($scope.output.page_type == "atShowLocalResults") {
                            $scope.reflow_equalizer();
                            // case 11262 - hide/show expected value option if applicable
                            $scope.$parent.pipeOptions.showIndex = $scope.output.PipeParameters.showIndex;
                            $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;
                            $scope.$parent.is_result_screen = true;
                            $scope.is_judgment_screen = false;
                            $scope.results_data = $scope.output.PipeParameters.results_data;
                            angular.forEach($scope.results_data, function (result) {
                                result[0] = parseFloat(result[0]);
                            });

                            if ($scope.results_data.length > 0) {
                                switch ($scope.results_data[0][4]) {
                                    case "rvBoth":
                                        $scope.columns = [["nodeID", "Index"], ["nodeName", "Name"], ["yourResults", "Your Results"], ["combine", "Combined"]];
                                        break;
                                    case "rvGroup":
                                        $scope.columns = [["nodeID", "Index"], ["nodeName", "Name"], ["combine", "Combined"]];
                                        break;
                                    case "rvIndividual":
                                        $scope.columns = [["nodeID", "Index"], ["nodeName", "Name"], ["yourResults", "Your Results"]];
                                        break;
                                }
                            }
                            
                            $scope.columnname = "this[" + $scope.output.PipeParameters.defaultsort + "]";
                            $scope.currentColumn = undefined;

                            $scope.output.PipeParameters.StepPairsData = eval($scope.output.PipeParameters.StepPairsData);
                            $scope.output.PipeParameters.ObjectivesData = eval($scope.output.PipeParameters.ObjectivesData);
                            $scope.$parent.showHiddenValue();
                            $scope.$parent.getShowExpectedValue();
                            
                            $scope.$parent.resultsreverse = $scope.output.PipeParameters.defaultsort < 2 ? false : true;
                            if ($scope.reverseLocal == null)
                                $scope.reverseLocal = $scope.$parent.resultsreverse;
                            //console.log($scope.columnsortLocal);
                            if ($scope.columnsortLocal== null)
                                $scope.columnsortLocal = $scope.columns[$scope.output.PipeParameters.defaultsort] != null ? $scope.columns[$scope.output.PipeParameters.defaultsort] : $scope.columns[0];
                            
                            if ($scope.matrixbox.legend == null)
                                $scope.matrixbox.legend = true;
                            if ($scope.output.PipeParameters.MeasurementType == "ptGraphical") {
                                $scope.matrixbox.legend = false;
                            }

                            if ($scope.output.ParentNodeID !== PrevParentNodeID && !$scope.matrixReview) {
                                $scope.restoreDefaultView();
                            }

                            if (stip !== step && $scope.$parent.is_under_review === false && !$scope.matrixReview) {
                                $scope.restoreDefaultView();
                                stip = step;
                            }
                            $scope.isRedNumber = false;

                            $scope.$parent.is_under_review = false;

                            //console.log($scope.output.PipeParameters.JudgmentType);
                            if (["atPairwise", "atAllPairwise"].indexOf($scope.output.PipeParameters.JudgmentType) > -1)
                                getMatrixTemplate();

                            $scope.normalization_Change($scope.normalization, false);
                        }

                        if ($scope.output.page_type == "atShowGlobalResults") {
                            $scope.$parent.pipeOptions.showIndex = $scope.output.PipeParameters.showIndex;
                            $scope.wrt_node_id = $scope.output.PipeParameters.WrtNodeID;
                            $scope.$parent.wrt_id = $scope.output.PipeParameters.WrtNodeID;
                            $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;
                            $scope.is_judgment_screen = false;
                            $scope.canshowresult = $scope.output.PipeParameters.canshowresult;
                            $scope.$parent.is_result_screen = true;
                            $scope.results_data = $scope.output.PipeParameters.results_data;

                            angular.forEach($scope.results_data, function (result) {
                                result[0] = parseFloat(result[0]);
                            });
                            $timeout(function () {
                                $scope.wrt_node_name = $scope.output.PipeParameters.WrtNodeName;
                            }, 200);

                            if ($scope.results_data.length > 0) {
                                switch ($scope.results_data[0][4]) {
                                    case "rvBoth":
                                        $scope.columns = [["nodeID", "Index"], ["nodeName", "Name"], ["yourResults", "Your Results"], ["combine", "Combined"]];
                                        break;
                                    case "rvGroup":
                                        $scope.columns = [["nodeID", "Index"], ["nodeName", "Name"], ["combine", "Combined"]];
                                        break;
                                    case "rvIndividual":
                                        $scope.columns = [["nodeID", "Index"], ["nodeName", "Name"], ["yourResults", "Your Results"]];
                                        break;
                                }
                            }

                            $scope.columnname = "this[" + $scope.output.PipeParameters.defaultsort + "]";
                            $scope.currentColumn = undefined;

                            $scope.$parent.resultsreverse = $scope.output.PipeParameters.defaultsort < 2 ? false : true;
                            if ($scope.reverseGlobal == null)
                                $scope.reverseGlobal = $scope.$parent.resultsreverse;

                            if ($scope.columnsortGlobal == null)
                                $scope.columnsortGlobal = $scope.columns[$scope.output.PipeParameters.defaultsort] != null ? $scope.columns[$scope.output.PipeParameters.defaultsort] : $scope.columns[0];

                            $scope.$parent.showHiddenValue();
                            $scope.$parent.getShowExpectedValue();

                            if ($scope.output.sess_wrt_node_id != 1) {
                                $scope.save_WRTNodeID($scope.output.sess_wrt_node_id);
                            }

                            if (showLoadingMessage) {
                                LoadingScreen.end(100, true);
                            }

                            $scope.$parent.load_hierarchies(true);

                        }
                        $scope.$parent.disableNext = false;
                        if (($scope.output.page_type == "atNonPWAllCovObjs" || $scope.output.page_type == "atNonPWAllChildren") && $scope.output.non_pw_type == "mtRatings") {
                            $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;
                            for (var j = 0; j < $scope.output.multi_non_pw_data.length; j++) {
                                $scope.multi_ratings_row[j] = $scope.get_multi_ratings_data($scope.output.multi_non_pw_data[j].RatingID, $scope.output.multi_non_pw_data[j].DirectData, j);
                                $scope.multi_direct_value[j] = $scope.multi_ratings_row[j][0] != -1 ? $scope.multi_ratings_row[j][0] : $scope.multi_direct_value[j];

                            }
                            $scope.is_multi = true;
                            //$scope.select_multi_ratings_row(0);
                            if ($scope.output.pipeOptions.dontAllowMissingJudgment && $scope.output.IsUndefined) {
                                $scope.$parent.disableNext = true;
                            }

                        }

                        if (($scope.output.page_type == "atNonPWAllCovObjs" || $scope.output.page_type == "atNonPWAllChildren") && $scope.output.non_pw_type == "mtDirect") {
                            $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;
                            $scope.is_multi = true;
                            if ($scope.output.pipeOptions.dontAllowMissingJudgment && $scope.output.IsUndefined) {
                                $scope.$parent.disableNext = true;
                            }
                        }

                        //for case 13171: Anytime Comments
                        var comment = $scope.output.comment;
                        if ($scope.output.comment.indexOf("@@") != -1) {
                            $scope.output.comment = $scope.output.comment.split("@@");
                            $scope.output.comment = $scope.output.comment[0];
                        }
                        var multi_data;
                        if ($scope.output.page_type == "atAllPairwise") {
                            multi_data = $scope.output.multi_pw_data;
                            for (i = 0; i < multi_data.length; i++) {
                                var com = multi_data[i].Comment;
                                if (com.indexOf("@@") != -1) {
                                    $scope.output.multi_pw_data[i].Comment = com.split("@@");
                                    $scope.output.multi_pw_data[i].Comment = $scope.output.multi_pw_data[i].Comment[0];
                                }
                            }
                        }
                        if ($scope.output.page_type == "atNonPWAllChildren") {
                            multi_data = $scope.output.multi_non_pw_data;
                            for (i = 0; i < multi_data.length; i++) {
                                var com = multi_data[i].Comment;
                                if (com.indexOf("@@") != -1) {
                                    $scope.output.multi_non_pw_data[i].Comment = com.split("@@");
                                    $scope.output.multi_non_pw_data[i].Comment = $scope.output.multi_non_pw_data[i].Comment[0];
                                }
                            }
                        }
                        switch ($scope.output.page_type) {
                            case "atPairwise":
                                var comment = $scope.output.comment;
                                $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;
                                $scope.reflow_equalizer();
                                $scope.is_multi = false;
                                switch ($scope.output.pairwise_type) {
                                    case "ptVerbal":
                                        //  $scope.get_infodoc_params(output.ParentNodeGUID, null);
                                        $timeout(function () {
                                            $scope.reflow_equalizer();
                                        }, 500);
                                        break;

                                    case "ptGraphical":
                                        if ($scope.output.advantage == -1) {
                                            $scope.graphical_slider[1] = (1600 * (($scope.output.value) / (($scope.output.value) + 1)));
                                            $scope.graphical_slider[0] = 1600 - $scope.graphical_slider[1];
                                            $scope.numericalSlider[0] = (800 * (($scope.output.value) / (($scope.output.value) + 1)));


                                        }
                                        if ($scope.output.advantage == 1) {
                                            $scope.graphical_slider[0] = (1600 * ((($scope.output.value)) / (($scope.output.value) + 1)));
                                            $scope.graphical_slider[1] = 1600 - $scope.graphical_slider[0];
                                            $scope.numericalSlider[0] = 800 - (800 * (($scope.output.value) / (($scope.output.value) + 1)));
                                        }

                                        $scope.main_bar[0] = $scope.output.value > 0 ? parseFloat(($scope.output.advantage == 1 ? ($scope.output.value) * 1 : 1).toFixed(2)) : "";
                                        $scope.main_bar[1] = $scope.output.value > 0 ? parseFloat(($scope.output.advantage == -1 ? ($scope.output.value) * 1 : 1).toFixed(2)) : "";
                                        $scope.main_bar_txt[0] = ($scope.output.advantage == 1 ? ($scope.output.value) * 1 : 1).toFixed(2);
                                        $scope.main_bar_txt[1] = ($scope.output.advantage == -1 ? ($scope.output.value) * 1 : 1).toFixed(2);
                                        if ($scope.output.advantage == 0) {
                                            $scope.graphical_slider[0] = 800;
                                            $scope.graphical_slider[1] = 1600 - $scope.graphical_slider[0];
                                            $scope.numericalSlider[0] = 400;
                                        }

                                        if ($scope.getNoUiSlider() != null && $scope.output.advantage && $scope.page_load) {
                                            $scope.noUiTimeOut = $timeout(function () { $scope.setNoUiSlider($scope.numericalSlider[0], $scope.numericalSlider[0] + 800, -1); }, 100);
                                            $scope.page_load = false;
                                        }
                                        if ($scope.getNoUiSlider() != null && $scope.output.advantage == 0 && $scope.output.value == 1) {
                                            $scope.noUiTimeOut = $timeout(function () {
                                                $scope.setNoUiSlider(400, 1200, -1);
                                                $(".noUi-connect").css("background-color", "#0058a3");
                                                $(".graph-green-div").css("background-color", "#6aa84f");
                                            }, 100);
                                        }



                                        $scope.initialize_Pie();

                                        break;
                                }
                                if ($scope.output.pipeOptions.dontAllowMissingJudgment && $scope.output.IsUndefined) {
                                    $scope.$parent.disableNext = true;
                                }
                                break;
                            case "atAllPairwise":
                                $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;
                                $scope.is_multi = true;
                                //$scope.active_multi_index = 0;
                                switch ($scope.output.pairwise_type) {
                                    case "ptVerbal":
                                        var is_set = false;
                                        $scope.multi_data = $scope.output.multi_pw_data;
                                        $scope.temp_multi_data = $scope.multi_data;
                                        if (!$scope.$parent.is_under_review) {
                                            for (i = 0; i < $scope.multi_data.length; i++) {
                                                //console.log("multi_data[" + i + "]: " + $scope.multi_data[i].Value);
                                                if ($scope.multi_data[i].Value == -2147483648000) { //undefined
                                                    $scope.set_multi_index(i);
                                                    is_set = true;
                                                    break;
                                                }
                                            }
                                            if (is_set == false) $scope.set_multi_index(0);
                                        }
                                        //console.log("ptVerbal -> active_multi_index: " + $scope.active_multi_index);
                                        break;
                                    case "ptGraphical":
                                        snapValues = [];
                                        sliderLeft = [];
                                        sliderRight = [];
                                        var is_set = false;
                                        var max = $(".noUi-base").width();
                                        var zero = (max / 2);
                                        $(".graph-green-div").width(zero - 4);
                                        var count = $scope.output.multi_pw_data.length;
                                        $scope.multi_data = $scope.output.multi_pw_data;
                                        $scope.temp_multi_data = $scope.multi_data;
                                        $scope.oldSliderValue = [];
                                        var counter = 0;

                                        for (i = 0; i < count; i++) {
                                            if ($scope.multi_data[i].Advantage == -1) {
                                                var reverse = (800 * (($scope.multi_data[i].Value) / (($scope.multi_data[i].Value) + 1)));
                                                $scope.participant_slider[i] = reverse;

                                            }
                                            if ($scope.multi_data[i].Advantage == 1) {
                                                $scope.participant_slider[i] = 800 - (800 * (($scope.multi_data[i].Value) / (($scope.multi_data[i].Value) + 1)));
                                            }
                                            if ($scope.multi_data[i].Advantage == 0) {
                                                $scope.participant_slider[i] = 400;

                                            }
                                            $scope.left_input[i] = parseFloat(($scope.multi_data[i].Advantage > 0 ? ($scope.multi_data[i].Value) * 1 : 1).toFixed(2));
                                            $scope.right_input[i] = parseFloat(($scope.multi_data[i].Advantage < 0 ? ($scope.multi_data[i].Value) * 1 : 1).toFixed(2));
                                            //if ($scope.multi_data[i].Advantage == 0 && $scope.multi_data[i].Value == 1) {
                                            //    $scope.left_input[i] = 1;
                                            //    $scope.right_input[i] = 1;
                                            //}
                                            if ($scope.multi_data[i].Value <= 0) {
                                                $scope.left_input[i] = "";
                                                $scope.right_input[i] = "";
                                            }
                                            
                                            if ($scope.multi_data[i].Value == -2147483648000) { //undefined
                                                if (counter == 0) {
                                                    if (!$scope.$parent.is_under_review) {
                                                        $scope.active_multi_index = i;
                                                    }
                                                }
                                                is_set = true;
                                                counter++;
                                            }


                                        }
                                        if (is_set)
                                            $scope.set_multi_index($scope.active_multi_index);
                                        else
                                            $scope.set_multi_index(0);
                                        //$timeout(function () {
                                        //    $(".graph-green-div").addClass("others");
                                        //    $("#gslider" + $scope.active_multi_index + " .graph-green-div").removeClass("others");
                                        //    var max = $(".noUi-base").width();
                                        //    var zero = (max / 2);
                                        //    $(".graph-green-div.others").width(zero);
                                        //    var uibaseWidth = $("#gslider" + $scope.active_multi_index).width();
                                        //    $("#gslider" + $scope.active_multi_index + " .graph-green-div").width((uibaseWidth / 2));
                                        //    $(".graph-green-div").width((uibaseWidth / 2));
                                        //}, 1000);
                                        
                                        //console.log("ptGraphical -> active_multi_index: " + $scope.active_multi_index);
                                        break;
                                      

                                }
                                if ($scope.output.pipeOptions.dontAllowMissingJudgment && $scope.output.IsUndefined) {
                                    $scope.$parent.disableNext = true;
                                }
                               // GetMultiPW();
                                break;
                            case "atNonPWOneAtATime":
                                $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;
                                $scope.is_multi = false;
                                if ($scope.output.pipeOptions.dontAllowMissingJudgment && $scope.output.IsUndefined) {
                                    $scope.$parent.disableNext = true;
                                }
                                switch ($scope.output.non_pw_type) {
                                    case "mtRegularUtilityCurve":
                                    case "mtAdvancedUtilityCurve":
                                        $scope.stepChart = null;
                                        $scope.render_utility_curve();

                                        break;
                                    case "mtStep":
                                        $scope.chart = null;
                                        $(window).load();
                                        //jquery_step = $scope.output.current_step;

                                        //so $(window) could load first
                                        $timeout(function () {
                                            var minE = $scope.output.PipeParameters.min;
                                            var maxE = $scope.output.PipeParameters.max;
                                            var stepX = $scope.output.step_intervals[0];
                                            var stepY = $scope.output.step_intervals[1];

                                            render_step_function(minE, maxE, stepX, stepY);
                                            updateSliderSteps(0, minE, maxE);
                                            $dragHandle = $(".tt-steps-slider .ui-slider-handle");
                                            $timeout(function () {

                                                if ($scope.output.current_value < $scope.lowest_value) {
                                                    $("#steps-functionInput").val("");
                                                    $scope.set_slider_gray(".tt-steps-slider");
                                                    $(".tt-steps-slider .ui-slider-handle").addClass("back-pulse");
                                                    $timeout(function () {
                                                        $(".tt-steps-slider .ui-slider-handle").removeClass("back-pulse");
                                                    },
                                                    1500);
                                                    $("#stepsFunctionSlider").slider("option", "value", minE);
                                                }
                                                if ($scope.output.current_value > $scope.lowest_value) {
                                                    //resizeStepFunctionHandle($dragHandle);
                                                    $scope.set_slider_blue(".tt-steps-slider");
                                                    $("#stepsFunctionSlider").slider("option", "value", $scope.step_input);
                                                    $("#steps-functionInput").val($scope.step_input);
                                                }
                                                $(".tt-steps-slider .ui-slider-handle").fadeIn();
                                                var $handlePos = $(".tt-steps-slider .ui-slider-handle").position();
                                                var $dragHandle = $(".tt-steps-slider .ui-slider-handle");
                                                resizeStepFunctionHandle($dragHandle, $handlePos);


                                                $scope.execute_onresize();

                                            }, 1000);
                                        }, 400);

                                        $scope.step_input = "";
                                        $scope.isValueChanged = false;
                                        if ($scope.output.current_value > $scope.lowest_value) {
                                            $scope.step_input = (parseFloat($scope.output.current_value.toFixed(2)));
                                        }

                                        break;
                                }
                                break;
                            case "atSpyronSurvey":
                                $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;
                                $scope.SurveyPage = $scope.output.PipeParameters.SurveyPage;
                                $scope.SurveyAnswers = $scope.output.PipeParameters.SurveyAnswers;
                                $scope.QuestionNumbering = $scope.output.PipeParameters.QuestionNumbering;
                                $scope.output.PipeParameters.alternativelist = eval($scope.output.PipeParameters.alternativelist);
                                $scope.output.PipeParameters.objectivelist = eval($scope.output.PipeParameters.objectivelist);
                                $scope.QuestionStart = 0;
                                $scope.QuestionLength = $scope.SurveyPage.Questions.length;

                                $scope.changeNextButtonStatusForSurvey();
                                break;
                            case "atSensitivityAnalysis":
                                $scope.show_apply_qh_cluster = !$scope.is_html_empty($scope.output.defaultQhInfo);;
                                $http({
                                    method: "POST",
                                    url: baseUrl + "pages/Anytime/Anytime.aspx/initializeSensitivity",
                                    data: {}
                                }).then(function success(response) {
                                    $scope.Sense = JSON.parse(response.data.d);
                                    //alert($scope.Sense.GetGSASubobjectives);
                                    $scope.$parent.SASensitivity = $scope.Sense;
                                    $timeout(function () {
                                        initData();
                                        initPage();
                                        $("#DSACanvas").sa("resetSA");


                                    }, 500);


                                }, function error(response) {
                                    //console.log(response);
                                });
                                //$(".mobile-header").slideUp();
                                break;
                        }
                        if ($scope.is_multi) {

                            //if not being reevaluated
                            if (!$scope.$parent.is_under_review) {
                                if (typeof (multi_index) == "undefined") {
                                    $scope.active_multi_index = $scope.active_multi_index;
                                }
                                else {
                                    $scope.active_multi_index = multi_index;
                                }
                            }

                            if (($scope.output.page_type == "atNonPWAllCovObjs" || $scope.output.page_type == "atNonPWAllChildren") && $scope.output.non_pw_type == "mtRatings") {
                                //$scope.select_multi_ratings_row($scope.active_multi_index);
                                //Ashifur loop data here, and pass the index with undefined judgment multi ratings
                                // //console.log($scope.output.multi_non_pw_data);
                                var has_undefined = false;
                                for (i = 0; i < $scope.output.multi_non_pw_data.length; i++) {
                                    if ($scope.output.multi_non_pw_data[i].RatingID == -1) { //undefined
                                       // alert($scope.output.multi_non_pw_data[i].RatingID);
                                        $scope.active_multi_index = i;
                                        has_undefined = true;
                                        break;
                                    }
                                }

                                if (!has_undefined) {
                                    $scope.active_multi_index = 0 ;
                                }
                                //  alert($scope.active_multi_index);
                                //console.log("line# 2357");

                                var timeOutMs = $scope.getIEVersion() >= 12 ? 1000 : 3000;
                                $timeout(function() {
                                    $scope.select_multi_ratings_row($scope.active_multi_index, true);
                                }, timeOutMs);
                            }

                            if (($scope.output.page_type == "atNonPWAllCovObjs" || $scope.output.page_type == "atNonPWAllChildren") && $scope.output.non_pw_type == "mtDirect") {
                                //Ashifur: loop the data here first  $scope.output.multi_non_pw_data then pass the index in  $scope.get_multi_direct_data({index here});
                                $scope.active_multi_index = 0;
                                for (i = 0; i < $scope.output.multi_non_pw_data.length; i++) {
                                    if ($scope.output.multi_non_pw_data[i].DirectData == -1) { //undefined
                                        $scope.active_multi_index = i;
                                        break;
                                    }
                                }

                                $scope.get_multi_direct_data($scope.active_multi_index);
                                $scope.move_to_next_row($scope.active_multi_index - 1);
                            }

                            //if not being reevaluated
                            if (!$scope.$parent.is_under_review) {
                                $scope.set_multi_index($scope.active_multi_index);
                            }
                            else {
                                //if being reevaluated in multi
                                $timeout(function () {
                                    $("html, body")
                                        .animate({ scrollTop: $("#multi-row-" + $scope.active_multi_index).offset().top - 100 },
                                            800);
                                }, 500);
                            }
                        }
                        $timeout(function () {
                            //console.log(!$scope.output.is_qh_shown set_multi_index& ($scope.output.show_qh_automatically && $scope.qh_text != ""));
                            //console.log($scope.output.is_qh_shown);
                            //console.log($scope.output.show_qh_automatically);
                            //console.log($scope.qh_text);
                            if (!$scope.output.dont_show_qh && !$scope.output.is_qh_shown && ($scope.output.show_qh_automatically && $scope.qh_text != "")) { //&& ($scope.current_step - temp_step == 1 || $scope.current_step == -1)
                                $scope.set_qh_cookies(1, quick_help);
                                $(document).foundation();
                                $("#tt-view-qh-modal").foundation("reveal", "open");
                            }
                            
                            temp_step = $scope.current_step == -1 ? 1 : $scope.current_step;
                        }, 500);
                        
                        //reflow foundation
                         $timeout(function () {
                             $(document).foundation("reflow");
                            //console.log("reflow atAllPairwise timeout");
                         }, 800);

                        is_info_tooltip = $scope.output.is_infodoc_tooltip;
      
                        is_multi = $scope.is_multi;

                        try {
                            $scope.is_comment_updated = false;
                            //$scope.output.usersComments = JSON.parse($scope.output.usersComments);
                            //$timeout(function () {
                            //   $scope.$apply(function () {
                            //        $scope.output.usersComments = $scope.output.usersComments;
                            //       // //console.log($scope.output.usersComments);
                            //        $.each($scope.output.usersComments, function (i, value) {
                            //            if (!$scope.is_multi) {
                            //                var comment_str = $scope.output.usersComments[i].comment.split("_");
                            //               // $scope.formatted_comment = comment_str[0];

                            //                if (comment_str[1] == null) {
                            //                    comment_str[1] = new Date().getTime();
                            //                }
                            //                $scope.output.usersComments[i].c_date = parseFloat(comment_str[1]);
                            //                $scope.output.usersComments[i].date = $scope.get_comment_date(comment_str[1]);
                            //            }
                            //            else {
                            //                try{
                            //                    if ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atAllPairwiseOutcomes") {
                            //                        var comment_str = $scope.output.multi_pw_data[$scope.active_multi_index].Comment.split("_");
                            //                    }
                            //                    else {
                            //                        var comment_str = $scope.output.multi_non_pw_data[$scope.active_multi_index].Comment.split("_");
                            //                    }

                            //                   // $scope.formatted_comment = comment_str[0];
                            //                    $.each($scope.output.usersComments[i], function (j, value) {
                            //                        comment_str = value.comment.split("_");
                            //                        if (comment_str[1] == null) {
                            //                            comment_str[1] = new Date().getTime();
                            //                        }

                            //                        $scope.output.usersComments[i][j].c_date = parseFloat(comment_str[1]);
                            //                        $scope.output.usersComments[i][j].date = $scope.get_comment_date(comment_str[1]);
                            //                    });

                            //                    $scope.multi_comments = $scope.output.usersComments;
                            //                }
                            //                catch (e) {

                            //                }
                            //            }

                            //        });
                            //   });
                            //}, 100);


                          

                        }
                        catch (e) {

                        }

                        //
                        try {
                            if (!$scope.output.pipeOptions.hideNavigation) {
                                $scope.output.no_footer_options++;
                            }

                            if ((!$scope.output.pipeOptions.hideNavigation)) {
                                $scope.output.no_footer_options++;
                            }

                            if ($scope.output.pipeOptions.showUnassessed) {
                                $scope.output.no_footer_options++;
                            }

                            if ($scope.output.page_type != "atShowLocalResults" && $scope.output.page_type != "atShowGlobalResults" && $scope.output.show_comments && $scope.output.page_type != "atInformationPage" && $scope.output.page_type != "atSensitivityAnalysis" && $scope.output.page_type != "atSpyronSurvey") {
                                $scope.output.no_footer_options++;
                            }

                            //if (($scope.output.pipeOptions.showUnassessed) && ($scope.output.ShowUnassessed) && ($scope.output.nextUnassessedStep[0] != $scope.output.current_step) && ($scope.unassessed_data[0] != $scope.output.current_step || $scope.unassessed_data[1] > 1)) {
                            //same condition as Next Unassessed button of Footer.ascx
                            if (($scope.output.pipeOptions.showUnassessed) && ($scope.output.nextUnassessedStep == null || $scope.output.nextUnassessedStep[0] != $scope.output.current_step) && ($scope.unassessed_data[0] != $scope.output.current_step || $scope.unassessed_data[1] > 1)) {
                                $scope.output.no_footer_options++;
                            }
                        }
                        catch (e) {

                        }
                        $scope.output.no_footer_options = $scope.output.no_footer_options > 4 ? 4 - 1 : $scope.output.no_footer_options - 1;

                        $scope.$parent.is_multi = $scope.is_multi;
                        //break hyperlinks with lesser than 40 text
                        $timeout(function () {
                            $(".wrap-hyperlink a").each(function (e) {
                                if ($(this).text().length < 40) {
                                    $(this).css("word-break", "normal");
                                }
                            });
                        }, 500);

                        $timeout(function () {
                            $(".qh-icons").removeClass("change-color-pulse");
                        }, 7000);

                        if (!$scope.is_saving_judgment) {

                        }
                        $scope.refreshed = true;
                        $scope.check_offset_for_footer();
                        $window.scrollTo(0, 0);

                        var parentDiv = $(".multi-loop-wrap");
                        if (parentDiv.length > 0) {
                            parentDiv.scrollTop(0);
                        }


                    //end loading
                    if (showLoadingMessage) {
                        LoadingScreen.end(100);
                    }

                    $scope.showSelectStepModal();
                    $scope.showAutoAdvanceModal();
                    // $scope.reflow_equalizer();

                $timeout(function () {
                    ToggleWRTPath(6, true);

                    if ($scope.is_multi && info_doc_index >= 0) {
                        $scope.$apply(function () {
                            $scope.active_multi_index = info_doc_index;
                            $scope.set_multi_index(info_doc_index);
                            info_doc_index = -1;
                        });
                        
                    }
                }, 1000);

                //things to do before finish

                $scope.isGoodToCallResizeFrameImages = false;
                $scope.setMaxImageSizeFromFrames();
                $scope.renderInfoDocSizes(2000);

                if ($scope.output.isPipeViewOnly) {
                    $("#divHeading").remove();
                    $timeout(function() {
                        $("#divViewOnlyMessage").removeClass("hide");
                    }, 500, false);
                } else {
                    $("#divHeading").removeClass("hide");
                }

                //timeout value for IE 11 or less and other browsers
                var timeOutValue = $scope.getIEVersion() >= 12 ? 1500 : 3500;

                if (angular.isDefined($scope.pipeDataTimer)) {
                    $timeout.cancel($scope.pipeDataTimer);
                    $scope.pipeDataTimer = undefined;
                }
                
                $scope.pipeDataTimer = $timeout(function () {
                    //inner div resize
                    //check content heights on load
                    $scope.CheckMultiHeight("many-alternatives", "210");
                    $scope.CheckMultiHeight("jj-wrap-fix", "50");
                    $scope.CheckMultiHeight("multi-loop-wrap", "80");
                    $scope.CheckMultiHeight("multi-loop-wrap-local-results", "500");
                    $scope.CheckMultiHeight("tt-pipe-canvas", "147");
                    $scope.CheckMultiHeight("pipe-wrap", "100");
                    $scope.CheckMultiHeight("overflow-fix", "150");
                    //$scope.CheckMultiHeight("tt-rating-scale", "100");

                    $scope.removeSliderTabIndex();

                    $scope.isGoodToCallResizeFrameImages = true;
                    $scope.change_info_doc_image_size();
                    
                    // scrolling to active index after everything is done
                    //$scope.prev_multi_index = $scope.active_multi_index;
                    $scope.prev_no_of_lines = null;
                    $scope.scroll_to_active_index();
                    ToggleWRTPath(5, true);

                    if ($scope.isMobile() && ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atAllPairwiseOutcomes")) {
                        $timeout(function () {
                            $scope.set_multi_index($scope.active_multi_index);
                        }, 250);
                    }

                    //$scope.countPageWatchers();
                    $(".direct-tooltip").tooltip({
                        position: {
                            my: "center top",
                            at: "center bottom+2"
                        }
                    });
                    $(".ratings-tooltip").tooltip({
                        position: {
                            my: "center top",
                            at: "center bottom+2"
                        }
                    });
                }, timeOutValue);

                $scope.addLastStepButton();
                $scope.$parent.is_judgment_screen = $scope.is_judgment_screen;

                var timeoutTime = new Date();
                timeoutTime.setMinutes(timeoutTime.getMinutes() + 15);  //adding 15 minutes to current time 
                $scope.sessionTimeoutTime = timeoutTime;    //setting next session timeout time

                $scope.startAutoSaveJudgmentsTimer();

            }, function error(response) {
                //console.log(response);

                if (showLoadingMessage) {
                    LoadingScreen.end(100);
                }

                ////console.log(response);

                if (response.status == -1 || response.status == 404) {
                    //internet connection problem / session timeout / cci just published site
                    alert("Session timed out. Please try again.");
                    window.onbeforeunload = null;
                }
                else if (response.data == null) {
                    //timeout
                    alert("Something went wrong. Please try again");
                }
                else if (!(step == 1)) {
                    alert("Something went wrong. You will be moved back to the previous step");
                    $scope.get_pipe_data(stip);
                    return null;
                }

                window.location.href = baseUrl;

            });
        }

        $scope.addLastStepButton = function() {
            // what button to displayed in footer at the last step
            if ($scope.current_step == $scope.output.total_pipe_steps) {
                if ($scope.is_judgment_screen) {
                    if ($scope.output.doneOptions.stayAtEval) {
                        $scope.$parent.showSave = $scope.output.doneOptions.logout ? false : true;
                        $scope.$parent.showFinish = $scope.output.doneOptions.logout ? true : false;
                    }
                    else {
                        if (!$scope.output.doneOptions.logout) {
                            $scope.$parent.showSave = ($scope.output.doneOptions.redirect && $scope.output.doneOptions.url == "") || ($scope.output.doneOptions.openProject && $scope.output.nextProject == -1) ? true : false;
                            $scope.$parent.showFinish = ($scope.output.doneOptions.redirect && $scope.output.doneOptions.url == "") || ($scope.output.doneOptions.openProject && $scope.output.nextProject == -1) ? false : true;
                        }
                        else {
                            $scope.$parent.showFinish = true;
                        }
                    }
                }
                else {
                    $scope.$parent.showSave = false;
                    if ($scope.output.doneOptions.stayAtEval) {
                        $scope.$parent.showFinish = $scope.output.doneOptions.logout ? true : false;
                    }
                    else {
                        if (!$scope.output.doneOptions.logout) {
                            $scope.$parent.showFinish = ($scope.output.doneOptions.redirect && $scope.output.doneOptions.url == "") || ($scope.output.doneOptions.openProject && $scope.output.nextProject == -1) ? false : true;
                        }
                        else {
                            $scope.$parent.showFinish = true;
                        }
                    }
                }
            }
        }

        $scope.cancelAutoSaveJudgmentsTimer = function () {
            //Cancelling auto save timer when user navigates to a step
            if (angular.isDefined($scope.autoSaveTimer)) {
                //console.log(currentDateTimeString() + " -> cancelling auto save timer...");

                $timeout.cancel($scope.autoSaveTimer);
                $scope.autoSaveTimer = undefined;
            }
        }

        $scope.startAutoSaveJudgmentsTimer = function (cancelCurrentTimer) {
            if (angular.isDefined(cancelCurrentTimer) && cancelCurrentTimer) {
                $scope.cancelAutoSaveJudgmentsTimer();
            }

            if (!angular.isDefined($scope.autoSaveTimer)) {
                var timeoutValue = Math.abs(new Date() - $scope.sessionTimeoutTime);   //setting next auto save time
                //console.log(currentDateTimeString() + " -> adding auto save timer...");
                //$scope.showLoadingMessage = false; //for hiding loading/saving message

                //Start auto save timer on timeoutValue
                $scope.autoSaveTimer = $timeout(function () {
                    //console.log(currentDateTimeString() + " -> calling auto save...");

                    $scope.isAutoSaving = true;
                    if ($scope.output.page_type == "atSpyronSurvey") {
                        $scope.get_pipe_data($scope.current_step, $scope.is_AT_owner, null);
                    } else {
                        $scope.save_multis();
                    }
                    
                }, timeoutValue);
            }
        }

        $scope.setMaxImageSizeFromFrames = function () {
            //if ($scope.output.framed_info_docs && ($scope.isMobile() || $scope.output.autoFitInfoDocImages)) {
            if ($scope.output.pairwise_type == "ptVerbal" && ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atPairwise")) {
                var singleOrMultiPwData = [];

                if ($scope.is_multi) {
                    singleOrMultiPwData = $scope.multi_data;
                } else {
                    singleOrMultiPwData.push({
                        "InfodocLeft": $scope.output.first_node_info,
                        "InfodocRight": $scope.output.second_node_info
                    });
                }

                $scope.processMaxImageSizes(singleOrMultiPwData);
            }
        }

        $scope.processMaxImageSizes = function (singleOrMultiPwData) {
            if (angular.isDefined(singleOrMultiPwData) && singleOrMultiPwData.length > 0) {
                $scope.maxImageWidth = angular.isDefined($scope.maxImageWidth) ? $scope.maxImageWidth : 0;
                $scope.maxImageHeight = angular.isDefined($scope.maxImageHeight) ? $scope.maxImageHeight : 0;

                var maxCounter = singleOrMultiPwData.length > 1 ? Math.ceil(singleOrMultiPwData.length / 2) : Math.floor(singleOrMultiPwData.length / 2);

                for (var loopCounter = maxCounter; loopCounter >= 0; loopCounter--) {
                    var infoImage = $(singleOrMultiPwData[loopCounter].InfodocLeft).find("img");
                    if (infoImage) {
                        $("<img/>", {
                            load: function () {
                                $scope.setMaxImageWidthHightFromFrames(this.naturalWidth, this.naturalHeight, "Left");
                            },
                            src: infoImage.attr("src")
                        });
                    }

                    infoImage = $(singleOrMultiPwData[loopCounter].InfodocRight).find("img");
                    if (infoImage) {
                        $("<img/>", {
                            load: function () {
                                $scope.setMaxImageWidthHightFromFrames(this.naturalWidth, this.naturalHeight, "Right");
                            },
                            src: infoImage.attr("src")
                        });
                    }
                }
            }
        }

        $scope.setMaxImageWidthHightFromFrames = function(imageWidth, imageHeight, frameName) {
            $scope.maxImageWidth = imageWidth > $scope.maxImageWidth ? imageWidth : $scope.maxImageWidth;
            $scope.maxImageHeight = imageHeight > $scope.maxImageHeight ? imageHeight : $scope.maxImageHeight;
                                    
            ////console.log(frameName + " $scope.maxImageWidth: " + $scope.maxImageWidth + ", $scope.maxImageHeight: " + $scope.maxImageHeight);
            $scope.maxImageWidth = $scope.maxImageWidth == 0 ? undefined : $scope.maxImageWidth;
            $scope.maxImageHeight = $scope.maxImageHeight == 0 ? undefined : $scope.maxImageHeight;
        }

        $scope.allSinglePairwiseInfoDocs = [];
        $scope.getAllSinglePairwiseInfoDocs = function () {
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/GetAllSinglePairwiseInfoDocs",
                data: {},
                headers: {
                    'Content-Type': "application/json; charset=utf-8"
                }
            }).then(function success(response) {
                $scope.allSinglePairwiseInfoDocs = response.data.d;
                $scope.processMaxImageSizes($scope.allSinglePairwiseInfoDocs);
            }, function error(response) {
                $scope.allSinglePairwiseInfoDocs = [];
            });
        }

        $scope.checkProjectLockStatus = function () {
            $scope.cancelProjectLockStatusTimer();
            $(".locked-reload-message").show();

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/CheckProjectLockStatus",
                data: {},
                headers: {
                    'Content-Type': "application/json; charset=utf-8"
                }
            }).then(function success(response) {
                var lockedInfo = JSON.parse(response.data.d);

                if (lockedInfo.teamTimeUrl.length > 0) {
                    LoadingScreen.end(100, false);
                    $scope.$parent.projectLockedInfo = lockedInfo;
                    $scope.$parent.projectLockedInfo.status = true;

                    $("div.teamtime-started").remove();
                    $(".tt-full-height.tt-body.tt-pipe.anytime-wrap").remove();

                    //Redirect to Comparion TeamTime after 3 seconds as TeamTime session started
                    $scope.projectLockStatusTimer = $timeout(function () {
                        window.onbeforeunload = null;
                        window.location.href = lockedInfo.teamTimeUrl;
                    }, 5000);
                } else {
                    if ($scope.$parent.projectLockedInfo.status != lockedInfo.status) {
                        $scope.$parent.projectLockedInfo = lockedInfo;

                        if ($scope.$parent.projectLockedInfo.status) {
                            $(".tt-full-height.tt-body.tt-pipe.anytime-wrap").remove();
                        } else {
                            window.onbeforeunload = null;
                            window.location.href = baseUrl + "pages/Anytime/Anytime.aspx";
                        }
                    }

                    if ($scope.$parent.projectLockedInfo.status) {
                        $scope.projectLockStatusTimer = $timeout(function() {
                            $scope.checkProjectLockStatus();
                        }, 20000);
                    }
                }

                $(".locked-reload-message").hide();
            }, function error(response) {
                $scope.$parent.projectLockedInfo.status = false;
                $scope.$parent.projectLockedInfo.message = "";
            });
        }

        $scope.cancelProjectLockStatusTimer = function () {
            if (angular.isDefined($scope.projectLockStatusTimer)) {
                $timeout.cancel($scope.projectLockStatusTimer);
                $scope.projectLockStatusTimer = undefined;
            }
        }

        $scope.CheckMultiHeight = function (dc, size) {
            var divSize = size; //the DIV class to resize
            var divClass = dc; //the DIV class to resize
            var windowHeight = $(window).height();
            var mobileHeaderHeight = $(".mobile-header").height();
            var footerHeight = $(".tt-footer-wrap").height();

            var infodocHeight = $(".InfoDocsParentDiv").height();
            //var scales_description = $("#scales_description").height();

            //var tt_rating_scale = $(".tt-rating-scale").height();
            //if (scales_description >= tt_rating_scale)
            //    scales_description = 0;

            //var newHeight = windowHeight - (mobileHeaderHeight + footerHeight + infodocHeight + scales_description);
            var newHeight = windowHeight - (mobileHeaderHeight + footerHeight + infodocHeight);

            if (divClass == "pipe-wrap") {
                if ($scope.output.page_type != "atShowLocalResults") {
                    $("." + divClass).css({
                        "max-height": (newHeight - footerHeight) - divSize + "px"
                    });
                 }

            } else {
                var maxHeight = (newHeight - footerHeight) - divSize;
                
                if (divClass === "multi-loop-wrap") {
                    if ($scope.showBrowserWarning() && $scope.output.hashLink) {
                        maxHeight -= $("#browser-warning-message").outerHeight() + ($scope.is_explorer ? 36 : 1);
                    }

                    maxHeight = (maxHeight < 190 ? 190 : maxHeight);
                }
                
                //console.log("maxHeight:" + maxHeight);

                if (divClass !== "multi-loop-wrap-local-results") {
                    $("." + divClass).css({
                        "overflow": "auto",
                        "max-height": maxHeight + "px" //30 allowance
                    });

                    if (divClass == "multi-loop-wrap") {
                        var stickyRatingsHeight = $(".tt-sticky-element-ratings").length > 0 ? $(".tt-sticky-element-ratings").height() : maxHeight;

                        if (stickyRatingsHeight > maxHeight) {
                            $(".tt-sticky-element-ratings").css({
                                "overflow-y": "scroll",
                                "max-height": maxHeight + "px"
                            });
                        } else {
                            $(".tt-sticky-element-ratings").css({
                                "overflow-y": "",
                                "max-height": ""
                            });
                        }
                    }
                } else {
                    $("." + divClass).css({
                        "max-height": maxHeight + "px" //30 allowance
                    });
                }
            }



        }

        $scope.removeSliderTabIndex = function() {
            //removing tab index for multi direct sliders
            if ($scope.output.page_type === "atNonPWAllChildren" && $scope.output.non_pw_type === "mtDirect") {
                var multiDirectElements = $(".slider.multiDirectSlider .ui-slider-handle");
                if (multiDirectElements.length > 0) {
                    multiDirectElements.attr("tabindex", "-1");
                    //multiDirectElements.each(function () {
                    //    $(this).children().attr("tabindex", "-1");
                    //});
                }
            }
        }

        //show step selection modal
        $scope.showSelectStepModal = function () {
            if ($scope.output.isPipeViewOnly) {
                $scope.inconsistency = true;

                if (!$scope.output.isPipeStepFound) {
                    $("#PipeStepNotFoundModal").foundation("reveal", "open");
                }
            } else {
                //when users comes to left judgement first time after login && judgment made > 0 && total pipe steps != 1 && first unassessed step != current step && (current step > 1 || (current step > 0 && first unassessed step > 0))
                if ($scope.output.is_first_time && $scope.output.judgment_made > 0 && $scope.output.total_pipe_steps != 1 && $scope.output.first_unassessed_step != $scope.current_step && ($scope.current_step > 1 || ($scope.current_step > 0 && $scope.output.first_unassessed_step > 0))) {
                    $("#SelectStepModal").foundation("reveal", "open");
                }
            }
        }

        $scope.showAutoAdvanceModal = function () {
            if ($scope.output.show_auto_advance_modal) {
                $("#AutoAdvanceModal").foundation("reveal", "open");
            }
        }

        $scope.wrt_node_name = "";
        //End of get_pipe_data
        $scope.Sense == null;
        //survey
        $scope.QuestionStep = 0;
        //    $scope.hideSurveyItem = function (surveyHeight, bodyHeight, isprev) {
        //
        //        var height = 0;
        //        var footerHeight = $(".footer-nav-mobile").outerHeight();
        //
        //        if (!$scope.$parent.isMobile())
        //            footerHeight = $(".tt-footer-wrap").outerHeight() + $(".tt-header").outerHeight() + $(".tt-header-nav").outerHeight();
        //
        //        if (!isprev) {
        //            for (i = $scope.QuestionStep; i < $scope.SurveyPage.Questions.length; i++) {
        //                var tempHeight = $(".tt-survey-item-" + i).outerHeight();
        //
        //                if (height + tempHeight > (bodyHeight - footerHeight)) {
        //                    break;
        //                }
        //
        //
        //                height += tempHeight;
        //                $scope.QuestionStep = i;
        //                $scope.QuestionLength = $scope.QuestionStep;
        //            }
        //        }
        //        else {
        //            for (i = $scope.SurveyPage.Questions.length - 1; i >= 0; i--) {
        //                var tempHeight = $(".tt-survey-item-" + i).outerHeight();
        //                if (height + tempHeight > (bodyHeight - footerHeight)) {
        //                    if (isprev) {
        //                        //$scope.QuestionStart = $scope.SurveyPage.Questions.length - $scope.QuestionStep;
        //                        //$scope.QuestionLength = $scope.SurveyPage.Questions.length;
        //                    }
        //                    break;
        //                }
        //                height += tempHeight;
        //                $scope.QuestionStep = i;
        //                $scope.QuestionStart = $scope.QuestionStep;
        //            }
        //        }
        //
        //        //var heightbyitems = 
        //        //var footerHeight = $(".footer-nav-mobile").outerHeight();
        //        //$scope.QuestionStep =  Math.floor((bodyHeight - footerHeight) / heightbyitems);
        //        //if (isprev) {
        //
        //        //    $scope.QuestionStart = $scope.SurveyPage.Questions.length - $scope.QuestionStep;
        //        //    $scope.QuestionLength = $scope.SurveyPage.Questions.length;
        //        //}
        //        //else {
        //        //    $scope.QuestionLength = $scope.QuestionStep - 1;
        //        //}
        //
        //
        //    }
        //
        $scope.survey_spaces = 10;
        $scope.get_level_spaces = function (level) {
            var spaces = 0;
            for (i = 0; i <= level; i++) {
                spaces += $scope.survey_spaces;
            }
            return spaces + "px";
        }

        //survey
        $scope.init_checklist = function (index, list, type) {
            if (type == 12 || type == 13) {
                var stringAnswer = "";

                $scope.answer[index] = [];
                for (i = 0; i < Object.keys(list).length; i++) {
                    $scope.answer[index][i] = list.isDisabled;

                    var toAdd = $scope.answer[index][i] + ";";
                    if (i == Object.keys(list).length - 1) {
                        toAdd = $scope.answer[index][i];
                    }
                    stringAnswer += toAdd;
                }
                $scope.SurveyAnswers[index][0] = stringAnswer;
            }
        }



        $scope.sss = function (s) {

            return true;
        }
        //survey
        $scope.checkChbAnswer = function (answer, question, is_chb) {
            if (is_chb) {
                var min = question.MinSelectedVariants;
                var max = question.MaxSelectedVariants == 0 ? Number.MAX_VALUE : question.MaxSelectedVariants;
                if (question.Type == 4) {
                    var options = answer.split(";");
                    var totalAnsweredChb = options.length;
                    var totalVariants = question.Variants.length;
                    if (question.Variants[totalVariants - 1].Type == 2)
                        totalAnsweredChb -= 1;
                    if (min > totalAnsweredChb || max < totalAnsweredChb)
                        return false;
                    else {
                        return true;
                    }
                }
                else {
                    var totalAnsweredChb = (answer.match(/true/g) || []).length;
                    if (min > totalAnsweredChb || max < totalAnsweredChb)
                        return false;
                    else {
                        return true;
                    }
                }

            }
            return true;

        }

        $scope.text_is_equal = function (answer, variant, is_chb, question) {
            if (answer) {
                if (is_chb) {
                    var options = answer.split(";");
                    var totalAnsweredChb = options.length - 1;
                    var min = question.MinSelectedVariants;
                    var max = question.MaxSelectedVariants == 0 ? Number.MAX_VALUE : question.MaxSelectedVariants;
                    for (x in options) {
                        if (options[x] == variant) {
                            return true;
                        }
                    }
                }
                else {
                    if (variant == "Other:") {
                        var options = answer.split(":");
                        if (answer.indexOf(":") > -1)
                            return variant;
                    }
                    return answer;

                }
            }
            return false;
        }
        $scope.get_othervalue = function (answer, type, variant, index) {
            //for checkbox other value
            if (type == 3) {
                if (answer.indexOf(":") > -1) {
                    var options = answer.split(":");
                    return options[0];
                }
            }
            if (type == 4) {
                var options = answer.split(";");
                var last = options.length - 1;

                for (x in options) {
                    if (x == last) {
                        if ((options[last]).indexOf(":") > -1) {
                            var text = options[last].split(":");
                            //$scope.change_otherAnswer(index, answer, variant);
                            return text[0];
                        }
                        else
                            return "";
                    }
                }
                return "";
            }

            //for combobox other value
            if (type == 5) {
                var options = answer.split(":");
                return options[0];
            }
        }

        $scope.get_combobox_value = function (variants, answer) {
            if (answer.indexOf(";") > -1) {
                var strippedAnswer = answer.split(";");
                answer = strippedAnswer[0];
            }
            for (i = 0; i < variants.length; i++) {
                //alert(JSON.stringify(variants[i]));
                if (variants[i].VariantValue == answer) {
                    return variants[i];
                }
            }

        }

        //survey
        $scope.optionsFilter = function (option) {
            if (option.VariantValue == "") {
                return false;
            }
            return true;
        }

        $scope.answer = [];
        $scope.change_respondentAnswer = function (index, text, type, is_chb, variant, question) {
            if (is_chb) {
                if (text) {
                    var currentAnswer = ";" + variant;
                    if ($scope.SurveyAnswers[index][0] == "")
                        currentAnswer = variant;
                    $scope.SurveyAnswers[index][0] += currentAnswer;
                }
                else {
                    //if is first on string then delete ------

                    if ($scope.SurveyAnswers[index][0].indexOf(";" + variant) < 0)
                        $scope.SurveyAnswers[index][0] = ($scope.SurveyAnswers[index][0]).replace(variant + ";", "");
                    else
                        $scope.SurveyAnswers[index][0] = ($scope.SurveyAnswers[index][0]).replace(";" + variant, "");

                    if ($scope.SurveyAnswers[index][0].indexOf(";") < 0)
                        $scope.SurveyAnswers[index][0] = ($scope.SurveyAnswers[index][0]).replace(variant, "");

                }
            }
            else {
                if ([1, 2, 14, 15].indexOf(type) > -1) {
                    $scope.SurveyAnswers[index][0] = text;

                    $scope.answer[index] = text;
                }
                if (type == 3) {

                    if (text == "")
                        $scope.SurveyAnswers[index][0] = "";
                    else {
                        $scope.SurveyAnswers[index][0] = text[0].indexOf(":") < 0 ? text[0] : text[1];
                    }

                    $scope.answer[index] = text;
                }
                if (type == 5) {
                    $scope.answer[index][1] = "";
                    $scope.SurveyAnswers[index][0] = text;
                }
                if (type == 12 || type == 13) {

                    var stringAnswer = "";
                    for (i = 0; i < Object.keys($scope.answer[index]).length; i++) {
                        if (variant == 0) {
                            if ($scope.answer[index][0]) {
                                $scope.answer[index][i] = true;
                            }
                            else {
                                //variant is index2
                                if (variant == 0) {
                                    if ((i !== 1 && type == 12) || (type == 13))
                                        $scope.answer[index][i] = false;
                                }
                            }
                        }
                        var toAdd = $scope.answer[index][i] + ";";
                        if (i == Object.keys($scope.answer[index]).length - 1) {
                            toAdd = $scope.answer[index][i];
                        }
                        stringAnswer += toAdd;
                    }
                    //alert(stringAnswer);
                    $scope.SurveyAnswers[index][0] = stringAnswer;





                }
                
            }

            $scope.changeNextButtonStatusForSurvey();
        }

        //survey
        $scope.change_otherAnswer = function (type, index, text, variant) {
            if (type == 3) {
                $scope.SurveyAnswers[index][0] = text;
            }
            if (type == 4) {
                if (($scope.SurveyAnswers[index][0]).indexOf(":") > -1) {
                    var removestring = ($scope.SurveyAnswers[index][0]).split(";");
                    $scope.SurveyAnswers[index][0] = "";
                    //remove 'other' text so it wont create duplicate items
                    for (i = 0; i < removestring.length; i++) {
                        if (removestring[i].indexOf(":") < 0)
                            $scope.SurveyAnswers[index][0] += removestring[i] + ";";
                    }
                }
                if (text) {
                    var currentAnswer = text + ":";
                    if ($scope.SurveyAnswers[index][0] == "")
                        currentAnswer = text + ":";
                    $scope.SurveyAnswers[index][0] += currentAnswer;
                }
                else {
                    //if is first on string then delete ------
                    if ($scope.SurveyAnswers[index][0].indexOf(";" + text + ":") < 0)
                        $scope.SurveyAnswers[index][0] = ($scope.SurveyAnswers[index][0]).replace(text + ":" + ";", "");
                    else
                        $scope.SurveyAnswers[index][0] = ($scope.SurveyAnswers[index][0]).replace(";" + text + ":", "");

                    if ($scope.SurveyAnswers[index][0].indexOf(";") < 0)
                        $scope.SurveyAnswers[index][0] = ($scope.SurveyAnswers[index][0]).replace(text + ":", "");
                }
            }
            if (type == 5) {
                $scope.answer[index][0] = "";
                $scope.SurveyAnswers[index][0] = text;
            }

            $scope.changeNextButtonStatusForSurvey();
        }

        $scope.checkSurveyValue = function (value) {
            if (value == "" || value == " " || value == ":" || value == null)
                return false;
            return true;
        }
        //survey
        $scope.save_respondentAnswer = function (stip, step, isFinish) {

            //if ($scope.doNotSubmitSurvey) {
            //    alert("Please select necessary count of items"); //need to be changed into a modal alert!
            //    hide_loading_modal();
            //    return false;
            //}
            var canSubmit = false;
            var message = "";
            var skip = false;
            angular.forEach($scope.SurveyAnswers, function (value, key) {
                if (!skip) {
                    var surveyquestion = $scope.SurveyPage.Questions[key];
                    var reg = new RegExp('^[0-9.\n]+$');
                    if (surveyquestion.Type == 4) {
                        var options = value[0].split(";");
                        var totalAnsweredChb = options.length;
                        var totalVariants = surveyquestion.Variants.length;
                        if (surveyquestion.Variants[totalVariants - 1].Type == 2)
                            totalAnsweredChb -= 1;
                        var min = surveyquestion.MinSelectedVariants;
                        var max = surveyquestion.MaxSelectedVariants == 0 ? Number.MAX_VALUE : surveyquestion.MaxSelectedVariants;
                        if (min > totalAnsweredChb || max < totalAnsweredChb) {
                            canSubmit = true;
                            skip = true;
                            message = "Please select necessary count of items";
                        }
                    }
                    else if (surveyquestion.Type == 12 || surveyquestion.Type == 13) {
                        var min = surveyquestion.MinSelectedVariants;
                        var max = surveyquestion.MaxSelectedVariants == 0 ? Number.MAX_VALUE : surveyquestion.MaxSelectedVariants;
                        var totalAnsweredChb = (value[0].match(/true/g) || []).length;
                        if (min > totalAnsweredChb || max < totalAnsweredChb) {
                            canSubmit = true;
                            skip = true;
                            message = "Please select necessary count of items";
                        }
                    }
                    else if (surveyquestion.Type == 14 || (surveyquestion.Type == 15 && value[0] != "")) {
                        if (!value[0].match(reg)) {
                            message = "Please enter a valid number";
                            canSubmit = true;
                            skip = true;
                        }
                    }
                    if (value[2] == "False") {
                        if (!$scope.checkSurveyValue(value[0])) {
                            canSubmit = true;
                            skip = true;
                            message = "Please, complete all required questions.";
                        }
                    }
                }
            });
            if (canSubmit && parseInt(stip) < parseInt(step)) {
                alert(message); //need to be changed into a modal alert!
                LoadingScreen.end(100, true);
                return false;
            }

            $http({
                method: "POST",
                url: $scope.baseurl + "pages/Anytime/Anytime.aspx/saveRespondentAnswers",
                async: false,
                data: {
                    step: stip,
                    RespondentAnswers: $scope.SurveyAnswers
                }
            }).then(function success(response) {
                if (response.data.d) {
                    $scope.steps_list = eval("[" + response.data.d + "]");
                    $scope.$parent.steps_list = $scope.steps_list;
                    //if (step > 3)
                    //    step = 3;
                }

                $scope.SurveyAnswers = [];
                $scope.answer = [];
                

                if (angular.isUndefined(isFinish) || isFinish != true)
                    $scope.get_pipe_data(step, null);

                $scope.output.page_type = "";

            }, function error(response) {
                // alert("error! got pm!");

            });
        }

        $scope.changeNextButtonStatusForSurvey = function () {
            var disableNext = false;

            if ($scope.output.pipeOptions.dontAllowMissingJudgment) {
                angular.forEach($scope.SurveyAnswers,function(value, key) {
                    if (!disableNext) {
                        if (value[0] == "") {
                            disableNext = true;
                        }
                    }
                });
            }

            $scope.$parent.disableNext = disableNext;
        }



        $scope.is_screen_reduced = false;

  
        //comment updated
        $scope.temp_comments = [];
        $scope.close_multi_comment = function (index, type) {
            $(".f-dropdown").removeClass("open");
            //if ($scope.is_multi) {
            //    $("#comment-" + index).removeClass("open");
            //    if (type == "close") {
            //        if ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atAllPairwiseOutcomes") {
            //            $(".multi_comment_" + index).val($scope.output.multi_pw_data[index].Comment);
            //        }
            //        else {
            //            $(".multi_comment_" + index).val($scope.output.multi_non_pw_data[index].Comment);
            //        }
            //    }
            //    else {
            //        if ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atAllPairwiseOutcomes") {
            //            $scope.output.multi_pw_data[index].Comment = $scope.temp_comments[index];
            //        }
            //        else {
            //            $scope.output.multi_non_pw_data[index].Comment = $scope.temp_comments[index];
            //        }

            //    }
            //}
            //else {
            //    $("#comment-" + index).removeClass("open");
            //    if (type == "close") {
            //        $(".single_comment").val($scope.output.comment);
            //    }
            //    else {
            //        $scope.output.comment =  $(".single_comment").val();
            //    }
            //}
           
        }
        //$scope.close_single_comment = function () {
        //    $("#comment-single").removeClass("open");
        //}

        $scope.step_input = 0;

        //$scope.$on("comment_updated", function (e) {
        //    $scope.comment_updated();
        //});
        $scope.get_comment_date = function (timestamp) {
            var date = [];
            if (typeof (timestamp) != "undefined" && timestamp != null) {
                //do nothing
            }
            else {
                return date;
            }

            var d = new Date(parseFloat(timestamp));

            minutes = d.getMinutes().toString().length == 1 ? "0" + d.getMinutes() : d.getMinutes(),

            hours = d.getHours() > 12 ? d.getHours() - 12 : d.getHours();
            hours = hours.toString().length == 1 ? "0" + hours : hours,

            ampm = d.getHours() >= 12 ? "pm" : "am",
            months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            days = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
            date[0] = days[d.getDay()] + " " + months[d.getMonth()] + " " + d.getDate() + ", " + d.getFullYear();
            date[1] = hours + ":" + minutes + ampm;
            return date;
        }



        $scope.comment_updated = function (formatted_comment) {
            $("#comment-0").removeClass("open"); //close tooltip
            //if (formatted_comment.trim() == "") {
            //    alert("Please enter your comment.");
            //    return false;
            //}
            if ($scope.output != null) {
                $scope.is_comment_updated = true;
                $scope.$parent.openModal = false;   //11646
                var comment_str = "";

                if (!$scope.is_multi) {
                    //if (formatted_comment == "") {
                    //    $scope.output.comment = "";
                    //}
                    //else{
                    //    $scope.output.comment = formatted_comment + "_" + new Date().getTime();
                    //}
                } else {
                    //if ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atAllPairwiseOutcomes") {
                    //    if (formatted_comment.trim() == "") {
                    //        $scope.output.multi_pw_data[$scope.active_multi_index].Comment = "";
                    //    }
                    //    else {
                    //        $scope.output.multi_pw_data[$scope.active_multi_index].Comment = formatted_comment + "_" + new Date().getTime();
                    //    }
                    //}
                    //if (($scope.output.page_type == "atNonPWAllChildren" || $scope.output.page_type == "atNonPWAllCovObjs") && $scope.output.non_pw_type == "mtRatings") {
                    //    if (formatted_comment.trim() == "") {
                    //        $scope.output.multi_non_pw_data[$scope.active_multi_index].Comment = "";
                    //    }
                    //    else {
                    //        $scope.output.multi_non_pw_data[$scope.active_multi_index].Comment = formatted_comment + "_" + new Date().getTime();
                    //    }
                    //}
                    //if (($scope.output.page_type == "atNonPWAllChildren" || $scope.output.page_type == "atNonPWAllCovObjs") && $scope.output.non_pw_type == "mtDirect") {
                    //    if (formatted_comment.trim() == "") {
                    //        $scope.output.multi_non_pw_data[$scope.active_multi_index].Comment = "";
                    //    }
                    //    else {
                    //        $scope.output.multi_non_pw_data[$scope.active_multi_index].Comment = formatted_comment + "_" + new Date().getTime();
                    //    }
                    //}
                    $scope.save_multis();
                }

                $scope.output.comment = $(".single_comment").val();

                if ($scope.output.comment.indexOf("@@") != -1) {
                    $scope.output.comment = $scope.output.comment.split("@@");
                    $scope.output.comment = $scope.output.comment[0];
                }

                if ($scope.output.page_type == "atPairwise") {
                    if ($scope.is_comment_updated && $scope.output.value == 0) {
                        $scope.output.value = -2147483648000;
                    }
                   // $scope.save_pairwise($scope.output.value, $scope.output.advantage);
                }

                if ($scope.output.page_type == "atNonPWOneAtATime" && $scope.output.non_pw_type == "mtRatings") {
                    $.each($scope.output.intensities, function (index, intensity) {
                        if (intensity[0] == $scope.output.non_pw_value) {
                            $scope.save_ratings(intensity[2]);
                            return false;
                        }
                    });
                }

                if ($scope.output.page_type == "atNonPWOneAtATime" && $scope.output.non_pw_type == "mtDirect") {
                   // $scope.save_direct($scope.output.non_pw_value);
                } else if ($scope.output.page_type == "atNonPWOneAtATime" && $scope.output.non_pw_type == "mtStep") {
                   // $scope.save_StepFunction($scope.step_input);
                } else if ($scope.output.page_type == "atNonPWOneAtATime" && ($scope.output.non_pw_type == "mtAdvancedUtilityCurve" || $scope.output.non_pw_type == "mtRegularUtilityCurve")) {
                   // $scope.save_utility_curve($scope.utilityCurveInput);
                }

                $timeout(function () {
                    $scope.$apply(function () {
                        $scope.formatted_comment = "";
                    });
                }, 1000);


                $(document).foundation();

                if ($scope.isMobile()) {
                    $("#tt-c-modal").foundation("reveal", "close"); // 11532
                } else {
                    $("#comment-single").removeClass("open");    // 11532
                }
            }
        }

        $scope.close_all_modal = function (type) {
            var index = $scope.active_multi_index;
            $("#mobile-multi-tt-c-modal").foundation("reveal", "close");
            //if (type == "close") {
            //    if ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atAllPairwiseOutcomes") {
            //        $(".multi_comment_" + index).val($scope.output.multi_pw_data[index].Comment);
            //    }
            //    else {
            //        $(".multi_comment_" + index).val($scope.output.multi_non_pw_data[index].Comment);
            //    }
            //}
            //else {
            //    if ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atAllPairwiseOutcomes") {
            //        $scope.output.multi_pw_data[index].Comment = $scope.temp_comments[index];
            //    }
            //    else {
            //        $scope.output.multi_non_pw_data[index].Comment = $scope.temp_comments[index];
            //    }
            //}
        }

        $scope.close_single_modal = function () {
            var index = $scope.active_multi_index;
            $("#tt-c-modal").foundation("reveal", "close");
            $(".single_comment").val($scope.output.comment);
        }


        //info docs
        $scope.edit_info_doc = function (node, node_location, node_type, event) {
            //console.log(angular.element(this).click());
        }
        $scope.page_load = true;
        $scope.collapsed_info_docs = [];
        $scope.info_docs_headings = [];
        $scope.multi_collapsed_info_docs = [];
        $scope.temp_collapsed_info_docs = [true, true, true, true, true];

        $scope.isProcessing = false;
        $scope.$on("render_infodoc_sizes", function (e) {
            $scope.renderInfoDocSizes(5000);
        });

        $scope.renderInfoDocSizes = function(timeOut) {
            if ($scope.isProcessing == false) {
                $scope.isProcessing = true;

                $scope.render_infodoc_sizes();

                $timeout(function () {
                    $scope.isProcessing = false;
                }, timeOut, false);
            }
        }


        $scope.render_infodoc_sizes = function () {
            if ($scope.is_multi) {
                //console.log($scope.output.infodoc_params);
                try {
                    //console.log($scope.output.infodoc_params);
                    if ($scope.output.infodoc_params) {
                        for (i = 0; i < 5; i++) {
                            if ($scope.output.infodoc_params[i]) {
                                var node_params = $scope.output.infodoc_params[i].split("&");
                                var is_close = node_params[0].split("=");

                                var isCollapsed = is_close[1] == 1 ? true : false;

                                $scope.setCollapsedInfoDocsValue(i, isCollapsed);
                            }
                            else {
                                $scope.collapsed_info_docs[i] = false;
                            }
                        }
                    }
                }
                catch (e) {
                }

                $scope.callGetCollapseCookies();


                //if ($scope.output.multi_infodoc_params) {
                //    for (var i = 0; i < $scope.output.multi_infodoc_params.length; i++) {
                //        $scope.multi_collapsed_info_docs[i] = ["", "", "", "", ""];
                //        for (var j = 0; j < 5; j++) {
                //            if ($scope.output.multi_infodoc_params[i][j]) {
                //                var node_params = $scope.output.multi_infodoc_params[i][j].split("&");
                //                //alert(node_params);
                //                var is_close = node_params[0].split("=");
                //                $scope.multi_collapsed_info_docs[i][j] = is_close[1] == 1 ? true : false;
                //            }
                //            else {
                //                $scope.multi_collapsed_info_docs[i][j] = false;
                //            }

                //            //get headings

                //            if (typeof (node_params[3]) != "undefined") {
                //                var heading = node_params[3].split("=");
                //                $scope.info_docs_headings[i] = heading[1];
                //            }
                //            else {
                //                $scope.info_docs_headings[i] = "-1";
                //            }
                //        }
                //    }
                //}
            }
            else {
                try {
                    if ($scope.output.infodoc_params) {
                        for (i = 0; i < 5; i++) {
                            $scope.output.infodoc_params[i] = $scope.output.infodoc_params[i] ? $scope.output.infodoc_params[i] : "c=-1&w=200&h=200";
                            var node_params = $scope.output.infodoc_params[i].split("&");
                            var is_close = node_params[0].split("=");
                            if (is_close[1] == -1) {
                                $scope.temp_collapsed_info_docs[i] = false;
                                if ($scope.is_html_empty($scope.output.parent_node_info)) {
                                    $scope.temp_collapsed_info_docs[0] = true;
                                }

                                if ($scope.is_html_empty($scope.output.first_node_info)) {
                                    $scope.temp_collapsed_info_docs[1] = true;
                                }

                                if ($scope.is_html_empty($scope.output.second_node_info)) {
                                    $scope.temp_collapsed_info_docs[2] = true;
                                }

                                if ($scope.is_html_empty($scope.output.wrt_first_node_info)) {
                                    $scope.temp_collapsed_info_docs[3] = true;
                                }

                                if ($scope.is_html_empty($scope.output.wrt_second_node_info)) {
                                    $scope.temp_collapsed_info_docs[4] = true;
                                }
                            }
                            else {
                                $scope.temp_collapsed_info_docs[i] = is_close[1] == 1 ? true : false;
                            }
                              

                            //get headings
                            if (typeof (node_params[3]) != "undefined") {
                                var heading = node_params[3].split("=");
                                $scope.info_docs_headings[i] = heading[1];
                            }
                            else {
                                $scope.info_docs_headings[i] = "-1";
                            }

                            $scope.setCollapsedInfoDocsValue(i, $scope.temp_collapsed_info_docs[i]);
                        }
                    }
                }
                catch (e) {
                }

                try {
                    if (angular.isUndefined($scope.project_id)) {
                        $scope.project_id = $scope.current_project.project_id;
                    }

                   // alert(JSON.stringify($scope.temp_collapsed_info_docs));
                    $http({
                        method: "POST",
                        url: baseUrl + "pages/Anytime/Anytime.aspx/setSingleCollapsePrivateVar",
                        data: {
                            ProjectID: $scope.project_id,
                            step: $scope.current_step,
                            collapsed_status_list: $scope.temp_collapsed_info_docs
                        },
                        headers: {
                            'Content-Type': 'application/json; charset=utf-8'
                        }
                    }).then(function success(response) {

                        $scope.callGetCollapseCookies();

                    }, function error(response) {
                        // alert("error! got pm!");
                        $scope.collapsed_info_docs = $scope.temp_collapsed_info_docs;
                    });
                }
                catch (e) {
                    $scope.collapsed_info_docs = $scope.temp_collapsed_info_docs;
                }
            }

            if ($scope.infodocSizes == null && !$scope.isAutoSaving) {
                $http.post(
                    $scope.baseurl + "pages/Anytime/Anytime.aspx/getInfoDocSizes", {}
                ).then(function success(response) {
                    var infodoc_sizes = response.data.d;
                    var temp_infodoc_sizes = [];
                    var width = "auto";

                    if (infodoc_sizes == null) {

                        temp_infodoc_sizes[0] = new Array(width, "150px");
                        temp_infodoc_sizes[1] = new Array(width, "150px");
                        temp_infodoc_sizes[2] = new Array(width, "150px");
                        infodoc_sizes = temp_infodoc_sizes;
                    }

                    $scope.infodocSizes = infodoc_sizes;
                    $timeout(function() {
                        $scope.change_info_doc_image_size();
                    }, 100);
                });
            } else if ($scope.infodocSizes != null && !$scope.isAutoSaving) {
                $scope.change_info_doc_image_size();
            }
          
           
            //mobile - timeout because hide-for-large class prevent it to load at first
            //$timeout(function () {
            //    for (i = 3; i < 6; i++) {
            //        $(".tt-resizable-panel-" + i).css("height", "100%");
            //        $(".tt-resizable-panel-" + i).resizable({
            //            containment: ".tt-2-cols",
            //            start: function (event, ui) {
            //                var element = ui.element;
            //                temp_height = ui.size.height;
            //                info_docs_resize_event = true;
            //            },
            //            stop: function (event, ui) {
            //                var element = ui.element;
            //                var index = element.attr("data-index")
            //                var height = ui.size.height;
            //                var width = ui.size.width;
            //            }
            //        });
            //    }
            //}, 200);
        }

        $scope.show_framed_info_docs = function (shouldShow) {
            if ($scope.is_AT_owner || !$scope.isMobile()) {
                if (typeof shouldShow == "undefined") {
                    $scope.output.framed_info_docs = !$scope.output.framed_info_docs;
                } else {
                    $(".editable-content-height").addClass("hide");
                    $scope.output.framed_info_docs = shouldShow;

                    $timeout(function() {
                        $scope.scroll_to_active_index();
                    }, 50);
                }
                
                $scope.$parent.framed_info_docs = $scope.output.framed_info_docs;
                if ($scope.output.framed_info_docs) {
                    $scope.callResizeFrameImagesProportionally(50, "show_framed_info_docs");
                }

                if ($scope.is_AT_owner) {
                    $http({
                        method: "POST",
                        url: baseUrl + "pages/Anytime/Anytime.aspx/SetShowFramedInfodocsMobile",
                        data: { value: $scope.output.framed_info_docs }
                    }).then(function success(response) {

                    }, function error(response) {
                        console.log(response);
                    });
                }
            }
        }

        $scope.$on("show_framed_info_docs", function (e) {
            $scope.show_framed_info_docs();
        });

        $scope.setAutoFitInfoDocImages = function () {
            $scope.output.autoFitInfoDocImages = !$scope.output.autoFitInfoDocImages;
            $scope.$parent.autoFitInfoDocImages = $scope.output.autoFitInfoDocImages;

            $timeout(function () {
                $scope.change_info_doc_image_size();
            }, 50);
            //ajax
            //do ajax for update judgement backend
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/SetAutoFitInfoDocImages",
                data: {
                    value: $scope.output.autoFitInfoDocImages
                }
            }).then(function success(response) {
                
            }, function error(response) {
                console.log(response);
            });
        }

        $scope.$on("setAutoFitInfoDocImages", function (e) {
            $scope.setAutoFitInfoDocImages();
        });

        $scope.callGetCollapseCookies = function() {
            if ($scope.is_multi) {
                //$scope.get_collapse_cookies("pw-nodes");
                //$scope.get_collapse_cookies("pw-wrt-nodes");
                if ($scope.output.page_type == "atPairwise") {
                    $scope.get_collapse_cookies("parent-node");
                }
                $scope.get_collapse_cookies("pw-nodes");

                if ($scope.output.non_pw_type != "mtDirect" && $scope.output.non_pw_type != "mtRatings") {
                    $scope.get_collapse_cookies("pw-wrt-nodes");
                }

                if ($scope.output.page_type != "atPairwise") {
                    $scope.get_collapse_cookies("pw-nodes");
                }
            } else {
                try {
                    if ($scope.output.page_type == "atPairwise") {
                        $scope.get_collapse_cookies("parent-node");
                    }
                    $scope.get_collapse_cookies("pw-nodes");

                    if ($scope.output.non_pw_type != "mtDirect" && $scope.output.non_pw_type != "mtRatings" && $scope.output.non_pw_type != "mtStep" && $scope.output.non_pw_type != "mtRegularUtilityCurve") {
                        $scope.get_collapse_cookies("pw-wrt-nodes");
                    }

                    if ($scope.output.page_type != "atPairwise") {
                        $scope.get_collapse_cookies("pw-nodes");
                    }
                    // alert("got cookies!");
                } catch (e) {
                    $scope.collapsed_info_docs = $scope.temp_collapsed_info_docs;
                }
            }
        }

        $scope.change_info_doc_image_size = function () {
            if ($scope.infodocSizes != null) {
                for (var i = 0; i < $scope.infodocSizes.length; i++) {
                    var width = "auto";

                    $(".tt-resizable-panel-" + i).parent().attr("style", "");
                    if ($scope.infodocSizes[i].length < 1) {
                        $(".tt-resizable-panel-" + i).css("height", "100%");
                    } else {
                        $(".tt-resizable-panel-" + i).css("width", width);

                        if (!($scope.isMobile() && $scope.is_multi)) {
                            var height = $scope.infodocSizes[i][1] == null ? 50 : $scope.infodocSizes[i][1];
                            $(".tt-resizable-panel-" + i).css("height", height);
                        }

                        //AR: fix for sticky infodoc size
                        //$(".tt-resizable-panel-" + i).css("height", infodoc_sizes[i][1]);

                        //if (!($scope.isMobile() && $scope.is_multi)) {
                        //    $(".tt-resizable-panel-" + i).css("height", $scope.infodocSizes[i][1]);

                        //    $(".tt-resizable-panel-" + i + " img").each(function() {
                        //        //fix for case Case 13436
                        //        if ($(this).height() > 100) {
                        //            var imgHeight = $scope.infodocSizes[i][1] < 100 ? 101 : ($scope.infodocSizes[i][1] > 270 ? 270 : $scope.infodocSizes[i][1]) - 10;
                        //            //$(this).css("height", imgHeight);
                        //        }
                        //    });
                        //}
                    }

                    $scope.add_resizable_info_doc_event_handler(i);
                }

                $scope.callResizeFrameImagesProportionally(100, "change_info_doc_image_size");
            }

            hide_loading_modal();
        }

        //event handler for resizable info docs
        $scope.add_resizable_info_doc_event_handler = function (index) {
            try {
                $(".tt-resizable-panel-" + index).resizable({
                    handles: "s",
                    start: function(event, ui) {
                        var element = ui.element;
                        temp_height = ui.size.height;
                    },
                    resize: function(event, ui) {
                        var element = ui.element;
                        var index = element.attr("data-index");
                        var height = ui.size.height;
                        var width = ui.size.width;
                        height = height > 270 ? 270 : height;
                        $(".tt-resizable-panel-" + index).css("height", height);

                        //$scope.setMaxImageSizeFromFrames();

                        $scope.infodocSizes[index][0] = width;
                        $scope.infodocSizes[index][1] = height;

                        //if ($scope.output.autoFitInfoDocImages) {
                        $scope.callResizeFrameImagesProportionally(50, "resizable_info_doc_event: resize");
                        //}

                        //if ($scope.output.autoFitInfoDocImages) {
                        //    $(".tt-resizable-panel-" + index + " img").each(function () {
                        //        //fix for case Case 13436
                        //        if ($(this).height() > 100) {
                        //            var imgHeight = height > 110 ? height - 10 : 101;
                        //            $(this).css("height", imgHeight);
                        //        }
                        //    });
                        //}
                    },
                    stop: function(event, ui) {
                        var element = ui.element;
                        var index = element.attr("data-index");
                        var height = ui.size.height;
                        var width = ui.size.width;

                        if ($scope.is_AT_owner) {
                            $scope.set_infodoc_sizes(width, height, index);
                        }
                    }
                });
            } catch (e) {
            }
        }

        $scope.isGoodToCallResizeFrameImages = false;
        $scope.callResizeFrameImagesProportionally = function (timeOut, message, isRepeat) {
            if ($scope.isGoodToCallResizeFrameImages) {
                //if (angular.isDefined(message)) {
                //    console.log("Resizing frame images from " + message);
                //}

                ////var infoDocImage = $scope.getHtml($scope.multi_data[$scope.active_multi_index].InfodocLeft);
                ////infoDocImage = $scope.getHtml($scope.multi_data[$scope.active_multi_index].InfodocRight);

                if (angular.isUndefined(isRepeat) && $scope.callResizeFrameImagesCounter > 0) {
                    $scope.callResizeFrameImagesCounter = 0;
                }

                $timeout(function () {
                    $scope.resizeFrameImagesProportionally();
                }, timeOut, false);
            }
        }

        $scope.callResizeFrameImagesCounter = 0;
        $scope.callResizeFrameImagesRepeater = function() {
            if ($scope.callResizeFrameImagesCounter < 3) {
                $scope.callResizeFrameImagesCounter++;
                $scope.callResizeFrameImagesProportionally(200 * $scope.callResizeFrameImagesCounter, "Repeating " + $scope.callResizeFrameImagesCounter + " times", true);
            }
        }

        $scope.resizeFrameImagesProportionally = function() {
            if ($scope.infodocSizes != null && $scope.output.pairwise_type == "ptVerbal" && ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atPairwise")) {
                var multiplier = 1;
                var loopCounter = 0;
                var container;
                var isWidthSmaller = false;

                if ($scope.is_multi) {
                    container = $(".other-info-doc-height");
                    container = (container.length == 0 || container.hasClass("hide")) ? ".InfoDocsParentDiv" : ".other-info-doc-height";
                } else {
                    container = $(".pairwise-desktop");
                    container = container.length == 0 ? ".tg-legend .single-pw-verbal" : ".pairwise-desktop";
                }
                
                ////console.log("container: " + container);

                if ($scope.output.autoFitInfoDocImages) {
                    var maxImageSize = 0;
                    var frameWidth = 0, frameHeight = 0, maxImageWidth = 0, maxImageHeight = 0;
                    //console.log("maxImageSize: " + maxImageSize);

                    var currentFrame;
                    for (loopCounter = 0; loopCounter < 5; loopCounter++) {
                        $(container + " .box-" + loopCounter + " img").each(function (index) {
                            ////console.log("loop counter: " + loopCounter + ", index: " + index + ", this.naturalWidth: " + this.naturalWidth + ", this.naturalHeight: " + this.naturalHeight);
                            maxImageWidth = this.naturalWidth > maxImageWidth ? this.naturalWidth : maxImageWidth;
                            maxImageHeight = this.naturalHeight > maxImageHeight ? this.naturalHeight : maxImageHeight;

                            currentFrame = $(container + " .box-" + loopCounter);
                            if (currentFrame.length > 0 && currentFrame.width() > 100 && frameHeight == 0 && maxImageHeight > 0) {
                                frameWidth = currentFrame.width();
                                frameHeight = currentFrame.height() - 1;
                            }

                            ////console.log("frameWidth: " + currentFrame.width() + ", frameHeight: " + currentFrame.height());
                        });
                    }

                    //Setting maxImageWidth and maxImageWidth of all the info doc images.
                    if (angular.isDefined($scope.maxImageWidth) && angular.isDefined($scope.maxImageHeight)) {
                        maxImageWidth = $scope.maxImageWidth > maxImageWidth ? $scope.maxImageWidth : maxImageWidth;
                        maxImageHeight = $scope.maxImageHeight > maxImageHeight ? $scope.maxImageHeight : maxImageHeight;
                    }

                    var widthMultiplier = (frameWidth > 0 && maxImageWidth > 0 && maxImageWidth > frameWidth) ? (frameWidth / maxImageWidth) : 1;
                    var heightMultiplier = (frameHeight > 0 && maxImageHeight > 0 && maxImageHeight > frameHeight) ? (frameHeight / maxImageHeight) : 1;

                    isWidthSmaller = widthMultiplier < heightMultiplier;

                    if (isWidthSmaller) {
                        maxImageSize = maxImageSize > maxImageWidth ? maxImageSize : maxImageWidth;
                        multiplier = widthMultiplier;
                    } else {
                        maxImageSize = maxImageSize > maxImageHeight ? maxImageSize : maxImageHeight;;
                        multiplier = heightMultiplier;
                    }
                    
                    ////console.log("new maxImageSize: " + maxImageSize + ", multiplier: " + multiplier + ", widthMultiplier: " + widthMultiplier + ", heightMultiplier: " + heightMultiplier);
                }

                var callRepeater = false;

                for (loopCounter = 0; loopCounter < 5; loopCounter++) {
                    $(container + " .box-" + loopCounter + " img").each(function (index) {
                        ////console.log("2nd loop counter: " + loopCounter + ", image index: " + index + ", this.naturalHeight: " + this.naturalHeight + ", this.naturalWidth: " + this.naturalWidth);

                        var newImageSize = "auto";
                        callRepeater = (this.naturalWidth == 0 || this.naturalHeight == 0);

                        if (multiplier == 1) {
                            newImageSize = ((this.naturalWidth * multiplier).toFixed(2) + "px");
                            $(this).css("max-width", newImageSize);
                            $(this).css("height", "auto");
                        } else {
                            if (isWidthSmaller) {
                                newImageSize = ((this.naturalWidth * multiplier).toFixed(2) + "px");
                                $(this).css("width", newImageSize);
                                $(this).css("height", "auto");
                            } else {
                                newImageSize = ((this.naturalHeight * multiplier).toFixed(2) + "px");
                                $(this).css("width", "auto");
                                $(this).css("height", newImageSize);
                            }
                        }
                        
                        ////console.log("newImageSize: " + newImageSize + ", multiplier: " + multiplier);
                    });
                }

                if (callRepeater) {
                    $scope.callResizeFrameImagesRepeater();
                }
            }
        }

        $scope.set_infodoc_sizes = function (width, height, index) {

            var is_multi = $scope.is_multi;

            if ($scope.output.page_type != "atPairwise") {
                is_multi = true;
            }

            $http.post(
                    $scope.baseurl + "pages/Anytime/Anytime.aspx/setInfoDocSizes",
                    JSON.stringify({
                        index: index,
                        width: width,
                        height: height,
                        is_multi: is_multi
                    })
                    )
                .then(function success(response) {
                    // alert("done!");
                    //if (!$scope.is_multi) {
                    //    $scope.get_pipe_data($scope.current_step);
                    //}
                    //else {
                    //    // alert($scope.active_multi_index);
                    //    $scope.get_pipe_data($scope.current_step, null, null, $scope.active_multi_index);
                    //}
                });
        }

        //End of Info docs


        //save ratings
        $scope.ratings_data = [];


        $scope.get_ratings_data = function () {
            $scope.refreshed = true;
            if ($scope.output.is_direct) {
                $scope.ratings_data = [$scope.output.non_pw_value, "Direct Value", "-2", "0"];
            }
            else {
                $.each($scope.output.intensities, function (index, intensity) {
                    if (intensity[0] == $scope.output.non_pw_value) {
                        $scope.ratings_data = intensity;
                        return false;
                    }
                });
            }
        }


        $scope.save_ratings = function (value) { //add comment attr here later
           // var loadingMessage = stringResources.yellowLoadingIcon.save;
            //if ($scope.output.is_auto_advance && (!$scope.is_comment_updated && !(value < 0)))
               // loadingMessage = stringResources.yellowLoadingIcon.saveAA;
            var direct_value = "";
            if (value.indexOf("*") >= 0 || value == -2) {
                $scope.output.is_direct = true;
                direct_value = value.replace("*", "");
                $scope.ratings_data = [direct_value, "Direct Value", "-2", "0"];
                value = -2;

                $scope.singleRatingTempDirectValue = direct_value >= 0 ? parseFloat(direct_value) : "";
                $scope.output.non_pw_value = direct_value >= 0 ? parseFloat(direct_value) : "";
                console.log("singleRatingTempDirectValue: " + $scope.singleRatingTempDirectValue + ", non_pw_value: " + $scope.output.non_pw_value);
                $("#direct_value_input").focus();
                $("#-2").prop("checked", true);
            }
            else {
                $("#InputSingleRatingDirect").val("");

                $.each($scope.output.intensities, function (index, intensity) {
                    if (intensity[2] == value) {
                        $scope.ratings_data = intensity;
                        $scope.output.non_pw_value = intensity[0] >= 0 ? parseFloat(intensity[0]) : intensity[0];
                        $scope.output.is_direct = false;
                        return false;
                    }
                });

                if (value < 0) {
                    $("#direct_value_input").val("");
                }
            }

            if ($scope.output.is_auto_advance && !$scope.output.is_direct && value != -1) {
                $scope.save_ratings_on_next();

                $timeout(function () {
                    $(".next_step").removeClass("back-pulse");
                }, 200);
            } else{

                $timeout(function () {
                    $scope.$apply(function () {
                        $scope.ratings_data = $scope.ratings_data;
                       // //console.log(  $scope.ratings_data );
                    });
                }, 2000);
            }

            $scope.$parent.disableNext = $scope.output.pipeOptions.dontAllowMissingJudgment && (value == -1);
            $scope.output.IsUndefined = (value == -1);
            $scope.$parent.output.IsUndefined = (value == -1);

            if (value == -1) {
                $(".next_step").removeClass("back-pulse");
            } else {
                $(".next_step").addClass("back-pulse");
            }
           
            //do ajax for update judgement backend
            //$scope.$parent.runProgressBar(2, 0, 30, loadingMessage, false);
            
        }

        $scope.save_ratings_on_next = function () {
            
            var direct_value = "";
            var value = $scope.ratings_data[2];

            if ($scope.output.is_direct) {
                direct_value = $scope.ratings_data[0];
                value = -2;
            }

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/SaveRatings",
                data: {
                    step: $scope.current_step,
                    RatingID: value,
                    sComment: $scope.output.comment,
                    intensities: $scope.output.intensities,
                    UserID: 0,
                    DirectValue: direct_value
                }
            }).then(function success(response) {
                $scope.is_saving_judgment = true;
                $scope.$parent.footer_up();
                $timeout(function () {
                    if ($scope.is_comment_updated || value < 0 || (!$scope.output.is_auto_advance && $scope.isAutoSaving)) {
                        //if judgment is reset or comment updated
                        var timeOutValue = $scope.isAutoSaving ? 300 : 0;
                        $timeout(function () {
                            $scope.get_pipe_data($scope.current_step);
                        }, timeOutValue, false);
                    }
                    else {
                        if ($scope.output.is_auto_advance) {
                            $scope.is_saving_judgment = true;
                            $scope.get_pipe_data(parseInt($scope.current_step) + 1);
                        }
                        else {
                            $scope.is_saving_judgment = false;
                            $timeout(function () {
                                $(".tt-loading-icon-wrap").css("display", "none");
                            }, 1500);
                        }
                    }
                }, 200);
            }, function error(response) {
                //console.log(response);
                alert(JSON.stringify(response));
                window.location.href = baseUrl;
            });
        }

        $scope.singleRatingTempDirectValue = "";
        $scope.get_direct_value = function (value) {
            $scope.cancelDirectValueTimer();
            var timeOutMs = value == 0 ? 1500 : 800;

            $scope.directValueTimer = $timeout(function () {
                if ((value < 0 && value > 1) || value == undefined) {
                    $scope.output.non_pw_value = "";
                    return false;
                }

                $scope.save_ratings("*" + value);
            }, timeOutMs);
        }

        $scope.cancelDirectValueTimer = function () {
            if (angular.isDefined($scope.directValueTimer)) {
                $timeout.cancel($scope.directValueTimer);
                $scope.directValueTimer = undefined;
            }
        }
        //end ratings

        //multi ratings
        $scope.multi_direct_value = [];
        $scope.selected_multi_data = [];
        $scope.multi_ratings_row = [];

        $scope.add_multi_ratings_values = function (rating_id, index, $event, value) {
            //console.log("add_multi_ratings_values. rating_id: " + rating_id + ", index: " + index + ", value: " + value);
            if ($event) {
                $event.stopPropagation(); //for stopping parent element ng-click event
            }

            $("#ratings-dropdown" + index + " .hidden-item-wrap").slideUp();
            $(".cdd_" + index).hide();
            $(".arrow_" + index).show();

            if ($scope.active_multi_index !== index) {
                $(".multi-ratings-list .selected").removeClass("selected"); //11690
            }
            
            $scope.active_multi_index = index;
            var is_direct = false;
            var is_not_rated = false;

            if (rating_id >= 0) {
                $scope.output.multi_non_pw_data[$scope.active_multi_index].RatingID = rating_id;
            }
            else {
                if (rating_id == -2) {
                    if (value >= 0 && value <= 1) {
                        $scope.output.multi_non_pw_data[$scope.active_multi_index].RatingID = -1;
                        $scope.output.multi_non_pw_data[$scope.active_multi_index].DirectData = $scope.multi_direct_value[$scope.active_multi_index];
                        is_direct = true;
                    } else {
                        $scope.output.multi_non_pw_data[$scope.active_multi_index].RatingID = -1;
                        $scope.output.multi_non_pw_data[$scope.active_multi_index].DirectData = -1;
                        $scope.multi_direct_value[$scope.active_multi_index] = "";
                        is_not_rated = true;
                        is_direct = true;
                    }
                }
                else {
                    $scope.output.multi_non_pw_data[$scope.active_multi_index].RatingID = -1;
                    $scope.output.multi_non_pw_data[$scope.active_multi_index].DirectData = -1;
                    $scope.multi_direct_value[$scope.active_multi_index] = "";
                    is_not_rated = true;
                }
            }

            $timeout(function () {
                $scope.$apply(function () {
                    if (is_not_rated) {
                        $scope.multi_direct_value[$scope.active_multi_index] = "";
                    }

                    $scope.multi_ratings_row[$scope.active_multi_index] = $scope.get_multi_ratings_data(rating_id, $scope.multi_direct_value[$scope.active_multi_index], $scope.active_multi_index);
                    var multiData = $scope.multi_ratings_row;

                    if (rating_id >= 0) {
                        $scope.multi_direct_value[$scope.active_multi_index] = "";
                        var i = $scope.active_multi_index;
                        $scope.multi_direct_value[i] = $scope.multi_ratings_row[i][0] != -1 ? $scope.multi_ratings_row[i][0] : $scope.multi_direct_value[i];
                    }

                    if (is_direct == false && !is_not_rated) {
                        //console.log("line# 3828");
                        //Adding flash to judgment row when user enters judgment
                        if ($scope.active_multi_index >= 0) {
                            $("#multi-row-" + $scope.active_multi_index).addClass("judgment-flash");
                        }

                        $scope.select_multi_ratings_row($scope.active_multi_index + 1, true);
                    }

                    multiData = $scope.multi_direct_value;
                    $scope.checkMultiValue($scope.multi_ratings_row, 1);
                    $scope.active_multi_index = index;
                });
            }, 300);
        }

        $scope.unselect_row = function (index) {
           // $scope.activate_first_row = index;
        }

        $scope.select_multi_ratings_row = function (index, judgmentAdded) {
            //console.log("select_multi_ratings_row. index: " + index + ", active_multi_index: " + $scope.active_multi_index);
            //console.log("multi_direct_value[active_multi_index]: " + $scope.multi_direct_value[$scope.active_multi_index]);
            //console.log("multi_direct_value[index]: " + $scope.multi_direct_value[index]);

            var timeOutValue = $scope.isMobile() ? 300 : 1000;
            $timeout(function () {
                $(".multi-ratings-row").removeClass("judgment-flash");  //Removing flash from judgment row after desired time
            }, timeOutValue);

            if (index > $scope.output.multi_non_pw_data.length - 1) {
                index = 0;
                $scope.activate_first_row = true;
            }
            else {
                $scope.activate_first_row = false;
                $scope.active_multi_index = index;

                //Change row selection when judgment flash stops
                $timeout(function () {
                    $(".multi-ratings-row").removeClass("selected");
                    $("#multi-row-" + index).addClass("selected");

                    $(".multi-ratings-row").addClass("fade-fifty");
                    $("#multi-row-" + index).removeClass("fade-fifty");
                }, timeOutValue);

                $scope.selected_multi_data[index] = $scope.output.multi_non_pw_data[index];
            }

            //Hiding value from direct input field when value is not direct
            var directValue = $scope.selected_multi_data[index].RatingID == -1 && $scope.selected_multi_data[index].DirectData != 1 ? $scope.multi_direct_value[index] : "";
            if (directValue == "") {
                $("#multiRatingsDirectValue").css("color", "white");    //Setting color to white on direct input filed so that user doesn't see the value
                $timeout(function () {
                    $("#multiRatingsDirectValue").val(directValue); //Removing value from direct input field after a timeout
                    $("#multiRatingsDirectValue").css("color", ""); //Removing color from direct input field
                }, 1000);
            }

            if (index > 0) {
                //Changing selected row after desired time so that user can see flash on judgment row
                $timeout(function () {
                    $scope.set_multi_index(index);
                    $scope.callGetCollapseCookies();
                }, timeOutValue - 200);
            }

            if ($scope.activate_first_row) {
                angular.element(".next_step").addClass("back-pulse");
            }
            else {
                $scope.scroll_to_index = $scope.active_multi_index - 2;
                
                if ($scope.scroll_to_index <= 1) {
                    //do not scroll
                }
            }

            if (!(judgmentAdded && $scope.isMobile())) {
                $scope.selectNextMultiRatingsRow(index);
            }
        }

        $scope.selectNextMultiRatingsRow = function (nextIndex) {
            $timeout(function () {
                //$scope.$apply(function () {
                nextIndex = $scope.activate_first_row ? 0 : nextIndex;
                var selecteRatings = $("#multi-row-" + nextIndex);
                if (nextIndex >= 0 && selecteRatings.length > 0 && selecteRatings.outerHeight() >= 40) {
                    if ($scope.isMobile()) {
                        //var bodyScrollLeft = $(document).height() - ($(window).height() + $(document).scrollTop());
                        var elementTop = selecteRatings.offset().top;
                        if ($(document).scrollTop() < elementTop) {
                            var stickyInfo;
                            if (nextIndex >= $scope.output.multi_non_pw_data.length - 2) {
                                selecteRatings[0].scrollIntoView({ behavior: "smooth", block: "start", inline: "center" });
                            } else if (nextIndex == 0 && ($(window).height() - (elementTop + selecteRatings.height())) < 0) {
                                stickyInfo = $(".tt-sticky-element");
                                if (stickyInfo.length > 0) {
                                    $("html, body").animate({ scrollTop: "+=" + (stickyInfo.height()) }, 300);
                                    $timeout(function () {
                                        $("html, body").animate({ scrollTop: "-=" + (stickyInfo.height() + 3) + "px" }, 300);
                                    }, 350);
                                }
                            } else {
                                var scrollTo = elementTop - $(document).scrollTop();
                                stickyInfo = $(".tt-sticky-element");

                                if (stickyInfo.length > 0) {
                                    if (stickyInfo.height() < 80 && nextIndex > 0) {
                                        var prevElement = $("#multi-row-" + (nextIndex - 1));
                                        scrollTo = prevElement.offset().top - $(document).scrollTop();
                                    }

                                    scrollTo -= Math.round(stickyInfo.height());

                                    if (nextIndex == 0) {
                                        var originalInfoDoc = $(".original-info-doc");
                                        if (originalInfoDoc.length > 0) {
                                            scrollTo -= Math.round(originalInfoDoc.height());
                                        }
                                    }
                                }

                                scrollTo -= 30;
                                if (scrollTo > 5) {
                                    $("html, body").animate({ scrollTop: "+=" + scrollTo + "px" }, 0);
                                }
                            }

                            if (selecteRatings.length > 0) {
                                $timeout(function() {
                                    $("#mobile-multi-tt-c-modal.open").css("top", $(document).scrollTop());
                                }, 300);
                            }
                        }
                    } else {
                        var elementOffsetTop = selecteRatings.offset().top;
                        var elementHeight = selecteRatings.outerHeight();
                        var containerOffsetTop = $(".multi-loop-wrap").offset().top;
                        var containerHeight = $(".multi-loop-wrap").outerHeight();
                        var elementPosition = (elementOffsetTop + elementHeight) - (containerOffsetTop + containerHeight);

                        if (elementOffsetTop < containerOffsetTop) {
                            //When element at top and it can be seen partially
                            //selecteRatings[0].scrollIntoView({ behavior: "smooth", block: "start", inline: "center" }); //not supported by IE and Edge
                            selecteRatings[0].scrollIntoView(true);
                        } else if (elementPosition > 0) {
                            //When element at bottom and it can be seen partially OR it can not be seen
                            //selecteRatings[0].scrollIntoView({ behavior: "smooth", block: "end", inline: "center" }); //not supported by IE and Edge
                            selecteRatings[0].scrollIntoView(false);
                        }
                    }
                }
                //});
            }, 500);
        }

        $scope.get_multi_ratings_data = function (rating_id, direct_value, index) {
            $scope.refreshed = true;

            if (rating_id < 0) {
                if (direct_value > 0) {
                    return [direct_value, "Direct Value"];
                }
                else {
                    return ["", "Not Rated"];
                }
            }
            else {
                for (var i = 0; i < $scope.output.multi_non_pw_data[$scope.active_multi_index].Ratings.length; i++) {
                    if ($scope.output.multi_non_pw_data[index].Ratings[i].ID == rating_id) {
                        return [$scope.output.multi_non_pw_data[index].Ratings[i].Value, $scope.output.multi_non_pw_data[index].Ratings[i].Name];
                    }
                }
            }
        }

        $scope.save_multi_ratings_on_next = function () {
            var multivalues = [];
            var updatedIntensities = [];    //for intensity description

            for (i = 0; i < $scope.output.multi_non_pw_data.length; i++) {
                try {
                    var vals = [];
                    vals[0] = $scope.output.multi_non_pw_data[i].RatingID;
                    vals[1] = $scope.output.multi_non_pw_data[i].DirectData;
                    vals[2] = $scope.output.multi_non_pw_data[i].Comment;
                    multivalues[i] = vals;

                    var ratings = $scope.output.multi_non_pw_data[i].Ratings;
                    var newRatings = [];
                    for (var j = 0; j < ratings.length; ++j) {
                        newRatings[j] = [];
                        newRatings[j][0] = ratings[j].GuidID;
                        newRatings[j][1] = ratings[j].Comment;
                        //newRatings[j][2] = ratings[j].ID.toString();
                        //newRatings[j][3] = ratings[j].Value.toString();
                        //newRatings[j][4] = ratings[j].Name;
                    }

                    updatedIntensities[i] = newRatings;
                }
                catch (e) {

                }

            }
            
            //do ajax for update judgement backend
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/SaveMultiRatingsData",
                data: {
                    step: $scope.current_step,
                    multivalues: multivalues,
                    intensities: updatedIntensities
                }
            }).then(function success(response) {
                var loadingMessage = stringResources.yellowLoadingIcon.multiSave;
                $scope.$parent.runProgressBar(2, 0, 100, loadingMessage, true);
                if ($scope.is_comment_updated || $scope.isAutoSaving) {
                    $scope.is_saving_judgment = true;
                    
                    var timeOutValue = $scope.isAutoSaving ? 300 : 0;
                    $timeout(function () {
                        $scope.get_pipe_data($scope.current_step, null, null, $scope.active_multi_index);
                    }, timeOutValue, false);
                }
                $scope.$parent.footer_up();
            }, function error(response) {
                //console.log(response);
            });
        }

        $scope.getIntensityDescription = function (intensityName, intensityDescription) {
            var description;
            var maxLength = $scope.output.showPriorityAndDirect ? 30 : 60;

            if (intensityDescription.length > (maxLength - intensityName.length)) {
                description = intensityDescription.substring(0, maxLength - intensityName.length) + "...";
            } else {
                description = intensityDescription;
            }

            description = description.length > 0 ? " - " + description : description;

            return description;
        }

        $scope.editIntensityIndex = 0;
        $scope.changeEditIntensityIndex = function (index) {
            $scope.editIntensityIndex = index;
        }

        $scope.updateIntensityDescription = function () {
            //$(document).foundation();

            if ($scope.isMobile()) {
                //$("#tt-c-modal").foundation("reveal", "close"); // 1153save_multi_ratings_on_next2
            } else {
                if ($scope.is_multi) {
                    for (var i = 0; i < $scope.output.multi_non_pw_data.length; ++i) {
                        if ($scope.output.multi_non_pw_data[i].Ratings[$scope.editIntensityIndex].GuidID == $scope.output.multi_non_pw_data[$scope.active_multi_index].Ratings[$scope.editIntensityIndex].GuidID) {
                            $scope.output.multi_non_pw_data[i].Ratings[$scope.editIntensityIndex].Comment = $scope.output.multi_non_pw_data[$scope.active_multi_index].Ratings[$scope.editIntensityIndex].Comment;
                        }
                    }
                }

                $("#ratings-description").removeClass("open");    // 11532
            }
        }
        //end of multi ratings

       
        //view status
        $scope.$on("set_view_status", function (e) {
            $scope.set_view_status();
        });

        $scope.set_view_status = function () {
            $scope.is_AT_owner = !$scope.$parent.pm_switch ? 1 : 0;
            $scope.$parent.is_AT_owner = $scope.is_AT_owner;
        }

        //auto advance
        $scope.$on("set_auto_advance", function (e) {
            $scope.set_auto_advance();
        });

        $scope.set_auto_advance = function () {
            try {
                $("#AutoAdvanceModal").foundation("reveal", "close");
            }
            catch (e) {

            }

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/setAutoAdvance",
                data: {
                    value: $scope.output.is_auto_advance
                }
            }).then(function success(response) {
                try {
                    $scope.output.is_auto_advance = value;
                    $scope.$parent.output.is_auto_advance = value;
                    // alert($scope.$parent.output.is_auto_advance);
                }
                catch (e) {

                }
                //console.log(response);
            }, function error(response) {
                //console.log(response);
            });
        }

        $scope.$on("set_infodoc_mode", function (e) {
            $scope.set_infodoc_mode();
        })

        $scope.set_infodoc_mode = function () {

            $scope.$parent.orig_infodoc_setting = $scope.output.is_infodoc_tooltip;
            $scope.render_infodoc_sizes();
            $scope.show_framed_info_docs($scope.output.is_infodoc_tooltip ? false : $scope.output.framed_info_docs);

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/setInfodocMode",
                data: {
                    value: $scope.output.is_infodoc_tooltip ? 1 : 0
                }
            }).then(function success(response) {
                try {
                    $scope.output.is_infodoc_tooltip = $scope.output.is_infodoc_tooltip;
                    is_info_tooltip = $scope.output.is_infodoc_tooltip;

                }
                catch (e) {

                }
                //console.log(response);
            }, function error(response) {
                //console.log(response);
            });

            $timeout(function () {
                $scope.reflow_equalizer();
                $scope.CheckMultiHeight("multi-loop-wrap", "80");
                $scope.CheckMultiHeight("many-alternatives", "210");
                $scope.CheckMultiHeight("jj-wrap-fix", "50");
                $scope.CheckMultiHeight("multi-loop-wrap-local-results", "500");
                //$scope.CheckMultiHeight("tt-rating-scale", "100");
            }, 500);
        }

        //pairwise verbal 
        $scope.is_comment_updated = false;

        $scope.save_pairwise_on_next = function (auto_advance) {
            var temp_auto_advance = $scope.output.auto_advance;
            if (typeof (auto_advance) != "undefined") {
                temp_auto_advance = auto_advance;
            }

            $timeout.cancel($scope.savePwTimeout);

            var value = $scope.output.value == 0 ? -2147483648000 : $scope.output.value;
            var advantage = $scope.output.advantage;
            
           // alert(value + "==" + advantage + " on next ");
            $scope.refreshed = false;
            $scope.refreshed = true;
            $scope.is_saving_judgment = true;

           
            //do ajax for update judgement backend
            $scope.current_step = $scope.output.current_step;

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/SavePairwise",
                data: {
                    step: $scope.current_step,
                    value: value,
                    advantage: advantage,
                    comments: $scope.output.comment,
                    userId: 0
                }
            }).then(function success(response) {
               //$scope.$parent.runProgressBar(2, 0, 100, loadingMessage, false);
                
                angular.element(".load-canvas-gif").hide();
                var fextreme = false;
                if (!$scope.output.extremeMessage && $scope.output.value == 9 && !temp_auto_advance && $scope.output.pairwise_type != "ptGraphical") {
                    //$timeout(function () {
                    $scope.output.extremeMessage = true;
                    fextreme = true;
                    $scope.output.pipeWarning = response.data.d;
                    if (parseFloat(value) > 0) {
                        $scope.$parent.disableNext = false;
                    }
                    //$("#warningModal").foundation("reveal", "open");
                    //}, 0);
                }

                $timeout(function () {
                    $(".next_step").removeClass("back-pulse");
                }, 2000);
                if (!$scope.$parent.is_under_review && ($scope.is_comment_updated || value < 0)) { //if updated comment or judgment deleted
                    $scope.get_pipe_data($scope.current_step);
                }
                else {
                    $timeout.cancel($scope.savePwTimeout);
                    if ($scope.$parent.is_under_review && temp_auto_advance) {
                        if ($scope.satisfactory && !$scope.matrixReview)
                            $scope.show_inconsistency_view();
                        $scope.is_saving_judgment = true;
                        $scope.get_pipe_data($scope.previous_cluster_step);
                        $scope.$parent.show_check_box = false;
                    }
                    else {
                        if (temp_auto_advance) {
                            if ($scope.output.pairwise_type == "ptVerbal") {
                                $scope.output.value = value;
                                $scope.savePwTimeout = $timeout(function () {
                                    $scope.output.advantage = 2;
                                    $(".pm-bars").removeClass("active-selected");
                                    $(".pm-bars").removeClass("lft-selected");
                                    $(".pm-bars").removeClass("rgt-selected");
                                    $scope.is_saving_judgment = true;
                                    $scope.get_pipe_data($scope.current_step + 1);
                                }, 1500);
                            }
                            else { //if graphical
                                $scope.savePwTimeout = $timeout(function () {
                                    $scope.get_pipe_data($scope.current_step); //dont auto advance
                                }, 1500);
                            }
                        }
                        else {
                            $scope.savePwTimeout = $timeout(function () {
                                if ($scope.output.pairwise_type == "ptGraphical") {
                                    $scope.is_saving_judgment = true;
                                    $scope.get_pipe_data($scope.current_step);
                                    return false;
                                }
                                $scope.$apply(function () {
                                    if (!fextreme) {
                                        $scope.is_saving_judgment = true;
                                        $scope.get_pipe_data($scope.current_step);
                                    }


                                    $scope.$parent.footer_up();
                                });

                            }, 500);
                        }
                    }

                }
            }, function error(response) {
                //console.log(response);
                //alert(JSON.stringify(response));
                //window.location.href = baseUrl;
            });
        }

        $scope.createGraphicalSlider = function () {
            flast = false;
            angular.forEach($scope.output.multi_pw_data, function (element, $index) {
                if (($scope.output.multi_pw_data.length - 1) == $index) {
                    flast = true;
                }
                $scope.initializeNoUIGraphical($scope.participant_slider[$index], $scope.participant_slider[$index] + 800, $index, flast);
                
            });
        }
        $scope.save_pairwise = function (value, advantage) {
            $scope.output.value = value;
            $scope.output.advantage = advantage;
            //$scope.pw_advantage = advantage;
            $scope.$parent.disableNext = $scope.output.pipeOptions.dontAllowMissingJudgment && !(parseFloat(value) > 0);

            if (!$scope.output.extremeMessage && $scope.output.value == 9 && $scope.output.pairwise_type != "ptGraphical") {
                $scope.output.extremeMessage = true;
                $scope.isExtremeMessage = true;

                $("#warningModal").foundation("reveal", "open");
                return;
            }

            if (value != -2147483648000 && $scope.output.is_auto_advance && $scope.output.pairwise_type != "ptGraphical") {
                $scope.save_pairwise_on_next($scope.output.is_auto_advance);
            }

            if (advantage == 0 && value <= 0 && $scope.output.pairwise_type == "ptGraphical" && $scope.graphical_switch) {
                var slider = $scope.getNoUiSlider();
                $timeout(function () {
                    slider.noUiSlider.set([400, 1200]);
                    setNoUiColor(false, -1);
                    //$(".noUi-connect").css("background-color", "#AAAAAA");
                    //$(".graph-green-div").css("background-color", "#AAAAAA");
                    

                    $scope.$apply(function () {
                        $scope.main_bar[0] = "";
                        $scope.main_bar[1] = "";
                        sliderLeft = parseFloat($scope.main_bar[0]);
                        sliderRight = parseFloat($scope.main_bar[1]);
                    });
                    
                }, 10);
                $scope.output.IsUndefined = true;
            }
            else {
                $scope.output.IsUndefined = false;
            }

            if (value == -2147483648000) {
                $scope.output.IsUndefined = true;
                $(".next_step").removeClass("back-pulse");
            } else {
                $(".next_step").addClass("back-pulse");
            }
            
          //  alert(value + "==" + advantage);
        }
        //End of pairwise verbal

        //footer functions

        $scope.isExtremeMessage = false;

        $scope.closeModal = function (modalName) {
            $("#" + modalName).foundation("reveal", "close");

            if (modalName === "warningModal" && $scope.output.is_auto_advance && $scope.isExtremeMessage) {
                $scope.isExtremeMessage = false;
                $scope.save_pairwise($scope.output.value, $scope.output.advantage);
            }
            return false;
        }


        //PAIRWISE GRAPHICAL, This is used by verbal also - Ena
        $scope.graphical_switch = true;
        $scope.switch_graphical = function (isSingle) {
            $scope.graphical_switch = isSingle;
            $scope.$parent.graphical_switch = isSingle;
        }

        $scope.$on("switch_graphical", function (e, isSingle) {
            $scope.switch_graphical(isSingle);
        });

        $scope.getNoUiSlider = function (index) {
            //multi
            if (index >= 0)
                return document.getElementById("gslider" + index);


            //single        
            return document.getElementById("graphicalSlider");
        }

        $scope.setNoUiSlider = function (upper, lower, index) {
            var slider = $scope.getNoUiSlider(index);
            slider.noUiSlider.set([upper, lower]);

        }

        $scope.initializeNoUIGraphical = function (upper, lower, index, flast) {
            if (!upper) upper = 400;
            if (!lower) lower = 1200;
            if ($scope.output.advantage == 0 && $scope.output.value <= 0) { upper = 400; lower = 1200; };
            var lefthandle = upper;
            var righthandle = lower;
            var slider = false;
            $timeout(function () {

                if (!slider)
                    slider = $scope.getNoUiSlider(index);
                noUiSlider.create(slider, {
                    start: [lefthandle, righthandle],
                    behaviour: "drag-fixed-tap",
                    connect: true,
                    step: 1,
                    range: {
                        "min": 0,
                        "max": 1600
                    },
                    pips: {
                        mode: "positions",
                        values: [5, 9, 13, 17, 25, 50, 75, 83, 87, 91, 95],
                        density: 5,
                        stepped: false
                    }
                });


                var slider_value = slider.noUiSlider.get();
                var step = (parseFloat(slider_value[1]) - parseFloat(slider_value[0]));

                var lower = $(".noUi-handle-upper").offset().left - $(".noUi-handle-lower").offset().left;
                var half = lower / 2;

                var $div2 = $("<div></div>", { "class": "graph-green-div" });
                var max = $("#gslider" + index).width();
                var zero = (max / 2);
                //console.log("index: " + index + ", max: " + max + ", zero: " + zero);

                $div2.css({
                    "background-color": "#6aa84f",
                    "position": "absolute",
                    "left": "50%",
                    "overflow": "hidden",
                    "pointer-events": "none",
                    "max-height": $("#" + slider.id).height(),
                    "width": zero - 1+"px"
                });
                
                //var zero = (max / 2);
                //$div2.width(zero - 1);
                //$div2.height($("#" + slider.id).height());
                
                //

                var pipsy = ["9", "5", "3", "2", "1", "", "1", "2", "3", "5", "9"];
                if ($scope.is_multi) {
                    //snapValues = [];
                    //sliderLeft = [];
                    //sliderRight = [];
                    $div2.height($(".multigraphical0").height());
                    var storedValue = [
                        document.getElementById("input1" + index),
                        document.getElementById("input2" + index)
                    ];
                    snapValues.push(storedValue);
                    sliderLeft.push(parseFloat(storedValue[0].value));
                    sliderRight.push(parseFloat(storedValue[1].value));
                    $("#" + slider.id + " .noUi-value-large").each(function (i, e) {
                        $(this).css({"font-size": "11.5px", "top": "7.5px"});
                        $(this).text(pipsy[i]);

                    });
                } else {
                    $div2.height($("#" + slider.id).height());
                    snapValues = [];
                    sliderLeft = [];
                    sliderRight = [];
                    var storedValue = [
                        document.getElementById("noUiInput1"),
                        document.getElementById("noUiInput2")
                    ];
                    snapValues = storedValue;
                    sliderLeft = parseFloat(storedValue[0].value);
                    sliderRight = parseFloat(storedValue[1].value);
                    $div2.width(($(".noUi-base").width() / 2));
                    $(".noUi-value-large").each(function (i, e) {
                        $(this).text(pipsy[i]);

                    });
                }
                $div2.insertAfter($("#" + slider.id + " .noUi-connect"));

                $(".noUi-handle").addClass("disabled");
                $(".noUi-handle").css("width", "20px").css("top", "-5px").css("height", "26px");
                $(".noUi-handle.noUi-handle-upper").css("left", "-10px");
                $(".noUi-handle.noUi-handle-lower").css("left", "-10px");
                //slider functions
                if (flast) {
                    //$(".graph-green-div").css("background", "#6aa84f");
                    $timeout(function () {
                        $(".graph-green-div").width(zero - 4);
                        outerIndex = -1;
                    }, 500);
                    
                }


                $("#" + slider.id + " .noUi-draggable ").on("mousedown", function () {
                   
                    $(".multi-rows").removeClass("selected");
                    $scope.$apply(function () {
                        $scope.active_multi_index = index;
                    });
                    $(".multi-row-" + $scope.active_multi_index).addClass("selected");
                    //$scope.set_multi_index(index);
                });

                $("#" + slider.id + " .noUi-draggable ").on("touchstart", function () {
                    //console.log("start");
                    $(".multi-rows").removeClass("selected");
                    $scope.$apply(function () {
                        $scope.active_multi_index = index;
                    });
                    $(".multi-row-" + $scope.active_multi_index).addClass("selected");
                });

                //var isChangeRunning = true;
                slider.noUiSlider.on("change", function (value, handle) {
                    //console.log("change");
                    //console.log("left: " + value[0] + ", right: " + value[1]);
                    outerIndex = -1;
                    //isChangeRunning = true;
                    var SliderVal = slider.noUiSlider.get();
                    //console.log(slider.noUiSlider.get());
                    var thisUiOrigin;
                    var thisGreenDiv;
                    if ($scope.is_multi) {
                        thisUiOrigin = $("#gslider" + index + " .noUi-origin");
                        thisGreenDiv = $("#gslider" + index + " .graph-green-div");
                    }
                    else {
                        thisUiOrigin = $(".noUi-origin");
                        thisGreenDiv = $(".graph-green-div");
                    }
                    if (thisUiOrigin.is(":hidden")) {
                        thisUiOrigin.show();
                        thisGreenDiv.show();
                        if (parseFloat(SliderVal[1]) < 800) {
                            slider.noUiSlider.set([SliderVal[1], parseFloat(SliderVal[1]) + 800]);
                            return false;
                        }
                    };

                    var signal = -1;
                    $scope.$apply(function () {
                        if ($scope.is_multi) {
                            $scope.left_input[index] = isNaN(sliderLeft[index]) ? 1 : sliderLeft[index];
                            $scope.right_input[index] = isNaN(sliderRight[index]) ? 1 : sliderRight[index];
                            signal = $scope.oldSliderValue[index];
                        } else {
                            $scope.main_bar[0] = isNaN(sliderLeft) ? 1 : sliderLeft;
                            $scope.main_bar[1] = isNaN(sliderRight) ? 1 : sliderRight;
                            signal = $scope.oldSliderValue;
                        }
                    });
                    
                    var pass = false;
                    if (signal != parseInt(value[0])) {
                        var value1 = parseFloat(SliderVal[0]) + step;
                        slider.noUiSlider.set([SliderVal[0], value1]);
                    }
                    else {
                        var value1 = parseFloat(SliderVal[1]) - step;
                        slider.noUiSlider.set([value1, SliderVal[1]]);
                    }
                    slider_value = slider.noUiSlider.get();

                    SliderVal = slider.noUiSlider.get();
                    if (parseFloat(SliderVal[0]) > 720) {
                        slider.noUiSlider.set([720, 1520]);

                    }
                    else if (parseFloat(SliderVal[0]) < 80) {
                        slider.noUiSlider.set([80, 880]);
                    }

                    var SliderVal = slider.noUiSlider.get();
                    if ($scope.is_multi)
                        $scope.oldSliderValue[index] = parseInt(SliderVal[0]);
                    else
                        $scope.oldSliderValue = parseInt(SliderVal[0]);

                    var value = ""; var advantage = "";
                    if (SliderVal[0] > 400) {
                        //***SliderVal[1] = ((SliderVal[1] - 800) / (800 - SliderVal[0])).toFixed(2);
                        //***SliderVal[0] = 1;
                        SliderVal = $scope.getandCalculateSlider(SliderVal, false);
                        value = SliderVal[1];
                        advantage = -1;
                    }
                    else {
                        //***SliderVal[0] = ((800 - SliderVal[0]) / (SliderVal[0])).toFixed(2);
                        //***SliderVal[1] = 1;
                        SliderVal = $scope.getandCalculateSlider(SliderVal, false);
                        value = SliderVal[0];
                        advantage = 1;
                    }

                    //console.log(parseFloat(value));


                    //console.log(value);
                    if (value > 9)
                        value = 9;

                    $scope.$apply(function() {
                        if ($scope.is_multi) {
                            $scope.add_multivalues(value, advantage, index);
                        } else {
                            $scope.save_pairwise(value, advantage);
                        }
                    });

                    if (index && parseFloat(value) == 1) {
                        $scope.left_input[index] = 1;
                        $scope.right_input[index] = 1;
                    }
                });

                
                slider.noUiSlider.on("update", function (value, handle) {
                    $timeout.cancel($scope.noUiTimeOut);
                    if ($scope.is_multi) {
                        if (index != outerIndex) {

                            setNoUiColor(true, index);
                            //$("#gslider" + index + " .noUi-connect").css("background-color", "#0058a3");
                            //$("#gslider" + index + " .graph-green-div").css("background-color", "#6aa84f");


                            //var maxx = $(".noUi-base").width();
                            //var zero = (max / 2);
                            //$(".graph-green-div").width(zero);
                            //isChangeRunning = false;

                            //var SliderVal = $scope.getandCalculateSlider(slider.noUiSlider.get());
                            outerIndex = index;
                        }
                    }
                    else {
                        setNoUiColor(true, -1);
                    }




                });
                slider.noUiSlider.on("slide", function (data, handle, value) {
                    if (document.activeElement && document.activeElement.tagName == "INPUT") {
                        document.activeElement.blur();
                        $scope.blur_graphical_input();
                    }

                    //var uibaseWidth = $("#gslider" + index).width();
                    //$("#gslider" + index + " .graph-green-div").width((uibaseWidth / 2) - 1);
                    $scope.fFirst = true;
                    var firstvalue = parseInt(value[0]);
                    if ($scope.is_multi) {

                        if ($scope.oldSliderValue) {
                            if ($scope.oldSliderValue[index] != firstvalue) {
                                $scope.fFirst = false;
                            }
                        }
                    }
                    else {
                        if ($scope.oldSliderValue) {
                            if ($scope.oldSliderValue != firstvalue) {
                                $scope.fFirst = false;
                            }

                        }
                    }
                    //if (!fFirst)
                    //{
                    //    slider.noUiSlider.set([value[0], value[0] + 800]);
                    //}

                    var SliderVal = $scope.getandCalculateSlider(slider.noUiSlider.get(), $scope.fFirst);
                    //console.log(SliderVal);
                    //max = $("#gslider" + index).width();
                    //$(".graph-green-div").width((max / 2) - 1);
                    //$scope.$apply(function () {
                    if ($scope.is_multi) {

                        if (SliderVal[0] > 9 && $scope.left_input[index] <= 9) SliderVal[0] = 9;
                        if (SliderVal[1] > 9 && $scope.right_input[index] <= 9) SliderVal[1] = 9;
                        //for fast
                        snapValues[index][0].value = SliderVal[0];
                        snapValues[index][1].value = SliderVal[1];
                        sliderLeft[index] = parseFloat(SliderVal[0]);
                        sliderRight[index] = parseFloat(SliderVal[1]);

                    } else {
                        if (SliderVal[0] > 9) SliderVal[0] = 9;
                        if (SliderVal[1] > 9) SliderVal[1] = 9;
                        snapValues[0].value = SliderVal[0];
                        snapValues[1].value = SliderVal[1];
                        sliderLeft = parseFloat(SliderVal[0]);
                        sliderRight = parseFloat(SliderVal[1]);
                    }
                    //});

                });


                if ($scope.is_multi) {
                    $scope.oldSliderValue[index] = parseInt(slider_value[0]);
                    if ($scope.output.multi_pw_data[index].isUndefined) {
                        $scope.oldSliderValue[index] = 400;
                        setNoUiColor(false, index);
                        //$("#gslider" + index + " .noUi-connect").css("background-color", "#AAAAAA");
                        //$("#gslider" + index + " .graph-green-div").css("background-color", "#AAAAAA");
                        //$("#gslider" + index + " .noUi-handle").addClass("disabled");
                        //snapValues[index][0].value = SliderVal[0];
                        //snapValues[index][1].value = SliderVal[1];

                        $scope.left_input[index] = "";
                        $scope.right_input[index] = "";
                    }
                }
                else {
                    $scope.oldSliderValue = parseInt(slider_value[0]);
                    if ($scope.output.advantage == 0 && $scope.output.value <= 0) {
                        $scope.oldSliderValue = 400;
                        setNoUiColor(false, -1);
                        //$(".noUi-handle").addClass("disabled");
                        $(" .noUi-connect").css("background-color", "#AAAAAA");
                        $(" .graph-green-div").css("background-color", "#AAAAAA");
                        //                    $(" noUi-handle-upper").css("background-color", "#AAAAAA");
                        //                    $(" .graph-green-div").css("background-color", "#AAAAAA");
                        $scope.main_bar[0] = "";
                        $scope.main_bar[1] = "";
                    }
                }

                if (flast) {
                    var max = $(".noUi-base").width();
                    var zero = (max / 2);
                    //console.log("4718 -> index: " + index + ", max: " + max + ", zero: " + zero);
                    $(".graph-green-div").width(zero - 4);
                }
            }, 700); // TODO: optimize timeout

        }
        $scope.setNouiBox = function (val1, val2) {
            $scope.left_input[index] = parseFloat(val1);
            $scope.right_input[index] = parseFloat(val2);
        }

        function setNoUiColor(fJudgmentExist, index) {
            if (fJudgmentExist) {
                if (index < 0) {
                    $(".noUi-connect").css("background-color", "#0058a3 ");
                    $(".graph-green-div").css("background-color", "#6aa84f ");
                    //                $(" .noUi-handle-lower").css("background-color", "#0058a3 ");
                    //                $(".noUi-handle-upper").css("background-color", "#6aa84f ");
                }
                else {
                    //                $(".noUi-handle").removeClass("disabled");
                    $("#gslider" + index + " .noUi-connect").css("background-color", "#0058a3");
                    $("#gslider" + index + " .graph-green-div").css("background-color", "#6aa84f");
                    //                $("#gslider" + index + " .noUi-handle-lower").css("background-color", "#0058a3 ");
                    //                $("#gslider" + index + " .noUi-handle-upper").css("background-color", "#6aa84f ");
                }

            }
            else {
                if (index < 0) {
                    $(" .noUi-connect").css("background-color", "#AAAAAA ");
                    $(" .graph-green-div").css("background-color", "#AAAAAA ");
                    //                $(" .noUi-handle-upper").css("background-color", "#DDDDDD ");
                    //                $(" .noUi-handle-lower").css("background-color", "#DDDDDD ");
                }
                else {
                    $("#gslider" + index + " .noUi-connect").css("background-color", "#AAAAAA");
                    $("#gslider" + index + " .graph-green-div").css("background-color", "#AAAAAA");
                    //                $("#gslider" + index + " .noUi-handle-lower").css("background-color", "#DDDDDD ");
                    //                $("#gslider" + index + " .noUi-handle-upper").css("background-color", "#DDDDDD ");
                }
            }

        }

        $scope.getandCalculateSlider = function (sliderValue, fFirst) {
            if (fFirst)
                sliderValue[0] = sliderValue[1] - 800;

            //////if (sliderValue[0] > 400) {
            //////    sliderValue[1] = ((sliderValue[1] - 800) / (800 - sliderValue[0])).toFixed(2);
            //////    sliderValue[0] = 1;
            //////}
            //////else {
            //////    sliderValue[0] = ((800 - sliderValue[0]) / (sliderValue[0])).toFixed(2);
            //////    sliderValue[1] = 1;
            //////}
            var ranges = [];
            ranges.push({ "left": 80, "right": 1520 });
            ranges.push({ "left": 144, "right": 1456 });
            ranges.push({ "left": 208, "right": 1392 });
            ranges.push({ "left": 272, "right": 1328 });
            ranges.push({ "left": 400, "right": 1200 });

            if (sliderValue[0] > 400) {
                //if (sliderValue[1] >= 1520) {
                //    sliderValue[1] = 9;
                //} else if (sliderValue[1] >= 1456 && sliderValue[1] < 1520) {
                //    sliderValue[1] = (5 + ((sliderValue[1] - 1456) / 64 * 4)).toFixed(2);
                //} else if (sliderValue[1] >= 1392 && sliderValue[1] < 1456) {
                //    sliderValue[1] = (3 + ((sliderValue[1] - 1392) / 64 * 2)).toFixed(2);
                //} else if (sliderValue[1] >= 1328 && sliderValue[1] < 1392) {
                //    sliderValue[1] = (2 + ((sliderValue[1] - 1328) / 64 * 1)).toFixed(2);
                //} else {
                //    sliderValue[1] = (1 + ((sliderValue[1] - 1200) / 128 * 1)).toFixed(2);
                //}
                if (sliderValue[1] >= ranges[0].right) {
                    sliderValue[1] = 9;
                } else if (sliderValue[1] >= ranges[1].right && sliderValue[1] < ranges[0].right) {
                    sliderValue[1] = (5 + ((sliderValue[1] - ranges[1].right) / (ranges[0].right - ranges[1].right) * 4)).toFixed(2);
                } else if (sliderValue[1] >= ranges[2].right && sliderValue[1] < ranges[1].right) {
                    sliderValue[1] = (3 + ((sliderValue[1] - ranges[2].right) / (ranges[1].right - ranges[2].right) * 2)).toFixed(2);
                } else if (sliderValue[1] >= ranges[3].right && sliderValue[1] < ranges[2].right) {
                    sliderValue[1] = (2 + ((sliderValue[1] - ranges[3].right) / (ranges[2].right - ranges[3].right) * 1)).toFixed(2);
                } else {
                    sliderValue[1] = (1 + ((sliderValue[1] - ranges[4].right) / (ranges[3].right - ranges[4].right) * 1)).toFixed(2);
                }

                sliderValue[1] = $scope.getSliderLabelValue(sliderValue[1]);
                sliderValue[0] = 1;
            } else {
                //if (sliderValue[0] <= 80) {
                //    sliderValue[0] = 9;
                //} else if (sliderValue[0] > 80 && sliderValue[0] <= 144) {
                //    sliderValue[0] = (9 - ((sliderValue[0] - 80) / 64 * 4)).toFixed(2);
                //} else if (sliderValue[0] > 144 && sliderValue[0] <= 208) {
                //    sliderValue[0] = (5 - ((sliderValue[0] - 144) / 64 * 2)).toFixed(2);
                //} else if (sliderValue[0] > 208 && sliderValue[0] < 272) {
                //    sliderValue[0] = (3 - ((sliderValue[0] - 208) / 64 * 1)).toFixed(2);
                //} else {
                //    sliderValue[0] = (2 - ((sliderValue[0] - 272) / 128 * 1)).toFixed(2);
                //}
                if (sliderValue[0] <= ranges[0].left) {
                    sliderValue[0] = 9;
                } else if (sliderValue[0] > ranges[0].left && sliderValue[0] <= ranges[1].left) {
                    sliderValue[0] = (9 - ((sliderValue[0] - ranges[0].left) / (ranges[1].left - ranges[0].left) * 4)).toFixed(2);
                } else if (sliderValue[0] > ranges[1].left && sliderValue[0] <= ranges[2].left) {
                    sliderValue[0] = (5 - ((sliderValue[0] - ranges[1].left) / (ranges[2].left - ranges[1].left) * 2)).toFixed(2);
                } else if (sliderValue[0] > ranges[2].left && sliderValue[0] < ranges[3].left) {
                    sliderValue[0] = (3 - ((sliderValue[0] - ranges[2].left) / (ranges[3].left - ranges[2].left) * 1)).toFixed(2);
                } else {
                    sliderValue[0] = (2 - ((sliderValue[0] - ranges[3].left) / (ranges[4].left - ranges[3].left) * 1)).toFixed(2);
                }

                sliderValue[0] = $scope.getSliderLabelValue(sliderValue[0]);
                sliderValue[1] = 1;
            }

            console.log(sliderValue[0] + ", " + sliderValue[1]);

            return sliderValue;
        }

        $scope.getSliderLabelValue = function(value) {
            var ceilValue = Math.ceil(value);
            var middleValue = ceilValue - 0.5;
            var floorValue = Math.floor(value);

            if (value > (middleValue + 0.05) && value >= (ceilValue - 0.05)) {
                value = ceilValue;
            } else if (value <= (middleValue + 0.05) && value >= (middleValue - 0.05)) {
                value = middleValue;
            } else if (value <= (floorValue + 0.05)) {
                value = floorValue;
            }

            return value;
        }

        $scope.color_style = function (value, slider_value, i) {
            if (value < $scope.lowest_value && slider_value == 800) {
                var sample = { "background": "#AAAAAA" };
                $(".g-slider" + i + " .graphicalSlider .ui-slider-range").css("background", "#AAAAAA");
                $(".g-slider" + i + " .graphicalSlider ").css("background", "#AAAAAA");

            }
            else {
                var sample = { "background": "#6aa84f" };
                $(".g-slider" + i + " .graphicalSlider .ui-slider-range").css("background", "#0058a3");
                $(".g-slider" + i + " .graphicalSlider ").css("background", "#6aa84f");
            }
        }

        //set the values here when entering judgement
        $scope.active_multi_index = 0;
        $scope.activate_first_row = true;
        $scope.text_limit = $scope.$parent.text_limit;

        $scope.set_multi_index = function (index) {
            //console.log("index: " + index + ", active_multi_index: " + $scope.active_multi_index);
            //console.log("set_multi_index");

            //angular.element(".next_step").removeClass("back-pulse");
            $(".multi-rows").removeClass("selected");

            var callApply = $scope.active_multi_index != index;
            $scope.active_multi_index = index;

            $(".multi-row-" + $scope.active_multi_index).addClass("selected");
            $(".multi-row-" + index + " .not-disabled").addClass("change-color-pulse");

            try {
                if ($scope.isMobile() && ($scope.output.page_type == "atAllPairwise" || $scope.output.page_type == "atAllPairwiseOutcomes")) {
                    $timeout(function () {
                        $scope.mobile_manual_equalizer();
                    }, 2, callApply);
                }
            }
            catch (e) {
            }

            $timeout(function () {
                $(".not-disabled").removeClass("change-color-pulse");
                $(".disabled").removeClass("change-color-pulse");

                //$scope.$apply(function () {
                //    $scope.active_multi_index = index;
                //});

                $scope.scroll_to_active_index();
            }, 200, false);

            $scope.callResizeFrameImagesProportionally(200, "set_multi_index " + index);
        }

        //for direct multi
        $scope.move_to_next_row = function (index, multi_index) {
            $timeout(function () {
                $scope.removeSliderTabIndex();
                $(".multi-rows").removeClass("selected");
                $(".multi-rows").addClass("fade-fifty");

                if (index + 2 > $scope.output.multi_non_pw_data.length) {
                    $scope.active_multi_index = $scope.output.multi_non_pw_data.length - 1;
                    $(".multi-row-" + $scope.active_multi_index).addClass("selected");
                    $(".multi-row-" + $scope.active_multi_index).removeClass("fade-fifty");
                    $scope.activate_first_row = true;
                }
                else {
                    $scope.active_multi_index = index + 1;
                    var selectedRow = $(".multi-row-" + ($scope.active_multi_index));
                    selectedRow.addClass("selected");
                    selectedRow.removeClass("fade-fifty");
                    $scope.activate_first_row = false;

                    //var inputField = selectedRow.find("#at_direct_input" + ($scope.active_multi_index));
                    //if (inputField.length > 0) {
                    //    inputField[0].focus();
                    //}
                }

                //if (angular.isUndefined(multi_index) || multi_index == null) {
                //    $("#at_direct_input" + ($scope.active_multi_index)).focus();
                //}

                if ($scope.activate_first_row && $scope.is_multi_direct_judgments_done()) {
                    //angular.element("html, body").animate({ scrollTop: 0 }, "slow");
                    //var loadingMessage = stringResources.yellowLoadingIcon.multiSave;
                    //$scope.$parent.runProgressBar(2, 0, 100, loadingMessage, false);
                    angular.element(".next_step").addClass("back-pulse");
                }
                else {
                    $scope.scroll_to_index = $scope.active_multi_index - 2;
                    //console.log($scope.scroll_to_index);
                    if ($scope.scroll_to_index < 0) {
                        //do not scroll
                    }
                    else if (typeof (multi_index) == "undefined" && multi_index == null) {
                        //$location.hash("multi-row-" + $scope.scroll_to_index);
                        //$anchorScroll();
                        $scope.scroll_to_active_index();
                    }
                }

                if (typeof (multi_index) != "undefined" && multi_index != null) {
                    $scope.move_to_next_row(multi_index - 1);
                }

            }, 1);
        }

        $scope.is_multi_direct_judgments_done = function () {
            var judgmentsDone = true;

            for (i = 0; i < $scope.at_multi_direct_input.length; i++) {
                if ($scope.at_multi_direct_slider[i] == -1) {
                    judgmentsDone = false;
                    break;
                }
            }

            return judgmentsDone;
        }

        //Volt - reflow for multi, see there is no timeout but is working. timeout seems to make the messed up layout visibile for a while
        $scope.reflow = function () {
            $(document).foundation("reflow");
        }

        $scope.temp_tip = "";
        $scope.add_multivalues = function (value, advantage, index, event) {
            angular.element(".next_step").removeClass("back-pulse");
            //  $scope.multi_bar_index = $(event.target).attr("data-bar-index");
            //$scope.reflow_equalizer();

            //Timeout value is required for judgment line flash and 0 value when a judgment is deleted
            var flashTimeOut = value == "-2147483648000" ? 0 : 900;

            var isSelectedIndex = ($scope.active_multi_index == index);
            
            //Flash only when judgment is entered and no flash when a judgment is deleted
            if (value != "-2147483648000") {
                $timeout(function () {
                    //adding flash css class on judgment line if it's selected line
                    if (isSelectedIndex) {
                        $(".multi-row-" + index).addClass("judgment-flash");
                        $(".tt-verbal-bars-wrap.multi-loop.selected ul li").addClass("judgment-flash-li");
                    }
                }, 25, false);
            }

            $scope.active_multi_index = index;
            //$scope.set_multi_index(index);

            if (isSelectedIndex) {
                $timeout(function () {
                    $(".multi-rows").removeClass("selected");
                    $(".multi-row-" + index).addClass("selected");

                    if (value != "-2147483648000") {
                        //removing flash css class on judgment line
                        $(".multi-row-" + index).removeClass("judgment-flash");
                        $(".tt-verbal-bars-wrap.multi-loop.selected ul li").removeClass("judgment-flash-li");

                        if ($scope.output.pairwise_type == "ptGraphical") {
                            $(".graph-green-div").addClass("others");
                            $("#gslider" + index + " .graph-green-div").removeClass("others");
                            $(".graph-green-div").width(zero);
                            var max = $(".noUi-base").width();
                            var zero = (max / 2);
                            $(".graph-green-div.others").width(zero);
                            var uibaseWidth = $("#gslider" + index).width();
                            $("#gslider" + index + " .graph-green-div").width((uibaseWidth / 2) - 1);
                            $(".graph-green-div.others").width((uibaseWidth / 2));
                        }
                        //if (advantage == 1) {
                        //    Foundation.libs.tooltip.showTip(angular.element(".lft-tooltip-" + $scope.multi_bar_index));
                        //    $scope.temp_tip = ".lft-tooltip-" + $scope.multi_bar_index ;
                        //}
                        //else if (advantage == 0) {
                        //    Foundation.libs.tooltip.showTip(angular.element(".mid-tooltip-" + $scope.multi_bar_index));
                        //    $scope.temp_tip = ".mid-tooltip-" + $scope.multi_bar_index;
                        //}
                        //else {
                        //    Foundation.libs.tooltip.showTip(angular.element(".rgt-tooltip-" + $scope.multi_bar_index));
                        //    $scope.temp_tip = ".rgt-tooltip-" + $scope.multi_bar_index;
                        //}
                        $timeout(function() {

                                if (index + 2 > $scope.output.multi_pw_data.length) {
                                    $scope.active_multi_index = $scope.output.multi_pw_data.length - 1;
                                    $(".multi-row-" + $scope.active_multi_index).addClass("selected");
                                    $scope.activate_first_row = true;
                                } else {
                                    $(".multi-row-" + (index + 1)).addClass("selected");
                                    $scope.active_multi_index = index + 1;
                                    $scope.activate_first_row = false;
                                }

                                if ($scope.active_multi_index > 0 || $scope.activate_first_row) {
                                    // $location.hash("multi-row-" + $scope.active_multi_index);
                                    //$anchorScroll();
                                }

                                $scope.set_multi_index($scope.active_multi_index);
                                if ($scope.activate_first_row) {
                                    //angular.element("html, body").animate({ scrollTop: 0 }, "slow");
                                    angular.element(".next_step").addClass("back-pulse");
                                    //var loadingMessage = stringResources.yellowLoadingIcon.multiSave;
                                    //$scope.$parent.runProgressBar(2, 0, 100, loadingMessage, false);
                                } else {
                                    $scope.scroll_to_index = $scope.active_multi_index - 2;
                                    // //console.log($scope.scroll_to_index);
                                    if ($scope.scroll_to_index <= 1) {
                                        //do not scroll
                                    } else {
                                        //$location.hash("multi-row-" + $scope.scroll_to_index);
                                        //$anchorScroll();
                                    }
                                }

                                $scope.scroll_to_active_index();
                                // Foundation.libs.tooltip.hide(angular.element($scope.temp_tip));

                            }, 300);
                    }
                }, flashTimeOut, false);
            } else {
                $scope.callResizeFrameImagesProportionally(100, "add_multivalues");
            }

            $scope.updateMultiPairwiseObjectValue(value, advantage, index, event);
        }

        $scope.updateMultiPairwiseObjectValue = function (value, advantage, index, event) {
            ////console.log("updating... value: " + value + ", advantage: " + advantage);
            $scope.output.multi_pw_data[index].Value = parseFloat(value);
            $scope.output.multi_pw_data[index].Advantage = parseInt(advantage);
            $scope.checkMultiValue($scope.output.multi_pw_data);

            if (advantage == 0) {
                $scope.output.multi_pw_data[index].isUndefined = true;
            } else {
                $scope.output.multi_pw_data[index].isUndefined = false;
            }

            if (event) {
                event.stopPropagation();
            }
        }

        $scope.checkMultiValue = function (multiData, fNonPw) {
            ////console.log("Calling checkMultiValue...");
            //if ($scope.output.pipeOptions.dontAllowMissingJudgment) {
                //$scope.$parent.disableNext = false;
                var isUndefined = false;

                for (i = 0; i < multiData.length; i++) {
                    if (fNonPw == 1) {
                        var value = multiData[i][0];
                        if (value < 0 || value == "") {
                            isUndefined = true;
                        }   
                    } else if (fNonPw == 2) {
                        var value = multiData[i];
                        if (value < 0 || value == "") {
                            isUndefined = true;
                        }
                    } else {
                        if (multiData[i].Advantage == 0 && multiData[i].Value < 1) {
                            isUndefined = true;
                        }
                    }
                }

                $scope.$parent.disableNext = $scope.output.pipeOptions.dontAllowMissingJudgment && isUndefined;
                $scope.output.IsUndefined = isUndefined;
                $scope.$parent.output.IsUndefined = isUndefined;

                if (isUndefined) {
                    $(".next_step").removeClass("back-pulse");
                    ////console.log("removed back-pulse");
                } else {
                    if (!$(".next_step").hasClass("back-pulse")) {
                        $(".next_step").addClass("back-pulse");
                        console.log("added back-pulse");
                    }
                }
            //}

        }

        //scrolls to active index for multi-pairwise and multi direct
        $scope.prev_no_of_lines = null;

        $scope.scroll_to_active_index = function () {
            if ($scope.is_multi && ($scope.output.page_type === "atAllPairwise" || ($scope.output.page_type === "atNonPWAllChildren" && $scope.output.non_pw_type === "mtDirect"))) {
                $scope.scroll_body();
                $scope.scroll_multi_container();
            }
        }

        $scope.scroll_body = function() {
            var activeElement = $(".multi-row-" + ($scope.active_multi_index));
            var footerPosition = ($scope.isMobile() ? $(".footer-nav-mobile").offset().top - $(".footer-nav-mobile .fnv-toggler").outerHeight() : $(".tt-footer-wrap").offset().top);
            ////console.log("footerPosition 1: " + footerPosition);

            var divPushPosition = ($scope.isMobile() ? footerPosition : $("div.push").offset().top);
            ////console.log("divPushPosition 1: " + divPushPosition);

            footerPosition = ($scope.isMobile() ? footerPosition : (divPushPosition < footerPosition ? divPushPosition : footerPosition));
            ////console.log("footerPosition 2: " + footerPosition);
            //var footerButtonPosition = ($scope.isMobile() ? $("button.fnv-toggler").offset().top - $("button.fnv-toggler").outerHeight() : footerPosition);
            //footerPosition = (footerButtonPosition < footerPosition ? footerButtonPosition : footerPosition);
            var scrollTo = activeElement.offset().top + activeElement.outerHeight() - footerPosition;
            ////console.log("scrollTo 1: " + scrollTo);

            //var qh_icons_div = $(".qhelp-questions").outerHeight();
            //scrollTo -= qh_icons_div + 10;

            //calculates scroll for mobile multi-pairwise
            if ($scope.isMobile() && $scope.output.page_type === "atAllPairwise") {
                //getting max text of left and right node of selected pair
                var maxText = $scope.output.multi_pw_data[$scope.active_multi_index].LeftNode;
                maxText = (maxText.length < $scope.output.multi_pw_data[$scope.active_multi_index].RightNode.length
                    ? $scope.output.multi_pw_data[$scope.active_multi_index].RightNode
                    : maxText);

                var splittedString = maxText.split(" ");
                var lineText = "";
                var noOfLines = 0;

                //gets number of lines
                for (var i = 0; i < splittedString.length; ++i) {
                    lineText += lineText.length > 0 ? " " + splittedString[i] : splittedString[i];
                    if (lineText.length >= $scope.text_limit) {
                        noOfLines++;
                        if (lineText.length == $scope.text_limit) {
                            lineText = "";
                        } else {
                            lineText = splittedString[i];
                        }
                    }
                }

                if (lineText.length > 0) {
                    noOfLines++;
                    lineText = "";
                }

                noOfLines = noOfLines < 2 ? 2 : noOfLines;

                if ($scope.prev_no_of_lines == null || $scope.active_multi_index == 0) {
                    if ($scope.prev_no_of_lines == null) {
                        var stickyElement = $(".tt-sticky-element");
                        if (stickyElement.length > 0) {
                            stickyElement.addClass("sticky");
                        }
                    }

                    $scope.prev_no_of_lines = noOfLines;
                }

                ////console.log("calculated lines: " + noOfLines);

                if (noOfLines > $scope.prev_no_of_lines) {
                    //when previous element has different line than active element 
                    //scrollTo += (noOfLines - $scope.prev_no_of_lines) * 7;
                    scrollTo += (noOfLines - $scope.prev_no_of_lines) * 15;
                } else if (noOfLines === $scope.prev_no_of_lines) {
                    //scrollTo += 5;
                } else {
                    //when previous element has different line than active element
                    //scrollTo -= ($scope.prev_no_of_lines - noOfLines) * 15;
                    scrollTo -= ($scope.prev_no_of_lines - noOfLines) * 12;
                }

                ////console.log("scrollTo 2: " + scrollTo);

                //if ($scope.active_multi_index > 0) {
                //    var infoDoc = $("div.InfoDocsParentDiv");
                //    var isStickyInfo = false;

                //    if ($("div.question-header").offset().top > infoDoc.offset().top) {
                //        infoDoc = $("div.question-header");
                //        isStickyInfo = true;
                //    }

                //    if (activeElement.offset().top - scrollTo < infoDoc.offset().top + infoDoc.outerHeight()) {
                //        //changing scroll position below sticky info
                //        scrollTo -= infoDoc.offset().top + infoDoc.outerHeight() - (activeElement.offset().top - scrollTo);
                //    }

                //    var ttHeaderHeight = $(".tt-header").outerHeight();
                //    if ($(window).scrollTop() < ttHeaderHeight && $(window).scrollTop() + scrollTo > ttHeaderHeight) {
                //        if ($scope.active_multi_index > 1 && $scope.prev_multi_index !== $scope.active_multi_index) {
                //            scrollTo -= Math.round(scrollTo / 3);
                //        } else if ($scope.prev_multi_index === $scope.active_multi_index) {
                //            //scrollTo -= 50;
                //            scrollTo -= scrollTo > ttHeaderHeight ? (ttHeaderHeight / 2) : 0;
                //        } else {
                //            scrollTo -= 40;
                //            //scrollTo -= 20;
                //        }
                //    }
                //}

                $scope.prev_no_of_lines = noOfLines;
            }

            try {
                if ($scope.isMobile()) {
                    if (scrollTo > 0) {
                        //scrollTo += $scope.output.pairwise_type == "ptVerbal" ? 25 : 0;
                        if ($scope.output.pairwise_type === "ptVerbal") {
                            //if (!$scope.output.orig_collapse_bars)
                            //    scrollTo += 60;

                            //var headerHeight = $(".tt-header").outerHeight();
                            //if ($(window).scrollTop() < headerHeight && ($(window).scrollTop() + scrollTo > headerHeight)) {
                            //    scrollTo -= 30;
                            //}

                            if ($scope.active_multi_index > 0) {
                                //when Collapse Bars is not checked; pairwise verval line height is higher
                                if (!$scope.output.orig_collapse_bars)
                                    scrollTo += 60;

                                var previousElement = $(".multi-row-" + ($scope.active_multi_index - 1));
                                //console.log("previous element height: " + previousElement.outerHeight() + ", active element height: " + activeElement.outerHeight());
                                if (previousElement.outerHeight() > activeElement.outerHeight()) {
                                    //reducing scrollTo as the previous element height is greater than active element
                                    scrollTo -= previousElement.outerHeight() - (activeElement.outerHeight() + 10);
                                }
                            }
                        }
                    }
                } else {
                    var languageFooter = $(".tt-languages");
                    if (languageFooter) {
                        scrollTo -= languageFooter.outerHeight();
                    }
                }

                if (scrollTo > 0) {
                    //if ($scope.active_multi_index == 0 && $(window).scrollTop() + scrollTo > $(".tt-header").outerHeight()) {
                    //    scrollTo = $(".tt-header").outerHeight() - $(window).scrollTop() - 1;
                    //}

                    //console.log("body_scroll scrollTo: " + scrollTo);
                    $("html, body").animate({ scrollTop: "+=" + scrollTo + "px" }, 800);
                    ////console.log("scrolled: " + scrollTo + "px");
                } else if ($scope.isMobile() && $scope.output.pairwise_type === "ptGraphical") {
                    //console.log("body_scroll scrollTo: " + scrollTo);
                    $("html, body").animate({ scrollTop: "-=" + Math.abs(scrollTo + 5) + "px" }, 800);
                }
            } catch (e) {
            }
        }

        $scope.scroll_multi_container = function () {
            var parentElement = $(".multi-loop-wrap");

            if (parentElement.length > 0 && parentElement.innerHeight() > 0) {
                var activeElement = $(".multi-row-" + ($scope.active_multi_index));
                var scrollTo = 0;

                if ($scope.output.non_pw_type === "mtDirect") {
                    if (activeElement.offset().top < parentElement.offset().top) {
                        $(".multi-loop-wrap").animate({ scrollTop: "-=" + (parentElement.offset().top - activeElement.offset().top) + "px" }, 800);
                    } else {
                        scrollTo = (activeElement.offset().top + activeElement.outerHeight()) - (parentElement.offset().top + parentElement.outerHeight());
                    }
                } else {
                    //scrollTo = activeElement.innerHeight() + activeElement.offset().top - parentElement.innerHeight() - parentElement.scrollTop();
                    scrollTo = activeElement.innerHeight() + activeElement[0].offsetTop - parentElement.innerHeight() - parentElement.scrollTop();
                }
                
                //scrollTo += ($scope.isMobile() ? 30 : 10);
                ////console.log("scrollTo 1: " + scrollTo);

                if ($scope.output.pairwise_type === "ptVerbal" && $scope.active_multi_index < $scope.output.multi_pw_data.length - 1) {
                    //getting default height of each row
                    var defaultHeight = $(".multi-row-" + ($scope.active_multi_index + 1)).outerHeight();

                    if ($scope.active_multi_index > 0) {
                        //subtracting extra height to maintain scrolling because last element and current element height is bigger
                        scrollTo -= $(".multi-row-" + ($scope.active_multi_index - 1)).outerHeight() - defaultHeight;
                    }
                }

                if (scrollTo > 0) {
                    ////console.log("multi_container scrollTo: " + scrollTo);
                    $(".multi-loop-wrap").animate({ scrollTop: "+=" + scrollTo + "px" }, 800);
                }
            }

            if ($scope.active_multi_index == $scope.output.multi_pw_data.length - 1) {
                $scope.prev_no_of_lines = null;
            }
        }

        $scope.reflow_equalizer = function (speed) {
            $timeout(function () {
                $(document).foundation("equalizer", "reflow");
                $(document).foundation({
                    equalizer: {
                        equalize_on_stack: true
                    }
                });
            }, 0);
        }

        $scope.toggle_multi_node = function (index, side, type) {
            if (type == "full") {
                angular.element(".limited-" + side + "-" + index).hide();
                angular.element(".full-" + side + "-" + index).show();
            }
            else {
                angular.element(".limited-" + side + "-" + index).show();
                angular.element(".full-" + side + "-" + index).hide();
            }
        }

        $scope.set_slider_blank = function (index) {
            //$scope.participant_slider[index] = 80;

            $timeout(function () {
                $scope.setNoUiSlider(400, 1200, index);


                $scope.oldSliderValue[index] = 400;
                $scope.left_input[index] = "";
                $scope.right_input[index] = "";

                sliderLeft = [];
                sliderRight = [];
                setNoUiColor(false, index);
                outerIndex = -1;
            }, 100);
            //$(".g-slider" + index + " .graphicalSlider ").slider("value", 800);

        }

        //direct one at a time
        $scope.at_direct_slider = 0;
        //$scope.at_direct_input = 0;
        $scope.get_direct_data = function () {
            var value = $scope.output.non_pw_value;
            if ($scope.output.non_pw_value < -1) {
                value = 0;
            }
            $("#at_direct_input").val(parseFloat($scope.output.non_pw_value));
            $("#at_direct_slider").slider({
                    
                  value: value,
                  range: "min",
                });
            $scope.at_direct_input = parseFloat($scope.output.non_pw_value);
            $scope.at_direct_slider = value;
        }

        $scope.set_direct_input = function (value) {
            var timeOut = 800;

            if (parseFloat(value) || parseFloat(value) == 0) {
                $scope.is_saving_judgment = true;
                var temp_value = value < 0 || value > 1 ? -1 : value;

                $("#at_direct_slider").slider("value", temp_value);
                setTimeout(function () {
                    $scope.save_direct(temp_value);
                }, timeOut);
            } else {
                value = -1;
                $("#at_direct_slider").slider("value", value);
                setTimeout(function () {
                    $scope.save_direct(value);
                }, timeOut + 10);
                //$scope.get_direct_data();
            }
        }


        $scope.save_direct = function (value) { //add comment attr here later
            //do ajax forslider update judgement backend
            //var loadingMessage = stringResources.yellowLoadingIcon.save;
            
            $scope.$parent.disableNext = $scope.output.pipeOptions.dontAllowMissingJudgment && (value == -1);
            $scope.output.IsUndefined = (value == -1);
            $scope.$parent.output.IsUndefined = (value == -1);

            if (value == -1) {
                $(".next_step").removeClass("back-pulse");
            } else {
                $(".next_step").addClass("back-pulse");
            }

                $timeout(function () {
                    $scope.$apply(function () {
                        $scope.output.non_pw_value = value;
                        $scope.is_saving_judgment = true;
                        if (value == -1) {
                            $scope.at_direct_input = "";
                            $("#at_direct_input").val("");
                            $scope.at_direct_slider = -1;
                        }
                    });
                }, 100);
           // $scope.$parent.runProgressBar(2, 0, 30, loadingMessage, false);
           // $(".load-canvas-gif").show();
           
        }

        $scope.save_direct_on_next = function () {
            var value = $scope.output.non_pw_value;
            //consoleLog("SaveDirect. current_step: " + $scope.current_step);

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/SaveDirect",
                data: {
                    step: $scope.current_step,
                    value: value,
                    sComment: $scope.output.comment
                }
            }).then(function success(response) {
                //  $(".load-canvas-gif").hide();
               // $scope.$parent.runProgressBar(2, 0, 100, loadingMessage, false);
                if ($scope.is_comment_updated || value < 0) { //if updated comment or judgment deleted

                    if ($scope.is_comment_updated) {
                        $scope.is_saving_judgment = true;
                    }

                    //consoleLog("Calling get_pipe_data from SaveDirect. current_step: " + $scope.current_step);
                    $scope.get_pipe_data($scope.current_step);
                }
                else {
                    //if ($scope.output.is_auto_advance) {
                    //    $timeout(function () {
                    //        $(".load-canvas-gif").hide();
                    //        $scope.get_pipe_data($scope.current_step + 1);
                    //    }, 1500);

                    //}
                    //else {
                    //consoleLog("Calling get_pipe_data from SaveDirect. current_step: " + $scope.current_step);

                    $scope.get_pipe_data($scope.current_step);
                    $(".next_step").addClass("back-pulse");
                    $timeout(function () {
                        $(".load-canvas-gif").hide();
                        $(".next_step").removeClass("back-pulse");
                    }, 2000);
                    $scope.$parent.footer_up();
                    //}
                }
            }, function error(response) {
                //console.log(response);
                $(".load-canvas-gif").hide();
                alert(JSON.stringify(response));
                window.location.href = baseUrl;
            });
        }
        //end of direct one at a time

        //multi direct
        $scope.at_multi_direct_slider = [];
        $scope.at_multi_direct_input = [];
        // $scope.at_direct_input = 0;
        $scope.get_multi_direct_data = function (multi_index) {
            $timeout(function () {
                $scope.$apply(function () {
                    //Storing old values which needs to be restored at the end of this block so that we get original values
                    var currentDisableNext = $scope.$parent.disableNext;
                    var currentIsUndefined = $scope.output.IsUndefined;

                    for (var i = 0; i < $scope.output.multi_non_pw_data.length; i++) {
                        var value = $scope.output.multi_non_pw_data[i].DirectData;
                        $scope.update_multi_direct_slider(i, value, multi_index);
                        //if (i == $scope.output.multi_non_pw_data.length - 1) {
                        //    if (typeof (multi_index) != "undefined") {
                        //        $scope.set_multi_index(multi_index);
                        //        return false;
                        //    }
                        //}
                    }

                    $scope.$parent.disableNext = currentDisableNext;
                    $scope.output.IsUndefined = currentIsUndefined;
                    $scope.$parent.IsUndefined = currentIsUndefined;
                });
            }, 10);
        }

        $scope.update_multi_direct_slider = function (index, value, multi_index, from_input) {
            var temp_values = $scope.at_multi_direct_slider;
            if (parseFloat(value) || parseFloat(value) == 0) {
                //$scope.$parent.disableNext = $scope.output.pipeOptions.dontAllowMissingJudgment && (value == -1);
                //$scope.output.IsUndefined = (value == -1);
                //$scope.$parent.output.IsUndefined = (value == -1);

                if (value > 1) {
                    $timeout(function () {
                        $scope.at_multi_direct_input[index] = temp_values[index];
                        $scope.at_multi_direct_slider[index] = temp_values[index];
                    }, 1000);
                    return false;
                }
                $scope.at_multi_direct_input[index] = value < 0 ? "" : value; //display the direct data from output 
                $scope.at_multi_direct_slider[index] = value < 0 ? -1 : value;

                if (!angular.isUndefined(from_input)) { // if input box is edited move to next row
                    $("#at_direct_input" + index).focus().val("").val($scope.at_multi_direct_input[index]); //11508

                    $scope.checkMultiValue($scope.at_multi_direct_slider, 2);

                    //$timeout(function () {
                    //    $scope.move_to_next_row(index, multi_index);
                    //}, 1000);
                }

                $timeout(function () {
                    $scope.$apply(function () {
                        $("#at_multi_direct_input" + index).val(value < 0 ? "" : value);
                        $("#at_multi_direct_slider" + index).slider("value", value);
                    });
                }, 10);
            }
            else {
                if (angular.isUndefined(value) || value == null || value == "") {
                    $scope.$parent.disableNext = $scope.output.pipeOptions.dontAllowMissingJudgment;
                    $scope.output.IsUndefined = true;
                    $scope.$parent.output.IsUndefined = true;
                    $(".next_step").removeClass("back-pulse");
                    ////console.log("removed back-pulse");

                    $timeout(function () {
                        $scope.$apply(function () {
                            $scope.at_multi_direct_input[index] = "";
                            $scope.at_multi_direct_slider[index] = -1;
                            $("#at_multi_direct_input" + index).val("");
                            $("#at_multi_direct_slider" + index).slider(0);

                        });
                    }, 10);
                }
            }
        }

        $scope.save_multi_direct_on_next = function () {
            //do ajax for update judgement backend
            var multivalues = [];
            // //console.log($scope.output.multi_non_pw_data);
            for (i = 0; i < $scope.at_multi_direct_input.length; i++) {
                try {
                    var vals = [];
                    vals[0] = $scope.at_multi_direct_slider[i] == -1 ? "" : $scope.at_multi_direct_slider[i];
                    // //console.log($scope.output.multi_non_pw_data[i] + "index:" + i);
                    vals[1] = $scope.output.multi_non_pw_data[i].Comment;
                    multivalues[i] = vals;
                }
                catch (e) {

                }
            }
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/SaveMultiDirectData",
                data: {
                    step: $scope.current_step,
                    multivalues: multivalues
                }
            }).then(function success(response) {
                //console.log(response);
                var loadingMessage = stringResources.yellowLoadingIcon.multiSave;
                $scope.$parent.runProgressBar(2, 0, 100, loadingMessage, true);
                if ($scope.is_comment_updated || $scope.isAutoSaving) {
                    $scope.is_saving_judgment = true;
                    
                    var timeOutValue = $scope.isAutoSaving ? 300 : 0;
                    $timeout(function () {
                        $scope.get_pipe_data($scope.current_step, null, null, $scope.active_multi_index);
                    }, timeOutValue, false);
                }
                $scope.$parent.footer_up();
            }, function error(response) {
                //console.log(response);
            });
        }

        //end of multi direct



        //save multi pairwise after next step is clicked, this func is called on get pipe data if pag etype is multi
        $scope.save_multi_pairwise_on_next = function () {
            var multivalues = [];
            for (i = 0; i < $scope.output.multi_pw_data.length; i++) {
                try {
                    var vals = [];
                    vals[0] = $scope.multi_data[i].Value;
                    vals[1] = $scope.multi_data[i].Advantage;
                    vals[2] = $scope.multi_data[i].Comment;
                    multivalues[i] = vals;
                }
                catch (e) {

                }
            }
            //console.log("test:" + multivalues);
            //do ajax for update judgement backend
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/SaveMultiPairwiseData",
                async: false,
                data: {
                    step: $scope.current_step,
                    multivalues: multivalues
                }
            }).then(function success(response) {
                var loadingMessage = stringResources.yellowLoadingIcon.multiSave;
                $scope.$parent.runProgressBar(2, 0, 100, loadingMessage, true);
                // //console.log(response);
                if ($scope.is_comment_updated || $scope.isAutoSaving) {
                    $scope.is_saving_judgment = true;

                    var timeOutValue = $scope.isAutoSaving ? 300 : 0;
                    $timeout(function () {
                        $scope.get_pipe_data($scope.current_step, null, null, $scope.active_multi_index);
                    }, timeOutValue, false);
                }
            }, function error(response) {
                // //console.log(response);
            });
        }

        $scope.save_multi_pairwise = function (value, advantage, index) {
            //alert(value + "-" + advantage + "-" + index);
            var multivalues = [];
            for (i = 0; i < $scope.multi_data.length; i++) {

                var vals = [];
                vals[0] = $scope.multi_data[i].Value;
                vals[1] = $scope.multi_data[i].Advantage;
                multivalues[i] = vals;
                if (i == index) {
                    multivalues[i][0] = value;
                    multivalues[i][1] = advantage;
                }
            }


            //do ajax for update judgement backend
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/SaveMultiPairwiseData",
                data: {
                    step: $scope.current_step,
                    multivalues: multivalues
                }
            }).then(function success(response) {
                //$scope.get_pipe_data($scope.current_step);
                //$scope.get_pipe_data($scope.current_step);
                //console.log(response);
            }, function error(response) {
                //console.log(response);
            });
        }

        $scope.get_wrt_text = function (nodeType) {
            var leftText = "";
            var rightText = "";
            var wrtText = "WRT";

            try {
                if (!$scope.isMobile()) {
                    if ($scope.output.page_type === "atAllPairwise") {
                        
                        leftText = (nodeType === "left" ? $scope.output.multi_pw_data[$scope.active_multi_index].LeftNode : $scope.output.multi_pw_data[$scope.active_multi_index].RightNode);
                        rightText = $scope.output.parent_node;
                    } else if ($scope.output.page_type === "atPairwise") {
                        leftText = (nodeType === "left" ? $scope.output.first_node : $scope.output.second_node);
                        rightText = $scope.output.parent_node;
                    } else if ($scope.output.page_type === "atNonPWAllChildren" || $scope.output.page_type === "atNonPWAllCovObjs") {
                        leftText = $scope.output.multi_non_pw_data[$scope.active_multi_index].Title;
                        rightText = $scope.output.parent_node;
                    }
                    else {
                        //single non pw
                        leftText = $scope.output.child_node;
                        rightText = $scope.output.parent_node;
                    }
                    leftText = leftText.length > 14 && wrtText.length > 2 ? leftText.substring(0, 14) + "..." : leftText;
                    rightText = rightText.length > 14 && wrtText.length > 2 ? rightText.substring(0, 14) + "..." : rightText;

                }
            } catch(err) {
                leftText = "";
                wrtText = "";
                rightText = "";
            }

            return (leftText + " " + wrtText.trim() + " " + rightText).trim();
        }

        $scope.bar_color = 0;

        $scope.participant_slider = [];

        $scope.left_input = [];
        $scope.right_input = [];
        $scope.initialize_Pie = function () {
            var $a1 = $scope.graphical_slider[0];
            var $b = 1600 - $a1;
            var result;

            $(".numerical-slider").slider("values", [$scope.numericalSlider[0], 800, $scope.numericalSlider[0] + 800]);
            updateColors([$scope.numericalSlider[0], 800, $scope.numericalSlider[0] + 800], $scope.output.advantage == 0 && $scope.output.value == 0 ? true : false);
            if ($scope.output.value == 0) {
                $(".a_Slider").slider("value", 800);
                $(".b_Slider").slider("value", 800);
                $(document).ready(function () {
                    $(".a_Slider").slider("value", 800);
                    $(".b_Slider").slider("value", 800);
                });
                $scope.main_bar_txt[0] = "no judgment";
                $scope.main_bar_txt[1] = "no judgment";
                $a1 = null;
                //updateFrontEnd($a1, $b);
                //Pizza.init("body", {
                //    data: [0, 100],
                //});
            }
            else {
                //updateFrontEnd($b, $a1);
                result = calculate_pie_and_slider("blue", $a1);

                $(".a_Slider").slider("value", 1600 - result[1]);
                $(".b_Slider").slider("value", result[1]);
                $(document).ready(function () {
                    $(".a_Slider").val(1600 - result[1]);
                    $(".b_Slider").val(result[1]);
                });
            }
            $scope.setSliderColor();


        }

        $scope.setSliderColor = function () {
            $(".ui-slider-handle").addClass("disabled");
            if ($scope.output.value >= 1) {
                //$(".a_Slider .ui-slider-range").css("background", "#0058a3");
                //$(".b_Slider .ui-slider-range").css("background", "#6aa84f");

                $(".ui-slider-range").removeClass("disabled");
            }
            else {
                //            $(".a_Slider .ui-slider-range").css("background", "#AAAAAA");
                //            $(".b_Slider .ui-slider-range").css("background", "#AAAAAA");
                $(".ui-slider-range").addClass("disabled");
            }
        }

        $scope.graphical_push_judgment = function (direction, is_stopped) {

            if (is_stopped) {
                $interval.cancel($scope.graphical_arrow_timer);
                $timeout.cancel($scope.graphical_input_timeout);
                var advantage = 0; var value = 0;
                if ($scope.main_bar[0] > $scope.main_bar[1]) {
                    value = $scope.main_bar[0];
                    advantage = 1;
                }
                if ($scope.main_bar[1] > $scope.main_bar[0]) {
                    value = $scope.main_bar[1];
                    advantage = -1;
                }
                $scope.graphical_input_timeout = $timeout(function () {
                    $scope.save_pairwise(value, advantage);
                }, 500);
            }
            else {
                $scope.graphical_arrow_timer = $interval(function () {
                    var values = $(".numerical-slider").slider("option", "values");
                    if (direction == "left") {

                        values[0] -= 3;
                    }
                    if (direction == "right") {
                        values[0] += 3;
                    }
                    $scope.numericalSlider[0] = values[0];
                    //////if (values[0] > 400) {
                    //////    $scope.main_bar[0] = 1;
                    //////    $scope.main_bar[1] = parseFloat(((values[2] - 800) / (800 - values[0])).toFixed(2));
                    //////}
                    //////else if (values[0] < 400) {
                    //////    $scope.main_bar[0] = parseFloat(((800 - values[0]) / (values[2] - 800)).toFixed(2));
                    //////    $scope.main_bar[1] = 1;
                    //////}

                    var sliderValue = [values[0], values[2]];
                    sliderValue = $scope.getandCalculateSlider(sliderValue, false);
                    $scope.main_bar[0] = sliderValue[0];
                    $scope.main_bar[1] = sliderValue[1];

                    $(".numerical-slider").slider("values", [$scope.numericalSlider[0], 800, $scope.numericalSlider[0] + 800]);
                    updateColors([$scope.numericalSlider[0], 800, $scope.numericalSlider[0] + 800]);
                }, 50);
            }



        }


        $scope.graphical_key_up = function (event, value, position, state, index, is_main_bar) {
            var code = null;
            if (event) {
                code = event.keyCode;
            }
            $timeout.cancel($scope.graphical_input_timeout);
            
            var inputId = is_main_bar ? event.currentTarget.id : event.currentTarget.id.replace(index, "");
            ////console.log(event.type);
            if (parseFloat(value[0]) < 0) {
                ////console.log("value less than 0");
                
                if (inputId === "input1" && $scope.left_input[index] < 0) {
                    $scope.left_input[index] = 0;
                } else if (inputId === "input2" && $scope.right_input[index] < 0) {
                    $scope.right_input[index] = 0;
                } else if (inputId === "noUiInput1" && $scope.main_bar[0] < 0) {
                    $scope.main_bar[0] = 0;
                } else if (inputId === "noUiInput2" && $scope.main_bar[1] < 0) {
                    $scope.main_bar[1] = 0;
                }

                if (!(event.type === "mouseup" || event.type === "mouseout" || event.type === "mouseleave" || event.type === "blur"))
                    return event.preventDefault();
            }

            //if (code === 9 && inputId === "input2") {
            //    state = "up";   //when tab key is pressed from 2nd input box then move to next row
            //}
            
            if (state == "press") {
                if (angular.isUndefined(code) || event.ctrlKey || event.altKey) {

                } else if (!((code > 47 && code < 58) || (code > 95 && code < 106)) && code !== 110 && code !== 190 && code != 46 && code !== 13 && code !== 9 && code !== 8)
                    return event.preventDefault();
            }
            else {
                if (state == "up" && value[0] != null && value[0] != "") {
                    $scope.graphical_input_timeout = $timeout(function () {
                        var advantage;
                        if (is_main_bar) {
                            //FOR SINGLE 
                            outerIndex = -1;
                            if ($scope.main_bar[0] == $scope.main_bar[1]) {
                                value[0] = 1;
                                advantage = 1;
                            }
                            else if ($scope.main_bar[0] != 1 || $scope.main_bar[1] != 1) {
                                if ($scope.main_bar[0] > $scope.main_bar[1]) {
                                    if ($scope.main_bar[1] == null || $scope.main_bar[1] == "") {
                                        $scope.main_bar[1] = 1;
                                    }
                                    value[0] = $scope.main_bar[0] / $scope.main_bar[1];
                                    advantage = 1;
                                }
                                if ($scope.main_bar[0] < $scope.main_bar[1]) {
                                    if ($scope.main_bar[0] == null || $scope.main_bar[0] == "") {
                                        $scope.main_bar[0] = 1;
                                    }
                                    value[0] = $scope.main_bar[1] / $scope.main_bar[0];
                                    advantage = -1;
                                }
                            }
                            else {
                                value[0] = $scope.main_bar[0];
                                advantage = 0;
                            }
                            $scope.save_pairwise(value[0], advantage);

                            if (advantage == -1) {
                                $scope.numericalSlider[0] = (800 * ((value[0]) / ((value[0]) + 1)));


                            }
                            if (advantage == 1) {
                                $scope.numericalSlider[0] = 800 - (800 * ((value[0]) / ((value[0]) + 1)));
                            }

                            //$scope.main_bar[0] = value[0] > 0 ? parseFloat((advantage == 1 ? (value[0]) * 1 : 1).toFixed(2)) : "";
                            //$scope.main_bar[1] = value[0] > 0 ? parseFloat((advantage == -1 ? (value[0]) * 1 : 1).toFixed(2)) : "";
                            $scope.main_bar_txt[0] = (advantage == 1 ? (value[0]) * 1 : 1).toFixed(2);
                            $scope.main_bar_txt[1] = (advantage == -1 ? (value[0]) * 1 : 1).toFixed(2);
                            if (advantage == 0) {
                                $scope.numericalSlider[0] = 400;
                            }


                            if ($scope.getNoUiSlider() != null && advantage) {
                                $scope.noUiTimeOut = $timeout(function () { $scope.setNoUiSlider($scope.numericalSlider[0], $scope.numericalSlider[0] + 800, -1); }, 100);
                                $scope.page_load = false;
                            }
                            if ($scope.getNoUiSlider() != null && advantage == 0 && value[0] == 1) {
                                $scope.noUiTimeOut = $timeout(function () {
                                    $scope.setNoUiSlider(400, 1200, -1);
                                    $(".noUi-connect").css("background-color", "#0058a3");
                                    $(".graph-green-div").css("background-color", "#6aa84f");
                                }, 100);
                            }

                        }
                        else {
                            //FOR MULTI
                            var slider_value = 0;
                            if ($scope.left_input[index] == $scope.right_input[index]) {
                                value[0] = 0;
                                advantage = 1;
                                slider_value = 400;
                            }
                            else if ($scope.left_input[index] != 1 || $scope.right_input[index] != 1) {
                                if ($scope.left_input[index] > $scope.right_input[index]) {
                                    if ($scope.right_input[index] == null || $scope.right_input[index] == "") {
                                        $scope.right_input[index] = 1;
                                    }
                                    value[0] = $scope.left_input[index] / $scope.right_input[index];
                                    advantage = 1;
                                    slider_value = 800 - (800 * ((value[0]) / ((value[0]) + 1)));

                                    //$scope.left_input[index] = parseFloat(value[0]).toFixed(2);
                                    //$scope.right_input[index] = 1;
                                }
                                if ($scope.left_input[index] < $scope.right_input[index]) {
                                    if ($scope.left_input[index] == null || $scope.left_input[index] == "") {
                                        $scope.left_input[index] = 1;
                                    }
                                    value[0] = $scope.right_input[index] / $scope.left_input[index];
                                    advantage = -1;
                                    slider_value = (800 * ((value[0]) / ((value[0]) + 1)));
                                    //$scope.right_input[index] = parseFloat(value[0]).toFixed(2);
                                    //$scope.left_input[index] = 1;

                                }
                            }
                            else {
                                value[0] = $scope.left_input[index];
                                advantage = 0;
                                slider_value = 800;
                                $scope.right_input[index] = 1;
                                $scope.left_input[index] = 1;
                            }
                            //800 - (800 * (($scope.multi_data[i].Value) / (($scope.multi_data[i].Value) + 1)));
                            //console.log(value[0]);
                            //$(".noUi-connect").css("background-color", "#0058a3");
                            //$(".graph-green-div").css("background-color", "#6aa84f");

                            if ($scope.left_input[index] !== "" && $scope.right_input[index] !== ""){
                                $scope.setNoUiSlider(slider_value, slider_value + 800, index);

                                //$(".g-slider" + index + " .graphicalSlider ").slider("value", slider_value);
                                //if (!(event.type === "mouseup" || event.type === "mouseout" || event.type === "mouseleave" || event.type === "blur"))
                                //$scope.add_multivalues(value, advantage, index, event);
                                ////console.log("left_input: " + $scope.left_input[index] + ", right_input: " + $scope.right_input[index] + ", value: " + value);
                                $scope.updateMultiPairwiseObjectValue(value, advantage, index, event);
                            }
                        }

                    }, 200);

                } else if (value[0] == null || value[0] == "") {
                    if (is_main_bar) {
                        $scope.output.value = -2147483648000;
                        $scope.output.advantage = 0;
                        //$scope.pw_advantage = 0;

                        var slider = $scope.getNoUiSlider();
                        slider.noUiSlider.set([400, 1200]);
                        setNoUiColor(false, -1);
                    } else {
                        $scope.updateMultiPairwiseObjectValue(-2147483648000, 0, index, event);
                        $scope.setNoUiSlider(400, 1200, index);
                        $scope.oldSliderValue[index] = 400;
                        setNoUiColor(false, index);
                        outerIndex = -1;
                    }
                }
            }

           
        }

        //$scope.focus_graphical_input = function () {
        //if ($scope.isMobile()) {
        //    $timeout(function () {
        //        $scope.$parent.footer_down();
        //    },1000)


           
        //}

        $scope.focus_graphical_input = function (index) {
            var elem = document.activeElement;
            if ($scope.isMobile()) {
                $timeout(function() {
                    $scope.$parent.footer_down();

                    if (!$scope.collapsed_info_docs[0]) {
                        $scope.$apply(function() {
                            $scope.collapsed_info_docs[0] = true;
                        });
                    }
                }, 500).then(function () {
                    $scope.set_multi_index(index);
                    $scope.scroll_to_active_index();
                });
            }
        }

        $scope.temp_function = function () {

        }

        $scope.blur_graphical_input = function () {

            if ($scope.isMobile()) {
                $timeout(function () {
                    $scope.$parent.footer_up();

                    if ($scope.collapsed_info_docs[0] && !$scope.is_html_empty($scope.output.parent_node_info)) {
                        $scope.$apply(function() {
                            $scope.collapsed_info_docs[0] = false;
                        });
                    }
                }, 200);
            }
        }


        $scope.resize_UC_canvas = function () {
            var chart = $scope.chart;

            $scope.canvas_width = chart.chartArea.right - chart.chartArea.left - 3; //3 px for allowance to the right
            $scope.left_margin = chart.chartArea.left;
            $scope.canvas_height = chart.chart.height;
            $("#sliderCurve").attr("style", "width:" + $scope.canvas_width + "px !important; margin-left:" + $scope.left_margin + "px");

            var $handlePos = $(".tt-utility-curve-wrap .ui-slider-handle").position();
            var $dragHandle = $(".tt-utility-curve-wrap .ui-slider-handle");
            resizeCurveHandle($dragHandle, $handlePos);
        }

        $(window).on("resize.doResize", function () {
            
            $scope.text_limit = $scope.$parent.text_limit;
            $timeout(function () {
                $scope.$apply(function () {               
                        try {
                            if ($scope.output.pairwise_type == "ptGraphical") {
                                $scope.initialize_Pie();
                            }
                            if ($scope.output.page_type == "atPairwise") {
                                $scope.reflow_equalizer();
                            }

                            if ($scope.output.page_type == "atNonPWOneAtATime" && ($scope.output.non_pw_type == "mtAdvancedUtilityCurve" || $scope.output.non_pw_type == "mtRegularUtilityCurve")) {
                                $scope.resize_UC_canvas();
                            }

                            if (($scope.isMobile() && $scope.is_multi)) {
                                $(".tt-resizable-panel-0").attr("style", "");
                            }


                            if (($scope.isMobile() && $scope.is_multi && $scope.output.pairwise_type == "ptVerbal")) {
                                $scope.output.collapse_bars = $scope.output.orig_collapse_bars;
                                $scope.mobile_manual_equalizer();
                            }
                            else {
                                if ($scope.is_multi && $scope.output.pairwise_type == "ptVerbal") {
                                    $scope.output.collapse_bars = false;
                                }
                            }


                        }
                        catch (e) {
                        }
                });
            }, 1000);
        });
        

        $scope.mobile_manual_equalizer = function () {

            $(".selected .left-question").attr("style", "height: !important;");
            $(".selected .right-question").attr("style", "height: !important;");

            var height = $(".selected .right-question").outerHeight();
            if ($(".selected .left-question").outerHeight() > height) {
                height = $(".selected .left-question").outerHeight();
            }

            var minHeight = 40;
            height = height < minHeight ? minHeight : height + 5;

            if (height != 0) {
                $(".left-question").attr("style", "");
                $(".right-question").attr("style", "");

                $(".selected .left-question").attr("style", "height:" + height + "px !important;");
                $(".selected .right-question").attr("style", "height:" + height + "px !important;");
            }

            $(".multi-rows").each(function (i, element) {
                if (!$(element).hasClass("selected")) {
                    var height = $(element).find(".right-question").outerHeight();
                    if ($(element).find(".left-question").outerHeight() > $(element).find(".right-question").outerHeight()) {
                        height = $(element).find(".left-question").outerHeight();
                    }
                    height = height < 50 ? 50 : height + 5;
                    if (height != 0) {
                        //balance the uneven boxes
                        if ($(element).find(".left-question").outerHeight() != $(element).find(".right-question").outerHeight()) {
                            ////console.log("index: " + i + ", left height: " + $(element).find(".left-question").outerHeight() + ", right height: " + $(element).find(".right-question").outerHeight());
                            $(element).find(".left-question").attr("style", "height:" + height + "px !important;");
                            $(element).find(".right-question").attr("style", "height:" + height + "px !important;");
                        }
                    }
                }
            });
        }

        $scope.$on("$destroy", function () {
            $(window).off("resize.doResize"); //remove the handler added earlier
        });

        $scope.swap_value = function (index) {
            var value;
            var advantage;
            if (index != null) {
                if ($scope.left_input[index] != 1 || $scope.right_input[index] != 1) {
                    if (parseFloat($scope.left_input[index]) > parseFloat($scope.right_input[index])) {
                        value = $scope.left_input[index] / $scope.right_input[index];
                        advantage = 1;
                    }
                    if (parseFloat($scope.left_input[index]) < parseFloat($scope.right_input[index])) {
                        value = $scope.right_input[index] / $scope.left_input[index];
                        advantage = -1;
                    }

                }
                if (value < -999999999999 || value == 0)
                    advantage = advantage;
                else
                    advantage *= -1;

                value = typeof (value) == "undefined" ? -2147483648000 : value;
                $scope.add_multivalues(value, advantage, index);
                var count = $scope.output.multi_pw_data.length;
                $scope.multi_data = $scope.output.multi_pw_data;
                $scope.temp_multi_data = $scope.multi_data;
                for (i = 0; i < count; i++) {
                    if (index == i) {
                        if ($scope.multi_data[i].Advantage == -1) {
                            var reverse = (800 * (($scope.multi_data[i].Value) / (($scope.multi_data[i].Value) + 1)));
                            $scope.participant_slider[i] = reverse;
                        }
                        if ($scope.multi_data[i].Advantage == 1) {
                            $scope.participant_slider[i] = 800 - (800 * (($scope.multi_data[i].Value) / (($scope.multi_data[i].Value) + 1)));
                        }
                        if ($scope.multi_data[i].Advantage == 0) {
                            $scope.participant_slider[i] = 400;

                        }


                        if ($scope.multi_data[i].Value == 0) {
                            $scope.participant_slider[i] = 800;
                            $scope.left_input[i] = "";
                            $scope.right_input[i] = "";

                        }
                        var slidervalue = $scope.participant_slider[i];
                        var lefttxt = $scope.left_input[index];
                        var righttxt = $scope.right_input[index];
                        $timeout(function () {
                            $scope.setNoUiSlider(slidervalue, slidervalue + 800, index);
                            //console.log(lefttxt + " : " + righttxt);
                            $scope.$apply(function () {
                                $scope.left_input[index] = parseFloat(advantage > 0 ? righttxt * 1 : righttxt);
                                $scope.right_input[index] = parseFloat(advantage < 0 ? lefttxt * 1 : lefttxt);

                            });

                        }, 0);



                        //$(".g-slider" + index + " .graphicalSlider ").slider("value", $scope.participant_slider[i]);
                    }
                }
            }
            else {

                if ($scope.graphical_switch) {
                    $timeout(function () {
                        var slider = $scope.getNoUiSlider(index);
                        var val = slider.noUiSlider.get();
                        val[0] = 1600 - parseFloat(val[0]);
                        val[1] = 1600 - parseFloat(val[1]);

                        slider.noUiSlider.set([val[1], val[0]]);

                    }, 0);
                }
                if ($scope.main_bar[0] != 1 || $scope.main_bar[1] != 1) {
                    if ($scope.main_bar[0] > $scope.main_bar[1]) {
                        value = $scope.main_bar[0];
                        $scope.main_bar[0] = $scope.main_bar[1];
                        advantage = 1;
                        $scope.main_bar[1] = value;
                        value = $scope.main_bar[1] / $scope.main_bar[0];
                    }
                    else if ($scope.main_bar[0] < $scope.main_bar[1]) {
                        value = $scope.main_bar[1];
                        advantage = -1;
                        $scope.main_bar[1] = $scope.main_bar[0];
                        $scope.main_bar[0] = value;
                        value = $scope.main_bar[0] / $scope.main_bar[1];
                    }
                }
                if (value < -999999999999 || value == 0)
                    advantage = advantage;
                else
                    advantage *= -1;
                value = typeof (value) == "undefined" ? -2147483648000 : value;
                $scope.save_pairwise(value, advantage);
            }

        }

        $scope.$parent.hide_piechart = [true];
        // END OF PAIRWISE GRAPHICAL

        // STEP FUNCTION
        var fStepPriorityLoaded = false;
        function StepInitPriorityLabel() {
            if (!fStepPriorityLoaded) {
                fStepPriorityLoaded = true;
                var priorityLabelPlugin = {
                    afterDraw: function (chartInstance) {

                        var yScale = chartInstance.scales["y-axis-0"];
                        var xScale = chartInstance.scales["x-axis-0"];
                        var canvas = chartInstance.chart;
                        var ctx = canvas.ctx;
                        var index;
                        var line;
                        var style;
                        ctx.font = "bold 12pt Arial";
                        //ctx.globalAlpha = 0.5;
                        if (chartInstance.options.priorityLabel) {
                            for (index = 0; index < chartInstance.options.priorityLabel.length; index++) {
                                line = chartInstance.options.priorityLabel[index];

                                if (!line.style) {
                                    style = "rgb(231, 76, 60)";
                                } else {
                                    style = line.style;
                                }

                                if (line.y || line.y == 0) {
                                    yValue = yScale.getPixelForValue(line.y);
                                } else {
                                    yValue = 0;
                                }
                                if (line.x) {
                                    xValue = xScale.getPixelForValue(line.x);
                                } else {
                                    xValue = 0;
                                }

                                ctx.lineWidth = 3;

                                if (yValue) {
                                    //ctx.beginPath();
                                    //ctx.moveTo(0, yValue);
                                    //ctx.lineTo(canvas.width, yValue);
                                    //ctx.strokeStyle = style;
                                    //ctx.stroke();
                                }



                                if (line.text) {

                                    if (line.reverse) {
                                        if (line.bottom) {
                                            ctx.fillStyle = "black";
                                            ctx.globalAlpha = 0.4;
                                            ctx.fillRect(xValue + 5, yValue + ctx.lineWidth, 70, 30);
                                            ctx.globalAlpha = 1.0;
                                            ctx.fillStyle = style;
                                            ctx.fillText(line.text, xValue + 5, yValue + ctx.lineWidth + 20);
                                        }
                                        else {
                                            ctx.fillStyle = "black";
                                            ctx.globalAlpha = 0.4;
                                            ctx.fillRect(xValue + 5, yValue + ctx.lineWidth - 40, 70, 30);
                                            ctx.globalAlpha = 1.0;
                                            ctx.fillStyle = style;
                                            ctx.fillText(line.text, xValue + 5, yValue + ctx.lineWidth - 20);

                                        }

                                    }
                                    if (!line.reverse) {
                                        if (line.bottom) {
                                            ctx.fillStyle = "black";
                                            ctx.globalAlpha = 0.4;
                                            ctx.fillRect(xValue - 70, yValue + ctx.lineWidth, 70, 30);
                                            ctx.globalAlpha = 1.0;
                                            ctx.fillStyle = style;
                                            ctx.fillText(line.text, xValue - 65, yValue + ctx.lineWidth + 20);
                                        }
                                        else {
                                            ctx.fillStyle = "black";
                                            ctx.globalAlpha = 0.4;
                                            ctx.fillRect(xValue - 70, yValue + ctx.lineWidth - 40, 70, 30);
                                            ctx.globalAlpha = 1.0;
                                            ctx.fillStyle = style;
                                            ctx.fillText(line.text, xValue - 65, yValue + ctx.lineWidth - 20);
                                        }

                                    }


                                }


                            }
                            return;
                        };
                    }
                };
                Chart.pluginService.register(priorityLabelPlugin);
            }
            
        }
        $scope.stepChart = null;
        function render_step_function(min, max, stepX, stepY) {
            //        $(".tt-step-function-wrap").hide();
            if ($scope.stepChart != null ? $scope.stepChart.destroy() : 1 + 1);
            fStepPriorityLoaded = false;
            StepInitPriorityLabel();

            var value = parseFloat($scope.output.current_value);
            $(".load-canvas-gif").show();
            $scope.xAxisLabels = [];

            var canvas = document.getElementById("sFunctionCanvas");
            var ctx = canvas.getContext("2d");
            var overridenStep = (max - min) / 7.5;

            Chart.defaults.global.legend.display = false; //remove label at the top of graph
            Chart.defaults.global.tooltips.enabled = false;
            if (value < min) value = min;
            if (value > max) value = max;
            var canvasData = getXandYofPriority(value, getUCStepData(min, max, stepX, stepY)[0]);

            if (canvasData[0] != "")
                canvasData[0] = parseFloat((canvasData[0])).toFixed(2);

            $scope.stepChart = Chart.Line(ctx, {
                data: {
                    datasets: [{
                        label: $scope.output.child_node,
                        data: getUCStepData(min, max, stepX, stepY)[0],
                        lineTension: 0,
                        borderColor: "#0058a3",
                        fill: false,
                        pointBackgroundColor: "#ecfaff"
                    }]
                },

                options: {
                    responsive: true,
                    scales: {
                        xAxes: [{
                            type: "linear",
                            position: "bottom",
                            scaleLabel: {
                                display: true,
                                labelString: "Data"

                            },
                            ticks: {
                                min: min,
                                max: max,
                                fontSize: 10,
                                fixedStepSize: overridenStep,
                                callback: function (label, index, labels) {
                                    try {
                                        var fInt = parseFloat(label);
                                        label = fInt.toFixed(2);
                                    }
                                    catch (e) {

                                    }
                                    if (index == 0) {
                                        label = "-inf";
                                    }
                                    if (index == labels.length - 1) {
                                        label = "+inf";
                                    }


                                    return label;
                                }
                            }
                        }],
                        yAxes: [{
                            scaleLabel: {
                                display: true,
                                labelString: "Priority"
                            },
                            ticks: {
                                callback: function (label, index, labels) {
                                    if ($scope.is_float(label)) {
                                        label = parseFloat(label).toFixed(2);
                                    }
                                    return label + ".00%";
                                }
                            }
                        }]
                    },
                    priorityLabel: [{
                        "x": $scope.output.current_value == -2147483648000 ? $scope.output.current_value : (value == 0 ? 0.00000001 : value),
                        "y": canvasData[0],
                        "style": "white",
                        "text": canvasData[0] == "" || $scope.output.current_value == -2147483648000 ? + "" : canvasData[0] + " %",
                        "reverse": canvasData[1],
                        "bottom": canvasData[2]
                    }]
                }

            });

           // //console.log($scope.stepChart);
            $(".tt-step-function-wrap").show();
            $(".load-canvas-gif").hide();
            $timeout(function () {
                $scope.canvas_width = $scope.stepChart.chartArea.right - $scope.stepChart.chartArea.left; //3 px for allowance to the right
                $scope.left_margin = $scope.stepChart.chartArea.left;
                $scope.canvas_height = $scope.stepChart.chart.height;
                $("#stepsFunctionSlider").width($scope.canvas_width).css({ left: $scope.left_margin });

            }, 500);

        }

        $scope.checkGPtoVerbal = function (data) {
            if (!$scope.gptoVerbal) {
                $scope.gptoVerbal = data.Value % 1 > 0 ? gptoVerbal = true : false;
            }

        }


        function getUCStepData(XMin, XMax, StepsX, StepsY) {
            var data = [[]];
            var Shift = (XMax - XMin) / 100;
            data[0].push({ x: XMin, y: StepsY[0] * 100 });
            if ($scope.output.piecewise) {
                for (var i = 0; i <= StepsX.length - 1; i++) {
                    data[0].push({ x: StepsX[i], y: StepsY[i] * 100 });

                };
            }
            else {
                for (var i = 0; i <= StepsX.length - 1; i++) {
                    data[0].push({ x: StepsX[i], y: StepsY[i] * 100 });
                    if (i == StepsX.length - 1)
                        data[0].push({ x: XMax, y: StepsY[i] * 100 });
                    else {
                        data[0].push({ x: StepsX[i + 1], y: StepsY[i] * 100 });
                    }
                };

            }
            data[0].push({ x: XMax, y: StepsY[StepsY.length - 1] * 100 });
            //console.log(JSON.stringify(data));
            return data;
        }

        $scope.isValueChanged = false;
        $scope.save_StepFunction = function (val, is_erase) {
            if (!isNaN(val)) {
                //if (val != null) {
                    val = (val == null ? -2147483648000 : parseFloat(val));
                    is_erase = (val == -2147483648000);
                    $scope.step_input = val;
                    $scope.isValueChanged = $scope.output.current_value != val;

                    //console.log("val: " + val + ", is_erase: " + is_erase);

                    $timeout(function() {
                        $scope.$apply(function () {
                            $scope.$parent.disableNext = $scope.output.pipeOptions.dontAllowMissingJudgment && is_erase;
                            $scope.output.IsUndefined = is_erase;
                            $scope.$parent.output.IsUndefined = is_erase;
                        });
                    }, 50);

                    if (val < $scope.output.PipeParameters.min) val = $scope.output.PipeParameters.min;
                    if (val > $scope.output.PipeParameters.max) val = $scope.output.PipeParameters.max;

                    if (val < $scope.lowest_value) {
                        var min = $(".tt-steps-slider").slider("option", "min");
                        $(".tt-steps-slider").slider("value", min);
                    }

                    $(".tt-steps-slider").slider("value", val);
                    var stepLabel = getXandYofPriority(val, $scope.stepChart.data.datasets[0].data);

                    $scope.stepChart.options.priorityLabel[0].x = val == 0 ? 0.00000001 : val;
                    $scope.stepChart.options.priorityLabel[0].y = stepLabel[0];
                    $scope.stepChart.options.priorityLabel[0].reverse = stepLabel[1];
                    $scope.stepChart.options.priorityLabel[0].bottom = stepLabel[2];
                    
                    if (is_erase) {
                        angular.element(".next_step").removeClass("back-pulse");
                        $scope.step_input = "";
                        $("#steps-functionInput").val("");
                        $scope.stepChart.options.priorityLabel[0].text = "";
                        $scope.set_slider_gray(".tt-steps-slider");
                    } else {
                        $scope.stepChart.options.priorityLabel[0].text = stepLabel[0].toFixed(2) + "%";
                        $scope.set_slider_blue(".tt-steps-slider");
                        angular.element(".next_step").addClass("back-pulse");
                    }

                    $scope.stepChart.update();

                    // var loadingMessage = stringResources.yellowLoadingIcon.save;
                //}
            }
        }

        // END OF STEP FUNCTION

        $scope.save_step_function_on_next = function () {
            var val = ($scope.step_input == null || $scope.step_input.toString().length == 0 ? -2147483648000 : $scope.step_input);
            val = $scope.isValueChanged ? val : $scope.output.current_value;
            $scope.output.current_value = val;

            $http.post(
                $scope.baseurl + "pages/Anytime/Anytime.aspx/SaveStepFunction",
                JSON.stringify({
                    step: $scope.current_step,
                    value: val,
                    sComment: $scope.output.comment
                })
            ).then(function success(response) {
                $scope.$parent.footer_up();
                $scope.isValueChanged = false;

                if ($scope.is_comment_updated || $scope.isAutoSaving) {
                    $scope.is_saving_judgment = true;
                    
                    var timeOutValue = $scope.isAutoSaving ? 300 : 0;
                    $timeout(function () {
                        $scope.get_pipe_data($scope.current_step);
                    }, timeOutValue, false);
                }

                $timeout(function () {
                   // $scope.$parent.runProgressBar(2, 100, 100, '', true);
                    $scope.is_comment_updated = false;
                }, 2000);
            });
        }

        //Local Results
        $scope.currentColumn;
        $scope.sort_Results = function (request, global) {
            var reverse;
            if (global)
                reverse = $scope.reverseGlobal;
            else
                reverse = $scope.reverseLocal;

            if ($scope.currentColumn == request)
                reverse = !reverse;
            else {
                if (request == "nodeName" || request == "nodeID") {
                    reverse = false;
                } else {
                    reverse = true;
                }
            }
            var columntosort = -1;
            switch (request) {
                case "nodeID":
                    $scope.columnname = "this[0]";
                    columntosort = 0;
                    break;
                case "nodeName":
                    $scope.columnname = "this[1]";
                    columntosort = 1;
                    break;
                case "yourResults":
                    $scope.columnname = "this[2]";
                    columntosort = 2;
                    break;
                case "combine":
                    $scope.columnname = "this[3]";
                    columntosort = 3;
                    break;
            }

            $scope.currentColumn = request;
            $scope.$parent.resultsreverse = reverse;
            $scope.$parent.columnsort = $scope.currentColumn;
            if (global){
                $scope.reverseGlobal = reverse;
                $scope.columnsortGlobal = $scope.columns[columntosort];
            }
            else {
                $scope.reverseLocal = reverse;
                $scope.columnsortLocal = $scope.columns[columntosort];
            }
               
            var resulttype = 0;
            if ($scope.output.page_type == "atShowGlobalResults")
                resulttype = 2;
            if ($scope.output.page_type == "atShowLocalResults")
                resulttype = 1;

            //$http.post(
            //    $scope.baseurl + "pages/Anytime/Anytime.aspx/saveSort",
            //    JSON.stringify({
            //        sortmode: columntosort,
            //        sortwhere: resulttype
            //    })
            //    )
            //.then(function success(response) {
            //});
        }

        //11215
        $scope.hideEqualMessage = { "once": false, "all": false };
        $scope.setEqualMessage = function (once, all) {
            var allCluster = false;
            if (all) {
                allCluster = true;
            }
            if (all || once) {
                $http.post(
                    $scope.baseurl + "pages/Anytime/Anytime.aspx/HideEqualMessage",
                    JSON.stringify({
                        step: $scope.output.current_step,
                        allCluster: allCluster
                    })
                    )
                .then(function success(response) {
                });
            }
        }

        $scope.$on("sortcolumns", function (e) {
            $scope.sort_Results($scope.$parent.columnsort);
        });
        $scope.$on("reverse", function (e) {
            $scope.reverse = $scope.$parent.resultsreverse;
        });
        $scope.$on("someEvent", function (e) {

            if ($scope.output.page_type == "atShowLocalResults") {
                $scope.normalization_Change($scope.$parent.normalization, false);
            }
            else {
                $scope.normalization_Change($scope.$parent.normalization, true);
            }

        });
        //broadcast
        $scope.$on("get_pipe_data", function (e) {
            $scope.cancelAutoSaveJudgmentsTimer();
            var pipeData = $scope.$parent.pipe_data;
            $scope.get_pipe_data(pipeData[0], pipeData[1], pipeData[2]);
        });

        $scope.$on("save_survey", function (e, isFinish) {
            $scope.save_respondentAnswer($scope.output.current_step, $scope.output.current_step, isFinish);
        });

        $scope.$on("check-multi-height", function (e, divClass, divHeight) {
            $timeout(function() {
                $scope.CheckMultiHeight(divClass, divHeight);
            }, 100);
        });

        $scope.get = function () {
            return "LOL";
        }
        $scope.normalization_Change = function (value, is_global) {
            //  $(".results-redo-pairs").css("display", "inline !important");
            if (!is_global) {
                $http({
                    method: "POST",
                    url: baseUrl + "pages/Anytime/Anytime.aspx/changeNormalization",
                    async: false,
                    data: {
                        normalization: value.value,
                        step: $scope.output.current_step,
                        fGlobal: is_global,
                        wrtNodeID: -1
                    }
                })
                .then(function success(response) {
                    $scope.normalization = value;
                    $scope.normalization.value = value.value;
                    var results = response.data.d;
                    $scope.output.PipeParameters = results;
                    $scope.output.PipeParameters.checkbox_items = $scope.checkbox_items;
                    $scope.results_data = $scope.output.PipeParameters.results_data;
                    $scope.output.PipeParameters.StepPairsData = eval($scope.output.PipeParameters.StepPairsData);
                    $scope.output.PipeParameters.ObjectivesData = eval($scope.output.PipeParameters.ObjectivesData);
                    $scope.get_highest_result($scope.results_data);
                    getMatrixTemplate();

                }, function error(response) {
                    //console.log(response);
                });
            }

            else if (is_global) {
                $http({
                    method: "POST",
                    url: baseUrl + "pages/Anytime/Anytime.aspx/changeNormalization",
                    async: false,
                    data: {
                        normalization: value.value,
                        step: $scope.output.current_step,
                        fGlobal: is_global,
                        wrtNodeID: $scope.wrt_node_id
                    }
                })
                .then(function success(response) {
                    try {
                        $scope.normalization = value;
                        var results = response.data.d;
                        $scope.output.PipeParameters = results;
                        $scope.results_data = $scope.output.PipeParameters.results_data;
                        $scope.canshowresult = $scope.output.PipeParameters.canshowresult;
                        $scope.output.PipeParameters.StepPairsData = eval($scope.output.PipeParameters.StepPairsData);
                        $scope.output.PipeParameters.ObjectivesData = eval($scope.output.PipeParameters.ObjectivesData);
                        //$scope.get_highest_result("combine_data", results);
                        $scope.get_highest_result($scope.results_data);
                        $scope.ExpectedValue = results.ExpectedValue;
                        //console.log($scope.output.PipeParameters);
                    }
                    catch (e) {
                        //console.log($scope.output.PipeParameters);
                    }
                    $scope.wrt_node_id = $scope.wrt_node_id;
                }, function error(response) {
                    //console.log(response);
                });
            }
        }

        $scope.save_WRTNodeID = function (wrtnodeID) {
            $http.post(
                $scope.baseurl + "pages/Anytime/Anytime.aspx/GetOverallResultsData",
                JSON.stringify({
                    rmode: $scope.normalization.value,
                    wrtnodeID: wrtnodeID,
                    isReload: true
                })
                )
            .then(function success(response) {
                $scope.$parent.wrt_id = wrtnodeID;
                //$scope.normalization.value,
                $scope.results_data = response.data.d[0];
                $scope.canshowresult = response.data.d[1];
                $scope.wrt_node_id = wrtnodeID;
                $scope.get_highest_result();
                $scope.wrt_node_name = response.data.d[2];
                //console.log($scope.output.PipeParameters.highest_result);
            });
        }

        $scope.get_highest_result = function (results) {
            //sort by integersho
            angular.forEach($scope.results_data, function (result) {
                result[0] = parseFloat(result[0]);
                result[2] = parseFloat(result[2]);
                result[3] = parseFloat(result[3]);
            });
            if (!results)
                results = $scope.results_data;
            $scope.output.PipeParameters.highest_result = results[0][2];
            for (i = 0; i < results.length; i++) {
                if ($scope.output.PipeParameters.highest_result < results[i][3]) {
                    $scope.output.PipeParameters.highest_result = results[i][3];
                }
                if ($scope.output.PipeParameters.highest_result < results[i][2]) {
                    $scope.output.PipeParameters.highest_result = results[i][2];
                }

            }
        }



        $scope.normalization_list = [{ "value": 1, "text": "Normalized" }, { "value": 2, "text": "% of Maximum" }, { "value": 3, "text": "Unnormalized" }];
        $scope.normalization = $scope.normalization_list[0];
        $scope.reverse = true;
        $scope.sort = function () {
            $scope.reverse = !$scope.reverse; $scope.columnname = $scope.results_data[0][0];
        }
        
        $scope.get_results = function (request, result) {
            //if (request == "own") {
            //    for (i = 0; i < child.length; i++) {
            //        if ($scope.current_user[1] == child[i][0]) {
            //            temp_own_value = child[i][$scope.normalization.value];
            //            if (temp_own_value <= 0) { temp_own_value = 0; }
            //            return temp_own_value;
            //        }
            //    }
            //}
            //if (request == "combine") {
            //    temp_combine_value = child[child.length - 1][$scope.normalization.value];
            //    if (temp_combine_value <= 0) { temp_combine_value = 0; }
            //    return temp_combine_value;
            //}
            if (!$scope.output.PipeParameters.canshowresult)
                $scope.get_highest_result($scope.output.PipeParameters.results_data);
            var perRes = 0;
            if ($scope.output.PipeParameters == null) {
                return "";
            }
            var resvar = (parseFloat(result) * 100).toFixed(2);
            if ($scope.output.PipeParameters.highest_result == 0)
                return { "width": "0%" };
            var highestResult = (parseFloat($scope.output.PipeParameters.highest_result) * 100);
            
            if (request == "own_bar") {
                perRes = (resvar / highestResult) * 100;
                if (perRes == "NaN")
                    perRes = 0;
                return { "width": perRes + "%" };
            }
            if (request == "combine_bar") {
                perRes = (resvar / highestResult) * 100;
                if (perRes == "NaN")
                    perRes = 0;
                return { "width": perRes + "%" };
            }
        }



        $scope.inconsistency = false;
        $scope.saveInconsistency = function (bool) {
            $scope.inconsistency = bool;
            //if (bool) {
            //    $scope.reOrderMatrixTable();
            //    $scope.changedMatrixValues = [];
            //    matrixCellIDs = [];
            //}
            getMatrixTemplate(bool);
            $scope.enableReorderBtn = false;
            if (bool) {
                $("input[id='reviewjudgment']").prop("checked", true);
                $("input[id='changejudgment']").prop("checked", false);
            }
            else {
                $("input[id='changejudgment']").prop("checked", true);
                $("input[id='reviewjudgment']").prop("checked", false);
            }
        }
        // this should be constant
        $scope.check_limit = 2;

        //value to check if pipe is under review on intermediate results
        $scope.$parent.is_under_review = false;
        $scope.node_names = [];
        $scope.checked_boxes = 0;
        $scope.checkbox_items = [];
        $scope.selectCheckbox = function (is_checked, node_name) {
            if (is_checked) {
                $scope.checked_boxes++;
                $scope.node_names.push(node_name);
            }
            else {
                $scope.checked_boxes--;
                var index = $scope.node_names.indexOf(node_name);
                $scope.node_names.splice(index, 1);
            }
        }

        $scope.selectReevaluateRow = function (index, node_name) {

            if ($scope.checked_boxes < $scope.check_limit || $scope.checkbox_items[index] == true) {
                $scope.checkbox_items[index] = !$scope.checkbox_items[index];
                $(".itemCheckbox-" + index).prop("checked", $scope.checkbox_items[index]);
                $scope.selectCheckbox($scope.checkbox_items[index], node_name);
                switch ($scope.checked_boxes)
                {
                    case 0:
                        $("#results-title").html("Select a pair of element: 0/2");
                        break;
                    case 1:
                        $("#results-title").html("Select a pair of element: 1/2");
                        break;
                    case 2:
                        $("#results-title").html("Click the Re-evaluate button");
                        break;
                }
            }
           
        }
        $scope.saveOBPriority = function (chb) {
            $scope.output.orderbypriority = chb;
            //$scope.reOrderMatrixTable();
            $http.post(
                    $scope.baseurl + "pages/Anytime/Anytime.aspx/saveOBPriority",
                    JSON.stringify({
                        chb: chb
                    })
                    )
                .then(function success(response) {
                    //case 11505, case 11440
                    var results = response.data.d;
                    $scope.output.PipeParameters.ObjectivesData = eval(results.ObjectivesData);
                    $scope.output.PipeParameters.highest_result = results.highest_result;
                    $scope.output.PipeParameters.StepPairsData = eval(results.StepPairsData);
                    var fGlowInput = false;
                    getMatrixTemplate(fGlowInput);
                    //$scope.is_saving_judgment = true;
                    //$scope.get_pipe_data($scope.current_step, null, stringResources.yellowLoadingIcon.save);


                });
        }

        $scope.savebestfit = function (chb) {
            $scope.output.bestfit = chb;
            $http.post(
                    $scope.baseurl + "pages/Anytime/Anytime.aspx/savebestfit",
                    JSON.stringify({
                        chb: chb
                    })
                    )
                .then(function success(response) {
                    if (chb) {
                        $(".subscript.bestfit").fadeIn(300);
                        $(".subscript.bestfit").removeClass("hide");
                        $scope.saveInconsistency(true);
                    }
                    else {
                        $(".subscript.bestfit").fadeOut(300, function () {
                            $(".subscript.bestfit").addClass("hide");
                        });
                    }

                    //$scope.is_saving_judgment = true;
                    //$scope.get_pipe_data($scope.current_step);

                });
        }


        //GetMultiPWData
        function GetMultiPW() {
            var test = [];
            $.get($scope.baseurl + "templates/MultiPairwise.html", function (data, status) {
              
                test = _.template(data)({
                    output: $scope.output,
                    is_AT_owner: $scope.is_AT_owner
                });

                $("#multi-pw-div").html(test);
                $("#multi-pw-div").removeClass("hide");
            });
        }

        
        //Local Results Matrix table
        function getMatrixTemplate (glowMatrixInput) {
            var tableHeaders = [];
            var legendLength = $(".tt-matrix-table-legend").length;
            //if (!$scope.$parent.isMobile()) {
            //    legendLength = 0;
            //}
            legendLength = 0;
            $http({
                method: "GET",
                url: $scope.baseurl + "Templates/JudgmentTable.html",
                beforeSend: function () {
                    LoadingScreen.init(LoadingScreen.type.loadingModal, $(".LoadingModalPercentage"), $(".fullwidth-loading-wrap"));
                    LoadingScreen.start(70, loading_message);
                },
            }).success(function (data, status) {
                var stepPairs = eval($scope.output.PipeParameters.StepPairsData);
                //console.log("Step pairs: "+typeof(stepPairs));
                for (var i = 0; i < stepPairs.length; i++) {
                    var pair = stepPairs[i];
                    if (parseFloat(pair[1]) != 0) {
                        if (parseFloat(pair[1]) % 1 != 0)
                            stepPairs[i][1] = parseFloat(pair[1].toFixed(2));
                        else
                            stepPairs[i][1] = parseInt(pair[1]);
                    }
                }
                $scope.output.PipeParameters.StepPairsData = stepPairs;
                tableHeaders = _.template(data)({
                    checkbox_items: $scope.output.PipeParameters.checkbox_items,
                    headers: eval($scope.output.PipeParameters.ObjectivesData),
                    objectives: eval($scope.output.PipeParameters.ObjectivesData),
                    highestResult: $scope.output.PipeParameters.highest_result,
                    results: eval($scope.output.PipeParameters.ObjectivesData),
                    stepPairs: eval($scope.output.PipeParameters.StepPairsData),
                    legend: $scope.matrixbox.legend,
                    legendLength: legendLength,
                    rank: $scope.matrixbox.rank,
                    bestfit: $scope.output.bestfit,
                    inconsistency: $scope.inconsistency,
                    orderbypriority: true
                });
                if ($(".table-wrapper .pinned").length > 0) {
                    //$("#hiddentable").html("");
                    $("#hiddentable").html(tableHeaders);
                    Sswitched = false;
                    var content = updateTables(true);
                    //console.log($("#hiddentable").html());

                    $("#tableshouldbe").html(content);
                    
                    //$scope.executeResponse();


                }
                else {
                    $("#tableshouldbe").html(tableHeaders);
                }
                $(".priorities-title").each(function (i, e) {
                    var title = $(e).attr("title");
                    $("#" + i + ".tooltip.tip-top").text(title);
                });
                if (glowMatrixInput) {
                    glowMatrix(false, "#" + matrixTargetID);
                    $("#" + matrixTargetID).focus();
                }
            });
            

        }

        $(document).on("click touchstart", ".priorities-title", function () {
            var thisElement = this;
            //$(".priorities-name", this).addClass("hide");
            //$(".priorities-percent", this).fadeIn(300, function () {
            //    $(this).fadeOut(2000, function () {
            //        $(".priorities-name", thisElement).fadeIn();
            //        $(".priorities-name", thisElement).removeClass("hide");
            //    });
            //});
            $(".priorities-name", this).fadeOut();
            $(".priorities-percent", this).fadeIn();

            setTimeout(function () {
                $(".priorities-name", thisElement).fadeIn();
                $(".priorities-percent", thisElement).fadeOut();
            },2000);
        });

        var matrixTargetID = -1;
        $(document).on("keyup", ".matrix-cell", function (e) {
            var value = $(this).val();
            var index = $(this).data("index");
            var operationID = $(this).data("operation");
            var pairs = eval("[" + $(this).data("pairs") + "]");
            $scope.checkKeyPress(value, operationID, pairs, $scope.output.parentnodeID, e);
        });
        
        $(document).on("click", ".pwnl-click", function (e) {
            var pair1 = $(this).data("pair-left");
            var pair2 = $(this).data("pair-right");
            $scope.editPwnlClick($scope.output.parentnodeID, $scope.output.current_step, pair1, pair2);
        });

        $scope.showbtn = true;
        $scope.cancelbutton = function (e) {
            $scope.showbtn = true;
        }

        $scope.individualJudgment = function (e) {
            $scope.showbtn = false;

            $("input[type=text]").each(function (e) {
                $(this).click(function (e) {
                    var operationID = $(this).data("operation");
                    var pairs = eval("[" + $(this).data("pairs") + "]");
                    $scope.doMatrixOperation($(this).val(), operationID, pairs, $scope.output.parentnodeID, e);
                });
            });
        }

        $(document).on("change", ".matrix-cell", function (e) {
            var value = $(this).data("value");
            matrixTargetID = e.target.id;
            var newValue = $(this).val();
            newValue = newValue.length == 0 ? "0" : newValue;

            if (value != newValue) {
                var index = $(this).data("index");
                var operationID = $(this).data("operation");
                var pairs = eval("[" + $(this).data("pairs") + "]");
                $scope.doMatrixOperation(newValue, operationID, pairs, $scope.output.parentnodeID, null, true);
                //$(this).val(newValue);
            }
        });

        $(document).on("keydown", ".matrix-cell", function (e) {
            var code = e.which;

            if (((code >= 65 && code <= 72) || (code >= 74 && code <= 90)) || code == 188 || code == 191 || code == 192 || (code >= 219 && code <= 222) || code == 186 || code == 187 || code == 106 || code == 107 || code == 111 || code == 13)
                return e.preventDefault();
        });

        $scope.textField = null;
        $(document).on("focus", ".matrix-cell", function (e) {
            $scope.textField = null;
            $("#buttonInvertJudgment").removeClass("tt-button-green-normal");
            $("#buttonInvertJudgment").addClass("tt-button-transparent not-active disabled");
            $("#navInvertJudgment").removeClass("tt-button-green-normal");
            $("#navInvertJudgment").addClass("tt-button-transparent not-active-footer-navs");

            if ($(this).val().trim().length > 0 && !isNaN($(this).val())) {
                $scope.textField = this;
                $("#buttonInvertJudgment").removeClass("tt-button-transparent not-active disabled");
                $("#buttonInvertJudgment").addClass("tt-button-green-normal");
                $("#navInvertJudgment").removeClass("tt-button-transparent not-active-footer-navs");
                $("#navInvertJudgment").addClass("tt-button-green-normal");
            }
        });

        $(document).on("blur", ".matrix-cell", function (e) {
            $timeout(function () {
                var focusedElement = document.activeElement;

                if ($scope.textField == null || (focusedElement.id.indexOf("buttonInvertJudgment") < 0 && focusedElement.className.indexOf("matrix-cell") < 0)) {
                    $("#buttonInvertJudgment").removeClass("tt-button-green-normal");
                    $("#buttonInvertJudgment").addClass("tt-button-transparent not-active disabled");
                    $("#navInvertJudgment").removeClass("tt-button-green-normal");
                    $("#navInvertJudgment").addClass("tt-button-transparent not-active-footer-navs");
                    $scope.textField = null;
                }
            }, 100, false);
        });

        $scope.invertCurrentJudgment = function () {
            var textField = $scope.textField;
            if (textField != null && $(textField).val().trim().length > 0 && !isNaN($(textField).val())) {
                $(textField).val("-" + $(textField).val());
                $(textField).keyup();
                $(textField).change();
            }
        }

            $scope.checkKeyPress = function (val, operation, content, parent, e) {
                $scope.$parent.inputcode = e.keyCode;
                var code = e.which;
                //if (code == 45 || code == 105 || code == 73 || code == 189 || code == 173 || code == 109) {
                //    $scope.doMatrixOperation(val, operation, content, parent, e);
                //}
                if ((code >= 48 && code <= 57) || code == 8 || code == 190 || (code >= 96 && code <= 105) || code == 110 || code == 46)
                    $scope.doMatrixOperation(val, operation, content, parent, e);
                try {
                    if (val.includes("i") || val.includes("-")) {
                        e.which = 189;
                        $scope.doMatrixOperation(val, operation, content, parent, e);
                    }
                }
                catch (e) {
                    if (val.indexOf("-") != -1 || val.indexOf("i") != -1) {
                        e.which = 189;
                        $scope.doMatrixOperation(val, operation, content, parent, e);
                    }
                }
                
            }
        $scope.judgmenttable_txt = [];
        $scope.OBPriority = false;
        var matrixCellIDs = [];
        $scope.fInvert = false;
        $scope.doMatrixOperation = function (judgment, operationID, content, parentnodeID, e, blur) {
            if (operationID === 5) {
                var pass = $window.confirm("Are you sure you want to invert all of your judgments?");
                if (!pass)
                    return true;
                //$scope.$parent.runProgressBar(2, 0, 20, stringResources.yellowLoadingIcon.save, false);
            }

            if (operationID === 1 && operationID === 3)
                $scope.$parent.runProgressBar(2, 100, 100, stringResources.yellowLoadingIcon.save, false);
            
            
            if (e != null) {
                var code = e.which;
                //if (!(code === 105 || code === 189 || code === 45 || code === 46 || code === 73 || code === 13)) {
                //    return false;
                //}
                //fInvert = true;
                if (((code === 109 || code === 189 || code === 45 || code === 73 || code === 173))) {
                
                    $scope.fInvert = true;
                    try {
                        if (judgment.includes("-") || judgment.includes("i")) {
                            judgment = judgment.replace("-", "").replace("i", "");
                        }
                    }
                    catch (e) {
                        if (judgment.indexOf("-") != -1 || judgment.indexOf("i") != -1) {
                            judgment = judgment.replace("-", "").replace("i", "");
                        }
                    }
                }
                else if (code === 13) {
                    $scope.fInvert = false;
                }
                else {
                    return false;
                }
            }
            else {
                if (blur != null) {
                    var regex = /^\d+(\.\d{0,2})?$/;

                    try {
                        if (judgment.includes("i") || judgment.includes("-")) {
                            return false;
                        }
                    }
                    catch(e) {
                        if (judgment.indexOf("-") != -1 || judgment.indexOf("i") != -1) {
                            return false;
                        }
                    }

                    if (!(regex.test(judgment))) {
                        getMatrixTemplate();
                        return false;
                    }
                }
            }

            if (judgment) {
                $http({
                    method: "POST",
                    url: baseUrl + "pages/Anytime/Anytime.aspx/doMatrixOperation",
                    async: false,
                    data: {
                        judgment: judgment,
                        ID: operationID,
                        content: content,
                        parent: parentnodeID,
                        invert: $scope.fInvert,
                        rmode: $scope.normalization.value
                    }
                })
                    .then(function success(response) {
                        if (operationID == 1) {
                            $scope.enableReorderBtn = true;
                            $scope.output.PipeParameters.JudgmentsSaved = true;
                            var output = response.data.d;
                            matrixCellIDs.push("#cell" + output.ObjID);
                            $scope.output.PipeParameters.ObjectivesData = eval(output.results.ObjectivesData);
                            $scope.output.PipeParameters.highest_result = output.results.highest_result;
                            $scope.output.PipeParameters.StepPairsData = eval(output.results.StepPairsData);
                            $scope.output.PipeParameters.InconsistencyRatio = output.results.InconsistencyRatio;
                            $scope.output.PipeParameters.results_data = output.results.results_data;

                            var fGlowInput = true;
                            getMatrixTemplate(fGlowInput); //reset judgment table

                            $scope.$parent.runProgressBar(2, 100, 100, stringResources.yellowLoadingIcon.save, true);
                            $scope.output.PipeParameters.ClipBoardData = output.results.ClipBoardData;

                        } else {
                            $scope.enableReorderBtn = false;
                            $scope.changedMatrixValues = [];
                            matrixCellIDs = [];
                            var output = response.data.d.PipeParameters;
                            $scope.output.PipeParameters.JudgmentsSaved = output.JudgmentsSaved;
                            $scope.output.PipeParameters.ObjectivesData = eval(output.ObjectivesData);
                            $scope.output.PipeParameters.highestResult = output.highest_result;
                            $scope.output.PipeParameters.StepPairsData = eval(output.StepPairsData);
                            $scope.output.PipeParameters.InconsistencyRatio = output.InconsistencyRatio;
                            $scope.output.PipeParameters.results_data = output.results_data;

                            var fGlowInput = false;
                            getMatrixTemplate(fGlowInput);

                            glowMatrix(true);
                            $scope.$parent.runProgressBar(2, 100, 100, stringResources.yellowLoadingIcon.save, true);
                            //$scope.get_pipe_data($scope.current_step);
                            $scope.output.PipeParameters.ClipBoardData = output.ClipBoardData;
                        }
                        //
                    }, function error(response) {
                        //console.log(response);
                    });
            }
        }

        $scope.reOrderMatrixTable = function () {
            //$scope.output.orderbypriority = true;
            //$scope.saveOBPriority(true);
            $scope.enableReorderBtn = false;
        }

        $scope.changedMatrixValues = [];
        $scope.changeMatrixData = function (first, judgment, content, parentnodeID, e) {
            $scope.enableReorderBtn = true;
            var fInvert = false;
            if (e != null) {
                var code = e.which;
                if (((code === 105 || code === 109 || code === 189 || code === 45 || code === 46 || code === 73))) {
                    fInvert = true;
                }
                else if (((code >= 48 && code <= 57) || (code >= 96 && code <= 105) || code === 13)) {
                    fInvert = false;
                }
                else {
                    return false;
                }
                if ($scope.changedMatrixValues[first][4] == true)
                    fInvert = false;
            }
            //indexes
            $scope.changedMatrixValues[first][0] = true;
            $scope.changedMatrixValues[first][1] = judgment;
            $scope.changedMatrixValues[first][2] = content;
            $scope.changedMatrixValues[first][3] = parentnodeID;


            $scope.changedMatrixValues[first][4] = fInvert;
            if (fInvert)
                $scope.doMatrixOperation(judgment, 1, content, parentnodeID, e);

        }

        //Local Results Matrix table
        $scope.setMatrixLegend = function (show) {
            if (show) {
                $(".tt-matrix-table-legend").removeClass("hide");
                $("#matrixtable").removeClass("large-12").addClass("large-10");
            }
            else {
                $(".tt-matrix-table-legend").addClass("hide");
                $("#matrixtable").removeClass("large-10").addClass("large-12");
            }
        }

        $scope.setMatrixRank = function (bool) {
            if (bool) {
                $(".superscript.rank").fadeIn(300);
                $(".superscript.rank").removeClass("hide");
                $scope.saveInconsistency(true);
            }
            else {
                $(".superscript.rank").fadeOut(300, function () {
                    $(this).addClass("hide");
                });
            }
        }

        $scope.matrixbox = { "rank": false, "legend": true };
        $scope.isRedNumber = false;
        $scope.checkRedNumber = function (finvert) {
            $scope.isRedNumber = true;

            if (finvert) {
                return "black";
            }

            return "red";
        }

        $scope.checkBlackNumber = function (finvert) {
            if (finvert) {
                return "red";
            }

            return "black";
        }

        //$scope.saveRank = function (Rank) {
        //    $http.post(
        //            $scope.baseurl + "pages/Anytime/Anytime.aspx/savebestfit",
        //            JSON.stringify({
        //                chb: chb
        //            })
        //            )
        //        .then(function success(response) {
        //            $scope.get_pipe_data($scope.current_step);
        //        });
        //}

        function getStepNumber(id1, id2) {
            var retVal = -1;
            var o1 = parseInt(id1);
            var o2 = parseInt(id2);
            //console.log("o1: " + o1 + "o2:" + o2);
            
            var step_data, sd, pwn_sd;
            step_data = eval($scope.output.PipeParameters.StepPairsData);
            if (step_data) {
                    for (var i = 0; i < step_data.length; i++) {
                        pwn_sd = step_data[i];
                        if ((((parseInt(pwn_sd[2]) == o1) && (parseInt(pwn_sd[3]) == o2)) || ((parseInt(pwn_sd[2]) == o2) && (parseInt(pwn_sd[3]) == o1))) && parseInt(pwn_sd[0]) > 0) {
                            retVal = 1;
                            break;
                        }
                    }
                }
            return retVal;
        }

        $scope.editPwnlClick = function (parentnodeID, current_step, id1, id2) {
            var addstep = false;
            if (getStepNumber(id1, id2, true) < 0)
                addstep = true;
            $http.post(
                    $scope.baseurl + "pages/Anytime/Anytime.aspx/redoPairs",
                    JSON.stringify({
                        parentnodeID: parentnodeID,
                        current_step: current_step,
                        firstNode: id1,
                        secondNode: id2,
                        is_name: false,
                        add_step: addstep
                    })
                    )
                .then(function success(response) {
                    if ($scope.$parent.isMobile())
                        $(".tt-mobile-footer").removeClass("hide");
                    var step = response.data.d;
                    if (step == current_step)
                        addstep = true;
                    if (addstep) {
                        $scope.load_step_list();
                        $scope.previous_cluster_step = current_step + 1;
                        $scope.get_pipe_data(current_step, $scope.is_AT_owner);
                        return true;
                    }


                    if (step <= 0) {
                        $scope.active_multi_index = Math.abs(response.data.d);
                        step = JSON.stringify(parseInt($scope.output.PipeParameters.StepPairsData[0][0]) + 1);
                        //console.log("editPwnlClick -> active_multi_index: " + $scope.active_multi_index);
                    }
                    $scope.previous_cluster_step = current_step;
                    $scope.$parent.is_under_review = true;
                    $scope.matrixReview = true;
                    $scope.get_pipe_data(step);
                });
        }

        $scope.finishtable = true;

        $scope.executeResponse = function () {
            //$timeout(function () {

            //$scope.$apply(function () {
            updateTables();
            if ($(".pinned").length > 1)
                $(".pinned")[1].remove();
            //});

            //}, 200);
            //        $timeout(function () {
            //            $scope.finishtable = false;
            //        }, 400);
        }
        $scope.prevIndex = -1;
        $scope.matrixExpandHeader = function (index) {


            $timeout(function () {
                if ($scope.prevIndex && $scope.prevIndex !== index) {
                    $("#ObjHeader" + $scope.prevIndex).animate({ width: 80 });
                    //console.log($scope.prevIndex);
                }

                var parent = $("#ObjHeader" + index);
                var element = $("#ObjHeader" + index + " span");
                //console.log(element.width() + " " + parent.width());
                if (element.width() > parent.width()) {
                    parent.animate({ width: element.width() + 40 });
                } else {
                    parent.animate({ width: 80 });
                }

                $scope.prevIndex = index;
            }, 0);


        }



        $scope.reEvaluate = function (parentnodeID, current_step) {
            if ($scope.checked_boxes == 2) {
                var addstep = false;
                if (getStepNumber($scope.node_names[0], $scope.node_names[1]) < 0)
                    addstep = true;
                if ($scope.$parent.isMobile())
                    $(".tt-mobile-footer").removeClass("hide");
                
                if ($scope.check_limit == $scope.checked_boxes) {
                    $http.post(
                        $scope.baseurl + "pages/Anytime/Anytime.aspx/redoPairs",
                        JSON.stringify({
                            parentnodeID: parentnodeID,
                            current_step: current_step,
                            firstNode: $scope.node_names[0],
                            secondNode: $scope.node_names[1],
                            is_name: false,
                            add_step: addstep
                        })
                        )
                    .then(function success(response) {
                        var step = response.data.d;
                        $scope.$parent.is_under_review = true;
                        $scope.matrixReview = false;
                        if (addstep) {
                            $scope.load_step_list();
                            $scope.previous_cluster_step = current_step + 1;
                            $scope.get_pipe_data(current_step, $scope.is_AT_owner);
                            return true;
                        }
                        
                        if (step <= 0) {
                            $scope.active_multi_index = Math.abs(response.data.d);
                            step = JSON.stringify(parseInt($scope.output.PipeParameters.StepPairsData[0][0]) + 1);
                            
                            //console.log("STEP: "+step);
                            //if (($scope.previous_cluster_step) && $scope.previous_cluster_step == current_step) {
                            //    step -= 1;
                            //    $scope.get_pipe_data(step);
                            //    return true;
                            //}
                            //console.log("reEvaluate -> active_multi_index: " + $scope.active_multi_index);
                        }
                        $scope.get_pipe_data(step);
                        $scope.previous_cluster_step = current_step; //case 11539 - fix multi pw
                    });
                }
                else {
                    $(".results-error").fadeIn(200);

                    $timeout(function () {
                        $(".results-error").fadeOut(1000);
                    }, 5000);
                }
            }
        }
        $scope.$on("move_to_cluster", function () {
            if ($scope.$parent.isMobile() && $scope.intermediate_screen != 3)
                $(".tt-mobile-footer").addClass("hide");
            //$scope.active_multi_index = 0;
            $scope.$parent.is_under_review = true;
            //$scope.matrixReview = true;
            if (!$scope.is_multi && $scope.matrixReview)
                $scope.save_pairwise_on_next(true);
            else {
                if ($scope.previous_cluster_step)
                    $scope.get_pipe_data($scope.previous_cluster_step);
            }
        });
        $scope.$on("MoveStepat", function () {
            $scope.active_multi_index = 0;
            $scope.matrixReview = false;
            //$scope.$parent.is_under_review = false;
        });
        //$scope.move_to_cluster = function () {
        //    $scope.$parent.show_check_box = false;
        //    $scope.$parent.is_under_review = false;
        //    $scope.get_pipe_data($scope.previous_cluster_step);
        //}
        $scope.$parent.show_check_box = false;
        $scope.satisfactory = true;
        $scope.change_satisfactory = function (bool) {
            $(".pipe-wrap").effect("drop", {}, 200, function () {
                setTimeout(
                    function () {
                        //$(".pipe-wrap").addClass("hide");
                        $(".results-inconsistency-tree").removeClass("hide");
                        $(".results-main").addClass("hide");
                        $("#normalization").addClass("hide");
                        $(".priority-btn").addClass("hide");
                        
                    }, 100);
                setTimeout(
                    function () {
                        $(".pipe-wrap").fadeIn();
                    }, 100);

                //$scope.reflow_equalizer();

            });

        }

        //updates the tooltip in judgment table
        $scope.show_redo_pairs = function () {
            $scope.checked_boxes = 0;
            $scope.node_names = [];
            $scope.intermediate_screen = 3;
            $(".pipe-wrap").effect("drop", {}, 200, function () {
                $(".copy-paste").addClass("hide");
                $(".results-main").removeClass("hide");
                $(".priority-btn").removeClass("hide");
                $(".priorities-cbr").removeClass("hide");
                $(".results-inconsistency-tree").addClass("hide");
                $(".results-redo-pairs").removeClass("hide");
                $(".results-main-extra").addClass("hide");
                $("#normalization").removeClass("hide");
                $(".results-main-header-mobile").removeClass("hide");
                $("#results-title").html("Select a pair of element: 0/2");
                $(".results-main table input[type=checkbox]").addClass("cb-glow");
                $(".matrix-options").addClass("hide");
                $(".re-evaluate-options").removeClass("hide");
                $scope.$apply(function () {
                  //  $("#QHIcons").addClass("hide"); //need to comment this out - Ena case 12191
                    if ($scope.$parent.isMobile())
                        $(".tt-mobile-footer").addClass("hide");
                    $(".txt-area-redo").height($(".txt-area-redo").scrollHeight);
                });
                //setTimeout("",3000);
                setTimeout(
                    function () {
                        $(".pipe-wrap").fadeIn();
                    }
                , 100);
            });



        }
        $scope.showResult = function () {
            $scope.output.PipeParameters.equalMessage = false;
            if (["atPairwise", "atAllPairwise"].indexOf($scope.output.PipeParameters.JudgmentType) > -1)
                getMatrixTemplate();
        }
        $scope.reviewJudgment = function (parentnodeID, current_step) {
            $http.post(
                $scope.baseurl + "pages/Anytime/Anytime.aspx/reviewJudgment",
                JSON.stringify({
                    parentnodeID: parentnodeID,
                    current_step: current_step
                })
                )
            .then(function success(response) {
                var step = response.data.d;
                $scope.get_pipe_data(step, null, false);
            });
        }
        function glowMatrix(removeGlow, idElem) {
            if (removeGlow) {
                $(".matrix-cell").removeClass("box-glow");
            }
            else if (idElem) {
                $(".matrix-cell").removeClass("box-glow");
                $(idElem).addClass("box-glow");
            }
            else {
                $.each(matrixCellIDs, function (i, value) {
                    $(value).addClass("box-glow");
                });
            }
        }
        $scope.restoreDefaultView = function () {
            $scope.checked_boxes = 0;
            $scope.intermediate_screen = 0;
            $scope.results_data = $scope.output.PipeParameters.results_data;
            angular.forEach($scope.checkbox_items, function (result, key) {
                $scope.checkbox_items[key] = false;
            });

            $(".pipe-wrap").effect("drop", {}, 200, function () {
                matrixCellIDs = [];
                $(".results-main").removeClass("hide");
                $(".results-inconsistency-tree").addClass("hide");
                $(".results-redo-pairs").addClass("hide");
                $(".results-main-extra").removeClass("hide");
                $(".results-main-header-mobile").addClass("hide");
                $(".tt-inconsistency-wrap").addClass("hide");
                $(".priority-btn").removeClass("hide");
                $(".priorities-cbr").removeClass("hide");
                $("#normalization").removeClass("hide");
                $("#results-instruction").removeClass("hide");
                fJudgmentTableExist = false; //stop resize event of judgment table
                glowMatrix(true);
                $scope.$apply(function () {
                    $scope.$parent.show_check_box = false;
                    $("#QHIcons").removeClass("hide");
                    if ($scope.$parent.isMobile())
                        $(".tt-mobile-footer").removeClass("hide");
                    if ($scope.changedMatrixValues.length > 0) {
                        //$scope.reOrderMatrixTable();
                        $scope.changedMatrixValues = [];
                    }
                });
                setTimeout(
                    function () {
                        $(".pipe-wrap").fadeIn();
                    }, 100);
            });

            //if ($("#matrixtable").length > 0)
            //    $("#matrixtable").remove();

            //Sswitched = false;
            //if ($(".table-wrapper").length > 0)
            //$(".table-wrapper").remove();

            //if ($("#matrixtable").length > 0)
            //    $("#matrixtable").remove();

            //Sswitched = false;
            //if ($(".table-wrapper").length > 0)
            //    $(".table-wrapper").remove();


        }

        $scope.inconsistency_view = false;
        $scope.show_inconsistency_view = function () {
            
            $timeout(function () {
                $scope.reflow_equalizer();
                $scope.CheckMultiHeight("jj-wrap-fix", "50"); //fixes height of inner div
                //console.log("booomike");
            }, 500);
            
            //$scope.matrixReview = false;
            $scope.intermediate_screen = 2;
            $(".pipe-wrap").effect("drop", {}, 200, function () {
                //$scope.$apply(function () {
                //    $scope.inconsistency_view = true;
                //    $scope.reflow_equalizer();
                //});
                $(".results-inconsistency-tree").addClass("hide");
                $(".results-redo-pairs").addClass("hide");
                $(".results-main-extra").addClass("hide");
                $(".priorities-cbr").addClass("hide");
                $(".results-main").removeClass("hide");
                $(".tt-inconsistency-wrap").removeClass("hide");
                $(".results-main-header-mobile").removeClass("hide");
                $("#QHIcons").addClass("hide");
                $(".matrix-options").removeClass("hide");
                $(".re-evaluate-options").addClass("hide");
                $(".copy-paste").removeClass("hide");
                $("#results-instruction").addClass("hide");
                fJudgmentTableExist = true;
                setTimeout(
                    function () {
                        $(".pipe-wrap").fadeIn();
                        $("#results-title").html("Judgment Table");

                        $(".tt-mobile-footer").addClass("hide");

                        $scope.executeResponse();
                        $(document).foundation("equalizer", "reflow");
                    }, 100);

            });


        }

        $scope.hidematrixmodal = function () {
            angular.element("#myModal").foundation("reveal", "close");
        }

        $scope.setMatrixVsTxt = function (result, result2) {
            $("#jt-txt-1").text(result);
            $("#jt-txt-2").text(result2);
        }

        $scope.judgmentTableValues = {
            pairsData : [],
            objsData : []
        }
        $scope.copyJudgmentTable = function () {
                $scope.judgmentTableValues.pairsData = eval($scope.output.PipeParameters.StepPairsData);
                $scope.judgmentTableValues.objsData = eval($scope.output.PipeParameters.ObjectivesData);
        }
        $scope.open_pasteModal = function () {
            $scope.pasteMessage = "Paste the clipboard below and click Go";
            $scope.ClipBoardData = "";
            $("ClipBoardData").val("");
            angular.element("#pasteModal").foundation("reveal", "open");
        }

        $scope.setFocusonText = function () {
            setTimeout(function () {
                $("#ClipBoardData").focus();
            }, 500);
            
        }
        $scope.pasteJudgmentTable = function () {
            var clipBoardData = $("#ClipBoardData").val();
            //var clipBoardData = $scope.ClipBoardData;
            var thisIsBad = false;
            if ($scope.judgmentTableValues.objsData.length < $scope.output.PipeParameters.ObjectivesData.length) {
                thisIsBad = true;
            }

            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/pasteClipBoardData",
                data: {
                    clipBoardData: clipBoardData,
                    sameElements: thisIsBad
                }
            })
                .then(function success(response) {
                    if (response.data.d.success) {
                        var results_object = response.data.d.data;
                        results_object.StepPairsData = eval(results_object.StepPairsData);
                        results_object.ObjectivesData = eval(results_object.ObjectivesData);
                        $scope.output.PipeParameters = results_object;
                        $scope.output.PipeParameters.equalMessage = false;
                        $scope.pasteMessage = "Pasted all data successfully";
                        //$("#paste-message").text("Pasted all data successfully!");
                        $(".paste-message-icon").removeClass("icon-tt-close");
                        $(".paste-message-icon").addClass("icon-tt-check");
                        $(".paste-message-a").addClass("success").removeClass("error");
                        
                        getMatrixTemplate();
                    }
                    else {
                        alert(response.data.d.data);
                    }

                }, function error(response) {
                    $("#paste-message").text("Failed! The size of data copied does not match the current table");
                    $(".paste-message-icon").addClass("icon-tt-close");
                    $(".paste-message-icon").removeClass("icon-tt-check");
                    $(".paste-message-a").removeClass("success").addClass("error");
                    //console.log(response);
                });
            

            //if ($scope.judgmentTableValues.objsData.length == $scope.output.PipeParameters.ObjectivesData.length)
            //{
            //    $scope.output.PipeParameters.StepPairsData = $scope.judgmentTableValues.pairsdata;
            //    $http({
            //        method: "POST",
            //        url: baseUrl + "pages/Anytime/Anytime.aspx/updateAllinMatrix",
            //        async: false,
            //        data: {
            //            pairsData: $scope.output.PipeParameters.StepPairsData,
            //            parent: $scope.output.parentnodeID
            //        }
            //    })
            //        .then(function success(response) {
            //            //console.log(response.data);
            //            $("#paste-message").text("Pasted all data successfully!");
            //            $(".paste-message-icon").removeClass("icon-tt-close");
            //            $(".paste-message-icon").addClass("icon-tt-check");
            //            $(".paste-message-a").addClass("success").removeClass("error");
            //        }, function error(response) {
            //            $("#paste-message").text("Failed! The size of data copied does not match the current table");
            //            $(".paste-message-icon").addClass("icon-tt-close");
            //            $(".paste-message-icon").removeClass("icon-tt-check");
            //            $(".paste-message-a").removeClass("success").addClass("error");
            //            //console.log(response);
            //        });
            //    getMatrixTemplate();
            //}
            //else {
            //    $("#paste-message").text("Failed! The size of data copied does not match the current table");
            //    $(".paste-message-icon").addClass("icon-tt-close");
            //    $(".paste-message-icon").removeClass("icon-tt-check");
            //    $(".paste-message-a").removeClass("success").addClass("error");
            //}

        }

        $scope.get_matrix_bar = function (width) {
            return { 'width': ((width / $scope.output.PipeParameters.matrix_highest_result) * 100).toFixed(2) + "%" };

        }
        $scope.set_reverse = function (global) {
            if (global)
                $scope.reverseGlobal = !$scope.reverseGlobal;
            else
                $scope.reverseLocal = !$scope.reverseLocal;
        }
        $scope.columns = [["nodeID", "Index"], ["nodeName", "Name"], ["yourResults", "Your Results"], ["combine", "Combined"]];

        $scope.check_shown_results = function (individual, combined) {
            var data = $scope.results_data[0];
            var fpass = true;

            if (data[4] == "rvIndividual" && !individual) {
                fpass = false;
            } else if (data[4] == "rvGroup" && !combined) {
                fpass = false;
            } else if (data[4] == "rvBoth" && (!combined && !individual)) {
                fpass = false;
            }

            return fpass;
        }
        //End of Local Results

        //resizing

        //this is to detect resize in angular
        $scope.check_offset_for_footer = function () {
            if (angular.element(".mobile-paginator").offsetHeight > 30) {
                $scope.execute_onresize();
            }
        }


        $scope.screen_sizes = [{ "option": 0 }];
        $scope.screens = 0;
        $scope.execute_onresize = function () {
            //graphical new slider 
            if ($(".graph-green-div").length > 0) {
                if ($scope.$parent.is_multi) {

                }
                else {
                    //if ($(".graph-green-div").offset().top < $("#graphicalSlider").offset().top - 5) {
                    //   $(".graph-green-div").css("top", $("#graphicalSlider").offset().top);
                    //}
                }

                var nouibase = $(".noUi-base").width();
                $(".graph-green-div").css("left", "50%");
                $(".graph-green-div").width((nouibase / 2) - 1);
                var uibaseWidth = $("#gslider" + $scope.active_multi_index).width();
                $("#gslider" + $scope.active_multi_index + " .graph-green-div").width((uibaseWidth / 2) - 1);
            }


            var screen_height = window.outerHeight;
            var screen_width = document.documentElement.clientWidth;
            $scope.screens = screen_width;
            if (screen_width < 410) {
                $scope.screen_sizes.option = 1;
            }
            else if (screen_width < 500) {
                $scope.screen_sizes.option = 2;
            }//below is for desktop
            else if (screen_width < 600 || (screen_width > 1024 && screen_width < 1350)) {
                $scope.screen_sizes.option = 3;
            }
            else if (screen_width < 700 || (screen_width > 1024 && screen_width < 1600)) {

                $scope.screen_sizes.option = 4;
            }
            else if (screen_width < 800 || (screen_width > 1024 && screen_width < 1900)) {
                $scope.screen_sizes.option = 5;
            }
            else {
                $scope.screen_sizes.option = 6;
            }

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

            if ($scope.output != null) {
                if ($scope.output.non_pw_type == "mtStep") {
                    $timeout(function () {
                        $scope.canvas_width = $scope.stepChart.chartArea.right - $scope.stepChart.chartArea.left - 7; //3 px for allowance to the right
                        $scope.left_margin = $scope.stepChart.chartArea.left;
                        $scope.canvas_height = $scope.stepChart.chart.height;
                        $("#stepsFunctionSlider").width($scope.canvas_width - 3).css({ left: $scope.left_margin });

                        if (screen_width < 1024) {
                            $(".tt-steps-slider .ui-slider-handle").width(12);
                        } else {
                            $(".tt-steps-slider .ui-slider-handle").width(6);
                        }

                    }, 200);
                }
            }
        }

        //resizing
        $scope.mobile = [{ 'is_too_small': false }];

        //this is to detect resize in angular
        angular.element($window).bind("resize", "load", function () {
            $timeout(function () {
                $scope.execute_onresize();
            }, 300);

            //inner div resize
            //resize content heights on window resize
            $timeout(function () {
                $scope.CheckMultiHeight("tt-homepage-main-wrap", "40");
                $scope.CheckMultiHeight("multi-loop-wrap", "80");
                $scope.CheckMultiHeight("many-alternatives", "210");
                $scope.CheckMultiHeight("jj-wrap-fix", "50");
                $scope.CheckMultiHeight("multi-loop-wrap-local-results", "500");
                $scope.CheckMultiHeight("tt-pipe-canvas", "154");
                $scope.CheckMultiHeight("pipe-wrap", "100");
                $scope.CheckMultiHeight("overflow-fix", "150");
                //$scope.CheckMultiHeight("tt-rating-scale", "100");
            }, 800);
            
        });
        $scope.execute_onresize();
        $timeout(function () {
            $scope.execute_onresize();
        }, 1000);


        $scope.is_mobile = false;
        $scope.get_step_display = function (step, current_step, total_step, request, is_mobile) {
            var val1 = 7;
            var val2 = 5;
            var val3 = 6;
            var close_range = 7;
            if ($scope.screen_sizes.option == 6) {
                val1 = 9; val2 = 7; val3 = 8; close_range = 9;
            }
            if ($scope.screen_sizes.option == 5) {
                val1 = 7; val2 = 5; val3 = 6; close_range = 7;
            }
            if ($scope.screen_sizes.option == 4) {
                val1 = 5; val2 = 3; val3 = 4; close_range = 5;
            }
            if ($scope.screen_sizes.option == 3) {
                val1 = 4; val2 = 2; val3 = 3; close_range = 4;
            }
            if ($scope.screen_sizes.option == 2) {
                val1 = 3; val2 = 1; val3 = 2; close_range = 3;
            }
            if ($scope.screen_sizes.option == 1) {
                val1 = 2; val2 = 0; val3 = 1; close_range = 2;
                //alert($scope.mobile.is_too_small);
            }


            var LeftStep = val1 - (current_step - 1);
            var RightStep = val1 - (total_step - current_step);
            if (step < (current_step - val2) - Math.max(0, RightStep) && step != 1 && (current_step - close_range) != 1) {
                if (step == (current_step - val3) - Math.max(0, RightStep) && request == "unavailable") {
                    return true;
                }
                return false;
            }
            else if (step > (current_step + val2) + Math.max(0, LeftStep) && step != total_step && (current_step + close_range) != total_step) {
                if (step == total_step - 1 && request == "unavailable") {
                    return true;
                }
                return false;
            }
            else {
                if (request == "available" && current_step != step) {
                    return true;
                }
            }
        }



        $scope.MoveToPipe = function (pipe, event) {
            if (event != null) {
                var code = event.keyCode;
                if (code == 13) {
                    if (pipe > 0 && pipe <= $scope.output.total_pipe_steps)
                        $scope.get_pipe_data(pipe, null, false);
                    return event.preventDefault();
                }
            }
            else {
                if (pipe > 0 && pipe <= $scope.output.total_pipe_steps)
                    $scope.get_pipe_data(pipe, null, false);
            }


        }

        //End of Footers

        //initialize array for ng-repeat
        $scope.range = function (n) {
            return new Array(n);
        };
        //end of range

        $scope.SASignal = true;
        $scope.SAFocusin = function () {
            $scope.SASignal = false;
            $scope.SAcheck = true;
        };
        $scope.SAFocusout = function () {
            if ($scope.SAcheck != true) {
                $scope.SASignal = true;

            }
            $scope.SAcheck = false;

        };
        $scope.SAcheck = false;
        //$scope.$emit("ispipescreen", $scope.is_anytime);

        $scope.onChangeSubobjective = function onChangeSubobjective(id) {
            $("#DSACanvas").sa("option", "SelectedObjectiveIndex", Number(id));
            $("#DSACanvas").sa("redrawSA");
        }


        //CR - case 11322 
        $scope.redirectToComparionTeamtime = function () {
            $http({
                method: "POST",
                url: baseUrl + "pages/Anytime/Anytime.aspx/redirectToComparionTeamtime",
                data: {}
            }).then(function success(response) {
                window.location.href = response.data.d;
            }, function error(response) {
                //console.log(response);
            });
        }

        $scope.copy_responsive_link = function () {
            alert("Successfully copied to clipboard!");
        }

        //$scope.close_warning_message = function () {
        //    if (confirm("Are you sure you want to close this message?") == true) {
        //        $("#warning-pipe-message").fadeOut();
        //        $http({
        //            method: "POST",
        //            url: baseUrl + "pages/Anytime/Anytime.aspx/HideBrowserWarningMessage",
        //            data: {}
        //        }).then(function success(response) {
        //            $scope.output.hideBrowserWarning = true;
        //        }, function error(response) {
        //            //console.log(response);
        //        });
        //    } else {
               
        //    }
        //}

        //Knowledge Owl Help Starts

        $scope.hideKoHelpPage = function() {
            if ((typeof __ko16 != "undefined")) {
                var koWidgetCssName = "_ko16-widget-wrapper";
                var koOpenCssName = "_ko16-widget-open";
                var koOpenPage = $("." + koWidgetCssName + "." + koOpenCssName);
                if (koOpenPage.length > 0) {
                    koOpenPage.removeClass(koOpenCssName).addClass("_ko16-widget-closed");
                }
            }
        }

        $scope.$on("showKoHelpPage", function (e) {
            $scope.showKoHelpPage();
        });

        $scope.showKoHelpPage = function () {
            if ((typeof __ko16 != "undefined")) {
                var url = $scope.getKoHelpUrl();
                if (typeof url != "undefined" && url != "") {
                    __ko16.updatePageLoc(url);
                    __ko16.showContainer("._ko16_article_list_cntr");
                    __ko16._toggleOpen();
                }
            }
        }

        $scope.getKoHelpUrl = function () {
            var url = "";
            var measureType = "";

            switch ($scope.output.page_type) {
                case "atInformationPage":
                    url = "r-anytime-welcome";
                    break;
                case "atPairwise":
                case "atPairwiseOutcomes":
                    if ($scope.output.pairwise_type == "ptVerbal") {
                        url = "r-anytime-verbalpw";
                    } else if ($scope.output.pairwise_type == "ptGraphical") {
                        url = "r-anytime-graphicalpw";
                    }
                    break;
                case "atAllPairwise":
                case "atAllPairwiseOutcomes":
                    if ($scope.output.pairwise_type == "ptVerbal") {
                        url = "r-anytime-multiverbalpw";
                    } else if ($scope.output.pairwise_type == "ptGraphical") {
                        url = "r-anytime-multigraphicalpw";
                    }
                    break;
                case "atNonPWOneAtATime":
                    measureType = $scope.output.non_pw_type;
                    switch (measureType)
                    {
                        case "mtDirect":
                            url = "r-anytime-direct-input";
                            break;
                        case "mtRatings":
                            url = "r-anytime-ratings";
                            break;
                        case "mtStep":
                            url = "r-anytime-stepfunction";
                            break;
                        case "mtRegularUtilityCurve":
                        case "mtCustomUtilityCurve":
                            url = "r-anytime-utilitycurve";
                            break;
                    }
                    break;
                case "atNonPWAllChildren":
                case "atNonPWAllCovObjs":
                    measureType = $scope.output.non_pw_type;
                    switch (measureType)
                    {
                        case "mtDirect":
                            url = "r-anytime-multidirectinput";
                            break;
                        case "mtRatings":
                            url = "r-anytime-multiratings";
                            break;
                    }
                    break;
                case "atSpyronSurvey":
                case "atSurvey":
                    url = "r-anytime-survey";
                    break;
                case "atSensitivityAnalysis":
                    if ($scope.output.saType == "satDynamic") {
                        url = "r-anytime-dynamicsa";
                    } else if ($scope.output.saType == "satGradient") {
                        url = "r-anytime-gradientsa";
                    } else if ($scope.output.saType == "satPerformance") {
                        url = "r-anytime-performancesa";
                    }
                    break;
                case "atShowLocalResults":
                    url = "r-anytime-intermediateresults";
                    break;
                case "atShowGlobalResults":
                    url = "r-anytime-overallresults";
                    break;
                case "thankyou":
                    url = "r-anytime-thankyou";
                    break;
                default:
                    url = "r-anytime-other";
            }

            return url;
        }

        //Knowledge Owl Help Ends
     

        function checkLoadedScreens(method, measuretype, userControlContent)
        {
            //var loadedScreenCookie = $.cookie("loadedScreens");
            //loadedScreenCookie = loadedScreenCookie.split("&");
            //for (i = 0; i < loadedScreenCookie.length; i++)
            //{
            //    loadedScreenCookie[i] = loadedScreenCookie[i].split("=");
            //}
            //switch (loadedScreenCookie[i][0]) {
            //    case "atPairwise":

            //}
            var content = "";
            switch(method)
            {
                case "atPairwise":
                case "atPairwiseOutcomes":
                    if (userControlContent != false)
                        pipeContents.pairwise = userControlContent;
                    content = pipeContents.pairwise;
                    break;
                case "atAllPairwise":
                case "atAllPairwiseOutcomes":
                    if (userControlContent != false)
                        pipeContents.multiPairwise = userControlContent;
                    content = pipeContents.multiPairwise;
                    break;
                case "atNonPWOneAtATime":
                    {
                        switch (measuretype)
                        {
                            case "mtDirect":
                                if (userControlContent != false)
                                    pipeContents.direct = userControlContent;
                                content = pipeContents.direct;
                                break;
                            case "mtRatings":
                                if (userControlContent != false)
                                    pipeContents.ratings = userControlContent;
                                content = pipeContents.ratings;
                                break;
                            case "mtStep":
                                if (userControlContent != false)
                                    pipeContents.stepfunction = userControlContent;
                                content = pipeContents.stepfunction;
                                break;
                            case "mtRegularUtilityCurve":
                            case "mtCustomUtilityCurve":
                                if (userControlContent != false)
                                    pipeContents.utility = userControlContent;
                                content = pipeContents.utility;
                                break;
                        }
                    }
                    break;
                case "atNonPWAllChildren":
                case "atNonPWAllCovObjs":
                    {
                        switch (measuretype) {
                            case "mtDirect":
                                if (userControlContent != false)
                                    pipeContents.multiDirect = userControlContent;
                                content = pipeContents.multiDirect;
                                break;
                            case "mtRatings":
                                if (userControlContent != false)
                                    pipeContents.multiRatings = userControlContent;
                                content = pipeContents.multiRatings;
                                $scope.active_multi_index = 0;
                                break;
                        }
                    }
                case "atSpyronSurvey":
                case "atSurvey":
                    if (userControlContent != false)
                        pipeContents.survey = userControlContent;
                    content = pipeContents.survey;
                    break;
                case "atSensitivityAnalysis":
                    if (userControlContent != false)
                        pipeContents.sensitivty = userControlContent;
                    content = pipeContents.sensitivty;
                    break;
                case "atShowLocalResults":
                    if (userControlContent != false)
                        pipeContents.localresults = userControlContent;
                    content = pipeContents.localresults;
                    break;
                case "atShowGlobalResults":
                    if (userControlContent != false)
                        pipeContents.globalresults = userControlContent;
                    content = pipeContents.globalresults;
                    break;

            }
            localStorage.setItem("pipeContents", JSON.stringify(pipeContents));

            return content;
        }

        $scope.countPageWatchers = function () {
            console.log("No. of current scope watchers: " + $scope.$$watchersCount);
            console.log("No. of parent scope watchers: " + $scope.$parent.$$watchersCount);
        }
    }]);

    //Filters

    app.filter("intensityDecimal", function ($filter) {
        return function (input) {
            if (isNaN(input)) return input;

            var decimalPart = (input + "").split(".")[1];

            if (decimalPart) {
                var startRound;
                var areDecimalZeros = true;
                var maxRound = decimalPart.length > 5 ? 5 : decimalPart.length;

                for (startRound = 0; startRound < maxRound + 2; ++startRound) {
                    if (decimalPart[startRound] >= "1" && decimalPart[startRound] <= "9") {
                        areDecimalZeros = false;
                        ++startRound;
                        break;
                    }
                }

                if (areDecimalZeros) {
                    startRound = 2;
                } else {
                    startRound = startRound < 2 ? 2 : (startRound > 5 ? 5 : startRound);
                }

                return input.toFixed(startRound);
            } else {
                return input.toFixed(2);
            }
        };
    });

    //Directives

    app.directive("intensityInput", function ($filter, $browser) {
        return {
            require: "ngModel",
            link: function ($scope, $element, $attrs, ngModelCtrl) {
                //var listener = function () {
                //    var value = $element.val().replace(/,/g, "");
                //    $element.val($filter("number")(value, false));
                //}

                // This runs when we update the text field
                //ngModelCtrl.$parsers.push(function(viewValue) {
                //    return viewValue.replace(/,/g, "");
                //});

                // This runs when the model gets updated on the scope directly and keeps our view in sync
                ngModelCtrl.$render = function () {
                    //$element.val($filter("intensityDecimal")(ngModelCtrl.$viewValue, false));

                    var input = ngModelCtrl.$viewValue;
                    if (!isNaN(input)) {
                        var isExponential = false;
                        if (input.indexOf("1e-") >= 0) {
                            isExponential = true;
                            input = 1 + parseFloat(input);
                        } else {
                            input = parseFloat(input);
                        }
                        
                        var decimalPart = (input + "").split(".")[1];

                        if (decimalPart) {
                            var maxRound = decimalPart.length > 7 ? 7 : decimalPart.length;

                            for (var startRound = 2; startRound < maxRound; ++startRound) {
                                if (decimalPart[startRound] >= "1" && decimalPart[startRound] <= "9") {
                                    ++startRound;
                                    break;
                                }
                            }

                            startRound = startRound < 4 ? 4 : startRound;
                            if (isExponential) {
                                input = (input - 1).toFixed(startRound);
                            } else {
                                input = input.toFixed(startRound);
                            }
                        } else {
                            input = input.toFixed(2);
                        }
                    }

                    $element.val(input);
                }

                //$element.bind("change", listener);
                //$element.bind("keydown", function (event) {
                //    var key = event.keyCode;
                //    // If the keys include the CTRL, SHIFT, ALT, or META keys, or the arrow keys, do nothing.
                //    // This lets us support copy and paste too
                //    if (key == 91 || (15 < key && key < 19) || (37 <= key && key <= 40))
                //        return;
                //    $browser.defer(listener); // Have to do this or changes don't get picked up properly
                //});

                //$element.bind("paste cut", function () {
                //    $browser.defer(listener);
                //});
            }
        }
    });

    app.directive("anytimeGraphslider", function () {
        return {
            scope: {
                ngModel: "="
            },
            link: function (scope, elem, attrs) {
                return $(elem).slider({
                    animate: true,
                    orientation: "horizontal",
                    range: "min",
                    min: 0,
                    max: 1600,
                    value: scope.ngModel,
                    step: 1,
                    slide: function (event, ui) {
                        if (attrs.permission != 0) {
                            return false;
                        }
                        
                        //code from app.js+
                        var result = calculate_pie_and_slider(attrs.color, ui.value);
                        //updateFrontEnd($b, $a); // then update the user's view
                        return scope.$apply(function () {
                            if (attrs.color == "blue") {
                                scope.$parent.graphical_slider[1] = result[1];
                                $(".b_Slider").slider("value", scope.$parent.graphical_slider[1]);
                                if (ui.value <= 800) {
                                    if (result[0][1] > 9) { set_sliders(1, 9, ".b_Slider", ".a_Slider"); return false; }
                                    scope.$parent.main_bar_txt[0] = result[0][0];
                                    scope.$parent.main_bar_txt[1] = result[0][1];
                                }
                                else {
                                    if (result[0][0] > 9) { set_sliders(9, 1, ".a_Slider", ".b_Slider"); return false; }
                                    scope.$parent.main_bar_txt[0] = result[0][0];
                                    scope.$parent.main_bar_txt[1] = parseFloat(result[0][1]);
                                }
                            }
                            else if (attrs.color == "green") {
                                scope.$parent.graphical_slider[0] = result[1];
                                $(".a_Slider").slider("value", scope.$parent.graphical_slider[0]);

                                if (ui.value <= 800) {
                                    if (result[0][1] > 9) { set_sliders(1, 9, ".a_Slider", ".b_Slider"); return false; }
                                    scope.$parent.main_bar_txt[0] = result[0][1];
                                    scope.$parent.main_bar_txt[1] = result[0][0];
                                }
                                else {
                                    if (result[0][0] > 9) { set_sliders(9, 1, ".b_Slider", ".a_Slider"); return false; }
                                    scope.$parent.main_bar_txt[0] = result[0][1];
                                    scope.$parent.main_bar_txt[1] = result[0][0];
                                }
                            }
                            if ($(".ui-slider-range").hasClass("disabled")) {
                                $(".ui-slider-range").removeClass("disabled");
                            }
                            scope.$parent.main_bar[0] = parseFloat(scope.$parent.main_bar_txt[0]);
                            scope.$parent.main_bar[1] = parseFloat(scope.$parent.main_bar_txt[1]);
                            scope.ngModel = ui.value;
                        });

                    },
                    change: function (event, ui) {
                        if (attrs.permission != 0) {
                            return false;
                        }
                        //$(".a_Slider .ui-slider-range").css("background", "#0058a3");
                        //$(".b_Slider .ui-slider-range").css("background", "#6aa84f");
                    },
                    stop: function (event, ui) {
                        if (attrs.permission != 0) {
                            return false;
                        }
                        var result = calculate_pie_and_slider(attrs.color, ui.value);
                        return scope.$apply(function () {
                            var value;
                            var advantage;

                            if (attrs.color == "blue") {
                                if (parseFloat(result[0][0]) > parseFloat(result[0][1])) {
                                    value = parseFloat(result[0][0]);
                                    advantage = 1;
                                }
                                else if (parseFloat(result[0][1]) > parseFloat(result[0][0])) {
                                    value = parseFloat(result[0][1]);
                                    advantage = -1;
                                }
                                else if (parseFloat(result[0][1]) == parseFloat(result[0][0])) {
                                    value = parseFloat(result[0][1]);
                                    advantage = 0;
                                }
                            }
                            if (attrs.color == "green") {
                                if (parseFloat(result[0][0]) > parseFloat(result[0][1])) {
                                    value = parseFloat(result[0][0]);
                                    advantage = -1;
                                }
                                else if (parseFloat(result[0][1]) > parseFloat(result[0][0])) {
                                    value = parseFloat(result[0][1]);
                                    advantage = 1;
                                }
                                else if (parseFloat(result[0][1]) == parseFloat(result[0][0])) {
                                    value = parseFloat(result[0][1]);
                                    advantage = 0;
                                }
                            }
                            scope.$parent.save_pairwise(value, advantage);
                        });

                    }
                });
                function set_sliders(val1, val2, index1, index2) {
                    if (attrs.permission != 0) {
                        return false;
                    }
                    $(index1).slider("value", 1440);
                    $(index2).slider("value", 160);
                    scope.$parent.main_bar_txt[0] = val1;
                    scope.$parent.main_bar_txt[1] = val2;
                    scope.$parent.main_bar[0] = parseFloat(scope.$parent.main_bar_txt[0]);
                    scope.$parent.main_bar[1] = parseFloat(scope.$parent.main_bar_txt[1]);
                }
            }
        };
    });


    app.directive("numericalSlider", function () {
        return {
            scope: {
                ngModel: "="
            },
            link: function (scope, elem, attrs) {
                return $(elem).slider({

                    orientation: "horizontal",
                    range: "min",
                    min: 0,
                    max: 1600,
                    values: handlers,

                    step: 1,
                    slide: function (event, ui) {
                        return scope.$apply(function () {
                            clearTimeout(numerical_timeout);
                            //code for clicking on near the middle
                            if (ui.values[1] != 800) {
                                if (ui.values[1] < 800) {
                                    $(".numerical-slider").slider("option", "values", [ui.values[1], 800, ui.values[1] + 800]);
                                    var updated_value = $(".numerical-slider").slider("option", "values");
                                    if (updated_value[0] < 80) {
                                        $(".numerical-slider").slider("option", "values", [80, 800, 880]);
                                        updateColors($(".numerical-slider").slider("option", "values"));
                                        return false;
                                    }
                                }
                                if (ui.values[1] > 800) {
                                    $(".numerical-slider").slider("option", "values", [ui.values[1] - 800, 800, ui.values[1]]);
                                    var updated_value = $(".numerical-slider").slider("option", "values");
                                    if (updated_value[2] > 1520) {
                                        $(".numerical-slider").slider("option", "values", [720, 800, 1520]);
                                        updateColors($(".numerical-slider").slider("option", "values"));
                                        return false;
                                    }
                                }
                                updateColors($(".numerical-slider").slider("option", "values"));
                                return false;
                            }

                            //////if (ui.values[0] > 400) {

                            //////    scope.$parent.main_bar[0] = 1;
                            //////    scope.$parent.main_bar[1] = parseFloat(((ui.values[2] - 800) / (800 - ui.values[0])).toFixed(2));
                            //////    $(".numerical-slider").slider("option", "values", [ui.values[0], 800, ui.values[0] + 800]);
                            //////}
                            //////else if (ui.values[0] <= 400) {
                            //////    //alert(ui.values);
                            //////    scope.$parent.main_bar[0] = parseFloat(((800 - ui.values[0]) / (ui.values[0])).toFixed(2));
                            //////    scope.$parent.main_bar[1] = 1;
                            //////    $(".numerical-slider").slider("option", "values", [ui.values[0], 800, ui.values[0] + 800]);

                            //////}

                            var sliderValue = [ui.values[0], ui.values[2]];
                            sliderValue = scope.getandCalculateSlider(sliderValue, false);
                            scope.$parent.main_bar[0] = sliderValue[0];
                            scope.$parent.main_bar[1] = sliderValue[1];
                            $(".numerical-slider").slider("option", "values", [ui.values[0], 800, ui.values[0] + 800]);

                            //updateColors($(".numerical-slider").slider("option", "values"));
                            if (ui.values[2] < 880 || scope.$parent.main_bar[0] > 9 || scope.$parent.main_bar[0] < 1) {

                                $(".numerical-slider").slider("option", "values", [80, 800, 880]);
                                scope.$parent.main_bar[0] = 9;
                                //console.log(1);
                                updateColors($(".numerical-slider").slider("option", "values"));
                                return false;
                            }
                            else if (ui.values[0] > 720 || scope.$parent.main_bar[1] > 9 || scope.$parent.main_bar[0] < 1) {
                                $(".numerical-slider").slider("option", "values", [720, 800, 1520]);
                                scope.$parent.main_bar[1] = 9;
                                updateColors($(".numerical-slider").slider("option", "values"));
                                return false;
                            }
                            else {
                                //console.log(2);
                                $leftVal.val((ui.values[0] / 100) + 1);
                                $rightVal.val((ui.values[2] / 100) + 1);
                                if (handlers[0] != ui.values[0]) {
                                    if (ui.values[2] != ui.values[0] + 800) {
                                        $(".numerical-slider").slider("option", "values", [ui.values[0], 800, ui.values[0] + 800]);
                                        if (ui.values[0] > 800) {
                                            $(".numerical-slider").slider("option", "values", [800, 800, ui.values[0] + 800]);
                                            handlers = $(".numerical-slider").slider("option", "values");
                                            updateColors(handlers);
                                            return false;
                                        }
                                        handlers = $(".numerical-slider").slider("option", "values");
                                    }
                                }
                                else if (handlers[2] != ui.values[2]) {
                                    if (ui.values[0] != ui.values[2] - 800) {
                                        $(".numerical-slider").slider("option", "values", [ui.values[2] - 800, 800, ui.values[2]]);
                                        if (ui.values[2] < 800) {
                                            $(".numerical-slider").slider("option", "values", [ui.values[2] - 800, 800, 800]);
                                            updateColors(handlers);
                                            return false;
                                        }
                                        handlers = $(".numerical-slider").slider("option", "values");
                                    }
                                }


                                updateColors(handlers);
                            }

                        });
                        //refreshSwatch
                    },
                    stop: function (event, ui) {
                        //alert($scope.numericalSlider[0]);
                        return scope.$apply(function () {
                            numerical_timeout = setTimeout(function () {
                                var value;
                                var advantage;

                                if (scope.$parent.main_bar[0] > scope.$parent.main_bar[1]) {
                                    value = scope.$parent.main_bar[0];
                                    advantage = 1;
                                }
                                if (scope.$parent.main_bar[0] < scope.$parent.main_bar[1]) {
                                    value = scope.$parent.main_bar[1];
                                    advantage = -1;
                                }
                                scope.$parent.save_pairwise(value, advantage);
                            }, 500);

                        });
                    }
                });
            }
        };
    });

    //app.directive("multiSlider", function () {
    //    return {
    //        scope: {
    //            ngModel: "="
    //        },
    //        link: function (scope, elem, attrs) {
    //            //updateColors(handlers, attrs.index);
    //            //var $leftVal = $('.content-lefts input[type=text]');
    //            //var $rightVal = $('.content-right input[type=text]');
    //            updateTextPosition(attrs.index);
    //            return $(elem).slider({
    //                animate: true,
    //                orientation: "horizontal",
    //                range: "min",
    //                min: 0,
    //                max: 1600,
    //                value: scope.ngModel,
    //                step: 1,
    //                create: function (event, ui) {
    //                    //if (parseInt(attrs.judgment) < scope.$parent.lowest_value) {
    //                    //    $(".g-slider" + attrs.index + " .graphicalSlider .ui-slider-range").css("background", "gray");
    //                    //    $(".g-slider" + attrs.index + " .graphicalSlider ").css("background", "gray");
    //                    //}
    //                },

    //                slide: function (event, ui) {
    //                    if (attrs.permission != 0) {
    //                        return false;
    //                    }
    //                    var result = calculate_pie_and_slider("blue", ui.value, true);
    //                    if (scope.ngModel != undefined) {
    //                        return scope.$apply(function () {
    //                            //updateTextPosition(attrs.index);
    //                            scope.$parent.color_style(1, attrs.index);
    //                            $(".g-slider" + attrs.index + " .graphicalSlider .ui-slider-range").css("background", "#0058a3");
    //                            $(".g-slider" + attrs.index + " .graphicalSlider ").css("background", "#6aa84f");


    //                            //var ss = parseInt($(".g-slider" + attrs.index + " .graphicalSlider .ui-slider-handle").css("left"), 10);
    //                            //$(".content-participant" + attrs.index + " .left-input-text").css("left", ss - 100 + "px");
    //                            //$(".content-participant" + attrs.index + " .right-input-text").css("left", ss - 10 + "px");

    //                            if (ui.value <= 800) {
    //                                if (result[0][1] > 9) { set_sliders(1, 9, attrs.index, 160); return false; }
    //                                scope.$parent.left_input[attrs.index] = result[0][0];
    //                                scope.$parent.right_input[attrs.index] = result[0][1];
    //                            }
    //                            else {
    //                                if (result[0][0] > 9) { set_sliders(9, 1, attrs.index, 1440); return false; }
    //                                scope.$parent.left_input[attrs.index] = result[0][0];
    //                                scope.$parent.right_input[attrs.index] = result[0][1];
    //                            }
    //                            scope.ngModel = ui.value;
    //                        });
    //                    };
    //                },
    //                change: function (event, ui) {

    //                },
    //                stop: function (event, ui) {
    //                    var result = calculate_pie_and_slider("blue", ui.value, true);
    //                    return scope.$apply(function () {
    //                        var value;
    //                        var advantage;
    //                        if (parseFloat(result[0][0]) > parseFloat(result[0][1])) {
    //                            value = parseFloat(result[0][0]);
    //                            advantage = 1;

    //                        }
    //                        else if (parseFloat(result[0][1]) > parseFloat(result[0][0])) {
    //                            value = parseFloat(result[0][1]);
    //                            advantage = -1;
    //                        }
    //                        else if (parseFloat(result[0][1]) == parseFloat(result[0][0])) {
    //                            value = parseFloat(result[0][1]);
    //                            advantage = 0;
    //                        }
    //                        //scope.$parent.save_multi_pairwise(value, advantage, attrs.index);
    //                        //scope.$parent.bar_color = parseInt(attrs.index)+1;

    //                        scope.$parent.add_multivalues(value, advantage, attrs.index, event);


    //                        //scope.$parent.save_pairwise(value, advantage);

    //                    });
    //                }
    //            });

    //            function set_sliders(val1, val2, index, slider_val) {
    //                $(".g-slider" + index + " .graphicalSlider ").slider("value", slider_val);
    //                scope.$parent.participant_slider[index] = slider_val;
    //                scope.$parent.left_input[attrs.index] = val1;
    //                scope.$parent.right_input[attrs.index] = val2;
    //            }
    //        }
    //    };
    //});

    app.directive("anytimeDirectslider", function () {
        return {
            scope: {
                ngModel: "="
            },
            link: function (scope, elem, attrs) {
                return $(elem).slider({
                    range: "min",
                    animate: true,
                    value: scope.ngModel,
                    min: 0,
                    max: 1,
                    step: 0.01,
                    disabled: attrs.disable == "true",
                    slide: function (event, ui) {
                        if (attrs.permission != 0) {
                            return false;
                        }
                        if (scope.ngModel != undefined) {
                            return scope.$apply(function () {
                                scope.ngModel = ui.value;
                            });
                        }
                    },
                    stop: function (event, ui) {
                        if (attrs.permission != 0) {
                            return false;
                        }
                        return scope.$apply(function () {
                            //console.log(ui.value);
                            setTimeout(function () {
                                scope.$parent.save_direct(ui.value);
                            }, 1000);
                            //scope.$parent.update([ui.value, attrs.user, attrs.comment], "save", "mtDirect", scope.$parent.baseurl, attrs.user_list, attrs.cur_user);
                        });
                    }
                });

            }
        };
    });

    app.directive("anytimeMultidirectslider", function () {
        return {
            scope: {
                ngModel: "="
            },
            link: function (scope, elem, attrs) {

                scope.$watch("ngModel", function (value) {
                    scope.ngModel = value;
                    return $(elem).slider({
                        range:"min",
                        animate: true,
                        value: attrs.value,
                        min: 0,
                        max: 1,
                        step: 0.01,
                        disabled: attrs.disable == "true",
                        slide: function (event, ui) {
                            //scope.$parent.set_multi_index(attrs.index);

                            $(".multi-rows").removeClass("selected");
                            elem.parent().addClass("selected");
                            
                            $(".multi-rows").addClass("fade-fifty");
                            elem.parent().removeClass("fade-fifty");

                            //$(elem[0].parentNode).addClass("selected");

                            if (attrs.permission != 0) {
                                return false;
                            }
                            if (scope.ngModel != undefined) {
                                return scope.$apply(function () {
                                    scope.ngModel = ui.value;
                                    // alert(ui.value);
                                });
                            }
                        },
                        stop: function (event, ui) {
                           //  alert(1);
                            setTimeout(function () {
                                scope.$parent.move_to_next_row(parseInt(attrs.index));
                            }, 1000);


                            if (attrs.permission != 0) {
                                return false;
                            }
                            return scope.$apply(function () {

                                scope.$parent.checkMultiValue(scope.$parent.at_multi_direct_slider, 2);
                                //console.log(ui.value);
                                ///alert(ui.value);
                                //scope.$parent.update([ui.value, attrs.user, attrs.comment], "save", "mtDirect", scope.$parent.baseurl, attrs.user_list, attrs.cur_user);
                            });
                        }
                    });

                });



            }
        };
    });


    app.directive("sensitivityslider", function () {
        return {
            scope: {
                ngModel: "="
            },
            link: function (scope, elem, attrs) {
                return $(elem).slider({
                    orientation: "horizontal",
                    range: "min",
                    min: 0,
                    max: 100,
                    value: scope.ngModel,
                    step: 1,
                    slide: function (event, ui) {
                        if (ySA == 0)
                            ySA = event.clientY;
                        var pos = {
                            x: ui.value,
                            y: ySA
                        };
                        //console.log(ui.value);
                        if (ui.value == 0)
                            pos.x = 0;
                        $("#DSACanvas").sa("onMouseMove", pos);
                    },
                    change: function (event, ui) {
                    },
                    stop: function (event, ui) {
                        ySA = 0;
                    }
                });
            }
        };
    });


    //End of Directives
    //************ Angular JS Codes ****************
};

function updateColors(values, is_null) {
    var colorstops = colors[0] + ", "; // start left with the first color
    if (values[0] == 400 && is_null) {
        for (var i = 0; i < values.length; i++) {
            colorstops += "#AAAAAA" + " " + (values[i] / 1600) * 100 + "%,";
            colorstops += "#AAAAAA" + " " + (values[i] / 1600) * 100 + "%,";
        }
    }
    else {
        for (var i = 0; i < values.length; i++) {
            colorstops += colors[i] + " " + (values[i] / 1600) * 100 + "%,";
            colorstops += colors[i + 1] + " " + (values[i] / 1600) * 100 + "%,";
        }
    }
    // end with the last color to the right
    colorstops += colors[colors.length - 1];

    /* Safari 5.1, Chrome 10+ */
    var css = "-webkit-linear-gradient(left," + colorstops + ")";
    $(".numerical-slider").css("background-image", css);
}
var handlers = [400, 800, 1200];
var colors = ["#AAAAAA", "#0058a3", "#6aa84f", "#AAAAAA"];
updateColors(handlers);
var $curveVal = $('input[name="curvesValue"]');
var $leftVal = $('.content-left input[type=text]');
var $rightVal = $('.content-right input[type=text]');

var numerical_timeout;

var ySA = 0;

function calculate_pie_and_slider(color, ui, is_false) {
    var $a;
    var $b;
    var a;
    var b;
    var areal;
    var breal;
    var total;
    var sliderval;
    var ratiovalue = [];
    if (color == "blue") {
        $a = ui;
        $b = 1600 - $a;
        a = $a / 16;
        b = $b / 16;
        areal;
        breal;
        if ($a == 800) {
            areal = 1;
            breal = 1;
            ratiovalue[0] = areal;
            ratiovalue[1] = breal;
        }
        if ($a > 800) {
            areal = (1 + (($a - 800) / 100)).toFixed(2);
            breal = 1;
            ratiovalue[0] = (($a / 1600) / (1 - ($a / 1600))).toFixed(2);
            ratiovalue[1] = breal;
        }
        if ($a < 800) {
            areal = 1;
            breal = (1 + ((800 - $a) / 100)).toFixed(2);
            ratiovalue[0] = areal;
            ratiovalue[1] = (($b / 1600) / (1 - ($b / 1600))).toFixed(2);
        }
        total = parseFloat(ratiovalue[1]) + parseFloat(breal);
        //if (!is_false) {
        //    reloadPizza(ratiovalue[1] / total, ratiovalue[0] / total);
        //}
        sliderval = 1600 * (1 - ($a / 1600));
    }
    else if (color == "green") {
        $a = ui;
        $b = 1600 - $a;
        a = $a / 16;
        b = $b / 16;
        areal;
        breal;
        if ($a == 800) {
            areal = 1;
            breal = 1;
            ratiovalue[0] = areal;
            ratiovalue[1] = breal;
        }
        if ($a > 800) {
            areal = (1 + (($a - 800) / 100)).toFixed(2);
            breal = 1;
            ratiovalue[0] = (($a / 1600) / (1 - ($a / 1600))).toFixed(2);
            ratiovalue[1] = breal;

        }
        if ($a < 800) {
            areal = 1;
            breal = (1 + ((800 - $a) / 100)).toFixed(2);
            ratiovalue[0] = areal;
            ratiovalue[1] = (($b / 1600) / (1 - ($b / 1600))).toFixed(2);
        }
        total = parseFloat(ratiovalue[0]) + parseFloat(ratiovalue[1]);
        //if (!is_false) {
        //    reloadPizza(ratiovalue[0] / total, ratiovalue[1] / total);
        //}
        sliderval = 1600 * (1 - ($a / 1600));
    }
    positionBarInputs($a);
    return [ratiovalue, sliderval];
}
