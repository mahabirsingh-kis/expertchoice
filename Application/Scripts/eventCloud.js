/* eventCloud widget by DA */

(function ($) {
    $.widget("expertchoice.eventcloud", {
        options: {
            // events list sorted by likelihood
            data: [{ guid: "event GUID", name: "event Name", index: "0", color: "#777", l: 0, i: 0, r: 0, i_s: 0, r_s: 0 }], // l - likelihood, i - impact, r - risk, i_s - $ impact, r_s - $ risk
            showDollarValue: false,
            lblLikelihood: "Likelihood",
            lblImpact: "Impact",
            lblRisk: "Risk",
            lblNoData: "No data to display",
            currencyChar: "$",
            tooltipId: "DivRiskMapTooltip",
            decimals: 2,
            numbers: false
        },
        _create: function () {
            this.options.canvas = this.element[0];
            this.options.id = this.element[0].id;
            var self = this;
            $(window).resize(function () { self.resize(); });

            $(this.options.canvas).show();
            this.init();
            this.refresh();
            this.resize();

            this._on(this.options.canvas, {
                "mousemove": function (event) {
                    var rect = this.options.canvas.getBoundingClientRect();
                    var pos = {
                        x: event.clientX - rect.left,
                        y: event.clientY - rect.top
                    };
                    this.onMouseMove(event, pos);
                    return false;
                },
                "mouseout": function (event) {
                    this.onMouseOut();
                    return false;
                }
            });
        },
        init: function () {
            // init canvas context
            var canvas = this.options.canvas;
            if ((canvas)) {
                if ((typeof G_vmlCanvasManager != 'undefined') && (G_vmlCanvasManager)) G_vmlCanvasManager.initElement(canvas);
                if (!canvas.getContext) return false;
                this.options.ctx = canvas.getContext("2d");
            }
        },
        refresh: function () {            
            var ctx = this.options.ctx;
            var canvas = this.options.canvas;

            // fill the canvas with background color - works best in IE and FF
            ctx.fillStyle = canvas.style.backgroundColor;
            ctx.fillRect(0, 0, canvas.width, canvas.height);

            // default settings
            var DEFAULT_EVENT_CLOUD_FONT_FAMILY = "Arial";
            var EVENT_CLOUD_TEXT_LOWER_CASE = true;
            var EVENT_CLOUD_TEXT_MAX_LENGTH = 25;
            var EVENT_CLOUD_TEXT_MAX_FONT_SIZE = 54;
            var EVENT_CLOUD_TEXT_MIN_FONT_SIZE = 6;
            var DEFAULT_EVENT_CLOUD_FONT_COLOR = "#940000"; // - red color; "#003153" - blue
            var DEFAULT_EVENT_CLOUD_MAXIMUM_COLOR = "#940000";

            // initial params
            var canvas_width = canvas.width;
            var canvas_height = canvas.height;
            ctx.textBaseline = "top";
            var center_x = canvas_width / 2;
            var center_y = canvas_height / 2;
            var y = 0;
            var text_margin = 10;
            var max_count = 1000;

            var cloud_data = this.options.data;

            var intersects = function (cloud_data, val, index) {
                var retVal = false;
                var px0 = val.x - text_margin;
                var px1 = val.x + val.w + text_margin;
                var py0 = val.y - text_margin;
                var py1 = val.y + val.h + text_margin;

                for (var i = 0; i < index; i++) {
                    var c = cloud_data[i];
                    var cx0 = c.x;
                    var cx1 = c.x + c.w;
                    var cy0 = c.y;
                    var cy1 = c.y + c.h;

                    if ((px1 < cx0) || (py1 < cy0) || (px0 > cx1) || (py0 > cy1)) {
                        // 
                    } else {
                        retVal = true;
                        i = index;
                    }
                }
                return retVal;
            }

            // If no data then show "No data to display"
            if (!(cloud_data) || cloud_data.length == 0) {
                // count coords and offsets
                var lineMetrics = ctx.measureText(this.options.lblNoData);
                var val = {};
                val.h = 14; // label height
                val.w = lineMetrics.width; // label width
                val.x = center_x - val.w / 2;
                val.y = center_y - val.h / 2;

                // plot label text
                ctx.font = val.h + "pt " + DEFAULT_EVENT_CLOUD_FONT_FAMILY;
                ctx.fillStyle = "#FFFFFF";
                ctx.fillText(this.options.lblNoData, val.x, val.y);
            }

            var showNumbers = this.options.numbers;

            // plot event names
            $.each(cloud_data, function (index, val) {
                var lbl = val.name;
                if (EVENT_CLOUD_TEXT_LOWER_CASE) lbl = lbl.toUpperCase();
                var lbl_short = ShortString(lbl, EVENT_CLOUD_TEXT_MAX_LENGTH, false);
                var lbl_num = showNumbers ? val.index : ""; // number
                var display_text = lbl_num + (lbl_num == "" ? "" : " ") + lbl_short;

                // detect font size - depends on even's impact
                var fontSize = Math.ceil(val.i * EVENT_CLOUD_TEXT_MAX_FONT_SIZE);
                if (fontSize == 0 || fontSize < EVENT_CLOUD_TEXT_MIN_FONT_SIZE) fontSize = EVENT_CLOUD_TEXT_MIN_FONT_SIZE;
                //if (fontSize > EVENT_CLOUD_TEXT_MAX_FONT_SIZE) fontSize = EVENT_CLOUD_TEXT_MAX_FONT_SIZE;
                ctx.font = fontSize + "pt " + DEFAULT_EVENT_CLOUD_FONT_FAMILY;

                // actual datapoint color
                //val[IDX_LABEL1].color

                // detect font color - based on event's risk            
                var risk_prc = val.r;
                if (risk_prc <= 0) risk_prc = 0.0001;
                ctx.fillStyle = index == 0 ? DEFAULT_EVENT_CLOUD_MAXIMUM_COLOR : colorBrightness(DEFAULT_EVENT_CLOUD_FONT_COLOR, risk_prc <= 1 ? 100 - risk_prc * 100 : 0);
                //var regionIndex = (risk_prc > _Rh ? 0 : (risk_prc > _Rl ? 1 : 2));
                //switch (regionIndex) {
                //    case 0:
                //        ctx.fillStyle = _redBrush;
                //        break;
                //    case 1:
                //        ctx.fillStyle = _whiteBrush;
                //        break;
                //    case 2:
                //        ctx.fillStyle = _greenBrush;
                //        break;
                //}            


                // count coords and offsets
                var lineMetrics = ctx.measureText(display_text); // lineMetrics.width
                val.h = fontSize; // label height
                val.w = lineMetrics.width; // label width

                val.cx = center_x; // central x point of the text
                val.cy = center_y; // central y point of the text

                val.x = val.cx - val.w / 2;
                val.y = val.cy - val.h / 2;

                var count = 0
                var radius = center_x * 2;

                // distance between points to plot
                var chord = 10;

                // value of theta corresponding to end of last coil
                var coils = center_x / chord;
                var thetaMax = coils * 2 * Math.PI;

                // How far to step away from center for each side.
                var awayStep = radius / thetaMax;

                var rotation = 0;

                // For every side, step around and away from center.
                // start at the angle corresponding to a distance of chord
                // away from centre.
                // How far away from center
                var away = awayStep * theta;
                for (var theta = chord / awayStep; theta < thetaMax; theta += chord / away) {
                    away = awayStep * theta;

                    // How far around the center.
                    var around = theta + rotation;
                    
                    // Convert 'around' and 'away' to X and Y.
                    val.cx = center_x + Math.cos(around) * away;
                    val.cy = center_y + Math.sin(around) * away;
                    
                    // draw the datapoint of the spiral
                    //ctx.fillRect(val.cx, val.cy, 1, 1);

                    // Now that you know it, do it.
                    val.x = val.cx - val.w / 2;
                    val.y = val.cy - val.h / 2;

                    // to a first approximation, the points are on a circle
                    // so the angle between them is chord/radius
                    count += 1;

                    if (!intersects(cloud_data, val, index) || count >= max_count) {
                        // exit for
                        theta = thetaMax;
                    }
                }

                //val.x += text_margin * (Math.random() > 0.5 ? -1 : 1);
                //val.y += text_margin * (Math.random() > 0.5 ? -1 : 1);

                if (count == max_count) {
                    val.x = 0;
                    val.y = 0;
                }

                if (val.x < 0) val.x = 0;
                if (val.y < 0) val.y = 0;

                // plot label text
                ctx.fillText(display_text, val.x, val.y);
            });

        },
        _setOption: function (key, value) {
            this._super(key, value);
            if (key == 'value') {
                
            };
        },
        _setOptions: function (options) {
            this._super(options);
        },
        resize: function () {
            var canvas = this.options.canvas;
            var parent = canvas.parentElement;
            if ((parent)) {
                var min_canvas_width = 800;
                var min_canvas_height = 400;
                canvas.width = min_canvas_width;
                canvas.height = min_canvas_height;
                var w = parent.offsetWidth;
                var h = parent.offsetHeight;
                canvas.width = w > min_canvas_width ? w : min_canvas_width;
                canvas.height = h > min_canvas_height ? h : min_canvas_height;
                this.refresh();
            }
        },
        onMouseMove: function (e, pos) {
            var val = null;
            for (var i = 0; i < this.options.data.length; i++) {
                var c = this.options.data[i];
                if (pos.x > c.x && pos.x < c.x + c.w && pos.y > c.y && pos.y < c.y + c.h) {
                    val = c;
                    i = this.options.data.length;
                }
            }
            if ((val)) {
                // create tooltip
                var header = (val.index == "" ? "" : val.index + " ") + val.name;
                var lblL = this.options.lblLikelihood + ': ' + (val.l * 100).toFixed(this.options.decimals) + '%';
                var lblI = this.options.lblImpact + ': ' + (this.options.showDollarValue ? showCost(val.i_s, true) : (val.i * 100).toFixed(this.options.decimals) + '%');
                var lblR = this.options.lblRisk + ': ' + (this.options.showDollarValue ? showCost(val.r_s, true) : (val.r * 100).toFixed(this.options.decimals) + '%');
                $('#' + this.options.tooltipId).html('<br /><h6>' + header + '</h6>' + lblL + '<br />' + lblI + '<br />' + lblR + '<br />');

                // show tooltip
                $('#' + this.options.tooltipId).css({ left: e.clientX + 10, top: e.clientY + 10 }).show();
            } else {
                setTimeout("$('#" + this.options.tooltipId + "').hide();", 200);
                //$('#DivRiskMapTooltip').css({ left: e.clientX, top: e.clientY }).show().html("x = " + pos.x + " | y = " + pos.y);
            }
        },
        onMouseOut: function () {
            $('#' + this.options.tooltipId).hide();
        }
    });
})(jQuery);