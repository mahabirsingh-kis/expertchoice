Imports Microsoft.VisualBasic
Imports System.Collections
Imports System.IO
Imports ECSecurity.ECSecurity
Imports System.Linq

Namespace ExpertChoice.Data

    <Serializable()> Public Class clsLicense

        Public Const SPYRON_ALLOW_FOR_ALL As Boolean = False    ' D0820 + D3242

        Private _Enabled As Boolean
        Private _Parameters As List(Of clsLicenseParameter)    ' D0464
        Private _LicenseKey As String           ' D0261
        Private _Content As MemoryStream        ' D0261
        Private _ParamsFile As clsParamsFile
        Private _isValid As Boolean
        Private _isValidChecked As Boolean      ' D0261
        Private _isSystemLicense As Boolean     ' D0261

        Public Property Parameters() As List(Of clsLicenseParameter)   ' D0464
            Get
                Return _Parameters
            End Get
            Set(ByVal value As List(Of clsLicenseParameter))    ' D0464
                _Parameters = value
            End Set
        End Property

        Public Property Enabled() As Boolean    ' D0261
            Get
                Return _Enabled
            End Get
            Set(ByVal value As Boolean)
                _Enabled = value
            End Set
        End Property

        Public ReadOnly Property isValidLicense() As Boolean ' D0079
            Get
                If Not _isValidChecked Then
                    If String.IsNullOrEmpty(_LicenseKey) Or _Content Is Nothing Then
                        _isValid = False
                    Else
                        If Not _Content Is Nothing Then _Content.Seek(0, SeekOrigin.Begin)
                        _ParamsFile = New clsParamsFile ' D2644
                        _isValid = _ParamsFile.Read(_Content, _LicenseKey)
                    End If
                    _isValidChecked = True
                End If
                Return Not Enabled Or _isValid
            End Get
        End Property

        Public Function CheckAllParameters(ByRef WrongItem As Long) As Boolean
            For Each tParam As clsLicenseParameter In Parameters
                ' D0285 ===
                Select Case tParam.ID
                    Case ecLicenseParameter.AllowUseGurobi,
                        ecLicenseParameter.CommercialUseEnabled,
                        ecLicenseParameter.CreatedAt,
                        ecLicenseParameter.ExportEnabled,
                        ecLicenseParameter.isSelfHost,
                        ecLicenseParameter.MaxAlternatives,
                        ecLicenseParameter.MaxConcurrentEvaluatorsInModel,
                        ecLicenseParameter.MaxEvaluatorsInModel,
                        ecLicenseParameter.MaxLevelsBelowGoal,
                        ecLicenseParameter.MaxModelsPerOwner,
                        ecLicenseParameter.MaxObjectives,
                        ecLicenseParameter.MaxPMsInProject,
                        ecLicenseParameter.MaxProjectCreatorsInWorkgroup,
                        ecLicenseParameter.MaxUsersInProject,
                        ecLicenseParameter.MaxUsersInWorkgroup,
                        ecLicenseParameter.MaxViewOnlyUsers,
                        ecLicenseParameter.ResourceAlignerEnabled,
                        ecLicenseParameter.RiskEnabled,
                        ecLicenseParameter.RiskTreatments,
                        ecLicenseParameter.RiskTreatmentsOptimization,
                        ecLicenseParameter.SpyronEnabled,
                        ecLicenseParameter.TeamTimeEnabled ' D0741 + D0909 + D0912 + D0913 + D0917 + D2056 + D3586 + D3923 + D3965 + D6568
                    Case Else
                        ' D0285 ==
                        If Not CheckParameter(tParam) Then
                            WrongItem = tParam.ID
                            Return False
                        End If
                End Select
            Next
            Return True
        End Function

        Public Function GetParameterMax(ByVal tParameter As clsLicenseParameter) As Long
            If Not isValidLicense() Or tParameter Is Nothing Then Return LICENSE_NOVALUE ' D0820
            If tParameter.ID = ecLicenseParameter.SpyronEnabled AndAlso SPYRON_ALLOW_FOR_ALL Then Return 1 ' D0820 + D0913
            Dim tParam As clsRestrictionParameter = _ParamsFile.GetParameter(tParameter.ID) ' D0257
            If tParam Is Nothing Then
                Return LICENSE_NOVALUE
            Else
                If CLng(tParam.Value) = 0 AndAlso (tParameter.ID = ecLicenseParameter.MaxLifetimeProjects OrElse tParameter.ID = ecLicenseParameter.MaxUsersInProject OrElse tParameter.ID = ecLicenseParameter.MaxUsersInWorkgroup) Then tParam.Value = 1 ' D1490
                Return CLng(tParam.Value) ' D0257
            End If
        End Function

        Public Function GetParameterMaxByID(ByVal tParameterID As ecLicenseParameter) As Long   ' D0913
            Dim tParam As clsLicenseParameter = ParameterByID(tParameterID)
            If tParam Is Nothing Then Return LICENSE_NOVALUE Else Return GetParameterMax(tParam)
        End Function

        Public Function GetParameterValue(ByVal tParameter As clsLicenseParameter, Optional ByVal ItemData As Object = Nothing) As Long
            Return tParameter.Value(ItemData)
        End Function

        Public Function GetParameterValueByID(ByVal tParameterID As ecLicenseParameter, Optional ByVal ItemData As Object = Nothing) As Long    ' D0913
            Dim tParam As clsLicenseParameter = ParameterByID(tParameterID)
            If tParam Is Nothing Then Return LICENSE_NOVALUE Else Return GetParameterValue(tParam, ItemData)
        End Function

        Public Function GetParameterNameByID(ByVal tParameterID As ecLicenseParameter) As String    ' D0913
            Dim tParam As clsLicenseParameter = ParameterByID(tParameterID)
            If tParam Is Nothing Then Return "Unknown" Else Return tParam.Name
        End Function

        Public Function ParameterByID(ByVal tParameterID As ecLicenseParameter) As clsLicenseParameter  ' D0913
            Return Parameters?.FirstOrDefault(Function(tParam) (tParam.ID = tParameterID))
            'For Each tParam As clsLicenseParameter In Parameters
            '    If tParam.ID = tParameterID Then Return tParam
            'Next
            'Return Nothing
        End Function

        Public Function CheckParameter(ByVal tParameter As clsLicenseParameter, Optional ByVal ItemData As Object = Nothing, Optional ByVal fCanEqual As Boolean = True) As Boolean
            If Not Enabled Then Return True
            If Not isValidLicense() Then Return False
            Dim tMax As Long = GetParameterMax(tParameter)
            Dim tCur As Long = GetParameterValue(tParameter, ItemData)
            ' D1082 ===
            'Return tMax = UNLIMITED_VALUE Or tMax = UNLIMITED_DATE Or CBool(IIf(fCanEqual, tCur <= tMax, tCur < tMax))
            If tMax = UNLIMITED_VALUE Or tMax = UNLIMITED_DATE Then Return True
            If tParameter.ID = ecLicenseParameter.ExpirationDate Then
                Dim dMax As Date = DateTime.FromBinary(tMax).Date
                Dim dCur As Date = DateTime.FromBinary(tCur).Date
                Return If(fCanEqual, dCur <= dMax, dCur < dMax)
            Else
                ' D3946 ===
                If tParameter.ID = ecLicenseParameter.InstanceID Then
                    ' D3952 ===
                    If tMax = 0 OrElse tMax = -1 OrElse tMax = tCur Then Return True
                    Dim sMax As String = tMax.ToString("X16")
                    Dim sCur As String = tCur.ToString("X16")
                    If sMax.Substring(1, 7) = sCur.Substring(1, 7) AndAlso sMax.Substring(8).Trim(CChar("0")) = "" Then Return True ' vXXXXXXX-00000000: no check for DB
                    If sMax.Substring(8) = sCur.Substring(8) AndAlso sMax.Substring(1, 7).Trim(CChar("0")) = "" Then Return True ' v0000000-XXXXXXXX: no check for server
                    Return False
                    ' D3952 ==
                Else
                    ' D3946 ==
                    Return If(fCanEqual, tCur <= tMax, tCur < tMax)
                End If
            End If
            ' D1082 ==
        End Function

        Public Function CheckParameterByID(ByVal tParameterID As ecLicenseParameter, Optional ByVal ItemData As Object = Nothing, Optional ByVal fCanEqual As Boolean = True) As Boolean    ' D0913
            Dim tParam As clsLicenseParameter = ParameterByID(tParameterID)
            If tParam Is Nothing Then Return True Else Return CheckParameter(tParam, ItemData, fCanEqual) ' D0478: pass check when param not exists
        End Function

        ' D0261 ===
        Public Property isSystemLicense() As Boolean
            Get
                Return _isSystemLicense
            End Get
            Set(ByVal value As Boolean)
                _isSystemLicense = value
            End Set
        End Property

        Public Property LicenseKey() As String
            Get
                Return _LicenseKey
            End Get
            Set(ByVal value As String)
                '_LicenseKey = value.ToLower
                _LicenseKey = value     ' D1000
                _isValidChecked = False
            End Set
        End Property

        Public Property LicenseContent() As MemoryStream
            Get
                Return _Content
            End Get
            Set(ByVal value As MemoryStream)
                _Content = value
                _isValidChecked = False
            End Set
        End Property
        ' D0261 ===

        Public Sub New(Optional ByVal sLicenseContent As MemoryStream = Nothing, Optional ByVal sLicenseKey As String = "")  ' D0261 + D0264
            _Parameters = New List(Of clsLicenseParameter)  ' D0464
            ' D0261 ===
            _Content = sLicenseContent
            _LicenseKey = sLicenseKey.ToLower
            _ParamsFile = New clsParamsFile
            _isValidChecked = False
            _Enabled = LicenseContent IsNot Nothing And Not String.IsNullOrEmpty(sLicenseKey)    ' D0256
            ' D0261 ==
        End Sub

    End Class

End Namespace
