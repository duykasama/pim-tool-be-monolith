using System.Linq.Expressions;
using Autofac;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public ProjectService(ILifetimeScope scope) : base(scope)
    {
        _projectRepository = Resolve<IProjectRepository>();
        _groupRepository = Resolve<IGroupRepository>();
        _userRepository = Resolve<IPIMUserRepository>();
        _employeeRepository = Resolve<IEmployeeRepository>();
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
        var projects = await _projectRepository
            .FindByAsync(e => !e.IsDeleted).ConfigureAwait(false);

        if (req.SearchCriteria is not null)
        {
            Expression<Func<Project, bool>> conjunctionExpr = project => false;

            foreach (var searchInfo in req.SearchCriteria.ConjunctionSearchInfos)
            {
                var valueStr = searchInfo.Value.ToString()?.Trim();
                conjunctionExpr = searchInfo.FieldName switch
                {
                    "projectNumber" => ExpressionHelper.CombineOrExpressions(conjunctionExpr,
                        (Expression<Func<Project, bool>>)(project =>
                            EF.Functions.Like(project.ProjectNumber.ToString(), $"%{valueStr}%"))),
                    "name" => ExpressionHelper.CombineOrExpressions(conjunctionExpr,
                        (Expression<Func<Project, bool>>)(project => 
                            EF.Functions.Like(project.Name, $"%{valueStr}%"))),
                    "customer" => ExpressionHelper.CombineOrExpressions(conjunctionExpr,
                        (Expression<Func<Project, bool>>)(project =>
                            EF.Functions.Like(project.Customer, $"%{valueStr}%"))),
                    _ => conjunctionExpr
                };
            }
            
            projects = req.SearchCriteria.ConjunctionSearchInfos.IsNullOrEmpty() 
                ? projects 
                : projects.Where(conjunctionExpr);

            foreach (var searchInfo in req.SearchCriteria.DisjunctionSearchInfos)
            {
                var valueStr = searchInfo.Value.ToString()!.Trim();
                projects = searchInfo.FieldName switch
                {
                    "status" => projects.Where(p => EF.Functions.Like(p.Status, $"%{valueStr}%")),
                    _ => projects
                };
            }
        }

        if (req.AdvancedFilter is not null)
        {
            projects = projects
                .Include(p => p.Group.Leader)
                .Where(p => 
                    EF.Functions.Like(p.Group.Leader.FirstName, $"%{req.AdvancedFilter.LeaderName}%") ||
                    EF.Functions.Like(p.Group.Leader.LastName, $"%{req.AdvancedFilter.LeaderName}%"));
            
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
            foreach (var sortInfo in req.SortByInfos)
            {
                orderedProjects = sortInfo.FieldName switch
                {
                    "projectNumber" => sortInfo.Ascending
                        ? orderedProjects.ThenBy(p => p.ProjectNumber)
                        : orderedProjects.ThenByDescending(p => p.ProjectNumber),
                    "name" => sortInfo.Ascending
                        ? orderedProjects.ThenBy(p => p.Name)
                        : orderedProjects.ThenByDescending(p => p.Name),
                    "status" => sortInfo.Ascending
                        ? orderedProjects.ThenBy(p => p.Status)
                        : orderedProjects.ThenByDescending(p => p.Status),
                    "customer" => sortInfo.Ascending
                        ? orderedProjects.ThenBy(p => p.Customer)
                        : orderedProjects.ThenByDescending(p => p.Customer),
                    "startDate" => sortInfo.Ascending
                        ? orderedProjects.ThenBy(p => p.StartDate)
                        : orderedProjects.ThenByDescending(p => p.StartDate),
                    _ => orderedProjects
                };
            }
        }

        var paginatedResult = await PaginationHelper.BuildPaginatedResult(orderedProjects.ProjectTo<DtoProject>(_mapper.ConfigurationProvider),
                req.PageSize, req.PageIndex).ConfigureAwait(false);
        
        return new ApiActionResult(true) { Data = paginatedResult };
    }

    public async Task<ApiActionResult> CreateProjectAsync(CreateProjectRequest createProjectRequest)
    {
        if (await _projectRepository.ExistsAsync(p => !p.IsDeleted && p.ProjectNumber == createProjectRequest.ProjectNumber).ConfigureAwait(false))
        {
            throw new ProjectNumberAlreadyExistsException();
        }

        if (!await _groupRepository.ExistsAsync(g => g.Id == createProjectRequest.GroupId && !g.IsDeleted))
        {
            throw new GroupDoesNotExistException();
        }

        var existingProject = await _projectRepository
            .GetAsync(p => p.ProjectNumber == createProjectRequest.ProjectNumber && p.IsDeleted).ConfigureAwait(false);
        if (existingProject is not null)
        {
            await _projectRepository.DeleteAsync(existingProject.Id);
        }
        
        var newProject = _mapper.Map<Project>(createProjectRequest);
        newProject.SetCreatedInfo(Guid.Empty);
        
        foreach (var memberId in createProjectRequest.MemberIds)
        {
            var member = await _employeeRepository.GetAsync(e => !e.IsDeleted && e.Id == memberId);
            if (member is null)
            {
                throw new EmployeeDoesNotExistException();
            }
            
            _employeeRepository.SetModified(member);
            newProject.Employees.Add(member);
        }
        
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
            .Include(p => p.Employees)
            .FirstOrDefaultAsync();

        // var project = await _projectRepository.GetAsync(p => !p.IsDeleted && p.Id == id);
        if (project is null)
        {
            throw new ProjectDoesNotExistException();
        }

        var currentVersion = project.Version;
        _mapper.Map(request, project);
        project.Employees.Clear();
        
        foreach (var memberId in request.MemberIds)
        {
            var member = await _employeeRepository.GetAsync(e => !e.IsDeleted && e.Id == memberId);
            if (member is null)
            {
                throw new EmployeeDoesNotExistException();
            }
        
            _employeeRepository.SetModified(member);
            project.Employees.Add(member);
        }
        
        if (project.Version != currentVersion)
        {
            throw new VersionMismatchedException();
        }
        
        project.SetUpdatedInfo(updaterGuidId);
        await _projectRepository.UpdateAsync(project);
        await _unitOfWork.CommitAsync();
        return new ApiActionResult(true) {Detail = "Project updated successfully"};
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

        await _projectRepository.SoftDeleteAsync(id);
        await _unitOfWork.CommitAsync();
        return new ApiActionResult(true);
    }

    public async Task<ApiActionResult> FindProjectByProjectNumberAsync(int projectNumber)
    {
        var project = await (await _projectRepository.FindByAsync(p => !p.IsDeleted && p.ProjectNumber == projectNumber))
            .Include(p => p.Employees)
            .FirstOrDefaultAsync();
        if (project is null)
        {
            throw new ProjectDoesNotExistException();
        }

        return new ApiActionResult(true) { Data = _mapper.Map<DtoProjectDetail>(project) };
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