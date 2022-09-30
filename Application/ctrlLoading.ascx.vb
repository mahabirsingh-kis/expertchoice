Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlLoadingPanel

        Inherits UserControl

        Private _Caption As String = ""     ' D0090
        Private _Message As String = ""     ' D0189
        Private _Width As Integer = 200     ' D0189
        Private _isWarning As Boolean = False       ' D0189
        Private _warningTimeout As Integer = 5000   ' D0189
        Private _warningCloseButton As Boolean = True   ' D0189
        Private _warningShowOnLoad As Boolean = False   ' D0189
        Private _Closecaption As String = "Close"   ' D0189

        ' D0031 ===
        Public Property Message() As String
            Get
                Return _Message
            End Get
            Set(ByVal value As String)
                _Message = value
            End Set
        End Property
        ' D0031 ==

        ' D0189 ===
        Public Property Width As Integer
            Get
                Return _Width
            End Get
            Set(ByVal value As Integer)
                _Width = value
            End Set
        End Property

        Public Property isWarning() As Boolean
            Get
                Return _isWarning
            End Get
            Set(ByVal value As Boolean)
                _isWarning = value
            End Set
        End Property

        Public Property WarningDelay() As Integer
            Get
                Return _warningTimeout
            End Get
            Set(ByVal value As Integer)
                _warningTimeout = value
            End Set
        End Property

        Public Property WarningShowOnLoad() As Boolean
            Get
                Return _warningShowOnLoad
            End Get
            Set(ByVal value As Boolean)
                _warningShowOnLoad = value
            End Set
        End Property

        Public Property WarningShowCloseButton() As Boolean
            Get
                Return _warningCloseButton
            End Get
            Set(ByVal value As Boolean)
                _warningCloseButton = value
            End Set
        End Property

        Public Property CloseCaption() As String
            Get
                Return _Closecaption
            End Get
            Set(ByVal value As String)
                _Closecaption = value
            End Set
        End Property
        ' D0189 ==

        ' D0090 ===
        Public Property Caption() As String
            Get
                Return _Caption
            End Get
            Set(ByVal value As String)
                _Caption = value
            End Set
        End Property

        Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As EventArgs)
            ' D0150 ===
            If Not Page Is Nothing Then
                If TypeOf (Page) Is clsComparionCorePage Then   ' D0468
                    Dim PG As clsComparionCorePage = CType(Page, clsComparionCorePage)  ' D0468
                    If _Caption = "" Then _Caption = PG.ResString(CStr(IIf(isWarning, "lblWarning", "msgLoading"))) ' D0189
                    If _Message = "" And Not isWarning Then _Message = PG.ResString("lblPleaseWait") ' D0189
                End If
            End If
            ' D0150 ==
            ASPxRoundPanelLoading.Width = Width ' D0189
            If isWarning Then
                imgLoading.Visible = False ' D0189
                If WarningShowOnLoad And Not Page Is Nothing Then Page.ClientScript.RegisterStartupScript(GetType(String), String.Format("ShowWarning{0}", ClientID), String.Format("setTimeout(""SwitchWarning('{0}', 0);"", {1}); ", ClientID, WarningDelay), True) ' D0190
                If WarningShowCloseButton Then
                    btnClose.Visible = True
                    btnClose.Text = CloseCaption
                    btnClose.Focus()    ' D0191
                    btnClose.OnClientClick = String.Format("SwitchWarning('{0}', 0); return false;", ClientID)
                End If
            End If
            ASPxRoundPanelLoading.HeaderText = Caption
        End Sub
        ' D0090 ==

    End Class

End Namespace