Partial Class WordingTemplatePage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_ADMIN_WORKGROUP_WORDING_TEMPLATE)
    End Sub

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_PAGE_ACTION, "")).Trim().ToLower()  ' Anti-XSS
        Ajax_Callback(Request.Form.ToString)
    End Sub

    ' -D7390 ===
    'ReadOnly Property SESSION_SHOW_CONFIRMATION As String
    '    Get
    '        Return String.Format("WordingTemplates_ShowConfirmation_{0}", App.ProjectID.ToString)
    '    End Get
    'End Property

    'Public Property ShowConfirmation() As Boolean
    '    Get
    '        Return String.IsNullOrEmpty(SessVar(SESSION_SHOW_CONFIRMATION)) OrElse SessVar(SESSION_SHOW_CONFIRMATION) = "1"
    '    End Get
    '    Set(value As Boolean)
    '        SessVar(SESSION_SHOW_CONFIRMATION) = CStr(IIf(value, "1", "0"))
    '    End Set
    'End Property
    ' -D7390 ==

    Private Function TemplateItemsContainsKey(TemplateItems As List(Of clsTemplateItem), tStrTemplate As String) As Boolean
        For Each item In TemplateItems
            If item.TemplateName = tStrTemplate Then Return True
        Next
        Return False
    End Function
    Private Function TemplateItemsContainsTitle(TemplateItems As List(Of clsTemplateItem), tStrTemplate As String) As Boolean
        For Each item In TemplateItems
            If item.Title = tStrTemplate Then Return True
        Next
        Return False
    End Function

    Private Function GetTemplateItem(Item As clsTemplateItem) As String
        Dim sPastTense As String = ""
        Dim sPastTempl As String = ""
        If Item.TemplateName = "%%control%%" Then
            For Each kvp As KeyValuePair(Of String, String) In App.ActiveWorkgroup.WordingTemplates
                If kvp.Key.ToLower = "%%controlled%%" Then
                    sPastTense = kvp.Value
                    sPastTempl = "controlled"
                End If
            Next
        End If
        Return String.Format("['{0}','{1}','{2}','{3}','{4}','{5}','{6}',{7}]", JS_SafeString(Item.Title), JS_SafeString(Item.TemplateName.Trim(CChar("%"))), JS_SafeString(Item.Singular), JS_SafeString(Item.PluralTemplateName.Trim(CChar("%"))), JS_SafeString(Item.Plural), JS_SafeString(sPastTempl), JS_SafeString(sPastTense), If(Item.IsHeader, "1", "0"))
    End Function

    Public Function GetTemplatesList() As String
        Dim TemplateItems As New List(Of clsTemplateItem)

        TemplateItems.Clear()
        If App.ActiveWorkgroup IsNot Nothing Then
            If App.isRiskEnabled Then
                TemplateItems.Add(New clsTemplateItem With {.IsHeader = True, .Title = ResString("lblWordingTemplatesHeader2")})
            End If
            For Each item As KeyValuePair(Of String, String) In App.ActiveWorkgroup.WordingTemplates
                If Not item.Key.ToLower.StartsWith("%%vulnerabilit") AndAlso item.Key.ToLower <> "%%controlled%%" Then
                    Dim tTemplatefound As Boolean = False
                    Dim tTemplate As clsTemplateItem = Nothing

                    For Each eItem In TemplateItems
                        If (eItem.TemplateName.Trim(CChar("%")) + "s" = item.Key.Trim(CChar("%"))) OrElse (eItem.TemplateName.Trim(CChar("%")) + "es" = item.Key.Trim(CChar("%"))) OrElse (eItem.TemplateName.Trim(CChar("%")).Trim(CChar("y")) + "ies" = item.Key.Trim(CChar("%"))) Then
                            tTemplatefound = True
                            tTemplate = eItem
                        End If
                    Next

                    If Not tTemplatefound Then
                        If TemplateItemsContainsKey(TemplateItems, "%%event%%") AndAlso TemplateItemsContainsKey(TemplateItems, "%%source%%") AndAlso TemplateItemsContainsKey(TemplateItems, "%%consequence%%") AndAlso TemplateItemsContainsKey(TemplateItems, "%%control%%") AndAlso Not TemplateItemsContainsTitle(TemplateItems, ResString("lblWordingTemplatesHeader3")) Then
                            TemplateItems.Add(New clsTemplateItem With {.IsHeader = True, .Title = ResString("lblWordingTemplatesHeader3")})
                        End If
                        Dim tTitle As String = ""
                        Select Case item.Key
                            Case "%%event%%"
                                tTitle = ResString("lblWordingTemplatesEvents")
                            Case "%%source%%"
                                tTitle = ResString("lblWordingTemplatesSources")
                            Case "%%consequence%%"
                                tTitle = ResString("lblWordingTemplatesConsequences")
                            Case "%%control%%"
                                tTitle = ResString("lblWordingTemplatesControls")
                            Case "%%likelihood%%"
                                tTitle = ResString("lblWordingTemplatesLikelihood")
                            Case "%%impact%%"
                                tTitle = ResString("lblWordingTemplatesImpact")
                            Case "%%risk%%"
                                tTitle = ResString("lblWordingTemplatesRisk")
                            Case "%%opportunity%%"
                                tTitle = ResString("lblWordingTemplatesOpportunities")
                            Case "%%vulnerability%%"
                                tTitle = ResString("lblWordingTemplatesVulnerability")
                            Case "%%objective%%"
                                tTitle = ResString("lblWordingTemplatesObjective")
                            Case "%%objectives%%"
                                tTitle = ResString("lblWordingTemplatesObjectives")
                            Case "%%alternative%%"
                                tTitle = ResString("lblWordingTemplatesAlternative")
                            Case "%%alternatives%%"
                                tTitle = ResString("lblWordingTemplatesAlternatives")
                        End Select
                        TemplateItems.Add(New clsTemplateItem With {.Title = tTitle, .TemplateName = item.Key, .Singular = item.Value})
                    Else
                        If tTemplate IsNot Nothing Then
                            tTemplate.Plural = item.Value
                            tTemplate.PluralTemplateName = item.Key
                        End If
                    End If
                End If
            Next
        End If

        Dim retVal As String = ""
        For Each item As clsTemplateItem In TemplateItems
            retVal += CStr(IIf(retVal = "", "", ", ")) + GetTemplateItem(item)
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Private Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)

        Dim tResult As String = ""
        Dim tAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_ACTION)).ToLower()  ' Anti-XSS

        If Not String.IsNullOrEmpty(tAction) Then

            Select Case tAction
                Case "save"
                    If App.ActiveWorkgroup IsNot Nothing AndAlso App.CanvasMasterDBVersion >= "0.9997" Then
                        Dim keys As New List(Of String)
                        For Each item As KeyValuePair(Of String, String) In App.ActiveWorkgroup.WordingTemplates
                            keys.Add(item.Key)
                        Next
                        For Each item As String In keys
                            Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, item.Trim(CChar("%")))).Trim()  ' Anti-XSS
                            App.ActiveWorkgroup.WordingTemplates(item) = sValue
                        Next

                        App.CheckWordingTemplates(App.ActiveWorkgroup.WordingTemplates, App.ActiveWorkgroup.License.isValidLicense AndAlso App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.RiskEnabled))  ' D4413
                        App.DBWorkgroupUpdate(App.ActiveWorkgroup, False, "Update wording templates", False)
                    End If

                    ' -D7390
                    'Dim show_confirm As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "show_confirm")).ToLower()    ' Anti-XSS
                    'If Not String.IsNullOrEmpty(show_confirm) Then ShowConfirmation = show_confirm = "1"

                    tResult = String.Format("['{0}']", JS_SafeString(tAction)) 'etTemplatesList()
            End Select

        End If

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

End Class

