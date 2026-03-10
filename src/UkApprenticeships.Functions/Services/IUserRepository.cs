using UkApprenticeships.Functions.Models;

namespace UkApprenticeships.Functions.Services;

public interface IUserRepository
{
    Task<IReadOnlyList<UserDocument>> GetAllUsersAsync();
}
