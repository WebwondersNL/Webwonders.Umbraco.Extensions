using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Webwonders.Extensions;

public interface IWWCacheService
{
    T GetCacheItem<T>(string key);
    T GetCacheItem<T>(string key, Func<T> getCacheItem, Func<T, bool> isValidCache, TimeSpan? timeout = null, bool isSliding = false, string[] dependentFiles = null) where T : class;
    T GetCacheItem<T>(string key, string jsonFileToCache, Func<T, bool> isValidCache, TimeSpan? timeout = null, bool isSliding = false, string[] dependentFiles = null) where T : class;

    void InsertCacheItem<T>(string key, Func<T> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, string[] dependentFiles = null);
    void InsertCacheItem<T>(string key, string jsonFileToCache, TimeSpan? timeout = null, bool isSliding = false, string[] dependentFiles = null) where T : class;
}

public class WWCacheService : IWWCacheService
{

    private readonly IAppPolicyCache _runtimeCache;


    public WWCacheService(AppCaches appCaches)
    {
        _runtimeCache = appCaches.RuntimeCache;
    }


    /// <summary>
    /// Reads cache and returns cached data
    /// </summary>
    /// <typeparam name="T">typeof cached variable</typeparam>
    /// <param name="key">key in cache</param>
    /// <returns></returns>
    public T GetCacheItem<T>(string key)
    {
        return _runtimeCache.GetCacheItem<T>(key);
    }



    /// <summary>
    /// Reads cache and returns data. If necessary: first reads cache using GetCacheItem
    /// </summary>
    /// <typeparam name="T">typeof cached variable</typeparam>
    /// <param name="key">key in cache</param>
    /// <param name="GetCacheItem">function to get cached item when not yet in cache</param>
    /// <param name="isValidCache">function to check if the cache is valid</param>
    /// <param name="timeout">timeout for cache</param>
    /// <param name="isSliding">is this a sliding or absolute cache</param>
    /// <param name="dependentFiles">files this cache is dependant of</param>
    /// <returns></returns>
    public T GetCacheItem<T>(string key, Func<T> getCacheItem, Func<T, bool> isValidCache, TimeSpan? timeout = null, bool isSliding = false,
                      string[] dependentFiles = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }
        T result = _runtimeCache.GetCacheItem<T>(key, () => getCacheItem(), timeout, isSliding, dependentFiles);
        if (result != null && (isValidCache == null || isValidCache(result)))
        {
            return result;
        }
        return null;
    }



    /// <summary>
    /// Reads cache and returns cached data. If necessary: first reads cache from jsonfile
    /// </summary>
    /// <typeparam name="T">typeof cached variable</typeparam>
    /// <param name="key">key in cache</param>
    /// <param name="jsonFileToCache">jsonfile that is origin of this cache</param>
    /// <param name="isValidCache">function to check if the cache is valid</param>
    /// <param name="timeout">timeout for cache</param>
    /// <param name="isSliding">is this a sliding or absolute cache</param>
    /// <param name="dependentFiles">files this cache is dependant of (will be added to jsonFileToCache)</param>
    /// <returns></returns>
    public T GetCacheItem<T>(string key, string jsonFileToCache, Func<T, bool> isValidCache, TimeSpan? timeout = null, bool isSliding = false,
                             string[] dependentFiles = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(jsonFileToCache))
        {
            return null;
        }

        List<string> allDependentFiles = new() { jsonFileToCache };
        if (dependentFiles != null && dependentFiles.ToList() is List<string> dependentFilesList && dependentFiles.Any())
        {
            allDependentFiles.AddRange(dependentFiles);
        }
        return _runtimeCache.GetCacheItem<T>(key, () => ReadJsonFile<T>(jsonFileToCache, isValidCache), timeout, isSliding, allDependentFiles.ToArray());
    }


    /// <summary>
    /// Sets cache item without retrieving it
    /// </summary>
    /// <typeparam name="T">typeof cached variable</typeparam>
    /// <param name="key">key in cache</param>
    /// <param name="getCacheItem">function to get variable to cache</param>
    /// <param name="timeout">timeout of cache</param>
    /// <param name="isSliding">is cache sliding or absolute</param>
    /// <param name="dependentFiles">files of which cache is dependant</param>
    public void InsertCacheItem<T>(string key, Func<T> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, string[] dependentFiles = null)
    {
        if (!String.IsNullOrWhiteSpace(key) && getCacheItem != null)
        {
            _runtimeCache.InsertCacheItem<T>(key, () => getCacheItem(), timeout, isSliding, dependentFiles);
        }
    }


    /// <summary>
    /// sets cache item from Jsonfile without retrieving
    /// </summary>
    /// <typeparam name="T">typeof cached variable</typeparam>
    /// <param name="key">key in cache</param>
    /// <param name="jsonFileToCache">jsonfile to cache</param>
    /// <param name="timeout">timout of cache</param>
    /// <param name="isSliding">is cache sliding or absolute</param>
    /// <param name="dependentFiles">files this cache is dependant of (will be added to jsonFileToCache)</param>
    public void InsertCacheItem<T>(string key, string jsonFileToCache, TimeSpan? timeout = null, bool isSliding = false, string[] dependentFiles = null) where T : class
    {
        if (!String.IsNullOrWhiteSpace(key) && !String.IsNullOrWhiteSpace(jsonFileToCache))
        {

            List<string> allDependentFiles = new() { jsonFileToCache };
            if (dependentFiles != null && dependentFiles.ToList() is List<string> dependentFilesList && dependentFiles.Any())
            {
                allDependentFiles.AddRange(dependentFiles);
            }

            _runtimeCache.InsertCacheItem<T>(key, () => ReadJsonFile<T>(jsonFileToCache, null), timeout, isSliding, allDependentFiles.ToArray());
        }
    }


    private static T ReadJsonFile<T>(string jsonFile, Func<T, bool> isValidCache) where T : class
    {
        if (File.Exists(jsonFile) && File.ReadAllText(jsonFile) is string jsonString)
        {
            T result = JsonConvert.DeserializeObject<T>(jsonString);
            if (result != null && (isValidCache == null || isValidCache(result)))
            {
                return result;
            }
        }
        return null;
    }


    ///// <summary>
    ///// Makes a deep clone of a (complex) variable
    ///// </summary>
    ///// <typeparam name="T">typeof variable</typeparam>
    ///// <param name="source">variable to be deepcloned</param>
    ///// <returns>deepclone of variable</returns>
    //public T DeepClone<T>(T source)
    //{
    //    // Don't serialize a null object, simply return the default for that object
    //    if (source == null)
    //    {
    //        return default;
    //    }

    //    var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
    //    var serializeSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
    //    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, serializeSettings), deserializeSettings);
    //}



    ///// <summary>
    ///// Tries to convert object to Type T
    ///// </summary>
    ///// <typeparam name="T">type to convert to</typeparam>
    ///// <param name="objectVar">object to be converted</param>
    ///// <returns>object as T if possible, otherwise default of T</returns>
    //private T ConvertTo<T>(object objectVar)
    //{
    //    if (objectVar is T t)
    //    {
    //        return t;
    //    }
    //    try
    //    {
    //        return (T)Convert.ChangeType(objectVar, typeof(T));
    //    }
    //    catch (InvalidCastException)
    //    {
    //        return default;
    //    }
    //}

}

