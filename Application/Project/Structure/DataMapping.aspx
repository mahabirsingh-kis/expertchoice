<%@ Page Language="VB" CodeBehind="DataMapping.aspx.vb" Inherits="DataMappingPage" title="Data Mapping" %>
<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
    <script type="text/javascript">

    function PleaseWait(vis) {
        SwitchWarning("<% =pnlLoadingPanel.ClientID %>", (vis));
        theForm.disabled = (vis);
    }

    var is_mapped = <% = Bool2JS(SourceModelID > 0) %>;
    var externaldb__name = "<% =JS_SafeString(ExternalDB_Name) %>";
    var externaldb_tbl = "<% =JS_SafeString(ExternalDB_Table)%>";
    var externaldb_col = "<% =JS_SafeString(ExternalDB_Column)%>"; //AS/12323l
    var externaldb_mkey = "<% =JS_SafeString(ExternalDB_MapKey)%>"; //AS/12323zo
    var externaldb_attr = "<% =JS_SafeString(ExternalDB_Attribute)%>"; 
    var externaldb_tp = "<% =JS_SafeString(ExternalDB_Type)%>"; //'AS/15285m
    var old_externaldb_tp = externaldb_tp; //'AS/16024

    var btnSQLImport = null;
    var btnSQLMap = null;
    var btnAccessImport = null;
    var btnAccessMap = null;
    var btnOracleImport = null;
    var btnOracleMap = null;
    var from_datagrid = false;
    var coltype = "<% = JS_SafeString(getColtype)%>"; //'AS/24231j


        // === Begin map to ECC PROJECT routines ===

    function onSelectProject(val) {
        checkControls();
    }

    function onMap() {
        var id = $("#cbModelsList").val();

        if ((id) && (id != null) && (id != "")) {
            id = id * 1;
            theForm.disabled = true;
            //_ajax_ok_code = "onMapProjectComplete(data);"; //'AS/24231j
            if (coltype == 0 || coltype == 1 || coltype == 2) { //alternatives names, Costs, or Risk //'AS/24231j===
                externaldb__name = "<% =JS_SafeString(ExternalDB_Name)%>"; //'AS/24189d
                checkControls();
            }
            if (coltype == 3) { //attributes
                externaldb_attr = "<% =JS_SafeString(ExternalDB_Attribute)%>"; //'AS/24189d
                _ajax_ok_code = "onMapAttrComplete(data);";
            }
            if (coltype == 5) { //judgments (cov obj)
                external_covobj = "<% =JS_SafeString(ExternalECC_Covobj)%>"; //'AS/24189d
                _ajax_ok_code = "onMapCovobjComplete(data);";//'AS/24231j==
            }

            _ajaxSend("action=connect_ecc&prj=" + id);
        }
    }

        //function onMapProjectComplete(data) { //'AS/24231j
    function onMapCovobjComplete(data) { //'AS/24231j
        theForm.disabled = false;
         is_mapped = (data=='1'); 
        checkControls();
        getCovObjList("model=src");
    }

    function getCovObjList(params) {
        theForm.disabled = true;
        _ajax_ok_code = "onGetCovObjCompleted(data);";
        _ajaxSend("action=cov_obj&" + params);
    }

    function onGetCovObjCompleted(data) {
        theForm.disabled = false;
        if (data!="") {
            data = eval(data);
            if (Array.isArray(data)) {
                // var cb = $('select[name="optSelGetNonPW"]'); //'AS/24231j
                var cb = $('select[name="cbExternalCovobj"]'); //'AS/24231j
                cb.empty();
                for (var i=0; i<data.length; i++) {
                    cb.append($('<option>', {value: data[i][0], text: data[i][1]}));
                }
                if (!data.length) { cb.append($('<option>', {value: '', text: ' - no covering objectives - '}).prop("disabled", "disabled")); }
                cb.prop("disabled", (data.length ? "" : "disabled"));
            }
            checkControls();
        }
    }
    
    function onMapAttrComplete(data) { //'AS/24231j
        theForm.disabled = false;
        is_mapped = (data=='1'); 
        checkControls();
        getAttributesList("model=src");
    }

    function getAttributesList(val) { //'AS/24231c===
        theForm.disabled = true;
        _ajax_ok_code = "onGetAttrCompleted(data);";
        _ajaxSend("action=get_attr&");
    }

    function onGetAttrCompleted(data) {
        theForm.disabled = false;
        if (data!="") {
            data = eval(data);
            if (Array.isArray(data)) {
                var cb = $('select[name="cbExternalAttr"]');
                cb.empty();
                for (var i=0; i<data.length; i++) {
                    cb.append($('<option>', {value: data[i][0], text: data[i][1]}));
                }
                if (!data.length) { cb.append($('<option>', {value: '', text: ' - no attributes - '}).prop("disabled", "disabled")); }
                cb.prop("disabled", (data.length ? "" : "disabled"));
            }
            checkControls();
        }
    }//'AS/24231c==

    function onExternalCovobjSelect(val) { //'AS/24231j
        theForm.disabled = true;
        _ajax_ok_code = "onGetUsersCompleted(data);";
        _ajaxSend("action=get_user&");
    } 
        
    function onGetUsersCompleted(data) { //'AS/24231j
        theForm.disabled = false;
        if (data!="") {
            data = eval(data);
            if (Array.isArray(data)) {
                var cb = $('select[name="cbExternalUser"]');
                cb.empty();
                for (var i=0; i<data.length; i++) {
                    cb.append($('<option>', {value: data[i][0], text: data[i][1]}));
                }
                if (!data.length) { cb.append($('<option>', {value: '', text: ' - no users - '}).prop("disabled", "disabled")); }
                cb.prop("disabled", (data.length ? "" : "disabled"));
            }
            checkControls();
        }
    }

    var is_connected = <% = Bool2JS(ConnectedDB_ID > 0) %>; //'AS/12323k===
    var exists_nonPWCovObj= <% = Bool2JS(GetNonPWCovbj(App.ActiveProject.ProjectManager).Count > 0) %>; //'AS/24189a
    
    function onSelectDB(val) {
        checkControls();
    }

    function onConnect() {
        var id = $("#cbRemoteDB").val();
        if ((id) && (id != null) && (id != "")) {
            id = id * 1;
            theForm.disabled = true;
            _ajax_ok_code = "onConnectComplete(data);";
            _ajaxSend("action=connect_db=" + id);
        }
    }

    function onConnectComplete(data) {
        theForm.disabled = false;
        is_connected = (data=='1'); 
        checkControls();
        GetDatabasesNames
    } //'AS/12323k==

    //function onImportAlt() { //'AS/24400 removed function
    //    if (is_mapped) {
    //        var params = "";
    //        //params += "&include=" + $('input[name=optInclude]:checked').val(); //'AS/24231k===
    //        // params += "&infodocs=" + (theForm.optIncludeInfodocs.checked ? 1 : 0);
    //        //params += "&everything=" + (theForm.optIncludeEverything.checked ? 1 : 0); 
    //        //params += "&covobjfrom=" + $('select[name="optSelGetNonPW"]').val(); 
    //        //params += "&attrfrom=" + $('select[name="optSelectAttribute"]').val(); //'AS/24231c
    //        //params += "&datato=" + $('select[name="optSelWriteNonPW"]').val();
    //        //params += "&scaleoverwrite=" + (theForm.optOverwriteScale.checked ? 1 : 0);
    //        //params += "&duplicates=" + $('input[name=optDuplicates]:checked').val();
    //        ////            alert(params);
    //        //$("#btnImport").prop("disabled", "disabled");
    //        //theForm.disabled = true; //'AS/24231k==
            //        params += "&covobjfrom=" + $('select[name="cbExternalCovobj"]').val(); //'AS/24231j===
    //        params += "&attrfrom=" + $('select[name="cbExternalAttr"]').val(); 
    //        params += "&userfrom=" + $('select[name="cbExternalUser"]').val(); //'AS/24231j==
    //        _ajax_ok_code = "onImportAltComplete(data);";
    //        _ajaxSend("action=importalt" + params);
    //    }
    //}

    function onImportObj() { //'AS/4488f===
        if (is_mapped) {
            var param = "";
            param += "&include=" + $('input[name=optInclude]:checked').val();
            $("#btnImportSubhierarchy").prop("disabled", "disabled");
            theForm.disabled = true;
            _ajax_ok_code = "onImportObjComplete(data);";
            _ajaxSend("action=importobj" + param);
        }
    }

    function onImportObjComplete(data) {
        theForm.disabled = false;
        $("#btnImportSubhierarchy").prop("disabled", "");
        is_done = (data=='1'); 
        //alert("Import result: " +  is_done); //'AS/24495
    } //'AS/4488f==

    function onImportJudg() { //'AS/13258===
        if (is_mapped) {
            var parm = "";
            parm += "&include=" + $('input[name=optInclude]:checked').val();
            $("#btnImportJudg").prop("disabled", "disabled");
            theForm.disabled = true;
            _ajax_ok_code = "onImportJudgComplete(data);";
            _ajaxSend("action=importjudg" + parm);
        }
    }

    function onImportJudgComplete(data) {
        theForm.disabled = false;
        $("#btnImportJudgments").prop("disabled", "");
        is_done = (data=='1'); 
        //alert("Import result: " +  is_done); //'AS/24495
    }//'AS/13258==

        function onImportAltComplete(data) { 
        theForm.disabled = false;
        $("#btnImport").prop("disabled", "");
        is_done = (data=='1'); 
            //alert("Import result: " +  is_done); //'AS/24495
        }

    function checkControls() {
        var p = $("#cbModelsList").val();
        var sel = (p != null && (p) && p != "");
        $("#btnMap").prop("disabled", (sel ? "" : "disabled"));
        ExternalDBImportDisabled(sel ? false: true); //'AS/24400  
        //$("#btnImport").prop("disabled", (sel && is_mapped && $('select[name="optSelGetNonPW"]').val()!=undefined && $('select[name="optSelGetNonPW"]').val()!="" ? "" : "disabled")); <%-- 'AS/24189a --%> 
        $("#btnImportSubhierarchy").prop("disabled", (sel && is_mapped && $('select[name="optSelGetNonPW"]').val()!=undefined && $('select[name="optSelGetNonPW"]').val()!="" ? "" : "disabled")); <%-- 'AS/12323j--%>
        $("#btnImportJudgments").prop("disabled", (sel && is_mapped && $('select[name="optSelGetNonPW"]').val()!=undefined && $('select[name="optSelGetNonPW"]').val()!="" ? "" : "disabled")); <%-- 'AS/13258--%>
  
        //if (sel && is_mapped) $("#btnImport").show(); else $("#btnImport").hide(); //'AS/24189a //'AS/24231k
        // if (is_mapped) $("#divImportOptions").show(); else $("#divImportOptions").hide(); //'AS/24189e 'AS/24189h

        if (coltype == 0 || coltype == 1 || coltype == 2) { //alternatives names, Costs, or Risk //'AS/24231j===
            $("#divExternalCovObjective").hide(); 
            $("#divExternalAttribute").hide();
            $("#divExternalUser").hide(); }

        if (coltype == 3) { //attributes
            $("#divExternalCovObjective").hide(); 
            $("#divExternalAttribute").show();
            $("#divExternalUser").hide(); }
        
        if (coltype == 5) { //judgments (cov obj)
            $("#divExternalCovObjective").show(); 
            $("#divExternalAttribute").hide();
            $("#divExternalUser").show(); } //'AS/24231j==
      
    }

        function onSQLAttrSelect(val) {
            if (val == true)  $("#cbSelectAttr").prop("disabled", "disabled"); else $("#cbSelectAttr").prop("disabled", "");
            // call function to populate list of custom attributes (where user selects the attribute to import data to)
            PleaseWait(true);
            _ajax_ok_code = "onSetSQLAttr(data);"; 
            _ajaxSend("action=externaldb_attr&attr=" + encodeURIComponent(val)); 
        } //'NBU?

        function onSetSQLAttr(data) {
            PleaseWait(false);
            if (data!="" && data!=undefined) {
                var res = eval(data);
                if (res[0]) {
                    _ajax_ok_code = "onSaveSQLAttrList(data);";
                    _ajaxSend("action=externaldb_attr&str=" + encodeURIComponent(conn_str));
                }
                if (res[1]!="") jDialog((res[0] ? "" : "<h6 class='error' style='text-align:left'>Unable to select table.</h6><br>") + res[1], true, ";");
            }
            checkSQLForm();
        } //'NBU?

        function onSQLCovObjSelect(val) {
            // sets up cov obj to which the data should be imported
            PleaseWait(true);
            _ajax_ok_code = "onSetSQLCovobj(data);"; 
            _ajaxSend("action=externaldb_judg&judg=" + encodeURIComponent(val)); 
        } //'NBU?

        function onSetSQLCovobj(data) {
            PleaseWait(false);
            if (data!="" && data!=undefined) {
                var res = eval(data);
                if (res[0]) {
                    _ajax_ok_code = "onSaveSQLCovobjList(data);";
                    _ajaxSend("action=externaldb_judg&judg=" + encodeURIComponent(conn_str));
                }
                if (res[1]!="") jDialog((res[0] ? "" : "<h6 class='error' style='text-align:left'>Unable to select table.</h6><br>") + res[1], true, ";");
            }
            checkSQLForm();
        } //'NBU?

        function onOptionsImportTo(val){
            //if (val == "optAttributes")  $("#divMapAtributes").show(); else $("#divMapAtributes").hide(); 
            if (val == "optNonPWjudgments")  $("#divMapNonPWjudgments").show(); else $("#divMapNonPWjudgments").hide();
        } //'NBU?

        function onChangeUser(val){ //'AS/12323zs
            //if (val == "optSelectUser")  $("#divUsers").show(); else $("#divUser").hide();
        } //'NBU?

        // === End map to ECC PROJECT routines ===

                    
    function onDBtypeSelect(val) { 
        // show-hide the corresponding areas 'AS/15285m
<%--    if (val == <% =CInt(clsDataMapping.enumMappedDBType.mdtAccess)%>)  $("#divMapToAccess").show(); else $("#divMapToAccess").hide();  'AS/15285m===
        if (val == <% =CInt(clsDataMapping.enumMappedDBType.mdtSQL)%>)  $("#divMapToExternalDBServer").show(); else $("#divMapToExternalDBServer").hide();
        if (val == <% =CInt(clsDataMapping.enumMappedDBType.mdtECC)%>)  $("#divMapToECCproject").show(); else $("#divMapToECCproject").hide();
        if (val == <% =CInt(clsDataMapping.enumMappedDBType.mdtOracle)%>)  $("#divMapToOracle").show(); else $("#divMapToOracle").hide(); 'AS/15285m== --%>

        externaldb_tp = "<% =JS_SafeString(ExternalDB_Type)%>"; //'AS/15285p

        if (val == undefined && externaldb_tp == -1) { //'AS/15285m=== 'AS/15285p added ==-1 and else if<> undefined
            externaldb_tp = 3; }
        else if(val != undefined) { 
             
            if(val != externaldb_tp){ //'AS/15624p===  //'AS/15624r=== moved it to-under the Else If
                //clear out the boxes
                try {
                theForm.tbExternalDBConnStr.value="";
                $('#cbExternalDBTbl').empty();
                $('#cbExternalDBCol').empty();
                $('#cbExternalDBMapKey').empty();
                }
                catch (err){
                    // do nothing
                }
            }
            else if(val == externaldb_tp) { 
                // reload the form to restore the boxes to the initial values
               document.location.reload();
            } //'AS/15624p== //'AS/15624r==
            
            externaldb_tp = val; 
        }


        checkExternalDBForm();//'AS/15285o

        _ajaxSend("action=db_select&dbtype=" + encodeURIComponent(val)); //AS/15597b reapplied (was removed in the prev commit -- changeset 3717 (24a99bcee553) as of 09-17-2018)

       // $("#trDirection").toggle(val!=<% =CInt(clsDataMapping.enumMappedDBType.mdtECC)%>); //'AS/24192
        checkDirection();
    }

        // === Begin map to External DB routines ===

    function getSelectedValues(select) { //'AS/15285e
        //returns an array containing each selected option
        var db = $("#cbDBtype").val();//'AS/15285k==
        //_ajaxSend("action=" + (do_import ? (isImport() ? "externaldb_import" : "externaldb_export") : "externaldb_mapcol") + externaldb_params);
        var selectedcols = $('#cbSelectCol').val(); //'AS/15285n
        _ajaxSend("action=select_col&selected_col=" + selectedcols);

        //OR another way:
        //var result = [];
        //var options = select && select.options;
        //var opt;

        //for (var i=0, iLen=options.length; i<iLen; i++) {
        //    opt = options[i];

        //    if (opt.selected) {
        //        result.push(opt.value || opt.text);
        //    }
        //}
        //return result;
    }

    function isImport() {
        return $('input[name=optDirection]:checked').val()!="0";
    }

    function isGroupUser() { //'AS/24192
        var is_groupuser =  <% = Bool2JS(IsGroupSelected() > 0) %>;
        return $(is_groupuser);
    }


    function checkMultipleSelection(val) { //'AS/15285j
        if (val == "optExportSelected")  $("#divDISelectColumns").show(); else $("#divDISelectColumns").hide();
    }


    function checkDirection() {
        var is_import = isImport();
        var name = (is_import) ? "Import" : "Export";
        var is_groupuser = isGroupUser //'AS/24192
        if (btnExternalDBImport!=null) { btnExternalDBImport.text(name).prop('value', name); }
        if (btnAccessImport!=null) { btnAccessImport.text(name).prop('value', name); }
        if (btnOracleImport!=null) { btnOracleImport.text(name).prop('value', name); }

        if (externaldb_tp==1 ) {  //'AS/24231h=== 'AS/24189e moved up
            $("#trDirection").hide();
            // is_import=0 // 'AS/24192
        } //'AS/24231h==

        if (is_import != true) { //'AS/24191==
            var dis_exp = <% = Bool2JS(ExportAllowed(False) = 0) %>;
            ExternalDBExportDisabled(dis_exp);
        } else {
            var dis_imp = <% =Bool2JS(ImportAllowed(False)) %>;
            ExternalDBImportDisabled(dis_imp);
        } //'AS/24191==

        if (externaldb_tp==5 ) { $("#divImportOptions").hide();} else { $("#divImportOptions").toggle(is_import);}//'AS/16438a 

        if (is_groupuser) { $("#divDIOptions").hide();} else { $("#divDIOptions").show();} //'AS/24192
        
        $(".column_from").html(is_import ? "<% = JS_SafeString(ResString("lblDMColumnImportFrom"))%>" : "<% = JS_SafeString(ResString("lblDMColumnExportTo"))%>");
        $(".column_key").html(is_import ? "<% = JS_SafeString(ResString("lblDMColumnImportKey"))%>" : "<% = JS_SafeString(ResString("lblDMColumnExportKey"))%>");
    }

    //$(document).ready(function () { checkControls(); if (is_mapped) getCovObjList("model=src"); });
        $(document).ready(function () { 
            // checkForm();  'AS/15285m
            checkControls();
            checkExternalDBForm(); //'AS/15285m 'AS/15285o fixed syntax

            //var db = $("#cbDBtype").val(); //'AS/24507
            var dbindex = 0//'AS/24507===
            if (externaldb_tp==1 ) { //ECC model
                dbindex=3; //dbindex=5; 'AS/24189f
            } 
            if (externaldb_tp==2 ) { //Access 
                dbindex=1; 
            } 
            if (externaldb_tp==3 ) { //SQL
                dbindex=0; 
            } 
            if (externaldb_tp==4 ) { //Oracle
                //dbindex=2; 'AS/24189f
            } 
            if (externaldb_tp==5 ) { //MSProject
                dbindex=2; //dbindex=3; 'AS/24189f
            } 
            if (externaldb_tp==6 ) { //MSProjectServer
                //dbindex=4; 'AS/24189f
            } 
            document.getElementById("cbDBtype").selectedIndex = dbindex;
            var db = $("#cbDBtype").val(); //'AS/24507==

            var edt = "cbDBtype";
            edt= "tbExternalDBConnStr"; setTimeout('onExternalDBConnect(true);', 350); //'AS/15285n 'AS/15285s commented out - caused extra call and msg "empty string" on form open 'AS/15624k put back, it broke autofill
            if ($("#" + edt).length) setTimeout('$("#' + edt + '").select().focus();', 1500);

            if (mode=="obj") { //'AS/15959===
                $("#divImportOptions").hide();
            } //'AS/15959==
            setTimeout(checkDirection, 100);

        }); //added checkOracleForm();

        //setTimeout('window.focus(); theForm.cbDBtype.focus();', 500);

        // Begin unified routines -- 'AS/15285m===

    function onCancel() { //'AS/16024===
        //reset externaldb_tp and ExternalDB_Type
        externaldb_tp=undefined
        _ajaxSend("action=externaldb_cancel"); //'AS/16024b
    } //'AS/16024==

    function onImport(do_import) {
        var db = $("#cbDBtype").val();
        if (externaldb_tp == 1) { //'AS/25796===
            onMap();
        } //'AS/25796==
        onExternalDBImport(do_import);
    }

    function onMapping() {
        var db = $("#cbDBtype").val();
        if (externaldb_tp == 1) { //'AS/25796===
            onMap();
        } //'AS/25796==
        onExternalDBMap();
    }

    function onExternalDBMap() {
        {   var externaldb_params = ""; //'AS/12323v===
            externaldb_params += "&importas=" + $('input[name=optImport]:checked').val();
            //externaldb_params += "&attributesto=" + $('select[name="cbSelectAttr"]').val(); //'AS/15285n
            externaldb_params += "&newattribute=" + $('input[name=optCreateNewAttr]:checked').val();
            externaldb_params += "&nonpwdatato=" + $('select[name="cbExternalDBCoveObj"]').val(); 
            externaldb_params += "&newalt=" + $('input[name=ckCreateNewAlt]:checked').val(); 
            externaldb_params += "&replacevalue=" + $('input[name=optReplaceValue]:checked').val(); 
            externaldb_params += "&touser=" + $('input[name=optUser]:checked').val(); 
            externaldb_params += "&replacealt=" + $('input[name=ckReplaceAlt]:checked').val(); 
            externaldb_params += "&includecolumns=" + $('input[name=optDIincludeColumns]:checked').val(); 
            externaldb_params += "&selected_col=" + $('#cbSelectCol').val();//'AS/15285f

            externaldb_params += "&covobjfrom=" + $('select[name="cbExternalCovobj"]').val(); //'AS/24400===
            externaldb_params += "&attrfrom=" + $('select[name="cbExternalAttr"]').val(); 
            externaldb_params += "&userfrom=" + $('select[name="cbExternalUser"]').val(); //'AS/24400==
            externaldb_params += "&isimport=" + $('input[name=optDirection]:checked').val(); //'AS/24192

            ExternalDBImportDisabled(true);
            //theForm.disabled = true;
            _ajax_ok_code = "onImportExternalDBComplete(data);"; //' closes the mapping dialog
            _ajaxSend("action=externaldb_createmapping" + externaldb_params); 
        }
    } 

    function checkExternalDBForm() {
        if (theForm.tbExternalDBConnStr != undefined) { //'AS/15285p enclosed and added return to exit function
            theForm.cbExternalDB.disabled = (theForm.tbExternalDBConnStr.value==""); }
        else { return }

        //var db_val = theForm.cbExternalDB.value;
        //theForm.cbExternalDBTbl.disabled = (db_val=="" || db_val==undefined);

        var is_colmapped =  <% = Bool2JS(MappingExists() > 0) %>; //'AS/16011
        var is_groupuser =  <% = Bool2JS(IsGroupSelected() > 0) %>; //'AS/16038

        if (externaldb_tp==3 ) { //'AS/15285n=== 
            var db_val = theForm.cbExternalDB.value;
            $("#divExternalDBname").show();  //'AS/15285r
            theForm.cbExternalDBTbl.disabled = (db_val=="" || db_val==undefined); 
        } else { 
            $("#divExternalDBname").hide();  //'AS/15285r
            theForm.cbExternalDBTbl.disabled = (theForm.tbExternalDBConnStr.value=="");  //'AS/15285o
        }  //'AS/15285n==

        var tbl_val = theForm.cbExternalDBTbl.value;
        theForm.cbExternalDBCol.disabled = (tbl_val=="" || tbl_val==undefined);
        var col_val = theForm.cbExternalDBCol.value;
        theForm.cbExternalDBMapkey.disabled = (col_val=="" || col_val==undefined);
        var mapkeycol_val = theForm.cbExternalDBMapkey.value;
        ExternalDBImportDisabled(tbl_val=="" || tbl_val==undefined || col_val=="" || col_val==undefined || mapkeycol_val=="" || mapkeycol_val==undefined);

        if (externaldb_tp==5 ) { // mpp file'AS/15597c=== 
            var db_val = theForm.cbExternalDB.value;
            $("#divExternalDBTbl").hide();  
            $("#divExternalDBCol").hide();
            $("#divExternalDBMapkey").hide();
            theForm.cbExternalDBTbl.disabled = (db_val=="" || db_val==undefined);
            document.getElementById("lblConnString").innerHTML="Enter full path to the MS Project file"; //'AS/15597i=== 
            //document.getElementById("lblCreateNewAlt").innerHTML="Create New Task"; //'AS/16438a
            $("#divGraySmall").hide(); //'AS/15597i==
            $("#divDIOptions").hide();//'AS/16438
            $("#divImportOptions").hide();//'AS/16438a
            //ExternalDBImportDisabled(false); //'AS_hardcoded //'AS/16438

        } else { 
            $("#divExternalDBTbl").show(); 
            $("#divExternalDBCol").show();
            $("#divExternalDBMapkey").show();
            theForm.cbExternalDBTbl.disabled = (theForm.tbExternalDBConnStr.value==""); 
            document.getElementById("lblConnString").innerHTML="Enter Connection String"; //'AS/15597i=== 
            //document.getElementById("lblCreateNewAlt").innerHTML="Create New Alternative"; //'AS/16438a
            $("#divGraySmall").show(); //'AS/15597i==
            // $("#divDIOptions").show();//'AS/16438 //'AS/24192
            if (is_groupuser) { $("#divDIOptions").hide();} else { $("#divDIOptions").show();} //'AS/24192
            $("#divImportOptions").show();//'AS/16438a
        }  //'AS/15597c==

        if (externaldb_tp==1 ) { //ECC model 'AS/24189===
            $("#divMapToExternalDB").hide();
            $("#divMapToECCproject").show();
        }  else { 
            $("#divMapToExternalDB").show();
            $("#divMapToECCproject").hide();
        }//'AS/24189==

        if (is_colmapped == true) { //'AS/16011=== 'AS/1-24-19===
            theForm.cbDBtype.disabled = true;
            theForm.cbExternalDB.disabled = true;
            theForm.tbExternalDBConnStr.disabled = true;
            //theForm.cbExternalDBTbl.disabled = true; 'AS/22565a===
            //theForm.cbExternalDBCol.disabled = true;
            //theForm.cbExternalDBMapkey.disabled = true; //'AS/1-24-19== 'AS/22565a==
        }

        if (is_groupuser==true) { //'AS/16038===
            if (mode!="alts" && mode!="cost" && mode!="attribute" && mode!="risk") { //'AS/17325 enclosed
                theForm.optImport.disabled = true
                $("#optImport").prop("checked", "");
                $("#optExport").prop("checked", "checked");
            }
        } //'AS/16038==

    } 

    //function checkForm() {
        btnExternalDBImport = $("#btnExternalDBImport");
        btnExternalDBMap = $("#btnExternalDBMap"); //'AS/15285o'
        onDBtypeSelect($("#cbDBtype").val());
        var mode = "<% = JS_SafeString(CheckVar("mode", ""))%>";
        if (mode!="" && (window.parent) && (window.parent.btnImport)) {
            from_datagrid = true;
            btnExternalDBImport = window.parent.btnImport;
            btnExternalDBMap = window.parent.btnMap;
            btnAccessImport = window.parent.btnImport;
            btnAccessMap = window.parent.btnMap;
            btnOracleImport = window.parent.btnImport;
            btnOracleMap = window.parent.btnMap;
            btnECCImport = window.parent.btnMap;//wed1

            $("#btnExternalDBImport").hide();
            $("#trBtnExternalDBImport").hide();
            $("#btnAccessImport").hide();     
            $("#trBtnAccessImport").hide();
            $("#btnOracleImport").hide();
            $("#trBtnOracleImport").hide();
            //$("#div_ckCreateNewAlt").hide(); //'AS/15624s-
            $("#divImportOptions").hide();//'AS/15624s+

            if (mode=="alts") {
                //$("#div_optAlternatives").show();
                $("#optAlternatives").prop("checked", "checked");
                //$("#div_ckCreateNewAlt").show(); //'AS/15624s-
                $("#divImportOptions").show();
            } else {
                $("#optReplaceValue").prop("checked", "checked");
            }
            if (mode=="cost") {
                //$("#div_optCosts").show();
                $("#optCosts").prop("checked", "checked");
            }
            if (mode=="risk") {
                //$("#div_optRisks").show();
                $("#optRisks").prop("checked", "checked");
            }
            if (mode=="attribute") {
                //$("#div_optAttributes").show();
                $("#optAttributes").prop("checked", "checked");
                onOptionsImportTo("optAttributes");
                var a_guid = "<% = JS_SafeString(CheckVar("aguid", "")) %>";
                if (a_guid!="" && a_guid != guid_empty) {
                    //if ($('#cbSelectAttr option[value=' + a_guid +']').length) {
                        $("#optCreateNewAttr").prop("checked", "");
                    //}
                }
            }
            //if (mode=="obj") { //'AS/15959=== moved to document.ready()
            //    $("#divImportOptions").hide();
            //} //'AS/15959==
        } 
   // }

        function onExternalDBConnect(idle) {
            // validates connection string, establishes connection and calls function to populate list of databases
            if (theForm.tbExternalDBConnStr) {
                var conn_str = trim(theForm.tbExternalDBConnStr.value);

                if (conn_str=="") {
                    if (!idle) jDialog("Connection string is empty!", true, "setTimeout('theForm.tbExternalDBConnStr.focus();', 100);");
                    return//'AS/15624l
                } 

                else {
                    if (externaldb_tp==-1) { //'AS/15285p
                        externaldb_tp = "<% =JS_SafeString(ExternalDB_Type)%>"; }
                        
                    //  //Make sure that conection string matches the selected database type//'AS/15285s=== 'AS/15624u===
                
                    //    var cnnstr = conn_str.toLowerCase(); }
                    //if (externaldb_tp==3) { //SQL Server
                    //    var pos = cnnstr.search("server");
                    //    if (pos == -1){ //'AS/15624u===
                    //        pos = cnnstr.search("local");
                    //    } //'AS/15624u==
                    //    if (pos == -1){
                    //        if (!idle) jDialog("Connection string is not valid!", true, "setTimeout('theForm.tbExternalDBConnStr.focus();', 100);");
                    //        return
                    //    }  
                    //}

                    //else if (externaldb_tp==2) { //Access
                    //    var pos = cnnstr.search("provider");
                    //    if (pos == -1){
                    //        if (!idle) jDialog("Connection string is not valid!", true, "setTimeout('theForm.tbExternalDBConnStr.focus();', 100);");
                    //        return
                    //    }  
                    //}

                    //else if (externaldb_tp==4) { //Oracle
                    //    var pos = cnnstr.search("data source");
                    //    if (pos == -1){
                    //        if (!idle) jDialog("Connection string is not valid!", true, "setTimeout('theForm.tbExternalDBConnStr.focus();', 100);");
                    //        return
                    //    }  
                    //}//'AS/15285s== 'AS/15624u==

                    if (externaldb_tp==2 || externaldb_tp==4) { //Access or Oracle 'AS/15285m=== 
                        _ajax_ok_code = "onGetExternalDBTblList(data);";
                        _ajaxSend("action=externaldb__name&str=" + encodeURIComponent(conn_str));
                    } 
                    else if (externaldb_tp==5) { //'AS/16438===
                        ExternalDBImportDisabled(false);
                        _ajaxSend("action=externaldb_connect&str=" + encodeURIComponent(conn_str));
                    } //'AS/16438==

                    else { 
                        _ajax_ok_code = "onGetExternalDBList(data);";
                        _ajaxSend("action=externaldb_connect&str=" + encodeURIComponent(conn_str));//'AS/15285
                    } //'AS/15285m==
                }
            }
        }//'AS/15624v

        function onGetExternalDBList(data) {
            //populates combo - list of databases
            PleaseWait(false);
            if (data!="" && data!=undefined) {
                try { //'AS/15624l
                    var res = eval(data);
                    if (res[0] && res.length>2) {
                        $('#cbExternalDB').empty()//'AS/12323zq
                        var has_sel = false;
                        $.each(res[2], function (i, item) {
                            $('#cbExternalDB').append($('<option>', { 
                                value: item,
                                text : item
                            }));
                            if (item==externaldb__name) has_sel = true;
                        });
                        //$("#cbExternalDB option:selected").removeAttr("selected");
                        $("#cbExternalDB").val(externaldb__name);
                        if (has_sel) {
                            setTimeout('onExternalDBSelect(externaldb__name);', 50); 
                        } else {
                            if (externaldb_tbl=='') $('#cbExternalDBTbl').empty()
                        }
                    }
                    if (res[1]!="") jDialog((res[0] || !res[2].length ? "" : "<h6 class='error' style='text-align:left'>Unable to get the list of databases.</h6><br>") + res[1], true, ";");
                }
                catch(err) { //'AS/15624l===
                    // do nothing
                }//'AS/15624l==

            }
            checkExternalDBForm();
        }

            function onExternalDBSelect(val) {
                // calls function to populate list of tables
                PleaseWait(true);
                _ajax_ok_code = "onGetExternalDBTblList(data);";
                _ajaxSend("action=externaldb__name&db=" + encodeURIComponent(val));
            }

            //function onExternalDBselect(val) { //duplicate?
            //    // calls function to populate list of tables
            //    PleaseWait(true);
            //    _ajax_ok_code = "onExternalDBConnect(data);";
            //    _ajaxSend("action=externaldb__name&db=" + encodeURIComponent(val));
            //}

            function onGetExternalDBTblList(data) { //'AS/12323xo
                //populates combo - list of tables
                PleaseWait(false);
                if (data!="" && data!=undefined) {
                    try { //'AS/15624m
                        var res = eval(data);
                        if (res[0] && res.length>2) {
                            $('#cbExternalDBTbl').empty()//'AS/12323zq
                            var has_sel = false;
                            $.each(res[2], function (i, item) {
                                $('#cbExternalDBTbl').append($('<option>', { 
                                    value: item,
                                    text : item
                                }));
                                if (item==externaldb_tbl) has_sel = true;
                            });

                            $("#cbExternalDBTbl").val(externaldb_tbl);
                            if (has_sel) {
                                setTimeout('onExternalDBTblSelect(externaldb_tbl);', 50); 
                            } else {
                                if (externaldb_col=='') $('#cbExternalDBCol').empty()
                            }
                        }
                        if (res[1]!="") jDialog((res[0] || !res[2].length ? "" : "<h6 class='error' style='text-align:left'>Unable to get the list of tables.</h6><br>") + res[1], true, ";");
                    }
                    catch(err) { //'AS/15624m===
                        // do nothing
                    }//'AS/15624m==
                }
                checkExternalDBForm();
            }

            function onExternalDBTblSelect(val) {
                // call function to populate list of columns (where user selects the column to import data from)
                PleaseWait(true);
                // _ajax_ok_code = "onSetExternalDBTable(data);"; //AS/12323l moved to onSetExternalDBColumn
                _ajax_ok_code = "onGetExternalDBColList(data);"; //AS/12323l 
                _ajaxSend("action=externaldb_table&tbl=" + encodeURIComponent(val));
            }

            function onGetExternalDBColList(data) {
                // populates combo - list of columns
                PleaseWait(false);
                if (data!="" && data!=undefined) {
                    try { //'AS/15624n
                        var res = eval(data);
                        if (res[0] && res.length>2) {
                        $("#cbExternalDBCol").empty(); //AS/13232zo
                        $("#cbExternalDBMapkey").empty(); //AS/13232zo
                        $.each(res[2], function (i, item) {
                            $('#cbExternalDBCol').append($('<option>', { 
                                value: item,
                                text : item
                            }));
                            $('#cbExternalDBMapkey').append($('<option>', { //AS/13232zo===
                                value: item,
                                text : item
                            })); //AS/13232zo==
                        });
                        $("#cbExternalDBCol").val(externaldb_col);
                        $("#cbExternalDBMapkey").val(externaldb_mkey);
                    }
                    if (res[1]!="") jDialog((res[0] || !res[2].length ? "" : "<h6 class='error' style='text-align:left'>Unable to get the list of columns.</h6><br>") + res[1], true, ";");
                    }
                    catch(err) { //'AS/15624n===
                        // do nothing
                    }//'AS/15624n==
                }
                checkExternalDBForm();
            }
    
            function onExternalDBColSelect(val) {
                PleaseWait(true);
                _ajax_ok_code = "onSetExternalDBColumn(data);";
                _ajaxSend("action=externaldb_column&col=" + encodeURIComponent(val));
            } 
        
            function onSetExternalDBColumn(data) {
                PleaseWait(false);
                if (data!="" && data!=undefined) {
                    var externaldb_conn_str = trim(theForm.tbExternalDBConnStr.value);//'AS/15285i
                    try { //'AS/16011
                        var res = eval(data);
                        if (res[0]) {
                            // do action on select column
                            // save list of columns as new attributes
                            //_ajax_ok_code = "onSaveExternalDBColsList(data);"; 'AS/15285i
                            _ajaxSend("action=externaldb_columns&str=" + encodeURIComponent(externaldb_conn_str)); //'AS/15285i
                        }
                        if (res[1]!="") jDialog((res[0] ? "" : "<h6 class='error' style='text-align:left'>Unable to select table.</h6><br>") + res[1], true, ";");
                    }            
                    catch(err) { //'AS/16011===
                        // do nothing
                    }//'AS/16011==
                }
                checkExternalDBForm();
            }

            function onExternalDBMapkeySelect(val) {
                PleaseWait(true);
                _ajax_ok_code = "onSetMapkeyExternalDBColumn(data);";
                _ajaxSend("action=externaldb_mkey&mapkey=" + encodeURIComponent(val));
            } 
        
            function onSetMapkeyExternalDBColumn(data) {
                PleaseWait(false);
                if (data!="" && data!=undefined) {
                    var externaldb_conn_str = trim(theForm.tbExternalDBConnStr.value);//'AS/15285i
                    try { //'AS/16011
                        var res = eval(data);
                        if (res[0]) {
                            // do action on select column
                            // save list of columns as new attributes
                            //_ajax_ok_code = "onSaveExternalDBColsList(data);"; 'AS/15285i
                            _ajaxSend("action=externaldb_mkey&str=" + encodeURIComponent(externaldb_conn_str)); //'AS/15285i
                        }
                        if (res[1]!="") jDialog((res[0] ? "" : "<h6 class='error' style='text-align:left'>Unable to select table.</h6><br>") + res[1], true, ";");
                    }
                    catch(err) { //'AS/16011===
                        // do nothing
                    }//'AS/16011==
                }
                checkExternalDBForm();
            }

            function ExternalDBExportDisabled(dis) { //'AS/24191
                try { 
                    if (btnExternalDBImport.length) btnExternalDBImport.button({disabled: (dis)}); else btnExternalDBImport.prop("disabled", ((dis) ? "disabled" : ""));
                }
                catch (err){
                    // rte occurrs if the form not loaded
                }
            }

            function ExternalDBImportDisabled(dis) {
                try { //'AS/15959 enclosed
                    if (btnExternalDBImport.length) btnExternalDBImport.button({disabled: (dis)}); else btnExternalDBImport.prop("disabled", ((dis) ? "disabled" : ""));
                    if ((btnExternalDBMap) && (btnExternalDBMap.length)) btnExternalDBMap.button({disabled: (dis)});
                }
                catch (err){
                    // rte occurrs if the form not loaded
                }
            }

            function onExternalDBImport(do_import) {
                PleaseWait(true) //'AS/22602c

                {var externaldb_params = ""; //'AS/12323v===
                    externaldb_params += "&importas=" + $('input[name=optImport]:checked').val();
                    externaldb_params += "&attributesto=" + $('select[name="cbSelectAttr"]').val();
                    externaldb_params += "&newattribute=" + $('input[name=optCreateNewAttr]:checked').val();
                    externaldb_params += "&nonpwdatato=" + $('select[name="cbExternalDBCoveObj"]').val(); //'AS/12323v== 'AS/12323zt replaced cbSelectAttr with cbExternalDBCoveObj
                    externaldb_params += "&newalt=" + $('input[name=ckCreateNewAlt]:checked').val(); //'AS/12323zb===
                    //externaldb_params += "&writetoempty=" + $('input[name=optWriteToEmptyValueOnly]:checked').val(); //'AS/12323zr
                    externaldb_params += "&replacevalue=" + $('input[name=optReplaceValue]:checked').val(); //'AS/12323zb==
                    externaldb_params += "&touser=" + $('input[name=optUser]:checked').val(); //'AS/12323zt
                    externaldb_params += "&replacealt=" + $('input[name=ckReplaceAlt]:checked').val(); //'AS/14506b==
                    externaldb_params += "&includecolumns=" + $('input[name=optDIincludeColumns]:checked').val(); //'AS/15285d
                    externaldb_params += "&selected_col=" + $('#cbSelectCol').val();//'AS/15285f

                    externaldb_params += "&fromuser=" + $('#cbExternalUser').val(); //'AS/24231k

                    externaldb_params += "&covobjfrom=" + $('select[name="cbExternalCovobj"]').val(); //'AS/24400===
                    externaldb_params += "&attrfrom=" + $('select[name="cbExternalAttr"]').val(); 
                    externaldb_params += "&userfrom=" + $('select[name="cbExternalUser"]').val(); //'AS/24400==
                    externaldb_params += "&isimport=" + $('input[name=optDirection]:checked').val(); //'AS/24192

                    ExternalDBImportDisabled(true);
                    //theForm.disabled = true;
                    _ajax_ok_code = "onImportExternalDBComplete(data);";
                    _ajaxSend("action=" + (do_import ? (isImport() ? "externaldb_import" : "externaldb_export") : "mapcol") + externaldb_params); //'AS/15285
                }
            } 

            function onImportExternalDBComplete(data) {
                theForm.disabled = false;
                ExternalDBImportDisabled(false);
                is_done = (data=='1'); 
                if ((from_datagrid) && (window.parent)) {
                    window.parent.onImportDataCallback(typeof(data)=="string" ? eval(data) : data);
                }
                //alert("Import result: " +  is_done); //'AS/24495
            } 

        // End unified routines -- 'AS/15285m==

