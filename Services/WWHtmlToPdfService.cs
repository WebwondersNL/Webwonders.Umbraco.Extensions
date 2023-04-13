//using System;
//using System.IO;
//using Microsoft.AspNetCore.Hosting;
//using RazorLight;
//using WkHtmlToPdfDotNet;
//using WkHtmlToPdfDotNet.Contracts;

//namespace Webwonders.Umbraco.Extensions;


//// TODO check properties: if default for all




//public interface IWWHtmlToPdfService
//{
//    (bool succes, MemoryStream? stream) GetPdfMemoryStream(string pdfType, string viewName, WWHtmlToPdfSettings settings, object viewModel);

//}


//public class WWHtmlToPdfService : IWWHtmlToPdfService
//{

//    private readonly IWebHostEnvironment _webHostEnvironment;
//    private readonly IConverter _converter;

//    public WWHtmlToPdfService(IWebHostEnvironment webHostEnvironment,
//                              IConverter converter)
//    {
//        _webHostEnvironment = webHostEnvironment;
//        _converter = converter;
//    }


//    public (bool succes, MemoryStream? stream) GetPdfMemoryStream(string pdfType, string viewName, WWHtmlToPdfSettings settings, object viewModel)
//    {
//        if (String.IsNullOrWhiteSpace(pdfType))
//        {
//            throw new ArgumentNullException(nameof(pdfType));
//        }
//        if (String.IsNullOrWhiteSpace(viewName))
//        {
//            throw new ArgumentNullException(nameof(viewName));
//        }

//        string contentRootPath = _webHostEnvironment.ContentRootPath;
//        string pdfPath = Path.Combine(contentRootPath, $"Views\\pdf\\{pdfType}\\");

//        if (String.IsNullOrWhiteSpace(settings.HeaderHtmlUrl))
//        {
//            settings.HeaderHtmlUrl = pdfPath + "pdfHeader.cshtml";
//        }
//        if (String.IsNullOrWhiteSpace(settings.FooterHtmlUrl))
//        {
//            settings.FooterHtmlUrl = pdfPath + "pdfFooter.cshtml";
//        }


//        var engine = new RazorLightEngineBuilder()
//                         .UseFileSystemProject(pdfPath)
//                         .UseMemoryCachingProvider()
//                         .Build();

//        string htmlString = engine.CompileRenderAsync(viewName, viewModel).Result;

//        if (!String.IsNullOrWhiteSpace(htmlString))
//        {
//            IDocument document = CreatePdfDocument(htmlString, settings);

//            byte[] pdf = _converter.Convert(document);

//            if (pdf != null)
//            {
//                return (true, new MemoryStream(pdf));
//            }
//        }

//        return (false, null);

//    }




//    private static IDocument CreatePdfDocument(string html, WWHtmlToPdfSettings settings)
//    {
//        return new HtmlToPdfDocument
//        {
//            GlobalSettings = {
//                Orientation = settings.Orientation,
//                ColorMode = settings.ColorMode,
//                UseCompression = settings.UseCompression,
//                DPI = settings.DPI,
//                PageOffset = settings.PageOffset,
//                Copies = settings.Copies,
//                Collate = settings.Collate,
//                Outline = settings.Outline,
//                OutlineDepth = settings.OutlineDepth,
//                DumpOutline = settings.DumpOutline,
//                Out = settings.Out,
//                DocumentTitle = settings.DocumentTitle,
//                ImageDPI = settings.ImageDPI,
//                ImageQuality = settings.ImageQuality,
//                CookieJar = settings.CookieJar,
//                PaperSize = settings.PaperSize,
//                Margins = settings.Margins,
//            },
//            Objects = {
//                new ObjectSettings{
//                    Page = settings.Page,
//                    UseExternalLinks = settings.UseExternalLinks,
//                    UseLocalLinks = settings.UseLocalLinks,
//                    ProduceForms = settings.ProduceForms,
//                    IncludeInOutline = settings.IncludeInOutline,
//                    PagesCount = settings.PagesCount,
//                    HtmlContent = html,
//                    Encoding = settings.Encoding,
//                    WebSettings =
//                    {
//                        Background = settings.Background,
//                        LoadImages = settings.LoadImages,
//                        EnableJavascript = settings.EnableJavascript,
//                        EnableIntelligentShrinking = settings.EnableIntelligentShrinking,
//                        MinimumFontSize = settings.MinimumFontSize,
//                        PrintMediaType = settings.PrintMediaType,
//                        DefaultEncoding = settings.DefaultEncoding,
//                        UserStyleSheet = settings.UserStyleSheet,
//                        enablePlugins = settings.EnablePlugins,
//                    },
//                    HeaderSettings = {
//                        FontSize = settings.HeaderFontSize,
//                        FontName = settings.HeaderFontName,
//                        Left = settings.HeaderLeft,
//                        Center = settings.HeaderCenter,
//                        Right = settings.HeaderRight,
//                        Line = settings.HeaderLine,
//                        Spacing = settings.HeaderSpacing,
//                        HtmlUrl = settings.HeaderHtmlUrl
//                    },
//                    FooterSettings = {
//                        FontSize = settings.FooterFontSize,
//                        FontName = settings.FooterFontName,
//                        Left = settings.FooterLeft,
//                        Center = settings.FooterCenter,
//                        Right = settings.FooterRight,
//                        Line = settings.FooterLine,
//                        Spacing = settings.FooterSpacing,
//                        HtmlUrl = settings.FooterHtmlUrl
//                    },
//                    LoadSettings = {
//                        Username = settings.Username,
//                        Password = settings.Password,
//                        JSDelay = settings.JSDelay,
//                        ZoomFactor = settings.ZoomFactor,
//                        BlockLocalFileAccess = settings.BlockLocalFileAccess,
//                        StopSlowScript = settings.StopSlowScript,
//                        DebugJavascript = settings.DebugJavascript,
//                        LoadErrorHandling = settings.LoadErrorHandling,
//                        Proxy = settings.Proxy,
//                        CustomHeaders = settings.CustomHeaders,
//                        RepeatCustomHeaders = settings.RepeatCustomHeaders,
//                        Cookies = settings.Cookies,
//                        Post = settings.Post,
//                    }
//                }
//            }
//        };
//    }
//}