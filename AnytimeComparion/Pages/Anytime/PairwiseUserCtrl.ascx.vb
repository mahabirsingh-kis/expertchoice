Imports Canvas
Imports ECCore
Imports ExpertChoice.Data
Imports AnytimeComparion.Pages.external_classes
Imports ExpertChoice.Service
Imports System.Runtime.InteropServices

Public Class PairwiseUserCtrl
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    Public Shared Function GetPairWiseData(ByRef AnytimeAction As clsAction, ByRef App As clsComparionCore, ByRef StepNode As clsNode, <Out> ByRef Comment As String,
                                           <Out> ByRef PairwiseType As String, <Out> pipeHelpUrl As String, <Out> ByRef qh_help_id As ecEvaluationStepType, <Out> ByRef PipeWarning As String,
                                           <Out> ByRef infodoc_params As String(), <Out> ByRef question As String, <Out> ByRef wording As String,
                                 <Out> ByRef parent_node_info As String, <Out> ByRef first_node_info As String, <Out> ByRef second_node_info As String, <Out> ByRef wrt_first_node_info As String,
                                  <Out> ByRef wrt_second_node_info As String, <Out> ByRef FirstNodeName As String, <Out> ByRef SecondNodeName As String, <Out> ByRef ParentNodeName As String, <Out> ByRef PWValue As String,
                                    <Out> ByRef PWAdvantage As String, <Out> ByRef IsUndefined As String, <Out> ByRef ParentNodeGUID As Guid,
                             <Out> ByRef LeftNodeGUID As Guid, <Out> ByRef RightNodeGUID As Guid, <Out> ByRef ParentNodeID As Integer)

        qh_help_id = New PipeParameters.ecEvaluationStepType()
        infodoc_params = New String(4) {}

        Dim forceError = CBool(HttpContext.Current.Session(Constants.Sess_ForceError))
        Dim PWData = CType(AnytimeAction.ActionData, clsPairwiseMeasureData)
        Dim ParentNode = CType(App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.ParentNodeID), clsNode)
        Dim FirstNode = CType(Nothing, clsNode)
        Dim SecondNode = CType(Nothing, clsNode)
        StepNode = ParentNode
        Comment = PWData.Comment
        PairwiseType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(ParentNode).ToString()
        If (PairwiseType = "ptVerbal") Then
            pipeHelpUrl = TeamTimeClass.ResString("help_pipe_singlePairwiseVerbal")
            qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.VerbalPW
            PipeWarning = If(HttpContext.Current.Request.Cookies(Constants.Cook_Extreme) Is Nothing, TeamTimeClass.ResString("msgPWExtreme"), "")
        Else
            pipeHelpUrl = TeamTimeClass.ResString("help_pipe_singlePairwiseGraphical")
            qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.GraphicalPW
        End If

        Dim NodeType = App.ActiveProject.HierarchyObjectives.TerminalNodes()
        Dim index As Integer = NodeType.FindIndex(Function(item) item.NodeName = ParentNode.NodeName)

        If index >= 0 Then
            question = "alternatives"
            wording = App.ActiveProject.PipeParameters.JudgementAltsPromt
        End If
        If ParentNode.IsTerminalNode Then
            question = App.ActiveProject.PipeParameters.NameAlternatives
            wording = App.ActiveProject.PipeParameters.JudgementAltsPromt
            FirstNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(PWData.FirstNodeID)
            SecondNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(PWData.SecondNodeID)
        Else
            question = App.ActiveProject.PipeParameters.NameObjectives
            wording = App.ActiveProject.PipeParameters.JudgementPromt
            FirstNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.FirstNodeID)
            SecondNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.SecondNodeID)
        End If
        Dim StepTask As String = ""

        Try
            StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not FirstNode.IsTerminalNode AndAlso Not SecondNode.IsTerminalNode)
        Catch
            StepTask = ""
        End Try
        parent_node_info = Convert.ToString(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, True, True, -1))
        first_node_info = Convert.ToString(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(FirstNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), FirstNode.NodeID.ToString(), FirstNode.InfoDoc, True, True, -1))
        second_node_info = Convert.ToString(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(SecondNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), SecondNode.NodeID.ToString(), SecondNode.InfoDoc, True, True, -1))
        wrt_first_node_info = Convert.ToString(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, FirstNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(FirstNode.NodeGuidID, ParentNode.NodeGuidID), True, True, ParentNode.NodeID))
        wrt_second_node_info = Convert.ToString(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, SecondNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(SecondNode.NodeGuidID, ParentNode.NodeGuidID), True, True, ParentNode.NodeID))

        FirstNodeName = FirstNode.NodeName
        SecondNodeName = SecondNode.NodeName
        ParentNodeName = ParentNode.NodeName
        PWValue = PWData.Value
        PWAdvantage = PWData.Advantage
        IsUndefined = PWData.IsUndefined
        ParentNodeGUID = ParentNode.NodeGuidID
        LeftNodeGUID = FirstNode.NodeGuidID
        RightNodeGUID = SecondNode.NodeGuidID

        infodoc_params(0) = GeckoClass.GetInfodocParams(ParentNode.NodeGuidID, Guid.Empty)
        infodoc_params(1) = GeckoClass.GetInfodocParams(FirstNode.NodeGuidID, SecondNode.NodeGuidID)
        infodoc_params(2) = GeckoClass.GetInfodocParams(SecondNode.NodeGuidID, FirstNode.NodeGuidID)
        infodoc_params(3) = GeckoClass.GetInfodocParams(FirstNode.NodeGuidID, ParentNode.NodeGuidID)
        infodoc_params(4) = GeckoClass.GetInfodocParams(SecondNode.NodeGuidID, ParentNode.NodeGuidID)
        Dim default_params As String = "c=-1&w=200&h=200"

        If infodoc_params(0) = "" Then
            infodoc_params(0) = default_params
        End If

        If infodoc_params(1) = "" Then
            infodoc_params(1) = GeckoClass.getCommonParams(infodoc_params(2))
        End If
        If infodoc_params(2) = "" Then
            infodoc_params(2) = GeckoClass.getCommonParams(infodoc_params(1))
        End If

        If infodoc_params(2) = "" OrElse infodoc_params(1) = "" Then
            GeckoClass.SetInfodocParams(FirstNode.NodeGuidID, SecondNode.NodeGuidID, default_params)
            GeckoClass.SetInfodocParams(SecondNode.NodeGuidID, FirstNode.NodeGuidID, default_params)
            infodoc_params(1) = default_params
            infodoc_params(2) = default_params
        End If

        If infodoc_params(3) = "" Then
            infodoc_params(3) = GeckoClass.getCommonParams(infodoc_params(4))
        End If

        If infodoc_params(4) = "" Then
            infodoc_params(4) = GeckoClass.getCommonParams(infodoc_params(3))
        End If

        If infodoc_params(3) = "" OrElse infodoc_params(4) = "" Then
            GeckoClass.SetInfodocParams(FirstNode.NodeGuidID, ParentNode.NodeGuidID, default_params)
            GeckoClass.SetInfodocParams(SecondNode.NodeGuidID, ParentNode.NodeGuidID, default_params)
            infodoc_params(3) = default_params
            infodoc_params(4) = default_params
        End If

        Dim debug_test = infodoc_params
        ParentNodeID = PWData.ParentNodeID
    End Function
End Class