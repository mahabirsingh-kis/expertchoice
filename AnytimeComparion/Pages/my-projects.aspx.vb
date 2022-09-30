Imports ExpertChoice.Data
Imports ExpertChoice.Web
Imports AnytimeComparion.Pages.external_classes
Imports System.Web.Services
Imports System.Web.Script.Serialization
Imports ExpertChoice.Service

Public Class my_projects
    Inherits System.Web.UI.Page
    Public App As clsComparionCore
    Public Shared currentPage As Integer = 1
    Public Shared numberOfPages As Integer = 0
    Public isOnline As Boolean = True
    Public ProjectStatus As Boolean = True
    Public LastVisited As Boolean = False
    Public LastModify As Boolean = False
    Public DateCreated As Boolean = False
    Public OveralJudgmentProcess As Boolean = False
    Public chkUserRole As Boolean = False
    Public ProjStatus As String = ""

#Region "Page load section"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        App = CType(Session("App"), clsComparionCore)

        If App.ActiveUser Is Nothing Then
            Response.Redirect("~/login.aspx")
        End If

        Session("topHeader") = False

        If HttpContext.Current.Session("ProjectListSize") Is Nothing Then
            HttpContext.Current.Session("ProjectListSize") = "ALL"
        End If

        Dim projects As WorkgroupsandProjects = getWorkgroupsandProjects(0)
        projects.clsProjects = ProjectsSorting(projects.clsProjects)
        chkUserRole = projects.isPM
        ProjStatus = projects.project_status
        'tblProjectCount.InnerText = $"Projects {projects.clsProjects.Count()} "
        Dim newListItem As ListItem
        If projects.workgroups IsNot Nothing Then
            For i As Integer = 0 To projects.workgroups.Count - 1
                newListItem = New ListItem(projects.workgroups(i)(1), projects.workgroups(i)(0))
                select_workgroup.Items.Add(newListItem)
            Next
        End If

        If projects.isPM = True Then
            isOnline = projects.project_access
            ProjectStatus = projects.project_status
            LastVisited = projects.last_access
            LastModify = projects.last_modified
            DateCreated = projects.date_created
            OveralJudgmentProcess = projects.overal_judgment_process
        End If

        If projects.sort IsNot Nothing Then
            hdnSortingReverse.Value = projects.sort(1)
        End If
        hdnisAdmin.Value = projects.isPM
        hdnProjectsLength.Value = projects.clsProjects.Count()
        hdnwkgRoleGroupId.Value = projects.role_workgroup_id

        If projects.isPM = True Then
            select_workgroup2.Items.Add(New ListItem("Project Name", "name"))
            select_workgroup2.Items.Add(New ListItem("Online", "online"))
            select_workgroup2.Items.Add(New ListItem("Status", "status"))
            select_workgroup2.Items.Add(New ListItem("Last Access", "last"))
            select_workgroup2.Items.Add(New ListItem("Last Modified", "modify"))
            select_workgroup2.Items.Add(New ListItem("Date Created", "create"))
        Else
            select_workgroup2.Items.Add(New ListItem("Project Name", "name"))
            select_workgroup2.Items.Add(New ListItem("Online", "online"))
            select_workgroup2.Items.Add(New ListItem("Status", "status"))
            select_workgroup2.Items.Add(New ListItem("Last Access", "last"))
        End If

        Dim item = pageOptions.Items.FindByText(projects.pageSize)
        If item IsNot Nothing Then
            item.Selected = True
        Else
            item = pageOptions.Items.FindByText("ALL")
            item.Selected = True
        End If

        rptdataGrid.DataSource = projects.clsProjects
        rptdataGrid.DataBind()
        SiteMaster.Header = False
        SiteMaster.Footer = False
        pageCount.InnerText = "1/1"
        If projects.pageSize <> "ALL" Then
            numberOfPages = projects.clsProjects.Count / projects.pageSize
            numberOfPages = If(numberOfPages = 0, 1, numberOfPages)
            pageCount.InnerText = "1/" + numberOfPages.ToString()
        Else
            numberOfPages = 1
        End If
    End Sub

    Public Shared Function getWorkgroupsandProjects(ByVal Optional ecProjectStatus As Integer = 0) As WorkgroupsandProjects
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)

        If ecProjectStatus = -1 Then
            ecProjectStatus = CInt(context.Session(Constants.Sess_Project_Status))
        End If

        Dim ProjectStatus = CType(ecProjectStatus, ecProjectStatus)
        context.Session(Constants.Sess_Project_Status) = ProjectStatus

        If App.ActiveUser IsNot Nothing Then
            Dim Workgroups = App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups)
            Dim CurrentWorkgroup = New Object(1) {}

            Try

                If App.ActiveWorkgroup Is Nothing Then
                    Dim LastVisitedWGID = App.ActiveUser.Session.WorkgroupID
                    CurrentWorkgroup(0) = LastVisitedWGID

                    If CurrentWorkgroup(0) Is Nothing Then
                        CurrentWorkgroup(0) = Workgroups(0).ID
                    End If
                Else
                    CurrentWorkgroup(0) = App.ActiveWorkgroup.ID
                End If

            Catch
            End Try

            Dim WorkgroupsandProjects = New List(Of Object())()

            Try

                For i As Integer = 0 To Workgroups.Count - 1
                    Dim msg = ""
                    Dim IfAvailable = App.CheckLicense(Workgroups(i), msg, True)

                    If IfAvailable Then
                        Dim TempWorkGroup = New Object(1) {}
                        TempWorkGroup(0) = Workgroups(i).ID
                        TempWorkGroup(1) = Workgroups(i).Name
                        WorkgroupsandProjects.Add(TempWorkGroup)

                        If Convert.ToInt32(CurrentWorkgroup(0)) = Workgroups(i).ID Then
                            CurrentWorkgroup(1) = Workgroups(i).ID
                        End If
                    End If
                Next

            Catch
            End Try

            Dim CurrentProject = -1

            If App.ActiveProject IsNot Nothing Then
                CurrentProject = App.ActiveProject.ID
            End If

            Dim Projects = App.DBProjectsByWorkgroupID(Convert.ToInt32(CurrentWorkgroup(0)))
            Dim ProjecstList = New List(Of Projects)
            Dim ProjectInfo = New String(Projects.Count - 1)() {}
            Dim UserIsPM = New Boolean()
            Dim fCanManageAnyDecision = App.CanUserDoAction(ecActionType.at_alManageAnyModel, App.ActiveUserWorkgroup)
            Dim fCanSeeAllDecision = App.CanUserDoAction(ecActionType.at_alViewAllModels, App.ActiveUserWorkgroup)
            Dim debugprojects As Integer = 0
            Dim isAdmin = False
            Dim activeWorkgroup = App.ActiveWorkgroup
            isAdmin = checkRoleGroup

            For i As Integer = 0 To Projects.Count - 1
                Dim fCanModifyProject = App.CanUserModifyProject(App.ActiveUser.UserID, Projects(i).ID, App.ActiveUserWorkgroup, App.ActiveWorkspace)

                If isAdmin OrElse fCanModifyProject Then

                    If Projects(i).isMarkedAsDeleted Then

                        If Convert.ToInt32(ProjectStatus) = 4 Then
                            Dim meetingOwner = ""
                            If Projects(i).MeetingOwner IsNot Nothing Then meetingOwner = Projects(i).MeetingOwner.UserEmail.ToString()
                            ProjectInfo(i) = New String(12) {}
                            ProjectInfo(i)(0) = Projects(i).ID.ToString()
                            ProjectInfo(i)(1) = Projects(i).ProjectName
                            ProjectInfo(i)(2) = App.DBUserByID(Projects(i).OwnerID).UserName
                            ProjectInfo(i)(3) = Projects(i).isTeamTime.ToString()
                            ProjectInfo(i)(4) = Projects(i).isOnline.ToString()
                            ProjectInfo(i)(5) = Projects(i).MeetingOwnerID.ToString()
                            ProjectInfo(i)(6) = meetingOwner
                            ProjectInfo(i)(7) = Projects(i).LastModify.Value.Subtract(New DateTime(1970, 1, 1)).TotalSeconds.ToString()
                            ProjectInfo(i)(8) = Projects(i).Created.Value.Subtract(New DateTime(1970, 1, 1)).TotalSeconds.ToString()
                            Dim userscount = App.DBUsersByProjectID(Projects(i).ID).Count
                            ProjectInfo(i)(9) = userscount.ToString()
                            ProjectInfo(i)(10) = If(Projects(i).LastVisited.HasValue, Projects(i).LastVisited.Value.Subtract(New DateTime(1970, 1, 1)).TotalSeconds.ToString(), "")
                            ProjectInfo(i)(11) = fCanModifyProject.ToString()
                            ProjectInfo(i)(12) = Projects(i).isValidDBVersion.ToString()
                        End If
                    Else
                        debugprojects += 1

                        Try

                            If Projects(i).ProjectStatus = ProjectStatus AndAlso (isAdmin OrElse clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, Projects(i).ID, App.Workspaces) IsNot Nothing) AndAlso Projects(i).ProjectStatus = ProjectStatus AndAlso Not Projects(i).isMarkedAsDeleted Then
                                Dim meetingOwner = ""
                                If Projects(i).MeetingOwner IsNot Nothing Then meetingOwner = Projects(i).MeetingOwner.UserEmail.ToString()
                                ProjectInfo(i) = New String(12) {}
                                ProjectInfo(i)(0) = Projects(i).ID.ToString()
                                ProjectInfo(i)(1) = Projects(i).ProjectName
                                ProjectInfo(i)(2) = App.DBUserByID(Projects(i).OwnerID).UserName
                                ProjectInfo(i)(3) = Projects(i).isTeamTime.ToString()
                                ProjectInfo(i)(4) = Projects(i).isOnline.ToString()
                                ProjectInfo(i)(5) = Projects(i).MeetingOwnerID.ToString()
                                ProjectInfo(i)(6) = meetingOwner
                                ProjectInfo(i)(7) = Projects(i).LastModify.Value.Subtract(New DateTime(1970, 1, 1)).TotalSeconds.ToString()
                                ProjectInfo(i)(8) = Projects(i).Created.Value.Subtract(New DateTime(1970, 1, 1)).TotalSeconds.ToString()
                                Dim userscount = App.DBUsersByProjectID(Projects(i).ID).Count
                                ProjectInfo(i)(9) = userscount.ToString()
                                ProjectInfo(i)(10) = If(Projects(i).LastVisited.HasValue, Projects(i).LastVisited.Value.Subtract(New DateTime(1970, 1, 1)).TotalSeconds.ToString(), "")
                                ProjectInfo(i)(11) = fCanModifyProject.ToString()
                                ProjectInfo(i)(12) = Projects(i).isValidDBVersion.ToString()
                            End If

                        Catch
                        End Try
                    End If
                Else

                    Try

                        If fCanSeeAllDecision OrElse fCanManageAnyDecision OrElse clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, Projects(i).ID, App.Workspaces) IsNot Nothing AndAlso Projects(i).ProjectStatus = ProjectStatus AndAlso Not Projects(i).isMarkedAsDeleted Then
                            Dim meetingOwner = ""
                            If Projects(i).MeetingOwner IsNot Nothing Then meetingOwner = Projects(i).MeetingOwner.UserEmail.ToString()
                            ProjectInfo(i) = New String(12) {}
                            ProjectInfo(i)(0) = Projects(i).ID.ToString()
                            ProjectInfo(i)(1) = Projects(i).ProjectName
                            ProjectInfo(i)(2) = App.DBUserByID(Projects(i).OwnerID).UserName
                            ProjectInfo(i)(3) = Projects(i).isTeamTime.ToString()
                            ProjectInfo(i)(4) = Projects(i).isOnline.ToString()
                            ProjectInfo(i)(5) = Projects(i).MeetingOwnerID.ToString()
                            ProjectInfo(i)(6) = meetingOwner
                            ProjectInfo(i)(7) = Projects(i).LastModify.Value.Subtract(New DateTime(1970, 1, 1)).TotalSeconds.ToString()
                            ProjectInfo(i)(8) = Projects(i).Created.Value.Subtract(New DateTime(1970, 1, 1)).TotalSeconds.ToString()
                            Dim userscount = App.DBUsersByProjectID(Projects(i).ID).Count
                            ProjectInfo(i)(9) = userscount.ToString()
                            ProjectInfo(i)(10) = If(Projects(i).LastVisited.HasValue, Projects(i).LastVisited.Value.Subtract(New DateTime(1970, 1, 1)).TotalSeconds.ToString(), "")
                            ProjectInfo(i)(11) = fCanModifyProject.ToString()
                            ProjectInfo(i)(12) = Projects(i).isValidDBVersion.ToString()
                        End If

                    Catch
                    End Try
                End If
            Next

            Dim ListofProjects = New List(Of String())()
            Dim UserList = New List(Of Integer)()

            Try
                For i As Integer = 0 To Projects.Count - 1
                    If ProjectInfo(i) IsNot Nothing Then
                        ListofProjects.Add(ProjectInfo(i))
                        'ProjecstList.Add((From a In Projects Where a.ID = ProjectInfo(i)(0)).FirstOrDefault())

                        'Dim pro = New Projects()
                        'pro.ID = Convert.ToInt32(ProjectInfo(i)(0))
                        'pro.ProjectName = ProjectInfo(i)(1)
                        'pro.UserName = ProjectInfo(i)(2)
                        'pro.isTeamTime = Convert.ToBoolean(ProjectInfo(i)(3))
                        'pro.isOnline = Convert.ToBoolean(ProjectInfo(i)(4))
                        'pro.MeetingOwnerID = Projects(i).MeetingOwnerID
                        'pro.meetingOwner = ProjectInfo(i)(6)
                        'pro.LastModify = Convert.ToDateTime(ProjectInfo(i)(7))
                        'pro.userscount = Convert.ToInt32(ProjectInfo(i)(9))
                        'pro.fCanModifyProject = Convert.ToBoolean(ProjectInfo(i)(11))
                        'pro.isValidDBVersion = Convert.ToBoolean(ProjectInfo(i)(12))
                        'ProjecstList.Add(pro)
                    End If
                Next

            Catch
            End Try

            Dim CurrentUserID = App.ActiveUser.UserID
            Dim s = New JavaScriptSerializer()
            Dim obj As Object()
            If HttpContext.Current.Request.Cookies("ProjectListSort") IsNot Nothing Then
                obj = s.Deserialize(Of String())(HttpContext.Current.Request.Cookies("ProjectListSort").Value)
            End If

            If obj IsNot Nothing Then
                Try
                    ListofProjects = ListofProjects.OrderBy(Function(si) si(Convert.ToInt32(obj(0)))).ToList()
                Catch
                    ListofProjects = ListofProjects.OrderBy(Function(si) si(7)).ToList()
                    obj(0) = "7"
                End Try

                Dim freverse As Boolean

                If Boolean.TryParse(CStr(obj(1)), freverse) Then
                    If freverse Then ListofProjects.Reverse()
                End If
            End If

            For i As Integer = 0 To ListofProjects.Count - 1
                Dim pro = New Projects()
                pro.ID = Convert.ToInt32(ListofProjects(i)(0))
                pro.ProjectName = ListofProjects(i)(1)
                pro.UserName = ListofProjects(i)(2)
                pro.isTeamTime = Convert.ToBoolean(ListofProjects(i)(3))
                pro.isOnline = Convert.ToBoolean(ListofProjects(i)(4))
                pro.MeetingOwnerID = ListofProjects(i)(5)
                pro.meetingOwner = ListofProjects(i)(6)
                pro.LastModify = ListofProjects(i)(7)
                pro.DateCreated = Convert.ToDecimal(ListofProjects(i)(8))
                pro.userscount = Convert.ToInt32(ListofProjects(i)(9))
                pro.LastVisited = Convert.ToDecimal(ListofProjects(i)(10))
                pro.fCanModifyProject = Convert.ToBoolean(ListofProjects(i)(11))
                pro.isValidDBVersion = Convert.ToBoolean(ListofProjects(i)(12))
                If ListofProjects(i)(4) = "False" Then
                    pro.ProjectStatus = "Not Available"
                ElseIf ListofProjects(i)(4) = "True" And ListofProjects(i)(3) <> "True" Then
                    pro.ProjectStatus = "Available"
                Else
                    pro.ProjectStatus = ""
                End If
                ProjecstList.Add(pro)
            Next

            Dim cookie = context.Request.Cookies("Filters")
            Dim project_status = If(cookie.Values("ProjectStatus") IsNot Nothing AndAlso cookie.Values("ProjectStatus") = "1", True, False)
            Dim project_access = If(cookie.Values("ProjectAccess") IsNot Nothing AndAlso cookie.Values("ProjectAccess") = "1", True, False)
            Dim last_access = If(cookie.Values("LastAccess") IsNot Nothing AndAlso cookie.Values("LastAccess") = "1", True, False)
            Dim last_modified = If(cookie.Values("LastModified") IsNot Nothing AndAlso cookie.Values("LastModified") = "1", True, False)
            Dim date_created = If(cookie.Values("DateCreated") IsNot Nothing AndAlso cookie.Values("DateCreated") = "1", True, False)
            Dim overal_judgment_process = If(cookie.Values("OverallJudgmentProcess") IsNot Nothing AndAlso cookie.Values("OverallJudgmentProcess") = "1", True, False)
            Dim workgroup_rolegroup = App.ActiveRoleGroup.ID + App.ActiveWorkgroup.ID

            If context.Request.Cookies("HideWarningMessage") Is Nothing Then
                Dim warningCookie = New HttpCookie("HideWarningMessage", "1") With {
                    .HttpOnly = True,
                    .Expires = DateTime.Now.AddDays(1)
                }
                context.Request.Cookies.Add(warningCookie)
            End If

            Dim hideWarning As Boolean = context.Request.Cookies("HideWarningMessage").Value = "1"
            Dim output = New WorkgroupsandProjects
            output.workgroups = WorkgroupsandProjects
            output.projects = ListofProjects
            output.clsProjects = ProjecstList.ToList()
            output.active_workgroup_id = CurrentWorkgroup
            output.active_project_id = CurrentProject
            output.combined_group_id = App.ActiveRoleGroup.ID
            output.role_workgroup_id = App.Options.WorkgroupRoleGroupID
            output.users = UserList
            output.currentUserID = CurrentUserID.ToString()
            output.isPM = isAdmin
            output.pageSize = CStr(HttpContext.Current.Session("ProjectListSize"))
            output.sort = obj
            output.totalProjects = Projects.Count
            output.debugprojects = debugprojects
            output.ProjectStatus = ProjectStatus
            output.project_status = project_status
            output.project_access = project_access
            output.last_access = last_access
            output.last_modified = last_modified
            output.date_created = date_created
            output.overal_judgment_process = overal_judgment_process
            output.hideBrowserWarning = hideWarning

            Return output
        End If

        Return Nothing
    End Function

    Private Shared ReadOnly Property checkRoleGroup As Boolean
        Get
            Dim isAdmin = False
            Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)

            Select Case App.ActiveRoleGroup.GroupType
                Case ecRoleGroupType.gtAdministrator, ecRoleGroupType.gtWorkgroupManager, ecRoleGroupType.gtECAccountManager, ecRoleGroupType.gtProjectManager, ecRoleGroupType.gtProjectOrganizer
                    isAdmin = True
            End Select

            Return isAdmin
        End Get
    End Property
