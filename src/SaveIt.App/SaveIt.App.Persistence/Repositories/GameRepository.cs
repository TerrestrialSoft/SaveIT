using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.Persistence.Repositories;
internal class GameRepository(IDatabaseHandler _dbHandler) : BaseRepository<Game>(_dbHandler), IGameRepository
{
}
