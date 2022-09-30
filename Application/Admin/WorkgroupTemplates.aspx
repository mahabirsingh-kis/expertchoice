<%@ Page Language="VB" Inherits="WorkgroupTemplatesPages" title="Workgroup Templates" Codebehind="WorkgroupTemplates.aspx.vb" %>
<%@ Register TagPrefix="EC" TagName="ModalProcess" Src="~/ctrlModalProcess.ascx" %>
<%@ Register Assembly="DevExpress.Web.v9.1"  Namespace="DevExpress.Web.ASPxTabControl" TagPrefix="dxtc" %>
<%@ Register Assembly="DevExpress.Web.v9.1"  Namespace="DevExpress.Web.ASPxClasses" tagprefix="dxw" %>
<%@ Register Assembly="DevExpress.Web.v9.1" Namespace="DevExpress.Web.ASPxCallback" TagPrefix="dxcb" %>
<%@ Register Assembly="DevExpress.Web.v9.1" Namespace="DevExpress.Web.ASPxRoundPanel" TagPrefix="dxrp" %>
<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">

    var cur_prg = 0;
    var cur_step = 0;
    var cur_tab = -1;
    var cur_names = [];
    var total_steps = 0;
    var files = null;
    var names = null;
    var all_msg = "";
    var is_done = 0;

    var keep_msg = 0;
    var auto_close = 0;
    
    var cancel_btn = "btnCancel";
    
    var isDebug = 0;

    var startup_prjs = [<% =StartupProjects %>];
    
    modal_prg_width = 300;
    
    function GetSelection()
    {
        var lst = [];

        var tab =GetActiveTab();

        if (tab === 0)
        {
            for (var i=0; i<<%=FilesList.Count %>; i++)
            {
                var o = eval("theForm.tpl" + i);
                if ((o) && (o.checked)) lst.push(o.value);
            }
        }

        if (tab === 1)
        {
            names = [];
            for (var i=0; i<startup_prjs.length; i++)
            {
                var o = eval("theForm.prj" + i);
                if ((o) && (o.checked))
                {
                    lst.psuh(o.value);
                    names.push(startup_prjs[i]);
                }
            }
        }

        return lst;
    }

    function DoCopy()
    {
        var b = theForm.<% =btnOK.ClientID %>;
        if ((b) && !b.disabled) b.click();
        return false;
    }

    function GetActiveTab()
    {
        if ((PageControl))
        {
            return PageControl.GetActiveTab().index;
        }
        return -1;
    }

    function InitButton()
    {
        if ((theForm.<% =btnOK.ClientID %>))
        {
            theForm.<% =btnOK.ClientID %>.disabled = (!GetSelection().length);
            var l = $get("lnkDo");
            if ((l)) l.style.color = (theForm.<% =btnOK.ClientID %>.disabled ? "#cccccc" : "#6666cc");
        }
    }
    
    function StartProcess()
    {
        files = GetSelection();
        total_steps = files.length;
        if (!files.length) { files = null; return false }

        cur_tab = GetActiveTab();
        cur_names = (cur_tab === 1 ? names : files);
        cur_prg = 0;
        cur_step = 0;
        all_msg = "";
        is_done = 0;
        ShowStatus('<% =JS_SafeString(resString("msgWTSavingData")) %>', cur_prg);
        ModalSwitchMode('progressdiv', 1, cancel_btn, (cancel_btn === "" ? "" :  "StopProcess()")); 
        InitCancelButton(1);
        DoStep();
        return false;
    }
    
    function InitCancelButton(is_cancel)
    {
        if (!is_cancel) is_done = 1;
        if (cancel_btn === "") return;
        var btn = eval("theForm." + cancel_btn);
        if ((btn))  btn.value = (is_cancel ? '<% =JS_SafeString(ResString("btnCancel")) %>' : '<% =JS_SafeString(ResString("btnOK")) %>');
    }
    
    function StopProcess()
    {
        ModalSwitchMode('progressdiv', 0, "", ""); 
        cur_step = 0;
        cur_prg = 0;
        files = null;
        if (is_done) 
        {
            <% If isSLTheme Then %>if ((window.parent) && (typeof (window.parent.ReloadProjectsList) == "function")) window.parent.ReloadProjectsList();<% End if %>
            document.location.href = document.location.href;
        }
        is_done = 0;
        return false;
    }
    
    function DoStep()
    {
        if (cur_step<total_steps && (files!=null))
        {
            if (isDebug) window.status = "send data...";
            code = "?step=" + cur_step + "&mode=" + cur_tab + "&cmd=" + replaceString(".","%252E", replaceString("&","%2526",encodeURIComponent(files[cur_step])));
            ASPxCallbackPasscode.SendCallback(code);
        }
        else
        {
            InitCancelButton(0);
            if (auto_close) StopProcess();
        }
    }    
    
    function ProcessFeedback(msg)
    {
        if (files==null) return;
        if (isDebug) window.status = msg + cur_prg;
        var data = eval(msg);
        if (data[0]>cur_step)
        {
            cur_step = data[0];
            cur_prg = cur_step/total_steps;
            ShowStatus(data[1], cur_prg);
             
            if (cur_step<total_steps)
            {
                setTimeout("DoStep();", 200);
            }
            else
            { 
                cur_prg = 1; 
                InitCancelButton(0); 
                if (auto_close) setTimeout("StopProcess();", 250); 
            }
        }
    }
    
    function ShowStatus(msg, perc)
    {
        all_msg = (keep_msg ? all_msg : "") + "<div>" + msg + "</div>";
        ModalUpdateControl("msg", "<div style='margin-bottom:1ex'>" + (cur_step < total_steps  ? "<img src='<% =ImagePath %>devex_loading.gif' width=16 height=16 border=0 align=left style='margin-right:1ex'>" : "") + "<b>File " + cur_step + "/" + total_steps + ": " + cur_names[(cur_step >0 ? cur_step-1 : cur_step)] + "</b></div>" + ModalGetProgressBar(perc, 1) + all_msg, 0);
    }
    
    function SelectAll(sel)
    {
        var tab = GetActiveTab();

        if (tab === 0)
        {
            for (var i=0; i<<%=FilesList.Count %>; i++)
            {
                var o = eval("theForm.tpl" + i);
                if ((o)) o.checked = sel;
            }
        }

        if (tab === 1)
        {
            for (var i=0; i<startup_prjs.length; i++)
            {
                var o = eval("theForm.prj" + i);
                if ((o)) o.checked = sel;
            }
        }

        InitButton();
        return false;
    }


    function onResize()
    {
        var d = $("#tblTable").parent().parent().parent();
        if ((d))
        {
            $("#divFiles").height(10);
            $("#divStartup").height(10);
            $("#divExists").height(10);
            var h = d.height() - 200;
            if (h<100) h = 100;
            $("#divFiles").height(h);
            $("#divStartup").height(h);
            $("#divExists").height(h);
        }
    }

    function InitForm() {
        InitButton();
        var t = $("#PageContent_ASPxPageControlSources_RAC");
        if ((t) && (t.length))  {
            $("#divSelect").detach().appendTo(t);
        }
    }

    resize_custom = onResize;
  
