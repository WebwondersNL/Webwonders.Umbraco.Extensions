using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Webwonders.Models
{
    [DataContract(Name = "searchResult")]
    public class WWSearchResult
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "imageUrl")]
        public string ImageUrl { get; set; }
        [DataMember(Name = "url")]
        public string Url { get; set; }
    }

    public class WWSearchParameters
    {
        public string SearchString { get; set; }
        public string Culture { get; set; }
        public int MaxResults { get; set; }
        public IEnumerable<int>SearchPriority { get; set; }
    }
}
