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
using System.Security.Cryptography;
using Entities.Exceptions;
using Entities.ConfigurationModels;
using Microsoft.Extensions.Options;


namespace Service;

internal sealed class AuthenticationService : IAuthenticationService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
   private readonly IOptions<JwtConfiguration> _configuration; 
    private User? _user;
    private readonly JwtConfiguration _jwtConfiguration;

    // public AuthenticationService(ILoggerManager logger, IMapper mapper, UserManager<User> userManager, IConfiguration configuration)
    public AuthenticationService(ILoggerManager logger, IMapper mapper,  UserManager<User> userManager, IOptions<JwtConfiguration> configuration) 
    {
        _logger = logger;
        _mapper = mapper;
        _userManager = userManager;
        _configuration = configuration;
        _jwtConfiguration = new JwtConfiguration();
        // _configuration.Bind(_jwtConfiguration.Section, _jwtConfiguration);
        _jwtConfiguration = _configuration.Value; 
    }

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

    // public async Task<string> CreateToken()
    public async Task<TokenDto> CreateToken(bool populateExp)
    {
        var signingCredentials = GetSigningCredentials();
        var claims = await GetClaims();
        var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

        // return new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        var refreshToken = GenerateRefreshToken();

        _user.RefreshToken = refreshToken;

        if (populateExp) _user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

        await _userManager.UpdateAsync(_user);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        return new TokenDto(accessToken, refreshToken);
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


    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
    {
        // var jwtSettings = _configuration.GetSection("JwtSettings");
        var tokenOptions = new JwtSecurityToken
        (
        // issuer: jwtSettings["validIssuer"],
        // audience: jwtSettings["validAudience"],
        issuer: _jwtConfiguration.ValidIssuer,
        audience: _jwtConfiguration.ValidAudience,
        claims: claims,
        // expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expires"])),
        expires: DateTime.Now.AddMinutes(Convert.ToDouble(_jwtConfiguration.Expires)),
        signingCredentials: signingCredentials
        );
        return tokenOptions;
    }



    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        // var jwtSettings = _configuration.GetSection("JwtSettings");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"))),
            ValidateLifetime = true,
            // ValidIssuer = jwtSettings["validIssuer"],
            // ValidAudience = jwtSettings["validAudience"]
            ValidIssuer = _jwtConfiguration.ValidIssuer,
            ValidAudience = _jwtConfiguration.ValidAudience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }



    public async Task<TokenDto> RefreshToken(TokenDto tokenDto)
    {
        var principal = GetPrincipalFromExpiredToken(tokenDto.AccessToken);
        var user = await _userManager.FindByNameAsync(principal.Identity.Name);
        if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now) throw new RefreshTokenBadRequest();
        _user = user;
        return await CreateToken(populateExp: false);
    }
}