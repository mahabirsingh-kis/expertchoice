Imports System.Drawing
Imports ExpertChoice.Service

Namespace ExpertChoice.Data

    Public Class clsPairwiseData

#Region "Constants and parameters"

        Public Const MapName As String = "pwmap"    ' D0216 + D1945

        ' D0421 ===
        Public Const Scale_Tiny As String = "tiny"
        Public Const Scale_Tiny_Up As String = "tinyup"
        Public Const Scale_Small As String = "small"
        Public Const Scale_Regular As String = ""
        ' D0421 ==

        Public Shared VerbalHints() As String = {"Equal", "Moderately", "Strongly", "Very Strongly", "Extremely"}
        Public Shared VerbalTinyHints() As String = {"Eq", "M", "S", "VS", "E"}
        Public ScalesCount As Integer = VerbalHints.Length - 1

        Public LabelHeight As Integer = 8   ' D1487 + D6254
        Public fontLabel As String = "Arial"    ' D1487
        Public clrLabel As Color = Color.FromArgb(51, 51, 51)

        Public PointWidth As Integer = 32
        Public PointHeight As Integer = LabelHeight + 4
        Public PointMargin As Integer = 3
        Public clrPoints As Color = Color.FromArgb(204, 204, 204)

        Public clrBackground As Color = Color.White

        Public MarginHeight As Integer = 5
        Public MarginWidth As Integer = 2 * PointWidth

        Public ImgWidth As Integer = (ScalesCount * 2 + 1) * PointWidth + 2 * MarginWidth
        Public ImgHeight As Integer = PointHeight * ScalesCount + 2 * MarginHeight

        Public YCoeff As Integer = 1    ' D0199
        Public YOffset As Integer = 0   ' D0199

        Private _ScaleType As String = ""    ' D0216

        Public ReadOnly Property ImgXCenter() As Integer
            Get
                Return ImgWidth \ 2
            End Get
        End Property

        Public Property ScaleType() As String
            Get
                Return _ScaleType
            End Get
            Set(ByVal value As String)
                _ScaleType = value
                Select Case _ScaleType
                    Case Scale_Small    ' D0421
                        ' more narrow for synchronous
                        PointWidth = 30
                        ImgWidth = 380 '(VerbalScale.ScalesCount * 2 + 1) * VerbalScale.PointWidth + 2 * VerbalScale.MarginWidth
                        YCoeff = -1
                        YOffset = ImgHeight

                    Case Scale_Tiny, Scale_Tiny_Up  ' D0421
                        ' only capital chars
                        PointWidth = 30
                        MarginWidth = LabelHeight
                        ImgWidth = (ScalesCount * 2) * PointWidth + 4 * MarginWidth
                        ImgHeight = PointHeight + MarginHeight
                        YCoeff = CInt(IIf(_ScaleType = Scale_Tiny, -1, 1))    ' D0421
                        YOffset = CInt(IIf(ScaleType = Scale_Tiny, ImgHeight, 0))   ' D0421

                    Case Else
                        PointWidth = 32
                        PointHeight = LabelHeight + 5
                        MarginHeight = 5
                        MarginWidth = 2 * PointWidth
                        ImgWidth = (ScalesCount * 2 + 1) * PointWidth + 2 * MarginWidth
                        ImgHeight = PointHeight * ScalesCount + 2 * MarginHeight
                        YCoeff = 1
                        YOffset = 0
                End Select
            End Set
        End Property

