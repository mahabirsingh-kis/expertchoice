Imports System.IO

Namespace ExpertChoice.Service

    Public Module FileService

        Public Const _FILE_EXT_AHPS As String = ".ahps"     ' D0378
        Public Const _FILE_EXT_AHPX As String = ".ahpx"     ' D0091 + D0127
        Public Const _FILE_EXT_AHP As String = ".ahp"       ' D0127
        Public Const _FILE_EXT_AHPZ As String = ".ahpz"     ' D0470
        Public Const _FILE_EXT_SUR As String = ".sur"       ' D0297
        Public Const _FILE_EXT_SURS As String = ".surs"     ' D0381
        Public Const _FILE_EXT_XML As String = ".xml"       ' D0423
        Public Const _FILE_EXT_TXT As String = ".txt"       ' D2132
        Public Const _FILE_EXT_ZIP As String = ".zip"       ' D0240
        Public Const _FILE_EXT_RAR As String = ".rar"       ' D0242
        Public Const _FILE_EXT_NODESET As String = ".txt"   ' D5041

        ' D0132 ===
        ''' <summary>
        ''' Just alias for system GetTempFilename()
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function File_CreateTempName() As String
            Return Path.GetTempFileName ' Path.GetTempPath() + Path.GetRandomFileName()
        End Function
        ' D0132 ==

        ''' <summary>
        ''' Erase file without any RTEs
        ''' </summary>
        ''' <param name="sFileName">Full filename with path</param>
        ''' <remarks>All exceptions will be suppressed</remarks>
        Public Sub File_Erase(ByVal sFileName As String)
            Try
                If My.Computer.FileSystem.FileExists(sFileName) Then
                    DebugInfo(String.Format("Delete file '{0}'", sFileName))
                    My.Computer.FileSystem.DeleteFile(sFileName)
                End If
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)
            End Try
        End Sub

        ''' <summary>
        ''' Try to open and read all content for specified file
        ''' </summary>
        ''' <param name="sFName">Filename for reading</param>
        ''' <param name="sErrorMessage">Optional. You should to use string params {0} for filename and {1} for error details.</param>
        ''' <returns>String with file content or just a error message.</returns>
        ''' <remarks></remarks>
        Public Function File_GetContent(ByVal sFName As String, Optional ByVal sErrorMessage As String = "Couldn't load file '{0}'. Error: {1}") As String
            Dim sResult As String = ""
            Try
                DebugInfo(String.Format("Read file content '{0}'", sFName))
                Dim sr As IO.StreamReader = New IO.StreamReader(sFName)
                sResult = sr.ReadToEnd
                sr.Close()
            Catch ex As Exception
                sResult += String.Format(sErrorMessage, sFName, ex.Message)
                DebugInfo(sResult, _TRACE_WARNING)
            End Try
            Return sResult
        End Function

        ' D0657 ===
        Public Function File_GetContentAsMIME(ByVal sFileName As String, Optional ByRef sErrorMSg As String = Nothing) As String
            Dim sFileContent As String = Nothing
            Try
                DebugInfo(String.Format("Read file content '{0}'", sFileName))
                Dim fileContents As Byte() = My.Computer.FileSystem.ReadAllBytes(sFileName)
                DebugInfo("Encode file as MIME-base64 string")
                sFileContent = Convert.ToBase64String(fileContents, Base64FormattingOptions.InsertLineBreaks)
            Catch ex As Exception
                If sErrorMSg IsNot Nothing Then sErrorMSg = ex.Message
                DebugInfo(ex.Message, _TRACE_WARNING)
            End Try
            Return sFileContent
        End Function
        ' D0657 ==

        ' D6462 ===
        Public Function File_GetContentFromMIME(ByVal sFileName As String, Optional ByRef sErrorMSg As String = Nothing) As Byte()
            Dim sFileContent As Byte() = Nothing
            Try
                DebugInfo(String.Format("Read file content '{0}'", sFileName))
                Dim fileContents As String = My.Computer.FileSystem.ReadAllText(sFileName)
                If fileContents.StartsWith("data:") Then
                    Dim idx As Integer = fileContents.IndexOf("base64,")
                    If idx > 0 Then
                        fileContents = fileContents.Substring(idx + 7)
                    End If
                End If
                DebugInfo("Decode file from MIME-base64 string")
                sFileContent = Convert.FromBase64String(fileContents)
            Catch ex As Exception
                If sErrorMSg IsNot Nothing Then sErrorMSg = ex.Message
                DebugInfo(ex.Message, _TRACE_WARNING)
            End Try
            Return sFileContent
        End Function
        ' D6462 ==

        ''' <summary>
        ''' Create folder with specified Name without RTEs
        ''' </summary>
        ''' <param name="sFolderName">Folder name with path (recommended). When name is empty, will be created folder with new TempFilename and assigned to var sFolderName</param>
        ''' <returns>True, when folder was created.</returns>
        ''' <remarks>When folder exists before creation, this will be erased with all files. All RTEs will be suppressed</remarks>
        Public Function File_CreateFolder(ByRef sFolderName As String) As Boolean
            Dim fExist As Boolean = True
            Try
                If My.Computer.FileSystem.DirectoryExists(sFolderName) Then Return fExist ' D0240
                Dim sCurPath As String = Directory.GetCurrentDirectory  ' D0132
                If sFolderName = "" Then sFolderName = File_CreateTempName() ' D0132 + D0184
                DebugInfo(String.Format("Create folder '{0}'", sFolderName))
                File_Erase(sFolderName) ' D0240
                My.Computer.FileSystem.CreateDirectory(sFolderName)
                If sCurPath <> "" Then Directory.SetCurrentDirectory(sCurPath) ' D0132
                fExist = True
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)
                sFolderName = ""
            End Try
            Return fExist
        End Function

        ''' <summary>
        ''' Delete recursively folder with specified name without RTEs
        ''' </summary>
        ''' <param name="sFolderName">Folder name with path</param>
        ''' <remarks></remarks>
        Public Sub File_DeleteFolder(ByVal sFolderName As String)
            Try
                DebugInfo(String.Format("Delete folder '{0}'", sFolderName))
                ' D0146 ===
                Dim AllSubDirs As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Computer.FileSystem.GetDirectories(sFolderName)
                For Each sSubFolder As String In AllSubDirs
                    File_DeleteFolder(sSubFolder)
                Next
                ' D0146 ==
                My.Computer.FileSystem.DeleteDirectory(sFolderName, FileIO.DeleteDirectoryOption.DeleteAllContents)
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)
            End Try
        End Sub

        ' D0298 ===
        Public Function isSupportedArchive(ByVal sFileName As String) As Boolean
            Try ' D0994
                ' D0470 ===
                Select Case Path.GetExtension(sFileName).ToLower
                    Case _FILE_EXT_AHPZ, _FILE_EXT_RAR, _FILE_EXT_ZIP
                        Return True
                    Case Else
                        Return False
                End Select
                ' D0470 ==
            Catch ex As Exception   ' D0994
                Return False    ' D0994
            End Try
        End Function
        ' D0298 ==

        ' D0180 ===
        ''' <summary>
        ''' Get name for specified Project
        ''' </summary>
        ''' <param name="sName">Primary name (high priority, as usual -- .Filename)</param>
        ''' <param name="sAltName1">Alternative Name 1 (medium priority, as usual -- .Passcode)</param>
        ''' <param name="sAltName2">Alternative Name 2 (low priority, as usual -- fixed name)</param>
        ''' <param name="sExt">Extension for filename (with ot w/o ".")</param>
        ''' <returns>Selected by priority filename with specified extension (without path)</returns>
        ''' <remarks></remarks>
        Public Function GetProjectFileName(ByVal sName As String, ByVal sAltName1 As String, ByVal sAltName2 As String, Optional ByVal sExt As String = Nothing) As String
            Dim sFileName As String = sName
            If sFileName = "" Then
                If sAltName1 <> "" Then sFileName = sAltName1 Else sFileName = sAltName2
            End If ' D0994
            Dim invalidPathChars As Char() = Path.GetInvalidPathChars()
            ' D0792 ===
            Dim idx = invalidPathChars.Length
            Array.Resize(invalidPathChars, idx + 11) ' D1617 + D1619
            invalidPathChars(idx) = CChar("/")
            invalidPathChars(idx + 1) = CChar("\")
            invalidPathChars(idx + 2) = CChar(":")
            invalidPathChars(idx + 3) = CChar("*")  ' D1617
            invalidPathChars(idx + 4) = CChar(";")  ' D1617
            ' D1619 ===
            invalidPathChars(idx + 5) = CChar("+")
            invalidPathChars(idx + 6) = CChar("=")
            invalidPathChars(idx + 7) = CChar("«")
            invalidPathChars(idx + 8) = CChar(",")
            invalidPathChars(idx + 9) = CChar("""")
            invalidPathChars(idx + 10) = CChar("?")
            ' D1619 ==
            ' D0792 ==
            For Each invalidPChar As Char In invalidPathChars
                sFileName = sFileName.Replace(invalidPChar, "_")
            Next invalidPChar
            sFileName = sFileName.Replace("…", "_").Replace("__", "_").Replace("_ ", " ").Replace(" _", " ").Replace("  ", " ").Replace("  ", " ").Trim   ' D1617 + D1619 + D4725
            If Not String.IsNullOrEmpty(sExt) Then
                ' D0994 ===
                Try
                    ' D1622 ==
                    If sFileName.Length > 0 AndAlso Not (sFileName.EndsWith(")") Or sFileName.EndsWith("]")) Then
                        Dim sExtTmp As String = "." + Path.GetExtension(sFileName).Trim(CChar(".")).ToLower
                        Dim sExtList = {_FILE_EXT_RAR, _FILE_EXT_ZIP, ".ahp", ".ahps", ".ahpx", "ahpz"}
                        If sExtTmp.Length < 6 AndAlso Array.IndexOf(sExtList, sExtTmp) >= 0 Then
                            sFileName = Path.GetFileNameWithoutExtension(sFileName)
                        End If
                    End If
                    ' D1622 ==
                    sFileName = sFileName.Replace(".", "_")  ' D1619
                Catch ex As Exception
                    sFileName = "decision"
                End Try
                ' D0994 ==
                'If sFileName(sFileName.Length - 1) = "" Then sFileName = sFileName.Substring(0, sFileName.Length - 1)  ' -D1619 ?
                If sExt.Length > 0 Then
                    If sExt(0) <> "." Then sExt = "." + sExt
                    sFileName += sExt
                End If
            End If
            Return sFileName
        End Function
        ' D0180 ==

        ' D3478 ===
        Public Function SafeFileName(fromString As String) As String
            For Each c In IO.Path.GetInvalidFileNameChars
                fromString = fromString.Replace(c, "")
            Next
            Return fromString
        End Function
        ' D3478 ==

    End Module

End Namespace
