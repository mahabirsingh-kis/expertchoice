Imports GemBox.Document
<Serializable> Public Class ReportGeneratorPart

    Private _PartID As Integer = 0
    Private _PartName As String = ""
    Private _PartViewOrder As Integer
    <NonSerialized> Public gbSections As List(Of Section)

    Private _PicturePath As String = "" 'AS/18226 if not empty then its a picture

    Public Sub New(partID As Integer, partName As String, partOrder As Integer)
        _PartID = partID
        _PartName = partName
        _PartViewOrder = partOrder
    End Sub

    Public Property PartID As Integer
        Get
            Return _PartID
        End Get
        Set(value As Integer)
            _PartID = value
        End Set
    End Property

    Public Property PartName As String
        Get
            Return _PartName
        End Get
        Set(value As String)
            _PartName = value
        End Set
    End Property

    Public Property PartViewOrder As Integer
        Get
            Return _PartViewOrder
        End Get
        Set(value As Integer)
            _PartViewOrder = value
        End Set
    End Property

End Class
