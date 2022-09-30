<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlModalProcess" Codebehind="ctrlModalProcess.ascx.vb" %>
<script language="javascript" type="text/javascript">

    var modal_hotkey_old = null;
    var modal_cancel_event = null;
    var modal_disabled_list = [];
    var isIE = ((document.getElementById) || (document.all));

    var modal_disable_form = <% =CStr(IIF(DoDisableForm,  1, 0)) %>;
    
    var modal_prg_fill = '<% = ProgressFillStyle %>';
    var modal_prg_imgname = '<% = ProgressBlankImage %>';
    var modal_prg_maxvalue = <% =ProgressMaxValue %>;
    var modal_prg_height = <% =ProgressHeight %>;
    var modal_prg_width = <% =ProgressWidth %>;
    var modal_prg_padding = <% =ProgressPadding %>;
    
    function ModalGetProgressBar(value, show_perc)
    {
        var fillWidth  = Math.round(value * (modal_prg_width - modal_prg_padding) /  modal_prg_maxvalue);
        if (fillWidth < 1) fillWidth = 1; else if (fillWidth > modal_prg_width - modal_prg_padding) fillWidth = modal_prg_width - modal_prg_padding;
        var style = (value < 0.0001 ? "" : modal_prg_fill);
        var sFill = "<span class='" + style + "' style='width:" + fillWidth + "px;height:100%'><img src='" + modal_prg_imgname + "' width=" + fillWidth + " height=" + (modal_prg_height - modal_prg_padding) + " border=0 title='" + value + "'></span>";
//        var sBar = "<div><div class='progress' style='display:inline; height:" + (modal_prg_height + 2 *modal_prg_padding) + "px;width:" + (modal_prg_width + 2* modal_prg_padding) + "px;padding:" + modal_prg_padding + "px;margin:0px'>" + sFill + "</div>" + (show_perc ? "<div class='text small' style='display:inline; width:3em; text-align:right; margin-bottom:2px'>" + Math.round(100 * value / modal_prg_maxvalue) + "%</div>" : "") + "</div>";
        var sBar = "<div class='progress' style='display:block;height:" + (modal_prg_height) + "px;width:" + (modal_prg_width + 2* modal_prg_padding) + "px;padding:" + modal_prg_padding + "px;margin:0px'>" + sFill + "</div>";
        if (show_perc) sBar = "<table border=0 cellspacing=0 cellpadding=0><tr valign=middle><td>" + sBar + "</td><td style='width:3em;text-align:right' class='text small'>" + Math.round(100 * value / modal_prg_maxvalue) + "%</td></tr></table>";
        return sBar;
    }
    
    function ModalHasParent(ctrl, check_id)
    {
        if (ctrl==null || !ctrl.parentElement) return false;
        if (ctrl.parentElement.id == check_id ) return true; else return ModalHasParent(ctrl.parentElement, check_id);
    }
    
    
    function ModalSwitchMode(progress_name, is_visible, cancel_btn_name, cancel_event_name)
    {
        var m = $get("modaldiv");
        if (m)
        {
            var vis = (m.style.display == "block");
            var do_vis = (is_visible==1 || (!vis && is_visible==-1));
            var display = (!do_vis ? "none" : "block");
            m.style.display = display;
            if (do_vis && (document.body.scrollHeight)) m.style.height = document.body.scrollHeight;

            if (modal_disable_form && isIE)
            {
                var coll = theForm.getElementsByTagName("select");
//                var coll = (do_vis ? theForm.elements : dis_list);
                for (i=0; i<coll.length; i++)
                {
                    if (!ModalHasParent(coll[i], progress_name))
                    {
                        if ((coll[i].options)) coll[i].style.visibility = (do_vis? "hidden" : "visible");
//                          var doit = (!do_vis || !coll[i].disabled);
//                          if (doit)
//                          {
//                            if (do_vis) Array.add(dis_list, coll[i]);
//                            coll[i].disabled = do_vis;
//                            if ((coll[i].options)) coll[i].style.visibility = (do_vis? "hidden" : "visible");
//                          }  
                    }    
                }
            }    
            
            if (progress_name!="")
            {
                var p = $get(progress_name);
                if (p) p.style.display = display;
            }
            
            if (do_vis)
            {
                if (cancel_btn_name!="")
                {
                    var btn = eval("theForm." + cancel_btn_name);
                    if (btn)
                    {
                        theForm.blur();
                        btn.focus();
                        if (!document.onkeydown) 
                        {
                            modal_cancel_event = cancel_event_name;
                            modal_hotkey_old = document.onkeydown;
                            document.onkeydown = ModalHotkeys;
                        }    
                    }
                }
            }
            else // disable modal
            {
                if (modal_cancel_event!="")
                {
                    document.onkeydown = modal_hotkey_old;
                    modal_hotkey_old  = null;
                    modal_cancel_event = "";
                }
            } 
            
        }
        return false;
    }
    
    function ModalHotkeys(event)
    {
       if (!document.getElementById || modal_cancel_event=="") return;
       if (window.event) event = window.event;
       if (event)
        {
         var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);	// D0073
//         if (event.ctrlKey) // D0073
          switch (code)
            {
   	          case 27:
//   	            window.status = modal_cancel_event;
                eval(modal_cancel_event);
               break;
            }
        }
    }
    
    function ModalUpdateControl(control_name, text, is_add_text)
    {
        var m = $get(control_name);
        if (m) m.innerHTML = (is_add_text?  m.innerHTML + text : text);
    }
    
</script> 
<div style="display:none;" class="modalBackground" id="modaldiv"></div>