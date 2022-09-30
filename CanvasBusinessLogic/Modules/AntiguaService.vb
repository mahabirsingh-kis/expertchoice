Imports ExpertChoice.Data
Imports GenericDBAccess.ECGenericDatabaseAccess

Namespace ExpertChoice.Service

    Public Module AntiguaService

        ' D0589 ===
        Public Function ProjectLockInfoSet(ByVal sConnectionString As String, ByVal tProviderType As DBProviderType, ByVal tProjectID As Integer, ByRef sLockUserEmail As String, ByVal tLockStatus As ECLockStatus, ByVal tExpiration As Nullable(Of DateTime)) As Boolean
            Dim fResult As Boolean = False
            Using tDatabase As New clsDatabaseAdvanced(sConnectionString, tProviderType)    ' D2235
                If tDatabase.Connect Then
                    If tLockStatus <> ECLockStatus.lsUnLocked Then
                        If Not tExpiration.HasValue Then tExpiration = Now.AddSeconds(_DEF_LOCK_TIMEOUT)
                        If tExpiration < Now Then tLockStatus = ECLockStatus.lsUnLocked
                    Else
                        If Not tExpiration.HasValue Then tExpiration = Now
                    End If
                    Dim tParams As New List(Of Object)
                    tParams.Add(sLockUserEmail)
                    Dim tData As Object = tDatabase.ExecuteScalarSQL("SELECT ID FROM Users WHERE Email LIKE ?", tParams)
                    Dim UID As Integer = -1
                    If tData IsNot Nothing AndAlso Not IsDBNull(tData) Then UID = CInt(tData) Else DebugInfo(String.Format("User with email '{0}' not found!", sLockUserEmail), _TRACE_WARNING)
                    DebugInfo(String.Format("Get UserID: {0} (email was '{1}')", UID, sLockUserEmail))

                    If tProjectID > 0 AndAlso UID > 0 Then
                        tParams.Clear()
                        tParams.Add(tExpiration.Value)
                        fResult = tDatabase.ExecuteSQL(String.Format("UPDATE Projects SET LockStatus={0}, LockedByUserID={1}, LockExpiration=? WHERE ID={2}", CInt(tLockStatus), UID, tProjectID), tParams) > 0
                        DebugInfo(String.Format("Set project expiration {0}/{2}: {3} (prj #{1})", tLockStatus, tProjectID, tExpiration, fResult))
                    Else
                        DebugInfo("Can't set lock info for unknown decision or unknown user", _TRACE_WARNING)
                    End If

                    tDatabase.Close()
                Else
                    DebugInfo("Can't open DB connection", _TRACE_WARNING)
                End If
            End Using
            Return fResult
        End Function
        ' D0589 ==


    End Module

End Namespace
