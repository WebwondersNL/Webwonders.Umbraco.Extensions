﻿using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace Webwonders.Extensions;


public class WWExtensionsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services
            // used for HtmlToPdf
            .AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()))

            .AddScoped<IWWSearch, WWSearch>()
            .AddScoped<IWWFilter, WWFilter>()
            .AddScoped<IWWCacheService, WWCacheService>()
            .AddScoped<IWWSpreadsheetHandler, WWSpreadsheetHandler>()
            .AddScoped<IWWApiCallService, WWApiCallService>()
            .AddScoped<IWWDbService, WWDbService>()
            .AddScoped<IWWHtmlToPdfService, WWHtmlToPdfService>()
            .AddScoped<IWWLanguage, WWLanguage>();
    }
}
