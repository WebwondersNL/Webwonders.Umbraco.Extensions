using Umbraco.Core;
using Umbraco.Core.Composing;
using Webwonders.Services;

namespace Wewonders.Services
{

    /*
     * 
     * To keep order of services correct: all IUsercomposers in one file
     * 
     */
    public class ServicesComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            // Services
            composition.Register<IWWSpreadsheetHandlerService, WWSpreadsheetHandlerService>(); // Spreadsheethandler
            composition.Register<IWWCacheHandlingService, WWCacheHandlingService>();
        }
    }
}
