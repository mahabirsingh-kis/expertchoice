Partial Public Class InfodocWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Private ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Public Function Upload(Optional path As String = "") As String
        Dim FileName As String = ""
        If path <> "" AndAlso MyComputer.FileSystem.DirectoryExists(Server.MapPath(path)) Then
            If Not path.EndsWith("/") Then path += "/"
            'If Not path.StartsWith("/") Then path = _URL_ROOT + path
            If Request.Files.Count > 0 Then
                Dim File As HttpPostedFile = Request.Files(0)
                FileName = SafeFileName(File.FileName)
                Dim sExt As String = System.IO.Path.GetExtension(FileName).ToLower()
                Select Case sExt
                    Case ".png", ".gif", ".jpg", ".jpeg"
                        If FileName.ToLower.StartsWith("blobid") Then FileName = String.Format("blob_{0}{1}", (Now.Ticks Mod 1000000), sExt)
                        If (File.ContentLength <= _OPT_INFODOC_IMG_MAX_SIZE) Then
                            File.SaveAs(Server.MapPath(path + FileName))
                        Else
                            FetchWithCode(HttpStatusCode.BadRequest, True, "File is too big. Limit is " + SizeString(_OPT_INFODOC_IMG_MAX_SIZE))
                        End If
                    Case Else
                        FetchWithCode(HttpStatusCode.BadRequest, True, "Invalid extension")
                End Select
            Else
                FetchWithCode(HttpStatusCode.BadRequest, True, "No file uploaded")
            End If
        Else
            FetchWithCode(HttpStatusCode.BadRequest, True, "Path is invalid")
        End If
        Return String.Format("{{""location"": ""{0}""}}", JS_SafeString(FileName))
    End Function


    Private Sub RAWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNoActiveProject()

        Select Case _Page.Action

            Case "upload"
                _Page.ResponseData = Upload(GetParam(_Page.Params, "path", True))

        End Select
    End Sub

End Class
