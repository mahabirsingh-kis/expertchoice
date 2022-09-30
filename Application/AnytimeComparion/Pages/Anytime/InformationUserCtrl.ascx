<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="InformationUserCtrl.ascx.vb" Inherits=".InformationUserCtrl" %>

<style type="text/css">
    .bodycls {
        margin: auto;
        width: 100%;
        height: calc(100vh - 144px);
        margin-top: 0;
    }
    .inner_bodycls {
    min-height: 100%;
    height: auto;
        margin-top: 15px;
}
</style>
<div class="bodycls">
    <div class="align-items-center container d-flex inner_bodycls justify-content-center">        
        <div class="col-md-7">          
            <div id="dvInformation" runat="server">               
            </div>
        </div>
    </div>
</div>
