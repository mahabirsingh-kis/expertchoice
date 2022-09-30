<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StepFunction.ascx.cs" Inherits="AnytimeComparion.Pages.TeamTimeTest.StepFunction" %>
<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %>
<%@ Register Src="~/Pages/includes/LeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="LeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTLeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTLeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
<link href="/Content/stylesheets/stepfunction.css" rel="stylesheet" />
<div ng-init="init()" class="ds-div-trigger">  
   
    <div class="large-12 columns questionsHeader">
        <includes:QuestionHeader runat="server" id="QuestionHeader" /> 
    </div>

    
    <div class="columns tt-question-choices questionsWrap">
        <div class="medium-4 columns">
             <includes:LeftNodeInfoDoc runat="server" id="LeftNodeInfoDoc" />     
        </div>
        <div class="medium-4 columns">
            <includes:ParentNodeInfoDoc runat="server" id="ParentNodeInfoDoc" />    
        </div>
        <div class="medium-4 columns">
           <includes:WRTLeftNodeInfoDoc runat="server" id="WRTLeftNodeInfoDoc" />
        </div>
    </div>

<div class="large-12 columns disable-when-pause step-function">
    <div class="tt-judgements-item large-12 columns tt-rating-scale-wrap step-function">
        <div class="row large-uncollapse small-collapse tt-j-content tt-rating-scale-wrap">
            <div class="row large-uncollapse small-collapse">
              <div class="medium-12 large-5 columns tt-step-function-wrap small-centered text-center"  >
                    <%--<a class="tt-toggler close cancel-tg-sf hide" data-toggler="tg-sf">Edit Data</a>
                        <div class="large-12 columns small-centered text-center hide">
                            <div class="row tt-edit-steps-wrap text-left tg-sf hide">
                            <div class="columns">Scale Name: </div>
                            <div class="medium-7 columns">
                                <input type="text" placeholder="Name of scale">
                            </div>
                            <div class="medium-5 columns">
                                <label class="item-list">
                                    <input type="checkbox" class=""> <span class="">Make default name.</span>
                                </label>
                            </div>
                            <div class="row">
                                <div class="large-12 columns">
                                    <div class="large-3 columns">
                                        <label class="item-list">
                                            <input type="checkbox" name="linearToggle" ng-checked="stepfunction_piecewise == 1"> <span class="">Piecewise Linear</span><a data-tooltip aria-haspopup="true" class="has-tip" title="Disables step lines and curves"><span class="icon-tt-question-circle help-icon size20"></span></a>
                                        </label>
                                    </div>  
                                    <div class="large-3 columns">
                                        <div class="small-12 columns">
                                            <label class="item-list ">
                                                <input type="checkbox" name="stepsToggle" class="stepsToggle"> <span class="">Add Steps Value</span><a data-tooltip aria-haspopup="true" class="has-tip" title="Number of steps while dragging the handle bar"><span class="icon-tt-question-circle help-icon size20"></span></a>
                                            </label>
                                        </div>
                                        <div class="small-12 columns steps-value-wrap">
                                            <input type="number" name="stepsValue" placeholder="ex: 5" class="stepsValue"> 
                                        </div>
                                    </div>
                                    <div class="large-3 columns">
                                        <div class="small-12 columns">
                                            <label class="item-list">
                                                <input type="checkbox" name="curvesToggle"> <span class="">Activate Curves</span><a data-tooltip aria-haspopup="true" class="has-tip" title="Add curves on edges to make lines smooth"><span class="icon-tt-question-circle help-icon size20"></span></a>
                                            </label>
                                        </div>
                                        <!--<div class="small-12 columns curves-value-wrap">
                                            <div id="curveSlider" class="columns"></div>
                                            <input type="number" name="curvesValue" placeholder="0-100 or negative" maxlength="100" class="curvesValue left"> <a data-tooltip aria-haspopup="true" class="has-tip right" title="0 to 100 or Negative -1 to -100"><span class="icon-tt-question-circle help-icon size20"></span></a> 
                                        </div>-->
                                    </div>
                                    <div class="large-3 columns">
                                        <label class="item-list">
                                            <input type="checkbox" name="dotsToggle"> <span class="">Toggle Dots</span><a data-tooltip aria-haspopup="true" class="has-tip" title="Show / Hide dots in the graph"><span class="icon-tt-question-circle help-icon size20"></span></a>
                                        </label>
                                    </div>
                                </div>
                            </div>
                            <div class="row panel">
                                <div class="small-2 columns">
                                    <a data-tooltip aria-haspopup="true" class="has-tip tt-toggler switch-high-low nonInverted" data-toggler="tg-class-checkbox" data-class="icon-tt-high-low icon-tt-low-high" title="Switch from High to Low or viceversa"><span class="icon-tt-low-high icon"></span>
                                        <input type="checkbox" name="highToLow" readonly class="hide">
                                    </a>
                                </div>
                                                    
                                <div class="small-10 columns curves-value-wrap">
                                    <div class="row collapse">
                                        <div class="small-6 columns">Negative</div>
                                        <div class="small-6 columns text-right"> Positive</div>
                                                            
                                    </div>
                                    <div id="curveSlider" class="columns"></div>
                                    <input type="hidden" name="curvesValue" placeholder="0-100 or negative" maxlength="100" class="curvesValue"> 
