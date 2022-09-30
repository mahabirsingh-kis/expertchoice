<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlShowResults2" Codebehind="ctrlShowResults2.ascx.vb" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>
<script type="text/javascript"><!--

    var img_path        = '<% =ImgPath %>';
    var img_infodoc1	= new Image; img_infodoc1.src    = img_path + '<% =ImageInfodoc %>';
    var img_infodoc2	= new Image; img_infodoc2.src    = img_path + '<% =ImageInfodocEmpty %>';
    var img_WRTinfodoc1	= new Image; img_WRTinfodoc1.src = img_path + '<% =ImageWRTInfodoc %>';
    var img_WRTinfodoc2	= new Image; img_WRTinfodoc2.src = img_path + '<% =ImageWRTInfodocEmpty %>';
    
    var sort_field = <% =CInt(SortExpression) %>;
    var sort_dir = <% =CInt(SortDirection) %>;
    
    var pnl = null;
    var do_click = true;

    function DoCallback(cmd)
    {
        $("#<% = GridResults.ClientID %>").hide();
        if (!(pnl)) pnl = $find("<% =RadAjaxPanelResults.ClientID %>");
        if ((pnl)) pnl.ajaxRequest(cmd+"&r=" + Math.round(10000*Math.random()));
    }

    function onSelectNode(id) {
        var tree = $find("<% =RadTreeViewHierarchy.ClientID %>");
        if ((tree))
        {
            do_click = false;
            var nodes = tree.get_allNodes();
            tree.trackChanges();
            for (var i = 0; i < nodes.length; i++) 
            {
                var tmp_node = nodes[i];
                if (tmp_node.get_value()==id) tmp_node.select();
            }
            tree.commitChanges();
            do_click = true;
        }

    }

    function onNodeClicking(sender, eventArgs) 
    {
        if ((do_click))
        {
            var node = eventArgs.get_node();
            if ((node)) DoCallback("action=node&nid=" + node.get_value());
            setTimeout("onSelectNode('" + node.get_value() + "');", 30);
        }
    }

    function onSort(fld)
    {
        DoCallback("action=sort&fld=" + fld);
    }

    function res_open_infodoc(url, edit)
    {
        do_click = false;
        if ((edit)) { open_infodoc_edit(url); }  else { open_infodoc(url); }
        setTimeout("do_click = true;", 100);
    }
    
    var resize_tried = false;

    function onResultsResized()
    {
        var o = $("#PageContent_GridShowResults_RadAjaxPanelResults");
        if ((o) & (o.length)) { o.parent().parent().height(""); }
        var splitter = $find("<%= RadSplitterResults.ClientID %>");
        var td = $get("tdResultsContainer");
        if ((splitter) && (td) && (td.clientHeight))
        {
            splitter.set_height(101);
            var h = td.clientHeight-6;
            if (h < 100) h = 100;
            splitter.set_height(h);
        }
        else
        {
            if (!resize_tried)
            {
                setTimeout("resize_tried = true; onResultsResized();", 250);
            }
        }
        resize_tried = false;
        return true;
    }

    function OnClientResized(sender, eventArgs)
    {
        return false;
        //alert(eventArgs.get_oldWidth() + " - " + sender.get_width() + " - " + sender.get_id());
        var dif = eventArgs.get_oldWidth() - sender.get_width();
        var tree = $find("<% =RadTreeViewHierarchy.ClientID %>");
        if ((dif) && (tree))
        {
            var nodes = tree.get_allNodes();
            for (var i = 0; i < nodes.length; i++) 
            {
                var node = nodes[i];
                var tbl = $get("tbl" + node.get_value());
                var w =  sender.get_width() - <% =CInt(Iif(Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count>10, 124, 112)) %> - node.get_level() * 20;
                if ((tbl))
                {
//                    tbl.style.width = (tbl.clientWidth - dif) + "px";
                    tbl.style.width = w + "px";
                }
            }

        }
    }

    <% if CanEditInfodocs Then %> 
    function onRichEditorRefresh(empty, infodoc, callback)
    {
        if ((callback) && (callback[0]) && callback[0]!="")
        {
            var n = callback[0];
            var wrt = callback[0].substr(0,3)=="wrt";
            var f = $get(n);
            if ((f))
            {
                if (wrt)
                {
                    f.src = (empty==1 ? img_WRTinfodoc2 .src : img_WRTinfodoc1.src);
                }
                else
                {   
                    f.src = (empty==1 ? img_infodoc2.src : img_infodoc1.src);
                }   
            }
        }
        window.focus();
    }
    <% End If %>  
    
    window.onresize = onResultsResized;
    
//-->    
</script>
<table border="0" cellpadding="0" cellspacing="0" style="height:99%" align="center">
<tr valign="top">
<td style="width:1px"><asp:Image runat="server" ID="imgVertSpacer" SkinID="imgSpacer" Height="230" Width="1" AlternateText=""/></td>
 <td align='center' id="tdResultsContainer"> 
