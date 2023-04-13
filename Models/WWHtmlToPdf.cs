//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using WkHtmlToPdfDotNet;
//using WkHtmlToPdfDotNet.Contracts;

//namespace Webwonders.Umbraco.Extensions;


//public enum ColorMode
//{
//    Color = WkHtmlToPdfDotNet.ColorMode.Color,
//    Grayscale = WkHtmlToPdfDotNet.ColorMode.Grayscale
//}


//public class WWHtmlToPdfSettings
//{

//    /// <summary>
//    /// The orientation of the output document, must be either "Landscape" or "Portrait". Default = "portrait"
//    /// </summary>
//    public Orientation? Orientation { get; set; } = WkHtmlToPdfDotNet.Orientation.Portrait;

//    /// <summary>
//    /// Should the output be printed in color or gray scale, must be either "Color" or "Grayscale". Default = "color"
//    /// </summary>
//    public ColorMode? ColorMode { get; set; } = ColorMode.Color;

//    /// <summary>
//    /// Should we use loss less compression when creating the pdf file. Default = true
//    /// </summary>
//    public bool? UseCompression { get; set; } = true;

//    /// <summary>
//    ///   What dpi should we use when printing. Default = 96
//    /// </summary>
//    public int? DPI { get; set; } = 96;

//    /// <summary>
//    ///   A number that is added to all page numbers when printing headers, footers and table of content. Default = 0
//    /// </summary>
//    public int? PageOffset { get; set; } = 0;

//    /// <summary>
//    ///    How many copies should we print. Default = 1
//    /// </summary>
//    public int? Copies { get; set; } = 1;

//    /// <summary>
//    ///    Should the copies be collated. Default = true
//    /// </summary>
//    public bool? Collate { get; set; } = true;

//    /// <summary>
//    /// Should a outline (table of content in the sidebar) be generated and put into the PDF. Default = true
//    /// </summary>
//    public bool? Outline { get; set; } = true;

//    /// <summary>
//    ///   The maximal depth of the outline. Default = 4
//    /// </summary>
//    public int? OutlineDepth { get; set; } = 4;

//    /// <summary>
//    ///    If not set to the empty string a XML representation of the outline is dumped to this file. Default = ""
//    /// </summary>
//    public string DumpOutline { get; set; } = string.Empty;

//    /// <summary>
//    ///   The path of the output file, if "-" output is sent to stdout, if empty the output is stored in a buffer. Default = ""
//    /// </summary>
//    public string Out { get; set; } = string.Empty;

//    /// <summary>
//    /// The title of the PDF document. Default = ""
//    /// </summary>
//    public string DocumentTitle { get; set; } = string.Empty;

//    /// <summary>
//    ///   The maximal DPI to use for images in the pdf document. Default = 600
//    /// </summary>
//    public int? ImageDPI { get; set; } = 600;

//    /// <summary>
//    ///      The jpeg compression factor to use when producing the pdf document. Default =  94
//    /// </summary>
//    public int? ImageQuality { get; set; } = 94;

//    /// <summary>
//    /// Path of file used to load and store cookies. Default = ""
//    /// </summary>
//    public string CookieJar { get; set; } = string.Empty;

//    /// <summary>
//    /// Size of output paper. Default = A4
//    /// </summary>
//    public PechkinPaperSize PaperSize { get; set; } = PaperKind.A4;


//    /// <summary>
//    /// MarginSettings to use
//    /// </summary>
//    public MarginSettings Margins { get; set; } = new MarginSettings();


//    /// <summary>
//    ///  The URL or path of the web page to convert, if "-" input is read from stdin. Default = ""
//    /// </summary>
//    public string Page { get; set; } = string.Empty;

//    /// <summary>
//    ///      Should external links in the HTML document be converted into external pdf links. Default = true
//    /// </summary>
//    public bool? UseExternalLinks { get; set; } = true;

//    /// <summary>
//    ///    Should internal links in the HTML document be converted into pdf references. Default = true
//    /// </summary>
//    public bool? UseLocalLinks { get; set; } = true;

//    /// <summary>
//    /// Should we turn HTML forms into PDF forms. Default = false
//    /// </summary>
//    public bool? ProduceForms { get; set; } = false;

