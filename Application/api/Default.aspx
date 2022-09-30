<%@ Page Language="VB" Inherits="WebAPIHelpPage" EnableTheming="false" Theme="" Codebehind="Default.aspx.vb" %>
<!DOCTYPE html>
<html lang="en">
  <head>
    <link rel="stylesheet" type="text/css" href="./swagger-ui.css" />
    <link rel="icon" type="image/png" href="./favicon-32x32.png" sizes="32x32" />
    <link rel="icon" type="image/png" href="./favicon-16x16.png" sizes="16x16" />
    <style>
      html
      {
        box-sizing: border-box;
        overflow: -moz-scrollbars-vertical;
        overflow-y: scroll;
      }

      *,
      *:before,
      *:after
      {
        box-sizing: inherit;
      }

      body
      {
        margin:0;
        background: #fafafa;
        font-family: Arial, Helvetica, sans-serif;
      }

     .swagger-ui .topbar {
     	display: none;
     }

     #divPanel {
         text-align: center;
         padding: 1em 2em;
         border: 1px solid #e0e0e0;
         border-radius: 4px;
         background: #dff3ff;
         display: table;
         margin: 1ex auto;
         max-width: 600px;
     }

     #btnGenerate {
         float: right;
         display: inline-block;
         position: fixed;
         top: 1ex;
         right: 1ex;
         padding: 4px 1em;
         text-align: center;
         border: 1px solid #0390ce;
         border-radius: 4px;
         font-weight: bold;
         font-size: 0.8rem;
         background: #00a8f2;
         color: #ffffff;
         cursor: pointer;
     }

    #btnGenerate:hover {
         background: #0090cf;
    }

    </style>
    <script type="text/javascript" src="../Scripts/jquery.min.js" charset="UTF-8"></script>
    <script type="text/javascript" src="../Scripts/misc.js" charset="UTF-8"></script>
    <script type="text/javascript" src="../Scripts/masterpage.js" charset="UTF-8"></script>
    <% If Not isIE(Request) Then %><script type="text/javascript" src="./swagger-ui-bundle.js" charset="UTF-8"></script>
    <script type="text/javascript" src="./swagger-ui-standalone-preset.js" charset="UTF-8"></script><% End if %>
</head>
<body>
    <% If (Request IsNot Nothing AndAlso Request.IsLocal) OrElse (App.isAuthorized AndAlso App.ActiveUser.CannotBeDeleted) Then %><div id='btnGenerate' onclick='generateOpenAPI();'>Scan methods...</div><% End If %>
    <div id="divPanel" style="display:none;"></div>
    <div id="swagger-ui"><% If isIE(Request) Then  %><h5 style="text-align:center; margin-top:30%; height:2em;">Sorry, but Internet Explorer is not supported.</h5><% End If%></div>
    <script type="text/javascript">

        var has_openapi = <% =Bool2JS(MyComputer.FileSystem.FileExists(_FILE_API + _FILE_OPEN_API_JSON)) %>;

        function generateOpenAPI() {
            $("#divPanel").show().html("<b>Generate OpenAPI documentation</b><br /><br />Please wait..");
            $("#swagger-ui").hide();
            callAPI("help/openapi/?action=generate&t=" + Math.random(), {}, onGenerateOpenAPI);
        }

        function onGenerateOpenAPI(res) {
            if (isValidReply(res)) {
                $("#divPanel").hide().html("");
                $("#swagger-ui").show();
                loadOpenAPI();
            } else {
                $("#divPanel").html("<b>Error on generate documentation</b>.<p>" + res.Message);
            }
        }

        function loadOpenAPI() {<% If Not isIE(Request) Then %>
            const ui = SwaggerUIBundle({
                url: "<% =_FILE_OPEN_API_JSON %>",
              dom_id: '#swagger-ui',
              deepLinking: true,
              presets: [
                SwaggerUIBundle.presets.apis,
                SwaggerUIStandalonePreset
              ],
              plugins: [
                SwaggerUIBundle.plugins.DownloadUrl
              ],
              layout: "StandaloneLayout"
          })
          window.ui = ui;<% End if %>
        }

        window.onload = function() {
            if ((has_openapi)) {
                loadOpenAPI();
            } else {
                generateOpenAPI();
            }
        }

    </script>
</body>
</html>