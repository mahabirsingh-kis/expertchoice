<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlCheckPassword" Codebehind="ctrlCheckPassword.ascx.vb" %>
<%  If CreateJScriptCode Then%><script type="text/javascript"><!--

    var cp_prefix = "<% =JS_SafeString(PrefixResult) %>";
    var cp_Strengths = [<% =GetStrengths() %>];

    var cp_min_len = <% =Js_SafeNumber(MinimalLength) %>;

    var cp_coef_len = <% =Js_SafeNumber(CoeffLength) %>;
    var cp_coef_case = <% =Js_SafeNumber(CoeffCase) %>;
    var cp_coef_alpha = <% =Js_SafeNumber(CoeffAlphabetical) %>;
    var cp_coef_numeric = <% =Js_SafeNumber(CoeffNumerical) %>;
    var cp_coef_symbols = <% =Js_SafeNumber(CoeffSymbols) %>;

    function CP_ShowMessage(obj_id, msg)
    {
        if (obj_id!="")
        {
            var  div = eval(obj_id);
            if (div) div.innerHTML = msg;
        }
    }
    
    function CP_DetectStrong(val)
    {
        <% if Not ShowOnEmptyValue Then %>if (val=='') return 0;<% End if %>
        var _l = val.length/cp_min_len;
        if (_l>1) _l=1;
        var _n = 0;
        var _s = 0;
        var _uc = 0;
        var _lc = 0;
        for (var i=0; i<val.length; i++)
        {
            var c = val.charAt(i);
            if (c>='0' && c<='9') _n=1; else
            {
                if (c>='A' && c<'Z') _uc++; else
                {
                    if (c>='a' && c<'z') _lc++; else if (typeof c!="undefined" && c!=null) _s=1;
                }    
            }        
        }
        var _c = (_uc && _lc) ? 1 : 0;
        var _a = (_uc || _lc) ? 1 : 0;
        var _total = cp_coef_len * _l + cp_coef_case * _c + cp_coef_alpha * _a + cp_coef_numeric * _n + cp_coef_symbols * _s;
        if (_total>1) _total = 1;
        <% if isDebug then %> window.status = "[" + val + "] COEFF: length: " + Math.round(100*_l)/100 + "; case: " + _uc + "/" + _lc +"=" + _c + "; alphabetical: " + _a + " numerical: " + _n + "; symbols: " + _s +"; total: " + Math.round(100*_total) + "%"; <% End If %>
        return (cp_Strengths.length==1 ? 0 : Math.floor(_total*(cp_Strengths.length-0.9999)));
    }

    function CP_isValid(obj_id)
    {
        if (obj_id=="") return false;
        var obj = eval("theForm." + obj_id);
        if (obj) 
        {
            var res = CP_DetectStrong(obj.value);
            return (res>=<% =StrongStrength %>);
        }
        return false;    
    }

    function CP_CheckValue(obj_id, msg_id)
    {
        if (obj_id=="") return false;
        var obj = eval("theForm." + obj_id);
        if (obj) 
        {
            var s = CP_DetectStrong(obj.value);
            CP_ShowMessage(msg_id, (obj.value=="" ? "[ blank ]" : cp_prefix + cp_Strengths[s]));
//            CP_ShowMessage(msg_id, cp_prefix + cp_Strengths[s]);
        }
    }
    
//-->    
</script><% End if %>