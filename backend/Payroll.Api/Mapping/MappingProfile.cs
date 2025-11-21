using AutoMapper;
using Payroll.Application.DTOs.Employees;
using Payroll.Domain.Employees;

namespace Payroll.Api.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Employee, EmployeeDto>();
    }
}
