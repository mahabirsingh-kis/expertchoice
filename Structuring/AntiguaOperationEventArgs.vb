Imports ExpertChoice.Structuring
Imports Canvas
Imports ECCore.ECTypes
Imports System.IO
Imports System.Runtime.Serialization.Json
Imports System.Text
'Imports Newtonsoft.Json

Public Enum Command
    Connect
    ClientArrival
    SetMeetingState
    SetMeetingMode
    SetActiveHierarchyID
    CreateNewNode
    DeleteNodes
    MoveNodesOnBoard
    MoveFromTreeToBoard
    MoveFromBoardToTree
    CopyAltToBoard
    CopyAllAltsToBoard
    MoveAltToList
    ConvertCoveringObjsCopy
    ConvertCoveringObjsMove
    ReorderInTree
    ReorderInAlts
    'SetTreeLock
    SetMeetingLock
    ChatMessage
    DoRevert
    AutoSaved
    SetNodesColor
    Refresh
    DisconnectUser
    'DisconnectUserWhenMeetingStopped
    RefreshUsersList
    RenameItem
    ResizeItem
    RestoreFromRecycle
    SwitchProsCons
    GrantPermissions
    CopyInObjectives
    InfoDocChanged

    CreateNewItemToProsCons
    CopyFromProsConsToTree
    DeleteProOrCon
    CopyFromProsToCons
    RenameProsConsItem
    MarkProsConsItem

    AddComment
    EditComment
    DeleteComment
    SwitchComments

    AddNode
    AddRiskObjectives
    SendToObjectives
    SendToSources
    SendToConsequences
    SendToAlternatives

    DropAsSubObjective
    DetachSubObjective

    SetContribution
    RemoveContribution
    GetContributionsForNode

    Setting
    EditGoal
    PollTimeStamp

    NoSuccess
End Enum

<KnownType(GetType(Command))>
<KnownType(GetType(AntiguaConnectOperationEventArgs))>
<KnownType(GetType(AntiguaClientArrivalEventArgs))>
<KnownType(GetType(AntiguaStateOperationEventArgs))>
<KnownType(GetType(AntiguaMoveToHierarchyOperationEventArgs))>
<KnownType(GetType(AntiguaReorderOperationEventArgs))>
<KnownType(GetType(AntiguaCopyOperationEventArgs))>
<KnownType(GetType(AntiguaDeleteOperationEventArgs))>
<KnownType(GetType(AntiguaPropertiesOperationEventArgs))>
<KnownType(GetType(AntiguaChatOperationEventArgs))>
<KnownType(GetType(AntiguaNewNodeOperationEventArgs))>
<KnownType(GetType(AntiguaSaveOperationEventArgs))>
<KnownType(GetType(AntiguaMoveToBoardEventArgs))>
<KnownType(GetType(AntiguaDisconnectUserEventArgs))>
<KnownType(GetType(AntiguaSwitchProsConsEventArgs))>
<KnownType(GetType(AntiguaGrantPermissionsEventArgs))>
<KnownType(GetType(AntiguaInfoDocChangedEventArgs))>
<KnownType(GetType(AntiguaProsConsEventArgs))>
<KnownType(GetType(AntiguaCommentEventArgs))>
<KnownType(GetType(AntiguaConvertObjectivesEventArgs))>
<KnownType(GetType(AntiguaAddNodeOperationEventArgs))>
<KnownType(GetType(AntiguaAddRiskObjectivesOperationEventArgs))>
<KnownType(GetType(AntiguaCopyToBoardEventArgs))>
<KnownType(GetType(AntiguaSettingEventArgs))>
Public Class AntiguaOperationEventArgs

    Public Property CmdCode() As Command
    Public Property CmdOwner() As Integer
    Public Property MeetingOwner() As Integer = -1
    Public Property isAnonymousAction As Boolean = False
    Public Property DT As Long
    Public Property Tag As Object

    Public Function GetJSON() As String
        Dim ms As MemoryStream = New MemoryStream()
        Dim ser As DataContractJsonSerializer = New DataContractJsonSerializer(GetType(AntiguaOperationEventArgs))

        ser.WriteObject(ms, Me)

        ms.Position = 0
        Dim sr As StreamReader = New StreamReader(ms)

        Dim retVal As String = sr.ReadToEnd()
        ms.Close()

        Return retVal

        'Return JsonConvert.SerializeObject(Me)
    End Function

    Public Shared Function ReadJSON(sJSON As String) As AntiguaOperationEventArgs
        Dim deserializedData As AntiguaOperationEventArgs = New AntiguaOperationEventArgs()

        Dim ms As MemoryStream = New MemoryStream(Encoding.UTF8.GetBytes(sJSON))
        Dim ser As DataContractJsonSerializer = New DataContractJsonSerializer(GetType(AntiguaOperationEventArgs))

        deserializedData = Ctype(ser.ReadObject(ms), AntiguaOperationEventArgs)
        ms.Close()

        Return deserializedData

        'Return JsonConvert.DeserializeObject(Of AntiguaOperationEventArgs)(sJSON)
    End Function


