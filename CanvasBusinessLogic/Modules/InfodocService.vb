Imports Chilkat
Imports EasyByte
Imports System.IO
Imports System.Web
Imports ECCore
Imports Canvas
Imports ExpertChoice.Data
Imports System.Text.RegularExpressions
Imports System.Collections.ObjectModel

Namespace ExpertChoice.Service

    ' D0657 ===
    Public Enum mhtEncode
        mhtChilkat = 0
        mhtAD = 1
    End Enum
    ' D0657 ==

    Public Module InfodocService

        ''' <summary>
        ''' Flag for use PNG files instead GIF while RTF2HTML infodocs were parsed
        ''' </summary>
        ''' <remarks></remarks>
        Public _RTF2HTML_IMAGES_AS_PNG As Boolean = False   ' D0110

        Public _MHT_Encoder As mhtEncode = mhtEncode.mhtAD  ' D0657

        ''' <summary>
        ''' Option for replace all &lt;a href=''&gt; hyperlinks in parsed html infodocs for open in blank screens (use "target=_blank")
        ''' </summary>
        ''' <remarks></remarks>
        Public _UseBlankBaseHyperlinks As Boolean = True    ' D0600

        Public _ParseImageSrc As Boolean = True             ' D0611 + D0614 + D3396

        Public _ParseAbsolutePaths As Boolean = True        ' D0657

        Public _MHT_SearchFilesList() As String = {"*.gif", "*.png", ".bmp", "*.jpeg", "*.jpg"} ' D0657
        Public _MHT_Fix_Images_Ext As Boolean = True        ' D4598

        Private Const _INFODOC_CONTROL_MAX_LEN As Integer = 300     ' D4344 + D4345
        Private Const _INFODOC_CONTROL_MAX_ROWS As Integer = 5      ' D4372

        Public Const _OPT_INFODOC_IMG_MAX_SIZE = 5 * 1024 * 1024   ' D7265
        Public Const _OPT_INFODOC_PARSE_PLAIN_LINKS As Boolean = True   ' D7453

        Public Const _TEMPL_EMPTY_INFODOC As String = "<!DOCTYPE html><html xmlns='http://www.w3.org/1999/xhtml'><head><title>{1}</title><meta http-equiv='Cache-Control' content='no-cache, no-store, must-revalidate'/><meta http-equiv='Pragma' content='no-cache'/><meta http-equiv='Expires' content='0'/><link rel='icon' href='/Images/favicon/favicon.ico' type='image/x-icon'/><link href='{2}deco.css' type='text/css' rel='stylesheet'/><link href='{2}main.css' type='text/css' rel='stylesheet'/></head><body style='background:transparent !important; margin:1ex; height:auto; min-width: 32px; min-height:32px;'>{0}</body></html>"    ' D4371 + D4429 + D6504 + D6475

