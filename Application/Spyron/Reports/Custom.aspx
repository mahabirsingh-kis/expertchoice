<%@ Page Language="VB" Inherits="CustomResultsPage" title="Advanced/Custom" Codebehind="Custom.aspx.vb" %>

<%@ Register Assembly="DevExpress.Web.ASPxGridView.v9.1.Export, Version=9.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a"
    Namespace="DevExpress.Web.ASPxGridView.Export" TagPrefix="dxwgv" %>

<%@ Register Assembly="DevExpress.XtraCharts.v9.1.Web"  Namespace="DevExpress.XtraCharts.Web" TagPrefix="dxchartsui" %>
<%@ Register Assembly="System.Web" Namespace="System.Web.UI.HtmlControls" TagPrefix="cc3" %>
<%@ Register Assembly="DevExpress.Web.ASPxGridView.v9.1" Namespace="DevExpress.Web.ASPxGridView" TagPrefix="dxwgv" %>
<%@ Register Assembly="DevExpress.Web.ASPxEditors.v9.1" Namespace="DevExpress.Web.ASPxEditors" TagPrefix="dxe" %>
<%@ Register Assembly="DevExpress.Web.v9.1" Namespace="DevExpress.Web.ASPxTabControl" TagPrefix="dxtc" %>
<%@ Register Assembly="DevExpress.Web.v9.1" Namespace="DevExpress.Web.ASPxClasses" tagprefix="dxw" %>
<%@ Register Assembly="System.Web" Namespace="System.Web.UI" TagPrefix="cc1" %>
<%@ Register Assembly="DevExpress.Web.v9.1"  Namespace="DevExpress.Web.ASPxTabControl" TagPrefix="dxtc" %>
<%@ Register Assembly="DevExpress.Web.v9.1"  Namespace="DevExpress.Web.ASPxClasses" tagprefix="dxw" %>
<%@ Register Assembly="DevExpress.Web.ASPxPivotGrid.v9.1.Export" Namespace="DevExpress.XtraPivotGrid.Web" TagPrefix="dxpgw" %>
<%@ Register Assembly="DevExpress.Web.ASPxPivotGrid.v9.1" Namespace="DevExpress.Web.ASPxPivotGrid" TagPrefix="dxwpg" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%--<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<%--<EC:LoadingPanel id="pnlLoading" runat="server" isWarning="true" WarningShowOnLoad="false" WarningShowCloseButton="false" Width="230" Visible="true"/>--%>
    <asp:Panel ID="pnlReportOptions" runat="server"><table style="margin:10px"><tr><td class="text">Select participant or participants group: </td><td><asp:DropDownList ID="ddlGroups" runat="server" AutoPostBack="true"></asp:DropDownList></td></tr></table></asp:Panel>
    <dxtc:ASPxPageControl ID="ASPxPageControlReports" runat="server" TabPosition="Left" ActiveTabIndex="0" Width="100%" Height="90%">
        <TabPages>
