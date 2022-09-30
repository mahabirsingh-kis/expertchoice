Imports ECCore
Imports GenericDBAccess.ECGenericDatabaseAccess
Imports System.Data.Common
Imports System.Data.OleDb

Public Class clsUserInfo
    Private mUserID As Integer
    Private mUserName As String
    Private mUserEmail As String
    Private mIncludedInMerge As Boolean
    Private mHasData As Boolean
    Private mNewUserID As Integer

    Public Property UserID() As Integer
        Get
            Return mUserID
        End Get
        Set(ByVal value As Integer)
            mUserID = value
        End Set
    End Property

    Public Property UserName() As String
        Get
            Return mUserName
        End Get
        Set(ByVal value As String)
            mUserName = value
        End Set
    End Property

    Public Property UserEmail() As String
        Get
            Return mUserEmail
        End Get
        Set(ByVal value As String)
            mUserEmail = value
        End Set
    End Property

    Public Property IncludedInMerge() As Boolean
        Get
            Return mIncludedInMerge

        End Get
        Set(ByVal value As Boolean)
            mIncludedInMerge = value
        End Set
    End Property

    Public Property HasData() As Boolean
        Get
            Return mHasData
        End Get
        Set(ByVal value As Boolean)
            mHasData = value
        End Set
    End Property

    Public Property NewUserID() As Integer
        Get
            Return mNewUserID
        End Get
        Set(ByVal value As Integer)
            mNewUserID = value
        End Set
    End Property
End Class

