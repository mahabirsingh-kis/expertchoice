<%@ Page Language="VB" Inherits="DataGridPage" title="DataGrid" Codebehind="DataGrid.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/ec.dg.js"></script>
<script language="javascript" type="text/javascript">

    var hiddenAttributes = '<%=HiddenColumns(App.ActiveProject)%>';
    var attr_list = <%= GetAttributes() %>;
    var mappings_list = <%= GetDataMappings() %>; //wed1
    var btnImport = null;
    var btnMap = null;
    var toolbar = null;
    var groupsUsersList = <%=getGroupsUsersList() %>;
    var selectedUserID = <% =SelectedUserID %>;
    var isRisk = <%=Bool2JS(PM.IsRiskProject) %>;
<%--    function onGetReport(addReportItem) {
        if (typeof addReportItem == "function") {
            addReportItem({"name": "<% =JS_SafeString(ResString("titleReportDataGrid")) %>", "type": "<% =JS_SafeString(ecReportItemType.DataGrid.ToString) %>", "edit": document.location.href, "portrait": false});
        }
    }--%>

    function syncReceived(params) {
        hideLoadingPanel();
        if ((params)) {            
            var received_data = eval(params);
            if ((received_data)) {
                if (received_data[0] == 'rows'){
                    $('#dataGridTable').datagrid('option', 'rows', received_data[1]);
                    $('#dataGridTable').datagrid('redraw');
                };
            };
        }        
    }

    function sendCommand(params) {
        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    function syncError() {
        hideLoadingPanel();
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
        $(".ui-dialog").css("z-index", 9999);
    }

    function onUpload() {
        showLoadingPanel("Done. Please wait...");
        setTimeout(function () {
            document.location.href='<% =JS_SafeString(PageURL(CurrentPageID, "id=")) %>' + selectedUserID;
        }, 30);
    }

    function onSelectColumnsClick() {
        initSelectColumnsForm("Select Columns");
        dlg_select_columns.dialog("open");
        dxDialogBtnDisable(true, true);
    }

    function initSelectColumnsForm(_title) {
        cancelled = false;

        var labels = "";

        // generate list of attributes
        var attr_list_len = attr_list.length;
        for (var k = 0; k < attr_list_len; k++) {
            var checked = hiddenAttributes.indexOf(attr_list[k].guid) < 0;
            labels += "<label><input type='checkbox' class='select_clmn_cb' value='" + attr_list[k].guid + "' " + (checked ? " checked='checked' " : " " ) + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + attr_list[k].name + "</label><br>";
        }    

        $("#divSelectColumns").html(labels);

        dlg_select_columns = $("#selectColumnsForm").dialog({
            autoOpen: false,
            modal: true,
            width: 420,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: _title,
            position:  { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                Ok: { id: 'jDialog_btnOK', text: "OK", click: function() {
                    dlg_select_columns.dialog( "close" );
                }},
                Cancel: function() {
                    cancelled = true;
                    dlg_select_columns.dialog( "close" );
                }
            },
            open: function() {
                $("body").css("overflow", "hidden");
            },
            close: function() {
                $("body").css("overflow", "auto");                
                if (!cancelled) {
                    var clmn_ids = "";
                    var cb_arr = $("input:checkbox.select_clmn_cb");
                    $.each(cb_arr, function(index, val) { var cid = val.value + ""; if (!val.checked) { clmn_ids += (clmn_ids == "" ? "" : ",") + cid; } });
                    hiddenAttributes = clmn_ids;
                    _ajaxSend('action=select_columns&column_ids=' + clmn_ids); // save the selected columns via ajax
                    options.hiddenAttributes = hiddenAttributes;  // update the visible columns option                        
                    $('#dataGridTable').datagrid('option', 'hiddenAttributes', hiddenAttributes);    // update the datagrid option
                    $('#dataGridTable').datagrid('redraw'); // redraw datagrid
                }
            }
        });
        $(".ui-dialog").css("z-index", 9999);
    }

    function filterColumnsCustomSelect(chk) {
        $("input:checkbox.select_clmn_cb").prop('checked', chk*1 == 1);
        dxDialogBtnDisable(true, false);
    }
    function UploadDataGrid() {
        //CreatePopup('<% =PageURL(_PGID_REPORT_DATAGRID_UPLOAD, "id=") %>' + selectedUserID, 'UploadDataGrid', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=600,height=200', true);
        var w = dlgMaxWidth(550);
        var h = dlgMaxHeight(260);
        onShowIFrame('<% =PageURL(_PGID_REPORT_DATAGRID_UPLOAD, "btnUpload=0&id=") %>' + selectedUserID, resString("lblUploadFile"), w, h, {}, [{
            widget: 'dxButton',
            options: {
                elementAttr: { "id": "btnDoUpload", "class": "button_enter" },
                disabled: true,
                text: resString("btnUpload"),
                onClick: function (e) {
                    var frm = document.getElementById("frmInnerFrame");
                    if ((frm) && typeof frm.contentWindow.doUpload == "function") {
                        frm.contentWindow.doUpload();
                    }
                    return false;
                }
            },
            toolbar: "bottom",
            location: 'after'
        }, {
            widget: 'dxButton',
            options: {
                //icon: "far fa-times-circle", 
                text: resString("btnClose"),
                elementAttr: { "class": "button_esc" },
                onClick: function () {
                    closePopup();
                    return false;
                }
            },
            toolbar: "bottom",
            location: 'after'
        }]);
        return false;
    }

    function CheckUpload(dis)
    {
        var btn = $("#btnDoUpload");
        if ((btn) && (btn.data("dxButton"))) {
            btn.dxButton({disabled: dis});
        }
        if (!dis) hideLoadingPanel();
    }

    function OnClientButtonClicking(sender, args) 
    {
        var comandName = args.get_item().get_commandName();
        if (comandName == "Download") {
//            args.get_item().disable();
        };
        if (comandName == "Upload") {
            UploadDataGrid();
        }; 
    }  

    function onSelectGroup(val) {
        showLoadingPanel();
        theForm.submit();
    }

    function BtnFlash(tb_name, flash) {
        var tb = $find(tb_name);
        if ((tb)) {
            var btn = tb.get_items().getItem(0);
            if ((btn)) {
                var d = btn.get_middleWrapElement();
                d.style.background = (flash ? "#fff000" : "");
                if (flash) setTimeout("BtnFlash('" + tb_name + "', 0);", 250); 
            }
        }
    }

    var options = {
        rows: <%= GetRows() %>,
        columns: <%= GetColumns() %>,
        rowsTitle: "<%=ParseString("%%Alternatives%%")%>",
        viewMode: 'datagrid',  
        hiddenAttributes: hiddenAttributes,
        attributes: attr_list,          
        indexColumn: '<%=IDColumnMode%>', 
        nodeDataMap: <%= GetDataMapping() %>,
        datamappings: mappings_list,
        imgPath: '<%=ImagePath%>',
        contextMenu: true,
        use_datamapping: <% =Bool2JS(App.Options.ProjectUseDataMapping) %>,
        hid: <%=IIf(App.isRiskEnabled, CInt(PM.ActiveHierarchy), -1)%>,
        dgColAltDMGuid: '<%=GetAltsDMGUID()%>',
        hasMapKey: <%=HasMapKey()%>
        }

    // Normalization Modes
    var ntNormalizedForAll = <%=LocalNormalizationType.ntNormalizedForAll %>;
    var ntNormalizedMul100 = <%=LocalNormalizationType.ntNormalizedMul100 %>;
    var ntNormalizedSum100 = <%=LocalNormalizationType.ntNormalizedSum100 %>;
    var ntUnnormalized     = <%=LocalNormalizationType.ntUnnormalized     %>;
    var activeNormMode     = <%=PM.Parameters.Normalization               %>;

    var smIdeal = <%=ECSynthesisMode.smIdeal %>;
    var smDistributive = <%=ECSynthesisMode.smDistributive %>;
    var activeSynthesisMode = <%=PM.PipeParameters.SynthesisMode %>;

    function setPipeOption(is_pipeparams, name, value, on_saved) {
        callAPI("pm/params/?action=" + (is_pipeparams ? "set_pipe_option" : "set_param_option"), {"name": name, "value": value}, on_saved, typeof on_saved != "function");
    }

    $(document).ready(function () {
        if (selectedUserID < 0 && (groupsUsersList.length)) selectedUserID = groupsUsersList[0].id;
        initToolbar();
        resize_custom = function (force_redraw) { resize() };
        resetDivContent()
        options.height = $("#divContent").outerHeight()  - 4;
        options.width = $("#divContent").outerWidth() - 4; 
        $("#dataGridTable").datagrid(options);
        setTimeout("resize(); var tb=document.getElementById('input_Search'); if ((tb)) tb.focus();", 1000);
        if ($("#btnUploadDataGrid").data("dxButton")) $("#btnUploadDataGrid").dxButton("instance").option("disabled", (selectedUserID < 0));
    });

    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: [{
                location: 'before',
                template: function() {
                    return $("<div><%=ResString("lblDataGridUser")%></div>");
                }
            }, {
                location: 'before',
                widget: 'dxSelectBox',
                locateInMenu: 'auto',
                options: {
                    acceptCustomValue: false,
                    width: 140,
                    items: groupsUsersList,
                    valueExpr: "id",
                    displayExpr: "text",
                    value: selectedUserID,
                    onOpened: function (e) {
                        $(e.element).find(".dx-texteditor-input").select();
                    },
                    onValueChanged: function(e) {
                        var val = e.value;
                        if (typeof val == "undefined") {
                            e.component.option("value", e.previousValue);
                        } else {
                            if (selectedUserID !== val) {
                                selectedUserID = val;
                                showLoadingPanel();
                                $("#btnUploadDataGrid").dxButton("instance").option("disabled", (val < 0));
                                sendCommand("action=userchanged&id=" + val + "&sm=" + activeSynthesisMode + "&nm=" + activeNormMode);
                                uid=val; //'AS/21354c
                                is_groupuser=val<0 //'AS/21354c
                            }
                        }
                    },
                    searchEnabled: true
                }
            }, 
            {
                location: 'before',
                widget: 'dxSelectBox',
                locateInMenu: 'auto',
                visible: !isRisk,
                options: {
                    acceptCustomValue: false,
                    width: 140,
                    items: [{'id': ntUnnormalized, 'text': 'Unnormalized', disabled: (activeSynthesisMode == smDistributive)},
                            {'id': ntNormalizedForAll, 'text': 'Normalized'}
                    ],
                    valueExpr: "id",
                    displayExpr: "text",
                    value: (activeNormMode == ntUnnormalized ? activeNormMode : ntNormalizedForAll),
                    onValueChanged: function(e) {
                        activeNormMode = e.value;
                        setPipeOption(false, "Normalization", activeNormMode, function() {
                            sendCommand("action=userchanged&id=" + selectedUserID + "&sm=" + activeSynthesisMode + "&nm=" + activeNormMode);
                        });
                    }
                }
            },
            {
                location: 'before',
                widget: 'dxSelectBox',
                locateInMenu: 'auto',
                visible: !isRisk,
                options: {
                    acceptCustomValue: false,
                    width: 140,
                    items: [{'id': smIdeal, 'text': 'Ideal mode'},
                            {'id': smDistributive, 'text': 'Distributive mode'}
                    ],
                    valueExpr: "id",
                    displayExpr: "text",
                    value: activeSynthesisMode,
                    onValueChanged: function(e) {
                        activeSynthesisMode = e.value;
                        setPipeOption(true, 'SynthesisMode', activeSynthesisMode, function() {
                            if (activeSynthesisMode == smDistributive) {
                                activeNormMode = ntNormalizedForAll;
                            };
                            sendCommand("action=userchanged&id=" + selectedUserID +"&sm=" + activeSynthesisMode + "&nm=" + activeNormMode);
                            initToolbar();
                        });
                    }
                }
            },
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButton',
                options: {
                    text: resString("btnDownload"),
                    icon: '',
                    visible: true,
                    elementAttr: { id: 'btnDownloadDataGrid' },
                    onClick: function () {
                        document.location.href = "<% =JS_SafeString(PageURL(CurrentPageID, _PARAM_ACTION + "=download")) %>&id=" + selectedUserID;   // D4745
                        //sendCommand("action=download");
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButton',
                options: {
                    disabled: (selectedUserID<=0),
                    visible: <%= Bool2JS(CurrentPageID <>_PGID_REPORT_DATAGRID_NOUPLOAD) %>,
                    text: resString("btnUpload"),
                    icon: '',
                    elementAttr: { id: 'btnUploadDataGrid' },
                    onClick: function () {
                            UploadDataGrid()
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButton',
                options: {
                    text: "Select Columns",
                    icon: '',
                    elementAttr: { id: 'btnSelecColumns' },
                    onClick: function () {
                        onSelectColumnsClick();  
                    }
                }
            
            }]
        });

        toolbar = $("#toolbar").dxToolbar("instance");
    }

    function resetDivContent (){
        $("#divContent").width(200).height(100).width($("#tdContent").width()-4).height($("#tdContent").innerHeight()-40);
    }

    function resize() {
        resetDivContent();
        var w = $("#divContent").width();
        var h = $("#divContent").height();
        $('#dataGridTable').datagrid('resize', w-4, h-4);
        $('#dataGridTable').datagrid('redraw');
    }

<% If App.Options.ProjectUseDataMapping Then %>
    var frm_datamapping = null;
    var attr_active = null;

    function onFrameLoaded(frm)
    {
        //HidePleaseWait();
        if ((frm)) {
            frm_datamapping = frm.contentWindow;
            //if ((frm.style)) { frm.style.backgroundImage='none'; };
            //frm.focus();
            //frm_datamapping.focus();
            //if ((frm_datamapping.window)) frm_datamapping.window.focus();
        }
    }

    function DataGridReadOnly(readonly) {
        $('#dataGridTable').datagrid('setreadonly', (readonly));
    }

        var uid = <%=getSelectedUserID() %>; //'AS/21354c
        var is_groupuser= <%=IsGroupUser() %>; //'AS/21354c

    function showDMOptions(attr) {
        var url = "<% = PageURL(_PGID_REPORT_DATA_MAPPING, "temptheme=sl") %>";
        var is_empty = guid_equal(attr.dm, guid_empty);
        var mode = "attribute";
        //var uid = undefined //'AS/21354c
        //var is_groupuser=false  //'AS/21354c
        if (guid_equal(attr.guid, "<% = JS_SafeString(ATTRIBUTE_COST_ID.ToString)%>")) mode = "cost";
        if (guid_equal(attr.guid, "<% = JS_SafeString(ATTRIBUTE_RISK_ID.ToString) %>")) mode = "risk";
        if (guid_equal(attr.guid, "<% = JS_SafeString(clsProjectDataProvider.dgColAltName.ToString) %>")){ 
            mode = "alts";
            attr.name = "<% =JS_SafeString(ResString("lblDataGridAlternatives")) %>";
        }
        if (typeof (attr.id) != "undefined") {
            mode = "obj";
            attr.name = attr.title;
            attr.guid = attr.id;
        }
        <% If App.ActiveProject.HierarchyAlternatives.Nodes.Count=0 Then %>if (mode!="alts") {
            dxDialog("<% = JS_SafeString(ResString("msgDMNoAlts")) %>", false, ";");
            return false;
        }<% End if %>
        ping_active = false;
        _ajax_show_error = false;
        jDialog_show_icon = false;
        attr_active = attr;
        //document.getElementById('frmImport').src = url +"&mode=" + mode + "&" + (mode=="obj" ? "oguid" : "aguid") + "=" + attr.guid + "&dm=" + attr.dm + "&r=" + Math.round(Math.random()*100000); //'AS/16036
        //document.getElementById('frmImport').src = url +"&mode=" + mode + "&" + (mode=="obj" ? "oguid" : "aguid") + "=" + attr.guid + "&usrid=" +  uid  + "&dm=" + attr.dm + "&r=" + Math.round(Math.random()*100000); //'AS/16036 //'AS/16038
        document.getElementById('frmImport').src = url +"&mode=" + mode + "&" + (mode=="obj" ? "oguid" : "aguid") + "=" + attr.guid + "&usrid=" +  uid + "&isgroup=" + is_groupuser + "&dm=" + attr.dm + "&r=" + Math.round(Math.random()*100000); //'AS/16036 //'AS/16038

        //$("#frmImport").prop("src", url +"&mode=" + mode + "&aguid=" + attr.guid + "&dm=" + attr.dm + "&r=" + Math.round(Math.random()*100000));
        //var sData = "<iframe frameborder='0' src='" + url +"&mode=" + mode + "&aguid=" + attr.guid + "&dm=" + attr.dm + "&r=" + Math.round(Math.random()*100000) + "' class='frm_loading' onload='onFrameLoaded(this);' name='frmImport' id='frmImport' style='width:100%; height:99%'></iframe>";
        //var sData = "<iframe frameborder='0' src='<% =_URL_ROOT %>dummy.htm' class='frm_loading' onload='onFrameLoaded(this);' name='frmImport' id='frmImport' style='width:100%; height:99%'></iframe>";
        var h = $("#tdContent").height()-24;
        var w = $("#tdContent").width()-120;
        if (h<200) h = "undefined";
        if (w>650) w = 650;
        if (w<200) w = "undefined";
        DataGridReadOnly(true);
        //$("#divDataMapping").html(sData);

            $("#divDataMapping").dialog({
                autoOpen: true,
                modal: true,
                width: 550,
                height: h,
                dialogClass: "no-close",
                closeOnEscape: false,
                bgiframe: true,
                title: (is_empty ? "<% =JS_SafeString(ResString("titleDataMappingCreate"))%>" : "<% =JS_SafeString(ResString("titleDataMappingEdit"))%>") + (mode == "obj" ? " <% = JS_SafeString(ParseString(_TEMPL_OBJECTIVE)) %>" : "") +  " \'" + attr.name + "\'",
                position: { my: "center", at: "center", of: $("body"), within: $("body") },
                buttons: [
                    {
                        text: "<% =JS_SafeString(ResString("btnImport")) %>",
                        click: function() { onImportData(true); },
                        disabled: "disabled",
                        id: "btnImport",
                        width: "65px",
                        title: "Map and import/export data." //'AS/16301b
                    }, {
                        text: "<% =JS_SafeString(ResString("btnMap")) %>",
                        //click: function() { onImportData(false); }, //'AS/15285i
                        click: function() { onMapData(); }, //'AS/15285i
                        disabled: "disabled",
                        // visible: false, //'AS/15285i
                        id: "btnMap",
                        width: "65px",
                        title: "Map current column to the selected field in the external database" //'AS/16301b
                    }, {
                        text: "<% =JS_SafeString(ResString("btnDelete")) %>",
                        click: function() { onDMDeleteConfirm(); },
                        //disabled: (is_empty ? "disabled" : ""),
                        id: "btnDelete",
                        width: "65px",
                        title: "Delete mapping for the current column." //'AS/16301b
                    }, {
                        text: "<% =JS_SafeString(ResString("btnCancel")) %>",
                        click: function() { onHideDMOptions(false); },
                        id: "btnCancel",
                        width: "65px"
                    }],
                open: function() {
                    $("body").css("overflow", "hidden");
                },
                close: function() {
                    $("body").css("overflow", "auto");
                    //document.getElementById('frmImport').src = "<% =_URL_ROOT %>dummy.htm";
                    //$("#frmImport").prop("src", "<% =_URL_ROOT %>dummy.htm");
                }
            });
            $(".ui-dialog").css("z-index", 9999);
            btnImport = $("#btnImport");
            //btnImport.on("mouseover", function () { this.focus(); });
            btnMap = $("#btnMap");
            //btnMap.on("mouseover", function () { this.focus(); });
            //btnMap.hide(); //'AS/15285i
            if (is_empty) $("#btnDelete").hide();
            $("#btnCancel").focus();
            //ShowPleaseWait();
        }

    function onHideDMOptions(isOK) {
        //if ((frm_datamapping) && (typeof(frm_datamapping.onImport)=="function")) {//'AS/16024=== 'AS/24400=== removed
        //    //ShowPleaseWait();
        //    frm_datamapping.onCancel();
        //}//'AS/16024== 'AS/24400==
        btnImport = null; 
        btnMap = null; 
        DataGridReadOnly(false); 
        //HidePleaseWait(); //'AS/16216d-
        frm_datamapping = null;
        attr_active = null;
        $("#divDataMapping").dialog("close");
        ping_active = true;
        _ajax_show_error = true;
    }

    function onImportData(do_import) {
        if ((frm_datamapping) && (typeof(frm_datamapping.onImport)=="function")) {
            showLoadingPanel();
            frm_datamapping.onImport(do_import);
        }
    }

    //'AS/15285i===
    function onMapData() {
        if ((frm_datamapping) && (typeof(frm_datamapping.onImport)=="function")) {
            showLoadingPanel();
            frm_datamapping.onMapping();
        }
    }
    //'AS/15285i==

    function onImportDataCallback(res) {
        refreshSL();
        var attr = attr_active;
        //HidePleaseWait(); //'AS/16216d-
        if ((res) && (res.length) && (res[0]))
        {
            onHideDMOptions(true);
            if (guid_equal(attr.guid, "<% = JS_SafeString(clsProjectDataProvider.dgColAltName.ToString) %>")){
                $('#dataGridTable').datagrid('option', 'dgColAltDMGuid', res[3]);
            }else{
                $('#dataGridTable').datagrid('setAttrDM', attr.guid, res[3]);
            };
            //document.location.reload(); //'AS/16275
            document.location.href=document.location.href; //'AS/16275
        }
    }

    function refreshSL() {
        if ((window.opener) && ((typeof window.opener.onUpdateSLData)!="undefined")) { window.opener.onUpdateSLData("project"); }
        if ((window.parent) && ((typeof window.parent.onUpdateSLData)!="undefined")) { window.parent.onUpdateSLData("project"); }
    }

    var jDialogDelConf = null;
    function onDMDeleteConfirm() {
        if ((attr_active!="")) {
            var btns = {}
            btns.Ok = {id: 'jDialog_btnYes', text: "<% = JS_SafeString(ResString("btnYes")) %>", click: function () { if (jDialogDelConf!=null) jDialogDelConf.dialog("close"); onDMDelete(attr_active.guid); }, width: "60px"};
            btns.cancel = { id: 'jDialog_btnNo', text: "<% = JS_SafeString(ResString("btnNo")) %>", click: function () { if (jDialogDelConf!=null) jDialogDelConf.dialog("close"); }, width: "60px"} //'AS/16301a
            jDialogDelConf =  $("#divDialogConf").dialog({
                modal: true,
                autoOpen: true,
                dialogClass: "no-close",
                closeOnEscape: true,
                width:"auto",
                bgiframe: true,
                zIndex: 300,
                //title: "<% = JS_SafeString(ResString("btnNo")) %>", //'AS/16301a
                title: "<% = JS_SafeString(ResString("titleConfirmation")) %>", //'AS/16301a
                position: { my: "center", at: "center", of: $("body"), within: $("body") },
                buttons: btns,
                close: function () { jDialogDelConf = null; }
            });
        }
    }

    function onDMDelete(guid) {
        if ((guid!="")) {
            if (guid_equal(guid, "<% = JS_SafeString(clsProjectDataProvider.dgColAltName.ToString) %>")){ 
                $('#dataGridTable').datagrid('option', 'dgColAltDMGuid', "<% = JS_SafeString(Guid.Empty.ToString) %>");
            }else{
                $('#dataGridTable').datagrid('setAttrDM', guid, "<% = JS_SafeString(Guid.Empty.ToString) %>");
            };
            onHideDMOptions(true);
            showLoadingPanel();
            _ajax_ok_code = "onDMDeleted();";
            _ajaxSend("action=dm_delete&guid=" + encodeURI(guid));
        }
        attr_active = null;
    }

    function onDMDeleted() {
        hideLoadingPanel();
        $('#dataGridTable').datagrid('redraw');
    }

<% End if %>

</script> 
    <table border='0' cellspacing="0" cellpadding="0" class="whole">
       <tr valign="top" height="24">
        <td class='text' valign="middle" style_="padding-bottom:6px;">
            <div id="toolbar" class="dxToolbar"></div>
<%--            
                <telerik:RadToolBarButton ID="btnDownloadDataGrid" runat="server" Font-Bold="true" CommandName="Download" DisplayType="TextImage" Text="Download"/>
                <telerik:RadToolBarButton ID="btnUploadDataGrid" runat="server" Font-Bold="true" CommandName="Upload" DisplayType="TextImage" Text="Upload" PostBack="false" Enabled="false"/>
                <telerik:RadToolBarButton ID="btnSelecColumns" runat="server" Font-Bold="true" CommandName="Columns" DisplayType="TextImage" Text="Select Columns" PostBack="false" onclick="onSelectColumnsClick();"/>
        --%>
        </td>
       </tr>
        <tr><td id="tdContent">
        <div id="divContent" class="whole" style="text-align:left;vertical-align:top;overflow:auto;">
            <canvas id="dataGridTable">HTML5 not supported</canvas>
        </div>
       </td></tr>
    </table>
    <div id='selectColumnsForm' style='display: none; position: relative;'>
        <div id="divSelectColumns" style="padding: 5px; text-align: left;"></div>
        <div style='text-align: center; margin-top: 1ex; width: 100%;'>
            <a href="" onclick="filterColumnsCustomSelect(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
            <a href="" onclick="filterColumnsCustomSelect(0); return false;" class="actions"><% =ResString("lblNone")%></a>
        </div>
    </div>
    <div id="divDialogConf" style="display:none;"><p><% =ResString("msgDataGridDelDM") %></p></div>
    <% If App.Options.ProjectUseDataMapping Then %><div id="divDataMapping" style="display:none;"><iframe frameborder='0' src='' onload='onFrameLoaded(this);' name='frmImport' id='frmImport' style='width:100%; height:99%'></iframe></div><% End if %>
</asp:Content>