/// <reference path="~/Scripts/angular.js" />
/// <reference path="~/Scripts/angular-route.js" />
/// <reference path="~/Scripts/angular.min.js" />
/// <reference path="~/Scripts/angular-resource.js" />
/// <reference path="~/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Scripts/Controllers/AnytimeController.js" />
/// <reference path="../judgment.js" />
/// <reference path="MainController.js" />

//************ Angular JS Codes ****************

app.controller("TeamTimeController", function ($scope, $interval, $http, $timeout, $window, $sce) {
    var beforeUnloadTimeout = 0;

    $(window).bind('beforeunload', function () {
        //console.log('beforeunload');
        //beforeUnloadTimeout = setTimeout(function () {
        //    console.log('settimeout function'); //user stayed on the page
        //}, 500);

        return 'Are you sure?';
    });

    //if (performance.navigation.type == 1) {
    //    console.info("reload");
    //    $.ajax({
    //        type: "POST",
    //        url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/TeamTimeExecuteEvent",
    //        data: JSON.stringify({
    //            TeamTimeEvent: "resume"
    //        }),
    //        async: false,
    //        contentType: "application/json; charset=utf-8",
    //        success: function (data) {
    //            console.log("success");
    //        },
    //        error: function (response) {
    //            console.log("error");
    //        }
    //    });
    //}
    //$(window).bind('unload', function () {
    //    console.info("unloaded");
    //    $.ajax({
    //        type: "POST",
    //        url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/TeamTimeExecuteEvent",
    //        data: JSON.stringify({
    //            TeamTimeEvent: "pause"
    //        }),
    //        async: false,
    //        contentType: "application/json; charset=utf-8",
    //        success: function (data) {
    //            console.log("success");
    //        },
    //        error: function (response) {
    //            console.log("error");
    //        }
    //    });
    //    if (typeof beforeUnloadTimeout !== 'undefined' && beforeUnloadTimeout != 0)
    //        clearTimeout(beforeUnloadTimeout);
    //});

    $scope.pipe_measurements = []; 
    $scope.page_load = false;
    $scope.is_reload_pie = true;
    $scope.user_status;
    $scope.JSON_complete;
    $scope.hide_judgment;
    $scope.group_result = [];
    $scope.main_bar = [];
    $scope.main_bar_txt = [];
    $scope.current_action_type;
    $scope.current_mtype;
    $scope.user_judgment = 0;
    $scope.ratings_direct_values = [];
    $scope.restricted = false;
    $scope.current_user = [];
    $scope.output = null;
    $scope.hierarchies = [];
    $scope.switch_to_old = false;
    $scope.is_anytime = false;
    $scope.is_teamtime = true;
    $scope.active_multi_index = 0;
    $scope.$parent.is_anytime = false;
    $scope.$parent.is_teamtime = true;
    //constant variables, please do not change
    $scope.lowest_value = -999999999999;
    //constant variables, please do not change

     $scope.refreshed = false;
    //*************** ANGULAR TIMER *******************
     var angular_timer_running = false;

     //$scope.$emit("ispipescreen", $scope.is_teamtime);
     $scope.timer = { 'count': 0, 'hash': '' };

    //update_users_list
     $scope.$on('update_users_list', function (e) {
         $scope.update_users_list();
     });

    $scope.ratings_counter = 0;
    $scope.init_ratings = function() {
        //var actual_window_size = window.innerWidth;
        //try {

        //    if (actual_window_size <= 1024) {
        //        $(".arrow_0").click();
        //    } else {
        //        $(".cdd_0").click();
        //    }
        //    $scope.ratings_counter++;
        //} catch (e) {
        //}
    };

    
    $scope.init_counter = function() {
        return $scope.ratings_counter;
    }

    $(window).on("resize.doResize",
        function () {
            try {
                //if ($scope.current_mtype == 'mtRatings') {
                //    $scope.init_ratings();
                //};
            } catch (e) {
            }
        }
    );

   
 
    $scope.set_infodoc_mode = function () {
        $scope.is_infodoc_tooltip = $scope.$parent.is_infodoc_tooltip;
        //$scope.$parent.orig_infodoc_setting = $scope.output.is_infodoc_tooltip;
        //$scope.render_infodoc_sizes();
        //$http({
        //    method: "POST",
        //    url: baseUrl + "pages/Anytime/Anytime.aspx/setInfodocMode",
        //    data: {
        //        value: $scope.output.is_infodoc_tooltip ? 1 : 0
        //    }
        //}).then(function success(response) {
        //    try {
        //        $scope.output.is_infodoc_tooltip = $scope.output.is_infodoc_tooltip;
        //        is_info_tooltip = $scope.output.is_infodoc_tooltip;

        //    }
        //    catch (e) {

        //    }
        //    ////////////////console.log(response);
        //}, function error(response) {
        //    ////////////////console.log(response);
        //});
    }
    $scope.$on('set_infodoc_mode', function (e) {
        $scope.set_infodoc_mode();
    });
    
    $scope.update_infodocs = function (id, event) {
        //var node = $(".icon", this).attr("data-node");
        //var this_id = $(this).attr('id');
        //var parent = $(this).parent().parent();
        //var sub_visible = $('.tg-accordion-sub-' + this_id, parent).is(':hidden');
        //var thisElement = angular.element(event.currentTarget);
        //if (thisElement.hasClass("tt-toggler")) {
        //    var node = $(".icon", thisElement).attr("data-node");
        //    var this_id = $(thisElement).attr('id');
        //    var togglerClass = $(thisElement).attr("data-toggler") + "-" + this_id;

        //    var parent;
        //    if (togglerClass.indexOf("-sub-") > 0) {
        //        parent = $(thisElement).parent().parent();
        //    } else {
        //        //parent = $(thisElement).parent().parent().parent().parent();
        //        parent = $(thisElement).closest('div.tt-j-result');
        //    }

        //    //var sub_visible = $('.tg-accordion-sub-' + this_id, parent).is(':hidden');
        //    var sub_visible = $("." + togglerClass, parent).is(':hidden');

        //    //if ($scope.output.isPM) {
        //    save_dropdown(node, sub_visible, baseUrl);
        //    if (sub_visible) {
        //        $("." + togglerClass, parent).show();
        //        $(".icon", thisElement).removeClass("icon-tt-plus-square");
        //        $(".icon", thisElement).addClass("icon-tt-minus-square");
        //    } else {
        //        $("." + togglerClass, parent).hide();
        //        $(".icon", thisElement).removeClass("icon-tt-minus-square");
        //        $(".icon", thisElement).addClass("icon-tt-plus-square");
        //    }
            //} else {
            //    $.ajax({
            //        type: "POST",
            //        url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/HideAndShowInfoDocs",
            //        contentType: "application/json; charset=utf-8",
            //        data: JSON.stringify({
            //            index: node,
            //            value: sub_visible
            //        }),
            //        success: function(data) {
            //            //resize_infodoc_node(output.infodoc_sizes);
            //        },
            //        error: function(response) {
            //            console.log(JSON.stringify(response));
            //        }
            //    });
            //}
        //}
    }


    $scope.update_users_list = function () {
        if (angular_timer_running) {
            //console.log("Timer is running");
            return false;
        }
        //$http({
        //    method: "POST",
        //    url: $scope.baseurl + "Pages/TeamTimeTest/teamtime.aspx/refreshPage",
        //    data: {}
        //}).then(function success(response) {
        $scope.$parent.is_anytime = false;
        $scope.$parent.is_teamtime = true;
        $.ajax({
            type: "POST",
            url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/refreshPage",
            contentType: "application/json; charset=utf-8",
            data: "{}",
            timeout: 1000 * 30, // 30 seconds, if server hangs it cuts the timer after 30 sec
            beforeSend: function () {
                angular_timer_running = true;
            },
            complete: function () {
                angular_timer_running = false;
            },
            success: function (response) {
               
                var TTData = JSON.parse(response.d);
               // console.log(TTData);
               // console.log(TTData.hash != $scope.timer.hash);
                if (TTData.hash != $scope.timer.hash) {
                    $scope.timer.count = 0;
                    $scope.timer.hash = TTData.hash;
                }
                else {
                    if ($scope.timer.count > 0) {
                        //when code enters here the code stops executing unless if there are changes in the pipe
                        return false;
                    }
                    $scope.timer.count += 1;
                }
                var output = TTData.output; //JSON.parse(response.data.d) if angular

             

                if (output != null || output != undefined) {
                    //////////console.log("JSON DATA: " + output.sdata);
                    var jsonData = eval("[" + output.sdata + "]");
                    jsonData = jsonData[0];
                    $scope.output = output;
                    $scope.$parent.output = $scope.output;

                    var pipe_data = [];
                    sRes = $scope.output.sRes;
                    cluster_phrase = $scope.output.cluster_phrase;
                    var task;

                    if (jsonData != 0 && (jsonData != null || jsonData != undefined)) {
                        $scope.$parent.TTpause = "pause";
                        $('.disable-when-pause').removeClass("disabledbutton");
                        pipe_data = jsonData.data.data;
                        if (jsonData.options != $scope.TTOptions) {
                            $scope.is_reload_pie = true; // this is to reload the pie chart!
                                
                            $scope.TTOptions = jsonData.options;

                            $scope.TTOptions.isAnonymous = jsonData.options.isAnonymous;
                            $scope.$parent.TTOptions = $scope.TTOptions;
                            $scope.$parent.output.current_step = jsonData.progress.currentStep;
                            $scope.$parent.output.totalPipe = jsonData.progress.totalSteps;
                            $scope.users_eval_progress = jsonData.progress.evaluationProgress;

                            task = jsonData.data.task;
                            var temp_step_task = task;
                            $scope.information_nodes.Task = temp_step_task.replace(/<[\/]{0,1}(p)[^><]*>/ig, "");
                            if ($scope.is_tt_owner == 1) {
                                var question = $scope.information_nodes.Task;
                                if (!question.match("icon-tt-pencil")) {
                                    $scope.information_nodes.Task += '<a data-node-type="0" data-location="0"  data-node="question-node" class="edit-pencil edit-info-doc-btn" title="" href="#"><span class="icon-tt-pencil"></span></a>';
                                }
                            }

                        }

                       
                       // alert($scope.information_nodes.Task);
                        $scope.current_action_type = jsonData.data.type;
                        //alert($scope.single_line_mode);
                        var Temp_List;
                        var priority = pipe_data[2];
                        var variance = pipe_data[3];
                        var users_list = $scope.users_list;
                        if ($scope.current_action_type == "ruc" || $scope.current_action_type == "step" || $scope.current_action_type == "rating") {
                            Temp_List = pipe_data[2];
                            priority = pipe_data[3];
                            variance = pipe_data[4];
                            $scope.stepfunction_scale = pipe_data[1];
                        }
                        else
                        {
                            Temp_List = pipe_data[1];
                        }
                        if ($scope.current_mtype != output.mtype) {
                            
                                $scope.users_list = [];
                            
                        }
                        ////infodocs ignore for now

                        //$scope.information_nodes_infodocs.ParentNode = output.ParentNodeInfo;
                        //$scope.information_nodes_infodocs.LeftNode = output.leftnodeinfo;
                        //$scope.information_nodes_infodocs.RightNode = output.rightnodeinfo;
                        //$scope.information_nodes_infodocs.WrtLeftNode = output.WrtLeftInfo;
                        //$scope.information_nodes_infodocs.WrtRightNode = output.WrtRightInfo;
                        ////end of info docs
                        if ($scope.$parent.main_user_list_check) {
                            $scope.$parent.main_user_list_check = false;
                            $scope.users_list = [];
                        }

                        
                        if ($scope.restricted) {
                            $scope.current_action_type = 'restricted';
                        }
                        $scope.user_display = $scope.user_display_options[$scope.TTOptions.userDisplay];
                        $scope.meeting_owner = output.meetingOwner;
                        $scope.current_mtype = output.mtype;
                      
                        $scope.$parent.main_teamtime_screen = output.mtype;
                        $scope.no_judgment_page = $scope.current_action_type == 'message' || $scope.current_action_type == 'localresults' || $scope.current_action_type == 'globalresults';
                        if ($scope.users_list.length == 0) {
                            $scope.current_user = output.current_user;
                            //$scope.show_paginated_participant = [];
                            $scope.show_participant = Boolean(output.toggle);
                            $scope.pagination_CurrentPage = output.pagination[0];

                            $scope.pagination_NoOfUsers = output.pagination[1];
                            $scope.hierarchies = output.Hierarchies;
                           
                            $scope.online_users = [];

                            if ($scope.current_action_type == "ruc" || $scope.current_action_type == "rating") {
                                $scope.pipe_measurements = pipe_data[1];
                                //for ratings only
                                $scope.pipe_measurements.push([-2147483648000, 'Not Rated', 0]);
                                $scope.pipe_measurements.push([10, 'Direct Value', 0]);
                                //////////console.log(JSON.stringify($scope.pipe_measurements));
                                //for ratings only
                            }

                        }

                        if (JSON.stringify(jsonData) !== $scope.JSON_complete || $scope.users_list.length == 0) {
                            $scope.restricted = false;
                            $scope.JSON_complete = JSON.stringify(jsonData);
                            //this show the teamtime body after load

                            // this moves the current user to the top of the participant list
                            for (i = 0; i < Temp_List.length; i++) {
                                if (Temp_List[i][1].toLowerCase() == $scope.current_user[0].toLowerCase()) {
                                    $scope.current_user[1] = Temp_List[i][0];
                                    var index = Temp_List[i];
                                    Temp_List.splice(i, 1);
                                    Temp_List.splice(0, 0, index);

                                    $scope.chart = undefined;
                                    $scope.stepChart = undefined;
                                }
                            }

                            //sort participant list by online users
                            if ($scope.TTOptions.hideOfflineUsers) {
                                var filteredParticipantList = [];
                                for (var ol = 0; ol < Temp_List.length; ol++) {
                                    var index = Temp_List[ol];
                                    if (index[3] === 1) {
                                        filteredParticipantList.push(index);
                                    }
                                }
                                Temp_List = filteredParticipantList;
                            }

                            //this code calculates the items in the *number to display participants dropdown* in the participant list
                            $scope.pagination_select_list = [];
                            for (i = 1; i <= Math.floor(Temp_List.length / 5) ; i++) {
                                $scope.pagination_select_list.push(i * 5);
                            }

                            //jdata = "";
                            $scope.users_list = Temp_List;
                            $scope.pagination_pages = pagination_details();

                            //this code does all the judgment
                            
                            for (i = 0; i < $scope.users_list.length; i++) {
                                $scope.users_list[i] = Temp_List[i];
                                var judgment = $scope.users_list[i][5];

                               // $scope.information_nodes.Task = task;
                                if ($scope.users_list[i][0] == $scope.current_user[1]) {

                                    
                                    $scope.current_user[2] = $scope.users_list[i][4];
                                    
                                    if ($scope.current_user[2] == -2 && $scope.current_user[0].toLowerCase() != $scope.meeting_owner.toLowerCase()) {
                                        $scope.restricted = true;
                                        $scope.$parent.isRestricted = true;
                                    }
                                    if ($scope.current_user[2] != -2 && $scope.current_user[0].toLowerCase() != $scope.meeting_owner.toLowerCase()) {
                                        $scope.restricted = false;
                                        $scope.$parent.isRestricted = false;
                                    }

                                    $scope.current_user[3] = $scope.users_list[i][5] == -2147483648000 ? '' : $scope.users_list[i][5];
                                    $scope.user_judgment = $scope.users_list[i][5];
                                    $scope.comment_txt[1] = $scope.users_list[i][6];
                                    if ($scope.page_load == false) {
                                        $scope.comment_txt[0] = $scope.split_comment($scope.users_list[i][6], 0);
                                        $scope.page_load = true;
                                    }
                                }

                                if ($scope.restricted) {
                                    $scope.current_action_type = "restricted";
                                    output.actions = "restricted";
                                    $scope.teamtime_body = true;
                                    $scope.$parent.showLoadingModal = false;
                                    return false;
                                }
                                switch ($scope.current_action_type) {
                                    case 'rating':
                                        $scope.information_nodes.ParentNode = pipe_data[0][0][2];
                                        $scope.information_nodes.LeftNode = pipe_data[0][1][2];
                                       // $scope.information_nodes.Task = task;

                                        if ($scope.users_list[i][5].toString().indexOf('*') >= 0) {
                                            $scope.ratings_direct_values[i] = $scope.users_list[i][5].replace("*", '');
                                        }
                                        else{
                                            $scope.ratings_direct_values.splice(i, 1);
                                        }

                                        break;

                                    case 'direct':
                                        $scope.information_nodes.ParentNode = pipe_data[0][0][2];
                                        $scope.information_nodes.LeftNode = pipe_data[0][1][2];
                                       // $scope.information_nodes.Task = task;

                                        //$scope.direct[$scope.users_list[i][0]] = $scope.users_list[i][5] >= 0 ? parseFloat($scope.users_list[i][5]) : null;
                                        $scope.direct[i] = $scope.users_list[i][5] >= 0 ? parseFloat($scope.users_list[i][5]) : null;
                                        //$scope.update_slider(i, $scope.users_list[i][1], $scope.users_list[i][6])
                                        break;
                                    case 'step':
                                        $scope.information_nodes.ParentNode = pipe_data[0][0][2];
                                        $scope.information_nodes.LeftNode = pipe_data[0][1][2];
                                        //$scope.information_nodes.Task = task;

                                        //console.log($scope.stepChart);
                                        if ($scope.users_list[i][0] == $scope.current_user[1] && typeof ($scope.stepChart) == 'undefined') {
                                            var minE = 0;
                                            var maxE = 0;
                                            /// console.log("render step chart!");
                                            minE = $scope.stepfunction_scale[0][2];
                                            maxE = $scope.stepfunction_scale[pipe_data[1].length - 1][2];

                                            $scope.isPiecewise = pipe_data[5] == 1;
                                            $scope.$parent.showLoadingModal = false;

                                            $timeout(function() {
                                                $scope.render_step_function(minE, maxE, $scope.stepfunction_scale);
                                            }, 1200);
                                                   
                                                    
                                        }
                                        // else {
                                            // console.log("dont render step chart!");
                                        // }
                                        $scope.stepfunction_input[i] = $scope.users_list[i][5] < $scope.lowest_value ? '' : $scope.users_list[i][5];
                                        $scope.stepfunction_piecewise = pipe_data[5];
                                        //$scope.stepfunction_priority[i] = $scope.stepfunction_input[i];
                                        break;
                                    case 'ruc':
                                        $scope.information_nodes.ParentNode = pipe_data[0][0][2];
                                        $scope.information_nodes.LeftNode = pipe_data[0][1][2];
                                       // $scope.information_nodes.Task = task;
                                        //console.log($scope.chart);
                                        minE = $scope.stepfunction_scale[0];
                                        maxE = $scope.stepfunction_scale[1];
                                        $scope.$parent.showLoadingModal = false;
                                        // alert($scope.chart);
                                        if ($scope.users_list[i][0] == $scope.current_user[1] && typeof ($scope.chart) == 'undefined') {
                                            //console.log("render uc chart!");
                                            $timeout(function() {
                                                $scope.render_UC_canvas();
                                            }, 1200);
                                        }
                                        //else {
                                        //    console.log("dont render uc chart!");
                                        //}
                                        $scope.uc_input[i] = $scope.users_list[i][5] < $scope.lowest_value ? '' : $scope.users_list[i][5];
                                        //$scope.calculate_uc_priority($scope.uc_input[i]);
                                        break;

                                    case 'pairwise':
                                        //Nodes Information
                                        //$scope.current_mtype = pipe_data[4];
                                        var current_mtype = pipe_data[4];
                                        $scope.information_nodes.ParentNode = pipe_data[0][0][2];
                                        $scope.information_nodes.LeftNode = pipe_data[0][1][2];
                                       // $scope.information_nodes.Task = task;
                                        $scope.information_nodes.RightNode = pipe_data[0][2][2];

                                        $scope.verbal_favorable_nodes.left = pipe_data[0][1][2];
                                        $scope.verbal_favorable_nodes.right = pipe_data[0][2][2];
                                        switch (current_mtype) {

                                            case 1:
                                                break;

                                            case 9:
                                            case 99:
                                                //$scope.participant_slider[i] = [($scope.user_judgment * 50) + 400, 1000, ($scope.user_judgment * 50) + 1200];
                                                if ($scope.switch_to_old) {
                                                    var index = $scope.users_list[i][0];
                                                    if (!$scope.TTOptions.singleLineMode) {
                                                        if ($scope.users_list[i][5] == -2147483648000 || $scope.users_list[i][5] == 0) {
                                                            $scope.participant_slider[index] = 800;
                                                        }
                                                        else if ($scope.users_list[i][5] > 0) {
                                                            var reverse = (1600 * (($scope.users_list[i][5] + 1) / (($scope.users_list[i][5]) + 2)));
                                                            $scope.participant_slider[index] = 1600 - reverse;
                                                        }
                                                        else if ($scope.users_list[i][5] < 0) {
                                                            $scope.participant_slider[index] = 800 * ((($scope.users_list[i][5] * -1) + 1) / (($scope.users_list[i][5] * -1) + 2));
                                                        }


                                                        $scope.left_input[index] = parseFloat(($scope.users_list[i][5] > 0) ? ($scope.users_list[i][5] + 1) * 1 : 1).toFixed(2);
                                                        $scope.right_input[index] = parseFloat(($scope.users_list[i][5] < 0) ? ($scope.users_list[i][5] - 1) * -1 : 1).toFixed(2);
                                                        if ($scope.users_list[i][5] < $scope.lowest_value) {
                                                            $scope.participant_slider[index] = 800;
                                                            $scope.left_input[index] = '';
                                                            $scope.right_input[index] = '';

                                                        }

                                                        updateTextPosition(i);

                                                        if ($scope.user_judgment >= 0) {
                                                            $scope.graphical_slider[1] = (1600 * (($scope.user_judgment + 1) / (($scope.user_judgment) + 2)));
                                                            $scope.graphical_slider[0] = 1600 - $scope.graphical_slider[1];
                                                        }
                                                        if ($scope.user_judgment <= 0) {
                                                            $scope.graphical_slider[0] = (1600 * ((($scope.user_judgment * -1) + 1) / (($scope.user_judgment * -1) + 2)));
                                                            $scope.graphical_slider[1] = 1600 - $scope.graphical_slider[0];
                                                        }

                                                        $scope.main_bar[0] = $scope.user_judgment > $scope.lowest_value ? parseFloat(($scope.user_judgment < 0 ? ($scope.user_judgment - 1) * -1 : 1).toFixed(2)) : 0;
                                                        $scope.main_bar[1] = $scope.user_judgment > $scope.lowest_value ? parseFloat(($scope.user_judgment > 0 ? ($scope.user_judgment + 1) * 1 : 1).toFixed(2)) : 0;
                                                        $scope.main_bar_txt[0] = ($scope.user_judgment < 0 ? ($scope.user_judgment - 1) * -1 : 1).toFixed(2);
                                                        $scope.main_bar_txt[1] = ($scope.user_judgment > 0 ? ($scope.user_judgment + 1) * 1 : 1).toFixed(2);
                                                        if ($scope.user_judgment < -999999999999) {
                                                            $scope.graphical_slider[0] = 800;
                                                            $scope.graphical_slider[1] = 800;
                                                            $scope.main_bar[0] = '';
                                                            $scope.main_bar[1] = '';
                                                            $('#dsBarClr1').slider('value', $scope.graphical_slider[0]);
                                                            $('#dsBarClr2').slider('value', $scope.graphical_slider[1]);
                                                            $scope.main_bar_txt[0] = 'no judgment';
                                                            $scope.main_bar_txt[1] = 'no judgment';

                                                            $('.a_Slider .ui-slider-range').css('background', '#c9c9c9');
                                                            $('.b_Slider .ui-slider-range').css('background', '#c9c9c9');

                                                        }
                                                    }

                                                    //Single Line Mode
                                                    if ($scope.TTOptions.singleLineMode) {
                                                        var user_id = $scope.users_list[i][0];
                                                        if ($scope.users_list[i][5] <= 0 && $scope.users_list[i][5] > $scope.lowest_value) {
                                                            $scope.single_line_slider[user_id] = 800 - (800 * (($scope.users_list[i][5] - 1) / (($scope.users_list[i][5]) - 2)));
                                                        }
                                                        if ($scope.users_list[i][5] >= 0) {
                                                            $scope.single_line_slider[user_id] = (800 * (($scope.users_list[i][5] + 1) / (($scope.users_list[i][5]) + 2)));
                                                        }
                                                        if ($scope.users_list[i][5] < $scope.lowest_value) {
                                                            $scope.single_line_slider[user_id] = 80;
                                                        }
                                                    }


                                                    //nav styles

                                                    //show and hide text inputs for judgment
                                                    $scope.is_judgment_input[i] = true;
                                                    var index = parseInt($scope.users_list[i][0]);
                                                    var ss = parseInt($('.g-slider' + index + ' .graphicalSlider .ui-slider-handle').css('left'), 10);

                                                    $scope.nav_left_style[i] = {
                                                        'left': ' ' + ss - 100 + 'px'
                                                    };
                                                    $scope.nav_right_style[i] = {
                                                        'left': ' ' + ss - 10 + 'px'
                                                    };
                                                    if ($scope.users_list[i][5] < $scope.lowest_value) {
                                                        //$scope.$apply(function () {
                                                        $('.g-slider' + index + ' .graphicalSlider .ui-slider-range').css('background', '#c9c9c9');
                                                        $('.g-slider' + index + ' .graphicalSlider ').css('background', '#c9c9c9');
                                                        //});
                                                    }
                                                }
                                                if (!$scope.switch_to_old) {
                                                    //var index = $scope.users_list[i][0];

                                                    var advantage = '';
                                                    if (!$scope.TTOptions.singleLineMode) {
                                                        if ($scope.users_list[i][5] < $scope.lowest_value || $scope.users_list[i][5] == 0) {
                                                            $scope.single_line_slider[i] = 400;
                                                            $scope.main_slider[i] = 400;
                                                            advantage = 0;
                                                        }
                                                        else if ($scope.users_list[i][5] < 0) {
                                                            $scope.single_line_slider[i] = 800 - (800 * ((($scope.users_list[i][5] * -1) + 1) / (($scope.users_list[i][5] * -1) + 2)));
                                                            $scope.main_slider[i] = 800 - (800 * ((($scope.users_list[i][5] * -1) + 1) / (($scope.users_list[i][5] * -1) + 2)));
                                                            advantage = -1;
                                                        }
                                                        else if ($scope.users_list[i][5] > 0) {
                                                            $scope.single_line_slider[i] = (800 * (($scope.users_list[i][5] + 1) / (($scope.users_list[i][5] + 1) + 1)));
                                                            $scope.main_slider[i] = (800 * (($scope.users_list[i][5] + 1) / (($scope.users_list[i][5] + 1) + 1)));
                                                            advantage = 1;
                                                        }


                                                        //$scope.left_input[index] = parseFloat(($scope.users_list[i][5] > 0 && advantage == 1) ? ($scope.users_list[i][5]) : 1).toFixed(2);
                                                        //$scope.right_input[index] = parseFloat(($scope.users_list[i][5] < 0 && advantage == -1) ? ($scope.users_list[i][5] * -1) : 1).toFixed(2);
                                                        $scope.left_input[i] = parseFloat((($scope.users_list[i][5] < 0 && advantage == 1) ? ($scope.users_list[i][5] * -1) + 1 : 1).toFixed(2));
                                                        $scope.right_input[i] = parseFloat((($scope.users_list[i][5] > 0 && advantage == -1) ? ($scope.users_list[i][5] + 1) : 1).toFixed(2));
                                                        $scope.left_bar[i] = parseFloat((($scope.users_list[i][5] < 0 && advantage == 1) ? ($scope.users_list[i][5] * -1) + 1 : 1).toFixed(2));
                                                        $scope.right_bar[i] = parseFloat((($scope.users_list[i][5] > 0 && advantage == -1) ? ($scope.users_list[i][5]) + 1 : 1).toFixed(2));
                                                        if ($scope.users_list[i][5] < $scope.lowest_value) {
                                                            $scope.main_slider[i] = 400;
                                                            $scope.left_bar[i] = '';
                                                            $scope.right_bar[i] = '';
                                                            $scope.left_input[i] = '';
                                                            $scope.right_input[i] = '';
                                                        }

                                                        //updateTextPosition(i);

                                                    }
                                                }
                                                break;
                                        }

                                        break;
                                }

                            }

                            switch ($scope.current_action_type) {
                                case 'message':
                                    var messageId = pipe_data[0];
                                    var data = JSON.stringify({
                                        messageId: messageId
                                    });
                                    $http({
                                        method: "POST",
                                        url: $scope.baseurl + 'pages/teamtimetest/teamtime.aspx/GetInformationMessage',
                                        data: JSON.stringify({
                                            messageId: messageId
                                        }),
                                        async: false
                                    })
                                        .then(function success(response) {
                                            $scope.InformationMessage = response.data.d;
                                        }, function error(response) {
                                            console.log(response);
                                        });
                                    break;
                                case 'localresults':
                                    $scope.normalization_list = [{ "value": 2, "text": "Normalized" }, { "value": 3, "text": "Unnormalized" }, { "value": 4, "text": "% of Max. Normalized" }];
                                    $scope.normalization = $scope.normalization_list[0];
                                    $scope.JSON_results = pipe_data;
                                    $scope.isObjective = $scope.output.goal == $scope.JSON_results[0];
                                    //////////console.log(pipe_data);
                                    if($scope.isObjective)
                                        $scope.resultMode = $scope.output.nameObjectives;
                                    else
                                        $scope.resultMode = $scope.output.nameAlternatives;
                                    $scope.currentResultsNode = $scope.JSON_results[0];
                                    //////////console.log($scope.resultMode);
                                    //$scope.results_node = output.pipeData.currentNode;
                                    $scope.task = task;


                                    $scope.results_data = [];
                                    var tempresults = $scope.JSON_results[2];
                                    for (x = 0; x < tempresults.length; x++) {
                                        var temparray = [];
                                        temparray[0] = tempresults[x][0];
                                        temparray[1] = tempresults[x][1];
                                        for (y = 0; y < tempresults[x][2].length; y++) {
                                            if ($scope.current_user[1] == tempresults[x][2][y][0]) {
                                                temparray[2] = tempresults[x][2][y][$scope.normalization.value];
                                            }
                                        }
                                        temparray[3] = tempresults[x][2][tempresults[x][2].length - 1][$scope.normalization.value];
                                        $scope.results_data.push(temparray);
                                    }
                                    
                                    $scope.highest_result = 100;
                                    if (parseFloat($scope.results_data[0][2]) > 0 || parseFloat($scope.results_data[0][3]) > 0) {
                                        $scope.highest_result = 0;
                                    }
                                    for (i = 0; i < $scope.results_data.length; i++) {
                                        if ($scope.highest_result < parseFloat($scope.results_data[i][2])) {
                                            $scope.highest_result = parseFloat($scope.results_data[i][2]);
                                        }
                                        if ($scope.highest_result < parseFloat($scope.results_data[i][3])) {
                                            $scope.highest_result = parseFloat($scope.results_data[i][3]);
                                        }
                                        
                                    }
                                    $scope.reverse = false;

                                    $scope.columnname = "this[" + $scope.JSON_results[3][1] + "]";
                                    break;

                                case 'globalresults':
                                    $scope.normalization_list = [{ "value": 2, "text": "Normalized" }, { "value": 3, "text": "Unnormalized" }, { "value": 4, "text": "% of Max. Normalized" }];
                                    $scope.normalization = $scope.normalization_list[0];
                                    $scope.JSON_results = pipe_data;
                                    $scope.results_node = $scope.JSON_results[3];
                                    $scope.task = task;


                                    $scope.wrtNodeId = pipe_data[0];
                                    //////////console.log(pipe_data[0]);

                                    //Code below to reorder tree node
                                    $scope.treeNode = [];
                                    for (x = 0; x < pipe_data[3].length; x++) {
                                        if ($scope.results_node[x][6] != -1) {
                                            $scope.treeNode.push($scope.results_node[x]);
                                            $scope.treeNode[x][6] = 0;
                                            $scope.treeNode[x][7] = [];
                                            $scope.treeNode[x][8] = 1;
                                        }
                                        if (pipe_data[0] == pipe_data[3][x][0])
                                            $scope.wrt_node_name = pipe_data[3][x][2];
                                        for (y = 0; y < pipe_data[3].length; y++) {
                                            if (x != y && pipe_data[3][x][0] == pipe_data[3][y][1]) {
                                                $scope.treeNode[x][6] = 1;
                                                $scope.treeNode[x][7].push($scope.results_node[y]);
                                            }
                                        }
                                    }
                                    $scope.treeNode = eval("[" + JSON.stringify($scope.treeNode[0]) + "]");
                                    //////////console.log($scope.wrtNodeId);
                                    //////////console.log(JSON.stringify($scope.treeNode));
                                    $scope.results_data = [];
                                    var tempresults = $scope.JSON_results[2];
                                    //                                    console.log(JSON.stringify($scope.JSON_results[2]));
                                    for (x = 0; x < tempresults.length; x++) {
                                        var temparray = [];
                                        temparray[0] = tempresults[x][0];
                                        temparray[1] = tempresults[x][1];
                                        for (y = 0; y < tempresults[x][2].length; y++) {
                                            if ($scope.current_user[1] == tempresults[x][2][y][0]) {
                                                temparray[2] = tempresults[x][2][y][$scope.normalization.value];
                                            }
                                        }
                                        temparray[3] = tempresults[x][2][tempresults[x][2].length - 1][$scope.normalization.value];
                                        $scope.results_data.push(temparray);
                                    }

                                    $scope.highest_result = 0;
                                    if (parseFloat($scope.results_data[0][2]) > 0 || parseFloat($scope.results_data[0][3]) > 0) {
                                        $scope.highest_result = 0;
                                    }
                                    for (i = 0; i < $scope.results_data.length; i++) {
                                        if ($scope.highest_result < $scope.results_data[i][2]) {
                                            $scope.highest_result = $scope.results_data[i][2];
                                        }
                                        if ($scope.highest_result < $scope.results_data[i][3]) {
                                            $scope.highest_result = $scope.results_data[i][3];
                                        }
                                    }


                                    $scope.columnname = "this[" + $scope.JSON_results[4][1] + "]";
                                    //console.log(JSON.stringify($scope.results_data));
                                    break;
                            }
                            //this code does all the judgment
                            if (output.steps !== "" && $scope.output.isPM) {
                                if ($scope.output.previous_step < 1)
                                    $scope.output.previous_step = 1;
                                $scope.steps_list[$scope.output.previous_step - 1] = eval(output.steps);

                            }

                            $scope.group_result[0] = parseFloat(priority);
                            $scope.group_result[1] = parseFloat(variance);
                            //execute the jquery ajax

                            var screen_height = window.outerHeight;
                            var screen_width = window.outerWidth;
                            if (screen_width < 410) {
                                $scope.is_mobile_too_small = true;
                            }
                        
                            //                            //hide jquery modal window
                            
                            hide_loading_icon();
                            $scope.$parent.showLoadingModal = false;
                            $scope.teamtime_body = true;
                        }
                        if (($scope.page_load && $scope.teamtime_body) || $scope.is_reload_pie) {
                            
                            //******************This code is temporary as it is repeating. Need to refactor this next time ************************//
                            $scope.page_load = false;
                            
                            var $a1 = $scope.graphical_slider[0];
                            var $b = 1600 - $a1;
                            var result;
                            if ($scope.user_judgment < -999999999999) {
                                $a1 = null;
                                updateFrontEnd($a1, $b);
                                //Pizza.init('body', {
                                //    data: [0, 100],
                                //});
                            }
                            else {
                                updateFrontEnd($b, $a1);
                                result = calculate_pie_and_slider('blue', $a1);

                                $scope.$apply(function() {
                                    $('#dsBarClr1').slider('value', 1600 - result[1]);
                                    $('#dsBarClr2').slider('value', result[1]);
                                });
                            }
                            $scope.is_reload_pie = false;
                            //******************This code is temporary as it is repeating. Need to refactor this next time ************************//
                            $scope.$parent.showLoadingModal = false;
                        }
                          $scope.$parent.isPM = $scope.current_user[0].toLowerCase() === $scope.meeting_owner.toLowerCase() ? true : false;
                    }
                    else {
                        if (output.meetingOwner == $scope.current_user[0] && !output.TeamTimeOn) {
                            alert('Another Project Manager has stopped the current Teamtime session. You will be redirected to the project list');
                            $window.location.href = $scope.baseurl + "Pages/my-projects.aspx";
                            $scope.current_user = null;
                           
                        }
                        $scope.teamtime_body = true;
                        try {
                            if ($scope.current_user[0].toLowerCase() != $scope.meeting_owner.toLowerCase()) {
                                $scope.current_action_type = 'waiting';
                            }
                        } catch (e) {
                            $scope.current_action_type = 'waiting';
                        }
                        $scope.clearVariables();

                        $scope.$parent.showLoadingModal = false;
                    }

                   // $timeout(function () {
                    $scope.resize_TT_list("participants-list");
                    
                  //  }, 100);


                }
            },
            error: function (response) {
                console.log(response);
            }
        });
    //}
        //}, function error(response) {
        //    console.log('ANGULAR TEAMTIME AJAX ERROR');
        //});

    }
    $scope.scroll_multi_container = function () {
        $(".participants-list").scrollTop(0);
        $("html, body").scrollTop(0);
    }
    $scope.clearVariables = function () {
        $scope.stepChart = undefined;
        $scope.chart = undefined;
        jdata = '';
        $scope.JSON_complete = '';
        
    }

    window.onload = function () {
        $scope.ttTimer = null;
        $scope.$parent.showLoadingModal = true;
        
        $scope.init();
    }

    $scope.show_participant = false;
    $scope.pagination_CurrentPage = 'all';
    $scope.pagination_NoOfUsers = 10;
    $scope.is_tt_owner = false;

    $scope.init = function (fMovestep) {
        var loading_message = stringResources.loadingModalMessage.default;
        LoadingScreen.init(LoadingScreen.type.loadingModal, $("#LoadingModalPercentage"), $(".fullwidth-loading-wrap"));
        LoadingScreen.start(70, loading_message);
        
        if ($scope.show_participant) {
            var storecurrentpage = $scope.pagination_CurrentPage;
            if ($scope.pagination_CurrentPage == 'all') {
                $scope.pagination_CurrentPage = 0;
                $scope.pagination_save('all', 'all');
            } else {
                $scope.pagination_save('click', parseInt($scope.pagination_CurrentPage));
            }
            //$scope.pagination_CurrentPage = storecurrentpage;

        }
        $scope.refreshed = true;

        $http.post(
                $scope.baseurl + "Pages/TeamTimeTest/teamtime.aspx/is_tt_owner",
                JSON.stringify({
                
                })
            )
            .then(function success(response) {
                try {
                    $scope.is_tt_owner = response.data.d;
                } catch (e) {

                }
            });

        if (!fMovestep) {
            $http({
                    method: "POST",
                    url: $scope.baseurl + "Pages/TeamTimeTest/teamtime.aspx/LoadStepList",
                    data: {},
                    async: false
                })
                .then(function success(response) {
                        LoadingScreen.start(100, loading_message);
                        $scope.steps_list = eval("[" + response.data.d + "]");
                        $scope.$parent.steps_list = $scope.steps_list;

                        if ($scope.ttTimer == null) {
                            //Start TT Timer
                            $scope.ttTimer = $interval(function() { $scope.update_users_list(); }, 1000);
                        }

                        LoadingScreen.end(100);
                    },
                    function error(response) {
                        LoadingScreen.end(100);
                    });
        } else {
            LoadingScreen.end(100);
        }


        $(".tt-resizable-panel").css("max-height", "150px");
        $(".tt-resizable-panel").resizable({
            handles: 's'
        });
    }
    $scope.$on('MovePipeStep', function (e, step) {
        $scope.MovePipeStep(step);
    });

    $scope.MovePipeStep = function (step) {
        $scope.$parent.footer_up();
        $scope.$parent.showLoadingModal = true;
        $("#tt-s-modal").foundation('reveal', 'close');
        $('.pm-bars').removeClass('lft-selected').removeClass('rgt-selected');
        $scope.refreshed = false;
        $scope.$parent.is_anytime = false;
        $scope.$parent.is_teamtime = true;
        $scope.active_multi_index = 0;
        $http.post(
            $scope.baseurl + "Pages/TeamTimeTest/teamtime.aspx/MovePipeStep",
            {//JSON.stringify({
                step: step
                //})
            }
            )
        .then(function success(response) {
            $scope.init(true);
            //$scope.users_list = [];
            $scope.JSON_complete = '';
            $('#move_to_pipe').val("");
            $scope.scroll_multi_container();

        });
    }
    //*************** ANGULAR TIMER *******************

    $scope.baseurl = baseUrl;
    // ---- USER'S EVALUATION
    $scope.users_evaluation = [];

    $scope.$on('view_evaluation', function (e) {
        $scope.view_evaluation();
    });

    $scope.view_evaluation = function () {
        $scope.users_evaluation = $scope.users_eval_progress;
        var current_user_index = -1;

        //push current user on top of array
        for (var i=0; i < $scope.users_evaluation.length; i++){
            if ($scope.current_user[0] ==  $scope.users_evaluation[i][0].toLowerCase()) {
                current_user_index = i;
                break;
            }   
        }

        var jsonData = eval("[" + $scope.output.sdata + "]");
        var orig_users_eval_progress = jsonData[0].progress.evaluationProgress;
     

        var current_eval_user = orig_users_eval_progress[current_user_index];

        var temp_users_evaluation = $scope.users_evaluation;

        temp_users_evaluation.splice(current_user_index, 1);
        
        temp_users_evaluation.splice(0, 0, orig_users_eval_progress[current_user_index]);
        $scope.users_evaluation = temp_users_evaluation;
    }
    // ---- USER'S EVALUATION

    // ---- PARTICIPANT LIST
    $scope.event = 'none';
    $scope.users_list = [];
    $scope.update = function (judgment, request, mtype, baseurl, user_list, user_email, index) {
        if (request == 'individual') {

            if ($scope.show_participant == true) {
                $scope.show_participant = false;
            }
            else {
                $scope.show_participant = true;
            }
            update($scope.show_participant ? 1 : 0, $scope.getStepGuid(), request, mtype, baseurl);
        }
        else {
            update(judgment, $scope.getStepGuid(), request, mtype, baseurl, user_list, user_email, index);
            $scope.hide_loading_on_same_judgment(judgment[0], judgment[2]);
        }

    };


    $scope.user_display_options = [{ "value": 0, "text": "Name Only" }, { "value": 1, "text": "Email Only" }, { "value": 2, "text": "Name & Email" }];
    $scope.user_display = $scope.user_display_options[0];
    $scope.set_user_display = function (value) {
        $scope.user_display = $scope.user_display_options[value];
        if ($scope.TTOptions.isAnonymous == 0)
            update("false_" + value, $scope.getStepGuid(), 'anonymous', '', $scope.baseurl);
        else
            update("true_" + value, $scope.getStepGuid(), 'anonymous', '', $scope.baseurl);
    };

    // ---- PARTICIPANT LIST

    // ---- PAGINATION
    $scope.pagination_save = function (request, value) {
        
        if (request == 'click') {
            $scope.pagination_CurrentPage = value;
            //show_paginated_participant(false);
            update($scope.pagination_CurrentPage + '_' + $scope.pagination_NoOfUsers, $scope.getStepGuid(), 'pagination', '', $scope.baseurl);
        }

        if (request == 'option') {
            $scope.pagination_CurrentPage = 1;
            $scope.pagination_NoOfUsers = value;
            //show_paginated_participant(false);
            update($scope.pagination_CurrentPage + '_' + $scope.pagination_NoOfUsers, $scope.getStepGuid(), 'pagination', '', $scope.baseurl);
        }

        if (request == 'all') {
            $('.participants-list').scrollTop(0);
            var is_all = true;
            var passed_value = 0;
            if ($scope.pagination_CurrentPage == 'all') {
                value = 1;
                passed_value = 1;
                is_all = false;
            }
            $scope.pagination_CurrentPage = passed_value;
            update(passed_value + '_' + $scope.pagination_NoOfUsers, $scope.getStepGuid(), 'pagination', '', $scope.baseurl);
        }

        if (request == 'increment') {
            $('.participants-list').scrollTop(0);
            if ($scope.pagination_CurrentPage == 'all') {
                value = 1;
            }
            if (!(value > Math.ceil($scope.users_list.length / $scope.pagination_NoOfUsers))) {
                $scope.pagination_CurrentPage = value;

            }
        }

        if (request == 'decrement') {
            $('.participants-list').scrollTop(0);
            if ($scope.pagination_CurrentPage == 'all') {
                value = 1;
            }
            if (!(value < 1)) {
                $scope.pagination_CurrentPage = value;
            }

        }
        $scope.pagination_pages = pagination_details();
       // $scope.resize_TT_list("participants-list");
    }

    $scope.check = function (index) {
        //return $scope.show_participant && $scope.show_paginated_participant[index];
        return $scope.show_participant;
    }

    $scope.check_hide_offline = function (is_online) {
        if (Boolean($scope.TTOptions.hideOfflineUsers) == false) {
            return true;
        }
        else if (Boolean($scope.TTOptions.hideOfflineUsers) == true && is_online == 1) {
            return true;
        }
        else {
            return false;
        }

    }

    function pagination_details() {
        var pages = Math.ceil($scope.users_list.length / $scope.pagination_NoOfUsers);
        var pagination_list = [];
        pagination_list.push('<');
        
        for (var i = 1; i <= pages; i++) {
            pagination_list.push(i);
        }
        pagination_list.push('>');
        pagination_list.push('select');
        pagination_list.push('all');

        return pagination_list;
    }


    // ---- PAGINATION

    //function show_paginated_participant(is_all) {
    //    if (!is_all) {
    //        for (i = 0; i < $scope.users_list.length; i++) {
    //            if ($scope.pagination_CurrentPage == Math.ceil((i + 1) / $scope.pagination_NoOfUsers)) {
    //                $scope.show_paginated_participant[i] = true;
    //            }
    //            else {
    //                $scope.show_paginated_participant[i] = false;
    //            }
    //        }
    //    }
    //    else {
    //        for (i = 0; i < $scope.users_list.length; i++) {
    //            $scope.show_paginated_participant[i] = true;
    //        }
    //    }
    //}

    //************ STEPS LIST MODAL------------


    //$scope.STEPS_get_steps = function () {

    //    $scope.STEPS_pipe_steps = steps;
    //}
    $scope.MoveToPipe = function (pipe, event) {
        if (event != null) {
            var code = event.keyCode;
            if (code == 13) {
                
                if (pipe > 0 && pipe <= $scope.$parent.output.totalPipe)
                    $scope.MovePipeStep(pipe);
                return event.preventDefault();
            }
        }
        else{
            if (pipe > 0 && pipe <= $scope.$parent.output.totalPipe)
                $scope.MovePipeStep(pipe);
        }


    }


    $scope.get_step_display = function (step, current_step, total_step, request, is_mobile) {
        var val1 = 5;
        var val2 = 3;
        var val3 = 4;
        if (is_mobile) {
            val1 = 2;
            val2 = 1;
            val3 = 2;
        }

        var LeftStep = val1 - (current_step - 1);
        var RightStep = val1 - (total_step - current_step);
        if (step < (current_step - val2) - Math.max(0, RightStep) && step != 1 && (current_step - 3) - 2 != 1) {
            if (step == (current_step - val3) - Math.max(0, RightStep) && request == 'unavailable') {
                return true;
            }
            return false;
        }
        else if (step > (current_step + val2) + Math.max(0, LeftStep) && step != total_step && (current_step + 3) + 2 != total_step) {
            if (step == total_step - 1 && request == 'unavailable') {
                return true;
            }
            return false;
        }
        else {
            if (request == 'available' && current_step != step) {
                if ($scope.is_mobile_too_small) {
                    if (current_step - 1 == step || current_step + 1 == step) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                return true;
            }
        }
    }

    // ------------STEPS LIST MODAL************>

    //************ Local/GLobal Results------------
    $scope.normalization_list = [{ "value": 2, "text": "Normalized" }, { "value": 3, "text": "Unnormalized" }, { "value": 4, "text": "% of Max. Normalized" }];
    $scope.normalization = $scope.normalization_list[0];
    var temp_own_value;
    var temp_combine_value;
    $scope.get_results = function (request, child) {
        if (request == 'own') {
            for (i = 0; i < child.length; i++) {
                if ($scope.current_user[1] == child[i][0]) {
                    temp_own_value = child[i][$scope.normalization.value];
                    if (temp_own_value <= 0) { temp_own_value = 0; }
                    return temp_own_value;
                }
            }
        }
        if (request == 'combine') {
            temp_combine_value = child[child.length - 1][$scope.normalization.value];
            if (temp_combine_value <= 0) { temp_combine_value = 0; }
            return temp_combine_value;
        }
        if (request == 'own_bar') {
            return { 'width': ((child / $scope.highest_result) * 100).toFixed(2) + '%' };
        }
        if (request == 'combine_bar') {
            return { 'width': ((child / $scope.highest_result) * 100).toFixed(2) + '%' };
        }
    }

    $scope.normalization_change = function (normalized) {
        $scope.JSON_complete = '';
        $scope.normalization = normalized;

        $scope.results_data = [];
        var tempresults = $scope.JSON_results[2];

        for (x = 0; x < tempresults.length; x++) {
            var temparray = [];
            temparray[0] = tempresults[x][0];
            temparray[1] = tempresults[x][1];
            for (y = 0; y < tempresults[x][2].length; y++) {
                if ($scope.current_user[1] == tempresults[x][2][y][0]) {
                    temparray[2] = tempresults[x][2][y][$scope.normalization.value];
                }
            }
            temparray[3] = tempresults[x][2][tempresults[x][2].length - 1][$scope.normalization.value];
            $scope.results_data.push(temparray);
        }

        $scope.highest_result = 100;
        if (parseFloat($scope.results_data[0][2]) > 0 || parseFloat($scope.results_data[0][3]) > 0) {
            $scope.highest_result = 0;
        }
        for (i = 0; i < $scope.results_data.length; i++) {
            if ($scope.highest_result < parseFloat($scope.results_data[i][2])) {
                $scope.highest_result = parseFloat($scope.results_data[i][2]);
            }
            if ($scope.highest_result < parseFloat($scope.results_data[i][3])) {
                $scope.highest_result = parseFloat($scope.results_data[i][3]);
            }
            //////////console.log(JSON.stringify($scope.highest_result));
        }
        //console.log(JSON.stringify($scope.results_data));
    }

    $scope.save_WRTNodeID = function (wrtID) {
        show_loading_icon();
        update(wrtID, $scope.getStepGuid(), 'wrtID', '', $scope.baseurl);
    }
    $scope.results_data = [];
    $scope.results_bar = [];
    $scope.columnname = 'this[0]';
    $scope.sort_Results = function (request) {
        switch (request) {
            case 'nodeID':
                $scope.reverse = !$scope.reverse; $scope.columnname = 'this[0]';
                break;
            case 'nodeName':
                $scope.reverse = !$scope.reverse; $scope.columnname = 'this[1]';
                break;
            case 'yourResults':
                $scope.reverse = !$scope.reverse; $scope.columnname = 'this[2]';
                break;
            case 'combine':
                $scope.reverse = !$scope.reverse; $scope.columnname = 'this[3]';
                break;
        }
    }

    $scope.set_reverse = function () {
        $scope.reverse = !$scope.reverse;
    }

    $scope.teamtimecolumns = [["nodeID", "Index"], ["nodeName", "Name"], ["yourResults", "Your Results"], ["combine", "Combined"]];

    $scope.change_normalization = function (normalized) {
        show_loading_icon();
        $scope.JSON_complete = "";
        $scope.normalization = normalized;

        $scope.results_data = [];
        var tempresults = $scope.JSON_results[2];
        for (x = 0; x < tempresults.length; x++) {
            var temparray = [];
            temparray[0] = tempresults[x][0];
            temparray[1] = tempresults[x][1];
            for (y = 0; y < tempresults[x][2].length; y++) {
                if ($scope.current_user[1] == tempresults[x][2][y][0]) {
                    temparray[2] = tempresults[x][2][y][$scope.normalization.value];
                }
            }
            temparray[3] = tempresults[x][2][tempresults[x][2].length - 1][$scope.normalization.value];
            $scope.results_data.push(temparray);
        }
        //////////console.log(JSON.stringify($scope.results_data));
        $scope.highest_result = 0;
        if (parseFloat($scope.results_data[0][2]) > 0 || parseFloat($scope.results_data[0][3]) > 0) {
            $scope.highest_result = 0;
        }
        for (i = 0; i < $scope.results_data.length; i++) {
            if ($scope.highest_result < $scope.results_data[i][2]) {
                $scope.highest_result = $scope.results_data[i][2];
            }
            if ($scope.highest_result < $scope.results_data[i][3]) {
                $scope.highest_result = $scope.results_data[i][3];
            }
        }

        hide_loading_icon();
    }

    // ------------Local/Global Results************>

    // ------------Pairwise Verbal************>

    $scope.information_nodes = [
    { "ParentNode": "", "LeftNode": "", "RightNode": "", "Task" : "" }
    ];

   // $scope.information_nodes_infodocs = [
   //{ "ParentNode": "", "LeftNode": "", "RightNode": "", "WrtLeftNode":"", "WrtRightNode":"" }
   // ];
    $scope.bars_left =
                new Array(
                    [-8, "Extreme", "nine", "EX"],
                    [-7, "Very Strong to Extreme", "eight", " "],
                    [-6, "Very Strong", "seven", "VS"],
                    [-5, "Strong to Very Strong", "six", " "],
                    [-4, "Strong", "five", "S"],
                    [-3, "Moderate to Strong", "four", " "],
                    [-2, "Moderate", "three", "M"],
                    [-1, "Equal to Moderate", "two", " "]
                );
    $scope.bars_right = new Array(
                        [1, "Equal to Moderate", "two", " "],
                        [2, "Moderate", "three", "M"],
                        [3, "Moderate to Strong", "four", " "],
                        [4, "Strong", "five", "S"],
                        [5, "Strong to Very Strong", "six", " "],
                        [6, "Very Strong", "seven", "VS"],
                        [7, "Very Strong to Extreme", "eight", " "],
                        [8, "Extreme", "nine", "EX"]
                    );

    $scope.gradient_checked = $(".gradient-checker").is(":checked");

    $scope.verbal_favorable_nodes = [{ 'left': '', 'right': '' }];

    $scope.show_selected_item = function (value) {
        var index = parseInt(value);

        if (value > -9 && value < 0) {
            index = 8 + (value);
            return '<div class=" text-centered small-6 left columns"> <span class="leftLabel">' + $scope.bars_left[index][1] + '</span></div>';
        }
        else if (value < 9 && value > 0) {
            return '<div class=" text-centered  small-6 right columns"> <span class="rightLabel">' + $scope.bars_right[index - 1][1] + '</span></div>';
        }
        else {
            if (value == 0) {
                return '<div class=" text-center small-6 small-centered columns"> <span class="equal">Equal</span> </div>';
            }
            return '';
        }
    }
    // ------------Pairwise Verbal************>

    // ------------Pairwise Graphical--------------//
    // Switch to old pairwise graphical slider
    $scope.SwitchToOld = function (switch_to_old) {
        $scope.switch_to_old = switch_to_old;
    }
    $scope.nav_mousedown = function (element, index, judgment,event) {
        var step = .01; // value of movement in width
        if (element == 'rightclick') {
            var a = $('#a_' + index).width();
        }
        $scope.nav_interval = $interval(function () {
            if (element == 'left') {
                if ($scope.users_list[index][5] < $scope.lowest_value) { $scope.users_list[index][5] = 0; }
                $scope.users_list[index][5] += step;

                judgment = $scope.users_list[index][5];
                
            }
            if (element == 'right') {
                if ($scope.users_list[index][5] < $scope.lowest_value) { $scope.users_list[index][5] = 0; }
                $scope.users_list[index][5] -= step;
                judgment = $scope.users_list[index][5];
            }

            if (element == 'leftclick') {
                    var barwidth = $('#barindicator').width();
                    var x = event.offsetX;
                    var judgment = (((1600 * ((x / (barwidth*.9))-.05)) / 100) - 8) * -1;
                    $scope.users_list[index][5] = judgment;
            }

            if (element == 'rightclick') {
                var barwidth = $('#barindicator').width();
                var x = event.offsetX + a;
                var judgment = (((1600 * ((x / (barwidth * .9)) - .05)) / 100) - 8) * -1;
                $scope.users_list[index][5] = judgment;
            }

            $scope.nav_left_style[index] = { 'width': (judgment < 0 ? (judgment < -8 ? judgment < -999999999999 ? 0 : 95 : (((800 + (100 * (-judgment))) / 1600) * 100) * .9 + 5) : (((800 - (100 * (judgment))) / 1600) * 100) * .9 + 5) + '%' }
            $scope.nav_right_style[index] = {
                'width': (judgment > 0 ? (judgment > 8 ? 95 : (((800 + (100 * (judgment))) / 1600) * 100) * .9 + 5) : (((800 + (100 * (judgment))) / 1600) * 100) * .9 + 5) + '%'
            };

        }, 25);



    }

    $scope.stop = function () {
        if ($scope.nav_interval != null) {
            $interval.cancel($scope.nav_interval);
            $scope.nav_interval = null;
        }

    }

    $scope.interval_stop = function (email, judgment, comment) {
        
        if ($scope.nav_interval != null) {
            $interval.cancel($scope.nav_interval);
            $scope.nav_interval = null;
            $scope.update([judgment, email, comment], 'save', 'ptGraphical', $scope.baseurl, $scope.users_list.length, $scope.current_user[0]);
        }

    }

    $scope.nav_left_style = [];
    $scope.nav_right_style = [];
    $scope.is_judgment_input = [];

    $scope.latest_index;
    $scope.hide_previous_input = function (index) {
        if ($scope.latest_index != null) {
            if ($scope.latest_index != index) {
                $scope.is_judgment_input[$scope.latest_index] = true;
            }
        }
        $scope.is_judgment_input[index] = false;
        $scope.latest_index = index;
    }

    $scope.left_input = [];
    $scope.right_input = [];
    $scope.left_bar = [];
    $scope.right_bar = [];
    $scope.main_slider = [];
    $scope.graphical_key_up = function (event, value, position, state, index, is_main_bar) {
        var code; var advantage = '';
        if (event)
            code = event.which;
        //////////console.log(event.which);
        $scope.active_multi_index = index;
        if (state == "press") {
            if ((code >= 58 || code < 48 && code != 46) && code != 13)
                return event.preventDefault() && false;
        }
        else {
            if (state == "up" && value[0] != null && value[0] != "") {
                $scope.graphical_input_timeout = $timeout(function () {
                    if (is_main_bar) {
                        if ($scope.left_bar[index] == $scope.right_bar[index]) {
                            value[0] = 0;
                            advantage = 0;
                        }
                        else if ($scope.left_bar[index] != 1 || $scope.right_bar[index] != 1) {
                            if ($scope.left_bar[index] > $scope.right_bar[index]) {
                                if ($scope.right_bar[index] == null || $scope.right_bar[index] == "")
                                    $scope.right_bar[index] = 1;
                                value[0] = ($scope.left_bar[index] / $scope.right_bar[index] * -1) + 1;
                                advantage = -1;
                            }
                            if ($scope.left_bar[index] < $scope.right_bar[index]) {
                                if ($scope.left_bar[index] == null || $scope.left_bar[index] == "")
                                    $scope.left_bar[index] = 1;
                                value[0] = (($scope.right_bar[index] / $scope.left_bar[index]) - 1);
                                advantage = 1;
                            }
                        }
                        else {
                            value[0] = $scope.left_bar[index];
                            advantage = 0;
                        }

                        //$scope.update([value[0], value[1], value[2]], 'save', 'ptGraphical', $scope.baseurl, $scope.users_list.length, $scope.current_user[0]);
                        update([value[0], value[1], value[2]], $scope.getStepGuid(), 'save', 'ptGraphical', $scope.baseurl, $scope.users_list.length, $scope.current_user[1]);
                        if (advantage == -1)
                            $scope.main_slider[index] = 800 - (800 * ((value[0] * -1) / ((value[0] * -1) + 1)));
                        if (advantage == 1)
                            $scope.main_slider[index] = (800 * ((value[0]) / ((value[0]) + 1)));
                        if (advantage == 0)
                            $scope.main_slider[index] = 400;

                        if ($scope.getNoUiSlider(index, is_main_bar) != null && advantage) {
                            $timeout(function () {
                                $scope.setNoUiSlider($scope.main_slider[index], $scope.main_slider[index] + 800, index, is_main_bar);
                            }, 100);
                        }
                        if ($scope.getNoUiSlider(index, is_main_bar) != null && advantage == 0 && value[0] == 0) {
                            $scope.noUiTimeOut = $timeout(function () {
                                $scope.setNoUiSlider(400, 1200, index, is_main_bar);
                                $('.noUi-connect').css('background-color', '#0058a3');
                                $('.graph-green-div').css('background-color', '#6aa84f');
                            }, 100);
                        }
                    }
                    else {
                        if ($scope.left_input[index] == $scope.right_input[index]) {
                            value[0] = 0;
                            advantage = 0;
                        }
                        else if ($scope.left_input[index] != 1 || $scope.right_input[index] != 1) {
                            if ($scope.left_input[index] > $scope.right_input[index]) {
                                if ($scope.right_input[index] == null || $scope.right_input[index] == "")
                                    $scope.right_input[index] = 1;
                                value[0] = (($scope.left_input[index] / $scope.right_input[index]) * -1) + 1;
                                advantage = -1;
                            }
                            if ($scope.left_input[index] < $scope.right_input[index]) {
                                if ($scope.left_input[index] == null || $scope.left_input[index] == "")
                                    $scope.left_input[index] = 1;
                                value[0] = (($scope.right_input[index] / $scope.left_input[index]) - 1);
                                advantage = 1;
                            }
                        }
                        else {
                            value[0] = $scope.left_input[index];
                            advantage = 0;
                        }
                        //$scope.update([value[0], value[1], value[2]], 'save', 'ptGraphical', $scope.baseurl, $scope.users_list.length, $scope.current_user[0]);
                        update([value[0], value[1], value[2]], $scope.getStepGuid(), 'save', 'ptGraphical', $scope.baseurl, $scope.users_list.length, $scope.current_user[1]);
                        if (advantage == -1)
                            $scope.single_line_slider[index] = 800 - (800 * ((value[0] * -1) / ((value[0] * -1) + 1)));
                        if (advantage == 1)
                            $scope.single_line_slider[index] = (800 * ((value[0]) / ((value[0]) + 1)));
                        if (advantage == 0)
                            $scope.single_line_slider[index] = 400;
                        if ($scope.getNoUiSlider(index) != null && advantage) {
                            $timeout(function () {
                                $scope.setNoUiSlider($scope.single_line_slider[index], $scope.single_line_slider[index] + 800, index);
                            }, 100);
                        }
                        if ($scope.getNoUiSlider(index) != null && advantage == 0 && value[0] == 0) {
                            $scope.noUiTimeOut = $timeout(function () {
                                $scope.setNoUiSlider(400, 1200, index);
                                $('.noUi-connect').css('background-color', '#0058a3');
                                $('.graph-green-div').css('background-color', '#6aa84f');
                            }, 100);
                        }
                    }

                    $scope.is_judgment_input[index] = true;
                }, 100);

            }
        }
    }
    $scope.graphical_slider = [];

    $scope.swap_value = function (judgment, useremail, comment, index) {
        $scope.active_multi_index = index;
        if (judgment < -999999999999 || judgment == 0)
            judgment = judgment;
        else if (judgment < 0)
            judgment = Math.abs(judgment);
        else if (judgment > 0)
            judgment = 0 - judgment;
        update([judgment, useremail, comment], $scope.getStepGuid(), 'save', 'ptGraphical', $scope.baseurl, $scope.users_list.length, $scope.current_user[1], index);
    }
    
    $scope.clear_value = function (useremail, comment, index) {
        $scope.active_multi_index = index;
        update([-2147483648000, useremail, comment], $scope.getStepGuid(), 'save', 'ptGraphical', $scope.baseurl, $scope.users_list.length, useremail, index);
    }

    $scope.participant_slider = [];
    $scope.single_line_slider = [];

    $scope.getNoUiSlider = function (index, is_main) {
        if (is_main)
            return document.getElementById('mainSlider' + index);
        return document.getElementById('gslider' + index);


    }

    $scope.setNoUiSlider = function (upper, lower, index, is_main) {
        var slider = $scope.getNoUiSlider(index, is_main);
        slider.noUiSlider.set([upper, lower]);

    }


    $scope.initializaNoUIGraphical = function (upper, lower, index, user, is_main) {
        if (!upper) upper = 400;
        if (!lower) lower = 1200;
        if (user[5] < $scope.lowest_value) { upper = 400; lower = 1200; };
        var lefthandle = upper;
        var righthandle = lower;

        $scope.GraphicalTimeout = $scope.$evalAsync(function () {
            var slider = null;
            if (slider == null) 
                slider = $scope.getNoUiSlider(index, is_main);

            noUiSlider.create(slider, {
                start: [lefthandle, righthandle],
                behaviour: 'drag-fixed-tap',
                connect: true,
                step: 0.01,
                range: {
                    'min': 5,
                    'max': 1600
                },
                pips: {
                    mode: 'positions',
                    values: [4, 7, 11, 15, 23, 50, 77, 85, 89, 93, 96],
                    density: 5,
                    stepped: false
                }
            });
                if (slider && (user[4] < 0 || ($scope.current_user[0] !== $scope.meeting_owner && $scope.current_user[0] !== user[1].toLowerCase())))
                    slider.setAttribute('disabled', true);
                var slider_value = slider.noUiSlider.get();
                var step = (parseFloat(slider_value[1]) - parseFloat(slider_value[0]));
                var max = $('.noUi-base').width();
                var sliderleft = $('.noUi-base').offset().left;
                var zero = (max / 2);
                var lower = $('.noUi-handle-upper').offset().left - $('.noUi-handle-lower').offset().left;
                var half = lower / 2;

                var $div2 = $("<div></div>", { "class": "graph-green-div" });
                $('#' + slider.id + ' .noUi-connect').css('background-color', '#0058a3');
                $div2.css({
                    "background-color": "#6aa84f",
                    "position": "absolute",
                    "left": "50%",
                    "overflow": "hidden",
                    "pointer-events": "none",
                    "max-height": $('#' + slider.id).height(),
                    "width": zero - 1 + "px"
                });
                $div2.width(zero);
                $div2.height(100 + '%');
                $div2.insertAfter($('.noUi-connect'));
                $('.graph-green-div').width(50 + '%');
                var pipsy = ['9', '5', '3', '2', '1', '', '1', '2', '3', '5', '9'];
                $('#' + slider.id + ' .noUi-value-large').each(function (i, e) {
                    $(this).css({ "font-size": "11.5px", "top": "7.5px" });
                    $(this).text(pipsy[i]);
                });

                
                $div2.insertAfter($('#' + slider.id + ' .noUi-connect'));
                $('.noUi-handle').addClass('disabled');
                $('.noUi-handle').css('width', '20px').css('top', '-5px').css('height', '26px');
                $('.noUi-handle.noUi-handle-upper').css('left', '0px');
                $('.noUi-handle.noUi-handle-lower').css('left', '-20px');
                
                if (user[5] < $scope.lowest_value)
                    setNoUiColor(false, index);
                else
                    setNoUiColor(true, index);


        //slider functions
            //var isChangeRunning = true;
                var user_index = ($scope.users_list).indexOf(user);
            slider.noUiSlider.on('change', function (value, handle) {
                
                //isChangeRunning = true;
                var SliderVal = slider.noUiSlider.get();
                //////////////////console.log(slider.noUiSlider.get());
                if ($('#gslider' + index + ' .noUi-origin').is(":hidden")) {
                    $('#gslider' + index + ' .noUi-origin').show();
                    $('#gslider' + index + ' .graph-green-div').show();
                    if (parseFloat(SliderVal[1]) < 800) {
                        slider.noUiSlider.set([SliderVal[1], parseFloat(SliderVal[1]) + 800]);
                        return false;
                    }
                };
                var sliderval = [];
                sliderval[0] = parseFloat(value[0]);
                sliderval[1] = parseFloat(value[1]);
                if (index == 0 || is_main) {
                    if (handle == 0) {
                        $scope.graphicalChangeTimeout = $scope.$evalAsync(function () {
                            $scope.setNoUiSlider(sliderval[0], sliderval[0] + 800, 0);
                            $scope.setNoUiSlider(sliderval[0], sliderval[0] + 800, 0, true);
                        });
                    }
                    else {
                        $scope.graphicalChangeTimeout = $scope.$evalAsync(function () {
                            $scope.setNoUiSlider(sliderval[1] - 800, sliderval[1], 0);
                            $scope.setNoUiSlider(sliderval[1] - 800, sliderval[1], 0, true);
                        });
                    }
                }
                if (SliderVal[0] != slider_value[0]) {
                    var value1 = parseFloat(SliderVal[0]) + step;
                    slider.noUiSlider.set([SliderVal[0], value1]);
                }
                if (SliderVal[1] != slider_value[1]) {
                    var value1 = parseFloat(SliderVal[1]) - step;
                    slider.noUiSlider.set([value1, SliderVal[1]]);
                }
                if (parseFloat(SliderVal[0]) > 720) {
                    slider.noUiSlider.set([720, 1520]);
                }
                else if (parseFloat(SliderVal[0]) < 80) {
                    slider.noUiSlider.set([80, 880]);
                }

                var value = '';
                var sliderValues = setValueSlider(handle, SliderVal);

                //////////////////console.log(value);
                if (sliderValues[2] >= 9)
                    sliderValues[2] = 8;
                if (sliderValues[2] <= -9)
                    sliderValues[2] = -8;
                // commit
                
                svalue = [sliderValues[2], user[1], user[6]];
                //$scope.update(svalue, 'save', 'ptGraphical', $scope.baseurl, $scope.users_list.length, $scope.current_user[0]);
                update(svalue, $scope.getStepGuid(), 'save', 'ptGraphical', $scope.baseurl, $scope.users_list.length, $scope.current_user[1], index);
                $scope.active_multi_index = index;
            });
            slider.noUiSlider.on('slide', function (value, handle) {
                var SliderVal = slider.noUiSlider.get();
                var sliderValues = setValueSlider(handle, SliderVal);
                $scope.GraphicalSlideTimeout = $scope.$evalAsync(function () {
                    setNoUiColor(true, index);
                    if (sliderValues[0] > 9 && $scope.left_input[index] <= 9) sliderValues[0] = 9;
                    if (sliderValues[1] > 9 && $scope.right_input[index] <= 9) sliderValues[1] = 9;

                    $scope.left_input[index] = parseFloat(sliderValues[0].toFixed(2));
                    $scope.right_input[index] = parseFloat(sliderValues[1].toFixed(2));
                    $scope.single_line_slider[0] = $scope.main_slider[0];
                    $scope.left_bar[index] = parseFloat(sliderValues[0].toFixed(2));
                    $scope.right_bar[index] = parseFloat(sliderValues[1].toFixed(2));
                });
            });

            if (user[5] < $scope.lowest_value) {
                //$('#gslider' + index + ' .noUi-origin').hide();
                $scope.left_input[index] = '';
                $scope.right_input[index] = '';
                $scope.left_bar[user_index] = '';
                $scope.right_bar[user_index] = '';
            }
            else {
                var SliderVal = slider.noUiSlider.get();
                if (SliderVal[0] > 400) {
                    SliderVal[1] = ((SliderVal[1] - 800) / (800 - SliderVal[0]));
                    SliderVal[0] = 1;
                }
                else {
                    SliderVal[0] = ((800 - SliderVal[0]) / (SliderVal[0]));
                    SliderVal[1] = 1;
                }
                if (SliderVal[0] > 9 && $scope.left_input[index] <= 9) SliderVal[0] = 9;
                if (SliderVal[1] > 9 && $scope.right_input[index] <= 9) SliderVal[1] = 9;
                $scope.left_input[index] = parseFloat(SliderVal[0].toFixed(2));
                $scope.right_input[index] = parseFloat(SliderVal[1].toFixed(2));
                $scope.left_bar[index] = parseFloat(SliderVal[0].toFixed(2));
                $scope.right_bar[index] = parseFloat(SliderVal[1].toFixed(2));
                $('#gslider' + index + ' .graph-green-div').show();
                $('#mainSlider' + index + '.graph-green-div').show();
            }
            

        });
    }

    function setValueSlider(handle, SliderVal) {
        var returnValue = [];
        if (handle == 0) {
            if (SliderVal[0] > 400) {
                //SliderVal[1] = parseFloat((SliderVal[1] - 800) / (1600 - (SliderVal[1])));
                returnValue[1] = parseFloat((SliderVal[0]) / (800 - SliderVal[0]));
                returnValue[0] = 1;
                returnValue[2] = (returnValue[1] - 1);
            }
            else {
                returnValue[0] = parseFloat((800 - SliderVal[0]) / (SliderVal[0]));
                returnValue[1] = 1;
                returnValue[2] = (returnValue[0] * -1) + 1;
            }
        }
        else {
            if (SliderVal[1] > 1200) {
                returnValue[1] = parseFloat((SliderVal[1] - 800) / (1600 - (SliderVal[1])));
                returnValue[0] = 1;
                returnValue[2] = (returnValue[1] - 1);
            }
            else {
                //SliderVal[0] = parseFloat((800 - (SliderVal[0])) / (SliderVal[0]));
                returnValue[0] = parseFloat((1600 - SliderVal[1]) / (SliderVal[1] - 800));
                returnValue[1] = 1;
                returnValue[2] = (returnValue[0] * -1) + 1;
            }
        }
        return returnValue;
    }
    function setNoUiColor(fJudgmentExist, index, id) {
        if (fJudgmentExist) {
            $('#mainSlider' + index + ' .noUi-connect').css('background-color', '#0058a3');
            $('#mainSlider' + index + ' .graph-green-div').css('background-color', '#6aa84f');
            $('#gslider' + index + ' .noUi-connect').css('background-color', '#0058a3');
            $('#gslider' + index + ' .graph-green-div').css('background-color', '#6aa84f');
        }
        else {
            $('#mainSlider' + index + ' .noUi-connect').css('background-color', '#AAAAAA');
            $('#mainSlider' + index + ' .graph-green-div').css('background-color', '#AAAAAA');
            $('#gslider' + index + ' .noUi-connect').css('background-color', '#AAAAAA');
            $('#gslider' + index + ' .graph-green-div').css('background-color', '#AAAAAA');
                
        }

    }
    // ------------Pairwise Graphical--------------//

    // ------------Ratings--------------//
    $scope.ratings_dropdown = [];
    $scope.ratings_percentage = [];
    $scope.ratings_number_value = [];
    $scope.ratings_number_value_model = [];
    //$scope.ratings_direct_values = [];
    $scope.save_ratings = function (value, email, comment, index) {
       
        if (value == 10) { //direct
            $scope.ratings_dropdown[index] = $scope.display_user_dropdown("*" + value);
            $("#rating-input-box-" + index).focus();
            return false;
        }
        else {

            $scope.ratings_number_value[index] = $scope.display_user_rating(value);
            $scope.ratings_percentage[index] = $scope.display_user_rating(value) == "" ? 0 : ($scope.display_user_rating(value) * 100) + '%';

            $scope.ratings_dropdown[index] = $scope.display_user_dropdown(value);
            $scope.users_list[index][5] = value.toString();

            if (value == "" && value !=0 ) {
                value = -2147483648000;
            }

            update([value, email, comment], $scope.getStepGuid(), 'save', 'mtRatings', $scope.baseurl, $scope.users_list.length, email, index);

            
        }
          
    }


    $scope.set_current_user_ratings = function (measurement) {
        update([measurement, $scope.current_user[0], $scope.comment_txt[1]], $scope.getStepGuid(), 'save', 'mtRatings', $scope.baseurl, $scope.users_list.length, $scope.current_user[0], 0);
        $scope.ratings_number_value[0] = $scope.display_user_rating(measurement);
        $scope.ratings_dropdown[0] = $scope.display_user_dropdown(measurement);
        $scope.ratings_direct_values[0] = "";
    }

    $scope.mtRatings_direct_value = function (value, email, comment, index) {
        //update current user direct value
        var timOutMs = 1000;
        if ($scope.isMobile()) {
            timOutMs = timOutMs * 3;
        }

        $timeout(function () {
            if (value > 1) {
                value = 1;
            }
            if (value < 0 ) {
                value = 0;
            }

            //value = "*" + value;
            if ($scope.isMobile()) {
                value = "*" + $scope.ratings_number_value[index];
            } else {
                value = "*" + $scope.ratings_direct_values[index];
            }

            $scope.users_list[index][5] = value;
            $scope.ratings_percentage[index] = ($scope.display_user_rating(value) * 100) + '%';
            $scope.ratings_dropdown[index] = $scope.display_user_dropdown(value);
            $scope.ratings_number_value[index] = $scope.display_user_rating(value);

            update([value, email, comment], $scope.getStepGuid(), 'save', 'mtRatings', $scope.baseurl, $scope.users_list.length, email, index);
            $scope.hide_loading_on_same_judgment(value, comment);
        }, timOutMs);
    }

    $scope.display_user_rating = function (rating) {

        if (rating.toString().indexOf("*") >= 0) {
            rating = rating.replace("*", '');
        }
        else if (rating < 0) {
            rating = "";
            return rating;
        }
        else {
            //find ratings value in pipe measure
            for (i = 0; i < $scope.pipe_measurements.length ; i++) {
                if ($scope.pipe_measurements[i][0] == rating) {
                    rating = $scope.pipe_measurements[i][2];
                    return parseFloat(rating);
                }
            }
           // rating = $scope.pipe_measurements[-(rating - ($scope.pipe_measurements.length -3))][2];
        }
        return parseFloat(rating);
    }



    $scope.display_user_dropdown = function (rating) {
        if (rating.toString().indexOf("*") >= 0) {
            return $scope.pipe_measurements[$scope.pipe_measurements.length - 1][1];
        }
        else if (rating < 0) {
            return $scope.pipe_measurements[$scope.pipe_measurements.length - 2][1];
        }
        else {

            //find ratings dropdown in pipe measure
            for (i = 0; i < $scope.pipe_measurements.length ; i++) {
                if ($scope.pipe_measurements[i][0] == rating) {
                    return $scope.pipe_measurements[i][1];
                }
            }

            // rating = $scope.pipe_measurements[-(rating - ($scope.pipe_measurements.length -3))][1];
        }
    }

    //$scope.ratings_dropdown_init = function (index, value) {
    //    $scope.ratings_dropdown[index] = value;
    //}

    //console.log($scope.ratings_dropdown);
    // ------------Ratigns--------------//




    // ------------Direct Comparison--------------//
    $scope.direct = [];
    $scope.direct_sliders = [];
    $scope.getNouiDirect = function (index) {
        return document.getElementById("direct-dsBarClr" + index);
    }
    $scope.initializeNoUiDirect = function (model, index, user) {

        $scope.directTimeout = $scope.$evalAsync(function () {
            var slider = null;
            if (slider == null)
                slider = $scope.getNouiDirect(index);
            noUiSlider.create(slider, {
                start: [model],
                connect: 'lower',
                step: 0.01,
                range: {
                    'min': 0,
                    'max': 1
                }

            });

            if (slider && (user[4] < 0 || ($scope.current_user[0] !== $scope.meeting_owner && $scope.current_user[0] !== user[1].toLowerCase()))) {
                slider.setAttribute('disabled', true);
            }

            $('.noUi-handle').addClass('disabled');
            $('.noUi-handle').css('width', '20px').css('top', '-5px').css('height', '26px');
            $('.noUi-handle.noUi-handle-lower').css('left', '0px');
            // case 13008: for extra blue lines in slider
            $('#direct-dsBarClr' + index + ' .noUi-origin').css({
                'position': 'absolute',
                'top': '-1px',
                'height': '17.9px',
                'border': '1px solid #aeb0b2',
                'border-left': 'none',
                'border-right': 'none'
            });
            
            slider.noUiSlider.on('change', function (value, handle) {
                svalue = [parseFloat(value[0]), user[1], user[6]];
                $scope.direct[index] = parseFloat(value[0]);
                update(svalue, $scope.getStepGuid(), 'save', 'mtDirect', $scope.baseurl, $scope.users_list.length, $scope.current_user[1]);
            });
            slider.noUiSlider.on('update', function (value, handle) {
                $scope.driectSliderTimeout = $scope.$evalAsync(function () {
                    $scope.direct[index] = parseFloat(value[0]);
                });

            });
        });



    }
    $scope.reset_slider = function (index, user, comment) {
        $timeout(function () {
            var slider = $scope.getNouiDirect(index);
            slider.noUiSlider.set([0]);
            update(["-2147483648000", user, comment], $scope.getStepGuid(), "save", 'mtDirect', $scope.baseurl, $scope.users_list.length, $scope.current_user[1]);
        }, 100);
    }
    $scope.update_slider = function (event, index, user, comment, state) {
        //////////console.log(user);
        var code = event.which;
        if (state == "press") {
            if ((code >= 58 || code < 48 && code != 46) && code != 13)
                return event.preventDefault() && false;
        }
        else {
            
            $scope.directTimeout = $timeout(function () {
                var value = $scope.direct[index];
                if (value != "" && value != null && value <= 1 && value >= 0) {
                    var slider = $scope.getNouiDirect(index);
                    slider.noUiSlider.set([value]);
                    update([value, user, comment], $scope.getStepGuid(), "save", 'mtDirect', $scope.baseurl, $scope.users_list.length, $scope.current_user[1]);
                }
                else {
                    var slider = $scope.getNouiDirect(index);
                    slider.noUiSlider.set([0]);
                    update(["-2147483648000", user, comment], $scope.getStepGuid(), "save", 'mtDirect', $scope.baseurl, $scope.users_list.length, $scope.current_user[1]);
                }
                
                
            }, 1500);
        }
    }
    // ------------Direct Comparison--------------//

    // ------------Utility Function--------------//

    $scope.uc_input = [];
    $scope.uc_priority = [];
    $scope.uc_pwidth = [];
    $scope.uc_data = {};


    $scope.save_utility_curve = function (value, email, comment, index) {
        if (value < $scope.lowest_value) {
            value = -2147483648000;
        }

        if ($scope.current_user[0].toLowerCase() == email.toLowerCase()) {
            var temp_value = value == -2147483648000 ?  '' : value;
            $("#uCurveInput").val(temp_value);
        }

        $scope.calculate_uc_priority(value, index);
        update([value, email, comment], $scope.getStepGuid(), 'save', 'mtRegularUtilityCurve', $scope.baseurl, $scope.users_list.length, email, index);
    }

    $scope.calculate_uc_priority = function (value, index) {
        var res = "";   //"&#8212;";

        var XMin = $scope.stepfunction_scale[0];
        var XMax = $scope.stepfunction_scale[1];
        var Increasing = $scope.output.pipeData.Decreasing == "true" ? false : true;
        var Curvature = $scope.stepfunction_scale[2];

        var low = XMin;
        var high = XMax;
        var curv = Curvature;
        var val = value;

        if (low >= high) {
            res = 0;
        }
        else {
            var l = (high - low);
            var n = l * curv;
            if ((!Increasing))
            {
                if (val < low) res = 1;
                if (val > high) res = 0;
                if (val >= low && val <= high) {
                    res = (n == 0 ? (high - val) / l : (1 - Math.exp(-(high - val) / n)) / (1 - Math.exp(-l / n)));
                }
                       
            }
            else { // isIncreasing
                if (val < low) res = 0;
                if (val > high) res = 1;
                if (val >= low && val <= high) {
                    res = (n == 0 ? (val - low) / l : (1 - Math.exp(-(val - low) / n)) / (1 - Math.exp(-l / n)));
                }
            }
        }
        res = parseFloat(res);
        $scope.uc_priority[index] = res.toFixed(2);
        $scope.uc_pwidth[index] = { 'width': '' + ($scope.uc_priority[index] * 100) + '%' };
        if (val == -2147483648000) {
            $scope.uc_priority[index] = "0.00";
            $scope.uc_pwidth[index] = { 'width': '0%' };
        }
          

       
        

    }

    $scope.is_float = function (n) {
        return Number(n) === n && n % 1 !== 0;
    }

    $scope.UCFunction = function (x) {
        var XMin = $scope.stepfunction_scale[0];
        var XMax = $scope.stepfunction_scale[1];
        var Increasing = $scope.output.pipeData.Decreasing == "true" ? false : true;
        var Curvature = $scope.stepfunction_scale[2];
        if (XMin >= XMax) { return 0 };
        var n = new Number(0.0);
        if (!Increasing) {
            if (x < XMin) { return 1 };
            if (x > XMax) { return 0 };
            n = (XMax - XMin) * Curvature;
            if (n == 0) { return (XMax - x) / (XMax - XMin) }
            else { return (1 - Math.exp(-(XMax - x) / n)) / (1 - Math.exp(-(XMax - XMin) / n)) }
        }
        else {
            if (x < XMin) { return 0 };
            if (x > XMax) { return 1 };
            n = (XMax - XMin) * Curvature;
            if (n == 0) { return (x - XMin) / (XMax - XMin) }
            else { return (1 - Math.exp(-(x - XMin) / n)) / (1 - Math.exp(-(XMax - XMin) / n)) }
        }
    }

    $scope.render_UC_canvas = function () {
        

        var XMin = $scope.stepfunction_scale[0];
        var XMax = $scope.stepfunction_scale[1];
        var Increasing = $scope.output.pipeData.Decreasing == "true" ?  false : true;
        var Curvature = $scope.stepfunction_scale[2];
        
        var uc_points = [];

        var Shift = (XMax - XMin) / 10;

        for (var i = 0; i < 11; i++) {
            var x_point = (XMin + i * Shift);
            var y_point = $scope.UCFunction(XMin + i * Shift);
            //x.push( (XMin + i * Shift).toFixed(2) );
            //y.push( $scope.UCFunction(XMin + i * Shift) * 100 );
            uc_points.push({ x: x_point, y: y_point });
        };

        ////////////////console.log(uc_points);
        Chart.defaults.global.legend.display = false; //remove label at the top of graph
        var data = {
            datasets: [{
                label: "",
                borderColor: "#0058a3",
                fill: false,
                data: uc_points,
                pointBackgroundColor: "#ecfaff"
            }]
        };



        var options = {
            responsive: true,
            scales: {
                xAxes: [{
                    type: 'linear',
                    position: 'bottom',
                    scaleLabel: {
                        display: true,
                        labelString: 'Data'
                    },
                    ticks: {
                        callback: function (label, index, labels) {
                            if ($scope.is_float(label)) {
                                label = parseFloat(label).toFixed(1);
                            }
                            return label;
                        }
                    }
                }],
                yAxes: [{
                    scaleLabel: {
                        display: true,
                        labelString: 'Priority'
                    },
                    ticks: {
                        callback: function (label, index, labels) {
                            if ($scope.is_float(label)) {
                                label = parseFloat(label).toFixed(1);
                            }
                            return label;
                        }
                    }
                }]
            }
        };
       // console.log("Chart start");

        $timeout(function () {
            var canvas = document.getElementById("uCurve");
            var ctx = canvas.getContext("2d");

            $scope.chart = Chart.Line(ctx, {
                data: data,
                options: options
            });
           // console.log("Chart rendered");

        }, 1000);
       

      
    }

    //--END OF Utility--//

    // ------------Step Function--------------//
    $scope.render_step_function = function (min, max, steps) {
        $scope.xAxisLabels = [];
        $timeout(function () {
            var ctx = document.getElementById("sFunctionCanvas").getContext("2d");
            var overridenStep = (max - min) / 7.5;
            Chart.defaults.global.legend.display = false; //remove label at the top of graph
            $scope.stepChart = new Chart(ctx, {
                type: 'line',
                display: true,
                data: {
                    datasets: [{
                        label: $scope.output.child_node,
                        data: getUCStepData(min, max, steps)[0],
                        lineTension: 0,
                        borderColor: "#0058a3",
                        fill: false,
                        pointBackgroundColor: "#ecfaff"
                    }]
                },
                options: {
                    scales: {
                        xAxes: [{
                            type: 'linear',
                            position: 'bottom',
                            scaleLabel: {
                                display: true,
                                labelString: 'Data'
                            },
                            ticks: {
                                min: min,
                                max: max,
                                fontSize: 10,
                                fixedStepSize: overridenStep,
                                callback: function (label, index, labels) {
                                    $scope.xAxisLabels = labels;
                                    if ($scope.is_float(label)) {
                                        label = parseFloat(label).toFixed(1);
                                    }
                                    return label;
                                }

                            }
                        }],
                        yAxes: [{
                            scaleLabel: {
                                display: true,
                                labelString: 'Priority'
                            },
                            ticks: {
                                callback: function (label, index, labels) {
                                    if ($scope.is_float(label)) {
                                        label = parseFloat(label).toFixed(1);
                                    }
                                    return label;
                                }
                            }
                        }]
                    }
                }
            });
        }, 1500);
        
        //$timeout(function () {
        //    $scope.canvas_width = $scope.stepChart.chartArea.right - $scope.stepChart.chartArea.left - 10; //3 px for allowance to the right
        //    $scope.left_margin = $scope.stepChart.chartArea.left;
        //    $scope.canvas_height = $scope.stepChart.chart.height;
        //    $('#stepsFunctionSlider').width($scope.canvas_width - 32).css({ left: $scope.left_margin });
        //}, 200);

    }


    function getUCStepData(XMin, XMax, steps) {
        var data = [[]];
        var Shift = (XMax - XMin) / 100;
        data[0].push({ x: XMin, y: steps[0][4]});
        if ($scope.isPiecewise) {
            for (var i = 0; i <= steps.length - 1; i++) {
                data[0].push({ x: steps[i][2], y: steps[i][4] });

            };
        }
        else {
            for (var i = 0; i <= steps.length - 1; i++) {
                data[0].push({ x: steps[i][2], y: steps[i][4] });
                if (i == steps.length - 1)
                    data[0].push({ x: XMax, y: steps[i][4] });
                else {
                    data[0].push({ x: steps[i + 1][2], y: steps[i][4] });
                }
            };

        }
        data[0].push({ x: XMax, y: steps[steps.length - 1][4] });

        
        return data;
    }

    $scope.stepfunction_input = [];
    $scope.stepfunction_priority = [];
    $scope.stepfunction_pwidth = [];
    $scope.calculate_priority = function (value, index) {
        for (i = 0; i < $scope.stepfunction_scale.length; i++) {
            if (value < $scope.lowest_value) {
                $scope.stepfunction_priority[index] = '';
                $scope.stepfunction_pwidth[index] = { 'width': '' + 0 + '%' };
                break;
            }
            if (value < $scope.stepfunction_scale[0][2]) {
                $scope.stepfunction_priority[index] = ($scope.stepfunction_scale[i][4]).toFixed(2);
                $scope.stepfunction_pwidth[index] = { 'width': '' + ($scope.stepfunction_priority[index] * 100) + '%' };
                break;
            }
            if ($scope.isPiecewise) {
                if (value >= $scope.stepfunction_scale[i][2] && value < $scope.stepfunction_scale[i][3]) {

                    if (i == $scope.stepfunction_scale.length - 1) {
                        $scope.stepfunction_priority[index] = ($scope.stepfunction_scale[i][4]).toFixed(2);
                        $scope.stepfunction_pwidth[index] = { 'width': '' + ($scope.stepfunction_priority[index] * 100) + '%' };
                        break;
                    }
                    //equation = mvalue +(((val - low)/ (high - low)) * (next-mvalue - mvalue))
                    $scope.stepfunction_priority[index] = ($scope.stepfunction_scale[i][4] + (((value - $scope.stepfunction_scale[i][2]) / ($scope.stepfunction_scale[i][3] - $scope.stepfunction_scale[i][2])) * ($scope.stepfunction_scale[i + 1][4] - $scope.stepfunction_scale[i][4]))).toFixed(2);
                    $scope.stepfunction_pwidth[index] = { 'width': '' + ($scope.stepfunction_priority[index] * 100) + '%' };
                    break;
                }
            }
            else {
                if (value >= $scope.stepfunction_scale[i][2] && value < $scope.stepfunction_scale[i][3]) {
                    if (i == $scope.stepfunction_scale.length - 1) {
                        $scope.stepfunction_priority[index] = ($scope.stepfunction_scale[i][4]).toFixed(2);
                        $scope.stepfunction_pwidth[index] = { 'width': '' + ($scope.stepfunction_priority[index] * 100) + '%' };
                        break;
                    }
                    //equation = mvalue +(((val - low)/ (high - low)) * (next-mvalue - mvalue))
                    $scope.stepfunction_priority[index] = ($scope.stepfunction_scale[i][4]).toFixed(2);
                    $scope.stepfunction_pwidth[index] = { 'width': '' + ($scope.stepfunction_priority[index] * 100) + '%' };
                    break;
                }
            }
        }
    }

    //save judgment
    $scope.stepfunction_save = function (value, email, comment, index) {
        if (parseFloat(value)) {
            if (index == 0) {
                //$("#steps-functionInput").val(value);
                //$(".tt-steps-slider").slider("option", "value", value)
            }
            if (value < $scope.lowest_value) {
                value = -2147483648000;
            }
            if ($scope.current_user[0].toLowerCase() == email.toLowerCase()) {
                var temp_value = value == -2147483648000 ? '' : value;
                $("#steps-functionInput").val(temp_value);
            }

            $scope.calculate_priority(value, index);
            update([value, email, comment], $scope.getStepGuid(), 'save', 'mtStep', $scope.baseurl, $scope.users_list.length, email, index);
        }
        
    }

    //If you do any change in this function then you have to change accordingly on the same function in MainController.js
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

    //$scope.load_step_canvas = function () {


    //    if ($scope.user_judgment < $scope.lowest_value) {
    //        $('#steps-functionInput').val(0);
    //    } else {
    //        $('#steps-functionInput').val($scope.user_judgment);
    //    }
    //}
    // ------------ End for Step Function--------------//



    // ------------Comments--------------//
    $scope.split_comment = function (value, index) {
        if ($scope.no_judgment_page == false) {
            var comment = [];
            if (value.indexOf("@@") > -1) {
                comment = value.split("@@");
            }
            if ((comment.length <= 1 && index > 0)) {
                return "Recently";
            }
            return comment[index];
        }
    }
    $scope.comment_txt = ['', ''];
    $scope.send_comment = function (value) {
        $scope.comment_txt[0] = $scope.split_comment(value, 0);
        update($scope.user_judgment + '___' + value, $scope.getStepGuid(), 'comment', '', $scope.baseurl, $scope.users_list.length, $scope.current_user[0], $scope.current_user[1]);
        $scope.hide_loading_on_same_judgment($scope.user_judgment, value);
    }

    // ------------Comments--------------//


    // ------------OTHERS----------------//
    //this hides loading if it is same
    $scope.hide_loading_on_same_judgment = function (judgment, comment) {
        if (judgment == $scope.user_judgment && comment == $scope.comment_txt[1]) {
            hide_loading_icon();
        }
    }




    // ------------OTHERS----------------//

    $scope.tt_update = function (params, status, mtype, baseUrl, allusers, UserEmail, index) {
        //check if current user or teamtime owner
        if ($scope.current_user[0].toLowerCase() == $scope.meeting_owner.toLowerCase() || $scope.current_user[0] == UserEmail.toLowerCase()) {
            
            //check if view participant or restricted
            if (params[3] == -1 || params[3] == -2) {
                //do noting
                return false;
            }
            else {
                if (index && index >= 0 && mtype == "ptVerbal") {
                    $scope.users_list[index][5] = params[0];
                }

                params.pop();
                update(params, $scope.getStepGuid(), status, mtype, baseUrl, allusers, UserEmail, index);
            }
        }
    }

    $scope.get_user_color = function (permission) {
        if (permission == -1) {
            return "#999999";
        }
        if (permission == -2) {
            return "red";
        }
    }
    $scope.get_opacity = function (permission, current_user, user) {
        if ((permission == -1 || permission == -2)) 
            return "0.35";
        if (current_user || user) {
            if (current_user != $scope.meeting_owner && current_user != user[1].toLowerCase())
                return "0.35";
        }
    }
    $scope.get_left_color = function (permission, judgment) {
        if ((judgment < 0 && judgment >-9 && permission == -2)) {
            return "#999999";
        }
    }

    $scope.get_right_color = function (permission, judgment) {
        if ((judgment >=1 && judgment < 9 && permission == -2)) {
            return "#999999";
        }
    }

    $scope.showOptions = function () {
        $scope.$broadcast('showOptions');
    }
    

  

    $scope.resize_TT_list = function (dc) {
        var divClass = dc; //the DIV class to resize
        var windowHeight = $(window).height();
        var mobileHeaderHeight = $('.mobile-header').height();
        var footerHeight = $('.tt-footer-wrap').height();

        var questionHeader = $('.questionHeader').height();

        var infodocHeight = $('.questionsWrap').height();
        var judgmentHeight = $(".current-user-main-judgment").height();
        var groupResult = $(".teamtime-group-result-row").height();

        var newHeight = windowHeight - (mobileHeaderHeight + footerHeight + infodocHeight + judgmentHeight + questionHeader + groupResult) - 160; //200
        
        var maxHeight = (newHeight - footerHeight);

        if (maxHeight < 100) {
            maxHeight = 300;
        }

        if ($scope.current_action_type == "pairwise" && ($scope.current_mtype == "ptVerbal" || $scope.current_mtype == 1)) {
            try {
                if (typeof ($(".participants-list").get(0).scrollHeight) != "undefined") {
                    if ($(".participants-list").get(0).scrollHeight > $(".participants-list").height()) {
                        $(".participants-list").addClass("pw-verbal-auto-height-scroll-fixer");
                        $('.' + divClass).css({
                            "overflow-y": "auto",
                            "overflow-x": "hidden",
                            "max-height": maxHeight + "px" //30 allowance                          
                        });
                    }
                    else {
                        $(".participants-list").removeClass("pw-verbal-auto-height-scroll-fixer");
                    }
                }
               
            }
            catch (e) {

            }
            

            $('.' + divClass).css({
                "overflow-y": "auto",
                "overflow-x": "hidden",
                "max-height": maxHeight + "px" //30 allowance
            });
           
        }
        else {
            $('.' + divClass).css({
                "overflow-y": "auto",
                "overflow-x": "hidden",
                "max-height": maxHeight + "px" //30 allowance
            });
        }
      

       
    }

    $scope.round_off = function (num) {
        return $window.Math.round(num * 100) / 100;
    }

    $scope.number_ceiling = function (num) {
        return $window.Math.ceil(num);
    }

    $scope.number_floor = function (num) {
        return $window.Math.floor(num);
    }

    $scope.number_postive = function (num) {
        return $window.Math.abs(num);
    }
});

//********************** DIRECTIVES **********************//


app.directive('slider', function() {
    return {
        scope: {
            ngModel: '='
        },
        link: function (scope, elem, attrs) {
            scope.$watch('ngModel', function (value) {
                scope.ngModel = value;
                return $(elem).slider({
                    range: "min",
                    animate: true,
                    value: attrs.value,
                    min: 0,
                    max: 1,
                    step: 0.01,
                    disabled: attrs.disable == 'true',
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
                            scope.$parent.update([ui.value, attrs.user, attrs.comment], "save", 'mtDirect', scope.$parent.baseurl, attrs.user_list, attrs.cur_user);
                        });
                    }
                });
            });
        }
    };
});
//var handlers = [400, 1000, 1600];
//var colors = ["#c9c9c9", "#539ddd", "#6aa84f", "#c9c9c9"];