<dxtc:TabPage Text="Priorities Overview"><ContentCollection><dxw:ContentControl runat="server">

        <dxpgw:ASPxPivotGridExporter runat="server" ASPxPivotGridID="ASPxPivotGrid1" ID="ASPxPivotGridExporter1">
        </dxpgw:ASPxPivotGridExporter>
       <asp:Panel runat="server" Height="30" ID="Panel0" Width="90%" HorizontalAlign="Left">
       <telerik:RadToolbar ID="tbExport0" runat="server" AutoPostback="true">
            <Items>
                <telerik:RadToolBarButton ID="btnGeneratePrioritesOverview" runat="server"
                    ButtonText="Generate report" CommandName="Generate" DisplayType="TextImage" Text=""/>
                <%--<telerik:RadToolBarButton Text="|" Enabled="false" />--%>
                <telerik:RadToolbarButton ID="btnRTF0" runat="server" 
                    ButtonText="Export to RTF" CommandName="ExporttoRTF" DisplayType="TextImage" Text="RTF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnPDF0" runat="server" 
                    ButtonText="Export to PDF" CommandName="ExporttoPDF" DisplayType="TextImage" Text="PDF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnXLS0" runat="server" 
                    ButtonText="Export to XLS" CommandName="ExporttoXLS" DisplayType="TextImage" Text="XLS" Enabled="false" />
            </Items>
        </telerik:RadToolbar>
            <asp:CheckBox runat="server" ID="chkAllUsers" Text="Generate for Individuals as well" />
        </asp:Panel>
        <div style="text-align:center;margin:5px 5px 5px 5px">
            <asp:Label runat="server" CssClass="error" ID="lblError" Text="" Visible="false"></asp:Label>
        </div>
        <dxwpg:ASPxPivotGrid runat="server" ID="ASPxPivotGrid1" Visible="false">
            <OptionsPager RowsPerPage="30" AllButton-Visible="true"></OptionsPager>

            <OptionsView ShowFilterSeparatorBar="True" ShowFilterHeaders="False" ShowColumnTotals="False" ShowRowTotals="False" ShowColumnGrandTotals="False" ShowRowGrandTotals="False"></OptionsView>
            <Fields>
                <dxwpg:PivotGridField FieldName="AltName" Area="RowArea" ID="_AltName" AreaIndex="0"></dxwpg:PivotGridField>      
                <dxwpg:PivotGridField FieldName="UserName" Area="ColumnArea" ID="_UserName" AreaIndex="0"></dxwpg:PivotGridField>
                <dxwpg:PivotGridField FieldName="AltGlobalPriority" Area="DataArea" ID="_AltGlobalPriority" AreaIndex="0" CellFormat-FormatType="Numeric" CellFormat-FormatString="p2"></dxwpg:PivotGridField>
            </Fields>
        </dxwpg:ASPxPivotGrid>
</dxw:ContentControl></ContentCollection>
</dxtc:TabPage>
            <dxtc:TabPage Text="judgments Overview"><ContentCollection><dxw:ContentControl runat="server">
<dxwgv:ASPxGridViewExporter ID="ASPxGridViewExporter1" runat="server" GridViewID="ASPxGridView1">
</dxwgv:ASPxGridViewExporter>
       <asp:Panel runat="server" Height="30" ID="Panel1" Width="90%" HorizontalAlign="Left">
       <telerik:RadToolbar ID="tbExport" runat="server" AutoPostback="true">
            <Items>
                <telerik:RadToolBarButton ID="btnGeneratePWJudgements" runat="server"
                    ButtonText="Generate report" CommandName="Generate" DisplayType="TextImage" Text="Generate report"/>
                <%--<telerik:RadToolBarButton Text="|" Enabled="false" />--%>
                <telerik:RadToolbarButton ID="btnRTF" runat="server" 
                    ButtonText="Export to RTF" CommandName="ExporttoRTF" DisplayType="TextImage" Text="RTF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnPDF" runat="server" 
                    ButtonText="Export to PDF" CommandName="ExporttoPDF" DisplayType="TextImage" Text="PDF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnXLS" runat="server" 
                    ButtonText="Export to XLS" CommandName="ExporttoXLS" DisplayType="TextImage" Text="XLS" Enabled="false" />
            </Items>
        </telerik:RadToolbar>
        </asp:Panel>
<dxwgv:ASPxGridView runat="server" ID="ASPxGridView1" Visible="false" SettingsPager-AllButton-Visible="true">
<Settings ShowGroupPanel="True" ShowGroupedColumns="True" ShowFilterBar="Visible" ShowFilterRow="true">
</Settings>
</dxwgv:ASPxGridView>

</dxw:ContentControl></ContentCollection>
</dxtc:TabPage>
<dxtc:TabPage Visible="True" Text="Objectives priorities">
<ContentCollection><dxw:ContentControl ID="ContentControl1" runat="server">
        <dxwgv:ASPxGridViewExporter ID="ASPxGridViewExporter3" runat="server" GridViewID="ASPxGridView3">
        </dxwgv:ASPxGridViewExporter>
       <asp:Panel runat="server" Height="30" ID="Panel3" Width="90%" HorizontalAlign="Left">
       <telerik:RadToolbar ID="tbExport3" runat="server" AutoPostback="true">
            <Items>
                <telerik:RadToolBarButton ID="btnObjPriorities" runat="server"
                    ButtonText="Generate report" CommandName="Generate" DisplayType="TextImage" Text="Generate report"/>
                <%--<telerik:RadToolBarButton Text="|" Enabled="false" />--%>
                <telerik:RadToolbarButton ID="btnRTF3" runat="server" 
                    ButtonText="Export to RTF" CommandName="ExporttoRTF" DisplayType="TextImage" Text="RTF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnPDF3" runat="server" 
                    ButtonText="Export to PDF" CommandName="ExporttoPDF" DisplayType="TextImage" Text="PDF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnXLS3" runat="server" 
                    ButtonText="Export to XLS" CommandName="ExporttoXLS" DisplayType="TextImage" Text="XLS" Enabled="false" />
            </Items>
        </telerik:RadToolbar>
        </asp:Panel>
        <dxwgv:ASPxGridView runat="server" ID="ASPxGridView3" Visible="false" AutoGenerateColumns="true" SettingsPager-AllButton-Visible="true">
            <Settings ShowFilterRow="True" ShowFilterBar="Visible" ShowGroupPanel="True" ShowGroupedColumns="True"></Settings>
        </dxwgv:ASPxGridView>
