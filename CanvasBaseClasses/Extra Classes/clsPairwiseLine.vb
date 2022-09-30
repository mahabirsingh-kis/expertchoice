Imports ECCore

Namespace ExpertChoice.Data

    Public Class clsPairwiseLine

        Private _ID As Integer = -1
        Private _Advantage As Integer = -1
        Private _Value As Double = 0
        Private _isUndefined As Boolean = True
        Private _LeftNode As String = ""
        Private _NodeID_Left As Integer = -1 ' D3041
        Private _LeftNodeComment As String = "" ' D2989
        Private _InfodocLeft As String = ""
        Private _InfodocLeftURL As String = ""
        Private _InfodocLeftEditURL As String = ""
        Private _InfodocLeftWRT As String = ""
        Private _InfodocLeftWRTURL As String = ""
        Private _InfodocLeftWRTEditURL As String = ""
        Private _RightNode As String = ""
        Private _NodeID_Right As Integer = -1 ' D3041
        Private _RightNodeComment As String = "" ' D2989
        Private _InfodocRight As String = ""
        Private _InfodocRightURL As String = ""
        Private _InfodocRightEditURL As String = ""
        Private _InfodocRightWRT As String = ""
        Private _InfodocRightWRTURL As String = ""
        Private _InfodocRightWRTEditURL As String = ""
        Private _Comment As String = ""
        Public Property ScenarioNameLeft As String = ""     ' D6822
        Public Property ScenarioNameRight As String = ""    ' D6822

        Public Property KnownLikelihoodA As Double = -1      ' D2738
        Public Property KnownLikelihoodB As Double = -1      ' D2738

        Public Property ID() As Integer
            Get
                Return _ID
            End Get
            Set(ByVal value As Integer)
                _ID = value
            End Set
        End Property

        ' D3041 ===
        Public Property NodeID_Left() As Integer
            Get
                Return _NodeID_Left
            End Get
            Set(ByVal value As Integer)
                _NodeID_Left = value
            End Set
        End Property

        Public Property NodeID_Right() As Integer
            Get
                Return _NodeID_Right
            End Get
            Set(ByVal value As Integer)
                _NodeID_Right = value
            End Set
        End Property
        ' D3041 ==

        Public Property Advantage() As Integer
            Get
                Return _Advantage
            End Get
            Set(ByVal value As Integer)
                _Advantage = value
            End Set
        End Property

        Public Property Value() As Double
            Get
                If isUndefined Then Return CDbl(TeamTimeFuncs.UndefinedValue) Else Return _Value ' D2333
            End Get
            Set(ByVal value As Double)
                _Value = value
            End Set
        End Property

        Public Property isUndefined() As Boolean
            Get
                Return _isUndefined
            End Get
            Set(ByVal value As Boolean)
                _isUndefined = value
            End Set
        End Property

        Public Property LeftNode() As String
            Get
                Return _LeftNode
            End Get
            Set(ByVal value As String)
                _LeftNode = value
            End Set
        End Property

        ' D2989 ===
        Public Property LeftNodeComment() As String
            Get
                Return _LeftNodeComment
            End Get
            Set(ByVal value As String)
                _LeftNodeComment = value
            End Set
        End Property
        ' D2989 ==

        Public Property InfodocLeft() As String
            Get
                Return _InfodocLeft
            End Get
            Set(ByVal value As String)
                _InfodocLeft = value
            End Set
        End Property

        Public Property InfodocLeftURL() As String
            Get
                Return _InfodocLeftURL
            End Get
            Set(ByVal value As String)
                _InfodocLeftURL = value
            End Set
        End Property

        Public Property InfodocLeftEditURL() As String
            Get
                Return _InfodocLeftEditURL
            End Get
            Set(ByVal value As String)
                _InfodocLeftEditURL = value
            End Set
        End Property

        Public Property InfodocLeftWRT() As String
            Get
                Return _InfodocLeftWRT
            End Get
            Set(ByVal value As String)
                _InfodocLeftWRT = value
            End Set
        End Property

        Public Property InfodocLeftWRTURL() As String
            Get
                Return _InfodocLeftWRTURL
            End Get
            Set(ByVal value As String)
                _InfodocLeftWRTURL = value
            End Set
        End Property

        Public Property InfodocLeftWRTEditURL() As String
            Get
                Return _InfodocLeftWRTEditURL
            End Get
            Set(ByVal value As String)
                _InfodocLeftWRTEditURL = value
            End Set
        End Property

        Public Property RightNode() As String
            Get
                Return _RightNode
            End Get
            Set(ByVal value As String)
                _RightNode = value
            End Set
        End Property

        ' D2989 ===
        Public Property RightNodeComment() As String
            Get
                Return _RightNodeComment
            End Get
            Set(ByVal value As String)
                _RightNodeComment = value
            End Set
        End Property
        ' D2989 ==

        Public Property InfodocRight() As String
            Get
                Return _InfodocRight
            End Get
            Set(ByVal value As String)
                _InfodocRight = value
            End Set
        End Property

        Public Property InfodocRightURL() As String
            Get
                Return _InfodocRightURL
            End Get
            Set(ByVal value As String)
                _InfodocRightURL = value
            End Set
        End Property

        Public Property InfodocRightEditURL() As String
            Get
                Return _InfodocRightEditURL
            End Get
            Set(ByVal value As String)
                _InfodocRightEditURL = value
            End Set
        End Property

        Public Property InfodocRightWRT() As String
            Get
                Return _InfodocRightWRT
            End Get
            Set(ByVal value As String)
                _InfodocRightWRT = value
            End Set
        End Property

        Public Property InfodocRightWRTURL() As String
            Get
                Return _InfodocRightWRTURL
            End Get
            Set(ByVal value As String)
                _InfodocRightWRTURL = value
            End Set
        End Property

        Public Property InfodocRightWRTEditURL() As String
            Get
                Return _InfodocRightWRTEditURL
            End Get
            Set(ByVal value As String)
                _InfodocRightWRTEditURL = value
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

        Public Sub New(ByVal mID As Integer, tNodeID_Left As Integer, tNodeID_Right As Integer, ByVal sLeftNode As String, ByVal sRightNode As String, ByVal fIsUndefined As Boolean, ByVal tAdvantage As Integer, ByVal tValue As Double, ByVal sComment As String, Optional LikelihoodA As Double = -1, Optional LikelihoodB As Double = -1, Optional sScenarioLeft As String = "", Optional sScenarioRight As String = "") ' D2738 + D3041 + D6822
            ID = mID
            NodeID_Left = tNodeID_Left ' D3041
            NodeID_Right = tNodeID_Right    ' D3041
            LeftNode = sLeftNode
            RightNode = sRightNode
            Advantage = tAdvantage
            Value = tValue
            isUndefined = fIsUndefined
            Comment = sComment
            KnownLikelihoodA = LikelihoodA  ' D2738
            KnownLikelihoodB = LikelihoodB  ' D2738
            ScenarioNameLeft = sScenarioLeft    ' D6822
            ScenarioNameRight = sScenarioRight  ' D6822
        End Sub

    End Class

End Namespace