var IsAdmin = 'False', ProjectStatus = 'False', isProjectTeamtime = 'False', is_Online = 'False', is_general_link = 'False', isMobile = 'False',
    isMobileDevice = 'False', is_anytime = 'False', is_Teamtime = 'False';
var general_link = '', project_name = '', invite_message = '', permission_level = '1', signup_mode = 'enp', req_signup_mode = 'enp';
var project_id = '0', scrollbarWidth = 0;
var hash_links = [];
var signup = [{ 'email': 'e', 'name': 'n', 'password': 'p', 'message': '' }];
var req_signup = { 'email': 'e', 'name': 'n', 'password': 'p' };

$(document).ready(function () {
    IsAdmin = $('#MainContent_hdnisAdmin').val();
    if (IsAdmin != undefined && IsAdmin != null && IsAdmin == 'True') {
        HideShowColumn('project_access');
        HideShowColumn('project_status');
        HideShowColumn('last_access');
        HideShowColumn('last_modified');
        HideShowColumn('date_created');
        //HideShowColumn('overal_judgment_process');
    }
});

function getScrollbarWidth() {
    if (scrollbarWidth == 0) {
        var outerDiv = $("<div>").css({ visibility: "hidden", width: 100, overflow: "scroll" }).appendTo("body");
        scrollbarWidth = outerDiv.width() - $("<div>").css({ width: "100%" }).appendTo(outerDiv).outerWidth();
        outerDiv.remove();
    }
    return scrollbarWidth;
}

function isMobile() {
    isMobile = isMobileDevice == null ? false : isMobileDevice;
    if (isMobileDevice == null) {
        try {
            if (sessionStorage.desktop) // desktop storage 
                isMobile = false;
            else if (localStorage.mobile) // mobile storage
                isMobile = true;

            // alternative
            var mobile = ["iphone", "ipad", "android", "blackberry", "nokia", "opera mini", "windows mobile", "windows phone", "iemobile"];
            for (var i in mobile) if (navigator.userAgent.toLowerCase().indexOf(mobile[i].toLowerCase()) > 0) isMobile = true;
        }
        catch (e) {
        }

        try {
            var deviceWidth = $("body").width();
            deviceWidth += (deviceWidth < 1025 && document.body.scrollHeight > document.body.clientHeight) ? getScrollbarWidth() : 0;
            if (!isMobile) {
                if (deviceWidth < 1025)
                    isMobile = true;
                else {
                    isMobile = false;
                }
            }

            //for project list to display projects on tablets
            var hdnProjectsLength = $('#MainContent_hdnProjectsLength').val();
            if (parseInt(hdnProjectsLength) > 0) {
                if (deviceWidth > 640)
                    isMobile = false;
            }

            if (is_anytime) {
                var page_type = $('#MainContent_hdnPageType').val();
                var pairwise_type = $('#MainContent_hdnPairwiseType').val();
                if (page_type == "atShowGlobalResults" || pairwise_type == "ptGraphical") {
                    if (deviceWidth < 1025)
                        isMobile = true;
                    else {
                        isMobile = false;
                    }
                }
            }
        }
        catch (e) {

        }
        isMobileDevice = isMobile;
    }
    return isMobile;
}

$("#MainContent_select_workgroup").select2({
    placeholder: "Select",
    allowClear: true
});
$("#MainContent_select_workgroup2").select2({
    placeholder: "Select",
    allowClear: true
});

//Convert seconds to last access date
$('.showLastDate').each(function () {
    var date = $(this).text();
    var thisdate = new Date(1000 * date);
    var localdate = thisdate.getTimezoneOffset();
    thisdate.setHours(thisdate.getHours() + (localdate / 60));
    $(this).html('<div class="medium-12 small-5 columns">' + thisdate.toLocaleDateString() + '</div> <div class="medium-12 small-7 columns">' + thisdate.toLocaleTimeString() + '</div>');
});
//Convert seconds to last modify date
$('.showModifyDate').each(function () {
    var date = $(this).text();
    var thisdate = new Date(1000 * date);
    var localdate = thisdate.getTimezoneOffset();
    thisdate.setHours(thisdate.getHours() + (localdate / 60));
    $(this).html('<div class="medium-12 small-5 columns">' + thisdate.toLocaleDateString() + '</div> <div class="medium-12 small-7 columns">' + thisdate.toLocaleTimeString() + '</div>');
});
//Convert seconds to created date
$('.showCreateDate').each(function () {
    var date = $(this).text();
    var thisdate = new Date(1000 * date);
    var localdate = thisdate.getTimezoneOffset();
    thisdate.setHours(thisdate.getHours() + (localdate / 60));
    $(this).html('<div class="medium-12 small-5 columns">' + thisdate.toLocaleDateString() + '</div> <div class="medium-12 small-7 columns">' + thisdate.toLocaleTimeString() + '</div>');
});

