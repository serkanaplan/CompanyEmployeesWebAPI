namespace Contracts.ServiceContracts;

public interface IServiceManager
{
    ICompanyService CompanyService { get; }
    IEmployeeService EmployeeService { get; }
    IAuthenticationService AuthenticationService { get; } 
}
