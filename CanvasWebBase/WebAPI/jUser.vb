Imports System.Reflection
Imports ECCore
Imports ECCore.Attributes
Imports ECCore.ECTypes

Namespace ExpertChoice.WebAPI

    <Serializable> Public Class jUserShort
        Inherits clsJsonObject

        Public Property ID As Integer = -1
        Public Property Email As String = ""
        Public Property Name As String = ""

        Shared Function CreateFromBaseObject(tUser As clsUser) As jUserShort
            If tUser IsNot Nothing Then
                Return New jUserShort With {
                .ID = tUser.UserID,
                .Name = If(tUser.UserName = "", tUser.UserEMail, tUser.UserName),   ' D6429
                .Email = tUser.UserEMail
            }
            Else
                Return Nothing
            End If
        End Function

    End Class

    <Serializable> Public Class jUsersList
        Inherits clsJsonObject

        <JsonProperty("users")>
        Property Users As List(Of jUserProject) = Nothing

        <JsonProperty("groups")>
        Property Groups As Dictionary(Of Integer, String) = Nothing

        <JsonProperty("attributes")>
        Property Attributes As List(Of jUserAttribute) = Nothing

        <JsonProperty("evalProgress")>
        Property EvalProgress As Dictionary(Of Integer, jEvalProgress) = Nothing

    End Class

    <Serializable> Public Class jUserProject
        Inherits jUserShort

        <JsonProperty("hasData")>
        Property HasData As Boolean = False
        <JsonProperty("priority")>
        Property Priority As Double = 0
        <JsonProperty("groupIDs")>
        Property GroupIDs As New List(Of Integer)
        <JsonProperty("attrValues")>
        Property AttrubuteValues As New Dictionary(Of Guid, Object)

        Overloads Shared Function CreateFromBaseObject(tPrjUser As clsUser) As jUserProject
            Dim tUser As jUserProject = CType(jUserProject.doInherit(jUserShort.CreateFromBaseObject(tPrjUser), GetType(jUserProject)), jUserProject)
            If tUser IsNot Nothing Then
            End If
            Return tUser
        End Function

        Shared Function GetList(PM As clsProjectManager, Optional UsersList As List(Of clsUser) = Nothing) As List(Of jUserProject)    ' D7301
            Dim tRes As New List(Of jUserProject)
            If UsersList Is Nothing AndAlso PM IsNot Nothing Then UsersList = PM.UsersList

            If UsersList IsNot Nothing AndAlso PM IsNot Nothing Then
                PM.CombinedGroups.UpdateDynamicGroups()
                Dim UsersWithData As HashSet(Of Integer) = PM.StorageManager.Reader.DataExistsForUsersHashset(PM.ActiveHierarchy)
                For Each tUser As clsUser In UsersList
                    Dim jUser As jUserProject = jUserProject.CreateFromBaseObject(tUser)
                    jUser.HasData = UsersWithData.Contains(tUser.UserID)

                    jUser.Priority = Math.Round(tUser.Weight, 4)

                    'Fill groups IDs
                    For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
                        If group.UsersList.Contains(tUser) Then
                            jUser.GroupIDs.Add(group.ID)
                        End If
                    Next

                    'Fill attribute values
                    For Each tAttr As clsAttribute In PM.Attributes.AttributesList
                        If Not tAttr.IsDefault AndAlso tAttr.Type = AttributeTypes.atUser Then
                            Dim tAttrVal As Object = PM.Attributes.GetAttributeValue(tAttr.ID, tUser.UserID)
                            If tAttrVal IsNot Nothing Then
                                jUser.AttrubuteValues.Add(tAttr.ID, tAttrVal)
                            End If
                        End If
                    Next

                    tRes.Add(jUser)
                Next
            End If
            Return tRes
        End Function

        Shared Function GetUserGroups(GroupsList As List(Of clsGroup), Optional showAllParticipants As Boolean = False) As Dictionary(Of Integer, String)   ' D7308
            Dim tRes As New Dictionary(Of Integer, String)
            If GroupsList IsNot Nothing Then
                For Each tGrp As clsGroup In GroupsList
                    If CType(tGrp, clsCombinedGroup).CombinedUserID <> COMBINED_USER_ID OrElse showAllParticipants Then ' D7308
                        tRes.Add(tGrp.ID, tGrp.Name)
                    End If
                Next
            End If
            Return tRes
        End Function

    End Class

    <Serializable> Public Class jUserAttribute
        Inherits clsJsonObject

        <JsonProperty("guid")>
        Public Property ID() As Guid = Guid.Empty
        <JsonProperty("name")>
        Public Property Name() As String = ""
        <JsonProperty("valType")>
        Public Property ValueType() As AttributeValueTypes = AttributeValueTypes.avtString
        <JsonProperty("defValue")>
        Public Property DefaultValue() As Object = Nothing
        <JsonProperty("isDefault")>
        Public Property IsDefault() As Boolean = False
        <JsonProperty("enum")>
        Public Property EnumValues As List(Of clsAttributeEnumerationItem) = Nothing

        Shared Function CreateFromBaseObject(tAttr As clsAttribute, Attributes As clsAttributes) As jUserAttribute
            If tAttr IsNot Nothing Then
                Dim EnumVals As New List(Of clsAttributeEnumerationItem)
                If Attributes IsNot Nothing AndAlso Not tAttr.EnumID.Equals(Guid.Empty) AndAlso (tAttr.ValueType = AttributeValueTypes.avtEnumeration OrElse tAttr.ValueType = AttributeValueTypes.avtEnumerationMulti) Then
                    For Each tEnum As clsAttributeEnumerationItem In Attributes.GetEnumByID(tAttr.EnumID).Items
                        If Not tEnum.ID.Equals(Guid.Empty) AndAlso Not String.IsNullOrEmpty(tEnum.Value) Then EnumVals.Add(tEnum)
                    Next
                End If
                Return New jUserAttribute With {
                .ID = tAttr.ID,
                .Name = tAttr.Name,
                .ValueType = tAttr.ValueType,
                .DefaultValue = tAttr.DefaultValue,
                .IsDefault = tAttr.IsDefault,
                .EnumValues = EnumVals
            }
            Else
                Return Nothing
            End If
        End Function

        Shared Function GetList(List As List(Of clsAttribute), Attributes As clsAttributes, Optional IncludeDefaultAttribs As Boolean = False) As List(Of jUserAttribute)
            Dim tRes As New List(Of jUserAttribute)
            If List IsNot Nothing Then
                For Each tAttr As clsAttribute In List
                    If Not tAttr.IsDefault OrElse IncludeDefaultAttribs Then tRes.Add(jUserAttribute.CreateFromBaseObject(tAttr, Attributes))
                Next
            End If
            Return tRes
        End Function

    End Class

    <Serializable> Public Class jEvalProgress
        Inherits clsJsonObject

        <JsonProperty("userID")>
        Public Property UserID As Integer = -1

        <JsonProperty("total")>
        Public Property TotalCount As Integer = -1

        <JsonProperty("made")>
        Public Property MadeCount As Integer = -1

        <JsonProperty("perc")>
        Public Property Percentage As Double = 0
        '<JsonProperty("progress")>
        'Public ReadOnly Property Progress As String
        '    Get
        '        Return String.Format("{0}% ({1}/{2})", Percentage, MadeCount, TotalCount)
        '    End Get
        'End Property
        <JsonProperty("lastJudgment")>
        Public Property LastJudgment As Date? = Nothing

        Shared Function getEvalPercentage(made As Integer, total As Integer) As Double
            Return If(made > 0 AndAlso total > 0, made / total * 100, 0)
        End Function

        Shared Function GetList(PM As clsProjectManager, Optional UsersList As List(Of clsUser) = Nothing) As Dictionary(Of Integer, jEvalProgress)
            Dim tRes As New Dictionary(Of Integer, jEvalProgress)
            Dim IsEvaluationProgressForTreatments As Boolean = False

            If UsersList Is Nothing AndAlso PM IsNot Nothing Then UsersList = PM.UsersList
            If UsersList IsNot Nothing AndAlso PM IsNot Nothing Then

                Dim madeAllCount As Integer = 0
                Dim totalAllCount As Integer = 0
                Dim evalProgress As New Dictionary(Of String, UserEvaluationProgressData)
                If Not IsEvaluationProgressForTreatments Then
                    evalProgress = PM.StorageManager.Reader.GetEvaluationProgress(UsersList, PM.ActiveHierarchy, madeAllCount, totalAllCount)
                End If

                For Each tUser As clsUser In UsersList

                    Dim userLastJudgmentTime As Date? = Nothing

                    Dim madeCount As Integer
                    Dim totalCount As Integer
                    If IsEvaluationProgressForTreatments Then
                        ' Evaluation progress for Riskion Treatments
                        PM.PipeBuilder.GetControlsEvaluationProgress(tUser.UserID, madeCount, totalCount, userLastJudgmentTime.Value)
                    Else
                        ' Evaluation progress for Anytime
                        If evalProgress.ContainsKey(tUser.UserEMail.ToLower) Then
                            With evalProgress(tUser.UserEMail.ToLower)
                                madeCount = .EvaluatedCount
                                totalCount = .TotalCount
                                If .LastJudgmentTime.HasValue Then
                                    userLastJudgmentTime = .LastJudgmentTime
                                End If
                            End With
                        End If
                    End If

                    If madeCount > totalCount Then madeCount = totalCount
                    If madeCount <= 0 AndAlso userLastJudgmentTime.HasValue Then userLastJudgmentTime = Nothing

                    tRes.Add(tUser.UserID, New jEvalProgress With {
                        .UserID = tUser.UserID,
                        .MadeCount = madeCount,
                        .TotalCount = totalCount,
                        .Percentage = jEvalProgress.getEvalPercentage(madeCount, totalCount),
                        .LastJudgment = userLastJudgmentTime
                    })
                Next
            End If
            Return tRes
        End Function

    End Class


    ' D6429 ===
    <Serializable> Public Class jUserTeamTime
        Inherits jUserShort

        Public Property AccessMode As SynchronousEvaluationMode = SynchronousEvaluationMode.semNone
        Public Property Keypad As Integer = -1
        Public Property HasData As Boolean = False
        Public Property isOnline As Boolean = False
        Public Property CanEvaluate As Boolean = True
        Public Property Link As String = ""
        Public Property Groups As String = ""
        Public Property GroupIDs As New List(Of Integer)

        Overloads Shared Function CreateFromBaseObject(tPrjUser As clsUser) As jUserTeamTime
            Dim tRes As jUserTeamTime = Nothing
            If tPrjUser IsNot Nothing Then
                'tRes = CType(jUserTeamTime.doInherit(jUserTeamTime.CreateFromBaseObject(tPrjUser), GetType(jUserTeamTime)), jUserTeamTime)    ' D7267
                tRes = CType(clsJsonObject.doInherit(jUserShort.CreateFromBaseObject(tPrjUser), GetType(jUserTeamTime)), jUserTeamTime)    ' D7267
                If tRes IsNot Nothing Then
                    With tRes
                        .AccessMode = tPrjUser.SyncEvaluationMode
                        .Keypad = tPrjUser.VotingBoxID
                    End With
                End If
            End If
            Return tRes
        End Function
        ' D6429 ==

    End Class

    ' D6430 ===
    <Serializable> Public Class jUserTeamTimeOptions
        Inherits clsJsonObject

        Public Property Email As String = ""
        Public Property AccessMode As SynchronousEvaluationMode = SynchronousEvaluationMode.semNone
        Public Property Keypad As Integer = -1

    End Class
    ' D6430 ==

End Namespace