//Download project file
function DownloadFile(ProjectId) {
    var wnd = window.open(baseUrl + "Pages/download.aspx?projectid=" + ProjectId + "&ext=ahps");
    //show_loading_modal();
    $timeout(function () {
        wnd.close();
        //hide_loading_modal();
    }, 5000);
}

//Get all users by project id
function getUsersByProject(projectID) {
    $.ajax({
        type: "POST",
        url: baseUrl + "pages/my-projects.aspx/getUsersByProject",
        data: JSON.stringify({
            projectID: projectID
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $('#projectUsers_' + projectID).empty();
            var html = '';
            for (let user of data.d) {
                var a = user;
                html += '<li><a class="dropdown-item" href="javascript:void(0);">' + a.UserEMail + '</a></li>';
            }
            $('#projectUsers_' + projectID).append(html);
        },
        error: function (response) {
        }
    });
}

//Change project status like online or offline
function displayStatus(projectID) {
    $.ajax({
        type: "POST",
        url: baseUrl + "pages/my-projects.aspx/displayStatus",
        data: JSON.stringify({
            projid: projectID
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            window.location.reload();
        },
        error: function (response) {
        }
    });
}

//sorting on change of sort by dropdown for mobile device
$('#MainContent_select_workgroup2').change(function () {
    sort_Project($(this).val());
});

//Save project sorting column wise
function sort_Project(request) {
    debugger;
    var columnname = '0';
    var reverse = $('#MainContent_hdnSortingReverse').val();
    switch (request) {
        case "create":
            reverse = reverse == 'False' ? 'True' : 'False'; columnname = '8';
            break;
        case "modify":
            reverse = reverse == 'False' ? 'True' : 'False'; columnname = '7';
            break;
        case "status":
            reverse = reverse == 'False' ? 'True' : 'False'; columnname = '3';
            break;
        case "online":
            reverse = reverse == 'False' ? 'True' : 'False'; columnname = '4';
            break;
        case "name":
            reverse = reverse == 'False' ? 'True' : 'False'; columnname = '1';
            break;
        case "last":
            reverse = reverse == 'False' ? 'True' : 'False'; columnname = '10';
            break;
    }
    $.ajax({
        type: "POST",
        url: baseUrl + "pages/my-projects.aspx/save_sort",
        data: JSON.stringify({
            column: columnname,
            reverse: reverse
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $("#MainContent_hdnButton").trigger('click');
        },
        error: function (response) {
        }
    });
}

//Save chnaged page number
function set_Page(is_left) {
    $.ajax({
        type: "POST",
        url: baseUrl + "pages/my-projects.aspx/set_Page",
        data: JSON.stringify({
            is_left: is_left
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var output = JSON.parse(data.d);
            output.currentPage = output.currentPage == 0 ? 1 : output.currentPage;
            $('#MainContent_pageCount').text(output.currentPage + '/' + output.numberOfPages);
        },
        error: function (response) {
        }
    });
}

//Save changes page size
$('#MainContent_pageOptions').change(function () {
    $.ajax({
        type: "POST",
        url: baseUrl + "pages/my-projects.aspx/savePageSize",
        data: JSON.stringify({
            pageSize: $(this).val()
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            window.location.reload();
        },
        error: function (response) {
        }
    });
});

//Collapse table for mobile or small devices
function collapseToggle(ProjectId) {
    if ($('#online_' + ProjectId).hasClass('active')) {
        $('#online_' + ProjectId).removeClass('active');
    }
    else {
        $('#online_' + ProjectId).addClass('active');
    }
    if ($('#status_' + ProjectId).hasClass('active')) {
        $('#status_' + ProjectId).removeClass('active');
    }
    else {
        $('#status_' + ProjectId).addClass('active');
    }
    if ($('#actions_' + ProjectId).hasClass('active')) {
        $('#actions_' + ProjectId).removeClass('active');
    }
    else {
        $('#actions_' + ProjectId).addClass('active');
    }
    if ($('#name_' + ProjectId).hasClass('active')) {
        $('#name_' + ProjectId).removeClass('active');
    }
    else {
        $('#name_' + ProjectId).addClass('active');
    }
}

//Hide and show table column as per changes from filter dropdown just after workgroups
function HideShowColumn(column) {
    /*hide and show online column*/
    if (column != undefined && column != null && column == 'project_access') {
        if ($('#chkisOnline').is(':checked')) {
            $('#tblProjects thead tr th:eq(1)').show();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(1)').show();
            });
            //$('#tblProjects tbody tr td:eq(1)').show();
            RememberFilter(true, 'project_access');
        }
        else {
            $('#tblProjects thead tr th:eq(1)').hide();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(1)').hide();
            });
            RememberFilter(false, 'project_access');
        }
    }
    /*hide and show project status column*/
    if (column != undefined && column != null && column == 'project_status') {
        if ($('#chkProjectStatus').is(':checked')) {
            $('#tblProjects thead tr th:eq(2)').show();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(2)').show();
            });
            RememberFilter(true, column);
        }
        else {
            $('#tblProjects thead tr th:eq(2)').hide();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(2)').hide();
            });
            RememberFilter(false, column);
        }
    }
    /*hide and show project last access column*/
    if (column != undefined && column != null && column == 'last_access') {
        if ($('#chkLastVisited').is(':checked')) {
            $('#tblProjects thead tr th:eq(3)').show();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(3)').show();
            });
            RememberFilter(true, column);
        }
        else {
            $('#tblProjects thead tr th:eq(3)').hide();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(3)').hide();
            });
            RememberFilter(false, column);
        }
    }
    /*hide and show project last modified column*/
    if (column != undefined && column != null && column == 'last_modified') {
        if ($('#chkLastModify').is(':checked')) {
            $('#tblProjects thead tr th:eq(4)').show();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(4)').show();
            });
            RememberFilter(true, column);
        }
        else {
            $('#tblProjects thead tr th:eq(4)').hide();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(4)').hide();
            });
            RememberFilter(false, column);
        }
    }
    /*hide and show project date created column*/
    if (column != undefined && column != null && column == 'date_created') {
        if ($('#chkDateCreated').is(':checked')) {
            $('#tblProjects thead tr th:eq(5)').show();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(5)').show();
            });
            RememberFilter(true, column);
        }
        else {
            $('#tblProjects thead tr th:eq(5)').hide();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(5)').hide();
            });
            RememberFilter(false, column);
        }
    }
    /*hide and show overal judgment process column*/
    if (column != undefined && column != null && column == 'overal_judgment_process') {
        if ($('#chkDateCreated').is(':checked')) {
            $('#tblProjects thead tr th:eq(6)').show();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(6)').show();
            });
            RememberFilter(true, column);
        }
        else {
            $('#tblProjects thead tr th:eq(6)').hide();
            $('#tblProjects > tbody > tr').each(function () {
                $(this).find('td:eq(6)').hide();
            });
            RememberFilter(false, column);
        }
    }
}

