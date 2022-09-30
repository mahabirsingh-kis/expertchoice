Imports System.Diagnostics
Imports System.Threading

Partial Class IncreasingBudgetsServicePage
    Inherits Page

    Public Const LongPollMaxTime As Integer = 60 ' sec.
    Public Const LongPollDBCheckTime As Integer = 500 ' millisec.

    Public Sub New()
        MyBase.New() '_PGID_EFFICIENT_FRONTIER_SERVICE)
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        Ajax_Callback(Request.Form.ToString)
    End Sub

    Private _Application As clsComparionCore
    Public ReadOnly Property App() As clsComparionCore
        Get
            If _Application Is Nothing Then
                _Application = New clsComparionCore
                DebugInfo("Application object was created from Efficient Frontier 2 polling service (HTML)")
                LoadComparionCoreOptions(_Application)
                _Application.CheckCanvasMasterDBDefaults()
            End If
            Return _Application
        End Get
    End Property

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = GetParam(args, "action")
        Dim tResult As String = ""

        Select Case sAction
            Case "poll"
                Dim tPollingUserID As Integer = CInt(GetParam(args, "polling_user_id"))
                Dim tProjectID As Integer = CInt(GetParam(args, "prjid"))
                If tProjectID >= 0 Then
                    Dim N As Integer = CInt(LongPollMaxTime / (LongPollDBCheckTime / 1000))
                    Dim DT As Nullable(Of DateTime) = Nothing
                    Dim fExitPoll As Boolean = False
                    For i As Integer = 0 To N - 1
                        Dim sData As List(Of String) = App.DBTeamTimeDataReadAll(tProjectID, ecExtraProperty.IncreasingBudgetsJsonData, tPollingUserID, True)
                        If sData IsNot Nothing AndAlso sData.Count > 0 Then
                            Debug.Print(String.Format("Solved {0} datapoints", sData.Count))

                            For Each sLine As String In sData
                                tResult += If(tResult = "", "", ",") + sLine
                            Next

                            tResult = String.Format("[{0}]", tResult)
                            fExitPoll = True

                            Exit For
                        Else
                            Thread.Sleep(LongPollDBCheckTime)
                        End If
                    Next
                    If Not fExitPoll Then
                        tResult = "[""poll_completed""]"
                    End If
                End If
            Case "cancel"
                Dim tPollingUserID As Integer = CInt(GetParam(args, "polling_user_id"))
                Dim tProjectID As Integer = CInt(GetParam(args, "prjid"))
                If tProjectID >= 0 Then
                    App.DBTeamTimeDataWrite(tProjectID, -tPollingUserID, ecExtraProperty.IncreasingBudgetsJsonData, "cancelled", False)
                End If
                tResult = "[""poll_cancelled""]"
        End Select

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

End Class