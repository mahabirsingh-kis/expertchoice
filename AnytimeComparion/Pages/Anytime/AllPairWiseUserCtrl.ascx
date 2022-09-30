<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="AllPairWiseUserCtrl.ascx.vb" Inherits="AnytimeComparion.AllPairWiseUserCtrl" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %> 
<%@ Register Src="~/Pages/includes/FramedInfodDocs.ascx" TagPrefix="includes" TagName="FramedInfodDocs" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
<style type="text/css">
    .bodycls {
        margin: 76px;
        float: left;
    }
</style>

<div id="content" runat="server"></div>


<div class="large-12 columns questionsWrap">
    <div id="question" runat="server" visible="false" >
    <includes:QuestionHeader  runat="server" id="QuestionHeader"/>
   <includes:FramedInfodDocs runat="server" id="FramedInfodDocs" /></div>
    <div class="columns tt-auto-resize-content">
        <div id="dvContent" class="bodycls" runat="server"></div>
    </div>

</div>

