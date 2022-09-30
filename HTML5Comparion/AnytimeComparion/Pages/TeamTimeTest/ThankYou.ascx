<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ThankYou.ascx.cs" Inherits="AnytimeComparion.Pages.TeamTimeTest.ThankYou" %>



<!--<div class="    tt-stick-that-footer-wrap tt-stick-that-footer-content">-->
    <div class="columns tt-auto-resize-content">
        <div class="row">
            <div class="large-12 columns tt-homepage">

                <div id="thankyouText" class="tt-center-height-wrap comparion-text tt-stick-that-footer-content large-12 columns thankyouText" ng-bind-html="getHtml(InformationMessage)">
<%--                    <div class="text-center tt-center-content-wrap">
                        <h1>We are done!</h1>
                        <p>
                            Thank you for providing your input.
                        </p>
                    </div>--%>
                </div>
            </div>
        </div>
    </div>
<!--</div>-->


    <script>
        $(".thankyouText p").attr("style", $("body").attr("style"));
        $(".thankyouText").attr("style", $("body").attr("style"));
    </script>