using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cacahuate.DataAccess.Entities;
using Cacahuate.DataAccess.Repositories;
using Cacahuate.Services.Interfaces;
using Cacahuate.Shared.DTOs.Auth;
using Cacahuate.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cacahuate.Services.Implementations;

public class AuthService(
    IUserRepository userRepository,
    ITherapistRepository therapistRepository,
    IParentRepository parentRepository,
    IConfiguration configuration) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return BuildLoginResponse(user);
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        if (request.Role == UserRole.Admin)
            throw new InvalidOperationException("Self-registration as Admin is not allowed.");

        var existing = await userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("Email already in use.");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        if (request.Role == UserRole.Therapist)
        {
            await therapistRepository.AddAsync(new Therapist { UserId = user.Id });
            await therapistRepository.SaveChangesAsync();
        }
        else if (request.Role == UserRole.Parent)
        {
            await parentRepository.AddAsync(new Parent { UserId = user.Id });
            await parentRepository.SaveChangesAsync();
        }

        return BuildLoginResponse(user);
    }

    private LoginResponse BuildLoginResponse(User user)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("fullName", $"{user.FirstName} {user.LastName}")
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = user.Role.ToString(),
            ExpiresAt = expires
        };
    }
}
