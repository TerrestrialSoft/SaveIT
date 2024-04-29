using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.Persistence.Repositories;
internal class ImageRepository(IDatabaseHandler _dbHandler) : BaseRepository<ImageEntity>(_dbHandler), IImageRepository
{
}
