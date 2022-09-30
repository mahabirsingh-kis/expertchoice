
Partial Class SystemMessageEditorPage
    Inherits clsComparionCorePage

    Private _Message As String = Nothing
    Private fUpdated As Boolean = False

    Public Property Message() As String
        Get
            If _Message Is Nothing Then
                Dim tExtra As clsExtra = App.DBExtraRead(clsExtra.Params2Extra(-1, ecExtraType.Common, ecExtraProperty.Message))
                If tExtra IsNot Nothing AndAlso tExtra.Value IsNot Nothing Then _Message = CStr(tExtra.Value) Else _Message = ""
            End If
            Return _Message
        End Get
        Set(value As String)
            value = value.Trim
            If Message <> value Then
                Dim tExtra As clsExtra = clsExtra.Params2Extra(-1, ecExtraType.Common, ecExtraProperty.Message, value)
                Dim fAct As dbActionType = dbActionType.actModify
                Dim sMsg As String = ""
                If value = "" Then
                    fUpdated = App.DBExtraDelete(tExtra)
                    fAct = dbActionType.actDelete
                    sMsg = "Delete system message"
                Else
                    fUpdated = App.DBExtraWrite(tExtra)
                    fAct = dbActionType.actModify
                    sMsg = "Update system message"
                End If
                If fUpdated Then
                    _Message = value
                    App.DBSaveLog(fAct, dbObjectType.einfMessage, -1, sMsg, value)
                End If
            End If
        End Set
    End Property

    Public Sub New()
        MyBase.New(_PGID_SYSTEM_MESSAGE)
    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If Not App.isAuthorized Or Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then FetchAccess(_PGID_ERROR_403)
        AlignVerticalCenter = True
        AlignHorizontalCenter = True
        ShowNavigation = False

        If Not IsPostBack AndAlso Not IsCallback Then
            btnSave.Text = ResString("btnSave")     ' D0840
            btnExit.Text = ResString("btnReturn")   ' D0840
            btnExit.OnClientClick = String.Format("document.location.href='{0}'; return false;", PageURL(_DEF_PGID_ONPROJECTS))   ' D0840
            ClientScript.RegisterStartupScript(GetType(String), "Init", String.Format("setTimeout('theForm.{0}.focus();', 200);", tbMessage.ClientID), True)
        End If

    End Sub

    Protected Sub tbMessage_Init(sender As Object, e As EventArgs) Handles tbMessage.Init
        If Not IsPostBack Then
            tbMessage.Text = EcSanitizer.GetSafeHtmlFragment(Message) ' Anti-XSS
        End If
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Message = EcSanitizer.GetSafeHtmlFragment(tbMessage.Text) ' Anti-XSS
        lblMessage.Text = CStr(IIf(fUpdated, ResString("lblSystemMessageSaved"), "")) ' D0840
    End Sub

End Class