Imports System.IO
Imports SpyronControls.Spyron.Core  ' D0381

Partial Class DownloadSurvey
    Inherits clsComparionCorePage

    Const BuffSize As Integer = 4 * 1024 * 1024 ' D1460

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim fResult As Boolean = False
        Dim tID As Integer = CheckVar(_PARAM_ID, 0)
        'L0509 ===
        Dim sProjectID As String = CheckVar("prjid", "")
        Dim sSurveyType As String = CheckVar("st", "")
        Dim AProjectID As Integer = -1
        Dim ASurveyType As SurveyType = SurveyType.stWelcomeSurvey
        Dim ASurveyType_int As Integer = CInt(SurveyType.stWelcomeSurvey)
        If sProjectID <> "" Then Integer.TryParse(sProjectID, AProjectID)
        If sSurveyType <> "" AndAlso Integer.TryParse(sSurveyType, ASurveyType_int) Then ASurveyType = CType(ASurveyType_int, SurveyType)
        App.SurveysManager.ActiveUserEmail = ""
        Dim tSurvey As clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(AProjectID, ASurveyType, Nothing)   ' D0381
        'L0509 ==
        If Not tSurvey Is Nothing Then fResult = DownloadSurvey(tSurvey)
        If Not fResult Then
            ' D1459 ===
            If GetTempTheme.ToLower = _THEME_SL Then
                Response.Redirect(PageURL(_PGID_SERVICEPAGE, "?action=nosurvey&close=1&pause=5000" + GetTempThemeURI(True)), True)
            Else
                Response.Redirect(PageURL(_PGID_PROJECTSLIST, "?" + GetTempThemeURI(False)), True)  ' D2780
                'Response.Redirect(PageURL(_PGID_SURVEY_LIST, "?" + GetTempThemeURI(False)), True)
            End If
            ' D1459 ==
        End If
    End Sub

    Private Function DownloadSurvey(ByVal tSurvey As clsSurveyInfo) As Boolean
        If tSurvey Is Nothing Then Return False
        Dim FResult As Boolean = True

        Dim sFilename As String = File_CreateTempName()
        Dim fAsZIP As Boolean = CheckVar("zip", False)

        ' D0381 ===
        Dim tSurveyName As String = CStr(IIf(tSurvey.Title = "", IIf(tSurvey.SurveyType = SurveyType.stThankyouSurvey, "ThankYou", "Welcome"), tSurvey.Title))
        Dim sFName As String = GetProjectFileName(String.Format("{0}_{1}{2}", App.ActiveProject.ProjectName, tSurveyName, IIf(tSurveyName.ToLower.Contains("survey"), "", "_Survey")), tSurvey.AGuid.ToString, tSurveyName, CStr(IIf(fAsZIP, _FILE_EXT_ZIP, _FILE_EXT_SURS)))   ' D1460

        ' L0025 === Check SurveyDataProvider to Open Project from Database
        If tSurvey.SurveyDataProvider("") IsNot Nothing Then 'L0442

        End If
        ' L0025 ==

        Dim oldConnString As String = tSurvey.ConnectionString
        Dim oldStorageType As SurveyStorageType = tSurvey.StorageType

        tSurvey.ConnectionString = sFilename
        tSurvey.StorageType = SurveyStorageType.sstFileStream

        FResult = CBool(tSurvey.SaveSurvey(True)) 'L0441

        tSurvey.StorageType = oldStorageType
        tSurvey.ConnectionString = oldConnString

        If FResult Then
            ' D0381 ==
            If fAsZIP Then
                DebugInfo("Start zip file")
                Dim sZIPFile As String = Path.ChangeExtension(sFilename, _FILE_EXT_ZIP)
                Dim sModelName As String = Path.GetDirectoryName(sFilename) + "\" + sFName
                IO.File.Copy(sFilename, sModelName, True)
                Dim FilesList As New Chilkat.StringArray
                FilesList.Append(sModelName)
                Dim sError As String = ""
                If PackZipFiles(FilesList, sZIPFile, sError) Then
                    If IO.File.Exists(sZIPFile) Then
                        File_Erase(sModelName)
                        File_Erase(sFilename)
                        sFilename = sZIPFile
                        sFName = Path.ChangeExtension(sFName, _FILE_EXT_ZIP)
                    End If
                Else
                    fAsZIP = False
                    App.DBSaveLog(dbActionType.actCreateArchive, dbObjectType.einfSurvey, tSurvey.ID, "ZIP survey error", sError)   ' D0496
                End If
                File_Erase(sModelName)
            End If
        End If

        If FResult Then
            ' Generate downloadable content

            DownloadFile(sFilename, "application/zip", sFName, dbObjectType.einfSurvey, App.ProjectID)  ' D6593

            'RawResponseStart()

            '' D1460 ===
            'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}""", HttpUtility.UrlEncode(SafeFileName(sFName))))	' D3478 + D6591
            'Response.ContentType = CStr(IIf(fAsZIP, "application/zip", "application/octet-stream"))   ' D1460
            'Dim fileLen As Long = New IO.FileInfo(sFilename).Length ' D0425
            'Response.AddHeader("Content-Length", CStr(fileLen))

            'DebugInfo(String.Format("Start transferring for {0} bytes", fileLen))

            ''Response.BinaryWrite(My.Computer.FileSystem.ReadAllBytes(sFilenameTemp))
            'If fileLen > 0 Then
            '    Dim fs As New FileStream(sFilename, FileMode.Open, FileAccess.Read)
            '    Dim r As New BinaryReader(fs)
            '    Dim total As Integer = 0

            '    While total < fileLen
            '        Dim Buff As Byte() = r.ReadBytes(BuffSize)
            '        If Buff.GetUpperBound(0) >= 0 Then
            '            total += Buff.GetUpperBound(0) + 1
            '            Response.BinaryWrite(Buff)
            '        End If
            '    End While
            '    r.Close()
            '    fs.Close()
            'End If
            '' D1460 ==

            'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfSurvey, tSurvey.ID, String.Format("Download survey '{0}'", tSurvey.Title), String.Format("Filename: {0}; Size: {1}", sFName, fileLen)) ' D0496
            '' Flush and close stream instead IIS sending HTML-page
            'FResult = True
        End If

        File_Erase(sFilename)
        If Response.IsClientConnected Then
            If FResult Then RawResponseEnd()
        End If
        Return FResult
    End Function

    Public Sub New()
        MyBase.New(_PGID_SURVEY_DOWNLOAD)
    End Sub

End Class
