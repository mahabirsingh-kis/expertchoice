Imports System.Data.Common

Namespace ExpertChoice.Structuring

    Public Class StructuringAdmin

        Private connString As String

        Dim mDB As ExpertChoice.Database.clsDatabaseHelper
        Private ReadOnly Property DB() As ExpertChoice.Database.clsDatabaseHelper
            Get
                If mDB Is Nothing Then
                    Try
                        Dim CanvasMasterDBName = ExpertChoice.Service.WebConfigOption(ExpertChoice.Web.WebOptions._OPT_CANVASMASTERDB, "CoreDB", True)
                        connString = ExpertChoice.Data.clsDatabaseAdvanced.GetConnectionString(CanvasMasterDBName, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient)
                        Dim cd As ExpertChoice.Database.clsConnectionDefinition
                        cd = ExpertChoice.Database.getConnectionDefinition(CanvasMasterDBName, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient)
                        mDB = New ExpertChoice.Database.clsDatabaseHelper(cd)
                        mDB.Connect()
                    Catch ex As Exception
                        mDB = Nothing
                    End Try
                End If
                Return mDB
            End Get
        End Property

        Sub New()
        End Sub

        Public Function Connect() As Boolean
            Dim retVal As Boolean = False

            Try
                retVal = DB.Connect
            Catch ex As Exception
                retVal = False
            End Try

            Return retVal
        End Function

        Private Function Authenticated() As Boolean
            Return True
        End Function

        Private Function GetNewMeetingID() As Integer
            Dim retVal As Integer = -1
            Dim SQL As String

            Do
                retVal = rnd.Next(100000000, 999999999)
                SQL = "SELECT COUNT(*) FROM StructureMeetings WHERE MeetingID = " & retVal.ToString
            Loop While CInt(DB.ExecuteScalar(SQL)) > 0
            Return retVal
        End Function

        Dim rnd As New System.Random

        Private Function GetNewTokenID() As Integer

            Dim SQL As String
            Dim retVal As Integer

            Try
                Do
                    retVal = rnd.Next(100000000, 999999999)
                    SQL = "SELECT COUNT(*) FROM StructureTokens WHERE TokenID = " & retVal.ToString
                Loop While CInt(DB.ExecuteScalar(SQL)) > 0
            Catch ex As Exception
                retVal = -1
            End Try

            Return retVal
        End Function

        Public Function GetAdminToken(ByVal MeetingID As Integer) As UserToken

            Dim SQL As String = Nothing
            Dim retVal As UserToken = Nothing

            Dim dr As DbDataReader = Nothing
            Try
                SQL = String.Format("SELECT * FROM StructureTokens WHERE MeetingID = {0} and ClientType = {1} ", MeetingID, CInt(ClientType.Owner))
                dr = DB.ExecuteReader(SQL)
                If dr.Read Then
                    retVal = New UserToken
                    retVal.TokenID = CInt(dr("TokenID"))
                    retVal.MeetingID = CInt(dr("MeetingID"))
                    retVal.Email = dr("Email").ToString.Trim
                    retVal.ClientType = CType(dr("ClientType"), ClientType)
                    retVal.UserName = dr("UserName").ToString.Trim
                End If
            Catch ex As Exception
                retVal = Nothing
            Finally
                If dr IsNot Nothing AndAlso Not dr.IsClosed Then
                    dr.Close()
                End If
            End Try
            Return retVal

        End Function

        Public Function CreateMeeting(ByVal OwnerEmail As String, ByVal Name As String, ByVal ProjectID As Integer, ByVal MeetingPassword As String) As UserToken

            Dim ui As UserToken = Nothing

            'Create a new meeting in the DB
            If Authenticated() Then

                ui = New UserToken
                ui.TokenID = GetNewTokenID()
                ui.MeetingID = GetNewMeetingID()
                ui.Email = OwnerEmail
                ui.UserName = Name
                ui.ClientType = ClientType.Owner

                If ui.TokenID = -1 Or ui.MeetingID = -1 Then
                    ui = Nothing
                Else
                    Dim SQL As String

                    Try
                        DB.BeginTransaction()
                        SQL = String.Format("INSERT INTO StructureMeetings VALUES ({0}, '{1}', '{2}', '{3}', {4}, {5})", ui.MeetingID, MeetingPassword, OwnerEmail, Name, ProjectID, 0)
                        DB.ExecuteNonQuery(SQL, CommandType.Text, Database.ConnectionState.KeepOpen)

                        SQL = String.Format("INSERT INTO StructureTokens VALUES ({0}, {1}, '{2}', '{3}', {4})", ui.TokenID, ui.MeetingID, ui.Email, ui.UserName, CInt(ClientType.Owner))
                        DB.ExecuteNonQuery(SQL, CommandType.Text, Database.ConnectionState.KeepOpen)

                        DB.CommitTransaction()
                    Catch ex As Exception
                        Stop
                        DB.RollbackTransaction()
                        ui = Nothing
                    End Try
                End If

            End If

            Return ui

        End Function

        Public Function DeleteMeeting(ByVal MeetingID As Integer) As Boolean
            Dim retVal As Boolean = False

            If Authenticated() Then

                Try
                    Dim SQL As String
                    DB.BeginTransaction()
                    SQL = "Delete from StructureMeetings WHERE MeetingID = " & MeetingID
                    DB.ExecuteNonQuery(SQL)

                    SQL = "Delete from StructureMeetings WHERE MeetingID = " & MeetingID
                    DB.ExecuteNonQuery(SQL)

                    DB.CommitTransaction()
                    retVal = True
                Catch ex As System.Data.Common.DbException
                    DB.RollbackTransaction()
                    retVal = False
                End Try
            End If

            Return retVal

        End Function

        Private Function DBMeetings(Optional ByVal OwnerEmail As String = "") As Collection
            Dim c As New Collection

            Return c
        End Function

        Private Function GetDBMeetings(Optional ByVal OwnerEmail As String = "", Optional ByVal ProjectID As Integer = -1) As List(Of MeetingInfo)  ' D1205

            Dim retVal As List(Of MeetingInfo) = Nothing

            If Authenticated() Then
                retVal = New List(Of MeetingInfo)

                Dim SQL As String = "select * from StructureMeetings"
                If OwnerEmail.Length > 0 Then
                    SQL += String.Format(" WHERE OwnerEmail = '{0}'", OwnerEmail)
                End If

                If ProjectID > 0 Then SQL += String.Format(" AND ProjectID = {0}", ProjectID) ' D1205

                Dim dr As DbDataReader = Nothing
                Try
                    dr = DB.ExecuteReader(SQL)
                    While dr.Read
                        Dim m As New MeetingInfo() 'A0092
                        m.MeetingID = CInt(dr("MeetingID"))
                        m.MeetingPassword = dr("Password").ToString.Trim
                        m.OwnerEmail = dr("OwnerEmail").ToString.Trim
                        m.OwnerName = dr("OwnerName").ToString.Trim
                        m.ProjectID = CInt(dr("ProjectID"))
                        m.State = CType(dr("State"), MeetingState)
                        retVal.Add(m)
                    End While
                Catch ex As Exception
                    retVal = Nothing
                Finally
                    If dr IsNot Nothing AndAlso Not dr.IsClosed Then
                        dr.Close()
                    End If
                End Try
            End If

            Return retVal

        End Function

        Public Function GetMeetings() As List(Of MeetingInfo)

            Return GetDBMeetings()

        End Function

        ' D1205 ===
        Public Function GetMeetingByOwnerProjectID(ByVal OwnerEmail As String, ByVal ProjectID As Integer) As MeetingInfo
            Dim tRes As List(Of MeetingInfo) = GetDBMeetings(OwnerEmail, ProjectID)
            If tRes IsNot Nothing AndAlso tRes.Count > 0 Then Return tRes(0) Else Return Nothing
        End Function
        ' D1205 ==

        Public Function GetMeetingsByOwner(ByVal OwnerEmail As String) As List(Of MeetingInfo)

            Return GetDBMeetings(OwnerEmail)

        End Function

        Public Function SetMeetingPassword(ByVal MeetingID As Integer, ByVal Password As String) As Boolean

            Dim retVal As Boolean = False

            If Authenticated() Then

                Dim SQL As String
                Try
                    DB.BeginTransaction()
                    SQL = String.Format("Update StructureMeetings set Password = '{0}' where MeetingID = '{1}';", Password, MeetingID)
                    DB.ExecuteNonQuery(SQL, CommandType.Text, Database.ConnectionState.KeepOpen)
                    DB.CommitTransaction()
                    retVal = True
                Catch ex As Exception
                    DB.RollbackTransaction()
                    retVal = False
                End Try
            End If

            Return retVal

        End Function

    End Class

End Namespace
