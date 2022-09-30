<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="Footer.ascx.vb" Inherits="AnytimeComparion.Footer" %>

<!-- Modal -->
<div class="modal fade transaction-detailModal1" tabindex="-1" role="dialog" aria-labelledby="transaction-detailModalLabel1" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered modal-lg" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="transaction-detailModalLabel1">Cluster Details</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body" id="current-cluster">
                <div id="accordionFlushExample"></div>
            </div>
        </div>
    </div>
</div>
<div class="modal fade transaction-detailModal2" tabindex="-1" role="dialog" aria-labelledby="transaction-detailModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered modal-lg" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="transaction-detailModalLabel2">Steps</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" id="btnclose"></button>
            </div>
            <div class="modal-body set-step-model-view">
                <ul class="list_popup set-step-model-view" id="stepparent">
                </ul>
            </div>
        </div>
    </div>
</div>
<!-- Model End -->

<!-- Footer -->
<footer class="footer">
    <div class="container-fluid">
        <div class="row">
            <div class="col-sm-6 col-lg-4 col-md-6 align-self-lg-center">
                <div class="align-items-center d-flex justify-content-center justify-content-lg-start">
                    <div class="">
                        <button type="button" class="foot_btn crtclst" data-bs-toggle="modal" data-bs-target=".transaction-detailModal1" id="btnCurrentCluster">
                            <i class="bx bx-sitemap font-size-16 align-middle me-2"></i>Current Cluster
                        </button>
                        <button type="button" class="foot_btn" data-bs-toggle="modal" data-bs-target=".transaction-detailModal2" id="btnstep">
                            <i class="bx bx bx-list-ul font-size-16 align-middle me-2"></i><span id="stepvalue"></span>
                        </button>
                    </div>
                </div>
            </div>
            <div class="col-sm-6 col-lg-3 col-md-6">
                <div class="align-items-center d-flex justify-content-center justify-content-lg-start mb-2 mt-2 m-lg-0">
                    <div class="progress">
                        <span class="title timer" data-from="0" data-to="85" data-speed="1800">85</span>
                        <div class="overlay"></div>
                        <div class="left"></div>
                        <div class="right"></div>
                    </div>
                    <span><strong>Evaluated:</strong> 10/17</span>
                </div>
            </div>
            <div class="col-sm-6 col-lg-5  col-md-12 align-self-lg-center">
                <div class="d-flex justify-content-center justify-content-lg-end">
                    <ul class="pagination pagination-sm mb-0" id="pgntn">
                    </ul>
                </div>
            </div>
        </div>
    </div>
</footer>

<!-- JAVASCRIPT -->
<%--<script src="/assets/libs/jquery/jquery.min.js"></script>--%>
<%--<script src="/assets/libs/bootstrap/js/bootstrap.bundle.min.js"></script>--%>


