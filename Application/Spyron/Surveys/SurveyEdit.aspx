<%@ Page Language="VB" Inherits="SurveyEdit" title="Edit Survey" Codebehind="SurveyEdit.aspx.vb" %>
<%@ Register Assembly="DevExpress.Web.ASPxGridView.v9.1.Export, Version=9.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxGridView.Export" TagPrefix="dxwgv" %>
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
<%@ Register TagPrefix="Spyron" TagName="SurveyPageView" Src="ctrlSurveyPageView.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<script type="text/javascript">
    <!--
   
    function ConfirmDeleteGroupFilter()
    {
        return (confirm('Are you sure want to delete this Group Filter?'));
    }

    function onButtonClicking(sender, args) {
        var btn = args.get_item();
        if ((btn)) {
            var val = btn.get_value();
            if ((val) && (val == "delete")) {
                if (!confirm("Are you sure?")) args.set_cancel(true);
            }
        }
    }
    
    function onClose() {
        var o = window.opener;
        if ((o)) o.onUpdateSurvey();
    }

    window.onunload = onClose;

    $(document).ready(function () {
        $("textarea").each(function () { $(this).val($(this).val().trim()); });
    });
    // -->
</script>
<%--<asp:ObjectDataSource ID="dsSurveyResults" runat="server" 
    TypeName="SpyronControls.Spyron.DataSources.clsSurveysResultsDS" 
    SelectMethod="SelectAll">
</asp:ObjectDataSource>
<asp:ObjectDataSource ID="dsRespondentAnswers" runat="server" 
    TypeName="SpyronControls.Spyron.DataSources.clsRespondentAnswersDS" 
    SelectMethod="SelectAll">
    <SelectParameters>
        <asp:Parameter Name="GroupFilterGUID" Type="String" DefaultValue="-1" />
    </SelectParameters>
</asp:ObjectDataSource>--%>
<%--'L0058 ===--%>
<%--<asp:ObjectDataSource ID="dsGroupFilter" runat="server" 
    TypeName="SpyronControls.Spyron.DataSources.clsGroupFilterDS" 
    SelectMethod="SelectAll">
</asp:ObjectDataSource>--%>
<%--'L0058 ==--%>
<asp:ObjectDataSource ID="dsRespondents" runat="server" 
    TypeName="SpyronControls.Spyron.DataSources.clsRespondentsDS" 
    SelectMethod="SelectAll"
    UpdateMethod="Update">
    <UpdateParameters>
        <asp:FormParameter Name="GroupID" FormField="GroupID" Type="Int32" DefaultValue="-1"/>
    </UpdateParameters>
    <SelectParameters>
        <asp:Parameter Name="GroupID" Type="Int32" DefaultValue="-1" />
    </SelectParameters>
</asp:ObjectDataSource>
<asp:ObjectDataSource ID="dsRespondentGroups" runat="server" 
    TypeName="SpyronControls.Spyron.DataSources.clsRespondentGroupsDS" 
    SelectMethod="SelectAll"
    InsertMethod="Insert"
    DeleteMethod="Delete"
    UpdateMethod="Update">
    <InsertParameters>
        <asp:FormParameter Name="GroupName" FormField="Group Name" Type="String" DefaultValue=""/>
        <asp:FormParameter Name="Comments" FormField="Comments" Type="String" DefaultValue=""/>
        <%--<asp:FormParameter Name="ID" FormField="ID" Type="Int32" DefaultValue="-1"/>--%>
    </InsertParameters>
    <UpdateParameters>
        <asp:FormParameter Name="GroupName" FormField="Group Name" Type="String" DefaultValue=""/>
        <asp:FormParameter Name="Comments" FormField="Comments" Type="String" DefaultValue=""/>
    </UpdateParameters>
</asp:ObjectDataSource>

<%--<asp:ObjectDataSource ID="dsStatisticResults" runat="server" 
    TypeName="SpyronControls.Spyron.DataSources.clsStatisticResultsDS" 
    SelectMethod="SelectAll">
    <SelectParameters>
        <asp:Parameter Name="GroupFilterGUID" Type="String" DefaultValue="-1" />
    </SelectParameters>
