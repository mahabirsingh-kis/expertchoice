' utilities for web pages by DA

Imports System.Linq
Imports Canvas
Imports ECCore
Imports ExpertChoice.Service

Namespace ExpertChoice.Web

    Public Module WebPageUtils

        Public Sub WriteSetting(tPrj As clsProject, ID As Guid, valueType As AttributeValueTypes, value As Object, ObjectID As Guid, Optional sLogMsg As String = "", Optional sSnapshotComment As String = "")  'A1296
            tPrj.ProjectManager.Attributes.SetAttributeValue(ID, UNDEFINED_USER_ID, valueType, value, ObjectID, Guid.Empty)
            WriteAttributeValues(tPrj, sLogMsg, sSnapshotComment)
        End Sub

        Public Sub WriteSetting(tPrj As clsProject, ID As Guid, valueType As AttributeValueTypes, value As Object, Optional sLogMsg As String = "", Optional sSnapshotComment As String = "", Optional SkipSaving As Boolean = False)  ' D3731
            tPrj.ProjectManager.Attributes.SetAttributeValue(ID, UNDEFINED_USER_ID, valueType, value, Guid.Empty, Guid.Empty)
            If Not SkipSaving Then WriteAttributeValues(tPrj, sLogMsg, sSnapshotComment)   ' D3731
        End Sub

        Public Sub WriteAttributeValues(tPrj As clsProject, sLogMsg As String, sSnapshotComment As String)  ' D3731
            With tPrj.ProjectManager
                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            End With
            If Not String.IsNullOrEmpty(sLogMsg) Then tPrj.onProjectSaved.Invoke(tPrj, sLogMsg, False, sSnapshotComment) ' D3731
        End Sub

        Public Sub WriteControls(tPrj As clsProject, Optional sLogMsg As String = "", Optional sSnapshotComment As String = "")  ' D3731
            With tPrj.ProjectManager
                .Controls.WriteControls(ECModelStorageType.mstCanvasStreamDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
            End With
            If Not String.IsNullOrEmpty(sLogMsg) Then tPrj.onProjectSaved.Invoke(tPrj, sLogMsg, False, sSnapshotComment) ' D3731
        End Sub

        Public Sub WriteAttributes(tPrj As clsProject, Optional sLogMsg As String = "", Optional sSnapshotComment As String = "")  ' D3731
            With tPrj.ProjectManager
                .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
            End With
            If Not String.IsNullOrEmpty(sLogMsg) Then tPrj.onProjectSaved.Invoke(tPrj, sLogMsg, False, sSnapshotComment) ' D3731
        End Sub

        Public Function LongToBrush(ByVal intColor As Long) As String
            Dim result As String = "#" + intColor.ToString("X8").Substring(2)
            Return result
        End Function

        Public Function BrushToLong(tBrush As String) As Long
            Dim result As Long = 0
            Long.TryParse(tBrush, System.Globalization.NumberStyles.HexNumber, Globalization.CultureInfo.CurrentCulture, result)
            Return result
        End Function

#Region "CS common functions"

        'Public Function BrushToIntColor(hexColor As String) As Integer
        '    Dim retVal As Integer
        '    Int32.TryParse(hexColor, System.Globalization.NumberStyles.HexNumber, Globalization.CultureInfo.CurrentCulture, retVal)
        '    Return retVal
        'End Function

        Public Function HexStringToInt(ByVal s As String) As Integer
            s = s.Trim
            If s <> "" AndAlso s(0) = "#" Then s = s.Substring(1)

            Dim i As Integer = 0
            Int32.TryParse(s, System.Globalization.NumberStyles.HexNumber, Globalization.CultureInfo.CurrentCulture, i)
            Return i
        End Function

        Public Function HashByString(s As String) As Integer
            Dim a As Integer = 0
            s = s.Trim.ToLower()
            For i As Integer = 0 To If(s.Length < 100, s.Length, 100) - 1
                Dim b As Char = s(i)
                'a = CInt(Math.Abs((((a << 5) - a) + Convert.ToByte(b))))
                'a = a And a
                a += Convert.ToByte(b)
            Next
            Return Math.Abs(a)
        End Function

#End Region

        ' D4120 ===
        Enum ecRAGridPages
            NoPages = 0
            Page10 = 10
            Page15 = 15
            Page20 = 20
            Page50 = 50
            Page100 = 100
        End Enum
        ' D4120 ==

        Public Class clsTemplateItem
            Public Property Title As String = ""
            Public Property TemplateName As String = ""
            Public Property PluralTemplateName As String = ""
            Public Property Singular As String = ""
            Public Property Plural As String = ""
            Public Property IsHeader As Boolean = False
        End Class

        ' -D3954
        'Public Class clsUserEvaluationProgress

        '    Private Property _Email As String
        '    Public Property Email As String
        '        Get
        '            Return _Email
        '        End Get
        '        Set(value As String)
        '            _Email = value
        '        End Set
        '    End Property

        '    Private _EvaluatedCount As Integer
        '    Public Property EvaluatedCount As Integer
        '        Get
        '            Return _EvaluatedCount
        '        End Get
        '        Set(value As Integer)
        '            _EvaluatedCount = value
        '        End Set
        '    End Property

        '    Private _LastJudgmentTime As Date?
        '    Public Property LastJudgmentTime As Date?
        '        Get
        '            Return _LastJudgmentTime
        '        End Get
        '        Set(value As Date?)
        '            _LastJudgmentTime = value
        '        End Set
        '    End Property

        '    Private _LastJudgmentTimeUTC As String
        '    Public Property LastJudgmentTimeUTC As String
        '        Get
        '            Return _LastJudgmentTimeUTC
        '        End Get
        '        Set(value As String)
        '            _LastJudgmentTimeUTC = value
        '        End Set
        '    End Property

        '    Private _TotalCount As Integer
        '    Public Property TotalCount As Integer
        '        Get
        '            Return _TotalCount
        '        End Get
        '        Set(value As Integer)
        '            _TotalCount = value
        '        End Set
        '    End Property

        'End Class

        ' D3871 + A1217 ==
        Private Sub GetNodesBelowForPipe(ByVal tPrj As clsProject, ByVal node As ECCore.clsNode, tUsers As List(Of Integer), ByRef tSubNodes As List(Of ECCore.clsNode))
            Dim res As New List(Of ECCore.clsNode)
            If Not node.IsTerminalNode Then
                res = node.GetNodesBelow(UNDEFINED_USER_ID)
            Else
                Dim allAlts As List(Of ECCore.clsNode) = node.GetNodesBelow(UNDEFINED_USER_ID)
                For Each alt As ECCore.clsNode In allAlts
                    Dim CanEval As Boolean = False
                    Dim j As Integer = 0
                    While Not CanEval And (j < tUsers.Count)
                        CanEval = Not node.DisabledForUser(tUsers(j)) And tPrj.ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, tUsers(j))
                        j += 1
                    End While
                    If CanEval Then
                        res.Add(alt)
                    End If
                Next
            End If

            ' comment due to issue when pipe has categories as well (FB19327)
            ' -D6787 ===
            'If Not node.IsTerminalNode And tPrj.ProjectManager.IsRiskProject AndAlso node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
            '    For i As Integer = res.Count - 1 To 0 Step -1
            '        If res(i).RiskNodeType = RiskNodeType.ntCategory Then
            '            res.RemoveAt(i)
            '        End If
            '    Next
            'End If
            ' -D6787 ==

            For Each tNode As ECCore.clsNode In res
                tSubNodes.Add(tNode)
                If Not node.IsTerminalNode Then GetNodesBelowForPipe(tPrj, tNode, tUsers, tSubNodes)
            Next

        End Sub
        ' D3871 ==

        ' D3689 + A1217 ===
        Public Function CheckUserRolesAndEnabledItems(ByVal tPrj As clsProject, ByVal ActiveUserEmail As String) As Integer ' D3704
            Dim UsersList As New List(Of String)
            For Each user As clsUser In tPrj.ProjectManager.UsersList
                If user.UserEMail.ToLower = ActiveUserEmail.ToLower OrElse user.SyncEvaluationMode <> ECTypes.SynchronousEvaluationMode.semNone Then  ' D3832
                    UsersList.Add(user.UserEMail)
                End If
            Next

            Dim fHasObjs As Boolean = False
            Dim fHasAlts As Boolean = False

            Dim tUsers As New List(Of Integer)
            For Each sEmail As String In UsersList
                Dim tUser As ECCore.clsUser = tPrj.ProjectManager.GetUserByEMail(sEmail)
                If tUser IsNot Nothing Then tUsers.Add(tUser.UserID)
            Next

            If tUsers.Count > 0 Then

                Dim tObjs As New List(Of ECCore.clsNode)
                If tPrj.HierarchyObjectives.Nodes.Count > 1 Then GetNodesBelowForPipe(tPrj, tPrj.HierarchyObjectives.Nodes(0), tUsers, tObjs)
                fHasObjs = tObjs.Count > 0

                Dim tAlts As New List(Of ECCore.clsNode)
                For Each tNode As ECCore.clsNode In tPrj.HierarchyObjectives.TerminalNodes
                    GetNodesBelowForPipe(tPrj, tNode, tUsers, tAlts)
                    If tAlts.Count > 0 Then
                        fHasAlts = True
                        Exit For
                    End If
                Next

            End If

            Return CInt(IIf(fHasObjs OrElse tPrj.HierarchyObjectives.Nodes.Count = 1, 1, 0)) + CInt(IIf(fHasAlts OrElse tPrj.HierarchyAlternatives.Nodes.Count = 0 OrElse Not fHasObjs, 2, 0)) ' D3832 + D4017
        End Function

        Public Function HowControlsSelected(tPrj As clsProject, ParseString As Func(Of String, String), Optional ShowFullInfo As Boolean = False) As String
            If Not tPrj.isMyRiskRewardModel Then
                Dim sCostOfControls As String = CostString(tPrj.ProjectManager.Controls.CostOfFundedControls(), 0, True) ' Total cost of all active controls 'A1303
                Dim sNumOfControls As String = CStr(tPrj.ProjectManager.Controls.EnabledControls.Sum(Function(ctrl) If(ctrl.Active, 1, 0)))
                Dim sHowSelected As String = "Manually selected"
                'If PM.Parameters.Riskion_ControlsActualSelectionMode = 1 And PM.Parameters.Riskion_Use_Simulated_Values = 1 Then sHowSelected = "Optimized based on simulated input and computed output with budget of " + CostString(PM.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, DecimalDigits, True)
                'If PM.Parameters.Riskion_ControlsActualSelectionMode = 1 And PM.Parameters.Riskion_Use_Simulated_Values = 2 Then sHowSelected = "Optimized based on computed input and simulated output with budget of " + CostString(PM.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, DecimalDigits, True)
                'If PM.Parameters.Riskion_ControlsActualSelectionMode = 1 And PM.Parameters.Riskion_Use_Simulated_Values = 3 Then sHowSelected = "Optimized based on simulated input and output with budget of " + CostString(PM.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, DecimalDigits, True)
                If tPrj.ProjectManager.Parameters.Riskion_ControlsActualSelectionMode = 1 Then sHowSelected = "Optimized with budget of " + CostString(tPrj.ProjectManager.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, 1, True)
                If ShowFullInfo Then
                    Dim tbl As String = String.Format("<table class='text'><thead><tr><td class='th_detail'>{0}</td><td class='th_detail'>{1}</td><td class='th_detail'>{2}</td></tr></thead>", ParseString("# %%Controls%%"), ParseString("Cost of %%Controls%%"), "How Selected")
                    tbl += String.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", sNumOfControls, sCostOfControls, sHowSelected)
                    Return String.Format(" ({0})", tbl + "</table>")
                Else
                    Return String.Format(" {0}", sHowSelected)
                End If
            End If
            Return ""
        End Function


    End Module

End Namespace
