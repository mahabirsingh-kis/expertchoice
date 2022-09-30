<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Pairwise.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.Pairwise" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/LeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="LeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/RightNodeInfoDoc.ascx" TagPrefix="includes" TagName="RightNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTLeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTLeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTRightNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTRightNodeInfoDoc" %>
<%@ Register Src="~/Pages/includes/EraseAndCommentButtons.ascx" TagPrefix="includes" TagName="EraseAndCommentButtons" %> 
    <link href="/Content/stylesheets/nouislider.css" rel="stylesheet" />
    <link href="/Content/stylesheets/nouislider.pips.css" rel="stylesheet" />
    <link href="/Content/stylesheets/nouislider.tooltips.css" rel="stylesheet" />
    <link href="/Content/stylesheets/pairwise.css" rel="stylesheet" />
<%-- Pairwise --%>
    <includes:QuestionHeader runat="server" id="QuestionHeader" />     

        <div class="columns tt-auto-resize-content add-bottom-margin">
            <includes:ParentNodeInfoDoc runat="server" id="ParentNodeInfoDoc" />     
            <%-- Desktop PW --%> 
            <div 
                ng-if="!$parent.isMobile()"
                data-equalizer="nodes" class="large-12 columns tt-versus-wrap pairwise-desktop">
                <div class="large-3 columns vs-left-fix">
                    <div class="row collapse " ng-class="is_screen_reduced ? 'zoom-out' : ''">
                        <includes:LeftNodeInfoDoc runat="server" id="LeftNodeInfoDoc" />     
                        <includes:WRTLeftNodeInfoDoc runat="server" id="WRTLeftNodeInfoDoc" />
                    </div>
                </div>

                <%-- Desktop Graphical --%>
                <div class="large-6 columns tt-clear-padding-all vs-mid-fix single-pw-graphical" ng-if="output.pairwise_type == 'ptGraphical'">
                <div class="large-12 columns disable-when-pause">
                    <div class="tt-judgements-item large-12 columns">  
                     
                        <div class="row tt-j-content">
                            <div class="large-12 text-center">
                                <!-- Graphical Pie Chart only Here -->
                                <div class="columns tt-j-content">
                                     <div class="row tt-pizza-wrap" ng-hide="graphical_switch">
                                        <div class="large-2 medium-3 small-6 columns large-centered medium-centered small-centered text-center tt-pizza-graph " ng-hide="$parent.hide_piechart[0]" <%--ng-hide="hide_piechart[0]"--%>>
                                            <div class="pie-full" ></div>
                                            <div id="pie"></div>
                                            <div class="spaces hide"></div>
                                        </div>
                                        <br />
                                        <div class="columns">
                                            <a class="swap-judgment" ng-hide="output.IsUndefined" ng-click="swap_value()">
                                                <img src="../../Images/swap-icon.png" title="swap" alt="swap" width="30" class="point" />
                                            </a>
                                        </div>
                                        <div class="row small-collapse large-uncollapse ">
                                             <ul ng-style="{opacity:get_opacity(current_user[2])}" id="ttPizzaData" data-pie-id="pie" class="columns">
                                                <li class="m1" data-value="800">
                                                    <div class="small-3 columns text-center">
                                                             <span ng-if="output.first_node.length<=20" class=" label a">  
                                                                 {{output.first_node}}
                                                             </span>
                                                             <span ng-if="output.first_node.length>20" class=" label a">
                                                                    {{output.first_node.substring(0, 20)}}...
                                                             </span>
                                                    </div>
                                                    <div class="small-9 columns text-center bar-wrap">
                                                        <div class="row collapse">
                                                            <div class="large-3 medium-4 small-5 columns">
                                                                <div>
                                                                    <input name="a-value" class="number-input" type="number" ng-model="main_bar[0]" tabindex="1" 
                                                                        ng-keyup="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'up', $index, true)"
                                                                        ng-mousedown="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'press', $index, true)" 
                                                                        ng-mouseup="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'up', $index, true)"
                                                                        style="width:100%;" />
                                                                </div>

                                                             </div>
                                                            <div class="large-9 medium-8 small-7 columns large-text-right">
                                                                <div id="dsBarClr1" class="a_Slider columns dual-slider-wrap" index="1" color="blue" anytime-Graphslider ng-model="graphical_slider[0]"  cur_user="{{current_user[1]}}" user_list="{{users_list.length}}"
                                                                user="{{current_user[0]}}" 
                                                                permission="{{current_user[2]}}"
                                                                comment="{{comment_txt[1]}}"  >
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <input type="hidden" name="a" readonly class="clr-j-inpt-pizza"  style="width: 50px;">
                                                </li>
                                                <li class="m2" data-value="800">
                                                    <div class="small-3 columns text-center">
 
                                                            <span ng-if="output.second_node.length<=20" class=" label b">  
                                                                 {{output.second_node}}
                                                             </span>
                                                             <span ng-if="output.second_node.length>20" class=" label b">
                                                                    {{output.second_node.substring(0, 20)}}...
                                                             </span>
                                                    </div>
                                                    <div class="small-9 columns text-center bar-wrap" >
                                                        <div class="row collapse">
                                                            <div class="large-3 medium-4 small-5 columns">
                                                                <div   >
                                                                    <input name="b-value" class="number-input" type="number" tabindex="1" ng-model="main_bar[1]"
                                                                        ng-keyup="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'up', $index, true)"
                                                                        ng-mousedown="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'press', $index, true)" 
                                                                        ng-mouseup="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'up', $index, true)"
                                                                        style="width:100%;" />
                                                                 </div>

                                                            </div>
                                                            <div class="large-9 medium-8 small-7 columns large-text-right">
                                                                <div id="dsBarClr2" class="b_Slider columns dual-slider-wrap" index="1" color="green" anytime-Graphslider ng-model="graphical_slider[1]"  cur_user="{{current_user[1]}}" user_list="{{users_list.length}}"
                                                                    user="{{current_user[0]}}" 
                                                                    comment="{{comment_txt[1]}}" 
                                                                    permission="{{current_user[2]}}">
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <input type="hidden" name="b" readonly class="clr-j-inpt-pizza" style="width: 50px;">
                                                </li>
                                            </ul>
                                        </div>

                                    </div>
                                    <div class="large-12 large-centered columns graphical-slider-wrap text-center" ng-if="graphical_switch">
                                            <div class="text-center"  id="graphicalSlider" ng-model="numericalSlider[0]" ng-init="initializeNoUIGraphical(numericalSlider[0], numericalSlider[0] + 800)">
                                            </div>

                                    </div>
                                    <div class="large-12 large-centered columns" ng-if="graphical_switch">
                                        <div class="large-7 medium-6  {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-7' : 'small-12'}} columns large-centered text-centered medium-centered small-centered graphical-nouislider">
                                            <div class="large-3 medium-2 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-2' : 'small-3'}}  columns small-right text-right " style="margin-top:30px; padding-left:5px; padding-right:7px">
                                                <a class="swap-judgment" ng-hide="output.IsUndefined" ng-click="swap_value()">
                                                    <img src="../../Images/swap-icon.png" title="swap" alt="swap"  class="point" style="max-width:unset;width:22px" />
                                                </a>
                                            </div>
                                                <div class="large-3 medium-4 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-4' : 'small-3'}} columns" style="margin-top:25px; padding-right:7px; padding-left:5px">
                                                <input type="number" id="noUiInput1" step="any" ng-model="main_bar[0]" style="height:30px; font-size:0.8rem; margin-bottom:2px;" tabindex="1" 
                                                    ng-keyup="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'up', $index, true)" 
                                                    ng-keydown="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'press', $index, true)" 
                                                    ng-mousedown="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'press', $index, true)" 
                                                    ng-mouseup="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'up', $index, true)" />
                                            </div>
                                            <div class="large-3 medium-4 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-4' : 'small-3'}} columns"  style="margin-top:25px;  padding-left:7px; padding-right:5px">
                                                <input type="number" id="noUiInput2" step="any" ng-model="main_bar[1]" style="height:30px; font-size:0.8rem; margin-bottom:2px;" tabindex="1" 
                                                    ng-keyup="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'up', $index, true)" 
                                                    ng-keydown="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'press', $index, true)" 
                                                    ng-mousedown="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'press', $index, true)" 
                                                    ng-mouseup="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'up', $index, true)" />
                                            </div>
                                            <div class="large-3 medium-2 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-2' : 'small-3'}} columns small-left text-left" style="margin-top:30px; padding-left:5px">
                                                <span ng-if="main_bar[0] > 0" class="button tiny tt-button-primary tt-action-btn-clr-j " ng-click="save_pairwise(-2147483648000,0);"><span class="icon-tt-close icon"></span></span>
                                                <span ng-if="output.show_comments == true" class="mobile-comment-btn">
                                                    <a href="#" data-options="align:bottom" data-dropdown="comment-single" aria-controls="comment-single" aria-expanded="false" ng-class="output.comment == '' || output.comment == null ? 'comment-disabled' : ''" class=" smoothScrollLink" data-link="#comWrap" <%--ng-style="{color:output.comment != '' && output.comment != null ? '#008CBA' : '#b0d5ee'}" --%>>
                                                        <span class="icon-tt-comments"></span>
                                                    </a>
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="columns small-12 " ng-init="initialize_Pie()">
                                    <includes:EraseAndCommentButtons runat="server" id="EraseAndCommentButtonsGraphical" />  
                                </div>
                            </div>

                        </div>
                    </div>
                </div>

                
            </div>
                <%-- //Desktop Graphical --%>

                <%-- Desktop Verbal --%>
                <div ng-if="output.pairwise_type == 'ptVerbal'" class="large-6 columns tt-clear-padding-all vs-mid-fix">
                        <div ng-class="output.page_type != 'atPairwise' ? 'hide' : ''" class="" >    
                            <div class="disable-when-pause">
                                <div class="tt-judgements-item">
                                    <div class="row collapse tt-j-content">
                                        <div class="large-12 text-center ">
                                           <!--Verbal Bars only Here -->
 
                                            <div class="row tt-j-content tt-verbal-bars-wrap" >
                                                <div class="small-12 columns text-center " ng-class="is_screen_reduced ? 'zoom-out' : ''" >
                                                    <ul class="tt-equalizer-levels">
                                                        <li title="" class="nine lft {{main_gradient_checkbox ? '' : 'no-gradient'}} tg-gradients">
                                                            <span class="lvl-txt">Extreme</span>
                                                            <asp:Image ID="Image2" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="lvl-even lvl-e-1 eight lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt hide">Very Strong<br>
                                                                to Extreme</span>
                                                            <asp:Image ID="Image3" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="seven lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt">Very Strong</span>
                                                            <asp:Image ID="Image4" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="lvl-even lvl-e-2 six lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt hide">Strong to<br>
                                                                Very Strong</span>
                                                            <asp:Image ID="Image5" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="five lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt">Strong</span>
                                                            <asp:Image ID="Image6" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="lvl-even lvl-e-3 four lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt hide">Equal to<br>
                                                                Strong</span>
                                                            <asp:Image ID="Image7" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="three lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt">Moderate</span>
                                                            <asp:Image ID="Image8" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="lvl-even lvl-e-4 two lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt hide">Equal to<br>
                                                                Moderate</span>
                                                            <asp:Image ID="Image9" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>

                                                        <li title="" class="zero mid {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt">Equal</span>
                                                            <asp:Image ID="Image10" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>

                                                        <li title="" class="lvl-even lvl-e-5 two rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt hide">Equal to<br>
                                                                Moderate</span>
                                                            <asp:Image ID="Image11" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="three rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt">Moderate</span>
                                                            <asp:Image ID="Image12" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="lvl-even lvl-e-6 four rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt hide">Moderate
                                                                <br>
                                                                to Strong</span>
                                                            <asp:Image ID="Image17" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="five rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt">Strong</span>
                                                            <asp:Image ID="Image13" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="lvl-even lvl-e-7 six rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt hide">Strong to<br>
                                                                Very Strong</span>
                                                            <asp:Image ID="Image14" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="seven rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt">Very Strong</span>
                                                            <asp:Image ID="Image18" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="lvl-even lvl-e-8 eight rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt hide">Very Strong<br>
                                                                to Extreme</span>
                                                            <asp:Image ID="Image15" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                        <li title="" class="nine rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                            <span class="lvl-txt">Extreme</span>
                                                            <asp:Image ID="Image16" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                        </li>
                                                    </ul>
                                                    <ul id="pm-bars" class="tt-equalizer">
                                                        <li  
                                                            id="{{$index+1}}" 
                                                            title="{{::bar[1]}}"
                                                            class="{{::bar[2]}} pm-bars lft lft-eq lft-{{$index+1}} {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients"  
                                                            data-pos="lft"
                                                            ng-click="save_pairwise(bar[0], 1); " 
                                                            ng-class="output.advantage == 1 && output.value >= bar[0] ? 'active-selected': ''" 
                                                            ng-style="{ 'background' : (output.advantage == 1 && bar[0] > output.value && bar[0] - output.value < 1 &&  output.value % 1 > 0 ? 'linear-gradient(270deg, #0058a3 '+ (bar[0] - output.value) +'%, #cccccc 50%)' : '') }"
                                                            ng-repeat="bar in ::bars_left track by $index" 

                                                        ></li>   
                                                        <li 
                                                            id="9" 
                                                            title="Equal"
                                                            class="pm-bars zero mid lft-9 lft-eq rgt-eq rgt-9 mid-9  {{main_gradient_checkbox ? '' : 'no-gradient'}} 
                                    {{output.advantage == 1  ? 'lft-selected' : ''}}
                                    {{output.advantage == -1 ? 'rgt-selected' : ''}}  
                                    {{output.advantage == 0 && output.value == 1 ? 'active-selected' : ''}}                                              
                                                             tg-gradients" 
                                                            data-pos="mid"
                                                            ng-click="save_pairwise(1, 0); "
                                                        ></li>
                                                        <li id="{{bars_right.length-$index}}" 
                                                            title="{{::bar[1]}}" 
                                                            class="{{::bar[2]}} pm-bars rgt lvl-even-bar rgt-eq rgt-{{bars_right.length-$index}} {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients" 
                                                            data-pos="rgt" 
                                                            ng-click="save_pairwise(bar[0],-1); "
                                                            ng-class="output.advantage == -1 && output.value >= bar[0] ? 'active-selected': ''" 
                                                            ng-repeat="bar in ::bars_right track by $index" 
                                                            ng-style="{ 'background' : (output.advantage == -1 && bar[0] > output.value && bar[0] - output.value < 1 &&  output.value % 1 > 0 ? ('linear-gradient(90deg, #6aa84f '+ (bar[0] - output.value) +'%, #cccccc 50%)') : '') }"
                                                            ></li>
                                                    </ul>
                                                      <includes:EraseAndCommentButtons runat="server" id="EraseAndCommentButtonsVerbal" />  
                                                </div>
