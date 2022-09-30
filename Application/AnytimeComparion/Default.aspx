<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Default.aspx.vb" Inherits="._Default" %>

<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<%@ Import Namespace="ExpertChoice.Data" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Comparion</title>
    <!-- Bootstrap Css -->
    <link href="assets/css/bootstrap.min.css" id="bootstrapstyle" rel="stylesheet" type="text/css" />
    <!-- Icons Css -->
    <link href="assets/css/icons.css" rel="stylesheet" type="text/css" />
    <!-- App Css-->
    <link href="assets/css/app.css" id="appstyle" rel="stylesheet" type="text/css" />

    <script type="text/javascript" src="../../../Scripts/jquery.min.js"></script>
    <script type="text/javascript" src="../../../Scripts/jquery-ui.min.js"></script>
</head>
<body data-layout="horizontal">
    <div id="layout-wrapper" class="login_wrapper masg_preview">
        <header id="page-topbar">
            <div class="navbar-header">
                <div class="d-flex">
                    <!-- LOGO -->
                    <div class="navbar-brand-box">
                        <a href="javascript:void(0);" class="logo logo-dark">
                            <span class="logo-sm">
                                <img src="assets/images/logo.png" alt="" height="32" />
                            </span>
                            <span class="logo-lg">
                                <img src="assets/images/logo.png" alt="" height="60" />
                            </span>
                        </a>
                    </div>

                    <button type="button" class="btn btn-sm px-3 font-size-16 d-lg-none header-item waves-effect waves-light" data-bs-toggle="collapse" data-bs-target="#topnav-menu-content">
                        <i class="fa fa-fw fa-bars"></i>
                    </button>
                </div>
            </div>
        </header>

        <div class="page-content">
            <div class="container-fluid">
                <%If Session(Pages.external_classes.Constants.Sess_SignUp) IsNot Nothing AndAlso Session(Pages.external_classes.Constants.Sess_SignUp).ToString() <> "" Then %>
                <% If Session(Pages.external_classes.Constants.Sess_ShowMessage) IsNot Nothing AndAlso Session(Pages.external_classes.Constants.Sess_ShowMessage).ToString() <> "" AndAlso CBool(Session(Pages.external_classes.Constants.Sess_ShowMessage)) = True Then %>
                <div class="row">
                    <div class="large-12 columns large-centered text-center">
                        <h3 class="mt-3 normal-blue"><%=Session(Pages.external_classes.Constants.Sess_InviteMessage) %></h3>
                    </div>
                </div>
                <% End If %>
                <div class="row">
                    <% If Session("customTitle") IsNot Nothing AndAlso Session("customTitle").ToString().Trim() <> "" Then %>
                    <div class="col-md-8 col-lg-6 col-xl-6 offset-xl-3 offset-lg-3 offset-md-2 columns large-centered text-center">
                        <h4 class="mt-3 normal-blue"><%=Session("customTitle") %></h4>	   
                    </div>
                    <% End If %>
                    <% If Session("customMessage") IsNot Nothing AndAlso Session("customMessage").ToString().Trim() <> "" Then %>
                    <div class="col-md-8 col-lg-6 col-xl-6 offset-xl-3 offset-lg-3 offset-md-2 columns large-centered text-center mt-3 ">
                        <h6 class="text-center m-0 prm-color"><%=Session("customMessage") %></h6>
                    </div>
                    <% End If %>

                    <div class="col-md-8 col-lg-6 col-xl-6 offset-xl-3 offset-lg-3 offset-md-2 columns large-centered text-center">
                        <h4 class="normal-blue" id="InviteTitle" runat="server"></h4>
                    </div>

                    <div class="col-md-8 col-lg-6 col-xl-6 offset-xl-3 offset-lg-3 offset-md-2 columns large-centered text-center">
                        <div class="">
                            <h4 class="normal-blue">                              
                                <%:Session("ProjectName")%></h4>
                        </div>
                    </div>

                    <div class="col-md-8 col-lg-6 col-xl-6 offset-xl-3 offset-lg-3 offset-md-2">
                        <div class="card overflow-hidden">
                            <div class="bg-primary bg-soft">
                                <div class="row">
                                    <div class="col-12">
                                        <div class="p-4">
                                            <h5 class="text-center m-0 prm-color">Sign-up! or <span style="color: #0058a3 !important;">Sign in with an existing login</span></h5>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="card-body">
                                <div class="row ">
                                    <div class="large-12 medium-12 small-12 columns mb-2" id="signUpLinkFields_0">
                                        <span id="spnspinner" style="display: none;" class="spinner_icon rotating "><i id="addspinner" class="fa-1x mdi mdi-spin spinner-border"></i></span>
                                        <span class="irequired" id="irequired_e">*</span>
                                        <input type="text" runat="server" placeholder="Email" class="tt-input-form signup-form form-control loginClass" id="SEmail" onblur="checkExistingUser()" />
                                        <%--<i id="SEmailChecking" class="tt-input-form blue fas fa-circle-notch fa-spin d-none">checking</i>--%>
                                        <i id="SEmailChecking" class="fas fa-spinner fa-spin d-none"></i>
                                        <small id="SEmailError" class="error d-none">Invalid email.</small>
                                        <small id="SEmailreq" class="error d-none">Email is required</small>
                                    </div>
                                </div>

                                <div class="row ">
                                    <%--<div class="large-12 medium-12 small-12 columns" ng-show="signUpLinkFields[1] && signUpUser.IsNewUser">--%>
                                    <div class="large-12 medium-12 small-12 columns my-2" id="signUpLinkFields_1">
                                        <span class="irequired" id="irequired_n">*</span>
                                        <input type="text" runat="server" placeholder="Name" class="tt-input-form signup-form form-control loginClass" id="SName" name="signUpUser.SName" onkeydown="loginFormInputChanged('name')" />
                                        <small id="SNamereq" class="error d-none">Name is required</small>
                                    </div>
                                </div>

                                <div class="row">
                                    <%--<div class="large-12 medium-12 small-12 columns" ng-show="signUpLinkFields[3] && signUpUser.IsNewUser">--%>
                                    <div class="large-12 medium-12 small-12 columns my-2" id="signUpLinkFields_3">
                                        <span class="irequired" id="irequired_t">*</span>
                                        <input type="text" id="SPhone" placeholder="Phone Number" class="tt-input-form signup-form form-control loginClass" name="signUpUser.SPhone" />
                                        <small id="SPhoneError" class="error d-none">Invalid phone number.</small>
                                        <small id="SPhonereq" class="error d-none">Phone Number is required</small>
                                    </div>
                                </div>

                                <div class="row ">
                                    <%--<div class="large-12 medium-12 small-12 columns" ng-show="(signUpLinkFields[2]) || !signUpUser.IsNewUser">--%>
                                    <div class="large-12 medium-12 small-12 columns my-2" id="signUpLinkFields_2">
                                        <span class="irequired" id="irequired_p">*</span>
                                        <input type="password" runat="server" placeholder="Password" class="tt-input-form signup-form form-control loginClass mb-2 " id="SPass" ng-model="signUpUser.SPass" ng-keydown="loginFormInputChanged('pass')" />
                                        <%--<a href="javascript:void(0);" ng-show="!signUpUser.IsNewUser" style="font-size: 11px;" ng-click="showPasswordResetModal('hashLogin')">Password Help?</a>--%>
                                        <a href="javascript:void(0);" id="passHelp" class="d-none" style="font-size: 11px;" onclick="OpenModel('ResetPassword')">Password Help?</a>
                                        <small id="SPasswordreq" class="error d-none">Password is required.</small>
                                        <small id="SPasswordError" class="error d-none">Invalid password.</small>
                                    </div>
                                </div>

                                <div class="row ">
                                    <%--<div class="large-12 medium-12 small-12 columns" ng-show="signUpLinkFields[2] && signUpUser.IsNewUser">--%>
                                    <div class="large-12 medium-12 small-12 columns my-2" id="divsignUpLinkFields_2">
                                        <span class="irequired" id="irequired_cp">*</span>
                                        <input type="password" runat="server" placeholder="Re-type Password" class="tt-input-form signup-form form-control loginClass" id="SPass2" name="signUpUser.SPass2" onkeydown="loginFormInputChanged('repass')" />
                                        <small id="SPasswordError2" class="error d-none">Password do not match!</small>
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="large-12 medium-12 small-12 ">
                                        <div class="small-12 medium-4 medium-offset-8 large-3 large-offset-9 columns">
                                            <br />
                                            <a id="SignUpBtn" href="javascript:void(0);" onclick="processLogin(signUpUser.SEmail, signUpUser.SName, signUpUser.SPass, signUpUser.SPass2, signUpUser.SPhone)" class="btn btn-green" style="width: 100%;">Sign In</a>
                                        </div>
                                    </div>
                                </div>
                                <div class="tt-login-form-status"></div>
                                <div id="loginFormIp" class="hide"></div>
                            </div>
                        </div>
                    </div>
                </div>

                <% ElseIf Session("UserSpecificHashErrorMessage") IsNot Nothing AndAlso Session("UserSpecificHashErrorMessage").ToString() <> "" Then %>
                <div class="row">
                    <div class="center_datarow">
                        <h3 class="normal-blue"><%=Session("UserSpecificHashErrorMessage") %></h3>
                    </div>
                </div>
                <% ElseIf Session("pageError") IsNot Nothing AndAlso Session("pageError").ToString() <> "" Then %>
                <div class="row">
                    <div class="center_datarow">
                        <h3 class="normal-blue"><%=Session("pageError") %></h3>
                    </div>
                </div>
                <% End If %>
            </div>
        </div>
    </div>

    <!-- Modal -->
    <div id="PasswordResetModal" class="modal fade" role="dialog">
        <div class="modal-dialog">
            <!-- Modal content-->
            <div class="modal-content">
                <div class="modal-header">
                    <h4 id="modalTitle" runat="server" class="modal-title"><% =TeamTimeClass.ResString("lblPasswordRestore")%></h4>
                    <button type="button" class="close" data-dismiss="modal" onclick="CloseModel('ResetPassword')">&times;</button>
                </div>
                <div class="modal-body">
                    <div id="divmasPassRes" class="columns text-center">
                        <% =TeamTimeClass.ResString("msgPasswordRestore")%>
                    </div>
                    <div id="msgPasswordRestore" class="text-center" runat="server"></div>
                    <div class="home-forms">
                        <div id="ResetEmailDiv" class="mt-4">
                            <label class="mb-2">Email <span class="text-danger">*</span></label>
                            <input type="text" runat="server" placeholder="Email" class="form-control" id="ResetEmail" />
                            <small id="ResetEmailError" class="displayCls"></small>
                            <small id="ResetEmailInfo" class="displayCls" style="background-color: #0058a3;"></small>
                        </div>

                        <div id="ResetSendSuccess" class="hide"></div>

                        <div class="text-end">
                            <br />
                            <%--<a id="ResetPasswordBtn" href="#" ng-click="resetPassword(ResetEmail)" class="button tiny success" style="width: 100%;">{{isPasswordResetEmailSent ? 'OK' : 'Send'}}</a>--%>
                            <a id="ResetPasswordBtn" href="javasript:void(0);" onclick="ResetPassword(0);" class="btn btn_bg px-4 text-white">Send</a>
                        </div>
                        <%--<div class="columns content tt-home-not-logged small-12 medium-6 large-6" style="padding: 0 2rem" id="login-tab">
                        </div>--%>
                    </div>
                </div>
                <%--<div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                </div>--%>
            </div>
        </div>
    </div>

    <!-- JAVASCRIPT -->
    <script type="text/javascript" src="assets/libs/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="assets/libs/bootstrap/js/bootstrap.bundle.min.js"></script>
    <script type="text/javascript" src="assets/libs/metismenu/metisMenu.min.js"></script>

    <script type="text/javascript">
        var baseUrl = '<%= ResolveUrl("~/") %>';
        var shouldCursorMoveToNextField = false, existingUeserTimer = null, isPasswordResetFromSignUp = false, isPasswordResetEmailSent = false;
        var signUpLinkFields = [], general_link_title = "Sign up or Log in", is_signup_form = true;
        var signup = [{ 'email': 'e', 'name': 'n', 'password': 'p', 'message': '' }];
        var req_signup = { 'email': 'e', 'name': 'n', 'password': 'p' };
        var signup_mode = "enp";
        var req_signup_mode = "enp";
        var permission_level = "1";
        var signUpUser = {
            SEmail: "",
            SName: "",
            SPhone: "",
            SPass: "",
            SPass2: "",
            IsNewUser: true
        };

        $(document).ready(function () {
            <% If Session(Pages.external_classes.Constants.Sess_SignUp) IsNot Nothing Then%>
        <% If Session(Pages.external_classes.Constants.Sess_SignUpMode) IsNot Nothing AndAlso Session(Pages.external_classes.Constants.Sess_SignUpMode).ToString() <> "" Then%>
            signup_mode = '<%=Session(Pages.external_classes.Constants.Sess_SignUpMode)%>'
            req_signup_mode = '<%=Session(Pages.external_classes.Constants.Sess_Requirements)%>';
            show_signup_fields();
        //show_content_now();
            <% Else%>
            GeneralLinkSignUp(signUpUser.SEmail, signUpUser.SName, signUpUser.SPass, signUpUser.SPass2, signUpUser.SPhone, null, true);
        <%End If %>
            <%--<%
        ElseIf Session("UserSpecificHashErrorMessage") IsNot Nothing AndAlso Session("UserSpecificHashErrorMessage").ToString() <> "" Then%>
            var msg = "<%=Session("UserSpecificHashErrorMessage").ToString()%>";
            mesage_layout('alert', msg);--%>
        <%End If %>
        });

        //$('#SEmail').on('keypress', function () {
        //    var re = /([A-Z0-9a-z_-][^@])+?@[^$#<>?]+?\.[\w]{2,4}/.test(this.value);
        //    if (!re) {
        //        $("#SEmailError").show();
        //    } else {
        //        $("#SEmailError").hide();
        //    }

        //});

        var show_signup_fields = function () {
            
            if (signup_mode.indexOf("e") > -1) {
                signUpLinkFields[0] = true;
                $('#signUpLinkFields_0').show();
                $('#irequired_e').hide();
                $('#SEmailError').removeClass('d-none');
                $('#SEmailError').addClass('d-none');
                $('#SEmailreq').removeClass('d-none');
                $('#SEmailreq').addClass('d-none');
                if (req_signup_mode.indexOf('e') > -1)
                    $('#irequired_e').show();
            }
            else {
                signUpLinkFields[0] = false;
                $('#signUpLinkFields_0').hide();
                $('#irequired_e').hide();
                $('#SEmailError').removeClass('d-none');
                $('#SEmailError').addClass('d-none');
                $('#SEmailreq').removeClass('d-none');
                $('#SEmailreq').addClass('d-none');
            }

            if (signup_mode.indexOf("n") > -1) {
                signUpLinkFields[1] = true;
                $('#signUpLinkFields_1').show();
                $('#irequired_n').hide();
                $('#SNamereq').removeClass('d-none');
                $('#SNamereq').addClass('d-none');
                if (req_signup_mode.indexOf('n') > -1)
                    $('#irequired_n').show();
            }
            else {
                signUpLinkFields[1] = false;
                $('#signUpLinkFields_1').hide();
                $('#irequired_n').hide();
                $('#SNamereq').removeClass('d-none');
                $('#SNamereq').addClass('d-none');
            }

            if (signUpLinkFields[1] && signUpUser.IsNewUser) {
                $('#signUpLinkFields_1').show();
                $('#irequired_n').hide();
                $('#SNamereq').removeClass('d-none');
                $('#SNamereq').addClass('d-none');
                if (req_signup_mode.indexOf('n') > -1)
                    $('#irequired_n').show();
            }
            else {
                $('#signUpLinkFields_1').hide();
                $('#irequired_n').hide();
                $('#SNamereq').removeClass('d-none');
                $('#SNamereq').addClass('d-none');
            }

            if (signup_mode.indexOf("p") > -1) {
                signUpLinkFields[2] = true;
            }
            else {
                signUpLinkFields[2] = false;
            }

            if (signUpLinkFields[2] || !signUpUser.IsNewUser) {
                $('#signUpLinkFields_2').show();
                $('#irequired_p').hide();
                $('#SPasswordreq').removeClass('d-none');
                $('#SPasswordreq').addClass('d-none');
                $('#SPasswordError').removeClass('d-none');
                $('#SPasswordError').addClass('d-none');
                if (req_signup_mode.indexOf('p') > -1)
                    $('#irequired_p').show();
            }
            else {
                $('#signUpLinkFields_2').hide();
                $('#irequired_p').hide();
                $('#SPasswordreq').removeClass('d-none');
                $('#SPasswordreq').addClass('d-none');
                $('#SPasswordError').removeClass('d-none');
                $('#SPasswordError').addClass('d-none');
            }

            if (signUpLinkFields[2] && signUpUser.IsNewUser) {
                $('#divsignUpLinkFields_2').show();
                $('#irequired_cp').hide();
                $('#SPasswordError2').removeClass('d-none');
                $('#SPasswordError2').addClass('d-none');
                if (req_signup_mode.indexOf('p') > -1)
                    $('#irequired_cp').show();
            }
            else {
                $('#divsignUpLinkFields_2').hide();
                $('#irequired_cp').hide();
                $('#SPasswordError2').removeClass('d-none');
                $('#SPasswordError2').addClass('d-none');
            }

            if (signup_mode.indexOf("t") > -1) {
                signUpLinkFields[3] = true;
            }
            else {
                signUpLinkFields[3] = false;
            }

            if (signUpLinkFields[3] && signUpUser.IsNewUser) {
                $('#signUpLinkFields_3').show();
                $('#irequired_t').hide();
                $('#SPhoneError').removeClass('d-none');
                $('#SPhoneError').addClass('d-none');
                if (req_signup_mode.indexOf('t') > -1)
                    $('#irequired_t').show();
            }
            else {
                $('#signUpLinkFields_3').hide();
                $('#irequired_t').hide();
                $('#SPhoneError').removeClass('d-none');
                $('#SPhoneError').addClass('d-none');
            }
        }

        var checkEnteredKeyForEmail = function (event) {
            var keyCode = event ? event.keyCode : 0;
            if (keyCode == 9 || keyCode == 13) {
                shouldCursorMoveToNextField = true;
            }
        }

        var cancelExistingUserTimer = function () {
            if (existingUeserTimer != undefined && existingUeserTimer != null) {
                //setTimeout.cancel(existingUeserTimer);
                existingUeserTimer = undefined;
            }
        }

        var isValidEmail = function (email) {
            if (email == undefined || email == null || email.trim().length == 0) return false;

            var testEmail = /^[A-Z0-9._%+-]+@([A-Z0-9-]+\.)+[A-Z]{2,4}$/i;
            return testEmail.test(email);
        }

        var clearPreviousLoginMessage = function () {
            $(".tt-login-form-status").html("");
        }

        var moveToNextFieldOnSignUp = function () {
            if (shouldCursorMoveToNextField) {
                setTimeout(function () {
                    if (signUpUser.IsNewUser && signUpLinkFields[1]) {
                        if (existingUeserTimer == undefined) {
                            $("#SName").focus();
                        }
                    }
                    else {
                        $("#SPass").focus();
                    }
                }, 50);

                shouldCursorMoveToNextField = false;
            }
        }

        var mesage_layout = function (type, message) {
            /*return $(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box ' + type + ' radius tt-alert msg-stat">           <h3> ' + message + '<a href="javascript:void(0);" class="close">&times;</a>          </h3>        </div>      </div>');*/
            return $(".tt-login-form-status").html('<div data-alert="" class="alert alert-' + type + ' alert-dismissible fade show">           ' + message + '              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
        }

        var checkExistingUser = function () {
            // cancelExistingUserTimer();
            // var timeOutMs = isValidEmail(signUpUser.SEmail) ? 500 : 2000;

            //  existingUeserTimer = setTimeout(function () {
            signUpUser.SEmail = $('#SEmail').val();
            if (signUpUser.SEmail != undefined && signUpUser.SEmail != null && signUpUser.SEmail.trim().length > 0) {
                $("#spnspinner").css('display', 'block');
                $("#SEmail").val(signUpUser.SEmail.trim());
                //cancelExistingUserTimer();
                //$scope.clearPreviousLoginMessage();

                //$("#SEmailError").hide();
                //$("#SEmailreq").hide();
                $('#SEmailError').removeClass('d-none');
                $('#SEmailError').addClass('d-none');
                $('#SEmailreq').removeClass('d-none');
                $('#SEmailreq').addClass('d-none');

                /* setTimeout(function () {*/
                $.ajax({
                    type: "POST",
                    url: baseUrl + "AnytimeComparion/Default.aspx/IsExistingUser",
                    data: JSON.stringify({
                        email: signUpUser.SEmail.trim()
                    }),
                    async: false,
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        signUpUser.IsNewUser = (data.d == "false");
                        setTimeout(function () {
                            $("#spnspinner").css('display', 'none');
                        }, 500);
                        clearPreviousLoginMessage();
                        moveToNextFieldOnSignUp();
                        show_signup_fields();
                        if (!signUpUser.IsNewUser) {
                            $('#passHelp').removeClass('d-none');
                        }
                        else {
                            $('#passHelp').removeClass('d-none');
                            $('#passHelp').addClass('d-none');
                        }
                    },
                    error: function (response) {
                        $("#SEmailChecking").addClass("d-none");
                        var errorMessage = response.status + (response.data || "Request failed");
                        mesage_layout("danger", errorMessage);
                    }
                });
                // }, 100);
            }
            //  }, 0);
        }

        var loginFormInputChanged = function (inputType) {
            clearPreviousLoginMessage();

            if (inputType == "name") {
                //$("#SNameError").hide();
                $('#SNamereq').removeClass('d-none');
                $('#SNamereq').addClass('d-none');
            }
            else if (inputType == "pass") {
                //$("#SPasswordreq").hide();
                //$("#SPasswordError").hide();
                $('#SPasswordError').removeClass('d-none');
                $('#SPasswordError').addClass('d-none');
            }
            else if (inputType == "repass") {
                //$("#SPasswordError2").hide();
                $('#SPasswordError2').removeClass('d-none');
                $('#SPasswordError2').addClass('d-none');
            }
        }

        var showPasswordResetModal = function (formName) {
            if (formName == "login" || formName == "hashLogin") {
                $("#PasswordResetModal").foundation("reveal", "open");
                isPasswordResetFromSignUp = formName == "hashLogin";
            }
            return false;
        }

        var processLogin = function (email, name, password, password2, phone) {
            email = $('#SEmail').val();
            name = $('#SName').val();
            password = $('#SPass').val();
            password2 = $('#SPass2').val();
            phone = $('#SPhone').val();
            //if (existingUeserTimer != undefined) {
            if (signUpUser.IsNewUser) {
                GeneralLinkSignUp(email, name, password, password2, phone, null);
            }
            else {
                GeneralLinkLogin(email, password);
            }
            //}
        }

        var GeneralLinkSignUp = function (email, name, password, password2, phone, e, allow) {
            if (e == null) is_signup_form = true;
            //if enter or go key was pressed
            if (e != null) {
                var code = e.which;
                if (code != 13)
                    return false;
            }

            mesage_layout("success", "Proceeding...");
            //$(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box radius tt-alert msg-stat" style="background: #0058a3">           <h3>Proceeding...          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
            //$(".error").hide();

            var bypass = true;
            if (req_signup_mode.indexOf("e") > -1) {
                var testEmail = /^[A-Z0-9._%+-]+@([A-Z0-9-]+\.)+[A-Z]{2,4}$/i;
                if (email != null || email != "") {
                    if (!testEmail.test(email)) {
                        bypass = false;
                        //$("#SEmailError").show();
                        $('#SEmailError').removeClass('d-none');
                    }
                }
                else {
                    bypass = false;
                    //$("#SEmailreq").show();
                    $('#SEmailreq').removeClass('d-none');
                }
            }
            else {
                if (email == null || email == "") {
                    email = "";
                }
                else {
                    var testEmail = /^[A-Z0-9._%+-]+@([A-Z0-9-]+\.)+[A-Z]{2,4}$/i;
                    if (email != null) {
                        if (!testEmail.test(email)) {
                            bypass = false;
                            //$("#SEmailError").show();
                            $('#SEmailError').removeClass('d-none');
                        }
                    }
                }

            }

            if (req_signup_mode.indexOf("n") > -1) {
                if (name == null || name.trim() == "") {
                    bypass = false;
                    //$("#SNameError").show();
                    $('#SNamereq').removeClass('d-none');
                }
            }
            else {
                if (name == null) {
                    name = "";
                }
            }

            if (req_signup_mode.indexOf("p") > -1) {
                if (password == undefined || password == null || password == "") {
                    bypass = false;
                    //$("#SPasswordreq").show();
                    $('#SPasswordreq').removeClass('d-none');
                }

                if (password != password2) {
                    bypass = false;
                    //$("#SPasswordError2").show();
                    $('#SPasswordError2').removeClass('d-none');
                }
            }
            else {
                if (password == null || password == undefined) {
                    password = "";
                }
                if (password2 == null || password2 == undefined) {
                    password2 = "";
                }

                if (req_signup_mode.indexOf("p") == -1) {
                    password = "";
                    password2 = "";
                }

                if (password != password2) {
                    bypass = false;
                    //$("#SPasswordError2").show();
                    $('#SPasswordError2').removeClass('d-none');
                }
            }

            if (req_signup_mode.indexOf("t") > -1) {
                if (phone == null) {
                    bypass = false;
                    //$("#SPhonereq").show();
                    $('#SPhonereq').removeClass('d-none');
                }
            }
            else {
                if (phone == null) {
                    phone = "";
                }
            }

            //$(".tt-login-form-status").html("");

            if (allow) {
                bypass = true;
                email = "";
                name = "";
                password = "";
                //show_loading_modal();
            }

            if (bypass) {
                $.ajax({
                    type: "POST",
                    url: baseUrl + "AnytimeComparion/Default.aspx/GeneralLinkSignUp",
                    data: JSON.stringify({
                        email: email,
                        name: name,
                        password: password,
                        sPhone: phone,
                        signup_mode: signup_mode.indexOf("p") > -1
                    }),
                    async: false,
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        var result = JSON.parse(data.d);
                        if (result.pass) {
                            window.location.href = window.location.origin + "/AnytimeComparion/Pages/Anytime/Anytime.aspx";
                            mesage_layout("primary", "Proceeding...");

                            //LoadingScreen.init(LoadingScreen.type.loadingModal, $("#LoadingModalPercentage"), $(".fullwidth-loading-wrap"));
                            //LoadingScreen.start(70);
                            return true;
                        }
                        else {
                            mesage_layout("danger", result.message);

                            var errorElement = $("#SignUpModal .tt-login-form-status");
                            if (errorElement.length > 0) {
                                errorElement[0].scrollIntoView();
                            }

                            return false;
                        }
                    },
                    error: function (response) {

                    }
                });
            }
            else {
                clearPreviousLoginMessage();
            }
        }

        var GeneralLinkLogin = function (email, password) {
            is_signup_form = false;
            $(".tt-login-form-status").html('<div class="large-12 columns">          <div data-alert class="alert-box radius tt-alert msg-stat" style="background: #0058a3">           <h3>Checking...          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
            email = email == null ? "" : email;
            password = password == null ? "" : password;

            $.ajax({
                type: "POST",
                url: baseUrl + "AnytimeComparion/Default.aspx/GeneralLinkLogin",
                data: JSON.stringify({
                    email: email,
                    password: password
                }),
                async: false,
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var result = JSON.parse(data.d);
                    if (result.pass) {
                        window.location.href = "Pages/Anytime/Anytime.aspx";
                        mesage_layout("success", "Login successfull. Please wait while we redirect you to home page...");

                        //LoadingScreen.init(LoadingScreen.type.loadingModal, $("#LoadingModalPercentage"), $(".fullwidth-loading-wrap"));
                        //LoadingScreen.start(70);
                        return true;
                    }
                    else {
                        mesage_layout("danger", result.message);
                        return false;
                    }
                },
                error: function (response) {

                }
            });
        }

        //open popup
        function OpenModel(modelType) {
            //Open reset password model
            if (modelType != undefined && modelType != null && modelType == 'ResetPassword') {
                $('#PasswordResetModal').modal('show');
                RefreshModel(modelType);
            }
        }

        //close popup
        function CloseModel(modelType) {
            //Close reset password model
            if (modelType != undefined && modelType != null && modelType == 'ResetPassword') {
                $('#PasswordResetModal').modal('hide');
                RefreshModel(modelType);
            }
        }

        //Reset model values
        function RefreshModel(modelType) {
            if (modelType != undefined && modelType != null && modelType == 'ResetPassword') {
                if (isPasswordResetEmailSent) {
                    $('#divmasPassRes').hide();
                }
                else {
                    $('#divmasPassRes').show();
                }
                $('#msgPasswordRestore').show();
                $("#ResetEmailDiv").show();
                $('#ResetPasswordBtn').show();
                $('#ResetEmail').val('');
                $("#ResetEmailError").hide();
                $("#ResetEmailError").val('');
                $("#ResetSendSuccess").hide();
                $("#ResetSendSuccess").val('');
            }
        }

        //reset password
        function ResetPassword() {
            var email = $('#ResetEmail').val();
            if (email == undefined || email == null || email == '') {
                if ($('#ResetEmailError').hasClass('displayCls')) {
                    $('#ResetEmailError').removeClass('displayCls');
                    $("#ResetEmailError").addClass('loginerrors');
                    $("#ResetEmailError").text('Email is required');
                }
            }
            else {
                $("#ResetEmailError").text('');
                if (!$('#ResetEmailError').hasClass('displayCls')) {
                    $('#ResetEmailError').addClass('displayCls');
                }
                $.ajax({
                    type: "POST",
                    url: baseUrl + "AnytimeComparion/Login.aspx/ResetPassword",
                    data: JSON.stringify({ email: email }),
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    success: function (response) {
                        $("#ResetEmailInfo").hide();
                        var result = JSON.parse(response.d);
                        isPasswordResetEmailSent = result.isSuccess;
                        if (result.isSuccess) {
                            $('#divmasPassRes').hide();
                            $('#msgPasswordRestore').hide();
                            $("#ResetEmailDiv").hide();
                            $("#ResetSendSuccess").html(result.message);
                            $("#ResetSendSuccess").show();
                            $('#ResetPasswordBtn').hide();
                        }
                        else {
                            $('#divmasPassRes').show();
                            $('#msgPasswordRestore').show();
                            $("#ResetEmailDiv").show();
                            $("#ResetEmailError").html(result.message);
                            $("#ResetEmailError").show();
                            $('#ResetPasswordBtn').show();
                        }
                    },
                    error: function (response) {
                    }
                });
            }
        }
    </script>
</body>
</html>
