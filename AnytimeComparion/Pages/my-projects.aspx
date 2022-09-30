<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="my-projects.aspx.vb" Inherits="AnytimeComparion.my_projects" %>

<%@ Import Namespace="ExpertChoice.Data" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <form runat="server">
        <link href="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.6-rc.0/css/select2.min.css" rel="stylesheet" />
        <div class="my-projects">
            <div class="heading">My projects</div>
            <div class="dataTables_wrapper my-3">
                <div class="container">
                    <div class="row">

                        <div class="col-md-6 text-center text-md-start mb-3 mb-md-0">
                            <asp:Button ID="hdnButton" OnClick="hdnButton_Click" Style="display: none;" runat="server" CommandArgument="FillProjects" />
                            <asp:HiddenField ID="hdnSortingReverse" runat="server" Value="True" />
                            <asp:HiddenField ID="hdnisAdmin" runat="server" Value="False" />
                            <asp:HiddenField ID="hdnProjectsLength" runat="server" Value="0" />
                            <asp:HiddenField ID="hdnPageType" runat="server" Value="" />
                            <asp:HiddenField ID="hdnPairwiseType" runat="server" Value="" />
                            <asp:HiddenField ID="hdnwkgRoleGroupId" runat="server" Value="" />

                            <%-- Start workgroup and table clumn filter --%>
                            <div class="work-group d-flex align-items-center justify-content-md-start justify-content-center">
                                <label class="me-2">Workgroup:</label>
                                <asp:DropDownList ID="select_workgroup" OnSelectedIndexChanged="select_workgroup_SelectedIndexChanged" AutoPostBack="true"
                                    class="js-states form-control" runat="server">
                                </asp:DropDownList>
                                <div class="filter ms-3">
                                    <%--<i class="fas fa-filter"></i>--%>
                                    <% If chkUserRole = True Then %>
                                    <div class="dropdown d-inline-block">
                                        <button type="button" class="btn h-auto header-item mb-0 waves-effect" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                            <i class="fas fa-filter"></i>
                                        </button>
                                        <div class="dropdown-menu dropdown-menu-end">
                                            <a class="dropdown-item" href="javascript:void(0);">
                                                <label class="align-items-center d-flex mb-0">
                                                    <% If Me.isOnline = True Then %>
                                                    <input type="checkbox" onchange="HideShowColumn('project_access')" id="chkisOnline" class="me-2" checked />
                                                    <% Else %>
                                                    <input type="checkbox" onchange="HideShowColumn('project_access')" id="chkisOnline" class="me-2" />
                                                    <% End If %>
                                                    <span>Online</span>
                                                </label>
                                            </a>
                                            <div class="dropdown-divider"></div>
                                            <a class="dropdown-item" href="javascript:void(0);">
                                                <label class="align-items-center d-flex mb-0">
                                                    <% If Me.ProjectStatus = True Then %>
                                                    <input type="checkbox" id="chkProjectStatus" onchange="HideShowColumn('project_status')" class="me-2" checked />
                                                    <% Else %>
                                                    <input type="checkbox" id="chkProjectStatus" onchange="HideShowColumn('project_status')" class="me-2" />
                                                    <% End If %>
                                                    <span>Status</span>
                                                </label>
                                            </a>
                                            <div class="dropdown-divider"></div>
                                            <a class="dropdown-item" href="javascript:void(0);">
                                                <label class="align-items-center d-flex mb-0">
                                                    <% If Me.LastVisited = True Then %>
                                                    <input type="checkbox" id="chkLastVisited" onchange="HideShowColumn('last_access')" class="me-2" checked />
                                                    <% Else %>
                                                    <input type="checkbox" id="chkLastVisited" onchange="HideShowColumn('last_access')" class="me-2" />
                                                    <% End If %>
                                                    <span>Last Access</span>
                                                </label>
                                            </a>
                                            <div class="dropdown-divider"></div>
                                            <a class="dropdown-item" href="javascript:void(0);">
                                                <label class="align-items-center d-flex mb-0">
                                                    <% If Me.LastModify = True Then %>
                                                    <input type="checkbox" id="chkLastModify" onchange="HideShowColumn('last_modified')" class="me-2" checked />
                                                    <% Else %>
                                                    <input type="checkbox" id="chkLastModify" onchange="HideShowColumn('last_modified')" class="me-2" />
                                                    <% End If %>
                                                    <span>Last Modofied</span>
                                                </label>
                                            </a>
                                            <div class="dropdown-divider"></div>
                                            <a class="dropdown-item" href="javascript:void(0);">
                                                <label class="align-items-center d-flex mb-0">
                                                    <% If Me.DateCreated = True Then %>
                                                    <input type="checkbox" id="chkDateCreated" onchange="HideShowColumn('date_created')" class="me-2" checked />
                                                    <% Else %>
                                                    <input type="checkbox" id="chkDateCreated" onchange="HideShowColumn('date_created')" class="me-2" />
                                                    <% End If %>
                                                    <span>Date Created</span>
                                                </label>
                                            </a>
                                            <%--<div class="dropdown-divider"></div>
                                            <a class="dropdown-item" href="javascript:void(0);" runat="server">
                                                <label class="align-items-center d-flex mb-0">
                                                    <input type="checkbox" class="me-2" runat="server" id="chkOveralJudgmentProcess" onchange="HideShowColumn('overal_judgment_process')" checked="checked" />
                                                    <span>Overall Judgment Process</span>
                                                </label>
                                            </a>--%>
                                        </div>
                                    </div>
                                    <% End If %>
                                </div>
                            </div>
                            <%-- End workgroup and table clumn filter --%>
                        </div>
                        <%-- Start page number and search filter --%>
                        <div class="col-md-6 text-md-end text-center">
                            <div class="d-md-flex w-100 justify-content-md-end justify-content-center">
                                <div class="d-flex justify-content-center mb-3 mb-md-0">
                                    <div class="main-pagination d-flex align-items-center">
                                        <label class="me-2">Page:</label>
                                        <ul class="pagination mb-0">
                                            <li class="page-item" onclick="set_Page('True');"><a class="page-link" href="javascript:void(0);"><i class="fas fa-angle-double-left"></i></a></li>
                                            <li class="page-item"><a class="page-link" href="javascript:void(0);" id="pageCount" runat="server">1</a></li>
                                            <li class="page-item" onclick="set_Page('False');"><a class="page-link" href="javascript:void(0);"><i class="fas fa-angle-double-right"></i></a></li>
                                        </ul>
                                    </div>
                                    <div class="show mx-4">
                                        <asp:DropDownList ID="pageOptions" class="form-select w-auto" runat="server">
                                            <asp:ListItem Value="5">5</asp:ListItem>
                                            <asp:ListItem Value="10">10</asp:ListItem>
                                            <asp:ListItem Value="15">15</asp:ListItem>
                                            <asp:ListItem Value="20">20</asp:ListItem>
                                            <asp:ListItem Value="30">30</asp:ListItem>
                                            <asp:ListItem Value="ALL">ALL</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="sorting d-flex d-md-none align-items-center mb-3">
                                    <label class="me-2 text-nowrap">Sort By:</label>
                                    <asp:DropDownList ID="select_workgroup2" class="js-states form-control" runat="server"></asp:DropDownList>
                                </div>
                                <div class="search">
                                    <input type="text" class="form-control" placeholder="Filter results below" id="txtSearch1" />
                                    <asp:TextBox class="form-control" placeholder="Filter results below" ID="txtSearch" Visible="false" runat="server"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                        <%-- End page number and search clumn filter --%>
                    </div>
                </div>
            </div>
            <div class="container">
                <%-- my-projects repeater --%>
                <asp:Repeater ID="rptdataGrid" runat="server">
                    <HeaderTemplate>
                        <table class="table mt-4" id="tblProjects">
                            <thead>
                                <tr>
                                    <th colspan="2" id="tblProjectCount" runat="server" onclick="sort_Project('name');">Projects 
                                        <i class="fas fa-sort ms-2"></i>
                                    </th>
                                    <% If chkUserRole = True Then %>
                                    <th onclick="sort_Project('online');">Online <i class="fas fa-sort ms-2"></i></th>
                                    <% End If %>
                                    <th onclick="sort_Project('status');">Status <i class="fas fa-sort ms-2"></i></th>
                                    <% If chkUserRole = True Then %>
                                    <th onclick="sort_Project('last');">Last Access <i class="fas fa-sort ms-2"></i></th>
                                    <th onclick="sort_Project('modify');">Last Modify <i class="fas fa-sort ms-2"></i></th>
                                    <th onclick="sort_Project('create');">Date Created <i class="fas fa-sort ms-2"></i></th>
                                    <% End If %>
                                    <%--<% End If %>
                                    <% If Me.OveralJudgmentProcess = True Then %>
                                    <th onclick="sort_Project('create');">Overall Judgement Process <i class="fas fa-sort ms-2"></i></th>
                                    <%--<% End If %>--%>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td data-label="Projects Count" colspan="2">
                                <div class="project-details w-100">
                                    <h6><%# Eval("ProjectName") %> <a type="button" id="name_<%# Eval("ID") %>" onclick="collapseToggle('<%# Eval("ID") %>')" class="showHide"><i class="fas fa-angle-down"></i></a></h6>
                                    <p>
                                        <label>Owner:</label><span class="me-2"><%# Eval("UserName") %></span>
                                        <label>Your Role:</label><span>
                                            <% If chkUserRole = True Then %>
                                            Project Manger
                                            <% End If %>
                                        </span>
                                    </p>
                                    <div class="users d-flex justify-content-md-center">
                                        <div class="dropdown">
                                            <a class="ms-3" id="projectUsers" data-bs-toggle="dropdown" aria-expanded="false" href="javascript:void(0);" onclick="getUsersByProject('<%# Eval("ID") %>')">
                                                <i class="fas fa-users me-2"></i><span><%# Eval("userscount") %></span>
                                            </a>
                                            <ul class="dropdown-menu dropdown_list" aria-labelledby="projectUsers" id="projectUsers_<%# Eval("ID") %>">
                                                <%--<li><a class="dropdown-item" href="javascript:void(0);" onclick="DownloadFile('<%# Eval("ID") %>');">Download</a></li>--%>
                                            </ul>
                                        </div>
                                        <div class="dropdown">
                                            <a class="ms-3" id="download" data-bs-toggle="dropdown" aria-expanded="false">
                                                <i class="fas fa-angle-down"></i>
                                            </a>
                                            <ul class="dropdown-menu" aria-labelledby="download">
                                                <li><a class="dropdown-item" href="javascript:void(0);" onclick="DownloadFile('<%# Eval("ID") %>');">Download</a></li>
                                            </ul>
                                        </div>
                                    </div>
                                </div>
                            </td>
                            <% If chkUserRole = True Then %>
                            <td data-label="Online" id="online_<%# Eval("ID") %>" class="show-data">
                                <div class="custom-switch">
                                    <input type="checkbox" class="toggle" id="toggle_<%# Eval("ID") %>" onchange="displayStatus('<%# Eval("ID") %>');"
                                        <%# If(Eval("isOnline").ToString() = "True", "checked", "") %> />
                                    <label for="toggle_<%# Eval("ID") %>">
                                        <span class="on">Yes</span>
                                        <span class="off">No</span>
                                    </label>
                                </div>
                            </td>
                            <% End If %>
                            <td data-label="Status" id="status_<%# Eval("ID") %>" class="show-data">
                                <%--<%# If(Eval("isTeamTime").ToString() = "True",
                                                                $"<div class='columns text-center'>
                                                    <span>'{If(Eval("meetingOwner").ToString().ToLower() = "true", Eval("MeetingOwnerID"), Eval("meetingOwner"))}' TeamTime Evaluation in progress</span>

                                                    </div>", Eval("ProjectStatus"))%>--%>
                                <%# Eval("ProjectStatus") %>
                            </td>
                            <% If chkUserRole = True Then %>
                            <td data-label="Last" id="last_<%# Eval("ID") %>" class="show-data showLastDate"><%# Eval("LastVisited") %></td>
                            <td data-label="Modify" id="modify_<%# Eval("ID") %>" class="show-data showModifyDate"><%# Eval("LastModify") %></td>
                            <td data-label="Create" id="create_<%# Eval("ID") %>" class="show-data showCreateDate"><%# Eval("DateCreated") %></td>
                            <% End If %>
                            <td data-label="Actions" id="actions_<%# Eval("ID") %>" class="show-data">
                                <div>
                                    <a class="start-any" href="javascript:void(0);"
                                        onclick="main_StartAnytime('<%# Eval("ID") %>','<%# Eval("isValidDBVersion") %>','<%# Eval("isOnline") %>','<%# Eval("fCanModifyProject") %>')">
                                        <%--<%# If(Eval("isOnline").ToString() = "True" And Eval("fCanModifyProject").ToString() = "True", "", "disabled") %>>--%>
                                        <i class="fas fa-play-circle me-2"></i>Start Anytime
                                    </a>
                                    <% If chkUserRole = True Then %>
                                    <a class="invite d-block mt-2" href="javascript:void(0);"
                                        onclick="OpenModel('<%# Eval("isTeamTime") %>','<%# Eval("ID") %>','<%# Eval("ProjectName") %>','<%# Eval("isOnline") %>','Invite')">
                                        <i class="fas fa-user-plus me-2"></i>Invite
                                    </a>
                                    <% End If %>
                                </div>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div>

        <%-- Start Invite Model --%>
        <div class="modal fade" id="exampleModalToggle" aria-hidden="true" aria-labelledby="exampleModalToggleLabel" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="exampleModalToggleLabel">Invite Participant</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <input type="hidden" id="hdnProjectId" value="0" />
                        <input type="hidden" id="hdnCopytext" value="" />
                        <div class="offline_warning text-center pt-5 pb-5" id="divprojectStatus">
                            <h4 class="mb-4">Project is offline. Whould you like to make it online?</h4>
                            <div class="btn-group">
                                <button class="btn btn-success" type="button" onclick="showProjectStatus(false, true);">Yes</button>
                                <button class="btn btn-danger" type="button" onclick="showProjectStatus(false,false)">No</button>
                            </div>
                        </div>
                        <div class="body_content" id="divgenerateLinks">
                            <h3 class="mb-4">Anytime Links</h3>
                            <div class="link_type">
                                <div class="radiobuttons">
                                    <div class="rdio rdio-primary radio-inline">
                                        <input name="radio" value="1" id="radio1" type="radio" onchange="generateLinkbyType('1','disableCheckboxes');">
                                        <label for="radio1">Link for Anonymous Evaluation</label>
                                    </div>
                                    <div class="radio_check">
                                        <div class="rdio rdio-primary radio-inline">
                                            <input name="radio" value="2" id="radio2" type="radio" onchange="generateLinkbyType('2');" checked>
                                            <label for="radio2">Link for Signing In and Evaluation</label>
                                            <div class="checkbox_items" id="divCheckboxes">
                                                <div class="cust-checkbox">
                                                    <div class="custom-checkbox">
                                                        <input name="signup_email" class="checkbox-custom" id="signup_email" onchange="disableCheckbox('signup_email');" value="e" type="checkbox" checked>
                                                        <label class="checkbox-custom-label" for="signup_email">Email </label>
                                                    </div>
                                                    <div class="custom-checkbox">
                                                        <input name="signup_name" class="checkbox-custom" id="signup_name" onchange="disableCheckbox('signup_name');" value="n" type="checkbox" checked>
                                                        <label class="checkbox-custom-label" for="signup_name">Name </label>
                                                    </div>
                                                    <div class="custom-checkbox">
                                                        <input name="signup_password" class="checkbox-custom" id="signup_password" onchange="disableCheckbox('signup_password');" value="p" type="checkbox" checked>
                                                        <label class="checkbox-custom-label" for="signup_password">Password </label>
                                                    </div>
                                                </div>
                                                <div class="cust-checkbox">
                                                    <div class="custom-checkbox">
                                                        <input name="req_signup_email" class="checkbox-custom" id="req_signup_email" onchange="generateLinkbyType('2');" value="e" type="checkbox">
                                                        <label class="checkbox-custom-label" for="req_signup_email">Required </label>
                                                    </div>
                                                    <div class="custom-checkbox">
                                                        <input name="req_signup_name" class="checkbox-custom" id="req_signup_name" onchange="generateLinkbyType('2');" value="n" type="checkbox">
                                                        <label class="checkbox-custom-label" for="req_signup_name">Required </label>
                                                    </div>
                                                    <div class="custom-checkbox">
                                                        <input name="req_signup_password" class="checkbox-custom" id="req_signup_password" onchange="generateLinkbyType('2');" value="p" type="checkbox">
                                                        <label class="checkbox-custom-label" for="req_signup_password">Required </label>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="rdio rdio-primary radio-inline">
                                        <input name="radio" value="3" id="radio3" type="radio" onchange="generateLinkbyType('3');">
                                        <label for="radio3">Link for going to normal Comparion login screen</label>
                                    </div>
                                </div>
                            </div>
                            <div class="link_area">
                                <div class="row">
                                    <div class="col-lg-2">
                                        <span><strong>Genral Link</strong></span>
                                    </div>
                                    <div class="col-lg-7">
                                        <span id="Spangeneral_link">
                                            <%--file:///Users/sumitkumar/Downloads/OFFICE/exper_choice/HTML/index.html--%>
                                        </span>
                                    </div>
                                    <div class="col-lg-3">
                                        <button class="btn btn-clipboard" id="btnGenertedLink" type="button" onclick="copyToClipboard('Spangeneral_link','btnGenertedLink')">Copy to Clipboard</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div id="divuserLinks">
                            <h3 class="mb-4">Anytime Links</h3>
                            <div class="table-responsive link_table">
                                <table class="table table-striped table-hover" id="tblUsersLink">
                                    <thead>
                                        <tr>
                                            <th>Email</th>
                                            <th>Hash Tag</th>
                                            <th></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <%--<tr>
                                            <td>testuser@test.com</td>
                                            <td class="text-break">file:///Users/sumitkumar/Downloads/OFFICE/exper_choice/HTML/index.htmlfile:///Users/sumitkumar/Downloads/OFFICE/exper_choice/HTML/index.html</td>
                                            <td>
                                                <button type="button" class="btn btn-clipboard text-nowrap">Copy to Clipboard</button></td>
                                        </tr>--%>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <%--<button class="foot_btn" data-bs-target="#exampleModalToggle2" data-bs-toggle="modal" data-bs-dismiss="modal">Go to Participant Specific Link</button>--%>
                        <button type="button" class="foot_btn" id="btnSpecificLink" onclick="HideShowInviteDiv('SpecificLink');">Go to Participant Specific Link</button>
                        <button type="button" class="foot_btn" id="btnGneralLink" onclick="HideShowInviteDiv('GeneralLink');">Go to Genral Links</button>
                    </div>
                </div>
            </div>
        </div>
        <%-- End Invite Model --%>

        <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
        <script src="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.6-rc.0/js/select2.min.js"></script>
        <script src="../CustomScripts/my-projects.js"></script>
    </form>
</asp:Content>
