/// <reference path="~/Scripts/angular.js" />
/// <reference path="~/Scripts/angular-route.js" />
/// <reference path="~/Scripts/angular-resource.js" />
/// <reference path="~/Scripts/angular-sanitize.js" />
/// <reference path="~/Scripts/jquery-1.8.2.js" />
/// <reference path="MainController.js" />
/// <reference path="../app.js" />

var test = angular.module('comparion', ['ngSanitize', 'ngTouch', 'ngclipboard', 'infinite-scroll']);

test.controller("DefaultBrowserController",
    function ($scope, $http, $timeout, $window, $interval, $anchorScroll, $location) {

    $scope.copy_responsive_link = function () {
        alert("Successfully copied to clipboard!");
        return false;
    }

    $scope.continue_in_browser = function () {
        $http({
            method: "POST",
            url: baseUrl + "Default.aspx/setAllowIESession",
            data:{

            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8'
                }
        }).then(function success(response) {
            try {
                window.location.href = response.data.d;
            }
            catch (e) {

            }
        }, function error(response) {
            //console.log(response);
        });
    }

});