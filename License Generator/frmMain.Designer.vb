<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.dlgOpen = New System.Windows.Forms.OpenFileDialog()
        Me.pnlList = New System.Windows.Forms.Panel()
        Me.tsMenu = New System.Windows.Forms.ToolStrip()
        Me.txtWkgType = New System.Windows.Forms.ToolStripLabel()
        Me.btnSystem = New System.Windows.Forms.ToolStripButton()
        Me.btnNonSystem = New System.Windows.Forms.ToolStripButton()
        Me.pnlButtons = New System.Windows.Forms.Panel()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnLoad = New System.Windows.Forms.Button()
        Me.pnlCommon = New System.Windows.Forms.Panel()
        Me.btnExpiration = New System.Windows.Forms.CheckBox()
        Me.dateExpire = New System.Windows.Forms.DateTimePicker()
        Me.lblExpired = New System.Windows.Forms.Label()
        Me.pnlCheckboxes = New System.Windows.Forms.Panel()
        Me.cbIsSelfHost = New System.Windows.Forms.CheckBox()
        Me.tbInstanceID = New System.Windows.Forms.MaskedTextBox()
        Me.cbInstanceID = New System.Windows.Forms.CheckBox()
        Me.cbAllowUseGurobi = New System.Windows.Forms.CheckBox()
        Me.cbRisktreatmentsOptimization = New System.Windows.Forms.CheckBox()
        Me.cbRiskTreatments = New System.Windows.Forms.CheckBox()
        Me.cbRisk = New System.Windows.Forms.CheckBox()
        Me.cbCommercial = New System.Windows.Forms.CheckBox()
        Me.cbExport = New System.Windows.Forms.CheckBox()
        Me.cbResourceAligner = New System.Windows.Forms.CheckBox()
        Me.cbInsight = New System.Windows.Forms.CheckBox()
        Me.cbTeamTime = New System.Windows.Forms.CheckBox()
        Me.dlgSave = New System.Windows.Forms.SaveFileDialog()
        Me.pnlAddPeriods = New System.Windows.Forms.Panel()
        Me.lnkAdd1Month = New System.Windows.Forms.LinkLabel()
        Me.lnkAdd1Day = New System.Windows.Forms.LinkLabel()
        Me.lnkAdd1Year = New System.Windows.Forms.LinkLabel()
        Me.lnkAdd6Months = New System.Windows.Forms.LinkLabel()
        Me.lnkAdd3Months = New System.Windows.Forms.LinkLabel()
        Me.lnkAdd1Week = New System.Windows.Forms.LinkLabel()
        Me.tsMenu.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.pnlCommon.SuspendLayout()
        Me.pnlCheckboxes.SuspendLayout()
        Me.pnlAddPeriods.SuspendLayout()
        Me.SuspendLayout()
        '
        'dlgOpen
        '
        Me.dlgOpen.DefaultExt = "lic"
        Me.dlgOpen.Filter = "License files (*.lic)|*.lic|All files (*.*)|*.*"
        Me.dlgOpen.RestoreDirectory = True
        Me.dlgOpen.Title = "Choose the license file"
        '
        'pnlList
        '
        Me.pnlList.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlList.Location = New System.Drawing.Point(13, 81)
        Me.pnlList.Name = "pnlList"
        Me.pnlList.Size = New System.Drawing.Size(315, 18)
        Me.pnlList.TabIndex = 39
        '
        'tsMenu
        '
        Me.tsMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.txtWkgType, Me.btnSystem, Me.btnNonSystem})
        Me.tsMenu.Location = New System.Drawing.Point(0, 0)
        Me.tsMenu.Margin = New System.Windows.Forms.Padding(3, 2, 0, 0)
        Me.tsMenu.Name = "tsMenu"
        Me.tsMenu.Padding = New System.Windows.Forms.Padding(0, 2, 1, 0)
        Me.tsMenu.Size = New System.Drawing.Size(340, 25)
        Me.tsMenu.TabIndex = 40
        Me.tsMenu.Text = "ToolStripMain"
        '
        'txtWkgType
        '
        Me.txtWkgType.Name = "txtWkgType"
        Me.txtWkgType.Size = New System.Drawing.Size(96, 20)
        Me.txtWkgType.Text = "Workgroup type:"
        '
        'btnSystem
        '
        Me.btnSystem.Checked = True
        Me.btnSystem.CheckOnClick = True
        Me.btnSystem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.btnSystem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnSystem.Image = CType(resources.GetObject("btnSystem.Image"), System.Drawing.Image)
        Me.btnSystem.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnSystem.Margin = New System.Windows.Forms.Padding(1, 1, 1, 2)
        Me.btnSystem.Name = "btnSystem"
        Me.btnSystem.Size = New System.Drawing.Size(49, 20)
        Me.btnSystem.Text = "System"
        '
        'btnNonSystem
        '
        Me.btnNonSystem.Checked = True
        Me.btnNonSystem.CheckOnClick = True
        Me.btnNonSystem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.btnNonSystem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnNonSystem.Image = CType(resources.GetObject("btnNonSystem.Image"), System.Drawing.Image)
        Me.btnNonSystem.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnNonSystem.Margin = New System.Windows.Forms.Padding(1, 1, 1, 2)
        Me.btnNonSystem.Name = "btnNonSystem"
        Me.btnNonSystem.Size = New System.Drawing.Size(51, 20)
        Me.btnNonSystem.Text = "Regular"
        '
        'pnlButtons
        '
        Me.pnlButtons.Controls.Add(Me.btnSave)
        Me.pnlButtons.Controls.Add(Me.btnLoad)
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.Location = New System.Drawing.Point(0, 324)
        Me.pnlButtons.Name = "pnlButtons"
        Me.pnlButtons.Size = New System.Drawing.Size(340, 44)
        Me.pnlButtons.TabIndex = 41
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(244, 10)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(87, 22)
        Me.btnSave.TabIndex = 100
        Me.btnSave.Text = "Save…"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnLoad
        '
        Me.btnLoad.Location = New System.Drawing.Point(151, 10)
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.Size = New System.Drawing.Size(87, 22)
        Me.btnLoad.TabIndex = 99
        Me.btnLoad.Text = "Load…"
        Me.btnLoad.UseVisualStyleBackColor = True
        '
        'pnlCommon
        '
        Me.pnlCommon.Controls.Add(Me.btnExpiration)
        Me.pnlCommon.Controls.Add(Me.dateExpire)
        Me.pnlCommon.Controls.Add(Me.lblExpired)
        Me.pnlCommon.Location = New System.Drawing.Point(13, 34)
        Me.pnlCommon.Name = "pnlCommon"
        Me.pnlCommon.Size = New System.Drawing.Size(318, 28)
        Me.pnlCommon.TabIndex = 42
        '
        'btnExpiration
        '
        Me.btnExpiration.Appearance = System.Windows.Forms.Appearance.Button
        Me.btnExpiration.AutoSize = True
        Me.btnExpiration.FlatAppearance.CheckedBackColor = System.Drawing.Color.Gray
        Me.btnExpiration.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.btnExpiration.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.btnExpiration.Location = New System.Drawing.Point(265, 1)
        Me.btnExpiration.MaximumSize = New System.Drawing.Size(60, 26)
        Me.btnExpiration.MinimumSize = New System.Drawing.Size(48, 20)
        Me.btnExpiration.Name = "btnExpiration"
        Me.btnExpiration.Size = New System.Drawing.Size(48, 23)
        Me.btnExpiration.TabIndex = 2
        Me.btnExpiration.Text = "Unlim"
        Me.btnExpiration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.btnExpiration.UseVisualStyleBackColor = True
        '
        'dateExpire
        '
        Me.dateExpire.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dateExpire.Location = New System.Drawing.Point(135, 2)
        Me.dateExpire.MaxDate = New Date(9998, 1, 2, 0, 0, 0, 0)
        Me.dateExpire.Name = "dateExpire"
        Me.dateExpire.Size = New System.Drawing.Size(124, 20)
        Me.dateExpire.TabIndex = 1
        '
        'lblExpired
        '
        Me.lblExpired.AutoSize = True
        Me.lblExpired.Location = New System.Drawing.Point(1, 6)
        Me.lblExpired.Name = "lblExpired"
        Me.lblExpired.Size = New System.Drawing.Size(99, 13)
        Me.lblExpired.TabIndex = 26
        Me.lblExpired.Text = "License Expires on:"
        '
        'pnlCheckboxes
        '
        Me.pnlCheckboxes.Controls.Add(Me.cbIsSelfHost)
        Me.pnlCheckboxes.Controls.Add(Me.tbInstanceID)
        Me.pnlCheckboxes.Controls.Add(Me.cbInstanceID)
        Me.pnlCheckboxes.Controls.Add(Me.cbAllowUseGurobi)
        Me.pnlCheckboxes.Controls.Add(Me.cbRisktreatmentsOptimization)
        Me.pnlCheckboxes.Controls.Add(Me.cbRiskTreatments)
        Me.pnlCheckboxes.Controls.Add(Me.cbRisk)
        Me.pnlCheckboxes.Controls.Add(Me.cbCommercial)
        Me.pnlCheckboxes.Controls.Add(Me.cbExport)
        Me.pnlCheckboxes.Controls.Add(Me.cbResourceAligner)
        Me.pnlCheckboxes.Controls.Add(Me.cbInsight)
        Me.pnlCheckboxes.Controls.Add(Me.cbTeamTime)
        Me.pnlCheckboxes.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlCheckboxes.Location = New System.Drawing.Point(0, 88)
        Me.pnlCheckboxes.Name = "pnlCheckboxes"
        Me.pnlCheckboxes.Padding = New System.Windows.Forms.Padding(10, 0, 0, 0)
        Me.pnlCheckboxes.Size = New System.Drawing.Size(340, 236)
        Me.pnlCheckboxes.TabIndex = 40
        '
        'cbIsSelfHost
        '
        Me.cbIsSelfHost.AutoSize = True
        Me.cbIsSelfHost.Location = New System.Drawing.Point(17, 187)
        Me.cbIsSelfHost.Name = "cbIsSelfHost"
        Me.cbIsSelfHost.Size = New System.Drawing.Size(112, 17)
        Me.cbIsSelfHost.TabIndex = 60
        Me.cbIsSelfHost.Tag = "28"
        Me.cbIsSelfHost.Text = "Self-Host instance"
        Me.cbIsSelfHost.UseVisualStyleBackColor = True
        '
        'tbInstanceID
        '
        Me.tbInstanceID.Culture = New System.Globalization.CultureInfo("")
        Me.tbInstanceID.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals
        Me.tbInstanceID.Enabled = False
        Me.tbInstanceID.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.tbInstanceID.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite
        Me.tbInstanceID.Location = New System.Drawing.Point(189, 204)
        Me.tbInstanceID.Mask = ">A>A>A>A>A>A>A>A->A>A>A>A>A>A>A>A"
        Me.tbInstanceID.Name = "tbInstanceID"
        Me.tbInstanceID.PromptChar = Global.Microsoft.VisualBasic.ChrW(48)
        Me.tbInstanceID.Size = New System.Drawing.Size(136, 21)
        Me.tbInstanceID.TabIndex = 59
        Me.tbInstanceID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.tbInstanceID.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals
        '
        'cbInstanceID
        '
        Me.cbInstanceID.AutoSize = True
        Me.cbInstanceID.Location = New System.Drawing.Point(17, 207)
        Me.cbInstanceID.Name = "cbInstanceID"
        Me.cbInstanceID.Size = New System.Drawing.Size(165, 17)
        Me.cbInstanceID.TabIndex = 57
        Me.cbInstanceID.Tag = "27"
        Me.cbInstanceID.Text = "InstanceID [V{Server}-{DB}] :"
        Me.cbInstanceID.UseVisualStyleBackColor = True
        '
        'cbAllowUseGurobi
        '
        Me.cbAllowUseGurobi.AutoSize = True
        Me.cbAllowUseGurobi.Location = New System.Drawing.Point(37, 67)
        Me.cbAllowUseGurobi.Name = "cbAllowUseGurobi"
        Me.cbAllowUseGurobi.Size = New System.Drawing.Size(126, 17)
        Me.cbAllowUseGurobi.TabIndex = 56
        Me.cbAllowUseGurobi.Tag = "26"
        Me.cbAllowUseGurobi.Text = "Allow to use Solver B"
        Me.cbAllowUseGurobi.UseVisualStyleBackColor = True
        '
        'cbRisktreatmentsOptimization
        '
        Me.cbRisktreatmentsOptimization.AutoSize = True
        Me.cbRisktreatmentsOptimization.Location = New System.Drawing.Point(37, 167)
        Me.cbRisktreatmentsOptimization.Name = "cbRisktreatmentsOptimization"
        Me.cbRisktreatmentsOptimization.Size = New System.Drawing.Size(139, 17)
        Me.cbRisktreatmentsOptimization.TabIndex = 55
        Me.cbRisktreatmentsOptimization.Tag = "25"
        Me.cbRisktreatmentsOptimization.Text = "Treatments Optimization"
        Me.cbRisktreatmentsOptimization.UseVisualStyleBackColor = True
        '
        'cbRiskTreatments
        '
        Me.cbRiskTreatments.AutoSize = True
        Me.cbRiskTreatments.Location = New System.Drawing.Point(37, 147)
        Me.cbRiskTreatments.Name = "cbRiskTreatments"
        Me.cbRiskTreatments.Size = New System.Drawing.Size(79, 17)
        Me.cbRiskTreatments.TabIndex = 55
        Me.cbRiskTreatments.Tag = "24"
        Me.cbRiskTreatments.Text = "Treatments"
        Me.cbRiskTreatments.UseVisualStyleBackColor = True
        '
        'cbRisk
        '
        Me.cbRisk.AutoSize = True
        Me.cbRisk.Location = New System.Drawing.Point(17, 127)
        Me.cbRisk.Name = "cbRisk"
        Me.cbRisk.Size = New System.Drawing.Size(101, 17)
        Me.cbRisk.TabIndex = 55
        Me.cbRisk.Tag = "23"
        Me.cbRisk.Text = "Riskion License"
        Me.cbRisk.UseVisualStyleBackColor = True
        '
        'cbCommercial
        '
        Me.cbCommercial.AutoSize = True
        Me.cbCommercial.Location = New System.Drawing.Point(17, 107)
        Me.cbCommercial.Name = "cbCommercial"
        Me.cbCommercial.Size = New System.Drawing.Size(145, 17)
        Me.cbCommercial.TabIndex = 54
        Me.cbCommercial.Tag = "15"
        Me.cbCommercial.Text = "Commercial use available"
        Me.cbCommercial.UseVisualStyleBackColor = True
        '
        'cbExport
        '
        Me.cbExport.AutoSize = True
        Me.cbExport.Location = New System.Drawing.Point(17, 87)
        Me.cbExport.Name = "cbExport"
        Me.cbExport.Size = New System.Drawing.Size(143, 17)
        Me.cbExport.TabIndex = 53
        Me.cbExport.Tag = "14"
        Me.cbExport.Text = "Export functions enabled"
        Me.cbExport.UseVisualStyleBackColor = True
        '
        'cbResourceAligner
        '
        Me.cbResourceAligner.AutoSize = True
        Me.cbResourceAligner.Location = New System.Drawing.Point(17, 47)
        Me.cbResourceAligner.Name = "cbResourceAligner"
        Me.cbResourceAligner.Size = New System.Drawing.Size(148, 17)
        Me.cbResourceAligner.TabIndex = 52
        Me.cbResourceAligner.Tag = "12"
        Me.cbResourceAligner.Text = "Resource Aligner enabled"
        Me.cbResourceAligner.UseVisualStyleBackColor = True
        '
        'cbInsight
        '
        Me.cbInsight.AutoSize = True
        Me.cbInsight.Location = New System.Drawing.Point(17, 27)
        Me.cbInsight.Name = "cbInsight"
        Me.cbInsight.Size = New System.Drawing.Size(107, 17)
        Me.cbInsight.TabIndex = 51
        Me.cbInsight.Tag = "11"
        Me.cbInsight.Text = "Insight™ enabled"
        Me.cbInsight.UseVisualStyleBackColor = True
        '
        'cbTeamTime
        '
        Me.cbTeamTime.AutoSize = True
        Me.cbTeamTime.Location = New System.Drawing.Point(17, 7)
        Me.cbTeamTime.Name = "cbTeamTime"
        Me.cbTeamTime.Size = New System.Drawing.Size(126, 17)
        Me.cbTeamTime.TabIndex = 50
        Me.cbTeamTime.Tag = "9"
        Me.cbTeamTime.Text = "TeamTime™ enabled"
        Me.cbTeamTime.UseVisualStyleBackColor = True
        '
        'dlgSave
        '
        Me.dlgSave.DefaultExt = "lic"
        Me.dlgSave.Filter = "License files (*.lic)|*.lic|All files (*.*)|*.*"
        Me.dlgSave.RestoreDirectory = True
        Me.dlgSave.Title = "Choose the license file"
        '
        'pnlAddPeriods
        '
        Me.pnlAddPeriods.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlAddPeriods.Controls.Add(Me.lnkAdd1Month)
        Me.pnlAddPeriods.Controls.Add(Me.lnkAdd1Day)
        Me.pnlAddPeriods.Controls.Add(Me.lnkAdd1Year)
        Me.pnlAddPeriods.Controls.Add(Me.lnkAdd6Months)
        Me.pnlAddPeriods.Controls.Add(Me.lnkAdd3Months)
        Me.pnlAddPeriods.Controls.Add(Me.lnkAdd1Week)
        Me.pnlAddPeriods.Location = New System.Drawing.Point(17, 61)
        Me.pnlAddPeriods.Name = "pnlAddPeriods"
        Me.pnlAddPeriods.Size = New System.Drawing.Size(311, 20)
        Me.pnlAddPeriods.TabIndex = 43
        '
        'lnkAdd1Month
        '
        Me.lnkAdd1Month.AutoSize = True
        Me.lnkAdd1Month.Location = New System.Drawing.Point(94, 0)
        Me.lnkAdd1Month.Name = "lnkAdd1Month"
        Me.lnkAdd1Month.Size = New System.Drawing.Size(51, 13)
        Me.lnkAdd1Month.TabIndex = 5
        Me.lnkAdd1Month.TabStop = True
        Me.lnkAdd1Month.Text = "+1 month"
        '
        'lnkAdd1Day
        '
        Me.lnkAdd1Day.AutoSize = True
        Me.lnkAdd1Day.Location = New System.Drawing.Point(1, 0)
        Me.lnkAdd1Day.Name = "lnkAdd1Day"
        Me.lnkAdd1Day.Size = New System.Drawing.Size(39, 13)
        Me.lnkAdd1Day.TabIndex = 3
        Me.lnkAdd1Day.TabStop = True
        Me.lnkAdd1Day.Text = "+1 day"
        '
        'lnkAdd1Year
        '
        Me.lnkAdd1Year.AutoSize = True
        Me.lnkAdd1Year.Location = New System.Drawing.Point(266, 0)
        Me.lnkAdd1Year.Name = "lnkAdd1Year"
        Me.lnkAdd1Year.Size = New System.Drawing.Size(42, 13)
        Me.lnkAdd1Year.TabIndex = 8
        Me.lnkAdd1Year.TabStop = True
        Me.lnkAdd1Year.Text = "+1 year"
        '
        'lnkAdd6Months
        '
        Me.lnkAdd6Months.AutoSize = True
        Me.lnkAdd6Months.Location = New System.Drawing.Point(207, 0)
        Me.lnkAdd6Months.Name = "lnkAdd6Months"
        Me.lnkAdd6Months.Size = New System.Drawing.Size(56, 13)
        Me.lnkAdd6Months.TabIndex = 7
        Me.lnkAdd6Months.TabStop = True
        Me.lnkAdd6Months.Text = "+6 months"
        '
        'lnkAdd3Months
        '
        Me.lnkAdd3Months.AutoSize = True
        Me.lnkAdd3Months.Location = New System.Drawing.Point(148, 0)
        Me.lnkAdd3Months.Name = "lnkAdd3Months"
        Me.lnkAdd3Months.Size = New System.Drawing.Size(56, 13)
        Me.lnkAdd3Months.TabIndex = 6
        Me.lnkAdd3Months.TabStop = True
        Me.lnkAdd3Months.Text = "+3 months"
        '
        'lnkAdd1Week
        '
        Me.lnkAdd1Week.AutoSize = True
        Me.lnkAdd1Week.Location = New System.Drawing.Point(43, 0)
        Me.lnkAdd1Week.Name = "lnkAdd1Week"
        Me.lnkAdd1Week.Size = New System.Drawing.Size(48, 13)
        Me.lnkAdd1Week.TabIndex = 4
        Me.lnkAdd1Week.TabStop = True
        Me.lnkAdd1Week.Text = "+1 week"
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(340, 368)
        Me.Controls.Add(Me.pnlAddPeriods)
        Me.Controls.Add(Me.pnlCheckboxes)
        Me.Controls.Add(Me.pnlCommon)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.tsMenu)
        Me.Controls.Add(Me.pnlList)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "frmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Core License Generator [4.0.3965]"
        Me.tsMenu.ResumeLayout(False)
        Me.tsMenu.PerformLayout()
        Me.pnlButtons.ResumeLayout(False)
        Me.pnlCommon.ResumeLayout(False)
        Me.pnlCommon.PerformLayout()
        Me.pnlCheckboxes.ResumeLayout(False)
        Me.pnlCheckboxes.PerformLayout()
        Me.pnlAddPeriods.ResumeLayout(False)
        Me.pnlAddPeriods.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents dlgOpen As System.Windows.Forms.OpenFileDialog
    Friend WithEvents pnlList As System.Windows.Forms.Panel
    Friend WithEvents tsMenu As System.Windows.Forms.ToolStrip
    Friend WithEvents txtWkgType As System.Windows.Forms.ToolStripLabel
    Friend WithEvents btnSystem As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnNonSystem As System.Windows.Forms.ToolStripButton
    Friend WithEvents pnlButtons As System.Windows.Forms.Panel
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnLoad As System.Windows.Forms.Button
    Friend WithEvents pnlCommon As System.Windows.Forms.Panel
    Friend WithEvents dateExpire As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblExpired As System.Windows.Forms.Label
    Friend WithEvents pnlCheckboxes As System.Windows.Forms.Panel
    Friend WithEvents cbInsight As System.Windows.Forms.CheckBox
    Friend WithEvents cbTeamTime As System.Windows.Forms.CheckBox
    Friend WithEvents cbExport As System.Windows.Forms.CheckBox
    Friend WithEvents cbResourceAligner As System.Windows.Forms.CheckBox
    Friend WithEvents cbCommercial As System.Windows.Forms.CheckBox
    Friend WithEvents dlgSave As System.Windows.Forms.SaveFileDialog
    Friend WithEvents cbRisk As System.Windows.Forms.CheckBox
    Friend WithEvents btnExpiration As System.Windows.Forms.CheckBox
    Friend WithEvents cbRiskTreatments As System.Windows.Forms.CheckBox
    Friend WithEvents cbRisktreatmentsOptimization As System.Windows.Forms.CheckBox
    Friend WithEvents pnlAddPeriods As System.Windows.Forms.Panel
    Friend WithEvents lnkAdd1Month As System.Windows.Forms.LinkLabel
    Friend WithEvents lnkAdd1Day As System.Windows.Forms.LinkLabel
    Friend WithEvents lnkAdd1Year As System.Windows.Forms.LinkLabel
    Friend WithEvents lnkAdd6Months As System.Windows.Forms.LinkLabel
    Friend WithEvents lnkAdd3Months As System.Windows.Forms.LinkLabel
    Friend WithEvents lnkAdd1Week As System.Windows.Forms.LinkLabel
    Friend WithEvents cbAllowUseGurobi As System.Windows.Forms.CheckBox
    Friend WithEvents cbInstanceID As System.Windows.Forms.CheckBox
    Friend WithEvents tbInstanceID As System.Windows.Forms.MaskedTextBox
    Friend WithEvents cbIsSelfHost As System.Windows.Forms.CheckBox
    'Friend WithEvents ShapeContainer1 As Microsoft.VisualBasic.PowerPacks.ShapeContainer
    'Friend WithEvents LineShape1 As Microsoft.VisualBasic.PowerPacks.LineShape

End Class