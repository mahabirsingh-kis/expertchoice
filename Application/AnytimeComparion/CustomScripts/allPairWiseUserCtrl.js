var node_type, node_location, current_step, node_id, node_guid;

function showInfoPopup(type, location, step, nodeId, guid) {
    
    node_type = type;
    node_location = node_location;
    current_step = step;
    node_id = nodeId;
    node_guid = guid;
    $('#MainContent_AllPairWiseControl_GlobalInfoIconsModal').modal('show');
    CKEDITOR.instances.GlobalInfoDocValue.setData('');
    setTimeout(function () {
        //CKEDITOR.instances['GlobalInfoIconValue'].insertText("jhghj");
    }, 1000);
}

function btnApplyChanges() {
    
    try {
        //var temp_div = $("<div>");
        //temp_div.html(tinymce.activeEditor.getContent()).find(".tt-modal-header").removeClass("hide");
        var editors_content = $(".cke_editable p").innerHTML;
    }
    catch (e) {
       // var editors_content = tinymce.activeEditor.getContent();
    }

    //if (editors_content.indexOf("<p>") == -1) {
    //    editors_content = editors_content;
    //}
    node_guid = node_guid ? node_guid : 0;
    $.ajax({
        type: "POST",
        url: baseUrl + "Anytime.aspx/SaveInfoDocs",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({
            nodetxt: editors_content,
            obj: node_type,
            node: node_location,
            current_step: current_step,
            node_id: node_id,
            node_guid: node_guid
        }),
        beforeSend: function () {
            //$(".tt-loading-icon-wrap").show();
            //$(".save-info-doc-btn").html("Saving...");
        },
        success: function (data) {
            debugger
            //if (node_type == -1) {
            //    var node_name = "parent-node";
            //} else if (node_type == 2) {
            //    if (node_location == 1) {
            //        var node_name = "left-node";
            //    } else {
            //        var node_name = "right-node";
            //    }
            //} else if (node_type == 3) {
            //    if (node_location == 1) {
            //        var node_name = "wrt-left-node";
            //    } else {
            //        var node_name = "wrt-right-node";
            //    }
            //} else if (node_type == 4) {
            //    var node_name = "scale-node";
            //} else {
            //    node_name = "question-node";
            //}

            //var main_scope = angular.element($("#main-body")).scope();
            ////var at_scope = angular.element($("#anytime-page")).scope();
            //var at_scope;

            //if (is_anytime) {
            //    at_scope = angular.element($("#anytime-page")).scope();
            //} else {
            //    at_scope = angular.element($("#TeamTimeDiv")).scope();
            //}

            //if (node_id != 0 || is_multi) {
            //    try {
            //        var multi_node_name = "";
            //        multi_node_name = node_name.replace("right-", "");
            //        multi_node_name = multi_node_name.replace("left-", "");
            //    } catch (e) {

            //    }

            //    if (is_info_tooltip) {
            //        if (is_multi && node_name != "parent-node") {
            //            var element_name = "." + node_name + "-" + node_id + "-" + multi_index + "-tooltip span";
            //            $("." + node_name + "-" + multi_index + "-info-text").html(editors_content);
            //            $("." + node_name + "-" + node_id + "-" + multi_index + "-tooltip").click();
            //        } else {
            //            var element_name = "." + node_name + "-tooltip span";
            //            $("." + node_name + "-info-text").html(editors_content);
            //            $("." + node_name + "-tooltip").click();
            //        }

            //        if (!is_html_empty_js(editors_content)) {
            //            $(element_name).removeClass("disabled");
            //            $(element_name).addClass("not-disabled");
            //        } else {
            //            $(element_name).removeClass("not-disabled");
            //            $(element_name).addClass("disabled");
            //        }
            //    } else {
            //        //if framed doc mode

            //        $("." + node_name + "-info-text").html(editors_content);
            //    }

            //} else {
            //    if (node_name == "question-node") {
            //        //do nothing
            //    }
            //    else {
            //        if (is_info_tooltip) {
            //            if (!is_html_empty_js(editors_content)) {
            //                $("." + node_name + "-tooltip span").removeClass("disabled");
            //                $("." + node_name + "-tooltip span").addClass("not-disabled");
            //            } else {
            //                $("." + node_name + "-tooltip span").removeClass("not-disabled");
            //                $("." + node_name + "-tooltip span").addClass("disabled");
            //            }

            //            $("." + node_name + "-tooltip").click();
            //        }

            //        $("." + node_name + "-info-text").html(editors_content);
            //    }
            //}

            //try {
            //    if (is_anytime) {
            //        if (node_name == "question-node") {
            //            if (node_type == 0 && !apply_to) {
            //                //$(".step-" + current_step).click();
            //                main_scope.MoveStepat(e, at_scope.current_step, false, at_scope.is_AT_owner, false, "question");
            //            }
            //        } else {
            //            if (is_multi) {
            //                info_doc_index = at_scope.active_multi_index;
            //            }

            //            //main_scope.MoveStepat(e, at_scope.current_step, false, at_scope.is_AT_owner, false, "infodoc");
            //            if (node_type == -1) {
            //                atScope.output.parent_node_info = editorContent;
            //            } else if (node_type == 2) {
            //                changeLeftOrRightInfoDocs(at_scope, node_name, editors_content);
            //            } else if (node_type == 3) {
            //                changeWrtLeftOrRightInfoDocs(at_scope, node_name, editors_content);
            //            } else if (node_type == 4) {
            //                if (main_scope.output.non_pw_type == "mtRatings") {
            //                    at_scope.output.ScaleDescriptions[info_doc_index == -1 ? 0 : info_doc_index].Description = editors_content;
            //                }
            //            }

            //            at_scope.reflow_equalizer();
            //        }
            //    } else {
            //        at_scope.update_users_list();
            //    }
            //}
            //catch (e) {

            //}
        },
        complete: function () {

            //$(".tt-loading-icon-wrap").hide();
            //$(".save-info-doc-btn").html('<span class="icon-tt-save icon"></span><span class="text">Save</span>');
            //$("#GlobalInfoDocModal .success").show();
            ////setTimeout(function(){
            //$("#GlobalInfoDocModal").foundation("reveal", "close");
            //}, 2000);
        },
        error: function (response) {
            console.log(response);
            //$(".success").hide();
            //$("#GlobalInfoDocModal .alert-box .alert").show();
        }
    });

}