Imports System.IO
Imports SpyronControls.Spyron.Core

Partial Class SurveyUpload
    Inherits clsComparionCorePage

    ''D0480 ===
    'Private _SurveysAll As ArrayList = Nothing

    'Private Function SurveysList(ByVal fOnlyTemplates As Boolean) As ArrayList
    '    Dim Surveys As New ArrayList
    '    For Each tSurvey As clsSurveyInfo In App.ActiveSurveysList  ' D0488
    '        Surveys.Add(tSurvey)
    '    Next
    '    Return Surveys
    'End Function
    '' D0480 ==

    'Public ASurvey As clsSurvey

    Public Sub New()
        MyBase.New(_PGID_SURVEY_UPLOAD)
    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If Not IsPostBack AndAlso Not IsCallback Then

            AlignHorizontalCenter = True
            AlignVerticalCenter = True
            btnUpload.Text = ResString("btnDoSurveyUpload")
            btnUpload.Attributes("style") = "padding:1px 2px;"  ' D3804
            FileUploadSurvey.Attributes.Add("onchange", "CheckUpload()")
            FileUploadSurvey.Attributes.Add("onkeydown", "CheckUpload()")

            ' -D6423 ===
            'Dim fMasterExists As Boolean = App.isSpyronMasterDBValid
            'Dim fProjecxtsExists As Boolean = App.isSpyronProjectsDBValid
            'If Not fMasterExists Or Not fProjecxtsExists Then
            '    lblError.Text = String.Format("<div class='error' style='font-weight:bold;'>{0}<div class='text' style='margin-top:2em'>{1}</div></div>", String.Format(ResString("msgErrorDBConnection"), IIf(fMasterExists, App.Options.SpyronProjectsDBName + " (Spyron ProjectsDB)", App.Options.SpyronMasterDBName + " (Spyron MasterDB)")), String.Format(ResString("msgAuthContact"), SystemEmail))
            '    lblError.Visible = True
            '    divUploadSurvey.Disabled = True
            'Else
            ' -D6423 ==
            lblError.Text = ""
                btnUpload.Enabled = False

                ' D1460 ===
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("prjid", "")) ' Anti-XSS
                Dim sSurveyType As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("type", "")) ' Anti-XSS
                If sProjectID <> "" Then Integer.TryParse(sProjectID, AProjectID)
                'If sSurveyType <> "" Then Integer.TryParse(sSurveyType, ASurveyType)
                Session.Remove(SESSION_MAIN_SURVEY_INFO_EDIT + AProjectID.ToString + "-" + CInt(ASurveyType).ToString)
                ' D3804 ===
                Dim HasSurvey As Boolean = False
                With App.ActiveProject.ProjectManager.StorageManager.Reader
                    If ASurveyType = SurveyType.stWelcomeSurvey OrElse ASurveyType = SurveyType.stImpactWelcomeSurvey Then HasSurvey = .IsWelcomeSurveyAvailable(App.ActiveProject.isImpact) Else HasSurvey = .IsThankYouSurveyAvailable(App.ActiveProject.isImpact)
                End With
                If HasSurvey Then
                    ' D3804 ==
                    lblError.Text = String.Format("<div style='text-align:left; padding:0px 2em' class='text'><b>{0}</b></div>", ResString("msgSurveyExistsOnUpload"))
                    lblError.Visible = True
                End If
            ' D1460 ==

            'End If ' -D6423
        End If
    End Sub

    Private ASurveyType As SurveyType
    Private AProjectID As Integer = -1

    Protected Sub btnUpload_Click(sender As Object, e As EventArgs) Handles btnUpload.Click
        'lblError.Text = "We are sorry, but that function not implemented yet"
        'lblError.CssClass = "error text"
        'lblError.Visible = True
        Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("prjid", "")) ' Anti-XSS
        ASurveyType = CType(CheckVar("type", CInt(ASurveyType)), SurveyType)    ' D4734
        If sProjectID <> "" Then Integer.TryParse(sProjectID, AProjectID)
        'If sSurveyType <> "" Then Integer.TryParse(sSurveyType, ASurveyType)

        Dim fUploaded As Boolean = False
        Dim sErrorMessage As String = ""
        If FileUploadSurvey.HasFile Then

            Dim fHasError As Boolean = False
            Dim sUploadedFileName As String = File_CreateTempName() ' D0132
            FileUploadSurvey.SaveAs(sUploadedFileName)

            ' D0298 ===
            Dim sOriginalFile As String = FileUploadSurvey.FileName
            If isSupportedArchive(FileUploadSurvey.FileName) Then
                Dim ExtList As New ArrayList
                ExtList.Add(_FILE_EXT_SUR.ToLower)
                ExtList.Add(_FILE_EXT_SURS.ToLower) ' D0381
                Dim sExtractedFile As String = ExtractArchiveForFile(App, FileUploadSurvey.FileName, sUploadedFileName, ExtList, ResString("lblSurveyDefaultName"), sErrorMessage, sOriginalFile)   ' D0505
                If sErrorMessage = "" And sExtractedFile <> "" Then
                    sUploadedFileName = sExtractedFile
                Else
                    fHasError = True
                End If
                ExtList = Nothing
            End If
            ' D0298 ==

            ' D0381 === 
            Dim fFileType As SurveyStorageType = SurveyStorageType.sstFileStream
            Select Case Path.GetExtension(sOriginalFile).ToLower
                'Case _FILE_EXT_SUR
                '    fFileType = SurveyStorageType.sstDatabase
                Case _FILE_EXT_SURS
                    fFileType = SurveyStorageType.sstFileStream
                Case Else
                    fHasError = True
                    sErrorMessage = "Unsupported file format"
            End Select

            Dim tSurveyInfo As New clsSurveyInfo
            tSurveyInfo.StorageType = fFileType
            For Each aUser In App.ActiveProject.ProjectManager.UsersList
                tSurveyInfo.ComparionUsersList.Add(aUser.UserEMail, New clsComparionUser() With {.ID = aUser.UserID, .UserName = aUser.UserName})
            Next

            If Not fHasError Then
                Select Case fFileType
                    'Case SurveyStorageType.sstDatabase
                    '    Dim fProvider As DBProviderType = DBProviderType.dbptODBC
                    '    Dim FileConnString As String = clsConnectionDefinition.BuildJetConnectionDefinition(sUploadedFileName, DBProviderType.dbptODBC).ConnectionString    ' D0315 + D0329 + D0330 + MF0332 + D0488
                    '    If clsDatabaseAdvanced.CheckDBConnection(FileConnString, fProvider) Then    ' D0298 + D0329 + D0488
                    '        tSurveyInfo.ConnectionString = FileConnString
                    '        tSurveyInfo.ProviderType = fProvider
                    '        tSurveyInfo.Title = Path.GetFileNameWithoutExtension(sUploadedFileName)
                    '    End If

                    Case SurveyStorageType.sstFileStream
                        tSurveyInfo.ConnectionString = sUploadedFileName
                End Select
            End If

            If Not fHasError Then    ' D0298 + D0329
                If Not tSurveyInfo.SurveyDataProvider("").Survey Is Nothing Then 'L0442
                    ' D0381 ==

                    Dim tExistedSurvey As clsSurveyInfo = App.SurveysManager.GetSurveyInfo(App.ActiveSurveysList, tSurveyInfo.AGuid)
                    If Not tExistedSurvey Is Nothing Then
                        tSurveyInfo.AGuid = Guid.NewGuid
                        App.ApplicationErrorInitAndSaveLog(ecErrorStatus.errMessage, CurrentPageID, String.Format(ResString("msgSurveyGUIDReplace"), tExistedSurvey.Title), tSurveyInfo.AGuid.ToString) ' D6006
                    End If

                    tSurveyInfo.OwnerID = App.ActiveUser.UserID
                    tSurveyInfo.WorkgroupID = App.ActiveWorkgroup.ID

                    tSurveyInfo.ProviderType = App.SurveysManager.ProviderType
                    tSurveyInfo.ConnectionString = App.CanvasProjectsConnectionDefinition.ConnectionString  '  App.SpyronProjectsConnectionDefinition.ConnectionString  ' D0488 + D6423
                    tSurveyInfo.StorageType = SurveyStorageType.sstECCDatabaseStream_v1

                    tSurveyInfo.SurveyType = ASurveyType
                    tSurveyInfo.ProjectID = AProjectID

                    If tSurveyInfo.ProjectID > -1 Then tSurveyInfo.SaveSurvey(True)

                    'fUploaded = App.SurveysManager.CreateSurvey(tSurveyInfo)
                    'If fUploaded Then fUploaded = tSurveyInfo.SaveSurvey(True) 'L0441

                    'If Not fUploaded Then
                    '    sErrorMessage = "Can't save survey to stream"
                    '    If tSurveyInfo.StorageType = SpyronControls.Spyron.Core.SurveyStorageType.sstDatabaseStream Then App.SurveysManager.DeleteSurvey(tSurveyInfo)
                    'End If
                    Session(SESSION_MAIN_SURVEY_INFO_EDIT + CInt(ASurveyType).ToString) = tSurveyInfo
                Else
                    sErrorMessage = ResString("msgErrorCreateSurveyDB") ' D0298
                End If
            End If
            App.ActiveSurveysList = Nothing
            File_Erase(sUploadedFileName)

            ' D1460 ===
            If Not fHasError AndAlso sErrorMessage = "" Then
                Response.Redirect(PageURL(_PGID_SERVICEPAGE, _PARAM_ACTION + "=uploadsurvey&" + _PARAM_ID + "=" + CInt(tSurveyInfo.SurveyType).ToString + GetTempThemeURI(True)), True)
            End If
            ' D1460 ==

        End If
        'If fUploaded Then Response.Redirect(PageURL(_PGID_SURVEY_LIST) + "?" + GetTempThemeURI(False), True) 'L0424
        If sErrorMessage <> "" Then
            lblError.Text = String.Format("<div class='text error'><b>{0}</b>: {1}</div>", ResString("lblSurveyUploadError"), sErrorMessage)    ' D0298
            lblError.Visible = True
        End If

    End Sub

End Class
