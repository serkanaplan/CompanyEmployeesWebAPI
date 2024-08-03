using Contracts;
using Contracts.RepositoryContracts;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;

namespace Repository;

public class EmployeeRepository(RepositoryContext repositoryContext) : RepositoryBase<Employee>(repositoryContext), IEmployeeRepository
{
    public void CreateEmployeeForCompany(Guid companyId, Employee employee)
    {
        employee.CompanyId = companyId;
        Create(employee);
    }


    // public async Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId, bool trackChanges)
    // => await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges).OrderBy(e => e.Name).ToListAsync();

    // public async Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
    // => await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
    // .OrderBy(e => e.Name)
    // .Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize)
    // .Take(employeeParameters.PageSize)
    // .ToListAsync();

    public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
    {
        // var employees = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
        // var employees = await FindByCondition(e => e.CompanyId.Equals(companyId) && (e.Age>= employeeParameters.MinAge && e.Age <= employeeParameters.MaxAge), trackChanges) 
        var employees = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
        .FilterEmployees(employeeParameters.MinAge, employeeParameters.MaxAge)
        .Search(employeeParameters.SearchTerm)// eklenti metod
        .Sort(employeeParameters.OrderBy) // eklenti metod
        .OrderBy(e => e.Name) // eklenti metod
        .Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize)
        .Take(employeeParameters.PageSize)
        .ToListAsync();

        var count = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges).CountAsync();
        // return PagedList<Employee>.ToPagedList(employees, employeeParameters.PageNumber, employeeParameters.PageSize);
        return new PagedList<Employee>(employees, count, employeeParameters.PageNumber, employeeParameters.PageSize);
    }


    public Task<Employee> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges)
    => FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(id), trackChanges).SingleOrDefaultAsync();

    public void DeleteEmployee(Employee employee) => Delete(employee);

}