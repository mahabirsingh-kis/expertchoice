//***** START live edit*****//
//****GLOBAL VARS****/
//info docs start
var is_info_tooltip = false;
var node_type = ""; // -1 if parent, 2 if laternative then 3 if wrt
var node_location = "";
var node = ""; // 1 - left, 2 -right
var current_step = 1;
var sRes = "";
var cluster_phrase = "";
var is_direct = false;
var is_ratings = false;
var is_pairwise = false;
var output_parent_node = "";
var output_left_node = "";
var output_right_node = "";
var output_scale_node = "";
var temp_question = "";
var templates = [];
var node_id = 0;
var node_guid = 0;
var multi_index = 0;
var is_anytime = false;
var nodes_data = [];
var apply_to = false;
var custom_css = ".node_name, .task_bold, #wrt_name{font-weight:bold;}.wrt_path{display:none}.tt-content-wrap .tt-body .tt-question-title .question-node-info-text p, .tt-content-wrap .tt-body .tt-question-title .question-node-info-text span {color: #539ddd;}";
var quick_help = "";
var temp_iframe_width = "100%";
var draft = $(".LblTask").html();
var node_ids = [];

var is_multi = false;
var info_doc_index = -1;
var isTinyMceReadonly = false;
//****END OF GLOBAL VARS****/

