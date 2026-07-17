using InspectionTracker.Domain.Entities;

namespace InspectionTracker.Application.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
