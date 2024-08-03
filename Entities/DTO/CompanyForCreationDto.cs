
namespace Entities.DTO;

    public record CompanyForCreationDto
    (
        string Name,
        string Address,
        string Country,
        IEnumerable<EmployeeForCreationDto> Employees
    );
