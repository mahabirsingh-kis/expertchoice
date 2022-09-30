// Chat widget for Comparion/Gecko project. rev 170112
// (C) SL, AD // ExpertChoice Inc. 2017

(function ($) {
    $.widget("expetrchoice.chat", {

        options: {
            messages: [],
            uid: -1
        },

        _create: function () {
            var self = this;
            $(window).resize(function () { self.resize(); });
            this.refresh();
            if ($(this.element).is(":visible")) this.resize();
        },

        refresh: function () {
            var el = this.element;
            var id = el.id;
            var e = $(el);

            var msg_list = "";
            var m = this.options.messages;
            if ((m) && m.length>0) {
                var msgDiv = $('<div style="width:100%; height:100%"></div>', {});
                $(msgDiv).append($('#chatMsgTemp').tmpl(m));
                msg_list = msgDiv.html();
            };

            var just_refresh = (this.list().length);
            if (just_refresh) {
                // just refresh the messages list
                this.list().html(msg_list);
                this.scroll();

            } else {
                // draw first time

                var inputfield = '<textarea id="' + id + '_input" cols="40" rows="3" class="textarea" style="width:100%"></textarea><div style="text-align:right"><input id="' + id + '_send" type="button" class="button" value="Send" title="Send Comment"/></div>';
                var data = [{ messages: msg_list, input: inputfield }];
                var pane = $('#chatPaneTemp').tmpl(data);

                e.html(pane);

                function checkInput(event) {
                    var el = this.element;
                    $(el).chat("checkInput");
                }

                this._on(this.input(), { "keyup": checkInput });
                this._on(this.input(), { "click": checkInput });
                this._on(this.input(), { "change": checkInput });
                this._on(this.input(), { "onfocus": checkInput });

                this._on(this.btnSend(), {
                    "click": function (event) {
                        var el = this.element;
                        $(el).chat("sendMessage");
                    }
                });

                this._on(this.input(), {
                    "keypress": function (event) {
                        if (event.keyCode == 13) {
                            var el = this.element;
                            $(el).chat("sendMessage");
                            event.preventDefault();
                        }
                    }
                });

                this.checkInput();
            }

            this._trigger("refresh");
        },

        _setOption: function (key, value) {
            this._super(key, value);
            if (key == "messages") { this.options.messages = value; this.refresh(); }
        },

        _setOptions: function (options) {
            this._super(options);
        },

        list: function(){
            return $(this.element).find(".comment_list");
        },

        input: function () {
            return $("#" + this.element.id + "_input");
        },

        value: function () {
            return trim($("#" + this.element.id + "_input").val());
        },

        clear: function () {
            this.input().val("");
            this.checkInput();
        },

        btnSend: function () {
            return $("#" + this.element.id + "_send");
        },

        focus: function () {
            this.input().focus();
        },

        blur: function () {
            this.input().blur();
        },

        resize: function () {
            var lst = this.list();
            lst.height(100).height(lst.parent().height() - 8);
            this._trigger("resize");
            this.scroll();
        },

        scroll: function () {
            var lst = this.list();
            lst.scrollTop(lst.height());
        },

        checkInput: function () {
            this.btnSend().prop("disabled", (this.value() == "" ? "disabled" : ""));
        },

        sendMessage: function (event) {
            var v = this.value;
            if (v != "") {
                this._trigger("send");
                return true;
            }
            return false;
        },

        addMessage: function (msg) {
            this.options.messages.push(msg);
            this.refresh();
        },

        resetMessages: function () {
            this.options.messages= [];
            this.refresh();
        },

        destroy: function () {
            $(this.element).empty();
            $(window).unbind("resize");
        }

    });
})(jQuery);
