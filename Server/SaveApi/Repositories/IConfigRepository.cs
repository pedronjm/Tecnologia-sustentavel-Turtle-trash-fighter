using SaveApi.Models;

namespace SaveApi.Repositories;

public interface IConfigRepository
{
    Task<UserConfig?> GetAsync(long userId, CancellationToken cancellationToken = default);
    Task<UserConfig> UpsertAsync(long userId, UserConfigUpsertRequest request, CancellationToken cancellationToken = default);
}
