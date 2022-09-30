<%@ Page Language="VB" Inherits="ProjectIteratePage" title="Iterate" Codebehind="Iterate.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">
    var CurrentPageID = <%=CurrentPageID%>;
    var _PGID_ITERATE_STRUCTURING    = <%=_PGID_ITERATE_STRUCTURING%>;
    var _PGID_ITERATE_MEASUREMENT    = <%=_PGID_ITERATE_MEASUREMENT%>;
    var _PGID_STRUCTURE_WORKFLOW     = 20101;
    var _PGID_COLLECT_INPUT_WORKFLOW = 20120;
    
    function actionButtonClick() {
        switch (CurrentPageID) {
            case _PGID_ITERATE_STRUCTURING:
                openPageById(_PGID_STRUCTURE_WORKFLOW);
                break;
            case _PGID_ITERATE_MEASUREMENT:
                openPageById(_PGID_COLLECT_INPUT_WORKFLOW);
                break;
            default:
                break;
        }
    }
</script>

<div id="divMain" class="whole">
    <%--<h5 id="lblHeader" runat="server" style='padding:1ex'><%=ResString("titleIterateStructuring")%></h5>--%>
    <div style="text-align:left; margin:10px;">
        <a href="#" id="hbNavAction" class="actions" style="font-size:large;" runat="server" onclick="actionButtonClick(); return false;"></a>  
    </div>
</div>

</asp:Content>