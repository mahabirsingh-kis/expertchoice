Imports ECCore  ' D1008

Namespace ExpertChoice.Data

    Public Class clsRatingLine

        Private _ID As Integer
        Private _Idx As String = ""             ' D4013
        Private _sGUID As String                ' D2251
        Private _Title As String
        Private _Ratings As List(Of clsRating)  ' D1008
        Private _RatingID As Integer
        Private _RatingsComment As String       ' D2420
        Private _DirectData As Single   ' D0678
        Private _Infodoc As String              ' D0108
        Private _InfodocURL As String           ' D1064
        Private _InfodocEditURL As String       ' D1007
        Private _InfodocWRT As String           ' D1007
        Private _InfodocWRTURL As String        ' D1064
        Private _InfodocWRTEditURL As String    ' D1007
        Private _Comment As String              ' D0647
        Private _OriginalNode As clsNode = Nothing  ' D4106
        Public Property ScenarioName As String = "" ' D6822

        Public Property ID() As Integer
            Get
                Return _ID
            End Get
            Set(ByVal value As Integer)
                _ID = value
            End Set
        End Property

        ' D4013 ===
        Public Property Idx As String
            Get
                Return _Idx
            End Get
            Set(value As String)
                _Idx = value
            End Set
        End Property
        ' D4013 ==

        ' D2251 ===
        Public Property sGUID As String
            Get
                Return _sGUID
            End Get
            Set(value As String)
                _sGUID = value
            End Set
        End Property
        ' D2251 ==

        Public Property Title() As String
            Get
                Return _Title
            End Get
            Set(ByVal value As String)
                _Title = value
            End Set
        End Property

        Public Property RatingID() As Integer
            Get
                Return _RatingID
            End Get
            Set(ByVal value As Integer)
                _RatingID = value
            End Set
        End Property

        ' D2420 ===
        Public Property RatingsComment As String
            Get
                Return _RatingsComment
            End Get
            Set(value As String)
                _RatingsComment = value
            End Set
        End Property
        'D2420 ==

        Public Property Ratings() As List(Of clsRating) ' D1008
            Get
                Return _Ratings
            End Get
            Set(ByVal value As List(Of clsRating))  ' D1008
                _Ratings = value
            End Set
        End Property

        ' D0678 ===
        Public Property DirectData() As Single
            Get
                Return _DirectData
            End Get
            Set(ByVal value As Single)
                _DirectData = value
            End Set
        End Property
        ' D0678 ==

        ' D0108 ===
        Public Property Infodoc() As String
            Get
                Return _Infodoc
            End Get
            Set(ByVal value As String)
                _Infodoc = value
            End Set
        End Property
        ' D0108 ==

        ' D1064 ===
        Public Property InfodocURL() As String
            Get
                Return _InfodocURL
            End Get
            Set(ByVal value As String)
                _InfodocURL = value
            End Set
        End Property
        ' D1064 ==

        ' D1007 ===
        Public Property InfodocEditURL() As String
            Get
                Return _InfodocEditURL
            End Get
            Set(ByVal value As String)
                _InfodocEditURL = value
            End Set
        End Property

        Public Property InfodocWRT() As String
            Get
                Return _InfodocWRT
            End Get
            Set(ByVal value As String)
                _InfodocWRT = value
            End Set
        End Property

        ' D1064 ===
        Public Property InfodocWRTURL() As String
            Get
                Return _InfodocWRTURL
            End Get
            Set(ByVal value As String)
                _InfodocWRTURL = value
            End Set
        End Property
        ' D1064 ==

        Public Property InfodocWRTEditURL() As String
            Get
                Return _InfodocWRTEditURL
            End Get
            Set(ByVal value As String)
                _InfodocWRTEditURL = value
            End Set
        End Property
        ' D1007 ==

        ' D0647 ===
        Public Property Comment() As String
            Get
                Return _Comment
            End Get
            Set(ByVal value As String)
                _Comment = value
            End Set
        End Property
        ' D0647 ==

        ' D4106 ===
        Public Property OriginalNode As clsNode
            Get
                Return _OriginalNode
            End Get
            Set(value As clsNode)
                _OriginalNode = value
            End Set
        End Property

        ' D4106 ==

        Public Sub New(ByVal mID As Integer, mGUID As String, ByVal mTitle As String, ByVal mRatings As List(Of clsRating), ByVal mRatingID As Integer, Optional ByVal sInfoDoc As String = "", Optional ByVal sComment As String = "", Optional ByVal mDirectData As Single = 0, Optional ByVal mInfodocURL As String = "", Optional ByVal mInfodocEditURL As String = "", Optional ByVal mInfodocWRT As String = "", Optional ByVal mInfodocWRTURL As String = "", Optional ByVal mInfodocWRTEditURL As String = "", Optional sRatingsComment As String = "", Optional sIdx As String = "", Optional tNode As clsNode = Nothing, Optional sScenario As String = "")   ' D0108 + D0647 + D0678 + D1007 + D1008 + D1064 + D2251 + D2420 + D4013 + D4106 + D6822
            ID = mID
            Idx = sIdx  ' D4013
            sGUID = mGUID              ' D2251
            Title = mTitle
            Ratings = mRatings
            RatingID = mRatingID
            RatingsComment = sRatingsComment    ' D2420
            DirectData = mDirectData    ' D0678
            Infodoc = sInfoDoc          ' D0108
            InfodocURL = mInfodocURL    ' D1064
            InfodocEditURL = mInfodocEditURL    ' D1007
            InfodocWRT = mInfodocWRT            ' D1007
            InfodocWRTURL = mInfodocWRTURL  ' D1064
            InfodocWRTEditURL = mInfodocWRTEditURL ' D1007
            Comment = sComment          ' D0647
            ScenarioName = sScenario    ' D6822
            OriginalNode = tNode        ' D4106
        End Sub

    End Class

End Namespace