Imports System.IO
Public Class JSResourcesPage
    Inherits clsComparionCorePage

    Const NL As String = ""
    Const NB As String = """"   ' D4657

    Private swMain As New Stopwatch()

    Public Sub New()
        MyBase.New(_PGID_JS_RESOURCES)
        swMain.Start()
    End Sub

    ' D4657 ===
    Private Function GetResources() As String
        DebugInfo("Prepare the list of resources")
        Dim LastBreak As Integer = 0
        Dim Lines As String = ""
        If App.CurrentLanguage IsNot Nothing AndAlso App.DefaultLanguage IsNot Nothing Then
            For Each sRes As clsResourceParameter In App.DefaultLanguage.Resources.Parameters.Values
                Dim sValue As String = If(App.DefaultLanguage Is App.CurrentLanguage OrElse Not App.CurrentLanguage.Resources.ParameterExists(sRes.Name), sRes.Value, App.ResString(sRes.Name)) ' D6013
                Dim sNewLine As String = String.Format("{3}{2}{0}{2}:""{1}""" + NL, JS_SafeString(sRes.Name.ToLower.Trim), JS_SafeString(sValue, True).Replace(":", "\:"), NB, If(Lines = "", "", ","))  ' D4797 + D6035
                LastBreak += sNewLine.Length
                If LastBreak > 32000 AndAlso NL = "" Then
                    sNewLine += vbCrLf
                    LastBreak = 0
                End If
                Lines += sNewLine
            Next
        End If
        Lines = "var _resources={" + NL + Lines + "};"
        DebugInfo("List of resources has been created")
        Return Lines
    End Function
    ' D4657 ==

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        DebugInfo("Start request")
        ' D4657 ===
        Dim sResources As String = ""
        Dim DT As New DateTime?

        Dim sPath As String = _FILE_MHT_FILES + "Resources\"
        File_CreateFolder(sPath)
        DebugInfo("Folder checked")

        If App.CurrentLanguage IsNot Nothing AndAlso MyComputer.FileSystem.DirectoryExists(sPath) Then
            Dim ResxName As String = App.CurrentLanguage.ResxFilename
            Dim JSName As String = String.Format("{0}.js", App.CurrentLanguage.LanguageCode)
            Dim DTName As String = String.Format("{0}.cache", App.CurrentLanguage.LanguageCode)

            If MyComputer.FileSystem.FileExists(ResxName) Then
                DebugInfo("resx file exists")
                Dim fileData As FileInfo = MyComputer.FileSystem.GetFileInfo(ResxName)
                DT = fileData.LastWriteTime
                If fileData.CreationTime > DT Then DT = fileData.CreationTime
            End If

            Dim CacheDT As DateTime?
            Dim sSavedDT As String = File_GetContent(sPath + DTName, "")
            If sSavedDT <> "" Then CacheDT = BinaryStr2DateTime(sSavedDT)

            Dim fNeedCreate As Boolean = True
            If DT.HasValue AndAlso CacheDT.HasValue AndAlso Date2ULong(CacheDT.Value) >= Date2ULong(DT.Value) AndAlso MyComputer.FileSystem.FileExists(sPath + JSName) AndAlso Not CheckVar("reset", False) Then fNeedCreate = False
            DebugInfo("Need to create resx file: " + Bool2YesNo(fNeedCreate))

            If fNeedCreate Then
                sResources = GetResources()
                Dim sDT As String = Date2ULong(Now).ToString
                If DT.HasValue Then sDT = Date2ULong(DT.Value).ToString
                DebugInfo("Save resx file")
                MyComputer.FileSystem.WriteAllText(sPath + JSName, sResources, False)
                MyComputer.FileSystem.WriteAllText(sPath + DTName, sDT, False)
                DebugInfo("Resx file saved")
            Else
                sResources = File_GetContent(sPath + JSName, "")
            End If
        End If
        ' D4657 ==

        With Response
            DebugInfo("Response to client started")
            .ClearHeaders()
            .Clear()
            .ClearContent()
            .Buffer = True
            .BufferOutput = True
            .ContentType = "application/javascript"
            If DT.HasValue Then .Cache.SetLastModified(DT.Value)    ' D4657
            '.Cache.SetExpires(DateTime.Now.AddSeconds(_DEF_SESS_TIMEOUT))
            .Cache.SetCacheability(HttpCacheability.ServerAndPrivate And HttpCacheability.Public)
            '.Cache.VaryByParams("r") = True
            If App.CurrentLanguage IsNot Nothing Then .AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}.js""", App.CurrentLanguage.LanguageCode))  ' D4657
            '.AddHeader("Content-Length", Encoding.Unicode.GetByteCount(sResources).ToString)
            .Write(sResources)
            DebugInfo("Response to client finished")
            If .IsClientConnected Then .End()
        End With

    End Sub

    Private Sub JSResourcesPage_Unload(sender As Object, e As EventArgs) Handles Me.Unload
        swMain.Stop()
        'If Not IsPostBack Then 
        Debug.Print("Resources list generated on server for " + swMain.ElapsedMilliseconds.ToString + " ms.")
        'End If
    End Sub
End Class