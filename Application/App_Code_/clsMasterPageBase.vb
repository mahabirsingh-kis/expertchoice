Namespace ExpertChoice.Web

    Public Class clsMasterPageBase
        Inherits System.Web.UI.MasterPage

        Public __o As Object    ' dirty fix for VS error "'__o' is not declared"

        Private _PageLink As String = Nothing   ' D4943
        Private _LayoutName As String = Nothing ' D5043
        Private _CurPgID As Integer = -1        ' D5064

        Private _PagesParsed As Dictionary(Of Integer, List(Of PageDetails))    ' D5057
        Private _DTCode As String = Nothing                             ' D5058
        'Private _Wireframes As List(Of String)                          ' D6101 - D7253

        Public Const OPT_SHOW_EXPIRED_WORKGROUPS As Boolean = False     ' D4955
        Public Const OPT_ALLOW_RISKION_WORKGROUPS As Boolean = True     ' D4993

        Public Const _SESS_DT_CODE As String = "dtCode"                 ' D5058
        'Public Const _COOKIE_LAYOUT As String = "mnuLayout"             ' D5043 -D6757
        Public _LAYOUTS_ALLOWED_COMPARION As String() = {"default"}     ' D5043 + D6008 (-"2018")
        Public _LAYOUTS_ALLOWED_RISKION As String() = {"default", "riskion2"}   ' D6757

        Private isAdmin As Boolean = False  ' D6023

        ' D4898 ===
        Structure PageDetails
            Dim PgID As Integer
            Dim UID As String
            Dim Title As String
            Dim Parent As String
            Dim isHTML5 As Boolean
            Dim isDraft As Boolean
            Dim isReady As Boolean
            Dim isMissing As Boolean
            Dim isVisible As Boolean
            Dim Desc As String
        End Structure

        ' D4898 ==
        Public HotKeys As New Dictionary(Of String, Integer)    ' D4660

        Private swMain As New Stopwatch()

        Public Sub New()
            swMain.Start()
        End Sub

        Public ReadOnly Property _Page As clsComparionCorePage
            Get
                Return CType(Page, clsComparionCorePage)
            End Get
        End Property

        Public ReadOnly Property App As clsComparionCore
            Get
                Return _Page.App
            End Get
        End Property

        ' D5058 ===
        Public ReadOnly Property DTCode As String
            Get
                If String.IsNullOrEmpty(_DTCode) Then
                    If Session.Item(_SESS_DT_CODE) Is Nothing Then
                        _DTCode = ""
                        Dim sFile = _FILE_ROOT + "\Bin\Application.dll"
                        If _Page.MyComputer.FileSystem.FileExists(sFile) Then
                            Dim DT As Date = IO.File.GetLastWriteTime(sFile)
                            _DTCode = DT.ToString("yyMMdd.HHmmss.ff") ' D7355
                            ' D6643 ===
                            sFile = _FILE_ROOT + " \ Bin"
                            If _Page.MyComputer.FileSystem.DirectoryExists(sFile) Then
                                Dim DT2 As Date = IO.File.GetLastWriteTime(sFile)
                                If DT2 > DT Then _DTCode = DT2.ToString("yyMMdd.HHmmss.ff") ' D7355
                            End If
                            ' D6643 ==
                        End If
                        If _DTCode = "" Then _DTCode = App.GetCoreVersion.ToString()
                        Session.Add(_SESS_DT_CODE, _DTCode)
                    Else
                        _DTCode = CStr(Session(_SESS_DT_CODE))
                    End If
                End If
                Return _DTCode
            End Get
        End Property
        ' D5058 ==

        ' D7242 ===
        Public ReadOnly Property EvalPageID As Integer
            Get
                Dim sPgID = _Page.SessVar(clsComparionCorePage.SESSION_PIPE_PGID)
                Dim pg As Integer
                If Not Integer.TryParse(sPgID, pg) Then pg = _PGID_EVALUATION
                Return pg
            End Get
        End Property
        ' D7242 ==

        ' D6757 ===
        Public ReadOnly Property _LAYOUTS_ALLOWED As String()
            Get
                Return If(App.isRiskEnabled, _LAYOUTS_ALLOWED_RISKION, _LAYOUTS_ALLOWED_COMPARION)
            End Get
        End Property

        Public ReadOnly Property _COOKIE_LAYOUT As String
            Get
                Return If(App.isRiskEnabled, "mnuLayoutRisk", "mnuLayout")
            End Get
        End Property
        ' D6757 ==

        ' D6260 ===
        Public ReadOnly Property AllowLinks As Boolean
            Get
                Return _Page.CurrentPageID <> _PGID_EULA AndAlso _Page.NavigationPageID <> _PGID_EULA AndAlso _Page.CurrentPageID <> _PGID_CREATE_PASSWORD AndAlso Not (_Page.CurrentPageID = _PGID_PINCODE AndAlso Not _Page.ShowNavigation)   ' D6269 + D7187
            End Get
        End Property
        ' D6260 ==

        Public Function ResString(ByVal sResourceName As String, Optional ByVal fAsIsIfMissed As Boolean = False, Optional ByVal fParseTemplates As Boolean = True, Optional ByVal fCapitalized As Boolean = False) As String
            Return _Page.ResString(sResourceName, fAsIsIfMissed, fParseTemplates, fCapitalized)
        End Function

        Public Function ResStringJS(ByVal sResourceName As String) As String
            Return JS_SafeString(ResString(sResourceName))
        End Function

        Public Function ParseString(sTxt As String) As String
            Return _Page.ParseString(sTxt)
        End Function

        Public ReadOnly Property ShowPreloader As Boolean
            Get
                Return _Page.CheckVar("preloader", True)
            End Get
        End Property

        ' D5043 ===
        Public Property LayoutName() As String
            Get
                If String.IsNullOrEmpty(_LayoutName) Then
                    _LayoutName = _Page.GetCookie(_COOKIE_LAYOUT, "")   ' D5044
                    If Not _LAYOUTS_ALLOWED.Contains(_LayoutName) Then _LayoutName = _LAYOUTS_ALLOWED(0)
                End If
                Return _LayoutName
            End Get
            Set(value As String)
                _LayoutName = value
                _Page.SetCookie(_COOKIE_LAYOUT, value, False, False)    ' D5044
            End Set
        End Property
        ' D5043 ==

        Private Sub onPageInit(sender As Object, e As EventArgs) Handles Me.Init
            _Page.AlignHorizontalCenter = False
            _Page.AlignVerticalCenter = False
            If App.isAuthorized Then isAdmin = App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID)  ' D6023
        End Sub

        Private Sub clsMasterPageBase_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
            If Page.Header IsNot Nothing AndAlso Page.Header.Controls IsNot Nothing Then ' D4628 + D4652
                If Page.MasterPageFile.ToLower.Contains("mpdesktop") OrElse Page.MasterPageFile.ToLower.Contains("mpempty") Then
                    Page.Header.Controls.Add(New LiteralControl(String.Format("<script type=""text/javascript"" src=""{0}?dt={1}""></script>" + vbCrLf, _Page.PageURL(_PGID_JS_RESOURCES), DTCode)))   ' D4629 + D4657 + D5058
                    ' D4820 ===
                    Page.Header.Controls.Add(New LiteralControl(String.Format("<meta name=""application-name"" content=""{0}""/>", _Page.ApplicationName)))
                    Page.Header.Controls.Add(New LiteralControl(String.Format("<meta name=""apple-mobile-web-app-title"" content=""{0}""/>", _Page.ApplicationName)))
                    ' D4910 ===
                    Dim fShowMobile As Boolean = App.isMobileBrowser AndAlso Not _Page.NoMobileView
                    If _Page.CheckVar("mobile", "") <> "" Then  ' D4939
                        If fShowMobile AndAlso Not _Page.CheckVar("mobile", True) Then fShowMobile = False
                        If Not fShowMobile AndAlso _Page.CheckVar("mobile", False) Then fShowMobile = True
                    End If
                    If fShowMobile Then
                        Page.Header.Controls.Add(New LiteralControl("<meta name = ""viewport"" content=""width=device-width, initial - scale = 1""/>"))    ' D4850 + D4906
                        ' -D5057
                        'Else   
                        '    Page.Header.Controls.Add(New LiteralControl("<meta name = ""viewport"" content=""width=device-width""/>"))    ' D5000
                    End If
                    ' D4820 + D4910 ==
                    If Request IsNot Nothing AndAlso Not Request.IsLocal AndAlso Page.MasterPageFile.ToLower.Contains("mpdesktop") Then Page.Header.Controls.Add(New LiteralControl(String.Format("<script async src=""//www.googletagmanager.com/gtag/js?id={0}""></script>", HttpUtility.UrlEncode(WebConfigOption(_OPT_GOOGLE_UID, _DEF_GOOGLE_UA, False))))) ' D4668 + D5037 + D5067 + D6745
                    If ShowChatSupport() AndAlso Page.MasterPageFile.ToLower.Contains("mpdesktop") Then Page.Header.Controls.Add(New LiteralControl("<script src=""//www.socialintents.com/api/socialintents.1.3.js#2c9fa7465c3c6e61015c3dd5e6d700f5"" async=""async""></script>"))   ' D4668 + D4910 + D6012
                End If
                CheckHeaderControls(Page.Header.Controls, DTCode)    ' D5058 + D6584
            End If
            swMain.Stop()
            If Not IsPostBack Then Debug.Print(Page.MasterPageFile + " generated on server for " + swMain.ElapsedMilliseconds.ToString + " ms.")
        End Sub

        ' D6584 ===
        Private Sub CheckHeaderControls(ByRef Controls As ControlCollection, DTCode As String)
            Dim sCode As String = ".js?dt=" + DTCode
            For Each tItem As Control In Controls
                If TypeOf tItem Is LiteralControl Then
                    Dim tControl As LiteralControl = CType(tItem, LiteralControl)
                    If tControl.Text.ToLower.Contains("<script ") AndAlso tControl.Text.ToLower.Contains(".js") Then tControl.Text = tControl.Text.Replace(".js'", sCode + "'").Replace(".js""", sCode + """")
                    If _Page.Request.IsLocal AndAlso tControl.Text.ToLower.Contains("cloudfront.net") Then tControl.Text = "<!-- " + tControl.Text + " -->"
                End If
                If Not _Page.isCallback AndAlso Not _Page.IsPostBack AndAlso Not _Page.isAJAX Then
                    If TypeOf tItem Is HtmlLink Then
                        Dim tControl As HtmlLink = CType(tItem, HtmlLink)
                        If tControl.Href.ToLower.EndsWith(".css") Then
                            Select Case _Page.CurrentPageID
                                ' Ignore these pages since the issue with CSS on Custom.aspx (case #21033)
                                Case _PGID_REPORT_CUSTOM, _PGID_REPORT_CUSTOM, _PGID_REPORT_CUSTOM_PRIORITY, _PGID_REPORT_CUSTOM_JUDGMENTS, _PGID_REPORT_CUSTOM_OBJ_PRIORITY, _PGID_REPORT_CUSTOM_EVAL_PRG,
                                     _PGID_REPORT_CUSTOM_INCONSIST, _PGID_REPORT_CUSTOM_SURVEY, _PGID_SYNTHESIZE_INCONSIST, _PGID_SYNTHESIZE_SURVEY, _PGID_REPORT_CSV
                                Case Else
                                    tControl.Href += "?dt=" + DTCode
                            End Select


                        End If
                    End If
                End If
                If TypeOf tItem Is ContentPlaceHolder Then
                    CheckHeaderControls(tItem.Controls, DTCode)
                End If
            Next
        End Sub
        ' D6584 ==

        Public Function GetSitemapJSON(Optional isCompact As Boolean = False) As String ' D5043
            Dim sMenu As String = ""
            _CurPgID = _Page.CheckVar(_PARAM_PAGE, _Page.CheckVar("pg", _Page.CheckVar(_PARAM_NAV_PAGE, _Page.CurrentPageID)))  ' D5064
            'If _Page.ShowNavigation AndAlso SiteMap.RootNode IsNot Nothing Then
            If SiteMap.RootNode IsNot Nothing Then ' D4819
                Dim sw As New Stopwatch
                sw.Start()
                Dim fHasActive As Boolean = False
                Dim ItemsCount As Integer = 0 ' D4661
                _PagesParsed = New Dictionary(Of Integer, List(Of PageDetails)) ' D5057
                '_Wireframes = GetProjectFilesList(Server.MapPath("~/Wireframes/"), {".htm", ".html", ".jpg", ".jpeg", ".png", ".gif"})  ' D6101 -D7253
                sMenu = LoadMenuItems(SiteMap.RootNode, vbTab, fHasActive, ItemsCount, "", "", "", isCompact)  ' D4865 + D5043 + D5059
                If ItemsCount < 2 Then _Page.ShowNavigation = False ' D4661
                sw.Stop()
                If Not IsPostBack Then Debug.Print("GetSitemapJSON(): " + sw.ElapsedMilliseconds.ToString + " ms")
            End If
            Return sMenu
        End Function

        ' D4898 + D4959 ===
        Private Function GetPageStatPerc(Title As String, Value As Integer, Mode As String, Total As Integer) As String
            If Total > 0 Then
                Return String.Format("<div>{2}: <div class='gray' style='float: right; width:4em; font-size:80%; text-align:right'>{0}%</div><a href='' class='actions' onclick='showStats(""{3}"", ""{2}""); return false'>{1}</a></div>", (100 * Value / Total).ToString("F0"), Value, Title, Mode)
                ' D4959 ==
            End If
            Return ""
        End Function

        ''' <summary>
        ''' Get pages statistic information
        ''' </summary>
        ''' <returns>Html string containing information about counts of Total pages, html5 enabled pages, drafts and missing pages</returns>
        Public Function GetPagesStatistic() As String
            If SiteMap.RootNode IsNot Nothing Then
                Dim sw As New Stopwatch()
                sw.Start()
                Dim PagesList As New List(Of PageDetails)
                Parse4Stat(SiteMap.RootNode, "", "", PagesList, "")  ' D5062
                Dim HTML5 As Integer = 0
                Dim Draft As Integer = 0
                Dim Missing As Integer = 0
                Dim sList As String = ""
                For Each Pg As PageDetails In PagesList
                    If Pg.isDraft Then Draft += 1
                    If Pg.isMissing Then Missing += 1
                    If Pg.isHTML5 Then HTML5 += 1
                    sList += String.Format("<div class='{0}{1}{2}{3}' style='white-space: nowrap;' data-pgid='{5}'>{4}</div>", If(Pg.isDraft, " ps_draft", ""), If(Pg.isMissing, " ps_missing", ""), If(Pg.isHTML5, " ps_html5", ""), If(Not Pg.isHTML5 AndAlso Not Pg.isMissing, " ps_old", ""), If(Pg.Parent = "", "", "<span class='small gray'>" + Pg.Parent + " &gt; </span>") + "<!--{-->" + Pg.Title + "<!--}-->", Pg.PgID)
                Next
                Dim retVal As String = String.Format("<div style='display:none; max-height:12em; overflow-y: auto;'>{5}</div>Total pages count: {0}{1}{2}{3}{4}", PagesList.Count, GetPageStatPerc("HTML5", HTML5, "html5", PagesList.Count), GetPageStatPerc("HTML old", (PagesList.Count - HTML5 - Missing), "old", PagesList.Count), GetPageStatPerc("Draft", Draft, "draft", PagesList.Count), GetPageStatPerc("Missing", Missing, "missing", PagesList.Count), sList)
                sw.Stop()
                If Not IsPostBack Then Debug.Print("GetPagesStatistic(): " + sw.ElapsedMilliseconds.ToString + " ms")
                Return retVal
            End If
            Return "- No data -"
        End Function
        ' D4898 ==

        ' D4959 ===
        Private Sub Parse4Stat(tNode As SiteMapNode, sParent As String, sTab As String, ByRef tLst As List(Of PageDetails), sHID As String)
            If tNode IsNot Nothing Then
                ' D5062 ===
                For Each sRole As String In tNode.Roles
                    If sRole.ToLower.StartsWith("layout=") Then
                        If LayoutName().ToLower <> sRole.Substring(7).ToLower Then
                            Exit Sub
                        End If
                    End If
                Next
                ' D5062 ==
                Dim PgID As Integer = -1
                ' D5062 ===
                If App.isRiskEnabled Then
                    If tNode.Roles.Contains("impact") Then
                        sHID = "impact"
                    End If
                    If tNode.Roles.Contains("likelihood") Then
                        sHID = "likelihood"
                    End If
                End If
                Dim sTitle = If(tNode.Title.ToLower = "root", "", tNode.Title)
                Dim sResName = sTitle
                If sTitle.StartsWith("mnu") OrElse sTitle.StartsWith("nav") Then
                    sTitle = ResString(sTitle)
                    If sHID <> "" Then
                        sResName = sTitle + sHID
                        If App.CurrentLanguage.Resources.ParameterExists(sResName) Then
                            sTitle = ResString(sResName)
                        End If
                    End If
                End If
                ' D5062 ==
                If Integer.TryParse(tNode.ResourceKey, PgID) Then
                    Dim PgNew As New PageDetails
                    With PgNew
                        If Not tNode.Roles.Contains("resname") Then
                            sTitle = _Page.PageMenuItem(PgID)
                            ' D5062 ===
                            If sHID <> "" Then
                                sResName = sTitle + sHID
                                If App.CurrentLanguage.Resources.ParameterExists(sResName) Then
                                    sTitle = ResString(sResName)
                                End If
                            End If
                        End If
                        ' D5062 ==
                        .PgID = PgID
                        .Title = sTitle
                        .Parent = If(sTab = "", "", sTab + If(sParent = "", "", " / ")) + sParent ' D5062
                        .isDraft = tNode.Roles.Contains("draft")
                        .isHTML5 = tNode.Roles.Contains("html5")
                        .isMissing = tNode.Roles.Contains("missing") OrElse _Page.PageByID(PgID) Is Nothing
                        If _Page.PageByID(PgID) Is Nothing AndAlso sTitle = "" Then .Title = String.Format("#{0}", PgID)    ' D5062
                    End With
                    tLst.Add(PgNew)
                End If
                If tNode.Roles.Contains("tab") Then sTab = sTitle   ' D5062
                If tNode.HasChildNodes Then
                    For Each tChild As SiteMapNode In tNode.ChildNodes
                        If (Not tNode.Roles.Contains("riskion") AndAlso Not App.isRiskEnabled) OrElse (Not tNode.Roles.Contains("comparion") AndAlso App.isRiskEnabled) Then
                            Parse4Stat(tChild, sTitle, sTab, tLst, sHID)  ' D5062
                        End If
                    Next
                End If
            End If
        End Sub
        ' D4959 ==

        ' D4943 ===
        Public Function GetPageLink() As String
            If _PageLink Is Nothing Then
                _PageLink = ""
                ' -D6579 // temporary disable
                'If App.isAuthorized AndAlso Request IsNot Nothing AndAlso Request.Url IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveUserWorkgroup IsNot Nothing AndAlso Not _Page.isSSO Then  ' D6552
                '    Dim tAdminGrpID As Integer = App.ActiveWorkgroup.RoleGroupID(ecRoleGroupType.gtAdministrator)
                '    If App.ActiveUserWorkgroup.RoleGroupID <> tAdminGrpID Then
                '        _PageLink = _Page.CreateLogonURL(App.ActiveUser, If(App.HasActiveProject, App.ActiveProject, Nothing), "", "", If(App.HasActiveProject, App.ActiveProject.Passcode(App.ActiveProject.isImpact), ""), App.ActiveUserWorkgroup, False)
                '        Dim sParams As String = _Page.GetParamsWithoutAuthKeys(Request.QueryString) ' D5050
                '        Select Case _Page.NavigationPageID
                '            ' -D4945 // use onGetDirectLink() on client side
                '            'Case _PGID_EVALUATION, _PGID_EVALUATE_RISK_CONTROLS
                '            '    If App.ActiveWorkspace IsNot Nothing AndAlso Not sParams.ToLower.Contains(_PARAM_STEP + "=") Then
                '            '        sParams += String.Format("{0}{1}={2}", If(sParams = "", "", ","), _PARAM_STEP, App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact))
                '            '    End If
                '            Case _PGID_START, _PGID_LOGOUT, _PGID_START_WITH_SIGNUP, _PGID_SILVERLIGHT_UI, _PGID_SERVICEPAGE, _PGID_ERROR_403, _PGID_ERROR_404, _PGID_ERROR_500, _PGID_ERROR_503
                '                _PageLink = ""
                '        End Select
                '        If _PageLink <> "" Then
                '            _PageLink = String.Format("{0}{1}{2}{3}", _Page.ApplicationURL(False, _Page.NavigationPageID = _PGID_TEAMTIME).Trim(CChar("/")), Request.Url.AbsolutePath, _PageLink, If(sParams = "", "", "&" + sParams))
                '        End If
                '    End If
                'End If
            End If
            Return _PageLink
        End Function
        ' D4943 ==

        Private Function LoadMenuItems(tRootItem As SiteMapNode, sIndent As String, ByRef fHasActiveItems As Boolean, ByRef ItemsCount As Integer, ParentUID As String, ParentName As String, HierarchyRequest As String, Optional AsCompact As Boolean = False) As String   ' D4865 + D4898 + D5043 + D5059
            Dim fShowDraft As Boolean = True    ' ShowDraftPages ' D4889
            Dim sItems As String = ""
            fHasActiveItems = False
            If tRootItem IsNot Nothing Then
                'Dim sw As New Stopwatch()
                'sw.Start()
                'Dim sPg As String = _Page.CheckVar("pg", "").Trim.ToLower   ' D4705
                Dim HasActiveChild As Boolean = False
                Dim HasActiveLinks = False
                For Each tItem As SiteMapNode In tRootItem.ChildNodes
                    Dim PgID As Integer = -1
                    Dim tPg As clsPageAction = Nothing
                    Dim sName As String = tItem.Title
                    Dim sTitle As String = ""   ' D4642
                    Dim sURL As String = tItem.Url
                    Dim sIcon As String = "" ' "fa fa-genderless"   ' D5057
                    Dim sHotkey As String = ""  ' D4645
                    Dim sExtra As String = ""
                    Dim sJS As String = ""  ' D4917
                    Dim fEnabled As Boolean = True
                    Dim fVisible As Boolean = True  ' D4639
                    Dim sChilds As String = ""
                    Dim sPosition As String = ""
                    Dim sHID As String = HierarchyRequest   ' D5059
                    'Dim sHelp As String = tItem.Url.Trim.TrimStart(CChar("/"))   ' D4668
                    Dim sHelp As String = tItem.Url.Trim    ' D4668 + D4774
                    Dim sDesc As String = tItem.Description.Trim    ' D4668
                    Dim fIgnore As Boolean = tItem.Roles.Contains("ignore") ' D4661
                    Dim isWorkflow As Boolean = False  ' D4942
                    ' D4639 ===
                    If (tItem.Roles.Contains("riskion") AndAlso Not App.isRiskEnabled) OrElse
                       (tItem.Roles.Contains("comparion") AndAlso App.isRiskEnabled) OrElse
                       (tItem.Roles.Contains("overview") AndAlso Not _Page.isReviewAccount) OrElse
                       (tItem.Roles.Contains("nonadmin") AndAlso App.isSystemManager(App.ActiveUser)) OrElse
                       (tItem.Roles.Contains("project") AndAlso Not App.HasActiveProject) OrElse
                       (tItem.Roles.Contains("draft") AndAlso Not fShowDraft) OrElse
                       (tItem.Roles.Contains("auth") AndAlso Not App.isAuthorized) OrElse
                       (tItem.Roles.Contains("projectslist") AndAlso App.Options.isSingleModeEvaluation) OrElse
                       (tItem.Roles.Contains("starred") AndAlso Not _Page.isStarredAvailable) OrElse
                       (tItem.Roles.Contains("myriskreward") AndAlso Not (App.HasActiveProject AndAlso App.ActiveProject.isMyRiskRewardModel)) OrElse
                       (tItem.Roles.Contains("csdata") AndAlso Not (App.HasActiveProject AndAlso App.CanActiveUserModifyProject(App.ProjectID))) OrElse
                       (tItem.Roles.Contains("releasenotes") AndAlso (Not _OPT_SHOW_RELEASE_NOTES OrElse Not App.isAuthorized OrElse Not App.CanUserDoAction(ecActionType.at_alCreateNewModel, App.ActiveUserWorkgroup, App.ActiveWorkgroup))) Then     ' D4645 + D6322 + D6347 + D6404 + D6417 + D6799 + D7325
                        fVisible = False
                        fEnabled = False
                    End If
                    For Each sParam As String In tItem.Roles
                        If sParam.StartsWith("~") Then
                            If App.isAuthorized AndAlso App.HasActiveProject Then
                                Dim _pgID As Integer = -1
                                If sParam <> "" AndAlso Integer.TryParse(tItem.ResourceKey, _pgID) Then
                                    Dim isPM As Boolean = App.CanActiveUserModifyProject(App.ProjectID)
                                    If HasListPage(_PAGESLIST_ANTIGUA, _pgID) AndAlso Not isPM Then sParam = ""
                                    If HasListPage({_PGID_PROJECTSLIST}, _pgID) AndAlso (App.Options.isSingleModeEvaluation OrElse App.Options.isLoggedInWithMeetingID OrElse App.ActiveProjectsList.Count < 2) Then sParam = ""
                                End If
                            End If
                            If sParam <> "" Then sHotkey = sParam.Substring(1)   ' D4645 + D4660
                        End If
                        ' D5043 ===
                        If sParam.ToLower.StartsWith("layout=") Then
                            If LayoutName().ToLower <> sParam.Substring(7).ToLower Then
                                fVisible = False
                                fEnabled = False
                            End If
                        End If
                        ' D5043 ==
                        If sParam = "no_report" AndAlso Session(clsComparionCorePage.SESSION_NO_REPORTS) IsNot Nothing Then sParam = "missing"  ' D4872
                        ' D4646 ===
                        Select Case sParam
                            Case "draft", "html5", "missing", "gecko", "help"
                                If fShowDraft Then sExtra += String.Format("{0}{1}", If(sExtra = "", "", ","), sParam)   ' D4645 + D4777
                                If sParam = "help" AndAlso (Not _Page.CanUserModifyActiveProject OrElse (App.HasActiveProject AndAlso App.ActiveProject.ProjectStatus <> ecProjectStatus.psActive)) Then fVisible = False ' D4938 + D6096
                            Case "alts", "no_aip", "collapsemenu", "hidemenu", "csdata", "projectslist", "no_wrap", "hidelookup", "new", "highlight"  ' D4889 + D6347 + D6829 + D6928
                                sExtra += String.Format("{0}{1}", If(sExtra = "", "", ","), sParam)   ' D4777
                            Case "tab", "workflow", "usermenu", "left"  ' D4817 + D4869
                                sPosition = sParam  ' D4817
                                If sParam = "workflow" Then isWorkflow = True ' D4942
                            Case "nomobile" ' D4906
                                If PgID = _Page.NavigationPageID Then _Page.NoMobileView = True  ' D4906
                                ' D5059 ===
                            Case "likelihood", "impact"
                                If App.isRiskEnabled Then
                                    sHID = sParam
                                End If
                                sExtra += String.Format("{0}{1}", If(sExtra = "", "", ","), sParam) ' D5061
                                ' D5059 ==
                            Case Else
                                If sParam.StartsWith("~") Then sExtra += String.Format("{0}hotkey", If(sExtra = "", "", ","))   ' D4645
                                If sParam.StartsWith("js:") Then sJS = sParam.Substring(3)   ' D4645
                        End Select
                        ' D4646 ==
                    Next
                    If fVisible Then
                        ' D4639 ==
                        If Integer.TryParse(tItem.ResourceKey, PgID) Then
                            PgID = Math.Abs(PgID)   ' D5057
                            tPg = _Page.PageByID(PgID)
                            If tPg Is Nothing Then PgID = -1
                        End If
                        If tPg IsNot Nothing Then
                            sName = _Page.PageMenuItem(PgID)
                            sTitle = _Page.PageTitle(PgID)  ' D4642
                            If sTitle.Contains("(!)") Then sTitle = ""  ' D4705
                            ' D5043 ===
                            If tItem.Roles.Contains("resname") Then
                                If (tItem.Title.StartsWith("nav") OrElse tItem.Title.StartsWith("mnu")) AndAlso App.CurrentLanguage.Resources.ParameterExists(tItem.Title) Then
                                    sName = ResString(tItem.Title)
                                    sTitle = sName  ' D6313
                                End If
                                If (tItem.Title.StartsWith("title")) AndAlso App.CurrentLanguage.Resources.ParameterExists(tItem.Title) Then sTitle = ResString(tItem.Title)   ' D4642
                            End If
                            ' D5043 ==
                            sURL = _Page.PageURL(PgID)
                            ' D4938 ===
                            If fEnabled Then
                                Dim fHasPerm As Boolean = _Page.HasPermission(tPg, If(App.HasActiveProject, App.ActiveProject, Nothing))
                                If Not fHasPerm Then fVisible = False
                                ' D4998 ===
                                fEnabled = fHasPerm
                                If PgID = _PGID_LOGOUT AndAlso HasListPage(_PAGESLIST_ANTIGUA, _Page.NavigationPageID) Then ' D4917 + D6239 // force to show "Exit" for non-authorized
                                    fEnabled = True
                                    fVisible = True
                                End If
                                ' D4998 ==
                            End If
                            ' D7231 ===
                            If PgID = _PGID_PINCODE AndAlso (Not _PINCODE_ALLOWED OrElse Not App.isAuthorized OrElse App.ActiveUser Is Nothing) Then   ' D7249
                                fEnabled = False
                                fVisible = False
                            End If
                            ' D7250 ===
                            If fEnabled AndAlso PgID = _PGID_PINCODE AndAlso App.ActiveUser IsNot Nothing AndAlso App.ActiveUser.CannotBeDeleted Then
                                fEnabled = False
                            End If
                            ' D4938 + D7231 + D7250 ==
                        Else
                            If (tItem.Title.StartsWith("nav") OrElse tItem.Title.StartsWith("mnu")) AndAlso App.CurrentLanguage.Resources.ParameterExists(tItem.Title) Then sName = ResString(tItem.Title)
                            If (tItem.Title.StartsWith("title")) AndAlso App.CurrentLanguage.Resources.ParameterExists(tItem.Title) Then sTitle = ResString(tItem.Title)   ' D4642
                            'fEnabled = sURL <> ""
                        End If
                        If fEnabled AndAlso sPosition.ToLower = "usermenu" AndAlso Not AllowLinks AndAlso PgID <> _PGID_LOGOUT Then fEnabled = False    ' D6260
                        If fEnabled AndAlso tItem.HasChildNodes Then
                            Dim fHasActive As Boolean = False
                            ' D4661 ===
                            Dim ChildsCount As Integer = 0
                            sChilds = LoadMenuItems(tItem, sIndent + vbTab, fHasActive, ChildsCount, GetPageUID(PgID, PgID.ToString + ParentUID + tItem.Url + tItem.Title), sName, sHID, AsCompact)  ' D4865 + D4898 + D4917 + D5043 + D5059
                            If ChildsCount > 0 Then
                                ItemsCount += ChildsCount
                                ' -D5057
                                'If ChildsCount = 1 AndAlso Not isWorkflow Then  ' D4942
                                '    fIgnore = True
                                'Else
                                '    ItemsCount += ChildsCount
                                'End If
                            End If
                            ' D4661 ==
                            If fHasActive AndAlso sChilds <> "" Then
                                'sChilds = ",items:[" + vbCrLf + sIndent + vbTab + sChilds + "]"
                                If Not isWorkflow Then sURL = ""    ' D4994
                            Else
                                sChilds = ""
                                fEnabled = False
                            End If
                        End If
                        If Not String.IsNullOrEmpty(tItem.Item("icon")) Then sIcon = tItem.Item("icon")
                        If fEnabled Then fHasActiveItems = True
                        If Not fEnabled AndAlso (tItem.Roles.Contains("hide") OrElse PgID <= 0) Then fVisible = False  ' D4639 + D4674
                        ' D4826 ===
                        If sURL = sHelp AndAlso sHelp <> "" Then    ' D4828
                            sHelp = String.Format("{0}{2}", _URL_ROOT + ResString(If(App.isRiskEnabled, "help_BaseRisk", "help_Base")), ResStringJS(If(App.isRiskEnabled, "help_StartRisk", "help_Start")), sURL.TrimStart(CChar("/")))
                            sURL = ""
                        End If
                        ' D4826 ==
                        ' D6231 ===
                        If Not _OPT_HELP_ROBOHELP Then
                            If Not String.IsNullOrEmpty(tItem.Item("ko")) Then
                                sHelp = tItem.Item("ko")
                            Else
                                If sURL <> "" Then sHelp = ""
                            End If
                        End If
                        ' D6231 ==
                    End If
                    Dim sHash As String = GetPageUID(PgID, PgID.ToString + ParentUID + tItem.Url + tItem.Title)  ' D4705 + D4865 + D4917
                    'If sName <> sTitle AndAlso sTitle <> "" Then Debug.WriteLine(String.Format("#{0}{1}{2}{1}{3}{1}{4}", PgID, vbTab, If(tPg Is Nothing, tItem.Title, tPg.Name), sName, sTitle))
                    If fVisible Then
                        Dim PgUID As Integer = -1
                        Integer.TryParse(sHash, PgID)   ' D4960
                        ' D4649 ===
                        If fIgnore Then
                            If sChilds <> "" Then sItems += String.Format("{0}{1}", If(sItems = "", "", "," + If(AsCompact, "", vbCrLf + sIndent + vbTab)), sChilds)    ' D5043
                        Else
                            ' D5057 ===
                            If PgID > 0 Then
                                Dim PD As New PageDetails With {
                                    .PgID = PgID,
                                    .isVisible = fVisible,
                                    .Title = sName,
                                    .UID = If(PgUID > 0, PgUID.ToString, "")
                                }
                                If _PagesParsed.ContainsKey(PgID) Then
                                    Dim Lst As List(Of PageDetails) = _PagesParsed(PgID)
                                    PgID += Lst.Count * _PGID_MAX_MOD
                                    If Not sURL.ToLower.Contains(_PARAM_NAV_PAGE) Then
                                        sURL = URLNavPage(sURL, PgID)
                                    Else   ' D5059
                                        Debug.Print(String.Format("Can't replace page #{0} with URL during the parse sitemap.", PgID, sURL))
                                    End If
                                    PD.PgID = PgID
                                    Lst.Add(PD)
                                Else
                                    Dim Lst As New List(Of PageDetails)
                                    Lst.Add(PD)
                                    _PagesParsed.Add(PgID, Lst)
                                End If
                            End If
                            ' D5057 ==
                            ' D5059 ===
                            If App.HasActiveProject AndAlso App.isRiskEnabled AndAlso sHID <> "" AndAlso sURL <> "" Then
                                'If sHID.ToLower = "likelihood" AndAlso App.ActiveProject.isImpact Then sURL = URLWithParams(sURL, String.Format("{0}={1}", _PARAM_PASSCODE, HttpUtility.UrlEncode(App.ActiveProject.PasscodeLikelihood)))
                                'If sHID.ToLower = "impact" AndAlso Not App.ActiveProject.isImpact Then sURL = URLWithParams(sURL, String.Format("{0}={1}", _PARAM_PASSCODE, HttpUtility.UrlEncode(App.ActiveProject.PasscodeImpact)))
                                If sHID.ToLower = "likelihood" Then sURL = URLWithParams(sURL, String.Format("{0}={1}", _PARAM_PASSCODE, HttpUtility.UrlEncode(App.ActiveProject.PasscodeLikelihood)))  ' D7343
                                If sHID.ToLower = "impact" Then sURL = URLWithParams(sURL, String.Format("{0}={1}", _PARAM_PASSCODE, HttpUtility.UrlEncode(App.ActiveProject.PasscodeImpact)))          ' D7343
                            End If
                            ' D5059 ==

                            ' -D7253
                            '' D6101 ===
                            'Dim sWF As String = ""
                            'If _Wireframes IsNot Nothing Then
                            '    Dim sPGID As String = PgID.ToString
                            '    For Each sFN As String In _Wireframes
                            '        If sFN.StartsWith(sPGID + ".") OrElse sFN.StartsWith(sPGID + " ") Then  ' D6118
                            '            sWF = HttpUtility.UrlEncode(sFN)    ' D6118
                            '            Exit For
                            '        End If
                            '    Next
                            'End If
                            '' D6101 ==

                            ' D6618 ===
                            If fEnabled AndAlso (PgID > 0 OrElse PgUID > 0) AndAlso sHotkey <> "" Then
                                If Not HotKeys.ContainsKey(sHotkey) Then
                                    HotKeys.Add(sHotkey, If(PgID > 0, PgID, PgUID))  ' D4660 + D4960
                                Else
                                    If sHID.ToLower = "impact" AndAlso App.ActiveProject.isImpact Then HotKeys(sHotkey) = If(PgID > 0, PgID, PgUID)
                                    If sHID.ToLower = "lieklihood" AndAlso Not App.ActiveProject.isImpact Then HotKeys(sHotkey) = If(PgID > 0, PgID, PgUID)
                                End If
                            End If
                            ' D6618 ==

                            ' D7343 ===
                            Dim isSelected As Boolean = (PgID = _CurPgID OrElse sHash = _CurPgID.ToString)
                            If isSelected AndAlso App.isRiskEnabled AndAlso App.HasActiveProject AndAlso sHID <> "" Then
                                If sHID.ToLower = "impact" AndAlso Not App.ActiveProject.isImpact Then isSelected = False
                                If sHID.ToLower = "likelihood" AndAlso App.ActiveProject.isImpact Then isSelected = False
                            End If
                            ' D7343 ==

                            'Dim sNodeItem As String = String.Format("{0}{{""text"":""{1}"",""pgID"":{2},""url"":""{3}"",""icon"":""{4}"",""title"":""{10}"",""disabled"":{5},""help"":""{13}"",""uid"":""{14}"",""pguid"":""{16}""{6}{8}{9}{11}{12}{7}{15}{17}{18}{19}}}", If(sItems = "", "", "," + If(AsCompact, "", vbCrLf + sIndent)), sName, PgID, If(fEnabled OrElse sChilds = "", sURL, ""), sIcon, Bool2JS(Not fEnabled), If(PgID = _CurPgID OrElse sHash = _CurPgID.ToString, ", ""selected"":true", ""), If(sChilds = "", "", String.Format(",""items"":[" + If(AsCompact, "", vbCrLf + sIndent + vbTab) + "{0}]", sChilds)), If(sExtra = "", "", ",""extra"":""" + sExtra + """"), If(sChilds <> "", ",""selectable"":false,""closeMenuOnClick"":false", ""), If(sTitle <> "" AndAlso sTitle <> sName, JS_SafeString(sTitle), ""), If(sHotkey = "", "", String.Format(",""hotkey"":""{0}""", JS_SafeString(GetHotkeyText(sHotkey)))), If(sPosition = "", "", ",""position"":""" + sPosition + """"), JS_SafeString(sHelp), JS_SafeString(sHash), If(fShowDraft AndAlso sDesc = "", "", ", ""desc"":""" + JS_SafeString(sDesc) + """"), JS_SafeString(ParentUID), If(sJS = "", "", String.Format(",""js"":""{0}""", JS_SafeString(sJS))), If(sHID = "", "", String.Format(",""hid"":""{0}""", sHID)), If(sWF = "", "", String.Format(",""wf"":""{0}""", JS_SafeString(sWF)))).Replace(vbLf, " ").Replace(vbCr, " ").Replace("  ", " ").Trim ' D4642 + D4645 + D4658 + D4660 + D4668 + D4705 + D4767 + D4865 + D4917 + D5043 + D5059 + D5064 + D6101 + D6221
                            Dim sNodeItem As String = String.Format("{0}{{""text"":""{1}"",""pgID"":{2},""url"":""{3}"",""icon"":""{4}"",""title"":""{10}"",""disabled"":{5},""help"":""{13}"",""uid"":""{14}"",""pguid"":""{16}""{6}{8}{9}{11}{12}{7}{15}{17}{18}}}", If(sItems = "", "", "," + If(AsCompact, "", vbCrLf + sIndent)), sName, PgID, If(fEnabled OrElse sChilds = "", sURL, ""), sIcon, Bool2JS(Not fEnabled), If(isSelected, ", ""selected"":true", ""), If(sChilds = "", "", String.Format(",""items"":[" + If(AsCompact, "", vbCrLf + sIndent + vbTab) + "{0}]", sChilds)), If(sExtra = "", "", ",""extra"":""" + sExtra + """"), If(sChilds <> "", ",""selectable"":false,""closeMenuOnClick"":false", ""), If(sTitle <> "" AndAlso sTitle <> sName, JS_SafeString(sTitle), ""), If(sHotkey = "", "", String.Format(",""hotkey"":""{0}""", JS_SafeString(GetHotkeyText(sHotkey)))), If(sPosition = "", "", ",""position"":""" + sPosition + """"), JS_SafeString(sHelp), JS_SafeString(sHash), If(fShowDraft AndAlso sDesc = "", "", ", ""desc"":""" + JS_SafeString(sDesc) + """"), JS_SafeString(ParentUID), If(sJS = "", "", String.Format(",""js"":""{0}""", JS_SafeString(sJS))), If(sHID = "", "", String.Format(",""hid"":""{0}""", sHID))).Replace(vbLf, " ").Replace(vbCr, " ").Replace("  ", " ").Trim ' D4642 + D4645 + D4658 + D4660 + D4668 + D4705 + D4767 + D4865 + D4917 + D5043 + D5059 + D5064 + D6101 + D6221 + D7253 + D7343

                            ' D4710 ===
                            If App.isAuthorized AndAlso App.HasActiveProject AndAlso App.Options.EvalSiteURL <> "" AndAlso (PgID = _PGID_EVALUATION OrElse PgID = _PGID_TEAMTIME_STATUS) AndAlso Not isAdmin Then   ' D6023
                                Dim sEvalURL As String = ""
                                Select Case PgID
                                    Case _PGID_EVALUATION
                                        If If(App.isRiskEnabled, App.Options.EvalURL4Anytime_Riskion, App.Options.EvalURL4Anytime) Then sEvalURL = _Page.PageURL(_PGID_SERVICEPAGE, String.Format("{0}={1}", _PARAM_ACTION, "eval_anytime"))    ' D6016 + D6020
                                    Case _PGID_TEAMTIME_STATUS
                                        If If(App.isRiskEnabled, App.Options.EvalURL4TeamTime_Riskion, App.Options.EvalURL4TeamTime) Then sEvalURL = _Page.PageURL(_PGID_SERVICEPAGE, String.Format("{0}={1}", _PARAM_ACTION, "eval_teamtime"))  ' D6016 + D6020
                                End Select
                                If sEvalURL <> "" Then
                                    'If _Page.CurrentPageID <> _PGID_EVALUATION AndAlso Str2Bool(WebConfigOption(_OPT_EVAL_SITE_ONLY)) Then  ' D6260
                                    Dim pgID_ As Integer = PgID     ' D4938 + D5059 + D6359
                                    If App.Options.EvalSiteOnly Then  ' D6260 + D6359
                                        sNodeItem = ""
                                        If sHotkey <> "" AndAlso HotKeys.ContainsKey(sHotkey) Then HotKeys(sHotkey) = pgID_ ' D6255
                                    Else    ' D6255
                                        pgID_ = _PGID_MAX_MOD * 10 + PgID   ' D6359
                                        sName += " " + JS_SafeString(ResString("lblResponsive"))
                                        sTitle += " " + JS_SafeString(ResString("lblResponsive"))
                                    End If
                                    sExtra += If(sExtra = "", "", ";") + "eval"
                                    sNodeItem += String.Format("{0}{{""text"":""{1}"",""pgID"":{2},""url"":""{3}"",""icon"":""{4}"",""title"":""{10}"",""disabled"":{5},""help"":""{13}"",""uid"":""{14}"",""puid"":""{15}""{8}{9}{11}{12}{7}{16}}}", If(sNodeItem = "" AndAlso sItems = "", "", "," + If(AsCompact, "", vbCrLf + sIndent)), sName, pgID_, If(fEnabled OrElse sChilds = "", sEvalURL, ""), sIcon, Bool2JS(Not fEnabled), "", If(sChilds = "", "", String.Format(",""items"":[" + If(AsCompact, "", vbCrLf + sIndent + vbTab) + "{0}]", sChilds)), If(sExtra = "", "", ",""extra"":""" + sExtra + """"), If(sChilds <> "", ",""selectable"":false,""closeMenuOnClick"":false", ""), If(sTitle <> "" AndAlso sTitle <> sName, JS_SafeString(sTitle), ""), If(sHotkey = "", "", String.Format(",""hotkey"":""{0}""", JS_SafeString(GetHotkeyText(sHotkey)))), If(sPosition = "", "", ",""position"":""" + sPosition + """"), JS_SafeString(sHelp), JS_SafeString(sHash), JS_SafeString(ParentUID), If(sHID = "", "", String.Format(",""hid"":""{0}""", sHID)))   ' D4642 + D4645 + D4658 + D4660 + D4668 + D4705 + D4817 + D4865 + D4938 + D5043 + D5059
                                    ItemsCount += 1
                                End If
                            End If
                            ' D4710 ==

                            If sNodeItem <> "" Then
                                sItems += sNodeItem
                                ItemsCount += 1 ' D4661
                            End If

                        End If
                        ' D4649 ==
                    End If
                Next
                'sw.Stop()
                'If Not IsPostBack Then Debug.Print("LoadMenuItems(): " + sw.ElapsedMilliseconds.ToString + " ms")
            End If
            Return sItems
        End Function

        Private Function GetPageUID(PgID As Integer, sName As String) As String
            Return If(PgID > 0, PgID.ToString, (_PGID_MAX_MOD * 100 + (CRC32.Compute(sName) Mod _PGID_MAX_MOD * 100)).ToString).Trim.ToLower  ' D5064
        End Function
        ''' <summary>
        ''' Get list of available workgroups in form of JSON string
        ''' </summary>
        ''' <returns>JSON string</returns>
        Public Function GetWorkgroupsJSON() As String
            Dim sList As String = ""
            If App.ActiveUser IsNot Nothing Then
                Dim tGroups As List(Of clsWorkgroup) = App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups)
                If tGroups IsNot Nothing Then
                    ' D4871 ===
                    Dim fDisableAll As Boolean = False
                    Dim sError As String = ""
                    If Not App.CheckLicense(App.SystemWorkgroup, sError, True) Then ' D6568
                        fDisableAll = True
                        'tGroups.Clear()
                        'If App.ActiveUser.CannotBeDeleted AndAlso App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageOwnWorkgroup, App.ActiveUser.UserID) Then
                        '    tGroups.Add(App.SystemWorkgroup)
                        'End If
                    End If
                    ' D4871 ==
                    For Each tGrp As clsWorkgroup In tGroups
                        Dim sMsg As String = "" ' D4955
                        Dim tStatus As Integer = If(tGrp.Status = ecWorkgroupStatus.wsSystem, 2, 0)
                        If App.isStartupWorkgroup(tGrp) Then tStatus = 3
                        Dim fDisabled As Boolean = False
                        If tGrp.License IsNot Nothing AndAlso tGrp.License.isValidLicense() Then
                            If tStatus = 0 AndAlso tGrp.License.CheckParameterByID(ecLicenseParameter.RiskEnabled) Then tStatus = 1
                            fDisabled = tGrp.Status = ecWorkgroupStatus.wsDisabled OrElse Not App.isWorkgroupAvailable(tGrp, App.ActiveUser, clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, tGrp.ID, App.UserWorkgroups))
                            If fDisabled Then sMsg = ResString("lblWorkgroupeDisabled")    ' D4955
                        Else
                            fDisabled = True
                            sMsg = ResString("lblLicenseInvalid")   ' D4955
                        End If
                        If fDisableAll Then fDisabled = tGrp.Status <> ecWorkgroupStatus.wsSystem OrElse Not App.ActiveUser.CannotBeDeleted ' D4871
                        If Not fDisabled AndAlso tStatus = 1 AndAlso tGrp.ID <> App.ActiveWorkgroup.ID Then ' D4997
                            If Not OPT_ALLOW_RISKION_WORKGROUPS AndAlso Not Request.IsLocal Then fDisabled = True
                        End If
                        Dim fExpired As Boolean = Not App.CheckLicense(tGrp, sMsg, True)    ' D4955
                        If OPT_SHOW_EXPIRED_WORKGROUPS OrElse (App.ActiveUser IsNot Nothing AndAlso App.ActiveUser.CannotBeDeleted AndAlso tGrp.Status = ecWorkgroupStatus.wsSystem) OrElse (Not fExpired AndAlso Not fDisabled) Then ' D4955 + D6568
                            sList += String.Format("{0}{{""id"":{1},""title"":""{2}"",""status"":{3},""enabled"":{4},""expired"":{5},""comment"":""{6}""}}", If(sList = "", "", ","), tGrp.ID, JS_SafeString(tGrp.Name), tStatus, Bool2JS(Not fDisabled), Bool2JS(fExpired), JS_SafeString(sMsg))  ' D4955 + D4993
                        End If
                    Next
                End If
            End If
            Return sList
        End Function

        Public Function GetParsedTemplates() As String
            Dim Lines As String = ""
            Dim sEvalURL As String = App.Options.EvalSiteURL    ' D4924
            App.Options.EvalSiteURL = ""    ' D4924
            Dim AllTemplates As List(Of String) = _TEMPL_LIST_ALL(App.isRiskEnabled).Concat(If(App.isRiskEnabled, _TEMPL_LIST_EVALS_RISK, _TEMPL_LIST_EVALS)).Concat({_TEMPL_MODEL, _TEMPL_MODELS, _TEMPL_PROJECT, _TEMPL_PROJECTS}).ToList ' D6356
            For Each sTpl As String In AllTemplates
                If sTpl = _TEMPL_URL_APP OrElse sTpl = _TEMPL_URL_MEETINGID OrElse (Not sTpl.StartsWith("%%url_") AndAlso Not sTpl.StartsWith(_TEMPL_PAGE_PREFIX) AndAlso sTpl <> _TEMPL_USERPSW) Then ' -D4924 + D4936 + D6456
                    Dim sVal As String = _Page.ParseString(sTpl)
                    If sTpl <> sVal Then Lines += String.Format("{0}""{1}"":""{2}""", If(Lines = "", "", ","), JS_SafeString(sTpl.Replace("%%", "")), JS_SafeString(sVal))
                End If
            Next
            App.Options.EvalSiteURL = sEvalURL  ' D4924
            Lines = String.Format("var _templates={{{0}}};", Lines)
            Return Lines
        End Function

        ' D4660 ===
        Public Function GetHotKeysCode() As String
            Dim sCode As String = ""
            If HotKeys IsNot Nothing Then
                For Each sKey As String In HotKeys.Keys
                    Dim sIf As String = _Page.ParseHotKeyString(sKey)
                    If sIf <> "" Then sCode += String.Format("            if ({0}) {{ {1} }} // {2}" + vbCrLf, sIf, String.Format("onOpenPage({0});", HotKeys(sKey)), sKey)
                Next
            End If
            'If sCode <> "" Then sCode = vbTab + "$(document).keyup(function(e) {" + vbCrLf + sCode + "   });"
            Return sCode
        End Function
        ' D4660 ==

        ' D4672 ===
        Private Function GetHotkeyText(sHotkey As String) As String
            Return sHotkey.Replace("Ctrl", "Ctrl+").Replace("Alt", "Alt+").Replace("Shift", "Shift+")
        End Function

        Public Function GetHotKeysList() As String
            Dim sLst As String = ""
            If HotKeys IsNot Nothing Then
                For Each sKey As String In HotKeys.Keys
                    ' D6995 ===
                    Dim sTitle As String = ""
                    Dim tmpPg As clsPageAction = _Page.PageByID(HotKeys(sKey))
                    If tmpPg IsNot Nothing AndAlso App.CurrentLanguage.Resources.ParameterExists(_PGHOTKEY_PREFIX + tmpPg.Name) Then
                        sTitle = ResString(_PGHOTKEY_PREFIX + tmpPg.Name)
                        If Not String.IsNullOrEmpty(sTitle) Then sTitle = String.Format(",""title"":""{0}""", sTitle)
                    End If
                    sLst += String.Format("{0}{{""code"":""{1}"",""page"":""{2}""{3}}}", If(sLst = "", "", ","), JS_SafeString(GetHotkeyText(sKey)), JS_SafeString(HotKeys(sKey)), sTitle)
                    ' D6995 ==
                Next
            End If
            Return sLst
        End Function
        ' D4672 ==

        ' D4668 ===
        Public Function GetHelpPage() As String
            Dim sRes As String = App.ResString(String.Format("help_{0}", _Page.NavigationPageID), True)
            If sRes.StartsWith("help_") Then sRes = ""
            Return sRes
        End Function

        Public Function ShowChatSupport() As Boolean
            Return (WebOptions.ShowChatSupport(Request Is Nothing OrElse Not Request.IsLocal) AndAlso App.isAuthorized)
        End Function
        ' D4668 ==
        ''' <summary>
        ''' Get list of supported languages
        ''' </summary>
        ''' <returns>JSON string</returns>
        Public Function GetLanguages() As String
            Dim sList As String = ""
            For Each tLang As clsLanguageResource In App.Languages
                sList += String.Format("{0}{{""code"":""{1}"",""title"":""{2}""}}", If(sList = "", "", ","), JS_SafeString(tLang.LanguageCode), JS_SafeString(tLang.LanguageName))
            Next
            Return sList
        End Function

        ' D4954 ===
        Public Function HasCSData() As Boolean
            If App.HasActiveProject Then
                Return App.ActiveProject.ProjectManager.HasCollaborativeStructuringData(App.ActiveProject.ID)
            End If
            Return False
        End Function
        ' D4954 ==

        Public Sub FetchWrongObject(Optional ReplyAsJSON As Boolean = True, Optional Message As String = "NO_OBJECT_FOUND")
            _Page.FetchWithCode(HttpStatusCode.BadRequest, ReplyAsJSON, Message)
        End Sub

        ' D6552 ===
        Public Sub FetchNotAllowedSSO(Optional ReplyAsJSON As Boolean = True, Optional Message As String = "NOT_ALLOWED_DUE_TO_USE_SSO")
            _Page.FetchWithCode(HttpStatusCode.Forbidden, ReplyAsJSON, Message)
        End Sub
        ' D6552 ==

    End Class

End Namespace