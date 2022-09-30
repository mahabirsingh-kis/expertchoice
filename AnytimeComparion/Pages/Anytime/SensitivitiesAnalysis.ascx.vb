Imports Canvas
Imports ECCore
Imports ExpertChoice.Data
Imports ExpertChoice.Web

Public Class SensitivitiesAnalysis
    Inherits System.Web.UI.UserControl

    'Public Const ACTION_DSA_UPDATE_VALUES As String = "dsa_update_values"
    'Public Const ACTION_DSA_RESET As String = "dsa_reset"
    'Public msgNoEvaluationData As String = "no data for {0}"
    'Public msgNoGroupData As String = "no group data"
    Public msgSeeingCombined As String = "combined"
    Public msgSeeingIndividual As String = "individual"
    Public msgSeeingUser As String = "user"
    'Public msgHint As String = "drag bars"
    'Public lblNormalization As String = "Normaization: "
    'Public lblSeeing As String = ""
    'Public lblMessage As String = ""
    'Public lblSelectNode As String = "Select: "
    'Public lblRefreshCaption As String = "Refresh"
    'Public lblKeepSortedAlts As String = "Freeze order of alternatives (?)"
    'Public lblShowLines As String = "Show lines"
    'Public lblLineUp As String = "Align Labels"
    'Public lblShowLegend As String = "Show Legend"
    'Public pnlLoadingID As String = ""
    Public ProjectManager As clsProjectManager = Nothing
    'Public NormalizationsList As Dictionary(Of AlternativeNormalizationOptions, String) = New Dictionary(Of AlternativeNormalizationOptions, String)()
    Public Opt_ShowMaxAltsCount As Integer = -1
    'Public Opt_ShowYouAreSeeing As Boolean = True
    'Public Opt_isMobile As Boolean = False
    Private _Current_UserID As Integer = Integer.MinValue
    Private _SA_UserID As Integer = Integer.MinValue
    'Private _SA_Data As clsSensitivityAnalysisActionData = Nothing
    'Private _NormalizationMode As AlternativeNormalizationOptions = AlternativeNormalizationOptions.anoPercentOfMax
    Private _NodesList As Dictionary(Of Integer, String) = Nothing
    'Public Const _SESS_SA_NORMALIZATION As String = "SANormMode"
    'Public Const _SESS_SA_WRT_NODE As String = "SAWrtNode"
    'Public Const _OPT_IGNORE_CATEGORIES As Boolean = True
    Public ObjPriorities As Dictionary(Of Integer, Double) = New Dictionary(Of Integer, Double)()
    Public AltValues As Dictionary(Of Integer, Double) = New Dictionary(Of Integer, Double)()
    Public AltValuesInOne As Dictionary(Of Integer, Dictionary(Of Integer, Double)) = New Dictionary(Of Integer, Dictionary(Of Integer, Double))()
    Public AltValuesInZero As Dictionary(Of Integer, Dictionary(Of Integer, Double)) = New Dictionary(Of Integer, Dictionary(Of Integer, Double))()

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    'get details for sensitive analysis
    Public Shared Function GetDetails(ByVal ActionData As Object, ByVal App As clsComparionCore) As SensitivityAnalysisModel
        Dim model As SensitivityAnalysisModel = New SensitivityAnalysisModel()

        model.StepTask = ""
        Dim sensitivities As clsSensitivityAnalysisActionData = CType(ActionData, clsSensitivityAnalysisActionData)
        Dim sensitivitiesAnalysis As SensitivitiesAnalysis = New SensitivitiesAnalysis()
        sensitivitiesAnalysis.clearData()
        sensitivitiesAnalysis.CurrentUserID = App.ActiveProject.ProjectManager.UserID

        If App.Options.BackDoor = _BACKDOOR_PLACESRATED Then
            sensitivitiesAnalysis.Opt_ShowMaxAltsCount = 10
            sensitivitiesAnalysis.SAUserID = App.ActiveProject.ProjectManager.UserID
        Else
            sensitivitiesAnalysis.SAUserID = (If(App.ActiveProject.PipeParameters.CalculateSAForCombined, COMBINED_USER_ID, App.ActiveProject.ProjectManager.UserID))
        End If

        model.saType = sensitivities.SAType.ToString()

        Select Case sensitivities.SAType
            Case SAType.satDynamic
                model.StepTask = TeamTimeClass.ResString("lblEvaluationDynamicSA")
                model.qh_help_id = ecEvaluationStepType.DynamicSA
            Case SAType.satGradient
                model.StepTask = TeamTimeClass.ResString("lblEvaluationGradientSA")
                model.qh_help_id = ecEvaluationStepType.GradientSA
            Case SAType.satPerformance
                model.StepTask = TeamTimeClass.ResString("lblEvaluationPerformanceSA")
                model.qh_help_id = ecEvaluationStepType.PerformanceSA
        End Select

        Dim sSeeing As String = ""
        Dim SAUserID = sensitivitiesAnalysis.SAUserID
        Dim ProjectManager = sensitivitiesAnalysis.ProjectManager

        If sensitivitiesAnalysis.SAUserID = COMBINED_USER_ID Then
            sSeeing = sensitivitiesAnalysis.msgSeeingCombined
        ElseIf SAUserID = sensitivitiesAnalysis.CurrentUserID Then
            sSeeing = sensitivitiesAnalysis.msgSeeingIndividual
        Else

            If SAUserID <> Integer.MinValue AndAlso ProjectManager.User IsNot Nothing Then
                Dim sUserEmail As String = ""
                Dim tUser As clsUser = ProjectManager.GetUserByID(SAUserID)
                If tUser IsNot Nothing Then sUserEmail = tUser.UserEMail
                sSeeing = String.Format(sensitivitiesAnalysis.msgSeeingUser, sUserEmail)
            End If
        End If

        model.StepTask += If(sSeeing <> "", " " & sSeeing, "")

        Return model
    End Function

    'cleaer object and property data
    Public Sub clearData()
        ObjPriorities.Clear()
        AltValues.Clear()
        ProjectManager = Nothing
        AltValuesInZero.Clear()
        AltValuesInOne.Clear()
        SAUserID = Integer.MinValue
        CurrentUserID = Integer.MinValue
        _NodesList = Nothing
    End Sub

    Public Property CurrentUserID As Integer
        Get
            If _Current_UserID = Integer.MinValue AndAlso ProjectManager IsNot Nothing Then
                _Current_UserID = ProjectManager.UserID
            End If
            Return _Current_UserID
        End Get
        Set(ByVal value As Integer)
            _Current_UserID = value
        End Set
    End Property

    Public Property SAUserID As Integer
        Get
            If _SA_UserID = Integer.MinValue AndAlso ProjectManager IsNot Nothing Then
                _SA_UserID = ProjectManager.UserID
            End If
            Return _SA_UserID
        End Get
        Set(ByVal value As Integer)
            _SA_UserID = value
        End Set
    End Property

End Class