Partial Class ProjectsListPage
    Inherits clsComparionCorePage

    Public Property Message As String = ""
    Public Property isError As Boolean = False

    Public OPT_SHOW_NEWS As Boolean = True                  ' D5044 + D6115
    Public OPT_SHOW_KNOWN_ISSUES As Boolean = False         ' D6034 + D6115

    Public Property CanEditAtLeastOneModel As Boolean = False  ' D4939
    Public Property CanManageAllModels As Boolean = False      ' D4965
    Public Property CanCreateNewModels As Boolean = False      ' D4991

    ' keep it:
    'CType(CType(Config.GetSectionGroup("system.web"), System.Web.Configuration.SystemWebSectionGroup).HttpRuntime, System.Web.Configuration.HttpRuntimeSection).MaxRequestLength

    Public Sub New()
        MyBase.New(_PGID_PROJECTSLIST)
    End Sub

    Private Sub ProjectsListPage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        If App.isAuthorized Then
            CanManageAllModels = App.CanUserDoAction(ecActionType.at_alManageAnyModel, App.ActiveUserWorkgroup, App.ActiveWorkgroup)   ' D4965
            CanCreateNewModels = App.CanUserDoAction(ecActionType.at_alCreateNewModel, App.ActiveUserWorkgroup, App.ActiveWorkgroup)   ' D4991
            If CanCreateNewModels Then ' D4938 + D5005
                Select Case CheckVar(_PARAM_TAB, "").ToLower.Trim
                    Case "archived"
                        CurrentPageID = _PGID_PROJECTSLIST_ARCHIVED
                    Case "templates"
                        CurrentPageID = _PGID_PROJECTSLIST_TEMPLATES
                    Case "master"
                        CurrentPageID = _PGID_PROJECTSLIST_MASTERPROJECTS
                    Case "deleted"
                        CurrentPageID = _PGID_PROJECTSLIST_DELETED
                    Case Else
                        CurrentPageID = _PGID_PROJECTSLIST
                End Select
                CanEditAtLeastOneModel = True   ' D4939
            Else
                CanEditAtLeastOneModel = App.isAuthorized AndAlso App.CanUserModifySomeProject(App.ActiveUser.UserID, App.ActiveProjectsList, App.ActiveUserWorkgroup, App.Workspaces)   ' D4939
            End If
            ' D4740 ===
            If App.ApplicationError.Status <> ecErrorStatus.errNone AndAlso Not String.IsNullOrEmpty(App.ApplicationError.Message) Then
                Message = App.ApplicationError.Message
                isError = App.ApplicationError.Status <> ecErrorStatus.errPageNotFound
                App.ApplicationError.Reset()
            End If
            ' D4740 ==
        End If
    End Sub

    ' D6020 ===
    Public Function HasActiveProjects() As Boolean
        For Each tPrj As clsProject In App.ActiveProjectsList
            If tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted Then Return True   ' D6056
        Next
        Return False
    End Function

    Public Function CanShowPMInstruction() As Boolean
        If App.isAuthorized AndAlso CanCreateNewModels AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsEnabled Then
            If GetCookie("show_splash" + If(App.isRiskEnabled, "_risk", "") + If(App.isCommercialUseEnabled, "", "_" + App.ActiveWorkgroup.ID.ToString), "") <> "0" AndAlso (Not HasActiveProjects() OrElse Not App.isCommercialUseEnabled OrElse PMShowInstruction(App, App.ActiveUser.UserID)) Then   ' D6057
                Return True
            End If
        End If
        Return False
    End Function
    ' D6020 ==

    ' D4965 ===
    Public Function GetTabIDs() As String
        Return String.Format("{0}{1}{2}{3}{4}{5}", CInt(_PGID_PROJECTSLIST),
                             If(CanEditAtLeastOneModel, String.Format(", {0}", CInt(_PGID_PROJECTSLIST_ARCHIVED)), ""),
                             If(CanManageAllModels OrElse CanCreateNewModels, String.Format(", {0}", CInt(_PGID_PROJECTSLIST_TEMPLATES)), ""),
                             If(_OPT_ALLOW_REVIEW_ACCOUNT AndAlso isReviewAccount(), String.Format(", {0}", CInt(_PGID_PROJECTSLIST_REVIEW)), ""),
                             If(CurrentPageID = _PGID_PROJECTSLIST_MASTERPROJECTS OrElse (App.HasActiveProject AndAlso App.ActiveProject.ProjectStatus = ecProjectStatus.psMasterProject AndAlso Not App.ActiveProject.isMarkedAsDeleted), String.Format(", {0}", CInt(_PGID_PROJECTSLIST_MASTERPROJECTS)), ""),
                             If(CanEditAtLeastOneModel, String.Format(", {0}", CInt(_PGID_PROJECTSLIST_DELETED)), ""))  ' D5068 + D6226 + D9404
    End Function
    ' D4965 ==

    Private Sub ProjectsListPage_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        ShowNavigation = False  ' D4819
        ShowTopNavigation = App.HasActiveProject AndAlso CanViewActiveProject()     ' D4820 + D5079
    End Sub

End Class