Public Class clsAHPMerger
    Private mFiles As List(Of String)
    Private mUserData As Dictionary(Of String, List(Of clsUserInfo))
    Private mMasterFilePath As String

    Property Files() As List(Of String)
        Get
            Return mFiles
        End Get
        Set(ByVal value As List(Of String))
            mFiles = value
        End Set
    End Property

    Public Property UserData() As Dictionary(Of String, List(Of clsUserInfo))
        Get
            Return mUserData
        End Get
        Set(ByVal value As Dictionary(Of String, List(Of clsUserInfo)))
            mUserData = value
        End Set
    End Property

    Public Property MasterFilePath() As String
        Get
            Return mMasterFilePath
        End Get
        Set(ByVal value As String)
            mMasterFilePath = value
        End Set
    End Property

    Private Function UserHasData(ByVal ConnectionString As String, ByVal UserID As Integer) As Boolean
        If Not CheckDBConnection(DBProviderType.dbptOLEDB, ConnectionString) Then
            Return Nothing
        End If

        Dim dbConnection As DbConnection = GetDBConnection(DBProviderType.dbptOLEDB, ConnectionString)
        dbConnection.Open()
        Dim oCommand As DbCommand = GetDBCommand(DBProviderType.dbptOLEDB)
        oCommand.Connection = dbConnection

        oCommand.CommandText = "SELECT COUNT(*) FROM Judgments WHERE PID=?"
        oCommand.Parameters.Clear()
        oCommand.Parameters.Add(GetDBParameter(DBProviderType.dbptOLEDB, "PID", UserID))

        Dim obj As Object = DBExecuteScalar(DBProviderType.dbptOLEDB, oCommand)
        Dim count As Integer = IIf(obj Is Nothing, 0, CType(obj, Integer))

        If count = 0 Then
            oCommand.CommandText = "SELECT COUNT(*) FROM AltsData WHERE PID=?"
            oCommand.Parameters.Clear()
            oCommand.Parameters.Add(GetDBParameter(DBProviderType.dbptOLEDB, "PID", UserID))

            obj = DBExecuteScalar(DBProviderType.dbptOLEDB, oCommand)
            count = IIf(obj Is Nothing, 0, CType(obj, Integer))
        End If

        oCommand = Nothing
        dbConnection.Close()

        Return count > 0
    End Function

    Public Function AddFile(ByVal FilePath As String) As List(Of clsUserInfo)
        mFiles.Add(FilePath)
        Dim res As New List(Of clsUserInfo)

        Dim connStr As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FilePath + ";User Id=admin;Password=;"
        Dim users As List(Of clsUser) = ECCore.MiscFuncs.GetUsersList(connStr, ECModelStorageType.mstAHPDatabase, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptOLEDB)

        If Not CheckDBConnection(DBProviderType.dbptOLEDB, connStr) Then
            Return Nothing
        End If


        If users IsNot Nothing Then
            Dim dbConnection As DbConnection = GetDBConnection(DBProviderType.dbptOLEDB, connStr)
            dbConnection.Open()
            Dim oCommand As DbCommand = GetDBCommand(DBProviderType.dbptOLEDB)
            oCommand.Connection = dbConnection

            For Each user As clsUser In users

                oCommand.CommandText = "SELECT COUNT(*) FROM Judgments WHERE PID=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(DBProviderType.dbptOLEDB, "PID", user.UserID))

                Dim obj As Object = DBExecuteScalar(DBProviderType.dbptOLEDB, oCommand)
                Dim count As Integer = IIf(obj Is Nothing, 0, CType(obj, Integer))

                If count = 0 Then
                    oCommand.CommandText = "SELECT COUNT(*) FROM AltsData WHERE PID=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(DBProviderType.dbptOLEDB, "PID", user.UserID))

                    obj = DBExecuteScalar(DBProviderType.dbptOLEDB, oCommand)
                    count = IIf(obj Is Nothing, 0, CType(obj, Integer))
                End If

                Dim uInfo As New clsUserInfo
                uInfo.UserID = user.UserID
                uInfo.UserName = user.UserName
                uInfo.UserEmail = user.UserEMail
                uInfo.IncludedInMerge = True
                uInfo.HasData = count > 0
                res.Add(uInfo)
            Next
            oCommand = Nothing
            dbConnection.Close()
        End If

        mUserData.Add(FilePath, res)
        Return res
    End Function

    Private Sub PrintDataTable(ByVal DT As DataTable)
        For Each row As DataRow In DT.Rows
            Dim s As String = ""
            For j As Integer = 0 To DT.Columns.Count - 1
                s += DT.Columns(j).ColumnName + "=" + row(j).ToString + "; "
            Next
            Debug.Print(s)
        Next
    End Sub

    Private Function GetMaxUserIDInMasterModel() As Integer
        Dim MasterUsers As List(Of clsUserInfo) = mUserData(mMasterFilePath)
        If MasterFilePath Is Nothing Then
            Return -1
        End If

        Dim maxID As Integer = -1

        For Each uInfo As clsUserInfo In MasterUsers
            If uInfo.UserID > maxID Then
                maxID = uInfo.UserID
            End If
        Next

        Return maxID
    End Function

    Private Function CreateNewUserIDs() As Integer
        Dim NextUserID = GetMaxUserIDInMasterModel() + 1

        For Each DE As KeyValuePair(Of String, List(Of clsUserInfo)) In UserData
            If DE.Key <> MasterFilePath Then
                For Each uInfo As clsUserInfo In DE.Value
                    If uInfo.HasData And uInfo.IncludedInMerge Then
                        uInfo.NewUserID = NextUserID
                        NextUserID += 1
                    Else
                        uInfo.NewUserID = -1
                    End If
                Next

            End If
        Next
        Return NextUserID
    End Function

    Public Sub ReportProgress(ByVal Message As String)
        frmMain.tbMerge.Text += Message
        frmMain.tbMerge.Focus()
        frmMain.tbMerge.ScrollToCaret()
        frmMain.tbMerge.Update()
    End Sub

    Public Sub Merge()
        Dim DestConnStr As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + MasterFilePath + ";User Id=admin;Password=;"
        If Not CheckDBConnection(DBProviderType.dbptOLEDB, DestConnStr) Then
            Exit Sub
        End If

        Dim DestDBConnection As DbConnection = GetDBConnection(DBProviderType.dbptOLEDB, DestConnStr)

        Dim SelectQueries As New List(Of String)
        Dim InsertQueries As New List(Of String)

        SelectQueries.Add("SELECT PID, WRT, N1, N2, J, [Note] FROM Judgments WHERE PID=?")
        SelectQueries.Add("SELECT PID, AID, WRT, DATA, [Note] FROM AltsData WHERE PID=?")
        SelectQueries.Add("SELECT PID, AID, WRT, ALTVALUE FROM AltsValues WHERE PID=?")

        InsertQueries.Add("INSERT INTO Judgments (PID, WRT, N1, N2, J, [Note]) VALUES (?, ?, ?, ?, ?, ?)")
        InsertQueries.Add("INSERT INTO AltsData (PID, AID, WRT, DATA, [Note]) VALUES (?, ?, ?, ?, ?)")
        InsertQueries.Add("INSERT INTO AltsValues (PID, AID, WRT, ALTVALUE) VALUES (?, ?, ?, ?)")

        Try
            Dim NextPersonIDForAHP As Integer = CreateNewUserIDs()

            Dim totalFiles As Integer = UserData.Count - 1
            Dim currentFile As Integer = 1

            For Each DE As KeyValuePair(Of String, List(Of clsUserInfo)) In UserData
                If DE.Key <> MasterFilePath Then
                    ReportProgress("Merging file " + currentFile.ToString + "/" + totalFiles.ToString + " : " + DE.Key + " ........ ")
                    For Each uInfo As clsUserInfo In DE.Value
                        If uInfo.HasData And uInfo.IncludedInMerge Then
                            Dim SourceConnStr As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + DE.Key + ";User Id=admin;Password=;"
                            If CheckDBConnection(DBProviderType.dbptOLEDB, SourceConnStr) Then
                                Dim SourceDBConnection As DbConnection = GetDBConnection(DBProviderType.dbptOLEDB, SourceConnStr)
                                For i As Integer = 0 To 2
                                    Try
                                        'Get the data from the source database.
                                        Dim da As New OleDb.OleDbDataAdapter(SelectQueries(i), SourceDBConnection)
                                        da.SelectCommand.Parameters.AddWithValue("PID", uInfo.UserID)

                                        da.AcceptChangesDuringFill = False
                                        Dim dt As New DataTable
                                        Dim affected As Integer = da.Fill(dt)

                                        Debug.Print("Filled count: " + affected.ToString)
                                        PrintDataTable(dt)

                                        ' Change PID
                                        For Each row As DataRow In dt.Rows
                                            row("PID") = uInfo.NewUserID
                                        Next

                                        Debug.Print("Manually Updated: " + affected.ToString)
                                        PrintDataTable(dt)

                                        'Save the data to the destination database.
                                        da.InsertCommand = New OleDb.OleDbCommand(InsertQueries(i), DestDBConnection)
                                        da.InsertCommand.Parameters.Add("PID", OleDbType.Integer, 0, "PID")

                                        Select Case i
                                            Case 0
                                                da.InsertCommand.Parameters.Add("WRT", OleDbType.VarWChar, 0, "WRT")
                                                da.InsertCommand.Parameters.Add("N1", OleDbType.VarWChar, 0, "N1")
                                                da.InsertCommand.Parameters.Add("N2", OleDbType.VarWChar, 0, "N2")
                                                da.InsertCommand.Parameters.Add("J", OleDbType.Double, 0, "J")
                                                da.InsertCommand.Parameters.Add("Note", OleDbType.LongVarWChar, 0, "Note")
                                            Case 1
                                                da.InsertCommand.Parameters.Add("AID", OleDbType.VarWChar, 0, "AID")
                                                da.InsertCommand.Parameters.Add("WRT", OleDbType.VarWChar, 0, "WRT")
                                                da.InsertCommand.Parameters.Add("DATA", OleDbType.VarWChar, 0, "DATA")
                                                da.InsertCommand.Parameters.Add("Note", OleDbType.LongVarWChar, 0, "Note")
                                            Case 2
                                                da.InsertCommand.Parameters.Add("AID", OleDbType.VarWChar, 0, "AID")
                                                da.InsertCommand.Parameters.Add("WRT", OleDbType.VarWChar, 0, "WRT")
                                                da.InsertCommand.Parameters.Add("ALTVALUE", OleDbType.Double, 0, "ALTVALUE")
                                        End Select
                                        affected = da.Update(dt)
                                        Debug.Print("Updated count: " + affected.ToString)

                                    Catch ex As Exception
                                        Debug.Print(ex.Message)
                                        ReportProgress("Error: " + ex.Message + vbCrLf)
                                        If SourceDBConnection IsNot Nothing Then
                                            SourceDBConnection.Close()
                                        End If
                                    End Try
                                Next

                                Try
                                    If DestDBConnection.State <> ConnectionState.Open Then
                                        DestDBConnection.Open()
                                    End If

                                    Dim oCommand As New OleDbCommand("INSERT INTO People (PID, Email, PersonName, Combined, Participating, Weight, Organization, Keypad, Wave, Location, Eval, EvalCluster, RoleWritingType, RoleViewingType) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", DestDBConnection)
                                    oCommand.Parameters.AddWithValue("PID", uInfo.NewUserID)
                                    oCommand.Parameters.AddWithValue("Email", uInfo.UserEmail)
                                    oCommand.Parameters.AddWithValue("PersonName", uInfo.UserName)
                                    oCommand.Parameters.AddWithValue("Combined", False)
                                    oCommand.Parameters.AddWithValue("Participating", True)
                                    oCommand.Parameters.AddWithValue("Weight", DBNull.Value)
                                    oCommand.Parameters.AddWithValue("Organization", DBNull.Value)
                                    oCommand.Parameters.AddWithValue("Keypad", DBNull.Value)
                                    oCommand.Parameters.AddWithValue("Wave", DBNull.Value)
                                    oCommand.Parameters.AddWithValue("Location", DBNull.Value)
                                    oCommand.Parameters.AddWithValue("Eval", DBNull.Value)
                                    oCommand.Parameters.AddWithValue("EvalCluster", DBNull.Value)
                                    oCommand.Parameters.AddWithValue("RoleWritingType", DBNull.Value)
                                    oCommand.Parameters.AddWithValue("RoleViewingType", DBNull.Value)

                                    Dim n As Integer = oCommand.ExecuteNonQuery
                                Catch ex As Exception
                                    Debug.Print(ex.Message)
                                    ReportProgress("Error: " + ex.Message + vbCrLf)
                                End Try

                            End If
                        End If
                    Next
                    ReportProgress("Done!" + vbCrLf)
                    currentFile += 1
                End If
            Next

            Try
                If DestDBConnection.State <> ConnectionState.Open Then
                    DestDBConnection.Open()
                End If

                Dim oCommand As New OleDbCommand("UPDATE MProperties SET PValue=? WHERE PropertyName=?", DestDBConnection)
                oCommand.Parameters.AddWithValue("PValue", NextPersonIDForAHP)
                oCommand.Parameters.AddWithValue("PropertyName", "NextPersonID")

                Dim n As Integer = oCommand.ExecuteNonQuery
            Catch ex As Exception
                Debug.Print(ex.Message)
                ReportProgress("Error: " + ex.Message + vbCrLf)
            End Try

            ReportProgress("Successfully finished merging!" + vbCrLf)
        Finally
            DestDBConnection.Close()
        End Try
    End Sub

    Public Sub RemoveFile(ByVal FilePath As String)
        mFiles.Remove(FilePath)
        mUserData.Remove(FilePath)
    End Sub

    Public Sub ClearAll()
        mFiles.Clear()
        mUserData.Clear()
    End Sub

    Public Sub New()
        mFiles = New List(Of String)
        mUserData = New Dictionary(Of String, List(Of clsUserInfo))
    End Sub
End Class