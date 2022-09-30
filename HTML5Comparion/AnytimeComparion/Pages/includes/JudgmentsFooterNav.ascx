<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="JudgmentsFooterNav.ascx.cs" Inherits="AnytimeComparion.Pages.includes.JudgmentsFooterNav" %>

<% var doThis = Request.QueryString["doThis"];  %>
<% var view = Request.QueryString["view"];  %>
<% var style = Request.QueryString["style"];  %>
<% var design = Request.QueryString["design"];  %>
<% var actions = Request.QueryString["actions"];  %>

<div class="columns">
    <div class="large-5 medium-7 columns">

        <div class="large-12 columns">

            <% if (view == "anytime" || view == "teamtime" && actions != "evaluate")
               { %>
            <button type="button" class="button tiny tt-button-icond-left tt-button-primary left" data-reveal-id="tt-h-modal">
                <span class="icon-tt-site-map icon"></span>
                <span class="text">Hierarchy</span>
            </button>


            <button type="button" class="button tiny tt-button-icond-left tt-button-primary left tt-steps-btn" data-reveal-id="tt-s-modal">
                <span class="icon-tt-th-list icon"></span>
                <span class="text">Steps 0/40</span>
            </button>


            <% } %>

            <% if (view == "teamtime" && actions == "evaluate")
               { %>
            <div class="left tt-eval-steps tt-steps-btn">
                <span><strong>Steps:</strong> 7 of 53 </span><span><strong>Evaluated:</strong> 6/38</span>
            </div>

            <% } %>
            <div class="tt-c100 p19 size-30">
                <span>19%</span>
                <div class="slice">
                    <div class="bar"></div>
                    <div class="fill"></div>
                </div>
            </div>
        </div>

    </div>

    <div class="large-7 medium-5 columns tt-pagination text-right">
        <% if (view == "anytime" || view == "teamtime" && actions != "evaluate")
           { %>
        <ul class="pagination">
            <li class="arrow unavailable"><a href="">&laquo;</a></li>
            <li class="current"><a href="">1</a></li>
            <li><a href="">2</a></li>
            <li><a href="">3</a></li>
            <li class="unavailable"><a href="">&hellip;</a></li>
            <li><a href="">12</a></li>
            <li><a href="">13</a></li>
            <li class="arrow"><a href="">&raquo;</a></li>
        </ul>
        <% } %>
    </div>

    

    

</div>