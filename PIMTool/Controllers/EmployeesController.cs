using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIMTool.Controllers.Base;

namespace PIMTool.Controllers;

[Authorize]
[Route("[controller]")]
public class EmployeesController : BaseController
{
    [Route("all")]
    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        return Ok();
    }
    
    [HttpPost]
    public async Task<IActionResult> GetEmployees()
    {
        return Ok();
    }
    
    [Route("{id:guid}")]
    [HttpPost]
    public async Task<IActionResult> GetEmployee(Guid id)
    {
        return Ok();
    }
    
    [Route("create")]
    [HttpPost]
    public async Task<IActionResult> CreateEmployee()
    {
        return Ok();
    }
    
    [Route("{id:guid}")]
    [HttpPut]
    public async Task<IActionResult> UpdateEmployee(Guid id)
    {
        return Ok();
        
    }
    
    [Route("{id:guid}")]
    [HttpDelete]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        return Ok();
    }
}