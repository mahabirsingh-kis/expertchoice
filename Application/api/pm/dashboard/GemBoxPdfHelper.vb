Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports GemBox.Pdf
Imports GemBox.Pdf.Filters
Imports GemBox.Pdf.Objects
Imports System.Runtime.CompilerServices

Module GemBoxPdfHelper 'AS/18229 incorporated the class
    <Extension>
    Public Sub AddWatermark(pages As PdfPages, watermarkPage As PdfPage, prepend As Boolean, Optional additionalContent As String = Nothing)

        Dim watermarkForm = PdfIndirectObject.Create(watermarkPage.ToForm())
        For Each page In pages
            page.AddWatermark(watermarkForm, prepend, additionalContent)
        Next

    End Sub

    <Extension>
    Private Sub AddWatermark(page As PdfPage, watermarkForm As PdfIndirectObject, prepend As Boolean, additionalContent As String)

        Dim content = New StringBuilder()
        If prepend AndAlso Not String.IsNullOrEmpty(additionalContent) Then
            content.Append(additionalContent)
        End If

        content.AppendLine("q")
        Dim rect = page.CropBox
        content.AppendFormat("/{0} Do", page.AddFormResource(watermarkForm)).AppendLine()
        content.AppendLine("Q")

        If Not prepend AndAlso Not String.IsNullOrEmpty(additionalContent) Then
            content.Append(additionalContent)
        End If

        page.AddContent(content.ToString(), prepend)

    End Sub

    <Extension>
    Private Function ToForm(page As PdfPage) As PdfStream

        Dim formStream = PdfStream.Create()
        Dim formDictionary = formStream.Dictionary

        ' http//www.adobe.com/content/dam/acom/en/devnet/pdf/PDF32000_2008.pdf#page=226
        formDictionary.Add(PdfName.Create("Subtype"), PdfName.Create("Form"))
        ' http://www.adobe.com/content/dam/acom/en/devnet/pdf/PDF32000_2008.pdf#page=96
        Dim pageCropBox = page.CropBox

        formDictionary.Add(
            PdfName.Create("BBox"),
            PdfArray.Create(
                PdfNumber.Create(pageCropBox.Left),
                PdfNumber.Create(pageCropBox.Bottom),
                PdfNumber.Create(pageCropBox.Right),
                PdfNumber.Create(pageCropBox.Top)))

        ' http://www.adobe.com/content/dam/acom/en/devnet/pdf/PDF32000_2008.pdf#page=85
        Dim resourcesKey = PdfName.Create("Resources")
        Dim pageResources = CType(page.GetDictionary()(resourcesKey).AsDirect(), PdfBasicContainer)
        Dim formResources = pageResources.Clone(False)
        formDictionary.Add(resourcesKey, formResources)
        formStream.Filters.AddFilter(PdfFilterType.FlateDecode)

        Using formStreamData = formStream.Open(PdfStreamDataMode.Write, PdfStreamDataState.Decoded)
            For Each pageContentsStream In page.GetPageContents()
                Using pageStreamData = pageContentsStream.Open(PdfStreamDataMode.Read, PdfStreamDataState.Decoded)
                    pageStreamData.CopyTo(formStreamData)
                End Using
            Next
        End Using

        Return formStream

    End Function

    <Extension>
    Private Function GetPageContents(page As PdfPage) As IEnumerable(Of PdfStream)

        ' http://www.adobe.com/content/dam/acom/en/devnet/pdf/PDF32000_2008.pdf#page=85
        Dim pageContents = page.GetDictionary()(PdfName.Create("Contents")).AsDirect()
        Dim pageStreams As New List(Of PdfStream)

        If TypeOf pageContents Is PdfStream Then pageStreams.Add(DirectCast(pageContents, PdfStream))

        If TypeOf pageContents Is PdfArray Then
            For Each item In DirectCast(pageContents, PdfArray)
                pageStreams.Add(DirectCast(item.AsDirect(), PdfStream))
            Next
        End If

        Return pageStreams

    End Function

    <Extension>
    Private Function AddFormResource(page As PdfPage, form As PdfIndirectObject) As String

        Dim resources = CType(page.GetDictionary()(PdfName.Create("Resources")).AsDirect(), PdfDictionary)

        Dim xObjectKey = PdfName.Create("XObject")
        Dim xObjectValue As PdfBasicObject = Nothing
        If Not resources.TryGetValue(xObjectKey, xObjectValue) Then
            xObjectValue = PdfDictionary.Create()
            resources.Add(xObjectKey, xObjectValue)
        End If

        Dim xObjects = CType(xObjectValue.AsDirect(), PdfDictionary)
        Dim name As PdfName = Nothing
        Dim counter As Integer = 0

        Do
            counter += 1
            name = PdfName.Create("WatermarkForm" & counter)
        Loop While xObjects.ContainsKey(name)

        xObjects.Add(name, form)
        Return name.ToString()

    End Function

    <Extension>
    Private Sub AddContent(page As PdfPage, content As String, prepend As Boolean)

        Using contentData = New MemoryStream()

            Dim bytes As Byte()
            If prepend Then
                bytes = Encoding.ASCII.GetBytes(content)
                contentData.Write(bytes, 0, bytes.Length)
            End If

            bytes = Encoding.ASCII.GetBytes("q" & vbCrLf)
            contentData.Write(bytes, 0, bytes.Length)

            Dim pageContent = page.GetOrCreateContentStream()
            Using pageContentData = pageContent.Open(PdfStreamDataMode.Read, PdfStreamDataState.Decoded)
                pageContentData.CopyTo(contentData)
            End Using

            bytes = Encoding.ASCII.GetBytes("Q" & vbCrLf)
            contentData.Write(bytes, 0, bytes.Length)

            If Not prepend Then
                bytes = Encoding.ASCII.GetBytes(content)
                contentData.Write(bytes, 0, bytes.Length)
            End If

            contentData.Position = 0
            Using pageContentData = pageContent.Open(PdfStreamDataMode.Write, PdfStreamDataState.Decoded)
                contentData.CopyTo(pageContentData)
            End Using

        End Using

    End Sub

    <Extension>
    Private Function GetOrCreateContentStream(ByVal page As PdfPage) As PdfStream

        ' http://www.adobe.com/content/dam/acom/en/devnet/pdf/PDF32000_2008.pdf#page=85
        Dim pageContents = page.GetDictionary()(PdfName.Create("Contents")).AsDirect()

        Select Case pageContents.GetType()

            Case GetType(PdfStream)
                Return DirectCast(pageContents, PdfStream)

            Case GetType(PdfArray)
                Dim stream = PdfStream.Create()
                stream.Filters.AddFilter(PdfFilterType.FlateDecode)
                Using streamData = stream.Open(PdfStreamDataMode.Write, PdfStreamDataState.Decoded)
                    For Each pageContentsArrayItem In DirectCast(pageContents, PdfArray)
                        Using pageContentsStreamData = (DirectCast(pageContentsArrayItem.AsDirect(), PdfStream)).Open(PdfStreamDataMode.Read, PdfStreamDataState.Decoded)
                            pageContentsStreamData.CopyTo(streamData)
                        End Using
                        streamData.WriteByte(32)
                    Next
                End Using
                page.GetDictionary()(PdfName.Create("Contents")) = PdfIndirectObject.Create(stream)
                Return stream

            Case Else
                Dim stream = PdfStream.Create()
                stream.Filters.AddFilter(PdfFilterType.FlateDecode)
                stream.Open(PdfStreamDataMode.Write, PdfStreamDataState.Decoded).Close()
                page.GetDictionary().Add(PdfName.Create("Contents"), PdfIndirectObject.Create(stream))
                Return stream

        End Select

    End Function

    <Extension>
    Private Function AsDirect(ByVal obj As PdfBasicObject) As PdfBasicObject
        Return If(obj.ObjectType <> PdfBasicObjectType.IndirectObject, obj, (DirectCast(obj, PdfIndirectObject)).Value)
    End Function

End Module