</div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    <span ng-if="output.pairwise_type == 'ptVerbal' && output.value % 1 > 0" class="columns text-center icon-tt-info-circle action "><%=AnytimeComparion.Pages.external_classes.TeamTimeClass.ResString("msgGPWJudgment") %></span>
                </div>
                <%-- //Desktop Verbal --%>

                <div class="large-3 columns vs-right-fix">
                  
                    <div class="row collapse" ng-class="is_screen_reduced ? 'zoom-out' : ''">
                        <includes:RightNodeInfoDoc runat="server" id="RightNodeInfoDoc" /> 
                        <includes:WRTRightNodeInfoDoc runat="server" id="WRTRightNodeInfoDoc" />    
                    </div>

                </div>
            </div>
            <%-- End of PW --%> 
            
        </div>

    

        <%-- Mobile Codes --%>
        <div ng-if="$parent.isMobile()">
            <div class="columns ">
                <div class="row" style="margin:0px">
                    <%-- Mobile Verbal --%>  
                    <div class="tt-mobile-wrap disable-when-pause"  ng-class="output.page_type != 'atPairwise' ? 'hide' : ''" class="" ng-if="output.pairwise_type == 'ptVerbal'">
                        <div class="row">
                            <div class="large-12 medium-12 medium-centered large-uncentered  columns">

                                <div class="columns tt-j-content single-pw-verbal">

 
                                        <!-- show selected bar -->

                                            <div class="columns selected-item-single-pw text-center " style="" ng-bind-html ="display_selected_bar(output.value, output.advantage)">
                                            </div>
                                        <!-- /show selected bar -->

                                    <div class="text-center">
                                        
                                        <ul id="pm-bars" class="tt-equalizer-mobile single-pw-equalizer single-pw-vb">
                                            <li  
                                                id="{{$index+1}}" 
                                                title="{{::bar[1]}}"
                                                class="{{::bar[2]}} {{main_gradient_checkbox ? '' : 'no-gradient'}} pm-bars lft lft-eq lft-{{$index+1}} {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients"  
                                                data-pos="lft"
                                                ng-click="save_pairwise(bar[0], 1); " 
                                                ng-class="output.advantage == 1 ? output.value >= bar[0] ? 'active-selected' : '' : ''" 
                                                ng-repeat="bar in ::bars_left track by $index"
                                                ng-style="{ 'background' : (output.advantage == 1 && bar[0] > output.value && bar[0] - output.value < 1 &&  output.value % 1 > 0 ? 'linear-gradient(270deg, #0058a3 '+ (bar[0] - output.value) +'%, #cccccc 50%)' : '') }"
                                                 >
                                                <span ng-if="bar[3] != 'M'" class="bar-label">{{::bar[3]}}</span>
                                                <span ng-if="bar[3] == 'M'" class="bar-label" style="top:-7px; position:relative">{{::bar[3]}}</span>
                                            </li>

                                            <li 
                                                id="9" 
                                                title="Equal"
                                                class="pm-bars zero mid lft-9 lft-eq rgt-eq rgt-9 mid-9 {{main_gradient_checkbox ? '' : 'no-gradient'}} 
                        {{output.advantage == 1   ? 'lft-selected' : ''}}
                        {{output.advantage == -1 ? 'rgt-selected' : ''}}  
                        {{output.advantage == 0 && output.value == 1 ? 'active-selected' : ''}} tg-gradients" 
                                                data-pos="mid"
                                                ng-click="save_pairwise(1, 0);" 
                                                >

                                                <span class="bar-label" style="display: block; height: 100%; font-size: 7px; margin-top: -1px;">E</span> 
                                             </li>

                                            <li id="{{bars_right.length-$index}}" 
                                                title="{{::bar[1]}}" 
                                                class="{{::bar[2]}} pm-bars rgt lvl-even-bar rgt-eq rgt-{{bars_right.length-$index}} {{main_gradient_checkbox ? '' : 'no-gradient'}} tg-gradients" 
                                                data-pos="rgt" 
                                                ng-click="save_pairwise(bar[0],-1); "
                                                ng-class="output.advantage == -1 ? output.value >= bar[0] ? 'active-selected' : '' : ''" 
                                                ng-repeat="bar in ::bars_right track by $index"
                                                ng-style="{ 'background' : (output.advantage == -1 && bar[0] > output.value && bar[0] - output.value < 1 &&  output.value % 1 > 0 ? ('linear-gradient(90deg, #6aa84f '+ (bar[0] - output.value) +'%, #cccccc 50%)') : '') }" 
                                                >
                                                <span ng-if="bar[3] != 'M'" class="bar-label">{{::bar[3]}}</span>
                                                <span ng-if="bar[3] == 'M'" class="bar-label" style="top:-7px; position:relative">{{::bar[3]}}</span>
                                            </li>       
                                        </ul>

                                       
                                    </div>
                                </div>
                            </div>
                        </div>
                         <div class="columns small-12 text-center">
                            <includes:EraseAndCommentButtons runat="server" id="EraseAndCommentButtons1" />  
                        </div>
                    </div>
                    <%-- //Mobile Verbal --%>

                        <%-- Mobile Graphical --%>
                        <div class="large-12 columns disable-when-pause single-pw-graphical" ng-if="output.pairwise_type == 'ptGraphical'">
                            <div class="tt-judgements-item large-12 columns">  
                       
                            <div class="row tt-j-content">
                                <div class="large-12 text-center">
                                <!-- Graphical Pie Chart only Here -->
                                <div class="columns tt-j-content">
                                    <div class="row tt-pizza-wrap" ng-hide="graphical_switch">
                                        <div class="row small-collapse large-uncollapse ">
                                            <div class="large-12 small-12 columns">
                                                <ul ng-style="{opacity:get_opacity(current_user[2])}" id="ttPizzaData" data-pie-id="pie" class="">
                                                    <li class="m1" data-value="800">
                                                        <div class="medium-4 small-12 columns text-center tt-clear-padding-all add-bottom-margin"  >
                                                            <span ng-if="output.first_node.length<=20" class=" label a">
                                                                    {{output.first_node}}
                                                             </span>
                                                             <span ng-if="output.first_node.length>20" class=" label a">
                                                                    {{output.first_node.substring(0, 20)}}...
                                                             </span>
                                                        </div>
                                                        <div class="medium-8 small-12 columns medium-text-center small-text-right bar-wrap">
                                                            <div class="row">
                                                                <div class="large-3 medium-4 small-3 columns">
                                                                    <input name="a-value" class="number-input" type="number" ng-model="main_bar[0]" tabindex="2" 
                                                                           ng-keypress="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'press', $index, true)" 
                                                                           ng-keyup="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'up', $index, true)"
                                                                           ng-mousedown="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'press', $index, true)" 
                                                                           ng-mouseup="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'up', $index, true)"
                                                                           style="width:100%" />

                                                                </div>
                                                                <div class="large-9 medium-8 small-9 columns large-text-right">
                                                                    <div id="dsBarClr11" class="a_Slider columns dual-slider-wrap" index="1" color="blue" anytime-Graphslider ng-model="graphical_slider[0]"  cur_user="{{current_user[1]}}" user_list="{{users_list.length}}"
                                                                    user="{{current_user[0]}}" 
                                                                    permission="{{current_user[2]}}"
                                                                    comment="{{comment_txt[1]}}"  >
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <input type="hidden" name="a" readonly class="clr-j-inpt-pizza"  style="width: 50px;">
                                                    </li>
                                                    <li class="m2" data-value="800">
                                                        <div class="medium-4 small-12 columns text-center tt-clear-padding-all add-bottom-margin" >
                                                             <span ng-if="output.second_node.length<=20" class=" label b">
                                                                    {{output.second_node}}
                                                             </span>
                                                             <span ng-if="output.second_node.length>20" class=" label b">
                                                                    {{output.second_node.substring(0, 20)}}...
                                                             </span>
                                                        </div>
                                                        <div class="medium-8 small-12 columns medium-text-center small-text-right bar-wrap" >
                                                            <div class="row">
                                                                <div class="large-3 medium-4 small-3 columns">
                                                                    <input name="b-value" class="number-input" type="number" tabindex="2" ng-model="main_bar[1]"
                                                                        ng-keypress="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'press', $index, true)" 
                                                                        ng-keyup="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'up', $index, true)"
                                                                        ng-mousedown="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'press', $index, true)" 
                                                                        ng-mouseup="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'up', $index, true)"
                                                                        style="width:100%" />

                                                                </div>
                                                                <div class="large-9 medium-8 small-9 columns large-text-right">
                                                                    <div id="dsBarClr22" class="b_Slider columns dual-slider-wrap"  index="1" color="green" anytime-Graphslider ng-model="graphical_slider[1]"  cur_user="{{current_user[1]}}" user_list="{{users_list.length}}"
                                                                        user="{{current_user[0]}}" 
                                                                        comment="{{comment_txt[1]}}" 
                                                                        permission="{{current_user[2]}}">
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <input type="hidden" name="b" readonly class="clr-j-inpt-pizza" style="width: 50px;" ng-init="setSliderColor()">
                                                    </li>
                                                </ul>
                                            </div>
                                        </div>
                                        <div class="small-12 columns text-center swap-close-btns">
                                            <a  ng-hide="output.IsUndefined" class="swap-judgment" ng-click="swap_value()">
                                                <img src="../../Images/swap-icon.png" title="swap" alt="swap" width="30" class="point" />
                                            </a>
                                            &nbsp; &nbsp;
                                            <span ng-if="output.value!=0 && output.page_type == 'atPairwise'" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise" ng-click="save_pairwise(-2147483648000,0);">
                                                <span class="icon-tt-close icon"></span>
                                            </span>
                                        </div>

                                    </div>
                                    <div class="large-12 large-centered columns graphical-slider-wrap text-center" ng-if="graphical_switch">
  
                                        <div class="text-center"  id="graphicalSlider" ng-model="numericalSlider[0]" ng-init="initializeNoUIGraphical(numericalSlider[0], numericalSlider[0] + 800)">
                                        </div>

                                    </div>
                                    <div class="small-12 small-centered columns" ng-if="graphical_switch">
                                            <div class=" medium-6  {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-7' : 'small-12'}} columns large-centered text-centered medium-centered small-centered graphical-nouislider">
                                                <div class=" medium-2 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-2' : 'small-3'}}  columns small-right text-right " style="margin-top:30px; padding-left:5px; padding-right:7px">
                                                    <a class="swap-judgment"  ng-hide="output.IsUndefined" ng-click="swap_value()">
                                                        <img src="../../Images/swap-icon.png" title="swap" alt="swap"  class="point" style="max-width:unset;width:22px" />
                                                    </a>
                                                </div>
                                                    <div class=" medium-4 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-4' : 'small-3'}} columns" style="margin-top:25px; padding-right:7px; padding-left:5px">
                                                    <input id="noUiInput1" type="number"  step="any" ng-model="main_bar[0]" style="height:30px; font-size:0.8rem;" tabindex="1" 
                                                        ng-keyup="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'up', $index, true)" 
                                                        ng-keydown="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'press', $index, true)" 
                                                        ng-mousedown="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'press', $index, true)" 
                                                        ng-mouseup="graphical_key_up($event, [main_bar[0], current_user[0], comment_txt[1]], -1, 'up', $index, true)" />
                                                </div>
                                                <div class=" medium-4 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-4' : 'small-3'}} columns"  style="margin-top:25px;  padding-left:7px; padding-right:5px">
                                                    <input id="noUiInput2" type="number"  step="any" ng-model="main_bar[1]" style="height:30px; font-size:0.8rem;" tabindex="1" 
                                                        ng-keyup="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'up', $index, true)" 
                                                        ng-keydown="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'press', $index, true)" 
                                                        ng-mousedown="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'press', $index, true)" 
                                                        ng-mouseup="graphical_key_up($event, [main_bar[1], current_user[0], comment_txt[1]], 1, 'up', $index, true)"/>
                                                </div>
                                                <div class=" medium-2 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-2' : 'small-3'}} columns small-left text-left" style="margin-top:28px; padding-left:5px">
                                                    <span class="button tiny tt-button-primary tt-action-btn-clr-j " style="top: 2px" ng-click="save_pairwise(-2147483648000,0);"><span class="icon-tt-close icon"></span></span>
                                                      <span ng-if="output.show_comments == true" class="mobile-comment-btn">
                                                          <a href="#" data-reveal-id="tt-c-modal" class="toggleComments smoothScrollLink" data-link="#comWrap" ng-class="output.comment == '' || output.comment == null ? 'comment-disabled' : ''" <%--ng-style="{color:output.comment != '' && output.comment != null ? '#008CBA' : '#b0d5ee'}"--%> >
                                                              <span class="icon-tt-comments"></span>
                                                          </a>
                                                      </span>
                                                </div>
                                            </div>
                                    </div>
  
                                </div>

                                </div>

                            </div>
                        </div>
                        </div>
                        <%-- //Mobile Graphical --%>

                        <%-- Nodes and Info Docs--%>
                        <div class="tg-legend">
                            <div class="row tt-question-choices single-pw-verbal" data-equalizer="nodes" >
                                <div class="small-6 columns ">
                                      <includes:LeftNodeInfoDoc runat="server" id="MobileLeftNodeInfoDoc" />   
                                      <includes:WRTLeftNodeInfoDoc runat="server" id="MobileWRTLeftNodeInfoDoc" />    
                                </div>
                                <div class="small-6 columns ">
                                    <includes:RightNodeInfoDoc runat="server" id="MobileRightInfoDoc" />  
                                    <includes:WRTRightNodeInfoDoc runat="server" id="MobileWRTRightInfoDoc" />  
                                </div>
                            </div>
                        </div>
                        
                        <%-- // Nodes and Info Docs--%>
                </div>
                <span ng-if="output.pairwise_type == 'ptVerbal' && output.value % 1 > 0" class="columns text-center icon-tt-info-circle action "><%=AnytimeComparion.Pages.external_classes.TeamTimeClass.ResString("msgGPWJudgment") %></span>
            </div>    
        </div>    
        <%-- //Mobile Codes --%>      
