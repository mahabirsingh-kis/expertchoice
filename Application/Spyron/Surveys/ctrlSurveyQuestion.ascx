<%@ Control Language="VB" AutoEventWireup="false" Inherits="Spyron_Surveys_ctrlSurveyQuestion" Strict="false" Codebehind="ctrlSurveyQuestion.ascx.vb" %>
<asp:Table runat="server" ID="tblQuestionForm" CellPadding="0" CellSpacing="0">
    <asp:TableRow runat="server" ID="trQuestionForm">
        <asp:TableCell runat="server" ID="tcQuestionForm" CssClass="text">
        </asp:TableCell>
    </asp:TableRow>
    <asp:TableRow runat="server">
        <asp:TableCell CssClass="text">
            <asp:Label runat="server" ID="erEmptyFields" Visible="false" CssClass="error" Text="Please check the question text and answers.  Blank entries are not allowed."></asp:Label>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table><% If sInitField <> "" Then %>
<script language="javascript" type="text/javascript"><!--

  $(document).ready(function () {
    s_manual = true; 
    setTimeout("$(':input[type=text]:visible').first().focus();", 50);
    /*$("[id*='<% =sInitField %>']").focus();*/<% if sDoBlur Then %>
    setTimeout("$(':input[type=text]:visible').first().blur();", 150);/*setTimeout('$("[id*=\'<% =sInitField %>\']").blur();', 150);*/<% End if %>
    setTimeout('s_manual = false; $("html, body").scrollTop(0);', 200);
  })

//--></script><% End If%>