//save changed filter for display columns in table
function RememberFilter(model, filtername) {
    $.ajax({
        type: "POST",
        url: baseUrl + "pages/my-projects.aspx/RememberFilter",
        data: JSON.stringify({
            filter: model,
            filtername: filtername
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
        },
        error: function (response) {
        }
    });
}

//Search project by project name from table
$('#txtSearch1').keyup(function () {
    //$("#MainContent_hdnButton").trigger('click');
    var value = $(this).val();
    $("#tblProjects > tbody > tr").each(function (index) {
        $row = $(this);
        var id = $row.find("td:first").text().trim();
        if (id.toLowerCase().includes(value.toLowerCase())) {
            $row.show();
        }
        else {
            $row.hide();
        }
    });
});

/*============================================================Invite============================================================*/
var general_link_switch = { 'radio': '2', 'mradio': '2' };
//generate links project wise for invite
function generateLink(is_teamtime, projectID, projectName, is_online) {
    general_link = "";
    ProjectStatus = is_online == "False";
    //Current project id
    project_id = projectID;

    setTimeout(function () {
        project_name = projectName;
    }, 600);
    isProjectTeamtime = is_teamtime;
    setTimeout(function () {
        is_teamtime = is_teamtime;
        is_general_link = true;
    }, 1000);

    $.ajax({
        type: "POST",
        url: baseUrl + "pages/my-projects.aspx/GenerateLink",
        data: JSON.stringify({
            is_teamtime: is_teamtime,
            projID: projectID
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            project_id = projectID;
            is_Teamtime = is_teamtime;
            hash_links = response.d;
            var html = '';
            for (var i = 0; i < hash_links.length; i++) {
                html += '<tr>';
                html += '<td>' + hash_links[i][0] + '</td>';
                html += '<td><span id="SpanUser_link_' + i + '">' + hash_links[i][1] + '</span></td>';
                var id = '#SpanUser_link_' + i;
                html += '<td><button type="button" class="btn btn-clipboard text-nowrap" id="btnUserLink_' + i + '" onclick=copyToClipboard("' + hash_links[i][1] + '","btnUserLink_' + i + '");>Copy to Clipboard</button></td>';
                html += '</tr>';
            }
            $('#tblUsersLink tbody').empty();
            $('#tblUsersLink tbody').append(html);

            invite_message = "Please provide the following information to sign up or login and start evaluation «<b>" + projectName + "</b>»";
            if (isMobile) {
                generateGeneralLink(general_link_switch.mradio);
            }
            else {
                generateGeneralLink(general_link_switch.radio);
            }
        },
        error: function (response) {
        }
    });
}

//generate links according to mobile or device
function generateGeneralLink(tmode) {
    $('#Spangeneral_link').text('');
    general_link_switch.radio = tmode;
    general_link_switch.mradio = tmode;
    //angular.element("#general_link_copy_btn").html("Copy to Clipboard");
    if (general_link_switch.radio == tmode) {
        //permission_level = angular.element("#SelectPermissionLevel").val();
    } else {
        //permission_level = angular.element("#SelectMobilePermissionLevel").val();
    }

    signup_mode = "";
    req_signup_mode = "";
    signup.email = $('#signup_email').is(':checked');
    signup.name = $('#signup_name').is(':checked');
    signup.password = $('#signup_password').is(':checked');

    req_signup.email = $('#req_signup_email').is(':checked');
    req_signup.name = $('#req_signup_name').is(':checked');
    req_signup.password = $('#req_signup_password').is(':checked');

    //signup
    if (signup.email)
        signup_mode = "e";
    else
        signup_mode = signup_mode.replace("e", "");

    if (signup.name)
        signup_mode += "n";
    else
        signup_mode = signup_mode.replace("n", "");

    if (signup.password)
        signup_mode += "p";
    else
        signup_mode = signup_mode.replace("p", "");
    //req
    if (req_signup.email == true)
        req_signup_mode = "e";
    else
        req_signup_mode = req_signup_mode.replace("e", "");

    if (req_signup.name == true)
        req_signup_mode += "n";
    else
        req_signup_mode = req_signup_mode.replace("n", "");

    if (req_signup.password == true)
        req_signup_mode += "p";
    else
        req_signup_mode = req_signup_mode.replace("p", "");
    req_signup_mode = req_signup_mode;

    var groupID = -1; // for combined groups: unfinished
    var wkgid = $('#MainContent_hdnwkgRoleGroupId').val();
    wkgid = wkgid == undefined || null ? '0' : wkgid;
    wkgid = wkgid == '' ? '0' : wkgid;
    $.ajax({
        type: "POST",
        url: baseUrl + "pages/my-projects.aspx/getGeneralLink",
        data: JSON.stringify({
            tmode: tmode,
            projectID: project_id,
            signupmode: signup_mode,
            otherparams: req_signup_mode,
            combinedGroupID: groupID,
            wkgRoleGroupId: parseInt(permission_level)
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            setTimeout(function () {
                general_link = response.d;
                $('#Spangeneral_link').text(general_link);
            }, 100);
        },
        error: function (response) {
        }
    });
}

/*Copy to clipboard link*/
function copyToClipboard(text, btnId) {
    if (text == 'Spangeneral_link')
        $('#hdnCopytext').val($('#Spangeneral_link').text());
    else
        $('#hdnCopytext').val(text);

    /* Get the text field */
    var copyText = document.getElementById("hdnCopytext");

    /* Select the text field */
    copyText.select();
    //copyText.setSelectionRange(0, 99999); /* For mobile devices */

    /* Copy the text inside the text field */
    navigator.clipboard.writeText(copyText.value);

    $('#hdnCopytext').val('');
    /* Alert the copied text */
    //alert("Copied the text: " + copyText.value);
    if ($('#' + btnId).text() == 'Copy to Clipboard') {
        $('#' + btnId).text('Copied');
    }
    else {
        $('#' + btnId).text('Copy to Clipboard')
    }
}

//open popup
function OpenModel(is_teamtime, projectID, projectName, is_online, modelType) {
    is_Teamtime = is_teamtime;
    project_id = projectID;
    project_name = projectName;
    is_Online = is_online;
    //Open reset password model
    if (modelType != undefined && modelType != null && modelType == 'Invite') {
        $('#exampleModalToggle').modal('show');
        $('#hdnProjectId').val(projectID);
        $('#divuserLinks').hide();
        $('#btnGneralLink').hide();
        if (is_online == 'True') {
            generateLink(is_teamtime, projectID, projectName, is_online);
            $('#divprojectStatus').hide();
            $('#divgenerateLinks').show();
            $('#btnSpecificLink').show();
        }
        else {
            $('#divprojectStatus').show();
            $('#divgenerateLinks').hide();
            $('#btnSpecificLink').hide();
        }
    }
}

function HideShowInviteDiv(type) {
    if (type != undefined && type != null && type == 'SpecificLink') {
        $('#divgenerateLinks').hide();
        $('#divuserLinks').show();
        $('#btnSpecificLink').hide();
        $('#btnGneralLink').show();
    }
    else if (type != undefined && type != null && type == 'GeneralLink') {
        $('#divgenerateLinks').show();
        $('#divuserLinks').hide();
        $('#btnSpecificLink').show();
        $('#btnGneralLink').hide();
    }
}

//close popup
function CloseModel(modelType) {
    //Close reset password model
    if (modelType != undefined && modelType != null && modelType == 'Invite') {
        $('#exampleModalToggle').modal('hide');
        project_id = 0;
        $('#hdnProjectId').val('0');
        signup_mode = "";
        req_signup_mode = "";
        general_link = '';
        is_Teamtime = 'False';
        hash_links = [];
        invite_message = '';
    }
}

//if project status is offline and user click on yes thn it'll update the project status to online
function showProjectStatus(value, turn_online) {
    debugger;
    ProjectStatus = value;
    if (turn_online) {
        displayStatus($('#hdnProjectId').val());
    }
    if (!turn_online) {
        generateLink(is_Teamtime, project_id, project_name, is_Online);
        $('#divprojectStatus').hide();
        $('#divgenerateLinks').show();
        $('#btnSpecificLink').show();
    }
}

//disable email, name, and password checkbox as per selection of type on invite popup
function generateLinkbyType(mode, chk) {
    if (mode != undefined && mode != null && (mode == '1' || mode == '2' || mode == '3')) {
        if (chk != undefined && chk != null && chk == 'disableCheckboxes') {
            $('div#divCheckboxes :input').attr("disabled", true);
        }
        else
            $('div#divCheckboxes :input').attr("disabled", false);
        generateGeneralLink(mode);
    }
}

//enable / disable required checkbox of email, name, and password
function disableCheckbox(id) {
    if (id != undefined && id != null && id != '') {
        if (id == 'signup_email') {
            if ($('#signup_email').is(':checked')) {
                $('#req_signup_email').prop('disabled', false);
            }
            else {
                $('#req_signup_email').prop('disabled', true);
            }
        }
        if (id == 'signup_name') {
            if ($('#signup_name').is(':checked')) {
                $('#req_signup_name').prop('disabled', false);
            }
            else {
                $('#req_signup_name').prop('disabled', true);
            }
        }
        if (id == 'signup_password') {
            if ($('#signup_password').is(':checked')) {
                $('#req_signup_password').prop('disabled', false);
            }
            else {
                $('#req_signup_password').prop('disabled', true);
            }
        }
        generateGeneralLink('2');
    }
}

//start anytime function
var active_project = {};
function main_StartAnytime(project_id, is_valid, is_online, can_modify) {
    is_valid = is_valid == "True" ? true : false;
    if (is_online == "False" && can_modify == "False") return false;
    if (!is_valid) {
        //alert("This project has different version from the current database version.");
    }

    $.ajax({
        type: "POST",
        url: baseUrl + "login.aspx/StartAnytime",
        async: false,
        data: JSON.stringify({
            projID: project_id
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            var output = JSON.parse(response.d);
            active_project = output;

            if (output.start_anytime) {
                setTimeout(function () {
                    window.location.href = '/pages/Anytime/Anytime.aspx', 200
                }, 200);

            }
        },
        error: function (response) {
        }
    });
}