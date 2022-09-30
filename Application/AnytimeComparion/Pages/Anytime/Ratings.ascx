<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="Ratings.ascx.vb" Inherits=".Ratings" %>


<%--<script src="../../../Scripts/jquery.min.js"></script>--%>

<style type="text/css">
  /*  .rating_scale_data .drop_progress {
        width: 100%;
    }*/
   .row_wrapper.active_table_row {
    margin-top: 10px;
}
    .custom_dropdown {
        position: relative;
        width: 50%;
    }
    .tooltips_group {
        display: flex;
        width: 45%;
        align-items: center;
    }
    .drop_progress {
        width: 55%;
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        flex-wrap: wrap;
    }
    .custom_dropdown .options ul {
        background: var(--White) none repeat scroll 0 0;
        display: none;
        list-style: none;
        padding: 0px 0px;
        position: absolute;
        left: 0px;
        top: 32px;
        width: 100%;
        border: 1px solid var(--Light-Gray);
        width: 100%;
        z-index: 9;
        border-radius: 4px;
        padding: 0 10px;
    }
    .form-check-input{
        cursor:pointer;
    }
</style>

<div id="divContent" class="removeB divHeader" runat="server"></div>
<script>
    $(document).ready(function () {
    });
    <%--function BindHtml() {
        var test = <%=BindReHtml()%>;
    }--%>
</script>
