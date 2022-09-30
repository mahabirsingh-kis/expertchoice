Imports ECCore

Namespace ECCore
    <Serializable> Public Class IndividualAlternatives
        Public ReadOnly Property ProjectManager As clsProjectManager

        Friend Alternatives As New Dictionary(Of Integer, List(Of clsNode))

        Public ReadOnly Property UserAlternatives(UserID As Integer) As List(Of clsNode)
            Get
                Return If(Alternatives.ContainsKey(UserID), Alternatives(UserID), New List(Of clsNode))
            End Get
        End Property

        Private Function GetNextNodeID(UserID As Integer) As Integer
            If Not Alternatives.ContainsKey(UserID) Then
                Return -1
            Else
                Return Alternatives(UserID).Select(Function(n) (n.NodeID)).DefaultIfEmpty(-1).Min - 1
            End If
        End Function

        Public Function AddAlternative(UserID As Integer, Name As String) As clsNode
            If Not Alternatives.ContainsKey(UserID) Then Alternatives.Add(UserID, New List(Of clsNode))
            Dim newAlt As New clsNode
            newAlt.NodeName = Name
            newAlt.NodeID = GetNextNodeID(UserID)
            newAlt.Hierarchy = ProjectManager.ActiveAlternatives
            Alternatives(UserID).Add(newAlt)
            Return newAlt
        End Function

        Public Function AddAlternative(UserID As Integer, Alternative As clsNode) As clsNode
            If Not Alternatives.ContainsKey(UserID) Then Alternatives.Add(UserID, New List(Of clsNode))
            Alternative.Hierarchy = ProjectManager.ActiveAlternatives
            Alternatives(UserID).Add(Alternative)
            Return Alternative
        End Function


        Public Overloads Function DeleteAlternative(UserID As Integer, ID As Guid) As Boolean
            If Not Alternatives.ContainsKey(UserID) Then Return False
            Return Alternatives(UserID).RemoveAll(Function(node) node.NodeGuidID.Equals(ID)) > 0
        End Function

        Public Overloads Function DeleteAlternative(UserID As Integer, ID As Integer) As Boolean
            If Not Alternatives.ContainsKey(UserID) Then Return False
            Return Alternatives(UserID).RemoveAll(Function(node) node.NodeID = ID) > 0
        End Function

        Public Sub ClearAlternatives(UserID As Integer)
            Alternatives.Remove(UserID)
        End Sub

        Public Sub New(ProjectManager As clsProjectManager)
            Me.ProjectManager = ProjectManager
        End Sub
    End Class

End Namespace