</dxw:ContentControl></ContentCollection>
</dxtc:TabPage>

<dxtc:TabPage Visible="True" Text="Evaluation Progress">
<ContentCollection><dxw:ContentControl ID="ContentControl2" runat="server">
        <dxwgv:ASPxGridViewExporter ID="ASPxGridViewExporter4" runat="server" GridViewID="ASPxGridView4">
        </dxwgv:ASPxGridViewExporter>
       <asp:Panel runat="server" Height="30" ID="Panel4" Width="90%" HorizontalAlign="Left">
       <telerik:RadToolbar ID="tbExport4" runat="server" AutoPostback="true">
            <Items>
                <telerik:RadToolBarButton ID="btnEvalProgress" runat="server"
                    ButtonText="Generate report" CommandName="Generate" DisplayType="TextImage" Text="Generate report"/>
                <%--<telerik:RadToolBarButton Text="|" Enabled="false" />--%>
                <telerik:RadToolbarButton ID="btnRTF4" runat="server" 
                    ButtonText="Export to RTF" CommandName="ExporttoRTF" DisplayType="TextImage" Text="RTF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnPDF4" runat="server" 
                    ButtonText="Export to PDF" CommandName="ExporttoPDF" DisplayType="TextImage" Text="PDF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnXLS4" runat="server" 
                    ButtonText="Export to XLS" CommandName="ExporttoXLS" DisplayType="TextImage" Text="XLS" Enabled="false" />
            </Items>
        </telerik:RadToolbar>
        </asp:Panel>
        <dxwgv:ASPxGridView runat="server" ID="ASPxGridView4" Visible="false" AutoGenerateColumns="true" SettingsPager-AllButton-Visible="true">
            <Settings ShowFilterRow="True" ShowFilterBar="Visible" ShowGroupPanel="True" ShowGroupedColumns="True"></Settings>
        </dxwgv:ASPxGridView>
</dxw:ContentControl></ContentCollection>
</dxtc:TabPage>

