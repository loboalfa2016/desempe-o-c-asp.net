
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Interfaces;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "Ya existe una cuenta con ese correo electrónico.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user, "Participant");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = string.Join("; ",
                roleResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        return await CreateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null ||
            !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException(
                "Correo electrónico o contraseña incorrectos.");
        }

        return await CreateAuthResponseAsync(user);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var keyValue = _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(keyValue))
        {
            throw new InvalidOperationException(
                "La clave JWT no está configurada.");
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(keyValue));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddHours(2);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName)
        };

        claims.AddRange(roles.Select(
            role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}
