using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Webwonders.Services
{

    public class WWExtensionsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IWWSearch, WWSearch>();
            builder.Services.AddSingleton<IWWFilter, WWFilter>();
            builder.Services.AddSingleton<IWWCacheHandling, WWCacheHandling>();
            builder.Services.AddSingleton<IWWSpreadsheetHandler, WWSpreadsheetHandler>();

        }
    }

}
