Partial Class WorkgroupWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Public Function Open(ID As Integer) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arError, .Message = ResString("errAjaxWrongWkg")}   ' D7166
        App.UserWorkgroups = Nothing    ' D7166
        Dim tWkg As clsWorkgroup = clsWorkgroup.WorkgroupByID(ID, App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups))
        If tWkg IsNot Nothing Then
            App.ActiveWorkgroup = tWkg
            ' D4674 ===
            If App.ActiveWorkgroup IsNot Nothing Then
                If App.ActiveWorkgroup.ID = ID Then
                    App.ActiveUser.DefaultWorkgroupID = ID
                    App.DBUserUpdate(App.ActiveUser, False, "Set default workgroup")
                    App.DBSaveLog(dbActionType.actSelect, dbObjectType.einfWorkgroup, ID, "", "")
                    Res.Result = ecActionResult.arSuccess
                    Res.Message = ""
                End If
                ' D4674 ==
            End If
        End If
        If App.ActiveWorkgroup IsNot Nothing Then Res.ObjectID = App.ActiveWorkgroup.ID ' D7166
        Res.Data = ID   ' D7166
        Return Res
    End Function

    ' D7206 ===
    Public Function List() As jActionResult
        If Not App.isAuthorized Then FetchIfNotAuthorized()
        Dim Res As New jActionResult With {.Result = ecActionResult.arSuccess}
        Dim tList As New List(Of jWorkgroup)
        For Each tWkg As clsWorkgroup In App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups)
            tList.Add(jWorkgroup.CreateFromBaseObject(tWkg, clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, tWkg.ID, App.UserWorkgroups)))
        Next
        Res.Data = tList
        If App.ActiveWorkgroup IsNot Nothing Then Res.ObjectID = App.ActiveWorkgroup.ID
        Return Res
    End Function
    ' D7206 ==

    ' D7344 ===
    ''' <summary>
    ''' Set user workgroup state (enable/disable) for active user
    ''' </summary>
    ''' <param name="ID">WorkgroupID</param>
    ''' <param name="Disabled">Any positive value in case of disable</param>
    ''' <returns>.ObjectID as Workgroup ID (passed paramter), .Data is the current user workgroup state.</returns>
    ''' <remarks>User must be logged in and not an Admin permissions. In case of missing user workgroup (wrong ID) the error will be returned.</remarks>
    Public Function UpdateUserWorkgroup(ID As Integer, Disabled As Boolean) As jActionResult
        If App.ActiveUser.CannotBeDeleted Then FetchNoPermissions()
        Dim Res As New jActionResult With {.Result = ecActionResult.arError, .Message = ResString("errAjaxWrongWkg")}
        Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, ID, App.UserWorkgroups)
        If tUW Is Nothing Then
            FetchNotFound(, "NO_WORKGROUP_FOUND")
        Else
            tUW.Status = If(Disabled, ecUserWorkgroupStatus.uwDisabled, ecUserWorkgroupStatus.uwEnabled)
            App.DBUserWorkgroupUpdate(tUW, False, String.Format("Set user workgroup state as '{0}' for  '{1}'", ResString(String.Format(ResString("lblProps_Template"), tUW.Status.ToString)), App.ActiveUser.UserEmail))
            App.UserWorkgroups = Nothing    ' D7166
            ' D7354 ===
            Dim WkgID As Integer = If(App.ActiveWorkgroup IsNot Nothing, App.ActiveWorkgroup.ID, -1)
            If Disabled AndAlso App.ActiveWorkgroup IsNot Nothing Then
                If App.ActiveWorkgroup.ID = tUW.WorkgroupID Then
                    Dim DT As DateTime = Now.AddYears(-10)
                    For Each UWtmp As clsUserWorkgroup In App.UserWorkgroups
                        Dim dtmp As DateTime = DT
                        If (UWtmp.Created.HasValue AndAlso UWtmp.Created.Value > dtmp) Then dtmp = UWtmp.Created.Value
                        If (UWtmp.LastVisited.HasValue AndAlso UWtmp.LastVisited.Value > dtmp) Then dtmp = UWtmp.LastVisited.Value
                        If UWtmp.Status = ecUserWorkgroupStatus.uwEnabled AndAlso dtmp > DT Then
                            WkgID = UWtmp.WorkgroupID
                            DT = dtmp
                        End If
                    Next
                    If App.ActiveWorkgroup.ID <> WkgID Then
                        App.ActiveWorkgroup = If(WkgID < 0, Nothing, clsWorkgroup.WorkgroupByID(WkgID, App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups)))
                        If App.ActiveWorkgroup Is Nothing Then WkgID = -1
                        App.DBSaveLog(dbActionType.actSelect, dbObjectType.einfWorkgroup, WkgID, "Switch to another workgroup", If(WkgID > 0, "", "No available workgroup"))
                    End If
                End If
            End If
            ' D7354 ==
            Res.Result = ecActionResult.arSuccess
            Res.Message = ""
            Res.Data = tUW.Status
            Res.ObjectID = WkgID ' D7354
        End If
        Return Res
    End Function
    ' D7344 ==

    Private Sub AuthWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNotAuthorized()
        Select Case _Page.Action

            Case "open"
                Dim ID As Integer = -1
                Dim sID As String = GetParam(_Page.Params, _PARAM_ID, True)
                If Integer.TryParse(sID, ID) Then
                    _Page.ResponseData = Open(ID)
                Else
                    FetchNotFound(, "NO_WORKGROUP_FOUND")
                End If

            Case "list" ' D7206
                _Page.ResponseData = List() ' D7206

            Case "updateuserworkgroup"
                Dim ID As Integer = -1
                Dim sID As String = GetParam(_Page.Params, _PARAM_ID, True)
                If Integer.TryParse(sID, ID) Then
                    _Page.ResponseData = UpdateUserWorkgroup(ID, Str2Bool(GetParam(_Page.Params, "disabled", True)))
                Else
                    FetchNotFound(, "NO_WORKGROUP_FOUND")
                End If

        End Select
    End Sub

End Class