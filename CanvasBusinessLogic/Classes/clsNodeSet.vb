Imports ECCore

Namespace ExpertChoice.Data

    <Serializable> Public Enum ecNodeSetHierarchy
        hidObjectives = 0
        hidAlternatives = 1
        hidLikelihood = 2
        hidImpact = 3
        hidEvents = 4
        hidControls = 5
    End Enum

    <Serializable()> Public Class clsNodeSet

        Public Property Name As String
        Public Property Hierarchy As ecNodeSetHierarchy = ecNodeSetHierarchy.hidObjectives
        Public Property Filename As String = ""

        Private _Content As String = "" ' D5053
        Private _Nodes As List(Of clsNode) = Nothing    ' D5053
        Private _ParseContentError As String = Nothing  ' D5053

        Shared Function isRisk(HID As ecNodeSetHierarchy) As Boolean
            Return HID <> ecNodeSetHierarchy.hidObjectives AndAlso HID <> ecNodeSetHierarchy.hidAlternatives
        End Function

        Public Function isRisk() As Boolean
            Return clsNodeSet.isRisk(Hierarchy)
        End Function

        ' D5053 ===
        Public Property Content As String
            Get
                Return _Content
            End Get
            Set(value As String)
                If _Content <> value Then
                    _Content = value
                    _Nodes = Nothing
                    _ParseContentError = Nothing
                End If
            End Set
        End Property
        ' D5053 ==

        Shared Function GetByName(List As List(Of clsNodeSet), Name As String) As clsNodeSet
            If List IsNot Nothing Then
                For Each tSet As clsNodeSet In List
                    If String.Equals(tSet.Name, Name, StringComparison.CurrentCultureIgnoreCase) Then Return tSet
                Next
            End If
            Return Nothing
        End Function

        Shared Function GetByHID(List As List(Of clsNodeSet), HID As ecNodeSetHierarchy) As List(Of clsNodeSet)
            Dim tRes As New List(Of clsNodeSet)
            If List IsNot Nothing Then
                For Each tSet As clsNodeSet In List
                    If tSet.Hierarchy = HID Then tRes.Add(tSet)
                Next
            End If
            Return tRes
        End Function

        Shared Function NodeSet_GetSubPath(HID As ecNodeSetHierarchy) As String
            Dim sName As String = ""
            Select Case HID
                Case ecNodeSetHierarchy.hidObjectives
                    sName = "Objectives"
                Case ecNodeSetHierarchy.hidAlternatives
                    sName = "Alternatives"
                Case ecNodeSetHierarchy.hidLikelihood
                    sName = "Likelihood"
                Case ecNodeSetHierarchy.hidImpact
                    sName = "Impact"
                Case ecNodeSetHierarchy.hidControls
                    sName = "Controls"
                Case ecNodeSetHierarchy.hidEvents
                    sName = "Events"
            End Select
            Return sName    ' D5042
        End Function

        Shared Function NodeSet_GetSamplesPath(HID As ecNodeSetHierarchy) As String
            Return String.Format("{0}{1}\{2}\", _FILE_NODESETS_SAMPLES, If(clsNodeSet.isRisk(HID), "Riskion", "Comparion"), NodeSet_GetSubPath(HID)) ' D5042
        End Function

        Shared Function NodeSet_GetPath(HID As ecNodeSetHierarchy, WorkgroupID As Integer) As String
            Return String.Format("{0}{1}\{2}\", _FILE_NODESETS_WKG, WorkgroupID, NodeSet_GetSubPath(HID))
        End Function

        Shared Function NodeSet_Prefix(HID As ecNodeSetHierarchy) As String
            Dim sNames As String() = NodeSet_Prefixes(HID)  ' D5053
            If sNames IsNot Nothing AndAlso sNames.Length > 0 Then Return sNames(0) Else Return ""  ' D5053
        End Function

        Shared Function NodeSet_Prefixes(HID As ecNodeSetHierarchy) As String()
            Dim sNames As String() = {}
            Select Case HID
                Case ecNodeSetHierarchy.hidObjectives
                    sNames = clsTextModel._TR_HIERARCHY
                Case ecNodeSetHierarchy.hidAlternatives
                    sNames = clsTextModel._TR_ALTERNATIVES
                Case ecNodeSetHierarchy.hidLikelihood
                    sNames = clsTextModel._TR_LIKELIHOOD
                Case ecNodeSetHierarchy.hidImpact
                    sNames = clsTextModel._TR_IMPACT
                Case ecNodeSetHierarchy.hidEvents
                    sNames = clsTextModel._TR_EVENTS
                Case ecNodeSetHierarchy.hidControls
                    sNames = clsTextModel._TR_CONTROLS
            End Select
            Return sNames
        End Function


        Public Function AsNodes(Optional ByRef sError As String = Nothing) As List(Of clsNode)
            ' D5053 ===
            If _Nodes Is Nothing Then
                _Nodes = New List(Of clsNode)
                _ParseContentError = ""
                Dim tmpPM As New clsProjectManager(,, isRisk)
                Dim Cnt As Integer = 0
                Dim Lines As String() = clsTextModel.PrepareContent(Content)
                Select Case Hierarchy
                    Case ecNodeSetHierarchy.hidAlternatives, ecNodeSetHierarchy.hidEvents
                        _ParseContentError = clsTextModel.ReadAlternativesToHierarchy(Lines, NodeSet_Prefixes(Hierarchy), tmpPM.ActiveAlternatives)
                        _Nodes = tmpPM.ActiveAlternatives.Nodes
                    Case Else
                        _ParseContentError = clsTextModel.ReadNodesToHierarchy(Lines, NodeSet_Prefixes(Hierarchy), tmpPM.ActiveObjectives, Cnt)
                        _Nodes = tmpPM.ActiveObjectives.Nodes
                End Select
            End If
            Return _Nodes
            ' D5053 ==
        End Function

        Public Function Clone() As clsNodeSet
            Dim tNew As New clsNodeSet() With {
                .Name = Me.Name,
                .Hierarchy = Me.Hierarchy,
                .Content = Me.Content,
                .Filename = Me.Filename
            }
            'tNew.Attributes.AddRange(Me.Attributes)
            'tNew.Nodes.AddRange(Me.Nodes)
            Return tNew
        End Function

        Public Sub New()
        End Sub

    End Class

End Namespace
