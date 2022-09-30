Module Module1

    Dim InfoMessage As Boolean = False

    Sub Main()

        If Command.ToLower.Contains("/i") Then
            InfoMessage = True
        End If

        If Command.ToLower.Contains("/r") Then
            Try
                Dim SVNPath As String = "."
                SVNPath = System.IO.Path.GetFullPath(SVNPath)
                Dim svn As New LibSubWCRev.SubWCRev
                svn.GetWCInfo(SVNPath, 0, 0)
                SendMessage(svn.Revision)
            Catch ex As Exception
                SendMessage(ex.ToString)
            End Try
        Else
            Try

                Dim FilePath As String = Command.Split(",")(0)
                Dim SVNPath As String = Command.Split(",")(1)

                FilePath = System.IO.Path.GetFullPath(FilePath)
                SVNPath = System.IO.Path.GetFullPath(SVNPath)

                Dim reader As New System.IO.StreamReader(New System.IO.FileStream(FilePath, IO.FileMode.Open))

                Dim strContents = reader.ReadToEnd
                Dim searchString As String = "<Assembly: AssemblyVersion("

                Dim newLine As String = strContents.Substring(strContents.IndexOf(searchString) + searchString.Length + 1)
                Dim buildVer As String = newLine.Substring(0, newLine.IndexOf(""""))

                Dim svn As New LibSubWCRev.SubWCRev
                svn.GetWCInfo(SVNPath, 0, 0)
                Dim out As String = buildVer & "." & svn.Revision
                Dim temp As String() = Split(out, ".")
                Dim AssemblyString As String = ""
                For i As Integer = 1 To temp.Length
                    AssemblyString += CInt(temp(i - 1)).ToString
                    If i < temp.Length Then
                        AssemblyString += "."
                    End If
                Next
                SendMessage(AssemblyString.Trim)
            Catch ex As Exception
                SendMessage(ex.ToString)
            End Try
        End If

    End Sub

    Private Sub SendMessage(ByVal Message As String)
        If InfoMessage = True Then
            'MsgBox(Message, MsgBoxStyle.Information)
            System.Windows.Forms.Clipboard.SetText(Message)
        Else
            Console.Write(Message)
        End If
    End Sub

End Module