app.directive('participantSlider', function () {
    return {
        scope: {
            ngModel: '='
        },
        link: function (scope, elem, attrs) {
            //updateColors(handlers, attrs.index);
            //var $leftVal = $('.content-lefts input[type=text]');
            //var $rightVal = $('.content-right input[type=text]');
            updateTextPosition(attrs.index);
            return $(elem).slider({
                animate: true,
                orientation: "horizontal",
                range: 'min',
                min: 0,
                max: 1600,
                value: scope.ngModel,
                step: 1,
                disabled: attrs.disable === "true",
                create: function (event, ui) {
                    if (parseInt(attrs.judgment) < scope.$parent.lowest_value) {
                            $('.g-slider' + attrs.index + ' .graphicalSlider .ui-slider-range').css('background', '#c9c9c9');
                            $('.g-slider' + attrs.index + ' .graphicalSlider ').css('background', '#c9c9c9');  
                    }

                    if (attrs.permission == -2) {
                        $('.g-slider' + attrs.index + ' .graphicalSlider .ui-slider-range').css('width', '50%');
                        if (attrs.judgment > 1) {
                            $('.g-slider' + attrs.index + ' .graphicalSlider .ui-slider-range').css('background', '#c9c9c9');
                            $('.g-slider' + attrs.index + ' .graphicalSlider ').css('background', 'black');
                        }
                        else if (attrs.judgment < 1) {
                            $('.g-slider' + attrs.index + ' .graphicalSlider .ui-slider-range').css('background', 'black');
                            $('.g-slider' + attrs.index + ' .graphicalSlider ').css('background', '#c9c9c9');
                        }
                        else {
                            $('.g-slider' + attrs.index + ' .graphicalSlider .ui-slider-range').css('background', '#c9c9c9');
                            $('.g-slider' + attrs.index + ' .graphicalSlider ').css('background', '#c9c9c9');
                        }

                        $('.g-slider' + attrs.index + ' .graphicalSlider .ui-slider-handle ').css('left', '50%');  
                    }
                },

                slide: function (event, ui) {
                    if (attrs.permission != 0) {
                        return false;
                    }
                    var result = calculate_pie_and_slider('blue', ui.value, true);
                    if (scope.ngModel != undefined) {
                        return scope.$apply(function () {
                            //updateTextPosition(attrs.index);
                                $('.g-slider' + attrs.index + ' .graphicalSlider ').css('background', '#6aa84f');
                                $('.g-slider' + attrs.index + ' .graphicalSlider .ui-slider-range').css('background', '#539ddd');

                                

                            //var ss = parseInt($('.g-slider' + attrs.index + ' .graphicalSlider .ui-slider-handle').css('left'), 10);
                            //$('.content-participant' + attrs.index + ' .left-input-text').css('left', ss - 100 + 'px');
                            //$('.content-participant' + attrs.index + ' .right-input-text').css('left', ss - 10 + 'px');
                                $('.g-slider' + attrs.index + ' .graphicalSlider ').slider('value', ui.value);
                            if (ui.value <= 800) {
                                if (result[0][1] > 9) { set_sliders(1, 9, attrs.index, 160); return false; }
                                scope.$parent.left_input[attrs.index] = result[0][0];
                                scope.$parent.right_input[attrs.index] = result[0][1];
                            }
                            else {
                                if (result[0][0] > 9) { set_sliders(9, 1, attrs.index, 1440); return false; }
                                scope.$parent.left_input[attrs.index] = result[0][0];
                                scope.$parent.right_input[attrs.index] = result[0][1];
                            }
                            scope.ngModel = ui.value;
                        });
                    }
                else {
                        $('.g-slider' + attrs.index + ' .graphicalSlider ').slider('value', 800);
                    }
                },
                change: function (event, ui) {
                    //return scope.$apply(function () {

                    //        //updateTextPosition(attrs.index);
                    //        //scope.ngModel = ui.value;
                    //    });
                },
                stop: function (event, ui) {
                    if (attrs.permission != 0) {
                        return false;
                    }
                    return scope.$apply(function () {
                        var result = calculate_pie_and_slider('blue', ui.value, true);
                        //updateTextPosition(attrs.index);
                        var value = [(parseFloat(result[0][0]) - parseFloat(result[0][1])) * -1, attrs.user, attrs.comment];
                        scope.$parent.update(value, 'save', 'ptGraphical', scope.$parent.baseurl, scope.$parent.users_list.length, scope.$parent.current_user[0]);
                    });
                }
            });

            function set_sliders(val1, val2, index, slider_val) {
                $('.g-slider' + index + ' .graphicalSlider ').slider('value', slider_val);
                scope.$parent.participant_slider[index] = slider_val;
                scope.$parent.left_input[attrs.index] = val1;
                scope.$parent.right_input[attrs.index] = val2;
            }



        }
    };
});