<telerik:RadSplitter ID="RadSplitterResults" runat="server" Height="99%" Width="99%" BorderSize="0" BorderWidth="0px" BorderColor="White" BorderStyle="None" LiveResize="True" ResizeWithBrowserWindow="true" ResizeWithParentPane="true">

  <telerik:RadPane ID="radPanelTree" runat="server" MinWidth="180" Width="315" MaxWidth="600" Scrolling="None" CssClass="panel" OnClientResized="OnClientResized">
    <telerik:RadTreeView runat="server" ID="RadTreeViewHierarchy" AllowNodeEditing="false" AutoPostBack="false" AccessKey="T" Width="99%" Height="99%" EnableDragAndDrop="false" EnableEmbeddedScripts="true" MultipleSelect="true" OnClientNodeClicking="onNodeClicking">
        <NodeTemplate><table border='0' cellspacing='0' cellpadding='0' style='width:100%;'><tr><td class='text'><div id='tbl<%#DataBinder.Eval(Container, "Value") %>' style='width:<%# CInt(Iif(Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count>12, 190, 210)) - Cint(DataBinder.Eval(Container, "Level"))*18%>px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;'><nobr><%# CreateNodeInfodoc(CStr(DataBinder.Eval(Container, "Value")), reObjectType.Node)%><%#DataBinder.Eval(Container, "Text")%></nobr></div></td><td align='right' valign='middle' class='text small' style='width:56px; padding-left:4px; font-weight:normal; color:#333333;'><%# GetNodePrty(Container)%></nobr></td></tr></table></NodeTemplate>
    </telerik:RadTreeView>
  </telerik:RadPane>
  
  <telerik:RadSplitBar ID="RadSplitbarLeft" runat="server" CollapseMode="Forward" EnableResize="true"/>

  <telerik:RadPane ID="radPanelGrid" runat="server" MinHeight="150" MinWidth="430" Width="600" Scrolling="Y" MaxWidth="1000" MaxHeight="1200"><telerik:RadAjaxPanel ID="RadAjaxPanelResults" runat="server" HorizontalAlign="Center" CssClass="whole" Width="99%" Height="98%" LoadingPanelID="AjaxLoadingPanel">
  <div class="whole" style="text-align:center; padding:3px 3px 0px 5px;"><table border="0" cellpadding="0" cellspacing="0" class="whole">
  <tr runat="server" id="tdNormalizationMode"><td align="left" style="height:1em; padding:2px 3px 5px 1px;" class="text"><% =NormalizationModeCaption%>&nbsp;<asp:DropDownList runat="server" ID="ddResultsMode" AutoPostBack="true"/></td></tr>
  <tr valign="top">
   <td> 
    <asp:GridView EnableViewState="false" AutoGenerateColumns="False" AllowSorting="false" AllowPaging="false" ID="GridResults" runat="server" BorderWidth="0" BorderColor="#e0e0e0" CellSpacing="1" CellPadding="3" TabIndex="1" HorizontalAlign="Center" DataKeyNames="Name" Width="100%">
        <Columns>
          <asp:BoundField DataField="Position" SortExpression="position" HtmlEncode="False">
              <ItemStyle HorizontalAlign="Right" Width="2em" />
          </asp:BoundField>
          <asp:BoundField DataField="Name" SortExpression="name" HtmlEncode="False" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left">
              <ItemStyle/>
          </asp:BoundField>
          <asp:BoundField DataField="KnownLikelihood" HtmlEncode="False">
              <ItemStyle HorizontalAlign="Right" Width="5em" />
          </asp:BoundField>
          <asp:BoundField DataField="ValueMy" SortExpression="my" HtmlEncode="False">
              <headerstyle CssClass="label_my"></headerstyle>
              <ItemStyle HorizontalAlign="Right" Width="5em" />
          </asp:BoundField>
          <asp:BoundField DataField="ValueCombined" SortExpression="combined" HtmlEncode="False">
              <ItemStyle HorizontalAlign="Right" Width="5em" />
          </asp:BoundField>
          <asp:BoundField HtmlEncode="False">
              <ItemStyle Width="150px" />
          </asp:BoundField>
        </Columns>
        <RowStyle VerticalAlign="Middle" CssClass="text grid_row"/>
        <HeaderStyle CssClass="text grid_header actions"/>
        <AlternatingRowStyle CssClass="text grid_row_alt" />
    </asp:GridView>
    <%--<div runat="server" id="divRefresh" style="text-align:right; margin:1ex 0px"><asp:ImageButton ID="imgBtnRefresh" runat="server" SkinID="RefreshImageButton" ToolTip="Recalculate"/></div>--%>
    
    <div style="margin-top:1em; text-align:center" class="text"><asp:Label runat="server" ID="lblInconsistency" Visible="false"/></div>
    <div style="margin-top:1em; text-align:center; color: #006699" class="text"><asp:Label runat="server" ID="lblMessage" Visible="false" Font-Bold="true"/></div>
  
  </td></tr>
  </table><%--<asp:HiddenField runat="server" ID="LastNodeID" />--%></div></telerik:RadAjaxPanel>
  </telerik:RadPane>
</telerik:RadSplitter></td></tr></table>
<telerik:RadAjaxLoadingPanel ID="AjaxLoadingPanel" runat="server"><ec:loadingpanel id="LoadingPanel" runat="server" /></telerik:RadAjaxLoadingPanel>