</asp:ObjectDataSource>--%>

<table border="0" cellpadding="0" cellspacing="0" class="whole">
<% If CheckVar("close", False) Then%><tr><td align="center"><asp:Button ID="btnClose" runat="server" Text="Close" CssClass="button" OnClientClick="window.close()" /></td></tr><% End If %>
<tr><td valign="top" align="center"><%--<asp:ImageButton ID="imgBtnRefresh" runat="server" SkinID="RefreshImageButton" OnClick="imgBtnRefresh_Click"/><h4 style='margin-top:1ex'>--%><h4 style='margin-top:1ex'><% =ASurveyInfo.Title %></h4>
<dxtc:ASPxPageControl id="ASPxPageControlSpyron" runat="server" ActiveTabIndex="1"
        TabPosition="Top" Paddings-Padding="0px" Width="100%"
        ContentStyle-CssClass="text" ContentStyle-HorizontalAlign="Center" 
        ContentStyle-VerticalAlign="Top">
        <tabpages>
        
<dxtc:TabPage Name="tabProperties" Text="Properties">
    <ContentCollection><dxw:ContentControl runat="server">
        <asp:Panel runat="server" Id="Panel4" >
       
            <div>
            <asp:Table runat="server" BorderWidth="0px" ID="Table1"><asp:TableRow runat="server" CssClass="text" VerticalAlign="Middle"><asp:TableCell runat="server" HorizontalAlign="Right" VerticalAlign="Top"><%=ResString("Survey_lblTitle")%>:</asp:TableCell>
                <asp:TableCell runat="server"><asp:TextBox runat="server" Width="300px" ID="tbTitle"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server" CssClass="text" VerticalAlign="Middle"><asp:TableCell runat="server" HorizontalAlign="Right" VerticalAlign="Top"><%=ResString("Survey_lblComments")%>:</asp:TableCell>
                <asp:TableCell runat="server"><asp:TextBox runat="server" Rows="10" Width="300px" ID="tbComments" TextMode="MultiLine"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server" CssClass="text" VerticalAlign="Middle"><asp:TableCell runat="server" HorizontalAlign="Right" VerticalAlign="Top"></asp:TableCell>
                <asp:TableCell runat="server"><asp:CheckBox runat="server" ID="cbShowNumbers" Text="Hide Index Numbers for Questions"></asp:CheckBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server"><asp:TableCell ID="TableCell1" runat="server" HorizontalAlign="Right" VerticalAlign="Top"></asp:TableCell>
                <asp:TableCell runat="server"><asp:Button ID="btnUpdateProperties" runat="server" Text="Save" CssClass="button" /></asp:TableCell>
            </asp:TableRow>
            </asp:Table></div>
       
        </asp:Panel>     
    </dxw:ContentControl></ContentCollection>
</dxtc:TabPage>

<dxtc:TabPage Name="Pages" Text="Pages">
    <ContentCollection><dxw:ContentControl runat="server">
    <asp:Panel runat="server" Id="Panel5"  Height="95%" Width="100%" >
    

    <asp:Panel runat="server" ID="ToolbarPanel" HorizontalAlign="Left" CssClass="tobbuttonbar">
        <table width="100%"><tr>
        <td width="50%">
        <telerik:RadToolbar ID="tbPageEdit" runat="server" ImagesDir="~/Images/" AutoPostback="true" OnClientButtonClicking="onButtonClicking">
            <Items>
                <telerik:RadToolbarButton ID="btnNewQuestion" runat="server" ButtonImage="IconNewRadioList.gif" 
                    ButtonText="New Question" CommandName="NewQuestion" DisplayType="TextImage" 
                    Hidden="False" CssClass="highlight_btn" />
                <%--<telerik:RadToolBarButton runat="server" id="sep1" IsSeparator="true" />--%>    
                <telerik:RadToolbarButton ButtonImage="IconNewPage.gif" ID="btnNewPage" 
                    runat="server" CommandName="NewPage" ToolTip="" ButtonText="New Page" CommandArgument="NewPage" DisplayType="TextImage"/>
    
