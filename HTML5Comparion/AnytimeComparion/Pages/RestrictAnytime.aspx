<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RestrictAnytime.aspx.cs" Inherits="AnytimeComparion.Pages.RestrictAnytime" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" class="tt-content-wrap" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row" style="display:table;">
        <div class="large-6 columns large-centered text-center" style="display: table-cell; vertical-align: middle; padding: 10% 0 10% 0;">
            <div class="columns panel browser-notice">
                <div class="large-1 small-12  columns">
                    <span class="icon-tt-info-circle notice-icon"></span>
                </div>     
                <div class="large-11 small-12 columns ">
                    <span>
                        We are sorry but you can't access AnyTime evaluation while TeamTime meeting is ongoing.
                    </span>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
