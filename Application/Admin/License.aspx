<%@ Page Language="VB" Inherits="LicensePage" title="View License" Codebehind="License.aspx.vb" %>
<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/moment.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.maskedinput.js"></script>
<h4 title="<% =GetCreatedAt()%>"><% =PgTitle%></h4>
<% =GetLicenseDetails()%>

<h6 style="margin:1em; max-width:600px;" runat="server" id="divError" Visible="false" class="error"><% = ParseString(sError) %></h6>

<div style="text-align:center; margin:1ex"><nobr><% If fCanSee Then %>
<% If isEditMode Then%><p align="center" class='text'><% =ResString("lblLicensePsw")%>&nbsp;<asp:TextBox runat="server" ID="tbPsw" AutoPostBack="false" TextMode="Password" Text="" Width="12em" /></p><% End If%>
<asp:Button ID="btnEdit" runat="server" CssClass="button" UseSubmitBehavior="false" Visible="false" Width="12em"/> 
<asp:Button ID="btnSaveChanges" runat="server" CssClass="button" UseSubmitBehavior="true" Visible="false" OnClientClick="return CheckSaveForm();" Width="12em"/> 
<asp:Button ID="btnCancel" runat="server" CssClass="button" UseSubmitBehavior="false" Visible="false" OnClientClick="history.go(-1); return false;" Width="12em" />  
<asp:Button ID="btnEditWkg" runat="server" CssClass="button" UseSubmitBehavior="false" Visible="false" Width="12em" /> 
<asp:Button ID="btnWM" runat="server" CssClass="button" UseSubmitBehavior="false" Visible="false" Width="12em" />&nbsp; <% End If %>
<asp:Button ID="btnBack" runat="server" CssClass="button" UseSubmitBehavior="false" Visible="false" Width="12em" />
</nobr></div>
<script language="javascript" type="text/javascript"> // keep it here due to init check_list on GetLicenseDetails() before

    var check_list = [<% =CheckList %>];

    function GetWM() {
        showLoadingPanel();
        callAjax("", ShowWM, _method_POST, "Loading...", "?id=<% =If(CurrentWorkgroup Is Nothing, "", CurrentWorkgroup.ID.ToString) %>&action=wm_list");
        return false;
    }

    function ShowWM(data) {
        hideLoadingPanel();
        if ((typeof data!="undefined") && data!='' && data.length) { data=eval(data); } else { data=[]; }
        if ((data.length)) {
            var lst = "";
            for (var i=0; i<data.length; i++) {
                lst += "<li><nobr>" + data[i][0] + (data[i][1]!="" && data[i][1]!=data[i][0] ? " (" + data[i][1] + ")" : "") + "</nobr></li>\n";
            }
            lst = "<ul type='square' style='margin-top:0px; margin-bottom:0px;'>\n" + lst + "</ul>";
            if (data.length>15) lst = "<div style='height:15em; overflow:auto'>" + lst + "</div>";
            DevExpress.ui.dialog.alert(lst, "<% =JS_SafeString(ResString("lblWMList")) %>");
        }
        else {
            DevExpress.ui.dialog.alert("<% =JS_SafeString(ResString("errNoWM")) %>", "<% =JS_SafeString(ResString("titleInformation")) %>");
        }
    }

