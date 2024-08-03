using Entities.DTO;
using Microsoft.AspNetCore.Identity;

namespace Contracts.SerciceContracts;

public interface IAuthenticationService
{
    Task<IdentityResult> RegisterUser(UserForRegistrationDto userForRegistration);
    Task<bool> ValidateUser(UserForAuthenticationDto userForAuth);
    Task<TokenDto> CreateToken(bool populateExp); 
    Task<TokenDto> RefreshToken(TokenDto tokenDto); 
}
