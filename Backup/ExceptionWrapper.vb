Imports PostSharp
Imports PostSharp.Aspects
Imports System.Diagnostics
Imports ExpertChoice.Service
Imports ExpertChoice.TeamTime

<Serializable()> _
Public Class WCFTTAExceptionWrapper
    Inherits OnExceptionAspect

    Public Overrides Sub OnException(ByVal args As MethodExecutionArgs)
        Dim fbURL As String = WebConfigOption(Expertchoice.Web.WebOptions._OPT_FOGBUGZ_URL, "", True)
        Dim fbUser As String = WebConfigOption(Expertchoice.Web.WebOptions._OPT_FOGBUGZ_USERNAME, "", True)
        Dim fbProject As String = WebConfigOption(Expertchoice.Web.WebOptions._OPT_FOGBUGZ_PROJECT, "", True)
        Dim fbArea As String = WebConfigOption(Expertchoice.Web.WebOptions._OPT_FOGBUGZ_AREA, "", True)

        Dim msg As String = String.Format("{0} had an error @ {1}: {2}" & vbCrLf & "{3}" & vbCrLf, args.Method.Name, DateTime.Now, args.Exception.Message, args.Exception.StackTrace)

        Dim Changeset As String
        Try
            Changeset = WebConfigOption(Expertchoice.Web.WebOptions._OPT_CHANGESET, "", True)
        Catch ex As Exception
            Changeset = "IDE"
        End Try
        msg += String.Format("Changeset: {0}" & vbCrLf, Changeset)
        Dim RTETitle As String = GetCaseHeader(args.Exception)

        'TODO Redo submit cases on RTE
        'SendToFogbugz(RTETitle, msg, fbURL, fbUser, fbProject, fbArea, ExpertChoice.Web.WebOptions.SystemEmail)
    End Sub

End Class

