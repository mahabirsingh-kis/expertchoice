Imports System.IO
Imports System.Globalization

Partial Class LicensePage
    Inherits clsComparionCorePage

#Const _OPT_SAVE_LICENSE = True    ' D3046

    Private _CurWG As clsWorkgroup = Nothing    ' D0266
    Private _CurUW As clsUserWorkgroup = Nothing    ' D7280
    Private fCanEdit As Boolean = False         ' D3045
    Public IsEditMode As Boolean = False        ' D3045
    Public sError As String = ""                ' D3045

    Private Const _OPT_DT_FORMAT As String = "MM\/dd\/yyyy"   ' D3046
    Private Const _OPT_DT_FORMAT_SHORT As String = "MM\/dd\/yy"   ' D3277

    Public CheckList As String = ""             ' D3058
    Public fCanSee As Boolean = True            ' D4624

    Public PgTitle As String = ""               ' D6559

    Public Sub New()
        MyBase.New(_PGID_ADMIN_LICENSE) ' D3037
    End Sub

    ' D0266 ===
    Public ReadOnly Property CurrentWorkgroup() As clsWorkgroup
        Get
            If _CurWG Is Nothing AndAlso App.ActiveUser IsNot Nothing Then  ' D7364
                Dim ID As Integer = CheckVar(_PARAM_ID, -1)
                _CurWG = App.DBWorkgroupByID(ID)
                _CurUW = App.DBUserWorkgroupByUserIDWorkgroupID(App.ActiveUser.UserID, ID)  ' D7280
                Dim fCanSee As Boolean = App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) OrElse App.CanUserDoSystemWorkgroupAction(ecActionType.at_slViewLicenseAnyWorkgroup, App.ActiveUser.UserID) OrElse App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageOwnWorkgroup, App.ActiveUser.UserID)   ' D3288
                If Not fCanSee Then
                    fCanSee = App.isWorkgroupAvailable(_CurWG, App.ActiveUser, _CurUW)
                End If
                If _CurWG Is Nothing OrElse Not fCanSee Then _CurWG = App.ActiveWorkgroup
                CustomWorkgroupPermissions = _CurWG     ' D7270
            End If
            Return _CurWG
        End Get
    End Property
    ' D0266 ==

    ' D3965 ===
    Public Function GetCreatedAt() As String
        Dim tVal As Long = 0
        If CurrentWorkgroup.License.isValidLicense Then
            tVal = CurrentWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.CreatedAt)
        End If
        If tVal <> 0 AndAlso tVal <> -1 AndAlso tVal <> UNLIMITED_DATE AndAlso tVal <> UNLIMITED_VALUE Then Return String.Format("Created at {0}", BinaryStr2DateTime(tVal.ToString).Value) Else Return ""  ' D6559
    End Function
    ' D3965 ==

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignVerticalCenter = True
        AlignHorizontalCenter = True
        ' D0302 ===
        sError = ""
        If CurrentWorkgroup.Status = ecWorkgroupStatus.wsSystem And Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slViewLicenseAnyWorkgroup, App.ActiveUser.UserID) Then  ' D0474
            sError = ResString("msgCantSeeSystemLicense")
            fCanSee = False     ' D4624
        Else
            App.CheckLicense(CurrentWorkgroup, sError, False)  ' D0262 + D0266
        End If
        ' D0302 ==

        ' D3287 ===
        If isAJAX() Then Ajax_Callback() ' D3112
        ' D3287 ==

        ' D3045 ===
        fCanEdit = App.ActiveUser.CannotBeDeleted AndAlso CurrentWorkgroup.License IsNot Nothing AndAlso CurrentWorkgroup.License.isValidLicense AndAlso CurrentWorkgroup.License.LicenseKey <> "" AndAlso WebConfigOption(_OPT_LIC_PSW_HASH, "", True) <> "" AndAlso Not App.isSelfHost  ' D3046 + D3965
        IsEditMode = fCanEdit AndAlso EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")) = _ACTION_EDIT   'Anti-XSS

        btnBack.Visible = Request.UrlReferrer IsNot Nothing   ' D0993
        btnEdit.Visible = fCanEdit
        btnEditWkg.Visible = App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) OrElse App.isWorkgroupOwner(CurrentWorkgroup, App.ActiveUser, _CurUW)   ' D0993 + D7280

        If fCanEdit AndAlso IsEditMode Then
            btnSaveChanges.Visible = True
            btnSaveChanges.Text = ResString("btnSaveLicense")
            btnEdit.Visible = False
            btnCancel.Visible = True
            btnCancel.Text = ResString("btnCancel")
            btnEditWkg.Visible = False
            btnBack.Visible = False
        End If
        btnWM.Visible = Not IsEditMode AndAlso CurrentWorkgroup.Status <> ecWorkgroupStatus.wsSystem AndAlso btnEditWkg.Visible   ' D3287 + D3288 + D7280

        If btnEditWkg.Visible Then
            btnEditWkg.Text = PageTitle(_PGID_ADMIN_WORKGROUP_EDIT) ' D0993
            btnEditWkg.OnClientClick = String.Format("document.location.href='{0}'; this.disabled=1; return false;", PageURL(_PGID_ADMIN_WORKGROUP_EDIT, _PARAM_ID + "=" + CurrentWorkgroup.ID.ToString + GetTempThemeURI(True))) ' D0993
        End If

        If btnEdit.Visible Then
            btnEdit.Text = ResString("btnEditLicense")
            btnEdit.OnClientClick = String.Format("document.location.href='{0}'; return false;", PageURL(CurrentPageID, _PARAM_ID + "=" + CurrentWorkgroup.ID.ToString + GetTempThemeURI(True) + "&" + _PARAM_ACTION + "=" + _ACTION_EDIT)) ' D3505
        End If

        If btnBack.Visible Then
            btnBack.Text = ResString("btnBack") ' D0717 + D0921
            btnBack.OnClientClick = String.Format("document.location.href='{0}'; return false;", PageURL(_PGID_ADMIN_WORKGROUPS, GetTempThemeURI(False)))
            ClientScript.RegisterStartupScript(GetType(String), "Init", String.Format("setTimeout('theForm.{0}.focus();', 50);", btnBack.ClientID), True)
        End If
        ' D3045 ==

        If isSLTheme() AndAlso CheckVar("update", False) Then ClientScript.RegisterStartupScript(GetType(String), "UpdateSL", "ReloadWorkgroups(); ", True) ' D3241 + D3312
        If btnWM.Visible Then
            btnWM.Text = ResString("btnWMList")
            btnWM.OnClientClick = "GetWM(); return false;"
        End If
        ' D3287 ==

        divError.Visible = sError <> ""  ' D0262
        If sError <> "" Then sError = ParseString(sError) ' D6559
        PgTitle = String.Format(ResString(CStr(IIf(IsEditMode, "lblLicenseEditHeader", "lblLicenseHeader"))), CurrentWorkgroup.Name)    ' D6559
        SetPageTitle(PgTitle)  ' D3046 + D6559
    End Sub

    ' D3045 ===
    Public Function GetLicenseDetails() As String
        Dim sRes As String = ""
        If CurrentWorkgroup IsNot Nothing AndAlso CurrentWorkgroup.License IsNot Nothing AndAlso CurrentWorkgroup.License.isValidLicense AndAlso fCanSee Then   ' D4624
            Dim idx As Boolean = False
            For Each tItem As clsLicenseParameter In CurrentWorkgroup.License.Parameters

                Dim sItem As ecLicenseParameter = tItem.ID

                Dim tVal As Long = CurrentWorkgroup.License.GetParameterValueByID(sItem)
                Dim tMax As Long = CurrentWorkgroup.License.GetParameterMaxByID(sItem)

                Dim sName = tItem.Name
                If sName = "" Then sName = sItem.ToString Else sName = ResString(sName)

                Dim sVal As String = ""
                Dim sPrg As String = ""

                ' D3046 ===
                Dim sEdit As String = ""
                Dim isBool As Boolean = False
                Dim sUnlim As String = ""
                If IsEditMode Then sUnlim = String.Format("<input type='checkbox' class='checkbox' id='u_opt{0}' name='u_opt{0}' value='1'{1} onclick='changeUnlim({0}, this.checked);' onkeyup='changeUnlim({0}, this.checked);'/>", CInt(tItem.ID), IIf(tMax = UNLIMITED_VALUE OrElse tMax = UNLIMITED_DATE OrElse tMax = 0, " checked", "")) ' D3952
                ' D3046 ==

                Dim isValid As Boolean = tVal < tMax

                Select Case tItem.ID
                    Case ecLicenseParameter.ExpirationDate
                        Dim dayMax As Date = Date.Now.AddYears(1)
                        If tMax = UNLIMITED_DATE Then
                            sVal = String.Format("<b>{0}</b>", ResString("lblUserNoExpiration"))
                        Else
                            dayMax = Date.FromBinary(tMax)
                            Dim dayNow As Date = Date.FromBinary(tVal)
                            isValid = dayNow < dayMax ' D3277
                            sVal = String.Format("<b>{0}</b>", dayMax.ToShortDateString)
                            Dim sLeft As String = String.Format(ResString("lblLicenseDayLeft"), 0)
                            ' D3277 ===
                            If dayNow < dayMax Then
                                Dim d As Long = dayMax.Subtract(dayNow).Days
                                If d > 365 * 3 Then
                                    sLeft = String.Format(ResString("lblLicenseYearsLeft"), (d / 365).ToString("F1").Replace(",", "."))
                                Else
                                    If d > 365 Then
                                        sLeft = String.Format(ResString("lblLicenseMonthsLeft"), (d / 30.41).ToString("F1").Replace(",", "."))
                                    Else
                                        sLeft = String.Format(ResString("lblLicenseDayLeft"), d)
                                    End If
                                End If
                            End If
                            sVal += sLeft
                            ' D3277 ==
                        End If
                        Dim sAdd As String = String.Format("<div style='text-align:center;{0}' class='small gray' id='divAdd'><a href='' onclick='return AddPeriod(1,""week"");' class='actions dashed'>+1w</a> | <a href='' onclick='return AddPeriod(1,""month"");' class='actions dashed'>+1m</a> | <a href='' onclick='return AddPeriod(3,""month"");' class='actions dashed'>+3m</a> | <a href='' onclick='return AddPeriod(6,""month"");' class='actions dashed'>+6m</a> | <a href='' onclick='return AddPeriod(1,""year"");' class='actions dashed'>+1y</a></div>", IIf(tMax = UNLIMITED_VALUE OrElse tMax = UNLIMITED_DATE, "display:none;", "")) ' D3058
                        sEdit = String.Format("<input type='text' name='opt{0}' id='datepicker' value='{1}' style='width:7em'{2}>{3}", CInt(tItem.ID), IIf(tMax = UNLIMITED_DATE, "", dayMax.ToString(_OPT_DT_FORMAT)), IIf(tMax = UNLIMITED_DATE, " disabled='disabled'", ""), sAdd)   ' D3058
                        sUnlim = String.Format("<input type='checkbox' class='checkbox' id='u_opt{0}' name='u_opt{0}' value='1'{1} onclick='changeUnlimDate({0}, this.checked);' onkeyup='changeUnlimDate({0}, this.checked);'/>", CInt(tItem.ID), IIf(tMax = UNLIMITED_VALUE OrElse tMax = UNLIMITED_DATE, " checked", ""))

                    Case ecLicenseParameter.SpyronEnabled, ecLicenseParameter.TeamTimeEnabled,
                         ecLicenseParameter.CommercialUseEnabled, ecLicenseParameter.ExportEnabled,
                         ecLicenseParameter.RiskEnabled, ecLicenseParameter.ResourceAlignerEnabled,
                         ecLicenseParameter.RiskTreatments, ecLicenseParameter.RiskTreatmentsOptimization,
                         ecLicenseParameter.AllowUseGurobi, ecLicenseParameter.isSelfHost  ' bools ' D3586 + D3922 + D3965
                        isValid = tMax > 0
                        isBool = True   ' D3046
                        'If tItem.ID = ecLicenseParameter.RiskEnabled Then isValid = True
                        If isValid Then sVal = String.Format("<img src='{0}check.png' width='16' height='16' title='Enabled' border=0/>", ImagePath)
                        ' D3058 ===
                        Dim sExtra As String = ""
                        If tItem.ID = ecLicenseParameter.RiskEnabled Then
                            sExtra = String.Format(" onclick='CheckRisk(); ConfirmRisk({0},""{1}"",this.checked);'", IIf(tMax > 0, "true", "false"), CInt(tItem.ID))
                        End If
                        ' D3591 ===
                        If tItem.ID = ecLicenseParameter.RiskTreatments Then
                            sExtra = " onclick='CheckRiskTreatment();'"
                        End If
                        ' D3591 ==
                        ' D3923 ===
                        If tItem.ID = ecLicenseParameter.ResourceAlignerEnabled Then
                            sExtra = " onclick='CheckRA();'"
                        End If
                        ' D3923 ==
                        sEdit = String.Format("<input type='checkbox' class='checkbox' id='opt{0}' name='opt{0}' value='1'{1}{2}{3}/>", CInt(tItem.ID), IIf(tMax > 0, " checked", ""), IIf(tItem.ID = ecLicenseParameter.SpyronEnabled AndAlso clsLicense.SPYRON_ALLOW_FOR_ALL, " disabled", ""), sExtra)  ' D3046
                        ' D3058 ==

                    Case ecLicenseParameter.MaxObjectives, ecLicenseParameter.MaxLevelsBelowGoal,
                         ecLicenseParameter.MaxAlternatives, ecLicenseParameter.MaxUsersInProject,
                         ecLicenseParameter.MaxPMsInProject   ' numbers the projects
                        isValid = True
                        If tMax = UNLIMITED_VALUE Then
                            sVal = String.Format("<b>{0}</b>", ResString("lblLicenseUnlimitedValue"))
                        Else
                            If tMax = LICENSE_NOVALUE Then
                                sVal = "<span class='error'>Undefined</span>"
                            Else
                                sVal = String.Format("<b>{0}</b>", tMax)
                            End If
                        End If
                        sEdit = String.Format("<input type='text' name='opt{0}' id='opt{0}' class='spinner' value='{1}' style='width:3em'{2}>", CInt(tItem.ID), IIf(tMax = UNLIMITED_VALUE, "", tMax), IIf(tMax = UNLIMITED_VALUE, " dis", ""))
                        CheckList += String.Format("{0}'{1}'", IIf(CheckList = "", "", ","), CInt(tItem.ID))  ' D3058

                        ' D3946 ===
                    Case ecLicenseParameter.InstanceID
                        Dim fUndef As Boolean = tMax = 0 OrElse tMax = UNLIMITED_VALUE  ' D3952
                        If fUndef Then
                            sVal = ResString("lbl_atUnspecified")
                            isValid = True
                        Else
                            sVal = tMax.ToString("X16").Insert(8, "-")
                            isValid = CurrentWorkgroup.License IsNot Nothing AndAlso CurrentWorkgroup.License.isValidLicense AndAlso CurrentWorkgroup.License.CheckParameterByID(ecLicenseParameter.InstanceID) ' D3952
                        End If
                        sEdit = String.Format("<input id='opt{0}' name='opt{0}' type='text' style='width:13em; text-align:center; text-transform: uppercase' maxlength=17 value='{1}' class='nocross' {2}>&nbsp;<img src='{3}numbers.png' width=16 height=16 title='Fill with current InstanceID' style='cursor:pointer' onclick='insertInstanceID()'>", CInt(tItem.ID), JS_SafeString(IIf(fUndef, "", sVal)), IIf(fUndef, "disabled=1", ""), ImagePath)    ' D3952
                        ' D3946 ==

                    Case Else       ' numbers for workgroup objs
                        If tMax = UNLIMITED_VALUE Then
                            sVal = String.Format("<b>{0}</b>", ResString("lblLicenseUnlimitedValue"))
                            isValid = True
                        Else
                            If tMax = LICENSE_NOVALUE Then
                                sVal = "<span class='error'>Undefined</span>"
                            Else
                                sVal = String.Format("<b>{0}</b>", tMax)
                                'sPrg = HTMLCreateGraphBar(If(tVal > tMax, tMax, tVal), tMax, 150, 8, CStr(IIf(tVal >= tMax, "fill2", "fill1")), BlankImage)
                                sPrg = String.Format("<div class='license_progress' data-value='{0}' data-max='{1}'></div>", If(tVal > tMax, tMax, tVal), tMax)  ' D4944
                            End If
                        End If
                        sVal = String.Format(ResString("lblLicenseValueOf"), "<b>" + CStr(tVal) + "</b>", sVal)
                        If tMax > 0 AndAlso tMax <> UNLIMITED_VALUE Then sVal += String.Format(ResString("lblLicenseValueLeft"), IIf(tVal < tMax, tMax - tVal, 0))
                        sEdit = String.Format("<input type='text' name='opt{0}' id='opt{0}' class='spinner' value='{1}' style='width:3em'{2}>", CInt(tItem.ID), IIf(tMax = UNLIMITED_VALUE, "", tMax), IIf(tMax = UNLIMITED_VALUE, " dis", ""))
                        CheckList += String.Format("{0}'{1}'", IIf(CheckList = "", "", ","), CInt(tItem.ID))  ' D3058

                End Select

                ' D3586 ===
                Dim fIsTreatments As Boolean = tItem.ID = ecLicenseParameter.RiskTreatments OrElse tItem.ID = ecLicenseParameter.RiskTreatmentsOptimization
                Dim fVis As Boolean = True
                If (fIsTreatments AndAlso CurrentWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.RiskEnabled) = 0) OrElse ((Not _RA_GUROBI_AVAILABLE OrElse Not App.isRAAvailable) AndAlso tItem.ID = ecLicenseParameter.AllowUseGurobi) Then fVis = False ' D3923 + D4550 + D7007
                ' D3965 ===
                Dim w As Integer = 10
                Select Case sItem
                    Case ecLicenseParameter.RiskEnabled
                        w = 14
                    Case ecLicenseParameter.isSelfHost
                        w = 11
                End Select
                Dim sImg As String = String.Format("<img src='{1}' width='{2}' height='{2}' title='' border=0 style='margin:0px {3}px 0px 2px'/>", ImagePath, IIf(sItem = ecLicenseParameter.RiskEnabled, "/images/wkg_riskion.png", ImagePath + CStr(If(sItem = ecLicenseParameter.isSelfHost, "lock_small.gif", IIf(isValid, "apply_tiny.gif", "delete_tiny.gif")))), w, IIf(sItem = ecLicenseParameter.RiskEnabled, 3, 7))
                If sItem <> ecLicenseParameter.CreatedAt AndAlso (Not IsEditMode OrElse sItem <> ecLicenseParameter.isSelfHost) Then sRes += String.Format("<tr class='text {4}' valign='middle'{6} id='tr{7}'><td align=left{5}{8}>{3}{0}</td><td align=center{5}>{1}</td><td align=center>{2}</td></tr>" + vbCrLf, sName, IIf(IsEditMode AndAlso sEdit <> "", sEdit, IIf(sVal = "", "&nbsp;", sVal)), IIf(sPrg = "" OrElse IsEditMode, IIf(IsEditMode AndAlso Not isBool, sUnlim, "&nbsp;"), sPrg), IIf(IsEditMode, "", sImg), IIf(idx, "tbl_row", "tbl_row_alt"), IIf(isValid OrElse sItem = ecLicenseParameter.RiskEnabled OrElse sItem = ecLicenseParameter.isSelfHost, "", " class='error'"), IIf(fVis, "", " style='display:none'"), CInt(tItem.ID), IIf(fIsTreatments OrElse sItem = ecLicenseParameter.AllowUseGurobi, " style='padding-left:20px;'", "")) ' D3046 + D3922
                ' D3586 + D3965 ==

                idx = Not idx
            Next
            sRes = String.Format("<table border='0' cellspacing='0' cellpadding='4' class='tbl' style='width:100%; background:#f0f0f0; border:1px solid #d0d0d0;'><tr class='tbl_hdr h6'><td>License Parameter</td><td>Parameter value</td><td>{1}</td></tr>{0}</table>", sRes, IIf(IsEditMode, "Unlimited", "&nbsp;"))  ' D3046 + D3058
        End If
        Return sRes
    End Function

    ' D3046 ===
    Private Function Val2String(tVal As Long, isBool As Boolean, ParamType As ecLicenseParameter) As String    ' D3359 + D3947
        Dim sVal As String = tVal.ToString
        If isBool Then
            sVal = CStr(IIf(tVal = 0, "Yes", "No"))
        Else
            If tVal = UNLIMITED_DATE Then
                sVal = ResString("lblUserNoExpiration")
            Else
                If tVal = UNLIMITED_VALUE Then
                    sVal = ResString("lblLicenseUnlimitedValue")
                Else
                    ' D3947 ===
                    sVal = tVal.ToString
                    If ParamType = ecLicenseParameter.ExpirationDate Then sVal = Date.FromBinary(tVal).ToString(_OPT_DT_FORMAT) ' D3359
                    If ParamType = ecLicenseParameter.InstanceID Then sVal = tVal.ToString("X16").Insert(8, "-")
                    ' D3947 ==
                End If
            End If
        End If
        Return sVal
    End Function
    ' D3046 ==

    Protected Sub btnSaveChanges_Click(sender As Object, e As EventArgs) Handles btnSaveChanges.Click
        ' D3046 ===
        If fCanEdit AndAlso IsEditMode Then

            Dim sPsw As String = EcSanitizer.GetSafeHtmlFragment(tbPsw.Text)    'Anti-XSS
            Dim sPswEncr As String = GetMD5(sPsw)
            If sPswEncr <> WebConfigOption(_OPT_LIC_PSW_HASH, "", True) Then
                sError = ResString("msgWrongLicensePsw")
                divError.Visible = True
                Exit Sub
            End If

            Dim tNewLicense As New clsParamsFile
            CurrentWorkgroup.License.LicenseContent.Seek(0, SeekOrigin.Begin)
            If tNewLicense.Read(CurrentWorkgroup.License.LicenseContent, CurrentWorkgroup.License.LicenseKey) Then

                Dim ChangesList As String = ""
                For Each param As clsRestrictionParameter In tNewLicense.Parameters
                    Dim tMax As Long = param.Value
                    Dim sParam As String = String.Format("opt{0}", CInt(param.ID))
                    Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(sParam, ""))  'Anti-XSS
                    If Request.Params.AllKeys.Contains("u_" + sParam) AndAlso CheckVar("u_" + sParam, "") = "1" Then sVal = CStr(IIf(param.ID = ecLicenseParameter.ExpirationDate, UNLIMITED_DATE, IIf(param.ID = ecLicenseParameter.InstanceID, 0, UNLIMITED_VALUE))) ' D3947

                    Dim tVal As Long = tMax

                    Dim isBool As Boolean = False
                    Select Case param.ID
                        Case ecLicenseParameter.ExpirationDate
                            If sVal <> UNLIMITED_DATE.ToString AndAlso sVal <> "" Then
                                Dim MyCultureInfo As CultureInfo = New CultureInfo("en-US")
                                Dim tDT As DateTime
                                If DateTime.TryParse(sVal, MyCultureInfo, DateTimeStyles.None, tDT) Then
                                    ' D3277 ===
                                    If sVal.Length = 8 AndAlso sVal = Date.FromBinary(param.Value).ToString(_OPT_DT_FORMAT_SHORT) Then
                                        sVal = CStr(param.Value)
                                    Else
                                        If tDT.Year < 2010 Then tDT = tDT.AddYears(100) ' D3277
                                        sVal = CLng(Date2ULong(tDT)).ToString
                                    End If
                                    ' D3277 ==
                                End If
                            End If

                        Case ecLicenseParameter.SpyronEnabled, ecLicenseParameter.TeamTimeEnabled,
                             ecLicenseParameter.CommercialUseEnabled, ecLicenseParameter.ExportEnabled,
                             ecLicenseParameter.RiskEnabled, ecLicenseParameter.ResourceAlignerEnabled,
                             ecLicenseParameter.RiskTreatments, ecLicenseParameter.RiskTreatmentsOptimization,
                             ecLicenseParameter.AllowUseGurobi ' D3586 + D3923
                            'If Request.Params.AllKeys.Contains(sParam) AndAlso sVal = "" Then sVal = "0"
                            If sVal = "" Then sVal = "0"
                            isBool = True
                            If clsLicense.SPYRON_ALLOW_FOR_ALL AndAlso param.ID = ecLicenseParameter.SpyronEnabled Then sVal = tMax.ToString

                            ' D3952 ===
                        Case ecLicenseParameter.InstanceID
                            If sVal = "" OrElse sVal = "0" OrElse sVal = UNLIMITED_VALUE.ToString OrElse Not isValidInstanceID(sVal) Then
                                tVal = 0
                            Else
                                If Not Long.TryParse(sVal.Trim.Replace("-", ""), NumberStyles.AllowHexSpecifier, Nothing, tVal) Then tVal = 0
                            End If
                            sVal = tVal.ToString
                            ' D3952 ==

                    End Select

                    If sVal <> "" AndAlso Long.TryParse(sVal, tVal) Then
                        If tVal <> tMax Then
                            ChangesList += String.Format("Set '{0}' as {1} (was: {2}); " + vbCrLf, ResString(CurrentWorkgroup.License.GetParameterNameByID(param.ID)), Val2String(tVal, isBool, param.ID), Val2String(tMax, isBool, param.ID))    ' D3359 + D3947
                            tNewLicense.SetParameter(param.ID, tVal)
                        End If
                    End If
                Next