<%-- tooltips of pw --%>   
<div id="gdrop_parent_node" class="f-dropdown dropdown-layout-fix anytime-single-graphical question-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
    <div class="row collapse">
                
            <div class="small-9 columns text-center">
                {{output.parent_node}}
            </div>
            <div class="small-3 columns text-center">
                <div class="row collapse">
                    <div class="small-6 columns text-center">
                        <a ng-class="is_AT_owner == 1 ? '' : 'hide'" 
                            class="edit-info-doc-btn" data-node-type="-1" data-location="-1" 
                            data-node="parent-node" data-node-description="{{output.parent_node}}">
                            <span class="icon-tt-edit"></span>
                        </a>
                    </div>
                    <div class="small-6 columns text-center">
                        <a href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a>
                    </div>
                </div>
            </div>
        <hr>
    </div>
    <div class="row">
        <div class="infotext-wrap large-12 columns parent-node-info-text" ng-bind-html="getHtml(output.parent_node_info)"></div>
    </div>
</div>
<div 
    ng-class="{'medium' : has_long_content(output.first_node_info)}"
    id="gdrop_left_node" class="f-dropdown dropdown-layout-fix anytime-single-graphical alternative-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
    <div class="row collapse">
                
            <div class="small-9 columns text-center">
                {{output.first_node}}
            </div>
            <div class="small-3 columns text-center">
                <div class="row collapse">
                    <div class="small-6 columns text-center">
                        <a  style="color: #008CBA;"
                            ng-class="is_AT_owner == 1 ? '' : 'hide'" 
                            class="edit-info-doc-btn" data-node-type="2" data-location="1"  
                            data-node="left-node" data-node-description=" {{output.first_node}}{{output.child_node}}">
                            <span class="icon-tt-edit"></span>
                        </a>
                        <a  style="color: #008CBA;"
                            ng-class="is_AT_owner == 0 ? '' : 'hide'" 
                            class="edit-info-doc-btn" data-node-type="2" data-location="1" data-node="left-node"
                            data-node-description=" {{output.first_node}}{{output.child_node}}" data-readonly="1">
                            <span class="icon-tt-zoom-in"></span>
                        </a>
                    </div>
                    <div class="small-6 columns text-center">
                        <a style="color: #008CBA;" href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a>
                    </div>
                </div>
            </div>
        <hr>
    </div>
    <div class="row">
        <div class="infotext-wrap large-12 columns left-node-info-text" ng-bind-html="getHtml(output.first_node_info)"></div>
    </div>
