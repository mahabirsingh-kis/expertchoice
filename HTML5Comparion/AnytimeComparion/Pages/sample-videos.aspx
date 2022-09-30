<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="sample-videos.aspx.cs" Inherits="AnytimeComparion.Pages.sample_videos" %>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="tt-body">
        <div class="row collapse">
            <div class="large-12 columns">
                <div class="tt-header-nav blue-header columns">
                    <ul>
                        <li>
                            <a runat="server" href="~/Resources"><span class="icon-tt-chevron-left icon"></span></a>
                        </li>
                        <li class="end">
                            <span class="text">Sample Project Videos</span>
                        </li>
                        <li></li>
                    </ul>
                </div>
                <div class="columns">
                    <div class="large-12 columns">
                        <div class="large-6 columns">
                            <p>We suggest that you watch some of the sample project videos below. Your workspace already contains a sample model for each of these videos. </p>
                        </div>
                        <div class="large-6 columns">
                            <h3>Tips for watching videos:</h3>
                            <p>Want to test all the features right away? Open the Sample Project that in your workspace, and follow along with the matching video. Goto: <a href="">test link</a></p>
                        </div>
                    </div>

                    <div class="columns tt-all-video-container">
                        <div class="medium-4 columns tt-video-wrap">
                            <div class="medium-6 columns">
                                <iframe width="100%" height="100%" src="https://www.youtube.com/embed/RzoDdms-6jc" frameborder="0" allowfullscreen></iframe>
                            </div>
                            <div class="medium-6 columns tt-video-desc">
                                <h3><a href="#" data-reveal-id="SimpleCarExample" data-tooltip aria-haspopup="true" data-options="disable_for_touch:true" class="has-tip text" title="View full details">Simple Car Example</a></h3>
                                <p>Illustrates opening a project, examining objectives and alternatives, Examining Result, and Sensitivity Analysis</p>
                            </div>

                            <div id="SimpleCarExample" class="reveal-modal large tt-modal-wrap" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                                <h2 id="modalTitle" class="tt-modal-header blue-header">Simple Car Example</h2>
                                <div class="large-12 columns tt-modal-content">
                                    <div class="large-8 columns">
                                        <iframe width="100%" height="500px" src="https://www.youtube.com/embed/RzoDdms-6jc" frameborder="0" allowfullscreen></iframe>
                                    </div>
                                    <div class="large-4 columns">
                                        <h3>Simple Car Project</h3>

                                        <p>
                                            <span class="bold">Project Name</span><br>
                                            sample_01_carpurchasesimpleexample
                                        </p>
                                        <p>
                                            <span class="bold">Description</span><br>
                                            Seelcting a Car
                                        </p>
                                        <p>
                                            <span class="bold">Illustrates</span><br>
                                            Illustrates opening a project, examining objectives and alternatives, Examining Result, and Sensitivity Analysis
                                        </p>
                                    </div>
                                </div>

                                <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                            </div>
                        </div>

                        <div class="medium-4 columns tt-video-wrap">
                            <div class="medium-6 columns">
                                <iframe width="100%" height="100%" src="https://www.youtube.com/embed/RzoDdms-6jc" frameborder="0" allowfullscreen></iframe>
                            </div>
                            <div class="medium-6 columns tt-video-desc">
                                <h3><a href="#">Pros and Cons</a></h3>
                                <p>Illustrates Brainstorming for alternatives, identifying pros and cons, and creating objectives from pros and cons</p>
                            </div>
                        </div>

                        <div class="medium-4 columns tt-video-wrap">
                            <div class="medium-6 columns">
                                <iframe width="100%" height="100%" src="https://www.youtube.com/embed/RzoDdms-6jc" frameborder="0" allowfullscreen></iframe>
                            </div>
                            <div class="medium-6 columns tt-video-desc">
                                <h3><a href="#">Car Example Measurement</a></h3>
                                <p>Illustrates how judgements/data were entered for the simple car purchase example.</p>
                            </div>
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </div>
</asp:Content>
