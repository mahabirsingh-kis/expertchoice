var timer = "";

function update(param, stepGuId, status, mtype, baseUrl, allusers, userEmail, index) {
    //show loading icon
    console.log('1: Preparing Judgment ');
    reset_btn = index;
    clearInterval(intervalID);
    clearInterval(timer_interval);
    $('#last_judgment_div').show();
    updateTimeSinceLastJudgment();

    saveJudgment(param, stepGuId, status, mtype, baseUrl, allusers, userEmail, index);
}

function loadJudgment(param, status, mtype, baseUrl, allusers, userEmail, index) {
    var DBDatas;
    $.ajax({
        type: "POST",
        url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/loadTeamTime",
//        url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx",
        contentType: "application/json; charset=utf-8",
        data: "{}",
        beforeSend: function (response) {
        },
        success: function (data) {
            setInterval(timer, 1000);
            DBDatas = eval("[" + data.d + "]");
            DBDatas = DBDatas[0];
            //var dbarr = DBDatas.toString();
            js = DBDatas.data.data;
            console.log(DBDatas.data.data[2][0]);
        },
        error: function (response) {
            console.log(response);
        }
    });
}

function saveJudgment(param, stepGuId, status, mtype, baseUrl, allusers, userEmail, index) {
    var semail = "";
    var save_val;
    //below is for testing.
    var sguid = "";
    var scomment = '';
    var options = ["anonymous", "group", "hide", "individual", "infodoc-size", "node", "pagination", "pie-chart", "offline", "wrtID", "single_line"];

    if ($.inArray(status, options) >= 0) {
        sname = "";
        sguid = "";
        save_val = param;
    }
    if (status == "comment") {

        //sguid = DBDatas.data.stepGuid;
        //js = DBDatas.data.data;
        sname = userEmail;
        save_val = param;
        sguid = stepGuId;
    }

    if (status == "save") {
        $('.tt-loading-icon-wrap').show();
        //        console.log('show loading app.js');
        save_val = param[0];
        semail = param[1];
        scomment = param[2];
        sguid = stepGuId;

        $("#reset_" + reset_btn).hide();
        $(".pending_" + reset_btn).show();

        //var cheat;

        //sguid = DBDatas.data.stepGuid;
        //js = DBDatas.data.data;

        //for (iter = 0; iter < allusers; iter++) {
        //    if (mtype == "mtRatings" || mtype == "mtRegularUtilityCurve")
        //    {

        //    }
        //    else
        //    {
        //        if (param[1] != null) {
        //            if (js[1][iter][1] == param[1]) {
        //                js[1][iter][5] = param[0];
        //                semail = param[1];
        //            }
        //        }
        //        else {
        //            if (js[1][iter][1] == userEmail) {
        //                js[1][iter][5] = param[0];
        //                cheat = js[1][iter];
        //            }
        //        }
        //    }
        //}
    }

    console.log('2. Saving Judgment with value: ' + save_val);
    $.ajax({
        type: "POST",
        url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/setjson",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({
            status: status,
            svalue: save_val,
            sname: semail,
            sguid: sguid,
            scomment: scomment
        }),
        success: function (data) {
            loadJudgment(param, status, mtype, baseUrl, allusers, userEmail, index);
            if (status == "save") {
                //if(mtype == "ptVerbals"){
                //    if(cheat){
                //        $("li[id*='" + cheat[1] + "']").removeClass("active");
                //        if (cheat[5] > -9){
                //            for (i = 0; i <= Math.abs(cheat[5]); i++) {

                //                if (cheat[5] <= 0) {
                //                    if (i==0){
                //                        var c = document.getElementById(i + cheat[1]);
                //                        c.className = c.className + " active";
                //                    }
                //                    else{
                //                        var c = document.getElementById(i + cheat[1]);
                //                        c.className = c.className + " active";
                //                    }

                //                }//if (max <= 0) end
                //                if (cheat[5] > 0)  {
                //                    var c = document.getElementById(i + cheat[1]);
                //                    c.className = c.className + " active";
                //                }
                //            }
                //        }
                //    }
                //}


                //$('#resbars').prop('title', (PipeVal[2] * 1).toFixed(2));
                //show rest button
                $(".pendingIndicator").hide();
                $(".reset-btn").show();

                console.log('Judgment Completed');
            }
        },
        error: function (response) {
            loadJudgment(param, status, mtype, baseUrl, allusers, userEmail, index);
            console.log(response);
            setTimeout(function () {
                console.log('hide loading PairWiseComaprison.ascx - error line 1262');
                $('.tt-loading-icon-wrap').fadeOut(); //hide loading icon
            }, 2000);

        }
    });
}

function resizeCurveHandle($dragHandle, $handlePos) {
    var strJSON = $handlePos;
    var startTop = -37;
    var startHeight = 55;

    var newHeight = startHeight + (strJSON.left / 2);
    var newTop = startTop - (startHeight + strJSON.left / 2) + 55;
    $dragHandle.css({
        "height": (newHeight),
        "top": (newTop)
    });

    return false;
}

function save_dropdown(node, has_class, url)
{
    //parent
    if (node == "0") {
        update(has_class + "_0", 'node', '', url);
    }
    //left node
    if (node == "1") {
        update(has_class + "_1", 'node', '', url);
    }
    //wrt-left
    if (node == "3") {
        update(has_class + "_3", 'node', '', url);
    }
    //right node
    if (node == "2") {
        update(has_class + "_2", 'node', '', url);
    }
    //wrt-right
    if (node == "4") {
        update(has_class + "_4", 'node', '', url);
    }
}

function update_users_list(user_id, users_list, mtype)
{
    if (users_list.indexOf(user_id) < 0) {


        if (mtype == 'mtRegularUtilityCurve') {
            var $clone = $("#UserDivClone").clone();
            $clone.insertAfter("li.users-list-div:last");
            $clone.id = "";
            var divs = $clone.find(".active");
            divs.addClass("tg-idr");
            divs.removeClass("active");
            var user_display = $clone.find(".tt-nameAndEmail");
            user_display.removeClass(function (index, css) {
                return (css.match(/(^|\s)userdisplay\S+/g) || []).join(' ');
            });
            user_display.addClass("userdisplay" + user_id);

            var input_curve = $clone.find(".input-curve");
            input_curve.attr("id","InputCurve" + user_id);
            input_curve.attr("data-email", "ena");
            input_curve.addClass("judgment-results-div");
            input_curve.on("keypress", function (e) {
                var code = e.keyCode || e.which;
                if (code == 13) {
                    var input_value = this.value;
                    var input_email = $(this).attr("data-email");
                    input_curve_update(e, input_value, input_email);
                    return false;
                }
                else if (code >= 58 || code < 48 && code != 46) {
                    e.preventDefault();
                }
                return true;
            });
            trigger_update = 1;
            //priority
            var priority = $clone.find(".priority-sign");
            priority.addClass("judgment-results-div");
            priority.removeClass(function (index, css) {
                return (css.match(/(^|\s)curve-priority-\S+/g) || []).join(' ');
            });
            priority.addClass("curve-priority-" + user_id);
            //progess-bar
            var progress = $clone.find(".progress-sign");
            progress.addClass("judgment-results-div");
            progress.removeClass(function (index, css) {
                return (css.match(/(^|\s)curve-bar-\S+/g) || []).join(' ');
            });
            progress.addClass("curve-bar-" + user_id);
        }
    }
}

