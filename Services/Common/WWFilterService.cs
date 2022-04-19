﻿using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;



using Webwonders.Models;

namespace Webwonders.Services
{

    public interface IWWFilterService
    {
        OverviewDto<TItemDto> AddFilter<TFilters, TFilter, TItem, TItemDto>(OverviewDto<TItemDto> overviewDto,
                                                                                    List<TItem> allItems,
                                                                                    List<(string filterGroupName, string itemFilterPropertyAlias, TFilters filterPage)> filterGroups,
                                                                                    string filterNamePropertyAlias)
            where TFilters : IPublishedContent
            where TFilter : PublishedContentModel, IPublishedContent
            where TItem : IPublishedContent
            where TItemDto : BaseFilter;


        OverviewDto<TItemDto> AddDocTypeFilter<TItemDto>(OverviewDto<TItemDto> overviewDto, int id, string docTypeAlias, string name)
            where TItemDto : BaseFilter;


        OverviewDto<TItemDto> FilteredItems<TItem, TItemDto>(OverviewDto<TItemDto> overviewDto)
            where TItem : class, IPublishedContent
            where TItemDto : BaseFilter;

    }

    public class WWFilterService : IWWFilterService
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IPublishedValueFallback _publishedValueFallback;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly UmbracoMapper _mapper;

        public WWFilterService(IUmbracoContextFactory umbracoContextFactory, IPublishedValueFallback publishedValueFallback,
                               IVariationContextAccessor variationContextAccessor, UmbracoMapper mapper)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _publishedValueFallback = publishedValueFallback;
            _variationContextAccessor = variationContextAccessor;   
            _mapper = mapper;
        }



        public OverviewDto<TItemDto> AddFilter<TFilters, TFilter, TItem, TItemDto>(OverviewDto<TItemDto> overviewDto,
                                                                     List<TItem> allItems,
                                                                     List<(string filterGroupName, string itemFilterPropertyAlias, TFilters filterPage)> filterGroups,
                                                                     string filterPropertyAlias)
            where TFilters : IPublishedContent
            where TFilter : PublishedContentModel, IPublishedContent
            where TItem : IPublishedContent
            where TItemDto : BaseFilter
        {

            foreach (var (filterGroupName, itemFilterPropertyAlias, filterPage) in filterGroups.Where(x => !String.IsNullOrWhiteSpace(x.filterGroupName) && x.filterPage != null))
            {
                var allFilters = new List<FilterDto>();

                IEnumerable<TFilter> filterList = filterPage.Children.OfType<TFilter>().Where(x => x.IsVisible(_publishedValueFallback));

                foreach (var item in filterList)
                {

                    FilterDto filter = new FilterDto()
                    {
                        Id = item.Id,
                        Name = item.Value<string>(_publishedValueFallback, filterPropertyAlias, overviewDto.OverviewFilter.Culture)
                    };
                    if (String.IsNullOrWhiteSpace(filter.Name))
                    {
                        filter.Name = item.Name;
                    }

                    allFilters.Add(filter);
                }

                // Get all used Filters:
                // from allItems select the filterProperty
                // if filterproperty != null then select all the ids of the filters
                // This gives the list of all the filterIds that are used in all the items
                var allUsedFilters = allItems.Where(x => x.Value<IEnumerable<IPublishedContent>>(_publishedValueFallback, itemFilterPropertyAlias, overviewDto.OverviewFilter.Culture) != null)
                                             .SelectMany(y => y.Value<IEnumerable<IPublishedContent>>(_publishedValueFallback, itemFilterPropertyAlias, overviewDto.OverviewFilter.Culture))
                                             .Select(z => z.Id)
                                             .Distinct()
                                             .ToList();

                var usedFilters = allFilters.Where(x => allUsedFilters.Any(y => y == x.Id)).ToList();
                overviewDto.OverviewFilter.AddFilters(filterGroupName, usedFilters);

            }
            return overviewDto;
        }



        public OverviewDto<TItemDto> AddDocTypeFilter<TItemDto>(OverviewDto<TItemDto> overviewDto, int id, string docTypeAlias, string name)
            where TItemDto : BaseFilter
        {
            if (id > 0 && !String.IsNullOrWhiteSpace(docTypeAlias) && !String.IsNullOrWhiteSpace(name))
            {
                overviewDto.OverviewFilter.AddDoctypeFilter(new DoctypeFilterDto()
                {
                    Id = id,
                    DocTypeAlias = docTypeAlias,
                    Name = name
                });
            }
            return overviewDto;
        }



        public OverviewDto<TItemDto> FilteredItems<TItem, TItemDto>(OverviewDto<TItemDto> overviewDto)
            where TItem : class, IPublishedContent
            where TItemDto : BaseFilter
        {
            overviewDto.Items = new List<TItemDto>();

            if (overviewDto.OverviewFilter.ParentId <= 0)
            {
                return overviewDto;
            }

            // Get all items
            using (var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext())
            {
                if (umbracoContextReference.UmbracoContext.Content.GetById(overviewDto.OverviewFilter.ParentId) is IPublishedContent overviewPage)
                {
                    IEnumerable<IPublishedContent> items = overviewPage.Children(_variationContextAccessor, overviewDto.OverviewFilter.Culture).Where(x => x.IsVisible(_publishedValueFallback));
                    foreach (var item in items)
                    {
                        overviewDto.Items.Add(_mapper.Map<TItem, TItemDto>(item as TItem));//context => { context.SetCulture(overviewDto.OverviewFilter.Culture); }));
                    }

                }
            }
            overviewDto.Items = overviewDto.OverviewFilter.FilteredItems(overviewDto.Items);
            return overviewDto;

        }


    }
}
