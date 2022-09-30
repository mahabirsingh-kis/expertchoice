Imports System.IO

Namespace ExpertChoice.Data

    Public Enum ecSnapShotType
        Auto = 0
        Manual = 1
        RestorePoint = 2
    End Enum

    <Serializable()> Public Class clsSnapshot

        Public Property ID As Integer = 0
        Public Property Idx As Integer = 0              ' D3729
        Public Property RestoredFrom As Integer = 0     ' D3729
        Public Property DateTime As DateTime
        Public Property ProjectID As Integer = 0
        Public Property Type As ecSnapShotType = ecSnapShotType.Auto
        Public Property Comment As String = ""
        Public Property Details As String = ""      ' D3729 + D3731
        Public Property ProjectStream As MemoryStream = Nothing
        Public Property ProjectStreamMD5 As String = ""
        Public Property ProjectStreamSize As Integer = -1   ' D3775
        Public Property ProjectWorkspace As String = ""
        Public Property ProjectWorkspaceMD5 As String = ""
        Public Property ProjectWorkspaceSize As Integer = -1    ' D3775

        ' D3576 ===
        Public ReadOnly Property SnapshotID As String
            Get
                Return String.Format("{0}{1}{2}", ProjectID.ToString("X4"), ProjectWorkspaceMD5.Substring(0, 3), ProjectStreamMD5.Substring(0, 3)).ToLower
            End Get
        End Property
        ' D3576 ==

    End Class

End Namespace