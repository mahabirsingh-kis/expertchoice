function save_utility_curve(step, is_erase, flag, uctype) {
    is_comment_updated = true;
    comment = $('#txtRightComment_0').val();
    if (comment == '') {
        comment = output.comment;
    }
    var valtxt ="-2147483648000";
    if (uctype != 'Step') {        
        if ($('#uCurveInput').val() != '') {
            valtxt = $('#uCurveInput').val();
            $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
            $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)');
        }
        else {          
            $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
            $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
        }
    }
    else
    {
        if ($('#steps-functionInput').val() != '') {
            valtxt = $('#steps-functionInput').val();
        }       
    }
    if (!output.show_comments) {
        $('#ImgComment_iconEmpty_5').css('display', 'none');
        $('#ImgComment_icon_5').css('display', 'none');
    }
    else {
        HideShowComment_icon(5);
    }
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveUtilityCurve",
        data: JSON.stringify({
            step: hdnCurrentStep,
            value: valtxt,
            sComment: $('#txtRightComment_0').val(),
            uctype: uctype
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (flag) {
                //$('#MainContent_hdnPageNo').trigger('click');
                //$('#utilityCurveComment').removeClass('far fa-comments');
                //$('#utilityCurveComment').removeClass('fa fa-comments');

                //if ($('#commentBox').val() != '') {
                //    $('#utilityCurveComment').addClass('fa fa-comments');
                //}
                //else {
                //    $('#utilityCurveComment' + index).addClass('far fa-comments');
                //} 
                /*  hideNode(5, 5);*/
                //is_comment_updated = true;
                //comment = $('#commentBox').val();
                //HideShowComment_icon(5);
                //window.location.reload();
            }
            /*$('#index_' + index).addClass('active_row');*/
        },
        error: function (response) {
        }
    });
    block_Utilityunload_prompt = true;
}

function save_utility_curve_mobile(step, is_erase, flag, uctype) {
    is_comment_updated = true;
    comment = $('#txtRightComment').val();
    if (comment == '') {
        comment = output.comment;
    }
    if (uctype != 'Step') {
        if ($('#uCurveInput').val() != '') {
            $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
            $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)');
        }
        else {
            $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
            $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
        }
    }
    if (!output.show_comments) {
        $('#ImgComment_iconEmpty_5').css('display', 'none');
        $('#ImgComment_icon_5').css('display', 'none');
    }
    else {
        /*HideShowComment_icon(5);*/
        if (!isMobile)
            HideShowComment_icon(5);
        else
            HideShowComment_icon_mobile(5);
    }
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveUtilityCurve",
        data: JSON.stringify({
            step: hdnCurrentStep,
            value: (uctype == "Step") ? $('#steps-functionInput').val() : $('#uCurveInput').val(),
            sComment: $('#txtRightComment').val(),
            uctype: uctype
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (flag) {
                //$('#MainContent_hdnPageNo').trigger('click');
                //$('#utilityCurveComment').removeClass('far fa-comments');
                //$('#utilityCurveComment').removeClass('fa fa-comments');

                //if ($('#commentBox').val() != '') {
                //    $('#utilityCurveComment').addClass('fa fa-comments');
                //}
                //else {
                //    $('#utilityCurveComment' + index).addClass('far fa-comments');
                //} 
                /*  hideNode(5, 5);*/
                //is_comment_updated = true;
                //comment = $('#commentBox').val();
                //HideShowComment_icon(5);
                //window.location.reload();
            }
            /*$('#index_' + index).addClass('active_row');*/
        },
        error: function (response) {
        }
    });
    block_Utilityunload_prompt = true;
}


   
       
   


