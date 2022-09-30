Partial Class SAReductionPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_RISK_STANDALONE_REDUCTIONS_GRID)
    End Sub  

    Public Function ControlTypeToString(ctrlType As ControlType) As String
        Dim retVal As String = ""
        Select Case ctrlType
            Case ControlType.ctUndefined
                retVal = ParseString("Undefined")
            Case ControlType.ctCause
                retVal = ParseString("%%Likelihood%% Of %%Objectives(l)%%")
            Case ControlType.ctCauseToEvent
                retVal = ParseString("%%Likelihood%% Of %%Alternatives%%")
            Case ControlType.ctConsequence ' OBSOLETE
                retVal = ParseString("Consequence")
            Case ControlType.ctConsequenceToEvent
                retVal = ParseString("Consequences Of %%Alternatives%% To %%Objectives(i)%%")
        End Select
        Return retVal
    End Function

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        If App.ActiveProject Is Nothing Then FetchAccess()
    End Sub

End Class