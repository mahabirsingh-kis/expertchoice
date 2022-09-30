/* javascript plug-ins for SVG by SL */
const ecSVGDefOptions = {
    backgroundColor: '#ffffaa',
    dataSource: null,
    debugMode: false,
    height:200,
    width:300,
};

var testData = {'points':[]};

var ecSVGDefOptionsList = ['backgroundColor'];

(function ($) {
    $.widget('expertchoice.ecSVG', {
        options: ecSVGDefOptions,
        _create: function () {
            var opt = this.options;
            opt.divCanvas = this.element[0];
            opt.divCanvas.innerHTML = '';
            $(opt.divCanvas).addClass('widget-created');
            this.redraw();
        },
        _destroy: function () {
            if ((this.options) && (this.options.divCanvas)) $(this.options.divCanvas).removeClass('widget-created').html('');
        },
        getChangedOptions: function () {
            var opt = this.options;
            var opts = ecSVGDefOptions;
            var curOptions = {};
            for (var i = 0; i < ecSVGDefOptionsList.length; i++) {
                var prop = ecSVGDefOptionsList[i];
                if (Object.prototype.hasOwnProperty.call(opts, prop)) {
                    if (opts[prop] !== opt[prop]) {
                        curOptions[prop] = opt[prop];
                    }
                }
            }
            return curOptions;
        },
        resize: function (width, height) {
            var opt = this.options;
            var w = Math.round(width);
            var h = Math.round(height);
            var w2 = w/2;
            var h2 = h/2;
            opt.width = w;
            opt.height = h;
            opt.context.addClass("scroll").css('height', h + 'px').css('width', w + 'px');
            opt.svg.background.size(w, h).center(w2, h2);
            if (opt.debugMode) {
                opt.svg.l1.plot(w2, 0, w2, h);
                opt.svg.l2.plot(0, h2, w, h2);
                opt.svg.txt.text(w + 'x' + h).center(w2, h2);
                var OKGOFS = Math.min(w2, h2);
                opt.svg.txtO.font({size: OKGOFS}).center(w2 / 2, h2 / 2);
                opt.svg.txtK.font({size: OKGOFS}).center(w2 + w2 / 2, h2 / 2);
                opt.svg.txtG.font({size: OKGOFS}).center(w2 / 2, h2 + h2 / 2);
                opt.svg.txtO1.font({size: OKGOFS}).center(w2 + w2 / 2, h2 + h2 / 2);
            };
            //opt.svg.circle.radius(Math.min(h2,w2)/2).center(w2, h2);
        },
        redraw: function () {
            var opt = this.options;
            $('#' + opt.divCanvas.id).empty();
            opt.width = opt.divCanvas.parentNode.offsetWidth;
            opt.height = opt.divCanvas.parentNode.clientHeight;
            $("<div id='" + opt.divCanvas.id + "_main'></div>").appendTo( '#' + opt.divCanvas.id );
            opt.context = SVG().addTo('#' + opt.divCanvas.id +'_main').size(opt.width, 1000);
            opt.svg = {};
            this._draw();
        },
        _draw: function () {
            var opt = this.options;
            var ctx = opt.context;
            var w = opt.width;
            var h = opt.height;
            opt.svg.background = ctx.rect().fill(opt.backgroundColor);
            var strokeStyle = { width: 0.5, color: 'red' };
            //opt.svg.circle = ctx.circle().stroke(strokeStyle).fill('#ff0000');
            //opt.svg.circle.mouseenter(function () {
            //    this.animate(500, 100, 'now').attr({ opacity: 1, fill: '#00ff00' });
            //});
            if (opt.debugMode) {
                opt.svg.background.stroke(strokeStyle);
                opt.svg.txt = ctx.text('');
                opt.svg.l1 = ctx.line().stroke(strokeStyle);
                opt.svg.l2 = ctx.line().stroke(strokeStyle);
                opt.svg.txtO = ctx.text('O');
                opt.svg.txtK = ctx.text('K');
                opt.svg.txtG = ctx.text('G');
                opt.svg.txtO1 = ctx.text('O');
            };
            this.resize(w,h);
        },

        _setOption: function (key, value) {
            var opt = this.options;
            this._super(key, value);
            if (key == 'dataSource') {

            }
        },
        _setOptions: function (options) {
            this._super(options);
        },
    });
})(jQuery);