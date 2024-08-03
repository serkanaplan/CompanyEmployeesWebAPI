using Entities.Models;
using Entities.RequestFeatures;

namespace Contracts.RepositoryContracts;

// update metodu eklemedik çünkü trackchanges ile değişiklikleri izlememiz ve save metodunu çağırmamız update işlemini gerçekleştirecektir
public interface IEmployeeRepository : IRepositoryBase<Employee>
{ 
    // Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId,EmployeeParameters employeeParameters, bool trackChanges); 
    Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges); 
    Task<Employee> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges); 
    void CreateEmployeeForCompany(Guid companyId, Employee employee); 
    void DeleteEmployee(Employee employee); 
} 

