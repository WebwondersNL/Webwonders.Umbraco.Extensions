using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Webwonders.Extensions;


public class WWExtensionsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddScoped<IWWSearch, WWSearch>();
        builder.Services.AddScoped<IWWFilter, WWFilter>();
        builder.Services.AddScoped<IWWCacheService, WWCacheService>();
        builder.Services.AddScoped<IWWSpreadsheetHandler, WWSpreadsheetHandler>();
        builder.Services.AddScoped<IWWApiCallService, WWApiCallService>();
        builder.Services.AddScoped<IWWDbService, WWDbService>();
        builder.Services.AddScoped<IWWHtmlToPdfService, WWHtmlToPdfService>();
        builder.Services.AddScoped<IWWLanguage, WWLanguage>();
    }
}
