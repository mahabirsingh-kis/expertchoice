'Imports System.Web.UI.Page '-D0457

Namespace ExpertChoice.Data ' D0457

    <Serializable()> Public Enum ecErrorStatus
        errNone = -1
        errMessage = 1
        errDatabase = 2
        errAccessDenied = 3
        errPageNotFound = 4
        errWrongLicense = 5     ' D0257
        errTest = 8             ' D0175
        errRTE = 9
    End Enum

    Public Enum ecAuthenticateError
        aeUnknown = -1
        aeNoErrors = 0
        aeWrongPassword = 1         ' D0161
        aeWrongPasscode = 2
        aeUserLocked = 3
        aeGroupLocked = 4
        aeWorkgroupLocked = 5
        aeProjectLocked = 6         ' D0051
        aeWorkspaceLocked = 7       ' D0051
        aeNoWorkgroupSelected = 8  ' -D4842
        aeNoProjectsFound = 9
        aeNoUserFound = 10          ' D0161
        aeNoSynchronousProject = 11 ' D0213
        aeWrongLicense = 12         ' D0263
        ' D0390 ===
        aeWrongMeetingID = 14
        aeNoSynchronousStarted = 15
        aeUseRegularLogon = 16
        aeMeetingIDNotAllowed = 17
        ' D0390 ==
        aeUserWorkgroupExpired = 18 ' D0429
        aeWrongCredentials = 19     ' D0432
        aeSynchronousFull = 20      ' D0450
        aeSynchronousUserNotAllowed = 21    ' D0659 -D2038 + D2433
        aeWrongInstanceID = 22      ' D0823
        aeDeletedProject = 23       ' D1601
        aePasscodeNotAllowed = 24   ' D1726
        aeUserLockedByWrongPsw = 25 ' D2213
        aeProjectReadOnly = 26      ' D2489
        aeUserWorkgroupLocked = 27  ' D2618
        aeEvaluationNotAllowed = 28 ' D2808
        aeWorkgroupExpired = 29     ' D3303 // for PO, WM
        aeWorkgroupExpiredEval = 30 ' D3303 // for users
        aeSystemWorkgroupExpired = 31   ' D4982
        aeTotalWorkgroupsLimit = 32 ' D6568
        aeLocalhostAllowedOnly = 33 ' D6640
        aeMFA_Required = 34         ' D7502
    End Enum

    <Serializable()> Public Class clsApplicationError
        Private _ErrorType As ecErrorStatus
        Private _ErrorPage As Integer
        Private _ErrorMessage As String
        Private _ErrorURL As String
        Private _doFetch As Boolean     ' D0471
        Private _Details As Exception   ' D0466
        Private _Custom As String       ' D0488

        Public Property Status() As ecErrorStatus
            Get
                Return _ErrorType
            End Get
            Set(ByVal value As ecErrorStatus)
                _ErrorType = value
            End Set
        End Property

        Public Property PageID() As Integer
            Get
                Return _ErrorPage
            End Get
            Set(ByVal value As Integer)
                _ErrorPage = value
            End Set
        End Property

        Public Property Message() As String
            Get
                Return _ErrorMessage
            End Get
            Set(ByVal value As String)
                _ErrorMessage = value
            End Set
        End Property

        Public Property PageURL() As String
            Get
                Return _ErrorURL
            End Get
            Set(ByVal value As String)
                _ErrorURL = value
            End Set
        End Property

        ' D0471 ===
        Public Property DoFetch() As Boolean
            Get
                Return _doFetch
            End Get
            Set(ByVal value As Boolean)
                _doFetch = value
            End Set
        End Property
        ' D0471 ==

        ' D0466 ===
        Public Property Details() As Exception
            Get
                Return _Details
            End Get
            Set(ByVal value As Exception)
                _Details = value
            End Set
        End Property
        ' D0466 ==

        ' D0488 ==
        Public Property CustomData() As String
            Get
                Return _Custom
            End Get
            Set(ByVal value As String)
                _Custom = value
            End Set
        End Property
        ' D0488 ==

        Public Sub Reset()
            _ErrorPage = -1     ' D0457
            _ErrorType = ecErrorStatus.errNone
            _ErrorMessage = ""
            _ErrorURL = ""
            _doFetch = False    ' D0471
            _Details = Nothing  ' D0466
            _Custom = ""        ' D0488
        End Sub

        Public Sub Init(ByVal ErrType As ecErrorStatus, ByVal tPageID As Integer, ByVal sErrorMessage As String, Optional ByVal ErrObject As Object = Nothing, Optional ByVal sErrorSrcURL As String = "", Optional ByVal tDetails As Exception = Nothing) ' D0466
            Status = ErrType
            PageID = tPageID
            Message = sErrorMessage
            PageURL = sErrorSrcURL
            Details = tDetails  ' D0466
        End Sub

        Public Sub New()
            Reset()
        End Sub

    End Class

End Namespace
