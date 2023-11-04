using Microsoft.AspNetCore.Mvc;
using PIMTool.Controllers.Base;

namespace PIMTool.Controllers;

[Route("[controller]")]
public class GroupsController : BaseController
{
    [Route("all")]
    [HttpGet]
    public async Task<IActionResult> GetAllGroups()
    {
        return Ok();
    }
    
    [HttpPost]
    public async Task<IActionResult> GetGroups()
    {
        return Ok();
    }
    
    [Route("{id:guid}")]
    [HttpPost]
    public async Task<IActionResult> GetGroup(Guid id)
    {
        return Ok();
    }
    
    [Route("create")]
    [HttpPost]
    public async Task<IActionResult> CreateGroup()
    {
        return Ok();
    }
    
    [Route("{id:guid}")]
    [HttpPut]
    public async Task<IActionResult> UpdateGroup(Guid id)
    {
        return Ok();
        
    }
    
    [Route("{id:guid}")]
    [HttpDelete]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        return Ok();
    }
}