#Region "MHT file processing"

        ' D0131 ===
        ''' <summary>
        ''' Create MHT parser
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function CreateMHT() As Chilkat.Mht
            Dim mht As New Chilkat.Mht
            mht.UnlockComponent("DECERNCONSMHT_9RZdTwUd9Y1P")   ' L0151
            mht.UseCids = False
            Return mht
        End Function

        ' D0657 ===
        ''' <summary>
        ''' Encode HTML string as MHT string
        ''' </summary>
        ''' <param name="sHTML"></param>
        ''' <param name="sBaseURL">Base URL for HTML</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function PackHTMLasMHT(ByVal sHTML As String, ByVal sBaseURL As String, ByVal sBasePath As String) As String
            Dim sMHT As String = ""
            Select Case _MHT_Encoder
                Case mhtEncode.mhtChilkat
                    sMHT = PackHTMLasMHTbyChilkat(sHTML, sBaseURL)
                Case mhtEncode.mhtAD
                    sMHT = PackHTMLasMHTbyAD(sHTML, sBasePath)
            End Select
            Return sMHT
        End Function
        ' D0657 ==

        Public Function PackHTMLasMHTbyChilkat(ByVal sHTML As String, ByVal sBaseURL As String) As String    ' D0657
            Dim mht As Chilkat.Mht = CreateMHT()
            ' D0611 ===
            mht.BaseUrl = sBaseURL
            mht.EmbedImages = True
            'mht.EmbedLocalOnly = False ' for ver 9.0
            ' D0611 ==
            Dim sMHTContent As String = mht.HtmlToMHT(sHTML)
            mht.Dispose()
            Return sMHTContent
        End Function

        ' D0657 ===
        Public Function PackHTMLasMHTbyAD(ByVal sHTML As String, ByVal sBasePath As String) As String
            Dim sMHTML As String = ""

            Dim sBoundaryName As String = String.Format("boundary_{0}", Date.Now().ToString("yyMMdd.HHmmss"))
            Dim sBoundaryTemplate As String = "--" + sBoundaryName + vbCrLf +
                                              "Content-Type: {1}" + vbCrLf +
                                              "Content-Transfer-Encoding: base64" + vbCrLf +
                                              "Content-Location: {2}" + vbCrLf + vbCrLf +
                                              "{0}" + vbCrLf + vbCrLf

            Dim sMediaContent As String = ""

            sHTML = FixImgSrcMedia(sHTML)   ' D3396

            If sBasePath <> "" Then

                sBasePath = (sBasePath.Trim + "\").Replace("\\", "\")    ' D0657
                If My.Computer.FileSystem.DirectoryExists(sBasePath) Then   ' D7569

                    Dim files As ReadOnlyCollection(Of String)
                    files = My.Computer.FileSystem.GetFiles(sBasePath, FileIO.SearchOption.SearchAllSubDirectories, _MHT_SearchFilesList)   ' D0657

                    For Each sFile As String In files

                        Dim mediaFileData As FileInfo = My.Computer.FileSystem.GetFileInfo(sFile)   ' D0655

                        Dim sLocation As String = sFile.Substring(sBasePath.Length).Replace("\", "/")   ' D0657
                        Dim sMimeType As String = ""
                        If sFile.ToLower.EndsWith(".gif") Then sMimeType = "image/gif"
                        If sFile.ToLower.EndsWith(".png") Then sMimeType = "image/png"
                        If sFile.ToLower.EndsWith(".bmp") Then sMimeType = "image/bmp"
                        If sFile.ToLower.EndsWith(".jpeg") Or sFile.ToLower.EndsWith(".jpg") Then sMimeType = "image/jpeg"

                        If sMimeType <> "" AndAlso _ParseAbsolutePaths Then  ' D0657
                            Dim idx As Integer = -1
                            Dim st As Integer = 0
                            Do
                                idx = sHTML.IndexOf(mediaFileData.Name, st)
                                If idx < 0 Then idx = sHTML.IndexOf(HttpUtility.UrlEncode(mediaFileData.Name), st)
                                If idx > 1 Then ' D1130
                                    Dim idx_st = idx - 1
                                    If sHTML(idx_st) <> """" AndAlso sHTML(idx_st) <> "'" AndAlso sHTML(idx_st) <> "=" AndAlso sHTML(idx_st) <> "\" AndAlso sHTML(idx_st) <> "/" Then idx_st = -1 ' D1130
                                    Do While idx_st > 1 AndAlso sHTML(idx_st) <> """" AndAlso sHTML(idx_st) <> "'" AndAlso sHTML(idx_st) <> "="
                                        idx_st -= 1
                                    Loop
                                    If idx_st > 0 Then  ' D1130
                                        If sHTML(idx_st) = """" Or sHTML(idx_st) = "'" Or sHTML(idx_st) = "=" Then
                                            sHTML = sHTML.Substring(0, idx_st + 1) + _FILE_MHT_MEDIADIR + "/" + sHTML.Substring(idx)
                                        End If
                                        st = idx_st + (_FILE_MHT_MEDIADIR + "/" + mediaFileData.Name).Length
                                    Else
                                        st = idx + mediaFileData.Name.Length ' D1130
                                    End If
                                End If
                            Loop Until (idx < 0)
                        End If

                        sMediaContent += String.Format(sBoundaryTemplate, File_GetContentAsMIME(sFile), sMimeType, sLocation)   ' D0657
                    Next
                End If
            End If

            Dim EncodedBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(sHTML)  ' D0655
            Dim sEncodedHTML As String = Convert.ToBase64String(EncodedBytes, Base64FormattingOptions.InsertLineBreaks) ' D0655

            sMHTML = String.Format("Date: {3}" + vbCrLf +
                                   "Content-Type: multipart/related; " + vbCrLf +
                                   "  type=""text/html""; " + vbCrLf +
                                   "  boundary = ""{0}""; " + vbCrLf +
                                   "  charset=""utf-8"" " + vbCrLf + vbCrLf +
                                   "This is a multi-part message in MIME format." + vbCrLf + vbCrLf +
                                   "--{0}" + vbCrLf +
                                   "Content-Type: text/html;" + vbCrLf +
                                   "  charset = ""utf-8""" + vbCrLf +
                                   "Content-Transfer-Encoding: base64" + vbCrLf + vbCrLf +
                                   "{1}" + vbCrLf + vbCrLf +
                                   "{2}" +
                                   "--{0}--" + vbCrLf, sBoundaryName, sEncodedHTML, sMediaContent, Date.Now().ToString("R"))    ' D0655 + D4050
            sMHTML = String.Format("MIME-Version: 1.0" + vbCrLf +
                                   "X-ECC: {0}" + vbCrLf +
                                   "{1}", GetMD5(sMHTML), sMHTML)    ' D4050

            Return sMHTML
        End Function
        ' D0657 ==

        ''' <summary>
        ''' Decode MHT string to HTML file with unpack images
        ''' </summary>
        ''' <param name="MhtText"></param>
        ''' <param name="UnpackDir">Full path for writeable folder for unpack images</param>
        ''' <param name="htmlFilename">Name for created HTML</param>
        ''' <param name="partsSubDir">Sub-folder for included in HTML files (images, styles, etc)</param>
        ''' <returns></returns>
        ''' <remarks>Some files with web-unsafe names (non-urlencoded) could be created twice with encoded and non-encoded file names.</remarks>
        Public Function UnpackMHTasHTML(ByVal MhtText As String, ByVal UnpackDir As String, ByVal htmlFilename As String, ByVal sBaseURL As String, ByVal partsSubDir As String, Optional fForceAsHTML As Boolean = False) As Boolean     ' D0614 + D0657 + D4429
            ' D3283 ===
            Dim fResult As Boolean = False
            Dim md5_mht As String = String.Format("{0}\{1}\{2}.md5", UnpackDir, partsSubDir, GetMD5(MhtText))
            If Not My.Computer.FileSystem.FileExists(md5_mht) Then
                Dim sPath As String = UnpackDir + "\" + partsSubDir

                Dim tmpFiles As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = Nothing
                Try
                    tmpFiles = My.Computer.FileSystem.GetFiles(sPath)
                    For Each sFileName As String In tmpFiles
                        File_Erase(sFileName)
                    Next
                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)
                End Try
                ' D3283 ==

                Dim mht As Chilkat.Mht = CreateMHT()
                mht.BaseUrl = sBaseURL
                mht.NoScripts = True    ' D3283
                mht.ExcludeImagesMatching("*.md5")  ' D3283
                fResult = mht.UnpackMHTString(MhtText, UnpackDir, htmlFilename, partsSubDir)
                mht.Dispose()

                Dim HTMLText As String = "" ' D0611
                ' D0600 ===
                If fResult AndAlso Path.GetExtension(htmlFilename).ToLower = ".htm" AndAlso (_UseBlankBaseHyperlinks OrElse _ParseImageSrc) Then    ' D0614
                    HTMLText = File_GetContent(UnpackDir + "\" + htmlFilename, "WTF?")  ' D0611
                    ' D4429 ===
                    If HTMLText <> "WTF?" AndAlso (_UseBlankBaseHyperlinks OrElse fForceAsHTML) Then    ' D0611
                        DebugInfo("Try to parse hyperlinks...")
                        Dim sParsed As String = HTMLText
                        If _UseBlankBaseHyperlinks Then sParsed = BlankBaseHyperlinks(HTMLText)
                        If fForceAsHTML Then
                            Dim isHTML As Boolean = HTMLText.IndexOf("</html>", StringComparison.InvariantCultureIgnoreCase) > 0
                            If HTMLText = "" OrElse Not isHTML Then
                                sParsed = String.Format(_TEMPL_EMPTY_INFODOC, HTMLText, "", "/App_Themes/ec2018/")  ' D6504 // hardcoded path to styles
                            End If
                        End If
                        ' D4429 ==
                        If sParsed <> HTMLText Then
                            DebugInfo("Links parsed. Save changes.")
                            ' D1983 ===
                            Try
                                My.Computer.FileSystem.WriteAllText(UnpackDir + "\" + htmlFilename, sParsed, False)
                                HTMLText = sParsed
                            Catch ex As Exception
                                DebugInfo("Error on extract MHT file: " + ex.Message, _TRACE_RTE)
                            End Try
                            ' D1983 ==
                        End If
                    End If
                End If
                ' D0600 ==

                ' D0146 ===
                Dim sParsedHTML As String = HTMLText     ' D0611
                If _ParseImageSrc Then sParsedHTML = FixImgSrcMedia(sParsedHTML) ' D3396
                If fResult And My.Computer.FileSystem.DirectoryExists(sPath) Then
                    ' D3283 ===
                    Dim AllFiles As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = Nothing
                    Try
                        AllFiles = My.Computer.FileSystem.GetFiles(sPath)
                    Catch ex As Exception
                        DebugInfo(ex.Message, _TRACE_RTE)
                    End Try
                    If AllFiles IsNot Nothing Then
                        ' D3283 ==
                        For Each sFileName As String In AllFiles
                            If My.Computer.FileSystem.FileExists(sFileName) Then    ' D3396
                                Dim fSize As Long = My.Computer.FileSystem.GetFileInfo(sFileName).Length    ' D4666
                                If _MHT_Fix_Images_Ext AndAlso fSize > 10 Then  ' D4666
                                    Dim sExt As String = Path.GetExtension(sFileName).ToLower
                                    Dim sRealExt As String = sExt
                                    Dim sFileData As String = File.ReadAllText(sFileName).Substring(0, 6)   ' D4666
                                    If sFileData.Contains("PNG") AndAlso sExt.ToLower <> ".png" Then
                                        sRealExt = ".png"
                                    End If
                                    If sFileData.Contains("JFIF") AndAlso sExt.ToLower <> ".jpg" AndAlso sExt.ToLower <> ".jpeg" Then
                                        sRealExt = ".jpg"
                                    End If
                                    If sFileData.Contains("GIF89") AndAlso sExt.ToLower <> ".gif" Then
                                        sRealExt = ".gif"
                                    End If
                                    If sFileData.StartsWith("BM6") AndAlso sExt.ToLower <> ".bmp" Then
                                        sRealExt = ".bmp"
                                    End If
                                    If sExt <> sRealExt Then
                                        Dim sNewName As String = Path.GetFileName(sFileName.Replace(sExt, sRealExt))
                                        Try
                                            My.Computer.FileSystem.RenameFile(sFileName, sNewName)
                                            sParsedHTML = sParsedHTML.Replace(Path.GetFileName(sFileName), sNewName)
                                            DebugInfo("Rename file due to wrong extension: '" + sFileName + "' to '" + sNewName + "'")
                                            sFileName = sFileName.Replace(sExt, sRealExt)
                                        Catch ex As Exception
                                        End Try
                                    End If
                                End If

                                Dim sName As String = Path.GetFileName(sFileName)
                                Dim sDecodedName As String = sPath + "\" + HttpUtility.UrlDecode(sName)
                                Dim sEncodedName As String = sPath + "\" + HttpUtility.UrlPathEncode(Path.GetFileName(sDecodedName))

                                If sFileName.ToLower = sEncodedName.ToLower Then
                                    Try
                                        If Not My.Computer.FileSystem.FileExists(sDecodedName) Then My.Computer.FileSystem.CopyFile(sFileName, sDecodedName)
                                    Catch ex As Exception
                                    End Try
                                End If

                                'If sFileName.ToLower <> sDecodedName.ToLower Then
                                '    Try
                                '        If Not My.Computer.FileSystem.FileExists(sDecodedName) Then My.Computer.FileSystem.CopyFile(sFileName, sDecodedName)
                                '    Catch ex As Exception
                                '    End Try
                                'End If
                                'If sFileName.ToLower <> sEncodedName.ToLower Then
                                '    Try
                                '        If Not My.Computer.FileSystem.FileExists(sEncodedName) Then My.Computer.FileSystem.CopyFile(sFileName, sEncodedName)
                                '    Catch ex As Exception
                                '    End Try
                                'End If

                                ' D3396 ===
                                If sEncodedName.ToLower <> sDecodedName.ToLower AndAlso sFileName.ToLower = sEncodedName.ToLower AndAlso My.Computer.FileSystem.FileExists(sDecodedName) AndAlso
                                   My.Computer.FileSystem.GetFileInfo(sEncodedName).Length = My.Computer.FileSystem.GetFileInfo(sDecodedName).Length Then
                                    File_Erase(sEncodedName)
                                End If
                                ' D3396 ==

                                '' D0611 ===
                                'If HTMLText <> "WTF?" AndAlso _ParseImageSrc Then
                                '    sParsedHTML = Regex.Replace(sParsedHTML, "src\s*=\s*(['""]{1})(?<1>[^'"">]+)" + _FILE_MHT_MEDIADIR + "\/" + sName + "(['""]{1})", "src=""" + partsSubDir + "/" + sName + """", RegexOptions.IgnoreCase)    ' D0613 + D0614
                                'End If
                                '' D0611 ==
                            End If
                        Next
                    End If
                    ' D3283 ===
                    Try
                        My.Computer.FileSystem.WriteAllBytes(md5_mht, {}, False) ' D3283
                    Catch ex As Exception
                        DebugInfo("Unable save infodoc md5. " + ex.Message, _TRACE_RTE)
                    End Try
                    ' D3283 ==
                End If
                ' D0146 ==

                ' D0611 ===
                If (_ParseImageSrc OrElse _MHT_Fix_Images_Ext) AndAlso sParsedHTML <> HTMLText Then
                    DebugInfo("Links for images parsed. Save changes.")
                    My.Computer.FileSystem.WriteAllText(UnpackDir + "\" + htmlFilename, sParsedHTML, False)
                End If
                ' D0611 ==
            Else
                fResult = True  ' D3283
            End If
            Return fResult
        End Function

        Public Function Infodoc_GetInlineImages(HTML As String) As List(Of KeyValuePair(Of String, MemoryStream))   ' D6786
            Dim Lst As New List(Of KeyValuePair(Of String, MemoryStream))   ' D6786
            If Not String.IsNullOrEmpty(HTML) Then
                Dim imgRegex As New Regex("(?<=<img\s[\s\S]*?src="")(?:[^""]*\/)+(?=[^""]*\/)([^\/]*)\/([^""]+)", RegexOptions.IgnoreCase)
                Dim imgMatch As System.Text.RegularExpressions.Match = imgRegex.Match(HTML)
                While imgMatch.Success
                    Dim sLink = imgMatch.Value
                    If sLink.StartsWith("data:image", StringComparison.InvariantCultureIgnoreCase) Then
                        Dim idx As Integer = sLink.IndexOf(",")
                        If idx > 0 Then
                            Dim imgEncoded As String = sLink.Substring(idx + 1)
                            ' D6786 ===
                            Dim sType As String = ""
                            Dim idxFormat As Integer = sLink.IndexOf(";")
                            If idxFormat > 10 AndAlso idxFormat < idx Then sType = sLink.Substring(11, idxFormat - 11)   ' data:image/png;
                            ' D6786 ==
                            Try
                                Dim imgBytes As Byte() = Convert.FromBase64String(imgEncoded)
                                If imgBytes.GetLength(0) > 0 Then
                                    Dim msImg As New MemoryStream
                                    msImg.Write(imgBytes, 0, imgBytes.GetLength(0))
                                    Lst.Add(New KeyValuePair(Of String, MemoryStream)(sType, msImg))    ' D6786
                                End If
                            Catch ex As Exception   ' ignore when can't decode
                            End Try
                        End If
                    End If
                    imgMatch = imgMatch.NextMatch()
                End While
            End If

            Return Lst
        End Function

        ' D3396 ===
        Public Function FixImgSrcMedia(sHTML As String) As String
            Dim imgRegex As New Regex("<img \b[^\<\>]+?\bsrc\s*=\s*[""'](?<L>.+?)[""'][^\<\>]*?\>", RegexOptions.IgnoreCase)
            Dim imgMatch As System.Text.RegularExpressions.Match = imgRegex.Match(sHTML)
            While imgMatch.Success
                Dim sLink = imgMatch.Groups("L").Value
                Dim idxMedia As Integer = sLink.ToLower.IndexOf("/media/")
                If idxMedia > 0 Then
                    sHTML = sHTML.Replace(sLink, sLink.Substring(idxMedia + 1))
                End If
                imgMatch = imgMatch.NextMatch()
            End While
            Return sHTML
        End Function
        ' D3396 ==

        ''' <summary>
        ''' Create full path for MHT
        ''' </summary>
        ''' <param name="ProjectID"></param>
        ''' <param name="InfodocType"></param>
        ''' <param name="sInfodocID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Infodoc_Path(ByVal ProjectID As Integer, ActiveHierarchyID As Integer, ByVal InfodocType As reObjectType, ByVal sInfodocID As String, ByVal WRTParentNode As Integer) As String   ' D1003 + D1669
            ' D1003 ===
            Dim sNum As String = CStr(IIf(InfodocType = reObjectType.Description, "", sInfodocID))  ' D2683
            If InfodocType = reObjectType.AltWRTNode AndAlso WRTParentNode <> -1 Then sNum += "-" + WRTParentNode.ToString
            Return String.Format("{0}{1}\{2}{4}{5}{3}", _FILE_MHT_FILES, ProjectID, InfodocType.ToString.ToLower, sNum.ToLower, IIf(ActiveHierarchyID = 0, "", "-" + CStr(ActiveHierarchyID)), IIf(InfodocType = reObjectType.Description, "", "_"))   ' D1669 + D2683
            ' D1003 ==
        End Function

        Public Function Infodoc_URL(ByVal ProjectID As Integer, ActiveHierarchyID As Integer, ByVal InfodocType As reObjectType, ByVal sInfodocID As String, ByVal WRTParentNode As Integer) As String  ' D1003 + D1669
            ' D1003 ===
            Dim sNum As String = CStr(IIf(InfodocType = reObjectType.Description, "", sInfodocID))  ' D2683
            If InfodocType = reObjectType.AltWRTNode AndAlso WRTParentNode <> -1 Then sNum += "-" + WRTParentNode.ToString
            Return String.Format("{0}DocMedia/MHTFiles/{1}/{2}{4}{5}{3}/", _URL_ROOT, ProjectID, InfodocType.ToString.ToLower, sNum.ToLower, IIf(ActiveHierarchyID = 0, "", "-" + CStr(ActiveHierarchyID)), IIf(InfodocType = reObjectType.Description, "", "_"))   ' D1669 + D2683
            ' D1003 ==
        End Function

        ''' <summary>
        ''' Prepare folder for MHT pack/unpack
        ''' </summary>
        ''' <param name="ProjectID"></param>
        ''' <param name="InfodocType"></param>
        ''' <param name="sInfodocID"></param>
        ''' <param name="sError"></param>
        ''' <returns>True when folder for specified MHT is available</returns>
        ''' <remarks>When folder not exists this will be created with media sub-folder</remarks>
        Public Function Infodoc_Prepare(ByVal ProjectID As Integer, ActiveHierarchyID As Integer, ByVal InfodocType As reObjectType, ByVal sInfodocID As String, Optional ByRef sError As String = Nothing, Optional ByVal WRTParentNode As Integer = -1) As Boolean    ' D0151 + D1003 + D1669
            Dim path As String = Infodoc_Path(ProjectID, ActiveHierarchyID, InfodocType, sInfodocID, WRTParentNode)    ' D1003 + D1669
            Try
                If Not My.Computer.FileSystem.DirectoryExists(path) Then My.Computer.FileSystem.CreateDirectory(path)
                If Not My.Computer.FileSystem.DirectoryExists(path + "\" + _FILE_MHT_MEDIADIR) Then My.Computer.FileSystem.CreateDirectory(path + "\" + _FILE_MHT_MEDIADIR) ' D0132
            Catch ex As Exception
                If Not sError Is Nothing Then sError = ex.Message ' D0151
                Return False
            End Try
            Return My.Computer.FileSystem.DirectoryExists(path)
        End Function

        ' D6078 ===
        Public Function isHTML(sText As String) As Boolean
            If String.IsNullOrEmpty(sText) Then Return False
            Dim Text As String = sText.ToLower
            Return isMHT(sText) OrElse Text.Contains("<html") OrElse Text.Contains("<body") OrElse Text.Contains("<br") OrElse Text.Contains("<p") OrElse Text.Contains("<div") OrElse Text.Contains("/>")
        End Function
        ' D6078 ==

        ' D1277 ===
        Public Function isMHT(ByVal sText As String) As Boolean
            Return (sText IsNot Nothing AndAlso sText.IndexOf("MIME-Version:") >= 0)    ' D1278
        End Function
        ' D1277 ==

        ''' <summary>
        ''' Unpack MHT-Infodoc string to HTML-string
        ''' </summary>
        ''' <param name="ProjectID"></param>
        ''' <param name="InfodocType"></param>
        ''' <param name="sInfodocID"></param>
        ''' <param name="sInfodocContent"></param>
        ''' <param name="fPrepareFolderWrite"></param>
        ''' <param name="fFixPath"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Infodoc_Unpack(ByVal ProjectID As Integer, ActiveHierarchyID As Integer, ByVal InfodocType As reObjectType, ByVal sInfodocID As String, ByVal sInfodocContent As String, fPrepareFolderWrite As Boolean, fFixPath As Boolean, WRTParentNode As Integer, Optional fForceAsHTML As Boolean = False) As String     ' D0164 + D0657 + D1003 + D1669 + D2267 + D4429
            If InfodocType = reObjectType.Description Then sInfodocID = "description" ' D2683
            Dim path As String = Infodoc_Path(ProjectID, ActiveHierarchyID, InfodocType, sInfodocID, WRTParentNode)    ' D1003 + D1669
            Dim filename As String = String.Format("{0}.htm", sInfodocID)
            If fPrepareFolderWrite Then Infodoc_Prepare(ProjectID, ActiveHierarchyID, InfodocType, sInfodocID, Nothing, WRTParentNode) ' D1003 + D1669
            If sInfodocContent = "" Then Return ""
            If Not isMHT(sInfodocContent) AndAlso InfodocType <> reObjectType.SurveyQuestion Then Return sInfodocContent ' D1277 + D1278
            'If sInfodocContent.IndexOf("MIME-Version:") < 0 Then Return sInfodocContent
            Dim sError As String = ""   ' D0151
            If Infodoc_Prepare(ProjectID, ActiveHierarchyID, InfodocType, sInfodocID, sError, WRTParentNode) Then ' D0151 + D1004 + D1669
                UnpackMHTasHTML(sInfodocContent, path, filename, _URL_ROOT, _FILE_MHT_MEDIADIR, fForceAsHTML) ' D0614 + D0657 + D4429
                Dim sInfodoc As String = File_GetContent(path + "\" + filename)
                If fFixPath Then sInfodoc = sInfodoc.Replace(" src=""" + _FILE_MHT_MEDIADIR + "/", " src=""" + path.Replace(_FILE_ROOT, _URL_ROOT).Replace("\", "/") + "/" + _FILE_MHT_MEDIADIR + "/") ' D0164
                'Infodoc_GetInlineImages(sInfodoc)
                Return sInfodoc
            Else
                Return String.Format("Error: {0}", sError)
            End If
        End Function

        ''' <summary>
        ''' Encode HTML-string as MHT-string
        ''' </summary>
        ''' <param name="sInfodoc"></param>
        ''' <param name="sBaseURL"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Infodoc_Pack(ByVal sInfodoc As String, ByVal sBaseURL As String, ByVal sBasePath As String) As String ' D0611 + D0613 + D0657
            If sInfodoc = "" Then
                Return ""
            Else
                If isHTMLEmpty(sInfodoc) Then Return "" Else Return PackHTMLasMHT(sInfodoc, sBaseURL, sBasePath)  ' D6682
            End If
        End Function

        ''' <summary>
        ''' Try to erase folder with MHT-data
        ''' </summary>
        ''' <param name="ProjectID"></param>
        ''' <param name="InfodocType"></param>
        ''' <param name="sInfodocID"></param>
        ''' <remarks></remarks>
        Public Sub Infodoc_CleanItem(ByVal ProjectID As Integer, ActiveHierarchyID As Integer, ByVal InfodocType As reObjectType, ByVal sInfodocID As String, ByVal WRTParentID As Integer)   ' D1003 + D1669
            Dim path As String = Infodoc_Path(ProjectID, ActiveHierarchyID, InfodocType, sInfodocID, WRTParentID) ' D1003 + D1669
            Try ' D0144
                If My.Computer.FileSystem.DirectoryExists(path) Then File_DeleteFolder(path)
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)
            End Try
        End Sub


        ''' <summary>
        ''' Try to erase all MHT-folders for specified ProjectID
        ''' </summary>
        ''' <param name="ProjectID"></param>
        ''' <remarks></remarks>
        Public Sub Infodoc_CleanProject(ByVal ProjectID As Integer)
            Dim path As String = String.Format("{0}{1}", _FILE_MHT_FILES, ProjectID)
            Try ' D0144
                If My.Computer.FileSystem.DirectoryExists(path) Then File_DeleteFolder(path)
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)
            End Try
        End Sub
        ' D0131 ==

        ' D4154 ===
        Public Function Infodoc2Text(tProject As clsProject, sInfodoc As String, Optional fReadyAsHTML As Boolean = False) As String
            Dim sText As String = sInfodoc
            If Not String.IsNullOrEmpty(sText) AndAlso Not tProject Is Nothing Then
                If isMHT(sText) Then
                    Dim HID As Integer = CInt(IIf(tProject.isImpact, ECHierarchyID.hidImpact, ECHierarchyID.hidLikelihood))
                    Dim tmpID As String = GetRandomString(8, True, False)
                    Dim sPath As String = Infodoc_Path(tProject.ID, HID, reObjectType.Unspecified, tmpID, -1)
                    Dim sHTML As String = Infodoc_Unpack(tProject.ID, HID, reObjectType.Unspecified, tmpID, sText, False, False, -1)
                    sText = HTML2Text(sHTML)
                    File_DeleteFolder(sPath)
                End If
                If fReadyAsHTML Then sText = SafeFormString(sText).Replace(vbNewLine, "<br/>") ' D4155
            End If
            Return sText
        End Function
        ' D4154 ==

#End Region


#Region "RTF infodocs and files processing"

        ' D0165 ===
        ''' <summary>
        ''' Create RTF2HTML (HTML2RTF) parser.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Since version 8 no need to register EasyByte's DLLs in system. HTML2RTS also available.</remarks>
        Private Function CreateRTFParser() As RTF2HTMLv8
            Dim RTF2HTML As New RTF2HTMLv8     ' D0164
            RTF2HTML.LicenseKey = "fk0R0TGEkxZ2NEHvKFmz96qmliQV3iwehCBADSyFbLupsYtJo96sDDXfEsHt0JDWkA41laKO6TZr6RyacZ4y6Q=="    'v8
            RTF2HTML.Generator = "EasyByte RTF2HTMLv8 Convertor"
            RTF2HTML.Links = "yes"
            Return RTF2HTML
        End Function
        ' D0165 ==

        ' D0110 ===
        ''' <summary>
        ''' Parse string with RTF to HTML string
        ''' </summary>
        ''' <param name="sRTF">String with RTF. Should be not NULL.</param>
        ''' <param name="fParseImages">Use True for parse included images</param>
        ''' <param name="fRAW">Use True for parse as HTML without headers and other stuff</param>
        ''' <param name="sTitle">HTML page title</param>
        ''' <param name="sImagesPath">Path for write parsed images</param>
        ''' <param name="sImagesURL">URL for replace paths to parsed images</param>
        ''' <param name="sErrorMessage">Reference to variable with response when error occurred</param>
        ''' <returns>HTML string with parsed text</returns>
        ''' <remarks>Folder for images should be available for write. Use _RTF2HTML_MAGES_AS_PNG for parse palette images as PNG instantiated GIF.</remarks>
        Public Function ConvertRTF2HTML(ByVal sRTF As String, ByVal fParseImages As Boolean, Optional ByVal fRAW As Boolean = True, Optional ByVal sTitle As String = "", Optional ByVal sImagesPath As String = "", Optional ByVal sImagesURL As String = "", Optional ByVal sErrorMessage As String = "Can't parse RTF information document. Make sure appropriate DLLs were registered correctly. Error details: {0}") As String  ' D0079
            Dim sHTML As String = ""
            Dim RTF2HTML As RTF2HTMLv8 = CreateRTFParser()  ' D0164 + D0165
            Try
                RTF2HTML.XHTMLOutput = "no"
                RTF2HTML.ConvertImages = CStr(IIf(fParseImages, "yes", "no"))
                If fParseImages Then
                    If sImagesURL <> "" AndAlso Not sImagesURL.Contains("://") Then sImagesPath += "\" + sImagesURL.TrimStart(CChar("/")).Replace("/", "\") ' D4847
                    File_CreateFolder(sImagesPath)  ' D4847
                    RTF2HTML.ImageFormat = CStr(IIf(_RTF2HTML_IMAGES_AS_PNG, "png", "jpg"))
                    RTF2HTML.ImageFolder = sImagesPath
                End If

                ' D0139 ===
                ' For fix bug with unparsed BorderSpace RTF tags
                Dim RegBRSP As New System.Text.RegularExpressions.Regex("\\brsp([0-9]+)")
                sRTF = RegBRSP.Replace(sRTF, "")
                ' D0139 ==

                RTF2HTML.RTF_Text = sRTF
                If Not fRAW And sTitle <> "" Then RTF2HTML.HTML_Title = sTitle

                If fRAW Then sHTML = CStr(RTF2HTML.ConvertRawHtml) Else sHTML = CStr(RTF2HTML.ConvertRTF)
                ' AD: I found a strange bug with file-naming. Instead .jpg files .jpeg will be created.
                If fParseImages Then
                    sHTML = sHTML.Replace("file:///" + sImagesPath.TrimEnd(CChar("\")) + "\", If(sImagesURL = "", "", sImagesURL.TrimEnd(CChar("\")) + "\"))   ' D0638 + D4847
                    If Not _RTF2HTML_IMAGES_AS_PNG Then sHTML = sHTML.Replace("jpg", "jpeg")
                End If

            Catch ex As Exception
                sHTML = String.Format(sErrorMessage, ex.Message)
            Finally
                RTF2HTML = Nothing
            End Try

            Return sHTML
        End Function
        ' D0110 ==

        ' D0151 ===
        ''' <summary>
        ''' Convert HTML-string to RTF string
        ''' </summary>
        ''' <param name="sHTML">HTML</param>
        ''' <param name="sImagesPath">Path for images</param>
        ''' <param name="sErrorMessage">Reference to variable with response when error occurred</param>
        ''' <returns>HTML content, encoded as RTF string</returns>
        ''' <remarks></remarks>
        Public Function ConvertHTML2RTF(ByVal sHTML As String, ByVal sImagesPath As String, Optional ByVal sErrorMessage As String = "Can't parse HTML information to RTF document. Error details: {0}") As String
            Dim sRTF As String = ""
            Dim RTF2HTML As RTF2HTMLv8 = CreateRTFParser()    ' D0164 + D0165
            Try
                RTF2HTML.ImageFolder = sImagesPath
                RTF2HTML.HTML_Text = sHTML
                sRTF = RTF2HTML.ConvertHTML2RTF
            Catch ex As Exception
                sErrorMessage = String.Format(sErrorMessage, ex.Message)
            Finally
                RTF2HTML = Nothing
            End Try

            Return sRTF
        End Function
        ' D0151 ==

#End Region


#Region "Parse infodocs"

        ' D0115 ===
        Private Function ParseNodesRTFInfodoc(ByRef tNodesList As List(Of clsNode)) As Boolean 'C0384
            Dim fUpdated As Boolean = False
            For Each tNode As clsNode In tNodesList
                Dim sInfodoc As String = tNode.InfoDoc
                If sInfodoc <> "" Then
                    If sInfodoc.IndexOf("{\rtf1\") = 0 Then
                        ' D0139 ===
                        'Dim sError As String = ""  ' -D4794
                        Dim sTempFolder As String = File_CreateTempName()
                        File_CreateFolder(sTempFolder)
                        Dim sHTML As String = ConvertRTF2HTML(sInfodoc, True, False, "", sTempFolder, _FILE_MHT_MEDIADIR)   ' D4794 + D4847
                        tNode.InfoDoc = PackHTMLasMHT(sHTML, "", sTempFolder)   ' D0657
                        File_DeleteFolder(sTempFolder)
                        ' D0139 ==
                        fUpdated = True
                    End If
                End If
            Next
            Return fUpdated
        End Function
        ' D0115 ==

        ' D0110 ===
        Public Sub ParseInfodocsRTF2MHT(ByRef tProjectManager As clsProjectManager, ByVal tPipeParameters As clsPipeParamaters)   ' D0151 + D0174 + D0376
            If tProjectManager Is Nothing Or tPipeParameters Is Nothing Then Exit Sub
            Dim fUpdated As Boolean = False
            ' D0115 ===
            For Each tHierarhcy As clsHierarchy In tProjectManager.Hierarchies
                If ParseNodesRTFInfodoc(tHierarhcy.Nodes) Then fUpdated = True
            Next
            For Each tHierarhcy As clsHierarchy In tProjectManager.AltsHierarchies
                If ParseNodesRTFInfodoc(tHierarhcy.Nodes) Then fUpdated = True
            Next
            ' D0115 ==
            'If fUpdated Then tProject.SaveStructure()  ' - D0174
            ' D0165 ===
            fUpdated = False
            Dim HID As Integer = tProjectManager.ActiveHierarchy
            Dim AID As Integer = tProjectManager.ActiveAltsHierarchy
            'Dim sWelcome As String = tPipeParameters.PipeMessages.GetWelcomeText(HID, AID) 'C0139
            Dim sWelcome As String = tPipeParameters.PipeMessages.GetWelcomeText(PipeMessageKind.pmkText, HID, AID) 'C0139
            If ParseRTF2MHTPipeMessage(sWelcome) Then
                'tPipeParameters.PipeMessages.SetWelcomeText(HID, AID, sWelcome) 'C0139
                tPipeParameters.PipeMessages.SetWelcomeText(PipeMessageKind.pmkText, HID, AID, sWelcome) 'C0139
                fUpdated = True
            End If
            'Dim sThankYou As String = tPipeParameters.PipeMessages.GetThankYouText(HID, AID) 'C0139
            Dim sThankYou As String = tPipeParameters.PipeMessages.GetThankYouText(PipeMessageKind.pmkText, HID, AID) 'C0139
            If ParseRTF2MHTPipeMessage(sThankYou) Then
                'tPipeParameters.PipeMessages.SetThankYouText(HID, AID, sThankYou) 'C0139
                tPipeParameters.PipeMessages.SetThankYouText(PipeMessageKind.pmkText, HID, AID, sThankYou) 'C0139
                fUpdated = True
            End If
            'If fUpdated Then tProject.PipeParameters.PipeMessages.Save(PipeStorageType.pstDatabase, tProject.GetConnectionString)
            ' D0165 ==
            ' -D2291
            '' D2013 ===
            'Dim sDescription As String = tProjectManager.ProjectDescription
            'If ParseRTF2MHTPipeMessage(sDescription) Then
            '    tProjectManager.ProjectDescription = sDescription
            '    fUpdated = True
            'End If
            '' D2013 ==
        End Sub
        ' D0110 ==

        ' D0165 ===
        Public Function ParseRTF2MHTPipeMessage(ByRef sMessage As String) As Boolean
            Dim fUpdated As Boolean = False
            If sMessage <> "" Then
                If sMessage.IndexOf("{\rtf1\") = 0 Then
                    Dim sError As String = ""
                    Dim sTempFolder As String = File_CreateTempName()
                    File_CreateFolder(sTempFolder)
                    Dim sHTML As String = ConvertRTF2HTML(sMessage, True, False, "", sTempFolder, _FILE_MHT_MEDIADIR, sError)   ' D4847
                    sMessage = PackHTMLasMHT(sHTML, "", sTempFolder)    ' D0657
                    File_DeleteFolder(sTempFolder)
                    fUpdated = True
                End If
            End If
            Return fUpdated
        End Function

        Private Function ParseMHTPipeMessage(ByVal tProject As clsProject, ByRef sMessage As String, ByVal sID As String) As Boolean
            Dim fParsed As Boolean = False
            If sMessage <> "" Then
                If Infodoc_Prepare(tProject.ID, tProject.ProjectManager.ActiveHierarchy, reObjectType.PipeMessage, sID) Then    ' D1669
                    Dim sHTML As String = Infodoc_Unpack(tProject.ID, tProject.ProjectManager.ActiveHierarchy, reObjectType.PipeMessage, sID, sMessage, False, False, -1)   ' D1669
                    Dim sPath As String = Infodoc_Path(tProject.ID, tProject.ProjectManager.ActiveHierarchy, reObjectType.PipeMessage, sID, -1) + "\"    ' D1003 + D1669
                    Dim sError As String = ""
                    Dim sRTF As String = ConvertHTML2RTF(sHTML, sPath, sError)
                    If sError = "" And sRTF <> "" Then
                        sMessage = sRTF
                        fParsed = True
                    End If
                End If
            End If
            Return fParsed
        End Function
        ' D0165 ==

        ' D0151 ===
        Private Sub ParseNodesMHTInfodoc(ByVal tProject As clsProject, ByRef tNodesList As List(Of clsNode), ByVal tParseTo As ReportCommentType) 'C0384 + D0559
            For Each tNode As clsNode In tNodesList
                Dim sInfodoc As String = tNode.InfoDoc
                If sInfodoc <> "" Then
                    Dim iType As reObjectType = CType(IIf(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType)
                    Dim ID As String = tNode.NodeID.ToString
                    Dim WRTParentID As Integer = -1 ' D1003
                    If Infodoc_Prepare(tProject.ID, tProject.ProjectManager.ActiveHierarchy, iType, ID) Then    ' D1669
                        Dim sHTML As String = Infodoc_Unpack(tProject.ID, tProject.ProjectManager.ActiveHierarchy, iType, ID, sInfodoc, False, False, WRTParentID)   ' D1003 + D1669
                        Dim sPath As String = Infodoc_Path(tProject.ID, tProject.ProjectManager.ActiveHierarchy, iType, ID, WRTParentID) + "\" ' D0164 + D1003 + D1669
                        Dim sError As String = ""
                        Dim sRTF As String = ConvertHTML2RTF(sHTML, sPath, sError)
                        If sError = "" And sRTF <> "" Then
                            ' D0559 ===
                            Select Case tParseTo
                                Case ReportCommentType.rctComment
                                    tNode.Comment = sRTF
                                Case ReportCommentType.rctInfoDoc
                                    tNode.InfoDoc = sRTF
                                Case ReportCommentType.rctTag
                                    tNode.Tag = sRTF
                            End Select
                            ' D0559 ==
                        End If
                    End If
                End If
            Next
        End Sub

        Public Sub ParseInfodocsMHT2RTF(ByRef tProject As clsProject, Optional ByVal tParseTo As ReportCommentType = ReportCommentType.rctInfoDoc)   ' D0151 + D0559
            If tProject Is Nothing Then Exit Sub

            tProject.ProjectManager.MoveNodesInfoDocsToAdvanced()

            For Each tHierarchy As clsHierarchy In tProject.ProjectManager.Hierarchies
                ParseNodesMHTInfodoc(tProject, tHierarchy.Nodes, tParseTo)  ' D0559
            Next
            For Each tHierarhcy As clsHierarchy In tProject.ProjectManager.AltsHierarchies
                ParseNodesMHTInfodoc(tProject, tHierarhcy.Nodes, tParseTo)  ' D0559
            Next
            ' D0165 ===
            Dim HID As Integer = tProject.ProjectManager.ActiveHierarchy
            Dim AID As Integer = tProject.ProjectManager.ActiveAltsHierarchy

            ' -D3910 disable due to avoid convert original welcome/thank you MHT to RTF since ECD can't edit it
            'If tParseTo = ReportCommentType.rctInfoDoc Then     ' D0559
            '    'C0139===
            '    Dim sWelcome As String = tProject.PipeParameters.PipeMessages.GetWelcomeText(PipeMessageKind.pmkText, HID, AID) 'C0139
            '    If ParseMHTPipeMessage(tProject, sWelcome, "welcome") Then tProject.PipeParameters.PipeMessages.SetWelcomeText(PipeMessageKind.pmkText, HID, AID, sWelcome) 'C0139
            '    Dim sThankYou As String = tProject.PipeParameters.PipeMessages.GetThankYouText(PipeMessageKind.pmkText, HID, AID) 'C0139
            '    If ParseMHTPipeMessage(tProject, sThankYou, "thankyou") Then tProject.PipeParameters.PipeMessages.SetThankYouText(PipeMessageKind.pmkText, HID, AID, sThankYou) 'C0139
            '    'C0139==
            '    ' -D2291
            '    'Dim sDesciption As String = tProject.ProjectManager.ProjectDescription  ' D2013
            '    'If ParseMHTPipeMessage(tProject, sDesciption, "description") Then tProject.ProjectManager.ProjectDescription = sDesciption ' D2013
            '    ' D0165 ==
            'End If
        End Sub
        ' D0151 ==

#End Region


        ' D3695 ===
        Public Function GetQuickHelpObjectID(tEvalType As ecEvaluationStepType, tNode As clsNode) As String
            Dim ObjID As String = tEvalType.ToString
            If tNode IsNot Nothing Then ObjID += String.Format("_{0}{1}", IIf(tNode.IsAlternative, "alt", "obj"), tNode.NodeID)
            Return ObjID
        End Function
        ' D3695 ==

        ' D4345 ===
        Public Function GetControlInfodoc(tPrj As clsProject, tCtrl As clsControl, fPrepareForShow As Boolean) As String
            Dim sInfodoc As String = ""
            If tPrj IsNot Nothing AndAlso tCtrl IsNot Nothing Then
                sInfodoc = SafeFormString(tCtrl.InfoDoc)
                Dim sRealInfodoc As String = tPrj.ProjectManager.InfoDocs.GetCustomInfoDoc(tCtrl.ID, Guid.Empty)
                If Not String.IsNullOrEmpty(sRealInfodoc) AndAlso isMHT(sRealInfodoc) Then
                    sInfodoc = Infodoc_Unpack(tPrj.ID, tPrj.ProjectManager.ActiveHierarchy, reObjectType.Control, tCtrl.ID.ToString, sRealInfodoc, True, True, -1)
                Else
                    sInfodoc = sInfodoc.Replace(vbCrLf, "<br>") ' D4368
                End If
                If fPrepareForShow Then
                    ' D4371 ===
                    sInfodoc = SafeFormString(HTML2Text(sInfodoc)).Replace(vbNewLine + vbNewLine + vbNewLine, vbNewLine + vbNewLine)
                    Dim fShowMore As Boolean = False
                    If sInfodoc.Length > _INFODOC_CONTROL_MAX_LEN OrElse tCtrl.InfoDoc <> sInfodoc Then
                        If sInfodoc.Length > _INFODOC_CONTROL_MAX_LEN + _INFODOC_CONTROL_MAX_ROWS * 2 Then sInfodoc = sInfodoc.Substring(0, _INFODOC_CONTROL_MAX_LEN + _INFODOC_CONTROL_MAX_ROWS * 2)
                        fShowMore = True
                    End If
                    Dim sLines() As String = sInfodoc.Replace(vbNewLine + vbNewLine, vbNewLine).Split(CChar(vbNewLine))
                    If sLines.Length > _INFODOC_CONTROL_MAX_ROWS Then
                        sInfodoc = ""
                        For i = 0 To _INFODOC_CONTROL_MAX_ROWS - 1
                            sInfodoc += CStr(IIf(sInfodoc = "", "", vbNewLine)) + sLines(i)
                        Next
                        fShowMore = True
                    End If
                    sInfodoc = sInfodoc.Trim(CChar(vbNewLine)).Trim.Replace(vbNewLine, "<br>")
                    If fShowMore Then sInfodoc = ShortString(sInfodoc, _INFODOC_CONTROL_MAX_LEN) + "&hellip;&nbsp;%%viewmore%%"
                    ' D4371 ==
                End If
            End If
            Return sInfodoc
        End Function

        Public Function SetControlInfodoc(tPrj As clsProject, tCtrl As clsControl, sInfodoc As String, sBaseURL As String, Optional SaveLogMsg As String = Nothing, Optional sPackedInfodoc As String = Nothing) As Boolean
            Dim fResult As Boolean = False
            If tPrj IsNot Nothing AndAlso tCtrl IsNot Nothing Then

                Dim sBasePath As String = Infodoc_Path(tPrj.ID, tPrj.ProjectManager.ActiveHierarchy, reObjectType.Control, tCtrl.ID.ToString, -1)
                Dim sContent As String = Infodoc_Pack(sInfodoc, sBaseURL, sBasePath)
                If sPackedInfodoc IsNot Nothing Then sPackedInfodoc = sContent

                tPrj.ProjectManager.InfoDocs.SetCustomInfoDoc(sContent, tCtrl.ID, Guid.Empty)
                fResult = tPrj.ProjectManager.StorageManager.Writer.SaveInfoDocs()

                tCtrl.InfoDoc = HTML2Text(sInfodoc)
                With tPrj.ProjectManager
                    .Controls.WriteControls(ECModelStorageType.mstCanvasStreamDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With

                If Not String.IsNullOrEmpty(SaveLogMsg) Then tPrj.onProjectSaved.Invoke(tPrj, SaveLogMsg, False, ShortString(tCtrl.Name, 40))
            End If
            Return fResult
        End Function
        ' D4345 ==

    End Module

End Namespace