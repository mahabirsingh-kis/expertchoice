Namespace ExpertChoice.Data

    <Serializable()> Public Class clsMeetingID

        Public Enum ecMeetingIDType
            TeamTime = 0
            Passcode = 1
            Antigua = 2
        End Enum

        Shared _DEF_MEETING_ID_LENGTH As Integer = 9    ' D0384 + D0457
        Shared _TEMPL_MEETING_ID_STRING As String = "###-###-###"   ' D0384 + D0388 + D0457
        Shared _DEF_PASSCODE_LENGTH As Integer = 8      ' D2674
        Shared _TEMPL_PASSCODE_STRING As String = "####-####"       ' D2674
        Shared _TEMPL_ANTIGUA_STRING As String = "1####-#####"      ' D4920

        Public Shared Function ReNew(Optional fCreateAsPasscode As Boolean = False) As Long    ' D0388 + D0420 + D2674
            Randomize() ' D0388
            ' D1343 ===
            Dim res As String = ""
            Dim L As Integer = CInt(IIf(fCreateAsPasscode, _DEF_PASSCODE_LENGTH, _DEF_MEETING_ID_LENGTH))   ' D2674
            While res.Length < L + 5   ' D1360 + D2674
                res += Rnd().ToString("F5").Substring(2)
            End While
            res = res.TrimStart("0"c)   ' D1360
            If res.Length > L Then res = res.Substring(0, L) ' D1360 + D2674
            ' D1343 ==

            Dim ID As Long = CLng(res)
            Return ID
        End Function

        Public Shared Function AsString(ByVal tMeetingID As Long, Optional sType As ecMeetingIDType = ecMeetingIDType.TeamTime) As String   ' D0390 + D2674 + D4920
            Return tMeetingID.ToString(If(sType = ecMeetingIDType.Passcode, _TEMPL_PASSCODE_STRING, If(sType = ecMeetingIDType.Antigua, _TEMPL_ANTIGUA_STRING, _TEMPL_MEETING_ID_STRING)))    ' D0388 + D0390 + D2674 + D4920
        End Function

        ' D0390 ===
        Public Shared Function AsString(ByVal sMeetingID As String) As String   ' D0390
            Dim tMeetingID As Long = -1
            If TryParse(sMeetingID, tMeetingID) Then Return AsString(tMeetingID) Else Return ""
        End Function
        ' D0390 ==

        Public Shared Function TryParse(ByRef sMeetingID As String, ByRef tMeetingID As Long) As Boolean
            Dim sID As String = sMeetingID.Trim.ToLower.Replace("-", "")
            Dim ID As Long = -1
            Dim fResult As Boolean = Long.TryParse(sID, ID)
            If fResult Then tMeetingID = ID
            Return fResult
        End Function

    End Class

End Namespace