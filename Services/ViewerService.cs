using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SkiaSharp;
using Uno.Files.Options;
using Uno.Files.Options.Viewer;
using Uno.Files.Viewer;

public class ViewerService
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _accessor;

    public ViewerService(IWebHostEnvironment hostingEnvironment, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
    {
        _hostingEnvironment = hostingEnvironment;
        _cache = cache;
        _accessor = httpContextAccessor;
    }

    [JSInvokable]
    public string GetViewerSettings()
    {
        var viewerSettings = new ViewerSettings
        {
            LangFile = "en.json"
        };

        viewerSettings.PageSettings.PageMode = PageModeEnum.pan.ToString();

        viewerSettings.PageSettings.FitType = FitTypeEnum.width.ToString();
        viewerSettings.PageSettings.AutoFitPage = true;

        viewerSettings.PageSettings.AutoCopyText = true;
        viewerSettings.PageSettings.SelectTextColor = "gray";
        viewerSettings.PageSettings.CopyTextColor = "lime";

        viewerSettings.PageSettings.PageStatusLocation = PageStatusLocationEnum.bottom_right.ToString();
        viewerSettings.PageSettings.PageLayout = PageLayoutEnum.multiplePages.ToString();
        viewerSettings.PageSettings.ShowPageBusy = false;
        viewerSettings.ZoomSettings.PageZoom = 30;

        viewerSettings.SearchSettings.ActiveColor = "red";
        viewerSettings.SearchSettings.BorderStyle = "2px dashed black";
        viewerSettings.SearchSettings.BackColor = "lime";

        viewerSettings.ThumbSettings.ThumbImageQuality = 25;
        viewerSettings.ThumbSettings.ThumbWidth = 150;

        return JsonConvert.SerializeObject(viewerSettings, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
    }

    public FileOpenResult OpenFile(string fileName, string? password = "")
    {
        var pathToFile = Path.Combine(Path.Combine(_hostingEnvironment.WebRootPath, @"files\"), fileName);

        if (!File.Exists(pathToFile))
        {
            return new FileOpenResult { Message = $"File does not exists: {pathToFile}" };
        }

        var fileInfo = new FileInfo(pathToFile);

        var licenseFilePath = Path.Combine(Path.Combine(_hostingEnvironment.WebRootPath, "unoViewer"), "UnoViewer.xml.licx");

        var waterMark = new WaterMark
        {
            TextMark = "Hello World",
            Color = SKColors.Green,
            Font = new SKFont(SKTypeface.FromFamilyName("Verdana"), 30),
            Opacity = 20,
            Angle = -45,
            ShowOnCorners = true
        };

        var waterMarkString = UnoViewer.ApplyWatermark(waterMark);

        var viewOptions = new ViewOptions
        {
            Password = Convert.ToString(password),
            ImageResolution = 200,
            WatermarkInfo = waterMarkString,
            TimeOut = 30
        };

        var ctlUno = new UnoViewer(_cache, _accessor, licenseFilePath, viewOptions);


        BaseOptions? loadOptions = null;
        var pdfOptions = new PdfOptions { ExtractTexts = true, ExtractHyperlinks = true, AllowSearch = true };


        switch (fileInfo.Extension.ToUpper())
        {
            case ".DOC":
            case ".DOCX":
            case ".DOT":
            case ".DOTX":
            case ".ODT":
            case ".TXT":
                loadOptions = new WordOptions { ConvertPdf = true, PdfOptions = pdfOptions, ImageResolution = 200 };
                break;
            case ".XLS":
            case ".XLSX":
            case ".ODS":
                loadOptions = new ExcelOptions { ExportOnePagePerWorkSheet = true, ShowEmptyWorkSheets = true, AutoFitContents = true, PdfOptions = pdfOptions };
                break;
            case ".PPT":
            case ".PPS":
            case ".PPTX":
            case ".ODP":
                loadOptions = new PptOptions { ConvertPdf = true, PdfOptions = pdfOptions };
                break;

            case ".PDF":
                loadOptions = pdfOptions;
                break;
        }

        if (null != loadOptions)
        {
            try
            {
                var token = ctlUno.ViewFile(pathToFile, loadOptions);

                return new FileOpenResult { Success = true, Message = ctlUno.Token };

            }
            catch (Exception e)
            {
                return new FileOpenResult { Message = e.Message };
            }
        }

        return new FileOpenResult { Message = "Error, Invalid file type" };
    }
}

public class FileOpenResult
{
    public FileOpenResult()
    {
        Success = false;
        Message = string.Empty;
    }

    public bool Success { get; set; }
    public string Message { get; set; }

}