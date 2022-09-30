<%@ Page Language="VB" Inherits="WorkgroupsStatisticPage" title="Workgroups Statistic" Codebehind="WkgStat.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<h3><% =PageTitle(CurrentPageID)%></h3>
<div style="text-align:center"><asp:GridView runat="server" ID="gridStat" CssClass="tbl tbl_margins" HeaderStyle-CssClass="tbl_hdr text" RowStyle-CssClass="tbl_row text" AutoGenerateColumns="false" AlternatingRowStyle-CssClass="tbl_row_alt text" BorderColor="LightGray" BorderWidth="1">
<Columns>
<asp:BoundField HeaderText="Workgroup" DataField="WorkgroupName" />
<asp:BoundField HeaderText="Created" DataField="Created" ItemStyle-Wrap="false" ItemStyle-HorizontalAlign="Center" />
<asp:BoundField HeaderText="Last visited" DataField="LastVisited" ItemStyle-Wrap="false" ItemStyle-HorizontalAlign="Center" />
<asp:BoundField HeaderText="Expires" DataField="Expiration"  ItemStyle-Wrap="false" ItemStyle-HorizontalAlign="Center" />
<asp:BoundField HeaderText="Status" DataField="Status" ItemStyle-CssClass="small" />
<asp:BoundField HeaderText="Manager" DataField="WorkgroupManager" />
<asp:BoundField HeaderText="Projects" HeaderStyle-Width="3em" DataField="ProjectsCount" ItemStyle-HorizontalAlign="Right" />
<asp:BoundField HeaderText="Projects created for month" HeaderStyle-Width="3em" DataField="ProjectsCreated" ItemStyle-HorizontalAlign="Right" HeaderStyle-Wrap="true" />
<asp:BoundField HeaderText="#0" HeaderStyle-Width="4em" HeaderStyle-Wrap="false" DataField="ActivityCurrent" ItemStyle-HorizontalAlign="Right"/>
<asp:BoundField HeaderText="#1" HeaderStyle-Width="4em" HeaderStyle-Wrap="false" DataField="Activity1" ItemStyle-HorizontalAlign="Right"/>
<asp:BoundField HeaderText="#2" HeaderStyle-Width="4em" HeaderStyle-Wrap="false" DataField="Activity2" ItemStyle-HorizontalAlign="Right"/>
<asp:BoundField HeaderText="#3" HeaderStyle-Width="4em" HeaderStyle-Wrap="false" DataField="Activity3" ItemStyle-HorizontalAlign="Right"/>
<asp:BoundField HeaderText="#4" HeaderStyle-Width="4em" HeaderStyle-Wrap="false" DataField="Activity4" ItemStyle-HorizontalAlign="Right"/>
<asp:BoundField HeaderText="#5" HeaderStyle-Width="4em" HeaderStyle-Wrap="false" DataField="Activity5" ItemStyle-HorizontalAlign="Right"/>
<asp:BoundField HeaderText="Act/Total" HeaderStyle-Width="5em" HeaderStyle-Wrap="false" DataField="ActivityTotal" ItemStyle-HorizontalAlign="Right"/>
</Columns>
<EmptyDataTemplate><p align='center' class='text' style='background:#fffff; padding:4em'>Sorry, but list is empty. Please try agian.</p></EmptyDataTemplate>
</asp:GridView>
<p align='right'><asp:Button runat="server" ID="btnExport" Text="Export as CSV" Width="12em" CssClass="button" /></p>
</div>

</asp:Content>