<%--    <rad:RadToolbarTemplateButton>
        <ButtonTemplate>
            <asp:TextBox ID="tbNewPageTitle" runat="server" TextMode="SingleLine"></asp:TextBox>
        </ButtonTemplate>
    </rad:RadToolbarTemplateButton>        
    <rad:RadToolbarButton ButtonImage="IconEdit.gif" ID="btnRenamePage"
        runat="server" CommandName="RenamePage" ToolTip="" ButtonText="Rename Page" CommandArgument="RenamePage" DisplayType="TextImage"/>
    
    <rad:RadToolbarSeparator />--%>

                <telerik:RadToolbarButton ID="btnPageUp" runat="server" ButtonImage="IconMovePageUp.gif" 
                    ButtonText="Move Page Up" CommandName="PageUp" DisplayType="TextImage" 
                    Hidden="False" />
                <telerik:RadToolbarButton ID="btnPageDown" runat="server" ButtonImage="IconMovePageDown.gif" 
                    ButtonText="Move Page Down" CommandName="PageDown" DisplayType="TextImage" 
                    Hidden="False" />
                <%--<telerik:RadToolBarButton runat="server" ID="sep2" IsSeparator="true" />--%>
                <telerik:RadToolbarButton runat="server" ButtonImage="IconDeletePage.gif" ID="btnRemovePage" Value="delete"
                    ButtonText="Remove Page" CommandName="RemovePage" DisplayType="TextImage" Hidden="False" />
            </Items>
        </telerik:RadToolbar>
        </td>
<%--<rad:RadToolbar ID="tbQuestionEdit" runat="server" ImagesDir="Images/" AutoPostback="true">
<Items>
    <rad:RadToolbarButton ID="btnNewQuestion" runat="server" ButtonImage="IconNewRadioList.gif" 
        ButtonText="New Question" CommandName="NewQuestion" DisplayType="TextImage" 
        Hidden="False" />
    <rad:RadToolbarSeparator />
    <rad:RadToolbarButton ID="btnMoveUp" runat="server" ButtonImage="IconUp.gif" Enabled="true"
        ButtonText="Move Question Up" CommandName="QuestionUp" DisplayType="TextImage" 
        Hidden="False" />
    <rad:RadToolbarButton ID="btnMoveDown" runat="server" ButtonImage="IconDown.gif" Enabled="true"
        ButtonText="Move Question Down" CommandName="QuestionDown" DisplayType="TextImage" 
        Hidden="False" />
    <rad:RadToolbarSeparator />
        <rad:RadToolbarButton ID="btnDeleteQuestion" runat="server" ButtonImage="IconDelete.gif" 
        ButtonText="Delete Question" CommandName="DeleteQuestion" DisplayType="TextImage" 
        Hidden="False" />
        
</Items>
</rad:RadToolbar>--%>
        <td align="right" class="text">
            Pages: <asp:Label ID="PagesNavigator" runat="server" CssClass="text"></asp:Label>
        </td>
        </tr>
        </table>
    </asp:Panel>
    
    <br />
    <asp:Panel runat="server" Width="100%" ID="Panel6" HorizontalAlign="Left">
        <Spyron:SurveyPageView ID="SurveyPage" runat="server" />
    </asp:Panel> 
    <br /><br />
    </asp:Panel> 
    </dxw:ContentControl></ContentCollection>
