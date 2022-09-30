Imports ECCore
Imports System.Xml
Imports System.Linq

Namespace ECCore
    Public Module Groups
        Public Enum RuleAggregator
            raAND = 0
            raOR = 1
        End Enum

        Public Enum RuleOperation
            roContains = 0
            roEqual = 1
            roNotEqual = 2
            roStartsWith = 3
            GreaterThan = 4
            GreaterThanOrEqual = 5
            LessThan = 6
            LessThanOrequal = 7
            IsTrue = 8
            IsFalse = 9
        End Enum

        <Serializable()> Public Class clsRule
            Public Property Attribute As clsAttribute
            Public Property OperationType As RuleOperation
            Public Property FilterText As String

            Public ReadOnly Property RuleText As String
                Get
                    Dim res As String = """" + Attribute.Name + """"
                    Select Case OperationType
                        Case RuleOperation.roEqual
                            res += " = "
                        Case RuleOperation.roNotEqual
                            res += " <> "
                        Case RuleOperation.GreaterThan
                            res += " > "
                        Case RuleOperation.GreaterThanOrEqual
                            res += " >= "
                        Case RuleOperation.IsFalse
                            res += " = False"
                        Case RuleOperation.IsTrue
                            res += " = True"
                        Case RuleOperation.LessThan
                            res += " < "
                        Case RuleOperation.LessThanOrequal
                            res += " <= "
                        Case RuleOperation.roContains
                            res += " contains "
                        Case RuleOperation.roStartsWith
                            res += " starts with "
                    End Select

                    If OperationType <> RuleOperation.IsFalse And OperationType <> RuleOperation.IsTrue Then
                        res += """" + FilterText + """"
                    End If
                    Return res
                End Get
            End Property

            Public Sub New(Attribute As clsAttribute, OperationType As RuleOperation, FilterText As String)
                Me.Attribute = Attribute
                Me.OperationType = OperationType
                Me.FilterText = FilterText
            End Sub
        End Class

        <Serializable()> Public Class clsGroup
            Public Property ID() As Integer
            Public Property Name() As String
            Public Property Rule() As String = ""
        End Class

        <Serializable()> Public Class clsCombinedGroup 'C0152
            Inherits clsGroup

            Private mUsersList As List(Of clsUser) 'C0672

            Private mPrjManager As clsProjectManager 'C0163

            Public RulesParsed As Boolean = False

            Private mRules As New List(Of clsRule)
            Private mRuleAggregator As RuleAggregator

            Private Function ParseRules() As Boolean
                mRules.Clear()

                Dim reader As XmlReader = Nothing
                reader = XmlReader.Create(New System.IO.StringReader(Rule))
                'reader.WhitespaceHandling = WhitespaceHandling.None

                reader.ReadToFollowing("Settings")

                If (reader.NodeType = XmlNodeType.Element) And (reader.Name = "Settings") Then
                    Dim fc As String = reader.GetAttribute("FilterCombination")
                    mRuleAggregator = CType(fc, Integer)
                End If

                'Debug.Print("Rules:")
                reader.Read()
                While reader.Read()
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = "Rules") Then
                        While reader.Read()
                            If (reader.NodeType = XmlNodeType.Element) And (reader.Name = "Rule") Then
                                Dim aID As String = reader.GetAttribute("AttributeID")
                                Dim aGuidID As New Guid(aID)
                                Dim attribute As clsAttribute = mPrjManager.Attributes.GetAttributeByID(aGuidID)

                                Dim operation As String = reader.GetAttribute("OperationID")
                                Dim operationType As RuleOperation = CType(CInt(operation), RuleOperation)

                                Dim filter As String = reader.GetAttribute("FilterText").Trim(CChar(""""))

                                If attribute IsNot Nothing Then
                                    Dim NewRule As New clsRule(attribute, operationType, filter)
                                    mRules.Add(NewRule)
                                    'Debug.Print(NewRule.RuleText)
                                End If
                            End If
                        End While
                    End If
                End While
                Return True
            End Function

            Public Function GetRulesText() As String
                ParseRules()
                Dim res As String = ""
                For i As Integer = 0 To mRules.Count - 1
                    res += "(" + mRules(i).RuleText + ")" + If(i = mRules.Count - 1, "", If(mRuleAggregator = RuleAggregator.raAND, " AND ", " OR "))
                Next
                Return res
            End Function

            Public Sub ApplyRules()
                If CombinedUserID = COMBINED_USER_ID Or Rule = "" Then
                    Exit Sub
                End If

                ParseRules()

                mUsersList.Clear()
                For Each user As clsUser In mPrjManager.UsersList
                    Dim bIsTrue As Boolean = If(mRuleAggregator = RuleAggregator.raOR, False, True)
                    Dim i As Integer = 0
                    While (i < mRules.Count) And ((Not bIsTrue And mRuleAggregator = RuleAggregator.raOR) Or (bIsTrue And mRuleAggregator = RuleAggregator.raAND))
                        Dim R As clsRule = mRules(i)
                        Dim value As Object = mPrjManager.Attributes.GetAttributeValue(R.Attribute.ID, user.UserID)
                        If value IsNot Nothing OrElse R.Attribute.ValueType = AttributeValueTypes.avtString Then 'A0938
                            Select Case R.Attribute.ValueType
                                Case AttributeValueTypes.avtBoolean
                                    Dim bValue As Boolean = CType(value, Boolean)
                                    Select Case R.OperationType
                                        Case RuleOperation.IsTrue
                                            bIsTrue = (bValue = True)
                                        Case RuleOperation.IsFalse
                                            bIsTrue = (bValue = False)
                                    End Select
                                Case AttributeValueTypes.avtDouble
                                    Dim dValue As Double = CType(value, Double)
                                    Dim filterValue As Double = 0
                                    If ExpertChoice.Service.StringFuncs.String2Double(R.FilterText, filterValue) Then 'A1175
                                        Select Case R.OperationType
                                            Case RuleOperation.roEqual
                                                bIsTrue = dValue = filterValue
                                            Case RuleOperation.roNotEqual
                                                bIsTrue = dValue <> filterValue
                                            Case RuleOperation.GreaterThan
                                                bIsTrue = dValue > filterValue
                                            Case RuleOperation.GreaterThanOrEqual
                                                bIsTrue = dValue >= filterValue
                                            Case RuleOperation.LessThan
                                                bIsTrue = dValue < filterValue
                                            Case RuleOperation.LessThanOrequal
                                                bIsTrue = dValue <= filterValue
                                        End Select
                                    End If 
                                Case AttributeValueTypes.avtLong
                                    Dim lValue As Long = CType(value, Long)
                                    Dim filterValue As Long = 0
                                    If Long.TryParse(R.FilterText, filterValue) Then 'A1175
                                        Select Case R.OperationType
                                            Case RuleOperation.roEqual
                                                bIsTrue = lValue = filterValue
                                            Case RuleOperation.roNotEqual
                                                bIsTrue = lValue <> filterValue
                                            Case RuleOperation.GreaterThan
                                                bIsTrue = lValue > filterValue
                                            Case RuleOperation.GreaterThanOrEqual
                                                bIsTrue = lValue >= filterValue
                                            Case RuleOperation.LessThan
                                                bIsTrue = lValue < filterValue
                                            Case RuleOperation.LessThanOrequal
                                                bIsTrue = lValue <= filterValue
                                        End Select
                                    End If
                                Case AttributeValueTypes.avtString
                                    'A0938 ===
                                    Dim sValue As String = ""
                                    If value IsNot Nothing Then sValue = CType(value, String).Trim.ToLower()
                                    'A0938 ==
                                    Select Case R.OperationType
                                        Case RuleOperation.roContains
                                            bIsTrue = sValue.Contains(R.FilterText.ToLower)
                                        Case RuleOperation.roEqual
                                            bIsTrue = (sValue = R.FilterText.ToLower)
                                        Case RuleOperation.roNotEqual
                                            bIsTrue = (sValue <> R.FilterText.ToLower)
                                        Case RuleOperation.roStartsWith
                                            bIsTrue = (sValue.Substring(0, R.FilterText.Length) = R.FilterText.ToLower)
                                    End Select
                            End Select
                        Else
                            bIsTrue = False
                        End If
                        i += 1
                    End While

                    If bIsTrue Then
                        mUsersList.Add(user)
                    End If
                Next
            End Sub

            Public Function GetWeightsSum() As Double
                Return UsersList.Sum(Function(u) (u.Weight))
            End Function

            Public Property UsersList() As List(Of clsUser) 'C0672
                Get
                    Select Case CombinedUserID
                        Case COMBINED_USER_ID
                            mUsersList.Clear()
                            mUsersList.AddRange(mPrjManager.UsersList)
                            Name = DEFAULT_COMBINED_GROUP_NAME
                    End Select
                    Return mUsersList
                End Get
                Set(ByVal value As List(Of clsUser))
                    mUsersList = value
                End Set
            End Property

            Property CombinedUserID() As Integer

            Public Overloads Function ContainsUser(ByVal User As clsUser) As Boolean
                If mUsersList Is Nothing Then Return False
                Return mUsersList.Exists(Function(u) (u.UserEMail.ToLower = User.UserEMail.ToLower))
            End Function

            Public Overloads Function ContainsUser(ByVal UserID As Integer) As Boolean
                If mUsersList Is Nothing Then Return False
                Return mUsersList.Exists(Function(u) (u.UserID = UserID))
            End Function

            Public Function GetUserByEmail(ByVal Email As String) As clsUser
                If mUsersList Is Nothing Then Return Nothing
                Return mUsersList.FirstOrDefault(Function(u) (u.UserEMail = Email))
            End Function

            Public Sub New(ByVal ProjectManager As clsProjectManager)
                mUsersList = New List(Of clsUser)
                mPrjManager = ProjectManager
            End Sub
        End Class

        <Serializable()> Public Class clsGroups
            Public Property GroupsList() As New List(Of clsGroup)

            Public Function GetGroupByID(ByVal GroupID As Integer) As clsGroup
                Return GroupsList.FirstOrDefault(Function(g) (g.ID = GroupID))
            End Function

            Public Function GetGroupByCombinedID(ByVal cID As Integer) As clsCombinedGroup
                Return GroupsList.FirstOrDefault(Function(g) (CType(g, clsCombinedGroup).CombinedUserID = cID))
            End Function
            
            Public Overridable Overloads Function AddGroup(Optional ByVal GroupName As String = "") As clsGroup
                Dim newGroup As New clsGroup

                Dim newID As Integer = GroupsList.Select(Function(g) (g.ID)).DefaultIfEmpty(0).Max
                newGroup.ID = newID
                newGroup.Name = If(GroupName = "", "New Group " + newGroup.ID.ToString, GroupName)

                Return newGroup
            End Function

            Public Overridable Overloads Function AddGroup(ByVal Group As clsGroup) As Boolean
                If Group Is Nothing Then
                    Return False
                Else
                    If Not GroupsList.Contains(Group) Then GroupsList.Add(Group)
                    Return True
                End If
            End Function

            Public Overridable Overloads Function DeleteGroup(ByVal GroupID As Integer) As Boolean
                Return GroupsList.RemoveAll(Function(g) (g.ID = GroupID)) > 0
            End Function

            Public Overridable Overloads Function DeleteGroup(ByVal Group As clsGroup) As Boolean
                If Group Is Nothing Then
                    Return False
                Else
                    GroupsList.Remove(Group)
                    Return True
                End If
            End Function

            Public Function GroupExists(ByVal GroupID As Integer) As Boolean
                Return GroupsList.Exists(Function(g) (g.ID = GroupID))
            End Function

            Public Function CombinedGroupExists(ByVal CombinedUserID As Integer) As Boolean
                Return GroupsList.Exists(Function(g) (CType(g, clsCombinedGroup).CombinedUserID = CombinedUserID))
            End Function
        End Class

        <Serializable()> Public Class clsEvaluationGroups 'C0152
            Inherits clsGroups

            Private mPrjManager As clsProjectManager

            Public ReadOnly Property ProjectManager() As clsProjectManager 'C0148
                Get
                    Return mPrjManager
                End Get
            End Property

            Public Overridable Overloads Function DeleteGroup(ByVal GroupID As Integer) As Boolean 'C0152
                For Each U As clsUser In ProjectManager.UsersList
                    If U.EvaluationGroup IsNot Nothing Then
                        If U.EvaluationGroup.ID = GroupID Then
                            U.EvaluationGroup = Nothing
                        End If
                    End If
                Next

                Return MyBase.DeleteGroup(GroupID)

                'For i As Integer = GroupsList.Count - 1 To 0 Step -1
                '    If CType(GroupsList(i), clsGroup).ID = GroupID Then
                '        GroupsList.RemoveAt(i)
                '        Return True
                '    End If
                'Next
                'Return False
            End Function

            Public Overridable Overloads Function DeleteGroup(ByVal Group As clsGroup) As Boolean 'C0152
                If Group Is Nothing Then
                    Return False
                Else
                    For Each U As clsUser In ProjectManager.UsersList
                        If U.EvaluationGroup Is Group Then
                            U.EvaluationGroup = Nothing
                        End If
                    Next

                    Return MyBase.DeleteGroup(Group)
                    'GroupsList.Remove(Group)
                    'Return True
                End If
            End Function

            Public Sub New(ByVal ProjectManager As clsProjectManager)
                mPrjManager = ProjectManager
            End Sub
        End Class

        <Serializable()> Public Class clsCombinedGroups 'C0159
            Inherits clsGroups

            Private mPrjManager As clsProjectManager 'C0163

            Public Sub ApplyRules()
                UpdateDynamicGroups()
                SetRulesParsed(True)
            End Sub

            Public Sub SetRulesParsed(Value As Boolean)
                For Each group As clsCombinedGroup In GroupsList
                    If group.CombinedUserID <> COMBINED_USER_ID Then
                        group.RulesParsed = Value
                    End If
                Next
            End Sub

            Public Sub UpdateDynamicGroups()
                For Each CG As clsCombinedGroup In GroupsList
                    CG.ApplyRules()
                Next
            End Sub

            Public Overrides Function DeleteGroup(Group As clsGroup) As Boolean
                If Group Is Nothing Then Return False
                If CType(Group, clsCombinedGroup).CombinedUserID = COMBINED_USER_ID Then Return False

                mPrjManager.UsersRoles.CleanUpUserRoles(CType(Group, clsCombinedGroup).CombinedUserID)
                mPrjManager.StorageManager.Writer.SaveGroupPermissions(Group)

                Return MyBase.DeleteGroup(Group)
            End Function

            Public Overrides Function DeleteGroup(GroupID As Integer) As Boolean
                Dim CG As clsCombinedGroup = GetGroupByID(GroupID)
                mPrjManager.UsersRoles.CleanUpUserRoles(CG.CombinedUserID)
                mPrjManager.StorageManager.Writer.SaveGroupPermissions(CG)

                Return MyBase.DeleteGroup(GroupID)
            End Function

            Protected Function GetNextCombinedUserID() As Integer
                Dim min As Integer = COMBINED_GROUPS_USERS_START_ID + 1 'C0666
                For Each CG As clsCombinedGroup In GroupsList
                    'If CG.CombinedUserID < min Then 'C0714
                    If (CG.CombinedUserID < min) And (CG.CombinedUserID <> Integer.MinValue) And (CG.CombinedUserID <> Integer.MinValue + 1) Then 'C0714
                        min = CG.CombinedUserID
                    End If
                Next
                Return min - 1 'C0666
            End Function

            Public Function AddCombinedGroup(Optional ByVal GroupName As String = "") As clsCombinedGroup 'C0163
                'Dim newGroup As New clsCombinedGroup 'C0163
                Dim newGroup As New clsCombinedGroup(mPrjManager) 'C0163

                Dim max As Integer = -1
                For Each group As clsGroup In GroupsList
                    If group.ID > max Then
                        max = group.ID
                    End If
                Next

                newGroup.ID = max + 1
                newGroup.Name = If(GroupName = "", "New Group " + newGroup.ID.ToString, GroupName)
                newGroup.CombinedUserID = GetNextCombinedUserID()
                GroupsList.Add(newGroup) 'CXXX
                Return newGroup
            End Function

            Public Function GetDefaultCombinedGroup() As clsCombinedGroup
                Dim CG As clsCombinedGroup
                For Each CG In GroupsList
                    If CG.CombinedUserID = COMBINED_USER_ID Then
                        Return CG
                    End If
                Next

                CG = AddCombinedGroup(DEFAULT_COMBINED_GROUP_NAME)
                CG.CombinedUserID = COMBINED_USER_ID
                AddGroup(CG)

                Return CG
            End Function

            Public Function GetCombinedGroupByUserID(ByVal UserID As Integer) As clsCombinedGroup 'C0551
                Return GetGroupByCombinedID(UserID)
            End Function

            Public Function CombinedGroupUserIDExists(ByVal CGUserID As Integer) As Boolean
                Return GroupsList.FirstOrDefault(Function(g) (CType(g, clsCombinedGroup).CombinedUserID = CGUserID)) IsNot Nothing
            End Function

            Public Sub New(ByVal ProjectManager As clsProjectManager) 'C0163
                mPrjManager = ProjectManager 'C0163
            End Sub
        End Class

    End Module
End Namespace
