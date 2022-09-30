Namespace ExpertChoice.WebAPI

    ' D7209 ===
    <Serializable> Public Class jSessionStatus
        Inherits clsJsonObject

        <JsonProperty("sid")>
        Public Property ASPSessionID As String = ""

        <JsonProperty("usid")>
        Public Property UserSessionID As String = ""

        <JsonProperty("user")>
        Public Property User As jUserShort = Nothing

        <JsonProperty("wkg")>
        Public Property Workgroup As jWorkgroup = Nothing

        <JsonProperty("project")>
        Public Property Project As jProject = Nothing

        <JsonProperty("cmd")>
        Public Property Сmd As String = ""

        Overloads Shared Function CreateFromBaseObject(tApp As clsComparionCore, Session As HttpSessionState) As jSessionStatus
            If tApp IsNot Nothing Then
                ' D7536 ===
                Dim sCmd As String = ""
                If tApp.ActiveUser IsNot Nothing Then
                    Dim tSess As DateTime? = tApp.CheckSessionTerminate(tApp.ActiveUser)
                    If tSess.HasValue Then
                        Dim sDT As String = ""
                        If Session(_SESS_CMD_TERMINATE) IsNot Nothing Then sDT = CStr(Session(_SESS_CMD_TERMINATE))
                        If sDT <> Service.Date2ULong(tSess.Value).ToString Then
                            sCmd += "session_terminate"
                        End If
                    End If
                End If
                ' D7536 ==
                Return New jSessionStatus With {
                    .ASPSessionID = If(Session Is Nothing, "", Session.SessionID),
                    .UserSessionID = ExpertChoice.Service.StringFuncs.GetMD5(tApp.Options.SessionID),
                    .User = If(tApp.ActiveUser Is Nothing, Nothing, jUserShort.CreateFromBaseObject(tApp.ActiveUser)),
                    .Workgroup = If(tApp.ActiveWorkgroup Is Nothing, Nothing, jWorkgroup.CreateFromBaseObject(tApp.ActiveWorkgroup)),
                    .Project = If(tApp.HasActiveProject, jProject.GetProjectByID(tApp), Nothing),
                    .Сmd = sCmd}    ' D7356 + D7479 + D7644
                '.User = If(tApp.ActiveUser Is Nothing, New jUserShort With {.ID = -1}, jUserShort.CreateFromBaseObject(tApp.ActiveUser)),
                '    .Workgroup = If(tApp.ActiveWorkgroup Is Nothing, New jWorkgroup With {.ID = -1}, jWorkgroup.CreateFromBaseObject(tApp.ActiveWorkgroup)),
                '    .Project = If(tApp.HasActiveProject, jProject.CreateFromBaseObject(tApp, tApp.ActiveProject), New jProject With {.ID = -1})}
            Else
                Return Nothing
            End If
        End Function

    End Class
    ' D7209 ==

    <Serializable> Public Class jAppUserShort
        Inherits jUserShort

        Public Property HasPassword As Boolean = False

        Overloads Shared Function CreateFromBaseObject(tUser As clsApplicationUser) As jAppUserShort
            If tUser IsNot Nothing Then
                Return New jAppUserShort With {
                .ID = tUser.UserID,
                .Name = If(tUser.UserName = "", tUser.UserEmail, tUser.UserName),
                .Email = tUser.UserEmail,
                .HasPassword = tUser.HasPassword
            }
            Else
                Return Nothing ' D5033
            End If
        End Function

    End Class

    <Serializable> Public Class jOnlineUser
        Inherits clsJsonObject
        Public Property UserID As Integer = -1
        Public Property UserEmail As String = ""
        Public Property LastAccess As Date? = Nothing
        Public Property WorkgroupID As Integer = -1
        Public Property ProjectID As Integer = -1
        Public Property RoleGroupID As Integer = -1
        Public Property PageID As Integer = -1
        Public Property URL As String = ""
        Public Property SessionID As String = ""

        Public isPM As Boolean = False

        ''' <summary>
        ''' Please note: isPM property is not filling
        ''' </summary>
        ''' <param name="tSession"></param>
        ''' <returns></returns>
        Overloads Shared Function CreateFromBaseObject(tSession As clsOnlineUserSession) As jOnlineUser
            If tSession IsNot Nothing Then
                Return New jOnlineUser With {
                    .UserEmail = tSession.UserEmail,
                    .UserID = tSession.UserID,
                    .ProjectID = tSession.ProjectID,
                    .WorkgroupID = tSession.WorkgroupID,
                    .RoleGroupID = tSession.RoleGroupID,
                    .PageID = tSession.PageID,
                    .URL = tSession.URL,
                    .SessionID = tSession.SessionID,
                    .LastAccess = tSession.LastAccess
            }
            Else
                Return Nothing
            End If
        End Function

    End Class


End Namespace
