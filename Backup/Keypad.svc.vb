Imports Expertchoice.Data
Imports System.Data.Common
Imports ECCore

Namespace Expertchoice.TeamTime

    <ServiceBehavior(InstanceContextMode:=InstanceContextMode.Single)> _
    <WCFTTAExceptionWrapper()> _
    Public Class Keypad
        Implements IKeypad, IKeypadPrivilleged, IDisposable

        Dim ClientCleanup As New Timers.Timer

        Sub New()
            UserConnections.Clear()
            Call InitDB()
            'ClientCleanup.Interval = 30 * 1000
            'AddHandler ClientCleanup.Elapsed, AddressOf TimerCleanup
            'ClientCleanup.Start()
        End Sub

        Private Sub TimerCleanup()
        End Sub

#Region "Service Functions"

        Public Function GetMeetingStatus() As MeetingStatus Implements IKeypad.GetMeetingStatus
            Dim retVal As MeetingStatus = MeetingStatus.Unknown

            Try
                If mMeetingStatus.ContainsKey(CurrentUser.MeetingID) Then
                    retVal = mMeetingStatus(CurrentUser.MeetingID)
                End If
            Catch ex As Exception
            End Try

            If retVal <> MeetingStatus.MeetingEnded Then
                retVal = GetCurrentStep()
            End If

            Return retVal
        End Function

        Public Function MaxKeypadNum() As Integer Implements IKeypad.MaxKeypadNum
            Dim retVal As Integer = 0
            If CurrentUser() Is Nothing Then
                retVal = -1
            Else
                retVal = GetMaxKeypadNumber()
            End If
            Return retVal
        End Function

        Public Function GetUserInfo() As KeypadData Implements IKeypad.GetUsers

            Dim kpd As New KeypadData

            Try
                Dim tWSList As List(Of Expertchoice.Data.clsWorkspace) = WebCore.DBWorkspacesByProjectID(WebCore.ProjectID)
                Dim ProjectUsers As List(Of Expertchoice.Data.clsApplicationUser) = WebCore.DBUsersByProjectID(WebCore.ActiveProject.ID)

                For Each tAHPUser As ECCore.clsUser In WebCore.ActiveProject.ProjectManager.UsersList

                    Dim tUser As Expertchoice.Data.clsApplicationUser = Nothing
                    tUser = Expertchoice.Data.clsApplicationUser.UserByUserEmail(tAHPUser.UserEMail, ProjectUsers)

                    If tUser IsNot Nothing Then
                        Dim tWs As Expertchoice.Data.clsWorkspace = Expertchoice.Data.clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, WebCore.ProjectID, tWSList)

                        If tWs.isInTeamTime(WebCore.ActiveProject.isImpact) AndAlso tAHPUser.SyncEvaluationMode = ECCore.SynchronousEvaluationMode.semVotingBox Then ' D1945
                            Dim kpi As New KeyPadInfo
                            kpi.KeypadNumber = tAHPUser.VotingBoxID
                            kpi.UserEmail = tAHPUser.UserEMail
                            kpi.UserName = tAHPUser.UserName
                            kpi.Value = Nothing
                            kpd.Keypads.Add(kpi)
                        End If
                    End If
                Next
            Catch ex As Exception
                kpd = Nothing
            End Try

            Return kpd


        End Function


        Public Function SubmitKeyPadData(ByVal Data As KeypadData) As Boolean Implements IKeypad.SubmitKeyPadData

            'Dim msg As String = String.Format("{0} with pad {1} pressed {2}.  Email = {3}", Data.Keypads(0).UserName, Data.Keypads(0).KeypadNumber, Data.Keypads(0).Value, Data.Keypads(0).UserEmail)
            'Debug.WriteLine(msg)

            Dim retVal As Boolean = False

            Dim SessionData As New Dictionary(Of String, String)

            If CurrentUser() Is Nothing OrElse GetCurrentStep(SessionData) <> MeetingStatus.PollingOn Then
                retVal = False
            Else
                For Each k As KeyPadInfo In Data.Keypads
                    SendKeyPressToCore(k, SessionData)
                Next
                retVal = True
            End If

            Return retVal
        End Function

#End Region

