<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="temporaryList.ascx.cs" Inherits="AnytimeComparion.Pages.includes.temporaryList" %>


<div data-equalizer class="large-12 columns">
    <div data-equalizer-watch class="medium-12 columns">
        <h3>Direct Comparison</h3>

        <ul>
            <li><a runat="server" href="~/Judgement?doThis=DirectComparison&view=anytime&style=&design&actions=">Direct Comparison - PM (Anytime)</a></li>
            <li><a runat="server" href="~/Judgement?doThis=DirectComparison&view=teamtime&style=&design&actions=">Direct Comparison - PM (TeamTime)</a></li>
            <li><a runat="server" href="~/Judgement?doThis=DirectComparisonEvaluation&view=anytime&style=&design&actions=evaluate">Direct Comparison - Anytime Evaluation</a></li>
            <li><a runat="server" href="~/Judgement?doThis=DirectComparisonEvaluation&view=teamtime&style=&design&actions=evaluate">Direct Comparison - TeamTime Evaluation</a></li>

            <li><a runat="server" href="~/Judgement?doThis=DirectComparisonPriorities&view=anytime&style=&design&actions=">Direct Comparison > Priorities - PM (Anytime)</a></li>
            <li><a runat="server" href="~/Judgement?doThis=DirectComparisonPriorities&view=teamtime&style=&design&actions=">Direct Comparison > Priorities - PM (TeamTime)</a></li>
            <li><a runat="server" href="~/Judgement?doThis=DirectComparisonPrioritiesEvaluation&view=anytime&style=&design&actions=evaluate">Direct Comparison > Priorities - Anytime</a></li>
            <li><a runat="server" href="~/Judgement?doThis=DirectComparisonPrioritiesEvaluation&view=teamtime&style=&design&actions=evaluate">Direct Comparison > Priorities - TeamTime</a></li>

        </ul>
    </div>
    <div data-equalizer-watch class="medium-12 columns">
        <h3>Pairwise Comparison </h3>
        <ul>
            <li><a runat="server" href="~/Judgement?doThis=PairWiseComparison&view=anytime&style=&design=numerical&actions=">Pairwise Comparison - PM (Anytime-Numerical) wrt </a></li>
            <li><a runat="server" href="~/Judgement?doThis=PairWiseComparison&view=anytime&style=&design=verbal&actions=">Pairwise Comparison - PM (Anytime-Verbal) wrt </a></li>
            <li><a runat="server" href="~/Judgement?doThis=PairWiseComparison&view=teamtime&style=&design=numerical&actions=">Pairwise Comparison - PM (TeamTime-Numerical) wrt </a></li>
            <li><a runat="server" href="~/Judgement?doThis=PairWiseComparison&view=teamtime&style=&design=verbal&actions=">Pairwise Comparison - PM (TeamTime-Verbal) wrt </a></li>
            <li><a runat="server" href="~/Judgement?doThis=PairWiseComparisonEvaluation&view=anytime&style=&design&actions=">Pairwise Comparison - Anytime Evaluation </a></li>
            <li><a runat="server" href="~/Judgement?doThis=PairWiseComparisonEvaluation&view=teamtime&style&design=verbal&actions=evaluate">Pairwise Comparison - TeamTime Evaluation (Verbal) </a></li>
            <li><a runat="server" href="~/Judgement?doThis=PairWiseComparisonEvaluation&view=teamtime&style&design=numerical&actions=evaluate">Pairwise Comparison - TeamTime Evaluation (Numerical) </a></li>
        </ul>
    </div>
    <div data-equalizer-watch class="medium-12 columns">
        <h3>All Pairwise Comparison</h3>
        <ul>
            <li><a runat="server" href="~/Judgement?doThis=AllPairWiseComparison&view=anytime&style=&design=numerical&actions=">All Pairwise Comparison - PM (Anytime) numerical</a></li>
            <li><a runat="server" href="~/Judgement?doThis=AllPairWiseComparison&view=anytime&style=&design=verbal&actions=">All Pairwise Comparison - PM (Anytime) verbal</a></li>
        </ul>
    </div>
    <div data-equalizer-watch class="medium-12 columns">
        <h3>Rating Scale</h3>
        <ul>
            <li><a runat="server" href="~/Judgement?doThis=RatingScale&view=anytime&style=&design&actions=">Rating Scale - PM (Anytime) wrt </a></li>
            <li><a runat="server" href="~/Judgement?doThis=RatingScale&view=teamtime&style=&design&actions=">Rating Scale - PM (Teamtime) wrt </a></li>
            <li><a runat="server" href="~/Judgement?doThis=RatingScaleEvaluation&view=anytime&style=&design&actions=evaluate">Rating Scale - Anytime Evaluation</a></li>
            <li><a runat="server" href="~/Judgement?doThis=RatingScaleEvaluation&view=teamtime&style=&design&actions=evaluate">Rating Scale - TeamTime Evaluation</a></li>
        </ul>
    </div>
</div>
<div data-equalizer class="large-12 columns">
    <div data-equalizer-watch class="medium-12 columns">
        <h3>Utility Curve</h3>
        <ul>
            <li><a runat="server" href="~/Judgement?doThis=UtilityCurve&view=anytime&style=&design&actions=">Utility Curve - PM (Anytime) wrt</a></li>
            <li><a runat="server" href="~/Judgement?doThis=UtilityCurve&view=teamtime&style=&design&actions=">Utility Curve - PM (TeamTime) wrt</a></li>
            <li><a runat="server" href="~/Judgement?doThis=UtilityCurveEvaluation&view=anytime&style=&design&actions=evaluate">Utility Curve - Anytime</a></li>
            <li><a runat="server" href="~/Judgement?doThis=UtilityCurveEvaluation&view=teamtime&style=&design&actions=evaluate">Utility Curve - TeamTime Evaluation</a></li>
        </ul>
    </div>
    <div data-equalizer-watch class="medium-12 columns">
        <h3>Step Funcion</h3>
        <ul>
            <li><a runat="server" href="~/Judgement?doThis=StepFunction&view=anytime&style=&design&actions=">Step Function - PM (Anytime) wrt</a></li>
            <li><a runat="server" href="~/Judgement?doThis=StepFunction&view=teamtime&style=&design&actions=">Step Function - PM (TeamTime) wrt</a></li>
            <li><a runat="server" href="~/Judgement?doThis=StepFunctionEvaluation&view=anytime&style=&design&actions=evaluate">Step Function - Anytime</a></li>
            <li><a runat="server" href="~/Judgement?doThis=StepFunctionEvaluation&view=teamtime&style=&design&actions=evaluate">Step Function - TeamTime Evaluation</a></li>
        </ul>
    </div>
    <div data-equalizer-watch class="medium-12 columns">
        <h3>Judgement Priorities</h3>
        <ul>
            <li><a runat="server" href="~/Judgement?doThis=Priorities&view=anytime&style=&design">Judgement Priorities - PM (Anytime)</a></li>
            <li><a runat="server" href="~/Judgement?doThis=Priorities&view=anytime&style=&design&actions=redo">Judgement Priorities - redo</a></li>
        </ul>
    </div>

    <div data-equalizer-watch class="medium-12 columns">
        <h3>InFormation Page</h3>
        <ul>
            <li><a runat="server" href="~/Judgement?doThis=InformationPage&view=&style=&design">Thank You Page</a></li>
        </ul>
    </div>

</div>
