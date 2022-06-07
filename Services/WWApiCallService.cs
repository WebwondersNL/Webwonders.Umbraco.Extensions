using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Webwonders.Extensions.Helpers;

namespace Webwonders.Extensions.Services
{
    public interface IWWApiCallService
    {
        (HttpStatusCode StatusCode, T Result) GetFromApi<T>(string url);
        (HttpStatusCode StatusCode, T Result) GetFromApi<T>(string url, string username, string password);
    }


    public class WWApiCallService : IWWApiCallService
    {
        /// <summary>
        /// Performs Apicall to url and returns statuscode and response cast to T
        /// </summary>
        /// <typeparam name="T">returntype from API call</typeparam>
        /// <param name="url">address of API</param>
        /// <returns>Tuple of StatusCode and Result (containing response cast to T)</returns>
        public (HttpStatusCode StatusCode, T Result) GetFromApi<T>(string url)
        {
            return Task.Run(async () => await GetApiJson<T>(url, HttpClientProvider.HttpClient, null, null)).Result;
        }


        /// <summary>
        /// Performs Apicall to url and returns statuscode and response cast to T
        /// Username and password are passed as Basic authorization
        /// </summary>
        /// <typeparam name="T">returntype from API call</typeparam>
        /// <param name="url">address of API</param>
        /// <param name="username">username passed to API (Basic auth)</param>
        /// <param name="password">password passed to API (Basic auth)</param>
        /// <returns>Tuple of StatusCode and Result (containing response cast to T)</returns>
        public (HttpStatusCode StatusCode, T Result) GetFromApi<T>(string url, string username, string password)
        {
            return Task.Run(async () => await GetApiJson<T>(url, HttpClientProvider.HttpClient, username, password)).Result;
        }


        private static async Task<(HttpStatusCode StatusCode, T Result)> GetApiJson<T>(string url, HttpClient client, string username, string password)
        {
            T result = default;
            HttpRequestMessage httpRequestMessage = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Headers = { { HttpRequestHeader.Accept.ToString(), "application/json" } }
            };

            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
            {
                string encodedAuthorization = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                httpRequestMessage.Headers.Add("Authorization", "Basic " + encodedAuthorization);
            }

            var response = await client.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentLength.GetValueOrDefault() > 0)
                {
                    var apiResult = response.Content.ReadAsStringAsync().Result;
                    result = JsonConvert.DeserializeObject<T>(apiResult);
                }
            }
            return (response.StatusCode, result);
        }

    }
}
