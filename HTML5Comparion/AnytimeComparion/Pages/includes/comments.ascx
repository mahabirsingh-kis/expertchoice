<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="comments.ascx.cs" Inherits="AnytimeComparion.Pages.includes.WebUserControl1" %>
    <%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
        <span ng-if="is_teamtime">
        <div ng-if="is_teamtime" id="comWrap" class="large-12 columns tt-comments-form">
            <div class="tt-toggle-comments-wrap toggleComments tc-btn-right hide"><span class="icon-tt-close"></span></div>
        <div class="columns tt-normal-head blue">
            <span class="icon-tt-comments icon"></span>
            <span class="text">Comments</span>

        </div>
        <div class="columns ">
            <textarea class="comment-text-area" name="tt-comm-textarea" ng-model="comment_txt[0]" rows="2" placeholder="What can you say?"></textarea>
            <a class="button tiny tt-button-icond-left tt-button-primary right send-comment">
                <span class="icon-tt-plus icon"></span>
                <span class="text" ng-click="send_comment(comment_txt[0])">SEND COMMENT</span>
            </a>

        </div>
        </div>
        <div class="large-12 columns tt-comments-archives teamtime-comments">
            <%---                    <ul class="tabs button-group even-3" data-tab>
        <li class="tab-title active"><a href="#recent">Recent</a></li>
        <li class="tab-title"><a href="#lastweek">Last Week</a></li>
        <li class="tab-title"><a href="#older">Older</a></li>
    </ul>--%>
                <div class="tt-com-single-wrap">
                    <!-- single comment -->
                    <div class="columns tt-com-single" ng-repeat="user in users_list" ng-show="<%--(hide_offline && user[3] == 0) && --%>(check($index) || (current_user[0] == user[1].toLowerCase()))" ng-if="user[6] != ''">
                        <h5 class="tt-com-name">{{user[2]}}</h5>
                        <div class="tt-com-date">
                            <span class="action">{{split_comment(user[6],1)}}</span>
                            <span class="action" ng-if="split_comment(user[6],1) != 'Recently'">{{split_comment(user[6],2)}}</span>
                        </div>
                        <p class="another-line timestamp-here user-comments-<% %>" data-options="show_on:large">{{split_comment(user[6],0)}}</p>
                    </div>
                    <!-- //single comment -->
                </div>
        </div>
        </span>

        <%-- if anytime --%>
            

         <span ng-if="is_anytime">
            <div id="comWrap" class="large-12 columns tt-comments-form">
                <div class="tt-toggle-comments-wrap toggleComments tc-btn-right hide"><span class="icon-tt-close"></span></div>
                <div class="columns tt-normal-head blue">
                    <span class="icon-tt-comments icon"></span>
                    <span class="text">Comments</span>
                    <div class="nothing" >
                        <div ng-if="is_comment_updated" class="large-5 large-centered columns tt-fullwidth-loading tt-center-height-wrap text-center" >
                            <div class="tt-loading-icon small-loading-animate" style="position:absolute; width: 100%; z-index:1;" ><span class="icon-tt-loading " ></span></div>
                        </div>

                    </div> 

                </div>
                <div ng-if="!is_multi" class="columns ">
                    <textarea 
                        ng-model="formatted_comment"
                        class="comment-text-area" name="tt-comm-textarea"  rows="2" placeholder="What can you say?"></textarea>
                    <a ng-click="comment_updated(formatted_comment)" class="button tiny tt-button-icond-left tt-button-primary right send-comment">
                        <span class="icon-tt-plus icon"></span>
                        <span class="text">SEND COMMENT</span>
                    </a>

                </div>

                <div ng-if="is_multi && (output.page_type == 'atNonPWAllChildren' || output.page_type == 'atNonPWAllCovObjs')" class="columns ">
                    <textarea ng-model="formatted_comment"
                        class="comment-text-area" name="tt-comm-textarea"  rows="2" placeholder="What can you say?"></textarea>
                
                    <a ng-click="comment_updated(formatted_comment)" class="button tiny tt-button-icond-left tt-button-primary right send-comment">
                        <span class="icon-tt-plus icon"></span>
                        <span class="text">SEND COMMENT</span>
                    </a>
                </div>

                <div ng-if="is_multi && (output.page_type == 'atAllPairwise' || output.page_type == 'atAllPairwiseOutcomes')" class="columns">
                     <textarea 
                        ng-model="formatted_comment"
                        class="comment-text-area" name="tt-comm-textarea"  rows="2" placeholder="What can you say?">
                     </textarea>

                     <a ng-click="comment_updated(formatted_comment)" class="button tiny tt-button-icond-left tt-button-primary right send-comment">
                        <span class="icon-tt-plus icon"></span>
                        <span class="text">SEND COMMENT</span>
                    </a>
                </div>

            </div>

            <%-- will change this later --%>
             <div>&nbsp;</div>
            <div class="row collapse tt-comments-archives teamtime-comments anytime-com-wrap">
                    <ul class="tabs button-group even-3" data-tab>
                        <li class="tab-title active"><a href="#recent">Recent</a></li>
