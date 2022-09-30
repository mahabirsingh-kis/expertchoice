/// <reference path="~/Scripts/angular.js" />
/// <reference path="~/Scripts/angular-route.js" />
/// <reference path="~/Scripts/angular.min.js" />
/// <reference path="~/Scripts/angular.resource.js" />
/// <reference path="~/Scripts/jquery-1.8.2.js" />

//************ Angular JS Codes ****************

var app = angular.module('comparion', []);


    

app.controller("TeamTimeController", function ($scope, $interval) {
    //*************** ANGULAR TIMER *******************
    $scope.update_users_list = function () {

        var list = $scope.users_list.length;
        var Temp_List = TT_users_list;


        if (list != TT_users_list.length && list != null) {
            // this moves the current user to the top of the participant list


            $scope.show_paginated_participant = [];
            $scope.show_participant = show_participant_list;
            $scope.pagination_CurrentPage = pagination_CurrentPage;
            $scope.pagination_NoOfUsers = pagination_NoOfUsers;

            $scope.STEPS_current_step = steps_current_step;
            $scope.STEPS_total_steps = list_of_steps;

            for (i = 0; i < Temp_List.length; i++) {
                if (Temp_List[i][1] == $scope.current_user) {
                    var index = Temp_List[i];
                    Temp_List.splice(i, 1);
                    Temp_List.splice(0, 0, index);
                }
                if ($scope.pagination_CurrentPage == 'all') {
                    $scope.show_paginated_participant.push(true);
                }
                else {
                    if ($scope.pagination_CurrentPage == Math.ceil((i + 1) / $scope.pagination_NoOfUsers)) {
                        $scope.show_paginated_participant.push(true);
                    }
                    else {
                        $scope.show_paginated_participant.push(false);
                    }
                }



            }
            // this moves the current user to the top of the participant list

            $scope.pagination_select_list = [];
            for (i = 1; i <= Math.floor(Temp_List.length / 5) ; i++) {
                $scope.pagination_select_list.push(i * 5);
            }
            $scope.users_list = Temp_List;
            jdata = "";
            $scope.pagination_pages = pagination_details();

        }
    }
    //*************** ANGULAR TIMER *******************

    $scope.baseurl = baseUrl;
    // ---- USER'S EVALUATION
    $scope.users_evaluation = ['sample1', 'sample2', 'sample3'];

    $scope.view_evaluation = function () {
        $scope.users_evaluation = users_eval_progress;
    }
    // ---- USER'S EVALUATION

    // ---- PARTICIPANT LIST
            
    $scope.users_list = ['sample1', 'sample2', 'sample3'];
    $scope.current_user = current_user;
    $scope.meeting_owner = meeting_owner;
    $scope.update = function (judgment, request, mtype, baseurl, user_list, user_email, index) {
        if (request == 'individual') {

            if ($scope.show_participant == true) {
                $scope.show_participant = false;
            }
            else {
                $scope.show_participant = true;
            }
            update($scope.show_participant ? 1 : 0, request, mtype, baseurl);
        }
        else {
            update(judgment, request, mtype, baseurl, user_list, user_email, index);
        }
        
    };

    $interval(function () { $scope.update_users_list(); }, 1000);
    // ---- PARTICIPANT LIST

    // ---- PAGINATION
    $scope.pagination_save = function (request, value) {
        if (request == 'click'){
            $scope.pagination_CurrentPage = value;
            show_paginated_participant(false);
            update($scope.pagination_CurrentPage + '_' + $scope.pagination_NoOfUsers, 'pagination', '', $scope.baseurl);
        }
            
        if (request == 'option'){
            $scope.pagination_CurrentPage = 1;
            $scope.pagination_NoOfUsers = value;
            show_paginated_participant(false);
            update($scope.pagination_CurrentPage + '_' + $scope.pagination_NoOfUsers, 'pagination', '', $scope.baseurl);
        }

        if (request == 'all') {
            $scope.pagination_CurrentPage = value;
            show_paginated_participant(true);
            update(0 + '_' + $scope.pagination_NoOfUsers, 'pagination', '', $scope.baseurl);
        }

        if (request == 'increment') {
            if (!(value > Math.ceil($scope.users_list.length / $scope.pagination_NoOfUsers))) {
                $scope.pagination_CurrentPage = value;
                show_paginated_participant(false);
            }
        }

        if (request == 'decrement') {
            if (!(value < 1))
            {
                $scope.pagination_CurrentPage = value;
                show_paginated_participant(false);
            }
            
        }
        $scope.pagination_pages = pagination_details();
    }

    $scope.pagination_CurrentPage = pagination_CurrentPage;
    $scope.pagination_NoOfUsers = pagination_NoOfUsers;


    $scope.check = function (index) {
        return $scope.show_participant && $scope.show_paginated_participant[index];
    }
    function pagination_details()
    {
        var pages = Math.ceil($scope.users_list.length / $scope.pagination_NoOfUsers);
        var pagination_list = [];
        pagination_list.push('<');
        for (var i = 1; i <= pages; i++) {
            pagination_list.push(i)
        }
        pagination_list.push('select');
        pagination_list.push('all');
        pagination_list.push('>');
      
        return pagination_list;
    }


    // ---- PAGINATION

    function show_paginated_participant(is_all) {
        if (!is_all) {
            for (i = 0; i < $scope.users_list.length; i++) {
                if ($scope.pagination_CurrentPage == Math.ceil((i + 1) / $scope.pagination_NoOfUsers)) {
                    $scope.show_paginated_participant[i] = true;
                }
                else {
                    $scope.show_paginated_participant[i] = false;
                }
            }
        }
        else {
            for (i = 0; i < $scope.users_list.length; i++) {
                $scope.show_paginated_participant[i] = true;
            }
        }
    }

    //************ STEPS LIST MODAL------------
 
    
    $scope.STEPS_get_steps = function () {

        $scope.STEPS_pipe_steps = steps;
    }

    // ------------STEPS LIST MODAL************>
});
//************ Angular JS Codes ****************
