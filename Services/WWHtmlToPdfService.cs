using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using RazorLight;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace Webwonders.Extensions;

public interface IWWHtmlToPdfService
{
    (bool success, MemoryStream? stream) GetPdfMemoryStream(string pdfType, string viewName, object viewModel, WWHtmlToPdfSettings? settings = null);

}


public class WWHtmlToPdfService : IWWHtmlToPdfService
{

    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IConverter _converter;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IWWCacheService _cacheService;


    public WWHtmlToPdfService(IWebHostEnvironment webHostEnvironment,
                              IConverter converter,
                              IUmbracoContextAccessor umbracoContextAccessor,
                              IWWCacheService cacheService)
    {
        _webHostEnvironment = webHostEnvironment;
        _converter = converter;
        _umbracoContextAccessor = umbracoContextAccessor;
        _cacheService = cacheService;
    }




    /// <summary>
    /// Get the pdf as memorystream
    /// This expects a PdfType which is also the folder in the Views/Pdf where the views need to be located.
    /// The main view has to be passed as parameter. It is possible to include a header and footer view. When none are given, the default header and footer will be used.
    /// These are called Header.cshtml and Footer.cshtml and are located in the Views/Pdf folder. The given viewmodel is passed to the view and the header and footer.
    /// A custom header and footer can be given by passing the urls in the settings, the viewmodel will be passed there as well.
    /// There are booleans to indicate no header and/or footer should be used.
    /// </summary>
    /// <param name="pdfType">Type of the pdf (subfolder of views/pdf that will be searched)</param>
    /// <param name="viewName">Name of the view (in view/pdf/{type})</param>
    /// <param name="viewModel">Model to pass to view and if necessary to header and/or footer</param>
    /// <param name="settings">Settings of pdf</param>
    /// <returns>bool success </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public (bool success, MemoryStream? stream) GetPdfMemoryStream(string pdfType, string viewName, object viewModel, WWHtmlToPdfSettings? settings = null)
    {
        if (String.IsNullOrWhiteSpace(pdfType))
        {
            throw new ArgumentNullException(nameof(pdfType));
        }
        if (String.IsNullOrWhiteSpace(viewName))
        {
            throw new ArgumentNullException(nameof(viewName));
        }

        settings ??= new WWHtmlToPdfSettings(); // when settings null: all defaultvalues


        if (_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? context) && context != null)
        {
            var domain = context.CleanedUmbracoUrl.GetLeftPart(UriPartial.Authority);

            if (settings.UseHeaderHtml && String.IsNullOrWhiteSpace(settings.HeaderHtmlUrl))
            {
                settings.HeaderHtmlUrl = $"{domain.EnsureEndsWith('/')}umbraco/WebwondersExtensions/WWHtmlToPdfSurface/GetHeaderHtml?type={pdfType}";
            }

            if (settings.UseFooterHtml && String.IsNullOrWhiteSpace(settings.FooterHtmlUrl))
            {
                settings.FooterHtmlUrl = $"{domain.EnsureEndsWith('/')}umbraco/WebwondersExtensions/WWHtmlToPdfSurface/GetFooterHtml?type={pdfType}";
            }
        }

        string contentRootPath = _webHostEnvironment.ContentRootPath;
        string pdfPath = Path.Combine(contentRootPath, $"Views\\pdf\\{pdfType}\\");


        var engine = new RazorLightEngineBuilder()
                         .UseFileSystemProject(pdfPath)
                         .UseMemoryCachingProvider()
                         .Build();

        string htmlString = engine.CompileRenderAsync(viewName, viewModel).Result;

        if (!String.IsNullOrWhiteSpace(htmlString))
        {
            string cacheKey = string.Empty;

            // If header and/or footer: write to cache with a generated Guid key and add to url
            if (settings.UseHeaderHtml || settings.UseFooterHtml)
            {
                cacheKey = Guid.NewGuid().ToString();
                _cacheService.InsertCacheItem<object>(cacheKey, () => { return viewModel; }, new TimeSpan(0, 10, 0)); // cache for 10 minutes, will be cleared after pdf is created

                if (settings.UseHeaderHtml && !String.IsNullOrWhiteSpace(settings.HeaderHtmlUrl))
                {
                    settings.HeaderHtmlUrl = $"{settings.HeaderHtmlUrl}&key={cacheKey}";
                }
                if (settings.UseFooterHtml && !String.IsNullOrWhiteSpace(settings.FooterHtmlUrl))
                {
                    settings.FooterHtmlUrl = $"{settings.FooterHtmlUrl}&key={cacheKey}";
                }
            }


            IDocument document = CreatePdfDocument(htmlString, settings);

            byte[] pdf = _converter.Convert(document);

            // If cached: clear the cache
            if (settings.UseHeaderHtml || settings.UseFooterHtml)
            {
                _cacheService.ClearCacheItem(cacheKey);
            }

            if (pdf != null)
            {
                return (true, new MemoryStream(pdf));
            }

        }

