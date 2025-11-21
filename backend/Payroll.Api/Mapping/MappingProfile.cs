using AutoMapper;
using Payroll.Application.DTOs;
using Payroll.Domain.Employees;

namespace Payroll.Api.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => string.Concat(s.FirstName, " ", s.LastName)));
    }
}
