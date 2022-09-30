<%@ Page Language="VB" AutoEventWireup="false" Inherits="Test_Chart" EnableViewState="false"  EnableTheming="false" Theme="" StylesheetTheme="" Codebehind="UC_Chart.aspx.vb" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Regular Utility Curve graph</title>
    <style type="text/css">
        body, .text { font-size: 11px; font-family: Sans-serif, Arial, Helvetica, Verdana;	}
    </style>
</head>
<body><form id="theForm" runat="server"><table border="0" cellspacing="0" cellpadding="0">
<tr><td valign="bottom" class="text"><div>&nbsp;<% =ResString(GetLabel)%></div><asp:PlaceHolder runat="server" ID="Ctrl" /></td></tr>
<tr><td valign="top" align="right" class="text"><% =ResString("lblUnits")%></td></tr><%--<% =ResString("tblSyncValue")%>--%>
</table></form></body>
</html>