</script>
<EC:ModalProcess id="MainModal" runat="server" Visible="true"/>
<div style="display:none;" class="modalProgress" id="progressdiv"><table border="0" class="whole"><tr><td valign="middle" align="center" style="padding:150px 0px 0px 2em">
<dxrp:ASPxRoundPanel ID="ASPxRoundPanelLoading" runat="server" SkinID="RoundPanel" ShowHeader="true" Width="390px" HeaderText="Please wait...">
<PanelCollection>
    <dxrp:PanelContent runat="server">
        <div style="margin:1em" id="msg" class="text small">&nbsp;</div>
        <div style="text-align:center; margin-bottom:1em"><input type="button" name="btnCancel" class="button" onclick="StopProcess();"/></div>
    </dxrp:PanelContent>
</PanelCollection>
</dxrp:ASPxRoundPanel></td></tr></table></div>
<dxcb:ASPxCallback ID="ASPxCallbackPasscode" runat="server" ClientInstanceName="ASPxCallbackPasscode">
    <ClientSideEvents CallbackComplete="function(s, e) {
     ProcessFeedback(e.result);
}" />
</dxcb:ASPxCallback>

<table border='0' style='height:100%' cellspacing='0' cellpadding='0' id="tblTable">
<tr style='height:1em'>
    <td colspan="3"><h4 style='padding-bottom:8px;'><% =String.Format(ResString(CStr(IIf(isTemplates, "lblWorkgroupTemplates", "lblWorkgroupSamples"))), CurrentWorkgroup.Name)%></h4></td>
