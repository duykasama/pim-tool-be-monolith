using System;
using System.Threading.Tasks;
using PIMTool.Core.Interfaces.Services.Base;
using PIMTool.Core.Models;
using PIMTool.Core.Models.Request;

namespace PIMTool.Core.Interfaces.Services;

public interface IEmployeeService : IService
{
    ApiActionResult GetAllEmployees();
    Task<ApiActionResult> GetAllEmployeesAsync();
    // Task<ApiActionResult> FindEmployeesAsync();
    Task<ApiActionResult> FindEmployeesAsync(SearchEmployeesRequest searchEmployeesRequest);
    Task<ApiActionResult> FindEmployeeAsync(Guid id);
    Task<ApiActionResult> CreateEmployeeAsync(CreateEmployeeRequest createEmployeeRequest);
}