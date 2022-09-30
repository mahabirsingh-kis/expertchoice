Imports System.Web.UI.WebControls
Imports System.Collections.Specialized
Imports ExpertChoice.Data
Imports ExpertChoice.Service
Imports ECCore

Namespace ExpertChoice.Data

    <Serializable> Public Class clsSetting
        Public ID As Integer = -1
        Public Property ConfigName As String = ""
        Public Property ResName As String = ""
        Public Property SettingType As Type = GetType(Boolean)
        Public Property Value As String
        Public Property isAppSetting As Boolean

        Public Sub New(CName As String, RName As String, sType As Type, fAppSetting As Boolean, Optional tValue As String = Nothing)
            ConfigName = CName
            ResName = RName
            SettingType = sType
            ID = Convert.ToInt32(ExpertChoice.Service.GetMD5(ConfigName.ToLower).Substring(0, 6), 16)
            isAppSetting = fAppSetting
            If Not fAppSetting Then Value = WebConfigOption(CName, "", True)
            If tValue IsNot Nothing Then Value = tValue
        End Sub

        Shared Function SettingByName(Lst As List(Of clsSetting), CName As String) As clsSetting
            If Lst IsNot Nothing Then
                CName = CName.ToLower
                For Each tOpt As clsSetting In Lst
                    If tOpt.ConfigName.ToLower = CName Then Return tOpt
                Next
            End If
            Return Nothing
        End Function

        Shared Function SettingByID(Lst As List(Of clsSetting), ID As Integer) As clsSetting
            If Lst IsNot Nothing Then
                For Each tOpt As clsSetting In Lst
                    If tOpt.ID = ID Then Return tOpt
                Next
            End If
            Return Nothing
        End Function

    End Class

    Public Class clsSettingComparer
        Implements IComparer(Of clsSetting)

        Public Function Compare(ByVal A As clsSetting, ByVal B As clsSetting) As Integer Implements IComparer(Of clsSetting).Compare
            Return String.Compare(A.ConfigName, B.ConfigName)
        End Function
    End Class

End Namespace
