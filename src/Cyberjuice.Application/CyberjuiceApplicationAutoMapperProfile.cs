using AutoMapper;
using Cyberjuice.Employees;
using Cyberjuice.Employees.Dtos;
using Cyberjuice.Departments;
using Cyberjuice.Departments.Dtos;
using System.Linq;

namespace Cyberjuice;

public class CyberjuiceApplicationAutoMapperProfile : Profile
{
    public CyberjuiceApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        
        // Employee mappings
        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.CompanyIds, opt => opt.Ignore()); // Handled separately in the service
        CreateMap<CreateUpdateEmployeeInput, Employee>()
            .ForMember(dest => dest.Companies, opt => opt.Ignore()); // Handled separately
        
        // Department mappings
        CreateMap<Department, DepartmentDto>();
        CreateMap<CreateDepartmentDto, Department>();
        CreateMap<UpdateDepartmentDto, Department>();
    }
}
