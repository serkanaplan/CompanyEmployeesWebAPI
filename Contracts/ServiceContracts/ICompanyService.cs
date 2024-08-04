
using Entities.DTO;
using Entities.Responses;

namespace Contracts.ServiceContracts;

public interface ICompanyService
{
    // Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync(bool trackChanges);
    ApiBaseResponse GetAllCompanies(bool trackChanges);
    // Task<CompanyDto> GetCompanyAsync(Guid companyId, bool trackChanges);
    ApiBaseResponse GetCompany(Guid companyId, bool trackChanges);

    
    Task<CompanyDto> CreateCompanyAsync(CompanyForCreationDto company);
    Task<IEnumerable<CompanyDto>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
    Task<(IEnumerable<CompanyDto> companies, string ids)> CreateCompanyCollectionAsync(IEnumerable<CompanyForCreationDto> companyCollection);
    Task DeleteCompanyAsync(Guid companyId, bool trackChanges);
    Task UpdateCompanyAsync(Guid companyid, CompanyForUpdateDto companyForUpdate, bool trackChanges);
}