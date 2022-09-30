<%@ Page Language="VB" MasterPageFile="~/mpEmpty.master" Inherits="RichEditorPage" Codebehind="RichEditor.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" ID="Scripts" runat="server">
    <script type="text/javascript" src='/Scripts/tinymce/tinymce.min.js'></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
<div id="divClusters" style="display:none;">
    <div style='padding:1em 3ex 1ex 3ex'><div id="divClustersTitle"><b><% =ResString("lblQHApplyToPromt")%></b></div><div id="divList" runat="server"></div></div>
    <div class="text small gray" id="divSelNodes" style="text-align:right"><nobr><a href="" onclick="selNodes(1); return false;" class="action dashed"><% =ResString("lblCheckAllNodes") %></a> | <a href="" onclick="selNodes(0); return false;" class="action dashed"><% =ResString("lblUnCheckAllNodes")%></a> | <a href="" onclick="selNodes(-1); return false;" class="action dashed"><% =ResString("lblInvertCheckAllNodes") %></a></nobr></div>
</div>
<div id="divUpload" class="text" style="z-index:9999; width:400px; position:absolute; left:200px; top:105px; border:1px solid #999999; background:#fafaff; text-align:left; padding:1em 2em 2em 2em; display:none;">
    <h6 style="text-align:left; padding-bottom:12px;"><% =ResString("lblUploadFile")%></h6>
    <%=ResString("lblSelectMHTUpload")%>:<br /><asp:FileUpload onChange="uploadMHT()" ID="FileUpload" runat="server" Width="100%" CssClass="input"/><input type="hidden" name="doUpload" value="" /><br /><br /><input type="button" class="button" name="btnUpload" value="<% =ResString("btnUpload")%>" onclick="uploadMHT();" /> <input type="button" class="button" name="btnCancel" value="<% =ResString("btnCancel")%>" onclick="selectMHT(false);" />
</div>
<div class='overlay' style="display:none"></div>
<table border="0" cellpadding="0" cellspacing="0" class="whole">
    <tr style="height:42px">
        <td valign="middle" id="tdCaption"><h5 style="margin:8px 12px 0px 12px"><a style="right: 9px; cursor: pointer; position: absolute;" class="ec-icon" onclick="window.open('<% =JS_SafeString(HelpAuthWebAPI.GetHelpPath(App.isRiskEnabled)) %>help/using-the-rich-text-editor', 'helpwin', 'menubar=no,maximize=no,titlebar=no,status=yes,location=yes,toolbar=yes,channelmode=no,scrollbars=yes,resizable=yes,width=950,height=600,left=50,top=30');"><i class="fa fa-question-circle" title="Using the Rich Text Editor"></i></a><asp:Label ID="lblCaption" runat="server" /></h5></td>
    </tr>
    <tr>
        <td valign="middle" align="center" id="tdEditor" style="width:100%; padding:6px;">
            <asp:TextBox ID="TinyMCEEditor" runat="server" TextMode="MultiLine" Rows="25" CssClass="tinymce" Width="100%"></asp:TextBox><div id="divLoaderMsg">Loading. Please wait...</div>
            <asp:HiddenField runat="server" ID="InfodocPath"/>
