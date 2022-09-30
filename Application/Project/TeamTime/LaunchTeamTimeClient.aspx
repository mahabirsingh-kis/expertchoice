<%@ Page Language="VB" Inherits="LaunchTeamTimeClient" EnableTheming="false" StylesheetTheme="" Theme="" Codebehind="LaunchTeamTimeClient.aspx.vb" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>TeamTime Assistant launcher</title>
    <meta name="robots" content="none" />
    <link rel="icon" href="../../favicon.ico" type="image/ico" />
    <link rel="shortcut icon" href="../../favicon.ico" type="image/ico" />
    <link href="../../App_Themes/ec09/deco.css" type="text/css" rel="stylesheet" />
    <link href="../../App_Themes/jqueryui/jquery-ui.css" type="text/css" rel="stylesheet" />
    <link href="../../App_Themes/ec09/main.css" type="text/css" rel="stylesheet" />
</head>
<body bgcolor="#ffffcc" style='padding:2em 1em'>
    <script type="text/javascript" language="javascript"><!--

        var haveClickOnce = false;

        function OpenChromeExt() {
            window.open('https://chrome.google.com/webstore/detail/meta4-clickonce-launcher/jkncabbipkgbconhaajbapbhokpbgkdc', 'ChromeExt');
            window.close();
            return false;
        }

        function OpenFFExt() {
            window.open('https://addons.mozilla.org/firefox/downloads/latest/fxclickonce/platform:5/addon-337659-latest.xpi?src=dp-btn-primary', 'fxclickonce');
            //            window.open('https://addons.mozilla.org/en-US/firefox/addon/microsoft-net-framework-assist/', 'FFExt');
            // We are using own, which is could be detected from client side
                        //window.open('../../TTA/ClickOnce.xpi', 'FFExt');
            //document.location.href='?action=xpinstall';
            window.close();
            return false;
        }

        function CheckClickOnce() {
            var isFF = (navigator.userAgent.toLowerCase().indexOf('firefox') > -1);
            if (isFF) {
                document.write("<div style='text-align:center; padding-top:1ex;'><input type='button' class='button' value='<% =JS_SafeString(ResString("btnInstalledClickOnce")) %>' style='width:24em;' onclick='TryRunIt();'></div>");
            //    var iframe = document.createElement('iframe');
            //    //iframe.width = 1;
            //    //iframe.height = 1;
            //    try {
            //        iframe.src = 'chrome://fxclickonce/content/fxclickonce.xul';
            //        document.body.appendChild(iframe);
            //        iframe.onload = function () {
            //            haveClickOnce = true;
            //            alert(true);
            //        };
            //        iframe.onerror = function (e) {
            //            haveClickOnce = false;
            //            alert(e);
            //        };
            //    }
            //    catch (e) {
            //        haveClickOnce = false;
            //        alert(e.message);
            //    }
            }
        }

        function CheckMeta4ClickOnce() {
            var isChrome = (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) && (navigator.userAgent.toLowerCase().indexOf('edge') == -1);
            if (isChrome) {
                var img = new Image();
                try {
                    img.src = 'chrome-extension://jkncabbipkgbconhaajbapbhokpbgkdc/images/download.png';
                    img.onload = function () {
                        haveClickOnce = true;
                    };
                    img.onerror = function () {
                        haveClickOnce = false;
                    };
                }
                catch (e) {
                    haveClickOnce = false;
                }
            }
        }

        function TryRunIt() {
            document.location.href = document.location.href + "&action=force";
        }

        function CheckClickOnceExt() {
            if ((haveClickOnce) && document.location.href.indexOf("force") < 1) {
                TryRunIt();
                setTimeout("window.close();", 5000);
            }
            else {
                var l = document.getElementById("lblMsg");
                if ((l)) l.style.display = 'block';
            }
        }

   //--></script>
    <form id="form1" runat="server">
    <h2><asp:Label ID="lblLaunch" runat="server" Text="Loading Client..."></asp:Label></h2>
    <asp:TextBox ID="txtLaunch" runat="server" Visible="false" onFocus="this.select();"></asp:TextBox>
    <br />
    <div id='lblMsg' style='font-weight:normal; display:none'><% =ResString("msgTeamTimeBrowserNotSupported")%></div>
    </form>
</body>
</html>