<dxtc:TabPage Visible="True" Text="Inconsistencies">
<ContentCollection><dxw:ContentControl ID="ContentControl5" runat="server">
<script type='text/javascript' language='javascript'>

    var show_indiv = 0;

    function DoOpenInconsPipe(email, node_guid, show_indiv) {
        <% If ShowResponsive Then %>
        _ajax_ok_code = "OpenLink(data, <% =App.ProjectID %>, <% =App.ActiveProject.ProjectManager.ActiveHierarchy %>, '<% = JS_SafeString(App.ActiveProject.Passcode(App.ActiveProject.isImpact))%>');";
        _ajaxSend("action=link&email=" + encodeURIComponent(email) + "&guid=" + encodeURIComponent(node_guid) + "&show_indiv=" + show_indiv);<% Else %>
        var url = "<% =PageURL(_PGID_EVALUATE_READONLY) %>&mode=searchresults&id=" + encodeURIComponent(email) + "&node=" + encodeURIComponent(node_guid) + (show_indiv ? "&show_indiv=" + show_indiv : "") + "&temptheme=sl";
        CreatePopup(url, "incons");<% End If %>
    }

    function ShowInconsPipe(email, node_guid) {
        if (email.toLowerCase() == "admin") {
            DevExpress.ui.dialog.alert("Sorry, but you can't see evaluation for this user.", "Notification");
        } else {
            <% If App.ActiveProject.PipeParameters.LocalResultsView = ResultsView.rvNone OrElse App.ActiveProject.PipeParameters.LocalResultsView = ResultsView.rvGroup Then %>
            if (!show_indiv) {
                var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgShowIntermResults")) %>", resString("titleConfirmation"));
                result.done(function (dialogResult) {
                    if (dialogResult) {
                        show_indiv=1; 
                        DoOpenInconsPipe(email, node_guid, 1);
                    }
                });
            } else { 
                DoOpenInconsPipe(email, node_guid, 0);
            }
            <% Else %>DoOpenInconsPipe(email, node_guid, 0);<% End if %>
        }
    }

    function OpenUserPipe(email, node_guid) {
        if (<% =If(App.ActiveUser.CannotBeDeleted, "false", "email.toLowerCase() == 'admin'") %>) {
            DevExpress.ui.dialog.alert("Sorry, but you can't see the evaluation for this user.", "Notification");
        } else {
            <% If App.ActiveProject.PipeParameters.LocalResultsView = ResultsView.rvNone OrElse App.ActiveProject.PipeParameters.LocalResultsView = ResultsView.rvGroup Then %>
            if (!show_indiv) {
                var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgShowIntermResults")) %>", resString("titleConfirmation"));
                result.done(function (dialogResult) {
                    if (dialogResult) {
                        show_indiv=1; 
                        DoOpenUserPipe(email, node_guid, 1);
                    }
                });
            } else { 
                DoOpenUserPipe(email, node_guid, 0);
            }
            <% Else %>DoOpenUserPipe(email, node_guid, 0);<% End if %>
        }
    }

    function DoOpenUserPipe(email, node_guid, show_indiv) {
        _ajax_ok_code = "OpenLink(data, <% =App.ProjectID %>, <% =App.ActiveProject.ProjectManager.ActiveHierarchy %>, '<% = JS_SafeString(App.ActiveProject.Passcode(App.ActiveProject.isImpact))%>');";
        _ajaxSend("action=open&email=" + encodeURIComponent(email) + "&guid=" + encodeURIComponent(node_guid) + "&show_indiv=" + show_indiv);
    }

    function OpenLink(params, prj_id, hid, passcode) {
        if (params == "") {
            DevExpress.ui.dialog.alert("Sorry, but you can't see the evaluation for this user.", "Notification");
        } else {
            <% If ShowResponsive Then %>CreatePopup(params, "incons", ",");<% Else %>OpenLinkWithReturnUser(params, prj_id, hid, passcode, false);<% End If %>
        }
        return false;
    }

