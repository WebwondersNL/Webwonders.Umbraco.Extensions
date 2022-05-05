using System;
using System.IO;
using System.Runtime.Caching;
using Newtonsoft.Json;

namespace Webwonders.Services
{
    public interface IWWCacheHandling
    {
        T ReadCache<T>(string key) where T : class;
        T ReadCache<T>(string key, string datetimeKey, string jsonFile, Func<T, bool> checkIfValidCache) where T : class;
        void WriteCache<T>(string key, T value, CacheItemPolicy cacheItemPolicy);
        void WriteCache<T>(string key, T value);
        //T DeepClone<T>(T source);

    }

    public class WWCacheHandling : IWWCacheHandling
    {
        private ObjectCache CurrentCache => MemoryCache.Default;

        /// <summary>
        /// Reads cache and returns deepclone of cached data
        /// </summary>
        /// <typeparam name="T">typeof cached variable</typeparam>
        /// <param name="key">key in cache</param>
        /// <returns>deepclone of cached variable</returns>
        public T ReadCache<T>(string key) where T : class
        {
            object cachedVar = CurrentCache[key];
            T typedVar = ConvertTo<T>(cachedVar);
            if (typedVar != null && typedVar != default)
            {
                return DeepClone(typedVar);
            }
            return null;
        }



        /// <summary>
        /// Reads cache and returns deepclone of cached data
        /// When cache not found: reads jsonfile and caches data from file
        /// </summary>
        /// <typeparam name="T">typeof cached variable</typeparam>
        /// <param name="key">key in cache</param>
        /// <param name="datetimeKey">key in cache for datetimestamp</param>
        /// <param name="jsonFile">name of jsonfile to read</param>
        /// <param name="checkIfValidCache">optional function returning bool for extra check if jason is valid, eg. var.items.Any()</param>
        /// <returns></returns>
        public T ReadCache<T>(string key, string datetimeKey, string jsonFile, Func<T, bool> checkIfValidCache) where T : class
        {

            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(datetimeKey) || String.IsNullOrWhiteSpace(jsonFile))
            {
                return null;
            }

            // Try read from cache
            // if possible and valid: return from cache
            if (ReadCache<T>(key) is T typedVar
                && typedVar != null
                && (checkIfValidCache == null || checkIfValidCache(typedVar)))
            {
                return typedVar;
            }

            // if read from cache fails: try to read jsonfile
            if (File.Exists(jsonFile) && File.ReadAllText(jsonFile) is string jsonString)
            {
                typedVar = JsonConvert.DeserializeObject<T>(jsonString);
                if (typedVar != null && (checkIfValidCache == null || checkIfValidCache(typedVar)))
                {
                    WriteCache<DateTime>(datetimeKey, File.GetCreationTime(jsonFile));
                    WriteCache<T>(key, typedVar);
                    return DeepClone(typedVar);
                }
            }
            return null;

        }



        /// <summary>
        /// Write to cache with given cacheItemPolicy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheItemPolicy"></param>
        public void WriteCache<T>(string key, T value, CacheItemPolicy cacheItemPolicy)
        {
            if (!String.IsNullOrWhiteSpace(key) && value != null)
            {
                if (cacheItemPolicy != null)
                {
                    CurrentCache.Set(key, value, cacheItemPolicy);
                }
                else
                {
                    CurrentCache[key] = value;
                }
            }
        }


        /// <summary>
        /// Write to cache
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">variable to be cached</param>
        public void WriteCache<T>(string key, T value)
        {
            if (!String.IsNullOrWhiteSpace(key) && value != null)
            {
                WriteCache<T>(key, value, null);
            }
        }



        /// <summary>
        /// Makes a deep clone of a (complex) variable
        /// </summary>
        /// <typeparam name="T">typeof variable</typeparam>
        /// <param name="source">variable to be deepcloned</param>
        /// <returns>deepclone of variable</returns>
        public T DeepClone<T>(T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (source == null)
            {
                return default;
            }

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            var serializeSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, serializeSettings), deserializeSettings);
        }



        /// <summary>
        /// Tries to convert object to Type T
        /// </summary>
        /// <typeparam name="T">type to convert to</typeparam>
        /// <param name="objectVar">object to be converted</param>
        /// <returns>object as T if possible, otherwise default of T</returns>
        private T ConvertTo<T>(object objectVar)
        {
            if (objectVar is T t)
            {
                return t;
            }
            try
            {
                return (T)Convert.ChangeType(objectVar, typeof(T));
            }
            catch (InvalidCastException)
            {
                return default;
            }
        }

    }
}

