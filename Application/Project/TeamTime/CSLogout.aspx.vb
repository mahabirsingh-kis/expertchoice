Imports ExpertChoice.Structuring

Partial Class CSLogoutPage
    Inherits Page

    Private _Application As clsComparionCore
    Public ReadOnly Property App() As clsComparionCore
        Get
            If _Application Is Nothing Then
                _Application = New clsComparionCore
                DebugInfo("Application object was created from CS logout function (HTML)")
                LoadComparionCoreOptions(_Application)
                _Application.CheckCanvasMasterDBDefaults()
            End If
            Return _Application
        End Get
    End Property

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        Dim httpargs As NameValueCollection = Request.Form
        Dim sAction As String = GetParam(httpargs, "action")
        Dim CmdOwner As Integer = CInt(GetParam(httpargs, "cmdowner"))
        Dim MeetingOwner As Integer = CInt(GetParam(httpargs, "meetingowner"))
        Dim PRJ_ID As Integer = CInt(GetParam(httpargs, "prj_id"))
        Select Case sAction
            Case CInt(Command.DisconnectUser).ToString
                Dim args As New AntiguaConnectOperationEventArgs
                args.CmdCode = Command.DisconnectUser
                args.CmdOwner = CmdOwner
                Dim fSaveParams As Boolean = False
                If CmdOwner = MeetingOwner Then
                     SetMeetingState(MeetingState.OwnerDisconnected, CmdOwner, PRJ_ID)
                End If
                DBAppendAction(args, PRJ_ID)
        End Select
    End Sub

    Public Sub SetMeetingState(Value As MeetingState, CmdOwner As Integer, PRJ_ID As Integer)
        Dim args As New AntiguaStateOperationEventArgs
        args.CmdCode = Command.SetMeetingState
        args.CmdOwner = CmdOwner
        args.State = Value
        DBAppendAction(args, PRJ_ID)
    End Sub

    Private Sub DBAppendAction(args As AntiguaOperationEventArgs, PRJ_ID As Integer)
        If PRJ_ID >= 0 Then
            Dim DT = DateTime.Now.Ticks
            Dim sData = String.Format("{0}{1}{2}{1}{3}", DT, vbTab, If(args.isAnonymousAction, 1, 0), args.GetJSON())
            App.DBTeamTimeDataWrite(PRJ_ID, COMBINED_USER_ID, ecExtraProperty.StructuringJsonData, sData, False)
        End If
    End Sub

End Class