﻿using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IGameRepository
{
    Task CreateGameAsync(Game game);
    Task<Game?> GetGame(Guid id);
    Task<IEnumerable<Game>> GetAllGamesAsync();
}
