Imports ExpertChoice.Service
Imports Canvas
Imports ECCore

Namespace ExpertChoice.Data

    Public Class clsSensitivityAnalysisDataParser

        Private Const _OBJ_NAME As String = "obj_name"
        Private Const _OBJ_VALUE As String = "obj_value"
        Private Const _OBJ_IDX_NAME As String = "obj{0}_name"
        Private Const _OBJ_IDX_VALUE As String = "obj{0}_value"
        Private Const _OBJ_IDX_ID As String = "obj{0}_id"
        Private Const _ALT_IDX_NAME As String = "alt{0}_name"
        Private Const _ALT_IDX_VALUE As String = "alt{0}_value"
        Private Const _ALT_IDX_VALUE0 As String = "alt{0}_value0"
        Private Const _ALT_IDX_VALUE1 As String = "alt{0}_value1"
        Private Const _ALT_IDX_ID As String = "alt{0}_id"
        Private Const _MATRIX_XY As String = "m{0}_{1}"
        Private Const _ALT_MAX_VALUE As String = "MaxAltValue"
        Private Const _REFRESH_INTERVAL As String = "SendInterval"

        Private _Project As clsProject
        Private _SAType As SAType   ' D0182
        Private _AltsList As List(Of clsNode) = Nothing    ' D0338 'C0384
        Private _GetMaxAltValue As Boolean = False   ' D0368

        Public Const Option_IgnoreCategories As Boolean = True ' D2686

        Public NormalizationOption As AlternativeNormalizationOptions = AlternativeNormalizationOptions.anoPercentOfMax ' D2115

        Public Const _SESS_SA_NORMALIZATION As String = "SANormMode"     ' D2114

        Public Sub New(Optional ByVal tSAType As SAType = Canvas.SAType.satNone, Optional ByVal tProject As clsProject = Nothing, Optional tNormMode As AlternativeNormalizationOptions = AlternativeNormalizationOptions.anoPercentOfMax)   ' D0182 + D2115
            _SAType = tSAType
            _Project = tProject
            NormalizationOption = tNormMode ' D2115
            'TODO: COMBINED GROUPS
        End Sub

        Public Property Project() As clsProject
            Get
                Return _Project
            End Get
            Set(ByVal value As clsProject)
                _Project = value
            End Set
        End Property

        Public Property SAType() As SAType
            Get
                Return _SAType
            End Get
            Set(ByVal value As SAType)
                _SAType = value
            End Set
        End Property

        ' D0338 ===
        Public Property AlternativesList() As List(Of clsNode) 'C0384
            Get
                If _AltsList Is Nothing Then
                    If Not Project Is Nothing Then _AltsList = Project.ProjectManager.DynamicSensitivity.Alternatives ' D1920
                End If
                Return _AltsList
            End Get
            Set(ByVal value As List(Of clsNode))
                _AltsList = value
            End Set
        End Property
        ' D0338 ==

        ' D0368 ===
        Public Property GetMaxAltValue() As Boolean
            Get
                Return _GetMaxAltValue
            End Get
            Set(ByVal value As Boolean)
                _GetMaxAltValue = value
            End Set
        End Property
        ' D0368 ==

        Public Overridable Function GetSAData(ByVal QuesryString As String) As String
            Dim args As Specialized.NameValueCollection = HttpUtility.ParseQueryString(URLDecode(QuesryString)) ' D0178
            Return GetSAData(args)
        End Function

        Public Overridable Function GetSAData(ByVal tParams As Specialized.NameValueCollection) As String
            Dim sContent As String = ""
            If Not Project Is Nothing Then
                Dim fIgnoreCategories As Boolean = Option_IgnoreCategories AndAlso Project.IsRisk       ' D2686
                Dim tSANode As clsNode = Project.ProjectManager.DynamicSensitivity.Node
                If Not tSANode Is Nothing Then

                    Dim idx As Integer = 0

                    Select Case SAType

                        ' D0160 ===
                        Case Canvas.SAType.satGradient
                            If sContent <> "" Then sContent += "&"
                            sContent += String.Format(_OBJ_NAME + "={0}&" + _OBJ_VALUE + "={1}", Uri.EscapeUriString(tSANode.NodeName), JS_SafeNumber(tSANode.LocalPriority(Project.ProjectManager.CalculationsManager.GetCalculationTargetByUserID(CInt(IIf(Project.ProjectManager.DynamicSensitivity.CalculateForCombined, Project.ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, Project.ProjectManager.UserID)))) * 100)) 'C0555
                            'TODO: COMBINED GROUPS
                            ' D0160 ==

                        Case Canvas.SAType.satPerformance, Canvas.SAType.satDynamic     ' D0160
                            idx = 0

                            For Each child As clsNode In Project.ProjectManager.DynamicSensitivity.Objectives
                                If Not fIgnoreCategories OrElse child.RiskNodeType <> RiskNodeType.ntCategory Then  ' D2686
                                    idx += 1
                                    If sContent <> "" Then sContent += "&"
                                    sContent += String.Format(_OBJ_IDX_NAME + "={1}&" + _OBJ_IDX_VALUE + "={2}&" + _OBJ_IDX_ID + "={3}", idx, Uri.EscapeUriString(child.NodeName), Uri.EscapeUriString(JS_SafeNumber(child.SALocalPriority * 100)), child.NodeID)
                                End If
                            Next

                            Dim sID As String
                            For i As Integer = 1 To Project.ProjectManager.DynamicSensitivity.Objectives.Count
                                If Not fIgnoreCategories OrElse Project.ProjectManager.DynamicSensitivity.Objectives(i - 1).RiskNodeType <> RiskNodeType.ntCategory Then  ' D2686
                                    sID = GetParam(tParams, String.Format(_OBJ_IDX_ID, i))
                                    If (sID <> "") Then
                                        Dim nd As clsNode = Project.HierarchyObjectives.GetNodeByID(CInt(sID))
                                        If Not nd Is Nothing Then
                                            Dim Val As Double = 0   ' D1858
                                            If String2Double(GetParam(tParams, String.Format(_OBJ_IDX_VALUE, i)), Val) Then nd.SALocalPriority = CSng(Val / 100) ' D1858
                                        End If
                                    End If
                                End If
                            Next

                            Project.ProjectManager.DynamicSensitivity.Calculate()

                    End Select

                    Dim sum As Single = 0
                    For Each alt As clsNode In AlternativesList
                        'sum+=alt.SAGlobalPriority
                        sum += alt.WRTGlobalPriority
                    Next
                    If sum <> 0 Then
                        For Each alt As clsNode In AlternativesList
                            'alt.SAGlobalPriority = alt.SAGlobalPriority / sum
                            alt.SAGlobalPriority = alt.WRTGlobalPriority / sum
                        Next
                    End If

                    idx = 0
                    For Each alt As clsNode In AlternativesList ' D0338
                        idx += 1

                        Select Case SAType
                            Case Canvas.SAType.satGradient
                                Dim ValueZero As Single
                                Dim ValueOne As Single
                                Project.ProjectManager.DynamicSensitivity.GetGradientAltValuesForChild(tSANode, alt, ValueZero, ValueOne) 'C0567

                                sContent += String.Format("&" + _ALT_IDX_NAME + "={1}&" + _ALT_IDX_ID + "={2}&" + _ALT_IDX_VALUE0 + "={3}&" + _ALT_IDX_VALUE1 + "={4}", idx, Uri.EscapeUriString(alt.NodeName), alt.NodeID, Uri.EscapeUriString(JS_SafeNumber(ValueZero)), Uri.EscapeUriString(JS_SafeNumber(ValueOne)))

                            Case Else
                                ' D0360 ===
                                Dim tRealAlt As clsNode = GetNodeByID(Project.ProjectManager.DynamicSensitivity.Alternatives, alt.NodeID)
                                If tRealAlt Is Nothing Then tRealAlt = alt
                                sContent += String.Format("&" + _ALT_IDX_NAME + "={1}&" + _ALT_IDX_ID + "={2}&" + _ALT_IDX_VALUE + "={3}", idx, Uri.EscapeUriString(tRealAlt.NodeName), tRealAlt.NodeID, Uri.EscapeUriString(JS_SafeNumber(tRealAlt.SAGlobalPriority * 100)))
                                ' D0360 ==
                        End Select
                    Next

                    Select Case SAType

                        Case Canvas.SAType.satPerformance
                            Dim value As Single
                            Dim NodeIDx As Integer = 0
                            Dim Nodechilds As List(Of clsNode) = Project.ProjectManager.DynamicSensitivity.Node.Children 'C0385

                            For Each child As clsNode In Project.ProjectManager.DynamicSensitivity.Objectives
                                NodeIDx += 1
                                If Not fIgnoreCategories OrElse child.RiskNodeType <> RiskNodeType.ntCategory Then  ' D2686
                                    Dim AltIdx As Integer = 0
                                    Dim AltsList As List(Of clsNode) = AlternativesList 'C0385
                                    For Each alt As clsNode In AltsList
                                        AltIdx += 1
                                        value = Project.ProjectManager.DynamicSensitivity.GetAltValueForChild(child, alt)

                                        sContent += String.Format("&" + _MATRIX_XY + "={2}", NodeIDx, AltIdx, JS_SafeNumber(value))
                                    Next
                                    AltsList = Nothing
                                End If
                            Next
                            Nodechilds = Nothing

                    End Select

                    ' D0368 ===
                    Dim tMaxValue As Single = -1
                    If GetMaxAltValue Or SAType = Canvas.SAType.satGradient Or SAType = Canvas.SAType.satPerformance Then   ' D0379
                        tMaxValue = Project.ProjectManager.DynamicSensitivity.GetMaxAltValueInSA()
                    End If
                    sContent += String.Format("&{0}={1}", _ALT_MAX_VALUE, JS_SafeNumber(tMaxValue))
                    ' D0368 ==

                End If
            End If
            Return sContent
        End Function

    End Class

    ' D0338 ===
    Public Class clsSAGlobalPriorityComparer
        Implements IComparer

        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer _
             Implements IComparer.Compare
            Dim A As clsNode = CType(x, clsNode)
            Dim B As clsNode = CType(y, clsNode)
            Return CInt(IIf(A.SAGlobalPriority = B.SAGlobalPriority, 0, IIf(A.SAGlobalPriority > B.SAGlobalPriority, -1, 1)))
        End Function
    End Class
    ' D0338 ==

End Namespace
