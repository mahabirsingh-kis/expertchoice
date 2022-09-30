Imports System.Net

Partial Class WebFarm
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_UNKNOWN)
    End Sub

    Public Const Rows As Integer = 3
    Public Const Cols As Integer = 2

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Dim isDataRequest As Boolean = CheckVar("frm", "") <> ""

        tblFrames.Visible = Not isDataRequest
        lblData.Visible = isDataRequest

        If isDataRequest Then

            Dim ipEntry As IPHostEntry = Dns.GetHostEntry(Dns.GetHostName)
            Dim addr() As IPAddress = ipEntry.AddressList
            Dim sIPs As String = ""
            For i As Integer = 0 To addr.Length - 1
                sIPs += String.Format("{0} ", addr(i))
            Next

            lblData.Text = String.Format("<ul type=square>" + _
                                         "<li>Date: {0}</li>" + _
                                         "<li>Raw URL: {1}</li>" + _
                                         "<li>SessionID: {2}</li>" + _
                                         "<li>LCID: {3}</li>" + _
                                         "<li>Server: {4}</li>" + _
                                         "<li>DNS Host: {5}</li>" + _
                                         "</ul>", _
                                         Now.ToString, _
                                         Request.RawUrl, _
                                         Session.SessionID, _
                                         Session.LCID, _
                                         Server.MachineName, _
                                         sIPs)
            AlignHorizontalCenter = False
            AlignVerticalCenter = False

        Else

            For i As Integer = 1 To Rows
                Dim tr As New HtmlTableRow
                For j As Integer = 1 To Cols
                    Dim td As New HtmlTableCell
                    td.InnerHtml = String.Format("<iframe id='{0}' src='{1}' frameborder='0' height='100%' width='100%' scrolling='auto'></iframe>",
                                                 String.Format("frm{0}{1}", i, j),
                                                 String.Format("?frm={0}{1}&r={2}", i, j, GetRandomString(10, True, False)))
                    tr.Cells.Add(td)
                Next
                tblFrames.Rows.Add(tr)
            Next

        End If

    End Sub

End Class
