using AutoMapper;
using Contracts.OtherContracts;
using Contracts.RepositoryContracts;
using Contracts.SerciceContracts;
using Entities.ConfigurationModels;
using Entities.DTO;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Service;

// public sealed class ServiceManager(IRepositoryManager repositoryManager, ILoggerManager logger,IMapper mapper) : IServiceManager
// public sealed class ServiceManager(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper, UserManager<User> userManager, IConfiguration configuration) : IServiceManager
public sealed class ServiceManager(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper, UserManager<User> userManager, IOptions<JwtConfiguration> configuration) : IServiceManager
{
    private readonly Lazy<ICompanyService> _companyService = new(() => new CompanyService(repositoryManager, logger, mapper));
    private readonly Lazy<IEmployeeService> _employeeService = new(() => new EmployeeService(repositoryManager, logger, mapper, dataShaper));
    private readonly Lazy<IAuthenticationService> _authenticationService = new(() => new AuthenticationService(logger, mapper, userManager, configuration));

    public ICompanyService CompanyService => _companyService.Value;
    public IEmployeeService EmployeeService => _employeeService.Value;
     public IAuthenticationService AuthenticationService => _authenticationService.Value; 
}