</script>
        <dxwgv:ASPxGridViewExporter ID="ASPxGridViewExporter5" runat="server" GridViewID="ASPxGridView5">
        </dxwgv:ASPxGridViewExporter>
       <asp:Panel runat="server" Height="30" ID="Panel5" Width="90%" HorizontalAlign="Left">
       <telerik:RadToolbar ID="tbExport5" runat="server" AutoPostback="true">
            <Items>
                <telerik:RadToolBarButton ID="btnInconsistency" runat="server"
                    ButtonText="Generate report" CommandName="Generate" DisplayType="TextImage" Text="Generate report"/>
                <%--<telerik:RadToolBarButton Text="|" Enabled="false" />--%>
                <telerik:RadToolbarButton ID="btnRTF5" runat="server" 
                    ButtonText="Export to RTF" CommandName="ExporttoRTF" DisplayType="TextImage" Text="RTF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnPDF5" runat="server" 
                    ButtonText="Export to PDF" CommandName="ExporttoPDF" DisplayType="TextImage" Text="PDF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnXLS5" runat="server" 
                    ButtonText="Export to XLS" CommandName="ExporttoXLS" DisplayType="TextImage" Text="XLS" Enabled="false" />
            </Items>
        </telerik:RadToolbar>
        </asp:Panel>
        <dxwgv:ASPxGridView runat="server" ID="ASPxGridView5" Visible="false" AutoGenerateColumns="false" SettingsPager-AllButton-Visible="true">
            <Settings ShowFilterRow="True" ShowFilterBar="Visible" ShowGroupPanel="True" ShowGroupedColumns="True"></Settings>
            <Columns>
                <dxwgv:GridViewDataTextColumn FieldName="UserName" />
                <dxwgv:GridViewDataTextColumn FieldName="UserEmail" />
                <dxwgv:GridViewDataTextColumn FieldName="NodeName" />
                <dxwgv:GridViewDataTextColumn FieldName="NodePath" />
                <dxwgv:GridViewDataTextColumn FieldName="Inconsistency" Width="80" Settings-AllowAutoFilter="False">
                    <PropertiesTextEdit DisplayFormatString="n4"></PropertiesTextEdit>
                </dxwgv:GridViewDataTextColumn> 
                <dxwgv:GridViewDataTextColumn Width="80" FieldName="NumberOfChildren" />
                <dxwgv:GridViewDataTextColumn Caption="Action" Settings-AllowGroup="False" Settings-AllowSort="False" Settings-ShowInFilterControl="False" Settings-AllowAutoFilter="False" CellStyle-HorizontalAlign="Center" EditCellStyle-VerticalAlign="Middle">
                    <DataItemTemplate><a href="" onclick="ShowInconsPipe('<%#Eval("UserEmail")%>', '<%#Eval("NodeGUID")%>'); return false"><i class='icon-main fas fa-eye' title="<%= SafeFormString(ResString("lblEvalProgressHintUser"))%>"></i><%  If CanOpenPipe AndAlso (Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY) Then %>&nbsp;<a href="" onclick="OpenUserPipe('<%#Eval("UserEmail")%>', '<%#Eval("NodeGUID")%>'); return false"><i class='icon-main fas fa-sign-in-alt' title="<%= SafeFormString(ResString("btnGetAndOpenAnytimeEvalLink"))%>"></i><% End If%></DataItemTemplate>
                </dxwgv:GridViewDataTextColumn>
            </Columns>
        </dxwgv:ASPxGridView>
</dxw:ContentControl></ContentCollection>
</dxtc:TabPage>

<dxtc:TabPage Visible="True" Text="Survey results">
<ContentCollection><dxw:ContentControl ID="ContentControl6" runat="server">

<asp:ObjectDataSource ID="dsRespondentAnswers" runat="server" 
    TypeName="SpyronControls.Spyron.DataSources.clsRespondentAnswersDS" 
    SelectMethod="SelectAll">
    <SelectParameters>
        <asp:Parameter Name="GroupFilterGUID" Type="String" DefaultValue="-1" />
    </SelectParameters>
</asp:ObjectDataSource>

<asp:ObjectDataSource ID="dsStatisticResults" runat="server" 
    TypeName="SpyronControls.Spyron.DataSources.clsStatisticResultsDS" 
    SelectMethod="SelectAll">
    <SelectParameters>
        <asp:Parameter Name="GroupFilterGUID" Type="String" DefaultValue="-1" />
    </SelectParameters>
</asp:ObjectDataSource>

<asp:ObjectDataSource ID="dsRespondentAnswers2" runat="server" 
    TypeName="SpyronControls.Spyron.DataSources.clsRespondentAnswersDS" 
    SelectMethod="SelectAll">
    <SelectParameters>
        <asp:Parameter Name="GroupFilterGUID" Type="String" DefaultValue="-1" />
    </SelectParameters>
</asp:ObjectDataSource>

<asp:ObjectDataSource ID="dsStatisticResults2" runat="server" 
    TypeName="SpyronControls.Spyron.DataSources.clsStatisticResultsDS" 
    SelectMethod="SelectAll">
    <SelectParameters>
        <asp:Parameter Name="GroupFilterGUID" Type="String" DefaultValue="-1" />
    </SelectParameters>
</asp:ObjectDataSource>

<table border="0" cellpadding="0" cellspacing="0" class="whole" style="padding:1ex"><tr><td valign="top" align="center">
    <h5><asp:Label ID="lblNoWelcomeSurvey" runat="server"></asp:Label></h5>
    <asp:Panel runat="server" Id="PanelWelcomeSurvey" ForeColor="#5D7B9D" BackColor="#F7F6F3" BorderColor="#CCCCCC" HorizontalAlign="Left" BorderWidth="1px" Visible="false">
    <h4><asp:Label runat="server" id="lblSurveyTitle" Text="Welcome survey" />: <asp:Label runat="server" ID="lblWSurveyTitle"></asp:Label></h4>
