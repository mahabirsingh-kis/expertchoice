<%@ Page Language="VB" Inherits="DataGridUpload" title="DataGrid Upload" MasterPageFile="~/mpEmpty.Master" Codebehind="UploadDataGrid.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">
    <!--
    
    function HideError() {
        var l = $get("<%= lblError.ClientID %>");
        l.style.display = 'none';
    }

    function doUpload() {
        showLoadingPanel("<% = JS_SafeString(ResString("lblFileUploading")) %>");
        theForm.<% =btnUpload.ClientID %>.click();
        if ((window.parent) && (typeof window.parent.CheckUpload) != "undefined") {
            window.parent.CheckUpload(false);
        }
    }

    function CheckUpload()
    {
        var f = theForm.<% =FileUpload.ClientID %>;
        var dis = true;
        if ((f)) dis = (f.value == "");
        var b = theForm.<% =btnUpload.ClientID %>;
        if ((b)) b.disabled = dis;
        if ((window.parent) && (typeof window.parent.CheckUpload) != "undefined") {
            window.parent.CheckUpload(dis);
        }
        hideLoadingPanel();
        return false;
    }

    function onUploadFile()
    {
        if ((window.parent) && (typeof window.parent.onUpload) != "undefined") {
            window.parent.onUpload();
            closePopup();
            hideLoadingPanel();
        } else {
            if ((window.opener)) {
                if ((typeof window.opener.onUpload)!="undefined") {
                    window.opener.onUpload();
                } else {
                    setTimeout('window.opener.location.href="<% = PageURL(_PGID_REPORT_DATAGRID, "?id=" + CheckVar("id", -1).ToString + "&reset=1&") %>";', 200);
                }
            }
        }
        setTimeout("window.close();", 250);    
    }

    setTimeout('window.focus(); var v = theForm.<% =FileUpload.ClientID %>; if ((v)) v.focus();', 100);
    $(document).ready(function () {
        <% If Not CheckVar("btnUpload", True) Then %><% =String.Format("$('#{0}').hide();", btnUpload.ClientID) %><% End If %>
    });
  
    // -->
</script>

<table class="whole" border="0">
<tr><td valign="middle" align="center">
    <h6><% =PageTitle(CurrentPageID)%></h6>
    <div id="divUpload" style="padding:1em 0px 2em 0px; text-align:center" runat="server"><span class="text" style="padding:1em; border:1px solid #e0e0e0; background:#f0f0f0;"><%=ResString("lblDataGridSelectFile")%>:&nbsp;<asp:FileUpload ID="FileUpload" runat="server" Width="25em" CssClass="input" onclick="HideError();" />&nbsp;<asp:Button runat="server" ID="btnUpload" CssClass="button" OnClientClick="showLoadingPanel();"/></span></div>
    <asp:Label runat="server" ID="lblError" Visible="true" />
</td></tr>
</table>
    
</asp:Content>