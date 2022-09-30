<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlEvaluationControlBase" Codebehind="ctrlEvaluationControlBase.ascx.vb" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<script language="javascript" type="text/javascript"><!--

    var img_path        = '<% =ImagePath %>';
    
    var img_infodoc1	= new Image; img_infodoc1.src   = img_path + '<% =ImageInfodoc %>';
    var img_infodoc2	= new Image; img_infodoc2.src   = img_path + '<% =ImageInfodocEmpty %>';
   
    function PWFrameLoaded(frm)
    {
        if ((frm) && (frm.style)) { frm.style.backgroundImage='none'; };
//        if ((frm) && (frm.style)) { frm.className=''; };
    }

<% if CanEditInfodocs Then %>    
    function OnToolTipShowHandler(sender, args)
    {
        if ((sender) && (sender.get_text()=="")) args.set_cancel(true);
    }
        
<% End If %>    
//-->
</script>