using System.Collections.Generic;

namespace PIMTool.Core.Models.Request;

public class SearchCriteria
{
    public IList<SearchByInfo> ConjunctionSearchInfos { get; set; }
    public IList<SearchByInfo> DisjunctionSearchInfos { get; set; }

}