#End Region

#Region "my-projects repeater functions"
    'Get all users of project by project Id
    <WebMethod(EnableSession:=True)>
    Public Shared Function getUsersByProject(ByVal projectID As Integer) As List(Of clsApplicationUser)
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Return App.DBUsersByProjectID(projectID)
    End Function

    'Chnage project status like online / ofline by toggle
    <WebMethod(EnableSession:=True)>
    Public Shared Function displayStatus(ByVal projid As Integer) As Boolean
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim Project = App.DBProjectByID(projid)
        Dim status = False

        If Project.isOnline Then
            Project.StatusDataLikelihood = 0
            Project.StatusDataImpact = 0
            Project.isOnline = False
            status = False
        Else
            Project.StatusDataLikelihood = 1
            Project.StatusDataImpact = 1
            Project.isOnline = True
            status = True
        End If

        App.DBProjectUpdate(Project)
        Return status
    End Function

    'Bind data in repeater as per workgroup
    Protected Sub select_workgroup_SelectedIndexChanged(sender As Object, e As EventArgs)
        setWorkgroup(select_workgroup.SelectedValue)
        Dim projects As WorkgroupsandProjects = getWorkgroupsandProjects(0)
        projects.clsProjects = ProjectsSorting(projects.clsProjects)

        rptdataGrid.DataSource = projects.clsProjects
        rptdataGrid.DataBind()
    End Sub

    'Set selected workgroup value in session
    Public Shared Sub setWorkgroup(ByVal ID As Integer)
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim Workgroup = App.DBWorkgroupByID(ID)
        App.ActiveWorkgroup = Workgroup
    End Sub

    'save sorting column wise with ascending and descending order
    <WebMethod(EnableSession:=True)>
    Public Shared Sub save_sort(ByVal column As Integer, ByVal reverse As Boolean)
        Dim sort_datas = New Object(1) {}
        sort_datas(0) = column
        sort_datas(1) = reverse
        Dim myObjectJson As String = New JavaScriptSerializer().Serialize(sort_datas)
        HttpContext.Current.Response.Cookies("ProjectListSort").Expires = DateTime.Now.AddDays(10)
        HttpContext.Current.Response.Cookies("ProjectListSort").Value = myObjectJson
    End Sub

    'check or uncheck filter checkbox for display column in list
    'Public Sub AccesssFilter()
    '    Dim cookie = Context.Request.Cookies("Filters")
    '    If cookie IsNot Nothing Then
    '        'hide and show online column
    '        If cookie.Values("ProjectStatus") IsNot Nothing AndAlso cookie.Values("ProjectStatus") = "1" Then
    '            chkisOnline.Checked = True
    '        Else
    '            chkisOnline.Checked = False
    '        End If

    '        'hide and show project status column
    '        If cookie.Values("ProjectAccess") IsNot Nothing AndAlso cookie.Values("ProjectAccess") = "1" Then
    '            chkProjectStatus.Checked = True
    '        Else
    '            chkProjectStatus.Checked = False
    '        End If

    '        'hide and show project last access column
    '        If cookie.Values("LastAccess") IsNot Nothing AndAlso cookie.Values("LastAccess") = "1" Then
    '            chkLastVisited.Checked = True
    '        Else
    '            chkLastVisited.Checked = False
    '        End If

    '        'hide and show project last modified column
    '        If cookie.Values("LastModified") IsNot Nothing AndAlso cookie.Values("LastModified") = "1" Then
    '            chkLastModify.Checked = True
    '        Else
    '            chkLastModify.Checked = False
    '        End If

    '        'hide and show project date created column
    '        If cookie.Values("DateCreated") IsNot Nothing AndAlso cookie.Values("DateCreated") = "1" Then
    '            chkDateCreated.Checked = True
    '        Else
    '            chkDateCreated.Checked = False
    '        End If

    '        'hide and show project overal judgment process column
    '        'If cookie.Values("OverallJudgmentProcess") IsNot Nothing AndAlso cookie.Values("OverallJudgmentProcess") = "1" Then
    '        '    chkOveralJudgmentProcess.Checked = True
    '        'Else
    '        '    chkOveralJudgmentProcess.Checked = False
    '        'End If
    '    End If
    'End Sub

    'bind projects in repeater

    Public Sub FillProjects()
        Dim projects As WorkgroupsandProjects = getWorkgroupsandProjects(0)
        projects.clsProjects = ProjectsSorting(projects.clsProjects)

        Dim search As String = txtSearch.Text.Trim()
        If search IsNot Nothing AndAlso search <> "" Then
            'projects.clsProjects = (From lst In projects.clsProjects Where lst.ProjectName.Contains(search) Select lst).ToList()
            projects.clsProjects = projects.clsProjects.Where(Function(x) x.ProjectName.ToLower().Contains(search.ToLower())).ToList()
        End If

        rptdataGrid.DataSource = projects.clsProjects
        rptdataGrid.DataBind()
    End Sub

    'sorting projects value as per session value
    Public Shared Function ProjectsSorting(ByVal lst As List(Of Projects)) As List(Of Projects)
        Dim projects As List(Of Projects) = lst

        Dim obj As Object()
        If HttpContext.Current.Request.Cookies("ProjectListSort") IsNot Nothing Then
            obj = New JavaScriptSerializer().Deserialize(Of String())(HttpContext.Current.Request.Cookies("ProjectListSort").Value)
        End If
        If obj IsNot Nothing Then
            'sorting by Project name
            If obj(0) = "1" Then
                If obj(1) = "True" Then
                    projects = (From proj In projects Order By proj.ProjectName Descending Select proj).ToList()
                Else
                    projects = (From proj In projects Order By proj.ProjectName Ascending Select proj).ToList()
                End If
                'sorting by Online
            ElseIf obj(0) = "4" Then
                If obj(1) = "True" Then
                    projects = (From proj In projects Order By proj.isOnline Descending Select proj).ToList()
                Else
                    projects = (From proj In projects Order By proj.isOnline Ascending Select proj).ToList()
                End If
                'sorting by Status
            ElseIf obj(0) = "3" Then
                If obj(1) = "True" Then
                    projects = (From proj In projects Order By proj.ProjectStatus Descending Select proj).ToList()
                Else
                    projects = (From proj In projects Order By proj.ProjectStatus Ascending Select proj).ToList()
                End If
                'sorting by LastVisited
            ElseIf obj(0) = "10" Then
                If obj(1) = "True" Then
                    projects = (From proj In projects Order By proj.LastVisited Descending Select proj).ToList()
                Else
                    projects = (From proj In projects Order By proj.LastVisited Ascending Select proj).ToList()
                End If
                'sorting by Last Modify
            ElseIf obj(0) = "7" Then
                If obj(1) = "True" Then
                    projects = (From proj In projects Order By proj.LastVisited Descending Select proj).ToList()
                Else
                    projects = (From proj In projects Order By proj.LastVisited Ascending Select proj).ToList()
                End If
                'sorting by Created Date
            ElseIf obj(0) = "8" Then
                If obj(1) = "True" Then
                    projects = (From proj In projects Order By proj.DateCreated Descending Select proj).ToList()
                Else
                    projects = (From proj In projects Order By proj.DateCreated Ascending Select proj).ToList()
                End If
            End If
        End If
        Return projects
    End Function

    Protected Sub hdnButton_Click(sender As Object, e As EventArgs)
        FillProjects()
    End Sub

    'set page size on change value from page dropdown
    <WebMethod(EnableSession:=True)>
    Public Shared Function set_Page(ByVal is_left As Boolean)
        Dim projects As WorkgroupsandProjects = getWorkgroupsandProjects(0)
        If projects.pageSize <> "ALL" Then
            numberOfPages = projects.clsProjects.Count / projects.pageSize
        Else
            numberOfPages = 1
        End If

        If is_left Then
            currentPage -= 1
        Else
            currentPage += 1
        End If

        If currentPage > numberOfPages - 1 Then
            currentPage = numberOfPages - 1
        ElseIf currentPage < 0 Then
            currentPage = 0
        End If

        Dim output As Object = New With {
            Key .currentPage = currentPage,
            Key .numberOfPages = numberOfPages
        }
        Dim oSerializer = New JavaScriptSerializer()
        Return oSerializer.Serialize(output)
    End Function

    'save selected page size in session
    <WebMethod(EnableSession:=True)>
    Public Shared Sub savePageSize(ByVal pageSize As String)
        HttpContext.Current.Session("ProjectListSize") = pageSize
    End Sub

    'save filter in session for hide and show table column
    <WebMethod(EnableSession:=True)>
    Public Shared Sub RememberFilter(ByVal filter As Boolean, ByVal filtername As String)
        Dim context As HttpContext = HttpContext.Current
        Dim cookie = context.Request.Cookies("Filters")
        Dim value As String = If((filter = True), "1", "0")

        Select Case filtername
            Case "project_status"
                cookie.Values("ProjectStatus") = value
            Case "project_access"
                cookie.Values("ProjectAccess") = value
            Case "last_access"
                cookie.Values("LastAccess") = value
            Case "last_modified"
                cookie.Values("LastModified") = value
            Case "date_created"
                cookie.Values("DateCreated") = value
            Case "overal_judgment_process"
                cookie.Values("OverallJudgmentProcess") = value
        End Select

        context.Response.AppendCookie(cookie)
    End Sub