#Region "AuthTokens"

        Private _TABLE_AuthTokens As String = "AuthTokens"
        Private _FLD_AuthTokens_MeetingID As String = "MeetingID"
        Private _FLD_AuthTokens_AuthToken As String = "AuthToken"
        Private _FLD_AuthTokens_Master As String = "Master"

        Private DB As Expertchoice.Database.clsDatabaseHelper

        Private Sub InitDB()
            Dim cd As Expertchoice.Database.clsConnectionDefinition
            cd = Expertchoice.Database.getConnectionDefinition(System.Configuration.ConfigurationManager.AppSettings("SQLMasterDB"), GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient)
            DB = New Expertchoice.Database.clsDatabaseHelper(cd)
        End Sub

        Private mRegistered As New Collection
        Public Function Register(ByVal MeetingID As String, ByVal AuthorizationToken As Integer, ByVal email As String) As Boolean Implements IKeypad.Register
            MeetingID = Strip(MeetingID)

            Call InitDB()

            Dim MasterValue As Boolean
            Dim retVal As Boolean = True

            Try

                Dim SQL As String = String.Format("select Master from AuthTokens where MeetingID = '{0}' and AuthToken = {1}", MeetingID, AuthorizationToken)
                Dim dr As DbDataReader = DB.ExecuteReader(SQL)
                If dr.Read Then
                    MasterValue = dr(0) <> 0
                Else
                    retVal = False
                End If
                dr.Close()

                Dim Password As String = ""
                If retVal Then
                    Try
                        SQL = String.Format("select password, ID from users where email = '{0}'", email)
                        dr = DB.ExecuteReader(SQL)
                        If dr.Read Then
                            Password = dr(0)
                            Password = Expertchoice.Service.CryptService.DecodeString(Password, Nothing)    ' D0827
                        End If
                    Catch ex As Exception
                        Throw New Exception("Could not load credentials")
                    End Try
                End If

                If retVal Then
                    ' grab the callback channel and add it to dictionary of users
                    Dim ti As New TokenInfo()
                    ti.MeetingID = MeetingID
                    ti.AuthToken = AuthorizationToken
                    ti.Client = IIf(MasterValue, ClientType.Super, ClientType.Regular)
                    AddUserConnection(OperationContext.Current.SessionId, MeetingID, ti)

                    'Dim AuthResult As Expertchoice.Data.ecAuthenticateError = WebCore.Logon(email, Password, "", 0, False)
                    Dim AuthResult As Expertchoice.Data.ecAuthenticateError = WebCore.Logon(email, Password, "", False, True, False)    ' D7327
                End If

            Catch ex As Exception
                retVal = False
            End Try

            If retVal Then
                Try
                    Dim pi As Expertchoice.Data.clsProject = WebCore.DBProjectByMeetingID(MeetingID.Replace("-", ""))
                    If WebCore.ActiveWorkgroup Is Nothing Then
                        WebCore.ActiveWorkgroup = WebCore.DBWorkgroupByID(pi.WorkgroupID)
                    End If
                    If Not WebCore.HasActiveProject Then
                        WebCore.ProjectID = pi.ID
                    End If
                Catch ex As Exception
                    retVal = False
                End Try

            End If

            If retVal Then
                Try
                    mRegistered.Add(AuthorizationToken, CurrentUser.SessionID)
                Catch ex As Exception
                End Try
            End If

            Return retVal

        End Function

#End Region

#Region "Managing dictionary of users"
        Protected Friend UserConnections As New Collection

        Private Function CurrentUser() As UserConnection

            Dim m As UserConnection = Nothing

            If UserConnections.Contains(OperationContext.Current.SessionId) Then
                m = UserConnections(OperationContext.Current.SessionId)
            End If

            Return m

        End Function

        Private Sub AddUserConnection(ByVal sessionID As String, ByVal MeetingID As String, ByVal ClientToken As TokenInfo)
            MeetingID = Strip(MeetingID)
            If Not UserConnections.Contains(sessionID) Then
                Dim m As New UserConnection(OperationContext.Current, MeetingID)
                m.Token = ClientToken
                UserConnections.Add(m, m.SessionID)
            End If
        End Sub

        Public Sub Disconnect() Implements IKeypad.Disconnect
            Try
                UserConnections.Remove(CurrentUser.SessionID)
            Catch ex As Exception
            End Try
        End Sub

        Public Sub OnConnectionClosing(ByVal sender As Object, ByVal e As EventArgs)
        End Sub

        Public Sub OnConnectionClosed(ByVal sender As Object, ByVal e As EventArgs)

            For Each u As UserConnection In UserConnections
                If u.Context.Channel.State <> CommunicationState.Opened Or u.Context.Channel.State <> CommunicationState.Opening Then
                    Try
                        UserConnections.Remove(u.SessionID)
                    Catch ex As Exception
                    End Try
                End If
            Next

        End Sub

#End Region

