<%@ Control Language="VB" Inherits="SpyronPageControls.Spyron_Surveys_ctrlSurveyPage" EnableTheming="true"  Strict="false" Codebehind="ctrlSurveyPage.ascx.vb" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%--<%@ Register TagPrefix="Spyron" TagName="SurveyQuestion" Src="ctrlQuestionForm.ascx" %>--%>

<asp:UpdatePanel ID="UpPanel" runat="server" UpdateMode="always">
<ContentTemplate>
<asp:Panel runat="server" Height="60" ID="ToolbarPanel">
<telerik:RadToolbar ID="tbPageEdit" runat="server" AutoPostback="true">
<Items>
    <telerik:RadToolbarButton ImageURL="Images/IconNewPage.gif" 
        runat="server" CommandName="NewPage" ToolTip="" Text="New Page" CommandArgument="NewPage"/>
    <telerik:RadToolBarButton >
        <ItemTemplate>
            <asp:TextBox ID="tbNewPageTitle" runat="server" TextMode="SingleLine"></asp:TextBox>
        </ItemTemplate> 
    </telerik:RadToolBarButton>        
    <telerik:RadToolbarButton ImageURL="Images/IconEdit.gif" runat="server" CommandName="RenamePage" ToolTip="" Text="Rename Page" CommandArgument="RenamePage"/>
    
    <telerik:RadToolBarButton IsSeparator="true" />

    <telerik:RadToolbarButton runat="server" ImageURL="Images/IconMovePageUp.gif" 
        Text="Move Page Up" CommandName="PageUp" DisplayType="TextImage" 
        Hidden="False" />
    <telerik:RadToolbarButton runat="server" ImageURL="Images/IconMovePageDown.gif" 
        Text="Move Page Down" CommandName="PageDown" DisplayType="TextImage" 
        Hidden="False" />
    <telerik:RadToolBarButton IsSeparator="true" />
    <telerik:RadToolbarButton runat="server" ImageURL="Images/IconDeletePage.gif"
        Text="Remove Page" CommandName="RemovePage" DisplayType="TextImage" 
        Hidden="False" />
</Items>
</telerik:RadToolbar>

<telerik:RadToolbar ID="tbQuestionEdit" runat="server" AutoPostback="true">
<Items>
    <telerik:RadToolbarButton runat="server" ImageURL="Images/IconNewRadioList.gif" 
        Text="New Question" CommandName="NewQuestion"
        Hidden="False" />
    <telerik:RadToolBarButton IsSeparator="true" />
    <telerik:RadToolbarButton runat="server" ImageURL="Images/IconUp.gif" Enabled="true"
        Text="Move Question Up" CommandName="QuestionUp"
        Hidden="False" />
    <telerik:RadToolbarButton runat="server" ImageURL="Images/IconDown.gif" Enabled="true"
        Text="Move Question Down" CommandName="QuestionDown"
        Hidden="False" />
    <telerik:RadToolBarButton IsSeparator = "true" />
<%--    <telerik:RadToolbarButton ID="btnEditQuestion" runat="server" ImageURL="IconEdit.gif" 
        ButtonText="Edit Question" CommandName="EditQuestion" DisplayType="TextImage" 
        Hidden="False" 
        onclick="window.open('QuestionEditor.aspx', 'QuestionEditor', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes');" />
--%>   
        <telerik:RadToolbarButton runat="server" ImageURL="Images/IconDelete.gif" 
        Text="Delete Question" CommandName="DeleteQuestion"
        Hidden="False" />
        
</Items>
</telerik:RadToolbar>
</asp:Panel>

<asp:Table BorderWidth="0" width="98%" height="90%" runat="server">
<asp:TableRow runat="server" Height="92%">

<asp:TableCell runat="server" ID="PageEditArea" Width="160px">
 <asp:Panel ID="PageEditPanel" runat="server" Width="100%" Height="100%" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em">
    <asp:Table runat="server" ID="PagerTable" BorderColor="0">
        
    </asp:Table>
 </asp:Panel>
</asp:TableCell>

