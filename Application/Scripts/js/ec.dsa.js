/* javascript plug-ins for Sensitivity Analysis by SL */
const testData = {
"objs":[{"id":9,"idx":1,"name":"Leverage Knowledge","value":0.2779891491,"initValue":0.2779891491,
"gradientMaxValues":[{"altID":7,"val":0.2218995243},{"altID":2,"val":0.3930347264},
{"altID":16,"val":0.496779263},{"altID":12,"val":0.2955086231},
{"altID":3,"val":0.2303225845},{"altID":11,"val":0.1651814729},
{"altID":4,"val":0},{"altID":10,"val":0.1785545498},{"altID":14,"val":0.3260715604},
{"altID":8,"val":0.4651694894},{"altID":6,"val":0.2313463688},{"altID":1,"val":1},
{"altID":15,"val":0.4998238087},{"altID":13,"val":0.4589859843},{"altID":5,"val":0.2329474986},
{"altID":9,"val":0.7934586406}],"gradientMinValues":[{"altID":7,"val":0.5950049758},
{"altID":2,"val":0.6868094206},{"altID":16,"val":0.5299584866},{"altID":12,"val":0.4879012704},
{"altID":3,"val":0.6412448883},{"altID":11,"val":0.6407269239},{"altID":4,"val":0.7131854892},
{"altID":10,"val":0.3587312698},{"altID":14,"val":0.28340289},{"altID":8,"val":0.5743762255},
{"altID":6,"val":0.5095055103},{"altID":1,"val":0.6069543958},{"altID":15,"val":0.4948494434},
{"altID":13,"val":0.4927431345},{"altID":5,"val":0.5117707253},{"altID":9,"val":0.6369494796}],
"gradientInitMinValues":[{"altID":7,"val":0.5950049758},{"altID":2,"val":0.6868094206},
{"altID":16,"val":0.5299584866},{"altID":12,"val":0.4879012704},{"altID":3,"val":0.6412448883},
{"altID":11,"val":0.6407269239},{"altID":4,"val":0.7131854892},{"altID":10,"val":0.3587312698},
{"altID":14,"val":0.28340289},{"altID":8,"val":0.5743762255},{"altID":6,"val":0.5095055103},
{"altID":1,"val":0.6069543958},{"altID":15,"val":0.4948494434},{"altID":13,"val":0.4927431345},
{"altID":5,"val":0.5117707253},{"altID":9,"val":0.6369494796}],"values2D":[{}],
"color":"#084e95","visible":1},
{"id":11,"idx":2,"name":"Improve Organizational Efficiency","value":0.2694590092,"initValue":0.2694590092,
"gradientMaxValues":[{"altID":7,"val":0.4812718332},{"altID":2,"val":0.5279834867},
{"altID":16,"val":0.1690651476},{"altID":12,"val":0.2677386701},{"altID":3,"val":0.5728337765},
{"altID":11,"val":0.297334075},{"altID":4,"val":0.3149563968},{"altID":10,"val":0.2033084035},
{"altID":14,"val":0.1104048565},{"altID":8,"val":0.4928722382},{"altID":6,"val":0.4762985408},
{"altID":1,"val":0.4881637096},{"altID":15,"val":0.2801020443},{"altID":13,"val":0.1976124793},
{"altID":5,"val":0.4528645277},{"altID":9,"val":0.5292346478}],
"gradientMinValues":[{"altID":7,"val":0.4949793816},{"altID":2,"val":0.6336035132},
{"altID":16,"val":0.6504480243},{"altID":12,"val":0.4958977103},{"altID":3,"val":0.5101119876},
{"altID":11,"val":0.5864299536},{"altID":4,"val":0.5886868238},{"altID":10,"val":0.3474969864},
{"altID":14,"val":0.3634494543},{"altID":8,"val":0.5628829598},{"altID":6,"val":0.4159073234},
{"altID":1,"val":0.8003339171},{"altID":15,"val":0.5759515762},{"altID":13,"val":0.5887562037},
{"altID":5,"val":0.4273989499},{"altID":9,"val":0.7362356186}],
"gradientInitMinValues":[{"altID":7,"val":0.4949793816},{"altID":2,"val":0.6336035132},
{"altID":16,"val":0.6504480243},{"altID":12,"val":0.4958977103},{"altID":3,"val":0.5101119876},
{"altID":11,"val":0.5864299536},{"altID":4,"val":0.5886868238},{"altID":10,"val":0.3474969864},
{"altID":14,"val":0.3634494543},{"altID":8,"val":0.5628829598},{"altID":6,"val":0.4159073234},
{"altID":1,"val":0.8003339171},{"altID":15,"val":0.5759515762},{"altID":13,"val":0.5887562037},
{"altID":5,"val":0.4273989499},{"altID":9,"val":0.7362356186}],"values2D":[{}],"color":"#287A66","visible":1},
{"id":16,"idx":3,"name":"Maintain Serviceability","value":0.0802852064,"initValue":0.0802852064,
"gradientMaxValues":[{"altID":7,"val":0.8630307317},{"altID":2,"val":0.9315666556},
{"altID":16,"val":0.8469096422},{"altID":12,"val":0.4330189228},{"altID":3,"val":0.9315666556},
{"altID":11,"val":0.3819528222},{"altID":4,"val":0.8895329833},{"altID":10,"val":0.1726085395},
{"altID":14,"val":0.3779293895},{"altID":8,"val":0.6710087061},{"altID":6,"val":0.5923330784},
{"altID":1,"val":0.9699354172},{"altID":15,"val":0.6089448333},{"altID":13,"val":0.2968280315},
{"altID":5,"val":0.9117543697},{"altID":9,"val":0.7735292912}],
"gradientMinValues":[{"altID":7,"val":0.4588347971},{"altID":2,"val":0.5766485929},
{"altID":16,"val":0.4922620654},{"altID":12,"val":0.4345403612},{"altID":3,"val":0.4916979671},
{"altID":11,"val":0.5195798278},{"altID":4,"val":0.4822270274},{"altID":10,"val":0.3205190897},
{"altID":14,"val":0.288048178},{"altID":8,"val":0.5329324603},{"altID":6,"val":0.4181999862},
{"altID":1,"val":0.6940687299},{"altID":15,"val":0.4863931537},{"altID":13,"val":0.4996419549},
{"altID":5,"val":0.392578721},{"altID":9,"val":0.6723327637}],
"gradientInitMinValues":[{"altID":7,"val":0.4588347971},{"altID":2,"val":0.5766485929},
{"altID":16,"val":0.4922620654},{"altID":12,"val":0.4345403612},{"altID":3,"val":0.4916979671},
{"altID":11,"val":0.5195798278},{"altID":4,"val":0.4822270274},{"altID":10,"val":0.3205190897},
{"altID":14,"val":0.288048178},{"altID":8,"val":0.5329324603},{"altID":6,"val":0.4181999862},
{"altID":1,"val":0.6940687299},{"altID":15,"val":0.4863931537},{"altID":13,"val":0.4996419549},
{"altID":5,"val":0.392578721},{"altID":9,"val":0.6723327637}],"values2D":[{}],"color":"#49a737","visible":1},
{"id":21,"idx":4,"name":"Minimize Risks","value":0.1813986301,"initValue":0.1813986301,
"gradientMaxValues":[{"altID":7,"val":0.8541910648},{"altID":2,"val":1},{"altID":16,"val":0.9227401018},
{"altID":12,"val":0.8194691539},{"altID":3,"val":0.8319123983},{"altID":11,"val":1},{"altID":4,"val":1},
{"altID":10,"val":0.7359705567},{"altID":14,"val":0.2947221696},{"altID":8,"val":0.6437422633},
{"altID":6,"val":0.5501263738},{"altID":1,"val":0.6397395134},{"altID":15,"val":0.6039595008},
{"altID":13,"val":0.8834857345},{"altID":5,"val":0.3544708788},{"altID":9,"val":0.647783637}],
"gradientMinValues":[{"altID":7,"val":0.4108674228},{"altID":2,"val":0.5176447034},
{"altID":16,"val":0.4316523671},{"altID":12,"val":0.3490925431},{"altID":3,"val":0.4594484866},
{"altID":11,"val":0.3996228278},{"altID":4,"val":0.4074376523},{"altID":10,"val":0.2139503658},
{"altID":14,"val":0.2953844666},{"altID":8,"val":0.5219194293},{"altID":6,"val":0.4060438573},
{"altID":1,"val":0.733163774},{"altID":15,"val":0.4723603427},{"altID":13,"val":0.39469257},
{"altID":5,"val":0.4519420266},{"altID":9,"val":0.687697649}],
"gradientInitMinValues":[{"altID":7,"val":0.4108674228},{"altID":2,"val":0.5176447034},
{"altID":16,"val":0.4316523671},{"altID":12,"val":0.3490925431},{"altID":3,"val":0.4594484866},
{"altID":11,"val":0.3996228278},{"altID":4,"val":0.4074376523},{"altID":10,"val":0.2139503658},
{"altID":14,"val":0.2953844666},{"altID":8,"val":0.5219194293},{"altID":6,"val":0.4060438573},
{"altID":1,"val":0.733163774},{"altID":15,"val":0.4723603427},{"altID":13,"val":0.39469257},
{"altID":5,"val":0.4519420266},{"altID":9,"val":0.687697649}],"values2D":[{}],
"color":"#f9c74f","visible":1},{"id":62,"idx":5,"name":"Financials","value":0.1908680201,
"initValue":0.1908680201,"gradientMaxValues":[{"altID":7,"val":0.3965010643},{"altID":2,"val":0.5104275942},
{"altID":16,"val":0.5328369737},{"altID":12,"val":0.5066843033},{"altID":3,"val":0.4344977438},
{"altID":11,"val":0.8929135799},{"altID":4,"val":0.928625226},{"altID":10,"val":0.2979159355},
{"altID":14,"val":0.4771152139},{"altID":8,"val":0.5828686357},{"altID":6,"val":0.4829400182},
{"altID":1,"val":0.5908177495},{"altID":15,"val":0.6463311911},{"altID":13,"val":0.6204466224},
{"altID":5,"val":0.5761816502},{"altID":9,"val":0.7212700248}],
"gradientMinValues":[{"altID":7,"val":0.5136446953},{"altID":2,"val":0.6274859905},
{"altID":16,"val":0.5178803205},{"altID":12,"val":0.4173712134},{"altID":3,"val":0.5488365889},
{"altID":11,"val":0.4178574383},{"altID":4,"val":0.4173395932},{"altID":10,"val":0.3111747801},
{"altID":14,"val":0.2523671091},{"altID":8,"val":0.5348533988},{"altID":6,"val":0.4202064872},
{"altID":1,"val":0.7457975745},{"altID":15,"val":0.4608251154},{"altID":13,"val":0.4510211349},
{"altID":5,"val":0.4007828534},{"altID":9,"val":0.670830071}],
"gradientInitMinValues":[{"altID":7,"val":0.5136446953},{"altID":2,"val":0.6274859905},
{"altID":16,"val":0.5178803205},{"altID":12,"val":0.4173712134},{"altID":3,"val":0.5488365889},
{"altID":11,"val":0.4178574383},{"altID":4,"val":0.4173395932},{"altID":10,"val":0.3111747801},
{"altID":14,"val":0.2523671091},{"altID":8,"val":0.5348533988},{"altID":6,"val":0.4202064872},
{"altID":1,"val":0.7457975745},{"altID":15,"val":0.4608251154},{"altID":13,"val":0.4510211349},
{"altID":5,"val":0.4007828534},{"altID":9,"val":0.670830071}],
"values2D":[{}],"color":"#ff7300","visible":1}],
"alts":[{"id":7,"idx":0,"name":"AS/400 Replacements ABC 234",
"value":0.4912857413,"initValue":0.4912857413,"color":"#084e95","visible":0,"event_type":0},
{"id":2,"idx":1,"name":"Cisco Routers","value":0.6051431894,"initValue":0.6051431894,
"color":"#105989","visible":1,"event_type":0},{"id":16,"idx":2,"name":"Customer Service Call Center",
"value":0.5207350254,"initValue":0.5207350254,"color":"#18647D","visible":0,"event_type":0},
{"id":12,"idx":3,"name":"Desktop Replacements","value":0.4344182014,"initValue":0.4344182014,
"color":"#206F71","visible":0,"event_type":0},{"id":3,"idx":4,"name":"EMC Symmetrix",
"value":0.5270129442,"initValue":0.5270129442,"color":"#287A66","visible":1,"event_type":0},
{"id":11,"idx":5,"name":"Firewall and Antivirus Licenses","value":0.5085304379,"initValue":0.5085304379,
"color":"#30855A","visible":0,"event_type":0},{"id":4,"idx":6,"name":"Iron Mountain Backup Service",
"value":0.5149276257,"initValue":0.5149276257,"color":"#38904E","visible":0,"event_type":0},
{"id":10,"idx":7,"name":"Laptop Replacements","value":0.3086440861,"initValue":0.3086440861,
"color":"#49a737","visible":0,"event_type":0},{"id":14,"idx":8,"name":"Mobile Workforce Pocket PCs",
"value":0.2952643037,"initValue":0.2952643037,"color":"#75AF3D","visible":0,"event_type":0},
{"id":8,"idx":9,"name":"Oracle 9i Upgrade","value":0.5440179706,"initValue":0.5440179706,"color":"#A1B743",
"visible":1,"event_type":0},{"id":6,"idx":10,"name":"PeopleSoft Upgrade","value":0.4321802855,
"initValue":0.4321802855,"color":"#CDBF49","visible":0,"event_type":0},
{"id":1,"idx":11,"name":"Plumtree Corporate Portal","value":0.7162168026,"initValue":0.7162168026,
"color":"#f9c74f","visible":1,"event_type":0},{"id":15,"idx":12,"name":"ProServe System Upgrade",
"value":0.4962322414,"initValue":0.4962322414,"color":"#FAB23B","visible":0,"event_type":0},
{"id":13,"idx":13,"name":"Sales Force Laptops","value":0.4833590686,"initValue":0.4833590686,"color":"#FC9D27",
"visible":0,"event_type":0},{"id":5,"idx":14,"name":"SRDF Site/Service","value":0.434260875,
"initValue":0.434260875,"color":"#FD8813","visible":0,"event_type":0},
{"id":9,"idx":15,"name":"Thin Client Implementation","value":0.6804573536,"initValue":0.6804573536,
"color":"#ff7300","visible":1,"event_type":0}],
"comps":[{"altID":7,"objID":9,"comp":0.2218995303},
{"altID":2,"objID":9,"comp":0.3930347121},{"altID":16,"objID":9,"comp":0.4967792673},
{"altID":12,"objID":9,"comp":0.2955086218},{"altID":3,"objID":9,"comp":0.2303225865},
{"altID":11,"objID":9,"comp":0.1651814804},{"altID":4,"objID":9,"comp":0},
{"altID":10,"objID":9,"comp":0.1785545509},{"altID":14,"objID":9,"comp":0.3260715537},
{"altID":8,"objID":9,"comp":0.4651694768},{"altID":6,"objID":9,"comp":0.23134637},
{"altID":1,"objID":9,"comp":1},{"altID":15,"objID":9,"comp":0.4998238237},
{"altID":13,"objID":9,"comp":0.4589859904},{"altID":5,"objID":9,"comp":0.2329474918},
{"altID":9,"objID":9,"comp":0.7934586525},{"altID":7,"objID":11,"comp":0.4812718275},
{"altID":2,"objID":11,"comp":0.5279835268},{"altID":16,"objID":11,"comp":0.1690651487},
{"altID":12,"objID":11,"comp":0.2677386592},{"altID":3,"objID":11,"comp":0.5728338064},
{"altID":11,"objID":11,"comp":0.2973340615},{"altID":4,"objID":11,"comp":0.3149563837},
{"altID":10,"objID":11,"comp":0.2033084046},{"altID":14,"objID":11,"comp":0.1104048599},
{"altID":8,"objID":11,"comp":0.4928722329},{"altID":6,"objID":11,"comp":0.476298539},
{"altID":1,"objID":11,"comp":0.48816368},{"altID":15,"objID":11,"comp":0.2801020387},
{"altID":13,"objID":11,"comp":0.1976124804},{"altID":5,"objID":11,"comp":0.4528645471},
{"altID":9,"objID":11,"comp":0.5292346736},{"altID":7,"objID":16,"comp":0.8630307081},
{"altID":2,"objID":16,"comp":0.9315666562},{"altID":16,"objID":16,"comp":0.84690962},
{"altID":12,"objID":16,"comp":0.4330189316},{"altID":3,"objID":16,"comp":0.9315666562},
{"altID":11,"objID":16,"comp":0.3819528099},{"altID":4,"objID":16,"comp":0.8895330229},
{"altID":10,"objID":16,"comp":0.1726085409},{"altID":14,"objID":16,"comp":0.3779294076},
{"altID":8,"objID":16,"comp":0.6710086808},{"altID":6,"objID":16,"comp":0.5923330862},
{"altID":1,"objID":16,"comp":0.9699354011},{"altID":15,"objID":16,"comp":0.6089448075},
{"altID":13,"objID":16,"comp":0.2968280587},{"altID":5,"objID":16,"comp":0.911754363},
{"altID":9,"objID":16,"comp":0.7735292552},{"altID":7,"objID":21,"comp":0.8541910595},
{"altID":2,"objID":21,"comp":1},{"altID":16,"objID":21,"comp":0.922740133},
{"altID":12,"objID":21,"comp":0.8194691405},{"altID":3,"objID":21,"comp":0.8319124406},
{"altID":11,"objID":21,"comp":1},{"altID":4,"objID":21,"comp":1},{"altID":10,"objID":21,"comp":0.7359705396},
{"altID":14,"objID":21,"comp":0.2947221594},{"altID":8,"objID":21,"comp":0.6437422512},
{"altID":6,"objID":21,"comp":0.5501263817},{"altID":1,"objID":21,"comp":0.6397395242},
{"altID":15,"objID":21,"comp":0.6039595087},{"altID":13,"objID":21,"comp":0.8834857499},
{"altID":5,"objID":21,"comp":0.3544708832},{"altID":9,"objID":21,"comp":0.6477836429},
{"altID":7,"objID":62,"comp":0.3965010529},{"altID":2,"objID":62,"comp":0.5104275768},
{"altID":16,"objID":62,"comp":0.5328370026},{"altID":12,"objID":62,"comp":0.5066843103},
{"altID":3,"objID":62,"comp":0.4344977565},{"altID":11,"objID":62,"comp":0.8929136091},
{"altID":4,"objID":62,"comp":0.928625243},{"altID":10,"objID":62,"comp":0.2979159393},
{"altID":14,"objID":62,"comp":0.4771152101},{"altID":8,"objID":62,"comp":0.5828686122},
{"altID":6,"objID":62,"comp":0.4829400241},{"altID":1,"objID":62,"comp":0.5908177317},
{"altID":15,"objID":62,"comp":0.6463311783},{"altID":13,"objID":62,"comp":0.6204466278},
{"altID":5,"objID":62,"comp":0.5761816788},{"altID":9,"objID":62,"comp":0.7212700122}],
"saUserID":-1,"saUserName":"All Participants"};