</script>
<EC:LoadingPanel id="pnlLoadingPanel" runat="server" isWarning="true" WarningShowOnLoad="false" WarningShowCloseButton="false" Width="230" Visible="true"/>

<p align="center"><table align="center" border="0" style="height:99%" cellspacing="0" cellpadding="0">
    <tr>
        <td valign="top" align="center" class="text">
           
             <%--    =====  SELECT DATABASE TYPE --%>
            <div id="divSelectDBtype" class="text" style="margin-bottom:1em;">
                <fieldset class="legend" style="padding: 1em; text-align: center; width:450px;">
                    <table align="center" border="0" style="height:99%" cellspacing="2" cellpadding="0">
                        <tr><td align="right" class="text" id="txtSelectDB"><nobr><b>Select External Source</b>:</nobr></td><td class="text"><select id="cbDBtype" name="cbDBtype" onchange="onDBtypeSelect(this.value);" style="width:24em;"><% =GetDBtypesList()%></select></td></tr> <%--'AS/15597i added  id="txtSelectDB" and Select External Source --%>
                        <tr id="trDirection"><td align="right" class="text">Direction:</td><td class="text">
                            <input type="radio" name="optDirection" id="optImport" value="1" checked="checked" onclick="checkDirection();"/><label for="optImport">Import</label> &nbsp; 
                            <input type="radio" name="optDirection" id="optExport" value="0" onclick="checkDirection();"/><label for="optExport">Export</label></td>
                        </tr>
                    </table>
                </fieldset>
            </div>

                <%-- ===== SECTION FOR DATA MAPPING TO SQL/ACCESS ===== --%>
            <div id="divMapToExternalDB">
                <fieldset class="legend" style="padding: 1em; text-align: left; width:450px"> <%-- 'AS/12323o --%>
                    <legend class="text legend_title">Map to External Source</legend> <%--'AS/15597i--%>
                
                    <div id="divConnString"><span id="lblConnString">Enter Connection String:</span><br />  <%--'AS/15597i incorporated label --%>
                        <input type="text" id="tbExternalDBConnStr" name="tbExternalDBConnStr" style="width:100%; margin-bottom:2px;" value="<% = SafeFormString(ExternalDB_ConnectionString)%>"/>
                    </div>
                    <div style="text-align:right;float:right">
                        <input type="button" class="button" id="btnExternalDBConnect" name="btnExternalDBConnect" value="Connect" onclick="externaldb__name=''; externaldb_tbl=''; onExternalDBConnect(false);"/>
                    </div>
                    <div id="divGraySmall" class="gray small" style="padding-top:3px;">Use valid connection string to external database </div> <%--'AS/15597i added id="divGraySmall"--%>

                    <%-- ===== map database, table, column, and map key =====--%>
                    <div  id="divExternalDBname" style="margin-top:4px;">Select database:<br /><select id="cbExternalDB" name="cbExternalDB" onchange="externaldb_tbl=''; onExternalDBSelect(this.value);" style="width:100%;"><% =ExternalDB_Name%></select></div>  <%-- 'AS/12323xq added ExternalDB_Name 'AS/15285r added div id--%>
                    <div  id="divExternalDBTbl" style="margin-top:4px;">Select table:<br><select id="cbExternalDBTbl" name="cbExternalDBTbl" onchange="externaldb_col=''; onExternalDBTblSelect(this.value);" style="width:100%;"><% =ExternalDB_Table%></select></div>  <%-- 'AS/12323xq added externaldb_Table 'AS/15597c added div id--%> 
                    <div  id="divExternalDBCol" style="margin-top:4px;"><span class='column_from'>Select column to import data from:</span><br><select id="cbExternalDBCol" name="cbExternalDBCol" onchange="onExternalDBColSelect(this.value);" style="width:100%;"><% =ExternalDB_Column%></select></div>  <%-- 'AS/12323xq added externaldb_Column 'AS/15597c added div id--%>
                    <div  id="divExternalDBMapkey" style="margin-top:4px;"><span class='column_key'>Select column to create mapping keys:</span><br><select id="cbExternalDBMapkey" name="cbExternalDBMapkey" onchange="onExternalDBMapkeySelect(this.value);" style="width:100%;"><% =ExternalDB_MapKey%></select></div>  <%-- 'AS/12323xq added externaldb_MapKey 'AS/15597c added div id--%>
                </fieldset>
            </div> <%-- end divMapToExternalDB--%> <%--'AS/24231imoved up--%>

                <%-- ===== SECTION FOR DATA MAPPING TO ANOTHER ECC MODEL===== --%>
            <div id="divMapToECCproject" <%--hidden="hidden"--%>> <%-- 'AS/24231g=== --%> <%--'AS/24231k=== --%>
                <fieldset class="legend" style="padding: 1em; text-align: left; width:450px">
                    <legend class="text legend_title">Map to EC Comparion Model</legend>
        
                    <%-- ===== Select and map external model =====  --%>
                    <div id="divModelName"><span id="lblModelName">Select model:</span><br /> btnMap
                        <select id="cbModelsList" onchange="onSelectProject(this.value);" style="width:100%;"><% =GetProjectsList()%></select>
                    </div>
