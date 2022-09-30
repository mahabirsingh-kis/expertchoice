<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InformationPage.ascx.cs" Inherits="AnytimeComparion.Pages.TeamTimeTest.InformationPage" %>
 
     <div class="columns tt-auto-resize-content">
        <div class="row">
            <div class="large-12 columns tt-homepage">
                <div id="informationText" class="tt-center-height-wrap comparion-text tt-stick-that-footer-content large-12 columns informationText" ng-bind-html="getHtml(InformationMessage)">
                        
                </div>
            </div>
        </div>
    </div>
 <script>
     $(".informationText p").attr("style", $("body").attr("style"));
    $(".informationText").attr("style", $("body").attr("style"));
 </script>