// Default options
const ecSensitivityDefOptions = {
    htmlAltsContainer: "",
    htmlObjsContainer: "",
    readOnly: false,
    isMultiView: false,
    normalizationMode: 'unnormalized',
    showComponents: false,
    DSAComponentsData: testData.comps,
    valDigits: 2,
    altFilterValue: -1,
    SAAltsSortBy: 1,
    objs: testData.objs,
    alts: testData.alts,
    objMinNormValues: [],
    objMaxNormValues: [],
    MaxAltValue: 1,
    MinAltValue: 0,
    MaxAltValueNormalized: 1,
    MinAltValueNormalized: 0,
    minNormAltValue: 0,
    maxNormAltValue: 1,
    objGradientSum: [],
    isUpdateFinished: true,
    visibleAlts: [],
    objInitSum: 1,
    showMarkers: false,
    markersType: 1,
    DSAActiveSorting: false,
    DSAObjSorted: false,
    DSAAltSorted: false,
    DSARenormalize: true,
    onSortingEvent: null, //function onSortingEvent(sortMode, sortBy) sortMode = {0:objectives, 1:alternatives}, sortBy = {0: Index, 1: Value, 2: Name} 
    userID: -1,
    userName: "All Participants",
};

var ecSensitivityDefOptionsList = ["normalizationMode", "showComponents", "valDigits", "altFilterValue", "SAAltsSortBy", "showMarkers", "markersType", "DSAActiveSorting", "DSAObjSorted",
    "DSAAltSorted", "userID", "userName"];

