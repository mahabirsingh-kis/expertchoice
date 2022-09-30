<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Participants.aspx.cs" Inherits="AnytimeComparion.Pages.Participants" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="tt-body">

        <% if (Session["User"] != null || Session["UserType"] != null)
           { %>


        <div class="row collapse tt-2-cols">

            <div class="large-10 small-12 columns large-centered tt-left-col">

                <div class="large-12 columns tt-header-nav">
                    <div class="columns  tt-header-nav blue-header">
                        <span class="text">Participants <span class="action">(3)</span></span>
                    </div>

                    <ul class="large-12 columns ">
                        <!--
                <li>
                  <h3>Participants <span class="action">(3)</span></h3>
                </li>
-->
                        <li>
                            <button data-reveal-id="addPartModal" class="button tiny tt-button-icond-left tt-button-primary">
                                <span class="text">Add New</span>
                                <span class="icon-tt-plus icon"></span>
                            </button>

                            <!-- addPart modal wrap -->
                            <div id="addPartModal" class="reveal-modal medium tt-modal-wrap" data-reveal aria-labelledby="addPart" aria-hidden="true" role="dialog">
                                <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                                <h2 id="addPart" class="tt-modal-header blue-header">Add New Participants</h2>

                                <div class="tt-modal-content large-12 columns">
                                    <!--                  <form data-abide="ajax">-->
                                    <div class="large-12 columns">
                                        <div class="row collapse">
                                            <div class="small-3 large-2 columns">
                                                <span class="prefix">Name*</span>
                                            </div>
                                            <div class="small-9 large-10 columns">
                                                <input type="text" placeholder="" required>
                                                <small class="error">Your name is important.</small>
                                            </div>
                                        </div>
                                        <div class="row collapse">
                                            <div class="small-3 large-2 columns">
                                                <span class="prefix">Email*</span>
                                            </div>
                                            <div class="small-9 large-10 columns">
                                                <input type="email" placeholder="" required>
                                                <small class="error">Your email is important.</small>
                                            </div>
                                        </div>
                                        <div class="row collapse">
                                            <div class="small-3 large-2 columns">
                                                <span class="prefix">Phone</span>
                                            </div>
                                            <div class="small-9 large-10 columns">
                                                <input type="text" placeholder="ex: +1 1234567">
                                            </div>
                                        </div>
                                        <div class="row collapse">
                                            <div class="small-3 large-2 columns">
                                                <span class="prefix">Access*</span>
                                            </div>
                                            <div class="small-9 large-10 columns">
                                                <select required>
                                                    <option>Steps</option>
                                                    <option>Lorem</option>
                                                    <option>Ipsum</option>
                                                </select>
                                                <small class="error">You must add Access Level.</small>
                                            </div>
                                        </div>
                                        <div class="row collapse">
                                            <div class="small-3 large-2 columns">
                                                <span class="prefix">Assign Group*</span>
                                            </div>
                                            <div class="small-9 large-10 columns">
                                                <select required>
                                                    <option>Group1</option>
                                                    <option>Lorem</option>
                                                    <option>Ipsum</option>
                                                </select>
                                                <small class="error">You must Assign a Group to this user.</small>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="large-12 columns">
                                        <label class="columns">
                                            <input type="checkbox" class="">
                                            Generate Random Password
                                        </label>
                                        <label class="columns">
                                            <input type="checkbox" class="">
                                            Send a registration informtation to the user
                                        </label>
                                    </div>
                                    <div class="large-12 columns text-right">
                                        <hr>
                                        <button type="submit" class="button tiny tt-button-icond-left tt-button-primary right">
                                            <span class="text">Import</span>
                                            <span class="icon-tt-cloud-download icon"></span>
                                        </button>
                                        <button type="submit" class="button tiny tt-button-primary tt-button-transparent right">
                                            <span class="text">Cancel</span>
                                        </button>
                                    </div>
                                    <!--                  </form>-->
                                </div>
                            </div>
                            <!-- // addPart modal wrap -->

                        </li>
                        <li>
                            <button data-reveal-id="importPart-modal" class="button tiny tt-button-icond-left tt-button-primary">
                                <span class="text">Import</span>
                                <span class="icon-tt-cloud-download icon"></span>
                            </button>
                            <!-- importPart modal wrap -->
                            <div id="importPart-modal" class="reveal-modal medium tt-modal-wrap" data-reveal aria-labelledby="importPart" aria-hidden="true" role="dialog">
                                <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                                <h2 id="importPart" class="tt-modal-header blue-header">Import Participants</h2>
                                <div class="tt-modal-content">
                                    <div class="large-5 columns">
                                        <button class="button tt-button-primary tt-button-icond-left ">
                                            <span class="icon-tt-plus icon"></span>
                                            <span class="text">Select All</span>
                                        </button>

                                        <form>
                                            <div class="row collapse">
                                                <div class="small-3 large-2 columns">
                                                    <span class="prefix">Group:</span>
                                                </div>
                                                <div class="small-9 large-10 columns">
                                                    <select>
                                                        <option>All</option>
                                                        <option>Lorem</option>
                                                        <option>Ipsum</option>
                                                    </select>
                                                </div>
                                            </div>

                                        </form>
                                    </div>
                                    <div class="large-12 columns">
                                        <table class="responsive tt-table-wrap">
                                            <thead>
                                                <tr>
                                                    <th></th>
                                                    <th class="tt-with-sorter">
                                                        <div class="text small-11 columns">Name</div>
                                                        <div class="sorter-icons small-1 columns text-center">
                                                            <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                                        </div>
                                                    </th>
                                                    <th>Email</th>
                                                    <th class="tt-with-sorter">
                                                        <div class="text small-11 columns">Group</div>
                                                        <div class="sorter-icons small-1 columns text-center">
                                                            <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                                        </div>
                                                    </th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr>
                                                    <td>
                                                        <input type="checkbox">
                                                    </td>
                                                    <td>Aurora Morris</td>
                                                    <td>aurorawmorris@jourapid.com</td>
                                                    <td>Group 2</td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <input type="checkbox">
                                                    </td>
                                                    <td>Mike Asuncino</td>
                                                    <td>m.asuncion@eversun.ph</td>
                                                    <td>Group 1</td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                    <div class="large-12 columns">
                                        <label class="columns">
                                            <input type="checkbox" class="">
                                            Generate Random Password
                                        </label>
                                        <label class="columns">
                                            <input type="checkbox" class="">
                                            Send a registration informtation to the user
                                        </label>
                                    </div>
                                    <div class="large-12 columns text-right">
                                        <hr>
                                        <a href="#" class="button tiny tt-button-icond-left tt-button-primary right">
                                            <span class="text">Import</span>
                                            <span class="icon-tt-cloud-download icon"></span>
                                        </a>
                                        <a href="#" class="button tiny tt-button-primary tt-button-transparent right">
                                            <span class="text">Cancel</span>
                                        </a>
                                    </div>
                                </div>
                            </div>
                            <!-- // addPart modal wrap -->
                        </li>
                        <li>
                            <a href="#" class="tt-dropdown button tiny tt-button-primary" href="#" data-dropdown="tt-col-dd" data-options="is_hover:false;">
                                <span class="icon-tt-filter"></span>
                                <span class="text">Columns </span>
                                <span class="icon-tt-chevron-down drop-arrow"></span>
                            </a>

                            <ul id="tt-col-dd" class="tt-dropdown-content f-dropdown tt-dropdown-content-has-form" data-dropdown-content>
                                <li>
                                    <label>
                                        <input type="checkbox" data-name="progress" checked class="togglePart-columns">
                                        Progress</label>
                                </li>
                                <li>
                                    <label>
                                        <input type="checkbox" data-name="email" checked class="togglePart-columns">
                                        Email</label>
                                </li>
                                <li>
                                    <label>
                                        <input type="checkbox" data-name="phone" checked class="togglePart-columns">
                                        Phone</label>
                                </li>
                                <li>
                                    <label>
                                        <input type="checkbox" data-name="access" checked class="togglePart-columns">
                                        Access</label>
                                </li>
                                <li>
                                    <label>
                                        <input type="checkbox" data-name="role" checked class="togglePart-columns">
                                        Role</label>
                                </li>
                                <li>
                                    <label>
                                        <input type="checkbox" data-name="pgroup" checked class="togglePart-columns">
                                        Participant Group</label>
                                </li>
                            </ul>
                        </li>



                    </ul>


                    <div class="row collapse">
                        <div class="large-12 columns">
                            <table class="responsive tt-table-wrap">
                                <thead>
                                    <tr>
                                        <th class="tt-name"><span class="icon-tt-user"></span>Name</th>
                                        <th class="tt-progress"><span class="icon-tt-loading"></span>Progress</th>
                                        <th class="tt-email"><span class="icon-tt-envelope"></span>Email</th>
                                        <th class="tt-phone"><span class="icon-tt-phone"></span>Phone</th>
                                        <th class="tt-access"><span class="icon-tt-lock"></span>Access</th>
                                        <th class="tt-role"><span class="icon-tt-user-secret"></span>Role</th>
                                        <th class="tt-pgroup"><span class="icon-tt-users"></span>Participant Group</th>
                                        <th class="tt-action"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td class="tt-name"><span class="tt-user-status online">&#9679;</span>System Manager</td>
                                        <td class="tt-progress">
                                            <div class="nice radius success progress"><span class="meter" style="width: 100%"></span><span class="text">50%</span></div>
                                        </td>
                                        <td class="tt-email"><a href="mailto:email@email.com">System Manager</a></td>
                                        <td class="tt-phone"><a href="tel:+12356789">09123456789</a></td>
                                        <td class="tt-access">
                                            <select>
                                                <option value="">Evaluation</option>
                                                <option value="">View Only</option>
                                                <option value="">Others</option>
                                                <option value="">No Role</option>
                                            </select>
                                        </td>
                                        <td class="tt-role">
                                            <select>
                                                <option value="">Workgroup Manager</option>
                                                <option value="">Project Organizer</option>
                                                <option value="">Workgroup Member</option>
                                            </select>
                                        </td>
                                        <td class="tt-pgroup">
                                            <a href="#">Group 1</a>,
                          <a href="#">Group 2</a>
                                        </td>
                                        <td class="tt-action table-action">
                                            <a href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="Edit"><span class="icon-tt-pencil"></span></a>
                                            <a href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="View"><span class="icon-tt-link"></span></a>
                                            <a href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="Delete"><span class="icon-tt-trash"></span></a>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="tt-name"><span class="tt-user-status offline">&#9679;</span> Aileen</td>
                                        <td class="tt-progress">
                                            <div class="nice radius alert progress"><span class="meter" style="width: 20%"></span><span class="text">20%</span></div>
                                        </td>
                                        <td class="tt-email"><a href="mailto:email@email.com">email@email.com</a></td>
                                        <td class="tt-phone"><a href="tel:+12356789">091231232</a></td>
                                        <td class="tt-access">
                                            <select>
                                                <option value="">Evaluation</option>
                                                <option value="">View Only</option>
                                                <option value="">Others</option>
                                                <option value="">No Role</option>

                                            </select>
                                        </td>
                                        <td class="tt-role">
                                            <select>
                                                <option value="">Project Organizer</option>
                                                <option value="">Workgroup Member</option>
                                            </select>
                                        </td>
                                        <td class="tt-pgroup">
                                            <a href="#">Group 1</a>,
                          <a href="#">Group 2</a>
                                        </td>
                                        <td class="tt-action table-action">
                                            <a href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="Edit"><span class="icon-tt-pencil"></span></a>
                                            <a href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="View"><span class="icon-tt-link"></span></a>
                                            <a href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="Delete"><span class="icon-tt-trash"></span></a>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td class="tt-name"><span class="tt-user-status online">&#9679;</span> Mike Pogi</td>
                                        <td class="tt-progress">
                                            <div class="nice radius success progress"><span class="meter" style="width: 60%"></span><span class="text">60%</span></div>
                                        </td>
                                        <td class="tt-email"><a href="mailto:email@email.com">email@email.com</a></td>
                                        <td class="tt-phone"><a href="tel:+12356789">091231232</a></td>
                                        <td class="tt-access">
                                            <select>
                                                <option value="">Evaluation</option>
                                                <option value="">View Only</option>
                                                <option value="">Others</option>
                                                <option value="">No Role</option>

                                            </select>
                                        </td>
                                        <td class="tt-role">
                                            <select>
                                                <option value="">Project Organizer</option>
                                                <option value="">Workgroup Member</option>
                                            </select>
                                        </td>
                                        <td class="tt-pgroup">
                                            <a href="#">Group 1</a>,
                          <a href="#">Group 2</a>
                                        </td>
                                        <td class="tt-action table-action">
                                            <a href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="Edit"><span class="icon-tt-pencil"></span></a>
                                            <a href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="View"><span class="icon-tt-link"></span></a>
                                            <a href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="Delete"><span class="icon-tt-trash"></span></a>
                                        </td>
                                    </tr>

                                </tbody>
                            </table>

                        </div>
                    </div>
                </div>
            </div>

            <div class="large-3 columns tt-comments-wrap hide tt-right-col">
                <!-- sidebar-->
                <%@ Register TagPrefix="MainPages" TagName="SidebarRight" Src="~/Pages/sidebar.ascx" %>
                <MainPages:SidebarRight ID="SidebarRight" runat="server" />

            </div>
        </div>

        <% }
           else
           { %>
        <div class="row">
            <div class="large-12 columns text-center">
                <%@ Register TagPrefix="includes" TagName="loginMessage" Src="~/Pages/includes/NeedToLogin.ascx" %>
                <includes:loginMessage ID="needToLogin" runat="server" />
            </div>
        </div>
        <%} %>
    </div>
</asp:Content>
