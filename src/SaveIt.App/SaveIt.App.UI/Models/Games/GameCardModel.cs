using SaveIt.App.Domain.Entities;

namespace SaveIt.App.UI.Models.Games;
public class GameCardModel(Game game)
{
    public Game Game { get; set; } = game;
}
