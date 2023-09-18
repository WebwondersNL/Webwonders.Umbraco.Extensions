using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Webwonders.Umbraco.Extensions;

namespace Webwonders.Extensions;


public class WWExtensionsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services
            // used for ApiCalls
            .AddHttpClient()   

            // custom databases
            .AddSingleton<IWWDbService, WWDbService>()

            .AddScoped<IWWSearch, WWSearch>()
            .AddScoped<IWWFilter, WWFilter>()
            .AddScoped<IWWCacheService, WWCacheService>()
            .AddScoped<IWWSpreadsheetHandler, WWSpreadsheetHandler>()
            .AddScoped<IWWApiCallService, WWApiCallService>()
            .AddScoped<IWWLanguage, WWLanguage>()
            .AddScoped<IWWRequestService, WWRequestService>();
    }
}
