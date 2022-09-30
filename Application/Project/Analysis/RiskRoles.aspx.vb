Imports System.Drawing
Imports Telerik.Web.UI

Partial Class RiskRolesPage
    Inherits clsComparionCorePage

    Public Const COL_ID As Integer = 0 'not visible
    Public Const COL_TYPE As Integer = 1 'not visible, grouping
    Public Const COL_CHK As Integer = 2
    Public Const COL_NAME As Integer = 3
    Public Const COL_ROLES_START_IDX As Integer = 4

    Public Const HIDDEN_COLUMN_COUNT As Integer = 2

    Public Sub New()
        MyBase.New(_PGID_RISK_ROLES)
    End Sub

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    ReadOnly Property SESS_TREATMENTS As String
        Get
            Return String.Format("RISK_TREATMENTS_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public _Treatments As List(Of clsControl) = Nothing
    Public Property Treatments As List(Of clsControl)
        Get
            If _Treatments Is Nothing Then
                Dim tSessVar = Session(SESS_TREATMENTS)
                If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is List(Of clsControl) Then
                    _Treatments = CType(tSessVar, List(Of clsControl))
                End If
            End If

            Dim res As List(Of clsControl) = New List(Of clsControl)

            Dim _GridFilter As Integer = GridFilter
            For Each control As clsControl In _Treatments
                If _GridFilter = -1 OrElse _GridFilter = CInt(control.Type) Then
                    res.Add(control)
                End If
            Next

            Return res
        End Get
        Set(value As List(Of clsControl))
            _Treatments = value.Where(Function (ctrl) ctrl.Type <> ControlType.ctUndefined).ToList
            Session(SESS_TREATMENTS) = value
        End Set
    End Property

    ReadOnly Property SESS_USERS As String
        Get
            Return String.Format("RISK_USERS_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public _AppUsers As List(Of clsApplicationUser) = Nothing
    Public Property AppUsers As List(Of clsApplicationUser)
        Get
            If _AppUsers Is Nothing Then
                Dim tSessVar = Session(SESS_USERS)
                If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is List(Of clsApplicationUser) Then
                    _AppUsers = CType(tSessVar, List(Of clsApplicationUser))
                End If
            End If
            Return _AppUsers
        End Get
        Set(value As List(Of clsApplicationUser))
            _AppUsers = value
            Session(SESS_USERS) = value
        End Set
    End Property

    Public Property GridFilter As Integer
        Get
            Dim retVal As Integer = -1
            Dim s As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_ROLES_CONTROLS_TYPE_ID, UNDEFINED_USER_ID))
            If Not String.IsNullOrEmpty(s) Then retVal = CInt(s)
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_RISK_ROLES_CONTROLS_TYPE_ID, AttributeValueTypes.avtLong, value.ToString, "")
        End Set
    End Property

    Function CreateDataSource() As DataView 'ICollection      
        Dim dt As New DataTable()
        Dim dr As DataRow

        dt.Columns.Add(New DataColumn("Id", GetType(Guid))) ' hidden
        dt.Columns.Add(New DataColumn("Type", GetType(Integer))) ' hidden, group by this
        dt.Columns.Add(New DataColumn("Chk", GetType(String))) ' check box
        dt.Columns.Add(New DataColumn(ResString("tblControlName"), GetType(String))) 'treatment name

        'add columns for all users
        AppUsers = App.DBUsersByProjectID(App.ProjectID)
        Dim tWSList As List(Of clsWorkspace) = App.DBWorkspacesByProjectID(App.ProjectID)

        AppUsers.Sort(New clsApplicationUserComparer(ecApplicationUserSort.usEmail, SortDirection.Ascending))

        For Each usr As clsUser In AppUsers
            Dim tPrjUser As clsUser = PM.GetUserByEMail(usr.UserEMail)
            Dim UserName As String = usr.UserName
            If tPrjUser IsNot Nothing Then UserName = tPrjUser.UserName
            If String.IsNullOrEmpty(UserName.Trim) Then UserName = usr.UserEMail Else UserName = String.Format("{0}({1})", UserName, usr.UserEMail)
            'TODO: check if the column with the same name already exists            
            dt.Columns.Add(UserName, GetType(String))
        Next

        Dim CurType As Integer = -1

        For Each ctrl As clsControl In Treatments
            'Add summary row
            If ctrl.Type <> CurType Then
                CurType = ctrl.Type
                dr = dt.NewRow()
                dr(COL_NAME) = ctrl.Type.ToString
                Select Case ctrl.Type
                    Case ControlType.ctCause
                        dr(COL_NAME) = ParseString("%%Controls%% for %%Sources%%")
                    Case ControlType.ctCauseToEvent
                        dr(COL_NAME) = ParseString("%%Controls%% for %%Vulnerabilities%%")
                    Case ControlType.ctConsequenceToEvent
                        dr(COL_NAME) = ParseString("%%Controls%% for %%Impacts%%")
                    Case ControlType.ctConsequence
                        dr(COL_NAME) = "Treatments for Consequences - OBSOLETE"
                    Case ControlType.ctEvent
                        dr(COL_NAME) = "Treatments for Event - OBSOLETE"
                    Case ControlType.ctUndefined
                        dr(COL_NAME) = "Undefined"
                End Select
                dr(COL_ID) = Guid.Empty
                dr(COL_TYPE) = CInt(ctrl.Type)
                dt.Rows.Add(dr)
            End If

            dr = dt.NewRow()
            dr(COL_ID) = ctrl.ID
            dr(COL_TYPE) = CInt(ctrl.Type)
            dr(COL_CHK) = False
            dr(COL_NAME) = ctrl.Name

            For i As Integer = 0 To AppUsers.Count - 1
                Dim usr As clsUser = CType(AppUsers(i), clsUser)
                Dim tPrjUser As clsUser = PM.GetUserByEMail(usr.UserEMail)
                If tPrjUser IsNot Nothing Then
                    dr(COL_ROLES_START_IDX + i) = PM.ControlsRoles.IsAllowedObjective(ctrl.ID, tPrjUser.UserID)
                End If
            Next

            dt.Rows.Add(dr)
        Next

        Dim dv As New DataView(dt)
        Return dv
    End Function

    Private Sub BuildGridViewControl()
        Dim DS As DataView = CreateDataSource()

        GridControls.DataSource = DS
        GridControls.DataBind()
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack AndAlso Not IsCallback Then BuildGridViewControl()
    End Sub

    Protected Sub GridControls_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles GridControls.RowDataBound
        If e.Row.RowType = DataControlRowType.Header OrElse e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Cells(COL_ID).Visible = False
            e.Row.Cells(COL_TYPE).Visible = False

            If e.Row.RowType = DataControlRowType.Header Then
                e.Row.Cells(COL_CHK).Text = "&nbsp;"
                For i As Integer = COL_ROLES_START_IDX To e.Row.Cells.Count - 1
                    e.Row.Cells(i).ToolTip = e.Row.Cells(i).Text
                    Dim HeaderText As String = "<div style='width: 100px; overflow: hidden; white-space: wrap; text-overflow: ellipsis;'>" + e.Row.Cells(i).Text + "</div>"
                    e.Row.Cells(i).Text = "<label><input type='checkbox' onClick='sendCommand(""action=allowcol&uidx=" + (i - COL_ROLES_START_IDX).ToString + "&chk=""+(this.checked))'" + CStr(IIf(IsColAllowed(i - COL_ROLES_START_IDX), " checked", "")) + " />" + HeaderText + "</label>"
                    e.Row.Cells(i).VerticalAlign = VerticalAlign.Top
                Next
                e.Row.Height = 40
            End If

            If e.Row.DataItem IsNot Nothing Then
                Dim tRow As DataRowView = CType(e.Row.DataItem, DataRowView)
                Dim ctrlID As Guid = CType(tRow(COL_ID), Guid)
                If ctrlID.Equals(Guid.Empty) Then 'summary row
                    Dim SummaryText As String = CStr(tRow(COL_NAME))
                    Dim SpanCount As Integer = e.Row.Cells.Count
                    e.Row.Cells.Clear()
                    Dim cell As TableCell = New TableCell
                    cell.ColumnSpan = SpanCount
                    cell.Text = SummaryText
                    cell.BackColor = Color.LightGray
                    cell.ForeColor = Color.FromArgb(60, 90, 150)
                    cell.Font.Bold = True
                    cell.Height = 30
                    e.Row.Cells.Add(cell)
                Else
                    e.Row.Cells(COL_CHK).HorizontalAlign = HorizontalAlign.Center
                    e.Row.Cells(COL_CHK).Text = "<label><input type='checkbox' onClick='sendCommand(""action=allowrow&id=" + tRow(COL_ID).ToString + "&chk=""+(this.checked))'" + CStr(IIf(IsRowAllowed(CType(tRow(COL_ID), Guid)), " checked", "")) + " />&nbsp;</label>"
                    'e.Row.Cells(COL_NAME).Text = "<div style='width: 300px; overflow: hidden; white-space: wrap; text-overflow: ellipsis;'>" + e.Row.Cells(COL_NAME).Text + "</div>"
                    Dim sName As String = JS_SafeString(e.Row.Cells(COL_NAME).Text)
                    'Dim sDescr As String = JS_SafeString(GetControlByID(ctrlID).InfoDoc)
                    Dim sDescr As String = JS_SafeString(HTML2Text(GetControlInfodoc(PRJ, GetControlByID(ctrlID), False)))  ' D4345
                    e.Row.Cells(COL_NAME).Text = "<table cellpadding='0' cellspacing='0' class='text' style='width:100%; background-color:transparent;'>" + _
                    "<tr><td align='left' style='padding-left:3px; padding-top:3px; background-color:transparent;'><span title='" + sName + "' style='width: 300px; overflow: hidden; white-space: wrap; text-overflow: ellipsis;'>" + sName + "</span></td></tr>" + _
                    "<tr><td align='left' style='padding-top:1px; padding-left:3px; padding-bottom:3px; background-color:transparent;'><small title='" + sDescr + "' style='overflow: hidden; white-space: wrap; text-overflow: ellipsis;'>" + sDescr + "</small></td></tr></table>"
                    Dim Columns As DataColumnCollection = CType(GridControls.DataSource, DataView).Table.Columns
                    Dim TreatmentName As String = ""
                    Dim Treatment As clsControl = PM.Controls.GetControlByID(ctrlID)
                    If Treatment IsNot Nothing Then TreatmentName = Treatment.Name

                    For i As Integer = COL_ROLES_START_IDX To e.Row.Cells.Count - 1
                        Dim tt As String = String.Format("<b>{0}</b> : <i>{1}</i>", Columns(i).Caption, TreatmentName) 'tooltip
                        e.Row.Cells(i).ToolTip = tt
                        e.Row.Cells(i).Text = "<label><input type='checkbox' onClick='sendCommand(""action=allowcell&id=" + ctrlID.ToString + "&uidx=" + (i - COL_ROLES_START_IDX).ToString + "&chk=""+(this.checked))'" + CStr(IIf(e.Row.Cells(i).Text = "True", " checked", "")) + " />&nbsp;</label>"
                        e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Center
                    Next

                    e.Row.Attributes.Remove("onmouseover")
                    e.Row.Attributes.Add("onmouseover", String.Format("RowHover(this,1,{0});", IIf(e.Row.RowState = DataControlRowState.Alternate, 1, 0)))
                    e.Row.Attributes.Remove("onmouseout")
                    e.Row.Attributes.Add("onmouseout", String.Format("RowHover(this,0,{0});", IIf(e.Row.RowState = DataControlRowState.Alternate, 1, 0)))
                End If
            End If
        End If
    End Sub

    Private Function IsRowAllowed(ControlID As Guid) As Boolean
        Dim retVal As Boolean = True
        If DataExists() Then
            Dim j As Integer = 0
            While retVal AndAlso j < AppUsers.Count
                Dim usr As clsUser = CType(AppUsers(j), clsUser)
                Dim tPrjUser As clsUser = PM.GetUserByEMail(usr.UserEMail)
                If tPrjUser IsNot Nothing Then
                    Dim role As Boolean = PM.ControlsRoles.IsAllowedObjective(ControlID, tPrjUser.UserID)
                    If Not role Then
                        retVal = False
                    End If
                End If
                j += 1
            End While
        End If
        Return retVal
    End Function

    Private Function IsColAllowed(UserIdx As Integer) As Boolean
        Dim retVal As Boolean = True
        If DataExists() Then
            Dim i As Integer = 0
            Dim usr As clsUser = CType(AppUsers(UserIdx), clsUser)
            Dim tPrjUser As clsUser = PM.GetUserByEMail(usr.UserEMail)
            If tPrjUser IsNot Nothing Then
                While retVal AndAlso i < Treatments.Count
                    Dim role As Boolean = PM.ControlsRoles.IsAllowedObjective(Treatments(i).ID, tPrjUser.UserID)
                    If Not role Then
                        retVal = False
                    End If
                    i += 1
                End While
            End If
        End If
        Return retVal
    End Function

    Public Function DataExists() As Boolean
        Return Treatments IsNot Nothing AndAlso Treatments.Count > 0 AndAlso AppUsers IsNot Nothing AndAlso AppUsers.Count > 0
    End Function

    Public Function IsAllAllowed(Optional isDropped As Boolean = False) As Boolean
        Dim retVal As Boolean = True
        If DataExists() Then

            Dim i As Integer = 0
            While retVal AndAlso i < Treatments.Count

                Dim j As Integer = 0
                While retVal AndAlso j < AppUsers.Count
                    Dim usr As clsUser = CType(AppUsers(j), clsUser)
                    Dim tPrjUser As clsUser = PM.GetUserByEMail(usr.UserEMail)
                    If tPrjUser IsNot Nothing Then
                        Dim role As Boolean = PM.ControlsRoles.IsAllowedObjective(Treatments(i).ID, tPrjUser.UserID)
                        If (Not isDropped AndAlso Not role) OrElse (isDropped AndAlso role) Then
                            retVal = False
                        End If
                    End If
                    j += 1
                End While

                i += 1

            End While
        End If
        Return retVal
    End Function

    Public Function SetPermission(ControlIDs As List(Of Guid), UserIDs As List(Of Integer), IsAllowed As Boolean, Optional ModifyAllRoles As Boolean = False) As Boolean
        Dim _GridFilter As Integer = GridFilter

        Dim retVal As Boolean = False
        With PM
            If ModifyAllRoles Then
                '"Allow All" or "Drop All"
                ControlIDs = New List(Of Guid)
                For Each control As clsControl In .Controls.Controls
                    If _GridFilter = -1 OrElse _GridFilter = CInt(control.Type) Then
                        ControlIDs.Add(control.ID)
                    End If
                Next

                For Each User As clsUser In .UsersList
                    .ControlsRoles.ClearUserObjectivesRoles(User.UserID)
                    .ControlsRoles.SetObjectivesRoles(User.UserID, ControlIDs, CType(IIf(IsAllowed, RolesValueType.rvtAllowed, RolesValueType.rvtRestricted), RolesValueType))
                    .StorageManager.Writer.SaveUserControlsPermissions(User.UserID)
                Next
            Else
                'set specific permissions
                For Each userID As Integer In UserIDs
                    For Each ctrlID In ControlIDs
                        Dim control As clsControl = .Controls.GetControlByID(ctrlID)
                        If control IsNot Nothing Then
                            .ControlsRoles.SetObjectivesRoles(userID, control.ID, CType(IIf(IsAllowed, RolesValueType.rvtAllowed, RolesValueType.rvtRestricted), RolesValueType))
                        End If
                    Next
                    .StorageManager.Writer.SaveUserControlsPermissions(userID)
                Next
            End If
            Dim LJT As DateTime 'A1042
            .StorageManager.Reader.LoadUserJudgmentsControls(LJT) 'A1042
            For Each control As clsControl In .Controls.Controls
                For Each assignment In control.Assignments
                    assignment.Value = .Controls.GetCombinedEffectivenessValue(assignment.Judgments, control.ID, assignment.Value)
                Next
            Next
            .Controls.WriteControls(ECModelStorageType.mstCanvasStreamDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
        End With

        Return retVal
    End Function

    Private Function GetControlByID(ID As Guid) As clsControl
        For Each ctrl In Treatments
            If ctrl.ID.Equals(ID) Then Return ctrl
        Next
        Return Nothing
    End Function

    Protected Sub RadAjaxManagerMain_AjaxRequest(sender As Object, e As AjaxRequestEventArgs) Handles RadAjaxManagerMain.AjaxRequest
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(e.Argument)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower()   ' Anti-XSS
        With PM
            Select Case sAction
                Case "allow_all"
                    SetPermission(Nothing, Nothing, True, True)
                Case "drop_all"
                    SetPermission(Nothing, Nothing, False, True)
                Case "allowcol"
                    Dim ControlIDs As New List(Of Guid)
                    For Each ctrl In Treatments
                        ControlIDs.Add(ctrl.ID)
                    Next
                    Dim UserIDs As New List(Of Integer)
                    Dim tPrjUser As clsUser = PM.GetUserByEMail(AppUsers(CInt(GetParam(args, "uidx"))).UserEmail)
                    If tPrjUser IsNot Nothing Then UserIDs.Add(tPrjUser.UserID)
                    Dim Chk As Boolean = (GetParam(args, "chk").ToLower = "true") OrElse (GetParam(args, "chk").ToLower = "1")
                    If ControlIDs.Count > 0 AndAlso UserIDs.Count > 0 Then
                        SetPermission(ControlIDs, UserIDs, Chk, False)
                    End If
                Case "allowrow"
                    Dim ControlIDs As New List(Of Guid)
                    ControlIDs.Add(New Guid(GetParam(args, "id")))
                    Dim UserIDs As New List(Of Integer)
                    For Each usr In AppUsers
                        Dim tPrjUser As clsUser = PM.GetUserByEMail(usr.UserEmail)
                        If tPrjUser IsNot Nothing Then UserIDs.Add(tPrjUser.UserID)
                    Next
                    Dim Chk As Boolean = (GetParam(args, "chk").ToLower = "true") OrElse (GetParam(args, "chk").ToLower = "1")
                    If ControlIDs.Count > 0 AndAlso UserIDs.Count > 0 Then
                        SetPermission(ControlIDs, UserIDs, Chk, False)
                    End If
                Case "allowcell"
                    Dim ControlIDs As New List(Of Guid)
                    ControlIDs.Add(New Guid(GetParam(args, "id")))
                    Dim UserIDs As New List(Of Integer)
                    Dim tPrjUser As clsUser = PM.GetUserByEMail(AppUsers(CInt(GetParam(args, "uidx"))).UserEmail)
                    If tPrjUser IsNot Nothing Then UserIDs.Add(tPrjUser.UserID)
                    Dim Chk As Boolean = (GetParam(args, "chk").ToLower = "true") OrElse (GetParam(args, "chk").ToLower = "1")
                    If ControlIDs.Count > 0 AndAlso UserIDs.Count > 0 Then
                        SetPermission(ControlIDs, UserIDs, Chk, False)
                    End If
                Case "grid_filter"
                    GridFilter = CInt(GetParam(args, "val").Trim)
            End Select
        End With
        BuildGridViewControl()
    End Sub

    Protected Sub divAllAllowed_PreRender(sender As Object, e As EventArgs) Handles divAllAllowed.PreRender
        divAllAllowed.InnerText = CStr(IIf(IsAllAllowed(), "1", "0"))
    End Sub

    Protected Sub divAllDropped_PreRender(sender As Object, e As EventArgs) Handles divAllDropped.PreRender
        divAllDropped.InnerText = CStr(IIf(IsAllAllowed(True), "1", "0"))
    End Sub

    Private Sub RiskRolesPage_Init(sender As Object, e As EventArgs) Handles Me.Init
        ControlsListReset()
        Treatments = ControlsList
    End Sub

End Class