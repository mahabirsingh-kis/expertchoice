<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="DirectComparison.ascx.vb" Inherits=".DirectComparison" %>

<link href="../../CustomCSS/nouislider.css" rel="stylesheet" />
<link href="../../CustomCSS/nouislider.pips.css" rel="stylesheet" />
<link href="../../CustomCSS/nouislider.tooltips.css" rel="stylesheet" />
<script src="../../../Scripts/NoUI/nouislider.min.js"></script>
<script src="../../../Scripts/NoUI/wNumb.js"></script>
<link href="../../../App_Themes/EC2018/jquery-ui.css" rel="stylesheet" />

<style>
    
    .comment_div .info_tooltip.right_tooltip {
        right: -150px;
        left: auto;
        bottom: 32px;
        border-radius: 6px;
        transition: all .5s;
        transform-origin: bottom right;
        top: 14px;
        padding: 8px 8px;
    }
    .comment_div .info_tooltip.right_tooltip::after {
        right: 52px;
        left: 42%;
        top: -6px;
        bottom: auto;
        transform: rotate(-45deg);
    }

    .ui-slider .ui-slider-handle {
        cursor: e-resize;
    }
    .tooltip_wrapper {
        position: absolute;
        top: 32px;
        min-width: 18rem;
        background: #fff;
        border: 1px solid #ccc;
        padding: 3px 8px;
        box-shadow: 0 0 5px #ccc;
        color: var(--Black);
        font-size: 12px;
        left: -140px;
        z-index: 9;
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

        $('#at_direct_slider').removeClass(' ui-slider ui-corner-all ui-slider-horizontal ui-widget ui-widget-content');
    });
</script>

<div id="divContent" class="removeB divHeader" runat="server"></div>