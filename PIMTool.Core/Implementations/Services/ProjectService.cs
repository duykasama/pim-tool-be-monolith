using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

namespace PIMTool.Core.Implementations.Services;

public class ProjectService : BaseService, IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public ProjectService(ILifetimeScope scope) : base(scope)
    {
        _projectRepository = Resolve<IProjectRepository>();
        _groupRepository = Resolve<IGroupRepository>();
        _unitOfWork = Resolve<IUnitOfWork>();
        _mapper = Resolve<IMapper>();
    }

    public async Task<ApiActionResult> GetAllProjectsAsync()
    {
        var projects = await _projectRepository.FindByAsync(p => !p.IsDeleted);
        var projectDtos = _mapper.Map<IEnumerable<DtoProject>>(await projects.ToListAsync());
        return new ApiActionResult(true) { Data = projectDtos };
    }

    public async Task<ApiActionResult> FindProjectsAsync(SearchProjectsRequest req)
    {
        var projects = (await _projectRepository
            .FindByAsync(e => !e.IsDeleted));

        if (req.SearchCriteria is not null)
        {
            var conjunctionWhere =
                ExpressionHelper.CombineOrExpressions<Project>(req.SearchCriteria.ConjunctionSearchInfos, p => !p.IsDeleted);
            projects = projects.AsEnumerable().Where(conjunctionWhere).AsQueryable();

            var disjunctionWhere =
                ExpressionHelper.CombineAndExpressions<Project>(req.SearchCriteria.DisjunctionSearchInfos, p => true);
            projects = projects.AsEnumerable().Where(disjunctionWhere).AsQueryable();
        }

        var orderedProjects = projects.OrderBy(p => p.CreatedAt);
        if (req.SortByInfos is not null)
        {
            orderedProjects = req.SortByInfos.Aggregate(orderedProjects, (current, sort) => sort.Ascending
                ? current.ThenBy(p => ReflectionHelper.GetPropertyValueByName(p, sort.FieldName))
                : current.ThenByDescending(p => ReflectionHelper.GetPropertyValueByName(p, sort.FieldName)));
        }

        var paginatedResult =
            await PaginationHelper.BuildPaginatedResult<Project, DtoProject>(_mapper, orderedProjects,
                req.PageSize, req.PageIndex);
        
        return new ApiActionResult(true) { Data = paginatedResult};
    }

    public async Task<ApiActionResult> CreateProjectAsync(CreateProjectRequest createProjectRequest)
    {
        if (await _projectRepository.ExistsAsync(p => p.ProjectNumber == createProjectRequest.ProjectNumber).ConfigureAwait(false))
        {
            throw new ProjectNumberAlreadyExistsException();
        }

        if (!await _groupRepository.ExistsAsync(g => g.Id == createProjectRequest.GroupId && !g.IsDeleted))
        {
            throw new GroupDoesNotExistException();
        }
        var newProject = _mapper.Map<Project>(createProjectRequest);
        newProject.SetCreatedInfo(Guid.Empty);
        
        await _projectRepository.AddAsync(newProject).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
        return new ApiActionResult(true, "Project created successfully");
    }

    public async Task<ApiActionResult> CheckIfProjectNumberExistsAsync(int projectNumber)
    {
        var projects = await _projectRepository.FindByAsync(p => !p.IsDeleted && p.ProjectNumber == projectNumber);
        var isValid = projects.IsNullOrEmpty();
        var result = new ApiActionResult(true)
        {
            Detail = isValid ? "Project number does not exist" : "Project number already exists",
            Data = isValid
        };
        return result;
    }
}