</dxtc:TabPage>
<%--<dxtc:TabPage Name="RespondentGroups" Text="Respondent Groups" Enabled="true">
    <ContentCollection><dxw:ContentControl runat="server">
        <asp:Panel runat="server" Id="Panel3" ForeColor="#5D7B9D" BackColor="#F7F6F3" BorderColor="#CCCCCC" Height="95%" Width="99%" BorderWidth="1px">
        <br /><br />
        <dxwgv:ASPxGridView ID="ASPxGridViewGroup" runat="server" width="90%"
            AutoGenerateColumns="true"
            KeyFieldName="ID"
            DataSourceID="dsRespondentGroups">
            <Settings ShowFilterRow="false" ShowGroupPanel="false"/>
            <SettingsBehavior ColumnResizeMode="NextColumn" ConfirmDelete="True" />
            <SettingsPager Mode="ShowAllRecords"/>
            <Columns>
                <dxwgv:GridViewCommandColumn NewButton-Visible="true" EditButton-Visible="true" DeleteButton-Visible="true" Width="100">
                    <EditButton Visible="True"></EditButton>
                    <NewButton Visible="True"></NewButton>
                    <DeleteButton Visible="True"></DeleteButton>
                </dxwgv:GridViewCommandColumn>
                
                <dxwgv:GridViewDataTextColumn Name="GroupName" Caption="Group Name" FieldName="GroupName"></dxwgv:GridViewDataTextColumn>
                <dxwgv:GridViewDataMemoColumn Name="Comments" Caption="Comments" FieldName="Comments" PropertiesMemoEdit-Rows="5">
                    <PropertiesMemoEdit Rows="5"></PropertiesMemoEdit>
                </dxwgv:GridViewDataMemoColumn>
                <dxwgv:GridViewDataTextColumn Name="Respondents" Width="100" Caption="Respondents" FieldName="Respondents" ReadOnly="true" EditFormSettings-Visible="False">
                    <EditFormSettings Visible="False"></EditFormSettings>
                </dxwgv:GridViewDataTextColumn>
            </Columns>
        </dxwgv:ASPxGridView>
        <br /><br />
        </asp:Panel> 
    </dxw:ContentControl></ContentCollection>
</dxtc:TabPage>--%>

<%--<dxtc:TabPage Name="Respondents" Text="Respondents" Enabled="true" Visible="false">
    <ContentCollection><dxw:ContentControl runat="server">
    <asp:Panel runat="server" Id="Panel2" ForeColor="#5D7B9D" BackColor="#F7F6F3" BorderColor="#CCCCCC" Height="95%" Width="99%" BorderWidth="1px">
        <br /><br />
        <dxwgv:ASPxGridView ID="gvRespondentGroups" runat="server" Width="90%"
            AutoGenerateColumns="true"
            KeyFieldName="ID"
            DataSourceID="dsRespondentGroups">
            <Settings ShowFilterRow="false" ShowGroupPanel="false" />
            <SettingsBehavior ConfirmDelete="True" />
            <SettingsPager Mode="ShowAllRecords"/>
            <Columns>
                <dxwgv:GridViewCommandColumn NewButton-Visible="true" EditButton-Visible="true" DeleteButton-Visible="true" Width="100">
                    <EditButton Visible="True"></EditButton>
                    <NewButton Visible="True"></NewButton>
                    <DeleteButton Visible="True"></DeleteButton>
                </dxwgv:GridViewCommandColumn>
                <%--<dxwgv:GridViewDataTextColumn Name="ID" Width="35" Caption="ID" FieldName="ID" ReadOnly="true" EditFormSettings-Visible="False"></dxwgv:GridViewDataTextColumn>--%>