(function ($) {
    $.widget("expertchoice.ecDSA", {
        options: ecSensitivityDefOptions,
        _initSA: function () {
            var opt = this.options;
            this._applySAAltsFilter();
            // var objsString = sessionStorage.getItem(opt.sessionID);
            // var saState = JSON.parse(objsString);
            // if ((saState)) {
            //     if (saState.oIds !== []) {
            //         for (var i = 0; i < saState.oIds.length; i++) {
            //             var obj = this.getObjByID(saState.oIds[i]);
            //             if ((obj)) {
            //                 obj.value = saState.oVals[i];
            //             };
            //         };
            //     };
            //     if (saState.aIds !== []) {
            //         for (var i = 0; i < saState.aIds.length; i++) {
            //             var alt = this.getAltByID(saState.aIds[i]);
            //             if ((alt)) {
            //                 alt.value = saState.aVals[i];
            //             };
            //         };
            //     };
            // };
        },
        applyAltsSorting: function () {
            var opt = this.options;
            if (opt.SAAltsSortBy == 0) { opt.alts.sort(SortByIndex) };
            if (opt.SAAltsSortBy == 1) { opt.alts.sort(SortByValue) };
            if (opt.SAAltsSortBy == 2) { opt.alts.sort(SortByName) };
            opt.DSAAltSorted = true;
        },
        applyObjsSorting: function () {
            var opt = this.options;
            if (opt.ASASortBy == 0) { opt.objs.sort(SortByIndex) };
            if (opt.ASASortBy == 1) { opt.objs.sort(SortByValue) };
            if (opt.ASASortBy == 2) { opt.objs.sort(SortByName) };
            opt.DSAObjSorted = true;
        },
        _destroy: function () {

        },
        // Constructor function
        _create: function () {
            var opt = this.options;
            this._updateMinMaxAltBarValue();
            this.applyAltsSorting();
            this.applyObjsSorting();
            // opt.objInitSum = 0;
            // for (var i = 0; i < opt.objs.length; i++) {
            //     var obj = opt.objs[i];
            //     opt.objInitSum += obj.initValue;
            // };
            // if (opt.objInitSum == 0) { opt.objInitSum = 1 };
            // this._initSA();
            var scope = this;
            var objContainer = $(opt.htmlObjsContainer);
            var altContainer = $(opt.htmlAltsContainer);

            var objTMPL = $("#objTemplate").html();
            for (var i = 0; i < opt.objs.length; i++) {
                var obj = opt.objs[i];
                var objHTML = objTMPL.replaceAll("{name}", obj.name)
                .replaceAll("{value}", numberToStr(obj.value * 100, opt.valDigits)) 
                .replaceAll("{initValue}", numberToStr(obj.initValue * 100, opt.valDigits)) 
                .replaceAll("{id}", obj.id)
                .replaceAll("{color}", obj.color);
                var objNode = $(objHTML);
                objNode.appendTo(objContainer);
                $("#obj" + obj.id).on('input', function (e) {
                    var id = $(this).attr("tag") * 1;
                    $("#objVal"+id).html(numberToStr(this.value * 1, opt.valDigits) + "%");
                    scope.setObjLocalPriority(id, this.value / 100);
                    scope.updateAltPriorities(id);
                    for (var i = 0; i < opt.objs.length; i++) {
                        var obj = opt.objs[i];
                        if (obj.id != id) {
                            var displayVal = numberToStr(obj.value * 100, opt.valDigits);
                            $("#obj" + obj.id).val(displayVal);
                            $("#objVal" + obj.id).html(displayVal + "%");
                        }
                    };
                    for (var i = 0; i < opt.alts.length; i++) {
                        var alt = opt.alts[i];
                        var displayVal = numberToStr(alt.value * 100, opt.valDigits);
                        $("#alt" + alt.id).css("width", displayVal + "%");
                        $("#altVal" + alt.id).html(displayVal + "%");
                    };
                });
            };
            var altTMPL = $("#altTemplate").html();
            for (var i = 0; i < opt.alts.length; i++) {
                var alt = opt.alts[i];
                var altHTML = altTMPL.replaceAll("{name}", alt.name)
                .replaceAll("{value}", numberToStr(alt.value * 100, opt.valDigits))
                .replaceAll("{initValue}", numberToStr(alt.initValue * 100, opt.valDigits))
                .replaceAll("{id}", alt.id)
                .replaceAll("{color}", alt.color);
                var altNode = $(altHTML);
                altNode.appendTo(altContainer);
            };
        },
        _getDSAComponentCoef: function (altID, objID) {
            var opt = this.options;
            for (var i = 0; i < opt.DSAComponentsData.length; i++) {
                var comp = opt.DSAComponentsData[i];
                if ((comp.altID == altID) && (comp.objID == objID)) {
                    return comp.comp
                }
            }
            return 1;
        },
        _updateMinMaxAltBarValue: function () {
            var opt = this.options;
            var aMaxVal = 0;
            var aMinVal = 1;
            var aMaxValNorm = 0;
            var aMinValNorm = 1;
            opt.objGradientSum = [];
            for (var i = 0; i < opt.objs.length; i++) {
                var xMaxCoef = 0;
                var obj = opt.objs[i];
                for (var k = 0; k < opt.alts.length; k++) {
                    var altID = opt.alts[k].id;
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                    xMaxCoef += maxVal;
                };
                if (xMaxCoef == 0) { xMaxCoef = 1 };
                opt.objGradientSum.push(xMaxCoef);
                for (var j = 0; j < opt.alts.length; j++) {
                    var altID = opt.alts[j].id;
                    var minVal = this.getGradientValueByAltID(obj.gradientMinValues, altID);
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);

                    if (aMaxVal < minVal) { aMaxVal = minVal };
                    if (aMaxVal < maxVal) { aMaxVal = maxVal };
                    if (aMinVal > minVal) { aMinVal = minVal };
                    if (aMinVal > maxVal) { aMinVal = maxVal };

                    var minValNorm = minVal / xMaxCoef;
                    var maxValNorm = maxVal / xMaxCoef;
                    if (aMaxValNorm < minValNorm) { aMaxValNorm = minValNorm };
                    if (aMaxValNorm < maxValNorm) { aMaxValNorm = maxValNorm };
                    if (aMinValNorm > minValNorm) { aMinValNorm = minValNorm };
                    if (aMinValNorm > maxValNorm) { aMinValNorm = maxValNorm };
                };
            };
            if (aMaxVal > 1) aMaxVal = 1;
            if (aMaxValNorm > 1) aMaxValNorm = 1;
            if (aMaxVal > 0) { opt.MaxAltValue = aMaxVal };
            if (aMinVal < 1) { opt.MinAltValue = aMinVal };
            if (aMaxValNorm > 0) { opt.MaxAltValueNormalized = aMaxValNorm };
            if (aMinValNorm < 1) { opt.MinAltValueNormalized = aMinValNorm };
        },
        _getMaxScaleValue: function () {
            var opt = this.options;
            var aMaxVal = 0;
            for (var i = 0; i < opt.objs.length; i++) {
                var obj = opt.objs[i];
                var xMaxCoef = 0;
                if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) {
                    for (var k = 0; k < opt.alts.length; k++) {
                        var altID = opt.alts[k].id;
                        var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                        xMaxCoef += maxVal;
                    };
                };
                if (xMaxCoef == 0) { xMaxCoef = 1 };
                for (var j = 0; j < opt.alts.length; j++) {
                    var altID = opt.alts[j].id;
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID) / xMaxCoef;
                    if (aMaxVal < maxVal) { aMaxVal = maxVal };
                };
            };
            if (aMaxVal > 0) { return aMaxVal };
            return 1;
        },
        _getMaxObjScaleValue: function (obj) {
            var opt = this.options;
            var aMaxVal = 0;
            var xMaxCoef = 0;
            if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) {
                for (var k = 0; k < opt.alts.length; k++) {
                    var altID = opt.alts[k].id;
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                    xMaxCoef += maxVal;
                };
            };
            if (xMaxCoef == 0) { xMaxCoef = 1 };
            for (var j = 0; j < opt.alts.length; j++) {
                var altID = opt.alts[j].id;
                var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID) / xMaxCoef;
                if (aMaxVal < maxVal) { aMaxVal = maxVal };
            };
            if (aMaxVal > 0) { return aMaxVal };
            return 1;
        },
        _getAltNormCoef: function () {
            var normCoef = 0;
            switch (this.options.normalizationMode) {
                case 'normMax100':
                    for (var i = 0; i < this.options.alts.length; i++) {
                        var alt = this.options.alts[i];
                        if (normCoef < alt.value) { normCoef = alt.value }
                    };
                    break;
                case 'normAll':
                    for (var i = 0; i < this.options.alts.length; i++) {
                        var alt = this.options.alts[i];
                        normCoef += alt.value;
                    };
                    break;
                case 'normSelected':
                    for (var i = 0; i < this.options.alts.length; i++) {
                        var alt = this.options.alts[i];
                        if (alt.visible == 1) { normCoef += alt.value };
                    };
                    break;
                default:
                    return 1;
            };
            if (normCoef == 0) { return 1 } else { return normCoef };
        },
        _setOption: function (key, value) {
            var opt = this.options;
            this._super(key, value);
            if (key === 'objs') {
                if (opt.SelectedObjectiveIndex > opt.objs.length - 1) opt.SelectedObjectiveIndex = 0;
                opt.SelectedObjectiveIndexY = 0;
                this._initSA();
                this.applyObjsSorting();
                opt.ObjScrollShift = 0
            };
            if (key === 'alts') {
                this._applySAAltsFilter();
                this._initSA();
                this.applyAltsSorting();
                opt.AltScrollShift = 0
            };
            if (key === 'altFilterValue') {
                this._applySAAltsFilter();
                this._initSA();
            };
        },
        _setOptions: function (options) {
            this._super(options);
        },
        _applySAAltsFilter: function () {
            var opt = this.options;
            opt.visibleAlts = [];
            for (var i = 0; i < opt.alts.length; i++) {
                var alt = opt.alts[i];
                if (alt.visible == 1) {
                    opt.visibleAlts.push(alt);
                }
            }
        },
        resetSA: function () {
            var opt = this.options;
            this._updateMinMaxAltBarValue();
            for (var i = 0; i < opt.alts.length; i++) {
                var alt = opt.alts[i];
                alt.value = alt.initValue;
            };
            for (var i = 0; i < opt.objs.length; i++) {
                var obj = opt.objs[i];
                obj.value = obj.initValue;
            };
            for (var i = 0; i < opt.objs.length; i++) {
                for (var j = 0; j < opt.alts.length; j++) {
                    var obj = opt.objs[i];
                    obj.gradientMinValues[j] = obj.gradientInitMinValues[j];
                };
            };

            this.applyAltsSorting();
            this.applyObjsSorting();

            var saSavedState = JSON.stringify({ oVals: [], oIds: [], aVals: [], aIds: [] });
            sessionStorage.setItem(opt.sessionID, saSavedState);
            if (opt.isMultiView) this.syncSACharts(false);
        },
        _calcualteNormalizedValues: function () {
            var opt = this.options;
            opt.minNormAltValue = 1;
            opt.maxNormAltValue = 0;
            var obj = opt.objs[opt.SelectedObjectiveIndex];
            var minValues = [];
            var maxValues = [];
            for (var i = 0; i < opt.alts.length; i++) {
                var alt = opt.alts[i];
                if (((opt.normalizationMode == 'normSelected') && (alt.visible == 1)) || (opt.normalizationMode !== 'normSelected')) {
                    if ((obj) && (obj.gradientMinValues)) minValues.push(this.getGradientValueByAltID(obj.gradientMinValues, alt.id));
                    if ((obj) && (obj.gradientMaxValues)) maxValues.push(this.getGradientValueByAltID(obj.gradientMaxValues, alt.id));
                };
            };
            var minSum = 0;
            var maxSum = 0;
            for (var i = 0; i < minValues.length; i++) {
                minSum += minValues[i];
                maxSum += maxValues[i];
            };
            opt.objMinNormValues = [];
            opt.objMaxNormValues = [];
            for (var i = 0; i < minValues.length; i++) {
                opt.objMinNormValues.push(minValues[i] / minSum);
                opt.objMaxNormValues.push(maxValues[i] / maxSum);
            };

            for (var i = 0; i < opt.objMinNormValues.length; i++) {
                if (opt.minNormAltValue > opt.objMinNormValues[i]) { opt.minNormAltValue = opt.objMinNormValues[i] };
                if (opt.maxNormAltValue < opt.objMinNormValues[i]) { opt.maxNormAltValue = opt.objMinNormValues[i] };
                if (opt.minNormAltValue > opt.objMaxNormValues[i]) { opt.minNormAltValue = opt.objMaxNormValues[i] };
                if (opt.maxNormAltValue < opt.objMaxNormValues[i]) { opt.maxNormAltValue = opt.objMaxNormValues[i] };
            };
        },
        _calculateOverallMinMaxValues: function () {
            var opt = this.options;
            opt.minNormAltValue = 1;
            opt.maxNormAltValue = 0;
            opt.maxSum = [];
            for (var j = 0; j < opt.objs.length; j++) {
                var obj = opt.objs[j];
                var minValues = [];
                var maxValues = [];
                for (var i = 0; i < opt.alts.length; i++) {
                    var alt = opt.alts[i];
                    if (((opt.normalizationMode == 'normSelected') && (alt.visible == 1)) || (opt.normalizationMode !== 'normSelected')) {
                        minValues.push(this.getGradientValueByAltID(obj.gradientMinValues, alt.id));
                        maxValues.push(this.getGradientValueByAltID(obj.gradientMaxValues, alt.id));
                    };
                };
                var minSum = 0;
                var maxSum = 0;
                for (var i = 0; i < minValues.length; i++) {
                    minSum += minValues[i];
                    maxSum += maxValues[i];
                };
                opt.maxSum.push(maxSum);
                var objMinNormValues = [];
                var objMaxNormValues = [];
                for (var i = 0; i < minValues.length; i++) {
                    objMinNormValues.push(minValues[i] / minSum);
                    objMaxNormValues.push(maxValues[i] / maxSum);
                };

                for (var i = 0; i < objMinNormValues.length; i++) {
                    if (opt.minNormAltValue > objMinNormValues[i]) { opt.minNormAltValue = objMinNormValues[i] };
                    if (opt.maxNormAltValue < objMinNormValues[i]) { opt.maxNormAltValue = objMinNormValues[i] };
                    if (opt.minNormAltValue > objMaxNormValues[i]) { opt.minNormAltValue = objMaxNormValues[i] };
                    if (opt.maxNormAltValue < objMaxNormValues[i]) { opt.maxNormAltValue = objMaxNormValues[i] };
                };
            };
        },
        _getNextVisibleObjIndex: function (idx) {
            for (var i = idx; i < this.options.ASAdata.objectives.length; i++) {
                var obj = this.options.ASAdata.objectives[i];
                if ((obj.visible === 1)&&(this.options.ASAPageObjs.indexOf(obj) >= 0)) { return i; };
            };
            return -1;
        },
        GradientMinValues: function (value) {
            for (var i = 0; i < value.length; i++) {
                var objItem = value[i];
                var objID = objItem[0];
                var obj = this.getObjByID(objID);
                obj.gradientMinValues = objItem[1];
            };
            this._updateMinMaxAltBarValue();
            this.redrawSA();
            this.options.IsObjValuesChanged = false;
        },
        getGradientValueByAltID: function (gradientValues, altID) {
            if (typeof gradientValues !== "undefined") {
                for (var i = 0; i < gradientValues.length; i++) {
                    var itm = gradientValues[i];
                    if ((itm)) {
                        if (itm.altID == altID) {
                            return itm.val;
                        }
                    }
                }
            }
            return 0;
        },
        setSelectedObjectiveIndex: function (ObjIndex) {
            this.options.SelectedObjectiveIndex = ObjIndex;
        },
        updateContent: function (x, y) {
            var opt = this.options;
            if (opt.isMouseDown) {
                if ((opt.SelectedObjectiveIndex < opt.objs.length) && (opt.SelectedObjectiveIndex >= 0)) {
                    var NewObjValue = 1;
                    if (opt.viewMode === 'DSA') {
                        NewObjValue = ((x - 10) / (opt.canvasHalfWidth - 30)); // * opt.objInitSum;
                    };
                    var aObj = opt.objs[opt.SelectedObjectiveIndex];
                    if (NewObjValue != aObj.value) { opt.IsObjValuesChanged = true };
                    this.setObjLocalPriority(aObj.id, NewObjValue);
                    this.updateAltPriorities(aObj.id);
                    if (opt.SAAltsSortBy == 1) {
                        opt.DSAAltSorted = isSortedByValue(opt.alts);
                    };
                    if (opt.SAAltsSortBy == 2) {
                        opt.DSAAltSorted = isSortedByName(opt.alts);
                    };
                    if (opt.SAAltsSortBy == 0) {
                        opt.DSAAltSorted = isSortedByIndex(opt.alts);
                    };
                    if (opt.ASASortBy == 1) {
                        opt.DSAObjSorted = isSortedByValue(opt.objs);
                    };
                    if (opt.ASASortBy == 2) {
                        opt.DSAObjSorted = isSortedByName(opt.objs);
                    };
                    if (opt.ASASortBy == 0) {
                        opt.DSAObjSorted = isSortedByIndex(opt.objs);
                    };
                    if (opt.isMultiView) this.syncSACharts(true, aObj.id, NewObjValue);
                };
                this.redrawSA();
            };
            opt.isUpdateFinished = true;
        },
        _dsaUpdateCurrentValues: function () {
            var values = "";
            var objids = "";
            var oVals = [];
            var oIds = [];
            var aVals = [];
            var aIds = [];
            for (var i = 0; i < this.options.objs.length; i++) {
                var obj = this.options.objs[i];
                values += (values == "" ? "" : ",") + obj.value;
                objids += (objids == "" ? "" : ",") + obj.id;
                oVals.push(obj.value);
                oIds.push(obj.id);
            };
            for (var i = 0; i < this.options.alts.length; i++) {
                var alt = this.options.alts[i];
                aVals.push(alt.value);
                aIds.push(alt.id);
            };
            var saSavedState = JSON.stringify({oVals:oVals, oIds:oIds, aVals:aVals, aIds:aIds});
            sessionStorage.setItem(this.options.sessionID, saSavedState);
            if (typeof this.options.onMouseUpEvent == "function") {
                this.options.onMouseUpEvent(values, objids, this);
            };
        },
        getObjByID: function (ObjID) {
            return $.grep(this.options.objs, function (e) { return e.id === ObjID; })[0];
        },
        getAltByID: function (AltID) {
            AltID = AltID * 1;
            var opt = this.options;
            var alts = opt.alts;
            var selAlt = alts.find(x => x.id === AltID); //$.grep(alts, function (e) { return e.id === AltID; })[0];
            return selAlt;
        },
        setAltColor: function (AltID, color) {
            var alt = this.getAltByID(Number(AltID));
            if ((alt)) alt.color = color;
            this.redrawSA();
        },
        setObjLocalPriority: function (ObjID, ObjValue) {
            var objSum = this.options.objInitSum;
            if (ObjValue < 0) { ObjValue = 0 };
            if (ObjValue > objSum) { ObjValue = objSum };
            if (ObjValue > 1) { ObjValue = 1 };
            var obj = this.getObjByID(ObjID);
            obj.value = ObjValue;
            total = 0;
            coef = 1;
            for (var i = 0; i < this.options.objs.length; i++) {
                var aObj = this.options.objs[i];
                if (aObj.id != ObjID) {
                    total += aObj.value;
                };
            };
            if (total <= 0) {
                coef = 0;
            } else {
                coef = (objSum - ObjValue) / total;
            };
            for (var i = 0; i < this.options.objs.length; i++) {
                var aObj = this.options.objs[i];
                if (aObj.id != ObjID) {
                    aObj.value *= coef;
                    if (aObj.value <= 0.000001) {
                        aObj.value = 0.00001 * aObj.initValue;
                    };
                    if (aObj.value > 1) { aObj.value = 1 };
                };
            };
        },
        updateAltPriorities: function (ObjID) {
            var opt = this.options;
            for (var i = 0; i < opt.alts.length; i++) {
                var alt = opt.alts[i];
                var obj = this.getObjByID(ObjID);
                var aMinValue = this.getGradientValueByAltID(obj.gradientMinValues, alt.id);
                var aMaxValue = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);;
                alt.value = aMinValue + (aMaxValue - aMinValue) * obj.value;
            };
            
            if (opt.viewMode === 'DSA' && opt.DSAActiveSorting) {
                this.sortAlternatives()
            };
            if ((opt.DSAActiveSorting && opt.isMultiView) || (!opt.isMultiView)) {
                if ((opt.viewMode === 'GSA') && opt.GSAShowLegend) { this.sortAlternatives() };
                if ((opt.viewMode === 'ASA') && opt.GSAShowLegend) { opt.alts.sort(SortByValue) };
            };
        },
        sortAlternatives: function () {
            this.applyAltsSorting();
        },
        getChangedOptions: function () {    // D6585
            var opt = this.options;
            var opts = ecSensitivityDefOptions;
            var curOptions = {};
            for (var i = 0; i < ecSensitivityDefOptionsList.length; i++) {
                var prop = ecSensitivityDefOptionsList[i];
                if (Object.prototype.hasOwnProperty.call(opts, prop)) {
                    if (opts[prop] !== opt[prop]) {
                        curOptions[prop] = opt[prop];
                    }
                }
            }
            return curOptions;
        },
    });
})(jQuery);

