Partial Class StructuringServicePage
    Inherits Page

    Public Const LongPollMaxTime As Integer = 30 * 1 ' sec.
    Public Const OPT_LongPollDBCheckTime As Integer = 2000 ' millisec. - Individual mode or anonymous
    Public Const OPT_LongPollDBCheckTimePM As Integer = 500 ' millisec. - Meeting Owner when there are > 1 participant in the meeting

    Public Property tPollingUserID As Integer = Integer.MinValue
    Public Property tProjectID As Integer = Integer.MinValue


    Private _Application As clsComparionCore
    Public ReadOnly Property App() As clsComparionCore
        Get
            If _Application Is Nothing Then
                _Application = New clsComparionCore
                DebugInfo("Application object was created from CS polling service (HTML)")
                LoadComparionCoreOptions(_Application)
                _Application.CheckCanvasMasterDBDefaults()
            End If
            Return _Application
        End Get
    End Property

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        Dim args As NameValueCollection = Request.Form
        Dim sAction As String = GetParam(args, "action")
        Dim tResult As String = ""

        Select Case sAction
            Case "poll"
                tPollingUserID = CInt(GetParam(args, "polling_user_id"))
                tProjectID = CInt(GetParam(args, "prjid"))

                Dim IsPM As Boolean = CInt(GetParam(args, "is_pm")) > 0, ParticipantsCount As Integer = CInt(GetParam(args, "participant_count"))

                If tProjectID >= 0 Then 
                    Dim TimeStamp As Long = CLng(GetParam(args, "timestamp"))
                    Dim LongPollDBCheckTime As Integer = If(IsPM AndAlso ParticipantsCount > 1, OPT_LongPollDBCheckTimePM, OPT_LongPollDBCheckTime), 
                        N As Integer = CInt(LongPollMaxTime / (LongPollDBCheckTime / 1000))
                    Dim DT As Nullable(Of DateTime) = Nothing
                    Dim fExitPoll As Boolean = False

                    If Not IsPM Then
                        Dim argsTS As New AntiguaOperationEventArgs With { .CmdCode = Command.PollTimeStamp, .CmdOwner = tPollingUserID, .DT = DateTime.Now.Ticks, .isAnonymousAction = True }
                        Dim sData = String.Format("{0}{1}{2}{1}{3}", argsTS.DT, vbTab, If(argsTS.isAnonymousAction, 1, 0), argsTS.GetJSON())
                        App.DBTeamTimeDataWrite(tProjectID, COMBINED_USER_ID, ecExtraProperty.StructuringJsonData, sData, False)
                    End If

                    For i As Integer = 0 To N - 1
                        Dim tList As List(Of String) = App.DBTeamTimeDataReadAll(tProjectID, ecExtraProperty.StructuringJsonData, COMBINED_USER_ID, False)

                        If tList.Count > 0 Then
                            Dim retVal As String = "", sDataToStore As String = ""
                            Dim lastTickToStore As Long = 0, rowDT As Long, doCleanup As Boolean = False

                            ' Cleanup the commands that are older than 1.LongPollMaxTime + 1 minute - check every 1 minute and on poll start
                            'If IsPM AndAlso ((i = 0) OrElse ((i + 1) Mod (60 / (LongPollDBCheckTime / 1000))) = 0) Then
                            If IsPM AndAlso i = 0 Then
                                'lastTickToStore = DateTime.Now.Ticks - CLng(TimeSpan.TicksPerSecond * (LongPollMaxTime + 60))
                                lastTickToStore = DateTime.Now.Ticks - CLng(TimeSpan.TicksPerSecond * (OPT_LongPollDBCheckTime * 3)) ' store only last 5000 * 3 = 15 seconds
                                doCleanup = True
                            End If

                            For Each sData As String In tList
                                Dim row As String() = sData.Split(CChar(vbTab))
                                If row.Length = 3 Then
                                    rowDT = CLng(row(0))
                                    If rowDT > TimeStamp Then 
                                        Dim isAnonymousAction As Boolean = row(1) = "1"
                                        If IsPM OrElse Not isAnonymousAction Then ' only meeting organizer can write DB actions
                                            retVal += If(retVal = "", "", ",") + row(2)
                                        End If
                                    End If

                                    If doCleanup AndAlso sDataToStore = "" AndAlso rowDT < lastTickToStore Then
                                        sDataToStore = "has old records"
                                    End If
                                End If
                            Next

                            If doCleanup AndAlso sDataToStore <> "" Then 
                                App.DBTeamTimeDataDelete(tProjectID, DateTime.FromBinary(lastTickToStore), ecExtraProperty.StructuringJsonData,  COMBINED_USER_ID)
                            End If

                            If retVal <> "" Then
                                retVal = String.Format("[{0}]", retVal)
                                Dim fRes As jActionResult = New jActionResult With {.Data = retVal, .Result = ecActionResult.arSuccess, .Tag = rowDT.ToString}
                                tResult = fRes.ToJSON
                                fExitPoll = True
                                Exit For
                            Else
                                Thread.Sleep(LongPollDBCheckTime)
                            End If
                        Else
                            Thread.Sleep(LongPollDBCheckTime)
                        End If
                    Next
                    If Not fExitPoll Then 
                        tResult = (New jActionResult With {.Data = "poll_completed", .Result = ecActionResult.arSuccess, .Tag = TimeStamp.ToString}).ToJSON
                    End If
                End If
        End Select

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Private Sub StructuringServicePage_Error(sender As Object, e As EventArgs) Handles Me.[Error], Me.AbortTransaction
        If tPollingUserID <> Integer.MinValue Then
            Dim args As New AntiguaConnectOperationEventArgs With { .CmdCode = Command.DisconnectUser, .CmdOwner = tPollingUserID, .DT = DateTime.Now.Ticks }
            Dim sData = String.Format("{0}{1}{2}{1}{3}", args.DT, vbTab, If(args.isAnonymousAction, 1, 0), args.GetJSON())
            App.DBTeamTimeDataWrite(tProjectID, COMBINED_USER_ID, ecExtraProperty.StructuringJsonData, sData, False)
        End If
    End Sub

End Class