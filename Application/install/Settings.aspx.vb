Partial Class SystemSettingsPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_SYSTEM_SETTINGS)
    End Sub

    Public Function GetSettings(AppSettings As Boolean) As String
        Dim sRes = ""
        If _Options_Individual IsNot Nothing Then

            ' D7230 ===
            Dim DBSettings As New List(Of Integer)
            Dim tExtras As List(Of clsExtra) = App.DBExtrasByDetails(, ecExtraType.Common, ecExtraProperty.WebConfigSetting)
            If tExtras IsNot Nothing Then
                For Each tExtra As clsExtra In tExtras
                    If tExtra.Value IsNot Nothing Then
                        DBSettings.Add(tExtra.ObjectID)
                    End If
                Next
            End If
            ' D7230 ==

            For Each tOpt As clsSetting In _Options_Individual
                If tOpt.isAppSetting = AppSettings Then
                    Dim sOpt As String = ""
                    Dim sName As String = ResString(tOpt.ResName, True)
                    Dim tValue As String = tOpt.Value   ' D6689
                    If tOpt.ConfigName = _OPT_FIPS_MODE Then
                        Dim CSet As Boolean = _OPT_USE_CUSTOM_SETTINGS
                        _OPT_USE_CUSTOM_SETTINGS = False
                        tValue = WebConfigOption(WebOptions._OPT_FIPS_MODE, "", True)  ' D6689
                        _OPT_USE_CUSTOM_SETTINGS = CSet
                    End If
                    If tOpt.ConfigName = _OPT_LOGOUT_AFTER_TIMEOUT Then sName += String.Format(" ({0} mins)", SessionTimeout / 60)    ' D6642
                    If tOpt.SettingType Is GetType(Boolean) Then sOpt = String.Format("<input type='checkbox' name='opt{0}' id='opt{0}' value='1' {2} onclick='onChange(this.name, (this.checked ? 1 : 0));'>&nbsp;<label for='opt{0}'>{1}</label>", tOpt.ConfigName, sName, IIf(tValue = "1", "checked", ""))
                    If tOpt.SettingType Is GetType(Integer) Then sOpt = String.Format("<span class='gray'>&raquo;</span>&nbsp; {1}: <input type='input' name='opt{0}' value='{2}' size=5 onblur='onChange(this.name, this.value);'>", tOpt.ConfigName, sName, tValue)
                    If tOpt.SettingType Is GetType(String) Then sOpt = String.Format("<span class='gray'>&raquo;</span>&nbsp; {1}: <input type='input' name='opt{0}' value='{2}' size=50 onblur='onChange(this.name, this.value);'>", tOpt.ConfigName, sName, tValue)
                    If sOpt <> "" Then sRes += String.Format("<div style='padding:2px 0px 2px {1}px;{3}' id='div{2}'><nobr>{0}</nobr><span style='float:right; padding-top:2px;{4}' class='small gray' onmouseover='$(""#div{2}"").css(""background-color"", ""#eaeaea"");' onmouseout='$(""#div{2}"").css(""background-color"", """");'>[<a href='' class='actions' style='padding:0px 2px; color: #cc9999;' onclick='reset(""{2}""); return false;'><b>X</b></a>]</div>", sOpt, IIf(GetType(Boolean) Is tOpt.SettingType, "0", "7"), tOpt.ConfigName, If(tOpt.Value.ToLower <> tValue.ToLower, " color:#951515;", If(DBSettings.Contains(tOpt.ID), "color:#006699'", "")), If(DBSettings.Contains(tOpt.ID), "", " display:none;")) ' D4773 + D6689 + D7230
                End If
            Next
        End If
        Return sRes
    End Function

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        'If Not App.isAuthorized OrElse Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) OrElse WebConfigOption(_OPT_LIC_PSW_HASH, "", True) = "" OrElse App.isSelfHost Then FetchAccess(_PGID_ERROR_403) ' D3949 + D3965
        If Not App.isAuthorized OrElse Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then FetchAccess(_PGID_ERROR_403) ' D3949 + D3965 + D7019
        'If Not _OPT_USE_CUSTOM_SETTINGS Then FetchAccess(_PGID_DB_SETUP)
        AlignVerticalCenter = True
        AlignHorizontalCenter = True
        ShowNavigation = False
        If isAJAX() Then
            Dim sRes As String = ""
            Select Case CheckVar(_PARAM_ACTION, "").ToLower
                Case _ACTION_SAVE
                    Dim sName As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("name", "")) 'Anti-XSS
                    If sName.StartsWith("opt") Then sName = sName.Substring(3)
                    If sName <> "" Then
                        Dim tOpt As clsSetting = clsSetting.SettingByName(_Options_Individual, sName)
                        If tOpt IsNot Nothing Then
                            Dim sVal As String = Nothing
                            If tOpt.SettingType Is GetType(Boolean) Then sVal = CStr(IIf(CheckVar("val", (tOpt.Value = "1")), "1", "0"))
                            If tOpt.SettingType Is GetType(Integer) Then
                                Dim tDef As Integer = Integer.MinValue
                                If Integer.TryParse(tOpt.Value, tDef) Then
                                    sVal = CStr(CheckVar("val", tDef))
                                End If
                            End If
                            If tOpt.SettingType Is GetType(String) Then sVal = EcSanitizer.GetSafeHtmlFragment(CheckVar("val", tOpt.Value)) 'Anti-XSS
                            ' D6646 ===
                            Dim tVal As Integer
                            If Not Integer.TryParse(sVal, tVal) Then tVal = -1
                            Select Case tOpt.ConfigName
                                Case _OPT_PSW_MIN_LEN
                                    Integer.TryParse(sVal, _DEF_PASSWORD_MIN_LENGTH)
                                    If _DEF_PASSWORD_MAX_LENGTH > 0 AndAlso _DEF_PASSWORD_MIN_LENGTH > 0 AndAlso _DEF_PASSWORD_MAX_LENGTH < _DEF_PASSWORD_MIN_LENGTH Then sVal = _DEF_PASSWORD_MAX_LENGTH.ToString
                                    If _DEF_PASSWORD_MIN_LENGTH > 48 Then sVal = "48"   ' D6657
                                    If _DEF_PASSWORD_MIN_LENGTH < 0 Then sVal = "0"     ' D6657
                                Case _OPT_PSW_MAX_LEN
                                    Integer.TryParse(sVal, _DEF_PASSWORD_MAX_LENGTH)
                                    If _DEF_PASSWORD_MAX_LENGTH > 0 AndAlso _DEF_PASSWORD_MIN_LENGTH > 0 AndAlso _DEF_PASSWORD_MAX_LENGTH < _DEF_PASSWORD_MIN_LENGTH Then sVal = _DEF_PASSWORD_MIN_LENGTH.ToString
                                    If _DEF_PASSWORD_MAX_LENGTH > 48 Then sVal = "48"   ' D6657
                                    If _DEF_PASSWORD_MAX_LENGTH < 0 Then sVal = "0"     ' D6657
                                ' -D6657
                                'Case _OPT_PSW_MIN_LIFETIME
                                '    Integer.TryParse(sVal, _DEF_PASSWORD_MIN_LIFETIME)
                                '    If _DEF_PASSWORD_MAX_LIFETIME > 0 AndAlso _DEF_PASSWORD_MIN_LIFETIME > 0 AndAlso _DEF_PASSWORD_MAX_LIFETIME < _DEF_PASSWORD_MIN_LIFETIME Then sVal = _DEF_PASSWORD_MAX_LIFETIME.ToString
                                Case _OPT_PSW_MAX_LIFETIME
                                    Integer.TryParse(sVal, _DEF_PASSWORD_MAX_LIFETIME)
                                    'If _DEF_PASSWORD_MAX_LIFETIME > 0 AndAlso _DEF_PASSWORD_MIN_LIFETIME > 0 AndAlso _DEF_PASSWORD_MAX_LIFETIME < _DEF_PASSWORD_MIN_LIFETIME Then sVal = _DEF_PASSWORD_MIN_LIFETIME.ToString
                                    If _DEF_PASSWORD_MAX_LIFETIME < 0 Then sVal = "0"   ' D6657
                                    ' D6659 ===
                                Case _OPT_PSW_PREV_HASHES
                                    Integer.TryParse(sVal, _DEF_PASSWORD_KEEP_HASHES)
                                    If _DEF_PASSWORD_KEEP_HASHES < 0 Then sVal = "0"
                                    If _DEF_PASSWORD_KEEP_HASHES > _DEF_PASSWORD_HASHES_MAX Then _DEF_PASSWORD_KEEP_HASHES = _DEF_PASSWORD_HASHES_MAX
                                    ' D6659 ==
                            End Select
                            ' D6646 ==
                            If tOpt.Value <> sVal OrElse tOpt.ConfigName = _OPT_FIPS_MODE Then ' D6689
                                tOpt.Value = sVal
                                App.DBExtraWrite(clsExtra.Params2Extra(tOpt.ID, ecExtraType.Common, ecExtraProperty.WebConfigSetting, tOpt.Value))
                                App.DBSaveLog(dbActionType.actModify, dbObjectType.einfConfigSetting, tOpt.ID, "Edit config setting", String.Format("{0}:                     '{1}'", tOpt.ConfigName, IIf(tOpt.ConfigName.ToLower = "licensepswhash", "-hidden-", tOpt.Value)))  ' D3949
                                LoadComparionCoreOptions(App)   ' D6432
                                sRes = String.Format("['{0}','{1}', {2}]", JS_SafeString(tOpt.ConfigName), JS_SafeString(sVal), Bool2JS(tOpt.SettingType Is GetType(Boolean)))   ' D6646
                            End If
                        End If
                    End If

                    ' D4773 ===
                Case "reset"
                    Dim sName As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("name", "")) 'Anti-XSS
                    If sName <> "" Then
                        Dim tOpt As clsSetting = clsSetting.SettingByName(_Options_Individual, sName)
                        If tOpt IsNot Nothing Then
                            App.DBExtraDelete(clsExtra.Params2Extra(tOpt.ID, ecExtraType.Common, ecExtraProperty.WebConfigSetting))
                            OptionsIndividualInit(App)
                            App.DBSaveLog(dbActionType.actDelete, dbObjectType.einfConfigSetting, tOpt.ID, "Reset config setting", String.Format("{0}", tOpt.ConfigName))
                            sRes = "['reset', 'true']"  ' D6646
                        End If
                    End If
                    ' D4773 ==
            End Select

            ' D6646 ===
            RawResponseStart()
            Response.ContentType = "text/plain"
            Response.Write(sRes)
            Response.End()
            ' D6646 ==

        End If
    End Sub

End Class
