// Combobox draft (C) AD //  Expert Choice Inc., 2014

    var _img_scombobox_down  = new Image();
    var _img_scombobox_down_ = new Image();

    var msg_sComobox_WrongValue = "Wrong value!\nPlease select value from list of available or type a number between 0 and 1.";
    var sCombobox_DelayOnType = 50;

    var is_ie = (navigator.appName.toLowerCase().indexOf("internet explorer")>0);

    function sCombobox_Init(id, values_name) {
        var edit = document.getElementById(id);
        if ((edit)) {
        	edit.setAttribute("data", values_name);
        	edit.onkeyup = function () { setTimeout("sCombobox_onKeyPress(\"" + id + "\");", sCombobox_DelayOnType); };
            var lst = document.createElement("div");
            lst.id = id + "_list";
            lst.className = "sCombobox_list";
            lst.style.width = (edit.clientWidth + (is_ie ? +2 : -4)) + "px";
            lst.style.left = edit.offsetLeft*1;
            lst.style.top = edit.offsetTop+edit.clientHeight+2;
            lst.style.display = "none";
    	edit.parentNode.insertBefore(lst, edit);
    	var btn = document.createElement("input");
    	btn.type = "button";
    	btn.className = "sCombobox_btn";
    	btn.id = id + "_btn";
    	btn.onclick = function (event) { sCombobox_ClickAllValues(id); }
    	if (is_ie) {
    	  btn.onmouseover = function (event) { sCombobox_ChangeIcon(this, true); }
       	  btn.onmouseout = function (event) { sCombobox_ChangeIcon(this, false); }
       	}
    	if ((edit.nextSibling)) edit.parentNode.insertBefore(btn, edit.nextSibling); else edit.parentNode.addElement(btn)
        }
    };

    function sCombobox_BarByValue(id, value) {
        var edit = document.getElementById(id);
        if ((edit)) {
        	if (value<0) value=0;
        	if (value>1) value=1;
        	var val = Math.round(value * edit.clientWidth)+"px";
        	if (edit.style.backgroundPosition!=val) edit.style.backgroundPosition = val;
        }
    }

    function sCombobox_ClickAllValues(id) {
        if (sCombobox_isExpanded(id)) sCombobox_Collapse(id); else sCombobox_Expand(id);
    }

    function sCombobox_ChangeIcon(obj, hover) {
        if ((obj)) obj.style.backgroundImage = 'url(' + (hover ? _img_scombobox_down : _img_scombobox_down_).src + ')';
    }

    function sCombobox_isExpanded(id)
    {
        var lst = document.getElementById(id+"_list");
        return ((lst) && (lst.style.display!="none"));
    }

    function sCombobox_Expand(id)
    {
        var lst = document.getElementById(id+"_list");
        if ((lst)) {
        	lst.style.display="block";
        	sCombobox_UpdateItemsList(id, "");
            var fld = eval("theForm." + id);
        	if ((fld)) fld.focus();
        }
    }

    function sCombobox_UpdateItemsList(id, text)
    {
        var lst = document.getElementById(id+"_list");
        if ((lst)) {
        	lst.innerHTML = "";
        	var values = sCombobox_Values(id);
        	for (i=0; i<values.length; i++)
        	  {
                var n = values[i].text;
        	    if (text==null || text=="" || n.toLowerCase().indexOf(text)>=0)
        	    {
        	        var idx = n.toLowerCase().indexOf(text);
        	        if (idx>=0) n = n.substr(0,idx)+"<b>"+n.substr(idx,text.length)+"</b>"+n.substr(idx+text.length);
        	    	lst.innerHTML += "<a href='' onclick='sCombobox_SelectItem(\"" + id + "\", " + i + "); return false;' style='text-decoration:none'><div class='sCombobox_item' id='" + id + "_item" + i +"'>" + n + "</div></a>";
        	    }
        	  }
        }
    }

    function sCombobox_SelectItem(id, idx)
    {
        var fld = eval("theForm." + id);
        if ((fld)) {
            var values = sCombobox_Values(id);
            if ((values)) fld.value = values[idx].text;
            fld.style.color = "";
            sCombobox_BarByValue(id, values[idx].value);
        	sCombobox_Collapse(id);
        }
        return false;
    }

    function sCombobox_Collapse(id)
    {
        var lst = document.getElementById(id+"_list");
        if ((lst)) lst.style.display="none";
        var fld = eval("theForm." + id);
        if ((fld)) fld.focus();
    }

    function sCombobox_Values(id)
    {
        var res = null;
        var fld = eval("theForm." + id);
        if ((fld)) {
        	var name = fld.getAttribute("data");
        	if (name!="") res = eval(name);
        }
        return res;
    }

    function sCombobox_GetValue(id) {
        var fld = eval("theForm." + id);
        var res = "";
        if ((fld)) {
        	var values = sCombobox_Values(id);
        	if ((values)) {
                var val = fld.value.toLowerCase();
                var is_empty = (val == "");
                var is_fixed = false;
                var is_number = false;
                if (!(is_empty)) {
                    for (var i = 0; i < values.length; i++) {
                        if (val == values[i].text.toLowerCase()) {
                            is_fixed = true;
                            res = 1*values[i].value;
                            break; 
                        }
                    }
                    if (!(is_fixed)) {
                        var n = val.replace(",", ".");
                        if (n[0] == ".") n = "0" + n;
                        if (n == "0.") n = "0";
                        if ((n * 1) == n) {
                            n = n * 1;
                            is_number = (n >= 0 && n <= 1);
                            if ((is_number)) res = n;
                        }
                    }
                }
            }
        }
        return res;
    }

    function sCombobox_onKeyPress(id) {
        var fld = eval("theForm." + id);
        if ((fld)) {
        	if (sCombobox_isExpanded(id))
        	{
        	   if (fld.value=="") sCombobox_Collapse(id); else sCombobox_UpdateItemsList(id, fld.value);
        	}
        	else
        	{
        	   var values = sCombobox_Values(id);
        	   if ((values) && fld.value!="") {
                  for (var i = 0; i < values.length; i++) {
                     if (values[i].text.toLowerCase().indexOf(fld.value)>=0) {
                        sCombobox_Expand(id);
                        sCombobox_UpdateItemsList(id, fld.value);
                        break; 
                     }
                  }
        	   }
        	}
        	var val = sCombobox_GetValue(id);
            if (val==="") {
                fld.style.color = "#990000";
                sCombobox_BarByValue(id, 0);
                return false;
            }
            else {
                if (fld.style.color!="") fld.style.color = "";
                sCombobox_BarByValue(id, val);
            }
        }
        return true;
    }

/*
    var ratings_list_def = [{ text: "Not rated", 	value: "-1",   id: "-1" },
                            { text: "Outstanding", 	value: "1.0",  id: "1" },
                            { text: "Excellent", 	value: "0.86", id: "2" },
                            { text: "Very Good", 	value: "0.69", id: "3" },
                            { text: "Good to Very Good",value: "0.56", id: "4" },
                            { text: "Good", 		value: "0.47", id: "5" },
                            { text: "Moderate to Good", value: "0.36", id: "6" },
                            { text: "Moderate", 	value: "0.26", id: "7" },
                            { text: "A Tad", 		value: "0.4",  id: "8" },
                            { text: "None", 		value: "1.0",  id: "9" }];

    _img_scombobox_down.src = "scombobox_down.gif";
    _img_scombobox_down_.src = "scombobox_down_gray.gif";

    function Init() {
    	sCombobox_Init("ratings", "ratings_list");
    }
*/