<%@ Page Language="vb" CodeBehind="Default.aspx.vb" MasterPageFile="~/mpDesktop.Master" Strict="true" Inherits="PinCodePage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript"><!--

    var left = <% =JS_SafeNumber(Timeout) %>;

    function nextTick() {
        if (left < 2) {
            reloadPage();
        } else {
            m = Math.floor(left / 60);
            s = Math.floor(left % 60);
            $("#divPinTimeout").html(m + ":" + ('0' + s).slice(-2));
            if (left < 20) $("#divPinTimeout").css("color", "red");
            setTimeout(nextTick, 1000);
            left--;
        }
    }

    $(document).ready(function () {
        nextTick();
    });

//--></script>
<table border="0" cellpadding="0" cellspacing="0" align="center" class="whole">
<tr valign="middle" align="center">
    <td style="padding:0px 0px 2em 0px;">
        <h6>You can authorize in the external application or device<br /> with current credentials as '<% = SafeFormString(App.ActiveUser.UserEmail) %>'. <p>Please use this PIN code:</p></h6>
        <div style="text-align:center; padding:32px 0px;">
            <div style="display:inline-block; padding-left:24px;">
                <span class="pincode"><% =Pin(0) %></span>
                <span class="pincode"><% =Pin(1) %></span>
                <span class="pincode"><% =Pin(2) %></span>
                <span class="pincode"><% =Pin(3) %></span>
            </div>
            <div style="display:inline-block; margin-left:4px; font-size:14px;"><a href="?action=refresh" class="as_icon"><i class="fa fa-sync-alt" title="Get new PIN code"></i></a></div>
        </div>
        <p style="font-size:13px">This code will be changed in less than <b><span id="divPinTimeout">0:00</span></b></p>
    </td>
</tr>
</table>
</asp:Content>