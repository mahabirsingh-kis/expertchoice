Imports Canvas
Imports ECCore

Namespace ExpertChoice.Structuring
    <Serializable> Public Class clsMeeting 'A0090

        Private _Info As MeetingInfo
        Public Property Info() As MeetingInfo
            Get
                Return _Info
            End Get
            Set(ByVal value As MeetingInfo)
                _Info = value
            End Set
        End Property

        Private _Sessions As New Dictionary(Of String, clsSession)
        Public Property Sessions() As Dictionary(Of String, clsSession)
            Get
                Return _Sessions
            End Get
            Set(ByVal value As Dictionary(Of String, clsSession))
                _Sessions = value
            End Set
        End Property

        Public IsLoggingEnabled As Boolean = False

        Private _RestorePoints As New List(Of clsRestorePoint)
        Public Property RestorePoints() As List(Of clsRestorePoint)
            Get
                Return _RestorePoints
            End Get
            Set(ByVal value As List(Of clsRestorePoint))
                _RestorePoints = value
            End Set
        End Property

        <NonSerialized> Private RevertTimer As System.Threading.Timer
        Private AutoSaveInterval As Integer = 1000 * 60 'TODO: Put this to config

        Public Sub New()
            'TODO
            'RevertTimer = New System.Threading.Timer(New Threading.TimerCallback(AddressOf OnRevertTimer), Nothing, 0, AutoSaveInterval)
        End Sub

        Private Sub OnRevertTimer(ByVal state As Object)
            If Info.State = MeetingState.Active Then
                DoAutoSave(False)
            End If
        End Sub

        Public Sub DoAutoSave(ByVal ManualSave As Boolean)
            ' Save
            If Info.ProjectManager IsNot Nothing Then
                Dim P As New clsRestorePoint
                P.DashBoardNodes = CloneVNodesList(Info.ProjectManager.AntiguaDashboard.Nodes)
                P.Recycle = CloneVNodesList(Info.ProjectManager.AntiguaRecycleBin.Nodes)
                P.Hierarchy = CloneNodesList(Info.ProjectManager.Hierarchy(Info.ProjectManager.ActiveHierarchy).Nodes)
                If Info.ProjectManager.AltsHierarchies.Count > 0 Then 'A0160 RTE here
                    P.AltsHierarchy = CloneNodesList(Info.ProjectManager.AltsHierarchy(Info.ProjectManager.ActiveAltsHierarchy).Nodes)
                End If
                RestorePoints.Add(P)
                'End SyncLock
            End If
            ' Notify Owner
            Dim e As New AntiguaSaveOperationEventArgs
            e.CmdCode = Command.AutoSaved
            e.Manual = ManualSave
            'TODO        
            'For Each user In Sessions
            '    If user.Value.UserToken.ClientType = ClientType.Owner Then
            '        user.Value.OperationResult(e)
            '    End If
            'Next
        End Sub

        Public Shared Function CloneVNodesList(ByVal source As List(Of clsVisualNode)) As List(Of clsVisualNode)
            Dim dest As New List(Of clsVisualNode)
            For Each item In source
                dest.Add(item.Clone)
            Next
            Return dest
        End Function

        Public Shared Function CloneNodesList(ByVal source As List(Of clsNode)) As List(Of clsNode)
            Dim dest As New List(Of clsNode)
            For Each item In source
                'TODO: AC to implement all attributes cloning in core
                dest.Add(item.Clone)
            Next
            Return dest
        End Function

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
            If RevertTimer IsNot Nothing Then
                RevertTimer.Dispose()
            End If
        End Sub

    End Class
End Namespace