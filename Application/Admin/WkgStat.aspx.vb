Imports System.Drawing

Partial Class WorkgroupsStatisticPage
    Inherits clsComparionCorePage

    Const _Sess_WkgStat As String = "_SessWkgStat"
    Private _data As List(Of clsWkgStat) = Nothing

    ' D2034 ===
    Private Const _SYN_START As String = ""
    Private Const _SYN_DELIM As String = vbTab
    Private Const _SYN_END As String = ""
    ' D2034 ==

    Public Sub New()
        MyBase.New(_PGID_ADMIN_LOGINS_STAT)
    End Sub

    Public ReadOnly Property Data() As List(Of clsWkgStat)
        Get
            If _data Is Nothing Then
                If Session(_Sess_WkgStat) Is Nothing Then
                    _data = New List(Of clsWkgStat)

                    ' D2034 ===
                    Dim tWkgTotal As New clsWkgStat ' D2034
                    tWkgTotal.WorkgroupName = "Total"
                    tWkgTotal.WorkgroupID = -1
                    If App.SystemWorkgroup IsNot Nothing Then
                        tWkgTotal.Created = App.SystemWorkgroup.Created
                        tWkgTotal.Expiration = Date.FromBinary(App.SystemWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.ExpirationDate))
                        If tWkgTotal.Expiration.HasValue AndAlso tWkgTotal.Expiration >= BinaryStr2DateTime(UNLIMITED_DATE.ToString) Then tWkgTotal.Expiration = Nothing ' D2221
                        Dim tSID As Long = -1
                        If Not App.SystemWorkgroup.License.CheckAllParameters(tSID) Then
                            tWkgTotal.Status = App.LicenseErrorMessage(App.SystemWorkgroup.License, CType(tSID, ecLicenseParameter))
                        End If
                    End If
                    ' D2034 ==

                    Dim WkgList As List(Of clsWorkgroup) = App.DBWorkgroupsAll(False, False)
                    For Each tWorkgroup As clsWorkgroup In WkgList
                        'If tWorkgroup.Status <> ecWorkgroupStatus.wsSystem AndAlso tWorkgroup.Name <> _DB_DEFAULT_STARTUPWORKGROUP_NAME Then   ' -D2221
                        Dim Wkg As New clsWkgStat()
                        Wkg.WorkgroupID = tWorkgroup.ID
                        Wkg.WorkgroupName = tWorkgroup.Name
                        Wkg.Comment = tWorkgroup.Comment
                        Wkg.LastVisited = tWorkgroup.LastVisited
                        If tWorkgroup.LastVisited.HasValue AndAlso (Not tWkgTotal.LastVisited.HasValue OrElse tWkgTotal.LastVisited.Value < Wkg.LastVisited.Value) Then tWkgTotal.LastVisited = Wkg.LastVisited ' D2034
                        Wkg.WorkgroupManager = ""
                        Wkg.Created = tWorkgroup.Created
                        If tWorkgroup.ECAMID > 0 Then Wkg.WorkgroupManager = App.OwnerEmail(tWorkgroup.ECAMID) Else Wkg.WorkgroupManager = App.OwnerEmail(tWorkgroup.OwnerID)
                        Wkg.ProjectsCount = App.DBProjectsByWorkgroupID(tWorkgroup.ID).Count
                        tWkgTotal.ProjectsCount += Wkg.ProjectsCount    ' D2034
                        Dim Exp As Long = tWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.ExpirationDate)    ' D0913 + D1096
                        If Exp <> LICENSE_NOVALUE AndAlso Exp < UNLIMITED_DATE Then Wkg.Expiration = DateTime.FromBinary(Exp) Else Wkg.Expiration = Nothing ' D2221
                        Dim tID As Long = -1
                        If Not tWorkgroup.License.CheckAllParameters(tID) Then
                            Wkg.Status = App.LicenseErrorMessage(tWorkgroup.License, CType(tID, ecLicenseParameter))
                        End If
                        If tWorkgroup.Status = ecWorkgroupStatus.wsDisabled Then Wkg.Status += CType(IIf(Wkg.Status = "", "", ", "), String) + "Disabled"
                        _data.Add(Wkg)
                        'End If
                    Next

                    Dim sSQL As String = "SELECT COUNT(ID) AS Cnt, WorkgroupID FROM Logs WHERE WorkgroupID>0 GROUP BY WorkgroupID"
                    Dim Rows As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(sSQL)
                    For Each tRow As Dictionary(Of String, Object) In Rows
                        Dim tWkgID As Integer = CInt(tRow("WorkgroupID"))
                        Dim tWkg As clsWkgStat = clsWkgStat.ItemByWkgID(tWkgID, _data)
                        If tWkg IsNot Nothing Then
                            tWkg.ActivityTotal = CInt(tRow("Cnt"))
                            tWkgTotal.ActivityTotal += tWkg.ActivityTotal   ' D2034
                        End If
                    Next

                    ' D2487 ===
                    sSQL = "SELECT COUNT(ID) AS Cnt, WorkgroupID FROM Projects WHERE Created>DATEADD(month, -1, GETDATE()) GROUP BY WorkgroupID"
                    Rows = App.Database.SelectBySQL(sSQL)
                    For Each tRow As Dictionary(Of String, Object) In Rows
                        Dim tWkgID As Integer = CInt(tRow("WorkgroupID"))
                        Dim tWkg As clsWkgStat = clsWkgStat.ItemByWkgID(tWkgID, _data)
                        If tWkg IsNot Nothing Then
                            tWkg.ProjectsCreated = CInt(tRow("Cnt"))
                        End If
                    Next
                    ' D2487 ==

                    For i As Integer = 0 To 5
                        Dim Dt As Date = Now().AddMonths(-i)
                        sSQL = String.Format("SELECT COUNT(ID) AS Cnt, WorkgroupID FROM Logs WHERE (WorkgroupID>0) AND (MONTH(DT)={0}) AND (YEAR(DT)={1}) GROUP BY WorkgroupID", Dt.ToString("MM"), Dt.ToString("yyyy"))
                        Rows = App.Database.SelectBySQL(sSQL)
                        For Each tRow As Dictionary(Of String, Object) In Rows
                            Dim tWkgID As Integer = CInt(tRow("WorkgroupID"))
                            Dim tWkg As clsWkgStat = clsWkgStat.ItemByWkgID(tWkgID, _data)
                            If tWkg IsNot Nothing Then
                                Dim tCnt As Integer = CInt(tRow("Cnt"))
                                Select Case i
                                    Case 0
                                        tWkg.ActivityCurrent = tCnt
                                        tWkgTotal.ActivityCurrent += tCnt ' D2034
                                    Case 1
                                        tWkg.Activity1 = tCnt
                                        tWkgTotal.Activity1 += tCnt ' D2034
                                    Case 2
                                        tWkg.Activity2 = tCnt
                                        tWkgTotal.Activity2 += tCnt ' D2034
                                    Case 3
                                        tWkg.Activity3 = tCnt
                                        tWkgTotal.Activity3 += tCnt ' D2034
                                    Case 4
                                        tWkg.Activity4 = tCnt
                                        tWkgTotal.Activity4 += tCnt ' D2034
                                    Case 5
                                        tWkg.Activity5 = tCnt
                                        tWkgTotal.Activity5 += tCnt ' D2034
                                End Select
                            End If
                        Next
                    Next

                    tWkgTotal.Created = Nothing ' D2221
                    _data.Add(tWkgTotal) ' D2034

                    Session(_Sess_WkgStat) = _data
                Else
                    Try
                        _data = CType(Session(_Sess_WkgStat), List(Of clsWkgStat))
                    Catch ex As Exception
                        Session(_Sess_WkgStat) = Nothing
                    End Try
                End If
            End If
            Return _data
        End Get
    End Property

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignHorizontalCenter = False
        AlignVerticalCenter = False
    End Sub

    Protected Sub gridStat_Load(sender As Object, e As EventArgs) Handles gridStat.Load
        If gridStat.DataSource Is Nothing Then
            For i As Integer = 0 To 5
                Dim S As String = Now().AddMonths(-i).ToString("MMM yy")
                gridStat.Columns(8 + i).HeaderText = S.ToUpper().First() + S.Substring(1)   ' D2487
            Next
            gridStat.DataSource = Data
            gridStat.DataBind()
        End If
    End Sub

    Protected Sub gridStat_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gridStat.RowDataBound
        If e.Row.DataItem IsNot Nothing AndAlso TypeOf (e.Row.DataItem) Is clsWkgStat Then
            Dim tRow As clsWkgStat = CType(e.Row.DataItem, clsWkgStat)
            If Not tRow.Expiration.HasValue Then
                e.Row.Cells(3).Text = "never"
            Else
                e.Row.Cells(3).Text = tRow.Expiration.Value.Date.ToShortDateString ' D1173
                If tRow.Expiration < Now Then e.Row.Cells(3).ForeColor = Color.Red
            End If
            If Not tRow.LastVisited.HasValue Then
                e.Row.Cells(2).Text = "never"
            End If
            If tRow.Comment <> "" Then e.Row.Cells(0).Text += String.Format("<div class='small gray'>{0}</div>", tRow.Comment)
            If tRow.WorkgroupID = -1 Then
                e.Row.CssClass = "tbl_row_sel text"
                e.Row.Font.Bold = True
            End If
        End If
    End Sub

    Protected Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        Dim sResponse As String = String.Format("{0}Workgroup{1}Created{1}Last visited{1}Expires{1}Status{1}Manager{1}Projects{1}Projects created for month{1}%1{1}%2{1}%3{1}%4{1}%5{1}Total Activity{1}Comment{2}" + vbCrLf, _SYN_START, _SYN_DELIM, _SYN_END)   ' D2034 + D2487
        For i As Integer = 0 To 5
            Dim S As String = Now().AddMonths(-i).ToString("MMM yy")
            sResponse = sResponse.Replace("%" + i.ToString, S.ToUpper().First() + S.Substring(1))   ' D2034
        Next
        For Each tRow As clsWkgStat In Data
            ' D1173 ===
            Dim sLast As String = ""
            If tRow.LastVisited.HasValue Then sLast = tRow.LastVisited.Value.ToString
            Dim sExp As String = ""
            If tRow.Expiration.HasValue Then sExp = tRow.Expiration.Value.Date.ToShortDateString
            sResponse += String.Format("{0}{3}{1}{4}{1}{5}{1}{6}{1}""{7}""{1}{8}{1}{9}{1}{10}{1}{11}{1}{12}{1}{13}{1}{14}{1}{15}{1}{16}{1}{17}{1}""{18}""{2}" + vbCrLf, _
                                        _SYN_START, _SYN_DELIM, _SYN_END, _
                                        tRow.WorkgroupName.Replace(vbTab, " ").Trim, tRow.Created, sLast, sExp, _
                                        tRow.Status, tRow.WorkgroupManager, tRow.ProjectsCount, tRow.ProjectsCreated, _
                                        tRow.ActivityCurrent, tRow.Activity1, tRow.Activity2, _
                                        tRow.Activity3, tRow.Activity4, tRow.Activity5, tRow.ActivityTotal, tRow.Comment.Replace(vbTab, " "))   ' D2034 + D2487 + D2621
            ' D1173 ==
        Next

        DownloadContent(sResponse, "text/csv", String.Format("Stats_{0}_{1}.csv", RemoveXssFromUrl(Request.Url.Host).Replace(".", "-"), Now.ToString("yyyy-MM")), dbObjectType.einfWorkgroup) ' D6593 + D6767
        'RawResponseStart()
        'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""Stats_{0}_{1}.csv""", Request.Url.Host.Replace(".", "-"), Now.ToString("yyyy-MM")))  ' D2034
        'Response.ContentType = "text/csv"   ' D2034
        ''Response.AddHeader("Content-Length", sResponse.Length.ToString)
        'Response.Write(sResponse)
        'RawResponseEnd()
    End Sub

