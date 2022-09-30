var Sswitched = false;
var fJudgmentTableExist = false;

function updateTables(hidden) {
    
        //alert(Sswitched);
        if (($(window).width() < 767) && !Sswitched) {
            Sswitched = true;
            if (hidden) {
                $("#hiddentable table.responsive").each(function (i, element) {
                    console.log(i);
                    if (i == 0)
                        splitTable($(element));
                    
                });
                $("#hiddentable .pinned tbody tr").height(30);
                return $("#hiddentable").html();
            }
            else {
                $("table.responsive").each(function (i, element) {
                    console.log(i);
                    if (i == 0)
                        splitTable($(element));
                });
            }

            return true;
        }
        else if (Sswitched && ($(window).width() > 767)) {
            
            Sswitched = false;
            
            $("table.responsive").each(function (i, element) {
                unsplitTable($(element));
            });
        }




};

//$(window).load(updateTables());
//$(window).on("redraw", function () { Sswitched = false; updateTables(); }); // An event to listen for
//$(window).on("resize", function () {
//    Sswitched = true;
//    updateTables()
//});

//$(window).on('load', function () {
    // Only wire up the resize handler after loading is complete to prevent fire of resize before page is loaded.
$(window).on('resize', function () {
    if (fJudgmentTableExist)
        updateTables();
    });
//});

function splitTable(original) {
    var tablewrapper = $(".table-wrapper").length;
    //if (tablewrapper > 0) {
    //    original.closest(".table-wrapper").find(".pinned").remove();
    //    original.unwrap();
    //}
    original.wrap("<div class='table-wrapper' />");
    var copy = original.clone();
    copy.find("td:not(:first-child), th:not(:first-child)").css("display", "none");
    copy.removeClass("responsive");

    original.closest(".table-wrapper").append(copy);
    copy.wrap("<div class='pinned' />");
    original.wrap("<div class='scrollable' />");

    setCellHeights(original, copy);
}

function unsplitTable(original) {
    var tablewrapper = $(".table-wrapper").length;
    if (tablewrapper > 0) {
        original.closest(".table-wrapper").find(".pinned").remove();
        original.unwrap();
    }
}

function setCellHeights(original, copy) {
    var tr = original.find('tr'),
        tr_copy = copy.find('tr'),
        heights = [];

    tr.each(function (index) {
        var self = $(this),
            tx = self.find('th, td');

        tx.each(function () {
            var height = $(this).outerHeight(true);
            heights[index] = heights[index] || 0;
            if (height > heights[index]) heights[index] = height;
        });

    });

    tr_copy.each(function (index) {
        $(this).height(heights[index]);
    });
}