End Class


Public Class AntiguaConnectOperationEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property ConnectError() As ConnectErrorCode = ConnectErrorCode.None
    Public Property Token() As UserToken
    Public Property CanBePM As Boolean = False
    Public Property CanChangePM As Boolean = False
    Public Property IsPM As Boolean = False
    Public Property State() As MeetingState
    Public Property TreeMode() As MeetingMode
    Public Property BoardMode() As MeetingMode
    Public Property CSMode() As StructuringMode
    Public Property BoardNodes() As List(Of clsVisualNode)
    Public Property TreeNodes() As List(Of clsVisualNode)
    Public Property RecycleNodes() As List(Of clsVisualNode)
    Public Property AltsNodes() As List(Of clsVisualNode)
    Public Property Users() As List(Of UserToken)
    Public Property IsRefreshing() As Boolean = False
    Public Property Credentials() As String = ""
    ' D0826 ===
    Public Property InstanceID() As String = ""
    ' D0826 ==
    Public Property TreeLocked() As Boolean = False
    Public Property BoardLocked() As Boolean = False
    Public Property RecycleLocked() As Boolean = False

    Public Property ActiveHierarchyID() As ECCore.ECTypes.ECHierarchyID = ECHierarchyID.hidLikelihood 'A0777
End Class


Public Class AntiguaClientArrivalEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property Token() As UserToken
    Public Property Entry() As MeetingEntry
End Class

Public Class AntiguaStateOperationEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property State() As MeetingState
    Public Property TreeMode() As MeetingMode
    Public Property BoardMode() As MeetingMode
    Public Property CSMode() As StructuringMode

    Public Property IsMeetingLocked() As Boolean
    'Public Property BoardLocked() As Boolean
    'Public Property RecycleLocked() As Boolean

    Public Property SetActiveHierarchyID() As Boolean = False
    Public Property ActiveHierarchyID() As ECCore.ECTypes.ECHierarchyID = ECHierarchyID.hidLikelihood 'A0777
End Class


Public Class AntiguaMoveToHierarchyOperationEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property HierarchyID() As ECHierarchyID = ECHierarchyID.hidLikelihood
    Public Property NodesGuids() As List(Of Guid)
    Public Property DestNodeGuid() As Guid
    Public Property Action() As NodeMoveAction
    Public Property Position() As Integer
End Class


Public Class AntiguaReorderOperationEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property SourceNodeGuid() As Guid
    Public Property DestNodeGuid() As Guid
    Public Property Action() As NodeMoveAction
End Class


Public Class AntiguaCopyOperationEventArgs
    Inherits AntiguaReorderOperationEventArgs


    Public Property CopyAllDescendants() As Boolean = False
    Public Property ResList() As List(Of clsVisualNode)
End Class


Public Class AntiguaDeleteOperationEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property IsWhiteboardItems As Boolean = False
    Public Property NodesGuids() As List(Of Guid)
End Class


Public Class AntiguaPropertiesOperationEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property NodeGuid() As Guid
    Public Property Title() As String

    Public Property Width() As Double
    Public Property Height() As Double

    Public Property sColor() As String
