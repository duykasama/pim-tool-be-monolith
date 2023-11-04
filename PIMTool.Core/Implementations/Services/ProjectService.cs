using System.Linq.Expressions;
using Autofac;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Exceptions;
using PIMTool.Core.Helpers;
using PIMTool.Core.Implementations.Services.Base;
using PIMTool.Core.Interfaces.Repositories;
using PIMTool.Core.Interfaces.Services;
using PIMTool.Core.Models;
using PIMTool.Core.Models.Dtos;
using PIMTool.Core.Models.Request;
using ReflectionHelper = AutoMapper.Internal.ReflectionHelper;

namespace PIMTool.Core.Implementations.Services;

public class ProjectService : BaseService, IProjectService
{
    private readonly IProjectRepository _projectServerRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;
    public ProjectService(ILifetimeScope scope) : base(scope)
    {
        _projectServerRepository = Resolve<IProjectRepository>();
        _groupRepository = Resolve<IGroupRepository>();
        _unitOfWork = Resolve<IUnitOfWork>();
    }
    
    public IEnumerable<Project> GetAllProjects()
    {
        throw new NotImplementedException();
    }

    public async Task<ApiActionResult> GetAllProjectsAsync()
    {
        return new ApiActionResult();
    }

    public IEnumerable<Project> FindProjects()
    {
        throw new NotImplementedException();
    }

    public async Task<ApiActionResult> FindProjectsAsync(SearchProjectsRequest req)
    {
        var mapper = Resolve<IMapper>();
        var projects = (await _projectServerRepository
            .FindByAsync(e => !e.IsDeleted));

        if (req.SearchCriteria is not null)
        {
            var conjunctionWhere =
                ExpressionHelper.CombineOrExpressions<Project>(req.SearchCriteria.ConjunctionSearchInfos, p => !p.IsDeleted);
            // projects = projects.Where(conjunctionWhere);

            var disjunctionWhere =
                ExpressionHelper.CombineAndExpressions<Project>(req.SearchCriteria.DisjunctionSearchInfos, p => true);
            // projects = projects.Where(disjunctionWhere);
        }
        Expression<Func<Project, object>> orderByExp = proj => proj.CreatedAt;

        var orderByExpr = projects.OrderBy(p => "");
        if (req.SortByInfos is not null)
        {
            // orderByExpr = req.SortByInfos.Aggregate(orderByExpr, (current, sort) => sort.Ascending
            //     ? current.ThenBy(p => ReflectionHelper.GetPropertyValueByName(p, sort.FieldName))
            //     : current.ThenByDescending(p => ReflectionHelper.GetPropertyValueByName(p, sort.FieldName)));
        }

        var projectDtos = orderByExpr.Select(mapper.Map<DtoProject>);
        
        return new ApiActionResult(true) { Data = BuildPaginatedResult(projectDtos, req.PageSize, req.PageIndex)};
    }

    public async Task<ApiActionResult> CreateProjectAsync(CreateProjectRequest createProjectRequest)
    {
        if (await _projectServerRepository.ExistsAsync(p => p.ProjectNumber == createProjectRequest.ProjectNumber).ConfigureAwait(false))
        {
            throw new ProjectNumberAlreadyExistsException();
        }

        if (!await _groupRepository.ExistsAsync(g => g.Id == createProjectRequest.GroupId && !g.IsDeleted))
        {
            throw new GroupDoesNotExistException();
        }
        var newProject = Resolve<IMapper>().Map<Project>(createProjectRequest);
        newProject.SetCreatedInfo(Guid.Empty);

        await _projectServerRepository.AddAsync(newProject).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
        return new ApiActionResult(true, "Project created successfully");
    }

    public async Task<ApiActionResult> CheckIfProjectNumberExistsAsync(int projectNumber)
    {
        var projects = await _projectServerRepository.FindByAsync(p => !p.IsDeleted && p.ProjectNumber == projectNumber);
        var isValid = projects.IsNullOrEmpty();
        var result = new ApiActionResult(true)
        {
            Detail = isValid ? "Project number does not exist" : "Project number already exists",
            Data = isValid
        };
        return result;
    }

    private PaginatedResult BuildPaginatedResult(IEnumerable<DtoProject> projects, int pageSize, long pageIndex)
    {
        var dtoProjects = projects.ToList();
        var lastPage = (long)Math.Ceiling((decimal)dtoProjects.Count / pageSize);
        pageIndex = pageIndex > lastPage ? lastPage : pageIndex;
        var isLastPage = pageIndex == lastPage;
        var results = dtoProjects.Skip(((int)pageIndex - 1) * pageSize)
            .Take(pageSize);
        return new PaginatedResult()
        {
            PageIndex = pageIndex, PageSize = pageSize, Data = results, LastPage = lastPage, IsLastPage = isLastPage
        };
    }
}