<% If isEditMode Then %>
    var instanceID = "<% =JS_SafeString(App.GetInstanceID_AsString) %>";
    $.mask.definitions['h'] = "[A-Fa-f0-9]";

    $(function() {
        $("#datepicker").datepicker({
            changeMonth: true,
            changeYear: true,
            showButtonPanel: false,
            minDate: "01/01/2010",
            maxDate: "01/01/3000",
            defaultDate: new Date()
        });
        $(".spinner").spinner({
            min:1
        });
        $(".spinner[dis]").spinner("disable");
        $("#opt<% =CInt(ecLicenseParameter.InstanceID) %>").mask("hhhhhhhh-hhhhhhhh",{placeholder:"0"}).attr('autocomplete','off').attr('autocorrect','off').attr('autocapitalize','on');;
    });

    function changeUnlim(id, chk) {
        if (id == <% =CInt(ecLicenseParameter.InstanceID) %>) {
            $("#opt<% =CInt(ecLicenseParameter.InstanceID) %>").prop("disabled", (chk ? "disabled" : ""));
            if (!chk) $("#opt<% =CInt(ecLicenseParameter.InstanceID) %>").focus();
        } else {
            $("#opt" + id).spinner(chk ? "disable" : "enable");
            if (!chk) { if ($("#opt" + id).spinner("value")=="") $("#opt" + id).spinner("value", 1); $("#opt" + id).focus(); }
        }
    }

    function changeUnlimDate(id, chk) {
        eval("theForm.opt" + id + ".disabled = " + (chk ? true : false));
        if ((chk)) $("#divAdd").hide(); else $("#divAdd").show();
        if (!chk) {
            var ds = $("#datepicker").datepicker("getDate");
            if (!(ds)) $("#datepicker").datepicker("setDate", new Date());
            $("#datepicker").datepicker("show");
        }
    }
  
    function AddPeriod(n,p) {
        var ds = $("#datepicker").datepicker("getDate");
        if (!(ds)) ds = new Date();
        var dt = moment(ds);
        if ((ds) && (dt) && (dt.isValid())) {
            dt = moment(dt).add(n,p);
            $("#datepicker").datepicker("setDate", dt.toDate());
        }
        return false;
    }

    function ConfirmRisk(from_risk, id, chk) {
        var c_r = "<% = JS_SafeString(ResString("msgChangeLicenseType2Risk")) %>";
        var r_c = "<% = JS_SafeString(ResString("msgChangeLicenseType2Comparion")) %>";
        var c = "theForm.opt" + id;
        if ((c) && ((from_risk) != (chk)))  {
            window.scrollTo(0, 0);
            var result = DevExpress.ui.dialog.confirm((from_risk ? r_c : c_r), resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (!dialogResult) {
                    var v = eval('theForm.opt" + id + "'); 
                    if (v) { 
                        v.checked = !v.checked; 
                        v.focus(); 
                        CheckRisk(); 
                    }
                }
            });
        }
    }

    function CheckRisk() {
        var r = theForm.opt<% =CInt(ecLicenseParameter.RiskEnabled) %>;
        if ((r)) {
            if (r.checked) { $("#tr<% =CInt(ecLicenseParameter.RiskTreatments)%>").show(); $("#tr<% =CInt(ecLicenseParameter.RiskTreatmentsOptimization)%>").show();  } else { $("#tr<% =CInt(ecLicenseParameter.RiskTreatments)%>").hide(); $("#tr<% =CInt(ecLicenseParameter.RiskTreatmentsOptimization)%>").hide(); }
        }
    }

    function CheckRiskTreatment() {
        var r = theForm.opt<% =CInt(ecLicenseParameter.RiskTreatments)%>;
        if ((r)) $("#tr<% =CInt(ecLicenseParameter.RiskTreatmentsOptimization)%>").prop("disabled", !(r.checked));
    }

    function CheckRA() {
        var r = theForm.opt<% =CInt(ecLicenseParameter.ResourceAlignerEnabled)%>;
        if ((r)) {
            if (r.checked) { $("#tr<% =CInt(ecLicenseParameter.AllowUseGurobi)%>").show(); } else { $("#tr<% =CInt(ecLicenseParameter.AllowUseGurobi)%>").hide(); }
            $("#tr<% =CInt(ecLicenseParameter.AllowUseGurobi)%>").prop("disabled", !(r.checked));
        }
    }

    function insertInstanceID() {
        var u = theForm.u_opt<% =CInt(ecLicenseParameter.InstanceID) %>;
        if ((u)) u.checked = false;
        $("#opt<% =CInt(ecLicenseParameter.InstanceID) %>").prop("disabled", "").val(instanceID).focus();
    }
    
    var need_check = true;

    function CheckSaveForm() {
        if (!need_check) return true;

        window.scrollTo(0, 0);

        var e = theForm.<% =tbPsw.ClientID %>;
        var res = ((e) && e.value!='');
        if (!(res)) {
            var result = DevExpress.ui.dialog.alert("<% =JS_SafeString(ResString("errEmptyLicensePsw")) %>", "<% =JS_SafeString(ResString("titleError")) %>");
            result.done(function () {
                theForm.<% =tbPsw.ClientID %>.focus();
            });
        }

        if ((res)) {
            var inp = theForm.u_opt<% =CInt(ecLicenseParameter.ExpirationDate) %>;
            if ((inp) && !(inp.checked)) {
                var ds = $("#datepicker").val();
                if (!(ds) || !moment(ds, "MM/DD/YYYY").isValid()) {
                    var result = DevExpress.ui.dialog.alert("<% =JS_SafeString(ResString("errWrongExpDate")) %>", "<% =JS_SafeString(ResString("titleError")) %>");
                    result.done(function () {
                        $('#datepicker').focus();
                    });
                    res = false;
                }
            }
        }

        if ((res)) {
            for (var i=0; i<check_list.length; i++) {
                var inp = eval("theForm.opt" + check_list[i]);
                var unlim = eval("theForm.u_opt" + check_list[i]);
                if ((inp) && (unlim) && !(unlim.checked)) {
                    var v = inp.value * 1;
                    if (inp.value=='' || v<1 || inp.value!=v+"" || Math.round(v)!=v) {
                        var result = DevExpress.ui.dialog.alert("<% =JS_SafeString(ResString("errWrongLicParameter")) %>", "<% =JS_SafeString(ResString("titleError")) %>");
                        result.done(function () {
                            eval("theForm.opt" + check_list[i] +".focus();");
                        });
                        res = false;
                        break;
                    }
                }
            }
        }

        if ((res)) {
            var u = theForm.u_opt<% =Cint(ecLicenseParameter.InstanceID) %>;
            if ((u) && !u.checked) {
                var instance = $("#opt<% =CInt(ecLicenseParameter.InstanceID) %>").val();
                if   (instance != instanceID && instance!="" && instanceID!="00000000-00000000") {
                    var result = DevExpress.ui.dialog.confirm("You are trying to save the license with an InstanceID, which is not allowed on that site. Do you really want to continue?", "<% =JS_SafeString(ResString("titleConfirmation")) %>");
                    result.done(function (res) {
                        if (res) {
                            need_check = false; theForm.<% = btnSaveChanges.UniqueID%>.click();
                        } else {
                            $('#opt<% =CInt(ecLicenseParameter.InstanceID) %>').focus();
                        }
                    });
                    res = false;
                }
            }
        }

        return res;
    }
<% End if %>

    function ReloadWorkgroups()
    {
        if ((window.parent) && (typeof (window.parent.ReloadWorkgroupsList) == "function")) {
            window.parent.ReloadWorkgroupsList();
        }
    }

    function initProgressBars() {
        var lst = $(".license_progress");
        if ((lst) && (lst.length)) {
            for (var i=0; i<lst.length; i++) {
                createProgress($(lst[i]), 150, "progress_blue", "progress_red");
            }
        }
    }

    $(document).ready(function () {
        initProgressBars();
    });

</script>
</asp:Content>