<!--                                                        <a data-tooltip aria-haspopup="true" class="has-tip right" title="0 to 100 or Negative -1 to -100"><span class="icon-tt-question-circle help-icon size20"></span></a> -->
                                </div>
                                                    
                                <!--<a data-tooltip aria-haspopup="true" class="has-tip tt-toggler switch-high-low-inverted inverted hide" data-toggler="tg-class-checkbox-inverted" data-class="icon-tt-inverted-high-low icon-tt-inverted-low-high" title="Inverted High to Low or viceversa"><span class="icon-tt-inverted-low-high icon"></span>
                                </a>-->
                                                    
                                                    
                            </div>
                                                    
                            <div class="large-12 columns non-curve-content">
                                <table>
                                    <thead>
<!--                                                            <th></th>-->
<!--                                                            <th>Name</th>-->
                                        <th>Priority</th>
                                        <th>Lower Bound Data</th>
                                    </thead>
                                    <tbody class="data-steps-content">
                                        <tr>
<!--                                                                <td>A</td>-->
<!--                                                                <td><input type="text" name="" class="inline-edit"></td>-->
                                            <td>
                                                <input type="text" name="lowerBound" class="inline-edit" placeholder="ex: 0,10,20,40,50, 80,90,100" value="0,10,20,30, 40,50,60,70, 80,90,100">
                                                <!--<div id="sliderCurveHandle" class="columns"></div>
                                                <hr>
                                                <input type="text" id="amount" readonly style="border:0; color:#f6931f; font-weight:bold;">-->
                                            </td>
                                            <td><input type="text" name="priority" class="inline-edit" placeholder="ex: 0,10,20,40,50, 80,90,100" value="0,10,20,30, 40,50,60,70, 80,90,100"></td>
                                        </tr>
                                    </tbody>
                                </table>

                                                    
<!--                                                    <a data-tooltip aria-haspopup="true" class="has-tip right add-data-steps-btn" title="Add data row"><span class="icon-tt-plus-circle icon"></span></a>-->
                            </div>
                            <div class="row collapse">
                                <hr>
                                <div class="medium-9 columns">
                                        <button type="button" class="button tiny tt-button-icond-left tt-button-primary info">
                                            <span class="icon-tt-copy icon"></span>
                                            <span class="text">Copy to clipboard</span>
                                        </button>
                                        <button type="button" class="button tiny tt-button-icond-left tt-button-primary info">
                                            <span class="icon-tt-clipboard icon"></span>
                                            <span class="text">Paste to clipboard</span>
                                        </button>

                                        <button type="button" class="button tiny tt-button-icond-left tt-button-primary info">
                                            <span class="icon-tt-clipboard icon"></span>
                                            <span class="text">Asset Priorities</span>
                                        </button>

                                </div>
                                <div class="medium-3 columns text-right">
                                    <!--<button type="button" class="button tiny tt-button-icond-left tt-button-primary success tt-save-steps-btn">
                                        <span class="icon-tt-save icon"></span>
                                        <span class="text">Save</span>
                                    </button>-->
                                    <button type="button" class="button tiny tt-button-icond-left tt-button-primary success cancel-sf">
                                        <span class="icon-tt-check-circle icon"></span>
                                        <span class="text">OK</span>
                                    </button>

                                </div>

                            </div>
                            </div>
                        </div>--%>
