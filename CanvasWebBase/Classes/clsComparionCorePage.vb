Imports System.Collections.Specialized  ' D0214
Imports System.Web.UI.WebControls
Imports ExpertChoice.Service
Imports ExpertChoice.Database
Imports ECCore
Imports Canvas
Imports ECSecurity.ECSecurity
Imports System.Web.UI.HtmlControls
Imports SpyronControls.Spyron.Core  ' D1653
Imports System.Text.RegularExpressions
Imports System.Linq
Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Namespace ExpertChoice.Web

#Const _Hook_Errors = True      ' D0117

    <ViewStateModeById()>
    Partial Public Class clsComparionCorePage  ' D0043 + D1239
        Inherits System.Web.UI.Page

        Public __o As Object    ' dirty fix for VS error "'__o' is not declared"

        Public MyComputer As New Microsoft.VisualBasic.Devices.Computer

        ' D0005 ===
        Private _sOptionalPageTitle As String   ' D0043
        Private _fAlignHCenter As Boolean = False ' D0112
        Private _fAlignVCenter As Boolean = False ' D0112
        ' D0005 ==
        'Private _BlankMasterPage As Boolean ' D0033
        Private _Application As clsComparionCore    ' D0466
        Private _isFirstRun As Boolean      ' D0068
        Private _CanEditActiveProject As Boolean       ' D0135
        Private _ActiveProjectLockInfo As clsProjectLockInfo    ' D0487

        Public CustomWorkgroupPermissions As clsWorkgroup = Nothing ' D7270

        Public Const _SESS_APP As String = "AppCore"    ' D0010
        Private Const _SESS_UID As String = "UID"       ' D2289
        Private Const _SESS_HELP_CHECKED As String = "HelpChecked"  ' D4023

        Private Const _OPT_RR_CUSTOM_WORDING As Boolean = False     ' D6921

        Public Const _OPT_DOWNLOAD_BUFF_SIZE As Integer = 8192      ' D6593

        Private _isAdvancedMode As Boolean = False     ' D5083
        Private _isJustEvaluator As Boolean = False     ' D7271

        ' D0141 ===
        Public Const _SESS_MT_ACTION As String = "MT_Action"
        Public Const _SESS_MT_NODEID As String = "MT_NodeID"
        Public Const _SESS_MT_TYPEID As String = "MT_TypeID"
        ' D0141 ==
        Public Const _SESS_TT_Pipe As String = "Sess_TT_Pipe"    ' D1270 + D1284
        Public Const _SESS_TT_Passcode As String = "Sess_TT_Passcode"   ' D6789
        Public Const _SESS_FORCE_PIPE As String = "ForcePipe"       ' D4404
        Public Const _SESS_OPEN_DLG As String = "OpenDlg"           ' D7290
        Public Const _SESS_RET_URL As String = "RetURL"     ' D4916

        Public Const _Anonymous_Template As String = "Anonym-{0}_{1}"   ' D1057
        Public Const _Anonymous_RandomPsw As Boolean = False            ' D1057 + D1071

        Private _IFDEF_UseAutologon As Boolean = False
        Private _IFDEF_RestoreLastPage As Boolean = True            ' D6112

        Private _IFDEF_CheckEmailsBeforeUpload As Boolean = False   ' D0129 + D6112

        Private _ExtraWarningMessage As String = ""     ' D0336

        Private _fShowNavigation As Boolean     ' D0352
        Private _StorePageID As Boolean = True   ' D0271
        Private _StorePageCustomURL As String = ""  ' D0628

        Private _MasterPage As String = ""  ' D0560
        Private Const sMasterExt As String = ".master"  ' D0560

        ' D2364 ===
        Public ClusterPhrase As String = ""
        Public ClusterPhraseIsCustom As Boolean = False
        Public ClusterTitle As String = ""              ' D4329
        Public ClusterTitleIsCustom As Boolean = False  ' D4329
        Public TaskTemplates As New Dictionary(Of String, String)
        Public TaskNodeGUID As String = ""
        'Public PreParsedTemplates As New Dictionary(Of String, String) ' D4659
        ' D2364 ==

        'Public isHTML5Page As Boolean = True        ' D3464 + D4220 -D6491 // due to issues with jQuery, jQueryUI

        'Public Const SLShellURIParam As String = "temptheme=sl"     ' D0729 -D0766

        Public Const EncryptAllCookies As Boolean = False           ' D0755 + D2893 // redundant
        Private Const CookieEncryptionPrefix As String = "@n(#"     ' D0755

        Private SESS_WRT_PATH As String = ""                        ' D2775
        Private SESS_QH_MD5 As String = "QH_Shown" ' D3727
        Private SESS_REPORT_SAMPLES As String = "ReportSamples" ' D6901

        'Public newRelic As ExpertChoice.Diagnostics.Newrelic = New ExpertChoice.Diagnostics.Newrelic()

        Private sOldWRTPath As String = Nothing ' D2776

        Friend _Treatments As List(Of clsControl) = Nothing ' D4187

        ''' <summary>
        ''' By default is false. You can override that option for force to show top line menu when ShowNavigation=False (left panel is hidden)
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowTopNavigation As Boolean = False     ' D4820
        Public Property NoMobileView As Boolean = False ' D4906

        Public Property CanUserModifyActiveProject As Boolean = False   ' D4916

        Private swMain As New Stopwatch()

        'A1205 ===
        Public ReadOnly Property ImagePath As String
            Get
                Return ThemePath + _FILE_IMAGES
            End Get
        End Property
        'A1205 ==

        'A1417 ===
        Public Function GetEmptyMessage() As String
            Return ResString("msgRiskResultsNoData")
        End Function
        'A1417 ==

        Public ReadOnly Property PRJ As clsProject
            Get
                Return App.ActiveProject
            End Get
        End Property

        Public ReadOnly Property PM As clsProjectManager
            Get
                Return If(App.HasActiveProject, App.ActiveProject.ProjectManager, Nothing)
            End Get
        End Property

        Public ReadOnly Property ProjectManager As clsProjectManager
            Get
                Return PM
            End Get
        End Property

        Public Sub New(ByVal pgID As Integer)  ' D0043
            swMain.Start()
            'Debug.WriteLine(vbCrLf) ' D0785
            isTraceEnabled = WebConfigOption(_OPT_TRACE_ENABLED, "0", True) = "1"   ' D0461
            If isTraceEnabled Then _Trace_PageContext = Context ' D0461
            'DebugInfo(String.Format("Create New Canvas Page with PageID {0}. isCallback:{1}. isPostBack:{2}", pgID.ToString, IsCallback, IsPostBack)) ' D0010 + D0043 + D0785
            DebugInfo(String.Format("Create New Canvas Page with PageID {0}", pgID.ToString)) ' D0010 + D0043 + D0785 + D3996 (can't use isCallBack since it's crashing)
            _CurrentPageID = pgID       ' D0466
            _Trace_PageContext = Context    ' D0010
            _fAlignHCenter = True       ' D0112
            _fAlignVCenter = True       ' D0112
            '_BlankMasterPage = False    ' D0033
            _Application = Nothing      ' D0466
            _sOptionalPageTitle = ""    ' D0043
            _isFirstRun = False         ' D0068
            _fShowNavigation = True     ' D0352
            _CanEditActiveProject = False ' D0135
            _ActiveProjectLockInfo = Nothing    ' D0487
            InitUserLocaleInfo() 'A0908
        End Sub

        ' D4996 ===
        Public ReadOnly Property _SESS_PRJ_ANON_NAME As String  ' D6328
            Get
                Return String.Format("AntiguaProject_{0}", App.Antigua_MeetingID)
            End Get
        End Property
        ' D4996 ==

        ' D1240 ==
        Protected Overrides Function LoadPageStateFromPersistenceMedium() As Object
            Dim res As Object = Nothing
            Try
                res = MyBase.LoadPageStateFromPersistenceMedium()
                'Catch err As Exception
            Catch err As HttpException
                If TypeOf err.InnerException Is System.Web.UI.ViewStateException Then
                    App.DBSaveLog(dbActionType.actShowRTE, dbObjectType.einfRTE, CurrentPageID, String.Format("RTE: {0}", err.Message), err.StackTrace)
                Else
                    Throw
                End If
            End Try
            Return res
        End Function
        ' D1240 ==

        ' D0576 ===
        Public Function GetWebCoreVersion() As System.Version
            ' D3816 ===
            Dim tVersion As New System.Version(0, 0, 0)
            Try
                Dim tOriginalVersion As System.Version = System.Reflection.Assembly.GetExecutingAssembly.GetName.Version
                tVersion = tOriginalVersion
                tVersion = New System.Version(tOriginalVersion.Major, tOriginalVersion.Minor, tOriginalVersion.Build, RevisionNumber(_FILE_ROOT))   ' D0473
            Catch ex As Exception
            End Try
            Return tVersion
            ' D3816 ==
        End Function
        ' D0576 ==

        ' D0335 ===
        Public Property DefaultMasterPage() As String
            Get
                If _MasterPage = "" Then
                    _MasterPage = GetCookie(_COOKIE_MASTER_PAGE, "")
                    ' D3071 ===
                    If _MasterPage <> "" Then
                        Dim sPath As String = Server.MapPath(_MasterPage)
                        If Not My.Computer.FileSystem.FileExists(sPath) Then
                            _MasterPage = ""
                            SetCookie(_COOKIE_MASTER_PAGE, "")
                        End If
                    End If
                    ' D3071 ==
                    If _MasterPage = "" Then
                        ' D0339 ===
                        Dim sPage As String = WebConfigOption(_OPT_MASTER_PAGE, "", False)
                        If sPage = "" Then
                            Select Case WebConfigOption(WebOptions._OPT_BACKDOOR, "").Trim.ToLower
                                Case _BACKDOOR_PLACESRATED
                                    sPage = "mpPlacesRated.master"
                                Case Else
                                    sPage = If(Page.MasterPageFile = "", _FILE_DEFAULT_MASTERPAGE, Page.MasterPageFile) ' D4668
                            End Select
                        Else
                            If Not sPage.ToLower.EndsWith(sMasterExt) Then sPage += sMasterExt  ' D4961
                        End If
                        If sPage.Contains("/") Then sPage = sPage.Substring(sPage.LastIndexOf("/") + 1) ' D4668
                        ' D4958 ===
                        Dim sPath As String = Server.MapPath(sPage)
                        If Not My.Computer.FileSystem.FileExists(sPath) Then
                            sPage = _FILE_DEFAULT_MASTERPAGE
                        End If
                        ' D4958 ==
                        _MasterPage = sPage
                        ' D0339 ==
                    End If
                    ' If Not My.Computer.FileSystem.FileExists(_FILE_ROOT + _MasterPage) Then _MasterPage = _FILE_DEFAULT_MASTERPAGE    ' -D0573 (for precompiled)
                End If
                Return _MasterPage
            End Get
            Set(ByVal value As String)
                If Not value.EndsWith(sMasterExt) Then value += sMasterExt
                If _MasterPage.ToLower <> value.ToLower Then
                    'If My.Computer.FileSystem.FileExists(_FILE_ROOT + value) Then  ' -D0573 (for precompiled)
                    _MasterPage = value
                    'SetCookie(_COOKIE_MASTER_PAGE, _MasterPage)    'TODO for allow to switch in future
                    'End If
                End If
            End Set
        End Property
        ' D0335 ==

        '-D0493
        '' D0033 ===
        'Public ReadOnly Property isBlankMasterPage() As Boolean
        '    Get
        '        Return _BlankMasterPage
        '    End Get
        'End Property
        '' D0033 ==

        ' D0336 ===
        Public Property ExtraPageMessage() As String
            Get
                Return _ExtraWarningMessage
            End Get
            Set(ByVal value As String)
                _ExtraWarningMessage = value
            End Set
        End Property
        ' D0336 ==

        ' D0271 ===
        Public Property StorePageID() As Boolean
            Get
                Return _StorePageID
            End Get
            Set(ByVal value As Boolean)
                _StorePageID = value
            End Set
        End Property
        ' D0271 ==

        ' D0268 ===
        Public Property StorePageCustomURL() As String
            Get
                Return _StorePageCustomURL
            End Get
            Set(ByVal value As String)
                _StorePageCustomURL = value
            End Set
        End Property
        ' D0268 ==

        ' D0068 ===
        Public Property isFirstRun() As Boolean
            Get
                Return _isFirstRun
            End Get
            Set(ByVal value As Boolean)
                _isFirstRun = value
            End Set
        End Property
        ' D0068 ==

        ' D3996 ===
        Public Shadows ReadOnly Property isCallback() As Boolean
            Get
                Return MyBase.IsCallback OrElse (Request IsNot Nothing AndAlso (CheckVar("ajax", False) OrElse GetParam(Request.Params, "ajax") <> ""))
            End Get
        End Property
        ' D3996 ==

        ' D6450 ===
        Public Function IsLocalUrl(ByVal url As String) As Boolean
            Dim tempURI As Uri = Nothing
            If Uri.TryCreate(url, UriKind.Relative, tempURI) Then Return True
            If Uri.TryCreate(url, UriKind.Absolute, tempURI) AndAlso tempURI IsNot Nothing AndAlso tempURI.Host.Equals(Request.Url.Host, StringComparison.OrdinalIgnoreCase) Then Return True
            Return False
        End Function
        ' D6450 ==

        ' D3996 ===
        Public ReadOnly Property isAJAX() As Boolean
            Get
                Return CheckVar("ajax", False)
            End Get
        End Property
        ' D3996 ==

        ' D0134 ===
        Public ReadOnly Property CanEditActiveProject() As Boolean
            Get
                Return _CanEditActiveProject
            End Get
        End Property
        ' D0134 ==

        ' D5083 ===
        Public Property isAdvancedMode As Boolean
            Get
                Return _isAdvancedMode
            End Get
            Set(value As Boolean)
                _isAdvancedMode = value
                SetCookie(_COOKIE_IS_ADVANCED, Bool2Num(value).ToString)
            End Set
        End Property
        ' D5083 ==

        ' D5079 ===
        Public Function isJustEvaluator() As Boolean
            Return _isJustEvaluator
        End Function

        Public Function CanViewActiveProject() As Boolean
            If App.isAuthorized AndAlso App.HasActiveProject Then
                Return CanUserModifyActiveProject OrElse App.CanUserDoProjectAction(ecActionType.at_mlViewModel, App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkgroup)
            End If
            Return False
        End Function
        ' D5079 ==

        ' D5074 ===
        Public ReadOnly Property isStarredAvailable As Boolean
            Get
                'Return _OPT_ALLOW_STARRED_PROJECTS AndAlso App.isAuthorized AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.Status <> ecWorkgroupStatus.wsSystem AndAlso Not isJustEvaluator() AndAlso Not App.Options.isSingleModeEvaluation
                Return _OPT_ALLOW_STARRED_PROJECTS AndAlso App.isAuthorized AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.Status <> ecWorkgroupStatus.wsSystem AndAlso Not App.Options.isSingleModeEvaluation   ' D7271
            End Get
        End Property
        ' D5074 ==

        '' D2949 ===
        'Public ReadOnly Property isMobileBrowser As Boolean
        '    Get
        '        Return _isMobileBrowser
        '    End Get
        'End Property
        '' D2949 ==

        ' D0622 ===
        Public Function isProjectDBValidOrUpdated(ByVal tProject As clsProject, ByVal tReturnPageID As Integer) As Boolean
            If tProject Is Nothing Then Return False
            'If App.Options.AutoUpdateOutdatedDecisions AndAlso Not tProject.DBUpdated AndAlso Not tProject.isValidDBVersion AndAlso tProject.ProjectStatus = ecProjectStatus.psActive AndAlso tProject.isDBVersionCanBeUpdated Then  ' D0641 + D1429
            If App.Options.AutoUpdateOutdatedDecisions AndAlso Not tProject.IgnoreDBVersion AndAlso Not tProject.isValidDBVersion AndAlso tProject.isDBVersionCanBeUpdated AndAlso tProject.ProjectStatus <> ecProjectStatus.psArchived Then  ' D0641 + D1429 + D4740
                If Not isCallback() AndAlso Not IsPostBack AndAlso Not IsCrossPagePostBack AndAlso CurrentPageID <> _PGID_PROJECT_UPDATE Then Response.Redirect(URLProjectID(PageURL(_PGID_PROJECT_UPDATE, "back=" + tReturnPageID.ToString + GetTempThemeURI(True)), tProject.ID), True) ' D0763 + D0766 + D1429
            End If
            Return tProject.isValidDBVersion
        End Function
        ' D0622 ==

        ' D0766 ===
        Public Function GetTempTheme() As String
            ' D4663 ===
            'Dim ThemesList As String() = {_THEME_DEFAULT, _THEME_EC09, _THEME_EC2018, _THEME_SL, _THEME_TT}
            Dim ThemesList As String() = {_THEME_EC2018, _THEME_SL, _THEME_TT}  ' D4961
            Dim sTheme As String = CheckVar(_PARAM_TEMP_THEME, "").Trim.ToLower
            If ThemesList.Contains(sTheme) Then Return sTheme Else Return ""
            ' D4663 ==
        End Function

        Public Function isSLTheme(Optional ByVal fAllowTTTheme As Boolean = False) As Boolean ' D1333
            If fAllowTTTheme And GetTempTheme() = _THEME_TT Then Return True ' D1333
            Return GetTempTheme() = _THEME_SL
        End Function

        Public Function GetTempThemeURI(ByVal fAddAmpersand As Boolean) As String
            Dim sTemp As String = RemoveXssFromParameter(GetTempTheme())    ' Anti-XSS
            If sTemp <> "" Then Return String.Format("{0}{1}={2}", IIf(fAddAmpersand, "&", ""), _PARAM_TEMP_THEME, sTemp) Else Return ""
        End Function
        ' D0766 ==

        ' D2947 + D2949 + D4750 + D6619 ===
        Private Function isMobileBrowserClient(Request As HttpRequest) As Boolean
            ' thanks to http://detectmobilebrowsers.com/
            Dim u As String = Request.ServerVariables("HTTP_USER_AGENT")
            Dim b As New Regex("(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase)
            Dim v As New Regex("1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase)
            Return Not String.IsNullOrEmpty(u) AndAlso (b.IsMatch(u) OrElse v.IsMatch(Left(u, 4)))
        End Function
        ' D2947 + D2949 + D4750 + D6619 ==

        ' D0659 ===
        Private Function AuthenticateByMeetingID(ByVal sEmail As String, ByVal sName As String, ByVal sMeetingID As String, ByRef sPasscode As String) As ecAuthenticateError   ' D0660

            Dim tMeetingID As Long = -1
            If Not clsMeetingID.TryParse(sMeetingID, tMeetingID) Then Return ecAuthenticateError.aeWrongMeetingID

            Dim tProject As clsProject = App.DBProjectByMeetingID(tMeetingID)
            If tProject Is Nothing Then Return ecAuthenticateError.aeWrongMeetingID

            'If tProject.ProjectStatus <> ecProjectStatus.psActive Or tProject.ProjectParticipating = ecProjectParticipating.ppOffline Then Return ecAuthenticateError.aeProjectLocked
            If tProject.ProjectStatus <> ecProjectStatus.psActive Or Not tProject.isOnline Then Return ecAuthenticateError.aeProjectLocked ' D0748
            'If Not tProject.isPublic Then Return ecAuthenticateError.aePasscodeNotAllowed ' D2018
            If tProject.isMarkedAsDeleted Then Return ecAuthenticateError.aeDeletedProject ' D0789 + D2436

            sPasscode = tProject.Passcode
            App.Options.isLoggedInWithMeetingID = True
            If Not tProject.PipeParameters.ForceDefaultParameters Then tProject.PipeParameters.CurrentParameterSet = tProject.PipeParameters.TeamTimeParameterSet   ' D0805 + D6434

            ' If tProject.isTeamTime AndAlso tProject.MeetingOwner IsNot Nothing Then   '-D0660
            If tProject.MeetingOwner IsNot Nothing Then ' D0660
                If tProject.MeetingOwner.UserEmail.ToLower = sEmail.ToLower Then Return ecAuthenticateError.aeUseRegularLogon
            End If

            If Not _LOGIN_WITH_MEETINGID_TO_INACTIVE_MEETING AndAlso Not tProject.isTeamTime Then Return ecAuthenticateError.aeNoSynchronousStarted

            Dim tUser As clsApplicationUser = Nothing   ' D0830

            ' D2038 ===
            If sName = "" Then sName = sEmail
            tUser = App.UserWithSignup(sEmail, sName, "", "User sign-up", Nothing, True)   ' D2215  ' sign-up with blank psw
            'If Not tProject.PipeParameters.TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin Then
            '    If sName = "" Then sName = sEmail
            '    tUser = App.UserWithSignup(sEmail, sName, "", "User sign-up")   ' sign-up with blank psw
            'Else
            '    tUser = App.DBUserByEmail(sEmail)   ' D0830
            '    If tUser Is Nothing Then Return ecAuthenticateError.aeSynchronousUserNotAllowed
            'End If
            ' D2038 ==

            If tUser Is Nothing Then Return ecAuthenticateError.aeNoUserFound

            Dim tWS As clsWorkspace = App.DBWorkspaceByUserIDProjectID(tUser.UserID, tProject.ID)

            ' D2113 ===
            If Not tProject.isTeamTimeLikelihood AndAlso Not tProject.isTeamTimeImpact Then    ' D2808
                If App.CanUserModifyProject(tUser.UserID, tProject.ID, App.DBUserWorkgroupByUserIDWorkgroupID(tUser.UserID, tProject.WorkgroupID), tWS) Then Return ecAuthenticateError.aeUseRegularLogon
                If Not tProject.PipeParameters.TeamTimeAllowMeetingID Then Return ecAuthenticateError.aeMeetingIDNotAllowed ' D2954
            End If
            ' D2113 ==

            ' D2038 ===
            Dim fIsImpact As Boolean = (sMeetingID = tProject.MeetingIDImpact.ToString)
            If tWS IsNot Nothing AndAlso tProject.PipeParameters.TeamTimeAllowMeetingID AndAlso (Not tWS.isInTeamTime(fIsImpact) OrElse tWS.TeamTimeStatus(fIsImpact) = ecWorkspaceStatus.wsDisabled) Then
                tWS.TeamTimeStatus(fIsImpact) = ecWorkspaceStatus.wsSynhronousActive
                tWS.Status(fIsImpact) = ecWorkspaceStatus.wsEnabled
                App.DBWorkspaceUpdate(tWS, False, "Include user to TeamTime session")
            End If
            ' D2038 ==

            ' -D2038
            'If tProject.PipeParameters.TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin Then
            '    If tWS Is Nothing Then Return ecAuthenticateError.aeSynchronousUserNotAllowed
            '    If tWS.Status = ecWorkspaceStatus.wsDisabled Then Return ecAuthenticateError.aeUserLocked
            '    If Not tWS.isInTeamTime AndAlso tWS.TeamTimeStatus <> ecWorkspaceStatus.wsSynhronousActive AndAlso tWS.TeamTimeStatus <> ecWorkspaceStatus.wsSynhronousReadOnly Then Return ecAuthenticateError.aeSynchronousUserNotAllowed ' D0660
            'End If

            If App.TeamTimeCheckMaxUsers(tProject, tUser, App.DBWorkspacesByUserID(tUser.UserID)) Then Return ecAuthenticateError.aeSynchronousFull

            tProject.PipeParameters.CurrentParameterSet = tProject.PipeParameters.DefaultParameterSet   ' D0443

            Return ecAuthenticateError.aeNoErrors
        End Function
        ' D0659 ==

        ' D4943 ===
        Public Function GetParamsWithoutAuthKeys(tParams As NameValueCollection) As String
            Return GetParamsButExclude(tParams, _PARAMS_LOGON)  ' D6766
            'Dim sParams As String = ""
            'If tParams IsNot Nothing Then
            '    For Each sName As String In tParams
            '        If Not String.IsNullOrEmpty(sName) Then
            '            Dim sName_ As String = sName.ToLower
            '            If Array.IndexOf(_PARAMS_LOGON, sName_) < 0 Then sParams += String.Format("{0}{1}={2}", If(sParams = "", "", "&"), sName, HttpUtility.UrlEncode(tParams(sName)))
            '            'If Array.IndexOf(_PARAMS_KEY, sName_) < 0 AndAlso Array.IndexOf(_PARAMS_TINYURL, sName_) < 0 AndAlso Array.IndexOf(_PARAMS_ANONYMOUS_SIGNUP, sName_) < 0 AndAlso
            '            '    Array.IndexOf(_PARAMS_SIGNUP, sName_) < 0 AndAlso Array.IndexOf(_PARAMS_SIGNUP_MODE, sName_) < 0 AndAlso Array.IndexOf(_PARAMS_EMAIL, sName_) < 0 AndAlso
            '            '    Array.IndexOf(_PARAMS_MEETING_ID, sName_) < 0 AndAlso Array.IndexOf(_PARAMS_PASSCODE, sName_) < 0 AndAlso Array.IndexOf(_PARAMS_PASSWORD, sName_) < 0 AndAlso
            '            '    Array.IndexOf(_PARAMS_REMEMBERME, sName_) < 0 AndAlso Array.IndexOf(_PARAMS_USERNAME, sName_) < 0 AndAlso sName_ <> "ttonly" AndAlso
            '            '    sName_ <> _PARAM_AS_PM AndAlso sName_ <> _PARAM_WKG_ROLEGROUP AndAlso sName_ <> _PARAM_ROLEGROUP AndAlso sName_ <> "pipe" AndAlso sName_ <> "phone" AndAlso
            '            '    sName_ <> "fld" AndAlso sName_ <> "req" Then   ' D6346
            '            '    'tLst.Add(sName, tParams(sName))
            '            '    sParams += String.Format("{0}{1}={2}", If(sParams = "", "", "&"), sName, HttpUtility.UrlEncode(tParams(sName)))
            '            'End If
            '        End If
            '    Next
            'End If
            'Return sParams
        End Function
        ' D4943 ==

        ' D6766 ===
        Public Function GetParamsButExclude(tParams As NameValueCollection, ExcludeList As String()) As String
            Dim sParams As String = ""
            If tParams IsNot Nothing AndAlso ExcludeList IsNot Nothing Then
                For Each sName As String In tParams
                    If Not String.IsNullOrEmpty(sName) Then
                        Dim sName_ As String = sName.ToLower
                        If Array.IndexOf(ExcludeList, sName_) < 0 Then sParams += String.Format("{0}{1}={2}", If(sParams = "", "", "&"), sName, HttpUtility.UrlEncode(tParams(sName)))
                    End If
                Next
            End If
            Return sParams
        End Function
        ' D6766 ==

        Public Function OpenSSO(ByRef sError As String) As Boolean
            'Dim retURL As String = New Uri(ApplicationURL(False, False) + PageURL(_PGID_SSO_ASSERT), UriKind.Absolute).AbsoluteUri  ' D6451 + D6550

            ' D7420 ===
            Dim sParams As String = GetParamsButExclude(Request.QueryString, {_PARAM_ACTION}).Trim().Trim(CChar("&")).Trim(CChar("?"))
            Dim retURL As String = ApplicationURL(False, False) ' D6451 + D6550 + D6552 + D7403
            If sParams <> "" Then retURL = String.Format("{0}{1}orig_params={2}", retURL, If(retURL.Contains("?"), "&", "?"), Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sParams)))
            ' D7420 ==
            SetCookie(_COOKIE_SSO_DEBUG, CheckVar("debug", "")) ' D7424

            Dim partnerIdP As String = System.Web.Configuration.WebConfigurationManager.AppSettings("PartnerIdP")
            If Not String.IsNullOrEmpty(partnerIdP) Then
                Try
                    ComponentSpace.SAML2.SAMLServiceProvider.InitiateSSO(Response, retURL, partnerIdP)  ' D6451
                    If Not String.IsNullOrEmpty(Response.RedirectLocation) Then
                        Response.Redirect(Response.RedirectLocation, True)
                        Return True
                    Else
                        sError = String.Format("SSO service authentication URL is not specified")
                    End If
                Catch ex As Exception
                    sError = String.Format("Error occured: {0}", ex.Message)
                End Try
            Else
                sError = "PartnerIdP address is not specified. Please check config settings."
            End If
            Return False
        End Function

        ' D0214 ===
        ''' <summary>
        ''' Perform web user authentication
        ''' </summary>
        ''' <param name="tParamsOrig"></param>
        ''' <param name="sErrorMessage"></param>
        ''' <param name="sRedirectURL"></param>
        ''' <returns></returns>
        ''' <remarks>IsLoggedInWithMeetingID property should be set properly before this call</remarks>
        Public Function Authenticate(ByVal tParamsOrig As NameValueCollection, ByRef sErrorMessage As String, ByRef sRedirectURL As String, ByVal tFormKind As ecAuthenticateWay, Optional ignoreSSO As Boolean = True) As ecAuthenticateError    ' D0511 + D1342 + D6532
            Dim AuthResult As ecAuthenticateError = ecAuthenticateError.aeNoUserFound   ' D0161
            Dim fHasError As Boolean = False

            Dim sExtraParams As String = ""  ' D1505

            ' D7439 ===
            If isSSO() Then
                Dim SSO_Params As String() = {_PARAM_EMAIL, _PARAM_PASSWORD, _PARAM_PASSCODE, _PARAM_MEETING_ID}
                Dim tmpParams As New NameValueCollection(tParamsOrig)
                For Each sParam As String In SSO_Params ' D7440
                    Dim SSO_Val As String = GetParam(tmpParams, _PARAM_SSO_PREFIX + sParam, False)
                    If Not String.IsNullOrEmpty(SSO_Val) Then
                        tParamsOrig(sParam) = SSO_Val
                        tParamsOrig.Remove(_PARAM_SSO_PREFIX + sParam)
                    End If
                Next
            End If
            ' D7439 ==

            ' D1342 ===
            Dim tParams As New NameValueCollection
            For Each sName As String In tParamsOrig
                tParams.Add(sName, RemoveXssFromParameter(tParamsOrig(sName)))  ' D6766
            Next
            ' D1342 ==

            Dim isTokenString As Boolean = False    ' D0304,RiskPlot
            Dim sKey As String = ParamByName(tParams, _PARAMS_KEY)
            ' D0896 ===
            Dim sTinyURL As String = ParamByName(tParams, _PARAMS_TINYURL)  ' D0896
            If sKey = "" AndAlso sTinyURL <> "" Then
                sKey = App.DecodeTinyURL(sTinyURL)
                If String.IsNullOrEmpty(sKey) Then
                    sKey = ""
                    If Not HasParamByName(tParams, _PARAMS_EMAIL) Then
                        AuthResult = ecAuthenticateError.aeWrongInstanceID ' D4841
                    End If
                End If
            End If
            ' D0896 ==
            If sKey <> "" Then
                sKey = DecodeURL(sKey, App.DatabaseID)  ' D0826
                If sKey <> "" Then
                    isTokenString = True
                    tFormKind = ecAuthenticateWay.awTokenizedURL    ' D0511
                    ' D0215 ===
                    'Dim tNewParams As NameValueCollection = HttpUtility.ParseQueryString(URLDecode(sKey))
                    Dim tNewParams As NameValueCollection = HttpUtility.ParseQueryString(sKey)  ' D1189

                    ' D2216 ===
                    If CurrentPageID <> _PGID_CREATE_PASSWORD AndAlso Not isSSO() Then   ' D6532
                        Dim tAction() As String = {_PARAM_ACTION}
                        If ParamByName(tNewParams, tAction) = "resetpsw" Then
                            If Request.Url IsNot Nothing AndAlso Not String.IsNullOrEmpty(Request.Url.Query) Then SessVar(_SESS_RET_URL) = Request.Url.Query    ' D4916
                            'TODO: check this part when called from webAPI
                            If CurrentPageID = _PGID_WEBAPI Then
                                sRedirectURL = PageURL(_PGID_CREATE_PASSWORD)
                                Return ecAuthenticateError.aeWrongPassword
                            Else
                                Response.Redirect(PageURL(_PGID_CREATE_PASSWORD), True) ' D4916
                            End If
                        End If
                    End If
                    ' D2216 ==

                    If tNewParams.Count > 0 Then
                        If tParams.Count = 1 AndAlso isTokenString Then tParams = tNewParams ' D3769 + D4285 + D7617
                        For Each sName As String In tParams
                            If Not String.IsNullOrEmpty(sName) Then ' D3783
                                Dim sName_ As String = sName.ToLower
                                If Array.IndexOf(_PARAMS_KEY, sName_) < 0 AndAlso Array.IndexOf(_PARAMS_TINYURL, sName_) < 0 AndAlso Array.IndexOf(tNewParams.AllKeys, sName) < 0 Then tNewParams.Add(sName, HttpUtility.UrlDecode(tParams(sName))) ' D0896 + D1189 + D1505 + D4018
                            End If
                        Next
                        tParams = tNewParams
                        'sExtraParams = GetParamsWithoutAuthKeys(tParams)    ' D4943
                        sExtraParams = GetParamsWithoutAuthKeys(If(Request Is Nothing, tParams, Request.QueryString))   ' D4943 + D7167
                    End If
                    ' D0215 ==
                Else
                    AuthResult = ecAuthenticateError.aeWrongCredentials
                End If
                ' D4569 ===
            Else
                If sTinyURL <> "" Then
                    AuthResult = ecAuthenticateError.aeWrongCredentials
                End If
                ' D4569 ==
            End If

            If AuthResult = ecAuthenticateError.aeWrongCredentials Then
                sErrorMessage = ResString("errInvalidHash")
                fHasError = True
            End If

            Dim sEmail As String = ParamByName(tParams, _PARAMS_EMAIL)
            Dim sPsw As String = ParamByName(tParams, _PARAMS_PASSWORD)
            Dim sPasscode As String = ParamByName(tParams, _PARAMS_PASSCODE)
            Dim sPhone As String = ParamByName(tParams, {"phone"})
            Dim sMeetingID As String = ParamByName(tParams, _PARAMS_MEETING_ID)   ' D0390
            Dim fRememberMe As Boolean = False
            If Not isSSO() AndAlso Not Str2Bool(ParamByName(tParams, _PARAMS_REMEMBERME), fRememberMe) Then   ' D6552
                Str2Bool(GetCookie(_COOKIE_REMEMBER, ""), fRememberMe)
            End If
            ' D0226 ===
            Dim fAllowQuickSignup As Boolean = False
            Str2Bool(ParamByName(tParams, _PARAMS_SIGNUP), fAllowQuickSignup)
            ' D0226 ==

            If isSSO() AndAlso Not ignoreSSO Then
                If sEmail.ToLower <> _DB_DEFAULT_ADMIN_LOGIN.ToLower AndAlso Not App.isAuthorized Then
                    If CurrentPageID <> _PGID_START AndAlso CurrentPageID <> _PGID_ERROR_403 AndAlso CurrentPageID <> _PGID_SSO_ASSERT Then
                        Dim tPg As clsPageAction = PageByID(CurrentPageID)
                        If tPg IsNot Nothing AndAlso tPg.Permission <> ecPagePermission.ppUnspecified AndAlso tPg.Permission <> ecPagePermission.ppEveryone Then
                            OpenSSO(sErrorMessage)
                            If Not String.IsNullOrEmpty(sErrorMessage) Then
                                fHasError = True
                                If CurrentPageID <> _PGID_ERROR_403 Then
                                    App.ApplicationError.Status = ecErrorStatus.errMessage
                                    App.ApplicationError.Message = sErrorMessage
                                    Response.Redirect(PageURL(_PGID_ERROR_403, GetTempThemeURI(False)), True)
                                End If
                                Return ecAuthenticateError.aeWrongCredentials
                            End If
                        End If
                    End If
                End If
            End If

            Dim isAnonymous As Boolean = False
            ' D1057 ===
            If fAllowQuickSignup AndAlso sPasscode <> "" AndAlso Not isSSO() Then   ' D6532
                Dim tProject As clsProject = App.DBProjectByPasscode(sPasscode)
                If tProject IsNot Nothing Then

                    If App.isAuthorized Then App.Logout() ' D4104

                    ' D2019 ===
                    Dim tErr As ecAuthenticateError = ecAuthenticateError.aeNoErrors
                    If Not tProject.isOnline Then tErr = ecAuthenticateError.aeProjectLocked
                    If Not tProject.isPublic Then tErr = ecAuthenticateError.aePasscodeNotAllowed
                    If tProject.isTeamTime Then
                        Dim CurPS As ParameterSet = tProject.PipeParameters.CurrentParameterSet
                        If Not tProject.PipeParameters.ForceDefaultParameters Then tProject.PipeParameters.CurrentParameterSet = tProject.PipeParameters.TeamTimeParameterSet
                        If Not tProject.PipeParameters.TeamTimeAllowMeetingID Then tErr = ecAuthenticateError.aeMeetingIDNotAllowed
                        tProject.PipeParameters.CurrentParameterSet = CurPS
                        If sMeetingID = "" Then sMeetingID = CStr(IIf(tProject.isImpact, tProject.MeetingIDImpact, tProject.MeetingIDLikelihood)) ' D2433
                    End If
                    If tErr <> ecAuthenticateError.aeNoErrors Then
                        sErrorMessage = ParseAllTemplates(App.GetMessageByAuthErrorCode(tErr, sPasscode), App.ActiveUser, tProject)  ' D3993
                        Return tErr
                    End If
                    ' D2019 ==

                    sEmail = GetCookie(_COOKIE_START_EMAIL, ParamByName(tParams, _PARAMS_EMAIL))
                    sPsw = GetCookie(_COOKIE_START_PWD, ParamByName(tParams, _PARAMS_PASSWORD))

                    ' D1059 ===
                    Dim isClear As Boolean = CheckVar("clear", False) OrElse CheckVar("c", False)   ' D4097
                    If isClear Then
                        If Str2Bool(ParamByName(tParams, _PARAMS_SIGNUP), False) Then
                            sEmail = ParamByName(tParams, _PARAMS_EMAIL)
                            sPsw = ParamByName(tParams, _PARAMS_PASSWORD)
                        Else
                            sEmail = ""
                            sPsw = ""
                        End If
                    End If

                    fRememberMe = False
                    'App.Options.SingleModeProjectPasscode = sPasscode  ' -D2293
                    ' D1059 ==

                    Str2Bool(ParamByName(tParams, _PARAMS_ANONYMOUS_SIGNUP), isAnonymous)
                    If isAnonymous Then
                        If sEmail = "" Then sEmail = String.Format(_Anonymous_Template, sPasscode.ToLower, GetRandomString(8, True, True).ToLower) ' D1059
                        Dim sName As String = ResString("defAnonymousName")
                        If tParams.AllKeys.Contains(_PARAMS_USERNAME(0)) Then tParams(_PARAMS_USERNAME(0)) = sName Else tParams.Add(_PARAMS_USERNAME(0), sName)
                        If sPsw = "" AndAlso (_Anonymous_RandomPsw OrElse Not WebOptions.AllowBlankPsw) Then sPsw = GetRandomString(8, True, False) ' D1059 + D4599
                        SetCookie(_COOKIE_START_EMAIL, sEmail,, True)    ' D7148
                        SetCookie(_COOKIE_START_PWD, sPsw,, True)        ' D7148
                    Else
                        If sEmail = "" Then
                            ' D4158 ===
                            If CurrentPageID <> _PGID_WEBAPI AndAlso isMobileBrowserClient(Request) AndAlso WebConfigOption(_OPT_MOBILE_EVAL_SITE, "0", True) = "1" AndAlso App.Options.EvalSiteURL <> "" AndAlso sTinyURL <> "" AndAlso GetParam(tParams, "redirect").ToLower <> "no" Then   ' D4184 + D5057
                                Response.Redirect(String.Format("{0}?hash={1}", App.Options.EvalSiteURL, sTinyURL), True)
                            Else
                                ' D4158 ==
                                'TODO: check this when called from webapi
                                If CurrentPageID = _PGID_WEBAPI Then
                                    sRedirectURL = PageURL(_PGID_START_WITH_SIGNUP, RemoveXssFromUrl(Request.Url.Query))    ' D6767
                                    Return ecAuthenticateError.aeNoUserFound
                                Else
                                    Response.Redirect(PageURL(_PGID_START_WITH_SIGNUP, RemoveXssFromUrl(Request.Url.Query)), True) ' D1059 + D6767
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            ' D1057 ==

            If App.isAuthorized AndAlso sPasscode <> "" AndAlso sEmail = "" AndAlso sPsw = "" AndAlso Not fAllowQuickSignup Then
                Dim tmpPrj As clsProject = App.DBProjectByPasscode(sPasscode)
                If tmpPrj IsNot Nothing AndAlso tmpPrj.ID <> App.ProjectID Then ' D1768
                    sEmail = App.ActiveUser.UserEmail
                    sPsw = App.ActiveUser.UserPassword
                    tParams.Add(_PARAM_EMAIL, sEmail)
                    tParams.Add(_PARAM_PASSWORD, sPsw)
                    App.Logout()
                End If
            End If

            ' D3319 ===
            ' This is for cases when user tries to login to another project when already logged as SingleEvalMode: logout/reset singlemode and use full projects list
            If App.isAuthorized AndAlso sPasscode <> "" AndAlso App.Options.isSingleModeEvaluation AndAlso sPasscode.Trim.ToLower <> App.Options.SingleModeProjectPasscode.Trim.ToLower Then
                App.Logout()
                App.Options.SingleModeProjectPasscode = ""
            End If
            ' D3319 ==

            'If sEmail = "" AndAlso sPsw = "" AndAlso sPasscode = "" Then Return ecAuthenticateError.aeUnknown
            If Not fHasError AndAlso sKey = "" AndAlso sEmail = "" AndAlso sPsw = "" AndAlso Not fAllowQuickSignup Then Return ecAuthenticateError.aeUnknown ' D1056 + D4841

            ' D0390 ===
            ' D0859 ===
            If Not fAllowQuickSignup AndAlso (tFormKind = ecAuthenticateWay.awCredentials Or tFormKind = ecAuthenticateWay.awTokenizedURL) Then    ' D1057
                App.Options.isLoggedInWithMeetingID = False
                App.Options.SingleModeProjectPasscode = ""
                App.Options.OnlyTeamTimeEvaluation = False
                App.Options.ShowAppNavigation = True
            End If
            ' D0859 ==

            'If Not App.Options.isSingleModeEvaluation Then App.Options.SingleModeProjectPasscode = CStr(IIf(isTokenString, sPasscode, "")) ' D0304 + D0438 + D0471 + D0659
            If (isTokenString AndAlso sEmail.ToLower <> GetCookie(_COOKIE_EMAIL, "").ToLower) AndAlso sMeetingID = "" Then fRememberMe = False ' D0304 + D0391 + D3866
            If GetParam(tParams, "TTOnly") = "1" OrElse tFormKind = ecAuthenticateWay.awJoinMeeting Then App.Options.OnlyTeamTimeEvaluation = True ' D0655 + D0659

            If GetParam(tParams, _ACTION_NAVIGATION) = _ROLE_HIDE Then App.Options.ShowAppNavigation = False ' D0349 + D0352

            Dim fForcePipeYes As Boolean = False    ' D7144
            Dim fForcePipeNo As Boolean = False     ' D7144
            Dim fCanEvaluate As Boolean = False     ' D7145
            Dim fCanModify As Boolean = False       ' D7145

            ' D0659 ===
            If Not fHasError AndAlso (tFormKind = ecAuthenticateWay.awJoinMeeting OrElse App.Options.OnlyTeamTimeEvaluation AndAlso sMeetingID <> "") Then  ' D2433
                AuthResult = AuthenticateByMeetingID(sEmail, ParamByName(tParams, _PARAMS_USERNAME), sMeetingID, sPasscode) ' D0660
                fHasError = AuthResult <> ecAuthenticateError.aeNoErrors
                'If Not fHasError AndAlso Not HasPermission(_PGID_EVALUATE_TEAMTIME, App.ActiveProject) Then
                '    AuthResult = ecAuthenticateError.aeEvaluationNotAllowed ' D2808
                '    fHasError = True
                'End If

                If Not fHasError Then
                    fAllowQuickSignup = True
                    App.Options.SingleModeProjectPasscode = sPasscode
                End If

                ' D4920 ===
                If AuthResult = ecAuthenticateError.aeWrongMeetingID Then
                    Dim tMeeting As Long
                    If clsMeetingID.TryParse(sMeetingID.Substring(1), tMeeting) Then
                        Dim tPrj As clsProject = App.DBProjectByMeetingID(tMeeting)
                        If tPrj IsNot Nothing Then
                            If App.isAuthorized Then App.Logout()
                            App.Antigua_ResetCredentials()
                            App.Antigua_MeetingID = tMeeting
                            App.Antigua_UserEmail = sEmail
                            App.Antigua_UserName = ParamByName(tParams, _PARAMS_USERNAME)
                            Dim tUser As clsApplicationUser = App.DBUserByEmail(sEmail)
                            If tUser IsNot Nothing Then
                                App.Antigua_UserID = tUser.UserID
                                If App.Antigua_UserName = "" AndAlso tUser.UserName <> "" Then App.Antigua_UserName = tUser.UserName
                            End If
                            SetCookie(_COOKIE_REMEMBER, CStr(IIf(fRememberMe, "1", "0")))
                            If fRememberMe Then
                                ' D7148 ===
                                SetCookie(_COOKIE_EMAIL, sEmail, False, True)
                                SetCookie(_COOKIE_MEETING_LOGIN, App.Antigua_UserName, True, True)
                                SetCookie(_COOKIE_MEETING_ID, sMeetingID, True, True)
                                ' D7148 ==
                            End If
                            sErrorMessage = ""
                            sRedirectURL = PageURL(_PGID_ANTIGUA_MEETING)
                            AuthResult = ecAuthenticateError.aeNoErrors
                            Return AuthResult
                        End If
                    End If
                End If
                ' D4920 ==
            End If
            ' D0659 ==

            If Not Integer.TryParse(GetParam(tParams, _PARAM_ROLEGROUP), App.Options.UserRoleGroupID) Then App.Options.UserRoleGroupID = -1 ' D1937
            If Not Integer.TryParse(GetParam(tParams, _PARAM_WKG_ROLEGROUP), App.Options.WorkgroupRoleGroupID) Then App.Options.WorkgroupRoleGroupID = -1 ' D2287

            If GetParam(tParams, _PARAM_AS_PM) = "1" Then App.Options.JoinAsPMonAttachProject = True ' D4332

            ' D0226 ===
            If Not fHasError And fAllowQuickSignup Then ' D0390
                Dim sName As String = ParamByName(tParams, _PARAMS_USERNAME)
                If sName = "" Then sName = sEmail
                Dim tmpUser As clsApplicationUser = App.UserWithSignup(sEmail, sName, sPsw, "Sign-up via URL", Nothing, Not HasParamByName(tParams, _PARAMS_PASSWORD)) ' D0659 + D1057 + D2215 + D2297
                If tmpUser IsNot Nothing AndAlso tFormKind = ecAuthenticateWay.awJoinMeeting Then sPsw = tmpUser.UserPassword ' D0659
            End If
            ' D0226 ==

            If sEmail <> "" Then

                If Not fHasError Then
                    ' D4943 ===
                    Dim fDoAuth As Boolean = True   ' D6224
                    If App.isAuthorized AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso Not fAllowQuickSignup AndAlso isTokenString Then
                        If App.ActiveUser.UserEmail.Equals(sEmail, StringComparison.CurrentCultureIgnoreCase) AndAlso App.ActiveUser.UserPassword = sPsw Then   ' D7564
                            If (Not App.HasActiveProject AndAlso sPasscode = "") OrElse (App.HasActiveProject AndAlso App.ActiveProject.Passcode(App.ActiveProject.isImpact).Equals(sPasscode, StringComparison.CurrentCultureIgnoreCase)) Then  ' D4950
                                AuthResult = ecAuthenticateError.aeUnknown
                                If Request IsNot Nothing AndAlso Request.Url IsNot Nothing Then
                                    AuthResult = ecAuthenticateError.aeNoErrors
                                    sExtraParams = GetParamsWithoutAuthKeys(If(Request Is Nothing, tParamsOrig, Request.QueryString))  ' D7167
                                    If sExtraParams <> "" Then sExtraParams = "?" + sExtraParams
                                    sRedirectURL = String.Format("{0}{1}", RemoveXssFromUrl(Request.Url.AbsolutePath), sExtraParams)    ' D6767
                                End If
                                If CurrentPageID = _PGID_START OrElse CurrentPageID = _PGID_START_WITH_SIGNUP Then
                                    fDoAuth = False ' D6224
                                Else
                                    Return AuthResult
                                End If
                            End If
                        End If
                    End If
                    ' D4943 ==
                    If fDoAuth Then
                        If App.isAuthorized OrElse App.isAntiguaAuthorized Then App.Logout()    ' D6407
                        AuthResult = App.Logon(sEmail, sPsw, sPasscode, fAllowQuickSignup OrElse (tFormKind = ecAuthenticateWay.awJoinMeeting AndAlso sMeetingID <> "" AndAlso _MEETING_ID_AVAILABLE) OrElse Str2Bool(GetParam(tParams, "ignoreoffline")) OrElse tFormKind = ecAuthenticateWay.awSSO, AllowBlankPsw, False) ' D0096 + D0390 + D0471 + D0511 + D1057 + D1672 + D1724 + D6224 + D6364 + D7327 + D7440
                    End If
                End If

                ' D0659 + D2857 ===
                If Not fHasError AndAlso (tFormKind = ecAuthenticateWay.awJoinMeeting OrElse App.Options.OnlyTeamTimeEvaluation) Then  ' D2433 + D2943
                    'If App.ActiveProject.isValidDBVersion AndAlso Not App.ActiveProject.PipeParameters.TeamTimeAllowMeetingID AndAlso Not HasPermission(_PGID_EVALUATE_TEAMTIME, App.ActiveProject) Then
                    If App.HasActiveProject AndAlso App.ActiveProject.isValidDBVersion AndAlso Not HasPermission(_PGID_TEAMTIME, App.ActiveProject) Then   ' D2943
                        AuthResult = ecAuthenticateError.aeEvaluationNotAllowed ' D2808
                        fHasError = True
                    End If
                End If
                ' D0659 + D2857 ==

                'If AuthResult = ecAuthenticateError.aeNoErrors OrElse AuthResult = ecAuthenticateError.aeNoWorkgroupSelected Then   ' D1768
                If AuthResult = ecAuthenticateError.aeNoErrors Then   ' D1768 + D4842
                    ' D6640 ===
                    If App.ActiveUser IsNot Nothing AndAlso _ADMINS_LOCAL_ONLY AndAlso Not Request.IsLocal Then
                        If App.ActiveUser.CannotBeDeleted OrElse App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then
                            App.Logout()
                            AuthResult = ecAuthenticateError.aeLocalhostAllowedOnly
                        End If
                    End If
                    If AuthResult = ecAuthenticateError.aeNoErrors Then
                        ' D6640 ==
                        SetCookie(_COOKIE_REMEMBER, CStr(IIf(fRememberMe, "1", "0")))
                        If fRememberMe AndAlso Not isSSO() Then   ' D6552
                            SetCookie(_COOKIE_EMAIL, sEmail, False, True)   ' D0755
                            SetCookie(_COOKIE_PASSWORD, ParamByName(tParams, _PARAMS_PASSWORD), False, True)   ' D0390 + D0579 + D0755
                            'SetCookie(_COOKIE_PASSCODE, ParamByName(tParams, _PARAMS_PASSCODE))   ' D0033 + D0390 -D6287
                            SetCookie(_COOKIE_MEETING_ID, RemoveHTMLTags(ParamByName(tParams, _PARAMS_MEETING_ID)), True, True) ' D0390 + D7148
                        Else
                            SetCookie(_COOKIE_PASSWORD, "", False, True)    ' D0755
                            'SetCookie(_COOKIE_PASSCODE, "")     ' D0033 -D6287
                            SetCookie(_COOKIE_MEETING_ID, "")   ' D0390
                        End If
                    End If
                    ' D7363 ===
                    Dim sRetUser As String = HttpUtility.UrlDecode(GetCookie(_COOKIE_RET_USER, ""))
                    If sRetUser <> "" Then
                        Dim sRetEmail As String = DecodeString(sRetUser, App.DatabaseID)
                        If Not String.IsNullOrEmpty(sRetEmail) AndAlso App.OriginalSessionUser = "" Then App.OriginalSessionUser = sRetEmail
                    End If
                    ' D7363 ==
                End If
            End If

            Dim sURL As String = ""

            ' D0226 ==
            If App.isAuthorized AndAlso CurrentPageID <> _PGID_ERROR_403 AndAlso (AuthResult = ecAuthenticateError.aeNoUserFound OrElse AuthResult = ecAuthenticateError.aeWrongPassword OrElse AuthResult = ecAuthenticateError.aeEvaluationNotAllowed OrElse AuthResult = ecAuthenticateError.aeMeetingIDNotAllowed) Then   ' D2943
                App.Logout()
            End If
            ' D0226 ==

            '' D0737 + D0840 ===
            'If AuthResult = ecAuthenticateError.aeNoWorkgroupSelected AndAlso App.Options.ShowSilverlightShellOnLogon AndAlso App.UserWorkgroups.Count > 1 Then
            '    Dim LastVisitedWGID As Integer = -1
            '    If Not App.ActiveUser.Session Is Nothing AndAlso App.ActiveUser.Session.WorkgroupID > 0 Then LastVisitedWGID = App.ActiveUser.Session.WorkgroupID
            '    Dim tLVWkg As clsWorkgroup = clsWorkgroup.WorkgroupByID(LastVisitedWGID, App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups))
            '    If tLVWkg IsNot Nothing Then
            '        If tLVWkg.Status = ecWorkgroupStatus.wsEnabled AndAlso tLVWkg.License.isValidLicense AndAlso tLVWkg.License.CheckParameterByID(ecLicenseParameter.ExpirationDate, Nothing, True) AndAlso tLVWkg.License.CheckParameterByID(ecLicenseParameter.InstanceID, Nothing, True) Then ' D1175 + D3947
            '            App.ActiveWorkgroup = tLVWkg
            '            App.ActiveUserWorkgroup = Nothing
            '            App.Workspaces = Nothing
            '            App.ActiveProject = Nothing
            '            AuthResult = ecAuthenticateError.aeNoErrors
            '        End If
            '    End If
            'End If
            '' D0737 +D0840 ==

            If AuthResult = ecAuthenticateError.aeWrongPassword AndAlso isTokenString AndAlso sTinyURL <> "" Then AuthResult = ecAuthenticateError.aeWrongCredentials   ' D7137

            Select Case AuthResult  ' D0046
                Case ecAuthenticateError.aeNoErrors

                    sURL = PageURL(_PGID_PROJECTSLIST)    ' D0043 + D0438
                    If sPasscode <> "" Then

                        If ParamByName(tParams, _PARAMS_PASSCODE) <> "" Then App.Options.isLoggedInWithPasscode = True ' D1355 + D2927

                        Dim Prj As clsProject = App.ActiveProject
                        If Prj Is Nothing Then
                            sURL = ""
                            App.Logout()
                            AuthResult = ecAuthenticateError.aeWrongPasscode    ' D0214
                            sPasscode = ""  ' D0304
                        Else
                            App.ProjectID = Prj.ID  ' D0043

                            fCanEvaluate = App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkgroup)       ' D4390
                            fCanModify = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup)  ' D4689

                            ' D2943 ===
                            If App.Options.OnlyTeamTimeEvaluation AndAlso sMeetingID = "" AndAlso sPasscode <> "" AndAlso fCanEvaluate Then    ' D4390
                                If sPasscode = Prj.PasscodeImpact Then sMeetingID = Prj.MeetingIDImpact.ToString
                                If sPasscode = Prj.PasscodeLikelihood Then sMeetingID = Prj.MeetingIDLikelihood.ToString
                            End If
                            ' D2943 ==

                            ' D2927 ===
                            If App.HasActiveProject AndAlso (App.ActiveProject.isTeamTimeImpact OrElse App.ActiveProject.isTeamTimeLikelihood) AndAlso fCanEvaluate Then   ' D4390
                                If App.ActiveProject.LockInfo IsNot Nothing AndAlso App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsLockForTeamTime AndAlso Not App.ActiveProject.LockInfo.isLockedByUser(App.ActiveUser) Then
                                    'tFormKind = ecAuthenticateWay.awJoinMeeting
                                    sMeetingID = CStr(IIf(App.ActiveProject.isTeamTimeLikelihood, App.ActiveProject.MeetingIDLikelihood, App.ActiveProject.isTeamTimeImpact))
                                    App.Options.isLoggedInWithMeetingID = True
                                End If
                            End If
                            ' D2927 ==

                            ' D0390 ===
                            ' Try to attach new user to TeamTime session by MeetingID
                            If App.isAuthorized AndAlso sMeetingID <> "" Then
                                If App.ActiveProject.isValidDBVersion Then
                                    ' D2943 ===
                                    Dim tMtg As Long
                                    If Not clsMeetingID.TryParse(sMeetingID, tMtg) Then tMtg = -1
                                    If tMtg = App.ActiveProject.MeetingIDImpact Then App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
                                    If tMtg = App.ActiveProject.MeetingIDLikelihood Then App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
                                    ' D2943 ==
                                End If

                                ' D2943 ===
                                If Not App.ActiveProject.PipeParameters.TeamTimeAllowMeetingID Then
                                    If Not fCanModify AndAlso (App.ActiveUser.SyncEvaluationMode = SynchronousEvaluationMode.semNone OrElse App.ActiveWorkspace Is Nothing OrElse (Not App.ActiveWorkspace.isInTeamTime(False) AndAlso Not App.ActiveWorkspace.isInTeamTime(True))) Then    ' D2954 + D4689
                                        AuthResult = ecAuthenticateError.aeMeetingIDNotAllowed
                                        fHasError = True
                                        App.Logout()
                                        sURL = ""
                                        Exit Select
                                    End If
                                End If
                                ' D2943 ==

                                If fCanEvaluate Then ' D4390
                                    If App.ActiveWorkspace IsNot Nothing AndAlso Not App.ActiveWorkspace.isInTeamTime(App.ActiveProject.isImpact) Then    ' D0471 + D1945
                                        App.ActiveWorkspace.Status(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsSynhronousActive    ' D1945
                                        App.DBWorkspaceUpdate(App.ActiveWorkspace, False, "Attach to Synchronous Session")   ' D0499
                                    End If
                                    ' D4390 ===
                                Else
                                    If App.ActiveWorkspace IsNot Nothing AndAlso App.ActiveWorkspace.isInTeamTime(App.ActiveProject.isImpact) Then
                                        App.ActiveWorkspace.Status(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsUnknown
                                        App.DBWorkspaceUpdate(App.ActiveWorkspace, False, "Detach from Synchronous Session")
                                    End If
                                End If
                                ' D4390 ==
                            End If
                            ' D0390 ==

                            If (App.ActiveProject.isValidDBVersion OrElse App.ActiveProject.isDBVersionCanBeUpdated) AndAlso App.HasActiveProject() AndAlso sPhone <> "" AndAlso fAllowQuickSignup Then
                                With App.ActiveProject.ProjectManager
                                    If .Attributes.GetAttributeByID(ATTRIBUTE_USER_PHONE_ID) Is Nothing Then
                                        '.Attributes.AddAttribute(ATTRIBUTE_USER_PHONE_ID, ATTRIBUTE_USER_PHONE_NAME, AttributeTypes.atUser, AttributeValueTypes.avtString, "", False, , "lblUserPhone")
                                        .Attributes.AddAttribute(ATTRIBUTE_USER_PHONE_ID, ResString("lblUserPhone"), AttributeTypes.atUser, AttributeValueTypes.avtString, "", False, , "lblUserPhone")
                                        .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                                    End If

                                    Dim tPrjUser As clsUser = .GetUserByEMail(App.ActiveUser.UserEmail)
                                    If tPrjUser Is Nothing Then
                                        tPrjUser = .AddUser(App.ActiveUser.UserEmail, True, App.ActiveUser.UserName)
                                    End If

                                    .Attributes.SetAttributeValue(ATTRIBUTE_USER_PHONE_ID, tPrjUser.UserID, AttributeValueTypes.avtString, sPhone, Guid.Empty, Guid.Empty)
                                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, tPrjUser.UserID)
                                End With
                            End If

                            If App.isRiskEnabled AndAlso App.ActiveProject.Passcode <> sPasscode AndAlso (App.ActiveProject.isValidDBVersion OrElse App.ActiveProject.isDBVersionCanBeUpdated) Then
                                If App.ActiveProject.PasscodeImpact.ToLower = sPasscode.ToLower Then
                                    App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
                                    App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = App.ActiveProject.ProjectManager.PipeParameters.ImpactParameterSet
                                End If
                            End If

                            fForcePipeYes = fCanEvaluate AndAlso (CheckVar("pipe", False) OrElse GetParam(tParams, "pipe").Trim.ToLower = "yes" OrElse GetParam(tParams, "pipe").Trim.ToLower = "true" OrElse GetParam(tParams, "pipe").Trim = "1") ' D3866 +  D4942
                            fForcePipeNo = Not CheckVar("pipe", True) OrElse GetParam(tParams, "pipe").Trim.ToLower = "no" OrElse GetParam(tParams, "pipe").Trim.ToLower = "false" OrElse GetParam(tParams, "pipe").Trim = "0"   ' D3866
                            SessVar(_SESS_FORCE_PIPE) = Bool2Num(fForcePipeYes).ToString    ' D4404
                            App.Options.ignoreOffline = Str2Bool(GetParam(tParams, "ignoreoffline"))    ' D6619

                            Dim fUseEvalSite As Boolean = Not App.isRiskEnabled AndAlso Not CheckVar("ignoreval", False) AndAlso CheckVar("redirect", True) AndAlso (Not isAnonymous AndAlso App.CanUseEvalURLForProject(App.ActiveProject))  ' D6403 + D6410 + D6411 // ignore when URI param, riskion, anonymous+dynamic surveys

                            sURL = URLProjectID(PageURL(If(Prj.ProjectStatus = ecProjectStatus.psActive AndAlso fCanEvaluate AndAlso (Not fCanModify OrElse fForcePipeYes), _PGID_EVALUATION, _PGID_PROJECT_DESCRIPTION), , fUseEvalSite), Prj.ID)    ' D0215 + D0499 + D1221 + D2808 + D4740 + D4743 + D4942 + D6359 + D6362 + D6382 + D6403
                            ' D6089 ===
                            If fCanEvaluate AndAlso (App.Options.isLoggedInWithMeetingID OrElse App.Options.OnlyTeamTimeEvaluation) AndAlso sMeetingID <> "" Then    ' D2943 + D4740
                                sURL = PageURL(_PGID_TEAMTIME) ' D1432
                                'If Not fCanModify AndAlso fCanEvaluate AndAlso (App.Options.isLoggedInWithMeetingID OrElse App.Options.OnlyTeamTimeEvaluation) AndAlso sMeetingID <> "" Then    ' D2943 + D4740
                                'If tFormKind = ecAuthenticateWay.awStartMeeting OrElse tFormKind = ecAuthenticateWay.awJoinMeeting OrElse App.Options.OnlyTeamTimeEvaluation Then ' D6041 + D6079
                                '    sURL = PageURL(_PGID_TEAMTIME) ' D1432
                                'End If
                                ' D6089 ==
                                ' -D6086 // temporary due to a test TT
                                '' D6084 ===
                                'If fCanModify AndAlso (Not App.ActiveProject.isTeamTime OrElse App.ActiveProject.MeetingStatus(App.ActiveUser) = ECTeamTimeStatus.tsTeamTimeSessionOwner) Then
                                '    sURL = PageURL(_PGID_TEAMTIME_USERS)
                                'End If
                                '' D6084 ==
                            End If

                            ' D4740 ===
                            If (Not App.Options.isLoggedInWithMeetingID OrElse fForcePipeNo) AndAlso Not App.Options.OnlyTeamTimeEvaluation Then    ' D0750 + D2943 + D2947 + D2949 + D3296 + D3866

                                If App.Options.isSingleModeEvaluation AndAlso sMeetingID = "" AndAlso (Not App.isRiskEnabled OrElse App.Options.isLoggedInWithPasscode) Then  ' D1041 + D2943 + 3012
                                    If isMobileBrowserClient(Request) AndAlso WebConfigOption(_OPT_MOBILE_EVAL_SITE, "0", True) = "1" AndAlso App.Options.EvalSiteURL <> "" AndAlso isTokenString AndAlso sTinyURL <> "" Then
                                        sURL = String.Format("{0}?hash={1}", App.Options.EvalSiteURL, sTinyURL)
                                    End If
                                Else

                                    Dim sSessURL As String = SessVar(_SESS_RET_URL) ' D4943
                                    Dim oldEURL As String = App.Options.EvalSiteURL         ' D6403
                                    If Not fUseEvalSite Then App.Options.EvalSiteURL = ""   ' D6403
                                    ' D6399 ===
                                    Dim tRes As New jActionResult
                                    jProject.GetActionResultByProject(Me, tRes, App.ActiveProject, True, If(fForcePipeYes, ecProjectStateOnOpen.psAnytimePipe, ecProjectStateOnOpen.psRegular)) ' D6406
                                    If Not String.IsNullOrEmpty(tRes.URL) Then
                                        sURL = tRes.URL
                                    Else
                                        If fCanModify AndAlso ((App.ActiveProject.isOnline AndAlso Not fForcePipeYes) OrElse Not App.ActiveProject.isOnline OrElse App.ActiveProject.ProjectStatus <> ecProjectStatus.psActive) Then sURL = PageURL(_DEF_PGID_ONSELECTPROJECT)   ' D4952
                                    End If
                                    ' D6399 ==
                                    App.Options.EvalSiteURL = oldEURL   ' D6403

                                    ' D4943
                                    Select Case CurrentPageID
                                        Case _PGID_START, _PGID_START_WITH_SIGNUP, _PGID_SILVERLIGHT_UI, _PGID_SERVICEPAGE, _PGID_ERROR_403, _PGID_ERROR_404, _PGID_ERROR_500, _PGID_ERROR_503, _PGID_EULA, _PGID_LOGOUT, _PGID_PING, _PGID_WEBAPI, _PGID_SSO_ASSERT ' D7440
                                        Case Else
                                            If Request IsNot Nothing AndAlso Request.Url IsNot Nothing Then
                                                sURL = RemoveXssFromUrl(Request.Url.AbsolutePath)   ' D6767
                                                If sExtraParams = "" Then sExtraParams = GetParamsWithoutAuthKeys(tParamsOrig)
                                            End If
                                    End Select

                                    If (Not Str2Bool(GetParam(tParams, "ignorepsw")) AndAlso (App.ActiveUser.PasswordStatus = -1 OrElse (Not AllowBlankPsw() AndAlso App.ActiveUser.UserPassword = ""))) Then  ' D6364
                                        If Not fForcePipeYes AndAlso Not isTokenString AndAlso Not App.Options.isSingleModeEvaluation Then  ' D7138
                                            SessVar(_SESS_RET_URL) = sURL           ' D4916
                                            sURL = PageURL(_PGID_CREATE_PASSWORD)   ' D4916
                                            sSessURL = ""   ' D4943
                                        End If
                                    End If

                                    ' D4943 ===
                                    If Not IsValidEULA() Then
                                        SessVar(_SESS_RET_URL) = sURL
                                        sURL = PageURL(_PGID_EULA)
                                        sSessURL = ""   ' D4943
                                    End If

                                    If Not String.IsNullOrEmpty(sSessURL) Then sURL = sSessURL
                                    ' D4943 ==

                                End If
                            End If
                            ' D4740 ==
                        End If

                    Else    ' sPasscode = ""
                        ' D0089 ===
                        If App.ActiveWorkgroup IsNot Nothing Then
                            If App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then sURL = PageURL(_DEF_PGID_ONSYSTEMWORKGROUP)
                        End If
                        ' D0089 ==

                        If sPasscode = "" AndAlso Not IsValidEULA() Then sURL = PageURL(_PGID_EULA)
                    End If

                    If sPasscode = "" AndAlso DebugRestoreLastPage AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then ' D4744
                        If App.ActiveUserWorkgroup IsNot Nothing Then
                            Dim oldPrj As clsProject = clsProject.ProjectByID(App.ActiveUserWorkgroup.LastProjectID, App.ActiveProjectsList)    ' D6533
                            If oldPrj Is Nothing OrElse (oldPrj IsNot Nothing AndAlso Not oldPrj.isMarkedAsDeleted AndAlso oldPrj.isValidDBVersion) Then   ' D6533 + D6922 + D7307
                                If oldPrj IsNot Nothing Then App.ProjectID = App.ActiveUserWorkgroup.LastProjectID ' D4744 + D7307
                                sURL = GetOptionalStartURL(App.ActiveUser.UserID, sURL)
                            End If
                        End If
                    End If

                    ' D7187 ===
                    If Str2Bool(GetParam(tParams, "pin")) Then
                        If String.IsNullOrEmpty(SessVar(_SESS_RET_URL)) Then SessVar(_SESS_RET_URL) = sURL
                        sURL = PageURL(_PGID_PINCODE)
                    End If
                    ' D7187 ==

                '    ' D0077 ===
                'Case ecAuthenticateError.aeNoWorkgroupSelected
                '    ' D0096 ===
                '    If App.ActiveUser Is Nothing Then App.ActiveUser = App.DBUserByEmail(sEmail)
                '    'App.ActiveUser = App.User(sEmail)
                '    If Not App.ActiveUser Is Nothing Then
                '        'App.DB_UserSessionStart(App.ActiveUser) ' D0101
                '        sURL = PageURL(_PGID_WORKGROUP_SELECT)
                '    End If
                '    ' D0096 ==
                '    ' D0077 ==

                Case ecAuthenticateError.aeWrongPassword    ' D0519
                    If tFormKind = ecAuthenticateWay.awJoinMeeting Then AuthResult = ecAuthenticateError.aeUseRegularLogon ' D0519
                    ' D6068 ===
                    If App.CanvasMasterDBVersion >= "0.9996" AndAlso App.Database.Connect AndAlso sEmail <> "" Then
                        If CheckUserLockWhenInvalidPassword(sEmail) Then    ' D6346 + D6351
                            AuthResult = ecAuthenticateError.aeUserLockedByWrongPsw
                            sErrorMessage = ParseAllTemplates(App.GetMessageByAuthErrorCode(AuthResult), App.ActiveUser, Nothing)  ' D3993
                        End If
                    End If
                    ' D6068 ==

                Case ecAuthenticateError.aeNoUserFound, ecAuthenticateError.aeWrongPassword
                    Threading.Thread.Sleep(5000)

            End Select

            ' D0272 + D0273 ===
            If AuthResult = ecAuthenticateError.aeNoErrors And Not App.ActiveUser Is Nothing Then
                ' D2288 ===
                If App.ActiveUser.UserEmail.ToLower = _DB_DEFAULT_ADMIN_LOGIN.ToLower Then
                    Dim sSQL As String = String.Format("SELECT * FROM {0} WHERE {1}={2} AND NOT {3} LIKE '{4}' AND NOT {3} LIKE '{5}'", clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_STATUS, clsApplicationUser._mask_CantBeDelete, clsComparionCore._FLD_USERS_EMAIL, _DB_DEFAULT_ADMIN_LOGIN, ReviewAccount)
                    Dim tList As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(sSQL)
                    If tList IsNot Nothing Then
                        For Each tRow As Dictionary(Of String, Object) In tList
                            Dim tUser As clsApplicationUser = App.DBParse_ApplicationUser(tRow)
                            If tUser.CannotBeDeleted Then
                                tUser.CannotBeDeleted = False
                                App.DBUserUpdate(tUser, False, "Reset wrong status (not allowed flag)")
                            End If
                        Next
                    End If
                End If
                ' D2288 ==
                'If isTokenString AndAlso App.Options.isSingleModeEvaluation AndAlso App.Options.ShowSilverlightShellOnLogon Then App.Options.SingleModeProjectPasscode = "" ' D0846 -D0850
                '                If Not App.CheckUserEULA(App.ActiveUserWorkgroup) Then sURL = PageURL(_PGID_EULA)
                Dim tRes As DateTime? = App.CheckSessionTerminate(App.ActiveUser)   ' D7536
                If tRes.HasValue Then SessVar(_SESS_CMD_TERMINATE) = Date2ULong(tRes.Value).ToString    ' D7356
            End If
            ' D0272 + D0273 ==
            sRedirectURL = sURL.Replace("?#/", "#/")    ' D4286s
            Dim sMode As String = GetParam(tParams, "mode") ' D7241
            If sMode <> "" AndAlso sMode.Length > 2 AndAlso Not sExtraParams.ToLower.Contains("mode=") Then sExtraParams += If(sExtraParams = "", "", "&") + "mode=" + sMode    ' D7241 + D7283 // Len>2 since the usually "mode" contains the AuthMethod
            If sExtraParams <> "" AndAlso sRedirectURL <> "" Then sRedirectURL += IIf(sRedirectURL.Contains("?"), "&", "?").ToString + sExtraParams ' D1505
            If AuthResult <> ecAuthenticateError.aeNoErrors Then
                App.Options.SingleModeProjectPasscode = ""      ' D0789
                App.Options.OnlyTeamTimeEvaluation = False      ' D0789
                ' D1207 ===
                If sPasscode = "" AndAlso AuthResult = ecAuthenticateError.aeProjectLocked AndAlso sMeetingID <> "" AndAlso (tFormKind = ecAuthenticateWay.awJoinMeeting Or tFormKind = ecAuthenticateWay.awStartMeeting) Then
                    Dim tMeeting As Long
                    If clsMeetingID.TryParse(sMeetingID, tMeeting) Then
                        Dim tPrj As clsProject = App.DBProjectByMeetingID(tMeeting)
                        If tPrj IsNot Nothing Then sPasscode = tPrj.Passcode
                    End If
                End If
                ' D1207 ==
                sErrorMessage = ParseAllTemplates(App.GetMessageByAuthErrorCode(AuthResult, sPasscode), App.ActiveUser, Nothing) ' D0348 + D3993
                If AuthResult = ecAuthenticateError.aeWrongLicense AndAlso App.ApplicationError.Message <> "" Then sErrorMessage = App.ApplicationError.Message ' D1208
            End If

            ' D4569 ===
            'If AuthResult = ecAuthenticateError.aeNoErrors AndAlso CurrentPageID <> _PGID_CREATE_PASSWORD AndAlso App.ActiveUser IsNot Nothing AndAlso App.ActiveUser.PasswordStatus <0 AndAlso Not isAnonymous AndAlso tFormKind <> ecAuthenticateWay.awJoinMeeting Then   ' D4599 + D4603
            If AuthResult = ecAuthenticateError.aeNoErrors AndAlso CurrentPageID <> _PGID_CREATE_PASSWORD AndAlso App.ActiveUser IsNot Nothing AndAlso App.ActiveUser.PasswordStatus < 0 AndAlso Not isAnonymous AndAlso tFormKind <> ecAuthenticateWay.awJoinMeeting AndAlso Not Str2Bool(GetParam(tParams, "ignorepsw")) AndAlso Not isSSO() AndAlso Not App.Options.isLoggedInWithMeetingID AndAlso (_DEF_ASK_PSW_NEW_USER_BY_INVITES OrElse (Not App.Options.isSingleModeEvaluation AndAlso Not App.Options.OnlyTeamTimeEvaluation AndAlso Not fForcePipeYes AndAlso Not fCanModify)) Then   ' D4599 + D4603 + D6364 + D6552 + D7144 + D7145 + D7147
                If String.IsNullOrEmpty(SessVar(_SESS_RET_URL)) Then SessVar(_SESS_RET_URL) = sRedirectURL           ' D4196
                sRedirectURL = PageURL(_PGID_CREATE_PASSWORD)   ' D4916
            End If
            ' D4569 ==

            ' D7502 ===
            Dim MFA As ecPinCodeType = ecPinCodeType.mfaNone
            If AuthResult = ecAuthenticateError.aeNoErrors Then
                MFA = App.GetMFA_Mode(App.ActiveUser)
                If MFA <> ecPinCodeType.mfaNone Then AuthResult = ecAuthenticateError.aeMFA_Required
            End If
            ' D7502 ==

            App.DBSaveLogonEvent(sEmail, AuthResult, tFormKind, Request, sErrorMessage)   ' D0557 + D6068

            ' D7502 ===
            If AuthResult = ecAuthenticateError.aeMFA_Required Then
                Dim sMFAError As String = ""
                Dim tRes As Boolean = SendMFA_Code(MFA, App.ActiveUser, sMFAError)
                If Not tRes Then
                    AuthResult = ecAuthenticateError.aeNoErrors
                    'App.DBSaveLog(dbActionType.actLogon, dbObjectType.einfUser, App.ActiveUser.UserID, "Unable to perform MFA", String.Format("{0}, Error: {1}", MFA, sMFAError))
                End If
            End If
            App.MFA_Requested = (AuthResult = ecAuthenticateError.aeMFA_Required)
            ' D7502 ==

            Return AuthResult
        End Function
        ' D0214 ==

        ' D6346 ===
        Public Function CheckUserLockWhenInvalidPassword(sEmail As String) As Boolean
            Dim tUser As clsApplicationUser = App.DBUserByEmail(sEmail)
            Return CheckUserLockWhenInvalidPassword(tUser)
        End Function

        ' D6062 ===
        Public Function CheckUserLockWhenInvalidPassword(ByRef tUser As clsApplicationUser) As Boolean
            ' D6346 ==
            Dim fIsLocked As Boolean = False
            Try
                ' D2217 ===
                'Dim tUser As clsApplicationUser = App.DBUserByEmail(sEmail)    ' -D6346
                If tUser IsNot Nothing Then
                    ' D6062 ===
                    Dim Errors_Cnt As Integer = 0   ' D6074
                    If _DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK Then   ' D6074
                        Dim ActionIDs As String = String.Format("{0},{1},{2},{3},{4}", CInt(dbActionType.actLogon), CInt(dbActionType.actTokenizedURLLogon), CInt(dbActionType.actCredentialsLogon), CInt(dbActionType.actSSOLogin), CInt(dbActionType.actUnLock))  ' D6077
                        Dim sSQL As String = String.Format("SELECT * FROM Logs WHERE ActionID IN ({0}) And TypeID={1} And (ObjectID={2} Or COMMENT Like ?) And DT>? ORDER BY DT", ActionIDs, CInt(dbObjectType.einfUser), tUser.UserID)   ' D6077
                        Dim sqlParams As New List(Of Object)
                        sqlParams.Add(String.Format("{0} %", tUser.UserEmail))
                        sqlParams.Add(Now.AddMinutes(-_DEF_PASSWORD_ATTEMPTS_PERIOD))   ' D6070
                        Dim tries As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(sSQL, sqlParams)
                        Dim aewp As String = ecAuthenticateError.aeWrongPassword.ToString.ToLower
                        Dim aeul As String = ecAuthenticateError.aeUserLockedByWrongPsw.ToString.ToLower    ' D6077
                        For Each tRow As Dictionary(Of String, Object) In tries
                            If Not IsDBNull(tRow("Result")) Then
                                ' D6077 ===
                                If CInt(tRow("ActionID")) = CInt(dbActionType.actUnLock) Then
                                    Errors_Cnt = 0
                                Else
                                    Dim sResult As String = CStr(tRow("Result")).ToLower
                                    If sResult.StartsWith(aewp) OrElse sResult.StartsWith(aeul) Then
                                        If (sResult.StartsWith(aeul)) AndAlso Errors_Cnt < _DEF_PASSWORD_ATTEMPTS Then Errors_Cnt = _DEF_PASSWORD_ATTEMPTS Else Errors_Cnt += 1
                                    Else
                                        Errors_Cnt = 0
                                    End If
                                End If
                                ' D6077 ==
                            End If
                        Next
                        ' D6074 ===
                    Else
                        Errors_Cnt = tUser.PasswordStatus
                    End If
                    If Errors_Cnt < 0 Then Errors_Cnt = 0
                    ' D6074 ==
                    Errors_Cnt += 1
                    If Errors_Cnt > _DEF_PASSWORD_ATTEMPTS Then Errors_Cnt = _DEF_PASSWORD_ATTEMPTS  ' D6086
                    Dim OldStatus As Integer = tUser.PasswordStatus
                    tUser.PasswordStatus = Errors_Cnt
                    ' D2700 ===
                    If OldStatus <> tUser.PasswordStatus Then
                        ' D6062 ==
                        Dim tParams As New List(Of Object)
                        tParams.Add(tUser.UserEmail)    ' D6346
                        ' D2700 ==
                        'If tUser.PasswordStatus < 1 Then tUser.PasswordStatus = 1 Else tUser.PasswordStatus += 1   ' -D0602 due to assign as Errors_Cnt where ot's with a current error event
                        App.Database.ExecuteSQL(String.Format("UPDATE {0} SET PasswordStatus={2} WHERE {1}=?", clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_EMAIL, tUser.PasswordStatus), tParams)
                        ' D6065 ===
                        If tUser.PasswordStatus = _DEF_PASSWORD_ATTEMPTS Then
                            App.DBSaveLog(dbActionType.actLock, dbObjectType.einfUser, tUser.UserID, "Lock user due to a max login attempts" + If(_DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK, " in a period", ""), GetClientIP(Request)) ' D6074
                            If Not isSSO_Only() Then    ' D7432
                                Dim sError As String = ""
                                Dim sOldURL As String = App.Options.EvalSiteURL ' D6372
                                App.Options.EvalSiteURL = ""                    ' D6372
                                SendMail(WebOptions.SystemEmail, tUser.UserEmail, ParseAllTemplates(ResString("subjLockedPswAttempts", False, False), tUser, Nothing), ParseAllTemplates(ResString(If(_DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK, "bodyLockedPswAttempts", "bodyLockedPswNoUnlock"), False, False), tUser, Nothing), sError, "", False, WebOptions.SMTPSSL)  ' D6367
                                App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfUser, tUser.UserID, "Email about the locking account due to a wrong password", sError)
                                App.Options.EvalSiteURL = sOldURL   ' D6372
                            End If
                        End If
                        ' D6065 ==
                        Threading.Thread.Sleep(3000)    ' D2798
                    End If
                    If tUser.PasswordStatus >= _DEF_PASSWORD_ATTEMPTS Then
                        App.DBUpdateDateTime(clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_LASTVISITED, tUser.UserID)  ' D6062
                        fIsLocked = True
                    End If
                End If
                ' D2217 ==
            Catch ex As Exception
            End Try
            ' D2213 ==
            Return fIsLocked
        End Function
        ' D6062 ==

        ' D7502 ===
        Public Function SendMFA_Email(tUser As clsApplicationUser, sCode As String, Optional ByRef sError As String = Nothing) As Boolean
            Dim tRes As Boolean = False
            If tUser IsNot Nothing Then
                tRes = SendMail(WebOptions.SystemEmail, tUser.UserEmail, ParseAllTemplates(ResString("subjMFA_Code", False, False), tUser, Nothing), ParseAllTemplates(String.Format(ResString("bodyMFA_Code", False, False), sCode, (_DEF_MFA_EMAIL_TIMEOUT \ 60)), tUser, Nothing), sError, "", False, SMTPSSL)  ' D7507 + D7508
            End If
            Return tRes
        End Function

        Public Function SendMFA_Code(PinType As ecPinCodeType, tUser As clsApplicationUser, Optional ByRef sError As String = Nothing) As Boolean
            Dim tRes As Boolean = False
            If PinType <> ecPinCodeType.mfaNone AndAlso tUser IsNot Nothing Then
                Select Case PinType
                    Case ecPinCodeType.mfaEmail
                        Dim Data As Tuple(Of Integer, Integer) = App.GetUserPin(PinType, tUser.UserID, , , False)   ' D7504
                        tRes = SendMFA_Email(tUser, Data.Item1.ToString, sError)
                End Select
                App.DBSaveLog(dbActionType.actLogon, dbObjectType.einfUser, tUser.UserID, "Send MFA code", String.Format("{0}: {1}", PinType, If(tRes, "OK", If(String.IsNullOrEmpty(sError), "Unknown error", "Error: " + sError))))
            End If
            Return tRes
        End Function
        ' D7502 ==

        ' D7114 ===
        Public Function TinyURLUpdateUserPsw(UserID As Integer, sOldPsw As String, sNewPsw As String) As Integer
            Dim cnt As Integer = 0
            If UserID > 0 AndAlso sOldPsw <> sNewPsw Then

                Dim sqlUpdate As String = ""

                Dim sqlParams As New List(Of Object)
                sqlParams.Add(UserID)
                Dim Rows As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL("SELECT * FROM PrivateURLs WHERE UserID = ?", sqlParams)

                sqlParams.Clear()
                For Each tRow As Dictionary(Of String, Object) In Rows
                    Dim sHash As String = CStr(tRow("Hash"))
                    Dim sURL As String = CStr(tRow("URL"))
                    If sURL <> "" Then
                        Dim sDecoded As String = DecodeURL(sURL, App.DatabaseID)
                        If Not String.IsNullOrEmpty(sDecoded) Then
                            Dim tParams As NameValueCollection = HttpUtility.ParseQueryString(sDecoded)
                            If HasParamByName(tParams, _PARAMS_PASSWORD) Then
                                For Each sPswParam As String In tParams.Keys
                                    If Not String.IsNullOrEmpty(sPswParam) Then
                                        If _PARAMS_PASSWORD.Contains(sPswParam.ToLower) Then
                                            If tParams(sPswParam) IsNot Nothing AndAlso tParams(sPswParam) = sOldPsw Then
                                                tParams(sPswParam) = sNewPsw
                                                Dim sNewURL As String = tParams.ToString
                                                Dim sEncoded As String = EncodeURL(sNewURL, App.DatabaseID)
                                                sqlUpdate += String.Format("UPDATE PrivateURLs SET URL = ? WHERE Hash = ?" + vbNewLine)
                                                sqlParams.Add(sEncoded)
                                                sqlParams.Add(sHash)
                                                cnt += 1
                                                Exit For
                                            End If
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    End If
                Next
                If cnt > 0 AndAlso sqlUpdate <> "" Then
                    Dim affected As Integer = App.Database.ExecuteSQL(sqlUpdate, sqlParams)
                    If affected > 0 Then App.DBSaveLog(dbActionType.actModify, dbObjectType.einfUser, UserID, "Update existing token URLs after changing the user psw", String.Format("Updated: {0} of {1}", affected, cnt))
                End If
            End If
            Return cnt
        End Function
        ' D7114 ==

        ' D7446 ===
        Public Function SSO_Logout(ByRef sError As String) As Boolean
            Dim fResult As Boolean = False
            If App.SSO_User Then
                Try
                    ComponentSpace.SAML2.SAMLServiceProvider.SendSLO(Response, sError)
                    fResult = True
                Catch ex As Exception
                    If sError IsNot Nothing Then sError += If(sError = "", "", "; RTE: ") + ex.Message
                End Try
            End If
            Return fResult
        End Function
        ' D7446 ==

        ' D2055 ===
        Public Sub UserLogout(Optional sLogoutMessage As String = "")   ' D7159
            Dim sAppError As String = ""
            Dim sError As String = ""
            ' D7446 ===
            If App.SSO_User Then
                If Not SSO_Logout(sError) Then sAppError = sError
                Dim UID As Integer = If(App.ActiveUser Is Nothing, -1, App.ActiveUser.UserID)
                App.DBSaveLog(dbActionType.actLogout, dbObjectType.einfUser, UID, "SSO Logout", "Error: " + sError, UID, If(App.ActiveWorkgroup Is Nothing, -1, App.ActiveWorkgroup.ID))
            End If
            ' D7446 ==
            App.Logout(sLogoutMessage)  ' D7159
            Session(_SESS_TT_Pipe) = Nothing
            Session(_SESS_TT_Passcode) = Nothing    ' D6789
            Session(SESS_QH_MD5) = Nothing
            Session.Abandon()
            OptionsIndividualInit(App)  ' D6256
            Response.Cookies.Add(New HttpCookie("ASP.NET_SessionId", ""))
            SetCookie("first_run", "0", True, False)
            SetCookie(_COOKIE_FEDRAMP_NOTIFICATION, "") ' D6642
            If Not String.IsNullOrEmpty(sAppError) Then App.ApplicationError.Init(ecErrorStatus.errMessage, _PGID_START, sAppError)   ' D7446  // AD: doesn't work since reset all session things. No workaround for now.
        End Sub
        ' D2055 ==

        ' D1529 ===
        Public Sub LogoutAndCheckReturnUser(Optional sLogoutMessage As String = "")
            Dim sUser As String = ""
            If App.ActiveUser IsNot Nothing Then sUser = App.ActiveUser.UserEmail
            App.Logout(sLogoutMessage)
            CheckReturnUser(sUser)
        End Sub

        Public Sub CheckReturnUser(sActiveUserEmail As String)
            Dim sRetUser As String = HttpUtility.UrlDecode(GetCookie(_COOKIE_RET_USER, ""))
            Dim sRetPrjID As String = GetCookie(_COOKIE_RET_PRJID, "")
            Dim sRetPasscode As String = GetCookie(_COOKIE_RET_PASSCODE, "")    ' D1696
            Dim sRetHID As String = GetCookie(_COOKIE_RET_HID, "")  ' D1672
            Dim sRetPath As String = HttpUtility.UrlDecode(GetCookie(_COOKIE_RET_PATH, ""))
            Dim sRetRemember As String = HttpUtility.UrlDecode(GetCookie(_COOKIE_RET_REMEMBER, "")) ' D1696
            If sActiveUserEmail <> "" AndAlso Not String.IsNullOrEmpty(sRetUser) Then
                sRetUser = DecodeString(sRetUser, App.DatabaseID)
                If sActiveUserEmail <> sRetUser Then
                    DebugInfo("Detect 'return user': " + sRetUser)
                    SetCookie(_COOKIE_RET_USER, "")
                    SetCookie(_COOKIE_RET_PRJID, "")
                    SetCookie(_COOKIE_RET_PASSCODE, "") ' D6364
                    SetCookie(_COOKIE_RET_HID, "")  ' D1672
                    SetCookie(_COOKIE_RET_PATH, "")
                    SetCookie(_COOKIE_RET_REMEMBER, "") ' D1696
                    If sRetUser.ToLower = App.OriginalSessionUser.ToLower Then App.OriginalSessionUser = "" ' D7363
                    Dim tUser As clsApplicationUser = App.DBUserByEmail(sRetUser)
                    If tUser IsNot Nothing Then
                        Dim tProject As clsProject = Nothing
                        Dim mPrjID As Integer = -1
                        If Integer.TryParse(sRetPrjID, mPrjID) Then tProject = App.DBProjectByID(mPrjID)
                        If String.IsNullOrEmpty(sRetPath) Then sRetPath = GetOptionalStartURL(tUser.UserID, sRetPath)
                        Dim tParams As New NameValueCollection()
                        tParams.Add(_PARAM_EMAIL, tUser.UserEmail)
                        tParams.Add(_PARAM_PASSWORD, tUser.UserPassword)
                        tParams.Add(_PARAM_REMEMBERME, sRetRemember)    ' D1696
                        If sRetPasscode <> "" Then tParams.Add(_PARAM_PASSCODE, sRetPasscode) ' D3387
                        If sRetPasscode = "" AndAlso tProject IsNot Nothing Then  ' D3387
                            tParams.Add(_PARAM_PASSCODE, CStr(IIf(sRetHID <> CStr(CInt(ECHierarchyID.hidImpact)) OrElse tProject.PasscodeImpact = "", tProject.PasscodeLikelihood, tProject.PasscodeImpact)))   ' D1724
                        End If
                        Dim sError As String = ""
                        Dim sTmpPath As String = ""
                        Dim tErrCode As ecAuthenticateError = Authenticate(tParams, sError, sTmpPath, ecAuthenticateWay.awRegular, True)    ' D6532
                        'SetCookie(_COOKIE_PASSCODE, sRetPasscode)   ' D1696
                        'If Str2Bool(sRetRemember) Then SetCookie(_COOKIE_PASSCODE, GetCookie(_COOKIE_RET_ORIG_PASSCODE, ""), False)    ' D4744 -D6287
                        'SetCookie(_COOKIE_RET_ORIG_PASSCODE, "")    ' D4744 -D6287
                        If tErrCode = ecAuthenticateError.aeNoErrors Then
                            Response.Redirect(sRetPath, True)
                        Else
                            DebugInfo("No success for login as return user")
                        End If
                    End If
                Else
                    App.OriginalSessionUser = ""        ' D7363
                End If
            End If
            UserLogout()    ' D6642 + D7134
        End Sub
        ' D1529 ==

        ' D0112 ===
        Public Property AlignHorizontalCenter() As Boolean
            Get
                Return _fAlignHCenter
            End Get
            Set(ByVal value As Boolean)
                _fAlignHCenter = value
            End Set
        End Property

        Public Property AlignVerticalCenter() As Boolean
            Get
                Return _fAlignVCenter
            End Get
            Set(ByVal value As Boolean)
                _fAlignVCenter = value
            End Set
        End Property
        ' D0112 ==

        ' D0010 ===
        Public Property App() As clsComparionCore   ' D0466
            Get
                If _Application Is Nothing Then
                    Try
                        If Session(_SESS_APP) Is Nothing Then
                        End If
                    Catch ex As Exception
                        Session.Remove(_SESS_APP)   ' in case of crashing when unable to convert existing session object due to changes in the structure. Usually during the debug sessions
                    End Try
                    If Session(_SESS_APP) IsNot Nothing Then
                        _Application = CType(Session(_SESS_APP), clsComparionCore)
                        If Not _Application.isOptionsLoaded Or String.IsNullOrEmpty(GlobalDefaultProvider) Then LoadComparionCoreOptions(_Application) ' D0466
                        ' D0499 ===
                        If _Application.CanvasMasterConnectionDefinition.DBName.ToLower <> _Application.Options.CanvasMasterDBName.ToLower AndAlso _Application.Options.CanvasMasterDBName <> "" Then
                            _Application.ApplicationError.Reset()
                            _Application.CanvasMasterConnectionDefinition = Nothing
                            _Application.CanvasProjectsConnectionDefinition = Nothing
                            '_Application.SpyronMasterConnectionDefinition = Nothing        ' -D6423
                            '_Application.SpyronProjectsConnectionDefinition = Nothing      ' -D6423
                            _Application.SystemWorkgroup = Nothing  ' D0540
                            _Application.Database.ResetError()  ' D2157
                            _Application.ResetDBChecks()
                        End If
                        ' D0499 ==
                        DebugInfo("Application Object was Loaded from Session")
                    Else
                        '-A1376 UserLocale = UserCulture()  ' D3198
                        _Application = New clsComparionCore         ' D0463
                        DebugInfo("Application Object was created")
                        LoadComparionCoreOptions(_Application)
                        ' D2289 ===
                        Dim sUID As String = SessVar(_SESS_UID)
                        If String.IsNullOrEmpty(sUID) Then sUID = GetCookie(_COOKIE_UID, "")
                        If String.IsNullOrEmpty(sUID) Then
                            sUID = GetRandomString(8, True, False)
                            Session.Add(_SESS_UID, sUID)
                            SetCookie(_COOKIE_UID, sUID, False, False)
                        End If
                        _Application.Options.SessionID = String.Format("{0}#{1}", sUID, Session.SessionID.Substring(0, 6)).ToLower
                        ' D2289 ==
                        _Application.CheckCanvasMasterDBDefaults()    ' D0475
                        SetLanguage(GetCookie(_COOKIE_LANGUAGE, ""))    ' D0125
                        isFirstRun = True   ' D0068 + D0521
                        _Revision = -1      ' D0473
                        '_Application.Options.ShowSilverlightShellOnLogon = GetCookie(_COOKIE_FORCE_SL, _Application.Options.ShowSilverlightShellOnLogon.ToString).ToLower <> "false"    ' D0778
                        ' D3698 ===
                        If _Application.Options.EvalSiteURL <> "" Then
                            Dim sURL As String = Request.Url.GetComponents(UriComponents.Host, UriFormat.Unescaped)
                            Dim sPort As String = Request.Url.GetComponents(UriComponents.Port, UriFormat.Unescaped)
                            Dim sScheme As String = Request.Url.GetComponents(UriComponents.Scheme, UriFormat.Unescaped)
                            _Application.Options.EvalSiteURL = _Application.Options.EvalSiteURL.Replace("%%server%%", sURL).Replace("%%port%%", sPort).Replace("%%scheme%%", sScheme)
                        End If
                        ' D3698 ==
                        ' D6328 ===
                        If Not _Application.isAuthorized AndAlso _Application.isAntiguaAuthorized Then ' D9402
                            If Session(_SESS_PRJ_ANON_NAME) IsNot Nothing Then
                                Dim CSProject As clsProject = CType(Session(_SESS_PRJ_ANON_NAME), clsProject)
                                If CSProject IsNot Nothing Then _Application.isRiskForceValue(CSProject.IsRisk)
                            End If
                        End If
                        ' D6328 ==
                        Session(_SESS_APP) = _Application   ' D0476
                    End If
                    If Not Baron_Path_to_EXE.Contains(":\") Then Baron_Path_to_EXE = Server.MapPath(Baron_Path_to_EXE) ' D4072 + D4073
                End If
                Return _Application
            End Get
            Set(ByVal value As clsComparionCore)
                _Application = value
            End Set
        End Property
        ' D0010 ==

        ' D0125 ===
        Public Function SetLanguage(ByVal sLangCode As String) As Boolean
            If Not App.LanguageCode Is Nothing Then
                If sLangCode.ToLower = "default" Then sLangCode = WebConfigOption(WebOptions._OPT_DEFLANG, _LANG_DEFCODE, True) ' D4672
                If sLangCode <> "" And sLangCode.ToLower <> App.LanguageCode.ToLower Then
                    If Not clsLanguageResource.LanguageByCode(sLangCode, App.Languages) Is Nothing Then ' D0466
                        App.LanguageCode = sLangCode
                        SetCookie(_COOKIE_LANGUAGE, sLangCode, False)
                        DebugInfo(String.Format("Set Language as '{0}'", sLangCode))    ' D0466
                        Return True
                    End If
                End If
            End If
            Return False
        End Function
        ' D0125 ==

        ' D0030 ===
        Public Function ResString(ByVal sResourceName As String, Optional ByVal fAsIsIfMissed As Boolean = False, Optional ByVal fParseTemplates As Boolean = True, Optional ByVal fCapitalized As Boolean = False) As String  ' D0232 + A1299
            Dim sRes As String = App.ResString(sResourceName, fAsIsIfMissed)
            If sResourceName.ToLower <> _TEMPL_APPNAME.ToLower AndAlso sResourceName.ToLower <> _TEMPL_APPNAME_PLAIN.ToLower Then  ' D3886
                If fParseTemplates AndAlso sRes.IndexOf("%") >= 0 Then sRes = ParseString(sRes) ' D0060 +  D0220 + D0232 + D0466 + D6736
            End If
            'A1299 ===
            If fCapitalized Then
                sRes = sRes.Substring(0, 1).ToUpper() + sRes.Substring(1).ToLower()
            End If
            'A1299 ==
            Return sRes
        End Function
        ' D0030 ==

        ' D3731 ===
        Public Function GetNodeTypeAndName(tNode As ECCore.clsNode, Optional tNameMaxLen As Integer = 40) As String
            Dim sNode As String = ""
            If tNode IsNot Nothing Then sNode = String.Format("{0} '{1}'", PrepareTask(CStr(IIf(tNode.IsAlternative, _TEMPL_ALTERNATIVE, _TEMPL_OBJECTIVE))), ShortString(tNode.NodeName, tNameMaxLen, True))
            Return sNode
        End Function
        ' D3731 ==

        ' D3757 ===
        Public Function GetNodeName(tNode As ECCore.clsNode, Optional tNameMaxLen As Integer = 45) As String
            Dim sNode As String = ""
            If tNode IsNot Nothing Then sNode = String.Format("{0}", ShortString(tNode.NodeName, tNameMaxLen, True))
            Return sNode
        End Function
        ' D3757 ==

        ' D7216 ===
        Public Function GetProjectTypeName(tPrj As clsProject) As String
            Dim sRes As String = ""
            If tPrj IsNot Nothing Then
                Select Case tPrj.ProjectStatus
                    Case ecProjectStatus.psActive
                        sRes = If(tPrj.IsRisk, "optIsRiskModel", "tblProject")
                        If tPrj.isRiskAssociatedModel Then
                            sRes = "lblAssociatedRiskModel"
                        Else
                            If tPrj.RiskionProjectType <> ProjectType.ptRegular AndAlso tPrj.IsRisk Then
                                sRes = String.Format("lbl_{0}", App.ActiveProject.RiskionProjectType)
                            End If
                        End If
                    Case Else
                        sRes = String.Format("lbl_{0}", App.ActiveProject.ProjectStatus)
                End Select
            End If
            If sRes <> "" Then
                sRes = ResString(sRes)
                Dim sModel As String = ResString("tblProject")
                If tPrj.ProjectStatus <> ecProjectStatus.psMasterProject AndAlso Not sRes.ToLower.Contains(sModel.ToLower) Then sRes = String.Format("{0} {1}", sRes, sModel)
            End If
            Return sRes
        End Function
        ' D7216 ==

        ' D0090 ===
        Public ReadOnly Property ApplicationName(Optional NameWithMark As Boolean = True) As String ' D3886
            Get
                Return App.ResString(CStr(IIf(App.isRiskEnabled, "titleApplicationNameRisk", "titleApplicationName")) + CStr(IIf(NameWithMark, "", "Plain")))    ' D1627 + D1634 + D2257 + D3886
            End Get
        End Property
        ' D0090 ==

        ' D4340 ===
        Public Function ActiveUserName() As String
            If App.isAuthorized Then Return App.ActiveUser.UserName Else Return ""
        End Function

        Public Function ActiveUserEmail(Optional fShowOnlyValid As Boolean = False) As String
            If App.isAuthorized AndAlso (Not fShowOnlyValid OrElse isValidEmail(App.ActiveUser.UserEmail)) Then Return App.ActiveUser.UserEmail Else Return ""
        End Function
        ' D4340 ==

        ' D6011 ===
        Public Function ActiveProjectName() As String
            Return If(App.HasActiveProject, String.Format("{1} (#{0}, access code: {2})", App.ProjectID, App.ActiveProject.ProjectName, App.ActiveProject.Passcode(App.ActiveProject.isImpact)), "- none -")
        End Function

        Public Function ActiveWorkgroupName() As String
            Return If(App.ActiveWorkgroup IsNot Nothing, App.ActiveWorkgroup.Name, "- none -")
        End Function
        ' D6011 ==

        ' D9402 ===
        Public Function AnonAntiguaProject() As clsProject
            Dim _PRJ As clsProject = Nothing
            If Session(_SESS_PRJ_ANON_NAME) IsNot Nothing Then
                _PRJ = CType(Session(_SESS_PRJ_ANON_NAME), clsProject)
            End If
            If _PRJ Is Nothing AndAlso App.isAntiguaAuthorized Then
                _PRJ = App.DBProjectByMeetingID(App.Antigua_MeetingID)
                If _PRJ IsNot Nothing Then
                    Dim tWkg As clsWorkgroup = App.DBWorkgroupByID(_PRJ.WorkgroupID, True, True)
                    If tWkg IsNot Nothing AndAlso tWkg.License.isValidLicense Then
                        App.ActiveWorkgroup = tWkg
                        _PRJ.IsRisk = App.isRiskEnabled
                    End If
                End If
            End If
            If _PRJ IsNot Nothing AndAlso _PRJ.IsRisk Then
                If _PRJ.MeetingIDImpact = App.Antigua_MeetingID Then
                    _PRJ.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
                End If
            End If
            If _PRJ IsNot Nothing AndAlso Session(_SESS_PRJ_ANON_NAME) Is Nothing Then Session(_SESS_PRJ_ANON_NAME) = _PRJ
            Return _PRJ
        End Function
        ' D9402 ==

        ' D6363 ===
        Public Function PrepareProjectForOpenPipe(ByRef tProject As clsProject) As Boolean
            Dim fUpdated As Boolean = False
            If tProject IsNot Nothing AndAlso (Not tProject.isOnline OrElse Not tProject.isPublic) Then
                Dim sError As String = ""
                If App.CanChangeProjectOnlineStatus(tProject, sError) Then
                    tProject.isOnline = True
                    tProject.isPublic = True
                    fUpdated = App.DBProjectUpdate(tProject, False, "Set on-line for login as other user")
                    Dim tLock As clsProjectLockInfo = App.DBProjectLockInfoRead(tProject.ID)
                    If tLock IsNot Nothing AndAlso tLock.LockStatus = ECLockStatus.lsLockForModify Then App.DBProjectLockInfoWrite(ECLockStatus.lsUnLocked, tLock, App.ActiveUser, Now)
                End If
            End If
            Return fUpdated
        End Function
        ' D6363 ==

        ' -D0849
        '' D0566 ===
        'Public ReadOnly Property CultureName() As String
        '    Get
        '        Const res_name As String = "_culture"
        '        Dim cult As String = ResString(res_name, True)
        '        If cult = res_name Then cult = "en"
        '        Return cult
        '    End Get
        'End Property
        '' D0566 ==

        ' D0089 ===
        'Public Function ParseTemplate(ByVal sMessage As String, ByVal sTemplateName As String, ByVal sTemplateValue As String, Optional ByVal fUseJSSafeString As Boolean = False, Optional CacheParsedValue As Boolean = True) As String    ' D0220 + D4667
        Public Function ParseTemplate(ByVal sMessage As String, ByVal sTemplateName As String, ByVal sTemplateValue As String, Optional ByVal fUseJSSafeString As Boolean = False) As String    ' D0220 + D4667 + D4761
            'If PreParsedTemplates Is Nothing Then PreParsedTemplates = New Dictionary(Of String, String) ' D4659
            'If CacheParsedValue AndAlso Not PreParsedTemplates.ContainsKey(sTemplateName) AndAlso Not sTemplateValue.Contains("%%") Then PreParsedTemplates.Add(sTemplateName, sTemplateValue) ' D4659 + D4667
            Return clsComparionCorePage.ParseTemplateCommon(sMessage, sTemplateName, sTemplateValue, fUseJSSafeString)
        End Function

        Shared Function ParseTemplateCommon(ByVal sMessage As String, ByVal sTemplateName As String, ByVal sTemplateValue As String, Optional ByVal fUseJSSafeString As Boolean = False) As String    ' D0220
            If fUseJSSafeString Then sTemplateValue = JS_SafeString(sTemplateValue) ' D0220
            If sMessage IsNot Nothing Then  ' D3858
                sMessage = sMessage.Replace("%%" + Capitalize(sTemplateName.Trim(CChar("%"))) + "%%", Capitalize(sTemplateValue))  ' D2427
                sMessage = sMessage.Replace(sTemplateName, sTemplateValue)
            End If
            Return sMessage
        End Function

        Public Function ParseURLAndPathTemplates(sMessage As String, Optional ByVal fUseJSSafeString As Boolean = False) As String
            Dim sRes As String = sMessage
            sRes = ParseTemplate(sRes, _TEMPL_ROOT_URI, _URL_ROOT, fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_ROOT_PATH, _FILE_ROOT, fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_ROOT_IMAGES, ImagePath, fUseJSSafeString)
            Return sRes
        End Function

        Public Function ParseAllTemplates(ByVal sMessage As String, ByVal tUser As clsApplicationUser, ByVal tProject As clsProject, Optional ByVal fUseJSSafeString As Boolean = False, Optional ByVal fReplaceOnlyCommon As Boolean = False) As String   ' D0220 + D0312
            If String.IsNullOrEmpty(sMessage) OrElse sMessage.IndexOf("%%") < 0 Then Return sMessage ' D0220

            '' D4659 ===
            ' If PreParsedTemplates IsNot Nothing AndAlso PreParsedTemplates.Count > 0 Then
            '    sMessage = ParseStringTemplates(sMessage, PreParsedTemplates)
            '    If sMessage.IndexOf("%%") <0 Then Return sMessage
            'End If
            '' D4659 ==

            ' D6736 ===
            If tProject Is Nothing AndAlso Not App.isAuthorized AndAlso App.isAntiguaAuthorized Then tProject = AnonAntiguaProject()
            If tUser Is Nothing AndAlso App.isAntiguaAuthorized Then
                tUser = New clsApplicationUser()
                With tUser
                    .UserEmail = App.Antigua_UserEmail
                    .UserName = App.Antigua_UserName
                End With
            End If
            ' D6736 ==

            Dim sRes As String = ParseTemplate(sMessage, _TEMPL_APPNAME, ApplicationName, fUseJSSafeString) ' D0090
            ' D3886 ===
            sRes = ParseTemplate(sRes, _TEMPL_APPNAME_PLAIN, ApplicationName(False), fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_APPNAME_TEAMTIME, App.ResString("titleApplicationTeamTime"), fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_APPNAME_SURVEY, App.ResString("titleApplicationSurvey"), fUseJSSafeString)
            ' D3886 ==
            ' D6308 ===
            sRes = ParseTemplate(sRes, _TEMPL_PROJECTS, App.ResString("templ_Projects"), fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_PROJECT, App.ResString("templ_Project"), fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_MODELS, App.ResString("templ_Models"), fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_MODEL, App.ResString("templ_Model"), fUseJSSafeString)
            ' D6308 ==
            sRes = ParseTemplate(sRes, _TEMPL_SERVICE_EMAIL, WebOptions.SystemEmail, fUseJSSafeString) ' D0220 + D0315 + D1152

            'Dim sGroupName As String = ""  ' -D1830
            Dim sUserName As String = ""
            Dim sUserEmail As String = ""
            Dim sUserPsw As String = "*********" ' D0220 + D2216
            If Not tUser Is Nothing Then
                sUserEmail = tUser.UserEmail
                sUserName = tUser.UserName
                ' D0220 ===
                'sUserPsw = tUser.UserPassword  ' -D2216

                ' -D1830
                'If Not App.ActiveWorkgroup Is Nothing Then
                '    If Not App.ActiveRoleGroup Is Nothing Then sGroupName = App.ActiveRoleGroup.Name ' D0466
                'End If
                ' D0220 ==
            End If

            ' D0270 ===
            Dim sOwnerName As String = ""
            Dim sOwnerEmail As String = ""
            If Not App.ActiveUser Is Nothing Then
                sOwnerName = App.ActiveUser.UserName
                sOwnerEmail = App.ActiveUser.UserEmail
            End If
            ' D0270 ==

            Dim sWkgName As String = "" ' D3401
            If App.ActiveWorkgroup IsNot Nothing Then sWkgName = App.ActiveWorkgroup.Name ' D3401

            '' D0121 ===
            'sRes = ParseTemplate(sRes, _TEMPL_ROOT_URI, _URL_ROOT, fUseJSSafeString)
            'sRes = ParseTemplate(sRes, _TEMPL_ROOT_PATH, _FILE_ROOT, fUseJSSafeString)
            'sRes = ParseTemplate(sRes, _TEMPL_ROOT_IMAGES, ThemePath + _FILE_IMAGES, fUseJSSafeString)
            '' D0121 ==
            sRes = ParseURLAndPathTemplates(sRes, fUseJSSafeString) ' D1550

            ' D0220 ===
            Dim sPrjName As String = ""
            Dim sPrjPasscode As String = ""
            Dim sMeetingID As String = ""   ' D0388
            Dim sRiskModel As String = ""   ' D1804
            Dim sRiskModels As String = ""  ' D3001
            If tProject IsNot Nothing Then
                'tProject = App.ActiveProject   ' -D1630: Who did this?
                sPrjName = SafeFormString(tProject.ProjectName) 'L0399
                sPrjPasscode = SafeFormString(tProject.Passcode) 'L0399
                sMeetingID = clsMeetingID.AsString(tProject.MeetingID) ' D0420
                ' D1640 ===
                If sUserEmail <> "" AndAlso tProject.isValidDBVersion Then
                    Dim tPrjuser As clsUser = tProject.ProjectManager.GetUserByEMail(sUserEmail)
                    If tPrjuser IsNot Nothing AndAlso Not String.IsNullOrEmpty(tPrjuser.UserName) Then sUserName = tPrjuser.UserName
                    If App.ActiveUser IsNot Nothing Then
                        tPrjuser = tProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEmail)
                        If tPrjuser IsNot Nothing AndAlso Not String.IsNullOrEmpty(tPrjuser.UserName) Then sOwnerName = tPrjuser.UserName
                    End If
                    If App.isRiskEnabled Then
                        sRiskModel = App.ResString(CStr(IIf(tProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact, "lblImpact", "lblLikelihood")))
                        sRiskModels = App.ResString(CStr(IIf(tProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact, "lblImpacts", "lblLikelihoods")))   ' D3001
                    Else
                        sRiskModel = App.ResString("lblProject")
                        sRiskModels = App.ResString("lblProjects")  ' D3001
                    End If
                End If
                ' D1640 ==
                sRes = ParseTemplate(sRes, _TPL_RISK_LOSS, CStr(IIf(tProject.isOpportunityModel, App.ResString("tpl_gain"), App.ResString("tpl_loss")))) 'A1377
                sRes = ParseTemplate(sRes, _TPL_RISK_GAIN, CStr(IIf(tProject.isOpportunityModel, App.ResString("tpl_gain"), App.ResString("tpl_loss")))) 'A1377
            End If

            If sUserPsw = "" Then sUserPsw = App.ResString("lblDummyBlankPassword") ' D0306

            If Not fReplaceOnlyCommon Then  ' D0312
                'sRes = ParseTemplate(sRes, _TEMPL_GROUPNAME, sGroupName, fUseJSSafeString)  ' D0220 -D1830
                sRes = ParseTemplate(sRes, _TEMPL_USEREMAIL, sUserEmail, fUseJSSafeString)
                sRes = ParseTemplate(sRes, _TEMPL_USERNAME, sUserName, fUseJSSafeString)
                sRes = ParseTemplate(sRes, _TEMPL_USER_FIRSTNAME, GetUserFirstName(sUserName), fUseJSSafeString)  ' D2279
                sRes = ParseTemplate(sRes, _TEMPL_USERPSW, sUserPsw, fUseJSSafeString)  ' D0220
            End If

            ' D0270 ===
            sRes = ParseTemplate(sRes, _TEMPL_OWNERNAME, sOwnerName, fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_OWNEREMAIL, sOwnerEmail, fUseJSSafeString)
            ' D0270 ==
            sRes = ParseTemplate(sRes, _TEMPL_WORKGROUP, sWkgName, fUseJSSafeString)    ' D3401

            sRes = ParseTemplate(sRes, _TEMPL_PRJNAME, sPrjName, fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_PRJPASSCODE, sPrjPasscode, fUseJSSafeString)

            sRes = ParseTemplate(sRes, _TEMPL_RISKMODEL, sRiskModel, fUseJSSafeString)  ' D1804
            sRes = ParseTemplate(sRes, _TEMPL_RISKMODELS, sRiskModels, fUseJSSafeString)    ' D3001

            sRes = ParseTemplate(sRes, _TEMPL_MEETING_ID, sMeetingID, fUseJSSafeString) ' D0388

            ' D3494 ===
            Dim sStartURL As String = ApplicationURL(False, False) + _URL_ROOT  ' D0445 + D3308
            Dim sEvalAnytimeURL As String = ApplicationURL(True, False)
            Dim sEvalTTURL As String = ApplicationURL(True, True)
            If Not sEvalAnytimeURL.EndsWith("/") Then sEvalAnytimeURL += "/"    ' D4925
            If Not sEvalTTURL.EndsWith("/") Then sEvalTTURL += "/"              ' D4925
            ' D3494 ==
            sRes = ParseTemplate(sRes, _TEMPL_URL_APP, sStartURL, fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_URL_APP_TT, sEvalTTURL, fUseJSSafeString) ' D3494
            sRes = ParseTemplate(sRes, _TEMPL_URL_MEETINGID, String.Format("{0}?{1}={2}", sEvalTTURL, _PARAM_MEETING_ID, sMeetingID), fUseJSSafeString) ' D1352 + D6069
            If Not fReplaceOnlyCommon AndAlso tUser IsNot Nothing Then  ' D0312 + A1467
                Dim tUW As clsUserWorkgroup = Nothing   ' D4622
                If App.ActiveWorkgroup IsNot Nothing Then
                    If App.ActiveUser IsNot Nothing AndAlso App.ActiveUser.UserID = tUser.UserID Then
                        tUW = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID, App.UserWorkgroups)
                    Else
                        tUW = App.DBUserWorkgroupByUserIDWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID) ' D4616 + D4622
                    End If
                End If

                ' D7432 ===
                If isSSO_Only() Then
                    Dim sPasscodeURL As String = If(tProject Is Nothing, sStartURL, URLWithParams(sStartURL, _PARAM_PASSCODE + "="))
                    For Each sTpl As String In _TEMPL_LIST_HIDE_URLS
                        If sRes.ToLower.Contains(sTpl.ToLower) Then sRes = ParseTemplate(sRes, sTpl, sPasscodeURL + If(tProject Is Nothing, "", If(sTpl = _TEMPL_URL_EVALUATE_IMPACT, tProject.PasscodeImpact, tProject.PasscodeLikelihood)), fUseJSSafeString)
                    Next
                End If
                ' D7432 ==

                ' D1672 + D1734 + D3308 + D3494 ===
                If sRes.ToLower.Contains(_TEMPL_URL_LOGIN.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_LOGIN, CreateLogonURL(tUser, Nothing, "", sStartURL, , tUW), fUseJSSafeString)
                If sRes.ToLower.Contains(_TEMPL_URL_EVALUATE.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE, CreateLogonURL(tUser, tProject, "pipe=yes" + App.Options.EvalURLPostfix, sEvalAnytimeURL, , tUW), fUseJSSafeString) ' D3150
                If sRes.ToLower.Contains(_TEMPL_URL_EVALUATE_TT.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_TT, CreateLogonURL(tUser, tProject, "TTOnly=1" + App.Options.EvalURLPostfix, sEvalTTURL, , tUW), fUseJSSafeString) ' D0655
                If App.isRiskEnabled AndAlso sRes.ToLower.Contains(_TEMPL_URL_EVALUATE_LIKELIHOOD.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_LIKELIHOOD, CreateLogonURL(tUser, tProject, "pipe=yes" + App.Options.EvalURLPostfix, sEvalAnytimeURL, tProject.PasscodeLikelihood, tUW), fUseJSSafeString) ' D2467 + D3150
                If App.isRiskEnabled AndAlso sRes.ToLower.Contains(_TEMPL_URL_EVALUATE_IMPACT.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_IMPACT, CreateLogonURL(tUser, tProject, "pipe=yes" + App.Options.EvalURLPostfix, sEvalAnytimeURL, tProject.PasscodeImpact, tUW), fUseJSSafeString) ' D2467 + D3150
                If App.isRiskEnabled AndAlso sRes.ToLower.Contains(_TEMPL_URL_EVALUATE_CONTROLS.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_CONTROLS, CreateLogonURL(tUser, tProject, "pipe=yes&mode=riskcontrols" + App.Options.EvalURLPostfix, sEvalAnytimeURL, tProject.PasscodeLikelihood, tUW), fUseJSSafeString) ' D3769
                ' D1060 + D1830 + D3308 ===
                If sRes.ToLower.Contains(_TEMPL_URL_EVALUATE_ANONYM.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_ANONYM, CreateEvaluateSignupURL(tProject, True, "", "", sStartURL), fUseJSSafeString)
                If sRes.ToLower.Contains(_TEMPL_URL_EVALUATE_SIGNUP.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_SIGNUP, CreateEvaluateSignupURL(tProject, False, "", "", sStartURL), fUseJSSafeString)
                If sRes.ToLower.Contains(_TEMPL_URL_RESETPSW.ToLower) AndAlso tUser IsNot Nothing Then sRes = ParseTemplate(sRes, _TEMPL_URL_RESETPSW, CreateResetPswURL(tUser, "", sStartURL), fUseJSSafeString) ' D2216
                If sRes.ToLower.Contains(_TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY, CreateEvaluateSignupURL(tProject, False, "e", "", sStartURL), fUseJSSafeString)
                If sRes.ToLower.Contains(_TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW, CreateEvaluateSignupURL(tProject, False, "ep", "", sStartURL), fUseJSSafeString)
                If sRes.ToLower.Contains(_TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY, CreateEvaluateSignupURL(tProject, False, "n", "", sStartURL), fUseJSSafeString)
                ' D1060 + D1830 ==
            End If
            ' D0220 ==

            If sRes.ToLower.Contains(_TEMPL_IP.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_IP, Request.UserHostAddress, fUseJSSafeString) ' D2413
            If sRes.ToLower.Contains(_TEMPL_PSWLOCK_TIMEOUT.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_PSWLOCK_TIMEOUT, _DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT.ToString, fUseJSSafeString)    ' D6062
            If sRes.ToLower.Contains(_TEMPL_PSWLOCK_ATTEMPTS.ToLower) Then sRes = ParseTemplate(sRes, _TEMPL_PSWLOCK_ATTEMPTS, _DEF_PASSWORD_ATTEMPTS.ToString, fUseJSSafeString)       ' D6350

            Dim _TPL_ALT_OBJ As String() = {_TEMPL_ALTERNATIVES, _TEMPL_ALTERNATIVE, _TEMPL_OBJECTIVES, _TEMPL_OBJECTIVE}
            ' D6080 ===
            If App.isRiskEnabled Then
                _TPL_ALT_OBJ = _TPL_ALT_OBJ.Concat({_TEMPL_ALTERNATIVES_LIKELIHOOD, _TEMPL_ALTERNATIVES_IMPACT, _TEMPL_OBJECTIVE_LIKELIHOOD, _TEMPL_OBJECTIVE_IMPACT, _TEMPL_OBJECTIVES_LIKELIHOOD, _TEMPL_OBJECTIVES_IMPACT}).ToArray
            End If
            ' D6080 ==

            If sRes.Contains(_TEMPL_PAGE_PREFIX) Then
                Dim idx As Integer = 0
                While sRes.IndexOf(_TEMPL_PAGE_PREFIX, idx) >= 0
                    Dim s As Integer = sRes.IndexOf(_TEMPL_PAGE_PREFIX)
                    idx += 1
                    If s >= 0 Then
                        Dim e As Integer = sRes.IndexOf("%%", s + 2)
                        Dim n As Integer = sRes.IndexOf("-", s + 2)
                        If e > 0 AndAlso n > 0 AndAlso n < e Then
                            Dim tName As String = sRes.Substring(s, n - s + 1)
                            Dim sPg As String = sRes.Substring(n + 1, e - n - 1)
                            Dim tmppgid As Integer
                            If Integer.TryParse(sPg, tmppgid) Then
                                Dim tmppg As clsPageAction = PageByID(tmppgid)
                                If tmppg IsNot Nothing Then
                                    Dim sRepl As String = ""
                                    Select Case tName.ToLower
                                        Case _TEMPL_PAGE_NAME_PREFIX
                                            sRepl = PageMenuItem(tmppgid)
                                        Case _TEMPL_PAGE_TITLE_PREFIX
                                            sRepl = PageTitle(tmppgid)
                                        Case _TEMPL_PAGE_URL_PREFIX
                                            sRepl = PageURL(tmppgid)
                                        Case _TEMPL_PAGE_LINK_PREFIX
                                            sRepl = HTMLTextLink(PageURL(tmppgid), PageMenuItem(tmppgid))
                                    End Select
                                    If sRepl <> "" Then
                                        sRes = sRes.Substring(0, s) + sRepl + sRes.Substring(e + 2)
                                        e = s + sRepl.Length - 2
                                    End If
                                End If
                            End If

                            idx = e + 2
                        End If
                    End If
                End While
            End If

            ' D2427 ===
            'If sRes.Contains("%%") AndAlso App.isRiskEnabled AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.WordingTemplates IsNot Nothing Then
            If sRes.Contains("%%") AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.WordingTemplates IsNot Nothing Then    ' D2442
                'A0954 ===
                Dim lres As String = sRes.ToLower
                If tProject IsNot Nothing AndAlso tProject.ProjectManager.PipeParameters.ProjectType = ProjectType.ptOpportunities AndAlso   ' D7537
                   (lres.Contains(_TPL_RISK_VULNERABILITIES) OrElse lres.Contains(_TPL_RISK_VULNERABILITY) OrElse
                    lres.Contains(_TPL_RISK_EVENT) OrElse lres.Contains(_TPL_RISK_EVENTS) OrElse
                    lres.Contains(_TPL_RISK_CONTROL) OrElse lres.Contains(_TPL_RISK_CONTROLS) OrElse
                    lres.Contains(_TPL_RISK_RISK) OrElse lres.Contains(_TPL_RISK_RISKS)) Then
                    sRes = ParseTemplate(sRes, _TPL_RISK_VULNERABILITY, App.ResString("templ_vulnerability_for_opportunity"), fUseJSSafeString)
                    sRes = ParseTemplate(sRes, _TPL_RISK_VULNERABILITIES, App.ResString("templ_vulnerabilities_for_opportunity"), fUseJSSafeString)
                    sRes = ParseTemplate(sRes, _TPL_RISK_EVENT, App.ResString("templ_event_for_opportunity"), fUseJSSafeString)
                    sRes = ParseTemplate(sRes, _TPL_RISK_EVENTS, App.ResString("templ_events_for_opportunity"), fUseJSSafeString)
                    sRes = ParseTemplate(sRes, _TPL_RISK_CONTROL, App.ResString("templ_control_for_opportunity"), fUseJSSafeString)
                    sRes = ParseTemplate(sRes, _TPL_RISK_CONTROLS, App.ResString("templ_controls_for_opportunity"), fUseJSSafeString)
                    sRes = ParseTemplate(sRes, _TPL_RISK_RISK, App.ResString("templ_risk_for_opportunity"), fUseJSSafeString)
                    sRes = ParseTemplate(sRes, _TPL_RISK_RISKS, App.ResString("templ_risks_for_opportunity"), fUseJSSafeString)

                End If
                'A0954 ==
                For Each sName As String In App.ActiveWorkgroup.WordingTemplates.Keys
                    If sRes.ToLower.Contains(sName.ToLower) AndAlso sName.ToLower <> _TEMPL_OBJECTIVES AndAlso sName.ToLower <> _TEMPL_ALTERNATIVES Then   ' D4979
                        ' D4575 ===
                        Dim sTpl As String = App.ActiveWorkgroup.WordingTemplates(sName)
                        If tProject IsNot Nothing AndAlso tProject.WordingTemplates IsNot Nothing AndAlso tProject.WordingTemplates.ContainsKey(sName) AndAlso tProject.WordingTemplates(sName).InUse Then    ' D7537
                            Dim sPrjTpl As String = tProject.WordingTemplates(sName).Value.Trim     ' D7537
                            If sPrjTpl <> "" Then sTpl = sPrjTpl
                        End If
                        If sTpl <> "" Then sRes = ParseTemplate(sRes, sName, sTpl, fUseJSSafeString)
                        ' D4575 ==
                    End If
                Next
            End If
            ' D2427 ==

            ' D2368 ===
            If sRes.Contains("%%") Then
                For Each sTempl As String In _TPL_ALT_OBJ
                    If sRes.ToLower.Contains(sTempl.ToLower) Then
                        sRes = clsComparionCorePage.PrepareTaskCommon(App, sRes,,, tProject)    ' D6080 + D6736
                        Exit For
                    End If
                Next
            End If
            ' D2368 ==

            Return sRes
        End Function
        ' D0089 ==

        'A1203 ===
        Public Function ParseString(sTxt As String) As String
            Return ParseAllTemplates(sTxt, App.ActiveUser, App.ActiveProject)
        End Function
        'A1203 ==

        ' D0947 ===
        Public Sub SendTeamTimeInvitationSummaryEmail(ByVal sFrom As String, ByVal sTo As String, ByVal sCount As String, ByVal sList As String)    ' D1177
            Dim sError As String = ""
            Dim sResume As String = ParseAllTemplates(ResString("bodyTeamTimeInvitationSummary"), App.ActiveUser, App.ActiveProject)
            sResume = ParseTemplate(sResume, "%%count%%", sCount)
            sResume = ParseTemplate(sResume, "%%list%%", sList)
            SendMail(sFrom, sTo, ParseAllTemplates(ResString("subjTeamTimeInvitationSummary"), App.ActiveUser, App.ActiveProject), sResume, sError, "", False, WebOptions.SMTPSSL)  ' D1177 + Â1187
            App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfProject, App.ProjectID, "Send TT invitation summary e-mail", sError)  ' D1187
        End Sub
        ' D0947 ==

        Public Sub SendAnyTimeInvitationSummaryEmail(ByVal sFrom As String, ByVal sTo As String, ByVal sCount As String, ByVal sList As String)   ' D1187
            Dim sError As String = ""
            Dim sResume As String = ParseAllTemplates(ResString("bodyAnyTimeInvitationSummary"), App.ActiveUser, App.ActiveProject)
            sResume = ParseTemplate(sResume, "%%count%%", sCount)
            sResume = ParseTemplate(sResume, "%%list%%", sList)
            SendMail(sFrom, sTo, ParseAllTemplates(ResString("subjAnyTimeInvitationSummary"), App.ActiveUser, App.ActiveProject), sResume, sError, "", False, WebOptions.SMTPSSL)
            App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfProject, App.ProjectID, "Send invitation summary e-mail", sError) ' D1187
        End Sub

        ' -D6087 // use DBUserUnlock with a new random psw
        '' D0276 ===
        'Public Sub ResetUserPassword(ByRef tUser As clsApplicationUser, ByVal fSendEmailNotify As Boolean)
        '    If Not tUser Is Nothing Then
        '        tUser.UserPassword = GetRandomString(_DEF_PASSWORD_LENGTH, True, True) ' D0286
        '        App.DBUserUpdate(tUser, False, "Reset user password")   ' D0471
        '        If fSendEmailNotify Then
        '            Dim sError As String = ""
        '            Dim fSent As Boolean = SendMail(WebOptions.SystemEmail, tUser.UserEmail, ParseAllTemplates(ResString("subjReminder", False, False), tUser, Nothing), ParseAllTemplates(ResString("bodyReminder", False, False), tUser, Nothing), sError, "", False, WebOptions.SMTPSSL)   ' D0315 + D0758 + D1152
        '            App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfUser, tUser.UserID, "Send password to user", sError)  ' D0496
        '        End If
        '    End If
        'End Sub
        '' D0276 ==

        ' D0220 ===
        Public Function CreateLogonURL(ByVal tUser As clsApplicationUser, ByVal tProject As clsProject, ByVal sOtherParams As String, ByVal sPagePath As String, Optional sPasscode As String = Nothing, Optional tUW As clsUserWorkgroup = Nothing, Optional fAllowAdmins As Boolean = False) As String  ' D2467 + D4616
            Dim sURL As String = ""
            If isSSO_Only() Then    ' D7432
                sURL = ResString("msgNoUserLinkSSO")  ' D7432
            Else
                If Not tUser Is Nothing Then
                    If tUser Is App.ActiveUser Then fAllowAdmins = True ' D4797 // allow to see own links for admins
                    ' D4616 ===
                    Dim tAdminGrpID As Integer = If(App.ActiveWorkgroup Is Nothing, -1, App.ActiveWorkgroup.RoleGroupID(ecRoleGroupType.gtAdministrator))  ' D7209
                    If Not fAllowAdmins AndAlso tUW Is Nothing AndAlso App.ActiveWorkgroup IsNot Nothing Then tUW = App.DBUserWorkgroupByUserIDWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID) ' D4622
                    If (fAllowAdmins OrElse (tUW IsNot Nothing AndAlso tUW.RoleGroupID <> tAdminGrpID AndAlso Not tUser.CannotBeDeleted)) Then  ' D4620
                        ' D4616 ==
                        sURL = String.Format("{0}={1}&{2}={3}", _PARAM_EMAIL, HttpUtility.UrlEncode(tUser.UserEmail), _PARAM_PASSWORD, HttpUtility.UrlEncode(tUser.UserPassword))   ' D1465
                        If Not tProject Is Nothing Then
                            sURL += String.Format("&{0}={1}", _PARAM_PASSCODE, HttpUtility.UrlEncode(CStr(IIf(String.IsNullOrEmpty(sPasscode), tProject.Passcode, sPasscode))))   ' D1465 + D1529 + D2467
                        End If
                        If sOtherParams <> "" Then
                            If sURL <> "" AndAlso Not sOtherParams.StartsWith("&") Then sURL += "&"
                            sURL += sOtherParams
                        End If
                        ' D0896 ===
                        'Return String.Format("{0}?key={1}", sPagePath, EncodeURL(sURL, App.InstanceID)) ' D0826
                        sURL = EncodeURL(sURL, App.DatabaseID)
                        Dim sLink As String = CStr(IIf(sPagePath.Contains("?"), "&", "?"))  ' D1529
                        If App.Options.UseTinyURL Then
                            ' D0899 ===
                            Dim PID As Integer = -1
                            Dim UID As Integer = -1
                            If tProject IsNot Nothing Then PID = tProject.ID
                            If tUser IsNot Nothing Then UID = tUser.UserID
                            sURL = String.Format("{0}{3}{2}={1}", sPagePath, App.CreateTinyURL(sURL, PID, UID), _PARAMS_TINYURL(0), sLink)  ' D1529
                            ' D0899 ==
                        Else
                            sURL = String.Format("{0}{3}{2}={1}", sPagePath, sURL, _PARAMS_KEY(0), sLink)  ' D1529
                        End If
                    End If
                End If
            End If
            Return sURL
            ' D0896 ==
        End Function
        ' D0220 ==

        ' D1060 ===
        Public Function CreateEvaluateSignupURL(ByVal tProject As clsProject, ByVal fIsAnonymous As Boolean, ByVal sSignupMode As String, ByVal sOtherParams As String, ByVal sPagePath As String) As String
            Dim sURL As String = ""
            If isSSO_Only() Then    ' D7432
                sURL = ResString("msgNoSignupSSO")  ' D7432
            Else
                If tProject Is Nothing Then Return sURL
                sURL += String.Format("&{0}=1&{1}={2}&{3}={4}" + If(fIsAnonymous, "", "&{5}={6}"), _PARAMS_SIGNUP(0), _PARAMS_ANONYMOUS_SIGNUP(0), IIf(fIsAnonymous, "1", "0"), _PARAM_PASSCODE, HttpUtility.UrlEncode(App.ActiveProject.Passcode), _PARAMS_SIGNUP_MODE(0), sSignupMode)   ' D1465 + D4598
            If sOtherParams <> "" Then
                If sURL <> "" AndAlso Not sOtherParams.StartsWith("&") Then sURL += "&" ' D5077
                sURL += sOtherParams
            End If
            sURL = EncodeURL(sURL, App.DatabaseID)
                If App.Options.UseTinyURL Then
                    Dim PID As Integer = -1
                    If tProject IsNot Nothing Then PID = tProject.ID
                    sURL = String.Format("{0}?{2}={1}", sPagePath, App.CreateTinyURL(sURL, PID, -1), _PARAMS_TINYURL(0))
                Else
                    sURL = String.Format("{0}?{2}={1}", sPagePath, sURL, _PARAMS_KEY(0))
                End If
            End If
            Return sURL
        End Function
        ' D1060 ==

        ' D2216 ===
        Public Function CreateResetPswURL(tUser As clsApplicationUser, ByVal sOtherParams As String, ByVal sPagePath As String) As String
            Dim sURL As String = ""
            If isSSO() Then
                sURL = ResString("msgNoResetPswSSO") ' 6552
            Else
                If tUser IsNot Nothing Then
                    If App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, tUser.UserID) OrElse tUser.CannotBeDeleted Then ' D4733
                        sURL = ResString("msgNoResetPsw4Admin") ' D4733
                    Else
                        'App.DBTinyURLDelete(-1, -2, tUser.UserID)  ' -D4936 // is it really required? No need to wipeout all existing links since user can never call psw reset
                        sURL = String.Format("{0}=resetpsw&ue={1}&up={2}&t={3}", _PARAM_ACTION, tUser.UserEmail, tUser.UserPassword, Now.Ticks)
                        sURL = EncodeURL(sURL, App.DatabaseID)
                        sURL = String.Format("{0}Password.aspx?{2}={1}", sPagePath, App.CreateTinyURL(sURL, -2, tUser.UserID), _PARAMS_TINYURL(0))
                    End If
                End If
            End If
            Return sURL
        End Function
        ' D2216 ==

        ' D0090 ===
        Public ReadOnly Property ThemePath() As String
            Get
                Return String.Format("{0}{1}/", _URL_THEMES, IIf(Page.Theme = "", _THEME_EC09, Page.Theme))    ' D1319
            End Get
        End Property
        ' D0090 ==

        ' D0352 ===
        Public Property ShowNavigation() As Boolean
            Get
                Return _fShowNavigation
            End Get
            Set(ByVal value As Boolean)
                _fShowNavigation = value
            End Set
        End Property
        ' D0352 ==

        ' D4220 ===
        Public Function ShowChatSupport() As Boolean
            Return (WebOptions.ShowChatSupport(Request Is Nothing OrElse Not Request.IsLocal) AndAlso Not isSLTheme() AndAlso App.isAuthorized)
        End Function
        ' D4220 ==

        ' D6508 ===
        Public Function LoadOnlineHelpOnDemand() As Boolean ' D6510
            Return Not _OPT_HELP_ROBOHELP AndAlso _OPT_HELP_LOAD_ONDEMAND   ' D6510
        End Function
        ' D6508 ==

        ' D5001 ===
        Public Property ShowLandingPages(App As clsComparionCore, tUserID As Integer) As Boolean
            Get
                'Dim retVal As Boolean = False
                'Dim tOption As clsExtra = App.DBExtraRead(clsExtra.Params2Extra(tUserID, ecExtraType.SilverLight, ecExtraProperty.ShowIntroductionPage))
                'If tOption Is Nothing Then retVal = False Else retVal = CStr(tOption.Value) = "1" ' D3622 + D4998
                Dim retVal As Boolean = False
                Str2Bool(GetCookie(_COOKIE_SHOW_LANDING, "1"), retVal)
                Return retVal
            End Get
            Set(value As Boolean)
                'App.DBExtraWrite(clsExtra.Params2Extra(tUserID, ecExtraType.SilverLight, ecExtraProperty.ShowIntroductionPage, IIf(value, 1, 0)))   ' D3622
                SetCookie(_COOKIE_SHOW_LANDING, Bool2Num(value).ToString, False, False)
            End Set
        End Property
        ' D5001 ==

        ' D6034 ===
        Public Property ShowKnownIssues(App As clsComparionCore) As Boolean
            Get
                Dim retVal As Boolean = False
                Str2Bool(GetCookie(_COOKIE_SHOW_KNOWNISSUES, "1"), retVal)
                Return retVal
            End Get
            Set(value As Boolean)
                SetCookie(_COOKIE_SHOW_KNOWNISSUES, Bool2Num(value).ToString, False, False)
            End Set
        End Property
        ' D6034 ==

        ' D4581 ===
        Public Function ShowLanguages() As Boolean
            Return WebOptions.ShowLanguages()
        End Function
        ' D4581 ==

        ' D0426 ===
        Public Function FileLink(ByVal sLink As String, ByVal sTitle As String) As String
            Return HTMLFileLink(sLink, Server.MapPath(sLink), sTitle)
        End Function
        ' D0426 ==

        ' D0800 ===
        Public ReadOnly Property GetIncFile(ByVal sFilename As String) As String
            Get
                Dim sPath As String = _FILE_DATA_INC + App.LanguageCode + "\" + sFilename   ' D2325
                If My.Computer.FileSystem.FileExists(sPath) Then Return sPath
                sPath = _FILE_DATA_INC + _LANG_DEFCODE + "\" + sFilename    ' D2325
                If My.Computer.FileSystem.FileExists(sPath) Then Return sPath Else Return _FILE_DATA_INC + sFilename
            End Get
        End Property
        ' D0800 ==

        ' D2325 ===
        Public ReadOnly Property GetWelcomeThankYouIncFile(fIsThankYou As Boolean, fIsImpact As Boolean, fIsOpportunity As Boolean, fIsRiskWithControls As Boolean) As String   ' D3326 + D4293
            Get
                Dim sName As String = ""
                If App.isRiskEnabled Then
                    ' D2537 ===
                    If fIsRiskWithControls Then  ' D4285 + D4293
                        If fIsOpportunity Then  ' D3327
                            sName = CStr(IIf(fIsThankYou, _FILE_TEMPL_THANKYOU_RISK_OPPORTUNITY, _FILE_TEMPL_WELCOME_RISK_OPPORTUNITY)) ' D3327
                        Else
                            sName = CStr(IIf(fIsThankYou, _FILE_TEMPL_THANKYOU_RISK, _FILE_TEMPL_WELCOME_RISK))
                        End If
                    Else
                        ' D2537 ==
                        If fIsThankYou Then
                            If fIsOpportunity Then  ' D3326
                                sName = CStr(IIf(fIsImpact, _FILE_TEMPL_THANKYOU_IMPACT_OPPORTUNITY, _FILE_TEMPL_THANKYOU_LIKELIHOOD_OPPORTUNITY))  ' D3326
                            Else
                                sName = CStr(IIf(fIsImpact, _FILE_TEMPL_THANKYOU_IMPACT, _FILE_TEMPL_THANKYOU_LIKELIHOOD))
                            End If
                        Else
                            If fIsOpportunity Then  ' D3326
                                sName = CStr(IIf(fIsImpact, _FILE_TEMPL_WELCOME_IMPACT_OPPORTUNITY, _FILE_TEMPL_WELCOME_LIKELIHOOD_OPPORTUNITY))    ' D3326
                            Else
                                sName = CStr(IIf(fIsImpact, _FILE_TEMPL_WELCOME_IMPACT, _FILE_TEMPL_WELCOME_LIKELIHOOD))
                            End If
                        End If
                    End If
                Else
                    If fIsOpportunity Then  ' D3326
                        sName = CStr(IIf(fIsThankYou, _FILE_TEMPL_THANKYOU_OPPORTUNITY, _FILE_TEMPL_WELCOME_EVALUATE_OPPORTUNITY))  ' D3326
                    Else
                        sName = CStr(IIf(fIsThankYou, _FILE_TEMPL_THANKYOU, _FILE_TEMPL_WELCOME_EVALUATE))
                    End If
                End If
                sName = GetIncFile(sName)
                If App.isRiskEnabled AndAlso Not My.Computer.FileSystem.FileExists(sName) Then sName = GetIncFile(CStr(IIf(fIsThankYou, _FILE_TEMPL_THANKYOU, _FILE_TEMPL_WELCOME_EVALUATE)))
                Return sName
            End Get
        End Property
        ' D2325 ==

        ' D4164 ===
        Public Function GetNextProject(tProject As clsProject) As clsProject
            Dim tPrj As clsProject = Nothing
            If tProject IsNot Nothing Then
                With tProject.ProjectManager.Parameters
                    If .EvalOpenNextProjectAtFinish AndAlso .EvalNextProjectPasscodeAtFinish <> "" Then
                        Dim sPasscode As String = .EvalNextProjectPasscodeAtFinish.ToLower.Replace(clsProjectParametersWithDefaults.OPT_PASSCODE_RISK_CONTROLS_PIPE_PREFIX.ToLower, "") ' D4177
                        If sPasscode <> "" Then
                            tPrj = App.DBProjectByPasscode(sPasscode)
                            If tPrj Is Nothing Then
                                .EvalOpenNextProjectAtFinish = False
                                .Save()
                            Else
                                'If tPrj.ProjectStatus <> ecProjectStatus.psActive OrElse Not tPrj.isOnline OrElse tPrj.WorkgroupID <> App.ActiveProject.WorkgroupID Then
                                If tPrj.ProjectStatus <> ecProjectStatus.psActive OrElse tPrj.WorkgroupID <> App.ActiveProject.WorkgroupID Then
                                    .EvalOpenNextProjectAtFinish = False
                                    .Save()
                                End If
                            End If
                        End If
                    End If
                End With
            End If
            Return tPrj
        End Function
        ' D4164 ==

        ' D2081 + D2427 ===
        Public Function GetRatingAltsWording(fUseRate As Boolean) As String    ' D2099
            Return clsComparionCorePage.GetRatingAltsWordingCommon(App, fUseRate)
        End Function

        Shared Function GetRatingAltsWordingCommon(tApp As clsComparionCore, fUseRate As Boolean, Optional tProject As clsProject = Nothing) As String    ' D2099 + D6736
            Dim sWording As String = ""
            If tProject Is Nothing Then tProject = tApp.ActiveProject   ' D6736
            If tApp IsNot Nothing AndAlso tProject IsNot Nothing Then   ' D6736

                If tProject.PipeParameters.JudgementAltsPromt <> "" Then
                    sWording = tProject.PipeParameters.JudgementAltsPromt
                Else
                    Dim sTempl As String = String.Format("lbl_promt_alt_{0}{{0}}", IIf(tApp.isRiskEnabled, IIf(tProject.isImpact, "impact_", "likelihood_"), ""))   ' D2320
                    sWording = tApp.CurrentLanguage.GetString(String.Format(sTempl, IIf(tProject.PipeParameters.JudgementAltsPromtID < 0, 0, tProject.PipeParameters.JudgementAltsPromtID)), "")
                    If sWording = "" Then sWording = tApp.ResString(String.Format(sTempl, 0))
                End If
                sWording = sWording.Trim.ToLower

                Dim sOption As String = CStr(IIf(tApp.isRiskEnabled, IIf(fUseRate, IIf(tProject.isImpact, "lblRateAlts_DefaultImpact", "lblRateAlts_DefaultRisk"), IIf(tProject.isImpact, "lblEstAlts_DefaultImpact", "lblEstAlts_DefaultRisk")), IIf(fUseRate, "lblRateAlts_Default", "lblEstAlts_Default")))  ' D2258 + D2328 + D2572

                For i As Integer = 0 To 9
                    Dim sRes As String = String.Format("lblRateAlts_Src{0}", i)
                    Dim sSrc As String = tApp.ResString(sRes, True)
                    If sSrc <> sRes AndAlso sSrc.ToLower = sWording Then
                        sOption = String.Format(CStr(IIf(fUseRate, "lblRateAlts_Dest{0}", "lblEstAlts_Dest{0}")), i)  ' D2320
                        Exit For
                    End If
                Next
                Return tApp.ResString(sOption)
            Else
                Return ""
            End If
        End Function
        ' D2081 + D2427 ==

        ' D4187 ===
        'Private Function GetTreatments() As List(Of clsControl)
        '    Dim res As New List(Of clsControl)
        '    If App.HasActiveProject Then
        '        With App.ActiveProject.ProjectManager
        '            .Controls.ReadControls(ECModelStorageType.mstCanvasStreamDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)

        '            For Each user As clsUser In .UsersList
        '                .StorageManager.Reader.LoadUserControlsPermissions(user.UserID)
        '            Next

        '            For Each control As clsControl In .Controls.Controls.OrderByDescending(Function(c) c.Type)
        '                If control.Cost = UNDEFINED_INTEGER_VALUE Then control.Cost = 0
        '                res.Add(control)
        '            Next
        '        End With
        '    End If
        '    Return res
        'End Function

        Public ReadOnly Property ControlsList As List(Of clsControl)
            Get
                If _Treatments Is Nothing Then
                    _Treatments = App.ActiveProject.ProjectManager.Controls.Controls.OrderBy(Function(ctrl) ctrl.Type).ToList()
                End If
                Return _Treatments
            End Get
        End Property

        ' D4245 ===
        Public Sub ControlsListReset()
            _Treatments = Nothing
        End Sub
        ' D4245 ==

        Public Function GetControlName(tCtrl As clsControl) As String
            Dim sRes As String = ""
            If tCtrl IsNot Nothing Then
                sRes = tCtrl.Name
                If _OPT_SHOW_CONTOL_IDX Then
                    For idx As Integer = 0 To ControlsList.Count - 1
                        If ControlsList(idx).ID.Equals(tCtrl.ID) Then
                            sRes = String.Format("{0}. {1}", padWithZeros((idx + 1).ToString, ControlsList.Count.ToString.Length), sRes)
                            Exit For
                        End If
                    Next
                End If
            End If
            Return sRes
        End Function
        ' D4187 ==

        ' D2449 ===
        Public Function GetRatingObjWording(fUseRate As Boolean) As String
            Return clsComparionCorePage.GetRatingObjWordingCommon(App, fUseRate)
        End Function

        Shared Function GetRatingObjWordingCommon(tApp As clsComparionCore, fUseRate As Boolean, Optional tProject As clsProject = Nothing) As String
            Dim sWording As String = ""
            If tProject Is Nothing Then tProject = tApp.ActiveProject   ' D6736
            If tApp IsNot Nothing AndAlso tProject IsNot Nothing Then   ' D6736

                If tProject.PipeParameters.JudgementPromt <> "" Then
                    sWording = tProject.PipeParameters.JudgementPromt
                Else
                    Dim sTempl As String = String.Format("lbl_promt_obj_{0}{{0}}", IIf(tApp.isRiskEnabled, IIf(tProject.isImpact, "impact_", "likelihood_"), "")) ' D2516
                    sWording = tApp.CurrentLanguage.GetString(String.Format(sTempl, IIf(tProject.PipeParameters.JudgementPromtID < 0, 0, tProject.PipeParameters.JudgementPromtID)), "")
                    If sWording = "" Then sWording = tApp.ResString(String.Format(sTempl, 0))
                End If
                sWording = sWording.Trim.ToLower

                Dim sOption As String = CStr(IIf(tApp.isRiskEnabled, IIf(fUseRate, IIf(tProject.isImpact, "lblRate_DefaultImpact", "lblRate_DefaultRisk"), IIf(tProject.isImpact, "lblEst_DefaultImpact", "lblEst_DefaultRisk")), IIf(fUseRate, "lblRate_Default", "lblEst_Default")))  ' D2572

                For i As Integer = 0 To 9
                    Dim sRes As String = String.Format("lblRate_Src{0}", i)
                    Dim sSrc As String = tApp.ResString(sRes, True)
                    If sSrc <> sRes AndAlso sSrc.ToLower = sWording Then
                        sOption = String.Format(CStr(IIf(fUseRate, "lblRate_Dest{0}", "lblEst_Dest{0}")), i)
                        Exit For
                    End If
                Next
                Return tApp.ResString(sOption)
            Else
                Return ""
            End If
        End Function
        ' D2449 ==

        ' D2449 ===
        Public Function GetPromptWord(fIsAlts As Boolean) As String
            Return clsComparionCorePage.GetPromptWordCommon(App, fIsAlts)
        End Function

        Shared Function GetPromptWordCommon(tApp As clsComparionCore, fIsAlts As Boolean, Optional tProject As clsProject = Nothing) As String  ' D6736
            Dim sWording As String = ""
            If tProject Is Nothing Then tProject = tApp.ActiveProject   ' D6736
            If tApp IsNot Nothing AndAlso tProject IsNot Nothing Then   ' D6736

                Dim sPrjWording As String = CStr(IIf(fIsAlts, tProject.PipeParameters.JudgementAltsPromt, tProject.PipeParameters.JudgementPromt)).Trim
                Dim sPrjWordingID As Integer = CInt(IIf(fIsAlts, tProject.PipeParameters.JudgementAltsPromtID, tProject.PipeParameters.JudgementPromtID))

                If sPrjWording <> "" Then
                    sWording = sPrjWording
                Else
                    Dim sTempl As String = String.Format("lbl_promt_{0}{{0}}", IIf(tApp.isRiskEnabled, IIf(tProject.isImpact, "impact_", "likelihood_"), ""))
                    sWording = tApp.CurrentLanguage.GetString(String.Format(sTempl, IIf(tProject.PipeParameters.JudgementPromtID < 0, 0, tProject.PipeParameters.JudgementPromtID)), "")
                    If sWording = "" Then sWording = tApp.ResString(String.Format(sTempl, 0))
                End If
                sWording = sWording.Trim.ToLower

                Dim sOption As String = CStr(IIf(tApp.isRiskEnabled, IIf(fIsAlts, "lblWordAlts_DefaultRisk", "lblWord_DefaultRisk"), IIf(fIsAlts, "lblWordAlts_Default", "lblWord_Default")))

                For i As Integer = 0 To 9
                    Dim sRes As String = String.Format("lblRate_Src{0}", i)
                    Dim sSrc As String = tApp.ResString(sRes, True)
                    If sSrc <> sRes AndAlso sSrc.ToLower = sWording Then
                        sOption = String.Format(CStr(IIf(fIsAlts, "lblWordAlts_Dest{0}", "lblWord_Dest{0}")), i)
                        Exit For
                    End If
                Next
                Return tApp.ResString(sOption)
            Else
                Return ""
            End If
        End Function
        ' D2449 ==

        ' D2316 ===
        Public Function GetPipeStepTask(ByVal Action As clsAction, tExtraParam As Object, Optional fHasSubnodes As Boolean = False, Optional fIgnoreClusterPhrase As Boolean = False, Optional fCanBePathInteractive As Boolean = True, Optional fParseNodeNames As Boolean = True) As String    ' D2692 + D2830 + D3487
            Dim sRes As String = ""
            Dim Params As New Dictionary(Of String, String)
            Dim fGetResString As Boolean = True ' D2503
            Dim tClusterNode As clsNode = Nothing   ' D2364

            Dim IsRiskWithControls As Boolean = App.isRiskEnabled AndAlso App.HasActiveProject AndAlso (CurrentPageID = _PGID_EVALUATE_RISK_CONTROLS OrElse CurrentPageID = _PGID_EVALUATE_RISK_CONTROLS_READONLY)   ' D2503 + D4133 + D4285
            If App.HasActiveProject AndAlso Action IsNot Nothing AndAlso Action.ActionData IsNot Nothing Then
                Dim Hierarchy As clsHierarchy = App.ActiveProject.HierarchyObjectives
                Dim isImpact As Boolean = App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact OrElse App.ActiveProject.isImpact  ' D6912
                ' D4209 ===
                If App.isRiskEnabled AndAlso tExtraParam IsNot Nothing Then
                    If TypeOf (tExtraParam) Is clsRatingScale AndAlso CType(tExtraParam, clsRatingScale).Type = ScaleType.stImpact Then isImpact = True     ' D6912
                    If TypeOf (tExtraParam) Is clsStepFunction AndAlso CType(tExtraParam, clsStepFunction).Type = ScaleType.stImpact Then isImpact = True   ' D6912
                End If
                ' D4209 ==

                Select Case Action.ActionType

                    Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                        Dim Data As clsPairwiseMeasureData = CType(Action.ActionData, clsPairwiseMeasureData)
                        Dim parentNode As clsNode = Nothing
                        Select Case Action.ActionType
                            Case ActionType.atPairwise
                                parentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(Data.ParentNodeID)
                                Hierarchy = CType(IIf(parentNode.IsTerminalNode, App.ActiveProject.HierarchyAlternatives, App.ActiveProject.HierarchyObjectives), clsHierarchy)
                            Case ActionType.atPairwiseOutcomes
                                parentNode = Action.ParentNode
                        End Select
                        If App.isRiskEnabled Then
                            ' D2732 ===
                            If parentNode.RiskNodeType = RiskNodeType.ntCategory Then
                                sRes = CStr(IIf(parentNode.IsTerminalNode, "task_Pairwise_Alternatives_Category", "task_Pairwise_Objectives_Category"))
                            Else
                                ' D2732 ==
                                If parentNode.IsTerminalNode Then
                                    sRes = CStr(IIf(parentNode.Level = 0, "task_Pairwise_AlternativesNoObj", IIf(isImpact, "task_Pairwise_Alternatives", "task_Pairwise_AlternativesLikelihood"))) ' D2374 + D2714
                                Else
                                    If parentNode.ParentNode Is Nothing Then
                                        ' D4209 ===
                                        If tExtraParam IsNot Nothing Then
                                            If App.isRiskEnabled Then
                                                sRes = CStr(IIf(isImpact, "task_Pairwise_ObjectivesIntensities_Impact", "task_Pairwise_ObjectivesIntensities_Likelihood"))
                                            Else
                                                sRes = "task_Pairwise_ObjectivesIntensities"
                                            End If
                                        Else
                                            sRes = "task_Pairwise_ObjectivesGoal"
                                        End If
                                        'sRes = CStr(IIf(tExtraParam IsNot Nothing, "task_Pairwise_ObjectivesIntensities", "task_Pairwise_ObjectivesGoal"))
                                        ' D4209 ==
                                    Else
                                        sRes = CStr(IIf(isImpact, "task_Pairwise_Objectives", "task_Pairwise_ObjectivesLikelihood"))    ' D2718
                                    End If
                                End If
                            End If
                        Else
                            sRes = CStr(IIf(parentNode.IsTerminalNode, "task_Pairwise_Alternatives", "task_Pairwise_Objectives"))
                        End If

                        Dim tNodeLeft As New clsNode
                        Dim tNodeRight As New clsNode
                        Dim fIsPWOutcomes As Boolean = Action.ActionType = ActionType.atPairwiseOutcomes
                        If fIsPWOutcomes AndAlso parentNode IsNot Nothing Then  ' D2351
                            Dim tRS As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                            sRes = CStr(IIf(parentNode.IsAlternative, IIf(Action.PWONode.ParentNode Is Nothing, "task_PairwiseOutcomesAltGoal", "task_PairwiseOutcomesAlt"), IIf(Action.PWONode Is Nothing, "task_PairwiseOutcomesGoal", "task_PairwiseOutcomes")))    ' D2318 + D2351 + D2410 + D2438
                            If parentNode.Level > 1 Then sRes = "task_PairwiseOutcomesLevels"
                            If tRS.IsPWofPercentages Then sRes = CStr(IIf(parentNode.ParentNode Is Nothing, "task_PairwisePercentagesGoal", "task_PairwisePercentages"))    ' D6122
                            If tRS.IsExpectedValues Then sRes = CStr(IIf(parentNode.ParentNode Is Nothing, "task_PairwiseExpectedValuesGoal", "task_PairwiseExpectedValues"))
                            App.ActiveProject.ProjectManager.PipeBuilder.GetPWNodes(Action, Data, tNodeLeft, tNodeRight)
                            Params.Add(_TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Action.PWONode, fCanBePathInteractive)) ' D2830
                        Else
                            If Hierarchy IsNot Nothing Then
                                tNodeLeft = Hierarchy.GetNodeByID(Data.FirstNodeID)
                                tNodeRight = Hierarchy.GetNodeByID(Data.SecondNodeID)
                            End If
                        End If
                        If parentNode IsNot Nothing Then Params.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(parentNode, fCanBePathInteractive)) ' D2830
                        If tNodeLeft IsNot Nothing Then Params.Add(_TEMPL_NODE_A, JS_SafeHTML(tNodeLeft.NodeName))
                        If tNodeRight IsNot Nothing Then Params.Add(_TEMPL_NODE_B, JS_SafeHTML(tNodeRight.NodeName))
                        tClusterNode = parentNode   ' D2364

                    Case ActionType.atNonPWOneAtATime
                        Dim data As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData)
                        If Not data Is Nothing Then
                            ' D2503 ===
                            If IsRiskWithControls Then
                                If data IsNot Nothing AndAlso data.Assignment IsNot Nothing AndAlso data.Control IsNot Nothing Then
                                    Params.Add(_TEMPL_NODENAME, GetControlName(data.Control))   ' D4187
                                    Dim tNode As clsNode = Nothing
                                    Dim WRT As clsNode = Nothing

                                    Select Case data.Control.Type
                                        Case ControlType.ctCause
                                            If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                                tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(data.Assignment.ObjectiveID)
                                                If tNode IsNot Nothing Then Params.Add(_TEMPL_NODE_A, GetWRTNodeNameWithPath(tNode, fCanBePathInteractive)) ' D4133
                                                WRT = tNode
                                            End If
                                            If Not Guid.Equals(data.Assignment.EventID, Guid.Empty) Then    ' D4033
                                                tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID)
                                                If tNode IsNot Nothing Then Params.Add(_TEMPL_JUSTNODE, JS_SafeHTML(tNode.NodeName))
                                            End If
                                        Case ControlType.ctCauseToEvent
                                            If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                                tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(data.Assignment.ObjectiveID)
                                                If tNode Is Nothing Then tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(data.Assignment.ObjectiveID)   ' D6560
                                                If tNode IsNot Nothing Then Params.Add(_TEMPL_NODE_A, GetWRTNodeNameWithPath(tNode, fCanBePathInteractive)) ' D4133
                                                WRT = tNode
                                            End If
                                            If Not Guid.Equals(data.Assignment.EventID, Guid.Empty) Then        ' D4033
                                                tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID)
                                                If tNode IsNot Nothing Then Params.Add(_TEMPL_JUSTNODE, JS_SafeHTML(tNode.NodeName))
                                            End If
                                        Case ControlType.ctConsequenceToEvent
                                            If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                                tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(data.Assignment.ObjectiveID)
                                                If tNode IsNot Nothing Then Params.Add(_TEMPL_NODE_B, GetWRTNodeNameWithPath(tNode, fCanBePathInteractive)) ' D4133
                                                WRT = tNode
                                            End If
                                            If Not Guid.Equals(data.Assignment.EventID, Guid.Empty) Then    ' D4033
                                                tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID)
                                                If tNode IsNot Nothing Then Params.Add(_TEMPL_JUSTNODE, JS_SafeHTML(tNode.NodeName))
                                            End If
                                    End Select
                                    tClusterNode = WRT  ' D2703

                                    Dim sName As String = ""
                                    Select Case data.Control.Type
                                        Case ControlType.ctCause
                                            sName = ResString("lblControlCause")
                                        Case ControlType.ctCauseToEvent
                                            sName = ResString(CStr(IIf(WRT IsNot Nothing AndAlso WRT.Level = 0, If(WRT.IsAlternative, "lblControlCauseFromEventToEvent", "lblControlCauseToEventGoal"), "lblControlCauseToEvent")))   ' D2654 + D3705 + D4033 + D6560
                                            'sName = ResString(CStr(IIf(tNode IsNot Nothing AndAlso tNode.ParentNode Is Nothing, "lblControlCauseToEventGoal", "lblControlCauseToEvent")))   ' D2654 + D3705
                                        Case ControlType.ctConsequenceToEvent
                                            sName = ResString("lblControlConsequence")
                                    End Select
                                    sRes = String.Format(ResString("taskControlNonPWOneAtATime"), sName)
                                    fGetResString = False
                                End If

                            Else
                                ' D2503 ==
                                Select Case data.MeasurementType
                                    Case ECMeasureType.mtRatings
                                        ' D2508 ===
                                        ' D2589 ===
                                        Dim isAlt As Boolean = data.EdgeMeasurementScale IsNot Nothing OrElse (data.Node IsNot Nothing AndAlso (data.Node.IsAlternative OrElse data.Node.IsTerminalNode)) ' D2530 + D2587
                                        Dim tData As clsNonPairwiseMeasureData = CType(data.Judgment, clsNonPairwiseMeasureData)
                                        Dim tNode As clsNode = Nothing
                                        If isAlt Then   ' D2530
                                            tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(CType(data.Judgment, clsNonPairwiseMeasureData).NodeID)
                                        Else
                                            tNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(CType(data.Judgment, clsNonPairwiseMeasureData).NodeID)
                                            If tNode Is Nothing Then
                                                tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(CType(data.Judgment, clsNonPairwiseMeasureData).NodeID)
                                                isAlt = True
                                            End If
                                        End If
                                        If tNode IsNot Nothing Then Params.Add(_TEMPL_NODE_A, JS_SafeHTML(tNode.NodeName))
                                        ' D2589 ==
                                        ' D1508 ==
                                        Params.Add(_TEMPL_NODE_B, GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive))  ' D0558 + D2830
                                        If App.isRiskEnabled Then
                                            If isImpact Then
                                                sRes = CStr(IIf(data.Node.Level > 0, "lblEvaluationRatingImpact", "lblEvaluationRatingImpactNoLevels"))
                                            Else
                                                sRes = CStr(IIf(data.Node.Level > 0, "lblEvaluationRatingRisk", "lblEvaluationRatingNoLevelsRisk")) ' D2407
                                            End If
                                        Else
                                            sRes = CStr(IIf(data.Node.Level > 0, "lblEvaluationRating", "lblEvaluationRatingNoLevels")) ' D2589
                                        End If
                                        If data.Node IsNot Nothing AndAlso Not isAlt Then sRes += "Obj" ' D2527 + D2530

                                    Case ECMeasureType.mtStep
                                        Dim tStep As clsStepMeasureData = CType(data.Judgment, clsStepMeasureData)
                                        Dim tParentNode As clsNode = CType(data.Node.Hierarchy.GetNodeByID(tStep.ParentNodeID), clsNode)
                                        Dim tAlt As clsNode
                                        If data.EdgeMeasurementScale IsNot Nothing OrElse tParentNode.IsTerminalNode Then
                                            tAlt = data.Node.Hierarchy.ProjectManager.AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(tStep.NodeID)
                                        Else
                                            tAlt = data.Node.Hierarchy.GetNodeByID(tStep.NodeID)
                                        End If

                                        Params.Add(_TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode, fCanBePathInteractive))  ' D2830
                                        Params.Add(_TEMPL_NODENAME, JS_SafeHTML(tAlt.NodeName))
                                        ' D2408 ===
                                        If App.isRiskEnabled Then
                                            If isImpact Then
                                                sRes = CStr(IIf(tParentNode.ParentNode Is Nothing, "lblEvaluationStepGoalImpact", "lblEvaluationStepImpact"))
                                            Else
                                                sRes = CStr(IIf(tParentNode.ParentNode Is Nothing, "lblEvaluationStepGoalRisk", "lblEvaluationStepRisk"))
                                            End If
                                        Else
                                            sRes = "lblEvaluationStep"
                                        End If
                                        ' D2408 ==

                                    Case ECMeasureType.mtDirect
                                        Params.Add(_TEMPL_NODE_A, GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive))  ' D2361 + D2379 + D2830
                                        ' D2540 ===
                                        Dim tDirect As clsDirectMeasureData = CType(data.Judgment, clsDirectMeasureData)
                                        Dim tH As clsHierarchy
                                        'If data.Node.IsTerminalNode Then tH = App.ActiveProject.HierarchyAlternatives Else tH = App.ActiveProject.HierarchyObjectives
                                        If Action.IsFeedback And App.ActiveProject.ProjectManager.FeedbackOn Then
                                            tH = App.ActiveProject.HierarchyObjectives
                                        Else
                                            If data.EdgeMeasurementScale IsNot Nothing OrElse data.Node.IsTerminalNode Then tH = App.ActiveProject.HierarchyAlternatives Else tH = App.ActiveProject.HierarchyObjectives
                                        End If
                                        Params.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(tH.GetNodeByID(tDirect.NodeID), fCanBePathInteractive)) ' D2830
                                        ' D2540 ==
                                        If App.isRiskEnabled Then
                                            If isImpact Then
                                                sRes = CStr(IIf(data.Node.Level = 0, "task_DirectDataImpactNoObj", "task_DirectDataImpact")) ' D2398
                                            Else
                                                sRes = CStr(IIf(data.Node.Level = 0, "task_DirectDataRiskNoObj", "task_DirectDataRisk"))
                                            End If
                                            Params.Add(_TEMPL_NODETYPE, CStr(IIf(data.Node.IsTerminalNode, _TEMPL_ALTERNATIVE, _TEMPL_OBJECTIVE)))   ' D2540
                                        Else
                                            sRes = "task_DirectData"
                                        End If

                                    Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtCustomUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                                        Dim tParentNode As clsNode = CType(data.Node.Hierarchy.GetNodeByID(CType(data.Judgment, clsUtilityCurveMeasureData).ParentNodeID), clsNode)
                                        Params.Add(_TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode, fCanBePathInteractive))   ' D2830
                                        Dim tAlt As clsNode
                                        If data.EdgeMeasurementScale IsNot Nothing OrElse tParentNode.IsTerminalNode Then
                                            tAlt = data.Node.Hierarchy.ProjectManager.AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(CType(data.Judgment, clsUtilityCurveMeasureData).NodeID)
                                        Else
                                            tAlt = data.Node.Hierarchy.GetNodeByID(CType(data.Judgment, clsUtilityCurveMeasureData).NodeID)
                                        End If
                                        Params.Add(_TEMPL_NODENAME, JS_SafeHTML(tAlt.NodeName))
                                        Select Case data.MeasurementType
                                            Case ECMeasureType.mtAdvancedUtilityCurve
                                                sRes = "task_AdvancedUtilityCurve"
                                            Case Else
                                                ' D2405 ===
                                                If App.isRiskEnabled Then
                                                    If isImpact Then
                                                        sRes = CStr(IIf(tParentNode.ParentNode Is Nothing, "lblEvaluationUCGoalImpact", "lblEvaluationUCImpact"))
                                                    Else
                                                        sRes = CStr(IIf(tParentNode.ParentNode Is Nothing, "lblEvaluationUCGoalRisk", "lblEvaluationUCRisk"))
                                                    End If
                                                Else
                                                    sRes = "lblEvaluationUC"
                                                End If
                                                ' D2405 ==
                                        End Select

                                End Select
                                tClusterNode = data.Node    ' D2364
                            End If
                        End If

                    Case ActionType.atNonPWAllChildren
                        Dim data As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData)
                        If data IsNot Nothing AndAlso data.ParentNode IsNot Nothing Then

                            Select Case data.MeasurementType
                                Case ECMeasureType.mtRatings
                                    If App.isRiskEnabled Then
                                        If isImpact Then
                                            sRes = CStr(IIf(data.ParentNode.IsAlternative, "task_MultiRatings_AllCovObjImpact", IIf(data.ParentNode.ParentNode Is Nothing, "task_MultiRatings_AllAltsGoalImpact", "task_MultiRatings_AllAltsImpact")))
                                        Else
                                            sRes = CStr(IIf(data.ParentNode Is Nothing OrElse data.ParentNode.ParentNode Is Nothing, "lblEvaluationMultiDirectDataLikelihood", IIf(data.ParentNode.IsTerminalNode, "task_MultiRatings_AllAltsRisk", IIf(data.ParentNode.RiskNodeType = RiskNodeType.ntCategory, "task_MultiRatings_AllObjRisk_Cat", "task_MultiRatings_AllObjRisk")))) ' D2318 + D2319 + D2964
                                        End If
                                        If Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1 Then sRes = "task_MultiRatings_AllAlts_NoObj"
                                    Else
                                        sRes = "task_MultiRatings_AllAlts"
                                    End If


                                Case ECMeasureType.mtDirect
                                    If App.isRiskEnabled Then
                                        If data.ParentNode.IsTerminalNode Then
                                            ' D2354 ===
                                            If isImpact Then
                                                sRes = CStr(IIf(data.ParentNode.Level = 0, "lblEvaluationMultiDirectDataAltsGoalRisk", "lblEvaluationMultiDirectDataAltsRisk")) ' D2399
                                            Else
                                                sRes = CStr(IIf(data.ParentNode.Level = 0, "lblEvaluationMultiDirectDataAltsGoalLikelihood", "lblEvaluationMultiDirectDataAltsLikelihood"))
                                            End If
                                            ' D2354 ==
                                        Else
                                            If Not isImpact Then
                                                sRes = CStr(IIf(data.ParentNode.Level > 0, "lblEvaluationMultiDirectDataLevelsLikelihood", "lblEvaluationMultiDirectDataLikelihood"))
                                            Else
                                                sRes = CStr(IIf(data.ParentNode.ParentNode Is Nothing, "lblEvaluationMultiDirectDataGoalRisk", "lblEvaluationMultiDirectDataRiskObj"))
                                            End If
                                        End If
                                    Else
                                        If data.ParentNode.IsTerminalNode Then
                                            sRes = "lblEvaluationMultiDirectDataAlts"
                                        Else
                                            sRes = "lblEvaluationMultiDirectData"
                                        End If
                                    End If

                            End Select

                            Params.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.ParentNode, fCanBePathInteractive AndAlso data.ParentNode.RiskNodeType <> RiskNodeType.ntCategory)) ' D2830 + D2964
                            Params.Add(_TEMPL_EVALCOUNT, CStr(data.Children.Count))
                            tClusterNode = data.ParentNode  ' D2364
                        End If


                    Case ActionType.atNonPWAllCovObjs
                        Dim data As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData) 'Cv2 'C0464
                        If Not data Is Nothing Then

                            Select Case data.MeasurementType
                                Case ECMeasureType.mtRatings
                                    If App.isRiskEnabled Then
                                        If isImpact Then
                                            sRes = CStr(IIf(App.ActiveProject.HierarchyObjectives.GetMaxLevel < 1, "task_MultiRatings_AllCovObjGoalImpact", "task_MultiRatings_AllCovObjImpact"))
                                        Else
                                            sRes = CStr(IIf(App.ActiveProject.HierarchyObjectives.GetMaxLevel < 1, "task_MultiRatings_AllCovObjGoal", "task_MultiRatings_AllCovObj"))
                                            'If Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1 Then sRes = "task_MultiRatings_AllAlts_NoObj" ' - D2429
                                        End If
                                    Else
                                        sRes = "task_MultiRatings_AllCovObj"
                                    End If

                                Case ECMeasureType.mtDirect
                                    If App.isRiskEnabled Then
                                        ' D2360 ===
                                        If data.Alternative.IsTerminalNode Then
                                            If Hierarchy.Nodes.Count <= 1 Then sRes = "lblEvaluationMultiDirectAltWRTCovRiskNoObj" Else sRes = "lblEvaluationMultiDirectAltWRTCovRisk"
                                        Else
                                            If Hierarchy.Nodes.Count <= 1 Then sRes = "lblEvaluationMultiDirectDataRiskNoObj" Else sRes = "lblEvaluationMultiDirectDataRisk"
                                        End If
                                        ' D2360 ==
                                    Else
                                        sRes = CStr(IIf(data.Alternative.IsTerminalNode, "lblEvaluationMultiDirectAltWRTCov", "lblEvaluationMultiDirectData"))
                                    End If
                            End Select

                            Params.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.Alternative, fCanBePathInteractive))    ' D2830
                            Params.Add(_TEMPL_EVALCOUNT, CStr(data.CoveringObjectives.Count))
                            tClusterNode = data.Alternative ' D2364
                        End If


                        ' D3250 ===
                    Case ActionType.atAllEventsWithNoSource
                        Dim data As clsAllEventsWithNoSourceEvaluationActionData = CType(Action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)
                        If Not data Is Nothing Then
                            Select Case data.MeasurementType
                                Case ECMeasureType.mtRatings, ECMeasureType.mtDirect
                                    sRes = "lblEvaluationNoSources"
                                    tClusterNode = Nothing
                            End Select
                        End If
                        ' D3250 ==

                    Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                        If TypeOf (Action.ActionData) Is clsAllPairwiseEvaluationActionData Then

                            Dim Data As clsAllPairwiseEvaluationActionData = CType(Action.ActionData, clsAllPairwiseEvaluationActionData)
                            Dim fIsPWOutcomes As Boolean = Action.ActionType = ActionType.atAllPairwiseOutcomes
                            Params.Add(_TEMPL_EVALCOUNT, CStr(Data.Judgments.Count))
                            If fIsPWOutcomes Then
                                If Action.ParentNode IsNot Nothing Then Params.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(Action.ParentNode, fCanBePathInteractive)) ' D2830 + D4095
                                If Action.ParentNode IsNot Nothing Then Params.Add(_TEMPL_NODE_A, Action.ParentNode.NodeName) ' D4296
                                Params.Add(_TEMPL_JUSTNODE, JS_SafeHTML(Data.ParentNode.NodeName))
                                If Action.ParentNode IsNot Nothing AndAlso Action.ParentNode.IsAlternative Then
                                    sRes = CStr(IIf(Data.ParentNode IsNot Nothing AndAlso Data.ParentNode.ParentNode Is Nothing, "task_MultiPairwise_Alternatives_PW{0}_Goal", "task_MultiPairwise_Alternatives_PW{0}"))    ' D2351
                                Else
                                    sRes = CStr(IIf(Data.ParentNode IsNot Nothing AndAlso Data.ParentNode.ParentNode Is Nothing, "task_MultiPairwise_Hierarchy_PW{0}", "task_MultiPairwise_Objectives_PW{0}"))
                                End If
                                Dim tRS As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                                Dim sName As String = "Outcomes"
                                If tRS.IsPWofPercentages Then sName = "Percentages"
                                If tRS.IsExpectedValues Then sName = "ExpectedValues"
                                sRes = String.Format(sRes, sName)
                                If Action.ParentNode IsNot Nothing Then tClusterNode = Action.ParentNode Else tClusterNode = Data.ParentNode ' D4296
                            Else
                                Params.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(Data.ParentNode, fCanBePathInteractive))    ' D2830 + D4095
                                If Data.ParentNode IsNot Nothing AndAlso Data.ParentNode.IsTerminalNode Then
                                    sRes = CStr(IIf(Data.ParentNode.ParentNode Is Nothing, IIf(App.isRiskEnabled, "task_MultiPairwise_AlternativesGoalRisk", "task_MultiPairwise_AlternativesGoal"), IIf(App.isRiskEnabled AndAlso Not isImpact, "task_MultiPairwise_AlternativesLikelihood", "task_MultiPairwise_Alternatives")))   ' D2569 + D2730 + D3614
                                Else
                                    ' D4209 ===
                                    If tExtraParam IsNot Nothing Then
                                        If TypeOf (tExtraParam) Is clsStepFunction Then
                                            If App.isRiskEnabled Then
                                                sRes = CStr(IIf(isImpact, "task_MultiPairwise_Objectives_Intensities_SF_Impact", "task_MultiPairwise_Objectives_Intensities_SF_Likelihood"))
                                            Else
                                                sRes = "task_MultiPairwise_Objectives_Intensities_SF"
                                            End If
                                        Else
                                            If App.isRiskEnabled Then
                                                sRes = CStr(IIf(isImpact, "task_MultiPairwise_Objectives_Intensities_Impact", "task_MultiPairwise_Objectives_Intensities_Likelihood"))
                                            Else
                                                sRes = "task_MultiPairwise_Objectives_Intensities"
                                            End If
                                        End If
                                    Else
                                        sRes = CStr(IIf(Data.ParentNode.ParentNode Is Nothing, IIf(App.isRiskEnabled, "task_MultiPairwise_ObjectivesGoalRisk", "task_MultiPairwise_ObjectivesGoal"), IIf(App.isRiskEnabled AndAlso Not isImpact, "task_MultiPairwise_ObjectivesLikelihood", "task_MultiPairwise_Objectives")))
                                    End If
                                    'sRes = CStr(IIf(tExtraParam IsNot Nothing, IIf(TypeOf (tExtraParam) Is clsStepFunction, "task_MultiPairwise_Objectives_Intensities_SF", "task_MultiPairwise_Objectives_Intensities"), IIf(Data.ParentNode.ParentNode Is Nothing, IIf(App.isRiskEnabled, "task_MultiPairwise_ObjectivesGoalRisk", "task_MultiPairwise_ObjectivesGoal"), IIf(App.isRiskEnabled AndAlso Not isImpact, "task_MultiPairwise_ObjectivesLikelihood", "task_MultiPairwise_Objectives"))))    ' D2569 + D2730 + D3614
                                    ' D4209 ==
                                End If
                                tClusterNode = Data.ParentNode  ' D2364
                            End If
                        End If

                    Case ActionType.atShowLocalResults
                        Dim Data As clsShowLocalResultsActionData = CType(Action.ActionData, clsShowLocalResultsActionData)
                        Params.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(Data.ParentNode, fCanBePathInteractive)) ' D2830
                        Dim fIsPWOutcomes As Boolean = Data.PWOutcomesNode IsNot Nothing AndAlso Data.ParentNode.MeasureType = ECMeasureType.mtPWOutcomes
                        If fIsPWOutcomes Then Params.Add(_TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Data.PWOutcomesNode, fCanBePathInteractive)) ' D2830
                        If tExtraParam IsNot Nothing Then
                            sRes = "task_LocalResultsObjectivesIntensities"
                        Else
                            If App.isRiskEnabled Then
                                If Data.ParentNode.IsTerminalNode Then
                                    sRes = CStr(IIf(Not isImpact, "task_LocalResultsAlternativesRisk", "task_LocalResultsAlternativesImpact"))
                                Else
                                    ' D2723 ==
                                    If Not isImpact AndAlso Data.ParentNode.RiskNodeType = RiskNodeType.ntCategory Then
                                        sRes = "task_LocalResultsObjectives_Category"
                                    Else
                                        ' D2723 ==
                                        If Not isImpact Then
                                            sRes = CStr(IIf(Data.ParentNode.ParentNode Is Nothing, "task_LocalResultsObjectivesGoal", "task_LocalResultsObjectivesRisk"))
                                        Else
                                            sRes = CStr(IIf(Data.ParentNode.ParentNode Is Nothing, "task_LocalResultsObjectivesGoalImpact", "task_LocalResultsObjectivesImpact"))
                                        End If
                                    End If
                                End If
                            Else
                                sRes = CStr(IIf(Data.ParentNode.IsTerminalNode, "task_LocalResultsAlternatives", "task_LocalResultsObjectives"))
                            End If
                        End If
                        tClusterNode = Data.ParentNode   ' D2364


                    Case ActionType.atShowGlobalResults
                        If App.isRiskEnabled Then
                            sRes = CStr(IIf(isImpact, "task_GlobalResultsImpact", "task_GlobalResultsRisk"))
                        Else
                            sRes = "task_GlobalResults"
                        End If
                        tClusterNode = Hierarchy.Nodes(0)   ' D2364
                        Params.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(tClusterNode, fCanBePathInteractive))  ' D2364 + D2830

                    Case ActionType.atSensitivityAnalysis
                        Dim Data As clsSensitivityAnalysisActionData = CType(Action.ActionData, clsSensitivityAnalysisActionData)
                        Select Case Data.SAType
                            Case SAType.satDynamic
                                sRes = CStr(IIf(App.isRiskEnabled, IIf(Not isImpact, "task_DynamicSARisk", "task_DynamicSAImpact"), "task_DynamicSA"))  ' D2527
                            Case SAType.satGradient
                                sRes = CStr(IIf(App.isRiskEnabled, IIf(Not isImpact, "task_GradientSARisk", "task_GradientSAImpact"), "task_GradientSA"))
                            Case SAType.satPerformance
                                sRes = CStr(IIf(App.isRiskEnabled, IIf(Not isImpact, "task_PerformanceSARisk", "task_PerformanceSAImpact"), "task_PerformanceSA"))  ' D2527
                        End Select
                        tClusterNode = Action.ParentNode    ' D2364

                    ' D6671 ===
                    Case ActionType.atEmbeddedContent
                        Select Case Action.EmbeddedContentType
                            Case EmbeddedContentType.AlternativesRank
                                sRes = "taskAlternativesOrderStep"
                        End Select
                        ' D6671 ==

                End Select
            End If

            ' D2364 ===
            Dim sDefTask As String = ""
            If fGetResString AndAlso sRes <> "" Then sDefTask = ResString(sRes, False, False) Else sDefTask = sRes ' D2503 + D2596

            If tClusterNode Is Nothing Then tClusterNode = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionNode(Action) ' D4711

            ' D2751 ===
            Dim isMulti As Boolean = False
            Dim tAGuid As Guid = Guid.Empty ' D4053
            Select Case Action.ActionType
                Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
                    isMulti = True
                    ' D4053 ===
                Case ActionType.atShowLocalResults
                    isMulti = True
                    If tClusterNode IsNot Nothing Then tAGuid = tClusterNode.NodeGuidID
                Case ActionType.atShowGlobalResults, ActionType.atSensitivityAnalysis
                    If tClusterNode IsNot Nothing Then tAGuid = tClusterNode.NodeGuidID
                    ' D4053 ==
            End Select
            ' D2751 ==

            If tClusterNode IsNot Nothing AndAlso Not fIgnoreClusterPhrase Then
                ClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterPhraseForNode(tClusterNode, isMulti, tAGuid, IsRiskWithControls) ' D2692 + D2751 + D4053 + D4133
            End If
            If ClusterPhrase = "" OrElse HTML2Text(ClusterPhrase) = "" Then ClusterPhrase = sDefTask ' D2729
            ClusterPhraseIsCustom = ClusterPhrase.Trim.ToLower <> sDefTask.Trim.ToLower
            If tClusterNode IsNot Nothing Then TaskNodeGUID = tClusterNode.NodeGuidID.ToString
            TaskTemplates = Params

            ' D2929 ===
            If tClusterNode Is Nothing OrElse (tClusterNode IsNot Nothing AndAlso tClusterNode.RiskNodeType <> RiskNodeType.ntCategory) Then    ' D2964
                'If fCanBePathInteractive Then  ' D2972
                Dim sNodesTempls As String() = {_TEMPL_JUSTNODE, _TEMPL_NODENAME, _TEMPL_NODE_A, _TEMPL_NODE_B}
                Dim sNodeParams As New Dictionary(Of String, String)
                ' D4116 ===
                ClusterPhrase = ClusterPhrase.Replace("'node_name'>", """node_name"">").Replace("'wrt_name'>", """wrt_name"">")
                For Each sTpl As String In sNodesTempls
                    If ClusterPhrase.Contains(sTpl) Then
                        Dim sRepl As String = String.Format("<span class=""node_name"">{0}</span>", sTpl)
                        Dim sSearch As String = String.Format("<span class=""node_name"">{0}</span>", sRepl)
                        While ClusterPhrase.Contains(sSearch)
                            ClusterPhrase = ClusterPhrase.Replace(sSearch, sRepl)
                        End While
                        If Not ClusterPhrase.ToLower.Contains("""node_name"">" + sTpl.ToLower) AndAlso Not ClusterPhrase.ToLower.Contains("""wrt_name"">" + sTpl.ToLower) Then
                            sNodeParams.Add(sTpl, sRepl)
                        End If
                    End If
                    ' D4116 ==
                Next
                If sNodeParams.Count > 0 Then ClusterPhrase = ParseStringTemplates(ClusterPhrase, sNodeParams)
                'End If
            End If
            ' D2929 ==

            Dim sTask As String = ClusterPhrase    ' D3487
            If fParseNodeNames AndAlso Params IsNot Nothing Then    ' D3487
                sTask = PrepareTask(ParseStringTemplates(sTask, Params), tExtraParam, fHasSubnodes, Params) ' D6981
                If Params IsNot Nothing Then
                    For Each sKey As String In Params.Keys
                        If Not TaskTemplates.ContainsKey(sKey) Then TaskTemplates.Add(sKey, Params(sKey))
                    Next
                End If
            Else
                sTask = PrepareTask(sTask, tExtraParam, fHasSubnodes)   ' D3487 + D6981
            End If

            ' D4148 ===
            If ClusterPhrase <> "" AndAlso ClusterPhrase.Contains("%%") Then
                Dim tList As Dictionary(Of String, String) = GetUserTemplateReplacements(True, tClusterNode)
                If tList IsNot Nothing Then
                    For Each sName As String In tList.Keys
                        ClusterPhrase = ParseTemplate(ClusterPhrase, sName, tList(sName))
                    Next
                End If
            End If
            ' D4148 ==

            Return sTask
            ' D2364 ==
        End Function
        ' D2316 ==

        ' D4148 ===
        Private Function AddTemplatePair(sName As String, sWordingTpl As String, ByRef tDict As Dictionary(Of String, String), fDirectReplace As Boolean) As Boolean
            Dim sVal As String = ""
            'If App.ActiveWorkgroup.WordingTemplates.ContainsKey(sWordingTpl) Then sVal = App.ActiveWorkgroup.WordingTemplates(sWordingTpl)
            If App.ActiveWorkgroup.WordingTemplates.ContainsKey(sWordingTpl) Then sVal = sWordingTpl
            If String.IsNullOrEmpty(sVal) Then sVal = ParseString(sName)
            If tDict IsNot Nothing AndAlso Not String.IsNullOrEmpty(sVal) AndAlso Not sName.Equals(sVal, StringComparison.InvariantCultureIgnoreCase) Then
                If Not sVal.Contains("%%") Then sVal = String.Format("%%{0}%%", sVal)
                If fDirectReplace Then tDict.Add(sName, sVal) Else tDict.Add(sVal, sName)
                Return True
            Else
                Return False
            End If
        End Function

        Public Function GetUserTemplateReplacements(fDirectReplace As Boolean, Optional tStepNode As clsNode = Nothing) As Dictionary(Of String, String)
            Dim tList As New Dictionary(Of String, String)
            If App.isRiskEnabled AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.WordingTemplates IsNot Nothing Then
                Dim isImpact As Boolean = App.ActiveProject.isImpact
                If (CurrentPageID = _PGID_EVALUATE_RISK_CONTROLS OrElse CurrentPageID = _PGID_EVALUATE_RISK_CONTROLS_READONLY) AndAlso tStepNode IsNot Nothing AndAlso tStepNode.Hierarchy IsNot Nothing Then isImpact = tStepNode.Hierarchy.HierarchyID = ECHierarchyID.hidImpact ' D4285
                AddTemplatePair(_TPL_COMPARION_ALT, _TPL_RISK_EVENT, tList, fDirectReplace)
                AddTemplatePair(_TPL_COMPARION_ALTS, _TPL_RISK_EVENTS, tList, fDirectReplace)
                AddTemplatePair(_TPL_COMPARION_OBJ, CStr(IIf(isImpact, _TPL_RISK_CONSEQUENCE, _TPL_RISK_SOURCE)), tList, fDirectReplace)
                AddTemplatePair(_TPL_COMPARION_OBJS, CStr(IIf(isImpact, _TPL_RISK_CONSEQUENCES, _TPL_RISK_SOURCES)), tList, fDirectReplace)
            End If
            Return tList
        End Function
        ' D4148 ==

        ' D1653 ===
        Public Function GetPipeStepHint(ByVal Action As clsAction, Optional tExtraParam As Object = Nothing, Optional fCanBePathInteractive As Boolean = False, Optional fGetResultsCustomTitle As Boolean = False) As String  '  D1870 + D2830 + D4329
            Dim sRes As String = "" ' D1770
            If App.HasActiveProject AndAlso Action IsNot Nothing AndAlso (Action.ActionData IsNot Nothing OrElse Action.EmbeddedContentType <> EmbeddedContentType.None) Then    ' D1308 + D1587 + D1653 + D4715
                Dim Hierarchy As clsHierarchy = App.ActiveProject.HierarchyObjectives   ' D1636
                Dim tParams As New Generic.Dictionary(Of String, String)    ' D1636

                ' D6912 ===
                Dim isImpact As Boolean = App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact OrElse App.ActiveProject.isImpact
                If App.isRiskEnabled AndAlso tExtraParam IsNot Nothing Then
                    If TypeOf (tExtraParam) Is clsRatingScale AndAlso CType(tExtraParam, clsRatingScale).Type = ScaleType.stImpact Then isImpact = True
                    If TypeOf (tExtraParam) Is clsStepFunction AndAlso CType(tExtraParam, clsStepFunction).Type = ScaleType.stImpact Then isImpact = True
                End If
                ' D6912 ==
                Dim IsRiskWithControls As Boolean = App.isRiskEnabled AndAlso (CurrentPageID = _PGID_EVALUATE_RISK_CONTROLS OrElse CurrentPageID = _PGID_EVALUATE_RISK_CONTROLS_READONLY) ' D2503 + D4285
                Select Case Action.ActionType

                    Case ActionType.atInformationPage
                        Dim sPage As String = ""
                        Select Case CType(Action.ActionData, clsInformationPageActionData).Description.ToLower 'C0464
                            Case "welcome"
                                sPage = "lblWelcome"
                            Case "thankyou"
                                sPage = "lblThankYou"
                        End Select
                        sRes = String.Format(ResString("lblEvaluationInfoPage"), ResString(sPage))  ' D0031 + D1770

                        ' D0249 ===
                    Case ActionType.atSpyronSurvey
                        ' D0428 ===
                        Dim sHint As String = ResString("lblSpyronSurvey")
                        Dim UsersList As New Dictionary(Of String, clsComparionUser)
                        'UsersList.Add(App.ActiveUser.UserEmail, New clsComparionUser() With {.ID = App.ActiveUser.UserID, .UserName = App.ActiveUser.UserName})
                        UsersList.Add(App.ActiveUser.UserEmail, New clsComparionUser() With {.ID = App.ActiveProject.ProjectManager.UserID, .UserName = App.ActiveProject.ProjectManager.User.UserName})
                        Dim Data As clsSpyronSurveyAction = CType(Action.ActionData, clsSpyronSurveyAction)
                        App.SurveysManager.ActiveUserEmail = App.ActiveUser.UserEmail
                        Dim tSurvey As SpyronControls.Spyron.Core.clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(App.ProjectID, CType(Data.SurveyType, SurveyType), UsersList) ' D1653
                        If Not tSurvey Is Nothing Then
                            Dim tmpSurvey As clsSurvey = tSurvey.Survey(App.ActiveUser.UserEmail)   ' D1308
                            If tmpSurvey IsNot Nothing AndAlso Data.StepNumber > 0 AndAlso tmpSurvey.Pages IsNot Nothing AndAlso tmpSurvey.Pages.Count >= Data.StepNumber AndAlso Data.StepNumber > 0 Then    ' D1308 + D1587
                                Dim tPage As SpyronControls.Spyron.Core.clsSurveyPage = CType(tmpSurvey.Pages(Data.StepNumber - 1), clsSurveyPage) 'L0442 + D1653
                                If Not tPage Is Nothing Then sHint = String.Format("{0}: {1}", sHint, tPage.Title)
                            End If
                        End If
                        sRes = sHint   ' D1770
                        ' D0428 ==
                        ' D0249 ==

                        ' D4715 ===
                    Case ActionType.atEmbeddedContent
                        Select Case Action.EmbeddedContentType
                            Case EmbeddedContentType.RiskResults
                                sRes = ResString("lblRiskResults")
                            Case EmbeddedContentType.HeatMap    ' D6664
                                sRes = ResString("mnuRiskPlotOverall")  ' D6664
                            Case EmbeddedContentType.AlternativesRank  ' D6671
                                sRes = ResString("lblAlternativesOrderStep")    ' D6671
                        End Select
                        ' D4715 ==

                    Case ActionType.atShowLocalResults
                        ' D2137 ===
                        Dim Data As clsShowLocalResultsActionData = CType(Action.ActionData, clsShowLocalResultsActionData)
                        If Not Data.ParentNode Is Nothing Then
                            ' D0120 ===
                            tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(Data.ParentNode, fCanBePathInteractive))    ' D0558 + D2830
                            Dim fIsPWOutcomes As Boolean = Data.PWOutcomesNode IsNot Nothing AndAlso Data.ParentNode.MeasureType = ECMeasureType.mtPWOutcomes
                            If fIsPWOutcomes Then tParams.Add(_TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Data.PWOutcomesNode, fCanBePathInteractive)) ' D2830
                            ' D2258 + D2270 ===
                            If tExtraParam IsNot Nothing Then
                                sRes = CStr(IIf(Data.ParentNode.ParentNode IsNot Nothing, "lblEvaluationResultIntensities", "lblEvaluationResultObjectiveIntensities"))
                            Else
                                If fIsPWOutcomes Then
                                    If Data.PWOutcomesNode IsNot Nothing AndAlso Data.PWOutcomesNode.IsAlternative Then
                                        sRes = CStr(IIf(Data.ParentNode.ParentNode IsNot Nothing, "lblEvaluationResultPW{0}AltsHierarchy", "lblEvaluationResultPW{0}Alts"))
                                    Else
                                        sRes = CStr(IIf(Data.ParentNode.ParentNode IsNot Nothing, "lblEvaluationResultPW{0}Hierarchy", "lblEvaluationResultPW{0}")) ' D2265
                                    End If
                                    ' D2294 ===
                                    Dim sName As String = "Outcomes"
                                    If Action.PWONode IsNot Nothing AndAlso Action.PWONode.MeasurementScale IsNot Nothing Then
                                        Dim tRS As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                                        If tRS.IsPWofPercentages Then sName = "Percentages"
                                        If tRS.IsExpectedValues Then sName = "ExpectedValues"
                                    End If
                                    sRes = String.Format(sRes, sName)    ' D2265
                                    ' D2294 ==
                                Else

                                    If Data.ParentNode.IsTerminalNode Then
                                        If App.isRiskEnabled Then
                                            If isImpact Then
                                                sRes = CStr(IIf(Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1, "lblEvaluationResultAlternativesNoObjImpact", "lblEvaluationResultAlternativesImpact"))
                                            Else
                                                sRes = CStr(IIf(Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1, "lblEvaluationResultAlternativesNoObjsRisk", "lblEvaluationResultAlternativesRisk"))
                                            End If
                                        Else
                                            sRes = "lblEvaluationResultAlternatives"
                                        End If
                                    Else
                                        If App.isRiskEnabled Then
                                            ' D2723 ===
                                            If Not isImpact AndAlso Data.ParentNode.RiskNodeType = RiskNodeType.ntCategory Then
                                                sRes = "lblEvaluationResultObjective_Category"
                                            Else
                                                ' D2723 ==
                                                If Data.ParentNode.ParentNode Is Nothing Then
                                                    sRes = CStr(IIf(isImpact, "lblEvaluationResultObjectiveGoalImpact", "lblEvaluationResultObjectiveGoalRisk"))
                                                Else
                                                    sRes = CStr(IIf(isImpact, "lblEvaluationResultObjectiveImpact", "lblEvaluationResultObjectiveRisk"))
                                                End If
                                            End If
                                        Else
                                            sRes = "lblEvaluationResultObjective"
                                        End If
                                    End If
                                End If
                            End If
                            ' D2270 ==
                            ' D4329 ===
                            sRes = ResString(sRes, True, False)
                            If Data.ParentNode IsNot Nothing AndAlso fGetResultsCustomTitle Then
                                Dim tAddGUID As Guid = Guid.Empty
                                If tExtraParam IsNot Nothing AndAlso TypeOf (tExtraParam) Is clsMeasurementScale Then tAddGUID = CType(tExtraParam, clsMeasurementScale).GuidID
                                ClusterTitle = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterTitleForResults(Data.ParentNode.NodeGuidID, tAddGUID)
                                If ClusterTitle = "" OrElse HTML2Text(ClusterTitle) = "" Then ClusterTitle = sRes
                                ClusterTitleIsCustom = ClusterTitle.Trim.ToLower <> sRes.Trim.ToLower
                                If ClusterTitleIsCustom AndAlso ClusterTitle.Trim <> "" Then sRes = ClusterTitle
                            End If
                            ' D4329 ==

                            sRes = PrepareTask(ParseStringTemplates(sRes, tParams), tExtraParam)
                            ' D0120 + D2137 + D2258 ==
                        End If

                    Case ActionType.atShowGlobalResults
                        ' D0120 ===
                        If App.ActiveProject.HierarchyObjectives.Nodes.Count > 0 Then   ' D1308
                            tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(App.ActiveProject.HierarchyObjectives.Nodes(0), fCanBePathInteractive))   ' D0558 + D2830
                            ' D2258 ===
                            If App.isRiskEnabled Then
                                sRes = CStr(IIf(isImpact, "lblEvaluationResultsOverallImpact", "lblEvaluationResultsOverallRisk"))
                            Else
                                sRes = "lblEvaluationResultsOverall"
                            End If
                            sRes = ParseStringTemplates(ResString(sRes, True, False), tParams)   ' D0031 + D1636 + D1870 + D1954
                        End If
                        ' D0120 + D2258 ==

                        ' D2046 ===
                    Case ActionType.atPairwise, ActionType.atPairwiseOutcomes

                        Dim Act As clsPairwiseMeasureData = CType(Action.ActionData, clsPairwiseMeasureData)
                        Dim parentNode As clsNode = Nothing
                        Dim H As clsHierarchy = Nothing

                        Dim fIsPWOutcomes As Boolean = Action.ActionType = ActionType.atPairwiseOutcomes
                        If fIsPWOutcomes Then
                            parentNode = Action.ParentNode
                        Else
                            parentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(Act.ParentNodeID)
                            If parentNode Is Nothing Then parentNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(Act.ParentNodeID) ' D3041
                            If parentNode IsNot Nothing Then H = CType(IIf(parentNode.IsAlternative OrElse parentNode.IsTerminalNode, App.ActiveProject.HierarchyAlternatives, App.ActiveProject.HierarchyObjectives), clsHierarchy) ' D2109 + D3041
                        End If

                        Dim tNodeLeft As New clsNode
                        Dim tNodeRight As New clsNode

                        If fIsPWOutcomes Then
                            App.ActiveProject.ProjectManager.PipeBuilder.GetPWNodes(Action, Act, tNodeLeft, tNodeRight)
                            tParams.Add(_TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Action.PWONode, fCanBePathInteractive)) ' D2830
                        Else
                            If H IsNot Nothing Then
                                tNodeLeft = H.GetNodeByID(Act.FirstNodeID)
                                tNodeRight = H.GetNodeByID(Act.SecondNodeID)
                            End If
                        End If

                        If parentNode IsNot Nothing Then tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(parentNode, fCanBePathInteractive)) ' D2830
                        If tNodeLeft IsNot Nothing Then tParams.Add(_TEMPL_NODE_A, JS_SafeHTML(tNodeLeft.NodeName))
                        If tNodeRight IsNot Nothing Then tParams.Add(_TEMPL_NODE_B, JS_SafeHTML(tNodeRight.NodeName))

                        If fIsPWOutcomes Then
                            ' D2294 ===
                            Dim tRS As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                            If tRS IsNot Nothing Then   ' D2440
                                sRes = CStr(IIf(parentNode IsNot Nothing AndAlso parentNode.IsAlternative, IIf(Action.PWONode IsNot Nothing AndAlso Action.PWONode.ParentNode Is Nothing, "lblEvaluationPWOutcomesAltsGoal", "lblEvaluationPWOutcomesAlts"), "lblEvaluationPWOutcomes"))  ' D2321 + D2410 + D2438 + D2440
                                If parentNode.Level > 1 Then sRes = "lblEvaluationPWOutcomesLevels"
                                If tRS.IsPWofPercentages Then sRes = "lblEvaluationPWPercentages"
                                If tRS.IsExpectedValues Then sRes = "lblEvaluationExpectedValues"
                                ' D2294 ==
                            End If
                        Else
                            ' D2723 ===
                            If App.isRiskEnabled AndAlso parentNode.RiskNodeType = RiskNodeType.ntCategory Then
                                sRes = CStr(IIf(parentNode.IsTerminalNode, "lblEvaluationPWAlt_Category", "lblEvaluationPW_Category"))
                            Else
                                ' D2723 ==
                                sRes = CStr(IIf(parentNode.Level = 0, "lblEvaluationPWNoObj", IIf(App.isRiskEnabled AndAlso Not isImpact, IIf(parentNode.IsTerminalNode, "lblEvaluationPWLikelihoodAlts", "lblEvaluationPWLikelihood"), "lblEvaluationPW")))    ' D2718
                            End If
                        End If
                        sRes = ParseStringTemplates(ResString(sRes), tParams) ' D1870 + D2258
                        ' D2046 ==

                        ' D1366 ===
                    Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes ' D2046
                        Dim tNode As clsNode = CType(Action.ActionData, clsAllPairwiseEvaluationActionData).ParentNode
                        If Not tNode Is Nothing Then
                            If Action.ActionType = ActionType.atAllPairwiseOutcomes Then
                                tParams.Add(_TEMPL_JUSTNODE, GetWRTNodeNameWithPath(tNode, fCanBePathInteractive)) ' D2046 + D2137 + D2830 + D4095
                                If Action.ParentNode IsNot Nothing Then
                                    tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(Action.ParentNode, fCanBePathInteractive))  ' D2830 + D4095
                                End If
                                ' D2265 ===

                                If Action.ParentNode IsNot Nothing AndAlso Action.ParentNode.IsAlternative Then
                                    sRes = CStr(IIf(tNode IsNot Nothing AndAlso tNode.ParentNode Is Nothing, "task_MultiPairwise_Alternatives_PW{0}_Goal", "task_MultiPairwise_Alternatives_PW{0}"))    ' D2351
                                Else
                                    sRes = CStr(IIf(tNode IsNot Nothing AndAlso tNode.ParentNode Is Nothing, "task_MultiPairwise_Hierarchy_PW{0}", "task_MultiPairwise_Objectives_PW{0}"))
                                End If
                                'If Action.ParentNode IsNot Nothing AndAlso Action.ParentNode.IsAlternative Then
                                '    sRes = CStr(IIf(tNode.ParentNode IsNot Nothing, "lblEvaluationAllPW{0}AltsHierarchy", "lblEvaluationAllPW{0}Alts"))
                                'Else
                                '    If Action.ParentNode IsNot Nothing AndAlso Action.ParentNode.ParentNode IsNot Nothing AndAlso Action.ParentNode.ParentNode.ParentNode IsNot Nothing Then
                                '        sRes = "lblEvaluationAllPW{0}Hierarchy"
                                '    Else
                                '        sRes = "lblEvaluationAllPW{0}"
                                '    End If
                                'End If

                                ' D2294 ===
                                Dim tRs As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                                Dim sName As String = "Outcomes"
                                If tRs.IsPWofPercentages Then sName = "Percentages"
                                If tRs.IsExpectedValues Then sName = "ExpectedValues"
                                sRes = String.Format(sRes, sName)
                                ' D2265 + D2294 ==
                            Else
                                tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(tNode, fCanBePathInteractive))  ' D2830 + D4095
                                sRes = "lblEvaluationAllPW"
                                ' D2731 ===
                                If App.isRiskEnabled AndAlso Not isImpact Then
                                    sRes = If(Not tNode.IsAlternative AndAlso tNode.ParentNode Is Nothing, "task_MultiPairwise_ObjectivesGoalRisk", If(tNode IsNot Nothing AndAlso tNode.IsTerminalNode, "lblEvaluationAllPWLikelihoodAlt", "lblEvaluationAllPWLikelihoodObj"))   ' D4816
                                End If
                                ' D2731 ==
                            End If
                            sRes = ParseStringTemplates(ResString(sRes), tParams)  ' D1870 + D2176 + D2177 + D2193 + D2258
                        End If
                        ' D1366 + D2258 ==

                        ' D0038 ===
                    Case ActionType.atNonPWOneAtATime 'Cv2
                        Dim data As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData) 'Cv2 'C0464
                        ' D2503 ===
                        If IsRiskWithControls Then
                            If data IsNot Nothing AndAlso data.Assignment IsNot Nothing AndAlso data.Control IsNot Nothing Then
                                tParams.Add(_TEMPL_NODENAME, GetControlName(data.Control))  ' D4187
                                Dim tNode As clsNode = Nothing
                                Dim WRT As clsNode = Nothing

                                Select Case data.Control.Type
                                    Case ControlType.ctCause
                                        If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                            tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(data.Assignment.ObjectiveID)
                                            If tNode IsNot Nothing Then tParams.Add(_TEMPL_NODE_A, JS_SafeHTML(tNode.NodeName))
                                            WRT = tNode   ' D4033
                                        End If
                                        If Not Guid.Equals(data.Assignment.EventID, Guid.Empty) Then    ' D4033
                                            tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID)
                                            If tNode IsNot Nothing Then tParams.Add(_TEMPL_JUSTNODE, JS_SafeHTML(tNode.NodeName))
                                        End If
                                    Case ControlType.ctCauseToEvent
                                        If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                            tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(data.Assignment.ObjectiveID)
                                            If tNode Is Nothing Then tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(data.Assignment.ObjectiveID)   ' D6560
                                            If tNode IsNot Nothing Then tParams.Add(_TEMPL_NODE_A, JS_SafeHTML(tNode.NodeName))
                                            WRT = tNode   ' D4033
                                        End If
                                        If Not Guid.Equals(data.Assignment.EventID, Guid.Empty) Then        ' D4033
                                            tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID)
                                            If tNode IsNot Nothing Then tParams.Add(_TEMPL_JUSTNODE, JS_SafeHTML(tNode.NodeName))
                                        End If
                                    Case ControlType.ctConsequenceToEvent
                                        If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                            tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(data.Assignment.ObjectiveID)
                                            If tNode IsNot Nothing Then tParams.Add(_TEMPL_NODE_B, JS_SafeHTML(tNode.NodeName))
                                            WRT = tNode   ' D4033
                                        End If
                                        If Not Guid.Equals(data.Assignment.EventID, Guid.Empty) Then    ' D4033
                                            tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID)
                                            If tNode IsNot Nothing Then tParams.Add(_TEMPL_JUSTNODE, JS_SafeHTML(tNode.NodeName))
                                        End If
                                End Select

                                'If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                '    tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(data.Assignment.ObjectiveID)
                                '    If tNode IsNot Nothing Then tParams.Add(_TEMPL_NODE_A, JS_SafeHTML(tNode.NodeName))
                                '    tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID)
                                '    If tNode IsNot Nothing Then tParams.Add(_TEMPL_JUSTNODE, JS_SafeHTML(tNode.NodeName))
                                'End If
                                'If Not Guid.Equals(data.Assignment.EventID, Guid.Empty) Then
                                '    tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(data.Assignment.EventID)
                                '    If tNode IsNot Nothing Then tParams.Add(_TEMPL_NODE_B, JS_SafeHTML(tNode.NodeName))
                                'End If
                                Dim sName As String = ""
                                Select Case data.Control.Type
                                    Case ControlType.ctCause
                                        sName = ResString("lblControlCause")
                                    Case ControlType.ctCauseToEvent
                                        sName = ResString(CStr(IIf(WRT IsNot Nothing AndAlso WRT.Level = 0, If(WRT.IsAlternative, "lblControlCauseFromEventToEvent", "lblControlCauseToEventGoal"), "lblControlCauseToEvent")))   ' D2654 + D4033 + D6560
                                        'sName = ResString(CStr(IIf(tNode IsNot Nothing AndAlso tNode.ParentNode Is Nothing, "lblControlCauseToEventGoal", "lblControlCauseToEvent")))   ' D2654
                                    Case ControlType.ctConsequenceToEvent
                                        sName = ResString("lblControlConsequence")
                                End Select
                                sRes = String.Format(ResString("hintControlNonPWOneAtATime"), sName)    ' D4134
                                sRes = ParseStringTemplates(sRes, tParams)
                            End If

                        Else
                            ' D2503 ==
                            If data IsNot Nothing AndAlso data.Judgment IsNot Nothing Then  ' D2345
                                ' D0312 ===
                                Select Case data.MeasurementType
                                    Case ECMeasureType.mtDirect
                                        ' D2264 ===
                                        Dim tDirect As clsDirectMeasureData = CType(data.Judgment, clsDirectMeasureData)
                                        tParams.Add(_TEMPL_NODE_A, JS_SafeHTML(data.Node.NodeName))
                                        Dim tH As clsHierarchy
                                        'If data.Node.IsTerminalNode Then tH = App.ActiveProject.HierarchyAlternatives Else tH = App.ActiveProject.HierarchyObjectives
                                        If Action.IsFeedback And App.ActiveProject.ProjectManager.FeedbackOn Then
                                            tH = App.ActiveProject.HierarchyObjectives
                                        Else
                                            If data.EdgeMeasurementScale IsNot Nothing OrElse data.Node.IsTerminalNode Then tH = App.ActiveProject.HierarchyAlternatives Else tH = App.ActiveProject.HierarchyObjectives
                                        End If

                                        tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(tH.GetNodeByID(tDirect.NodeID), fCanBePathInteractive)) ' D2830
                                        If App.isRiskEnabled Then
                                            If isImpact Then
                                                sRes = CStr(IIf(data.Node.Level > 0, "lblEvaluationDirectImpact", "lblEvaluationDirectImpactNoLevels"))
                                            Else
                                                sRes = CStr(IIf(data.Node.Level = 0, "lblEvaluationDirectRiskNoObj", "lblEvaluationDirectRisk")) ' D2361 + D2540
                                            End If
                                            tParams.Add(_TEMPL_NODETYPE, CStr(IIf(data.Node.IsTerminalNode, _TEMPL_ALTERNATIVE, _TEMPL_OBJECTIVE)))   ' D2540
                                        Else
                                            sRes = "lblEvaluationDirect"
                                        End If
                                        sRes = ParseStringTemplates(ResString(sRes), tParams)

                                        ' D1636 ===
                                    Case ECMeasureType.mtStep
                                        ' D2334 ===
                                        Dim tStep As clsStepMeasureData = CType(data.Judgment, clsStepMeasureData)
                                        Dim tParentNode As clsNode = CType(data.Node.Hierarchy.GetNodeByID(tStep.ParentNodeID), clsNode)
                                        Dim tAlt As clsNode
                                        If data.EdgeMeasurementScale IsNot Nothing OrElse tParentNode.IsTerminalNode Then
                                            tAlt = data.Node.Hierarchy.ProjectManager.AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(tStep.NodeID)
                                        Else
                                            tAlt = data.Node.Hierarchy.GetNodeByID(tStep.NodeID)
                                        End If

                                        tParams.Add(_TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode, fCanBePathInteractive))  ' D2830
                                        tParams.Add(_TEMPL_NODENAME, JS_SafeHTML(tAlt.NodeName))
                                        ' D2408 ===
                                        If App.isRiskEnabled Then
                                            If isImpact Then
                                                sRes = CStr(IIf(tParentNode.ParentNode Is Nothing, "lblEvaluationStepGoalImpact", "lblEvaluationStepImpact"))
                                            Else
                                                sRes = CStr(IIf(tParentNode.ParentNode Is Nothing, "lblEvaluationStepGoalRisk", "lblEvaluationStepRisk"))
                                            End If
                                        Else
                                            sRes = "lblEvaluationStep"
                                        End If
                                        ' D2408 ==
                                        sRes = ParseStringTemplates(ResString(sRes), tParams)
                                        ' D2334 ==

                                        ' D1790 + D2334 ===
                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                                        Dim tParentNode As clsNode = CType(data.Node.Hierarchy.GetNodeByID(CType(data.Judgment, clsUtilityCurveMeasureData).ParentNodeID), clsNode)
                                        tParams.Add(_TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode, fCanBePathInteractive))  ' D2830
                                        Dim tAlt As clsNode
                                        If data.EdgeMeasurementScale IsNot Nothing OrElse tParentNode.IsTerminalNode Then
                                            tAlt = data.Node.Hierarchy.ProjectManager.AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(CType(data.Judgment, clsUtilityCurveMeasureData).NodeID)
                                        Else
                                            tAlt = data.Node.Hierarchy.GetNodeByID(CType(data.Judgment, clsUtilityCurveMeasureData).NodeID)
                                        End If
                                        tParams.Add(_TEMPL_NODENAME, JS_SafeHTML(tAlt.NodeName))
                                        Select Case data.MeasurementType
                                            Case ECMeasureType.mtAdvancedUtilityCurve
                                                sRes = "task_AdvancedUtilityCurve"
                                            Case Else
                                                ' D2405 ===
                                                If App.isRiskEnabled Then
                                                    If isImpact Then
                                                        sRes = CStr(IIf(tParentNode.ParentNode Is Nothing, "lblEvaluationUCGoalImpact", "lblEvaluationUCImpact"))
                                                    Else
                                                        sRes = CStr(IIf(tParentNode.ParentNode Is Nothing, "lblEvaluationUCGoalRisk", "lblEvaluationUCRisk"))
                                                    End If
                                                Else
                                                    sRes = "lblEvaluationUC"
                                                End If
                                                ' D2405 ==
                                        End Select
                                        sRes = ParseStringTemplates(ResString(sRes), tParams)
                                        ' D2334 ==

                                    Case Else
                                        ' D0120 ===
                                        ' D2589 ===
                                        Dim isAlt As Boolean = data.EdgeMeasurementScale IsNot Nothing OrElse (data.Node IsNot Nothing AndAlso (data.Node.IsAlternative OrElse data.Node.IsTerminalNode))    ' D2530 + D2587
                                        Dim tData As clsNonPairwiseMeasureData = CType(data.Judgment, clsNonPairwiseMeasureData)
                                        Dim tNode As clsNode = Nothing
                                        If isAlt Then   ' D2530
                                            tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tData.NodeID)
                                        Else
                                            tNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(tData.NodeID)
                                            If tNode Is Nothing Then
                                                tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tData.NodeID)
                                                isAlt = True
                                            End If
                                        End If
                                        If tNode IsNot Nothing Then tParams.Add(_TEMPL_NODE_A, JS_SafeHTML(tNode.NodeName))
                                        ' D2589 ==
                                        tParams.Add(_TEMPL_NODE_B, JS_SafeHTML(data.Node.NodeName))
                                        ' D2258 ===
                                        Dim fHasLevels As Boolean = data.Node.Level > 0
                                        If App.isRiskEnabled Then
                                            If isImpact Then
                                                sRes = CStr(IIf(fHasLevels, "lblEvaluationRatingImpact", "lblEvaluationRatingImpactNoLevels"))
                                            Else
                                                sRes = CStr(IIf(fHasLevels, "lblEvaluationRatingRisk", "lblEvaluationRatingNoLevelsRisk"))
                                            End If
                                        Else
                                            sRes = CStr(IIf(fHasLevels, "lblEvaluationRating", "lblEvaluationRatingNoLevels"))
                                        End If
                                        If data.Node IsNot Nothing AndAlso Not isAlt Then sRes += "Obj" ' D2527 + D2530
                                        sRes = ParseStringTemplates(ResString(sRes), tParams)   ' D2320
                                        ' D2258 ==
                                        ' D0120 ==
                                End Select
                                ' D0312 ==
                            End If
                        End If
                        ' D0038 ==

                        ' D0075 ===
                    Case ActionType.atNonPWAllChildren  'Cv2 + D0677
                        Dim data As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData) 'Cv2 'C0464 + D0677
                        If data IsNot Nothing AndAlso data.ParentNode IsNot Nothing Then    ' D1308

                            Select Case data.MeasurementType
                                Case ECMeasureType.mtRatings
                                    If App.isRiskEnabled Then
                                        If isImpact Then
                                            sRes = CStr(IIf(data.ParentNode.IsAlternative, "task_MultiRatings_AllCovObjImpact", IIf(data.ParentNode.ParentNode Is Nothing, "task_MultiRatings_AllAltsGoalImpact", "task_MultiRatings_AllAltsImpact")))
                                        Else
                                            sRes = CStr(IIf(data.ParentNode Is Nothing OrElse data.ParentNode.ParentNode Is Nothing, "lblEvaluationMultiDirectDataLikelihood", IIf(data.ParentNode.IsTerminalNode, "task_MultiRatings_AllAltsRisk", IIf(data.ParentNode.RiskNodeType = RiskNodeType.ntCategory, "task_MultiRatings_AllObjRisk_Cat", "task_MultiRatings_AllObjRisk")))) ' D2318 + D2319 + D2964
                                        End If
                                        If Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1 Then sRes = "task_MultiRatings_AllAlts_NoObj"
                                    Else
                                        sRes = "task_MultiRatings_AllAlts"
                                    End If


                                Case ECMeasureType.mtDirect
                                    If App.isRiskEnabled Then
                                        If data.ParentNode.IsTerminalNode Then
                                            ' D2355 ===
                                            If isImpact Then
                                                sRes = CStr(IIf(data.ParentNode.Level = 0, "lblEvaluationMultiDirectDataAltsGoalRisk", "lblEvaluationMultiDirectDataAltsRisk")) ' D2399
                                            Else
                                                sRes = CStr(IIf(data.ParentNode.Level = 0, "lblEvaluationMultiDirectDataAltsGoalLikelihood", "lblEvaluationMultiDirectDataAltsLikelihood"))
                                            End If
                                            ' D2355 ==
                                        Else
                                            If Not isImpact Then
                                                sRes = CStr(IIf(data.ParentNode.Level > 0, "lblEvaluationMultiDirectDataLevelsLikelihood", "lblEvaluationMultiDirectDataLikelihood"))
                                            Else
                                                sRes = CStr(IIf(data.ParentNode.ParentNode Is Nothing, "lblEvaluationMultiDirectDataGoalRisk", "lblEvaluationMultiDirectDataRiskObj"))
                                            End If
                                        End If
                                    Else
                                        If data.ParentNode.IsTerminalNode Then
                                            sRes = "lblEvaluationMultiDirectDataAlts"
                                        Else
                                            sRes = "lblEvaluationMultiDirectData"
                                        End If
                                    End If

                            End Select

                            tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.ParentNode, fCanBePathInteractive))   ' D2830
                            tParams.Add(_TEMPL_EVALCOUNT, CStr(data.Children.Count))
                            sRes = ParseStringTemplates(ResString(sRes), tParams)
                        End If

                    Case ActionType.atNonPWAllCovObjs 'Cv2
                        Dim data As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData) 'Cv2 'C0464
                        If Not data Is Nothing Then
                            ' D0120 ===
                            tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.Alternative, fCanBePathInteractive))     ' D0558 +  D2830
                            ' D1931 + D2360 ===
                            If data.MeasurementType = ECMeasureType.mtDirect Then
                                If App.isRiskEnabled Then
                                    sRes = CStr(IIf(Hierarchy.Nodes.Count <= 1, "lblEvaluationAllCovObjsRiskNoObj", "lblEvaluationAllCovObjsRisk"))
                                Else
                                    sRes = "lblEvaluationAllCovObjs"
                                End If
                            End If
                            ' D2360 ==
                            If data.MeasurementType = ECMeasureType.mtRatings Then
                                If App.isRiskEnabled Then
                                    If isImpact Then
                                        sRes = CStr(IIf(App.ActiveProject.HierarchyObjectives.GetMaxLevel < 1, "task_MultiRatings_AllCovObjGoalImpact", "task_MultiRatings_AllCovObjImpact"))
                                    Else
                                        sRes = CStr(IIf(App.ActiveProject.HierarchyObjectives.GetMaxLevel < 1, "task_MultiRatings_AllCovObjGoal", "task_MultiRatings_AllCovObj"))   ' D1939
                                    End If
                                Else
                                    sRes = "lblEvaluationAllCovObjsRatings" ' D2327
                                End If
                            End If
                            sRes = ParseStringTemplates(ResString(sRes), tParams)  ' D1636 + D1770 + D2195 + D2320
                            ' D0120 + D1931 ==
                        End If

                        ' D3250 ===
                    Case ActionType.atAllEventsWithNoSource
                        Dim data As clsAllEventsWithNoSourceEvaluationActionData = CType(Action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)
                        If Not data Is Nothing Then
                            Select Case data.MeasurementType
                                Case ECMeasureType.mtRatings, ECMeasureType.mtDirect
                                    sRes = "lblEvaluationNoSources"
                            End Select
                        End If
                        If sRes <> "" Then sRes = ParseStringTemplates(ResString(sRes), tParams)
                        ' D3250 ==

                        ' D0152 + D0182 ===
                    Case ActionType.atSensitivityAnalysis
                        Select Case CType(Action.ActionData, clsSensitivityAnalysisActionData).SAType 'C0464
                            Case SAType.satDynamic
                                sRes = ResString("lblEvaluationDynamicSA")  ' D1770
                            Case SAType.satGradient
                                sRes = ResString("lblEvaluationGradientSA") ' D1770
                            Case SAType.satPerformance
                                sRes = ResString("lblEvaluationPerformanceSA")  ' D1770
                        End Select
                        ' D0152 + D0182 ==

                End Select
            End If
            Return PrepareTask(sRes, tExtraParam)    ' D1770 + D2258
        End Function

        ' D0558 + D2776 ===
        Public Function GetWRTNodeNameWithPath(ByVal tNode As clsNode, CanBeInteractive As Boolean) As String
            Dim sName As String = ""
            If tNode IsNot Nothing Then
                sName = JS_SafeHTML(tNode.NodeName)
                If App.ActiveProject.PipeParameters.ShowFullObjectivePath <> ecShowObjectivePath.DontShowPath Then  ' D4100
                    Dim sDivider As String = JS_SafeHTML(ResString("lblObjectivePathDivider"))
                    Dim sPath As String = ""
                    While tNode.ParentNode IsNot Nothing
                        If tNode.ParentNode.ParentNode IsNot Nothing Then sPath = JS_SafeHTML(tNode.ParentNode.NodeName) + sDivider + sPath ' D3700
                        tNode = tNode.ParentNode
                    End While

                    'If Not App.ActiveProject.PipeParameters.ShowFullObjectivePath AndAlso CanBeInteractive AndAlso sPath <> "" Then
                    If App.ActiveProject.PipeParameters.ShowFullObjectivePath = ecShowObjectivePath.CollapsePath AndAlso CanBeInteractive AndAlso sPath <> "" Then  ' D4100
                        If sOldWRTPath Is Nothing Then sOldWRTPath = SessVar(SESS_WRT_PATH)
                        'Dim fCanSee As Boolean = App.ActiveProject.PipeParameters.ShowFullObjectivePath OrElse Not String.Equals(sPath, sOldWRTPath)    ' D3533
                        Dim fCanSee As Boolean = Not String.Equals(sPath, sOldWRTPath) ' -D3533
                        'sName = String.Format("<span onmouseover=""this.title='{3}';"" onclick='ToggleWRTPath();' class='wrt_link'><span id='wrt_path' class='wrt_path'{2}>{0}</span>{1}</span>", sPath, sName, IIf(fCanSee, "", " style='display:none;'"), JS_SafeString((sPath + sName).Replace("""", "&#39;")))  ' D2830
                        sName = String.Format("<span onmouseover=""this.title='{3}';"" onclick='ToggleWRTPath();' class='wrt_link'><span id='wrt_path' class='wrt_path'{2}>{0}</span>{1}</span>", sPath, sName, "", JS_SafeString((sPath + sName).Replace("""", "&#39;")))  ' D2830 + D3533
                        'If Not App.ActiveProject.PipeParameters.ShowFullObjectivePath Then ' -D4100
                        If fCanSee Then
                            ClientScript.RegisterStartupScript(GetType(String), "InitWRTPath", "setTimeout('DoFlashWRTLink(""wrt_path"");', 1500);", True)  ' D3533
                            SessVar(SESS_WRT_PATH) = sPath
                        Else
                            ClientScript.RegisterStartupScript(GetType(String), "InitWRTPathLink", "setTimeout('DoFlashWRTPath();', 1500);", True)  ' D3533
                        End If
                        'End If
                    Else
                        'If App.ActiveProject.PipeParameters.ShowFullObjectivePath Then Return sPath + sName Else Return sName
                        If App.ActiveProject.PipeParameters.ShowFullObjectivePath = ecShowObjectivePath.AlwaysShowFull Then sName = sPath + sName ' D4100
                    End If
                End If
            End If
            Return sName
        End Function
        ' D0558 + D1653 + D2776 ==

        ' D4078 ===
        Public Function GetMessageByBaronError() As String
            Dim sResult As String = ""
            If Baron_Error_Code <> BaronErrorCode.None Then
                sResult = String.Format(ResString("errBaron" + Baron_Error_Code.ToString), Baron_Error_Details)
                sResult = sResult.Replace(Server.MapPath("/"), "")
                sResult = sResult.Replace(System.IO.Path.GetTempPath(), "")
            End If
            Return sResult
        End Function
        ' D4078 ==

        ' D1304 ===
        ' Dmitriy Alekseenko: See Shell\MainPage.xaml.vb  --  Public Sub ShowEvaluationPipeHelp
        Public Function GetEvaluationHelpPageName(ByVal helpID As Integer, fIsLIkelihood As Boolean) As String
            Dim sURL As String = ""
            Select Case helpID
                Case 0
                    sURL = "navAnytimeInstructions_help"
                Case 1
                    sURL = "navCollectInputParticipantDirect_help"
                Case 2
                    sURL = "navCollectInputParticipantPWGraphical_help"
                Case 3
                    sURL = "navCollectInputParticipantPWVerbal_help"
                Case 4
                    sURL = "navCollectInputParticipantRatings_help"
                Case 5
                    sURL = "navCollectInputParticipantStepFunction_help"
                Case 6
                    sURL = "navCollectInputParticipantUtilityCurve_help"
                Case 8
                    sURL = "navCollectInputDSA_help"
                Case 9
                    sURL = "navCollectInputGSA_help"
                Case 10
                    sURL = "navCollectInputLocalResults_help"
                Case 11
                    sURL = "navCollectInputPSA_help"
                Case 12
                    sURL = "navCollectInputSurvey_help"
                Case 13
                    sURL = "navCollectInputThankYou_help"
                Case 14
                    sURL = "navCollectInputWelcome_help"
                Case 15
                    sURL = "navCollectInputGlobalResults_help"
                    ' D1461 ===
                Case 16
                    sURL = "navCollectInputMultiVerbalPW_help"
                Case 17
                    sURL = "navCollectInputMultiGraphicalPW_help"   ' D2077
                    ' D1461 ==
            End Select
            ' D2258 ===
            If App.isRiskEnabled Then
                sURL = ResString("navHelpBaseRisk") + ResString("navHelpStartRisk") + ResString(sURL).Replace("navHelpBaseRisk", "")
            Else
                sURL = ResString("navHelpBase") + ResString("navHelpStart") + ResString(sURL).Replace("navHelpBase", "")
            End If
            Dim sRiskAdd As String = ""
            If App.isRiskEnabled Then sRiskAdd = ResString(CStr(IIf(fIsLIkelihood, "navHelpLikelihood", "navHelpImpact")))
            Return String.Format(sURL, sRiskAdd)  ' D2010
            ' D2258 ==
        End Function
        ' D1304 ==


        ' D3738 ===
        Public Function CheckShow_QuickHelp(sQuickHelpContent As String) As Boolean
            Dim fShowQH As Boolean = False
            If clsPipeMessages.OPT_QUICK_HELP_AVAILABLE Then    ' D4077
                If clsPipeMessages.OPT_QUICK_HELP_AUTO_SHOW_ONCE Then   ' D6670
                    Dim QH_list As Dictionary(Of Integer, List(Of String))
                    If Session(SESS_QH_MD5) IsNot Nothing Then
                        QH_list = CType(Session(SESS_QH_MD5), Dictionary(Of Integer, List(Of String)))
                    Else
                        QH_list = New Dictionary(Of Integer, List(Of String))
                    End If
                    Dim Lst As List(Of String)
                    If QH_list.ContainsKey(App.ProjectID) Then
                        Lst = QH_list(App.ProjectID)
                    Else
                        Lst = New List(Of String)
                        QH_list.Add(App.ProjectID, Lst)
                    End If
                    Dim sMD5 As String = GetMD5(sQuickHelpContent)
                    If Not Lst.Contains(sMD5) Then
                        Lst.Add(sMD5)
                        fShowQH = True
                    End If
                    If fShowQH Then Session(SESS_QH_MD5) = QH_list
                Else
                    fShowQH = True  ' D6670
                End If
            End If
            Return fShowQH
        End Function
        ' D3738 ==

        ' D0136 ===
        Public ReadOnly Property BlankImage() As String ' D0146
            Get
                'Return ThemePath + _FILE_IMAGE_BLANK
                Return String.Format("/{0}", _FILE_IMAGE_BLANK)  ' D4720 + D4944
            End Get
        End Property
        ' D0136 ==

        ' D1503 + D2327 ===
        Public Function PrepareTask(sTask As String, Optional tExtraParam As Object = Nothing, Optional fHasSubNodes As Boolean = False, Optional ByRef tUsedParams As Generic.Dictionary(Of String, String) = Nothing) As String ' D1862 + D2364
            Return clsComparionCorePage.PrepareTaskCommon(App, sTask, tExtraParam, fHasSubNodes)
        End Function

        Shared Function PrepareTaskCommon(tApp As clsComparionCore, sTask As String, Optional tExtraParam As Object = Nothing, Optional fHasSubNodes As Boolean = False, Optional tProject As clsProject = Nothing) As String ' D1862
            Dim sObjectives As String = ""
            Dim sObjective As String = ""
            Dim sAlternatives As String = ""
            Dim sAlternative As String = ""
            Dim sPromtObj As String = ""    ' D1819
            Dim sPromtAlt As String = ""    ' D1819
            If sTask IsNot Nothing AndAlso tApp IsNot Nothing Then
                Dim fIsImpact As Boolean = False    ' D2141
                Dim fNeedOtherHIDParse As Boolean = False
                If tProject Is Nothing Then tProject = tApp.ActiveProject
                If tProject IsNot Nothing AndAlso tProject.isValidDBVersion Then    ' D3314

                    ' D0237 ===
                    With tProject.ProjectManager.PipeParameters

                        Dim OldParams As ParameterSet = .CurrentParameterSet

                        Dim sHid As String = ""
                        If tProject.IsRisk Then   ' D3692
                            If tProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact Then
                                sHid = "_Impact"    ' D2292
                                If .CurrentParameterSet IsNot .ImpactParameterSet Then .CurrentParameterSet = .ImpactParameterSet
                                fIsImpact = True    ' D2141
                            End If
                            If tProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood Then
                                sHid = "_Likelihood"    ' D2292
                                If .CurrentParameterSet IsNot .DefaultParameterSet Then .CurrentParameterSet = .DefaultParameterSet
                            End If
                        End If
                        ' D2037 ==

                        sAlternatives = .NameAlternatives
                        sObjectives = .NameObjectives

                        ' D1819 ===
                        If .JudgementPromt <> "" Then
                            sPromtObj = .JudgementPromt
                        Else
                            ' D1822 ===
                            Dim sTempl As String = "lbl_promt_obj{1}_{0}"  ' D1822 + D2292
                            sPromtObj = tApp.CurrentLanguage.GetString(String.Format(sTempl, IIf(.JudgementPromtID < 0, 0, .JudgementPromtID), sHid), "")
                            If sPromtObj = "" Then sPromtObj = tApp.ResString(String.Format(sTempl, 0, sHid))
                            ' D1822 ==
                        End If

                        If .JudgementAltsPromt <> "" Then
                            sPromtAlt = .JudgementAltsPromt
                        Else
                            ' D1822 ===
                            Dim sTempl As String = "lbl_promt_alt{1}_{0}"   ' D2292
                            sPromtAlt = tApp.CurrentLanguage.GetString(String.Format(sTempl, IIf(.JudgementAltsPromtID < 0, 0, .JudgementAltsPromtID), sHid), "")
                            If sPromtAlt = "" Then sPromtAlt = tApp.ResString(String.Format(sTempl, 0, sHid))
                            ' D1822 ==
                        End If

                        .CurrentParameterSet = OldParams    ' D2037

                    End With
                    ' D1819 ==

                    ' D6080 ===
                    If tApp.isRiskEnabled Then
                        ' replace %%*(x)%% specific template as regular %%*%% for a current hierarchy;
                        Dim sCurrent As String = String.Format("({0})", If(fIsImpact, "i", "l"))
                        Dim sOther As String = String.Format("({0})", If(fIsImpact, "l", "i"))
                        If sTask.IndexOf(sCurrent + "%%", StringComparison.CurrentCultureIgnoreCase) > 0 Then sTask = sTask.Replace(sCurrent.ToLower, "").Replace(sCurrent.ToUpper, "")
                        If sTask.IndexOf(sOther + "%%", StringComparison.CurrentCultureIgnoreCase) > 0 Then fNeedOtherHIDParse = True
                    End If
                    ' D6080 ==
                End If

                ' D2427 ===
                If tApp.isRiskEnabled AndAlso tApp.ActiveWorkgroup IsNot Nothing AndAlso tApp.ActiveWorkgroup.WordingTemplates IsNot Nothing Then
                    If String.IsNullOrEmpty(sAlternatives) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_RISK_EVENTS) Then sAlternatives = tApp.ActiveWorkgroup.WordingTemplates(_TPL_RISK_EVENTS)

                    If String.IsNullOrEmpty(sAlternative) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_RISK_EVENT) Then sAlternative = tApp.ActiveWorkgroup.WordingTemplates(_TPL_RISK_EVENT)
                    If tProject IsNot Nothing AndAlso tProject.WordingTemplates.ContainsKey(_TPL_RISK_EVENT) AndAlso tProject.WordingTemplates(_TPL_RISK_EVENT).InUse Then sAlternative = tProject.WordingTemplates(_TPL_RISK_EVENT).Value 'A2107

                    Dim sObjName As String = If(fIsImpact, _TPL_RISK_CONSEQUENCES, _TPL_RISK_SOURCES)    ' D2464
                    If String.IsNullOrEmpty(sObjectives) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(sObjName) Then sObjectives = tApp.ActiveWorkgroup.WordingTemplates(sObjName)
                    sObjName = If(fIsImpact, _TPL_RISK_CONSEQUENCE, _TPL_RISK_SOURCE)    ' D2464

                    If String.IsNullOrEmpty(sObjective) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(sObjName) Then sObjective = tApp.ActiveWorkgroup.WordingTemplates(sObjName)
                    If tProject IsNot Nothing AndAlso tProject.WordingTemplates.ContainsKey(sObjName) AndAlso tProject.WordingTemplates(sObjName).InUse Then sObjective = tProject.WordingTemplates(sObjName).Value 'A2107
                End If
                ' D4979 ===
                If Not tApp.isRiskEnabled AndAlso tApp.ActiveWorkgroup IsNot Nothing AndAlso tApp.ActiveWorkgroup.WordingTemplates IsNot Nothing Then
                    If String.IsNullOrEmpty(sAlternatives) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_COMPARION_ALTS) Then sAlternatives = tApp.ActiveWorkgroup.WordingTemplates(_TPL_COMPARION_ALTS)
                    If String.IsNullOrEmpty(sAlternative) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_COMPARION_ALT) Then sAlternative = tApp.ActiveWorkgroup.WordingTemplates(_TPL_COMPARION_ALT)
                    If String.IsNullOrEmpty(sObjectives) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_COMPARION_OBJS) Then sObjectives = tApp.ActiveWorkgroup.WordingTemplates(_TPL_COMPARION_OBJS)
                    If String.IsNullOrEmpty(sObjective) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_COMPARION_OBJ) Then sObjective = tApp.ActiveWorkgroup.WordingTemplates(_TPL_COMPARION_OBJ)

                    If tProject IsNot Nothing AndAlso tProject.WordingTemplates.ContainsKey(_TPL_COMPARION_ALT) AndAlso tProject.WordingTemplates(_TPL_COMPARION_ALT).InUse Then sAlternative = tProject.WordingTemplates(_TPL_COMPARION_ALT).Value 'A2107
                    If tProject IsNot Nothing AndAlso tProject.WordingTemplates.ContainsKey(_TPL_COMPARION_OBJ) AndAlso tProject.WordingTemplates(_TPL_COMPARION_OBJ).InUse Then sObjective = tProject.WordingTemplates(_TPL_COMPARION_OBJ).Value 'A2107
                End If
                ' D2427 + D4979 ==

                ' D2258 ===
                If String.IsNullOrEmpty(sAlternatives) Then sAlternatives = tApp.ResString(CStr(IIf(tApp.isRiskEnabled, "templ_AlternativesRisk", "templ_Alternatives")))
                If String.IsNullOrEmpty(sAlternative) Then sAlternative = tApp.ResString(CStr(IIf(tApp.isRiskEnabled, "templ_AlternativeRisk", "templ_Alternative")))
                If String.IsNullOrEmpty(sObjectives) OrElse fHasSubNodes Then sObjectives = tApp.ResString(CStr(IIf(tApp.isRiskEnabled, IIf(fIsImpact, "templ_Objectives_Impact", "templ_ObjectivesRisk"), "templ_Objectives"))) ' D1770 + D2141
                If String.IsNullOrEmpty(sObjective) OrElse fHasSubNodes Then sObjective = tApp.ResString(CStr(IIf(tApp.isRiskEnabled, IIf(fIsImpact, "templ_Objective_Impact", "templ_ObjectiveRisk"), "templ_Objective"))) ' D1770 + D2141
                ' D2258 ==

                ' D6813 ===
                If _OPT_RR_CUSTOM_WORDING AndAlso tApp.isRiskEnabled AndAlso tProject IsNot Nothing AndAlso tProject.isMyRiskRewardModel Then   ' D6921
                    ' D6827 ===
                    'If sObjectives.Equals(tApp.ResString("lblScenarios"), StringComparison.CurrentCultureIgnoreCase) Then sObjective = tApp.ResString("lblScenario")
                    For idx As Integer = 0 To 9
                        Dim sSingle As String = String.Format("lblRR_Wording{0}", idx)
                        Dim sPlural As String = String.Format("lblRR_Wordings{0}", idx)
                        If tApp.CurrentLanguage.Resources.ParameterExists(sSingle) Then
                            If sObjectives.Equals(tApp.ResString(sPlural), StringComparison.CurrentCultureIgnoreCase) Then sObjective = tApp.ResString(sSingle)
                        End If
                    Next
                    ' D6827 ==
                End If
                ' D6813 ==

                ' D1826 + D2071 ===
                If tExtraParam IsNot Nothing AndAlso (TypeOf (tExtraParam) Is clsStepFunction OrElse TypeOf (tExtraParam) Is clsRatingScale) Then
                    If TypeOf (tExtraParam) Is clsStepFunction Then
                        sObjective = tApp.ResString("templ_Interval")
                        sObjectives = tApp.ResString("templ_Intervals")
                    Else
                        sObjective = tApp.ResString("templ_Intensity")
                        sObjectives = tApp.ResString("templ_Intensities")
                        ' D1862 ===
                    End If
                    ' D2071 ==
                    sAlternative = sObjective
                    sAlternatives = sObjectives
                    ' D1862 ==
                End If
                ' D1826 ==

                Dim tParams As New Generic.Dictionary(Of String, String)
                ' D4979 === 'AD: Remove toLowercase by DA / A1027
                tParams.Add(_TEMPL_ALTERNATIVES, JS_SafeHTML(sAlternatives))
                tParams.Add(_TEMPL_ALTERNATIVE, JS_SafeHTML(sAlternative))
                tParams.Add(_TEMPL_OBJECTIVES, JS_SafeHTML(sObjectives))
                tParams.Add(_TEMPL_OBJECTIVE, JS_SafeHTML(sObjective))
                If tProject IsNot Nothing Then   ' D6080 + D6736
                    ' D4979 ==
                    If sTask.ToLower.Contains("%%promt_") Then      ' D2457
                        tParams.Add(_TEMPL_PROMT_OBJ, sPromtObj)    ' D1819
                        tParams.Add(_TEMPL_PROMT_ALT, sPromtAlt)    ' D1819
                        tParams.Add(_TEMPL_PROMT_ALT_WORD, clsComparionCorePage.GetPromptWordCommon(tApp, True, tProject))    ' D2457
                        tParams.Add(_TEMPL_PROMT_OBJ_WORD, clsComparionCorePage.GetPromptWordCommon(tApp, False, tProject))   ' D2457
                    End If
                    ' D2320 ===
                    If sTask.Contains("wording%%") Then
                        tParams.Add(_TEMPL_RATE_WORDING, clsComparionCorePage.GetRatingAltsWordingCommon(tApp, True, tProject))
                        tParams.Add(_TEMPL_RATE_OBJ_WORDING, clsComparionCorePage.GetRatingObjWordingCommon(tApp, True, tProject))    ' D2449
                        tParams.Add(_TEMPL_EST_WORDING, clsComparionCorePage.GetRatingAltsWordingCommon(tApp, False, tProject))
                        tParams.Add(_TEMPL_EST_OBJ_WORDING, clsComparionCorePage.GetRatingObjWordingCommon(tApp, False, tProject))    ' D2449
                    End If
                    ' D2320 ==
                End If

                tParams.Add(_TEMPL_EVAL_OBJECT, "<span id='evalObject'></span>")    ' D6957
                sTask = ParseStringTemplates(sTask, tParams)

                ' D6080 ===
                If fNeedOtherHIDParse AndAlso tProject IsNot Nothing Then  ' D6736
                    With tProject.ProjectManager
                        Dim OldHID As Integer = .ActiveHierarchy
                        .ActiveHierarchy = If(fIsImpact, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
                        sTask = PrepareTaskCommon(tApp, sTask, tExtraParam, fHasSubNodes, tProject)
                        .ActiveHierarchy = OldHID
                    End With
                End If
                ' D6080 ==

                'If tUsedParams IsNot Nothing Then tUsedParams = tParams ' D2364
                ' D2429 ===
                If sTask.Contains("%%") AndAlso tApp.isRiskEnabled AndAlso tApp.ActiveWorkgroup IsNot Nothing AndAlso tApp.ActiveWorkgroup.WordingTemplates IsNot Nothing Then
                    For Each sName As String In tApp.ActiveWorkgroup.WordingTemplates.Keys
                        If sTask.ToLower.Contains(sName.ToLower) Then
                            sTask = clsComparionCorePage.ParseTemplateCommon(sTask, sName, tApp.ActiveWorkgroup.WordingTemplates(sName), False)
                        End If
                    Next
                End If
                ' D2429 ==
            End If
            Return sTask
        End Function
        ' D1503 + D2327 ==

        ' D0790 ===
        Public Function GetPeriodString(ByVal tDate As DateTime) As String
            Dim diff As Long = DateTime.Now.Ticks - tDate.Ticks
            Dim sDiff As String = ""
            If diff < TimeSpan.TicksPerMinute Then
                sDiff = ResString("lblPeriodMoment")
            Else
                If diff < TimeSpan.TicksPerHour Then
                    sDiff = String.Format(ResString("lblPeriodMinutes"), Math.Ceiling(diff / TimeSpan.TicksPerMinute))
                Else
                    If diff < TimeSpan.TicksPerDay Then
                        sDiff = String.Format(ResString("lblPeriodHours"), Math.Ceiling(diff / TimeSpan.TicksPerHour))
                    Else
                        If diff < 60 * TimeSpan.TicksPerDay Then
                            sDiff = String.Format(ResString("lblPeriodDays"), Math.Ceiling(diff / TimeSpan.TicksPerDay))
                        Else
                            sDiff = tDate.ToString("yy-MM-dd")  ' D6443
                            'sDiff = String.Format(ResString("lblPeriodMonths"), Math.Ceiling(diff / (60 * TimeSpan.TicksPerDay)))
                        End If
                    End If
                End If
            End If
            Return sDiff
        End Function
        ' D0790 ==

        ' D6018 ===
        Public ReadOnly Property GetMaxUploadSize As Integer
            Get
                Dim Size As Integer = 30 * 1024 * 1024
                Dim section As System.Web.Configuration.HttpRuntimeSection = CType(System.Configuration.ConfigurationManager.GetSection("system.web/httpRuntime"), System.Web.Configuration.HttpRuntimeSection)
                If section IsNot Nothing Then
                    Size = section.MaxRequestLength * 1024
                End If
                Return Size
            End Get
        End Property
        ' D6108 ==

        ' D0216 ===
        Public Function CreateImageScale(ByVal ScaleType As String) As clsPairwiseData
            Dim VerbalScale As New clsPairwiseData

            ' D7595 ===
            Select Case ScaleType.ToLower
                Case clsPairwiseData.Scale_Tiny, clsPairwiseData.Scale_Tiny_Up, clsPairwiseData.Scale_Small
                    VerbalScale.ScaleType = ScaleType
                Case Else
                    VerbalScale.ScaleType = clsPairwiseData.Scale_Regular
            End Select
            ' D7595 ==

            clsPairwiseData.VerbalHints(0) = ResString("lblEvaluationPWHintEqual")
            clsPairwiseData.VerbalHints(1) = ResString("lblEvaluationPWHintModerately")
            clsPairwiseData.VerbalHints(2) = ResString("lblEvaluationPWHintStrongly")
            clsPairwiseData.VerbalHints(3) = ResString("lblEvaluationPWHintVeryStrongly")
            clsPairwiseData.VerbalHints(4) = ResString("lblEvaluationPWHintExtremely")

            clsPairwiseData.VerbalTinyHints(0) = ResString("lblEvaluationPWShortHintEqual")
            clsPairwiseData.VerbalTinyHints(1) = ResString("lblEvaluationPWShortHintModerately")
            clsPairwiseData.VerbalTinyHints(2) = ResString("lblEvaluationPWShortHintStrongly")
            clsPairwiseData.VerbalTinyHints(3) = ResString("lblEvaluationPWShortHintVeryStrongly")
            clsPairwiseData.VerbalTinyHints(4) = ResString("lblEvaluationPWShortHintExtremely")

            Return VerbalScale
        End Function
        ' D0216 ==

        ' D0103 ===
        Public Function LoadingMessage(Optional ByVal sLoadingMessage As String = "", Optional ByVal fShowOnlyImage As Boolean = False, Optional ByVal fShowAsDiv As Boolean = False) As String
            If sLoadingMessage = "" Then sLoadingMessage = ResString("msgLoading")
            Dim sImg As String = String.Format("<img src='{0}devex_loading.gif' width='16' height='16' border='0' title='{1}'>", ImagePath, sLoadingMessage)
            If Not fShowOnlyImage Then sImg += String.Format("&nbsp;&nbsp;{0}", sLoadingMessage)
            If fShowAsDiv Then sImg = String.Format("<div style='text-align:center' class='gray'>{0}</div>", sImg)
            Return sImg
        End Function
        ' D0103 ==

        Public Sub FetchAccess(Optional ByVal Redirect2PageID As Integer = _PGID_UNKNOWN)
            DebugInfo(String.Format("Request fetch access for page '{0}'", CurrentPageID), _TRACE_WARNING) ' D4326
            ' D0043 ===
            If Redirect2PageID <> _PGID_UNKNOWN AndAlso Not HasPermission(Redirect2PageID, App.ActiveProject) Then Redirect2PageID = _PGID_UNKNOWN ' D0262 + D0466
            If Redirect2PageID = _PGID_UNKNOWN Then
                ' D0262 ===
                If Not App.ActiveWorkgroup Is Nothing Then
                    If App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then Redirect2PageID = _DEF_PGID_ONSYSTEMWORKGROUP
                End If
                If Redirect2PageID = _PGID_UNKNOWN AndAlso App.isAuthorized Then    ' D0492
                    Redirect2PageID = _DEF_PGID_ONPROJECTS ' D0492 + D0818 + D4640
                End If
                ' D0262 ==
            End If
            If CurrentPageID = Redirect2PageID AndAlso CurrentPageID <> _PGID_START Then Redirect2PageID = _PGID_START ' D0263
            If App.ActiveUser Is Nothing Then Redirect2PageID = _PGID_ERROR_403 ' D0492
            ' D4326 ===
            Dim sWkg As String = "-"
            If App.ActiveWorkgroup IsNot Nothing Then sWkg = App.ActiveWorkgroup.Name
            Dim sPrj As String = "-"
            If App.HasActiveProject Then sPrj = String.Format("#{0} {1}", App.ActiveProject.ID, App.ActiveProject.ProjectName)
            Dim sUsr As String = "-"
            If App.isAuthorized Then sUsr = App.ActiveUser.UserEmail
            DebugInfo(String.Format("ActiveWkg: {0}; ActiveProject: {1}; ActiveUSer: {2}", sWkg, sPrj, sUsr), _TRACE_WARNING)
            ' D4326 ==
            Dim sURL As String = PageURL(Redirect2PageID, GetTempThemeURI(False))   ' D0763 + D0766
            If sURL = "" Then sURL = _URL_ROOT
            If Redirect2PageID = _PGID_ERROR_403 Then App.ApplicationError.Init(ecErrorStatus.errAccessDenied, CurrentPageID, CStr(IIf(App.isAuthorized, "", ResString("msgRestricted"))), Nothing, Request.Url.AbsoluteUri) ' D0117 + D0402 + D0459
            ' D0043 ==
            If isSLTheme() Then sURL = PageURL(_PGID_SERVICEPAGE, _PARAM_ACTION + "=msg&_pgid=" + CurrentPageID.ToString + "&type=" + CStr(IIf(Array.IndexOf({_PGID_ERROR_403, _PGID_ERROR_404, _PGID_ERROR_500, _PGID_ERROR_503}, Redirect2PageID) >= 0, "err&pg=" + CStr(Redirect2PageID), "fetch"))) Else sURL += CStr(IIf(sURL.ToLower.Contains("temptheme"), "", IIf(sURL.IndexOf("?") > 0, "&", "?").ToString + GetTempThemeURI(False))) ' D1624 + D3565
            sURL = sURL.TrimEnd(CChar("&")).TrimEnd(CChar("?")) ' D6446
            DebugInfo(String.Format("Access restricted. Redirect to '{0}'", sURL), _TRACE_WARNING) ' D0043 + D0264
            'If IsCallback Then
            '    ' D3296 ===
            '    RawResponseStart()
            '    Response.Write("No permissions for get this page")
            '    RawResponseEnd()
            '    ' D3296 ==
            'Else
            '    Response.Redirect(sURL, True) ' D0194
            'End If
            If Not isCallback() Then
                Response.Redirect(sURL, True)
            Else
                If Not App.isAuthorized Then FetchIfNotAuthorized(False) Else FetchNoPermissions(False) ' D0194 + D3296 + D6643
            End If
        End Sub

        ' D0264 ===
        Public Sub FetchAccessByWrongLicense(Optional ByVal sMessage As String = "", Optional ByVal fOnlyWarning As Boolean = False)
            If sMessage <> "" And App.ApplicationError.Status <> ecErrorStatus.errWrongLicense Then App.LicenseInitError(sMessage, fOnlyWarning)
            DebugInfo(String.Format("Wrong license '{0}'", sMessage), _TRACE_WARNING)   ' D4326
            If CurrentPageID <> _PGID_ERROR_503 Then FetchAccess(_PGID_ERROR_503) ' D3572
        End Sub
        ' D0264 ==

        ' D6006 ===
        Private Sub TerminateWithRTEAsAJON(Status As ecErrorStatus, ex As Exception)
            Response.ClearContent()
            Response.ContentType = "application/json"   ' D5031
            Response.StatusDescription = If(Status = ecErrorStatus.errRTE, "RTE", Status.ToString)
            Response.StatusCode = 500   ' Internal error
            If ex IsNot Nothing AndAlso Not String.IsNullOrEmpty(ex.Message) Then
                Dim sText As String = GetRTEHeader()
                If sText = "" Then sText = ex.Message
                Response.Write(sText)
                If Request IsNot Nothing AndAlso Request.IsLocal AndAlso Not String.IsNullOrEmpty(ex.StackTrace) Then Response.Write(vbNewLine + vbNewLine + ex.StackTrace)
            End If
            Response.End()
        End Sub
        ' D6006 ==

        ' D0117 ===
        Public Sub ErrorFeedback(ByVal sender As Object, ByVal e As System.EventArgs)  ' D4382 // remove Handled for every time
            Dim ex As Exception = Server.GetLastError
            DebugInfo("RTE: " + ex.Message, _TRACE_RTE) ' D0785
            If HookErrors() AndAlso CurrentPageID <> _PGID_ERROR_500 Then   ' D4382
                Dim OldStatus As ecErrorStatus = App.ApplicationError.Status
                ' D4175 ===
                App.ApplicationError.Init(ecErrorStatus.errRTE, CurrentPageID, ex.Message, Nothing, Request.Url.AbsoluteUri, ex) ' D0459 + D0517
                If _OPT_SNAPSHOTS_CREATE_ON_RTE AndAlso App.ProjectID > 0 AndAlso WebConfigOption(_OPT_SNAPSHOT_ON_RTE, CStr(Bool2Num(_OPT_SNAPSHOTS_CREATE_ON_RTE))) <> "0" Then
                    Try
                        DebugInfo("Save snapshot on RTE", _TRACE_INFO)
                        App.SnapshotSaveProject(ecSnapShotType.RestorePoint, "Save on RTE", App.ProjectID, False, GetRTEHeader())
                    Catch tmpEx As Exception
                        DebugInfo("Unable to save snapshot on RTE: " + tmpEx.Message, _TRACE_RTE)
                    End Try
                End If
                ' D4175 ==
                If isCallback() Then ' D0785
                    App.DBSaveLog(dbActionType.actShowRTE, dbObjectType.einfRTE, CurrentPageID, String.Format("RTE: {0}", ex.Message), ex.StackTrace)   ' D4175
                Else
                    ' D1240 ===
                    If OldStatus <> ecErrorStatus.errRTE AndAlso TypeOf (ex) Is HttpException AndAlso TypeOf (ex.InnerException) Is ViewStateException Then
                        App.DBSaveLog(dbActionType.actShowRTE, dbObjectType.einfRTE, CurrentPageID, String.Format("RTE: {0}", ex.Message), ex.StackTrace)
                        App.Database.Close()    ' D2235
                        Response.Redirect(Request.Url.AbsoluteUri, True)
                    End If
                End If
                ' D1240 ==
                App.Database.Close()    ' D2235
                ' D6006 ===
                'If isAJAX OrElse CurrentPageID = _PGID_WEBAPI OrElse App.ApplicationError.PageID = _PGID_WEBAPI Then
                '    Response.ClearContent()
                '    Response.ContentType = "application/json"   ' D5031
                '    Response.StatusDescription = If(OldStatus = ecErrorStatus.errRTE, "RTE", OldStatus.ToString)
                '    Response.StatusCode = 500   ' Internal error
                '    If ex IsNot Nothing AndAlso Not String.IsNullOrEmpty(ex.Message) Then
                '        Dim sText As String = GetRTEHeader()
                '        If sText = "" Then sText = ex.Message
                '        Response.Write(sText)
                '        If Request IsNot Nothing AndAlso Request.IsLocal AndAlso Not String.IsNullOrEmpty(ex.StackTrace) Then Response.Write(vbNewLine + vbNewLine + ex.StackTrace)
                '    End If
                '    Response.End()
                'Else
                ' D6358 ===
                Response.ClearContent()
                Response.Clear()
                ' D7675 ===
                Try
                    Server.Transfer(PageURL(_PGID_ERROR_500, GetTempThemeURI(True)), False)
                Catch tmp_ex As Exception
                    Response.Redirect(PageURL(_PGID_ERROR_500, GetTempThemeURI(True)), True)
                End Try
                ' D7675 ==
                ' D6358 ==
                'Server.Transfer(PageURL(_PGID_ERROR_500, "ajax=" + Bool2YesNo(isAJAX) + "&callback=" + Bool2YesNo(isCallback OrElse IsPostBack OrElse IsCrossPagePostBack) + GetTempThemeURI(True)), False)
                'If isAJAX OrElse CurrentPageID = _PGID_WEBAPI OrElse App.ApplicationError.PageID = _PGID_WEBAPI Then
                '    Server.Transfer(PageURL(_PGID_ERROR_500, "ajax=" + Bool2YesNo(isAJAX) + "&callback=" + Bool2YesNo(isCallback OrElse IsPostBack OrElse IsCrossPagePostBack) + GetTempThemeURI(True)), False)
                'Else
                '    Response.Redirect(PageURL(_PGID_ERROR_500, GetTempThemeURI(False)))
                'End If
                'Response.Redirect(PageURL(_PGID_ERROR_500, "ajax=" + Bool2YesNo(isAJAX) + "&callback=" + Bool2YesNo(isCallback OrElse IsPostBack OrElse IsCrossPagePostBack) + GetTempThemeURI(True)))
                'Server.Transfer(PageURL(_PGID_ERROR_500, GetTempThemeURI(True))) ' D0717 + D0729 + D0766 + D4750 + D6358
                'Server.Transfer(PageURL(_PGID_ERROR_500, "ajax=" + Bool2YesNo(isAJAX) + "&callback=" + Bool2YesNo(isCallback OrElse IsPostBack OrElse IsCrossPagePostBack) + GetTempThemeURI(True)), False) ' D0717 + D0729 + D0766 + D4750
                'End If
            End If
            'If Not HookErrors() AndAlso ex IsNot Nothing AndAlso Not String.IsNullOrEmpty(ex.Message) Then
            '    'Dim tErrOld As clsApplicationError = App.ApplicationError
            '    App.ApplicationErrorInitAndSaveLog(ecErrorStatus.errRTE, CurrentPageID, ex.Message, Nothing, Request.Url.AbsoluteUri, ex)
            '    'App.ApplicationError = tErrOld
            '    If isAJAX OrElse CurrentPageID = _PGID_WEBAPI OrElse App.ApplicationError.PageID = _PGID_WEBAPI Then
            '        TerminateWithRTEAsAJON(App.ApplicationError.Status, ex)
            '    End If
            'End If
            ' D6006 ==
        End Sub
        ' D0117 ==

        Public Function GetParam(ByVal ParamsList As Specialized.NameValueCollection, ByVal sArgName As String, Optional fCheckVar As Boolean = False) As String
            ' D5025 ===
            If ParamsList IsNot Nothing AndAlso Not String.IsNullOrEmpty(sArgName) AndAlso ParamsList(sArgName) IsNot Nothing Then
                Return RemoveBadTags(ParamsList(sArgName))
            Else
                If fCheckVar AndAlso Not String.IsNullOrEmpty(sArgName) Then Return CheckVar(sArgName, "") Else Return ""
            End If
            ' D5025 ==
        End Function

        ' D6455 ===
        Public Function HasParam(ByVal ParamsList As Specialized.NameValueCollection, ByVal sArgName As String, Optional fCheckVar As Boolean = False) As Boolean
            If Not String.IsNullOrEmpty(sArgName) Then
                If ParamsList IsNot Nothing AndAlso ParamsList.AllKeys.Contains(sArgName) Then Return True
                If fCheckVar AndAlso Request.Params.AllKeys.Contains(sArgName) Then Return True
            End If
            Return False
        End Function
        ' D6455 ==

        Public Overloads Function CheckVar(ByVal sVarName As String) As String 'A1546
            Return CheckVar(sVarName, "")
        End Function

        Public Overloads Function CheckVar(ByVal sVarName As String, ByVal DefValue As String) As String
            Dim Res As String = DefValue
            If sVarName IsNot Nothing AndAlso Request(sVarName) IsNot Nothing Then Res = RemoveBadTags(CStr(Request(sVarName))) ' D1800 + D5039
            Return Res
        End Function

        Public Overloads Function CheckVar(ByVal sVarName As String, ByVal DefValue As Guid) As Guid
            Dim Res As Guid = DefValue
            ' D4507 ===
            If sVarName IsNot Nothing AndAlso Request(sVarName) IsNot Nothing Then
                Dim sGUID As String = RemoveXssFromParameter(Request(sVarName))    ' D6767
                If Guid.TryParse(sGUID, Res) Then Res = New Guid(sGUID)
            End If
            ' D4507 ==
            Return Res
        End Function

        Public Overloads Function CheckVar(ByVal sVarName As String, ByVal DefValue As Integer) As Integer
            Dim Res As Integer = DefValue
            Try
                If sVarName IsNot Nothing AndAlso Request(sVarName) IsNot Nothing Then Res = CInt(RemoveXssFromParameter(Request(sVarName))) ' D1800 + D6767
            Catch ex As Exception
                Res = DefValue
            End Try
            Return Res
        End Function

        Public Overloads Function CheckVar(ByVal sVarName As String, ByVal DefValue As Boolean) As Boolean
            Dim sValue As String = DefValue.ToString.ToLower
            If sVarName IsNot Nothing AndAlso Request IsNot Nothing AndAlso Request(sVarName) IsNot Nothing Then sValue = CStr(RemoveXssFromParameter(Request(sVarName))).ToLower ' D1800 + D3996 + D6767
            Return (sValue = True.ToString.ToLower OrElse sValue = "true" OrElse sValue = "1" OrElse sValue = "yes")
        End Function

        Public Sub SetCookie(ByVal sName As String, ByVal sValue As String, Optional ByVal fOnlyForSession As Boolean = False, Optional ByVal fForceEncode As Boolean = False)  ' D0057 + D0755
            If String.IsNullOrEmpty(sValue) Then    ' D3452
                If sValue Is Nothing Then sValue = "" ' D3501
                Response.Cookies.Remove(sName) ' D3452
                Response.Cookies.Add(New HttpCookie(sName, sValue)) ' D3501
            Else
                Dim tst As Long = -1 ' D0755 + D0941
                If fForceEncode Or (EncryptAllCookies AndAlso sValue <> "" AndAlso Not Long.TryParse(sValue, tst)) Then sValue = CookieEncryptionPrefix + EncodeURL(sValue, App.DatabaseID) ' D0755 + D0826 + D0941
                Dim cookie As New HttpCookie(sName, sValue)
                If Request.Url.Port = 443 Then cookie.Secure = True ' D0043
                cookie.Path += ";SameSite=Strict"   ' D7641
                cookie.HttpOnly = True  ' D4178
                If Not fOnlyForSession Then cookie.Expires = Now.AddSeconds(_EXPIRE_COOKIES) ' D0057
                Response.Cookies.Add(cookie)
            End If
        End Sub

        Public Function GetCookie(ByVal sName As String, Optional ByVal sDefValue As String = "") As String
            If (Request Is Nothing OrElse Request.Cookies(sName) Is Nothing) Then   ' D5065
                Return sDefValue
            Else
                ' D0755 ===
                Dim sVal As String = RemoveXssFromText(Request.Cookies(sName).Value)    ' D6766
                If sVal <> "" AndAlso (EncryptAllCookies OrElse sVal.StartsWith(CookieEncryptionPrefix)) Then sVal = DecodeURL(sVal.Substring(CookieEncryptionPrefix.Length), App.DatabaseID) ' D0826 + D2895
                If sVal = "" Then sVal = sDefValue ' D0829
                Return sVal
                ' D0755 ==
            End If
        End Function

        ' -D0510
        'Public Function HasActiveProject() As Boolean
        '    Return App.HasActiveProject ' D0466
        'End Function

        'Public ReadOnly Property ActiveProject() As clsProject
        '    Get
        '        ' D0466 ===
        '        If Not App.HasActiveProject Then
        '            DebugInfo("Try to access to Active Project. Fetch", _TRACE_WARNING)
        '            FetchAccess()   ' D0043
        '        End If
        '        Return App.ActiveProject
        '        ' D0466 ==
        '    End Get
        'End Property
        ' D0029 ==

        ' D2796 ===
        Public Function IsValidEULA() As Boolean
            Return App.CheckUserEULA(App.ActiveUserWorkgroup) OrElse isSLTheme() OrElse String.IsNullOrEmpty(Page.MasterPageFile) OrElse Page.MasterPageFile.ToLower.Contains("mpempty") OrElse Page.MasterPageFile.ToLower.Contains("mpblank") OrElse Page.MasterPageFile.ToLower.Contains("mppopup") OrElse GetCookie(_COOKIE_RET_PATH, "") <> "" OrElse CurrentPageID = _PGID_EVALUATE_INFODOC   ' D6260
        End Function
        ' D2796 ==

        ' D6081 ===
        Public Function isNotificationAccepted() As Boolean
            Return Not _FEDRAMP_MODE OrElse GetCookie(_COOKIE_FEDRAMP_NOTIFICATION, "") = "1"
        End Function
        ' D6081 ==

        ' D4639 ===
        Public Function isReviewAccount() As Boolean
            Return App.ActiveUser IsNot Nothing AndAlso ReviewAccount.ToLower = App.ActiveUser.UserEmail.ToLower
        End Function
        ' D4639 ==

        ' D6532 ===
        Public Function isSSO() As Boolean
            Return _SSO_MODE
        End Function
        ' D6532 ==

        ' D6550 ===
        Public Function isSSO_Only() As Boolean
            Return _SSO_MODE AndAlso _SSO_ONLY  ' D6552
        End Function
        ' D6550 ==

        ' D3129 ===
        Public Function CheckAssociatedRiskModel(tProject As clsProject, fDoResetIfWrong As Boolean, ByRef sModelName As String) As Boolean ' D4488
            Dim fResult As Boolean = True
            If tProject IsNot Nothing AndAlso (tProject.IsProjectLoaded OrElse tProject.isValidDBVersion OrElse tProject.isDBVersionCanBeUpdated) Then
                If tProject.PipeParameters.AssociatedModelIntID > 0 Then
                    fResult = False
                    Dim tPrj As clsProject = clsProject.ProjectByID(tProject.PipeParameters.AssociatedModelIntID, App.ActiveProjectsList)
                    If tPrj Is Nothing Then tPrj = App.DBProjectByID(tProject.PipeParameters.AssociatedModelIntID)
                    ' D3190 ===
                    If tPrj Is Nothing AndAlso tProject.PipeParameters.AssociatedModelGuidID <> "" AndAlso tProject.PipeParameters.AssociatedModelGuidID <> Guid.Empty.ToString Then
                        Dim tPrjList As List(Of clsProject) = clsProject.ProjectsByGUID(New Guid(tProject.PipeParameters.AssociatedModelGuidID), App.ActiveProjectsList, False)
                        If tPrjList IsNot Nothing AndAlso tPrjList.Count > 0 Then
                            tPrjList.Sort(New clsProjectComparer(ecProjectSort.srtProjectDateTime, SortDirection.Descending, Nothing))
                            tPrj = tPrjList(0)
                            App.DBSaveLog(dbActionType.actModify, dbObjectType.einfProject, tProject.ID, "Auto-assign Associated Risk model by GUID", String.Format("Attached Model: ID #{0}, '{1}'", tPrj.ID, tProject.ProjectName))
                            tProject.PipeParameters.AssociatedModelIntID = tPrj.ID
                            tProject.SaveProjectOptions("Set Associated Risk model", , , String.Format("Auto-assign to '{0}'", ShortString(tPrj.ProjectName, 80, True)))    ' D3758
                        End If
                    End If
                    ' D3190 ==
                    If tPrj IsNot Nothing Then
                        If tPrj.WorkgroupID = App.ActiveWorkgroup.ID AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted AndAlso App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, App.ActiveUser.UserID, tPrj.ID, App.ActiveUserWorkgroup, App.ActiveWorkgroup) Then fResult = True ' D3190
                        If fResult AndAlso sModelName IsNot Nothing Then sModelName = tPrj.ProjectName ' D4488
                    End If
                    If Not fResult AndAlso fDoResetIfWrong Then
                        App.DBSaveLog(dbActionType.actModify, dbObjectType.einfProject, tProject.ID, "Can't find Associated Risk model", String.Format("Removed Associated Risk Model links: ID #{0}, GUID '{1}'", tProject.PipeParameters.AssociatedModelIntID, tProject.PipeParameters.AssociatedModelGuidID))
                        tProject.PipeParameters.AssociatedModelIntID = 0
                        tProject.PipeParameters.AssociatedModelGuidID = ""
                        tProject.SaveProjectOptions("Reset Associated Risk model")
                    End If
                End If
            End If
            Return fResult
        End Function
        ' D3129 ==

        ' D0043 ===
        Public Sub CheckPermissions()
            ' D4326 ===
            Dim fCheckPage As Boolean = CurrentPageID <> _PGID_ERROR_403 AndAlso CurrentPageID <> _PGID_ERROR_404 AndAlso CurrentPageID <> _PGID_ERROR_500 AndAlso CurrentPageID <> _PGID_ERROR_503 AndAlso CurrentPageID <> _PGID_START
            Dim fNoPermissions As Boolean = fCheckPage AndAlso Not HasPermission(CurrentPageID, App.ActiveProject, CustomWorkgroupPermissions)  ' D7270
            If fNoPermissions Then  ' D0046 + D0257 + D0391 + D0439 + D0466
                ' D4326 ==
                'Clear page instead redirect for SLShell // D0735
                'If isSLShellCalls Then
                '    Response.ClearContent()
                '    Response.Flush()
                '    Response.End()
                'End If
                DebugInfo(String.Format("User has no permissions for access to page '{0}'. Check permissions: {1}", PageTitle(CurrentPageID), Not fNoPermissions), _TRACE_WARNING) ' D4326
                ' D0325 ===
                Dim pgID As Integer = -1
                If App.isAuthorized AndAlso Not App.ActiveUser Is Nothing Then
                    If Not IsValidEULA() Then pgID = _PGID_EULA ' D0473 + D0741 + D0766 + D2796
                    If App.MFA_Requested Then pgID = _PGID_START ' D7503
                End If
                'If isSLTheme() AndAlso Not IsCallback AndAlso pgID <> _PGID_EULA Then Response.Redirect(PageURL(_PGID_SILVERLIGHT_SERVICE, "msg=auth" + GetTempThemeURI(True))) Else FetchAccess(pgID) ' D0323 + D1624
                FetchAccess(pgID) ' D0323 + D1624
                ' D0325 ==
            Else
                ' D6262 ===
                If App.isAuthorized AndAlso App.ActiveUser IsNot Nothing AndAlso Not _OPT_ALLOW_IGNORE_EULA AndAlso Not IsValidEULA() AndAlso CurrentPageID <> _PGID_EULA AndAlso CurrentPageID <> _PGID_LOGOUT AndAlso CurrentPageID <> _PGID_START AndAlso CurrentPageID <> _PGID_START_WITH_SIGNUP Then
                    SessVar(_SESS_RET_URL) = Request.Url.AbsoluteUri
                    Response.Redirect(PageURL(_PGID_EULA), True)
                End If
                ' D6262 ==
                DebugInfo("Check User permissions passed")  ' D0046
            End If
        End Sub
        ' D0043 ==

        ' D3853 ===
        Private Function GetCodeForHotkey(tPgID As Integer, sExtraCode As String) As String
            Dim sCode As String = String.Format("_pageNavigate('{0}', '{1}');", tPgID, JS_SafeString(PageURL(tPgID, GetTempThemeURI(False))))
            If Not String.IsNullOrEmpty(sExtraCode) Then sCode = sExtraCode.TrimEnd(CChar(";")) + "; " + sCode
            Return sCode
        End Function
        ' D3853 ==

        ' D3852 ===
        Private Sub ScanHotkeys(ByRef Lst As Dictionary(Of String, String), tNode As SiteMapNode)
            If tNode IsNot Nothing Then
                Dim tPrj As clsProject = Nothing
                If App.HasActiveProject Then tPrj = App.ActiveProject
                For Each tSubNode As SiteMapNode In tNode.ChildNodes
                    Dim tPgID As Integer = 0
                    Dim fAvailable As Boolean = True
                    ' D3967 ===
                    Dim hasHotKeys As Boolean = False
                    For i As Integer = 0 To tSubNode.Roles.Count - 1
                        If tSubNode.Roles(i).ToString.StartsWith("~") Then hasHotKeys = True
                    Next
                    If hasHotKeys AndAlso Integer.TryParse(tSubNode.ResourceKey, tPgID) Then
                        ' D3967 ==
                        fAvailable = HasPermission(tPgID, tPrj)
                        If fAvailable Then
                            If tSubNode.Roles.IndexOf(_ROLE_RISK) >= 0 AndAlso Not App.isRiskEnabled Then fAvailable = False
                            If tSubNode.Roles.IndexOf(_ROLE_NORISK) >= 0 AndAlso App.isRiskEnabled Then fAvailable = False
                            If fAvailable Then
                                For i As Integer = 0 To tSubNode.Roles.Count - 1
                                    If tSubNode.Roles(i).ToString.StartsWith("~") Then
                                        'If tPgID <> _Page.CurrentPageID AndAlso _Page.HasPermission(tPgID, tPrj) Then
                                        'Lst.Add(tSubNode.Roles(i).ToString.TrimStart(CChar("~")), GetCodeForHotkey(tPgID, tSubNode.Description))    ' D3853
                                        Dim tKey As String = tSubNode.Roles(i).ToString.TrimStart(CChar("~"))
                                        If Not Lst.ContainsKey(tKey) Then Lst.Add(tKey, GetCodeForHotkey(tPgID, ""))    ' D3853 + D4775
                                    End If
                                Next
                            End If
                        End If
                    End If
                    If fAvailable AndAlso tSubNode.HasChildNodes Then ScanHotkeys(Lst, tSubNode)
                Next
            End If
        End Sub

        Public Function ParseHotKeyString(sKey As String) As String
            Dim sIf As String = ""
            sKey = sKey.Trim.ToLower
            If sKey <> "" Then
                sIf += String.Format("{0}{1}e.ctrlKey", IIf(sIf = "", "", " && "), IIf(sKey.Contains("ctrl"), "", "!"))
                sIf += String.Format("{0}{1}e.altKey", IIf(sIf = "", "", " && "), IIf(sKey.Contains("alt"), "", "!"))
                sIf += String.Format("{0}{1}e.shiftKey", IIf(sIf = "", "", " && "), IIf(sKey.Contains("shift"), "", "!"))
                sKey = sKey.Replace("ctrl", "").Replace("alt", "").Replace("shift", "").Trim
                If sKey = "" Then
                    sIf = ""
                Else
                    If sKey.Length >= 2 AndAlso sKey.Length <= 3 AndAlso sKey(0) = "f" AndAlso sKey(1) >= "1" AndAlso sKey(1) <= "9" Then
                        Dim N As Integer
                        If Integer.TryParse(sKey.Substring(1), N) AndAlso N >= 1 AndAlso N <= 12 Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), N + 111)
                    End If
                    If sKey = "del" OrElse sKey = "delete" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 46)
                    If sKey = "esc" OrElse sKey = "escape" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 27)
                    If sKey = "ins" OrElse sKey = "insert" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 45)
                    If sKey = "tab" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 9)
                    If sKey = "enter" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 13)
                    If sKey = "space" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 32)
                    If sKey = "left" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 37)
                    If sKey = "right" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 39)
                    If sKey = "up" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 38)
                    If sKey = "down" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 40)
                    If sKey = "home" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 36)
                    If sKey = "end" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 35)
                    If sKey = "pgup" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 33)
                    If sKey = "pgdn" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 34)
                    If sKey = "back" OrElse sKey = "backspace" Then sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), 8)
                    If sKey.Length = 1 Then
                        If sKey >= "a" AndAlso sKey <= "z" Then
                            sIf += String.Format("{0}(e.keyCode=={1} || e.keyCode=={2})", IIf(sIf = "", "", " && "), Asc(sKey), Asc(sKey.ToUpper))
                        Else
                            sIf += String.Format("{0}e.keyCode=={1}", IIf(sIf = "", "", " && "), Asc(sKey))
                        End If
                    Else
                        sIf = ""
                    End If
                End If
            End If
            Return sIf
        End Function

        Public Function GetHotkeysHandler() As String
            Dim sCode As String = ""
            Dim HotkeysList As New Dictionary(Of String, String)
            ScanHotkeys(HotkeysList, SiteMap.RootNode)

            ' D3853 ===
            If HotkeysList.ContainsKey("CtrlY") Then HotkeysList.Remove("CtrlY")
            If App.isRiskEnabled Then
                If HotkeysList.ContainsKey("CtrlM") Then HotkeysList.Remove("CtrlM")
                Dim isRisk As Boolean = HasListPage(_PAGESLIST_RISK, CurrentPageID)
                Dim isTreatments As Boolean = HasListPage(_PAGESLIST_TREATMENTS, CurrentPageID)
                HotkeysList.Add("CtrlM", GetCodeForHotkey(CInt(IIf(isRisk OrElse isTreatments, _PGID_EVALUATE_RISK_CONTROLS, _PGID_EVALUATION)), ""))
                Dim pgDG As Integer = _PGID_REPORT_DATAGRID
                If isRisk Then pgDG = _PGID_REPORT_DATAGRID_RISK
                If isTreatments Then pgDG = _PGID_REPORT_DATAGRID_RISK_TREATMENTS
                HotkeysList.Add("CtrlY", GetCodeForHotkey(pgDG, ""))

                'A1478 ===
                If HotkeysList.ContainsKey("CtrlAltE") Then HotkeysList.Remove("CtrlAltE")
                If HotkeysList.ContainsKey("CtrlAltI") Then HotkeysList.Remove("CtrlAltI")
                If HotkeysList.ContainsKey("CtrlAltO") Then HotkeysList.Remove("CtrlAltO")
                If HotkeysList.ContainsKey("CtrlAltR") Then HotkeysList.Remove("CtrlAltR")
                If HotkeysList.ContainsKey("CtrlAltB") Then HotkeysList.Remove("CtrlAltB")
                If HotkeysList.ContainsKey("CtrlAltH") Then HotkeysList.Remove("CtrlAltH")
                If HotkeysList.ContainsKey("CtrlAltS") Then HotkeysList.Remove("CtrlAltS")

                HotkeysList.Add("CtrlAltE", GetCodeForHotkey(_PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_OVERALL, ""))
                HotkeysList.Add("CtrlAltI", GetCodeForHotkey(_PGID_RISK_TREATMENTS_REGISTER, ""))
                HotkeysList.Add("CtrlAltO", GetCodeForHotkey(_PGID_RISK_OPTIMIZER_OVERALL, ""))
                HotkeysList.Add("CtrlAltR", GetCodeForHotkey(_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL, ""))
                HotkeysList.Add("CtrlAltB", GetCodeForHotkey(_PGID_RISK_BOW_TIE_WITH_CONTROLS, ""))
                HotkeysList.Add("CtrlAltH", GetCodeForHotkey(_PGID_RISK_PLOT_OVERALL_WITH_CONTROLS, ""))
                'A1478 ==
            Else
                HotkeysList.Add("CtrlY", GetCodeForHotkey(CInt(IIf(HasListPage(_PAGESLIST_RESULTS, CurrentPageID), _PGID_REPORT_DATAGRID_NOUPLOAD, _PGID_REPORT_DATAGRID)), ""))
            End If
            ' D3853 ==

            For Each sKey As String In HotkeysList.Keys
                Dim sIf As String = ParseHotKeyString(sKey)
                If sIf <> "" Then sCode += String.Format("if ({0}) {{ {1} }}" + vbCrLf, sIf, HotkeysList(sKey))
            Next
            If sCode <> "" Then sCode = "  $(document).keyup(function(e) {" + vbCrLf + sCode + "   });"
            Return sCode
        End Function
        ' D3852 ==

        ' D0019 ===
        Public Function GetBackURL(Optional ByVal sDefURL As String = "") As String
            If sDefURL = "" Then sDefURL = CStr(IIf(App.ActiveUser Is Nothing, PageURL(_PGID_START), PageURL(_PGID_PROJECTSLIST))) ' D0043
            If Not Request.UrlReferrer Is Nothing Then
                If Request.UrlReferrer.AbsoluteUri <> Request.Url.AbsoluteUri Then sDefURL = Request.UrlReferrer.AbsoluteUri
            End If
            Return RemoveXssFromUrl(sDefURL, True)  ' Anti-XSS
        End Function

        Public Function JSGetBackURL(Optional ByVal sDefURL As String = "") As String
            Return String.Format("document.location.href='{0}'; return false", JS_SafeString(GetBackURL(sDefURL)))  ' D4630
        End Function
        ' D0019 ==

        ' D4918 ==
        Public Function GetSessionUserName() As String
            If App.isAuthorized Then
                Return If(App.ActiveUser.UserName = "", App.ActiveUser.UserEmail, App.ActiveUser.UserName)
            Else
                Return If(App.Antigua_UserName = "", App.Antigua_UserEmail, App.Antigua_UserName)   ' D4920
            End If
        End Function
        ' D4918 ==

        ' D0652 ===
        Public Function MasterByTheme(ByVal sTheme As String) As String
            Dim sMaster As String = ""
            Select Case sTheme.ToLower
                'Case _THEME_DEFAULT.ToLower        ' -D4961
                '    sMaster = _MASTER_DEFAULT
                'Case _THEME_EC09.ToLower ' D0781 + D4918    // _THEME_SL.ToLower -D4961
                '    sMaster = _MASTER_EC09
                Case _THEME_EC2018              ' D4668
                    sMaster = _MASTER_EC2018    ' D4668
                Case _THEME_SL.ToLower          ' D4918
                    sMaster = _MASTER_EMPTY     ' D4918
            End Select
            Return sMaster
        End Function

        ' D4058 ===
        Private Function isSecuritySwitchUsed() As Boolean
            Dim fResult As Boolean = False
            Dim myHttpContext As HttpContext = HttpContext.Current
            Dim myHttpApplication As HttpApplication = myHttpContext.ApplicationInstance
            Dim myHttpModuleCollection As HttpModuleCollection = myHttpApplication.Modules
            'Dim httpModuleName As String = myHttpModuleCollection.GetKey(1)
            'Dim allModules() As String = myHttpModuleCollection.AllKeys
            For Each sName As String In myHttpModuleCollection.AllKeys
                If sName.ToLower = "securityswitch" Then
                    fResult = True
                    Exit For
                End If
            Next
            Return fResult
        End Function
        ' D4058 ==

        '' D2760 ===
        'Public Function FixSSLForShell(sURL As String) As String
        '    If Not WebOptions.ForceSSL(Request) AndAlso Request IsNot Nothing AndAlso Request.Url IsNot Nothing AndAlso isSecureConnection(Request) Then ' D4061
        '        If isSecuritySwitchUsed() Then sURL = ApplicationURL(False, False).Replace("https://", "http://") + sURL ' D3308 + D3494 + D4058
        '    End If
        '    Return sURL
        'End Function
        '' D2760 ==

        '' D0729 ===
        'Public Function PageURL_SLShell(ByVal PgID As Integer, Optional ByVal sParams As String = "") As String
        '    Return FixSSLForShell(PageURL(_PGID_SILVERLIGHT_UI, sParams) + "#pgid=" + CStr(PgID))   ' D2760
        'End Function
        '' D0729 ==

        ' -D4961
        'Public Function GetThemesList(ByVal sLinkAttribs As String, ByVal fHasTabs As Boolean) As String  ' D0695
        '    Dim sThemes As String = ""
        '    For Each sThemeName As String In _THEMES_LIST
        '        If ShowDraftPages() Or Array.IndexOf(_THEMES_DRAFT, sThemeName) < 0 Then
        '            Dim sURL As String = ""
        '            Dim sTitle As String = ResString("theme_" + sThemeName)
        '            If DefaultMasterPage.ToLower <> MasterByTheme(sThemeName).ToLower Or (CurrentPageID <> _PGID_SILVERLIGHT_UI AndAlso DefaultMasterPage.ToLower = _MASTER_BLANK.ToLower) Or CurrentPageID = _PGID_SILVERLIGHT_UI Then ' D0653
        '                Select Case sThemeName
        '                    Case Else
        '                        sURL = PageURL(CurrentPageID, _PARAM_THEME + "=" + sThemeName)
        '                End Select
        '            End If
        '            ' -D4850
        '            'If sThemes <> "" Then sThemes += " | "
        '            'If sURL = "" Then sThemes += sTitle Else sThemes += String.Format("<a href='{0}' {1}>{2}</a>", sURL, sLinkAttribs + CStr(IIf(CurrentPageID = _PGID_SILVERLIGHT_UI, " onclick='return ShowHTML(this.href);'", "")), sTitle) ' D0695
        '        End If
        '    Next
        '    ' -D4850
        '    '' D0682 + D0695 ===
        '    'If fHasTabs Then  ' D0681
        '    '    If sThemes <> "" Then sThemes += " | " ' D0779
        '    '    sThemes += HTMLTextLink(PageURL(If(CurrentPageID = _PGID_EVALUATION, _PGID_EVALUATION, CurrentPageID)), PageMenuItem(_PGID_SILVERLIGHT_UI), True, sLinkAttribs, " style='color:#dddddd'")  ' D0690 'A0159 + D0779
        '    '    ' D0682 ==
        '    'End If
        '    '' D0695 ==
        '    Return sThemes
        'End Function
        '' D0652 ==

        Public Property DebugDisableAutoComplete4Logon As Boolean
            Get
                ' D4663 ===
                Dim Val As Boolean = WebConfigOption(_OPT_DISABLEAUTOCOMPLETEFORLOGIN, "0") <> "0"
                Str2Bool(GetCookie(_COOKIE_DEBUG_AUTOCOMPLETE, Val.ToString), Val)
                Return Val OrElse isSSO() ' D6552
                ' D4663 ==
            End Get
            Set(value As Boolean)
                SetCookie(_COOKIE_DEBUG_AUTOCOMPLETE, value.ToString)
            End Set
        End Property

        ' D0157 ===
        Public Property DebugAutoLogon() As Boolean
            Get
                Str2Bool(GetCookie(_COOKIE_DEBUG_AUTOLOGON, _IFDEF_UseAutologon.ToString), _IFDEF_UseAutologon)    ' D0181 + D4663
                Return _IFDEF_UseAutologon AndAlso Not isSSO()  ' D6550
            End Get
            Set(ByVal value As Boolean)
                _IFDEF_UseAutologon = value
                SetCookie(_COOKIE_DEBUG_AUTOLOGON, value.ToString)
            End Set
        End Property

        Public Property DebugRestoreLastPage() As Boolean
            Get
                Str2Bool(GetCookie(_COOKIE_DEBUG_LASTPAGE, _IFDEF_RestoreLastPage.ToString), _IFDEF_RestoreLastPage)    ' D0181 + D4663
                Return _IFDEF_RestoreLastPage
            End Get
            Set(ByVal value As Boolean)
                _IFDEF_RestoreLastPage = value
                SetCookie(_COOKIE_DEBUG_LASTPAGE, value.ToString)
            End Set
        End Property

        Public Property DebugCheckEmailsBeforeUpload() As Boolean
            Get
                Str2Bool(GetCookie(_COOKIE_DEBUG_CHECKMODEL, _IFDEF_CheckEmailsBeforeUpload.ToString), _IFDEF_CheckEmailsBeforeUpload) ' D0181 + D4663
                Return _IFDEF_CheckEmailsBeforeUpload
            End Get
            Set(ByVal value As Boolean)
                _IFDEF_CheckEmailsBeforeUpload = value
                SetCookie(_COOKIE_DEBUG_CHECKMODEL, value.ToString, False, False)   ' D1240
            End Set
        End Property
        ' D0157 ==

        ' D6788 ===
        Public Function GetDefaultPath(CheckLastVisisted As Boolean) As String
            Dim sURL As String = _URL_ROOT
            If App.isAuthorized Then
                sURL = PageURL(_PGID_PROJECTSLIST)
                If App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then
                    sURL = PageURL(_PGID_ADMIN_WORKGROUPS)
                Else
                    If CheckLastVisisted AndAlso DebugRestoreLastPage Then
                        Dim sRetURL As String = GetOptionalStartURL(App.ActiveUser.UserID, sURL)
                        If isFirstRun OrElse (Request.UrlReferrer Is Nothing AndAlso Not sRetURL.ToLower.Equals(Request.Url.PathAndQuery, StringComparison.CurrentCultureIgnoreCase)) Then
                            sURL = sRetURL
                        End If
                    End If
                    If App.HasActiveProject AndAlso Not App.isEvalURL_EvalSite AndAlso (App.ActiveProjectsList.Count = 1 OrElse App.Options.isSingleModeEvaluation) Then sURL = PageURL(_PGID_EVALUATION,, True)    ' D6359
                    ' D4955 ===
                    Dim sMSg As String = ""
                    If CurrentPageID <> _PGID_ERROR_503 AndAlso Not App.CheckLicense(App.ActiveWorkgroup, sMSg, True) Then
                        If Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then FetchAccessByWrongLicense(sMSg, True)
                    End If
                    ' D4955 ==
                End If
            End If
            Return sURL
        End Function
        ' D6788 ==

        ' D0108 ===
        Public Function GetOptionalStartURL(ByVal tUserID As Integer, ByVal sCustomURL As String) As String
            If DebugRestoreLastPage AndAlso App.CanvasMasterDBVersion >= "0.92" Then    ' D0157 + D0515
                ' D0491 ===
                Dim sLastURL As String = ""
                Dim tRes As Object = App.Database.ExecuteScalarSQL(String.Format("SELECT {3} FROM {0} WHERE {1}={2}", clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_ID, tUserID, clsComparionCore._FLD_USERS_LASTURL))
                If Not tRes Is Nothing AndAlso Not IsDBNull(tRes) Then sLastURL = CStr(tRes)
                ' D0491 ==
                If sLastURL.ToLower.Contains("comparion.aspx") OrElse sLastURL.ToLower.Contains(_WEBAPI_ROOT) OrElse sLastURL.ToLower.Contains("password.aspx") Then sLastURL = ""   ' D4826 + D7413 + D7621
                ' D6325 ===
                If sLastURL <> "" Then  ' try to check path as the real file and reset if not exists
                    Try
                        Dim sURL As String = ""
                        Dim tUrl As Uri
                        If sURL.Contains("://") Then tUrl = New Uri(sLastURL, UriKind.Absolute) Else tUrl = New Uri(New Uri(ApplicationURL(False, False)), sLastURL)
                        If tUrl IsNot Nothing Then sURL = tUrl.LocalPath
                        If sURL.EndsWith("/") Then sURL += "Default.aspx"
                        Dim sPath As String = Server.MapPath(sURL)
                        If Not MyComputer.FileSystem.FileExists(sPath) Then sLastURL = ""
                    Catch ex As Exception
                    End Try
                End If
                ' D6325 ==
                If sLastURL <> "" AndAlso Not sLastURL.Contains("http") Then sLastURL = CStr(_URL_ROOT + sLastURL).Replace("//", "/") ' D0345
                If sLastURL <> "" Then sCustomURL = sLastURL
            End If
            Return sCustomURL
        End Function
        ' D0108 ==

        ' D6886 ===
        Public Function getReportCategoryName(RCat As ecReportCategory) As String
            Return ResString("titleReport" + RCat.ToString)    ' D6605
        End Function
        ' D6886 ==

        ' D6585 ===
        Public Function getReportTypeName(RType As ecReportType) As String
            Return ResString("titleReport" + RType.ToString)    ' D6605
        End Function

        Public Function getReportTypeName(tReport As clsReport) As String
            Return getReportTypeName(tReport.ReportType)
        End Function
        ' D6585 ==

        ' D6605 ===
        Public Function getReportItemTypeName(ItemType As ecReportItemType) As String
            Return ResString("lblReportItem" + ItemType.ToString)    ' D6605
        End Function

        Public Function getReportItemTypeName(tItem As clsReportItem) As String
            'Return getReportItemTypeName(tItem.ItemType)
            Return getReportItemTypeName(CType(tItem.ItemType, ecReportItemType))   ' D7020
        End Function
        ' D6605 ==

        ' D6899 ===
        Public Function GetReportSamples() As clsReportsCollection
            Dim tList As New clsReportsCollection(Nothing)
            If Session(SESS_REPORT_SAMPLES) Is Nothing Then
                Dim sPath As String = If(App.isRiskEnabled, _FILE_DATA_SAMPLES_RISK, _FILE_DATA_SAMPLES)
                Dim sFiles As List(Of String) = GetProjectFilesList(sPath, {".json", ".dash", ".rep"})
                For Each sFilename As String In sFiles
                    Dim sJSON As String = MyComputer.FileSystem.ReadAllText(String.Format("{0}/{1}", sPath, sFilename))
                    Dim Data As clsReportsCollection = Nothing
                    Dim sError As String = ""
                    Dim Meta As clsReportsMeta = clsReportsMeta.ParseJSON(Nothing, sJSON, Data, sError)
                    If Meta IsNot Nothing AndAlso Meta.Type <> ecReportsStreamType.Unspecified AndAlso Data IsNot Nothing AndAlso Data.Reports.Count > 0 AndAlso sError = "" Then
                        Dim Added As List(Of clsReport) = tList.AddToCollection(Data, ecReportCategory.All)
                    End If
                Next
                Session.Add(SESS_REPORT_SAMPLES, tList)
            Else
                tList = CType(Session(SESS_REPORT_SAMPLES), clsReportsCollection)
            End If
            Return tList
        End Function
        ' D6899 ==

        'D0041 ===
        Public Sub RawResponseStart()
            DebugInfo("Clear response on custom content reply")
            'Response.Buffer = False ' D4946
            'Response.BufferOutput = False   ' D4946
            'Response.Clear()
            ''Response.ClearHeaders()
            Response.ClearContent()
            ' D7625 ===
            Response.Headers.Remove("Server")
            Response.Headers.Remove("X-AspNet-Version")
            Response.Headers.Remove("X-Powered-By")
            Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate")
            Response.AppendHeader("Pragma", "no-cache")
            Response.Cache.SetLastModified(Now())
            Response.Cache.SetExpires(Now())
            Response.Cache.SetMaxAge(TimeSpan.Zero)
            Response.Cache.SetNoStore()
            Response.Cache.SetNoServerCaching()
            Response.Cache.SetCacheability(HttpCacheability.NoCache)
            If isSecureConnection(Request) Then Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains")
            ' D7625 ==
        End Sub

        Public Sub RawResponseEnd()
            If Response.IsClientConnected Then  ' D1232
                'Response.Flush()
                'Response.Close()   ' -D4946
            End If
            Response.End()
        End Sub
        ' D0041 ==

        ' D5024 ===
        Public Sub ResponseError(Code As HttpStatusCode, Optional Message As String = Nothing)
            Response.ClearContent()
            Response.StatusCode = CInt(Code)
            If Not String.IsNullOrEmpty(Message) Then Response.StatusDescription = ParseString(Message)    ' D7174
            Response.End()
        End Sub

        ' D5025 ===
        Public Sub FetchWithCode(Code As HttpStatusCode, Optional ReplyAsJSON As Boolean = True, Optional Message As String = Nothing)  ' D6430
            Message = ParseString(Message)  ' D7174
            If ReplyAsJSON Then
                SendResponseJSON(ecActionResult.arError, Message, If(String.IsNullOrEmpty(Message), ResString(String.Format("errAjax{0}", CInt(Code))), Message))
            Else
                ResponseError(Code, Message)
            End If
        End Sub

        Public Sub FetchNotFound(Optional ReplyAsJSON As Boolean = True, Optional Message As String = Nothing)
            FetchWithCode(HttpStatusCode.NotFound, ReplyAsJSON, Message)
        End Sub

        Public Sub FetchWrongAction(Optional ReplyAsJSON As Boolean = True, Optional Message As String = Nothing)
            FetchWithCode(HttpStatusCode.BadRequest, ReplyAsJSON, Message)
        End Sub

        Public Sub FetchNotImplemented(Optional ReplyAsJSON As Boolean = True, Optional Message As String = Nothing)
            FetchWithCode(HttpStatusCode.NotImplemented, ReplyAsJSON, Message)
        End Sub

        Public Sub FetchIfNotAuthorized(Optional ReplyAsJSON As Boolean = True, Optional Message As String = Nothing)
            If Not App.isAuthorized Then FetchWithCode(HttpStatusCode.Unauthorized, ReplyAsJSON, Message)
        End Sub

        Public Sub FetchIfNoActiveProject(Optional ReplyAsJSON As Boolean = True, Optional Message As String = "NO_ACTIVE_PROJECT")
            FetchIfNotAuthorized(ReplyAsJSON)
            If Not App.HasActiveProject Then FetchWithCode(HttpStatusCode.Forbidden, ReplyAsJSON, Message)
        End Sub

        ' D5026 ===
        Public Sub FetchNoPermissions(Optional ReplyAsJSON As Boolean = True, Optional Message As String = "NO_PERMISSIONS_FOR_ACTION")
            FetchWithCode(HttpStatusCode.Forbidden, ReplyAsJSON, Message)
        End Sub

        ' D5033 ===
        Public Sub FetchIfCantEditProject(Optional ReplyAsJSON As Boolean = True, Optional Message As String = "NO_PERMISSIONS_TO_EDIT_PROJECT")
            FetchWithCode(HttpStatusCode.Forbidden, ReplyAsJSON, Message)
        End Sub

        Public Function FetchIfCantEditProject(ProjectID As Integer, Optional ReplyAsJSON As Boolean = True, Optional Message As String = "NO_PERMISSIONS_TO_EDIT_PROJECT") As clsProject
            Dim tPrj As clsProject = FetchIfWrongProject(ProjectID, ReplyAsJSON)
            If Not App.CanActiveUserModifyProject(ProjectID) Then FetchIfCantEditProject(ReplyAsJSON, Message)
            Return tPrj
        End Function
        ' D5033 ==

        ' D5032 ===
        Public Sub FetchProjectNotFound(Optional ReplyAsJSON As Boolean = True, Optional Message As String = "PROJECT_NOT_FOUND")
            FetchWithCode(HttpStatusCode.Forbidden, ReplyAsJSON, Message)
        End Sub
        ' D5032 ==

        Public Sub FetchNoMethod(Optional ReplyAsJSON As Boolean = True, Optional Message As String = "METHOD_NOT_FOUND")
            FetchWithCode(HttpStatusCode.BadRequest, ReplyAsJSON, Message)
        End Sub
        ' D5026 ==

        Public Function FetchIfWrongProject(ProjectID As Integer, Optional ReplyAsJSON As Boolean = True, Optional Message As String = "PROJECT_NOT_FOUND") As clsProject
            FetchIfNotAuthorized(ReplyAsJSON)
            Dim tProject As clsProject = If(App.ProjectID = ProjectID, App.ActiveProject, clsProject.ProjectByID(ProjectID, App.ActiveProjectsList))    ' D6709
            If tProject Is Nothing Then FetchWithCode(HttpStatusCode.NotFound, ReplyAsJSON, Message)
            If tProject.isMarkedAsDeleted Then FetchWithCode(HttpStatusCode.Forbidden, ReplyAsJSON, "DELETED_PROJECT")
            Return tProject
        End Function
        ' D5025 ==

        Public Sub SendResponseJSON(Data As String)
            Response.ClearContent()
            Response.ContentType = "application/json"   ' D5031
            Response.Write(Data)
            Response.End()
        End Sub

        Public Sub SendResponseJSON(Data As Object)
            Select Case Data.GetType
                Case GetType(String)
                    SendResponseJSON(CStr(Data))
                Case GetType(jActionResult)
                    SendResponseJSON(CType(Data, jActionResult))
                Case Else
                    SendResponseJSON(JsonConvert.SerializeObject(Data))
            End Select
        End Sub

        Public Sub SendResponseJSON(Data As jActionResult)
            SendResponseJSON(JsonConvert.SerializeObject(Data))
        End Sub

        Public Sub SendResponseJSON(Result As ecActionResult, Data As Object, Message As String, Optional ObjectID As Integer = -1, Optional URL As String = "", Optional Tag As Object = Nothing)
            SendResponseJSON(New jActionResult With {.Result = Result, .Data = Data, .Message = Message, .ObjectID = ObjectID, .URL = URL, .Tag = Tag})
        End Sub
        ' D5024 ===

        ' D5025 ===
        Public Function ParseJSONParams(Optional sParams As String = Nothing) As NameValueCollection
            Dim tLst As New NameValueCollection
            If sParams Is Nothing Then sParams = CheckVar("params", "")
            If Not String.IsNullOrEmpty(sParams) AndAlso sParams <> """""" Then ' D5033
                Try
                    Dim Data As JObject = CType(JsonConvert.DeserializeObject(sParams), JObject)
                    If Data IsNot Nothing Then
                        For Each tParam As KeyValuePair(Of String, JToken) In Data
                            ' D6430 ===
                            If tParam.Value.GetType Is GetType(JArray) OrElse tParam.Value.GetType Is GetType(Array) OrElse tParam.Value.GetType Is GetType(JObject) Then   ' D6733
                                tLst.Add(CStr(tParam.Key), JsonConvert.SerializeObject(tParam.Value))
                            Else
                                tLst.Add(CStr(tParam.Key), CStr(tParam.Value))
                            End If
                            ' D6430 ==
                        Next
                    End If
                    'D7315 ===
                Catch ex As Exception
                    Dim sMsg As String = ResString("errParseJSON")
                    If Request.IsLocal Then sMsg = String.Format("{0}. [{1}]", sMsg, ex.Message)
                    If HookErrors() AndAlso CurrentPageID <> _PGID_ERROR_500 Then
                        App.DBSaveLog(dbActionType.actShowRTE, dbObjectType.einfRTE, -1, sMsg, sParams)
                        If (isAJAX AndAlso Not String.IsNullOrEmpty(CheckVar("params", ""))) OrElse CurrentPageID = _PGID_WEBAPI Then
                            FetchWithCode(HttpStatusCode.BadRequest, True, sMsg)
                        Else
                            App.ApplicationError.Init(ecErrorStatus.errRTE, CurrentPageID, ex.Message, Nothing, Request.Url.AbsoluteUri, ex)
                            App.Database.Close()    ' D2235
                            ' D7594 ===
                            'Response.ClearContent()
                            'Response.Clear()
                            'Server.Transfer(PageURL(_PGID_ERROR_500, GetTempThemeURI(True)), False)
                            RawResponseStart()
                            Response.ContentType = "text/plain"
                            Response.Write(sMsg)
                            RawResponseEnd()
                            ' D7594 ==
                        End If
                    Else
                        Throw New Exception(sMsg, ex)
                    End If
                End Try
                ' D7315 ==
            End If
            Return tLst
        End Function
        ' D5025 ==

        ' D6625 + D6593 ===
        Public Function DownloadFile(sFileName As String, sMime As String, sDownloadName As String, Optional sObjType As dbObjectType = dbObjectType.einfFile, Optional LogsObjID As Integer = -1, Optional forceDownload As Boolean = True, Optional fCloseResponse As Boolean = True) As Long  ' D6493
            RawResponseStart()
            Dim sToken As String = CheckVar("t", "")
            If Not String.IsNullOrEmpty(sToken) Then Response.AppendCookie(New HttpCookie("dl_token", sToken))   ' D6330  // token for control downloading on the client side
            sDownloadName = HttpUtility.UrlEncode(SafeFileName(sDownloadName), Text.Encoding.UTF8)
            Response.AppendHeader("Content-Disposition", String.Format("{1}; filename=""{0}"";  filename*=UTF-8''{0}", sDownloadName, If(forceDownload, "attachment", "inline")))    ' D6493
            If Not String.IsNullOrEmpty(sMime) Then Response.ContentType = sMime
            Dim fileLen As Long = New IO.FileInfo(sFileName).Length
            Response.AddHeader("Content-Length", CStr(fileLen))
            If fileLen > 0 Then
                'Response.TransmitFile(sFileName)
                Dim fs As New FileStream(sFileName, FileMode.Open, FileAccess.Read)
                Dim r As New BinaryReader(fs)
                Dim total As Integer = 0

                While total < fileLen
                    Dim Buff As Byte() = r.ReadBytes(_OPT_DOWNLOAD_BUFF_SIZE)
                    If Buff.GetUpperBound(0) >= 0 Then
                        total += Buff.GetUpperBound(0) + 1
                        Response.BinaryWrite(Buff)
                    End If
                End While
                r.Close()
                fs.Close()
            End If
            File_Erase(sFileName)
            App.DBSaveLog(dbActionType.actDownload, sObjType, LogsObjID, String.Format("Download file ({0})", sObjType.ToString.Substring(4)), String.Format("Filename: {0}; Size: {1}", sDownloadName, fileLen))
            If fCloseResponse Then RawResponseEnd()
            Return fileLen
        End Function
        ' D6625 ==

        Public Function DownloadContent(sContent As String, sMime As String, sDownloadName As String, Optional sObjType As dbObjectType = dbObjectType.einfFile, Optional LogsObjID As Integer = -1, Optional forceDownload As Boolean = True, Optional fCloseResponse As Boolean = True) As Long
            RawResponseStart()
            Dim sToken As String = CheckVar("t", "")
            If Not String.IsNullOrEmpty(sToken) Then Response.AppendCookie(New HttpCookie("dl_token", sToken))  ' token for control downloading on the client side
            sDownloadName = HttpUtility.UrlEncode(SafeFileName(sDownloadName), Text.Encoding.UTF8)
            Response.AppendHeader("Content-Disposition", String.Format("{1}; filename=""{0}"";  filename*=UTF-8''{0}", sDownloadName, If(forceDownload, "attachment", "inline")))
            If Not String.IsNullOrEmpty(sMime) Then Response.ContentType = sMime
            Dim fileLen As Long = System.Text.ASCIIEncoding.Unicode.GetByteCount(sContent)
            Response.AddHeader("Content-Length", fileLen.ToString)
            If fileLen > 0 Then
                Response.Write(sContent)
            End If
            App.DBSaveLog(dbActionType.actDownload, sObjType, LogsObjID, String.Format("Download file ({0})", sObjType.ToString.Substring(4)), String.Format("Filename: {0}; Size: {1}", sDownloadName, fileLen))
            If fCloseResponse Then RawResponseEnd()
            Return fileLen
        End Function

        Public Function DownloadStream(tStream As Stream, sMime As String, sDownloadName As String, Optional sObjType As dbObjectType = dbObjectType.einfFile, Optional LogsObjID As Integer = -1, Optional forceDownload As Boolean = True, Optional fCloseResponse As Boolean = True) As Long
            RawResponseStart()
            Dim sToken As String = CheckVar("t", "")
            If Not String.IsNullOrEmpty(sToken) Then Response.AppendCookie(New HttpCookie("dl_token", sToken))  ' token for control downloading on the client side
            sDownloadName = HttpUtility.UrlEncode(SafeFileName(sDownloadName), Text.Encoding.UTF8)
            Response.AppendHeader("Content-Disposition", String.Format("{1}; filename=""{0}"";  filename*=UTF-8''{0}", sDownloadName, If(forceDownload, "attachment", "inline")))
            If Not String.IsNullOrEmpty(sMime) Then Response.ContentType = sMime
            Dim fileLen As Long = tStream.Length
            Response.AddHeader("Content-Length", fileLen.ToString)
            If fileLen > 0 Then
                tStream.CopyTo(Response.OutputStream)
            End If
            App.DBSaveLog(dbActionType.actDownload, sObjType, LogsObjID, String.Format("Download file ({0})", sObjType.ToString.Substring(4)), String.Format("Filename: {0}; Size: {1}", sDownloadName, fileLen))
            If fCloseResponse Then RawResponseEnd()
            Return fileLen
        End Function
        ' D6593 ==

        ' D0107 ===
        Public Function PopupRichEditor(ByVal ObjType As reObjectType, ByVal sURLParams As String) As String
            Return (PagePopupAction(_PGID_RICHEDITOR, String.Format("type={0}&{1}{2}", CInt(ObjType), sURLParams, GetTempThemeURI(True)), 840, 500, True))  ' D1625
        End Function
        ' D0107 ==

        ' D3628 ===
        Public Function SolverStateHTML(tSolver As RASolver, Optional sExtraText As String = "") As String 'A0890: moved this routine from RA/Default.aspx.vb here in order to use it in all other pages + D4521
            Dim sRes As String = ""
            Dim fIsError As Boolean = False
            If tSolver IsNot Nothing Then
                Select Case tSolver.SolverState
                    Case raSolverState.raError
                        If Not String.IsNullOrEmpty(tSolver.LastError) Then
                            sRes += String.Format(ResString("errRAError"), tSolver.LastError)   ' D3628
                            fIsError = True
                        End If
                    Case raSolverState.raInfeasible
                        sRes += ResString("errRAInfeasible")    ' D3628
                        fIsError = True
                    Case raSolverState.raExceedLimits   ' D3228
                        sRes += ResString("errRAExceedLimit")    ' D3228 + D3628
                        fIsError = True ' D3228
                End Select
                ' D3825 ===
                If tSolver.Alternatives.Count = 0 Then
                    sRes = ResString("errRANoAlts")
                    fIsError = True
                End If
                ' D3825 ==
                If sRes <> "" Then
                    If sExtraText <> "" Then sRes += sExtraText ' D4521
                    sRes = String.Format("<h6 style='margin:1ex;padding:0px'{0}><span title='{2}'>{1}</span></h6>", IIf(fIsError, " class='error'", ""), sRes, CStr(IIf(tSolver.LastErrorReal <> "", tSolver.LastErrorReal, IIf(tSolver.LastError = sRes, "", tSolver.LastError))).Replace("""", "&quot;"))   ' D3879
                End If
                Dim sSolver As String = "Solver"
                Select Case tSolver.SolverLibrary
                    'Case raSolverLibrary.raMSF
                    '    sSolver = ResString("lblRASolverMSF")
                    Case raSolverLibrary.raXA
                        sSolver = ResString("lblRASolverXA")
                    Case raSolverLibrary.raGurobi
                        sSolver = ResString("lblRASolverGurobi")
                End Select
                sRes = sRes.Replace(_FILE_ROOT, "").Replace("%%solver%%", sSolver) ' D3628
            End If
            Return sRes
        End Function
        ' D3628 ==

        ' D0247 + D4175 ===
        Public Function GetRTEHeader() As String
            Dim sKind As String = ""
            Select Case App.ApplicationError.Status
                Case ecErrorStatus.errAccessDenied
                    sKind = "Access Denied"
                Case ecErrorStatus.errDatabase
                    sKind = "Database error"
                Case ecErrorStatus.errPageNotFound
                    sKind = "Page not found"
                Case ecErrorStatus.errRTE
                    sKind = "RTE"
                Case ecErrorStatus.errTest
                    sKind = "Test"
            End Select
            If sKind <> "" Then sKind += ": "
            'L0462 + D1400 ===
            Dim StackTraceHash As String = ""
            If App.ApplicationError.Details IsNot Nothing Then
                StackTraceHash = ParseStackTrace(App.ApplicationError.Details.StackTrace)
                StackTraceHash = String.Format(" [{0}]", Strings.Left(GetMD5(StackTraceHash), 7))
            End If
            Return If(App.ApplicationError.Status = ecErrorStatus.errNone, "", String.Format("{2}'{0}' [{1}]{3}", App.ApplicationError.Message, App.ApplicationError.PageID, sKind, StackTraceHash)) ' D6006
            'L0462 + D1400 ==
        End Function
        ' D0247 + D4175 ==

        ' D0119 ===
        Public Function GetApplicationStatus() As String
            Dim _tpl As String = ResString("lblProps_Template")

            ' D2914 ===
            Dim sRepo As String = ""
            If My.Computer.FileSystem.FileExists(_FILE_ROOT + "repo.txt") Then
                sRepo = File_GetContent(_FILE_ROOT + "repo.txt").Replace(vbCrLf + vbCrLf, vbCrLf)
                If sRepo <> "" Then sRepo = vbCrLf + " * " + sRepo.Replace(vbCrLf, vbCrLf + " * ").Trim().TrimEnd(CChar("*")).Replace(" * " + vbCrLf + " * ", " * Build ").Trim.TrimEnd(CChar("*")) ' D2919
            End If
            ' D2914 ==

            Dim _pgID As Integer = CurrentPageID
            If App.ApplicationError.Status <> ecErrorStatus.errNone OrElse App.ApplicationError.PageID > 0 Then _pgID = App.ApplicationError.PageID

            Dim sExtra As String = String.Format(" ##### Application Status Dump #####" + vbCrLf +
                                                 " * GMT: {0}" + vbCrLf +
                                                 " * Local time: {10}" + vbCrLf +
                                                 " * Page: '{1}' [#{2}]" + vbCrLf +
                                                 " * Application URL: {11}" + vbCrLf +
                                                 " * Application Path: {4}" + vbCrLf +
                                                 " * Root URI: {3}" + vbCrLf +
                                                 " * Canvas Core version: {5}" + vbCrLf +
                                                 " * ECCore version: {6}" + vbCrLf +
                                                 " * Canvas Module version: {7}" + vbCrLf +
                                                 " * Changeset: {8}{9}" + vbCrLf + vbCrLf,
                                                 Now().ToUniversalTime.ToString("r"),
                                                 PageTitle(_pgID), _pgID,
                                                 _URL_ROOT, _FILE_ROOT, GetVersion(App.GetCoreVersion()),
                                                 GetVersion(GetCurrentECCoreVersion()),
                                                 GetVersion(GetCurrentCanvasModuleVersion()),
                                                 WebConfigOption(_OPT_CHANGESET, "Unknown", True), sRepo, Now(), ApplicationURL(False, False))    ' D0141 + D0571 + D1400 + D2914 + D4175

            sExtra = String.Format("{0} * InstanceID: {1}" + vbCrLf +
                                    " * Server name: {2}" + vbCrLf +
                                    " * DB Instance: {3}" + vbCrLf +
                                    " * Master DB: {4}" + vbCrLf +
                                    " * Draft mode: {5}" + vbCrLf + vbCrLf,
                                    sExtra, App.GetInstanceID_AsString(), System.Environment.MachineName, App.CanvasMasterConnectionDefinition.Server, App.Options.CanvasMasterDBName, Bool2YesNo(ShowDraftPages))  ' D4175

            If App.ApplicationError.Status = ecErrorStatus.errRTE Then
                Dim err As Exception = App.ApplicationError.Details     ' D0466
                If Not err Is Nothing Then
                    ' D0168 ===
                    Dim sInnerEx As String = ""
                    ' D0169 ===
                    If Not err.InnerException Is Nothing Then
                        sInnerEx = err.InnerException.Message
                        If Not err.InnerException.InnerException Is Nothing Then
                            sInnerEx += "; " + err.InnerException.InnerException.Message
                            If Not err.InnerException.InnerException.InnerException Is Nothing Then sInnerEx += "; " + err.InnerException.InnerException.InnerException.Message
                        End If
                    End If
                    ' D0169 ==
                    sExtra += String.Format(" === Application Error === " + vbCrLf +
                                            " * Message: '{0}'" + vbCrLf +
                                            " * Source: '{1}'" + vbCrLf + vbCrLf +
                                            " === Stack Trace ===" + vbCrLf + "{2}" + vbCrLf + "{3}" + vbCrLf + vbCrLf,
                                            err.Message, err.Source, ShortString(err.StackTrace, CInt(IIf(sInnerEx = "", 4096, 2048)), True), ShortString(sInnerEx, 2048, True))   ' D1075
                    ' D0168 ==
                End If
            End If


            Dim sRefURL As String = ""  ' D0880
            If Request IsNot Nothing AndAlso Request.UrlReferrer IsNot Nothing Then sRefURL = Request.UrlReferrer.AbsoluteUri ' D0880
            sExtra += String.Format(" === Location ===" + vbCrLf +
                                    " * Local: {0}" + vbCrLf +
                                    " * Host: {1}" + vbCrLf +
                                    " * URL: {2}" + vbCrLf +
                                    " * Referrer: {3}" + vbCrLf +
                                    " * Is Callback: {4}" + vbCrLf + vbCrLf,
                                    Request.IsLocal.ToString,
                                    Request.Url.Host,
                                    If(App.ApplicationError.PageURL = "" AndAlso Request IsNot Nothing, Request.RawUrl, App.ApplicationError.PageURL), sRefURL, isCallback)   ' D0466 + D0880 + D4175 + D5063


            sExtra += " === User information ===" + vbCrLf
            If Not App.ActiveUser Is Nothing Then
                sExtra += String.Format(" * ID: {0}" + vbCrLf +
                                        " * Email: {1}" + vbCrLf +
                                        " * Name: {2}" + vbCrLf + vbCrLf,
                                        App.ActiveUser.UserID,
                                        App.ActiveUser.UserEmail,
                                        App.ActiveUser.UserName)    ' D0466
            Else
                sExtra += " (-) unauthorized" + vbCrLf + vbCrLf
            End If


            sExtra += " === User-Session information: ===" + vbCrLf
            Dim tWorkgroup As clsWorkgroup = App.ActiveWorkgroup
            If tWorkgroup Is Nothing Then
                sExtra += " * Workgroup: (-) unavailable" + vbCrLf + vbCrLf
            Else
                sExtra += String.Format(" * Workgroup: '{0}' (ID {1})" + vbCrLf +
                                        " * Workgroup Status: {2}" + vbCrLf,
                                        tWorkgroup.Name,
                                        tWorkgroup.ID.ToString + CStr(IIf(App.ActiveUser Is Nothing, "", IIf(App.ActiveUser.DefaultWorkgroupID = tWorkgroup.ID, ", default", ""))),
                                        ResString(String.Format(_tpl, tWorkgroup.Status.ToString)))

                Dim tUW As clsUserWorkgroup = App.ActiveUserWorkgroup
                If tUW Is Nothing Then
                    sExtra += " * User Workgroup: (-) unavailable" + vbCrLf
                Else
                    Dim sRoleName As String = "unknown"
                    Dim sRoleType As String = "unknown"
                    If Not App.ActiveRoleGroup Is Nothing Then
                        sRoleName = App.ActiveRoleGroup.Name
                        sRoleType = ResString(String.Format(_tpl, App.ActiveRoleGroup.GroupType.ToString))
                    End If
                    sExtra += String.Format(" * User Workgroup ID: {0}" + vbCrLf +
                                            " * User Workgroup Status: {1}" + vbCrLf +
                                            " * Role Group Type: '{3}'" + vbCrLf +
                                            " * Role Group Name: '{2}'" + vbCrLf,
                                            tUW.ID,
                                            ResString(String.Format(_tpl, App.ActiveWorkgroup.Status.ToString)),
                                            sRoleName,
                                            sRoleType)  ' D0466 + D0513
                End If
                sExtra += vbCrLf
            End If

            sExtra += " === Project information: ===" + vbCrLf
            If App.HasActiveProject() Then
                Dim tWorkSpace As clsWorkspace = App.ActiveWorkspace    ' D0466
                Dim sWorkspaceStatus As String = "not linked"
                Dim CurStep As String = "?" ' D0914
                Dim sWorkspaceStatus2 As String = "not linked"  ' D1945
                Dim CurStep2 As String = "?" ' D1945
                If Not tWorkSpace Is Nothing Then
                    ' D1945 ===
                    sWorkspaceStatus = ResString(String.Format(_tpl, tWorkSpace.StatusLikelihood.ToString))
                    CurStep = tWorkSpace.ProjectStepLikelihood.ToString   ' D0914
                    sWorkspaceStatus2 = ResString(String.Format(_tpl, tWorkSpace.StatusImpact.ToString))
                    CurStep2 = tWorkSpace.ProjectStepImpact.ToString
                    ' D1945 ==
                End If
                Dim tPrjUser As clsUser = Nothing
                ' D0126 ===
                Dim sUserActive As String = "False"
                Dim sUserHasData As String = "False"
                If Not App.ActiveUser Is Nothing Then   ' D0459
                    tPrjUser = App.ActiveProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEmail)
                    If Not tPrjUser Is Nothing Then ' D0223
                        sUserActive = tPrjUser.Active.ToString
                        sUserHasData = MiscFuncs.DataExistsInProject(clsProject.StorageType, App.ProjectID, App.ActiveProject.ConnectionString, App.ActiveProject.ProviderType, tPrjUser.UserEMail).ToString     'C0274 + D0369 + D0376
                    End If
                    ' D0126 ==
                End If
                ' D0760 + D0948 ===
                Dim sLockedBy As String = ""
                If App.HasActiveProject AndAlso App.ActiveProject.LockInfo IsNot Nothing Then
                    If App.ActiveProject.LockInfo.LockStatus <> ECLockStatus.lsUnLocked Then
                        Dim tUser As clsApplicationUser = Nothing
                        If App.ActiveUser IsNot Nothing AndAlso App.ActiveProject.LockInfo.LockerUserID = App.ActiveUser.UserID Then tUser = App.ActiveUser
                        If tUser Is Nothing Then App.DBUserByID(App.ActiveProject.LockInfo.LockerUserID)
                        If tUser IsNot Nothing AndAlso App.ActiveProject.LockInfo.LockerUserID > 0 Then
                            sLockedBy = ", by " + tUser.UserEmail
                        End If
                    End If
                    sLockedBy = App.ActiveProject.LockInfo.LockStatus.ToString + sLockedBy
                End If
                ' D0760 + D0948 ==
                sExtra += String.Format(" * ID: {0}" + vbCrLf +
                                        " * Name: {1}" + vbCrLf +
                                        " * Passcode: " + CStr(IIf(App.isRiskEnabled, "Likelihood - {2}, Impact - {23}", "{2}")) + vbCrLf +
                                        " * Project Type: {24}" + vbCrLf +
                                        " * Version: {10}" + vbCrLf +
                                        " * FileName: {3}" + vbCrLf +
                                        " * isAvailable: {11}" + vbCrLf +
                                        CStr(IIf(App.isRiskEnabled, " * Active Hierarchy: {18}" + vbCrLf, "")) +
                                        " * Project Status: {4}" + vbCrLf +
                                        " * Marked as deleted: {5}" + vbCrLf +
                                        " * Access Code available: {6}" + vbCrLf +
                                        " * Project is On-line: {16}" + vbCrLf +
                                        " * Locked by: {12}" + vbCrLf +
                                        " * Editable: {13}" + vbCrLf +
                                        " * TeamTime status: " + CStr(IIf(App.isRiskEnabled, "Likelihood: {14}, Impact: {19}", "{14}")) + vbCrLf +
                                        " * Load on-demand status: {15}, loaded: {20}" + vbCrLf + vbCrLf +
                                        " === Workspace information ===" + vbCrLf +
                                        " * Workspace Status: " + CStr(IIf(App.isRiskEnabled, "Likelihood: {7}, Impact: {21}", "{7}")) + vbCrLf +
                                        " * Is Participating: {8}" + vbCrLf +
                                        " * Has Data In Project: {9}" + vbCrLf +
                                        " * Current step: " + CStr(IIf(App.isRiskEnabled, "Likelihood: {17}, Impact: {22}", "{17}")) + vbCrLf,
                                        App.ProjectID,
                                        App.ActiveProject.ProjectName,
                                        App.ActiveProject.PasscodeLikelihood,
                                        App.ActiveProject.ProjectName,
                                        ResString(String.Format(_tpl, App.ActiveProject.ProjectStatus.ToString)),
                                        App.ActiveProject.isMarkedAsDeleted.ToString,
                                        App.ActiveProject.isPublic.ToString,
                                        sWorkspaceStatus,
                                        sUserActive,
                                        sUserHasData,
                                        App.ActiveProject.DBVersion.GetVersionString,
                                        True,
                                        sLockedBy,
                                        CanEditActiveProject.ToString,
                                        App.ActiveProject.isTeamTimeLikelihood.ToString,
                                        App.ActiveProject.isLoadOnDemand.ToString,
                                        App.ActiveProject.isOnline.ToString,
                                        CurStep,
                                        IIf(App.ActiveProject.isImpact, "Impact", "Likelihood"),
                                        App.ActiveProject.isTeamTimeImpact.ToString,
                                        IIf(App.ActiveProject.IsProjectLoaded, "Yes", "No"),
                                        sWorkspaceStatus2,
                                        CurStep2,
                                        App.ActiveProject.PasscodeImpact, GetProjectTypeName(App.ActiveProject))  ' D0126 + D0141 + D0146 + D0223 + D0300 + D0376 + D0466 + D0471 + D0483 + D0748 + D0760 + D0789 + D0914 + D0948 + D1193 + D1945 + D4407 + D4431 + D7505

                ' D4175 ===
                If App.isSnapshotsAvailable Then
                    If App.LastSnapshot IsNot Nothing Then
                        sExtra += String.Format(" * Latest snapshot: #{0} ({1}, {2}) " + vbCrLf, App.LastSnapshot.Idx, App.LastSnapshot.SnapshotID, App.LastSnapshot.Comment)   ' D7505
                    End If
                End If
                ' D4175 ==


                If tWorkSpace IsNot Nothing Then   ' D0466 + D4175
                    Dim tRole As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tWorkSpace.GroupID)
                    If Not tRole Is Nothing Then
                        sExtra += String.Format(" * User Active Role Group: '{0}'" + vbCrLf +
                                                " * Role Group Type: '{1}'" + vbCrLf +
                                                " * Role Group Status: '{2}'" + vbCrLf,
                                                tRole.Name,
                                                ResString(String.Format(_tpl, tRole.GroupType)),
                                                ResString(String.Format(_tpl, tRole.Status)))   ' D1945 + D4175
                    End If
                End If
                sExtra += vbCrLf
            Else
                sExtra += " (-) unavailable" + vbCrLf + vbCrLf
            End If

            sExtra += String.Format(" === Web-Session details === " + vbCrLf +
                                    " * Browser: {0} (isMobile: {6})" + vbCrLf +
                                    " * Platform: {1}" + vbCrLf +
                                    " * Host Address: {2}" + vbCrLf +
                                    " * Host Name: {3}" + vbCrLf +
                                    " * User-Agent: {5}" + vbCrLf +
                                    " * UID/SessID (Logs): {4}" + vbCrLf + vbCrLf,
                                    Request.Browser.Browser, Request.Browser.Platform,
                                    GetClientIP(Request), Request.UserHostName, App.Options.SessionID, Request.UserAgent, App.isMobileBrowser.ToString)   ' D2289 + D2959 + D2977 + D7115

            ' D1631 ===
            If Request.Url IsNot Nothing Then
                Dim sURL As String = Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) + _URL_ROOT
                If App.HasActiveProject Then
                    sExtra += String.Format(vbCrLf + " > Link to this project by passcode: {0}?{1}={2}" + vbCrLf, sURL, _PARAM_PASSCODE, App.ActiveProject.Passcode) ' D2440
                    If App.ActiveUser IsNot Nothing AndAlso Not App.ActiveUser.CannotBeDeleted Then ' D2440
                        ' D2237 ===
                        If App.isRiskEnabled AndAlso App.ActiveProject.IsProjectLoaded Then
                            Dim bIsImpact As Boolean = App.ActiveProject.isImpact
                            App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
                            sExtra += String.Format(vbCrLf + " > Link to project for this user (likelihood hierarchy): {0}" + vbCrLf, CreateLogonURL(App.ActiveUser, App.ActiveProject, "", sURL, , App.ActiveUserWorkgroup))  ' D1672 + D4616
                            App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
                            sExtra += String.Format(" > Link to project for this user (impact hierarchy): {0}" + vbCrLf, CreateLogonURL(App.ActiveUser, App.ActiveProject, "", sURL, , App.ActiveUserWorkgroup)) ' D1672 + D1734 + D4616
                            App.ActiveProject.ProjectManager.ActiveHierarchy = CInt(IIf(bIsImpact, ECHierarchyID.hidImpact, ECHierarchyID.hidLikelihood))
                        Else
                            sExtra += String.Format(vbCrLf + " > Link to project for this user: {0}" + vbCrLf, CreateLogonURL(App.ActiveUser, App.ActiveProject, "", sURL, , App.ActiveUserWorkgroup))   ' D4616
                        End If
                        ' D2237 ==
                    End If
                End If
                If App.isAuthorized AndAlso Not App.ActiveUser.CannotBeDeleted Then sExtra += String.Format(" > Link for user '{1}': {0}" + vbCrLf, CreateLogonURL(App.ActiveUser, Nothing, "", sURL, , App.ActiveUserWorkgroup), App.ActiveUser.UserEmail) ' D4616
            End If
            ' D1631 ==

            Return sExtra
        End Function

        ' D5063 ===
        ' -D7082
        'Public Function getSessionStateJSON() As String
        '    Dim sUser As String = If(App.isAuthorized AndAlso App.ActiveUser IsNot Nothing, jAppUserShort.CreateFromBaseObject(App.ActiveUser).ToJSON, "{""ID"":-1,""Email"":""""}")    ' D7208
        '    Dim sWkg As String = If(App.isAuthorized AndAlso App.ActiveWorkgroup IsNot Nothing, jWorkgroup.CreateFromBaseObject(App.ActiveWorkgroup).ToJSON, "{""ID"":-1}")    ' D7208
        '    Dim sProject As String = If(App.HasActiveProject, jProject.GetProjectByID(App, App.ProjectID).ToJSON(), "{""ID"":-1}")   ' D5040 + D7267

        '    Dim sOther As String = ""
        '    '' D7197 ===
        '    'If App.HasActiveProject AndAlso App.ActiveWorkspace IsNot Nothing Then
        '    '    sOther += String.Format(", ""workspace"": {{""lastVisited"": {0}}}", JsonConvert.SerializeObject(If(App.ActiveWorkspace.LastModify.HasValue, App.ActiveWorkspace.LastModify.Value, Nothing)))
        '    'End If
        '    '' D7197 ==

        '    Return String.Format("""sid"":""{0}"",""usid"":""{5}"",""user"":{1},""wkg"":{2},""project"":{3}{4}", If(Session Is Nothing, "", Session.SessionID), sUser, sWkg, sProject, sOther, App.Options.SessionID)  ' D7203
        'End Function
        ' D5063 ==

        ' D0628 ===
        Public Function StorePageInformation() As Boolean
            Dim sURL As String = ""
            Dim sHost As String = "unknown" ' D3269
            If Not Request Is Nothing Then
                sURL = Request.Url.ToString
                If StorePageCustomURL <> "" Then sURL = StorePageCustomURL ' D0628
                Dim sRoot As String = CStr(ApplicationURL(False, False) + _URL_ROOT).Replace("//", "/").Replace(":/", "://")   ' D3308 + D3494
                sURL = sURL.Replace(sRoot, "")
                sHost = GetClientIP(Request)    ' D5084
            End If

            Dim tParams As New List(Of Object)
            tParams.Add(Now)
            tParams.Add(CurrentPageID)
            tParams.Add(sURL)
            ' D0494 ===
            tParams.Add(App.ActiveWorkgroup.ID)
            tParams.Add(String.Format("{0}/{1}", ShortString(sHost, 60), App.Options.SessionID))  ' D3269 + D4029 // was sHost
            tParams.Add(App.ActiveUser.UserID)
            Try
                App.Database.ExecuteSQL(String.Format("UPDATE {0} SET {2}=?, {3}=?, {4}=?, {5}=?, {6}=?, {7}=1 WHERE {1}=?", clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_ID, clsComparionCore._FLD_USERS_LASTVISITED, clsComparionCore._FLD_USERS_LASTPAGEID, clsComparionCore._FLD_USERS_LASTURL, "LastWorkgroupID", "SessionID", "isOnline"), tParams)
            Catch ex As Exception
                'App.DBSaveLog(dbActionType.actSendRTE, dbObjectType.einfPage, CurrentPageID, "Unable to save last visited page info", sURL + " / " + sHost + " / RTE: " + ex.Message) ' D3269 -D4029
                Return False
            End Try
            ' D0494 ==
            Return True
        End Function
        ' D0628 ==

        ' D0140 ===
        Public Property SessVar(ByVal sName As String) As String
            Get
                If Session(sName) Is Nothing Then Return Nothing Else Return CStr(Session(sName))
            End Get
            Set(ByVal value As String)
                If value Is Nothing Then
                    If Session(sName) IsNot Nothing Then Session.Remove(sName)  ' D7007
                Else
                    If Session(sName) Is Nothing Then Session.Add(sName, value) Else Session(sName) = value
                End If
            End Set
        End Property
        ' D0140 ==

        Public Function HasPermissionByAction(ByVal tActionType As ecActionType, ByRef fInitApplicationError As Boolean, ByVal fOnlyWarning As Boolean) As Boolean
            If Not App.Options.CheckLicense Then Return True ' D0315 + D0460
            Dim fLicensePassed As Boolean = True
            Dim tCurWG As clsWorkgroup = App.ActiveWorkgroup
            Dim tSysWG As clsWorkgroup = App.SystemWorkgroup
            Dim sMessage As String = ""
            If Not tCurWG Is Nothing Then
                Select Case tActionType
                    Case ecActionType.at_slCreateWorkgroup
                        fLicensePassed = tCurWG.License.CheckParameterByID(ecLicenseParameter.MaxWorkgroupsTotal, Nothing, False)   ' D0913
                        If Not fLicensePassed Then sMessage = App.LicenseErrorMessage(tCurWG.License, ecLicenseParameter.MaxWorkgroupsTotal) ' D0913
                    Case ecActionType.at_alCreateNewModel, ecActionType.at_alUploadModel
                        fLicensePassed = tCurWG.License.CheckParameterByID(ecLicenseParameter.MaxProjectsTotal, Nothing, False)   ' D0913
                        ' D0404 ===
                        If fLicensePassed Then
                            fLicensePassed = tCurWG.License.CheckParameterByID(ecLicenseParameter.MaxLifetimeProjects, Nothing, False)  ' D0919
                            If fLicensePassed AndAlso App.SystemWorkgroup IsNot Nothing Then fLicensePassed = App.SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxLifetimeProjects, Nothing, False) ' D0919
                            ' -D0418
                            'fLicensePassed = tCurWG.License.CheckParameterByID(MAX_ONLINE_MODELS_TOTAL, Nothing, False) ' D0417
                            ' D0404 ==
                            ' D2548 ===
                            If fLicensePassed And Not tSysWG Is Nothing Then
                                fLicensePassed = tSysWG.License.CheckParameterByID(ecLicenseParameter.MaxProjectsTotal, Nothing, False)   ' D0913
                                If Not fLicensePassed Then sMessage = App.LicenseErrorMessage(tSysWG.License, ecLicenseParameter.MaxProjectsTotal) ' D0913
                                'If fLicensePassed Then
                                '    fLicensePassed = tCurWG.License.CheckParameterByID(ecLicenseParameter.MaxModelsPerOwner, App.ActiveUser, False)    ' D0913
                                '    If fLicensePassed And Not tSysWG Is Nothing Then
                                '        fLicensePassed = tSysWG.License.CheckParameterByID(ecLicenseParameter.MaxModelsTotal, Nothing, False)   ' D0913
                                '        If Not fLicensePassed Then sMessage = App.LicenseErrorMessage(tSysWG.License, ecLicenseParameter.MaxModelsTotal) ' D0913
                                '    Else
                                '        sMessage = App.LicenseErrorMessage(tCurWG.License, ecLicenseParameter.MaxModelsPerOwner)   ' D0913
                                '    End If
                                '    ' D0404 ===
                                '    ' -D0418
                                '    'Else
                                '    '        sMessage = LicenseErrorMessage(tCurWG.License, MAX_ONLINE_MODELS_TOTAL) ' D0417
                                ' D2548 ==
                            Else
                                sMessage = App.LicenseErrorMessage(tCurWG.License, ecLicenseParameter.MaxLifetimeProjects)  ' D0919
                            End If
                            ' D0404 ==
                        Else
                            sMessage = App.LicenseErrorMessage(tCurWG.License, ecLicenseParameter.MaxProjectsTotal)  ' D0913
                        End If
                        ' -D2780
                        '    ' D0285 ===
                        'Case ecActionType.at_alManageSurveys
                        '    fLicensePassed = tCurWG.License.CheckParameterByID(ecLicenseParameter.SpyronEnabled, Nothing, True) ' D0913
                        '    If Not fLicensePassed Then sMessage = App.LicenseErrorMessage(tCurWG.License, ecLicenseParameter.SpyronEnabled) ' D0913
                        '    ' D0285 ==
                End Select
                If Not fLicensePassed And fInitApplicationError Then App.LicenseInitError(sMessage, fOnlyWarning)
            End If
            Return fLicensePassed
        End Function

        ' D0045 ===
        Public Overloads Function HasPermission(ByVal tPageAction As clsPageAction, ByVal tProject As clsProject, Optional tWorkgroup As clsWorkgroup = Nothing) As Boolean ' D7270
            Dim tUser As clsApplicationUser = App.ActiveUser
            ' D7270 ===
            If tWorkgroup Is Nothing Then tWorkgroup = App.ActiveWorkgroup
            Dim tUserWorkgroup As clsUserWorkgroup = Nothing
            If tWorkgroup IsNot Nothing AndAlso tUser IsNot Nothing Then tUserWorkgroup = If(App.ActiveWorkgroup IsNot Nothing AndAlso tWorkgroup.ID = App.ActiveWorkgroup.ID, App.ActiveUserWorkgroup, clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, tWorkgroup.ID, App.UserWorkgroups))
            Dim tUserRoleGroup As clsRoleGroup = Nothing
            If tUserWorkgroup IsNot Nothing AndAlso tWorkgroup IsNot Nothing Then tUserRoleGroup = tWorkgroup.RoleGroup(tUserWorkgroup.RoleGroupID, tWorkgroup.RoleGroups)
            Dim tWorkspace As clsWorkspace = Nothing
            If tProject Is Nothing Then tProject = App.ActiveProject
            If tProject IsNot Nothing AndAlso tUser IsNot Nothing Then tWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tProject.ID, App.Workspaces)
            ' D7270 ==

            If tPageAction Is Nothing Then
                DebugInfo("Couldn't check permissions for unregistered page!", _TRACE_WARNING)
                Return False
            End If

            ' D0197 ===
            If Not ShowDraftPages() Then   ' D0315
                If isDraftPage(tPageAction.ID) Then    ' D0459
                    DebugInfo(String.Format("Page #{0} restricted as DRAFT", tPageAction.ID), _TRACE_WARNING)
                    Return False
                End If
            End If
            ' D0197 ==

            ' D4624 ===
            If tPageAction.ID = _PGID_ADMIN_WORKGROUP_WORDING_TEMPLATE AndAlso (tWorkgroup Is Nothing OrElse tWorkgroup.Status = ecWorkgroupStatus.wsSystem) Then Return False
            If tWorkgroup IsNot Nothing AndAlso tWorkgroup.Status = ecWorkgroupStatus.wsSystem Then
                Select Case tPageAction.ID
                    Case _PGID_ADMIN_LICENSE
                        If Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slViewLicenseAnyWorkgroup, tUser.UserID) Then Return False
                    Case _PGID_ADMIN_LOGINS_STAT, _PGID_ADMIN_PRJ_LOOKUP, _PGID_ADMIN_PRJ_STAT, _PGID_ADMIN_STATISTIC
                        Return False
                    Case _PGID_ADMIN_ROLEGROUPS, _PGID_ADMIN_USERSLIST ' , _PGID_ADMIN_WORKGROUPS
                        If Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, tUser.UserID) Then Return False
                End Select
            End If
            ' D4624 ==

            ' -D2780
            '' D1962 ===
            'If tPageAction.ActionType = ecActionType.at_alManageSurveys Then
            '    tPageAction = New clsPageAction(tPageAction.ID, tPageAction.Name, tPageAction.URL, ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects)
            'End If
            '' D1962 ==

            '' D1962 ===
            'If tPageAction.ActionType = ecActionType.at_alManageSurveys Then
            '    tPageAction = New clsPageAction(tPageAction.ID, tPageAction.Name, tPageAction.URL, ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects)
            'End If
            '' D1962 ==

            Dim sUserInfo As String = "unknown"
            If Not tUser Is Nothing Then sUserInfo = tUser.UserEmail
            If Not tUserRoleGroup Is Nothing Then
                sUserInfo = String.Format("{0} with group {1}", sUserInfo, tUserRoleGroup.Name)
            End If
            Dim sStatus As String = "none"
            If Not tProject Is Nothing Then sStatus = tProject.Passcode
            ' D0052 ===
            Dim WS As clsWorkspace = Nothing
            Dim tProjectRoleGroup As clsRoleGroup = Nothing
            If Not tUser Is Nothing And Not tProject Is Nothing Then
                If Not App.Workspaces Is Nothing Then WS = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tProject.ID, App.Workspaces)
            End If
            Dim fCanManageAnyDecision As Boolean = App.CanUserDoAction(ecActionType.at_alManageAnyModel, tUserWorkgroup)    ' D0450 + D0466 + D0930
            Dim fCanSeeAllDecisions As Boolean = App.CanUserDoAction(ecActionType.at_alViewAllModels, tUserWorkgroup)    ' D2785
            Dim fCanCreateProject As Boolean = App.CanUserDoAction(ecActionType.at_alCreateNewModel, tUserWorkgroup)    ' D2544
            If tWorkgroup IsNot Nothing Then
                Dim RGID As Integer = -1    ' D0930
                If WS IsNot Nothing Then RGID = WS.GroupID ' D0930
                If fCanManageAnyDecision OrElse (tProject IsNot Nothing AndAlso fCanCreateProject AndAlso (tProject.ProjectStatus = ecProjectStatus.psMasterProject OrElse tProject.ProjectStatus = ecProjectStatus.psTemplate) AndAlso Not tProject.isMarkedAsDeleted) Then    ' D0060 + D2544
                    RGID = tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)    ' D0087 + D0930 + D2780
                    If WS IsNot Nothing Then WS.GroupID = RGID ' D0930
                End If
                If WS Is Nothing AndAlso fCanSeeAllDecisions AndAlso tProject IsNot Nothing AndAlso tProject.ProjectStatus = ecProjectStatus.psActive Then RGID = tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator) ' D2785
                If RGID > 0 Then    ' D0930
                    tProjectRoleGroup = tWorkgroup.RoleGroup(RGID)
                    If Not tProjectRoleGroup Is Nothing Then sUserInfo = String.Format("{0} + {1}", sUserInfo, tProjectRoleGroup.Name) Else WS = Nothing
                End If
            End If
            DebugInfo(String.Format("Check permissions. User: {0}. Project: '{1}'. Page: '{2}', require '{3}', '{4}'", sUserInfo, sStatus, tPageAction.Name, tPageAction.Permission.ToString, tPageAction.ActionType.ToString))
            ' D0052 ==

            ' -D4842
            '' D0204 ===
            'If tPageAction.ID = _PGID_WORKGROUP_SELECT Then
            '    If tUser Is Nothing Or App.UserWorkgroups Is Nothing Then Return False ' D0466
            '    If App.UserWorkgroups.Count < 2 Then Return False ' D0270
            'End If
            '' D0204 ===

            ' D0304 ===
            If App.Options.isSingleModeEvaluation Then
                If Array.IndexOf(_PAGESLIST_PASSCODE_ONLY, tPageAction.ID) < 0 AndAlso tPageAction.Permission <> ecPagePermission.ppUnspecified Then Return False ' D0390
            End If
            ' D0304 ==

            ' moved up in 0391
            ' D0285 + D0286 ===
            If HasListPage(_PAGESLIST_TEAMTIME, tPageAction.ID) Then     ' D0459
                If Not App.isTeamTimeAvailable OrElse tProject Is Nothing OrElse tProject.ProjectStatus <> ecProjectStatus.psActive Then Return False ' D0488 + D6096 + D6103
            End If
            ' D6239 ===
            If HasListPage(_PAGESLIST_ANTIGUA, tPageAction.ID) Then     ' D0459
                If Not App.isTeamTimeAvailable OrElse (tProject IsNot Nothing AndAlso tProject.ProjectStatus <> ecProjectStatus.psActive) Then Return False
                If tUser IsNot Nothing AndAlso tProject IsNot Nothing AndAlso tUserWorkgroup IsNot Nothing AndAlso Not App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, tUser.UserID, tProject.ID, tUserWorkgroup) Then Return False ' D6322 + D6328
            End If
            ' D6239 ==
            If HasListPage(_PAGESLIST_SPYRON, tPageAction.ID) Then  ' D0459
                If Not App.isSpyronAvailable Then Return False ' D0488
            End If
            ' D2922 ===
            If HasListPage(_PAGESLIST_RA, tPageAction.ID) Then
                If Not App.isRAAvailable OrElse tProject Is Nothing OrElse tProject.ProjectStatus <> ecProjectStatus.psActive Then Return False ' D6096 + D6103
            End If
            ' D0285 + D0286 + D2922 ==

            ' D0919 ===
            If HasListPage(_PAGESLIST_EXPORTS, tPageAction.ID) Then
                If Not App.isExportAvailable Then Return False
            End If
            ' D0919 ==

            If tPageAction.RiskAccess = ecPageRiskAccessType.raRiskOnly AndAlso Not App.isRiskEnabled Then Return False ' D2256
            If tPageAction.RiskAccess = ecPageRiskAccessType.raNoRisk AndAlso App.isRiskEnabled Then Return False ' D2256

            If tPageAction.Permission = ecPagePermission.ppUnspecified Then Return False
            If tPageAction.Permission = ecPagePermission.ppEveryone Then Return True

            If tPageAction.Permission = ecPagePermission.ppUnAuthorized Then If Not tUser Is Nothing Then Return False

            ' throw below only authorized = (tPageAction.Permission = ecPagePermission.ppAuthorized) 
            If tUser Is Nothing Then Return False

            ' D7503 ===
            If _MFA_REQUIRED AndAlso tUser IsNot Nothing AndAlso App.MFA_Requested AndAlso App.GetMFA_Mode(App.ActiveUser) = ecPinCodeType.mfaEmail Then
                Select Case tPageAction.Permission
                    Case ecPagePermission.ppAuthorized, ecPagePermission.ppProject, ecPagePermission.ppProjectWithLock
                        Return False
                End Select
            End If
            ' D7503 ==

            ' -D4943 disable since sometimes we can login by hash when EULA is not accepted yet
            '' D0272 ===
            'If tPageAction.Permission = ecPagePermission.ppAuthorized Then
            '    ' D0323 ===
            '    Dim tUsrWkg As clsUserWorkgroup = tUserWorkgroup
            '    If tUsrWkg Is Nothing Then Return False
            '    If tPageAction.ID <> _PGID_EULA AndAlso Not IsValidEULA() Then Return False ' D0472 + D0741 + D0766 + D2796
            '    ' D0323 ==
            'End If
            '' D0272 ==

            If tPageAction.ID = _PGID_LOGOUT OrElse tPageAction.ID = _PGID_ACCOUNT_EDIT Then Return True ' D0097 + D0255
            If tUserWorkgroup Is Nothing Then Return False ' D0466

            Dim fDisabledWorkgroup As Boolean = False   ' D0348

            ' D0089 ===
            If Not tWorkgroup Is Nothing Then
                If tWorkgroup.Status = ecWorkgroupStatus.wsSystem Then
                    If tPageAction.PageType = ecPageAccessType.paOnlyProjects Then Return False ' D0093
                Else
                    If tPageAction.PageType = ecPageAccessType.paOnlySystem Then Return False ' D0093
                End If
                fDisabledWorkgroup = tWorkgroup.Status = ecWorkgroupStatus.wsDisabled  ' D0348
            End If
            ' D0089 ==

            ' D0262 ===
            If App.Options.CheckLicense Then ' D0315
                Select Case tPageAction.ID
                    'Case _PGID_ERROR_403, _PGID_ERROR_404, _PGID_ERROR_500, _PGID_ERROR_503, _PGID_START, _PGID_START_WITH_SIGNUP, _PGID_WORKGROUP_SELECT, _PGID_ADMIN_LICENSE, _PGID_ADMIN_WORKGROUPS, _PGID_ADMIN_WORKGROUP_EDIT, _PGID_UNKNOWN, _PGID_DB_SETUP, _PGID_DB_DECISIONS, _PGID_LOGOUT, _PGID_ADMIN_ONLINE_USERS, _PGID_ADMIN_VIEWLOGS  ' D0283 + D0371 + D0391 -D0439 + D1056 + D3037
                    Case _PGID_ERROR_403, _PGID_ERROR_404, _PGID_ERROR_500, _PGID_ERROR_503, _PGID_START, _PGID_START_WITH_SIGNUP, _PGID_ADMIN_LICENSE, _PGID_ADMIN_WORKGROUPS, _PGID_ADMIN_WORKGROUP_EDIT, _PGID_UNKNOWN, _PGID_DB_SETUP, _PGID_DB_DECISIONS, _PGID_LOGOUT, _PGID_ADMIN_ONLINE_USERS, _PGID_ADMIN_VIEWLOGS  ' D0283 + D0371 + D0391 -D0439 + D1056 + D3037 + D4842
                    Case Else
                        Dim tWG As clsWorkgroup = tWorkgroup
                        If Not tWG Is Nothing Then
                            If Not App.CheckLicense(tWG, Nothing, True) Then Return False ' D0266
                        End If
                End Select
                ' D0264 ===
                'If App.Options.LicenseCheckInCore Then  ' D0315
                '    If Not HasPermissionByAction(tUser, tPageAction.ActionType, False, True) Then Return False
                'End If
                ' D0264 ==
            Else
                If tPageAction.ID = _PGID_ADMIN_LICENSE Then Return False
            End If
            ' D0262 ==

            ' D0100 ===
            Select Case tPageAction.ID

                'Case _PGID_PROJECT_DELETE, _PGID_PROJECT_DOWNLOAD, _PGID_SURVEY_DOWNLOAD, _PGID_SURVEY_LIST
                Case _PGID_PROJECT_DELETE, _PGID_PROJECT_DOWNLOAD, _PGID_SURVEY_DOWNLOAD    ' D2780
                    If fDisabledWorkgroup And Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, tUser.UserID) Then Return False ' D0499

                    ' disable any actions for create/modify decisions when workgroup is disabled
                'Case _PGID_PROJECT_COPY, _PGID_PROJECT_CREATE, _PGID_PROJECT_CREATE_FROM_TPL, _PGID_PROJECT_UPDATE, _PGID_PROJECT_LOCK, _PGID_PROJECT_UPLOAD, _PGID_PROJECT_PROPERTIES, _PGID_PROJECT_STATUS, _PGID_PROJECT_UNLOCK,
                Case _PGID_PROJECT_COPY, _PGID_PROJECT_CREATE, _PGID_PROJECT_CREATE_FROM_TPL, _PGID_PROJECT_UPDATE, _PGID_PROJECT_UPLOAD, _PGID_PROJECT_PROPERTIES,
                    _PGID_ADMIN_ONLINE_USERS, _PGID_ADMIN_ROLEGROUPS, _PGID_ADMIN_VIEWLOGS ' D0641 + D5092
                    If fDisabledWorkgroup Then Return False
                    ' D0348 ==
                Case _PGID_PROJECT_DELETE
                    If Not tProject Is Nothing And Not tUser Is Nothing Then
                        If Not App.CanUserDoProjectAction(ecActionType.at_mlDeleteModel, tUser.UserID, tProject.ID, tUserWorkgroup) Then Return False ' D0471
                    End If
            End Select
            ' D0100 ==

            ' -D4940
            'If tPageAction.Permission = ecPagePermission.ppNoAvailableProjects Then
            '    If Not App.Workspaces Is Nothing AndAlso App.Workspaces.Count > 0 Then Return False ' D0466
            '    If App.HasActiveProject Then Return False ' D0466
            'End If

            ' D0286 ===
            If tProject IsNot Nothing AndAlso Array.IndexOf(_PAGESLIST_ARCHIVED_ONLY, tPageAction.ID) < 0 Then
                If tProject.LockInfo Is Nothing Then tProject.LockInfo = App.DBProjectLockInfoRead(tProject.ID) ' D0613
                'If tProject.ProjectStatus <> ecProjectStatus.psActive Or fDisabledWorkgroup Or (tProject.LockInfo.LockStatus = ECLockStatus.lsLockForAntigua AndAlso tPageAction.Permission <> ecPagePermission.ppProjectWithStructuringLock) Or tProject.LockInfo.LockStatus = ECLockStatus.lsLockForSystem Then    ' D0300 + D0348 + D0589 -D0664
                If tProject.ProjectStatus = ecProjectStatus.psArchived OrElse fDisabledWorkgroup OrElse (tProject.LockInfo IsNot Nothing AndAlso tProject.LockInfo.LockStatus = ECLockStatus.lsLockForSystem) Then    ' D0300 + D0348 + D0589 + D0664 + D0760
                    'If tPageAction.Permission = ecPagePermission.ppProject OrElse tPageAction.Permission = ecPagePermission.ppProjectNotEmpty OrElse
                    '   tPageAction.Permission = ecPagePermission.ppProjectWithLock OrElse tPageAction.Permission = ecPagePermission.ppProjectNotEmptyWithLock OrElse tPageAction.Permission = ecPagePermission.ppProjectWithStructuringLock Then    ' D0589
                    If tPageAction.Permission = ecPagePermission.ppProject OrElse tPageAction.Permission = ecPagePermission.ppProjectWithLock Then    ' D0589 + D4940
                        Return False
                    End If
                End If
            End If
            ' D0286 ==

            'If tPageAction.Permission = ecPagePermission.ppProject Or tPageAction.Permission = ecPagePermission.ppProjectNotEmpty Or
            '   tPageAction.Permission = ecPagePermission.ppProjectWithLock Or tPageAction.Permission = ecPagePermission.ppProjectNotEmptyWithLock Then   ' D0097 + D0134
            If tPageAction.Permission = ecPagePermission.ppProject OrElse tPageAction.Permission = ecPagePermission.ppProjectWithLock Then   ' D0097 + D0134 + D4940
                If tProject Is Nothing Then
                    Return False
                End If
                If Not tProject.isValidDBVersion AndAlso Not tProject.isDBVersionCanBeUpdated Then
                    App.ProjectID = -1  ' D4840 //need to reset active project since can redirect in infinite loop 
                    Return False ' D4790 // for avoid crashing on read model with a new DB version
                End If

                ' -D4940
                '' D0097 ===
                'If tPageAction.Permission = ecPagePermission.ppProjectNotEmpty Or tPageAction.Permission = ecPagePermission.ppProjectNotEmptyWithLock Then    ' D0134
                '    'If tProject.IsProjectEmpty Or tProject.ProjectStatus = ecProjectStatus.psTemplate Then Return False ' D0206 + D0300 -D0654
                '    If tProject.IsProjectEmpty Then Return False ' D0206 + D0300 + D0654
                'End If
                '' D0097 ==

                ' D0206 ===
                If tProject.ProjectStatus = ecProjectStatus.psTemplate Then   ' D0300
                    Return HasListPage(_PAGESLIST_TEMPLATE, tPageAction.ID) ' D0580
                End If
                ' D0206 ==
                ' D2479 ===
                If tProject.ProjectStatus = ecProjectStatus.psMasterProject Then
                    If CurrentPageID = _PGID_EVALUATION AndAlso (Request.Params(_PARAM_INTENSITIES) IsNot Nothing OrElse SessVar(SESSION_SCALE_ID) IsNot Nothing) Then
                        ' AD: Force to allow pass 'anytime evaluation' page for templates due to have ability to access intensities (permissions will be checked later, on evaluation page)
                    Else
                        Return HasListPage(_PAGESLIST_MASTERPROJECTS, tPageAction.ID)
                    End If
                End If
                ' D0206 ==
                ' D0789 ===
                If tProject.isMarkedAsDeleted Then   ' D0300
                    ' D7159 ===
                    If App.CanUserModifyProject(tUser.UserID, tProject.ID, tUserWorkgroup, tWorkspace, tWorkgroup) Then
                        Return HasListPage(_PAGESLIST_DELETED, tPageAction.ID) ' D0580
                    Else
                        Return False
                    End If
                    ' D7159 ==
                Else
                    'If tPageAction.ID = _PGID_PROJECT_UNDELETE Then Return False   ' -D0592
                End If
                ' D0789 ==
            End If

            ' D2239 ===
            If tPageAction.ID = _PGID_PROJECT_DOWNLOAD AndAlso Not App.HasActiveProject Then
                Return App.isAuthorized AndAlso App.CanUserModifySomeProject(tUser.UserID, App.ActiveProjectsList, tUserWorkgroup, App.Workspaces)
            End If
            ' D2239 ==

            ' -D2857
            '' D2808 ===
            'Dim EvalPages As Integer() = {_PGID_EVALUATION, _PGID_EVALUATENOW, _PGID_EVALUATE_TEAMTIME, _PGID_EVALUATE_RISK_CONTROLS, _PGID_EVALUATE_READONLY}
            'If tProject IsNot Nothing AndAlso tWorkspace IsNot Nothing AndAlso HasListPage(EvalPages, tPageAction.ID) Then
            '    If tWorkspace.GroupID = tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtViewer) Then
            '        If tProject.isValidDBVersion Then
            '            Dim tAHPUser As clsUser = tProject.ProjectManager.GetUserByEMail(tUser.UserEmail)
            '            If tAHPUser IsNot Nothing AndAlso tProject.ProjectManager.ProjectAnalyzer.GetTotalJudgmentsCount(tAHPUser.UserID, , False) = 0 Then Return False
            '        End If
            '    End If
            'End If
            '' D2808 ==

            If tPageAction.ActionType = ecActionType.atUnspecified Then Return True

            ' D0052 ===
            Dim tCheckRoleGroup As clsRoleGroup = CType(IIf(tPageAction.RoleLevel <> ecRoleLevel.rlModelLevel, tUserRoleGroup, tProjectRoleGroup), clsRoleGroup) ' D0087
            If tCheckRoleGroup Is Nothing Then Return False
            ' D0087 ===
            Dim fPassed As Boolean = tCheckRoleGroup.ActionStatus(tPageAction.ActionType) = ecActionStatus.asGranted
            If Not fPassed AndAlso fCanManageAnyDecision AndAlso tCheckRoleGroup.RoleLevel = ecRoleLevel.rlModelLevel Then fPassed = True ' D0450
            ' D0052 ==
            If Not fPassed Then
                Dim DegreeActionType As ecActionType = ecActionType.atUnspecified
                Select Case tPageAction.ActionType
                    Case ecActionType.at_alManageWorkgroupRights
                        DegreeActionType = ecActionType.at_slManageAllUsers ' D0091
                    Case ecActionType.at_alManageWorkgroupUsers
                        DegreeActionType = ecActionType.at_slManageAllUsers ' D0727
                    Case ecActionType.at_slDeleteOwnWorkgroup
                        DegreeActionType = ecActionType.at_slDeleteAnyWorkgroup
                    Case ecActionType.at_slManageOwnWorkgroup
                        DegreeActionType = ecActionType.at_slManageAnyWorkgroup
                    Case ecActionType.at_slSetLicenseOwnWorkgroup
                        DegreeActionType = ecActionType.at_slSetLicenseAnyWorkgroup
                    Case ecActionType.at_slViewLicenseOwnWorkgroup
                        DegreeActionType = ecActionType.at_slViewLicenseAnyWorkgroup    ' D0091
                    Case ecActionType.at_slViewOwnWorkgroupReports
                        DegreeActionType = ecActionType.at_slViewAnyWorkgroupReports    ' D0091
                    Case ecActionType.at_slViewOwnWorkgroupLogs
                        DegreeActionType = ecActionType.at_slViewAnyWorkgroupLogs   ' D0091
                End Select
                If DegreeActionType <> ecActionType.atUnspecified Then
                    fPassed = tCheckRoleGroup.ActionStatus(DegreeActionType) = ecActionStatus.asGranted
                End If
            End If

            ' D3298 + D7270 ===
            'If Not fPassed AndAlso tWorkgroup IsNot Nothing AndAlso tWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then
            If tWorkgroup IsNot Nothing AndAlso App.isAuthorized Then
                Select Case tPageAction.ActionType
                        ' D7268 ===
                    Case ecActionType.at_slManageOwnWorkgroup, ecActionType.at_slSetLicenseOwnWorkgroup, ecActionType.at_slViewLicenseOwnWorkgroup,
                             ecActionType.at_slViewOwnWorkgroupLogs, ecActionType.at_slViewOwnWorkgroupReports
                        fPassed = (fPassed OrElse tPageAction.ID = _PGID_ADMIN_WORKGROUPS OrElse tWorkgroup.OwnerID = tUser.UserID OrElse (tPageAction.ActionType = ecActionType.at_alManageWorkgroupUsers AndAlso tWorkgroup.ECAMID = tUser.UserID)) AndAlso (App.CanUserDoSystemWorkgroupAction(tPageAction.ActionType, tUser.UserID) OrElse App.CanUserDoAction(tPageAction.ActionType, tUserWorkgroup)) ' D7273 + D7635
                        ' D7270 ==
                    Case ecActionType.at_slCreateWorkgroup, ecActionType.at_slDeleteAnyWorkgroup, ecActionType.at_slManageAllUsers,
                             ecActionType.at_slManageAnyWorkgroup, ecActionType.at_slSetLicenseAnyWorkgroup, ecActionType.at_slViewAnyWorkgroupLogs,
                             ecActionType.at_slViewAnyWorkgroupReports, ecActionType.at_slViewLicenseAnyWorkgroup
                        fPassed = App.CanUserDoSystemWorkgroupAction(tPageAction.ActionType, tUser.UserID)
                        ' D7268 ==
                End Select
            End If
            ' D3298 ==

            ' D0302 ===
            If fPassed Then
                Select Case tPageAction.ActionType
                    Case ecActionType.at_slViewAnyWorkgroupLogs, ecActionType.at_slViewAnyWorkgroupReports
                        If Not tWorkgroup Is Nothing AndAlso tWorkgroup.Status = ecWorkgroupStatus.wsSystem AndAlso App.CanUserDoSystemWorkgroupAction(tPageAction.ActionType, tUser.UserID) Then Return True ' D0305 + D0473 + D0517
                End Select
            End If
            ' D0302 ==

            If Not fPassed Then DebugInfo("Checks not passed!")
            Return fPassed
            ' D0087 ==
        End Function

        Public Overloads Function HasPermission(ByVal tPageID As Integer, ByVal tProject As clsProject, Optional tWorkgroup As clsWorkgroup = Nothing) As Boolean   ' D7270
            Return HasPermission(PageByID(tPageID), tProject, tWorkgroup)   ' D7270
        End Function

        'A0908 ===
        Private Sub InitUserLocaleInfo()
            ' D4507 ===
            Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.CreateSpecificCulture(UserLocale.Name)
            Threading.Thread.CurrentThread.CurrentCulture = ci
            Threading.Thread.CurrentThread.CurrentUICulture = ci
            ' D4507 ==

            'disabled this routine for now, until verifying with AD

            'Dim Request As HttpRequest = HttpContext.Current.Request

            'If Request.UserLanguages IsNot Nothing Then
            '    Dim Lang As String = Request.UserLanguages(0)

            '    If Not String.IsNullOrEmpty(Lang) Then
            '        If Lang.Length < 3 Then Lang = Lang + "-" + Lang.ToUpper()
            '        Try
            '            System.Threading.Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo(Lang)
            '        Catch
            '        End Try
            '    End If
            'End If

        End Sub

        Public Shared Function UserLang() As String
            Dim Request As HttpRequest = HttpContext.Current.Request
            If Request.UserLanguages IsNot Nothing Then
                Dim Lang As String = Request.UserLanguages(0)
                If Not String.IsNullOrEmpty(Lang) Then
                    If Lang.Length < 3 Then Lang = Lang + "-" + Lang.ToUpper()
                    Return Lang
                End If
            End If

            Return System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName
        End Function

        ' -D4507
        'Public Shared Function UserCulture() As Globalization.CultureInfo
        '    Return Globalization.CultureInfo.CreateSpecificCulture(UserLang)
        'End Function
        'A0908 ==

        ' D4526 ===
        Public Sub RAAddLogEvent(sMessage As String, sComment As String)
            App.DBSaveLog(dbActionType.actRASolveModel, dbObjectType.einfProject, App.ProjectID, sMessage, sComment)
        End Sub
        ' D4526 ==

#Region "Page Events"

        Private Sub Page_PreInitMain(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit    ' D0533
            initAntiCSRF()  ' D7641
            _isAdvancedMode = Str2Bool(GetCookie(_COOKIE_IS_ADVANCED, "0"))  ' D5083
            If App.HasActiveProject AndAlso App.ActiveUser IsNot Nothing AndAlso App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup) Then CanUserModifyActiveProject = True    ' D4916 + D4938 + D7621
            If CurrentPageID <> _PGID_ERROR_500 Then AddHandler Me.Error, AddressOf ErrorFeedback ' D4382
            ' D4827 ===
            If Request IsNot Nothing Then
                If Request.Url IsNot Nothing Then DebugInfo("Request: " + Request.Url.AbsoluteUri, _TRACE_INFO) ' D0785
                App.isMobileBrowser = isMobileBrowserClient(Request) ' D2949
                If WebOptions.ForceSSL(Request) AndAlso Not isSecureConnection(Request) AndAlso Request.Url IsNot Nothing AndAlso Request.Url.AbsoluteUri IsNot Nothing Then ' D4061
                    Response.Redirect(Request.Url.AbsoluteUri.Replace("http:", "https:"), True) ' D0035 + D0135
                End If
            End If
            ' D4827 ==

            ' D0117 ===
            If App.isAuthorized AndAlso Not IsPostBack AndAlso Not isCallback Then
                Dim tmpUser As clsApplicationUser = App.DBUserByID(App.ActiveUser.UserID)
                If tmpUser IsNot Nothing Then
                    If tmpUser.Status <> ecUserStatus.usEnabled Then
                        UserLogout("Terminate user session due to disabled account")
                        App.ApplicationError.Init(ecErrorStatus.errAccessDenied, CurrentPageID, ResString("msgUserIsLocked"), Nothing, Request.Url.AbsoluteUri) ' D7505
                        Response.Redirect(PageURL(_PGID_START), True)   ' D7505
                    Else
                        If App.ActiveWorkgroup IsNot Nothing Then
                            Dim tUWG As clsUserWorkgroup = App.DBUserWorkgroupByUserIDWorkgroupID(App.ActiveUser.UserID, App.ActiveWorkgroup.ID)
                            If tUWG Is Nothing OrElse (tUWG.isExpired OrElse tUWG.Status <> ecUserWorkgroupStatus.uwEnabled) Then
                                UserLogout(String.Format("Terminate user session due to disable account in the workgroup '{0}' workgroup", App.ActiveWorkgroup.Name))
                            Else
                                ' D7160 ===
                                If App.ActiveUserWorkgroup IsNot Nothing AndAlso App.ActiveUserWorkgroup.RoleGroupID <> tUWG.RoleGroupID Then
                                    Dim PrjID As Integer = App.ProjectID
                                    App.ActiveUserWorkgroup = tUWG
                                    App.ActiveProjectsList = Nothing
                                    App.ProjectID = PrjID
                                End If
                                ' D7160 ==
                                If App.HasActiveProject Then
                                    Dim tmpPrj As clsProject = App.DBProjectByID(App.ProjectID)
                                    If tmpPrj Is Nothing Then
                                        App.ProjectID = -1
                                        App.ActiveUserWorkgroup = tUWG
                                    End If
                                    If App.HasActiveProject AndAlso App.ActiveProjectsList IsNot Nothing Then
                                        'For i As Integer = 0 To App.ActiveProjectsList.Count - 1
                                        '    If App.ActiveProjectsList(i).ID = tmpPrj.ID Then
                                        '        Dim tmpPM As clsProjectManager = If(App.ActiveProjectsList(i).IsProjectLoaded, App.ActiveProjectsList(i).ProjectManager, Nothing)
                                        '        App.ActiveProjectsList(i) = tmpPrj
                                        '        If tmpPM IsNot Nothing Then App.ActiveProjectsList(i).ProjectManager = tmpPM
                                        '        Exit For
                                        '    End If
                                        'Next
                                    End If
                                    ' D7160 ===
                                    Dim tWs As clsWorkspace = App.DBWorkspaceByUserIDProjectID(App.ActiveUser.UserID, App.ProjectID)
                                    If tWs IsNot Nothing AndAlso App.ActiveWorkspace IsNot Nothing AndAlso tWs.GroupID <> App.ActiveWorkspace.GroupID Then
                                        App.Workspaces = Nothing
                                        App.ActiveProjectsList = Nothing
                                    End If
                                    If App.HasActiveProject AndAlso Not App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, tUWG, If(tWs Is Nothing, App.ActiveWorkspace, tWs), App.ActiveWorkgroup) Then
                                        If tWs Is Nothing OrElse tmpPrj.isMarkedAsDeleted OrElse (App.isRiskEnabled AndAlso tWs.StatusImpact = ecWorkspaceStatus.wsDisabled OrElse tWs.StatusLikelihood = ecWorkspaceStatus.wsDisabled) Then
                                            App.ProjectID = -1
                                        End If
                                    End If
                                    ' D7160 ==
                                End If
                            End If
                        End If
                    End If
                Else
                    UserLogout("Terminate user session due to unknown/deleted account")
                End If
                'If Not App.isAuthorized AndAlso Not isAJAX Then FetchAccess() ' not suitable for webAPI/JSON calls
            End If

            ' D0781 ===
            If CheckVar("resetproject", False) AndAlso (App.HasActiveProject OrElse AnonAntiguaProject() IsNot Nothing) AndAlso Not isCallback() Then  ' D2538 + Â3541
                If App.HasActiveProject Then
                    Dim HID As Integer = -1 ' D3541
                    If App.ActiveProject.IsProjectLoaded Then HID = App.ActiveProject.ProjectManager.ActiveHierarchy ' D3541
                    If Session(_SESS_TT_Pipe) IsNot Nothing Then Session.Remove(_SESS_TT_Pipe) ' D1302
                    If Session(_SESS_TT_Passcode) IsNot Nothing Then Session.Remove(_SESS_TT_Passcode) ' D6789
                    App.ActiveProject.ResetProject()    ' D2658
                    App.ActiveProjectsList = Nothing
                    App.Workspaces = Nothing
                    App.UserWorkgroups = Nothing
                    If HID >= 0 Then App.ActiveProject.ProjectManager.ActiveHierarchy = HID
                End If
                If AnonAntiguaProject() IsNot Nothing Then
                    Session(_SESS_PRJ_ANON_NAME) = Nothing
                End If
                Dim sURL As String = ""
                If Request IsNot Nothing AndAlso Request.Url IsNot Nothing Then
                    sURL = Request.Url.Query.ToLower.Replace("resetproject=" + CheckVar("resetproject", ""), "").Replace("&&", "&").TrimEnd(CChar("&")) ' D0652 + D3541 + D4827
                    If sURL = "?" Then sURL = ""
                    Response.Redirect(Request.Url.AbsolutePath + sURL, True)
                End If
            End If
            ' D0781 ==

            ' D4672 ===
            Dim sLang As String = CheckVar("language", "")
            If sLang <> "" AndAlso sLang.ToLower <> App.LanguageCode.ToLower Then   ' D6766
                If SetLanguage(sLang) Then
                    App.ResetLanguages()
                    Dim sURL As String = PageURL(CurrentPageID)
                    ' D6766 ===
                    'If Not Request.UrlReferrer Is Nothing Then sURL = Request.UrlReferrer.AbsoluteUri
                    'If sURL = "" Then sURL = _URL_ROOT
                    If Request.Url IsNot Nothing Then sURL = URLWithParams(Request.Url.AbsolutePath, GetParamsButExclude(Request.QueryString, {"language"}))
                    ' D6766 ==
                    Response.Redirect(sURL, True)
                End If
            End If
            ' D4672 ==

            Dim sNewTheme As String = CheckVar(_PARAM_THEME, "").Trim  ' D0652
            If sNewTheme <> "" Then
                Dim sMaster As String = MasterByTheme(sNewTheme).ToLower
                'If sMaster <> "" AndAlso (CurrentPageID = _PGID_SILVERLIGHT_UI Or DefaultMasterPage.ToLower <> sMaster) Then
                If sMaster <> "" AndAlso DefaultMasterPage.ToLower <> sMaster AndAlso Request IsNot Nothing AndAlso Request.Url IsNot Nothing Then  ' D4827
                    DefaultMasterPage = sMaster
                    Dim sRefreshURL As String = Request.Url.Query.ToLower.Replace(_PARAM_THEME + "=" + sNewTheme, "").Replace("&&", "&").TrimEnd(CChar("&")) ' D0652
                    If sRefreshURL = "?" Then sRefreshURL = ""
                    sRefreshURL = Request.Url.AbsolutePath + sRefreshURL
                    Response.Redirect(sRefreshURL, True)
                End If
            End If

            If Page.EnableTheming Then  ' D0352
                Select Case DefaultMasterPage.ToLower
                    'Case _MASTER_DEFAULT.ToLower        ' D0567 + D0652 -D4961
                    '    Page.Theme = _THEME_DEFAULT     ' D0781
                    'Case _MASTER_EC09.ToLower, _MASTER_EC09_TT.ToLower, _MASTER_BLANK.ToLower, _MASTER_POPUP.ToLower    ' D4668
                    Case _MASTER_EC09_TT.ToLower, _MASTER_POPUP.ToLower    ' D4668
                        Page.Theme = _THEME_EC09
                    Case Else
                        Page.Theme = _THEME_EC2018      ' D4668
                End Select
                ' D0560 ==

                ' D0652 ===
                Dim fNeedCheckMaster As Boolean = True
                Dim sTempTheme As String = GetTempTheme()
                Select Case CurrentPageID
                    Case _PGID_ERROR_403, _PGID_ERROR_404, _PGID_ERROR_500, _PGID_ERROR_503
                        If sTempTheme.ToLower = _THEME_SL OrElse isCallback OrElse IsPostBack OrElse isAJAX OrElse IsAsync OrElse IsCrossPagePostBack OrElse CheckVar("callback", False) Then   ' D4750
                            Page.MasterPageFile = _URL_ROOT + _MASTER_EMPTY
                            Page.Theme = _THEME_EC2018
                            sTempTheme = ""
                            StorePageID = False ' D0679
                        End If
                End Select
                If Page.MasterPageFile Is Nothing Then Page.MasterPageFile = _URL_ROOT + _MASTER_DEFAULT ' D4961
                If Page.MasterPageFile IsNot Nothing AndAlso Page.MasterPageFile.ToLower.EndsWith(_MASTER_DEFAULT.ToLower) AndAlso sTempTheme = "" Then fNeedCheckMaster = False ' D1234 + D1245 + D4734 + D4958
                If fNeedCheckMaster AndAlso sTempTheme <> "" Then   ' D4958
                    ' D0653 ===
                    'Page.MasterPageFile = _URL_ROOT + DefaultMasterPage    ' -D4958
                    ' D0766 ===
                    If sTempTheme <> "" Then  ' D0729
                        Select Case sTempTheme.Trim.ToLower
                            Case _THEME_SL
                                'Page.MasterPageFile = _URL_ROOT + _MASTER_BLANK
                                Page.MasterPageFile = _URL_ROOT + _MASTER_EMPTY ' D4918
                            Case _THEME_TT
                                If App.HasActiveProject AndAlso App.ActiveProject.isTeamTime Then
                                    ' D6779 ===
                                    Select Case CurrentPageID
                                        Case _PGID_ERROR_500, _PGID_ERROR_404, _PGID_ERROR_403
                                            ShowNavigation = False
                                            ShowTopNavigation = False
                                        Case Else
                                            Page.MasterPageFile = _URL_ROOT + _MASTER_EC09_TT ' D0778
                                    End Select
                                    ' D6779 ==
                                End If
                        End Select
                        ' D0766 ==
                        'Page.Theme = _THEME_EC09    ' D0778
                        Page.Theme = _THEME_EC2018  ' D0778 + D4918
                        StorePageID = False ' D0679
                    End If
                    ' D0652 ==
                    ' D0653 ==
                End If
                'If Page.MasterPageFile.Contains(_MASTER_BLANK) OrElse Page.MasterPageFile.Contains(_MASTER_EC09) OrElse Page.MasterPageFile.Contains(_MASTER_EC09_TT) Then
                If Page.MasterPageFile IsNot Nothing AndAlso Page.MasterPageFile.ToLower.Contains(_MASTER_EC09_TT.ToLower) Then   ' D4961
                    If Page.Theme <> _THEME_EC09 Then Page.Theme = _THEME_EC09
                End If
            End If

            If Page.MasterPageFile IsNot Nothing AndAlso Page.MasterPageFile.ToLower.Contains(_MASTER_WEBAPI.ToLower) Then
                If Request IsNot Nothing AndAlso Request.QueryString.ToString = "help" Then
                    RawResponseStart()
                    Dim sMethods As String = GetMethodsList(Me.GetType().BaseType())
                    If String.IsNullOrEmpty(sMethods) Then
                        Response.StatusCode = CInt(System.Net.HttpStatusCode.NotImplemented)
                        Response.StatusDescription = "NO_METHODS_FOUND"
                    Else
                        Response.ContentType = "text/plain"
                        Response.Write(sMethods)
                    End If
                    RawResponseEnd()
                End If
            End If

            DebugInfo(String.Format("Master page is {0}", Page.MasterPageFile))
            App.CheckIsEvalSiteOnly()   ' D6359

            ' D7271 ===
            If App.isAuthorized AndAlso App.ActiveWorkgroup IsNot Nothing Then   ' D6359 // since we need to check permission when no active project as well
                _isJustEvaluator = App.ActiveRoleGroup IsNot Nothing AndAlso App.ActiveRoleGroup.GroupType = ecRoleGroupType.gtUser
                If App.HasActiveProject AndAlso App.ActiveProjectRoleGroup IsNot Nothing Then _isJustEvaluator = App.ActiveProjectRoleGroup.GroupType = ecRoleGroupType.gtEvaluator  ' D7451
            End If
            ' D7271 ==

            ' -D4538
            '' D3205 ===
            'If Not IsPostBack AndAlso Not isCallback() AndAlso App.Options.LastVisitedPage <> tPrevPage AndAlso StorePageID AndAlso WebConfigOption("NewRelic.agentEnabled", "false").ToLower = "true" Then
            '    Dim sCode As String = App.RecordCustomEvent(dbActionType.actOpen, dbObjectType.einfPage, CurrentPageID, "Open page", Request.Url.AbsoluteUri)
            '    If sCode <> "" Then
            '        sCode = String.Format("<script type='text/javascript' src='{1}js/newrelic.js'></script>" + vbCrLf + _
            '                              "<script type='text/javascript'> var newrelic_data=[{0}]; if(typeof(RecordNewRelic)=='function') RecordNewRelic(newrelic_data); </script>", sCode, _URL_ROOT) ' D3208
            '        ClientScript.RegisterStartupScript(GetType(String), "AddNewRelic", sCode, False)    ' D3208
            '    End If
            'End If
            '' D3205 ==

            ' Remove since it hooks the user session before he can be really logged via hash/token
            '' D4459 ===
            'Dim tPg As clsPageAction = PageByID(CurrentPageID)
            'If tPg IsNot Nothing Then
            '    Select Case tPg.Permission
            '        'Case ecPagePermission.ppProject, ecPagePermission.ppProjectNotEmpty, ecPagePermission.ppProjectNotEmptyWithLock,
            '        '     ecPagePermission.ppProjectWithLock, ecPagePermission.ppProjectWithStructuringLock
            '        Case ecPagePermission.ppProject, ecPagePermission.ppProjectWithLock ' D4940
            '            If Not App.HasActiveProject OrElse Not App.isAuthorized Then
            '                If isSLTheme() Then
            '                    Dim sURL As String = PageURL(_PGID_SERVICEPAGE, String.Format("action=msg&type=err&pg=", _PGID_ERROR_403))
            '                    Response.Redirect(sURL, True)
            '                Else
            '                    FetchAccess(_PGID_PROJECTSLIST)
            '                End If
            '            End If
            '        Case ecPagePermission.ppAuthorized
            '            If Not App.isAuthorized Then
            '                If isSLTheme() Then
            '                    Dim sURL As String = PageURL(_PGID_SERVICEPAGE, String.Format("action=msg&type=err&pg=", _PGID_ERROR_403))
            '                    Response.Redirect(sURL, True)
            '                Else
            '                    FetchAccess(_PGID_START)
            '                End If
            '            End If

            '    End Select
            'End If
            '' D4459 ==

            ' D4946 + D7352 ===
            Response.Headers.Remove("Server")
            Response.Headers.Remove("X-AspNet-Version")
            Response.Headers.Remove("X-Powered-By")
            ' D7352 ==
            Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate")
            Response.AppendHeader("Pragma", "no-cache")
            ' D496 ==
            Response.Cache.SetLastModified(Now())
            Response.Cache.SetExpires(Now())
            Response.Cache.SetMaxAge(TimeSpan.Zero)
            Response.Cache.SetNoStore()
            Response.Cache.SetNoServerCaching()
            Response.Cache.SetCacheability(HttpCacheability.NoCache)
            ' D7149 ===
            If isSecureConnection(Request) Then Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains")
            'Response.Headers.Add("X-XSS-Protection", "1; mode=block")  // added via web.config
            'Response.Headers.Add("Content-Security-Policy", "") -- can break some embedded content

            ' D7149 ==

        End Sub

        Private Sub Page_InitMain(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init    ' D4289
            'ShowTopNavigation = ShowNavigation ' D4820

            ' D5057 ===
            Dim CurPg As Integer = CheckVar(_PARAM_PAGE, -1)
            If CurPg > 0 AndAlso CurPg <> CurrentPageID Then
                Dim Pg As clsPageAction = PageByID(CurPg)
                If Pg IsNot Nothing Then
                    Dim navURL As String = PageURL(CurPg).Split(CChar("?"))(0).ToLower
                    If Request.Url.AbsolutePath.ToLower.Contains(navURL) Then
                        CurrentPageID = CurPg
                    End If
                End If
            End If

            Dim NavPg As Integer = CheckVar(_PARAM_NAV_PAGE, -1)
            If NavPg > 0 AndAlso NavPg <> CurrentPageID Then
                Dim realPg As Integer = NavPg Mod _PGID_MAX_MOD
                Dim Pg As clsPageAction = PageByID(realPg)
                If Pg IsNot Nothing Then
                    Dim navURL As String = PageURL(realPg).Split(CChar("?"))(0).ToLower
                    If Request.Url.AbsolutePath.ToLower.Contains(navURL) Then
                        NavigationPageID = NavPg
                    End If
                End If
            End If
            ' D5057 ==

            ' D6081 ===
            Dim fIgnorePage As Boolean = String.IsNullOrEmpty(Page.MasterPageFile) OrElse Page.MasterPageFile.ToLower.Contains("mpblank") OrElse Page.MasterPageFile.ToLower.Contains("mppopup") OrElse GetCookie(_COOKIE_RET_PATH, "") <> "" OrElse CurrentPageID = _PGID_EVALUATE_INFODOC ' D6093
            If CurrentPageID <> _PGID_FEDRAMP_NOTIFICATION AndAlso Not isCallback AndAlso Not IsPostBack AndAlso Not fIgnorePage AndAlso App.ApplicationError.Status = ecErrorStatus.errNone AndAlso (CurrentPageID = _PGID_START OrElse Not HasListPage(_PAGESLIST_SKIP_LASTVISITED, CurrentPageID)) Then   ' D6093 + D6094
                If Not isNotificationAccepted() Then
                    Session.Add("ReturnURL", Request.RawUrl)
                    Response.Redirect(PageURL(_PGID_FEDRAMP_NOTIFICATION), True)    ' Server.Transfer
                End If
            End If
            ' D6081 ==

            Dim tPrjID As Integer = CheckVar(_PARAM_PROJECT, App.ProjectID) ' D0479 + D0481
            ' D1429 ===
            If tPrjID > 0 AndAlso App.ProjectID <> tPrjID Then
                Dim tmpPrj As clsProject = clsProject.ProjectByID(tPrjID, App.ActiveProjectsList)
                Dim fCanOpen As Boolean = False
                ' D7159 ===
                If tmpPrj IsNot Nothing AndAlso App.isAuthorized Then
                    tmpPrj = App.DBProjectByID(tPrjID)
                    If tmpPrj IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing Then
                        Dim tWS As clsWorkspace = App.DBWorkspaceByUserIDProjectID(App.ActiveUser.UserID, tPrjID)
                        Dim tUW As clsUserWorkgroup = App.DBUserWorkgroupByUserIDWorkgroupID(App.ActiveUser.UserID, App.ActiveWorkgroup.ID)
                        If tUW IsNot Nothing AndAlso tUW.Status <> ecUserWorkgroupStatus.uwDisabled Then
                            Dim fCanModify As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, tPrjID, tUW, tWS, App.ActiveWorkgroup)
                            Dim fCanEvaluate As Boolean = Not tmpPrj.isMarkedAsDeleted AndAlso tmpPrj.ProjectStatus = ecProjectStatus.psActive AndAlso (tmpPrj.isValidDBVersion OrElse tmpPrj.isDBVersionCanBeUpdated) AndAlso App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, App.ActiveUser.UserID, tmpPrj.ID, tUW, App.ActiveWorkgroup) AndAlso ((tWS IsNot Nothing AndAlso tWS.StatusLikelihood <> ecWorkspaceStatus.wsDisabled) OrElse fCanModify) ' D7427
                            Dim fCanView As Boolean = False
                            If Not tmpPrj.isMarkedAsDeleted AndAlso (tmpPrj.isValidDBVersion OrElse tmpPrj.isDBVersionCanBeUpdated) Then
                                fCanView = App.CanUserDoProjectAction(ecActionType.at_mlViewModel, App.ActiveUser.UserID, tmpPrj.ID, tUW, App.ActiveWorkgroup)
                                If Not fCanView AndAlso tmpPrj.ProjectStatus = ecProjectStatus.psTemplate AndAlso App.CanUserDoAction(ecActionType.at_alCreateNewModel, tUW, App.ActiveWorkgroup) Then fCanView = True
                            End If
                            If tmpPrj.LockInfo IsNot Nothing Then
                                If tmpPrj.LockInfo.LockStatus = ECLockStatus.lsLockForSystem Then
                                    fCanModify = False
                                    fCanEvaluate = False
                                    fCanView = False
                                End If
                            End If
                            fCanOpen = fCanModify OrElse fCanView OrElse fCanEvaluate
                        End If
                    End If
                End If
                If fCanOpen Then
                    Dim OldPrjID As Integer = App.ProjectID
                    App.ProjectID = tPrjID ' D0479 + D0481
                    If Not App.HasActiveProject OrElse (CurrentPageID <> _PGID_PROJECT_UPDATE AndAlso Not isProjectDBValidOrUpdated(App.ActiveProject, CurrentPageID)) Then App.ProjectID = OldPrjID
                    ' D1429 == 
                End If
                ' D7159 ==
            End If

            ' D4740 + D6042 ===
            If App.HasActiveProject AndAlso CurrentPageID <> _PGID_PROJECT_UPDATE AndAlso CurrentPageID <> _PGID_PING AndAlso CurrentPageID <> _PGID_WEBAPI AndAlso CurrentPageID <> _PGID_PROJECT_DOWNLOAD AndAlso CurrentPageID <> _PGID_RA_DOWNLOAD AndAlso CurrentPageID <> _PGID_SURVEY_DOWNLOAD AndAlso Not App.ActiveProject.isValidDBVersion Then
                Dim tPg As clsPageAction = PageByID(CurrentPageID)
                If tPg.RoleLevel = ecRoleLevel.rlModelLevel Then
                    'Select Case tPg.Permission
                    '    'Case ecPagePermission.ppProject, ecPagePermission.ppProjectNotEmpty, ecPagePermission.ppProjectNotEmptyWithLock, ecPagePermission.ppProjectWithLock, ecPagePermission.ppProjectWithStructuringLock
                    '    Case ecPagePermission.ppProject, ecPagePermission.ppProjectWithLock ' D4940
                    If Not isProjectDBValidOrUpdated(App.ActiveProject, CurrentPageID) Then
                        App.ApplicationError.Init(ecErrorStatus.errMessage, CurrentPageID, ResString("msgCantReadProjectDBVersion"), App.ActiveProject)
                        FetchAccess(_PGID_PROJECTSLIST)
                    End If
                    'End Select
                    ' D6042 ==
                End If
            End If
            ' D4740 ==

            ' D0117 ==
            If CurrentPageID <> _PGID_UNKNOWN AndAlso CurrentPageID <> _PGID_ADMIN_SERVICEHASH AndAlso CurrentPageID <> _PGID_WEBAPI Then   ' D1630 + D6030

                ' D0214 ===
                If Request.QueryString IsNot Nothing AndAlso Not isCallback AndAlso Not IsPostBack AndAlso Not isAJAX Then  ' D0226 + D4953
                    If Request.QueryString.Count > 0 AndAlso CurrentPageID <> _PGID_ERROR_500 AndAlso CurrentPageID <> _PGID_ERROR_404 AndAlso CurrentPageID <> _PGID_ERROR_403 AndAlso CurrentPageID <> _PGID_ERROR_503 AndAlso CurrentPageID <> _PGID_START_WITH_SIGNUP AndAlso CurrentPageID <> _PGID_CREATE_PASSWORD Then   ' D0257 + D1056 + D2216
                        Dim sError As String = ""
                        Dim sURL As String = ""
                        Dim AuthResult As ecAuthenticateError = Authenticate(Request.QueryString, sError, sURL, ecAuthenticateWay.awTokenizedURL, Not isSSO())   ' D0511 + D6532
                        If AuthResult <> ecAuthenticateError.aeUnknown Then
                            Dim Pg As clsPageAction = PageByID(CurrentPageID)
                            If AuthResult = ecAuthenticateError.aeNoErrors AndAlso sURL <> "" AndAlso Pg.Permission <> ecPagePermission.ppEveryone AndAlso Pg.Permission <> ecPagePermission.ppUnAuthorized AndAlso Pg.Permission <> ecPagePermission.ppUnspecified AndAlso App.Options.isLoggedInWithPasscode AndAlso App.Options.isSingleModeEvaluation Then sURL = "" ' D2289
                            'If AuthResult = ecAuthenticateError.aeNoErrors And sURL <> "" And Pg.Permission <> ecPagePermission.ppEveryone And Pg.Permission <> ecPagePermission.ppUnAuthorized And Pg.Permission <> ecPagePermission.ppUnspecified Then sURL = ""
                            If sURL <> "" Then
                                Response.Redirect(sURL, True)
                            End If
                            If AuthResult <> ecAuthenticateError.aeNoErrors Then
                                App.ApplicationError.Status = ecErrorStatus.errAccessDenied
                                App.ApplicationError.Message = sError
                                App.ApplicationError.CustomData = AuthResult.ToString   ' D3307
                                If CurrentPageID <> _PGID_ERROR_403 Then
                                    Response.Redirect(PageURL(_PGID_ERROR_403, GetTempThemeURI(False)), True) ' D0763 + D0766
                                End If
                            Else
                                ' D0226 ===
                                Dim sNewParams As String = ""
                                If Not Request.QueryString Is Nothing Then  ' D0228
                                    For Each tParam As String In Request.QueryString    ' D0228
                                        If tParam IsNot Nothing Then    ' D1734
                                            Dim sParam As String = RemoveXssFromParameter(tParam).Trim.ToLower  ' D6767
                                            If Array.IndexOf(_PARAMS_EMAIL, sParam) < 0 AndAlso
                                               Array.IndexOf(_PARAMS_KEY, sParam) < 0 AndAlso
                                               Array.IndexOf(_PARAMS_TINYURL, sParam) < 0 AndAlso
                                               (Array.IndexOf(_PARAMS_PASSCODE, sParam) < 0 OrElse CurrentPageID = _PGID_START) AndAlso
                                               Array.IndexOf(_PARAMS_PASSWORD, sParam) < 0 AndAlso
                                               Array.IndexOf(_PARAMS_REMEMBERME, sParam) < 0 AndAlso
                                               Array.IndexOf(_PARAMS_SIGNUP, sParam) < 0 AndAlso
                                               Array.IndexOf(_PARAMS_ANONYMOUS_SIGNUP, sParam) < 0 AndAlso
                                               Array.IndexOf(_PARAMS_SIGNUP_MODE, sParam) < 0 _
                                               Then sNewParams += CStr(IIf(sNewParams = "", "?", "&")) + String.Format("{0}={1}", tParam, RemoveXssFromParameter(Request.Params(tParam))) ' D0896 + D1056 + D1057 + D1763 + D6767
                                        End If
                                    Next
                                End If
                                Response.Redirect(PageURL(CurrentPageID, sNewParams), True)
                                ' D0226 ==
                            End If
                        End If
                    End If
                End If
                ' D0214 ==

                ' D7356 ===
                If App.ActiveUser IsNot Nothing AndAlso Not isCallback AndAlso Not IsPostBack Then  ' D7357
                    Dim tSess As DateTime? = App.CheckSessionTerminate(App.ActiveUser)
                    If tSess.HasValue Then
                        Dim sDT As String = ""
                        If Session(_SESS_CMD_TERMINATE) IsNot Nothing Then sDT = CStr(Session(_SESS_CMD_TERMINATE))
                        If sDT <> Service.Date2ULong(tSess.Value).ToString Then
                            App.Logout()
                            App.ApplicationError.Init(ecErrorStatus.errMessage, -1, ResString("msgPswHasBeenChanged"), True)
                            Response.Redirect(PageURL(_PGID_START), True)
                        End If
                    End If
                End If
                ' D7356 ==

                CheckPermissions() ' D0043
                ' D0429 ===
                ' check user workgroup expiration when authorized
                'If App.isAuthorized AndAlso CurrentPageID <> _PGID_START AndAlso CurrentPageID <> _PGID_START_WITH_SIGNUP AndAlso CurrentPageID <> _PGID_ERROR_403 AndAlso CurrentPageID <> _PGID_WORKGROUP_SELECT AndAlso Not isCallback() Then    ' D1056 + D3288
                If App.isAuthorized AndAlso CurrentPageID <> _PGID_START AndAlso CurrentPageID <> _PGID_START_WITH_SIGNUP AndAlso CurrentPageID <> _PGID_ERROR_403 AndAlso Not isCallback() Then    ' D1056 + D3288 + D4842
                    If Not App.ActiveUserWorkgroup Is Nothing Then
                        If App.ActiveUserWorkgroup.ExpirationDate.HasValue AndAlso Not App.ActiveUser.CannotBeDeleted Then
                            If App.ActiveUserWorkgroup.ExpirationDate.Value.Date < Date.Today Then
                                DebugInfo("User workgroup is expired. Log out...", _TRACE_WARNING)
                                ' -D4842
                                'If App.UserWorkgroups.Count > 1 Then
                                '    Response.Redirect(PageURL(_PGID_WORKGROUP_SELECT), True)
                                'Else
                                App.Logout()
                                App.ApplicationError.Init(ecErrorStatus.errAccessDenied, CurrentPageID, ParseAllTemplates(App.GetMessageByAuthErrorCode(ecAuthenticateError.aeUserWorkgroupExpired), App.ActiveUser, Nothing))   ' D3993
                                FetchAccess(_PGID_START)
                                'End If
                            End If
                        End If
                    End If
                End If
                ' D0429 ==

                If App.HasActiveProject AndAlso App.ActiveProject Is Nothing Then App.ProjectID = -1 ' D0513

                If App.HasActiveProject() AndAlso App.isAuthorized Then
                    ' D0144 ===
                    Dim Pg As clsPageAction = PageByID(CurrentPageID)
                    Dim fCanModify As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace)  ' D0471 + D0835
                    ' D0144 ==
                    ' D0135 + D0473 ===
                    App.ActiveProject.LockInfo = App.DBProjectLockInfoRead(App.ActiveProject.ID)    ' D0589
                    Dim tPrjLock As clsProjectLockInfo = App.ActiveProject.LockInfo ' D0483

                    ' D0754 ===
                    Dim fReloadProject As Boolean = CheckVar("resetproject", False)
                    If Not fReloadProject AndAlso tPrjLock IsNot Nothing AndAlso tPrjLock.LockStatus = ECLockStatus.lsLockForTeamTime AndAlso Not App.ActiveProject.isTeamTime Then fReloadProject = True
                    If fReloadProject Then App.ActiveProjectsList = Nothing ' D0754
                    ' D0754 ==

                    'Dim isLocked As ECLockStatus = tPrjLock.LockStatus  ' D0474 + D0589
                    'Dim isLockedByMe As Boolean = tPrjLock.isLockedByUser(App.ActiveUser)   ' D0494 + D0589
                    _CanEditActiveProject = True    ' D0611
                    If tPrjLock IsNot Nothing Then _CanEditActiveProject = (tPrjLock.isLockAvailable(App.ActiveUser) AndAlso (App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsUnLocked OrElse App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsLockForModify OrElse (App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsLockForTeamTime AndAlso App.ActiveProject.LockInfo.LockerUserID = App.ActiveUser.UserID) OrElse (App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsLockForAntigua AndAlso App.ActiveProject.LockInfo.LockerUserID = App.ActiveUser.UserID))) AndAlso App.ActiveProject.ProjectStatus = ecProjectStatus.psActive AndAlso (Not App.ActiveProject.isTeamTime Or App.ActiveProject.MeetingOwnerID = App.ActiveUser.UserID) AndAlso Not App.ActiveProject.isMarkedAsDeleted ' D0194 + D0206 + D0300 + D0301 + D0589 + D0611 + D0789 + D1289
                    '' modify since we allow to edit project when TT is active
                    'If tPrjLock IsNot Nothing Then _CanEditActiveProject = (tPrjLock.isLockAvailable(App.ActiveUser) OrElse (App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsUnLocked OrElse App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsLockForTeamTime OrElse (App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsLockForModify AndAlso App.ActiveProject.LockInfo.LockerUserID = App.ActiveUser.UserID))) AndAlso App.ActiveProject.ProjectStatus = ecProjectStatus.psActive AndAlso Not App.ActiveProject.isMarkedAsDeleted ' D0194 + D0206 + D0300 + D0301 + D0589 + D0611 + D0789 + D1289 + D4310

                    ' D0135 ==
                    If Not Pg Is Nothing And Not App.ActiveUser Is Nothing And _CanEditActiveProject Then   ' D0137
                        'If Pg.Permission = ecPagePermission.ppProjectWithStructuringLock Or Pg.Permission = ecPagePermission.ppProjectNotEmptyWithLock Or Pg.Permission = ecPagePermission.ppProjectWithLock Then ' D0589
                        If Pg.Permission = ecPagePermission.ppProjectWithLock Then ' D0589 + D4940
                            If fCanModify AndAlso (tPrjLock.isLockAvailable(App.ActiveUser) AndAlso (App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsUnLocked OrElse App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsLockForModify)) Then
                                App.DBProjectLockInfoWrite(ECLockStatus.lsLockForModify, App.ActiveProject.LockInfo, App.ActiveUser, Nothing)  ' D0471 + D0483 + D0589
                            End If
                        End If
                    End If
                    If _CanEditActiveProject And Not App.ActiveProject.isValidDBVersion Then _CanEditActiveProject = False ' D0144
                    ' D0348 ===
                    If Not App.ActiveWorkgroup Is Nothing Then
                        If App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsDisabled Then _CanEditActiveProject = False
                    End If
                    ' D0348 ==

                    ' D1672 + D1724 ===
                    Dim tmpPasscode As String = CheckVar(_PARAM_PASSCODE, "").Trim.ToLower
                    If tmpPasscode <> "" Then
                        If tmpPasscode = App.ActiveProject.PasscodeLikelihood.ToLower Then
                            App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
                            App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = App.ActiveProject.ProjectManager.PipeParameters.DefaultParameterSet   ' D4179
                        End If
                        If App.ActiveProject.IsRisk AndAlso tmpPasscode = App.ActiveProject.PasscodeImpact.ToLower Then
                            App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact ' D2898
                            App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = App.ActiveProject.ProjectManager.PipeParameters.ImpactParameterSet    ' D4179
                        End If
                    End If
                    ' D1672 + D1724 ==

                    If App.isAuthorized AndAlso App.HasActiveProject AndAlso App.ActiveProject.isValidDBVersion AndAlso App.ActiveProject.ProjectManager.User Is Nothing Then App.CheckProjectManagerUsers(App.ActiveProject, False)    ' D7000

                    ' D0591 ===
                    Dim tPg As clsPageAction = PageByID(CurrentPageID)
                    If tPg IsNot Nothing Then
                        Select Case tPg.Permission
                            'Case ecPagePermission.ppProject, ecPagePermission.ppProjectNotEmpty, ecPagePermission.ppProjectNotEmptyWithLock,
                            '     ecPagePermission.ppProjectWithLock, ecPagePermission.ppProjectWithStructuringLock
                            Case ecPagePermission.ppProject, ecPagePermission.ppProjectWithLock ' D4940
                                ' D0591 ==
                                ' D0589 ===
                                If App.ActiveProject.LockInfo IsNot Nothing Then    ' D0760
                                    Select Case App.ActiveProject.LockInfo.LockStatus
                                        Case ECLockStatus.lsLockForAntigua, ECLockStatus.lsLockForSystem
                                            DebugInfo("Reset project manager (Antigua session detected)")
                                            App.ActiveProject.ResetProject()
                                        Case Else
                                            ' D0591 ===
                                            If App.HasActiveProject And (Not IsPostBack AndAlso Not isCallback()) Then    ' D0785
                                                If App.ActiveProject.IsProjectLoaded AndAlso App.ActiveProject.ProjectManager.LastModifyTime.HasValue Then  ' D2036
                                                    Dim LastDT As DateTime = App.DBProjectStructureLastModifyTime(App.ProjectID)
                                                    If LastDT.AddSeconds(-1) > App.ActiveProject.ProjectManager.LastModifyTime Then ' D2036
                                                        App.DBUpdateDateTime(clsComparionCore._TABLE_PROJECTS, clsComparionCore._FLD_PROJECTS_LASTMODIFY, App.ProjectID)
                                                        App.ActiveProject.ResetProject(True)
                                                        Session(_SESS_TT_Pipe) = Nothing        ' D1284
                                                        Session(_SESS_TT_Passcode) = Nothing    ' D6789
                                                        DebugInfo("Project manager data is outdated. Reloaded.")
                                                    End If
                                                End If
                                            End If
                                            ' D0591 ==
                                            'If Not _CanEditActiveProject Then App.ActiveProject.ResetProject(True) - D0591
                                    End Select
                                End If
                                ' D0589 ==
                        End Select
                    End If

                    ' D0811 ===
                    If _CanEditActiveProject Then
                        If Not App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) OrElse Not App.SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) OrElse Not App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.InstanceID) OrElse Not App.SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.InstanceID) Then _CanEditActiveProject = False ' D0913 + D3947
                    End If
                    ' D0811 ==

                End If
            End If

            If App.isAuthorized AndAlso App.HasActiveProject AndAlso App.ActiveProject.isValidDBVersion Then App.CheckProjectManagerUserAsActive(App.ActiveProject)  ' D7001
        End Sub

        ' -D4289 // check another method for that event
        'Private Sub Page_Init(sender As Object, e As System.EventArgs) Handles Me.Init
        '    App.Options.LastVisitedPage = CurrentPageID     ' D3183
        '    App.Options.LastVisitedPageName = PageMenuItem(CurrentPageID)   ' D3186
        '    App.Options.LastVisitedPageGroup = GetPageGroup(CurrentPageID)  ' D3183
        '    'Commented because Anti-XSRF is already implemented in beta and this was throwing error there
        '    'Page.ViewStateUserKey = "ECC" + App.Options.SessionID   ' D4048
        'End Sub

        Private Sub Page_LoadComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LoadComplete
            'If isSLTheme(True) Then StorePageID = False    ' -D4536

            Dim PGAction As clsPageAction = PageAction(CurrentPageID)
            Dim fDoSaveInfo As Boolean = App.isAuthorized AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso CurrentPageID < 90000 AndAlso Not HasListPage(_PAGESLIST_SKIP_LASTVISITED, CurrentPageID)  ' D6495
            If Not IsCrossPagePostBack AndAlso Not isCallback() AndAlso Not IsPostBack AndAlso Not isAJAX AndAlso Not IsPostBack AndAlso StorePageID AndAlso EnableTheming Then ' D5063
                If fDoSaveInfo AndAlso App.HasActiveProject Then  ' D4544 + D4758 + D5063 + D6495
                    If Not PGAction Is Nothing Then
                        ' D7464 ===
                        Dim saveLastPrj As Boolean = False
                        Select Case PGAction.Permission
                            Case ecPagePermission.ppProject, ecPagePermission.ppProjectWithLock ' D4940
                                saveLastPrj = True
                            Case Else
                                If CurrentPageID >= _PGID_PROJECT_DESCRIPTION AndAlso CurrentPageID < 80000 Then saveLastPrj = True
                        End Select
                        If saveLastPrj Then App.DBUpdateDateTime(clsComparionCore._TABLE_PROJECTS, clsComparionCore._FLD_PROJECTS_LASTVISITED, App.ProjectID, clsComparionCore._FLD_PROJECTS_ID)
                        ' D7464 ==
                    End If
                End If
                'If DefaultMasterPage.ToLower <> _MASTER_BLANK.ToLower Then StorePageInformation()
            End If

            ' D6688 ===
            'Check: Is this session has active project, page working with project data And info could be saved, try to reset the pipe for avoid issues on calculates/change data
            If StorePageID AndAlso App.isAuthorized AndAlso App.HasActiveProject AndAlso App.ActiveProject.IsProjectLoaded AndAlso App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated AndAlso PGAction IsNot Nothing AndAlso (PGAction.Permission = ecPagePermission.ppProject OrElse PGAction.Permission = ecPagePermission.ppProjectWithLock) Then
                Select Case CurrentPageID
                    Case _PGID_EVALUATION, _PGID_EVALUATE_INTENSITIES, _PGID_EVALUATE_READONLY, _PGID_EVALUATE_RISK_CONTROLS, _PGID_EVALUATE_RISK_CONTROLS_READONLY
                    Case Else
                        App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False
                End Select
            End If
            ' D6688 ==

            ' D6495 ===
            If App.isAuthorized AndAlso (fDoSaveInfo OrElse CurrentPageID = _PGID_WEBAPI OrElse CurrentPageID = _PGID_PING) Then
                If StorePageID AndAlso fDoSaveInfo Then ' D4544
                    StorePageInformation() ' D4536
                Else
                    If App.ActiveUser IsNot Nothing Then App.DBUpdateDateTime(clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_LASTVISITED, App.ActiveUser.UserID) ' D4544
                End If
            End If
            ' D6495 ==
            Session(_SESS_APP) = App
        End Sub

        ' D0743 ===
        ' Add/Edit meta "<meta http-equiv="X-UA-Compatible" content="IE=EmulateIE7" />" in head section;
        Private Sub Page_PreRenderComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRenderComplete
            'If HasListPage(_PAGESLIST_HTML5, CurrentPageID) Then isHTML5Page = True ' D3464
            If Page.Header IsNot Nothing Then
                ' D4764 ===
                For i As Integer = Page.Header.Controls.Count - 1 To 0 Step -1
                    ' D1643 ===
                    If TypeOf (Page.Header.Controls(i)) Is HtmlMeta Then
                        Select Case CType(Page.Header.Controls(i), HtmlMeta).Name.ToLower
                            Case "application-name", "apple-mobile-web-app-title"
                                CType(Page.Header.Controls(i), HtmlMeta).Content = ApplicationName(False)
                        End Select
                    End If
                Next
                ' D4764 ==

                ' -D6491
                'Dim fHasIE7 As Boolean = False
                'Dim IE7Mode As String = "<meta http-equiv=""X-UA-Compatible"" content=""IE=EmulateIE7"" />"
                'If Not isHTML5Page Then
                '    For i As Integer = Page.Header.Controls.Count - 1 To 0 Step -1
                '        ' D1643 ===
                '        If TypeOf (Page.Header.Controls(i)) Is HtmlMeta Then
                '            Dim meta As HtmlMeta = CType(Page.Header.Controls(i), HtmlMeta)
                '            If meta.HttpEquiv.ToLower.Contains("compatible") Then
                '                Page.Header.Controls.RemoveAt(i)
                '                Exit For
                '                'fHasIE7 = True
                '                'meta.Text = IE7Mode
                '                ' D1643 ==
                '            End If
                '            ' -D0868
                '            'Dim favIdx As Integer = meta.Text.ToLower.IndexOf("favicon.ico")    ' D0867
                '            'If favIdx > 0 Then meta.Text = meta.Text.Substring(0, favIdx) + ApplicationURL() + meta.Text.Substring(favIdx) ' D0867
                '        End If
                '    Next
                '    If Not fHasIE7 Then    ' D3406 + D3464
                '        ' D1643 ===
                '        Dim meta As New HtmlMeta()
                '        meta.HttpEquiv = "X-UA-Compatible"
                '        meta.Content = "IE=EmulateIE7"
                '        ' D1643 ==
                '        Page.Header.Controls.AddAt(0, meta)
                '    End If
                'End If

                ' D1498 ===
                If App.isAuthorized Then
                    Dim sData As String = ""
                    If App.ActiveUser IsNot Nothing Then sData += String.Format(" * User: <{0}>, ""{1}""" + vbCrLf, App.ActiveUser.UserEmail, App.ActiveUser.UserName)
                    ' D6040 ===
                    If App.ActiveWorkgroup IsNot Nothing Then sData += String.Format(" * Workgroup: ""{0}""" + vbCrLf, App.ActiveWorkgroup.Name)
                    If App.HasActiveProject Then sData += String.Format(" * Project: ""{0}"" (Access code: {1}{2}, #{3})" + vbCrLf, App.ActiveProject.ProjectName, App.ActiveProject.Passcode, If(App.isRiskEnabled, String.Format(" [{0}]", If(App.ActiveProject.isImpact, "Impact", "Likelihood")), ""), App.ProjectID) ' D3308 + D3494
                    Dim StatusLabel As New LiteralControl(String.Format("<!--" + vbCrLf + " -=- Session information -=- " + vbCrLf + "{0} -->", sData))
                    Page.Header.Controls.AddAt(If(Page.Header.Controls.Count > 2, 2, 0), StatusLabel)
                End If
                ' D1498 + D6040 ==
            End If

            'TODO ping
            '' D3987 ===
            'If App.isAuthorized AndAlso Not isCallback() AndAlso Not IsPostBack Then
            '    Dim CurPg As clsPageAction = PageAction(CurrentPageID, False)
            '    If CurPg IsNot Nothing AndAlso Not HasListPage(_PAGESLIST_IGNORE_PING, CurPg.ID) Then   ' D3993 + D4032
            '        If CurPg.Permission <> ecPagePermission.ppUnspecified AndAlso CurPg.Permission <> ecPagePermission.ppUnAuthorized AndAlso CurPg.Permission <> ecPagePermission.ppEveryone Then
            '            ClientScript.RegisterStartupScript(GetType(String), "InitPing", String.Format("if (typeof _doPing == 'function') {{ ping_address='{1}'; ping_data = {0}; _doPing(); }}", JS_SessionData(), JS_SafeString(ApplicationURL(False, False) + PageURL(_PGID_PING))), True)
            '        End If
            '    End If
            'End If
            '' D3987 ==

            ' D4023 ===
            If _OPT_SHOW_HELP_AUTHORIZED Then    ' D6235 + D6261
                Dim sSession As String = GetCookie(_COOKIE_ACTIVE_SESS, "")
                If sSession <> Bool2Num(App.isAuthorized).ToString Then
                    SetCookie(_COOKIE_ACTIVE_SESS, Bool2Num(App.isAuthorized).ToString, True, False)
                    If _OPT_HELP_ROBOHELP AndAlso SessVar(_SESS_HELP_CHECKED) <> "1" Then   ' D6261
                        Try
                            Dim sFile As String = String.Format("{0}{1}whver.js", _FILE_ROOT, ResString(CStr(IIf(App.isRiskEnabled, "help_BaseRisk", "help_Base")))).Replace("/", "\")  ' D6235
                            If My.Computer.FileSystem.FileExists(sFile) Then
                                Dim sScript As String = My.Computer.FileSystem.ReadAllText(sFile)
                                If Not sScript.Contains("Expertchoice") Then
                                    Dim sInjection As String = File_GetContent(String.Format("{0}Scripts\help_injection.js", _FILE_ROOT), "")   ' D6235
                                    If sInjection <> "" Then My.Computer.FileSystem.WriteAllText(sFile, ParseAllTemplates(sInjection, Nothing, Nothing, True, True), True)
                                End If
                            End If
                        Catch ex As Exception
                        End Try
                        SessVar(_SESS_HELP_CHECKED) = "1"
                    End If
                End If
            End If
            ' D4023 ==

            ' D7413 ===
            If App.ApplicationError.Status <> ecErrorStatus.errNone Then
                Select Case CurrentPageID
                    Case _PGID_START, _PGID_START_WITH_SIGNUP, _PGID_ERROR_403, _PGID_ERROR_404, _PGID_ERROR_500, _PGID_ERROR_503, _PGID_UNKNOWN, _PGID_SERVICEPAGE, _PGID_JS_RESOURCES, _PGID_WEBAPI    ' D0298 + D0391 + D0439 + D1056 + D1953 + D2780 + D4630
                    Case Else
                        If Not HasListPage(_PAGESLIST_IGNORE_PING, CurrentPageID) Then App.ApplicationError.Reset()
                End Select
            End If
            ' D7413 ==

            If App IsNot Nothing Then Session(_SESS_APP) = App  ' D6006
            If App IsNot Nothing AndAlso App.Database IsNot Nothing Then App.Database.Close() ' D2235 + D3996
        End Sub
        ' D0743 ==

        Private Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
            If App IsNot Nothing Then Session(_SESS_APP) = App  ' D6006
            If App IsNot Nothing AndAlso App.Database IsNot Nothing Then App.Database.Close()
            swMain.Stop()
            If Not MyBase.IsCallback AndAlso Not IsPostBack AndAlso CurrentPageID <> _PGID_WEBAPI Then Debug.Print("ASP page '" + CurrentPageID.ToString + "' generated on server for " + swMain.ElapsedMilliseconds.ToString + " ms.")
        End Sub

        ' D7641 ===
        Private Sub clsComparionCorePage_PreLoad(sender As Object, e As EventArgs) Handles Me.PreLoad ' D7645
            If Not CheckAntiCSRF() Then
                App.DBSaveLog(dbActionType.actShowRTE, dbObjectType.einfRTE, -1, "Validation of Anti-CSRF token", "Validation failed")
                FetchWithCode(HttpStatusCode.BadRequest, CurrentPageID = _PGID_WEBAPI, "Validation of Anti-CSRF token failed.")
            End If
        End Sub
        ' D7641 ==

#End Region

#Region "Anti CSRF"
        ' D7641 ===
        Public _antiXsrfToken As String = ""
        Const _COOKIE_CSRF_TOKEN As String = "_csrf_token"
        Const _SESS_CSRF_TOKEN As String = "csrf_token"
        Const _FORM_CSRF_TOKEN As String = "csrf_token"

        Private Sub InitAntiCSRF()
            Dim requestCookie As String = GetCookie(_COOKIE_CSRF_TOKEN, "")
            Dim requestCookieGuidValue As Guid
            If (Not String.IsNullOrEmpty(requestCookie) AndAlso Guid.TryParse(requestCookie, requestCookieGuidValue)) Then
                _antiXsrfToken = requestCookie
            Else
                ' Generate a new CSRF token
                _antiXsrfToken = Guid.NewGuid.ToString("N") ' D7645
                SetCookie(_COOKIE_CSRF_TOKEN, _antiXsrfToken, True, False)
                'SessVar(_SESS_CSRF_TOKEN) = _antiXsrfToken    ' D7645
            End If
            If Session(_SESS_CSRF_TOKEN) Is Nothing Then SessVar(_SESS_CSRF_TOKEN) = _antiXsrfToken    ' D7645 + D7647
            'Page.ViewStateUserKey = _antiXsrfToken
            'AddHandler Page.PreLoad, AddressOf clsComparionCorePage_PreLoad
        End Sub

        Private Function CheckAntiCSRF() As Boolean
            'Dim sUserID As String = String.Format("{0}{1}", If(Session IsNot Nothing AndAlso Not String.IsNullOrEmpty(Session.SessionID), Session.SessionID.Replace("@", ""), App.Options.SessionID).Substring(0, 12), GetClientIP(Request).Replace(" ", "").Replace(".", "").Replace(",", "").Substring(0, 12))   ' D6744
            Dim fRes As Boolean = True
            ' D7645 ===
            If IsPostBack AndAlso Not isCallback AndAlso CurrentPageID <> _PGID_WEBAPI Then ' D7644
                ' Validate the CSRF token
                Dim sTokenSess As String = SessVar(_SESS_CSRF_TOKEN)
                Dim sTokenForm As String = CheckVar(_FORM_CSRF_TOKEN, "")
                If sTokenSess <> _antiXsrfToken OrElse sTokenForm <> _antiXsrfToken Then    ' D6744
                    ' D7645 ==
                    fRes = False
                End If
            End If
            Return fRes
        End Function
        ' D7641 ==
#End Region

    End Class

End Namespace

