Option Strict Off

Partial Class DownloadReportData
    Inherits clsComparionCorePage

#Const USE_FILE_FOR_DATA = False

    Const _FILE_EXT_CSV As String = ".csv"

    Public Sub New()
        MyBase.New(_PGID_REPORT_GET_DATA)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim fResult As Boolean = False
        'Dim sID As String = CheckVar("id", "")
        Dim tID As Integer = CheckVar("id", 0)
        Dim tUID As Integer = CheckVar("uid", COMBINED_USER_ID)
        If tID > 0 Then DownloadReport(tID, tUID)
    End Sub

    Private Function HowToGetPriorities(PM As clsProjectManager, UserID As Integer) As Boolean
        Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
        Dim CalcTarget As clsCalculationTarget = Nothing
        If IsCombinedUserID(UserID) Then
            Dim CG As clsCombinedGroup = PM.CombinedGroups.GetCombinedGroupByUserID(UserID)
            If CG IsNot Nothing Then
                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
            End If
        Else
            Dim user As clsUser
            user = PM.GetUserByID(UserID)
            If user IsNot Nothing Then
                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, user)
            End If
        End If

        If CalcTarget IsNot Nothing Then
            ' here you can set Distributive or Ideal mode
            Dim CalcMode As ECSynthesisMode = ECSynthesisMode.smDistributive
            ' here you can set whether to use reductions or not
            Dim UseReductions As Boolean = False

            PM.CalculationsManager.SynthesisMode = CalcMode
            PM.CalculationsManager.UseReductions = UseReductions

            PM.CalculationsManager.Calculate(CalcTarget, H.Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

            ' Priorities of objectives
            For Each node As clsNode In H.Nodes
                ' 'Debug.Print("Local priority (wrt to parent): " + node.LocalPriority(CalcTarget).ToString)
                ' 'Debug.Print("Local priority unnormalized (wrt to parent): " + node.LocalPriorityUnnormalized(CalcTarget).ToString)
                ' 'Debug.Print("Global priority: " + node.WRTGlobalPriority.ToString)
                ' 'Debug.Print("Global priority unnormalized: " + node.UnnormalizedPriority.ToString)
            Next

            ' Priorities of alternatives wrt covering objectives
            For Each node As clsNode In H.TerminalNodes
                For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                    ' 'Debug.Print("Local priority (wrt to covering objective) - distributive: " + node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smDistributive, PM.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID).ToString)
                    ' 'Debug.Print("Local priority (wrt to covering objective) - ideal: " + node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID).ToString)
                    ' 'Debug.Print("Local priority unnormalized (wrt to covering objective) - distributive: " + node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smDistributive, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID).ToString)
                    ' 'Debug.Print("Local priority unnormalized (wrt to covering objective) - ideal: " + node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID).ToString)
                Next
            Next

            ' Global priorities of alternatives
            For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                ' 'Debug.Print("Global priority: " + alt.WRTGlobalPriority.ToString)
                ' 'Debug.Print("Global priority unnormalized: " + alt.UnnormalizedPriority.ToString)
            Next
        End If

        Return True
    End Function

    Private Function GetCSVStringFormat(ColumnMaxIndex As Integer) As String
        Dim AResult As String = ""
        For i = 0 To ColumnMaxIndex
            If i = 0 Then
                AResult += """{" + i.ToString + "}"""
            Else
                AResult += ";""{" + i.ToString + "}"""
            End If
        Next
        Return AResult
    End Function

    Private Sub AddFieldValue(ByRef SB As StringBuilder, Value As Object, Optional AddNewLine As Boolean = False)
        If AddNewLine Then
            SB.AppendLine(String.Format("""{0}""", Value.ToString.Replace("""", """""")))
        Else
            SB.Append(String.Format("""{0}"";", Value.ToString.Replace("""", """""")))
        End If
    End Sub

    Private Function DownloadReport(tReportID As Integer, UserID As Integer) As Boolean
        Dim FResult As Boolean = True

#If USE_FILE_FOR_DATA Then
        Dim sFilename As String = File_CreateTempName()
#Else
        Dim sContent As String = ""
#End If
        Dim sFName As String = GetProjectFileName(App.ActiveProject.ProjectName, "ReportData", "", _FILE_EXT_CSV)
        Dim SB As New StringBuilder()
        Select Case tReportID
            Case 1
                sFName = GetProjectFileName(App.ActiveProject.ProjectName + "-Alternatives", "ReportData", "", _FILE_EXT_CSV)

                With App.ActiveProject.ProjectManager
                    SB.Append(String.Format(GetCSVStringFormat(4), "AltID", "Name", "Comment", "Priority", "Unnormalized Priority"))
                    For Each attr In .Attributes.GetAlternativesAttributes()
                        If attr.Name <> "Known Likelihood" Then SB.Append(";""" + attr.Name + """")
                    Next
                    SB.AppendLine()
                    Dim H As clsHierarchy = .Hierarchy(.ActiveHierarchy)
                    Dim CalcTarget As clsCalculationTarget = Nothing
                    If IsCombinedUserID(UserID) Then
                        Dim CG As clsCombinedGroup = .CombinedGroups.GetCombinedGroupByUserID(UserID)
                        If CG IsNot Nothing Then
                            CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
                        End If
                    Else
                        Dim user As clsUser
                        user = .GetUserByID(UserID)
                        If user IsNot Nothing Then
                            CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, user)
                        End If
                    End If

                    If CalcTarget IsNot Nothing Then
                        ' here you can set Distributive or Ideal mode
                        Dim CalcMode As ECSynthesisMode = ECSynthesisMode.smDistributive
                        ' here you can set whether to use reductions or not
                        Dim UseReductions As Boolean = False

                        .CalculationsManager.SynthesisMode = CalcMode
                        .CalculationsManager.UseReductions = UseReductions

                        .CalculationsManager.Calculate(CalcTarget, H.Nodes(0), .ActiveHierarchy, .ActiveAltsHierarchy)
                    End If
                    For Each A In .AltsHierarchy(.ActiveAltsHierarchy).Nodes
                        AddFieldValue(SB, A.NodeID)
                        AddFieldValue(SB, A.NodeName)
                        AddFieldValue(SB, A.Comment)
                        AddFieldValue(SB, A.WRTGlobalPriority)
                        AddFieldValue(SB, A.UnnormalizedPriority)
                        Dim AAltAttributes As New List(Of clsAttribute)
                        AAltAttributes = .Attributes.GetAlternativesAttributes()
                        For Each attr In AAltAttributes
                            If attr.Name <> "Known Likelihood" Then
                                Dim AVal As Object = .Attributes.GetAttributeValue(attr.ID, A.NodeGuidID)
                                If AVal IsNot Nothing Then
                                    AddFieldValue(SB, AVal)
                                Else
                                    AddFieldValue(SB, "")
                                End If
                            End If
                        Next
                        AddFieldValue(SB, "", True)
                    Next
                End With
            Case 2
                sFName = GetProjectFileName(App.ActiveProject.ProjectName + "-Objectives", "ReportData", "", _FILE_EXT_CSV)
                With App.ActiveProject.ProjectManager
                    SB.Append(String.Format(GetCSVStringFormat(7), "NodeID", "Parent ID", "Name", "Comment", "Local Priority", "Local Unnormalized Priority", "Global Priority", "Global Unnormalized Priority"))
                    For Each attr In .Attributes.GetNodeAttributes
                        SB.Append(";""" + attr.Name + """")
                    Next
                    SB.AppendLine()
                    Dim H As clsHierarchy = .Hierarchy(.ActiveHierarchy)
                    Dim CalcTarget As clsCalculationTarget = Nothing
                    If IsCombinedUserID(UserID) Then
                        Dim CG As clsCombinedGroup = .CombinedGroups.GetCombinedGroupByUserID(UserID)
                        If CG IsNot Nothing Then
                            CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
                        End If
                    Else
                        Dim user As clsUser
                        user = .GetUserByID(UserID)
                        If user IsNot Nothing Then
                            CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, user)
                        End If
                    End If

                    If CalcTarget IsNot Nothing Then
                        Dim CalcMode As ECSynthesisMode = ECSynthesisMode.smDistributive
                        Dim UseReductions As Boolean = False

                        .CalculationsManager.SynthesisMode = CalcMode
                        .CalculationsManager.UseReductions = UseReductions
                        .CalculationsManager.Calculate(CalcTarget, H.Nodes(0), .ActiveHierarchy, .ActiveAltsHierarchy)

                        For Each O In H.Nodes
                            SB.Append(String.Format(GetCSVStringFormat(7), O.NodeID, O.ParentNodeID, O.NodeName, O.Comment, O.LocalPriority(CalcTarget), O.LocalPriorityUnnormalized(CalcTarget), O.WRTGlobalPriority, O.UnnormalizedPriority))
                            Dim AObjAttributes As New List(Of clsAttribute)
                            AObjAttributes = .Attributes.GetNodeAttributes
                            If AObjAttributes.Count > 0 Then SB.Append(";")
                            For Each attr In AObjAttributes
                                Dim AVal As Object = .Attributes.GetAttributeValue(attr.ID, O.NodeGuidID)
                                If AVal IsNot Nothing Then
                                    AddFieldValue(SB, AVal)
                                Else
                                    AddFieldValue(SB, "")
                                End If
                            Next
                            AddFieldValue(SB, "", True)
                        Next
                    End If
                End With
            Case 3
                sFName = GetProjectFileName(App.ActiveProject.ProjectName + "-Participants", "ReportData", "", _FILE_EXT_CSV)
                With App.ActiveProject.ProjectManager
                    SB.Append(String.Format(GetCSVStringFormat(3), "UserID", "Name", "EMail", "Weight"))
                    For Each attr In .Attributes.GetUserAttributes
                        SB.Append(";""" + attr.Name + """")
                    Next
                    SB.AppendLine()
                    For Each aUser As clsUser In .UsersList()
                        SB.Append(String.Format(GetCSVStringFormat(3), aUser.UserID, aUser.UserName, aUser.UserEMail, aUser.Weight))
                        Dim AAltAttributes As New List(Of clsAttribute)
                        AAltAttributes = .Attributes.GetUserAttributes
                        If AAltAttributes.Count > 0 Then
                            SB.Append(";")
                        End If
                        For Each attr In AAltAttributes
                            Dim AVal As Object = .Attributes.GetAttributeValue(attr.ID, aUser.UserID)
                            If AVal IsNot Nothing Then
                                AddFieldValue(SB, AVal)
                            Else
                                AddFieldValue(SB, "")
                            End If
                        Next
                        AddFieldValue(SB, "", True)
                    Next
                End With
            Case 4
                sFName = GetProjectFileName(App.ActiveProject.ProjectName + "-Judgements", "ReportData", "", _FILE_EXT_CSV)
                Dim UserIDs As New ArrayList
                Dim Judgments As List(Of clsCustomMeasureData)
                With App.ActiveProject.ProjectManager
                    For Each U In .UsersList
                        .StorageManager.Reader.LoadUserJudgments(U)
                        UserIDs.Add(U.UserID)
                    Next
                    SB.AppendLine(String.Format(GetCSVStringFormat(12), "User ID", "User Name", "User EMail", "Parent Node ID", "Parent Node Name", "Node ID", "Node Name", "Measurement Type", "Rating", "Pairwise Judgement", "Input Value", "Priority Value", "Comment"))
                    For Each node As clsNode In .Hierarchy(.ActiveHierarchy).Nodes
                        Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)
                        If node.MeasureType = ECMeasureType.mtPairwise Then
                            For Each J As clsPairwiseMeasureData In Judgments
                                If Not J.IsUndefined Then
                                    If J.UserID >= 0 Then
                                        AddFieldValue(SB, J.UserID)
                                        AddFieldValue(SB, .GetUserByID(J.UserID).UserName)
                                        AddFieldValue(SB, .GetUserByID(J.UserID).UserEMail)
                                        AddFieldValue(SB, .Hierarchy(.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeID)
                                        AddFieldValue(SB, .Hierarchy(.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeName)
                                        AddFieldValue(SB, String.Format("{0}|{1}", J.FirstNodeID, J.SecondNodeID))
                                        If node.IsTerminalNode Then
                                            AddFieldValue(SB, .AltsHierarchy(.ActiveAltsHierarchy).GetNodeByID(J.FirstNodeID).NodeName + "|" + .AltsHierarchy(.ActiveAltsHierarchy).GetNodeByID(J.SecondNodeID).NodeName)
                                        Else
                                            AddFieldValue(SB, .Hierarchy(.ActiveHierarchy).GetNodeByID(J.FirstNodeID).NodeName + "|" + .Hierarchy(.ActiveHierarchy).GetNodeByID(J.SecondNodeID).NodeName)
                                        End If
                                        SB.Append("Pairwise;")
                                        SB.Append(";")
                                        AddFieldValue(SB, (J.Value * J.Advantage).ToString.Replace(",", "."))
                                        SB.Append(";")
                                        SB.Append(";")
                                        AddFieldValue(SB, J.Comment, True)
                                    End If
                                End If
                            Next
                        End If
                        If node.MeasureType = ECMeasureType.mtRatings Then
                            For Each J As clsRatingMeasureData In Judgments
                                If Not J.IsUndefined Then
                                    If J.UserID >= 0 Then
                                        AddFieldValue(SB, J.UserID)
                                        AddFieldValue(SB, .GetUserByID(J.UserID).UserName)
                                        AddFieldValue(SB, .GetUserByID(J.UserID).UserEMail)
                                        AddFieldValue(SB, .Hierarchy(.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeID)
                                        AddFieldValue(SB, .Hierarchy(.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeName)
                                        AddFieldValue(SB, J.NodeID)
                                        If node.IsTerminalNode Then
                                            AddFieldValue(SB, .AltsHierarchy(.ActiveAltsHierarchy).GetNodeByID(J.NodeID).NodeName)
                                        Else
                                            AddFieldValue(SB, .Hierarchy(.ActiveHierarchy).GetNodeByID(J.NodeID).NodeName)
                                        End If
                                        SB.Append("Ratings;")
                                        AddFieldValue(SB, J.Rating.Name)
                                        SB.Append(";")
                                        AddFieldValue(SB, J.Rating.Value.ToString.Replace(",", "."))
                                        AddFieldValue(SB, J.Rating.Value.ToString.Replace(",", "."))
                                        AddFieldValue(SB, J.Comment, True)
                                    End If
                                End If
                            Next
                        End If
                        If (node.MeasureType <> ECMeasureType.mtRatings) And (node.MeasureType <> ECMeasureType.mtPairwise) Then
                            For Each J As clsNonPairwiseMeasureData In Judgments
                                If Not J.IsUndefined Then
                                    If J.UserID >= 0 Then
                                        AddFieldValue(SB, J.UserID)
                                        AddFieldValue(SB, .GetUserByID(J.UserID).UserName)
                                        AddFieldValue(SB, .GetUserByID(J.UserID).UserEMail)
                                        AddFieldValue(SB, .Hierarchy(.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeID)
                                        AddFieldValue(SB, .Hierarchy(.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeName)
                                        AddFieldValue(SB, J.NodeID)
                                        If node.IsTerminalNode Then
                                            AddFieldValue(SB, .AltsHierarchy(.ActiveAltsHierarchy).GetNodeByID(J.NodeID).NodeName)
                                        Else
                                            AddFieldValue(SB, .Hierarchy(.ActiveHierarchy).GetNodeByID(J.NodeID).NodeName)
                                        End If
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtAdvancedUtilityCurve
                                                SB.Append("Advanced Utility Curve;")
                                            Case ECMeasureType.mtRegularUtilityCurve
                                                SB.Append("Regular Utility Curve;")
                                            Case ECMeasureType.mtStep
                                                SB.Append("Step Function;")
                                            Case ECMeasureType.mtDirect
                                                SB.Append("Direct;")
                                        End Select
                                        SB.Append(";")
                                        SB.Append(";")
                                        AddFieldValue(SB, CSng(J.ObjectValue).ToString.Replace(",", "."))
                                        AddFieldValue(SB, J.SingleValue.ToString.Replace(",", "."))
                                        AddFieldValue(SB, J.Comment, True)
                                    End If
                                End If
                            Next
                        End If
                    Next
                End With
            Case Else
                SB.Append("Incorrect report ID")
        End Select

#If USE_FILE_FOR_DATA Then
                MyComputer.FileSystem.WriteAllText(sFilename, tReportID.ToString, False)
                FResult = True
#Else
        sContent = SB.ToString
        FResult = True
#End If

        If FResult Then
            RawResponseStart()
            Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}""", HttpUtility.UrlEncode(SafeFileName(sFName))))   ' D3478 + D6591
            Response.ContentType = "application/octet-stream"

#If USE_FILE_FOR_DATA Then
            Dim tLen As Integer = MyComputer.FileSystem.GetFileInfo(sFilename).Length
#Else
            Dim tLen As Integer = sContent.Length
#End If

            Response.AddHeader("Content-Length", CStr(tLen))


#If USE_FILE_FOR_DATA Then
            Response.BinaryWrite(MyComputer.FileSystem.ReadAllBytes(sFilename))
            File_Erase(sFilename)
#Else
            Response.Write(sContent)
#End If

            App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProjectReport, App.ProjectID, "Get Report data '{0}'", String.Format("Filename: {0}; Size: {1}", sFName, tLen))
        End If

        If FResult Then RawResponseEnd()

        Return FResult
    End Function

End Class
