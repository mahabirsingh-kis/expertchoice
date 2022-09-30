
Partial Class SendMailPage
    Inherits clsComparionCorePage

    Private _UsersList As List(Of clsApplicationUser) = Nothing

    Public Sub New()
        MyBase.New(_PGID_SEND_MAIL)
    End Sub

    Public ReadOnly Property isProjectUsers As Boolean
        Get
            Return App.HasActiveProject AndAlso CheckVar("mode", "").ToLower = "prj"
        End Get
    End Property

    ' D3589 ===
    Public Function WorkgroupPath() As String
        Return String.Format("{0}Workgroup_{1}\", _FILE_MHT_FILES, App.ActiveWorkgroup.ID)
    End Function
    ' D3589 ==

    Public Property UsersList As List(Of clsApplicationUser)
        Get
            If _UsersList Is Nothing Then
                _UsersList = New List(Of clsApplicationUser)
                Dim sIDList As String() = EcSanitizer.GetSafeHtmlFragment(CheckVar("lst", "")).Split(CChar(",")) ' Anti-XSS 'A1514
                Dim tAllUsers As List(Of clsApplicationUser)
                If isProjectUsers Then tAllUsers = App.DBUsersByProjectID(App.ProjectID) Else tAllUsers = App.DBUsersByWorkgroupID(App.ActiveWorkgroup.ID)
                If CheckVar("lst", "").ToLower = "all" Then
                    _UsersList = tAllUsers
                Else
                    For Each sID As String In sIDList
                        Dim ID As Integer
                        If Integer.TryParse(sID, ID) Then
                            Dim tUser As clsApplicationUser = clsApplicationUser.UserByUserID(ID, tAllUsers)
                            If tUser IsNot Nothing Then _UsersList.Add(tUser)
                        End If
                    Next
                End If
            End If
            Return _UsersList
        End Get
        Set(value As List(Of clsApplicationUser))
            _UsersList = value
        End Set
    End Property

    Public Function GetUserEmail(tUser As clsApplicationUser) As String
        Dim sUser As String = ""
        If tUser IsNot Nothing Then
            sUser = tUser.UserEmail
            If tUser.UserName <> "" AndAlso tUser.UserName <> tUser.UserEmail Then sUser = String.Format("{0} <{1}>", tUser.UserName, sUser)
        End If
        Return sUser
    End Function

    Public Function GetSenders() As String
        ' D3581 ===
        'Dim sLst As String = ""

        'Dim fActiveValid As Boolean = isValidEmail(App.ActiveUser.UserEmail)
        'sLst = String.Format("<option id='{0}'{1}>{2}</option>", SafeFormString(App.ActiveUser.UserEmail), IIf(fActiveValid, " selected", ""), SafeFormString(GetUserEmail(App.ActiveUser)))
        'sLst += String.Format("<option id='{0}'{1}>{2}</option>", SafeFormString(WebOptions.SupportEmail), IIf(fActiveValid, "", " selected"), SafeFormString(WebOptions.SupportEmail))

        'sLst = "<select id='tbFrom' style='width:100%'>" + sLst + "</select>"
        'Return sLst
        Return String.Format("<input type='text' style='width:100%' disabled  value='{0}'>", SafeFormString(SupportEmail))
        ' D3581 ==
    End Function

    Public Function GetRecepients() As String
        Dim sLst As String = ""
        If UsersList.Count = 0 Then
            sLst = "<span class='error'>No users selected</span>"
        Else
            If CheckVar("lst", "").ToLower = "all" Then
                sLst = String.Format("<i>All {0} participants</i>", IIf(isProjectUsers, "project", "workgroup"))
            Else
                If UsersList.Count > 3 Then
                    sLst = String.Format("<div id='divList'><a href='' onclick='ShowList(true); return false;' class='actions dashed'>{0} participant(s)</a></div>", UsersList.Count)
                    sLst += "<div id='divListAll' style='display:none;'><div style='border:1px solid #f0f0f0; padding:2px 4px; height:5em; overflow-y:auto;'>"
                End If

                For Each tUser As clsApplicationUser In UsersList
                    sLst += String.Format("<div>&middot; {0}</div>", HTMLUserLinkEmail(tUser, CStr(IIf(isValidEmail(tUser.UserEmail), "", " class='error'"))))  ' D3583
                Next

                If UsersList.Count > 3 Then
                    sLst += "</div><a href='' onclick='ShowList(false); return false;' class='actions dashed small'>Collapse list</a></div>"
                End If
            End If
        End If
        Return sLst
    End Function

    ' D3580 ===
    Public Property CustomSubject() As String
        Get
            If isProjectUsers Then
                Return CStr(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_CUSTOM_EMAIL_ID, CUSTOM_EMAIL_SUBJ_ID, Guid.Empty))
            Else
                'Dim tExtra As clsExtra = App.DBExtraRead(clsExtra.Params2Extra(App.ActiveWorkgroup.ID, ecExtraType.Workgroup, ecExtraProperty.EmailSubject, ""))   ' -D3589
                'If tExtra IsNot Nothing Then Return tExtra.Value Else Return "" ' -D3589
                Return File_GetContent(WorkgroupPath() + _FILE_WKG_EMAIL_SUBJ, "")  ' D3589
            End If
        End Get
        Set(value As String)
            If isProjectUsers Then
                With App.ActiveProject.ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_CUSTOM_EMAIL_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, CUSTOM_EMAIL_SUBJ_ID, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            Else
                'App.DBExtraWrite(clsExtra.Params2Extra(App.ActiveWorkgroup.ID, ecExtraType.Workgroup, ecExtraProperty.EmailSubject, value.Substring(0, 1999)))    ' -D3589
                File_CreateFolder(WorkgroupPath)
                MyComputer.FileSystem.WriteAllText(WorkgroupPath() + _FILE_WKG_EMAIL_SUBJ, value, False)   ' D3589
            End If
        End Set
    End Property

    Public Property CustomBody() As String
        Get
            If isProjectUsers Then
                Return CStr(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_CUSTOM_EMAIL_ID, CUSTOM_EMAIL_BODY_ID, Guid.Empty)) 'A1514
            Else
                'Dim tExtra As clsExtra = App.DBExtraRead(clsExtra.Params2Extra(App.ActiveWorkgroup.ID, ecExtraType.Workgroup, ecExtraProperty.EmailBody, ""))  ' -D3589
                'If tExtra IsNot Nothing Then Return tExtra.Value Else Return ""    ' -D3589
                Return File_GetContent(WorkgroupPath() + _FILE_WKG_EMAIL_BODY, "")  ' D3589
            End If
        End Get
        Set(value As String)
            If isProjectUsers Then
                With App.ActiveProject.ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_CUSTOM_EMAIL_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, CUSTOM_EMAIL_BODY_ID, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            Else
                'App.DBExtraWrite(clsExtra.Params2Extra(App.ActiveWorkgroup.ID, ecExtraType.Workgroup, ecExtraProperty.EmailBody, value.Substring(0, 1999)))    ' -D3589
                File_CreateFolder(WorkgroupPath)
                MyComputer.FileSystem.WriteAllText(WorkgroupPath() + _FILE_WKG_EMAIL_BODY, value, False)  ' D3589
            End If
        End Set
    End Property

    Private Sub Ajax_Callback()
        'Dim args As NameValueCollection = Page.Request.QueryString

        Dim sResult As String = ""
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).ToLower  ' Anti-XSS 'Sanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_ACTION)).ToLower  ' D3583 + Anti-XSS

        Dim tmp As String = App.Options.EvalSiteURL ' D3901
        App.Options.EvalSiteURL = ""    ' D3901

        Select Case sAction

            Case "save"
                CustomSubject = EcSanitizer.GetSafeHtmlFragment(CheckVar("subj", ""))    ' D3583 + Anti-XSS
                CustomBody = EcSanitizer.GetSafeHtmlFragment(CheckVar("body", ""))       ' D3583 + Anti-XSS
                'CustomSubject = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "subj"))  ' Anti-XSS
                'CustomBody = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "body")) ' Anti-XSS
                sResult = "OK"

                ' D3581 ===
            Case "send"
                Dim sSubj As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("subj", ""))  ' D3583 + Anti-XSS
                Dim sBody As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("body", ""))  ' D3583 + Anti-XSS
                'Dim sSubj As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "subj"))    ' Anti-XSS
                'Dim sBody As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "body"))    ' Anti-XSS
                CustomSubject = sSubj
                CustomBody = sBody
                If sBody <> "" AndAlso UsersList.Count > 0 Then

                    Dim sErrorsList As String = ""
                    Dim tPrj As clsProject = Nothing
                    If isProjectUsers AndAlso App.HasActiveProject Then tPrj = App.ActiveProject

                    Dim tErrors As Integer = 0
                    For Each tUser As clsApplicationUser In UsersList
                        Dim sError As String = ""
                        If Not SendMail(SupportEmail, tUser.UserEmail, ParseAllTemplates(sSubj, tUser, tPrj), ParseAllTemplates(sBody, tUser, tPrj), sError, "", False, SMTPSSL) OrElse sError <> "" Then tErrors += 1
                        If sError <> "" Then
                            If sErrorsList.Length < 500 Then sErrorsList += String.Format("{0}{1}: {2}", IIf(sErrorsList = "", "", "<br>"), tUser.UserEmail, sError) Else If Not sErrorsList.EndsWith("...") Then sErrorsList += "<br>..."
                        End If
                    Next

                    sResult = String.Format("Sent {0} e-mail(s).", UsersList.Count)
                    If tErrors > 0 Then
                        sResult += String.Format("Errors: {0}", If(sErrorsList = "", CStr(tErrors), String.Format("<a href='' onclick='$(""#divErrors"").show(); return false;' class='actions dashed'>{0}</a>", tErrors))) 'A1514
                        If sErrorsList <> "" Then sResult += String.Format("<p class='text small' id='divErrors' style='display:none'><b>Errors:</b><br>{0}</p>", sErrorsList)
                    End If

                    App.DBSaveLog(dbActionType.actSendEmail, If(isProjectUsers, dbObjectType.einfProject, dbObjectType.einfWorkgroup), If(isProjectUsers, App.ProjectID, App.ActiveWorkgroup.ID), "Send custom e-mail", sResult)
                End If

            Case "preview"
                Dim sSubj As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("subj", ""))  ' D3583 + Anti-XSS
                Dim sBody As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("body", ""))  ' D3583 + Anti-XSS
                'Dim sSubj As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "subj"))    ' Anti-XSS
                'Dim sBody As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "body"))    ' Anti-XSS
                Dim tPrj As clsProject = Nothing
                If isProjectUsers AndAlso App.HasActiveProject Then tPrj = App.ActiveProject

                sResult = String.Format("<div style='height:250px; overflow-y:auto; border:1px solid #f0f0f0; padding:1ex;'><b>From</b>: {0}<br><b>To</b>: {1}<br><b>Subj</b>: {2}<br><hr>{3}</div>", SupportEmail, GetUserEmail(App.ActiveUser), ParseAllTemplates(sSubj, App.ActiveUser, tPrj), ParseAllTemplates(sBody, App.ActiveUser, tPrj).Replace(vbLf, "").Replace(vbCr, "<br>"))    ' D3588
                ' D3581 ==

        End Select

        App.Options.EvalSiteURL = tmp   ' D3901

        If sResult <> "" Then
            RawResponseStart()
            Response.ContentType = "text/plain"
            Response.Write(sResult)
            Response.End()
        End If
    End Sub
    ' D3580 ==

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        CType(Master, clsComparionCoreMasterPopupPage).ShowButtonsLine = True
        ' D3580 ===
        '-A1514 pnlLoadingPanel.Caption = ResString("msgLoading")
        '-A1514 pnlLoadingPanel.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "process.gif")
        If isAjax() Then Ajax_Callback()
        ' D3580 ==        
    End Sub

    Public Function GetTooltipHints() As String 'A1514
        ' D3581 ===
        Dim sTooltip As String = "<ul type=square style='margin:0px 2em 1ex 1.5em; text-align: left;'>" 'A1523
        Dim tLst As String() = _TEMPL_LIST_ALL(App.isRiskEnabled)
        For i As Integer = 0 To tLst.Length - 1   ' D2467
            If tLst(i) <> _TEMPL_MEETING_ID OrElse _MEETING_ID_AVAILABLE Then    ' D0395
                If isProjectUsers OrElse _TEMPL_LIST_NO_PRJ.Contains(tLst(i)) Then    ' D3583
                    If Not isSSO() OrElse tLst(i) <> _TEMPL_URL_RESETPSW Then   ' D6552
                        sTooltip += String.Format("<li><a href='' onclick='return InsertTemplate(""{2}"")' class='actions'>{0}</a>: {1}</li>", tLst(i), ResString(_TEMPL_LIST_RES(App.isRiskEnabled)(i)), JS_SafeString(tLst(i)))    ' D0221 + D2467
                    End If
                End If
            End If
        Next
        sTooltip += String.Format("</ul><div style='margin:1ex' class='text small gray'>{0}</div>", ResString("msgClick2Insert"))
        '-A1514 RadToolTipHints.Text = sTooltip
        ' D3581 ==
        Return sTooltip 'A1514
    End Function

End Class
