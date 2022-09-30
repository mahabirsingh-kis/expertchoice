Imports ECCore
Imports ECCore.ECTypes

Namespace ExpertChoice.WebAPI

    <Serializable> Public Class jNode
        Inherits clsJsonObject

        Public Property ID As Integer = -1
        Public Property NodeGUID As Guid
        Public Property ParentNodeID As Integer = -1
        Public Property ParentNodeGUID As List(Of Guid)
        'Public Property Idx As Integer = 0
        Public Property Name As String = ""
        Public Property Infodoc As String = ""      ' D5053
        Public Property isAlternative As Boolean = False
        'Public Property HierarchyID As Integer = CInt(ECHierarchyID.hidLikelihood)
        Public Property Level As Integer = 0
        Public Property Tag As Object = Nothing
        Public Property pros As List(Of AlternativeProAndCon)
        Public Property cons As List(Of AlternativeProAndCon)

        Shared Function CreateFromBaseObject(tNode As clsNode, Optional withProsAndCons As Boolean = False) As jNode
            If tNode IsNot Nothing Then
                ' D5053 ===
                Dim sInfodoc As String = tNode.Comment
                If ExpertChoice.Service.isMHT(tNode.InfoDoc) Then
                    Dim PrjID As Integer = -1
                    Dim sContent As String = ExpertChoice.Service.Infodoc_Unpack(PrjID, 0, If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), tNode.NodeID.ToString, tNode.InfoDoc, True, True, -1)
                    If Not String.IsNullOrEmpty(sContent) Then sInfodoc = ExpertChoice.Service.HTML2Text(sContent)
                Else
                    sInfodoc = tNode.InfoDoc
                End If
                Dim PM As clsProjectManager = tNode.Hierarchy.ProjectManager
                Dim retVal As jNode = New jNode With {.ID = tNode.NodeID, .NodeGUID = tNode.NodeGuidID, .ParentNodeID = tNode.ParentNodeID, .ParentNodeGUID = tNode.ParentNodesGuids,
                                          .Name = tNode.NodeName, .Infodoc = sInfodoc, .isAlternative = tNode.IsAlternative,
                                          .Level = tNode.Level, .Tag = tNode.Tag}   ' D5053     // .HierarchyID = CType(If(tNode.Hierarchy IsNot Nothing, tNode.Hierarchy.HierarchyID, -1), ECHierarchyID),
                If withProsAndCons Then
                    retVal.pros = tNode.Pros(PM)
                    retVal.cons = tNode.Cons(PM)
                End If
                Return retVal
                ' D5053 ==
            Else
                Return Nothing
            End If
        End Function

    End Class

    <Serializable> Public Class jNodeDataGrid
        Inherits jNode
        Public Property MeasureType As ECMeasureType = ECMeasureType.mtNone
        Overloads Shared Function CreateFromBaseObject(tNode As clsNode) As jNodeDataGrid
            Dim tNodeDataGrid As jNodeDataGrid = CType(clsJsonObject.doInherit(jNode.CreateFromBaseObject(tNode), GetType(jNodeDataGrid)), jNodeDataGrid)
            If tNodeDataGrid IsNot Nothing Then
                tNodeDataGrid.MeasureType = tNode.MeasureType
            End If
            Return tNodeDataGrid
        End Function
    End Class

    <Serializable> Public Class jContribution
        Inherits clsJsonObject

        Public Property ColumnGUID As Guid
        Public Property RowGUID As Guid
        Public Property Data As Object
    End Class

    <Serializable> Public Class jContributions
        Inherits clsJsonObject

        Public Property Objectives As List(Of jNode)
        Public Property Alternatives As List(Of jNode)
        Public Property Data As List(Of jContribution)
        Public Property DefaultState As jContribution

    End Class
End Namespace
