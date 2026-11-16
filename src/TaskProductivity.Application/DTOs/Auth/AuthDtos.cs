namespace TaskProductivity.Application.DTOs.Auth;

public sealed record RegisterRequestDto(
    string FullName,
    string Email,
    string Password);

public sealed record LoginRequestDto(
    string Email,
    string Password);

public sealed record AuthResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    string AccessToken,
    DateTime ExpiresAtUtc);
