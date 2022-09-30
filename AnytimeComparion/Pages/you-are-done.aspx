<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="you-are-done.aspx.vb" Inherits="AnytimeComparion.you_are_done" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="text-center set-text-view">
        <h1 class="set-you-are-done-font-style" style="padding-top:30px;">You are done. You may now close this window/tab</h1>
    </div>
    
    <script src="http://code.jquery.com/jquery-latest.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $("#topHeader").hide();
            $("footer.footer").hide();
         });
    </script>
</asp:Content>



