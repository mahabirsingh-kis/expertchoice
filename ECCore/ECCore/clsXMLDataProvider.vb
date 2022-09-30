Imports System.Xml
Imports System.IO
Imports ECCore

Namespace ECCore
    <Serializable()> Public Class clsXMLDataProvider 'C0430
        Private Sub WriteNode(ByVal writer As XmlTextWriter, ByVal node As clsNode)
            If node Is Nothing Then
                Exit Sub
            End If

            writer.WriteStartElement("Node")
            writer.WriteAttributeString("Name", node.NodeName)
            writer.WriteAttributeString("ID", node.NodeID.ToString)

            For Each child As clsNode In node.Children
                WriteNode(writer, child)
            Next

            writer.WriteEndElement() 'Node
        End Sub

        Private Sub WriteHierarchyToDataGrid1XML(ByVal writer As XmlTextWriter, ByVal ProjectManager As clsProjectManager)
            writer.WriteStartElement("Hierarchy")
            WriteNode(writer, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0))
            writer.WriteEndElement() 'Hierarchy
        End Sub

        Private Sub WriteRatingScalesToDataGrid1XML(ByVal writer As XmlTextWriter, ByVal ProjectManager As clsProjectManager)
            writer.WriteStartElement("RatingScales")
            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                RS.Sort()

                writer.WriteStartElement("RatingScale")
                writer.WriteAttributeString("Name", RS.Name)
                writer.WriteAttributeString("ID", RS.ID.ToString)

                For Each RI As clsRating In RS.RatingSet
                    writer.WriteStartElement("Intensity")
                    writer.WriteAttributeString("Name", RI.Name)
                    writer.WriteAttributeString("ID", RI.ID.ToString) 'C0451
                    writer.WriteAttributeString("Value", RI.Value.ToString)
                    writer.WriteEndElement()
                Next

                writer.WriteEndElement() 'RatingScale
            Next
            writer.WriteEndElement() 'RatingScales
        End Sub

        Private Sub WriteCoveringObjectivesToDataGrid1XML(ByVal writer As XmlTextWriter, ByVal ProjectManager As clsProjectManager, ByVal UserID As Integer)
            writer.WriteStartElement("CoveringObjectives")

            For Each CO As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes
                writer.WriteStartElement("CovObj")
                writer.WriteAttributeString("ID", CO.NodeID.ToString)
                'If CO.UserPermissions(UserID).Write Then 'C0901
                If CO.IsAllowed(UserID) Then 'C0901
                    writer.WriteAttributeString("MT", CInt(CO.MeasureType).ToString)
                    'C0455===
                    If CO.MeasureType = ECMeasureType.mtRatings Then
                        writer.WriteAttributeString("ScaleID", CO.MeasurementScaleID)
                    End If
                    'C0455==
                End If
                writer.WriteEndElement() 'CovObj
            Next

            writer.WriteEndElement() 'CoveringObjectives
        End Sub

        Private Sub WriteAlternativesToDataGrid1XML(ByVal writer As XmlTextWriter, ByVal ProjectManager As clsProjectManager)
            writer.WriteStartElement("Alternatives")

            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                writer.WriteStartElement("Alt")
                writer.WriteAttributeString("Name", alt.NodeName)
                writer.WriteAttributeString("ID", alt.NodeID.ToString)
                writer.WriteEndElement() 'Alt
            Next

            writer.WriteEndElement() 'Alternatives
        End Sub

        Private Sub WriteDataGrid1(ByVal writer As XmlTextWriter, ByVal ProjectManager As clsProjectManager, ByVal UserID As Integer)
            writer.WriteStartElement("Cells")

            'Dim altsBelow As ArrayList 'C0450
            Dim altsBelow As List(Of clsNode) 'C0450
            For Each CO As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes
                'If CO.UserPermissions(UserID).Write Then 'C0901
                If CO.IsAllowed(UserID) Then 'C0901
                    If CO.MeasureType <> ECMeasureType.mtPairwise Then
                        'C0481===
                        Dim user As clsUser = ProjectManager.GetUserByID(UserID)
                        If user Is Nothing Then
                            user = ProjectManager.GetDataInstanceUserByID(UserID)
                        End If
                        'C0481==
                        'ProjectManager.AddEmptyMissingJudgments(ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy, _
                        '                                        ProjectManager.GetUserByID(UserID), CO.NodeID) 'C0481
                        ProjectManager.AddEmptyMissingJudgments(ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy, _
                                                                user, CO.NodeID) 'C0481
                        'altsBelow = GetAllowedNodesBelow(CO, UserID) 'C0450
                        altsBelow = CO.GetNodesBelow(UserID) 'C0450
                        For Each J As clsNonPairwiseMeasureData In CO.Judgments.JudgmentsFromUser(UserID)
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(J.NodeID)
                            If altsBelow.Contains(alt) Then
                                writer.WriteStartElement("Cell")
                                writer.WriteAttributeString("ObjID", CO.NodeID.ToString)
                                writer.WriteAttributeString("AltID", alt.NodeID.ToString)
                                If J.IsUndefined Then
                                    writer.WriteAttributeString("Value", "")
                                Else
                                    If CO.MeasureType = ECMeasureType.mtRatings Then
                                        writer.WriteAttributeString("Value", CType(J, clsRatingMeasureData).Rating.ID.ToString)
                                    Else
                                        writer.WriteAttributeString("Value", CSng(J.ObjectValue))
                                    End If
                                End If
                                writer.WriteEndElement() 'Cell
                            End If
                        Next
                    End If
                End If
            Next

            writer.WriteEndElement() 'Cells
        End Sub

        Public Function GetDataGrid1XML(ByVal ProjectManager As clsProjectManager, ByVal UserID As Integer) As String
            Dim writer As XmlTextWriter = Nothing

            Dim SW As New StringWriter

            writer = New XmlTextWriter(SW)
            writer.Formatting = Formatting.Indented
            writer.WriteStartDocument()

            writer.WriteStartElement("DataGrid")

            WriteHierarchyToDataGrid1XML(writer, ProjectManager)
            WriteRatingScalesToDataGrid1XML(writer, ProjectManager)
            WriteCoveringObjectivesToDataGrid1XML(writer, ProjectManager, UserID)
            WriteAlternativesToDataGrid1XML(writer, ProjectManager)
            WriteDataGrid1(writer, ProjectManager, UserID)

            writer.WriteEndElement() 'DataGrid

            writer.Close()

            Return SW.ToString
        End Function

        Public Function GetUsersList(ByVal ProjectManager As clsProjectManager) As String 'C0470
            Dim writer As XmlTextWriter = Nothing

            Dim SW As New StringWriter

            writer = New XmlTextWriter(SW)
            writer.Formatting = Formatting.Indented
            writer.WriteStartDocument()

            writer.WriteStartElement("UsersList")

            'C0471===
            Dim uList As New List(Of clsUser)
            For Each user As clsUser In ProjectManager.UsersList
                uList.Add(user)
            Next
            For Each user As clsUser In ProjectManager.DataInstanceUsers
                uList.Add(user)
            Next
            'C0471==


            'For Each user As clsUser In ProjectManager.UsersList 'C0471
            For Each user As clsUser In uList 'C0471
                'If user.UserID >= 0 Then 'C0481
                If (user.UserID >= 0) Or (user.UserID < -1) And (user.UserID > COMBINED_GROUPS_USERS_START_ID) Then 'C0481
                    writer.WriteStartElement("User")
                    writer.WriteAttributeString("Name", user.UserName)
                    writer.WriteAttributeString("EMail", user.UserEMail)
                    writer.WriteEndElement() 'User
                End If
            Next

            writer.WriteEndElement() 'UsersList

            writer.Close()

            Return SW.ToString
        End Function

        Public Function ParseDataGridUpdates(ByVal DataGridUpdatesXML As String, ByVal ProjectManager As clsProjectManager) As Boolean 'C0471
            Dim reader As XmlTextReader = Nothing
            Try
                Dim SR As New StringReader(DataGridUpdatesXML)

                reader = New XmlTextReader(SR)
                reader.WhitespaceHandling = WhitespaceHandling.None

                'reader.Read() 'C0476
                reader.ReadToFollowing("DataGridUpdates") 'C0476
                If (reader.NodeType = XmlNodeType.Element) And (reader.Name = "DataGridUpdates") Then
                    Dim userEmail As String = reader.GetAttribute("UserEMail") 'C0476
                    Dim user As clsUser = ProjectManager.GetUserByEMail(userEmail)
                    ProjectManager.StorageManager.Reader.LoadUserJudgments(user)

                    reader.Read()
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = "Cells") Then
                        While reader.Read()
                            If (reader.NodeType = XmlNodeType.Element) And (reader.Name = "Cell") Then
                                Dim sObjID As String = reader.GetAttribute("ObjID")
                                Dim CovObjID As Integer
                                If Integer.TryParse(sObjID, CovObjID) Then
                                    Dim node As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(CovObjID)
                                    If node IsNot Nothing AndAlso node.IsTerminalNode AndAlso node.MeasureType <> ECMeasureType.mtPairwise Then
                                        Dim sAltID As String = reader.GetAttribute("AltID")
                                        Dim AltID As Integer
                                        If Integer.TryParse(sAltID, AltID) Then
                                            Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(AltID)
                                            If alt IsNot Nothing Then
                                                'C0480===
                                                Dim sValue As String = reader.GetAttribute("Value")
                                                If sValue = "" Then
                                                    Dim nonpwData As clsNonPairwiseMeasureData = Nothing
                                                    Select Case node.MeasureType
                                                        Case ECMeasureType.mtRatings
                                                            nonpwData = New clsRatingMeasureData(AltID, node.NodeID, user.UserID, Nothing, node.MeasurementScale, True)
                                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve 'C0026
                                                            nonpwData = New clsUtilityCurveMeasureData(AltID, node.NodeID, user.UserID, Single.NaN, node.MeasurementScale, True)
                                                        Case ECMeasureType.mtStep
                                                            nonpwData = New clsStepMeasureData(AltID, node.NodeID, user.UserID, Single.NaN, node.MeasurementScale, True) 'C0489 (changed Nothing to Single.NaN)
                                                        Case ECMeasureType.mtDirect
                                                            nonpwData = New clsDirectMeasureData(AltID, node.NodeID, user.UserID, Single.NaN, True)
                                                    End Select
                                                    If nonpwData IsNot Nothing Then
                                                        node.Judgments.AddMeasureData(nonpwData)
                                                        ProjectManager.StorageManager.Writer.SaveUserJudgments(user)
                                                    End If
                                                Else
                                                    If node.MeasureType = ECMeasureType.mtRatings Then
                                                        'Dim sRatingID As String = reader.GetAttribute("Value") 'C0480
                                                        Dim sRatingID As String = sValue 'C0480
                                                        Dim RatingID As Integer

                                                        If Integer.TryParse(sRatingID, RatingID) Then
                                                            If node.MeasurementScale IsNot Nothing Then
                                                                Dim RatingScale As clsRatingScale = CType(node.MeasurementScale, clsRatingScale)
                                                                Dim R As clsRating = RatingScale.GetRatingByID(RatingID)
                                                                If R IsNot Nothing Then
                                                                    Dim RD As clsRatingMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, user.UserID)
                                                                    'C0489===
                                                                    'RD.Rating = R
                                                                    'RD.IsUndefined = False 'C0480
                                                                    'C0489==

                                                                    'C0489===
                                                                    If RD IsNot Nothing Then
                                                                        RD.Rating = R
                                                                        RD.IsUndefined = False
                                                                    Else
                                                                        RD = New clsRatingMeasureData(alt.NodeID, node.NodeID, user.UserID, R, node.MeasurementScale, False)
                                                                        node.Judgments.AddMeasureData(RD)
                                                                    End If
                                                                    'C0489==
                                                                    ProjectManager.StorageManager.Writer.SaveUserJudgments(user)
                                                                End If
                                                            End If
                                                        End If
                                                    Else
                                                        sValue = MiscFuncs.FixStringWithSingleValue(sValue) 'C0480
                                                        'Dim sSingleValue As String = reader("Value") 'C0480
                                                        Dim sSingleValue As String = sValue 'C0480
                                                        Dim SingleValue As Single

                                                        If Single.TryParse(sSingleValue, SingleValue) Then
                                                            Dim nonPWData As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, user.UserID)
                                                            'C0489===
                                                            'nonPWData.ObjectValue = SingleValue
                                                            'nonPWData.IsUndefined = False 'C0480
                                                            'C0489==

                                                            'C0489===
                                                            If nonPWData IsNot Nothing Then
                                                                nonPWData.ObjectValue = SingleValue
                                                                nonPWData.IsUndefined = False
                                                            Else
                                                                Select Case node.MeasureType
                                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                                        nonPWData = New clsUtilityCurveMeasureData(alt.NodeID, node.NodeID, user.UserID, SingleValue, node.MeasurementScale, False)
                                                                    Case ECMeasureType.mtStep
                                                                        nonPWData = New clsStepMeasureData(alt.NodeID, node.NodeID, user.UserID, SingleValue, node.MeasurementScale, False)
                                                                    Case ECMeasureType.mtDirect
                                                                        nonPWData = New clsDirectMeasureData(alt.NodeID, node.NodeID, user.UserID, SingleValue, False)
                                                                End Select
                                                                node.Judgments.AddMeasureData(nonPWData)
                                                            End If
                                                            'C0489==
                                                            ProjectManager.StorageManager.Writer.SaveUserJudgments(user)
                                                        End If
                                                    End If
                                                End If
                                            'C0480==
                                        End If
                                        End If
                                    End If
                                End If
                            End If
                        End While
                    End If
                End If
                'TODO: Save user judgments only once
            Finally
                If Not (reader Is Nothing) Then
                    reader.Close()
                End If
            End Try

            Return True
        End Function

        ' D3878 ===
        Public Function GetProjectXML(ByVal ProjectManager As clsProjectManager, SaveStructureOnly As Boolean) As String
            Dim sXML As String = ""
            ' Put code here
            Return sXML
        End Function

        ' D3878 ==

    End Class

End Namespace
