Imports ECCore
Imports System.Linq

<Serializable()> Public Enum InfoDocType
    idtNode = 0
    idtNodeWRTParent = 1
    idtJudgment = 2 ' not in use currently
    idtUser = 3 ' not in use currently
    idtUserWRTGroup = 4 ' not in use currently
    idtProjectDescription = 5
    idtCustom = 6
End Enum

<Serializable()> Public Class clsInfoDoc 'C0920
    Public Property DocumentType() As InfoDocType
    Public Property TargetID() As Guid
    Public Property AdditionalID() As Guid
    Public Property InfoDoc() As String

    Public Sub New(ByVal DocType As InfoDocType, ByVal TargetID As Guid, ByVal AdditionalID As Guid, ByVal InfoDoc As String)
        Me.DocumentType = DocType
        Me.TargetID = TargetID
        Me.AdditionalID = AdditionalID
        Me.InfoDoc = InfoDoc
    End Sub
End Class

<Serializable()> Public Class clsInfoDocs
    Property ProjectManager() As clsProjectManager
    Property InfoDocs() As New List(Of clsInfoDoc)

    Public Function GetNodeInfoDoc(ByVal NodeGuid As Guid) As String
        Dim iDoc As clsInfoDoc = InfoDocs.FirstOrDefault(Function(doc) (doc.DocumentType = InfoDocType.idtNode AndAlso doc.TargetID.Equals(NodeGuid)))
        Return If(iDoc IsNot Nothing, iDoc.InfoDoc, "")
    End Function

    Public Function SetNodeInfoDoc(ByVal NodeGuid As Guid, ByVal InfoDoc As String) As clsInfoDoc
        Dim iDoc As clsInfoDoc = InfoDocs.FirstOrDefault(Function(doc) (doc.DocumentType = InfoDocType.idtNode AndAlso doc.TargetID.Equals(NodeGuid)))
        If iDoc IsNot Nothing Then
            iDoc.InfoDoc = InfoDoc
        Else
            iDoc = New clsInfoDoc(InfoDocType.idtNode, NodeGuid, Guid.Empty, InfoDoc)
            InfoDocs.Add(iDoc)
        End If
        Return iDoc
    End Function

    Public Function GetCustomInfoDoc(ByVal tID As Guid, tExtraGUID As Guid) As String
        Dim iDoc As clsInfoDoc = InfoDocs.FirstOrDefault(Function(doc) (doc.DocumentType = InfoDocType.idtCustom AndAlso doc.TargetID.Equals(tID) AndAlso ((tExtraGUID.Equals(Guid.Empty) OrElse tExtraGUID.Equals(doc.AdditionalID)))))
        Return If(iDoc IsNot Nothing, iDoc.InfoDoc, "")
    End Function

    Public Function SetCustomInfoDoc(ByVal InfoDoc As String, ByVal tID As Guid, tExtraGUID As Guid) As clsInfoDoc
        Dim iDoc As clsInfoDoc = InfoDocs.FirstOrDefault(Function(doc) (doc.DocumentType = InfoDocType.idtCustom AndAlso doc.TargetID.Equals(tID) AndAlso ((tExtraGUID.Equals(Guid.Empty) OrElse tExtraGUID.Equals(doc.AdditionalID)))))
        If iDoc IsNot Nothing Then
            iDoc.InfoDoc = InfoDoc
        Else
            iDoc = New clsInfoDoc(InfoDocType.idtCustom, tID, tExtraGUID, InfoDoc)
            InfoDocs.Add(iDoc)
        End If
        Return iDoc
    End Function

    Public Function GetProjectDescription() As String
        Dim iDoc As clsInfoDoc = InfoDocs.FirstOrDefault(Function(doc) (doc.DocumentType = InfoDocType.idtProjectDescription))
        Return If(iDoc IsNot Nothing, iDoc.InfoDoc, "")
    End Function

    Public Function SetProjectDescription(ByVal InfoDoc As String) As clsInfoDoc
        Dim iDoc As clsInfoDoc = InfoDocs.FirstOrDefault(Function(doc) (doc.DocumentType = InfoDocType.idtProjectDescription))
        If iDoc IsNot Nothing Then
            iDoc.InfoDoc = InfoDoc
        Else
            iDoc = New clsInfoDoc(InfoDocType.idtProjectDescription, Guid.Empty, Guid.Empty, InfoDoc)
            InfoDocs.Add(iDoc)
        End If
        Return iDoc
    End Function

    Public Function GetNodeWRTInfoDoc(ByVal NodeGuid As Guid, ByVal ParentNodeGuid As Guid) As String
        Dim iDoc As clsInfoDoc = InfoDocs.FirstOrDefault(Function(doc) (doc.DocumentType = InfoDocType.idtNodeWRTParent AndAlso doc.TargetID.Equals(NodeGuid) AndAlso doc.AdditionalID.Equals(ParentNodeGuid)))
        Return If(iDoc IsNot Nothing, iDoc.InfoDoc, "")
    End Function

    Public Function SetNodeWRTInfoDoc(ByVal NodeGuid As Guid, ByVal ParentNodeGuid As Guid, ByVal InfoDoc As String) As clsInfoDoc
        Dim iDoc As clsInfoDoc = InfoDocs.FirstOrDefault(Function(doc) (doc.DocumentType = InfoDocType.idtNodeWRTParent AndAlso doc.TargetID.Equals(NodeGuid) AndAlso doc.AdditionalID.Equals(ParentNodeGuid)))
        If iDoc IsNot Nothing Then
            iDoc.InfoDoc = InfoDoc
        Else
            iDoc = New clsInfoDoc(InfoDocType.idtNodeWRTParent, NodeGuid, ParentNodeGuid, InfoDoc)
            InfoDocs.Add(iDoc)
        End If
        Return iDoc
    End Function

    Public Sub New(ByVal ProjectManager As clsProjectManager)
        Me.ProjectManager = ProjectManager
    End Sub
End Class