#End Region

        Private Function CreateVerbalScale(ByVal Hints() As String) As Bitmap   ' D0216
            Dim objBitmap As New Bitmap(ImgWidth, ImgHeight)
            Dim objGraphic As Graphics = Graphics.FromImage(objBitmap)
            'objGraphic.Clear(clrBackground)    ' D1436
            objGraphic.SmoothingMode = Drawing2D.SmoothingMode.None                 ' D1542
            objGraphic.TextRenderingHint = Text.TextRenderingHint.AntiAliasGridFit  ' D1542
            objBitmap.MakeTransparent()         ' D1436

            Dim labelBrush As New SolidBrush(clrLabel)
            Dim PointPen As New Pen(clrPoints, 1)

            For i As Integer = -ScalesCount To ScalesCount
                Dim x As Integer = ImgXCenter - i * PointWidth
                Dim y As Integer = ImgHeight - Math.Abs(i) * PointHeight - 2
                If i = 0 Then y -= LabelHeight + LabelHeight \ 2 + 7
                objGraphic.DrawLine(PointPen, x, YOffset + 0, x, YOffset + YCoeff * y)
                Dim x2 As Integer = x
                If (i <> 0) Then
                    x2 = x + CInt(IIf(i < 0, 1, -1)) * PointMargin
                    objGraphic.DrawLine(PointPen, x, YOffset + YCoeff * y, x2, YOffset + YCoeff * y)
                End If
                Dim sLabel As String = Hints(Math.Abs(i))
                Dim xl As Integer = x2
                If i < 0 Then xl += 1
                Dim yl As Integer = y + 1
                If i = 0 Then yl = y + LabelHeight - 2
                Dim stringFormat As New StringFormat()
                If i = 0 Then
                    stringFormat.Alignment = StringAlignment.Center
                Else
                    If i > 0 Then stringFormat.Alignment = StringAlignment.Far Else stringFormat.Alignment = StringAlignment.Near
                End If
                stringFormat.LineAlignment = StringAlignment.Center
                Dim labelFont As New Font(fontLabel, LabelHeight + If(i = 0, 1, 0), If(i = 0, FontStyle.Bold, FontStyle.Regular)) ' D6254
                objGraphic.DrawString(sLabel, labelFont, labelBrush, xl, YOffset + YCoeff * yl, stringFormat)
            Next

            Return objBitmap
        End Function

        ' D0216 ===
        Private Function CreateVerbalTinyScale(ByVal TinyHints() As String) As Bitmap

            Dim objBitmap As New Bitmap(ImgWidth, ImgHeight)
            Dim objGraphic As Graphics = Graphics.FromImage(objBitmap)
            'objGraphic.Clear(clrBackground)    ' D1436
            objGraphic.SmoothingMode = Drawing2D.SmoothingMode.None                 ' D1542
            objGraphic.TextRenderingHint = Text.TextRenderingHint.AntiAliasGridFit  ' D1542
            objBitmap.MakeTransparent()         ' D1436

            Dim labelBrush As New SolidBrush(clrLabel)
            Dim PointPen As New Pen(clrPoints, 1)

            For i As Integer = -ScalesCount To ScalesCount
                Dim x As Integer = ImgXCenter - i * PointWidth
                objGraphic.DrawLine(PointPen, x, YOffset + 0, x, YOffset + YCoeff * MarginHeight)
                Dim sLabel As String = ShortString(TinyHints(Math.Abs(i)), 6, True)
                Dim stringFormat As New StringFormat()
                stringFormat.Alignment = StringAlignment.Center
                stringFormat.LineAlignment = StringAlignment.Center
                Dim labelFont As New Font(fontLabel, LabelHeight + If(i = 0, 1, 0), If(i = 0, FontStyle.Bold, FontStyle.Regular)) ' D6254
                objGraphic.DrawString(sLabel, labelFont, labelBrush, x + 1, YOffset + YCoeff * (LabelHeight + MarginHeight - 1), stringFormat)
            Next

            Return objBitmap
        End Function
        ' D0216 ==

        Private Function CreateVerbalMap(ByVal sMapName As String, ByVal Hints() As String, ByVal UseLinks As Boolean) As String    ' D0216 + D1395
            Dim sMap As String = ""
            For i As Integer = -ScalesCount To ScalesCount
                Dim sLabel As String = Hints(Math.Abs(i))
                Dim x1 As Integer = ImgXCenter - i * PointWidth
                Dim w As Integer = CInt((0.85 * sLabel.Length + 1) * LabelHeight)
                Dim x2 As Integer = x1
                If i = 0 Then
                    x1 -= w \ 2
                    x2 += w \ 2
                Else
                    If i < 0 Then x2 += w Else x2 -= w
                End If
                Dim y As Integer = ImgHeight - Math.Abs(i) * PointHeight - PointHeight \ 2 - 1
                If i = 0 Then y -= LabelHeight - 1
                ' D0216 ===
                If UseLinks Then
                    sMap += String.Format("<area shape='rect' coords='{0},{3},{2},{1}' href='' onclick='return SetPW({4},{5})' onmouseover='this.title=""{6}""'>" + vbCrLf, x1, YOffset + YCoeff * y, x2, YOffset + YCoeff * (y + PointHeight - 1), 2 * Math.Abs(i) + 1, IIf(i > 0, 1, -1), SafeFormString(sLabel))   ' D1395 + D1842 + D1997 + D3706
                Else
                    sMap += String.Format("<area shape='rect' coords='{0},{3},{2},{1}' onmouseover='this.title=""{4}""'>" + vbCrLf, x1, YOffset + YCoeff * y, x2, YOffset + YCoeff * (y + PointHeight - 1), SafeFormString(sLabel))    ' D1842 + D3706
                End If
                ' D0216 ==
            Next
            If sMap <> "" Then sMap = String.Format("<map name='{0}'>" + vbCrLf + "{1}</map>", sMapName, sMap)
            Return sMap
        End Function

        ' D0216 ===
        Private Function CreateVerbalTinyMap(ByVal sMapName As String, ByVal Hints() As String, ByVal UseLinks As Boolean) As String    ' D0216 + D1395
            Dim sMap As String = ""
            For i As Integer = -ScalesCount To ScalesCount
                Dim sLabel As String = Hints(Math.Abs(i))
                Dim x As Integer = ImgXCenter - i * PointWidth
                Dim y As Integer = YOffset + YCoeff * (LabelHeight + MarginHeight - 1)
                ' D0216 ===
                If UseLinks Then
                    sMap += String.Format("<area shape='rect' coords='{0},{3},{2},{1}' href='' onclick='return SetPW({4},{5})' alt='{6}'>" + vbCrLf, x - PointWidth \ 2 + 2, YOffset + YCoeff * y + 1, x + PointWidth \ 2, YOffset + YCoeff * (y + PointHeight - 1) + 1, 2 * Math.Abs(i) + 1, IIf(i > 0, 1, -1), sLabel)   ' D1395 + D1842 + D1997
                Else
                    sMap += String.Format("<area shape='rect' coords='{0},{3},{2},{1}' alt='{4}'>", x - PointWidth \ 2 + 2, YOffset + YCoeff * y + 1, x + PointWidth \ 2, YOffset + YCoeff * (y + PointHeight - 1) + 1, sLabel) ' D1842
                End If
                ' D0216 ==
            Next
            If sMap <> "" Then sMap = String.Format("<map name='{0}'>" + vbCrLf + "{1}</map>", sMapName, sMap)
            Return sMap
        End Function

        Public Function GetScaleImage() As Bitmap
            If ScaleType.ToLower <> Scale_Tiny And ScaleType.ToLower <> Scale_Tiny_Up Then   ' D0421
                Return CreateVerbalScale(VerbalHints)
            Else
                Return CreateVerbalTinyScale(VerbalTinyHints)
            End If
        End Function

        Public Function GetScaleMap(ByVal sMapName As String, ByVal fUseLinks As Boolean) As String ' D1395 + D1997
            If ScaleType.ToLower <> Scale_Tiny And ScaleType.ToLower <> Scale_Tiny_Up Then   ' D0421
                Return CreateVerbalMap(sMapName, VerbalHints, fUseLinks)  ' D1395 + D1997
            Else
                Return CreateVerbalTinyMap(sMapName, VerbalHints, fUseLinks)  ' D1395 + D1997
            End If
        End Function
        ' D0216 ==

    End Class

End Namespace
