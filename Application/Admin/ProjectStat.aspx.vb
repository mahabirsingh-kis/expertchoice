Partial Class ProjectStatisticPage
    Inherits clsComparionCorePage

    Public Const DT As String = "yyyy-MM-dd HH:mm:ss"

    Public Sub New()
        MyBase.New(_PGID_ADMIN_PRJ_STAT)    'D41613
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        ShowNavigation = False
        If isAJAX() Then

            Dim sResult As String = ""

            Select Case CheckVar(_PARAM_ACTION, "").ToLower

                Case "open"
                    Dim PrjID As Integer
                    If Integer.TryParse(CheckVar("id", ""), PrjID) Then
                        Dim tPrj As clsProject = App.DBProjectByID(PrjID)
                        If tPrj IsNot Nothing AndAlso Not tPrj.isMarkedAsDeleted Then
                            Dim tWkg As clsWorkgroup = App.DBWorkgroupByID(tPrj.WorkgroupID)
                            If tWkg IsNot Nothing AndAlso tWkg.License IsNot Nothing AndAlso tWkg.License.isValidLicense AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.InstanceID) Then
                                sResult = CreateLogonURL(App.ActiveUser, tPrj, CStr(IIf(tPrj.isTeamTimeImpact OrElse tPrj.isTeamTimeLikelihood, "pipe=no", "")), _URL_ROOT, tPrj.PasscodeLikelihood, , True)    ' D4616
                            End If
                        End If
                    End If

                Case "load"
                    Dim sText As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("text", "").Trim())  'Anti-XSS
                    Dim OnlineUsers As List(Of clsOnlineUserSession) = App.DBOnlineSessions()

                    'Dim tUsersCnt As New List(Of Dictionary(Of String, Object))
                    Dim tStructureSizes As New List(Of Dictionary(Of String, Object))
                    Dim tUserDataSizes As New List(Of Dictionary(Of String, Object))
                    Dim tSnapshotSizes As New List(Of Dictionary(Of String, Object))
                    Dim tSnapshotCounts As New List(Of Dictionary(Of String, Object))
                    Try
                        'tUsersCnt = App.Database.SelectBySQL(String.Format("SELECT P.ID, COUNT(W.ID) as CNT FROM Projects P LEFT JOIN Workspace W ON P.ID = W.ProjectID WHERE P.WorkgroupID={0} GROUP BY P.ID", App.ActiveWorkgroup.ID))
                        tStructureSizes = App.Database.SelectBySQL(String.Format("SELECT SUM(M.StreamSize) as FullSize, M.ProjectID FROM ModelStructure as M LEFT JOIN Projects as P ON P.ID = M.ProjectID WHERE P.WorkgroupID={0} GROUP BY M.ProjectID", App.ActiveWorkgroup.ID))
                        tUserDataSizes = App.Database.SelectBySQL(String.Format("SELECT SUM(D.StreamSize) as FullSize, D.ProjectID FROM UserData as D LEFT JOIN Projects as P ON P.ID = D.ProjectID WHERE P.WorkgroupID={0} GROUP BY D.ProjectID", App.ActiveWorkgroup.ID))
                        tSnapshotSizes = App.Database.SelectBySQL(String.Format("SELECT SUM(DATALENGTH(Stream) + DATALENGTH(Workspace)) as FullSize, COUNT(S.ID) as Cnt, SnapshotType, ProjectID FROM Snapshots S LEFT JOIN Projects P ON P.ID=S.ProjectID WHERE P.WorkgroupID = {0} GROUP BY S.ProjectID, S.SnapshotType", App.ActiveWorkgroup.ID))
                    Catch ex As Exception
                    End Try

                    sResult = ""
                    For Each tProject As clsProject In App.ActiveProjectsList
                        If sText = "" OrElse tProject.ProjectName.ToLower.Contains(sText.ToLower) OrElse
                           tProject.Comment.ToLower.Contains(sText.ToLower) OrElse
                           tProject.PasscodeImpact.ToLower.Replace("-", "").Contains(sText.ToLower) OrElse
                           tProject.PasscodeLikelihood.ToLower.Replace("-", "").Contains(sText.ToLower) Then

                            Dim sStatus As String = ResString("lbl_" + tProject.ProjectStatus.ToString)
                            If tProject.isMarkedAsDeleted Then sStatus = ResString("lblMarkedAsDeleted")

                            Dim sCreated As String = ""
                            Dim sVisited As String = ""
                            Dim sModified As String = ""
                            If tProject.Created.HasValue Then sCreated = tProject.Created.Value.ToString(DT)
                            If tProject.LastVisited.HasValue Then sVisited = tProject.LastVisited.Value.ToString(DT)
                            If tProject.LastModify.HasValue Then sModified = tProject.LastModify.Value.ToString(DT)

                            Dim sLock As String = ""
                            If tProject.LockInfo IsNot Nothing AndAlso tProject.LockInfo.LockStatus <> ECLockStatus.lsUnLocked Then
                                sLock = App.GetMessageByLockStatus(tProject.LockInfo, tProject, App.ActiveUser.UserID, False)
                                If tProject.LockInfo.LockerUserID > 0 AndAlso tProject.LockInfo.LockerUserID <> App.ActiveUser.UserID Then
                                    Dim tUser As clsApplicationUser = App.DBUserByID(tProject.LockInfo.LockerUserID)
                                    If tUser IsNot Nothing Then sLock += String.Format(" ({0})", tUser.UserEmail)
                                End If
                            End If

                            Dim tStructSize As Long = 0
                            If tStructureSizes IsNot Nothing Then
                                For Each tmpRow As Dictionary(Of String, Object) In tStructureSizes
                                    If Not IsDBNull(tmpRow("ProjectID")) AndAlso Not IsDBNull(tmpRow("ProjectID")) AndAlso CInt(tmpRow("ProjectID")) = tProject.ID Then
                                        tStructSize = CLng(tmpRow("FullSize"))
                                        tStructureSizes.Remove(tmpRow)
                                        Exit For
                                    End If
                                Next
                            End If

                            Dim tUsrDataSize As Long = 0
                            If tUserDataSizes IsNot Nothing Then
                                For Each tmpRow As Dictionary(Of String, Object) In tUserDataSizes
                                    If Not IsDBNull(tmpRow("ProjectID")) AndAlso Not IsDBNull(tmpRow("ProjectID")) AndAlso CInt(tmpRow("ProjectID")) = tProject.ID Then
                                        tUsrDataSize = CLng(tmpRow("FullSize"))
                                        tUserDataSizes.Remove(tmpRow)
                                        Exit For
                                    End If
                                Next
                            End If

                            Dim CntManual As Integer = 0
                            Dim CntAuto As Integer = 0
                            Dim SnapshotsSize As Long = 0
                            If tSnapshotSizes IsNot Nothing Then
                                For Each tmpRow As Dictionary(Of String, Object) In tSnapshotSizes
                                    If Not IsDBNull(tmpRow("ProjectID")) AndAlso Not IsDBNull(tmpRow("ProjectID")) AndAlso CInt(tmpRow("ProjectID")) = tProject.ID Then
                                        SnapshotsSize += CLng(tmpRow("FullSize"))
                                        Dim Type As Integer = CInt(tmpRow("SnapshotType"))
                                        Dim Cnt As Integer = CInt(tmpRow("Cnt"))
                                        Select Case CType(Type, ecSnapShotType)
                                            Case ecSnapShotType.Manual
                                                CntManual += Cnt
                                            Case Else
                                                CntAuto += Cnt
                                        End Select
                                    End If
                                Next
                            End If

                            'PrjID, PrjName, PrjComment, Passcode, Passcode2, PrjEnabled, PrjStatus, Online, Created, LastVisited, Modified, Lock, TT, Online, SizeStreams, SizeSnapshots, SizeTotal, CountManual, CountAuto, CountTotal
                            sResult += CStr(If(sResult = "", "", ", ")) + String.Format("[{0},'{1}','{2}','{3}','{4}',{5},'{6}','{7}','{8}','{9}','{10}','{11}',{12},{13},{14},{15},{16},{17},{18},{19}]",
                                                                                    tProject.ID, JS_SafeString(tProject.ProjectName), JS_SafeString(If(tProject.Comment = "", "", tProject.Comment).Trim),
                                                                                    JS_SafeString(tProject.PasscodeLikelihood), JS_SafeString(If(tProject.PasscodeImpact = tProject.PasscodeLikelihood, "", tProject.PasscodeImpact)),
                                                                                    If(tProject.isMarkedAsDeleted, 0, 1), JS_SafeString(sStatus), JS_SafeString(If(tProject.isOnline, ResString("lblYes"), "")),
                                                                                    sCreated, sVisited, sModified, JS_SafeString(sLock), If(tProject.isTeamTimeImpact, 2, If(tProject.isTeamTimeLikelihood, 1, 0)),
                                                                                    clsOnlineUserSession.OnlineSessionsByProjectID(tProject.ID, OnlineUsers).Count,
                                                                                    tStructSize + tUsrDataSize, SnapshotsSize, tStructSize + tUsrDataSize + SnapshotsSize, CntManual, CntAuto, CntManual + CntAuto)
                        End If
                    Next

                    sResult = "[" + sResult + "]"

            End Select

            If sResult <> "" Then
                RawResponseStart()
                Response.ContentType = "text/plain"
                'Response.AddHeader("Content-Length", CStr(sResult))
                Response.Write(sResult)
                Response.End()
                'RawResponseEnd()
            End If


        End If
    End Sub

End Class