<script>
    var TotalPagecount = 0;
    var defaultValueActivepage = 1;
    var stepno = [];
    var i = 0;
    var hdnCurrentStep = 1, hdnTotalSteps = 1;

    $(document).ready(function () {
        hdnCurrentStep = $('#MainContent_hdnCurrentStep').val();
        hdnCurrentStep = hdnCurrentStep == undefined || null ? 1 : hdnCurrentStep;
        hdnTotalSteps = $('#MainContent_hdnTotalSteps').val();
        hdnTotalSteps = hdnTotalSteps == undefined || null ? 1 : hdnTotalSteps;

        debugger;
        loadStepsdata();
        setsteps(hdnCurrentStep);
    });

    function setsteps(activepage) {
        //$('#stepvalue').text("Step " + activepage + "/" + localStorage.getItem("totalpages"));
        $('#stepvalue').text("Step " + activepage + "/" + hdnTotalSteps);
    }

    function loadStepsdata() {
        var first = 1;
        var last = 0;
        $.ajax({
            type: "POST",
            url: baseUrl + "Default.aspx/loadStepList",
            data: JSON.stringify({ first: first, last: last }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: true,
            success: function (data) {
                var childItem = '';
                var firstArray = data.d.split('[')
                var finaldata = firstArray.map(r => r.replace('],', '').replace(']', '').replace(/['"`]/g, '').split(','))
                TotalPagecount = finaldata.length;
                //localStorage.setItem('totalpages', TotalPagecount);
                $.each(finaldata, function (i, item) {
                    var pageIndex = i;
                    if (localStorage.getItem("activepage") != null) {
                        //if (pageIndex == localStorage.getItem("activepage")) {
                        if (pageIndex == hdnCurrentStep) {
                            if (item[0] && item[0] != " ") childItem += "<li class='active' id='" + pageIndex + "' onclick=setPagination(" + pageIndex + ")>" + item[0] + "</li>";
                            //childItem += "<li class='active' id='" + pageIndex + "'>" + item[0] + "</li>";
                            //childItem += "<li class='active' id='" + pageIndex + "' onclick=setPagination(" + pageIndex + ")>" + item[0] + "</li>";
                            defaultValueActivepage = defaultValueActivepage + 1;
                        }
                        else {
                            if (item[0] && item[0] != " ") childItem += "<li id='" + pageIndex + "' onclick=setPagination(" + pageIndex + ")>" + item[0] + "</li>";
                            //childItem += "<li id='" + pageIndex + "'>" + item[0] + "</li>";
                            //childItem += "<li id='" + pageIndex + "' onclick=setPagination(" + pageIndex + ")>" + item[0] + "</li>";
                        }
                    }
                });
                $('#stepparent').html(childItem);
                setPagination();
            },
            error: function () {
                alert("error");
            }
        });
    }

    function setPagination(activePageValue = null) {
        var count = 0;
        //var activePage = activePageValue ? activePageValue : localStorage.getItem("activepage") ? parseInt(localStorage.getItem("activepage")) : 1;
        var activePage = activePageValue ? activePageValue : hdnCurrentStep ? parseInt(hdnCurrentStep) : 1;
        var displayPage = 9;
        var pageItems = "";
        var startIndex = 0;
        var endIndex = 0;
        //localStorage.setItem('activepage', activePage);
        //localStorage.setItem('currentclusteractivepage', activePage);
        sethighlightstepcrtclstr(activePage);

        if ((TotalPagecount - activePage) < (displayPage / 2)) {
            startIndex = TotalPagecount - displayPage;
            endIndex = TotalPagecount;
        }
        else if (activePage > (displayPage / 2)) {
            startIndex = (activePage - (displayPage / 2));
            endIndex = startIndex + displayPage;
        } else {
            startIndex = 1;
            endIndex = startIndex + displayPage;
        }
        if (activePage == 1) {
            pageItems += '<li class="page-item disabled"><a class="page-link" href="javascript:void(0);" tabindex="-1">Previous</a></li>'
        } else {
            pageItems += '<li class="page-item" onclick=previous()><a class="page-link" href="javascript:void(0);" tabindex="-1">Previous</a></li>'
        }
        for (var i = 0; i < TotalPagecount; i++) {
            count = count + 1;
            if (count >= startIndex && count <= endIndex) {
                if (activePage == count) {
                    pageItems += '<li class="page-item active"><a class="page-link" href="javascript:void(0);">' + count + '</a></li>';
                } else {
                    pageItems += '<li class="page-item" onclick=setPagination(' + count + ')><a class="page-link" href="javascript:void(0);">' + count + '</a></li>';
                    //pageItems += '<li class="page-item"><a class="page-link" href="#">' + count + '</a></li>';
                }
            }
        }
        if (activePage == count) {
            pageItems += '<li class="page-item disabled"><a class="page-link" href="javascript:void(0);">Next</a></li>'
        } else {
            pageItems += '<li class="page-item" onclick=next()><a class="page-link" href="javascript:void(0);">Next</a></li>'
        }
        setsteps(activePage);
        $('#pgntn').html(pageItems);

        if (activePage != hdnCurrentStep) {
            $('#MainContent_hdnPageNumber').val(activePage);
            $('#MainContent_hdnPageNo').trigger('click');
        }

        //saveCurrentPage(activePage);
        //$("[data-bs-dismiss=modal]").trigger({ type: "click" });
    }

    $("#btnstep").click(function () {
        var setdefaultval = 0;
        if (defaultValueActivepage == 2) {
            setdefaultval = defaultValueActivepage;
            setdefaultval = setdefaultval - 1;
        }
        loadStepsdata();
    });

    function previous() {
        var activeIndex = parseInt($(".page-item.active .page-link").text());
        setPagination(activeIndex - 1);
        //saveCurrentPage(activeIndex - 1);
        $('#MainContent_hdnPageNumber').val(activeIndex - 1);
        $('#MainContent_hdnPageNo').trigger('click');
    }

    function next(activePage) {
        var activeIndex = parseInt($(".page-item.active .page-link").text());
        setPagination(activeIndex + 1);
        //saveCurrentPage(activeIndex + 1);
        $('#MainContent_hdnPageNumber').val(activeIndex + 1);
        $('#MainContent_hdnPageNo').trigger('click');
    }

    function saveCurrentPage(activeIndex) {
        activeIndex = activeIndex == undefined || null ? 0 : activeIndex;
        $.ajax({
            type: "POST",
            url: baseUrl + "pages/Anytime/Anytime.aspx/setCurrentStep",
            data: JSON.stringify({
                stepNo: activeIndex
            }),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                window.location.reload();
            },
            error: function (response) {
                alert('Somwthing went wrong');
                window.location.reload();
            }
        });
    }

    var baseUrl = '<%= ResolveUrl("~/") %>';
    $("#btnCurrentCluster").click(function () {
        $.ajax({
            type: "POST",
            url: baseUrl + "Default.aspx/loadHierarchy",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: true,
            success: function (data) {
                var accordionItem = '';
                var steps = data.d.data;
                accordionItem = getStepesDate(steps, accordionItem);
                $("#accordionFlushExample").html(accordionItem);
                setPagination();
            },
            error: function () {
                alert("error");
            }
        });
    });

    function getStepesDate(steps, accordionItem) {
        accordionItem += '<ul class="tree" id="main-tab">'
        steps.forEach(element => {
            if (element.length == 6 && element[5].length > 0) {
                accordionItem += '<li class="treee_section">'
                accordionItem += '<input type="checkbox" id=group' + element[4] + ' checked />'
            } else {
                accordionItem += '<li class="tree_empty">'
            }
            //accordionItem += '<label  for=group' + element[4] + '><span id = "cluster-' + element[4] + '" class="sethovercolor spncls" style="color:#ed8f2b;" onclick=setPagination(' + element[4] + ')>' + element[1] + '<span style="color:#A9A9A9;">&nbsp;(#' + element[4] + ')</span></label>'
            accordionItem += '<label  for=group' + element[4] + '><span id = "cluster-' + element[4] + '" class="sethovercolor spncls" style="color:#ed8f2b;" >' + element[1] + '<span style="color:#A9A9A9;">&nbsp;(#' + element[4] + ')</span></label>'
            stepno[i++] = element[4];

            if (element.length == 6 && element[5].length > 0) {
                accordionItem = getStepesDate(element[5], accordionItem);
            }
            accordionItem += '</li>'
        });
        accordionItem += '</ul>'
        return accordionItem;
    }

    function sethighlightstepcrtclstr(actpage = 0) {
        var result = [];
        stepno.sort(function (a, b) {
            if (a > b) return 1;
            if (a < b) return -1;
            return 0;
        });

        for (var j = 1; j < stepno.length; j++) {
            result.push({ startpage: stepno[j - 1], endpage: stepno[j] });
        }
        result.push({ startpage: stepno[stepno.length - 1], endpage: TotalPagecount });
        var clusterpage = result.filter(x => actpage >= x.startpage && actpage < x.endpage);
        var sp = clusterpage.map(x => x.startpage);
        $('#main-tab #cluster-' + sp + '').addClass('activecurrentcluster');
    }

    function removeclass() {
        $('#main-tab li label span').removeClass('activecurrentcluster');
    }
</script>


