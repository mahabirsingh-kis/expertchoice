<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Survey.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.Survey" %>
<%@ Register Src="~/Pages/includes/QHicons.ascx" TagPrefix="includes" TagName="QHicons" %> 
<style>
:not(p) div .question-content {
    white-space: pre-line;
}
.survey-content .question-content p {
        display: inline;
    }

</style>
<!-- Start Quick Help -->
<div class="anytime-quick-help-survey">  
        <includes:QHicons runat="server" ID="SurveyPage_QHicons" />
</div>     
<!-- End Quick Help -->
<div class="tt-body" >
    <div class="tt-survery-wrap">
        <div class="row tt-header-nav blue-header" ng-if="!is_html_empty(SurveyPage.Title)">
            <div class="large-12 columns text-center">
                <h1 class="tt-survey-title" style="margin-bottom:0.2rem">{{SurveyPage.Title}}</h1>
            </div>
        </div>
        <div class="row">
            <div class="large-12 columns survey-div" style="margin-bottom: 80px;">                
                <ol class="row tt-survey-item-list-wrap " ng-if="SurveyPage" style="overflow-x: hidden;list-style:none">
                    <li class="tt-survey-item tt-survey-item-{{$index}}" ng-repeat="question in SurveyPage.Questions">
                        <div class="large-12 columns survey-content" style="top:0px">
                            <div class="left question-content" ng-if="question.Type != 0" ng-bind-html="getHtml(QuestionNumbering[$index] + (QuestionNumbering[$index] ? '. ' : '') + (question.Text))"></div>
                            <div class="left" ng-if="question.Type == 0" ng-bind-html="getHtml(question.Text)"></div>
                             <span class="left" ng-if="!question.AllowSkip"><sup>Required</sup></span>
                             <div class="row">
                                <span ng-if="SurveyAnswers[$index][0] != '' && SurveyAnswers[$index][0] != ':' && answer[$index] != null && checkChbAnswer(SurveyAnswers[$index][0], question, [4, 12, 13 ].indexOf(question.Type) > -1 )" class="icon-tt-check-circle survey-check" ></span>
                            </div>
                                   
                            <%-- question options  --%>
                            <div ng-if="question.Type == 1">
                                <input type="text" class="columns question-1" ng-model="answer[$index]" ng-init="answer[$index] = SurveyAnswers[$index][0]; ssss(answer[$index])" ng-blur="change_respondentAnswer($index, answer[$index],question.Type)" ></input>
                            </div>
                            
                            <div ng-if="question.Type == 2">
                                <textarea class="columns" ng-model="answer[$index]" ng-init="answer[$index] = SurveyAnswers[$index][0]; " ng-blur="change_respondentAnswer($index, answer[$index], question.Type)" ng-if="question.Type == 2"></textarea>
                            </div>
                            
                            <div ng-if="question.Type == 3 ">
                                <label ng-repeat="variant in question.Variants" class="columns"> 
                                    <input ng-model="answer[$parent.$index][0]" type="radio" class="left" 
                                        ng-change="change_respondentAnswer($parent.$index, answer[$parent.$index], 3)" 
                                        ng-value="variant.Text" 
                                        ng-init="answer[$parent.$index][0] = text_is_equal(SurveyAnswers[$parent.$index][0], variant.Text, false)"
                                         />{{variant.Text }}<%--<span ng-bind-html="$last && variant.Type != 2 ? '<br />' : ''"></span>--%>
                                    <input ng-if="variant.Type == 2" style="display:inline; width:150px;height:30px;" type="text" 
                                        ng-disabled="answer[$parent.$parent.$index][0] != 'Other:'"
                                        ng-model="answer[$parent.$parent.$index][1]"
                                        ng-init="answer[$parent.$parent.$index][1] = get_othervalue(SurveyAnswers[$parent.$parent.$index][0], question.Type, variant.Text, $parent.$parent.$index)"
                                        ng-blur="change_otherAnswer(question.Type, $parent.$parent.$index, answer[$parent.$parent.$index][1], variant.Text)" />
                                    <button ng-if="$last" type="button" class="button tiny tt-button-primary" ng-click="answer[$parent.$parent.$index] = ''; change_respondentAnswer($parent.$parent.$index, '', question.Type)">Clear</button>
                                </label>
                    
                                
                            </div>
                            <div ng-if="question.Type == 4" >
                                <sup ng-if="question.MinSelectedVariants > 0 || question.MaxSelectedVariants > 0">
                                    (Select {{question.MinSelectedVariants > 0 ? 'minimum ' + question.MinSelectedVariants + ' item(s)' : ''}} 
                                            {{question.MinSelectedVariants > 0 && question.MaxSelectedVariants > 0 ? 'and' : ''}} 
                                            {{question.MaxSelectedVariants > 0 ? 'maximum ' + question.MaxSelectedVariants + ' item(s)' : ''}})
                                </sup>
                                <label ng-repeat="variant in question.Variants" class="columns" ng-switch on="variant.Type" > 
                                    <input ng-switch-when="0" ng-model="answer[$parent.$parent.$index][$parent.$index]" name="c1" type="checkbox" class="left" 
                                        ng-change="change_respondentAnswer($parent.$parent.$index, answer[$parent.$parent.$index][$parent.$index], question.Type, true, variant.Text, question)" 
                                        ng-init="answer[$parent.$parent.$index][$parent.$index] = text_is_equal(SurveyAnswers[$parent.$parent.$index][0], variant.Text, true, question)" >
                                    {{variant.Text}}
                                    <input ng-switch-when="2" style="display:inline; width:150px;height:30px;" type="text" ng-model="answer[$parent.$parent.$index][$parent.$index]"
                                        ng-init="answer[$parent.$parent.$index][$parent.$index] = get_othervalue(SurveyAnswers[$parent.$parent.$index][0], question.Type, variant.Text, $parent.$parent.$index)"
                                        ng-blur="change_otherAnswer(question.Type, $parent.$parent.$index, answer[$parent.$parent.$index][$parent.$index], variant.Text)" />                                            
                                </label>
                            </div>
                            <div ng-if="question.Type == 5" >
                                <select  ng-model="answer[$index][0]"
                                    ng-init="answer[$index][0] = get_combobox_value(question.Variants, SurveyAnswers[$index][0])"
                                    ng-options="variant.VariantValue for variant in question.Variants | filter : optionsFilter" 
                                    ng-change="change_respondentAnswer($index, answer[$index][0].VariantValue, question.Type)" >
                                    <option style="display:none" value="">--Select One--</option>
                                </select> <button type="button" class="button tiny tt-button-primary" ng-click="answer[$index] = ''; change_respondentAnswer($index, '', question.Type)">Clear</button>
                                <div ng-if="question.Variants[question.Variants.length - 1].Type == 2">
                                    Other: <input style="display:inline; width:150px;height:30px;" type="text" ng-model="answer[$index][1]"
                                    ng-init="answer[$index][1] = (SurveyAnswers[$index][0]).indexOf(':') > -1 ? get_othervalue(SurveyAnswers[$index][0], question.Type) : ''"
                                    ng-blur="change_otherAnswer(question.Type, $index, answer[$index][1])" />
                                </div>
                            </div>
                            <div ng-if="question.Type == 12">
                                <sup ng-if="question.MinSelectedVariants > 0 || question.MaxSelectedVariants > 0">
                                    (Select {{question.MinSelectedVariants > 0 ? 'minimum ' + question.MinSelectedVariants + ' item(s)' : ''}} 
                                            {{question.MinSelectedVariants > 0 && question.MaxSelectedVariants > 0 ? 'and' : ''}} 
                                            {{question.MaxSelectedVariants > 0 ? 'maximum ' + question.MaxSelectedVariants + ' item(s)' : ''}})
                                </sup>
                                <label ng-repeat="objective in output.PipeParameters.objectivelist" class="columns">
                                    <input ng-model="answer[$parent.$index][$index]" type="checkbox" class="left" 
                                        ng-change="change_respondentAnswer($parent.$index, answer[$parent.$index][$index], question.Type, false, $index, question)" 
                                        ng-init="answer[$parent.$index][$index] = objective.isDisabled; change_respondentAnswer($parent.$index, answer[$parent.$index][$index], question.Type, false, $index, question)"
                                        ng-disabled="$index == 1"
                                        ng-style="{'margin-left': get_level_spaces(objective.level)}">
                                            
                                    {{objective.NodeName}}
                                </label>
                            </div>
                            <div ng-if="question.Type == 13">
                                <sup ng-if="question.MinSelectedVariants > 0 || question.MaxSelectedVariants > 0">
                                    (Select {{question.MinSelectedVariants > 0 ? 'minimum ' + question.MinSelectedVariants + ' item(s)' : ''}} 
                                            {{question.MinSelectedVariants > 0 && question.MaxSelectedVariants > 0 ? 'and' : ''}} 
                                            {{question.MaxSelectedVariants > 0 ? 'maximum ' + question.MaxSelectedVariants + ' item(s)' : ''}})
                                </sup>
                                <label ng-repeat="alternative in output.PipeParameters.alternativelist" class="columns" > 
                                    <input ng-model="answer[$parent.$index][$index]" type="checkbox" class="left" 
                                        ng-change="change_respondentAnswer($parent.$index, answer[$parent.$index][$index], question.Type, false, $index, question)" 
                                        ng-init="answer[$parent.$index][$index] = alternative.isDisabled; change_respondentAnswer($parent.$index, answer[$parent.$index][$index], question.Type, false, $index, question)"
                                        ng-style="{'margin-left': $index != 0 ? '10px':''}" >
                                    {{alternative.NodeName}}
                                            
                                </label>
                            </div>
                            <div ng-if="question.Type == 14">
                                <label>
                                    (Enter a valid number)
                                </label>
                                <input type="text" class="columns question-1" ng-model="answer[$index]" ng-init="answer[$index] = SurveyAnswers[$index][0]; ssss(answer[$index])" ng-blur="change_respondentAnswer($index, answer[$index],question.Type)" ></input>
                            </div>
                            <div ng-if="question.Type == 15">
                                <label>
                                    (Enter multiple numbers separated by a new line)
                                </label>
                                <textarea class="columns" ng-model="answer[$index]" ng-init="answer[$index] = SurveyAnswers[$index][0]; " ng-blur="change_respondentAnswer($index, answer[$index], question.Type)" ng-if="question.Type == 15"></textarea>
                            </div>
                        </div>
                    </li>
    
                </ol>
               
            </div>
        </div>

    </div>
</div>
 
<style>
     
    /* hide this for suvey only */
    .push{
        display:none;
    }

    
</style>
    
<script>
    $(window).on("resize", function () {
        getSurveyListHeight();
    });
    function getSurveyListHeight() {
        var bodyWidth = $("body").width();
        ////console.log("bodyWidth: " + bodyWidth);

        if (bodyWidth < 1024)
            $(".tt-survey-item-list-wrap").height("100%");
        else {
            var headerHeight = $(".tt-header").height();
            var headerNavHeight = $(".tt-header-nav.blue-header").height();
            var footerHeight = $(".tt-footer-wrap").height() + $(".footer-pagination-wrap").height() + $(".footer-content").height();
            var bodyHeight = $("body").height();
            var surveyHeight = bodyHeight - (headerHeight + headerNavHeight + footerHeight);
            $(".tt-survey-item-list-wrap").height(surveyHeight - 50);

            ////console.log("headerHeight: " + headerHeight + ", headerNavHeight: " + headerNavHeight);
            ////console.log("footerHeight: " + footerHeight + ", bodyHeight: " + bodyHeight);
            ////console.log("surveyHeight: " + surveyHeight);
       }
    }
    setTimeout(function () {
        getSurveyListHeight();
    }, 500);


</script>

