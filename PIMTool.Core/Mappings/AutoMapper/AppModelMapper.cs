using System.ComponentModel;
using AutoMapper;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Helpers;
using PIMTool.Core.Models;
using PIMTool.Core.Models.Dtos;
using PIMTool.Core.Models.Request;

namespace PIMTool.Core.Mappings.AutoMapper;

public static class AppModelMapper
{
    public static void MappingDto(IMapperConfigurationExpression config)
    {
        MapModels(config);
    }

    private static void MapModels(IMapperConfigurationExpression config)
    {
        config.CreateMap<Project, DtoProject>().ReverseMap();
        config.CreateMap<CreateProjectRequest, Project>();
        config.CreateMap<CreateEmployeeRequest, Employee>();
        config.CreateMap<Employee, DtoEmployee>();
        config.CreateMap<CreateGroupRequest, Group>();
        config.CreateMap<Group, DtoGroup>();
        config.CreateMap<UserRegisterModel, PIMUser>()
            .AfterMap((model, user) => user.Password = EncryptionHelper.Encrypt(model.Password));
    }
}