#End Region

#Region "Invite popup links"
    'Get link user wise of all users of current project
    <WebMethod(EnableSession:=True)>
    Public Shared Function GenerateLink(ByVal is_teamtime As Boolean, ByVal projID As Integer) As Object
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        App.ActiveProject = App.DBProjectByID(projID)
        Dim users = App.DBUsersByProjectID(App.ActiveProject.ID)
        Dim listofhashlinks = New List(Of String())()
        Dim project = App.ActiveProject
        Dim baseUrl As String = HttpContext.Current.Request.Url.Scheme & "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath.TrimEnd("/"c) & "/"

        For Each user As clsApplicationUser In users
            If user.UserEmail.Equals("admin", StringComparison.CurrentCultureIgnoreCase) Then Continue For
            Dim arrayofstring = New String(1) {}
            arrayofstring(0) = user.UserEmail

            If Not is_teamtime Then
                arrayofstring(1) = baseUrl & GeckoClass.CreateLogonURL(user, project, False, "", "")
            Else
                arrayofstring(1) = baseUrl & GeckoClass.CreateLogonURL(user, project, True, "", "")
            End If

            listofhashlinks.Add(arrayofstring)
        Next

        Return listofhashlinks
    End Function

    'get link custom wise as per selection by user on Invite model
    <WebMethod(EnableSession:=True)>
    Public Shared Function getGeneralLink(ByVal tmode As Integer, ByVal projectID As Integer, ByVal signupmode As String, ByVal otherparams As String, ByVal combinedGroupID As Integer, ByVal wkgRoleGroupId As Integer) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim sResult As String = ""
        If Not App.HasActiveProject() Then App.ProjectID = projectID
        Dim tProject = App.ActiveProject
        Dim roleGroup As clsRoleGroup = App.ActiveWorkgroup.RoleGroups.FirstOrDefault(Function(rg) rg.GroupType = CType((wkgRoleGroupId + 9), ecRoleGroupType))
        wkgRoleGroupId = If(roleGroup Is Nothing, 0, roleGroup.ID)

        If tProject IsNot Nothing AndAlso wkgRoleGroupId > 0 Then
            Dim baseurl = context.Request.Url.Scheme & "://" + context.Request.Url.Authority + context.Request.ApplicationPath.TrimEnd("/"c) & "/"

            If tmode = 3 Then
                sResult = baseurl & "?passcode=" + HttpUtility.UrlEncode(tProject.Passcode)
            Else
                If otherparams <> "" Then otherparams = "req=" & otherparams
                sResult = CreateEvaluationSignupURL(tProject, tProject.Passcode, tmode = 1, signupmode, otherparams, baseurl, combinedGroupID, wkgRoleGroupId)
            End If
        End If

        Return sResult
    End Function

    Public Shared Function CreateEvaluationSignupURL(ByVal tProject As clsProject, ByVal sPasscode As String, ByVal fIsAnonymous As Boolean, ByVal sSignupMode As String, ByVal sOtherParams As String, ByVal sPagePath As String, ByVal tGroupID As Integer, ByVal tWkgRoleGroupID As Integer) As String
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim sURL As String = ""
        If tProject Is Nothing Then Return sURL
        sURL += String.Format("&{0}=1&{1}={2}&{3}={4}&{5}={6}", _PARAMS_SIGNUP(0), _PARAMS_ANONYMOUS_SIGNUP(0), (If(fIsAnonymous, "1", "0")), _PARAM_PASSCODE, HttpUtility.UrlEncode(sPasscode), _PARAMS_SIGNUP_MODE(0), sSignupMode)
        If tGroupID >= 0 Then sURL += String.Format("&{0}={1}", _PARAM_ROLEGROUP, tGroupID)
        If tWkgRoleGroupID >= 0 Then sURL += String.Format("&{0}={1}", _PARAM_WKG_ROLEGROUP, tWkgRoleGroupID)

        If sOtherParams <> "" Then
            If sURL <> "" Then sURL += "&"
            sURL += sOtherParams
        End If

        sURL = EncodeURL(sURL, App.DatabaseID)

        If App.Options.UseTinyURL Then
            Dim PID As Integer = tProject.ID
            sURL = String.Format("{0}?{2}={1}", sPagePath, App.CreateTinyURL(sURL, PID, -1), _PARAMS_TINYURL(0))
        Else
            sURL = String.Format("{0}?{2}={1}", sPagePath, sURL, _PARAMS_KEY(0))
        End If

        Return sURL
    End Function
#End Region

End Class