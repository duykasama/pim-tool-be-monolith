using PIMTool.Core.Interfaces.Services.Base;
using PIMTool.Core.Models;
using PIMTool.Core.Models.Request;

namespace PIMTool.Core.Interfaces.Services;

public interface IGroupService : IService
{
    Task<ApiActionResult> CreateGroupAsync(CreateGroupRequest createGroupRequest);
    Task<ApiActionResult> FindGroupsAsync(SearchGroupsRequest searchGroupsRequest);
}