<%--                        <li class="tab-title"><a href="#lastweek">Last Week</a></li>
                        <li class="tab-title"><a href="#older">Older</a></li>--%>
                    </ul>

                    <div class="tabs-content columns">
                        <!-- single comment -->
                        <!--RECENT -->
                        <div class="content active" id="recent">  
                            <!-- single comment -->
                            <div 
                                ng-init="comment_str = user_comment.comment.split('_')"
                                ng-if="user_comment.comment != '' && !is_multi" ng-repeat="user_comment in output.usersComments track by $index | orderBy: '-c_date'" class="columns tt-com-single">
                                <h4>{{user_comment.name}} 
                                    <span  class="timestamp" title="{{user_comment.date[0]}}">{{user_comment.date[0]}} {{user_comment.date[1]}}</span></h4>
                                <div class="tt-com-workgroups">
                                    <span class="">{{user_comment.email}}</span>
                                </div>
                                <p class="comment_{{user_comment.email}}"> {{comment_str[0]}} </p>
                            </div>
                            <!-- //single comment -->

                             <!-- single comment -->
                            <div 
                                ng-if="user_comment.comment != '' && is_multi"
                                ng-init="comment_str = user_comment.comment.split('_');"
                                ng-repeat="user_comment in multi_comments[active_multi_index] track by $index  | orderBy: '-c_date'" class="columns tt-com-single">
                                <h4>{{user_comment.name}} <span class="timestamp " title="{{user_comment.date[0]}}">{{user_comment.date[0]}} {{user_comment.date[1]}}</span></h4>
                                <div class="tt-com-workgroups">
                                    <span class="">{{user_comment.email}}</span>
                                    
                                </div>
                                <p class="comment_{{user_comment.email}}"> {{comment_str[0]}} </p>
                            </div>
                            <!-- //single comment -->
                        </div>
                        <!--LAST WEEK -->
                        <div class="content" id="lastweek">
                            <%--<!-- single comment -->
                            <div class="columns tt-com-single">
                                <h4>Aileen <span data-tooltip class="timestamp has-tip tip-top" title="Nov. 23, 2016">11am</span></h4>
                                <div class="tt-com-workgroups">
                                    <span class="">Gecko3, </span>
                                    <span class="">Group 2</span>
                                    
                                </div>
                                <p>is, porttitor convallis nunc accis, porttitor convallis nunc accis, porttitor convallis nunc accDonec sed placerat magna. Duis sollicitudin, dolor quis pellentesque auctor, libero nunc iaculis est, id pharetra nisi mi suscipit tellus. Etiam varius lacinia nunc ac fringilla. In eleifend urna ut odio mattumsan. </p>
                            </div>
                            <!-- //single comment -->

                            <!-- single comment -->
                            <div class="columns tt-com-single">
                                <h4>Romeo <span data-tooltip class="timestamp has-tip tip-top" title="Nov. 23, 2016">11am</span></h4>
                                <div class="tt-com-workgroups">
                                    <span class="">Gecko1,</span>
                                    <span class="">others</span>
                                    
                                </div>
                                <p>nd urna ut odio mattind urna ut odio mattind urna ut odio mattiDonec sed placerat magna. Duis sollicitudin, dolor quis pellentesque auctor, libero nunc iaculis est, id pharetra nisi mi suscipit tellus. Etiam varius lacinia nunc ac fringilla. In eleifend urna ut odio mattis, porttitor convallis nunc accumsan. </p>
                            </div>
                            <!-- //single comment -->--%>
                        </div>

                        <!--OLDER -->
                        <div class="content" id="older">
                            <%--<!-- single comment -->
                            <div class="columns tt-com-single">
                                <h4>Mike <span data-tooltip class="timestamp has-tip tip-top" title="Nov. 23, 2016">11am</span></h4>
                                <div class="tt-com-workgroups">
                                    <span class="">Gecko,</span>
                                    <span class="">Group 2</span>
                                    
                                </div>
                                <p>Donec sed nd urna ut odio matti, porttitor convallis nunc accumsan. </p>
                            </div>
                            <!-- //single comment -->

                            <!-- single comment -->
                            <div class="columns tt-com-single">
                                <h4>Ena <span data-tooltip class="timestamp has-tip tip-top" title="Nov. 23, 2016">11am</span></h4>
                                <div class="tt-com-workgroups">
                                    <span class="">Lorem,</span>
                                    <span class="">Group 2</span>
                                    
                                </div>
                                <p>Donec sed placerat nd urna ut odio mattind urna ut odio mattind urna ut odio matti porttitor convallis nunc accumsan. </p>
                            </div>
                            <!-- //single comment -->

                            <!-- single comment -->
                            <div class="columns tt-com-single">
                                <h4>Shera <span data-tooltip class="timestamp has-tip tip-top" title="Nov. 23, 2016">11am</span></h4>
                                <div class="tt-com-workgroups">
                                    <span class="">Lorem,</span>
                                    <span class="">Group 2</span>
                                    
                                </div>
                                <p>Donec sed placerat magna. Duis sollicitudin, dolor quis pellentesque auctor, libero nunc iaculis est, id pharetra nisi mi suscipit tellus. Etiam varius lacinia nunc ac fringilla. In eleifend urna ut odio mattis, porttitor convallis nunc accumsan. </p>
                            </div>
                            <!-- //single comment -->

                            <!-- single comment -->
                            <div class="columns tt-com-single">
                                <h4>Leo The Great</h4>
                                <div class="tt-com-date">
                                    <span class="action">Group 1</span>
                                    <span class="action">Group 2</span>
                                    <span class="action">Group 3</span>
                                    <span class="action">Group 4</span>
                                    <span class="action">May 1, 2015</span>
                                </div>
                                <p>Donec sed placerat magna. Duis sollicitudin, dolor quis pellentesque auctor, libero nunc iaculis est, id pharetra nisi mi suscipit tellus. Etiam varius lacinia nunc ac fringilla. In eleifend urna ut odio mattis, porttitor convallis nunc accumsan. </p>
                            </div>
                            <!-- //single comment -->--%>
                        </div>


                        <!-- //single comment -->
                    </div>
                </div>
         </span>
