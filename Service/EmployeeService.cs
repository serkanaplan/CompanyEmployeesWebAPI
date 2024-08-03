using System.Dynamic;
using AutoMapper;
using Contracts.OtherContracts;
using Contracts.RepositoryContracts;
using Contracts.ServiceContracts;
using Entities.DTO;
using Entities.Exceptions;
using Entities.Models;
using Entities.RequestFeatures;

namespace Service;

internal sealed class EmployeeService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto>  dataShaper) : IEmployeeService
{
    private readonly IRepositoryManager _repository = repository;
    private readonly ILoggerManager _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IDataShaper<EmployeeDto> _dataShaper = dataShaper;


    // public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
    // {
    //     await CheckIfCompanyExists(companyId, trackChanges);

    //     var employeesFromDb = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges);

    //     var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
    //     return employeesDto;
    // }

    // public async Task<(IEnumerable<EmployeeDto> employees, MetaData metaData)> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
    public async Task<(IEnumerable<ExpandoObject> employees, MetaData metaData)> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
    {
        if (!employeeParameters.ValidAgeRange) throw new MaxAgeRangeBadRequestException(); 

        await CheckIfCompanyExists(companyId, trackChanges);
        var employeesWithMetaData = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges);
        var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesWithMetaData);

        var shapedData = _dataShaper.ShapeData(employeesDto,employeeParameters.Fields); 
        // return (employees: employeesDto, metaData: employeesWithMetaData.MetaData); //shape data ekledik oyuzden bunu yoruma aldık
    
        return (employees: shapedData, metaData: employeesWithMetaData.MetaData); 
    }


    public async Task<EmployeeDto> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);

        var employeeDb = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges); ;
        var employee = _mapper.Map<EmployeeDto>(employeeDb);

        return employee;
    }


    public async Task<EmployeeDto> CreateEmployeeForCompanyAsync(Guid companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);
        var employeeEntity = _mapper.Map<Employee>(employeeForCreation);

        _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
        await _repository.SaveAsync();

        var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

        return employeeToReturn;
    }



    public async Task DeleteEmployeeForCompany(Guid companyId, Guid id, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);

        var employeeForCompany = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);
        _repository.Employee.DeleteEmployee(employeeForCompany);

        await _repository.SaveAsync();
    }



    public async Task UpdateEmployeeForCompany(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChanges)
    {
        await CheckIfCompanyExists(companyId, compTrackChanges);

        var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

        _mapper.Map(employeeForUpdate, employeeEntity);
        await _repository.SaveAsync();// entity durumu trackchanges= true yapılark zaten takip edildiği için sadece save çağırmamız yeterli

    }



    public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync(Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges)
    {
        await CheckIfCompanyExists(companyId, compTrackChanges);

        var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);
        var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);

        return (employeeToPatch, employeeEntity);
    }

    public async Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
    {
        _mapper.Map(employeeToPatch, employeeEntity);
        await _repository.SaveAsync();
    }


    private async Task CheckIfCompanyExists(Guid companyId, bool trackChanges)
    {
        _ = await _repository.Company.GetCompanyAsync(companyId, trackChanges) ?? throw new CompanyNotFoundException(companyId);
    }

    private async Task<Employee> GetEmployeeForCompanyAndCheckIfItExists(Guid companyId, Guid id, bool trackChanges)
    {
        var employeeDb = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges) ?? throw new EmployeeNotFoundException(id);

        return employeeDb;
    }
}
