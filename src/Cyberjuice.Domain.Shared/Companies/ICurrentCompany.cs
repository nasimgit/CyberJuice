using System;
using System.Collections.Generic;

namespace Cyberjuice.Companies;

public interface ICurrentCompany
{
    Guid? Id { get; }
    bool HasAccessToCurrentCompany { get; }
    string Name { get; }
    IDisposable Change(Guid? id);
    IDisposable Change(bool hasAccessTOCompany, Guid? id, string name);
}
