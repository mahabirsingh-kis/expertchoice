Imports System.IO
Imports Canvas
Imports ECCore
Imports ExpertChoice.Data
Imports ExpertChoice.Database
Imports ExpertChoice.Service
Imports GenericDBAccess.ECGenericDatabaseAccess
Imports ExpertChoice.Web
Imports OfficeOpenXml

Public Class download
    Inherits System.Web.UI.Page

    Const _MailMerge_MDB As String = "ComparionInvitations.mdb"
    Const _MailMerge_DOC_AT As String = "MailMergeAT.docx"
    Const _MailMerge_DOC_TT As String = "MailMergeTT.docx"
    Const _MailMerge_ReadMe As String = "README.docx"
    Const _MailMerge_Folder As String = "MailMerge"
    Const _MailMerge_TableName As String = "Office_Address_list"
    Private Const dInvitations As String = "Invitations"
    Private Const dStyles As String = "Styles"
    Private Const sStyleHyperLink As String = "styleHyperlink"
    Public Const DoCompactJet As Boolean = True
    Private Shared _Files2Archive As Chilkat.StringArray = Nothing
    Const _CSV_DELIM As String = ";"
    Const BuffSize As Integer = 4 * 1024 * 1024

#Region "Page Load"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim projectid = Request.QueryString("projectid")
        Dim ext = Request.QueryString("ext")
        DownloadDecision(Convert.ToInt32(projectid), ext)
    End Sub

    'File download function
    Public Sub DownloadDecision(ByVal ProjectID As Integer, ByVal ext As String)
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim sError As String = ""
        Dim sTempFolder As String = ""
        Dim sFilename As String = ""

        If CheckVar("mode", "").ToLower() = "multi" Then
            Dim sIDs As String() = CheckVar("list", "").Trim().ToLower().TrimEnd(Convert.ToChar(",")).Split(Convert.ToChar(","))
            Dim sFileList As Chilkat.StringArray = New Chilkat.StringArray()
            Dim sArcName As String = ""

            For Each sID As String In sIDs
                Dim ID As Integer = -1

                If Integer.TryParse(sID, ID) Then
                    sFilename = PrepareDownloadFile(ID, False, sTempFolder, sError)

                    If String.IsNullOrEmpty(sError) AndAlso Not String.IsNullOrEmpty(sFilename) AndAlso File.Exists(sFilename) Then
                        sFileList.Append(sFilename)
                        If sArcName.Length < 50 Then sArcName += (If(String.IsNullOrEmpty(sArcName), "", "_")) & Path.GetFileNameWithoutExtension(sFilename)
                    End If
                End If
            Next

            sFilename = ""

            If sFileList.Count > 0 Then
                If String.IsNullOrEmpty(sArcName) Then sArcName = "Comparion_Projects"
                sArcName += FileService._FILE_EXT_ZIP

                If String.IsNullOrEmpty(sFilename) Then
                    sFilename = Path.GetTempPath()
                Else
                    sFilename = sTempFolder & "\"
                End If

                If File.Exists(sFilename & sArcName) Then sArcName = DateTime.Now.ToString("yyMMddHHmmss") & "_" & sArcName
                sFilename += sArcName

                If ArchivesService.PackZipFiles(sFileList, sFilename, sError) Then
                    sError = ""
                Else
                    sFilename = ""
                End If
            End If
        Else
            Dim ID As Integer = ProjectID
            If ID = -1 AndAlso Not App.HasActiveProject() Then RawResponseEnd()
            sFilename = PrepareDownloadFile(ID, False, sTempFolder, sError)
        End If

        Dim FResult As Boolean = (String.IsNullOrEmpty(sError)) AndAlso Not String.IsNullOrEmpty(sFilename) AndAlso File.Exists(sFilename)

        If FResult Then
            Dim fileLen As Long = New FileInfo(sFilename).Length
            Response.AddHeader("Content-Length", Convert.ToString(fileLen))
            Common.DebugInfo(String.Format("Start transferring for {0} bytes", fileLen))

            If fileLen > 0 Then
                Dim fs As FileStream = New FileStream(sFilename, FileMode.Open, FileAccess.Read)
                Dim r As BinaryReader = New BinaryReader(fs)
                Dim total As Integer = 0

                While total < fileLen
                    Dim Buff As Byte() = r.ReadBytes(BuffSize)

                    If Buff.GetUpperBound(0) >= 0 Then
                        total += Buff.GetUpperBound(0) + 1
                        context.Response.BinaryWrite(Buff)
                    End If
                End While

                r.Close()
                fs.Close()
            End If

            Dim file = New FileInfo(sFilename)
            Response.ClearContent()
            Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", file.Name))
            Response.AddHeader("Content-Length", file.Length.ToString())
            Response.ContentType = "application/octet-stream"
            Response.TransmitFile(file.FullName)
            App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProject, App.ProjectID, "", String.Format("Filename: {0}; Size: {1}", Path.GetFileName(sFilename), fileLen))
            Response.[End]()
            FResult = True
        End If

        If Not FResult Then
            App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProject, App.ProjectID, "Error on download", sError)
            If sError.ToLower().StartsWith("chilkat") Then sError = "Error on zip file. Contact with system administrator"
        End If

        If context.Response.IsClientConnected Then
        End If
    End Sub

    'check parameter in query string
    Public Function CheckVar(ByVal sVarName As String, ByVal DefValue As String) As String
        Dim context As HttpContext = HttpContext.Current
        Dim Res As String = DefValue
        'Dim idd = context.Request.QueryString("id")
        If sVarName IsNot Nothing AndAlso context.Request.QueryString(sVarName) IsNot Nothing Then Res = Convert.ToString(context.Request.QueryString(sVarName))
        Return Res
    End Function

    'Prepare file for download
    Private Function PrepareDownloadFile(ByVal ProjectID As Integer, ByVal fIgnoreZip As Boolean, ByRef sTempFolder As String, ByRef sError As String) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim sFilename As String = ""
        sError = ""
        Dim Project As clsProject = Nothing

        If ProjectID = App.ProjectID Or (ProjectID = -1 AndAlso App.HasActiveProject()) Then
            Project = App.ActiveProject
        Else
            Project = clsProject.ProjectByID(ProjectID, App.ActiveProjectsList)
        End If

        If Project Is Nothing Then Return ""
        ProjectID = Project.ID
        If Not App.CanUserDoProjectAction(ecActionType.at_mlDownloadModel, App.ActiveUser.UserID, ProjectID, App.ActiveUserWorkgroup) Then Return ""
        sFilename = FileService.File_CreateTempName()
        'Dim fMasterDB As Boolean = App.get_CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) AndAlso CheckVar("db", "") = "master"
        Dim fMasterDB As Boolean = App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) AndAlso CheckVar("db", "") = "master"
        Dim fAsZIP As Boolean = CheckVar("zip", fMasterDB) And Not fIgnoreZip
        Dim fType As ECTypes.ECModelStorageType = ECTypes.ECModelStorageType.mstCanvasStreamDatabase
        Dim sExt As String = ""
        Dim isPipeParams As Boolean = False
        Dim isInvitations As Boolean = False
        Dim isMailMerge As Boolean = False
        Dim ss = "ahps"

        Select Case ss
            Case "ahp"
                fType = ECTypes.ECModelStorageType.mstAHPFile
                sExt = FileService._FILE_EXT_AHP
            Case "ahpx"
                fType = ECTypes.ECModelStorageType.mstCanvasDatabase
                sExt = FileService._FILE_EXT_AHPX
            Case "ahps"
                fType = ECTypes.ECModelStorageType.mstCanvasStreamDatabase
                sExt = FileService._FILE_EXT_AHPS
            Case "gridxml"
                fType = ECTypes.ECModelStorageType.mstXMLFile
                sExt = FileService._FILE_EXT_XML
                fAsZIP = False
            Case "pipexml"
                fType = ECTypes.ECModelStorageType.mstXMLFile
                sExt = FileService._FILE_EXT_XML
                fAsZIP = False
                isPipeParams = True
            Case "invitations", "tt_invitations"
                fType = ECTypes.ECModelStorageType.mstXMLFile
                sExt = ".xlsx"
                fAsZIP = False
                isInvitations = True
            Case "mailmerge"
                fType = ECTypes.ECModelStorageType.mstXMLFile
                sExt = ".temp"
                fAsZIP = True
                isMailMerge = True
        End Select

        If Not fMasterDB Then

            If Not Project.isValidDBVersion AndAlso Not Project.isDBVersionCanBeUpdated Then
                sError = String.Format("Error on download project '{2}': " & TeamTimeClass.ResString("msgWrongProjectDBVersion"), Project.DBVersion.GetVersionString(), ECTypes.GetCurrentDBVersion().GetVersionString(), StringFuncs.ShortString(Project.ProjectName, 65))
                Return ""
            End If

            If Project.CheckGUID() Then App.DBProjectUpdate(Project, False, "Init Project GUID")

            If fType = ECTypes.ECModelStorageType.mstAHPFile Or fType = ECTypes.ECModelStorageType.mstCanvasDatabase Then
                Project = Project.Clone()

                If Project.isLoadOnDemand Then
                    Project.isLoadOnDemand = False
                    Project.ResetProject()
                    Project.ProjectManager.ResourceAligner.Load(Project.ProjectManager.StorageManager.StorageType, Project.ProjectManager.StorageManager.ProjectLocation, Project.ProjectManager.StorageManager.ProviderType, Project.ID)
                End If

                If fType = ECTypes.ECModelStorageType.mstAHPFile Then InfodocService.ParseInfodocsMHT2RTF(Project)
            End If

            Dim sFileConnString = ""

            Select Case fType
                Case ECTypes.ECModelStorageType.mstAHPFile, ECTypes.ECModelStorageType.mstCanvasDatabase
                    File.Copy((If(fType = ECTypes.ECModelStorageType.mstAHPFile, Consts._FILE_PROJECTDB_AHP, Consts._FILE_PROJECTDB_AHPX)), sFilename, True)
                    Common.DebugInfo("Create mdb file and start save decision data", Common._TRACE_INFO)
                    Dim tFileProvType As GenericDB.DBProviderType = GenericDB.DBProviderType.dbptOLEDB
                    sFileConnString = clsConnectionDefinition.BuildJetConnectionDefinition(sFilename, tFileProvType).ConnectionString
                    Dim oldLocation As String = Project.ProjectManager.StorageManager.ProjectLocation
                    Dim oldProviderType As GenericDB.DBProviderType = Project.ProjectManager.StorageManager.ProviderType
                    Project.ProjectManager.StorageManager.StorageType = fType
                    Project.ProjectManager.StorageManager.ProjectLocation = sFileConnString
                    Project.ProjectManager.StorageManager.ProviderType = tFileProvType

                    If Not Project.ProjectManager.StorageManager.Writer.SaveProject() Then
                        sError = "Can't save project. Please contact with administrator."
                    End If

                    Select Case fType
                        Case ECTypes.ECModelStorageType.mstAHPFile, ECTypes.ECModelStorageType.mstAHPDatabase
                            Project.PipeParameters.TableName = "MProperties"
                            Project.PipeParameters.PropertyNameColumnName = "PropertyName"
                            Project.PipeParameters.PropertyValueColumnName = "PValue"
                        Case ECTypes.ECModelStorageType.mstCanvasDatabase
                            Project.PipeParameters.TableName = PipeParameters.PROPERTIES_DEFAULT_TABLE_NAME
                            Project.PipeParameters.PropertyNameColumnName = PipeParameters.PROPERTY_NAME_DEFAULT_DB_COLUMN_NAME
                            Project.PipeParameters.PropertyValueColumnName = PipeParameters.PROPERTY_VALUE_DEFAULT_DB_COLUMN_NAME
                    End Select

                    Project.PipeParameters.Write(Canvas.PipeParameters.PipeStorageType.pstDatabase, sFileConnString, tFileProvType, Project.ID)
                    Project.PipeParameters.PipeMessages.Save(Canvas.PipeParameters.PipeStorageType.pstDatabase, sFileConnString, tFileProvType, Project.ID)
                    Project.ProjectManager.Attributes.ReadAttributes(Attributes.AttributesStorageType.astStreamsDatabase, oldLocation, oldProviderType, Project.ID)
                    Project.ProjectManager.Attributes.WriteAttributes(Attributes.AttributesStorageType.astDatabase, sFileConnString, tFileProvType, -1)
                    Project.ProjectManager.Attributes.ReadAttributeValues(Attributes.AttributesStorageType.astStreamsDatabase, oldLocation, oldProviderType, Project.ID, -1)
                    Project.ProjectManager.Attributes.WriteAttributeValues(Attributes.AttributesStorageType.astDatabase, sFileConnString, tFileProvType, -1, -1)
                    Project.ProjectManager.AntiguaDashboard.LoadPanel(ECTypes.ECModelStorageType.mstCanvasStreamDatabase, oldLocation, oldProviderType, Project.ID)
                    Project.ProjectManager.AntiguaRecycleBin.LoadPanel(ECTypes.ECModelStorageType.mstCanvasStreamDatabase, oldLocation, oldProviderType, Project.ID)
                    Project.ProjectManager.AntiguaDashboard.SavePanel(fType, sFileConnString, tFileProvType, -1)
                    Project.ProjectManager.AntiguaRecycleBin.SavePanel(fType, sFileConnString, tFileProvType, -1)
                    ECCore.MiscFuncs.ECMiscFuncs.WriteSurveysToAHPFile(oldLocation, oldProviderType, Project.ID, sFileConnString, tFileProvType)
                    Project.ProjectManager.StorageManager.ProviderType = oldProviderType
                    Project.ProjectManager.StorageManager.ProjectLocation = oldLocation
                    Project.ProjectManager.StorageManager.StorageType = ECTypes.ECModelStorageType.mstCanvasStreamDatabase
                    Project.ResetProject()
                    JetDatabasesService.FixRAcontraintsTableAfterDownload(sFilename)

                    If fType = ECTypes.ECModelStorageType.mstAHPFile AndAlso CheckVar("ra", False) Then
                        ECCore.MiscFuncs.ECMiscFuncs.UpdateMPropertiesWithRAFlag(sFileConnString, tFileProvType)
                    End If

                Case ECTypes.ECModelStorageType.mstCanvasStreamDatabase
                    Common.DebugInfo("Start saving stream to the file...", Common._TRACE_INFO)
                    ECCore.MiscFuncs.ECMiscFuncs.DownloadProject_CanvasStreamDatabase(Project.ConnectionString, Project.ProviderType, Project.ID, sFilename, App.isRiskEnabled)
                Case ECTypes.ECModelStorageType.mstXMLFile

                    If isMailMerge Then
                        Common.DebugInfo("Prepare MailMerge ...", Common._TRACE_INFO)
                        Dim sTmpFolder As String = FileService.File_CreateTempName()
                        sTempFolder = sTmpFolder
                        FileService.File_CreateFolder(sTmpFolder)
                        Dim SrcDOC As String = (If(CheckVar("tt", False), _MailMerge_DOC_TT, _MailMerge_DOC_AT))
                        Dim ProjectName As String = FileService.GetProjectFileName(Project.ProjectName, SrcDOC, "MailMerge", ".docx")

                        Try
                            File.Copy(Consts._FILE_DATA + _MailMerge_Folder & "\" + _MailMerge_MDB, sTmpFolder & "\" & _MailMerge_MDB)
                            File.Copy(Consts._FILE_DATA + _MailMerge_Folder & "\" & SrcDOC, sTmpFolder & "\" & ProjectName)
                            File.Copy(Consts._FILE_DATA + _MailMerge_Folder & "\" + _MailMerge_ReadMe, sTmpFolder & "\" & _MailMerge_ReadMe)
                            _Files2Archive = New Chilkat.StringArray()
                            _Files2Archive.Append(sTmpFolder & "\" & _MailMerge_MDB)
                            _Files2Archive.Append(sTmpFolder & "\" & ProjectName)
                            _Files2Archive.Append(sTmpFolder & "\" & _MailMerge_ReadMe)
                        Catch ex As Exception
                            sError = "Can't copy required templates."
                        End Try

                        If String.IsNullOrEmpty(sError) Then
                            Common.DebugInfo("Save data to MDB file...", Common._TRACE_INFO)
                            Dim ProviderType As GenericDB.DBProviderType = GenericDB.DBProviderType.dbptODBC
                            sFileConnString = clsConnectionDefinition.BuildJetConnectionDefinition(sTmpFolder & "\" & _MailMerge_MDB, ProviderType).ConnectionString

                            If GenericDB.CheckDBConnection(ProviderType, sFileConnString) Then

                                Using dbConnection As System.Data.Common.DbConnection = GenericDB.GetDBConnection(ProviderType, sFileConnString)
                                    dbConnection.Open()
                                    Dim oCommand As System.Data.Common.DbCommand = GenericDB.GetDBCommand(ProviderType)
                                    oCommand.Connection = dbConnection
                                    Dim sExtraParams As String = ""
                                    Dim sTemplates As String = ""

                                    For Each sTempl As String In _MailMerge_ExtraColumns
                                        oCommand.CommandText = String.Format("ALTER TABLE {0} ADD COLUMN {1} TEXT", _MailMerge_TableName, sTempl.Replace("%%", ""))
                                        GenericDB.DBExecuteNonQuery(ProviderType, oCommand)
                                        sExtraParams += ", ?"
                                        sTemplates += sTempl & System.Environment.NewLine
                                    Next

                                    oCommand.CommandText = String.Format("INSERT INTO {0} VALUES (?{1})", _MailMerge_TableName, sExtraParams)
                                    Dim tUsers As List(Of Integer) = New List(Of Integer)()

                                    For Each sID As String In CheckVar("uid_list", "").Split(","c)
                                        Dim ID As Integer = 0
                                        If Integer.TryParse(sID, ID) Then tUsers.Add(ID)
                                    Next

                                    For Each tUser As clsApplicationUser In App.DBUsersByProjectID(ProjectID)

                                        If Not tUser.CannotBeDeleted AndAlso (tUsers.Count = 0 OrElse tUsers.Contains(tUser.UserID)) Then
                                            oCommand.Parameters.Clear()
                                            oCommand.Parameters.Add(New System.Data.Odbc.OdbcParameter("ID", tUser.UserID))
                                            Dim sData As String = TeamTimeClass.ParseAllTemplates(sTemplates, tUser, Project)
                                            Dim idx As Integer = 0
                                            Dim s = System.Environment.NewLine.ToString()

                                            For Each sLine As String In sData.Split(New String() {System.Environment.NewLine}, System.StringSplitOptions.RemoveEmptyEntries)
                                                oCommand.Parameters.Add(New System.Data.Odbc.OdbcParameter("Param" & idx.ToString(), sLine.Trim()))
                                                idx += 1
                                            Next

                                            Dim affected As Integer = GenericDB.DBExecuteNonQuery(ProviderType, oCommand)
                                        End If
                                    Next

                                    oCommand = Nothing
                                    dbConnection.Close()
                                End Using

                                Common.DebugInfo("Mail merge DB is done.", Common._TRACE_INFO)
                            Else
                                sError = "Can't open template DB for saving data"
                            End If
                        End If
                    Else

                        If isInvitations Then
                            Common.DebugInfo("Save invitations...", Common._TRACE_INFO)
                            WriteInvitationXLSX(sFilename, Project.ID, CheckVar("type", "").ToLower() = "tt_invitations")
                            Dim tUsers As List(Of clsApplicationUser) = App.DBUsersByProjectID(Project.ID)

                            If tUsers IsNot Nothing Then

                                For Each tUser As clsApplicationUser In tUsers

                                    If Not tUser.CannotBeDeleted Then
                                        Dim sName As String = tUser.UserName
                                        If sName.Contains(_CSV_DELIM) OrElse sName.Contains("""") Then sName = String.Format("""{0}""", sName.Replace("""", """"""))
                                    End If
                                Next
                            End If
                        Else

                            If isPipeParams Then
                                Common.DebugInfo("Save pipe params...", Common._TRACE_INFO)
                                Project.PipeParameters.Write(Canvas.PipeParameters.PipeStorageType.pstXMLFile, sFilename, App.DefaultProvider, Project.ID)
                            Else
                                Common.DebugInfo("Start getting Grid XML...", Common._TRACE_INFO)
                                Dim sXML As String = Project.ProjectManager.mXML.GetDataGrid1XML(Project.ProjectManager, Project.ProjectManager.UserID)
                                Dim f As StreamWriter = New StreamWriter(sFilename, False, System.Text.Encoding.Unicode)
                                f.Write(sXML)
                                f.Close()
                                f = Nothing
                            End If
                        End If
                    End If
            End Select

            Common.DebugInfo("File is saved", Common._TRACE_INFO)
        Else
            File.Copy(Consts._FILE_SQL_EMPTY_MDB, sFilename, True)
            Dim sConnString As String = clsDatabaseAdvanced.GetConnectionString(App.CanvasMasterConnectionDefinition.DBName, GenericDB.DBProviderType.dbptODBC)

            If Not clsDatabaseAdvanced.CopyDatabaseToJet(sConnString, sFilename, sError) Then
                sError = String.Format("<span title='{0}'>Can't create project copy</span>", StringFuncs.SafeFormString(sError))
            End If
        End If

        If String.IsNullOrEmpty(sError) AndAlso DoCompactJet AndAlso (fType = ECTypes.ECModelStorageType.mstAHPFile Or fType = ECTypes.ECModelStorageType.mstCanvasDatabase) Then
            JetDatabasesService.CompactJetDatabase(sFilename, sError)
        End If

        Dim sFName As String = (If(fMasterDB, String.Format("{0}.mdb", App.Options.CanvasMasterDBName), ""))

        If Not fMasterDB Then
            Dim sExtraName As String = ""
            If isPipeParams Then sExtraName = " (settings)"
            If isInvitations Then sExtraName = " (links)"
            If isMailMerge Then sExtraName = " (MailMerge)"
            sFName = FileService.GetProjectFileName(sFName, Project.ProjectName & sExtraName, Project.Passcode, sExt)
        End If

        If String.IsNullOrEmpty(sTempFolder) Then
            sTempFolder = FileService.File_CreateTempName()
            FileService.File_Erase(sTempFolder)
            If Not FileService.File_CreateFolder(sTempFolder) Then sTempFolder = ""
        End If

        If Not String.IsNullOrEmpty(sTempFolder) Then

            Try
                If File.Exists(sTempFolder & "\" & sFName) Then sFName = Project.ID.ToString() & " _" & sFName
                sFName = sTempFolder & "\" & sFName
                File.Copy(sFilename, sFName)
                FileService.File_Erase(sFilename)
                sFilename = sFName
            Catch ex As Exception
                sFName = sFilename
            End Try
        End If

        If fAsZIP AndAlso String.IsNullOrEmpty(sError) Then
            Dim sZIPFile As String = Path.ChangeExtension(sFilename, (If(CheckVar("ext", "").ToLower() = "ahpz", FileService._FILE_EXT_AHPZ, FileService._FILE_EXT_ZIP)))
            Dim FilesList As Chilkat.StringArray = New Chilkat.StringArray()

            If _Files2Archive Is Nothing Then
                FilesList.Append(sFilename)
            Else
                FilesList = _Files2Archive
            End If

            If ArchivesService.PackZipFiles(FilesList, sZIPFile, sError) Then
                sError = ""
            Else
                fAsZIP = False
            End If

            If File.Exists(sZIPFile) Then
                FileService.File_Erase(sFilename)
                sFilename = sZIPFile
            End If
        End If

        If Not String.IsNullOrEmpty(sError) Then
            If Not String.IsNullOrEmpty(sFilename) Then FileService.File_Erase(sFilename)
            If Not String.IsNullOrEmpty(sTempFolder) Then FileService.File_DeleteFolder(sTempFolder)
            sFilename = ""
        End If

        Return sFilename
    End Function

    Public Sub RawResponseEnd()
        If Response.IsClientConnected Then
            Response.Flush()
            Response.Close()
            Response.[End]()
        End If
    End Sub

    Friend ReadOnly Property _MailMerge_ExtraColumns As String()
        Get
            Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
            Return _TEMPL_LIST_ALL(App.isRiskEnabled)
        End Get
    End Property

    'Creating XLSX fil for invitation
    Public Sub WriteInvitationXLSX(ByVal Filename As String, ByVal ProjectID As Integer, ByVal fIsTeamTime As Boolean)
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim Project As clsProject = Nothing

        If ProjectID = App.ProjectID Or (ProjectID = -1 AndAlso App.HasActiveProject()) Then
            Project = App.ActiveProject
        Else
            Project = clsProject.ProjectByID(ProjectID, App.ActiveProjectsList)
        End If

        If Project IsNot Nothing Then
            Dim Users As List(Of clsApplicationUser) = App.DBUsersByProjectID(ProjectID)

            Try

                Try

                    If File.Exists(Filename) Then
                        File.Delete(Filename)
                    End If

                Catch
                End Try

                Dim MasterXLSX = AppDomain.CurrentDomain.BaseDirectory & "App_Data/Invitations/Master.xlsx"
                File.Copy(MasterXLSX, Filename)
                Dim fi = New FileInfo(Filename)

                Using package As ExcelPackage = New ExcelPackage(fi)
                    Dim Invitations As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dInvitations).First()
                    Dim Styles As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dStyles).First()
                    Dim row As Integer = 1

                    If Users IsNot Nothing Then

                        For Each tUser As clsApplicationUser In Users

                            If Not tUser.CannotBeDeleted Then
                                row += 1
                                Dim sName As String = tUser.UserName
                                If sName.Contains(_CSV_DELIM) OrElse sName.Contains("""") Then sName = String.Format("""{0}""", sName.Replace("""", """"""))
                                Dim URL As String = TeamTimeClass.ParseAllTemplates((If(fIsTeamTime, _TEMPL_URL_EVALUATE_TT, _TEMPL_URL_EVALUATE)), tUser, Project)
                                Invitations.Cells(row, 1).Value = sName
                                Invitations.Cells(row, 2).Value = tUser.UserEmail
                                Invitations.Cells(row, 3).Value = URL
                                Invitations.Cells(row, 3).StyleName = sStyleHyperLink
                            End If
                        Next
                    End If

                    Styles.Hidden = eWorkSheetHidden.VeryHidden
                    package.Workbook.Worksheets(dInvitations).View.TabSelected = True
                    package.Save()
                End Using

            Catch ex As Exception
                Throw New Exception("Error creating Invitation XLSX")
            End Try
        End If
    End Sub
#End Region

End Class