<asp:TableCell runat="server">
<%--<asp:Wizard ID="SurveyWizard" Width="100%" Height="100%" runat="server" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em">
    <WizardSteps>
       <asp:WizardStep ID="DefaultPage" Title="DefaultPage" runat="server"></asp:WizardStep>
    </WizardSteps>    
    <StepStyle Font-Size="0.8em" ForeColor="#333333" VerticalAlign="Top" />
    <SideBarStyle BackColor="#507CD1" Font-Size="10pt" VerticalAlign="Top" Width="120px" />
    <NavigationButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid"
        BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284E98" />
   
    <SideBarButtonStyle BackColor="#406CC1" Font-Names="Verdana" ForeColor="White" Font-Size="8pt" Width="120px" />
    
    <HeaderStyle BackColor="#284E98" BorderColor="#EFF3FB" BorderStyle="Solid" BorderWidth="2px"
        Font-Bold="True" Font-Size="0.9em" ForeColor="White" HorizontalAlign="Center" />
 </asp:Wizard>--%>
 <asp:Panel ID="Panel1" runat="server" Width="100%" Height="100%" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em">
    <asp:MultiView ID="SurveyWizard" runat="server">
    </asp:MultiView>
 </asp:Panel>
<%--    <asp:Panel ID="SurveyWizard" runat="server" Width="100%" Height="100%" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em">
    </asp:Panel>
--%> 
</asp:TableCell> 
 <asp:TableCell runat="server" ID="QuestionEditPanel" Font-Size="X-Small" BorderWidth="1" VerticalAlign="Top" Width="10%">
 
 <asp:Label runat="server" Font-Bold="true" Text="Question Name:"></asp:Label><br />
 <asp:TextBox runat="server" ID="eQuestionName" Width="250px"></asp:TextBox>
 <br />
 
 <asp:Label ID="Label1" runat="server" Font-Bold="true" Text="Question Text:"></asp:Label><br />
 <asp:TextBox runat="server" ID="eQuestionText" TextMode="MultiLine" Width="250px" Rows="3"></asp:TextBox><br /><br />
 
 <asp:Label ID="Label2" runat="server" Font-Bold="true" Text="Question Type:"></asp:Label><br />
 <asp:RadioButtonList runat="server" ID="eQuestionType" Font-Size="Smaller" AutoPostBack="true">
    <asp:ListItem Text="Comment" Value="0"></asp:ListItem>
    <asp:ListItem Text="Open One Line Question" Value="1"></asp:ListItem>
    <asp:ListItem Text="Open Multi Line Question" Value="2"></asp:ListItem>
    <asp:ListItem Text="Select Radio" Value="3"></asp:ListItem>
    <asp:ListItem Text="Select Checkbox" Value="4"></asp:ListItem>
    <asp:ListItem Text="Select Dropdown List" Value="5"></asp:ListItem>
    <asp:ListItem Text="Select Objectives" Value="12"></asp:ListItem>
    <asp:ListItem Text="Select Alternatives" Value="13"></asp:ListItem>
 </asp:RadioButtonList>
 <br />
 
 <asp:Label ID="Label3" runat="server" Font-Bold="true" Text="Answers:"></asp:Label><br />
 <asp:TextBox runat="server" ID="eAnswers" TextMode="MultiLine" Width="250px" Rows="5"></asp:TextBox>
 <br />
 <asp:CheckBox ID="eOther" runat="server" Text="Allow Other answer" AutoPostBack="true" />
 
 <br /> <br />
 <div style="text-align:center; margin:1em 0px 2em 0px"> 
    <asp:Button ID="btnSave" runat="server" Text="Apply" CssClass="button" />
 </div>
 </asp:TableCell> 
 </asp:TableRow> 
 
 <asp:TableRow>
 <asp:TableCell HorizontalAlign="Right" ColumnSpan="2" runat="server" ID="NavigationCell">
 <asp:Button ID="btnPrevSurveyPage" runat="server" Text="Previous" CssClass="button"/>
 <asp:Button ID="btnNextSurveyPage" runat="server" Text="Next" CssClass="button"/>
 </asp:TableCell>
 </asp:TableRow>
 
 </asp:Table> 
 </ContentTemplate></asp:UpdatePanel>
