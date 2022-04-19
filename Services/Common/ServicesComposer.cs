using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Webwonders.Services;

namespace Wewonders.Services
{

    /*
     * 
     * To keep order of services correct: all IComposers in one file
     * 
     */
    public class ServicesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Services
            builder.Services.AddScoped<IWWSearchService, WWSearchService>();
            builder.Services.AddScoped<IWWFilterService, WWFilterService>();
            builder.Services.AddSingleton<IWWCacheHandlingService, WWCacheHandlingService>();
            builder.Services.AddScoped<IWWSpreadsheetHandlerService, WWSpreadsheetHandlerService>();
        }
    }
}