        return (false, null);

    }




    /// <summary>
    /// Create the IDocument by copying the settings and the html string
    /// </summary>
    /// <param name="html"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    private static IDocument CreatePdfDocument(string html, WWHtmlToPdfSettings settings)
    {
        return new HtmlToPdfDocument
        {
            GlobalSettings = {
                Orientation = settings.Orientation == null ? WkHtmlToPdfDotNet.Orientation.Portrait : (WkHtmlToPdfDotNet.Orientation)settings.Orientation,
                ColorMode = settings.ColorMode == null ? WkHtmlToPdfDotNet.ColorMode.Color : (WkHtmlToPdfDotNet.ColorMode)settings.ColorMode,
                UseCompression = settings.UseCompression,
                DPI = settings.DPI,
                PageOffset = settings.PageOffset,
                Copies = settings.Copies,
                Collate = settings.Collate,
                Outline = settings.Outline,
                OutlineDepth = settings.OutlineDepth,
                DumpOutline = settings.DumpOutline,
                Out = settings.Out,
                DocumentTitle = settings.DocumentTitle,
                ImageDPI = settings.ImageDPI,
                ImageQuality = settings.ImageQuality,
                CookieJar = settings.CookieJar,
                PaperSize = settings.PaperSize,
                Margins = settings.Margins,
            },
            Objects = {
                new ObjectSettings{
                    Page = settings.Page,
                    UseExternalLinks = settings.UseExternalLinks,
                    UseLocalLinks = settings.UseLocalLinks,
                    ProduceForms = settings.ProduceForms,
                    IncludeInOutline = settings.IncludeInOutline,
                    PagesCount = settings.PagesCount,
                    HtmlContent = html,
                    Encoding = settings.Encoding,
                    WebSettings =
                    {
                        Background = settings.Background,
                        LoadImages = settings.LoadImages,
                        EnableJavascript = settings.EnableJavascript,
                        EnableIntelligentShrinking = settings.EnableIntelligentShrinking,
                        MinimumFontSize = settings.MinimumFontSize,
                        PrintMediaType = settings.PrintMediaType,
                        DefaultEncoding = settings.DefaultEncoding,
                        UserStyleSheet = settings.UserStyleSheet,
                        enablePlugins = settings.EnablePlugins,
                    },
                    HeaderSettings = {
                        FontSize = settings.HeaderFontSize,
                        FontName = settings.HeaderFontName,
                        Left = settings.HeaderLeft,
                        Center = settings.HeaderCenter,
                        Right = settings.HeaderRight,
                        Line = settings.HeaderLine,
                        Spacing = settings.HeaderSpacing,
                        HtmlUrl = settings.HeaderHtmlUrl
                    },
                    FooterSettings = {
                        FontSize = settings.FooterFontSize,
                        FontName = settings.FooterFontName,
                        Left = settings.FooterLeft,
                        Center = settings.FooterCenter,
                        Right = settings.FooterRight,
                        Line = settings.FooterLine,
                        Spacing = settings.FooterSpacing,
                        HtmlUrl = settings.FooterHtmlUrl
                    },
                    LoadSettings = {
                        Username = settings.Username,
                        Password = settings.Password,
                        JSDelay = settings.JSDelay,
                        ZoomFactor = settings.ZoomFactor,
                        BlockLocalFileAccess = settings.BlockLocalFileAccess,
                        StopSlowScript = settings.StopSlowScript,
                        DebugJavascript = settings.DebugJavascript,
                        LoadErrorHandling = settings.LoadErrorHandling,
                        Proxy = settings.Proxy,
                        CustomHeaders = settings.CustomHeaders,
                        RepeatCustomHeaders = settings.RepeatCustomHeaders,
                        Cookies = settings.Cookies,
                        Post = settings.Post,
                    }
                }
            }
        };
    }
}

