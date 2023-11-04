﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PIMTool.Core.Constants;
using PIMTool.Core.Helpers;
using PIMTool.Core.Interfaces;
using PIMTool.Core.Interfaces.Repositories;

namespace PIMTool.Core.Implementations.Repositories;

public class AppDbContext : DbContext, IAppDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // var connectionString = DataAccessHelper.GetConnectionString();
        var connectionString = "Server=localhost;User Id=sa;Password=12345;Initial Catalog=PIMTool.Scaffold;Trust Server Certificate=True;";
        optionsBuilder.UseSqlServer(connectionString,options =>
                options.CommandTimeout(DataAccessConstants.DEAULT_COMMAND_TIMEOUT_IN_SECONDS))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var types = ReflectionHelper.GetClassesFromAssignableType(typeof(IModelMapper));
        foreach (var t in types)
        {
            var instance = Activator.CreateInstance(t) as IModelMapper;
            instance?.Mapping(modelBuilder);
        }
    }
    
    public DbSet<T> CreateSet<T>() where T : class
    {
        return base.Set<T>();
    }
    
    public new void Attach<T>(T item) where T : class
    {
        base.Entry<T>(item).State = EntityState.Unchanged;
    }
    
    public void SetModified<T>(T item) where T : class
    {
        base.Entry<T>(item).State = EntityState.Modified;
    }
    
    public void Refresh<T>(T item) where T : class
    {
        base.Entry<T>(item).Reload();
    }
    
    public int ExecuteSqlRaw(string sql, params object[] parameters)
    {
        // return Database.ExecuteSqlRaw(sql, parameters);
        return 0;
    }
    
    public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
    {
        return 0;
        // return await Database.ExecuteSqlRawAsync(sql, parameters);
    }
    
    public new void SaveChanges()
    {
        base.SaveChanges();
    }
    
    public async Task SaveChangesAsync()
    {
        await base.SaveChangesAsync();
    }
}