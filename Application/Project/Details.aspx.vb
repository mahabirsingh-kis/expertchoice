Imports System.IO
Imports System.Data.Common
Imports System.Data.Odbc
Imports Chilkat
Imports OfficeOpenXml

Partial Class ProjectDetailsPage
    Inherits clsComparionCorePage

#Const SAVE_INVITATIONS_AS_XML = True       ' D1720

    ' D0243 ===
    Private _LM As Nullable(Of Date) = Nothing
    Private Const _ACTION_DOWNLOAD As String = "download"
    Private Const sZIP As String = "&zip=yes"   ' D0377

    ' D1828 ===
    Const _MailMerge_MDB As String = "ComparionInvitations.mdb"
    Const _MailMerge_DOC_AT As String = "MailMergeAT.docx"  ' D1835
    Const _MailMerge_DOC_TT As String = "MailMergeTT.docx"  ' D1835
    Const _MailMerge_ReadMe As String = "README.docx"       ' D1835
    Const _MailMerge_Folder As String = "MailMerge"
    Const _MailMerge_TableName As String = "Office_Address_list"    ' D1830

    Dim _Files2Archive As StringArray = Nothing
    ' D1828 ==

    Const _CSV_DELIM As String = ";"    ' D1629

    Const BuffSize As Integer = 4 * 1024 * 1024 ' D1169

    Public Const DoCompactJet As Boolean = True ' D0412

    Public Sub New()
        MyBase.New(_PGID_PROJECT_DESCRIPTION)
    End Sub

    Public Class ProjectDetailsItem
        Public Property id As Integer
        Public Property name As String
        Public Property description As String
        Public Property passcode_likelihood As String
        Public Property passcode_impact As String
        Public Property db_version As String
        Public Property is_public As String
        Public Property is_online As String
        Public Property project_type As String
        Public Property date_created As String
        Public Property date_last_modified As String
        Public Property date_last_visited As String
        Public Property statistic As String
        Public Property begins_on As String
        Public Property ends_on As String
        Public Property timeframe As String     ' D6968
        Public Property assumptions As String   ' D6969
    End Class

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return If(PRJ IsNot Nothing, PRJ.ProjectManager, Nothing)
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean
        Get
            With App
                Return PRJ.ProjectStatus = ecProjectStatus.psArchived OrElse Not .CanUserModifyProject(.ActiveUser.UserID, .ProjectID, .ActiveUserWorkgroup, .ActiveWorkspace, .ActiveWorkgroup)
                'Return PRJ.ProjectStatus <> ecProjectStatus.psArchived AndAlso PRJ.ProjectStatus <> ecProjectStatus.psActive AndAlso Not .CanUserModifyProject(.ActiveUser.UserID, .ProjectID, .ActiveUserWorkgroup, .ActiveWorkspace, .ActiveWorkgroup)
            End With
        End Get
    End Property

    Public ReadOnly Property isGetFile() As Boolean
        Get
            Return CheckVar(_PARAM_ACTION, "").ToLower = _ACTION_DOWNLOAD
        End Get
    End Property

    ' D0599 ===
    Public ReadOnly Property DownloadLink() As String
        Get
            Return PageURL(_PGID_PROJECT_DOWNLOAD, _PARAM_ACTION + "=" + _ACTION_DOWNLOAD + "&" + _PARAM_ID + "=" + App.ProjectID.ToString) ' D0884
        End Get
    End Property
    ' D0599 ==

    ' D3860 ===
    Public ReadOnly Property SurveyLink() As String
        Get
            Return PageURL(_PGID_SURVEY_DOWNLOAD, "?action=download&prjid=" + App.ProjectID.ToString + GetTempThemeURI(True))
        End Get
    End Property
    ' D3860 ==

    ' D2468 ===
    Public ReadOnly Property _MailMerge_ExtraColumns() As String()
        Get
            Return _TEMPL_LIST_ALL(App.isRiskEnabled)
        End Get
    End Property
    ' D2468 ==

    ' D3994 ===
    Private Function AddLink(ByVal sValue As String, ByVal sText As String) As String
        'Return String.Format("<option value='{0}'>{1}</option>", SafeFormString(sValue), sText)
        Return String.Format(",{{""value"":""{0}"", ""text"":""{1}""}}", JS_SafeString(sValue), JS_SafeString(sText))
    End Function

    Public Function GetLinks() As String
        Dim sLinks As String = ""

        sLinks += AddLink("type=ahps", ResString("lblDownloadAsAHPS"))
        sLinks += AddLink("type=ahps&zip=yes", ResString("lblDownloadAsZipAHPS"))
        If Not App.isRiskEnabled Then sLinks += AddLink("type=ahp&zip=yes&ext=ahpz", ResString("lblDownloadAsAHPZ"))
        If Not App.isRiskEnabled Then sLinks += AddLink("type=ahp&ext=ahp", ResString("lblDownloadAsAHP")) ' D4498
        sLinks += AddLink("type=txt&ext=txt", ResString("lblDownloadAsTXT")) ' D6486
        If App.HasActiveProject Then
            If App.ActiveProject.ProjectManager.StorageManager.Reader.IsWelcomeSurveyAvailable(App.ActiveProject.isImpact) Then sLinks += AddLink("type=survey&st=" + IIf(App.ActiveProject.isImpact, 3, 1).ToString, ResString("lblDownloadSurveyWelcome"))
            If App.ActiveProject.ProjectManager.StorageManager.Reader.IsThankYouSurveyAvailable(App.ActiveProject.isImpact) Then sLinks += AddLink("type=survey&st=" + IIf(App.ActiveProject.isImpact, 4, 2).ToString, ResString("lblDownloadSurveyThankYou"))
        End If

        If ShowDraftPages() Then
            sLinks += AddLink("type=pipexml", ResString("lblDownloadPipeParams"))
            sLinks += AddLink("type=invitations", "Anytime Evaluation links for all participants (.xlsx)")
            sLinks += AddLink("type=tt_invitations", "TeamTime Evaluation links for all participants (.xlsx)")
            sLinks += AddLink("type=mailmerge", "MS Word MailMerge files (.zip)")
            'sLinks += AddLink("type=xml_structure", "Model structure as XML")
            'sLinks += AddLink("type=xml", "Whole model as XML")
        End If

        'sLinks = String.Format("<select id='cbFormat' style='width:350px' onchange='checkOptions();'>{0}</select>", sLinks)
        Return sLinks.Trim(CChar(","))
    End Function
    ' D3994 ==