<!------------------------------------>
<!-------- Data Graph Toggle --------->
<!------------------------------------>
<ul class="accordion teamtime-graph-accordion" data-accordion>
  <li class="accordion-navigation">
    <a href="#graphpanel" class="hide-for-large-up">Hide Graph/Show Graph</a>
    <div id="graphpanel" class="content active">
       <div class="large-12 columns small-centered text-center" >
         <canvas id="sFunctionCanvas" class="steps-functions" width="" height="">Your browser does not support HTML5 Canvas</canvas>
            <%--<div id="stepsFunctionSlider" data-sliderval="10" data-disable="" onclick="" class="slider columns tt-steps-slider dsBarClr"></div>--%>
             <input id="steps-functionInput" type="number" step="0.01" name="" value="{{ current_user[3] == -2147483648000 ? '' : current_user[3] }}" class="steps-slider-dragger"  ng-model="current_user[3]" ng-change="stepfunction_save(current_user[3], current_user[0], comment_txt[1], 0)">
       </div>
       <div class="large-12 columns small-centered text-center">
          <div id="erasebtn" class="large-8 columns large-centered text-center" ng-if="current_user > -9">
              <a id="" ng-click="stepfunction_save('-2147483648000', current_user[0], comment_txt[1], 0)" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise"><span class="icon-tt-close icon"></span></a>
          </div>
      </div>
    </div>
  </li>
</ul>