End Class


Public Class AntiguaChatOperationEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property UserName() As String
    Public Property Text() As String
    Public Property TimeStamp() As String
    Public Property AtTime() As DateTime
End Class


Public Class AntiguaNewNodeOperationEventArgs 'Add new node to the whiteboard
    Inherits AntiguaOperationEventArgs

    Public Property TmpID() As String
    Public Property Node() As clsVisualNode
End Class


Public Class AntiguaAddNodeOperationEventArgs 'Add node to alternatives or objectives hierarchy
    Inherits AntiguaOperationEventArgs

    Public Property NodeTitle() As String
    Public Property NodeID() As Guid
    Public Property ParentNodeID() As Guid
    Public Property IsAlternative() As Boolean
    Public Property HierarchyID As ECCore.ECTypes.ECHierarchyID = ECHierarchyID.hidLikelihood

End Class


Public Class AntiguaAddRiskObjectivesOperationEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property NodesNames() As String
    Public Property NewNodes() As Dictionary(Of Guid, String)
    Public Property HierarchyID As ECCore.ECTypes.ECHierarchyID = ECHierarchyID.hidLikelihood

End Class


Public Class AntiguaSaveOperationEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property Manual() As Boolean = False
    Public Property RevertID() As Integer
End Class



Public Class AntiguaMoveToBoardEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property NodeGuid() As Guid
    Public Property Location() As GUILocation = GUILocation.Board
    Public Property Position() As System.Drawing.Point
    Public Property Size() As System.Drawing.Point
End Class


Public Class AntiguaCopyToBoardEventArgs
    Inherits AntiguaOperationEventArgs

    Public Enum CopyModes
        cmTile
        cmList
    End Enum

    Public Property Position() As System.Drawing.Point
    Public Property Size() As System.Drawing.Point
    Public Property CopyMode() As CopyModes
    Public Property ArrangedNodesCoords() As Dictionary(Of Guid, System.Drawing.Point)
End Class


Public Class AntiguaDisconnectUserEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property TokenID() As Integer
End Class


Public Class AntiguaGrantPermissionsEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property TokenID() As Integer
End Class


Public Class AntiguaSwitchProsConsEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property NodeGuid() As Guid
    Public Property Show() As Boolean
    'Public Property DoForAll() As Boolean
    Public Property Mode() As String
End Class


Public Class AntiguaInfoDocChangedEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property NodeGuid() As Guid
    Public Property HasInfodoc() As Boolean = False
End Class


Public Class AntiguaProsConsEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property NodeGuid() As Guid
    Public Property Source() As List(Of Guid)
    Public Property Target() As List(Of Guid)
    Public Property Result() As List(Of Guid)
    Public Property Position() As System.Drawing.Point
    Public Property NewItemTitle() As String
    Public Property IsPro() As Boolean
    Public Property Action() As NodeMoveAction
    Public Property AltIndex() As Integer
    Public Property Mark() As Boolean
    'Public Property StringList As List(Of String)
    Public Property DoForAll As Boolean = False
End Class


Public Class AntiguaCommentEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property NodeGuid() As Guid
    Public Property CommentGuid() As Guid
    Public Property CommentString() As String
    Public Property AllowComments() As Boolean
End Class


Public Class AntiguaConvertObjectivesEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property NodeGuid() As Guid
    Public Property NewAlternativesGuids() As Dictionary(Of Guid, Guid) = New Dictionary(Of Guid, Guid)
    Public Property doCopy() As Boolean = False

End Class


Public Class AntiguaContributionsForAlternativeEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property AltGuid() As Guid
    Public Property ContributedCauses() As New Dictionary(Of Guid, String)
    Public Property ContributedConsequences() As New Dictionary(Of Guid, String)

End Class

Public Class AntiguaContributionDataItem

    Public Property ID() As Guid
    Public Property Name() As String
    Public Property BooleanValue() As Boolean

End Class


Public Class AntiguaSettingEventArgs
    Inherits AntiguaOperationEventArgs

    Public Property Name() As String
    Public Property Value() As String

End Class
