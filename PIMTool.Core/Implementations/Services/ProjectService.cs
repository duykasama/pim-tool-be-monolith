using Autofac;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PIMTool.Core.Constants;
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
    private readonly IPIMUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public ProjectService(ILifetimeScope scope) : base(scope)
    {
        _projectRepository = Resolve<IProjectRepository>();
        _groupRepository = Resolve<IGroupRepository>();
        _userRepository = Resolve<IPIMUserRepository>();
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
            .FindByAsync(e => !e.IsDeleted).ConfigureAwait(false))
            .Include(p => p.Group)
            .ThenInclude(g => g.Leader)
            .AsQueryable();

        if (req.SearchCriteria is not null)
        {
            var conjunctionWhere =
                ExpressionHelper.CombineOrExpressions<Project>(req.SearchCriteria.ConjunctionSearchInfos, p => !p.IsDeleted);
            projects = projects.AsEnumerable().Where(conjunctionWhere).AsQueryable();

            var disjunctionWhere =
                ExpressionHelper.CombineAndExpressions<Project>(req.SearchCriteria.DisjunctionSearchInfos, p => true);
            projects = projects.AsEnumerable().Where(disjunctionWhere).AsQueryable();
        }

        if (req.AdvancedFilter is not null)
        {
            projects = projects.Where(p =>
                p.Group.Leader.FirstName.Contains(req.AdvancedFilter.LeaderName, StringComparison.OrdinalIgnoreCase) ||
                p.Group.Leader.LastName.Contains(req.AdvancedFilter.LeaderName));
            if (req.AdvancedFilter.StartDateRange?.From != null)
            {
                projects = projects.Where(p => p.StartDate >= req.AdvancedFilter.StartDateRange.From);
            }
            if (req.AdvancedFilter.StartDateRange?.To != null)
            {
                projects = projects.Where(p => p.StartDate <= req.AdvancedFilter.StartDateRange.To);
            }
            
            if (req.AdvancedFilter.EndDateRange?.From != null)
            {
                projects = projects.Where(p => p.EndDate != null && p.EndDate >= req.AdvancedFilter.EndDateRange.From);
            }
            if (req.AdvancedFilter.EndDateRange?.To != null)
            {
                projects = projects.Where(p => p.EndDate != null && p.EndDate <= req.AdvancedFilter.EndDateRange.To);
            }
        }

        var orderedProjects = projects.OrderBy(p => "");
        if (req.SortByInfos is not null)
        {
            orderedProjects = req.SortByInfos.Aggregate(orderedProjects, (current, sort) => sort.Ascending
                ? current.ThenBy(p => ReflectionHelper.GetPropertyValueByName(p, sort.FieldName))
                : current.ThenByDescending(p => ReflectionHelper.GetPropertyValueByName(p, sort.FieldName)));
        }

        var paginatedResult = PaginationHelper.BuildPaginatedResult<Project, DtoProject>(_mapper, orderedProjects,
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

    public async Task<ApiActionResult> UpdateProjectAsync(UpdateProjectRequest request, Guid id, string updaterId)
    {
        var parseSuccess = Guid.TryParse(updaterId, out var updaterGuidId);
        if (!parseSuccess)
        {
            throw new InvalidGuidIdException();
        }
        
        if (!await _userRepository.ExistsAsync(u => !u.IsDeleted && u.Id == updaterGuidId))
        {
            throw new UserDoesNotExistException();
        }

        var project = await (await _projectRepository
                .FindByAsync(p => !p.IsDeleted && p.Id == id))
            .FirstOrDefaultAsync();
        if (project is null)
        {
            throw new ProjectDoesNotExistException();
        }

        if (project.Version != request.Version)
        {
            throw new VersionMismatchedException();
        }
        
        _mapper.Map(request, project);
        project.SetUpdatedInfo(updaterGuidId);
        await _projectRepository.UpdateAsync(project);
        await _unitOfWork.CommitAsync();
        
        return new ApiActionResult(true);
    }

    public async Task<ApiActionResult> DeleteProjectAsync(Guid id)
    {
        var project = await (await _projectRepository
                .FindByAsync(p => !p.IsDeleted && p.Id == id)
            ).FirstOrDefaultAsync();
        if (project is null)
        {
            throw new ProjectDoesNotExistException();
        }

        if (project.Status != ProjectStatus.NEW)
        {
            throw new IndelibleProjectException();
        }

        await _projectRepository.DeleteAsync(id);
        await _unitOfWork.CommitAsync();
        return new ApiActionResult(true);
    }

    public async Task<ApiActionResult> FindProjectByProjectNumberAsync(int projectNumber)
    {
        var project = await (await _projectRepository.FindByAsync(p => !p.IsDeleted && p.ProjectNumber == projectNumber))
            .FirstOrDefaultAsync();
        if (project is null)
        {
            throw new ProjectDoesNotExistException();
        }

        return new ApiActionResult(true) { Data = _mapper.Map<DtoProject>(project) };
    }

    public async Task<ApiActionResult> DeleteMultipleProjectsAsync(DeleteMultipleProjectsRequest request)
    {
        foreach (var projectId in request.ProjectIds)
        {
            var project = await (await _projectRepository
                    .FindByAsync(p => !p.IsDeleted && p.Id == projectId)
                ).FirstOrDefaultAsync();
            if (project is null)
            {
                throw new ProjectDoesNotExistException();
            }

            if (project.Status != ProjectStatus.NEW)
            {
                throw new IndelibleProjectException();
            }

            await _projectRepository.DeleteAsync(projectId);
        }
        await _unitOfWork.CommitAsync();
        return new ApiActionResult(true);
    }
}