/// <reference path="~/Scripts/angular.js" />
/// <reference path="~/Scripts/angular-route.js" />
/// <reference path="~/Scripts/angular-resource.js" />
/// <reference path="~/Scripts/angular-sanitize.js" />
/// <reference path="~/Scripts/jquery-1.8.2.js" />
/// <reference path="MainController.js" />
/// <reference path="../app.js" />

app.controller("SynthesizeController",
    function ($scope, $http, $timeout, $window, $interval, $anchorScroll, $location) {

        $scope.synthesizePages = {
            defaultPage: "pages/Synthesize/default.html",
            SAObjective: "pages/Synthesize/Synthesize-objectives.html"
        };

        $scope.currentPage = $scope.currentPage ? $scope.currentPage : $scope.synthesizePages.defaultPage;

        $scope.changePage = function(page) {
            $scope.currentPage = page;
        }
        //Sensitivty Objectives



    });