</div>
<div 
    ng-class="{'medium' : has_long_content(output.wrt_first_node_info)}"
    id="gdrop_wrt_left_node" class="f-dropdown dropdown-layout-fix anytime-single-graphical alternative-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
    <div class="row collapse">
                
            <div class="small-9 columns text-center">
                {{get_wrt_text('left')}}
            </div>
            <div class="small-3 columns text-center">
                <div class="row collapse">
                    <div class="small-6 columns text-center">
                        <a ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn" 
                            data-node-type="3" data-location="1" data-node="wrt-left-node" 
                            data-node-title="{{output.first_node}}{{output.child_node}}" data-node-description="{{output.parent_node}}">
                            <span class="icon-tt-edit"></span>
                        </a>  
                    </div>
                    <div class="small-6 columns text-center">
                        <a href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a>
                    </div>
                </div>
            </div>
        <hr>
    </div>
    <div class="row">
        <div class="infotext-wrap large-12 columns wrt-left-node-info-text" ng-bind-html="getHtml(output.wrt_first_node_info)"></div>
    </div>
</div>   
<div 
    ng-class="{'medium' : has_long_content(output.second_node_info)}"
    id="gdrop_right_node" class="f-dropdown dropdown-layout-fix anytime-single-graphical alternative-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
    <div class="row collapse">
                
            <div class="small-9 columns text-center">
                {{output.second_node}}
            </div>
            <div class="small-3 columns text-center">
                <div class="row collapse">
                    <div class="small-6 columns text-center">
                        <a style="color: #008CBA;"
                            ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn" 
                            data-node-type="2" data-location="2" data-node="right-node" 
                            data-node-description=" {{output.second_node}}">
                            <span class="icon-tt-edit"></span>
                        </a>  
                    </div>
                    <div class="small-6 columns text-center">
                        <a style="color: #008CBA;" href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a>
                    </div>
                </div>
            </div>
        <hr>
    </div>
    <div class="row">
        <div class="infotext-wrap large-12 columns right-node-info-text" ng-bind-html="getHtml(output.second_node_info)"></div>
    </div>
