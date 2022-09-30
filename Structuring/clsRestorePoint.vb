Imports Canvas
Imports ECCore

Namespace ExpertChoice.Structuring
    ''' <summary>
    ''' Data for Revert feature
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable> Public Class clsRestorePoint
        Private _Dashboard As List(Of clsVisualNode)
        Public Property DashBoardNodes() As List(Of clsVisualNode)
            Get
                Return _Dashboard
            End Get
            Set(ByVal value As List(Of clsVisualNode))
                _Dashboard = value
            End Set
        End Property

        Private _Hierarchy As List(Of clsNode)
        Public Property Hierarchy() As List(Of clsNode)
            Get
                Return _Hierarchy
            End Get
            Set(ByVal value As List(Of clsNode))
                _Hierarchy = value
            End Set
        End Property

        Private _AltsHierarchy As List(Of clsNode)
        Public Property AltsHierarchy() As List(Of clsNode)
            Get
                Return _AltsHierarchy
            End Get
            Set(ByVal value As List(Of clsNode))
                _AltsHierarchy = value
            End Set
        End Property

        Private _Recycle As List(Of clsVisualNode)
        Public Property Recycle() As List(Of clsVisualNode)
            Get
                Return _Recycle
            End Get
            Set(ByVal value As List(Of clsVisualNode))
                _Recycle = value
            End Set
        End Property

    End Class

End Namespace