End Class

<Serializable()> _
Public Class clsWkgStat

    Private _WkgID As Integer = -1
    Private _Name As String = ""
    Private _Comment As String = ""
    Private _Status As String = ""
    Private _Expiration As Nullable(Of DateTime) = Nothing
    Private _Created As Nullable(Of Date) = Nothing
    Private _LastVisited As Nullable(Of Date) = Nothing
    Private _Manager As String = ""
    Private _Projects As Integer = 0
    Private _ProjectsCreated As Integer = 0
    Private _ActCur As Integer = 0
    Private _Act1 As Integer = 0
    Private _Act2 As Integer = 0
    Private _Act3 As Integer = 0
    Private _Act4 As Integer = 0
    Private _Act5 As Integer = 0
    Private _ActTotal As Integer = 0

    Public Property WorkgroupID() As Integer
        Get
            Return _WkgID
        End Get
        Set(value As Integer)
            _WkgID = value
        End Set
    End Property

    Public Property WorkgroupName() As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
        End Set
    End Property

    Public Property Comment() As String
        Get
            Return _Comment
        End Get
        Set(value As String)
            _Comment = value
        End Set
    End Property

    Public Property Status() As String
        Get
            Return _Status
        End Get
        Set(value As String)
            _Status = value
        End Set
    End Property

    Public Property WorkgroupManager() As String
        Get
            Return _Manager
        End Get
        Set(value As String)
            _Manager = value
        End Set
    End Property

    Public Property Expiration() As Nullable(Of DateTime)
        Get
            Return _Expiration
        End Get
        Set(value As Nullable(Of DateTime))
            _Expiration = value
        End Set
    End Property

    Public Property Created() As Nullable(Of DateTime)
        Get
            Return _Created
        End Get
        Set(value As Nullable(Of DateTime))
            _Created = value
        End Set
    End Property

    Public Property LastVisited() As Nullable(Of DateTime)
        Get
            Return _LastVisited
        End Get
        Set(value As Nullable(Of DateTime))
            _LastVisited = value
        End Set
    End Property

    Public Property ProjectsCount() As Integer
        Get
            Return _Projects
        End Get
        Set(value As Integer)
            _Projects = value
        End Set
    End Property

    Public Property ProjectsCreated() As Integer
        Get
            Return _ProjectsCreated
        End Get
        Set(value As Integer)
            _ProjectsCreated = value
        End Set
    End Property

    Public Property ActivityCurrent() As Integer
        Get
            Return _ActCur
        End Get
        Set(value As Integer)
            _ActCur = value
        End Set
    End Property

    Public Property Activity1() As Integer
        Get
            Return _Act1
        End Get
        Set(value As Integer)
            _Act1 = value
        End Set
    End Property

    Public Property Activity2() As Integer
        Get
            Return _Act2
        End Get
        Set(value As Integer)
            _Act2 = value
        End Set
    End Property

    Public Property Activity3() As Integer
        Get
            Return _Act3
        End Get
        Set(value As Integer)
            _Act3 = value
        End Set
    End Property

    Public Property Activity4() As Integer
        Get
            Return _Act4
        End Get
        Set(value As Integer)
            _Act4 = value
        End Set
    End Property

    Public Property Activity5() As Integer
        Get
            Return _Act5
        End Get
        Set(value As Integer)
            _Act5 = value
        End Set
    End Property

    Public Property ActivityTotal() As Integer
        Get
            Return _ActTotal
        End Get
        Set(value As Integer)
            _ActTotal = value
        End Set
    End Property

    Shared Function ItemByWkgID(ID As Integer, WkgList As List(Of clsWkgStat)) As clsWkgStat
        For Each tItem As clsWkgStat In WkgList
            If tItem.WorkgroupID = ID Then Return tItem
        Next
        Return Nothing
    End Function

End Class