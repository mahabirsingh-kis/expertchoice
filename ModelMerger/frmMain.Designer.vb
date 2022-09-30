<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label
        Me.dgModels = New System.Windows.Forms.DataGridView
        Me.colFileName = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.colFilePath = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.dgUsers = New System.Windows.Forms.DataGridView
        Me.colUserID = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.colUserName = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.colUserEmail = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.colIncludeInMerge = New System.Windows.Forms.DataGridViewCheckBoxColumn
        Me.colHasData = New System.Windows.Forms.DataGridViewCheckBoxColumn
        Me.Label2 = New System.Windows.Forms.Label
        Me.btnOpenModels = New System.Windows.Forms.Button
        Me.dlgOpen = New System.Windows.Forms.OpenFileDialog
        Me.btnSetMasterModel = New System.Windows.Forms.Button
        Me.btnRemoveModel = New System.Windows.Forms.Button
        Me.btnClearAll = New System.Windows.Forms.Button
        Me.btnAnalyze = New System.Windows.Forms.Button
        Me.btnMerge = New System.Windows.Forms.Button
        Me.tbLog = New System.Windows.Forms.TextBox
        Me.tbMerge = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        CType(Me.dgModels, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgUsers, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.Label1.Location = New System.Drawing.Point(1, 67)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(88, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Selected files:"
        '
        'dgModels
        '
        Me.dgModels.AllowUserToAddRows = False
        Me.dgModels.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgModels.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colFileName, Me.colFilePath})
        Me.dgModels.Location = New System.Drawing.Point(4, 99)
        Me.dgModels.MultiSelect = False
        Me.dgModels.Name = "dgModels"
        Me.dgModels.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgModels.Size = New System.Drawing.Size(695, 162)
        Me.dgModels.TabIndex = 1
        '
        'colFileName
        '
        Me.colFileName.HeaderText = "File Name"
        Me.colFileName.Name = "colFileName"
        Me.colFileName.Width = 300
        '
        'colFilePath
        '
        Me.colFilePath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colFilePath.HeaderText = "File Path"
        Me.colFilePath.Name = "colFilePath"
        '
        'dgUsers
        '
        Me.dgUsers.AllowUserToAddRows = False
        Me.dgUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgUsers.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colUserID, Me.colUserName, Me.colUserEmail, Me.colIncludeInMerge, Me.colHasData})
        Me.dgUsers.Location = New System.Drawing.Point(4, 282)
        Me.dgUsers.Name = "dgUsers"
        Me.dgUsers.Size = New System.Drawing.Size(695, 150)
        Me.dgUsers.TabIndex = 2
        '
        'colUserID
        '
        Me.colUserID.HeaderText = "UserID"
        Me.colUserID.Name = "colUserID"
        Me.colUserID.ReadOnly = True
        '
        'colUserName
        '
        Me.colUserName.HeaderText = "User Name"
        Me.colUserName.Name = "colUserName"
        Me.colUserName.Width = 150
        '
        'colUserEmail
        '
        Me.colUserEmail.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colUserEmail.HeaderText = "User Email"
        Me.colUserEmail.Name = "colUserEmail"
        '
        'colIncludeInMerge
        '
        Me.colIncludeInMerge.HeaderText = "Include in Merge"
        Me.colIncludeInMerge.Name = "colIncludeInMerge"
        '
        'colHasData
        '
        Me.colHasData.HeaderText = "Has Data"
        Me.colHasData.Name = "colHasData"
        Me.colHasData.ReadOnly = True
        Me.colHasData.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colHasData.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.Label2.Location = New System.Drawing.Point(1, 264)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(146, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Users in selected model:"
        '
        'btnOpenModels
        '
        Me.btnOpenModels.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.btnOpenModels.Location = New System.Drawing.Point(4, 9)
        Me.btnOpenModels.Name = "btnOpenModels"
        Me.btnOpenModels.Size = New System.Drawing.Size(132, 23)
        Me.btnOpenModels.TabIndex = 4
        Me.btnOpenModels.Text = "Add models..."
        Me.btnOpenModels.UseVisualStyleBackColor = True
        '
        'dlgOpen
        '
        Me.dlgOpen.DefaultExt = "ahp"
        Me.dlgOpen.Filter = "EC Desktop File (*.ahp)|*.ahp"
        Me.dlgOpen.Multiselect = True
        '
        'btnSetMasterModel
        '
        Me.btnSetMasterModel.Location = New System.Drawing.Point(142, 9)
        Me.btnSetMasterModel.Name = "btnSetMasterModel"
        Me.btnSetMasterModel.Size = New System.Drawing.Size(160, 23)
        Me.btnSetMasterModel.TabIndex = 5
        Me.btnSetMasterModel.Text = "Set Selected Model as Master"
        Me.btnSetMasterModel.UseVisualStyleBackColor = True
        '
        'btnRemoveModel
        '
        Me.btnRemoveModel.Location = New System.Drawing.Point(309, 9)
        Me.btnRemoveModel.Name = "btnRemoveModel"
        Me.btnRemoveModel.Size = New System.Drawing.Size(142, 23)
        Me.btnRemoveModel.TabIndex = 6
        Me.btnRemoveModel.Text = "Remove Selected Model"
        Me.btnRemoveModel.UseVisualStyleBackColor = True
        '
        'btnClearAll
        '
        Me.btnClearAll.Location = New System.Drawing.Point(553, 9)
        Me.btnClearAll.Name = "btnClearAll"
        Me.btnClearAll.Size = New System.Drawing.Size(146, 23)
        Me.btnClearAll.TabIndex = 7
        Me.btnClearAll.Text = "Clear All"
        Me.btnClearAll.UseVisualStyleBackColor = True
        '
        'btnAnalyze
        '
        Me.btnAnalyze.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.btnAnalyze.Location = New System.Drawing.Point(4, 438)
        Me.btnAnalyze.Name = "btnAnalyze"
        Me.btnAnalyze.Size = New System.Drawing.Size(75, 23)
        Me.btnAnalyze.TabIndex = 8
        Me.btnAnalyze.Text = "Analyze"
        Me.btnAnalyze.UseVisualStyleBackColor = True
        '
        'btnMerge
        '
        Me.btnMerge.Enabled = False
        Me.btnMerge.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.btnMerge.Location = New System.Drawing.Point(4, 540)
        Me.btnMerge.Name = "btnMerge"
        Me.btnMerge.Size = New System.Drawing.Size(75, 23)
        Me.btnMerge.TabIndex = 9
        Me.btnMerge.Text = "Merge!"
        Me.btnMerge.UseVisualStyleBackColor = True
        '
        'tbLog
        '
        Me.tbLog.Location = New System.Drawing.Point(4, 469)
        Me.tbLog.Multiline = True
        Me.tbLog.Name = "tbLog"
        Me.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.tbLog.Size = New System.Drawing.Size(695, 65)
        Me.tbLog.TabIndex = 10
        '
        'tbMerge
        '
        Me.tbMerge.Location = New System.Drawing.Point(4, 569)
        Me.tbMerge.Multiline = True
        Me.tbMerge.Name = "tbMerge"
        Me.tbMerge.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.tbMerge.Size = New System.Drawing.Size(695, 75)
        Me.tbMerge.TabIndex = 11
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.Red
        Me.Label3.Location = New System.Drawing.Point(97, 443)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(388, 13)
        Me.Label3.TabIndex = 12
        Me.Label3.Text = "Please click Merge button ONLY after Analyze is ""Ready to merge"""
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.Red
        Me.Label4.Location = New System.Drawing.Point(97, 67)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(614, 26)
        Me.Label4.TabIndex = 13
        Me.Label4.Text = "Please make sure you have master model (model to merge to) selected (it will be highlighted with light blue)" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Use ""Set Selected Model as Master"" button above to " & _
            "change default master model."
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(711, 656)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.tbMerge)
        Me.Controls.Add(Me.tbLog)
        Me.Controls.Add(Me.btnMerge)
        Me.Controls.Add(Me.btnAnalyze)
        Me.Controls.Add(Me.btnClearAll)
        Me.Controls.Add(Me.btnRemoveModel)
        Me.Controls.Add(Me.btnSetMasterModel)
        Me.Controls.Add(Me.btnOpenModels)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.dgUsers)
        Me.Controls.Add(Me.dgModels)
        Me.Controls.Add(Me.Label1)
        Me.Name = "frmMain"
        Me.Text = "EC Desktop Model Merger"
        CType(Me.dgModels, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgUsers, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents dgModels As System.Windows.Forms.DataGridView
    Friend WithEvents colFileName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colFilePath As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgUsers As System.Windows.Forms.DataGridView
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents btnOpenModels As System.Windows.Forms.Button
    Friend WithEvents dlgOpen As System.Windows.Forms.OpenFileDialog
    Friend WithEvents btnSetMasterModel As System.Windows.Forms.Button
    Friend WithEvents btnRemoveModel As System.Windows.Forms.Button
    Friend WithEvents btnClearAll As System.Windows.Forms.Button
    Friend WithEvents btnAnalyze As System.Windows.Forms.Button
    Friend WithEvents btnMerge As System.Windows.Forms.Button
    Friend WithEvents colUserID As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colUserName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colUserEmail As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colIncludeInMerge As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents colHasData As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents tbLog As System.Windows.Forms.TextBox
    Friend WithEvents tbMerge As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label

End Class