app.directive('graphSlider', function () {
    return {
        scope: {
            ngModel: '='
        },
        link: function (scope, elem, attrs) {
            return $(elem).slider({
                orientation: "horizontal",
                range: "min",
                min: 0,
                max: 1600,
                value: scope.ngModel,
                step: 1,
                disabled: attrs.disable == 'true',
                create: function(event, ui){
                    $('.a_Slider .ui-slider-range').css('background', '#539ddd');
                    $('.b_Slider .ui-slider-range').css('background', '#6aa84f');
                },
                slide: function (event, ui) {
                    if (attrs.permission != 0) {
                        return false;
                    }
                    //code from app.js
                    info_docs_resize_event = false;
                    var result = calculate_pie_and_slider(attrs.color, ui.value);
                    //updateFrontEnd($b, $a); // then update the user's view

                    return scope.$apply(function () {
                        if (attrs.color == 'blue') {
                            scope.$parent.graphical_slider[1] = result[1];
                            $('#dsBarClr2').slider('value', scope.$parent.graphical_slider[1]);
                            if (ui.value <= 800) {
                                if (result[0][1] > 9) { set_sliders(1, 9, 2, 1); return false; }
                                scope.$parent.main_bar_txt[0] = result[0][0];
                                scope.$parent.main_bar_txt[1] = result[0][1];
                                //////////console.log(result[0]);
                            }
                            else {
                                if (result[0][0] > 9) { set_sliders(9, 1, 1, 2); return false; }
                                scope.$parent.main_bar_txt[0] = result[0][0];
                                scope.$parent.main_bar_txt[1] = parseFloat(result[0][1]);
                                //////////console.log(result[0]);
                            }
                        }
                        else if (attrs.color == 'green') {
                            scope.$parent.graphical_slider[0] = result[1];

                            $('#dsBarClr1').slider('value', scope.$parent.graphical_slider[0]);
                            if (ui.value <= 800) {
                                if (result[0][1] > 9) { set_sliders(1, 9, 1, 2); return false; }
                                scope.$parent.main_bar_txt[0] = result[0][1];
                                scope.$parent.main_bar_txt[1] = result[0][0];
                            }
                            else {
                                if (result[0][0] > 9) { set_sliders(9, 1, 2, 1); return false; }
                                scope.$parent.main_bar_txt[0] = result[0][1];
                                scope.$parent.main_bar_txt[1] = result[0][0];
                            }
                        }
                        $('.a_Slider .ui-slider-range').css('background', '#539ddd');
                        $('.b_Slider .ui-slider-range').css('background', '#6aa84f');
                        scope.$parent.main_bar[0] = parseFloat(scope.$parent.main_bar_txt[0]);
                        scope.$parent.main_bar[1] = parseFloat(scope.$parent.main_bar_txt[1]);
                        scope.ngModel = ui.value;
                    });
                },
                change: function (event, ui) {
                    if (attrs.permission != 0) {
                        return false;
                    }
                    $('.a_Slider .ui-slider-range').css('background', '#539ddd');
                    $('.b_Slider .ui-slider-range').css('background', '#6aa84f');
                },
                stop: function (event, ui) {
                    if (attrs.permission != 0) {
                        return false;
                    }
                    var result = calculate_pie_and_slider(attrs.color, ui.value);
                    return scope.$apply(function () {
                        var value= [];
                        if (attrs.color == 'blue') {
                            value = [(parseFloat(result[0][0]) - parseFloat(result[0][1])) * -1, attrs.user, attrs.comment];
                        }
                        if (attrs.color == 'green') {
                            value = [parseFloat(result[0][0]) - parseFloat(result[0][1]) * 1, attrs.user, attrs.comment];
                        }
                        //////////console.log(value[0]);
                        scope.$parent.update(value, "save", 'ptGraphical', scope.$parent.baseurl, attrs.user_list, attrs.cur_user);
                    });

                }
            });
            function set_sliders(val1, val2, index1, index2) {
                if (attrs.permission != 0) {
                    return false;
                }
                $('#dsBarClr' + index1).slider('value', 1440);
                $('#dsBarClr' + index2).slider('value', 160);
                scope.$parent.main_bar_txt[0] = val1;
                scope.$parent.main_bar_txt[1] = val2;
                scope.$parent.main_bar[0] = parseFloat(scope.$parent.main_bar_txt[0]);
                scope.$parent.main_bar[1] = parseFloat(scope.$parent.main_bar_txt[1]);
            }      
        }
    };
});

