Namespace ECSecurity

    Public Module ParamsConstants   ' D0076

        Public Enum ecLicenseParameter  ' D0913
            Unknown = -1    ' D0913
            MaxProjectCreatorsInWorkgroup = 1   ' D2582
            MaxPMsInProject = 2                 ' D2582
            MaxEvaluatorsInModel = 3   ' -D2548
            MaxModelsPerOwner = 4      ' -D2548
            MaxProjectsTotal = 5                ' D2582
            MaxConcurrentEvaluatorsInModel = 6     ' -D2548
            ExpirationDate = 7
            MaxWorkgroupsTotal = 8
            TeamTimeEnabled = 9
            MaxProjectsOnline = 10      ' D2582
            SpyronEnabled = 11
            ResourceAlignerEnabled = 12
            ExportEnabled = 14
            CommercialUseEnabled = 15   ' D0917
            MaxLifetimeProjects = 16
            MaxObjectives = 17
            MaxLevelsBelowGoal = 18     ' D0927
            MaxAlternatives = 19
            MaxViewOnlyUsers = 20       ' -D2548
            MaxUsersInProject = 21      ' D1482
            MaxUsersInWorkgroup = 22    ' D1482
            RiskEnabled = 23            ' D2056
            RiskTreatments = 24         ' D3585
            RiskTreatmentsOptimization = 25 ' D3585
            AllowUseGurobi = 26         ' D3922
            InstanceID = 27             ' D3946
            isSelfHost = 28             ' D3965
            CreatedAt = 29              ' D3965
        End Enum

        Public Const UNLIMITED_VALUE As Long = -1
        Public Const UNLIMITED_DATE As Long = 946392818100000000 ' 1.01.3000

        ' D3952 ===
        Public Function isValidInstanceID(sValue As String) As Boolean
            Dim sID As String = sValue.Trim.Replace("-", "")
            If sID = "" Then Return True
            Try
                Dim tRes As Long
                Return Long.TryParse(sID, System.Globalization.NumberStyles.AllowHexSpecifier, Nothing, tRes)
            Catch ex As Exception
                Return False
            End Try
        End Function
        ' D3952 ==

    End Module

End Namespace