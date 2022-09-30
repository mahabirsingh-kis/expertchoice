Imports System.Drawing
Imports System.Drawing.Imaging

Partial Class PWScaleImage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_UNKNOWN)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        ' D1436 ===
        Dim VerbalScale As clsPairwiseData = CreateImageScale(CheckVar("type", "").ToLower) ' D0216
        Dim dName As String = String.Format("{0}Scales", _FILE_MHT_FILES)
        Dim fName As String = String.Format("{0}\Scale_{1}_{2}.png", dName, App.LanguageCode, VerbalScale.ScaleType)

        Response.Clear()
        Response.ClearContent()
        Response.ContentType = "image/png"

        If Not MyComputer.FileSystem.FileExists(fName) Then
            If Not MyComputer.FileSystem.DirectoryExists(dName) Then File_CreateFolder(dName)

            Dim imgCodecInfo As ImageCodecInfo = Nothing
            Dim mimeType As String = "image/png"
            For Each tEnc As ImageCodecInfo In ImageCodecInfo.GetImageEncoders()
                If tEnc.MimeType.ToLower = mimeType Then imgCodecInfo = tEnc
            Next

            Dim img As Bitmap = VerbalScale.GetScaleImage()
            img.Save(fName, imgCodecInfo, Nothing)
        End If

        Response.TransmitFile(fName)
        ' D1436 ==

        If Response.IsClientConnected Then  ' D1232
            'Response.Flush()
            Response.End()
        End If

    End Sub

End Class

