using Autofac;
using AutoMapper;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Helpers;
using PIMTool.Core.Implementations.Services.Base;
using PIMTool.Core.Interfaces.Repositories;
using PIMTool.Core.Interfaces.Services;
using PIMTool.Core.Models;
using PIMTool.Core.Models.Dtos;
using PIMTool.Core.Models.Request;

namespace PIMTool.Core.Implementations.Services;

public class GroupService : BaseService, IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public GroupService(ILifetimeScope scope) : base(scope)
    {
        _groupRepository = Resolve<IGroupRepository>();
        _unitOfWork = Resolve<IUnitOfWork>();
    }

    public async Task<ApiActionResult> CreateGroupAsync(CreateGroupRequest createGroupRequest)
    {
        var group = Resolve<IMapper>().Map<Group>(createGroupRequest);
        group.SetCreatedInfo(Guid.Empty);
        await _groupRepository.AddAsync(group);
        await _unitOfWork.CommitAsync();
        return new ApiActionResult(true);
    }

    public async Task<ApiActionResult> FindGroupsAsync(SearchGroupsRequest req)
    {
        var groups = await _groupRepository.FindByAsync(g => !g.IsDeleted);
        var paginatedResult =
            await PaginationHelper.BuildPaginatedResult<Group, DtoGroup>(Resolve<IMapper>(), groups, req.PageSize,
                req.PageIndex);
        return new ApiActionResult(true) { Data = paginatedResult };
    }
}