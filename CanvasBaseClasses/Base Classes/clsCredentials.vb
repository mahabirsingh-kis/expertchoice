Imports System.Web.UI.Page
Imports System.Collections.Specialized
Imports ExpertChoice.Service
Imports System.Web

Namespace ExpertChoice.Data

    <Serializable()> Public Class clsCredentials

        Private mStuff As Long     ' D0432
        Private mUserEmail As String
        Private mPasscode As String
        Private mHash As String
        Private mInstanceID As String   ' D0557
        Private mWorkgroupID As Integer ' D0718
        Private mSessionID As String    ' D2289

        Shared KEY_USEREMAIL As String = "email"
        Shared KEY_PASSCODE As String = "passcode"
        Shared KEY_INSTANCE As String = "guid"  ' D0557
        Shared KEY_WKG_ID As String = "wkgid"   ' D0718
        Shared KEY_STUFF As String = "t"        ' D0432
        Shared KEY_SESS_ID As String = "sid"    ' D2289

        Public Property UserEmail() As String
            Get
                Return mUserEmail
            End Get
            Set(ByVal value As String)
                If mUserEmail <> value Then
                    mUserEmail = value
                    ResetHash()
                End If
            End Set
        End Property

        Public Property Passcode() As String
            Get
                Return mPasscode
            End Get
            Set(ByVal value As String)
                If mPasscode <> value Then
                    mPasscode = value
                    ResetHash()
                End If
            End Set
        End Property

        ' D0557 ===
        Public Property InstanceID() As String
            Get
                Return mInstanceID
            End Get
            Set(ByVal value As String)
                If mInstanceID <> value Then
                    mInstanceID = value
                    ResetHash()
                End If
            End Set
        End Property
        ' D0557 ==

        ' D2289 ===
        Public Property SessionID() As String
            Get
                Return mSessionID
            End Get
            Set(ByVal value As String)
                mSessionID = value
            End Set
        End Property
        ' D2289 ===

        Public Property WorkgroupID() As Integer
            Get
                Return mWorkgroupID
            End Get
            Set(ByVal value As Integer)
                mWorkgroupID = value
            End Set
        End Property
        ' D0718 ==

        Public Sub ResetHash()
            mHash = ""
        End Sub

        Public ReadOnly Property HashString() As String
            Get
                If mHash = "" Then
                    mHash = String.Format("{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}={9}", KEY_STUFF, Stuff, KEY_USEREMAIL, HttpUtility.UrlEncode(UserEmail), KEY_PASSCODE, HttpUtility.UrlEncode(Passcode), KEY_INSTANCE, InstanceID, KEY_WKG_ID, WorkgroupID.ToString)    ' D0432 + D0459 + D0557 + D0718 + D1466 + D1672 + D1724
                    If SessionID <> "" Then mHash += String.Format("&{0}={1}", KEY_SESS_ID, SessionID) ' D2289
                    mHash = EncodeURL(mHash, InstanceID)    ' D0826
                End If
                Return mHash
            End Get
        End Property

        ' D0432 ===
        Private Property Stuff() As Long
            Get
                Return mStuff
            End Get
            Set(ByVal value As Long)
                mStuff = value
            End Set
        End Property
        ' D0432 ==

        Shared Function TryParseHash(ByVal sHashString As String, ByVal sInstanceID As String, Optional ByRef tCredentials As clsCredentials = Nothing) As Boolean   ' D0826
            Dim fResult As Boolean = False
            If Not String.IsNullOrEmpty(sHashString) AndAlso Not String.IsNullOrEmpty(sInstanceID) Then   ' D1566
                Dim sURL As String = DecodeURL(sHashString, sInstanceID) ' D0826
                If Not String.IsNullOrEmpty(sURL) Then   ' D1466
                    Dim tParams As NameValueCollection = HttpUtility.ParseQueryString(URLDecode(HttpUtility.UrlEncode(sURL)))   ' D1466
                    Dim tPassed As Integer = 0
                    Dim tmpCredentials As New clsCredentials

                    For Each sName As String In tParams
                        If Not String.IsNullOrEmpty(sName) Then ' D1466
                            Select Case sName.ToLower

                                Case KEY_USEREMAIL, KEY_PASSCODE, KEY_STUFF, KEY_INSTANCE, KEY_WKG_ID, KEY_SESS_ID    ' D0432 + D0459 + D0557 + D0718 + D1672 + D1724 + D2289
                                    Dim sData As String = tParams(sName).Trim
                                    If sData <> "" Then
                                        tPassed += 1
                                        ' D0432 ====
                                        Select Case sName.ToLower
                                            Case KEY_USEREMAIL
                                                tmpCredentials.UserEmail = sData
                                            Case KEY_PASSCODE
                                                tmpCredentials.Passcode = sData
                                            Case KEY_INSTANCE   ' D0557
                                                tmpCredentials.InstanceID = sData   ' D0557
                                            Case KEY_STUFF
                                                Long.TryParse(sData, tmpCredentials.Stuff)
                                            Case KEY_WKG_ID     ' D0718
                                                Integer.TryParse(sData, tmpCredentials.WorkgroupID) ' D0718
                                            Case KEY_SESS_ID    ' D2289
                                                tmpCredentials.SessionID = sData    ' D2289
                                        End Select
                                        ' D0432 ==
                                    End If

                            End Select
                        End If
                    Next
                fResult = tPassed >= 2
                If fResult AndAlso Not tCredentials Is Nothing Then tCredentials = tmpCredentials
            End If
            End If

            Return fResult
        End Function

        Public Function Clone() As clsCredentials
            Dim Copy As New clsCredentials
            Copy.UserEmail = Me.UserEmail
            Copy.Passcode = Me.Passcode
            Copy.Stuff = Me.Stuff           ' D0432
            Copy.InstanceID = Me.InstanceID ' D0557
            Copy.WorkgroupID = Me.WorkgroupID   ' D0718
            Copy.SessionID = Me.SessionID   ' D2289
            Return Copy
        End Function

        Public Sub New(Optional ByVal sUserEmail As String = "", Optional ByVal sPasscode As String = "", Optional ByVal sInstanceID As String = "", Optional ByVal tWorkgroupID As Integer = -1, Optional tSessionID As String = "")    ' D0557 + D0718 + D1672 + D1724 + D2289
            mStuff = Now.ToBinary   ' D0432
            mUserEmail = sUserEmail
            mPasscode = sPasscode
            mInstanceID = sInstanceID   ' D0557
            mWorkgroupID = tWorkgroupID ' D0718
            mSessionID = tSessionID     ' D2289
            ResetHash()
        End Sub

    End Class

End Namespace
