using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;
using Webwonders.Extensions.Models;
using static Umbraco.Cms.Core.Constants;

namespace Webwonders.Extensions.Services
{
    public interface IWWSearch
    {
        List<T> Search<T>(WWSearchParameters searchParameters) where T : IPublishedContent;
    }



    public class WWSearch : IWWSearch
    {

        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IExamineManager _examineManager;
        private readonly IUmbracoMapper _mapper;
        //private readonly ILogger<WWSearch> _logger;

        private const string HideInSiteSearch = "hideInSiteSearch";
        private const string TrueString = "1";


        public WWSearch(IUmbracoContextFactory umbracoContextFactory, IExamineManager examineManager, IUmbracoMapper mapper, ILogger<WWSearch> logger)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _examineManager = examineManager;
            _mapper = mapper;
            //_logger = logger;
        }


        /// <summary>
        /// Common search procedure: external index, all content except hideInSiteSearch
        /// Note: when T is NOT IPublishedContent a mapper from ISearchResult to T needs to be defined
        /// </summary>
        /// <typeparam name="T">Type of result - when not IPublishedContent a mapper from ISearchResult is necessary</typeparam>
        /// <param name="searchParameters"></param>
        /// <returns>A list of T as result of the search, possibly sorted in the order of SearchParameters.SearchPriority</returns>
        public List<T> Search<T>(WWSearchParameters searchParameters) where T : IPublishedContent
        {

            List<T> result = new List<T>();

            //try
            //{
            if (searchParameters == null)
            {
                throw new ArgumentNullException(nameof(searchParameters));
            }
            if (!_examineManager.TryGetIndex(UmbracoIndexes.ExternalIndexName, out IIndex index))
            {
                throw new InvalidOperationException($"No index found by name {UmbracoIndexes.ExternalIndexName}");
            }

            using var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

            ISearcher searcher = index.Searcher;
            ISearchResults allSearchResults = searcher.CreateQuery(IndexTypes.Content)
                                                .ManagedQuery(searchParameters.SearchString) // all fields
                                                .Not().Field(HideInSiteSearch, TrueString) // Keep last: only content that is not hidden from sitesearch
                                                .Execute();
            // TODO test cultural behaviour
            // add culture? query.GroupedOr(String.Format(SearchFields, currentCulture).Split(','), searchTerm);

            // TODO add pageIndex and pageSize to query:
            //This should be the correct way to do this but there is still a bug with the umbraco core code.
            //For some reason it only brings back results for the first page and nothing above
            //QueryOptions queryOptions = new QueryOptions(pageIndex * pageSize, blogSearch.ItemsPerPage);
            //ISearchResults searchResult = examineQuery.Execute(queryOptions);
            //IEnumerable<ISearchResult> pagedResults = searchResult;


            if (searchParameters.SearchPriority == null || !searchParameters.SearchPriority.Any())
            {
                // defaultorder
                foreach (ISearchResult searchResult in allSearchResults)
                {
                    if (typeof(T) == typeof(IPublishedContent))
                    {
                        if (umbracoContextReference.UmbracoContext.Content.GetById(int.Parse(searchResult.Id)) is IPublishedContent resultpage)
                        {
                            result.Add((T)resultpage);
                        }
                    }
                    else
                    {
                        result.Add(_mapper.Map<ISearchResult, T>(searchResult, context => { context.SetCulture(searchParameters.Culture); }));
                    }
                }
            }
            else
            {
                // sort result in the order wanted
                foreach (int id in searchParameters.SearchPriority)
                {
                    foreach (ISearchResult searchResult in allSearchResults)
                    {
                        string path = searchResult.AllValues.FirstOrDefault(x => x.Key.ToLower() == "path").Value[0];
                        if (path.Split(',').Contains(searchResult.Id) &&
                            result.Where(x => x.Id.ToString() == searchResult.Id)?.Any() == false)

                        {
                            if (typeof(T) == typeof(IPublishedContent))
                            {
                                if (umbracoContextReference.UmbracoContext.Content.GetById(int.Parse(searchResult.Id)) is IPublishedContent resultpage)
                                {
                                    result.Add((T)resultpage);
                                }
                            }
                            else
                            {
                                result.Add(_mapper.Map<ISearchResult, T>(searchResult, context => { context.SetCulture(searchParameters.Culture); }));
                            }
                        }
                    }
                }
                // Add all searchResults that are not encountered in the sorting
                foreach (ISearchResult searchResult in allSearchResults)
                {
                    if (result.Where(x => x.Id.ToString() == searchResult.Id) == null)
                    {
                        if (typeof(T) == typeof(IPublishedContent))
                        {
                            if (umbracoContextReference.UmbracoContext.Content.GetById(int.Parse(searchResult.Id)) is IPublishedContent resultpage)
                            {
                                result.Add((T)resultpage);
                            }
                        }
                        else
                        {
                            result.Add(_mapper.Map<ISearchResult, T>(searchResult, context => { context.SetCulture(searchParameters.Culture); }));
                        }
                    }
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "WWSearch GetResults: Exception {0} | Message {1}", ex.InnerException?.ToString(), ex.Message?.ToString());
            //    return result;
            //}

            // Todo number of maxresults can be moved to query above
            result = (searchParameters.MaxResults < 0) ? result : result.Take(searchParameters.MaxResults).ToList();

            return result;
        }

    }
}
