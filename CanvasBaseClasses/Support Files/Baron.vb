Namespace ExpertChoice.Service

    Public Module Baron

        ' D4072 ===
        Public Enum BaronErrorCode
            None = 0
            NoOptimizer = 1
            CantFindSolver = 2
            CantCreateFolder = 3
            CantCreateInputFile = 4
            NoInputData = 5
            CantSaveInputData = 6
            CantRun = 7
            ProcessTerminated = 8
            ExitCode = 9
            SolverError = 10
        End Enum
        ' D4072 ==

        Public Baron_Path_to_EXE As String = "~/App_Data/Baron/"
        Public Baron_Custom_OutputFolder As String = ""     ' D4075  Use if you want to keep all files aftert the solve in your custom folder

        Public Baron_Error_Code As BaronErrorCode = BaronErrorCode.None ' D4072
        Public Baron_Error_Details As String = ""                       ' D4072

        Public Const Baron_EXE_filename As String = "Baron.exe"
        Public Const Baron_bar_name As String = "input.bar"
        Public Const Baron_res_name As String = "output.res"
        Public Const Baron_log_name As String = "log.txt"
        Public Const Baron_run_timeout As Integer = 120 * 1000    ' ms

        Public Const Baron_cmd As String = "{0}"      ' 0 - input file; 1 - log file // for now " > " DOS command doesn't work as expected

        Private Const Baron_Error_BARON As String = vbLf + "BARON: "   ' D4078
        Private Const Baron_Error_ERROR As String = vbLf + "ERROR: "   ' D4078

        Public Function RunBaronSolver(Optimizer As RiskOptimizer, EventIDs As List(Of Guid), ByRef FundedControls As List(Of Guid), ByRef TotalCost As Double) As Double
            Dim fRes As Double = -1
            ' D4072 ===
            Baron_Error_Details = ""
            Baron_Error_Code = BaronErrorCode.None

            If Optimizer IsNot Nothing Then

                Dim sExe As String = String.Format("{0}{1}", Baron_Path_to_EXE, Baron_EXE_filename)
                If My.Computer.FileSystem.FileExists(sExe) Then

                    Dim sPath As String = Baron_Custom_OutputFolder     ' D4075
                    If sPath = "" Then sPath = File_CreateTempName() ' D4075
                    sPath = sPath.TrimEnd(CChar("\"))
                    File_Erase(sPath)
                    File_CreateFolder(sPath)

                    If My.Computer.FileSystem.DirectoryExists(sPath) Then

                        Dim fileInput As String = String.Format("{0}\{1}", sPath, Baron_bar_name)
                        Dim fileOutput As String = String.Format("{0}\{1}", sPath, Baron_res_name)
                        Dim fileLogs As String = String.Format("{0}\{1}", sPath, Baron_log_name)

                        Dim sInput As String = Optimizer.GetBaron(sPath + "\", Baron_res_name, EventIDs)

                        If Not String.IsNullOrEmpty(sInput) Then

                            Try
                                My.Computer.FileSystem.WriteAllText(fileInput, sInput, False, Text.Encoding.ASCII)
                            Catch ex As Exception
                                ' file will be checked next
                            End Try

                            If My.Computer.FileSystem.FileExists(fileInput) Then

                                Dim PrcOutput As String = ""
                                Dim PrcError As String = ""

                                Dim sOutput As String = ""
                                Dim sLogs As String = ""

                                Using Prc As New Process()

                                    Dim Params As New ProcessStartInfo
                                    Params.Arguments = String.Format(Baron_cmd, fileInput, fileLogs)
                                    Params.UseShellExecute = False
                                    Params.CreateNoWindow = True
                                    Params.RedirectStandardError = True
                                    Params.RedirectStandardOutput = True
                                    Params.FileName = sExe
                                    Params.WorkingDirectory = Baron_Path_to_EXE

                                    Try
                                        Prc.StartInfo = Params
                                        Prc.Start()

                                        ' Synchronously read the standard output of the spawned process. 
                                        PrcOutput = Prc.StandardOutput.ReadToEnd()
                                        Console.WriteLine(vbCrLf + "BARON output: " + PrcOutput + vbCrLf)
                                        PrcError = Prc.StandardError.ReadToEnd()
                                        Console.WriteLine(vbCrLf + "BARON error: " + PrcError + vbCrLf)

                                        Prc.WaitForExit(Baron_run_timeout)
                                        If Prc.HasExited Then
                                            ' -D4078
                                            ' AD: disable it due to unable to parse real solver error message
                                            'If Prc.ExitCode > 1 Then
                                            '    Baron_Error_Code = BaronErrorCode.ExitCode
                                            '    Baron_Error_Details = Prc.ExitCode.ToString
                                            'End If
                                        Else
                                            Prc.Kill()
                                            Baron_Error_Code = BaronErrorCode.ProcessTerminated
                                        End If

                                    Catch ex As Exception
                                        Baron_Error_Code = BaronErrorCode.CantRun
                                        Baron_Error_Details = ex.Message
                                    End Try

                                End Using

                                If Baron_Error_Code = BaronErrorCode.None Then

                                    If My.Computer.FileSystem.FileExists(fileOutput) Then sOutput = My.Computer.FileSystem.ReadAllText(fileOutput)
                                    If My.Computer.FileSystem.FileExists(fileLogs) Then sLogs = My.Computer.FileSystem.ReadAllText(fileLogs)

                                    ' D4075 ===
                                    Dim sPrcLogs As String = ""
                                    If PrcOutput <> "" Then sPrcLogs = "*** BARON OUTPUT ***" + vbCrLf + vbCrLf + PrcOutput + vbCrLf
                                    If PrcError <> "" Then sPrcLogs = String.Format("{0}{1}{2}", sPrcLogs, IIf(sPrcLogs = "", "", vbCrLf + vbCrLf), "*** BARON ERROR *** " + vbCrLf + vbCrLf + PrcError + vbCrLf) ' D4078
                                    If sPrcLogs <> "" Then
                                        sLogs += vbCrLf + sPrcLogs
                                        My.Computer.FileSystem.WriteAllText(fileLogs, sPrcLogs, False, Text.Encoding.ASCII)
                                    End If
                                    ' D4075 ==

                                    ' D4078 ===
                                    If Baron_Error_Code = BaronErrorCode.None Then
                                        sPrcLogs = sPrcLogs.Replace(vbCr, "")
                                        Dim sError As String = ""
                                        Dim sBaron As String = ""
                                        Dim idx As Integer = sPrcLogs.IndexOf(Baron_Error_BARON)
                                        If idx >= 0 Then
                                            sBaron = sPrcLogs.Substring(idx + Baron_Error_BARON.Length)
                                            idx = sBaron.IndexOf(vbLf)
                                            If idx > 0 Then sBaron = sBaron.Substring(0, idx)
                                            sBaron = sBaron.Trim
                                        End If
                                        idx = sPrcLogs.IndexOf(Baron_Error_ERROR)
                                        If idx >= 0 Then
                                            sError = sPrcLogs.Substring(idx + Baron_Error_ERROR.Length)
                                            idx = sError.IndexOf(vbLf)
                                            If idx > 0 Then sError = sError.Substring(0, idx)
                                            sError = sError.Trim
                                        End If
                                        If sBaron <> "" OrElse sError <> "" Then
                                            Baron_Error_Code = BaronErrorCode.SolverError
                                            If sBaron <> "" Then Baron_Error_Details = sBaron
                                            If sError <> "" Then Baron_Error_Details = String.Format("{0}{1}{2}", Baron_Error_Details, IIf(Baron_Error_Details = "", "", "; "), sError)
                                        End If
                                    End If

                                    If Baron_Error_Code = BaronErrorCode.None Then fRes = Optimizer.ParseBaron(sOutput, sLogs, EventIDs, FundedControls, TotalCost)
                                    ' D4078 ==

                                End If

                            Else
                                Baron_Error_Code = BaronErrorCode.CantSaveInputData
                                Baron_Error_Details = fileInput
                            End If

                        Else
                            Baron_Error_Code = BaronErrorCode.NoInputData
                        End If

                        ' D4075 ===
                        If Baron_Custom_OutputFolder = "" Then
                            File_DeleteFolder(sPath)
                        Else
                            sInput = sInput.Replace(sPath + "\", "")
                            My.Computer.FileSystem.WriteAllText(fileInput, sInput, False, Text.Encoding.ASCII)
                        End If
                        ' D4075 ==

                    Else
                        Baron_Error_Code = BaronErrorCode.CantCreateFolder
                        Baron_Error_Details = sPath
                    End If
                Else
                    Baron_Error_Code = BaronErrorCode.CantFindSolver
                    Baron_Error_Details = sExe
                End If
            Else
                Baron_Error_Code = BaronErrorCode.NoOptimizer
            End If
            ' D4072 ==
            Return fRes
        End Function

        Public Function RunBaronSolver2(Optimizer As Canvas.RASolver, EventIDs As List(Of Guid), ByRef FundedControls As List(Of Guid), ByRef TotalCost As Double, Optional BaronModel As String = "", Optional ByRef SolverState As Canvas.raSolverState = Canvas.raSolverState.raSolved) As Double
            Dim fRes As Double = -1
            ' D4072 ===
            Baron_Error_Details = ""
            Baron_Error_Code = BaronErrorCode.None

            If Optimizer IsNot Nothing Then

                Dim sExe As String = String.Format("{0}{1}", Baron_Path_to_EXE, Baron_EXE_filename)
                If My.Computer.FileSystem.FileExists(sExe) Then

                    Dim sPath As String = Baron_Custom_OutputFolder     ' D4075
                    If sPath = "" Then sPath = File_CreateTempName() ' D4075
                    sPath = sPath.TrimEnd(CChar("\"))
                    File_Erase(sPath)
                    File_CreateFolder(sPath)

                    If My.Computer.FileSystem.DirectoryExists(sPath) Then

                        Dim fileInput As String = String.Format("{0}\{1}", sPath, Baron_bar_name)
                        Dim fileOutput As String = String.Format("{0}\{1}", sPath, Baron_res_name)
                        Dim fileLogs As String = String.Format("{0}\{1}", sPath, Baron_log_name)

                        Dim sInput As String = If(BaronModel = "", Optimizer.GetBaron(sPath + "\", Baron_res_name, EventIDs), String.Format(BaronModel, sPath + "\", Baron_res_name))

                        If Not String.IsNullOrEmpty(sInput) Then

                            Try
                                My.Computer.FileSystem.WriteAllText(fileInput, sInput, False, Text.Encoding.ASCII)
                            Catch ex As Exception
                                ' file will be checked next
                            End Try

                            If My.Computer.FileSystem.FileExists(fileInput) Then

                                Dim PrcOutput As String = ""
                                Dim PrcError As String = ""

                                Dim sOutput As String = ""
                                Dim sLogs As String = ""

                                Using Prc As New Process()

                                    Dim Params As New ProcessStartInfo
                                    Params.Arguments = String.Format(Baron_cmd, fileInput, fileLogs)
                                    Params.UseShellExecute = False
                                    Params.CreateNoWindow = True
                                    Params.RedirectStandardError = True
                                    Params.RedirectStandardOutput = True
                                    Params.FileName = sExe
                                    Params.WorkingDirectory = Baron_Path_to_EXE

                                    Try
                                        Prc.StartInfo = Params
                                        Prc.Start()

                                        ' Synchronously read the standard output of the spawned process. 
                                        PrcOutput = Prc.StandardOutput.ReadToEnd()
                                        Console.WriteLine(vbCrLf + "BARON output: " + PrcOutput + vbCrLf)
                                        PrcError = Prc.StandardError.ReadToEnd()
                                        Console.WriteLine(vbCrLf + "BARON error: " + PrcError + vbCrLf)

                                        Prc.WaitForExit(Baron_run_timeout)
                                        If Prc.HasExited Then
                                            ' -D4078
                                            ' AD: disable it due to unable to parse real solver error message
                                            'If Prc.ExitCode > 1 Then
                                            '    Baron_Error_Code = BaronErrorCode.ExitCode
                                            '    Baron_Error_Details = Prc.ExitCode.ToString
                                            'End If
                                        Else
                                            Prc.Kill()
                                            Baron_Error_Code = BaronErrorCode.ProcessTerminated
                                        End If

                                    Catch ex As Exception
                                        Baron_Error_Code = BaronErrorCode.CantRun
                                        Baron_Error_Details = ex.Message
                                    End Try

                                End Using

                                If Baron_Error_Code = BaronErrorCode.None Then

                                    If My.Computer.FileSystem.FileExists(fileOutput) Then sOutput = My.Computer.FileSystem.ReadAllText(fileOutput)
                                    If My.Computer.FileSystem.FileExists(fileLogs) Then sLogs = My.Computer.FileSystem.ReadAllText(fileLogs)

                                    ' D4075 ===
                                    Dim sPrcLogs As String = ""
                                    If PrcOutput <> "" Then sPrcLogs = "*** BARON OUTPUT ***" + vbCrLf + vbCrLf + PrcOutput + vbCrLf
                                    If PrcError <> "" Then sPrcLogs = String.Format("{0}{1}{2}", sPrcLogs, IIf(sPrcLogs = "", "", vbCrLf + vbCrLf), "*** BARON ERROR *** " + vbCrLf + vbCrLf + PrcError + vbCrLf) ' D4078
                                    If sPrcLogs <> "" Then
                                        sLogs += vbCrLf + sPrcLogs
                                        My.Computer.FileSystem.WriteAllText(fileLogs, sPrcLogs, False, Text.Encoding.ASCII)
                                    End If
                                    ' D4075 ==

                                    ' D4078 ===
                                    If Baron_Error_Code = BaronErrorCode.None Then
                                        sPrcLogs = sPrcLogs.Replace(vbCr, "")
                                        Dim sError As String = ""
                                        Dim sBaron As String = ""
                                        Dim idx As Integer = sPrcLogs.IndexOf(Baron_Error_BARON)
                                        If idx >= 0 Then
                                            sBaron = sPrcLogs.Substring(idx + Baron_Error_BARON.Length)
                                            idx = sBaron.IndexOf(vbLf)
                                            If idx > 0 Then sBaron = sBaron.Substring(0, idx)
                                            sBaron = sBaron.Trim
                                        End If
                                        idx = sPrcLogs.IndexOf(Baron_Error_ERROR)
                                        If idx >= 0 Then
                                            sError = sPrcLogs.Substring(idx + Baron_Error_ERROR.Length)
                                            idx = sError.IndexOf(vbLf)
                                            If idx > 0 Then sError = sError.Substring(0, idx)
                                            sError = sError.Trim
                                        End If
                                        If sBaron <> "" OrElse sError <> "" Then
                                            Baron_Error_Code = BaronErrorCode.SolverError
                                            If sBaron <> "" Then Baron_Error_Details = sBaron
                                            If sError <> "" Then Baron_Error_Details = String.Format("{0}{1}{2}", Baron_Error_Details, IIf(Baron_Error_Details = "", "", "; "), sError)
                                        End If
                                    End If

                                    If Baron_Error_Code = BaronErrorCode.None Then fRes = Optimizer.ParseBaron(sOutput, sLogs, EventIDs, FundedControls, TotalCost, SolverState)
                                    ' D4078 ==

                                End If

                            Else
                                Baron_Error_Code = BaronErrorCode.CantSaveInputData
                                Baron_Error_Details = fileInput
                            End If

                        Else
                            Baron_Error_Code = BaronErrorCode.NoInputData
                        End If

                        ' D4075 ===
                        If Baron_Custom_OutputFolder = "" Then
                            File_DeleteFolder(sPath)
                        Else
                            sInput = sInput.Replace(sPath + "\", "")
                            My.Computer.FileSystem.WriteAllText(fileInput, sInput, False, Text.Encoding.ASCII)
                        End If
                        ' D4075 ==

                    Else
                        Baron_Error_Code = BaronErrorCode.CantCreateFolder
                        Baron_Error_Details = sPath
                    End If
                Else
                    Baron_Error_Code = BaronErrorCode.CantFindSolver
                    Baron_Error_Details = sExe
                End If
            Else
                Baron_Error_Code = BaronErrorCode.NoOptimizer
            End If
            ' D4072 ==
            Return fRes
        End Function

    End Module

End Namespace
