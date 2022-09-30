<%@ Page Language="VB" Inherits="NewQuestion" title="New Question Wizard" Codebehind="NewQuestion.aspx.vb" %>
<%@ Register TagPrefix="Spyron" TagName="SurveyQuestion" Src="ctrlSurveyQuestion.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server" >
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/Spyron.js"></script>
<asp:UpdatePanel runat="server" ID="pnlUpdate">
    <ContentTemplate >
    <p align="center">
        <asp:Wizard runat="server" id="NewQuestionWizard" 
            BackColor="#F7F6F3" BorderColor="#CCCCCC" DisplaySideBar="false"
            BorderStyle="Solid" BorderWidth="1px" 
            DisplayCancelButton="true"  CssClass="text">
            <HeaderStyle BackColor="#5D7B9D" BorderStyle="Solid" Font-Bold="True" 
                Font-Size="0.9em" ForeColor="White" HorizontalAlign="Left" />
            <NavigationButtonStyle CssClass="button" />
            <SideBarButtonStyle BorderWidth="0px" Font-Names="Verdana" ForeColor="White" />
            <SideBarStyle BackColor="#7C6F57" BorderWidth="0px" Font-Size="0.9em" 
                VerticalAlign="Top" />
            <StepStyle BorderWidth="1px" BorderColor="#CCCCCC" ForeColor="#3D5B7D" HorizontalAlign="Left" VerticalAlign="Top"  />
        <WizardSteps>
            <asp:WizardStep StepType="Start" ID="WSTextType" runat="server" Title="Input Question Text and Select Question Type:" >
                <div style="padding:1em;  min-width:400px;">
                        <span style="float:left"><b>Question Text</b>:</span>
                            <div id='divPlain' class='text' style='display:<% =iif(EditorMode, "none", "block") %>'><div class='text' style='text-align:right; padding:2px;'><% =GetSwitcher(False, True) + "&nbsp;" + GetSwitcher(True, False)%></div><asp:TextBox runat="server" ID="tbQuestionText" TextMode="MultiLine" Width="400" Rows="4" Height="150"/></div>
                            <div id='divRich' class='text' style='display:<% =iif(EditorMode, "block", "none") %>;'><div class='text' style='text-align:right; padding:2px;'><% =GetSwitcher(False, False) + "&nbsp;" + GetSwitcher(True, True)%></div><iframe src='<% =GetInfodocURL() %>' id='frmRich'  class='text' style='width:398px; height:126px; margin-bottom:2px; border:1px solid #b0b0b0; background:#fafafa;' frameborder='0' allowtransparency='true' onload="parseHTML();"></iframe><div style='text-align:right'><input type='button' class='button button_small' id='btnEdit' value='<% =ResString("mnuRichEditor") %>' onclick="OpenRichEditor('?type=<% =Cint(reObjectType.SurveyQuestion) %>&st=<% =CInt(ASurveyInfo.SurveyType) %>&question=<% =AQuestion.AGUID.ToString() %>&new=1&callback=<% =tbQuestionText.ClientID %>' + (use_plain ? '&fld=<% =tbQuestionText.ClientID %>' : '')); return false;" width='8em' /></div></div>
                            <input type='hidden' name='EditorMode' value='<% =iif(EditorMode, 1, 0) %>' />
                            <input type='hidden' name='isChanged' value='0' defaultValue="0" />
                <br />
                <b>Question Type</b>:<br />
                <asp:ListBox runat="server" ID="lbQuestionType" Width="400" Rows="10" SelectionMode="Single"></asp:ListBox>
                </div>
            </asp:WizardStep>
            
            <asp:WizardStep StepType="Step" ID="WSVariants" runat="server" Title="Input Answers:">
                <div style="padding:1em; min-width:400px;"><b>Answers</b>:<br />
                 <asp:TextBox runat="server" ID="tbVariants" TextMode="MultiLine" Width="400" Rows="10" ></asp:TextBox><br />
                 <asp:CheckBox runat="server" ID="cbAllowOther" Text="Allow other answer" />
                 <input type='hidden' name='EditorMode' value='<% =iif(EditorMode, 1, 0) %>' />
                 <input type='hidden' name='isChanged' value='1' defaultValue="0" />
                </div>
            </asp:WizardStep>

            <asp:WizardStep StepType="Finish" ID="WSAdjustView" runat="server" Title="Preview Created Question:" AllowReturn="false">
                <div style="padding:1em; min-width:400px;">

                    <div  id="divOptions" runat="server" style="padding-bottom:1em;"><b>Question Options</b>:<br /><br />
                        <asp:CheckBox runat="server" ID="cbRequired" Text="Required question" /><br />
                        <asp:CheckBox runat="server" ID="cbSelectAll" Text="Select All By Default" Checked="true" /><br />
                        <div id="divSelCount" runat="server">Minimum selected Answers: <asp:DropDownList runat="server" ID="ddlMinSelected"></asp:DropDownList><br />
                        Maximum selected Answers: <asp:DropDownList runat="server" ID="ddlMaxSelected"></asp:DropDownList><br /></div>
                    </div>

                    <h6 style="padding:1ex 0px 1ex 0px; margin:0px;">Question Preview:</h6>
                    <div style="padding:1ex; background:#ffffff; border:2px dashed #e0e0e0;">
                        <Spyron:SurveyQuestion runat="server" ID="sqQuestionPreview" BackColor="#FFFFFF" ViewMode="1"/>
                    </div>
                 
                    <input type='hidden' name='EditorMode' value='<% =iif(EditorMode, 1, 0) %>' />
                    <input type='hidden' name='isChanged' value='1' defaultValue="0" />
                </div>
            </asp:WizardStep>
        </WizardSteps>
        </asp:Wizard></p>
    </ContentTemplate>
</asp:UpdatePanel>
<script type="text/javascript"><!--

    plain_fld = theForm.<% =tbQuestionText.ClientID %>;
    
    function OpenRichEditor(cmd) 
    {
        window.open('<% =PageURL(_PGID_RICHEDITOR) %>' + cmd, 'RichEditor', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes,width=840,height=500', true);
    }
    function SetSelection(t, cls) {
        $('.' + cls).not(t).prop('checked', t.checked);
    }
    function fixTextarea() {
        $("textarea").each(function () { $(this).val($(this).val().trim()); });
    }

    $( document ).ready(function() {
        fixTextarea();
    });
    // -->
</script>
</asp:Content>