using Entities.Models;

namespace Contracts.RepositoryContracts;

public interface ICompanyRepository : IRepositoryBase<Company>
{

    Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges);
    Task<Company> GetCompanyAsync(Guid companyId, bool trackChanges);
    void CreateCompany(Company company);
    Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
    void DeleteCompany(Company company);
}
