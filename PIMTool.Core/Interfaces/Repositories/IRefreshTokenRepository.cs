﻿using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Interfaces.Repositories.Base;

namespace PIMTool.Core.Interfaces.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    
}