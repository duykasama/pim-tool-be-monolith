using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Implementations.Repositories.Base;
using PIMTool.Core.Interfaces.Repositories;

namespace PIMTool.Core.Implementations.Repositories;

public class PIMUserRepository : Repository<PIMUser, Guid>, IPIMUserRepository
{
    public PIMUserRepository(IAppDbContext appDbContext) : base(appDbContext)
    {
    }
}