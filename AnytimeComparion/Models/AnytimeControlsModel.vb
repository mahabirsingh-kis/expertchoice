﻿Imports Canvas
Imports ECCore
Imports ExpertChoice.Data

Public Class LocalResultModel
    Public Property AnytimeAction As clsAction
    Public Property qh_help_id As ecEvaluationStepType
    Public Property StepNode As clsNode
    Public Property CurrentStep As Integer
    Public Property StepTask As String
    Public Property ParentNodeID As Integer
    Public Property ParentNodeGUID As Guid
    Public Property PipeParameters As Object
End Class

Public Class GlobalResultModel
    Public Property question As String
    Public Property qh_help_id As ecEvaluationStepType
    Public Property StepTask As String
    Public Property PipeParameters As Object
    Public Property ParentNodeID As Integer
End Class

Public Class SensitivityAnalysisModel
    Public Property StepTask As String
    Public Property saType As String
    Public Property qh_help_id As ecEvaluationStepType
End Class

Public Class SurveyPage
    Public Property Title As String
    Public ReadOnly Property Questions() As ArrayList
End Class

Public Class PipeParametersModel
    Public Property SurveyPage As SurveyPage
    Public Property SurveyAnswers As String()
    Public Property alternativelist As List(Of Object)()
    Public Property objectivelist As List(Of Object)()
    Public Property QuestionNumbering As List(Of Integer)()
End Class

Public Class doneOptionsModel
    Public Property logout As String
    Public Property redirect As String
    Public Property url As String
    Public Property closeTab As String
    Public Property openProject As String
    Public Property stayAtEval As String
End Class

Public Class AnytimeOutputModel
    Public Property hashLink As String
    Public Property status As String
    Public Property pipeOptions As pipeOptionsModel
    Public Property showinfodocnode As Boolean
    Public Property current_step As Integer
    Public Property previous_step As Integer
    Public Property total_pipe_steps As Integer
    Public Property is_first_time As Boolean
    Public Property show_auto_advance_modal As Boolean
    Public Property first_unassessed_step As Integer
    Public Property help_pipe_root As String
    Public Property help_pipe_url As String
    Public Property page_type As String
    Public Property pairwise_type As String
    Public Property first_node As String
    Public Property second_node As String
    Public Property parent_node As String
    Public Property first_node_info As String
    Public Property second_node_info As String
    Public Property parent_node_info As String
    Public Property wrt_first_node_info As String
    Public Property wrt_second_node_info As String
    Public Property ScaleDescriptions As List(Of Object)
    Public Property question As String
    Public Property wording As String
    Public Property nameAlternatives As String
    Public Property information_message As String
    Public Property step_task As String
    Public Property value As Double
    Public Property advantage As Double
    Public Property IsUndefined As Boolean
    Public Property sRes As String
    Public Property current_value As Double
    Public Property comment As String
    Public Property show_comments As Boolean
    Public Property ShowUnassessed As Boolean
    Public Property nextUnassessedStep As Integer()
    Public Property steps As String
    Public Property stepButtons As String
    Public Property usersComments As String
    Public Property currentUserEmail As String
    Public Property extremeMessage As Boolean
    Public Property pipeWarning As String
    Public Property sess_wrt_node_id As Integer
    Public Property parentnodeID As Integer
    Public Property orderbypriority As Boolean
    Public Property bestfit As Boolean
    Public Property dont_show As Boolean
    Public Property multi_GUIDs As List(Of String())
    Public Property multi_pw_data As List(Of clsPairwiseLine)
    Public Property multi_infodoc_params As List(Of String())
    Public Property non_pw_type As String
    Public Property precision As Integer
    Public Property showPriorityAndDirect As Boolean
    Public Property child_node As String
    Public Property intensities As List(Of String())
    Public Property non_pw_value As String
    Public Property is_direct As Boolean
    Public Property multi_non_pw_data As List(Of clsRatingLine)
    Public Property multi_intensities As List(Of String())
    Public Property step_intervals As List(Of String())
    Public Property piecewise As Boolean
    Public Property judgment_made As Integer
    Public Property overall As Double
    Public Property total_evaluation As Integer
    Public Property ParentNodeGUID As Guid
    Public Property LeftNodeGUID As Guid
    Public Property RightNodeGUID As Guid
    Public Property LeftNodeWrtGUID As Guid
    Public Property RightNodeWrtGUID As Guid
    Public Property infodoc_params As String()
    Public Property is_auto_advance As Boolean
    Public Property PipeParameters As Object
    Public Property doneOptions As doneOptionsModel
    Public Property is_infodoc_tooltip As Boolean
    Public Property defaultQhInfo As String
    Public Property qh_info As String
    Public Property qh_help_id As Integer
    Public Property qh_tnode_id As Integer
    Public Property qh_yt_info As String
    Public Property saType As String
    Public Property show_qh_automatically As Boolean
    Public Property is_qh_shown As Boolean
    Public Property dont_show_qh As Boolean
    Public Property show_qh_setting As Boolean
    Public Property multi_collapse_default As List(Of Boolean)
    Public Property cluster_phrase As String
    Public Property nodes_data As List(Of String())
    Public Property UCData As Object
    Public Property read_only As Boolean
    Public Property read_only_user As String
    Public Property collapse_bars As Boolean
    Public Property userControlContent As String
    Public Property isPM As Boolean
    Public Property cluster_nodes As List(Of String())
    Public Property has_cluster As Boolean
    Public Property name As String
    Public Property owner As String
    Public Property passcode As String
    Public Property hideBrowserWarning As Boolean
    Public Property autoFitInfoDocImages As Boolean
    Public Property autoFitInfoDocImagesOptionText As String
    Public Property framed_info_docs As Boolean
    Public Property hideInfoDocCaptions As Boolean
    Public Property fromComparion As Boolean
    Public Property nextProject As Integer
    Public Property isPipeViewOnly As Boolean
    Public Property isPipeStepFound As Boolean
End Class

Public Class pipeOptionsModel
    Public Property hideNavigation As Boolean
    Public Property disableNavigation As Boolean
    Public Property showUnassessed As Boolean
    Public Property dontAllowMissingJudgment As Boolean
End Class