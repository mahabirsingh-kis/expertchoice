Imports System.Xml
Imports System.Xml.XPath
Imports System.IO

Public Class frmMain

    Private mSourceReader As New clsResourceReader
    Private mDestReader As New clsResourceReader

    ' D0798 ===
    Private Const _CELL_LABEL As Integer = 0
    Private Const _CELL_ORIGINAL As Integer = 1
    Private Const _CELL_DESTINATION As Integer = 2
    Private Const _CELL_COMMENT As Integer = 3

    Private fSrcHasChanges As Boolean = False       ' D0799
    Private fDestHasChanges As Boolean = False      ' D0799
    ' D0798 ==

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowseSource.Click
        If dlgOpen.ShowDialog = Windows.Forms.DialogResult.OK Then
            tbSourceFile.Text = dlgOpen.FileName
            LoadStrings()   ' D0798
        End If
    End Sub

    Private Sub btnBrowseDest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowseDest.Click
        If dlgOpen.ShowDialog = Windows.Forms.DialogResult.OK Then
            tbDestFile.Text = dlgOpen.FileName
            LoadStrings()   ' D0798
        End If
    End Sub

    Private Sub LoadStrings()
        ' D0798 ===
        If tbDestFile.Text <> "" AndAlso tbSourceFile.Text <> "" Then
            If Not mSourceReader.ReadResourceFile(tbSourceFile.Text) Then
                MessageBox.Show("Can't read original resources")
                Exit Sub
            End If
            If Not mDestReader.ReadResourceFile(tbDestFile.Text) Then
                MessageBox.Show("Can't read destination resources")
                Exit Sub
            End If

            ' D0799 ===
            GridViewStrings.Visible = False
            lblMessage.Visible = True
            Application.DoEvents()
            ' D0799 ==
            Dim tRow() As String = {"", "", "", ""}

            GridViewStrings.Rows.Clear()
            For Each param As clsResourceParameter In mSourceReader.Parameters.Values   ' D5063
                'Dim sDestParam As clsResourceParameter = Nothing
                'If Not mDestReader.ParameterExists(param.Name) Then
                '    sDestParam = New clsResourceParameter(param.Name, param.Value, param.Comment)
                '    mDestReader.AddParameter(sDestParam)
                'Else
                '    sDestParam = mDestReader.ParameterByName(param.Name)
                'End If
                ' D0799 ===
                Dim sDestValue As String = ""
                Dim sDestParam As clsResourceParameter = Nothing
                If mDestReader.ParameterExists(param.Name) Then
                    sDestParam = mDestReader.ParameterByName(param.Name)
                    sDestValue = sDestParam.Value
                End If
                ' D0799 ==
                tRow(_CELL_LABEL) = param.Name
                tRow(_CELL_ORIGINAL) = param.Value
                tRow(_CELL_DESTINATION) = sDestValue    ' D0799
                tRow(_CELL_COMMENT) = param.Comment
                Dim idx As Integer = GridViewStrings.Rows.Add(tRow) ' D0799
                If sDestParam Is Nothing Then
                    GridViewStrings.Rows(idx).Cells(_CELL_DESTINATION).Style.BackColor = Color.FromArgb(255, 255, 240, 240) ' D0799
                Else
                    If sDestValue = param.Value Then
                        GridViewStrings.Rows(idx).Cells(_CELL_DESTINATION).Style.BackColor = Color.FromArgb(255, 240, 240, 255)
                        GridViewStrings.Rows(idx).Cells(_CELL_ORIGINAL).Style.BackColor = GridViewStrings.Rows(idx).Cells(_CELL_DESTINATION).Style.BackColor
                    End If
                End If
            Next

            ' D0799 ===
            lblMessage.Visible = False
            GridViewStrings.Visible = True
            Application.DoEvents()
            GridViewStrings.Focus()
            GridViewStrings.Sort(GridViewStrings.Columns(_CELL_LABEL), System.ComponentModel.ListSortDirection.Ascending)
            If GridViewStrings.Rows.Count > 0 Then GridViewStrings.Rows(0).Selected = True
            ' D0799 ==
        End If
        ' D0798 ==
    End Sub

    Private Sub btnCopy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCopy.Click
        txtDest.Text = txtSource.Text
    End Sub

    Private Sub txtDest_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtDest.KeyDown, GridViewStrings.KeyDown
        If e.KeyCode = Keys.Enter AndAlso e.Control Then
            e.SuppressKeyPress = True
            If GridViewStrings.SelectedRows.Count > 0 Then
                Dim tRow As DataGridViewRow = GridViewStrings.SelectedRows(0)
                If tRow.Index < GridViewStrings.Rows.Count - 1 Then GridViewStrings.Rows(tRow.Index + 1).Selected = True
            End If
        End If
    End Sub

    Private Sub txtDest_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtDest.TextChanged, txtSource.TextChanged  ' D0799
        ' D0798 ===
        If GridViewStrings.SelectedRows.Count > 0 Then
            Dim tRow As DataGridViewRow = GridViewStrings.SelectedRows(0)

            ' D0799  ===
            Dim sLabel As String = CStr(tRow.Cells(_CELL_LABEL).Value)
            Dim fDestEdit As Boolean = sender Is txtDest

            Dim param As clsResourceParameter = Nothing
            If fDestEdit Then
                If Not mDestReader.ParameterExists(sLabel) Then
                    Dim sOrigParam As clsResourceParameter = mSourceReader.ParameterByName(sLabel)
                    If sOrigParam IsNot Nothing Then
                        param = New clsResourceParameter(sOrigParam.Name, sOrigParam.Value, "")
                        mDestReader.AddParameter(param)
                    End If
                Else
                    param = mDestReader.ParameterByName(sLabel)
                End If
            Else
                param = mSourceReader.ParameterByName(sLabel)
            End If

            If param IsNot Nothing Then
                Dim sValue As String = ""
                If fDestEdit Then sValue = txtDest.Text Else sValue = txtSource.Text
                If param.Value <> sValue Then
                    Dim CellIdx As Integer = CInt(IIf(fDestEdit, _CELL_DESTINATION, _CELL_ORIGINAL))
                    tRow.Cells(CellIdx).Style.BackColor = Color.FromArgb(255, 255, 245, 230)
                    param.Value = sValue
                    tRow.Cells(CellIdx).Value = param.Value
                    If fDestEdit Then
                        Dim sComment As String = CStr(tRow.Cells(_CELL_COMMENT).Value)
                        If sComment <> "" Then param.Comment = sComment
                    End If
                    If fDestEdit Then fDestHasChanges = True Else fSrcHasChanges = True
                    ' D0799 ==
                    btnSave.Enabled = True
                End If
            End If
        End If
        ' D0798 ==
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        ' D0799 ===
        If fSrcHasChanges Then
            Save(mSourceReader, tbSourceFile.Text)
            fSrcHasChanges = False
        End If
        If fDestHasChanges Then
            Save(mDestReader, tbDestFile.Text)
            fDestHasChanges = False
        End If
        btnSave.Enabled = False
        ' D0799 ==
    End Sub

    Private Sub Save(ByVal tReader As clsResourceReader, ByVal sFileName As String)    ' D0799
        Dim document As XmlDocument = New XmlDocument()
        document.Load(Directory.GetCurrentDirectory + "\empty.xml")
        Dim navigator As XPath.XPathNavigator = document.CreateNavigator()
        Dim writer As XmlWriter
        If navigator.MoveToFirstChild Then
            For Each param As clsResourceParameter In tReader.Parameters.Values     ' D0799 + D5063
                writer = navigator.AppendChild()
                writer.WriteStartElement("data")
                writer.WriteAttributeString("name", param.Name)
                writer.WriteElementString("value", param.Value)
                writer.WriteElementString("comment", param.Comment) ' D0798
                writer.WriteEndElement()
                writer.Close()
            Next
        End If
        document.Save(sFileName)    ' D0799
    End Sub

    ' D0798 ===
    Private Sub GridViewStrings_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridViewStrings.SelectionChanged
        If GridViewStrings.SelectedRows.Count > 0 Then
            Dim tRow As DataGridViewRow = GridViewStrings.SelectedRows(0)
            txtSource.Text = CStr(tRow.Cells(_CELL_ORIGINAL).Value)
            txtDest.Text = CStr(tRow.Cells(_CELL_DESTINATION).Value)
        End If
    End Sub

    Private Sub frmMain_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        If (fSrcHasChanges Or fDestHasChanges) AndAlso MessageBox.Show("You have unsaved rows. Do you want to save it before the exit?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = Windows.Forms.DialogResult.Yes Then btnSave_Click(sender, Nothing) ' D0799
    End Sub

    Private Sub GridViewStrings_CellDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridViewStrings.CellDoubleClick
        If e.ColumnIndex = _CELL_ORIGINAL Then txtSource.Focus() ' D0799
        If e.ColumnIndex = _CELL_DESTINATION Then txtDest.Focus()
    End Sub
    ' D0798 ==

    ' D0799 ===
    Private Sub frmMain_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Dim sParam As String = ""
        If My.Application.CommandLineArgs.Count > 0 Then
            sParam = My.Application.CommandLineArgs(0).Trim
            If Not sParam.Contains("\") Then sParam = My.Computer.FileSystem.CombinePath(Directory.GetCurrentDirectory, sParam)
            If My.Computer.FileSystem.FileExists(sParam) Then tbSourceFile.Text = sParam
        End If
        If My.Application.CommandLineArgs.Count > 1 Then
            sParam = My.Application.CommandLineArgs(1).Trim
            If Not sParam.Contains("\") Then sParam = My.Computer.FileSystem.CombinePath(Directory.GetCurrentDirectory, sParam)
            If My.Computer.FileSystem.FileExists(sParam) Then tbDestFile.Text = sParam
        End If
        LoadStrings()
    End Sub
    ' D0799 ==

End Class