Imports ECCore
Imports System.Reflection

Namespace ExpertChoice.WebAPI

    <Serializable> Public Class jWorkgroup
        Inherits clsJsonObject

        <JsonProperty("id")>
        Public Property ID As Integer = 0

        <JsonProperty("name")>
        Public Property Name As String = ""

        <JsonProperty("isRisk")>
        Public Property isRisk As Boolean = False

        <JsonProperty("lastVisited")>
        Public Property lastVisited As Date? = Nothing  ' D7206

        <JsonProperty("status")>
        Public Property Status As ecWorkgroupStatus = ecWorkgroupStatus.wsEnabled   ' D7208

        <JsonProperty("isLicenseValid")>
        Public Property IsLicenseValid As Boolean = False   ' D7208

        Shared Function CreateFromBaseObject(tWkg As clsWorkgroup, Optional UserWorkgroup As clsUserWorkgroup = Nothing) As jWorkgroup  ' D7206
            If tWkg IsNot Nothing Then
                Return New jWorkgroup With {
                    .ID = tWkg.ID,
                    .Name = tWkg.Name,
                    .isRisk = tWkg.License.CheckParameterByID(ECSecurity.ECSecurity.ParamsConstants.ecLicenseParameter.RiskEnabled, Nothing, True),
                    .lastVisited = If(UserWorkgroup IsNot Nothing AndAlso UserWorkgroup.LastVisited.HasValue, UserWorkgroup.LastVisited.Value, Nothing),
                    .Status = tWkg.Status,
                    .IsLicenseValid = tWkg.License IsNot Nothing AndAlso tWkg.License.isValidLicense}   ' D7206
            Else
                Return Nothing
            End If
        End Function

    End Class

    <Serializable> Public Class jNodeSet
        Inherits clsJsonObject

        Public Property Name As String = ""
        Public Property Hierarchy As ecNodeSetHierarchy = ecNodeSetHierarchy.hidObjectives
        Public Property isRisk As Boolean = False
        Public Property Nodes As New List(Of jNode)

        Shared Sub clsNodes2jNode(Nodes As List(Of clsNode), ByRef Items As List(Of jNode))
            ' D5054 ===
            If Items Is Nothing Then Items = New List(Of jNode)
            If Nodes IsNot Nothing Then
                For Each tNode As clsNode In Nodes
                    Dim tItem As jNode = jNode.CreateFromBaseObject(tNode)
                    Items.Add(tItem)
                Next
            End If
            ' D5054 ==
        End Sub

        Shared Function CreateFromBaseObject(tSet As clsNodeSet) As jNodeSet
            If tSet IsNot Nothing Then
                Dim Items As New List(Of jNode)
                Dim Nodes As List(Of clsNode) = tSet.AsNodes()
                If Nodes IsNot Nothing Then clsNodes2jNode(Nodes, Items)
                Return New jNodeSet With {
                    .Name = tSet.Name,
                    .Hierarchy = tSet.Hierarchy,
                    .isRisk = tSet.isRisk,
                    .Nodes = Items
                }
            Else
                Return Nothing
            End If
        End Function

    End Class

End Namespace