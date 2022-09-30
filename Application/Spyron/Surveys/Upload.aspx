<%@ Page Language="VB" Inherits="SurveyUpload" title="Survey Upload" Codebehind="Upload.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">
    <!--

    function CheckUpload()
    {
        var f = theForm.<% =FileUploadSurvey.ClientID %>;
        var b = theForm.<% =btnUpload.ClientID %>;
        if (f && b) b.disabled = (f.value == "");
        return false;
    }
  
    // -->
</script>

<table class="whole" border="0">
<tr><td valign="middle" align="center">

<h6><% =PageTitle(CurrentPageID)%></h6>
<div id="divUploadSurvey" style="margin:1em 2em 2em 2em; text-align:center; border:1px solid #e0e0e0; background:#f0f0f0; padding:1em;" runat="server" class="text"><!--span class="text" style="padding:1em; border:1px solid #e0e0e0; background:#f0f0f0;"--><%=ResString("lblPropertiesSelectFile")%>:&nbsp;<asp:FileUpload ID="FileUploadSurvey" runat="server" Width="25em" CssClass="input" />&nbsp;<asp:Button runat="server" ID="btnUpload" CssClass="button" Height="22"/><!--/span--></div>
<asp:Label runat="server" ID="lblError" Visible="false"/>

</td></tr>
</table>


</asp:Content>

