<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EraseAndCommentButtons.ascx.cs" Inherits="AnytimeComparion.Pages.includes.EraseAndCommentButtons" %>

<div class="columns erase-and-comment-wrap">
    <span id="erasebtn" class="" ng-hide="output.pairwise_type == 'ptGraphical' && graphical_switch">
        <span ng-if="output.page_type == 'atNonPWOneAtATime' && output.non_pw_type == 'mtStep' && !output.IsUndefined" class="button tiny tt-button-primary tt-action-btn-clr-j" ng-click="save_StepFunction(-2147483648000, true)"><span class="icon-tt-close icon"></span></span>
        <span ng-if="output.value != 0 && output.page_type == 'atPairwise'" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise" ng-click="save_pairwise(-2147483648000,0);"><span class="icon-tt-close icon"></span></span>
        <span ng-if="output.page_type == 'atNonPWOneAtATime' && (output.non_pw_type == 'mtRegularUtilityCurve' || output.non_pw_type == 'mtAdvancedUtilityCurve') && !output.IsUndefined" class="button tiny tt-button-primary tt-action-btn-clr-j  erase-uc-btn" ng-click="save_utility_curve(-2147483648000, true)"><span class="icon-tt-close icon"></span></span>
    </span>
    <span  ng-if="(output.show_comments == true && output.page_type != 'atPairwise') || (output.show_comments == true && output.page_type=='atPairwise' && output.pairwise_type!='ptGraphical')">
        <a ng-if="isMobile()" href="#" data-reveal-id="tt-c-modal" class="smoothScrollLink" data-link="#comWrap" ng-style="{color:output.comment != '' && output.comment != null ? '#008CBA' : '#b0d5ee'}" ><span class="icon-tt-comments  icon-30"></span></a>
        <a ng-if="!isMobile()" href="#" data-dropdown="comment-single" aria-controls="comment-single" aria-expanded="false"
            data-options="{{output.page_type == 'atNonPWOneAtATime' && (output.non_pw_type == 'mtRegularUtilityCurve' || output.non_pw_type == 'mtAdvancedUtilityCurve' || output.non_pw_type == 'mtStep') ? 'align:top' : 'align:bottom'}}"
            class="smoothScrollLink" data-link="#comWrap" ng-style="{color:output.comment != '' && output.comment != null ? '#008CBA' : '#b0d5ee'}" >
            <span class="icon-tt-comments  icon-30"></span>
        </a>
    </span>
</div>