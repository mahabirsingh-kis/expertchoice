<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="Pairwise.ascx.vb" Inherits=".Pairwise" %>

<link href="../../CustomCSS/nouislider.css" rel="stylesheet" />
<link href="../../CustomCSS/nouislider.pips.css" rel="stylesheet" />
<link href="../../CustomCSS/nouislider.tooltips.css" rel="stylesheet" />
<script src="../../../Scripts/NoUI/nouislider.min.js"></script>
<script src="../../../Scripts/NoUI/wNumb.js"></script>
<link href="../../../App_Themes/EC2018/jquery-ui.css" rel="stylesheet" />
<style>
    .resizepane {
        overflow: hidden;
    }
    .txtStepTask {
        max-width: calc(100% - 0px);
        max-height: 75px;
        overflow: hidden;
    }
    input.toggle_checkbox {
        position: absolute;
        top: 5px;
        opacity: 0;
        cursor: pointer;
    }
    .active_row .selected_left, .active_row .selected_left1 {
        background: #0059a3;
    }
    .active_row .selected_right, .active_row .selected_right1 {
        background: var(--Primary-Green);
    }
    .rating_area .comment_div {
        position: relative;
        order: 0;
        width: calc(50% - 20px);
        text-align: right;
        display: none;
    }
    .rating_area {
        width: calc(58% - 40px);
        display: initial;
        justify-content: center;
        flex-wrap: wrap;
        align-items: center;
    }
    .rating_area .curvechart_input {
        order: 0;
        display: flex;
        align-items:center; 
    }
    .rating_area .curvechart_input input {
        width: 50px;
        margin: 0 5px;
    }

    .heading_content .page-title-box .txtStepTask {
        -webkit-line-clamp: 3;
        -webkit-box-orient: vertical;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .noUi-handle{
        cursor:e-resize;
    }

</style>
<script>
    $(document).ready(function () {
        setTimeout(function () {
            if ($('.setDivMaxHeight5').height() > $('.setDivMaxHeight7').height())
                $('.setDivMaxHeight7').height($('.setDivMaxHeight5').height());
            else
                $('.setDivMaxHeight5').height($('.setDivMaxHeight7').height());
        }, 200);
    });
</script> 
<div id="divContent" class="removeB divHeader header-padding" runat="server"></div>
<%--<asp:HiddenField ID="hdnpairwise_type" runat="server" Value="" />--%>

