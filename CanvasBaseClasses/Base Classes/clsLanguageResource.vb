Imports System.IO

Namespace ExpertChoice.Data

    <Serializable()> Public Class clsLanguageResource

        Shared _RES_LANG_NAME As String = "_LanguageName"
        Shared _RES_LANG_CODE As String = "_LanguageCode"
        Shared _RES_LANG_CULTURE As String = "_culture"     ' D0849

        Private _ResxFilename As String
        Private _Resources As clsResourceReader
        Private _isLoaded As Boolean

        Public Sub New()
            '_ID = 0    ' -D0461
            _ResxFilename = ""
            _Resources = Nothing
            _isLoaded = False
        End Sub

        Public Property ResxFilename() As String
            Get
                Return _ResxFilename
            End Get
            Set(ByVal value As String)
                If _ResxFilename <> value Then
                    _ResxFilename = value
                    _Resources = New clsResourceReader
                    _isLoaded = _Resources.ReadResourceFile(_ResxFilename)
                End If
            End Set
        End Property

        Public ReadOnly Property isLoaded() As Boolean
            Get
                Return _isLoaded
            End Get
        End Property

        Public ReadOnly Property Resources() As clsResourceReader
            Get
                Return _Resources
            End Get
        End Property

        Public Function GetString(ByVal sResourceName As String, Optional ByVal sDefString As String = "", Optional ByRef fExistedFlag As Boolean = Nothing) As String  ' D0461 + D2256
            Dim sRes As String = sDefString
            Dim tRes As clsResourceParameter = Nothing  ' D0461
            If Not Resources Is Nothing Then
                tRes = Resources.ParameterByName(sResourceName) ' D0461
                If Not tRes Is Nothing Then sRes = tRes.Value
            End If
            fExistedFlag = CBool(Not tRes Is Nothing) ' D0461 + D2256
            Return sRes
        End Function

        Public ReadOnly Property LanguageName() As String
            Get
                Return GetString(_RES_LANG_NAME, Path.GetFileNameWithoutExtension(ResxFilename))
            End Get
        End Property

        Public ReadOnly Property LanguageCode() As String
            Get
                Return GetString(_RES_LANG_CODE, Path.GetFileNameWithoutExtension(ResxFilename))
            End Get
        End Property

        ' D0849 ===
        Public ReadOnly Property LanguageCulture() As String
            Get
                Return GetString(_RES_LANG_CULTURE, "en")
            End Get
        End Property
        ' D0849 ==

        ' D0466 ===
        Public Shared Function LanguageByCode(ByVal sCode As String, ByVal tLanguagesList As List(Of clsLanguageResource)) As clsLanguageResource
            sCode = sCode.ToLower
            If Not tLanguagesList Is Nothing Then
                For Each tLang As clsLanguageResource In tLanguagesList
                    If tLang.LanguageCode.ToLower = sCode OrElse _
                       Path.GetFileName(tLang.ResxFilename).ToLower = sCode OrElse _
                       Path.GetFileNameWithoutExtension(tLang.ResxFilename).ToLower = sCode _
                    Then Return tLang
                Next
            End If
            Return Nothing
        End Function
        ' D0466 ==

    End Class

End Namespace