<%--                <dxwgv:GridViewDataTextColumn Name="GroupName" Caption="Group Name" FieldName="GroupName"></dxwgv:GridViewDataTextColumn>
                <dxwgv:GridViewDataMemoColumn Name="Comments" Caption="Comments" FieldName="Comments" PropertiesMemoEdit-Rows="5">
                    <PropertiesMemoEdit Rows="5"></PropertiesMemoEdit>
                </dxwgv:GridViewDataMemoColumn>--%>
               <%-- <dxwgv:GridViewDataTextColumn Name="Respondents" Caption="Respondents" FieldName="Respondents" ReadOnly="true" EditFormSettings-Visible="False">
                    <EditFormSettings Visible="False"></EditFormSettings>
                </dxwgv:GridViewDataTextColumn>--%>
            <%--</Columns>
            <Templates>
                <DetailRow>
                    <dxwgv:ASPxGridView ID="ASPxGridViewRespondents" runat="server" Width="100%"
                        AutoGenerateColumns="true"
                        KeyFieldName="ID"
                        DataSourceID="dsRespondents" 
                        OnBeforePerformDataSelect="detailGrid_DataSelect" OnRowUpdated="ASPxGridViewRespondents_RowUpdated">
                      
                        <SettingsPager PageSize="30"></SettingsPager>
                        <Columns>
                            <dxwgv:GridViewCommandColumn Name="Change Respondent Group" EditButton-Visible="true"></dxwgv:GridViewCommandColumn>
                            <dxwgv:GridViewDataTextColumn Name="RespondentEmail" Caption="Respondent Email" FieldName="RespondentEmail" ReadOnly="true" EditFormSettings-Visible="False"></dxwgv:GridViewDataTextColumn>
                            <dxwgv:GridViewDataTextColumn Name="RespondentName" Caption="Respondent Name" FieldName="RespondentName" ReadOnly="true" EditFormSettings-Visible="False"></dxwgv:GridViewDataTextColumn>
                            <dxwgv:GridViewDataComboBoxColumn Name="GroupID" Caption="Respondent Group" FieldName="GroupID" PropertiesComboBox-DataSourceID="dsRespondentGroups" PropertiesComboBox-ShowShadow="true" PropertiesComboBox-TextField="GroupName" PropertiesComboBox-ValueField="ID" PropertiesComboBox-ValueType="System.Int32"> </dxwgv:GridViewDataComboBoxColumn>
                            <dxwgv:GridViewDataTextColumn Name="GivenAnswers" Caption="Given Answers" FieldName="GivenAnswers" ReadOnly="true" EditFormSettings-Visible="False"></dxwgv:GridViewDataTextColumn>
                        </Columns>
                        <SettingsDetail IsDetailGrid="true" />
                    </dxwgv:ASPxGridView>
                </DetailRow>
            </Templates>
            <SettingsDetail ShowDetailRow="true" AllowOnlyOneMasterRowExpanded="true" />
        </dxwgv:ASPxGridView>

        <br /><br />
    </asp:Panel> 
    </dxw:ContentControl></ContentCollection>
</dxtc:TabPage>--%>

<%--<dxtc:TabPage Name="Rules" Text="Rules" Enabled="false" Visible="false">
</dxtc:TabPage>--%>

<%--<dxtc:TabPage Name="Analysis" Text="Result Analysis" Enabled="true" Visible="false">
    <ContentCollection><dxw:ContentControl runat="server">
        
        <dxwpg:ASPxPivotGrid ID="ASPxPivotGridSurveyAnalysis" runat="server" DataSourceID="dsSurveyResults">
        <OptionsView ShowColumnTotals="False" ShowRowTotals="False" ShowColumnGrandTotals="False" ShowRowGrandTotals="False"></OptionsView>
        <Fields>
            <dxwpg:PivotGridField FieldName="RespondentName" Area="RowArea" ID="_RespondentName" AreaIndex="0" Caption="Respondent" ExpandedInFieldsGroup="false"></dxwpg:PivotGridField>
            <dxwpg:PivotGridField FieldName="QuestionText" Area="ColumnArea" ID="_QuestionText" AreaIndex="0" Caption="Question"></dxwpg:PivotGridField>
            <dxwpg:PivotGridField FieldName="AnswerText" Area="ColumnArea" ID="_AnswerText" AreaIndex="1" Caption="Answers"></dxwpg:PivotGridField>
            <dxwpg:PivotGridField FieldName="Selected" Area="DataArea" ID="_SelectedField" AreaIndex="0" Caption="Selected"></dxwpg:PivotGridField>
        </Fields>
        </dxwpg:ASPxPivotGrid>
    </dxw:ContentControl></ContentCollection>
