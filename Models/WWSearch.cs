using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Webwonders.Extensions;

[DataContract(Name = "searchResult")]
public class WWSearchResult
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "description")]
    public string Description { get; set; } = string.Empty;

    [DataMember(Name = "imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [DataMember(Name = "url")]
    public string Url { get; set; } = string.Empty;
}

public class WWSearchParameters
{
    public string SearchString { get; set; } = string.Empty;

    public string Culture { get; set; } = string.Empty;

    public int MaxResults { get; set; }

    public IEnumerable<int> SearchPriority { get; set; } = new List<int>();
}
