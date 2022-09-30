<%@ Page Language="vb" CodeBehind="Notification.aspx.vb" MasterPageFile="~/mpEmpty.Master" Strict="true" Inherits="NotificationPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript"><!--

    $(document).ready(function () {
        $("#btnAccept").dxButton({
            text: "Accept & Continue",
            type: "default",
            width: "200px",
            icon: "fa fa-chevron-circle-right",
            onClick: function (e) {
                document.cookie = "<% =_COOKIE_FEDRAMP_NOTIFICATION %>=1";
                loadURL("<% =JS_SafeString(GetRetPath()) %>");
                return false;
            }
        });
    });

//--></script>
<table border="0" cellpadding="0" cellspacing="0" align="center" style="height:99%; margin: auto auto; padding:0px 1em; min-width:320px; max-width:800px">
<tr valign="top" style="height:25%; min-height:80px">
    <td style="padding:2em 0px 1em 0px">
        <div style="padding-bottom:1ex; border-bottom:2px solid #0051a8;"><img src="<% =ThemePath + _FILE_IMAGES %><% If App.SystemWorkgroup IsNot Nothing AndAlso App.SystemWorkgroup.License IsNot Nothing AndAlso App.SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.RiskEnabled) Then %>RiskionA_Logo.png" width="90" height="77"<% Else %>ecc_logo_320.png" width="320" height="77"<% End if %> border="0" title="ExpertChoice"/></div>
    </td>
</tr>
<tr valign="top">
    <td style="padding:0px 0px 2em 0px;">
        <div style='font-size:1.2em;'>
            <p align="justify">You are accessing a Government information system. Your use of this system may be monitored, recorded, and subject to audit. Unauthorized use of this information system is prohibited and subject to criminal and civil penalties. Your use of this information system indicates consent to monitoring and recording. If you cannot accept these terms and conditions, please close this browser window.</p>
            <%--<input type="button" class="button" name="btnAccept" id="btnAccept" value="Accept & Continue &gt;" onclick="onAccept(); return false;" style="width:200px; margin-top:2em; padding:5px 2px; border-radius:3px;" />--%>
            <div id="btnAccept" style="font-weight:bold; font-size:1.06em; margin-top:1em; padding:4px 1ex"></div>
        </div>
    </td>
</tr>
<tr valign="middle" style="height:1em;">
    <td style="padding:1ex; text-align:left; background: #0051a8; color: #ffffff;">This commercial computer software is delivered with restricted rights to the Government. Use, reproduction or disclosure is subject to the restrictions set forth in the FAR, DFARS, or other license provisions referenced in the contract.</td>
</tr>
</table>
</asp:Content>