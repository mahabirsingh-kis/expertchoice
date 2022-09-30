<%@ Page Language="VB" Inherits="ProjectLogsPage" title="Project Logs" Codebehind="ProjectLogs.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables.extra.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables_only.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.highlight.js"></script>
<script type="text/javascript">

    var dataSet = [<% =GetData() %>];
    var realCnt = <% =RealCount%>;

    var table = null;
    var tbl_id = "tblLogs";

    function InitTable() {
        table = $('#' + tbl_id).DataTable({
            data: dataSet,
            columns: [
                { title: "Date", "data": 1, "width": 140 },
                { title: "Action&nbsp;&nbsp;", "data": 3, "width": 100 },
                { title: "Object", "data": 5, "width": 70 },
                { title: "User e-mail", "data": 7 },
                { title: "User name", "data": 8 },
                { title: "Event", "data": 9 },
                { title: "Details", "data": 10 }
            ],
            "paging": false,
            "ordering": true,
            "scrollY": "350px",
            "stateSave": false, 
            "scrollCollapse": true,
            "info": true,
            "order": [[0, 'desc']],
            "createdRow": function (row, data, index) {
                //$('td', row).eq(4).css("font-weight", "bold");
                if (data[4]==<% =CInt(dbObjectType.einfRTE) %> || data[2]==<% =CInt(dbActionType.actServiceRTE)%> || data[2]==<% =CInt(dbActionType.actShellError)%>) {
                    $('td', row).addClass('error');
                }
            }

        });

        dataTablesHighlighSearch(table);
        setTimeout("onResize();", 10);
        setTimeout("onResize();", 1000);
    };

    function onResize() {
        dataTablesResize(tbl_id, "tdMain", 60);
    }

    <% If CanSeeLogs() Then%>resize_custom = onResize;
    $(document).ready(function () { InitTable(); });<% End If %>

</script>
<table border=0 style="height:99%; width:100%">
<tr style='height:2em'><td style="border-bottom:3px double #e0e0e0"><h4><% = GetTitle()%></h4></td></tr>
<tr valign="top"><td id="tdMain" class="text"><% If CanSeeLogs() Then%><table id="tblLogs" class="text display" width="100%"></table><% Else%><h6 class='error'>No model specified or you don't have the required permissions.</h6><% End If%></td></tr>
<tr><td style='height:1em; padding-top:1ex;' class="text small gray"><% If RealCount > 0 Then%><span style="float:right; margin-left:2em;" id="spanNote" class="text small gray"><nobr>Events count total: <% =RealCount %></nobr></span><% end if%><b>Note:</b> This report might not contain some of the events related to current mo. For additional information please check Global Logs or contact our Customer Support team.</td></tr>
</table>
</asp:Content>

