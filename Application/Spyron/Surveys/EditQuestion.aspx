<%@ Page Language="VB" Inherits="EditQuestion" title="Edit Question" Codebehind="EditQuestion.aspx.vb" %>
<%@ Register TagPrefix="Spyron" TagName="SurveyQuestion" Src="ctrlSurveyQuestion.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server" >
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/Spyron.js"></script>
<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <p align="center">
            <table class="text" border="0" cellspacing="0" cellpadding="0" style="border:1px solid #cccccc; background:#f7f6f3">
                <tr><td style="padding:1em;">
                <table class="text" border="0" style="text-align: left;">
                    <tr>
                        <td valign="top" align="right" class="text" style="padding-top:1.5em;">Question Text:</td>
                        <td><div style='height:174px; width:400px;'>
                            <div id='divPlain' class='text' style='display:<% =iif(EditorMode, "none", "block") %>'><div class='text' style='text-align:right; padding:2px;'><% =GetSwitcher(False, True) + "&nbsp;" + GetSwitcher(True, False)%></div><asp:TextBox runat="server" ID="tbQuestionText" TextMode="MultiLine" Width="400" Rows="4" Height="150"/></div>
                            <div id='divRich' class='text' style='display:<% =iif(EditorMode, "block", "none") %>;'><div class='text' style='text-align:right; padding:2px;'><% =GetSwitcher(False, False) + "&nbsp;" + GetSwitcher(True, True)%></div><iframe src='<% =GetInfodocURL() %>' id='frmRich' class='text' style='width:398px; height:126px; margin-bottom:2px; border:1px solid #b0b0b0; background:#fafafa;' frameborder='0' allowtransparency='true' onload="parseHTML();"></iframe><div style='text-align:right'><input type='button' class='button button_small' id='btnEdit' value='<% =ResString("mnuRichEditor") %>' onclick="OpenRichEditor('?type=<% =Cint(reObjectType.SurveyQuestion) %>&st=<% =CInt(ASurveyInfo.SurveyType) %>&question=<% =AQuestion.AGUID.ToString() %>&new=0&callback=<% =tbQuestionText.ClientID %>' + (use_plain ? '&fld=<% =tbQuestionText.ClientID %>' : '')); return false;" width='8em' /></div></div>
                            <input type='hidden' name='EditorMode' value='<% =iif(EditorMode, 1, 0) %>' />
                            <input type='hidden' name='isChanged' value='0' defaultValue="0" />
                        </div></td>
                    </tr>
                    <tr id="tdType" runat="server">
                        <td valign="top" align="right">Question Type:</td>
                        <td><asp:DropDownList runat="server" ID="lbQuestionType" Width="400" AutoPostBack="true"></asp:DropDownList></td>
                    </tr>
                    <tr id="tdVariants" runat="server">
                        <td valign="top" align="right">Answers:</td>
                        <td><asp:Panel runat="server" ID="panVariantsEdit"/></td>
                    </tr>
                    <tr id="tdAddVar" runat="server" >
                        <td valign="top" align="right">Add New Answers:</td>
                        <td><asp:TextBox runat="server" ID="tbAddVariants" TextMode="MultiLine" Width="400" Rows="5"></asp:TextBox></td>
                    </tr>

                    <tr id="tdOptions" runat="server">
                        <td valign="top" align="right">Question Options:</td>
                        <td><asp:CheckBox runat="server" ID="cbRequired" Text="Required question" /><br />
                            <asp:CheckBox runat="server" ID="cbSelectAllByDefault" Text="Select All By Default" />
                            <div id="divSelCount" runat="server">
                                Minimum selected Answers: <asp:DropDownList runat="server" ID="ddlMinSelected"></asp:DropDownList><br />
                                Maximum selected Answers: <asp:DropDownList runat="server" ID="ddlMaxSelected"></asp:DropDownList>
                            </div> 
                            <div id="divLink" runat="server">Link question to participant attribute: <asp:DropDownList runat="server" ID="ddlAttributes"></asp:DropDownList></div>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <label class="error" visible="false" id="erEmptyFields" runat="server">Please check the question text and Answers. Blank entries are not allowed.</label>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <h6 style="padding:3ex 0px 1ex 0px; margin:0px;">Question Preview:</h6>
                            <div style="padding:1ex; background:#ffffff; border:2px dashed #e0e0e0;">
                                <Spyron:SurveyQuestion runat="server" ID="sqQuestionPreview" BackColor="#FFFFFF" ViewMode="1"/>
                            </div>
                        </td>
                    </tr>
                 </table></td></tr>
                <tr>
                    <td align="right" style="padding:1ex 1em; border-top:1px solid #cccccc;">
                        <asp:Button runat="server" CssClass="button" ID="btnOK" Text="OK" OnClientClick="alert_on_exit = false;" />
                        <asp:Button runat="server" CssClass="button" ID="btnApply" Text="Apply" OnClientClick="alert_on_exit = false;" />
                        <asp:Button runat="server" CssClass="button" ID="btnCancel" Text="Cancel" OnClientClick="alert_on_exit = false;" />
                    </td>
                </tr>
            </table>
        </p>
    </ContentTemplate>
</asp:UpdatePanel>
<script type="text/javascript"><!--

    plain_fld = theForm.<% =tbQuestionText.ClientID %>;
    
        function OpenRichEditor(cmd) 
        {
            window.open('<% =PageURL(_PGID_RICHEDITOR) %>' + cmd, 'RichEditor', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes,width=840,height=500', true);
    }
    function SetSelection(t, cls) {
        $('.'+cls).not(t).prop('checked', t.checked);
    }
    $( document ).ready(function() {
        $("textarea").each(function () { $(this).val($(this).val().trim()); });
    });
    
    // -->
</script>
</asp:Content>