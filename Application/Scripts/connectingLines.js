(function($){
	$.fn.extend({ 
		connectingLines: function(options) {
			var defaults = 	{
			    container: "<body>",
			    strokeColor: '#707070',
				strokeWidth: 1,
				opacity: 1,
				fill: "none"
			};			
			
			//override default options with user set options
			var settings = $.extend(defaults, options);
			
			var me = {};
			
			me.init = function(initObj){
				if (initObj) {
					$.each(initObj, function(index, value){
						settings[index] = value;
					});
				}
			};

			me.set = function(prop, val) {
				settings[prop] = val;
			};

			me.on = function(el1, el2) {
				var $el1 = $(el1);
				var $el2 = $(el2);
				if ($el1.length && $el2.length && $el1.is(":visible") && $el2.is(":visible")) {
					var svgheight, p, svgleft, svgtop, svgwidth;
	
					var el1pos = $(el1).position();
					var el2pos = $(el2).position();
	
					var el1H = $(el1).outerHeight();
					var el1W = $(el1).outerWidth();
	
					var el2H = $(el2).outerHeight();
					var el2W = $(el2).outerWidth();
	
					svgleft = Math.round(el1pos.left + el1W);
					svgwidth = Math.round(el2pos.left - svgleft);
	
					var cpt;
					var factor = 500;
	
					////Determine which is higher/lower
					if ((el2pos.top + (el2H / 2)) <= (el1pos.top + (el1H / 2))) {
						// console.log("low to high");
						svgheight = Math.round((el1pos.top + el1H / 2) - (el2pos.top + el2H / 2));
						svgtop = Math.round(el2pos.top + el2H / 2) - settings.strokeWidth;
						cpt = Math.round(svgwidth * Math.min(svgheight / factor, 1));
						p = "M0," + (svgheight + settings.strokeWidth) + " C" + cpt + "," + (svgheight + settings.strokeWidth) + " " + (svgwidth - cpt) + "," + settings.strokeWidth + " " + svgwidth + "," + settings.strokeWidth;
					} else {
						// console.log("high to low");
						svgheight = Math.round((el2pos.top + el2H / 2) - (el1pos.top + el1H / 2));
						svgtop = Math.round(el1pos.top + el1H / 2) - settings.strokeWidth;
						cpt = Math.round(svgwidth * Math.min(svgheight / factor, 1));
						p = "M0," + settings.strokeWidth + " C" + cpt + ",0 " + (svgwidth - cpt) + "," + (svgheight + settings.strokeWidth) + " " + svgwidth + "," + (svgheight + settings.strokeWidth);
					}
					$divConnectingLines = $('#divConnectingLines').length ? $('#divConnectingLines') : $(settings.container).append($("<div id='divConnectingLines'></div>")).find('#divConnectingLines');
	
					var svgnode = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
					var newpath = document.createElementNS('http://www.w3.org/2000/svg', "path");
					
					newpath.setAttributeNS(null, "d", p);
					newpath.setAttributeNS(null, "stroke", settings.strokeColor);
					newpath.setAttributeNS(null, "stroke-width", settings.strokeWidth);
					newpath.setAttributeNS(null, "opacity", settings.opacity);
					newpath.setAttributeNS(null, "fill", settings.fill);
					svgnode.appendChild(newpath);
					$(svgnode).css({
						left : svgleft,
						top : svgtop,
						position : 'absolute',
						width : svgwidth,
						height : svgheight + settings.strokeWidth * 2,
						minHeight : '20px'
					});
					$divConnectingLines.append(svgnode);
				}
			};

			me.off = function() {
				$("#divConnectingLines").empty();
			};
			
			//// Redraw upon resizing the window
			//var selector = this.selector;
			//$(window).resize(function() {
			//     me.off();
			//     $(selector).each(function() {
			//		var theID = this.id;
			//	    $("." + theID).each(function(i,e) {
			//	      me.on($("#" + theID), e);
			//	    });
			//	});
			//});
			
			// Loop through each parent and draw the lines connecting children
			return this.each(function() {
				var theID = this.id;
			    $("."+theID).each(function(i,e) {
			      me.on($("#" + theID), e);
			    });
			});
		}
	});
})(jQuery);