<%--            <asp:FileUpload ID="ImageUpload" runat="server" onChange="uploadImage()"/>
            <asp:Button ID="ImageUploadBtn" runat="server" Text="Upload" OnClick="ImageUploadBtn_Click" OnClientClick="showLoadingPanel('', false);" />--%>
            <asp:Label ID="lblError" CssClass="text error" Font-Bold="true" Text="<div style='text-align:center'>Object not found</div>" runat="server" Visible="false"/>
        </td>
    </tr>
    <tr runat="server" id='trQHOptions' visible="false" style="height:1ex;" valign="bottom" enableviewstate="false">
        <td class='text' id="tdQHOptions" style="padding:0px 6px;"></td>
    </tr>
    <tr style="height:1em">
        <td style="padding:6px;">
            <table border="0" cellpadding="0" cellspacing="0" width="100%" id="tblButtons">
                <tr valign="middle">
                    <td style="width:180px"><nobr><asp:CheckBox runat="server" ID="cbAutoSave" CssClass="text"/></nobr></td>
                    <td runat="server" id='tdHints' visible="false" enableviewstate="false" class='text' style="width:100px;"><nobr>[&nbsp;<span class='aslink' style='color:#003399;'><asp:Label runat="server" ID="lnkHints"/></span><div runat="server" id="dxToolTipHints" style="display: inline-block; text-align: left;"></div>&nbsp;]</nobr></td>
                    <td style="width:30%"><div id="divMsg" style="text-align:center" class="text gray" runat="server">&nbsp;</div></td>
                    <td align="right"><nobr>
                        <asp:Button ID="btnSave" runat="server" CssClass="button" UseSubmitBehavior="false" OnClientClick="return SaveChanges(true, '');" />
                        <asp:Button ID="btnApply" runat="server" CssClass="button" UseSubmitBehavior="false" OnClientClick="return SaveChanges(false, '');" />
                        <asp:Button ID="btnClose" runat="server" CssClass="button" UseSubmitBehavior="false" OnClientClick="return CloseWin(refresh_on_cancel);" />
                    </nobr></td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<script type="text/javascript">

    var tinymceID = "";

    var fonts = 'Andale Mono=andale mono,times;' + 'Arial=arial,helvetica,sans-serif;' + 'Arial Black=arial black,avant garde;' + 'Book Antiqua=book antiqua,palatino;' + 'Comic Sans MS=comic sans ms,sans-serif;' + 'Courier New=courier new,courier;' + 'Georgia=georgia,palatino;' + 'Helvetica=helvetica;' + 'Impact=impact,chicago;' + 'Symbol=symbol;' + 'Tahoma=tahoma,arial,helvetica,sans-serif;' + 'Terminal=terminal,monaco;' + 'Times New Roman=times new roman,times;' + 'Trebuchet MS=trebuchet ms,geneva;' + 'Verdana=verdana,geneva;' + 'Webdings=webdings;' + 'Wingdings=wingdings,zapf dingbats';
    var font_sizes = "6p 7pt 8pt 9pt 10pt 11pt 12pt 13pt 14pt 16pt 18pt 24pt 36pt";

    var need_close = 0;
    var is_autosave = 0;
    var autosave_timeout = 3*60*1000;
    var infodoc = <% =IIf(CheckVar("field", "").ToLower = "infodoc", 1, 0) %>;
    var callback = "<% =JS_SafeString(SafeFormString(CheckVar("callback", ""))) %>";
    var refresh_on_cancel = 0;
    var closing = 0;

    var cookie_name = "RE_AutoSave";
    var timeout = 1000 * 60 * 5;

    var is_dlg = false;
    var wnd = (window.opener || window.parent);

    function initEditor() {
        var path = theForm.<% =InfodocPath.ClientID %>.value;
        tinymce.init({
            selector: '.tinymce',
            //theme: 'Silver',
            font_formats: fonts,
            fontsize_formats: font_sizes,
            //plugins: 'print preview fullpage searchreplace autolink directionality visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists imagetools textpattern help',
            plugins: 'print preview fullpage searchreplace autolink directionality visualblocks visualchars fullscreen image link media table charmap hr pagebreak nonbreaking anchor insertdatetime advlist lists imagetools textpattern help',
            //toolbar1: 'formatselect fontselect fontsizeselect | bold italic strikethrough forecolor backcolor | link | alignleft aligncenter alignright alignjustify  | numlist bullist outdent indent  | removeformat | custombutton custombutton1 custombutton2',
            toolbar: [
                'undo redo | copy cut paste | formatselect fontselect fontsizeselect forecolor backcolor | removeformat | bold italic underline strikethrough subscript superscript | alignleft aligncenter alignright alignjustify | outdent indent | bullist numlist table link image media |custombutton1 custombutton2'
            ],
            paste_data_images: true,
            image_advtab: true,
            object_resizing: true,
            resize: false,
            auto_focus: '<% =TinyMCEEditor.ClientID %>',
            default_link_target: '_blank',
            link_context_toolbar: true,
            //plugin_preview_height: function () { 
            //    return  $("#tdEditor").prop("clientHeight")-24 
            //},
            content_css: [
                "../../App_Themes/ec2018/deco.css",
                "../../App_Themes/ec2018/main.css",
                "../../App_Themes/ec2018/fontawesome-all.min.css"
            ],
            setup: function (editor) {
                editor.on('init', function(args) {
                    var id = editor.id;
                    tinymceID = id;
                    $(window).trigger('resize');
                });
                <%--editor.ui.registry.addButton('custombutton', {
                    text: 'Upload Image',
                    icon: "image",
                    onAction: function () {
                        $("#" + "<%=ImageUpload.ClientID%>").click();
                    }
                }); --%> 
            
                editor.ui.registry.addButton('custombutton1', {
                    //text: 'Download',
                    tooltip: "Download",
                    icon: "save",
                    onAction: function () {
                        SaveChanges(false, "&download=1");;    
                    }
                });  
                editor.ui.registry.addButton('custombutton2', {
                    //text: 'Upload',
                    tooltip: "Upload",
                    icon: "upload",
                    onAction: function () {    
                        var l = 200;
                        var t = 100;
                        var p = $(".Upload").position();
                        if ((p)) { l = p.left-$("#divUpload").width() - 25; t = p.top + 27; }
                        if (l<12) l = 12;
                        $("#divUpload").css({left: l+'px', top: t+'px', position:'absolute'});
                        selectMHT(true);
                    }
                });
            },
            automatic_uploads: true,
            images_upload_url: '<% =_WEBAPI_ROOT %>pm/infodoc/?action=upload&path=' + encodeURIComponent(path),
            images_upload_base_path: path,
            file_picker_types: 'image', 
<%--            file_picker_callback: function(cb, value, meta) {
                if (meta.filetype == 'image') {
                    var input = document.createElement('input');
                    input.setAttribute('type', 'file');
                    input.setAttribute('accept', 'image/*');

                    /*
                      Note: In modern browsers input[type="file"] is functional without
                      even adding it to the DOM, but that might not be the case in some older
                      or quirky browsers like IE, so you might want to add it to the DOM
                      just in case, and visually hide it. And do not forget do remove it
                      once you do not need it anymore.
                    */

                    input.onchange = function () {
                        var file = this.files[0];

                        var reader = new FileReader();
                        reader.onload = function () {
                            /*
                              Note: Now we need to register the blob in TinyMCEs image blob
                              registry. In the next release this part hopefully won't be
                              necessary, as we are looking to handle it internally.
                            */
                            var id = 'blobid' + (new Date()).getTime();
                            var blobCache =  tinymce.activeEditor.editorUpload.blobCache;
                            var base64 = reader.result.split(',')[1];
                            var blobInfo = blobCache.create(id, file, base64);
                            blobCache.add(blobInfo);

                            /* call the callback and populate the Title field with the file name */
                            cb(blobInfo.blobUri(), { title: file.name });
                        };
                        reader.readAsDataURL(file);
                    };

                    input.click();
                } else {
                    $("#" + "<%=ImageUpload.ClientID%>").click(); //upload image when Insert->Image is click in the editor
                }
            }--%>
        });

        //tinymce.activeEditor.uploadImages(function(success) {
        //    $.post('ajax/post.php', tinymce.activeEditor.getContent()).done(function() {
        //        console.log("Uploaded images and posted content as an ajax request.");
        //    });
        //});

    }

    function CloseWin(refresh)
    {
        if (refresh && (wnd) && (typeof wnd.<% =CallbackFunction%> == "function")) {
            var content_html = tinymce.activeEditor.getContent();
            var content_text = tinymce.activeEditor.getBody().textContent.trim();
            var empty = (content_text == "" && content_html.toLowerCase().indexOf("<img")<0 && content_html.toLowerCase().indexOf("<object")<0 ? "1" : "0");   // D0861
            <% If _ObjectType=reObjectType.QuickHelp  Then%>callback = (content_html.toLowerCase().indexOf("youtu.be/")>0 || content_html.toLowerCase().indexOf("youtube.com/")>0);
            <% End If%>wnd.<% =CallbackFunction%>(empty, infodoc, [callback,'<% =JS_SafeString(GetInfodocURL()) %>']);
            wnd.focus(); // D0193
        }
        if (!closing) {
            if ((is_dlg) && (wnd) && typeof wnd.onCloseRichEditor == "function") {
                wnd.onCloseRichEditor(); 
            } else { 
                window.close();
            }
        }
        return false;
    }
    
    
    function RefreshTimer()
    {
        SwitchAutoSave();
    }
    
    function SaveChanges(do_close, extra)
    {
        ShowMessage("<% =JS_SafeString(ResString("msgSaving")) %>", 1);
        var text = tinymce.activeEditor.getContent();
        //window.status = text;
        theForm.<% =btnSave.ClientID %>.disabled = 1;
        if ((theForm.<% =btnApply.ClientID %>)) theForm.<% =btnApply.ClientID %>.disabled = 1;
        need_close = do_close;
        var autoshow = "";
        if ((theForm.cbQHAutoShow)) autoshow = "&autoshow=" + ((theForm.cbQHAutoShow.checked) ? 1 : 0);
        sendAJAX("<% =_PARAM_ACTION %>=<% =_ACTION_SAVE %>&title=" + encodeURIComponent($("#<% = lblCaption.ClientID %>").text()) + autoshow + "&text=" + encodeURIComponent(encodeURIComponent(text)) + extra);
        return false;
    }

    function useDefault() {
        if ((tinymce) && tinymce.activeEditor.getBody().textContent.trim() !="" ) {
            var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("confQHReset"))%>", resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (dialogResult) {
                    setTimeout('reset2Default();', 250);
                }
            });
        }
        else{
            reset2Default();
        }
    }

    function reset2Default() {
        var d = theForm.txtDefault;
        if((tinymce) && (d)){
            tinymce.activeEditor.setContent(d.value);
            SetFocusOnRadEditor();
        }
    }

    function sendAJAX(params) {
        _ajax_ok_code = function (data) { receiveAJAX(data); };
        _ajaxSend(params, "?<% = RemoveXssFromUrl(GetParamsButExclude(Request.QueryString, {_PARAM_ACTION, "ajax", "r", "type"})) %>&type=<% =CInt(_ObjectType) %>&ajax=yes&<% =_PARAM_ACTION %>=<% =_ACTION_SAVE%>&r=" + Math.random());
    }

    function receiveAJAX(data) {
        refresh_on_cancel = 1;
        if (data!=null && data.length<500) {
            if (data.substr(0,4).toLowerCase() == "http") document.location.href = data; else ShowMessage(data, 1);
        }
        theForm.<% =btnSave.ClientID %>.disabled = 0;
        if ((theForm.<% =btnApply.ClientID %>)) theForm.<% =btnApply.ClientID %>.disabled = 0;
        if ((need_close)) CloseWin(true);
    }

    function SwitchAutoSave() {
        var cb = theForm.<% =cbAutoSave.ClientID %>;
        if (cb)
        {
            is_autosave = cb.checked;
            //window.status = is_autosave;
            document.cookie = cookie_name + "=" +  (is_autosave ? "1" : "0");
            if (is_autosave) setTimeout("if (is_autosave) { SaveChanges(0, ''); SwitchAutoSave(); }", autosave_timeout);
        }
    }
    
    function SetFocusOnRadEditor() {
        if ((tinymce) && (tinymce.activeEditor)) {
            tinymce.activeEditor.focus(false);
        }
    }
   
    function onResize() {
        var e = $(".tox-tinymce");
        if ((e) && (e.length)) {
            e.hide();
            var w = $("#tdEditor").prop("clientWidth");
            var h = $("#tdEditor").prop("clientHeight");
            e.show();
            if (w>200) e.width(w - ((is_dlg) ? 24 : 24));
            if (h>100) e.height(h - ((is_dlg) ? 16 : 16));
        }
    }
   
    function InitContent(fld) {
        if(tinymce && (wnd) && (wnd.theForm)){
            var f = eval("wnd.theForm." + fld);
            if ((f) && f.value!="") tinymce.activeEditor.setContent(replaceString("<", "&lt;", replaceString(">", "&gt;", f.value)));
        }
    }
   
    function ShowMessage(text, timeout) {
        var msg = $get("<% = divMsg.ClientID %>");
        if (msg) msg.innerHTML = text; else window.status = "!";
        if (msg && timeout) setTimeout("ShowMessage('&nbsp;', 0);", 2000);
    }

    function InsertTemplate(txt) {
        SwitchTooltip_("<% =dxToolTipHints.ClientID %>");
        InsertText(txt);
        return false;
    }

    function InsertText(txt) {
        if (tinymce) {
            SetFocusOnRadEditor();
            tinymce.activeEditor.execCommand('mceInsertContent', false, txt);
        }
        return false;
    }

    function SwitchTooltip_(id)
    {
        var t = $("#" + id).dxTooltip("instance");
        if ((t)) {
            if (t.option("visible")) { t.hide(); } else { t.show(); }
        }
    }

    var dlg_clusters = null;
    function onApplyTo() {

        if ((dlg_clusters)) { dlg_clusters.dialog("open"); return true; }

        var cnt = $("#qh_apply_cnt").val()*1;
        if (cnt>3) $("#divSelNodes").show(); else $("#divSelNodes").hide();
        
        dlg_clusters = $("#divClusters").dialog({
            autoOpen: true,
            //              height: 500, //(lst.length>10 ? 400 : "auto"),
            minWidth: 400,
            maxWidth: 750,
            minHeight: 260,
            maxHeight: 450,
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: "<% = JS_SafeString(ResString("confQHReset"))%>",
            position: { my: "center", at: "center", of: $("body"), within: $("#frmMain") },
            buttons: {
                OK: {
                    id: "btnApplyTo",
                    text: "<% = JS_SafeString(ResString("titleConfirmation"))%>",
                    //disabled: true,
                    click: function() {
                        QHApplyTo();
                        if ((dlg_clusters)) dlg_clusters.dialog("close");
                    }
                },
                Cancel: function() {
                    if ((dlg_clusters)) dlg_clusters.dialog("close");
                }
            },
            open: function() {
                $("body").css("overflow", "hidden");
            },
            close: function() {
                $("body").css("overflow", "auto");
            },
            resize: function( event, ui ) { $("#divList").height(30).height(Math.round(ui.size.height - $("#divClustersTitle").height() - 168)); }
        });
    }

    function selNodes(s) {
        var cnt = $("#qh_apply_cnt").val()*1;
        for (i=0; i<cnt; i++) {
            var f = $("#qh_apply_" + i);
            if ((f) && (f.prop("disabled")=="")) {
                c = f.prop('checked');
                c = (s==1 || (s==-1 && !c));
                f.prop('checked', c);
            }
        }
    }

    function QHApplyTo() {
        var lst = "";
        var cnt = $("#qh_apply_cnt").val()*1;
        for (i=0; i<cnt; i++) {
            var f = $("#qh_apply_" + i);
            if ((f) && (f.prop('checked'))) lst += (lst=="" ? "":",") + f.val();
        }
        if (lst!="") lst = "&applyto=" + lst;
        SaveChanges(true, lst);
        return false;
    }

    <%--    function setQHAutoShow(val) {
        need_close = false;
        $.ajax({
            url: "<% =Request.Url.OriginalString %>&ajax=yes&<% =_PARAM_ACTION %>=autoshow&autoshow=" + ((val) ? "1" : "0") + "&r=" + Math.random(),
            type: "GET",
            dataType: "text",
            async:true,
            success: function (data) {
                receiveAJAX(data);
            },
            error: function( jqXHR, textStatus ) {
                dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", true, ";", "undefined", "<% = JS_SafeString(ResString("titleError")) %>", 350, 280);
            }
        });
    }--%>

    function selectMHT(vis) {
        if ((vis)) {
            $(".overlay").show();
            $("#divUpload").show();
            setTimeout('theForm.btnUpload.disabled=0; theForm.btnUpload.focus(); if (theForm.<% =FileUpload.ClientID %>.value=="") theForm.btnUpload.disabled=1;',150);
            $("#tblButtons").prop("disabled","disabled");
            $("#<% =tdHints.ClientID%>").hide();
        } else {
            $(".overlay").hide();
            $("#divUpload").hide();
            $("#<% =tdHints.ClientID%>").show();
            $("#tblButtons").prop("disabled","");
        }
    }

    function uploadMHT() {
        if (theForm.<% =FileUpload.ClientID %>.value!="") {
            theForm.btnUpload.disabled = 1;
            theForm.doUpload.value = "1";
            theForm.submit();
        }
    }

    function msgWrongFile() {
        DevExpress.ui.dialog.alert("<% =JS_SafeString(ResString("msgWrongMHTFile")) %>", resString("titleError"));
    }