<!------------------------------------>
<!------ End Data Graph Toggle ------->
<!------------------------------------>
              </div>
              <div class="small-12 medium-12 large-7 columns tt-j-others-result small-centered text-center no-more-padding">
                <!-------------------------------------------->
                <!-- Start Viewing Options & Pagination Row -->   
                <!-------------------------------------------->                    
                    <div class="tt-paginate-users">
                                        <div class="row collapse teamtime-pagination-row teamtime-data-pagination">
                                            <div ng-if="output.isPM" class="small-4 medium-3 large-3 columns">                                           
                                               <span class="sort-label show-for-large-up">Show</span> <select ng-model="user_display" ng-change="set_user_display(user_display.value)" ng-options="obj.text for obj in user_display_options track by obj.value" class="toggle-user-display toggle-name-email">
                                               </select>
                                            </div>
                                            <div class="small-7 medium-5 large-6 columns text-center user-pagination pagination-centered" ng-if="show_participant">
                                                <ul  class="pagination verbal-paginate verbal" ng-if="users_list.length > 5"><!--ng-repeat="page in pagination_pages"-->
                                                    <li ng-repeat="page in pagination_pages" ng-hide="(page > pagination_CurrentPage || page < pagination_CurrentPage) && page != 'all' && page != 'select' && page != '<' && page != '>'">
                                                    <a class="arrow" data-arrow="left" ng-click="pagination_save('decrement', pagination_CurrentPage - 1)" ng-if="page == '<'">&laquo;</a>
                                                    <a class="{{pagination_CurrentPage == page ? 'current' : ''}} paginate-users" ng-if="page == pagination_CurrentPage && page != '<' && page != '>' && page != 'all' && page != 'select'">
                                                        {{page}}
                                                    </a>
                                                    <select class="show-smart users-verbal" ng-model="pagination_NoOfUsers" ng-options="item for item in pagination_select_list" ng-change="pagination_save('option' , pagination_NoOfUsers)" ng-if="page == 'select'">
                                                    </select>
                                                    <a ng-click="pagination_save('all', page)" class="paginate-all {{pagination_CurrentPage == 0 ? 'current' : ''}}" ng-if="page == 'all'">{{page}}</a>
                                                    <a ng-click="pagination_save('increment', pagination_CurrentPage + 1)" class="arrow" data-arrow="right" ng-if="page == '>'" >&raquo;</a>
                                                </li>
                                                </ul>
                                          </div>
                                          <div class="small-1 medium-3 large-3 columns text-right right">                                   
                                                    <a class="" id="idr" data-slide-toggle=".participant-div" data-slide-toggle-duration="400" ng-click="update('', 'individual', '', baseurl, '', '', '')">
                                                        <span class="iconstyle icon-tt-minus-square icon" ng-if="show_participant"></span>
                                                        <span  ng-click="pagination_save('click', pagination_CurrentPage)" class="iconstyle icon-tt-plus-square icon" ng-if="show_participant == false"></span>
                                                        <div class="hide-for-small">
                                                            <span  class="togname text" ng-if="show_participant">Hide Individual</span>
                                                            <span  class="togname text"  ng-click="pagination_save('click', pagination_CurrentPage)" ng-if="show_participant == false">Show Individual</span>
                                                        </div>
                                                    </a>
                                    
                                            </div>
                                        </div>                           
                                      </div>
                <!-------------------------------------------->
                <!-- End Viewing Options & Pagination Row -->   
                <!-------------------------------------------->

                  <div class="row large-uncollapse small-collapse">
                      <div>
                          <ul class="tt-drag-slider-wrap teamtime-data-rows-wrap tt-step-function-wrap participants-list">
                              
                              <li  ng-init="user_index=$index" ng-repeat="user in users_list | startTo : pagination_NoOfUsers : pagination_CurrentPage  | limitTo:pagination_CurrentPage == 0 ? users_list.length : pagination_NoOfUsers "  ng-if="check_hide_offline(user[3]) && (check($index) || (current_user[0] == user[1].toLowerCase()))">
                                  <div ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div' "
                                      class=" tt-drag-slider-item teamtime-data-row" data-index="{{$index}}" >
                                      <div class="small-12 large-5 columns teamtime-user-column">
                                          <span class="tt-user-status online online" ng-hide="user[3] == 0">&#9679;</span>
                                          <div class=" user_{{$index}} userdisplay{{ user[0] }} tt-emailOnly tt-nameOnly tt-nameAndEmail tg-en">
                                            <span  ng-style="{color:get_user_color(user[4])}" ng-if="TTOptions.isAnonymous == 1">{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : 'Person ' + $index}}</span>
                                            <span  ng-style="{color:get_user_color(user[4])}" ng-if="TTOptions.isAnonymous == 0" >{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : (user_display.value==0) ? user[2] : (user_display.value==1) ? user[1] : user[2] + '(' + user[1] + ')'}}</span>
                                               <span ng-if="user[4] == -1" ng-style="{color:get_user_color(user[4])}" >(View Only)</span>
                                          </div>
                                      </div>
                                      <div ng-style="{opacity:get_opacity(user[4])}" class="small-4 large-2 columns tg-ratings teamtime-user-rating" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()"> 
                                            <input 
                                                 ng-readonly="user[4]!=0"
                                                 ng-disabled="(meeting_owner != current_user[0] && current_user[0] != user[1].toLowerCase()) || (meeting_owner == current_user[0] && user[4]==-2)" 
                                                type="number" step="0.01" class="input-curve {{current_user[0].toLowerCase() == user[1].toLowerCase() ? '' : 'judgment-results-div'}}"
                                                 style="cursor: auto;" ng-model="stepfunction_input[$index]" ng-change="stepfunction_save(stepfunction_input[$index], user[1], user[6], $index);">
                                       </div>
                                      <div ng-style="{opacity:get_opacity(user[4])}" class="small-7 large-4 columns text-right teamtime-user-judgment" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()"
                                            ng-init="calculate_priority(user[5] < lowest_value ? -2147483648000 : user[5], $index)">
                                            <div class="small-4 large-3 columns text-center"> 
                                                <span class="curve-priority-{{user[0]}} priority-sign {{current_user[0].toLowerCase() == user[1].toLowerCase() ? '' : 'judgment-results-div'}}">{{stepfunction_priority[$index]}}</span>
                                            </div>
                                            <div class="small-8 large-9 columns direc-comparison-wrap" >
                                                <div class="curve-bar-{{user[0]}} dc-group-result-bar progress-sign {{current_user[0].toLowerCase() == user[1].toLowerCase() ? '' : 'judgment-results-div'}}" ng-style="stepfunction_pwidth[$index]"></div>
                                            </div>            
                                      </div>
                                     <div class="small-1 medium-1 columns large-text-right small-text-center reset-result-btn-wrap" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()">
                                          <a title="" ng-if="(meeting_owner == current_user[0] || current_user[0] == user[1]) && user[4] == 0 && user[5] != -2147483648000" 
                                              ng-click="stepfunction_save(-2147483648000, user[1], user[6], $index)"><span  id="reset_{{$index}}" data-index="{{$index}}" data-mtype="mtRatings" data-email="{{ user[1] }}" class="icon-tt-close reset-btn teamtime-page"></span></a>
                                    </div>
                                  </div>
                              </li>          
                          </ul>
                      </div>
                      <div class="the-clear-fix">&nbsp;</div>
                            <!------------------------>
                            <!-- Start Group Result -->
                            <!------------------------>
                            <div class="small-12 medium-12 large-12 columns medium-centered">
                                <div class="row tt-j-result teamtime-group-result-row step-function"  ng-if="!(!TTOptions.showToMe && TTOptions.hideJudgments)">
                                <div class="small-12 medium-4 large-5 columns small-text-center medium-text-left large-text-left">
                                    <span class="icon-tt-chart icon"></span> <strong>GROUP RESULT</strong>
                                </div>
                                    <!-- owner result -->                                                                                    
                                <div class="small-12 medium-8 large-7 columns">
                                     <div class="tt-j-result-title ">
                                         <div class="large-5 medium-5 columns large-text-left medium-text-center small-text-center tt-clear-padding-left" >Priority: {{group_result[0] >= 0 ? (group_result[0] | number:2) : '' }}</div>
                                         <div class="large-7 medium-7 columns rating-scale-wrap">
                                                <div class="dc-group-result-bar" ng-style="{'width': (group_result[0] > 0 ? group_result[0] * 100 : 0) + '%'}"></div>
                                         </div>
                                    </div>
                                </div>                
                                    <!-- //owner result -->           
                            </div>
                            </div>
                            <div class="row">                        
                                <div class="large-12 columns small-centered text-center group-result-figure"> <!--tt-geometric-variance-wrap-->
                                     Geometric Variance: {{group_result[1] | number:2}}
                                </div>
                             </div>
                            <!------------------------>
                            <!-- End Group Result -->
                            <!------------------------>
                  </div>
              </div>
            </div>

        </div>

        <!------------------------>
        <!-- Start Group Result -->
        <!------------------------>

        <!------------------------>
        <!-- End Group Result -->
        <!------------------------>
    </div>
</div>
</div>
<div class="row detect-scroll-wrap">Scroll down to see more details</div>
<div class="row text-center detect-scroll-div-bottom ds-div-bottom hide-for-medium-down"></div>
<script>

    function save_step() {
        show_loading_icon();
        var scope = angular.element($("#TeamTimeDiv")).scope();
        var judgment = $("#steps-functionInput").val();
        var email = scope.current_user[0];
        var comment = scope.comment_txt[1];
        var index = 0;

        scope.stepfunction_save(judgment, email, comment, index);
    };



    var Obj;
    var NodeNum;
    var id;
    $('.edit-pencil').on('click', function() {
        Obj = $(this).attr("data-obj");
        NodeNum = $(this).attr("data-element");

    });

  

    $('.cancelbtn').on('click', function() {
        $('#editCon-3').foundation('reveal', 'close');
    });



</script>