$(document).ready(function () {

    $(document).on("click", ".editable-trigger", Foundation.utils.debounce(function (e) {
        var $id = $(this).attr("id");

        $(".eiw-" + $id).show();
        $(".ei-" + $id).focus();
        $(".hide-when-editing-" + $id).hide();
        //$(".ei-"+$id).val("");

        //        var currentValue = $(this).text();
        //$(".ei-"+$id).val(currentValue); //get current value and apply to the input
        //console.log($id);
    }, 300, true));


    $(document).on("click", ".cancel-wrt-btn", Foundation.utils.debounce(function (e) {
        var $id = $(this).attr("id");
        $(".eiw-" + $id).hide();
        $(".hide-when-editing-" + $id).show();
        $(".ei-" + $id).val("");// clear value

    }, 300, true));

    $(document).on("click", ".save-wrt-btn", Foundation.utils.debounce(function (e) {
        var $id = $(this).attr("id");
        $(".eiw-" + $id).hide();
        $(".hide-when-editing-" + $id).show();

        setTimeout(function () {
            var editedValue = $(".ei-" + $id).val();
            if (editedValue) {
                $(".et-" + $id).css("color", "black");
                $(".et-" + $id).html(editedValue);
            }
            else {
                $(".et-" + $id).html("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            }

        }, 500);


    }, 300, true));
    //***** END live edit*****//

    
    function init_tinymce() {
        //$(".fullwidth-loading-wrap").removeClass("hide");

        getTemplatesFromServer();

        //Need to increase this value if no templates doesn't show under Templates button
        //It's because getTemplatesFromServer is taking more time to get templates
        var timeoutForTinyMce = node_type == 0 ? 500 : 0; //Setting a timeout value when editing question

        setTimeout(function () {
            initizlizeTinyMce();
        }, timeoutForTinyMce);

        //setTimeout(function () {
        //    $(".fullwidth-loading-wrap").addClass("hide");
        //}, timeoutForTinyMce + 200);
    }

    function getTemplatesFromServer() {
        if (node_type == 0) {
            //Gets templates from server by AJAX call when editing question
            $.ajax({
                type: "POST",
                url: baseUrl + "Default.aspx/getTemplateValues",
                contentType: "application/json; charset=utf-8",
                success: function(data) {
                    templates = data.d;
                },
                error: function(response) {
                    console.log(response);
                }
            });
        }
    }

    function initizlizeTinyMce() {
        //default font
        var fonts = "Andale Mono=andale mono,times;" + "Arial=arial,helvetica,sans-serif;" + "Arial Black=arial black,avant garde;" + "Book Antiqua=book antiqua,palatino;" + "Comic Sans MS=comic sans ms,sans-serif;" + "Courier New=courier new,courier;" + "Georgia=georgia,palatino;" + "Helvetica=helvetica;" + "Impact=impact,chicago;" + "Symbol=symbol;" + "Tahoma=tahoma,arial,helvetica,sans-serif;" + "Terminal=terminal,monaco;" + "Times New Roman=times new roman,times;" + "Trebuchet MS=trebuchet ms,geneva;" + "Verdana=verdana,geneva;" + "Webdings=webdings;" + "Wingdings=wingdings,zapf dingbats";
        var font_sizes = "8pt 9pt 10pt 11pt 12pt 14pt 16pt 18pt 24pt 36pt";
        var colors = [
        "000000", "Black",
        "993300", "Burnt orange",
        "333300", "Dark olive",
        "003300", "Dark green",
        "003366", "Dark azure",
        "000080", "Navy Blue",
        "333399", "Indigo",
        "333333", "Very dark gray",
        "800000", "Maroon",
        "FF6600", "Orange",
        "808000", "Olive",
        "008000", "Green",
        "008080", "Teal",
        "0000FF", "Blue",
        "666699", "Grayish blue",
        "808080", "Gray",
        "FF0000", "Red",
        "FF9900", "Amber",
        "99CC00", "Yellow green",
        "339966", "Sea green",
        "33CCCC", "Turquoise",
        "3366FF", "Royal blue",
        "800080", "Purple",
        "999999", "Medium gray",
        "FF00FF", "Magenta",
        "FFCC00", "Gold",
        "FFFF00", "Yellow",
        "00FF00", "Lime",
        "00FFFF", "Aqua",
        "00CCFF", "Sky blue",
        "993366", "Red violet",
        "FFFFFF", "White",
        "FF99CC", "Pink",
        "FFCC99", "Peach",
        "FFFF99", "Light yellow",
        "CCFFCC", "Pale green",
        "CCFFFF", "Pale cyan",
        "99CCFF", "Light sky blue",
        "CC99FF", "Plum"
        ];

        if (node_type == 0) {
            //fonts = "Arial=arial,helvetica,sans-serif;"
            //font_sizes = "13pt";
            //colors = [
            //    "539ddd", "Light Sky Blue",
            //];
        }
        //console.log("Node Type: " + node_type);

        tinymce.init({
            color_picker_callback: function (callback, value) {
                if (node_type == 0) {
                    //alert("Default color will be used in this text.")
                    // return false;
                }
            },
            textcolor_map: colors,
            selector: "#GlobalInfoDocValue",
            content_style: custom_css,
            theme: "silver",
            font_formats: fonts,
            fontsize_formats: font_sizes,
            plugins: "print preview fullpage searchreplace autolink directionality visualblocks visualchars fullscreen image link media table charmap hr pagebreak nonbreaking anchor insertdatetime advlist lists imagetools textpattern help",
            //plugins: [
            //    "advlist autolink autosave link image lists charmap print preview hr anchor pagebreak spellchecker", "searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking imagetools", "table contextmenu directionality emoticons textcolor paste fullpage textcolor colorpicker textpattern imagetools"
            //],
            toolbar: isTinyMceReadonly ? false : [
                "undo redo | copy cut paste | formatselect fontselect fontsizeselect forecolor backcolor | removeformat | bold italic underline strikethrough subscript superscript | alignleft aligncenter alignright alignjustify | outdent indent | bullist numlist table link image media |custombutton1 custombutton2"
            ],
            //toolbar1: "fontselect fontsizeselect | bold italic underline strikethrough | forecolor backcolor| alignleft aligncenter alignright alignjustify | bullist numlist | outdent indent | media",
            //toolbar2: "table | hr removeformat | subscript superscript charmap pagebreak | templateButton resettodefault editcaption",
            menubar: !isTinyMceReadonly,
            branding: false,
            image_advtab: true,
            setup: function (editor) {

                try {
                    var $textarea = $("#" + editor.id);
                    draft = $textarea.val();

                }
                catch (e) {

                }


                editor.on("change", function (e) {
                    draft = editor.getContent();
                });


                editor.on("init", function () {

                    if (node_type == 0) {
                        //this.getDoc().body.style.fontSize = "17px";
                        //this.getDoc().body.style.fontFamily = "Arial";
                        //this.getDoc().body.style.color = "#539ddd";
                    }
                    else {
                        this.getDoc().body.style.fontSize = "15px";
                        this.getDoc().body.style.fontFamily = "Arial";
                    }
                });


                var at_scope;
                if (is_anytime) {
                    at_scope = angular.element($("#anytime-page")).scope();
                } else {
                    at_scope = angular.element($("#TeamTimeDiv")).scope();
                }


                if (!at_scope.isMobile()) {
                    editor.ui.registry.addButton("resettodefault", {
                        text: "Upload Image",
                        icon: "image",
                        onAction: function () {
                            $("#GlobalImage").click();
                        }
                    });
                }


                if (node_type == -1) {
                    editor.ui.registry.addButton("editcaption", {
                        text: "Edit Caption",
                        icon: false,
                        onAction: function () {
                            open_edit_caption();
                            //("#GlobalImage").click();
                        }
                    });
                }

                if (node_type == 0) {
                    editor.ui.registry.addButton("resettodefault", {
                        text: "Reset to default",
                        icon: false,
                        onAction: function () {
                            var output = sRes.split("%%");
                            //add bold tags to %%
                            for (var i = 0; i < output.length; i++) {
                                if (i % 2 === 0) {
                                    if (i === output.length - 1) {
                                        temp_question += output[i];
                                    } else {
                                        temp_question += output[i] + "<b>%%";
                                    }
                                }
                                else {
                                    temp_question += output[i] + "%%</b>";
                                }
                            }

                            if (confirm("Do you really want to reset the question to the default wording?")) {
                                var formatted_sRes = "<span style='font-family: Arial; font-size: 1rem; font-weight: normal; line-height: 1.6;'>" + temp_question + "</span>";
                                editor.setContent(formatted_sRes);
                                temp_question = "";
                            } else {
                                //cancelled
                            }
                        }
                    });

                    //Adds Templates... button
                    addTemplatesButton(editor);

                    //var scope = angular.element($("#anytime-page")).scope();
                    var scope;
                    if (is_anytime) {
                        scope = angular.element($("#anytime-page")).scope();
                    } else {
                        scope = angular.element($("#TeamTimeDiv")).scope();
                    }


                    $(document).foundation("reflow");
                }

            },
            toolbar_items_size: "small",

            style_formats: [{
                title: "Bold text",
                inline: "b"
            }, {
                title: "Red text",
                inline: "span",
                styles: {
                    color: "#ff0000"
                }
            }, {
                title: "Red header",
                block: "h1",
                styles: {
                    color: "#ff0000"
                }
            }, {
                title: "Example 1",
                inline: "span",
                classes: "example1"
            }, {
                title: "Example 2",
                inline: "span",
                classes: "example2"
            }, {
                title: "Table styles"
            }, {
                title: "Table row 1",
                selector: "tr",
                classes: "tablerow1"
            }],
            imagetools_cors_hosts: ["www.tinymce.com", "codepen.io"],
            //toolbar: !isTinyMceReadonly,
            readonly: isTinyMceReadonly,
            statusbar: !isTinyMceReadonly,
            force_br_newlines: true,
            force_p_newlines: false,
            forced_root_block: "",
            /* enable title field in the Image dialog*/
            image_title: true,
            /* enable automatic uploads of images represented by blob or data URIs*/
            automatic_uploads: true,
            /*
              URL of our upload handler (for more details check: https://www.tiny.cloud/docs/configure/file-image-upload/#images_upload_url)
              images_upload_url: 'postAcceptor.php',
              here we add custom filepicker only to Image dialog
            */
            file_picker_types: "image",
            /* and here's our custom image picker*/
            file_picker_callback: function (cb, value, meta) {
                var input = document.createElement("input");
                input.setAttribute("type", "file");
                input.setAttribute("accept", "image/*");

                /*
                  Note: In modern browsers input[type="file"] is functional without
                  even adding it to the DOM, but that might not be the case in some older
                  or quirky browsers like IE, so you might want to add it to the DOM
                  just in case, and visually hide it. And do not forget do remove it
                  once you do not need it anymore.
                */

                input.onchange = function () {
                    var file = this.files[0];

                    var reader = new FileReader();
                    reader.onload = function () {
                        /*
                          Note: Now we need to register the blob in TinyMCEs image blob
                          registry. In the next release this part hopefully won't be
                          necessary, as we are looking to handle it internally.
                        */
                        var id = "blobid" + (new Date()).getTime();
                        var blobCache = tinymce.activeEditor.editorUpload.blobCache;
                        var base64 = reader.result.split(",")[1];
                        var blobInfo = blobCache.create(id, file, base64);
                        blobCache.add(blobInfo);

                        /* call the callback and populate the Title field with the file name */
                        cb(blobInfo.blobUri(), { title: file.name });
                    };
                    reader.readAsDataURL(file);
                };

                input.click();
            }
        });

        if (isTinyMceReadonly) {
            //setTimeout(function () {
            //    $("#GlobalInfoDocValue").closest(".reveal-modal").css("top", "20px");
            //}, 400);

            setTimeout(function () {
                //console.log($("#GlobalInfoDocValue").closest(".reveal-modal").height());
                $("#GlobalInfoDocModal .tox.tox-tinymce").addClass("full");
                $(tinymce.editors[0].iframeElement).css("min-height", $("#GlobalInfoDocValue").closest(".reveal-modal").height() * 0.9);
            }, 300);
        } else {
            //setTimeout(function () {
            //    $("#GlobalInfoDocValue").closest(".reveal-modal").css("top", "100px");
            //}, 400);

            setTimeout(function () {
                $("#GlobalInfoDocModal .tox.tox-tinymce").removeClass("full");
            }, 300);
        }
    }

    function addTemplatesButton(editor) {
        var templateMenuItems = [];

        //Generating menu items for Templates... button
        $.each(templates, function (index, value) {
            if (value.Key != value.Value) {
                var formattedValue = decodeHtml(value.Value);
                templateMenuItems.push({
                    text: value.Key + " - " + formattedValue,
                    onclick: function () { editor.insertContent(value.Key) }
                });
            }
        });

        //Adding Templates... button on TinyMCE editor
        editor.ui.registry.addButton("templateButton", {
            type: "menubutton",
            text: "Templates...",
            icon: false,
            menu: templateMenuItems
        });
    }

    function decodeHtml(html) {
        return $("<div>").html(html).text();
    }

    //end decode html

    //related to full objective path
    $(document).on("click", ".wrt_link", function () {
        if ($(".wrt_path").hasClass("hide")) {
            $(".wrt_path").removeClass("hide");
        }
        else {
            $(".wrt_path").addClass("hide");
        }
    });
    // full path


    $(document).on("opened.fndtn.reveal", "#SignUpModal[data-reveal]", function () {
        $("#SignUpModal").data("revealInit").close_on_background_click = false;
    });


    $(document).on("opened.fndtn.reveal", "#InviteParticipant[data-reveal]", function () {
        $("html, body").css({
            "overflow": "hidden",
            "height": "100%"
        });
        //boomike
    });


    $(document).on("closed.fndtn.reveal", "#InviteParticipant[data-reveal]", function () {
        $("html, body").css({
            "overflow": "auto",
            "height": "auto"
        });
    });

    $(document).on("click", ".save-qh-btn", function () {
        $("#tt-qh-modal").foundation("reveal", "close");
    });

    $(document).on("click", ".tg-aa-wrap > label", function () {
        $("#aa").click();
    });


    //initialize rich text editor when modal is opened
    $(document).on("closed.fndtn.reveal", "#tt-qh-modal[data-reveal]", function () {
        $(".qh-text-area").addClass("hide");
        tinymce.remove("#qh_info_value");
        $(".fullwidth-loading-wrap").addClass("hide");
        $(".fullwidth-loading-wrap").removeAttr("style");
    });

    
    //initialize rich text editor when modal is opened
    $(document).on("opened.fndtn.reveal", "#tt-view-qh-modal[data-reveal]", function () {
        //var at_scope = angular.element($("#anytime-page")).scope();
        var at_scope;
        if (is_anytime) {
            at_scope = angular.element($("#anytime-page")).scope();
        } else {
            at_scope = angular.element($("#TeamTimeDiv")).scope();
        }

        $("#quick-help-content").html(at_scope.output.qh_yt_info);
        var screen_width = document.documentElement.clientWidth;

        //if (screen_width > 1300) {
        //    $("#tt-view-qh-modal").removeClass("medium");
        //    $("#tt-view-qh-modal").addClass("small");
        //}
        //else {
        //    $("#tt-view-qh-modal").removeClass("small");
        //    $("#tt-view-qh-modal").addClass("medium");
        //}

        $("iframe").each(function () {
            $(this)[0].width = temp_iframe_width;
        });
    });

    $(document).on("closed.fndtn.reveal", "#tt-view-qh-modal[data-reveal]", function () {
        tinymce.remove("#qh_info_value");
        var temp_src = "";
        $("iframe").each(function () {
            //if ($(this)[0].src.indexOf("enablejsapi") === -1) {
            //    // ...check whether there is already a query string or not:
            //    // (ie. whether to prefix "enablejsapi" with a "?" or an "&")
            //    var prefix = ($(this)[0].src.indexOf("?") === -1) ? "?" : "&amp;";
            //    $(this)[0].src += prefix + "enablejsapi=true";
            //    temp_src = $(this)[0].src;
            //}
            $(this)[0].src = "";
        });
    });

    $("#tt-view-qh-modal").on("closed.fndtn.reveal", function () {
        tinymce.remove("#qh_info_value");
        var temp_src = "";
        $("iframe").each(function () {
            //if ($(this)[0].src.indexOf("enablejsapi") === -1) {
            //    // ...check whether there is already a query string or not:
            //    // (ie. whether to prefix "enablejsapi" with a "?" or an "&")
            //    var prefix = ($(this)[0].src.indexOf("?") === -1) ? "?" : "&amp;";
            //    $(this)[0].src += prefix + "enablejsapi=true";
            //    temp_src = $(this)[0].src;
            //}
            $(this)[0].src = "";
        });
    });

    //initialize rich text editor when modal is opened
    $(document).on("opened.fndtn.reveal", "#tt-qh-modal[data-reveal]", function () {
        $(".qh-text-area").addClass("hide");
        node_type = "quick-help";
        //show_loading_modal();
       
        var fonts = "Andale Mono=andale mono,times;" + "Arial=arial,helvetica,sans-serif;" + "Arial Black=arial black,avant garde;" + "Book Antiqua=book antiqua,palatino;" + "Comic Sans MS=comic sans ms,sans-serif;" + "Courier New=courier new,courier;" + "Georgia=georgia,palatino;" + "Helvetica=helvetica;" + "Impact=impact,chicago;" + "Symbol=symbol;" + "Tahoma=tahoma,arial,helvetica,sans-serif;" + "Terminal=terminal,monaco;" + "Times New Roman=times new roman,times;" + "Trebuchet MS=trebuchet ms,geneva;" + "Verdana=verdana,geneva;" + "Webdings=webdings;" + "Wingdings=wingdings,zapf dingbats";
        var font_sizes = "8pt 9pt 10pt 11pt 12pt 14pt 16pt 18pt 24pt 36pt";
        var colors = [
        "000000", "Black",
        "993300", "Burnt orange",
        "333300", "Dark olive",
        "003300", "Dark green",
        "003366", "Dark azure",
        "000080", "Navy Blue",
        "333399", "Indigo",
        "333333", "Very dark gray",
        "800000", "Maroon",
        "FF6600", "Orange",
        "808000", "Olive",
        "008000", "Green",
        "008080", "Teal",
        "0000FF", "Blue",
        "666699", "Grayish blue",
        "808080", "Gray",
        "FF0000", "Red",
        "FF9900", "Amber",
        "99CC00", "Yellow green",
        "339966", "Sea green",
        "33CCCC", "Turquoise",
        "3366FF", "Royal blue",
        "800080", "Purple",
        "999999", "Medium gray",
        "FF00FF", "Magenta",
        "FFCC00", "Gold",
        "FFFF00", "Yellow",
        "00FF00", "Lime",
        "00FFFF", "Aqua",
        "00CCFF", "Sky blue",
        "993366", "Red violet",
        "FFFFFF", "White",
        "FF99CC", "Pink",
        "FFCC99", "Peach",
        "FFFF99", "Light yellow",
        "CCFFCC", "Pale green",
        "CCFFFF", "Pale cyan",
        "99CCFF", "Light sky blue",
        "CC99FF", "Plum"
        ];

        tinymce.init({
            textcolor_map: colors,
            selector: "#qh_info_value",
            content_style: custom_css,
            theme: "silver",
            font_formats: fonts,
            fontsize_formats: font_sizes,
            plugins: "print preview fullpage searchreplace autolink directionality visualblocks visualchars fullscreen image link media table charmap hr pagebreak nonbreaking anchor insertdatetime advlist lists imagetools textpattern help",
            //plugins: [
            //    "advlist autolink autosave link image lists charmap print preview hr anchor pagebreak spellchecker", "searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking imagetools", "table contextmenu directionality emoticons textcolor paste fullpage textcolor colorpicker textpattern imagetools"
            //],
            toolbar: [
                "undo redo | copy cut paste | formatselect fontselect fontsizeselect forecolor backcolor | removeformat | bold italic underline strikethrough subscript superscript | alignleft aligncenter alignright alignjustify | outdent indent | bullist numlist table link image media |custombutton1 custombutton2"
            ],
            //toolbar1: "fontselect fontsizeselect | bold italic underline strikethrough | forecolor backcolor| alignleft aligncenter alignright alignjustify | bullist numlist | outdent indent | media  ",
            //toolbar2: "table | hr removeformat | subscript superscript charmap | pagebreak | uploadimg",
            //menubar: false,
            image_advtab: true,
            setup: function (editor) {

                try {
                    var $textarea = $("#" + editor.id);
                    draft = $textarea.val();

                }
                catch (e) {

                }

                editor.on("change", function (e) {
                    draft = editor.getContent();
                });

                editor.on("init", function (e) {
                    this.getDoc().body.style.fontSize = "15px";
                    this.getDoc().body.style.fontFamily = "Arial";
                    $(".qh-text-area").removeClass("hide");
                });


                var at_scope;
                if (is_anytime) {
                    at_scope = angular.element($("#anytime-page")).scope();
                } else {
                    at_scope = angular.element($("#TeamTimeDiv")).scope();
                }


                //if (!at_scope.isMobile()) {
                    editor.ui.registry.addButton("uploadimg", {
                        text: "Upload Image",
                        icon: "image",
                        onAction: function () {
                            $("#GlobalImage").click();
                        }
                    });
                //}
            },
            toolbar_items_size: "small",

            style_formats: [{
                title: "Bold text",
                inline: "b"
            }, {
                title: "Red text",
                inline: "span",
                styles: {
                    color: "#ff0000"
                }
            }, {
                title: "Red header",
                block: "h1",
                styles: {
                    color: "#ff0000"
                }
            }, {
                title: "Example 1",
                inline: "span",
                classes: "example1"
            }, {
                title: "Example 2",
                inline: "span",
                classes: "example2"
            }, {
                title: "Table styles"
            }, {
                title: "Table row 1",
                selector: "tr",
                classes: "tablerow1"
            }],
            imagetools_cors_hosts: ["www.tinymce.com", "codepen.io"],
            force_br_newlines: true,
            force_p_newlines: false,
            forced_root_block: ""
        });
        //$(".qh-text-area").removeClass("hide");
      
        hide_loading_modal();

    });//end of initialize rich text editor when modal is opened

    //initialize rich text editor when modal is opened
    $(document).on("closed.fndtn.reveal", "#GlobalNodesModal[data-reveal]", function () {
        apply_to = false;
       // $("#GlobalInfoDocModal").foundation("reveal", "open");

    });//end of initialize rich text editor when modal is opened

    //initialize rich text editor when modal is opened
    $(document).on("opened.fndtn.reveal", "#GlobalNodesModal[data-reveal]", function () {
        init_tinymce();
    });//end of initialize rich text editor when modal is opened

    function open_apply_to() {
        $("#GlobalInfoDocModal").foundation("reveal", "close");
        $("#GlobalNodesModal").foundation("reveal", "open");
    }

    function open_edit_caption() {
        $("#GlobalInfoDocModal").foundation("reveal", "close");
        $("#GlobalInfoDocCaptionModal").foundation("reveal", "open");
    }

    //initialize tinyMCE editor when modal is open
    $(document).on("open.fndtn.reveal", "#GlobalInfoDocModal[data-reveal]", function () {
        apply_to = false;
        init_tinymce();
    });//end of initialize tinyMCE editor when modal is open

    //removing tinyMCE editor when modal is closed
    $(document).on("closed.fndtn.reveal", "#GlobalInfoDocModal[data-reveal]", function () {
        try {
            draft = tinymce.activeEditor.getContent();
            tinymce.remove("#GlobalInfoDocValue");
            //$(".fullwidth-loading-wrap").removeAttr("style");
        }
        catch (e) {
        }
        templates = [];
    });//end of removing tinyMCE editor when modal is closed


    function is_html_empty_js(html) {
        if (/<[a-z\][\s\S]*>/i.test(html)) {
            if (html.indexOf("img") >= 0) {
                return false;
            }
            if ($(html).text().trim() == "") {
                return true;
            }
            else {
                return false;
            }
        }
        else {
            if (html != "") {
                return false;
            }
            else {
                return true;
            }
        }
    }

    $(document).on("click", ".save-info-doc-btn", Foundation.utils.debounce(function (e) {
        try {
            var temp_div = $("<div>");
            temp_div.html(tinymce.activeEditor.getContent()).find(".tt-modal-header").removeClass("hide");
            var editors_content = temp_div.html();
        }
        catch (e) {
            var editors_content = tinymce.activeEditor.getContent();
        }

        //if (editors_content.indexOf("<p>") == -1) {
        //    editors_content = editors_content;
        //}
        node_guid = node_guid ? node_guid : 0;
        $.ajax({
            type: "POST",
            url: baseUrl + "Default.aspx/SaveInfoDocs",
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
                $(".tt-loading-icon-wrap").show();
                $(".save-info-doc-btn").html("Saving...");
            },
            success: function (data) {
                if (node_type == -1) {
                    var node_name = "parent-node";
                } else if (node_type == 2) {
                    if (node_location == 1) {
                        var node_name = "left-node";
                    } else {
                        var node_name = "right-node";
                    }
                } else if (node_type == 3) {
                    if (node_location == 1) {
                        var node_name = "wrt-left-node";
                    } else {
                        var node_name = "wrt-right-node";
                    }
                } else if (node_type == 4) {
                    var node_name = "scale-node";
                } else {
                    node_name = "question-node";
                }

                var main_scope = angular.element($("#main-body")).scope();
                //var at_scope = angular.element($("#anytime-page")).scope();
                var at_scope;

                if (is_anytime) {
                    at_scope = angular.element($("#anytime-page")).scope();
                } else {
                    at_scope = angular.element($("#TeamTimeDiv")).scope();
                }

                if (node_id != 0 || is_multi) {
                    try {
                        var multi_node_name = "";
                        multi_node_name = node_name.replace("right-", "");
                        multi_node_name = multi_node_name.replace("left-", "");
                    } catch (e) {

                    }

                    if (is_info_tooltip) {
                        if (is_multi && node_name!="parent-node") {
                            var element_name = "." + node_name + "-" + node_id + "-" + multi_index + "-tooltip span";
                            $("." + node_name + "-" + multi_index + "-info-text").html(editors_content);
                            $( "." + node_name + "-" + node_id + "-" + multi_index + "-tooltip").click();
                        } else {
                            var element_name = "." + node_name + "-tooltip span";
                            $("." + node_name + "-info-text").html(editors_content);
                            $( "." + node_name + "-tooltip").click();
                        }

                        if (!is_html_empty_js(editors_content)) {
                            $(element_name).removeClass("disabled");
                            $(element_name).addClass("not-disabled");
                        } else {
                            $(element_name).removeClass("not-disabled");
                            $(element_name).addClass("disabled");
                        }
                    } else {
                        //if framed doc mode

                        $("." + node_name + "-info-text").html(editors_content);
                    }
                    
                } else {
                    if (node_name == "question-node") {
                        //do nothing
                    }
                    else {
                        if (is_info_tooltip) {
                            if (!is_html_empty_js(editors_content)) {
                                $("." + node_name + "-tooltip span").removeClass("disabled");
                                $("." + node_name + "-tooltip span").addClass("not-disabled");
                            } else {
                                $("." + node_name + "-tooltip span").removeClass("not-disabled");
                                $("." + node_name + "-tooltip span").addClass("disabled");
                            }

                            $("." + node_name + "-tooltip").click();
                        }

                        $("." + node_name + "-info-text").html(editors_content);
                    }
                }
               
                try{
                    if (is_anytime) {
                        if (node_name == "question-node") {
                            if (node_type == 0 && !apply_to) {
                                //$(".step-" + current_step).click();
                                main_scope.MoveStepat(e, at_scope.current_step, false, at_scope.is_AT_owner, false, "question");
                            }
                        } else {
                            if (is_multi) {
                                info_doc_index = at_scope.active_multi_index;
                            }

                            //main_scope.MoveStepat(e, at_scope.current_step, false, at_scope.is_AT_owner, false, "infodoc");
                            if (node_type == -1) {
                                atScope.output.parent_node_info = editorContent;
                            } else if (node_type == 2) {
                                changeLeftOrRightInfoDocs(at_scope, node_name, editors_content);
                            } else if (node_type == 3) {
                                changeWrtLeftOrRightInfoDocs(at_scope, node_name, editors_content);
                            } else if (node_type == 4) {
                                if (main_scope.output.non_pw_type == "mtRatings") {
                                    at_scope.output.ScaleDescriptions[info_doc_index == -1 ? 0 : info_doc_index].Description = editors_content;
                                }
                            }

                            at_scope.reflow_equalizer();
                        }
                    } else {
                        at_scope.update_users_list();
                    }
                }
                catch (e) {

                }
            },
            complete: function(){
                
                $(".tt-loading-icon-wrap").hide();
                $(".save-info-doc-btn").html('<span class="icon-tt-save icon"></span><span class="text">Save</span>');
                $("#GlobalInfoDocModal .success").show();
                //setTimeout(function(){
                $("#GlobalInfoDocModal").foundation("reveal", "close");
                //}, 2000);
            },
            error: function (response) {
                console.log(response);
                $(".success").hide();
                $("#GlobalInfoDocModal .alert-box .alert").show();
            }
        });


    }, 1000, false));

    function changeLeftOrRightInfoDocs(atScope, nodeName, editorContent) {
        if (is_multi) {
            if (atScope.output.page_type == "atAllPairwise") {
                var nodeId = -1;

                if (nodeName == "left-node") {
                    nodeId = atScope.multi_data[atScope.active_multi_index].NodeID_Left;
                    atScope.multi_data[atScope.active_multi_index].InfodocLeft = editorContent;
                } else if (nodeName == "right-node") {
                    nodeId = atScope.multi_data[atScope.active_multi_index].NodeID_Right;
                    atScope.multi_data[atScope.active_multi_index].InfodocRight = editorContent;
                }

                //Updating edited node info docs for all pairs where it exists
                for (var i = 0; i < atScope.multi_data.length; i++) {
                    if (i != atScope.active_multi_index) {
                        if (nodeId == atScope.multi_data[i].NodeID_Left) {
                            atScope.multi_data[i].InfodocLeft = editorContent;
                        } else if (nodeId == atScope.multi_data[i].NodeID_Right) {
                            atScope.multi_data[i].InfodocRight = editorContent;
                        }
                    }
                }
            } else {
                atScope.output.multi_non_pw_data[atScope.active_multi_index].Infodoc = editorContent;
            }
        } else {
            if (nodeName == "left-node") {
                atScope.output.first_node_info = editorContent;
            } else if (nodeName == "right-node") {
                atScope.output.second_node_info = editorContent;
            }
        }
    }

    function changeWrtLeftOrRightInfoDocs(atScope, nodeName, editorContent) {
        if (is_multi) {
            if (atScope.output.page_type == "atAllPairwise") {
                var nodeId = -1;

                if (nodeName == "wrt-left-node") {
                    nodeId = atScope.multi_data[atScope.active_multi_index].NodeID_Left;
                    atScope.multi_data[atScope.active_multi_index].InfodocLeftWRT = editorContent;
                } else if (nodeName == "wrt-right-node") {
                    nodeId = atScope.multi_data[atScope.active_multi_index].NodeID_Right;
                    atScope.multi_data[atScope.active_multi_index].InfodocRightWRT = editorContent;
                }

                //Updating edited node info docs for all pairs where it exists
                for (var i = 0; i < atScope.multi_data.length; i++) {
                    if (i != atScope.active_multi_index) {
                        if (nodeId == atScope.multi_data[i].NodeID_Left) {
                            atScope.multi_data[i].InfodocLeftWRT = editorContent;
                        } else if (nodeId == atScope.multi_data[i].NodeID_Right) {
                            atScope.multi_data[i].InfodocRightWRT = editorContent;
                        }
                    }
                }
            } else {
                atScope.output.multi_non_pw_data[atScope.active_multi_index].InfodocWRT = editorContent;
            }
        } else {
            if (nodeName == "wrt-left-node") {
                atScope.output.wrt_first_node_info = editorContent;
            } else if (nodeName == "wrt-right-node") {
                atScope.output.wrt_second_node_info = editorContent;
            }
        }
    }

    $(document).on("click", ".edit-info-doc-btn, .tooltip-infodoc-btn", function () {
        //setTimeout(function () {
            $(".alert-box").hide();

            node = $(this).attr("data-node");
            node_location = $(this).attr("data-location");
            node_type = $(this).attr("data-node-type");
            node_id = $(this).attr("data-node-id");
            node_guid = $(this).attr("data-node-guid");
            multi_index = $(this).attr("data-index");

            isTinyMceReadonly = $(this).attr("data-readonly");
            if (isTinyMceReadonly) {
                isTinyMceReadonly = isTinyMceReadonly == "1" ? true : false;
            } else {
                isTinyMceReadonly = false;
            }

            var editText = "Edit ";
            if (isTinyMceReadonly) {
                editText = "";

                $("#GlobalInfoDocModal > .tt-modal-content > .button-content").addClass("hide");
                $("#GlobalInfoDocModal > .tt-modal-content > .message-content").addClass("hide");
                $("#GlobalInfoDocModal > .tt-modal-content").css("margin-top", "5px");
                $("#GlobalInfoDocModal > .tt-modal-content").css("margin-bottom", "0");
                $("#GlobalInfoDocModal > .tt-modal-content").css("max-height", "100%");
                $("#GlobalInfoDocModal > .tt-modal-content").css("padding", "0 5px");

                $("#GlobalInfoDocModal").css("height", "auto");
                //$("#GlobalInfoDocModal").css("min-height", "90%");
                $("#GlobalInfoDocModal").css("top", "15px");
                $("#GlobalInfoDocModal").removeClass("medium").addClass("xlarge");
            } else {
                //$("#GlobalInfoDocModal").removeAttr("style");
                $("#GlobalInfoDocModal").css("top", "15px");
                $("#GlobalInfoDocModal").removeClass("xlarge").addClass("medium");
                $("#GlobalInfoDocModal > .tt-modal-content").removeAttr("style");
                $("#GlobalInfoDocModal > .tt-modal-content > .button-content").removeClass("hide");
                $("#GlobalInfoDocModal > .tt-modal-content > .message-content").removeClass("hide");
            }

            if (typeof ($(this).attr("data-node-id")) == "undefined") {
                node_id = 0;
            }


            if (typeof (multi_index) == "undefined") {
                multi_index = 0;
            }

            //alert(parent_node);
            $("#GlobalInfoDocModal .alert-box").hide();

            $(".tt-content-wrap title").remove();

            if (is_info_tooltip) {

                if (typeof ($("." + node + "-" + node_id + "-info-text").html()) == "undefined") {
                    $("#GlobalInfoDocValue").val($("." + node + "-info-text").html());
                }
                else {
                    $("#GlobalInfoDocValue").val($("." + node + "-" + node_id + "-info-text").html());
                }
               
            }
            else {
                $("#GlobalInfoDocValue").val($("." + node + "-info-text").html());
            }

            $("#GlobalInfoDocModal .tt-modal-header ").hide();

            var parent_node_types = ["wrt-right-node", "wrt-left-node", "parent-node"];

            if ($.inArray(node, parent_node_types) >= 0) {
                if (node_type == 3) {
                    if (node == "wrt-left-node") {
                        output_parent_node = $(this).attr("data-node-description");
                        output_left_node = $(this).attr("data-node-title");
                        $("#GlobalInfoDocModal > .parent-node-header").html(editText + output_left_node + ' with respect to "' + output_parent_node + '"');
                    }
                    else {
                        output_parent_node = $(this).attr("data-node-description");
                        output_right_node = $(this).attr("data-node-title");
                        $("#GlobalInfoDocModal > .parent-node-header").html(editText + output_right_node + ' with respect to "' + output_parent_node + '"');
                    }
                }
                else {
                    output_parent_node = $(this).attr("data-node-description");;
                    $("#GlobalInfoDocModal > .parent-node-header").html(editText + "Description/Definition for " + '"' + output_parent_node + '"');
                }
                $("#GlobalInfoDocModal > .parent-node-header").show();
            }
            else {
                if (node == "left-node") {
                    output_left_node = $(this).attr("data-node-description");;
                    $("#GlobalInfoDocModal > .left-node-header").html(editText + "Description/Definition for " + '"' + output_left_node + '"');
                }
                else {
                    output_right_node = $(this).attr("data-node-description");;
                    $("#GlobalInfoDocModal > .right-node-header").html(editText + "Description/Definition for " + '"' + output_right_node + '"');
                }
                $("#GlobalInfoDocModal ." + node + "-header").show();
            }

            if (node == "question-node") {
                $("#GlobalInfoDocModal .question-node-header").show();
                $("#GlobalInfoDocValue").val(cluster_phrase);
            } else if (node == "scale-node") {
                output_scale_node = $(this).attr("data-node-description");
                $("#GlobalInfoDocModal > .scale-node-header").html(editText + "Description for measurement scale " + '"' + output_scale_node + '"');
            }

            $("#GlobalInfoDocModal").foundation("reveal", "open");

            
        //}, 1000);
    });

    $(document).on("click", ".magnify-info-doc-btn", function () {
        //setTimeout(function () {
        $(".alert-box").hide();

        node = $(this).attr("data-node");
        node_location = $(this).attr("data-location");
        node_type = $(this).attr("data-node-type");
        node_id = $(this).attr("data-node-id");
        node_guid = $(this).attr("data-node-guid");
        multi_index = $(this).attr("data-index");
        if (typeof ($(this).attr("data-node-id")) == "undefined") {
            node_id = 0;
        }


        if (typeof (multi_index) == "undefined") {
            multi_index = 0;
        }

        //alert(parent_node);
        $("#MagnifyInfoDocModal .alert-box").hide();

        $(".tt-content-wrap title").remove();

        if (is_info_tooltip) {

            if (typeof ($("." + node + "-" + node_id + "-info-text").html()) == "undefined") {
                $("#MagnifyInfoDocValue").val($("." + node + "-info-text").html());
            }
            else {
                $("#MagnifyInfoDocValue").val($("." + node + "-" + node_id + "-info-text").html());
            }

        }
        else {
            $("#MagnifyInfoDocValue").val($("." + node + "-info-text").html());
        }

        $("#MagnifyInfoDocModal .tt-modal-header ").hide();

        var parent_node_types = ["wrt-right-node", "wrt-left-node", "parent-node"];

        if ($.inArray(node, parent_node_types) >= 0) {
            if (node_type == 3) {
                if (node == "wrt-left-node") {
                    output_parent_node = $(this).attr("data-node-description");
                    output_left_node = $(this).attr("data-node-title");
                    $("#MagnifyInfoDocModal > .parent-node-header").html(output_left_node + ' with respect to "' + output_parent_node + '"');
                }
                else {
                    output_parent_node = $(this).attr("data-node-description");
                    output_right_node = $(this).attr("data-node-title");
                    $("#MagnifyInfoDocModal > .parent-node-header").html(output_right_node + ' with respect to "' + output_parent_node + '"');
                }
            }
            else {
                output_parent_node = $(this).attr("data-node-description");;
                $("#MagnifyInfoDocModal > .parent-node-header").html("Description/Definition for " + '"' + output_parent_node + '"');
            }
            $("#MagnifyInfoDocModal > .parent-node-header").show();
        }
        else {
            if (node == "left-node") {
                output_left_node = $(this).attr("data-node-description");;
                $("#MagnifyInfoDocModal > .left-node-header").html("Description/Definition for " + '"' + output_left_node + '"');
            }
            else {
                output_right_node = $(this).attr("data-node-description");;
                $("#MagnifyInfoDocModal > .right-node-header").html("Description/Definition for " + '"' + output_right_node + '"');
            }
            $("#MagnifyInfoDocModal ." + node + "-header").show();
        }

        if (node == "question-node") {
            $("#MagnifyInfoDocModal .question-node-header").show();
            $("#MagnifyInfoDocValue").val(cluster_phrase);
        } else if (node == "scale-node") {
            output_scale_node = $(this).attr("data-node-description");
            $("#MagnifyInfoDocModal > .scale-node-header").html("Description for measurement scale " + '"' + output_scale_node + '"');
        }

        $("#MagnifyInfoDocModal").foundation("reveal", "open");


        //}, 1000);
    });

    $(document).on("click", ".cancelbtn", function () {
        if ($(this).hasClass("cancel-apply-btn")) {
            $("#GlobalNodesModal").foundation("reveal", "close");
            $("#edit-question-btn").click();
        }
        else {
            $("#GlobalInfoDocModal").foundation("reveal", "close");
        }
    });
   
    $(document).on("click", ".apply_to_checkboxes", function () {
        apply_to = true;
        
        if ($(this).is(":checked")) {
            
            if ($.inArray($(this).attr("data-id"), node_ids) == -1) {
                node_ids.push($(this).attr("data-id"));
            }
        }
        else {
            node_ids.splice($.inArray($(this).attr("data-id"), node_ids), 1);
        }

        //console.log(node_ids);
    });

    $(document).on("click", ".apply-nodes-btn", Foundation.utils.debounce(function (e) {
        var is_multi = $(this).attr("data-is-multi");

        if (typeof is_multi == "undefined") {
            is_multi = false;
        }

        //var scope = angular.element($("#anytime-page")).scope();
        var scope;
        if (is_anytime) {
            scope = angular.element($("#anytime-page")).scope();
        } else {
            scope = angular.element($("#TeamTimeDiv")).scope();
        }

        var is_evaluation = true;
        if (scope.output.page_type == "atShowLocalResults" || scope.output.page_type == "atShowGlobalResults") {
            is_evaluation = false;
        }

        $.ajax({
            type: "POST",
            url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/ApplyCustomWordingToNodes",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                node_ids: node_ids,
                custom_word: draft,
                is_multi: is_multi,
                is_evaluation: is_evaluation
            }),
            beforeSend: function () {
                $(".tt-loading-icon-wrap").show();
                $(".apply-nodes-btn").html("Saving...");
            },
            success: function (data) {
                //console.log(node_ids);
                //if (is_anytime && node_type == 0) {
                //    $(".step-" + current_step).click();
                //}

                var main_scope = angular.element($("#main-body")).scope();
                //var at_scope = angular.element($("#anytime-page")).scope();
                var at_scope;
                if (is_anytime) {
                    at_scope = angular.element($("#anytime-page")).scope();
                } else {
                    at_scope = angular.element($("#TeamTimeDiv")).scope();
                }

                try {
                    if (is_anytime) {
                        main_scope.MoveStepat(e, at_scope.current_step, false, at_scope.is_AT_owner, false, "question");
                    }
                }
                catch (e) {

                }

                $(".tt-loading-icon-wrap").hide();
                //$('.apply-nodes-btn').html('<a href="#" class="button tiny tt-button-icond-left apply-nodes-btn button tiny tt-button-primary success"><span class="icon-tt-save icon"></span><span class="text">Save</span></a>');
                //$("#GlobalNodesModal .success").show();

                setTimeout(function () {
                    $("#GlobalNodesModal").foundation("reveal", "close");
                }, 2000);
            },
            error: function (response) {
                $(".success").hide();
                $("#GlobalNodesModal .alert-box .alert").show();
            }
        });

    }, 1000, false));


    //========== Copied from MultiRatings.ascx starts ==========

    $(document).on("click", ".multi-ratings-list .hidden-toggle-btn", Foundation.utils.debounce(function (e) {
        $(".arrow").show();
        $(".htd").addClass("hidden-toggle-btn");
        $(".close-drop-down").hide();
        $(".down-arrow").show();

        var index = $(this).attr("data-index");

        $(".hidden-item-wrap").not($("#ratings-dropdown" + index + " .hidden-item-wrap")[0]).slideUp().removeClass("hidden-toggle-btn-collapsed");

        $("#ratings-dropdown" + index + " .hidden-item-wrap").slideToggle();

        if ($(this).hasClass("hidden-toggle-btn-collapsed")) {
            $(this).removeClass("hidden-toggle-btn-collapsed");
            $(".cdd_" + index).hide();
            $(".arrow_" + index).show();
        } else {
            $(this).addClass("hidden-toggle-btn-collapsed");
            $(".cdd_" + index).show();
            $(".arrow_" + index).hide();
        }

    }, 200, true));

    //========== Copied from MultiRatings.ascx ends ==========
});