#If _OPT_SAVE_LICENSE Then
                If ChangesList <> "" Then
                    Dim sName As String = File_CreateTempName()
                    If tNewLicense.Write(sName, CurrentWorkgroup.License.LicenseKey) Then
                        Dim LicenseData() As Byte = IO.File.ReadAllBytes(sName)
                        CurrentWorkgroup.License.LicenseContent = New MemoryStream(LicenseData)
                        App.DBWorkgroupUpdate(CurrentWorkgroup, False, String.Format("On-line edit license params for '{0}'", CurrentWorkgroup.Name), True)
                        If CurrentWorkgroup.ID = App.ActiveWorkgroup.ID Then App.ActiveWorkgroup = App.DBWorkgroupByID(App.ActiveWorkgroup.ID, True, True)
                        App.DBExtraDelete(clsExtra.Params2Extra(CurrentWorkgroup.ID, ecExtraType.Workgroup, ecExtraProperty.CheckMasterProjectsDate)) ' D7007
                        SessVar(String.Format("CheckMaster_{0}", CurrentWorkgroup.ID)) = Nothing
                        App.DBSaveLog(dbActionType.actModify, dbObjectType.einfWorkgroup, CurrentWorkgroup.ID, "Edit license", ChangesList, App.ActiveUser.UserID, CurrentWorkgroup.ID)
                    End If
                    File_Erase(sName)
                    App.ResetWorkgroupsList()     ' D4984
                Else
                    If App.ActiveWorkgroup IsNot Nothing AndAlso CurrentWorkgroup.ID = App.ActiveWorkgroup.ID Then App.ResetWorkgroupsList()    ' D7676
                End If
