<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="start.aspx.cs" Inherits="AnytimeComparion.test.start" %>

 <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="tt-body tt-myprojects-wrap">
        <center> Please login as the admin to view the correct "my projects" page</center>
    </div>
     <script type="text/javascript">
         $(document).ready(function () {
             $("#PasscodeLink").focus(function () { this.select(); });
         });

         function CopyToClipboard() {
             var controlValue = document.getElementById('PasscodeLink');
             $('#PasscodeLink').select();
         }

         function showlink(ID) {
             
             $.ajax({
                 type: "POST",
                 url: baseUrl + "Pages/TeamTimeTest/start.aspx/GetLink",
                 contentType: "application/json; charset=utf-8",
                 data: JSON.stringify({
                     ProjectID: ID
                 }),
                 success: function (data) {
                     $("#PasscodeLink").val('<%=(Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath).ToString() + "?passcode="%>' + data.d)
                 },
                 error: function (response) {
                     console.log(response);
                 }
             });
         }



         <%foreach (ExpertChoice.Data.clsProject project in App.DBProjectsAll())
           {
              foreach (ExpertChoice.Data.clsProject projectall in App.DBProjectsAll())
              {
                 if(project.ID == projectall.ID)
                 {
                   int id = project.ID;
                   if (project.isOnline)
                   {%>
                         if ($("#switch-" + <%=id%>).length > 0) {
                             $("#switch-" + <%=id%>).prop('checked', true);
                         }
                 <%}
                }
             }
             }%>


         <%foreach (ExpertChoice.Data.clsWorkgroup Workgroup in App.DBWorkgroupsAll())
           {
               int id = Workgroup.ID;
               if(App.ActiveWorkgroup != null){
                   if (Workgroup.Name == App.ActiveWorkgroup.Name)
                     {
                   %>
         $("#workgroup").val('<%=App.ActiveWorkgroup.Name%>');
               
                
          <%}} }%>


         function displayStatus(id)
         {
             projid = id.split("-");
             var baseUrl = '<%= ResolveUrl("~/") %>';

             $.ajax({
                 type: "POST",
                 url: baseUrl + "Pages/TeamTimeTest/start.aspx/displayStatus",
                 contentType: "application/json; charset=utf-8",
                 data: JSON.stringify({
                     projid: projid[1]
                 }),
                 success: function (data) {
                     if(data.d)
                         $("#switch-" + id).prop('checked', true);
                     else
                         $("#switch-" + id).prop('checked', false);
                 },
                 error: function (response) {
                     console.log(response);
                 }
             });
         }

         function ProcessProject(ProjectID, status) {
             var baseUrl = '<%= ResolveUrl("~/") %>';
             $.ajax({
                 type: "POST",
                 url: baseUrl + "Pages/TeamTimeTest/start.aspx/CheckProject",
                 contentType: "application/json; charset=utf-8",
                 data: JSON.stringify({
                     ProjectID: ProjectID,
                     Status: status
                 }),
                 success: function (data) {
                     if(data.d){
                         window.location.href = baseUrl;
                     }
                     else{
                         __doPostBack('__Page', '');
                     }
                 },
                 error: function (response) {
                     console.log(response);
                 }
             });
         }

         $('#workgroup').on('change', function() {
             var baseUrl = '<%= ResolveUrl("~/") %>';
             $.ajax({
                 type: "POST",
                 url: baseUrl + "Pages/TeamTimeTest/start.aspx/setWorkgroup",
                 contentType: "application/json; charset=utf-8",
                 data: JSON.stringify({
                     sVal: this.value
                 }),
                 success: function (data) {
                     $(".workgroups").prop('selected', false);
                     __doPostBack('__Page', '');
                 },
                 error: function (response) {
                     console.log(response);
                 }
             });
         });
     </script>         
  </asp:Content>