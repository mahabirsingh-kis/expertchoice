<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="404.aspx.cs" Inherits="AnytimeComparion._404" %>

<html lang="en">

<head><meta charset="UTF-8" /><title>
	404 Page not found
</title><link rel="shortcut icon" href="favicon.ico" type="image/x-icon" /><link href="/Content/stylesheets/style.css" rel="stylesheet" />
<link href="/Content/stylesheets/app.css" rel="stylesheet"/>
<link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.min.css" />
<script src="/Content/bower_components/modernizr/modernizr.js"></script>
<script src="/Scripts/jquery-1.8.2.min.js"></script>
<script src="/Scripts/angular.min.js"></script>
<script src="/Scripts/ng-infinite-scroll.js"></script>
<script src="/Content/bower_components/jquery.cookie/jquery.cookie.js"></script>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <script>
        var app = angular.module('comparion', ['ngSanitize', 'ngTouch', 'ngclipboard', 'infinite-scroll']);
    </script>
</head>
    <body>
    <form id="form1" runat="server">
    <div class="the-error-page">
        <div class="text-center" ng-click="show_offset()">
            <p><a href="<%= ResolveUrl("~/") %>"><asp:Image ID="Image1" runat="server" ImageUrl='/Images/logos/teamtime-logo-transparent-compressed.png'></asp:Image></a></p>
            <h1>404 PAGE NOT FOUND</h1>
            <p>We can't seem to find the page you're looking for. Please check your URL or return to the <strong><a href="<%= ResolveUrl("~/") %>">Home Page</a></strong>.</p>
         </div>   
    </div>
    </form>
</body>
</html>
