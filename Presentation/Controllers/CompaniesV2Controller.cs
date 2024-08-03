using Contracts.SerciceContracts;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Presentation.Controllers;

[ApiVersion("2.0", Deprecated = true)] //artık kullanılmayan sürümleri deprecated olarak işaretleyebiliriz. böylece responsun header bölümünde kullanıcı artık bu apinin kullanılmadığını anlar
// [Route("api/{v:apiversion}/companies")] //url api versioning için. ama header versioning kullandığımız için yoruma aldık
[Route("api/companies")]
[ApiController]
[ApiExplorerSettings(GroupName = "v2")]
public class CompaniesV2Controller(IServiceManager service) : ControllerBase
{
    private readonly IServiceManager _service = service;

    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _service.CompanyService.GetAllCompaniesAsync(trackChanges: false);
        // return Ok(companies);
        var companiesV2 = companies.Select(x => $"{x.Name} V2");
        return Ok(companiesV2);
    }
}