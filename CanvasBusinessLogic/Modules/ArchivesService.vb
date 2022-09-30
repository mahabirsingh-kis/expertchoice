Imports Chilkat
Imports ExpertChoice.Data
Imports System.IO
Imports System.IO.Compression

Namespace ExpertChoice.Service

    Public Module ArchivesService

#Region "Processing ZIP And RAR files"

        ' D0240 ===
        Private Function CreateZipObj() As Zip
            Dim ZipObj As New Zip
            ZipObj.UnlockComponent("DECERNCONSZIP_9RZdTwUd9Y1P")
            Return ZipObj
        End Function

        Public Function PackZipFiles(ByVal sFilesList As StringArray, ByVal sZipFilename As String, ByRef sError As String) As Boolean
            Dim fResult As Boolean = False
            Dim ZipFile As Zip = CreateZipObj()
            ZipFile.NewZip(sZipFilename)
            ZipFile.TempDir = Path.GetTempPath  ' D1297
            If ZipFile.AppendMultiple(sFilesList, False) Then
                fResult = ZipFile.WriteZipAndClose()
            End If
            If Not sError Is Nothing Then sError = ZipFile.LastErrorText
            Return fResult
        End Function

        ' D3815 ===
        Public Function PackZipFilesAndDirs(sBaseDir As String, ByVal sFilesList As List(Of String), ByVal sZipFilename As String, ByRef sError As String) As Boolean
            Dim fResult As Boolean = False
            Dim ZipFile As Zip = CreateZipObj()
            ZipFile.NewZip(sZipFilename)
            ZipFile.TempDir = Path.GetTempPath
            Dim fRes As Boolean = True
            ZipFile.PathPrefix = sBaseDir
            ZipFile.AppendFromDir = sBaseDir
            For Each sName As String In sFilesList
                If Not ZipFile.AppendOneFileOrDir(sName, False) Then
                    fRes = False
                    Exit For
                End If
            Next
            If fRes Then fResult = ZipFile.WriteZipAndClose() Else fRes = False
            If Not sError Is Nothing Then sError = ZipFile.LastErrorText
            Return fResult
        End Function
        ' D3815 ==

        Public Function UnPackZipFile(ByVal sZipFile As String, ByVal sDestFolder As String, ByRef sError As String) As Boolean
            Dim fResult As Boolean = False
            If File_CreateFolder(sDestFolder) Then
                Dim ZipFile As Zip = CreateZipObj()
                ZipFile.OpenZip(sZipFile)
                Dim cnt As Integer = ZipFile.Unzip(sDestFolder)
                fResult = cnt > 0
                ZipFile.CloseZip()
                If Not sError Is Nothing Then sError = ZipFile.LastErrorText
            End If
            Return fResult
        End Function
        ' D0240 ==

        ' D0242 ===
        Public Function UnPackRarFile(ByVal sRarFile As String, ByVal sDestFolder As String, ByRef sError As String) As Boolean
            Dim fResult As Boolean = False
            If File_CreateFolder(sDestFolder) Then
                Try
                    Dim RarFile As New RARDecompressor(sRarFile)
                    fResult = RarFile.UnPackAll(sDestFolder)
                Catch ex As Exception
                    sError = ex.Message
                End Try
            End If
            Return fResult
        End Function
        ' D0242 ==

        ''' <summary>
        ''' Try to extract decision from the archive
        ''' </summary>
        ''' <param name="CoreApp">Could be Nothing. In this case nothing will be saved in Logs and you will get default messages.</param>
        ''' <param name="sOriginalName"></param>
        ''' <param name="sArchiveName"></param>
        ''' <param name="sFileExtList"></param>
        ''' <param name="sLogObject"></param>
        ''' <param name="sError"></param>
        ''' <param name="sFoundFilename"></param>
        ''' <param name="fEraseArchive"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ExtractArchiveForFile(ByVal CoreApp As clsComparionCore, ByVal sOriginalName As String, ByVal sArchiveName As String, ByVal sFileExtList As ArrayList, ByVal sLogObject As String, ByRef sError As String, Optional ByRef sFoundFilename As String = Nothing, Optional ByVal fEraseArchive As Boolean = True) As String   ' D0382 + D0505 + D1224
            Dim sExtractedFile As String = ""
            If Not isSupportedArchive(sOriginalName) Then Return sExtractedFile
            Dim sArcFolder As String = ""   ' D0242
            Dim sArcError As String = ""    ' D0242
            sArcFolder = File_CreateTempName()  ' D0242
            Dim fHasModel As Boolean = False
            ' D0242 ===
            DebugInfo(String.Format("Unpack Archive '{0}'", sArchiveName))
            Dim fUnpacked As Boolean = False
            Select Case Path.GetExtension(sOriginalName).ToLower
                Case _FILE_EXT_ZIP, _FILE_EXT_AHPZ  ' D0470
                    fUnpacked = UnPackZipFile(sArchiveName, sArcFolder, sArcError)
                Case _FILE_EXT_RAR
                    fUnpacked = UnPackRarFile(sArchiveName, sArcFolder, sArcError)
            End Select
            If fUnpacked Then
                ' D0242 ==
                Try
                    Dim AllFiles As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Computer.FileSystem.GetFiles(sArcFolder)  ' D0242
                    DebugInfo("Scan for required files")
                    For Each sFileName As String In AllFiles
                        If sFileExtList.IndexOf(Path.GetExtension(sFileName).ToLower) >= 0 Then
                            DebugInfo(String.Format("Found model: {0}", sFileName))
                            If fEraseArchive Then File_Erase(sArchiveName) ' D1224
                            If Not sFoundFilename Is Nothing Then sFoundFilename = sFileName ' D0381
                            sExtractedFile = File_CreateTempName()
                            File.Copy(sFileName, sExtractedFile, True)
                            fHasModel = True
                            If Not CoreApp Is Nothing Then CoreApp.DBSaveLog(dbActionType.actExtractArchive, dbObjectType.einfFile, -1, Path.GetFileName(sArchiveName), Path.GetFileName(sFileName)) ' D0496 + D0505
                            Exit For
                            'End If
                        End If
                    Next
                    If Not fHasModel Then
                        ' D0505 ===
                        Dim sDefError As String = "No models found in the archive"
                        If Not CoreApp Is Nothing Then sDefError = CoreApp.ResString("msgNoModelInZIP")
                        If sError = "" Then sError = sDefError
                        If sArcError = "" Then sArcError = sDefError
                        ' D0505 ==
                    End If
                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                End Try
            End If
            If sArcFolder <> "" Then File_DeleteFolder(sArcFolder) ' D0240 + D0242

            If Not fHasModel Then
                ' D0505 ===
                If Not CoreApp Is Nothing Then CoreApp.DBSaveLog(dbActionType.actExtractArchive, dbObjectType.einfFile, -1, Path.GetFileName(sArchiveName), "Error: " + sArcError) ' D0496
                Dim sErrorString = "Error while expand archive"
                If Not CoreApp Is Nothing Then CoreApp.ResString("msgUnzipError")
                If sError = "" Then sError = String.Format(sErrorString)
                ' D0505 ==
            End If
            ' D0240 ==
            Return sExtractedFile
        End Function


#End Region

#Region "Work with memory streams"

        ' D3512 ===
        Public Function StreamCompress(tStream As MemoryStream) As MemoryStream
            tStream.Seek(0, SeekOrigin.Begin)
            Dim tOutBytes() As Byte
            Using ms As New MemoryStream
                Dim tInBytes() As Byte = tStream.ToArray
                Using gzStream As New GZipStream(ms, CompressionMode.Compress)
                    gzStream.Write(tInBytes, 0, tInBytes.Length)
                End Using
                tOutBytes = ms.ToArray
            End Using
            Return New MemoryStream(tOutBytes)
        End Function

        Public Function StreamDecompress(tStream As MemoryStream) As MemoryStream
            tStream.Seek(0, SeekOrigin.Begin)
            Dim tOutBytes() As Byte
            Using gzStream As New GZipStream(tStream, CompressionMode.Decompress)
                Using sr As New MemoryStream
                    gzStream.CopyTo(sr)
                    tOutBytes = sr.ToArray
                End Using
            End Using
            Return New MemoryStream(tOutBytes)
        End Function
        ' D3512 ==

#End Region

    End Module

End Namespace
