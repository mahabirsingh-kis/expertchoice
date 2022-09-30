<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Login.aspx.vb" Inherits=".Login" %>

<%@ Import Namespace="Pages.external_classes" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>Comparion</title>
    <%--<meta name="viewport" content="width=device-width, initial-scale=1.0">
    <!-- App favicon -->
    <link rel="shortcut icon" href="assets/images/favicon.png">--%>

    <!-- Bootstrap Css -->
    <link href="assets/css/bootstrap.min.css" id="bootstrapstyle" rel="stylesheet" type="text/css" />
    <!-- Icons Css -->
    <link href="assets/css/icons.css" rel="stylesheet" type="text/css" />
    <!-- App Css-->
    <link href="assets/css/app.css" id="appstyle" rel="stylesheet" type="text/css" />
</head>
<body data-layout="horizontal">
    <div id="layout-wrapper" class="login_wrapper">
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
                <!-- start page title -->
                <div class="row">
                    <div class="col-md-8 col-lg-6 col-xl-6 offset-xl-3 offset-lg-3 offset-md-2 text-center">
                        <p class="mb-sm-0 font-size-18 prm-color">
                            <strong>Comparion<br />
                                Collaborative Decision Making Solution
                            </strong>
                        </p>
                        <p>
                            Ensure that you set objective that align with strategic goals. Make better decisions
                        With fewer wasted meetings and less gridlock. Improve communication, transparency leading to improved alignment and implementation
                        </p>
                        <div id="hashLinkMessage" class="text-center" runat="server" style="font-size: 14px; margin-bottom: 15px;"></div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-8 col-lg-6 col-xl-6 offset-xl-3 offset-lg-3 offset-md-2">
                        <div class="card overflow-hidden">
                            <div class="bg-primary bg-soft">
                                <div class="row">
                                    <div class="col-12">
                                        <div class="p-4">
                                            <h5 class="text-center m-0 prm-color">Login to Expert Choice Comparion&reg;</h5>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="card-body pt-0">
                                <div class="row">
                                    <div class="col-md-8 offset-md-2">
                                        <div class="pt-5">
                                            <%--<form class="form-horizontal" action="index.html">--%>
                                            <div class="mb-3">
                                                <label for="username" class="form-label">Username</label>
                                                <%--<input type="text" class="form-control" id="username" placeholder="Enter username" />--%>
                                                <input type="email" placeholder="Email" class="form-control loginClass" runat="server" id="LoginEmail" />
                                                <small id="EmailError" class="displayCls">Invalid email.</small>
                                                <small id="EmailExistError" class="displayCls">Incorrect email. Please check your credentials.</small>
                                            </div>

                                            <div class="mb-3">
                                                <label class="form-label">Password</label>
                                                <%--<input type="password" class="form-control" placeholder="Enter password" aria-label="Password" aria-describedby="password-addon" />--%>
                                                <input type="password" placeholder="Password" class="form-control loginClass" runat="server" id="LoginPassword" />
                                                <small id="PasswordError" class="displayCls">Password is required.</small>
                                                <small id="IncorrectPasswordError" class="displayCls">Incorrect password. Please check your credentials.</small>
                                                <span class="mt-2 d-block" onclick="OpenModel('ResetPassword')"><a href="javascript:void(0);">Password help?</a></span>
                                            </div>
                                            <div class="mb-3">
                                                <label for="username" class="form-label">Access Code(optional)</label>
                                                <%--<input type="text" class="form-control" id="username" placeholder="Enter username" />--%>
                                                <input id="AccessCodeTxt" type="text" placeholder="Access Code" runat="server" class="form-control loginClass" />
                                                <small class="displayCls">Missing Access Code</small>
                                            </div>
                                            <div class="form-check">
                                                <input class="form-check-input" type="checkbox" runat="server" id="SrembMeLogin" />
                                                <label class="form-check-label" for="remember-check">
                                                    Remember me
                                                </label>
                                            </div>

                                            <div class="mt-3 d-grid">
                                                <button id="LoginBtn" class="btn btn-primary btn_bg clogin success" type="submit">Log In</button>
                                                <%--<asp:Button ID="LoginBtn" runat="server" CssClass="btn btn-primary btn_bg" OnClick="LoginBtn_Click" Text="Log In" />--%>
                                            </div>
                                            <%--<div class="mt-4 text-center">
                                                <a href="#" class="text-muted"><i class="mdi mdi-lock me-1"></i>Forgot your password?</a>
                                            </div>--%>
                                            <br />
                                            <div runat="server" id="LoginStatus" class="tt-login-form-status"></div>
                                            <div runat="server" id="Div2" style="clear: both;"></div>
                                            <%--</form>--%>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- end page title -->
            </div>
        </div>
    </div>

    <!-- Modal -->
    <div id="PasswordResetModal" class="modal fade" role="dialog">
        <div class="modal-dialog">
            <!-- Modal content-->
            <div class="modal-content">
                <div class="modal-header">
                    <h4 id="modalTitle" runat="server" class="modal-title"></h4>
                    <button type="button" class="close" data-dismiss="modal" onclick="CloseModel('ResetPassword')">&times;</button>
                </div>
                <div class="modal-body">
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
                        <div class="columns content tt-home-not-logged small-12 medium-6 large-6" style="padding: 0 2rem" id="login-tab">
                        </div>
                    </div>
                </div>
                <%--<div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                </div>--%>
            </div>
        </div>
    </div>
    <a href="<%= ResolveUrl("~/") %>" class="hide" id="base_url"></a>

    <!-- JAVASCRIPT -->
    <script type="text/javascript" src="assets/libs/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="assets/libs/bootstrap/js/bootstrap.bundle.min.js"></script>
    <script type="text/javascript" src="assets/libs/metismenu/metisMenu.min.js"></script>
    <script type="text/javascript" src="CustomScripts/login.js"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            <%  if (Request.Cookies("usernam") IsNot Nothing And Request.Cookies("passwor") IsNot Nothing) Then%>
            var a = "<%=Request.Cookies("usernam").Value  %>";
            var b = "<% =Request.Cookies("passwor").Value  %>";
            $("#LoginEmail").val(a);
            $("#LoginPassword").val(b);
            $("#SrembMeLogin").prop('checked', true);

                <% End If
        Dim fDB_OK As Boolean = True
        If (Session("App") IsNot Nothing) Then
            Dim App = CType(HttpContext.Current.Session("App"), ExpertChoice.Data.clsComparionCore)
            Dim fMasterExists As Boolean = App.isCanvasMasterDBValid
            Dim fProjectsExists As Boolean = App.isCanvasProjectsDBValid
            fDB_OK = (fMasterExists And fProjectsExists)
        End If

        If (fDB_OK = False) Then %>
            $("#LoginBtn").addClass("disabled");
            $("#joinmeetingbtn").addClass("disabled");
            $("#LoginBtn, #joinmeetingbtn").click(function (event) {
                event.preventDefault();
                return false;
            });
             <% End If %>
        });
    </script>
</body>
</html>
