using AutoMapper;
using PIMTool.Core.Models;

namespace PIMTool.Core.Helpers;

public static class PaginationHelper
{
    public static Task<PaginatedResult> BuildPaginatedResult<T, TDto>(IMapper mapper, IQueryable<T> source, int pageSize, int pageIndex)
    {
        var total = source.Count();
        if (total == 0)
        {
            return Task.FromResult(new PaginatedResult()
            {
                PageIndex = 1,
                PageSize = pageSize,
                Data = new List<TDto>(),
                LastPage = 1,
                IsLastPage = true,
                Total = total
            });
        }

        pageSize = Math.Max(1, pageSize);
        var lastPage = (int)Math.Ceiling((decimal)total / pageSize);
        lastPage = lastPage < 1 ? 1 : lastPage;
        pageIndex = pageIndex > lastPage ? lastPage : pageIndex;
        var isLastPage = pageIndex == lastPage;

        var paginatedResult = new PaginatedResult()
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            LastPage = lastPage,
            IsLastPage = isLastPage,
            Total = total
        };
        
        if (pageIndex > lastPage / 2)
        {
            var mod = total % pageSize;
            var skip = Math.Max((lastPage - pageIndex - 1) * pageSize + mod, 0);
            var take = isLastPage ? mod : pageSize;
            var reverse = source.Reverse();
            
            var res = reverse.Skip(skip).Take(take);
            paginatedResult.Data = res.Reverse();
            return Task.FromResult(paginatedResult);
        }
        
        var results = source.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize);
        paginatedResult.Data = results;
        return Task.FromResult(paginatedResult);
    }
    
    public static async Task<PaginatedResult> BuildPaginatedResult<T>(IQueryable<T> source, int pageSize, int pageIndex)
    {
        return await BuildPaginatedResult<T, T>(null!, source, pageSize, pageIndex);
    }
}