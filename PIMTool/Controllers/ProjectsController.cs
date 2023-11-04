using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIMTool.Controllers.Base;
using PIMTool.Core.Interfaces.Services;
using PIMTool.Core.Models.Request;

namespace PIMTool.Controllers;

[Authorize]
[Route("[controller]")]
public class ProjectsController : BaseController
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAllProjects()
    {
        return await ExecuteApiAsync(
            async () => await _projectService.GetAllProjectsAsync().ConfigureAwait(false)
        ).ConfigureAwait(false);
    }

    [HttpPost]
    public async Task<IActionResult> GetProjects(SearchProjectsRequest searchProjectsRequest)
    {
        return await ExecuteApiAsync(
            async () => await _projectService.FindProjectsAsync(searchProjectsRequest).ConfigureAwait(false)
        ).ConfigureAwait(false);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateProject(CreateProjectRequest createProjectRequest)
    {
        return await ExecuteApiAsync(
            async () => await _projectService.CreateProjectAsync(createProjectRequest).ConfigureAwait(false)
        ).ConfigureAwait(false);
    }

    [HttpPost]
    [Route("validate/{projectNumber:int}")]
    public async Task<IActionResult> ValidateProjectNumber(int projectNumber)
    {
        return await ExecuteApiAsync(
            async () => await _projectService.CheckIfProjectNumberExistsAsync(projectNumber).ConfigureAwait(false)
        ).ConfigureAwait(false);
    }
}