//********************** DIRECTIVES **********************//

//************ Angular JS Codes ****************

//functions
function updateTextPosition(index) {
    $('.g-slider' + index + ' .graphicalSlider ').css('background', '#6aa84f');
    $('.g-slider' + index + ' .graphicalSlider .ui-slider-range').css('background', '#539ddd');
    var ss = parseInt($('.g-slider' + index + ' .graphicalSlider .ui-slider-handle').css('left'), 10);
//    $('.content-participant' + index + ' .left-input-text').css('left', ss - 100 + 'px');
//    $('.content-participant' + index + ' .right-input-text').css('left', ss - 10 + 'px');
}
//function updateColors(values, index) {
//    var colorstops = colors[0] + ", "; // start left with the first color
//    for (var i = 0; i < values.length; i++) {
//        colorstops += colors[i] + " " + (values[i] / 2000) * 100 + "%,";
//        colorstops += colors[i + 1] + " " + (values[i] / 2000) * 100 + "%,";
//    }
//    // end with the last color to the right
//    colorstops += colors[colors.length - 1];

//    /* Safari 5.1, Chrome 10+ */
//    var css = '-webkit-linear-gradient(left,' + colorstops + ')';
//    $('.g-slider' + index + ' .graphicalSlider').css('background-image', css);
//}

function calculate_pie_and_slider(color, ui, is_false) {
    var $a; var $b;
    var a; var b;
    var areal; var breal;
    var total; var sliderval;
    var ratiovalue = [];
    if (color == 'blue') {
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
        if (!is_false) {
            if(ui <= 1440 && ui >=160)
                reloadPizza(ratiovalue[1] / total, ratiovalue[0] / total);
        }
        sliderval = 1600 * (1 - ($a / 1600));
    }
    if (color == 'green') {
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
        if (!is_false) {
            if (ui <= 1440 && ui >= 160)
                reloadPizza(ratiovalue[0] / total, ratiovalue[1] / total);
        }
        sliderval = 1600 * (1 - ($a / 1600));

    }
    positionBarInputs($a);

    return [ratiovalue, sliderval];
}

app.directive('myRepeatDirective', function() {
    return function(scope, element, attrs) {
        //if (scope.$last) {
        //    setTimeout(function () {
        //        var counter = scope.$parent.init_counter(); 
        //        if (counter == 0) {
        //            scope.$parent.init_ratings();
        //        }
        //    }, 1000);
           
        //}
    };
})