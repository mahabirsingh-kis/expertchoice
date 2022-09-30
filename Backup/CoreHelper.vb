Imports Expertchoice.Data
Imports ECCore
Imports Canvas

Namespace Expertchoice.TeamTime

    Partial Public Class Keypad

        Private mDirectValues As New Hashtable
        Private Property DirectValues(ByVal KeypadNumber As Integer) As String
            Get
                If Not mDirectValues.ContainsKey(CurrentUser.SessionID & KeypadNumber) Then
                    Return Nothing
                Else
                    Return CType(mDirectValues(CurrentUser.SessionID & KeypadNumber), String)
                End If
            End Get
            Set(ByVal value As String)
                mDirectValues(CurrentUser.SessionID & KeypadNumber) = value
            End Set
        End Property

        Dim mWebCore As New Hashtable
        Public Property WebCore() As clsComparionCore
            Get
                If Not mWebCore.ContainsKey(CurrentUser.SessionID) Then
                    Dim _app As New clsComparionCore
                    Expertchoice.Web.LoadComparionCoreOptions(_app)
                    mWebCore.Add(CurrentUser.SessionID, _app)
                End If
                Dim retVal As clsComparionCore = CType(mWebCore(CurrentUser.SessionID), clsComparionCore)
                Return retVal
            End Get
            Set(ByVal value As clsComparionCore)
                If mWebCore(CurrentUser.SessionID) Is Nothing Then
                    mWebCore.Add(CurrentUser.SessionID, value)
                Else
                    mWebCore(CurrentUser.SessionID) = value
                End If
            End Set
        End Property

        Private Sub UpdateUserKeypadNumber(ByVal Data As KeyPadInfo)
            For Each tAHPUser As clsUser In WebCore.ActiveProject.ProjectManager.UsersList
                Dim user As clsUser = WebCore.ActiveProject.ProjectManager.GetUserByEMail(Data.UserEmail)
                user.VotingBoxID = Data.KeypadNumber
                WebCore.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
            Next
        End Sub

        Private Function ConvertSessionData(SessionData As String) As Dictionary(Of String, String)
            Dim UserInfo As New Dictionary(Of String, String)

            Try
                Dim s As String = SessionData
                s = s.Remove(0, s.IndexOf("['data'"))

                Dim words As String() = s.Split("],")
                Dim startWord As Integer = -1
                Dim endWord As Integer = -1

                Dim word As String
                Dim i As Integer = 0
                For Each word In words
                    If Left(word, 3) = ",[[" Then
                        startWord = i
                        words(i) = word.Replace(",[[", ",[")
                    End If
                    i = i + 1
                    If startWord >= 0 And word = "" Then
                        endWord = i - 2
                        Exit For
                    End If
                Next
                For j As Integer = startWord To endWord
                    Dim ui As String = words(j).Replace(",[", "")
                    Dim parse As String() = ui.Split(",")
                    Dim key As String = ui.Remove(ui.IndexOf(","))
                    UserInfo.Add(parse(1).Replace("'", ""), ui)
                Next
            Catch ex As Exception
            End Try

            Return UserInfo
        End Function

        Private Function GetCurrentStep(Optional ByRef SessionData As Dictionary(Of String, String) = Nothing) As MeetingStatus
            Dim retVal As MeetingStatus = MeetingStatus.Unknown
            Try
                If WebCore.HasActiveProject Then

                    Dim sSessionData As String = WebCore.DBTeamTimeDataRead(WebCore.ProjectID, WebCore.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionData, Nothing)
                    SessionData = ConvertSessionData(sSessionData)

                    If sSessionData Is Nothing Then
                        retVal = MeetingStatus.PollingOff
                    Else
                        Dim sStepData As String = WebCore.DBTeamTimeDataRead(WebCore.ProjectID, WebCore.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeStepData, Nothing)
                        If sStepData <> "" Then
                            Dim sParams As String() = sStepData.Split(clsTeamTimePipe.Judgment_Delimeter)
                            If sParams.Count >= 3 AndAlso sParams(0) <> "" AndAlso sParams(2) <> "" Then
                                Select Case CInt(sParams(1))
                                    Case ActionType.atPairwise, ActionType.atNonPWOneAtATime
                                        retVal = MeetingStatus.PollingOn
                                    Case Else
                                        retVal = MeetingStatus.PollingOff
                                End Select
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
            End Try
            Return retVal
        End Function

        Private mLastStep As Integer = -1
        Private SignDirection As New Dictionary(Of String, Integer)

        Private Function SendKeyPressToCore(ByVal Data As KeyPadInfo, SessionData As Dictionary(Of String, String)) As Boolean
            Dim retVal As Boolean = True

            Try

                ' D1350 ===
                If WebCore.HasActiveProject Then
                    Dim sStepData As String = WebCore.DBTeamTimeDataRead(WebCore.ProjectID, WebCore.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeStepData, Nothing)

                    ' If we have step info: TT session is active
                    If sStepData <> "" Then
                        Dim sParams As String() = sStepData.Split(clsTeamTimePipe.Judgment_Delimeter)
                        If sParams.Count >= 3 AndAlso sParams(0) <> "" AndAlso sParams(2) <> "" Then

                            Dim sStepID As String = sParams(0)

                            If sStepID <> mLastStep Then
                                mLastStep = sStepID
                                SignDirection = New Dictionary(Of String, Integer)
                            End If

                            Dim sStepGUID As String = sParams(2)
                            Dim sValue As String = "" ' there will be judgment

                            ' Check the type of current step
                            Select Case sParams(1)
                                Case CInt(ActionType.atPairwise).ToString
                                    Dim CurrentVal As Long
                                    Try
                                        CurrentVal = CLng(SessionData(Data.UserEmail).Split(",")(5))
                                    Catch ex As Exception
                                        CurrentVal = 0
                                    End Try

                                    If Data.Value = "*" Then
                                        Dim Direction As Integer = -1 'Default to left if no direction set
                                        If SignDirection.ContainsKey(Data.UserEmail) Then
                                            Direction = -SignDirection(Data.UserEmail)
                                            SignDirection.Remove(Data.UserEmail)
                                        Else
                                            Direction = -Direction
                                        End If
                                        SignDirection.Add(Data.UserEmail, Direction)
                                        If Math.Abs(CurrentVal) <= 8 Then
                                            sValue = Direction * Math.Abs(CurrentVal)
                                        Else
                                            sValue = Long.MinValue  'delete judgment because bad data already exists
                                        End If
                                    Else
                                        Dim LastDirection As Integer = IIf(CurrentVal < 0, -1, 1)
                                        If SignDirection.ContainsKey(Data.UserEmail) Then
                                            LastDirection = SignDirection(Data.UserEmail)
                                            SignDirection.Remove(Data.UserEmail)
                                        End If

                                        Dim newVal As Integer = CInt(Data.Value.ToString)
                                        If newVal > 0 Then
                                            sValue = (newVal - 1) * LastDirection
                                        Else
                                            sValue = Long.MinValue
                                        End If
                                        SignDirection.Add(Data.UserEmail, LastDirection)
                                    End If
                                Case CInt(ActionType.atNonPWOneAtATime).ToString
                                    Select Case Data.Value
                                        Case "0"
                                            sValue = "-1"
                                        Case Else
                                            Dim tVal As Integer
                                            If Integer.TryParse(Data.Value, tVal) Then sValue = (tVal - 1).ToString
                                    End Select
                                    Dim Idx As Integer
                                    If Integer.TryParse(sValue, Idx) AndAlso sParams(3) <> "" Then
                                        Dim Itensities As String() = sParams(3).Split(","c)
                                        If Idx >= 0 AndAlso Idx < Itensities.Length Then sValue = Itensities(Idx)
                                    End If
                                Case Else   ' other steps
                            End Select

                            ' we have some info for write (or empty when non-supported steps: info, results, etc.)
                            If sValue <> "" Then
                                Dim prjUser As clsUser = WebCore.ActiveProject.ProjectManager.GetUserByEMail(Data.UserEmail)
                                If prjUser IsNot Nothing Then
                                    Dim sJudgment As String = String.Format("{1}{0}{2}{0}{3}{0}{4}{0}", clsTeamTimePipe.Judgment_Delimeter, prjUser.UserID, sStepID, sStepGUID, sValue)
                                    WebCore.DBTeamTimeDataWrite(WebCore.ProjectID, WebCore.ActiveUser.UserID, ecExtraProperty.TeamTimeJudgment, sJudgment, False)
                                End If
                            End If

                        End If
                    End If
                End If
            Catch ex As Exception
                retVal = False
                SaveLog(dbActionType.actMakeJudgment, dbObjectType.einfProject, WebCore.ProjectID, "SendKeypressToCore", ex.ToString)
            End Try

            Return retVal
        End Function

        Public Function SaveLog(ByVal tActionType As dbActionType, ByVal tObjectType As dbObjectType, ByVal tObjectID As Integer, ByVal sComment As String, ByVal sResult As String, Optional ByVal SendLog As Boolean = True) As Boolean   ' D0496
            If Not SendLog Then Exit Function
            If Expertchoice.Service.WebConfigOption("SaveLogs", "true", True).ToLower.Trim = "true" And WebCore.Database.Connect Then
                Console.WriteLine(sResult)
                WebCore.DBSaveLog(tActionType, tObjectType, tObjectID, sComment, sResult)
                Return True
            End If
            Return False
        End Function

        Private Function GetMaxKeypadNumber() As Integer

            Dim maxKeypadNum As Integer = 0
            For Each tUser As clsApplicationUser In WebCore.DBUsersByProjectID(WebCore.ProjectID)
                Dim tAHPUser As clsUser = WebCore.ActiveProject.ProjectManager.GetUserByEMail(tUser.UserEmail)
                If tAHPUser.VotingBoxID > maxKeypadNum Then
                    maxKeypadNum = tAHPUser.VotingBoxID
                End If
            Next
            Return maxKeypadNum

        End Function

    End Class

End Namespace
