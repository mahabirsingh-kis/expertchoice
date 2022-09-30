Imports System.IO
Imports ECCore
Imports ExpertChoice.Data
Imports Canvas.PipeParameters
Imports GenericDBAccess.ECGenericDatabaseAccess.GenericDB
Imports ExpertChoice.Database

Namespace ExpertChoice.Service

    Public Module UploadService

        Public Const FORCE_USE_FILENAME_AS_PROJECTNAME As Boolean = True   ' D1224
        Public SupportedProjectFiles4Upload() As String = {_FILE_EXT_AHP, _FILE_EXT_AHPX, _FILE_EXT_AHPS, _FILE_EXT_AHPZ, _FILE_EXT_RAR, _FILE_EXT_ZIP}   ' D2601

        ' D2751 ===
        Public Sub CheckOld_AHP(sFileConnectionString As String, tDBProvider As DBProviderType, AHP_DBVer As Single, AHP_ChangedInECD As Boolean, ByRef sErrorMsg As String) 'AS/6-28-16 added AHP_ChangedInECD
            Dim TDBV As clsTranslateDBversion = New clsTranslateDBversion(tDBProvider, sFileConnectionString)
            If Not TDBV.VersionTranslated(GetAHPDBVersion(sFileConnectionString, tDBProvider).ToString, True, AHP_ChangedInECD) Then
                sErrorMsg = "Not translated"    ' D3698
            End If
        End Sub
        ' D2751 ==

        ' D0357 + D2600 ===
        Public Function CreateModelFromFile(ByRef App As clsComparionCore, ByVal sFilename As String, tProjectStatus As ecProjectStatus, tCurrentWorkgroupID As Integer, fCheckEmailsBeforeUpload As Boolean, ByRef sError As String) As clsProject ' D1081 + D2601
            Dim tPrj As clsProject = Nothing    ' D2601
            Dim fUploaded As Boolean = False
            sError = ""
            ' D1224 ===
            Dim sExtractedArchive As String = ""
            Dim sOriginalName As String = sFilename
            Select Case Path.GetExtension(sFilename).ToLower
                Case _FILE_EXT_AHPZ, _FILE_EXT_ZIP, _FILE_EXT_RAR
                    If isSupportedArchive(sFilename) Then

                        Dim ExtList As New ArrayList
                        ExtList.Add(_FILE_EXT_AHP.ToLower)
                        ExtList.Add(_FILE_EXT_AHPX.ToLower)
                        ExtList.Add(_FILE_EXT_AHPS.ToLower) ' D0378
                        Dim sUnPackedFile As String = ""
                        sExtractedArchive = ExtractArchiveForFile(App, sFilename, sFilename, ExtList, "", sError, sUnPackedFile, False)
                        If sError = "" And sExtractedArchive <> "" Then
                            Dim sTemp As String = File_CreateTempName()
                            File_CreateFolder(sTemp)
                            sFilename = sTemp + "\" + Path.GetFileName(sUnPackedFile)
                            File.Move(sExtractedArchive, sFilename)
                            sExtractedArchive = sTemp
                        Else
                            sExtractedArchive = ""
                        End If
                        ExtList = Nothing
                    Else
                        sError = App.ResString("errWrongUploadFile")
                    End If
            End Select

            If sError = "" Then
                ' D1224 ==

                ' D0387 ===
                Dim fStorageType As ECModelStorageType
                Select Case Path.GetExtension(sFilename).ToLower
                    Case _FILE_EXT_AHP
                        fStorageType = ECModelStorageType.mstAHPDatabase
                    Case _FILE_EXT_AHPX
                        fStorageType = ECModelStorageType.mstCanvasDatabase
                    Case _FILE_EXT_AHPS
                        fStorageType = ECModelStorageType.mstAHPSStream
                    Case Else
                        sError = App.ResString("errWrongUploadFile")
                End Select
                ' D0387 ==

                ' D1081 ===
                Dim sConnString As String = clsConnectionDefinition.BuildJetConnectionDefinition(sFilename, DBProviderType.dbptODBC).ConnectionString   ' D0457

                Select Case fStorageType
                    Case ECModelStorageType.mstAHPSStream
                        Dim AHPStream As New IO.FileStream(sFilename, FileMode.Open, FileAccess.Read)
                        If Not MiscFuncs.IsAHPSStream(AHPStream) Then sError = App.ResString("errWrongUploadFile")
                        AHPStream.Close()

                    Case ECModelStorageType.mstAHPDatabase
                        ' D3839 ===
                        If _DB_CHECK_AHP_VERSION Then
                            Dim AHP_DBVer As Single = GetAHPDBVersion(sConnString, DBProviderType.dbptODBC)
                            If AHP_DBVer < AHP_DB_DEFAULT_VERSION Then
                                sError = String.Format(App.ResString("errOldAHPVersion"), AHP_DB_DEFAULT_VERSION)
                            ElseIf AHP_DBVer < AHP_DB_LATEST_VERSION Then
                                Dim AHP_LastUploadToECC As String = GetAHPLastUploadToECC(sConnString, DBProviderType.dbptODBC)
                                Dim AHP_ChangedInECD As Boolean = AHP_LastUploadToECC.Length > 0
                                CheckOld_AHP(sConnString, DBProviderType.dbptODBC, AHP_DBVer, AHP_ChangedInECD, sError)
                            End If
                        End If
                End Select

                If sError = "" Then
                    ' D1081 ==

                    ' Create database
                    tPrj = New clsProject(App.Options.ProjectLoadOnDemand, App.Options.ProjectForceAllowedAlts, App.ActiveUser.UserEmail, App.isRiskEnabled, , , App.Options.ProjectUseDataMapping)   ' D2255 + D4465
                    tPrj.isOnline = False   ' D0748
                    tPrj.isPublic = False   ' D0748
                    tPrj.ProjectStatus = tProjectStatus
                    tPrj.PasscodeLikelihood = App.ProjectUniquePasscode("", -1) ' D1709
                    tPrj.PasscodeImpact = App.ProjectUniquePasscode("", -1)     ' D1709
                    'If Not App.isUniquePasscode(tPrj.Passcode, -1) Then tPrj.Passcode = "tpl_" + GetRandomString(_DEF_PASSCODE_LENGTH - 2, True, False) ' D0443 -D1709
                    tPrj.WorkgroupID = tCurrentWorkgroupID
                    tPrj.OwnerID = App.ActiveUser.UserID
                    tPrj.ProjectName = GetNameByFilename(sOriginalName) ' D1224

                    ' D0387 ===
                    If Not App.DBProjectCreate(tPrj, String.Format("Upload project from file '{0}'", sFilename)) Then  ' D0479 + D1081 + D1193 + D2603

                        sError = App.ResString("msgWTCantCreateDB") ' D0360

                    Else

                        ' Upload decision

                        Select Case fStorageType
                            Case ECModelStorageType.mstAHPDatabase
                                ExpertChoice.Service.FixRAcontraintsTableBeforeUpload(sFilename)     ' D1081
                                fUploaded = App.DBProjectCreateFromAHPFile(tPrj, sFilename, sError) ' D1081

                            Case ECModelStorageType.mstCanvasDatabase
                                fUploaded = App.DBProjectCreateFromAHPXFile(tPrj, sFilename, sError)    ' D0479

                            Case ECModelStorageType.mstAHPSStream
                                Dim FS As IO.FileStream = Nothing
                                Try
                                    FS = New IO.FileStream(sFilename, FileMode.Open, FileAccess.Read)
                                    tPrj.ResetProject(True)
                                    fUploaded = tPrj.ProjectManager.StorageManager.Writer.SaveFullProjectStream(FS)
                                Catch ex As Exception
                                    sError = ex.Message + vbCrLf + "Stack trace: " + ex.StackTrace ' D3361
                                    fUploaded = False   ' D3361
                                Finally
                                    If Not FS Is Nothing Then FS.Close()
                                End Try
                        End Select
                    End If
                    ' D0387 ==

                    If Not fUploaded Then
                        App.DBProjectDelete(tPrj, True)
                    Else

                        If tProjectStatus = ecProjectStatus.psMasterProject OrElse tProjectStatus = ecProjectStatus.psTemplate Then ' D1081

                            ' D0358 ===
                            ' clean-up for all judgments and users
                            Dim UsersList As List(Of clsUser) = MiscFuncs.GetUsersList(tPrj.ConnectionString, ECModelStorageType.mstCanvasStreamDatabase, tPrj.ProviderType) ' D0387 + D0479
                            For Each tUser As clsUser In UsersList
                                tPrj.ProjectManager.DeleteUserJudgments(tUser)
                                tPrj.ProjectManager.DeleteUser(tUser)
                            Next
                            ' D0358 ==
                        Else

                            '''' (!) Crash under system workgroup
                            If Not App.ImportProjectUsers(tPrj, App.ActiveUser, True, fCheckEmailsBeforeUpload) Then sError = "Error on import users list" ' D1081

                        End If

                        tPrj.ResetProject()
                        If tPrj.Comment = "" And Not tPrj.PipeParameters.ProjectPurpose Is Nothing Then tPrj.Comment = tPrj.PipeParameters.ProjectPurpose
                        If Not FORCE_USE_FILENAME_AS_PROJECTNAME AndAlso tPrj.ProjectName = "" And Not tPrj.PipeParameters.ProjectName Is Nothing Then tPrj.ProjectName = tPrj.PipeParameters.ProjectName ' D1224
                        If tPrj.Comment <> "" And String.IsNullOrEmpty(tPrj.PipeParameters.ProjectPurpose) Then tPrj.PipeParameters.ProjectPurpose = tPrj.Comment
                        If tPrj.ProjectName <> "" And String.IsNullOrEmpty(tPrj.PipeParameters.ProjectName) Then tPrj.PipeParameters.ProjectName = tPrj.ProjectName
                        If Not Guid.Equals(tPrj.PipeParameters.ProjectGuid, Guid.Empty) Then tPrj.ProjectGUID = tPrj.PipeParameters.ProjectGuid ' D2603
                        tPrj.PipeParameters.Write(PipeStorageType.pstStreamsDatabase, tPrj.ConnectionString, tPrj.ProviderType, tPrj.ID)  ' D0369 + D0376 + D0387
                        App.DBProjectUpdate(tPrj, False)
                        fUploaded = fUploaded AndAlso sError = ""

                    End If

                    ' D1224 ===
                    If sExtractedArchive <> "" Then
                        File_DeleteFolder(sExtractedArchive)
                        File_Erase(sExtractedArchive)
                    End If
                    ' D1224 ==

                End If ' on check file
            End If
            If Not fUploaded AndAlso sError <> "" Then App.DBSaveLog(dbActionType.actShowRTE, dbObjectType.einfProject, tPrj.ID, String.Format("Unable to upload model '{0}' from .file '{1}'", tPrj.ProjectName, Path.GetFileName(sFilename)), sError) ' D3361

            Return tPrj
        End Function
        ' D0357 ==

        '' D1081 ===
        'Private Function GetXMLPipeParamsFilename() As String
        '    Dim XMLFile As String = WebConfigOption(IIf(App.isRiskEnabled, WebOptions._OPT_DEFAULTPIPEPARAMS_RISK, WebOptions._OPT_DEFAULTPIPEPARAMS), "", True)    ' D2575
        '    If XMLFile <> "" Then If Not My.Computer.FileSystem.FileExists(_FILE_DATA_SETTINGS + XMLFile) Then XMLFile = ""
        '    If XMLFile = "" Then XMLFile = _FILE_SETTINGS_DEFPIPEPARAMS
        '    Return XMLFile
        'End Function
        '' D1081 ==

        ' D1224 ===
        Function GetNameByFilename(ByVal sFilename As String) As String
            Return URLDecode(Path.GetFileNameWithoutExtension(sFilename)).Replace("+", " ").Replace("_", " ").Replace("  ", " ").Trim  ' D7218
        End Function
        ' D1224 + D2600 ==

        ' D2601 ===
        Public Function GetProjectFilesList(sPath As String, SupportedFiles() As String) As List(Of String)
            Dim _FilesList As New List(Of String)
            Try
                Dim AllFiles As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Computer.FileSystem.GetFiles(sPath)
                DebugInfo("Scan for projects…")
                For Each sFileName As String In AllFiles
                    If IO.Path.GetFileName(sFileName)(0) <> "~" Then
                        Dim sExt As String = IO.Path.GetExtension(sFileName).ToLower
                        For i As Integer = 0 To SupportedFiles.Length - 1
                            If SupportedFiles(i).ToLower = sExt Then
                                DebugInfo(String.Format("Found model: {0}", sFileName))
                                _FilesList.Add(IO.Path.GetFileName(sFileName))
                            End If
                        Next
                    End If
                Next
            Catch ex As Exception
            End Try
            Return _FilesList
        End Function
        ' D2601 ==


    End Module

End Namespace