<%--                    <div style="text-align:right;float:right">
                        <input type="button" class="button" id="btnMap" name="" value="Connect" onclick="externaldb__name=''; external_attr=''; external_covobj=''; onMap();"/>
                    </div>--%>
                    <br />

                        <%-- ===== select and map attribute, cov objective, and participant =====--%>
                    <div  id="divExternalAttribute" style="margin-top:4px;"><br>Select attribute to import data from:<br />
                        <select id="cbExternalAttr" name="cbExternalAttr" onchange="external_attr=''; onExternalAttrSelect(this.value);" style="width:100%;"><% =ExternalDB_Attribute%></select>
                    </div>
              
                    <div  id="divExternalCovObjective" style="margin-top:4px;">Select covering objective to import data from:<br>
                        <select id="cbExternalCovobj" name="cbExternalCovobj" onchange="external_covobj=''; onExternalCovobjSelect(this.value);" style="width:100%;"><% =ExternalECC_Covobj%></select>
                    </div> 
              
                    <div  id="divExternalUser" style="margin-top:4px;">Select participant to read data from:<br>
                        <select id="cbExternalUser" name="cbExternalUser" onchange="external_user=''; onExternalUserSelect(this.value);" style="width:100%;"><% =ExternalECC_User%></select>
                    </div>  
                </fieldset>
            </div> <%--end divMapToECCprojec--%> <%--'AS/24231k== --%>

                 <%-- ===== Import options =====--%> <%--'AS/12323zb=== --%>
            <div style="margin-top:4px;" id="divImportOptions">
                <fieldset class="legend" style="padding: 1em; text-align: left; width:450px">
                    <legend class="text legend_title">Options</legend> 

                    <div style="margin-left: 20px; padding: 3px 1ex; background: #fafafa; border: 1px solid #e0e0e0;">
                        <input type="checkbox" name="ckCreateNewAlt" id="ckCreateNewAlt" value="1" checked="checked"/><label for="ckCreateNewAlt" id="lblCreateNewAlt">Create new alternatives</label><br/> <%--'AS/15597i added id="lblCreateNewAlt--%>
                        <input type="checkbox" name="ckReplaceAlt" id="ckReplaceAlt" value="<% =CInt(importMapDataReplaceOption.optReplaceExisting)%>" checked="checked"/><label for="ckReplaceAlt">Replace existing values</label><br/> <%-- 'AS/16038 =--%> <%--'AS/15597i changed wording--%>
                    </div>
                                      
                     <%-- ===== Select columns for data interchange (DI) ===== 'AS/15285e=== --%>
                    <div style="margin-top:4px;" id="divDIOptions">Do for:
                        <div style="padding-left: 1ex;" id="divExportColumn">
                            <input type="radio" name="optDIincludeColumns" id="optExportCurrent" onchange="checkMultipleSelection(id);" value="<% =CInt(DataInterchangeInclude.diCurrentColumn)%>" checked="checked" /><label for="optExportCurrent">Current column</label><br />
                            <input type="radio" name="optDIincludeColumns" id="optExportSelected" onchange="checkMultipleSelection(id);" value="<% =CInt(DataInterchangeInclude.diSelectedColumns)%>" /><label for="optExportSelected">Selected mapped columns</label><br />
                            <input type="radio" name="optDIincludeColumns" id="optExportAll"  onchange="checkMultipleSelection(id);" value="<% =CInt(DataInterchangeInclude.diAllMappedColumns)%>" /><label for="optExportAll">All mapped columns</label><br />
                        </div>

                        <div id="divDISelectColumns" style="margin-left: 20px; margin-top: 3px;" hidden="hidden">
                            <div style="margin: 0px; padding: 1ex 2ex; background: #fafafa; border: 1px solid #e0e0e0;">
                                <div style="padding: 1ex 0px;">
                                    <nobr>Select columns to export:</nobr><br />
                                    <select multiple name="cbSelectCol"  id="cbSelectCol" onchange="getSelectedValues(cbSelectCol);" style="width: 75%"><% =GetMappedColumnsList(GetMappedAttributesAndNodes(App.ActiveProject.ProjectManager))%></select>
                                </div>
                            </div>
                        </div> <%--end divDISelectColumns--%>
                    </div> <%--end divDIOptions--%>       
                </fieldset>
            </div> <%-- end divImportOptions--%> 

<%--        <div style="text-align:center">
            <input type="button" class="button" id="btnImport" value="Import Alternatives" style="margin-top: 1em; width: 12em; padding: 3px 1ex;" onclick="onImportAlt();" /> <%--'AS/24189a removed disabled--%>
<%--        </div>--%>

        </td></tr>
    </table></p>
</asp:Content>