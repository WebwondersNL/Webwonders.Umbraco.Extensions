using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Webwonders.Extensions;

namespace Webwonders.Umbraco.Extensions;


public interface IWWRequestService
{
    Task<(Dictionary<string, StringValues> keyValues, IFormFile file)> getMultiPartRequestAsync(HttpRequest request, HttpContext httpContext, ModelStateDictionary modelState,
                                                                                       string[] permittedExtensions = null, long fileSizeLimit = 10485760 /*10 MB*/);
}

public class WWRequestService : IWWRequestService
{

    private static readonly string[] _permittedExtensions = { ".xls", ".xlsx" };

    public async Task<(Dictionary<string, StringValues> keyValues, IFormFile file)> getMultiPartRequestAsync(HttpRequest request,
                                                                                              HttpContext httpContext,
                                                                                              ModelStateDictionary modelState,
                                                                                              string[] permittedExtensions = null,
                                                                                              long fileSizeLimit = 10485760 /*10 MB*/)
    {

        // TODO generalize: see https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-7.0&viewFallbackFrom=aspnetcore-2.0 (part of streaming to a database)

        if (!IsMultipartContentType(request.ContentType))
        {
            throw new Exception("No MultiPartContentType");
        }

        IFormFile? contentFile = null;

        // Accumulate the form data key-value pairs in the request (formAccumulator).
        var formAccumulator = new KeyValueAccumulator();

        permittedExtensions ??= _permittedExtensions; // if not set, use default

        var defaultFormOptions = new FormOptions();
        var boundary = GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), defaultFormOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, httpContext.Request.Body);

        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {

            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

            if (hasContentDispositionHeader)
            {

                if (HasFileContentDisposition(contentDisposition))
                {
                    // process file

                    //contentFile = new FormFile(section.Body, 0, section.Body.Length, contentDisposition.Name.Value, contentDisposition.FileName.Value);
                    var streamedFileContent = await FileHelpers.ProcessStreamedFile(section, contentDisposition, modelState, permittedExtensions, fileSizeLimit);
                    var memoryStream = new MemoryStream(streamedFileContent);

                    // need to pass a headers property, otherwise the file will have no accessible contenttype
                    contentFile = new FormFile(memoryStream, 0, streamedFileContent.Length, contentDisposition.Name.Value, contentDisposition.FileName.Value)
                    {
                        Headers = new HeaderDictionary()
                    };

                }
                else if (HasFormDataContentDisposition(contentDisposition))
                {
                    // process form data

                    // Don't limit the key name length because the 
                    // multipart headers length limit is already in effect.
                    var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                    var encoding = GetEncoding(section);

                    using (var streamReader = new StreamReader(section.Body, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                    {
                        // The value length limit is enforced by MultipartBodyLengthLimit
                        var value = await streamReader.ReadToEndAsync();

                        if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                        {
                            value = string.Empty;
                        }

                        formAccumulator.Append(key, value);

                        if (formAccumulator.ValueCount > defaultFormOptions.ValueCountLimit)
                        {
                            // Form key count limit of _defaultFormOptions.ValueCountLimit  is exceeded.
                            throw new InvalidDataException($"Form key count limit {defaultFormOptions.ValueCountLimit} exceeded.");
                        }
                    }

                }
            }

            // Drain any remaining section body that hasn't been consumed and
            // read the headers for the next section.
            section = await reader.ReadNextSectionAsync();
        }

        return (formAccumulator.GetResults(), contentFile);

    }


    private static bool IsMultipartContentType(string contentType)
    {
        return !string.IsNullOrEmpty(contentType) && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);
    }



    // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
    // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
    private static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
        }

        return boundary;
    }



    private static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        return contentDisposition != null
            && contentDisposition.DispositionType.Equals("form-data")
            && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }


    private static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        // Content-Disposition: form-data; name="key";
        return contentDisposition != null
            && contentDisposition.DispositionType.Equals("form-data")
            && string.IsNullOrEmpty(contentDisposition.FileName.Value)
            && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
    }



    private static Encoding GetEncoding(MultipartSection section)
    {

        var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out MediaTypeHeaderValue mediaType);
        
        // UTF-7 is insecure and should not be honored. UTF-8 will succeed in most cases.
        if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
        {
            return Encoding.UTF8;
        }
        return mediaType.Encoding;
    }



}