</div>
 <div 
       ng-class="{'medium' : has_long_content(output.wrt_second_node_info)}"
     id="gdrop_wrt_right_node" class="f-dropdown dropdown-layout-fix anytime-single-graphical alternative-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
    <div class="row collapse">
                
            <div class="small-9 columns text-center">
                {{get_wrt_text('right')}}
            </div>
            <div class="small-3 columns text-center">
                <div class="row collapse">
                    <div class="small-6 columns text-center">
                        <a ng-class="is_AT_owner == 1 ? '' : 'hide'" 
                            class="edit-info-doc-btn" 
                            data-node-type="3"
                            data-location="2" 
                            data-node="wrt-right-node" 
                            data-node-title="{{output.first_node}}{{output.child_node}}" 
                            data-node-description="{{output.parent_node}}"
                            >
                            <span class="icon-tt-edit"></span>
                        </a>  
                    </div>
                    <div class="small-6 columns text-center">
                        <a href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a>
                    </div>
                </div>
            </div>
        <hr>
    </div>
    <div class="row">
        <div class="infotext-wrap large-12 columns wrt-right-node-info-text" ng-bind-html="getHtml(output.wrt_second_node_info)"></div>
    </div>
</div>

<script src="/Scripts/NoUI/nouislider.min.js"></script>
<script src="/Scripts/NoUI/wNumb.js"></script>
<asp:PlaceHolder runat="server">
    <%: Scripts.Render("~/bundles/pairwise") %>
</asp:PlaceHolder>
 

<%-- End of Pairwise --%>
 