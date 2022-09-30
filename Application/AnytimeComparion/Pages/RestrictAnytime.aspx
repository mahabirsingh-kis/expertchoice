<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/AnytimeComparion/Site.Master" CodeBehind="RestrictAnytime.aspx.vb" Inherits=".RestrictAnytime" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="start-div">
        <div class="main-div" style="padding: 10% 0 10% 0;">
            <div class="panel-div">
                <div class="set-icon-div">
                    <span class="mdi mdi-information-outline"></span>
                </div>
                <div class="set-text-div ">
                    <span>We are sorry but you can't access AnyTime evaluation while TeamTime meeting is ongoing.
                    </span>
                </div>
            </div>
        </div>
    </div>
    <script src="http://code.jquery.com/jquery-latest.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $("#topHeader").hide();
            $("footer.footer").hide();
        });
    </script>
</asp:Content>
