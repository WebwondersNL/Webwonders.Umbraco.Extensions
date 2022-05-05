using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Webwonders.Models
{

    [DataContract]
    public class BaseFilter
    {
        [DataMember(Name = "filterids")]
        public List<int> FilterIds { get; set; }

        [DataMember(Name = "docTypeId")]
        public int DocTypeId { get; set; }
    }


    [DataContract(Name = "docTypeFilter")]
    public class DoctypeFilterDto
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "docTypeAlias")]
        public string DocTypeAlias { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }
    }



    [DataContract(Name = "filter")]
    public class FilterDto
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }
    }


    [DataContract(Name = "filters")]
    public class FiltersDto
    {
        // Id of the filterspage
        //[DataMember(Name="filterId")]
        //public int Id { get; set; }

        // Name of the filterspage
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "filters")]
        public List<FilterDto> Filters { get; set; }

        public FiltersDto()
        {
            Filters = new List<FilterDto>();
        }
    }



    [DataContract(Name = "overviewFilter")]
    public class OverviewFilterDto
    {
        [DataMember(Name = "parentId")]
        public int ParentId { get; set; }

        [DataMember(Name = "culture")]
        public string Culture { get; set; }

        [DataMember(Name = "filters")]
        public List<FiltersDto> Filters { get; set; }

        [DataMember(Name = "docTypeFilters")]
        public List<DoctypeFilterDto> DocTypeFilters { get; set; }

        public OverviewFilterDto()
        {
            Filters = new List<FiltersDto>();
            DocTypeFilters = new List<DoctypeFilterDto>();
        }


        /// <summary>
        /// Get all filters of a certain filterName
        /// </summary>
        /// <param name="filterName">Name of filterGroup with filters</param>
        /// <returns>list of filters</returns>
        public List<FilterDto> GetFilters(string filterName)
        {
            if (String.IsNullOrWhiteSpace(filterName)) { return null; }
            return Filters?.FirstOrDefault(x => x.Name == filterName)?.Filters;
        }


        //public List<DoctypeFilterDto> GetDocTypeFilters(string docTypeAlias)
        //{s
        //    if (String.IsNullOrWhiteSpace(docTypeAlias)) { return null; }
        //    return DocTypeFilters?.FirstOrDefault(x => x.DocTypeAlias == docTypeAlias)?.Filters;
        //}


        public void AddFilters(string filterName, List<FilterDto> filters)
        {
            if (!String.IsNullOrWhiteSpace(filterName) && filters != null)
            {
                FiltersDto newFilter = new FiltersDto() { Name = filterName };
                if (Filters.FirstOrDefault(x => x.Name == filterName) is FiltersDto existingFilter)
                {
                    existingFilter.Filters = existingFilter.Filters.Union(filters).ToList();
                }
                else
                {
                    newFilter.Filters = filters;
                    Filters.Add(newFilter);
                }
            }
        }


        public void AddDoctypeFilter(DoctypeFilterDto doctypeFilter)
        {
            if (doctypeFilter != null && !String.IsNullOrWhiteSpace(doctypeFilter.DocTypeAlias))
            {
                if (DocTypeFilters == null || !DocTypeFilters.Any(x => x.DocTypeAlias == doctypeFilter.DocTypeAlias))
                {
                    DocTypeFilters.Add(doctypeFilter);
                }
            }
        }


        /// <summary>
        /// Filter all items on filters of filterName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allItems">all items to be filtered</param>
        /// <param name="filterName">name of filtergroup that contains all filters</param>
        /// <returns>filtered list of items</returns>
        private List<T> FilteredItems<T>(List<T> allItems, string filterName) where T : BaseFilter
        {
            if (allItems == null || String.IsNullOrWhiteSpace(filterName)) { return null; }

            List<T> filteredItems = allItems;
            List<FilterDto> filters = GetFilters(filterName);

            if (allItems.Any() && (filters?.Any() == true))
            {
                foreach (T item in allItems)
                {
                    if (item.FilterIds.Any() && item.FilterIds.Any(x => filters.Any(y => y.Id == x)))
                    {
                        filteredItems.Add(item);
                    }
                }
            }

            return filteredItems;
        }

        public List<T> FilteredItems<T>(List<T> allItems) where T : BaseFilter
        {
            List<T> result = allItems;
            if (result.Any())
            {
                foreach (var filtername in Filters.Select(x => x.Name))
                {
                    result = FilteredItems<T>(result, filtername);
                }
            }
            return result;
        }

    }


    [DataContract(Name = "overview")]
    public class OverviewDto<T> where T : class
    {
        [DataMember(Name = "items")]
        public List<T> Items { get; set; }

        [DataMember(Name = "overviewFilter")]
        public OverviewFilterDto OverviewFilter { get; set; }

        public OverviewDto()
        {
            Items = new List<T> { };
        }
    }


}
