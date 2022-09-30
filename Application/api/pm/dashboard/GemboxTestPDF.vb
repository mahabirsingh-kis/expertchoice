Imports GemBox.Pdf
Imports GemBox.Document
Imports System.IO

<Serializable> Public Class GemboxTestPDF 'AS/17533m added the class

    Private App As clsComparionCore
    Private PM As clsProjectManager
    Private pdfdoc As PdfDocument

    Public Function Example_AddWatermark(filepath As String) As PdfDocument 'AS/18229
        Using watermarkDoc = PdfDocument.Load(CreateWatermarkDocument("D:\My Projects\GemboxTest\AddWatermark [VB]\WatermarkImage.png"))

            Using exampleDoc = PdfDocument.Load("D:\My Projects\GemboxTest\AddWatermark [VB]\Example.pdf")
                Dim watermarkPage = watermarkDoc.Pages(0)
                exampleDoc.Pages.AddWatermark(watermarkPage, True)
                exampleDoc.Save(filepath)
                Return exampleDoc
            End Using
        End Using


    End Function

    Private Shared Function CreateWatermarkDocument(ByVal watermarkImage As String) As Stream 'AS/18229
        Dim doc = New DocumentModel()
        Dim sec = New Section(doc)
        Dim pic = New Picture(doc, "D:\My Projects\GemboxTest\AddWatermark [VB]\WatermarkImage.png")
        pic.Layout = New FloatingLayout(New HorizontalPosition(HorizontalPositionType.Absolute, HorizontalPositionAnchor.Page), New VerticalPosition(VerticalPositionType.Absolute, VerticalPositionAnchor.Page), New Size(sec.PageSetup.PageWidth, sec.PageSetup.PageHeight)) With {
            .WrappingStyle = TextWrappingStyle.BehindText
        }
        sec.Blocks.Add(New Paragraph(doc, pic))
        doc.Sections.Add(sec)
        Dim stream = New MemoryStream()
        doc.Save(stream, GemBox.Document.SaveOptions.PdfDefault)
        Return stream
    End Function

    Public Function Example_CreateParagraphs(filePath As String) As DocumentModel
        'source -- https://www.gemboxsoftware.com/document/examples/c-sharp-vb-net-write-word-file/302

        Dim document As DocumentModel = New DocumentModel()

        Dim text1 As String = Infodoc2Text(App.ActiveProject, PM.Hierarchies(0).Nodes(0).InfoDoc)

        ' Add new section with two paragraphs, containing some text and symbols.
        document.Sections.Add(
            New Section(document,
                New Paragraph(document,
                    New Run(document, "This is our first paragraph with symbols added on a new line."),
                    New SpecialCharacter(document, SpecialCharacterType.LineBreak),
                    New Run(document, ChrW(&HFC) & ChrW(&HF0) & ChrW(&H32)) With {.CharacterFormat = New CharacterFormat() With {.FontName = "Wingdings", .Size = 48}}),
                New Paragraph(document, text1),
                New Paragraph(document, "This is our third paragraph.")))

        Return document

    End Function
    Function HelloWorld(filepath As String) As PdfDocument

        Using pdfdoc As New PdfDocument()

            ' Add a first empty page.
            pdfdoc.Pages.Add()

            ' Add a second empty page.
            pdfdoc.Pages.Add()

            'pdfdoc.Save("D:\Temp\Hello World.pdf")
            pdfdoc.Save(filepath)
            Return pdfdoc

        End Using

    End Function

    Public Sub New(_App As clsComparionCore)
        App = _App
        PM = App.ActiveProject.ProjectManager

        ' If using Professional version, put your serial key below.
        GemBox.Pdf.ComponentInfo.SetLicense("AZNK-TASR-VC9J-1SFL")
        GemBox.Document.ComponentInfo.SetLicense("DN-2019Oct23-o9cPV8FqJpXV/58cTjlRakTEbT0YmbK2qfwJS4OkrVTGqcdjqxHlgRDfWwwv2q3j9qruUP258vApmepOW0Z46sRTc6Q==A")

        ' Create new empty document.
        pdfdoc = New PdfDocument()

    End Sub

End Class
