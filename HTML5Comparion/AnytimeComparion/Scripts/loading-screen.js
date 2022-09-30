var countBackUp = 0;
var LoadingScreen = {
    type : {
        'loadingModal': 1,
        'yellowLoadingIcon': 2,
        'smallLoadingIcon' : 3
    },
    getType: 1,
    setType: function (value) {
        this.getType = value;
        return this.getType;
    },
    count: 0,
    intervalObject: null,
    loadingPercentageElement: null,
    loadingElement: null,
    viewElement: null,
    start: function (end, loadingMessage) {
        clearInterval(this.intervalObject);
        var type = this.getType;
        var count = LoadingScreen.count;
        var element = this.loadingPercentageElement;
        
        switch(type)
        {
            case this.type.loadingModal:
                {
                    //if (loadingMessage)
                        //$(".LoadingModalPercentage").html(loadingMessage[0]);
                    $(this.loadingElement).removeClass("hide");
                    loadingMessage = typeof (loadingMessage) != "undefined" ? loadingMessage : false;
                    if (loadingMessage)
                        $(".LoadingModalMessage").html(loadingMessage);
                    this.intervalObject = setInterval(
                        function () {
                            var step = (end - count) / end;
                            count += step;
                            LoadingScreen.count = count;
                            //console.log(this.count);
                            countBackUp = LoadingScreen.count;
                            if (LoadingScreen.count >= end) {
                                clearInterval(LoadingScreen.intervalObject);
                            }
                            $(element).html(LoadingScreen.count.toFixed(0) + '%');

                        });
                    break;
                }
            case this.type.smallLoadingIcon:
                {
                    loadingMessage = typeof (loadingMessage) != "undefined" ? loadingMessage : false;
                    if (loadingMessage)
                        $(".LoadingModalMessage").html(loadingMessage);
                    this.intervalObject = 
                        function(){
                            var step = (end - count) / end;
                            count += step;
                            LoadingScreen.count = count;
                            //console.log(this.count);
                            countBackUp = LoadingScreen.count;
                            if (LoadingScreen.count >= end) {
                                clearInterval(LoadingScreen.intervalObject);
                            }
                            $(element).html(LoadingScreen.count.toFixed(0) + '%');

                        };
                    break;
                }
        }
    },
    end: function (end, finish, loadingMessage) {
        var count = LoadingScreen.count;
        if (LoadingScreen.count == 0)
            count = countBackUp;
        var element = this.loadingPercentageElement;
        var loadingElement = this.loadingElement;
        var viewElement  = this.viewElement;
        clearInterval(this.intervalObject);

        if (finish) {
            $(element).html(100 + '%');
            setTimeout(function () {
                $(loadingElement).addClass("hide");
                $(viewElement).removeClass("hide");
            }, 500);
            LoadingScreen.reset();
        }
        this.intervalObject = setInterval(
            function () {
                count += 1;
                LoadingScreen.count = count;
                if (LoadingScreen.count >= end) {
                    $(element).html(100 + '%');
                    setTimeout(function () {
                        $(loadingElement).addClass("hide");
                        $(viewElement).removeClass("hide");
                    }, 500);
                    clearInterval(LoadingScreen.intervalObject);
                    LoadingScreen.reset();
                    
                }
                else {
                    $(element).html(LoadingScreen.count.toFixed(0) + '%');
                }
            }, 5);
    },
    reset: function () {
        this.count = 0;
        countBackUp = 0;
        this.getType = 1;
        this.intervalObject = null;
        this.loadingPercentageElement = null;
        this.loadingElement = null;
        this.viewElement = null;
    },
    init: function (type, loadingPercentageElement, loadingElement, viewElement) {
        this.setType(type);
        this.loadingPercentageElement = loadingPercentageElement;  // the element on where to put the percentage value
        this.loadingElement = loadingElement;                       // the whole element of the loading element
        this.viewElement = viewElement;                             // the element that will be shown after loading is complete
    }

};

//var intermediateResults = (function ($) {
//    //var baseUrl = $("#base_url").attr("href");
    
//    return {
//        init: init
//    };


//})($);
//$(intermediateResults.init);