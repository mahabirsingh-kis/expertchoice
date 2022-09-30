Imports System.IO

Public Class frmMain

    Private sFormName As String = ""    ' D2245
    Private mParamsFile As clsParamsFile
    Private tpCreatedAt As ToolTip = Nothing  ' D3965

    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        mParamsFile = New clsParamsFile

        ' D1048 ===
        InitParams()
        btnExpiration.Tag = dateExpire  ' D3352

        UpdateExpiration()
        UpdateOptionsList()
        UpdateCheckboxes()

        Me.Top -= pnlList.Height >> 1
        ' D1048 ==
    End Sub

    ' D3946 ===
    Private Sub InitParams()
        SetValue(ecLicenseParameter.ExpirationDate, False, Date.Now.ToBinary)
        SetValue(ecLicenseParameter.MaxProjectCreatorsInWorkgroup, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxPMsInProject, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxEvaluatorsInModel, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxModelsPerOwner, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxProjectsTotal, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxConcurrentEvaluatorsInModel, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxWorkgroupsTotal, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxProjectsOnline, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.TeamTimeEnabled, True, 1)
        SetValue(ecLicenseParameter.SpyronEnabled, True, 1)
        SetValue(ecLicenseParameter.ResourceAlignerEnabled, True, 1)
        SetValue(ecLicenseParameter.ExportEnabled, True, 1)
        SetValue(ecLicenseParameter.CommercialUseEnabled, True, 1)
        SetValue(ecLicenseParameter.MaxLifetimeProjects, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxObjectives, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxLevelsBelowGoal, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxAlternatives, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxViewOnlyUsers, False, UNLIMITED_VALUE)
        SetValue(ecLicenseParameter.MaxUsersInWorkgroup, False, UNLIMITED_VALUE)    ' D1482
        SetValue(ecLicenseParameter.RiskEnabled, True, 0)   ' D2056
        SetValue(ecLicenseParameter.RiskTreatments, True, 1)   ' D3585
        SetValue(ecLicenseParameter.RiskTreatmentsOptimization, True, 1)   ' D3585
        SetValue(ecLicenseParameter.AllowUseGurobi, True, 0)   ' D3622
        SetValue(ecLicenseParameter.InstanceID, False, 0)      ' D3946
        SetValue(ecLicenseParameter.isSelfHost, True, 0)       ' D3965
        SetValue(ecLicenseParameter.CreatedAt, False, 0)       ' D3965
        SetCreatedAt(0) ' D3965
    End Sub
    ' D3946 ==

    ' D3965 ===
    Private Sub SetCreatedAt(dt As Long)
        If tpCreatedAt Is Nothing Then
            tpCreatedAt = New ToolTip()
            With tpCreatedAt
                .AutoPopDelay = 5000
                .InitialDelay = 1000
                .ReshowDelay = 300
                .ShowAlways = True
            End With
        End If

        Dim sDate As String = " - unspecified -"
        If dt <> 0 AndAlso dt <> -1 AndAlso dt <> UNLIMITED_DATE AndAlso dt <> UNLIMITED_VALUE Then
            sDate = Date.FromBinary(dt).ToString
        End If
        tpCreatedAt.SetToolTip(cbInstanceID, "License created at: " + sDate)
    End Sub
    ' D3965 ==

    ' D1048 ===
    Private Sub SetValue(ByVal ValID As ecLicenseParameter, Optional ByVal fIsBoolean As Boolean = False, Optional ByVal DefValue As Long = UNLIMITED_VALUE)
        mParamsFile.SetParameter(ValID, DefValue)
    End Sub

    Private Function GetValue(ByVal ValID As ecLicenseParameter) As Long
        Dim tParam As clsRestrictionParameter = mParamsFile.GetParameter(ValID)
        If tParam IsNot Nothing Then Return tParam.Value Else Return UNLIMITED_VALUE
    End Function

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        ' D3946 ===
        If cbInstanceID.Checked AndAlso Not isValidInstanceID(tbInstanceID.Text) Then
            MessageBox.Show("Wrong Instance ID. Please check data for input.", "Error")
            Exit Sub
        End If
        ' D3946 ==
        If frmKey.ShowDialog = Windows.Forms.DialogResult.OK Then   ' D2244
            ' D1050 ===
            If dlgSave.FileName = "" AndAlso dlgOpen.FileName <> "" Then dlgSave.FileName = dlgOpen.FileName
            If dlgSave.FileName = "" AndAlso frmKey.txtSerial.Text <> "" Then dlgSave.FileName = String.Format("{0}.lic", frmKey.txtSerial.Text.Trim)
            If dlgSave.ShowDialog = Windows.Forms.DialogResult.OK AndAlso dlgSave.FileName <> "" Then   ' D0955
                SetName(dlgSave.FileName)   ' D2245

                ' D1050 ==
                If btnExpiration.Checked Then SetValue(ecLicenseParameter.ExpirationDate, False, UNLIMITED_DATE) ' D1050 + D3352
                ' D3946 ===
                If cbInstanceID.Checked Then
                    Dim sID As String = tbInstanceID.Text.Trim.Replace("-", "")
                    Dim tRes As Long
                    If Not Long.TryParse(sID, System.Globalization.NumberStyles.AllowHexSpecifier, Nothing, tRes) Then tRes = 0
                    mParamsFile.SetParameter(ecLicenseParameter.InstanceID, tRes)
                Else
                    mParamsFile.SetParameter(ecLicenseParameter.InstanceID, 0)
                End If
                ' D3946 ==
                SetValue(ecLicenseParameter.CreatedAt, False, Date.Now.ToBinary)    ' D3965

                mParamsFile.Write(dlgSave.FileName, frmKey.txtSerial.Text)  ' D2244
            End If
        End If
    End Sub

    Private Sub btnLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLoad.Click
        If dlgOpen.ShowDialog = Windows.Forms.DialogResult.OK AndAlso dlgOpen.FileName <> "" Then   ' D0955 + D1050
            If frmKey.ShowDialog = Windows.Forms.DialogResult.OK Then   ' D2244
                InitParams()
                If mParamsFile.Read(dlgOpen.FileName, frmKey.txtSerial.Text) Then
                    SetName(dlgOpen.FileName)   ' D2245
                    ' D1050 ===
                    If GetValue(ecLicenseParameter.ExpirationDate) = UNLIMITED_DATE Then btnExpiration.Checked = True ' D3352
                    ' D1050 ==
                    Dim InstanceID As Long = GetValue(ecLicenseParameter.InstanceID)
                    If InstanceID = 0 OrElse InstanceID = UNLIMITED_VALUE Then
                        cbInstanceID.Checked = False
                        tbInstanceID.Text = ""
                    Else
                        cbInstanceID.Checked = True
                        tbInstanceID.Text = InstanceID.ToString("X16")
                    End If

                    ' D1048 ===
                    UpdateExpiration()
                    UpdateOptionsList()
                    UpdateCheckboxes()
                    ' D1048 ==
                    SetCreatedAt(GetValue(ecLicenseParameter.CreatedAt))    ' D3965
                Else
                    MessageBox.Show("Key file corrupted or wrong license key", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)    ' D1050 + D2245
                End If
            End If
        End If
    End Sub

    ' D2245 ===
    Private Sub SetName(sName As String)
        If sFormName = "" Then sFormName = Text
        If sName = "" Then Text = sFormName Else Text = String.Format("{{{0}}} - {1}", My.Computer.FileSystem.GetName(sName), sFormName)
    End Sub
    ' D2245 ==

    ' D1048 ===
    Private Sub UpdateExpiration()
        dateExpire.Value = Date.FromBinary(GetValue(ecLicenseParameter.ExpirationDate))
        Dim Checked As Boolean = btnExpiration.Checked  ' D3352
        pnlAddPeriods.Visible = Not Checked
        pnlCommon.Height = CInt(IIf(Checked, 26, 47))
        pnlList.Top = pnlCommon.Bottom
        UpdateContainer()
    End Sub

    Private Sub UpdateOptionsList()
        pnlList.Controls.Clear()
        pnlList.Height = 0
        If btnSystem.Checked Then ShowSystemParams()
        If btnNonSystem.Checked Then ShowNonSystemParams()
        ' D3952 ===
        cbAllowUseGurobi.Enabled = btnNonSystem.Checked
        cbRiskTreatments.Enabled = btnNonSystem.Checked
        cbRisktreatmentsOptimization.Enabled = btnNonSystem.Checked
        ' D3952 ==
        tsMenu.Focus()
    End Sub

    Private Sub UpdateCheckboxes()
        cbTeamTime.Checked = GetValue(ecLicenseParameter.TeamTimeEnabled) = 1
        cbInsight.Checked = GetValue(ecLicenseParameter.SpyronEnabled) = 1
        cbResourceAligner.Checked = GetValue(ecLicenseParameter.ResourceAlignerEnabled) = 1
        cbExport.Checked = GetValue(ecLicenseParameter.ExportEnabled) = 1
        cbCommercial.Checked = GetValue(ecLicenseParameter.CommercialUseEnabled) = 1
        cbRisk.Checked = GetValue(ecLicenseParameter.RiskEnabled) = 1   ' D2056
        ' D3585 ===
        cbRiskTreatments.Checked = GetValue(ecLicenseParameter.RiskTreatments) = 1 AndAlso btnNonSystem.Checked ' D3952
        cbRisktreatmentsOptimization.Checked = GetValue(ecLicenseParameter.RiskTreatmentsOptimization) = 1 AndAlso btnNonSystem.Checked ' D3952
        CheckRiskOptions()
        ' D3585 ==
        cbAllowUseGurobi.Checked = GetValue(ecLicenseParameter.AllowUseGurobi) = 1 AndAlso btnNonSystem.Checked ' D3922 + D3952
        tbInstanceID.Enabled = cbInstanceID.Checked ' D3946
        cbIsSelfHost.Checked = GetValue(ecLicenseParameter.isSelfHost) = 1  ' D3965
        CheckRAOptions()    ' D3922
    End Sub

    Private Sub ShowSystemParams()
        AddNumber("Total workgroups count:", ecLicenseParameter.MaxWorkgroupsTotal, False)
        AddNumber("Total number of projects:", ecLicenseParameter.MaxProjectsTotal, False)
        ' D3952 ===
        If Not btnNonSystem.Checked Then
            AddNumber("Max lifetime projects count:", ecLicenseParameter.MaxLifetimeProjects, False)
            AddNumber("Max number of Participants in workgroup:", ecLicenseParameter.MaxUsersInWorkgroup, False)
        End If
        ' D3952 ==
    End Sub

    Private Sub ShowNonSystemParams()
        If Not btnSystem.Checked Then AddNumber("Max number of projects:", ecLicenseParameter.MaxProjectsTotal, False)
        AddNumber("Max lifetime projects count:", ecLicenseParameter.MaxLifetimeProjects, False)
        AddNumber("Max number of On-line projects:", ecLicenseParameter.MaxProjectsOnline, False)
        AddNumber("Max number of Participants in workgroup:", ecLicenseParameter.MaxUsersInWorkgroup, False) ' D1482 + D1642
        AddNumber("Max number of Project Organizers in wkg:", ecLicenseParameter.MaxProjectCreatorsInWorkgroup, False)  ' D3946
        AddNumber("Max number of Participants in project:", ecLicenseParameter.MaxUsersInProject, False)     ' D1482 + D1642
        AddNumber("Max number of Project Managers in project:", ecLicenseParameter.MaxPMsInProject, False)
        AddNumber("Max number of Objectives in project:", ecLicenseParameter.MaxObjectives, False)
        AddNumber("Max number of level below the Goal:", ecLicenseParameter.MaxLevelsBelowGoal, False)
        AddNumber("Max number of Alternatives in project:", ecLicenseParameter.MaxAlternatives, False)
    End Sub

    Private Sub AddNumber(ByVal sCaption As String, ByVal tParamID As ecLicenseParameter, ByVal fUnused As Boolean)
        Dim tValue As Long = GetValue(tParamID)

        Dim CommonHeight = pnlList.Height   ' D3585

        Dim lbl As New Label
        lbl.Text = sCaption
        lbl.Top = CommonHeight + 4
        lbl.AutoSize = True
        If fUnused Then lbl.Enabled = False
        Dim Val As New NumericUpDown
        ' D3352 ===
        Dim btn As New CheckBox
        btn.Appearance = btnExpiration.Appearance
        btn.TextAlign = btnExpiration.TextAlign
        btn.FlatStyle = btnExpiration.FlatStyle
        btn.Font = btnExpiration.Font
        btn.MinimumSize = btnExpiration.MinimumSize
        btn.Left = btnExpiration.Left
        btn.Top = CommonHeight
        btn.Size = btnExpiration.Size
        btn.Text = btnExpiration.Text
        btn.TextAlign = btnExpiration.TextAlign
        btn.Tag = Val
        ' D3352 ==
        Val.Width = 50
        Val.Left = btn.Left - Val.Width - 3 ' D2860
        Val.Minimum = -1
        Val.Maximum = Integer.MaxValue  ' D3140
        Val.Top = CommonHeight
        Val.Value = tValue
        Val.Tag = tParamID
        AddHandler Val.ValueChanged, AddressOf NumericUpDown_ValueChanged
        AddHandler btn.CheckedChanged, AddressOf btnExpiration_CheckedChanged ' D3352
        If tValue < 0 Then btn.Checked = True ' D3352
        pnlList.Controls.Add(lbl)
        pnlList.Controls.Add(Val)
        pnlList.Controls.Add(btn)
        pnlList.Height = btn.Bottom + 3 ' D3585
        'pnlCheckboxes.Top = pnlList.Bottom  ' D3585
        UpdateContainer()    ' D2224
    End Sub

    Private Sub UpdateContainer()
        'pnlCheckboxes.Height = CInt(IIf(cbRisk.Checked, cbRisktreatmentsOptimization.Bottom, cbRisk.Bottom)) + 2
        'pnlCheckboxes.Height = cbRisktreatmentsOptimization.Bottom + 2    ' D3922
        Me.Height = pnlList.Bottom + pnlCheckboxes.Height + pnlButtons.Height + 32
    End Sub

    Private Sub ToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSystem.Click, btnNonSystem.Click
        Dim btn As ToolStripButton = CType(sender, ToolStripButton)
        If Not btnNonSystem.Checked AndAlso Not btnSystem.Checked Then btn.Checked = True Else UpdateOptionsList()
    End Sub

    Private Sub NumericUpDown_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim NumDown As NumericUpDown = CType(sender, NumericUpDown)
        If NumDown.Tag IsNot Nothing Then SetValue(CType(NumDown.Tag, ecLicenseParameter), False, CLng(NumDown.Value))
    End Sub

    ' D3585 ===
    Private Sub CheckRiskOptions()
        cbRiskTreatments.Enabled = cbRisk.Checked AndAlso btnNonSystem.Checked ' D3922
        cbRisktreatmentsOptimization.Enabled = cbRisk.Checked AndAlso cbRiskTreatments.Checked AndAlso btnNonSystem.Checked ' D3591 + D3922
        UpdateContainer()
    End Sub
    ' D3585 ==

    ' D3922 ===
    Private Sub CheckRAOptions()
        cbAllowUseGurobi.Enabled = cbResourceAligner.Checked AndAlso btnNonSystem.Checked   ' D3952
        UpdateContainer()
    End Sub
    ' D3922 ==

    Private Sub CheckBox_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbTeamTime.CheckedChanged, cbInsight.CheckedChanged, cbResourceAligner.CheckedChanged, cbExport.CheckedChanged, cbCommercial.CheckedChanged, cbRisk.CheckedChanged, cbRiskTreatments.CheckedChanged, cbRisktreatmentsOptimization.CheckedChanged, cbAllowUseGurobi.CheckedChanged, cbIsSelfHost.CheckedChanged     ' D2056 + D3922 + D3965
        Dim chk As CheckBox = CType(sender, CheckBox)
        If chk.Tag IsNot Nothing Then SetValue(CType(chk.Tag, ecLicenseParameter), True, CLng(IIf(chk.Checked, 1, 0)))
        If sender Is cbRisk OrElse sender Is cbRiskTreatments OrElse sender Is cbRisktreatmentsOptimization Then CheckRiskOptions() ' D3591 + D3922
        If sender Is cbResourceAligner Then CheckRAOptions() ' D3922
    End Sub

    Private Sub lnkAdd1Day_LinkClicked(sender As Object, e As EventArgs) Handles lnkAdd1Day.Click
        dateExpire.Value = dateExpire.Value.AddDays(1)
    End Sub

    Private Sub lnkAdd1Week_LinkClicked(sender As Object, e As EventArgs) Handles lnkAdd1Week.Click
        dateExpire.Value = dateExpire.Value.AddDays(7)
    End Sub

    Private Sub lnkAdd1Month_LinkClicked(sender As Object, e As EventArgs) Handles lnkAdd1Month.Click
        dateExpire.Value = dateExpire.Value.AddMonths(1)
    End Sub

    Private Sub lnkAdd3Months_LinkClicked(sender As Object, e As EventArgs) Handles lnkAdd3Months.Click
        dateExpire.Value = dateExpire.Value.AddMonths(3)
    End Sub

    Private Sub lnkAdd6Months_LinkClicked(sender As Object, e As EventArgs) Handles lnkAdd6Months.Click
        dateExpire.Value = dateExpire.Value.AddMonths(6)
    End Sub

    Private Sub lnkAdd1Year_LinkClicked(sender As Object, e As EventArgs) Handles lnkAdd1Year.Click
        dateExpire.Value = dateExpire.Value.AddYears(1)
    End Sub

    Private Sub dateExpire_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dateExpire.ValueChanged
        SetValue(ecLicenseParameter.ExpirationDate, False, dateExpire.Value.ToBinary)
    End Sub
    ' D1048 ===

    ' D3352 ===
    Private Sub frmMain_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        btnLoad.Focus()
    End Sub

    Private Sub UpdateButton(btn As CheckBox)
        ' D1050 ==
        Dim ctrl As NumericUpDown = CType(btn.Tag, NumericUpDown)
        ctrl.Enabled = Not btn.Checked
        ' D1050 ===
        ctrl.Visible = Not btn.Checked
        If ctrl.Enabled Then
            ctrl.Focus()
            If ctrl.Value = -1 AndAlso ctrl.Tag IsNot Nothing Then ctrl.Value = 1
        Else
            'ctrl.Value = -1
            SetValue(CType(ctrl.Tag, ecLicenseParameter), False, -1)    ' D3352
        End If
    End Sub

    Private Sub btnExpiration_CheckedChanged(sender As Object, e As System.EventArgs) Handles btnExpiration.CheckedChanged
        Dim btn As CheckBox = CType(sender, CheckBox)   ' D3352
        If btn.Tag IsNot Nothing Then
            ' D1050 ===
            If btn Is btnExpiration Then
                dateExpire.Visible = Not btn.Checked
                If dateExpire.Visible Then
                    If dateExpire.Value.ToBinary >= UNLIMITED_DATE Then dateExpire.Value = Now
                    dateExpire.Focus()
                End If
                UpdateExpiration()
            Else
                UpdateButton(btn)
            End If
            ' D1050 ==
        End If
    End Sub
    ' D3352 ==

    ' D3946 ===
    Private Sub cbInstanceID_CheckedChanged(sender As Object, e As EventArgs) Handles cbInstanceID.CheckedChanged
        UpdateCheckboxes()
        If cbInstanceID.Checked AndAlso tbInstanceID.CanFocus Then tbInstanceID.Focus()
    End Sub

    ' -D3952
    'Function isValidInstanceID(sValue As String) As Boolean
    '    Dim sID As String = sValue.Trim.Replace("-", "")
    '    If sID = "" Then Return True
    '    Try
    '        Dim tRes As Long
    '        Return Long.TryParse(sID, System.Globalization.NumberStyles.AllowHexSpecifier, Nothing, tRes)
    '    Catch ex As Exception
    '        Return False
    '    End Try
    'End Function

    Private Sub tbInstanceID_TextChanged(sender As Object, e As EventArgs) Handles tbInstanceID.TextChanged
        Dim sValue As String = tbInstanceID.Text.Trim
        If sValue = "" OrElse sValue = "0" OrElse sValue = UNLIMITED_VALUE.ToString OrElse isValidInstanceID(sValue) Then tbInstanceID.ForeColor = System.Drawing.SystemColors.WindowText Else tbInstanceID.ForeColor = Color.DarkRed
    End Sub

    Private Sub tbInstanceID_KeyDown(sender As Object, e As KeyEventArgs) Handles tbInstanceID.KeyDown
        If e.Modifiers = Keys.None AndAlso Not System.Uri.IsHexDigit(ChrW(e.KeyCode)) Then e.SuppressKeyPress = True
    End Sub
    ' D3946 ==

End Class