</dxtc:TabPage>--%>
<%--'L0005 === --%>
<%--<dxtc:TabPage Name="GroupFilterTab" Text="Create Group Filter" Enabled="true">
    <ContentCollection><dxw:ContentControl runat="server">
    <asp:UpdatePanel runat="server">
    <ContentTemplate>
        <asp:Wizard runat="server" id="FilterWizard" Height="90%" Width="90%" 
            BackColor="#F7F6F3" BorderColor="#CCCCCC" DisplaySideBar="false"
            BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em">
            <HeaderStyle BackColor="#5D7B9D" BorderStyle="Solid" Font-Bold="True" 
                Font-Size="0.9em" ForeColor="White" HorizontalAlign="Left" />
            <NavigationButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" 
                BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" 
                ForeColor="#284775" />
            <SideBarButtonStyle BorderWidth="0px" Font-Names="Verdana" ForeColor="White" />
            <SideBarStyle BackColor="#7C6F57" BorderWidth="0px" Font-Size="0.9em" 
                VerticalAlign="Top" />
            <StepStyle BorderWidth="0px" ForeColor="#5D7B9D" HorizontalAlign="Center" VerticalAlign="Top"  />
        <WizardSteps>
            <asp:WizardStep Title="Select question to create filter:" ID="FTQStep" runat="server">
                <br />
                <asp:Label ID="lblQuestionStepTitle" runat="server" Text="Specify questions to use in filter:" Font-Size="Small" CssClass="Header"></asp:Label>
                <br /><br />
                <asp:Table runat="server" Id="FTQuestions" CssClass="grid_header" Font-Size="X-Small" Width="90%"></asp:Table>
            </asp:WizardStep>
            <asp:WizardStep Title="Specify answers for selected questions:" ID="FTAStep" runat="server">
                <br />
                <asp:Label ID="lblAnswersStepTitle" runat="server" Text="Specify answers for selected questions:" Font-Size="Small" CssClass="Header"></asp:Label>
                <br /><br />
                <asp:Table runat="server" Id="FTAnswers" CssClass="grid_header" Font-Size="X-Small" Width="90%"></asp:Table>
            <dxe:ASPxFilterControl ID="fcGroupFilter" runat="server"  EnableCallbackCompression="true" >
            </dxe:ASPxFilterControl>
         </asp:WizardStep>
            <asp:WizardStep ID="FTGStep" runat="server">
                <br />
                <table id="tblGroupInfo" width="100%">
                <tr>
                <td valign="top" align="right"><asp:Label ID="lblGroupName" runat="server" Text="Group name: "></asp:Label></td>
                <td><asp:TextBox runat="server" TextMode="SingleLine" ID="TBGroupName" Width="300"></asp:TextBox></td>
                </tr>
                <tr>
                <td valign="top" align="right"><asp:Label ID="lblComments" runat="server" Text="Comments: "></asp:Label></td>
                <td><asp:TextBox runat="server" TextMode="MultiLine" ID="TBGroupComments" Width="300" Rows="5"></asp:TextBox></td>
                </tr>
                <tr>
                <td valign="top" align="right"><asp:Label ID="lblRuleLabel" runat="server" Text="Rule: "></asp:Label></td>
                <td valign="top"><asp:Label ID="lblRule" runat="server" Text=""></asp:Label></td>
                </tr>
                </table>
                
                <br /><br />
                <asp:Label ID="lblRespondentsApplyed" runat="server" Text="Respondents Applyed to this Group:" Font-Size="Small" CssClass="Header"></asp:Label>
                <br /><br />
                <asp:Table runat="server" Id="FTRespondents" CssClass="grid_header" Font-Size="X-Small" Width="90%"></asp:Table>
                <br /><br />
                 <dxwgv:ASPxGridView ID="gvGroupFilter" runat="server" 
                    AutoGenerateColumns="true"
                    KeyFieldName="ID"
                    DataSourceID="dsGroupFilter" Settings-ShowFooter="true">
                    <TotalSummary>
                    <dxwgv:ASPxSummaryItem FieldName="RespondentEmail" SummaryType="Count" />
                    </TotalSummary> 
                </dxwgv:ASPxGridView>
             </asp:WizardStep>
        </WizardSteps>
        </asp:Wizard>
        </ContentTemplate>
        </asp:UpdatePanel>
    </dxw:ContentControl></ContentCollection>
