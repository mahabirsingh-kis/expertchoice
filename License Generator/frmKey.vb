Public Class frmKey

    Private Sub btnOK_Click(sender As System.Object, e As System.EventArgs) Handles btnOK.Click
        If txtSerial.Text = "" Then
            MessageBox.Show("You need to provide the License Key", "Error", MessageBoxButtons.OK)
            txtSerial.Focus()
            DialogResult = Windows.Forms.DialogResult.None
        Else
            DialogResult = Windows.Forms.DialogResult.OK
            Close()
        End If
    End Sub

    Private Sub txtSerial_KeyUp(sender As System.Object, e As System.Windows.Forms.KeyEventArgs) Handles txtSerial.KeyUp
        If Not e.Alt AndAlso e.KeyCode = Keys.Enter Then btnOK_Click(sender, Nothing)
    End Sub

    ' D3352 ===
    Private Sub frmKey_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        txtSerial.Focus()
    End Sub
    ' D3352 ==

End Class