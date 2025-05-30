using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Cyberjuice.Companies;

public class Company : AuditedAggregateRoot<Guid>
{
    public string Name { get; set; }
}
