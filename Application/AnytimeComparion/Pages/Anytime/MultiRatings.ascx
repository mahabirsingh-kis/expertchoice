<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="MultiRatings.ascx.vb" Inherits=".MultiRatings" %>
<style>
     
    .form-check-input{
        cursor:pointer;
    }

    .rating_scale_data {
        margin-top: 10px;
    }

   /*  #removeB .rating_scale_data,
     #removeB .page_heading_section > .container:nth-child(3) > .row .col-lg-6 {
    max-height: calc(100vh - 15em);
}
     #titleDiv{
         margin-bottom:10px;
     }*/
</style>
<script>
    $(document).ready(function () {
        
        setTimeout(function () {
            var removeptagdlength = $(".removeptagd").length;
            for (i = 0; i < removeptagdlength; i++) {
                var phtml = $('.removeptagd' + i).html().replaceAll('<p></p>', '');

                $('.removeptagd' + i).html('');
                $('.removeptagd' + i).html(phtml);
            }
            var removeptagdlengthw = $(".removeptagw").length;
            for (i = 0; i < removeptagdlengthw; i++) {
                var phtml = $('.removeptagw' + i).html().replaceAll('<p></p>', '');

                $('.removeptagw' + i).html('');
                $('.removeptagw' + i).html(phtml);
            }
        },500);
    });
</script>

<div id="divContent" class="" runat="server"></div>
<%--<asp:HiddenField ID="hdnmulti_non_pw_data" runat="server" Value="" />--%>
<asp:HiddenField ID="hdnactive_multi_index" runat="server" Value="" />
<%--<asp:HiddenField ID="hdnshowPriorityAndDirect" runat="server" Value="" />--%>
