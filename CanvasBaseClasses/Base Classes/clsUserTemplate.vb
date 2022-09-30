Imports ExpertChoice.Service    ' D0990
Imports Canvas.PipeParameters
Imports System.IO

Namespace ExpertChoice.Data

    <Serializable()> Public Class clsUserTemplate

        Private _ID As Integer
        Private _UserID As Integer
        Private _TemplateName As String
        Private _Comment As String
        Private _TemplateType As StructureType
        Private _TemplateData As clsPipeParamaters  ' D0796
        Private _TemplateHash As String ' D2184
        'Private _XMLName As String      ' D2184
        Private _IsCustom As Boolean    ' D2184
        Private _LastModify As Nullable(Of DateTime)

        Public Property ID() As Integer
            Get
                Return _ID
            End Get
            Set(ByVal value As Integer)
                _ID = value
            End Set
        End Property

        Public Property UserID() As Integer
            Get
                Return _UserID
            End Get
            Set(ByVal value As Integer)
                _UserID = value
            End Set
        End Property

        Public Property TemplateName() As String
            Get
                Return _TemplateName
            End Get
            Set(ByVal value As String)
                _TemplateName = SubString(value.Trim, 200)    ' D0990
            End Set
        End Property

        Public Property Comment() As String
            Get
                Return _Comment
            End Get
            Set(ByVal value As String)
                _Comment = value
            End Set
        End Property

        Public Property TemplateType() As StructureType
            Get
                Return _TemplateType
            End Get
            Set(ByVal value As StructureType)
                _TemplateType = value
            End Set
        End Property

        Public Property TemplateData() As clsPipeParamaters ' D0796
            Get
                Return _TemplateData
            End Get
            Set(ByVal value As clsPipeParamaters)   ' D0796
                _TemplateData = value
            End Set
        End Property

        Public Property LastModify() As Nullable(Of DateTime)
            Get
                Return _LastModify
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _LastModify = value
            End Set
        End Property

        ' D2184 ===
        Public Property TemplateHash As String
            Get
                If String.IsNullOrEmpty(_TemplateHash) Then Return CalculateHash() Else Return _TemplateHash
            End Get
            Set(value As String)
                _TemplateHash = value
            End Set
        End Property

        Public Property IsCustom As Boolean
            Get
                Return _IsCustom
            End Get
            Set(value As Boolean)
                _IsCustom = value
            End Set
        End Property

        'Public Property XMLName As String
        '    Get
        '        Return _XMLName
        '    End Get
        '    Set(value As String)
        '        _XMLName = value
        '    End Set
        'End Property
        ' D2184 ==

        Public Overloads Function Clone() As clsUserTemplate
            Dim newUT As New clsUserTemplate
            newUT.ID = Me.ID
            newUT.UserID = Me.UserID
            newUT.TemplateName = Me.TemplateName
            newUT.Comment = Me.Comment
            newUT.TemplateType = Me.TemplateType
            newUT.TemplateData = Me.TemplateData
            newUT.TemplateHash = Me.TemplateHash    ' D2184
            'newUT.XMLName = Me.XMLName              ' D2184
            newUT.IsCustom = Me.IsCustom            ' D2184
            newUT.LastModify = Me.LastModify
            Return newUT
        End Function

        Public Sub New()
            MyBase.New()
            _ID = 0
            _UserID = -1
            _TemplateName = ""
            _Comment = ""
            _TemplateType = StructureType.stPipeOptions
            _TemplateData = Nothing
            _TemplateHash = ""  ' D2184
            '_XMLName = ""       ' D2184
            _IsCustom = False   ' D2184
            _LastModify = Nothing
        End Sub

        Public Shared Function UserTemplateByID(ByVal tID As Integer, ByVal tUserTemplatesList As List(Of clsUserTemplate)) As clsUserTemplate
            If Not tUserTemplatesList Is Nothing Then
                For Each tUT As clsUserTemplate In tUserTemplatesList
                    If tUT IsNot Nothing AndAlso tUT.ID = tID Then Return tUT
                Next
            End If
            Return Nothing
        End Function

        Public Shared Function UserTemplatesByUserID(ByVal tUserID As Integer, ByVal tUserTemplatesList As List(Of clsUserTemplate)) As List(Of clsUserTemplate)
            Dim tList As New List(Of clsUserTemplate)
            If Not tUserTemplatesList Is Nothing Then
                For Each tUT As clsUserTemplate In tUserTemplatesList
                    If tUT IsNot Nothing AndAlso tUT.UserID = tUserID Then tList.Add(tUT)
                Next
            End If
            Return tList
        End Function

        ' D2184 ===
        Public Shared Function UserTemplateByName(ByVal sName As String, ByVal tUserTemplatesList As List(Of clsUserTemplate)) As clsUserTemplate
            If sName IsNot Nothing AndAlso tUserTemplatesList IsNot Nothing Then
                sName = sName.ToLower
                For Each tUT As clsUserTemplate In tUserTemplatesList
                    If tUT IsNot Nothing AndAlso tUT.TemplateName.ToLower = sName Then Return tUT
                Next
            End If
            Return Nothing
        End Function
        ' D2184 ==

        ' D2183 ===
        Public Function CalculateHash() As String
            Dim sHash As String = ""
            If TemplateData IsNot Nothing Then
                Dim MS As New MemoryStream
                MS = TemplateData.WriteToStream()
                sHash = GetMD5(MS.ToArray)
            End If
            Return sHash
        End Function
        ' D2183 ==

    End Class

End Namespace