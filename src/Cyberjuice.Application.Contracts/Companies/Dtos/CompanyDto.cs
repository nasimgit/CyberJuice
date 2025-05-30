using System;
using Volo.Abp.Application.Dtos;

namespace Cyberjuice.Companies.Dtos;

public class CompanyDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; }
}
