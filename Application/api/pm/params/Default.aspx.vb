Partial Class PipeParamsWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Private ReadOnly Property PM As clsProjectManager
        Get
            If App.HasActiveProject Then Return App.ActiveProject.ProjectManager Else Return Nothing
        End Get
    End Property

    Public Property NormalizeMode As LocalNormalizationType
        Get
            If PM.IsRiskProject Then Return LocalNormalizationType.ntUnnormalized
            Return CType(PM.Parameters.Normalization, LocalNormalizationType)
        End Get
        Set(value As LocalNormalizationType)
            PM.Parameters.Normalization = value
            PM.Parameters.Save()
        End Set
    End Property

    Private Sub SavePipeParameters(OptionName As String, OptionValue As String)
        App.ActiveProject.SaveProjectOptions("api/pm/params set_pipe_option " + OptionName + "=" + OptionValue, , False, "")
    End Sub

    ' D6336 ===
    Public Function Get_Pipe_Option(OptionName As String) As jActionResult ' D6336
        Dim tResult As New jActionResult
        Try
            tResult.Data = CallByName(PM.PipeParameters, OptionName, CallType.Get)
            tResult.Tag = OptionName
            tResult.Result = ecActionResult.arSuccess
        Catch ex As Exception
            tResult.Result = ecActionResult.arError
            tResult.Message = "Unable to find option with that name"
        End Try
        Return tResult
    End Function
    ' D6336 ==

    Public Function Set_Pipe_Option(OptionName As String, OptionValue As String) As jActionResult ' D6336
        Dim OptionType As Type = Nothing
        ' D6336 ===
        Dim tResult As New jActionResult

        Try
            OptionType = CallByName(PM.PipeParameters, OptionName, CallType.Get).GetType
        Catch ex As Exception
            tResult.Result = ecActionResult.arError
            tResult.Message = "Unable to find option with that name"
            ' D6336 ==
        End Try

        If OptionType IsNot Nothing Then
            ' D6336 ===
            If OptionType.IsEnum Then
                CallByName(PM.PipeParameters, OptionName, CallType.Set, {CInt(OptionValue)})
            Else
                ' D6850 ===
                Select Case OptionType.Name
                    Case GetType(Boolean).ToString
                        CallByName(PM.PipeParameters, OptionName, CallType.Set, {Str2Bool(OptionValue)})
                        'Case GetType(Integer).ToString
                        '    CallByName(PM.PipeParameters, OptionName, CallType.Set, {CInt(OptionValue)})
                        'Case GetType(Double).ToString
                        '    CallByName(PM.PipeParameters, OptionName, CallType.Set, {CDbl(OptionValue)})
                        'Case GetType(Long).ToString
                        '    CallByName(PM.PipeParameters, OptionName, CallType.Set, {CLng(OptionValue)})
                    Case Else
                        CallByName(PM.PipeParameters, OptionName, CallType.Set, {OptionValue})
                End Select
                ' D6850 ==
            End If
            Select Case OptionName.ToLower
                Case "synthesismode"
                    PM.CalculationsManager.SynthesisMode = CType(CInt(OptionValue), ECSynthesisMode)
                    If PM.PipeParameters.SynthesisMode = ECSynthesisMode.smDistributive And NormalizeMode = LocalNormalizationType.ntUnnormalized Then
                        NormalizeMode = LocalNormalizationType.ntNormalizedForAll
                        PM.CalculationsManager.UseNormalizationForSA = Not (NormalizeMode = LocalNormalizationType.ntUnnormalized)
                    End If
                Case "useweights"
                    PM.CalculationsManager.UseUserWeights = PM.PipeParameters.UseWeights
                Case "combinedmode"
                    PM.CalculationsManager.CombinedMode = PM.PipeParameters.CombinedMode
            End Select
            SavePipeParameters(OptionName, OptionValue)
            tResult.Result = ecActionResult.arSuccess
            tResult.Tag = OptionName
            tResult.Data = CallByName(PM.PipeParameters, OptionName, CallType.Get)
        End If

        Return tResult
        ' D6336 ==
    End Function

    ' D6850 ===
    Public Function Get_Param_Option(OptionName As String) As jActionResult
        Dim tResult As New jActionResult
        Try
            tResult.Data = CallByName(PM.Parameters, OptionName, CallType.Get)
            tResult.Tag = OptionName
            tResult.Result = ecActionResult.arSuccess
        Catch ex As Exception
            tResult.Result = ecActionResult.arError
            tResult.Message = "Unable to find parameter with that name"
        End Try
        Return tResult
    End Function

    Public Function Set_Param_Option(OptionName As String, OptionValue As String) As jActionResult
        Dim OptionType As Type = Nothing
        Dim tResult As New jActionResult

        Try
            OptionType = CallByName(PM.Parameters, OptionName, CallType.Get).GetType
        Catch ex As Exception
            tResult.Result = ecActionResult.arError
            tResult.Message = "Unable to find parameter with that name"
        End Try

        If OptionType IsNot Nothing Then
            If OptionType.IsEnum Then
                CallByName(PM.Parameters, OptionName, CallType.Set, {CInt(OptionValue)})
            Else
                Select Case OptionType.Name
                    Case GetType(Boolean).ToString
                        CallByName(PM.Parameters, OptionName, CallType.Set, {Str2Bool(OptionValue)})
                        'Case GetType(Integer).ToString, GetType(Int32).ToString
                        '    CallByName(PM.Parameters, OptionName, CallType.Set, {CInt(OptionValue)})
                        'Case GetType(Double).ToString
                        '    CallByName(PM.Parameters, OptionName, CallType.Set, {CDbl(OptionValue)})
                        'Case GetType(Long).ToString
                        '    CallByName(PM.Parameters, OptionName, CallType.Set, {CLng(OptionValue)})
                    Case Else
                        CallByName(PM.Parameters, OptionName, CallType.Set, {OptionValue})
                End Select
            End If

            App.ActiveProject.ProjectManager.Parameters.Save()  ' D6948
            tResult.Result = ecActionResult.arSuccess
            tResult.Tag = OptionName
            tResult.Data = CallByName(PM.Parameters, OptionName, CallType.Get)
        End If

        Return tResult
    End Function
    ' D6850 ==

    Private Sub PipeParamsWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNoActiveProject()
        Select Case _Page.Action

            Case "get_pipe_option"  ' D6336
                _Page.ResponseData = Get_Pipe_Option(GetParam(_Page.Params, "name", True))   ' D6336

            Case "set_pipe_option"
                _Page.ResponseData = Set_Pipe_Option(GetParam(_Page.Params, "name", True), GetParam(_Page.Params, "value", True))   ' D6336

                ' D6850 === 
            Case "get_param_option"
                _Page.ResponseData = Get_Param_Option(GetParam(_Page.Params, "name", True))

            Case "set_param_option"
                _Page.ResponseData = Set_Param_Option(GetParam(_Page.Params, "name", True), GetParam(_Page.Params, "value", True))
                ' D6850 ==

            Case "shell_settings"
                Dim tRes As New jActionResult With {.Result = ecActionResult.arSuccess}
                Dim paletteId As Integer = CInt(GetParam(_Page.Params, "paletteId", True))
                With App.ActiveProject.ProjectManager.Parameters
                    If .SynthesisColorPaletteId <> paletteId Then 
                        .SynthesisColorPaletteId = paletteId
                        .Save()
                    End If
                End With
                _Page.ResponseData = tRes
        End Select
    End Sub

End Class