<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="AllPairWiseUserCtrl.ascx.vb" Inherits=".AllPairWiseUserCtrl" %>

<%@ Register Src="~/AnytimeComparion/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %>


<%@ Register Src="~/AnytimeComparion/Pages/includes/FramedInfodDocs.ascx" TagPrefix="includes" TagName="FramedInfodDocs" %>
<%@ Register Src="~/AnytimeComparion/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
<link href="../../CustomCSS/nouislider.css" rel="stylesheet" />
<link href="../../CustomCSS/nouislider.pips.css" rel="stylesheet" />
<link href="../../CustomCSS/nouislider.tooltips.css" rel="stylesheet" />
<%--<link href="../../CustomCSS/app.css" rel="stylesheet" />--%>
<link href="../../CustomCSS/pairwise.css" rel="stylesheet" />
<script src="../../../ckeditor/ckeditor.js"></script>

<style type="text/css">
    .bodycls {
        margin: auto;
        width: 100%;
        height: calc(100vh - 124px);
    }


    .top-first-row {
        top: 6px;
    }

    .noUi-handle{
        cursor:e-resize;
    }

    /*.noUi-origin{
        background-color: rgb(0, 88, 163);
    }*/

    
</style>
<script src="../../../Scripts/NoUI/nouislider.js"></script>
<script src="../../../Scripts/NoUI/nouislider.min.js"></script>
<script src="../../../Scripts/NoUI/wNumb.js"></script>
<%--<script src="../../CustomScripts/allPairWiseUserCtrl.js"></script>--%>
<%--<script src="../../CustomScripts/anytime.js"></script>--%>

<script>
    setTimeout(function () {

    },1000)
</script>

<div class="large-12 columns questionsWrap">
    <div id="question" runat="server">
        <includes:QuestionHeader runat="server" ID="QuestionHeader" />
    </div>

    <div class="columns tt-auto-resize-content">
        <includes:FramedInfodDocs runat="server" ID="FramedInfodDocs" />

        <div id="dvContent" class="divHeader header-padding" runat="server"></div>
    </div>

</div>

<div class="modal fade" id="GlobalInfoIconsModal" tabindex="-1" aria-labelledby="exampleModalToggleLabel" aria-hidden="true" runat="server">
    <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="PageContent_lblCaption">Edit Question</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <textarea id="GlobalInfoIconValue" name="GlobalInfoIconValue" rows="7"></textarea>
                <script type="text/javascript" lang="javascript">
                    //CKEDITOR.replace('GlobalInfoDocValue');
                    CKEDITOR.replace('GlobalInfoIconValue',
                        {
                            //toolbar: 'Basic', / this does the magic /
                            uiColor: '#9AB8F3',
                            height: 300,
                            allowedContent: true
                        });
                </script>
                <div runat="server" id="saveStatus" class="tt-GlobalInfoDoc-status"></div>
            </div>
            <div class="modal-footer">
                <%--<button type="button" id="btnmodelClose" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>--%>
                <button type="button" id="btnmodelSave" onclick="saveContent()" class="btn btn-primary">Save changes</button>
            </div>
        </div>
    </div>
</div>
<script>
    $(document).ready(function () {

        var oldhtml = $('.txtStepTask').html();
        var newhtml = oldhtml.replaceAll('<p></p>', '');
        $('.txtStepTask').html(newhtml);
    });
</script>
