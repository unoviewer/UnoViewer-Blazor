
/* global */

var objUno = null;
var resizing = false;
var isMobile = IsMobile();

function InitViewer(viewerSettings) {

    if (null !== viewerSettings && "undefined" !== viewerSettings) {

        objUno = $("#div_ctlDoc").unoViewer(JSON.parse(viewerSettings));

        /* Build the UI */
        objUno.UI();

        /* Attach all events */

        objUno.on('linkClicked', function (e) {

            if (e.url && e.url.length > 0) {
                window.open(e.url);
            }
            else if (e.meta && e.meta.length > 0) {
                alert(e.meta);
            }

        });

        objUno.on('clipboardCopied', function (e) {

            navigator.clipboard.writeText(e.clipboard).then(() => {
                // alert("Successfully copied.");
            });

            // Optional call to change page to current
            // objUno.GotoPage(e.page);

        });

        objUno.on('textCopied', function (e) {

            navigator.clipboard.writeText(e.text).then(() => {
                // alert("Successfully copied.");
            });

        });

        objUno.on('viewerError', function (e) {
            alert(e.msg);
        });

        objUno.on('pageClicked', function (e) {
            // alert(e.page);
        });

        objUno.on('thumbnailClicked', function (e) {
            // alert(e.thumb);
        });

        objUno.on('fileOpen', function () {
            alert("File open called.");
        });

        objUno.on('fileClose', function () {
            objUno.Close(true);
        });

        objUno.on('splitterResized', function () {

        });

        objUno.on('viewerBusy', function () {

        });

        objUno.on('languageChanged', function (e) {
            // alert(e.langName);

            setTimeout(function () {

                $(".menuWord .unoText").html("Word");
                $(".menuExcel .unoText").html("Excel");
                $(".menuPowerPoint .unoText").html("PowerPoint");
                $(".menuPdf .unoText").html("PDF");

            }, 500);

        });

        objUno.on('themeChanged', function (e) {
            // alert(e.themeName);
        });

        objUno.on('viewerReady', function () {
            $("#imgWait").hide();
        });

        objUno.on('pinchEnd', function (e) {

            objUno.ZoomStep(20);

            if (e.zoomMode == "ZoomIn") {
                objUno.Zoom(true);
            }
            else {
                objUno.Zoom(false);
            }

            objUno.ZoomStep(10);
        });

        /* End attaching events */

    }
}

function OpenToken(token) {

    if (null != objUno) {

        objUno.Close(true);

        objUno.View(token);

        objUno.Loading(false);
    }
}

function OpenFile(fName) {

    window.viewJsInterop.viewFile(fName, "");
}

window.viewJsInterop = {
    dotNetHelper: null,
    setDotNetHelper: function (dotNetObjectRef) {
        this.dotNetHelper = dotNetObjectRef;
    },
    viewFile: function (fileName, password) {
        if (this.dotNetHelper) {

            this.dotNetHelper.invokeMethodAsync('OpenFile', fileName, password).then(openResult => {

                if (null !== openResult && openResult.success == true) {

                    objUno.Loading(true);

                    OpenToken(openResult.message);
                }
                else {
                    alert("Error opening " + fileName);
                }
            });

        }
    }
};

function ShowWait() {
    if (null != objUno) {
        objUno.Loading(true);
    }
}

function CloseFile() {
    if (null != objUno) {
        objUno.Close(true);
        objUno.Loading(true);
    }
}

function SetViewerHeight() {
    if (resizing === true) { return; }

    resizing = true;

    setTimeout(function () {

        var h = "innerHeight" in window ? window.innerHeight : document.documentElement.offsetHeight;

        var reduceH = 90;
        var windowW = parseInt(window.innerWidth);
        var navW = parseInt($("#navMenu").width());

        var w = windowW - navW;

        if (w <= windowW / 2) {
            reduceH = reduceH + 50;
            w = windowW;

            if (null != objUno) {
                objUno.ShowThumbnails(false);
            }
        }
        else {

            if (null != objUno) {
                objUno.ShowThumbnails(true);
            }
        }

        $("#divUnoViewer").height(h - reduceH).width(w - 50).show();

        resizing = false;

    }, 500);
}

function IsMobile() {
    var _isMobile = (window.navigator.maxTouchPoints || 'ontouchstart' in document);
    return _isMobile;
}

function AttachEvents() {

    // Attach resize event
    $(window).on("load resize orientationchange", function () {
        SetViewerHeight();
    });

    // Prevent postback on enter key
    $(document).on("keypress", function (e) {
        if (e.keyCode === 13) {
            e.preventDefault();
            return false;
        }
    });

    // Change theme ( optional )
    // objUno.ThemeFile("dark.json");

    // Hide thumbnails when mobile ( optional )

    if (isMobile) {
        objUno.ShowThumbnails(false);
    }

    // Additional menu items

    $(".menuWord .unoText").html("Word");
    $(".menuExcel .unoText").html("Excel");
    $(".menuPowerPoint .unoText").html("PowerPoint");
    $(".menuPdf .unoText").html("PDF");

    $(".menuWord").on("click", function () { OpenFile("Sample.docx"); });
    $(".menuExcel").on("click", function () { OpenFile("Sample.xls"); });
    $(".menuPowerPoint").on("click", function () { OpenFile("Sample.ppt"); });
    $(".menuPdf").on("click", function () { OpenFile("Sample.pdf"); });

    $("#imgWait").hide();

}

