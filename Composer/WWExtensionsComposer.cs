using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Webwonders.Extensions.Services
{

    public class WWExtensionsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddScoped<IWWSearch, WWSearch>();
            builder.Services.AddScoped<IWWFilter, WWFilter>();
            builder.Services.AddScoped<IWWCacheHandling, WWCacheHandling>();
            builder.Services.AddScoped<IWWSpreadsheetHandler, WWSpreadsheetHandler>();
            builder.Services.AddScoped<IWWApiCallService, WWApiCallService>();
        }
    }

}