#End If
            End If
        End If
        ' D3046 ==
        IsEditMode = False
        Response.Redirect(PageURL(CurrentPageID, _PARAM_ID + "=" + CurrentWorkgroup.ID.ToString + If(isSLTheme(), "&update=1", "") + GetTempThemeURI(True)), True)   ' D3241 + D3312 (reload wkg list for every time
    End Sub
    ' D3045 ==

    ' D3287  ===
    Private Sub Ajax_Callback()
        Dim sResult As String = ""
        Dim args As NameValueCollection = Page.Request.QueryString
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_ACTION).ToLower())    'Anti-XSS

        Select Case sAction
            Case "wm_list"
                Dim WMGrpID As Integer = CurrentWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtWorkgroupManager)
                Dim tList As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(String.Format("SELECT U.* FROM {0} as U, {1} as UW WHERE UW.{4}={5} AND U.{2}=UW.{3} AND UW.{6}={7} ORDER BY U.{8}", clsComparionCore._TABLE_USERS, clsComparionCore._TABLE_USERWORKGROUPS, clsComparionCore._FLD_USERS_ID, clsComparionCore._FLD_USERWRKG_USERID, clsComparionCore._FLD_USERWRKG_WRKGID, CurrentWorkgroup.ID, clsComparionCore._FLD_USERWRKG_ROLEID, WMGrpID, clsComparionCore._FLD_USERS_EMAIL))
                Dim tUsers As New List(Of clsApplicationUser)
                For Each tRow As Dictionary(Of String, Object) In tList
                    tUsers.Add(App.DBParse_ApplicationUser(tRow))
                Next
                For Each tmpUser As clsApplicationUser In tUsers
                    sResult += String.Format("{0}['{1}','{2}']", IIf(sResult = "", "", ","), tmpUser.UserEmail, tmpUser.UserName)
                Next
                sResult = "[" + sResult + "]"

        End Select

        RawResponseStart()
        Response.ContentType = "text/plain"
        'Response.AddHeader("Content-Length", CStr(sResult))
        Response.Write(sResult)
        Response.End()
        'RawResponseEnd()

    End Sub
    ' D3287==

    Private Sub LicensePage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        If CurrentWorkgroup Is Nothing Then FetchAccess(_PGID_ADMIN_WORKGROUPS) ' D7270
    End Sub


End Class
