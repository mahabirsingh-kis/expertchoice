Imports System.Reflection

Namespace ExpertChoice.WebAPI

    ' AD: Please update values _res_* in masterpage.js in case of changes in that enum

    ''' <summary>
    ''' The code or API request result
    ''' </summary>
    <Serializable> Public Enum ecActionResult
        ''' <summary>
        ''' Unknown state
        ''' </summary>
        arNone = 0
        ''' <summary>
        ''' Successfully
        ''' </summary>
        arSuccess = 1
        ''' <summary>
        ''' Had an error
        ''' </summary>
        arError = 2
        ''' <summary>
        ''' No error, but can be warning or important message
        ''' </summary>
        arWarning = 3
        'arMessage = 4
    End Enum

    ''' <summary>
    ''' Base class for json objects
    ''' </summary>
    <Serializable> Public Class clsJsonObject

        'Public Function FromJSON(JSON As String) As Object
        '    Try
        '        Return JsonConvert.DeserializeObject(JSON)
        '    Catch ex As Exception
        '        Return Nothing
        '    End Try
        'End Function

        Public Function ToJSON() As String
            Return JsonConvert.SerializeObject(Me)
        End Function

        ' D7267 ===
        Shared Function doInherit(ByRef Base As Object, DestType As Type) As Object
            Dim Dest As Object = Activator.CreateInstance(DestType)
            If Base IsNot Nothing AndAlso Dest IsNot Nothing Then
                Dim properties As PropertyInfo() = Base.GetType().GetProperties()
                For Each P As PropertyInfo In properties
                    If P IsNot Nothing Then
                        Dim Val As Object = P.GetValue(Base, Nothing)   ' D6494
                        If Val IsNot Nothing Then P.SetValue(Dest, Val, Nothing) ' D6385 + D6494
                    End If
                Next
            End If
            Return Dest
        End Function
        ' D7267 ==

    End Class

    ''' <summary>
    ''' This is the base class for webAPI responses
    ''' </summary>
    <Serializable> Public Class jActionResult
        Inherits clsJsonObject

        ''' <summary>
        ''' Enum that reflect the request processing: is is success or not
        ''' </summary>
        Public Result As ecActionResult = ecActionResult.arNone
        Public ObjectID As Integer = -1
        ''' <summary>
        ''' Can be used for pass error/warning messages
        ''' </summary>
        Public Message As String = ""
        Public URL As String = ""
        ''' <summary>
        ''' Open parameter for pass any kind of data
        ''' </summary>
        Public Data As Object = Nothing
        Public Tag As Object = Nothing

    End Class

    <Serializable> Public Class jLogEvent
        Inherits clsJsonObject

        Public Property ID As Integer = -1
        Public Property DT As DateTime? = Nothing
        Public Property WorkgroupID As Integer = -1
        Public Property UserEmail As String = ""
        'Public Property UserName As String = ""
        Public Property ActionID As Integer = 0 'dbActionType
        Public Property TypeID As Integer = -1 'dbObjectType
        Public Property ObjectName As String = ""
        Public Property Comment As String = ""
        Public Property Result As String = ""
    End Class

End Namespace