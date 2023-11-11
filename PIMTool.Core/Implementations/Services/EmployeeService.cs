﻿using System.Collections;
using Autofac;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Exceptions;
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
    private readonly IPIMUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public EmployeeService(ILifetimeScope scope) : base(scope)
    {
        _employeeRepository = Resolve<IEmployeeRepository>();
        _userRepository = Resolve<IPIMUserRepository>();
        _unitOfWork = Resolve<IUnitOfWork>();
        _mapper = Resolve<IMapper>();
    }

    public async Task<ApiActionResult> GetAllEmployeesAsync()
    {
        var result = await _employeeRepository.GetAllAsync();
        var resultDto = _mapper.Map<IEnumerable<DtoEmployee>>(await result.ToListAsync());
        return new ApiActionResult(true) { Data = resultDto };
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

        var orderEmployees = employees.OrderBy(p => p.CreatedAt);
        if (req.SortByInfos is not null)
        {
            orderEmployees = req.SortByInfos.Aggregate(orderEmployees, (current, sort) => sort.Ascending
                ? current.ThenBy(p => ReflectionHelper.GetPropertyValueByName(p, sort.FieldName))
                : current.ThenByDescending(p => ReflectionHelper.GetPropertyValueByName(p, sort.FieldName)));
        }

        var paginatedResult = PaginationHelper.BuildPaginatedResult<Employee, DtoEmployee>(_mapper, orderEmployees, req.PageSize, req.PageIndex);
            
        return new ApiActionResult(true) { Data = paginatedResult};
    }

    public async Task<ApiActionResult> FindEmployeeAsync(Guid id)
    {
        var employees = await _employeeRepository.FindByAsync(e => e.Id == id && !e.IsDeleted);
        var employee = await employees
            .Include(e => e.Groups)
            .Include(e => e.Projects)
            .FirstOrDefaultAsync();
        if (employee is null)
        {
            throw new EmployeeDoesNotExistException();
        }

        var employeeDto = _mapper.Map<DtoEmployeeDetail>(employee);
        return new ApiActionResult(true) { Data = employeeDto };
    }

    public async Task<ApiActionResult> CreateEmployeeAsync(CreateEmployeeRequest createEmployeeRequest)
    {
        var employee = _mapper.Map<Employee>(createEmployeeRequest);
        employee.SetCreatedInfo(Guid.Empty);
        await _employeeRepository.AddAsync(employee);
        await _unitOfWork.CommitAsync();
        return new ApiActionResult(true);
    }

    public async Task<ApiActionResult> UpdateEmployeeAsync(UpdateEmployeeRequest request, Guid id, string updaterId)
    {
        var parseSuccess = Guid.TryParse(updaterId, out var updaterGuidId);
        if (!parseSuccess)
        {
            throw new InvalidGuidIdException();
        }
        if (!await _userRepository.ExistsAsync(u => !u.IsDeleted && u.Id == updaterGuidId))
        {
            throw new UserDoesNotExistException();
        }

        var employee = await (await _employeeRepository
                .FindByAsync(e => !e.IsDeleted && e.Id == id))
            .FirstOrDefaultAsync();
        if (employee is null)
        {
            throw new EmployeeDoesNotExistException();
        }
        _mapper.Map(request, employee);
        employee.SetUpdatedInfo(updaterGuidId);

        await _employeeRepository.UpdateAsync(employee);
        await _unitOfWork.CommitAsync();
        
        return new ApiActionResult(true);
    }

    public async Task<ApiActionResult> DeleteEmployeeAsync(Guid id)
    {
        if (!await _employeeRepository.ExistsAsync(e => !e.IsDeleted && e.Id == id))
        {
            throw new EmployeeDoesNotExistException();
        }

        await _employeeRepository.DeleteAsync(id);
        await _unitOfWork.CommitAsync();
        return new ApiActionResult(true);
    }
}