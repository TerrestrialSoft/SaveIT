using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SQLite;

namespace SaveIt.App.Persistence.Repositories;
internal class ImageRepository(IDatabaseHandler _dbHandler) : IImageRepository
{
    private readonly SQLiteAsyncConnection _db = _dbHandler.CreateAsyncConnection();

    public async Task CreateImageAsync(ImageEntity image)
    {
        await _db.InsertAsync(image);
    }

    public async Task DeleteImageAsync(Guid id)
    {
        await _db.DeleteAsync<ImageEntity>(id);
    }
}
