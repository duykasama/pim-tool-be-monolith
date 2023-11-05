using System.Reflection;
using Autofac;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PIMTool.Core.Constants;
using PIMTool.Core.Helpers;
using PIMTool.Core.Implementations.Repositories;
using PIMTool.Core.Implementations.Services;
using PIMTool.Core.Interfaces.Repositories;
using PIMTool.Core.Interfaces.Services;
using PIMTool.Core.Mappings.AutoMapper;
using PIMTool.Extensions;
using PIMTool.Middlewares;

namespace PIMTool;

public class Startup
{
    public ILifetimeScope Scope { get; set; }
    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(NLogService))!)
            .As<ILoggerService>()
            .InstancePerLifetimeScope();
       
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IAppDbContext))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(UnitOfWork))!)
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IProjectRepository))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IRefreshTokenRepository))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IPIMUserRepository))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IAuthenticationService))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IJwtService))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IProjectService))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IProjectService))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IGroupRepository))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IGroupService))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IEmployeeRepository))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IEmployeeService))!)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        

        var config = new MapperConfiguration(AppModelMapper.MappingDto);
        var mapper = config.CreateMapper();
        builder.RegisterInstance(mapper).As<IMapper>();
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        LogHelper.InitLoggerService(new NLogService(Scope));

        services.AddControllers().AddNewtonsoftJson(opt => 
            opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
        services.AddJwtAuthentication();
        services.AddAuthorization();
        services.AddAppCors();
    }

    public void Configure(IApplicationBuilder app)
    {
        DataAccessHelper.InitConfiguration(app.ApplicationServices.GetRequiredService<IConfiguration>());
        DataAccessHelper.MigrateDatabase(Assembly.GetExecutingAssembly().GetName().Name!);
        
        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseCors(CoreConstants.APP_CORS);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}