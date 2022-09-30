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
        Me.tbSourceFile = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.dlgOpen = New System.Windows.Forms.OpenFileDialog
        Me.Label3 = New System.Windows.Forms.Label
        Me.tbDestFile = New System.Windows.Forms.TextBox
        Me.btnBrowseDest = New System.Windows.Forms.Button
        Me.Label4 = New System.Windows.Forms.Label
        Me.Label5 = New System.Windows.Forms.Label
        Me.txtDest = New System.Windows.Forms.TextBox
        Me.btnCopy = New System.Windows.Forms.Button
        Me.btnSave = New System.Windows.Forms.Button
        Me.btnBrowseSource = New System.Windows.Forms.Button
        Me.GridViewStrings = New System.Windows.Forms.DataGridView
        Me.ResName = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.OrigString = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.DestString = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.Comment = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.txtSource = New System.Windows.Forms.TextBox
        Me.lblMessage = New System.Windows.Forms.Label
        CType(Me.GridViewStrings, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'tbSourceFile
        '
        Me.tbSourceFile.Location = New System.Drawing.Point(99, 9)
        Me.tbSourceFile.Name = "tbSourceFile"
        Me.tbSourceFile.Size = New System.Drawing.Size(226, 20)
        Me.tbSourceFile.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(0, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(93, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Source Resource:"
        '
        'dlgOpen
        '
        Me.dlgOpen.FileName = "OpenFileDialog1"
        Me.dlgOpen.Filter = "Resource Files (*.resx) | *.resx"
        Me.dlgOpen.RestoreDirectory = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(417, 13)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(112, 13)
        Me.Label3.TabIndex = 10
        Me.Label3.Text = "Destination Resource:"
        '
        'tbDestFile
        '
        Me.tbDestFile.Location = New System.Drawing.Point(535, 9)
        Me.tbDestFile.Name = "tbDestFile"
        Me.tbDestFile.Size = New System.Drawing.Size(226, 20)
        Me.tbDestFile.TabIndex = 9
        '
        'btnBrowseDest
        '
        Me.btnBrowseDest.Location = New System.Drawing.Point(769, 8)
        Me.btnBrowseDest.Name = "btnBrowseDest"
        Me.btnBrowseDest.Size = New System.Drawing.Size(66, 20)
        Me.btnBrowseDest.TabIndex = 8
        Me.btnBrowseDest.Text = "Browse..."
        Me.btnBrowseDest.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(0, 306)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(74, 13)
        Me.Label4.TabIndex = 11
        Me.Label4.Text = "Source String:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(393, 306)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(93, 13)
        Me.Label5.TabIndex = 13
        Me.Label5.Text = "Destination String:"
        '
        'txtDest
        '
        Me.txtDest.AcceptsReturn = True
        Me.txtDest.Location = New System.Drawing.Point(396, 326)
        Me.txtDest.Multiline = True
        Me.txtDest.Name = "txtDest"
        Me.txtDest.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtDest.Size = New System.Drawing.Size(348, 74)
        Me.txtDest.TabIndex = 12
        '
        'btnCopy
        '
        Me.btnCopy.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.btnCopy.Location = New System.Drawing.Point(367, 350)
        Me.btnCopy.Name = "btnCopy"
        Me.btnCopy.Size = New System.Drawing.Size(23, 22)
        Me.btnCopy.TabIndex = 14
        Me.btnCopy.Text = "»"
        Me.btnCopy.UseVisualStyleBackColor = True
        '
        'btnSave
        '
        Me.btnSave.Enabled = False
        Me.btnSave.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.btnSave.Location = New System.Drawing.Point(750, 377)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(85, 23)
        Me.btnSave.TabIndex = 15
        Me.btnSave.Text = "&Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnBrowseSource
        '
        Me.btnBrowseSource.Location = New System.Drawing.Point(331, 10)
        Me.btnBrowseSource.Name = "btnBrowseSource"
        Me.btnBrowseSource.Size = New System.Drawing.Size(66, 20)
        Me.btnBrowseSource.TabIndex = 16
        Me.btnBrowseSource.Text = "Browse..."
        Me.btnBrowseSource.UseVisualStyleBackColor = True
        '
        'GridViewStrings
        '
        Me.GridViewStrings.AllowUserToAddRows = False
        Me.GridViewStrings.AllowUserToDeleteRows = False
        Me.GridViewStrings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridViewStrings.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ResName, Me.OrigString, Me.DestString, Me.Comment})
        Me.GridViewStrings.Location = New System.Drawing.Point(3, 39)
        Me.GridViewStrings.MultiSelect = False
        Me.GridViewStrings.Name = "GridViewStrings"
        Me.GridViewStrings.ReadOnly = True
        Me.GridViewStrings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.GridViewStrings.ShowEditingIcon = False
        Me.GridViewStrings.ShowRowErrors = False
        Me.GridViewStrings.Size = New System.Drawing.Size(832, 264)
        Me.GridViewStrings.TabIndex = 17
        '
        'ResName
        '
        Me.ResName.Frozen = True
        Me.ResName.HeaderText = "Label"
        Me.ResName.Name = "ResName"
        Me.ResName.ReadOnly = True
        Me.ResName.Width = 140
        '
        'OrigString
        '
        Me.OrigString.HeaderText = "Original string"
        Me.OrigString.Name = "OrigString"
        Me.OrigString.ReadOnly = True
        Me.OrigString.Width = 275
        '
        'DestString
        '
        Me.DestString.HeaderText = "Destination String"
        Me.DestString.Name = "DestString"
        Me.DestString.ReadOnly = True
        Me.DestString.Width = 275
        '
        'Comment
        '
        Me.Comment.HeaderText = "Comment"
        Me.Comment.Name = "Comment"
        Me.Comment.ReadOnly = True
        Me.Comment.Width = 80
        '
        'txtSource
        '
        Me.txtSource.AcceptsReturn = True
        Me.txtSource.Location = New System.Drawing.Point(3, 326)
        Me.txtSource.Multiline = True
        Me.txtSource.Name = "txtSource"
        Me.txtSource.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtSource.Size = New System.Drawing.Size(348, 74)
        Me.txtSource.TabIndex = 18
        '
        'lblMessage
        '
        Me.lblMessage.AutoSize = True
        Me.lblMessage.Location = New System.Drawing.Point(338, 175)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.Size = New System.Drawing.Size(111, 13)
        Me.lblMessage.TabIndex = 19
        Me.lblMessage.Text = "Loading. Please wait…"
        Me.lblMessage.Visible = False
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(841, 411)
        Me.Controls.Add(Me.lblMessage)
        Me.Controls.Add(Me.txtSource)
        Me.Controls.Add(Me.GridViewStrings)
        Me.Controls.Add(Me.btnBrowseSource)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnCopy)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtDest)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.tbDestFile)
        Me.Controls.Add(Me.btnBrowseDest)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.tbSourceFile)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Tyro Resource Translation Tool 1.2"
        CType(Me.GridViewStrings, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents tbSourceFile As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents dlgOpen As System.Windows.Forms.OpenFileDialog
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents tbDestFile As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowseDest As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtDest As System.Windows.Forms.TextBox
    Friend WithEvents btnCopy As System.Windows.Forms.Button
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents ImageList1 As System.Windows.Forms.ImageList
    Friend WithEvents btnBrowseSource As System.Windows.Forms.Button
    Friend WithEvents GridViewStrings As System.Windows.Forms.DataGridView
    Friend WithEvents txtSource As System.Windows.Forms.TextBox
    Friend WithEvents ResName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents OrigString As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DestString As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Comment As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lblMessage As System.Windows.Forms.Label

End Class
