using Cyberjuice.Departments.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Cyberjuice.Departments
{
    public interface IDepartmentAppService : IApplicationService
    {
        Task<DepartmentDto> CreateAsync(CreateDepartmentDto input);
        Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentDto input);
        Task<DepartmentDto> GetAsync(Guid id);
        Task<PagedResultDto<DepartmentDto>> GetListAsync(PagedAndSortedResultRequestDto input);
        Task DeleteAsync(Guid id);
    }
} 