using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.Persistence.Repositories;
internal class GameSaveRepository(IDatabaseHandler _dbHandler) : BaseRepository<GameSave>(_dbHandler), IGameSaveRepository
{
}
