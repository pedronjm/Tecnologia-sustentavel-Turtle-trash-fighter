using SaveApi.Models;

namespace SaveApi.Repositories;

public interface IUserRepository
{
    Task<UserAccount?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<UserAccount> CreateAsync(string login, string passwordHash, string passwordSalt, string nome, CancellationToken cancellationToken = default);
}
