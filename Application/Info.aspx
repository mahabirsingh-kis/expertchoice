<%@ Page Language="vb" CodeBehind="Info.aspx.vb" Inherits="InfoPage" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="PageContent" runat="server">
<script type="text/javascript">

    var help_uid = 0;

    function onSetPage(pgid, e) {
        var res = false;
        if ((e) && e.url == "" && e.help != "" && e.uid != "" && e.position != position_workflow) {
            if ((e.uid) && (e.uid == help_uid)) {
                res = true;
            } else {
                res = initHelp(e.uid);
                help_uid = ((e.uid) ? e.uid : 0);
            }
        }
        return res;
    }

    function initHelp(uid) {
        var res = false;
        var pg = pageByUID(nav_json, uid);
        if ((pg)) {
            var url = pg.help;
            if (url != "") {
                if ((pg.id)) pgID = pg.id;
                showLoadingPanel();
                document.title = document.title = ((pg.title != "") ? pg.title : pg.text);
                if (url[0] != "/") url = getHelpURL(url, false);
                initFrameLoader(document.getElementById("frmHelp"));
                //setTimeout('document.getElementById("frmHelp").location.replace("' + url + '");', 30);
                frmHelp.location.replace(url);   // for avoid to add to browser history used replace() instead of simple assign to .src;
                help_uid = uid;
                res = true;
            }
        } else {
            $("#frmHelp").hide();
            $("#divMdg").show();
        }
        return res;
    }

    function onLoadContent() {
        hideLoadingPanel();
    }

    $(document).ready(function () {
        initHelp(<% =NavigationPageID %>);
    });

</script>
<iframe frameborder="0" name="frmHelp" id="frmHelp" height="99%" width="100%" scrolling="yes" src="" style="border:0px solid #f0f0f0" onload="setTimeout('onLoadContent();', 200);"></iframe><h5 id="divMdg" style="display:none; margin-top:20%">No content</h5>
</asp:Content>