using AutoMapper;
using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Shared.DTO;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Service.Contracts;

namespace Service;

internal sealed class AuthenticationService(ILoggerManager logger, IMapper mapper, UserManager<User> userManager, IConfiguration configuration) : IAuthenticationService
{
    private readonly ILoggerManager _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private User? _user;

    public async Task<IdentityResult> RegisterUser(UserForRegistrationDto userForRegistration)
    {
        var user = _mapper.Map<User>(userForRegistration);
        var result = await _userManager.CreateAsync(user, userForRegistration.Password);

        if (result.Succeeded) await _userManager.AddToRolesAsync(user, userForRegistration.Roles);

        return result;
    }

    public async Task<bool> ValidateUser(UserForAuthenticationDto userForAuth)
    {
        _user = await _userManager.FindByNameAsync(userForAuth.UserName);

        var result = _user != null && await _userManager.CheckPasswordAsync(_user, userForAuth.Password);
        if (!result) _logger.LogWarn($"{nameof(ValidateUser)}: Authentication failed. Wrong user name or password.");

        return result;
    }

    public async Task<string> CreateToken()
    {
        var signingCredentials = GetSigningCredentials();
        var claims = await GetClaims();
        var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }

    private SigningCredentials GetSigningCredentials()
    {
        var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"));
        var secret = new SymmetricSecurityKey(key);

        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }

    private async Task<List<Claim>> GetClaims()
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, _user.UserName) };

        var roles = await _userManager.GetRolesAsync(_user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }


    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials,List<Claim> claims)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var tokenOptions = new JwtSecurityToken
        (
        issuer: jwtSettings["validIssuer"],
        audience: jwtSettings["validAudience"],
        claims: claims,
        expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expires"])),
        signingCredentials: signingCredentials
        );
        return tokenOptions;
    }
}