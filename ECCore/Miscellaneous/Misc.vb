Imports Canvas

Namespace ExpertChoice

    Public Module Misc

        Public Const FUNDED_ROUND_PRECISION As Integer = 4 'A0939
        Public Const CLIPBOARD_CHAR_UNDEFINED_VALUE As String = " " 'A0961

        'A0902 ===
        Public Function HTMLColor2Long(sColor As String) As Long
            Return Long.Parse("FF" + sColor.Replace("#", ""), Globalization.NumberStyles.HexNumber)
        End Function

        Public Function Long2HTMLColor(tColor As Long) As String
            Dim hCol As String = Hex(tColor - 4278190080) 'tColor - #FF000000
            While hCol.Length < 6
                hCol = "0" + hCol
            End While
            Return "#" + hCol
        End Function
        'A0902 ==

        'A0915 ===
        Public Function GetAlternativeColor(PM As ECCore.clsProjectManager, altIndex As Integer, altGuid As Guid) As String
            Dim tColor As String = ""
            Dim tAttrColor As Long = CLng(PM.Attributes.GetAttributeValue(ECCore.ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, altGuid, Guid.Empty))
            If tAttrColor >= 0 Then
                tColor = Misc.Long2HTMLColor(tAttrColor)
            Else
                tColor = GetPaletteColor(PM.Parameters.SynthesisColorPaletteId, altIndex, True)
            End If
            Return tColor
        End Function
        'A0915 ==

        Public Function GetPalette(id As Integer) As String()
            'Dim retVal As String()
            'Select Case id
            '    Case 1 'New Palette
            '        retVal = ColorPalette1.Split(",")
            '    Case 2 'Color Picker Palette
            '        retVal = ColorPalette2.Split(",")
            '    Case 3 'Rainbow Palette
            '        retVal = ColorPalette3Rainbow.Split(",")
            '    Case Else 'Default (Old) Palette
            '        retVal = ColorPalette0.Split(",")
            'End Select
            'Return retVal
            If id < 0 OrElse id >= ColorPalettes.Count Then id = 0
            Return ColorPalettes(id).Split(",")
        End Function

        Public Function GetPaletteColor(paletteID As Integer, colorIdx As Integer, Optional isAlt As Boolean = False) As String
            Dim pal = GetPalette(paletteID)
            Dim retVal = ""
            If isAlt Then
                retVal = pal(pal.length - 1 - (colorIdx Mod pal.length))
            Else
                retVal = pal(colorIdx Mod pal.length)
            End If
            Return retVal
        End Function

        Public ColorPalettes As String() = {"#2c75ff,#fa7000,#9d27a8,#478430,#e33000,#80bdff,#a10040,#0affe3,#00523c,#ffbde6,#00c49f,#7280c4,#009180,#6c3b2a,#9e2373,#f24961,#663d2e,#9600fa,#919100,#5c00f7,#a15f00,#cce6ff,#00465c,#adff69,#f24ba0,#0dff87,#ff8c47,#349400,#b3b300,#a10067,#ba544a,#edc2d1,#00e8c3,#3f0073,#5ec1f7,#6e00b8,#f5f5c4,#e33000,#52ba00,#ff943b,#0079db,#f0e6c0,#ffb517,#cf0076,#e8cfc9",
            "#336699,#99CCFF,#999933,#666699,#CC9933,#006666,#3399FF,#993300,#CCCC99,#669966,#FFCC66,#6699CC,#663366,#9999CC,#CC99CC,#669999,#CCCC66,#CC6600,#9999FF,#0066CC,#99CCCC,#99CC99,#FFCC00,#009999,#99CC33,#FF9900,#999966,#66CCCC,#339966,#CCCC33",
            "#5451FF,#00967F,#DB0000,#356383,#068200,#898200,#DB7C00,#DB008A,#9d27a8,#837CFF,#00C1A7,#FF0000,#367AAB,#09B500,#C1B853,#FF8C00,#FF0CA6,#B24FFF,#A6A3FF,#0BE2C2,#FF5656,#4791C5,#0CFF00,#FFFF00,#FFA33A,#FF3DBB,#C277FF,#B6B5FF,#23FFE1,#FFA8A8,#68A6D0,#70FF68,#FFFF56,#FFB260,#FF6DCC,#D39EFF,#C6D4FF,#9EFFEE,#FFE7E7,#8ABBDB,#A0FF6A,#FFFFA6,#FFC993,#FFA8E2,#E3AEFF", 
            "#e6261f,#eb7532,#f7d038,#a3e048,#49da9a,#34bbe6,#4355db,#d23be7,#DC3183,#8B48E1,#3C88E1,#3FCBC0,#76DD71,#CDD840,#F1A335,#E94E29"}

        Public Function CurrentPaletteID(PM As ECCore.clsProjectManager) As Integer
            If PM IsNot Nothing Then Return PM.Parameters.SynthesisColorPaletteId Else Return 1
        End Function

        Public AlternativeUniformColor As String = "#7d9b3c"
        Public ObjectiveUniformColor As String = "#060cc3"

        Public NoSpecificColor As String = "#cccccc" ' "No category"

        ' D4715 ===
        Public Function HasEmbeddedContent(sOptions As String, EmbedType As EmbeddedContentType) As Boolean
            Dim sPages As String() = sOptions.Trim.Split(vbNewLine)
            For Each sPage As String In sPages
                Dim sParams As String() = sPage.Trim.Split(CChar(";"))
                If sParams.Count > 1 Then
                    Dim Pg As Integer
                    If Integer.TryParse(sParams(0), Pg) AndAlso Pg = CInt(EmbedType) Then Return sParams(1).Trim = "1"
                End If
            Next
            Return False
        End Function

        Public Function SetEmbeddedContent(sOptions As String, EmbedType As EmbeddedContentType, Include As Boolean) As String
            Dim sResult As String = ""
            Dim sPages As String() = sOptions.Trim.Split(vbNewLine)
            Dim fUpdated As Boolean = False
            For Each sPage As String In sPages
                Dim sParams As String() = sPage.Trim.Split(CChar(";"))
                If sParams.Count > 1 Then
                    Dim Pg As Integer
                    If Integer.TryParse(sParams(0), Pg) Then
                        If ([Enum].IsDefined(GetType(EmbeddedContentType), CType(Pg, EmbeddedContentType))) Then
                            Dim Cont As EmbeddedContentType = CType(Pg, EmbeddedContentType)
                            If Cont <> EmbeddedContentType.None Then
                                If Cont = EmbedType Then
                                    sParams(1) = If(Include, "1", "0")
                                    fUpdated = True
                                End If
                                sResult += String.Format("{0}{1}", If(sResult = "", "", vbNewLine), String.Join(";", sParams))
                            End If
                        End If
                    End If
                End If
            Next
            If Not fUpdated Then
                sResult += String.Format("{0}{1};{2}", If(sResult = "", "", vbNewLine), CInt(EmbedType), If(Include, "1", "0"))
            End If
            Return sResult
        End Function
        ' D4715 ==

    End Module

End Namespace