//    /// <summary>
//    ///     Should the sections from this document be included in the outline and table of content. Default = false
//    /// </summary>
//    public bool? IncludeInOutline { get; set; } = false;

//    /// <summary>
//    /// Should we count the pages of this document, in the counter used for TOC, headers and footers. Default = false
//    /// </summary>
//    public bool? PagesCount { get; set; } = false;

//    /// <summary>
//    /// Html content conversed to Pdf
//    /// </summary>
//    public string HtmlContent { get; set; }

//    /// <summary>
//    /// Encoding used
//    /// </summary>
//    public Encoding Encoding { get; set; }

//    /// <summary>
//    /// Should we print the background. Default = true
//    /// </summary>
//    public bool? Background { get; set; } = true;

//    /// <summary>
//    ///    Should we load images. Default = true
//    /// </summary>
//    public bool? LoadImages { get; set; } = true;

//    /// <summary>
//    /// Should we enable javascript. Default = true
//    /// </summary>
//    public bool? EnableJavascript { get; set; } = true;

//    /// <summary>
//    ///    Should we enable intelligent shrinkng to fit more content on one page. Has no effect for wkhtmltoimage. Default = true
//    /// </summary>
//    public bool? EnableIntelligentShrinking { get; set; } = true;

//    /// <summary>
//    /// The minimum font size allowed. Default = -1
//    /// </summary>
//    public int? MinimumFontSize { get; set; } = -1;

//    /// <summary>
//    ///  Should the content be printed using the print media type instead of the screen  media type. Default = false
//    /// </summary>
//    public bool? PrintMediaType { get; set; } = false;

//    /// <summary>
//    ///     What encoding should we guess content is using if they do not specify it properly.  Default = ""
//    /// </summary>
//    public string DefaultEncoding { get; set; } = string.Empty;

//    /// <summary>
//    /// Url or path to a user specified style sheet. Default = ""
//    /// </summary>
//    public string UserStyleSheet { get; set; } = "";

//    /// <summary>
//    ///   Should we enable NS plugins. Enabling this will have limited success. Default = false
//    /// </summary>
//    public bool? EnablePlugins { get; set; } = false;


//    /// <summary>
//    /// The font size to use for the header. Default = 12
//    /// </summary>
//    public int? HeaderFontSize { get; set; } = 12;

//    /// <summary>
//    /// The name of the font to use for the header. Default = "Ariel"
//    /// </summary>
//    public string HeaderFontName { get; set; } = "Ariel";

//    /// <summary>
//    ///      The string to print in the left part of the header, note that some sequences are replaced in this string, 
//    ///      see the wkhtmltopdf manual. Default = ""
//    /// </summary>
//    public string HeaderLeft { get; set; } = string.Empty;

//    /// <summary>
//    ///  The text to print in the right part of the header, note that some sequences are replaced in this string, 
//    ///  see the wkhtmltopdf manual. Default = ""
//    /// </summary>
//    public string HeaderCenter { get; set; } = string.Empty;

//    /// <summary>
//    ///   The text to print in the right part of the header, note that some sequences are replaced in this string, 
//    ///   see the wkhtmltopdf manual. Default = ""
//    /// </summary>
//    public string HeaderRight { get; set; } = string.Empty;


//    /// <summary>
//    /// Whether a line should be printed under the header. Default = false
//    /// </summary>
//    public bool? HeaderLine { get; set; } = false;

//    /// <summary>
//    ///    The amount of space to put between the header and the content, e.g. "1.8". 
//    ///    Be aware that if this is too large the header will be printed outside the pdf document.
//    //     This can be corrected with the margin.top setting. Default = 0.00
//    /// </summary>
//    public double? HeaderSpacing { get; set; } = 0.00;

//    /// <summary>
//    /// Url for a HTML document to use for the header. Default = "pdfHeader.cshtml" under Views\Pdf\pdfType
//    /// </summary>
//    public string HeaderHtmlUrl { get; set; } = string.Empty;


//    /// <summary>
//    ///  The font size to use for the footer. Default = 12
//    /// </summary>
//    public int? FooterFontSize { get; set; } = 12;

