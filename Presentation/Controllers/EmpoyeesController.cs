using System.Text.Json;
using CompanyEmployees.Presentation.Filters;
using Contracts.ServiceContracts;
using Entities.DTO;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Presentation.Controllers;

// Alternatif olarak, /api/employees gibi düz bir yapı da kullanılabilirdi, ancak mevcut yapı daha zengin ve ilişkisel bir API tasarımı sunar. Bu, özellikle karmaşık iş mantığı ve veri ilişkileri olan sistemlerde faydalıdır.
[Route("api/companies/{companyId}/[controller]")]
[ApiController]
public class EmployeesController(IServiceManager service) : ControllerBase
{
    private readonly IServiceManager _service = service;


    // http://localhost:5113/api/companies/c9d4c053-49b6-410c-bc78-2d54a9991870/employees
    [HttpGet]
    public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)// bu id rotadan otomatik gelicek çünkü çalışanlara şirketler rotası üzerinden erişiyoruz yani şirket rotasının önceden girilmiş olması gerekiyor ki bu endpointe ulaşabilsin Bu  nedenle  onu  [HttpGet]'e  yerleştirmedik.
    {
        var (employees, metaData) = await _service.EmployeeService.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);
        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metaData));
        return Ok(employees);
    }


    // http://localhost:5113/api/companies/c9d4c053-49b6-410c-bc78-2d54a9991870/employees/86dba8c0-d17841e7-938c-ed49778fb52a 
    [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
    public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
    {
        var employee = await _service.EmployeeService.GetEmployeeAsync(companyId, id, trackChanges: false);
        return Ok(employee);
    }


    // http://localhost:5113/api/companies/ 14759d51-e9c1-4afc-f9bf-08d98898c9c3/employees 
    [HttpPost]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
    {
        // if (employee is null) return BadRequest("EmployeeForCreationDto object is null");//filter kullandığımız için yoruma aldık
        // if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        var employeeToReturn = await _service.EmployeeService.CreateEmployeeForCompanyAsync(companyId, employee, trackChanges: false);

        return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
    }


    // https://localhost:5001/api/companies/14759d51-e9c1-4afc-f9bf-08d98898c9c3/employees/e06cfcc6-e3534bd8-0870-08d988af0956 
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
    {
        await _service.EmployeeService.DeleteEmployeeForCompany(companyId, id, trackChanges: false);
        return NoContent();
    }


    // http://localhost:5113/api/companies/C9D4C053-49B6-410C-BC78-2D54A9991870/employees/80ABBCA8-664D-4B20-B5DE-024705497D4A 
    [HttpPut("{id:guid}")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
    {
        // if (employee is null) return BadRequest("EmployeeForUpdateDto object is null");//filter kullandığımız için yoruma aldık
        // if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        await _service.EmployeeService.UpdateEmployeeForCompany(companyId, id, employee, compTrackChanges: false, empTrackChanges: true);

        return NoContent();
    }



    //  http://localhost:5113/api/companies/C9D4C053-49B6-410C-BC78-2D54A9991870/employees/80ABBCA8-664D4B20-B5DE-024705497D4A
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
    {
        if (patchDoc is null) return BadRequest("patchDoc object sent from client is null.");

        // var result = _service.EmployeeService.GetEmployeeForPatch(companyId, id,compTrackChanges: false,empTrackChanges: true);
        var (employeeToPatch, employeeEntity) = await _service.EmployeeService.GetEmployeeForPatchAsync(companyId, id, compTrackChanges: false, empTrackChanges: true);

        patchDoc.ApplyTo(employeeToPatch, ModelState);

        TryValidateModel(employeeToPatch);

        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
        await _service.EmployeeService.SaveChangesForPatchAsync(employeeToPatch, employeeEntity);
        return NoContent();
    }
}