function numberToStr(n, decimals) {
    var n1 = Number(n.toFixed(decimals));
    var n2 = Number(n.toFixed());
    if (n1 == n2) {
        return (n).toFixed()
    } else {
        return n.toFixed(decimals)
    };
}

function SortByValue(a, b) {
    return ((a.value < b.value) ? 1 : ((a.value > b.value) ? -1 : 0));
}

function SortByIndex(a, b) {
    return ((a.idx > b.idx) ? 1 : ((a.idx < b.idx) ? -1 : 0));
}

function SortByName(a, b) {
    return ((a.name.toLowerCase() > b.name.toLowerCase()) ? 1 : ((a.name.toLowerCase() < b.name.toLowerCase()) ? -1 : 0));
}

function SortByPriority(a, b) {
    return ((a.prty < b.prty) ? 1 : ((a.prty > b.prty) ? -1 : 0));
}

function isSortedByName(arr) {
    var sorted = true;
    for (var i = 0; i < arr.length - 1; i++) {
        if (arr[i].name > arr[i + 1].name) {
            sorted = false;
            break;
        }
    };
    return sorted;
}

function isSortedByValue(arr) {
    var sorted = true;
    for (var i = 0; i < arr.length - 1; i++) {
        if (arr[i].value < arr[i + 1].value) {
            sorted = false;
            break;
        }
    };
    return sorted;
}

function isSortedByIndex(arr) {
    var sorted = true;
    for (var i = 0; i < arr.length - 1; i++) {
        if (arr[i].idx > arr[i + 1].idx) {
            sorted = false;
            break;
        }
    };
    return sorted;
}