using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PIMTool.Core.Models;

namespace PIMTool.Core.Helpers;

public static class PaginationHelper
{
    public static PaginatedResult BuildPaginatedResult<T, TDto>(IMapper mapper, IQueryable<T> source, int pageSize, long pageIndex)
    {
        var total = source.Count();
        var lastPage = (long)Math.Ceiling((decimal)total / pageSize);
        lastPage = lastPage < 1 ? 1 : lastPage;
        pageIndex = pageIndex > lastPage ? lastPage : pageIndex;
        var isLastPage = pageIndex == lastPage;
        var results = source.Skip(((int)pageIndex - 1) * pageSize)
            .Take(pageSize).ToList();
        return new PaginatedResult()
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Data = mapper.Map<List<TDto>>(results),
            LastPage = lastPage,
            IsLastPage = isLastPage,
            Total = total
        };
    }
}