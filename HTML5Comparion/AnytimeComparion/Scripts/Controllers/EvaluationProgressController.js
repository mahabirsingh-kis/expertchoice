//app.run(function () {
//    FastClick.attach(document.body);
//});

app.controller("EvaluationProgressController", function($scope, $http, $timeout, $window, $interval, $anchorScroll, $location) {
    
    $scope.get_evaluation_progress_data = function() {
        $http({
            method: "POST",
            url: baseUrl + "pages/evaluation-progress.aspx/GetEvaluationProgressData",
            data: {
                
            }
        }).then(function success(response) {
            try {
                $scope.output = JSON.parse(response.data.d);
            }
            catch(e)
            {
                $scope.output = null;
            }
           
            console.log($scope.output);
        }, function error(response) {
            console.log(response);
        });
    }

    $scope.eval_link = "";
    $scope.open_view_link_modal = function (evalLink, isEnabled) {

        if (typeof (isEnabled) != "undefined") {
            if (!isEnabled) {
                return false;
            }
        }
        angular.element("#evaluators_link").html(evalLink);
        angular.element("#evaluators_copy_link_btn").attr("data-clipboard-text", evalLink);
        $scope.eval_link = evalLink;
        $("#viewLinkModal").foundation("reveal", "open");
    }

    $scope.login_as_participant = function (evalLink, isEnabled) {
        if (typeof (isEnabled) != "undefined") {
            if (!isEnabled) {
                return false;
            }
        }
        show_loading_modal();
        window.location.href = evalLink;
    }

    $scope.view_anytime_readonly = function(user_id) {
        show_loading_modal();
        $http.post(
            $scope.baseurl + "default.aspx/StartAnytime",
            JSON.stringify({
                projID: $scope.$parent.current_project.project_id
            }))
        .then(function success(response) {
            if (response.data.d) {
                hide_loading_modal();
                window.location.href = $scope.baseurl + "pages/Anytime/Anytime.aspx?readonly=1&id=" + user_id;
            }
            else
                $(document).ready(function () { $('#ErrorMessage').foundation('reveal', 'open'); });
            hide_loading_modal();
        });
    }


});