var baseUrl = $('#base_url').val();

//hit enter call login function
$(document).on('keypress', '.meetingInput', function (e) {
    if (e.which == 13) {
        $('#joinmeetingbtn').click();
        //console.log('enter clicked');
        return false;
    }
});

//hit enter call login function
$(document).on('keypress', '.loginClass', function (e) {
    if (e.which == 13) {
        if (this.id == "ResetEmail") {
            $("#ResetPasswordBtn").click();
        } else if (this.id == "LoginEmail" || this.id == "LoginPassword" || this.id == "AccessCodeTxt") {
            $("#LoginBtn").trigger("click");
            $('.clogin').trigger('click');
        }
        //console.log('enter clicked');
        return false;
    }
});

$("#LoginEmail").focusout(function () {
    $("#EmailError").hide();
});

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
            url: baseUrl + "Login.aspx/ResetPassword",
            data: JSON.stringify({ email: email }),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                $("#ResetEmailInfo").hide();
                var result = JSON.parse(response.d);
                if (result.isSuccess) {
                    $('#msgPasswordRestore').hide();
                    $("#ResetEmailDiv").hide();
                    $("#ResetSendSuccess").html(result.message);
                    $("#ResetSendSuccess").show();
                    $('#ResetPasswordBtn').hide();
                } else {
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

//login
$(document).on("click", ".clogin", function () {
    //validation
    if (this.id == "joinmeetingbtn") {
        var MeetingID;
        if ($("#MeetingID").val() != null) {
            MeetingID = $("#MeetingID").val()
        }
        if ($("#LoggedMeetingID").val() != null) {
            MeetingID = $("#LoggedMeetingID").val()
        }

        var valid = true;

        if (!$("#meeting_name").val()) {
            $("#MeetingNameError").show();
            valid = false;
        } else {
            $("#MeetingNameError").hide();
        }

        if (!$("#meeting_email").val()) {
            $("#MeetingEmailError").show();
            valid = false;
        } else {
            $("#MeetingEmailError").hide();
        }

        if (valid) {
            //show_loading_modal();
            $.ajax({
                type: "POST",
                url: baseUrl + "Login.aspx/login",
                data: JSON.stringify({
                    email: "",
                    password: "",
                    passcode: "",
                    rememberme: "",
                    MeetingID: MeetingID,
                    meeting_name: $("#meeting_name").val(),
                    meeting_email: $("#meeting_email").val()
                }),
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    $(".error-msg").hide();
                    var output = JSON.parse(data.d);
                    if (!output.success) {
                        hide_loading_modal();
                        $("#join_meeting_auth_error").html('<div class="large-12 columns"><div data-alert class="alert-box alert radius tt-alert msg-stat"> <h3>' + output.message + '</h3></div></div>');
                        $("#join_meeting_auth_error").show();
                    }
                    else {
                        if (output.url != "") {
                            window.location.href = baseUrl + output.url;
                        }
                        else {
                            window.location.href = baseUrl + "?LoginviaMeetingID=true";
                        }


                    }
                },
                error: function (response) {
                    $(".error-msg").hide();
                    console.log(response);
                    $("#server_error_msg").show();
                    hide_loading_modal();
                }
            });
        }

    }
    else {
        var valid = true;
        if (!$("#LoginEmail").val()) {
            $("#EmailError").show();
            if ($("#EmailError").hasClass('displayCls')) {
                $("#EmailError").removeClass('displayCls');
                $("#EmailError").addClass('loginerrors');
            }
            valid = false;
        }
        else {
            var testEmail = /^[A-Z0-9._%+-]+@([A-Z0-9-]+\.)+[A-Z]{2,4}$/i;
            if (!testEmail.test($("#LoginEmail").val()) && $("#LoginEmail").val() != 'admin') {
                $('#EmailError').show()
                if ($("#EmailError").hasClass('displayCls')) {
                    $("#EmailError").removeClass('displayCls');
                    $("#EmailError").addClass('loginerrors');
                }
                valid = false;
            }
            else {
                $("#EmailError").hide();
                if ($("#EmailError").hasClass('loginerrors')) {
                    $("#EmailError").removeClass('loginerrors');
                    $("#EmailError").addClass('displayCls');
                }
            }
        }

        if (!$("#LoginPassword").val()) {
            $("#PasswordError").show();
            if ($("#PasswordError").hasClass('displayCls')) {
                $("#PasswordError").removeClass('displayCls');
                $("#PasswordError").addClass('loginerrors');
            }
            valid = false;
        } else {
            $("#PasswordError").hide();
            if ($("#PasswordError").hasClass('loginerrors')) {
                $("#PasswordError").removeClass('loginerrors');
                $("#PasswordError").addClass('displayCls');
            }
        }

        if (valid) {
            //login ajax
            $.ajax({
                type: "POST",
                url: baseUrl + "Login.aspx/login",
                data: JSON.stringify({
                    email: $("#LoginEmail").val(),
                    password: $("#LoginPassword").val(),
                    passcode: $("#AccessCodeTxt").val(),
                    rememberme: $('#SrembMeLogin').is(":checked"),
                    MeetingID: "",
                    meeting_name: "",
                    meeting_email: ""
                }),
                beforeSend: function () {
                    $(".error").hide();
                    $('.tt-login-form-status').html('<div data-alert="" class="alert alert-success alert-dismissible fade show">           Checking...              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
                },
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    debugger;
                    // console.log(data.d);
                    var output = JSON.parse(data.d);
                    // console.log(output);
                    if (output.success) {
                        //$('.tt-login-form-status').html('<div class="large-12 columns">          <div data-alert class="alert-box success radius tt-alert msg-stat alert-proceed">           <h3>Proceeding...          <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
                        $('.tt-login-form-status').html('<div data-alert="" class="alert alert-primary alert-dismissible fade show">           Proceeding...              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');

                        //if (output.mobile) {
                        //    window.location.href = baseUrl + "m.judgement.aspx";
                        //}
                        //window.location.href = baseUrl + 'default.aspx';
                        if (output.teamtime) {
                            window.location.href = baseUrl + "pages/teamtimetest/teamtime.aspx";
                        }
                        else if (output.anytime) {
                            window.location.href = baseUrl + "pages/Anytime/Anytime.aspx";
                        }
                        else {
                            if (output.mobile) {
                                window.location.reload();
                            }
                            else {
                                window.location.href = baseUrl + "pages/my-projects.aspx";
                            }
                        }
                    } else {
                        $('.tt-login-form-status').html('');
                        $('.tt-login-form-status').html('<div data-alert="" class="alert alert-danger alert-dismissible fade show">           <span>' + output.message + '</span>              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
                        //$('.tt-login-form-status').html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <span>' + output.message + '</span>        </div>      </div>');

                    }
                    $(document).foundation('reflow');
                },
                error: function (response) {
                    //console.log(response);
                    $('.tt-login-form-status').html('<div class="large-12 columns">          <div data-alert class="alert-box alert radius tt-alert msg-stat">           <h3>Something went wrong. Kindly try again.         <a href="#" class="close">&times;</a>          </h3>        </div>      </div>');
                    //$(document).foundation('reflow');
                }
            });
        }
    }// closing for the if else after .clogin click
    return false;
});