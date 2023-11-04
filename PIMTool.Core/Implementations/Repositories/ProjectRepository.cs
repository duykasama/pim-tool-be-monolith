using System;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Implementations.Repositories.Base;
using PIMTool.Core.Interfaces.Repositories;

namespace PIMTool.Core.Implementations.Repositories;

public class ProjectRepository : Repository<Project, Guid>, IProjectRepository
{
    public ProjectRepository(IAppDbContext appDbContext) : base(appDbContext)
    {
    }
}