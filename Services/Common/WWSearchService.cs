using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;
using Webwonders.Models;
using static Umbraco.Cms.Core.Constants;

namespace Webwonders.Services
{
    public interface IWWSearchService
    {
        List<ISearchResult> Search(WWSearchParameters searchParameters);
    }



    public class WWSearchService : IWWSearchService
    {

        private readonly IUmbracoContextFactory _context;
        private readonly IPublishedSnapshotAccessor _snapshotAccessor;
        private readonly IPublishedValueFallback _publishedValueFallback;
        private readonly IExamineManager _examineManager;
        private readonly ILogger _logger;

        private const string HideInSiteSearch = "hideInSiteSearch";
        private const string TrueString = "1";


        public WWSearchService(IUmbracoContextFactory context, IPublishedSnapshotAccessor snapshotAccessor, IPublishedValueFallback publishedValueFallback, IExamineManager examineManager, ILogger logger)
        {
            _context = context;
            _snapshotAccessor = snapshotAccessor;
            _publishedValueFallback = publishedValueFallback;
            _examineManager = examineManager;
            _logger = logger;
        }


        public List<ISearchResult> Search(WWSearchParameters searchParameters)
        {

            List<ISearchResult> result = new List<ISearchResult>();

            try
            {
                if (searchParameters == null)
                {
                    throw new ArgumentNullException(nameof(searchParameters));
                }
                if (!_examineManager.TryGetIndex(UmbracoIndexes.ExternalIndexName, out IIndex index))
                {
                    throw new InvalidOperationException($"No index found by name {UmbracoIndexes.ExternalIndexName}");
                }
                ISearcher searcher = index.Searcher;
                ISearchResults allSearchResults = searcher.CreateQuery(IndexTypes.Content)
                                                    .ManagedQuery(searchParameters.SearchString) // all fields
                                                    .Not().Field(HideInSiteSearch, TrueString) // Keep last: only content that is not hidden from sitesearch
                                                    .Execute();

                // TODO add pageIndex and pageSize to query:
                //This should be the correct way to do this but there is still a bug with the umbraco core code.
                //For some reason it only brings back results for the first page and nothing above
                //QueryOptions queryOptions = new QueryOptions(pageIndex * pageSize, blogSearch.ItemsPerPage);
                //ISearchResults searchResult = examineQuery.Execute(queryOptions);
                //IEnumerable<ISearchResult> pagedResults = searchResult;

                using UmbracoContextReference contextReference = _context.EnsureUmbracoContext();
                IUmbracoContext umbracoContext = contextReference.UmbracoContext;
                if (searchParameters.SearchPriority == null || !searchParameters.SearchPriority.Any())
                {
                    // defaultorder
                    foreach (ISearchResult searchResult in allSearchResults)
                    {
                        //result.Add(new WWSearchResult(umbracoContext.Content.GetById(int.Parse(searchResult.Id)), _snapshotAccessor, _publishedValueFallback, searchParameters.Culture));
                        result.Add(searchResult);
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
                                //result.Add(new WWSearchResult(umbracoContext.Content.GetById(int.Parse(searchResult.Id)), _snapshotAccessor, _publishedValueFallback, searchParameters.Culture));
                                result.Add(searchResult);
                            }
                        }
                    }
                    // Add all searchResults that are not encountered in the sorting
                    foreach (ISearchResult searchResult in allSearchResults)
                    {
                        if (result.Where(x => x.Id.ToString() == searchResult.Id) == null)
                        {
                            //result.Add(new WWSearchResult(umbracoContext.Content.GetById(int.Parse(searchResult.Id)), _snapshotAccessor, _publishedValueFallback, searchParameters.Culture));
                            result.Add(searchResult);
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WWSearchService GetResults: Exception {0} | Message {1}", ex.InnerException?.ToString(), ex.Message?.ToString());
                return result;
            }

            // Todo number of maxresults can be moved to query above
            result = (searchParameters.MaxResults < 0) ? result : result.Take(searchParameters.MaxResults).ToList();

            return result;
        }

    }
}
