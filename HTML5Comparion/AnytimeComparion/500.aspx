<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="500.aspx.cs" Inherits="AnytimeComparion._500" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>!Runtime Error</title>
</head>
<body>
    <form id="form1" runat="server">
    <div class="row large-collapse mobile-header">
        <div class="large-2 medium-7 small-6 columns tt-logo text-center " ng-click="show_offset()">
            <a href="<%= ResolveUrl("~/") %>">
                <asp:Image ID="Image1" class="logo-img" runat="server" ImageUrl='/Images/logos/teamtime-logo-transparent-compressed.png'></asp:Image>
            </a>
        </div>
        <div class="medium-5 small-6 columns show-for-medium-down tt-mobile-nav">
            <h1>Something went wrong <br />
            Please try again later or Contact Support</h1>
        </div>
    </div>
    </form>
</body>
</html>
