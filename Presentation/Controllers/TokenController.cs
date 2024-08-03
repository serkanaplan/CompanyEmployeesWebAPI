using CompanyEmployees.Presentation.Filters;
using Contracts.ServiceContracts;
using Entities.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Presentation.Controllers;

//  tokenın  süresi  dolmak  üzereyse,  api/token  uç  noktasına  istekte  bulunun  ve  yeni  bir  geçerli  token  alın.
[Route("api/[controller]")]
[ApiController]
public class TokenController(IServiceManager service) : ControllerBase
{
    private readonly IServiceManager _service = service;


    [HttpPost("refresh")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> Refresh([FromBody] TokenDto tokenDto)
    {
        var tokenDtoToReturn = await _service.AuthenticationService.RefreshToken(tokenDto);
        return Ok(tokenDtoToReturn);
    }
}