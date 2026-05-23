using TeamFlow.Entities;

namespace TeamFlow.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