#Region "Privileged Functions"

        Private Function Strip(ByVal ID As String) As String
            Return ID.Replace("-", "")
        End Function

        Public Function AddToken(ByVal MeetingID As String, ByVal Client As ClientType) As Integer Implements IKeypadPrivilleged.AddToken
            MeetingID = Strip(MeetingID)

            If mMeetingStatus.ContainsKey(MeetingID) Then
                mMeetingStatus.Remove(MeetingID)
            End If

            Dim rnd As New System.Random
            Dim token As Integer

            Try
                Dim sSQL As String = ""

                Dim firstSuper As Integer = 0
                If Client = ClientType.Super Then 'if there is more than one master for this meeting, delete the extras
                    sSQL = String.Format("SELECT {0} FROM {1} WHERE {2}='{3}' AND {4}<>0", _FLD_AuthTokens_Master, _TABLE_AuthTokens, _FLD_AuthTokens_MeetingID, MeetingID, _FLD_AuthTokens_Master)
                    Dim rdr As DbDataReader = DB.ExecuteReader(sSQL)

                    Dim supercount As Integer = 0
                    If Not rdr Is Nothing Then
                        While rdr.Read
                            supercount += 1
                            If supercount = 1 Then
                                firstSuper = rdr(0)
                            End If
                        End While
                    End If
                    rdr.Close()
                    If supercount > 1 Then
                        sSQL = String.Format("DELETE FROM {0} WHERE {1}='{2}' AND {3}<>{4}", _TABLE_AuthTokens, _FLD_AuthTokens_MeetingID, MeetingID, _FLD_AuthTokens_Master, firstSuper)
                        DB.ExecuteNonQuery(sSQL)
                    End If
                End If

                If Client = ClientType.Regular Or firstSuper = 0 Then
                    Do
                        token = rnd.Next(1000000, 9999999)
                        sSQL = String.Format("SELECT COUNT(*) FROM {0} WHERE {1}='{2}' AND {3}={4}", _TABLE_AuthTokens, _FLD_AuthTokens_MeetingID, MeetingID, _FLD_AuthTokens_AuthToken, token)
                    Loop While DB.ExecuteScalar(sSQL) > 0

                    sSQL = String.Format("INSERT INTO {0} VALUES ('{1}', {2}, {3})", _TABLE_AuthTokens, MeetingID, token, IIf(Client = ClientType.Super, 1, 0))
                    DB.ExecuteNonQuery(sSQL)
                Else
                    token = firstSuper
                End If

            Catch ex As System.Data.Common.DbException
                token = -1
            Finally
            End Try

            Return token
        End Function

        Public Sub ClearTokens(ByVal MeetingID As String) Implements IKeypadPrivilleged.ClearTokens
            MeetingID = Strip(MeetingID)
            Try
                Dim sSQL As String = String.Format("DELETE FROM {0} WHERE {1}='{2}'", _TABLE_AuthTokens, _FLD_AuthTokens_MeetingID, MeetingID)
                DB.ExecuteNonQuery(sSQL)
            Catch ex As Exception
            Finally
            End Try
        End Sub

        Public Sub DeleteToken(ByVal MeetingID As String, ByVal AuthToken As Integer) Implements IKeypadPrivilleged.DeleteToken
            MeetingID = Strip(MeetingID)
            Try
                Dim sSQL As String = String.Format("DELETE FROM {0} WHERE {1}='{2}'AND {3}={4}", _TABLE_AuthTokens, _FLD_AuthTokens_MeetingID, MeetingID, _FLD_AuthTokens_AuthToken, AuthToken)
                DB.ExecuteNonQuery(sSQL)
            Catch ex As Exception
            Finally
            End Try
        End Sub

        Public Function GetTokens(ByVal MeetingID As String) As System.Collections.Generic.List(Of TokenInfo) Implements IKeypadPrivilleged.GetTokens
            MeetingID = Strip(MeetingID)
            Dim retVal As New List(Of TokenInfo)
            Return retVal

            'Try

            '    Dim sSQL As String = String.Format("SELECT {0}, {1} FROM {2} WHERE {3}='{4}'", _FLD_AuthTokens_AuthToken, _FLD_AuthTokens_Master, _TABLE_AuthTokens, _FLD_AuthTokens_MeetingID, MeetingID)
            '    Dim RS As DbDataReader = DB.ExecuteReader(sSQL)

            '    If Not RS Is Nothing Then
            '        While RS.Read
            '            Dim ti As New TokenInfo
            '            ti.MeetingID = MeetingID
            '            ti.AuthToken = RS(0)
            '            retVal.Add(ti)
            '        End While
            '    End If
            '    RS.Close()
            'Catch ex As Exception
            'Finally
            'End Try

            'Return retVal

        End Function

        Public Function ServiceCheck() As Boolean Implements IKeypadPrivilleged.ServiceCheck
            Return True
        End Function

        Private mMeetingStatus As New Hashtable

        Public Sub EndMeeting(ByVal MeetingID As String) Implements IKeypadPrivilleged.EndMeeting
            MeetingID = Strip(MeetingID)
            If Not mMeetingStatus.ContainsKey(MeetingID) Then
                mMeetingStatus.Add(MeetingID, MeetingStatus.MeetingEnded)
            Else
                mMeetingStatus(MeetingID) = MeetingStatus.MeetingEnded
            End If
        End Sub

#End Region

#Region " IDisposable Support "
        Private disposedValue As Boolean = False        ' To detect redundant calls

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: free other state (managed objects).
                End If

                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#End Region

    End Class

End Namespace
