using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;



namespace Webwonders.Extensions;


public static class QueryStringHelpers
{
    /// <summary>
    /// Builds a querystring based on the current querystring, and updates or adds a key/value-pair.
    /// </summary>
    /// <param name="shortStringHelper">IShortStringHelper service</param>
    /// <param name="queryString">current querystring</param>
    /// <param name="key">key to add or update</param>
    /// <param name="value">value to add or update</param>  
    /// <param name="resetPagination">if true, removes the page-parameter from the querystring</param>
    /// <returns>new querystring</returns>
    public static string? QueryStringBuilder(IShortStringHelper shortStringHelper, QueryString queryString, string key, string value, bool resetPagination = false)
    {
        var queryStringKeyValuePairs = QueryHelpers.ParseQuery(queryString.ToString());

        if (resetPagination && queryStringKeyValuePairs.ContainsKey("page"))
        {
            queryStringKeyValuePairs.Remove("page");
        }

        if (value.IsNullOrWhiteSpace())
        {
            // empty value: remove
            queryStringKeyValuePairs.Remove(key);
        }
        else
        {
            if (queryStringKeyValuePairs.ContainsKey(key))
            {
                // replace if exists
                queryStringKeyValuePairs[key] = value.ToUrlSegment(shortStringHelper);
            }
            else
            {
                // add if not exists
                queryStringKeyValuePairs.Add(key, value.ToUrlSegment(shortStringHelper));
            }
        }

        return QueryString.Create(queryStringKeyValuePairs).ToString();
    }
}