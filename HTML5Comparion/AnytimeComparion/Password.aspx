<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Password.aspx.cs" Inherits="AnytimeComparion.Password" %>
<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<%--<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>--%>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="row" ng-init="checkPasswordResetUrl()" style="font-size: 14px; margin-top: 50px;">
        <div class="columns small-12 medium-6 medium-offset-3 large-4 large-offset-4">
            <div id="divPasswordInput" class="hide">
                <div class="row">
                    <div class="columns small-12">
                        <h5 class="normal-blue" style="margin-bottom:1em; font-size: 16px;"><% =TeamTimeClass.ResString("msgCreatePassword")%></h5>
                    </div>
                </div>
                <div class="row">
                    <div class="columns small-12">
                        <input type="password" id="InputPassword" runat="server" placeholder="{{PasswordFieldText}}" ng-model="PasswordNew" ng-keyup="checkPasswordStrength(PasswordNew, PasswordAgain)"/>
                    </div>
                </div>
                <div class="row">
                    <div class="columns small-12">
                        <input type="password" id="InputPasswordAgain" runat="server" placeholder="{{PasswordAgainFieldText}}" ng-model="PasswordAgain" ng-keyup="checkPasswordStrength(PasswordNew, PasswordAgain)"/>
                    </div>
                </div>

                <div class="row">
                    <div class="columns small-12">
                        <div id="divPasswordCheckMessage" class="columns small-12" style="background-color: yellow;"></div>
                    </div>
                </div>
                <div class="row">
                    <div class="small-12 medium-4 medium-offset-8 large-3 large-offset-9 columns">
                        <br/>
                        <a id="ButtonSave" href="#" ng-click="saveNewPassword(PasswordNew, PasswordAgain)" class="button tiny success normal-green-bg radius" style="width: 100%;"><% =TeamTimeClass.ResString("btnSave") %></a>
                    </div>
                </div>
                
                <div class="row">
                    <div class="columns small-12">
                        <div id="divPasswordErrorMessage" class="columns small-12 bold center normal-red"></div>
                    </div>
                </div>
            </div>

            <div id="divResetSuccessMessage" class="hide">
                <div class="row">
                    <div class="columns small-12">
                        <div id="divMessage" class="columns small-12 bold center normal-blue"></div>
                    </div>
                </div>
                <div class="row">
                    <div class="small-12 medium-4 medium-offset-8 large-3 large-offset-9 columns">
                        <br/>
                        <a id="ButtonOK" href="/Default.aspx" class="button tiny success normal-green-bg radius" style="width: 100%;"><% =TeamTimeClass.ResString("btnOK") %></a>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
