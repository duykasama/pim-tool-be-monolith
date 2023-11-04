using System;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Implementations.Repositories.Base;
using PIMTool.Core.Interfaces.Repositories;

namespace PIMTool.Core.Implementations.Repositories;

public class EmployeeRepository : Repository<Employee, Guid>, IEmployeeRepository
{
    public EmployeeRepository(IAppDbContext appDbContext) : base(appDbContext)
    {
    }
}