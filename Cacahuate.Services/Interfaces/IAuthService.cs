using Cacahuate.Shared.DTOs.Auth;

namespace Cacahuate.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> RefreshAsync(RefreshTokenRequest request);
    Task RevokeAsync(string refreshToken);
}