<div style='padding:6px'>
       
       <asp:Panel runat="server" ID="Panel6" HorizontalAlign="Left">
       <telerik:RadToolbar ID="tbExport6" runat="server" ImagesDir="~/Images/" AutoPostback="true">
            <Items>
                <telerik:RadToolbarButton ID="btnRTF6" runat="server" ButtonImage="RTF.gif" 
                    ButtonText="Export to RTF" CommandName="ExporttoRTF" DisplayType="TextImage" Text="RTF" />
                <telerik:RadToolbarButton ID="btnPDF6" runat="server" ButtonImage="PDF.gif" 
                    ButtonText="Export to PDF" CommandName="ExporttoPDF" DisplayType="TextImage" Text="PDF" />
                <telerik:RadToolbarButton ID="btnXLS6" runat="server" ButtonImage="XLS.gif" 
                    ButtonText="Export to XLS" CommandName="ExporttoXLS" DisplayType="TextImage" Text="XLS" />
            </Items>
        </telerik:RadToolbar>
        
        
        <dxwgv:ASPxGridViewExporter ID="ASPxGridViewExporterResultOverview" runat="server" GridViewID="gvResultOverview">
        </dxwgv:ASPxGridViewExporter>
        
        <dxwgv:ASPxGridView ID="gvResultOverview" runat="server" Width="90%"
            AutoGenerateColumns="true"
            SettingsBehavior-AllowMultiSelection="true"
            KeyFieldName="ID"
            DataSourceID="dsRespondentAnswers">
            <Settings  ShowFilterBar="Visible" ShowFilterRow="true" />
       </dxwgv:ASPxGridView>
       </asp:Panel>
       <br /><br />
       
       <asp:Panel runat="server" ID="Panel7" HorizontalAlign="Left">
       <telerik:RadToolbar ID="tbExport7" runat="server" ImagesDir="Images" AutoPostback="true">
            <Items>
                <telerik:RadToolbarButton ID="btnRTF7" runat="server" ButtonImage="RTF.gif" 
                    ButtonText="Export to RTF" CommandName="ExporttoRTF" DisplayType="TextImage" Text="RTF"/>
                <telerik:RadToolbarButton ID="btnPDF7" runat="server" ButtonImage="PDF.gif" 
                    ButtonText="Export to PDF" CommandName="ExporttoPDF" DisplayType="TextImage" Text="PDF"/>
                <telerik:RadToolbarButton ID="btnXLS7" runat="server" ButtonImage="XLS.gif" 
                    ButtonText="Export to XLS" CommandName="ExporttoXLS" DisplayType="TextImage" Text="XLS"/>
            </Items>
        </telerik:RadToolbar>
        
        
        <dxwgv:ASPxGridViewExporter ID="ASPxGridViewExporterStatisticResults" runat="server" GridViewID="gvStatisticResults">
        </dxwgv:ASPxGridViewExporter>
       
        <dxwgv:ASPxGridView ID="gvStatisticResults" runat="server"
            AutoGenerateColumns="false"
            DataSourceID="dsStatisticResults">
            <Settings ShowGroupPanel="false" />
            <SettingsPager Mode="ShowAllRecords" />
            <Columns>
                <dxwgv:GridViewDataTextColumn Caption="Page" FieldName="Page" Width="100" GroupIndex="0" SortOrder="Ascending" />
                <dxwgv:GridViewDataTextColumn Caption="Question" FieldName="Question" Width="500" GroupIndex="1" SortOrder="Ascending" />
                <dxwgv:GridViewDataTextColumn Caption="Answer" FieldName="Answer" Width="300" CellStyle-Wrap="True" />
                <dxwgv:GridViewDataTextColumn Caption="Percent" FieldName="Percent" Width="50">
                    <PropertiesTextEdit DisplayFormatString="p2"></PropertiesTextEdit>
                </dxwgv:GridViewDataTextColumn> 
                <dxwgv:GridViewDataTextColumn Caption="Respondents Count"  Width="50" FieldName="RespondentsCount" />
                <dxwgv:GridViewDataTextColumn Caption="Graph" Width="210" CellStyle-HorizontalAlign="Left">
                    <DataItemTemplate>
                       <table width="206" style="margin:0; border-width:1px; border-style:solid; border-color:Black" cellpadding="0" cellspacing="2"><tr><td>
                            <img src="Images/bar.gif" style="border-style:none" width="<%#Math.Round(CType(DataBinder.Eval(Container, "DataItem.Percent"), Double) * 200).ToString%>" height="12" />
                       </td></tr></table> 
                    </DataItemTemplate>
                </dxwgv:GridViewDataTextColumn>
            </Columns>
        </dxwgv:ASPxGridView>
        </asp:Panel>
   </div>
    </asp:Panel>
    <br /><hr /><br />
    <h5><asp:Label ID="lblNoThankyouSurvey" runat="server"></asp:Label></h5>
    <asp:Panel runat="server" Id="PanelThankyouSurvey" ForeColor="#5D7B9D" BackColor="#F7F6F3" BorderColor="#CCCCCC" HorizontalAlign="Left" BorderWidth="1px" Visible="false">
    <h4><asp:Label runat="server" Text="Thank you survey" />: <asp:Label runat="server" ID="lblTSurveyTitle"></asp:Label></h4>