</dxtc:TabPage>--%>
<%--<dxtc:TabPage Name="GroupResponseOverview" Text="Group Filters" Enabled="true">
    <ContentCollection><dxw:ContentControl runat="server">
    <asp:Panel runat="server" Id="ResultsPanel" ForeColor="#5D7B9D" BackColor="#F7F6F3" BorderColor="#CCCCCC" Height="95%" Width="99%" BorderWidth="1px">
        <br /><br />
        <div>
            <asp:Repeater ID="repGroupFilters" runat="server">
                <HeaderTemplate><table width="90%"></HeaderTemplate>
                <FooterTemplate></table></FooterTemplate>
                <ItemTemplate>
                    <tr style="background-color:White">
                        <td width="20px"><a href='SurveyEdit.aspx?delGroupFilter=<%#DataBinder.Eval(Container,"DataItem.AGUID").ToString%>' onclick='return ConfirmDeleteGroupFilter();'><img src="Images/IconDelete.gif" border="0" alt="Delete"/></a> </td>
                        <td>
                            <b><a href='GroupFilter.aspx?id=<%#DataBinder.Eval(Container,"DataItem.AGUID").ToString%>' title='<%#IIf(CStr(DataBinder.Eval(Container, "DataItem.ReadableRule")) = "", "", "Rule: " + SafeFormString(CStr(DataBinder.Eval(Container, "DataItem.ReadableRule"))))%>'> <%#DataBinder.Eval(Container, "DataItem.Name").ToString%></a></b>
                            <br />
                            <%#IIf(CStr(DataBinder.Eval(Container, "DataItem.Comments")) = "", "<div class='small'>&nbsp;</div>", "<div class='small'>Comment: " + SafeFormString(CStr(DataBinder.Eval(Container, "DataItem.Comments"))) + "</div>")%>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <br />
        <asp:Button ID="btnNewFilter" runat="server" Text="Create new filter" Width="150" CssClass="button" />--%>
    <%--    <a href="GroupFilter.aspx?action=new">Create new filter</a>--%>

<%--       <br />
       <asp:Label ID="lblGroupResponseOverview" runat="server" Text="Group Response Overview" Font-Size="Small" CssClass="Header"></asp:Label>
       <br /><br /><br />
       <asp:Table runat="server" Id="FTResultsHeader" CssClass="grid_header" Font-Size="X-Small" Width="99%">
            <asp:TableHeaderRow CssClass="h5">
                <asp:TableHeaderCell>
                    <asp:Label ID="lblRespondentGroup" runat="server" Text="Respondent Group: "></asp:Label>
                    <asp:DropDownList runat="server" ID="DDGroup" AutoPostBack="true"></asp:DropDownList>
                    <asp:Button runat="server" ID="btnDeleteGroupFilter" Text="Delete selected Group Filter" />
                </asp:TableHeaderCell>
                <asp:TableHeaderCell>
                    <asp:Label ID="lblRespondentsCount" runat="server" Text="Respondents: xxx of xxx"></asp:Label>
                    <asp:Button runat="server" ID="btnRefreshGroupFilters" Text="Refresh Filters" />
                </asp:TableHeaderCell>
            </asp:TableHeaderRow>
       </asp:Table>--%>
<%--       <br />
       <asp:Table runat="server" Id="FTResults" CssClass="grid_header" Font-Size="X-Small" Width="99%"></asp:Table>
       <br /><br />
    </asp:Panel>
    </dxw:ContentControl></ContentCollection>
</dxtc:TabPage>--%>
<%--'L0005 == --%>
</tabpages>

<ContentStyle HorizontalAlign="Center" VerticalAlign="Top" CssClass="text"></ContentStyle>

</dxtc:ASPxPageControl></td></tr>
<%--<tr><td valign="bottom" style='height:1%; text-align:left; padding:1ex 0px 0.5ex 1.2ex'>--%>
<%--<asp:Button runat="server" ID="btnBack" CssClass="button" Width="12em" />--%>
<%--<asp:Button runat="server" ID="btnViewResults" CssClass="button" Width="12em"/>--%>
<%--<asp:Button runat="server" ID="btnSurveyList" CssClass="button" Width="12em" Visible="false" />--%>
<%--</td></tr>--%>
</table>
</asp:Content>