function ToggleWRTPath(timeOutSeconds, shouldFlash) {
  if (timeOutSeconds) {
    timeOutSeconds = timeOutSeconds * 1000;
  } else {
    timeOutSeconds = 0;
  }

  if (shouldFlash) {
    $(".wrt_path").addClass("flash");
  } else {
    $(".wrt_path").removeClass("flash");
  }

  setTimeout(function () {
    if (timeOutSeconds > 0) {
      $(".wrt_path").fadeOut(2000);
    } else {
      $(".wrt_path").toggle();
    }
    
  }, timeOutSeconds);
}

var lowests_value = -999999999999;
function getXandYofPriority(val, dataset) {
    if (val < lowests_value || val === "") {

        return ["", false, false];
    }

    var firstData;
    var secondData;
    //console.log(dataset);
    var lowestY = 100;
    var highestY = 0;
    var lowestX = Number.MAX_VALUE;
    var highestX = Number.MIN_VALUE;
    var reverse = false;

    for (i = 0; i < dataset.length; i++) {

        if (dataset[i].y < lowestY)
            lowestY = dataset[i].y;
        if (dataset[i].y > highestY)
            highestY = dataset[i].y;
        if (dataset[i].x < lowestX)
            lowestX = parseFloat(dataset[i].x);
        if (dataset[i].x > lowestX)
            highestX = parseFloat(dataset[i].x);

        if (secondData == null) {
            if (firstData != undefined) {
                if ((parseFloat(val)).toFixed(2) <= parseFloat(dataset[i].x)) {
                    secondData = dataset[i];
                }
                else {
                    firstData = dataset[i];
                }
            } else {
                if (parseFloat(dataset[i].x) <= parseFloat(val)) {
                    firstData = dataset[i];
                }
                else {
                    firstData = dataset[0];
                }
            }
        }
    }

    if (firstData.x > secondData.x) {
        var tempFirstData = firstData;
        firstData = secondData;
        secondData = tempFirstData;
    }

    var temp = secondData.x - firstData.x;
    var tVal = val - firstData.x;
    var percentage = tVal / temp;
    var xAndy = firstData.y + ((secondData.y - firstData.y) * percentage);

    if (parseFloat(secondData.y) < parseFloat(firstData.y)) {
        xAndy = secondData.y + ((firstData.y - secondData.y) * (1 - percentage));
        reverse = false;
    }

    var bottom = false;
   // console.log(lowestX);
    if (highestY <= xAndy + (highestY * 0.4)) {
        bottom = true;
    }

    if (highestY <= xAndy + (highestY * 0.4))
        reverse = false;

    if (((highestX * 0.2) + lowestX) > val) {
        reverse = true;
        //console.log([val, xAndy, reverse, bottom]);
    }

    return [xAndy, reverse, bottom];
}

var rProgressTimer = null;
function runLoadingScreen(start, end, fFinish) {
    
    clearInterval(rProgressTimer);
    var loadingElement = $(".loading-percentage .percentage-value");
    var intervalStep = 100;
    var percentageValue;

    if (fFinish) {
        percentageValue = parseInt(loadingElement.html());

        if (percentageValue >= 100)
            percentageValue /= 10;

        intervalStep = 5;
    } 

    if (!fFinish) {
        loadingElement.html(start);
        percentageValue = start;
    }

    rProgressTimer = setInterval(function () {
        var step = (end - percentageValue) / end;

        if (fFinish)
            step = 1;

        percentageValue += step;
        loadingElement.html(percentageValue.toFixed(0));

        if (percentageValue >= 100)
            loadingElement.html(100);

        if (fFinish && percentageValue >= 100) {
            clearInterval(rProgressTimer);

            setTimeout(function () {
                $(".nothing").addClass("hide");
                $(".steps_list_content ul").removeClass("hide");
                $("#tt-h-modal .hierarchies").removeClass("hide");
            }, 500);
        }
    }, intervalStep);
}