//    /// <summary>
//    /// The name of the font to use for the footer. Default = "Ariel"
//    /// </summary>
//    public string FooterFontName { get; set; } = "Ariel";

//    /// <summary>
//    ///   The string to print in the left part of the footer, note that some sequences are replaced in this string, 
//    ///   see the wkhtmltopdf manual. Default = ""
//    /// </summary>
//    public string FooterLeft { get; set; } = string.Empty;

//    /// <summary>
//    /// The text to print in the right part of the footer, note that some sequences are replaced in this string, 
//    /// see the wkhtmltopdf manual. Default = ""
//    /// </summary>
//    public string FooterCenter { get; set; } = string.Empty;

//    /// <summary>
//    ///    The text to print in the right part of the footer, note that some sequences are replaced in this string, 
//    ///    see the wkhtmltopdf manual. Default = ""
//    /// </summary>
//    public string FooterRight { get; set; } = string.Empty;

//    /// <summary>
//    /// Whether a line should be printed above the footer. Default = false
//    /// </summary>
//    public bool? FooterLine { get; set; } = false;

//    /// <summary>
//    ///  The amount of space to put between the footer and the content, e.g. "1.8". 
//    ///  Be aware that if this is too large the footer will be printed outside the pdf document. 
//    ///  This can be corrected with the margin.bottom setting. Default = 0.00
//    /// </summary>
//    public double? FooterSpacing { get; set; } = 0.00;

//    /// <summary>
//    /// Url for a HTML document to use for the footer. Default = "pdfFooter.cshtml" under Views\Pdf\pdfType
//    /// </summary>
//    public string FooterHtmlUrl { get; set; } = string.Empty;


//    /// <summary>
//    /// The user name to use when loging into a website. Default = ""
//    /// </summary>
//    public string Username { get; set; } = string.Empty;

//    /// <summary>
//    ///  The password to used when logging into a website. Default = ""
//    /// </summary>
//    public string Password { get; set; } = string.Empty;

//    /// <summary>
//    /// The amount of time in milliseconds to wait after a page has done loading until it is actually printed. E.g. "1200". 
//    /// We will wait this amount of time or until, javascript calls window.print(). Default = 200
//    /// </summary>
//    public int? JSDelay { get; set; } = 200;

//    /// <summary>
//    /// How much should we zoom in on the content. Default = 1.0
//    /// </summary>
//    public double? ZoomFactor { get; set; } = 1.0;

//    /// <summary>
//    /// Disallow local and piped files to access other local files. Default = true
//    /// </summary>
//    public bool? BlockLocalFileAccess { get; set; } = true;

//    /// <summary>
//    /// Stop slow running javascript. Default = true
//    /// </summary>
//    public bool? StopSlowScript { get; set; } = true;

//    /// <summary>
//    ///  Forward javascript warnings and errors to the warning callback. Default = false
//    /// </summary>
//    public bool? DebugJavascript { get; set; } = false;

//    /// <summary>
//    /// How should we handle obejcts that fail to load. Default = Abort
//    /// </summary>
//    public ContentErrorHandling? LoadErrorHandling { get; set; } = ContentErrorHandling.Abort;

//    /// <summary>
//    /// String describing what proxy to use when loading the object. Default = ""
//    /// </summary>
//    public string Proxy { get; set; } = string.Empty;

//    /// <summary>
//    ///  Custom headers used when requesting page. Defaulty = empty
//    /// </summary>
//    public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>();

//    /// <summary>
//    ///  Should the custom headers be sent all elements loaded instead of only the main page. Default = false
//    /// </summary>
//    public bool? RepeatCustomHeaders { get; set; } = false;

//    /// <summary>
//    /// Cookies used when requesting page. Default = empty
//    /// </summary>
//    public Dictionary<string, string> Cookies { get; set; } = new Dictionary<string, string>();

//    /// <summary>
//    /// Post items
//    /// </summary>
//    public Dictionary<string, string> Post { get; set; } = new Dictionary<string, string>();



//    public byte[] GetContent()
//    {
//        if (HtmlContent == null)
//        {
//            return Array.Empty<byte>();
//        }

//        return (Encoding ?? Encoding.UTF8).GetBytes(HtmlContent);
//    }



//}
