Imports System.Web
Imports System.Configuration
Imports System.Web.Hosting
Imports ExpertChoice.Data

Namespace ExpertChoice.Service

    ''' <summary>
    ''' Module with run-time options from web.config file and web-server parameters.
    ''' </summary>
    ''' <remarks>Could be used like as Options.*, when this unit is not imported</remarks>
    Public Module Options

#Region "Web.config options"

        Public _Options_Individual As List(Of clsSetting) = Nothing  ' D3821

        ''' <summary>
        ''' Get option value from web.config file
        ''' </summary>
        ''' <param name="sOptionName">Name for Options.</param>
        ''' <param name="sDefaultValue">Optional, string for default value (when not provided)</param> 
        ''' <param name="fEmptyAsDefault">Use sDefaultValue instead empty option. Optional</param>
        ''' <returns>String with value. If Option not been found, empty string will be returned.</returns>
        ''' <remarks></remarks>
        Public Function WebConfigOption(ByVal sOptionName As String, Optional ByVal sDefaultValue As String = "", Optional ByVal fEmptyAsDefault As Boolean = False) As String
            Dim sVals() As String = ConfigurationManager.AppSettings.GetValues(sOptionName)
            Dim sValue As String = sDefaultValue
            If Not sVals Is Nothing Then
                If sVals.GetLength(0) > 0 Then sValue = CStr(sVals(sVals.GetLowerBound(0)))
            End If

            ' D3821 ===
            Dim tOpt As clsSetting = Nothing
            If _OPT_USE_CUSTOM_SETTINGS AndAlso _Options_Individual IsNot Nothing Then
                tOpt = clsSetting.SettingByName(_Options_Individual, sOptionName)
                If tOpt IsNot Nothing AndAlso tOpt.Value <> "" Then
                    sValue = tOpt.Value
                    DebugInfo(String.Format("Override web.config option: {0}='{1}'", sOptionName, sValue))
                End If
            End If
            ' D3821 ==

            If sValue = "" AndAlso fEmptyAsDefault Then sValue = sDefaultValue

            If _OPT_USE_CUSTOM_SETTINGS AndAlso tOpt IsNot Nothing AndAlso tOpt.Value <> sValue Then tOpt.Value = sValue ' D3821

            'DebugInfo(String.Format("Read web.config option: {0}='{1}'", sOptionName, sValue))
            Return sValue
        End Function

#End Region

    End Module

End Namespace
