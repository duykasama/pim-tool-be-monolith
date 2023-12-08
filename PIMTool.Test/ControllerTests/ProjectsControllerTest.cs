using Microsoft.AspNetCore.Mvc;
using Moq;
using PIMTool.Controllers;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Interfaces.Services;
using PIMTool.Core.Models;
using PIMTool.Core.Models.Request;
using PIMTool.Test.ServiceTests;

namespace PIMTool.Test.ControllerTests;

public class ProjectsControllerTest : BaseTest
{
    [Test]
    public async Task GetAllProjects_Success()
    {
        // Arrange
        var projectController = new ProjectsController(ResolveService<IProjectService>());
        
        // Act
        var result = await projectController.GetAllProjects();
        
        // Assert
        Assert.That(result, Is.AssignableTo<OkObjectResult>());
    }
    
    [Test]
    public async Task GetProjectsPagination_Success()
    {
        // Arrange
        var projectController = new ProjectsController(ResolveService<IProjectService>());
        var request = new SearchProjectsRequest()
        {

        };
        
        // Act
        var result = await projectController.GetProjects(request);
        
        // Assert
        Assert.That(result, Is.AssignableTo<OkObjectResult>());
    }
    
    [Test]
    public async Task CreateProject_Success()
    {
        // Arrange
        var projectController = new ProjectsController(ResolveService<IProjectService>());
        var request = new CreateProjectRequest()
        {
            ProjectNumber = 1000,
            Name = "Mock customer",
            Customer = "Mock customer",
            Status = "NEW",
            StartDate = DateTime.Now,
            GroupId = groupId,
            MemberIds = new List<Guid> {employeeId}
        };
        
        // Act
        var result = await projectController.CreateProject(request);
        
        // Assert
        Assert.That(result, Is.AssignableTo<OkObjectResult>());
    }

    [Test]
    public async Task ValidProjectNumber_Success()
    {
        // Arrange
        var projectController = new ProjectsController(ResolveService<IProjectService>());
        
        // Act
        var result = await projectController.ValidateProjectNumber(1);
        
        // Assert
        Assert.That(result, Is.AssignableTo<OkObjectResult>());
    }
}