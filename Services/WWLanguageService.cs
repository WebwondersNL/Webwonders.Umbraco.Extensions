using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;

namespace Webwonders.Extensions.Services;

public interface IWWLanguage {
    public string GetDictionaryItem(string key, string? culture);
}


public class WWLanguage : IWWLanguage {

    private readonly ILocalizationService _localizationService;


    public WWLanguage(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }


    /// <summary>
    /// Get dictionary item by key and language
    /// </summary>
    /// <param name="key">Key in dictionary</param>
    /// <returns>Value of dictionaryItem in culture, or key or empty string</returns>
    public string GetDictionaryItem(string key, string? culture)
    {

        string result = String.Empty;

        if (!String.IsNullOrWhiteSpace(key))
        {
            var dictionaryItem = _localizationService.GetDictionaryItemByKey(key);
            if (dictionaryItem != null && !String.IsNullOrWhiteSpace(culture))
            {
                result = dictionaryItem.Translations.FirstOrDefault(x => x.Language != null && x.Language.CultureInfo != null && x.Language.CultureInfo.Name.Equals(culture))?.Value ?? key;
            }
            if (String.IsNullOrWhiteSpace(result))
            {
                result = key;
            }

        }

        return result;
    }

}