<div style='padding:6px'>
       
       <asp:Panel runat="server" ID="Panel8" HorizontalAlign="Left">
       <telerik:RadToolbar ID="tbExport8" runat="server" ImagesDir="~/Images/" AutoPostback="true">
            <Items>
                <telerik:RadToolbarButton ID="btnRTF8" runat="server" ButtonImage="RTF.gif" 
                    ButtonText="Export to RTF" CommandName="ExporttoRTF" DisplayType="TextImage" Text="RTF" />
                <telerik:RadToolbarButton ID="btnPDF8" runat="server" ButtonImage="PDF.gif" 
                    ButtonText="Export to PDF" CommandName="ExporttoPDF" DisplayType="TextImage" Text="PDF" />
                <telerik:RadToolbarButton ID="btnXLS8" runat="server" ButtonImage="XLS.gif" 
                    ButtonText="Export to XLS" CommandName="ExporttoXLS" DisplayType="TextImage" Text="XLS" />
            </Items>
        </telerik:RadToolbar>
        
        
        <dxwgv:ASPxGridViewExporter ID="ASPxGridViewExporterResultOverview2" runat="server" GridViewID="gvResultOverview2">
        </dxwgv:ASPxGridViewExporter>
        
        <dxwgv:ASPxGridView ID="gvResultOverview2" runat="server" Width="90%"
            AutoGenerateColumns="true"
            SettingsBehavior-AllowMultiSelection="true"
            KeyFieldName="ID"
            DataSourceID="dsRespondentAnswers2">
            <Settings  ShowFilterBar="Visible" ShowFilterRow="true" />
       </dxwgv:ASPxGridView>
       </asp:Panel>
       <br /><br />
       
       <asp:Panel runat="server" ID="Panel9" HorizontalAlign="Left">
       <telerik:RadToolbar ID="tbExport9" runat="server" ImagesDir="Images" AutoPostback="true">
            <Items>
                <telerik:RadToolbarButton ID="btnRTF9" runat="server" ButtonImage="RTF.gif" 
                    ButtonText="Export to RTF" CommandName="ExporttoRTF" DisplayType="TextImage" Text="RTF"/>
                <telerik:RadToolbarButton ID="btnPDF9" runat="server" ButtonImage="PDF.gif" 
                    ButtonText="Export to PDF" CommandName="ExporttoPDF" DisplayType="TextImage" Text="PDF"/>
                <telerik:RadToolbarButton ID="btnXLS9" runat="server" ButtonImage="XLS.gif" 
                    ButtonText="Export to XLS" CommandName="ExporttoXLS" DisplayType="TextImage" Text="XLS"/>
            </Items>
        </telerik:RadToolbar>
        
        
        <dxwgv:ASPxGridViewExporter ID="ASPxGridViewExporterStatisticResults2" runat="server" GridViewID="gvStatisticResults2">
        </dxwgv:ASPxGridViewExporter>
       
        <dxwgv:ASPxGridView ID="gvStatisticResults2" runat="server"
            AutoGenerateColumns="false"
            DataSourceID="dsStatisticResults2">
            <Settings ShowGroupPanel="false" />
            <SettingsPager Mode="ShowAllRecords" />
            <Columns>
                <dxwgv:GridViewDataTextColumn Caption="Page" FieldName="Page" Width="100" GroupIndex="0" SortOrder="Ascending" />
                <dxwgv:GridViewDataTextColumn Caption="Question" FieldName="Question" Width="500" GroupIndex="1" SortOrder="Ascending" />
                <dxwgv:GridViewDataTextColumn Caption="Answer" FieldName="Answer" Width="300" CellStyle-Wrap="True" />
                <dxwgv:GridViewDataTextColumn Caption="Percent" FieldName="Percent" Width="50">
                    <PropertiesTextEdit DisplayFormatString="p2"></PropertiesTextEdit>
                </dxwgv:GridViewDataTextColumn> 
                <dxwgv:GridViewDataTextColumn Caption="Respondents Count"  Width="50" FieldName="RespondentsCount" />
                <dxwgv:GridViewDataTextColumn Caption="Graph" Width="210" CellStyle-HorizontalAlign="Left">
                    <DataItemTemplate>
                       <table width="206" style="margin:0; border-width:1px; border-style:solid; border-color:Black" cellpadding="0" cellspacing="2"><tr><td>
                            <img src="Images/bar.gif" style="border-style:none" width="<%#Math.Round(CType(DataBinder.Eval(Container, "DataItem.Percent"), Double)*200).ToString%>" height="12" />
                       </td></tr></table> 
                    </DataItemTemplate>
                </dxwgv:GridViewDataTextColumn>
            </Columns>
        </dxwgv:ASPxGridView>
        </asp:Panel>
   </div>
    </asp:Panel>

