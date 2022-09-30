Imports System.Linq
Imports ECCore

Namespace ExpertChoice.Data

    Public Enum ecWorkspaceStatus
        wsUnknown = -2              ' D0660
        wsDisabled = -1
        wsEnabled = 0
        wsSynhronousActive = 2      ' D0192 + D0382
        wsSynhronousReadOnly = 3    ' D0382
    End Enum

    <Serializable()> Public Class clsWorkspace

        Private Const _STARRED_VALUE = 64   ' D5006 + D5089

        Private _ID As Integer
        Private _UserID As Integer
        Private _ProjectID As Integer
        Private _GroupID As Integer
        ' D1945 + D5006 ===
        Private _StatusLikelihood As ecWorkspaceStatus
        Private _StatusImpact As ecWorkspaceStatus

        Public Property isStarred() As Boolean

        ' D5006 ==
        Private _TTStatusLikelihood As ecWorkspaceStatus      ' D0660
        Private _TTStatusImpact As ecWorkspaceStatus
        ' D1945 ==
        Private _Comment As String
        Private _StepLikelihood As Integer      ' D1945
        Private _StepImpact As Integer          ' D1945
        Private _Created As Nullable(Of DateTime)   ' D0494
        Private _LastModify As Nullable(Of DateTime)    ' D0494

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

        Public Property ProjectID() As Integer
            Get
                Return _ProjectID
            End Get
            Set(ByVal value As Integer)
                _ProjectID = value
            End Set
        End Property

        Public Property GroupID() As Integer
            Get
                Return _GroupID
            End Get
            Set(ByVal value As Integer)
                _GroupID = value
            End Set
        End Property

        Public Property Status(fIsImpact As Boolean) As ecWorkspaceStatus ' D1945
            Get
                If fIsImpact Then Return StatusImpact Else Return StatusLikelihood 'D1945
            End Get
            Set(ByVal value As ecWorkspaceStatus)
                If fIsImpact Then StatusImpact = value Else StatusLikelihood = value ' D1945
            End Set
        End Property

        ' D1945 + D5006 + D5089 ===
        Public Property StatusLikelihood() As ecWorkspaceStatus
            Get
                Return _StatusLikelihood
            End Get
            Set(ByVal value As ecWorkspaceStatus)
                _StatusLikelihood = value
                If value = ecWorkspaceStatus.wsSynhronousActive OrElse value = ecWorkspaceStatus.wsSynhronousReadOnly Then _StatusImpact = ecWorkspaceStatus.wsEnabled
                If value = ecWorkspaceStatus.wsDisabled Then
                    _StatusImpact = ecWorkspaceStatus.wsDisabled
                    isStarred = False
                End If
            End Set
        End Property

        Public Property StatusImpact() As ecWorkspaceStatus
            Get
                Return _StatusImpact
            End Get
            Set(ByVal value As ecWorkspaceStatus)
                _StatusImpact = value
                If value = ecWorkspaceStatus.wsSynhronousActive OrElse value = ecWorkspaceStatus.wsSynhronousReadOnly Then _StatusLikelihood = ecWorkspaceStatus.wsEnabled
                If value = ecWorkspaceStatus.wsDisabled Then
                    _StatusLikelihood = ecWorkspaceStatus.wsDisabled
                    isStarred = False
                End If
            End Set
        End Property
        ' D1945  ==

        Public Property StatusDataLikelihood As Integer
            Get
                Return CInt(_StatusLikelihood)
            End Get
            Set(value As Integer)
                _StatusLikelihood = CType(value, ecWorkspaceStatus)
            End Set
        End Property

        Public Property StatusDataImpact As Integer
            Get
                Return CInt(_StatusImpact) Or If(isStarred, _STARRED_VALUE, 0)
            End Get
            Set(value As Integer)
                If value >= 0 Then
                    _StatusImpact = CType(value And (Not _STARRED_VALUE), ecWorkspaceStatus)
                    isStarred = (value And _STARRED_VALUE) <> 0
                Else
                    _StatusImpact = CType(value, ecWorkspaceStatus)
                End If
            End Set
        End Property
        ' D5089 ==

        ' D0660 ===
        Public Property TeamTimeStatus(fIsImpact As Boolean) As ecWorkspaceStatus ' D1945
            Get
                If fIsImpact Then Return TeamTimeStatusImpact Else Return TeamTimeStatusLikelihood ' D1945
            End Get
            Set(ByVal value As ecWorkspaceStatus)
                If fIsImpact Then TeamTimeStatusImpact = value Else TeamTimeStatusLikelihood = value ' D1945
            End Set
        End Property
        ' D0660 ==

        ' D1945 ===
        Public Property TeamTimeStatusLikelihood() As ecWorkspaceStatus
            Get
                Return _TTStatusLikelihood
            End Get
            Set(ByVal value As ecWorkspaceStatus)
                _TTStatusLikelihood = value
                If value = ecWorkspaceStatus.wsDisabled Then _TTStatusImpact = ecWorkspaceStatus.wsDisabled
                If value = ecWorkspaceStatus.wsSynhronousActive OrElse value = ecWorkspaceStatus.wsSynhronousReadOnly Then _TTStatusImpact = ecWorkspaceStatus.wsEnabled
            End Set
        End Property

        Public Property TeamTimeStatusImpact() As ecWorkspaceStatus
            Get
                Return _TTStatusImpact
            End Get
            Set(ByVal value As ecWorkspaceStatus)
                _TTStatusImpact = value
                If value = ecWorkspaceStatus.wsDisabled Then _TTStatusLikelihood = ecWorkspaceStatus.wsDisabled
                If value = ecWorkspaceStatus.wsSynhronousActive OrElse value = ecWorkspaceStatus.wsSynhronousReadOnly Then _TTStatusLikelihood = ecWorkspaceStatus.wsEnabled
            End Set
        End Property
        ' D1945 ==

        ' D0382 ===
        Public ReadOnly Property isInTeamTime(fIsImpact As Boolean) As Boolean  ' D1945
            Get
                Return TeamTimeStatus(fIsImpact) = ecWorkspaceStatus.wsSynhronousActive OrElse TeamTimeStatus(fIsImpact) = ecWorkspaceStatus.wsSynhronousReadOnly OrElse Status(fIsImpact) = ecWorkspaceStatus.wsSynhronousActive OrElse Status(fIsImpact) = ecWorkspaceStatus.wsSynhronousReadOnly   ' D3040 + D4390
            End Get
        End Property
        ' D0382 ==

        Public Property Comment() As String
            Get
                Return _Comment
            End Get
            Set(ByVal value As String)
                _Comment = value
            End Set
        End Property

        Public Property ProjectStep(fIsImpact As Boolean) As Integer  ' D1945
            Get
                If fIsImpact Then Return ProjectStepImpact Else Return ProjectStepLikelihood ' D1945
            End Get
            Set(ByVal value As Integer)
                If fIsImpact Then ProjectStepImpact = value Else ProjectStepLikelihood = value ' D1945
            End Set
        End Property

        ' D1945 ===
        Public Property ProjectStepLikelihood() As Integer
            Get
                Return _StepLikelihood
            End Get
            Set(ByVal value As Integer)
                _StepLikelihood = value
            End Set
        End Property

        Public Property ProjectStepImpact() As Integer
            Get
                Return _StepImpact
            End Get
            Set(ByVal value As Integer)
                _StepImpact = value
            End Set
        End Property
        ' D1945 ==

        ' D0494 ===
        Public Property Created() As Nullable(Of DateTime)
            Get
                Return _Created
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _Created = value
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
        ' D0494 ==

        ' D0131 ===
        Public Function Clone() As clsWorkspace
            Dim NewWS As New clsWorkspace
            NewWS.ID = Me.ID
            NewWS.UserID = Me.UserID
            NewWS.ProjectID = Me.ProjectID
            NewWS.GroupID = Me.GroupID
            ' D1945 ===
            NewWS.StatusLikelihood = Me.StatusLikelihood
            NewWS.StatusImpact = Me.StatusImpact
            NewWS.TeamTimeStatusLikelihood = Me.TeamTimeStatusLikelihood    ' D0660
            NewWS.TeamTimeStatusImpact = Me.TeamTimeStatusImpact
            NewWS.ProjectStepLikelihood = Me.ProjectStepLikelihood
            NewWS.ProjectStepImpact = Me.ProjectStepImpact
            ' D1945 ==
            NewWS.Comment = Me.Comment
            NewWS.Created = Me.Created  ' D0494
            NewWS.LastModify = Me.LastModify    ' D0494
            Return NewWS
        End Function
        ' D0131 ==

        ' D6429 ===
        Shared Function TeamTimStatusAsSyncMode(tStatus As ecWorkspaceStatus) As SynchronousEvaluationMode
            Select Case tStatus
                Case ecWorkspaceStatus.wsSynhronousActive
                    Return SynchronousEvaluationMode.semOnline
                Case ecWorkspaceStatus.wsSynhronousReadOnly
                    Return SynchronousEvaluationMode.semByFacilitatorOnly
            End Select
            Return SynchronousEvaluationMode.semNone
        End Function
        ' D6429 ==

        Public Sub New()
            _ID = -1
            _UserID = -1
            _ProjectID = -1
            _GroupID = -1
            ' D1945 ===
            _StatusLikelihood = ecWorkspaceStatus.wsEnabled
            _StatusImpact = ecWorkspaceStatus.wsEnabled
            _TTStatusLikelihood = ecWorkspaceStatus.wsUnknown ' D0660
            _TTStatusImpact = ecWorkspaceStatus.wsUnknown
            ' D1945 ==
            _Comment = ""
            _StepLikelihood = -1
            _StepImpact = -1        ' D1945
            _Created = Nothing  ' D0494
            _LastModify = Nothing   ' D0494
        End Sub

        ' D0465 ===
        Public Shared Function WorkspaceByID(ByVal tID As Integer, ByVal tWorkspacesList As List(Of clsWorkspace)) As clsWorkspace
            If Not tWorkspacesList Is Nothing Then
                For Each tWS As clsWorkspace In tWorkspacesList
                    If tWS IsNot Nothing AndAlso tWS.ID = tID Then Return tWS ' D0603
                Next
            End If
            Return Nothing
        End Function

        Public Shared Function WorkspacesByProjectID(ByVal tProjectID As Integer, ByVal tWorkspacesList As List(Of clsWorkspace)) As List(Of clsWorkspace)
            Dim tWSList As New List(Of clsWorkspace)
            If Not tWorkspacesList Is Nothing Then
                For Each tWS As clsWorkspace In tWorkspacesList
                    If tWS IsNot Nothing AndAlso tWS.ProjectID = tProjectID Then tWSList.Add(tWS) ' D0603
                Next
            End If
            Return tWSList
        End Function

        Public Shared Function WorkspacesByUserID(ByVal tUserID As Integer, ByVal tWorkspacesList As List(Of clsWorkspace)) As List(Of clsWorkspace)
            Dim tWSList As New List(Of clsWorkspace)
            If Not tWorkspacesList Is Nothing Then
                For Each tWS As clsWorkspace In tWorkspacesList
                    If tWS IsNot Nothing AndAlso tWS.UserID = tUserID Then tWSList.Add(tWS) ' D0603
                Next
            End If
            Return tWSList
        End Function

        Public Shared Function WorkspaceByUserIDAndProjectID(ByVal tUserID As Integer, ByVal tProjectID As Integer, ByVal tWorkspacesList As List(Of clsWorkspace)) As clsWorkspace
            Return tWorkspacesList?.FirstOrDefault(Function(tws) (tws IsNot Nothing AndAlso tws.ProjectID = tProjectID AndAlso tws.UserID = tUserID))
            'If Not tWorkspacesList Is Nothing Then
            '    For Each tWS As clsWorkspace In tWorkspacesList
            '        If tWS IsNot Nothing AndAlso tWS.ProjectID = tProjectID AndAlso tWS.UserID = tUserID Then Return tWS ' D0603
            '    Next
            'End If
            'Return Nothing
        End Function
        ' D0465 ==

        ' D5006 ===
        Public Shared Function WorkspacesStarred(ByVal tWorkspacesList As List(Of clsWorkspace)) As List(Of clsWorkspace)
            Dim tWSList As New List(Of clsWorkspace)
            If Not tWorkspacesList Is Nothing Then
                For Each tWS As clsWorkspace In tWorkspacesList
                    If tWS IsNot Nothing AndAlso tWS.StatusLikelihood <> ecWorkspaceStatus.wsDisabled AndAlso tWS.isStarred Then tWSList.Add(tWS)   ' D5089
                Next
            End If
            Return tWSList
        End Function
        ' D5006 ==

    End Class

    '' D3512 ===
    'Public Class clsWorkspaceComparer
    '    Implements IComparer(Of clsWorkspace)

    '    Public Function Compare(ByVal A As clsWorkspace, ByVal B As clsWorkspace) As Integer Implements IComparer(Of clsWorkspace).Compare
    '        Return CInt(IIf(A.UserID = B.UserID, 0, IIf(A.UserID < B.UserID, -1, 1)))
    '    End Function
    'End Class
    '' D3512 ==

End Namespace
