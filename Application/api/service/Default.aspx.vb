Partial Class ServiceWebAPI
    Inherits clsComparionCorePage

    Public _OPT_WHATSNEW_MAXDAYS As Integer = 30        ' D6026
    Public _OPT_WHATSNEW_SHOWDATE As Boolean = False    ' D6586

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Public Function Version() As String
        Return App.GetCoreVersion.ToString
    End Function

    Public Function Session_State(Optional Hash As String = "", Optional ExtraParams As NameValueCollection = Nothing) As Object   ' D5027 + D6022
        Dim sContent As String = jSessionStatus.CreateFromBaseObject(App, Session).ToJSON ' D5063 + D7208
        ' -D7276 Disable it due to user kick off when edit some workgroups/etc under (Admin, System wkg manager)
        '' D7160 ===
        'Dim tmpPgID As Integer = -1
        'If Not Integer.TryParse(GetParam(ExtraParams, "pgid", True), tmpPgID) Then tmpPgID = -1
        'If App.HasActiveProject AndAlso tmpPgID > 0 Then
        '    Dim tPg As clsPageAction = PageByID(tmpPgID)
        '    If tPg IsNot Nothing AndAlso tPg.PageType <> ecPageAccessType.paOnlySystem Then ' D7273
        '        If Not HasPermission(tmpPgID, App.ActiveProject, CustomWorkgroupPermissions) Then   ' D7273
        '            sContent += String.Format(", ""msg"": ""{0}""", ResString("errNoPermissions"))
        '        End If
        '    End If
        'End If
        '' D7160 ==

        Dim sExtra As String = ""   ' D7208
        If ExtraParams IsNot Nothing Then
            Select Case GetParam(ExtraParams, "check", True).ToLower
                Case "projects"
                    If App.isAuthorized AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsEnabled Then
                        Dim LastSec As Integer = _DEF_PING_TIMEOUT
                        Dim sLast As String = GetParam(ExtraParams, "last", True)
                        Integer.TryParse(sLast, LastSec)
                        If LastSec < 1 Then LastSec = 1
                        Dim DT As DateTime = Now.AddSeconds(-LastSec - 3)   ' add extra seconds
                        ' D7166 ===
                        Dim sSQL = String.Format("SELECT p.* FROM {0} AS p LEFT JOIN {4} AS w ON p.ID=w.ProjectID WHERE p.{1}=? AND (p.{2}>=? OR p.{3}>=? OR p.Created>=? OR w.{5}>=? OR w.Created>=?)", clsComparionCore._TABLE_PROJECTS, clsComparionCore._FLD_PROJECTS_WRKGID, clsComparionCore._FLD_PROJECTS_LASTVISITED, clsComparionCore._FLD_PROJECTS_LASTMODIFY, clsComparionCore._TABLE_WORKSPACE, clsComparionCore._FLD_WORKSPACE_LASTMODIFY)
                        Dim tParams As New List(Of Object)
                        tParams.Add(App.ActiveWorkgroup.ID)
                        tParams.Add(DT)
                        tParams.Add(DT)
                        tParams.Add(DT)
                        tParams.Add(DT)
                        tParams.Add(DT)
                        ' D7166 ==
                        Dim Data As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(sSQL, tParams)
                        If Data IsNot Nothing Then
                            App.Workspaces = Nothing    ' D7165
                            Dim tLst As New List(Of clsProject)
                            For Each tRow As Dictionary(Of String, Object) In Data
                                Dim tProject As clsProject = App.DBParse_Project(tRow)
                                tLst.Add(tProject)
                                ' D7165 ===
                            Next
                            tLst = App.CheckProjectsList(App.ActiveUser, App.ActiveWorkgroup, tLst, App.ActiveUserWorkgroup, App.Workspaces)
                            For Each tProject As clsProject In tLst
                                ' D7165 ==
                                Dim tLstPrj As clsProject = clsProject.ProjectByID(tProject.ID, App.ActiveProjectsList)
                                If tLstPrj Is Nothing Then
                                    App.ActiveProjectsList.Add(tProject)
                                Else
                                    Dim idx As Integer = App.ActiveProjectsList.IndexOf(tLstPrj)
                                    If idx >= 0 Then App.ActiveProjectsList(idx) = tProject
                                End If
                            Next
                            ' D7166 ===
                            ' check the actual projects list and cached in the memory
                            Dim curPrjList As List(Of clsProject) = App.CheckProjectsList(App.ActiveUser, App.ActiveWorkgroup, App.ActiveProjectsList, App.ActiveUserWorkgroup, App.Workspaces)
                            If curPrjList.Count <> App.ActiveProjectsList.Count Then
                                ' check which projects are missing now
                                For i As Integer = App.ActiveProjectsList.Count - 1 To 0 Step -1
                                    Dim prjID As Integer = App.ActiveProjectsList(i).ID
                                    If clsProject.ProjectByID(prjID, curPrjList) Is Nothing Then
                                        If clsProject.ProjectByID(prjID, tLst) Is Nothing Then tLst.Add(clsProject.ProjectByID(prjID, App.ActiveProjectsList)) ' add this model for update properties (disabled, removed, etc)
                                        App.ActiveProjectsList.RemoveAt(i)  ' remove from  projects list since this is not available for now
                                        If prjID = App.ProjectID Then App.ProjectID = -1
                                    End If
                                Next
                            End If
                            ' D7166 ==
                            Dim onlineSess As List(Of clsOnlineUserSession) = App.DBOnlineSessions
                            For Each tSess As clsOnlineUserSession In onlineSess
                                If tSess.WorkgroupID = App.ActiveWorkgroup.ID AndAlso tSess.UserID <> App.ActiveUser.UserID AndAlso tSess.ProjectID > 0 Then
                                    If clsProject.ProjectByID(tSess.ProjectID, tLst) IsNot Nothing Then ' D7165
                                        tLst.Add(clsProject.ProjectByID(tSess.ProjectID, App.ActiveProjectsList))
                                    End If
                                End If
                            Next
                            If tLst.Count > 0 Then
                                Dim jLst As List(Of jProject) = jProject.GetProjectsList(App, tLst)
                                If jLst IsNot Nothing AndAlso jLst.Count > 0 Then
                                    sExtra += String.Format(",""projects"":{0}", JsonConvert.SerializeObject(jLst))  ' D7208
                                End If
                            End If
                        End If
                    End If
            End Select
        End If

        Dim sHash As String = GetMD5(sContent)
        sContent = If(sHash = Hash, "", sContent.Substring(1, sContent.Length - 2) + sExtra) ' Cut { } for attach to other string    ' D7208

        sContent = String.Format("{{""hash"":{0}{1}{2}}}", JsonConvert.SerializeObject(sHash), If(sContent = "", "", ","), sContent)
        Return sContent ' D5027
    End Function

    ' D5063 ===
    Private Function Application_State() As jActionResult
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .Data = GetApplicationStatus()
            }
    End Function
    ' D5063 ==

    ' D5049 ===
    Public Function WhatsNew(FullList As Boolean) As jActionResult
        FetchIfNotAuthorized()
        Dim sError As String = ""
        Dim News As String = ""
        Dim sToday As String = Date.Now().ToString("yyyy-MM-dd")
        Dim sLastShow As String = GetCookie(_COOKIE_LAST_NEWS, "")
        If FullList Then sLastShow = ""
        If String.IsNullOrEmpty(sLastShow) OrElse sLastShow < sToday Then
            If MyComputer.FileSystem.FileExists(_FILE_ROOT + _FILE_WHATS_NEW) Then
                Dim NewsLines As String() = File_GetContent(_FILE_ROOT + _FILE_WHATS_NEW, sError).Split(CChar(vbNewLine))
                If String.IsNullOrEmpty(sError) Then
                    Dim Cnt As Integer = 0  ' D5048
                    For Each sLine As String In NewsLines
                        If sLine.Trim <> "" Then
                            Dim idx As Integer = sLine.IndexOfAny({CChar(vbTab), CChar(" ")})
                            If idx > 0 Then
                                Dim sDate As String = sLine.Substring(0, idx).Trim
                                Dim sText As String = sLine.Substring(idx + 1).Trim
                                ' D6026 ===
                                If Not FullList Then
                                    Dim DT As Date
                                    If Date.TryParse(sDate, DT) Then
                                        If DT < Now.AddDays(-_OPT_WHATSNEW_MAXDAYS) Then Exit For
                                    End If
                                End If
                                ' D6026 ==
                                If sText <> "" AndAlso sDate <> "" AndAlso sDate > sLastShow Then
                                    News += String.Format("<li>{0}{1}</li>", If(_OPT_WHATSNEW_SHOWDATE, "<div class='text small gray'>" + sDate + ":</div>", ""), sText) ' D6586
                                End If
                            End If
                        End If
                        Cnt += 1    ' D5048
                        If Cnt > If(FullList, 20, 8) Then Exit For ' D5048
                    Next
                    If News <> "" Then
                        News = String.Format("<ul type='square' class='text ul-dialog'>{0}</ul>", News)
                        If Not FullList Then SetCookie(_COOKIE_LAST_NEWS, sToday)
                    End If
                End If
            End If
        End If
        Return New jActionResult With {
            .Result = If(String.IsNullOrEmpty(sError), ecActionResult.arSuccess, ecActionResult.arError),
            .Data = News,
            .Message = sError
        }
    End Function
    ' D5049 ==

    ' D6028 ===
    Public Function KnownIssues() As jActionResult
        FetchIfNotAuthorized()
        Dim sError As String = ""
        Dim Text As String = ""
        ' D6956 ===
        Dim sFName As String = _FILE_ROOT + If(App.isRiskEnabled, _FILE_KNOWN_ISSUES_RISKION, _FILE_KNOWN_ISSUES)
        If MyComputer.FileSystem.FileExists(sFName) Then
            Text = File_GetContent(sFName, sError)
            ' D6956 ==
            Dim sToday As String = Date.Now().ToString("yyyy-MM-dd")    ' D6034
            SetCookie(_COOKIE_LAST_ISSUES, sToday)  ' D6034
        End If
        Return New jActionResult With {
            .Result = If(String.IsNullOrEmpty(sError), ecActionResult.arSuccess, ecActionResult.arError),
            .Data = Text,
            .Message = sError
        }
    End Function
    ' D6028 ==

    ' D6461 ===
    Public Function Upload() As jActionResult
        Dim tRes As New jActionResult
        Dim File As HttpPostedFile = Nothing
        Dim DocMedia As Boolean = App.HasActiveProject AndAlso GetParam(_Page.Params, "dest", True).ToLower = "docmedia"
        If Request IsNot Nothing AndAlso Request.Files.Count > 0 Then
            tRes.Result = ecActionResult.arSuccess
            For i As Integer = 0 To Request.Files.Count - 1
                Dim tFile As HttpPostedFile = Request.Files(i)
                Dim destName As String = ""
                If DocMedia Then
                    If Infodoc_Prepare(App.ProjectID, ECHierarchyID.hidLikelihood, reObjectType.Uploads, "", tRes.Message) Then
                        destName = String.Format("{0}\{1}", Infodoc_Path(App.ProjectID, ECHierarchyID.hidLikelihood, reObjectType.Uploads, "", -1), SafeFileName(tFile.FileName))
                        tRes.URL = String.Format("{0}{1}", Infodoc_URL(App.ProjectID, ECHierarchyID.hidLikelihood, reObjectType.Uploads, "", -1), SafeFileName(tFile.FileName))
                    Else
                        tRes.Message = "Unable to create a folder for file uploading"
                    End If
                    If Not String.IsNullOrEmpty(tRes.Message) Then tRes.Result = ecActionResult.arError
                Else
                    destName = File_CreateTempName()
                    tRes.URL = MyComputer.FileSystem.GetName(destName)
                End If
                tFile.SaveAs(destName)
                ' D6462 ===
                tRes.Data = tFile.ContentLength
                If Str2Bool(GetParam(_Page.Params, "base64", True)) Then
                    Dim tContent As Byte() = File_GetContentFromMIME(destName, tRes.Message)
                    MyComputer.FileSystem.WriteAllBytes(destName, tContent, False)
                    tRes.Data = tContent.Length
                End If
                ' D6462 ==
                If tRes.Result <> ecActionResult.arSuccess Then Exit For
            Next
        Else
            tRes.Result = ecActionResult.arError
            tRes.Message = "No data for upload"
        End If
        Return tRes
    End Function
    ' D6461 ==

    Private Sub ServiceWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        Select Case _Page.Action

            Case "version"
                ' D7442 ===
                Response.ClearContent()
                Response.ContentType = "text/plain"
                Response.Write(Version())
                Response.End()
                ' D7442 ==

            Case "session_state"
                _Page.ResponseData = CStr(Session_State(GetParam(_Page.Params, "hash", True), _Page.Params))  ' D5027 + D6022

            Case "whatsnew" ' D5049
                _Page.ResponseData = WhatsNew(Str2Bool(GetParam(_Page.Params, "full", True)))   ' D5049

            Case "knownissues" ' D6028
                _Page.ResponseData = KnownIssues()  ' D6028

            Case "application_state"
                _Page.ResponseData = Application_State() ' D5063

                ' D6461 ===
            Case "upload"
                _Page.ResponseData = Upload()
                ' D6461 ==

        End Select
    End Sub

End Class