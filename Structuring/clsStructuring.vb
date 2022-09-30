Imports ECCore

Namespace ExpertChoice.Structuring

    <KnownType(GetType(MeetingState))>
    <KnownType(GetType(MeetingMode))>
    <KnownType(GetType(StructuringMode))>    
    <Serializable>
    Public Class MeetingInfo
        Public Property MeetingID() As Integer
        Public Property State() As MeetingState
        Public Property OwnerEmail() As String
        Public Property OwnerName() As String
        Public Property ProjectID() As Integer
        Public Property MeetingPassword() As String
        Private mProjectManager As clsProjectManager = Nothing 'A0651
        Public Property ProjectManager() As clsProjectManager
        Public Property SaveOnDemand() As Boolean = False 'C0610
        Public Property IsMeetingLocked() As Boolean 'A0093
        'Public Property BoardLocked() As Boolean 'A0093
        'Public Property RecycleLocked() As Boolean 'A0093

        Public Property TreeMode() As MeetingMode = MeetingMode.Sources
        Public Property BoardMode() As MeetingMode = MeetingMode.Sources

        Public Property CSMode() As StructuringMode = StructuringMode.Collaborative
        Public Property AllowComments() As Boolean = True

        Public Property ActiveHierarchyID() As ECCore.ECTypes.ECHierarchyID = ECCore.ECHierarchyID.hidLikelihood 'A0777

    End Class

End Namespace
