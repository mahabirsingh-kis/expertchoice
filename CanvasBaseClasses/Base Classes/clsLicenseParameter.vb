Imports Microsoft.VisualBasic.CallType
Imports ECSecurity.ECSecurity.ParamsConstants

Namespace ExpertChoice.Data

    Public Module LicenseTypes

        Public Const LICENSE_NOVALUE As Long = 0

        Delegate Function LicenseValueFunction(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long ' D0261

    End Module


    <Serializable()> Public Class clsLicenseParameter

        Private _ID As ecLicenseParameter   ' D0913
        Private _Name As String
        Private _Enable As Boolean
        Private _Workgroup As clsWorkgroup   ' D0261
        Private _FunctionValue As LicenseValueFunction

        Public Property ID() As ecLicenseParameter  ' D0913
            Get
                Return _ID
            End Get
            Set(ByVal value As ecLicenseParameter)
                _ID = value
            End Set
        End Property

        Public Property Enable() As Boolean
            Get
                Return _Enable
            End Get
            Set(ByVal value As Boolean)
                _Enable = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return _Name
            End Get
            Set(ByVal value As String)
                _Name = value
            End Set
        End Property

        ' D0261 ===
        Public Property Workgroup() As clsWorkgroup
            Get
                Return _Workgroup
            End Get
            Set(ByVal value As clsWorkgroup)
                _Workgroup = value
            End Set
        End Property
        ' D0261 ==

        ' D0264 ===
        Public ReadOnly Property HasFunctionValue() As Boolean
            Get
                Return _FunctionValue IsNot Nothing
            End Get
        End Property
        ' D0264 ==

        Public WriteOnly Property FunctionValue() As LicenseValueFunction
            Set(ByVal value As LicenseValueFunction)
                _FunctionValue = value
            End Set
        End Property

        Public Function Value(ByVal ItemData As Object) As Long
            If Not _FunctionValue Is Nothing Then Return _FunctionValue(Workgroup, ItemData) ' D0261
            Return LICENSE_NOVALUE
        End Function

        Public Sub New()
            _ID = 0
            _Name = ""
            _Enable = False
            _Workgroup = Nothing    ' D0261
            _FunctionValue = Nothing
        End Sub

    End Class

End Namespace