</tr>

<tr>
<td align='right'><div style='position:absolute; width:200px; margin-top:6px; margin-left:200px;' class='text'><div id="divSelect" style="text-align:right;"><nobr><a href='' onclick='SelectAll(1); return false;' class='dashed'><% =ResString("lblSelectAll") %></a> | <a href='' onclick='SelectAll(0); return false;' class='dashed'><% =ResString("lblSelectNone")%></a></nobr></div></div>
<dxtc:ASPxPageControl ID="ASPxPageControlSources" runat="server" ActiveTabIndex="0" Width="430px" Height="100%" SkinID="ASPxPageControl" SaveStateToCookies="true" EnableClientSideAPI="true" ClientInstanceName="PageControl" ClientSideEvents-ActiveTabChanged="InitButton">
    <TabPages>
      <dxtc:TabPage>
        <ContentCollection>
            <dxw:ContentControl ID="ContentControlFiles" runat="server">
                <div id="divFiles" class='text' style='height:150px; overflow:auto;'><nobr><asp:Label runat="server" ID="lblFilesList"/></nobr></div>
            </dxw:ContentControl>
        </ContentCollection>
      </dxtc:TabPage>
      <dxtc:TabPage>
        <ContentCollection>
            <dxw:ContentControl ID="ContentControlStartup" runat="server">
                <div id="divStartup" class='text' style='height:150px; overflow:auto;'><nobr><asp:Label runat="server" ID="lblStartup"/></nobr></div>
            </dxw:ContentControl>
        </ContentCollection>
      </dxtc:TabPage>
    </TabPages>
</dxtc:ASPxPageControl>
</td>    

<td style="width:30px; font-size:32pt; padding:0px 8px; color:#999999;" valign="middle" align="center"><a href='' class='nu' onclick='DoCopy(); return false;' id='lnkDo'>&raquo;</a></td>

<td>
<dxtc:ASPxPageControl ID="ASPxPageControlDest" runat="server" ActiveTabIndex="0" Width="430px" Height="100%" SkinID="ASPxPageControl">
    <TabPages>
      <dxtc:TabPage>
        <ContentCollection>
            <dxw:ContentControl ID="ContentControlExists" runat="server">
                <div id="divExists" style='height:150px; overflow:auto;'><asp:Label runat="server" ID="lblExisted" CssClass="text"/></div>
            </dxw:ContentControl>
        </ContentCollection>
      </dxtc:TabPage>
    </TabPages>
</dxtc:ASPxPageControl>
</td>    
</tr>

<tr style='height:1em'>
    <td colspan="3" align='right'><div style='margin-top:1ex;'><asp:Button ID="btnOK" runat="server" CssClass="button" UseSubmitBehavior="false" OnClientClick="StartProcess(); return false;" Width="8em"/> <asp:Button ID="btnCancel" runat="server" CssClass="button" OnClientClick="history.go(-1); return false;" UseSubmitBehavior="false" Width="8em"/></div></td>
</tr>
</table>

</asp:Content>