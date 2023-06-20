using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Webwonders.Extensions;

public interface IWWApiCallService
{
    /// <summary>
    /// Performs Apicall to url and returns statuscode and response cast to T
    /// </summary>
    /// <typeparam name="T">returntype from API call</typeparam>
    /// <param name="url">address of API</param>
    /// <returns>Tuple of StatusCode and Result (containing response cast to T)</returns>
    (HttpStatusCode StatusCode, T Result) GetFromApi<T>(string url, HttpClientHandler httpClientHandler = null);


    /// <summary>
    /// Performs Apicall to url and returns statuscode and response cast to T
    /// Username and password are passed as Basic authorization
    /// </summary>
    /// <typeparam name="T">returntype from API call</typeparam>
    /// <param name="url">address of API</param>
    /// <param name="username">username passed to API (Basic auth)</param>
    /// <param name="password">password passed to API (Basic auth)</param>
    /// <returns>Tuple of StatusCode and Result (containing response cast to T)</returns>
    (HttpStatusCode StatusCode, T Result) GetFromApi<T>(string url, string username, string password, HttpClientHandler httpClientHandler = null);


    /// <summary>
    /// Performs Apicall to url and returns statuscode and response cast to T
    /// </summary>
    /// <typeparam name="T">returntype from API call</typeparam>
    /// <param name="url">address of API</param>
    /// <param name="additionalHeaders">additional headers to be passed, for instance: {"apiKey", "apiKey-Value"}</param>
    /// <returns></returns>
    (HttpStatusCode StatusCode, T Result) GetFromApi<T>(string url, Dictionary<string, string> additionalHeaders, HttpClientHandler httpClientHandler = null);
}


public class WWApiCallService : IWWApiCallService
{

    private readonly IHttpClientFactory _httpClientFactory;

    public WWApiCallService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc/>
    public (HttpStatusCode StatusCode, T Result) GetFromApi<T>(string url, HttpClientHandler clientHandler = null)
    {
        return Task.Run(async () => await GetApiJson<T>(url, null, null, null, clientHandler)).Result;
    }


    /// <inheritdoc/>
    public (HttpStatusCode StatusCode, T Result) GetFromApi<T>(string url, string username, string password, HttpClientHandler clientHandler = null)
    {
        return Task.Run(async () => await GetApiJson<T>(url, username, password, null, clientHandler)).Result;
    }


    /// <inheritdoc/>
    public (HttpStatusCode StatusCode, T Result) GetFromApi<T>(string url, Dictionary<string, string> additionalHeaders, HttpClientHandler clientHandler = null)
    {
        return Task.Run(async () => await GetApiJson<T>(url, null, null, additionalHeaders, clientHandler)).Result;
    }


    private async Task<(HttpStatusCode StatusCode, T Result)> GetApiJson<T>(string url, string username, string password, Dictionary<string, string> additionalHeaders,
                        HttpClientHandler httpClientHandler = null)
    {
        T result = default;


        HttpClient httpClient;
        if (httpClientHandler != null)
        {
            // TODO this should be injected, but is there any way to do this since the handler is only known here?
            // Perhaps with a named client that is configured in the startup?
            httpClient = new HttpClient(httpClientHandler);
        }
        else
        {
            httpClient = _httpClientFactory.CreateClient();
        }


        HttpRequestMessage httpRequestMessage = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
            Headers = { { HttpRequestHeader.Accept.ToString(), "application/json" } }
        };

        if (additionalHeaders != null)
        {
            foreach (var header in additionalHeaders)
            {
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }
        }

        if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
        {
            string encodedAuthorization = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            httpRequestMessage.Headers.Add("Authorization", "Basic " + encodedAuthorization);
        }

        var response = await httpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            if (response.Content.Headers.ContentLength.GetValueOrDefault() > 0)
            {
                var apiResult = response.Content.ReadAsStringAsync().Result;
                result = JsonConvert.DeserializeObject<T>(apiResult);
            }
        }

        if (httpClientHandler != null)
        {
            // cannot dispose of handler, as it is passed in
            // but need to dispose of client, since in this case it is created by ourself
            httpClient.Dispose();
        }
        return (response.StatusCode, result);
    }

}
