<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Resources.aspx.cs" Inherits="AnytimeComparion.Pages.Resources" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="tt-body">
        <div class="row">
            <div class="large-12 columns">
                <div class="large-8 columns large-centered text-center tt-resources-wrap">
                    <h1>Resources</h1>
                    <div class="large-12 columns" data-equalizer>
                        <div class="medium-4 columns" data-equalizer-watch>
                            <a class="tt-r-btn-cta" runat="server" href="~/sample-videos">
                                <span class="icon-tt-film icon"></span>
                                <br>
                                Sample
                                <br>
                                Videos
                            </a>
                        </div>
                        <div class="medium-4 columns" data-equalizer-watch>
                            <a class="tt-r-btn-cta" runat="server" href="~/getting-started-videos-and-documentation">
                                <span class="icon-tt-video-documentation icon"></span>
                                <br>
                                Getting Started:
                                <br>
                                Videos and Documentation
                            </a>
                        </div>
                        <div class="medium-4 columns" data-equalizer-watch>
                            <a class="tt-r-btn-cta" runat="server" href="~/resource-allocation-videos">
                                <span class="icon-tt-video-resources icon"></span>
                                <br>
                                Resource Allocation Videos
                            </a>
                        </div>
                    </div>

                    <div class="large-12 columns" data-equalizer>
                        <div class="medium-4 columns" data-equalizer-watch>
                            <a class="tt-r-btn-cta" runat="server" href="~/comparison-sample-project-files">
                                <span class="icon-tt-file icon"></span>
                                <br>
                                Comparison Sample
                                <br>
                                Project Files
                            </a>
                        </div>
                        <div class="medium-4 columns" data-equalizer-watch>
                            <a class="tt-r-btn-cta" runat="server" href="~/comparison-sample-template-files">
                                <span class="icon-tt-clipboard icon"></span>
                                <br>
                                Comparison Sample
                                <br>
                                Template Files
                            </a>
                        </div>
                        <div class="medium-4 columns" data-equalizer-watch>
                            <a class="tt-r-btn-cta" runat="server" href="~/help-and-other-resources">
                                <span class="icon-tt-folder-open icon"></span>
                                <br>
                                Help and Other
                                <br>
                                Resources
                            </a>
                        </div>
                    </div>

                </div>

            </div>
        </div>
    </div>
</asp:Content>