#Region "Page events"
    Private Sub Page_Init(sender As Object, e As System.EventArgs) Handles Me.Init
        ' D5092 ===
        If Not isGetFile AndAlso Not App.HasActiveProject Then FetchAccess(_PGID_PROJECTSLIST)
        Select Case CheckVar("mode", "").ToLower
            Case "edit"
                CurrentPageID = _PGID_PROJECT_PROPERTIES
                'NavigationPageID = _PGID_PROJECT_DESCRIPTION
            Case Else
                CurrentPageID = If(isGetFile OrElse CheckVar("dl", False), _PGID_PROJECT_DOWNLOAD, _PGID_PROJECT_DESCRIPTION)
        End Select
        ' D5092 ==
        If isGetFile Then DownloadDecision()
        If PRJ Is Nothing AndAlso lblError.Text <> "" Then
            RawResponseStart()
            Response.Write(lblError.Text)
            RawResponseEnd()
        End If
        If isAJAX Then onAjax()
    End Sub
#End Region

    Public Function GetProjectStatistic() As String
        Dim retVal As String = ""
        ' D6490 ===
        retVal += String.Format(ParseString("%%Alternatives%% count: <a href='{1}' onclick='return navigatePage();' class='actions'>{0}</a><br/>"), PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Count, JS_SafeString(PageURL(_PGID_STRUCTURE_ALTERNATIVES)))
        If PM.IsRiskProject Then
            Dim HID As Integer = PM.ActiveHierarchy
            PM.ActiveHierarchy = ECHierarchyID.hidLikelihood
            retVal += String.Format(ParseString("%%Objectives%% count: <a href='{2}' onclick='navigatePage();' class='actions'>{0}</a>, covering %%objectives%%: {1}<br/>"), PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes.Count, PM.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes.Count, JS_SafeString(PageURL(_PGID_STRUCTURE_SOURCES)))
            PM.ActiveHierarchy = ECHierarchyID.hidImpact
            retVal += String.Format(ParseString("%%Objectives%% count: <a href='{2}' onclick='navigatePage();' class='actions'>{0}</a>, covering %%objectives%%: {1}<br/>"), PM.Hierarchy(ECHierarchyID.hidImpact).Nodes.Count, PM.Hierarchy(ECHierarchyID.hidImpact).TerminalNodes.Count, JS_SafeString(PageURL(_PGID_STRUCTURE_OBJECTIVES)))
            PM.ActiveHierarchy = HID
        Else
            retVal += String.Format(ParseString("%%Objectives%% count: <a href='{2}' onclick='navigatePage();' class='actions'>{0}</a>, terminal %%objectives%%: {1}<br/>"), PM.Hierarchy(PM.ActiveHierarchy).Nodes.Count, PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes.Count, JS_SafeString(PageURL(_PGID_STRUCTURE_HIERARCHY)))
        End If
        'If PRJ.ProjectStatus = ecProjectStatus.psActive OrElse PRJ.ProjectStatus = ecProjectStatus.psArchived Then retVal += String.Format(ParseString("Participants count: <a href='{2}' onclick='navigatePage();' class='actions'>{0}</a>, with judgments</a>: <a href='{3}' onclick='navigatePage();' class='actions'>{1}</a>"), App.DBWorkspacesByProjectID(App.ProjectID).Count, UsersWithDataCount(), JS_SafeString(PageURL(_PGID_PROJECT_USERS)), JS_SafeString(PageURL(_PGID_MEASURE_EVAL_PROGRESS)))   ' D9404
        If PRJ.ProjectStatus = ecProjectStatus.psActive OrElse PRJ.ProjectStatus = ecProjectStatus.psArchived Then retVal += String.Format(ParseString("Participants count: <a href='{1}' onclick='navigatePage();' class='actions'>{0}</a>"), App.DBWorkspacesByProjectID(App.ProjectID).Count, JS_SafeString(PageURL(_PGID_PROJECT_USERS)))   ' D9404
        ' D6490 ==
        If PM.IsRiskProject Then retVal += "<br>" + GetControlCountWithTypes(PM.Controls.Controls)    ' D7394
        Return retVal
    End Function

    Public Function GetControlCountWithTypes(controls As List(Of clsControl)) As String
        Dim returnValue As String = String.Empty

        Dim lblControlsForSources As String = ParseString("%%Controls%% for %%Objective(l)%%")
        Dim lblControlsForEvents As String = ParseString("%%Controls%% for %%Alternatives%%")
        Dim lblControlsForObjs As String = ParseString("%%Controls%% for %%Objectives(i)%%")

        If (controls IsNot Nothing AndAlso controls.Count > 0) Then
            Dim controlTypes As List(Of ControlType) = [Enum].GetValues(GetType(ControlType)).Cast(Of ControlType)().ToList()
            
            For Each item As ControlType In controlTypes
                Dim typeControls As List(Of clsControl) = controls.Where(Function(x) x.Type = item).ToList()

                If (typeControls IsNot Nothing AndAlso typeControls.Count > 0) Then
                    If(returnValue.Length > 0) Then
                        returnValue += ", "
                    End If

                    returnValue += $"{If(item = ControlType.ctCause, lblControlsForSources, If(item = ControlType.ctCauseToEvent, lblControlsForEvents, If(item = ControlType.ctConsequenceToEvent, lblControlsForObjs, item.ToString.Substring(2))))}: {typeControls.Count}"
                End If
            Next

            returnValue = $"Controls count: {PM.Controls.Controls.Count} ({returnValue})<br/>"
        End If

        Return returnValue
    End Function

    Public Function UsersWithDataCount() As Integer
        Return PM.StorageManager.Reader.DataExistsForUsersHashset().Count ' D6489   // PM.ActiveHierarchy
        'Dim UsersWithJudgCount As Integer = 0
        'For Each tUser As clsUser In PM.UsersList
        '    Dim HasData As Boolean = HasData = MiscFuncs.DataExistsInProject(ECModelStorageType.mstCanvasStreamDatabase, PRJ.ID, PRJ.ConnectionString, PRJ.ProviderType, tUser.UserEMail)
        '    If HasData Then UsersWithJudgCount += 1
        'Next
        'Return UsersWithJudgCount
    End Function

    Public Sub UpdateProjectInfo(Data As ProjectDetailsItem)
        PRJ.ProjectName = RemoveBadTags(Data.name)
        Dim sOldLikelihood As String = PRJ.PasscodeLikelihood
        PRJ.PasscodeLikelihood = App.ProjectUniquePasscode(RemoveBadTags(Data.passcode_likelihood), PRJ.ID)
        PRJ.PasscodeImpact = App.ProjectUniquePasscode(RemoveBadTags(Data.passcode_impact), PRJ.ID)
        If PRJ.PasscodeImpact = PRJ.PasscodeLikelihood OrElse PRJ.PasscodeImpact = sOldLikelihood Then PRJ.PasscodeImpact = App.ProjectUniquePasscode("", -1)

        Dim sExtra As String = ""
        Dim fOnline As Boolean = Str2Bool(Data.is_online)
        If PRJ.isOnline <> fOnline Then sExtra = "On-line: " + fOnline.ToString + ";"
        Dim fAccessCodeEnabled As Boolean = Str2Bool(Data.is_public)
        If PRJ.isPublic <> fAccessCodeEnabled Then sExtra = "Access code allowed: " + Bool2YesNo(fAccessCodeEnabled) + ";"

        PRJ.isOnline = fOnline
        If fOnline AndAlso Not App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsOnline, Nothing, False) Then PRJ.isOnline = False
        PRJ.isPublic = fAccessCodeEnabled
        Dim TimelineStart As Date?
        Dim TimelineEnd As Date?
        If Data.begins_on IsNot Nothing AndAlso Data.ends_on IsNot Nothing AndAlso Data.begins_on.Trim() <> "" AndAlso Data.ends_on.Trim() <> "" Then
            TimelineStart = Date.Parse(Data.begins_on)
            TimelineEnd = Date.Parse(Data.ends_on)
            If TimelineStart > TimelineEnd Then
                Dim dt As Date = TimelineStart.Value
                TimelineStart = TimelineEnd
                TimelineEnd = dt
            End If
        End If

        ' D6968 ===
        Dim fSaveParams As Boolean = False
        If App.isRiskEnabled Then
            If PM.Parameters.TimeFrame <> Data.timeframe Then
                'Dim DT As Date
                'If String.IsNullOrEmpty(Data.timeframe) Then
                '    PM.Parameters.TimeFrame = ""
                '    fSaveParams = True
                'Else
                '    If Date.TryParse(Data.timeframe, DT) Then
                '        PM.Parameters.TimeFrame = DT.ToString
                '        fSaveParams = True
                '    End If
                'End If
                PM.Parameters.TimeFrame = Data.timeframe.Trim   ' D7003
                fSaveParams = True  ' D7003
            End If
            If PM.Parameters.Assumptions <> Data.assumptions.Trim Then
                PM.Parameters.Assumptions = Data.assumptions.Trim
                fSaveParams = True
            End If
        End If
        ' D6968 ==

        Dim PP As clsPipeParamaters = PRJ.ProjectManager.PipeParameters
        Dim oldParamSet As ParameterSet = PP.CurrentParameterSet

        PP.CurrentParameterSet = PP.DefaultParameterSet
        PRJ.PipeParameters.StartDate = TimelineStart
        PRJ.PipeParameters.EndDate = TimelineEnd

        If PRJ.IsRisk Then
            PP.CurrentParameterSet = PP.ImpactParameterSet
            PRJ.PipeParameters.StartDate = TimelineStart
            PRJ.PipeParameters.EndDate = TimelineEnd

            Dim prjType As ProjectType = CType(CInt(Data.project_type), ProjectType)
            If PRJ.PipeParameters.ProjectType <> prjType OrElse PRJ.RiskionProjectType <> prjType Then
                PRJ.PipeParameters.ProjectType = prjType
                PRJ.RiskionProjectType = prjType
            End If
        End If

        PP.CurrentParameterSet = oldParamSet
        If Not IsReadOnly OrElse PRJ.ProjectStatus = ecProjectStatus.psArchived Then  ' D4740
            If fSaveParams Then PM.Parameters.Save()  ' D6968
            PRJ.SaveProjectOptions("Update project info")
            App.DBProjectUpdate(PRJ, False, Trim("Update Project info. " + sExtra + "(SL)"))
        End If
    End Sub

    Private Function GetPasscode() As String
        Return App.ProjectUniquePasscode("", -1)    ' D5040
        'Randomize()
        'Dim ID As Integer = CInt(Math.Round(Rnd() * 90000000) + 10000000)
        'Return ID.ToString("####-####")
    End Function

    Private Sub onAjax()
        Dim sAction As String = CheckVar(_PARAM_ACTION, "").Trim.ToLower
        Dim sResult As String = ""

        If Not String.IsNullOrEmpty(sAction) Then
            Select Case sAction
                Case "save"
                    Dim sData As String = htmlUnescape(HttpUtility.UrlDecode(EcSanitizer.GetSafeHtmlFragment(CheckVar("data", ""))))    ' Anti-XSS

                    Dim tData As ProjectDetailsItem = JsonConvert.DeserializeObject(Of ProjectDetailsItem)(sData)
                    'Dim jss As New JavaScriptSerializer()
                    'Dim item = jss.Deserialize(Of ProjectDetailsItem)(sData)
                    UpdateProjectInfo(tData)
                    sResult = String.Format("['{0}',{1},'{2}',{3}]", sAction, Bool2JS(PRJ.isOnline), JS_SafeString(PRJ.LastModify.ToString), CInt(PRJ.RiskionProjectType))
                Case "get_passcode"                    
                    Dim sFor As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("for", ""))    ' Anti-XSS
                    sResult = String.Format("['{0}','{1}','{2}','{3}']", sAction, GetPasscode(), sFor, JS_SafeString(PRJ.LastModify.ToString))
                Case "description_height"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("value", ""))    ' Anti-XSS
                    Dim tValue As Double
                    If Not IsReadOnly AndAlso String2Double(sValue, tValue) Then
                        PM.Parameters.Project_DescriptionHeight = tValue
                        PM.Parameters.Save()
                    End If
                    sResult = String.Format("['{0}']", sAction)
                Case "get_link", "get_likelihood_link", "get_impact_link"
                    Dim sIsProjectOnline As String = CStr(IIf(App.ActiveProject.isOnline AndAlso App.ActiveProject.isPublic, "1", "0"))
                    Dim sCanSetOnline As String = CStr(IIf(CanChangeProjectOnlineStatus(), "1", "0"))
                    sResult = String.Format("['{0}','{1}',{2},{3},'{4}']", sAction, String.Format("{0}={1}", ApplicationURL(False, False) + PageURL(_PGID_START, _PARAM_PASSCODE), If(sAction = "get_impact_link", PRJ.PasscodeImpact, PRJ.PasscodeLikelihood)), sIsProjectOnline, sCanSetOnline, JS_SafeString(PRJ.LastModify.ToString))
                Case "set_prj_online"
                    PRJ.isOnline = True
                    If Not App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsOnline, Nothing, False) Then PRJ.isOnline = False
                    If Not IsReadOnly Then App.DBProjectUpdate(PRJ, False, Trim("Set Project online."))
                    sResult = String.Format("['{0}',{1},'{2}']", sAction, Bool2JS(PRJ.isOnline), JS_SafeString(PRJ.LastModify.ToString))
            End Select
        End If

        If sAction <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(sResult)
            Response.End()
        End If
    End Sub

    Public Function CanChangeProjectOnlineStatus() As Boolean
        Return PRJ IsNot Nothing AndAlso PRJ.ProjectStatus = ecProjectStatus.psActive AndAlso Not PRJ.isMarkedAsDeleted AndAlso Not PRJ.isTeamTimeImpact AndAlso Not PRJ.isTeamTimeLikelihood AndAlso Not IsReadOnly
    End Function

    Private Function PrepareDownloadFile(ProjectID As Integer, fIgnoreZip As Boolean, ByRef sTempFolder As String, ByRef sError As String) As String ' D0856
        Dim sFilename As String = ""
        sError = ""

        Dim Project As clsProject = Nothing
        If ProjectID = App.ProjectID Or (ProjectID = -1 AndAlso App.HasActiveProject) Then Project = App.ActiveProject Else Project = clsProject.ProjectByID(ProjectID, App.ActiveProjectsList)
        If Project Is Nothing Then Return ""
        ProjectID = Project.ID  ' D1629
        If Not App.CanUserDoProjectAction(ecActionType.at_mlDownloadModel, App.ActiveUser.UserID, ProjectID, App.ActiveUserWorkgroup) Then Return "" ' D0857

        sFilename = File_CreateTempName()   ' D0132
        ' D0127 ===
        Dim fMasterDB As Boolean = App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) AndAlso CheckVar("db", "") = "master" ' D0289 + D0368 + D0423 + D0479
        Dim fAsZIP As Boolean = CheckVar("zip", fMasterDB) And Not fIgnoreZip   ' D0240 + D023 + D1169

        ' D0378 ===
        Dim fType As ECModelStorageType = ECModelStorageType.mstCanvasStreamDatabase    ' D0387
        Dim sExt As String = ""
        Dim XMLMode As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("type", "")).ToLower    ' D3878 + Anti-XSS

        Select Case CheckVar("type", "").ToLower
            Case "ahp"
                fType = ECModelStorageType.mstAHPFile   ' D0387
                sExt = _FILE_EXT_AHP
            Case "ahpx"
                fType = ECModelStorageType.mstCanvasDatabase    ' D0387
                sExt = _FILE_EXT_AHPX
            Case "ahps"
                fType = ECModelStorageType.mstCanvasStreamDatabase  ' D0387
                sExt = _FILE_EXT_AHPS
                ' D0423 ===
            Case "gridxml"
                fType = ECModelStorageType.mstXMLFile
                sExt = _FILE_EXT_XML
                fAsZIP = False
                ' D0423 ==
            ' D6486 ===
            Case "txt"
                fType = ECModelStorageType.mstTextFile
                sExt = _FILE_EXT_TXT
                fAsZIP = False
                ' D6486 ==
                ' D0832 ===
            Case "pipexml"
                fType = ECModelStorageType.mstXMLFile
                sExt = _FILE_EXT_XML
                fAsZIP = False
                ' D0832 ==
                ' D1629 ===
            Case "invitations", "tt_invitations"    ' D3351
                fType = ECModelStorageType.mstXMLFile
                sExt = ".xlsx"
                fAsZIP = False
                ' D1629 ==
                ' D1828 ===
            Case "mailmerge"
                fType = ECModelStorageType.mstXMLFile
                sExt = ".temp"
                fAsZIP = True
                ' D1828 ==
                ' D3878 ===
            Case "xml_structure", "xml"
                fType = ECModelStorageType.mstXMLFile
                sExt = _FILE_EXT_XML
                fAsZIP = False
                ' D3878 ==
        End Select
        ' D0378 ==

        If Not fMasterDB Then   ' D0368

            '' D2914 ===
            'If Not Project.isValidDBVersion AndAlso Not Project.isDBVersionCanBeUpdated Then    ' D3645
            '    sError = String.Format("Error on download project '{2}': " + ResString("msgWrongProjectDBVersion"), Project.DBVersion.GetVersionString(), GetCurrentDBVersion.GetVersionString, ShortString(Project.ProjectName, 65))
            '    Return ""
            'End If
            '' D2914 ==

            If Project.CheckGUID Then App.DBProjectUpdate(Project, False, "Init Project GUID") ' D0892

            If fType = ECModelStorageType.mstAHPFile Or fType = ECModelStorageType.mstCanvasDatabase Then   ' D0419
                Project = Project.Clone     ' D0151
                ' D0183 ===
                If Project.isLoadOnDemand Then
                    Project.ProjectManager.ResourceAligner.Load(Project.ProjectManager.StorageManager.StorageType, Project.ProjectManager.StorageManager.ProjectLocation, Project.ProjectManager.StorageManager.ProviderType, Project.ID)    ' D4857
                    Project.ProjectManager.StorageManager.Reader.LoadUserData(Nothing)
                End If
                ' D0183 ==
                If fType = ECModelStorageType.mstAHPFile Then ParseInfodocsMHT2RTF(Project, ReportCommentType.rctInfoDoc) ' D0151 + D0419 + D0459 + D3910
            End If

            Select Case fType

                Case ECModelStorageType.mstAHPFile, ECModelStorageType.mstCanvasDatabase     ' D0378 + D0387

                    MyComputer.FileSystem.CopyFile(CStr(IIf(fType = ECModelStorageType.mstAHPFile, _FILE_PROJECTDB_AHP, _FILE_PROJECTDB_AHPX)), sFilename, True) ' D0368 + D0378 + D0387

                    DebugInfo("Create mdb file and start save decision data", _TRACE_INFO) ' D0412

                    ' Create ahp file
                    '' AC Project.ProjectManager.StorageManager.GoalDefaultInfoDoc = Project.PipeParameters.ProjectPurpose ' D0133 + D0174

                    Dim tFileProvType As DBProviderType = DBProviderType.dbptOLEDB
                    Dim sFileConnString As String = clsConnectionDefinition.BuildJetConnectionDefinition(sFilename, tFileProvType).ConnectionString   ' D0368 + D0459 'C0830


                    Dim oldLocation As String = Project.ProjectManager.StorageManager.ProjectLocation 'C0019
                    Dim oldProviderType As DBProviderType = Project.ProjectManager.StorageManager.ProviderType  ' D0346

                    Project.ProjectManager.StorageManager.StorageType = fType   'C0028 + D0368 + D0378 + D0387

                    Project.ProjectManager.StorageManager.ProjectLocation = sFileConnString 'C0830

                    Project.ProjectManager.StorageManager.ProviderType = tFileProvType    ' D0346 + D0368

                    If Not Project.ProjectManager.StorageManager.AHPWriter.SaveProject() Then 'C20070822 + D0130 'C0028
                        sError = "Can't save project. Please contact with administrator."
                        'FetchAccess()
                    End If

                    'C0051===
                    Select Case fType
                        Case ECModelStorageType.mstAHPFile, ECModelStorageType.mstAHPDatabase
                            Project.PipeParameters.TableName = "MProperties"
                            Project.PipeParameters.PropertyNameColumnName = "PropertyName"
                            Project.PipeParameters.PropertyValueColumnName = "PValue"
                        Case ECModelStorageType.mstCanvasDatabase
                            Project.PipeParameters.TableName = PROPERTIES_DEFAULT_TABLE_NAME
                            Project.PipeParameters.PropertyNameColumnName = PROPERTY_NAME_DEFAULT_DB_COLUMN_NAME
                            Project.PipeParameters.PropertyValueColumnName = PROPERTY_VALUE_DEFAULT_DB_COLUMN_NAME
                    End Select

                    Project.PipeParameters.Write(PipeStorageType.pstDatabase, sFileConnString, tFileProvType, Project.ID) 'C0390
                    Project.PipeParameters.PipeMessages.Save(PipeStorageType.pstDatabase, sFileConnString, tFileProvType, Project.ID) 'C0052 + D0174 + D0329 + D0368 'C0420
                    'C0051==

                    Project.ProjectManager.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, oldLocation, oldProviderType, Project.ID)
                    Project.ProjectManager.Attributes.WriteAttributes(AttributesStorageType.astDatabase, sFileConnString, tFileProvType, -1)

                    Project.ProjectManager.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, oldLocation, oldProviderType, Project.ID, -1)
                    Project.ProjectManager.Attributes.WriteAttributeValues(AttributesStorageType.astDatabase, sFileConnString, tFileProvType, -1, -1)

                    'C0783===
                    Project.ProjectManager.AntiguaDashboard.LoadPanel(ECModelStorageType.mstCanvasStreamDatabase, oldLocation, oldProviderType, Project.ID)
                    Project.ProjectManager.AntiguaRecycleBin.LoadPanel(ECModelStorageType.mstCanvasStreamDatabase, oldLocation, oldProviderType, Project.ID)

                    Project.ProjectManager.AntiguaDashboard.SavePanel(fType, sFileConnString, tFileProvType, -1)
                    Project.ProjectManager.AntiguaRecycleBin.SavePanel(fType, sFileConnString, tFileProvType, -1)
                    'C0783==

                    MiscFuncs.WriteSurveysToAHPFile(oldLocation, oldProviderType, Project.ID, sFileConnString, tFileProvType)

                    Project.ProjectManager.StorageManager.ProviderType = oldProviderType    ' D0346
                    Project.ProjectManager.StorageManager.ProjectLocation = oldLocation 'C0019
                    'Project.ProjectManager.StorageManager.StorageType = ECModelStorageType.mstCanvasDatabase 'C0028 'C0783
                    Project.ProjectManager.StorageManager.StorageType = ECModelStorageType.mstCanvasStreamDatabase 'C0783
                    ' D0127 ==

                    Project.ResetProject()  ' D0151
                    FixRAcontraintsTableAfterDownload(sFilename) 'C0427

                    ' D0950 ===
                    If fType = ECModelStorageType.mstAHPFile AndAlso CheckVar("ra", False) Then
                        MiscFuncs.UpdateMPropertiesWithRAFlag(sFileConnString, tFileProvType)
                    End If
                    ' D0950 ==

                    ' D0378 ===
                Case ECModelStorageType.mstCanvasStreamDatabase ' D0387

                    DebugInfo("Start saving stream to the file...", _TRACE_INFO) ' D0412

                    'C0742===
                    ' D3890 ===
                    If MiscFuncs.DownloadProject_CanvasStreamDatabase(Project.ConnectionString, Project.ProviderType, Project.ID, sFilename, App.isRiskEnabled) Then   ' D2256 + D3890
                        If App.isSnapshotsAvailable AndAlso (CheckVar("snapshots", False) AndAlso _SNAPSHOT_USE_IN_AHPS) Then ' D3892 + D3945
                            Dim FS As New IO.FileStream(sFilename, FileMode.Append, System.IO.FileAccess.Write)
                            Dim tBW As New BinaryWriter(FS)
                            tBW.Write(CHUNK_CANVAS_STREAMS_SNAPSHOTS_STREAM)
                            Dim Pos As Long = tBW.BaseStream.Position
                            tBW.Write(CInt(0))
                            App.SnapshotsAll2Stream(Project.ID, CType(FS, Stream))
                            'Dim PosEnd As Integer = tBW.BaseStream.Position
                            tBW.Seek(CInt(Pos), SeekOrigin.Begin)
                            tBW.Write(CInt(FS.Length - Pos - 4))    ' D3892
                            tBW.Close()
                            FS.Close()
                        End If
                    End If
                    ' D3890 ==
                    'C0742==

                    ' D6486 ===
                Case ECModelStorageType.mstTextFile
                    DebugInfo("Start downloading as TEXT MODEL...", _TRACE_INFO)
                    MyComputer.FileSystem.WriteAllText(sFilename, clsTextModel.GetModelStructure(Project, Project.IsRisk, False), False)
                    ' D6486 ==

                    ' D0423 ===
                Case ECModelStorageType.mstXMLFile
                    Select Case XMLMode ' D3878
                        ' D1828 ===
                        Case "mailmerge"  ' D3878

                            DebugInfo("Prepare MailMerge ...", _TRACE_INFO)

                            Dim sTmpFolder As String = File_CreateTempName()
                            sTempFolder = sTmpFolder
                            File_CreateFolder(sTmpFolder)

                            Dim SrcDOC As String = CStr(IIf(CheckVar("tt", False), _MailMerge_DOC_TT, _MailMerge_DOC_AT)) ' D1835
                            Dim ProjectName As String = GetProjectFileName(Project.ProjectName, SrcDOC, "MailMerge", ".docx")   ' D1835
                            Try
                                MyComputer.FileSystem.CopyFile(_FILE_DATA + _MailMerge_Folder + "\" + _MailMerge_MDB, sTmpFolder + "\" + _MailMerge_MDB)
                                MyComputer.FileSystem.CopyFile(_FILE_DATA + _MailMerge_Folder + "\" + SrcDOC, sTmpFolder + "\" + ProjectName)   ' D1835
                                MyComputer.FileSystem.CopyFile(_FILE_DATA + _MailMerge_Folder + "\" + _MailMerge_ReadMe, sTmpFolder + "\" + _MailMerge_ReadMe) ' D1835
                                _Files2Archive = New StringArray
                                _Files2Archive.Append(sTmpFolder + "\" + _MailMerge_MDB)
                                _Files2Archive.Append(sTmpFolder + "\" + ProjectName)
                                _Files2Archive.Append(sTmpFolder + "\" + _MailMerge_ReadMe) ' D1835
                            Catch ex As Exception
                                sError = "Can't copy required templates."
                            End Try

                            If sError = "" Then
                                DebugInfo("Save data to MDB file...", _TRACE_INFO)

                                Dim ProviderType As DBProviderType = DBProviderType.dbptODBC
                                Dim sFileConnString As String = clsConnectionDefinition.BuildJetConnectionDefinition(sTmpFolder + "\" + _MailMerge_MDB, ProviderType).ConnectionString
                                If CheckDBConnection(ProviderType, sFileConnString) Then

                                    Using dbConnection As DbConnection = GetDBConnection(ProviderType, sFileConnString) ' D2232
                                        dbConnection.Open()

                                        Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                                        oCommand.Connection = dbConnection

                                        Dim sExtraParams As String = ""
                                        Dim sTemplates As String = ""
                                        For Each sTempl As String In _MailMerge_ExtraColumns
                                            oCommand.CommandText = String.Format("ALTER TABLE {0} ADD COLUMN {1} TEXT", _MailMerge_TableName, sTempl.Replace("%%", ""))
                                            DBExecuteNonQuery(ProviderType, oCommand)
                                            sExtraParams += ", ?"
                                            sTemplates += sTempl + vbCrLf
                                        Next

                                        oCommand.CommandText = String.Format("INSERT INTO {0} VALUES (?{1})", _MailMerge_TableName, sExtraParams)

                                        ' D1835 ===
                                        Dim tUsers As New List(Of Integer)
                                        For Each sID As String In EcSanitizer.GetSafeHtmlFragment(CheckVar("uid_list", "")).Split(CChar(","))    ' Anti-XSS
                                            Dim ID As Integer
                                            If Integer.TryParse(sID, ID) Then tUsers.Add(ID)
                                        Next
                                        ' D1835 ==

                                        For Each tUser As clsApplicationUser In App.DBUsersByProjectID(ProjectID)
                                            If Not tUser.CannotBeDeleted AndAlso (tUsers.Count = 0 OrElse tUsers.Contains(tUser.UserID)) Then   ' D1835
                                                oCommand.Parameters.Clear()
                                                oCommand.Parameters.Add(New OdbcParameter("ID", tUser.UserID))
                                                Dim sData As String = ParseAllTemplates(sTemplates, tUser, Project)
                                                Dim idx As Integer = 0
                                                For Each sLine As String In sData.Split(CChar(vbCrLf))
                                                    oCommand.Parameters.Add(New OdbcParameter("Param" + idx.ToString, sLine.Trim))
                                                    idx += 1
                                                Next
                                                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                                            End If
                                        Next

                                        oCommand = Nothing
                                        dbConnection.Close()
                                    End Using
                                    DebugInfo("Mail merge DB is done.", _TRACE_INFO)

                                Else
                                    sError = "Can't open template DB for saving data"
                                End If
                            End If
                            ' D1828 ==

                        Case "invitations", "tt_invitations" ' D3878
                            ' D1629 ===
                            DebugInfo("Save invitations...", _TRACE_INFO)
                            Dim sOldEval As String = App.Options.EvalSiteURL    ' D4873
                            If CheckVar("ignoreval", False) Then App.Options.EvalSiteURL = ""   ' D4873
                            WriteInvitationXLSX(sFilename, Project.ID, XMLMode = "tt_invitations")   ' D3351 + D3878
                            Dim tUsers As List(Of clsApplicationUser) = App.DBUsersByProjectID(Project.ID)
                            If tUsers IsNot Nothing Then
                                For Each tUser In tUsers
                                    If Not tUser.CannotBeDeleted Then
                                        Dim sName As String = tUser.UserName
                                        If sName.Contains(_CSV_DELIM) OrElse sName.Contains("""") Then sName = String.Format("""{0}""", sName.Replace("""", """""")) ' D1814
                                    End If
                                Next
                            End If
                            ' D1629 ==
                            App.Options.EvalSiteURL = sOldEval  ' D4873

                        Case "pipexml"
                            ' D0832 ===
                            DebugInfo("Save evaluation params...", _TRACE_INFO)
                            Project.PipeParameters.Write(PipeStorageType.pstXMLFile, sFilename, App.DefaultProvider, Project.ID)
                            ' D0832 ==

                        Case "gridxml"
                            DebugInfo("Start getting Grid XML...", _TRACE_INFO)
                            ' D0425 ===
                            Dim sXML As String = Project.ProjectManager.mXML.GetDataGrid1XML(Project.ProjectManager, Project.ProjectManager.UserID) 'C0453
                            Dim f As New StreamWriter(sFilename, False, Encoding.Unicode)
                            f.Write(sXML)
                            f.Close()
                            f = Nothing
                            ' D0425 ==
                            ' D0423 ==

                            ' D3878 ===
                        Case "xml", "xml_structure"
                            DebugInfo("Start getting Project XML...", _TRACE_INFO)
                            Dim sXML As String = Project.ProjectManager.mXML.GetProjectXML(Project.ProjectManager, XMLMode <> "xml")
                            Dim f As New StreamWriter(sFilename, False, Encoding.Unicode)
                            f.Write(sXML)
                            f.Close()
                            f = Nothing
                            ' D3878 ==

                    End Select
            End Select
            ' D0378 ==

            DebugInfo("File is saved", _TRACE_INFO) ' D0412

        Else

            ' Create MasterDB file
            ' D0108 ===
            MyComputer.FileSystem.CopyFile(_FILE_SQL_EMPTY_MDB, sFilename, True) ' D0792
            Dim sConnString As String = clsDatabaseAdvanced.GetConnectionString(App.CanvasMasterConnectionDefinition.DBName, DBProviderType.dbptODBC)   ' D0330 + D0412 + D0459
            If Not clsDatabaseAdvanced.CopyDatabaseToJet(sConnString, sFilename, sError) Then  ' D0479
                sError = String.Format("<span title='{0}'>Can't create project copy</span>", SafeFormString(sError)) ' D0792
                'FetchAccess() ' D0130
            End If
            ' D0108 ==

        End If

        ' D0412 ===
        If sError = "" AndAlso DoCompactJet AndAlso (fType = ECModelStorageType.mstAHPFile Or fType = ECModelStorageType.mstCanvasDatabase) Then
            CompactJetDatabase(sFilename)
        End If
        ' D0412 ==

        Dim sFName As String = CStr(IIf(fMasterDB, String.Format("{0}.mdb", App.Options.CanvasMasterDBName), ""))   ' D0108 + D0315 + D1193
        ' D0180 ===
        If Not fMasterDB Then
            ' D1629 ===
            Dim sExtraName As String = ""
            If XMLMode = "pipexml" Then sExtraName = " (settings)"
            If XMLMode.Contains("invitation") Then sExtraName = " (links)"
            If XMLMode = "mailmerge" Then sExtraName = " (MailMerge)" ' D1828
            sFName = GetProjectFileName(sFName, Project.ProjectName + sExtraName, Project.Passcode, sExt)   ' D0346 + D0378 + D1629
            ' D1629 ==
        End If
        ' D0180 ==

        ' D0856 ===
        If sTempFolder = "" Then
            sTempFolder = File_CreateTempName()
            File_Erase(sTempFolder)
            If Not File_CreateFolder(sTempFolder) Then sTempFolder = ""
        End If

        If sTempFolder <> "" Then
            Try
                If MyComputer.FileSystem.FileExists(sTempFolder + "\" + sFName) Then sFName = Project.ID.ToString + " _" + sFName
                sFName = sTempFolder + "\" + sFName
                MyComputer.FileSystem.CopyFile(sFilename, sFName)
                File_Erase(sFilename)
                sFilename = sFName
            Catch ex As Exception
                sFName = sFilename
            End Try
        End If
        ' D0856 ==

        ' D0240 ===
        If fAsZIP AndAlso sError = "" Then
            Dim sZIPFile As String = Path.ChangeExtension(sFilename, CStr(IIf(CheckVar("ext", "").ToLower = "ahpz", _FILE_EXT_AHPZ, _FILE_EXT_ZIP)))
            Dim FilesList As New StringArray
            ' D1828 ===
            If _Files2Archive Is Nothing Then
                FilesList.Append(sFilename)
            Else
                FilesList = _Files2Archive
            End If
            ' D1828 ==
            If PackZipFiles(FilesList, sZIPFile, sError) Then
                sError = ""
            Else
                fAsZIP = False
            End If

            If MyComputer.FileSystem.FileExists(sZIPFile) Then
                File_Erase(sFilename)
                sFilename = sZIPFile
            End If
        End If
        ' D0240 ==

        If sError <> "" Then
            If sFilename <> "" Then File_Erase(sFilename)
            If sTempFolder <> "" Then File_DeleteFolder(sTempFolder)
            sFilename = ""
        End If
        Return sFilename
    End Function

    Private Const dInvitations As String = "Invitations"
    Private Const dStyles As String = "Styles"
    Private Const sStyleHyperLink As String = "styleHyperlink"

    Public Sub WriteInvitationXLSX(Filename As String, ProjectID As Integer, fIsTeamTime As Boolean)    ' D3351
        Dim Project As clsProject = Nothing
        If ProjectID = App.ProjectID Or (ProjectID = -1 AndAlso App.HasActiveProject) Then
            Project = App.ActiveProject
        Else
            Project = clsProject.ProjectByID(ProjectID, App.ActiveProjectsList)
        End If

        If Project IsNot Nothing Then
            Dim Users As List(Of clsApplicationUser) = App.DBUsersByProjectID(ProjectID)
            Try
                'erase the file if it already exists
                Try
                    If MyComputer.FileSystem.FileExists(Filename) Then
                        MyComputer.FileSystem.DeleteFile(Filename)
                    End If
                Catch
                End Try
                Dim MasterXLSX = AppDomain.CurrentDomain.BaseDirectory & "App_Data\Invitations\Master.xlsx"
                File.Copy(MasterXLSX, Filename)
                Dim fi = New FileInfo(Filename)
                Using package As New ExcelPackage(fi)
                    Dim Invitations As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dInvitations).First
                    Dim Styles As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dStyles).First
                    Dim row As Integer = 1
                    If Users IsNot Nothing Then
                        For Each tUser As clsApplicationUser In Users
                            If Not tUser.CannotBeDeleted Then
                                row += 1
                                Dim sName As String = tUser.UserName
                                If sName.Contains(_CSV_DELIM) OrElse sName.Contains("""") Then sName = String.Format("""{0}""", sName.Replace("""", """"""))
                                Dim URL As String = ParseAllTemplates(CStr(IIf(fIsTeamTime, _TEMPL_URL_EVALUATE_TT, _TEMPL_URL_EVALUATE)), tUser, Project)   ' D3351
                                Invitations.Cells(row, 1).Value = sName
                                Invitations.Cells(row, 2).Value = tUser.UserEmail
                                Invitations.Cells(row, 3).Value = URL
                                Invitations.Cells(row, 3).StyleName = sStyleHyperLink
                            End If
                        Next
                    End If
                    '==============cleanup========================
                    Styles.Hidden = eWorkSheetHidden.VeryHidden
                    package.Workbook.Worksheets(dInvitations).View.TabSelected = True
                    package.Save()
                End Using
            Catch ex As Exception
                Throw New Exception("Error creating Invitation XLSX")
            End Try

        End If
    End Sub

    ' D0243 ===
    Private Function DownloadDecision() As Boolean
        StorePageID = False ' D6486
        ' D0856 ===
        Dim sError As String = ""
        Dim sTempFolder As String = ""
        Dim sFilename As String = ""

        If CheckVar("mode", "").ToLower = "multi" Then
            Dim sIDs() As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("list", "")).Trim.ToLower.TrimEnd(CChar(",")).Split(CChar(","))   ' Anti-XSS
            Dim sFileList As New StringArray
            Dim sArcName As String = ""
            For Each sID As String In sIDs
                Dim ID As Integer = -1
                If Integer.TryParse(sID, ID) Then
                    sFilename = PrepareDownloadFile(ID, False, sTempFolder, sError)
                    If sError = "" AndAlso sFilename <> "" AndAlso MyComputer.FileSystem.FileExists(sFilename) Then
                        sFileList.Append(sFilename)
                        If sArcName.Length < 50 Then sArcName += CStr(IIf(sArcName = "", "", "_")) + Path.GetFileNameWithoutExtension(sFilename)
                    End If
                End If
            Next
            sFilename = ""
            If sFileList.Count > 0 Then
                If sArcName = "" Then sArcName = "Comparion_Projects"
                sArcName += _FILE_EXT_ZIP
                If sFilename = "" Then sFilename = Path.GetTempPath Else sFilename = sTempFolder + "\"
                If MyComputer.FileSystem.FileExists(sFilename + sArcName) Then sArcName = Now.ToString("yyMMddHHmmss") + "_" + sArcName
                sFilename += sArcName
                If PackZipFiles(sFileList, sFilename, sError) Then sError = "" Else sFilename = ""
            End If

        Else

            Dim sID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("id", "-1")) ' Anti-XSS
            Dim ID As Integer = -1
            If Not Integer.TryParse(sID, ID) Then ID = -1
            If ID = -1 AndAlso Not App.HasActiveProject Then
                sError = "No model found"   ' D6593
            Else
                sFilename = PrepareDownloadFile(ID, False, sTempFolder, sError)
            End If

        End If

        Dim FResult As Boolean = (sError = "") AndAlso sFilename <> "" AndAlso MyComputer.FileSystem.FileExists(sFilename)
        ' D0856 ==

        If FResult Then

            Dim sContentType As String = "application/octet-stream"
            If CheckVar("type", "").ToLower = "invitations" Then sContentType = "application/vnd.ms-excel"
            If Path.GetExtension(sFilename).ToLower = _FILE_EXT_ZIP Then sContentType = "application/zip"
            Dim sDownloadName As String = Path.GetFileName(sFilename)    ' D1191 + D3221

            DownloadFile(sFilename, sContentType, sDownloadName, dbObjectType.einfProject, App.ProjectID,, False)

            '' Generate downloadable content
            'RawResponseStart()  ' D0041
            'Response.AppendCookie(New HttpCookie("dl_token", CheckVar("t", "")))   ' D4510


            'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}"";  filename*=UTF-8''{0}", HttpUtility.UrlEncode(sDownloadName, Encoding.UTF8)))  ' D0856 + D1191 + D6591
            'Response.Charset = "UTF-8"

            '' D1629 ===
            'Dim sContentType As String = "application/octet-stream"
            'If CheckVar("type", "").ToLower = "invitations" Then sContentType = "application/vnd.ms-excel"
            'If Path.GetExtension(sFilename).ToLower = _FILE_EXT_ZIP Then sContentType = "application/zip"
            'Response.ContentType = sContentType ' D0240 + D0470 + D0856
            '' D1629 ==

            'Dim fileLen As Long = MyComputer.FileSystem.GetFileInfo(sFilename).Length ' D0425

            'Response.AddHeader("Content-Length", CStr(fileLen))

            'DebugInfo(String.Format("Start transferring for {0} bytes", fileLen))
            'If fileLen > 0 Then
            '    ' D1169 ===
            '    Using fs As New FileStream(sFilename, FileMode.Open, System.IO.FileAccess.Read)
            '        Dim r As New BinaryReader(fs)
            '        Dim total As Integer = 0

            '        While total < fileLen
            '            Dim Buff As Byte() = r.ReadBytes(BuffSize)
            '            If Buff.GetUpperBound(0) >= 0 Then
            '                total += Buff.GetUpperBound(0) + 1
            '                Response.BinaryWrite(Buff)
            '            End If
            '        End While

            '        r.Close()
            '    End Using
            '    ' D1169 ==
            '    ' D0423 ==
            'End If

            'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProject, App.ProjectID, "", String.Format("Filename: {0}; Size: {1}", Path.GetFileName(sFilename), fileLen))    ' D0496 + D0856
            FResult = True
        End If

        'File_Erase(sFilename)
        If sTempFolder <> "" Then File_DeleteFolder(sTempFolder) ' D0857

        ' D1296 ===
        If Not FResult Then
            App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProject, App.ProjectID, "Error on download", sError)
            If sError.ToLower.StartsWith("chilkat") Then sError = "Error on zip file. Contact with system administrator" ' D1297
            lblError.Text = sError
        End If
        ' D1296 ==
        ' Flush and close stream instead IIS sending HTML-page
        If FResult Then RawResponseEnd() ' D0275 + D0363
        Return FResult
    End Function
    ' D0243 ===

End Class