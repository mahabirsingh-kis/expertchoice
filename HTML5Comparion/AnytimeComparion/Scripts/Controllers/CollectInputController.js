/// <reference path="~/Scripts/angular.js" />
/// <reference path="~/Scripts/angular-route.js" />
/// <reference path="~/Scripts/angular-resource.js" />
/// <reference path="~/Scripts/angular-sanitize.js" />
/// <reference path="~/Scripts/jquery-1.8.2.js" />
/// <reference path="MainController.js" />
/// <reference path="../app.js" />

app.controller("CollectInputController",
    function ($scope, $http, $timeout, $window, $interval, $anchorScroll, $location) {

        $scope.collectInputPages = {
            defaultPage: { name: "Collect Input", url: "pages/CollectInput/default.html" },
            datagrid: { name: "Data Grid", url: "pages/CollectInput/Datagrid.html" },
            objectivesMeasurement: { name: "Objectives Measurement", url: "pages/CollectInput/ObjectivesMeasurement.html" },
            alternativesMeasurement: { name: "Alternatives Measurement", url: "pages/CollectInput/AlternativesMeasurement.html" },
            scheduleMeeting: { name: "Schedule Meeting", url: "pages/CollectInput/ScheduleMeeting.html" },
            displaySettings: { name: "Display Settings", url: "pages/CollectInput/DisplaySettings.html" },
            selectQuestionnaire: { name: "Select Questionnaire", url: "pages/CollectInput/SelectQuestionnaire.html" },
            inviteParticipant: { name: "Invite Participant", url: "pages/CollectInput/InviteParticipant.html" }
        };

        $scope.currentPage = $scope.currentPage ? $scope.currentPage : $scope.collectInputPages.defaultPage;

        $scope.changePage = function (page) {
            $scope.currentPage = page;
        }



    });