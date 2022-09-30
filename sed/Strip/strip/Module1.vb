Module Module1

    Sub Main(ByVal args As String())

        Dim sep As String = ""
        If args.Length > 0 Then
            sep = args(0)
        End If

        If sep.ToLower.Trim = "\t" Then
            sep = vbTab
        End If

        Dim stdIn As System.IO.TextReader = Console.In
        Dim sInput As String = stdIn.ReadToEnd

        Dim t As String = sInput
        Dim s As String = t.Replace(vbNewLine, sep).Replace(vbLf, sep).Replace(vbCr, sep)
        
        System.Console.WriteLine(s)


    End Sub

End Module
