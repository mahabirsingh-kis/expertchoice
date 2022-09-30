Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlCheckPassword

        Inherits UserControl

        Private _ctrlPasswordControl As WebControl = Nothing
        Private _ctrlMessageClientID As String
        Private _useJScriptCode As Boolean = True

        Private _sPrefix As String = "Strength: "
        Private _sStrength() As String = {"Very Poor", "Weak", "Average", "Strong", "Excellent"}
        Private _tStrong As Integer = 2 ' D0283

        Private _coefLength As Double = 0.5
        Private _coefCase As Double = 0.15
        Private _coefAlphabetical As Double = 0.15
        Private _coefNumerical As Double = 0.1
        Private _coefSymbols As Double = 0.1

        Private _minLength As Integer = 8

        Private _ShowOnEmpty As Boolean = False ' D0281

        Public isDebug As Boolean = False

        Public Property PasswordControl() As WebControl
            Get
                Return _ctrlPasswordControl
            End Get
            Set(ByVal value As WebControl)
                _ctrlPasswordControl = value
            End Set
        End Property

        Public ReadOnly Property PasswordClientID() As String
            Get
                If _ctrlMessageClientID Is Nothing Then Return "" Else Return _ctrlPasswordControl.ClientID
            End Get
        End Property

        Public Property MessageClientID() As String
            Get
                Return _ctrlMessageClientID
            End Get
            Set(ByVal value As String)
                _ctrlMessageClientID = value
            End Set
        End Property

        Public Property PrefixResult() As String
            Get
                Return _sPrefix
            End Get
            Set(ByVal value As String)
                _sPrefix = value
            End Set
        End Property

        Public Property Strengths() As String()
            Get
                Return _sStrength
            End Get
            Set(ByVal value As String())
                _sStrength = value
            End Set
        End Property

        Public ReadOnly Property GetStrengths() As String
            Get
                Dim sRes As String = ""
                For i As Integer = _sStrength.GetLowerBound(0) To _sStrength.GetUpperBound(0)
                    sRes += CStr(IIf(sRes <> "", ", ", "")) + String.Format("""{0}""", JS_SafeString(_sStrength(i)))
                Next
                Return sRes
            End Get
        End Property

        ' D0283 ===
        Public Property StrongStrength() As Integer
            Get
                Return _tStrong
            End Get
            Set(ByVal value As Integer)
                _tStrong = value
            End Set
        End Property
        ' D0283 ==

        Public Property MinimalLength() As Integer
            Get
                Return _minLength
            End Get
            Set(ByVal value As Integer)
                _minLength = value
            End Set
        End Property

        Public Property CoeffLength() As Double
            Get
                Return _coefLength
            End Get
            Set(ByVal value As Double)
                _coefLength = value
            End Set
        End Property

        Public Property CoeffAlphabetical() As Double
            Get
                Return _coefAlphabetical
            End Get
            Set(ByVal value As Double)
                _coefAlphabetical = value
            End Set
        End Property

        Public Property CoeffCase() As Double
            Get
                Return _coefCase
            End Get
            Set(ByVal value As Double)
                _coefCase = value
            End Set
        End Property

        Public Property CoeffNumerical() As Double
            Get
                Return _coefNumerical
            End Get
            Set(ByVal value As Double)
                _coefNumerical = value
            End Set
        End Property

        Public Property CoeffSymbols() As Double
            Get
                Return _coefSymbols
            End Get
            Set(ByVal value As Double)
                _coefSymbols = value
            End Set
        End Property

        Public Property CreateJScriptCode() As Boolean
            Get
                Return _useJScriptCode
            End Get
            Set(ByVal value As Boolean)
                _useJScriptCode = value
            End Set
        End Property

        ' D0281 ===
        Public Property ShowOnEmptyValue() As Boolean
            Get
                Return _ShowOnEmpty
            End Get
            Set(ByVal value As Boolean)
                _ShowOnEmpty = value
            End Set
        End Property
        ' D0281 ==

        Public Sub New()
            If Page Is Nothing Then AddHandler Load, AddressOf InitComponent Else AddHandler Page.Load, AddressOf InitComponent
        End Sub

        Protected Sub InitComponent(ByVal sender As Object, ByVal e As EventArgs)
            If Not _ctrlPasswordControl Is Nothing Then
                AddChecker(_ctrlPasswordControl.Attributes, "onfocus")  ' D6446
                AddChecker(_ctrlPasswordControl.Attributes, "onblur")
                AddChecker(_ctrlPasswordControl.Attributes, "onkeyup")
            End If
        End Sub

        Private Sub AddChecker(ByRef sAttributes As AttributeCollection, ByVal sHandler As String)
            Dim sEvent As String = sAttributes(sHandler)
            If sEvent <> "" Then
                sAttributes.Remove(sHandler)
                sEvent += "; "
            End If
            sAttributes.Add(sHandler, sEvent + String.Format("CP_CheckValue('{0}', '{1}');", PasswordClientID, MessageClientID))    ' D0281
        End Sub

        Public Sub Init4ComparionCore(ByVal tCanvasPage As clsComparionCorePage)    ' D0471
            If Not tCanvasPage Is Nothing Then
                PrefixResult = Trim(tCanvasPage.ResString("lblPasswordPrefixStrength")) + " "
                Strengths(0) = tCanvasPage.ResString("lblPasswordPoor")
                Strengths(1) = tCanvasPage.ResString("lblPasswordWeak")
                Strengths(2) = tCanvasPage.ResString("lblPasswordAverage")
                Strengths(3) = tCanvasPage.ResString("lblPasswordStrong")
                Strengths(4) = tCanvasPage.ResString("lblPasswordExcellent")
            End If
        End Sub

    End Class

End Namespace