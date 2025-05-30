namespace Cyberjuice;

public static class CyberjuiceDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */
    
    public const string DepartmentNameAlreadyExists = "Cyberjuice:DepartmentNameAlreadyExists";
    public const string NotEnoughLeaveDays = "Cyberjuice:NotEnoughLeaveDays";
    public const string EmployeeEmailAlreadyExists = "Cyberjuice:EmployeeEmailAlreadyExists";
    public const string EmployeeMustBelongToAtLeastOneCompany = "Cyberjuice:EmployeeMustBelongToAtLeastOneCompany";
    public const string CompanyNotFound = "Cyberjuice:CompanyNotFound";
}
