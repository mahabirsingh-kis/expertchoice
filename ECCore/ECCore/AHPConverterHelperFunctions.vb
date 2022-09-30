Imports System.Linq

Namespace ECCore.AHPConverterHelperFunctions
    Module AHPConverterHelperFunctions
        Public Function IsHierarchyNodesZeroBased(ByVal Hierarchy As clsHierarchy) As Boolean
            Dim node As clsNode = Hierarchy.Nodes.FirstOrDefault(Function(n) (n.NodeID = 0))
            Return node IsNot Nothing
        End Function

        Public Function IsGroupsZeroBased(ProjectManager As clsProjectManager) As Boolean
            Dim group As clsGroup = ProjectManager.CombinedGroups.GroupsList.FirstOrDefault(Function(g) (g.ID = 0))
            Return group IsNot Nothing
        End Function

        Public Function IsNodesZeroBased(ProjectManager As clsProjectManager) As Boolean
            Return IsHierarchyNodesZeroBased(ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy))
        End Function

        Public Function IsAltsZeroBased(ProjectManager As clsProjectManager) As Boolean
            Return IsHierarchyNodesZeroBased(ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy))
        End Function

        Public Function GetNextAltID(ProjectManager As clsProjectManager) As Integer
            Dim max As Integer = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes.Select(Function(a) (a.NodeID)).DefaultIfEmpty(0).Max
            Return If(IsAltsZeroBased(ProjectManager), max + 2, max + 1)
        End Function

        Public Function GetNextNodeID(ProjectManager As clsProjectManager) As Integer
            Dim max As Integer = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes.Select(Function(n) (n.NodeID)).DefaultIfEmpty(0).Max
            Return If(IsNodesZeroBased(ProjectManager), max + 2, max + 1)
        End Function

        Public Function GetNextGroupID(ProjectManager As clsProjectManager) As Integer
            Dim max As Integer = ProjectManager.CombinedGroups.GroupsList.Select(Function(g) (g.ID)).DefaultIfEmpty(0).Max
            Return If(IsGroupsZeroBased(ProjectManager), max + 2, max + 1)
        End Function

        Public Function GetNextUserID(ProjectManager As clsProjectManager) As Integer
            Dim uCount As Integer = ProjectManager.UsersList.Count
            Return If(uCount = 0, 0, If(ProjectManager.UsersList.Count > 1, ProjectManager.UsersList.Count + 1, 1))
        End Function

        Public Function GetAHPUserID(ProjectManager As clsProjectManager, ByVal user As clsUser) As Integer
            If user Is Nothing OrElse ProjectManager Is Nothing Then Return -1
            If ProjectManager.GetUserByID(user.UserID) Is Nothing Then Return -1
            Dim i As Integer = ProjectManager.UsersList.IndexOf(user)
            If i >= 1 Then i += 1
            Return i
        End Function
    End Module

End Namespace
