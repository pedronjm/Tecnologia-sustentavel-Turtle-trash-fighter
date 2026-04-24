using SaveApi.Models;

namespace SaveApi.Repositories;

public interface ISaveRepository
{
    Task<IReadOnlyList<SaveSlotRecord>> GetAllAsync(
        long userId,
        CancellationToken cancellationToken = default
    );
    Task<SaveSlotRecord?> GetAsync(
        long userId,
        int slotIndex,
        CancellationToken cancellationToken = default
    );
    Task<SaveSlotRecord> UpsertAsync(
        long userId,
        SaveUpsertRequest request,
        CancellationToken cancellationToken = default
    );
    Task<bool> DeleteAsync(
        long userId,
        int slotIndex,
        CancellationToken cancellationToken = default
    );
}
