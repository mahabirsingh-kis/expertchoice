<%@ Page Language="VB" Inherits="WhoIsOnlinePage" Codebehind="BigBrother.aspx.vb" %>
<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">
<!--
    
    var timeout = 15000;

    function GetList() {
        document.location.reload();
    }

    function ReloadList()
    {
        setTimeout("GetList()", timeout);
    }
    
    ReloadList();
    
//-->
</script>
<div style="width:99%; text-align:center;"><asp:ImageButton ID="imgBtnRefresh" runat="server" SkinID="RefreshImageButton" OnClientClick="GetList(); return false;"/><h4 style="margin:1ex 0px 1ex 0px"><% =ResString("lblWhoIsOnline")%></h4>
<asp:GridView AutoGenerateColumns="False" ID="GridViewUsers" runat="server" BorderWidth="0" BorderColor="#e0e0e0" CellSpacing="1" CellPadding="0" OnRowDataBound="GridViewUsers_RowDataBound" OnPreRender="GridViewUsers_PreRender" Width="100%">
<Columns>
  <asp:BoundField DataField="LastAccess" HtmlEncode="false" ItemStyle-Width="15%" ItemStyle-Wrap="false"/>
  <asp:BoundField DataField="UserEmail" HtmlEncode="false" ItemStyle-Width="10%" ItemStyle-Wrap="false"/>
  <asp:BoundField DataField="WorkgroupID" HtmlEncode="false" ItemStyle-Width="15%" ItemStyle-Wrap="false"/>
  <asp:BoundField DataField="ProjectID" HtmlEncode="false" ItemStyle-Width="25%" ItemStyle-Wrap="false"/>
  <asp:BoundField DataField="URL" HtmlEncode="false" ItemStyle-Width="25%" ItemStyle-Wrap="false"/>
  <asp:BoundField DataField="SessionID" HtmlEncode="false" ItemStyle-Width="10%" ItemStyle-Wrap="false"/>
</Columns>
<RowStyle CssClass="text small grid_row actions"/>
<AlternatingRowStyle CssClass="text small grid_row_alt actions" />
<HeaderStyle CssClass="text grid_header"/>
<PagerStyle CssClass="actions" HorizontalAlign="Center" />
<EmptyDataTemplate><div style="margin-top:5em"><h5><% =ResString("lblListEmpty") %></h5></div></EmptyDataTemplate>
</asp:GridView>
</div>
</asp:Content>