</td></tr>
</table>
</dxw:ContentControl></ContentCollection>
</dxtc:TabPage>
<dxtc:TabPage Visible="True" Text="Inconsistencies">
<ContentCollection><dxw:ContentControl ID="ContentControl3" runat="server">
<h4>Select report to download as csv file</h4>
<p align="center"><span class="text infopanel" style="padding:1ex 1em">These reports are compatible with <a href="http://www.tableausoftware.com/" target=_blank>Tableau software</a> for further analysis and visualization.</span></p>
<table style="margin:1em auto">
    <tr class="grid_header text"><td>Report</td><td>Description</td></tr>
    <tr class="grid_row text"><td><a href='<% =PageURL(_PGID_REPORT_GET_DATA, _PARAM_ID + "=1") %>'>Alternatives</a></td><td>Shows alternatives global priorities with attributes</td></tr>
    <tr class="grid_row_alt text"><td><a href='<% =PageURL(_PGID_REPORT_GET_DATA, _PARAM_ID + "=2") %>'>Objectives</a></td><td>Shows Objectives priorities with additional description</td></tr>
    <tr class="grid_row text"><td><a href='<% =PageURL(_PGID_REPORT_GET_DATA, _PARAM_ID + "=3") %>'>Participants</a></td><td>Shows participants information</td></tr>
    <tr class="grid_row_alt text"><td><a href='<% =PageURL(_PGID_REPORT_GET_DATA, _PARAM_ID + "=4") %>'>Judgements</a></td><td>Shows participants judgements</td></tr>
</table>
</dxw:ContentControl></ContentCollection>
</dxtc:TabPage>
</TabPages>
    </dxtc:ASPxPageControl>
<script language='javascript' type='text/javascript'>

    function BtnFlash(tb_name, flash) {
        var tb = $find(tb_name);
        if ((tb)) {
            var btn = tb.get_items().getItem(0);
            if ((btn)) {
                var d = btn.get_middleWrapElement();
                d.style.background = (flash ? "#fff000" : "");
                if (flash) setTimeout("BtnFlash('" + tb_name + "', 0);", 250); 
            }
        }
    }

    function checkFFStyle() {
        $('<style>.dxpcModalBackground { display:none; }</style>').appendTo('body');
        //$(".dxpcModalBackground").hide();
        //setTimeout('checkFFStyle();', 1500);
    }

    function onResize(force, w, h) {
        var p = $("#<% = ASPxPageControlReports.ClientID %>");
        if ((p) && (p.length)) {
            var th = 0;
            var t = $("#<% =pnlReportOptions.clientID %>");
            if ((t) && (t.height)) th = t.height();
            p.height(200).height(h - th - 30);
        }
    }

    if ((is_firefox)) setTimeout('checkFFStyle();', 2000);

    resize_custom = onResize;

</script>
</asp:Content>

