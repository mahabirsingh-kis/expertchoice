Imports System.IO

Public Class frmMain
    Private mMerger As New clsAHPMerger
    Private mCanMerge As Boolean = False

    Private Sub btnOpenModels_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpenModels.Click
        If dlgOpen.ShowDialog = Windows.Forms.DialogResult.OK Then
            For Each filePath As String In dlgOpen.FileNames
                Dim fileName As String = Path.GetFileName(filePath)
                dgModels.Rows.Add(fileName, filePath)
                Dim users As List(Of clsUserInfo) = mMerger.AddFile(filePath)
                dgModels.Rows(dgModels.Rows.Count - 1).Tag = users
            Next
            dgModels.Rows(0).DefaultCellStyle.BackColor = Color.LightSteelBlue ' first row is default master
            mMerger.MasterFilePath = dgModels.Rows(0).Cells(colFilePath.Name).Value
            dgModels.Rows(dgModels.Rows.Count - 1).Selected = True
            If dgModels.Rows.Count = 1 Then
                dgModels_SelectionChanged(Me, e)
            End If
        End If
    End Sub

    Private Sub btnClearAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClearAll.Click
        dgModels.Rows.Clear()
        dgUsers.Rows.Clear()
        tbLog.Clear()
        tbMerge.Clear()
        mMerger.ClearAll()
        mCanMerge = False
        btnMerge.Enabled = False
    End Sub

    Private Sub btnRemoveModel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveModel.Click
        If dgModels.SelectedRows.Count > 0 Then
            mMerger.UserData.Remove(dgModels.SelectedRows(0).Cells(1).Value)
            dgModels.Rows.Remove(dgModels.SelectedRows(0))
            If dgModels.Rows.Count > 0 Then
                dgModels.Rows(0).Selected = True
                If dgModels.Rows.Count = 1 Then
                    dgModels.Rows(0).DefaultCellStyle.BackColor = Color.LightSteelBlue
                    mMerger.MasterFilePath = dgModels.Rows(0).Cells(colFilePath.Name).Value
                End If
            Else
                dgUsers.Rows.Clear()
            End If
        End If
    End Sub

    Private Sub btnSetMasterModel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetMasterModel.Click
        If dgModels.SelectedRows.Count > 0 Then
            For i As Integer = 0 To dgModels.Rows.Count - 1
                If dgModels.Rows(i).Selected Then
                    dgModels.Rows(i).DefaultCellStyle.BackColor = Color.LightSteelBlue
                    mMerger.MasterFilePath = dgModels.Rows(i).Cells(colFilePath.Name).Value
                    'dgModels.SelectedRows(0).DefaultCellStyle.BackColor = Color.LightSteelBlue
                Else
                    dgModels.Rows(i).DefaultCellStyle.BackColor = Color.White
                End If
            Next
        End If
    End Sub

    Private Sub dgModels_SelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dgModels.SelectionChanged
        If dgModels.SelectedRows.Count > 0 Then
            Dim users As List(Of clsUserInfo) = dgModels.SelectedRows(0).Tag
            dgUsers.Rows.Clear()
            If users IsNot Nothing Then
                For Each uInfo As clsUserInfo In users
                    With uInfo
                        dgUsers.Rows.Add(.UserID, .UserName, .UserEmail, .IncludedInMerge, .HasData)
                        If Not .HasData Then
                            dgUsers.Rows(dgUsers.Rows.Count - 1).DefaultCellStyle.BackColor = Color.LightGray
                        End If
                        dgUsers.Rows(dgUsers.Rows.Count - 1).Tag = uInfo
                    End With
                Next
            End If
        End If
    End Sub

    Private Sub dgUsers_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgUsers.CellValueChanged
        If e.RowIndex >= 0 And e.ColumnIndex >= 0 Then
            Dim uInfo As clsUserInfo = CType(dgUsers.Rows(e.RowIndex).Tag, clsUserInfo)
            If uInfo IsNot Nothing Then
                Select Case e.ColumnIndex
                    Case 1
                        If dgUsers.Rows(e.RowIndex).Cells(e.ColumnIndex).Value IsNot Nothing Then
                            uInfo.UserName = dgUsers.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                        Else
                            uInfo.UserName = ""
                        End If
                    Case 2
                        If dgUsers.Rows(e.RowIndex).Cells(e.ColumnIndex).Value IsNot Nothing Then
                            uInfo.UserEmail = dgUsers.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                        Else
                            uInfo.UserEmail = ""
                        End If
                    Case 3
                        uInfo.IncludedInMerge = dgUsers.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                End Select
            End If
        End If
    End Sub

    Public Function isValidEmail(ByVal sEmail As String) As Boolean
        Dim emailRegex As New System.Text.RegularExpressions.Regex("^([^@]+)@([^@]+)\.([^@]+)$")
        Dim emailMatch As System.Text.RegularExpressions.Match = emailRegex.Match(sEmail)
        Return (sEmail <> "" AndAlso sEmail.Trim = sEmail AndAlso emailMatch.Success)
    End Function

    Private Sub btnAnalyze_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAnalyze.Click
        tbLog.Clear()
        tbMerge.Clear()
        tbLog.Text += "WARNING: Only users that have data will be merged!" + vbCrLf

        mCanMerge = True

        Dim emails As New List(Of String)
        For Each DE As KeyValuePair(Of String, List(Of clsUserInfo)) In mMerger.UserData
            For Each uInfo In DE.Value
                If uInfo.HasData And uInfo.IncludedInMerge Then
                    Dim s As String = uInfo.UserName.Trim

                    If s = "" Then
                        tbLog.Text += vbCrLf
                        tbLog.Text += "Error! Incorrect user name" + vbCrLf
                        tbLog.Text += "Error details: Model: " + DE.Key + "; UserID:  " + uInfo.UserID.ToString + vbCrLf
                        mCanMerge = False
                    End If

                    If Not isValidEmail(uInfo.UserEmail) Then
                        tbLog.Text += vbCrLf
                        tbLog.Text += "Error! Incorrect user email" + vbCrLf
                        tbLog.Text += "Error details: Model: " + DE.Key + "; UserID:  " + uInfo.UserID.ToString + "; UserEmail:  " + uInfo.UserEmail + vbCrLf
                        mCanMerge = False
                    End If

                    emails.Add(uInfo.UserEmail.ToLower)
                End If
            Next
        Next

        emails.Sort()

        Dim i As Integer = 0
        While (i <= emails.Count - 2)
            If emails(i) = emails(i + 1) Then
                tbLog.Text += vbCrLf
                tbLog.Text += "Error! Duplicate email: " + emails(i) + vbCrLf
                tbLog.Text += "Files with this email: " + vbCrLf
                For Each DE As KeyValuePair(Of String, List(Of clsUserInfo)) In mMerger.UserData
                    For Each uInfo In DE.Value
                        If uInfo.UserEmail.ToLower = emails(i) Then
                            tbLog.Text += DE.Key + vbCrLf
                            Exit For
                        End If
                    Next
                Next

                mCanMerge = False
                While (i <= emails.Count - 2) AndAlso (emails(i) = emails(i + 1))
                    i += 1
                End While
            End If
            i += 1
        End While

        If mCanMerge Then
            tbLog.Text += "No errors found! Ready to merge!" + vbCrLf
            tbLog.Text += "Files will be merged to: " + mMerger.MasterFilePath + vbCrLf

        End If

        btnMerge.Enabled = mCanMerge
    End Sub

    Private Sub btnMerge_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMerge.Click
        tbMerge.Clear()

        mMerger.Merge()
        MessageBox.Show("Merging complete! View log for details", "EC Desktop Model Merger", MessageBoxButtons.OK)
    End Sub
End Class
