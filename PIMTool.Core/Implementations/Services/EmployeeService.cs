using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

public class EmployeeService : BaseService, IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public EmployeeService(ILifetimeScope scope) : base(scope)
    {
        _employeeRepository = Resolve<IEmployeeRepository>();
        _unitOfWork = Resolve<IUnitOfWork>();
    }

    public ApiActionResult GetAllEmployees()
    {
        throw new NotImplementedException();
    }

    public async Task<ApiActionResult> GetAllEmployeesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<ApiActionResult> FindEmployeesAsync(SearchEmployeesRequest req)
    {
        var employees = (await _employeeRepository.FindByAsync(e => !e.IsDeleted));

        if (req.SearchCriteria is not null)
        {
            var conjunctionWhere =
                ExpressionHelper.CombineOrExpressions<Employee>(req.SearchCriteria.ConjunctionSearchInfos, p => !p.IsDeleted);
            employees = employees.AsEnumerable().Where(conjunctionWhere).AsQueryable();

            var disjunctionWhere =
                ExpressionHelper.CombineAndExpressions<Employee>(req.SearchCriteria.DisjunctionSearchInfos, p => true);
            employees = employees.AsEnumerable().Where(disjunctionWhere).AsQueryable();
        }

        var orderEmployees = employees.OrderBy(p => "");
        if (req.SortByInfos is not null)
        {
            orderEmployees = req.SortByInfos.Aggregate(orderEmployees, (current, sort) => sort.Ascending
                ? current.ThenBy(p => ReflectionHelper.GetPropertyValueByName(p, sort.FieldName))
                : current.ThenByDescending(p => ReflectionHelper.GetPropertyValueByName(p, sort.FieldName)));
        }

        var paginatedResult = await PaginationHelper.BuildPaginatedResult<Employee, DtoEmployee>(Resolve<IMapper>(), orderEmployees, req.PageSize, req.PageIndex);
            
        return new ApiActionResult(true) { Data = paginatedResult};
    }

    public async Task<ApiActionResult> FindEmployeeAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiActionResult> CreateEmployeeAsync(CreateEmployeeRequest createEmployeeRequest)
    {
        var employee = Resolve<IMapper>().Map<Employee>(createEmployeeRequest);
        employee.SetCreatedInfo(Guid.Empty);
        await _employeeRepository.AddAsync(employee);
        await _unitOfWork.CommitAsync();
        return new ApiActionResult(true);
    }
}