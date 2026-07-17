namespace InspectionTracker.WebApi.Contracts
{
    public record RegisterRequest(string Email, string DisplayName, string Password);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string Token, string Email, string DisplayName);
}
