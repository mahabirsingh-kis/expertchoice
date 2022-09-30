Partial Class DashboardPage
    Inherits clsComparionCorePage

    Public Property ViewOnly As Boolean = True ' D6910

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            If App.HasActiveProject Then Return App.ActiveProject.ProjectManager Else Return Nothing
        End Get
    End Property

    '' D6857 ===
    'Public Function OptionsList() As NameValueCollection
    '    If PM IsNot Nothing Then Return ParseJSONParams(PM.Parameters.Dashboard_ChartOptions) Else Return Nothing
    'End Function

    'Public Function GetOptionValue(sName As String, sDefValue As String) As String
    '    Dim Options As NameValueCollection = OptionsList()
    '    If Opt,GRIDions IsNot Nothing AndAlso Options(sName) IsNot Nothing Then Return Options(sName) Else Return sDefValue
    'End Function
    '' D6857 ==

    ' D6896 ===
    Public ReadOnly Property ActiveProjectHasAlternativeAttributes As Boolean
        Get
            If PM.Attributes IsNot Nothing AndAlso PM.Attributes.AttributesList IsNot Nothing Then
                For Each attr In PM.Attributes.AttributesList
                    If attr.Type = AttributeTypes.atAlternative AndAlso Not attr.IsDefault Then Return True
                Next
            End If
            Return False
        End Get
    End Property

    ''' <summary>
    ''' 1-N - show top 1-N alternatives
    ''' -1  - Show All alternatives
    ''' -2  - Advanced
    ''' -3  - Select/deselect alternatives
    ''' -4  - Filter by funded alternatives in RA scenario
    ''' -5  - Filter by alternative attributes
    ''' -6  - Show risks only
    ''' -5  - Show opportunities only
    ''' </summary>
    ''' <returns></returns>
    Public Property AlternativesFilterValue As Integer
        Get
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_ALTS_FILTER_ID, UNDEFINED_USER_ID))
            If retVal = -5 AndAlso Not ActiveProjectHasAlternativeAttributes Then retVal = -1
            'If Not IsMixedOrAltsView() AndAlso Not IsConsensusView() AndAlso retVal < -1 Then retVal = -1
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_ALTS_FILTER_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property
    ' D6896 ===

    Public Sub New()
        MyBase.New(_PGID_ANALYSIS_DASHBOARD)
    End Sub

    ' D6910 ===
    Private Sub DashboardPage_Init(sender As Object, e As EventArgs) Handles Me.Init
        ViewOnly = App.ActiveUser Is Nothing OrElse Not App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup)
    End Sub
    ' D6910 ==

End Class