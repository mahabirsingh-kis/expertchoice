
Partial Class Snapshots_Timeline
    Inherits clsComparionCorePage

    Private _PrjID As Integer = -1
    Private _Project As clsProject = Nothing    ' D3882

    Public Const _COOKIE_SNAPSHOTS_MODE As String = "SnapshotsMode"   ' D3713

    Public Sub New()
        MyBase.New(_PGID_PROJECT_SNAPSHOTS)
        'Page.Theme = _THEME_EC2018 'A1561
    End Sub

    Public ReadOnly Property ProjectID As Integer
        Get
            If _PrjID < 0 Then _PrjID = CheckVar("prj_id", App.ProjectID)
            Return _PrjID
        End Get
    End Property

    ' D3882 ===
    Public ReadOnly Property CurProject As clsProject
        Get
            If _Project Is Nothing AndAlso ProjectID > 0 Then
                If ProjectID = App.ProjectID Then _Project = App.ActiveProject Else _Project = App.DBProjectByID(ProjectID)
            End If
            Return _Project
        End Get
    End Property
    ' D3882 ==

    ' D4578 ===
    Public Function GetTitle() As String
        Return String.Format(ResString("lblSnapshotsList"), If(App.HasActiveProject AndAlso ProjectID = App.ProjectID, "", String.Format("<span title=""{0}"">&laquo;{1}&raquo;</span>", CurProject.ProjectName.Replace("""", "&quot;"), ShortString(CurProject.ProjectName, 45))))
    End Function
    ' D4578 ==

    Private Function GetSnapshotData(tSnapshot As clsSnapshot) As String
        Dim sDT As String = DateTime2String(tSnapshot.DateTime, True)
        Return String.Format("[{0},'{1}','{2}',{3},'{4}',{5},{6},'{7}','{8}',{9},'{10}',{11},{12}]", tSnapshot.ID, JS_SafeString(sDT), JS_SafeString(ShortString(tSnapshot.Comment, 200, False, 40)), CInt(tSnapshot.Type), JS_SafeString(tSnapshot.SnapshotID), JS_SafeNumber(tSnapshot.Idx), JS_SafeNumber(tSnapshot.RestoredFrom), JS_SafeString(tSnapshot.Details), JS_SafeString(tSnapshot.ProjectStreamMD5), JS_SafeNumber(tSnapshot.ProjectStreamSize), JS_SafeString(tSnapshot.ProjectWorkspaceMD5), JS_SafeNumber(tSnapshot.ProjectWorkspaceSize), IIf(CurProject.Created.HasValue AndAlso CurProject.Created.Value > tSnapshot.DateTime, 1, 0))    ' D3731 + D3775 + D3882
    End Function

    Public Function GetAllSnapshotsData(sCustomMsg As String, Optional ByRef tMD5 As String = Nothing) As String
        Dim sContent As String = ""
        Dim sMsg As String = sCustomMsg
        Dim fIsError As Boolean = False

        If Not App.isSnapshotsAvailable Then
            sMsg = ResString("msgNoSnapshotsService")   ' D3577
            fIsError = True
        Else
            If ProjectID < 1 Then
                sMsg = ResString("msgNoActiveProject")   ' D3577
                fIsError = True
            Else
                Dim tList As List(Of clsSnapshot) = App.DBSnapshotsReadAll(ProjectID, False)
                App.SnapshotsCheckMissingIdx(tList) ' D3731
                If tList.Count > 0 Then
                    For Each tItem As clsSnapshot In tList
                        Dim sDT As String = DateTime2String(tItem.DateTime, True)
                        'If tItem.DateTime.AddDays(3) > Now Then sDT = GetPeriodString(tItem.DateTime)
                        sContent += CStr(IIf(sContent = "", "", ",")) + GetSnapshotData(tItem)
                    Next
                Else
                    If sMsg = "" Then sMsg = ResString("msgNoSnapshots") ' D3577
                End If
            End If
        End If

        If sMsg <> "" AndAlso fIsError Then sMsg = "<span class='error'>" + sMsg + "</span>"
        ' D3595 ===
        Dim sMD5 As String = GetMD5(sContent)   ' D3747
        If tMD5 IsNot Nothing Then tMD5 = sMD5

        Return String.Format("['{0}', [{1}], '{2}']", JS_SafeString(sMsg), sContent, sMD5)
        ' D3595 ==
    End Function

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        AlignHorizontalCenter = False
        AlignVerticalCenter = False
        If CurProject Is Nothing Then FetchAccess()
        If IsAJAX Then onAJAXRequest()
    End Sub

    Private Sub onAJAXRequest()
        Dim sResponse As String = ""
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).ToLower() ' D3707 + Anti-XSS

        Select Case sAction
            Case "getdata"
                sResponse = GetAllSnapshotsData("")

                ' D3595 ===
            Case "checkdata"
                Dim sMD5 As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("md5", ""))    ' D3707 + Anti-XSS
                Dim sNewMD5 As String = ""
                sResponse = GetAllSnapshotsData("", sNewMD5)
                If sMD5 = sNewMD5 Then sResponse = ""
                ' D3595 ==

            Case "create"
                ' D3668 ===
                Dim sComment As String = SafeFormString(CheckVar("text", "").Trim)  ' D3707
                ' D3731 ===
                Dim sMessage As String = ResString("lblSnapshotOnDemand")
                If sComment <> "" AndAlso App.CanvasMasterDBVersion < "0.99992" Then sMessage = sComment
                Dim tSnapshot As clsSnapshot = App.SnapshotSaveProject(ecSnapShotType.Manual, sMessage, ProjectID, True, sComment)   ' D3577
                ' D3568 + D3731 ==
                sResponse = GetAllSnapshotsData(CStr(IIf(tSnapshot Is Nothing, String.Format("<span class='error'>{0}</span>", ResString("errCantCreateSnapshot")), "")))   ' D3577

                ' D3658 ===
            Case "edit"
                ' D3668 ===
                Dim tSnapshot As clsSnapshot = Nothing
                Dim tID As Integer = -1
                If Integer.TryParse(CheckVar("id", ""), tID) Then   ' D3707
                    Dim sComment As String = SafeFormString(CheckVar("text", "").Trim)  ' D3707
                    tSnapshot = App.DBSnapshotRead(tID, False)
                    If tSnapshot IsNot Nothing AndAlso sComment <> "" Then  ' D3662
                        tSnapshot.Details = sComment    ' D3731
                        App.DBSnapshotInfoUpdate(tSnapshot)
                    End If
                End If
                sResponse = GetAllSnapshotsData(CStr(IIf(tSnapshot Is Nothing, String.Format("<span class='error'>{0}</span>", ResString("errCantEditSnapshot")), "")))
                ' D3658 ==


            Case "restore"
                Dim tID As Integer = -1
                Dim sMsg = ""
                If Integer.TryParse(CheckVar("id", ""), tID) Then    ' D3707
                    ' D4196 ===
                    Dim sName As String = CheckVar("name", "")
                    If sName = "" Then sName = ResString("lblSnapshotSaveBeforeRestore")
                    If CheckVar("save", False) Then App.SnapshotSaveProject(ecSnapShotType.Manual, sName, ProjectID, True, "") ' D3682 + D3746
                    ' D4196 ==
                    Dim tSnapshot As clsSnapshot = App.SnapshotRestoreProject(tID, ProjectID, sMsg) ' D3893
                    If tSnapshot IsNot Nothing AndAlso sMsg = "" Then   ' D3893
                        If _OPT_SNAPSHOTS_SAVE_ON_RESTORE Then  ' D3723
                            Dim sID As String = CStr(IIf(tSnapshot.Idx > 0, tSnapshot.Idx, tSnapshot.SnapshotID))
                            tSnapshot = App.SnapshotSaveProject(ecSnapShotType.RestorePoint, String.Format(ResString("lblPrjRestoredFromSnapshot"), sID), ProjectID, False, String.Format("Restored from #{0} (hash: {1}) ", sID, tSnapshot.SnapshotID), tSnapshot.Idx)   ' D3577 + D3729 + D3731 + D3811
                            'tSnapshot = App.SnapshotSaveProject(ecSnapShotType.RestorePoint, String.Format(ResString("lblPrjRestoredFromSnapshot"), CStr(IIf(tSnapshot.Idx > 0, tSnapshot.Idx, tSnapshot.SnapshotID)), True), ProjectID, False, "Restored from: " + tSnapshot.SnapshotID, tSnapshot.Idx)   ' D3577 + D3729 + D3731
                            If tSnapshot Is Nothing Then sMsg = ResString("msgProjectRestored") ' D3577
                        End If
                        App.ProjectCheckPropertiesFromPM(clsProject.ProjectByID(ProjectID, App.ActiveProjectsList))  ' D6588 + D7051
                    Else
                        If sMsg = "" Then sMsg = ResString("msgUnableProjectRestore") Else sMsg = String.Format(ResString("msgSnapshotRestoreError"), ParseAllTemplates(sMsg, App.ActiveUser, CurProject)) ' D3577 + D3893
                    End If
                Else
                    sMsg = ResString("msgWrongParameter")   ' D3577
                End If
                If sMsg <> "" Then sMsg = "<span class='error'>" + sMsg + "</span>"
                'If sMsg = "" AndAlso App.ProjectID <> ProjectID Then sMsg = ResString("msgProjectRestoredFromSnapshot") ' D3893 -D3958
                sResponse = GetAllSnapshotsData(sMsg)

                ' D3577 ===
            Case "clear"
                Dim Cnt As Integer = App.DBSnapshotsDeleteAll(ProjectID)    ' D3584
                App.DBSaveLog(dbActionType.actDelete, dbObjectType.einfSnapshot, ProjectID, "Delete all snapshots", String.Format("Deleted elements: {0}", Cnt)) ' D3584
                sResponse = GetAllSnapshotsData("")
                ' D3577 ==

                ' D3584 ===
            Case "delete"
                Dim sLst As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("lst", "")) ' Anti-XSS
                If sLst <> "" Then
                    Dim Cnt As Integer = 0
                    Dim IDsList As String() = sLst.Split(CType(",", Char()))
                    For Each sID As String In IDsList
                        Dim ID As Integer
                        If Integer.TryParse(sID, ID) Then If App.DBSnapshotDelete(ID) Then Cnt += 1
                    Next
                    App.DBSaveLog(dbActionType.actDelete, dbObjectType.einfSnapshot, ProjectID, "Delete snapshot(s)", String.Format("Deleted elements: {0}", Cnt)) ' D3584
                End If
                sResponse = GetAllSnapshotsData("")
                ' D3584 ==

        End Select

        'If sResponse <> "" Then
        RawResponseStart()
        Response.ContentType = "text/plain"
        Response.Write(sResponse)
        Response.End()
        'End If

    End Sub

End Class
