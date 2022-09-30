<%@ Page Language="VB" Inherits="InfodocsPage" title="Infodocs" Codebehind="Infodocs.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
    <script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/ec.dg.js"></script>
    <script>
        function onRichEditorRefresh(empty, infodoc, callback_param)
        {        
            window.focus();
            if (infodoc)
            {
                $("#dataGridTable").datagrid("updateInfodoc", callback_param[0], empty);                                  
            }    
        }              

        function scrolify(tblAsJQueryObject, height){
            var oTbl = tblAsJQueryObject;

            // for very large tables you can remove the four lines below
            // and wrap the table with <div> in the mark-up and assign
            // height and overflow property  
            var oTblDiv = $("<div></div>");
            oTblDiv.css('height', height);
            oTblDiv.css('overflow','scroll');               
            oTbl.wrap(oTblDiv);

            // save original width
            oTbl.attr("data-item-original-width", oTbl.width());
            oTbl.find('thead tr td').each(function(){
                $(this).attr("data-item-original-width",$(this).width());
            }); 
            oTbl.find('tbody tr:eq(0) td').each(function(){
                $(this).attr("data-item-original-width",$(this).width());
            });                 


            // clone the original table
            var newTbl = oTbl.clone();

            // remove table header from original table
            oTbl.find('thead tr').remove();                 
            // remove table body from new table
            newTbl.find('tbody tr').remove();   

            oTbl.parent().parent().prepend(newTbl);
            newTbl.wrap("<div></div>");

            // replace ORIGINAL COLUMN width                
            newTbl.width(newTbl.attr('data-item-original-width'));
            newTbl.find('thead tr td').each(function(){
                $(this).width($(this).attr("data-item-original-width"));
            });     
            oTbl.width(oTbl.attr('data-item-original-width'));      
            oTbl.find('tbody tr:eq(0) td').each(function(){
                $(this).width($(this).attr("data-item-original-width"));
            });                 
        }

        function syncReceived(params) {
            if ((params)) {            
                var received_data = eval(params);
                if ((received_data)) {
                    if (received_data[0] == 'reload'){
                        document.location.href = '<% =PageURL(CurrentPageID) %>?<% =GetTempThemeURI(True) %>';
                    };
                    if (received_data[0] == 'contributed_nodes') {                        
                        initContributionsDlg(received_data[1]);
                        dlg_contributions.dialog('open');
                    }
                }
            }        
        }

        function syncError() {
            //showLoadingPanel(0);
            dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", true, ";", "undefined", "Error", 350, 280);
            $(".ui-dialog").css("z-index", 9999);
        }

        var showAttributes = '<%=SelectedColumns(App.ActiveProject)%>';
        var attr_list = <%= GetAttributes() %>;
        
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
                var checked = showAttributes.indexOf(attr_list[k].guid) != -1;
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
                position: { my: "center", at: "center", of: $("body"), within: $("body") },
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
                        $.each(cb_arr, function(index, val) { var cid = val.value + ""; if (val.checked) { clmn_ids += (clmn_ids == "" ? "" : ",") + cid; } });
                        showAttributes = clmn_ids;
                        _ajaxSend('action=select_columns&column_ids=' + clmn_ids); // save the selected columns via ajax
                        options.showAttributes = showAttributes;  // update the visible columns option                        
                        $('#dataGridTable').datagrid('option', 'showAttributes', showAttributes);    // update the datagrid option
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

        //A1203 ===
        var dlg_contributions = null;
        var selected_event_name = "";

        function showContributedNodes(event_id, event_name) {                
            selected_event_name = event_name;

            _ajax_ok_code = syncReceived;
            _ajax_error_code = syncError;
            _ajaxSend('action=contributed_nodes&event_id=' + event_id);
        }

        function initContributionsDlg(nodes) {
            dlg_contributions = $("#divContributions").dialog({
                autoOpen: false,
                width: 490,
                height: 350,
                modal: true,
                closeOnEscape: true,
                dialogClass: "no-close",
                bgiframe: true,
                title: "<%=IIf(App.isRiskEnabled, IIf(PM.ActiveHierarchy = ECHierarchyID.hidImpact, ParseString("Sources of "), ParseString("Objectives of ")), ParseString("Contributions of ")) %>" + selected_event_name,
                position: { my: "center", at: "center", of: $("body"), within: $("body") },
                buttons: [{ text:"Close", click: function() { dlg_contributions.dialog( "close" ); }}],
                open:  function() { },
                close: function() { }
            });
            $(".ui-dialog").css("z-index", 9999);
            var txt = "";
            for (var i = 0; i < nodes.length; i++) {
                txt += nodes[i][1] + "<br>";
            }
            $("#divContributions").html(txt);
        }
        //A1203 ==

        var options = {
            rows: <%= GetRows() %>,
            columns: <%= GetColumns() %>,
            rowsTitle: "<%=ParseString("%%Alternatives%%")%>",
            viewMode: 'infodocs',
            infodocReadOnly: <%=Bool2JS(App.IsActiveProjectStructureReadOnly)%>,
            wrtInfodocs: <%= GetWRTInfodocs() %>,
            nodeInfodocs: <%= GetNodeInfodocs() %>,
            hierarchyNodes: <%= GetHierarchyColumns() %>,
            showAttributes: showAttributes, //A1201
            attributes: attr_list,          //A1201
            imgPath: '<%=ImagePath%>',
            contextMenu: true,
            hid: <%=IIf(App.isRiskEnabled, CInt(PM.ActiveHierarchy), -1)%>
        };

        $(document).ready(function () {
            initPage();
            options.height = $("#divContent").height() - 15;
            options.width = $("#divContent").width() - 15; 
            $("#dataGridTable").datagrid(options);
            //scrolify($('#dataGridTable'), 160);
        });

        function initPage() {
            resetDivContent()
            var disabled =  <%=Bool2JS(App.IsActiveProjectStructureReadOnly)%>;
            $("#divToolbar").dxToolbar({
                items: [{
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    options: {
                        disabled: false,
                        text: "Select Columns",
                        onClick: function () {
                            onSelectColumnsClick();
                        }
                    }
                }
                ]
            });
        }

        function resetDivContent (){
            $("#divContent").height(100);
            $("#divContent").width(100);
            var h = $("#divMain").height() - $("#divToolbar").height() - 10;
            var w = $("#divMain").width() - 10;
            $("#divContent").height(h);
            $("#divContent").width(w);
        }

        resize_custom = resizePage;

        function resizePage(force, w, h) {
            $("#divContent").height(h);
            $("#divContent").width(w);
            var toolbarHeight = ((($("#divToolbar"))) ? $("#divToolbar").height():0);
            if (typeof ((toolbarHeight)) == "undefined") {
                toolbarHeight = 0;
            };
            $('#dataGridTable').datagrid('resize', w-15, h - 40 - toolbarHeight);
            $('#dataGridTable').datagrid('redraw');
        }

    </script>
    <div id="divMain" style="width:99%;height:98%;text-align:left;vertical-align:top;overflow:hidden;">
        <%If PM.Attributes.GetAlternativesAttributes(True).Count > 0 Then%>
            <div id='divToolbar' class='dxToolbar'></div>
        <%End If%>
<%--        <div id="divContent" style="text-align: left; vertical-align: top; overflow: auto;">
            <table id="dataGridTable" class="dg_table"></table>
        </div>--%>
        <div class="overview" id="divContent" style="text-align: left; vertical-align: top; overflow: auto; margin-top:5px;">
            <canvas id="dataGridTable">HTML5 not supported</canvas>
        </div>
    </div>

    <div id='selectColumnsForm' style='display: none; position: relative;'>
        <div id="divSelectColumns" style="padding: 5px; text-align: left;"></div>
        <div style='text-align: center; margin-top: 1ex; width: 100%;'>
            <a href="" onclick="filterColumnsCustomSelect(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
            <a href="" onclick="filterColumnsCustomSelect(0); return false;" class="actions"><% =ResString("lblNone")%></a>
        </div>
    </div>

    <div id="divContributions" style="display:none; overflow:auto; position: relative;">
                
    </div>
</asp:Content>

