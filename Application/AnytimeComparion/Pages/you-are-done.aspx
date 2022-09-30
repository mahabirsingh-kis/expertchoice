<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/AnytimeComparion/Site.Master" CodeBehind="you-are-done.aspx.vb" Inherits=".you_are_done" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="text-center set-text-view">
        <h1 class="set-you-are-done-font-style" style="padding-top: 30px;">You are done. You may now close this window/tab</h1>
    </div>

    <%--<script src="http://code.jquery.com/jquery-latest.min.js" type="text/javascript"></script>--%>
    <script src="../../Scripts/jquery.min.js"></script>
    <script src="../../Scripts/jquery-ui.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $('#page-topbar').hide();
            $("#topHeader").hide();
            $("footer.footer").hide();
            $(".fixed_info").hide();
            $("#stickyHeader").hide();
           
        });
    </script>
</asp:Content>