<%--    function uploadImage(){
        $("#" +  "<%= ImageUploadBtn.clientID %>").click();
    }--%>

    function showMsg(msg) {
        DevExpress.ui.dialog.alert("<div style='max-width:700px;'>" + msg + "</div>", resString("titleInformation"));
    }

    //    function BeforeExit()
    //    {
    //        closing = 1;
    //        if ((refresh_on_cancel)) CloseWin(true);
    //    }

    //    window.onbeforeunload = BeforeExit;
    resize_custom = onResize;
    $(".tinymce").hide();
<%--    $("#<%=ImageUpload.ClientID%>").hide();
    $("#<%=ImageUploadBtn.ClientID%>").hide();--%>
    $(document).ready(function () {
        is_dlg = (!(window.opener) && (window.parent) && typeof window.parent.onCloseRichEditor == "function");
        wnd = (is_dlg ? window.parent : window.opener);
        $("#divLoaderMsg").hide();
        $(".tinymce").show();
        initEditor();
        $("#<%=dxToolTipHints.ClientID%>").dxTooltip({
            target: '#<%=lnkHints.ClientID%>',
            showEvent: 'dxhoverstart',
            hideEvent: 'dxclick',
            hoverStateEnabled: true
        });
        onResize();
        RefreshTimer();
        window.focus();
        hideLoadingPanel();
        setTimeout('SetFocusOnRadEditor();', 1500);
<%--        var msg = "<% = JS_SafeString(MessageOnLoad) %>";
        if ((msg!="")) showMsg(msg);--%>
    });

</script>
</asp:Content>