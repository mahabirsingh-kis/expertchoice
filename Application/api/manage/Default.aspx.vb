Imports System.Web.Script.Serialization

Partial Class ManageWebAPI
    Inherits clsComparionCorePage

    Public Const _LOGS_MAX_ROWS = 5000  ' D7602

    Public Class jsonSortSelector
        Public Property selector As String
        Public Property desc As Boolean
    End Class

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Private Function ParseFilterObject(filter As Object(), ByRef tParams As List(Of Object)) As String
        Dim resVal = ""
        Dim AllStrings As Boolean = True
        If filter.Count = 3 Then
            For Each item As Object In filter
                If item.GetType.Name = "Object[]" Then
                    AllStrings = False
                End If
            Next
            If AllStrings Then
                Dim fFieldName As String = CType(filter(0), String)
                If fFieldName = "WorkgroupID" Then fFieldName = "Logs.WorkgroupID"
                'If fFieldName = "UserName" Then fFieldName = "Users.FullName"
                If fFieldName = "ActionID" Then fFieldName = "Logs.ActionID"
                If fFieldName = "Comment" Then fFieldName = "Logs.Comment"
                If fFieldName = "Result" Then fFieldName = "Logs.Result"

                Dim fOperator As String = CType(filter(1), String)
                Dim fValue As String = CType(filter(2), String)
                Select Case fOperator
                    Case "contains"
                        fOperator = "LIKE"
                        fValue = "%" + fValue + "%"
                    Case "notcontains"
                        fOperator = "NOT LIKE"
                        fValue = "%" + fValue + "%"
                    Case "startswith"
                        fOperator = "LIKE"
                        fValue = fValue + "%"
                    Case "endswith"
                        fOperator = "LIKE"
                        fValue = "%" + fValue
                End Select
                resVal += String.Format("{0} {1} ?", fFieldName, fOperator)
                tParams.Add(fValue)
            End If
        End If
        If filter.Count <> 3 Or Not AllStrings Then
            For Each item As Object In filter
                If item.GetType.Name <> "Object[]" Then
                    resVal += " " + CType(item, String) + " "
                Else
                    resVal += "(" + ParseFilterObject(CType(item, Object()), tParams) + ")"
                End If
            Next
        End If
        Return resVal
    End Function

    Private Function LogEventsWhere(Optional ProjectID As Integer = -1, Optional WorkgroupID As Integer = -1, Optional filter As String = "", Optional ByRef tParams As List(Of Object) = Nothing) As String
        Dim sWhere As String = ""   ' D6635

        ' D6635 ===
        If WorkgroupID <= 0 AndAlso App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then
        Else
            If WorkgroupID <= 0 Then
                Dim WkgIDs As String = ""
                For Each tWkg As clsWorkgroup In App.AvailableWorkgroupsAsWM
                    WkgIDs += If(WkgIDs = "", "", ",") + CStr(tWkg.ID)
                Next
                sWhere += If(sWhere = "", "", " AND ") + String.Format("Logs.WorkgroupID IN ({0})", WkgIDs)
            Else
                sWhere += If(sWhere = "", "", " AND ") + String.Format("Logs.WorkgroupID = {0}", WorkgroupID)
            End If
        End If

        If filter <> "" Then
            Try
                Dim sqlFilter As String = ""
                Dim jss As New JavaScriptSerializer()
                Dim filterList As Object() = jss.Deserialize(Of Object())(filter)

                sqlFilter = ParseFilterObject(filterList, tParams)

                If sqlFilter <> "" Then sWhere += If(sWhere = "", "", " AND ") + sqlFilter
            Catch ex As Exception
            End Try
        End If

        If sWhere <> "" Then sWhere = " WHERE " + sWhere

        Return sWhere
    End Function

    Private Function LogEventsObjectName() As String
        Return "CASE Logs.TypeID WHEN 10 THEN Users.Email WHEN 20 THEN Projects.Passcode WHEN 21 THEN Projects.Passcode WHEN 53 THEN Workgroups.Name WHEN 56 THEN Workgroups.Name WHEN 51 THEN RoleGroups.Name WHEN 62 THEN Projects.Passcode ELSE CAST(Logs.ObjectID AS VARCHAR(10)) END"
    End Function

    Private Function DB_GetLogEvents(Optional ProjectID As Integer = -1, Optional WorkgroupID As Integer = -1, Optional Limit As Integer = 1000, Optional Skip As Integer = 0, Optional sort As String = "", Optional filter As String = "") As List(Of jLogEvent)  ' D6635
        Dim tList As New List(Of jLogEvent)

        Dim tParams As New List(Of Object)

        Dim SQL As String = String.Format("SELECT Logs.ID, Logs.DT AS DT, Logs.TypeID, Logs.ActionID, UserAction.Email AS UserEmail, Logs.WorkgroupID, Logs.Comment, Logs.Result, {0} ObjectName FROM Logs", LogEventsObjectName)
        SQL += " LEFT OUTER JOIN Users as UserAction ON Logs.UserID = UserAction.ID LEFT OUTER JOIN Users ON Logs.ObjectID = Users.ID AND Logs.ObjectID>0 LEFT OUTER JOIN Projects ON Logs.ObjectID = Projects.ID AND Logs.ObjectID>0 LEFT OUTER JOIN Workgroups ON Logs.ObjectID = Workgroups.ID AND Logs.ObjectID>0 LEFT OUTER JOIN RoleGroups ON Logs.ObjectID = RoleGroups.ID AND Logs.ObjectID>0"  ' D7179

        Dim sWhere As String = LogEventsWhere(ProjectID, WorkgroupID, filter, tParams)
        If sWhere.Contains("UserEmail") Then
            sWhere = sWhere.Replace("UserEmail", "UserAction.Email")
        End If
        If sWhere.Contains("ObjectName") Then
            sWhere = sWhere.Replace("ObjectName", LogEventsObjectName)
        End If
        SQL += sWhere

        If sort <> "" Then
            Try
                Dim jss As New JavaScriptSerializer()
                Dim sortList As List(Of jsonSortSelector) = jss.Deserialize(Of List(Of jsonSortSelector))(sort)
                If sortList.Count > 0 Then
                    SQL += " ORDER BY " + sortList(0).selector + If(sortList(0).desc, " DESC", "")
                Else
                    SQL += " ORDER BY Logs.ID DESC"
                End If
            Catch ex As Exception
                sort = ""
            End Try
        End If
        If sort = "" Then SQL += " ORDER BY Logs.ID DESC"

        SQL += String.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY; ", Skip, Limit)

        Dim tData As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(SQL, tParams)
        For Each tRow As Dictionary(Of String, Object) In tData
            Dim tEvent As New jLogEvent
            With tEvent
                If tRow.ContainsKey("ID") AndAlso Not IsDBNull(tRow("ID")) Then .ID = CInt(tRow("ID"))
                If tRow.ContainsKey("DT") AndAlso Not IsDBNull(tRow("DT")) Then .DT = CDate(tRow("DT"))
                If tRow.ContainsKey("ActionID") AndAlso Not IsDBNull(tRow("ActionID")) Then .ActionID = CInt(tRow("ActionID"))
                If tRow.ContainsKey("TypeID") AndAlso Not IsDBNull(tRow("TypeID")) Then .TypeID = CInt(tRow("TypeID"))
                'If tRow.ContainsKey("UserName") AndAlso Not IsDBNull(tRow("UserName")) Then .UserName = CStr(tRow("UserName"))
                If tRow.ContainsKey("UserEmail") AndAlso Not IsDBNull(tRow("UserEmail")) Then .UserEmail = CStr(tRow("UserEmail"))
                If tRow.ContainsKey("WorkgroupID") AndAlso Not IsDBNull(tRow("WorkgroupID")) Then .WorkgroupID = CInt(tRow("WorkgroupID"))
                If tRow.ContainsKey("ObjectName") AndAlso Not IsDBNull(tRow("ObjectName")) Then .ObjectName = ParseString(CStr(tRow("ObjectName"))) ' D7179
                If tRow.ContainsKey("Comment") AndAlso Not IsDBNull(tRow("Comment")) Then .Comment = ParseString(CStr(tRow("Comment"))) ' D7179
                If tRow.ContainsKey("Result") AndAlso Not IsDBNull(tRow("Result")) Then .Result = ParseString(CStr(tRow("Result")))    ' D7179
            End With
            tList.Add(tEvent)
        Next

        Return tList
    End Function

    Private Function DB_GetLogEventsCount(Optional ProjectID As Integer = -1, Optional WorkgroupID As Integer = -1, Optional filter As String = "") As Integer
        Dim tParams As New List(Of Object)

        Dim SQL As String = "SELECT COUNT(Logs.ID) AS Total FROM Logs"

        Dim sWhere As String = LogEventsWhere(ProjectID, WorkgroupID, filter, tParams)
        If sWhere.Contains("UserEmail") Then
            SQL += " LEFT OUTER JOIN Users as UserAction ON Logs.UserID = UserAction.ID"
            sWhere = sWhere.Replace("UserEmail", "UserAction.Email")
        End If
        If sWhere.Contains("ObjectName") Then
            SQL += " LEFT OUTER JOIN Users ON Logs.ObjectID = Users.ID LEFT OUTER JOIN Projects ON Logs.ObjectID = Projects.ID AND Logs.ObjectID>0 LEFT OUTER JOIN Workgroups ON Logs.ObjectID = Workgroups.ID AND Logs.ObjectID>0 LEFT OUTER JOIN RoleGroups ON Logs.ObjectID = RoleGroups.ID AND Logs.ObjectID>0" ' D7179
            sWhere = sWhere.Replace("ObjectName", LogEventsObjectName)
        End If
        SQL += sWhere

        Return CInt(App.Database.ExecuteScalarSQL(SQL, tParams))
    End Function

    Private Function DB_GetLogEventsFilter(Optional ProjectID As Integer = -1, Optional WorkgroupID As Integer = -1, Optional filter As String = "", Optional dataField As String = "") As List(Of jLogEvent)  ' D6635
        Dim tList As New List(Of jLogEvent)

        Dim tParams As New List(Of Object)

        If dataField <> "" Then
            Dim SQL As String = ""
            Dim supportedDataField = False
            If dataField = "DT" Then
                SQL = "SELECT DISTINCT convert(varchar, Logs.DT, 23) as DT "
                supportedDataField = True
            End If
            If supportedDataField Then
                'SQL += " CASE Logs.TypeID WHEN 10 THEN Users_1.Email WHEN 20 THEN Projects.Passcode WHEN 21 THEN Projects.Passcode WHEN 53 THEN Workgroups_1.Name WHEN 56 THEN Workgroups_1.Name WHEN 51 THEN RoleGroups.Name WHEN 62 THEN Projects.Passcode ELSE CAST(Logs.ObjectID AS VARCHAR(10)) END ObjectName "
                SQL += " FROM Logs "
                SQL += " LEFT OUTER JOIN Users ON Logs.UserID = Users.ID  "
                SQL += " LEFT OUTER JOIN Workgroups ON Logs.WorkgroupID = Workgroups.ID "
                SQL += " LEFT OUTER JOIN Users AS Users_1 ON Logs.ObjectID = Users_1.ID "
                SQL += " LEFT OUTER JOIN Projects ON Logs.ObjectID = Projects.ID "
                SQL += " LEFT OUTER JOIN Workgroups AS Workgroups_1 ON Logs.ObjectID = Workgroups_1.ID "
                SQL += " LEFT OUTER JOIN RoleGroups ON Logs.ObjectID = RoleGroups.ID "
                SQL += LogEventsWhere(ProjectID, WorkgroupID, filter, tParams)
                Dim tData As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(SQL, tParams)
                For Each tRow As Dictionary(Of String, Object) In tData
                    Dim tEvent As New jLogEvent
                    With tEvent
                        If tRow.ContainsKey("ID") AndAlso Not IsDBNull(tRow("ID")) Then .ID = CInt(tRow("ID"))
                        If tRow.ContainsKey("DT") AndAlso Not IsDBNull(tRow("DT")) Then .DT = CDate(tRow("DT"))
                        If tRow.ContainsKey("ActionID") AndAlso Not IsDBNull(tRow("ActionID")) Then .ActionID = CInt(tRow("ActionID"))
                        If tRow.ContainsKey("TypeID") AndAlso Not IsDBNull(tRow("TypeID")) Then .TypeID = CInt(tRow("TypeID"))
                        'If tRow.ContainsKey("UserName") AndAlso Not IsDBNull(tRow("UserName")) Then .UserName = CStr(tRow("UserName"))
                        If tRow.ContainsKey("UserEmail") AndAlso Not IsDBNull(tRow("UserEmail")) Then .UserEmail = CStr(tRow("UserEmail"))
                        If tRow.ContainsKey("WorkgroupID") AndAlso Not IsDBNull(tRow("WorkgroupID")) Then .WorkgroupID = CInt(tRow("WorkgroupID"))
                        If tRow.ContainsKey("ObjectName") AndAlso Not IsDBNull(tRow("ObjectName")) Then .ObjectName = ParseString(CStr(tRow("ObjectName")))    ' D7179
                        If tRow.ContainsKey("Comment") AndAlso Not IsDBNull(tRow("Comment")) Then .Comment = ParseString(CStr(tRow("Comment"))) ' D7179
                        If tRow.ContainsKey("Result") AndAlso Not IsDBNull(tRow("Result")) Then .Result = ParseString(CStr(tRow("Result"))) ' D7179
                    End With
                    tList.Add(tEvent)
                Next
            End If
        End If
        Return tList
    End Function

    ' D5033 ===
    ''' <summary>
    ''' Get list of system log events using sorting and filter options
    ''' </summary>
    ''' <param name="Limit">Use this parameter to limit number of items, default value is 1000</param>
    ''' <param name="Skip">Use it to skip first records to get next portion (useful for pagination), default value is 0</param>
    ''' <param name="WkgID">Use this parameter to specify workgroup, use default value -1 to show events for all workgroups</param>
    ''' <param name="sort">Use this parameter in form of jsonSortSelector {selector:"fieldName", desc: true}, to sort log events</param>
    ''' <param name="filter"></param>
    ''' <param name="dataField"></param>
    ''' <returns></returns>
    Public Function Logs_System(Optional Limit As Integer = 1000, Optional Skip As Integer = 0, Optional WkgID As Integer = -1, Optional sort As String = "", Optional filter As String = "", Optional dataField As String = "") As jActionResult    ' D6635
        FetchIfNotAuthorized()
        Dim Res As New jActionResult
        If App.CanUserDoAction(ecActionType.at_slViewAnyWorkgroupLogs, App.ActiveUserWorkgroup, App.ActiveWorkgroup) OrElse App.CanUserDoAction(ecActionType.at_slViewOwnWorkgroupLogs, App.ActiveUserWorkgroup, App.ActiveWorkgroup) OrElse App.CanUserDoAction(ecActionType.at_alManageWorkgroupUsers, App.ActiveUserWorkgroup, App.ActiveWorkgroup) Then    ' D6635
            Res.Result = ecActionResult.arSuccess
            Res.ObjectID = DB_GetLogEventsCount(, WkgID, filter)
            If dataField = "" Then
                Res.Data = DB_GetLogEvents(, WkgID, Limit, Skip, sort, filter)   ' D6635
            Else
                Res.Data = DB_GetLogEventsFilter(, WkgID, filter, dataField)
            End If

        Else
                FetchNoPermissions()
        End If
        Return Res
        ' D5033 ==
    End Function

    Private Sub ManageWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        Select Case _Page.Action

            Case "logs_system"
                ' D6635 ===
                Dim Limit As Integer
                If Not Integer.TryParse(GetParam(_Page.Params, "take", True), Limit) OrElse Limit < 1 Then Limit = _LOGS_MAX_ROWS
                If Limit > _LOGS_MAX_ROWS Then Limit = _LOGS_MAX_ROWS

                Dim Skip As Integer
                If Not Integer.TryParse(GetParam(_Page.Params, "skip", True), Skip) Then Skip = 0

                Dim wkgID As Integer
                If Not Integer.TryParse(GetParam(_Page.Params, "wkgid", True), wkgID) Then wkgID = -1 ' App.ActiveWorkgroup.ID

                Dim jSort As String = GetParam(_Page.Params, "sort", True)

                Dim jFilter As String = GetParam(_Page.Params, "filter", True)
                'jFilter = jFilter.Replace("[", "").Replace("]", "").Replace("""", "")

                Dim dataField As String = GetParam(_Page.Params, "dataField", True)
                'TODO: add "last_dt/last_id" for check only new events
                _Page.ResponseData = Logs_System(Limit, Skip, wkgID, jSort, jFilter, dataField)
                ' D6635 ==

        End Select
    End Sub

End Class