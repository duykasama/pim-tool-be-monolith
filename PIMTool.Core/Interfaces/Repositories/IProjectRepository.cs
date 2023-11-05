using System;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Interfaces.Repositories.Base;

namespace PIMTool.Core.Interfaces.Repositories;

public interface IProjectRepository : IRepository<Project, Guid>
{
    void SetModified(Project project);
}