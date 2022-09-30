Imports System.Web
Imports System.Configuration
Imports ECCore
Imports System.Collections.Specialized
Imports System.Net.Mail ' D1357
Imports System.Net

Namespace ExpertChoice.Service

    Public Module Common

        ''' <summary>
        ''' Show "Info" tag in trace output (by default)
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _TRACE_INFO As String = "Info"

        ''' <summary>
        ''' Show "Warning" tag in trace output (like a notification)
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _TRACE_WARNING As String = "Warning"

        ''' <summary>
        ''' Show "RTE" tag in trace output (As usual, this is critical situations)
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _TRACE_RTE As String = "RTE"

        ''' <summary>
        ''' HTTPContext for create and write trace info
        ''' </summary>
        ''' <remarks></remarks>
        Public _Trace_PageContext As HttpContext = Nothing  ' D0010

        Public isTraceEnabled As Boolean = False

        Public _OPT_USE_CUSTOM_SETTINGS As Boolean = False  ' D3821

        ''' <summary>
        ''' Show debug message to Trace context and debug output
        ''' </summary>
        ''' <param name="sMessage">Message for output, should be non-empty</param>
        ''' <param name="sCategory">Category as massage tag (use _TRACE_* consts, _TRACE_INFO used by default)</param>
        ''' <remarks>Written to Debug and Trace context when "TraceEnabled" option is turned on in web.config file</remarks>
        Public Sub DebugInfo(ByVal sMessage As String, Optional ByVal sCategory As String = _TRACE_INFO)
            Dim fShow As Boolean = sCategory = _TRACE_RTE OrElse sCategory = _TRACE_WARNING    ' D0330
            'If sMessage <> "" And WebConfigOption("TraceEnabled") = "1" Then
            If Not String.IsNullOrEmpty(sMessage) AndAlso Not String.IsNullOrEmpty(sCategory) AndAlso isTraceEnabled Then
                Dim OutMessage As String = sMessage.Replace(vbCrLf, " \n ")
                If _Trace_PageContext IsNot Nothing AndAlso _Trace_PageContext.Trace IsNot Nothing Then _Trace_PageContext.Trace.Write(sCategory, OutMessage)
                fShow = True   ' D0330
            End If
            If fShow Then System.Diagnostics.Debug.WriteLine(sMessage, String.Format("{0}.{1} / {2}", Now.ToLongTimeString, Now.Millisecond, sCategory)) ' D0183 + D0330
        End Sub

        ' D2413 ===
        Public Function GetClientIP(Optional tRequest As HttpRequest = Nothing) As String   ' D3208
            Dim sIP As String = ""
            Try
                Dim tIPS() As IPAddress = Dns.GetHostAddresses(Dns.GetHostName())
                If tIPS IsNot Nothing Then
                    For Each tIP As IPAddress In tIPS
                        If tIP.ToString.Length >= 7 AndAlso tIP.ToString.Length <= 17 Then sIP += CStr(IIf(sIP = "", "", ", ")) + tIP.ToString ' D3208
                    Next
                    If tRequest IsNot Nothing AndAlso sIP <> tRequest.UserHostAddress AndAlso Not tRequest.UserHostAddress.StartsWith("::") Then sIP += CStr(IIf(sIP = "", "", ",")) + tRequest.UserHostAddress.ToString ' D3208 + D5084 + D7616
                    'If sIP <> "" Then sIP = "[" + sIP + "]"
                End If
            Catch ex As Exception
            End Try
            Return sIP
        End Function
        ' D2413 ==

        ''' <summary>
        ''' Send email
        ''' </summary>
        ''' <param name="sFrom">Sender Address</param>
        ''' <param name="sTo">Recipient(s) address(es)</param>
        ''' <param name="sSubject">Letter subject</param>
        ''' <param name="sBody">Letter content (as plain MIMEd-text)</param>
        ''' <param name="sError">Reference to feedback string when error occurred</param>
        ''' <returns>True when mail is sent</returns>
        ''' <remarks>All SMTP setting should be described in web.config file. See "mailSettings" section.</remarks>
        Public Function SendMail(ByVal sFrom As String, ByVal sTo As String, ByVal sSubject As String, ByVal sBody As String, ByRef sError As String, Optional ByVal sBCC As String = "", Optional ByVal isHTML As Boolean = False, Optional ByVal fUseSSL As Boolean = False, Optional ByVal LinkedItems As List(Of LinkedResource) = Nothing) As Boolean   ' D0300 + D0758 + D1357 + D3292
            Dim fSent As Boolean = False
            Try
                sSubject = sSubject.Replace(vbCr, " ").Replace(vbLf, "").Trim   ' D1124
                ' D3291 ==
                '' AD: next one doesn't work, I see auto-wrapped on mail(), but not by RFC and first line with "Subject: " is longer than 78 chars
                'Dim st As Integer = 0
                'While sSubject.Length - st > 70
                '    sSubject = sSubject.Insert(st + 70, vbCr)
                '    st += 70
                'End While
                ' D3291 ==
                'If sSubject.Length > 75 Then sSubject = SubString(sSubject, 75) ' D1124 - D3291
                DebugInfo("Start to send e-mail", _TRACE_INFO)
                ' D7425 ===
                'Dim Email As New MailMessage(sFrom, sTo, sSubject, sBody)
                Dim Email As New MailMessage()
                Email.From = New MailAddress(sFrom.Trim)
                Email.To.Add(New MailAddress(sTo.Trim))
                Email.Subject = sSubject
                Email.Body = sBody
                If sBCC <> "" Then Email.Bcc.Add(New MailAddress(sBCC.Trim)) ' D0300 + D0322
                ' D7425 ==
                Email.IsBodyHtml = isHTML  ' D0300
                Email.Headers.Add("X-originating-IP", GetClientIP())    ' D2413
                Email.Headers.Add("X-Mailer", String.Format("Comparion Core // {0}", Dns.GetHostName()))
                ' D4183 ===
                If Not isHTML AndAlso sBody <> "" Then
                    Dim tmpHTML As String = sBody.ToLower
                    If Not tmpHTML.Contains("a href") AndAlso (tmpHTML.Contains("http") OrElse tmpHTML.Contains("www.")) Then
                        tmpHTML = ParseTextHyperlinks(sBody)
                        If tmpHTML <> sBody Then
                            sBody = tmpHTML.Trim().Replace(vbNewLine, vbCr).Replace(vbLf, vbCr).Replace(vbCr + vbCr, "<p>").Replace(vbCr, "<br/>")   ' D4537
                            sBody = String.Format("<html><head></head><body>{0}</body></html>", sBody)  ' D4537
                            isHTML = True
                        End If
                    End If
                End If
                ' D4183 ==
                ' D1357 ===
                If isHTML Then

                    ' D3292 ===
                    Email.BodyEncoding = System.Text.Encoding.UTF8
                    Email.SubjectEncoding = System.Text.Encoding.UTF8

                    Dim htmlView As AlternateView = AlternateView.CreateAlternateViewFromString(sBody, Nothing, "text/html")
                    htmlView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable

                    If LinkedItems IsNot Nothing Then
                        For Each tItem As LinkedResource In LinkedItems
                            htmlView.LinkedResources.Add(tItem)
                        Next
                    End If

                    Email.AlternateViews.Add(htmlView)

                    'Else

                    '    If AttachmentsList IsNot Nothing Then
                    '        For Each tAttachment As Attachment In AttachmentsList
                    '            Email.Attachments.Add(tAttachment)
                    '        Next
                    '    End If
                    ' D3292 ==
                End If
                ' D1357 ==
                Dim smtp As New System.Net.Mail.SmtpClient()
                If fUseSSL Then smtp.EnableSsl = True ' D0758
                smtp.Send(Email)
                fSent = True
            Catch ex As Exception
                ' D0168 ===
                sError = ex.Message
                ' D0169 ===
                If Not ex.InnerException Is Nothing Then
                    sError += "; " + ex.InnerException.Message
                    If Not ex.InnerException.InnerException Is Nothing Then sError += "; " + ex.InnerException.InnerException.Message
                End If
                ' D0169 ==
                DebugInfo(sError, _TRACE_RTE)
                ' D0168 ==
            End Try
            Return fSent
        End Function
        ' D0079 ==

        ' D0459 ===
        Public Function HasListPage(ByVal tPagesList() As Integer, ByVal tPageID As Integer) As Boolean
            If tPagesList Is Nothing Then Return False
            Return Array.IndexOf(tPagesList, tPageID) >= 0
        End Function
        ' D0459 ==

        ' D0459 ===
        Public Function HasListPage(ByVal tPagesList() As Integer, ByVal tPagesForCheck() As Integer) As Boolean
            If tPagesList Is Nothing Or tPagesForCheck Is Nothing Then Return False
            For Each tPageID As Integer In tPagesForCheck
                If HasListPage(tPagesList, tPageID) Then Return True
            Next
            Return False
        End Function
        ' D0459 ==

        ' D0129 ===
        ''' <summary>
        ''' Get clsNode By NodeID from ArrayList with clsNodes;
        ''' </summary>
        ''' <param name="Nodes">List of nodes</param>
        ''' <param name="NodeID">NodeID for search</param>
        ''' <returns>Nothing, when Node not found</returns>
        ''' <remarks>Used for cases, when direct assess to GetNodeByID is not available (for example, for search in GetNodesBelow() list)</remarks>
        Public Function GetNodeByID(ByVal Nodes As List(Of clsNode), ByVal NodeID As Integer) As clsNode 'C0384
            If Not Nodes Is Nothing Then
                For Each tNode As clsNode In Nodes
                    If tNode.NodeID = NodeID Then Return tNode
                Next
            End If
            Return Nothing
        End Function
        ' D0129 ==

        ' -D4228
        '' D0701 ===
        'Public Function GetNodeByGUID(ByVal Nodes As List(Of clsNode), ByVal GUID As Guid) As clsNode
        '    If Nodes IsNot Nothing Then ' D0843
        '        For Each node As clsNode In Nodes
        '            If node.NodeGuidID.Equals(GUID) Then
        '                Return node
        '            End If
        '        Next
        '    End If
        '    Return Nothing
        'End Function
        '' D0701 ==        

#Region "URI options and params"

        ''' <summary>
        ''' Get Parameter value by his name from NamevalueCollection (as usual, passed via request or form)
        ''' </summary>
        ''' <param name="ParamsList">List with named parameters</param>
        ''' <param name="sArgName">Parameter name for search, could be not exists in list</param>
        ''' <returns>Value of parameter or empty string when not found</returns>
        ''' <remarks></remarks>
        Public Function GetParam(ByVal ParamsList As Specialized.NameValueCollection, ByVal sArgName As String) As String
            If ParamsList Is Nothing Or sArgName Is Nothing Then Return ""
            If Not ParamsList(sArgName) Is Nothing Then Return RemoveBadTags(ParamsList(sArgName)) Else Return ""   ' D5040
        End Function

        ' D0178 ===
        ''' <summary>
        ''' Decode encoded URL (%) to Unicode string with all international chars
        ''' </summary>
        ''' <param name="sURL"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function URLDecode(ByVal sURL As String) As String
            If String.IsNullOrEmpty(sURL) Then Return "" ' D0184
            Dim _Args() As Byte = HttpUtility.UrlDecodeToBytes(sURL)
            Dim sArgs As String = ""
            For i As Integer = 0 To _Args.Length - 1
                sArgs += ChrW(_Args(i))
            Next
            Return sArgs
        End Function
        ' D0178 ==

        ' D0214 ===
        Public Function ParamByName(ByVal tParams As NameValueCollection, ByVal tNamesList() As String) As String ' D0466
            For Each sName As String In tNamesList
                Dim sVal As String = GetParam(tParams, sName)
                If Not String.IsNullOrEmpty(sVal) Then Return sVal ' D1535
            Next
            Return ""
        End Function
        ' D0214 ==

        ' D2297 ===
        Public Function HasParamByName(ByVal tParams As NameValueCollection, ByVal tNamesList() As String) As Boolean
            For Each sName As String In tNamesList
                Dim sVal As String = GetParam(tParams, sName)
                If sVal IsNot Nothing Then Return True ' D4691
            Next
            Return False
        End Function
        ' D2297 ==

        Public Function Param2Bool(ByVal tParams As NameValueCollection, ByVal sName As String) As Boolean 'A1042
            Dim retVal As Boolean = False
            Dim sVal As String = GetParam(tParams, sName)   ' D4030
            If Not String.IsNullOrEmpty(sVal) Then
                sVal = sVal.Trim.ToLower    ' D4030
                retVal = sVal <> "false" AndAlso sVal <> "0"
            End If
            Return retVal
        End Function

        Public Function Param2Int(ByVal tParams As NameValueCollection, ByVal sName As String) As Integer 'A1261
            Dim retVal As Integer = UNDEFINED_INTEGER_VALUE
            Dim tmpIntVal As Integer
            Dim tmpDblVal As Double
            Dim sVal As String = GetParam(tParams, sName)
            If Integer.TryParse(sVal, tmpIntVal) Then
                retVal = tmpIntVal
            Else 
                If String2Double(sVal, tmpDblVal) Then
                    retVal = CInt(tmpDblVal)
                End If
            End If
            Return retVal
        End Function

        'A1228 ===
        Public Function Param2IntList(sLst As String, Optional sSeparator As Char = CChar(",")) As List(Of Integer)
            Dim iIDs As List(Of Integer) = New List(Of Integer)
            Dim sIDs As String() = sLst.Split(sSeparator)
            For Each id As String In sIDs
                Dim i As Integer
                If Integer.TryParse(id, i) Then iIDs.Add(i)
            Next
            Return iIDs
        End Function
        'A1228 ==

        'A1506 ===
        Public Function Param2GuidList(sLst As String, Optional sSeparator As Char = CChar(",")) As List(Of Guid)
            Dim iIDs As List(Of Guid) = New List(Of Guid)
            Dim sIDs As String() = sLst.Split(sSeparator)
            For Each sId As String In sIDs
                If sId.Length >= 36 Then
                    Dim id As Guid = New Guid(sId.Trim(CType("""", Char())))
                    iIDs.Add(id)
                End If
            Next
            Return iIDs
        End Function
        'A1506 ==

        'A1231 ===
        Public Function Param2Array(ByVal tParams As NameValueCollection, ByVal sName As String, Optional sSeparator As Char = CChar(",")) As String()
            Dim sVal As String = GetParam(tParams, sName)
            Return sVal.Split(sSeparator)
        End Function
        'A1231 ==

#End Region


    End Module

End Namespace
