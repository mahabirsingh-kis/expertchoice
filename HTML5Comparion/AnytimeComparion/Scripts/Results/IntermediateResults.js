var intermediateResults = (function ($) {
    var baseUrl = $("#base_url").attr("href");

    return {
        init: init
    };

    function init() {
        //addHandlers();
        addHandlers();
    }

    function addHandlers() {
        $("#showMatrixTableButton").on("click", function () {
            $(".results-main-content").show();
            alert($(".results-main-content").length);
            //$("#threeInconsistency").effect("drop", {}, 200, function () {
            //    $(".results-main-content").fadeIn();
            //});
            //$.get(baseUrl + "pages/", function(data, status){
            //    template = _.template(data)({items: results});
            //   	$(".jg-result-list").html(template);
            //});
        });
    }


})($);
$(intermediateResults.init);
