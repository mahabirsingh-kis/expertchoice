$(document).ready(function () {
    /**-- clear judgments --**/
    $(document).on('click', '.clrPairwise', Foundation.utils.debounce(function (e) {
        var $this = $(this);//$('.clrPairwise');
        $this.hide();
        $('.lft-eq').removeClass('active-selected');
        $('.rgt-eq').removeClass('active-selected');
        $('.lft-eq').removeClass('lft-selected');
        $('.rgt-eq').removeClass('rgt-selected');
        $('.rs').prop('checked', false);
    }, 200, true));
    $(document).on('click', '.clrPairwiseResult', Foundation.utils.debounce(function (e) {

        var id = $(this).attr('id');
        var $this = $(this);//$('.clrPairwiseResult');

        $this.hide();
        $('.ownerResult-' + id).removeClass('active');
        console.log(id);

    }, 200, true));
    //*****END VERBAL MOBILE *****//


    //*****START VERBAL DESKTOP  *****//
    //**-- bar animations--**// 
    $(document).on('mouseout', '.lft-eq', Foundation.utils.debounce(function (e) {
        $('.rgt-eq').removeClass('active');
        $('.lft-eq').removeClass('active');

    }, 0, true));

    $(document).on('mouseenter', '.lft-eq', Foundation.utils.debounce(function (e) {
        var id = $(this).attr('id');
        $('.rgt-eq').removeClass('active');
        $('.lft-eq').removeClass('active');

        animateThis(id, 'lft');
    }, 0, true));

    $(document).on('mouseout', '.rgt-eq', Foundation.utils.debounce(function (e) {

        $('.lft-eq').removeClass('active');

        $('.rgt-eq').removeClass('active');

    }, 0, true));

    $(document).on('mouseenter', '.rgt-eq', Foundation.utils.debounce(function (e) {
        var id = $(this).attr('id');
        var pos = $(this).attr("data-pos");
        $('.rgt-eq').removeClass('active');
        $('.lft-eq').removeClass('active');
        animateThis(id, pos);
    }, 0, true));

    //**-- remove hover effects --**//
    $(document).on('mousemove', 'body', Foundation.utils.debounce(function (e) {
        if ($('ul.tt-equalizer:hover').length > 0) {
        } else {
            $('.rgt-eq').removeClass('active');
            $('.lft-eq').removeClass('active');

        }
    }, 0, true));
    //*****END VERBAL DESKTOP *****//


    //*****START VERBAL MOBILE *****//

    /**-- bar clickables --**/
    $(document).on('click', '.lft-eq', Foundation.utils.debounce(function (e) {
        var id = $(this).attr('id');
        var title = $(this).attr('title');
        var pos = $(this).attr("data-pos");

        $('.lft-eq').removeClass('active-selected');
        $('.rgt-eq').removeClass('active-selected');
        markThis(id, pos);

        //updSelectedItem(pos, title);
        //                console.log("left:" + title);

        $('.tt-j-clear, .tt-action-btn-clr-j').fadeIn();
    }, 200, true));

    $(document).on('click', '.rgt-eq', Foundation.utils.debounce(function (e) {
        var id = $(this).attr('id');
        var title = $(this).attr('title');
        var pos = $(this).attr("data-pos");
        $('.lft-eq').removeClass('active-selected');
        $('.rgt-eq').removeClass('active-selected');
        markThis(id, pos);
        //updSelectedItem(pos, title);
        //                console.log("right:" + title);
        $('.tt-j-clear, .tt-action-btn-clr-j').fadeIn();

    }, 200, true));
});

function animateThis(id, pos) {
    var text = "";
    var i;
    for (i = id; i < 10; i++) {
        $('.' + pos + '-' + i).addClass('active');
    }
}
function markThis(id, pos) {
    var i;
    for (i = id; i < 10; i++) {
        $('.' + pos + '-' + i).removeClass('active-selected');
        $('.' + pos + '-' + i).addClass('active-selected');
    }
    //remove initial class on click
    if (pos != "mid") {
        $('.mid').removeClass('rgt-selected');
        $('.mid').removeClass('lft-selected');
        $('.mid').removeClass('active-selected');
    }

    //then add class based on clicked bars
    $('.mid').addClass(pos + '-selected');
}


